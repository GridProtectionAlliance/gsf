//*******************************************************************************************************
//  DigitalDefinition.cs
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
    /// Represents the IEEE 1344 representation of a <see cref="IDigitalDefinition"/>.
    /// </summary>
    [Serializable()]
    public class DigitalDefinition : DigitalDefinitionBase
    {
        #region [ Members ]

        // Fields
        private short m_statusFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/>.
        /// </summary>
        protected DigitalDefinition()
        {
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
            m_statusFlags = info.GetInt16("statusFlags");
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/> using the specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        /// <param name="index">The index of this <see cref="DigitalDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="DigitalDefinition"/>.</param>
        public DigitalDefinition(ConfigurationCell parent, string label)
            : base(parent, label)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets normal status for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public short NormalStatus
        {
            get
            {
                return (short)(m_statusFlags & Bit.Bit4);
            }
            set
            {
                if (value > 0)
                    m_statusFlags = (short)(m_statusFlags | Bit.Bit4);
                else
                    m_statusFlags = (short)(m_statusFlags & ~Bit.Bit4);
            }
        }

        /// <summary>
        /// Gets or sets valid input for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public short ValidInput
        {
            get
            {
                return (short)(m_statusFlags & Bit.Bit0);
            }
            set
            {
                if (value > 0)
                    m_statusFlags = (short)(m_statusFlags | Bit.Bit0);
                else
                    m_statusFlags = (short)(m_statusFlags & ~Bit.Bit0);
            }
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="DigitalDefinition"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                return EndianOrder.BigEndian.GetBytes(m_statusFlags);
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="DigitalDefinition"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Normal Status", NormalStatus.ToString());
                baseAttributes.Add("Valid Input", ValidInput.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        internal void ParseConversionFactor(byte[] binaryImage, int startIndex)
        {
            m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize digital definition
            info.AddValue("statusFlags", m_statusFlags);
        }

        #endregion

        #region [ Static ]

        // Static Properties

        // Gets length of conversion factor
        internal static int ConversionFactorLength
        {
            get
            {
                return 2;
            }
        }

        // Static Methods

        // Delegate handler to create a new IEEE 1344 phasor value
        internal static IDigitalDefinition CreateNewDigitalDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex)
        {
            IDigitalDefinition digital = new DigitalDefinition() { Parent = parent };

            digital.Initialize(binaryImage, startIndex, 0);

            return digital;
        }

        #endregion
    }
}