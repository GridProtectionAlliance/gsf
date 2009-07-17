//*******************************************************************************************************
//  FrequencyDefinition.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IFrequencyDefinition"/>.
    /// </summary>
    [Serializable()]
    public class FrequencyDefinition : FrequencyDefinitionBase
    {
        #region [ Members ]

        // Fields
        private ushort m_statusFlags;

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
        /// Creates a new <see cref="FrequencyDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="FrequencyDefinition"/>.</param>
        public FrequencyDefinition(ConfigurationCell parent, string label)
            : base(parent, label, 1000, 100, 0.0D)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize frequency definition
            m_statusFlags = info.GetUInt16("statusFlags");
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
        /// Gets or sets flag that determines if frequency measurement is available in <see cref="FrequencyValue"/>.
        /// </summary>
        public bool FrequencyIsAvailable
        {
            get
            {
                return (m_statusFlags & (ushort)Bits.Bit08) == 0;
            }
            set
            {
                if (value)
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit08);
                else
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit08);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if df/dt measurement is available in <see cref="FrequencyValue"/>.
        /// </summary>
        public bool DfDtIsAvailable
        {
            get
            {
                return (m_statusFlags & (ushort)Bits.Bit09) == 0;
            }
            set
            {
                if (value)
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit09);
                else
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit09);
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
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit00);
                else
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit00);

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
            m_statusFlags = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex);

            Parent.NominalFrequency = ((m_statusFlags & (ushort)Bits.Bit00) > 0) ? LineFrequency.Hz50 : LineFrequency.Hz60;

            return 2;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
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
            IFrequencyDefinition frequencyDefinition = new FrequencyDefinition(parent);

            parsedLength = frequencyDefinition.Initialize(binaryImage, startIndex, 0);

            return frequencyDefinition;
        }

        #endregion
    }
}