//******************************************************************************************************
//  ModbusPoller.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  07/26/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Reflection;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Statistics;
using Modbus.Device;
using Modbus.Utility;
using ModbusAdapters.Model;
using Measurement = GSF.TimeSeries.Measurement;
using Timer = System.Timers.Timer;

#pragma warning disable SCS0006 // Weak hash
#pragma warning disable SCS0018 // Path traversal

namespace ModbusAdapters
{
    [Description("Modbus: Implements Modbus polling capabilities")]
    [EditorBrowsable(EditorBrowsableState.Advanced)] // Normally defined as an input device protocol
    public class ModbusPoller : InputAdapterBase
    {
        #region [ Members ]

        // Nested Types

        private enum DerivedType
        {
            String,
            Single,
            Double,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64
        }

        private enum RecordType
        {
            DI, // Discrete Input
            CO, // Coil
            IR, // Input Register
            HR  // Holding Register
        }

        private enum SequenceType
        {
            Read,
            Write
        }

        private class AddressRecord
        {
            public readonly RecordType Type;
            public readonly ushort Address;

            public AddressRecord(string recordType, ushort address)
            {
                Enum.TryParse(recordType, true, out Type);
                Address = address;
            }
        }

        private class DerivedValue
        {
            public readonly DerivedType Type;
            public readonly List<AddressRecord> AddressRecords;

            public DerivedValue(DerivedType type)
            {
                Type = type;
                AddressRecords = new List<AddressRecord>();
            }

            public DerivedValue(string derivedType)
            {
                Enum.TryParse(derivedType, true, out Type);
                AddressRecords = new List<AddressRecord>();
            }

            public ushort[] GetDataValues(Group[] groups)
            {
                ushort[] dataValues = new ushort[AddressRecords.Count];

                for (int i = 0; i < dataValues.Length; i++)
                {
                    ushort address = AddressRecords[i].Address;

                    foreach (Group group in groups)
                    {
                        if (group.HasDataValue(address))
                        {
                            dataValues[i] = group.DataValues[address - group.StartAddress];
                            break;
                        }
                    }
                }

                return dataValues;
            }
        }

        private class Group
        {
            public RecordType Type;
            public ushort StartAddress;
            public ushort PointCount;
            public ushort[] DataValues;

            public bool HasDataValue(ushort address) => address >= StartAddress && address <= StartAddress + PointCount - 1;
        }

        private class Sequence
        {
            public readonly SequenceType Type;
            public readonly List<Group> Groups = new List<Group>();

            public Sequence(SequenceType type)
            {
                Type = type;
            }
        }

        // Define a IDevice implementation for to provide daily reports
        private class DeviceProxy : IDevice
        {
            private readonly ModbusPoller m_parent;

            public DeviceProxy(ModbusPoller parent)
            {
                m_parent = parent;
            }

            // Gets or sets total data quality errors of this <see cref="IDevice"/>.
            public long DataQualityErrors
            {
                get;
                set;
            }

            // Gets or sets total time quality errors of this <see cref="IDevice"/>.
            public long TimeQualityErrors
            {
                get;
                set;
            }

            // Gets or sets total device errors of this <see cref="IDevice"/>.
            public long DeviceErrors
            {
                get
                {
                    return m_parent.m_deviceErrors;
                }
                set
                {
                    // Ignoring updates
                }
            }

            // Gets or sets total measurements received for this <see cref="IDevice"/> - in local context "successful connections" per day.
            public long MeasurementsReceived
            {
                get
                {
                    return m_parent.m_measurementsReceived;
                }
                set
                {
                    // Ignoring updates
                }
            }

            // Gets or sets total measurements expected to have been received for this <see cref="IDevice"/> - in local context "attempted connections" per day.
            public long MeasurementsExpected
            {
                get
                {
                    return m_parent.m_measurementsExpected;
                }
                set
                {
                    // Ignoring updates
                }
            }

            // Gets or sets the number of measurements received while this <see cref="IDevice"/> was reporting errors.
            public long MeasurementsWithError
            {
                get
                {
                    return 0;
                }
                set
                {
                    // Ignoring updates
                }
            }

            // Gets or sets the number of measurements (per frame) defined for this <see cref="IDevice"/>.
            public long MeasurementsDefined
            {
                get
                {
                    return m_parent.OutputMeasurements.Length;
                }
                set
                {
                    // Ignoring updates
                }
            }
        }

        // Constants
        private const byte DefaultUnitID = 255;
        private const int DefaultPollingRate = 2000;
        private const int DefaultInterSequenceGroupPollDelay = 250;

        // Fields
        private readonly DeviceProxy m_deviceProxy;
        private IModbusMaster m_modbusConnection;
        private TcpClient m_tcpClient;
        private UdpClient m_udpClient;
        private SerialPort m_serialClient;
        private byte m_unitID;
        private int m_pollingRate;
        private int m_interSequenceGroupPollDelay;
        private Dictionary<MeasurementKey, DerivedValue> m_derivedValues;
        private readonly Dictionary<MeasurementKey, string> m_derivedStrings;
        private List<Sequence> m_sequences;
        private Timer m_pollingTimer;
        private ShortSynchronizedOperation m_pollingOperation;
        private long m_pollOperations;
        private long m_deviceErrors;
        private long m_measurementsReceived;
        private long m_measurementsExpected;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ModbusPoller"/>.
        /// </summary>
        public ModbusPoller()
        {
            m_deviceProxy = new DeviceProxy(this);
            m_derivedStrings = new Dictionary<MeasurementKey, string>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets unit ID for Modbus connection.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines unit ID for Modbus connection."),
        DefaultValue(DefaultUnitID)]
        public byte UnitID
        {
            get
            {
                return m_unitID;
            }
            set
            {
                m_unitID = value;
            }
        }

        /// <summary>
        /// Gets or sets polling rate, in milliseconds, for Modbus connection.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines overall polling rate, in milliseconds, for Modbus connection."),
        DefaultValue(DefaultPollingRate)]
        public int PollingRate
        {
            get
            {
                return m_pollingRate;
            }
            set
            {
                m_pollingRate = value;
            }
        }

        /// <summary>
        /// Gets or sets inter sequence-group poll delay, in milliseconds, for Modbus connection.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines inter sequence-group poll delay, in milliseconds, for Modbus connection."),
        DefaultValue(DefaultInterSequenceGroupPollDelay)]
        public int InterSequenceGroupPollDelay
        {
            get
            {
                return m_interSequenceGroupPollDelay;
            }
            set
            {
                m_interSequenceGroupPollDelay = value;
            }
        }

        /// <summary>
        /// Gets total read or derived measurements over the Modbus poller instance lifetime.
        /// </summary>
        public long MeasurementsReceived => m_measurementsReceived;

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        /// <remarks>
        /// Derived classes should return true when data input source is connects asynchronously, otherwise return false.
        /// </remarks>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendFormat("                   Unit ID: {0}", UnitID);
                status.AppendLine();
                status.AppendFormat("              Polling Rate: {0:N0}ms", PollingRate);
                status.AppendLine();
                status.AppendFormat("Inter Seq-Group Poll Delay: {0:N0}ms", InterSequenceGroupPollDelay);
                status.AppendLine();
                status.AppendFormat("  Executed Poll Operations: {0:N0}", m_pollOperations);
                status.AppendLine();
                status.AppendFormat("             Device Errors: {0:N0}", m_deviceErrors);
                status.AppendLine();
                status.AppendFormat("     Measurements Received: {0:N0}", m_measurementsReceived);
                status.AppendLine();
                status.AppendFormat("     Measurements Expected: {0:N0}", m_measurementsExpected);
                status.AppendLine();
                status.AppendFormat("      Last Derived Strings: {0:N0} total", m_derivedStrings.Count);
                status.AppendLine();

                foreach (KeyValuePair<MeasurementKey, string> item in m_derivedStrings)
                {
                    status.AppendFormat("{0} = {1}", item.Key.ToString().PadLeft(10), item.Value);
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ModbusPoller"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        StatisticsEngine.Unregister(m_deviceProxy);

                        DisposeConnections();

                        if ((object)m_pollingTimer != null)
                        {
                            m_pollingTimer.Enabled = false;
                            m_pollingTimer.Elapsed -= m_pollingTimer_Elapsed;
                            m_pollingTimer.Dispose();
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        private void DisposeConnections()
        {
            m_tcpClient?.Dispose();
            m_udpClient?.Dispose();
            m_serialClient?.Dispose();
            m_modbusConnection?.Dispose();
        }

        /// <summary>
        /// Initializes <see cref="ModbusPoller" />.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            ConnectionStringParser<ConnectionStringParameterAttribute> parser = new ConnectionStringParser<ConnectionStringParameterAttribute>();
            parser.ParseConnectionString(ConnectionString, this);

            // Register downloader with the statistics engine
            StatisticsEngine.Register(this, "Modbus", "MOD");
            StatisticsEngine.Register(m_deviceProxy, Name, "Device", "PMU");

            // Attach to output measurements for Modbus device
            OutputMeasurements = ParseOutputMeasurements(DataSource, false, $"FILTER ActiveMeasurements WHERE Device = '{Name}'");

            // Parse derived value expressions from defined signal reference fields
            m_derivedValues = OutputMeasurements.Select(measurement => measurement.Key).ToDictionary(key => key, key =>
            {
                DataTable measurements = DataSource.Tables["ActiveMeasurements"];
                DerivedValue derivedValue = null;
                DataRow[] records = measurements.Select($"ID = '{key}'");

                if (records.Length > 0)
                    derivedValue = ParseDerivedValue(records[0]["SignalReference"].ToNonNullString());

                return derivedValue;
            });

            m_sequences = new List<Sequence>();

            Dictionary<string, string> settings = Settings;
            string setting;
            int sequenceCount = 0;

            if (settings.TryGetValue("sequenceCount", out setting))
                int.TryParse(setting, out sequenceCount);

            for (int i = 0; i < sequenceCount; i++)
            {
                if (settings.TryGetValue($"sequence{i}", out setting))
                {
                    Dictionary<string, string> sequenceSettings = setting.ParseKeyValuePairs();
                    SequenceType sequenceType = SequenceType.Read;

                    if (sequenceSettings.TryGetValue("sequenceType", out setting))
                        Enum.TryParse(setting, true, out sequenceType);

                    Sequence sequence = new Sequence(sequenceType);
                    int groupCount;

                    if (sequenceSettings.TryGetValue("groupCount", out setting) && int.TryParse(setting, out groupCount))
                    {
                        for (int j = 0; j < groupCount; j++)
                        {
                            Group group = new Group();

                            if (sequenceSettings.TryGetValue($"groupType{j}", out setting))
                                Enum.TryParse(setting, true, out group.Type);

                            if (sequenceSettings.TryGetValue($"groupStartAddress{j}", out setting))
                                ushort.TryParse(setting, out group.StartAddress);

                            if (sequenceSettings.TryGetValue($"groupPointCount{j}", out setting))
                                ushort.TryParse(setting, out group.PointCount);

                            if (group.PointCount > 0)
                            {
                                // Load any defined write sequence values
                                if (sequence.Type == SequenceType.Write)
                                {
                                    group.DataValues = new ushort[group.PointCount];

                                    for (int k = 0; k < group.PointCount; k++)
                                    {
                                        if (sequenceSettings.TryGetValue($"group{j}DataValue{k}", out setting))
                                            ushort.TryParse(setting, out group.DataValues[k]);
                                    }
                                }

                                sequence.Groups.Add(group);
                            }
                        }
                    }

                    if (sequence.Groups.Count > 0)
                        m_sequences.Add(sequence);
                }
            }

            if (m_sequences.Count == 0)
                throw new InvalidOperationException("No sequences defined, cannot start Modbus polling.");

            // Define synchronized polling operation
            m_pollingOperation = new ShortSynchronizedOperation(PollingOperation, exception => OnProcessException(MessageLevel.Warning, exception));

            // Define polling timer
            m_pollingTimer = new Timer(m_pollingRate);
            m_pollingTimer.AutoReset = true;
            m_pollingTimer.Elapsed += m_pollingTimer_Elapsed;
        }

        private DerivedValue ParseDerivedValue(string signalReference)
        {
            if (string.IsNullOrWhiteSpace(signalReference))
                return null;

            DerivedValue derivedValue;

            // Remove device name from signal reference
            if (signalReference.StartsWith(Name, StringComparison.OrdinalIgnoreCase))
                signalReference = signalReference.Substring(Name.Length);

            int indexOfType = signalReference.IndexOf("-DV!", StringComparison.OrdinalIgnoreCase);

            if (indexOfType < 0)
            {
                // Instantaneous value
                derivedValue = new DerivedValue(DerivedType.UInt16);

                string addressDefinition = signalReference.Substring(signalReference.IndexOf('-') + 1);
                string recordType = addressDefinition.Substring(0, 2).ToUpperInvariant();
                ushort address = ushort.Parse(addressDefinition.Substring(2));

                derivedValue.AddressRecords.Add(new AddressRecord(recordType, address));
            }
            else
            {
                // Derived type value
                indexOfType += 4;

                int indexOfAt = signalReference.IndexOf('@', indexOfType);
                string derivedType = signalReference.Substring(indexOfType, indexOfAt - indexOfType).ToUpperInvariant();
                string[] addressList = signalReference.Substring(indexOfAt + 1).Split('#');

                derivedValue = new DerivedValue(derivedType);

                for (int i = 0; i < addressList.Length; i++)
                {
                    string addressDefinition = addressList[i].Trim();
                    string recordType = addressDefinition.Substring(0, 2).ToUpperInvariant();
                    ushort address = ushort.Parse(addressDefinition.Substring(2));
                    derivedValue.AddressRecords.Add(new AddressRecord(recordType, address));
                }
            }

            return derivedValue;
        }

        private void PollingOperation()
        {
            if ((object)m_modbusConnection == null)
                return;

            try
            {
                Ticks timestamp = DateTime.UtcNow.Ticks;
                Dictionary<MeasurementKey, Measurement> measurements = OutputMeasurements.Select(measurement => Measurement.Clone(measurement, timestamp)).ToDictionary(measurement => measurement.Key, measurement => measurement);
                Group[] groups = m_sequences.SelectMany(sequence => sequence.Groups).ToArray();

                int measurementsReceived = 0;
                int overallTasksCompleted = 0;
                int overallTasksCount = m_sequences.Count + 1;

                OnStatusMessage(MessageLevel.Info, $"Executing poll operation {m_pollOperations + 1}.");

                // Entering the queued state will clear the message well on the UI
                OnProgressUpdated(this, new ProgressUpdate() { State = ProgressState.Queued });

                OnProgressUpdated(this, new ProgressUpdate()
                {
                    State = ProgressState.Processing,
                    Message = "Starting polling operation...",
                    OverallProgress = overallTasksCompleted,
                    OverallProgressTotal = overallTasksCount,
                    Progress = 0,
                    ProgressTotal = 1
                });

                // Handle read/write operations for sequence groups
                try
                {
                    foreach (Sequence sequence in m_sequences)
                    {
                        foreach (Group group in sequence.Groups)
                        {
                            OnProgressUpdated(this, new ProgressUpdate()
                            {
                                Message = $"Executing poll for {sequence.Type.ToString().ToLowerInvariant()} sequence group \"{group.Type}@{group.StartAddress}\"...",
                                Progress = 0
                            });

                            switch (group.Type)
                            {
                                case RecordType.DI:
                                    group.DataValues = ReadDiscreteInputs(group.StartAddress, group.PointCount);
                                    break;
                                case RecordType.CO:
                                    if (sequence.Type == SequenceType.Read)
                                        group.DataValues = ReadCoils(group.StartAddress, group.PointCount);
                                    else
                                        WriteCoils(group.StartAddress, group.DataValues);
                                    break;
                                case RecordType.IR:
                                    group.DataValues = ReadInputRegisters(group.StartAddress, group.PointCount);
                                    break;
                                case RecordType.HR:
                                    if (sequence.Type == SequenceType.Read)
                                        group.DataValues = ReadHoldingRegisters(group.StartAddress, group.PointCount);
                                    else
                                        WriteHoldingRegisters(group.StartAddress, group.DataValues);
                                    break;
                            }

                            OnProgressUpdated(this, new ProgressUpdate()
                            {
                                Message = $"Completed poll for {sequence.Type.ToString().ToLowerInvariant()} sequence group \"{group.Type}@{group.StartAddress}\".",
                                Progress = 1
                            });

                            Thread.Sleep(m_interSequenceGroupPollDelay);
                        }

                        OnProgressUpdated(this, new ProgressUpdate() { OverallProgress = ++overallTasksCompleted });
                    }
                }
                catch
                {
                    m_deviceErrors++;
                    throw;
                }

                OnProgressUpdated(this, new ProgressUpdate() { Message = "Processing derived values...", Progress = 0, ProgressTotal = m_derivedValues.Count });

                // Calculate derived values
                foreach (KeyValuePair<MeasurementKey, DerivedValue> item in m_derivedValues)
                {
                    DerivedValue derivedValue = item.Value;
                    ushort[] dataValues = derivedValue.GetDataValues(groups);
                    Measurement measurement;

                    if (measurements.TryGetValue(item.Key, out measurement))
                    {
                        // TODO: Properly interpret measurement types after GSF data type transport update
                        switch (derivedValue.Type)
                        {
                            case DerivedType.String:
                                if (derivedValue.AddressRecords.Count > 0)
                                {
                                    m_derivedStrings[item.Key] = DeriveString(dataValues);
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"No address records defined for derived String value \"{item.Key}\".");
                                }
                                break;
                            case DerivedType.Single:
                                if (derivedValue.AddressRecords.Count > 1)
                                {
                                    measurement.Value = DeriveSingle(dataValues[0], dataValues[1]);
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"{derivedValue.AddressRecords.Count} address records defined for derived Single value \"{item.Key}\", expected 2.");
                                }
                                break;
                            case DerivedType.Double:
                                if (derivedValue.AddressRecords.Count > 3)
                                {
                                    measurement.Value = DeriveDouble(dataValues[0], dataValues[1], dataValues[2], dataValues[3]);
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"{derivedValue.AddressRecords.Count} address records defined for derived Double value \"{item.Key}\", expected 4.");
                                }
                                break;
                            case DerivedType.UInt16:
                                if (derivedValue.AddressRecords.Count > 0)
                                {
                                    measurement.Value = dataValues[0];
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"No address records defined for UInt16 value \"{item.Key}\".");
                                }
                                break;
                            case DerivedType.Int32:
                                if (derivedValue.AddressRecords.Count > 1)
                                {
                                    measurement.Value = DeriveInt32(dataValues[0], dataValues[1]);
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"{derivedValue.AddressRecords.Count} address records defined for derived Int32 value \"{item.Key}\", expected 2.");
                                }
                                break;
                            case DerivedType.UInt32:
                                if (derivedValue.AddressRecords.Count > 1)
                                {
                                    measurement.Value = DeriveUInt32(dataValues[0], dataValues[1]);
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"{derivedValue.AddressRecords.Count} address records defined for derived UInt32 value \"{item.Key}\", expected 2.");
                                }
                                break;
                            case DerivedType.Int64:
                                if (derivedValue.AddressRecords.Count > 3)
                                {
                                    measurement.Value = DeriveInt64(dataValues[0], dataValues[1], dataValues[2], dataValues[3]);
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"{derivedValue.AddressRecords.Count} address records defined for derived Int64 value \"{item.Key}\", expected 4.");
                                }
                                break;
                            case DerivedType.UInt64:
                                if (derivedValue.AddressRecords.Count > 3)
                                {
                                    measurement.Value = DeriveUInt64(dataValues[0], dataValues[1], dataValues[2], dataValues[3]);
                                    measurementsReceived++;
                                }
                                else
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"{derivedValue.AddressRecords.Count} address records defined for derived UInt64 value \"{item.Key}\", expected 4.");
                                }
                                break;
                        }
                    }

                    if (measurementsReceived % 20 == 0)
                        OnProgressUpdated(this, new ProgressUpdate() { Progress = measurementsReceived });
                }

                OnNewMeasurements(measurements.Values.ToArray());
                m_measurementsReceived += measurementsReceived;
                m_pollOperations++;

                OnProgressUpdated(this, new ProgressUpdate()
                {
                    State = ProgressState.Success,
                    Message = "Polling operation complete",
                    Summary = $"{m_measurementsReceived} Values Processed",
                    Progress = m_derivedValues.Count,
                    OverallProgress = overallTasksCount
                });
            }
            catch (Exception ex)
            {
                OnProgressUpdated(this, new ProgressUpdate()
                {
                    State = ProgressState.Fail,
                    Message = $"Failed during poll operation: {ex.Message}",
                    OverallProgress = 1,
                    OverallProgressTotal = 1
                });

                // Restart connection cycle when an exception occurs
                Start();
                throw;
            }
            finally
            {
                m_measurementsExpected += OutputMeasurements.Length;
            }
        }

        /// <summary>
        /// Attempts to connect to data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt connection to data input source here.  Any exceptions thrown
        /// by this implementation will result in restart of the connection cycle.
        /// </remarks>
        protected override void AttemptConnection()
        {
            Dictionary<string, string> parameters = Settings;

            string frameFormat, transport, setting;

            if (!parameters.TryGetValue("frameFormat", out frameFormat) || string.IsNullOrWhiteSpace(frameFormat))
                throw new ArgumentException("Connection string is missing \"frameFormat\".");

            if (!parameters.TryGetValue("transport", out transport) || string.IsNullOrWhiteSpace(transport))
                throw new ArgumentException("Connection string is missing \"transport\".");

            if (!parameters.TryGetValue("unitID", out setting) || !byte.TryParse(setting, out m_unitID))
                throw new ArgumentException("Connection string is missing \"unitID\" or value is invalid.");

            bool useIP = false;
            bool useRTU = false;

            switch (frameFormat.ToUpperInvariant())
            {
                case "RTU":
                    useRTU = true;
                    break;
                case "TCP":
                    useIP = true;
                    break;
            }

            if (useIP)
            {
                int port;

                if (!parameters.TryGetValue("port", out setting) || !int.TryParse(setting, out port))
                    throw new ArgumentException("Connection string is missing \"port\" or value is invalid.");

                if (transport.ToUpperInvariant() == "TCP")
                {
                    string hostName;

                    if (!parameters.TryGetValue("hostName", out hostName) || string.IsNullOrWhiteSpace(hostName))
                        throw new ArgumentException("Connection string is missing \"hostName\".");

                    m_tcpClient = new TcpClient(hostName, port);
                    m_modbusConnection = ModbusIpMaster.CreateIp(m_tcpClient);

                    m_pollingTimer.Enabled = true;
                    return;
                }

                string interfaceIP;

                if (!parameters.TryGetValue("interface", out interfaceIP))
                    interfaceIP = "0.0.0.0";

                m_udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(interfaceIP), port));
                m_modbusConnection = ModbusIpMaster.CreateIp(m_udpClient);

                m_pollingTimer.Enabled = true;
                return;
            }

            string portName;
            int baudRate;
            int dataBits;
            Parity parity;
            StopBits stopBits;

            if (!parameters.TryGetValue("portName", out portName) || string.IsNullOrWhiteSpace(portName))
                throw new ArgumentException("Connection string is missing \"portName\".");

            if (!parameters.TryGetValue("baudRate", out setting) || !int.TryParse(setting, out baudRate))
                throw new ArgumentException("Connection string is missing \"baudRate\" or value is invalid.");

            if (!parameters.TryGetValue("dataBits", out setting) || !int.TryParse(setting, out dataBits))
                throw new ArgumentException("Connection string is missing \"dataBits\" or value is invalid.");

            if (!parameters.TryGetValue("parity", out setting) || !Enum.TryParse(setting, out parity))
                throw new ArgumentException("Connection string is missing \"parity\" or value is invalid.");

            if (!parameters.TryGetValue("stopBits", out setting) || !Enum.TryParse(setting, out stopBits))
                throw new ArgumentException("Connection string is missing \"stopBits\" or value is invalid.");

            m_serialClient = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            m_modbusConnection = useRTU ? ModbusSerialMaster.CreateRtu(m_serialClient) : ModbusSerialMaster.CreateAscii(m_serialClient);

            m_pollingTimer.Enabled = true;
        }

        /// <summary>
        /// Attempts to disconnect from data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt disconnect from data input source here.  Any exceptions thrown
        /// by this implementation will be reported to host via <see cref="E:GSF.TimeSeries.Adapters.AdapterBase.ProcessException" /> event.
        /// </remarks>
        protected override void AttemptDisconnection()
        {
            if ((object)m_pollingTimer != null)
                m_pollingTimer.Enabled = false;

            DisposeConnections();
            OnStatusMessage(MessageLevel.Info, "Device disconnected.");
        }

        /// <summary>
        /// Gets a short one-line status of this adapter.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>
        /// A short one-line summary of the current status of this adapter.
        /// </returns>
        public override string GetShortStatus(int maxLength)
        {
            if (!Enabled)
                return "Polling is disabled...".CenterText(maxLength);

            return $"Polling enabled for every {PollingRate:N0}ms".CenterText(maxLength);
        }

        /// <summary>
        /// Queues polling operation for immediate execution.
        /// </summary>
        [AdapterCommand("Queues polling operation for immediate execution.", "Administrator", "Editor")]
        public void QueueTasks()
        {
            m_pollingOperation?.RunOnceAsync();
        }

        private ushort[] ReadDiscreteInputs(ushort startAddress, ushort pointCount)
        {
            return m_modbusConnection.ReadInputs(m_unitID, startAddress, pointCount).Select(value => (ushort)(value ? 1 : 0)).ToArray();
        }

        private ushort[] ReadCoils(ushort startAddress, ushort pointCount)
        {
            return m_modbusConnection.ReadCoils(m_unitID, startAddress, pointCount).Select(value => (ushort)(value ? 1 : 0)).ToArray();
        }

        private ushort[] ReadInputRegisters(ushort startAddress, ushort pointCount)
        {
            return m_modbusConnection.ReadInputRegisters(m_unitID, startAddress, pointCount);
        }

        private ushort[] ReadHoldingRegisters(ushort startAddress, ushort pointCount)
        {
            return m_modbusConnection.ReadHoldingRegisters(m_unitID, startAddress, pointCount);
        }

        private void WriteCoils(ushort startAddress, ushort[] data)
        {
            m_modbusConnection.WriteMultipleCoilsAsync(m_unitID, startAddress, data.Select(value => value != 0).ToArray());
        }

        private void WriteHoldingRegisters(ushort startAddress, ushort[] data)
        {
            m_modbusConnection.WriteMultipleRegistersAsync(m_unitID, startAddress, data);
        }

        private string DeriveString(ushort[] values)
        {
            return Encoding.Default.GetString(values.Select(BigEndianOrder.Default.GetBytes).SelectMany(bytes => bytes).ToArray());
        }

        private float DeriveSingle(ushort highValue, ushort lowValue)
        {
            return ModbusUtility.GetSingle(highValue, lowValue);
        }

        private double DeriveDouble(ushort b3, ushort b2, ushort b1, ushort b0)
        {
            return ModbusUtility.GetDouble(b3, b2, b1, b0);
        }

        private int DeriveInt32(ushort highValue, ushort lowValue)
        {
            return (int)ModbusUtility.GetUInt32(highValue, lowValue);
        }

        private uint DeriveUInt32(ushort highValue, ushort lowValue)
        {
            return ModbusUtility.GetUInt32(highValue, lowValue);
        }

        private long DeriveInt64(ushort b3, ushort b2, ushort b1, ushort b0)
        {
            return (long)DeriveUInt64(b3, b2, b1, b0);
        }

        private ulong DeriveUInt64(ushort b3, ushort b2, ushort b1, ushort b0)
        {
            return Word.MakeQuadWord(ModbusUtility.GetUInt32(b3, b2), ModbusUtility.GetUInt32(b1, b0));
        }

        private void m_pollingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_pollingOperation?.RunOnce();
        }

        #endregion

        #region [ Static ]

        // Static Events

        /// <summary>
        /// Raised when there is a Modbus data operation in progress notification for any downloader instance.
        /// </summary>
        public static event EventHandler<EventArgs<ProgressUpdate>> ProgressUpdated;

        // Static Methods

        private static void OnProgressUpdated(ModbusPoller instance, ProgressUpdate update)
        {
            ProgressUpdated?.Invoke(instance, new EventArgs<ProgressUpdate>(update));
        }

        /// <summary>
        /// Restores embedded resource Modbus configurations.
        /// </summary>
        /// <param name="targetPath">Target locations for restoration.</param>
        public static void RestoreConfigurations(string targetPath)
        {
            Assembly executingAssembly = AssemblyInfo.ExecutingAssembly.Assembly;

            targetPath = FilePath.AddPathSuffix(targetPath);

            foreach (string name in executingAssembly.GetManifestResourceNames().Where(name => name.EndsWith(".json")))
            {
                using (Stream resourceStream = executingAssembly.GetManifestResourceStream(name))
                {
                    if ((object)resourceStream != null)
                    {
                        string filePath = name;
                        
                        //                                 1         2        
                        //                       012345678901234567890123456789
                        if (filePath.StartsWith("ModbusAdapters.ModbusConfigs."))
                            filePath = filePath.Substring(29);

                        filePath = $"{FilePath.GetFileNameWithoutExtension(filePath).Replace('.', Path.DirectorySeparatorChar).Replace("_", "")}.json";

                        string targetFileName = Path.Combine(targetPath, filePath);
                        bool restoreFile = true;

                        if (File.Exists(targetFileName))
                        {
                            string resourceMD5 = GetMD5HashFromStream(resourceStream);
                            resourceStream.Seek(0, SeekOrigin.Begin);
                            restoreFile = !resourceMD5.Equals(GetMD5HashFromFile(targetFileName));
                        }

                        if (restoreFile)
                        {
                            byte[] buffer = new byte[resourceStream.Length];
                            resourceStream.Read(buffer, 0, (int)resourceStream.Length);

                            string directory = Path.GetDirectoryName(targetFileName);

                            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                                Directory.CreateDirectory(directory);

                            using (StreamWriter writer = File.CreateText(targetFileName))
                                writer.Write(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                        }
                    }
                }
            }
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return GetMD5HashFromStream(stream);
        }

        private static string GetMD5HashFromStream(Stream stream)
        {
            using (MD5 md5 = MD5.Create())
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
        }

        #endregion
    }
}
