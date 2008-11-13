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
//  AnalogDefinition.vb - PDCstream Analog definition
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
    namespace BpaPdcStream
    {

        [CLSCompliant(false), Serializable()]
        public class AnalogDefinition : AnalogDefinitionBase
        {



            protected AnalogDefinition()
            {
            }

            protected AnalogDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public AnalogDefinition(ConfigurationCell parent)
                : base(parent)
            {


            }

            public AnalogDefinition(ConfigurationCell parent, int index, string label)
                : base(parent, index, label, 1, 0)
            {


            }

            public AnalogDefinition(ConfigurationCell parent, IAnalogDefinition analogDefinition)
                : base(parent, analogDefinition)
            {


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

            public override int MaximumLabelLength
            {
                get
                {
                    return int.MaxValue;
                }
            }

            protected override ushort BodyLength
            {
                get
                {
                    return 0;
                }
            }

            /// <remarks>BPA PDCstream does not include analog definition in descriptor packet.  Only a count of available values is defined in the data frame.</remarks>
            protected override byte[] BodyImage
            {
                get
                {
                    return null;
                }
            }

        }

    }

}
