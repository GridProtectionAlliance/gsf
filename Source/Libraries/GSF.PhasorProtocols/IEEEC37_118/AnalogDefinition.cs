//******************************************************************************************************
//  AnalogDefinition.cs - Gbtc
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
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of an <see cref="IAnalogDefinition"/>.
    /// </summary>
    [Serializable]
    public class AnalogDefinition : AnalogDefinitionBase
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 4;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="AnalogDefinition"/>.</param>
        public AnalogDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="offset">The offset of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="type">The <see cref="AnalogType"/> of this <see cref="AnalogDefinition"/>.</param>
        public AnalogDefinition(ConfigurationCell parent, string label, uint scale, double offset, AnalogType type)
            : base(parent, label, scale, offset, type)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="AnalogDefinition"/>.
        /// </summary>
        public new virtual ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="AnalogDefinition"/>.
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
            IAnalogDefinition analogDefinition = new AnalogDefinition(parent);

            parsedLength = analogDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return analogDefinition;
        }

        #endregion
    }
}
