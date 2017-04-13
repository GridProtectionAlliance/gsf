//******************************************************************************************************
//  Cipher.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/11/2017 - Ritchie Carroll
//       Generated partial version of class in GSF.Shared.
//
//******************************************************************************************************

using System.Security.Cryptography;
using Microsoft.Win32;

namespace GSF.Security.Cryptography
{
    /// <summary>
    /// Provides general use cryptographic functions.
    /// </summary>
    /// <remarks>
    /// This class exists to simplify usage of basic cryptography functionality.
    /// </remarks>
    public static partial class Cipher
    {
        // Switch to turn off managed encryption and use wrappers over FIPS-compliant algorithms.
        private static readonly bool s_managedEncryption;
        
        /// <summary>
        /// Gets a flag that determines if system will allow use of managed, i.e., non-FIPS compliant, security algorithms.
        /// </summary>
        public static bool SystemAllowsManagedEncryption => s_managedEncryption;

        /// <summary>
        /// Static constructor for the <see cref="Cipher"/> class.
        /// </summary>
        static Cipher()
        {
#if MONO
            // Common .NET FIPS wrapper algorithms are implemented as managed code under Mono, check status of Crimson project
            s_managedEncryption = true;
#else
            const string fipsKeyOld = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa";
            const string fipsKeyNew = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa\\FipsAlgorithmPolicy";

            // Determine if the user needs to use FIPS-compliant algorithms
            try
            {
                s_managedEncryption = (Registry.GetValue(fipsKeyNew, "Enabled", 0) ?? Registry.GetValue(fipsKeyOld, "FIPSAlgorithmPolicy", 0)).ToString() == "0";
            }
            catch
            {
                s_managedEncryption = true;
            }
#endif
            // Continue with any needed static construction activities
            OnCreated();
        }

        static partial void OnCreated();

        /// <summary>
        /// Creates a <see cref="SHA1"/> hashing algorithm that respects current FIPS setting.
        /// </summary>
        /// <returns>New <see cref="SHA1"/> hashing algorithm that respects current FIPS setting.</returns>
        public static SHA1 CreateSHA1()
        {
            return s_managedEncryption ? new SHA1Managed() : new SHA1CryptoServiceProvider() as SHA1;
        }

        /// <summary>
        /// Creates a <see cref="SHA256"/> hashing algorithm that respects current FIPS setting.
        /// </summary>
        /// <returns>New <see cref="SHA256"/> hashing algorithm that respects current FIPS setting.</returns>
        public static SHA256 CreateSHA256()
        {
            return s_managedEncryption ? new SHA256Managed() : new SHA256CryptoServiceProvider() as SHA256;
        }

        /// <summary>
        /// Creates a <see cref="SHA384"/> hashing algorithm that respects current FIPS setting.
        /// </summary>
        /// <returns>New <see cref="SHA384"/> hashing algorithm that respects current FIPS setting.</returns>
        public static SHA384 CreateSHA384()
        {
            return s_managedEncryption ? new SHA384Managed() : new SHA384CryptoServiceProvider() as SHA384;
        }

        /// <summary>
        /// Creates a <see cref="SHA512"/> hashing algorithm that respects current FIPS setting.
        /// </summary>
        /// <returns>New <see cref="SHA512"/> hashing algorithm that respects current FIPS setting.</returns>
        public static SHA512 CreateSHA512()
        {
            return s_managedEncryption ? new SHA512Managed() : new SHA512CryptoServiceProvider() as SHA512;
        }

        /// <summary>
        /// Creates an <see cref="Aes"/> encryption algorithm that respects current FIPS setting.
        /// </summary>
        /// <returns>New <see cref="Aes"/> encryption algorithm that respects current FIPS setting.</returns>
        public static Aes CreateAes()
        {
            return s_managedEncryption ? new AesManaged() : new AesCryptoServiceProvider() as Aes;
        }
    }
}