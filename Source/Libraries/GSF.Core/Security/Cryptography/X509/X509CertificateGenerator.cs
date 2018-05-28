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

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace GSF.Security.Cryptography.X509
{
    /// <summary>
    /// A class to Generate Version 3 X509Certificates.
    /// </summary>
    public class X509CertificateGenerator
    {
        private abstract class EncryptionMethod : IDisposable
        {
            public string SignatureOID { get; protected set; }

            public abstract void Dispose();

            public abstract void SignData(byte[] data, DerWriter wr);

            public abstract void WritePublicKey(DerWriter wr);
            public abstract void WritePrivateKey(DerWriter wr);
        }

        private class RSAMode : EncryptionMethod
        {
            private RSAParameters m_parameters;
            private HashAlgorithmName hashName;
            private CngKey m_key;
            private RSACng m_cng;
            private RSACryptoServiceProvider m_csp;
            private RSA m_rsa;

            public RSAMode(int signatureBits, int keySize, bool useCNG)
            {
                switch (signatureBits)
                {
                    case 160:
                        hashName = HashAlgorithmName.SHA1;
                        SignatureOID = Sha1WithRsaEncryption;
                        break;
                    case 256:
                        hashName = HashAlgorithmName.SHA256;
                        SignatureOID = Sha256WithRsaEncryption;
                        break;
                    case 384:
                        hashName = HashAlgorithmName.SHA384;
                        SignatureOID = Sha384WithRsaEncryption;
                        break;
                    case 512:
                        hashName = HashAlgorithmName.SHA512;
                        SignatureOID = Sha512WithRsaEncryption;
                        break;
                    default:
                        throw new ArgumentException("Invalid signature bit size.", nameof(SignatureBits));
                }

                if (useCNG)
                {
                    var p = new CngKeyCreationParameters();
                    p.ExportPolicy = CngExportPolicies.AllowExport | CngExportPolicies.AllowPlaintextExport;
                    p.KeyUsage = CngKeyUsages.AllUsages;
                    p.KeyCreationOptions = CngKeyCreationOptions.OverwriteExistingKey;
                    p.Parameters.Add(new CngProperty("Length", BitConverter.GetBytes(keySize), CngPropertyOptions.None));
                    m_key = CngKey.Create(CngAlgorithm.Rsa, null, p);
                    m_cng = new RSACng(m_key);
                    m_rsa = m_cng;
                }
                else
                {
                    CspParameters parms = new CspParameters();
                    parms.KeyContainerName = Guid.NewGuid().ToString().ToUpperInvariant();
                    m_csp = new RSACryptoServiceProvider(keySize, parms);
                    m_csp.PersistKeyInCsp = false;
                    m_rsa = m_csp;
                }

                m_parameters = m_rsa.ExportParameters(true);
            }

            public override void Dispose()
            {
                m_csp?.Dispose();
                m_cng?.Dispose();
                m_key?.Dispose();
            }

            public override void SignData(byte[] data, DerWriter wr)
            {
                byte[] sign = m_rsa.SignData(data, hashName, RSASignaturePadding.Pkcs1);
                wr.Write(sign);
            }

            public override void WritePublicKey(DerWriter wr)
            {
                using (wr.BeginSequence())
                {
                    wr.WriteOID(RsaEncryption);
                    wr.WriteNull();
                }
                using (wr.BeginBitString())
                using (wr.BeginSequence())
                {
                    wr.WriteInteger(m_parameters.Modulus);
                    wr.WriteInteger(m_parameters.Exponent);
                }
            }

            public override void WritePrivateKey(DerWriter wr)
            {
                wr.WriteInteger(0);
                using (wr.BeginSequence())
                {
                    wr.WriteOID(RsaEncryption);
                }

                using (wr.BeginOctetString())
                {
                    using (wr.BeginSequence())
                    {
                        wr.WriteInteger(0);
                        wr.WriteInteger(m_parameters.Modulus);
                        wr.WriteInteger(m_parameters.Exponent);
                        wr.WriteInteger(m_parameters.D);
                        wr.WriteInteger(m_parameters.P);
                        wr.WriteInteger(m_parameters.Q);
                        wr.WriteInteger(m_parameters.DP);
                        wr.WriteInteger(m_parameters.DQ);
                        wr.WriteInteger(m_parameters.InverseQ);
                    }
                }
            }
        }

        private class ECDSAMode : EncryptionMethod
        {
            string m_curveOID;
            private CngKey m_key;
            private ECDsaCng m_cng;

            public byte[] d;
            public byte[] PublicKeyData;

            public ECDSAMode(int signatureBits, int keySize)
            {
                CngAlgorithm keyAlg;
                CngAlgorithm hashName;

                switch (signatureBits)
                {
                    case 256:
                        hashName = CngAlgorithm.Sha256;
                        SignatureOID = EcdsaWithSha256;
                        break;
                    case 384:
                        hashName = CngAlgorithm.Sha384;
                        SignatureOID = EcdsaWithSha384;
                        break;
                    case 512:
                        hashName = CngAlgorithm.Sha512;
                        SignatureOID = EcdsaWithSha512;
                        break;
                    default:
                        throw new ArgumentException("Invalid signature bit size.", nameof(SignatureBits));
                }

                switch (keySize)
                {
                    case 256:
                        m_curveOID = Secp256r1;
                        keyAlg = CngAlgorithm.ECDsaP256;
                        break;
                    case 384:
                        m_curveOID = Secp384r1;
                        keyAlg = CngAlgorithm.ECDsaP384;
                        break;
                    case 521:
                        m_curveOID = Secp521r1;
                        keyAlg = CngAlgorithm.ECDsaP521;
                        break;
                    default:
                        throw new ArgumentException("Invalid signature bit size.", nameof(SignatureBits));
                }

                var p = new CngKeyCreationParameters();
                p.ExportPolicy = CngExportPolicies.AllowExport | CngExportPolicies.AllowPlaintextExport;
                p.KeyUsage = CngKeyUsages.AllUsages;
                p.KeyCreationOptions = CngKeyCreationOptions.OverwriteExistingKey;

                m_key = CngKey.Create(keyAlg, null, p);
                m_cng = new ECDsaCng(m_key);
                m_cng.HashAlgorithm = hashName;

                byte[] blob = m_key.Export(CngKeyBlobFormat.EccPrivateBlob);
                int blockSize = BitConverter.ToInt32(blob, 4);
                byte[] blob2 = new byte[blockSize * 2 + 1];
                Array.Copy(blob, 8, blob2, 1, blob2.Length - 1);
                blob2[0] = 4;
                d = new byte[blockSize];

                Array.Copy(blob, 8 + blockSize * 2, d, 0, d.Length);

                PublicKeyData = blob2;
            }

            public override void Dispose()
            {
                m_cng?.Dispose();
                m_key?.Dispose();
            }

            public override void SignData(byte[] data, DerWriter wr)
            {
                byte[] sign = m_cng.SignData(data);
                byte[] sx = new byte[sign.Length / 2];
                byte[] sy = new byte[sign.Length / 2];
                Array.Copy(sign, 0, sx, 0, sx.Length);
                Array.Copy(sign, sy.Length, sy, 0, sy.Length);
                using (wr.BeginBitString())
                {
                    using (wr.BeginSequence())
                    {
                        wr.WriteInteger(sx);
                        wr.WriteInteger(sy);
                    }
                }
            }

            public override void WritePublicKey(DerWriter wr)
            {
                using (wr.BeginSequence())
                {
                    wr.WriteOID(EcPublicKey);
                    wr.WriteOID(m_curveOID);
                }
                wr.Write(PublicKeyData);
            }

            public override void WritePrivateKey(DerWriter wr)
            {
                wr.WriteInteger(0);
                using (wr.BeginSequence())
                {
                    wr.WriteOID(EcPublicKey);
                    wr.WriteOID(m_curveOID);
                }

                using (wr.BeginOctetString())
                {
                    using (wr.BeginSequence())
                    {
                        wr.WriteInteger(1);
                        wr.WriteOctetString(d);
                        using (wr.BeginTaggedObject(0))
                        {
                            wr.WriteOID(m_curveOID);
                        }
                    }
                }
            }
        }

        private static readonly string RsaEncryption = "1.2.840.113549.1.1.1";
        private static readonly string Sha1WithRsaEncryption = "1.2.840.113549.1.1.5";
        private static readonly string Sha256WithRsaEncryption = "1.2.840.113549.1.1.11";
        private static readonly string Sha384WithRsaEncryption = "1.2.840.113549.1.1.12";
        private static readonly string Sha512WithRsaEncryption = "1.2.840.113549.1.1.13";
        private static readonly string Cn = "2.5.4.3";

        private static readonly string EcdsaWithSha256 = "1.2.840.10045.4.3.2";
        private static readonly string EcdsaWithSha384 = "1.2.840.10045.4.3.3";
        private static readonly string EcdsaWithSha512 = "1.2.840.10045.4.3.4";
        private static readonly string EcPublicKey = "1.2.840.10045.2.1";

        private static readonly string Secp256r1 = "1.2.840.10045.3.1.7";
        private static readonly string Secp384r1 = "1.3.132.0.34";
        private static readonly string Secp521r1 = "1.3.132.0.35";

        private static readonly string PKCS7Data = "1.2.840.113549.1.7.1";
        private static readonly string PKCS12KeyBag = "1.2.840.113549.1.12.10.1.1";
        private static readonly string PKCS12LocalKeyID = "1.2.840.113549.1.9.21";
        private static readonly string PKCS12FriendlyName = "1.2.840.113549.1.9.20";
        private static readonly string PKCS12CertBag = "1.2.840.113549.1.12.10.1.3";
        private static readonly string PKCS12X509Certificate = "1.2.840.113549.1.9.22.1";

        /// <summary>
        /// A positive serial number to use for the certificate.
        /// </summary>
        public long SerialNumber;
        /// <summary>
        /// The issuer name. Do not include CN=. Defaults to a GUID Hexstring.
        /// </summary>
        public string Issuer;
        /// <summary>
        /// The subject name. Do not include CN=. Defaults to a GUID Hexstring.
        /// </summary>
        public string Subject;
        /// <summary>
        /// The effective date. Defaults to the first day of the calendar year. (Unless it's before Jan 7, in that case, it's Jan 1 of the prior year)
        /// </summary>
        public DateTime NotBefore;
        /// <summary>
        /// Defaults to 10 years after then effective date.
        /// </summary>
        public DateTime NotAfter;
        /// <summary>
        /// The key size to generate.
        /// </summary>
        public int KeySize;
        /// <summary>
        /// The number of bits of the hash method used to sign the certificate.
        /// </summary>
        public int SignatureBits;
        /// <summary>
        /// The cipher engine to use.
        /// </summary>
        public CipherEngine CipherEngine;

        /// <summary>
        /// Used to generate an X509Certificate.
        /// </summary>
        public X509CertificateGenerator()
        {
            System.Random r = new System.Random();
            CipherEngine = CipherEngine.RSACryptoServiceProvider;

            KeySize = 2048;
            SignatureBits = 256;
            SerialNumber = ((long)r.Next() << 31) ^ r.Next();
            Issuer = Guid.NewGuid().ToString("N");
            Subject = Issuer;
            NotBefore = new DateTime(DateTime.UtcNow.Date.AddDays(-7).Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            NotAfter = NotBefore.AddYears(10);
        }

        /// <summary>
        /// Generate a new X509Certificate using the passed in SignatureCalculator.
        /// </summary>
        /// <returns>An X509Certificate.</returns>
        public X509Certificate2 Generate()
        {
            EncryptionMethod method;
            switch (CipherEngine)
            {
                case CipherEngine.RSACryptoServiceProvider:
                    method = new RSAMode(SignatureBits, KeySize, false);
                    break;
                case CipherEngine.RSACng:
                    method = new RSAMode(SignatureBits, KeySize, true);
                    break;
                case CipherEngine.ECDsaCng:
                    method = new ECDSAMode(SignatureBits, KeySize);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            using (method)
            {
                if (SerialNumber <= 0 || string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Subject))
                {
                    throw new InvalidOperationException("not all mandatory fields set");
                }

                DerWriter tbsCertificate = new DerWriter();
                using (tbsCertificate.BeginSequence())
                {
                    using (tbsCertificate.BeginSequence())
                    {
                        using (tbsCertificate.BeginTaggedObject(0))
                        {
                            tbsCertificate.WriteInteger(2);
                        }

                        tbsCertificate.WriteInteger(SerialNumber);

                        using (tbsCertificate.BeginSequence())
                        {
                            tbsCertificate.WriteOID(method.SignatureOID);
                        }

                        using (tbsCertificate.BeginSequence())
                        using (tbsCertificate.BeginSet())
                        using (tbsCertificate.BeginSequence())
                        {
                            tbsCertificate.WriteOID(Cn);
                            tbsCertificate.Write(Issuer);
                        }
                        using (tbsCertificate.BeginSequence())
                        {
                            tbsCertificate.Write(NotBefore);
                            tbsCertificate.Write(NotAfter);
                        }
                        using (tbsCertificate.BeginSequence())
                        using (tbsCertificate.BeginSet())
                        using (tbsCertificate.BeginSequence())
                        {
                            tbsCertificate.WriteOID(Cn);
                            tbsCertificate.Write(Subject);
                        }

                        using (tbsCertificate.BeginSequence())
                        {
                            method.WritePublicKey(tbsCertificate);
                        }
                    }
                    byte[] encoded = tbsCertificate.ToArray();

                    using (tbsCertificate.BeginSequence())
                        tbsCertificate.WriteOID(method.SignatureOID);
                    method.SignData(encoded, tbsCertificate);
                }

                byte[] data = tbsCertificate.ToArray();
                byte[] data2 = MakePFX(data, method);

                return new X509Certificate2(data2, "", X509KeyStorageFlags.Exportable); 
            }
        }

        /// <summary>
        /// Generates a PFX file using the supplied password.
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <param name="password">the password to encrypt the file with.</param>
        public void GenerateFile(string fileName, string password)
        {
            using (var x509 = Generate())
            {
                File.WriteAllBytes(fileName, x509.Export(X509ContentType.Pkcs12, password));
            }
        }

        private byte[] MakePFX(byte[] cert, EncryptionMethod p)
        {
            var wr = new DerWriter();
            using (wr.BeginSequence())
            {
                wr.WriteInteger(3);
                using (wr.BeginSequence())
                {
                    OuterDataObject(cert, p, wr);
                }
            }

            return wr.ToArray();
        }

        private static void OuterDataObject(byte[] cert, EncryptionMethod p, DerWriter wr)
        {
            wr.WriteOID(PKCS7Data);
            using (wr.BeginTaggedObject(0))
            {
                using (wr.BeginOctetString())
                {
                    using (wr.BeginSequence())
                    {
                        using (wr.BeginSequence())
                        {
                            InnerDataObject1(p, wr);
                        }
                        using (wr.BeginSequence())
                        {
                            InnerDataObject2(cert, p, wr);
                        }
                    }
                }
            }
        }

        private static void InnerDataObject1(EncryptionMethod p, DerWriter wr)
        {
            wr.WriteOID(PKCS7Data);
            using (wr.BeginTaggedObject(0))
            {
                using (wr.BeginOctetString())
                {
                    using (wr.BeginSequence())
                    {
                        using (wr.BeginSequence())
                        {
                            WriteKeyBag(p, wr);
                        }
                    }
                }
            }
        }

        private static void InnerDataObject2(byte[] cert, EncryptionMethod p, DerWriter wr)
        {
            wr.WriteOID(PKCS7Data);
            using (wr.BeginTaggedObject(0))
            {
                using (wr.BeginOctetString())
                {
                    using (wr.BeginSequence())
                    {
                        using (wr.BeginSequence())
                        {
                            WriteCertBag(cert, p, wr);
                        }
                    }
                }
            }
        }

        private static void WriteCertBag(byte[] cert, EncryptionMethod p, DerWriter wr)
        {
            wr.WriteOID(PKCS12CertBag);
            using (wr.BeginTaggedObject(0))
            {
                using (wr.BeginSequence())
                {
                    WriteX509Cert(cert, p, wr);
                }
            }

            using (wr.BeginSet())
            {
                using (wr.BeginSequence())
                {
                    wr.WriteOID(PKCS12LocalKeyID);
                    using (wr.BeginSet())
                    {
                        wr.WriteOctetString(new byte[] { 1 });
                    }
                }

                using (wr.BeginSequence())
                {
                    wr.WriteOID(PKCS12FriendlyName);
                    using (wr.BeginSet())
                    {
                        wr.Write("Certificate1");
                    }
                }
            }
        }

        private static void WriteX509Cert(byte[] cert, EncryptionMethod p, DerWriter wr)
        {
            wr.WriteOID(PKCS12X509Certificate);
            using (wr.BeginTaggedObject(0))
            {
                wr.WriteOctetString(cert);
            }
        }

        private static void WriteKeyBag(EncryptionMethod p, DerWriter wr)
        {
            wr.WriteOID(PKCS12KeyBag);
            using (wr.BeginTaggedObject(0))
            {
                using (wr.BeginSequence())
                {
                    p.WritePrivateKey(wr);
                }
            }

            using (wr.BeginSet())
            {
                using (wr.BeginSequence())
                {
                    wr.WriteOID(PKCS12LocalKeyID);
                    using (wr.BeginSet())
                    {
                        wr.WriteOctetString(new byte[] { 1 });
                    }
                }

                using (wr.BeginSequence())
                {
                    wr.WriteOID(PKCS12FriendlyName);
                    using (wr.BeginSet())
                    {
                        wr.Write("Certificate1");
                    }
                }
            }
        }


    }
}
