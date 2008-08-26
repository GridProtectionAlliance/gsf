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
using System.Text;

//*******************************************************************************************************
//  FrequencyDefinition.vb - IEEE C37.118 Frequency definition
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
    namespace IeeeC37_118
    {

        [CLSCompliant(false), Serializable()]
        public class FrequencyDefinition : FrequencyDefinitionBase
        {



            protected FrequencyDefinition()
            {
            }

            protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


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

            protected override ushort BodyLength
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
                    return EndianOrder.BigEndian.GetBytes((short)(Parent.NominalFrequency == LineFrequency.Hz50 ? Bit.Bit0 : Bit.Nill));
                }
            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                Parent.NominalFrequency = ((EndianOrder.BigEndian.ToInt16(binaryImage, startIndex) & Bit.Bit0) > 0) ? LineFrequency.Hz50 : LineFrequency.Hz60;

            }

        }

    }
}
