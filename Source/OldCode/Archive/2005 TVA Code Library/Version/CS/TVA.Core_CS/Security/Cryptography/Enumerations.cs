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
//       Moved all namespace level enumerations into "Enumerations.vb" file.
//  10/11/2007 - J. Ritchie Carroll
//       Added "Level 5" encryption to further obfuscate data using bit-rotation.
// 12/13/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************

namespace TVA
{
    namespace Security
    {
        namespace Cryptography
        {


            /// <summary>Enumerates cryptographic strength.</summary>
            /// <remarks>
            /// <para>
            /// Encryption algorithms are cumulative. The levels represent tradeoffs on speed vs. cipher strength. Level 1
            /// will have the fastest encryption speed with the simplest encryption strength, and level 5 will have the
            /// strongest cumulative encryption strength with the slowest encryption speed.
            /// </para>
            /// </remarks>
            public enum EncryptLevel
            {
                /// <summary>Uses no encryption.</summary>
                None,
                /// <summary>Adds simple multi-alogorithm XOR based encryption.</summary>
                Level1,
                /// <summary>Adds TripleDES based encryption.</summary>
                Level2,
                /// <summary>Adds RC2 based encryption.</summary>
                Level3,
                /// <summary>Adds RijndaelManaged based enryption.</summary>
                Level4,
                /// <summary>Adds simple bit-rotation based enryption.</summary>
                Level5
            }

        }
    }
}
