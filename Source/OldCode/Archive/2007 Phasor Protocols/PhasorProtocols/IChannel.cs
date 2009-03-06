//*******************************************************************************************************
//  IChannel.cs
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System.Collections.Generic;
using PCS.Parsing;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any data type that can
    /// be parsed or generated.<br/>
    /// This is the base interface implemented by all parsing/generating classes in the phasor
    /// protocols library; it is the root of the parsing/generating interface hierarchy.
    /// </summary>
    public interface IChannel : ISupportBinaryImage
    {
        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="IChannel"/> object.
        /// </summary>
        Dictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets or sets the parsing state for this <see cref="IChannel"/> object.
        /// </summary>
        IChannelParsingState State { get; set; }

        /// <summary>
        /// Gets or sets a user definable reference to an object associated with this <see cref="IChannel"/> object.
        /// </summary>
        /// <remarks>
        /// Classes implementing <see cref="IChannel"/> should only track this value.
        /// </remarks>
        object Tag { get; set; }
    }
}