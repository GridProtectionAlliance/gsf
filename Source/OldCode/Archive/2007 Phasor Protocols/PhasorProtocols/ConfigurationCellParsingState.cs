//*******************************************************************************************************
//  ConfigurationCellParsingState.vb - Configuration frame cell parsing state class
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of a parsing state for a set of configuration related data settings that can be sent or received from a PMU.</summary>
    [CLSCompliant(false)]
    public class ConfigurationCellParsingState : ChannelCellParsingStateBase, IConfigurationCellParsingState
    {
        private CreateNewDefinitionFunctionSignature<IPhasorDefinition> m_createNewPhasorDefinitionFunction;
        private CreateNewDefinitionFunctionSignature<IFrequencyDefinition> m_createNewFrequencyDefinitionFunction;
        private CreateNewDefinitionFunctionSignature<IAnalogDefinition> m_createNewAnalogDefinitionFunction;
        private CreateNewDefinitionFunctionSignature<IDigitalDefinition> m_createNewDigitalDefinitionFunction;

        public ConfigurationCellParsingState(CreateNewDefinitionFunctionSignature<IPhasorDefinition> createNewPhasorDefinitionFunction, CreateNewDefinitionFunctionSignature<IFrequencyDefinition> createNewFrequencyDefinitionFunction, CreateNewDefinitionFunctionSignature<IAnalogDefinition> createNewAnalogDefinitionFunction, CreateNewDefinitionFunctionSignature<IDigitalDefinition> createNewDigitalDefinitionFunction)
        {

            m_createNewPhasorDefinitionFunction = createNewPhasorDefinitionFunction;
            m_createNewFrequencyDefinitionFunction = createNewFrequencyDefinitionFunction;
            m_createNewAnalogDefinitionFunction = createNewAnalogDefinitionFunction;
            m_createNewDigitalDefinitionFunction = createNewDigitalDefinitionFunction;

        }

        public override Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

        public virtual CreateNewDefinitionFunctionSignature<IPhasorDefinition> CreateNewPhasorDefinitionFunction
        {
            get
            {
                return m_createNewPhasorDefinitionFunction;
            }
        }

        public virtual CreateNewDefinitionFunctionSignature<IFrequencyDefinition> CreateNewFrequencyDefinitionFunction
        {
            get
            {
                return m_createNewFrequencyDefinitionFunction;
            }
        }

        public virtual CreateNewDefinitionFunctionSignature<IAnalogDefinition> CreateNewAnalogDefinitionFunction
        {
            get
            {
                return m_createNewAnalogDefinitionFunction;
            }
        }

        public virtual CreateNewDefinitionFunctionSignature<IDigitalDefinition> CreateNewDigitalDefinitionFunction
        {
            get
            {
                return m_createNewDigitalDefinitionFunction;
            }
        }
    }
}
