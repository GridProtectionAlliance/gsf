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

//*******************************************************************************************************
//  FrequencyValue.vb - FNet Frequency value
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


namespace PCS.PhasorProtocols
{
    namespace FNet
    {

        [CLSCompliant(false), Serializable()]
        public class FrequencyValue : FrequencyValueBase
        {



            protected FrequencyValue()
            {
            }

            protected FrequencyValue(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition, float frequency, float dfdt)
                : base(parent, frequencyDefinition, frequency, dfdt)
            {


            }

            public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition, short unscaledFrequency, short unscaledDfDt)
                : base(parent, frequencyDefinition, unscaledFrequency, unscaledDfDt)
            {


            }

            public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition, IFrequencyValue frequencyValue)
                : base(parent, frequencyDefinition, frequencyValue)
            {


            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataCell Parent
            {
                get
                {
                    return (DataCell)base.Parent;
                }
            }

            public new FrequencyDefinition Definition
            {
                get
                {
                    return (FrequencyDefinition)base.Definition;
                }
                set
                {
                    base.Definition = value;
                }
            }

        }

    }
}
