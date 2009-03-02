//*******************************************************************************************************
//  IChannelParsingState.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state used by any kind of
    /// <see cref="IChannel"/> data.<br/>
    /// This is the base interface implemented by all parsing state classes in the phasor protocols library;
    /// it is the root of the parsing state interface hierarchy.
    /// </summary>
    /// <remarks>
    /// Data parsing is very format specific, classes implementing this interface create a common form for
    /// parsing state information particular to a data type.
    /// </remarks>
    public interface IChannelParsingState
    {
        /// <summary>
        /// Gets or sets the length of the associated <see cref="IChannel"/> object being parsed from the binary image.
        /// </summary>
        int ParsedBinaryLength { get; set; }
    }
}