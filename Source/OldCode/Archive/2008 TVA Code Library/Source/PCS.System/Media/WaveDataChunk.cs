/**************************************************************************\
   Copyright (c) 2008 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

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
            TypeCode sampleTypeCode = m_waveFormat.GetSampleTypeCode();
            LittleBinaryValue[] sampleBlock;

            int bytesRead = source.Read(buffer, 0, blockSize);

            while (bytesRead == blockSize)
            {
                // Create a new sample block, one little-endian formatted binary sample value for each channel
                sampleBlock = new LittleBinaryValue[channels];

                for (int x = 0; x < channels; x++)
                {
                    sampleBlock[x] = new LittleBinaryValue(sampleTypeCode, buffer, x * sampleSize, sampleSize);
                }

                m_sampleBlocks.Add(sampleBlock);

                bytesRead = source.Read(buffer, 0, blockSize);
            }
        }

        #endregion

        #region [ Properties ]

        public WaveFormatChunk WaveFormat
        {
            get
            {
                return m_waveFormat;
            }
            set
            {
                m_waveFormat = value;
            }
        }

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

        #region [ Methods ]

        public new WaveDataChunk Clone()
        {
            WaveDataChunk waveDataChunk = new WaveDataChunk(m_waveFormat);
    
            // Deep clone sample blocks
            foreach (LittleBinaryValue[] samples in m_sampleBlocks)
            {
                waveDataChunk.SampleBlocks.Add(WaveFile.CloneSampleBlock(samples));
            }
    
            return waveDataChunk;
        }

        #endregion
    }
}
