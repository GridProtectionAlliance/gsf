using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;

//*******************************************************************************************************
//  IDataCellParsingState.vb - Data cell parsing state interface
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
//  04/16/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    public delegate TValue CreateNewValueFunctionSignature<TDefinition, TValue>(IDataCell parent, TDefinition definition, Byte[] binaryImage, int startIndex)
        where TDefinition : IChannelDefinition
        where TValue : IChannelValue<TDefinition>;

    /// <summary>This interface represents the protocol independent parsing state of a set of phasor related data values.</summary>
    [CLSCompliant(false)]
    public interface IDataCellParsingState : IChannelCellParsingState
    {
        IConfigurationCell ConfigurationCell
        {
            get;
        }

        CreateNewValueFunctionSignature<IPhasorDefinition, IPhasorValue> CreateNewPhasorValueFunction
        {
            get;
        }

        CreateNewValueFunctionSignature<IFrequencyDefinition, IFrequencyValue> CreateNewFrequencyValueFunction
        {
            get;
        }

        CreateNewValueFunctionSignature<IAnalogDefinition, IAnalogValue> CreateNewAnalogValueFunction
        {
            get;
        }

        CreateNewValueFunctionSignature<IDigitalDefinition, IDigitalValue> CreateNewDigitalValueFunction
        {
            get;
        }

    }
}
