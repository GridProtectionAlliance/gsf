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
//  04/27/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GSF.Units.EE;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IFrequencyDefinition"/>.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
    public class FrequencyDefinition : FrequencyDefinitionBase
    {
        #region [ Members ]

        // Fields
        private readonly int m_dummy;
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
            DfDtScalingValue = 1000;
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
            int index = 0;

            FrequencyDefinition defaultFrequency = parent is null ? new FrequencyDefinition(null) : parent.Parent.DefaultFrequency;

            // If initial entry is an F - we just ignore this
            if (string.Compare(entry[index].Trim(), "F", StringComparison.OrdinalIgnoreCase) == 0)
                index++;

            ScalingValue = entry.Length > index ? uint.Parse(entry[index++].Trim()) : defaultFrequency.ScalingValue;
            Offset = entry.Length > index ? double.Parse(entry[index++].Trim()) : defaultFrequency.Offset;
            DfDtScalingValue = entry.Length > index ? uint.Parse(entry[index++].Trim()) : defaultFrequency.DfDtScalingValue;
            DfDtOffset = entry.Length > index ? double.Parse(entry[index++].Trim()) : defaultFrequency.DfDtOffset;
            m_dummy = entry.Length > index ? int.Parse(entry[index++].Trim()) : defaultFrequency.m_dummy;
            Label = entry.Length > index ? entry[index].Trim() : defaultFrequency.Label;
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
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the offset of this <see cref="FrequencyDefinition"/>.
        /// </summary>
        public override double Offset
        {
            get => Parent is null ? m_frequencyOffset : base.Offset;
            set
            {
                if (Parent is null)
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
        public override int MaximumLabelLength => 256;

    #endregion

        #region [ Static ]

        // Static Methods

        // Creates frequency information for an INI based BPA PDCstream configuration file
        internal static string ConfigFileFormat(IFrequencyDefinition definition)
        {
            // type, scale, offset, dF/dt scale, dF/dt offset, dummy, label 
            //   F,  1000,    60,      1000,         0,          0,   Frequency
            if (definition is FrequencyDefinition frequency)
                return "F," + frequency.ScalingValue + "," + frequency.Offset + "," + frequency.DfDtScalingValue + "," + frequency.DfDtOffset + "," + frequency.m_dummy + "," + frequency.Label;

            return definition is null ? "" : "F," + definition.ScalingValue + "," + definition.Offset + "," + definition.DfDtScalingValue + "," + definition.DfDtOffset + ",0," + definition.Label;
        }

        #endregion
    }
}