using System.Diagnostics;
using System;
////using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
////using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  FrequencyDefinition.vb - IEEE 1344 Frequency definition
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
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PCS.PhasorProtocols
{
    namespace Ieee1344
    {

        [CLSCompliant(false), Serializable()]
        public class FrequencyDefinition : FrequencyDefinitionBase
        {



            private short m_statusFlags;

            protected FrequencyDefinition()
            {
            }

            protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize frequency definition
                m_statusFlags = info.GetInt16("statusFlags");

            }

            public FrequencyDefinition(ConfigurationCell parent)
                : base(parent)
            {

                ScalingFactor = 1000;
                DfDtScalingFactor = 100;

            }

            public FrequencyDefinition(ConfigurationCell parent, byte[] binaryImage, int startIndex)
                : base(parent, binaryImage, startIndex)
            {

                ScalingFactor = 1000;
                DfDtScalingFactor = 100;

            }

            public FrequencyDefinition(ConfigurationCell parent, string label, int scale, float offset, int dfdtScale, float dfdtOffset)
                : base(parent, label, scale, offset, dfdtScale, dfdtOffset)
            {


            }

            public FrequencyDefinition(ConfigurationCell parent, IFrequencyDefinition frequencyDefinition)
                : base(parent, frequencyDefinition)
            {


            }

            internal static IFrequencyDefinition CreateNewFrequencyDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            {

                return new FrequencyDefinition((ConfigurationCell)parent, binaryImage, startIndex);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationCell Parent
            {
                get
                {
                    return (ConfigurationCell)base.Parent;
                }
            }

            public bool FrequencyIsAvailable
            {
                get
                {
                    return (m_statusFlags & Bit.Bit8) == 0;
                }
                set
                {
                    if (value)
                    {
                        m_statusFlags = (short)(m_statusFlags & ~Bit.Bit8);
                    }
                    else
                    {
                        m_statusFlags = (short)(m_statusFlags | Bit.Bit8);
                    }
                }
            }

            public bool DfDtIsAvailable
            {
                get
                {
                    return (m_statusFlags & Bit.Bit9) == 0;
                }
                set
                {
                    if (value)
                    {
                        m_statusFlags = (short)(m_statusFlags & ~Bit.Bit9);
                    }
                    else
                    {
                        m_statusFlags = (short)(m_statusFlags | Bit.Bit9);
                    }
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
                    if (NominalFrequency == LineFrequency.Hz50)
                    {
                        m_statusFlags = (short)(m_statusFlags | Bit.Bit0);
                    }
                    else
                    {
                        m_statusFlags = (short)(m_statusFlags & ~Bit.Bit0);
                    }

                    return EndianOrder.BigEndian.GetBytes(m_statusFlags);
                }
            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                Parent.NominalFrequency = ((m_statusFlags & Bit.Bit0) > 0) ? LineFrequency.Hz50 : LineFrequency.Hz60;

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize frequency definition
                info.AddValue("statusFlags", m_statusFlags);

            }

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

        }

    }
}
