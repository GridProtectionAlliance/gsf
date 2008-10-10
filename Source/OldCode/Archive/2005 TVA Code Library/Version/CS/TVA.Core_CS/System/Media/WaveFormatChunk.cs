/**************************************************************************\
   Copyright (c) 2008, James Ritchie Carroll
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

namespace System.Media
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    public class WaveFormatChunk : RiffChunk
    {
        #region [ Members ]

        // Constants
        public const string RiffTypeID = "fmt ";

        // Fields
        private ushort m_audioFormat;
        private short m_channels;
        private int m_sampleRate;
        private int m_byteRate;
        private short m_blockAlignment;
        private short m_bitsPerSample;
        private WaveFormatExtensible m_extensibleFormat;
        private short m_extraParametersSize;
        private byte[] m_extraParameters;

        #endregion

        #region [ Constructors ]

        [CLSCompliant(false)]
        public WaveFormatChunk(int sampleRate, short bitsPerSample, short channels, ushort audioFormat)
            : base(RiffTypeID)
        {
            if (bitsPerSample % 8 != 0)
                throw new InvalidDataException("Invalid bit rate specified - wave file bit rates must be a multiple of 8");

            m_sampleRate = sampleRate;
            m_bitsPerSample = bitsPerSample;
            m_channels = channels;
            AudioFormat = audioFormat;

            UpdateByteRate();
            UpdateBlockAlignment();

            // In order to get Windows Media Player to play a 24-bit encoded WAVE file, you have to use the
            // WaveFormatExtensible audio format, but even then the .NET Windows SoundPlayer will only play
            // PCM encoded files - bah...
            if (bitsPerSample == 24 && audioFormat == (short)WaveFormats.PCM)
                AudioFormat = (ushort)WaveFormats.WaveFormatExtensible;
        }

        /// <summary>Reads a new WAVE format section from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed RIFF chunk header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <exception cref="InvalidOperationException">WAVE format or extra parameters section too small, wave file corrupted.</exception>
        public WaveFormatChunk(RiffChunk preRead, Stream source)
            : base(preRead, RiffTypeID)
        {
            int length = ChunkSize;
            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            if (bytesRead < length)
                throw new InvalidOperationException("WAVE format section too small, wave file corrupted.");

            AudioFormat = EndianOrder.LittleEndian.ToUInt16(buffer, 0);
            m_channels = EndianOrder.LittleEndian.ToInt16(buffer, 2);
            m_sampleRate = EndianOrder.LittleEndian.ToInt32(buffer, 4);
            m_byteRate = EndianOrder.LittleEndian.ToInt32(buffer, 8);
            m_blockAlignment = EndianOrder.LittleEndian.ToInt16(buffer, 12);
            m_bitsPerSample = EndianOrder.LittleEndian.ToInt16(buffer, 14);

            if (m_bitsPerSample % 8 != 0)
                throw new InvalidDataException("Invalid bit rate encountered - wave file bit rates must be a multiple of 8");

            if (length > 16)
            {
                m_extraParametersSize = EndianOrder.LittleEndian.ToInt16(buffer, 16);

                // Read extra parameters, if any
                if (m_extraParametersSize > 0)
                {
                    m_extraParameters = new byte[m_extraParametersSize];

                    bytesRead = source.Read(m_extraParameters, 0, m_extraParametersSize);

                    if (bytesRead < m_extraParametersSize)
                        throw new InvalidOperationException("WAVE extra parameters section too small, wave file corrupted.");
                }
            }
        }

        #endregion

        #region [ Properties ]

        public override int ChunkSize
        {
            get
            {
                if (ExtraParametersSize > 0)
                    return 18 + ExtraParametersSize;
                else
                    return 16;
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
                int startIndex = base.BinaryLength;

                Buffer.BlockCopy(base.BinaryImage, 0, binaryImage, 0, startIndex);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_audioFormat), 0, binaryImage, startIndex, 2);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_channels), 0, binaryImage, startIndex + 2, 2);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_sampleRate), 0, binaryImage, startIndex + 4, 4);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_byteRate), 0, binaryImage, startIndex + 8, 4);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_blockAlignment), 0, binaryImage, startIndex + 12, 2);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_bitsPerSample), 0, binaryImage, startIndex + 14, 2);

                if (ExtraParametersSize > 0)
                {
                    Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(ExtraParametersSize), 0, binaryImage, startIndex + 16, 2);

                    if (ExtraParametersSize > 0)
                        Buffer.BlockCopy(ExtraParameters, 0, binaryImage, startIndex + 18, ExtraParametersSize);
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

        [CLSCompliant(false)]
        public ushort AudioFormat
        {
            get
            {
                return m_audioFormat;
            }
            set
            {
                m_audioFormat = value;

                // Create extensible format object if needed
                if (m_audioFormat == (ushort)WaveFormats.WaveFormatExtensible && m_extensibleFormat == null)
                    m_extensibleFormat = new WaveFormatExtensible(this);
            }
        }

        public short Channels
        {
            get
            {
                return m_channels;
            }
            set
            {
                m_channels = value;
                UpdateByteRate();
                UpdateBlockAlignment();
            }
        }

        public int SampleRate
        {
            get
            {
                return m_sampleRate;
            }
            set
            {
                m_sampleRate = value;
                UpdateByteRate();
            }
        }

        public short BlockAlignment
        {
            get
            {
                return m_blockAlignment;
            }
            set
            {
                m_blockAlignment = value;
            }
        }

        public short BitsPerSample
        {
            get
            {
                return m_bitsPerSample;
            }
            set
            {
                m_bitsPerSample = value;
                UpdateByteRate();
                UpdateBlockAlignment();
            }
        }

        /// <summary>Gets or sets new extensible extensible wave format object.</summary>
        public WaveFormatExtensible ExtensibleFormat
        {
            get
            {
                return m_extensibleFormat;
            }
            set
            {
                m_extensibleFormat = value;

                // If user is creating a new extensible format object, audio format must match...
                if (m_extensibleFormat != null && m_audioFormat != (ushort)WaveFormats.WaveFormatExtensible)
                    m_audioFormat = (ushort)WaveFormats.WaveFormatExtensible;
            }
        }

        /// <summary>Gets extra parameter size based on value in <see cref="ExtraParameters"/>.</summary>
        public short ExtraParametersSize
        {
            get
            {
                if (m_extensibleFormat == null)
                    return m_extraParametersSize;
                else
                    return (short)m_extensibleFormat.BinaryLength;
            }
        }

        /// <summary>
        /// When <see cref="ExtensibleFormat"/> is defined, this returns a binary image for the <see cref="WaveFormatExtensible"/> instance,
        /// otherwise this gets or sets custom set of extra parameters for Wave format chunk.
        /// </summary>
        public byte[] ExtraParameters
        {
            get
            {
                if (m_extensibleFormat == null)
                    return m_extraParameters;
                else
                    return m_extensibleFormat.BinaryImage;
            }
            set
            {
                if (value != null && m_extensibleFormat != null)
                    throw new InvalidOperationException("Cannot assign custom extra parameters when an ExtensibleFormat object is already defined.");

                m_extraParameters = value;

                if (m_extraParameters == null)
                    m_extraParametersSize = 0;
                else
                    m_extraParametersSize = (short)m_extraParameters.Length;
            }
        }

        #endregion

        #region [ Methods ]

        private void UpdateByteRate()
        {
            m_byteRate = m_sampleRate * m_channels * (m_bitsPerSample / 8);
        }

        private void UpdateBlockAlignment()
        {
            m_blockAlignment = (short)(m_channels * (m_bitsPerSample / 8));
        }

        #endregion
    }
}
