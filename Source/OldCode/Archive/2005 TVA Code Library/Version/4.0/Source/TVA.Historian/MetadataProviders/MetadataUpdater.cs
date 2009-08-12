//*******************************************************************************************************
//  MetadataUpdater.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/07/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
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
        public MetadataUpdater(MetadataFile metadata)
        {
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
        public void UpdateMetadata(DataTable tableData)
        {
            if (m_metadata == null)
                throw new ArgumentNullException("Metadata");

            if (tableData == null)
                throw new ArgumentNullException("tableData");

            if (tableData.Rows.Count == 0)
                throw new ArgumentException("tableData must contain data.");

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
        }

        /// <summary>
        /// Updates the <see cref="Metadata"/> from <paramref name="streamData"/>
        /// </summary>
        /// <param name="streamData"><see cref="Stream"/> containing serialized <see cref="SerializableMetadata"/>.</param>
        /// <param name="dataFormat"><see cref="RestDataFormat"/> in which the <see cref="SerializableMetadata"/> is serialized.</param>
        /// <exception cref="NotSupportedException">Specified <paramref name="dataFormat"/> is not supported.</exception>
        public void UpdateMetadata(Stream streamData, RestDataFormat dataFormat)
        {
            // Deserialize serialized metadata.
            SerializableMetadata deserializedMetadata = null;
            if (dataFormat == RestDataFormat.AsmxXml)
            {
                // Data is in ASMX XML format.
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableMetadata));
                deserializedMetadata = (SerializableMetadata)serializer.Deserialize(streamData);
            }
            else if (dataFormat == RestDataFormat.RestXml)
            {
                // Data is in REST XML format.
                DataContractSerializer serializer = new DataContractSerializer(typeof(SerializableMetadata));
                deserializedMetadata = (SerializableMetadata)serializer.ReadObject(streamData);
            }
            else if (dataFormat == RestDataFormat.RestJson)
            {
                // Data is in REST JSON format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SerializableMetadata));
                deserializedMetadata = (SerializableMetadata)serializer.ReadObject(streamData);
            }
            else
            {
                // Data format is not supported.
                throw new NotSupportedException(string.Format("{0} data format is not supported.", dataFormat));
            }

            // Update metadata from the deserialized metadata.
            foreach (SerializableMetadataRecord deserializedMetadataRecord in deserializedMetadata.MetadataRecords)
            {
                m_metadata.Write(deserializedMetadataRecord.HistorianID, deserializedMetadataRecord.Deflate());
            }
        }

        /// <summary>
        /// Serializes <see cref="Metadata"/> to <see cref="SerializableMetadata"/> in the specified <paramref name="dataFormat"/>.
        /// </summary>
        /// <param name="outputStream"><see cref="Stream"/> where serialized <see cref="Metadata"/> is to be outputted.</param>
        /// <param name="dataFormat"><see cref="RestDataFormat"/> in which the <see cref="Metadata"/> is to be serialized.</param>
        /// <exception cref="NotSupportedException">Specified <paramref name="dataFormat"/> is not supported.</exception>
        public void ExtractMetadata(ref Stream outputStream, RestDataFormat dataFormat)
        {
            // Serialize data to the provided stream.
            if (dataFormat == RestDataFormat.AsmxXml)
            {
                // Serialize to ASMX XML data format.
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableMetadata));
                serializer.Serialize(outputStream, new SerializableMetadata(m_metadata));
            }
            else if (dataFormat == RestDataFormat.RestXml)
            {
                // Serialize to REST XML data format.
                DataContractSerializer serializer = new DataContractSerializer(typeof(SerializableMetadata));
                serializer.WriteObject(outputStream, new SerializableMetadata(m_metadata));
            }
            else if (dataFormat == RestDataFormat.RestJson)
            {
                // Serialize to REST JSON data format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SerializableMetadata));
                serializer.WriteObject(outputStream, new SerializableMetadata(m_metadata));
            }
            else
            {
                // Data format is not supported.
                throw new NotSupportedException(string.Format("{0} data format is not supported.", dataFormat));
            }
        }

        #endregion       
    }
}
