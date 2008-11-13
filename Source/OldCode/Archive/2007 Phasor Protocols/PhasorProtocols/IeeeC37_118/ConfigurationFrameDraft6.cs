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
//  ConfigurationFrameDraft6.vb - IEEE C37.118 Draft6 Configuration Frame
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


namespace PCS.PhasorProtocols
{
    namespace IeeeC37_118
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationFrameDraft6 : ConfigurationFrame
        {



            protected ConfigurationFrameDraft6()
            {
            }

            protected ConfigurationFrameDraft6(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public ConfigurationFrameDraft6(FrameType frameType, int timeBase, ushort idCode, long ticks, short frameRate, byte version)
                : base(frameType, timeBase, idCode, ticks, frameRate, version)
            {


            }

            public ConfigurationFrameDraft6(ICommonFrameHeader parsedFrameHeader, byte[] binaryImage, int startIndex)
                : base(parsedFrameHeader, binaryImage, startIndex)
            {


            }

            public ConfigurationFrameDraft6(IConfigurationFrame configurationFrame)
                : base(configurationFrame)
            {


            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public override DraftRevision DraftRevision
            {
                get
                {
                    return IeeeC37_118.DraftRevision.Draft6;
                }
            }

        }

    }

}
