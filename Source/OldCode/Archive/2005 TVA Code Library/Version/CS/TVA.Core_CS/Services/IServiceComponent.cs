using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Xml.Common.vb - Common XML Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/23/2003 - J. Ritchie Carroll
//       Original version of source code generated
//  01/23/2006 - J. Ritchie Carroll
//       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
//
//*******************************************************************************************************

namespace TVA
{
    namespace Services
    {

        /// <summary>
        /// Defines an interface for user created components used by the service so that components can inform service
        /// of current status and automatically react to service events.
        /// </summary>
        public interface IServiceComponent : IDisposable
        {


            string Name
            {
                get;
            }
            string Status
            {
                get;
            }
            void ServiceStateChanged(ServiceState newState);
            void ProcessStateChanged(string processName, ProcessState newState);

        }

    }
}
