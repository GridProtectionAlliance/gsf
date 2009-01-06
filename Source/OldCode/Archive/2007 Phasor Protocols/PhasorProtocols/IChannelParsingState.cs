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

using System;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// This interface represents a protocol independent parsing state used by any kind of data.
    /// </summary>
    /// <remarks>
    /// This is the parsing state root interface of the phasor protocol library.<br/>
    /// Data parsing is very format specific, classes implementing this interface create a common
    /// form for parsing state information particular to a data type.
    /// </remarks>
    public interface IChannelParsingState
    {
        /// <summary>
        /// Gets the final derived type of class implementing <see cref="IChannelParsingState"/>.
        /// </summary>
        /// <remarks>
        /// This is expected to be overriden by the final derived class.
        /// </remarks>
        Type DerivedType { get; }
    }
}