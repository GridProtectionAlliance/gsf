//*******************************************************************************************************
//  IFrameImage.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using PCS.Parsing;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the interface that uniquely identifies IEEE 1344 frame images (see <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/>).
    /// </summary>
    [CLSCompliant(false)]
    public interface IFrameImage : ISupportFrameImage<FrameType>, IChannelFrame
    {
    }
}