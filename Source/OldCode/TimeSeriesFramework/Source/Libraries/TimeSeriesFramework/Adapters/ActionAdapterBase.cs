//******************************************************************************************************
//  ActionAdapterBase.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  02/08/2011 - J. Ritchie Carroll
//       Added invokable ExamineQueueState method to analyze real-time concentrator frame queue state.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Threading;
using TVA;

namespace TimeSeriesFramework.Adapters
{
    /// <summary>
    /// Represents the base class for action adapters.
    /// </summary>
    /// <remarks>
    /// This base class acts as a measurement concentrator which time aligns all incoming measurements for proper processing.
    /// Derived classes are expected to override <see cref="ConcentratorBase.PublishFrame"/> to handle time aligned measurements
    /// and call <see cref="OnNewMeasurements"/> for any new measurements that may get created.
    /// </remarks>
    public abstract class ActionAdapterBase : ConcentratorBase, IActionAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when <see cref="InputMeasurementKeys"/> are updated.
        /// </summary>
        public event EventHandler InputMeasurementKeysUpdated;

        /// <summary>
        /// Event is raised when <see cref="OutputMeasurements"/> are updated.
        /// </summary>
        public event EventHandler OutputMeasurementsUpdated;

        /// <summary>
        /// Provides new measurements from action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        // Fields
        private string m_name;
        private uint m_id;
        private string m_connectionString;
        private IAdapterCollection m_parent;
        private Dictionary<string, string> m_settings;
        private DataSet m_dataSource;
        private int m_initializationTimeout;
        private bool m_autoStart;
        private bool m_processMeasurementFilter;
        private MeasurementKey[] m_inputMeasurementKeys;
        private List<MeasurementKey> m_inputMeasurementKeysHash;
        private IMeasurement[] m_outputMeasurements;
        private MeasurementKey[] m_requestedInputMeasurementKeys;
        private MeasurementKey[] m_requestedOutputMeasurementKeys;
        private List<string> m_inputSourceIDs;
        private List<string> m_outputSourceIDs;
        private int m_minimumMeasurementsToUse;
        private ManualResetEvent m_initializeWaitHandle;
        private DateTime m_startTimeConstraint;
        private DateTime m_stopTimeConstraint;
        private int m_hashCode;
        private bool m_initialized;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ActionAdapterBase"/>.
        /// </summary>
        protected ActionAdapterBase()
            : base()
        {
            m_name = this.GetType().Name;
            m_settings = new Dictionary<string, string>();
            m_startTimeConstraint = DateTime.MinValue;
            m_stopTimeConstraint = DateTime.MaxValue;
            GenHashCode();

            // Create wait handle to use for adapter initialization
            m_initializeWaitHandle = new ManualResetEvent(false);
            m_initializationTimeout = AdapterBase.DefaultInitializationTimeout;

            // For most implementations millisecond resolution will be sufficient
            base.TimeResolution = Ticks.PerMillisecond;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets key/value pair connection information specific to action adapter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example connection string using manually defined measurements:<br/>
        /// <c>framesPerSecond=30; lagTime=1.0; leadTime=0.5; minimumMeasurementsToUse=-1;
        /// useLocalClockAsRealTime=true; allowSortsByArrival=false;</c><br/>
        /// <c>inputMeasurementKeys={P1:1245;P1:1247;P2:1335};</c><br/>
        /// <c>outputMeasurements={P3:1345,60.0,1.0;P3:1346;P3:1347}</c><br/>
        /// When defined manually, elements in key:<br/>
        /// * inputMeasurementKeys are defined as "ArchiveSource:PointID"<br/>
        /// * outputMeasurements are defined as "ArchiveSource:PointID,Adder,Multiplier", the adder and multiplier are optional
        /// defaulting to 0.0 and 1.0 respectively.
        /// <br/>
        /// </para>
        /// <para>
        /// Example connection string using measurements defined in a <see cref="DataSource"/> table:<br/>
        /// <c>framesPerSecond=30; lagTime=1.0; leadTime=0.5; minimumMeasurementsToUse=-1;
        /// useLocalClockAsRealTime=true; allowSortsByArrival=false;</c><br/>
        /// <c>inputMeasurementKeys={FILTER ActiveMeasurements WHERE Company='TVA' AND SignalType='FREQ' ORDER BY ID};</c><br/>
        /// <c>outputMeasurements={FILTER ActiveMeasurements WHERE SignalType IN ('IPHA','VPHA') AND Phase='+'}</c><br/>
        /// <br/>
        /// Basic filtering syntax is as follows:<br/>
        /// <br/>
        ///     {FILTER &lt;TableName&gt; WHERE &lt;Expression&gt; [ORDER BY &lt;SortField&gt;]}<br/>
        /// <br/>
        /// Source tables are expected to have at least the following fields:<br/>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Name</term>
        ///         <term>Type</term>
        ///         <description>Description.</description>
        ///     </listheader>
        ///     <item>
        ///         <term>ID</term>
        ///         <term>NVARCHAR</term>
        ///         <description>Measurement key formatted as: ArchiveSource:PointID.</description>
        ///     </item>
        ///     <item>
        ///         <term>PointTag</term>
        ///         <term>NVARCHAR</term>
        ///         <description>Point tag of measurement.</description>
        ///     </item>
        ///     <item>
        ///         <term>Adder</term>
        ///         <term>FLOAT</term>
        ///         <description>Adder to apply to value, if any (default to 0.0).</description>
        ///     </item>
        ///     <item>
        ///         <term>Multiplier</term>
        ///         <term>FLOAT</term>
        ///         <description>Multipler to apply to value, if any (default to 1.0).</description>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// Note that framesPerSecond, lagTime and leadTime are required parameters, all other parameters are optional.
        /// </para>
        /// </remarks>
        public virtual string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;

                // Preparse settings upon connection string assignment
                if (string.IsNullOrWhiteSpace(m_connectionString))
                    m_settings = new Dictionary<string, string>();
                else
                    m_settings = m_connectionString.ParseKeyValuePairs();
            }
        }

        /// <summary>
        /// Gets a read-only reference to the collection that contains this <see cref="ActionAdapterBase"/>.
        /// </summary>
        public ReadOnlyCollection<IAdapter> Parent
        {
            get
            {
                return new ReadOnlyCollection<IAdapter>(m_parent);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="ActionAdapterBase"/>.
        /// </summary>
        public virtual DataSet DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                m_dataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum time system will wait during <see cref="Start"/> for initialization.
        /// </summary>
        /// <remarks>
        /// Set to <see cref="Timeout.Infinite"/> to wait indefinitely.
        /// </remarks>
        public virtual int InitializationTimeout
        {
            get
            {
                return m_initializationTimeout;
            }
            set
            {
                m_initializationTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if adapter should automatically start or otherwise connect on demand.
        /// </summary>
        public virtual bool AutoStart
        {
            get
            {
                return m_autoStart;
            }
            set
            {
                m_autoStart = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if measurements being queued for processing should be tested to see if they are in the <see cref="InputMeasurementKeys"/>.
        /// </summary>
        public virtual bool ProcessMeasurementFilter
        {
            get
            {
                return m_processMeasurementFilter;
            }
            set
            {
                m_processMeasurementFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the action adapter expects.
        /// </summary>
        public virtual MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return m_inputMeasurementKeys;
            }
            set
            {
                m_inputMeasurementKeys = value;

                // Update input key lookup hash table
                if (value != null && value.Length > 0)
                {
                    m_inputMeasurementKeysHash = new List<MeasurementKey>(value);
                    m_inputMeasurementKeysHash.Sort();

                    // The input measurements typically define the "expected measurements" of the action adapter, so
                    // we use the number of these items to define the expected measurement count
                    ExpectedMeasurements = m_inputMeasurementKeys.Length;
                }
                else
                {
                    m_inputMeasurementKeysHash = null;
                    ExpectedMeasurements = 0;
                }

                OnInputMeasurementKeysUpdated();
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        public virtual IMeasurement[] OutputMeasurements
        {
            get
            {
                return m_outputMeasurements;
            }
            set
            {
                m_outputMeasurements = value;
                OnOutputMeasurementsUpdated();
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter input measurement keys.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of measurements based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        public virtual string[] InputSourceIDs
        {
            get
            {
                if (m_inputSourceIDs == null)
                    return null;

                return m_inputSourceIDs.ToArray();
            }
            set
            {
                if (value == null)
                {
                    m_inputSourceIDs = null;
                }
                else
                {
                    m_inputSourceIDs = new List<string>(value);
                    m_inputSourceIDs.Sort();
                }

                // Filter measurements to list of specified source IDs
                AdapterBase.LoadInputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter output measurements.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of measurements based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        public virtual string[] OutputSourceIDs
        {
            get
            {
                if (m_outputSourceIDs == null)
                    return null;

                return m_outputSourceIDs.ToArray();
            }
            set
            {
                if (value == null)
                {
                    m_outputSourceIDs = null;
                }
                else
                {
                    m_outputSourceIDs = new List<string>(value);
                    m_outputSourceIDs.Sort();
                }

                // Filter measurements to list of specified source IDs
                AdapterBase.LoadOutputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets input measurement keys that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual MeasurementKey[] RequestedInputMeasurementKeys
        {
            get
            {
                return m_requestedInputMeasurementKeys;
            }
            set
            {
                m_requestedInputMeasurementKeys = value;
            }
        }

        /// <summary>
        /// Gets or sets output measurement keys that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual MeasurementKey[] RequestedOutputMeasurementKeys
        {
            get
            {
                return m_requestedOutputMeasurementKeys;
            }
            set
            {
                m_requestedOutputMeasurementKeys = value;
            }
        }

        /// <summary>
        /// Gets the start time temporal procesing constraint defined by call to <see cref="SetTemporalConstraint"/>.
        /// </summary>
        /// <remarks>
        /// This value will be <see cref="DateTime.MinValue"/> when start time constraint is not set - meaning the adapter
        /// is processing data in real-time.
        /// </remarks>
        public virtual DateTime StartTimeConstraint
        {
            get
            {
                return m_startTimeConstraint;
            }
        }

        /// <summary>
        /// Gets the stop time temporal processing constraint defined by call to <see cref="SetTemporalConstraint"/>.
        /// </summary>
        /// <remarks>
        /// This value will be <see cref="DateTime.MaxValue"/> when stop time constraint is not set - meaning the adapter
        /// is processing data in real-time.
        /// </remarks>
        public virtual DateTime StopTimeConstraint
        {
            get
            {
                return m_stopTimeConstraint;
            }
        }

        /// <summary>
        /// Gets or sets minimum number of input measurements required for adapter.  Set to -1 to require all.
        /// </summary>
        public virtual int MinimumMeasurementsToUse
        {
            get
            {
                // Default to all measurements if minimum is not specified
                if (m_minimumMeasurementsToUse < 1 && InputMeasurementKeys != null)
                    return InputMeasurementKeys.Length;

                return m_minimumMeasurementsToUse;
            }
            set
            {
                m_minimumMeasurementsToUse = value;
            }
        }

        /// <summary>
        /// Gets name of the action adapter.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                GenHashCode();
            }
        }

        /// <summary>
        /// Gets or sets numeric ID associated with this action adapter.
        /// </summary>
        public virtual uint ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
                GenHashCode();
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if the action adapter has been initialized successfully.
        /// </summary>
        public virtual bool Initialized
        {
            get
            {
                return m_initialized;
            }
            set
            {
                m_initialized = value;

                // When initialization is complete we send notification
                if (m_initializeWaitHandle != null)
                {
                    if (value)
                        m_initializeWaitHandle.Set();
                    else
                        m_initializeWaitHandle.Reset();
                }
            }
        }

        /// <summary>
        /// Gets settings <see cref="Dictionary{TKey,TValue}"/> parsed when <see cref="ConnectionString"/> was assigned.
        /// </summary>
        public Dictionary<string, string> Settings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Returns the detailed status of the action adapter.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                const int MaxMeasurementsToShow = 10;

                StringBuilder status = new StringBuilder();
                DataSet dataSource = this.DataSource;

                status.AppendFormat("       Data source defined: {0}", (dataSource != null));
                status.AppendLine();
                if (dataSource != null)
                {
                    status.AppendFormat("    Referenced data source: {0}, {1} tables", dataSource.DataSetName, dataSource.Tables.Count);
                    status.AppendLine();
                }
                status.AppendFormat("    Initialization timeout: {0}", InitializationTimeout < 0 ? "Infinite" : InitializationTimeout.ToString() + " milliseconds");
                status.AppendLine();
                status.AppendFormat(" Using measurement routing: {0}", !ProcessMeasurementFilter);
                status.AppendLine();
                status.AppendFormat("       Adapter initialized: {0}", Initialized);
                status.AppendLine();
                status.AppendFormat("         Parent collection: {0}", m_parent == null ? "Undefined" : m_parent.Name);
                status.AppendLine();
                status.AppendFormat("         Operational state: {0}", Enabled ? "Running" : "Stopped");
                status.AppendLine();
                status.AppendFormat("         Connect on demand: {0}", !AutoStart);
                status.AppendLine();
                status.AppendFormat("    Processed measurements: {0}", ProcessedMeasurements);
                status.AppendLine();
                status.AppendFormat("    Total adapter run time: {0}", RunTime.ToString());
                status.AppendLine();
                status.AppendFormat("     Start time constraint: {0}", StartTimeConstraint == DateTime.MinValue ? "Unspecified" : StartTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                status.AppendLine();
                status.AppendFormat("      Stop time constraint: {0}", StopTimeConstraint == DateTime.MaxValue ? "Unspecified" : StopTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                status.AppendLine();
                status.AppendFormat("                Adpater ID: {0}", ID);
                status.AppendLine();

                Dictionary<string, string> keyValuePairs = Settings;
                char[] keyChars;
                string value;

                status.AppendFormat("         Connection string: {0} key/value pairs", keyValuePairs.Count);
                //                            1         2         3         4         5         6         7
                //                   123456789012345678901234567890123456789012345678901234567890123456789012345678
                //                                         Key = Value
                //                                                        1         2         3         4         5
                //                                               12345678901234567890123456789012345678901234567890
                status.AppendLine();
                status.AppendLine();

                foreach (KeyValuePair<string, string> item in keyValuePairs)
                {
                    keyChars = item.Key.Trim().ToCharArray();
                    keyChars[0] = char.ToUpper(keyChars[0]);

                    value = item.Value.Trim();
                    if (value.Length > 50)
                        value = value.TruncateRight(47) + "...";

                    status.AppendFormat("{0} = {1}", (new string(keyChars)).TruncateRight(25).PadLeft(25), value.PadRight(50));
                    status.AppendLine();
                }

                status.AppendLine();

                if (OutputMeasurements != null)
                {
                    status.AppendFormat("       Output measurements: {0} defined measurements", OutputMeasurements.Length);
                    status.AppendLine();
                    status.AppendLine();

                    for (int i = 0; i < Common.Min(OutputMeasurements.Length, MaxMeasurementsToShow); i++)
                    {
                        status.Append(OutputMeasurements[i].ToString().TruncateRight(25).PadLeft(25));
                        status.Append(" ");
                        status.AppendLine(OutputMeasurements[i].ID.ToString());
                    }

                    if (OutputMeasurements.Length > MaxMeasurementsToShow)
                        status.AppendLine("...".PadLeft(26));

                    status.AppendLine();
                }

                if (InputMeasurementKeys != null)
                {
                    status.AppendFormat("        Input measurements: {0} defined measurements", InputMeasurementKeys.Length);
                    status.AppendLine();
                    status.AppendLine();

                    for (int i = 0; i < Common.Min(InputMeasurementKeys.Length, MaxMeasurementsToShow); i++)
                    {
                        status.AppendLine(InputMeasurementKeys[i].ToString().TruncateRight(50).CenterText(50));
                    }

                    if (InputMeasurementKeys.Length > MaxMeasurementsToShow)
                        status.AppendLine("...".CenterText(50));

                    status.AppendLine();
                }

                status.AppendFormat(" Minimum measurements used: {0}", MinimumMeasurementsToUse);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ActionAdapterBase"/> object and optionally releases the managed resources.
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
                        if (m_initializeWaitHandle != null)
                            m_initializeWaitHandle.Close();

                        m_initializeWaitHandle = null;
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Initializes <see cref="ActionAdapterBase"/>.
        /// </summary>
        public virtual void Initialize()
        {
            Initialized = false;

            Dictionary<string, string> settings = Settings;
            string errorMessage = "{0} is missing from Settings - Example: framesPerSecond=30; lagTime=3; leadTime=1";
            string setting;

            // Load required parameters
            if (!settings.TryGetValue("framesPerSecond", out setting))
                throw new ArgumentException(string.Format(errorMessage, "framesPerSecond"));

            base.FramesPerSecond = int.Parse(setting);

            if (!settings.TryGetValue("lagTime", out setting))
                throw new ArgumentException(string.Format(errorMessage, "lagTime"));

            base.LagTime = double.Parse(setting);

            if (!settings.TryGetValue("leadTime", out setting))
                throw new ArgumentException(string.Format(errorMessage, "leadTime"));

            base.LeadTime = double.Parse(setting);

            // Load optional parameters
            if (settings.TryGetValue("usePrecisionTimer", out setting))
                UsePrecisionTimer = setting.ParseBoolean();

            if (settings.TryGetValue("useLocalClockAsRealTime", out setting))
                UseLocalClockAsRealTime = setting.ParseBoolean();

            if (settings.TryGetValue("ignoreBadTimestamps", out setting))
                IgnoreBadTimestamps = setting.ParseBoolean();

            if (settings.TryGetValue("allowSortsByArrival", out setting))
                AllowSortsByArrival = setting.ParseBoolean();

            if (settings.TryGetValue("inputMeasurementKeys", out setting))
                InputMeasurementKeys = AdapterBase.ParseInputMeasurementKeys(DataSource, setting);

            if (settings.TryGetValue("outputMeasurements", out setting))
                OutputMeasurements = AdapterBase.ParseOutputMeasurements(DataSource, setting);

            if (settings.TryGetValue("minimumMeasurementsToUse", out setting))
                MinimumMeasurementsToUse = int.Parse(setting);

            if (settings.TryGetValue("timeResolution", out setting))
                TimeResolution = long.Parse(setting);

            if (settings.TryGetValue("allowPreemptivePublishing", out setting))
                AllowPreemptivePublishing = setting.ParseBoolean();

            if (settings.TryGetValue("performTimestampReasonabilityCheck", out setting))
                PerformTimestampReasonabilityCheck = setting.ParseBoolean();

            if (settings.TryGetValue("processByReceivedTimestamp", out setting))
                ProcessByReceivedTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("trackPublishedTimestamp", out setting))
                TrackPublishedTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("maximumPublicationTimeout", out setting))
                MaximumPublicationTimeout = int.Parse(setting);

            if (settings.TryGetValue("downsamplingMethod", out setting))
            {
                DownsamplingMethod method;

                if (Enum.TryParse<DownsamplingMethod>(setting, true, out method))
                {
                    DownsamplingMethod = method;
                }
                else
                {
                    OnStatusMessage("WARNING: No downsampling method labeled \"{0}\" exists, \"LastReceived\" method was selected.", setting);
                    DownsamplingMethod = DownsamplingMethod.LastReceived;
                }
            }

            if (settings.TryGetValue("inputSourceIDs", out setting))
                InputSourceIDs = setting.Split(',');

            if (settings.TryGetValue("outputSourceIDs", out setting))
                OutputSourceIDs = setting.Split(',');

            if (settings.TryGetValue("connectOnDemand", out setting))
                AutoStart = !setting.ParseBoolean();
            else
                AutoStart = true;

            string startTime, stopTime, parameters;

            settings.TryGetValue("startTimeConstraint", out startTime);
            settings.TryGetValue("stopTimeConstraint", out stopTime);
            settings.TryGetValue("timeConstraintParameters", out parameters);

            SetTemporalConstraint(startTime, stopTime, parameters);

            if (settings.TryGetValue("processingInterval", out setting))
                ProcessingInterval = int.Parse(setting);
        }

        /// <summary>
        /// Starts the <see cref="ActionAdapterBase"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the action adapter or restarts it if it is already running.")]
        public override void Start()
        {
            // Make sure we are stopped (e.g., disconnected) before attempting to start (e.g., connect)
            if (Enabled)
                Stop();

            // Wait for adapter intialization to complete...
            if (WaitForInitialize(InitializationTimeout))
                base.Start();
            else
                OnProcessException(new TimeoutException("Failed to start action adapter due to timeout waiting for initialization."));
        }

        /// <summary>
        /// Stops the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Stops the action adapter.")]
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Examines the concentrator frame queue state of the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Examines concentration frame queue state.")]
        public void ExamineQueueState()
        {
            OnStatusMessage(QueueState);
        }

        /// <summary>
        /// Assigns the reference to the parent <see cref="IAdapterCollection"/> that will contain this <see cref="ActionAdapterBase"/>.
        /// </summary>
        /// <param name="parent">Parent adapter collection.</param>
        protected virtual void AssignParentCollection(IAdapterCollection parent)
        {
            m_parent = parent;
        }

        void IAdapter.AssignParentCollection(IAdapterCollection parent)
        {
            AssignParentCollection(parent);
        }

        /// <summary>
        /// Resets the statistics of the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Resets the statistics of the action adapter.")]
        public override void ResetStatistics()
        {
            base.ResetStatistics();

            if (Enabled)
                OnStatusMessage("Action adapter concentration statistics have been reset.");
        }

        /// <summary>
        /// Manually sets the intialized state of the <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="initialized">Desired initialized state.</param>
        [AdapterCommand("Manually sets the intialized state of the action adapter.")]
        public virtual void SetInitializedState(bool initialized)
        {
            this.Initialized = initialized;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="ActionAdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public virtual string GetShortStatus(int maxLength)
        {
            int inputCount = 0, outputCount = 0;

            if (InputMeasurementKeys != null)
                inputCount = InputMeasurementKeys.Length;

            if (OutputMeasurements != null)
                outputCount = OutputMeasurements.Length;

            return string.Format("Total input measurements: {0}, total output measurements: {1}", inputCount, outputCount).PadLeft(maxLength);
        }

        /// <summary>
        /// Queues a single measurement for processing. Measurements is automatically filtered to the defined <see cref="IAdapter.InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        /// <remarks>
        /// Measurement is filtered against the defined <see cref="InputMeasurementKeys"/>.
        /// </remarks>
        public virtual void QueueMeasurementForProcessing(IMeasurement measurement)
        {
            if (m_disposed)
                return;

            // If this is an input measurement to this adapter, sort it!
            if (!ProcessMeasurementFilter || IsInputMeasurement(measurement.Key))
                SortMeasurement(measurement);
        }

        /// <summary>
        /// Queues a collection of measurements for processing. Measurements are automatically filtered to the defined <see cref="IAdapter.InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// Measurements are filtered against the defined <see cref="InputMeasurementKeys"/>.
        /// </remarks>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (m_disposed)
                return;

            if (ProcessMeasurementFilter)
            {
                List<IMeasurement> inputMeasurements = new List<IMeasurement>();

                foreach (IMeasurement measurement in measurements)
                {
                    if (IsInputMeasurement(measurement.Key))
                        inputMeasurements.Add(measurement);
                }

                if (inputMeasurements.Count > 0)
                    SortMeasurements(inputMeasurements);
            }
            else
            {
                SortMeasurements(measurements);
            }
        }

        /// <summary>
        /// Determines if specified measurement key is defined in <see cref="InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="item">Primary key of measurement to find.</param>
        /// <returns>true if specified measurement key is defined in <see cref="InputMeasurementKeys"/>.</returns>
        public virtual bool IsInputMeasurement(MeasurementKey item)
        {
            if (m_inputMeasurementKeysHash != null)
                return (m_inputMeasurementKeysHash.BinarySearch(item) >= 0);

            // If no input measurements are defined we must assume user wants to accept all measurements - yikes!
            return true;
        }

        /// <summary>
        /// Blocks the <see cref="Thread.CurrentThread"/> until the action adapter is <see cref="Initialized"/>.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait.</param>
        /// <returns><c>true</c> if the initialization succeeds; otherwise, <c>false</c>.</returns>
        public virtual bool WaitForInitialize(int timeout)
        {
            if (m_initializeWaitHandle != null)
                return m_initializeWaitHandle.WaitOne(timeout);

            return false;
        }

        /// <summary>
        /// Defines a temporal processing constraint for the adapter.
        /// </summary>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint.</param>
        /// <remarks>
        /// <para>
        /// This method defines a temporal processing contraint for an adapter, i.e., the start and stop time over which an
        /// adapter will process data. Actual implementation of the constraint will be adapter specific. Implementations
        /// should be able to dynamically handle multitple calls to this function with new constraints. Passing in <c>null</c>
        /// for the <paramref name="startTime"/> and <paramref name="stopTime"/> should cancel the temporal constraint and
        /// return the adapter to standard / real-time operation.
        /// </para>
        /// <para>
        /// The <paramref name="startTime"/> and <paramref name="stopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        [AdapterCommand("Defines a temporal processing constraint for the adapter.")]
        public virtual void SetTemporalConstraint(string startTime, string stopTime, string constraintParameters)
        {
            if (!string.IsNullOrWhiteSpace(startTime))
                m_startTimeConstraint = AdapterBase.ParseTimeTag(startTime);
            else
                m_startTimeConstraint = DateTime.MinValue;

            if (!string.IsNullOrWhiteSpace(stopTime))
                m_stopTimeConstraint = AdapterBase.ParseTimeTag(stopTime);
            else
                m_stopTimeConstraint = DateTime.MaxValue;
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="ActionAdapterBase"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ActionAdapterBase"/>.</returns>
        public override int GetHashCode()
        {
            return m_hashCode;
        }

        /// <summary>
        /// Attempts to retrieve the minimum needed number of measurements from the frame (as specified by MinimumMeasurementsToUse)
        /// </summary>
        /// <param name="frame">Source frame for the measurements</param>
        /// <param name="measurements">Return array of measurements</param>
        /// <returns>True if minimum needed number of measurements were returned in measurement array</returns>
        /// <remarks>
        /// <para>
        /// Remember this function will *only* return the minimum needed number of measurements, no more.  If you want to use
        /// all available measurements in your adapter you should just use Frame.Measurements.Values directly.
        /// </para>
        /// <para>
        /// Note that the measurements array parameter will be created if the reference is null, otherwise if caller creates
        /// array it must be sized to MinimumMeasurementsToUse
        /// </para>
        /// </remarks>
        protected virtual bool TryGetMinimumNeededMeasurements(IFrame frame, ref IMeasurement[] measurements)
        {
            int index = 0, minNeeded = MinimumMeasurementsToUse;
            IDictionary<MeasurementKey, IMeasurement> frameMeasurements = frame.Measurements;
            MeasurementKey[] measurementKeys = InputMeasurementKeys;

            if (measurements == null || measurements.Length < minNeeded)
                measurements = new IMeasurement[minNeeded];

            if (measurementKeys == null)
            {
                // No input measurements are defined, just get first set of measurements in this frame
                foreach (IMeasurement measurement in frameMeasurements.Values)
                {
                    measurements[index++] = measurement;
                    if (index == minNeeded)
                        break;
                }
            }
            else
            {
                // Loop through all input measurements to see if they exist in this frame
                IMeasurement measurement;

                for (int x = 0; x < measurementKeys.Length; x++)
                {
                    if (frameMeasurements.TryGetValue(measurementKeys[x], out measurement))
                    {
                        measurements[index++] = measurement;
                        if (index == minNeeded)
                            break;
                    }
                }
            }

            return (index == minNeeded);
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            if (NewMeasurements != null)
                NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        protected virtual void OnStatusMessage(string status)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(status));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event with a formatted status message.
        /// </summary>
        /// <param name="formattedStatus">Formatted status message.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convienence.
        /// </remarks>
        protected virtual void OnStatusMessage(string formattedStatus, params object[] args)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(string.Format(formattedStatus, args)));
        }

        /// <summary>
        /// Raises <see cref="InputMeasurementKeysUpdated"/> event.
        /// </summary>
        protected virtual void OnInputMeasurementKeysUpdated()
        {
            if (InputMeasurementKeysUpdated != null)
                InputMeasurementKeysUpdated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="OutputMeasurementsUpdated"/> event.
        /// </summary>
        protected virtual void OnOutputMeasurementsUpdated()
        {
            if (OutputMeasurementsUpdated != null)
                OutputMeasurementsUpdated(this, EventArgs.Empty);
        }

        private void GenHashCode()
        {
            // We cache hash code during construction or after element value change to speed usage
            m_hashCode = (Name + ID.ToString()).GetHashCode();
        }

        #endregion
    }
}