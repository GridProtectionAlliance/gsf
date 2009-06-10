//*******************************************************************************************************
//  IChannelFrameParsingState.cs
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

namespace PhasorProtocols
{
    /// <summary>
    /// Defines function signature for creating new <see cref="IChannelCell"/> objects.
    /// </summary>
    /// <param name="parent">Reference to parent <see cref="IChannelFrame"/>.</param>
    /// <param name="state">Current parsing state of associated <see cref="IChannelFrame"/>.</param>
    /// <param name="index">Index of cell within its collection.</param>
    /// <param name="binaryImage">Binary image to parse <see cref="IChannelCell"/> from.</param>
    /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
    /// <param name="parsedLength">Returns the total number of bytes parsed from <paramref name="binaryImage"/>.</param>
    /// <returns>New <see cref="IChannelCell"/> object.</returns>
    /// <typeparam name="T">Specific <see cref="IChannelCell"/> type of object that gets created by referenced function.</typeparam>
    public delegate T CreateNewCellFunction<T>(IChannelFrame parent, IChannelFrameParsingState<T> state, int index, byte[] binaryImage, int startIndex, out int parsedLength) where T : IChannelCell;

    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state of any kind of <see cref="IChannelFrame"/>.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IChannelCell"/> type that the <see cref="IChannelFrameParsingState{T}"/> references.</typeparam>
    public interface IChannelFrameParsingState<T> : IChannelParsingState where T : IChannelCell
    {
        /// <summary>
        /// Gets reference to delegate used to create a new <see cref="IChannelCell"/> object.
        /// </summary>
        CreateNewCellFunction<T> CreateNewCell { get; }

        /// <summary>
        /// Gets or sets number of cells that are expected in associated <see cref="IChannelFrame"/> being parsed.
        /// </summary>
        int CellCount { get; set; }
    }
}