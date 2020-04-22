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
//  04/20/2020 Dan Brancaccio
//      Changed RequiredArgumentCount check to < required for error from !=
//      Changed the required location of the exe to a subfolder of the application
//      Added IfDebug for location of exe.config file
//      Sample CSV
//      Owner PMU Name,	WISP PMU Name, Owner Signal Name, WISP Signal Name, 16-bit WISP ID, Owner Description
//
//******************************************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CreateOutputStream.Model;
using GSF;
using GSF.Console;
using GSF.Data;
using GSF.Data.Model;
using GSF.Identity;
using GSF.IO;

// ReSharper disable AssignNullToNotNullAttribute
namespace CreateOutputStream
{
    class Program
    {
        private const string DefaultSourceApp = "SIEGate";
        private const string DefaultOutputName = "IMPORTEDSTREAM";
        private const string DefaultUserID = "AUTOGEN";
        private const int RequiredArgumentCount = 1;

        private static Guid nodeID;
        private static string currentUserID;

        static int Main()
        {
            int row = 0;

            try
            {
                // Handle command line arguments
                Arguments args = new Arguments(Environment.CommandLine, true);
                bool skipFirstRow;
                int successes = 0, failures = 0;

                // First ordered argument is source CSV file name, it is required
                if (args.Count < RequiredArgumentCount)
                    throw new ArgumentException($"Expected {RequiredArgumentCount:N0} argument, received {args.Count:N0}.");

                // Check for switch based arguments
                if (!args.TryGetValue("sourceApp", out string sourceApp))   // Source GPA application, defaults to "SIEGate"
                    sourceApp = DefaultSourceApp;

                if (!args.TryGetValue("outputName", out string outputName)) // Target IEEE C37.118 output stream name, defaults to "IMPORTEDSTREAM"
                    outputName = DefaultOutputName;

                if (args.TryGetValue("skipFirstRow", out string setting))   // Setting to skip first row of import file, default to true
                    skipFirstRow = setting.ParseBoolean();
                else
                    skipFirstRow = true;

                // Make sure output name is upper case, this is an acronym for the output adapter in the target system
                outputName = outputName.ToUpperInvariant();

                // Make provided files name are relative to run path if not other path was provided
                string sourceFileName = FilePath.GetAbsolutePath(args["OrderedArg1"]);
                string configFile = FilePath.GetAbsolutePath($"..\\{sourceApp}.exe.config");
                
#if DEBUG
                configFile = "C:\\Program Files\\openPDC\\openPDC.exe.config";
#endif

                // Fail if source config file does not exist
                if (!File.Exists(configFile))
                    throw new FileNotFoundException($"Config file for {sourceApp} application \"{configFile}\" was not found.");

                // Load needed database settings from target config file
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

                // Open database schema and input CSV file
                using (AdoDataConnection connection = new AdoDataConnection(connectionString, dataProviderString))
                using (StreamReader reader = File.OpenText(sourceFileName))                
                {
                    // Configuration database tracks user changes for CIP reasons, use current user for ID
                    currentUserID = UserInfo.CurrentUserID ?? DefaultUserID;

                    // Setup database table operations for OutputStream table - this allows model, i.e., class instance representing record, based CRUD operations
                    TableOperations<OutputStream> outputStreamTable = new TableOperations<OutputStream>(connection);
                    
                    // See if target output stream already exists
                    OutputStream outputStream = outputStreamTable.QueryRecordWhere("NodeID = {0} AND Acronym = {1}", nodeID, outputName);

                    if (outputStream == null)
                    {
                        // Setup a new output stream using default settings (user can adjust later as needed)
                        outputStream = new OutputStream
                        {
                            NodeID = nodeID,
                            Acronym = outputName,
                            Name = outputName,
                            ConnectionString = "RoundToNearestTimestamp=True; addPhaseLabelSuffix=false;",
                            DataChannel = "port=-1; clients=localhost:4712; interface=0.0.0.0",
                            AutoPublishConfigFrame = true,
                            AutoStartDataChannel = true,
                            NominalFrequency = 60,
                            FramesPerSecond = 30,
                            LagTime = 5.0D,
                            LeadTime = 5.0D,
                            AllowSortsByArrival = true,
                            TimeResolution = 330000,
                            AllowPreemptivePublishing = true,
                            PerformTimeReasonabilityCheck = true,
                            DownsamplingMethod = "LastReceived",
                            DataFormat = "FloatingPoint",
                            CoordinateFormat = "Polar",
                            CurrentScalingValue = 2423,
                            VoltageScalingValue = 2725785,
                            AnalogScalingValue = 1373291,
                            DigitalMaskValue = -65536,
                            Enabled = true,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = currentUserID,
                            UpdatedOn = DateTime.UtcNow,
                            UpdatedBy = currentUserID
                        };

                        outputStreamTable.AddNewRecord(outputStream);
                        outputStream = outputStreamTable.QueryRecordWhere("NodeID = {0} AND Acronym = {1}", nodeID, outputName);

                        if (outputStream == null)
                            throw new InvalidOperationException($"Failed to lookup OutputStream record with Acronym of \"{outputName}\".");
                    }
                    else
                    {
                        // If record already exists, just track updates by user with timestamp
                        outputStream.UpdatedOn = DateTime.UtcNow;
                        outputStream.UpdatedBy = currentUserID;
                        outputStreamTable.UpdateRecord(outputStream);
                    }

                    // Setup database table operations for other needed tables
                    TableOperations<Device> deviceTable = new TableOperations<Device>(connection);
                    TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(connection);
                    TableOperations<Phasor> phasorTable = new TableOperations<Phasor>(connection);
                    TableOperations<OutputStreamDevice> outputStreamDeviceTable = new TableOperations<OutputStreamDevice>(connection);
                    TableOperations<OutputStreamMeasurement> outputStreamMeasurementTable = new TableOperations<OutputStreamMeasurement>(connection);
                    TableOperations<OutputStreamDevicePhasor> outputStreamDevicePhasorTable = new TableOperations<OutputStreamDevicePhasor>(connection);

                    Device device = null;
                    OutputStreamDevice outputStreamDevice = null;

                    string line, lastDeviceName = null;
                    int deviceIndex = 0, phasorIndex = 0;

                    // Loop through each line in CSV input file
                    while ((line = reader.ReadLine()) != null)
                    {
                        row++;

                        if (skipFirstRow)
                        {
                            skipFirstRow = false;
                            continue;
                        }

                        string[] columns = line.Split(',');

                        if (columns.Length < 6)
                        {
                            Console.WriteLine($"Not enough columns in CSV file at row {row} - expected 6, encountered {columns.Length}, skipped row.");
                            continue;
                        }

                        // Read columns of data from current row
                        string sourceDeviceName = columns[0].ToUpperInvariant().Trim();
                        string destDeviceName = columns[1].ToUpperInvariant().Trim();
                        string sourcePhasorName = columns[2].ToUpperInvariant().Trim();
                        string destPhasorName = columns[3].ToUpperInvariant().Trim();
                        ushort idCode = ushort.Parse(columns[4].Trim());
                        string description = columns[5].Trim();

                        if (!sourceDeviceName.Equals(lastDeviceName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            lastDeviceName = sourceDeviceName;
                            deviceIndex++;
                            phasorIndex = 0;

                            // Lookup existing source device
                            device = deviceTable.QueryRecordWhere("Acronym = {0}", sourceDeviceName);

                            if (device == null)
                            {
                                Console.WriteLine($"Failed to find source device \"{sourceDeviceName}\" - cannot create new output stream device for \"{destDeviceName}\".");
                                failures++;
                                continue;
                            }

                            Console.WriteLine($"Mapping source device \"{sourceDeviceName}\" to output stream device \"{destDeviceName}\"...");

                            // Setup a new output stream device
                            outputStreamDevice = outputStreamDeviceTable.QueryRecordWhere("NodeID = {0} AND AdapterID = {1} AND Acronym = {2}", nodeID, outputStream.ID, destDeviceName);

                            if (outputStreamDevice == null)
                            {
                                outputStreamDevice = new OutputStreamDevice
                                {
                                    NodeID = nodeID,
                                    AdapterID = outputStream.ID,
                                    IDCode = idCode > 0 ? idCode : deviceIndex,
                                    Acronym = destDeviceName,
                                    Name = destDeviceName.ToTitleCase(),
                                    Enabled = true,
                                    CreatedOn = DateTime.UtcNow,
                                    CreatedBy = currentUserID,
                                    UpdatedOn = DateTime.UtcNow,
                                    UpdatedBy = currentUserID
                                };

                                outputStreamDeviceTable.AddNewRecord(outputStreamDevice);
                                outputStreamDevice = outputStreamDeviceTable.QueryRecordWhere("NodeID = {0} AND AdapterID = {1} AND Acronym = {2}", nodeID, outputStream.ID, destDeviceName);

                                if (outputStreamDevice == null)
                                    throw new InvalidOperationException($"Failed to lookup OutputStreamDevice record with Acronym of \"{destDeviceName}\".");
                            }
                            else
                            {
                                // TODO: Could augment existing record, current logic just skips existing to account for possible input file errors
                                outputStreamDevice.IDCode = idCode > 0 ? idCode : deviceIndex;
                                outputStreamDevice.UpdatedOn = DateTime.UtcNow;
                                outputStreamDevice.UpdatedBy = currentUserID;
                                outputStreamDeviceTable.UpdateRecord(outputStreamDevice);
                            }

                            // Validate base output stream measurements exist
                            foreach (string signalType in new[] { "SF", "FQ", "DF" })    // Status flags, frequency and dF/dT (delta frequency over delta time, i.e., rate of change of frequency)
                                AddOutputStreamMeasurement(measurementTable, outputStreamMeasurementTable, outputStream.ID, $"{device.Acronym}-{signalType}", $"{destDeviceName}-{signalType}");
                        }

                        if (device == null)
                        {
                            failures++;
                            continue;
                        }

                        //                  123456789012345678901234567890
                        Console.WriteLine($"    Adding phasors for \"{$"{destPhasorName} - {description}".TrimWithEllipsisEnd(70)}\"");

                        // Lookup existing device phasor record
                        Phasor phasor = phasorTable.QueryRecordWhere("DeviceID = {0} AND Label = {1}", device.ID, sourcePhasorName);
                        phasorIndex++;

                        if (phasor == null)
                        {
                            Console.WriteLine($"Failed to lookup Phasor record with Label of \"{sourcePhasorName}\"");
                            failures++;
                        }
                        else
                        {
                            // Setup a new output stream device phasor
                            OutputStreamDevicePhasor outputStreamDevicePhasor = outputStreamDevicePhasorTable.QueryRecordWhere("NodeID = {0} AND OutputStreamDeviceID = {1} AND Label = {2}", nodeID, outputStreamDevice.ID, destPhasorName);

                            if (outputStreamDevicePhasor == null)
                            {
                                outputStreamDevicePhasor = new OutputStreamDevicePhasor
                                {
                                    NodeID = nodeID,
                                    OutputStreamDeviceID = outputStreamDevice.ID,
                                    Label = destPhasorName,
                                    Type = phasor.Type,
                                    Phase = phasor.Phase,
                                    LoadOrder = phasorIndex,
                                    CreatedOn = DateTime.UtcNow,
                                    CreatedBy = currentUserID,
                                    UpdatedOn = DateTime.UtcNow,
                                    UpdatedBy = currentUserID
                                };

                                outputStreamDevicePhasorTable.AddNewRecord(outputStreamDevicePhasor);
                                outputStreamDevicePhasor = outputStreamDevicePhasorTable.QueryRecordWhere("NodeID = {0} AND OutputStreamDeviceID = {1} AND Label = {2}", nodeID, outputStreamDevice.ID, destPhasorName);

                                if (outputStreamDevicePhasor == null)
                                    throw new InvalidOperationException($"Failed to lookup OutputStreamDevicePhasor record with Label of \"{destPhasorName}\".");
                            }
                            else
                            {
                                // TODO: Could augment existing record, current logic just skips existing to account for possible input file errors
                                outputStreamDevicePhasor.UpdatedOn = DateTime.UtcNow;
                                outputStreamDevicePhasor.UpdatedBy = currentUserID;
                                outputStreamDevicePhasorTable.UpdateRecord(outputStreamDevicePhasor);
                            }

                            // Define output stream phasor measurements
                            AddOutputStreamMeasurement(measurementTable, outputStreamMeasurementTable, outputStream.ID, $"{device.Acronym}-PA{phasor.SourceIndex}", $"{destDeviceName}-PA{phasorIndex}");
                            AddOutputStreamMeasurement(measurementTable, outputStreamMeasurementTable, outputStream.ID, $"{device.Acronym}-PM{phasor.SourceIndex}", $"{destDeviceName}-PM{phasorIndex}");

                            successes++;
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"{successes:N0} successful phasor imports.");
                Console.WriteLine($"{failures:N0} failed phasor imports.");

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

        private static void AddOutputStreamMeasurement(TableOperations<Measurement> measurementTable, TableOperations<OutputStreamMeasurement> outputStreamMeasurementTable, int outputStreamID, string sourceSignalReference, string destSignalReference)
        {
            Measurement measurement = measurementTable.QueryRecordWhere("SignalReference = {0}", $"{sourceSignalReference}");

            if (measurement == null)
            {
                Console.WriteLine($"Failed to lookup Measurement record with SignalReference of \"{sourceSignalReference}\"");                
            }
            else
            {
                OutputStreamMeasurement outputStreamMeasurement = outputStreamMeasurementTable.QueryRecordWhere("AdapterID = {0} AND SignalReference = {1}", outputStreamID, destSignalReference);

                if (outputStreamMeasurement == null)
                {
                    outputStreamMeasurement = new OutputStreamMeasurement
                    {
                        NodeID = nodeID,
                        AdapterID = outputStreamID,
                        HistorianID = measurement.HistorianID,
                        PointID = measurement.PointID,
                        SignalReference = destSignalReference,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = currentUserID,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = currentUserID
                    };

                    outputStreamMeasurementTable.AddNewRecord(outputStreamMeasurement);
                }
            }
        }
    }
}
