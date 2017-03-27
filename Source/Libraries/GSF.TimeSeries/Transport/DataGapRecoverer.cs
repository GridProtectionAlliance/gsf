//******************************************************************************************************
//  DataGapRecoverer.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/27/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries.Adapters;
using GSF.Units;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a data gap recovery module. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Data gaps will be recovered using an unsynchronized temporal subscription.
    /// </para>
    /// <para>
    /// This class expects that source historian that feeds temporal subscription
    /// will recover data in time-sorted order.
    /// </para>
    /// </remarks>
    public class DataGapRecoverer : ISupportLifecycle, IProvideStatus
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="RecoveryStartDelay"/>.
        /// </summary>
        public const double DefaultRecoveryStartDelay = 20.0D;

        /// <summary>
        /// Default value for <see cref="DataMonitoringInterval"/>.
        /// </summary>
        public const double DefaultDataMonitoringInterval = 10.0D;

        /// <summary>
        /// Default value for <see cref="MinimumRecoverySpan"/>.
        /// </summary>
        public const double DefaultMinimumRecoverySpan = 30.0D;

        /// <summary>
        /// Default value for <see cref="MaximumRecoverySpan"/>.
        /// </summary>
        public const double DefaultMaximumRecoverySpan = Time.SecondsPerDay * 10;

        /// <summary>
        /// Default value for <see cref="FilterExpression"/>.
        /// </summary>
        public const string DefaultFilterExpression = "FILTER ActiveMeasurements WHERE Internal <> 0";

        /// <summary>
        /// Default value for <see cref="RecoveryProcessingInterval"/>.
        /// </summary>
        public const int DefaultRecoveryProcessingInterval = 66;

        /// <summary>
        /// Default value for <see cref="UseMillisecondResolution"/>.
        /// </summary>
        public const bool DefaultUseMillisecondResolution = true;

        /// <summary>
        /// Default value for <see cref="StartRecoveryBuffer"/>.
        /// </summary>
        public const double DefaultStartRecoveryBuffer = -20.0D;

        /// <summary>
        /// Default value for <see cref="EndRecoveryBuffer"/>.
        /// </summary>
        public const double DefaultEndRecoveryBuffer = 10.0D;

        // Events

        /// <summary>
        /// Provides recovered measurements from temporal subscription.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of measurements for consumer to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> RecoveredMeasurements;

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered during <see cref="DataGapRecoverer"/> processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Raised after the <see cref="DataGapRecoverer"/> has been properly disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private readonly UnsynchronizedSubscriptionInfo m_subscriptionInfo;
        private readonly ManualResetEventSlim m_dataGapRecoveryCompleted;
        private DataSubscriber m_temporalSubscription;
        private OutageLogProcessor m_dataGapLogProcessor;
        private OutageLog m_dataGapLog;
        private SharedTimer m_dataStreamMonitor;
        private DataSet m_dataSource;
        private string m_loggingPath;
        private string m_sourceConnectionName;
        private string m_connectionString;
        private Time m_recoveryStartDelay;
        private Time m_minimumRecoverySpan;
        private Time m_maximumRecoverySpan;
        private Outage m_currentDataGap;
        private Ticks m_mostRecentRecoveredTime;
        private long m_measurementsRecoveredForDataGap;
        private long m_measurementsRecoveredOverLastInterval;
        private volatile bool m_abnormalTermination;
        private volatile bool m_enabled;
        private volatile bool m_connected;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataGapRecoverer"/>.
        /// </summary>
        public DataGapRecoverer()
        {
            Log = Logger.CreatePublisher(GetType(), MessageClass.Framework);
            Log.InitialStackMessages = Log.InitialStackMessages.Union("ComponentName", GetType().Name);

            m_dataGapRecoveryCompleted = new ManualResetEventSlim(true);

            m_recoveryStartDelay = DefaultRecoveryStartDelay;
            m_minimumRecoverySpan = DefaultMinimumRecoverySpan;
            m_maximumRecoverySpan = DefaultMaximumRecoverySpan;
            StartRecoveryBuffer = DefaultStartRecoveryBuffer;
            EndRecoveryBuffer = DefaultEndRecoveryBuffer;

            string loggingPath = FilePath.GetDirectoryName(FilePath.GetAbsolutePath(DataSubscriber.DefaultLoggingPath));

            if (Directory.Exists(loggingPath))
                m_loggingPath = loggingPath;

            m_subscriptionInfo = new UnsynchronizedSubscriptionInfo(false);
            m_subscriptionInfo.FilterExpression = DefaultFilterExpression;
            m_subscriptionInfo.ProcessingInterval = DefaultRecoveryProcessingInterval;
            m_subscriptionInfo.UseMillisecondResolution = DefaultUseMillisecondResolution;

            m_dataStreamMonitor = Common.TimerScheduler.CreateTimer((int)(DefaultDataMonitoringInterval * 1000.0D));
            m_dataStreamMonitor.Elapsed += DataStreamMonitor_Elapsed;
            m_dataStreamMonitor.AutoReset = true;
            m_dataStreamMonitor.Enabled = false;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="DataGapRecoverer"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~DataGapRecoverer()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Log messages generated by an adapter.
        /// </summary>
        protected LogPublisher Log { get; }

        /// <summary>
        /// Gets or sets name of source connection device (e.g., a data subscriber).
        /// </summary>
        /// <remarks>
        /// This name will be used to create the <see cref="OutageLog.FileName"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be null or an empty string.</exception>
        public string SourceConnectionName
        {
            get
            {
                return m_sourceConnectionName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "SourceConnectionName cannot be null or an empty string.");

                m_sourceConnectionName = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="DataGapRecoverer"/>.
        /// </summary>
        public DataSet DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                m_dataSource = value;

                if ((object)m_temporalSubscription != null)
                    m_temporalSubscription.DataSource = m_dataSource;
            }
        }

        /// <summary>
        /// Gets or sets connection string that will be used to make a temporal subscription when recovering data for an <see cref="Outage"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be null or an empty string.</exception>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "ConnectionString cannot be null or an empty string.");

                m_connectionString = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum time delay, in seconds, to wait before starting the data recovery for an <see cref="Outage"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For some archiving systems it may take a few seconds for data to make it to disk and therefore be readily
        /// available for a temporal subscription query. The <see cref="RecoveryStartDelay"/> should be adjusted based
        /// on the nature of the system used to archive data. If the archival system makes data immediately available
        /// because of internal caching or other means, this value can be zero.
        /// </para>
        /// <para>
        /// Use of this value depends on the local clock, as such the value should be increased by the uncertainty of
        /// accuracy of the local clock. For example, if it is know that the local clock floats +/-5 seconds from
        /// real-time, then increase the desired value of the <see cref="RecoveryStartDelay"/> by 5 seconds.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be a negative number.</exception>
        public Time RecoveryStartDelay
        {
            get
            {
                return m_recoveryStartDelay;
            }
            set
            {
                if (value < 0.0D)
                    throw new ArgumentOutOfRangeException(nameof(value), "RecoveryStartDelay cannot be a negative number.");

                m_recoveryStartDelay = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval, in seconds, over which the data monitor will check for new data.
        /// </summary>
        /// <remarks>
        /// Once a connection is established a timer is enabled to monitor for new incoming data. The data monitoring timer
        /// exists to make sure data is being received so that the process of recovery does not wait endlessly for data that
        /// may never come because of a possible error in the recovery process. The <see cref="DataMonitoringInterval"/>
        /// allows the consumer to adjust the interval over which the timer will check for new incoming data.
        /// <para>
        /// It will take some time, perhaps a couple of seconds, to start the temporal subscription and begin the process
        /// of recovering data for an <see cref="Outage"/>. Make sure the value for <see cref="DataMonitoringInterval"/> is
        /// sufficiently large enough to handle any initial delays in data transmission.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be zero or a negative number.</exception>
        public Time DataMonitoringInterval
        {
            get
            {
                return m_dataStreamMonitor.Interval / 1000.0D;
            }
            set
            {
                if ((object)m_dataStreamMonitor == null)
                    throw new ArgumentNullException();

                if (value <= 0.0D)
                    throw new ArgumentOutOfRangeException(nameof(value), "DataMonitoringInterval cannot be zero or a negative number.");

                m_dataStreamMonitor.Interval = (int)(value * 1000.0D);
            }
        }

        /// <summary>
        /// Gets to sets the minimum time span, in seconds, for which a data recovery will be attempted.
        /// Set to zero for no minimum.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be a negative number.</exception>
        public Time MinimumRecoverySpan
        {
            get
            {
                return m_minimumRecoverySpan;
            }
            set
            {
                if (value < 0.0D)
                    throw new ArgumentOutOfRangeException(nameof(value), "MinimumRecoverySpan cannot be a negative number.");

                m_minimumRecoverySpan = value;
            }
        }

        /// <summary>
        /// Gets to sets the maximum time span, in seconds, for which a data recovery will be attempted.
        /// Set to <see cref="Double.MaxValue"/> for no maximum.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be zero or a negative number.</exception>
        public Time MaximumRecoverySpan
        {
            get
            {
                return m_maximumRecoverySpan;
            }
            set
            {
                if (value <= 0.0D)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaximumRecoverySpan cannot be zero or a negative number.");

                m_maximumRecoverySpan = value;
            }
        }

        /// <summary>
        /// Gets or sets logging path to be used to be runtime and outage logs of the subscriber which are required for
        /// automated data recovery.
        /// </summary>
        /// <remarks>
        /// Leave value blank for default path, i.e., installation folder. Can be a fully qualified path or a path that
        /// is relative to the installation folder, e.g., a value of "ConfigurationCache" might resolve to
        /// "C:\Program Files\MyTimeSeriespPp\ConfigurationCache\".
        /// </remarks>
        public string LoggingPath
        {
            get
            {
                return m_loggingPath;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    string loggingPath = FilePath.GetDirectoryName(FilePath.GetAbsolutePath(value));

                    if (Directory.Exists(loggingPath))
                        value = loggingPath;
                }

                m_loggingPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the filter expression used to define which measurements are being requested for data recovery during an <see cref="Outage"/>.
        /// </summary>
        public string FilterExpression
        {
            get
            {
                return m_subscriptionInfo.FilterExpression;
            }
            set
            {
                m_subscriptionInfo.FilterExpression = value;
            }
        }

        /// <summary>
        /// Gets or sets the data recovery processing interval, in whole milliseconds, to use in the temporal data
        /// subscription when recovering data for an <see cref="Outage"/>.<br/>
        /// A value of <c>-1</c> indicates the default processing interval will be requested.<br/>
        /// A value of <c>0</c> indicates data will be processed as fast as possible.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, the <see cref="RecoveryProcessingInterval"/> value specifies
        /// the desired historical data playback processing interval in milliseconds. This is basically a delay, or timer
        /// interval, over which to process data. Setting this value to -1 means to use the default processing interval
        /// while setting the value to 0 means to process data as fast as possible, i.e., as fast as the historian can
        /// query the data. Depending on the available bandwidth, this parameter may need to be adjusted such that the
        /// data being recovered does not adversely interfere with the ongoing transmission of real-time data.
        /// </remarks>
        public int RecoveryProcessingInterval
        {
            get
            {
                return m_subscriptionInfo.ProcessingInterval;
            }
            set
            {
                m_subscriptionInfo.ProcessingInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether measurement timestamps use millisecond resolution.
        /// If false, time will be of <see cref="Ticks"/> resolution.
        /// </summary>
        /// <remarks>
        /// If the source and destination historians can handle timestamps at a greater than millisecond resolution then
        /// the <see cref="UseMillisecondResolution"/> can be set to <c>false</c> to ensure that a full resolution timestamp
        /// is delivered through the data recovery process. Setting this property to <c>true</c> allows the temporal subscription
        /// used in the data recovery process to conserve data transmission bandwidth since not as much space will be needed for
        /// a timestamp with only millisecond resolution.
        /// </remarks>
        public bool UseMillisecondResolution
        {
            get
            {
                return m_subscriptionInfo.UseMillisecondResolution;
            }
            set
            {
                m_subscriptionInfo.UseMillisecondResolution = value;
            }
        }

        /// <summary>
        /// Gets or sets start buffer time, in seconds, to add to start of outage window to ensure all missing data is recovered.
        /// </summary>
        public double StartRecoveryBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets end buffer time, in seconds, to add to end of outage window to ensure all missing data is recovered.
        /// </summary>
        public double EndRecoveryBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any additional constraint parameters that will be supplied to adapters in temporal
        /// subscription used when recovering data for an <see cref="Outage"/>.
        /// </summary>
        public string ConstraintParameters
        {
            get
            {
                return m_subscriptionInfo.ConstraintParameters;
            }
            set
            {
                m_subscriptionInfo.ConstraintParameters = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="DataGapRecoverer"/> is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;

                if ((object)m_dataGapLogProcessor != null)
                    m_dataGapLogProcessor.Enabled = m_enabled;

                if ((object)m_temporalSubscription != null)
                    m_temporalSubscription.Enabled = m_enabled;
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        public bool IsDisposed => m_disposed;

        /// <summary>
        /// Gets reference to the data gap <see cref="OutageLog"/> for this <see cref="DataGapRecoverer"/>.
        /// </summary>
        protected OutageLog DataGapLog => m_dataGapLog;

        /// <summary>
        /// Gets reference to the data gap <see cref="OutageLogProcessor"/> for this <see cref="DataGapRecoverer"/>.
        /// </summary>
        protected OutageLogProcessor DataGapLogProcessor => m_dataGapLogProcessor;

        // Gets the name of the data gap recoverer.
        string IProvideStatus.Name
        {
            get
            {
                if ((object)m_temporalSubscription != null)
                    return m_temporalSubscription.Name;

                return GetType().Name;
            }
        }

        /// <summary>
        /// Gets the status of the temporal <see cref="DataSubscriber"/> used to query historical data.
        /// </summary>
        public string TemporalSubscriptionStatus => (object)m_temporalSubscription != null ? m_temporalSubscription.Status : "undefined";

        /// <summary>
        /// Gets the status of this <see cref="DataGapRecoverer"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat(" Data recovery start delay: {0}", RecoveryStartDelay.ToString(2));
                status.AppendLine();
                status.AppendFormat("  Data monitoring interval: {0}", DataMonitoringInterval.ToString(2));
                status.AppendLine();
                status.AppendFormat("Minimum data recovery span: {0}", MinimumRecoverySpan.ToString(2));
                status.AppendLine();
                status.AppendFormat("Maximum data recovery span: {0}", MaximumRecoverySpan.ToString(2));
                status.AppendLine();
                status.AppendFormat("Recovery filter expression: {0}", FilterExpression.TruncateRight(51));
                status.AppendLine();
                status.AppendFormat(" Recovery processing speed: {0}", RecoveryProcessingInterval < 0 ? "Default" : (RecoveryProcessingInterval == 0 ? "As fast as possible" : RecoveryProcessingInterval.ToString("N0") + " milliseconds"));
                status.AppendLine();
                status.AppendFormat("Use millisecond resolution: {0}", UseMillisecondResolution);
                status.AppendLine();
                status.AppendFormat("     Start recovery buffer: {0:N2} seconds", StartRecoveryBuffer);
                status.AppendLine();
                status.AppendFormat("       End recovery buffer: {0:N2} seconds", EndRecoveryBuffer);
                status.AppendLine();
                status.AppendFormat("              Logging path: {0}", FilePath.TrimFileName(m_loggingPath.ToNonNullNorWhiteSpace(FilePath.GetAbsolutePath("")), 51));
                status.AppendLine();
                status.AppendFormat("Last recovered measurement: {0}", ((DateTime)m_mostRecentRecoveredTime).ToString(OutageLog.DateTimeFormat));
                status.AppendLine();

                Outage currentDataGap = m_currentDataGap;

                if ((object)currentDataGap != null)
                {
                    status.AppendFormat("      Currently recovering: {0};{1}", currentDataGap.Start.ToString(OutageLog.DateTimeFormat), currentDataGap.End.ToString(OutageLog.DateTimeFormat));
                    status.AppendLine();
                }

                if ((object)m_temporalSubscription != null)
                {
                    status.AppendLine();
                    status.AppendLine("Data Gap Temporal Subscription Status".CenterText(50));
                    status.AppendLine("-------------------------------------".CenterText(50));
                    status.AppendFormat(m_temporalSubscription.Status);
                }

                if ((object)m_dataGapLog != null)
                {
                    status.AppendLine();
                    status.AppendLine("Data Gap Log Status".CenterText(50));
                    status.AppendLine("-------------------".CenterText(50));
                    status.AppendFormat(m_dataGapLog.Status);
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataGapRecoverer"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataGapRecoverer"/> object and optionally releases the managed resources.
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
                        if ((object)m_dataGapRecoveryCompleted != null)
                        {
                            // Signal any waiting threads
                            m_abnormalTermination = true;
                            m_dataGapRecoveryCompleted.Set();
                            m_dataGapRecoveryCompleted.Dispose();
                        }

                        if ((object)m_dataStreamMonitor != null)
                        {
                            m_dataStreamMonitor.Elapsed -= DataStreamMonitor_Elapsed;
                            m_dataStreamMonitor.Dispose();
                            m_dataStreamMonitor = null;
                        }

                        if ((object)m_dataGapLogProcessor != null)
                        {
                            m_dataGapLogProcessor.Dispose();
                            m_dataGapLogProcessor = null;
                        }

                        if ((object)m_dataGapLog != null)
                        {
                            m_dataGapLog.ProcessException -= Common_ProcessException;
                            m_dataGapLog = null;
                        }

                        if ((object)m_temporalSubscription != null)
                        {
                            m_temporalSubscription.StatusMessage -= Common_StatusMessage;
                            m_temporalSubscription.ProcessException -= Common_ProcessException;
                            m_temporalSubscription.ConnectionEstablished -= TemporalSubscription_ConnectionEstablished;
                            m_temporalSubscription.ConnectionTerminated -= TemporalSubscription_ConnectionTerminated;
                            m_temporalSubscription.NewMeasurements -= TemporalSubscription_NewMeasurements;
                            m_temporalSubscription.ProcessingComplete -= TemporalSubscription_ProcessingComplete;
                            m_temporalSubscription.Dispose();
                            m_temporalSubscription = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.

                    if ((object)Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="DataGapRecoverer"/>.
        /// </summary>
        public void Initialize()
        {
            if (m_disposed)
                throw new InvalidOperationException("Data gap recoverer has been disposed. Cannot initialize.");

            Dictionary<string, string> settings = m_connectionString.ToNonNullString().ParseKeyValuePairs();
            string setting;
            double timeInterval;
            int processingInterval;

            if (settings.TryGetValue("sourceConnectionName", out setting) && !string.IsNullOrWhiteSpace(setting))
                m_sourceConnectionName = setting;

            if (settings.TryGetValue("recoveryStartDelay", out setting) && double.TryParse(setting, out timeInterval))
                RecoveryStartDelay = timeInterval;

            if (settings.TryGetValue("dataMonitoringInterval", out setting) && double.TryParse(setting, out timeInterval))
                DataMonitoringInterval = timeInterval;

            if (settings.TryGetValue("minimumRecoverySpan", out setting) && double.TryParse(setting, out timeInterval))
                MinimumRecoverySpan = timeInterval;

            if (settings.TryGetValue("maximumRecoverySpan", out setting) && double.TryParse(setting, out timeInterval))
                MaximumRecoverySpan = timeInterval;

            if (settings.TryGetValue("filterExpression", out setting) && !string.IsNullOrWhiteSpace(setting))
                FilterExpression = setting;

            if (settings.TryGetValue("recoveryProcessingInterval", out setting) && int.TryParse(setting, out processingInterval))
                RecoveryProcessingInterval = processingInterval;

            if (settings.TryGetValue("useMillisecondResolution", out setting))
                UseMillisecondResolution = setting.ParseBoolean();

            if (settings.TryGetValue("startRecoveryBuffer", out setting) && double.TryParse(setting, out timeInterval))
                StartRecoveryBuffer = timeInterval;

            if (settings.TryGetValue("endRecoveryBuffer", out setting) && double.TryParse(setting, out timeInterval))
                EndRecoveryBuffer = timeInterval;

            // Get logging path, if any has been defined
            if (settings.TryGetValue("loggingPath", out setting))
            {
                setting = FilePath.GetDirectoryName(FilePath.GetAbsolutePath(setting));

                if (Directory.Exists(setting))
                    m_loggingPath = setting;
                else
                    OnStatusMessage(MessageLevel.Warning, $"Logging path \"{setting}\" not found, defaulting to \"{FilePath.GetAbsolutePath("")}\"...", "Initialization");
            }

            if (string.IsNullOrEmpty(m_sourceConnectionName))
                throw new NullReferenceException("Source connection name must defined - it is used to create outage log file name.");

            // Setup a new temporal data subscriber that will be used to query historical data
            m_temporalSubscription = new DataSubscriber();
            m_temporalSubscription.Name = m_sourceConnectionName + "!" + GetType().Name;
            m_temporalSubscription.DataSource = m_dataSource;
            m_temporalSubscription.ConnectionString = $"{m_connectionString};BypassStatistics=true";
            m_temporalSubscription.StatusMessage += Common_StatusMessage;
            m_temporalSubscription.ProcessException += Common_ProcessException;
            m_temporalSubscription.ConnectionEstablished += TemporalSubscription_ConnectionEstablished;
            m_temporalSubscription.ConnectionTerminated += TemporalSubscription_ConnectionTerminated;
            m_temporalSubscription.ProcessingComplete += TemporalSubscription_ProcessingComplete;
            m_temporalSubscription.NewMeasurements += TemporalSubscription_NewMeasurements;
            m_temporalSubscription.Initialize();

            // Setup data gap outage log to persist unprocessed outages between class life-cycles
            m_dataGapLog = new OutageLog();
            m_dataGapLog.FileName = GetLoggingPath(m_sourceConnectionName + "_OutageLog.txt");
            m_dataGapLog.ProcessException += Common_ProcessException;
            m_dataGapLog.Initialize();

            // Setup data gap processor to process items one at a time, a 5-second minimum period is established between each gap processing
            m_dataGapLogProcessor = new OutageLogProcessor(m_dataGapLog, ProcessDataGap, CanProcessDataGap, ex => OnProcessException(MessageLevel.Warning, ex), GSF.Common.Max(5000, (int)(m_recoveryStartDelay * 1000.0D)));
        }

        /// <summary>
        /// Logs a new data gap for processing.
        /// </summary>
        /// <param name="startTime">Start time of data gap.</param>
        /// <param name="endTime">End time of data gap.</param>
        /// <param name="forceLog">Indicates whether to skip data gap validation and force the outage to be logged.</param>
        /// <returns><c>true</c> if data gap was logged for processing; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Data gap will not be logged for processing if the <paramref name="startTime"/> and <paramref name="endTime"/> do not represent
        /// a valid time span for recovery according to <see cref="MinimumRecoverySpan"/> and <see cref="MaximumRecoverySpan"/>.
        /// </remarks>
        public bool LogDataGap(DateTimeOffset startTime, DateTimeOffset endTime, bool forceLog = false)
        {
            if (m_disposed)
                throw new InvalidOperationException("Data gap recoverer has been disposed. Cannot log data gap for processing.");

            if ((object)m_dataGapLog == null)
                throw new InvalidOperationException("Data gap recoverer has not been initialized. Cannot log data gap for processing.");

            OnStatusMessage(MessageLevel.Info, $"Data gap recovery requested for period \"{startTime.ToString(OutageLog.DateTimeFormat, CultureInfo.InvariantCulture)}\" - \"{endTime.ToString(OutageLog.DateTimeFormat, CultureInfo.InvariantCulture)}\"...");

            Time dataGapSpan = (endTime - startTime).TotalSeconds;

            // Only log data gap for processing if it is in an acceptable time span for recovery
            if (forceLog || dataGapSpan >= m_minimumRecoverySpan && dataGapSpan <= m_maximumRecoverySpan)
            {
                // Since local clock may float we add some buffer around recovery window
                m_dataGapLog.Add(new Outage(startTime.AddSeconds(StartRecoveryBuffer), endTime.AddSeconds(EndRecoveryBuffer)));
                return true;
            }

            Time rangeLimit;
            string rangeLimitText;

            if (dataGapSpan < m_minimumRecoverySpan)
            {
                rangeLimit = m_minimumRecoverySpan;
                rangeLimitText = "minimum";
            }
            else
            {
                rangeLimit = m_maximumRecoverySpan;
                rangeLimitText = "maximum";
            }

            OnStatusMessage(MessageLevel.Info, $"Skipped data gap recovery for {Time.ToElapsedTimeString(dataGapSpan, 2)} of missed data - outside configured {rangeLimitText} range of {Time.ToElapsedTimeString(rangeLimit, 2)}.");

            return false;
        }

        /// <summary>
        /// Removes a data gap from the outage log so that it will not be processed.
        /// </summary>
        /// <param name="startTime">Start time of data gap.</param>
        /// <param name="endTime">End time of data gap.</param>
        /// <returns>True if the data gap was successfully removed; false otherwise.</returns>
        public bool RemoveDataGap(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            if (m_disposed)
                throw new InvalidOperationException("Data gap recoverer has been disposed. Cannot log data gap for processing.");

            if ((object)m_dataGapLog == null)
                throw new InvalidOperationException("Data gap recoverer has not been initialized. Cannot log data gap for processing.");

            // Since local clock may float we add some buffer around recovery window
            return m_dataGapLog.Remove(new Outage(startTime, endTime));
        }

        /// <summary>
        /// Produces a dump of the contents of the outage log.
        /// </summary>
        /// <returns>The contents of the outage log.</returns>
        public string DumpOutageLog()
        {
            List<Outage> outages = m_dataGapLog.Outages;
            StringBuilder dump = new StringBuilder();

            foreach (Outage outage in outages)
                dump.AppendLine($"{outage.Start.ToString(OutageLog.DateTimeFormat)};{outage.End.ToString(OutageLog.DateTimeFormat)}");

            dump.AppendLine($"Count: {outages.Count} outages");

            return dump.ToString();
        }

        // Can only start data gap processing when end time of recovery range is beyond recovery start delay
        private bool CanProcessDataGap(Outage dataGap)
        {
            return m_connected && Enabled && (DateTime.UtcNow - dataGap.End).TotalSeconds > m_recoveryStartDelay;
        }

        // Any exceptions in this handler will be exposed through ProcessException event and cause OutageLogProcessor
        // to requeue the data gap outage so it will be processed again (could be that remote system is offline).
        private void ProcessDataGap(Outage dataGap)
        {
            // Establish start and stop time for temporal session
            m_subscriptionInfo.StartTime = dataGap.Start.ToString(OutageLog.DateTimeFormat, CultureInfo.InvariantCulture);
            m_subscriptionInfo.StopTime = dataGap.End.ToString(OutageLog.DateTimeFormat, CultureInfo.InvariantCulture);

            OnStatusMessage(MessageLevel.Info, $"Starting data gap recovery for period \"{m_subscriptionInfo.StartTime}\" - \"{m_subscriptionInfo.StopTime}\"...");

            // Enable data monitor            
            m_dataStreamMonitor.Enabled = true;

            // Reset measurement counters
            m_measurementsRecoveredForDataGap = 0;
            m_measurementsRecoveredOverLastInterval = 0;

            // Reset processing fields
            m_mostRecentRecoveredTime = dataGap.Start.Ticks;
            m_abnormalTermination = false;

            // Reset process completion wait handle
            m_dataGapRecoveryCompleted.Reset();

            // Start temporal data recovery session
            m_temporalSubscription.Subscribe(m_subscriptionInfo);

            // Save the currently processing data gap for reporting
            m_currentDataGap = dataGap;

            // Wait for process completion - success or fail
            m_dataGapRecoveryCompleted.Wait();

            // Clear the currently processing data gap
            m_currentDataGap = null;

            // If temporal session failed to connect, retry data recovery for this outage
            if (m_abnormalTermination)
            {
                // Make sure any data recovered so far doesn't get unnecessarily re-recovered, this requires that source historian report data in time-sorted order
                dataGap = new Outage(new DateTime(GSF.Common.Max((Ticks)dataGap.Start.Ticks, m_mostRecentRecoveredTime - (m_subscriptionInfo.UseMillisecondResolution ? Ticks.PerMillisecond : 1L)), DateTimeKind.Utc), dataGap.End);

                // Re-insert adjusted data gap at the top of the processing queue
                m_dataGapLog.Add(dataGap);

                if (m_measurementsRecoveredForDataGap == 0)
                    OnStatusMessage(MessageLevel.Warning, $"Failed to establish temporal session. Data recovery for period \"{m_subscriptionInfo.StartTime}\" - \"{m_subscriptionInfo.StopTime}\" will be re-attempted.");
                else
                    OnStatusMessage(MessageLevel.Warning, $"Temporal session was disconnected during recovery operation. Data recovery for adjusted period \"{dataGap.Start.ToString(OutageLog.DateTimeFormat, CultureInfo.InvariantCulture)}\" - \"{m_subscriptionInfo.StopTime}\" will be re-attempted.");
            }

            // Unsubscribe from temporal session
            m_temporalSubscription.Unsubscribe();

            // Disable data monitor            
            m_dataStreamMonitor.Enabled = false;

            OnStatusMessage(m_measurementsRecoveredForDataGap == 0 ? MessageLevel.Warning : MessageLevel.Info, $"Recovered {m_measurementsRecoveredForDataGap} measurements for period \"{m_subscriptionInfo.StartTime}\" - \"{m_subscriptionInfo.StopTime}\".");
        }

        /// <summary>
        /// Raises the <see cref="RecoveredMeasurements"/> event.
        /// </summary>
        protected virtual void OnRecoveredMeasurements(ICollection<IMeasurement> measurements)
        {
            try
            {
                RecoveredMeasurements?.Invoke(this, new EventArgs<ICollection<IMeasurement>>(measurements));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for RecoveredMeasurements event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event and sends this data to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="level">The <see cref="MessageLevel"/> to assign to this message</param>
        /// <param name="status">New status message.</param>
        /// <param name="eventName">A fixed string to classify this event; defaults to <c>null</c>.</param>
        /// <param name="flags"><see cref="MessageFlags"/> to use, if any; defaults to <see cref="MessageFlags.None"/>.</param>
        /// <remarks>
        /// <see pref="eventName"/> should be a constant string value associated with what type of message is being
        /// generated. In general, there should only be a few dozen distinct event names per class. Exceeding this
        /// threshold will cause the EventName to be replaced with a general warning that a usage issue has occurred.
        /// </remarks>
        protected virtual void OnStatusMessage(MessageLevel level, string status, string eventName = null, MessageFlags flags = MessageFlags.None)
        {
            try
            {
                Log.Publish(level, flags, eventName ?? "DataGapRecovery", status);

                using (Logger.SuppressLogMessages())
                    StatusMessage?.Invoke(this, new EventArgs<string>(AdapterBase.GetStatusWithMessageLevelPrefix(status, level)));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for StatusMessage event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="level">The <see cref="MessageLevel"/> to assign to this message</param>
        /// <param name="exception">Processing <see cref="Exception"/>.</param>
        /// <param name="eventName">A fixed string to classify this event; defaults to <c>null</c>.</param>
        /// <param name="flags"><see cref="MessageFlags"/> to use, if any; defaults to <see cref="MessageFlags.None"/>.</param>
        /// <remarks>
        /// <see pref="eventName"/> should be a constant string value associated with what type of message is being
        /// generated. In general, there should only be a few dozen distinct event names per class. Exceeding this
        /// threshold will cause the EventName to be replaced with a general warning that a usage issue has occurred.
        /// </remarks>
        protected virtual void OnProcessException(MessageLevel level, Exception exception, string eventName = null, MessageFlags flags = MessageFlags.None)
        {
            try
            {
                Log.Publish(level, flags, eventName ?? "DataGapRecovery", exception?.Message, null, exception);

                using (Logger.SuppressLogMessages())
                    ProcessException?.Invoke(this, new EventArgs<Exception>(exception));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                Log.Publish(MessageLevel.Info, "ConsumerEventException", $"Exception in consumer handler for ProcessException event: {ex.Message}", null, ex);
            }
        }

        private string GetLoggingPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(m_loggingPath))
                return FilePath.GetAbsolutePath(filePath);

            return Path.Combine(m_loggingPath, filePath);
        }

        private void TemporalSubscription_ConnectionEstablished(object sender, EventArgs e)
        {
            m_connected = true;
        }

        private void TemporalSubscription_ConnectionTerminated(object sender, EventArgs e)
        {
            m_connected = false;

            try
            {
                // Disable data monitor            
                m_dataStreamMonitor.Enabled = false;

                // If temporal subscription is currently enabled - connection termination was not expected
                if ((object)m_temporalSubscription != null)
                    m_abnormalTermination = m_temporalSubscription.Enabled;
            }
            finally
            {
                m_dataGapRecoveryCompleted.Set();
            }
        }

        private void TemporalSubscription_ProcessingComplete(object sender, EventArgs<string> e)
        {
            OnStatusMessage(MessageLevel.Info, "Temporal data recovery processing completed.");

            m_dataGapRecoveryCompleted.Set();
            m_dataStreamMonitor.Enabled = false;
        }

        private void TemporalSubscription_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            ICollection<IMeasurement> measurements = e.Argument;
            int total = measurements.Count;

            if (total > 0)
            {
                m_measurementsRecoveredForDataGap += total;
                m_measurementsRecoveredOverLastInterval += total;

                // Publish recovered measurements back to consumer
                OnRecoveredMeasurements(measurements);

                // Track latest reporting time
                long mostRecentRecoveredTime = measurements.Select(m => (long)m.Timestamp).Max();

                if (mostRecentRecoveredTime > m_mostRecentRecoveredTime)
                    m_mostRecentRecoveredTime = mostRecentRecoveredTime;
            }

            // See if consumer has requested to stop recovery operations
            if (!m_enabled)
            {
                OnStatusMessage(MessageLevel.Info, "Data gap recovery has been canceled.");

                m_abnormalTermination = true;

                m_dataGapRecoveryCompleted.Set();
                m_dataStreamMonitor.Enabled = false;
            }
        }

        private void Common_StatusMessage(object sender, EventArgs<string> e) => OnStatusMessage(MessageLevel.Info, e.Argument);

        private void Common_ProcessException(object sender, EventArgs<Exception> e) => OnProcessException(MessageLevel.Warning, e.Argument);

        private void DataStreamMonitor_Elapsed(object sender, EventArgs<DateTime> e)
        {
            if (m_measurementsRecoveredOverLastInterval == 0)
            {
                // If we've received no measurements in the last time-span, we cancel current process...
                m_dataStreamMonitor.Enabled = false;
                OnStatusMessage(MessageLevel.Warning, $"\r\nNo data received in {(m_dataStreamMonitor.Interval / 1000.0D).ToString("0.0")} seconds, canceling current data recovery operation...\r\n");
                m_dataGapRecoveryCompleted.Set();
            }

            // Reset measurements received count being monitored
            m_measurementsRecoveredOverLastInterval = 0L;
        }

        #endregion
    }
}
