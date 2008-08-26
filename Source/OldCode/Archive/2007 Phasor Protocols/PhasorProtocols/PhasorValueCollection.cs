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
//  PhasorValueCollection.vb - Phasor value collection class
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Note: Phasors are stored in rectangular format internally
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    /// <summary>This class represents the protocol independent collection of phasor values.</summary>
    [CLSCompliant(false), Serializable()]
    public class PhasorValueCollection : ChannelValueCollectionBase<IPhasorDefinition, IPhasorValue>
    {



        protected PhasorValueCollection()
        {
        }

        protected PhasorValueCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        public PhasorValueCollection(int maximumCount)
            : base(maximumCount)
        {


        }

        public override Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

    }
}
