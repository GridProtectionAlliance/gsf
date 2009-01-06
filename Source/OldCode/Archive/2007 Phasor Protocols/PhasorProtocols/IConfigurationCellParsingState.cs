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
//  IConfigurationCellParsingState.vb - Configuration cell parsing state interface
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
//  04/16/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    public delegate T CreateNewDefinitionFunctionSignature<T>(IConfigurationCell parent, Byte[] binaryImage, int startIndex) where T : IChannelDefinition;

    /// <summary>This interface represents the protocol independent parsing state of a set of configuration related data settings (typically related to a PMU).</summary>
    [CLSCompliant(false)]
    public interface IConfigurationCellParsingState : IChannelCellParsingState
    {
        CreateNewDefinitionFunctionSignature<IPhasorDefinition> CreateNewPhasorDefinitionFunction
        {
            get;
        }

        CreateNewDefinitionFunctionSignature<IFrequencyDefinition> CreateNewFrequencyDefinitionFunction
        {
            get;
        }

        CreateNewDefinitionFunctionSignature<IAnalogDefinition> CreateNewAnalogDefinitionFunction
        {
            get;
        }

        CreateNewDefinitionFunctionSignature<IDigitalDefinition> CreateNewDigitalDefinitionFunction
        {
            get;
        }

    }
}
