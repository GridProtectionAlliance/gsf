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
using System.Security.Permissions;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IDigitalDefinition"/>.
    /// </summary>
    [Serializable()]
    public class DigitalDefinition : DigitalDefinitionBase
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 2;

        // Fields
        private ushort m_statusFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        public DigitalDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="DigitalDefinition"/>.</param>
        public DigitalDefinition(ConfigurationCell parent, string label)
            : base(parent, label)
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
            m_statusFlags = info.GetUInt16("statusFlags");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.
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
        /// Gets or sets normal status for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public ushort NormalStatus
        {
            get
            {
                return (ushort)(m_statusFlags & (ushort)Bits.Bit04);
            }
            set
            {
                if (value > 0)
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit04);
                else
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit04);
            }
        }

        /// <summary>
        /// Gets or sets valid input for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public ushort ValidInput
        {
            get
            {
                return (ushort)(m_statusFlags & (ushort)Bits.Bit00);
            }
            set
            {
                if (value > 0)
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit00);
                else
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit00);
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

                byte[] normalStatusBytes = BitConverter.GetBytes(NormalStatus);
                byte[] validInputBytes = BitConverter.GetBytes(ValidInput);

                baseAttributes.Add("Normal Status", NormalStatus.ToString());
                baseAttributes.Add("Normal Status (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(normalStatusBytes));
                baseAttributes.Add("Normal Status (Hexadecimal)", "0x" + ByteEncoding.Hexadecimal.GetString(normalStatusBytes));

                baseAttributes.Add("Valid Input", ValidInput.ToString());
                baseAttributes.Add("Valid Input (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(validInputBytes));
                baseAttributes.Add("Valid Input (Hexadecimal)", "0x" + ByteEncoding.Hexadecimal.GetString(validInputBytes));

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
        internal int ParseConversionFactor(byte[] binaryImage, int startIndex)
        {
            m_statusFlags = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex);
            return ConversionFactorLength;
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

            // Serialize digital definition
            info.AddValue("statusFlags", m_statusFlags);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE 1344 digital definition
        internal static IDigitalDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IDigitalDefinition digitalDefinition = new DigitalDefinition(parent);

            parsedLength = digitalDefinition.Initialize(binaryImage, startIndex, 0);

            return digitalDefinition;
        }

        #endregion
    }
}