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

namespace PCS.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream protocol implementation of the parsing state used by a <see cref="DataFrame"/>.
    /// </summary>
    public class DataFrameParsingState : PhasorProtocols.DataFrameParsingState
    {
        #region [ Members ]

        // Fields
        private int m_remainingPdcBlockPmus;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="IDataFrame"/> being parsed.</param>
        /// <param name="configurationFrame">Reference to the <see cref="IConfigurationFrame"/> associated with the <see cref="IDataFrame"/> being parsed.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="IDataCell"/> instances.</param>
        public DataFrameParsingState(int parsedBinaryLength, IConfigurationFrame configurationFrame, CreateNewCellFunction<IDataCell> createNewCellFunction)
            : base(parsedBinaryLength, configurationFrame, createNewCellFunction)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationFrame"/> associated with the <see cref="DataFrame"/> being parsed.
        /// </summary>
        public new ConfigurationFrame ConfigurationFrame
        {
            get
            {
                return base.ConfigurationFrame as ConfigurationFrame;
            }
        }

        /// <summary>
        /// Gets or sets the remaining number of PMU's the PDC block to be parsed.
        /// </summary>
        public int RemainingPdcBlockPmus
        {
            get
            {
                return m_remainingPdcBlockPmus;
            }
            set
            {
                m_remainingPdcBlockPmus = value;
            }
        }

        #endregion
    }
}