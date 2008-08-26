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
using TVA.Parsing;

//*******************************************************************************************************
//  IChannel.vb - Channel interface - this is the root interface
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
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PhasorProtocols
{
    /// <summary>This interface represents a protocol independent representation of any data type.</summary>
    [CLSCompliant(false)]
    public interface IChannel : IBinaryDataProvider
    {

        Type DerivedType
        {
            get;
        }

        // At its most basic level - all data represented by the protocols can either be "parsed" or "generated"
        // hence the following methods common to all elements

        void ParseBinaryImage(IChannelParsingState state, byte[] binaryImage, int startIndex);

        new ushort BinaryLength
        {
            get;
        }

        Dictionary<string, string> Attributes
        {
            get;
        }

        object Tag
        {
            get;
            set;
        }

    }
}
