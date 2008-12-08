//*******************************************************************************************************
//  Payload.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using PCS.IO.Compression;
using PCS.Security.Cryptography;

namespace PCS.Communication
{
    /// <summary>
    /// A helper class containing methods for manipulation of payload.
    /// </summary>
    public static class Payload
    {
        /// <summary>
        /// Specifies the length of the segment in a "Payload-Aware" transmission that contains the payload length.
        /// </summary>
        public const int LengthSegment = 4;

        /// <summary>
        /// Default byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.
        /// </summary>
        public static byte[] DefaultMarker = { 0xAA, 0xBB, 0xCC, 0xDD };

        /// <summary>
        /// Adds header containing the <paramref name="marker"/> to the payload in the <paramref name="buffer"/> for "Payload-Aware" transmission.
        /// </summary>
        /// <param name="buffer">The buffer containing the payload.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> at which the payload starts.</param>
        /// <param name="length">The lenght of the payload in the <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        public static void AddHeader(ref byte[] buffer, ref int offset, ref int length, byte[] marker)
        {
            // The resulting buffer will be at least 4 bytes bigger than the payload.

            // Resulting buffer = x bytes for payload marker + 4 bytes for the payload size + The payload
            byte[] result = new byte[(length - offset) + marker.Length + LengthSegment];

            // First, copy the the payload marker to the buffer.
            Buffer.BlockCopy(marker, 0, result, 0, marker.Length);
            // Then, copy the payload's size to the buffer after the payload marker.
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, result, marker.Length, LengthSegment);
            // At last, copy the payload after the payload marker and payload size.
            Buffer.BlockCopy(buffer, offset, result, marker.Length + LengthSegment, length);

            buffer = result;
            offset = 0;
            length = buffer.Length;
        }

        /// <summary>
        /// Determines whether or not the <paramref name="buffer"/> contains the header information of a "Payload-Aware" transmission.
        /// </summary>
        /// <param name="buffer">The buffer to be checked at index 0.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        /// <returns>true if the buffer contains "Payload-Aware" transmission header; otherwise false.</returns>
        public static bool HasHeader(byte[] buffer, byte[] marker)
        {
            for (int i = 0; i <= marker.Length - 1; i++)
            {
                if (buffer[i] != marker[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines the length of a payload in a "Payload-Aware" transmission from the payload header information.
        /// </summary>
        /// <param name="buffer">The buffer containg payload header information starting at index 0.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        /// <returns>Length of the payload.</returns>
        public static int ExtractLength(byte[] buffer, byte[] marker)
        {
            if (buffer.Length >= (marker.Length + LengthSegment) && HasHeader(buffer, marker))
            {
                // We have a buffer that's at least as big as the payload header and has the payload marker.
                return BitConverter.ToInt32(buffer, marker.Length);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Performs the necessary uncompression and decryption on the data contained in the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to be processed.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> from where data is to be processed.</param>
        /// <param name="length">The length of data in <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="cryptoLevel">One of the <see cref="CipherStrength"/> values.</param>
        /// <param name="cryptoKey">The key to be used for decrypting the data in the <paramref name="buffer"/>.</param>
        /// <param name="compressLevel">One of the <see cref="CompressionStrength"/> values.</param>
        public static void ProcessReceived(ref byte[] buffer, ref int offset, ref int length, CipherStrength cryptoLevel, string cryptoKey, CompressionStrength compressLevel)
        {
            if (cryptoLevel != CipherStrength.None)
            {
                buffer = buffer.Decrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
                offset = 0;
                length = buffer.Length;
            }

            if (compressLevel != CompressionStrength.NoCompression)
            {
                buffer = new MemoryStream(buffer, offset, length).Decompress().ToArray();
                offset = 0;
                length = buffer.Length;
            }

            //if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
            //{
            //    if (length - offset == data.Length)
            //        return data;
            //    else
            //        return data.BlockCopy(offset, length);
            //}
            //else
            //{
            //    if (cryptoLevel != CipherStrength.None)
            //    {
            //        data = data.Decrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
            //        offset = 0;
            //        length = data.Length;
            //    }

            //    if (compressLevel != CompressionStrength.NoCompression)
            //    {
            //        data = new MemoryStream(data, offset, length).Decompress().ToArray();
            //    }

            //    return data;
            //}
        }

        /// <summary>
        /// Performs the necessary compression and encryption on the data contained in the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to be processed.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> from where data is to be processed.</param>
        /// <param name="length">The length of data in <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="cryptoLevel">One of the <see cref="CipherStrength"/> values.</param>
        /// <param name="cryptoKey">The key to be used for encrypting the data in the <paramref name="buffer"/>.</param>
        /// <param name="compressLevel">One of the <see cref="CompressionStrength"/> values.</param>
        public static void ProcessTransmit(ref byte[] buffer, ref int offset, ref int length, CipherStrength cryptoLevel, string cryptoKey, CompressionStrength compressLevel)
        {
            if (compressLevel != CompressionStrength.NoCompression)
            {
                // Compress the data.
                buffer = new MemoryStream(buffer, offset, length).Compress(compressLevel).ToArray();
                offset = 0;
                length = buffer.Length;
            }

            if (cryptoLevel != CipherStrength.None)
            {
                // Encrypt the data.
                buffer = buffer.Encrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
                offset = 0;
                length = buffer.Length;
            }

            //if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
            //{
            //    if (length - offset == data.Length)
            //        return data;
            //    else
            //        return data.BlockCopy(offset, length);
            //}
            //else
            //{
            //    if (compressLevel != CompressionStrength.NoCompression)
            //    {
            //        // Compress the data.
            //        data = new MemoryStream(data, offset, length).Compress(compressLevel).ToArray();
            //        offset = 0;
            //        length = data.Length;
            //    }

            //    if (cryptoLevel != CipherStrength.None)
            //    {
            //        // Encrypt the data.
            //        data = data.Encrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
            //    }

            //    return data;
            //}
        }
    }
}

///// <summary>
///// Size of the header that is prepended to the payload. This header has information about the payload.
///// </summary>
//public const int HeaderSize = 8;

///// <summary>
///// A sequence of bytes that will mark the beginning of a payload.
///// </summary>
//public static byte[] BeginMarker = { 0xAA, 0xBB, 0xCC, 0xDD };

//public static byte[] AddHeader(byte[] payload)
//{
//    // The resulting buffer will be 8 bytes bigger than the payload.

//    // Resulting buffer = 4 bytes for payload marker + 4 bytes for the payload size + The payload
//    byte[] result = new byte[payload.Length + HeaderSize];

//    // First, copy the the payload marker to the buffer.
//    Buffer.BlockCopy(BeginMarker, 0, result, 0, 4);

//    // Then, copy the payload's size to the buffer after the payload marker.
//    Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, result, 4, 4);

//    // At last, copy the payload after the payload marker and payload size.
//    Buffer.BlockCopy(payload, 0, result, 8, payload.Length);

//    return result;
//}

//public static bool HasBeginMarker(byte[] data)
//{
//    for (int i = 0; i <= BeginMarker.Length - 1; i++)
//    {
//        if (data[i] != BeginMarker[i])
//            return false;
//    }

//    return true;
//}

//public static int GetSize(byte[] data)
//{
//    if (data.Length >= HeaderSize && HasBeginMarker(data))
//    {
//        // We have a buffer that's at least as big as the payload header and has the payload marker.
//        return BitConverter.ToInt32(data, BeginMarker.Length);
//    }
//    else
//    {
//        return -1;
//    }
//}

//public static byte[] Retrieve(byte[] data)
//{
//    if (data.Length > HeaderSize && HasBeginMarker(data))
//    {
//        int payloadSize = GetSize(data);

//        if (payloadSize > (data.Length - HeaderSize))
//            payloadSize = data.Length - HeaderSize;

//        return data.BlockCopy(HeaderSize, payloadSize);
//    }
//    else
//    {
//        return new byte[] { };
//    }
//}

//public static byte[] CompressData(byte[] data, int offset, int length, CompressionStrength compressionLevel)
//{
//    // Using streaming compression since needed uncompressed size will be serialized into destination stream
//    return new MemoryStream(data, offset, length).Compress(compressionLevel).ToArray();
//}

//public static byte[] DecompressData(byte[] data, CompressionStrength compressionLevel)
//{
//    // Using streaming decompression since needed uncompressed size was serialized into source stream
//    return new MemoryStream(data).Decompress().ToArray();
//}

//public static byte[] EncryptData(byte[] data, int offset, int length, string encryptionKey, CipherStrength encryptionLevel)
//{
//    byte[] key = Encoding.ASCII.GetBytes(encryptionKey);
//    return data.Encrypt(offset, length, key, key, encryptionLevel);
//}

//public static byte[] DecryptData(byte[] data, int offset, int length, string encryptionKey, CipherStrength encryptionLevel)
//{
//    byte[] key = Encoding.ASCII.GetBytes(encryptionKey);
//    return data.Decrypt(key, key, encryptionLevel);
//}