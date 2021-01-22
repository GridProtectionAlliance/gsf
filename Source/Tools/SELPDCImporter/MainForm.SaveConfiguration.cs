//******************************************************************************************************
//  MainForm.SaveConfiguration.cs - Gbtc
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
using GSF.PhasorProtocols;
using GSF.Units.EE;
using Phasor = SELPDCImporter.Model.Phasor;
using SignalType = SELPDCImporter.Model.SignalType;

namespace SELPDCImporter
{
    public static class TableOperationExtensions
    {
        public static Device FindDeviceByEndPoint(this Device[] devices, string connectionString)
        {
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

        // Remove any invalid characters from acronym
        public static string GetCleanAcronym(this string acronym) => 
            Regex.Replace(acronym.ToUpperInvariant().Replace(" ", "_"), @"[^A-Z0-9\-!_\.@#\$]", "", RegexOptions.IgnoreCase);
    }

    // TODO: No need for partial form here
    partial class MainForm
    {
        // Connection string template
        private const string ConnectionStringTemplate = "{0}; autoStartDataParsingSequence = true; skipDisableRealTimeData = false; disableRealTimeDataOnStop = true";

        private Dictionary<string, SignalType> m_deviceSignalTypes;
        private Dictionary<string, SignalType> m_phasorSignalTypes;

        private void SaveDeviceConfiguration(ImportParameters importParams)
        {
            AdoDataConnection connection = importParams.Connection;
            TableOperations<SignalType> signalTypeTable = new TableOperations<SignalType>(connection);

            // Apply other connection string parameters that are specific to device operation
            importParams.ConnectionString = string.Format(ConnectionStringTemplate, importParams.ConnectionString);

            if (m_deviceSignalTypes is null)
                m_deviceSignalTypes = signalTypeTable.LoadSignalTypes("PMU").ToDictionary(key => key.Acronym, StringComparer.OrdinalIgnoreCase);

            if (m_phasorSignalTypes is null)
                m_phasorSignalTypes = signalTypeTable.LoadSignalTypes("Phasor").ToDictionary(key => key.Acronym, StringComparer.OrdinalIgnoreCase);

            SavePDCDeviceConnection(importParams);
        }

        private void SavePDCDeviceConnection(ImportParameters importParams)
        {
            string connectionString = importParams.ConnectionString;
            ConfigurationFrame configFrame = importParams.ConfigFrame;
            TableOperations<Device> deviceTable = importParams.DeviceTable;
            Guid nodeID = importParams.NodeID;

            // TODO: Consider best options for existing device lookup - is destination UDP port unique? (should be)
            Device device = /*importParams.Devices.FindDeviceByEndPoint(connectionString) ??*/ deviceTable.NewDevice();
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

            bool autoStartDataParsingSequence = true;
            bool skipDisableRealTimeData = false;

            // Handle connection string parameters that are fields in the device table
            if (settings.ContainsKey("autoStartDataParsingSequence"))
            {
                autoStartDataParsingSequence = bool.Parse(settings["autoStartDataParsingSequence"]);
                settings.Remove("autoStartDataParsingSequence");
                connectionString = settings.JoinKeyValuePairs();
            }

            if (settings.ContainsKey("skipDisableRealTimeData"))
            {
                skipDisableRealTimeData = bool.Parse(settings["skipDisableRealTimeData"]);
                settings.Remove("skipDisableRealTimeData");
                connectionString = settings.JoinKeyValuePairs();
            }

            ConfigurationCell deviceConfig = configFrame.Cells[0];

            string deviceAcronym = deviceConfig.IDLabel;
            string deviceName = null;

            if (string.IsNullOrWhiteSpace(deviceAcronym) && !string.IsNullOrWhiteSpace(deviceConfig.StationName))
                deviceAcronym = deviceConfig.StationName.GetCleanAcronym();
            else
                throw new InvalidOperationException("Unable to get station name or ID label from device configuration frame");

            if (!string.IsNullOrWhiteSpace(deviceConfig.StationName))
                deviceName = deviceConfig.StationName;

            device.NodeID = nodeID;
            device.Acronym = deviceAcronym;
            device.Name = deviceName ?? deviceAcronym;
            device.ProtocolID = importParams.IeeeC37_118ProtocolID;
            device.FramesPerSecond = configFrame.FrameRate;
            device.AccessID = configFrame.IDCode;
            device.IsConcentrator = true;
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

                // Save associated PMU records
                SavePMUDevices(importParams, newDevice);
            }
            else
            {
                // Update existing device record
                deviceTable.UpdateDevice(device);

                // Save associated PMU records
                SavePMUDevices(importParams, device);
            }
        }

        private void SavePMUDevices(ImportParameters importParams, Device parentDevice)
        {
            ConfigurationFrame configFrame = importParams.ConfigFrame;

            foreach (IConfigurationCell cell in configFrame.Cells)
            {
                if (cell is ConfigurationCell configCell)
                    SavePMUDevice(importParams, configCell, parentDevice);
            }
        }

        private void SavePMUDevice(ImportParameters importParams, ConfigurationCell configCell, Device parentDevice)
        {
            string connectionString = importParams.ConnectionString;
            ConfigurationFrame configFrame = importParams.ConfigFrame;
            TableOperations<Device> deviceTable = importParams.DeviceTable;
            Guid nodeID = importParams.NodeID;

            // TODO: Consider best options for existing device lookup - is destination UDP port unique? (should be)
            Device device = /*importParams.Devices.FindDeviceByEndPoint(connectionString) ??*/ deviceTable.NewDevice();
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

            bool autoStartDataParsingSequence = true;
            bool skipDisableRealTimeData = false;

            // Handle connection string parameters that are fields in the device table
            if (settings.ContainsKey("autoStartDataParsingSequence"))
            {
                autoStartDataParsingSequence = bool.Parse(settings["autoStartDataParsingSequence"]);
                settings.Remove("autoStartDataParsingSequence");
                connectionString = settings.JoinKeyValuePairs();
            }

            if (settings.ContainsKey("skipDisableRealTimeData"))
            {
                skipDisableRealTimeData = bool.Parse(settings["skipDisableRealTimeData"]);
                settings.Remove("skipDisableRealTimeData");
                connectionString = settings.JoinKeyValuePairs();
            }

            ConfigurationCell deviceConfig = configFrame.Cells[0];

            string deviceAcronym = deviceConfig.IDLabel;
            string deviceName = null;

            if (string.IsNullOrWhiteSpace(deviceAcronym) && !string.IsNullOrWhiteSpace(deviceConfig.StationName))
                deviceAcronym = deviceConfig.StationName.GetCleanAcronym();
            else
                throw new InvalidOperationException("Unable to get station name or ID label from device configuration frame");

            if (!string.IsNullOrWhiteSpace(deviceConfig.StationName))
                deviceName = deviceConfig.StationName;

            device.NodeID = nodeID;
            device.Acronym = deviceAcronym;
            device.Name = deviceName ?? deviceAcronym;
            device.ProtocolID = importParams.IeeeC37_118ProtocolID;
            device.FramesPerSecond = configFrame.FrameRate;
            device.AccessID = configFrame.IDCode;
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
                SaveDeviceRecords(importParams, newDevice);
            }
            else
            {
                // Update existing device record
                deviceTable.UpdateDevice(device);

                // Save associated device records
                SaveDeviceRecords(importParams, device);
            }
        }

        private void SaveDeviceRecords(ImportParameters importParams, Device device)
        {
            ConfigurationFrame configFrame = importParams.ConfigFrame;
            AdoDataConnection connection = importParams.Connection;
            TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(connection);
            ConfigurationCell cell = configFrame.Cells[0];

            // Add frequency
            SaveFixedMeasurement(importParams, m_deviceSignalTypes["FREQ"], device, measurementTable, cell.FrequencyDefinition.Label);

            // Add dF/dt
            SaveFixedMeasurement(importParams, m_deviceSignalTypes["DFDT"], device, measurementTable);

            // Add status flags
            SaveFixedMeasurement(importParams, m_deviceSignalTypes["FLAG"], device, measurementTable);

            // Add analogs
            SignalType analogSignalType = m_deviceSignalTypes["ALOG"];

            for (int i = 0; i < cell.AnalogDefinitions.Count; i++)
            {
                if (cell.AnalogDefinitions[i] is not AnalogDefinition analogDefinition)
                    continue;

                int index = i + 1;
                string signalReference = $"{device.Acronym}-{analogSignalType.Suffix}{index}";

                // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
                Measurement measurement = measurementTable.QueryMeasurement(signalReference);
                string pointTag = importParams.CreateIndexedPointTag(device.Acronym, analogSignalType.Acronym, index);
                measurement.DeviceID = device.ID;
                measurement.PointTag = pointTag;
                measurement.AlternateTag = analogDefinition.Label;
                measurement.Description = analogDefinition.Description ?? $"{device.Acronym} Analog Value {index} {analogDefinition.AnalogType}: {analogDefinition.Label}";
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
                if (cell.DigitalDefinitions[i] is not DigitalDefinition digitialDefinition)
                    continue;
                
                int index = i + 1;
                string signalReference = $"{device.Acronym}-{digitalSignalType.Suffix}{index}";

                // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
                Measurement measurement = measurementTable.QueryMeasurement(signalReference);
                string pointTag = importParams.CreateIndexedPointTag(device.Acronym, digitalSignalType.Acronym, index);
                measurement.DeviceID = device.ID;
                measurement.PointTag = pointTag;
                measurement.AlternateTag = digitialDefinition.Label;
                measurement.Description = digitialDefinition.Description ?? $"{device.Acronym} Digital Value {index}: {digitialDefinition.Label}";
                measurement.SignalReference = signalReference;
                measurement.SignalTypeID = digitalSignalType.ID;
                measurement.Internal = true;
                measurement.Enabled = true;

                measurementTable.AddNewOrUpdateMeasurement(measurement);
            }

            // Add phasors
            SaveDevicePhasors(importParams, cell, device, measurementTable);
        }

        private void SaveFixedMeasurement(ImportParameters importParams, SignalType signalType, Device device, TableOperations<Measurement> measurementTable, string label = null)
        {
            string signalReference = $"{device.Acronym}-{signalType.Suffix}";

            // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
            Measurement measurement = measurementTable.QueryMeasurement(signalReference);
            string pointTag = importParams.CreatePointTag(device.Acronym, signalType.Acronym);
            measurement.DeviceID = device.ID;
            measurement.PointTag = pointTag;
            measurement.Description = $"{device.Acronym} {signalType.Name}{(string.IsNullOrWhiteSpace(label) ? "" : $" - {label}")}";
            measurement.SignalReference = signalReference;
            measurement.SignalTypeID = signalType.ID;
            measurement.Internal = true;
            measurement.Enabled = true;

            measurementTable.AddNewOrUpdateMeasurement(measurement);
        }

        private void SaveDevicePhasors(ImportParameters importParams, ConfigurationCell cell, Device device, TableOperations<Measurement> measurementTable)
        {
            AdoDataConnection connection = importParams.Connection;
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

                foreach (IPhasorDefinition definition in cell.PhasorDefinitions)
                {
                    if (definition is not PhasorDefinition phasorDefinition)
                        continue;

                    bool isVoltage = phasorDefinition.PhasorType == PhasorType.Voltage;

                    Phasor phasor = phasorTable.NewPhasor();
                    phasor.DeviceID = device.ID;
                    phasor.Label = phasorDefinition.Label;
                    phasor.Type = isVoltage ? 'V' : 'I';
                    phasor.Phase = phasorDefinition.Phase;
                    phasor.BaseKV = 500;
                    phasor.DestinationPhasorID = null;
                    phasor.SourceIndex = phasorDefinition.Index;

                    phasorTable.AddNewPhasor(phasor);
                    SavePhasorMeasurement(importParams, isVoltage ? vphmSignalType : iphmSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable);
                    SavePhasorMeasurement(importParams, isVoltage ? vphaSignalType : iphaSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable);
                }
            }
            else
            {
                foreach (IPhasorDefinition definition in cell.PhasorDefinitions)
                {
                    if (definition is not PhasorDefinition phasorDefinition)
                        continue;

                    bool isVoltage = phasorDefinition.PhasorType == PhasorType.Voltage;

                    Phasor phasor = phasorTable.QueryPhasorForDevice(device.ID, phasorDefinition.Index);
                    phasor.DeviceID = device.ID;
                    phasor.Label = phasorDefinition.Label;
                    phasor.Type = isVoltage ? 'V' : 'I';

                    phasorTable.AddNewPhasor(phasor);
                    SavePhasorMeasurement(importParams, isVoltage ? vphmSignalType : iphmSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable);
                    SavePhasorMeasurement(importParams, isVoltage ? vphaSignalType : iphaSignalType, device, phasorDefinition, phasor.SourceIndex, measurementTable);
                }
            }
        }

        private void SavePhasorMeasurement(ImportParameters importParams, SignalType signalType, Device device, PhasorDefinition phasorDefinition, int index, TableOperations<Measurement> measurementTable)
        {
            string signalReference = $"{device.Acronym}-{signalType.Suffix}{index}";

            // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
            Measurement measurement = measurementTable.QueryMeasurement(signalReference);
            string pointTag = importParams.CreatePhasorPointTag(device.Acronym, signalType.Acronym, phasorDefinition.Label, phasorDefinition.Phase.ToString(), index, 500);

            measurement.DeviceID = device.ID;
            measurement.PointTag = pointTag;
            measurement.Description = phasorDefinition.Description ?? $"{device.Acronym} {phasorDefinition.Label} {signalType.Name}";
            measurement.PhasorSourceIndex = index;
            measurement.SignalReference = signalReference;
            measurement.SignalTypeID = signalType.ID;
            measurement.Internal = true;
            measurement.Enabled = true;

            measurementTable.AddNewOrUpdateMeasurement(measurement);
        }
    }
}