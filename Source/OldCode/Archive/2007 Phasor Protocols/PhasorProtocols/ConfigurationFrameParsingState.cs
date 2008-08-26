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
//  ConfigurationFrameParsingState.vb - Configuration frame parsing state class
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
    /// <summary>This class represents the protocol independent common implementation the parsing state of a data frame that can be sent or received from a PMU.</summary>
    [CLSCompliant(false)]
    public class ConfigurationFrameParsingState : ChannelFrameParsingStateBase<IConfigurationCell>, IConfigurationFrameParsingState
    {



        public ConfigurationFrameParsingState(ConfigurationCellCollection cells, ushort frameLength, CreateNewCellFunctionSignature<IConfigurationCell> createNewCellFunction)
            : base((IChannelCellCollection<IConfigurationCell>)cells, frameLength, createNewCellFunction)
        {


        }

        public override System.Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

        public virtual new ConfigurationCellCollection Cells
        {
            get
            {
                return (ConfigurationCellCollection)base.Cells;
            }
        }

    }
}
