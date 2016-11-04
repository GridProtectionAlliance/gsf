//******************************************************************************************************
//  FrequencyDefinition.cs - Gbtc
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
//  05/20/2011 - Ritchie
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IFrequencyDefinition"/>.
    /// </summary>
    [Serializable]
    public class FrequencyDefinition : FrequencyDefinitionBase
    {
        #region [ Members ]

        // Fields
        private readonly uint m_dummy;
        private double m_frequencyOffset;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        public FrequencyDefinition(IConfigurationCell parent)
            : base(parent)
        {
            ScalingValue = 1000;
            DfDtScalingValue = 100;
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from the specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="entryValue">The entry value from the INI based configuration file.</param>
        public FrequencyDefinition(ConfigurationCell parent, string entryValue)
            : base(parent)
        {
            string[] entry = entryValue.Split(',');
            FrequencyDefinition defaultFrequency;
            int index = 0;
            uint iValue;
            double dValue;

            if (parent != null)
                defaultFrequency = parent.Parent.DefaultFrequency;
            else
                defaultFrequency = new FrequencyDefinition(null as ConfigurationCell);

            // If initial entry is an F - we just ignore this
            if (string.Compare(entry[index].Trim(), "F", true) == 0)
                index++;

            if (entry.Length > index && uint.TryParse(entry[index++].Trim(), out iValue))
                ScalingValue = iValue;
            else
                ScalingValue = defaultFrequency.ScalingValue;

            if (entry.Length > index && double.TryParse(entry[index++].Trim(), out dValue))
                Offset = dValue;
            else
                Offset = defaultFrequency.Offset;

            if (entry.Length > index && uint.TryParse(entry[index++].Trim(), out iValue))
                DfDtScalingValue = iValue;
            else
                DfDtScalingValue = defaultFrequency.DfDtScalingValue;

            if (entry.Length > index && double.TryParse(entry[index++].Trim(), out dValue))
                DfDtOffset = dValue;
            else
                DfDtOffset = defaultFrequency.DfDtOffset;

            if (entry.Length > index && uint.TryParse(entry[index++].Trim(), out iValue))
                m_dummy = iValue;
            else
                m_dummy = defaultFrequency.m_dummy;

            if (entry.Length > index)
                Label = entry[index++].Trim();
            else
                Label = defaultFrequency.Label;
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.
        /// </summary>
        public new virtual ConfigurationCell Parent
        {
            get
            {
                return base.Parent as ConfigurationCell;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the offset of this <see cref="FrequencyDefinition"/>.
        /// </summary>
        public override double Offset
        {
            get
            {
                if (Parent == null)
                    return m_frequencyOffset;
                else
                    return base.Offset;
            }
            set
            {
                if (Parent == null)
                {
                    // Store local value for default frequency definition
                    m_frequencyOffset = value;
                }
                else
                {
                    // Frequency offset is stored as nominal frequency of parent cell
                    if (value >= 60.0F)
                        Parent.NominalFrequency = LineFrequency.Hz60;
                    else
                        Parent.NominalFrequency = LineFrequency.Hz50;
                }
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ChannelDefinitionBase.Label"/> of this <see cref="FrequencyDefinition"/>.
        /// </summary>
        public override int MaximumLabelLength
        {
            get
            {
                return 256;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="FrequencyDefinition"/> object.
        /// </summary>
        /// <remarks>
        /// BPA PDCstream does not include frequency definition in descriptor packet.
        /// </remarks>
        protected override byte[] BodyImage
        {
            get
            {
                return null;
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
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Creates frequency information for an INI based BPA PDCstream configuration file
        internal static string ConfigFileFormat(IFrequencyDefinition definition)
        {
            FrequencyDefinition frequency = definition as FrequencyDefinition;

            // type, scale, offset, dF/dt scale, dF/dt offset, dummy, label 
            //   F,  1000,    60,      1000,         0,          0,   Frequency

            if (frequency != null)
                return "F," + frequency.ScalingValue + "," + frequency.Offset + "," + frequency.DfDtScalingValue + "," + frequency.DfDtOffset + "," + frequency.m_dummy + "," + frequency.Label;
            else if (definition != null)
                return "F," + definition.ScalingValue + "," + definition.Offset + "," + definition.DfDtScalingValue + "," + definition.DfDtOffset + ",0," + definition.Label;

            return "";
        }

        // Delegate handler to create a new BPA PDCstream frequency definition
        internal static IFrequencyDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IFrequencyDefinition frequencyDefinition = new FrequencyDefinition(parent);

            parsedLength = frequencyDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return frequencyDefinition;
        }

        #endregion
    }
}