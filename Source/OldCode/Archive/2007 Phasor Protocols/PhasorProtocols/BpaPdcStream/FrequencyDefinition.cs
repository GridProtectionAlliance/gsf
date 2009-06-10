//*******************************************************************************************************
//  FrequencyDefinition.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IFrequencyDefinition"/>.
    /// </summary>
    [Serializable()]
    public class FrequencyDefinition : FrequencyDefinitionBase
    {
        #region [ Members ]

        // Fields
        private int m_dummy;
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

            if (parent != null)
                defaultFrequency = parent.Parent.DefaultFrequency;
            else
                defaultFrequency = new FrequencyDefinition(null as ConfigurationCell);

            // First entry is an F - we just ignore this
            if (entry.Length > 1)
                ScalingValue = uint.Parse(entry[1].Trim());
            else
                ScalingValue = defaultFrequency.ScalingValue;

            if (entry.Length > 2)
                Offset = double.Parse(entry[2].Trim());
            else
                Offset = defaultFrequency.Offset;

            if (entry.Length > 3)
                DfDtScalingValue = uint.Parse(entry[3].Trim());
            else
                DfDtScalingValue = defaultFrequency.DfDtScalingValue;

            if (entry.Length > 4)
                DfDtOffset = double.Parse(entry[4].Trim());
            else
                DfDtOffset = defaultFrequency.DfDtOffset;

            if (entry.Length > 5)
                m_dummy = int.Parse(entry[5].Trim());
            else
                m_dummy = defaultFrequency.m_dummy;

            if (entry.Length > 6)
                Label = entry[6].Trim();
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
        public virtual new ConfigurationCell Parent
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
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
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

            if (frequency != null)
                return "F," + frequency.ScalingValue + "," + frequency.Offset + "," + frequency.DfDtScalingValue + "," + frequency.DfDtOffset + "," + frequency.m_dummy + "," + frequency.Label;
            
            return "";
        }

        // Delegate handler to create a new BPA PDCstream frequency definition
        internal static IFrequencyDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IFrequencyDefinition frequencyDefinition = new FrequencyDefinition(parent);

            parsedLength = frequencyDefinition.Initialize(binaryImage, startIndex, 0);

            return frequencyDefinition;
        }

        #endregion
    }
}