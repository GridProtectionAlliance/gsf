//*******************************************************************************************************
//  Standard.cs
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

namespace TVA.Security.Cryptography
{
    /// <summary>
    /// This class is used internally do define a standard key and buffer size. The standard key gets
    /// used within the <see cref="Cipher"/> class so that when consumer does not provide a key, data
    /// will still at least get obfuscated.
    /// </summary>
    internal static class Standard
    {
        // The following constants should not be changed
        public const string Key = "{§&-<«%=£($#/P.C:S!\\_¤,@[20O9¡]*ªn^±j`&|?)>+~¥}";
        public static byte[] KeyValue = Cipher.GetBinaryKeyFromString(Key);
        public const int BufferSize = 262144; // 256K
    }
}
