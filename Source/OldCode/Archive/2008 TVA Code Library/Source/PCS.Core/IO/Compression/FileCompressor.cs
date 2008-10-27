//*******************************************************************************************************
//  FileCompressor.cs
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

namespace PCS.IO.Compression
{
    /// <summary>Performs basic compression and decompression on a file.</summary>
    public static class FileCompressor
    {
        /// <summary>
        /// Compress a file using default compression strength (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFilename">Source file name.</param>
        /// <param name="destinationFilename">Destination file name.</param>
        public static void Compress(String sourceFilename, String destinationFilename)
        {
            Compress(sourceFilename, destinationFilename, CompressionStrength.DefaultCompression);
        }

        /// <summary>
        /// Compress a file using specified compression strength (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFilename">Source file name.</param>
        /// <param name="destinationFilename">Destination file name.</param>
        /// <param name="strength">Stength of compression to apply.</param>
        public static void Compress(String sourceFilename, String destinationFilename, CompressionStrength strength)
        {
            Compress(sourceFilename, destinationFilename, strength, null);
        }

        /// <summary>
        /// Compress a file using specified compression strength (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFilename">Source file name.</param>
        /// <param name="destinationFilename">Destination file name.</param>
        /// <param name="strength">Stength of compression to apply.</param>
        /// <param name="progressHandler">User function to call with compression progress updates.</param>
        public static void Compress(String sourceFilename, String destinationFilename, CompressionStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFilename);

            sourceFileStream.Compress(destFileStream, strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }

        /// <summary>
        /// Uncompress a file compressed with CompressFile (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFilename">Source file name.</param>
        /// <param name="destinationFilename">Destination file name.</param>
        public static void Decompress(String sourceFilename, String destinationFilename)
        {
            Decompress(sourceFilename, destinationFilename, null);
        }

        /// <summary>
        /// Uncompress a file compressed with CompressFile given progress event handler (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFilename">Source file name.</param>
        /// <param name="destinationFilename">Destination file name.</param>
        /// <param name="progressHandler">User function to call with decompression progress updates.</param>
        public static void Decompress(String sourceFilename, String destinationFilename, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFilename);

            sourceFileStream.Decompress(destFileStream, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }
    }
}
