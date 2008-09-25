//*******************************************************************************************************
//  StreamExtensions.cs
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
//  09/19/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System.IO;

namespace TVA
{
    /// <summary>Defines extension functions related to stream manipulation.</summary>
    public static class StreamExtensions
    {
        private const int BufferSize = 32768;

        /// <summary>Copies input stream onto output stream.</summary>
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

        /// <summary>Reads entire stream contents, and returns byte array of data.</summary>
        /// <remarks>Note: You should only use this on streams where you know the data size is small.</remarks>
        public static byte[] ReadStream(this Stream source)
        {
            MemoryStream outStream = new MemoryStream();

            CopyStream(source, outStream);

            return outStream.ToArray();
        }

    }
}
