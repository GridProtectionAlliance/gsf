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
//  IFrequencyValue.vb - Frequency value interface
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
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent interface of a frequency value.</summary>
    [CLSCompliant(false)]
    public interface IFrequencyValue : IChannelValue<IFrequencyDefinition>
    {


        float Frequency
        {
            get;
            set;
        }

        float DfDt
        {
            get;
            set;
        }

        short UnscaledFrequency
        {
            get;
            set;
        }

        short UnscaledDfDt
        {
            get;
            set;
        }

    }
}
