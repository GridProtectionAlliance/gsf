//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/28/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GSF;
using GSF.Console;
using GSF.Data;
using GSF.Data.Model;
using GSF.Identity;
using GSF.IO;
using LoadImpedanceCalcs.Model;
using Phasor = LoadImpedanceCalcs.Model.Phasor;

// ReSharper disable AssignNullToNotNullAttribute
namespace LoadImpedanceCalcs
{
    class Program
    {
        private const string DefaultSourceApp = "SIEGate";
        private const string DefaultSourceDevice = "IMPEDANCE";
        private const string DefaultUserID = "AUTOGEN";
        private const double DefaultLagTime = 3.0D;
        private const double DefaultLeadTime = 5.0D;
        private const int DefaultFramesPerSecond = 30;
        private const string AssemblyName = "PowerCalculations.dll";
        private const string TypeName = "PowerCalculations.ImpedanceCalculator";
        private const int RequiredArgumentCount = 1;

        private static Guid nodeID;
        private static string currentUserID;

        static int Main()
        {
            int row = 0;

            try
            {
                Arguments args = new Arguments(Environment.CommandLine, true);
                string sourceApp, sourceDevice, setting;
                double lagTime, leadTime;
                int framesPerSecond, successes = 0, failures = 0, simpleFormattedRecords = 0, explicitFormattedRecords = 0;
                bool skipFirstRow;

                if (args.Count != RequiredArgumentCount)
                    throw new ArgumentException($"Expected {RequiredArgumentCount} argument, received {args.Count}.");

                if (!args.TryGetValue("sourceApp", out sourceApp))
                    sourceApp = DefaultSourceApp;

                if (!args.TryGetValue("sourceDevice", out sourceDevice))
                    sourceDevice = DefaultSourceDevice;

                if (!args.TryGetValue("lagTime", out setting) || !double.TryParse(setting, out lagTime))
                    lagTime = DefaultLagTime;

                if (!args.TryGetValue("leadTime", out setting) || !double.TryParse(setting, out leadTime))
                    leadTime = DefaultLeadTime;

                if (!args.TryGetValue("framesPerSecond", out setting) || !int.TryParse(setting, out framesPerSecond))
                    framesPerSecond = DefaultFramesPerSecond;

                if (args.TryGetValue("skipFirstRow", out setting))
                    skipFirstRow = setting.ParseBoolean();
                else
                    skipFirstRow = true;

                string sourceFileName = FilePath.GetAbsolutePath(args["OrderedArg1"]);

                string configFile = FilePath.GetAbsolutePath($"{sourceApp}.exe.config");

                if (!File.Exists(configFile))
                    throw new FileNotFoundException($"Config file for {sourceApp} application \"{configFile}\" was not found.");

                XDocument serviceConfig = XDocument.Load(configFile);

                nodeID = Guid.Parse(serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "NodeID".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault());

                string connectionString = serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "ConnectionString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault();

                string dataProviderString = serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "DataProviderString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault();

                using (AdoDataConnection connection = new AdoDataConnection(connectionString, dataProviderString))
                using (StreamReader reader = File.OpenText(sourceFileName))
                {
                    currentUserID = UserInfo.CurrentUserID ?? DefaultUserID;

                    TableOperations<Device> deviceTable = new TableOperations<Device>(connection);
                    TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(connection);
                    TableOperations<Phasor> phasorTable = new TableOperations<Phasor>(connection);
                    TableOperations<CustomActionAdapter> actionAdapterTable = new TableOperations<CustomActionAdapter>(connection);

                    string line;

                    while ((object)(line = reader.ReadLine()) != null)
                    {
                        row++;

                        if (skipFirstRow)
                        {
                            skipFirstRow = false;
                            continue;
                        }

                        // Simple mode: -- simple mode only works when devices on both end of the line are measuring a single voltage and current
                        // 0        1              2
                        // TieLine, SendingDevice, ReceivingDevice

                        // Explicit mode:
                        // 0        1                  2                    3         4             5         6             7         8             9         10
                        // TieLine, SendingSubstation, ReceivingSubstation, SV1Angle, SV1Magnitude, SI1Angle, SI1Magnitude, RV1Angle, RV1Magnitude, RI1Angle, RI1Magnitude
                        string[] columns = line.Split(',');

                        if (columns.Length < 3)
                        {
                            Console.WriteLine($"Not enough columns in CSV file at row {row} - expected 3 for simple mode, encountered {columns.Length}, skipped row.");
                            continue;
                        }

                        if (columns.Length > 3 && columns.Length < 11)
                        {
                            Console.WriteLine($"Not enough columns in CSV file at row {row} - expected 11 for explicit mode, encountered {columns.Length}, skipped row.");
                            continue;
                        }

                        string tieLineID = columns[0].ToUpperInvariant().Trim();
                        string sendDevice = columns[1].ToUpperInvariant().Trim();
                        string receiveDevice = columns[2].ToUpperInvariant().Trim();
                        string sv1Angle = null, sv1Magnitude = null, si1Angle = null, si1Magnitude = null, rv1Angle = null, rv1Magnitude = null, ri1Angle = null, ri1Magnitude = null;
                        string[] measurements;
                        bool simpleMode = columns.Length == 3;
                        List<string> inputMeasurements = new List<string>();
                        Dictionary<string, string> adapterConnectionString = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        if (simpleMode)
                        {
                            simpleFormattedRecords++;
                        }
                        else
                        {
                            explicitFormattedRecords++;
                            sv1Angle = columns[3].ToUpperInvariant().Trim();
                            sv1Magnitude = columns[4].ToUpperInvariant().Trim();
                            si1Angle = columns[5].ToUpperInvariant().Trim();
                            si1Magnitude = columns[6].ToUpperInvariant().Trim();
                            rv1Angle = columns[7].ToUpperInvariant().Trim();
                            rv1Magnitude = columns[8].ToUpperInvariant().Trim();
                            ri1Angle = columns[9].ToUpperInvariant().Trim();
                            ri1Magnitude = columns[10].ToUpperInvariant().Trim();
                        }

                        // Add initial connection string settings
                        adapterConnectionString["FramesPerSecond"] = framesPerSecond.ToString();
                        adapterConnectionString["LagTime"] = lagTime.ToString(CultureInfo.InvariantCulture);
                        adapterConnectionString["LeadTime"] = leadTime.ToString(CultureInfo.InvariantCulture);

                        // Get or add virtual device to associate new output measurements with
                        int protocolID = connection.ExecuteScalar<int?>("SELECT ID FROM Protocol WHERE Acronym='VirtualInput'") ?? 11;
                        Device device = GetOrAddDevice(deviceTable, sourceDevice, protocolID);                    

                        if (simpleMode)
                        {
                            // Get input measurement IDs for sending device
                            measurements = GetInputMeasurements(deviceTable, measurementTable, phasorTable, sendDevice);

                            if ((object)measurements == null)
                            {
                                failures++;
                                continue;
                            }

                            inputMeasurements.AddRange(measurements);

                            // Get input measurement IDs for receiving device
                            measurements = GetInputMeasurements(deviceTable, measurementTable, phasorTable, receiveDevice);

                            if ((object)measurements == null)
                            {
                                failures++;
                                continue;
                            }

                            inputMeasurements.AddRange(measurements);
                        }
                        else
                        {
                            if (GetInputMeasurement(measurementTable, sv1Angle, inputMeasurements) ||
                                GetInputMeasurement(measurementTable, sv1Magnitude, inputMeasurements) ||
                                GetInputMeasurement(measurementTable, si1Angle, inputMeasurements) ||
                                GetInputMeasurement(measurementTable, si1Magnitude, inputMeasurements) ||
                                GetInputMeasurement(measurementTable, rv1Angle, inputMeasurements) ||
                                GetInputMeasurement(measurementTable, rv1Magnitude, inputMeasurements) ||
                                GetInputMeasurement(measurementTable, ri1Angle, inputMeasurements) ||
                                GetInputMeasurement(measurementTable, ri1Magnitude, inputMeasurements))
                            {
                                failures++;
                                continue;
                            }
                        }

                        // Define input measurement keys connection string parameter
                        adapterConnectionString["InputMeasurementKeys"] = string.Join("; ", inputMeasurements);

                        bool newAdd;
                        CustomActionAdapter actionAdapter = GetOrAddActionAdapter(actionAdapterTable, tieLineID, out newAdd);
                        Console.WriteLine($"{(newAdd ? "Adding" : "Augmenting")} impedance calculation for \"{tieLineID}\"...");

                        // Get output measurement IDs for calculator adapter, creating them if needed
                        OrderedDictionary outputTypes = new OrderedDictionary(StringComparer.OrdinalIgnoreCase)
                        {
                            ["RESISTANCE"] = "Resistance",
                            ["REACTANCE"] = "Reactance",
                            ["CONDUCTANCE"] = "Conductance",
                            ["SUSCEPTANCE"] = "Susceptance",
                            ["LINEIMPEDANCE"] = "Line Impedance",
                            ["LINEIMPEDANCEANGLE"] = "Line Impedance Angle",
                            ["LINEADMITTANCE"] = "Line Admittance",
                            ["LINEADMITTANCEANGLE"] = "Line Admittance Angle"
                        };

                        measurements = GetOutputMeasurements(measurementTable, device.ID, tieLineID, sendDevice, receiveDevice, outputTypes);

                        // Define input measurement keys connection string parameter
                        adapterConnectionString["OutputMeasurements"] = string.Join("; ", measurements);

                        // Save updates to action adapter
                        actionAdapter.Enabled = true;
                        actionAdapter.ConnectionString = adapterConnectionString.JoinKeyValuePairs();
                        actionAdapterTable.UpdateRecord(actionAdapter);

                        successes++;
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"Found {simpleFormattedRecords:N0} simple formatted records.");
                Console.WriteLine($"Found {explicitFormattedRecords:N0} explicit formatted records.");
                Console.WriteLine();
                Console.WriteLine($"{successes:N0} successful imports.");
                Console.WriteLine($"{failures:N0} failed imports.");
#if DEBUG
                Console.ReadKey();
#endif
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Import halted at row {row}!");
                Console.Error.WriteLine($"Load Exception: {ex.Message}");
                return 1;
            }
        }

        private static Device GetOrAddDevice(TableOperations<Device> deviceTable, string acronym, int protocolID)
        {
            Device device = deviceTable.QueryRecord("NodeID = {0} AND Acronym = {1}", nodeID, acronym);

            if ((object)device == null)
            {
                device = deviceTable.NewRecord();

                device.NodeID = nodeID;
                device.UniqueID = Guid.NewGuid();
                device.Acronym = acronym;
                device.Name = acronym;
                device.ProtocolID = protocolID;
                device.Enabled = true;
                device.CreatedOn = DateTime.UtcNow;
                device.CreatedBy = currentUserID;
                device.UpdatedOn = DateTime.UtcNow;
                device.UpdatedBy = currentUserID;

                deviceTable.AddNewRecord(device);
                device = deviceTable.QueryRecord("NodeID = {0} AND Acronym = {1}", nodeID, acronym);

                if ((object)device == null)
                    throw new InvalidOperationException($"Failed to lookup Device record with Acronym of \"{acronym}\".");
            }

            return device;
        }

        private static Device GetDevice(TableOperations<Device> deviceTable, string acronym)
        {
            Device device = deviceTable.QueryRecord("NodeID = {0} AND Acronym = {1}", nodeID, acronym);

            if ((object)device == null)
                Console.WriteLine($"Failed to lookup Device record with Acronym of \"{acronym}\"");

            return device;
        }

        private static Phasor[] GetPostiveSequencePhasors(TableOperations<Phasor> phasorTable, int deviceID)
        {
            List<Phasor> phasors = phasorTable.QueryRecords("SourceIndex", new RecordRestriction("Phase = {0} AND DeviceID = {1}", "+", deviceID)).ToList();
            return new[] { phasors.FirstOrDefault(phasor => phasor.Type == "V"), phasors.FirstOrDefault(phasor => phasor.Type == "I") };
        }

        private static Measurement GetPhasorMeasurement(TableOperations<Measurement> measurementTable, string deviceAcronym, char phasorElement, int phasorIndex)
        {
            string signalReference = $"{deviceAcronym}-P{phasorElement}{phasorIndex}";
            Measurement measurement = measurementTable.QueryRecord("SignalReference = {0}", signalReference);

            if ((object)measurement == null)
                Console.WriteLine($"Failed to lookup Measurement record with SignalReference of \"{signalReference}\"");

            return measurement;
        }

        private static bool GetInputMeasurement(TableOperations<Measurement> measurementTable, string pointTag, List<string> inputMeasurements)
        {
            Measurement measurement = measurementTable.QueryRecord("PointTag = {0}", pointTag);

            if ((object)measurement == null)
            {
                Console.WriteLine($"Failed to lookup Measurement record with PointTag of \"{pointTag}\"");
                return true;
            }

            inputMeasurements.Add(measurement.SignalID.ToString());
            return false;
        }

        private static string[] GetInputMeasurements(TableOperations<Device> deviceTable, TableOperations<Measurement> measurementTable, TableOperations<Phasor> phasorTable, string deviceAcronym)
        {
            Device device = GetDevice(deviceTable, deviceAcronym);

            if ((object)device == null)
                return null;

            Phasor[] phasors = GetPostiveSequencePhasors(phasorTable, device.ID);

            if (phasors.Length != 2 || phasors.Any(phasor => (object)phasor == null))
            {
                Console.WriteLine($"Failed to find a positive sequence voltage and current phasor for device \"{device.Acronym}\"");
                return null;
            }

            List<Measurement> measurements = new List<Measurement>();
            Measurement measurement;

            for (int i = 0; i < phasors.Length; i++)
            {
                // Get angle measurement
                measurement = GetPhasorMeasurement(measurementTable, deviceAcronym, 'A', phasors[i].SourceIndex);

                if ((object)measurement == null)
                    return null;

                measurements.Add(measurement);

                // Get magnitude measurement
                measurement = GetPhasorMeasurement(measurementTable, deviceAcronym, 'M', phasors[i].SourceIndex);

                if ((object)measurement == null)
                    return null;

                measurements.Add(measurement);
            }

            return measurements.Select(m => m.SignalID.ToString()).ToArray();
        }

        private static CustomActionAdapter GetOrAddActionAdapter(TableOperations<CustomActionAdapter> actionAdapterTable, string tielineID, out bool newAdd)
        {
            string adapterName = $"IMPEDANCE_{tielineID}_CALC";
            CustomActionAdapter actionAdapter = actionAdapterTable.QueryRecord("NodeID = {0} AND AdapterName = {1}", nodeID, adapterName);

            if ((object)actionAdapter == null)
            {
                actionAdapter = actionAdapterTable.NewRecord();

                actionAdapter.NodeID = nodeID;
                actionAdapter.AdapterName = adapterName;
                actionAdapter.AssemblyName = AssemblyName;
                actionAdapter.TypeName = TypeName;
                actionAdapter.CreatedOn = DateTime.UtcNow;
                actionAdapter.CreatedBy = currentUserID;
                actionAdapter.UpdatedOn = DateTime.UtcNow;
                actionAdapter.UpdatedBy = currentUserID;

                actionAdapterTable.AddNewRecord(actionAdapter);

                // Re-query newly added record to get auto-increment ID
                actionAdapter = actionAdapterTable.QueryRecord("NodeID = {0} AND AdapterName = {1}", nodeID, adapterName);
                newAdd = true;

                if ((object)actionAdapter == null)
                    throw new InvalidOperationException($"Failed to lookup CustomActionAdapter record with AdapterName of \"{adapterName}\".");
            }
            else
            {
                actionAdapter.UpdatedOn = DateTime.UtcNow;
                actionAdapter.UpdatedBy = currentUserID;
                newAdd = false;
            }

            return actionAdapter;
        }

        private static string[] GetOutputMeasurements(TableOperations<Measurement> measurementTable, int deviceID, string tieLineID, string sender, string receiver, OrderedDictionary outputTypes)
        {
            List<Measurement> measurements = new List<Measurement>();

            foreach (string outputType in outputTypes.Keys)
            {
                string pointTag = $"IMPEDANCE_{tieLineID}-{outputType}:CV";
                string description = $"{tieLineID} [{sender} => {receiver}] Calculated {outputTypes[outputType]} Value";

                Measurement measurement = measurementTable.QueryRecord("PointTag = {0}", pointTag);

                if ((object)measurement == null)
                {
                    measurement = measurementTable.NewRecord();

                    measurement.DeviceID = deviceID;
                    measurement.PointTag = pointTag;
                    measurement.SignalReference = pointTag;
                    measurement.Description = description;
                    measurement.Enabled = true;

                    measurementTable.AddNewRecord(measurement);
                    measurement = measurementTable.QueryRecord("PointTag = {0}", pointTag);

                    if ((object)measurement == null)
                        throw new InvalidOperationException($"Failed to lookup Measurement record with PointTag of \"{pointTag}\".");
                }
                else
                {
                    measurement.Description = description;
                    measurement.UpdatedOn = DateTime.UtcNow;
                    measurement.UpdatedBy = currentUserID;

                    measurementTable.UpdateRecord(measurement);
                }

                measurements.Add(measurement);
            }

            return measurements.Select(m => m.SignalID.ToString()).ToArray();
        }
    }
}
