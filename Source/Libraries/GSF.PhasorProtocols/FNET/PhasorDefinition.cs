//******************************************************************************************************
//  PhasorDefinition.cs - Gbtc
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
using GSF.Units.EE;

namespace GSF.PhasorProtocols.FNET
{
    /// <summary>
    /// Represents the F-NET implementation of a <see cref="IPhasorDefinition"/>.
    /// </summary>
    [Serializable]
    public class PhasorDefinition : PhasorDefinitionBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        public PhasorDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="type">The <see cref="PhasorType"/> of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="voltageReference">The associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).</param>
        public PhasorDefinition(ConfigurationCell parent, string label, PhasorType type, PhasorDefinition voltageReference)
            : base(parent, label, 1, 0.0D, type, voltageReference)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public new virtual ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        #endregion
    }
}