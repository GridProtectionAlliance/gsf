//*******************************************************************************************************
//  MetadataRecord.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/03/2006 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, TVA.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
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

using System;
using System.Text;
using TVA.Parsing;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Represents a record in the <see cref="MetadataFile"/> that contains the various attributes associates to a <see cref="HistorianID"/>.
    /// </summary>
    /// <seealso cref="MetadataFile"/>
    /// <seealso cref="MetadataRecordAlarmFlags"/>
    /// <seealso cref="MetadataRecordGeneralFlags"/>
    /// <seealso cref="MetadataRecordSecurityFlags"/>
    /// <seealso cref="MetadataRecordAnalogFields"/>
    /// <seealso cref="MetadataRecordComposedFields"/>
    /// <seealso cref="MetadataRecordConstantFields"/>
    /// <seealso cref="MetadataRecordDigitalFields"/>
    /// <seealso cref="MetadataRecordSummary"/>
    public class MetadataRecord : ISupportBinaryImage, IComparable
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 512        0-511      Char(512)  Remarks                                                       *
        // * 512        512-1023   Char(512)  HardwareInfo                                                  *
        // * 512        1024-1535  Char(512)  EmailAddresses                                                *
        // * 80         1536-1615  Char(80)   Description                                                   *
        // * 80         1616-1695  Char(80)   CurrentData                                                   *
        // * 40         1696-1735  Char(40)   Name                                                          *
        // * 40         1736-1775  Char(40)   Synonym1                                                      *
        // * 40         1776-1815  Char(40)   Synonym2                                                      *
        // * 40         1816-1855  Char(40)   Synonym3                                                      *
        // * 40         1856-1895  Char(40)   PagerNumbers                                                  *
        // * 40         1896-1935  Char(40)   PhoneNumbers                                                  *
        // * 24         1936-1959  Char(24)   PlantCode                                                     *
        // * 24         1960-1983  Char(24)   System                                                        *
        // * 40         1984-2023  Char(40)   EmailTime                                                     *
        // * 40         2024-2063  Char(40)   [Spare string field 1]                                        *
        // * 40         2064-2103  Char(40)   [Spare string field 2]                                        *
        // * 4          2104-2107  Single     ScanRate                                                      *
        // * 4          2108-2111  Int32      UnitNumber                                                    *
        // * 4          2112-2115  Int32      SecurityFlags                                                 *
        // * 4          2116-2119  Int32      GeneralFlags                                                  *
        // * 4          2120-2123  Int32      AlarmFlags                                                    *
        // * 4          2124-2127  Int32      CompressionMinTime                                            *
        // * 4          2128-2131  Int32      CompressionMaxTime                                            *
        // * 4          2132-2135  Int32      SourceID                                                      *
        // * 4          2136-2139  Int32      [Spare 32-bit field 1]                                        *
        // * 4          2140-2143  Int32      [Spare 32-bit field 2]                                        *
        // * 4          2144-2147  Int32      [Spare 32-bit field 3]                                        *
        // * 4          2148-2151  Int32      [Spare 32-bit field 4]                                        *
        // * 512        2152-2663  Byte(512)  (Analog | Digital| Composed |Constant)Fields                  *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="MetadataRecord"/>.
        /// </summary>
        public const int ByteCount = 2664;

        // Fields
        private int m_historianID;
        private string m_remarks;
        private string m_hardwareInfo;
        private string m_emailAddresses;
        private string m_description;
        private string m_currentData;
        private string m_name;
        private string m_synonym1;
        private string m_synonym2;
        private string m_synonym3;
        private string m_pagerNumbers;
        private string m_phoneNumbers;
        private string m_plantCode;
        private string m_system;
        private string m_emailTime;
        private float m_scanRate;
        private int m_unitNumber;
        private MetadataRecordSecurityFlags m_securityFlags;
        private MetadataRecordGeneralFlags m_generalFlags;
        private MetadataRecordAlarmFlags m_alarmFlags;
        private int m_compressionMinTime;
        private int m_compressionMaxTime;
        private int m_sourceID;
        private MetadataRecordAnalogFields m_analogFields;
        private MetadataRecordDigitalFields m_digitalFields;
        private MetadataRecordComposedFields m_composedFields;
        private MetadataRecordConstantFields m_constantFields;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataRecord"/> class.
        /// </summary>
        /// <param name="historianID">Historian identifier of <see cref="MetadataRecord"/>.</param>
        public MetadataRecord(int historianID)
        {
            m_historianID = historianID;
            m_remarks = string.Empty;
            m_hardwareInfo = string.Empty;
            m_emailAddresses = string.Empty;
            m_description = string.Empty;
            m_currentData = string.Empty;
            m_name = string.Empty;
            m_synonym1 = string.Empty;
            m_synonym2 = string.Empty;
            m_synonym3 = string.Empty;
            m_pagerNumbers = string.Empty;
            m_phoneNumbers = string.Empty;
            m_plantCode = string.Empty;
            m_system = string.Empty;
            m_emailTime = string.Empty;
            m_securityFlags = new MetadataRecordSecurityFlags();
            m_generalFlags = new MetadataRecordGeneralFlags();
            m_alarmFlags = new MetadataRecordAlarmFlags();
            m_analogFields = new MetadataRecordAnalogFields();
            m_digitalFields = new MetadataRecordDigitalFields();
            m_composedFields = new MetadataRecordComposedFields();
            m_constantFields = new MetadataRecordConstantFields();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataRecord"/> class.
        /// </summary>
        /// <param name="historianID">Historian identifier of <see cref="MetadataRecord"/>.</param>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="MetadataRecord"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public MetadataRecord(int historianID, byte[] binaryImage, int startIndex, int length)
            : this(historianID)
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets any remarks associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="Remarks"/> is 512 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string Remarks
        {
            get
            {
                return m_remarks;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_remarks = value.TruncateRight(512);
            }
        }

        /// <summary>
        /// Gets or sets hardware information associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="HardwareInfo"/> is 512 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string HardwareInfo
        {
            get
            {
                return m_hardwareInfo;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_hardwareInfo = value.TruncateRight(512);
            }
        }

        /// <summary>
        /// Gets or sets a comma-seperated list of email addresses that will receive alarm notification email messages based 
        /// on the <see cref="AlarmFlags"/> settings for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="AlarmEmails"/> is 512 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string AlarmEmails
        {
            get
            {
                return m_emailAddresses;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_emailAddresses = value.TruncateRight(512);
            }
        }

        /// <summary>
        /// Gets or sets the description associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="Description"/> is 80 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_description = value.TruncateRight(80);
            }
        }

        /// <summary>
        /// Gets or sets the time, value and quality of the most current data received for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="CurrentData"/> is 80 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string CurrentData
        {
            get
            {
                return m_currentData;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_currentData = value.TruncateRight(80);
            }
        }

        /// <summary>
        /// Gets or sets a alpha-numeric name for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="Name"/> is 40 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_name = value.TruncateRight(40);
            }
        }

        /// <summary>
        /// Gets or sets an alternate <see cref="Name"/> for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="Synonym1"/> is 40 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string Synonym1
        {
            get
            {
                return m_synonym1;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_synonym1 = value.TruncateRight(40);
            }
        }

        /// <summary>
        /// Gets or sets an alternate <see cref="Name"/> for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="Synonym2"/> is 40 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string Synonym2
        {
            get
            {
                return m_synonym2;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_synonym2 = value.TruncateRight(40);
            }
        }

        /// <summary>
        /// Gets or sets an alternate <see cref="Name"/> for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="Synonym3"/> is 40 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string Synonym3
        {
            get
            {
                return m_synonym3;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_synonym3 = value.TruncateRight(40);
            }
        }

        /// <summary>
        /// Gets or sets a comma-seperated list of pager numbers that will receive alarm notification text messages based 
        /// on the <see cref="AlarmFlags"/> settings for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="AlarmPagers"/> is 40 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string AlarmPagers
        {
            get
            {
                return m_pagerNumbers;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_pagerNumbers = value.TruncateRight(40);
            }
        }

        /// <summary>
        /// Gets or sets a comma-seperated list of phone numbers that will receive alarm notification voice messages based 
        /// on the <see cref="AlarmFlags"/> settings for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="AlarmPhones"/> is 40 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string AlarmPhones
        {
            get
            {
                return m_phoneNumbers;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_phoneNumbers = value.TruncateRight(40);
            }
        }
        
        /// <summary>
        /// Gets or sets the name of the plant to which the <see cref="HistorianID"/> is associated.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="PlantCode"/> is 24 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string PlantCode
        {
            get
            {
                return m_plantCode;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_plantCode = value.TruncateRight(24);
            }
        }

        /// <summary>
        /// Gets or sets the alpha-numeric system identifier for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="SystemName"/> is 24 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string SystemName
        {
            get
            {
                return m_system;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_system = value.TruncateRight(24);
            }
        }

        /// <summary>
        /// Gets or sets the data and time when an alarm notification is sent based on the <see cref="AlarmFlags"/> settings for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// Maximum length for <see cref="EmailTime"/> is 40 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string EmailTime
        {
            get
            {
                return m_emailTime;
            }
            set
            {
                m_emailTime = value.TruncateRight(40);
            }
        }

        /// <summary>
        /// Gets or sets the rate at which the source device scans and sends data for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ScanRate"/> is used by data aquisition components for polling data from the actual device.
        /// </remarks>
        public float ScanRate
        {
            get
            {
                return m_scanRate;
            }
            set
            {
                m_scanRate = value;
            }
        }

        /// <summary>
        /// Gets or sets the unit (i.e. generator) to which the <see cref="HistorianID"/> is associated.
        /// </summary>
        public int UnitNumber
        {
            get
            {
                return m_unitNumber;
            }
            set
            {
                m_unitNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordSecurityFlags"/> associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        public MetadataRecordSecurityFlags SecurityFlags
        {
            get
            {
                return m_securityFlags;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_securityFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags"/> associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        public MetadataRecordGeneralFlags GeneralFlags
        {
            get
            {
                return m_generalFlags;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_generalFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAlarmFlags"/> associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        public MetadataRecordAlarmFlags AlarmFlags
        {
            get
            {
                return m_alarmFlags;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_alarmFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowable time (in seconds) between archived data for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="CompressionMinTime"/> is useful for limiting archived data for noisy <see cref="HistorianID"/>s.
        /// </remarks>
        public int CompressionMinTime
        {
            get
            {
                return m_compressionMinTime;
            }
            set
            {
                m_compressionMinTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum time (in seconds) after which data is to be archived for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="CompressionMaxTime"/> ensures that archived data exist every "n" seconds for the <see cref="HistorianID"/>, 
        /// which would otherwise be omitted due to compression.
        /// </remarks>
        public int CompressionMaxTime
        {
            get
            {
                return m_compressionMaxTime;
            }
            set
            {
                m_compressionMaxTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the numeric identifier of the data source for the <see cref="HistorianID"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="SourceID"/> is used for the determination of "global time" when that client option is in effect.  
        /// When "global time" is in effect, the historian returns the current data time for a <see cref="HistorianID"/> 
        /// based on the latest time received for all <see cref="HistorianID"/>s with the same <see cref="SourceID"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">The value being assigned is not positive or zero.</exception>
        public int SourceID
        {
            get
            {
                return m_sourceID;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive or zero.");

                m_sourceID = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields"/> associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        public MetadataRecordAnalogFields AnalogFields
        {
            get
            {
                return m_analogFields;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_analogFields = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordDigitalFields"/> associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        public MetadataRecordDigitalFields DigitalFields
        {
            get
            {
                return m_digitalFields;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_digitalFields = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordComposedFields"/> associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        public MetadataRecordComposedFields ComposedFields
        {
            get
            {
                return m_composedFields;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_composedFields = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordConstantFields"/> associated with the <see cref="HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        public MetadataRecordConstantFields ConstantFields
        {
            get
            {
                return m_constantFields;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_constantFields = value;
            }
        }

        /// <summary>
        /// Gets the historian identifier of <see cref="MetadataRecord"/>.
        /// </summary>
        public int HistorianID
        {
            get
            {
                return m_historianID;
            }
        }

        /// <summary>
        /// Gets the <see cref="MetadataRecordSummary"/> object for <see cref="MetadataRecord"/>.
        /// </summary>
        public MetadataRecordSummary Summary
        {
            get
            {
                return new MetadataRecordSummary(this);
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return ByteCount;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="MetadataRecord"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                // Construct the binary IP buffer for this event
                Array.Copy(Encoding.ASCII.GetBytes(m_remarks.PadRight(512)), 0, image, 0, 512);
                Array.Copy(Encoding.ASCII.GetBytes(m_hardwareInfo.PadRight(512)), 0, image, 512, 512);
                Array.Copy(Encoding.ASCII.GetBytes(m_emailAddresses.PadRight(512)), 0, image, 1024, 512);
                Array.Copy(Encoding.ASCII.GetBytes(m_description.PadRight(80)), 0, image, 1536, 80);
                Array.Copy(Encoding.ASCII.GetBytes(m_currentData.PadRight(80)), 0, image, 1616, 80);
                Array.Copy(Encoding.ASCII.GetBytes(m_name.PadRight(40)), 0, image, 1696, 40);
                Array.Copy(Encoding.ASCII.GetBytes(m_synonym1.PadRight(40)), 0, image, 1736, 40);
                Array.Copy(Encoding.ASCII.GetBytes(m_synonym2.PadRight(40)), 0, image, 1776, 40);
                Array.Copy(Encoding.ASCII.GetBytes(m_synonym3.PadRight(40)), 0, image, 1816, 40);
                Array.Copy(Encoding.ASCII.GetBytes(m_pagerNumbers.PadRight(40)), 0, image, 1856, 40);
                Array.Copy(Encoding.ASCII.GetBytes(m_phoneNumbers.PadRight(40)), 0, image, 1896, 40);
                Array.Copy(Encoding.ASCII.GetBytes(m_plantCode.PadRight(24)), 0, image, 1936, 24);
                Array.Copy(Encoding.ASCII.GetBytes(m_system.PadRight(24)), 0, image, 1960, 24);
                Array.Copy(Encoding.ASCII.GetBytes(m_emailTime.PadRight(40)), 0, image, 1984, 40);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_scanRate), 0, image, 2104, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_unitNumber), 0, image, 2108, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_securityFlags.Value), 0, image, 2112, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_generalFlags.Value), 0, image, 2116, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_alarmFlags.Value), 0, image, 2120, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_compressionMinTime), 0, image, 2124, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_compressionMaxTime), 0, image, 2128, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_sourceID), 0, image, 2132, 4);
                switch (m_generalFlags.DataType)
                {
                    case DataType.Analog:
                        Array.Copy(m_analogFields.BinaryImage, 0, image, 2152, MetadataRecordAnalogFields.ByteCount);
                        break;
                    case DataType.Digital:
                        Array.Copy(m_digitalFields.BinaryImage, 0, image, 2152, MetadataRecordDigitalFields.ByteCount);
                        break;
                    case DataType.Composed:
                        Array.Copy(m_composedFields.BinaryImage, 0, image, 2152, MetadataRecordComposedFields.ByteCount);
                        break;
                    case DataType.Constant:
                        Array.Copy(m_constantFields.BinaryImage, 0, image, 2152, MetadataRecordConstantFields.ByteCount);
                        break;
                }

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="MetadataRecord"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="MetadataRecord"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="MetadataRecord"/>.</returns>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= ByteCount)
            {
                // Binary image has sufficient data.
                Remarks = Encoding.ASCII.GetString(binaryImage, startIndex, 512).Trim();
                HardwareInfo = Encoding.ASCII.GetString(binaryImage, startIndex + 512, 512).Trim();
                AlarmEmails = Encoding.ASCII.GetString(binaryImage, startIndex + 1024, 512).Trim();
                Description = Encoding.ASCII.GetString(binaryImage, startIndex + 1536, 80).Trim();
                CurrentData = Encoding.ASCII.GetString(binaryImage, startIndex + 1616, 80).Trim();
                Name = Encoding.ASCII.GetString(binaryImage, startIndex + 1696, 40).Trim();
                Synonym1 = Encoding.ASCII.GetString(binaryImage, startIndex + 1736, 40).Trim();
                Synonym2 = Encoding.ASCII.GetString(binaryImage, startIndex + 1776, 40).Trim();
                Synonym3 = Encoding.ASCII.GetString(binaryImage, startIndex + 1816, 40).Trim();
                AlarmPagers = Encoding.ASCII.GetString(binaryImage, startIndex + 1856, 40).Trim();
                AlarmPhones = Encoding.ASCII.GetString(binaryImage, startIndex + 1896, 40).Trim();
                PlantCode = Encoding.ASCII.GetString(binaryImage, startIndex + 1936, 24).Trim();
                SystemName = Encoding.ASCII.GetString(binaryImage, startIndex + 1960, 24).Trim();
                EmailTime = Encoding.ASCII.GetString(binaryImage, startIndex + 1984, 40).Trim();
                ScanRate = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 2104);
                UnitNumber = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2108);
                SecurityFlags.Value = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2112);
                GeneralFlags.Value = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2116);
                AlarmFlags.Value = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2120);
                CompressionMinTime = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2124);
                CompressionMaxTime = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2128);
                SourceID = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2132);
                switch (GeneralFlags.DataType)
                {
                    case DataType.Analog:
                        m_analogFields.Initialize(binaryImage, startIndex + 2152, length);
                        break;
                    case DataType.Digital:
                        m_digitalFields.Initialize(binaryImage, startIndex + 2152, length);
                        break;
                    case DataType.Composed:
                        m_composedFields.Initialize(binaryImage, startIndex + 2152, length);
                        break;
                    case DataType.Constant:
                        m_constantFields.Initialize(binaryImage, startIndex + 2152, length);
                        break;
                }

                return ByteCount;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        /// <summary>
        /// Compares the current <see cref="MetadataRecord"/> object to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="MetadataRecord"/> object is to be compared.</param>
        /// <returns>
        /// Negative value if the current <see cref="MetadataRecord"/> object is less than <paramref name="obj"/>, 
        /// Zero if the current <see cref="MetadataRecord"/> object is equal to <paramref name="obj"/>, 
        /// Positive value if the current <see cref="MetadataRecord"/> object is greater than <paramref name="obj"/>.
        /// </returns>
        public virtual int CompareTo(object obj)
        {
            MetadataRecord other = obj as MetadataRecord;
            if (other == null)
                return 1;
            else
                return m_historianID.CompareTo(other.HistorianID);
        }

        /// <summary>
        /// Determines whether the current <see cref="MetadataRecord"/> object is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="MetadataRecord"/> object is to be compared for equality.</param>
        /// <returns>true if the current <see cref="MetadataRecord"/> object is equal to <paramref name="obj"/>; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return (CompareTo(obj) == 0);
        }

        /// <summary>
        /// Returns the text representation of <see cref="MetadataRecord"/> object.
        /// </summary>
        /// <returns>A <see cref="string"/> value.</returns>
        public override string ToString()
        {
            return string.Format("ID={0}; Name={1}", m_historianID, m_name);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="MetadataRecord"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer value.</returns>
        public override int GetHashCode()
        {
            return m_historianID.GetHashCode();
        }

        #endregion
    }
}
