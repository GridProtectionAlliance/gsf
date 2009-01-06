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

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// This interface represents a protocol independent representation of any kind of data cell.
    /// </summary>
    /// <remarks>
    /// This phasor protocol implementation defines a "cell" as a portion of a frame, i.e., a logical unit of data.
    /// For example, a <see cref="IDataCell"/> (dervied from <see cref="IChannelCell"/>) could be defined as a PMU
    /// within a frame of data, a <see cref="IDataFrame"/>, that contains multiple PMU's coming from a PDC.
    /// </remarks>
    [CLSCompliant(false)]
    public interface IChannelCell : IChannel, ISerializable
    {
        /// <summary>
        /// Gets a reference to the parent <see cref="IChannelFrame"/> for this <see cref="IChannelCell"/>.
        /// </summary>
        IChannelFrame Parent { get; }

        /// <summary>
        /// Gets the numeric ID code for this <see cref="IChannelCell"/>.
        /// </summary>
        /// <remarks>
        /// Most phasor measurement devices define some kind of numeric identifier (e.g., a hardware identifier coded into the device ROM); this is the
        /// abstract representation of this identifier.
        /// </remarks>
        ushort IDCode { get; set; }

        /// <summary>
        /// Gets a flag that determines if the <see cref="IChannelCell"/> is aligned on a double-word (i.e., 32-bit) boundry.
        /// </summary>
        /// <remarks>
        /// If protocol requires this property to be true, the <see cref="ISupportBinaryImage.BinaryLength"/> of the <see cref="IChannelCell"/>
        /// will be padded to align evenly at 4-byte intervals.
        /// </remarks>
        bool AlignOnDWordBoundary { get; }
    }
}
