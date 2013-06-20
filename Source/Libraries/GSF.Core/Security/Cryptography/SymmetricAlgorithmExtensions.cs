//******************************************************************************************************
//  SymmetricAlgorithmExtensions.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  09/19/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.IO;
using System.Security.Cryptography;
using GSF.IO;

namespace GSF.Security.Cryptography
{
    /// <summary>
    /// Defines extension functions related to cryptographic <see cref="SymmetricAlgorithm"/> objects.
    /// </summary>
    public static class SymmetricAlgorithmExtensions
    {
        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="algorithm"><see cref="SymmetricAlgorithm"/> to use for encryption.</param>
        /// <param name="data">Source buffer containing data to encrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="data"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="data"/> buffer to encrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">The secret key to use for the symmetric algorithm.</param>
        /// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>Encrypted version of <paramref name="data"/> buffer.</returns>
        public static byte[] Encrypt(this SymmetricAlgorithm algorithm, byte[] data, int startIndex, int length, byte[] key, byte[] iv)
        {
            // Fastest to use existing buffer in non-expandable memory stream for source and large block allocated memory stream for destination
            using (MemoryStream source = new MemoryStream(data, startIndex, length))
            using (BlockAllocatedMemoryStream destination = new BlockAllocatedMemoryStream())
            {
                algorithm.Encrypt(source, destination, key, iv);
                return destination.ToArray();
            }
        }

        /// <summary>
        /// Encrypts input stream onto output stream for the given parameters.
        /// </summary>
        /// <param name="algorithm"><see cref="SymmetricAlgorithm"/> to use for encryption.</param>
        /// <param name="source">Source stream that contains data to encrypt.</param>
        /// <param name="destination">Destination stream used to hold encrypted data.</param>
        /// <param name="key">The secret key to use for the symmetric algorithm.</param>
        /// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
        public static void Encrypt(this SymmetricAlgorithm algorithm, Stream source, Stream destination, byte[] key, byte[] iv)
        {
            byte[] buffer = new byte[Standard.BufferSize];
            CryptoStream encodeStream = new CryptoStream(destination, algorithm.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            int read;

            // Encrypts data onto output stream.
            read = source.Read(buffer, 0, Standard.BufferSize);

            while (read > 0)
            {
                encodeStream.Write(buffer, 0, read);
                read = source.Read(buffer, 0, Standard.BufferSize);
            }

            encodeStream.FlushFinalBlock();
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="algorithm"><see cref="SymmetricAlgorithm"/> to use for decryption.</param>
        /// <param name="data">Source buffer containing data to decrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="data"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="data"/> buffer to decrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">The secret key to use for the symmetric algorithm.</param>
        /// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>Decrypted version of <paramref name="data"/> buffer.</returns>
        public static byte[] Decrypt(this SymmetricAlgorithm algorithm, byte[] data, int startIndex, int length, byte[] key, byte[] iv)
        {
            // Fastest to use existing buffer in non-expandable memory stream for source and large block allocated memory stream for destination
            using (MemoryStream source = new MemoryStream(data, startIndex, length))
            using (BlockAllocatedMemoryStream destination = new BlockAllocatedMemoryStream())
            {
                algorithm.Decrypt(source, destination, key, iv);
                return destination.ToArray();
            }
        }

        /// <summary>
        /// Decrypts input stream onto output stream for the given parameters.
        /// </summary>
        /// <param name="algorithm"><see cref="SymmetricAlgorithm"/> to use for decryption.</param>
        /// <param name="source">Source stream that contains data to decrypt.</param>
        /// <param name="destination">Destination stream used to hold decrypted data.</param>
        /// <param name="key">The secret key to use for the symmetric algorithm.</param>
        /// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
        public static void Decrypt(this SymmetricAlgorithm algorithm, Stream source, Stream destination, byte[] key, byte[] iv)
        {
            byte[] buffer = new byte[Standard.BufferSize];
            CryptoStream decodeStream = new CryptoStream(destination, algorithm.CreateDecryptor(key, iv), CryptoStreamMode.Write);
            int read;

            // Decrypts data onto output stream.
            read = source.Read(buffer, 0, Standard.BufferSize);

            while (read > 0)
            {
                decodeStream.Write(buffer, 0, read);
                read = source.Read(buffer, 0, Standard.BufferSize);
            }

            decodeStream.FlushFinalBlock();
        }
    }
}