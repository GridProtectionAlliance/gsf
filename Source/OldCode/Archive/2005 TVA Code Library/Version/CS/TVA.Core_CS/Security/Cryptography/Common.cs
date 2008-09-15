using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
//using TVA.Common;
//using TVA.Math.Common;
using TVA.IO.Common;
using TVA.Interop.Bit;

//*******************************************************************************************************
//  TVA.Security.Cryptography.Common.vb - Handy Cryptography Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
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
//
//*******************************************************************************************************


namespace TVA
{
    namespace Security
    {
        namespace Cryptography
        {


            /// <summary>Performs common cryptographic functions.</summary>
            public sealed class Common
            {


                public delegate void ProgressEventHandler(long bytesCompleted, long bytesTotal);

                // IMPORTANT! Never change the following constants, or you will break cross-application crypto operability:
                private const string StandardKey = "{&-<%=($#/T.V:A!\\,@[20O3]*^_j`|?)>+~}";
                private const int BufferSize = 262144; // 256K

                private Common()
                {

                    // This class contains only global functions and is not meant to be instantiated.

                }

                /// <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
                /// the given parameter, using the standard encryption key and encryption level 1,</summary>
                public static string Encrypt(string str)
                {

                    return Encrypt(str, null, EncryptLevel.Level1);

                }

                /// <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
                /// the given parameters using standard encryption.</summary>
                public static string Encrypt(string str, EncryptLevel strength)
                {

                    return Encrypt(str, null, strength);

                }

                /// <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
                /// the given parameters.</summary>
                public static string Encrypt(string str, string encryptionKey, EncryptLevel strength)
                {

                    if (string.IsNullOrEmpty(str))
                    {
                        return null;
                    }
                    if (string.IsNullOrEmpty(encryptionKey))
                    {
                        encryptionKey = StandardKey;
                    }

                    byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
                    byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

                    return Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(str), rgbKey, rgbIV, strength));

                }

                /// <summary>Returns a binary array of encrypted data for the given parameters.</summary>
                public static byte[] Encrypt(byte[] data, byte[] key, byte[] IV, EncryptLevel strength)
                {

                    if (strength == EncryptLevel.None)
                    {
                        return data;
                    }

                    // Performs requested levels of encryption.
                    data = Crypt(data, key);
                    if (strength >= EncryptLevel.Level2)
                    {
                        data = Encrypt(new TripleDESCryptoServiceProvider(), data, key, IV);
                        if (strength >= EncryptLevel.Level3)
                        {
                            data = Encrypt(new RC2CryptoServiceProvider(), data, key, IV);
                            if (strength >= EncryptLevel.Level4)
                            {
                                data = Encrypt(new RijndaelManaged(), data, key, IV);
                                if (strength >= EncryptLevel.Level5)
                                {
                                    data = Obfuscate(data, key);
                                }
                            }
                        }
                    }

                    return data;

                }

                /// <summary>Returns a binary array of encrypted data for the given parameters.</summary>
                public static byte[] Encrypt(SymmetricAlgorithm algorithm, byte[] data, byte[] key, byte[] IV)
                {

                    return ((MemoryStream)(Encrypt(algorithm, new MemoryStream(data), key, IV))).ToArray();

                }

                /// <summary>Returns a stream of encrypted data for the given parameters.</summary>
                public static Stream Encrypt(Stream inStream, byte[] key, byte[] IV, EncryptLevel strength)
                {

                    if (strength == EncryptLevel.None)
                    {
                        return inStream;
                    }

                    // Performs requested levels of encryption.
                    inStream = Crypt(inStream, key);
                    if (strength >= EncryptLevel.Level2)
                    {
                        inStream = Encrypt(new TripleDESCryptoServiceProvider(), inStream, key, IV);
                        if (strength >= EncryptLevel.Level3)
                        {
                            inStream = Encrypt(new RC2CryptoServiceProvider(), inStream, key, IV);
                            if (strength >= EncryptLevel.Level4)
                            {
                                inStream = Encrypt(new RijndaelManaged(), inStream, key, IV);
                                if (strength >= EncryptLevel.Level5)
                                {
                                    inStream = Obfuscate(inStream, key);
                                }
                            }
                        }
                    }

                    return inStream;

                }

                /// <summary>Returns a stream of encrypted data for the given parameters.</summary>
                public static Stream Encrypt(SymmetricAlgorithm algorithm, Stream inStream, byte[] key, byte[] IV)
                {

                    MemoryStream outStream = new MemoryStream();

                    Encrypt(algorithm, inStream, outStream, key, IV);
                    outStream.Position = 0;

                    return outStream;

                }

                /// <summary>Encrypts input stream onto output stream for the given parameters.</summary>
                public static void Encrypt(Stream inStream, Stream outStream, byte[] key, byte[] IV, EncryptLevel strength, ProgressEventHandler progressHandler)
                {

                    byte[] inBuffer = new byte[BufferSize];
                    byte[] outBuffer;
                    byte[] lengthBuffer;
                    int read;
                    long total;
                    long length = -1;

                    // Sends initial progress event.
                    if (progressHandler != null)
                    {
                        try
                        {
                            if (inStream.CanSeek)
                            {
                                length = inStream.Length;
                            }
                        }
                        catch
                        {
                            length = -1;
                        }

                        progressHandler(0, length);
                    }

                    // Reads initial buffer.
                    read = inStream.Read(inBuffer, 0, BufferSize);

                    while (read > 0)
                    {
                        // Encrypts buffer.
                        outBuffer = Encrypt(CopyBuffer(inBuffer, 0, read), key, IV, strength);

                        // The destination encryption stream length does not have to be same as the input stream length, so we
                        // prepend the final size of each encrypted buffer onto the destination ouput stream so that we can
                        // safely decrypt the stream in a "chunked" fashion later.
                        lengthBuffer = BitConverter.GetBytes(outBuffer.Length);
                        outStream.Write(lengthBuffer, 0, lengthBuffer.Length);
                        outStream.Write(outBuffer, 0, outBuffer.Length);

                        // Updates encryption progress.
                        if (progressHandler != null)
                        {
                            total += read;
                            progressHandler(total, length);
                        }

                        // Reads next buffer.
                        read = inStream.Read(inBuffer, 0, BufferSize);
                    }

                }

                /// <summary>Encrypts input stream onto output stream for the given parameters.</summary>
                public static void Encrypt(SymmetricAlgorithm algorithm, Stream inStream, Stream outStream, byte[] key, byte[] IV)
                {

                    // This is the root encryption function. Eventually, all the encryption functions perform their actual
                    // encryption here.
                    byte[] rgbKey = GetLegalKey(algorithm, key);
                    byte[] rgbIV = GetLegalIV(algorithm, IV);
                    CryptoStream encodeStream = new CryptoStream(outStream, algorithm.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                    byte[] buffer = new byte[BufferSize];
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
                public static void EncryptFile(string sourceFileName, string destFileName, EncryptLevel strength)
                {

                    EncryptFile(sourceFileName, destFileName, null, strength, null);

                }

                /// <summary>Creates an encrypted file from source file data.</summary>
                public static void EncryptFile(string sourceFileName, string destFileName, string encryptionKey, EncryptLevel strength, ProgressEventHandler progressHandler)
                {

                    if (string.IsNullOrEmpty(encryptionKey))
                    {
                        encryptionKey = StandardKey;
                    }

                    FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    FileStream destFileStream = File.Create(destFileName);
                    byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
                    byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

                    Encrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, strength, progressHandler);

                    destFileStream.Flush();
                    destFileStream.Close();
                    sourceFileStream.Close();

                }

                /// <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
                /// parameter using the standard encryption key and encryption level 1.</summary>
                public static string Decrypt(string str)
                {

                    return Decrypt(str, null, EncryptLevel.Level1);

                }

                /// <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
                /// parameters using the standard encryption key.</summary>
                public static string Decrypt(string str, EncryptLevel strength)
                {

                    return Decrypt(str, null, strength);

                }

                /// <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
                /// parameters.</summary>
                public static string Decrypt(string str, string encryptionKey, EncryptLevel strength)
                {

                    if (string.IsNullOrEmpty(str))
                    {
                        return null;
                    }
                    if (string.IsNullOrEmpty(encryptionKey))
                    {
                        encryptionKey = StandardKey;
                    }

                    byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
                    byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

                    return Encoding.Unicode.GetString(Decrypt(Convert.FromBase64String(str), rgbKey, rgbIV, strength));

                }

                /// <summary>Returns a binary array of decrypted data for the given parameters.</summary>
                public static byte[] Decrypt(byte[] data, byte[] key, byte[] IV, EncryptLevel strength)
                {

                    if (strength == EncryptLevel.None)
                    {
                        return data;
                    }

                    // Performs requested levels of decryption.
                    if (strength >= EncryptLevel.Level5)
                    {
                        data = Deobfuscate(data, key);
                    }
                    if (strength >= EncryptLevel.Level4)
                    {
                        data = Decrypt(new RijndaelManaged(), data, key, IV);
                    }
                    if (strength >= EncryptLevel.Level3)
                    {
                        data = Decrypt(new RC2CryptoServiceProvider(), data, key, IV);
                    }
                    if (strength >= EncryptLevel.Level2)
                    {
                        data = Decrypt(new TripleDESCryptoServiceProvider(), data, key, IV);
                    }

                    return Crypt(data, key);

                }

                /// <summary>Returns a binary array of decrypted data for the given parameters.</summary>
                public static byte[] Decrypt(SymmetricAlgorithm algorithm, byte[] data, byte[] key, byte[] IV)
                {

                    return ((MemoryStream)(Decrypt(algorithm, new MemoryStream(data), key, IV))).ToArray();

                }

                /// <summary>Returns a stream of decrypted data for the given parameters.</summary>
                public static Stream Decrypt(Stream inStream, byte[] key, byte[] IV, EncryptLevel strength)
                {

                    if (strength == EncryptLevel.None)
                    {
                        return inStream;
                    }

                    // Performs requested levels of decryption.
                    if (strength >= EncryptLevel.Level5)
                    {
                        inStream = Deobfuscate(inStream, key);
                    }
                    if (strength >= EncryptLevel.Level4)
                    {
                        inStream = Decrypt(new RijndaelManaged(), inStream, key, IV);
                    }
                    if (strength >= EncryptLevel.Level3)
                    {
                        inStream = Decrypt(new RC2CryptoServiceProvider(), inStream, key, IV);
                    }
                    if (strength >= EncryptLevel.Level2)
                    {
                        inStream = Decrypt(new TripleDESCryptoServiceProvider(), inStream, key, IV);
                    }

                    return Crypt(inStream, key);

                }

                /// <summary>Returns a stream of decrypted data for the given parameters.</summary>
                public static Stream Decrypt(SymmetricAlgorithm algorithm, Stream inStream, byte[] key, byte[] IV)
                {

                    MemoryStream outStream = new MemoryStream();

                    Decrypt(algorithm, inStream, outStream, key, IV);
                    outStream.Position = 0;

                    return outStream;

                }

                /// <summary>Decrypts input stream onto output stream for the given parameters.</summary>
                public static void Decrypt(Stream inStream, Stream outStream, byte[] key, byte[] IV, EncryptLevel strength, ProgressEventHandler progressHandler)
                {

                    byte[] inBuffer;
                    byte[] outBuffer;
                    byte[] lengthBuffer = BitConverter.GetBytes(0);
                    int size;
                    int read;
                    long total;
                    long length = -1;

                    // Sends initial progress event.
                    if (progressHandler != null)
                    {
                        try
                        {
                            if (inStream.CanSeek)
                            {
                                length = inStream.Length;
                            }
                        }
                        catch
                        {
                            length = -1;
                        }

                        progressHandler(0, length);
                    }

                    // When the source stream was encrypted, it was known that the encrypted stream length did not have to be same as
                    // the input stream length. We prepended the final size of the each encrypted buffer onto the destination
                    // ouput stream (now the input stream to this function), so that we could safely decrypt the stream in a
                    // "chunked" fashion, hence the following:

                    // Reads the size of the next buffer from the stream.
                    read = inStream.Read(lengthBuffer, 0, lengthBuffer.Length);

                    while (read > 0)
                    {
                        // Converts the byte array containing the buffer size into an integer.
                        size = BitConverter.ToInt32(lengthBuffer, 0);

                        if (size > 0)
                        {
                            // Creates and reads the next buffer.
                            inBuffer = new byte[size];
                            read = inStream.Read(inBuffer, 0, size);

                            if (read > 0)
                            {
                                // Decrypts buffer.
                                outBuffer = Decrypt(inBuffer, key, IV, strength);
                                outStream.Write(outBuffer, 0, outBuffer.Length);

                                // Updates decryption progress.
                                if (progressHandler != null)
                                {
                                    total += read + lengthBuffer.Length;
                                    progressHandler(total, length);
                                }
                            }
                        }

                        // Reads the size of the next buffer from the stream.
                        read = inStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    }

                }

                /// <summary>Decrypts input stream onto output stream for the given parameters.</summary>
                public static void Decrypt(SymmetricAlgorithm algorithm, Stream inStream, Stream outStream, byte[] key, byte[] IV)
                {

                    // This is the root decryption function. Eventually, all the decryption functions perform their actual
                    // decryption here.
                    byte[] rgbKey = GetLegalKey(algorithm, key);
                    byte[] rgbIV = GetLegalIV(algorithm, IV);
                    CryptoStream decodeStream = new CryptoStream(outStream, algorithm.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                    byte[] buffer = new byte[BufferSize];
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
                public static void DecryptFile(string sourceFileName, string destFileName, EncryptLevel strength)
                {

                    DecryptFile(sourceFileName, destFileName, null, strength, null);

                }

                /// <summary>Creates a decrypted file from source file data.</summary>
                public static void DecryptFile(string sourceFileName, string destFileName, string encryptionKey, EncryptLevel strength, ProgressEventHandler progressHandler)
                {

                    if (string.IsNullOrEmpty(encryptionKey))
                    {
                        encryptionKey = StandardKey;
                    }

                    FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    FileStream destFileStream = File.Create(destFileName);
                    byte[] rgbKey = Encoding.ASCII.GetBytes(encryptionKey);
                    byte[] rgbIV = Encoding.ASCII.GetBytes(encryptionKey);

                    Decrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, strength, progressHandler);

                    destFileStream.Flush();
                    destFileStream.Close();
                    sourceFileStream.Close();

                }

                /// <summary>Coerces key to maximum legal bit length for given encryption algorithm.</summary>
                public static byte[] GetLegalKey(SymmetricAlgorithm algorithm, byte[] key)
                {

                    byte[] rgbKey = TVA.Common.CreateArray<byte>(algorithm.LegalKeySizes(0).MaxSize / 8);

                    for (int x = 0; x <= rgbKey.Length - 1; x++)
                    {
                        if (x < key.Length)
                        {
                            rgbKey[x] = key[x];
                        }
                        else
                        {
                            rgbKey[x] = Encoding.ASCII.GetBytes(StandardKey.Substring(x % StandardKey.Length, 1))[0];
                        }
                    }

                    return rgbKey;

                }

                /// <summary>Coerces initialization vector to legal block size for given encryption algorithm.</summary>
                public static byte[] GetLegalIV(SymmetricAlgorithm algorithm, byte[] IV)
                {

                    byte[] rgbIV = TVA.Common.CreateArray<byte>(algorithm.LegalBlockSizes(0).MinSize / 8);

                    for (int x = 0; x <= rgbIV.Length - 1; x++)
                    {
                        if (x < IV.Length)
                        {
                            rgbIV[x] = IV[IV.Length - 1 - x];
                        }
                        else
                        {
                            rgbIV[x] = Encoding.ASCII.GetBytes(StandardKey.Substring(x % StandardKey.Length, 1))[0];
                        }
                    }

                    return rgbIV;

                }

                /// <summary>Encrypts or decrypts input stream onto output stream using XOR based algorithms. Call once to
                /// encrypt, call again with same key to decrypt.</summary>
                public static Stream Crypt(Stream inStream, byte[] encryptionKey)
                {

                    if (inStream.CanSeek)
                    {
                        MemoryStream outStream = new MemoryStream();
                        byte[] buffer = TVA.Common.CreateArray<byte>(inStream.Length);

                        inStream.Read(buffer, 0, buffer.Length);
                        buffer = Crypt(buffer, encryptionKey);
                        outStream.Write(buffer, 0, buffer.Length);

                        outStream.Position = 0;
                        return outStream;
                    }
                    else
                    {
                        // For streams that can not be positioned (i.e., cannot obtain length), we copy all
                        // data onto a memory stream and try again.
                        MemoryStream outStream = new MemoryStream();
                        byte[] buffer = new byte[BufferSize];
                        int read;

                        read = inStream.Read(buffer, 0, BufferSize);

                        while (read > 0)
                        {
                            outStream.Write(buffer, 0, read);
                            read = inStream.Read(buffer, 0, BufferSize);
                        }

                        outStream.Position = 0;
                        return Crypt(outStream, encryptionKey);
                    }

                }

                /// <summary>Encrypts or decrypts data using XOR based algorithms. Call once to encrypt; call again with same
                /// key to decrypt.</summary>
                public static byte[] Crypt(byte[] data, byte[] encryptionKey)
                {

                    // The longer the encryption key, the better the encryption.
                    // Repeated encryption sequences do not occur for (3 * encryptionKey.Length) unique bytes.

                    byte[] cryptData = TVA.Common.CreateArray<byte>(data.Length);
                    long keyIndex;
                    int algorithm;
                    int cryptChar;

                    // Re-seeds random number generator.
                    VBMath.Rnd(-1);
                    VBMath.Randomize(encryptionKey[0]);

                    for (int x = 0; x <= data.Length - 1; x++)
                    {
                        cryptData[x] = data[x];
                        if (cryptData[x] > 0)
                        {
                            switch (algorithm)
                            {
                                case 0:
                                    cryptChar = encryptionKey[keyIndex];
                                    if (cryptData[x] != cryptChar)
                                    {
                                        cryptData[x] = cryptData[x] ^ cryptChar;
                                    }
                                    break;
                                case 1:
                                    cryptChar = Conversion.Int(VBMath.Rnd() * (encryptionKey[keyIndex] + 1));
                                    if (cryptData[x] != cryptChar)
                                    {
                                        cryptData[x] = cryptData[x] ^ cryptChar;
                                    }
                                    break;
                                case 2:
                                    cryptChar = Conversion.Int(VBMath.Rnd() * 256);
                                    if (cryptData[x] != cryptChar)
                                    {
                                        cryptData[x] = cryptData[x] ^ cryptChar;
                                    }
                                    break;
                            }
                        }

                        // Selects next encryption algorithm.
                        algorithm++;
                        if (algorithm == 3)
                        {
                            algorithm = 0;

                            // Selects next encryption key.
                            keyIndex++;
                            if (keyIndex > encryptionKey.Length - 1)
                            {
                                keyIndex = 0;
                            }
                        }
                    }

                    return cryptData;

                }

                /// <summary>Obfuscates input stream onto output stream using bit-rotation algorithms.</summary>
                public static Stream Obfuscate(Stream inStream, byte[] encryptionKey)
                {

                    if (inStream.CanSeek)
                    {
                        MemoryStream outStream = new MemoryStream();
                        byte[] buffer = TVA.Common.CreateArray<byte>(inStream.Length);

                        inStream.Read(buffer, 0, buffer.Length);
                        buffer = Obfuscate(buffer, encryptionKey);
                        outStream.Write(buffer, 0, buffer.Length);

                        outStream.Position = 0;
                        return outStream;
                    }
                    else
                    {
                        // For streams that cannot be positioned (i.e., cannot obtain length), we copy all
                        // data onto a memory stream and try again.
                        MemoryStream outStream = new MemoryStream();
                        byte[] buffer = new byte[BufferSize];
                        int read;

                        read = inStream.Read(buffer, 0, BufferSize);

                        while (read > 0)
                        {
                            outStream.Write(buffer, 0, read);
                            read = inStream.Read(buffer, 0, BufferSize);
                        }

                        outStream.Position = 0;
                        return Obfuscate(outStream, encryptionKey);
                    }

                }

                /// <summary>Obfuscates data using bit-rotation algorithms.</summary>
                public static byte[] Obfuscate(byte[] data, byte[] encryptionKey)
                {

                    byte key;
                    long keyIndex = encryptionKey.Length - 1;
                    byte[] cryptData = TVA.Common.CreateArray<byte>(data.Length);

                    // Starts bit rotation cycle.
                    for (int x = 0; x <= cryptData.Length - 1; x++)
                    {
                        // Gets current key value.
                        key = encryptionKey[keyIndex];

                        if (key % 2 == 0)
                        {
                            cryptData[x] = BitRotL(data[x], key);
                        }
                        else
                        {
                            cryptData[x] = BitRotR(data[x], key);
                        }

                        // Selects next encryption key index.
                        keyIndex--;
                        if (keyIndex < 0)
                        {
                            keyIndex = encryptionKey.Length - 1;
                        }
                    }

                    return cryptData;

                }

                /// <summary>Deobfuscates input stream onto output stream using bit-rotation algorithms.</summary>
                public static Stream Deobfuscate(Stream inStream, byte[] encryptionKey)
                {

                    if (inStream.CanSeek)
                    {
                        MemoryStream outStream = new MemoryStream();
                        byte[] buffer = TVA.Common.CreateArray<byte>(inStream.Length);

                        inStream.Read(buffer, 0, buffer.Length);
                        buffer = Deobfuscate(buffer, encryptionKey);
                        outStream.Write(buffer, 0, buffer.Length);

                        outStream.Position = 0;
                        return outStream;
                    }
                    else
                    {
                        // For streams that cannot be positioned (i.e., cannot obtain length), we copy all
                        // data onto a memory stream and try again.
                        MemoryStream outStream = new MemoryStream();
                        byte[] buffer = new byte[BufferSize];
                        int read;

                        read = inStream.Read(buffer, 0, BufferSize);

                        while (read > 0)
                        {
                            outStream.Write(buffer, 0, read);
                            read = inStream.Read(buffer, 0, BufferSize);
                        }

                        outStream.Position = 0;
                        return Deobfuscate(outStream, encryptionKey);
                    }

                }

                /// <summary>Deobfuscates data using bit-rotation algorithms.</summary>
                public static byte[] Deobfuscate(byte[] data, byte[] encryptionKey)
                {

                    byte key;
                    long keyIndex = encryptionKey.Length - 1;
                    byte[] cryptData = TVA.Common.CreateArray<byte>(data.Length);

                    // Starts bit rotation cycle.
                    for (int x = 0; x <= cryptData.Length - 1; x++)
                    {
                        // Gets current key value.
                        key = encryptionKey[keyIndex];

                        if (key % 2 == 0)
                        {
                            cryptData[x] = BitRotR(data[x], key);
                        }
                        else
                        {
                            cryptData[x] = BitRotL(data[x], key);
                        }

                        // Selects next encryption key index.
                        keyIndex--;
                        if (keyIndex < 0)
                        {
                            keyIndex = encryptionKey.Length - 1;
                        }
                    }

                    return cryptData;

                }

                /// <summary>Generates a random key useful for cryptographic functions.</summary>
                public static string GenerateKey()
				{
					
					char[] keyChars;
					char keyChar;
					int x;
					int y;
					
					// Generates a character array of unique values.
					System.Text.StringBuilder with_1 = new StringBuilder;
					with_1.Append(StandardKey);
					with_1.Append(Guid.NewGuid().ToString().ToLower().Replace("-", "©ª¦"));
					with_1.Append(DateTime.UtcNow.Ticks);
					for (x = 1; x <= 50; x++)
					{
						with_1.Append(Convert.ToChar(TVA.Math.Common.RandomInt32Between(33, 255)));
					}
					with_1.Append(Environment.MachineName);
					with_1.Append(GetKeyFromSeed(Microsoft.VisualBasic.DateAndTime.Timer));
					with_1.Append(Environment.UserDomainName);
					with_1.Append(Environment.UserName);
					with_1.Append(Microsoft.VisualBasic.DateAndTime.Timer);
					with_1.Append(DateTime.Now.ToString().Replace("/", "¡¤¥").Replace(" ", "°"));
					with_1.Append(Guid.NewGuid().ToString().ToUpper().Replace("-", "£§"));
					keyChars = with_1.ToString().ToCharArray();
					
					// Swaps values around in array at random.
					for (x = 0; x <= keyChars.Length - 1; x++)
					{
						y = TVA.Math.Common.RandomInt32Between(1, keyChars.Length) - 1;
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
					
					// This is a handy algorithm for encoding an integer value, use GetSeedFromKey to decode.
					int seedValue = System.Convert.ToInt32(seed);
					byte[] seedBytes = new byte[4];
					int alphaIndex;
					int asciiA = Strings.Asc('A');
					
					if (seedValue < 0)
					{
						throw (new ArgumentException("Cannot calculate key from negative seed"));
					}
					
					// Breaks seed into its component bytes.
					seedBytes[0] = LoByte(LoWord(seedValue));
					seedBytes[1] = HiByte(LoWord(seedValue));
					seedBytes[2] = LoByte(HiWord(seedValue));
					
					// Creates alpha-numeric key string.
					System.Text.StringBuilder with_1 = new StringBuilder;
					for (int x = 0; x <= 2; x++)
					{
						alphaIndex = TVA.Math.Common.RandomInt32Between(0, 25);
						if (x > 0)
						{
							with_1.Append('-');
						}
						with_1.Append(Convert.ToChar(asciiA + (25 - alphaIndex)));
						with_1.Append(seedBytes[x] + alphaIndex);
					}
					
					return with_1.ToString();
					
				}

                /// <summary>Returns the number from a string encoded with <see cref="GetKeyFromSeed" />.</summary>
                public static Int24 GetSeedFromKey(string key)
                {

                    if (string.IsNullOrEmpty(key))
                    {
                        throw (new ArgumentNullException("key", "key cannot be null"));
                    }

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
                                value = Conversion.Val(code.Substring(code.Length - code.Length - 1, code.Length - 1)) - (25 - (Strings.Asc(code.Substring(0, 1)) - Strings.Asc("A")));

                                // Validates calculation.
                                if (value >= 0 && value <= 255)
                                {
                                    seedBytes[x] = System.Convert.ToByte(value);
                                }
                                else
                                {
                                    // Determines the key is invalid and exits with -1.
                                    return -1;
                                }
                            }

                            // Creates seed from its component bytes.
                            return MakeDWord(MakeWord(0, seedBytes[2]), MakeWord(seedBytes[1], seedBytes[0]));
                        }
                    }

                    // Determines the key is invalid and exits with -1.
                    return -1;

                }

            }

        }
    }

}
