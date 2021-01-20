//******************************************************************************************************
//  AnalogDefinitionBase.cs - Gbtc
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
//   02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent definition of an <see cref="IAnalogValue"/>.
    /// </summary>
    [Serializable]
    public abstract class AnalogDefinitionBase : ChannelDefinitionBase, IAnalogDefinition
    {
        #region [ Members ]

        // Fields
        private AnalogType m_type;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="AnalogDefinitionBase"/>.</param>
        protected AnalogDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="label">The label of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="offset">The offset of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="type">The <see cref="AnalogType"/> of this <see cref="AnalogDefinitionBase"/>.</param>
        protected AnalogDefinitionBase(IConfigurationCell parent, string label, uint scale, double offset, AnalogType type)
            : base(parent, label, scale, offset)
        {
            m_type = type;
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize analog definition
            m_type = (AnalogType)info.GetValue("type", typeof(AnalogType));
        }

        #endregion

        #region [ Properties ]

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

        #endregion
   }
}