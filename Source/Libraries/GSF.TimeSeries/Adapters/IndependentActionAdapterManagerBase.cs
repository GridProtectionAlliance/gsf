//******************************************************************************************************
//  IndependentActionAdapterManagerBase.cs - Gbtc
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
//  02/13/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.Reflection;
using GSF.Threading;
using GSF.TimeSeries.Data;
using GSF.Units.EE;
using MeasurementRecord = GSF.TimeSeries.Model.Measurement;
using DeviceRecord = GSF.TimeSeries.Model.Device;
using static GSF.TimeSeries.Adapters.IndependentAdapterManagerExtensions;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents an adapter base class that provides functionality to manage and distribute measurements to a collection of action adapters.
    /// </summary>
    public abstract class IndependentActionAdapterManagerBase<TAdapter> : ActionAdapterCollection, IIndependentAdapterManager where TAdapter : IAdapter, new()
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="FramesPerSecond"/>.
        /// </summary>
        public const int DefaultFramesPerSecond = 30;

        /// <summary>
        /// Defines the default value for the <see cref="LagTime"/>.
        /// </summary>
        public const double DefaultLagTime = 5.0D;

        /// <summary>
        /// Defines the default value for the <see cref="LeadTime"/>.
        /// </summary>
        public const double DefaultLeadTime = 5.0D;

        // Fields
        private readonly LongSynchronizedOperation m_parseConnectionString;
        private readonly LongSynchronizedOperation m_initializeChildAdapters;
        private readonly List<MeasurementKey[]> m_inputMeasurementKeysQueue;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="IndependentActionAdapterManagerBase{TAdapter}"/>.
        /// </summary>
        protected IndependentActionAdapterManagerBase()
        {
            this.HandleConstruct();

            // Define a synchronized operation to manage parsing connection string operations sequentially
            m_parseConnectionString = new LongSynchronizedOperation(ParseConnectionString, ex => OnProcessException(MessageLevel.Warning, ex, nameof(ParseConnectionString)))
            {
                IsBackground = true
            };

            m_initializeChildAdapters = new LongSynchronizedOperation(InitializeChildAdapters, ex => OnProcessException(MessageLevel.Warning, ex, nameof(InitializeChildAdapters)))
            {
                IsBackground = true
            };

            m_inputMeasurementKeysQueue = new List<MeasurementKey[]>(2);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets primary keys of input measurements for the <see cref="IndependentActionAdapterManagerBase{TAdapter}"/>.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines primary keys of input measurements the adapter expects; can be one of a filter expression, measurement key, point tag or Guid.")]
        [CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        [DefaultValue(null)]
        public override MeasurementKey[] InputMeasurementKeys
        {
            get => base.InputMeasurementKeys;
            set
            {
                base.InputMeasurementKeys = value;
                InputMeasurementKeyTypes = DataSource.GetSignalTypes(value, SourceMeasurementTable);
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the <see cref="IndependentActionAdapterManagerBase{TAdapter}"/> will produce, if any.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines primary keys of output measurements the adapter expects; can be one of a filter expression, measurement key, point tag or Guid.")]
        [CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        [DefaultValue(null)]
        public override IMeasurement[] OutputMeasurements
        {
            get => base.OutputMeasurements;
            set
            {
                base.OutputMeasurements = value;
                OutputMeasurementTypes = DataSource.GetSignalTypes(value, SourceMeasurementTable);
            }
        }

        /// <summary>
        /// Gets or sets the number of frames per second applied to each adapter.
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        [ConnectionStringParameter]
        [Description("Defines the number of frames per second applied to each adapter.")]
        [DefaultValue(DefaultFramesPerSecond)]
        public virtual int FramesPerSecond { get; set; } = DefaultFramesPerSecond;

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second) applied to each adapter.
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        [ConnectionStringParameter]
        [Description("Defines the allowed past time deviation tolerance, in seconds (can be sub-second) applied to each adapter.")]
        [DefaultValue(DefaultLagTime)]
        public virtual double LagTime { get; set; } = DefaultLagTime;

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second) applied to each adapter.
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        [ConnectionStringParameter]
        [Description("Defines the allowed future time deviation tolerance, in seconds (can be sub-second) applied to each adapter.")]
        [DefaultValue(DefaultLeadTime)]
        public virtual double LeadTime { get; set; } = DefaultLeadTime;

        /// <summary>
        /// Gets or sets the wait timeout, in milliseconds, that system wait for system configuration reload to complete.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the wait timeout, in milliseconds, that system wait for system configuration reload to complete.")]
        [DefaultValue(DefaultConfigurationReloadWaitTimeout)]
        public int ConfigurationReloadWaitTimeout { get; set; } = DefaultConfigurationReloadWaitTimeout;

        /// <summary>
        /// Gets or sets the total number of attempts to wait for system configuration reloads when waiting for configuration updates to be available.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the total number of attempts to wait for system configuration reloads when waiting for configuration updates to be available.")]
        [DefaultValue(DefaultConfigurationReloadWaitAttempts)]
        public virtual int ConfigurationReloadWaitAttempts { get; set; } = DefaultConfigurationReloadWaitAttempts;

        /// <summary>
        /// Gets or sets the connection string used for database operations. Leave blank to use local configuration database defined in "systemSettings".
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the connection string used for database operations. Leave blank to use local configuration database defined in \"systemSettings\".")]
        [DefaultValue(DefaultDatabaseConnectionString)]
        public virtual string DatabaseConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the provider string used for database operations. Defaults to a SQL Server provider string.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the provider string used for database operations. Defaults to a SQL Server provider string.")]
        [DefaultValue(DefaultDatabaseProviderString)]
        public virtual string DatabaseProviderString { get; set; }

        /// <summary>
        /// Gets or sets template for output measurement point tag names.
        /// </summary>
        [ConnectionStringParameter]
        [Description($"Defines template for output measurement point tag names, typically an expression like \"{DefaultPointTagTemplate}\" where \"{{0}}\" is substituted with this adapter name, a dash and then the PerAdapterOutputNames value for the current measurement. Note that \"{{0}}\" token is not required, property can be overridden to provide desired value.")]
        [DefaultValue(DefaultPointTagTemplate)]
        public virtual string PointTagTemplate { get; set; } = DefaultPointTagTemplate;

        /// <summary>
        /// Gets or sets template for output measurement alternate tag names.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines template for output measurement alternate tag names, typically an expression where \"{0}\" is substituted with this adapter name, a dash and then the PerAdapterOutputNames value for the current measurement. Note that \"{0}\" token is not required, property can be overridden to provide desired value.")]
        [DefaultValue(DefaultAlternateTagTemplate)]
        public virtual string AlternateTagTemplate { get; set; } = DefaultAlternateTagTemplate;

        /// <summary>
        /// Gets or sets template for output measurement signal reference names.
        /// </summary>
        [ConnectionStringParameter]
        [Description($"Defines template for output measurement signal reference names, typically an expression like \"{DefaultSignalReferenceTemplate}\" where \"{{0}}\" is substituted with this adapter name, a dash and then the PerAdapterOutputNames value for the current measurement. Note that \"{{0}}\" token is not required, property can be overridden to provide desired value.")]
        [DefaultValue(DefaultSignalReferenceTemplate)]
        public virtual string SignalReferenceTemplate { get; set; } = DefaultSignalReferenceTemplate;

        /// <summary>
        /// Gets or sets template for output measurement descriptions.
        /// </summary>
        [ConnectionStringParameter]
        [Description($"Defines template for output measurement descriptions, typically an expression like \"{DefaultDescriptionTemplate}\".")]
        [DefaultValue(DefaultDescriptionTemplate)]
        public virtual string DescriptionTemplate { get; set; } = DefaultDescriptionTemplate;

        /// <summary>
        /// Gets or sets template for the parent device acronym used to group associated output measurements.
        /// </summary>
        [ConnectionStringParameter]
        [Description($"Defines template for the parent device acronym used to group associated output measurements, typically an expression like \"{DefaultParentDeviceAcronymTemplate}\" where \"{{0}}\" is substituted with this adapter name. Set to blank value to create no parent device associated output measurements. Note that \"{{0}}\" token is not required, you can simply use a specific device acronym.")]
        [DefaultValue(DefaultParentDeviceAcronymTemplate)]
        public virtual string ParentDeviceAcronymTemplate { get; set; } = DefaultParentDeviceAcronymTemplate;

        /// <summary>
        /// Gets or sets default signal type to use for all output measurements when <see cref="SignalTypes"/> array is not defined.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the default signal type to use for all output measurements. Used when per output measurement SignalTypes array is not defined.")]
        [DefaultValue(typeof(SignalType), DefaultSignalType)]
        public virtual SignalType SignalType { get; set; } = (SignalType)Enum.Parse(typeof(SignalType), DefaultSignalType);

        /// <summary>
        /// Gets per adapter signal type for output measurements, used when each output needs to be a different type.
        /// </summary>
        public virtual SignalType[] SignalTypes { get; } = null;

        /// <summary>
        /// Gets any custom adapter settings to be added to each adapter connection string. Can be used to add
        /// settings that are custom per adapter.
        /// </summary>
        public virtual string CustomAdapterSettings { get; } = null;

        /// <summary>
        /// Gets or sets the target historian acronym for output measurements.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the target historian acronym for output measurements.")]
        [DefaultValue(DefaultTargetHistorianAcronym)]
        public virtual string TargetHistorianAcronym { get; set; } = DefaultTargetHistorianAcronym;

        /// <summary>
        /// Gets or sets the source measurement table to use for configuration.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the source measurement table to use for configuration.")]
        [DefaultValue(DefaultSourceMeasurementTable)]
        public virtual string SourceMeasurementTable { get; set; } = DefaultSourceMeasurementTable;

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source used to load each <see cref="IAdapter"/>. Updates
        /// to this property will cascade to all adapters in this <see cref="IndependentActionAdapterManagerBase{TAdapter}"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get => base.DataSource;
            set
            {
                if (!this.DataSourceChanged(value))
                    return;

                base.DataSource = value;
                this.HandleUpdateDataSource();
            }
        }

        /// <summary>
        /// Gets number of input measurement required by each adapter. If value is not constant, use
        /// <see cref="CurrentAdapterIndex"/> to determine input count for adapter at current index.
        /// </summary>
        public abstract int PerAdapterInputCount { get; }

        /// <summary>
        /// Gets or sets the index into the per adapter input measurements to use for target adapter name.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the index into the per adapter input measurements to use for target adapter name.")]
        public virtual int InputMeasurementIndexUsedForName { get; set; }

        /// <summary>
        /// Gets output measurement names to use for each adapter.
        /// </summary>
        public abstract ReadOnlyCollection<string> PerAdapterOutputNames { get; }

        /// <summary>
        /// Gets or sets flag that determines if the <see cref="IndependentActionAdapterManagerBase{TAdapter}"/> adapter
        /// <see cref="AdapterCollectionBase{T}.ConnectionString"/> should be automatically parsed every time the
        /// <see cref="DataSource"/> is updated without requiring adapter to be reinitialized. Defaults to <c>true</c>
        /// to allow child adapters to come and go based on updates to system configuration.
        /// </summary>
        /// <remarks>
        /// When this value is <c>true</c> (the default), implementors must call the <see cref="InitializeChildAdapterManagement"/>
        /// method an overridden <see cref="ParseConnectionString"/> method.
        /// </remarks>
        protected virtual bool AutoReparseConnectionString { get; set; } = true;

        /// <summary>
        /// Gets input measurement <see cref="SignalType"/>'s for each of the <see cref="ActionAdapterBase.InputMeasurementKeys"/>, if any.
        /// </summary>
        public virtual SignalType[] InputMeasurementKeyTypes { get; private set; }

        /// <summary>
        /// Gets output measurement <see cref="SignalType"/>'s for each of the <see cref="ActionAdapterBase.OutputMeasurements"/>, if any.
        /// </summary>
        public virtual SignalType[] OutputMeasurementTypes { get; private set; }

        /// <summary>
        /// Gets or sets current adapter ID counter.
        /// </summary>
        public uint AdapterIDCounter { get; set; }

        /// <summary>
        /// Gets adapter index currently being processed.
        /// </summary>
        public int CurrentAdapterIndex { get; protected set; }

        /// <summary>
        /// Gets adapter output index currently being processed.
        /// </summary>
        public int CurrentOutputIndex { get; protected set; }

        /// <summary>
        /// Gets associated device ID for <see cref="CurrentAdapterIndex"/>, if any, for measurement generation. If overridden to provide custom
        /// device ID, <see cref="ParentDeviceAcronymTemplate"/> should be set to <c>null</c> so no parent device is created.
        /// </summary>
        public virtual int CurrentDeviceID { get; } = 0;

        /// <summary>
        /// Returns the detailed status of the <see cref="IndependentActionAdapterManagerBase{TAdapter}"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new();

                status.AppendLine($"         Frames Per Second: {FramesPerSecond:N0}");
                status.AppendLine($"      Lag Time / Lead Time: {LagTime:N3} / {LeadTime:N3}");
                status.Append(this.HandleStatus());
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="IndependentActionAdapterManagerBase{TAdapter}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                this.HandleDispose();
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Initializes the <see cref="IndependentActionAdapterManagerBase{TAdapter}" />.
        /// </summary>
        public override void Initialize() => this.HandleInitialize();

        /// <summary>
        /// Initializes management operations for child adapters based on inputs. Derived classes should override
        /// <see cref="ParseConnectionString"/> and call this method when <see cref="AutoReparseConnectionString"/> is
        /// <c>true</c> (the default).
        /// </summary>
        /// <param name="inputMeasurementKeys">Input measurement keys to use for initialization.</param>
        /// <remarks>
        /// When <see cref="AutoReparseConnectionString"/> is <c>true</c> (the default), implementors must override
        /// the <see cref="ParseConnectionString"/> method and at least call this method.
        /// </remarks>
        protected virtual void InitializeChildAdapterManagement(MeasurementKey[] inputMeasurementKeys)
        {
            lock (m_inputMeasurementKeysQueue)
            {
                if (m_inputMeasurementKeysQueue.Count < 2)
                    m_inputMeasurementKeysQueue.Add(inputMeasurementKeys);
                else
                    m_inputMeasurementKeysQueue[1] = inputMeasurementKeys;

                m_initializeChildAdapters.RunOnceAsync();
            }
        }

        private void InitializeChildAdapters()
        {
            MeasurementKey[] inputMeasurementKeys;

            lock (m_inputMeasurementKeysQueue)
            {
                if (m_inputMeasurementKeysQueue.Count == 0)
                    return;

                inputMeasurementKeys = m_inputMeasurementKeysQueue[0];
                m_inputMeasurementKeysQueue.RemoveAt(0);
            }

            try
            {
                // If no inputs are defined, skip setup
                if (inputMeasurementKeys.Length == 0)
                {
                    OnStatusMessage(MessageLevel.Warning, "No inputs were configured. Cannot initialize child adapters.");
                    return;
                }

                ReadOnlyCollection<string> perAdapterOutputNames = PerAdapterOutputNames;
                
                CurrentAdapterIndex = 0;
                int inputsPerAdapter = PerAdapterInputCount;
                
                int outputsPerAdapter = perAdapterOutputNames?.Count ?? 0;

                if (perAdapterOutputNames is null || inputsPerAdapter <= 0 || outputsPerAdapter <= 0)
                {
                    OnStatusMessage(MessageLevel.Warning, "No PerAdapterInputCount or PerAdapterOutputNames where defined. Cannot initialize child adapters.");
                    return;
                }

                int nameIndex = InputMeasurementIndexUsedForName;
                int? currentDeviceID = null;

                // Create associated parent device for output measurements if parent device acronym template is defined
                if (!string.IsNullOrWhiteSpace(ParentDeviceAcronymTemplate))
                {
                    if (CurrentDeviceID > 0)
                        OnStatusMessage(MessageLevel.Warning, $"WARNING: Creating a parent device for \"{Name}\" [{GetType().Name}] based on specified template \"{ParentDeviceAcronymTemplate}\", but overridden CurrentDeviceID property reports non-zero value: {CurrentDeviceID:N0}");

                    using AdoDataConnection connection = GetConfiguredConnection();

                    TableOperations<DeviceRecord> deviceTable = new(connection);
                    string deviceAcronym = string.Format(ParentDeviceAcronymTemplate, Name);

                    DeviceRecord device = deviceTable.QueryRecordWhere("Acronym = {0}", deviceAcronym) ?? deviceTable.NewRecord();
                    int protocolID = connection.ExecuteScalar<int?>("SELECT ID FROM Protocol WHERE Acronym = 'VirtualInput'") ?? 15;

                    device.Acronym = deviceAcronym;
                    device.Name = deviceAcronym;
                    device.ProtocolID = protocolID;
                    device.Enabled = true;

                    deviceTable.AddNewOrUpdateRecord(device);
                    currentDeviceID = deviceTable.QueryRecordWhere("Acronym = {0}", deviceAcronym)?.ID;
                }

                HashSet<string> activeAdapterNames = new(StringComparer.Ordinal);
                List<TAdapter> adapters = new();
                HashSet<Guid> signalIDs = new();

                // Create settings dictionary for connection string to use with primary child adapters
                Dictionary<string, string> settings = new(StringComparer.OrdinalIgnoreCase);

                foreach (PropertyInfo property in GetType().GetProperties())
                {
                    if (property.AttributeExists<PropertyInfo, ConnectionStringParameterAttribute>())
                        settings[property.Name] = $"{property.GetValue(this)}";
                }

                // Create child adapter for provided inputs to the parent bulk collection-based adapter
                for (int i = 0; i < inputMeasurementKeys.Length; i += inputsPerAdapter)
                {
                    CurrentAdapterIndex = adapters.Count;
                    inputsPerAdapter = PerAdapterInputCount;

                    Guid[] inputs = new Guid[inputsPerAdapter];
                    Guid[] outputs = new Guid[outputsPerAdapter];

                    // Adapter inputs are presumed to be grouped together
                    for (int j = 0; j < inputsPerAdapter && i + j < inputMeasurementKeys.Length; j++)
                        inputs[j] = inputMeasurementKeys[i + j].SignalID;

                    string inputName = this.LookupPointTag(inputs[nameIndex], SourceMeasurementTable);
                    string adapterName = $"{Name}!{inputName}";
                    string alternateTagPrefix = null;

                    if (!string.IsNullOrWhiteSpace(AlternateTagTemplate))
                    {
                        string deviceName = this.LookupDevice(inputs[nameIndex], SourceMeasurementTable);
                        string phasorLabel = this.LookupPhasorLabel(inputs[nameIndex], SourceMeasurementTable);
                        alternateTagPrefix = $"{deviceName}-{phasorLabel}";
                    }

                    // Track active adapter names so that adapters that no longer have sources can be removed
                    activeAdapterNames.Add(adapterName);

                    // See if child adapter already exists
                    if (this.FindAdapter(adapterName) is not null)
                        continue;

                    // Setup output measurements for new child adapter
                    for (int j = 0; j < outputsPerAdapter; j++)
                    {
                        CurrentOutputIndex = j;

                        string perAdapterOutputName = perAdapterOutputNames[j].ToUpper();
                        string outputPrefix = $"{adapterName}-{perAdapterOutputName}";
                        string outputPointTag = string.Format(PointTagTemplate, outputPrefix);
                        string outputAlternateTag = string.Format(AlternateTagTemplate, alternateTagPrefix is null ? "" : $"{alternateTagPrefix}-{perAdapterOutputName}");
                        string signalReference = string.Format(SignalReferenceTemplate, outputPrefix);
                        SignalType signalType = SignalTypes?[j] ?? SignalType;
                        string description = string.Format(DescriptionTemplate, outputPrefix, signalType, Name, GetType().Name);

                        // Get output measurement record, creating a new one if needed
                        MeasurementRecord measurement = this.GetMeasurementRecord(currentDeviceID ?? CurrentDeviceID, outputPointTag, outputAlternateTag, signalReference, description, signalType, TargetHistorianAcronym);

                        // Track output signal IDs
                        signalIDs.Add(measurement.SignalID);
                        outputs[j] = measurement.SignalID;
                    }

                    // Add inputs and outputs to connection string settings for child adapter
                    settings[nameof(InputMeasurementKeys)] = string.Join(";", inputs);
                    settings[nameof(OutputMeasurements)] = string.Join(";", outputs);

                    string connectionString = settings.JoinKeyValuePairs();

                    if (!string.IsNullOrWhiteSpace(CustomAdapterSettings))
                        connectionString = $"{connectionString}; {CustomAdapterSettings}";

                    adapters.Add(new TAdapter
                    {
                        Name = adapterName,
                        ID = AdapterIDCounter++,
                        ConnectionString = connectionString,
                        DataSource = DataSource
                    });
                }

                // Check for adapters that are no longer referenced and need to be removed
                IActionAdapter[] adaptersToRemove = this.Where<IActionAdapter>(adapter => !activeAdapterNames.Contains(adapter.Name)).ToArray();

                foreach (IActionAdapter adapter in adaptersToRemove)
                    Remove(adapter);

                // Host system was notified about configuration changes, i.e., new or updated output measurements.
                // Before initializing child adapters, we wait for this process to complete.
                this.WaitForSignalsToLoad(signalIDs.ToArray(), SourceMeasurementTable);

                // Add new adapters to parent bulk adapter collection, this will auto-initialize each child adapter
                foreach (TAdapter adapter in adapters)
                    Add(adapter as IActionAdapter);

                RecalculateRoutingTables();
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"Failed while initializing child adapters: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Validates that an even number of inputs are provided for specified <see cref="PerAdapterInputCount"/>.
        /// </summary>
        protected void ValidateEvenInputCount() => this.HandleValidateEvenInputCount();

        /// <summary>
        /// Parses connection string. Derived classes should override for custom connection string parsing and call the
        /// <see cref="InitializeChildAdapterManagement"/> method when <see cref="AutoReparseConnectionString"/> is
        /// <c>true</c> (the default).
        /// </summary>
        /// <remarks>
        /// When <see cref="AutoReparseConnectionString"/> is <c>true</c> (the default), implementors must override
        /// this method and at least call the <see cref="InitializeChildAdapterManagement"/> method.
        /// </remarks>
        public virtual void ParseConnectionString()
        {
            this.HandleParseConnectionString();

            if (FramesPerSecond < 1)
                FramesPerSecond = DefaultFramesPerSecond;

            if (LagTime < 0.0D)
                LagTime = DefaultLagTime;

            if (LeadTime < 0.0D)
                LeadTime = DefaultLeadTime;
        }

        /// <summary>
        /// Notifies derived classes that configuration has been reloaded
        /// </summary>
        public virtual void ConfigurationReloaded()
        {
            if (AutoReparseConnectionString || !Initialized)
                return;

            InitializeChildAdapterManagement(InputMeasurementKeys);
        }

        /// <summary>
        /// Recalculates routing tables.
        /// </summary>
        public virtual void RecalculateRoutingTables() => 
            this.HandleRecalculateRoutingTables();

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IActionAdapter"/> connected to this <see cref="IndependentActionAdapterManagerBase{TAdapter}"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements) => 
            this.HandleQueueMeasurementsForProcessing(measurements);

        /// <summary>
        /// Gets a short one-line status of this <see cref="IndependentActionAdapterManagerBase{TAdapter}"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of the <see cref="IndependentActionAdapterManagerBase{TAdapter}"/>.</returns>
        public override string GetShortStatus(int maxLength) => 
            this.HandleGetShortStatus(maxLength);

        /// <summary>
        /// Enumerates child adapters.
        /// </summary>
        [AdapterCommand("Enumerates child adapters.")]
        public virtual void EnumerateAdapters() => 
            this.HandleEnumerateAdapters();

        /// <summary>
        /// Gets subscriber information for specified client connection.
        /// </summary>
        /// <param name="adapterIndex">Enumerated index for child adapter.</param>
        /// <returns>Status for adapter with specified <paramref name="adapterIndex"/>.</returns>
        [AdapterCommand("Gets subscriber information for specified client connection.")]
        public virtual string GetAdapterStatus(int adapterIndex) => 
            this.HandleGetAdapterStatus(adapterIndex);
        
        /// <summary>
        /// Gets configured database connection.
        /// </summary>
        /// <returns>New ADO data connection based on configured settings.</returns>
        public AdoDataConnection GetConfiguredConnection() => 
            this.HandleGetConfiguredConnection();

        #endregion

        #region [ IIndependentAdapterManager Implementation ]

        RoutingTables IIndependentAdapterManager.RoutingTables { get; set; }

        string IIndependentAdapterManager.OriginalDataMember { get; set; }

        ManualResetEventSlim IIndependentAdapterManager.ConfigurationReloadedWaitHandle { get; set; }

        bool IIndependentAdapterManager.AutoReparseConnectionString { get => AutoReparseConnectionString; set => AutoReparseConnectionString = value; }

        void IIndependentAdapterManager.OnConfigurationChanged() => OnConfigurationChanged();

        void IIndependentAdapterManager.OnInputMeasurementKeysUpdated() => OnInputMeasurementKeysUpdated();

        void IIndependentAdapterManager.OnStatusMessage(MessageLevel level, string status, string eventName, MessageFlags flags) => OnStatusMessage(level, status, eventName, flags);

        void IIndependentAdapterManager.OnProcessException(MessageLevel level, Exception exception, string eventName, MessageFlags flags) => OnProcessException(level, exception, eventName, flags);

        void IIndependentAdapterManager.ParseConnectionString() => m_parseConnectionString.RunOnceAsync();

        #endregion
    }
}
