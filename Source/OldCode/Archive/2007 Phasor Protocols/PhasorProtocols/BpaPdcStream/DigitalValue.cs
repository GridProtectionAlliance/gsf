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
//  DigitalValue.vb - PDCstream Digital value
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
    namespace BpaPdcStream
    {

        [CLSCompliant(false), Serializable()]
        public class DigitalValue : DigitalValueBase
        {



            protected DigitalValue()
            {
            }

            protected DigitalValue(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public DigitalValue(IDataCell parent, IDigitalDefinition digitalDefinition, short value)
                : base(parent, digitalDefinition, value)
            {


            }

            public DigitalValue(IDataCell parent, IDigitalDefinition digitalDefinition, byte[] binaryImage, int startIndex)
                : base(parent, digitalDefinition, binaryImage, startIndex)
            {


            }

            public DigitalValue(IDataCell parent, IDigitalDefinition digitalDefinition, IDigitalValue digitalValue)
                : base(parent, digitalDefinition, digitalValue)
            {


            }

            internal static IDigitalValue CreateNewDigitalValue(IDataCell parent, IDigitalDefinition definition, byte[] binaryImage, int startIndex)
            {

                return new DigitalValue(parent, definition, binaryImage, startIndex);

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

            public new DigitalDefinition Definition
            {
                get
                {
                    return (DigitalDefinition)base.Definition;
                }
                set
                {
                    base.Definition = value;
                }
            }

        }

    }
}
