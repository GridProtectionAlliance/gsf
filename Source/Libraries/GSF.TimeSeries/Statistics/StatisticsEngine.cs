//******************************************************************************************************
//  StatisticsEngine.cs - Gbtc
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Parsing;
using GSF.Threading;
using GSF.TimeSeries.Adapters;
using Timer = System.Timers.Timer;

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

        // Represents a source for a statistics
        private class StatisticSource
        {
            public WeakReference<object> SourceReference;
            public string SourceName;
            public string SourceCategory;
            public string SourceAcronym;
            public string StatisticMeasurementNameFormat;

            public List<DataRow> StatisticMeasurements;
            public bool HasUpdatedStatisticMeasurements;
        }

        // Represents a signal reference
        private struct SignalReference
        {
            public readonly string Acronym;
            public readonly int Index;
            public readonly bool IsStatistic;

            public SignalReference(string signal)
            {
                const string Pattern = "^(?<Acronym>.+)-ST(?<Index>[0-9]+)$";
                Match match = Regex.Match(signal, Pattern);

                if (match.Success)
                {
                    Acronym = match.Groups["Acronym"].Value;
                    Index = Convert.ToInt32(match.Groups["Index"].Value);
                    IsStatistic = true;
                }
                else
                {
                    Acronym = signal;
                    Index = 0;
                    IsStatistic = false;
                }
            }
        }

        private class DBUpdateHelper
        {
            #region [ Members ]

            // Fields
            private readonly AdoDataConnection m_database;
            private StatisticSource m_source;
            private DataRow m_statistic;

            private string m_name;
            private int? m_historianID;
            private object m_deviceID;
            private string m_pointTag;
            private int? m_signalTypeID;
            private string m_signalReference;
            private string m_description;

            private Guid? m_nodeID;
            private string m_nodeOwner;
            private string m_company;

            private Dictionary<string, DataRow> m_deviceLookup;
            private Dictionary<int, string> m_companyLookup;

            #endregion

            #region [ Constructors ]

            public DBUpdateHelper(AdoDataConnection database)
            {
                m_database = database;
            }

            #endregion

            #region [ Properties ]

            public StatisticSource Source
            {
                get
                {
                    return m_source;
                }
                set
                {
                    m_source = value;
                    m_name = null;
                    m_deviceID = null;
                    m_company = null;
                }
            }

            public DataRow Statistic
            {
                get
                {
                    return m_statistic;
                }
                set
                {
                    m_statistic = value;
                    m_name = null;
                    m_pointTag = null;
                    m_signalReference = null;
                    m_description = null;
                }
            }

            public string Name
            {
                get
                {
                    return m_name ?? (m_name = GetName());
                }
            }

            public int HistorianID
            {
                get
                {
                    return m_historianID ?? (int)(m_historianID = GetHistorianID());
                }
            }

            public object DeviceID
            {
                get
                {
                    return m_deviceID ?? (m_deviceID = GetDeviceID());
                }
            }

            public string PointTag
            {
                get
                {
                    return m_pointTag ?? (m_pointTag = GetPointTag());
                }
            }

            public int SignalTypeID
            {
                get
                {
                    return m_signalTypeID ?? (int)(m_signalTypeID = GetSignalTypeID());
                }
            }

            public string SignalReference
            {
                get
                {
                    return m_signalReference ?? (m_signalReference = GetSignalReference());
                }
            }

            public string Description
            {
                get
                {
                    return m_description ?? (m_description = GetDescription());
                }
            }

            public int Index
            {
                get
                {
                    return Convert.ToInt32(m_statistic["SignalIndex"]);
                }
            }

            private Guid NodeID
            {
                get
                {
                    return m_nodeID ?? (Guid)(m_nodeID = GetNodeID());
                }
            }

            private string Company
            {
                get
                {
                    return m_company ?? (m_company = GetCompany());
                }
            }

            private string NodeOwner
            {
                get
                {
                    return m_nodeOwner ?? (m_nodeOwner = GetNodeOwner());
                }
            }

            private string Category
            {
                get
                {
                    return m_source.SourceCategory;
                }
            }

            private string Acronym
            {
                get
                {
                    return m_source.SourceAcronym;
                }
            }

            private Dictionary<string, DataRow> DeviceLookup
            {
                get
                {
                    return m_deviceLookup ?? (m_deviceLookup = GetDeviceLookup());
                }
            }

            private Dictionary<int, string> CompanyLookup
            {
                get
                {
                    return m_companyLookup ?? (m_companyLookup = GetCompanyLookup());
                }
            }

            #endregion

            #region [ Methods ]

            private string GetName()
            {
                string arguments;
                Dictionary<string, string> substitutions;
                TemplatedExpressionParser parser;

                arguments = m_statistic.Field<string>("Arguments");

                if (string.IsNullOrWhiteSpace(arguments))
                    return m_source.SourceName;

                substitutions = arguments.ParseKeyValuePairs();
                parser = new TemplatedExpressionParser();
                parser.TemplatedExpression = m_source.StatisticMeasurementNameFormat;

                if (substitutions.Count == 0)
                {
                    substitutions = arguments
                        .Split(',')
                        .Select((arg, index) => Tuple.Create(index.ToString(), arg))
                        .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
                }

                substitutions = substitutions.ToDictionary(kvp => string.Concat("{", kvp.Key, "}"), kvp => kvp.Value);
                substitutions["{}"] = m_source.SourceName;

                return parser.Execute(substitutions);
            }

            private int GetHistorianID()
            {
                const string StatHistorianIDFormat = "SELECT ID FROM Historian WHERE Acronym = 'STAT' AND NodeID = {0}";
                return ExecuteScalar<int>(StatHistorianIDFormat, m_database.Guid(NodeID));
            }

            private object GetDeviceID()
            {
                DataRow device;

                if (!DeviceLookup.TryGetValue(Name, out device))
                    return DBNull.Value;

                return device["ID"];
            }

            private string GetPointTag()
            {
                return string.Format("{0}_{1}!{2}:ST{3}", Company, Name, Acronym, Index);
            }

            private int GetSignalTypeID()
            {
                const string SignalTypeIDFormat = "SELECT ID FROM SignalType WHERE Acronym = 'STAT'";
                return ExecuteScalar<int>(SignalTypeIDFormat);
            }

            private string GetSignalReference()
            {
                return string.Format("{0}!{1}-ST{2}", Name, Acronym, Index);
            }

            private string GetDescription()
            {
                return string.Format("{0} statistic for {1}", Category, m_statistic["Description"].ToNonNullString());
            }

            private Guid GetNodeID()
            {
                return Guid.Parse(ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].Value);
            }

            private string GetCompany()
            {
                DataRow device;
                int companyID;
                string company = null;

                bool isNodeOwner = !DeviceLookup.TryGetValue(Name, out device) ||
                                   !int.TryParse(device["CompanyID"].ToNonNullString(), out companyID) ||
                                   !CompanyLookup.TryGetValue(companyID, out company);

                return isNodeOwner ? NodeOwner : company;
            }

            private string GetNodeOwner()
            {
                const string NodeCompanyIDFormat = "SELECT CompanyID FROM Node WHERE ID = {0}";
                const string CompanyAcronymFormat = "SELECT MapAcronym FROM Company WHERE ID = {0}";

                int nodeCompanyID;
                string companyAcronym;

                try
                {
                    nodeCompanyID = ExecuteScalar<int>(NodeCompanyIDFormat, m_database.Guid(NodeID));
                    companyAcronym = ExecuteScalar<string>(CompanyAcronymFormat, nodeCompanyID);
                }
                catch
                {
                    companyAcronym = ConfigurationFile.Current.Settings["systemSettings"]["CompanyAcronym"].Value.TruncateRight(3);
                }

                return companyAcronym;
            }

            private Dictionary<string, DataRow> GetDeviceLookup()
            {
                const string DeviceLookupFormat = "SELECT ID, Acronym, CompanyID FROM Device WHERE NodeID = {0}";

                return RetrieveData(DeviceLookupFormat, m_database.Guid(NodeID)).Select()
                    .ToDictionary(row => row["Acronym"].ToNonNullString());
            }

            private Dictionary<int, string> GetCompanyLookup()
            {
                const string CompanyLookupFormat = "SELECT ID, Acronym FROM Company";
                int id = 0;

                return RetrieveData(CompanyLookupFormat).Select()
                    .Where(row => int.TryParse(row["ID"].ToNonNullString(), out id))
                    .ToDictionary(row => id, row => row["Acronym"].ToNonNullString());
            }

            public T ExecuteScalar<T>(string queryFormat, params object[] parameters)
            {
                string query = m_database.ParameterizedQueryString(queryFormat, parameters.Select((parameter, index) => "p" + index).ToArray());
                return (T)Convert.ChangeType(m_database.Connection.ExecuteScalar(query, DataExtensions.DefaultTimeoutDuration, parameters), typeof(T));
            }

            public void ExecuteNonQuery(string queryFormat, params object[] parameters)
            {
                string query = m_database.ParameterizedQueryString(queryFormat, parameters.Select((parameter, index) => "p" + index).ToArray());
                m_database.Connection.ExecuteNonQuery(query, DataExtensions.DefaultTimeoutDuration, parameters);
            }

            public DataTable RetrieveData(string queryFormat, params object[] parameters)
            {
                string query = m_database.ParameterizedQueryString(queryFormat, parameters.Select((parameter, index) => "p" + index).ToArray());
                return m_database.Connection.RetrieveData(m_database.AdapterType, query, DataExtensions.DefaultTimeoutDuration, parameters);
            }

            #endregion
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

        private Timer m_reloadStatisticsTimer;
        private Timer m_statisticCalculationTimer;

        private readonly LongSynchronizedOperation m_updateStatisticMeasurementsOperation;
        private readonly LongSynchronizedOperation m_loadStatisticsOperation;
        private readonly LongSynchronizedOperation m_calculateStatisticsOperation;
        private readonly ShortSynchronizedOperation m_validateSourceReferencesOperation;

        private readonly PerformanceMonitor m_performanceMonitor;

        private double m_reportingInterval;

        private int m_lastStatisticCalculationCount;
        private DateTime m_lastStatisticCalculationTime;

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
            m_reloadStatisticsTimer = new Timer();
            m_statisticCalculationTimer = new Timer();

            // Set up the statistic calculation timer
            m_reloadStatisticsTimer.Elapsed += ReloadStatisticsTimer_Elapsed;
            m_reloadStatisticsTimer.Interval = 1.0D;
            m_reloadStatisticsTimer.AutoReset = false;
            m_reloadStatisticsTimer.Enabled = false;

            m_updateStatisticMeasurementsOperation = new LongSynchronizedOperation(UpdateStatisticMeasurements, ex =>
            {
                string message = "An error occurred while attempting to update statistic measurement definitions: " + ex.Message;
                OnProcessException(new InvalidOperationException(message, ex));
            });

            m_loadStatisticsOperation = new LongSynchronizedOperation(LoadStatistics, ex =>
            {
                string message = "An error occurred while attempting to load statistic definitions: " + ex.Message;
                OnProcessException(new InvalidOperationException(message, ex));
            });

            m_calculateStatisticsOperation = new LongSynchronizedOperation(CalculateStatistics, ex =>
            {
                string message = "An error occurred while attempting to calculate statistics: " + ex.Message;
                OnProcessException(new InvalidOperationException(message, ex));
            });

            m_validateSourceReferencesOperation = new ShortSynchronizedOperation(ValidateSourceReferences, ex =>
            {
                string message = "An error occurred while attempting to validate statistic source references: " + ex.Message;
                OnProcessException(new InvalidOperationException(message, ex));
            });

            m_updateStatisticMeasurementsOperation.IsBackground = true;
            m_loadStatisticsOperation.IsBackground = true;
            m_calculateStatisticsOperation.IsBackground = true;

            m_performanceMonitor = new PerformanceMonitor();

            SourceRegistered += HandleSourceRegistered;
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
                RestartReloadStatisticsTimer();
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
                status.AppendFormat("     Last stat calculation: {0}", m_lastStatisticCalculationTime);
                status.AppendLine();

                lock (StatisticSources)
                {
                    status.AppendFormat("    Statistic source count: {0}", StatisticSources.Count);
                    status.AppendLine();
                }

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

            base.Initialize();
            settings = Settings;

            // Load the statistic reporting interval
            if (settings.TryGetValue("reportingInterval", out setting))
                m_reportingInterval = double.Parse(setting) * 1000.0;
            else
                m_reportingInterval = 10000.0;

            if (UseLocalClockAsRealTime || !TrackLatestMeasurements)
            {
                // Set up the statistic calculation timer
                m_statisticCalculationTimer.Elapsed += StatisticCalculationTimer_Elapsed;
                m_statisticCalculationTimer.Interval = m_reportingInterval;
                m_statisticCalculationTimer.AutoReset = true;
                m_statisticCalculationTimer.Enabled = false;
            }

            // Register system as a statistics source
            Register(m_performanceMonitor, GetSystemName(), "System", "SYSTEM");
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            foreach (IMeasurement measurement in measurements)
            {
                if (m_lastStatisticCalculationTime == default(DateTime))
                    m_lastStatisticCalculationTime = measurement.Timestamp;

                base.QueueMeasurementsForProcessing(new IMeasurement[] { measurement });

                if (!UseLocalClockAsRealTime && TrackLatestMeasurements)
                {
                    if (RealTime >= m_lastStatisticCalculationTime.AddMilliseconds(m_reportingInterval).Ticks)
                        CalculateStatistics();
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="StatisticsEngine"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the statistics engine or restarts it if it is already running.", "Administrator", "Editor")]
        public override void Start()
        {
            base.Start();

            if (UseLocalClockAsRealTime || !TrackLatestMeasurements)
            {
                m_statisticCalculationTimer.Start();
                OnStatusMessage("Started statistics calculation timer.");
            }
        }

        /// <summary>
        /// Stops the <see cref="StatisticsEngine"/>.
        /// </summary>		
        [AdapterCommand("Stops the statistics engine.", "Administrator", "Editor")]
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
                m_updateStatisticMeasurementsOperation.RunOnceAsync();
                m_loadStatisticsOperation.RunOnce();
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
                        if ((object)m_reloadStatisticsTimer != null)
                        {
                            m_reloadStatisticsTimer.Elapsed -= ReloadStatisticsTimer_Elapsed;
                            m_reloadStatisticsTimer.Dispose();
                            m_reloadStatisticsTimer = null;
                        }

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

        private void UpdateStatisticMeasurements()
        {
            const string StatisticSelectFormat = "SELECT Source, SignalIndex, Arguments, Description FROM Statistic WHERE Enabled <> 0";
            const string StatisticMeasurementSelectFormat = "SELECT SignalReference FROM Measurement WHERE SignalReference IN ({0})";
            const string StatisticMeasurementInsertFormat = "INSERT INTO Measurement(HistorianID, DeviceID, PointTag, SignalTypeID, SignalReference, Description, Enabled) VALUES({0}, {1}, {2}, {3}, {4}, {5}, 1)";

            StatisticSource[] sources;

            Dictionary<string, List<DataRow>> statisticsLookup;
            List<DataRow> statistics;
            List<DataRow> statisticMeasurements;

            DBUpdateHelper helper;
            HashSet<int> existingIndexes;

            bool configurationChanged = false;

            lock (StatisticSources)
            {
                // Obtain a snapshot of the sources that are
                // currently registered with the statistics engine
                sources = StatisticSources.ToArray();
            }

            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            {
                // Handles database queries, caching, and lazy loading for
                // determining the parameters to send in to each INSERT query
                helper = new DBUpdateHelper(database);

                // Load statistics from the statistics table to determine
                // what statistics should be defined for each source
                statisticsLookup = helper.RetrieveData(StatisticSelectFormat).Select()
                    .GroupBy(row => row["Source"].ToNonNullString())
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

                // Make sure the full set of statistic measurements are defined for each source
                foreach (StatisticSource source in sources)
                {
                    List<string> signalReferences;
                    string args;

                    // If statistic measurements have already been updated for this source,
                    // do not attempt to update them again. This helps to prevent race conditions
                    // between configuration changes and statistics engine registration
                    if (source.HasUpdatedStatisticMeasurements)
                        continue;

                    // If no statistics exist for this category,
                    // there are no statistics that can be created for this source
                    if (!statisticsLookup.TryGetValue(source.SourceCategory, out statistics))
                        continue;

                    // Build a list of signal references for this source
                    // based on the statistics in this category
                    signalReferences = new List<string>();

                    helper.Source = source;

                    foreach (DataRow statistic in statistics)
                    {
                        helper.Statistic = statistic;
                        signalReferences.Add(helper.SignalReference);
                    }

                    // Get the statistic measurements from the database which have already been defined for this source
                    args = string.Join(",", signalReferences.Select((signalReference, index) => string.Concat("{", index, "}")));
                    statisticMeasurements = helper.RetrieveData(string.Format(StatisticMeasurementSelectFormat, args), signalReferences.ToArray<object>()).Select().ToList();

                    // If the number of statistics for the source category matches
                    // the number of statistic measurements for the source, assume
                    // all is well and move to the next source
                    if (statistics.Count == statisticMeasurements.Count)
                        continue;

                    // Get a collection of signal indexes already have statistic measurements
                    existingIndexes = new HashSet<int>(statisticMeasurements
                        .Select(measurement => measurement["SignalReference"].ToNonNullString())
                        .Select(str => new SignalReference(str))
                        .Select(signalReference => signalReference.Index));

                    // Create statistic measurements for statistics that do not have any
                    foreach (DataRow statistic in statistics)
                    {
                        helper.Statistic = statistic;

                        // If a measurement already exists for this statistic, skip it
                        if (existingIndexes.Contains(helper.Index))
                            continue;

                        // Insert the statistic measurement and mark configuration as changed
                        helper.ExecuteNonQuery(StatisticMeasurementInsertFormat, helper.HistorianID, helper.DeviceID, helper.PointTag, helper.SignalTypeID, helper.SignalReference, helper.Description);
                        configurationChanged = true;
                    }

                    source.HasUpdatedStatisticMeasurements = true;
                }
            }

            // If configuration was changed by this operation, notify the host
            if (configurationChanged)
                OnConfigurationChanged();
        }

        private void LoadStatistics()
        {
            StatisticSource[] sources;
            StatisticSource source;
            Statistic statistic;

            Assembly assembly;
            Type type;
            MethodInfo method;
            string assemblyName, typeName, methodName, signalReference;

            Dictionary<string, StatisticSource> sourceLookup;
            Dictionary<StatisticSource, List<DataRow>> activeMeasurementsLookup;
            List<DataRow> statisticMeasurements;
            long statisticMeasurementCount = 0L;

            bool reenable;

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

            lock (StatisticSources)
            {
                // Obtain a snapshot of the sources that are
                // currently registered with the statistics engine
                sources = StatisticSources.ToArray();
            }

            // Create a lookup table from signal reference to statistic source
            sourceLookup = new Dictionary<string, StatisticSource>();

            foreach (Tuple<StatisticSource, IEnumerable<Statistic>> mapping in sources.GroupJoin(m_statistics, src => src.SourceCategory, stat => stat.Source, Tuple.Create))
            {
                foreach (Statistic stat in mapping.Item2)
                {
                    signalReference = GetSignalReference(stat, mapping.Item1);

                    if (sourceLookup.ContainsKey(signalReference))
                        OnStatusMessage("WARNING: Encountered duplicate signal reference statistic: {0}", signalReference);
                    else
                        sourceLookup.Add(signalReference, mapping.Item1);
                }
            }

            // Create a lookup table from statistic source to a
            // list of data rows from the ActiveMeasurements table
            activeMeasurementsLookup = new Dictionary<StatisticSource, List<DataRow>>();

            foreach (DataRow row in DataSource.Tables["ActiveMeasurements"].Select("SignalType = 'STAT'"))
            {
                if (sourceLookup.TryGetValue(row.Field<string>("SignalReference"), out source))
                {
                    statisticMeasurements = activeMeasurementsLookup.GetOrAdd(source, statisticSource => new List<DataRow>());
                    statisticMeasurements.Add(row);
                    statisticMeasurementCount++;
                }
            }

            // Update StatisticMeasurements collections for all sources
            foreach (StatisticSource src in sources)
            {
                if (activeMeasurementsLookup.TryGetValue(src, out statisticMeasurements))
                    src.StatisticMeasurements = statisticMeasurements;
            }

            OnStatusMessage("Loaded {0} statistic calculation definitions and {1} statistic measurement definitions.", m_statistics.Count, statisticMeasurementCount);

            if (reenable)
            {
                m_statisticCalculationTimer.Enabled = true;
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
                lock (StatisticSources)
                {
                    // Get a snapshot of the current list of sources
                    // that can be iterated safely without locking
                    sources = StatisticSources.ToArray();
                }

                lock (m_statisticsLock)
                {
                    // Get a snapshot of the current list of statistics
                    // that can be iterated safely without locking
                    statistics = m_statistics.ToArray();
                }

                OnBeforeCalculate();
                serverTime = RealTime;

                foreach (StatisticSource source in sources)
                    calculatedStatistics.AddRange(CalculateStatistics(statistics, serverTime, source));

                // Send calculated statistics into the system
                OnNewMeasurements(calculatedStatistics);

                // Notify that statistics have been calculated
                OnCalculated();

                // Update value used when displaying short status
                m_lastStatisticCalculationCount = calculatedStatistics.Count;

                // Update last statistic calculation time
                m_lastStatisticCalculationTime = serverTime;
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
            List<DataRow> measurements;

            try
            {
                measurements = source.StatisticMeasurements;

                // Calculate statistics
                if ((object)measurements != null)
                {
                    foreach (DataRow measurement in measurements)
                    {
                        calculatedStatistic = CalculateStatistic(statistics, serverTime, source, measurement);

                        if ((object)calculatedStatistic != null)
                            calculatedStatistics.Add(calculatedStatistic);
                    }
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
            object target;

            if (source.SourceReference.TryGetTarget(out target))
            {
                Guid signalID;
                string signalReference;
                int signalIndex;

                Statistic statistic;

                try
                {
                    // Get the signal ID and signal reference of the current measurement
                    signalID = Guid.Parse(measurement["SignalID"].ToString());
                    signalReference = measurement["SignalReference"].ToString();
                    signalIndex = Convert.ToInt32(signalReference.Substring(signalReference.LastIndexOf("-ST", StringComparison.Ordinal) + 3));

                    // Find the statistic corresponding to the current measurement
                    statistic = statistics.FirstOrDefault(stat => (source.SourceCategory == stat.Source) && (signalIndex == stat.Index));

                    if ((object)statistic != null)
                    {
                        MeasurementKey key = MeasurementKey.LookUpOrCreate(signalID, measurement["ID"].ToString());

                        // Calculate the current value of the statistic measurement
                        return new Measurement()
                        {
                            CommonMeasurementFields = key.CommonMeasurementFields,
                            Value = statistic.Method(target, statistic.Arguments),
                            Timestamp = serverTime
                        };
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Error calculating statistic for {0}: {1}", measurement["SignalReference"], ex.Message);
                    OnProcessException(new Exception(errorMessage, ex));
                }
            }
            else
            {
                m_validateSourceReferencesOperation.RunOnceAsync();
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

        private void RestartReloadStatisticsTimer()
        {
            if ((object)m_reloadStatisticsTimer != null)
            {
                m_reloadStatisticsTimer.Stop();
                m_reloadStatisticsTimer.Start();
            }
        }

        private void HandleSourceRegistered(object sender, EventArgs eventArgs)
        {
            if ((object)DataSource != null)
                RestartReloadStatisticsTimer();
        }

        private void ReloadStatisticsTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            ReloadStatistics();
        }

        private void StatisticCalculationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // If multiple timer events overlap, try-run will make sure only one is running at once
            m_calculateStatisticsOperation.TryRunOnce();
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
        private static readonly List<StatisticSource> StatisticSources;
        private static event EventHandler<EventArgs> SourceRegistered;

        // Static Constructor

        /// <summary>
        /// Static initialization of <see cref="StatisticsEngine"/> class.
        /// </summary>
        static StatisticsEngine()
        {
            StatisticSources = new List<StatisticSource>();
        }

        // Static Methods

        /// <summary>
        /// Registers the given adapter with the statistics engine as a source of statistics.
        /// </summary>
        /// <param name="adapter">The source of the statistics.</param>
        /// <param name="sourceCategory">The category of the statistics.</param>
        /// <param name="sourceAcronym">The acronym used in signal references.</param>
        /// <param name="statisticMeasurementNameFormat">Format string used to name statistic measurements for this source.</param>
        public static void Register(IAdapter adapter, string sourceCategory, string sourceAcronym, string statisticMeasurementNameFormat = "{}")
        {
            Register(adapter, adapter.Name, sourceCategory, sourceAcronym, statisticMeasurementNameFormat);
        }

        /// <summary>
        /// Registers the given object with the statistics engine as a source of statistics.
        /// </summary>
        /// <param name="source">The source of the statistics.</param>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="sourceCategory">The category of the statistics.</param>
        /// <param name="sourceAcronym">The acronym used in signal references.</param>
        /// <param name="statisticMeasurementNameFormat">Format string used to name statistic measurements for this source.</param>
        public static void Register(object source, string sourceName, string sourceCategory, string sourceAcronym, string statisticMeasurementNameFormat = "{}")
        {
            StatisticSource sourceInfo;
            IAdapter adapter;

            sourceInfo = new StatisticSource()
            {
                SourceReference = new WeakReference<object>(source),
                SourceName = sourceName,
                SourceCategory = sourceCategory,
                SourceAcronym = sourceAcronym,
                StatisticMeasurementNameFormat = statisticMeasurementNameFormat
            };

            lock (StatisticSources)
            {
                object target;

                if (StatisticSources.Any(registeredSource => registeredSource.SourceReference.TryGetTarget(out target) && target == source))
                    throw new InvalidOperationException(string.Format("Unable to register {0} as statistic source because it is already registered.", sourceName));

                adapter = source as IAdapter;

                if ((object)adapter != null)
                {
                    adapter.Disposed += (sender, args) => Unregister(sender);

                    if (adapter.IsDisposed)
                        return;
                }

                StatisticSources.Add(sourceInfo);
            }

            OnSourceRegistered();
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
            object target;

            if (source != null)
            {
                lock (StatisticSources)
                {
                    for (int i = 0; i < StatisticSources.Count; i++)
                    {
                        if (StatisticSources[i].SourceReference.TryGetTarget(out target) && target == source)
                        {
                            StatisticSources.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
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

        // Gets the signal reference of the measurement associated with the given statistic and source pair.
        private static string GetSignalReference(Statistic statistic, StatisticSource source)
        {
            string arguments;
            Dictionary<string, string> substitutions;
            TemplatedExpressionParser parser;

            arguments = statistic.Arguments;
            substitutions = arguments.ParseKeyValuePairs();
            parser = new TemplatedExpressionParser();
            parser.TemplatedExpression = source.StatisticMeasurementNameFormat;

            if (substitutions.Count == 0)
            {
                substitutions = arguments
                    .Split(',')
                    .Select((arg, index) => Tuple.Create(index.ToString(), arg))
                    .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
            }

            substitutions = substitutions.ToDictionary(kvp => string.Concat("{", kvp.Key, "}"), kvp => kvp.Value);
            substitutions["{}"] = source.SourceName;

            return string.Format("{0}!{1}-ST{2}", parser.Execute(substitutions), source.SourceAcronym, statistic.Index);
        }

        // Triggered when a source registers with the statistics engine.
        private static void OnSourceRegistered()
        {
            EventHandler<EventArgs> sourceRegistered = SourceRegistered;

            if ((object)sourceRegistered != null)
                sourceRegistered(null, EventArgs.Empty);
        }

        private static void ValidateSourceReferences()
        {
            List<int> expiredSources = new List<int>();
            object target;

            lock (StatisticSources)
            {
                for (int i = 0; i < StatisticSources.Count; i++)
                {
                    if (!StatisticSources[i].SourceReference.TryGetTarget(out target))
                        expiredSources.Add(i);
                }

                for (int i = expiredSources.Count - 1; i >= 0; i--)
                    StatisticSources.RemoveAt(expiredSources[i]);
            }
        }

        #endregion
    }
}
