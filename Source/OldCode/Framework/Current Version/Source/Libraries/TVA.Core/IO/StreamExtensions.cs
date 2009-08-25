//*******************************************************************************************************
//  StreamExtensions.cs
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
//  09/19/2008 - James R. Carroll
//       Generated original version of source code.
//  10/24/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System.IO;

namespace TVA.IO
{
    /// <summary>
    /// Defines extension functions related to <see cref="Stream"/> manipulation.
    /// </summary>
    public static class StreamExtensions
    {
        private const int BufferSize = 32768;

        /// <summary>
        /// Copies input <see cref="Stream"/> onto output <see cref="Stream"/>.
        /// </summary>
        /// <param name="source">The input <see cref="Stream"/>.</param>
        /// <param name="destination">The output <see cref="Stream"/>.</param>
        public static void CopyStream(this Stream source, Stream destination)
        {
            byte[] buffer = new byte[BufferSize];
            int bytesRead = source.Read(buffer, 0, BufferSize);

            while (bytesRead > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                bytesRead = source.Read(buffer, 0, BufferSize);
            }
        }

        /// <summary>
        /// Reads entire <see cref="Stream"/> contents, and returns <see cref="byte"/> array of data.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> to be converted to <see cref="byte"/> array.</param>
        /// <returns>An array of <see cref="byte"/>.</returns>
        public static byte[] ReadStream(this Stream source)
        {
            MemoryStream outStream = new MemoryStream();

            source.CopyStream(outStream);

            return outStream.ToArray();
        }
    }
}
