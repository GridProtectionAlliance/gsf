//*******************************************************************************************************
//  WaveDataChunk.cs
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
//  10/03/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;

namespace TVA.Media
{
    /// <summary>
    /// Represents the data chunk in a WAVE media format file.
    /// </summary>
    public class WaveDataChunk : RiffChunk
    {
        public const string RiffTypeID = "data";

        /// <summary>Reads a new WAVE format section from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed RIFF chunk header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <exception cref="InvalidOperationException">WAVE format or extra parameters section too small, wave file corrupted.</exception>
        public WaveDataChunk(RiffChunk preRead, Stream source)
            : base(preRead, RiffTypeID)
        {
            int length = BinaryLength - preRead.BinaryLength;
            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            if (bytesRead < length)
                throw new InvalidOperationException("WAVE data section too small, wave file corrupted.");

            Initialize(buffer, 0);

        }

        public override int Initialize(byte[] binaryImage, int startIndex)
        {
            return 0;
        }

        public override byte[] BinaryImage
        {
            get
            {
                byte[] binaryImage = new byte[BinaryLength];
                int startIndex = base.BinaryLength;

                Buffer.BlockCopy(base.BinaryImage, 0, binaryImage, 0, startIndex);

                // TODO: copy in data...

                return binaryImage;
            }
        }

        public override int BinaryLength
        {
            get
            {
                return ChunkSize;
            }
        }
    }
}
