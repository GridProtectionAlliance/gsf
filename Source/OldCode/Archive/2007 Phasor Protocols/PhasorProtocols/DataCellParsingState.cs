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

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IDataCell"/>.
    /// </summary>
    public class DataCellParsingState : ChannelCellParsingStateBase, IDataCellParsingState
    {
        #region [ Members ]

        // Fields
        private IConfigurationCell m_configurationCell;
        private CreateNewValueFunction<IPhasorDefinition, IPhasorValue> m_createNewPhasorValueFunction;
        private CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> m_createNewFrequencyValueFunction;
        private CreateNewValueFunction<IAnalogDefinition, IAnalogValue> m_createNewAnalogValueFunction;
        private CreateNewValueFunction<IDigitalDefinition, IDigitalValue> m_createNewDigitalValueFunction;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="configurationFrame">Reference to the <see cref="IConfigurationCell"/> associated with the <see cref="IDataCell"/> being parsed.</param>
        /// <param name="createNewPhasorValueFunction">Reference to delegate to create new <see cref="IPhasorValue"/> instances.</param>
        /// <param name="createNewFrequencyValueFunction">Reference to delegate to create new <see cref="IFrequencyValue"/> instances.</param>
        /// <param name="createNewAnalogValueFunction">Reference to delegate to create new <see cref="IAnalogValue"/> instances.</param>
        /// <param name="createNewDigitalValueFunction">Reference to delegate to create new <see cref="IDigitalValue"/> instances.</param>
        public DataCellParsingState(IConfigurationCell configurationCell, CreateNewValueFunction<IPhasorDefinition, IPhasorValue> createNewPhasorValueFunction, CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> createNewFrequencyValueFunction, CreateNewValueFunction<IAnalogDefinition, IAnalogValue> createNewAnalogValueFunction, CreateNewValueFunction<IDigitalDefinition, IDigitalValue> createNewDigitalValueFunction)
        {
            m_configurationCell = configurationCell;
            m_createNewPhasorValueFunction = createNewPhasorValueFunction;
            m_createNewFrequencyValueFunction = createNewFrequencyValueFunction;
            m_createNewAnalogValueFunction = createNewAnalogValueFunction;
            m_createNewDigitalValueFunction = createNewDigitalValueFunction;

            PhasorCount = m_configurationCell.PhasorDefinitions.Count;
            AnalogCount = m_configurationCell.AnalogDefinitions.Count;
            DigitalCount = m_configurationCell.DigitalDefinitions.Count;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="IConfigurationCell"/> associated with the <see cref="IDataCell"/> being parsed.
        /// </summary>
        public virtual IConfigurationCell ConfigurationCell
        {
            get
            {
                return m_configurationCell;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IPhasorValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IPhasorDefinition, IPhasorValue> CreateNewPhasorValue
        {
            get
            {
                return m_createNewPhasorValueFunction;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IFrequencyValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IFrequencyDefinition, IFrequencyValue> CreateNewFrequencyValue
        {
            get
            {
                return m_createNewFrequencyValueFunction;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IAnalogValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IAnalogDefinition, IAnalogValue> CreateNewAnalogValue
        {
            get
            {
                return m_createNewAnalogValueFunction;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewValueFunction{TDefinition,TValue}"/> delegate used to create new <see cref="IDigitalValue"/> objects.
        /// </summary>
        public virtual CreateNewValueFunction<IDigitalDefinition, IDigitalValue> CreateNewDigitalValue
        {
            get
            {
                return m_createNewDigitalValueFunction;
            }
        }

        #endregion
    }
}