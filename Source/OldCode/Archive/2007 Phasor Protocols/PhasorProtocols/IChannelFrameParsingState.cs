using System.Diagnostics;
using System;
////using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
////using PCS.Interop.Bit;
using System.Linq;

//*******************************************************************************************************
//  IChannelFrameParsingState.vb - Channel data frame parsing state interface
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
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

namespace PCS.PhasorProtocols
{
    public delegate T CreateNewCellFunctionSignature<T>(IChannelFrame parent, IChannelFrameParsingState<T> state, int index, byte[] binaryImage, int startIndex) where T : IChannelCell;

    /// <summary>This interface represents the protocol independent parsing state of any frame of data.</summary>
    [CLSCompliant(false)]
    public interface IChannelFrameParsingState<T> : IChannelParsingState where T : IChannelCell
    {
        CreateNewCellFunctionSignature<T> CreateNewCellFunction { get; }

        //IChannelCellCollection<T> Cells
        //{
        //    get;
        //}

        int CellCount { get; set; }

        int ParsedBinaryLength { get; set; }
    }
}
