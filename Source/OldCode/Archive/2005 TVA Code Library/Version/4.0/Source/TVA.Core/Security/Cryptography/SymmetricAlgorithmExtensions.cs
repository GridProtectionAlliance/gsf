//*******************************************************************************************************
//  SymmetricAlgorithmExtensions.cs
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
//  09/19/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using TVA.Collections;

namespace TVA.Security.Cryptography
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
            MemoryStream source = new MemoryStream(data, startIndex, length);
            MemoryStream destination = new MemoryStream();

            algorithm.Encrypt(source, destination, key, iv);
            destination.Position = 0;

            return destination.ToArray();
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
            byte[] buffer = new byte[StandardKey.BufferSize];
            CryptoStream encodeStream = new CryptoStream(destination, algorithm.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            int read;

            // Encrypts data onto output stream.
            read = source.Read(buffer, 0, StandardKey.BufferSize);

            while (read > 0)
            {
                encodeStream.Write(buffer, 0, read);
                read = source.Read(buffer, 0, StandardKey.BufferSize);
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
            MemoryStream source = new MemoryStream(data, startIndex, length);
            MemoryStream destination = new MemoryStream();

            algorithm.Decrypt(source, destination, key, iv);
            destination.Position = 0;

            return destination.ToArray();
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
            byte[] buffer = new byte[StandardKey.BufferSize];
            CryptoStream decodeStream = new CryptoStream(destination, algorithm.CreateDecryptor(key, iv), CryptoStreamMode.Write);
            int read;

            // Decrypts data onto output stream.
            read = source.Read(buffer, 0, StandardKey.BufferSize);

            while (read > 0)
            {
                decodeStream.Write(buffer, 0, read);
                read = source.Read(buffer, 0, StandardKey.BufferSize);
            }

            decodeStream.FlushFinalBlock();
        }

        /// <summary>
        /// Coerces given key to maximum legal bit length for given encryption algorithm using a repeatable generation algorithm.
        /// </summary>
        /// <param name="algorithm"><see cref="SymmetricAlgorithm"/> used to determine legal key length.</param>
        /// <param name="key">The source secret key to coerce into a legal bit length.</param>
        /// <returns>A legal sized secret key for given encryption algorithm.</returns>
        public static byte[] GetLegalKey(this SymmetricAlgorithm algorithm, byte[] key)
        {
            List<byte> rgbKey = new List<byte>();
            int length = algorithm.LegalKeySizes[0].MaxSize / 8;

            // Note that we use a repeatable salted injection and randomization sequence such that if user provides
            // a key that is too short, the same short key could be used to decrypt data later.
            for (int x = 0; x < length; x++)
            {
                if (x < key.Length)
                    rgbKey.Add(key[x]);
                else
                    rgbKey.Add(StandardKey.Value[x % StandardKey.Value.Length]);
            }

            rgbKey.Scramble(rgbKey[0]);

            return rgbKey.ToArray();
        }

        /// <summary>
        /// Coerces given initialization vector to legal block size for given encryption algorithm using a repeatable generation algorithm.
        /// </summary>
        /// <param name="algorithm"><see cref="SymmetricAlgorithm"/> used to determine legal initialization vector block size.</param>
        /// <param name="iv">The source initialization vector to coerce into a legal block size.</param>
        /// <returns>A legal sized initialization vector for given encryption algorithm.</returns>
        public static byte[] GetLegalIV(this SymmetricAlgorithm algorithm, byte[] iv)
        {
            List<byte> rgbIV = new List<byte>();
            int length = algorithm.LegalBlockSizes[0].MinSize / 8;

            // Note that we use a repeatable salted injection and randomization sequence such that if user provides
            // a key that is too short, the same short key could be used to decrypt data later.
            for (int x = 0; x < length; x++)
            {
                if (x < iv.Length)
                    rgbIV.Add(iv[iv.Length - 1 - x]);
                else
                    rgbIV.Add(StandardKey.Value[x % StandardKey.Value.Length]);
            }

            rgbIV.Scramble(rgbIV[0]);

            return rgbIV.ToArray();
        }
    }
}