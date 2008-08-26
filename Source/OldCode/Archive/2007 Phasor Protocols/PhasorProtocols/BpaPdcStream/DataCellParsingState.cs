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
//  DataCellParsingState.vb - BPA PDCstream specific data frame cell parsing state class
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
    namespace BpaPdcStream
    {

        /// <summary>This class represents the BPA PDCstream protocol implementation of the parsing state of a data frame cell that can be sent or received from a PMU.</summary>
        [CLSCompliant(false)]
        public class DataCellParsingState : PhasorProtocols.DataCellParsingState
        {



            private bool m_isPdcBlockPmu;
            private int m_index;

            public DataCellParsingState(IConfigurationCell configurationCell, CreateNewValueFunctionSignature<IPhasorDefinition, IPhasorValue> createNewPhasorValueFunction, CreateNewValueFunctionSignature<IFrequencyDefinition, IFrequencyValue> createNewFrequencyValueFunction, CreateNewValueFunctionSignature<IAnalogDefinition, IAnalogValue> createNewAnalogValueFunction, CreateNewValueFunctionSignature<IDigitalDefinition, IDigitalValue> createNewDigitalValueFunction, int index)
                : base(configurationCell, createNewPhasorValueFunction, createNewFrequencyValueFunction, createNewAnalogValueFunction, createNewDigitalValueFunction)
            {


                m_index = index;

            }

            public DataCellParsingState(IConfigurationCell configurationCell, CreateNewValueFunctionSignature<IPhasorDefinition, IPhasorValue> createNewPhasorValueFunction, CreateNewValueFunctionSignature<IFrequencyDefinition, IFrequencyValue> createNewFrequencyValueFunction, CreateNewValueFunctionSignature<IAnalogDefinition, IAnalogValue> createNewAnalogValueFunction, CreateNewValueFunctionSignature<IDigitalDefinition, IDigitalValue> createNewDigitalValueFunction, bool isPdcBlockPmu)
                : base(configurationCell, createNewPhasorValueFunction, createNewFrequencyValueFunction, createNewAnalogValueFunction, createNewDigitalValueFunction)
            {


                m_isPdcBlockPmu = isPdcBlockPmu;

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationCell ConfigurationCell
            {
                get
                {
                    return (ConfigurationCell)base.ConfigurationCell;
                }
            }

            public bool IsPdcBlockPmu
            {
                get
                {
                    return m_isPdcBlockPmu;
                }
            }

            public int Index
            {
                get
                {
                    return m_index;
                }
            }

        }

    }

}
