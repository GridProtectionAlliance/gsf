//*******************************************************************************************************
//  IDataFrameParsingState.cs
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
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state of a <see cref="IDataFrame"/>.
    /// </summary>
    public interface IDataFrameParsingState : IChannelFrameParsingState<IDataCell>
    {
        /// <summary>
        /// Gets reference to the <see cref="IConfigurationFrame"/> associated with the <see cref="IDataFrame"/> being parsed.
        /// </summary>
        IConfigurationFrame ConfigurationFrame { get; }
    }
}