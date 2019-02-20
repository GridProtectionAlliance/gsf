//******************************************************************************************************
//  CertificateSigningMode.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/27/2018 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Diagnostics.CodeAnalysis;

namespace GSF.Security.Cryptography.X509
{
    //https://nvlpubs.nist.gov/nistpubs/SpecialPublications/NIST.SP.800-57pt1r4.pdf
    //Table 2:
    //Encryption Strength | RSA   | ECDSA | Hash Only
    //          80        | 1024  | 160   | SHA1
    //          112       | 2048  | 224   | SHA-224
    //          128       | 3072  | 256   | SHA-256
    //          192       | 7680  | 384   | SHA-384
    //          256       | 15360 | 512   | SHA-512
    // From RFC 3766
    // RSA4096 = 142-bit.

    //Times on Windows 10 CNG. I7-6800K. 3.4GHz. 6-Core.
    //    ECDSA 256 Generate 0 ms | Sign 0.31 ms | Verify 0.27 ms
    //    ECDSA 384 Generate 1 ms | Sign 0.72 ms | Verify 0.72 ms
    //    ECDSA 521 Generate 1 ms | Sign 1.63 ms | Verify 1.74 ms
    //    RSA 512 SHA1 Generate 2 ms | Sign 0.09 ms | Verify 0.03 ms
    //    RSA 1024 SHA1 Generate 8 ms | Sign 0.18 ms | Verify 0.03 ms
    //    RSA 2048 SHA1 Generate 52 ms | Sign 0.68 ms | Verify 0.05 ms
    //    RSA 3072 SHA1 Generate 167 ms | Sign 1.91 ms | Verify 0.07 ms
    //    RSA 4096 SHA1 Generate 421 ms | Sign 4.12 ms | Verify 0.09 ms
    //    RSA 5120 SHA1 Generate 1099 ms | Sign 7.73 ms | Verify 0.15 ms
    //    RSA 6144 SHA1 Generate 2418 ms | Sign 13.04 ms | Verify 0.17 ms
    //    RSA 7168 SHA1 Generate 2491 ms | Sign 20.18 ms | Verify 0.22 ms
    //    RSA 8192 SHA1 Generate 4166 ms | Sign 29.58 ms | Verify 0.27 ms
    //    RSA 9216 SHA1 Generate 10936 ms | Sign 41.69 ms | Verify 0.35 ms
    //    RSA 10240 SHA1 Generate 9454 ms | Sign 56.90 ms | Verify 0.42 ms
    //    RSA 11264 SHA1 Generate 14348 ms | Sign 75.58 ms | Verify 0.51 ms
    //    RSA 12288 SHA1 Generate 18983 ms | Sign 97.19 ms | Verify 0.59 ms
    //    RSA 13312 SHA1 Generate 50140 ms | Sign 121.99 ms | Verify 0.69 ms
    //    RSA 14336 SHA1 Generate 30837 ms | Sign 150.17 ms | Verify 0.77 ms
    //    RSA 15360 SHA1 Generate 88815 ms | Sign 178.59 ms | Verify 0.84 ms

    /// <summary>
    /// The mechanism that will be used to sign the certificate.
    /// </summary>
    public enum CertificateSigningMode
    {
        /// <summary>
        /// 80-bit security. Before 2016.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_1024_SHA_1,
        /// <summary>
        /// 112-bit security. Until 2030.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_2048_SHA2_256,
        /// <summary>
        /// 128-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_3072_SHA2_256,
        /// <summary>
        /// 128-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_3072_SHA2_384,
        /// <summary>
        /// 128-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_4096_SHA2_256,
        /// <summary>
        /// 142-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_4096_SHA2_384,
        /// <summary>
        /// 192-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_7680_SHA2_384,
        /// <summary>
        /// 256-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        RSA_15360_SHA2_512,
        /// <summary>
        /// 128-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        ECDSA_256_SHA2_256,
        /// <summary>
        /// 192-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        ECDSA_384_SHA2_384,
        /// <summary>
        /// 256-bit security
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        ECDSA_521_SHA2_512,
    }
}