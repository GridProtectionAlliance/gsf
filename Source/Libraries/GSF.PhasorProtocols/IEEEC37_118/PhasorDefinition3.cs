//******************************************************************************************************
//  PhasorDefinition3.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/27/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 configuration frame 3 implementation of a <see cref="IPhasorDefinition"/>.
    /// </summary>
    [Serializable]
    public class PhasorDefinition3 : ChannelDefinitionBase3, IPhasorDefinition
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 4;

        // Fields
        private PhasorType m_type;
        private IPhasorDefinition m_voltageReference;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinition3"/>.</param>
        public PhasorDefinition3(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell3"/> parent of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="label">The label of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="offset">The offset of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="type">The <see cref="PhasorType"/> of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="voltageReference">The associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).</param>
        public PhasorDefinition3(ConfigurationCell3 parent, string label, uint scale, double offset, PhasorType type, PhasorDefinition3 voltageReference)
            : base(parent, label, scale, offset)
        {
            m_type = type;
            m_voltageReference = type == PhasorType.Voltage ? this : voltageReference;
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorDefinition3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize phasor definition
            m_type = (PhasorType)info.GetValue("type", typeof(PhasorType));
            m_voltageReference = (IPhasorDefinition)info.GetValue("voltageReference", typeof(IPhasorDefinition));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell3"/> parent of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public new virtual ConfigurationCell3 Parent
        {
            get => base.Parent as ConfigurationCell3;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets the <see cref="GSF.PhasorProtocols.DataFormat"/> of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        public override DataFormat DataFormat => Parent.PhasorDataFormat;

        /// <summary>
        /// Gets or sets the <see cref="GSF.PhasorProtocols.CoordinateFormat"/> of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        public virtual CoordinateFormat CoordinateFormat => Parent.PhasorCoordinateFormat;

        /// <summary>
        /// Gets or sets the <see cref="GSF.PhasorProtocols.AngleFormat"/> of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        public virtual AngleFormat AngleFormat => Parent.PhasorAngleFormat;

        /// <summary>
        /// Gets or sets the <see cref="PhasorType"/> of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        public virtual PhasorType PhasorType
        {
            get => m_type;
            set => m_type = value;
        }

        /// <summary>
        /// Gets or sets the associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).
        /// </summary>
        /// <remarks>
        /// This only applies to current phasors.
        /// </remarks>
        public virtual IPhasorDefinition VoltageReference
        {
            get => m_voltageReference;
            set
            {
                if (m_type == PhasorType.Voltage)
                    m_voltageReference = this;
                else
                    m_voltageReference = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="PhasorDefinitionBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Phasor Type", $"{(int)PhasorType}: {PhasorType}");

                return baseAttributes;
            }
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];
                UInt24 scalingFactor = ScalingValue > UInt24.MaxValue ? UInt24.MaxValue : (UInt24)ScalingValue;

                buffer[0] = (byte)PhasorType;

                BigEndian.CopyBytes(scalingFactor, buffer, 1);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the string representation of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        /// <returns>String representation of this <see cref="PhasorDefinitionBase"/>.</returns>
        public override string ToString()
        {
            return (PhasorType == PhasorType.Current ? "I: " : "V: ") + Label;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize phasor definition
            info.AddValue("type", m_type, typeof(PhasorType));
            info.AddValue("voltageReference", m_voltageReference, typeof(IPhasorDefinition));
        }

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        internal int ParseConversionFactor(byte[] buffer, int startIndex)
        {
            // Get phasor type from first byte
            PhasorType = buffer[startIndex] == 0 ? PhasorType.Voltage : PhasorType.Current;

            // Last three bytes represent scaling factor
            ScalingValue = BigEndian.ToUInt24(buffer, startIndex + 1);

            return ConversionFactorLength;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 phasor definition
        internal static IPhasorDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IPhasorDefinition phasorDefinition = new PhasorDefinition3(parent);

            parsedLength = phasorDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return phasorDefinition;
        }

        #endregion
    }
}