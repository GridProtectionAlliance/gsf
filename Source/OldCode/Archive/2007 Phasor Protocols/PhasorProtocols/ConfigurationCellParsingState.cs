//*******************************************************************************************************
//  ConfigurationCellParsingState.cs
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
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IConfigurationCell"/>.
    /// </summary>
    public class ConfigurationCellParsingState : ChannelCellParsingStateBase, IConfigurationCellParsingState
    {
        #region [ Members ]

        // Fields
        private CreateNewDefinitionFunction<IPhasorDefinition> m_createNewPhasorDefinitionFunction;
        private CreateNewDefinitionFunction<IFrequencyDefinition> m_createNewFrequencyDefinitionFunction;
        private CreateNewDefinitionFunction<IAnalogDefinition> m_createNewAnalogDefinitionFunction;
        private CreateNewDefinitionFunction<IDigitalDefinition> m_createNewDigitalDefinitionFunction;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="createNewPhasorDefinitionFunction">Reference to delegate to create new <see cref="IPhasorDefinition"/> instances.</param>
        /// <param name="createNewFrequencyDefinitionFunction">Reference to delegate to create new <see cref="IFrequencyDefinition"/> instances.</param>
        /// <param name="createNewAnalogDefinitionFunction">Reference to delegate to create new <see cref="IAnalogDefinition"/> instances.</param>
        /// <param name="createNewDigitalDefinitionFunction">Reference to delegate to create new <see cref="IDigitalDefinition"/> instances.</param>
        public ConfigurationCellParsingState(CreateNewDefinitionFunction<IPhasorDefinition> createNewPhasorDefinitionFunction, CreateNewDefinitionFunction<IFrequencyDefinition> createNewFrequencyDefinitionFunction, CreateNewDefinitionFunction<IAnalogDefinition> createNewAnalogDefinitionFunction, CreateNewDefinitionFunction<IDigitalDefinition> createNewDigitalDefinitionFunction)
        {
            m_createNewPhasorDefinitionFunction = createNewPhasorDefinitionFunction;
            m_createNewFrequencyDefinitionFunction = createNewFrequencyDefinitionFunction;
            m_createNewAnalogDefinitionFunction = createNewAnalogDefinitionFunction;
            m_createNewDigitalDefinitionFunction = createNewDigitalDefinitionFunction;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IPhasorDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IPhasorDefinition> CreateNewPhasorDefinition
        {
            get
            {
                return m_createNewPhasorDefinitionFunction;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IFrequencyDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IFrequencyDefinition> CreateNewFrequencyDefinition
        {
            get
            {
                return m_createNewFrequencyDefinitionFunction;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IAnalogDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IAnalogDefinition> CreateNewAnalogDefinition
        {
            get
            {
                return m_createNewAnalogDefinitionFunction;
            }
        }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunction{T}"/> delegate used to create new <see cref="IDigitalDefinition"/> objects.
        /// </summary>
        public virtual CreateNewDefinitionFunction<IDigitalDefinition> CreateNewDigitalDefinition
        {
            get
            {
                return m_createNewDigitalDefinitionFunction;
            }
        }

        #endregion
    }
}