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
    #region [ Enumerations ]

    /// <summary>Spatial positioning flags for <see cref="WaveFormatExtensible.ChannelMask"/> property.</summary>
    [Flags]
    public enum Speakers
    {
        Undefined = 0x0,
        FrontLeft = 0x1,
        FrontRight = 0x2,
        FronCenter = 0x4,
        LowFrequency = 0x8,
        BackLeft = 0x10,
        BackRight = 0x20,
        FronLeftOfCenter = 0x40,
        FronRightOfCenter = 0x80,
        BackCenter = 0x100,
        SideLeft = 0x200,
        SideRight = 0x400,
        TopCenter = 0x800,
        TopFrontLeft = 0x1000,
        TopFrontCenter = 0x2000,
        TopFrontRight = 0x4000,
        TopBackLeft = 0x8000,
        TopBackCenter = 0x10000,
        TopBackRight = 0x20000,
        Reserved = 0x7FFC0000,
        All = -2147483648 // 0x80000000
    }

    /// <summary>Common sub-type GUID's for <see cref="WaveFormatExtensible.SubFormat"/> property.</summary>
    public static class DataFormatSubType
    {
        /// <summary>Standard pulse-code modulation audio format</summary>
        /// <remarks>
        /// PCM (Pulse Code Modulation) is a common method of storing and transmitting uncompressed digital audio.
        /// Since it is a generic format, it can be read by most audio applications—similar to the way a plain text
        /// file can be read by any word-processing program. PCM is used by Audio CDs and digital audio tapes (DATs).
        /// PCM is also a very common format for AIFF and WAV files.
        /// </remarks>
        public static Guid PCM = new Guid("00000001-0000-0010-8000-00aa00389b71");

        /// <summary>Adpative differential pulse-code modulation encoding algorithm</summary>
        public static Guid ADPCM = new Guid("00000002-0000-0010-8000-00aa00389b71");

        /// <summary>Floating point PCM encoding algorithm</summary>
        public static Guid IeeeFloat = new Guid("00000003-0000-0010-8000-00aa00389b71");

        /// <summary>Digital Rights Management encoded format (for digital-audio content protected by Microsoft DRM).</summary>
        public static Guid DRM = new Guid("00000009-0000-0010-8000-00aa00389b71");

        /// <summary>A-law encoding algorithm (used in Europe and the rest of the world)</summary>
        public static Guid ALaw = new Guid("00000006-0000-0010-8000-00aa00389b71");

        /// <summary>μ-law encoding algorithm (used in North America and Japan)</summary>
        public static Guid MuLaw = new Guid("00000007-0000-0010-8000-00aa00389b71");

        /// <summary>MPEG Audio is a family of open standards for compressed audio that includes MP2, MP3 and AAC.</summary>
        public static Guid Mpeg = new Guid("00000050-0000-0010-8000-00aa00389b71");

        /// <summary>Analog sub-format.</summary>
        public static Guid Analog = new Guid("6dba3190-67bd-11cf-a0f7-0020afd156e4");
    }

    #endregion
    
    /// <summary>
    /// Represents the "extensible" format structure for a WAVE media format file.
    /// </summary>
    /// <example>
    /// For some special bit-encodings you may need to use the "WaveFormatExtensible" audio format,
    /// here is an example of how to use that format:
    /// <code>
    /// using System;
    /// using System.Media;
    /// using System.Media.Sound;

    /// static class Program
    /// {
    ///     static void Main()
    ///     {
    ///         // Generate an 8000 Hz, 32 bits per sample, mono channeled WAVE file in "Extensible" format
    ///         WaveFile waveFile = new WaveFile(8000, 32, 1, (ushort)WaveFormat.WaveFormatExtensible);
    ///
    ///         // Apply the "WaveFormatExtensible" extra parameters
    ///         WaveFormatExtensible extensible = new WaveFormatExtensible(waveFile.FormatChunk);
    ///         waveFile.ExtraParameters = extensible.BinaryImage;
    ///
    ///         // Generate the EBS Alert noise
    ///         DTMF.Generate(waveFile, DTMF.EmergencyBroadcastSystemAlert, 0.25D);
    ///
    ///         // Play all the generated tones
    ///         waveFile.Save("ExtensibleTest.wav");
    ///
    ///         Console.Write("File available to be played from Windows Media Player...");
    ///         Console.ReadKey();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class WaveFormatExtensible
    {
        #region [ Members ]

        // Fields
        private WaveFormatChunk m_waveFormat;
        private ushort m_sampleValue;
        private Speakers m_channelMask;
        private Guid m_subFormat;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Generate a basic extensible object based on the <see cref="WaveFormatChunk"/> settings.
        /// </summary>
        public WaveFormatExtensible(WaveFormatChunk waveFormat)
        {
            m_waveFormat = waveFormat;
            m_sampleValue = (ushort)m_waveFormat.BitsPerSample;
            m_channelMask = Speakers.All;
            m_subFormat = DataFormatSubType.PCM;
        }

        /// <summary>
        /// Generate an extensible object based on the given settings.
        /// </summary>
        [CLSCompliant(false)]
        public WaveFormatExtensible(WaveFormatChunk waveFormat, ushort sampleValue, Speakers channelMask, Guid subFormat)
        {
            m_sampleValue = sampleValue;
            m_channelMask = channelMask;
            m_subFormat = subFormat;
        }

        #endregion

        #region [ Properties ]

        /// <summary>Gets or sets sample value.</summary>
        [CLSCompliant(false)]
        public ushort SampleValue
        {
            get
            {
                return m_sampleValue;
            }
            set
            {
                m_sampleValue = value;
            }
        }

        /// <summary>Gets or sets flags representing spatial locations of data channels (i.e., speaker locations).</summary>
        public Speakers ChannelMask
        {
            get
            {
                return m_channelMask;
            }
            set
            {
                m_channelMask = value;
            }
        }

        /// <summary>Gets or sets GUID for sub-format type of extensible WaveFile.</summary>
        public Guid SubFormat
        {
            get
            {
                return m_subFormat;
            }
            set
            {
                m_subFormat = value;
            }
        }

        public byte[] BinaryImage
        {
            get
            {
                byte[] binaryImage = new byte[BinaryLength];

                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(m_sampleValue), 0, binaryImage, 0, 2);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes((int)m_channelMask), 0, binaryImage, 2, 4);
                Buffer.BlockCopy(m_subFormat.ToByteArray(), 0, binaryImage, 6, 16);               

                return binaryImage;
            }
        }

        public int BinaryLength
        {
            get
            {
                return 22;
            }
        }

        #endregion
    }
}
