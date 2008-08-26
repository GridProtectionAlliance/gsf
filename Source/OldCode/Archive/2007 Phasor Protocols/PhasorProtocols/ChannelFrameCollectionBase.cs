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
//  ChannelFrameCollectionBase.vb - Channel data frame collection base class
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of a collection of any frame of data that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelFrameCollectionBase<T> : ChannelCollectionBase<T> where T : IChannelFrame
    {



        protected ChannelFrameCollectionBase()
        {
        }

        protected ChannelFrameCollectionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        protected ChannelFrameCollectionBase(int maximumCount)
            : base(maximumCount)
        {


        }

        public override ushort BinaryLength
        {
            get
            {
                // Frames will be different lengths, so we must manually sum lengths
                ushort length = 0;

                for (int x = 0; x <= Count - 1; x++)
                {
                    length += this[x].BinaryLength;
                }

                return length;
            }
        }

    }
}
