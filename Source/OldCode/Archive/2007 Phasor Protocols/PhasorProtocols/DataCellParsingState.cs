using System.Diagnostics;
using System;
//using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
//using TVA.Interop.Bit;
using System.Linq;

//*******************************************************************************************************
//  DataCellParsingState.vb - Data frame cell parsing state class
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
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

namespace PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of the parsing state of a data frame cell that can be sent or received from a PMU.</summary>
    [CLSCompliant(false)]
    public class DataCellParsingState : ChannelCellParsingStateBase, IDataCellParsingState
    {



        private IConfigurationCell m_configurationCell;
        private CreateNewValueFunctionSignature<IPhasorDefinition, IPhasorValue> m_createNewPhasorValueFunction;
        private CreateNewValueFunctionSignature<IFrequencyDefinition, IFrequencyValue> m_createNewFrequencyValueFunction;
        private CreateNewValueFunctionSignature<IAnalogDefinition, IAnalogValue> m_createNewAnalogValueFunction;
        private CreateNewValueFunctionSignature<IDigitalDefinition, IDigitalValue> m_createNewDigitalValueFunction;

        public DataCellParsingState(IConfigurationCell configurationCell, CreateNewValueFunctionSignature<IPhasorDefinition, IPhasorValue> createNewPhasorValueFunction, CreateNewValueFunctionSignature<IFrequencyDefinition, IFrequencyValue> createNewFrequencyValueFunction, CreateNewValueFunctionSignature<IAnalogDefinition, IAnalogValue> createNewAnalogValueFunction, CreateNewValueFunctionSignature<IDigitalDefinition, IDigitalValue> createNewDigitalValueFunction)
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

        public override System.Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

        public virtual IConfigurationCell ConfigurationCell
        {
            get
            {
                return m_configurationCell;
            }
        }

        public virtual CreateNewValueFunctionSignature<IPhasorDefinition, IPhasorValue> CreateNewPhasorValueFunction
        {
            get
            {
                return m_createNewPhasorValueFunction;
            }
        }

        public virtual CreateNewValueFunctionSignature<IFrequencyDefinition, IFrequencyValue> CreateNewFrequencyValueFunction
        {
            get
            {
                return m_createNewFrequencyValueFunction;
            }
        }

        public virtual CreateNewValueFunctionSignature<IAnalogDefinition, IAnalogValue> CreateNewAnalogValueFunction
        {
            get
            {
                return m_createNewAnalogValueFunction;
            }
        }

        public virtual CreateNewValueFunctionSignature<IDigitalDefinition, IDigitalValue> CreateNewDigitalValueFunction
        {
            get
            {
                return m_createNewDigitalValueFunction;
            }
        }

    }
}
