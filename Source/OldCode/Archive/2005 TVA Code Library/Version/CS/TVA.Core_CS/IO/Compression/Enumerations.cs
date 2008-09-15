using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.IO.Compression.Enumerations.vb - Compression enumerations
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
//  03/25/2008 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA
{
    namespace IO
    {
        namespace Compression
        {


            /// <summary>
            /// Specifies the level of compression to be performed on data.
            /// </summary>
            public enum CompressLevel
            {
                DefaultCompression = -1,
                NoCompression = 0,
                BestSpeed = 1,
                BestCompression = 9,
                MultiPass = 10
            }

        }
    }
}
