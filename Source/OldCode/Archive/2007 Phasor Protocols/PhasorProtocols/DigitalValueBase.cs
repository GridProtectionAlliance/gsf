//*******************************************************************************************************
//  DigitalValueBase.cs
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
//  02/18/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent representation of a digital value.
    /// </summary>
    [Serializable()]
    public abstract class DigitalValueBase : ChannelValueBase<IDigitalDefinition>, IDigitalValue
    {
        #region [ Members ]

        // Fields
        private short m_value;
        private bool m_valueAssigned;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalValueBase"/>.
        /// </summary>
        protected DigitalValueBase()
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalValueBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DigitalValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize digital value
            m_value = info.GetInt16("value");
            m_valueAssigned = true;
        }

        /// <summary>
        /// Creates a new <see cref="DigitalValueBase"/> from the specified parameters.
        /// </summary>
        protected DigitalValueBase(IDataCell parent, IDigitalDefinition digitalDefinition, short value)
            : base(parent, digitalDefinition)
        {
            m_value = value;
            m_valueAssigned = (value != -1);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the 16-bit integer value (composed of digital bits) that represents this <see cref="DigitalValueBase"/>.
        /// </summary>
        public virtual short Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                m_valueAssigned = true;
            }
        }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="DigitalValueBase"/> have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        public override bool IsEmpty
        {
            get
            {
                return !m_valueAssigned;
            }
        }

        /// <summary>
        /// Gets the composite values of this <see cref="DigitalValueBase"/>.
        /// </summary>
        public override double[] CompositeValues
        {
            get
            {
                return new double[] { m_value };
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
        /// Gets the binary body image of the <see cref="DigitalValueBase"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                EndianOrder.BigEndian.CopyBytes(m_value, buffer, 0);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DigitalValueBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;
                byte[] valueBytes = BitConverter.GetBytes(Value);

                baseAttributes.Add("Digital Value", Value.ToString());
                baseAttributes.Add("Digital Value (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(valueBytes));
                baseAttributes.Add("Digital Value (Hexadecimal)", "0x" + ByteEncoding.Hexadecimal.GetString(valueBytes));

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
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            // TODO: It is expected that parent IDataCell will validate that it has
            // enough length to parse entire cell well in advance so that low level
            // parsing routines do not have to re-validate that enough length is
            // available to parse needed information as an optimization...

            m_value = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
            m_valueAssigned = true;
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

            // Serialize digital value
            info.AddValue("value", m_value);
        }

        #endregion
    }
}