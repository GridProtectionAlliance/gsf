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
//  IConfigurationFrame.vb - Configuration frame interface
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
    /// <summary>This interface represents the protocol independent representation of any configuration frame.</summary>
    [CLSCompliant(false)]
    public interface IConfigurationFrame : IChannelFrame
    {


        new ConfigurationCellCollection Cells
        {
            get;
        }

        short FrameRate
        {
            get;
            set;
        }

        decimal TicksPerFrame
        {
            get;
        }

        void SetNominalFrequency(LineFrequency value);

    }
}
