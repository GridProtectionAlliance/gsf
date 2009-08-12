//*******************************************************************************************************
//  SerializableMetadataRecord.cs
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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVA.Historian.Files;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Represents a flattened <see cref="MetadataRecord"/> that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>..
    /// </summary>
    /// <seealso cref="XmlSerializer"/>
    /// <seealso cref="DataContractSerializer"/>
    /// <seealso cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
    [XmlType("MetadataRecord"), DataContract(Name = "MetadataRecord")]
    public class SerializableMetadataRecord
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMetadataRecord"/> class.
        /// </summary>
        public SerializableMetadataRecord()
        {
            Name = string.Empty;
            Synonym1 = string.Empty;
            Synonym2 = string.Empty;
            Synonym3 = string.Empty;
            Description = string.Empty;
            HardwareInfo = string.Empty;
            Remarks = string.Empty;
            PlantCode = string.Empty;
            SystemName = string.Empty;
            EngineeringUnits = string.Empty;
            SetDescription = string.Empty;
            ClearDescription = string.Empty;
            AlarmEmails = string.Empty;
            AlarmPagers = string.Empty;
            AlarmPhones = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMetadataRecord"/> class.
        /// </summary>
        /// <param name="metadataRecord"><see cref="MetadataRecord"/> from which <see cref="SerializableMetadataRecord"/> is to be initialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadataRecord"/> is null.</exception>
        public SerializableMetadataRecord(MetadataRecord metadataRecord)
            : this()
        {
            if (metadataRecord == null)
                throw new ArgumentNullException("metadataRecord");

            HistorianID = metadataRecord.HistorianID;
            DataType = (int)metadataRecord.GeneralFlags.DataType;
            Name = metadataRecord.Name;
            Synonym1 = metadataRecord.Synonym1;
            Synonym2 = metadataRecord.Synonym2;
            Synonym3 = metadataRecord.Synonym3;
            Description = metadataRecord.Description;
            HardwareInfo = metadataRecord.HardwareInfo;
            Remarks = metadataRecord.Remarks;
            PlantCode = metadataRecord.PlantCode;
            UnitNumber = metadataRecord.UnitNumber;
            SystemName = metadataRecord.SystemName;
            SourceID = metadataRecord.SourceID;
            Enabled = metadataRecord.GeneralFlags.Enabled;
            ScanRate = metadataRecord.ScanRate;
            CompressionMinTime = metadataRecord.CompressionMinTime;
            CompressionMaxTime = metadataRecord.CompressionMaxTime;
            ChangeSecurity = metadataRecord.SecurityFlags.ChangeSecurity;
            AccessSecurity = metadataRecord.SecurityFlags.AccessSecurity;
            StepCheck = metadataRecord.GeneralFlags.StepCheck;
            AlarmEnabled = metadataRecord.GeneralFlags.AlarmEnabled;
            AlarmFlags = metadataRecord.AlarmFlags.Value;
            AlarmToFile = metadataRecord.GeneralFlags.AlarmToFile;
            AlarmByEmail = metadataRecord.GeneralFlags.AlarmByEmail;
            AlarmByPager = metadataRecord.GeneralFlags.AlarmByPager;
            AlarmByPhone = metadataRecord.GeneralFlags.AlarmByPhone;
            AlarmEmails = metadataRecord.AlarmEmails;
            AlarmPagers = metadataRecord.AlarmPagers;
            AlarmPhones = metadataRecord.AlarmPhones;
            if (DataType == 0)
            {
                // Analog properties.
                EngineeringUnits = metadataRecord.AnalogFields.EngineeringUnits;
                LowWarning = metadataRecord.AnalogFields.LowWarning;
                HighWarning = metadataRecord.AnalogFields.HighWarning;
                LowAlarm = metadataRecord.AnalogFields.LowAlarm;
                HighAlarm = metadataRecord.AnalogFields.HighAlarm;
                LowRange = metadataRecord.AnalogFields.LowRange;
                HighRange = metadataRecord.AnalogFields.HighRange;
                CompressionLimit = metadataRecord.AnalogFields.CompressionLimit;
                ExceptionLimit = metadataRecord.AnalogFields.ExceptionLimit;
                DisplayDigits = metadataRecord.AnalogFields.DisplayDigits;
                AlarmDelay = metadataRecord.AnalogFields.AlarmDelay;
            }
            else if (DataType == 1)
            {
                // Digital properties.
                SetDescription = metadataRecord.DigitalFields.SetDescription;
                ClearDescription = metadataRecord.DigitalFields.ClearDescription;
                AlarmState = metadataRecord.DigitalFields.AlarmState;
                AlarmDelay = metadataRecord.DigitalFields.AlarmDelay;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="MetadataRecord.HistorianID"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int HistorianID { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.DataType"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int DataType { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.Name"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.Synonym1"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Synonym1 { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.Synonym2"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Synonym2 { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.Synonym3"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Synonym3 { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.Description"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.HardwareInfo"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string HardwareInfo { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.Remarks"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Remarks { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.PlantCode"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string PlantCode { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.UnitNumber"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int UnitNumber { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.SystemName"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string SystemName { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.SourceID"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int SourceID { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.Enabled"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool Enabled { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.ScanRate"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float ScanRate { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.CompressionMinTime"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int CompressionMinTime { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.CompressionMaxTime"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int CompressionMaxTime { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.EngineeringUnits"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string EngineeringUnits { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.LowWarning"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float LowWarning { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.HighWarning"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float HighWarning { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.LowAlarm"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float LowAlarm { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.HighAlarm"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float HighAlarm { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.LowRange"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float LowRange { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.HighRange"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float HighRange { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.CompressionLimit"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float CompressionLimit { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.ExceptionLimit"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float ExceptionLimit { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.DisplayDigits"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int DisplayDigits { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordDigitalFields.SetDescription"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string SetDescription { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordDigitalFields.ClearDescription"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string ClearDescription { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordDigitalFields.AlarmState"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int AlarmState { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordSecurityFlags.ChangeSecurity"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int ChangeSecurity { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordSecurityFlags.AccessSecurity"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int AccessSecurity { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.StepCheck"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool StepCheck { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.AlarmEnabled"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmEnabled { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAlarmFlags.Value"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int AlarmFlags { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordAnalogFields.AlarmDelay"/> or <see cref="MetadataRecordDigitalFields.AlarmDelay"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float AlarmDelay { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.AlarmToFile"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmToFile { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.AlarmByEmail"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmByEmail { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.AlarmByPager"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmByPager { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecordGeneralFlags.AlarmByPhone"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmByPhone { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.AlarmEmails"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string AlarmEmails { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.AlarmPagers"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string AlarmPagers { get; private set; }

        /// <summary>
        /// Gets the <see cref="MetadataRecord.AlarmPhones"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string AlarmPhones { get; private set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="MetadataRecord"/> object for this <see cref="SerializableMetadataRecord"/>.
        /// </summary>
        /// <returns>A <see cref="MetadataRecord"/> object.</returns>
        public MetadataRecord Deflate()
        {
            MetadataRecord metadataRecord = new MetadataRecord(HistorianID);
            metadataRecord.GeneralFlags.DataType = (DataType)DataType;
            metadataRecord.Name = Name;
            metadataRecord.Synonym1 = Synonym1;
            metadataRecord.Synonym2 = Synonym2;
            metadataRecord.Synonym3 = Synonym3;
            metadataRecord.Description = Description;
            metadataRecord.HardwareInfo = HardwareInfo;
            metadataRecord.Remarks = Remarks;
            metadataRecord.PlantCode = PlantCode;
            metadataRecord.UnitNumber = UnitNumber;
            metadataRecord.SystemName = SystemName;
            metadataRecord.SourceID = SourceID;
            metadataRecord.GeneralFlags.Enabled = Enabled;
            metadataRecord.ScanRate = ScanRate;
            metadataRecord.CompressionMinTime = CompressionMinTime;
            metadataRecord.CompressionMaxTime = CompressionMaxTime;
            metadataRecord.SecurityFlags.ChangeSecurity = ChangeSecurity;
            metadataRecord.SecurityFlags.AccessSecurity = AccessSecurity;
            metadataRecord.GeneralFlags.StepCheck = StepCheck;
            metadataRecord.GeneralFlags.AlarmEnabled = AlarmEnabled;
            metadataRecord.AlarmFlags.Value = AlarmFlags;
            metadataRecord.GeneralFlags.AlarmToFile = AlarmToFile;
            metadataRecord.GeneralFlags.AlarmByEmail = AlarmByEmail;
            metadataRecord.GeneralFlags.AlarmByPager = AlarmByPager;
            metadataRecord.GeneralFlags.AlarmByPhone = AlarmByPhone;
            metadataRecord.AlarmEmails = AlarmEmails;
            metadataRecord.AlarmPagers = AlarmPagers;
            metadataRecord.AlarmPhones = AlarmPhones;
            if (DataType == 0)
            {
                // Analog properties.
                metadataRecord.AnalogFields.EngineeringUnits = EngineeringUnits;
                metadataRecord.AnalogFields.LowWarning = LowWarning;
                metadataRecord.AnalogFields.HighWarning = HighWarning;
                metadataRecord.AnalogFields.LowAlarm = LowAlarm;
                metadataRecord.AnalogFields.HighAlarm = HighAlarm;
                metadataRecord.AnalogFields.LowRange = LowRange;
                metadataRecord.AnalogFields.HighRange = HighRange;
                metadataRecord.AnalogFields.CompressionLimit = CompressionLimit;
                metadataRecord.AnalogFields.ExceptionLimit = ExceptionLimit;
                metadataRecord.AnalogFields.DisplayDigits = DisplayDigits;
                metadataRecord.AnalogFields.AlarmDelay = AlarmDelay;
            }
            else if (DataType == 1)
            {
                // Digital properties.
                metadataRecord.DigitalFields.SetDescription = SetDescription;
                metadataRecord.DigitalFields.ClearDescription = ClearDescription;
                metadataRecord.DigitalFields.AlarmState = AlarmState;
                metadataRecord.DigitalFields.AlarmDelay = AlarmDelay;
            }

            return metadataRecord;
        }

        #endregion
    }
}
