//******************************************************************************************************
//  ActionAdapterBase.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/01/2013 - Stephen C. Wills
//       Updated to process time-series entities.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using GSF.TimeSeries.Routing;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the base class for action adapters.
    /// </summary>
    /// <remarks>
    /// This base class acts as a time-series entity concentrator which time aligns all incoming entities for proper processing.
    /// Derived classes are expected to override <see cref="ConcentratorBase.PublishFrame"/> to handle time aligned entities
    /// and call <see cref="OnNewEntities"/> for any new entities that may get created.
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
        /// Event is raised when <see cref="InputSignalIDs"/> are updated.
        /// </summary>
        public event EventHandler InputSignalIDsUpdated;

        /// <summary>
        /// Event is raised when <see cref="OutputSignalIDs"/> are updated.
        /// </summary>
        public event EventHandler OutputSignalIDsUpdated;

        /// <summary>
        /// Event is raised when adapter is aware of a configuration change.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Provides new entities from action adapter.
        /// </summary>
        public event EventHandler<RoutingEventArgs> NewEntities;

        // Fields
        private string m_name;
        private uint m_id;
        private string m_connectionString;
        private Dictionary<string, string> m_settings;
        private DataSet m_dataSource;
        private RoutingEventArgs m_routingEventArgs;
        private readonly object m_newEntitiesLock;
        private int m_initializationTimeout;
        private bool m_autoStart;
        private bool m_respectInputDemands;
        private bool m_respectOutputDemands;
        private ISet<Guid> m_inputSignals;
        private ISet<Guid> m_outputSignals;
        private ISet<Guid> m_requestedInputSignals;
        private ISet<Guid> m_requestedOutputSignals;
        private List<string> m_inputSourceIDs;
        private List<string> m_outputSourceIDs;
        private int m_minimumSignalsToUse;
        private ManualResetEvent m_initializeWaitHandle;
        private DateTime m_startTimeConstraint;
        private DateTime m_stopTimeConstraint;
        private bool m_initialized;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ActionAdapterBase"/>.
        /// </summary>
        protected ActionAdapterBase()
        {
            m_name = GetType().Name;
            m_settings = new Dictionary<string, string>();
            m_startTimeConstraint = DateTime.MinValue;
            m_stopTimeConstraint = DateTime.MaxValue;
            m_newEntitiesLock = new object();

            // Create wait handle to use for adapter initialization
            m_initializeWaitHandle = new ManualResetEvent(false);
            m_initializationTimeout = AdapterBase.DefaultInitializationTimeout;

            // For most implementations millisecond resolution will be sufficient
            TimeResolution = Ticks.PerMillisecond;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets key/value pair connection information specific to action adapter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example connection string using manually defined signals:<br/>
        /// <c>framesPerSecond=30; lagTime=1.0; leadTime=0.5; minimumSignalsToUse=-1;
        /// useLocalClockAsRealTime=true; allowSortsByArrival=false;</c><br/>
        /// <c>inputSignals={P1:1245;P1:1247;P2:1335};</c><br/>
        /// <c>outputSignals={P3:1345;P3:1346;P3:1347}</c><br/>
        /// When defined manually, elements in key: are defined as "ArchiveSource:PointID"<br/>
        /// </para>
        /// <para>
        /// Example connection string using signals defined in a <see cref="DataSource"/> table:<br/>
        /// <c>framesPerSecond=30; lagTime=1.0; leadTime=0.5; minimumSignalsToUse=-1;
        /// useLocalClockAsRealTime=true; allowSortsByArrival=false;</c><br/>
        /// <c>inputSignals={FILTER ActiveMeasurements WHERE Company='GSF' AND SignalType='FREQ' ORDER BY ID};</c><br/>
        /// <c>outputSignals={FILTER ActiveMeasurements WHERE SignalType IN ('IPHA','VPHA') AND Phase='+'}</c><br/>
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
        ///         <description>Point tag of signal.</description>
        ///     </item>
        ///     <item>
        ///         <term>Adder</term>
        ///         <term>FLOAT</term>
        ///         <description>Adder to apply to value, if any (default to 0.0).</description>
        ///     </item>
        ///     <item>
        ///         <term>Multiplier</term>
        ///         <term>FLOAT</term>
        ///         <description>Multiplier to apply to value, if any (default to 1.0).</description>
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
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on input demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is setup to connect on demand. In the case of respecting auto-start input demands,
        /// as an example, this would be <c>false</c> for an action adapter that calculated measurements, but <c>true</c> for an action adapter used to archive inputs.
        /// </remarks>
        public virtual bool RespectInputDemands
        {
            get
            {
                return m_respectInputDemands;
            }
            set
            {
                m_respectInputDemands = value;
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on output demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is setup to connect on demand. In the case of respecting auto-start output demands,
        /// as an example, this would be <c>true</c> for an action adapter that calculated measurements, but <c>false</c> for an action adapter used to archive inputs.
        /// </remarks>
        public virtual bool RespectOutputDemands
        {
            get
            {
                return m_respectOutputDemands;
            }
            set
            {
                m_respectOutputDemands = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        [ConnectionStringParameter,
        Description("Defines the number of frames per second expected by the adapter.")]
        public new int FramesPerSecond
        {
            get
            {
                return base.FramesPerSecond;
            }
            set
            {
                base.FramesPerSecond = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past timestamps.</para>
        /// <para>The number of seconds allowed before assuming a timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter,
        Description("Defines the allowed past time deviation tolerance, in seconds (can be sub-second).")]
        public new double LagTime
        {
            get
            {
                return base.LagTime;
            }
            set
            {
                base.LagTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future timestamps.</para>
        /// <para>The number of seconds allowed before assuming a timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter,
        Description("Defines the allowed future time deviation tolerance, in seconds (can be sub-second).")]
        public new double LeadTime
        {
            get
            {
                return base.LeadTime;
            }
            set
            {
                base.LeadTime = value;
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input signals the action adapter expects.
        /// </summary>
        /// <remarks>
        /// If your adapter needs to receive all signals, you must explicitly set InputSignals to null.
        /// </remarks>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines primary keys of input signals the action adapter expects; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public virtual ISet<Guid> InputSignalIDs
        {
            get
            {
                return m_inputSignals;
            }
            set
            {
                m_inputSignals = value;
                OnInputSignalsUpdated();
            }
        }

        /// <summary>
        /// Gets or sets output signals that the action adapter will produce, if any.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines primary keys of output signals the action adapter expects; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public virtual ISet<Guid> OutputSignalIDs
        {
            get
            {
                return m_outputSignals;
            }
            set
            {
                m_outputSignals = value;
                OnOutputSignalsUpdated();
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter input signals.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of signals based on the source of the measurement keys.
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

                // Filter signals to list of specified source IDs
                AdapterBase.LoadInputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter output signals.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of signals based on the source of the measurement keys.
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

                // Filter signals to list of specified source IDs
                AdapterBase.LoadOutputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets input signals that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual ISet<Guid> RequestedInputSignals
        {
            get
            {
                return m_requestedInputSignals;
            }
            set
            {
                m_requestedInputSignals = value;
            }
        }

        /// <summary>
        /// Gets or sets output signals that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual ISet<Guid> RequestedOutputSignals
        {
            get
            {
                return m_requestedOutputSignals;
            }
            set
            {
                m_requestedOutputSignals = value;
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
        /// Gets the start time temporal processing constraint defined by call to <see cref="SetTemporalConstraint"/>.
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
        /// Gets or sets minimum number of input signals required for adapter.  Set to -1 to require all.
        /// </summary>
        public virtual int MinimumSignalsToUse
        {
            get
            {
                // Default to all signals if minimum is not specified
                if (m_minimumSignalsToUse < 1)
                    return InputSignalIDs.Count;

                return m_minimumSignalsToUse;
            }
            set
            {
                m_minimumSignalsToUse = value;
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
                const int MaxSignalsToShow = 10;

                StringBuilder status = new StringBuilder();
                DataSet dataSource = DataSource;

                status.AppendFormat("       Data source defined: {0}", (dataSource != null));
                status.AppendLine();
                if (dataSource != null)
                {
                    status.AppendFormat("    Referenced data source: {0}, {1} tables", dataSource.DataSetName, dataSource.Tables.Count);
                    status.AppendLine();
                }
                status.AppendFormat("    Initialization timeout: {0}", InitializationTimeout < 0 ? "Infinite" : InitializationTimeout.ToString() + " milliseconds");
                status.AppendLine();
                status.AppendFormat("       Adapter initialized: {0}", Initialized);
                status.AppendLine();
                status.AppendFormat("         Operational state: {0}", Enabled ? "Running" : "Stopped");
                status.AppendLine();
                status.AppendFormat("         Connect on demand: {0}", !AutoStart);
                status.AppendLine();
                status.AppendFormat("  Respecting input demands: {0}", RespectInputDemands);
                status.AppendLine();
                status.AppendFormat(" Respecting output demands: {0}", RespectOutputDemands);
                status.AppendLine();
                status.AppendFormat("        Processed entities: {0}", ProcessedEntities);
                status.AppendLine();
                status.AppendFormat("    Total adapter run time: {0}", RunTime.ToString());
                status.AppendLine();
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
                status.AppendFormat("                Adapter ID: {0}", ID);
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

                if (InputSignalIDs.Any(signalID => signalID != Guid.Empty))
                {
                    status.AppendFormat("             Input Signals: {0} defined signals", InputSignalIDs.Count);
                    status.AppendLine();
                    status.AppendLine();

                    foreach (Guid signalID in InputSignalIDs.Take(MaxSignalsToShow))
                    {
                        status.Append(this.GetSignalInfo(signalID, maxLength: 40).PadLeft(40));
                        status.Append(" ");
                        status.AppendLine(signalID.ToString());
                    }

                    if (InputSignalIDs.Count > MaxSignalsToShow)
                        status.AppendLine("...".PadLeft(26));

                    status.AppendLine();
                }

                if (OutputSignalIDs.Any(signalID => signalID != Guid.Empty))
                {
                    status.AppendFormat("            Output Signals: {0} defined signals", OutputSignalIDs.Count);
                    status.AppendLine();
                    status.AppendLine();

                    foreach (Guid signalID in OutputSignalIDs.Take(MaxSignalsToShow))
                    {
                        status.Append(this.GetSignalInfo(signalID, maxLength: 40).PadLeft(40));
                        status.Append(" ");
                        status.AppendLine(signalID.ToString());
                    }

                    if (OutputSignalIDs.Count > MaxSignalsToShow)
                        status.AppendLine("...".PadLeft(26));

                    status.AppendLine();
                }

                if ((object)RequestedInputSignals != null && RequestedInputSignals.Count > 0)
                {
                    status.AppendFormat("   Requested Input Signals: {0} defined signals", RequestedInputSignals.Count);
                    status.AppendLine();
                    status.AppendLine();

                    foreach (Guid signalID in RequestedInputSignals.Take(MaxSignalsToShow))
                    {
                        status.Append(this.GetSignalInfo(signalID, maxLength: 40).PadLeft(40));
                        status.Append(" ");
                        status.AppendLine(signalID.ToString());
                    }

                    if (RequestedInputSignals.Count > MaxSignalsToShow)
                        status.AppendLine("...".PadLeft(26));

                    status.AppendLine();
                }

                if ((object)RequestedOutputSignals != null && RequestedOutputSignals.Count > 0)
                {
                    status.AppendFormat("  Requested Output Signals: {0} defined signals", RequestedOutputSignals.Count);
                    status.AppendLine();
                    status.AppendLine();

                    foreach (Guid signalID in RequestedOutputSignals.Take(MaxSignalsToShow))
                    {
                        status.Append(this.GetSignalInfo(signalID, maxLength: 40).PadLeft(40));
                        status.Append(" ");
                        status.AppendLine(signalID.ToString());
                    }

                    if (RequestedOutputSignals.Count > MaxSignalsToShow)
                        status.AppendLine("...".PadLeft(26));

                    status.AppendLine();
                }

                status.AppendFormat("      Minimum signals used: {0}", MinimumSignalsToUse);
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
                        if ((object)m_initializeWaitHandle != null)
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
            const string ErrorMessage = "{0} is missing from Settings - Example: framesPerSecond=30; lagTime=3; leadTime=1";
            string setting;

            // Load required parameters
            if (!settings.TryGetValue("framesPerSecond", out setting))
                throw new ArgumentException(string.Format(ErrorMessage, "framesPerSecond"));

            base.FramesPerSecond = int.Parse(setting);

            if (!settings.TryGetValue("lagTime", out setting))
                throw new ArgumentException(string.Format(ErrorMessage, "lagTime"));

            base.LagTime = double.Parse(setting);

            if (!settings.TryGetValue("leadTime", out setting))
                throw new ArgumentException(string.Format(ErrorMessage, "leadTime"));

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

            if (settings.TryGetValue("inputSignals", out setting))
                InputSignalIDs = AdapterBase.ParseFilterExpression(DataSource, true, setting);
            else
                InputSignalIDs = new HashSet<Guid>();

            if (settings.TryGetValue("outputSignals", out setting))
                OutputSignalIDs = AdapterBase.ParseFilterExpression(DataSource, true, setting);

            if (settings.TryGetValue("minimumSignalsToUse", out setting))
                MinimumSignalsToUse = int.Parse(setting);

            if (settings.TryGetValue("timeResolution", out setting))
                TimeResolution = long.Parse(setting);

            if (settings.TryGetValue("allowPreemptivePublishing", out setting))
                AllowPreemptivePublishing = setting.ParseBoolean();

            if (settings.TryGetValue("performTimestampReasonabilityCheck", out setting))
                PerformTimestampReasonabilityCheck = setting.ParseBoolean();

            if (settings.TryGetValue("processByReceivedTimestamp", out setting))
                ProcessByCreationTime = setting.ParseBoolean();

            if (settings.TryGetValue("maximumPublicationTimeout", out setting))
                MaximumPublicationTimeout = int.Parse(setting);

            if (settings.TryGetValue("inputSourceIDs", out setting))
                InputSourceIDs = setting.Split(',');

            if (settings.TryGetValue("outputSourceIDs", out setting))
                OutputSourceIDs = setting.Split(',');

            if (settings.TryGetValue("connectOnDemand", out setting))
                AutoStart = !setting.ParseBoolean();
            else
                AutoStart = true;

            if (settings.TryGetValue("respectInputDemands", out setting))
                RespectInputDemands = setting.ParseBoolean();
            else
                RespectInputDemands = false;

            if (settings.TryGetValue("respectOutputDemands", out setting))
                RespectOutputDemands = setting.ParseBoolean();
            else
                RespectOutputDemands = true;

            string startTime, stopTime, parameters;

            bool startTimeDefined = settings.TryGetValue("startTimeConstraint", out startTime);
            bool stopTimeDefined = settings.TryGetValue("stopTimeConstraint", out stopTime);

            if (startTimeDefined || stopTimeDefined)
            {
                settings.TryGetValue("timeConstraintParameters", out parameters);
                SetTemporalConstraint(startTime, stopTime, parameters);
            }

            int processingInterval;

            if (settings.TryGetValue("processingInterval", out setting) && !string.IsNullOrWhiteSpace(setting) && int.TryParse(setting, out processingInterval))
                ProcessingInterval = processingInterval;
        }

        /// <summary>
        /// Starts the <see cref="ActionAdapterBase"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the action adapter or restarts it if it is already running.", "Administrator", "Editor")]
        public override void Start()
        {
            // Make sure we are stopped (e.g., disconnected) before attempting to start (e.g., connect)
            if (Enabled)
                Stop();

            base.Start();
        }

        /// <summary>
        /// Stops the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Stops the action adapter.", "Administrator", "Editor")]
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Examines the concentrator frame queue state of the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Examines concentration frame queue state.", "Administrator", "Editor", "Viewer")]
        public void ExamineQueueState()
        {
            OnStatusMessage(QueueState);
        }

        /// <summary>
        /// Resets the statistics of the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Resets the statistics of the action adapter.", "Administrator", "Editor")]
        public override void ResetStatistics()
        {
            base.ResetStatistics();

            if (Enabled)
                OnStatusMessage("Action adapter concentration statistics have been reset.");
        }

        /// <summary>
        /// Manually sets the initialized state of the <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="initialized">Desired initialized state.</param>
        [AdapterCommand("Manually sets the initialized state of the action adapter.", "Administrator", "Editor")]
        public virtual void SetInitializedState(bool initialized)
        {
            Initialized = initialized;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="ActionAdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public virtual string GetShortStatus(int maxLength)
        {
            return string.Format("Total input signals: {0}, total output signals: {1}", InputSignalIDs.Count, OutputSignalIDs.Count).PadLeft(maxLength);
        }

        /// <summary>
        /// Queues a collection of time-series entities for processing.
        /// </summary>
        /// <param name="entities">Collection of entities to queue for processing.</param>
        public virtual void QueueEntitiesForProcessing(IEnumerable<ITimeSeriesEntity> entities)
        {
            if (m_disposed)
                return;

            SortEntities(entities);
        }

        /// <summary>
        /// Defines a temporal processing constraint for the adapter.
        /// </summary>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint.</param>
        /// <remarks>
        /// <para>
        /// This method defines a temporal processing constraint for an adapter, i.e., the start and stop time over which an
        /// adapter will process data. Actual implementation of the constraint will be adapter specific. Implementations
        /// should be able to dynamically handle multiple calls to this function with new constraints. Passing in <c>null</c>
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
        [AdapterCommand("Defines a temporal processing constraint for the adapter.", "Administrator", "Editor", "Viewer")]
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

            // When processing historical data, timestamps should not be evaluated for reasonability
            if (this.TemporalConstraintIsDefined())
            {
                PerformTimestampReasonabilityCheck = false;
                LeadTime = double.MaxValue;
            }
        }

        /// <summary>
        /// Raises the <see cref="NewEntities"/> event.
        /// </summary>
        protected virtual void OnNewEntities(ICollection<ITimeSeriesEntity> entities)
        {
            try
            {
                lock (m_newEntitiesLock)
                {
                    if ((object)m_routingEventArgs == null)
                        m_routingEventArgs = new RoutingEventArgs();

                    m_routingEventArgs.TimeSeriesEntities = entities;

                    if ((object)NewEntities != null)
                        NewEntities(this, m_routingEventArgs);
                }
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for NewEntities event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        protected virtual void OnStatusMessage(string status)
        {
            try
            {
                if ((object)StatusMessage != null)
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
        /// This overload combines string.Format and SendStatusMessage for convenience.
        /// </remarks>
        protected virtual void OnStatusMessage(string formattedStatus, params object[] args)
        {
            try
            {
                if ((object)StatusMessage != null)
                    StatusMessage(this, new EventArgs<string>(string.Format(formattedStatus, args)));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for StatusMessage event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises <see cref="InputSignalIDsUpdated"/> event.
        /// </summary>
        protected virtual void OnInputSignalsUpdated()
        {
            try
            {
                if ((object)InputSignalIDsUpdated != null)
                    InputSignalIDsUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for InputSignalsUpdated event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises <see cref="OutputSignalIDsUpdated"/> event.
        /// </summary>
        protected virtual void OnOutputSignalsUpdated()
        {
            try
            {
                if ((object)OutputSignalIDsUpdated != null)
                    OutputSignalIDsUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for OutputSignalsUpdated event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises <see cref="ConfigurationChanged"/> event.
        /// </summary>
        protected virtual void OnConfigurationChanged()
        {
            try
            {
                if ((object)ConfigurationChanged != null)
                    ConfigurationChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ConfigurationChanged event: {0}", ex.Message), ex));
            }
        }

        #endregion
    }
}