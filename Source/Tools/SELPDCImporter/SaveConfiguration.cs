//******************************************************************************************************
//  SaveConfiguration.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/10/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using SELPDCImporter.Model;
using GSF;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.PhasorProtocols;
using GSF.Units.EE;
using Phasor = SELPDCImporter.Model.Phasor;
using SignalType = SELPDCImporter.Model.SignalType;

namespace SELPDCImporter
{
    public static class TableOperationExtensions
    {
        public static Device FindDeviceByEndPoint(this Device[] devices, string endPoint)
        {
            // Lookup by DNS?
            //IPEndPoint resolvedEndPoint = new IPEndPoint(IPAddress.TryParse)

            foreach (Device device in devices)
            {
                Dictionary<string, string> settings = device.ConnectionString?.ParseKeyValuePairs();

                if (settings is null)
                    continue;

                //if (settings.TryGetValue("server", out string endPoint) && port.Equals(portName, StringComparison.OrdinalIgnoreCase))
                //    return device;
            }

            return null;
        }

        public static void AddNewDevice(this TableOperations<Device> deviceTable, Device device) => 
            deviceTable.AddNewRecord(device);

        public static Device NewDevice(this TableOperations<Device> deviceTable) => 
            deviceTable.NewRecord();

        public static Device QueryDevice(this TableOperations<Device> deviceTable, string acronym) => 
            deviceTable.QueryRecordWhere("Acronym = {0}", acronym) ?? deviceTable.NewDevice();

        public static void UpdateDevice(this TableOperations<Device> deviceTable, Device device) => 
            deviceTable.UpdateRecord(device);

        public static IEnumerable<SignalType> LoadSignalTypes(this TableOperations<SignalType> signalTypeTable, string source) => 
            signalTypeTable.QueryRecordsWhere("Source = {0}", source);

        public static Measurement NewMeasurement(this TableOperations<Measurement> measurementTable) => 
            measurementTable.NewRecord();

        public static Measurement QueryMeasurement(this TableOperations<Measurement> measurementTable, string signalReference) => 
            measurementTable.QueryRecordWhere("SignalReference = {0}", signalReference) ?? measurementTable.NewMeasurement();

        public static void AddNewOrUpdateMeasurement(this TableOperations<Measurement> measurementTable, Measurement measurement) => 
            measurementTable.AddNewOrUpdateRecord(measurement);

        public static IEnumerable<Phasor> QueryPhasorsForDevice(this TableOperations<Phasor> phasorTable, int deviceID) => 
            phasorTable.QueryRecordsWhere("DeviceID = {0}", deviceID).OrderBy(phasor => phasor.SourceIndex);

        public static int DeletePhasorsForDevice(this AdoDataConnection connection, int deviceID) => 
            connection.ExecuteScalar<int>("DELETE FROM Phasor WHERE DeviceID = {0}", deviceID);

        public static Phasor NewPhasor(this TableOperations<Phasor> phasorTable) => 
            phasorTable.NewRecord();

        public static void AddNewPhasor(this TableOperations<Phasor> phasorTable, Phasor phasor) =>
            phasorTable.AddNewRecord(phasor);

        public static Phasor QueryPhasorForDevice(this TableOperations<Phasor> phasorTable, int deviceID, int sourceIndex) => 
            phasorTable.QueryRecordWhere("DeviceID = {0} AND SourceIndex = {1}", deviceID, sourceIndex) ?? phasorTable.NewPhasor();
    }

    partial class MainForm
    {
        // Connection string template
        private const string ConnectionStringTemplate = "transportProtocol=Serial; port=COM{0}; baudrate={1}; parity={2}; stopbits={3}; databits={4}; dtrenable={5}; rtsenable={6}; {7}";

        // Connection string parameters of system that is controlling COM connection
        private const string ControllingConnectionString = "autoStartDataParsingSequence = true; skipDisableRealTimeData = false; disableRealTimeDataOnStop = false";

        // Connection string parameters of system that is only listening to COM connection
        private const string ListeningConnectionString = "autoStartDataParsingSequence = false; skipDisableRealTimeData = true; disableRealTimeDataOnStop = false";

        private Dictionary<string, SignalType> m_deviceSignalTypes;
        private Dictionary<string, SignalType> m_phasorSignalTypes;

        private string GetCleanAcronym(string acronym)
        {
            // Remove any invalid characters from acronym
            return Regex.Replace(acronym, @"[^A-Z0-9\-!_\.@#\$]", "", RegexOptions.IgnoreCase);
        }

        private bool SaveDeviceConfiguration(IConfigurationFrame configFrame, string endPoint, int idCode, ImportParameters scanParams)
        {
            try
            {
                AdoDataConnection connection = scanParams.Connection;
                TableOperations<SignalType> signalTypeTable = new TableOperations<SignalType>(connection);

                // TODO: Fix connection string
                //string configConnectionMode = ControllingConnectionString;
                string connectionString = ""; // string.Format(ConnectionStringTemplate, comPort, Settings.BaudRate, Settings.Parity, Settings.StopBits, Settings.DataBits, Settings.DtrEnable, Settings.RtsEnable, configConnectionMode);

                //ShowUpdateMessage($"{Tab2}Saving \"{configFrame.Cells[0].StationName}\" configuration received on COM{comPort} with ID code {idCode}...");

                if (m_deviceSignalTypes is null)
                    m_deviceSignalTypes = signalTypeTable.LoadSignalTypes("PMU").ToDictionary(key => key.Acronym, StringComparer.OrdinalIgnoreCase);

                if (m_phasorSignalTypes is null)
                    m_phasorSignalTypes = signalTypeTable.LoadSignalTypes("Phasor").ToDictionary(key => key.Acronym, StringComparer.OrdinalIgnoreCase);

                SaveDeviceConnection(configFrame, connectionString, endPoint, idCode, scanParams);
                
                return true;
            }
            catch (Exception ex)
            {
                //ShowUpdateMessage($"{Tab2}ERROR: Failed while saving \"{configFrame.Cells[0].StationName}\" configuration: {ex.Message}");
                m_log.Publish(MessageLevel.Error, nameof(SELPDCImporter), exception: ex);
                
                return false;
            }
        }

        private void SaveDeviceConnection(IConfigurationFrame configFrame, string connectionString, string endPoint, int idCode, ImportParameters scanParams)
        {
            TableOperations<Device> deviceTable = scanParams.DeviceTable;
            Guid nodeID = scanParams.NodeID;

            //ShowUpdateMessage($"{Tab2}Saving device connection...");
            // TODO: Consider alternatives for this existing device lookup
            Device device = scanParams.Devices.FindDeviceByEndPoint(endPoint) ?? deviceTable.NewDevice();
            Dictionary<string, string> connectionStringMap = connectionString.ParseKeyValuePairs();

            bool autoStartDataParsingSequence = true;
            bool skipDisableRealTimeData = false;

            // Handle connection string parameters that are fields in the device table
            if (connectionStringMap.ContainsKey("autoStartDataParsingSequence"))
            {
                autoStartDataParsingSequence = bool.Parse(connectionStringMap["autoStartDataParsingSequence"]);
                connectionStringMap.Remove("autoStartDataParsingSequence");
                connectionString = connectionStringMap.JoinKeyValuePairs();
            }

            if (connectionStringMap.ContainsKey("skipDisableRealTimeData"))
            {
                skipDisableRealTimeData = bool.Parse(connectionStringMap["skipDisableRealTimeData"]);
                connectionStringMap.Remove("skipDisableRealTimeData");
                connectionString = connectionStringMap.JoinKeyValuePairs();
            }

            IConfigurationCell deviceConfig = configFrame.Cells[0];

            string deviceAcronym = deviceConfig.IDLabel;
            string deviceName = null;

            if (string.IsNullOrWhiteSpace(deviceAcronym) && !string.IsNullOrWhiteSpace(deviceConfig.StationName))
                deviceAcronym = GetCleanAcronym(deviceConfig.StationName.ToUpperInvariant().Replace(" ", "_"));
            else
                throw new InvalidOperationException("Unable to get station name or ID label from device configuration frame");

            if (!string.IsNullOrWhiteSpace(deviceConfig.StationName))
                deviceName = deviceConfig.StationName;

            device.NodeID = nodeID;
            device.Acronym = deviceAcronym;
            device.Name = deviceName ?? deviceAcronym;
            device.ProtocolID = scanParams.IeeeC37_118ProtocolID;
            device.FramesPerSecond = configFrame.FrameRate;
            device.AccessID = idCode;
            device.IsConcentrator = false;
            device.ConnectionString = connectionString;
            device.AutoStartDataParsingSequence = autoStartDataParsingSequence;
            device.SkipDisableRealTimeData = skipDisableRealTimeData;
            device.Enabled = true;

            // Check if this is a new device or an edit to an existing one
            if (device.ID == 0)
            {
                // Add new device record
                deviceTable.AddNewDevice(device);

                // Get newly added device with auto-incremented ID
                Device newDevice = deviceTable.QueryDevice(device.Acronym);

                // Save associated device records
                SaveDeviceRecords(configFrame, newDevice, scanParams);
            }
            else
            {
                // Update existing device record
                deviceTable.UpdateDevice(device);
                
                // Save associated device records
                SaveDeviceRecords(configFrame, device, scanParams);
            }
        }

        private void SaveDeviceRecords(IConfigurationFrame configFrame, Device device, ImportParameters scanParams)
        {
            AdoDataConnection connection = scanParams.Connection;
            TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(connection);
            IConfigurationCell cell = configFrame.Cells[0];

            // Add frequency
            SaveFixedMeasurement(m_deviceSignalTypes["FREQ"], device, measurementTable, scanParams, cell.FrequencyDefinition.Label);

            // Add dF/dt
            SaveFixedMeasurement(m_deviceSignalTypes["DFDT"], device, measurementTable, scanParams);

            // Add status flags
            SaveFixedMeasurement(m_deviceSignalTypes["FLAG"], device, measurementTable, scanParams);

            // Add analogs
            SignalType analogSignalType = m_deviceSignalTypes["ALOG"];

            for (int i = 0; i < cell.AnalogDefinitions.Count; i++)
            {
                int index = i + 1;
                IAnalogDefinition analogDefinition = cell.AnalogDefinitions[i];
                string signalReference = $"{device.Acronym}-{analogSignalType.Suffix}{index}";

                // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
                Measurement measurement = measurementTable.QueryMeasurement(signalReference);
                string pointTag = scanParams.CreateIndexedPointTag(device.Acronym, analogSignalType.Acronym, index);
                measurement.DeviceID = device.ID;
                measurement.PointTag = pointTag;
                measurement.AlternateTag = analogDefinition.Label;
                measurement.Description = $"{device.Acronym} Analog Value {index} {analogDefinition.AnalogType}: {analogDefinition.Label}";
                measurement.SignalReference = signalReference;
                measurement.SignalTypeID = analogSignalType.ID;
                measurement.Internal = true;
                measurement.Enabled = true;

                measurementTable.AddNewOrUpdateMeasurement(measurement);
            }

            // Add digitals
            SignalType digitalSignalType = m_deviceSignalTypes["DIGI"];

            for (int i = 0; i < cell.DigitalDefinitions.Count; i++)
            {
                int index = i + 1;
                IDigitalDefinition digitialDefinition = cell.DigitalDefinitions[i];
                string signalReference = $"{device.Acronym}-{digitalSignalType.Suffix}{index}";

                // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
                Measurement measurement = measurementTable.QueryMeasurement(signalReference);
                string pointTag = scanParams.CreateIndexedPointTag(device.Acronym, digitalSignalType.Acronym, index);
                measurement.DeviceID = device.ID;
                measurement.PointTag = pointTag;
                measurement.AlternateTag = digitialDefinition.Label;
                measurement.Description = $"{device.Acronym} Digital Value {index}: {digitialDefinition.Label}";
                measurement.SignalReference = signalReference;
                measurement.SignalTypeID = digitalSignalType.ID;
                measurement.Internal = true;
                measurement.Enabled = true;

                measurementTable.AddNewOrUpdateMeasurement(measurement);
            }

            // Add phasors
            SaveDevicePhasors(cell, device, measurementTable, scanParams);
        }

        private void SaveFixedMeasurement(SignalType signalType, Device device, TableOperations<Measurement> measurementTable, ImportParameters scanParams, string label = null)
        {
            string signalReference = $"{device.Acronym}-{signalType.Suffix}";

            // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
            Measurement measurement = measurementTable.QueryMeasurement(signalReference);
            string pointTag = scanParams.CreatePointTag(device.Acronym, signalType.Acronym);
            measurement.DeviceID = device.ID;
            measurement.PointTag = pointTag;
            measurement.Description = $"{device.Acronym} {signalType.Name}{(string.IsNullOrWhiteSpace(label) ? "" : " - " + label)}";
            measurement.SignalReference = signalReference;
            measurement.SignalTypeID = signalType.ID;
            measurement.Internal = true;
            measurement.Enabled = true;

            measurementTable.AddNewOrUpdateMeasurement(measurement);
        }

        private void SaveDevicePhasors(IConfigurationCell cell, Device device, TableOperations<Measurement> measurementTable, ImportParameters scanParams)
        {
            AdoDataConnection connection = scanParams.Connection;
            TableOperations<Phasor> phasorTable = new TableOperations<Phasor>(connection);

            // Get phasor signal types
            SignalType iphmSignalType = m_phasorSignalTypes["IPHM"];
            SignalType iphaSignalType = m_phasorSignalTypes["IPHA"];
            SignalType vphmSignalType = m_phasorSignalTypes["VPHM"];
            SignalType vphaSignalType = m_phasorSignalTypes["VPHA"];

            Phasor[] phasors = phasorTable.QueryPhasorsForDevice(device.ID).ToArray();

            bool dropAndAdd = phasors.Length != cell.PhasorDefinitions.Count;

            if (!dropAndAdd)
            {
                // Also do add operation if phasor source index records are not sequential
                for (int i = 0; i < phasors.Length; i++)
                {
                    if (phasors[i].SourceIndex != i + 1)
                    {
                        dropAndAdd = true;
                        break;
                    }
                }
            }

            if (dropAndAdd)
            {
                if (cell.PhasorDefinitions.Count > 0)
                    connection.DeletePhasorsForDevice(device.ID);

                foreach (IPhasorDefinition phasorDefinition in cell.PhasorDefinitions)
                {
                    bool isVoltage = phasorDefinition.PhasorType == PhasorType.Voltage;

                    Phasor phasor = phasorTable.NewPhasor();
                    phasor.DeviceID = device.ID;
                    phasor.Label = phasorDefinition.Label;
                    phasor.Type = isVoltage ? 'V' : 'I';
                    phasor.Phase = '+';     // TODO: Make a guess later
                    phasor.BaseKV = 500;    // TODO: Make a guess later
                    phasor.DestinationPhasorID = null;
                    phasor.SourceIndex = phasorDefinition.Index;

                    phasorTable.AddNewPhasor(phasor);
                    SavePhasorMeasurement(isVoltage ? vphmSignalType : iphmSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable, scanParams);
                    SavePhasorMeasurement(isVoltage ? vphaSignalType : iphaSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable, scanParams);
                }
            }
            else
            {
                foreach (IPhasorDefinition phasorDefinition in cell.PhasorDefinitions)
                {
                    bool isVoltage = phasorDefinition.PhasorType == PhasorType.Voltage;

                    Phasor phasor = phasorTable.QueryPhasorForDevice(device.ID, phasorDefinition.Index);
                    phasor.DeviceID = device.ID;
                    phasor.Label = phasorDefinition.Label;
                    phasor.Type = isVoltage ? 'V' : 'I';

                    phasorTable.AddNewPhasor(phasor);
                    SavePhasorMeasurement(isVoltage ? vphmSignalType : iphmSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable, scanParams);
                    SavePhasorMeasurement(isVoltage ? vphaSignalType : iphaSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable, scanParams);
                }
            }
        }

        private void SavePhasorMeasurement(SignalType signalType, Device device, IPhasorDefinition phasorDefinition, int index, TableOperations<Measurement> measurementTable, ImportParameters scanParams)
        {
            string signalReference = $"{device.Acronym}-{signalType.Suffix}{index}";

            // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
            Measurement measurement = measurementTable.QueryMeasurement(signalReference);
            string pointTag = scanParams.CreatePhasorPointTag(device.Acronym, signalType.Acronym, phasorDefinition.Label, "+", index, 0);

            measurement.DeviceID = device.ID;
            measurement.PointTag = pointTag;
            measurement.Description = $"{device.Acronym} {phasorDefinition.Label} {signalType.Name}";
            measurement.PhasorSourceIndex = index;
            measurement.SignalReference = signalReference;
            measurement.SignalTypeID = signalType.ID;
            measurement.Internal = true;
            measurement.Enabled = true;

            measurementTable.AddNewOrUpdateMeasurement(measurement);
        }
    }
}