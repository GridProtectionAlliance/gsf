//******************************************************************************************************
//  Certificate.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/8/2012 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace OGE.Core.Security.CertificateGenerator
{
    //http://stackoverflow.com/questions/3770233/is-it-possible-to-programmatically-generate-an-x509-certificate-using-only-c/3771913#3771913
    //http://stackoverflow.com/questions/22230745/generate-self-signed-certificate-on-the-fly
    //https://blog.differentpla.net/post/53/how-do-i-create-a-self-signed-certificate-using-bouncy-castle-
    //https://blog.differentpla.net/post/20/how-do-i-convert-a-bouncy-castle-certificate-to-a-net-certificate-
    //http://www.fkollmann.de/v2/post/Creating-certificates-using-BouncyCastle.aspx

    public static class CertificateMaker
    {

        //public static string CreatePassword(string origionalPassword, int iterationCount)
        //{
        //    var sha1 = new SHA1Managed();
        //    Rfc2898DeriveBytes generateBytes = new Rfc2898DeriveBytes(origionalPassword, sha1.ComputeHash(Encoding.Unicode.GetBytes(origionalPassword)), iterationCount);
        //    return Convert.ToBase64String(generateBytes.GetBytes(32)).Substring(0, 30);
        //}

        //public static X509Certificate2 Create(string issuer, string[] subjectNames, int strength)
        //{
        //    var cert = GenerateSelfSignedCertificate(issuer);

        //    //add CA cert to store
        //    addCertToStore(cert, StoreName.My, StoreLocation.CurrentUser);
        //    return cert;
        //}

        /// <summary>
        /// Creates a certificate authority certificate.
        /// </summary>
        /// <returns></returns>
        public static X509Certificate2 GenerateSelfSignedCertificate(string subjectDirName, DateTime startDate, DateTime endDate, short signatureBits, int keyStrength)
        {
            string signatureAlgorithm;
            switch (signatureBits)
            {
                case 160:
                    signatureAlgorithm = "SHA1withRSA";
                    break;
                case 224:
                    signatureAlgorithm = "SHA224withRSA";
                    break;
                case 256:
                    signatureAlgorithm = "SHA256withRSA";
                    break;
                case 384:
                    signatureAlgorithm = "SHA384withRSA";
                    break;
                case 512:
                    signatureAlgorithm = "SHA512withRSA";
                    break;
                default:
                    throw new ArgumentException("Invalid signature bit size.", "signatureBits");
            }

            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            // Generate public/private keys.
            AsymmetricCipherKeyPair encryptionKeys;

            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            encryptionKeys = keyPairGenerator.GenerateKeyPair();

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            certificateGenerator.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random));
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);
            certificateGenerator.SetIssuerDN(new X509Name(subjectDirName));
            certificateGenerator.SetSubjectDN(new X509Name(subjectDirName));
            certificateGenerator.SetNotBefore(startDate);
            certificateGenerator.SetNotAfter(endDate);
            certificateGenerator.SetPublicKey(encryptionKeys.Public);

            // selfsign certificate
            X509Certificate certificate = certificateGenerator.Generate(encryptionKeys.Private, random);

            var store = new Pkcs12Store();
            string friendlyName = certificate.SubjectDN.ToString();
            var certificateEntry = new X509CertificateEntry(certificate);
            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(encryptionKeys.Private), new[] { certificateEntry });

            var stream = new MemoryStream();
            store.Save(stream, "SuPerS3cr#2Pawd!".ToCharArray(), random);

            //Verify that the certificate is valid.
            return new X509Certificate2(stream.ToArray(), "SuPerS3cr#2Pawd!", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        }

        /// <summary>
        /// Creates a certificate authority certificate.
        /// </summary>
        /// <returns></returns>
        public static void GenerateSelfSignedCertificate(string subjectDirName, DateTime startDate, DateTime endDate, short signatureBits, int keyStrength, string password, string fileName)
        {
            string signatureAlgorithm;
            switch (signatureBits)
            {
                case 160:
                    signatureAlgorithm = "SHA1withRSA";
                    break;
                case 224:
                    signatureAlgorithm = "SHA224withRSA";
                    break;
                case 256:
                    signatureAlgorithm = "SHA256withRSA";
                    break;
                case 384:
                    signatureAlgorithm = "SHA384withRSA";
                    break;
                case 512:
                    signatureAlgorithm = "SHA512withRSA";
                    break;
                default:
                    throw new ArgumentException("Invalid signature bit size.", "signatureBits");
            }

            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            // Generate public/private keys.
            AsymmetricCipherKeyPair encryptionKeys;

            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            encryptionKeys = keyPairGenerator.GenerateKeyPair();

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            certificateGenerator.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random));
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);
            certificateGenerator.SetIssuerDN(new X509Name(subjectDirName));
            certificateGenerator.SetSubjectDN(new X509Name(subjectDirName));
            certificateGenerator.SetNotBefore(startDate);
            certificateGenerator.SetNotAfter(endDate);
            certificateGenerator.SetPublicKey(encryptionKeys.Public);

            // selfsign certificate
            X509Certificate certificate = certificateGenerator.Generate(encryptionKeys.Private, random);

            var store = new Pkcs12Store();
            string friendlyName = certificate.SubjectDN.ToString();
            var certificateEntry = new X509CertificateEntry(certificate);
            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(encryptionKeys.Private), new[] { certificateEntry });

            var stream = new MemoryStream();
            store.Save(stream, password.ToCharArray(), random);

            //Verify that the certificate is valid.
            var convertedCertificate = new X509Certificate2(stream.ToArray(), password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            //Write the file.
            File.WriteAllBytes(fileName, stream.ToArray());

            File.WriteAllBytes(Path.ChangeExtension(fileName, ".cer"), certificate.GetEncoded());
        }

        public static void GenerateSignedCertificate(X509Certificate2 caCertificate, string subjectDirName, DateTime startDate, DateTime endDate, int signatureBits, int keyStrength, string password, string fileName)
        {
            string signatureAlgorithm;
            switch (signatureBits)
            {
                case 160:
                    signatureAlgorithm = "SHA1withRSA";
                    break;
                case 224:
                    signatureAlgorithm = "SHA224withRSA";
                    break;
                case 256:
                    signatureAlgorithm = "SHA256withRSA";
                    break;
                case 384:
                    signatureAlgorithm = "SHA384withRSA";
                    break;
                case 512:
                    signatureAlgorithm = "SHA512withRSA";
                    break;
                default:
                    throw new ArgumentException("Invalid signature bit size.", "signatureBits");
            }

            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            // Generate public/private keys.
            AsymmetricCipherKeyPair encryptionKeys;

            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            encryptionKeys = keyPairGenerator.GenerateKeyPair();

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            certificateGenerator.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random));
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);
            certificateGenerator.SetIssuerDN(new X509Name(subjectDirName));
            certificateGenerator.SetSubjectDN(new X509Name(subjectDirName));
            certificateGenerator.SetNotBefore(startDate);
            certificateGenerator.SetNotAfter(endDate);
            certificateGenerator.SetPublicKey(encryptionKeys.Public);

            var parameters = DotNetUtilities.GetRsaKeyPair(caCertificate.PrivateKey as RSA);
            var parameters2 = DotNetUtilities.GetRsaKeyPair((caCertificate.PrivateKey as RSA).ExportParameters(true));

            var cert = caCertificate.PrivateKey as RSACryptoServiceProvider;
            var cert2 = DotNetUtilities.FromX509Certificate(caCertificate);

            // selfsign certificate
            X509Certificate certificate = certificateGenerator.Generate(parameters.Private, random);

            var store = new Pkcs12Store();
            string friendlyName = certificate.SubjectDN.ToString();
            var certificateEntry = new X509CertificateEntry(certificate);
            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(encryptionKeys.Private), new[] { certificateEntry });

            var stream = new MemoryStream();
            store.Save(stream, password.ToCharArray(), random);

            //Verify that the certificate is valid.
            var convertedCertificate = new X509Certificate2(stream.ToArray(), password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            //Write the file.
            File.WriteAllBytes(fileName, stream.ToArray());
        }

        //public static X509Certificate2 OpenCertificate(string fileName, string password)
        //{
        //    return new X509Certificate2(fileName, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        //}

        //private static bool addCertToStore(X509Certificate2 cert, StoreName st, StoreLocation sl)
        //{
        //    bool bRet = false;

        //    try
        //    {
        //        X509Store store = new X509Store(st, sl);
        //        store.Open(OpenFlags.ReadWrite);
        //        store.Add(cert);

        //        store.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Console.WriteLine(ex.ToString());
        //    }

        //    return bRet;
        //}

    }

}