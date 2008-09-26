//*******************************************************************************************************
//  CompressionExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/26/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using TVA.IO.Compression.Zip.Algorithms;

namespace TVA.IO.Compression
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the level of compression to be performed on data.
    /// </summary>
    /// <remarks>
    /// Compression strengths represent tradeoffs on speed of compression vs. effectiveness of compression.
    /// </remarks>
    public enum CompressionStrength
    {
        /// <summary>Use default compression (balanced mix of strength vs. speed).</summary>
        DefaultCompression = -1,
        /// <summary>Turns off compression.</summary>
        NoCompression = 0,
        /// <summary>Enables best speed.</summary>
        BestSpeed = 1,
        /// <summary>Enables best compression.</summary>
        BestCompression = 9,
        /// <summary>Enables multi-pass compression (using BestCompression level) to continue recompressing buffer as long as size continues to shrink.</summary>
        MultiPass = 10
    }

    #endregion

    /// <summary>Defines extension functions related to compression.</summary>
    public static class CompressionExtensions
    {
        // A 256K buffer size produces very good compression, slightly better than WinZip (~2%) when using the
        // CompressFile function with CompressLevel.BestCompression.  To achieve best results zlib needs a
        // sizeable buffer to work with, however when these buffers are needed in the code they are created on
        // the garbage collected heap and used as briefly as possible.  Even so, you may want to reduce this
        // buffer size if you intend on running this code on an embedded device...
        public const int BufferSize = 262144;

        /// <summary>Needed version of this library to uncompress stream (1.0.0 stored as byte 100).</summary>
        public const byte CompressionVersion = 100;


        /// <summary>Compress a byte array using default compression strength.</summary>
        public static byte[] Compress(this byte[] source)
        {
	        return source.Compress(CompressionStrength.DefaultCompression);
        }

        /// <summary>Compress a byte array using specified compression strength.</summary>
        public static byte[] Compress(this byte[] source, CompressionStrength strength)
        {
            return source.Compress(0, source.Length, strength, 0);
        }

        /// <summary>Compress a byte array using specified compression strength.</summary>
        public static byte[] Compress(this byte[] source, int startIndex, int length, CompressionStrength strength)
        {
	        return source.Compress(startIndex, length, strength, 0);
        }

        // When user requests multi-pass compression, we allow multiple compression passes on a buffer because
        // this can often produce better compression results
        private static byte[] Compress(this byte[] source, int startIndex, int length, CompressionStrength strength, int compressionDepth)
        {
	        // zlib requests destination buffer to be 0.1% and 12 bytes larger than source stream...
            int destinationLength = length + (int)(length * 0.001) + 12;
	        byte[] destination = new byte[destinationLength];

            // Create a new zip deflater
            Deflater deflater = new Deflater(strength > CompressionStrength.BestCompression ? CompressionStrength.BestCompression : strength);

            deflater.SetInput(source, startIndex, length);
            deflater.Finish();
            destinationLength = deflater.Deflate(destination);
        
	        // Preprend compression depth and extract only used part of compressed buffer
	        byte[] outBuffer = new byte[++destinationLength];	
	        outBuffer[0] = (byte)compressionDepth;

	        for (int x = 1; x < destinationLength; x++)
		        outBuffer[x] = destination[x - 1];

            if (strength == CompressionStrength.MultiPass && destinationLength < length && compressionDepth < 256)
	        {
		        // See if another pass would help the compression...
                byte[] testBuffer = Compress(outBuffer, 0, outBuffer.Length, strength, compressionDepth + 1);

		        if (testBuffer.Length < outBuffer.Length)
			        return testBuffer;
		        else
			        return outBuffer;
	        }
	        else
		        return outBuffer;
        }

        /// <summary>Compress a stream using specified compression strength</summary>
        /// <remarks>
        /// This returns a memory stream of the compressed results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Compress(this Stream source, CompressionStrength strength)
        {
	        MemoryStream destination = new MemoryStream();
	        Compress(source, destination, strength, null);
	        return destination;
        }

        // Compress a stream onto given output stream using specified compression strength
        public static void Compress(this Stream source, Stream destination, CompressionStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgress<long>.Handler progress = null;
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
                progress = new ProcessProgress<long>.Handler(progressHandler, "Compress", length);
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
			    outBuffer = Compress(inBuffer, 0, read, strength);

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

        // Uncompress a byte array
        public static byte[] Decompress(this byte[] source, int uncompressedSize)
        {
	        // Uncompressed buffer size is requested because we must allocate a buffer large enough to hold resultant uncompressed
	        // data and user will have a better idea of what this will be since they compressed the original data
            int destinationLength;
	        byte[] destination = new byte[uncompressedSize];

            Inflater inflater = new Inflater();
            inflater.SetInput(source, 1, source.Length -1); // Skip compression depth marker
            destinationLength = inflater.Inflate(destination);

            // Extract only used part of compressed buffer
            if (destinationLength != uncompressedSize)
                destination = destination.CopyBuffer(0, destinationLength);

	        // When user requests muli-pass compression, there may be multiple compression passes on a buffer,
	        // so we cycle through the needed uncompressions to get back to the original data
	        if (source[0] > 0)
                return Decompress(destination, uncompressedSize);
	        else
                return destination;
        }

        /// <summary>Uncompress a stream.</summary>
        /// <remarks>
        /// This returns a memory stream of the uncompressed results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Decompress(this Stream source)
        {
	        MemoryStream destination = new MemoryStream();
	        Decompress(source, destination, null);
	        return destination;
        }

        // Uncompress a stream onto given output stream
        public static void Decompress(this Stream source, Stream destination, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgress<long>.Handler progress = null;
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
                progress = new ProcessProgress<long>.Handler(progressHandler, "Uncompress", length);
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
					        outBuffer = Decompress(inBuffer, BufferSize);
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