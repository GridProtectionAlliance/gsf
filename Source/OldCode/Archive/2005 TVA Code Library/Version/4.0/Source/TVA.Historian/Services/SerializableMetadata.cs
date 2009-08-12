//*******************************************************************************************************
//  SerializableMetadata.cs
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVA.Historian.Files;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Represents a container for <see cref="SerializableMetadataRecord"/>s that can be serialized using <see cref="XmlSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    /// <example>
    /// This is the output for <see cref="SerializableMetadata"/> serialized using <see cref="XmlSerializer"/>:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?> 
    /// <Metadata xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    ///   <MetadataRecords>
    ///     <MetadataRecord HistorianID="1" DataType="0" Name="TVA_CORD-BUS2:ABBV" Synonym1="4-PM1" Synonym2="VPHM" Synonym3="" 
    ///       Description="Cordova ABB-521 500 kV Bus 2 Positive Sequence Voltage Magnitude" HardwareInfo="ABB RES521" Remarks="" 
    ///       PlantCode="P1" UnitNumber="1" SystemName="CORD" SourceID="3" Enabled="true" ScanRate="0.0333333351" CompressionMinTime="0" 
    ///       CompressionMaxTime="0" EngineeringUnits="Volts" LowWarning="475000" HighWarning="525000" LowAlarm="450000" HighAlarm="550000" 
    ///       LowRange="475000" HighRange="525000" CompressionLimit="0" ExceptionLimit="0" DisplayDigits="7" SetDescription="" ClearDescription="" 
    ///       AlarmState="0" ChangeSecurity="5" AccessSecurity="0" StepCheck="false" AlarmEnabled="false" AlarmFlags="0" AlarmDelay="0" AlarmToFile="false" 
    ///       AlarmByEmail="false" AlarmByPager="false" AlarmByPhone="false" AlarmEmails="" AlarmPagers="" AlarmPhones="" /> 
    ///   </MetadataRecords>
    /// </Metadata>
    /// ]]>
    /// </code>
    /// This is the output for <see cref="SerializableMetadata"/> serialized using <see cref="DataContractSerializer"/>:
    /// <code>
    /// <![CDATA[
    /// <Metadata xmlns="http://schemas.datacontract.org/2004/07/TVA.Historian.Services" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
    ///   <MetadataRecords>
    ///     <MetadataRecord>
    ///       <AccessSecurity>0</AccessSecurity> 
    ///       <AlarmByEmail>false</AlarmByEmail> 
    ///       <AlarmByPager>false</AlarmByPager> 
    ///       <AlarmByPhone>false</AlarmByPhone> 
    ///       <AlarmDelay>0</AlarmDelay> 
    ///       <AlarmEmails /> 
    ///       <AlarmEnabled>false</AlarmEnabled> 
    ///       <AlarmFlags>0</AlarmFlags> 
    ///       <AlarmPagers /> 
    ///       <AlarmPhones /> 
    ///       <AlarmState>0</AlarmState> 
    ///       <AlarmToFile>false</AlarmToFile> 
    ///       <ChangeSecurity>5</ChangeSecurity> 
    ///       <ClearDescription /> 
    ///       <CompressionLimit>0</CompressionLimit> 
    ///       <CompressionMaxTime>0</CompressionMaxTime> 
    ///       <CompressionMinTime>0</CompressionMinTime> 
    ///       <DataType>0</DataType> 
    ///       <Description>Cordova ABB-521 500 kV Bus 2 Positive Sequence Voltage Magnitude</Description> 
    ///       <DisplayDigits>7</DisplayDigits> 
    ///       <Enabled>true</Enabled> 
    ///       <EngineeringUnits>Volts</EngineeringUnits> 
    ///       <ExceptionLimit>0</ExceptionLimit> 
    ///       <HardwareInfo>ABB RES521</HardwareInfo> 
    ///       <HighAlarm>550000</HighAlarm> 
    ///       <HighRange>525000</HighRange> 
    ///       <HighWarning>525000</HighWarning> 
    ///       <HistorianID>1</HistorianID> 
    ///       <LowAlarm>450000</LowAlarm> 
    ///       <LowRange>475000</LowRange> 
    ///       <LowWarning>475000</LowWarning> 
    ///       <Name>TVA_CORD-BUS2:ABBV</Name> 
    ///       <PlantCode>P1</PlantCode> 
    ///       <Remarks /> 
    ///       <ScanRate>0.0333333351</ScanRate> 
    ///       <SetDescription /> 
    ///       <SourceID>3</SourceID> 
    ///       <StepCheck>false</StepCheck> 
    ///       <Synonym1>4-PM1</Synonym1> 
    ///       <Synonym2>VPHM</Synonym2> 
    ///       <Synonym3 /> 
    ///       <SystemName>CORD</SystemName> 
    ///       <UnitNumber>1</UnitNumber> 
    ///     </MetadataRecord>
    ///   </MetadataRecords>
    /// </Metadata>
    /// ]]>
    /// </code>
    /// This is the output for <see cref="SerializableMetadata"/> serialized using <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>:
    /// <code>
    /// {
    ///   "MetadataRecords":
    ///     [{"AccessSecurity":0,
    ///       "AlarmByEmail":false,
    ///       "AlarmByPager":false,
    ///       "AlarmByPhone":false,
    ///       "AlarmDelay":0,
    ///       "AlarmEmails":"",
    ///       "AlarmEnabled":false,
    ///       "AlarmFlags":0,
    ///       "AlarmPagers":"",
    ///       "AlarmPhones":"",
    ///       "AlarmState":0,
    ///       "AlarmToFile":false,
    ///       "ChangeSecurity":5,
    ///       "ClearDescription":"",
    ///       "CompressionLimit":0,
    ///       "CompressionMaxTime":0,
    ///       "CompressionMinTime":0,
    ///       "DataType":0,
    ///       "Description":"Cordova ABB-521 500 kV Bus 2 Positive Sequence Voltage Magnitude",
    ///       "DisplayDigits":7,
    ///       "Enabled":true,
    ///       "EngineeringUnits":"Volts",
    ///       "ExceptionLimit":0,
    ///       "HardwareInfo":"ABB RES521",
    ///       "HighAlarm":550000,
    ///       "HighRange":525000,
    ///       "HighWarning":525000,
    ///       "HistorianID":1,
    ///       "LowAlarm":450000,
    ///       "LowRange":475000,
    ///       "LowWarning":475000,
    ///       "Name":"TVA_CORD-BUS2:ABBV",
    ///       "PlantCode":"P1",
    ///       "Remarks":"",
    ///       "ScanRate":0.0333333351,
    ///       "SetDescription":"",
    ///       "SourceID":3,
    ///       "StepCheck":false,
    ///       "Synonym1":"4-PM1",
    ///       "Synonym2":"VPHM",
    ///       "Synonym3":"",
    ///       "SystemName":"CORD",
    ///       "UnitNumber":1}]
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="SerializableMetadataRecord"/>
    /// <seealso cref="XmlSerializer"/>
    /// <seealso cref="DataContractSerializer"/>
    /// <seealso cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
    [XmlRoot("Metadata"), DataContract(Name = "Metadata")]
    public class SerializableMetadata
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMetadata"/> class.
        /// </summary>
        public SerializableMetadata()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMetadata"/> class.
        /// </summary>
        /// <param name="metadataFile"><see cref="MetadataFile"/> object from which <see cref="SerializableMetadata"/> is to be initialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadataFile"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="metadataFile"/> is closed.</exception>
        public SerializableMetadata(MetadataFile metadataFile)
            : this()
        {
            if (metadataFile == null)
                throw new ArgumentNullException("metadataFile");

            if (!metadataFile.IsOpen)
                throw new ArgumentException("metadataFile is closed.");

            // Process all records in the metadata file.
            List<SerializableMetadataRecord> serializedMetadataRecords = new List<SerializableMetadataRecord>();
            foreach (MetadataRecord metadataRecord in metadataFile.Read())
            {
                serializedMetadataRecords.Add(new SerializableMetadataRecord(metadataRecord));
            }
            MetadataRecords = serializedMetadataRecords.ToArray();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="SerializableMetadataRecord"/>s contained in the <see cref="SerializableMetadata"/>.
        /// </summary>
        [XmlArray(), DataMember()]
        public SerializableMetadataRecord[] MetadataRecords { get; set; }

        #endregion
    }
}
