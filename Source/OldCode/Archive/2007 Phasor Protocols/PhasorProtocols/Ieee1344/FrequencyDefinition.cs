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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IFrequencyDefinition"/>.
    /// </summary>
    [Serializable()]
    public class FrequencyDefinition : FrequencyDefinitionBase
    {
        #region [ Members ]

        // Fields
        private short m_statusFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/>.
        /// </summary>
        protected FrequencyDefinition()
        {
            ScalingValue = 1000;
            DfDtScalingValue = 100;
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ScalingValue = 1000;
            DfDtScalingValue = 100;

            // Deserialize frequency definition
            m_statusFlags = info.GetInt16("statusFlags");
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> using the specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="offset">The offset of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="dfdtScale">The df/dt scaling value of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="dfdtOffset">The df/dt offset of this <see cref="FrequencyDefinition"/>.</param>
        public FrequencyDefinition(ConfigurationCell parent, string label, uint scale, double offset, uint dfdtScale, double dfdtOffset)
            : base(parent, label, scale, offset, dfdtScale, dfdtOffset)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if frequency measurement is available in <see cref="FrequencyValue"/>.
        /// </summary>
        public bool FrequencyIsAvailable
        {
            get
            {
                return (m_statusFlags & Bit.Bit8) == 0;
            }
            set
            {
                if (value)
                    m_statusFlags = (short)(m_statusFlags & ~Bit.Bit8);
                else
                    m_statusFlags = (short)(m_statusFlags | Bit.Bit8);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if df/dt measurement is available in <see cref="FrequencyValue"/>.
        /// </summary>
        public bool DfDtIsAvailable
        {
            get
            {
                return (m_statusFlags & Bit.Bit9) == 0;
            }
            set
            {
                if (value)
                    m_statusFlags = (short)(m_statusFlags & ~Bit.Bit9);
                else
                    m_statusFlags = (short)(m_statusFlags | Bit.Bit9);
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="FrequencyDefinition"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                if (NominalFrequency == LineFrequency.Hz50)
                    m_statusFlags = (short)(m_statusFlags | Bit.Bit0);
                else
                    m_statusFlags = (short)(m_statusFlags & ~Bit.Bit0);

                return EndianOrder.BigEndian.GetBytes(m_statusFlags);
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="FrequencyDefinition"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frequency Is Available", FrequencyIsAvailable.ToString());
                baseAttributes.Add("df/dt Is Available", DfDtIsAvailable.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The base implementation assumes that all channel defintions begin with a label as this is
        /// the general case, override functionality if this is not the case.
        /// </remarks>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);

            Parent.NominalFrequency = ((m_statusFlags & Bit.Bit0) > 0) ? LineFrequency.Hz50 : LineFrequency.Hz60;

            return 2;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize frequency definition
            info.AddValue("statusFlags", m_statusFlags);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE 1344 frequency definition
        internal static IFrequencyDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IFrequencyDefinition frequencyDefinition = new FrequencyDefinition() { Parent = parent };

            parsedLength = frequencyDefinition.Initialize(binaryImage, startIndex, 0);

            return frequencyDefinition;
        }

        #endregion
    }
}