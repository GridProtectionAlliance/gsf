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

using System;
using System.Collections.Generic;
using PCS.Parsing;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// This interface represents a protocol independent representation of any data type that can be parsed or generated.
    /// </summary>
    /// <remarks>
    /// This is the root interface of the phasor protocol library.
    /// </remarks>
    public interface IChannel : ISupportBinaryImage
    {
        /// <summary>
        /// Final derived <see cref="IChannel"/> <see cref="Type"/>.
        /// </summary>
        Type DerivedType { get; }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="IChannel"/> object.
        /// </summary>
        Dictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets or sets the parsing state for the <see cref="IChannel"/> object.
        /// </summary>
        IChannelParsingState State { get; set; }

        /// <summary>
        /// User definable object used to hold a reference associated with the <see cref="IChannel"/> object.
        /// </summary>
        object Tag { get; set; }
    }
}