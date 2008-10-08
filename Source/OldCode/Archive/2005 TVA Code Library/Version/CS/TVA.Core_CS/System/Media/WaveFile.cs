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
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace System.Media
{
    #region [ Enumerations ]

    /// <summary>
    /// Typical samples rates supported by PCM wave files.
    /// </summary>
    public enum SampleRate
    {
        /// <summary>8000 samples per second</summary>
        Hz8000 = 8000,
        /// <summary>11025 samples per second</summary>
        Hz11025 = 11025,
        /// <summary>12000 samples per second</summary>
        Hz12000 = 12000,
        /// <summary>16000 samples per second</summary>
        Hz16000 = 16000,
        /// <summary>22050 samples per second</summary>
        Hz22050 = 22050,
        /// <summary>24000 samples per second</summary>
        Hz24000 = 24000,
        /// <summary>32000 samples per second</summary>
        Hz32000 = 32000,
        /// <summary>44100 samples per second</summary>
        /// <remarks>This is the standard setting for CD audio quality</remarks>
        Hz44100 = 44100,
        /// <summary>48000 samples per second</summary>
        Hz48000 = 48000
    }

    /// <summary>
    /// Typical bit sizes supported by PCM wave files.
    /// </summary>
    public enum BitsPerSample : short
    {
        /// <summary>8-bits per sample</summary>
        Bits8 = 8,
        /// <summary>16-bits per sample</summary>
        /// <remarks>This is the standard setting for CD audio quality</remarks>
        Bits16 = 16,
        /// <summary>24-bits per sample</summary>
        Bits24 = 24,
        /// <summary>32-bits per sample</summary>
        Bits32 = 32,
    }

    /// <summary>
    /// Typical number of data channels used by PCM wave files.
    /// </summary>
    /// <remarks>
    /// These are some common number of data channels, but wave files can support any number of data channels.
    /// </remarks>
    public enum DataChannels : short
    {
        /// <summary>Defines a single (monaural) data sample channel</summary>
        Mono = 1,
        /// <summary>Defines two (stereo) data sample channels</summary>
        /// <remarks>This is the standard setting for CD audio quality</remarks>
        Stereo = 2,
        /// <summary>Defines three data sample channels</summary>
        /// <remarks>3.0 Channel Surround (analog matrixed: Dolby Surround)</remarks>
        DolbySurround = 3,
        /// <summary>Defines four data sample channels</summary>
        /// <remarks>4.0 Channel Surround (analog matrixed: Dolby Pro Logic)</remarks>
        DolbyProLogic = 4,
        /// <summary>Defines six data sample channels</summary>
        /// <remarks>5.1 Channel Surround (digital discrete: Dolby Digital, DTS, SDDS)</remarks>
        DolbyDigital = 6
    }

    /// <summary>
    /// Typical WAVE audio encoding formats.
    /// </summary>
    /// <remarks>
    /// Microsoft defines 133 different audio encoding formats for WAVE files.
    /// </remarks>
    public enum WaveFormats : short
    {
        /// <summary>Wave format type is undefined.</summary>
        UNKNOWN = 0x0,
        /// <summary>Standard pulse-code modulation audio format</summary>
        /// <remarks>
        /// PCM (Pulse Code Modulation) is a common method of storing and transmitting uncompressed digital audio.
        /// Since it is a generic format, it can be read by most audio applications—similar to the way a plain text
        /// file can be read by any word-processing program. PCM is used by Audio CDs and digital audio tapes (DATs).
        /// PCM is also a very common format for AIFF and WAV files.
        /// </remarks>
        PCM = 0x1,
        /// <summary>Adpative differential pulse-code modulation encoding algorithm</summary>
        ADPCM = 0x2,
        /// <summary>Floating point PCM encoding algorithm</summary>
        IEEE_FLOAT = 0x3,
        /// <summary>Vector sum excited linear prediction (VSELP) speech encoding algorithm</summary>
        VSELP = 0x4,
        /// <summary>Continuously variable slope delta modulation speech encoding algorithm</summary>
        IBM_CVSD = 0x5,
        /// <summary>A-law encoding algorithm (used in Europe and the rest of the world)</summary>
        ALAW = 0x6,
        /// <summary>μ-law encoding algorithm (used in North America and Japan)</summary>
        MULAW = 0x7,
        /// <summary>Decode Timestamp encoding algorithm (used in MPEG-coded multimedia)</summary>
        DTS = 0x8,                             // Microsoft Corporation    
        //DRM = 0x9,                           // Microsoft Corporation
        //OKI_ADPCM = 0x10,                    // OKI
        //DVI_ADPCM = 0x11,                    // Intel Corporation
        //IMA_ADPCM = 0x11,                    // Intel Corporation
        //MEDIASPACE_ADPCM = 0x12,             // Videologic
        //SIERRA_ADPCM = 0x13,                 // Sierra Semiconductor Corp
        //G723_ADPCM = 0x14,                   // Antex Electronics Corporation
        //DIGISTD = 0x15,                      // DSP Solutions, Inc.
        //DIGIFIX = 0x16,                      // DSP Solutions, Inc.
        //DIALOGIC_OKI_ADPCM = 0x17,           // Dialogic Corporation
        //MEDIAVISION_ADPCM = 0x18,            // Media Vision, Inc.
        //CU_CODEC = 0x19,                     // Hewlett-Packard Company
        //YAMAHA_ADPCM = 0x20,                 // Yamaha Corporation of America
        //SONARC = 0x21,                       // Speech Compression
        //DSPGROUP_TRUESPEECH = 0x22,          // DSP Group, Inc
        //ECHOSC1 = 0x23,                      // Echo Speech Corporation
        //AUDIOFILE_AF36 = 0x24,               // Virtual Music, Inc.
        //APTX = 0x25,                         // Audio Processing Technology
        //AUDIOFILE_AF10 = 0x26,               // Virtual Music, Inc.
        //PROSODY_1612 = 0x27,                 // Aculab plc
        //LRC = 0x28,                          // Merging Technologies S.A.
        //DOLBY_AC2 = 0x30,                    // Dolby Laboratories
        //GSM610 = 0x31,                       // Microsoft Corporation
        //MSNAUDIO = 0x32,                     // Microsoft Corporation
        //ANTEX_ADPCME = 0x33,                 // Antex Electronics Corporation
        //CONTROL_RES_VQLPC = 0x34,            // Control Resources Limited
        //DIGIREAL = 0x35,                     // DSP Solutions, Inc.
        //DIGIADPCM = 0x36,                    // DSP Solutions, Inc.
        //CONTROL_RES_CR10 = 0x37,             // Control Resources Limited
        //NMS_VBXADPCM = 0x38,                 // Natural MicroSystems
        //CS_IMAADPCM = 0x39,                  // Crystal Semiconductor IMA ADPCM
        //ECHOSC3 = 0x3A,                      // Echo Speech Corporation
        //ROCKWELL_ADPCM = 0x3B,               // Rockwell International
        //ROCKWELL_DIGITALK = 0x3C,            // Rockwell International
        //XEBEC = 0x3D,                        // Xebec Multimedia Solutions Limited
        //G721_ADPCM = 0x40,                   // Antex Electronics Corporation
        //G728_CELP = 0x41,                    // Antex Electronics Corporation
        //MSG723 = 0x42,                       // Microsoft Corporation
        /// <summary>MPEG Audio is a family of open standards for compressed audio that includes MP2, MP3 and AAC.</summary>
        MPEG = 0x50,                         // Microsoft Corporation
        //RT24 = 0x52,                         // InSoft, Inc.
        //PAC = 0x53,                          // InSoft, Inc.
        /// <summary>ISO MPEG-Layer 3 audio format.</summary>
        MPEGLAYER3 = 0x55                    // ISO/MPEG Layer3 Format Tag
        //LUCENT_G723 = 0x59,                  // Lucent Technologies
        //CIRRUS = 0x60,                       // Cirrus Logic
        //ESPCM = 0x61,                        // ESS Technology
        //VOXWARE = 0x62,                      // Voxware Inc
        //CANOPUS_ATRAC = 0x63,                // Canopus, co., Ltd.
        //G726_ADPCM = 0x64,                   // APICOM
        //G722_ADPCM = 0x65,                   // APICOM
        //DSAT_DISPLAY = 0x67,                 // Microsoft Corporation
        //VOXWARE_BYTE_ALIGNED = 0x69,         // Voxware Inc
        //VOXWARE_AC8 = 0x70,                  // Voxware Inc
        //VOXWARE_AC10 = 0x71,                 // Voxware Inc
        //VOXWARE_AC16 = 0x72,                 // Voxware Inc
        //VOXWARE_AC20 = 0x73,                 // Voxware Inc
        //VOXWARE_RT24 = 0x74,                 // Voxware Inc
        //VOXWARE_RT29 = 0x75,                 // Voxware Inc
        //VOXWARE_RT29HW = 0x76,               // Voxware Inc
        //VOXWARE_VR12 = 0x77,                 // Voxware Inc
        //VOXWARE_VR18 = 0x78,                 // Voxware Inc
        //VOXWARE_TQ40 = 0x79,                 // Voxware Inc
        //SOFTSOUND = 0x80,                    // Softsound, Ltd.
        //VOXWARE_TQ60 = 0x81,                 // Voxware Inc
        //MSRT24 = 0x82,                       // Microsoft Corporation
        //G729A = 0x83,                        // AT&T Labs, Inc.
        //MVI_MVI2 = 0x84,                     // Motion Pixels
        //DF_G726 = 0x85,                      // DataFusion Systems (Pty) (Ltd)
        //DF_GSM610 = 0x86,                    // DataFusion Systems (Pty) (Ltd)
        //ISIAUDIO = 0x88,                     // Iterated Systems, Inc.
        //ONLIVE = 0x89,                       // OnLive! Technologies, Inc.
        //SBC24 = 0x91,                        // Siemens Business Communications Sys
        //DOLBY_AC3_SPDIF = 0x92,              // Sonic Foundry
        //MEDIASONIC_G723 = 0x93,              // MediaSonic
        //PROSODY_8KBPS = 0x94,                // Aculab plc
        //ZYXEL_ADPCM = 0x97,                  // ZyXEL Communications, Inc.
        //PHILIPS_LPCBB = 0x98,                // Philips Speech Processing
        //PACKED = 0x99,                       // Studer Professional Audio AG
        //MALDEN_PHONYTALK = 0xA0,             // Malden Electronics Ltd.
        //RHETOREX_ADPCM = 0x100,              // Rhetorex Inc.
        //IRAT = 0x101,                        // BeCubed Software Inc.
        //VIVO_G723 = 0x111,                   // Vivo Software
        //VIVO_SIREN = 0x112,                  // Vivo Software
        //DIGITAL_G723 = 0x123,                // Digital Equipment Corporation
        //SANYO_LD_ADPCM = 0x125,              // Sanyo Electric Co., Ltd.
        //SIPROLAB_ACEPLNET = 0x130,           // Sipro Lab Telecom Inc.
        //SIPROLAB_ACELP4800 = 0x131,          // Sipro Lab Telecom Inc.
        //SIPROLAB_ACELP8V3 = 0x132,           // Sipro Lab Telecom Inc.
        //SIPROLAB_G729 = 0x133,               // Sipro Lab Telecom Inc.
        //SIPROLAB_G729A = 0x134,              // Sipro Lab Telecom Inc.
        //SIPROLAB_KELVIN = 0x135,             // Sipro Lab Telecom Inc.
        //G726ADPCM = 0x140,                   // Dictaphone Corporation
        //QUALCOMM_PUREVOICE = 0x150,          // Qualcomm, Inc.
        //QUALCOMM_HALFRATE = 0x151,           // Qualcomm, Inc.
        //TUBGSM = 0x155,                      // Ring Zero Systems, Inc.
        //MSAUDIO1 = 0x160,                    // Microsoft Corporation
        //UNISYS_NAP_ADPCM = 0x170,            // Unisys Corp.
        //UNISYS_NAP_ULAW = 0x171,             // Unisys Corp.
        //UNISYS_NAP_ALAW = 0x172,             // Unisys Corp.
        //UNISYS_NAP_16K = 0x173,              // Unisys Corp.
        //CREATIVE_ADPCM = 0x200,              // Creative Labs, Inc
        //CREATIVE_FASTSPEECH8 = 0x202,        // Creative Labs, Inc
        //CREATIVE_FASTSPEECH10 = 0x203,       // Creative Labs, Inc
        //UHER_ADPCM = 0x210,                  // UHER informatic GmbH
        //QUARTERDECK = 0x220,                 // Quarterdeck Corporation
        //ILINK_VC = 0x230,                    // I-link Worldwide
        //RAW_SPORT = 0x240,                   // Aureal Semiconductor
        //ESST_AC3 = 0x241,                    // ESS Technology, Inc.
        //IPI_HSX = 0x250,                     // Interactive Products, Inc.
        //IPI_RPELP = 0x251,                   // Interactive Products, Inc.
        //CS2 = 0x260,                         // Consistent Software
        //SONY_SCX = 0x270,                    // Sony Corp.
        //FM_TOWNS_SND = 0x300,                // Fujitsu Corp.
        //BTV_DIGITAL = 0x400,                 // Brooktree Corporation
        //QDESIGN_MUSIC = 0x450,               // QDesign Corporation
        //VME_VMPCM = 0x680,                   // AT&T Labs, Inc.
        //TPC = 0x681,                         // AT&T Labs, Inc.
        //OLIGSM = 0x1000,                     // Ing C. Olivetti & C., S.p.A.
        //OLIADPCM = 0x1001,                   // Ing C. Olivetti & C., S.p.A.
        //OLICELP = 0x1002,                    // Ing C. Olivetti & C., S.p.A.
        //OLISBC = 0x1003,                     // Ing C. Olivetti & C., S.p.A.
        //OLIOPR = 0x1004,                     // Ing C. Olivetti & C., S.p.A.
        //LH_CODEC = 0x1100,                   // Lernout & Hauspie
        //NORRIS = 0x1400,                     // Norris Communications, Inc.
        //SOUNDSPACE_MUSICOMPRESS = 0x1500,    // AT&T Labs, Inc.
        //DVM = 0x2000                         // FAST Multimedia AG
    }

    #endregion

    /// <summary>
    /// Represents a waveform audio format file (WAV).
    /// </summary>
    public class WaveFile
    {
        #region [ Waveform Format Structure ]

        // /* general waveform format structure (information common to all formats) */
        // typedef struct waveformat_tag {
        //     WORD    wFormatTag;        /* format type */
        //     WORD    nChannels;         /* number of channels (i.e. mono, stereo...) */
        //     DWORD   nSamplesPerSec;    /* sample rate */
        //     DWORD   nAvgBytesPerSec;   /* for buffer estimation */
        //     WORD    nBlockAlign;       /* block size of data */
        // } WAVEFORMAT;

        //Offset  Size  Name             Description
        //--------------------------------------------------------------------------------
        //The canonical WAVE format starts with the RIFF header:

        //0         4   ChunkID          Contains the letters "RIFF" in ASCII form
        //                               (0x52494646 big-endian form).
        //4         4   ChunkSize        36 + SubChunk2Size, or more precisely:
        //                               4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
        //                               This is the size of the rest of the chunk 
        //                               following this number.  This is the size of the 
        //                               entire file in bytes minus 8 bytes for the
        //                               two fields not included in this count:
        //                               ChunkID and ChunkSize.
        //8         4   Format           Contains the letters "WAVE"
        //                               (0x57415645 big-endian form).

        //The "WAVE" format consists of two subchunks: "fmt " and "data":
        //The "fmt " subchunk describes the sound data's format:

        //12        4   Subchunk1ID      Contains the letters "fmt "
        //                               (0x666d7420 big-endian form).
        //16        4   Subchunk1Size    16 for PCM.  This is the size of the
        //                               rest of the Subchunk which follows this number.
        //20        2   AudioFormat      PCM = 1 (i.e. Linear quantization)
        //                               Values other than 1 indicate some 
        //                               form of compression.
        //22        2   NumChannels      Mono = 1, Stereo = 2, etc.
        //24        4   SampleRate       8000, 44100, etc.
        //28        4   ByteRate         == SampleRate * NumChannels * BitsPerSample/8
        //32        2   BlockAlign       == NumChannels * BitsPerSample/8
        //                               The number of bytes for one sample including
        //                               all channels. I wonder what happens when
        //                               this number isn't an integer?
        //34        2   BitsPerSample    8 bits = 8, 16 bits = 16, etc.
        //          2   ExtraParamSize   if PCM, then doesn't exist
        //          X   ExtraParams      space for extra parameters

        //The "data" subchunk contains the size of the data and the actual sound:

        //36        4   Subchunk2ID      Contains the letters "data"
        //                               (0x64617461 big-endian form).
        //40        4   Subchunk2Size    == NumSamples * NumChannels * BitsPerSample/8
        //                               This is the number of bytes in the data.
        //                               You can also think of this as the size
        //                               of the read of the subchunk following this 
        //                               number.
        //44        *   Data             The actual sound data.

        #endregion

        #region [ Members ]

        // Fields
        private RiffHeaderChunk m_waveHeader;
        private WaveFormatChunk m_waveFormat;
        private WaveDataChunk m_waveData;

        #endregion

        #region [ Constructors ]

        /// <summary>Creates a new empty in-memory wave file using standard CD quality settings</summary>
        public WaveFile()
        {
            m_waveHeader = new RiffHeaderChunk("WAVE");
            m_waveFormat = new WaveFormatChunk(44100, 16, 2, 0x1);
            m_waveData = new WaveDataChunk(m_waveFormat);
        }

        /// <summary>Creates a new empty in-memory wave file in Pulse Code Modulation (PCM) audio format</summary>
        /// <param name="sampleRate">Desired sample rate</param>
        /// <param name="bitsPerSample">Desired bits-per-sample</param>
        /// <param name="channels">Desired data channels</param>
        public WaveFile(SampleRate sampleRate, BitsPerSample bitsPerSample, DataChannels channels)
            : this((int)sampleRate, (short)bitsPerSample, (short)channels, (short)WaveFormats.PCM)
        {
        }

        /// <summary>Creates a new empty in-memory wave file in specified audio format</summary>
        /// <param name="sampleRate">Desired sample rate</param>
        /// <param name="bitsPerSample">Desired bits-per-sample</param>
        /// <param name="channels">Desired data channels</param>
        /// <param name="audioFormat">Desired audio format</param>
        /// <remarks>Consumer will need to apply appropriate data compression for non-PCM data formats.</remarks>
        public WaveFile(SampleRate sampleRate, BitsPerSample bitsPerSample, DataChannels channels, WaveFormats audioFormat)
            : this((int)sampleRate, (short)bitsPerSample, (short)channels, (short)audioFormat)
        {
        }

        /// <summary>Creates a new empty in-memory wave file in Pulse Code Modulation (PCM) audio format</summary>
        /// <param name="sampleRate">Desired sample rate (e.g., 44100)</param>
        /// <param name="bitsPerSample">Desired bits-per-sample (e.g., 16)</param>
        /// <param name="channels">Desired data channels (e.g., 2 for stereo)</param>
        public WaveFile(int sampleRate, short bitsPerSample, short channels)
            : this(sampleRate, bitsPerSample, channels, (short)WaveFormats.PCM)
        {
        }

        /// <summary>Creates a new empty in-memory wave file in specified audio format</summary>
        /// <param name="sampleRate">Desired sample rate (e.g., 44100)</param>
        /// <param name="bitsPerSample">Desired bits-per-sample (e.g., 16)</param>
        /// <param name="channels">Desired data channels (e.g., 2 for stereo)</param>
        /// <param name="audioFormat">Desired audio format (e.g., 0x1 for Pulse Code Modulation)</param>
        /// <remarks>Consumer will need to apply appropriate data compression for non-PCM data formats.</remarks>
        public WaveFile(int sampleRate, short bitsPerSample, short channels, short audioFormat)
        {
            m_waveHeader = new RiffHeaderChunk("WAVE");
            m_waveFormat = new WaveFormatChunk(sampleRate, bitsPerSample, channels, audioFormat);
            m_waveData = new WaveDataChunk(m_waveFormat);
        }

        /// <summary>Creates a new empty in-memory wave file using existing constituent chunks</summary>
        public WaveFile(RiffHeaderChunk waveHeader, WaveFormatChunk waveFormat, WaveDataChunk waveData)
        {
            m_waveHeader = waveHeader;
            m_waveFormat = waveFormat;
            m_waveData = waveData;
        }

        #endregion

        #region [ Properties ]

        public short AudioFormat
        {
            get
            {
                return m_waveFormat.AudioFormat;
            }
            set
            {
                m_waveFormat.AudioFormat = value;
            }
        }

        public short Channels
        {
            get
            {
                return m_waveFormat.Channels;
            }
            set
            {
                m_waveFormat.Channels = value;
            }
        }

        public int SampleRate
        {
            get
            {
                return m_waveFormat.SampleRate;
            }
            set
            {
                m_waveFormat.SampleRate = value;
            }
        }

        public short BlockAlignment
        {
            get
            {
                return m_waveFormat.BlockAlignment;
            }
            set
            {
                m_waveFormat.BlockAlignment = value;
            }
        }

        public short BitsPerSample
        {
            get
            {
                return m_waveFormat.BitsPerSample;
            }
            set
            {
                m_waveFormat.BitsPerSample = value;
            }
        }

        public byte[] ExtraParameters
        {
            get
            {
                return m_waveFormat.ExtraParameters;
            }
            set
            {
                m_waveFormat.ExtraParameters = value;
            }
        }

        public List<LittleBinaryValue[]> SampleBlocks
        {
            get
            {
                return m_waveData.SampleBlocks;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a block of samples to the wave file.
        /// </summary>
        /// <param name="samples">Samples to add to the wave file.</param>
        /// <remarks>
        /// <para>
        /// You need to pass in one sample for each defined channel (e.g., if wave is configured for stereo
        /// you will need to pass in two parameters).
        /// </para>
        /// You should only add values that match the wave file's bits-per-sample (e.g., if wave file is
        /// configured for 16-bits only pass in Int16 values, casting if necessary).
        /// </remarks>
        public void AddBlock(params LittleBinaryValue[] samples)
        {
            // Validate number of samples
            if (samples.Length != m_waveFormat.Channels)
                throw new ArgumentOutOfRangeException("samples", "You must provide one sample for each defined channel.");

            int byteLength = m_waveFormat.BitsPerSample / 8;

            // Validate bit-lengths of samples
            foreach (LittleBinaryValue item in samples)
            {
                if (item.Buffer.Length != byteLength)
                    throw new ArrayTypeMismatchException(string.Format("One of the parameters is {0}-bits and wave is configured for {1}-bits per sample.", item.Buffer.Length * 8, m_waveFormat.BitsPerSample));
            }

            m_waveData.SampleBlocks.Add(samples);
        }

        public void Play()
        {
            MemoryStream stream = new MemoryStream();
            SoundPlayer player = new SoundPlayer(stream);
            Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            player.Play();
        }

        public void Save(string waveFileName)
        {
            FileStream stream = File.Create(waveFileName);
            Save(stream);
            stream.Close();
        }

        public void Save(Stream destination)
        {
            m_waveHeader.ChunkSize = 4 + m_waveFormat.BinaryLength + m_waveData.BinaryLength;
            destination.Write(m_waveHeader.BinaryImage, 0, m_waveHeader.BinaryLength);
            destination.Write(m_waveFormat.BinaryImage, 0, m_waveFormat.BinaryLength);
            destination.Write(m_waveData.BinaryImage, 0, m_waveData.BinaryLength);
        }
        
        public void Reverse()
        {
            m_waveData.SampleBlocks.Reverse();
        }

        #endregion

        #region [ Static ]

        /// <summary>Creates a new in-memory wave loaded from an existing wave file.</summary>
        /// <param name="waveFileName">File name of WAV file to load.</param>
        /// <returns>In-memory representation of wave file.</returns>
        public static WaveFile Load(string waveFileName)
        {
            FileStream source = File.Open(waveFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            RiffChunk riffChunk;
            RiffHeaderChunk waveHeader = null;
            WaveFormatChunk waveFormat = null;
            WaveDataChunk waveData = null;

            while (waveData == null)
            {
                riffChunk = RiffChunk.ReadNext(source);

                switch (riffChunk.TypeID)
                {
                    case RiffHeaderChunk.RiffTypeID:
                        waveHeader = new RiffHeaderChunk(riffChunk, source, "WAVE");
                        break;
                    case WaveFormatChunk.RiffTypeID:
                        if (waveHeader == null)
                            throw new InvalidDataException("WAVE format section encountered before RIFF header, wave file corrupted.");

                        waveFormat = new WaveFormatChunk(riffChunk, source);
                        break;
                    case WaveDataChunk.RiffTypeID:
                        if (waveFormat == null)
                            throw new InvalidDataException("WAVE data section encountered before format section, wave file corrupted.");

                        waveData = new WaveDataChunk(riffChunk, source, waveFormat);
                        break;
                    default:
                        break;
                }
            }

            return new WaveFile(waveHeader, waveFormat, waveData);
        }

        #endregion
    }
}
