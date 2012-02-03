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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Data;

namespace DataQualityMonitoring
{
    public class AlarmAdapter : FacileActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private List<Alarm> m_alarms;
        private ConcurrentQueue<IMeasurement> m_measurementQueue;
        private Thread m_processThread;
        private Semaphore m_processSemaphore;
        private long m_eventCount;
        private bool m_supportsTemporalProcessing;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return m_supportsTemporalProcessing;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="AlarmAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            string filterExpression;

            // Run base class initialization
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameter
            if (settings.TryGetValue("supportsTemporalProcessing", out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = false;

            // Create alarms using definitions from the database
            m_alarms = DataSource.Tables["Alarms"].Rows.Cast<DataRow>()
                .Where(row => row.ConvertField<uint>("AdapterID") == ID)
                .Select(row => CreateAlarm(row))
                .ToList();

            // Generate filter expression for input measurements
            filterExpression = m_alarms.Select(a => a.SignalID)
                .Distinct()
                .Select(id => id.ToString())
                .Aggregate((list, id) => list + ";" + id);

            // Set input measurement keys for measurement routing
            InputMeasurementKeys = ParseInputMeasurementKeys(DataSource, filterExpression);
        }

        /// <summary>
        /// Starts the <see cref="AlarmAdapter"/>, or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_measurementQueue = new ConcurrentQueue<IMeasurement>();
            m_processThread = new Thread(ProcessMeasurements);
            m_processSemaphore = new Semaphore(0, int.MaxValue);
            m_eventCount = 0L;

            m_processThread.Start();
        }

        /// <summary>
        /// Stops the <see cref="AlarmAdapter"/>.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if ((object)m_processSemaphore != null)
            {
                m_processSemaphore.Dispose();
                m_processSemaphore = null;
            }

            if ((object)m_processThread != null && m_processThread.ThreadState == ThreadState.Running)
            {
                m_processThread.Join();
                m_processThread = null;
            }
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            base.QueueMeasurementsForProcessing(measurements);

            foreach (IMeasurement measurement in measurements)
            {
                m_measurementQueue.Enqueue(measurement);
                m_processSemaphore.Release();
            }
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

        // Creates an alarm using data defined in the database.
        private Alarm CreateAlarm(DataRow row)
        {
            return new Alarm()
            {
                ExpressionText = row["ExpressionText"].ToString(),
                ID = System.Guid.Parse(row.Field<object>("ID").ToString()),
                SignalID = System.Guid.Parse(row.Field<object>("SignalID").ToString())
            };
        }

        // Processes measurements in the queue.
        private void ProcessMeasurements()
        {
            IMeasurement measurement, clone;
            IEnumerable<Alarm> events;

            while (Enabled)
            {
                if (m_processSemaphore.WaitOne() && m_measurementQueue.TryDequeue(out measurement))
                {
                    // Get alarms that triggered events
                    events = m_alarms.Where(a => a.SignalID == measurement.ID)
                        .Where(a => a.Condition(measurement));

                    // Create event measurements and send them into the system
                    foreach (Alarm a in events)
                    {
                        clone = Measurement.Clone(measurement);
                        clone.ID = a.ID;
                        OnNewMeasurement(clone);
                        m_eventCount++;
                    }
                }
            }
        }

        // Helper method to raise the NewMeasurements event
        // when only a single measurement is to be provided.
        private void OnNewMeasurement(IMeasurement measurement)
        {
            OnNewMeasurements(new IMeasurement[] { measurement });
        }

        #endregion
    }
}
