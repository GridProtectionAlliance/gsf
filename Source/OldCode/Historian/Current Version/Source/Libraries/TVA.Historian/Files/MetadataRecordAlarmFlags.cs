//*******************************************************************************************************
//  MetadataRecordAlarmFlags.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/22/2007 - Pinal C. Patel
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

namespace TVA.Historian.Files
{
    /// <summary>
    /// Defines which data <see cref="Quality"/> should trigger an alarm notification.
    /// </summary>
    /// <seealso cref="MetadataRecord"/>
    public class MetadataRecordAlarmFlags
    {
        #region [ Members ]

        // Fields
        private int m_value;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.Unknown"/> should trigger an alarm notification.
        /// </summary>
        public bool Unknown
        {
            get
            {
                return m_value.CheckBits(Bits.Bit00);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit00) : m_value.ClearBits(Bits.Bit00);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DeletedFromProcessing"/> should trigger an alarm notification.
        /// </summary>
        public bool DeletedFromProcessing
        {
            get
            {
                return m_value.CheckBits(Bits.Bit01);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit01) : m_value.ClearBits(Bits.Bit01);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.CouldNotCalculate"/> should trigger an alarm notification.
        /// </summary>
        public bool CouldNotCalculate
        {
            get
            {
                return m_value.CheckBits(Bits.Bit02);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit02) : m_value.ClearBits(Bits.Bit02);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.FrontEndHardwareError"/> should trigger an alarm notification.
        /// </summary>
        public bool FrontEndHardwareError
        {
            get
            {
                return m_value.CheckBits(Bits.Bit03);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit03) : m_value.ClearBits(Bits.Bit03);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SensorReadError"/> should trigger an alarm notification.
        /// </summary>
        public bool SensorReadError
        {
            get
            {
                return m_value.CheckBits(Bits.Bit04);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit04) : m_value.ClearBits(Bits.Bit04);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.OpenThermocouple"/> should trigger an alarm notification.
        /// </summary>
        public bool OpenThermocouple
        {
            get
            {
                return m_value.CheckBits(Bits.Bit05);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit05) : m_value.ClearBits(Bits.Bit05);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InputCountsOutOfSensorRange"/> should trigger an alarm notification.
        /// </summary>
        public bool InputCountsOutOfSensorRange
        {
            get
            {
                return m_value.CheckBits(Bits.Bit06);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit06) : m_value.ClearBits(Bits.Bit06);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.UnreasonableHigh"/> should trigger an alarm notification.
        /// </summary>
        public bool UnreasonableHigh
        {
            get
            {
                return m_value.CheckBits(Bits.Bit07);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit07) : m_value.ClearBits(Bits.Bit07);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.UnreasonableLow"/> should trigger an alarm notification.
        /// </summary>
        public bool UnreasonableLow
        {
            get
            {
                return m_value.CheckBits(Bits.Bit08);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit08) : m_value.ClearBits(Bits.Bit08);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.Old"/> should trigger an alarm notification.
        /// </summary>
        public bool Old
        {
            get
            {
                return m_value.CheckBits(Bits.Bit09);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit09) : m_value.ClearBits(Bits.Bit09);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueAboveHiHiLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool SuspectValueAboveHiHiLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit10);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit10) : m_value.ClearBits(Bits.Bit10);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueBelowLoLoLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool SuspectValueBelowLoLoLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit11);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit11) : m_value.ClearBits(Bits.Bit11);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueAboveHiLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool SuspectValueAboveHiLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit12);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit12) : m_value.ClearBits(Bits.Bit12);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueBelowLoLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool SuspectValueBelowLoLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit13);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit13) : m_value.ClearBits(Bits.Bit13);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectData"/> should trigger an alarm notification.
        /// </summary>
        public bool SuspectData
        {
            get
            {
                return m_value.CheckBits(Bits.Bit14);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit14) : m_value.ClearBits(Bits.Bit14);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DigitalSuspectAlarm"/> should trigger an alarm notification.
        /// </summary>
        public bool DigitalSuspectAlarm
        {
            get
            {
                return m_value.CheckBits(Bits.Bit15);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit15) : m_value.ClearBits(Bits.Bit15);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueAboveHiHiLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool InsertedValueAboveHiHiLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit16);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit16) : m_value.ClearBits(Bits.Bit16);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueBelowLoLoLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool InsertedValueBelowLoLoLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit17);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit17) : m_value.ClearBits(Bits.Bit17);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueAboveHiLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool InsertedValueAboveHiLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit18);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit18) : m_value.ClearBits(Bits.Bit18);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueBelowLoLimit"/> should trigger an alarm notification.
        /// </summary>
        public bool InsertedValueBelowLoLimit
        {
            get
            {
                return m_value.CheckBits(Bits.Bit19);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit19) : m_value.ClearBits(Bits.Bit19);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValue"/> should trigger an alarm notification.
        /// </summary>
        public bool InsertedValue
        {
            get
            {
                return m_value.CheckBits(Bits.Bit20);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit20) : m_value.ClearBits(Bits.Bit20);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DigitalInsertedStatusInAlarm"/> should trigger an alarm notification.
        /// </summary>
        public bool DigitalInsertedStatusInAlarm
        {
            get
            {
                return m_value.CheckBits(Bits.Bit21);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit21) : m_value.ClearBits(Bits.Bit21);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.LogicalAlarm"/> should trigger an alarm notification.
        /// </summary>
        public bool LogicalAlarm
        {
            get
            {
                return m_value.CheckBits(Bits.Bit22);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit22) : m_value.ClearBits(Bits.Bit22);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueAboveHiHiAlarm"/> should trigger an alarm notification.
        /// </summary>
        public bool ValueAboveHiHiAlarm
        {
            get
            {
                return m_value.CheckBits(Bits.Bit23);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit23) : m_value.ClearBits(Bits.Bit23);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueBelowLoLoAlarm"/> should trigger an alarm notification.
        /// </summary>
        public bool ValueBelowLoLoAlarm
        {
            get
            {
                return m_value.CheckBits(Bits.Bit24);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit24) : m_value.ClearBits(Bits.Bit24);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueAboveHiAlarm"/> should trigger an alarm notification.
        /// </summary>
        public bool ValueAboveHiAlarm
        {
            get
            {
                return m_value.CheckBits(Bits.Bit25);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit25) : m_value.ClearBits(Bits.Bit25);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueBelowLoAlarm"/> should trigger an alarm notification.
        /// </summary>
        public bool ValueBelowLoAlarm
        {
            get
            {
                return m_value.CheckBits(Bits.Bit26);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit26) : m_value.ClearBits(Bits.Bit26);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DeletedFromAlarmChecks"/> should trigger an alarm notification.
        /// </summary>
        public bool DeletedFromAlarmChecks
        {
            get
            {
                return m_value.CheckBits(Bits.Bit27);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit27) : m_value.ClearBits(Bits.Bit27);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InhibitedByCutoutPoint"/> should trigger an alarm notification.
        /// </summary>
        public bool InhibitedByCutoutPoint
        {
            get
            {
                return m_value.CheckBits(Bits.Bit28);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit28) : m_value.ClearBits(Bits.Bit28);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.Good"/> should trigger an alarm notification.
        /// </summary>
        public bool Good
        {
            get
            {
                return m_value.CheckBits(Bits.Bit29);
            }
            set
            {
                m_value = value ? m_value.SetBits(Bits.Bit29) : m_value.ClearBits(Bits.Bit29);
            }
        }

        /// <summary>
        /// Gets or sets the 32-bit integer value used for defining which data <see cref="Quality"/> should trigger an alarm notification.
        /// </summary>
        public int Value
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

        #endregion
    }
}
