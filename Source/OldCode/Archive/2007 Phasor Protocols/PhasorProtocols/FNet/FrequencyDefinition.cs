using System.Diagnostics;
using System;
////using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
////using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  FrequencyDefinition.vb - FNet Frequency definition
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
//  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PhasorProtocols
{
    namespace FNet
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


            }

            public FrequencyDefinition(ConfigurationCell parent, string label, int scale, float offset, int dfdtScale, float dfdtOffset)
                : base(parent, label, scale, offset, dfdtScale, dfdtOffset)
            {


            }

            public FrequencyDefinition(ConfigurationCell parent, IFrequencyDefinition frequencyDefinition)
                : base(parent, frequencyDefinition)
            {


            }

            // FNet supports no configuration frame in the data stream - so there will be nothing to parse
            //Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As int)

            //    MyBase.New(parent, binaryImage, startIndex)

            //End Sub

            //Friend Shared Function CreateNewFrequencyDefinition(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As int) As IFrequencyDefinition

            //    Return New FrequencyDefinition(parent, binaryImage, startIndex)

            //End Function

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

        }

    }
}
