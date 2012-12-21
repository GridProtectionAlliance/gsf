//******************************************************************************************************
//  FileCompressor.cs - Gbtc
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
//  09/26/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;

namespace GSF.IO.Compression
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
