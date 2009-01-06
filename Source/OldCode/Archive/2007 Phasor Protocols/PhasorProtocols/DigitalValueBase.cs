using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;

//*******************************************************************************************************
//  DigitalValueBase.vb - Digital value base class
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent representation of a digital value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class DigitalValueBase : ChannelValueBase<IDigitalDefinition>, IDigitalValue
    {



        private short m_value;
        private bool m_valueAssigned;

        protected DigitalValueBase()
        {
        }

        protected DigitalValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize digital value
            m_value = info.GetInt16("value");
            m_valueAssigned = true;

        }

        protected DigitalValueBase(IDataCell parent)
            : base(parent)
        {


        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal value As short)
        protected DigitalValueBase(IDataCell parent, IDigitalDefinition digitalDefinition, short value)
            : base(parent, digitalDefinition)
        {


            m_value = value;
            m_valueAssigned = value != -1;

        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As int)
        protected DigitalValueBase(IDataCell parent, IDigitalDefinition digitalDefinition, byte[] binaryImage, int startIndex)
            : base(parent, digitalDefinition)
        {

            ParseBinaryImage(null, binaryImage, startIndex);

        }

        // Derived classes are expected to expose a Public Sub New(ByVal digitalValue As IDigitalValue)
        protected DigitalValueBase(IDataCell parent, IDigitalDefinition digitalDefinition, IDigitalValue digitalValue)
            : this(parent, digitalDefinition, digitalValue.Value)
        {


        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override DataFormat DataFormat
        {
            get
            {
                return base.DataFormat;
            }
        }

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

        public override float this[int index]
        {
            get
            {
                return m_value;
            }
            set
            {
                try
                {
                    m_value = (short)value;
                    m_valueAssigned = true;
                }
                catch (OverflowException)
                {
                    m_value = short.MinValue;
                }
            }
        }

        public override int CompositeValueCount
        {
            get
            {
                return 1;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return !m_valueAssigned;
            }
        }

        protected override int BodyLength
        {
            get
            {
                return 2;
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                EndianOrder.BigEndian.CopyBytes(m_value, buffer, 0);

                return buffer;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            m_value = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
            m_valueAssigned = true;

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize digital value
            info.AddValue("value", m_value);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;
                byte[] valueBytes = BitConverter.GetBytes(Value);

                baseAttributes.Add("Digital Value", Value.ToString());
                baseAttributes.Add("Digital Value (Big Endian Bits)", ((ByteEncoding)ByteEncoding.BigEndianBinary).GetString(valueBytes));
                baseAttributes.Add("Digital Value (Hexadecimal)", "0x" + ((ByteEncoding)ByteEncoding.Hexadecimal).GetString(valueBytes));

                return baseAttributes;
            }
        }

    }
}
