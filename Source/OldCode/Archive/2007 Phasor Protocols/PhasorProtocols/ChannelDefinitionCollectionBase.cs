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
//  ChannelDefinitionCollectionBase.vb - Channel data definition collection base class
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
//  3/7/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent collection of definitions of any kind of data.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelDefinitionCollectionBase<T> : ChannelCollectionBase<T> where T : IChannelDefinition
    {



        protected ChannelDefinitionCollectionBase()
        {
        }

        protected ChannelDefinitionCollectionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        protected ChannelDefinitionCollectionBase(int maximumCount)
            : base(maximumCount)
        {


        }

        public override void Add(T item)
        {

            base.Add(item);
            item.Index = Count - 1;

        }

    }
}
