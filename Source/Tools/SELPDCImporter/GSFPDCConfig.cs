//******************************************************************************************************
//  GSFPDCConfig.cs - Gbtc
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
        public static void AddNewDevice(this TableOperations<Device> deviceTable, Device device) => 
            deviceTable.AddNewRecord(device);

        public static Device NewDevice(this TableOperations<Device> deviceTable) => 
            deviceTable.NewRecord();

        public static Device QueryDevice(this TableOperations<Device> deviceTable, string acronym) => 
            deviceTable.QueryRecordWhere("Acronym = {0}", acronym) ?? deviceTable.NewDevice();

        public static Device QueryDeviceByID(this TableOperations<Device> deviceTable, int deviceID) =>
            deviceTable.QueryRecordWhere("ID = {0}", deviceID) ?? deviceTable.NewDevice();

        public static Device QueryParentDeviceByIDCode(this TableOperations<Device> deviceTable, ushort idCode) =>
            deviceTable.QueryRecordWhere("ParentID IS NULL AND AccessID = {0}", idCode) ?? deviceTable.NewDevice();

        public static IEnumerable<Device> QueryChildDevices(this TableOperations<Device> deviceTable, int deviceID) =>
            deviceTable.QueryRecordsWhere("ParentID = {0}", deviceID);

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

        public static Device FindDeviceByIDCode(this Device[] devices, ushort idCode, int? parentID = null) =>
            devices.FirstOrDefault(device => device.ParentID == parentID && device.AccessID == idCode);

        // Remove any invalid characters from acronym
        public static string GetCleanAcronym(this string acronym) => 
            Regex.Replace(acronym.ToUpperInvariant().Replace(" ", "_"), @"[^A-Z0-9\-!_\.@#\$]", "", RegexOptions.IgnoreCase);
    }

    public static class GSFPDCConfig
    {
        // Connection string template
        private const string ConnectionStringTemplate = "{0}; autoStartDataParsingSequence = true; skipDisableRealTimeData = false; disableRealTimeDataOnStop = true";

        private static Dictionary<string, SignalType> m_deviceSignalTypes;
        private static Dictionary<string, SignalType> m_phasorSignalTypes;

        public static void SaveConnection(ImportParameters importParams)
        {
            AdoDataConnection connection = importParams.Connection;
            ConfigurationFrame configFrame = importParams.ConfigFrame;
            TableOperations<Device> deviceTable = importParams.DeviceTable;
            TableOperations<SignalType> signalTypeTable = new TableOperations<SignalType>(connection);
            Guid nodeID = importParams.NodeID;
            string connectionString = importParams.EditedConnectionString;

            // Apply other connection string parameters that are specific to device operation
            importParams.EditedConnectionString = string.Format(ConnectionStringTemplate, importParams.EditedConnectionString);

            if (m_deviceSignalTypes is null)
                m_deviceSignalTypes = signalTypeTable.LoadSignalTypes("PMU").ToDictionary(key => key.Acronym, StringComparer.OrdinalIgnoreCase);

            if (m_phasorSignalTypes is null)
                m_phasorSignalTypes = signalTypeTable.LoadSignalTypes("Phasor").ToDictionary(key => key.Acronym, StringComparer.OrdinalIgnoreCase);

            Device device = importParams.Devices?.FindDeviceByIDCode(configFrame.IDCode) ?? deviceTable.NewDevice();
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

            string deviceAcronym = configFrame.Acronym;
            string deviceName = null;

            if (string.IsNullOrWhiteSpace(deviceAcronym))
            {
                if (string.IsNullOrWhiteSpace(configFrame.Name))
                    throw new InvalidOperationException("Unable to get name or acronym for PDC from parsed configuration frame");
                
                deviceAcronym = configFrame.Name.GetCleanAcronym();
            }

            if (!string.IsNullOrWhiteSpace(configFrame.Name))
                deviceName = configFrame.Name;

            device.NodeID = nodeID;
            device.ParentID = null;
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

        private static void SavePMUDevices(ImportParameters importParams, Device parentDevice)
        {
            ConfigurationFrame configFrame = importParams.ConfigFrame;

            foreach (IConfigurationCell cell in configFrame.Cells)
            {
                if (cell is ConfigurationCell configCell)
                    SavePMUDevice(importParams, configCell, parentDevice);
            }
        }

        private static void SavePMUDevice(ImportParameters importParams, ConfigurationCell configCell, Device parentDevice)
        {
            TableOperations<Device> deviceTable = importParams.DeviceTable;
            Guid nodeID = importParams.NodeID;
            Device device = importParams.Devices?.FindDeviceByIDCode(configCell.IDCode, parentDevice.ID) ?? deviceTable.NewDevice();
            string deviceAcronym = configCell.IDLabel;
            string deviceName = null;

            if (string.IsNullOrWhiteSpace(deviceAcronym))
            {
                if (string.IsNullOrWhiteSpace(configCell.StationName))
                    throw new InvalidOperationException("Unable to get station name or ID label for PMU from parsed device configuration cell");
                
                deviceAcronym = configCell.StationName.GetCleanAcronym();
            }

            if (!string.IsNullOrWhiteSpace(configCell.StationName))
                deviceName = configCell.StationName;

            device.NodeID = nodeID;
            device.ParentID = parentDevice.ID;
            device.Acronym = deviceAcronym;
            device.Name = deviceName ?? deviceAcronym;
            device.ProtocolID = importParams.IeeeC37_118ProtocolID;
            device.FramesPerSecond = configCell.FrameRate;
            device.AccessID = configCell.IDCode;
            device.IsConcentrator = false;
            device.Enabled = true;

            // Check if this is a new device or an edit to an existing one
            if (device.ID == 0)
            {
                // Add new device record
                deviceTable.AddNewDevice(device);

                // Get newly added device with auto-incremented ID
                Device newDevice = deviceTable.QueryDevice(device.Acronym);

                // Save associated device records
                SaveDeviceRecords(importParams, configCell, newDevice);
            }
            else
            {
                // Update existing device record
                deviceTable.UpdateDevice(device);

                // Save associated device records
                SaveDeviceRecords(importParams, configCell, device);
            }
        }

        private static void SaveDeviceRecords(ImportParameters importParams, ConfigurationCell configCell, Device device)
        {
            AdoDataConnection connection = importParams.Connection;
            TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(connection);

            // Add frequency
            SaveFixedMeasurement(importParams, m_deviceSignalTypes["FREQ"], device, measurementTable);

            // Add dF/dt
            SaveFixedMeasurement(importParams, m_deviceSignalTypes["DFDT"], device, measurementTable);

            // Add status flags
            SaveFixedMeasurement(importParams, m_deviceSignalTypes["FLAG"], device, measurementTable);

            // Add analogs
            SignalType analogSignalType = m_deviceSignalTypes["ALOG"];

            for (int i = 0; i < configCell.AnalogDefinitions.Count; i++)
            {
                if (configCell.AnalogDefinitions[i] is not AnalogDefinition analogDefinition)
                    continue;

                int index = i + 1;
                string signalReference = $"{device.Acronym}-{analogSignalType.Suffix}{index}";

                // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
                Measurement measurement = measurementTable.QueryMeasurement(signalReference);
                string pointTag = importParams.CreateIndexedPointTag(device.Acronym, analogSignalType.Acronym, index);
                measurement.DeviceID = device.ID;
                measurement.PointTag = pointTag;
                measurement.AlternateTag = analogDefinition.Label;
                measurement.Description = $"{device.Acronym} Analog Value {index} {analogDefinition.AnalogType}: {analogDefinition.Label}{(string.IsNullOrWhiteSpace(analogDefinition.Description) ? "" : $" - {analogDefinition.Description}")}";
                measurement.SignalReference = signalReference;
                measurement.SignalTypeID = analogSignalType.ID;
                measurement.Internal = true;
                measurement.Enabled = true;

                measurementTable.AddNewOrUpdateMeasurement(measurement);
            }

            // Add digitals
            SignalType digitalSignalType = m_deviceSignalTypes["DIGI"];

            for (int i = 0; i < configCell.DigitalDefinitions.Count; i++)
            {
                if (configCell.DigitalDefinitions[i] is not DigitalDefinition digitalDefinition)
                    continue;
                
                int index = i + 1;
                string signalReference = $"{device.Acronym}-{digitalSignalType.Suffix}{index}";

                // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
                Measurement measurement = measurementTable.QueryMeasurement(signalReference);
                string pointTag = importParams.CreateIndexedPointTag(device.Acronym, digitalSignalType.Acronym, index);
                measurement.DeviceID = device.ID;
                measurement.PointTag = pointTag;
                measurement.AlternateTag = digitalDefinition.Label;
                measurement.Description = $"{device.Acronym} Digital Value {index}: {digitalDefinition.Label}{(string.IsNullOrWhiteSpace(digitalDefinition.Description) ? "" : $" - {digitalDefinition.Description}")}";
                measurement.SignalReference = signalReference;
                measurement.SignalTypeID = digitalSignalType.ID;
                measurement.Internal = true;
                measurement.Enabled = true;

                measurementTable.AddNewOrUpdateMeasurement(measurement);
            }

            // Add phasors
            SaveDevicePhasors(importParams, configCell, device, measurementTable);
        }

        private static void SaveFixedMeasurement(ImportParameters importParams, SignalType signalType, Device device, TableOperations<Measurement> measurementTable, string description = null)
        {
            string signalReference = $"{device.Acronym}-{signalType.Suffix}";

            // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
            Measurement measurement = measurementTable.QueryMeasurement(signalReference);
            string pointTag = importParams.CreatePointTag(device.Acronym, signalType.Acronym);
            measurement.DeviceID = device.ID;
            measurement.PointTag = pointTag;
            measurement.Description = $"{device.Acronym} {signalType.Name}{(string.IsNullOrWhiteSpace(description) ? "" : $" - {description}")}";
            measurement.SignalReference = signalReference;
            measurement.SignalTypeID = signalType.ID;
            measurement.Internal = true;
            measurement.Enabled = true;

            measurementTable.AddNewOrUpdateMeasurement(measurement);
        }

        private static void SaveDevicePhasors(ImportParameters importParams, ConfigurationCell configCell, Device device, TableOperations<Measurement> measurementTable)
        {
            AdoDataConnection connection = importParams.Connection;
            TableOperations<Phasor> phasorTable = new TableOperations<Phasor>(connection);

            // Get phasor signal types
            SignalType iphmSignalType = m_phasorSignalTypes["IPHM"];
            SignalType iphaSignalType = m_phasorSignalTypes["IPHA"];
            SignalType vphmSignalType = m_phasorSignalTypes["VPHM"];
            SignalType vphaSignalType = m_phasorSignalTypes["VPHA"];

            Phasor[] phasors = phasorTable.QueryPhasorsForDevice(device.ID).ToArray();

            bool dropAndAdd = phasors.Length != configCell.PhasorDefinitions.Count;

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
                if (configCell.PhasorDefinitions.Count > 0)
                    connection.DeletePhasorsForDevice(device.ID);

                foreach (IPhasorDefinition definition in configCell.PhasorDefinitions)
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
                foreach (IPhasorDefinition definition in configCell.PhasorDefinitions)
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

        private static void SavePhasorMeasurement(ImportParameters importParams, SignalType signalType, Device device, PhasorDefinition phasorDefinition, int index, TableOperations<Measurement> measurementTable)
        {
            string signalReference = $"{device.Acronym}-{signalType.Suffix}{index}";

            // Query existing measurement record for specified signal reference - function will create a new blank measurement record if one does not exist
            Measurement measurement = measurementTable.QueryMeasurement(signalReference);
            string pointTag = importParams.CreatePhasorPointTag(device.Acronym, signalType.Acronym, phasorDefinition.Label, phasorDefinition.Phase.ToString(), index, 500);
            char phase = phasorDefinition.Phase;

            string phaseDescription = phase switch
            {
                '+' => "Positive Sequence",
                '-' => "Negative Sequence",
                '0' => "Zero Sequence",
                 _  => $"{phase}-Phase"
            };

            measurement.DeviceID = device.ID;
            measurement.PointTag = pointTag;
            measurement.Description = $"{device.Acronym} {phasorDefinition.Label.Trim()} {signalType.Name} ({phaseDescription}){(string.IsNullOrWhiteSpace(phasorDefinition.Description) ? "" : $" - {phasorDefinition.Description}")}";
            measurement.PhasorSourceIndex = index;
            measurement.SignalReference = signalReference;
            measurement.SignalTypeID = signalType.ID;
            measurement.Internal = true;
            measurement.Enabled = true;

            measurementTable.AddNewOrUpdateMeasurement(measurement);
        }

        public static ConfigurationFrame ExtractConfigurationFrame(AdoDataConnection connection, ushort idCode)
        {
            TableOperations<Device> deviceTable = new TableOperations<Device>(connection);
            TableOperations<Phasor> phasorTable = new TableOperations<Phasor>(connection);
            TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(connection);
            Device pdc = deviceTable.QueryParentDeviceByIDCode(idCode);

            if (pdc.ID == 0)
                return null;

            ushort frameRate = (ushort)pdc.FramesPerSecond.GetValueOrDefault();

            ConfigurationFrame configFrame = new ConfigurationFrame(idCode, frameRate, pdc.Name, pdc.Acronym)
            {
                Settings = pdc.ConnectionString.ParseKeyValuePairs()
            };

            if (pdc.ParentID == null)
            {
                IEnumerable<Device> pmus = deviceTable.QueryChildDevices(pdc.ID);

                foreach (Device pmu in pmus)
                {
                    idCode = (ushort)pmu.AccessID;

                    // Create new configuration cell
                    ConfigurationCell configCell = new ConfigurationCell(configFrame, pmu.Name, idCode, pmu.Acronym)
                    {
                        ID = pmu.ID,
                        ParentID = pdc.ID,
                    };

                    configCell.FrequencyDefinition = new FrequencyDefinition(configCell) 
                    {
                        Label = $"{configCell.IDLabel} Frequency"
                    };

                    // Extract phasor definitions
                    foreach (Phasor phasor in phasorTable.QueryPhasorsForDevice(pmu.ID))
                    {
                        string description = measurementTable.QueryMeasurement(SignalReference.ToString(configCell.IDLabel, SignalKind.Angle, phasor.SourceIndex))?.Description ?? phasor.Label;
                        PhasorType phasorType = phasor.Type == 'I' ? PhasorType.Current : PhasorType.Voltage;

                        configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, phasor.Label, description, phasorType, phasor.Phase)
                        {
                            ID = phasor.ID,
                            SourceIndex = phasor.SourceIndex
                        });
                    }

                    // Add cell to frame
                    configFrame.Cells.Add(configCell);
                }

                if (configFrame.Cells.Count > 0)
                {
                    configFrame.IsConcentrator = true;
                }
                else
                {
                    // This is a directly connected device
                    configFrame.IsConcentrator = false;

                    ConfigurationCell configCell = new ConfigurationCell(configFrame, pdc.Name, configFrame.IDCode, pdc.Acronym)
                    {
                        ID = pdc.ID,
                        ParentID = null,
                    };

                    configCell.FrequencyDefinition = new FrequencyDefinition(configCell)
                    {
                        Label = $"{configCell.IDLabel} Frequency"
                    };

                    // Extract phasor definitions
                    foreach (Phasor phasor in phasorTable.QueryPhasorsForDevice(pdc.ID))
                    {
                        string description = measurementTable.QueryMeasurement(SignalReference.ToString(configCell.IDLabel, SignalKind.Angle, phasor.SourceIndex))?.Description ?? phasor.Label;
                        PhasorType phasorType = phasor.Type == 'I' ? PhasorType.Current : PhasorType.Voltage;

                        configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, phasor.Label, description, phasorType, phasor.Phase)
                        {
                            ID = phasor.ID,
                            SourceIndex = phasor.SourceIndex
                        });
                    }

                    // Add cell to frame
                    configFrame.Cells.Add(configCell);
                }
            }
            else
            {
                // Extracting a single PMU from a PDC configuration
                configFrame.IsConcentrator = true;

                // Create new configuration cell
                ConfigurationCell configCell = new ConfigurationCell(configFrame, pdc.Name, configFrame.IDCode, pdc.Acronym)
                {
                    ID = pdc.ID,
                    ParentID = null,
                };

                configCell.FrequencyDefinition = new FrequencyDefinition(configCell)
                {
                    Label = $"{configCell.IDLabel} Frequency"
                };

                // Extract phasor definitions
                foreach (Phasor phasor in phasorTable.QueryPhasorsForDevice(pdc.ID))
                {
                    string description = measurementTable.QueryMeasurement(SignalReference.ToString(configCell.IDLabel, SignalKind.Angle, phasor.SourceIndex))?.Description ?? phasor.Label;
                    PhasorType phasorType = phasor.Type == 'I' ? PhasorType.Current : PhasorType.Voltage;

                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, phasor.Label, description, phasorType, phasor.Phase)
                    {
                        ID = phasor.ID,
                        SourceIndex = phasor.SourceIndex
                    });
                }

                // Add cell to frame
                configFrame.Cells.Add(configCell);
            }

            return configFrame;
        }
    }
}