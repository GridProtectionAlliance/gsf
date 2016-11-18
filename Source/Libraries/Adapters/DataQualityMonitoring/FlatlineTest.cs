//******************************************************************************************************
//  FlatlineTest.cs - Gbtc
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
//  11/10/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/11/2009 - Pinal C. Patel
//       Added thread synchronization.
//       Replaced the use of LatestTimestamp with base.RealTime.
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
using System.Timers;
using DataQualityMonitoring.Services;
using GSF;
using GSF.Net.Smtp;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Tests measurements to determine whether they have flat-lined.
    /// </summary>
    [Description("Flat-line Test: Notifies when measurements are received whose values do not change")]
    public class FlatlineTest : ActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private Ticks m_minFlatline;
        private Ticks m_warnInterval;
        private Ticks m_emailInterval;
        private string m_adminEmailAddress;
        private string m_smtpServer;

        private readonly Dictionary<MeasurementKey, IMeasurement> m_lastChange;
        private readonly Dictionary<MeasurementKey, Ticks> m_lastNotified;
        private readonly Timer m_warningTimer;
        private FlatlineService m_flatlineService;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FlatlineTest"/> class.
        /// </summary>
        public FlatlineTest()
        {
            m_minFlatline = Ticks.FromSeconds(4);
            m_warnInterval = Ticks.FromSeconds(4);
            m_emailInterval = Ticks.FromSeconds(3600);
            m_smtpServer = Mail.DefaultSmtpServer;

            m_lastChange = new Dictionary<MeasurementKey, IMeasurement>();
            m_lastNotified = new Dictionary<MeasurementKey, Ticks>();
            m_warningTimer = new Timer();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the length of time, in seconds, that a measurement's value must remain stale to be considered flat-lined.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the length of time, in seconds, that a measurement's value must remain stale to be considered flat-lined."),
        DefaultValue(4)]
        public double MinFlatline
        {
            get
            {
                return m_minFlatline.ToSeconds();
            }
            set
            {
                m_minFlatline = Ticks.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets the amount of time, in seconds, between console updates.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the amount of time, in seconds, between console updates."),
        DefaultValue(4)]
        public double WarnInterval
        {
            get
            {
                return m_warnInterval.ToSeconds();
            }
            set
            {
                m_warnInterval = Ticks.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets the email address or address list to which notifications will be sent.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the email address or address list to which notifications will be sent."),
        DefaultValue(null)]
        public string AdminEmailAddress
        {
            get
            {
                return m_adminEmailAddress;
            }
            set
            {
                m_adminEmailAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of time, in minutes, between email notifications.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the amount of time, in minutes, between email notifications."),
        DefaultValue(60)]
        public long EmailInterval
        {
            get
            {
                return (long)m_emailInterval.ToSeconds() / 60L;
            }
            set
            {
                m_emailInterval = Ticks.FromSeconds(value * 60L);
            }
        }

        /// <summary>
        /// Gets or sets the host name or IP address of the SMTP server used to send email notifications.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the host name or IP address of the SMTP server used to send email notifications."),
        DefaultValue(Mail.DefaultSmtpServer)]
        public string SmtpServer
        {
            get
            {
                return m_smtpServer;
            }
            set
            {
                m_smtpServer = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="FlatlineTest"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue("minFlatline", out setting))
                m_minFlatline = Ticks.FromSeconds(double.Parse(setting));

            if (settings.TryGetValue("warnInterval", out setting))
                m_warnInterval = Ticks.FromSeconds(double.Parse(setting));

            if (settings.TryGetValue("adminEmailAddress", out m_adminEmailAddress))
            {
                // emailInterval is entered in minutes.
                if (settings.TryGetValue("emailInterval", out setting))
                    m_emailInterval = Ticks.FromSeconds(long.Parse(setting) * 60L);
            }

            if (settings.TryGetValue("smtpServer", out setting))
                m_smtpServer = setting;

            m_warningTimer.Interval = m_warnInterval.ToMilliseconds();
            m_warningTimer.Elapsed += m_warningTimer_Elapsed;

            m_flatlineService = new FlatlineService(this);
            m_flatlineService.SettingsCategory = base.Name + m_flatlineService.SettingsCategory;
            m_flatlineService.ServiceProcessException += m_flatlineService_ServiceProcessException;
            m_flatlineService.Initialize();

            InitializeLastChange();
        }

        /// <summary>
        /// Starts the <see cref="FlatlineTest"/>, if it is not already running.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_warningTimer.Start();
        }

        /// <summary>
        /// Stops the <see cref="FlatlineTest"/>.
        /// </summary>
        public override void Stop()
        {
            m_warningTimer.Stop();
            base.Stop();
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IMeasurement measurement;

            lock (m_lastChange)
            {
                foreach (MeasurementKey key in frame.Measurements.Keys)
                {
                    measurement = frame.Measurements[key];

                    if (!m_lastChange.ContainsKey(key))
                        m_lastChange.Add(key, measurement);
                    else if (m_lastChange[key].Value != measurement.Value)
                        m_lastChange[key] = measurement;
                }
            }

            SendEmailNotifications();
        }

        /// <summary>
        /// Returns a collection of measurements that are flat-lined.
        /// </summary>
        /// <returns>A collection of flat-lined measurements.</returns>
        public ICollection<IMeasurement> GetFlatlinedMeasurements()
        {
            ICollection<IMeasurement> flatlinedMeasurements = new List<IMeasurement>();

            lock (m_lastChange)
            {
                IMeasurement measurement;

                foreach (MeasurementKey key in m_lastChange.Keys)
                {
                    measurement = m_lastChange[key];

                    Ticks timeDiff = base.RealTime - measurement.Timestamp;
                    if (timeDiff >= m_minFlatline)
                        flatlinedMeasurements.Add(measurement);
                }
            }

            return flatlinedMeasurements;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FlatlineTest"/> object and optionally releases the managed resources.
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
                        if (m_flatlineService != null)
                        {
                            m_flatlineService.ServiceProcessException -= m_flatlineService_ServiceProcessException;
                            m_flatlineService.Dispose();
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

        // Initialize m_lastChange with dummy measurements so the adapter
        // can detect measurements that have never been published
        private void InitializeLastChange()
        {
            Ticks timestamp = base.RealTime;

            foreach (MeasurementKey key in InputMeasurementKeys)
            {
                // Try to find the signal ID and add the measurement to the m_lastChange collection
                if (DataSource.Tables.Contains("ActiveMeasurements"))
                {
                    DataRow[] measurementRows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("ID = '{0}'", key.ToString()));

                    if (measurementRows.Length > 0)
                    {
                        string signalID = measurementRows[0]["SignalID"].ToNonNullString();

                        if (signalID != "")
                        {
                            m_lastChange.Add(key, new Measurement
                            {
                                //Assumption made that key.SignalID == signalId
                                CommonMeasurementFields = key.DefaultCommonMeasurementFields,
                                Value = double.NaN,
                                Timestamp = timestamp
                            });
                        }
                    }
                }

                // If the signal ID could not be found, add the measurement without it
                if (!m_lastChange.ContainsKey(key))
                {
                    m_lastChange.Add(key, new Measurement
                    {
                        CommonMeasurementFields = key.DefaultCommonMeasurementFields,
                        Value = double.NaN,
                        Timestamp = timestamp
                    });
                }
            }
        }

        // Sends email notifications about changes in the flat-lined status of measurements.
        private void SendEmailNotifications()
        {
            Ticks now = DateTime.UtcNow.Ticks;
            ICollection<IMeasurement> allFlatlinedMeasurements = GetFlatlinedMeasurements();
            IEnumerable<IMeasurement> flatlined, noLongerFlatlined;

            lock (m_lastChange)
            {
                flatlined = m_lastNotified
                    .Where(pair => now - pair.Value > m_emailInterval)
                    .Select(pair => m_lastChange[pair.Key])
                    .Concat(allFlatlinedMeasurements.Where(measurement => !m_lastNotified.Keys.Contains(measurement.Key)))
                    .ToList();

                noLongerFlatlined = m_lastNotified
                    .Where(pair => allFlatlinedMeasurements.All(measurement => pair.Key != measurement.Key))
                    .Select(pair => m_lastChange[pair.Key])
                    .ToList();
            }

            if (flatlined.Any())
                SendEmailNotification(flatlined, true);

            if (noLongerFlatlined.Any())
                SendEmailNotification(noLongerFlatlined, false);
        }

        // Send an email address notifying the admin of a changes in the flat-lined status of measurements.
        private void SendEmailNotification(IEnumerable<IMeasurement> measurements, bool flatlined)
        {
            Ticks now = DateTime.Now.Ticks;
            Mail message = new Mail("notifications@company.com", m_adminEmailAddress, m_smtpServer);
            StringBuilder body = new StringBuilder();

            body.AppendLine("Measurement Key, Value, Timestamp");

            foreach (IMeasurement measurement in measurements)
            {
                body.Append(measurement.Key);
                body.Append(", ");
                body.Append(measurement.AdjustedValue);
                body.Append(", ");
                body.Append(measurement.Timestamp);
                body.AppendLine();

                if (flatlined)
                    m_lastNotified[measurement.Key] = now;
                else
                    m_lastNotified.Remove(measurement.Key);
            }

            if (flatlined)
                message.Subject = "Flat-lined measurements";
            else
                message.Subject = "No longer flat-lined measurements";

            message.Body = body.ToString();
            message.Send();
            message.Dispose();
        }

        private void m_warningTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ICollection<IMeasurement> flatlinedMeasurements = GetFlatlinedMeasurements();

            foreach (IMeasurement measurement in flatlinedMeasurements)
            {
                Ticks timeDiff = RealTime - measurement.Timestamp;
                OnStatusMessage(string.Format("{0} flat-lined for {1} seconds.", measurement, (int)timeDiff.ToSeconds()));
            }
        }

        private void m_flatlineService_ServiceProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        #endregion
    }
}
