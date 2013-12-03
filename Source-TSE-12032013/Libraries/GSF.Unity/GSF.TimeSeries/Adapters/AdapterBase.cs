//******************************************************************************************************
//  AdapterBase.cs - Gbtc
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
//
//******************************************************************************************************

using GSF.Units;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the base class for any adapter.
    /// </summary>
    public abstract class AdapterBase : IAdapter
    {
        #region [ Members ]

        // Constants
        private const int DefaultMeasurementReportingInterval = 100000;

        /// <summary>
        /// Default initialization timeout.
        /// </summary>
        public const int DefaultInitializationTimeout = 15000;

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Event is raised when <see cref="InputMeasurementKeys"/> are updated.
        /// </summary>
        public event EventHandler InputMeasurementKeysUpdated;

        /// <summary>
        /// Event is raised when <see cref="OutputMeasurements"/> are updated.
        /// </summary>
        public event EventHandler OutputMeasurementsUpdated;

        /// <summary>
        /// Event is raised when <see cref="AdapterBase"/> is disposed.
        /// </summary>
        /// <remarks>
        /// If an adapter references another adapter by enumerating the <see cref="Parent"/> collection, this
        /// event should be monitored to release the reference.
        /// </remarks>
        public event EventHandler Disposed;

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
        private ManualResetEvent m_initializeWaitHandle;
        private AutoResetEvent[] m_externalEventHandles;
        private int m_externalEventTimeout;
        private MeasurementKey[] m_inputMeasurementKeys;
        private IMeasurement[] m_outputMeasurements;
        private List<MeasurementKey> m_inputMeasurementKeysHash;
        private long m_processedMeasurements;
        private int m_measurementReportingInterval;
        private bool m_enabled;
        private long m_startTime;
        private long m_stopTime;
        private DateTime m_startTimeConstraint;
        private DateTime m_stopTimeConstraint;
        private int m_processingInterval;
        private int m_hashCode;
        private bool m_initialized;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="AdapterBase"/>.
        /// </summary>
        protected AdapterBase()
        {
            m_name = this.GetType().Name;
            m_settings = new Dictionary<string, string>();
            m_startTimeConstraint = DateTime.MinValue;
            m_stopTimeConstraint = DateTime.MaxValue;
            m_processingInterval = -1;
            GenHashCode();

            // Create wait handle to use for adapter initialization
            m_initializeWaitHandle = new ManualResetEvent(false);
            m_initializationTimeout = DefaultInitializationTimeout;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AdapterBase"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AdapterBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes should provide a name for the adapter.
        /// </remarks>
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
        /// Gets or sets numeric ID associated with this <see cref="AdapterBase"/>.
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
        /// Gets or sets key/value pair connection information specific to this <see cref="AdapterBase"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example connection string using manually defined measurements:<br/>
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
        /// <c>inputMeasurementKeys={FILTER ActiveMeasurements WHERE Company='GSF' AND SignalType='FREQ' ORDER BY ID};</c><br/>
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
                if (m_connectionString.IsNullOrWhiteSpace())
                    m_settings = new Dictionary<string, string>();
                else
                    m_settings = m_connectionString.ParseKeyValuePairs();
            }
        }

        /// <summary>
        /// Gets a read-only reference to the collection that contains this <see cref="AdapterBase"/>.
        /// </summary>
        public ReadOnlyCollection<IAdapter> Parent
        {
            get
            {
                return new ReadOnlyCollection<IAdapter>(m_parent);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="AdapterBase"/>.
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
        /// Gets or sets primary keys of input measurements the <see cref="AdapterBase"/> expects, if any.
        /// </summary>
        /// <remarks>
        /// If your adapter needs to receive all measurements, you must explicitly set InputMeasurementKeys to null.
        /// </remarks>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines primary keys of input measurements the adapter expects; can be one of a filter expression, measurement key, point tag or Guid.")]
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
                }
                else
                    m_inputMeasurementKeysHash = null;

                OnInputMeasurementKeysUpdated();
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the <see cref="AdapterBase"/> will produce, if any.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines primary keys of output measurements the adapter expects; can be one of a filter expression, measurement key, point tag or Guid.")]
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
        /// Gets the total number of measurements handled thus far by the <see cref="AdapterBase"/>.
        /// </summary>
        public virtual long ProcessedMeasurements
        {
            get
            {
                return m_processedMeasurements;
            }
        }

        /// <summary>
        /// Gets or sets the measurement reporting interval.
        /// </summary>
        /// <remarks>
        /// This is used to determined how many measurements should be processed before reporting status.
        /// </remarks>
        public virtual int MeasurementReportingInterval
        {
            get
            {
                return m_measurementReportingInterval;
            }
            set
            {
                m_measurementReportingInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if the adapter has been initialized successfully.
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
                if ((object)m_initializeWaitHandle != null)
                {
                    if (value)
                        m_initializeWaitHandle.Set();
                    else
                        m_initializeWaitHandle.Reset();
                }
            }
        }

        /// <summary>
        /// Gets or sets enabled state of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <remarks>
        /// Base class simply starts or stops <see cref="AdapterBase"/> based on flag value. Derived classes should
        /// extend this if needed to enable or disable operational state of the adapter based on flag value.
        /// </remarks>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (m_enabled && !value)
                    Stop();
                else if (!m_enabled && value)
                    Start();
            }
        }

        /// <summary>
        /// Gets the UTC time this <see cref="AdapterBase"/> was started.
        /// </summary>
        public Ticks StartTime
        {
            get
            {
                return m_startTime;
            }
        }

        /// <summary>
        /// Gets the UTC time this <see cref="AdapterBase"/> was stopped.
        /// </summary>
        public Ticks StopTime
        {
            get
            {
                return m_stopTime;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public abstract bool SupportsTemporalProcessing
        {
            get;
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
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, overwhich to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        public virtual int ProcessingInterval
        {
            get
            {
                return m_processingInterval;
            }
            set
            {
                m_processingInterval = value;

                if (m_processingInterval < -1)
                    m_processingInterval = -1;
            }
        }

        /// <summary>
        /// Gets the total amount of time, in seconds, that the adapter has been active.
        /// </summary>
        public virtual Time RunTime
        {
            get
            {
                Ticks processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                    {
                        processingTime = m_stopTime - m_startTime;
                    }
                    else
                    {
#if UseHighResolutionTime
                        processingTime = PrecisionTimer.UtcNow.Ticks - m_startTime;
#else
                        processingTime = DateTime.UtcNow.Ticks - m_startTime;
#endif
                    }
                }

                if (processingTime < 0)
                    processingTime = 0;

                return processingTime.ToSeconds();
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
        /// Gets the status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes should provide current status information about the adapter for display purposes.
        /// </remarks>
        public virtual string Status
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
                status.AppendFormat("    External event handles: {0}", m_externalEventHandles == null ? "None defined" : m_externalEventHandles.Length + " defined");
                status.AppendLine();
                if (m_externalEventHandles != null)
                {
                    status.AppendFormat("    External event timeout: {0} milliseconds", m_externalEventTimeout);
                    status.AppendLine();
                }
                status.AppendFormat("       Temporal processing: {0}", SupportsTemporalProcessing ? "Supported" : "Unsupported");
                status.AppendLine();
                if (SupportsTemporalProcessing)
                {
                    status.AppendFormat("     Start time constraint: {0}", StartTimeConstraint == DateTime.MinValue ? "Unspecified" : StartTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    status.AppendLine();
                    status.AppendFormat("      Stop time constraint: {0}", StopTimeConstraint == DateTime.MaxValue ? "Unspecified" : StopTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    status.AppendLine();
                    status.AppendFormat("       Processing interval: {0}", ProcessingInterval < 0 ? "Default" : (ProcessingInterval == 0 ? "As fast as possible" : ProcessingInterval + " milliseconds"));
                    status.AppendLine();
                }
                status.AppendFormat("   Item reporting interval: {0}", MeasurementReportingInterval);
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

                if (OutputMeasurements != null && OutputMeasurements.Length > OutputMeasurements.Count(m => m.Key == MeasurementKey.Undefined))
                {
                    status.AppendFormat("       Output measurements: {0} defined measurements", OutputMeasurements.Length);
                    status.AppendLine();
                    status.AppendLine();

                    for (int i = 0; i < Common.Min(OutputMeasurements.Length, MaxMeasurementsToShow); i++)
                    {
                        status.Append(OutputMeasurements[i].ToString().TruncateRight(40).PadLeft(40));
                        status.Append(" ");
                        status.AppendLine(OutputMeasurements[i].ID.ToString());
                    }

                    if (OutputMeasurements.Length > MaxMeasurementsToShow)
                        status.AppendLine("...".PadLeft(26));

                    status.AppendLine();
                }

                if (InputMeasurementKeys != null && InputMeasurementKeys.Length > InputMeasurementKeys.Count(k => k == MeasurementKey.Undefined))
                {
                    status.AppendFormat("        Input measurements: {0} defined measurements", InputMeasurementKeys.Length);
                    status.AppendLine();
                    status.AppendLine();

                    for (int i = 0; i < Common.Min(InputMeasurementKeys.Length, MaxMeasurementsToShow); i++)
                    {
                        status.AppendLine(InputMeasurementKeys[i].ToString().TruncateRight(25).CenterText(50));
                    }

                    if (InputMeasurementKeys.Length > MaxMeasurementsToShow)
                        status.AppendLine("...".CenterText(50));

                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets or sets collection of <see cref="AutoResetEvent"/> wait handles used to synchronize activity with external events.
        /// </summary>
        /// <remarks>
        /// Each defined external event in the collection should be acquired via <see cref="GetExternalEventHandle"/>.
        /// </remarks>
        protected AutoResetEvent[] ExternalEventHandles
        {
            get
            {
                return m_externalEventHandles;
            }
            set
            {
                if (value != null && value.Length > 0)
                    m_externalEventHandles = value;
                else
                    m_externalEventHandles = null;
            }
        }

        /// <summary>
        /// Gets or sets maximum to time to wait, in milliseconds, for external events before proceeding.
        /// </summary>
        protected int ExternalEventTimeout
        {
            get
            {
                return m_externalEventTimeout;
            }
            set
            {
                m_externalEventTimeout = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AdapterBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdapterBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_initializeWaitHandle != null)
                            m_initializeWaitHandle.Close();

                        m_initializeWaitHandle = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Intializes <see cref="AdapterBase"/>.
        /// </summary>
        public virtual void Initialize()
        {
            Initialized = false;

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue("inputMeasurementKeys", out setting))
                InputMeasurementKeys = AdapterBase.ParseInputMeasurementKeys(DataSource, setting);
            else
                InputMeasurementKeys = new MeasurementKey[0];

            if (settings.TryGetValue("outputMeasurements", out setting))
                OutputMeasurements = AdapterBase.ParseOutputMeasurements(DataSource, setting);

            if (settings.TryGetValue("measurementReportingInterval", out setting))
                MeasurementReportingInterval = int.Parse(setting);
            else
                MeasurementReportingInterval = DefaultMeasurementReportingInterval;

            if (settings.TryGetValue("connectOnDemand", out setting))
                AutoStart = !setting.ParseBoolean();
            else
                AutoStart = true;

            string startTime, stopTime, parameters;

            bool startTimeDefined = settings.TryGetValue("startTimeConstraint", out startTime);
            bool stopTimeDefined = settings.TryGetValue("stopTimeConstraint", out stopTime);

            if (startTimeDefined || stopTimeDefined)
            {
                settings.TryGetValue("timeConstraintParameters", out parameters);
                SetTemporalConstraint(startTime, stopTime, parameters);
            }

            int processingInterval;

            if (settings.TryGetValue("processingInterval", out setting) && !setting.IsNullOrWhiteSpace() && int.TryParse(setting, out processingInterval))
                ProcessingInterval = processingInterval;

//            // Establish any defined external event wait handles needed for inter-adapter synchronization
//            if (settings.TryGetValue("waitHandleNames", out setting) && !setting.IsNullOrWhiteSpace())
//                m_externalEventHandles = setting.Split(',').Select(GetExternalEventHandle).ToArray();

            int waitHandleTimeout;

            if (settings.TryGetValue("waitHandleTimeout", out setting) && !setting.IsNullOrWhiteSpace() && int.TryParse(setting, out waitHandleTimeout))
                m_externalEventTimeout = waitHandleTimeout;
            else
                m_externalEventTimeout = 33;
        }

        /// <summary>
        /// Starts the <see cref="AdapterBase"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the adapter or restarts it if it is already running.")]
        public virtual void Start()
        {
            // Make sure we are stopped (e.g., disconnected) before attempting to start (e.g., connect)
            if (m_enabled)
                Stop();

            // Wait for adapter intialization to complete...
            m_enabled = WaitForInitialize(InitializationTimeout);

            if (m_enabled)
            {
                m_stopTime = 0;
                m_startTime = PrecisionTimer.UtcNow.Ticks;
            }
            else
                OnProcessException(new TimeoutException("Failed to start adapter due to timeout waiting for initialization."));
        }

        /// <summary>
        /// Stops the <see cref="AdapterBase"/>.
        /// </summary>		
        [AdapterCommand("Stops the adapter.")]
        public virtual void Stop()
        {
            m_enabled = false;
            m_stopTime = PrecisionTimer.UtcNow.Ticks;
        }

        /// <summary>
        /// Assigns the reference to the parent <see cref="IAdapterCollection"/> that will contain this <see cref="AdapterBase"/>.
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
        /// Gets a common wait handle for inter-adapter synchronization.
        /// </summary>
        /// <param name="name">Case-insensitive wait handle name.</param>
        /// <returns>A <see cref="AutoResetEvent"/> based wait handle associated with the given <paramref name="name"/>.</returns>
        public virtual AutoResetEvent GetExternalEventHandle(string name)
        {
            return m_parent.GetExternalEventHandle(name);
        }
  
        protected bool WaitForExternalEvents()
        {
            return WaitForExternalEvents(0);
        }
        
        /// <summary>
        /// Waits for all external events to fire; no wait will happen if no external events are defined.
        /// </summary>
        /// <param name="timeout">Maximum to time to wait, in milliseconds, for external events before proceeding - value defaults to <see cref="ExternalEventTimeout"/>.</param>
        /// <returns><c>true</c> when every all <see cref="ExternalEventHandles"/> have received a signal or there are no external events defined; otherwise, <c>false</c>.</returns>
        protected virtual bool WaitForExternalEvents(int timeout)
        {
            if (m_externalEventHandles != null)
            {
                if (timeout == 0)
                    timeout = m_externalEventTimeout;

                return WaitHandle.WaitAll(m_externalEventHandles, timeout);
            }

            return true;
        }

        /// <summary>
        /// Manually sets the intialized state of the <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="initialized">Desired initialized state.</param>
        [AdapterCommand("Manually sets the intialized state of the adapter.")]
        public virtual void SetInitializedState(bool initialized)
        {
            this.Initialized = initialized;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public abstract string GetShortStatus(int maxLength);

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
        /// Blocks the <see cref="Thread.CurrentThread"/> until the adapter is <see cref="Initialized"/>.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait.</param>
        /// <returns><c>true</c> if the initialization succeeds; otherwise, <c>false</c>.</returns>
        public virtual bool WaitForInitialize(int timeout)
        {
            if ((object)m_initializeWaitHandle != null)
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
            if (!startTime.IsNullOrWhiteSpace())
                m_startTimeConstraint = ParseTimeTag(startTime);
            else
                m_startTimeConstraint = DateTime.MinValue;

            if (!stopTime.IsNullOrWhiteSpace())
                m_stopTimeConstraint = ParseTimeTag(stopTime);
            else
                m_stopTimeConstraint = DateTime.MaxValue;
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="AdapterBase"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="AdapterBase"/>.</returns>
        public override int GetHashCode()
        {
            return m_hashCode;
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        protected virtual void OnStatusMessage(string status)
        {
            try
            {
                if (StatusMessage != null)
                    StatusMessage(this, new EventArgs<string>(status));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for StatusMessage event: {0}", ex.Message), ex));
            }
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
            try
            {
                if (StatusMessage != null)
                    StatusMessage(this, new EventArgs<string>(string.Format(formattedStatus, args)));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for StatusMessage event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises <see cref="InputMeasurementKeysUpdated"/> event.
        /// </summary>
        protected virtual void OnInputMeasurementKeysUpdated()
        {
            try
            {
                if (InputMeasurementKeysUpdated != null)
                    InputMeasurementKeysUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for InputMeasurementKeysUpdated event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises <see cref="OutputMeasurementsUpdated"/> event.
        /// </summary>
        protected virtual void OnOutputMeasurementsUpdated()
        {
            try
            {
                if (OutputMeasurementsUpdated != null)
                    OutputMeasurementsUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for OutputMeasurementsUpdated event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Safely increments the total processed measurements.
        /// </summary>
        /// <param name="totalAdded"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void IncrementProcessedMeasurements(long totalAdded)
        {
            // Check to see if total number of added points will exceed process interval used to show periodic
            // messages of how many points have been archived so far...
            int interval = MeasurementReportingInterval;

            if (interval > 0)
            {
                bool showMessage = m_processedMeasurements + totalAdded >= (m_processedMeasurements / interval + 1) * interval;

                m_processedMeasurements += totalAdded;

                if (showMessage)
                    OnStatusMessage("{0:N0} measurements have been processed so far...", m_processedMeasurements);
            }
        }

        private void GenHashCode()
        {
            // We cache hash code during construction or after element value change to speed usage
            m_hashCode = (Name + ID.ToString()).GetHashCode();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_filterExpression = new Regex("(FILTER[ ]+(?<TableName>\\w+)[ ]+WHERE[ ]+(?<Expression>.+)[ ]+ORDER[ ]+BY[ ]+(?<SortField>\\w+))|(FILTER[ ]+(?<TableName>\\w+)[ ]+WHERE[ ]+(?<Expression>.+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_timetagExpression = new Regex("\\*(?<Offset>[+-]?\\d*\\.?\\d*)(?<Unit>\\w+)", RegexOptions.Compiled);

        // Static Methods

        /// <summary>
        /// Parses a string formatted as an absolute or relative time tag.
        /// </summary>
        /// <param name="timetag">String formatted as an absolute or relative time tag.</param>
        /// <returns>
        /// <see cref="DateTime"/> representing the parsed <paramref name="timetag"/> string formatted as an absolute or relative time tag.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Relative times are parsed based on an offset to current time (UTC) specified by "*".
        /// </para>
        /// <para>
        /// The <paramref name="timetag"/> parameter can be specified in one of the following formats:
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
        /// <exception cref="ArgumentNullException"><paramref name="timetag"/> parameter cannot be null or empty.</exception>
        /// <exception cref="FormatException"><paramref name="timetag"/> does not contain a valid string representation of a date and time.</exception>
        public static DateTime ParseTimeTag(string timetag)
        {
            if (timetag.IsNullOrWhiteSpace())
                throw new ArgumentNullException("timetag", "Timetag string cannot be null or empty.");

            DateTime dateTime;

            if (timetag.Contains("*"))
            {
                // Relative time is specified.
                // Examples:
                // 1) * (Now)
                // 2) *-20s (20 seconds ago)
                // 3) *-10m (10 minutes ago)
                // 4) *-1h (1 hour ago)
                // 5) *-1d (1 day ago)

                dateTime = DateTime.UtcNow;
                timetag = timetag.RemoveWhiteSpace();

                if (timetag.Length > 1)
                {
                    Match timetagMatch;

                    lock (s_timetagExpression)
                    {
                        timetagMatch = s_timetagExpression.Match(timetag);
                    }

                    if (timetagMatch.Success)
                    {
                        double offset = double.Parse(timetagMatch.Result("${Offset}").Trim());
                        string unit = timetagMatch.Result("${Unit}").Trim().ToLower();

                        switch (unit[0])
                        {
                            case 's':
                                dateTime = dateTime.AddSeconds(offset);
                                break;
                            case 'm':
                                dateTime = dateTime.AddMinutes(offset);
                                break;
                            case 'h':
                                dateTime = dateTime.AddHours(offset);
                                break;
                            case 'd':
                                dateTime = dateTime.AddDays(offset);
                                break;
                        }
                    }
                    else
                    {
                        // Expression match failed, attempt to parse absolute time specification.
                        dateTime = DateTime.Parse(timetag, CultureInfo.InvariantCulture);
                    }
                }
            }
            else
            {
                // Absolute time is specified.
                dateTime = DateTime.Parse(timetag, CultureInfo.InvariantCulture);
            }

            return dateTime;
        }
  
        public static void LoadInputSourceIDs(IAdapter adapter)
        {
            LoadInputSourceIDs(adapter, "ActiveMeasurements");
        }
        
        /// <summary>
        /// Loads an <see cref="IOutputAdapter"/> or <see cref="IActionAdapter"/> instance's input measurement keys from a specific set of source ID's.
        /// </summary>
        /// <param name="adapter"><see cref="IAdapter"/> to load input measurement keys for.</param>
        /// <param name="measurementTable">Measurement table name used to load input source ID's.</param>
        /// <remarks>
        /// Any existing input measurement keys will be distinctly merged with those associated with specified source ID's.
        /// </remarks>
        public static void LoadInputSourceIDs(IAdapter adapter, string measurementTable)
        {
            string[] sourceIDs = null;

            if (adapter is IOutputAdapter)
                sourceIDs = (adapter as IOutputAdapter).InputSourceIDs;
            else if (adapter is IActionAdapter)
                sourceIDs = (adapter as IActionAdapter).InputSourceIDs;

            if (sourceIDs != null)
            {
                // Attempt to lookup input measurement keys for given source IDs from default measurement table, if defined
                try
                {
                    if (adapter.DataSource.Tables.Contains(measurementTable))
                    {
                        StringBuilder likeExpression = new StringBuilder();

                        // Build like expression for each source ID
                        foreach (string sourceID in sourceIDs)
                        {
                            if (likeExpression.Length > 0)
                                likeExpression.Append(" OR ");

                            likeExpression.AppendFormat("ID LIKE '{0}:*'", sourceID);
                        }

                        DataRow[] filteredRows = adapter.DataSource.Tables[measurementTable].Select(likeExpression.ToString());
                        MeasurementKey[] sourceIDKeys = null;

                        if (filteredRows.Length > 0)
                            sourceIDKeys = filteredRows.Select(row => MeasurementKey.Parse(row["ID"].ToNonNullString(MeasurementKey.Undefined.ToString()), row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>())).ToArray();

                        if (sourceIDKeys != null)
                        {
                            // Combine input measurement keys for source IDs with any existing input measurement keys and return unique set
                            if (adapter.InputMeasurementKeys == null)
                                adapter.InputMeasurementKeys = sourceIDKeys;
                            else
                                adapter.InputMeasurementKeys = sourceIDKeys.Concat(adapter.InputMeasurementKeys).Distinct().ToArray();
                        }
                    }
                }
                catch
                {
                    // Errors here are not catastrophic, this simply limits the auto-assignment of input measurement keys based on specified source ID's
                }
            }
        }
  
        public static void LoadOutputSourceIDs(IAdapter adapter)
        {
            LoadOutputSourceIDs(adapter, "ActiveMeasurements");
        }
        
        /// <summary>
        /// Loads an <see cref="IInputAdapter"/> or <see cref="IActionAdapter"/> instance's output measurements from a specific set of source ID's.
        /// </summary>
        /// <param name="adapter"><see cref="IAdapter"/> to load output measurements for.</param>
        /// <param name="measurementTable">Measurement table name used to load output source ID's.</param>
        /// <remarks>
        /// Any existing output measurements will be distinctly merged with those associated with specified source ID's.
        /// </remarks>
        public static void LoadOutputSourceIDs(IAdapter adapter, string measurementTable)
        {
            string[] sourceIDs = null;

            if (adapter is IInputAdapter)
                sourceIDs = (adapter as IInputAdapter).OutputSourceIDs;
            else if (adapter is IActionAdapter)
                sourceIDs = (adapter as IActionAdapter).OutputSourceIDs;

            if (sourceIDs != null)
            {
                // Attempt to lookup output measurements for given source IDs from default measurement table, if defined
                try
                {
                    if (adapter.DataSource.Tables.Contains(measurementTable))
                    {
                        StringBuilder likeExpression = new StringBuilder();

                        // Build like expression for each source ID
                        foreach (string sourceID in sourceIDs)
                        {
                            if (likeExpression.Length > 0)
                                likeExpression.Append(" OR ");

                            likeExpression.AppendFormat("ID LIKE '{0}:*'", sourceID);
                        }

                        DataRow[] filteredRows = adapter.DataSource.Tables[measurementTable].Select(likeExpression.ToString());
                        MeasurementKey[] sourceIDKeys = null;

                        if (filteredRows.Length > 0)
                            sourceIDKeys = filteredRows.Select(row => MeasurementKey.Parse(row["ID"].ToNonNullString(MeasurementKey.Undefined.ToString()), row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>())).ToArray();

                        if (sourceIDKeys != null)
                        {
                            List<IMeasurement> measurements = new List<IMeasurement>();

                            foreach (MeasurementKey key in sourceIDKeys)
                            {
                                // Create a new measurement for the provided field level information
                                Measurement measurement = new Measurement()
                                {
                                    Key = key
                                };

                                // Attempt to lookup other associated measurement meta-data from default measurement table, if defined
                                try
                                {
                                    if (adapter.DataSource.Tables.Contains(measurementTable))
                                    {
                                        filteredRows = adapter.DataSource.Tables[measurementTable].Select(string.Format("ID = '{0}'", key.ToString()));

                                        if (filteredRows.Length > 0)
                                        {
                                            DataRow row = filteredRows[0];

                                            measurement.ID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                                            measurement.TagName = row["PointTag"].ToNonNullString();

                                            // Attempt to update empty signal ID if available
                                            if (measurement.Key.SignalID == Guid.Empty)
                                                measurement.Key.UpdateSignalID(measurement.ID);

                                            measurement.Multiplier = double.Parse(row["Multiplier"].ToString());
                                            measurement.Adder = double.Parse(row["Adder"].ToString());
                                        }
                                    }
                                }
                                catch
                                {
                                    // Errors here are not catastrophic, this simply limits the available meta-data
                                    measurement.ID = Guid.Empty;
                                    measurement.TagName = string.Empty;
                                }

                                measurements.Add(measurement);
                            }

                            // Combine output measurements for source IDs with any existing output measurements and return unique set
                            if (adapter.OutputMeasurements == null)
                                adapter.OutputMeasurements = measurements.ToArray();
                            else
                                adapter.OutputMeasurements = measurements.Concat(adapter.OutputMeasurements).Distinct().ToArray();
                        }
                    }
                }
                catch
                {
                    // Errors here are not catastrophic, this simply limits the auto-assignment of input measurement keys based on specified source ID's
                }
            }
        }

        // Input keys can use DataSource for filtering desired set of input or output measurements
        // based on any table and fields in the data set by using a filter expression instead of
        // a list of measurement ID's. The format is as follows:

        //  FILTER <TableName> WHERE <Expression> [ORDER BY <SortField>]

        // Source tables are expected to have at least the following fields:
        //
        //      ID          NVARCHAR    Measurement key formatted as: ArchiveSource:PointID
        //      SignalID    GUID        Unique identification for measurement
        //      PointTag    NVARCHAR    Point tag of measurement
        //      Adder       FLOAT       Adder to apply to value, if any (default to 0.0)
        //      Multiplier  FLOAT       Multipler to apply to value, if any (default to 1.0)
        //
        // Could have used standard SQL syntax here but didn't want to give user the impression
        // that this a standard SQL expression when it isn't - so chose the word FILTER to make
        // consumer was aware that this was not SQL, but SQL "like". The WHERE clause expression
        // uses standard SQL syntax (it is simply the DataTable.Select filter expression).
  
        public static MeasurementKey[] ParseInputMeasurementKeys(DataSet dataSource, string value)
        {
            return ParseInputMeasurementKeys(dataSource, value, "ActiveMeasurements");
        }
        
        /// <summary>
        /// Parses input measurement keys from connection string setting.
        /// </summary>
        /// <param name="dataSource">The <see cref="DataSet"/> used to define input measurement keys.</param>
        /// <param name="value">Value of setting used to define input measurement keys, typically "inputMeasurementKeys".</param>
        /// <param name="measurementTable">Measurement table name used to load additional meta-data; this is not used when specifying a FILTER expression.</param>
        /// <returns>User selected input measurement keys.</returns>
        public static MeasurementKey[] ParseInputMeasurementKeys(DataSet dataSource, string value, string measurementTable)
        {
            List<MeasurementKey> keys = new List<MeasurementKey>();
            MeasurementKey key;
            Guid id;
            Match filterMatch = null;
            bool dataSourceAvailable = ((object)dataSource != null);

            value = value.Trim();

            if (!value.IsNullOrWhiteSpace())
            {
                if (dataSourceAvailable)
                {
                    lock (s_filterExpression)
                    {
                        filterMatch = s_filterExpression.Match(value);
                    }
                }

                if (dataSourceAvailable && filterMatch.Success)
                {
                    string tableName = filterMatch.Result("${TableName}").Trim();
                    string expression = filterMatch.Result("${Expression}").Trim();
                    string sortField = filterMatch.Result("${SortField}").Trim();

                    foreach (DataRow row in dataSource.Tables[tableName].Select(expression, sortField))
                    {
                        if (MeasurementKey.TryParse(row["ID"].ToString(), row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), out key))
                            keys.Add(key);
                    }
                }
                else
                {
                    // Add manually defined measurement keys
                    foreach (string item in value.Split(';'))
                    {
                        if (!item.IsNullOrWhiteSpace())
                        {
                            if (MeasurementKey.TryParse(item, Guid.Empty, out key))
                            {
                                // Attempt to update empty signal ID if available
                                if (dataSourceAvailable && key.SignalID == Guid.Empty)
                                {
                                    if (dataSource.Tables.Contains(measurementTable))
                                    {
                                        DataRow[] filteredRows = dataSource.Tables[measurementTable].Select(string.Format("ID = '{0}'", key.ToString()));

                                        if (filteredRows.Length > 0)
                                            key.SignalID = filteredRows[0]["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                                    }
                                }

                                keys.Add(key);
                            }
                            else if (Common.TryParseGuid(item, out id))
                            {
                                if (dataSourceAvailable && dataSource.Tables.Contains(measurementTable))
                                {
                                    DataRow[] filteredRows = dataSource.Tables[measurementTable].Select(string.Format("SignalID = '{0}'", id));

                                    if (filteredRows.Length > 0 && MeasurementKey.TryParse(filteredRows[0]["ID"].ToString(), id, out key))
                                        keys.Add(key);
                                }
                                else
                                {
                                    keys.Add(MeasurementKey.LookupBySignalID(id));
                                }
                            }
                            else
                            {
                                // Attempt to update empty signal ID if available
                                if (dataSourceAvailable && dataSource.Tables.Contains(measurementTable))
                                {
                                    DataRow[] filteredRows = dataSource.Tables[measurementTable].Select(string.Format("PointTag = '{0}'", item));

                                    if (filteredRows.Length > 0)
                                    {
                                        key = MeasurementKey.LookupBySignalID(filteredRows[0]["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>());
                                        keys.Add(key);
                                    }
                                }

                                if (key == default(MeasurementKey))
                                {
                                    throw new InvalidOperationException(string.Format("Could not parse input measurement definition \"{0}\" as a filter expression, measurement key, point tag or Guid", item));
                                }
                            }
                        }
                    }
                }
            }

            return keys.ToArray();
        }
  
        public static MeasurementKey[] ParseOutputMeasurementKeys(DataSet dataSource, string value)
        {
            return ParseOutputMeasurementKeys(dataSource, value, "ActiveMeasurements");
        }
        
        /// <summary>
        /// Parses output measurement keys from connection string setting.
        /// </summary>
        /// <param name="dataSource">The <see cref="DataSet"/> used to define output measurements.</param>
        /// <param name="value">Value of setting used to define output measurements, typically "outputMeasurements".</param>
        /// <param name="measurementTable">Measurement table name used to load additional meta-data; this is not used when specifying a FILTER expression.</param>
        /// <returns>User selected output measurements.</returns>
        public static MeasurementKey[] ParseOutputMeasurementKeys(DataSet dataSource, string value, string measurementTable)
        {
            return ParseOutputMeasurements(dataSource, value, measurementTable).MeasurementKeys().ToArray();
        }
  
        public static IMeasurement[] ParseOutputMeasurements(DataSet dataSource, string value)
        {
            return ParseOutputMeasurements(dataSource, value, "ActiveMeasurements");
        }
        
        /// <summary>
        /// Parses output measurements from connection string setting.
        /// </summary>
        /// <param name="dataSource">The <see cref="DataSet"/> used to define output measurements.</param>
        /// <param name="value">Value of setting used to define output measurements, typically "outputMeasurements".</param>
        /// <param name="measurementTable">Measurement table name used to load additional meta-data; this is not used when specifying a FILTER expression.</param>
        /// <returns>User selected output measurements.</returns>
        public static IMeasurement[] ParseOutputMeasurements(DataSet dataSource, string value, string measurementTable)
        {
            List<IMeasurement> measurements = new List<IMeasurement>();
            Measurement measurement;
            MeasurementKey key;
            Match filterMatch = null;
            Guid id;
            bool dataSourceAvailable = ((object)dataSource != null);

            value = value.Trim();

            if (!value.IsNullOrWhiteSpace())
            {
                if (dataSourceAvailable)
                {
                    lock (s_filterExpression)
                    {
                        filterMatch = s_filterExpression.Match(value);
                    }
                }

                if (dataSourceAvailable && filterMatch.Success)
                {
                    string tableName = filterMatch.Result("${TableName}").Trim();
                    string expression = filterMatch.Result("${Expression}").Trim();
                    string sortField = filterMatch.Result("${SortField}").Trim();

                    foreach (DataRow row in dataSource.Tables[tableName].Select(expression, sortField))
                    {
                        id = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();

                        measurement = new Measurement()
                        {
                            ID = id,
                            Key = MeasurementKey.Parse(row["ID"].ToString(), id),
                            TagName = row["PointTag"].ToNonNullString(),
                            Adder = double.Parse(row["Adder"].ToString()),
                            Multiplier = double.Parse(row["Multiplier"].ToString())
                        };

                        measurements.Add(measurement);
                    }
                }
                else
                {
                    string[] elem;
                    double adder, multipler;

                    foreach (string item in value.Split(';'))
                    {
                        if (!item.IsNullOrWhiteSpace())
                        {
                            elem = item.Trim().Split(',');

                            if (!MeasurementKey.TryParse(elem[0], Guid.Empty, out key))
                            {
                                if (Common.TryParseGuid(item, out id))
                                {
                                    if (dataSourceAvailable && dataSource.Tables.Contains(measurementTable))
                                    {
                                        DataRow[] filteredRows = dataSource.Tables[measurementTable].Select(string.Format("SignalID = '{0}'", id));

                                        if (filteredRows.Length > 0)
                                            MeasurementKey.TryParse(filteredRows[0]["ID"].ToString(), id, out key);
                                    }
                                    else
                                    {
                                        key = MeasurementKey.LookupBySignalID(id);
                                    }
                                }
                                else
                                {
                                    // Attempt to update empty signal ID if available
                                    if (dataSourceAvailable && dataSource.Tables.Contains(measurementTable))
                                    {
                                        DataRow[] filteredRows = dataSource.Tables[measurementTable].Select(string.Format("PointTag = '{0}'", item));

                                        if (filteredRows.Length > 0)
                                        {
                                            key = MeasurementKey.LookupBySignalID(filteredRows[0]["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>());
                                        }
                                    }

                                    if (key == default(MeasurementKey))
                                    {
                                        throw new InvalidOperationException(string.Format("Could not parse output measurement definition \"{0}\" as a filter expression, measurement key, point tag or Guid", item));
                                    }
                                }
                            }

                            // Adder and multipler may be optionally specified
                            if (elem.Length > 1)
                            {
                                if (!double.TryParse(elem[1].Trim(), out adder))
                                    adder = 0.0D;
                            }
                            else
                            {
                                adder = 0.0D;
                            }

                            if (elem.Length > 2)
                            {
                                if (!double.TryParse(elem[2].Trim(), out multipler))
                                    multipler = 1.0D;
                            }
                            else
                            {
                                multipler = 1.0D;
                            }

                            // Create a new measurement for the provided field level information
                            measurement = new Measurement()
                            {
                                ID = key.SignalID,
                                Key = key,
                                Adder = adder,
                                Multiplier = multipler
                            };

                            // Attempt to lookup other associated measurement meta-data from default measurement table, if defined
                            try
                            {
                                if (dataSourceAvailable && dataSource.Tables.Contains(measurementTable))
                                {
                                    DataRow[] filteredRows = dataSource.Tables[measurementTable].Select(string.Format("ID = '{0}'", key.ToString()));

                                    if (filteredRows.Length > 0)
                                    {
                                        DataRow row = filteredRows[0];

                                        measurement.ID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                                        measurement.TagName = row["PointTag"].ToNonNullString();

                                        // Attempt to update empty signal ID if available
                                        if (measurement.Key.SignalID == Guid.Empty)
                                            measurement.Key.UpdateSignalID(measurement.ID);

                                        // Manually specified adder and multiplier take precedence, but if none were specified,
                                        // then those defined in the meta-data are used instead
                                        if (elem.Length < 3)
                                            measurement.Multiplier = double.Parse(row["Multiplier"].ToString());

                                        if (elem.Length < 2)
                                            measurement.Adder = double.Parse(row["Adder"].ToString());
                                    }
                                }
                            }
                            catch
                            {
                                // Errors here are not catastrophic, this simply limits the available meta-data
                                measurement.TagName = string.Empty;
                            }

                            measurements.Add(measurement);
                        }
                    }
                }
            }

            return measurements.ToArray();
        }

        #endregion
    }
}