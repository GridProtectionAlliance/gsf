//******************************************************************************************************
//  DigitalDefinition.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.IEEE1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IDigitalDefinition"/>.
    /// </summary>
    [Serializable]
    public class DigitalDefinition : DigitalDefinitionBase
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 2;

        // Fields
        private ushort m_statusFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        public DigitalDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="DigitalDefinition"/>.</param>
        public DigitalDefinition(ConfigurationCell parent, string label)
            : base(parent, label)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DigitalDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize digital definition
            m_statusFlags = info.GetUInt16("statusFlags");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.
        /// </summary>
        public new virtual ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets normal status for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public ushort NormalStatus
        {
            get => (ushort)(m_statusFlags & (ushort)Bits.Bit04);
            set
            {
                if (value > 0)
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit04);
                else
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit04);
            }
        }

        /// <summary>
        /// Gets or sets valid input for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public ushort ValidInputs
        {
            get => (ushort)(m_statusFlags & (ushort)Bits.Bit00);
            set
            {
                if (value > 0)
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit00);
                else
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit00);
            }
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="DigitalDefinition"/>.
        /// </summary>
        internal byte[] ConversionFactorImage => BigEndian.GetBytes(m_statusFlags);

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="DigitalDefinition"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                byte[] normalStatusBytes = BitConverter.GetBytes(NormalStatus);
                byte[] validInputsBytes = BitConverter.GetBytes(ValidInputs);

                baseAttributes.Add("Normal Status", NormalStatus.ToString());
                baseAttributes.Add("Normal Status (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(normalStatusBytes));
                baseAttributes.Add("Normal Status (Hexadecimal)", $"0x{ByteEncoding.Hexadecimal.GetString(normalStatusBytes)}");

                baseAttributes.Add("Valid Inputs", ValidInputs.ToString());
                baseAttributes.Add("Valid Inputs (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(validInputsBytes));
                baseAttributes.Add("Valid Inputs (Hexadecimal)", $"0x{ByteEncoding.Hexadecimal.GetString(validInputsBytes)}");

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        internal int ParseConversionFactor(byte[] buffer, int startIndex)
        {
            m_statusFlags = BigEndian.ToUInt16(buffer, startIndex);
            return ConversionFactorLength;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize digital definition
            info.AddValue("statusFlags", m_statusFlags);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE 1344 digital definition
        internal static IDigitalDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IDigitalDefinition digitalDefinition = new DigitalDefinition(parent);

            parsedLength = digitalDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return digitalDefinition;
        }

        #endregion
    }
}