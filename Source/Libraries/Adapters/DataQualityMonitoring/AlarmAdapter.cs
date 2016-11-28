//******************************************************************************************************
//  AlarmAdapter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/31/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using DataQualityMonitoring.Services;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Statistics;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Action adapter that generates alarm measurements based on alarm definitions from the database.
    /// </summary>
    [Description("Alarm: Generates alarm events for alarms defined in the database")]
    public class AlarmAdapter : FacileActionAdapterBase
    {
        #region [ Members ]

        // Nested Types
        private class SignalAlarms
        {
            public string Name;
            public List<Alarm> Alarms;
            public AlarmStatistics Statistics;
        }

        private class StateChange
        {
            public Guid SignalID;
            public int? OldStateID;
            public int? NewStateID;
            public DateTime Timestamp;
            public double Value;
        }

        // Constants
        private const int UpToDate = 0;
        private const int Modified = 1;

        // Fields
        private readonly object m_alarmLock;

        private readonly DoubleBufferedQueue<IMeasurement> m_measurementQueue;
        private readonly MixedSynchronizedOperation m_processMeasurementsOperation;
        private DataSet m_alarmDataSet;
        private int m_dataSourceState;

        private Dictionary<Guid, SignalAlarms> m_alarmLookup;
        private AlarmService m_alarmService;
        private long m_eventCount;

        private readonly LongSynchronizedOperation m_alarmLogOperation;
        private readonly DoubleBufferedQueue<StateChange> m_stateChanges;
        private bool m_useAlarmLog;
        private int m_bulkInsertLimit;
        private int m_logProcessingDelay;

        private bool m_supportsTemporalProcessing;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AlarmAdapter"/> class.
        /// </summary>
        public AlarmAdapter()
        {
            m_alarmLock = new object();
            m_alarmLookup = new Dictionary<Guid, SignalAlarms>();

            m_measurementQueue = new DoubleBufferedQueue<IMeasurement>();
            m_processMeasurementsOperation = new MixedSynchronizedOperation(ProcessMeasurements, ex => OnProcessException(MessageLevel.Warning, "AlarmAdapter", ex));

            m_alarmLogOperation = new LongSynchronizedOperation(LogStateChanges, ex => OnProcessException(MessageLevel.Warning, "AlarmAdapter", ex));
            m_stateChanges = new DoubleBufferedQueue<StateChange>();
            m_alarmLogOperation.IsBackground = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="AdapterBase"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;

                if (Interlocked.CompareExchange(ref m_dataSourceState, Modified, UpToDate) == UpToDate && Initialized)
                {
                    m_processMeasurementsOperation.AsynchronousExecutionMode = AsynchronousExecutionMode.Long;
                    m_processMeasurementsOperation.RunOnceAsync();
                }
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter supports temporal processing."),
        DefaultValue(false)]
        public override bool SupportsTemporalProcessing => m_supportsTemporalProcessing;

        /// <summary>
        /// Gets or sets the flag indicating whether the alarm adapter should
        /// use the alarm log to track recently modified alarm states.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating whether to use the alarm log to track recently modified alarm states."),
        DefaultValue(true)]
        public bool UseAlarmLog
        {
            get
            {
                return m_useAlarmLog;
            }
            set
            {
                m_useAlarmLog = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of alarm state changes to pack into one alarm log insert query.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the maximum number of alarm state changes to pack into one alarm log insert query."),
        DefaultValue(300)]
        public int BulkInsertLimit
        {
            get
            {
                return m_bulkInsertLimit;
            }
            set
            {
                m_bulkInsertLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of time, in seconds, to wait for alarm log state changes between bulk inserts.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the amount of time, in seconds, to wait for alarm log state changes between bulk inserts."),
        DefaultValue(1.0D)]
        public double LogProcessingDelay
        {
            get
            {
                return m_logProcessingDelay / 1000.0D;
            }
            set
            {
                m_logProcessingDelay = (int)(value * 1000.0D);
            }
        }

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);
                statusBuilder.Append(m_alarmService.Status);
                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="AlarmAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings;
            string setting;
            double logProcessingDelay;

            // Run base class initialization
            base.Initialize();
            settings = Settings;

            // Load optional parameters
            if (settings.TryGetValue("supportsTemporalProcessing", out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = false;

            if (settings.TryGetValue("useAlarmLog", out setting))
                m_useAlarmLog = setting.ParseBoolean();
            else
                m_useAlarmLog = true;

            if (!settings.TryGetValue("bulkInsertLimit", out setting) || !int.TryParse(setting, out m_bulkInsertLimit))
                m_bulkInsertLimit = 300;

            if (settings.TryGetValue("logProcessingDelay", out setting) && double.TryParse(setting, out logProcessingDelay))
                m_logProcessingDelay = (int)(logProcessingDelay * 1000.0D);
            else
                m_logProcessingDelay = 1000;

            try
            {
                // Set up alarm service
                m_alarmService = new AlarmService(this);
                m_alarmService.SettingsCategory = base.Name.Replace("!", "").ToLower() + m_alarmService.SettingsCategory;
                m_alarmService.ServiceProcessException += AlarmService_ServiceProcessException;
                m_alarmService.PersistSettings = true;
                m_alarmService.Initialize();
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Error, "AlarmAdapter", new InvalidOperationException($"Unable to initialize alarm service due to exception: {ex.Message}", ex));
            }

            // Run the process measurements operation to ensure that the alarm configuration is up to date
            if (Interlocked.CompareExchange(ref m_dataSourceState, Modified, Modified) == Modified)
            {
                m_processMeasurementsOperation.AsynchronousExecutionMode = AsynchronousExecutionMode.Long;
                m_processMeasurementsOperation.RunOnceAsync();
            }
        }

        /// <summary>
        /// Starts the <see cref="AlarmAdapter"/>, or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();
            Interlocked.Exchange(ref m_eventCount, 0L);
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            m_measurementQueue.Enqueue(measurements);
            m_processMeasurementsOperation.RunOnceAsync();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"{Interlocked.Read(ref m_eventCount)} events processed since last start".CenterText(maxLength);
        }

        /// <summary>
        /// Gets a collection containing all the raised alarms in the system.
        /// </summary>
        /// <returns>A collection containing all the raised alarms.</returns>
        [AdapterCommand("Gets a collection containing all the raised alarms in the system.", "Administrator", "Editor", "Viewer")]
        public ICollection<Alarm> GetRaisedAlarms()
        {
            lock (m_alarmLock)
            {
                return m_alarmLookup
                    .SelectMany(kvp => kvp.Value.Alarms)
                    .Where(alarm => alarm.State == AlarmState.Raised)
                    .Select(alarm => alarm.Clone())
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a collection containing raised alarms with the highest severity for each signal in the system. 
        /// </summary>
        /// <returns>A collection containing all the highest severity raised alarms.</returns>
        [AdapterCommand("Gets a collection containing raised alarms with the highest severity for each signal in the system.", "Administrator", "Editor", "Viewer")]
        public ICollection<Alarm> GetHighestSeverityAlarms()
        {
            lock (m_alarmLock)
            {
                return m_alarmLookup
                    .Select(kvp => kvp.Value.Alarms.FirstOrDefault(alarm => alarm.State == AlarmState.Raised))
                    .Where(alarm => (object)alarm != null)
                    .Select(alarm => alarm.Clone())
                    .ToList();
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AlarmAdapter"/> object and optionally releases the managed resources.
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
                        if (m_alarmService != null)
                        {
                            m_alarmService.ServiceProcessException -= AlarmService_ServiceProcessException;
                            m_alarmService.Dispose();
                        }

                        lock (m_alarmLock)
                        {
                            foreach (SignalAlarms signalAlarms in m_alarmLookup.Values)
                                StatisticsEngine.Unregister(signalAlarms.Statistics);
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


        // Creates an alarm using data defined in the database.
        private Alarm CreateAlarm(DataRow row)
        {
            object associatedMeasurementId = row.Field<object>("AssociatedMeasurementID");

            return new Alarm((AlarmOperation)row.ConvertField<int>("Operation"))
            {
                ID = row.ConvertField<int>("ID"),
                TagName = row.Field<object>("TagName").ToString(),
                SignalID = Guid.Parse(row.Field<object>("SignalID").ToString()),
                AssociatedMeasurementID = ((object)associatedMeasurementId != null) ? Guid.Parse(associatedMeasurementId.ToString()) : (Guid?)null,
                Description = row.Field<object>("Description").ToNonNullString(),
                Severity = row.ConvertField<int>("Severity").GetEnumValueOrDefault<AlarmSeverity>(AlarmSeverity.None),
                SetPoint = row.ConvertNullableField<double>("SetPoint"),
                Tolerance = row.ConvertNullableField<double>("Tolerance"),
                Delay = row.ConvertNullableField<double>("Delay"),
                Hysteresis = row.ConvertNullableField<double>("Hysteresis")
            };
        }

        // Dequeues measurements from the measurement queue and processes them.
        private void ProcessMeasurements()
        {
            IList<IMeasurement> measurements;
            int dataSourceState;

            // Get the current state of the data source
            dataSourceState = Interlocked.CompareExchange(ref m_dataSourceState, Modified, Modified);

            // If the data source has been modified, elevate the synchronized operation
            // to a dedicated thread and ensure that it runs at least one more time
            if (dataSourceState == Modified)
            {
                if (m_processMeasurementsOperation.CurrentExecutionMode != AsynchronousExecutionMode.Long)
                {
                    m_processMeasurementsOperation.AsynchronousExecutionMode = AsynchronousExecutionMode.Long;
                    m_processMeasurementsOperation.RunOnceAsync();
                    return;
                }

                Interlocked.Exchange(ref m_dataSourceState, UpToDate);
                UpdateAlarmDefinitions();
            }

            // Attempt to dequeue measurements from the measurement queue and,
            // if there are measurements left in the queue, ensure that the
            // process measurements operation runs again
            if (m_measurementQueue.TryDequeue(out measurements) > 0)
                m_processMeasurementsOperation.RunOnceAsync();

            // If items were successfully dequeued, process them;
            // otherwise, switch to short asynchronous execution mode so that
            // we can keep attempting to dequeue in an asynchronous loop
            if (measurements.Count > 0)
                ProcessMeasurements(measurements);
            else
                m_processMeasurementsOperation.AsynchronousExecutionMode = AsynchronousExecutionMode.Short;

            // If there are no pending operations, switch back to the thread
            // pool so that the next operation will start up quicker
            if (!m_processMeasurementsOperation.IsPending)
                m_processMeasurementsOperation.AsynchronousExecutionMode = AsynchronousExecutionMode.Short;
        }

        // Updates alarm definitions when the data source has been updated.
        private void UpdateAlarmDefinitions()
        {
            DateTime now;
            DataSet alarmDataSet;

            Dictionary<int, Alarm> definedAlarmsLookup;
            Dictionary<Guid, SignalAlarms> newAlarmLookup;
            //List<IMeasurement> alarmEvents;

            // Get the current time in case we need
            // to generate alarm events for cleared alarms
            now = DateTime.UtcNow;

            // Get the latest version of the table of defined alarms
            alarmDataSet = new DataSet();
            alarmDataSet.Tables.Add(DataSource.Tables["Alarms"].Copy());

            // Compare the latest alarm table with the previous
            // alarm table to determine if anything needs to be done
            if (DataSetEqualityComparer.Default.Equals(m_alarmDataSet, alarmDataSet))
                return;

            m_alarmDataSet = alarmDataSet;

            // Get list of alarms defined in the latest version of the alarm table
            definedAlarmsLookup = alarmDataSet.Tables[0].Rows.Cast<DataRow>()
                .Where(row => row.ConvertField<bool>("Enabled"))
                .Select(CreateAlarm)
                .ToDictionary(alarm => alarm.ID);

            // Create a list to store alarm events generated by this process
            //alarmEvents = new List<IMeasurement>();

            lock (m_alarmLock)
            {
                foreach (Alarm existingAlarm in m_alarmLookup.SelectMany(kvp => kvp.Value.Alarms))
                {
                    Alarm definedAlarm;

                    // Attempt to locate the defined alarm corresponding to the existing alarm
                    definedAlarmsLookup.TryGetValue(existingAlarm.ID, out definedAlarm);

                    // Determine if a change to the alarm's
                    // configuration has changed the alarm's behavior
                    if (BehaviorChanged(existingAlarm, definedAlarm))
                    {
                        // Clone the existing alarm so that changes to alarm
                        // states can be observed later in this process
                        Alarm clone = existingAlarm.Clone();

                        // Clear the alarm and create an event
                        clone.State = AlarmState.Cleared;
                        //alarmEvents.Add(CreateAlarmEvent(now, clone));
                    }
                    else if ((object)definedAlarm != null)
                    {
                        // Update functionally irrelevant configuration info
                        existingAlarm.TagName = definedAlarm.TagName;
                        existingAlarm.Description = definedAlarm.Description;

                        // Use the existing alarm since the alarm is functionally the same
                        definedAlarmsLookup[definedAlarm.ID] = existingAlarm;
                    }
                }

                // Create the new alarm lookup to replace the old one
                newAlarmLookup = definedAlarmsLookup.Values
                    .GroupBy(alarm => alarm.SignalID)
                    .ToDictionary(grouping => grouping.Key, grouping => new SignalAlarms()
                    {
                    // Alarms are sorted in order of precedence:
                    //   1) Exemptions (forces alarm state to cleared)
                    //   2) Severity (high severity takes precedence over low severity)
                    //   3) ID (relative order of alarms with same severity doesn't change)
                    Alarms = grouping
                            .OrderByDescending(alarm => alarm.Severity == AlarmSeverity.None)
                            .ThenByDescending(alarm => alarm.Severity)
                            .ThenBy(alarm => alarm.ID)
                            .ToList()
                    });

                // Check for changes to alarms that need to go in the alarm log
                foreach (KeyValuePair<Guid, SignalAlarms> kvp in m_alarmLookup)
                {
                    SignalAlarms existingAlarms = kvp.Value;
                    SignalAlarms definedAlarms;

                    // Get the active alarms from both before and after the configuration changes were applied
                    Alarm existingActiveAlarm = existingAlarms.Alarms.FirstOrDefault(alarm => alarm.State == AlarmState.Raised);
                    Alarm definedActiveAlarm = null;

                    if (newAlarmLookup.TryGetValue(kvp.Key, out definedAlarms))
                    {
                        definedAlarms.Statistics = existingAlarms.Statistics;
                        definedActiveAlarm = definedAlarms.Alarms.FirstOrDefault(alarm => alarm.State == AlarmState.Raised);
                    }
                    else
                    {
                        StatisticsEngine.Unregister(existingAlarms.Statistics);
                    }

                    if ((object)definedActiveAlarm != null && (object)existingActiveAlarm != null)
                    {
                        // If the active alarm has changed as a result
                        // of the configuration change, log the change
                        if (definedActiveAlarm.ID != existingActiveAlarm.ID)
                            LogStateChange(kvp.Key, existingActiveAlarm, definedActiveAlarm, now, double.NaN);
                    }
                    else if ((object)existingActiveAlarm != null)
                    {
                        // If alarms were raised before the configuration change,
                        // but have all been cleared as a result of the
                        // configuration change, log the change
                        LogStateChange(kvp.Key, existingActiveAlarm, null, now, double.NaN);
                    }
                }
            }


            // Use SignalReference as the name of the signal when creating statistic source
            foreach (DataRow row in DataSource.Tables["ActiveMeasurements"].Rows)
            {
                Guid signalID;
                SignalAlarms signalAlarms;

                if (Guid.TryParse(row["SignalID"].ToNonNullString(), out signalID) && newAlarmLookup.TryGetValue(signalID, out signalAlarms))
                    signalAlarms.Name = row.Field<string>("SignalReference");
            }

            // Newly added signals that do not have statistics associated
            // with them yet need to be registered with the statistics engine
            foreach (KeyValuePair<Guid, SignalAlarms> kvp in newAlarmLookup)
            {
                if ((object)kvp.Value.Statistics == null)
                {
                    if (string.IsNullOrEmpty(kvp.Value.Name))
                        kvp.Value.Name = kvp.Key.ToString().Replace("-", "");

                    kvp.Value.Statistics = new AlarmStatistics();
                    StatisticsEngine.Register(kvp.Value.Statistics, kvp.Value.Name, "Point", "PT", "{0}!{}");
                }
            }

            lock (m_alarmLock)
            {
                m_alarmLookup = newAlarmLookup;

                // Only automatically update input measurement keys if the setting is not explicitly defined
                if (m_alarmLookup.Count > 0 && !Settings.ContainsKey("inputMeasurementKeys"))
                {
                    // Generate filter expression for input measurements
                    string filterExpression = string.Join(";", m_alarmLookup.Select(kvp => kvp.Key.ToString()));

                    // Set input measurement keys for measurement routing
                    InputMeasurementKeys = ParseInputMeasurementKeys(DataSource, true, filterExpression);
                }
            }
        }

        // Processes measurements in the queue.
        private void ProcessMeasurements(IList<IMeasurement> measurements)
        {
            SignalAlarms alarms;

            Alarm activeAlarm;
            Alarm firstRaisedAlarm;
            List<IMeasurement> alarmEvents;

            alarmEvents = new List<IMeasurement>();

            foreach (IMeasurement measurement in measurements)
            {
                lock (m_alarmLock)
                {
                    // Get alarms that apply to the measurement being processed
                    if (!m_alarmLookup.TryGetValue(measurement.ID, out alarms))
                        continue;

                    // Get the currently active alarm
                    activeAlarm = alarms.Alarms.FirstOrDefault(alarm => alarm.State == AlarmState.Raised);

                    // Test each alarm to determine whether their states have changed
                    alarmEvents.AddRange(alarms.Alarms.Where(alarm => alarm.Test(measurement)).Select(alarm => CreateAlarmEvent(measurement.Timestamp, alarm)));

                    // Get the alarm that will become the currently active alarm
                    firstRaisedAlarm = alarms.Alarms.FirstOrDefault(alarm => alarm.State == AlarmState.Raised);
                }

                // Update alarm log to show changes in state of active alarms
                if (firstRaisedAlarm != activeAlarm)
                    LogStateChange(measurement.ID, activeAlarm, firstRaisedAlarm, measurement.Timestamp, measurement.Value);

                if ((object)firstRaisedAlarm != null && firstRaisedAlarm.Severity > AlarmSeverity.None)
                    alarms.Statistics.IncrementCounter(firstRaisedAlarm);
            }

            if (alarmEvents.Count > 0)
            {
                // Update alarm history by sending
                // new alarm events into the system
                OnNewMeasurements(alarmEvents);
                Interlocked.Add(ref m_eventCount, alarmEvents.Count);
            }

            // Increment total count of processed measurements
            IncrementProcessedMeasurements(measurements.Count);
        }

        // Returns true if a change to the alarm's configuration also changed the alarm's behavior.
        private bool BehaviorChanged(Alarm existingAlarm, Alarm definedAlarm)
        {
            return (object)definedAlarm == null ||
                   (existingAlarm.SignalID != definedAlarm.SignalID) ||
                   (existingAlarm.AssociatedMeasurementID != definedAlarm.AssociatedMeasurementID) ||
                   (existingAlarm.Operation != definedAlarm.Operation) ||
                   (existingAlarm.SetPoint != definedAlarm.SetPoint) ||
                   (existingAlarm.Tolerance != definedAlarm.Tolerance) ||
                   (existingAlarm.Delay != definedAlarm.Delay) ||
                   (existingAlarm.Hysteresis != definedAlarm.Hysteresis);
        }

        // Creates an alarm event from the given alarm and measurement.
        private IMeasurement CreateAlarmEvent(Ticks timestamp, Alarm alarm)
        {
            IMeasurement alarmEvent = new Measurement()
            {
                Timestamp = timestamp,
                Value = (int)alarm.State
            };

            if ((object)alarm.AssociatedMeasurementID != null)
            {
                Guid alarmEventID = alarm.AssociatedMeasurementID.GetValueOrDefault();
                alarmEvent.Metadata = MeasurementKey.LookUpBySignalID(alarmEventID).Metadata;
            }

            return alarmEvent;
        }

        // Writes an entry to the alarm log when the alarm state changes.
        private void LogStateChange(Guid signalID, Alarm oldState, Alarm newState, DateTime timestamp, double value)
        {
            int? oldStateID;
            int? newStateID;

            if (m_useAlarmLog)
            {
                oldStateID = ((object)oldState != null) ? oldState.ID : (int?)null;
                newStateID = ((object)newState != null) ? newState.ID : (int?)null;

                StateChange stateChange = new StateChange()
                {
                    SignalID = signalID,
                    OldStateID = oldStateID,
                    NewStateID = newStateID,
                    Timestamp = timestamp,
                    Value = value
                };

                m_stateChanges.Enqueue(new [] { stateChange });
                m_alarmLogOperation.RunOnceAsync();
            }
        }

        private void LogStateChanges()
        {
            IList<StateChange> stateChanges;

            StringBuilder insertQuery;
            List<object> insertParameters;
            StringBuilder deleteQuery;
            List<object> deleteParameters;

            int count;

            Thread.Sleep(m_logProcessingDelay);

            stateChanges = m_stateChanges.Dequeue();
            insertQuery = new StringBuilder();
            insertParameters = new List<object>();
            deleteQuery = new StringBuilder();
            deleteParameters = new List<object>();
            count = 0;

            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                foreach (StateChange stateChange in stateChanges)
                {
                    if (insertQuery.Length == 0)
                    {
                        insertQuery.Append("INSERT INTO AlarmLog(SignalID, PreviousState, NewState, Ticks, Timestamp, Value) ");
                        insertQuery.Append("SELECT {0} AS SignalID, {1} AS PreviousState, {2} AS NewState, {3} AS Ticks, {4} AS Timestamp, {5} AS Value");

                        deleteQuery.Append("DELETE FROM AlarmLog WHERE ");
                        deleteQuery.Append("(SignalID = {0} AND Ticks < {1})");
                    }
                    else
                    {
                        insertQuery.Append(" UNION ALL ");
                        insertQuery.AppendFormat("SELECT {{{0}}}, {{{1}}}, {{{2}}}, {{{3}}}, {{{4}}}, {{{5}}}", Enumerable.Range(count * 6, 6).Cast<object>().ToArray());

                        deleteQuery.Append(" OR ");
                        deleteQuery.AppendFormat("(SignalID = {{{0}}} AND Ticks < {{{1}}})", Enumerable.Range(count * 2, 2).Cast<object>().ToArray());
                    }

                    insertParameters.Add(stateChange.SignalID);
                    insertParameters.Add(stateChange.OldStateID);
                    insertParameters.Add(stateChange.NewStateID);
                    insertParameters.Add(stateChange.Timestamp.Ticks);
                    insertParameters.Add(stateChange.Timestamp);
                    insertParameters.Add(stateChange.Value);

                    deleteParameters.Add(stateChange.SignalID);
                    deleteParameters.Add(stateChange.Timestamp.AddHours(-24.0D).Ticks);

                    count++;

                    if (count == m_bulkInsertLimit)
                    {
                        connection.ExecuteNonQuery(insertQuery.ToString(), insertParameters.ToArray());
                        connection.ExecuteNonQuery(deleteQuery.ToString(), deleteParameters.ToArray());

                        insertQuery.Clear();
                        insertParameters.Clear();
                        deleteQuery.Clear();
                        deleteParameters.Clear();

                        count = 0;
                    }
                }

                if (count > 0)
                {
                    connection.ExecuteNonQuery(insertQuery.ToString(), insertParameters.ToArray());
                    connection.ExecuteNonQuery(deleteQuery.ToString(), deleteParameters.ToArray());
                }
            }
        }

        // Processes exceptions thrown by the alarm service.
        private void AlarmService_ServiceProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(MessageLevel.Warning, "AlarmAdapter", e.Argument);
        }

        #endregion
    }
}
