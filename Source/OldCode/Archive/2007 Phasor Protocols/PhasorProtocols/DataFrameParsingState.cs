//*******************************************************************************************************
//  DataFrameParsingState.cs
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
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IDataFrame"/>.
    /// </summary>
    public class DataFrameParsingState : ChannelFrameParsingStateBase<IDataCell>, IDataFrameParsingState
    {
        #region [ Members ]

        // Fields
        private IConfigurationFrame m_configurationFrame;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="IDataFrame"/> being parsed.</param>
        /// <param name="configurationFrame">Reference to the <see cref="IConfigurationFrame"/> associated with the <see cref="IDataFrame"/> being parsed.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="IDataCell"/> instances.</param>
        public DataFrameParsingState(int parsedBinaryLength, IConfigurationFrame configurationFrame, CreateNewCellFunction<IDataCell> createNewCellFunction)
            : base(parsedBinaryLength, createNewCellFunction)
        {
            if (configurationFrame != null)
            {
                CellCount = configurationFrame.Cells.Count;
                m_configurationFrame = configurationFrame;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="IConfigurationFrame"/> associated with the <see cref="IDataFrame"/> being parsed.
        /// </summary>
        public virtual IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
        }

        #endregion
    }
}