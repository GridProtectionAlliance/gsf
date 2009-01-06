//*******************************************************************************************************
//  ChannelParsingStateBase.cs
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
//  3/7/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent parsing state class used by any kind of data.</summary>
    /// <remarks>This class is inherited by subsequent classes to provide parsing state information particular to data type needs.</remarks>
    public abstract class ChannelParsingStateBase : IChannelParsingState
    {
        /// <summary>
        /// Gets the final derived type of class implementing <see cref="IChannelParsingState"/>.
        /// </summary>
        /// <remarks>
        /// This is expected to be overriden by the final derived class.
        /// </remarks>
        public abstract Type DerivedType { get; }
    }
}
