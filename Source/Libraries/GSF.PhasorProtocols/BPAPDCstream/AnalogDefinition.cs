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

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of an <see cref="IAnalogDefinition"/>.
    /// </summary>
    [Serializable]
    public class AnalogDefinition : AnalogDefinitionBase
    {
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
        /// Gets the maximum length of the <see cref="ChannelDefinitionBase.Label"/> of this <see cref="AnalogDefinition"/>.
        /// </summary>
        public override int MaximumLabelLength => 256;

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => 0;

        /// <summary>
        /// Gets the binary body image of the <see cref="AnalogDefinition"/> object.
        /// </summary>
        /// <remarks>
        /// BPA PDCstream does not include analog definition in descriptor packet.  Only a count of available values is defined in the data frame.
        /// </remarks>
        protected override byte[] BodyImage => null;

    #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new BPA PDCstream analog definition
        internal static IAnalogDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IAnalogDefinition analogDefinition = new AnalogDefinition(parent);

            parsedLength = analogDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return analogDefinition;
        }

        #endregion
    }
}