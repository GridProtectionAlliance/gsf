//******************************************************************************************************
//  AlarmAdapter.cs - Gbtc
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
//  01/31/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using DataQualityMonitoring.Services;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Collections;
using TVA.Data;

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

        private readonly AsyncQueue<IEnumerable<IMeasurement>> m_measurementQueue;
        private long m_eventCount;

        private bool m_supportsTemporalProcessing;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AlarmAdapter"/>.
        /// </summary>
        public AlarmAdapter()
        {
            m_measurementQueue = new AsyncQueue<IEnumerable<IMeasurement>>()
            {
                ProcessItemFunction = ProcessMeasurements
            };

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
                m_alarmLookup.Add(signalID, m_alarms.Where(alarm => alarm.SignalID == signalID).ToList());

            if (m_alarms.Count > 0)
            {
                // Generate filter expression for input measurements
                filterExpression = m_alarms.Select(a => a.SignalID)
                    .Distinct()
                    .Select(id => id.ToString())
                    .Aggregate((list, id) => list + ";" + id);

                // Set input measurement keys for measurement routing
                InputMeasurementKeys = ParseInputMeasurementKeys(DataSource, filterExpression);
            }

            // Set up alarm service
            m_alarmService = new AlarmService(this);
            m_alarmService.SettingsCategory = base.Name.Replace("!", "").ToLower() + m_alarmService.SettingsCategory;
            m_alarmService.ServiceProcessException += AlarmService_ServiceProcessException;
            m_alarmService.PersistSettings = true;
            m_alarmService.Initialize();
        }

        /// <summary>
        /// Starts the <see cref="AlarmAdapter"/>, or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_eventCount = 0L;
            m_measurementQueue.Enabled = true;
        }

        /// <summary>
        /// Stops the <see cref="AlarmAdapter"/>.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            m_measurementQueue.Enabled = false;
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

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            //base.QueueMeasurementsForProcessing(measurements);
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
        public ICollection<Alarm> GetRaisedAlarms()
        {
            lock (m_alarms)
            {
                return m_alarms.Where(alarm => alarm.State == AlarmState.Raised)
                    .ToList();
            }
        }

        // Creates an alarm using data defined in the database.
        private Alarm CreateAlarm(DataRow row)
        {
            object associatedMeasurementId = row.Field<object>("AssociatedMeasurementID");

            return new Alarm()
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
        private void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            List<Alarm> alarms;
            List<Alarm> raisedAlarms;

            List<IMeasurement> alarmEvents;
            IMeasurement alarmEvent;
            long processedMeasurements;

            // Create the collection that will store
            // alarm events to be sent into the system
            alarmEvents = new List<IMeasurement>();
            processedMeasurements = 0L;

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
                    alarmEvent = new Measurement()
                    {
                        Timestamp = measurement.Timestamp,
                        Value = (int)alarm.State
                    };

                    if ((object)alarm.AssociatedMeasurementID != null)
                    {
                        alarmEvent.ID = alarm.AssociatedMeasurementID.Value;
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
        }

        // Processes exceptions thrown by the alarm service.
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
