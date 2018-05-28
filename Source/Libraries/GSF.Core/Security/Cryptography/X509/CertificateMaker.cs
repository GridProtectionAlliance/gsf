//******************************************************************************************************
//  CertificateMaker.cs - Gbtc
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

using System;
using System.Security.Cryptography.X509Certificates;

namespace GSF.Security.Cryptography.X509
{
    /// <summary>
    /// Allows for on the fly creation of a self signed X509 certificate.
    /// </summary>
    public static class CertificateMaker
    {
        /// <summary>
        /// Creates a certificate authority certificate.
        /// </summary>
        /// <returns></returns>
        public static X509Certificate2 GenerateSelfSignedCertificate(CertificateSigningMode mode, string subjectName, DateTime startDate = default(DateTime), DateTime endDate = default(DateTime))
        {
            var gen = new X509CertificateGenerator();
            gen.Issuer = subjectName;
            gen.Subject = subjectName;

            if (startDate != default(DateTime))
                gen.NotBefore = startDate;

            if (endDate != default(DateTime))
                gen.NotAfter = endDate;

            switch (mode)
            {
                case CertificateSigningMode.RSA_1024_SHA_1:
                    gen.KeySize = 1024;
                    gen.SignatureBits = 160;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.RSA_2048_SHA2_256:
                    gen.KeySize = 2048;
                    gen.SignatureBits = 256;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.RSA_3072_SHA2_256:
                    gen.KeySize = 3072;
                    gen.SignatureBits = 256;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.RSA_3072_SHA2_384:
                    gen.KeySize = 3072;
                    gen.SignatureBits = 384;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.RSA_4096_SHA2_256:
                    gen.KeySize = 4096;
                    gen.SignatureBits = 256;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.RSA_4096_SHA2_384:
                    gen.KeySize = 4096;
                    gen.SignatureBits = 384;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.RSA_7680_SHA2_384:
                    gen.KeySize = 7680;
                    gen.SignatureBits = 384;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.RSA_15360_SHA2_512:
                    gen.KeySize = 15360;
                    gen.SignatureBits = 512;
                    gen.CipherEngine = CipherEngine.RSACng;
                    break;
                case CertificateSigningMode.ECDSA_256_SHA2_256:
                    gen.KeySize = 256;
                    gen.SignatureBits = 256;
                    gen.CipherEngine = CipherEngine.ECDsaCng;
                    break;
                case CertificateSigningMode.ECDSA_384_SHA2_384:
                    gen.KeySize = 384;
                    gen.SignatureBits = 384;
                    gen.CipherEngine = CipherEngine.ECDsaCng;
                    break;
                case CertificateSigningMode.ECDSA_521_SHA2_512:
                    gen.KeySize = 521;
                    gen.SignatureBits = 512;
                    gen.CipherEngine = CipherEngine.ECDsaCng;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
            return gen.Generate();
        }
    }
    
}