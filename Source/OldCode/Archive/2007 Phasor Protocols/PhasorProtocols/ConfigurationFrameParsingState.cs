//*******************************************************************************************************
//  ConfigurationFrameParsingState.cs
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
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IConfigurationFrame"/>.
    /// </summary>
    public class ConfigurationFrameParsingState : ChannelFrameParsingStateBase<IConfigurationCell>, IConfigurationFrameParsingState
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="IConfigurationFrame"/> being parsed.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="IConfigurationCell"/> instances.</param>
        public ConfigurationFrameParsingState(int parsedBinaryLength, CreateNewCellFunction<IConfigurationCell> createNewCellFunction)
            : base(parsedBinaryLength, createNewCellFunction)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="IConfigurationFrame"/> being parsed.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="IConfigurationCell"/> instances.</param>
        /// <param name="cellCount">Number of cells that exist in the frame to be parsed.</param>
        public ConfigurationFrameParsingState(int parsedBinaryLength, CreateNewCellFunction<IConfigurationCell> createNewCellFunction, int cellCount)
            : base(parsedBinaryLength, createNewCellFunction)
        {
            CellCount = cellCount;
        }

        #endregion
    }
}