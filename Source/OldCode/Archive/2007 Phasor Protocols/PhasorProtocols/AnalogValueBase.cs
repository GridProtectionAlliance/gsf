using System.Diagnostics;
using System;
//using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
//using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  AnalogValueBase.vb - Analog value base class
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
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

namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent representation of an analog value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class AnalogValueBase : ChannelValueBase<IAnalogDefinition>, IAnalogValue
    {



        private float m_value;
        private bool m_valueAssigned;

        protected AnalogValueBase()
        {
        }

        protected AnalogValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize analog value
            m_value = info.GetSingle("value");
            m_valueAssigned = true;

        }

        protected AnalogValueBase(IDataCell parent)
            : base(parent)
        {


        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal value As Single)
        protected AnalogValueBase(IDataCell parent, IAnalogDefinition analogDefinition, float value)
            : base(parent, analogDefinition)
        {


            m_value = value;
            m_valueAssigned = !float.IsNaN(value);

        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal unscaledValue As short)
        protected AnalogValueBase(IDataCell parent, IAnalogDefinition analogDefinition, short integerValue)
            : this(parent, analogDefinition, (float)integerValue)
        {


        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As int)
        protected AnalogValueBase(IDataCell parent, IAnalogDefinition analogDefinition, byte[] binaryImage, int startIndex)
            : base(parent, analogDefinition)
        {

            ParseBinaryImage(null, binaryImage, startIndex);

        }

        // Derived classes are expected expose a Public Sub New(ByVal analogValue As IAnalogValue)
        protected AnalogValueBase(IDataCell parent, IAnalogDefinition analogDefinition, IAnalogValue analogValue)
            : this(parent, analogDefinition, analogValue.Value)
        {


        }

        public virtual float Value
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

        public virtual short IntegerValue
        {
            get
            {
                try
                {
                    return (short)m_value;
                }
                catch (OverflowException)
                {
                    return short.MinValue;
                }
            }
            set
            {
                m_value = (float)value;
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
                m_value = value;
                m_valueAssigned = true;
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

        protected override ushort BodyLength
        {
            get
            {
                if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                {
                    return 2;
                }
                else
                {
                    return 4;
                }
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                {
                    EndianOrder.BigEndian.CopyBytes(IntegerValue, buffer, 0);
                }
                else
                {
                    EndianOrder.BigEndian.CopyBytes(m_value, buffer, 0);
                }

                return buffer;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
            {
                IntegerValue = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
            }
            else
            {
                m_value = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
                m_valueAssigned = true;
            }

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize analog value
            info.AddValue("value", m_value);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Analog Value (Floating Point)", Value.ToString());
                baseAttributes.Add("Analog Value (Integer)", IntegerValue.ToString());

                return baseAttributes;
            }
        }

    }
}
