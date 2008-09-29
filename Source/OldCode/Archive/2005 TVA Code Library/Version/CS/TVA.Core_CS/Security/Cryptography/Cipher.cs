//*******************************************************************************************************
//  Common.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/04/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Crypto).
//  02/28/2007 - J. Ritchie Carroll
//       Changed string-based encrypt and decrypt functions to return null if
//       input string to be encrypted or decrypted was null or empty.
//  10/11/2007 - J. Ritchie Carroll
//       Added Obfuscate and Deobfuscate functions that perform data obfuscation
//       based upon simple bit-rotation algorithms.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/19/2008 - James R Carroll
//       Converted to C# - basic encryption/decryption extend string, byte[], and Stream.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic;
using TVA.Interop;

namespace TVA.Security.Cryptography
{
    #region [ Enumerations ]
    
    /// <summary>Enumerates cryptographic strength.</summary>
    /// <remarks>
    /// <para>
    /// Encryption algorithms are cumulative. The levels represent tradeoffs on speed vs. cipher strength. Level 1
    /// will have the fastest encryption speed with the simplest encryption strength, and level 5 will have the
    /// strongest cumulative encryption strength with the slowest encryption speed.
    /// </para>
    /// </remarks>
    public enum CipherStrength
    {
        /// <summary>Uses no encryption.</summary>
        None,
        /// <summary>Adds simple multi-alogorithm XOR based encryption.</summary>
        Level1,
        /// <summary>Adds TripleDES based encryption.</summary>
        Level2,
        /// <summary>Adds RC2 based encryption.</summary>
        Level3,
        /// <summary>Adds RijndaelManaged based enryption.</summary>
        Level4,
        /// <summary>Adds simple bit-rotation based enryption.</summary>
        Level5
    }

    #endregion

    /// <summary>Performs common cryptographic functions.</summary>
    public static class Cipher
    {
        // IMPORTANT! Never change the following constants, or you will break cross-application crypto operability:
        private const string StandardKey = "{&-<%=($#/T.V:A!\\,@[20O3]*^_j`|?)>+~}";
        private const int BufferSize = 262144; // 256K

        /// <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        /// the given parameter, using the standard encryption key and encryption level 1,</summary>
        public static string Encrypt(this string source)
        {
            return source.Encrypt(null, CipherStrength.Level1);
        }

        /// <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        /// the given parameters using standard encryption.</summary>
        public static string Encrypt(this string source, CipherStrength strength)
        {
            return source.Encrypt(null, strength);
        }

        /// <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        /// the given parameters.</summary>
        public static string Encrypt(this string source, string encryptionKey, CipherStrength strength)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            if (string.IsNullOrEmpty(encryptionKey))
                encryptionKey = StandardKey;

            byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
            byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

            return Convert.ToBase64String(Encoding.Unicode.GetBytes(source).Encrypt(rgbKey, rgbIV, strength));
        }

        /// <summary>Returns a binary array of encrypted data for the given parameters.</summary>
        public static byte[] Encrypt(this byte[] source, byte[] key, byte[] IV, CipherStrength strength)
        {
            return source.Encrypt(0, source.Length, key, IV, strength);
        }

        /// <summary>Returns a binary array of encrypted data for the given parameters.</summary>
        public static byte[] Encrypt(this byte[] source, int startIndex, int length, byte[] key, byte[] IV, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            // Performs requested levels of encryption.
            source = Crypt(source, startIndex, length, key);
            if (strength >= CipherStrength.Level2)
            {
                source = new TripleDESCryptoServiceProvider().Encrypt(source, 0, source.Length, key, IV);
                if (strength >= CipherStrength.Level3)
                {
                    source = new RC2CryptoServiceProvider().Encrypt(source, 0, source.Length, key, IV);
                    if (strength >= CipherStrength.Level4)
                    {
                        source = new RijndaelManaged().Encrypt(source, 0, source.Length, key, IV);
                        if (strength >= CipherStrength.Level5)
                        {
                            source = Obfuscate(source, 0, source.Length, key);
                        }
                    }
                }
            }

            return source;
        }

        #region [ Old Code ]

        // The following pure stream encryption implementation is incompatible with the one that implements a progress handler
        // since it embeds original stream lengths into the encrypted stream - so this method was removed to prevent possible
        // confusion/errors during decryption cycle. 

        ///// <summary>Returns a stream of encrypted data for the given parameters.</summary>
        //public static Stream Encrypt(Stream inStream, byte[] key, byte[] IV, CipherStrength strength)
        //{
        //    if (strength == CipherStrength.None)
        //        return inStream;

        //    // Performs requested levels of encryption.
        //    inStream = Crypt(inStream, key);

        //    if (strength >= CipherStrength.Level2)
        //    {
        //        inStream = Encrypt(new TripleDESCryptoServiceProvider(), inStream, key, IV);
        //        if (strength >= CipherStrength.Level3)
        //        {
        //            inStream = Encrypt(new RC2CryptoServiceProvider(), inStream, key, IV);
        //            if (strength >= CipherStrength.Level4)
        //            {
        //                inStream = Encrypt(new RijndaelManaged(), inStream, key, IV);
        //                if (strength >= CipherStrength.Level5)
        //                {
        //                    inStream = Obfuscate(inStream, key);
        //                }
        //            }
        //        }
        //    }

        //    return inStream;
        //}

        #endregion

        /// <summary>Returns a stream of encrypted data for the given parameters.</summary>
        /// <remarks>
        /// This returns a memory stream of the encrypted results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Encrypt(this Stream source, byte[] key, byte[] IV, CipherStrength strength)
        {
            MemoryStream destination = new MemoryStream();

            source.Encrypt(destination, key, IV, strength, null);
            destination.Position = 0;

            return destination;
        }

        /// <summary>Encrypts input stream onto output stream for the given parameters.</summary>
        public static void Encrypt(this Stream source, Stream destination, byte[] key, byte[] IV, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgress<long>.Handler progress = null;
            byte[] inBuffer = new byte[BufferSize];
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
                progress = new ProcessProgress<long>.Handler(progressHandler, "Encrypt", length);
                progress.Complete = 0;
            }

            // Reads initial buffer.
            read = source.Read(inBuffer, 0, BufferSize);

            while (read > 0)
            {
                // Encrypts buffer.
                outBuffer = inBuffer.CopyBuffer(0, read).Encrypt(key, IV, strength);

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
                read = source.Read(inBuffer, 0, BufferSize);
            }
        }

        /// <summary>Returns a binary array of encrypted data for the given parameters.</summary>
        public static byte[] Encrypt(this SymmetricAlgorithm algorithm, byte[] data, int startIndex, int length, byte[] key, byte[] IV)
        {
            MemoryStream inStream = new MemoryStream(data, startIndex, length);
            MemoryStream outStream = new MemoryStream();

            algorithm.Encrypt(inStream, outStream, key, IV);
            outStream.Position = 0;

            return outStream.ToArray();
        }

        /// <summary>Encrypts input stream onto output stream for the given parameters.</summary>
        public static void Encrypt(this SymmetricAlgorithm algorithm, Stream inStream, Stream outStream, byte[] key, byte[] IV)
        {
            // This is the root encryption function. Eventually, all the symmetric algorithm based encryption
            // functions perform their actual encryption here.
            byte[] rgbKey = algorithm.GetLegalKey(key);
            byte[] rgbIV = algorithm.GetLegalIV(IV);
            byte[] buffer = new byte[BufferSize];
            CryptoStream encodeStream = new CryptoStream(outStream, algorithm.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            int read;

            // Encrypts data onto output stream.
            read = inStream.Read(buffer, 0, BufferSize);

            while (read > 0)
            {
                encodeStream.Write(buffer, 0, read);
                read = inStream.Read(buffer, 0, BufferSize);
            }

            encodeStream.FlushFinalBlock();
        }

        /// <summary>Creates an encrypted file from source file data.</summary>
        public static void EncryptFile(string sourceFilename, string destinationFilename, CipherStrength strength)
        {
            EncryptFile(sourceFilename, destinationFilename, null, strength, null);
        }

        /// <summary>Creates an encrypted file from source file data.</summary>
        public static void EncryptFile(string sourceFilename, string destinationFilename, string encryptionKey, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            if (string.IsNullOrEmpty(encryptionKey)) encryptionKey = StandardKey;

            FileStream sourceFileStream = File.Open(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFilename);
            byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
            byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

            sourceFileStream.Encrypt(destFileStream, rgbKey, rgbIV, strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }

        /// <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        /// parameter using the standard encryption key and encryption level 1.</summary>
        public static string Decrypt(this string source)
        {
            return source.Decrypt(null, CipherStrength.Level1);
        }

        /// <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        /// parameters using the standard encryption key.</summary>
        public static string Decrypt(this string source, CipherStrength strength)
        {
            return source.Decrypt(null, strength);
        }

        /// <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        /// parameters.</summary>
        public static string Decrypt(this string source, string encryptionKey, CipherStrength strength)
        {
            if (string.IsNullOrEmpty(source)) return null;
            if (string.IsNullOrEmpty(encryptionKey)) encryptionKey = StandardKey;

            byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
            byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

            return Encoding.Unicode.GetString(Convert.FromBase64String(source).Decrypt(rgbKey, rgbIV, strength));
        }

        /// <summary>Returns a binary array of decrypted data for the given parameters.</summary>
        public static byte[] Decrypt(this byte[] source, byte[] key, byte[] IV, CipherStrength strength)
        {
            return source.Decrypt(0, source.Length, key, IV, strength);
        }

        /// <summary>Returns a binary array of decrypted data for the given parameters.</summary>
        public static byte[] Decrypt(this byte[] source, int startIndex, int length, byte[] key, byte[] IV, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            // Performs requested levels of decryption.
            if (strength >= CipherStrength.Level5)
            {
                source = Deobfuscate(source, startIndex, length, key);
                startIndex = 0;
                length = source.Length;
            }

            if (strength >= CipherStrength.Level4)
            {
                source = new RijndaelManaged().Decrypt(source, startIndex, length, key, IV);
                startIndex = 0;
                length = source.Length;
            }

            if (strength >= CipherStrength.Level3)
            {
                source = new RC2CryptoServiceProvider().Decrypt(source, startIndex, length, key, IV);
                startIndex = 0;
                length = source.Length;
            }

            if (strength >= CipherStrength.Level2)
            {
                source = new TripleDESCryptoServiceProvider().Decrypt(source, startIndex, length, key, IV);
                startIndex = 0;
                length = source.Length;
            }

            return Crypt(source, startIndex, length, key);
        }

        #region [ Old Code ]

        // The following pure stream decryption implementation is incompatible with the one that implements a progress handler
        // since it uses embedded stream lengths from the original encrypted stream - so this method was removed to prevent possible
        // confusion/errors during decryption cycle. 

        ///// <summary>Returns a stream of decrypted data for the given parameters.</summary>
        //public static Stream Decrypt(Stream inStream, byte[] key, byte[] IV, CipherStrength strength)
        //{
        //    if (strength == CipherStrength.None)
        //        return inStream;

        //    // Performs requested levels of decryption.
        //    if (strength >= CipherStrength.Level5)
        //        inStream = Deobfuscate(inStream, key);

        //    if (strength >= CipherStrength.Level4)
        //        inStream = Decrypt(new RijndaelManaged(), inStream, key, IV);

        //    if (strength >= CipherStrength.Level3)
        //        inStream = Decrypt(new RC2CryptoServiceProvider(), inStream, key, IV);

        //    if (strength >= CipherStrength.Level2)
        //        inStream = Decrypt(new TripleDESCryptoServiceProvider(), inStream, key, IV);

        //    return Crypt(inStream, key);
        //}

        #endregion

        /// <summary>Returns a stream of decrypted data for the given parameters.</summary>
        /// <remarks>
        /// This returns a memory stream of the decrypted results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Decrypt(this Stream source, byte[] key, byte[] IV, CipherStrength strength)
        {
            MemoryStream destination = new MemoryStream();

            source.Decrypt(destination, key, IV, strength, null);
            destination.Position = 0;

            return destination;
        }

        /// <summary>Decrypts input stream onto output stream for the given parameters.</summary>
        public static void Decrypt(this Stream source, Stream destination, byte[] key, byte[] IV, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgress<long>.Handler progress = null;
            byte[] inBuffer, outBuffer;
            byte[] lengthBuffer = BitConverter.GetBytes(0);
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
                progress = new ProcessProgress<long>.Handler(progressHandler, "Decrypt", length);
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
                        outBuffer = inBuffer.Decrypt(key, IV, strength);
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

        /// <summary>Returns a binary array of decrypted data for the given parameters.</summary>
        public static byte[] Decrypt(this SymmetricAlgorithm algorithm, byte[] data, int startIndex, int length, byte[] key, byte[] IV)
        {
            MemoryStream inStream = new MemoryStream(data, startIndex, length);
            MemoryStream outStream = new MemoryStream();

            algorithm.Decrypt(inStream, outStream, key, IV);
            outStream.Position = 0;

            return outStream.ToArray();
        }

        /// <summary>Decrypts input stream onto output stream for the given parameters.</summary>
        public static void Decrypt(this SymmetricAlgorithm algorithm, Stream inStream, Stream outStream, byte[] key, byte[] IV)
        {
            // This is the root decryption function. Eventually, all the symmetric algorithm based decryption
            // functions perform their actual decryption here.
            byte[] rgbKey = algorithm.GetLegalKey(key);
            byte[] rgbIV = algorithm.GetLegalIV(IV);
            byte[] buffer = new byte[BufferSize];
            CryptoStream decodeStream = new CryptoStream(outStream, algorithm.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            int read;

            // Decrypts data onto output stream.
            read = inStream.Read(buffer, 0, BufferSize);

            while (read > 0)
            {
                decodeStream.Write(buffer, 0, read);
                read = inStream.Read(buffer, 0, BufferSize);
            }

            decodeStream.FlushFinalBlock();
        }

        /// <summary>Creates a decrypted file from source file data.</summary>
        public static void DecryptFile(string sourceFilename, string destinationFilename, CipherStrength strength)
        {
            DecryptFile(sourceFilename, destinationFilename, null, strength, null);
        }

        /// <summary>Creates a decrypted file from source file data.</summary>
        public static void DecryptFile(string sourceFilename, string destinationFilename, string encryptionKey, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            if (string.IsNullOrEmpty(encryptionKey)) encryptionKey = StandardKey;

            FileStream sourceFileStream = File.Open(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFilename);
            byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
            byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

            sourceFileStream.Decrypt(destFileStream, rgbKey, rgbIV, strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }

        /// <summary>Coerces key to maximum legal bit length for given encryption algorithm.</summary>
        public static byte[] GetLegalKey(this SymmetricAlgorithm algorithm, byte[] key)
        {
            byte[] rgbKey = new byte[algorithm.LegalKeySizes[0].MaxSize / 8];

            for (int x = 0; x <= rgbKey.Length - 1; x++)
            {
                if (x < key.Length)
                    rgbKey[x] = key[x];
                else
                    rgbKey[x] = Encoding.ASCII.GetBytes(StandardKey.Substring(x % StandardKey.Length, 1))[0];
            }

            return rgbKey;
        }

        /// <summary>Coerces initialization vector to legal block size for given encryption algorithm.</summary>
        public static byte[] GetLegalIV(this SymmetricAlgorithm algorithm, byte[] IV)
        {
            byte[] rgbIV = new byte[algorithm.LegalBlockSizes[0].MinSize / 8];

            for (int x = 0; x <= rgbIV.Length - 1; x++)
            {
                if (x < IV.Length)
                    rgbIV[x] = IV[IV.Length - 1 - x];
                else
                    rgbIV[x] = Encoding.ASCII.GetBytes(StandardKey.Substring(x % StandardKey.Length, 1))[0];
            }

            return rgbIV;
        }

        /// <summary>Returns an encrypted or decrypted stream using XOR based algorithms. Call once to
        /// encrypt, call again with same key to decrypt.</summary>
        /// <remarks>
        /// This returns a memory stream of the encrypted or decrypted results, if the incoming
        /// stream is very large this will consume a large amount memory.  In this case use the
        /// overload that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Crypt(Stream source, byte[] encryptionKey)
        {
            MemoryStream destination = new MemoryStream();

            Crypt(source, destination, encryptionKey);
            destination.Position = 0;

            return destination;
        }

        /// <summary>Encrypts or decrypts input stream onto output stream using XOR based algorithms. Call once to
        /// encrypt, call again with same key to decrypt.</summary>
        public static void Crypt(Stream source, Stream destination, byte[] encryptionKey)
        {
            byte[] buffer = new byte[BufferSize], results;
            int bytesRead = source.Read(buffer, 0, BufferSize);

            while (bytesRead > 0)
            {
                results = Crypt(buffer, 0, bytesRead, encryptionKey);
                destination.Write(results, 0, results.Length);
                bytesRead = source.Read(buffer, 0, BufferSize);
            }
        }

        /// <summary>Encrypts or decrypts data using XOR based algorithms. Call once to encrypt; call again with same
        /// key to decrypt.</summary>
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

            // For backwards compatibility with older Visual Basic versions of the code library this
            // function replicates the original code along with proper calls to VB intrinsic functions...

            // The longer the encryption key, the better the encryption.
            // Repeated encryption sequences do not occur for (3 * encryptionKey.Length) unique bytes.
            byte[] cryptData = new byte[length];
            int algorithm = 0;
            int keyIndex = 0;
            int cryptKey;
            byte cryptByte;

            // Re-seeds Visual Basic random number generator.            
            VBMath.Rnd(-1);
            VBMath.Randomize(encryptionKey[0]);

            for (int x = startIndex; x < startIndex + length; x++)
            {
                cryptByte = source[x];

                if (cryptByte > 0)
                {
                    switch (algorithm)
                    {
                        case 0:
                            cryptKey = encryptionKey[keyIndex];
                            if (cryptByte != cryptKey) cryptByte = (byte)(cryptByte ^ cryptKey);
                            break;
                        case 1:
                            cryptKey = (int)Math.Round(Conversion.Int(VBMath.Rnd() * (encryptionKey[keyIndex] + 1)));
                            if (cryptByte != cryptKey) cryptByte = (byte)(cryptByte ^ cryptKey);
                            break;
                        case 2:
                            cryptKey = (int)Math.Round(Conversion.Int(VBMath.Rnd() * 256.0F));
                            if (cryptByte != cryptKey) cryptByte = (byte)(cryptByte ^ cryptKey);
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

        /// <summary>Returns an obfuscated stream using bit-rotation algorithms.</summary>
        /// <remarks>
        /// This returns a memory stream of the encrypted results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Obfuscate(Stream source, byte[] encryptionKey)
        {
            MemoryStream destination = new MemoryStream();

            Obfuscate(source, destination, encryptionKey);
            destination.Position = 0;

            return destination;
        }

        /// <summary>Obfuscates input stream onto output stream using bit-rotation algorithms.</summary>
        public static void Obfuscate(Stream source, Stream destination, byte[] encryptionKey)
        {
            byte[] buffer = new byte[BufferSize], results;
            int bytesRead = source.Read(buffer, 0, BufferSize);

            while (bytesRead > 0)
            {
                results = Obfuscate(buffer, 0, bytesRead, encryptionKey);
                destination.Write(results, 0, results.Length);
                bytesRead = source.Read(buffer, 0, BufferSize);
            }
        }

        /// <summary>Obfuscates data using bit-rotation algorithms.</summary>
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

        /// <summary>Returns a deobfuscated stream using bit-rotation algorithms.</summary>
        /// <remarks>
        /// This returns a memory stream of the decrypted results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Deobfuscate(Stream source, byte[] encryptionKey)
        {
            MemoryStream destination = new MemoryStream();

            Deobfuscate(source, destination, encryptionKey);
            destination.Position = 0;

            return destination;
        }

        /// <summary>Deobfuscates input stream onto output stream using bit-rotation algorithms.</summary>
        public static void Deobfuscate(Stream source, Stream destination, byte[] encryptionKey)
        {
            byte[] buffer = new byte[BufferSize], results;
            int bytesRead = source.Read(buffer, 0, BufferSize);

            while (bytesRead > 0)
            {
                results = Deobfuscate(buffer, 0, bytesRead, encryptionKey);
                destination.Write(results, 0, results.Length);
                bytesRead = source.Read(buffer, 0, BufferSize);
            }
        }

        /// <summary>Deobfuscates data using bit-rotation algorithms.</summary>
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

        /// <summary>Generates a random key useful for cryptographic functions.</summary>
        public static string GenerateKey()
		{
			char[] keyChars;
			char keyChar;
			int x, y;
			
			// Generates a character array of unique values.
			StringBuilder result = new StringBuilder();

			result.Append(StandardKey);
			result.Append(Guid.NewGuid().ToString().ToLower().Replace("-", "©ª¦"));
			result.Append(DateTime.UtcNow.Ticks);

			for (x = 1; x <= 50; x++)
			{
				result.Append(Convert.ToChar(Random.Int32Between(33, 255)));
			}

			result.Append(Environment.MachineName);
			result.Append(GetKeyFromSeed((Int24)DateAndTime.Timer));
			result.Append(Environment.UserDomainName);
			result.Append(Environment.UserName);
			result.Append(Common.SystemTimer);
			result.Append(DateTime.Now.ToString().Replace("/", "¡¤¥").Replace(" ", "°"));
			result.Append(Guid.NewGuid().ToString().ToUpper().Replace("-", "£§"));

			keyChars = result.ToString().ToCharArray();
			
			// Swaps values around in array at random.
			for (x = 0; x <= keyChars.Length - 1; x++)
			{
				y = Random.Int32Between(1, keyChars.Length) - 1;
				if (x != y)
				{
					keyChar = keyChars[x];
					keyChars[x] = keyChars[y];
					keyChars[y] = keyChar;
				}
			}
			
			return new string(keyChars);
		}

        /// <summary>Returns a simple encoded string representing a number which can later be decoded
        /// with <see cref="GetSeedFromKey" />.</summary>
        /// <remarks>This function was designed for 24-bit values.</remarks>
        public static string GetKeyFromSeed(Int24 seed)
		{
            if (seed < 0)
                throw new ArgumentException("Cannot calculate key from negative seed");

            // This is a handy algorithm for encoding an integer value, use GetSeedFromKey to decode.
			int seedValue = (int)seed;
			byte[] seedBytes = new byte[4];
			int alphaIndex;
			int asciiA = Strings.Asc('A');
			
			// Breaks seed into its component bytes.
			seedBytes[0] = Bit.LoByte(Bit.LoWord(seedValue));
			seedBytes[1] = Bit.HiByte(Bit.LoWord(seedValue));
			seedBytes[2] = Bit.LoByte(Bit.HiWord(seedValue));
			
			// Creates alpha-numeric key string.
			StringBuilder result = new StringBuilder();

			for (int x = 0; x <= 2; x++)
			{
				alphaIndex = Random.Int32Between(0, 25);
				if (x > 0) result.Append('-');
				result.Append(Convert.ToChar(asciiA + (25 - alphaIndex)));
				result.Append(seedBytes[x] + alphaIndex);
			}
			
			return result.ToString();
		}

        /// <summary>Returns the number from a string encoded with <see cref="GetKeyFromSeed" />.</summary>
        public static Int24 GetSeedFromKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw (new ArgumentNullException("key", "key cannot be null"));

            byte[] seedBytes = new byte[3];
            int delimeter1;
            int delimeter2;
            string code = "";
            int value;

            // Removes all white space from specified parameter.
            key = key.Trim().ToUpper();

            if (key.Length > 5 && key.Length < 15)
            {
                // Gets Delimiter positions.
                delimeter1 = Strings.InStr(key, "-", 0);
                delimeter2 = Strings.InStr(delimeter1 + 1, key, "-", 0);

                if (delimeter1 > 0 && delimeter2 > 0)
                {
                    for (int x = 0; x <= 2; x++)
                    {
                        // Extracts encoded byte.
                        switch (x)
                        {
                            case 0:
                                code = key.Substring(0, delimeter1 - 1);
                                break;
                            case 1:
                                code = key.Substring(delimeter1 + 1 - 1, delimeter2 - delimeter1 - 1);
                                break;
                            case 2:
                                code = key.Substring(key.Length - key.Length - delimeter2, key.Length - delimeter2);
                                break;
                        }

                        // Calculates byte.
                        value = (int)Conversion.Val(code.Substring(code.Length - code.Length - 1, code.Length - 1)) - 
                            (25 - (Strings.Asc(code.Substring(0, 1)) - Strings.Asc("A")));

                        // Validates calculation.
                        if (value >= 0 && value <= 255)
                        {
                            seedBytes[x] = (byte)value;
                        }
                        else
                        {
                            // Determines the key is invalid and exits with -1.
                            return -1;
                        }
                    }

                    // Creates seed from its component bytes.
                    return (Int24)Bit.MakeDWord(Bit.MakeWord(0, seedBytes[2]), Bit.MakeWord(seedBytes[1], seedBytes[0]));
                }
            }

            // Determines the key is invalid and exits with -1.
            return -1;
        }
    }
}