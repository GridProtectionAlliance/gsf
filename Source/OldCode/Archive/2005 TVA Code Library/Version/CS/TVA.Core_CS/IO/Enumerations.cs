using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.IO.Enumerations.vb - Common Configuration Functions
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  ??/??/???? - Pinal C. Patel
//       Generated original version of source code.
//  08/22/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************
namespace TVA
{
    namespace IO
    {

        /// <summary>
        /// Specifies the operation to be performed on the log file when it is full.
        /// </summary>
        public enum LogFileFullOperation
        {
            /// <summary>
            /// Truncates the existing entries in the log file to make space for new entries.
            /// </summary>
            Truncate,
            /// <summary>
            /// Rolls over to a new log file, and keeps the full log file for reference.
            /// </summary>
            Rollover
        }

    }
}
