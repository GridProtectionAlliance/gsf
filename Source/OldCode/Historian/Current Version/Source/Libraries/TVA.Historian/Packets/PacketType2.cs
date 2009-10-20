//*******************************************************************************************************
//  PacketType2.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/27/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, TVA.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/23/2009 - Pinal C. Patel
//       Edited code comments.
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

using System;
using System.Collections.Generic;
using System.Text;
using TVA.Historian.Files;

namespace TVA.Historian.Packets
{
    /// <summary>
    /// Represents a packet to be used for sending single time (expanded format) series data point to a historian for archival.
    /// </summary>
    public class PacketType2 : PacketBase
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 2          0-1        Int16      TypeID (packet identifier)                                    *
        // * 4          2-5        Int32      HistorianID                                                   *
        // * 2          6-7        Int16      Year                                                          *
        // * 1          8          Byte       Month                                                         *
        // * 1          9          Byte       Day                                                           *
        // * 1          10         Byte       Hour                                                          *
        // * 1          11         Byte       Minute                                                        *
        // * 1          12         Byte       Second                                                        *
        // * 1          13         Byte       Quality                                                       *
        // * 2          14-15      Int16      Milliseconds                                                  *
        // * 2          16-17      Int16      GmtOffset                                                     *
        // * 4          18-21      Single     Value                                                         *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="PacketType2"/>.
        /// </summary>
        public new const int ByteCount = 22;

        // Fields
        private int m_historianID;
        private short m_year;
        private short m_month;
        private short m_day;
        private short m_hour;
        private short m_minute;
        private short m_second;
        private Quality m_quality;
        private short m_millisecond;
        private short m_gmtOffset;
        private float m_value;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketType2"/> class.
        /// </summary>
        public PacketType2()
            : base(2)
        {
            ProcessHandler = Process;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketType2"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="PacketType2"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public PacketType2(byte[] binaryImage, int startIndex, int length)
            : this()
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the historian identifier of the time-series data.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
        public int HistorianID
        {
            get
            {
                return m_historianID;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_historianID = value;
            }
        }

        /// <summary>
        /// Gets or sets the year-part of the time.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 1995 and 2063.</exception>
        public short Year
        {
            get
            {
                return m_year;
            }
            set
            {
                if (value < TimeTag.MinValue.ToDateTime().Year || value > TimeTag.MaxValue.ToDateTime().Year)
                    throw new ArgumentException("Value must 1995 and 2063.");

                m_year = value;
            }
        }

        /// <summary>
        /// Gets or sets the month-part of the time.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 1 and 12.</exception>
        public short Month
        {
            get
            {
                return m_month;
            }
            set
            {
                if (value < 1 || value > 12)
                    throw new ArgumentException("Value must be between 1 and 12.");

                m_month = value;
            }
        }

        /// <summary>
        /// Gets or sets the day-part of the time.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 1 and 31.</exception>
        public short Day
        {
            get
            {
                return m_day;
            }
            set
            {
                if (value < 1 || value > 31)
                    throw new ArgumentException("Value must be between 1 and 31.");

                m_day = value;
            }
        }

        /// <summary>
        /// Gets or sets the hour-part of the time.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 0 and 23.</exception>
        public short Hour
        {
            get
            {
                return m_hour;
            }
            set
            {
                if (value < 0 || value > 23)
                    throw new ArgumentException("Value must be between 0 and 23.");

                m_hour = value;
            }
        }

        /// <summary>
        /// Gets or sets the minute-part of the time.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 0 and 59.</exception>
        public short Minute
        {
            get
            {
                return m_minute;
            }
            set
            {
                if (value < 0 || value > 59)
                    throw new ArgumentException("Value must be between 0 and 59.");

                m_minute = value;
            }
        }

        /// <summary>
        /// Gets or sets the second-part of the time.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 0 and 59.</exception>
        public short Second
        {
            get
            {
                return m_second;
            }
            set
            {
                if (value < 0 || value > 59)
                    throw new ArgumentException("Value must be between 0 and 59.");

                m_second = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Quality"/> of the time-series data.
        /// </summary>
        public Quality Quality
        {
            get
            {
                return m_quality;
            }
            set
            {
                m_quality = value;
            }
        }

        /// <summary>
        /// Gets or sets the millisecond-part of the time.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 0 and 999.</exception>
        public short Millisecond
        {
            get
            {
                return m_millisecond;
            }
            set
            {
                if (value < 0 || value > 999)
                    throw new ArgumentException("Value must be between 0 and 999.");

                m_millisecond = value;
            }
        }

        /// <summary>
        /// Gets or sets the difference, in hours, between the local time and Greenwich Mean Time (Universal Coordinated Time).
        /// </summary>
        public short GmtOffset
        {
            get
            {
                return m_gmtOffset;
            }
            set
            {
                m_gmtOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the time-series data.
        /// </summary>
        public float Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public override int BinaryLength
        {
            get
            {
                return ByteCount;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="PacketType2"/>.
        /// </summary>
        public override byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(TypeID), 0, image, 0, 2);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_historianID), 0, image, 2, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_year), 0, image, 6, 2);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_month), 0, image, 8, 1);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_day), 0, image, 9, 1);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_hour), 0, image, 10, 1);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_minute), 0, image, 11, 1);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_second), 0, image, 12, 1);
                Array.Copy(EndianOrder.LittleEndian.GetBytes((int)m_quality), 0, image, 13, 1);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_millisecond), 0, image, 14, 2);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_gmtOffset), 0, image, 16, 2);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_value), 0, image, 18, 4);

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="PacketType2"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="PacketType2"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="PacketType2"/>.</returns>
        public override int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length >= ByteCount)
            {
                // Binary image has sufficient data.
                short packetID = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex);
                if (packetID != TypeID)
                    throw new ArgumentException(string.Format("Unexpected packet id '{0}' (expected '{1}')", packetID, TypeID));

                // We have a binary image with the correct packet id.
                HistorianID = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2);
                Year = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex + 6);
                Month = Convert.ToInt16(binaryImage[startIndex + 8]);
                Day = Convert.ToInt16(binaryImage[startIndex + 9]);
                Hour = Convert.ToInt16(binaryImage[startIndex + 10]);
                Minute = Convert.ToInt16(binaryImage[startIndex + 11]);
                Second = Convert.ToInt16(binaryImage[startIndex + 12]);
                Quality = (Quality)(binaryImage[startIndex + 13]);
                Millisecond = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex + 14);
                GmtOffset = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex + 16);
                Value = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 18);

                // We'll send an "ACK" to the sender if this is the last packet in the transmission.
                if (length == ByteCount)
                    PreProcessHandler = PreProcess;

                return ByteCount;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        /// <summary>
        /// Extracts time-series data from <see cref="PacketType2"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> object of <see cref="ArchiveDataPoint"/>s.</returns>
        public override IEnumerable<IDataPoint> ExtractTimeSeriesData()
        {
            DateTime timestamp = new DateTime(m_year, m_month, m_day, m_hour + m_gmtOffset, m_minute, m_second, m_millisecond, DateTimeKind.Utc);

            return new ArchiveDataPoint[] { new ArchiveDataPoint(m_historianID, new TimeTag(timestamp), m_value, m_quality) };
        }

        /// <summary>
        /// Processes <see cref="PacketType2"/>.
        /// </summary>
        /// <returns>A null reference.</returns>
        protected virtual IEnumerable<byte[]> Process()
        {
            if (Archive != null)
            {
                foreach (IDataPoint dataPoint in ExtractTimeSeriesData())
                {
                    Archive.WriteData(dataPoint);
                }
            }

            return null;
        }

        /// <summary>
        /// Pre-processes <see cref="PacketType2"/>.
        /// </summary>
        /// <returns>A <see cref="byte"/> array for "ACK".</returns>
        protected virtual IEnumerable<byte[]> PreProcess()
        {
            return new byte[][] { Encoding.ASCII.GetBytes("ACK") };
        }

        #endregion
    }
}