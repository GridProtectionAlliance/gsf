//******************************************************************************************************
//  PhasorHub.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  08/12/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF;
using GSF.ComponentModel;
using GSF.Data.Model;
using GSF.IO;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.Units.EE;
using GSF.Web.Hubs;
using GSF.Web.Model;
using GSF.Web.Security;
using PhasorProtocolAdapters;
using PhasorWebUI.Adapters;
using PhasorWebUI.Model;
using PowerCalculations.PowerMultiCalculator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading.Tasks;
using AnalogDefinition = PhasorWebUI.Adapters.AnalogDefinition;
using ConfigurationCell = PhasorWebUI.Adapters.ConfigurationCell;
using DigitalDefinition = PhasorWebUI.Adapters.DigitalDefinition;
using FrequencyDefinition = PhasorWebUI.Adapters.FrequencyDefinition;
using Phasor = PhasorWebUI.Model.Phasor;
using PhasorDefinition = PhasorWebUI.Adapters.PhasorDefinition;
using PowerCalculation = PhasorWebUI.Model.PowerCalculation;
using SignalType = PhasorWebUI.Model.SignalType;

#pragma warning disable CS1591

namespace PhasorWebUI
{
    /// <summary>
    /// Represents a data hub used for web-based synchrophasor operations.
    /// </summary>
    [AuthorizeHubRole]
    public class PhasorHub : RecordOperationsHub<PhasorHub>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorHub"/>.
        /// </summary>
        public PhasorHub() : 
            this(null, null, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorHub"/> with the specified logging functions.
        /// </summary>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public PhasorHub(Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) :
            this(null, logStatusMessageFunction, logExceptionFunction)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorHub"/> with the specified <see cref="DataContext"/> and logging functions.
        /// </summary>
        /// <param name="settingsCategory">Setting category that contains the connection settings. Defaults to "securityProvider".</param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public PhasorHub(string settingsCategory, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) : 
            this(settingsCategory, logStatusMessageFunction, logExceptionFunction, true)
        {
            // Capture initial defaults
            if ((object)logStatusMessageFunction != null && (object)s_logStatusMessageFunction == null)
                s_logStatusMessageFunction = logStatusMessageFunction;

            if ((object)logExceptionFunction != null && (object)s_logExceptionFunction == null)
                s_logExceptionFunction = logExceptionFunction;
        }

        // ReSharper disable once UnusedParameter.Local
        private PhasorHub(string settingsCategory, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction, bool overload) :
            base(settingsCategory ?? "systemSettings", logStatusMessageFunction ?? s_logStatusMessageFunction, logExceptionFunction ?? s_logExceptionFunction)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Overrides base OnConnected method to provide logging
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            LogStatusMessage($"PhasorHub connect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {ConnectionCount}", UpdateType.Information, false);
            return base.OnConnected();
        }

        /// <summary>
        /// Overrides base OnDisconnected method to provide logging
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                // Dispose any associated hub operations associated with current SignalR client
                LogStatusMessage($"PhasorHub disconnect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {ConnectionCount}", UpdateType.Information, false);
            }

            return base.OnDisconnected(stopCalled);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static Action<string, UpdateType> s_logStatusMessageFunction;
        private static Action<Exception> s_logExceptionFunction;
        private static int s_ieeeC37_118ProtocolID;
        private static int s_virtualProtocolID;
        private static dynamic s_appModelGlobal;
        private static string s_companyAcronym;
        private static double s_defaultCalculationLagTime;
        private static double s_defaultCalculationLeadTime;
        private static int s_defaultCalculationFramesPerSecond;

        // Quasi-Static Properties

        private int IeeeC37_118ProtocolID => s_ieeeC37_118ProtocolID != default(int) ? s_ieeeC37_118ProtocolID : s_ieeeC37_118ProtocolID = DataContext.Connection.ExecuteScalar<int>("SELECT ID FROM Protocol WHERE Acronym='IeeeC37_118V1'");

        private int VirtualProtocolID => s_virtualProtocolID != default(int) ? s_virtualProtocolID : s_virtualProtocolID = DataContext.Connection.ExecuteScalar<int>("SELECT ID FROM Protocol WHERE Acronym='VirtualInput'");

        private dynamic AppModelGlobal => s_appModelGlobal != default(dynamic) ? s_appModelGlobal : s_appModelGlobal = (ValueExpressionParser.DefaultTypeRegistry["Global"] as ExpressionEvaluator.ValueType)?.Value;

        private string CompanyAcronym => s_companyAcronym ?? (s_companyAcronym = AppModelGlobal.CompanyAcronym);

        private double DefaultCalculationLagTime => s_defaultCalculationLagTime != default(double) ? s_defaultCalculationLagTime : s_defaultCalculationLagTime = AppModelGlobal.DefaultCalculationLagTime;
        
        private double DefaultCalculationLeadTime => s_defaultCalculationLeadTime != default(double) ? s_defaultCalculationLeadTime : s_defaultCalculationLeadTime = AppModelGlobal.DefaultCalculationLeadTime;

        private int DefaultCalculationFramesPerSecond => s_defaultCalculationFramesPerSecond != default(int) ? s_defaultCalculationFramesPerSecond : s_defaultCalculationFramesPerSecond = AppModelGlobal.DefaultCalculationFramesPerSecond;

        private const string SystemFrequencyDeviceName = "{0}SYSTEM!FREQ";

        #endregion

        // Client-side script functionality

        #region [ Device Table Operations ]

        [RecordOperation(typeof(Device), RecordOperation.QueryRecordCount)]

        public int QueryDeviceCount(Guid nodeID, string filterText)
        {
            TableOperations<Device> deviceTable = DataContext.Table<Device>();

            RecordRestriction restriction =
                new RecordRestriction("NodeID = {0}", nodeID) +
                deviceTable.GetSearchRestriction(filterText);

            return deviceTable.QueryRecordCount(restriction);
        }

        [RecordOperation(typeof(Device), RecordOperation.QueryRecords)]
        public IEnumerable<Device> QueryDevices(Guid nodeID, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            TableOperations<Device> deviceTable = DataContext.Table<Device>();

            RecordRestriction restriction =
                new RecordRestriction("NodeID = {0}", nodeID) +
                deviceTable.GetSearchRestriction(filterText);

            return deviceTable.QueryRecords(sortField, ascending, page, pageSize, restriction);
        }

        public IEnumerable<Device> QueryEnabledDevices(Guid nodeID, int limit, string filterText)
        {
            TableOperations<Device> deviceTable = DataContext.Table<Device>();

            RecordRestriction restriction =
                new RecordRestriction("NodeID = {0}", nodeID) +
                new RecordRestriction("Enabled <> 0") +
                deviceTable.GetSearchRestriction(filterText);

            return deviceTable.QueryRecords("Acronym", restriction, limit);
        }

        public Device QueryDevice(string acronym)
        {
            return DataContext.Table<Device>().QueryRecordWhere("Acronym = {0}", acronym) ?? NewDevice();
        }

        public Device QueryDeviceByID(int deviceID)
        {
            return DataContext.Table<Device>().QueryRecordWhere("ID = {0}", deviceID) ?? NewDevice();
        }

        public IEnumerable<Device> QueryChildDevices(int deviceID)
        {
            return DataContext.Table<Device>().QueryRecordsWhere("ParentID = {0}", deviceID);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Device), RecordOperation.DeleteRecord)]
        public void DeleteDevice(int id)
        {
            // TODO: Delete associated custom action adapters (generated by tag templates)
            TableOperations<Device> deviceTable = DataContext.Table<Device>();            
            deviceTable.DeleteRecordWhere("ParentID = {0}", id);
            deviceTable.DeleteRecord(id);
        }

        [RecordOperation(typeof(Device), RecordOperation.CreateNewRecord)]
        public Device NewDevice()
        {
            return DataContext.Table<Device>().NewRecord();
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Device), RecordOperation.AddNewRecord)]
        public void AddNewDevice(Device device)
        {
            if ((device.ProtocolID ?? 0) == 0)
                device.ProtocolID = IeeeC37_118ProtocolID;

            if (device.UniqueID == Guid.Empty)
                device.UniqueID = Guid.NewGuid();

            DataContext.Table<Device>().AddNewRecord(device);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Device), RecordOperation.UpdateRecord)]
        public void UpdateDevice(Device device)
        {
            if ((device.ProtocolID ?? 0) == 0)
                device.ProtocolID = IeeeC37_118ProtocolID;

            if (device.UniqueID == Guid.Empty)
                device.UniqueID = Guid.NewGuid();

            DataContext.Table<Device>().UpdateRecord(device);

            // TODO: Update name, if changed, of associated custom action adapters (generated by tag templates)
        }

        [AuthorizeHubRole("Administrator, Editor")]
        public void AddNewOrUpdateDevice(Device device)
        {
            DataContext.Table<Device>().AddNewOrUpdateRecord(device);
        }

        #endregion

        #region [ Measurement Table Operations ]

        [RecordOperation(typeof(Measurement), RecordOperation.QueryRecordCount)]
        public int QueryMeasurementCount(string filterText)
        {
            return DataContext.Table<Measurement>().QueryRecordCount(filterText);
        }

        [RecordOperation(typeof(Measurement), RecordOperation.QueryRecords)]
        public IEnumerable<Measurement> QueryMeasurements(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<Measurement>().QueryRecords(sortField, ascending, page, pageSize, filterText);
        }

        public Measurement QueryMeasurement(string signalReference)
        {
            return DataContext.Table<Measurement>().QueryRecordWhere("SignalReference = {0}", signalReference) ?? NewMeasurement();
        }

        public Measurement QueryMeasurementByPointTag(string pointTag)
        {
            return DataContext.Table<Measurement>().QueryRecordWhere("PointTag = {0}", pointTag) ?? NewMeasurement();
        }

        public Measurement QueryMeasurementBySignalID(Guid signalID)
        {
            return DataContext.Table<Measurement>().QueryRecordWhere("SignalID = {0}", signalID) ?? NewMeasurement();
        }

        public IEnumerable<Measurement> QueryDeviceMeasurements(int deviceID)
        {
            return DataContext.Table<Measurement>().QueryRecordsWhere("DeviceID = {0}", deviceID);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Measurement), RecordOperation.DeleteRecord)]
        public void DeleteMeasurement(int id)
        {
            DataContext.Table<Measurement>().DeleteRecord(id);
        }

        [RecordOperation(typeof(Measurement), RecordOperation.CreateNewRecord)]
        public Measurement NewMeasurement()
        {
            return DataContext.Table<Measurement>().NewRecord();
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Measurement), RecordOperation.AddNewRecord)]
        public void AddNewMeasurement(Measurement measurement)
        {
            DataContext.Table<Measurement>().AddNewRecord(measurement);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Measurement), RecordOperation.UpdateRecord)]
        public void UpdateMeasurement(Measurement measurement)
        {
            DataContext.Table<Measurement>().UpdateRecord(measurement);
        }

        public void AddNewOrUpdateMeasurement(Measurement measurement)
        {
            DataContext.Table<Measurement>().AddNewOrUpdateRecord(measurement);
        }

        #endregion

        #region [ Phasor Table Operations ]

        [RecordOperation(typeof(Phasor), RecordOperation.QueryRecordCount)]
        public int QueryPhasorCount(string filterText)
        {
            return DataContext.Table<Phasor>().QueryRecordCount(filterText);
        }

        [RecordOperation(typeof(Phasor), RecordOperation.QueryRecords)]
        public IEnumerable<Phasor> QueryPhasors(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<Phasor>().QueryRecords(sortField, ascending, page, pageSize, filterText);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Phasor), RecordOperation.DeleteRecord)]
        public void DeletePhasor(int id)
        {
            DataContext.Table<Phasor>().DeleteRecord(id);
        }

        [RecordOperation(typeof(Phasor), RecordOperation.CreateNewRecord)]
        public Phasor NewPhasor()
        {
            return DataContext.Table<Phasor>().NewRecord();
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Phasor), RecordOperation.AddNewRecord)]
        public void AddNewPhasor(Phasor phasor)
        {
            DataContext.Table<Phasor>().AddNewRecord(phasor);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Phasor), RecordOperation.UpdateRecord)]
        public void UpdatePhasor(Phasor phasor)
        {
            DataContext.Table<Phasor>().UpdateRecord(phasor);
        }

        public Phasor QueryPhasorForDevice(int deviceID, int sourceIndex)
        {
            return DataContext.Table<Phasor>().QueryRecordWhere("DeviceID = {0} AND SourceIndex = {1}", deviceID, sourceIndex) ?? NewPhasor();
        }

        public IEnumerable<Phasor> QueryPhasorsForDevice(int deviceID)
        {
            return DataContext.Table<Phasor>().QueryRecordsWhere("DeviceID = {0}", deviceID).OrderBy(phasor => phasor.SourceIndex);
        }

        public int QueryPhasorCountForDevice(int deviceID)
        {
            return DataContext.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Phasor WHERE DeviceID = {0}", deviceID);
        }

        public int DeletePhasorsForDevice(int deviceID)
        {
            return DataContext.Connection.ExecuteScalar<int>("DELETE FROM Phasor WHERE DeviceID = {0}", deviceID);
        }

        #endregion

        #region [ PowerCalculation Table Operations ]

        [RecordOperation(typeof(PowerCalculation), RecordOperation.QueryRecordCount)]
        public int QueryPowerCalculationCount(string filterText)
        {
            return DataContext.Table<PowerCalculation>().QueryRecordCount(filterText);
        }

        [RecordOperation(typeof(PowerCalculation), RecordOperation.QueryRecords)]
        public IEnumerable<PowerCalculation> QueryPowerCalculations(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<PowerCalculation>().QueryRecords(sortField, ascending, page, pageSize, filterText);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(PowerCalculation), RecordOperation.DeleteRecord)]
        public void DeletePowerCalculation(int id)
        {
            DataContext.Table<PowerCalculation>().DeleteRecord(id);
        }

        [RecordOperation(typeof(PowerCalculation), RecordOperation.CreateNewRecord)]
        public PowerCalculation NewPowerCalculation()
        {
            return DataContext.Table<PowerCalculation>().NewRecord();
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(PowerCalculation), RecordOperation.AddNewRecord)]
        public void AddNewPowerCalculation(PowerCalculation powerCalculation)
        {
            DataContext.Table<PowerCalculation>().AddNewRecord(powerCalculation);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(PowerCalculation), RecordOperation.UpdateRecord)]
        public void UpdatePowerCalculation(PowerCalculation powerCalculation)
        {
            DataContext.Table<PowerCalculation>().UpdateRecord(powerCalculation);
        }

        public PowerCalculation QueryPowerCalculationForDescriptionOrInputs(string circuitDescription, Guid voltageAngleSignalID, Guid voltageMagSignalID, Guid currentAngleSignalID, Guid currentMagSignalID)
        {
            return DataContext.Table<PowerCalculation>().QueryRecordWhere($"CircuitDescription LIKE '{circuitDescription}%' OR (VoltageAngleSignalID = {{0}} AND VoltageMagSignalID = {{1}} AND CurrentAngleSignalID = {{2}} AND CurrentMagSignalID = {{3}})", voltageAngleSignalID, voltageMagSignalID, currentAngleSignalID, currentMagSignalID) ?? NewPowerCalculation();
        }

        public PowerCalculation QueryPowerCalculationForInputs(Guid voltageAngleSignalID, Guid voltageMagSignalID, Guid currentAngleSignalID, Guid currentMagSignalID)
        {
            return DataContext.Table<PowerCalculation>().QueryRecordWhere("VoltageAngleSignalID = {0} AND VoltageMagSignalID = {1} AND CurrentAngleSignalID = {2} AND CurrentMagSignalID = {3}", voltageAngleSignalID, voltageMagSignalID, currentAngleSignalID, currentMagSignalID) ?? NewPowerCalculation();
        }

        #endregion

        #region [ CustomActionAdapter Table Operations ]

        [RecordOperation(typeof(CustomActionAdapter), RecordOperation.QueryRecordCount)]
        public int QueryCustomActionAdapterCount(string filterText)
        {
            return DataContext.Table<CustomActionAdapter>().QueryRecordCount(filterText);
        }

        [RecordOperation(typeof(CustomActionAdapter), RecordOperation.QueryRecords)]
        public IEnumerable<CustomActionAdapter> QueryCustomActionAdapters(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<CustomActionAdapter>().QueryRecords(sortField, ascending, page, pageSize, filterText);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(CustomActionAdapter), RecordOperation.DeleteRecord)]
        public void DeleteCustomActionAdapter(int id)
        {
            DataContext.Table<CustomActionAdapter>().DeleteRecord(id);
        }

        [RecordOperation(typeof(CustomActionAdapter), RecordOperation.CreateNewRecord)]
        public CustomActionAdapter NewCustomActionAdapter()
        {
            return DataContext.Table<CustomActionAdapter>().NewRecord();
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(CustomActionAdapter), RecordOperation.AddNewRecord)]
        public void AddNewCustomActionAdapter(CustomActionAdapter customActionAdapter)
        {
            DataContext.Table<CustomActionAdapter>().AddNewRecord(customActionAdapter);
        }

        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(CustomActionAdapter), RecordOperation.UpdateRecord)]
        public void UpdateCustomActionAdapter(CustomActionAdapter customActionAdapter)
        {
            DataContext.Table<CustomActionAdapter>().UpdateRecord(customActionAdapter);
        }

        public void AddNewOrUpdateCustomActionAdapter(CustomActionAdapter customActionAdapter)
        {
            TableOperations<CustomActionAdapter> customActionAdapterTable = DataContext.Table<CustomActionAdapter>();

            if (customActionAdapterTable.QueryRecordCountWhere("AdapterName = {0}", customActionAdapter.AdapterName) == 0)
            {
                AddNewCustomActionAdapter(customActionAdapter);
            }
            else
            {
                CustomActionAdapter existingActionAdapter = customActionAdapterTable.QueryRecordWhere("AdapterName = {0}", customActionAdapter.AdapterName);
                
                existingActionAdapter.AssemblyName = customActionAdapter.AssemblyName;
                existingActionAdapter.TypeName = customActionAdapter.TypeName;
                existingActionAdapter.ConnectionString = customActionAdapter.ConnectionString;
                existingActionAdapter.LoadOrder = customActionAdapter.LoadOrder;
                existingActionAdapter.Enabled = customActionAdapter.Enabled;
                existingActionAdapter.UpdatedBy = customActionAdapter.UpdatedBy;
                existingActionAdapter.UpdatedOn = customActionAdapter.UpdatedOn;
                
                UpdateCustomActionAdapter(existingActionAdapter);
            }
        }

        #endregion

        #region [ Synchrophasor Device Wizard Operations ]

        public IEnumerable<SignalType> LoadSignalTypes(string source)
        {
            return DataContext.Table<SignalType>().QueryRecordsWhere("Source = {0}", source);
        }

        public string CreatePointTag(string deviceAcronym, string signalTypeAcronym)
        {
            return CommonPhasorServices.CreatePointTag(CompanyAcronym, deviceAcronym, null, signalTypeAcronym);
        }

        public string CreateIndexedPointTag(string deviceAcronym, string signalTypeAcronym, int signalIndex)
        {
            return CommonPhasorServices.CreatePointTag(CompanyAcronym, deviceAcronym, null, signalTypeAcronym, null, signalIndex);
        }

        public string CreatePhasorPointTag(string deviceAcronym, string signalTypeAcronym, string phasorLabel, string phase, int signalIndex, int baseKV)
        {
            return CommonPhasorServices.CreatePointTag(CompanyAcronym, deviceAcronym, null, signalTypeAcronym, phasorLabel, signalIndex, string.IsNullOrWhiteSpace(phase) ? '_' : phase.Trim()[0], baseKV);
        }

        public ConfigurationFrame ExtractConfigurationFrame(int deviceID)
        {
            Device device = QueryDeviceByID(deviceID);

            if (device.ID == 0)
                return new ConfigurationFrame();

            ConfigurationFrame derivedFrame = new ConfigurationFrame
            {
                IDCode = (ushort)device.AccessID,
                StationName = device.Name,
                IDLabel = device.Acronym,
                ConnectionString = device.ConnectionString,
                ProtocolID = device.ProtocolID ?? IeeeC37_118ProtocolID
            };

            if ((device.FramesPerSecond ?? 0) > 0)
                derivedFrame.FrameRate = (ushort)device.FramesPerSecond.GetValueOrDefault();

            if (device.ParentID == null)
            {
                IEnumerable<Device> devices = QueryChildDevices(deviceID);

                foreach (Device childDevice in devices)
                {
                    // Create new configuration cell
                    ConfigurationCell derivedCell = new ConfigurationCell
                    {
                        ID = childDevice.ID,
                        ParentID = device.ID,
                        UniqueID = device.UniqueID,
                        Longitude = device.Longitude,
                        Latitude = device.Latitude,
                        IDCode = (ushort)childDevice.AccessID,
                        StationName = childDevice.Name,
                        IDLabel = childDevice.Acronym
                    };

                    derivedCell.FrequencyDefinition = new FrequencyDefinition { Label = "Frequency" };

                    // Extract phasor definitions
                    foreach (Phasor phasor in QueryPhasorsForDevice(childDevice.ID))
                        derivedCell.PhasorDefinitions.Add(new PhasorDefinition { ID = phasor.ID, Label = phasor.Label, PhasorType = phasor.Type == 'V' ? "Voltage" : "Current", Phase = phasor.Phase.ToString(), DestinationPhasorID = phasor.DestinationPhasorID, NominalVoltage = phasor.BaseKV, SourceIndex = phasor.SourceIndex });

                    // Add cell to frame
                    derivedFrame.Cells.Add(derivedCell);
                }

                if (derivedFrame.Cells.Count > 0)
                {
                    derivedFrame.IsConcentrator = true;
                }
                else
                {
                    // This is a directly connected device
                    derivedFrame.IsConcentrator = false;

                    ConfigurationCell derivedCell = new ConfigurationCell
                    {
                        ID = device.ID,
                        UniqueID = device.UniqueID,
                        Longitude = device.Longitude,
                        Latitude = device.Latitude,
                        ParentID = null,
                        IDCode = derivedFrame.IDCode,
                        StationName = device.Name,
                        IDLabel = device.Acronym
                    };

                    derivedCell.FrequencyDefinition = new FrequencyDefinition { Label = "Frequency" };

                    // Extract phasor definitions
                    foreach (Phasor phasor in QueryPhasorsForDevice(device.ID))
                        derivedCell.PhasorDefinitions.Add(new PhasorDefinition { ID = phasor.ID, Label = phasor.Label, PhasorType = phasor.Type == 'V' ? "Voltage" : "Current", Phase = phasor.Phase.ToString(), DestinationPhasorID = phasor.DestinationPhasorID, NominalVoltage = phasor.BaseKV, SourceIndex = phasor.SourceIndex });

                    // Add cell to frame
                    derivedFrame.Cells.Add(derivedCell);
                }
            }
            else
            {
                derivedFrame.IsConcentrator = true;

                // Create new configuration cell
                ConfigurationCell derivedCell = new ConfigurationCell
                {
                    ID = device.ID,
                    UniqueID = device.UniqueID,
                    Longitude = device.Longitude,
                    Latitude = device.Latitude,
                    ParentID = null,
                    IDCode = (ushort)device.AccessID,
                    StationName = device.Name,
                    IDLabel = device.Acronym
                };

                derivedCell.FrequencyDefinition = new FrequencyDefinition { Label = "Frequency" };

                // Extract phasor definitions
                foreach (Phasor phasor in QueryPhasorsForDevice(device.ID))
                    derivedCell.PhasorDefinitions.Add(new PhasorDefinition { ID = phasor.ID, Label = phasor.Label, PhasorType = phasor.Type == 'V' ? "Voltage" : "Current", Phase = phasor.Phase.ToString(), DestinationPhasorID = phasor.DestinationPhasorID, NominalVoltage = phasor.BaseKV, SourceIndex = phasor.SourceIndex });

                // Add cell to frame
                derivedFrame.Cells.Add(derivedCell);
            }

            return derivedFrame;
        }

        public ConfigurationFrame LoadConfigurationFrame(string sourceData)
        {
            IConfigurationFrame sourceFrame = GetConfigurationFrame(sourceData, out string connectionString);

            if (sourceFrame is ConfigurationErrorFrame)
                return new ConfigurationFrame();

            // Create a new simple concrete configuration frame for JSON serialization converted from equivalent configuration information
            int protocolID = 0, deviceID = 0, phasorID = -1; // Start phasor ID's at less than -1 since associated voltage == -1 is reserved as unselected
        
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
                protocolID = GetProtocolID(settings["phasorProtocol"]);
            }

            ConfigurationFrame derivedFrame = new ConfigurationFrame
            {
                IDCode = sourceFrame.IDCode,
                FrameRate = sourceFrame.FrameRate,
                ConnectionString = connectionString,
                ProtocolID = protocolID
            };

            foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
            {
                // Create new derived configuration cell
                ConfigurationCell derivedCell = new ConfigurationCell
                {
                    ID = --deviceID, // Provide a negative index so any database lookup will return null
                    ParentID = null,
                    IDCode = sourceCell.IDCode,
                    StationName = sourceCell.StationName,
                    IDLabel = sourceCell.IDLabel
                };

                if (sourceCell is ConfigurationCell3 configCell3)
                {
                    derivedCell.UniqueID = configCell3.GlobalID;
                    derivedCell.Longitude = configCell3.LongitudeM;
                    derivedCell.Latitude = configCell3.LatitudeM;
                }

                // Create equivalent derived frequency definition
                IFrequencyDefinition sourceFrequency = sourceCell.FrequencyDefinition;

                if (sourceFrequency != null)
                    derivedCell.FrequencyDefinition = new FrequencyDefinition { Label = sourceFrequency.Label };

                int sourceIndex = 0;

                // Create equivalent derived phasor definitions
                foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                {
                    string configPhase = string.Empty;
                    int? nominalVoltage = null;

                    if (sourcePhasor is PhasorDefinition3 phasor3)
                    {
                        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                        switch (phasor3.PhasorComponent)
                        {
                            case PhasorComponent.ZeroSequence:
                                configPhase = "0";
                                break;
                            case PhasorComponent.PositiveSequence:
                                configPhase = "+";
                                break;
                            case PhasorComponent.NegativeSequence:
                                configPhase = "-";
                                break;
                            case PhasorComponent.PhaseA:
                                configPhase = "A";
                                break;
                            case PhasorComponent.PhaseB:
                                configPhase = "B";
                                break;
                            case PhasorComponent.PhaseC:
                                configPhase = "C";
                                break;
                            case PhasorComponent.ReservedPhase:
                                break;
                            default:
                                configPhase = string.Empty;
                                break;
                        }

                        if (Enum.TryParse(phasor3.UserFlags.ToString(), out VoltageLevel level))
                            nominalVoltage = level.Value();
                    }

                    derivedCell.PhasorDefinitions.Add(new PhasorDefinition { ID = --phasorID, Label = sourcePhasor.Label, PhasorType = sourcePhasor.PhasorType.ToString(), Phase = configPhase, NominalVoltage = nominalVoltage, SourceIndex = ++sourceIndex });
                }

                // Create equivalent derived analog definitions (assuming analog type = SinglePointOnWave)
                foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                    derivedCell.AnalogDefinitions.Add(new AnalogDefinition { Label = sourceAnalog.Label, AnalogType = sourceAnalog.AnalogType.ToString() });

                // Create equivalent derived digital definitions
                foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                    derivedCell.DigitalDefinitions.Add(new DigitalDefinition { Label = sourceDigital.Label });

                // Add cell to frame
                derivedFrame.Cells.Add(derivedCell);
            }

            derivedFrame.IsConcentrator = derivedFrame.Cells.Count > 1;

            return derivedFrame;
        }

        private IConfigurationFrame GetConfigurationFrame(string sourceData, out string connectionString)
        {
            connectionString = "";

            try
            {
                SoapFormatter formatter = new SoapFormatter
                {
                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeFormat = FormatterTypeStyle.TypesWhenNeeded,
                    Binder = Serialization.LegacyBinder
                };

                // Try deserializing input as connection settings
                ConnectionSettings connectionSettings;

                using (MemoryStream source = new MemoryStream(Encoding.UTF8.GetBytes(sourceData)))
                    connectionSettings = formatter.Deserialize(source) as ConnectionSettings;

                if (connectionSettings != null)
                {
                    // If provided input was a connection settings object, get a valid connection string
                    connectionString = connectionSettings.ConnectionString;

                    Dictionary<string, string> connectionStringKeyValues = connectionString.ParseKeyValuePairs();

                    connectionString = "transportProtocol=" + connectionSettings.TransportProtocol + ";" + connectionStringKeyValues.JoinKeyValuePairs();

                    if (connectionSettings.ConnectionParameters != null)
                    {
                        switch (connectionSettings.PhasorProtocol)
                        {
                            case PhasorProtocol.BPAPDCstream:
                                if (connectionSettings.ConnectionParameters is GSF.PhasorProtocols.BPAPDCstream.ConnectionParameters bpaParameters)
                                    connectionString += "; iniFileName=" + bpaParameters.ConfigurationFileName + "; refreshConfigFileOnChange=" + bpaParameters.RefreshConfigurationFileOnChange + "; parseWordCountFromByte=" + bpaParameters.ParseWordCountFromByte;
                                break;
                            case PhasorProtocol.FNET:
                                if (connectionSettings.ConnectionParameters is GSF.PhasorProtocols.FNET.ConnectionParameters fnetParameters)
                                    connectionString += "; timeOffset=" + fnetParameters.TimeOffset + "; stationName=" + fnetParameters.StationName + "; frameRate=" + fnetParameters.FrameRate + "; nominalFrequency=" + (int)fnetParameters.NominalFrequency;
                                break;
                            case PhasorProtocol.SelFastMessage:
                                if (connectionSettings.ConnectionParameters is GSF.PhasorProtocols.SelFastMessage.ConnectionParameters selParameters)
                                    connectionString += "; messagePeriod=" + selParameters.MessagePeriod;
                                break;
                            case PhasorProtocol.IEC61850_90_5:
                                if (connectionSettings.ConnectionParameters is GSF.PhasorProtocols.IEC61850_90_5.ConnectionParameters iecParameters)
                                    connectionString += "; useETRConfiguration=" + iecParameters.UseETRConfiguration + "; guessConfiguration=" + iecParameters.GuessConfiguration + "; parseRedundantASDUs=" + iecParameters.ParseRedundantASDUs + "; ignoreSignatureValidationFailures=" + iecParameters.IgnoreSignatureValidationFailures + "; ignoreSampleSizeValidationFailures=" + iecParameters.IgnoreSampleSizeValidationFailures;
                                break;
                            case PhasorProtocol.Macrodyne:
                                if (connectionSettings.ConnectionParameters is GSF.PhasorProtocols.Macrodyne.ConnectionParameters macrodyneParameters)
                                    connectionString += "; protocolVersion=" + macrodyneParameters.ProtocolVersion + "; iniFileName=" + macrodyneParameters.ConfigurationFileName + "; refreshConfigFileOnChange=" + macrodyneParameters.RefreshConfigurationFileOnChange + "; deviceLabel=" + macrodyneParameters.DeviceLabel;
                                break;
                        }
                    }

                    connectionString += "; accessID=" + connectionSettings.PmuID;
                    connectionString += "; phasorProtocol=" + connectionSettings.PhasorProtocol;

                    // Parse connection string and return retrieved configuration frame
                    return RequestConfigurationFrame(connectionString);
                }

                // Try deserializing input as a configuration frame
                IConfigurationFrame configurationFrame;

                using (MemoryStream source = new MemoryStream(Encoding.UTF8.GetBytes(sourceData)))
                    configurationFrame = formatter.Deserialize(source) as IConfigurationFrame;

                if (configurationFrame != null)
                    return configurationFrame;

                // Finally, assume input is simply a connection string and attempt to return retrieved configuration frame
                return RequestConfigurationFrame(sourceData);
            }
            catch
            {
                return new ConfigurationErrorFrame();
            }
        }

        private IConfigurationFrame RequestConfigurationFrame(string connectionString)
        {
            using (CommonPhasorServices phasorServices = new CommonPhasorServices())
            {
                phasorServices.StatusMessage += (sender, e) => LogStatusMessage(e.Argument.Replace("**", ""));
                phasorServices.ProcessException += (sender, e) => LogException(e.Argument);
                return phasorServices.RequestDeviceConfiguration(connectionString);
            }
        }

        public IEnumerable<string> GetTemplateTypes()
        {
            List<string> templateTypes = new List<string>(FilePath.GetFileList(FilePath.GetAbsolutePath("*.TagTemplate")).Select(FilePath.GetFileNameWithoutExtension));
            templateTypes.Insert(0, "None: Save Mapping Only - No Calculations");
            return templateTypes;
        }

        public IEnumerable<TagTemplate> LoadTemplate(string templateType)
        {
            List<TagTemplate> tagTemplates = new List<TagTemplate>();
 
            foreach (string line in File.ReadLines(FilePath.GetAbsolutePath($"{templateType}.TagTemplate")))
            {
                // Skip comment lines
                if (line.TrimStart().StartsWith("#"))
                    continue;

                string[] parts = line.Split('\t');

                if (parts.Length == 5)
                {
                    tagTemplates.Add(new TagTemplate
                    {
                        TagName = parts[0].Trim(),
                        Inputs = parts[1].Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(input => input.Trim()).ToArray(),
                        Equation = parts[2].Trim(),
                        Type = parts[3].Trim(),
                        Description = parts[4].Trim()
                    });
                }
            }

            return tagTemplates;
        }

        public int GetProtocolID(string protocolAcronym)
        {
            if (Enum.TryParse(protocolAcronym, true, out PhasorProtocol protocol))
                return DataContext.Table<Protocol>().QueryRecordWhere("Acronym = {0}", protocol.ToString())?.ID ??
                       DataContext.Table<Protocol>().QueryRecordWhere("Acronym = {0}", GetOldProtocolName(protocol))?.ID ?? 0;

            return 0;
        }

        private static string GetOldProtocolName(PhasorProtocol protocol)
        {
            switch (protocol)
            {
                case PhasorProtocol.IEEEC37_118V2:
                    return "IeeeC37_118V2";
                case PhasorProtocol.IEEEC37_118V1:
                    return "IeeeC37_118V1";
                case PhasorProtocol.IEEEC37_118D6:
                    return "IeeeC37_118D6";
                case PhasorProtocol.IEEE1344:
                    return "Ieee1344";
                case PhasorProtocol.BPAPDCstream:
                    return "BpaPdcStream";
                case PhasorProtocol.FNET:
                    return "FNet";
                case PhasorProtocol.SelFastMessage:
                    return "SelFastMessage";
                case PhasorProtocol.Macrodyne:
                    return "Macrodyne";
                case PhasorProtocol.IEC61850_90_5:
                    return "Iec61850_90_5";
                default:
                    return protocol.ToString();
            }
        }

        public string GetProtocolCategory(int protocolID)
        {
            return DataContext.Table<Protocol>().QueryRecordWhere("ID = {0}", protocolID).Category;
        }

        public IEnumerable<SynchrophasorProtocol> GetSynchrophasorProtocols()
        {
            return Enum.GetValues(typeof(PhasorProtocol)).Cast<PhasorProtocol>().Select(protocol => new SynchrophasorProtocol
            {
                Acronym = protocol.ToString(),
                Name = protocol.GetFormattedProtocolName()
            });
        }

        public void ValidateCalculatorConfigurations(int? historianID, string systemName)
        {
            const int Avg = 0, Max = 1, Min = 2;
            PowerCalculationConfigurationValidation.ValidateDatabaseDefinitions();

            TableOperations<Measurement> measurementTable = DataContext.Table<Measurement>();
            string frequencyDeviceName = string.Format(SystemFrequencyDeviceName, systemName);

            // Look for existing frequency average
            if (measurementTable.QueryRecordCountWhere($"SignalReference = '{SignalReference.ToString(frequencyDeviceName, SignalKind.Frequency)}'") > 0)
                return;

            TableOperations<CustomActionAdapter> customActionAdapterTable = DataContext.Table<CustomActionAdapter>();
            CustomActionAdapter avgFreqAdapter = customActionAdapterTable.QueryRecordWhere("TypeName = {0}", typeof(PowerCalculations.AverageFrequency).FullName) ?? NewCustomActionAdapter();
            Measurement[] measurements = GetCalculatedFrequencyMeasurements(historianID, systemName, frequencyDeviceName);

            double lagTime = DefaultCalculationLagTime;

            // Reduce lag-time since dynamic calculations can depend on average frequency
            lagTime -= lagTime > 1.0 ? 1.0 : 0.5;

            if (lagTime < 0.1)
                lagTime = 0.1;

            avgFreqAdapter.AdapterName = "PHASOR!AVERAGEFREQ";
            avgFreqAdapter.AssemblyName = "PowerCalculations.dll";
            avgFreqAdapter.TypeName = typeof(PowerCalculations.AverageFrequency).FullName;
            avgFreqAdapter.ConnectionString = $"InputMeasurementKeys={{FILTER ActiveMeasurements WHERE SignalType = 'FREQ' AND SignalReference NOT LIKE '{frequencyDeviceName}%'}}; OutputMeasurements={{{measurements[Avg].SignalID};{measurements[Max].SignalID};{measurements[Min].SignalID}}}; LagTime={lagTime}; LeadTime={DefaultCalculationLeadTime}; FramesPerSecond={DefaultCalculationFramesPerSecond}";
            avgFreqAdapter.Enabled = true;

            customActionAdapterTable.AddNewOrUpdateRecord(avgFreqAdapter);            
        }

        private Measurement[] GetCalculatedFrequencyMeasurements(int? historianID, string systemName, string frequencyDeviceName)
        {
            SignalType freqSignalType = DataContext.Table<SignalType>().QueryRecordWhere("Acronym = 'FREQ'");

            if (freqSignalType.ID == 0)
                throw new InvalidOperationException("Failed to find 'FREQ' signal type");

            Device freqDevice = QueryDevice(frequencyDeviceName);

            freqDevice.Acronym = frequencyDeviceName;
            freqDevice.Name = $"Calculated System Frequency Statistics Virtual Device for {systemName}";
            freqDevice.IsConcentrator = false;
            freqDevice.HistorianID = historianID;
            freqDevice.ProtocolID = VirtualProtocolID;
            freqDevice.Enabled  = true;

            AddNewOrUpdateDevice(freqDevice);
            
            freqDevice = QueryDevice(frequencyDeviceName);

            // Signal references within a device are used to map frequencies back into a frame of data - since frames are only
            // designated to have a single frequency measurement, the max and min frequencies are marked as analog values
            string avgFreqSignalRef = SignalReference.ToString(frequencyDeviceName, SignalKind.Frequency);
            string maxFreqSignalRef = SignalReference.ToString(frequencyDeviceName, SignalKind.Analog, 1);
            string minFreqSignalRef = SignalReference.ToString(frequencyDeviceName, SignalKind.Analog, 2);

            Measurement avgFreqMeasurement = QueryMeasurement(avgFreqSignalRef);
            Measurement maxFreqMeasurement = QueryMeasurement(maxFreqSignalRef);
            Measurement minFreqMeasurement = QueryMeasurement(minFreqSignalRef);

            avgFreqMeasurement.PointTag = $"{frequencyDeviceName}-AVG-FQ";
            avgFreqMeasurement.SignalReference = avgFreqSignalRef;
            avgFreqMeasurement.SignalTypeID = freqSignalType.ID;
            avgFreqMeasurement.DeviceID = freqDevice.ID;
            avgFreqMeasurement.HistorianID = historianID;
            avgFreqMeasurement.Description = $"{systemName} Average System Frequency";
            avgFreqMeasurement.Internal = true;
            avgFreqMeasurement.Enabled = true;

            maxFreqMeasurement.PointTag = $"{frequencyDeviceName}-MAX-FQ";
            maxFreqMeasurement.SignalReference = maxFreqSignalRef;
            maxFreqMeasurement.SignalTypeID = freqSignalType.ID;
            maxFreqMeasurement.DeviceID = freqDevice.ID;
            maxFreqMeasurement.HistorianID = historianID;
            maxFreqMeasurement.Description = $"{systemName} Maximum System Frequency";
            maxFreqMeasurement.Internal = true;
            maxFreqMeasurement.Enabled = true;

            minFreqMeasurement.PointTag = $"{frequencyDeviceName}-MIN-FQ";
            minFreqMeasurement.SignalReference = minFreqSignalRef;
            minFreqMeasurement.SignalTypeID = freqSignalType.ID;
            minFreqMeasurement.DeviceID = freqDevice.ID;
            minFreqMeasurement.HistorianID = historianID;
            minFreqMeasurement.Description = $"{systemName} Minimum System Frequency";
            minFreqMeasurement.Internal = true;
            minFreqMeasurement.Enabled = true;

            AddNewOrUpdateMeasurement(avgFreqMeasurement);
            AddNewOrUpdateMeasurement(maxFreqMeasurement);
            AddNewOrUpdateMeasurement(minFreqMeasurement);

            Measurement[] measurements = new Measurement[3];

            // Requery frequency measurements in case they were newly added - this will retrieve autoinc / new Guids values
            measurements[0] = QueryMeasurement(avgFreqSignalRef);
            measurements[1] = QueryMeasurement(maxFreqSignalRef);
            measurements[2] = QueryMeasurement(minFreqSignalRef);

            return measurements;
        }

        #endregion
    }
}
