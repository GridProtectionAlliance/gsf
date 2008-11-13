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
//  AnalogValue.vb - PDCstream Analog value
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

        /// <summary>
        /// BPA PDCstream Analog Value Class
        /// </summary>
        [CLSCompliant(false), Serializable()]
        public class AnalogValue : AnalogValueBase
        {



            protected AnalogValue()
            {
            }

            protected AnalogValue(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public AnalogValue(IDataCell parent, IAnalogDefinition analogDefinition, float value)
                : base(parent, analogDefinition, value)
            {


            }

            public AnalogValue(IDataCell parent, IAnalogDefinition analogDefinition, short unscaledValue)
                : base(parent, analogDefinition, unscaledValue)
            {


            }

            public AnalogValue(IDataCell parent, IAnalogDefinition analogDefinition, byte[] binaryImage, int startIndex)
                : base(parent, analogDefinition, binaryImage, startIndex)
            {


            }

            public AnalogValue(IDataCell parent, IAnalogDefinition analogDefinition, IAnalogValue analogValue)
                : base(parent, analogDefinition, analogValue)
            {


            }

            internal static IAnalogValue CreateNewAnalogValue(IDataCell parent, IAnalogDefinition definition, byte[] binaryImage, int startIndex)
            {

                return new AnalogValue(parent, definition, binaryImage, startIndex);

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

            public new AnalogDefinition Definition
            {
                get
                {
                    return (AnalogDefinition)base.Definition;
                }
                set
                {
                    base.Definition = value;
                }
            }

        }

    }
}
