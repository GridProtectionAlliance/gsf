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
//  PhasorDefinition.vb - Phasor definition
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
//*******************************************************************************************************


namespace PhasorProtocols
{
    namespace Ieee1344
    {

        [CLSCompliant(false), Serializable()]
        public class DigitalDefinition : DigitalDefinitionBase
        {



            private short m_statusFlags;

            protected DigitalDefinition()
            {
            }

            protected DigitalDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize digital definition
                m_statusFlags = info.GetInt16("statusFlags");

            }

            public DigitalDefinition(ConfigurationCell parent)
                : base(parent)
            {


            }

            public DigitalDefinition(ConfigurationCell parent, int index, string label)
                : base(parent, index, label)
            {


            }

            public DigitalDefinition(ConfigurationCell parent, byte[] binaryImage, int startIndex)
                : base(parent, binaryImage, startIndex)
            {


            }

            public DigitalDefinition(ConfigurationCell parent, IDigitalDefinition digitalDefinition)
                : base(parent, digitalDefinition)
            {


            }

            internal static IDigitalDefinition CreateNewDigitalDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            {

                return new DigitalDefinition((ConfigurationCell)parent, binaryImage, startIndex);

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

            public short NormalStatus
            {
                get
                {
                    return (short)(m_statusFlags & Bit.Bit4);
                }
                set
                {
                    if (value > 0)
                    {
                        m_statusFlags = (short)(m_statusFlags | Bit.Bit4);
                    }
                    else
                    {
                        m_statusFlags = (short)(m_statusFlags & ~Bit.Bit4);
                    }
                }
            }

            public short ValidInput
            {
                get
                {
                    return (short)(m_statusFlags & Bit.Bit0);
                }
                set
                {
                    if (value > 0)
                    {
                        m_statusFlags = (short)(m_statusFlags | Bit.Bit0);
                    }
                    else
                    {
                        m_statusFlags = (short)(m_statusFlags & ~Bit.Bit0);
                    }
                }
            }

            internal static int ConversionFactorLength
            {
                get
                {
                    return 2;
                }
            }

            internal byte[] ConversionFactorImage
            {
                get
                {
                    return EndianOrder.BigEndian.GetBytes(m_statusFlags);
                }
            }

            internal void ParseConversionFactor(byte[] binaryImage, int startIndex)
            {

                m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize digital definition
                info.AddValue("statusFlags", m_statusFlags);

            }

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

        }

    }

}
