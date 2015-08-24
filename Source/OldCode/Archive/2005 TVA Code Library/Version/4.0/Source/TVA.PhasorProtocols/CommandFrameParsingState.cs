//*******************************************************************************************************
//  CommandFrameParsingState.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="ICommandFrame"/>.
    /// </summary>
    public class CommandFrameParsingState : ChannelFrameParsingStateBase<ICommandCell>, ICommandFrameParsingState
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="ICommandFrame"/> being parsed.</param>
        /// <param name="dataLength">Length of data in <see cref="ICommandFrame"/> being parsed (i.e., number of cells).</param>
        public CommandFrameParsingState(int parsedBinaryLength, int dataLength)
            : base(parsedBinaryLength, CommandCell.CreateNewCell)
        {
            CellCount = dataLength;
        }

        #endregion
    }
}