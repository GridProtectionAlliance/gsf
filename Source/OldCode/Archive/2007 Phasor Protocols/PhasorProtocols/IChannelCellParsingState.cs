//*******************************************************************************************************
//  IChannelCellParsingState.cs
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
    /// Represents a protocol independent interface representation of the parsing state of any kind of frame cell.
    /// </summary>
    public interface IChannelCellParsingState : IChannelParsingState
    {
        /// <summary>
        /// Gets or sets the number of phasor elements associated with the <see cref="IChannelCell"/>.
        /// </summary>
        int PhasorCount { get; set; }

        /// <summary>
        /// Gets or sets the number of analog elements associated with the <see cref="IChannelCell"/>.
        /// </summary>
        int AnalogCount { get; set; }

        /// <summary>
        /// Gets or sets the number of digital elements associated with the <see cref="IChannelCell"/>.
        /// </summary>
        int DigitalCount { get; set; }
    }
}
