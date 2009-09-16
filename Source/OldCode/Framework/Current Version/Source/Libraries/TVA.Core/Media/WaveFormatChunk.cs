//*******************************************************************************************************
//  WaveFormatChunk.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/29/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

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
using System.IO;
using TVA.Parsing;

namespace TVA.Media
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

            // Initialize class from buffer
            Initialize(buffer, 0, bytesRead);

            // Read extra parameters, if any
            if (m_extraParametersSize > 0)
            {
                m_extraParameters = new byte[m_extraParametersSize];

                bytesRead = source.Read(m_extraParameters, 0, m_extraParametersSize);

                if (bytesRead < m_extraParametersSize)
                    throw new InvalidOperationException("WAVE extra parameters section too small, wave file corrupted.");
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
                else
                    return chunkSize;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a binary representation of this <see cref="WaveFormatChunk"/>.
        /// </summary>
        public override byte[] BinaryImage
        {
            get
            {
                byte[] binaryImage = new byte[BinaryLength];
                int startIndex = base.BinaryLength;

                Buffer.BlockCopy(base.BinaryImage, 0, binaryImage, 0, startIndex);
                EndianOrder.LittleEndian.CopyBytes(m_audioFormat, binaryImage, startIndex);
                EndianOrder.LittleEndian.CopyBytes(m_channels, binaryImage, startIndex + 2);
                EndianOrder.LittleEndian.CopyBytes(m_sampleRate, binaryImage, startIndex + 4);
                EndianOrder.LittleEndian.CopyBytes(m_byteRate, binaryImage, startIndex + 8);
                EndianOrder.LittleEndian.CopyBytes(m_blockAlignment, binaryImage, startIndex + 12);
                EndianOrder.LittleEndian.CopyBytes(m_bitsPerSample, binaryImage, startIndex + 14);

                if (m_extraParametersSize > 0)
                {
                    EndianOrder.LittleEndian.CopyBytes(m_extraParametersSize, binaryImage, startIndex + 16);

                    if (m_extraParametersSize > 0 && m_extraParameters != null)
                        Buffer.BlockCopy(m_extraParameters, 0, binaryImage, startIndex + 18, m_extraParametersSize);
                }
                
                return binaryImage;
            }
        }

        /// <summary>
        /// Gets the length of <see cref="WaveFormatChunk"/>.
        /// </summary>
        public new int BinaryLength
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
        /// the <see cref="WaveFile"/>.  See <see cref="SampleRate"/> enumeraion for more details.
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
        /// However, this value can be changed as needed to accomodate better buffer estimations during
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
        /// However, this value can be changed as needed to accomodate even block-alignment of non-standard
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

                if (m_extraParameters == null)
                    m_extraParametersSize = 0;
                else
                    m_extraParametersSize = (short)m_extraParameters.Length;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses <see cref="WaveFormatChunk"/> object from <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initialization.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="binaryImage"/> to be used for initialization.</param>
        /// <param name="length">Valid number of bytes within binary image.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="binaryImage"/> (i.e., the number of bytes parsed).</returns>
        /// <exception cref="InvalidOperationException">WAVE format section too small, wave file corrupted.</exception>
        /// <exception cref="InvalidDataException">Invalid bit rate encountered - wave file bit rates must be a multiple of 8.</exception>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length < ChunkSize)
                throw new InvalidOperationException("WAVE format section too small, wave file corrupted.");

            m_audioFormat = EndianOrder.LittleEndian.ToUInt16(binaryImage, 0);
            m_channels = EndianOrder.LittleEndian.ToInt16(binaryImage, 2);
            m_sampleRate = EndianOrder.LittleEndian.ToInt32(binaryImage, 4);
            m_byteRate = EndianOrder.LittleEndian.ToInt32(binaryImage, 8);
            m_blockAlignment = EndianOrder.LittleEndian.ToInt16(binaryImage, 12);
            m_bitsPerSample = EndianOrder.LittleEndian.ToInt16(binaryImage, 14);

            if (m_bitsPerSample % 8 != 0)
                throw new InvalidDataException("Invalid bit rate encountered - wave file bit rates must be a multiple of 8");

            if (length > 16)
            {
                m_extraParametersSize = EndianOrder.LittleEndian.ToInt16(binaryImage, 16);
                return 18;
            }

            return 16;
        }

        /// <summary>
        /// Creates a copy of the <see cref="WaveFormatChunk"/>.
        /// </summary>
        /// <returns>A new copy of the <see cref="WaveFormatChunk"/>.</returns>
        public new WaveFormatChunk Clone()
        {
            WaveFormatChunk waveFormatChunk = new WaveFormatChunk(m_sampleRate, m_bitsPerSample, m_channels, m_audioFormat);

            if (m_extraParameters != null)
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
