//******************************************************************************************************
//  AlarmAdapter.cs - Gbtc
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
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Action adapter that generates alarm measurements based on alarm definitions from the database.
    /// </summary>
    [Description("Alarm: Generates alarm events for alarms defined in the database.")]
    public class AlarmAdapter : FacileActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private List<Alarm> m_alarms;
        private Dictionary<Guid, List<Alarm>> m_alarmLookup;
        private AlarmService m_alarmService;

        private readonly AsyncDoubleBufferedQueue<IMeasurement> m_measurementQueue;
        private long m_eventCount;
        private int m_processThreadState;

        private bool m_supportsTemporalProcessing;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AlarmAdapter"/>.
        /// </summary>
        public AlarmAdapter()
        {
            m_measurementQueue = new AsyncDoubleBufferedQueue<IMeasurement>();
            m_measurementQueue.ProcessException += m_measurementQueue_ProcessException;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter supports temporal processing."),
        DefaultValue(false)]
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return m_supportsTemporalProcessing;
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

            string filterExpression;

            // Run base class initialization
            base.Initialize();
            settings = Settings;

            // Load optional parameters
            if (settings.TryGetValue("supportsTemporalProcessing", out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = false;

            // Create alarms using definitions from the database
            m_alarms = DataSource.Tables["Alarms"].Rows.Cast<DataRow>()
                .Where(row => row.ConvertField<bool>("Enabled"))
                .Select(CreateAlarm)
                .ToList();

            // Create lookup table for alarms to speed up measurement processing
            m_alarmLookup = new Dictionary<Guid, List<Alarm>>();

            foreach (Guid signalID in m_alarms.Select(alarm => alarm.SignalID).Distinct())
            {
                m_alarmLookup.Add(signalID, m_alarms.Where(alarm => alarm.SignalID == signalID).ToList());
            }

            if (m_alarms.Count > 0)
            {
                // Generate filter expression for input measurements
                filterExpression = m_alarms.Select(a => a.SignalID)
                    .Distinct()
                    .Select(id => id.ToString())
                    .Aggregate((list, id) => list + ";" + id);

                // Set input measurement keys for measurement routing
                InputMeasurementKeys = ParseInputMeasurementKeys(DataSource, true, filterExpression);
            }

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
                string message = string.Format("Unable to initialize alarm service due to exception: {0}", ex.Message);
                OnProcessException(new InvalidOperationException(message, ex));
            }
        }

        /// <summary>
        /// Starts the <see cref="AlarmAdapter"/>, or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            Thread processThread;

            base.Start();
            m_eventCount = 0L;

            // If state is stopping (1), set it to running (2)
            if (Interlocked.CompareExchange(ref m_processThreadState, 2, 1) == 0)
            {
                // If state is stopped (0), set it to running (2) and run the new thread
                if (Interlocked.CompareExchange(ref m_processThreadState, 2, 0) == 0)
                {
                    processThread = new Thread(ProcessMeasurements);
                    processThread.IsBackground = true;
                    processThread.Start();
                }
            }
        }

        public override void Stop()
        {
            base.Stop();

            // If state is running (2), set it to stopping (1)
            Interlocked.CompareExchange(ref m_processThreadState, 1, 2);
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            m_measurementQueue.Enqueue(measurements);
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("{0} events processed since last start", m_eventCount).CenterText(maxLength);
        }

        /// <summary>
        /// Gets a collection containing all the raised alarms in the system.
        /// </summary>
        /// <returns>A collection containing all the raised alarms.</returns>
        [AdapterCommand("Gets a collection containing all the raised alarms in the system.", "Administrator", "Editor", "Viewer")]
        public ICollection<Alarm> GetRaisedAlarms()
        {
            lock (m_alarms)
            {
                return m_alarms.Where(alarm => alarm.State == AlarmState.Raised)
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
            return GetRaisedAlarms()
                .GroupBy(alarm => alarm.SignalID)
                .SelectMany(FilterToHighestSeverity)
                .ToList();
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

            return new Alarm
            {
                ID = row.ConvertField<int>("ID"),
                TagName = row.Field<object>("TagName").ToString(),
                SignalID = Guid.Parse(row.Field<object>("SignalID").ToString()),
                AssociatedMeasurementID = ((object)associatedMeasurementId != null) ? Guid.Parse(associatedMeasurementId.ToString()) : (Guid?)null,
                Description = row.Field<object>("Description").ToNonNullString(),
                Severity = (AlarmSeverity)row.ConvertField<int>("Severity"),
                Operation = (AlarmOperation)row.ConvertField<int>("Operation"),
                SetPoint = row.ConvertNullableField<double>("SetPoint"),
                Tolerance = row.ConvertNullableField<double>("Tolerance"),
                Delay = row.ConvertNullableField<double>("Delay"),
                Hysteresis = row.ConvertNullableField<double>("Hysteresis")
            };
        }

        // Processes measurements in the queue.
        private void ProcessMeasurements()
        {
            IEnumerable<IMeasurement> measurements;
            SpinWait spinner;
            int threadID;

            List<Alarm> alarms;
            List<Alarm> raisedAlarms;

            List<IMeasurement> alarmEvents;
            IMeasurement alarmEvent;
            long processedMeasurements;

            threadID = Thread.CurrentThread.ManagedThreadId;
            spinner = new SpinWait();
            alarmEvents = new List<IMeasurement>();

            // If state is stopping (1), set it to stopped (0)
            // If state is running (2), continue looping
            while (Interlocked.CompareExchange(ref m_processThreadState, 0, 1) == 2)
            {
                try
                {
                    // Empty the collection that will store
                    // alarm events to be sent into the system
                    alarmEvents.Clear();
                    processedMeasurements = 0L;

                    measurements = m_measurementQueue.Dequeue();

                    foreach (IMeasurement measurement in measurements)
                    {
                        lock (m_alarms)
                        {
                            // Get alarms that apply to the measurement being processed
                            if (!m_alarmLookup.TryGetValue(measurement.ID, out alarms))
                                alarms = new List<Alarm>();

                            // Get alarms that triggered events
                            raisedAlarms = alarms.Where(a => a.Test(measurement)).ToList();
                        }

                        // Create event measurements to be sent into the system
                        foreach (Alarm alarm in raisedAlarms)
                        {
                            alarmEvent = new Measurement
                            {
                                Timestamp = measurement.Timestamp,
                                Value = (int)alarm.State
                            };

                            if ((object)alarm.AssociatedMeasurementID != null)
                            {
                                alarmEvent.ID = alarm.AssociatedMeasurementID.GetValueOrDefault();
                                alarmEvent.Key = MeasurementKey.LookupBySignalID(alarmEvent.ID);
                            }

                            alarmEvents.Add(alarmEvent);
                            m_eventCount++;
                        }

                        // Increment processed measurement count
                        processedMeasurements++;
                    }

                    // Send new alarm events into the system,
                    // then reset the collection for the next
                    // group of measurements
                    OnNewMeasurements(alarmEvents);

                    // Increment total count of processed measurements
                    IncrementProcessedMeasurements(processedMeasurements);

                    if (measurements.Any())
                        spinner.Reset();
                    else
                        spinner.SpinOnce();
                }
                catch (Exception ex)
                {
                    // Log error and continue processing alarm events
                    string message = string.Format("Exception occurred while processing alarm measurements: {0}", ex.Message);
                    OnProcessException(new InvalidOperationException(message, ex));
                    spinner.SpinOnce();
                }
            }
        }

        // Get all the highest severity alarms in a list of alarms
        private IEnumerable<Alarm> FilterToHighestSeverity(IEnumerable<Alarm> alarms)
        {
            AlarmSeverity highestSeverity = alarms.Max(alarm => alarm.Severity);
            return alarms.Where(alarm => alarm.Severity == highestSeverity);
        }

        // Processes excpetions thrown by the alarm service.
        private void AlarmService_ServiceProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        // Processes exceptions thrown during measurement processing
        private void m_measurementQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        #endregion
    }
}
