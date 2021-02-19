//******************************************************************************************************
//  AnalogDefinition3.cs - Gbtc
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

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 configuration frame 3 implementation of an <see cref="IAnalogDefinition"/>.
    /// </summary>
    [Serializable]
    public class AnalogDefinition3 : ChannelDefinitionBase3, IAnalogDefinition
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 4;

        // Fields
        private AnalogType m_type;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="AnalogDefinition3"/>.</param>
        public AnalogDefinition3(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell3"/> parent of this <see cref="AnalogDefinition3"/>.</param>
        /// <param name="label">The label of this <see cref="AnalogDefinition3"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="AnalogDefinition3"/>.</param>
        /// <param name="offset">The offset of this <see cref="AnalogDefinition3"/>.</param>
        /// <param name="type">The <see cref="AnalogType"/> of this <see cref="AnalogDefinition3"/>.</param>
        public AnalogDefinition3(ConfigurationCell3 parent, string label, uint scale, double offset, AnalogType type)
            : base(parent, label, scale, offset)
        {
            m_type = type;
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogDefinition3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize analog definition
            m_type = (AnalogType)info.GetValue("type", typeof(AnalogType));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell3"/> parent of this <see cref="AnalogDefinition3"/>.
        /// </summary>
        public new virtual ConfigurationCell3 Parent
        {
            get => base.Parent as ConfigurationCell3;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets the <see cref="GSF.PhasorProtocols.DataFormat"/> for the <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        public override DataFormat DataFormat => Parent.AnalogDataFormat;

        /// <summary>
        /// Gets or sets <see cref="AnalogType"/> of this <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        public virtual AnalogType AnalogType
        {
            get => m_type;
            set => m_type = value;
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="AnalogDefinitionBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Analog Type", $"{(int)AnalogType}: {AnalogType}");

                return baseAttributes;
            }
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="AnalogDefinition3"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];
                UInt24 scalingFactor = ScalingValue > UInt24.MaxValue ? UInt24.MaxValue : (UInt24)ScalingValue;

                // Store analog type in first byte
                buffer[0] = (byte)AnalogType;

                // Store scaling in last three bytes
                BigEndian.CopyBytes(scalingFactor, buffer, 1);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize analog definition
            info.AddValue("type", m_type, typeof(AnalogType));
        }

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        internal int ParseConversionFactor(byte[] buffer, int startIndex)
        {
            // Get analog type from first byte
            AnalogType = (AnalogType)buffer[startIndex];

            // Last three bytes represent scaling factor
            ScalingValue = BigEndian.ToUInt24(buffer, startIndex + 1);

            return ConversionFactorLength;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 analog definition
        internal static IAnalogDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IAnalogDefinition analogDefinition = new AnalogDefinition3(parent);

            parsedLength = analogDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return analogDefinition;
        }

        #endregion
    }
}
