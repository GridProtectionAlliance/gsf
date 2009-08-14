//*******************************************************************************************************
//  Cipher.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2003 - James R. Carroll
//       Generated original version of source code.
//  01/04/2006 - James R. Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Crypto).
//  02/28/2007 - James R. Carroll
//       Changed string-based encrypt and decrypt functions to return null if
//       input string to be encrypted or decrypted was null or empty.
//  10/11/2007 - James R. Carroll
//       Added Obfuscate and Deobfuscate functions that perform data obfuscation
//       based upon simple bit-rotation algorithms.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/19/2008 - James R. Carroll
//       Converted to C# - basic encryption/decryption extends string, byte[], and Stream.
//  08/10/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TVA.Collections;

namespace TVA.Security.Cryptography
{
    #region [ Enumerations ]

    /// <summary>
    /// Cryptographic strength enumeration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Encryption algorithms are cumulative. The levels represent tradeoffs on speed vs. cipher strength. Level 1
    /// will have the fastest encryption speed with the weakest encryption strength; level 6 will have the
    /// strongest cumulative encryption strength with the slowest encryption speed.
    /// </para>
    /// </remarks>
    public enum CipherStrength
    {
        /// <summary>Uses no encryption.</summary>
        None,
        /// <summary>Adds simple multi-alogorithm XOR based encryption.</summary>
        /// <remarks>This is a proprietary encryption algorithm.</remarks>
        Level1,
        /// <summary>Adds TripleDES based encryption.</summary>
        /// <remarks>This is a standard encryption algorithm.</remarks>
        Level2,
        /// <summary>Adds RC2 based encryption.</summary>
        /// <remarks>This is a standard encryption algorithm.</remarks>
        Level3,
        /// <summary>Adds RijndaelManaged based encryption.</summary>
        /// <remarks>This is a standard encryption algorithm.</remarks>
        Level4,
        /// <summary>Adds simple bit-rotation based encryption.</summary>
        /// <remarks>This is a proprietary encryption algorithm.</remarks>
        Level5,
        /// <summary>Adds classic pkzip based encryption.</summary>
        /// <remarks>This is a common encryption algorithm.</remarks>
        Level6
    }

    #endregion

    /// <summary>
    /// Provides general use cryptographic functions.
    /// </summary>
    /// <remarks>
    /// This class exists to simplify usage of basic cryptography functionality. It uses a combination of standard and
    /// proprietary encryption algorithms to produce decent obfuscations of strings, buffers and streams of data.
    /// </remarks>
    public static class Cipher
    {
        private static List<int> m_excludedCharacters;

        /// <summary>
        /// Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        /// the given parameter, using the standard encryption key and encryption level 1.
        /// </summary>
        /// <param name="source">Source string to encrypt.</param>
        /// <returns>An encrypted version of the source string.</returns>
        public static string Encrypt(this string source)
        {
            return source.Encrypt(null, CipherStrength.Level1);
        }

        /// <summary>
        /// Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        /// the given parameters using standard encryption.
        /// </summary>
        /// <param name="source">Source string to encrypt.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting string.</param>
        /// <returns>An encrypted version of the source string.</returns>
        public static string Encrypt(this string source, CipherStrength strength)
        {
            return source.Encrypt(null, strength);
        }

        /// <summary>
        /// Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        /// the given parameters.
        /// </summary>
        /// <param name="source">Source string to encrypt.</param>
        /// <param name="key">Encryption key to use to encrypt string.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting string.</param>
        /// <returns>An encrypted version of the source string.</returns>
        public static string Encrypt(this string source, string key, CipherStrength strength)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            if (string.IsNullOrEmpty(key))
                key = Standard.Source;

            byte[] rgbKey = GetBinaryKeyFromString(key);

            return Convert.ToBase64String(Encoding.Unicode.GetBytes(source).Encrypt(rgbKey, rgbKey, strength));
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="key">Encryption key to use to encrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, byte[] key, CipherStrength strength)
        {
            return source.Encrypt(0, source.Length, key, key, strength);
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="key">Encryption key to use to encrypt data.</param>
        /// <param name="iv">Initialization vector to use to encrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, byte[] key, byte[] iv, CipherStrength strength)
        {
            return source.Encrypt(0, source.Length, key, iv, strength);
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to encrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">Encryption key to use to encrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, int startIndex, int length, byte[] key, CipherStrength strength)
        {
            return source.Encrypt(startIndex, length, key, key, strength);
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to encrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">Encryption key to use to encrypt data.</param>
        /// <param name="iv">Initialization vector to use to encrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, int startIndex, int length, byte[] key, byte[] iv, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            // Performs requested levels of encryption.
            source = Crypt(source, startIndex, length, key);
            if (strength >= CipherStrength.Level2)
            {
                source = LegalKeyEncrypt(new TripleDESCryptoServiceProvider(), source, 0, source.Length, key, iv);
                if (strength >= CipherStrength.Level3)
                {
                    source = LegalKeyEncrypt(new RC2CryptoServiceProvider(), source, 0, source.Length, key, iv);
                    if (strength >= CipherStrength.Level4)
                    {
                        source = LegalKeyEncrypt(new RijndaelManaged(), source, 0, source.Length, key, iv);
                        if (strength >= CipherStrength.Level5)
                        {
                            source = Obfuscate(source, 0, source.Length, key);
                            if (strength >= CipherStrength.Level6)
                            {
                                source = LegalKeyEncrypt(new Pkzip.PkzipClassicManaged(), source, 0, source.Length, key, iv);
                            }
                        }
                    }
                }
            }

            return source;
        }

        // Coerce key and iv into legal format for specificed algorithm then encrypt
        private static byte[] LegalKeyEncrypt(SymmetricAlgorithm algorithm, byte[] data, int startIndex, int length, byte[] key, byte[] iv)
        {
            byte[] rgbKey = algorithm.GetLegalKey(key);
            byte[] rgbIV = algorithm.GetLegalIV(iv);
            return algorithm.Encrypt(data, startIndex, length, rgbKey, rgbIV);
        }

        /// <summary>
        /// Returns a stream of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to encrypt.</param>
        /// <param name="key">Encryption key to use to encrypt stream.</param>
        /// <param name="iv">Initialization vector to use to encrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting stream.</param>
        /// <returns>An encrypted version of the source stream.</returns>
        /// <remarks>
        /// This returns a memory stream of the encrypted results, if the incoming stream is
        /// very large this will consume a large amount of memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Encrypt(this Stream source, byte[] key, byte[] iv, CipherStrength strength)
        {
            MemoryStream destination = new MemoryStream();

            source.Encrypt(destination, key, iv, strength, null);
            destination.Position = 0;

            return destination;
        }

        /// <summary>
        /// Encrypts input stream onto output stream for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to encrypt.</param>
        /// <param name="destination">Destination stream used to hold encrypted data.</param>
        /// <param name="key">Encryption key to use to encrypt stream.</param>
        /// <param name="iv">Initialization vector to use to encrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting stream.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for encrypting large streams.</param>
        public static void Encrypt(this Stream source, Stream destination, byte[] key, byte[] iv, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer = new byte[Standard.BufferSize];
            byte[] outBuffer, lengthBuffer;
            long total = 0;
            long length = -1;
            int read;

            // Sends initial progress event.
            if (progressHandler != null)
            {
                try
                {
                    if (source.CanSeek)
                        length = source.Length;
                }
                catch
                {
                    length = -1;
                }

                // Create a new progress handler to track encryption progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Encrypt", length);
                progress.Complete = 0;
            }

            // Reads initial buffer.
            read = source.Read(inBuffer, 0, Standard.BufferSize);

            while (read > 0)
            {
                // Encrypts buffer.
                outBuffer = inBuffer.BlockCopy(0, read).Encrypt(key, iv, strength);

                // The destination encryption stream length does not have to be same as the input stream length, so we
                // prepend the final size of each encrypted buffer onto the destination ouput stream so that we can
                // safely decrypt the stream in a "chunked" fashion later.
                lengthBuffer = BitConverter.GetBytes(outBuffer.Length);
                destination.Write(lengthBuffer, 0, lengthBuffer.Length);
                destination.Write(outBuffer, 0, outBuffer.Length);

                // Updates encryption progress.
                if (progressHandler != null)
                {
                    total += read;
                    progress.Complete = total;
                }

                // Reads next buffer.
                read = source.Read(inBuffer, 0, Standard.BufferSize);
            }
        }

        /// <summary>
        /// Creates an encrypted file from source file data.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting file.</param>
        public static void EncryptFile(string sourceFileName, string destinationFileName, CipherStrength strength)
        {
            EncryptFile(sourceFileName, destinationFileName, null, strength, null);
        }

        /// <summary>
        /// Creates an encrypted file from source file data.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="key">Encryption key to use to encrypt file.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting file.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for encrypting large files.</param>
        public static void EncryptFile(string sourceFileName, string destinationFileName, string key, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFileName);

            if (string.IsNullOrEmpty(key))
                key = Standard.Source;

            byte[] rgbKey = GetBinaryKeyFromString(key);

            sourceFileStream.Encrypt(destFileStream, rgbKey, rgbKey, strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }

        /// <summary>
        /// Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        /// parameter using the standard encryption key and encryption level 1.
        /// </summary>
        /// <param name="source">Source string to decrypt.</param>
        /// <returns>A decrypted version of the source string.</returns>
        public static string Decrypt(this string source)
        {
            return source.Decrypt(null, CipherStrength.Level1);
        }

        /// <summary>
        /// Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        /// parameters using the standard encryption key.
        /// </summary>
        /// <param name="source">Source string to decrypt.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting string.</param>
        /// <returns>A decrypted version of the source string.</returns>
        public static string Decrypt(this string source, CipherStrength strength)
        {
            return source.Decrypt(null, strength);
        }

        /// <summary>
        /// Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        /// parameters.
        /// </summary>
        /// <param name="source">Source string to decrypt.</param>
        /// <param name="key">Encryption key to use to decrypt string.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting string.</param>
        /// <returns>A decrypted version of the source string.</returns>
        public static string Decrypt(this string source, string key, CipherStrength strength)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            if (string.IsNullOrEmpty(key))
                key = Standard.Source;

            byte[] rgbKey = GetBinaryKeyFromString(key);

            return Encoding.Unicode.GetString(Convert.FromBase64String(source).Decrypt(rgbKey, rgbKey, strength));
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="key">Encryption key to use to decrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, byte[] key, CipherStrength strength)
        {
            return source.Decrypt(0, source.Length, key, key, strength);
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="key">Encryption key to use to decrypt data.</param>
        /// <param name="iv">Initialization vector to use to decrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, byte[] key, byte[] iv, CipherStrength strength)
        {
            return source.Decrypt(0, source.Length, key, iv, strength);
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to decrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">Encryption key to use to decrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, int startIndex, int length, byte[] key, CipherStrength strength)
        {
            return source.Decrypt(startIndex, length, key, key, strength);
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to decrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">Encryption key to use to decrypt data.</param>
        /// <param name="iv">Initialization vector to use to decrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, int startIndex, int length, byte[] key, byte[] iv, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            // Performs requested levels of decryption.
            if (strength >= CipherStrength.Level6)
            {
                source = LegalKeyDecrypt(new Pkzip.PkzipClassicManaged(), source, startIndex, length, key, iv);
                startIndex = 0;
                length = source.Length;
            }

            if (strength >= CipherStrength.Level5)
            {
                source = Deobfuscate(source, startIndex, length, key);
                startIndex = 0;
                length = source.Length;
            }

            if (strength >= CipherStrength.Level4)
            {
                source = LegalKeyDecrypt(new RijndaelManaged(), source, startIndex, length, key, iv);
                startIndex = 0;
                length = source.Length;
            }

            if (strength >= CipherStrength.Level3)
            {
                source = LegalKeyDecrypt(new RC2CryptoServiceProvider(), source, startIndex, length, key, iv);
                startIndex = 0;
                length = source.Length;
            }

            if (strength >= CipherStrength.Level2)
            {
                source = LegalKeyDecrypt(new TripleDESCryptoServiceProvider(), source, startIndex, length, key, iv);
                startIndex = 0;
                length = source.Length;
            }

            return Crypt(source, startIndex, length, key);
        }

        // Coerce key and iv into legal format for specificed algorithm then decrypt
        private static byte[] LegalKeyDecrypt(SymmetricAlgorithm algorithm, byte[] data, int startIndex, int length, byte[] key, byte[] iv)
        {
            byte[] rgbKey = algorithm.GetLegalKey(key);
            byte[] rgbIV = algorithm.GetLegalIV(iv);
            return algorithm.Decrypt(data, startIndex, length, rgbKey, rgbIV);
        }

        /// <summary>
        /// Returns a stream of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to decrypt.</param>
        /// <param name="key">Encryption key to use to decrypt stream.</param>
        /// <param name="iv">Initialization vector to use to decrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting stream.</param>
        /// <returns>A decrypted version of the source stream.</returns>
        /// <remarks>
        /// This returns a memory stream of the decrypted results, if the incoming stream is
        /// very large this will consume a large amount of memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Decrypt(this Stream source, byte[] key, byte[] iv, CipherStrength strength)
        {
            MemoryStream destination = new MemoryStream();

            source.Decrypt(destination, key, iv, strength, null);
            destination.Position = 0;

            return destination;
        }

        /// <summary>
        /// Decrypts input stream onto output stream for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to decrypt.</param>
        /// <param name="destination">Destination stream used to hold decrypted data.</param>
        /// <param name="key">Encryption key to use to decrypt stream.</param>
        /// <param name="iv">Initialization vector to use to decrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting stream.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for decrypting large streams.</param>
        public static void Decrypt(this Stream source, Stream destination, byte[] key, byte[] iv, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer, outBuffer;
            byte[] lengthBuffer = BitConverter.GetBytes((int)0);
            long total = 0;
            long length = -1;
            int size, read;

            // Sends initial progress event.
            if (progressHandler != null)
            {
                try
                {
                    if (source.CanSeek)
                        length = source.Length;
                }
                catch
                {
                    length = -1;
                }

                // Create a new progress handler to track decryption progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Decrypt", length);
                progress.Complete = 0;
            }

            // When the source stream was encrypted, it was known that the encrypted stream length did not have to be same as
            // the input stream length. We prepended the final size of the each encrypted buffer onto the destination
            // ouput stream (now the input stream to this function), so that we could safely decrypt the stream in a
            // "chunked" fashion, hence the following:

            // Reads the size of the next buffer from the stream.
            read = source.Read(lengthBuffer, 0, lengthBuffer.Length);

            while (read > 0)
            {
                // Converts the byte array containing the buffer size into an integer.
                size = BitConverter.ToInt32(lengthBuffer, 0);

                if (size > 0)
                {
                    // Creates and reads the next buffer.
                    inBuffer = new byte[size];
                    read = source.Read(inBuffer, 0, size);

                    if (read > 0)
                    {
                        // Decrypts buffer.
                        outBuffer = inBuffer.Decrypt(key, iv, strength);
                        destination.Write(outBuffer, 0, outBuffer.Length);

                        // Updates decryption progress.
                        if (progressHandler != null)
                        {
                            total += (read + lengthBuffer.Length);
                            progress.Complete = total;
                        }
                    }
                }

                // Reads the size of the next buffer from the stream.
                read = source.Read(lengthBuffer, 0, lengthBuffer.Length);
            }
        }

        /// <summary>
        /// Creates a decrypted file from source file data.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting file.</param>
        public static void DecryptFile(string sourceFileName, string destinationFileName, CipherStrength strength)
        {
            DecryptFile(sourceFileName, destinationFileName, null, strength, null);
        }

        /// <summary>
        /// Creates a decrypted file from source file data.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="key">Encryption key to use to decrypt file.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting file.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for decrypting large files.</param>
        public static void DecryptFile(string sourceFileName, string destinationFileName, string key, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFileName);

            if (string.IsNullOrEmpty(key))
                key = Standard.Source;

            byte[] rgbKey = GetBinaryKeyFromString(key);

            sourceFileStream.Decrypt(destFileStream, rgbKey, rgbKey, strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }

        /// <summary>
        /// Generates a binary encryption key from given string.
        /// </summary>
        /// <param name="encryptionKey">String based sequence to convert to an encryption key.</param>
        /// <returns>Binary encryption key from given string.</returns>
        public static byte[] GetBinaryKeyFromString(string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentException("Argument is null or empty.", "encryptionKey");

            List<byte> key = new List<byte>();
            ushort keyValue;

            for (int i = 0; i < encryptionKey.Length; i++)
            {
                keyValue = (ushort)encryptionKey[i];

                // Get all the relevant bytes from the Unicode character
                if (keyValue < 256)
                    key.Add((byte)keyValue);
                else
                {
                    key.Add(keyValue.LowByte());
                    key.Add(keyValue.HighByte());
                }
            }

            // Perform repeatable scramble on key sequence
            key.Scramble((int)encryptionKey[0]);

            return key.ToArray();
        }

        /// <summary>
        /// Generates a random key useful for cryptographic functions.
        /// </summary>
        /// <param name="size">Length of key to generate.</param>
        /// <returns>A <see cref="String"/> random key.</returns>
        public static string GenerateKey(int size)
        {
            // Generates a character array of unique values.
            List<char> keyChars = new List<char>();

            for (int x = 0; x < size; x++)
            {
                keyChars.Add((char)Random.Int32Between(1, 255));
            }

            return new string(keyChars.ToArray());
        }

        /// <summary>
        /// Generates a text based random key with no control characters useful for cryptographic functions.
        /// </summary>
        /// <param name="size">Length of key to generate.</param>
        /// <returns>A <see cref="String"/> based random key.</returns>
        public static string GenerateTextFriendlyKey(int size)
        {
            // Initialize excluded characters
            if (m_excludedCharacters == null)
            {
                m_excludedCharacters = new List<int>();

                m_excludedCharacters.Add(0x7F);

                for (int i = 0x81; i < 0xA0; i++)
                {
                    m_excludedCharacters.Add(i);
                }
            }

            // Generates a character array of unique text friendly values.
            List<char> keyChars = new List<char>();
            int character;

            for (int x = 0; x < size; x++)
            {
                do
                {
                    character = Random.Int32Between(33, 255);
                }
                while (m_excludedCharacters.BinarySearch(character) >= 0);

                keyChars.Add((char)character);
            }

            return new string(keyChars.ToArray());
        }

        /// <summary>
        /// Generates a text based random key that can be easily typed useful for cryptographic functions.
        /// </summary>
        /// <param name="size">Length of key to generate.</param>
        /// <returns>A <see cref="String"/> based random key.</returns>
        public static string GenerateKeyboardFriendlyKey(int size)
        {
            // Generates a character array of unique easily typeable values.
            List<char> keyChars = new List<char>();
            char character;

            for (int x = 0; x < size / 2; x++)
            {
                // Add a number (0 - 9)
                keyChars.Add((char)Random.Int32Between(0x30, 0x39));

                // Add a letter (A - Z)
                character = (char)Random.Int32Between(0x41, 0x5A);

                // Make every other letter lower case
                if (x % 2 == 0)
                    keyChars.Add(character);
                else
                    keyChars.Add(char.ToLower(character));
            }

            // Make sure length comes out as specified
            if (size % 2 != 0)
                keyChars.Add((char)Random.Int32Between(0x30, 0x39));


            // Mix up letters and numbers randomly
            keyChars.Scramble();

            return new string(keyChars.ToArray());
        }

        /// <summary>
        /// Returns an encrypted or decrypted stream using XOR based algorithms. Call once to
        /// encrypt, call again with same key to decrypt.
        /// </summary>
        /// <param name="source">Source stream to encrypt or decrypt.</param>
        /// <param name="encryptionKey">Encryption key to use to encrypt or decrypt stream.</param>
        /// <returns>An encrypted or decrypted version of source stream.</returns>
        /// <remarks>
        /// This returns a memory stream of the encrypted or decrypted results, if the incoming
        /// stream is very large this will consume a large amount of memory.  In this case use the
        /// overload that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Crypt(Stream source, byte[] encryptionKey)
        {
            MemoryStream destination = new MemoryStream();

            Crypt(source, destination, encryptionKey);
            destination.Position = 0;

            return destination;
        }

        /// <summary>
        /// Encrypts or decrypts input stream onto output stream using XOR based algorithms. Call once to
        /// encrypt, call again with same key to decrypt.
        /// </summary>
        /// <param name="source">Source stream to encrypt or decrypt.</param>
        /// <param name="destination">Destination stream to hold results..</param>
        /// <param name="encryptionKey">Encryption key to use to encrypt or decrypt stream.</param>
        public static void Crypt(Stream source, Stream destination, byte[] encryptionKey)
        {
            byte[] buffer = new byte[Standard.BufferSize], results;
            int bytesRead = source.Read(buffer, 0, Standard.BufferSize);

            while (bytesRead > 0)
            {
                results = Crypt(buffer, 0, bytesRead, encryptionKey);
                destination.Write(results, 0, results.Length);
                bytesRead = source.Read(buffer, 0, Standard.BufferSize);
            }
        }

        /// <summary>
        /// Encrypts or decrypts data using XOR based algorithms. Call once to encrypt; call again with same key to decrypt.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt or decrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to encrypt or decrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="encryptionKey">Encryption key to use to encrypt or decrypt data.</param>
        /// <returns>An encrypted or decrypted version of the source data.</returns>
        public static byte[] Crypt(byte[] source, int startIndex, int length, byte[] encryptionKey)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= source.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            if (startIndex + length > source.Length)
                throw new ArgumentOutOfRangeException("length", "exceeds buffer size");

            if (encryptionKey == null)
                throw new ArgumentNullException("encryptionKey");

            if (encryptionKey.Length == 0)
                throw new ArgumentOutOfRangeException("encryptionKey", "encryptionKey must contain at least one byte");

            // The longer the encryption key, the better the encryption.
            // Repeated encryption sequences do not occur for (3 * encryptionKey.Length) unique bytes.
            byte[] cryptData = new byte[length];
            int algorithm = 0;
            int keyIndex = 0;
            byte cryptKey;
            byte cryptByte;

            // Seeds random number generator
            System.Random random = new System.Random(encryptionKey[0]);

            for (int x = startIndex; x < startIndex + length; x++)
            {
                cryptByte = source[x];

                if (cryptByte > 0)
                {
                    switch (algorithm)
                    {
                        case 0:
                            cryptKey = encryptionKey[keyIndex];
                            if (cryptByte != cryptKey)
                                cryptByte ^= cryptKey;
                            break;
                        case 1:
                            cryptKey = (byte)(random.NextDouble() * (encryptionKey[keyIndex] + 1));
                            if (cryptByte != cryptKey)
                                cryptByte ^= cryptKey;
                            break;
                        case 2:
                            cryptKey = (byte)(random.NextDouble() * 256.0D);
                            if (cryptByte != cryptKey)
                                cryptByte ^= cryptKey;
                            break;
                    }

                    cryptData[x - startIndex] = cryptByte;
                }

                // Selects next encryption algorithm.
                algorithm++;

                if (algorithm == 3)
                {
                    algorithm = 0;

                    // Selects next encryption key.
                    keyIndex++;

                    if (keyIndex > encryptionKey.Length - 1)
                        keyIndex = 0;
                }
            }

            return cryptData;
        }

        /// <summary>
        /// Returns an obfuscated stream using bit-rotation algorithms.
        /// </summary>
        /// <param name="source">Source stream to obfuscate.</param>
        /// <param name="encryptionKey">Encryption key to use to obfuscate stream.</param>
        /// <returns>An obfuscated version of the source stream.</returns>
        /// <remarks>
        /// This returns a memory stream of the encrypted results, if the incoming stream is
        /// very large this will consume a large amount of memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Obfuscate(Stream source, byte[] encryptionKey)
        {
            MemoryStream destination = new MemoryStream();

            Obfuscate(source, destination, encryptionKey);
            destination.Position = 0;

            return destination;
        }

        /// <summary>
        /// Obfuscates input stream onto output stream using bit-rotation algorithms.
        /// </summary>
        /// <param name="source">Source stream to obfuscate.</param>
        /// <param name="destination">Destination stream used to hold obfuscated data.</param>
        /// <param name="encryptionKey">Encryption key to use to obfuscate stream.</param>
        public static void Obfuscate(Stream source, Stream destination, byte[] encryptionKey)
        {
            byte[] buffer = new byte[Standard.BufferSize], results;
            int bytesRead = source.Read(buffer, 0, Standard.BufferSize);

            while (bytesRead > 0)
            {
                results = Obfuscate(buffer, 0, bytesRead, encryptionKey);
                destination.Write(results, 0, results.Length);
                bytesRead = source.Read(buffer, 0, Standard.BufferSize);
            }
        }

        /// <summary>
        /// Obfuscates data using bit-rotation algorithms.
        /// </summary>
        /// <param name="source">Binary array of data to obfuscate.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to obfuscate starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="encryptionKey">Encryption key to use to obfuscate data.</param>
        /// <returns>An obfuscated version of the source data.</returns>
        public static byte[] Obfuscate(byte[] source, int startIndex, int length, byte[] encryptionKey)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= source.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            if (startIndex + length > source.Length)
                throw new ArgumentOutOfRangeException("length", "exceeds buffer size");

            if (encryptionKey == null)
                throw new ArgumentNullException("encryptionKey");

            if (encryptionKey.Length == 0)
                throw new ArgumentOutOfRangeException("encryptionKey", "encryptionKey must contain at least one byte");

            byte key;
            long keyIndex = encryptionKey.Length - 1;
            byte[] cryptData = new byte[length];

            // Starts bit rotation cycle.
            for (int x = startIndex; x < startIndex + length; x++)
            {
                // Gets current key value.
                key = encryptionKey[keyIndex];

                if (key % 2 == 0)
                    cryptData[x - startIndex] = source[x].BitRotL(key);
                else
                    cryptData[x - startIndex] = source[x].BitRotR(key);

                // Selects next encryption key index.
                keyIndex--;

                if (keyIndex < 0)
                    keyIndex = encryptionKey.Length - 1;
            }

            return cryptData;
        }

        /// <summary>
        /// Returns a deobfuscated stream using bit-rotation algorithms.
        /// </summary>
        /// <param name="source">Source stream to deobfuscate.</param>
        /// <param name="encryptionKey">Encryption key to use to deobfuscate stream.</param>
        /// <returns>A deobfuscated version of the source stream.</returns>
        /// <remarks>
        /// This returns a memory stream of the decrypted results, if the incoming stream is
        /// very large this will consume a large amount of memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Deobfuscate(Stream source, byte[] encryptionKey)
        {
            MemoryStream destination = new MemoryStream();

            Deobfuscate(source, destination, encryptionKey);
            destination.Position = 0;

            return destination;
        }

        /// <summary>
        /// Deobfuscates input stream onto output stream using bit-rotation algorithms.
        /// </summary>
        /// <param name="source">Source stream to deobfuscate.</param>
        /// <param name="destination">Destination stream used to hold deobfuscated data.</param>
        /// <param name="encryptionKey">Encryption key to use to deobfuscate stream.</param>
        public static void Deobfuscate(Stream source, Stream destination, byte[] encryptionKey)
        {
            byte[] buffer = new byte[Standard.BufferSize], results;
            int bytesRead = source.Read(buffer, 0, Standard.BufferSize);

            while (bytesRead > 0)
            {
                results = Deobfuscate(buffer, 0, bytesRead, encryptionKey);
                destination.Write(results, 0, results.Length);
                bytesRead = source.Read(buffer, 0, Standard.BufferSize);
            }
        }

        /// <summary>
        /// Deobfuscates data using bit-rotation algorithms.
        /// </summary>
        /// <param name="source">Binary array of data to deobfuscate.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to deobfuscate starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="encryptionKey">Encryption key to use to deobfuscate data.</param>
        /// <returns>A deobfuscated version of the source data.</returns>
        public static byte[] Deobfuscate(byte[] source, int startIndex, int length, byte[] encryptionKey)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= source.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            if (startIndex + length > source.Length)
                throw new ArgumentOutOfRangeException("length", "exceeds buffer size");

            if (encryptionKey == null)
                throw new ArgumentNullException("encryptionKey");

            if (encryptionKey.Length == 0)
                throw new ArgumentOutOfRangeException("encryptionKey", "encryptionKey must contain at least one byte");

            byte key;
            long keyIndex = encryptionKey.Length - 1;
            byte[] cryptData = new byte[length];

            // Starts bit rotation cycle.
            for (int x = startIndex; x < startIndex + length; x++)
            {
                // Gets current key value.
                key = encryptionKey[keyIndex];

                if (key % 2 == 0)
                    cryptData[x - startIndex] = source[x].BitRotR(key);
                else
                    cryptData[x - startIndex] = source[x].BitRotL(key);

                // Selects next encryption key index.
                keyIndex--;

                if (keyIndex < 0)
                    keyIndex = encryptionKey.Length - 1;
            }

            return cryptData;
        }

        /// <summary>
        /// Returns a simple encoded string representing a number which can later be decoded with <see cref="GetSeedFromKey"/>.
        /// </summary>
        /// <param name="seed"><see cref="Int24"/> seed to encode.</param>
        /// <returns>An encoded version of source seed.</returns>
        public static string GetKeyFromSeed(Int24 seed)
        {
            if (seed < 0)
                throw new ArgumentException("Cannot calculate key from negative seed");

            // This is a handy algorithm for encoding an integer value, use GetSeedFromKey to decode.
            byte[] seedBytes = seed.GetBytes();
            int alphaIndex;
            int asciiA = (int)'A';

            // Creates alpha-numeric key string.
            StringBuilder result = new StringBuilder();

            for (int x = 0; x < 3; x++)
            {
                alphaIndex = Random.Int32Between(0, 25);
                if (x > 0) result.Append('-');
                result.Append((char)(asciiA + (25 - alphaIndex)));
                result.Append(seedBytes[x] + alphaIndex);
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns the number from a string encoded with <see cref="GetKeyFromSeed" />.
        /// </summary>
        /// <param name="key">Key to decode to a seed.</param>
        /// <returns>Seed decoded from source key.</returns>
        public static Int24 GetSeedFromKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key", "key cannot be null");

            byte[] seedBytes = new byte[3];
            int delimeter1, delimeter2;
            string code = "";
            int value;

            // Removes all white space from specified parameter.
            key = key.Trim().ToUpper();

            if (key.Length > 5 && key.Length < 15)
            {
                // Gets Delimiter positions.
                delimeter1 = key.IndexOf('-');
                delimeter2 = key.IndexOf('-', delimeter1 + 1);

                if (delimeter1 > -1 && delimeter2 > -1)
                {
                    for (int x = 0; x <= 2; x++)
                    {
                        // Extracts encoded byte.
                        switch (x)
                        {
                            case 0:
                                code = key.Substring(0, delimeter1);
                                break;
                            case 1:
                                code = key.Substring(delimeter1 + 1, delimeter2 - delimeter1 - 1);
                                break;
                            case 2:
                                code = key.Substring(delimeter2 + 1, key.Length - delimeter2 - 1);
                                break;
                        }

                        // Calculates byte.
                        value = int.Parse(code.Substring(1, code.Length - 1)) - (25 - (code.Substring(0, 1)[0] - 'A'));

                        // Validates calculation.
                        if (value >= 0 && value <= 255)
                            seedBytes[x] = (byte)value;
                        else                            
                            return -1; // Determines the key is invalid and exits with -1.
                    }

                    // Creates seed from its component bytes.
                    return Int24.GetValue(seedBytes, 0);
                }
            }

            // Determines the key is invalid and exits with -1.
            return -1;
        }
    }
}