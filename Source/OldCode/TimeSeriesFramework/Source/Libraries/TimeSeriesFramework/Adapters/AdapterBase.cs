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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TVA;
using TVA.Units;

namespace TimeSeriesFramework.Adapters
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
        private ManualResetEvent m_initializeWaitHandle;
        private MeasurementKey[] m_inputMeasurementKeys;
        private IMeasurement[] m_outputMeasurements;
        private List<MeasurementKey> m_inputMeasurementKeysHash;
        private long m_processedMeasurements;
        private int m_measurementReportingInterval;
        private bool m_enabled;
        private long m_startTime;
        private long m_stopTime;
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
            m_initializeWaitHandle = new ManualResetEvent(false);
            m_settings = new Dictionary<string, string>();
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
                if (string.IsNullOrEmpty(m_connectionString))
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
        /// Gets or sets output measurements that the <see cref="AdapterBase"/> will produce, if any.
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
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="AdapterBase"/> expects, if any.
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
                if (value != null)
                {
                    m_inputMeasurementKeysHash = new List<MeasurementKey>(value);
                    m_inputMeasurementKeysHash.Sort();
                }
                else
                    m_inputMeasurementKeysHash = null;
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
        /// Gets the total amount of time, in seconds, that the concentrator has been active.
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

                if (processingTime < 0) processingTime = 0;

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
                status.AppendFormat("       Adapter initialized: {0}", Initialized);
                status.AppendLine();
                status.AppendFormat("         Parent collection: {0}", m_parent == null ? "Undefined" : m_parent.Name);
                status.AppendLine();
                status.AppendFormat("         Operational state: {0}", Enabled ? "Running" : "Stopped");
                status.AppendLine();
                status.AppendFormat("    Processed measurements: {0}", ProcessedMeasurements);
                status.AppendLine();
                status.AppendFormat("    Total adapter run time: {0}", RunTime.ToString());
                status.AppendLine();
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

                if (OutputMeasurements != null)
                {
                    status.AppendFormat("       Output measurements: {0} defined measurements", OutputMeasurements.Length);
                    status.AppendLine();
                    status.AppendLine();

                    for (int i = 0; i < Common.Min(OutputMeasurements.Length, MaxMeasurementsToShow); i++)
                    {
                        status.Append(OutputMeasurements[i].ToString().TruncateRight(25).PadLeft(25));
                        status.Append(" ");
                        status.AppendLine(OutputMeasurements[i].SignalID.ToString());
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
                        status.AppendLine(InputMeasurementKeys[i].ToString().TruncateRight(25).CenterText(50));
                    }

                    if (InputMeasurementKeys.Length > MaxMeasurementsToShow)
                        status.AppendLine("...".CenterText(50));

                    status.AppendLine();
                }

                return status.ToString();
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
                        if (m_initializeWaitHandle != null)
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

            if (settings.TryGetValue("outputMeasurements", out setting))
                OutputMeasurements = AdapterBase.ParseOutputMeasurements(DataSource, setting);

            if (settings.TryGetValue("measurementReportingInterval", out setting))
                MeasurementReportingInterval = int.Parse(setting);
            else
                MeasurementReportingInterval = DefaultMeasurementReportingInterval;
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
        /// Assigns the reference to the parent <see cref="IAdapterCollection"/> that will contain this <see cref="IAdapter"/>.
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
            if (m_initializeWaitHandle != null)
                return m_initializeWaitHandle.WaitOne(timeout);

            return false;
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
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
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

        #endregion

        #region [ Static ]

        // Static Fields
        private static Regex s_filterExpression = new Regex("(FILTER[ ]+(?<TableName>\\w+)[ ]+WHERE[ ]+(?<Expression>.+)[ ]+ORDER[ ]+BY[ ]+(?<SortField>\\w+))|(FILTER[ ]+(?<TableName>\\w+)[ ]+WHERE[ ]+(?<Expression>.+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Static Methods

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

        /// <summary>
        /// Parses input measurement keys from connection string setting.
        /// </summary>
        /// <param name="dataSource">The <see cref="DataSet"/> used to filter measurments when user specifies a FILTER expression to define input measurement keys.</param>
        /// <param name="value">Value of setting used to define input measurement keys, typically "inputMeasurementKeys".</param>
        /// <returns>User selected input measurement keys.</returns>
        public static MeasurementKey[] ParseInputMeasurementKeys(DataSet dataSource, string value)
        {
            List<MeasurementKey> keys = new List<MeasurementKey>();
            Match filterMatch;

            value = value.Trim();
            lock (s_filterExpression)
            {
                filterMatch = s_filterExpression.Match(value);
            }

            if (filterMatch.Success)
            {
                string tableName = filterMatch.Result("${TableName}").Trim();
                string expression = filterMatch.Result("${Expression}").Trim();
                string sortField = filterMatch.Result("${SortField}").Trim();

                foreach (DataRow row in dataSource.Tables[tableName].Select(expression, sortField))
                {
                    keys.Add(MeasurementKey.Parse(row["ID"].ToString()));
                }
            }
            else
            {
                // Add manually defined measurement keys
                foreach (string item in value.Split(';'))
                {
                    keys.Add(MeasurementKey.Parse(item));
                }
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Parses output measurements from connection string setting.
        /// </summary>
        /// <param name="dataSource">The <see cref="DataSet"/> used to filter measurments when user specifies a FILTER expression to define output measurements.</param>
        /// <param name="value">Value of setting used to define output measurements, typically "outputMeasurements".</param>
        /// <returns>User selected output measurements.</returns>
        public static IMeasurement[] ParseOutputMeasurements(DataSet dataSource, string value)
        {
            List<IMeasurement> measurements = new List<IMeasurement>();
            Measurement measurement;
            MeasurementKey key;
            Match filterMatch;

            value = value.Trim();
            lock (s_filterExpression)
            {
                filterMatch = s_filterExpression.Match(value);
            }

            if (filterMatch.Success)
            {
                string tableName = filterMatch.Result("${TableName}").Trim();
                string expression = filterMatch.Result("${Expression}").Trim();
                string sortField = filterMatch.Result("${SortField}").Trim();

                foreach (DataRow row in dataSource.Tables[tableName].Select(expression, sortField))
                {
                    key = MeasurementKey.Parse(row["ID"].ToString());
                    measurement = new Measurement(key.ID, key.Source, row["PointTag"].ToNonNullString(), double.Parse(row["Adder"].ToString()), double.Parse(row["Multiplier"].ToString()));
                    measurement.SignalID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                    measurements.Add(measurement);
                }
            }
            else
            {
                string[] elem;
                double adder, multipler;

                foreach (string item in value.Split(';'))
                {
                    elem = item.Trim().Split(',');

                    key = MeasurementKey.Parse(elem[0]);

                    // Adder and multipler may be optionally specified
                    if (elem.Length > 1)
                        adder = double.Parse(elem[1].Trim());
                    else
                        adder = 0.0D;

                    if (elem.Length > 2)
                        multipler = double.Parse(elem[2].Trim());
                    else
                        multipler = 1.0D;

                    // Create a new measurement for the provided field level information
                    measurement = new Measurement(key.ID, key.Source, string.Empty, adder, multipler);

                    // Attempt to lookup other associated measurement meta-data from default measurement table, if defined
                    try
                    {
                        if (dataSource.Tables.Contains("ActiveMeasurements"))
                        {
                            DataRow[] filteredRows = dataSource.Tables["ActiveMeasurements"].Select(string.Format("ID = '{0}'", key.ToString()));

                            if (filteredRows.Length > 0)
                            {
                                DataRow row = filteredRows[0];

                                measurement.SignalID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                                measurement.TagName = row["PointTag"].ToNonNullString();

                                // Manually specified adder and multiplier take precedence but if none were specified,
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
                        measurement.SignalID = Guid.Empty;
                        measurement.TagName = string.Empty;
                    }

                    measurements.Add(measurement);
                }
            }

            return measurements.ToArray();
        }

        #endregion        
    }
}