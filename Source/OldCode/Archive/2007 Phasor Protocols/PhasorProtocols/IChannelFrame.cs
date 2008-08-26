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
using TVA.DateTime;
using TVA.Measurements;

//*******************************************************************************************************
//  IChannelFrame.vb - Channel data frame interface
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
    /// <summary>This interface represents the protocol independent representation of any frame of data.</summary>
    [CLSCompliant(false)]
    public interface IChannelFrame : IChannel, IFrame, IComparable, ISerializable
    {


        FundamentalFrameType FrameType
        {
            get;
        }

        object Cells
        {
            get;
        }

        ushort IDCode
        {
            get;
            set;
        }

        UnixTimeTag TimeTag
        { // UNIX based time of this frame
            get;
        }

        bool IsPartial
        {
            get;
        }

    }
}
