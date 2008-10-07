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

namespace System.Media
{
    /// <summary>
    /// Represents the data chunk in a WAVE media format file.
    /// </summary>
    public class WaveDataChunk : RiffChunk
    {
        #region [ Members ]

        // Constants
        public const string RiffTypeID = "data";

        // Fields
        private WaveFormatChunk m_waveFormat;
        private List<LittleBinaryValue[]> m_sampleBlocks;

        #endregion

        #region [ Constructors ]

        public WaveDataChunk(WaveFormatChunk waveFormat)
            : base(RiffTypeID)
        {
            m_waveFormat = waveFormat;
            m_sampleBlocks = new List<LittleBinaryValue[]>();
        }

        /// <summary>Reads a new WAVE format section from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed RIFF chunk header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <param name="waveFormat">Format of the data section to be parsed.</param>
        /// <exception cref="InvalidOperationException">WAVE format or extra parameters section too small, wave file corrupted.</exception>
        public WaveDataChunk(RiffChunk preRead, Stream source, WaveFormatChunk waveFormat)
            : base(preRead, RiffTypeID)
        {
            m_waveFormat = waveFormat;
            m_sampleBlocks = new List<LittleBinaryValue[]>();

            int blockSize = waveFormat.BlockAlignment;
            int sampleSize = waveFormat.BitsPerSample / 8;
            byte[] buffer = new byte[blockSize];
            int channels = waveFormat.Channels;
            LittleBinaryValue[] sampleBlock;

            int bytesRead = source.Read(buffer, 0, blockSize);

            while (bytesRead == blockSize)
            {
                // Create a new sample block, one binary sample value for each channel
                sampleBlock = new LittleBinaryValue[channels];

                for (int x = 0; x < channels; x++)
                {
                    sampleBlock[x] = new LittleBinaryValue(buffer, x * sampleSize, sampleSize);
                }

                m_sampleBlocks.Add(sampleBlock);

                bytesRead = source.Read(buffer, 0, blockSize);
            }
        }

        #endregion

        #region [ Properties ]

        public List<LittleBinaryValue[]> SampleBlocks
        {
            get
            {
                return m_sampleBlocks;
            }
        }

        public override int ChunkSize
        {
            get
            {
                return m_sampleBlocks.Count * m_waveFormat.BlockAlignment;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override byte[] BinaryImage
        {
            get
            {
                byte[] binaryImage = new byte[BinaryLength];
                int blockSize = m_waveFormat.BlockAlignment;
                int sampleSize = m_waveFormat.BitsPerSample / 8;
                int startIndex = base.BinaryLength;
                LittleBinaryValue[] sampleChannels;

                Buffer.BlockCopy(base.BinaryImage, 0, binaryImage, 0, startIndex);

                for (int block = 0; block < m_sampleBlocks.Count; block++)
                {
                    sampleChannels = m_sampleBlocks[block];

                    for (int sample = 0; sample < sampleChannels.Length; sample++)
                    {
                        Buffer.BlockCopy(sampleChannels[sample].Buffer, 0, binaryImage, startIndex + block * blockSize + sample * sampleSize, sampleSize);
                    }
                }

                return binaryImage;
            }
        }

        public new int BinaryLength
        {
            get
            {
                return base.BinaryLength + ChunkSize;
            }
        }

        #endregion
    }
}
