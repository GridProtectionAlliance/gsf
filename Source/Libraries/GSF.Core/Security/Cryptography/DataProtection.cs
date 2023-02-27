//******************************************************************************************************
//  DataProtection.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  02/26/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;

namespace GSF.Security.Cryptography
{
    /// <summary>
    /// Provides methods for encrypting and decrypting data.
    /// </summary>
    /// <remarks>
    /// This is a safety wrapper around the <see cref="ProtectedData"/> class such that it can be used with
    /// <c>LocalMachine</c> scope regardless of current user. This is especially important for applications
    /// that may be running as user account that has no association to the current user, e.g., an Azure AD
    /// user or database account when authenticated using <c>AdoSecurityProvider</c>.
    /// </remarks>
    public class DataProtection
    {
        /// <summary>
        /// Encrypts the data in a specified byte array and returns a byte array that contains the encrypted data.
        /// </summary>
        /// <returns>A byte array representing the encrypted data.</returns>
        /// <param name="userData">A byte array that contains data to encrypt.</param>
        /// <param name="optionalEntropy">An optional additional byte array used to increase the complexity of the encryption, or null for no additional complexity.</param>
        /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="userData"/> parameter is null.</exception>
        /// <exception cref="CryptographicException">The encryption failed.</exception>
        /// <exception cref="NotSupportedException">The operating system does not support this method. </exception>
        /// <exception cref="OutOfMemoryException">The system ran out of memory while encrypting the data.</exception>
        public static byte[] Protect(byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
        {
            if (scope != DataProtectionScope.LocalMachine)
                return ProtectedData.Protect(userData, optionalEntropy, scope);

            IPrincipal principal = Thread.CurrentPrincipal;
            byte[] protectedBytes;

            try
            {
                Thread.CurrentPrincipal = null;
                protectedBytes = ProtectedData.Protect(userData, optionalEntropy, scope);
            }
            finally
            {
                Thread.CurrentPrincipal = principal;
            }

            return protectedBytes;
        }

        /// <summary>
        /// Decrypts the data in a specified byte array and returns a byte array that contains the decrypted data.
        /// </summary>
        /// <returns>A byte array representing the decrypted data.</returns>
        /// <param name="encryptedData">A byte array containing data encrypted using the <see cref="Protect"/> method.</param>
        /// <param name="optionalEntropy">An optional additional byte array that was used to encrypt the data, or null if the additional byte array was not used.</param>
        /// <param name="scope">One of the enumeration values that specifies the scope of data protection that was used to encrypt the data.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="encryptedData"/> parameter is null.</exception>
        /// <exception cref="CryptographicException">The decryption failed.</exception>
        /// <exception cref="NotSupportedException">The operating system does not support this method. </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
        {
            if (scope != DataProtectionScope.LocalMachine)
                return ProtectedData.Unprotect(encryptedData, optionalEntropy, scope);

            IPrincipal principal = Thread.CurrentPrincipal;
            byte[] unprotectedBytes;

            try
            {
                Thread.CurrentPrincipal = null;
                unprotectedBytes = ProtectedData.Unprotect(encryptedData, optionalEntropy, scope);
            }
            finally
            {
                Thread.CurrentPrincipal = principal;
            }

            return unprotectedBytes;
        }
    }
}
