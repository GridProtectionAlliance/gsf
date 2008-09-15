using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  Enumerations.vb - Global enumerations for this namespace
//  Copyright © 2005 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/11/2007 - J. Ritchie Carroll
//       Moved all namespace level enumerations into "Enumerations.vb" file
//
//*******************************************************************************************************

namespace TVA
{
    namespace Services
    {

        /// <summary>Windows service states</summary>
        public enum ServiceState
        {
            Started,
            Stopped,
            Paused,
            Resumed,
            Shutdown
        }

        /// <summary>Windows service process states</summary>
        public enum ProcessState
        {
            Unprocessed,
            Processing,
            Processed,
            Aborted,
            Exception
        }

    }
}
