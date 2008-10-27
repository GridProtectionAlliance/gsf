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

namespace System.Media
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    public class WaveFormatChunk : RiffChunk
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// RIFF type ID for wave format chunk (i.e., "fmt ").
        /// </summary>
        public const string RiffTypeID = "fmt ";

        // Fields
        private ushort m_audioFormat;
        private short m_channels;
        private int m_sampleRate;
        private int m_byteRate;
        private short m_blockAlignment;
        private short m_bitsPerSample;
        private short m_extraParametersSize;
        private byte[] m_extraParameters;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="WaveFormatChunk"/> using the specified audio parameters.
        /// </summary>
        /// <param name="sampleRate">Sample rate for the <see cref="WaveFormatChunk"/>.</param>
        /// <param name="bitsPerSample">Bits per sample for the <see cref="WaveFormatChunk"/>.</param>
        /// <param name="channels">Audio channels for the <see cref="WaveFormatChunk"/>.</param>
        /// <param name="audioFormat">Audio format for the <see cref="WaveFormatChunk"/>.</param>
        /// <exception cref="InvalidDataException">Invalid bit rate specified - wave file bit rates must be a multiple of 8.</exception>
        [CLSCompliant(false)]
        public WaveFormatChunk(int sampleRate, short bitsPerSample, short channels, ushort audioFormat)
            : base(RiffTypeID)
        {
            if (bitsPerSample % 8 != 0)
                throw new InvalidDataException("Invalid bit rate specified - wave file bit rates must be a multiple of 8");

            m_sampleRate = sampleRate;
            m_bitsPerSample = bitsPerSample;
            m_channels = channels;
            m_audioFormat = audioFormat;

            UpdateByteRate();
            UpdateBlockAlignment();
        }

        /// <summary>Reads a new <see cref="WaveFormatChunk"/> from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed <see cref="RiffChunk"/> header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <exception cref="InvalidOperationException">WAVE format or extra parameters section too small, wave file corrupted.</exception>
        /// <exception cref="InvalidDataException">Invalid bit rate encountered - wave file bit rates must be a multiple of 8.</exception>
        public WaveFormatChunk(RiffChunk preRead, Stream source)
            : base(preRead, RiffTypeID)
        {
            int length = ChunkSize;
            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            if (bytesRead < length)
                throw new InvalidOperationException("WAVE format section too small, wave file corrupted.");

            m_audioFormat = EndianOrder.LittleEndian.ToUInt16(buffer, 0);
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
                // Trust the read size over the typical constants if available
                int chunkSize = base.ChunkSize;

                if (chunkSize == 0)
                    chunkSize = 16;

                if (m_extraParametersSize > 0)
                    return chunkSize + 2 + m_extraParametersSize;
                else
                    return chunkSize;
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

                if (m_extraParametersSize > 0)
                {
                    Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_extraParametersSize), 0, binaryImage, startIndex + 16, 2);

                    if (m_extraParametersSize > 0 && m_extraParameters != null)
                        Buffer.BlockCopy(m_extraParameters, 0, binaryImage, startIndex + 18, m_extraParametersSize);
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
        
        public int ByteRate
        {
            get
            {
                return m_byteRate;
            }
            set
            {
                m_byteRate = value;
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

        public short ExtraParametersSize
        {
            get
            {
                return m_extraParametersSize;
            }
        }

        public byte[] ExtraParameters
        {
            get
            {
                return m_extraParameters;
            }
            set
            {
                m_extraParameters = value;

                if (m_extraParameters == null)
                    m_extraParametersSize = 0;
                else
                    m_extraParametersSize = (short)m_extraParameters.Length;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a copy of the <see cref="WaveFormatChunk"/>.
        /// </summary>
        /// <returns>A new copy of the <see cref="WaveFormatChunk"/>.</returns>
        public new WaveFormatChunk Clone()
        {
            WaveFormatChunk waveFormatChunk = new WaveFormatChunk(m_sampleRate, m_bitsPerSample, m_channels, m_audioFormat);
            waveFormatChunk.ExtraParameters = m_extraParameters;
            return waveFormatChunk;
        }

        /// <summary>
        /// Determines sample data type code based on defined <see cref="BitsPerSample"/>
        /// and <see cref="AudioFormat"/>.
        /// </summary>
        /// <returns>
        /// Sample type code based on  defined <see cref="BitsPerSample"/> and
        /// <see cref="AudioFormat"/>.
        /// </returns>
        public TypeCode GetSampleTypeCode()
        {
            // Determine sample data type based on bit size and audio format
            switch (m_bitsPerSample)
            {
                case 8:
                    return TypeCode.Byte;
                case 16:
                    return TypeCode.Int16;
                case 24:
                    // .NET does not define an Int24 type code and since an Int24 will
                    // fit inside an Int32, the Int32 type code is returned.
                    return TypeCode.Int32;
                case 32:
                    if (m_audioFormat == (ushort)WaveFormat.IeeeFloat)
                        return TypeCode.Single;
                    else
                        return TypeCode.Int32;
                case 64:
                    if (m_audioFormat == (ushort)WaveFormat.IeeeFloat)
                        return TypeCode.Double;
                    else
                        return TypeCode.Int64;
                default:
                    // Unable to determine proper type code, consumer may be using a special data format...
                    return TypeCode.Empty;
            }
        }

        /// <summary>
        /// Casts sample value to its equivalent native type based on defined <see cref="BitsPerSample"/>
        /// and <see cref="AudioFormat"/>.
        /// </summary>
        /// <param name="sample">Sample value.</param>
        /// <returns>
        /// Sample value cast to its equivalent native type based on defined <see cref="BitsPerSample"/>
        /// and <see cref="AudioFormat"/>.
        /// </returns>
        public LittleBinaryValue CastSample(double sample)
        {
            // Cast sample value to appropriate data type based on bit size
            switch (m_bitsPerSample)
            {
                case 8: // Bytes are unsigned and need 128 byte offset
                    return (Byte)(sample + 128);
                case 16:
                    return (Int16)sample;
                case 24:
                    return (Int24)sample;
                case 32:
                    if (m_audioFormat == (ushort)WaveFormat.IeeeFloat)
                        return (Single)sample;
                    else
                        return (Int32)sample;
                case 64:
                    if (m_audioFormat == (ushort)WaveFormat.IeeeFloat)
                        return (Double)sample;
                    else
                        return (Int64)sample;
                default:
                    throw new InvalidOperationException(string.Format("Cannot cast sample \'{0}\' into {1}-bits.", sample, BitsPerSample));
            }
        }

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
