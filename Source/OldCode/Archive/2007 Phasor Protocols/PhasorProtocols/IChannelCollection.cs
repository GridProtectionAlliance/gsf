//*******************************************************************************************************
//  IChannelCollection.cs
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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of a collection of any
    /// <see cref="IChannel"/> objects.<br/>
    /// This is the base interface implemented by all collections classes in the phasor
    /// protocols library; it is the root of the collection interface hierarchy.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IChannel"/> type that the <see cref="IChannelCollection{T}"/> contains.</typeparam>
    public interface IChannelCollection<T> : IChannel, IList<T>, ISerializable where T : IChannel
    {
        // Note that the channel collection interface inherits IChannel hence providing cumulative imaging properties
    }
}
