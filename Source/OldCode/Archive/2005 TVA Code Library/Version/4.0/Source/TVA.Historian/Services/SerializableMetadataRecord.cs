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
    /// Represents a flattened <see cref="MetadataRecord"/> that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    /// <example>
    /// This is the output for <see cref="SerializableMetadataRecord"/> serialized using <see cref="XmlSerializer"/>:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?> 
    /// <MetadataRecord HistorianID="1" DataType="0" Name="TVA_CORD-BUS2:ABBV" Synonym1="4-PM1" Synonym2="VPHM" Synonym3="" Description="Cordova ABB-521 500 kV Bus 2 Positive Sequence Voltage Magnitude" HardwareInfo="ABB RES521" 
    ///   Remarks="" PlantCode="P1" UnitNumber="1" SystemName="CORD" SourceID="3" Enabled="true" ScanRate="0.0333333351" CompressionMinTime="0" CompressionMaxTime="0" EngineeringUnits="Volts" LowWarning="475000" HighWarning="525000" 
    ///   LowAlarm="450000" HighAlarm="550000" LowRange="475000" HighRange="525000" CompressionLimit="0" ExceptionLimit="0" DisplayDigits="7" SetDescription="" ClearDescription="" AlarmState="0" ChangeSecurity="5" AccessSecurity="0" 
    ///   StepCheck="false" AlarmEnabled="false" AlarmFlags="0" AlarmDelay="0" AlarmToFile="false" AlarmByEmail="false" AlarmByPager="false" AlarmByPhone="false" AlarmEmails="" AlarmPagers="" AlarmPhones="" 
    ///   xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" /> 
    /// ]]>
    /// </code>
    /// This is the output for <see cref="SerializableMetadataRecord"/> serialized using <see cref="DataContractSerializer"/>:
    /// <code>
    /// <![CDATA[
    /// <MetadataRecord xmlns="http://schemas.datacontract.org/2004/07/TVA.Historian.Services" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
    ///   <AccessSecurity>0</AccessSecurity> 
    ///   <AlarmByEmail>false</AlarmByEmail> 
    ///   <AlarmByPager>false</AlarmByPager> 
    ///   <AlarmByPhone>false</AlarmByPhone> 
    ///   <AlarmDelay>0</AlarmDelay> 
    ///   <AlarmEmails /> 
    ///   <AlarmEnabled>false</AlarmEnabled> 
    ///   <AlarmFlags>0</AlarmFlags> 
    ///   <AlarmPagers /> 
    ///   <AlarmPhones /> 
    ///   <AlarmState>0</AlarmState> 
    ///   <AlarmToFile>false</AlarmToFile> 
    ///   <ChangeSecurity>5</ChangeSecurity> 
    ///   <ClearDescription /> 
    ///   <CompressionLimit>0</CompressionLimit> 
    ///   <CompressionMaxTime>0</CompressionMaxTime> 
    ///   <CompressionMinTime>0</CompressionMinTime> 
    ///   <DataType>0</DataType> 
    ///   <Description>Cordova ABB-521 500 kV Bus 2 Positive Sequence Voltage Magnitude</Description> 
    ///   <DisplayDigits>7</DisplayDigits> 
    ///   <Enabled>true</Enabled> 
    ///   <EngineeringUnits>Volts</EngineeringUnits> 
    ///   <ExceptionLimit>0</ExceptionLimit> 
    ///   <HardwareInfo>ABB RES521</HardwareInfo> 
    ///   <HighAlarm>550000</HighAlarm> 
    ///   <HighRange>525000</HighRange> 
    ///   <HighWarning>525000</HighWarning> 
    ///   <HistorianID>1</HistorianID> 
    ///   <LowAlarm>450000</LowAlarm> 
    ///   <LowRange>475000</LowRange> 
    ///   <LowWarning>475000</LowWarning> 
    ///   <Name>TVA_CORD-BUS2:ABBV</Name> 
    ///   <PlantCode>P1</PlantCode> 
    ///   <Remarks /> 
    ///   <ScanRate>0.0333333351</ScanRate> 
    ///   <SetDescription /> 
    ///   <SourceID>3</SourceID> 
    ///   <StepCheck>false</StepCheck> 
    ///   <Synonym1>4-PM1</Synonym1> 
    ///   <Synonym2>VPHM</Synonym2> 
    ///   <Synonym3 /> 
    ///   <SystemName>CORD</SystemName> 
    ///   <UnitNumber>1</UnitNumber> 
    /// </MetadataRecord>
    /// ]]>
    /// </code>
    /// This is the output for <see cref="SerializableMetadataRecord"/> serialized using <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>:
    /// <code>
    /// {
    ///   "AccessSecurity":0,
    ///   "AlarmByEmail":false,
    ///   "AlarmByPager":false,
    ///   "AlarmByPhone":false,
    ///   "AlarmDelay":0,
    ///   "AlarmEmails":"",
    ///   "AlarmEnabled":false,
    ///   "AlarmFlags":0,
    ///   "AlarmPagers":"",
    ///   "AlarmPhones":"",
    ///   "AlarmState":0,
    ///   "AlarmToFile":false,
    ///   "ChangeSecurity":5,
    ///   "ClearDescription":"",
    ///   "CompressionLimit":0,
    ///   "CompressionMaxTime":0,
    ///   "CompressionMinTime":0,
    ///   "DataType":0,
    ///   "Description":"Cordova ABB-521 500 kV Bus 2 Positive Sequence Voltage Magnitude",
    ///   "DisplayDigits":7,
    ///   "Enabled":true,
    ///   "EngineeringUnits":"Volts",
    ///   "ExceptionLimit":0,
    ///   "HardwareInfo":"ABB RES521",
    ///   "HighAlarm":550000,
    ///   "HighRange":525000,
    ///   "HighWarning":525000,
    ///   "HistorianID":1,
    ///   "LowAlarm":450000,
    ///   "LowRange":475000,
    ///   "LowWarning":475000,
    ///   "Name":"TVA_CORD-BUS2:ABBV",
    ///   "PlantCode":"P1",
    ///   "Remarks":"",
    ///   "ScanRate":0.0333333351,
    ///   "SetDescription":"",
    ///   "SourceID":3,
    ///   "StepCheck":false,
    ///   "Synonym1":"4-PM1",
    ///   "Synonym2":"VPHM",
    ///   "Synonym3":"",
    ///   "SystemName":"CORD",
    ///   "UnitNumber":1
    /// }
    /// </code>
    /// </example>
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
        /// Gets or sets the <see cref="MetadataRecord.HistorianID"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int HistorianID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.DataType"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int DataType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.Name"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.Synonym1"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Synonym1 { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.Synonym2"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Synonym2 { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.Synonym3"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Synonym3 { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.Description"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.HardwareInfo"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string HardwareInfo { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.Remarks"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string Remarks { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.PlantCode"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string PlantCode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.UnitNumber"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int UnitNumber { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.SystemName"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.SourceID"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int SourceID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.Enabled"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.ScanRate"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float ScanRate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.CompressionMinTime"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int CompressionMinTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.CompressionMaxTime"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int CompressionMaxTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.EngineeringUnits"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string EngineeringUnits { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.LowWarning"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float LowWarning { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.HighWarning"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float HighWarning { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.LowAlarm"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float LowAlarm { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.HighAlarm"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float HighAlarm { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.LowRange"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float LowRange { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.HighRange"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float HighRange { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.CompressionLimit"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float CompressionLimit { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.ExceptionLimit"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float ExceptionLimit { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.DisplayDigits"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int DisplayDigits { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordDigitalFields.SetDescription"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string SetDescription { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordDigitalFields.ClearDescription"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string ClearDescription { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordDigitalFields.AlarmState"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int AlarmState { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordSecurityFlags.ChangeSecurity"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int ChangeSecurity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordSecurityFlags.AccessSecurity"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int AccessSecurity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.StepCheck"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool StepCheck { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.AlarmEnabled"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmEnabled { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAlarmFlags.Value"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public int AlarmFlags { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordAnalogFields.AlarmDelay"/> or <see cref="MetadataRecordDigitalFields.AlarmDelay"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public float AlarmDelay { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.AlarmToFile"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmToFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.AlarmByEmail"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmByEmail { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.AlarmByPager"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmByPager { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecordGeneralFlags.AlarmByPhone"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public bool AlarmByPhone { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.AlarmEmails"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string AlarmEmails { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.AlarmPagers"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string AlarmPagers { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataRecord.AlarmPhones"/>.
        /// </summary>
        [XmlAttribute(), DataMember()]
        public string AlarmPhones { get; set; }

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
