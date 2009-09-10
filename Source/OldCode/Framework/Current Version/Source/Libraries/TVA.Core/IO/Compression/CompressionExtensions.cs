//*******************************************************************************************************
//  CompressionExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/26/2008 - James R. Carroll
//       Generated original version of source code.
//  08/05/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.IO;
using System.IO.Compression;

namespace TVA.IO.Compression
{
    #region [ Enumerations ]

    /// <summary>
    /// Level of compression enumeration.
    /// </summary>
    /// <remarks>
    /// Compression strengths represent tradeoffs on speed of compression vs. effectiveness of compression.
    /// </remarks>
    public enum CompressionStrength
    {
        /// <summary>Disables compression.</summary>
        NoCompression = 0,
        /// <summary>Enables standard compression.</summary>
        Standard = 1,
        /// <summary>Enables multi-pass compression to continue recompressing buffer as long as size continues to shrink.</summary>
        MultiPass = 2
    }

    #endregion

    /// <summary>
    /// Defines extension functions related to compression.
    /// </summary>
    public static class CompressionExtensions
    {
        /// <summary>
        /// Default compression buffer size.
        /// </summary>
        public const int BufferSize = 262144;

        /// <summary>
        /// Needed version of this library to uncompress stream (1.0.0 stored as byte 100).
        /// </summary>
        public const byte CompressionVersion = 100;
        
        /// <summary>
        /// Compress a byte array using standard compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to compress.</param>
        /// <returns>A compressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Compress(this byte[] source)
        {
	        return source.Compress(CompressionStrength.Standard);
        }

        /// <summary>
        /// Compress a byte array using specified compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to compress.</param>
        /// <param name="strength">The specified <see cref="CompressionStrength"/>.</param>
        /// <returns>A compressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Compress(this byte[] source, CompressionStrength strength)
        {
            return source.Compress(0, source.Length, strength, 0);
        }

        /// <summary>
        /// Compress a byte array using specified compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to compress.</param>
        /// <param name="length">The number of bytes to read into the byte array for compression.</param>
        /// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        /// <param name="strength">The specified <see cref="CompressionStrength"/>.</param>
        /// <returns>A compressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Compress(this byte[] source, int startIndex, int length, CompressionStrength strength)
        {
	        return source.Compress(startIndex, length, strength, 0);
        }

        // When user requests multi-pass compression, we allow multiple compression passes on a buffer because
        // this can often produce better compression results
        private static byte[] Compress(this byte[] source, int startIndex, int length, CompressionStrength strength, int compressionDepth)
        {
            if (strength == CompressionStrength.NoCompression)
            {
                // No compression requested, return specified portion of the buffer
                byte[] outBuffer = new byte[++length];
                outBuffer[0] = (byte)strength;

                for (int x = 1; x < length; x++)
                    outBuffer[x] = source[startIndex + x - 1];

                return outBuffer;
            }
            else
            {
                // Create a new compression deflater
                MemoryStream compressedData = new MemoryStream();
                DeflateStream deflater = new DeflateStream(compressedData, CompressionMode.Compress);

                // Provide data for compression
                deflater.Write(source, startIndex, length);
                deflater.Close();

                byte[] destination = compressedData.ToArray();
                int destinationLength = destination.Length;

                // Preprend compression depth and extract only used part of compressed buffer
                byte[] outBuffer = new byte[++destinationLength];
                
                // First two bits are reserved for compression strength - this leaves 6 bits for a maximum of 64 compressions
                outBuffer[0] = (byte)((compressionDepth << 2) | (int)strength);

                for (int x = 1; x < destinationLength; x++)
                    outBuffer[x] = destination[x - 1];

                if (strength == CompressionStrength.MultiPass && destinationLength < length && compressionDepth < 64)
                {
                    // See if another pass would help the compression...
                    byte[] testBuffer = outBuffer.Compress(0, outBuffer.Length, strength, compressionDepth + 1);

                    if (testBuffer.Length < outBuffer.Length)
                        return testBuffer;
                    else
                        return outBuffer;
                }
                else
                    return outBuffer;
            }
        }

        /// <summary>
        /// Compress a stream using specified compression strength.
        /// </summary>
        /// <remarks>
        /// This returns a memory stream of the compressed results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        /// <param name="source">The <see cref="Stream"/> to compress.</param>
        /// <param name="strength">The <see cref="CompressionStrength"/> of the compression.</param>
        /// <returns>Returns a <see cref="MemoryStream"/> of the compressed <see cref="Stream"/>.</returns>
        public static MemoryStream Compress(this Stream source, CompressionStrength strength)
        {
	        MemoryStream destination = new MemoryStream();
	        source.Compress(destination, strength, null);
	        return destination;
        }

        /// <summary>
        /// Compress a stream onto given output stream using specified compression strength.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> to compress.</param>
        /// <param name="strength">The <see cref="CompressionStrength"/> of the compression.</param>
        /// <param name="destination">The <see cref="Stream"/> destination.</param>
        /// <param name="progressHandler">The progress handler to check progress.</param>
        public static void Compress(this Stream source, Stream destination, CompressionStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer = new byte[BufferSize];
	        byte[] outBuffer;
	        byte[] lengthBuffer;
	        int read;
	        long total = 0, length = -1;

	        // Send initial progress event
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

                // Create a new progress handler to track compression progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Compress", length);
                progress.Complete = 0;
            }

	        // Read initial buffer
	        read = source.Read(inBuffer, 0, BufferSize);

	        // Write compression version into stream
	        byte[] version = new byte[1];
	        version[0] = CompressionVersion;
	        destination.Write(version, 0, 1);

	        while (read > 0)
	        {
		        // Compress buffer - note that we are only going to compress used part of buffer,
		        // we don't want any left over garbage to end up in compressed stream...
			    outBuffer = inBuffer.Compress(0, read, strength);

		        // The output stream is hopefully smaller than the input stream, so we prepend the final size of
		        // each compressed buffer into the destination output stream so that we can safely uncompress
		        // the stream in a "chunked" fashion later...
		        lengthBuffer = BitConverter.GetBytes(outBuffer.Length);
		        destination.Write(lengthBuffer, 0, lengthBuffer.Length);
		        destination.Write(outBuffer, 0, outBuffer.Length);

		        // Update compression progress
		        if (progressHandler != null)
		        {
			        total += read;
			        progress.Complete = total;
		        }

		        // Read next buffer
		        read = source.Read(inBuffer, 0, BufferSize);
	        }
        }

        /// <summary>
        /// Decompress a byte array.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to decompress.</param>
        /// <returns>A decompressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Decompress(this byte[] source)
        {
            return source.Decompress(0, source.Length);
        }

        /// <summary>
        /// Decompress a byte array.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to decompress.</param>
        /// <param name="length">The number of bytes to read into the byte array for compression.</param>
        /// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        /// <returns>A decompressed <see cref="Byte"/> array.</returns>
        public static byte[] Decompress(this byte[] source, int startIndex, int length)
        {
            const byte CompressionStrengthMask = (byte)(Bits.Bit00 | Bits.Bit01);

            // Unmask compression strength from first buffer byte
            CompressionStrength strength = (CompressionStrength)(source[startIndex] & CompressionStrengthMask);

            if (strength == CompressionStrength.NoCompression)
            {
                // No compression was applied to original buffer, return specified portion of the buffer
                return source.BlockCopy(startIndex + 1, length - 1);
            }
            else
            {
                // Create a new decompression deflater
                MemoryStream compressedData = new MemoryStream(source, startIndex + 1, length - 1);
                DeflateStream inflater = new DeflateStream(compressedData, CompressionMode.Decompress);
                int compressionDepth = (source[startIndex] & ~CompressionStrengthMask) >> 2;

                // Read uncompressed data
                byte[] destination = inflater.ReadStream();

                // When user requests muli-pass compression, there may be multiple compression passes on a buffer,
                // so we cycle through the needed uncompressions to get back to the original data
                if (strength == CompressionStrength.MultiPass && compressionDepth > 0)
                    return destination.Decompress();
                else
                    return destination;
            }
        }

        /// <summary>
        /// Decompress a stream.
        /// </summary>
        /// <remarks>
        /// This returns a memory stream of the uncompressed results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        /// <param name="source">A <see cref="Stream"/> source to decompress.</param>
        /// <returns>A <see cref="MemoryStream"/> representing the decompressed source.</returns>
        public static MemoryStream Decompress(this Stream source)
        {
	        MemoryStream destination = new MemoryStream();
	        source.Decompress(destination, null);
	        return destination;
        }

        /// <summary>
        /// Decompress a stream onto given output stream.
        /// </summary>
        /// <param name="source">A source <see cref="Stream"/> to decompress.</param>
        /// <param name="destination">The destination <see cref="Stream"/> to decompress to.</param>
        /// <param name="progressHandler">A <see cref="Action"/> handler to monitor the action's progress.</param>
        public static void Decompress(this Stream source, Stream destination, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer;
	        byte[] outBuffer;
	        byte[] lengthBuffer = BitConverter.GetBytes((int)0);
	        int read, size;
	        long total = 0, length = -1;

	        // Send initial progress event
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

                // Create a new progress handler to track compression progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Uncompress", length);
                progress.Complete = 0;
            }

	        // Read compression version from stream
	        byte[] versionBuffer = new byte[1];

	        if (source.Read(versionBuffer, 0, 1) > 0)
	        {
		        if (versionBuffer[0] != CompressionVersion)
			        throw new InvalidOperationException("Invalid compression version encountered in compressed stream - decompression aborted.");

		        // Read initial buffer
		        read = source.Read(lengthBuffer, 0, lengthBuffer.Length);

		        while (read > 0)
		        {
			        // Convert the byte array containing the buffer size into an integer
			        size = BitConverter.ToInt32(lengthBuffer, 0);

			        if (size > 0)
			        {
				        // Create and read the next buffer
				        inBuffer = new byte[size];
				        read = source.Read(inBuffer, 0, size);

				        if (read > 0)
				        {
					        // Uncompress buffer
					        outBuffer = inBuffer.Decompress();
					        destination.Write(outBuffer, 0, outBuffer.Length);
				        }

				        // Update decompression progress
				        if (progressHandler != null)
				        {
					        total += (read + lengthBuffer.Length);
					        progress.Complete = total;
				        }
			        }

			        // Read the size of the next buffer from the stream
			        read = source.Read(lengthBuffer, 0, lengthBuffer.Length);
		        }
	        }
        }
    }
}