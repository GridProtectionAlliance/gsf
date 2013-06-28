//******************************************************************************************************
//  StatisticsEngine.cs - Gbtc
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
//  02/22/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Statistics
{
    /// <summary>
    /// Represents the engine that computes statistics within applications of the TimeSeriesFramework.
    /// </summary>
    [Description("Statistics: defines the engine that computes all statistics within the system.")]
    public class StatisticsEngine : FacileActionAdapterBase
    {
        #region [ Members ]

        // Nested Types
        private class StatisticSource
        {
            public object Source;
            public string SourceName;
            public string SourceCategory;
            public string SourceAcronym;
            public DataRow[] StatisticMeasurements;
        }


        // Events

        /// <summary>
        /// Event is raised before statistics calculation.
        /// </summary>
        public static event EventHandler BeforeCalculate;

        /// <summary>
        /// Event is raised after statistics calculation.
        /// </summary>
        public static event EventHandler Calculated;


        // Fields
        private readonly object m_statisticsLock;
        private readonly List<Statistic> m_statistics;
        private Timer m_statisticCalculationTimer;
        private readonly PerformanceMonitor m_performanceMonitor;
        private int m_lastStatisticCalculationCount;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="StatisticsEngine"/> class.
        /// </summary>
        public StatisticsEngine()
        {
            m_statisticsLock = new object();
            m_statistics = new List<Statistic>();
            m_statisticCalculationTimer = new Timer();
            m_performanceMonitor = new PerformanceMonitor();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to the <see cref="StatisticsEngine"/>.
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
                ReloadStatistics();
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

        /// <summary>
        /// Returns the detailed status of the statistics engine.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder(base.Status);

                status.AppendFormat("          Statistics count: {0}", m_statistics.Count);
                status.AppendLine();
                status.AppendFormat(" Recently calculated stats: {0}", m_lastStatisticCalculationCount);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="StatisticsEngine"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings;
            string setting;
            double reportingInterval;

            base.Initialize();
            settings = Settings;

            // Load the statistic reporting interval
            if (settings.TryGetValue("reportingInterval", out setting))
                reportingInterval = double.Parse(setting) * 1000.0;
            else
                reportingInterval = 10000.0;

            // Set up the statistic calculation timer
            m_statisticCalculationTimer.Elapsed += StatisticCalculationTimer_Elapsed;
            m_statisticCalculationTimer.Interval = reportingInterval;
            m_statisticCalculationTimer.AutoReset = true;
            m_statisticCalculationTimer.Enabled = false;

            // Register system as a statistics source
            Register(m_performanceMonitor, GetSystemName(), "System", "SYSTEM");
        }

        /// <summary>
        /// Starts the <see cref="StatisticsEngine"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the statistics engine or restarts it if it is already running.", "Administrator", "Editor")]
        public override void Start()
        {
            base.Start();

            m_statisticCalculationTimer.Start();
            OnStatusMessage("Started statistics calculation timer.");
        }

        /// <summary>
        /// Stops the <see cref="StatisticsEngine"/>.
        /// </summary>		
        [AdapterCommand("Stops the statistics engine.", "Adminstrator", "Editor")]
        public override void Stop()
        {
            base.Stop();
            m_statisticCalculationTimer.Stop();
        }

        /// <summary>
        /// Loads or reloads system statistics.
        /// </summary>
        [AdapterCommand("Reloads system statistics.", "Administrator", "Editor")]
        public void ReloadStatistics()
        {
            // Make sure setting exists to allow user to by-pass phasor data source validation at startup
            ConfigurationFile configFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = configFile.Settings["systemSettings"];
            settings.Add("ProcessStatistics", true, "Determines if the statistics should be processed during operation");

            // See if statistics should be processed
            if (settings["ProcessStatistics"].ValueAsBoolean())
            {
                Statistic statistic;
                Assembly assembly;
                Type type;
                MethodInfo method;
                string assemblyName, typeName, methodName;
                bool reenable;

                lock (s_statisticSources)
                {
                    // Clear the statistic measurements for each source
                    // so that they will reload on next calculation
                    foreach (StatisticSource source in s_statisticSources)
                    {
                        source.StatisticMeasurements = null;
                    }
                }

                lock (m_statisticsLock)
                {
                    // Empty the statistics list
                    m_statistics.Clear();

                    // Turn off statistic calculation timer while statistics are being reloaded
                    reenable = m_statisticCalculationTimer.Enabled;
                    m_statisticCalculationTimer.Enabled = false;

                    // Load all defined statistics
                    foreach (DataRow row in DataSource.Tables["Statistics"].Select("Enabled <> 0", "Source, SignalIndex"))
                    {
                        // Create a new statistic
                        statistic = new Statistic();

                        // Load primary statistic parameters
                        statistic.Source = row["Source"].ToNonNullString();
                        statistic.Index = int.Parse(row["SignalIndex"].ToNonNullString("-1"));
                        statistic.Arguments = row["Arguments"].ToNonNullString();

                        // Load statistic's code location information
                        assemblyName = row["AssemblyName"].ToNonNullString();
                        typeName = row["TypeName"].ToNonNullString();
                        methodName = row["MethodName"].ToNonNullString();

                        if (string.IsNullOrEmpty(assemblyName))
                            throw new InvalidOperationException("Statistic assembly name was not defined.");

                        if (string.IsNullOrEmpty(typeName))
                            throw new InvalidOperationException("Statistic type name was not defined.");

                        if (string.IsNullOrEmpty(methodName))
                            throw new InvalidOperationException("Statistic method name was not defined.");

                        try
                        {
                            // See if statistic is defined in this assembly (no need to reload)
                            if (string.Compare(GetType().FullName, typeName, true) == 0)
                            {
                                // Assign statistic handler to local method (assumed to be private static)
                                statistic.Method = (StatisticCalculationFunction)Delegate.CreateDelegate(typeof(StatisticCalculationFunction), GetType().GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod));
                            }
                            else
                            {
                                // Load statistic method from containing assembly and type
                                assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(assemblyName));
                                type = assembly.GetType(typeName);
                                method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod);

                                // Assign statistic handler to loaded assembly method
                                statistic.Method = (StatisticCalculationFunction)Delegate.CreateDelegate(typeof(StatisticCalculationFunction), method);
                            }
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(new InvalidOperationException(string.Format("Failed to load statistic handler \"{0}\" from \"{1} [{2}::{3}()]\" due to exception: {4}", row["Name"].ToNonNullString("n/a"), assemblyName, typeName, methodName, ex.Message), ex));
                        }

                        // Add statistic to list
                        m_statistics.Add(statistic);
                    }
                }

                OnStatusMessage("Loaded {0} statistic calculation definitions...", m_statistics.Count);

                if (reenable)
                {
                    m_statisticCalculationTimer.Enabled = true;
                }
            }
            else
            {
                // Make sure statistic calculation timer is off since statistics aren't being processed
                Stop();
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="StatisticsEngine"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="StatisticsEngine"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Last published {0} statistics", m_lastStatisticCalculationCount).CenterText(maxLength);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="StatisticsEngine"/> object and optionally releases the managed resources.
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
                        if ((object)m_statisticCalculationTimer != null)
                        {
                            m_statisticCalculationTimer.Elapsed -= StatisticCalculationTimer_Elapsed;
                            m_statisticCalculationTimer.Dispose();
                            m_statisticCalculationTimer = null;
                        }

                        Unregister(m_performanceMonitor);
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        private void StatisticCalculationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                string message = "An error occurred while attempting to calculate statistics: " + ex.Message;
                OnProcessException(new InvalidOperationException(message, ex));
            }
        }

        private void CalculateStatistics()
        {
            List<IMeasurement> calculatedStatistics = new List<IMeasurement>();
            StatisticSource[] sources;
            Statistic[] statistics;
            DateTime serverTime;

            try
            {
                lock (s_statisticSources)
                {
                    // Get a snapshot of the current list of sources
                    // that can be iterated safely without locking
                    sources = s_statisticSources.ToArray();
                }

                lock (m_statisticsLock)
                {
                    // Get a snapshot of the current list of statistics
                    // that can be iterated safely without locking
                    statistics = m_statistics.ToArray();
                }

                OnBeforeCalculate();
                serverTime = DateTime.UtcNow;

                foreach (StatisticSource source in sources)
                    calculatedStatistics.AddRange(CalculateStatistics(statistics, serverTime, source));

                // Send calculated statistics into the system
                OnNewMeasurements(calculatedStatistics);

                // Notify that statistics have been calculated
                OnCalculated();

                // Update value used when displaying short status
                m_lastStatisticCalculationCount = calculatedStatistics.Count;
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error encountered while calculating statistics: {0}", ex.Message);
                OnProcessException(new Exception(errorMessage, ex));
            }
        }

        private ICollection<IMeasurement> CalculateStatistics(Statistic[] statistics, DateTime serverTime, StatisticSource source)
        {
            List<IMeasurement> calculatedStatistics = new List<IMeasurement>();
            IMeasurement calculatedStatistic;

            try
            {
                // Load statistic measurements for this source if none are currently loaded
                if ((object)source.StatisticMeasurements == null)
                    source.StatisticMeasurements = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalReference LIKE '{0}!{1}-ST%'", source.SourceName, source.SourceAcronym));

                // Calculate statistics
                foreach (DataRow measurement in source.StatisticMeasurements)
                {
                    calculatedStatistic = CalculateStatistic(statistics, serverTime, source, measurement);

                    if ((object)calculatedStatistic != null)
                        calculatedStatistics.Add(calculatedStatistic);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error calculating statistics for {0}: {1}", source.SourceName, ex.Message);
                OnProcessException(new Exception(errorMessage, ex));
            }

            return calculatedStatistics;
        }

        private IMeasurement CalculateStatistic(Statistic[] statistics, DateTime serverTime, StatisticSource source, DataRow measurement)
        {
            Guid signalID;
            string signalReference;
            Statistic statistic;

            try
            {
                // Get the signal ID and signal reference of the current measurement
                signalID = Guid.Parse(measurement["SignalID"].ToString());
                signalReference = measurement["SignalReference"].ToString();

                // Find the statistic corresponding to the current measurement
                statistic = statistics.FirstOrDefault(stat => (source.SourceCategory == stat.Source) && (signalReference == string.Format("{0}!{1}-ST{2}", source.SourceName, source.SourceAcronym, stat.Index)));

                if ((object)statistic != null)
                {
                    // Calculate the current value of the statistic measurement
                    return new Measurement
                        {
                        ID = signalID,
                        Key = MeasurementKey.Parse(measurement["ID"].ToString(), signalID),
                        TagName = measurement["PointTag"].ToNonNullString(),
                        Adder = double.Parse(measurement["Adder"].ToNonNullString("0.0")),
                        Multiplier = double.Parse(measurement["Multiplier"].ToNonNullString("1.0")),
                        Value = statistic.Method(source.Source, statistic.Arguments),
                        Timestamp = serverTime
                    };
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error calculating statistic for {0}: {1}", measurement["SignalReference"], ex.Message);
                OnProcessException(new Exception(errorMessage, ex));
            }

            return null;
        }

        private string GetSystemName()
        {
            Guid nodeID;

            if (DataSource.Tables.Contains("NodeInfo"))
            {
                return DataSource.Tables["NodeInfo"].Rows[0]["Name"]
                    .ToNonNullString()
                    .RemoveCharacters(c => !char.IsLetterOrDigit(c))
                    .Replace(' ', '_')
                    .ToUpper();
            }

            nodeID = ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].ValueAs<Guid>();

            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            {
                return database.Connection.ExecuteScalar(string.Format("SELECT Name FROM Node WHERE ID = '{0}'", database.Guid(nodeID))).ToNonNullString().ToUpper();
            }
        }

        private void OnBeforeCalculate()
        {
            if ((object)BeforeCalculate != null)
                BeforeCalculate(this, new EventArgs());
        }

        private void OnCalculated()
        {
            if ((object)Calculated != null)
                Calculated(this, new EventArgs());
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly List<StatisticSource> s_statisticSources;
        private static int? s_statSignalTypeID;
        private static int? s_statHistorianID;

        // Static Constructor

        /// <summary>
        /// Static initialization of <see cref="StatisticsEngine"/> class.
        /// </summary>
        static StatisticsEngine()
        {
            s_statisticSources = new List<StatisticSource>();
        }

        // Static Methods

        /// <summary>
        /// Registers the given adapter with the statistics engine as a source of statistics.
        /// </summary>
        /// <param name="adapter">The source of the statistics.</param>
        /// <param name="sourceCategory">The category of the statistics.</param>
        /// <param name="sourceAcronym">The acronym used in signal references.</param>
        public static void Register(IAdapter adapter, string sourceCategory, string sourceAcronym)
        {
            Register(adapter, adapter.Name, sourceCategory, sourceAcronym);
        }

        /// <summary>
        /// Registers the given object with the statistics engine as a source of statistics.
        /// </summary>
        /// <param name="source">The source of the statistics.</param>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="sourceCategory">The category of the statistics.</param>
        /// <param name="sourceAcronym">The acronym used in signal references.</param>
        public static void Register(object source, string sourceName, string sourceCategory, string sourceAcronym)
        {
            StatisticSource sourceInfo;
            IAdapter adapter;

            sourceInfo = new StatisticSource
                {
                Source = source,
                SourceName = sourceName,
                SourceCategory = sourceCategory,
                SourceAcronym = sourceAcronym
            };

            lock (s_statisticSources)
            {
                if (s_statisticSources.Any(registeredSource => registeredSource.Source == source))
                    throw new InvalidOperationException(string.Format("Unable to register {0} as statistic source because it is already registered.", sourceName));

                s_statisticSources.Add(sourceInfo);
            }

            adapter = source as IAdapter;

            if ((object)adapter != null)
                adapter.Disposed += (sender, args) => Unregister(sender);
        }

        /// <summary>
        /// Unregisters the given source, removing it from the list of
        /// statistic sources so that it no longer generates statistics.
        /// </summary>
        /// <param name="source">The adapter to be unregistered with the statistics engine.</param>
        /// <remarks>
        /// Sources that implement <see cref="IAdapter"/> do not need to
        /// explicitly unregister themselves from the statistics engine.
        /// The engine automatically unregisters them by attaching to the
        /// <see cref="ISupportLifecycle.Disposed"/> event.
        /// </remarks>
        public static void Unregister(object source)
        {
            if (source != null)
            {
                lock (s_statisticSources)
                {
                    for (int i = 0; i < s_statisticSources.Count; i++)
                    {
                        if (s_statisticSources[i].Source == source)
                        {
                            s_statisticSources.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the signal type ID used for statistic measurements.
        /// </summary>
        /// <param name="database">The database to be queried.</param>
        /// <returns>The signal type ID used for statistic measurements.</returns>
        private static int GetStatSignalTypeID(AdoDataConnection database)
        {
            const string statSignalTypeQuery = "SELECT ID FROM SignalType WHERE Acronym = 'STAT'";

            if ((object)s_statSignalTypeID == null)
                s_statSignalTypeID = Convert.ToInt32(database.Connection.ExecuteScalar(statSignalTypeQuery));

            return (int)s_statSignalTypeID;
        }

        /// <summary>
        /// Gets the ID of the statistics historian.
        /// </summary>
        /// <param name="database">The database to be queried.</param>
        /// <returns>The ID of the statistics historian.</returns>
        private static int GetStatHistorianID(AdoDataConnection database)
        {
            const string statHistorianQuery = "SELECT ID FROM Historian WHERE Acronym = 'STAT'";

            if ((object)s_statHistorianID == null)
                s_statHistorianID = Convert.ToInt32(database.Connection.ExecuteScalar(statHistorianQuery));

            return (int)s_statHistorianID;
        }

        /// <summary>
        /// Gets the device ID of the given adapter, or <see cref="DBNull.Value"/> if the adapter is not a device.
        /// </summary>
        /// <param name="database">The database to be queried.</param>
        /// <param name="adapter">The adapter whose device ID is queried.</param>
        /// <returns>The device ID of the given adapter, or <see cref="DBNull.Value"/> if the adapter is not a device.</returns>
        private static object GetDeviceID(AdoDataConnection database, IAdapter adapter)
        {
            const string runtimeCountQueryFormat = "SELECT COUNT(*) FROM Runtime WHERE ID = {0} AND SourceTable = 'Device'";
            const string deviceIDQueryFormat = "SELECT SourceID FROM Runtime WHERE ID = {0} AND SourceTable = 'Device'";
            object deviceID = DBNull.Value;
            int runtimeCount;

            if ((object)adapter != null)
            {
                runtimeCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(runtimeCountQueryFormat, adapter.ID)));

                if (runtimeCount > 0)
                    deviceID = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(deviceIDQueryFormat, adapter.ID)));
            }

            return deviceID;
        }

        /// <summary>
        /// Determines whether the given signal reference matches the
        /// signal reference regular expression using the given suffix.
        /// </summary>
        /// <param name="signalReference">The signal reference to be matched against the regular expression.</param>
        /// <param name="suffix">The suffix used by a particular type of statistic.</param>
        /// <returns>Flag indicating whether the signal reference matches the regular expression.</returns>
        /// <remarks>
        /// The format for signal reference of statistics is: <c>ACRONYM!SUFFIX-ST#</c>,
        /// where <c>ACRONYM</c> is the acronym of the measurement's source, <c>SUFFIX</c>
        /// is the suffix given as a parameter to this method, and <c>#</c> is an index
        /// used to differentiate between statistics with the same source and type.
        /// </remarks>
        public static bool RegexMatch(string signalReference, string suffix)
        {
            string regex = string.Format(@"!{0}-ST\d+$", suffix);
            return Regex.IsMatch(signalReference, regex);
        }

        #endregion
    }
}
