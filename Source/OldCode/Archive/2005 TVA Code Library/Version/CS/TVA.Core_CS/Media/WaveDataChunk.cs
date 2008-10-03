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
using System.Collections.Generic;
using TVA;
using TVA.Interop;

namespace TVA.Media
{
    /// <summary>
    /// Represents the data chunk in a WAVE media format file.
    /// </summary>
    public class WaveDataChunk : RiffChunk
    {
        public const string RiffTypeID = "data";

        private WaveFormatChunk m_waveFormat;
        private List<DataSample[]> m_dataSamples;

        /// <summary>Reads a new WAVE format section from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed RIFF chunk header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <exception cref="InvalidOperationException">WAVE format or extra parameters section too small, wave file corrupted.</exception>
        public WaveDataChunk(RiffChunk preRead, WaveFormatChunk waveFormat, Stream source)
            : base(preRead, RiffTypeID)
        {
            m_waveFormat = waveFormat;
            m_dataSamples = new List<DataSample[]>();

            int blockSize = waveFormat.BlockAlignent;
            int sampleSize = waveFormat.BitsPerSample / 8;
            byte[] buffer = new byte[blockSize];
            int channels = waveFormat.Channels;
            DataSample[] dataSamples;

            int bytesRead = source.Read(buffer, 0, blockSize);

            while (bytesRead == blockSize)
            {
                // Create a new data sample (one integer for each channel - most bits per sample will fit within a 32-bit integer)
                dataSamples = new DataSample[channels];

                for (int x = 0; x < waveFormat.Channels; x++)
                {
                    dataSamples[x] = new DataSample(buffer.CopyBuffer(x * sampleSize, sampleSize));
                }

                bytesRead = source.Read(buffer, 0, blockSize);
            }
            
        }

        public override int Initialize(byte[] binaryImage, int startIndex)
        {
            throw new NotImplementedException();
        }

        public List<DataSample[]> DataSamples
        {
            get
            {
                return m_dataSamples;
            }
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
                return base.BinaryLength + ChunkSize;
            }
        }
    }
}
