//*******************************************************************************************************
//  StandardKey.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO PCS, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/16/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVA.Security.Cryptography
{
    /// <summary>
    /// This class is used internally do define a standard key which is used when consumer does not provide a key
    /// so that data still always gets obfuscated.
    /// </summary>
    internal static class StandardKey
    {
        // The following constants should not be changed
        public const string Source = "{§&-<«%=£($#/P.C:S!\\_¤,@[20O9¡]*ªn^±j`&|?)>+~¥}";
        public const int BufferSize = 262144; // 256K
        public static byte[] Value = Cipher.GetBinaryKeyFromString(Source);
    }
}
