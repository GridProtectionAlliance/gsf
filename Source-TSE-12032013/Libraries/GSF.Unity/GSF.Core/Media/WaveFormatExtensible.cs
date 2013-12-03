//******************************************************************************************************
//  WaveFormatExtensible.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/29/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
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

using GSF.Parsing;
using System;

namespace GSF.Media
{
    #region [ Enumerations ]

    /// <summary>Spatial positioning flags for <see cref="WaveFormatExtensible.ChannelMask"/> property.</summary>
    [Flags]
    public enum Speakers
    {
        /// <summary>Speaker positions undefined.</summary>
        Undefined = 0x0,
        /// <summary>Front left speaker position.</summary>
        FrontLeft = 0x1,
        /// <summary>Front right speaker position.</summary>
        FrontRight = 0x2,
        /// <summary>Front center speaker position.</summary>
        FronCenter = 0x4,
        /// <summary>Low frequency speaker.</summary>
        LowFrequency = 0x8,
        /// <summary>Back left speaker position.</summary>
        BackLeft = 0x10,
        /// <summary>back right speaker position.</summary>
        BackRight = 0x20,
        /// <summary>Front left of center speaker position.</summary>
        FronLeftOfCenter = 0x40,
        /// <summary>Front right of center speaker position.</summary>
        FronRightOfCenter = 0x80,
        /// <summary>Back center speaker position.</summary>
        BackCenter = 0x100,
        /// <summary>Side left speaker position.</summary>
        SideLeft = 0x200,
        /// <summary>Side right speaker position.</summary>
        SideRight = 0x400,
        /// <summary>Top center speaker position.</summary>
        TopCenter = 0x800,
        /// <summary>Top front left speaker position.</summary>
        TopFrontLeft = 0x1000,
        /// <summary>Top front center speaker position.</summary>
        TopFrontCenter = 0x2000,
        /// <summary>Top front right speaker position.</summary>
        TopFrontRight = 0x4000,
        /// <summary>Top back left speaker position.</summary>
        TopBackLeft = 0x8000,
        /// <summary>Top back center speaker position.</summary>
        TopBackCenter = 0x10000,
        /// <summary>Top back right speaker position.</summary>
        TopBackRight = 0x20000,
        /// <summary>Reserved flags for <see cref="Speakers"/> enumeration.</summary>
        Reserved = 0x7FFC0000,
        /// <summary>All Speaker positions defined.</summary>
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
    /// using GSF.Media;
    /// using GSF.Media.Sound;
    ///
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
    ///         // Save the generated tone
    ///         waveFile.Save("ExtensibleTest.wav");
    ///
    ///         Console.Write("File available to be played from Windows Media Player...");
    ///         Console.ReadKey();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class WaveFormatExtensible : ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// The fixed byte length of a <see cref="WaveFormatExtensible"/> instance.
        /// </summary>
        public const int FixedLength = 22;

        // Fields
        private ushort m_sampleValue;
        private Speakers m_channelMask;
        private Guid m_subFormat;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="WaveFormatExtensible"/>.
        /// </summary>
        public WaveFormatExtensible()
        {
        }

        /// <summary>
        /// Creates a new <see cref="WaveFormatExtensible"/> object based on the <see cref="WaveFormatChunk"/> settings.
        /// </summary>
        /// <param name="waveFormat">A <see cref="WaveFormatChunk"/> format.</param>
        public WaveFormatExtensible(WaveFormatChunk waveFormat)
        {
            m_sampleValue = (ushort)waveFormat.BitsPerSample;
            m_channelMask = Speakers.All;
            m_subFormat = DataFormatSubType.PCM;
        }

        /// <summary>
        /// Creates a new <see cref="WaveFormatExtensible"/> object based on the given settings.
        /// </summary>
        /// <param name="sampleValue">An <see cref="UInt16"/> value representing the sample value.</param>
        /// <param name="channelMask">A <see cref="Speakers"/> object.</param>
        /// <param name="subFormat">A <see cref="Guid"/> value.</param>
        [CLSCompliant(false)]
        public WaveFormatExtensible(ushort sampleValue, Speakers channelMask, Guid subFormat)
        {
            m_sampleValue = sampleValue;
            m_channelMask = channelMask;
            m_subFormat = subFormat;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets sample value.
        /// </summary>
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

        /// <summary>
        /// Gets or sets flags representing spatial locations of data channels (i.e., speaker locations).
        /// </summary>
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

        /// <summary>
        /// Gets or sets <see cref="Guid"/> for sub-format type of extensible WaveFile.
        /// </summary>
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

        /// <summary>
        /// Gets the length of the binary representation of this <see cref="WaveFormatExtensible"/> instance.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return FixedLength;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses <see cref="WaveFormatExtensible"/> object by parsing the specified <paramref name="buffer"/> containing a binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            m_sampleValue = EndianOrder.LittleEndian.ToUInt16(buffer, startIndex);
            m_channelMask = (Speakers)EndianOrder.LittleEndian.ToInt32(buffer, startIndex + 2);
            m_subFormat = EndianOrder.LittleEndian.ToGuid(buffer, startIndex + 6);

            return BinaryLength;
        }

        /// <summary>
        /// Generates a binary representation of this <see cref="WaveFormatExtensible"/> and copies it into the given buffer.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            buffer.ValidateParameters(startIndex, FixedLength);

            EndianOrder.LittleEndian.CopyBytes(m_sampleValue, buffer, 0);
            EndianOrder.LittleEndian.CopyBytes((int)m_channelMask, buffer, 2);
            EndianOrder.LittleEndian.CopyBytes(m_subFormat, buffer, 6);

            return FixedLength;
        }

        #endregion
    }
}
