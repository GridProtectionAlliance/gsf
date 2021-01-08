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
//  05/05/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.Anonymous
{
    /// <summary>
    /// Represents a protocol independent implementation of an <see cref="IDigitalDefinition"/>.
    /// </summary>
    [Serializable]
    public class DigitalDefinition : DigitalDefinitionBase
    {
        #region [ Members ]

        // Fields
        private uint m_maskValue;
        private string m_label;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="DigitalDefinition"/>.</param>
        /// <param name="maskValue">The value of the digital mask made available in configuration frames.</param>
        public DigitalDefinition(ConfigurationCell parent, string label, uint maskValue)
            : base(parent, label)
        {
            m_maskValue = maskValue;
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
            m_label = info.GetString("digitalLabels");
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
        /// Gets or sets the digital mask value of this <see cref="DigitalDefinition"/> made available in configuration frames.
        /// </summary>
        public uint MaskValue
        {
            get => m_maskValue;
            set => m_maskValue = value;
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ChannelDefinitionBase.Label"/> of this <see cref="DigitalDefinition"/>.
        /// </summary>
        /// <remarks>
        /// This length is not restricted for anonymous protocol definitions.
        /// </remarks>
        public override int MaximumLabelLength => int.MaxValue;

        /// <summary>
        /// Gets or sets the label of this <see cref="DigitalDefinition"/>.
        /// </summary>
        public override string Label
        {
            get => m_label;
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "undefined";

                if (value.Trim().Length > MaximumLabelLength)
                {
                    throw new OverflowException("Label length cannot exceed " + MaximumLabelLength);
                }

                // We override this function since base class automatically "fixes-up" labels
                // by removing duplicate white space characters - this can throw off the fixed
                // label lengths in IEEE C37.118
                m_label = value.Trim();

                // We pass value along to base class for posterity...
                base.Label = value;
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

            // Serialize digital definition
            info.AddValue("digitalLabels", m_label);
        }

        #endregion
    }
}