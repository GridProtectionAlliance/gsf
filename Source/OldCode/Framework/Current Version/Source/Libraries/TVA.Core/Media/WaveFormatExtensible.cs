//*******************************************************************************************************
//  WaveFormatExtensible.cs - Gbtc
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
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
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
using TVA.Parsing;

namespace TVA.Media
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
    /// using TVA.Media;
    /// using TVA.Media.Sound;
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
        /// Gets binary representation of this <see cref="WaveFormatExtensible"/> instance.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] binaryImage = new byte[BinaryLength];

                EndianOrder.LittleEndian.CopyBytes(m_sampleValue, binaryImage, 0);
                EndianOrder.LittleEndian.CopyBytes((int)m_channelMask, binaryImage, 2);
                EndianOrder.LittleEndian.CopyBytes(m_subFormat, binaryImage, 6);               

                return binaryImage;
            }
        }

        /// <summary>
        /// Gets the length of the binary representation of this <see cref="WaveFormatExtensible"/> instance.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return 22;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses <see cref="WaveFormatExtensible"/> object from <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initialization.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="binaryImage"/> to be used for initialization.</param>
        /// <param name="length">Valid number of bytes within binary image.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="binaryImage"/> (i.e., the number of bytes parsed).</returns>
        /// <exception cref="InvalidOperationException">Not enough length in binary image to parse WaveFormatExtensible object.</exception>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length < BinaryLength)
                throw new InvalidOperationException("Not enough length in binary image to parse WaveFormatExtensible object");

            m_sampleValue = EndianOrder.LittleEndian.ToUInt16(binaryImage, startIndex);
            m_channelMask = (Speakers)EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2);
            m_subFormat = EndianOrder.LittleEndian.ToGuid(binaryImage, startIndex + 6);

            return BinaryLength;
        }

        #endregion
    }
}
