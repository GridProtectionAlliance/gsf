#region [ Modification History ]
/*
 * 07/22/2012 Denis Kholine
 *  Generated Original version of source code.
 */
#endregion

#region  [ UIUC NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois>
All rights reserved.

Developed by: <ITI>
<University of Illinois>
<http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
• Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/
#endregion

#region [ Using ]
using System;
using System.Linq;
using System.Net;
using System.Xml;
using System.Data.SqlClient;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using GSF.Data;
using System.Threading;
using GSF.TimeSeries.Routing;
using GSF.Units;
using GSF.Adapters;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Action adapter that generates alarm measurements based on alarm definitions from the database.
    /// </summary>
    [Description("Alarm: Generates alarm events for alarms defined in the database.")]
    public class IActionAdapterCase : FacileActionAdapterBase
    {
        #region [ Members ]
        // Constants
        private const int WaitTimeout = 1000;

        // Fields
        private List<Alarm> m_alarms;
        //private AlarmService m_alarmService;

        private bool m_disposed;
        private long m_eventCount;
        private ConcurrentQueue<IMeasurement> m_measurementQueue;
        private SemaphoreSlim m_processSemaphore;
        private Thread m_processThread;
        private bool m_supportsTemporalProcessing;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                /*
                StringBuilder statusBuilder = new StringBuilder(base.Status);
                statusBuilder.Append(m_alarmService.Status);
                return statusBuilder.ToString();
                 * */
                return "";
            }
        }

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

        #endregion

        #region [ Methods ]
        /// <summary>
        /// Gets a collection containing all the raised alarms in the system.
        /// </summary>
        /// <returns>A collection containing all the raised alarms.</returns>
        public ICollection<Alarm> GetRaisedAlarms()
        {
            lock (m_alarms)
            {
                /*
                return m_alarms.Where(alarm => alarm.State == AlarmState.Raised)
                    .ToList();
                 */
                return m_alarms;
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

        /// <summary>
        /// Initializes the <see cref="AlarmAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings;
            string setting;

            // string filterExpression;

            // Run base class initialization
            base.Initialize();
            settings = Settings;

            // Load optional parameters
            if (settings.TryGetValue("supportsTemporalProcessing", out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = false;

            /**/
            // Create alarms using definitions from the database
            m_alarms = DataSource.Tables["Alarms"].Rows.Cast<DataRow>()
                .Where(row => row.ConvertField<bool>("Enabled"))
                .Select(row => CreateAlarm(row))
                .ToList();
            /*
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
             */
        }

        ///// <summary>
        ///// Queues a collection of measurements for processing.
        ///// </summary>
        ///// <param name="measurements">Measurements to queue for processing.</param>
        //public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        //{
        //    base.QueueMeasurementsForProcessing(measurements);

        //    foreach (IMeasurement measurement in measurements)
        //    {
        //        m_measurementQueue.Enqueue(measurement);
        //        m_processSemaphore.Release();
        //    }
        //}

        /// <summary>
        /// Starts the <see cref="AlarmAdapter"/>, or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_measurementQueue = new ConcurrentQueue<IMeasurement>();
            m_processThread = new Thread(ProcessMeasurements);
            m_processSemaphore = new SemaphoreSlim(0, int.MaxValue);
            m_eventCount = 0L;

            m_processThread.Start();
        }

        /// <summary>
        /// Stops the <see cref="AlarmAdapter"/>.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if ((object)m_processThread != null)
            {
                m_processThread.Join();
                m_processThread = null;
            }

            if ((object)m_processSemaphore != null)
            {
                m_processSemaphore.Dispose();
                m_processSemaphore = null;
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
                        // if (m_alarmService != null)
                        // {
                        /*
                        m_alarmService.ServiceProcessException -= AlarmService_ServiceProcessException;
                        m_alarmService.Dispose();
                         */
                        // }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        // Processes excpetions thrown by the alarm service.
        private void AlarmService_ServiceProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        // Creates an alarm using data defined in the database.
        private Alarm CreateAlarm(DataRow row)
        {
            object associatedMeasurementId = row.ConvertField<object>("AssociatedMeasurementID");

            return new Alarm()
            {
                ID = row.ConvertField<int>("ID"),
                TagName = row.ConvertField <object>("TagName").ToString(),
                SignalID = Guid.Parse(row.ConvertField<object>("SignalID").ToString()),
                AssociatedMeasurementID = ((object)associatedMeasurementId != null) ? Guid.Parse(associatedMeasurementId.ToString()) : (Guid?)null,
                Description = row.ConvertField<object>("Description").ToNonNullString(),
                Severity = (AlarmSeverity)row.ConvertField<int>("Severity"),
                Operation = (AlarmOperation)row.ConvertField<int>("Operation"),
                SetPoint = row.ConvertNullableField<double>("SetPoint"),
                Tolerance = row.ConvertNullableField<double>("Tolerance"),
                Delay = row.ConvertNullableField<double>("Delay"),
                Hysteresis = row.ConvertNullableField<double>("Hysteresis")
            };
        }

        // Helper method to raise the NewMeasurements event
        // when only a single measurement is to be provided.
        //private void OnNewMeasurement(IMeasurement measurement)
        //{
        //    NewEntities(this,new RoutingEventArgs()); 
        //}

        // Processes measurements in the queue.
        private void ProcessMeasurements()
        {
            IMeasurement measurement, alarmEvent;
            List<Alarm> events;

            while (Enabled)
            {
                try
                {
                    if ((object)m_processSemaphore != null && m_processSemaphore.Wait(WaitTimeout) && m_measurementQueue.TryDequeue(out measurement))
                    {
                        lock (m_alarms)
                        {
                            events = m_alarms;
                            /*
                            // Get alarms that triggered events
                            events = m_alarms.Where(a => a.SignalID == measurement.ID)
                                .Where(a => a.Test(measurement))
                                .ToList();*/
                        }

                        // Create event measurements and send them into the system
                        foreach (Alarm alarm in events)
                        {
                            //alarmEvent = new Measurement<double>()
                            //{
                           //     Timestamp = measurement.Timestamp,
                           //     Value = (int)alarm.State
                            //};

                            //if ((object)alarm.AssociatedMeasurementID != null)
                              //  alarmEvent.ID = alarm.AssociatedMeasurementID.Value;

                            //NewEntities(this, new RoutingEventArgs()); 

                            //OnNewMeasurement(alarmEvent);
                          //  m_eventCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(ex);
                }
            }
        }

        #endregion
    }
}