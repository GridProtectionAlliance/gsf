//*******************************************************************************************************
//  IChannelCell.cs
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

using System.Runtime.Serialization;
using PCS.Parsing;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any kind of <see cref="IChannelFrame"/> cell.
    /// </summary>
    /// <remarks>
    /// This phasor protocol implementation defines a "cell" as a portion of a "frame", i.e., a logical unit of data.
    /// For example, a <see cref="IDataCell"/> (dervied from <see cref="IChannelCell"/>) could be defined as a PMU
    /// within a frame of data, a <see cref="IDataFrame"/>, that contains multiple PMU's coming from a PDC.
    /// </remarks>
    public interface IChannelCell : IChannel, ISerializable
    {
        /// <summary>
        /// Gets a reference to the parent <see cref="IChannelFrame"/> for this <see cref="IChannelCell"/>.
        /// </summary>
        IChannelFrame Parent { get; set; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ChannelCellBase"/>.
        /// </summary>
        new IChannelCellParsingState State { get; set; }

        /// <summary>
        /// Gets the numeric ID code for this <see cref="IChannelCell"/>.
        /// </summary>
        /// <remarks>
        /// Most phasor measurement devices define some kind of numeric identifier (e.g., a hardware identifier coded into the device ROM); this is the
        /// abstract representation of this identifier.
        /// </remarks>
        ushort IDCode { get; set; }
    }
}