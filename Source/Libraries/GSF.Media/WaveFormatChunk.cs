//******************************************************************************************************
//  WaveFormatChunk.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/29/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/**************************************************************************\
   Copyright © 2009 - J. Ritchie Carroll
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

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GSF.Parsing;

namespace GSF.Media
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    public class WaveFormatChunk : RiffChunk, ISupportBinaryImage
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
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WaveFormatChunk(RiffChunk preRead, Stream source)
            : base(preRead, RiffTypeID)
        {
            int length = ChunkSize;
            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            // Initialize class from buffer
            ParseBinaryImage(buffer, 0, bytesRead);

            // Read extra parameters, if any
            if (m_extraParametersSize > 0)
            {
                m_extraParameters = new byte[m_extraParametersSize];

                bytesRead = source.Read(m_extraParameters, 0, m_extraParametersSize);

                if (bytesRead < m_extraParametersSize)
                    throw new InvalidOperationException("WAVE extra parameters section too small, wave file corrupted");
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>Size of <see cref="WaveFormatChunk"/>.</summary>
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

                return chunkSize;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets the length of <see cref="WaveFormatChunk"/>.
        /// </summary>
        public override int BinaryLength
        {
            get
            {
                return base.BinaryLength + ChunkSize;
            }
        }

        /// <summary>
        /// Gets or sets audio format used by the <see cref="WaveFile"/>.
        /// </summary>
        /// <remarks>
        /// PCM = 1 (i.e., linear quantization), values other than 1 typically indicate some form of compression.
        /// See <see cref="WaveFormat"/> enumeration for more details.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets number of audio channels in the <see cref="WaveFile"/>.
        /// </summary>
        /// <remarks>
        /// This property defines the number of channels (e.g., mono = 1, stereo = 2, etc.) defined
        /// in each sample block. See <see cref="DataChannels"/> enumeration for more details.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the sample rate (i.e., the number of samples per second) defined in the <see cref="WaveFile"/>.
        /// </summary>
        /// <remarks>
        /// This property defines the number of samples per second defined in each second of data in
        /// the <see cref="WaveFile"/>.  See <see cref="SampleRate"/> enumeration for more details.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the byte rate used for buffer estimation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is not usually changed.  It will be automatically calculated for new wave files.
        /// </para>
        /// <para>
        /// This is typically just the arithmetic result of:
        /// <see cref="SampleRate"/> * <see cref="Channels"/> * <see cref="BitsPerSample"/> / 8.
        /// However, this value can be changed as needed to accommodate better buffer estimations during
        /// data read cycle.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the block size of a complete sample of data (i.e., samples for all channels of data at
        /// one instant in time).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is not usually changed.  It will be automatically calculated for new wave files.
        /// </para>
        /// <para>
        /// This is typically just the arithmetic result of:
        /// <see cref="Channels"/> * <see cref="BitsPerSample"/> / 8.
        /// However, this value can be changed as needed to accommodate even block-alignment of non-standard
        /// <see cref="BitsPerSample"/> values.
        /// </para>
        /// </remarks>
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


        /// <summary>
        /// Gets or sets number of bits-per-sample in the <see cref="WaveFile"/>.
        /// </summary>
        /// <remarks>
        /// This property defines the number of bits-per-sample (e.g., 8, 16, 24, 32, etc.) used
        /// by each sample in a block of samples - effectively the data sample size. See
        /// <see cref="BitsPerSample"/> enumeration for more details.
        /// </remarks>
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

        /// <summary>
        /// Gets the size of the <see cref="ExtraParameters"/> buffer, if defined.
        /// </summary>
        public short ExtraParametersSize
        {
            get
            {
                return m_extraParametersSize;
            }
        }

        /// <summary>
        /// Gets or sets any extra parameters defined in the format header of the <see cref="WaveFile"/>.
        /// </summary>
        /// <remarks>
        /// See the <see cref="WaveFormatExtensible"/> class for an example of usage of this property.
        /// </remarks>
        public byte[] ExtraParameters
        {
            get
            {
                return m_extraParameters;
            }
            set
            {
                m_extraParameters = value;

                if ((object)m_extraParameters == null)
                    m_extraParametersSize = 0;
                else
                    m_extraParametersSize = (short)m_extraParameters.Length;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses <see cref="WaveFormatChunk"/> object by parsing the specified <paramref name="buffer"/> containing a binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <exception cref="InvalidOperationException">WAVE format section too small, wave file corrupted.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if (length < ChunkSize)
                throw new InvalidOperationException("WAVE format section too small, wave file corrupted");

            m_audioFormat = LittleEndian.ToUInt16(buffer, 0);
            m_channels = LittleEndian.ToInt16(buffer, 2);
            m_sampleRate = LittleEndian.ToInt32(buffer, 4);
            m_byteRate = LittleEndian.ToInt32(buffer, 8);
            m_blockAlignment = LittleEndian.ToInt16(buffer, 12);
            m_bitsPerSample = LittleEndian.ToInt16(buffer, 14);

            if (m_bitsPerSample % 8 != 0)
                throw new InvalidDataException("Invalid bit rate encountered - wave file bit rates must be a multiple of 8");

            if (length > 16)
            {
                m_extraParametersSize = LittleEndian.ToInt16(buffer, 16);
                return 18;
            }

            return 16;
        }

        /// <summary>
        /// Generates a binary representation of this <see cref="WaveFormatChunk"/> and copies it into the given buffer.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            int length = BinaryLength;

            buffer.ValidateParameters(startIndex, length);

            startIndex += base.GenerateBinaryImage(buffer, startIndex);
            LittleEndian.CopyBytes(m_audioFormat, buffer, startIndex);
            LittleEndian.CopyBytes(m_channels, buffer, startIndex + 2);
            LittleEndian.CopyBytes(m_sampleRate, buffer, startIndex + 4);
            LittleEndian.CopyBytes(m_byteRate, buffer, startIndex + 8);
            LittleEndian.CopyBytes(m_blockAlignment, buffer, startIndex + 12);
            LittleEndian.CopyBytes(m_bitsPerSample, buffer, startIndex + 14);
            startIndex += 16;

            if (m_extraParametersSize > 0)
            {
                LittleEndian.CopyBytes(m_extraParametersSize, buffer, startIndex);
                startIndex += 2;

                if (m_extraParametersSize > 0 && (object)m_extraParameters != null)
                {
                    Buffer.BlockCopy(m_extraParameters, 0, buffer, startIndex, m_extraParametersSize);
                    //startIndex += m_extraParametersSize;
                }
            }

            return length;
        }

        /// <summary>
        /// Creates a copy of the <see cref="WaveFormatChunk"/>.
        /// </summary>
        /// <returns>A new copy of the <see cref="WaveFormatChunk"/>.</returns>
        public new WaveFormatChunk Clone()
        {
            WaveFormatChunk waveFormatChunk = new WaveFormatChunk(m_sampleRate, m_bitsPerSample, m_channels, m_audioFormat);

            if ((object)m_extraParameters != null)
            {
                byte[] extraParameters = new byte[m_extraParameters.Length];
                Buffer.BlockCopy(m_extraParameters, 0, extraParameters, 0, m_extraParameters.Length);
                waveFormatChunk.ExtraParameters = extraParameters;
            }

            return waveFormatChunk;
        }

        /// <summary>
        /// Determines sample data type code based on defined <see cref="BitsPerSample"/>
        /// and <see cref="AudioFormat"/>.
        /// </summary>
        /// <returns>
        /// Sample type code based on defined <see cref="BitsPerSample"/> and
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

                    return TypeCode.Int32;
                case 64:
                    if (m_audioFormat == (ushort)WaveFormat.IeeeFloat)
                        return TypeCode.Double;

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

                    return (Int32)sample;
                case 64:
                    if (m_audioFormat == (ushort)WaveFormat.IeeeFloat)
                        return (Double)sample;

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
