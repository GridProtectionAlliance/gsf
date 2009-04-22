//*******************************************************************************************************
//  DataCellParsingState.cs
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
    /// Represents the BPA PDCstream protocol implementation of the parsing state used by a <see cref="DataCell"/>.
    /// </summary>
    public class DataCellParsingState : PhasorProtocols.DataCellParsingState
    {
        #region [ Members ]

        // Fields
        private int m_index;
        private bool m_isPdcBlockPmu;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="configurationCell">Reference to the <see cref="IConfigurationCell"/> associated with the <see cref="IDataCell"/> being parsed.</param>
        /// <param name="createNewPhasorValue">Reference to delegate to create new <see cref="IPhasorValue"/> instances.</param>
        /// <param name="createNewFrequencyValue">Reference to delegate to create new <see cref="IFrequencyValue"/> instances.</param>
        /// <param name="createNewAnalogValue">Reference to delegate to create new <see cref="IAnalogValue"/> instances.</param>
        /// <param name="createNewDigitalValue">Reference to delegate to create new <see cref="IDigitalValue"/> instances.</param>
        /// <param name="index">Index of associated <see cref="DataCell"/> PMU.</param>
        public DataCellParsingState(IConfigurationCell configurationCell, CreateNewValueFunction<IPhasorDefinition, IPhasorValue> createNewPhasorValue, CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> createNewFrequencyValue, CreateNewValueFunction<IAnalogDefinition, IAnalogValue> createNewAnalogValue, CreateNewValueFunction<IDigitalDefinition, IDigitalValue> createNewDigitalValue, int index)
            : base(configurationCell, createNewPhasorValue, createNewFrequencyValue, createNewAnalogValue, createNewDigitalValue)
        {


            m_index = index;

        }

        /// <summary>
        /// Creates a new <see cref="DataCellParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="configurationCell">Reference to the <see cref="IConfigurationCell"/> associated with the <see cref="IDataCell"/> being parsed.</param>
        /// <param name="createNewPhasorValue">Reference to delegate to create new <see cref="IPhasorValue"/> instances.</param>
        /// <param name="createNewFrequencyValue">Reference to delegate to create new <see cref="IFrequencyValue"/> instances.</param>
        /// <param name="createNewAnalogValue">Reference to delegate to create new <see cref="IAnalogValue"/> instances.</param>
        /// <param name="createNewDigitalValue">Reference to delegate to create new <see cref="IDigitalValue"/> instances.</param>
        /// <param name="isPdcBlockPmu">Flag that determines if associated <see cref="DataCell"/> PMU is in a PDC block.</param>
        public DataCellParsingState(IConfigurationCell configurationCell, CreateNewValueFunction<IPhasorDefinition, IPhasorValue> createNewPhasorValue, CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> createNewFrequencyValue, CreateNewValueFunction<IAnalogDefinition, IAnalogValue> createNewAnalogValue, CreateNewValueFunction<IDigitalDefinition, IDigitalValue> createNewDigitalValue, bool isPdcBlockPmu)
            : base(configurationCell, createNewPhasorValue, createNewFrequencyValue, createNewAnalogValue, createNewDigitalValue)
        {
            m_isPdcBlockPmu = isPdcBlockPmu;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCell"/> associated with the <see cref="DataCell"/> being parsed.
        /// </summary>
        public new ConfigurationCell ConfigurationCell
        {
            get
            {
                return base.ConfigurationCell as ConfigurationCell;
            }
        }

        /// <summary>
        /// Gets index of associated <see cref="DataCell"/> PMU.
        /// </summary>
        public int Index
        {
            get
            {
                return m_index;
            }
        }

        /// <summary>
        /// Gets flag that determines if associated <see cref="DataCell"/> PMU is in a PDC block.
        /// </summary>
        public bool IsPdcBlockPmu
        {
            get
            {
                return m_isPdcBlockPmu;
            }
        }

        #endregion
    }
}