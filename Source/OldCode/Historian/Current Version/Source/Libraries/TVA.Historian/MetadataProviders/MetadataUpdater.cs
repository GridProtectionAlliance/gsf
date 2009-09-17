//*******************************************************************************************************
//  MetadataUpdater.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/07/2009 - Pinal C. Patel
//       Generated original version of source code.
//  08/21/2009 - Pinal C. Patel
//       Removed ExtractMetadata() method as this can be achived using Serialization.Serialize() method.
//       Modified UpdateMetadata() overload for processing web service data to use Serialization class.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/16/2009 - Pinal C. Patel
//       Modified UpdateMetadata() overloads to save the metadata file upon update.
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
using System.Data;
using System.IO;
using TVA.Historian.Files;
using TVA.Historian.Services;

namespace TVA.Historian.MetadataProviders
{
    /// <summary>
    /// A class that can update data in a <see cref="MetadataFile"/>.
    /// </summary>
    /// <seealso cref="MetadataFile"/>
    public class MetadataUpdater
    {
        #region [ Members ]

        // Fields
        private MetadataFile m_metadata;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataUpdater"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="MetadataFile"/> that is to be updated.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadata"/> is null</exception>
        public MetadataUpdater(MetadataFile metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            m_metadata = metadata;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="MetadataFile"/> to be updated.
        /// </summary>
        public MetadataFile Metadata 
        {
            get
            {
                return m_metadata;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Updates the <see cref="Metadata"/> from <paramref name="tableData"/>
        /// </summary>
        /// <param name="tableData"><see cref="DataTable"/> containing the new metadata.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tableData"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableData"/> does not contain 43 columns.</exception>
        public void UpdateMetadata(DataTable tableData)
        {
            if (tableData == null)
                throw new ArgumentNullException("tableData");

            if (tableData.Rows[0].ItemArray.Length != 43)
                throw new ArgumentException("tableData must contain 43 columns.");

            // Column 00: HistorianID
            // Column 01: DataType
            // Column 02: Name
            // Column 03: Synonym1
            // Column 04: Synonym2
            // Column 05: Synonym3
            // Column 06: Description
            // Column 07: HardwareInfo
            // Column 08: Remarks
            // Column 09: PlantCode
            // Column 10: UnitNumber
            // Column 11: SystemName
            // Column 12: SourceID
            // Column 13: Enabled
            // Column 14: ScanRate
            // Column 15: CompressionMinTime
            // Column 16: CompressionMaxTime
            // Column 17: EngineeringUnits
            // Column 18: LowWarning
            // Column 19: HighWarning
            // Column 20: LowAlarm
            // Column 21: HighAlarm
            // Column 22: LowRange
            // Column 23: HighRange
            // Column 24: CompressionLimit
            // Column 25: ExceptionLimit
            // Column 26: DisplayDigits
            // Column 27: SetDescription
            // Column 28: ClearDescription
            // Column 29: AlarmState
            // Column 30: ChangeSecurity
            // Column 31: AccessSecurity
            // Column 32: StepCheck
            // Column 33: AlarmEnabled
            // Column 34: AlarmFlags
            // Column 35: AlarmDelay
            // Column 36: AlarmToFile
            // Column 37: AlarmByEmail
            // Column 38: AlarmByPager
            // Column 39: AlarmByPhone
            // Column 40: AlarmEmails
            // Column 41: AlarmPagers
            // Column 42: AlarmPhones
            MetadataRecord metadataRecord;
            foreach (DataRow row in tableData.Rows)
            {
                metadataRecord = new MetadataRecord(Convert.ToInt32(row[0]));
                metadataRecord.GeneralFlags.DataType = (DataType)Convert.ToInt32(row[1]);
                metadataRecord.Name = Convert.ToString(row[2]);
                metadataRecord.Synonym1 = Convert.ToString(row[3]);
                metadataRecord.Synonym2 = Convert.ToString(row[4]);
                metadataRecord.Synonym3 = Convert.ToString(row[5]);
                metadataRecord.Description = Convert.ToString(row[6]);
                metadataRecord.HardwareInfo = Convert.ToString(row[7]);
                metadataRecord.Remarks = Convert.ToString(row[8]);
                metadataRecord.PlantCode = Convert.ToString(row[9]);
                metadataRecord.UnitNumber = Convert.ToInt32(row[10]);
                metadataRecord.SystemName = Convert.ToString(row[11]);
                metadataRecord.SourceID = Convert.ToInt32(row[12]);
                metadataRecord.GeneralFlags.Enabled = Convert.ToBoolean(row[13]);
                metadataRecord.ScanRate = Convert.ToSingle(row[14]);
                metadataRecord.CompressionMinTime = Convert.ToInt32(row[15]);
                metadataRecord.CompressionMaxTime = Convert.ToInt32(row[16]);
                metadataRecord.SecurityFlags.ChangeSecurity = Convert.ToInt32(row[30]);
                metadataRecord.SecurityFlags.AccessSecurity = Convert.ToInt32(row[31]);
                metadataRecord.GeneralFlags.StepCheck = Convert.ToBoolean(row[32]);
                metadataRecord.GeneralFlags.AlarmEnabled = Convert.ToBoolean(row[33]);
                metadataRecord.AlarmFlags.Value = Convert.ToInt32(row[34]);
                metadataRecord.GeneralFlags.AlarmToFile = Convert.ToBoolean(row[36]);
                metadataRecord.GeneralFlags.AlarmByEmail = Convert.ToBoolean(row[37]);
                metadataRecord.GeneralFlags.AlarmByPager = Convert.ToBoolean(row[38]);
                metadataRecord.GeneralFlags.AlarmByPhone = Convert.ToBoolean(row[39]);
                metadataRecord.AlarmEmails = Convert.ToString(row[40]);
                metadataRecord.AlarmPagers = Convert.ToString(row[41]);
                metadataRecord.AlarmPhones = Convert.ToString(row[42]);
                if (metadataRecord.GeneralFlags.DataType == DataType.Analog)
                {
                    metadataRecord.AnalogFields.EngineeringUnits = Convert.ToString(row[17]);
                    metadataRecord.AnalogFields.LowWarning = Convert.ToSingle(row[18]);
                    metadataRecord.AnalogFields.HighWarning = Convert.ToSingle(row[19]);
                    metadataRecord.AnalogFields.LowAlarm = Convert.ToSingle(row[20]);
                    metadataRecord.AnalogFields.HighAlarm = Convert.ToSingle(row[21]);
                    metadataRecord.AnalogFields.LowRange = Convert.ToSingle(row[22]);
                    metadataRecord.AnalogFields.HighRange = Convert.ToSingle(row[23]);
                    metadataRecord.AnalogFields.CompressionLimit = Convert.ToSingle(row[24]);
                    metadataRecord.AnalogFields.ExceptionLimit = Convert.ToSingle(row[25]);
                    metadataRecord.AnalogFields.DisplayDigits = Convert.ToInt32(row[26]);
                    metadataRecord.AnalogFields.AlarmDelay = Convert.ToSingle(row[35]);
                }
                else if (metadataRecord.GeneralFlags.DataType == DataType.Digital)
                {
                    metadataRecord.DigitalFields.SetDescription = Convert.ToString(row[27]);
                    metadataRecord.DigitalFields.ClearDescription = Convert.ToString(row[28]);
                    metadataRecord.DigitalFields.AlarmState = Convert.ToInt32(row[29]);
                    metadataRecord.DigitalFields.AlarmDelay = Convert.ToSingle(row[35]);
                }

                m_metadata.Write(metadataRecord.HistorianID, metadataRecord);
            }
            m_metadata.Save();
        }

        /// <summary>
        /// Updates the <see cref="Metadata"/> from <paramref name="readerData"/>
        /// </summary>
        /// <param name="readerData"><see cref="IDataReader"/> providing the new metadata.</param>
        /// <exception cref="ArgumentNullException"><paramref name="readerData"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="readerData"/> does not contain 43 columns.</exception>
        public void UpdateMetadata(IDataReader readerData)
        {
            if (readerData == null)
                throw new ArgumentNullException("readerData");

            if (readerData.FieldCount != 43)
                throw new ArgumentException("readerData must contain 43 columns.");

            MetadataRecord metadataRecord;
            while (readerData.Read())
            {
                metadataRecord = new MetadataRecord(Convert.ToInt32(readerData[0]));
                metadataRecord.GeneralFlags.DataType = (DataType)Convert.ToInt32(readerData[1]);
                metadataRecord.Name = Convert.ToString(readerData[2]);
                metadataRecord.Synonym1 = Convert.ToString(readerData[3]);
                metadataRecord.Synonym2 = Convert.ToString(readerData[4]);
                metadataRecord.Synonym3 = Convert.ToString(readerData[5]);
                metadataRecord.Description = Convert.ToString(readerData[6]);
                metadataRecord.HardwareInfo = Convert.ToString(readerData[7]);
                metadataRecord.Remarks = Convert.ToString(readerData[8]);
                metadataRecord.PlantCode = Convert.ToString(readerData[9]);
                metadataRecord.UnitNumber = Convert.ToInt32(readerData[10]);
                metadataRecord.SystemName = Convert.ToString(readerData[11]);
                metadataRecord.SourceID = Convert.ToInt32(readerData[12]);
                metadataRecord.GeneralFlags.Enabled = Convert.ToBoolean(readerData[13]);
                metadataRecord.ScanRate = Convert.ToSingle(readerData[14]);
                metadataRecord.CompressionMinTime = Convert.ToInt32(readerData[15]);
                metadataRecord.CompressionMaxTime = Convert.ToInt32(readerData[16]);
                metadataRecord.SecurityFlags.ChangeSecurity = Convert.ToInt32(readerData[30]);
                metadataRecord.SecurityFlags.AccessSecurity = Convert.ToInt32(readerData[31]);
                metadataRecord.GeneralFlags.StepCheck = Convert.ToBoolean(readerData[32]);
                metadataRecord.GeneralFlags.AlarmEnabled = Convert.ToBoolean(readerData[33]);
                metadataRecord.AlarmFlags.Value = Convert.ToInt32(readerData[34]);
                metadataRecord.GeneralFlags.AlarmToFile = Convert.ToBoolean(readerData[36]);
                metadataRecord.GeneralFlags.AlarmByEmail = Convert.ToBoolean(readerData[37]);
                metadataRecord.GeneralFlags.AlarmByPager = Convert.ToBoolean(readerData[38]);
                metadataRecord.GeneralFlags.AlarmByPhone = Convert.ToBoolean(readerData[39]);
                metadataRecord.AlarmEmails = Convert.ToString(readerData[40]);
                metadataRecord.AlarmPagers = Convert.ToString(readerData[41]);
                metadataRecord.AlarmPhones = Convert.ToString(readerData[42]);
                if (metadataRecord.GeneralFlags.DataType == DataType.Analog)
                {
                    metadataRecord.AnalogFields.EngineeringUnits = Convert.ToString(readerData[17]);
                    metadataRecord.AnalogFields.LowWarning = Convert.ToSingle(readerData[18]);
                    metadataRecord.AnalogFields.HighWarning = Convert.ToSingle(readerData[19]);
                    metadataRecord.AnalogFields.LowAlarm = Convert.ToSingle(readerData[20]);
                    metadataRecord.AnalogFields.HighAlarm = Convert.ToSingle(readerData[21]);
                    metadataRecord.AnalogFields.LowRange = Convert.ToSingle(readerData[22]);
                    metadataRecord.AnalogFields.HighRange = Convert.ToSingle(readerData[23]);
                    metadataRecord.AnalogFields.CompressionLimit = Convert.ToSingle(readerData[24]);
                    metadataRecord.AnalogFields.ExceptionLimit = Convert.ToSingle(readerData[25]);
                    metadataRecord.AnalogFields.DisplayDigits = Convert.ToInt32(readerData[26]);
                    metadataRecord.AnalogFields.AlarmDelay = Convert.ToSingle(readerData[35]);
                }
                else if (metadataRecord.GeneralFlags.DataType == DataType.Digital)
                {
                    metadataRecord.DigitalFields.SetDescription = Convert.ToString(readerData[27]);
                    metadataRecord.DigitalFields.ClearDescription = Convert.ToString(readerData[28]);
                    metadataRecord.DigitalFields.AlarmState = Convert.ToInt32(readerData[29]);
                    metadataRecord.DigitalFields.AlarmDelay = Convert.ToSingle(readerData[35]);
                }

                m_metadata.Write(metadataRecord.HistorianID, metadataRecord);
            }
            m_metadata.Save();
        }

        /// <summary>
        /// Updates the <see cref="Metadata"/> from <paramref name="streamData"/>
        /// </summary>
        /// <param name="streamData"><see cref="Stream"/> containing serialized <see cref="SerializableMetadata"/>.</param>
        /// <param name="dataFormat"><see cref="Services.SerializationFormat"/> in which the <see cref="SerializableMetadata"/> was serialized to <paramref name="streamData"/>.</param>
        public void UpdateMetadata(Stream streamData, Services.SerializationFormat dataFormat)
        {
            // Deserialize serialized metadata.
            SerializableMetadata deserializedMetadata = Services.Serialization.Deserialize<SerializableMetadata>(streamData, dataFormat);

            // Update metadata from the deserialized metadata.
            foreach (SerializableMetadataRecord deserializedMetadataRecord in deserializedMetadata.MetadataRecords)
            {
                m_metadata.Write(deserializedMetadataRecord.HistorianID, deserializedMetadataRecord.Deflate());
            }
            m_metadata.Save();
        }

        #endregion       
    }
}
