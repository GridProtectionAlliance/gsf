//*******************************************************************************************************
//  FrequencyValueBase.vb - Frequency and DfDt value base class
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
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************using System.Diagnostics;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TVA;
using TVA.Interop;

namespace PhasorProtocols
{
    /// <summary>This class represents the protocol independent a frequency and dfdt value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class FrequencyValueBase : ChannelValueBase<IFrequencyDefinition>, IFrequencyValue
    {
        private float m_frequency;
        private float m_dfdt;
        private bool m_frequencyAssigned;
        private bool m_dfdtAssigned;

        protected FrequencyValueBase()
        {
        }

        protected FrequencyValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize frequency value
            m_frequency = info.GetSingle("frequency");
            m_dfdt = info.GetSingle("dfdt");

            m_frequencyAssigned = true;
            m_dfdtAssigned = true;
        }

        protected FrequencyValueBase(IDataCell parent)
            : base(parent)
        {
        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)
        protected FrequencyValueBase(IDataCell parent, IFrequencyDefinition frequencyDefinition, float frequency, float dfdt)
            : base(parent, frequencyDefinition)
        {
            m_frequency = frequency;
            m_dfdt = dfdt;

            m_frequencyAssigned = !float.IsNaN(frequency);
            m_dfdtAssigned = !float.IsNaN(dfdt);
        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As short, ByVal unscaledDfDt As short)
        protected FrequencyValueBase(IDataCell parent, IFrequencyDefinition frequencyDefinition, short unscaledFrequency, short unscaledDfDt)
            : this(parent, frequencyDefinition, (float)unscaledFrequency / (float)frequencyDefinition.ScalingFactor + frequencyDefinition.Offset, (float)unscaledDfDt / (float)frequencyDefinition.DfDtScalingFactor + frequencyDefinition.DfDtOffset)
        {
        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As int)
        protected FrequencyValueBase(IDataCell parent, IFrequencyDefinition frequencyDefinition, byte[] binaryImage, int startIndex)
            : base(parent, frequencyDefinition)
        {
            ParseBinaryImage(null, binaryImage, startIndex);
        }

        // Derived classes are expected to expose a Public Sub New(ByVal frequencyValue As IFrequencyValue)
        protected FrequencyValueBase(IDataCell parent, IFrequencyDefinition frequencyDefinition, IFrequencyValue frequencyValue)
            : this(parent, frequencyDefinition, frequencyValue.Frequency, frequencyValue.DfDt)
        {
        }

        public virtual float Frequency
        {
            get
            {
                return m_frequency;
            }
            set
            {
                m_frequency = value;
                m_frequencyAssigned = true;
            }
        }

        public virtual float DfDt
        {
            get
            {
                return m_dfdt;
            }
            set
            {
                m_dfdt = value;
                m_dfdtAssigned = true;
            }
        }

        public virtual short UnscaledFrequency
        {
            get
            {
                return (short)((m_frequency - Definition.Offset) * (float)Definition.ScalingFactor);
            }
            set
            {
                m_frequency = (float)value / (float)Definition.ScalingFactor + Definition.Offset;
                m_frequencyAssigned = true;
            }
        }

        public virtual short UnscaledDfDt
        {
            get
            {
                return (short)((m_dfdt - Definition.DfDtOffset) * (float)Definition.DfDtScalingFactor);
            }
            set
            {
                m_dfdt = (float)value / (float)Definition.DfDtScalingFactor + Definition.DfDtOffset;
                m_dfdtAssigned = true;
            }
        }

        public override float this[int index]
        {
            get
            {
                switch (index)
                {
                    case CompositeFrequencyValue.Frequency:
                        return m_frequency;
                    case CompositeFrequencyValue.DfDt:
                        return m_dfdt;
                    default:
                        throw (new IndexOutOfRangeException("Specified frequency value composite index, " + index + ", is out of range - there are only two composite values for a frequency value: frequency (0) and df/dt (1)"));
                }
            }
            set
            {
                switch (index)
                {
                    case CompositeFrequencyValue.Frequency:
                        m_frequency = value;
                        m_frequencyAssigned = true;
                        break;
                    case CompositeFrequencyValue.DfDt:
                        m_dfdt = value;
                        m_dfdtAssigned = true;
                        break;
                    default:
                        throw (new IndexOutOfRangeException("Specified frequency value composite index, " + index + ", is out of range - there are only two composite values for a frequency value: frequency (0) and df/dt (1)"));
                }
            }
        }

        public override int CompositeValueCount
        {
            get
            {
                return 2;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (!m_frequencyAssigned || !m_dfdtAssigned);
            }
        }

        protected override ushort BodyLength
        {
            get
            {
                if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                {
                    return 4;
                }
                else
                {
                    return 8;
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
                    EndianOrder.BigEndian.CopyBytes(UnscaledFrequency, buffer, 0);
                    EndianOrder.BigEndian.CopyBytes(UnscaledDfDt, buffer, 2);
                }
                else
                {
                    EndianOrder.BigEndian.CopyBytes(m_frequency, buffer, 0);
                    EndianOrder.BigEndian.CopyBytes(m_dfdt, buffer, 4);
                }

                return buffer;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
            if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
            {
                UnscaledFrequency = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                UnscaledDfDt = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2);
            }
            else
            {
                m_frequency = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
                m_dfdt = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4);

                m_frequencyAssigned = true;
                m_dfdtAssigned = true;
            }
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize frequency value
            info.AddValue("frequency", m_frequency);
            info.AddValue("dfdt", m_dfdt);
        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frequency Value", Frequency.ToString());
                baseAttributes.Add("df/dt Value", DfDt.ToString());
                baseAttributes.Add("Unscaled Frequency Value", UnscaledFrequency.ToString());
                baseAttributes.Add("Unscaled df/dt Value", UnscaledDfDt.ToString());

                return baseAttributes;
            }
        }
    }
}
