//*******************************************************************************************************
//  FileCompressor.cs
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
//
//*******************************************************************************************************

using System;
using System.IO;

namespace TVA.IO.Compression
{
    /// <summary>Performs basic compression and decompression on a file.</summary>
    public static class FileCompressor
    {
        /// <summary>
        /// Compress a file using default compression strength (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        public static void Compress(String sourceFileName, String destinationFileName)
        {
            Compress(sourceFileName, destinationFileName, CompressionStrength.Standard);
        }

        /// <summary>
        /// Compress a file using specified compression strength (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="strength">Stength of compression to apply.</param>
        public static void Compress(String sourceFileName, String destinationFileName, CompressionStrength strength)
        {
            Compress(sourceFileName, destinationFileName, strength, null);
        }

        /// <summary>
        /// Compress a file using specified compression strength (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="strength">Stength of compression to apply.</param>
        /// <param name="progressHandler">User function to call with compression progress updates.</param>
        public static void Compress(String sourceFileName, String destinationFileName, CompressionStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFileName);

            sourceFileStream.Compress(destFileStream, strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }

        /// <summary>
        /// Uncompress a file compressed with CompressFile (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        public static void Decompress(String sourceFileName, String destinationFileName)
        {
            Decompress(sourceFileName, destinationFileName, null);
        }

        /// <summary>
        /// Uncompress a file compressed with CompressFile given progress event handler (not PKZip compatible...)
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="progressHandler">User function to call with decompression progress updates.</param>
        public static void Decompress(String sourceFileName, String destinationFileName, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFileName);

            sourceFileStream.Decompress(destFileStream, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }
    }
}
