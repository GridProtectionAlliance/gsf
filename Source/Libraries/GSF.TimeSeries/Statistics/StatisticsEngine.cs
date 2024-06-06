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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Net;
using GSF.Net.Snmp;
using GSF.Parsing;
using GSF.Threading;
using GSF.TimeSeries.Adapters;
using Timer = System.Timers.Timer;

#pragma warning disable 414

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable MemberCanBePrivate.Local
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
        private readonly struct SignalReference
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
                    Acronym = match.Groups[nameof(Acronym)].Value;
                    Index = Convert.ToInt32(match.Groups[nameof(Index)].Value);
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

            public DBUpdateHelper(AdoDataConnection database) => 
                m_database = database;

            #endregion

            #region [ Properties ]

            public StatisticSource Source
            {
                get => m_source;
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
                get => m_statistic;
                set
                {
                    m_statistic = value;
                    m_name = null;
                    m_pointTag = null;
                    m_signalReference = null;
                    m_description = null;
                }
            }

            public string Name => m_name ??= GetName();

            public int HistorianID => m_historianID ?? (int)(m_historianID = GetHistorianID());

            public object DeviceID => m_deviceID ??= GetDeviceID();

            public string PointTag => m_pointTag ??= GetPointTag();

            public int SignalTypeID => m_signalTypeID ?? (int)(m_signalTypeID = GetSignalTypeID());

            public string SignalReference => m_signalReference ??= GetSignalReference();

            public string Description => m_description ??= GetDescription();

            public int Index => Convert.ToInt32(m_statistic["SignalIndex"]);

            private Guid NodeID => m_nodeID ?? (Guid)(m_nodeID = GetNodeID());

            private string Company => m_company ??= GetCompany();

            private string NodeOwner => m_nodeOwner ??= GetNodeOwner();

            private string Category => m_source.SourceCategory;

            private string Acronym => m_source.SourceAcronym;

            private Dictionary<string, DataRow> DeviceLookup => m_deviceLookup ??= GetDeviceLookup();

            private Dictionary<int, string> CompanyLookup => m_companyLookup ??= GetCompanyLookup();

            #endregion

            #region [ Methods ]

            private string GetName()
            {
                string arguments = m_statistic.Field<string>("Arguments");

                if (string.IsNullOrWhiteSpace(arguments))
                    return m_source.SourceName;

                Dictionary<string, string> substitutions = arguments.ParseKeyValuePairs();
                TemplatedExpressionParser parser = new()
                {
                    TemplatedExpression = m_source.StatisticMeasurementNameFormat
                };

                if (substitutions.Count == 0)
                {
                    substitutions = arguments
                        .Split(',')
                        .Select((arg, index) => Tuple.Create(index.ToString(), arg))
                        .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
                }

                substitutions = substitutions.ToDictionary(kvp => $"{{{kvp.Key}}}", kvp => kvp.Value);
                substitutions["{}"] = m_source.SourceName;

                return parser.Execute(substitutions);
            }

            private int GetHistorianID()
            {
                const string StatHistorianIDFormat = "SELECT ID FROM Historian WHERE Acronym = 'STAT' AND NodeID = {0}";
                return ExecuteScalar<int>(StatHistorianIDFormat, m_database.Guid(NodeID));
            }

            private object GetDeviceID() => 
                !DeviceLookup.TryGetValue(Name, out DataRow device) ? DBNull.Value : device[nameof(ID)];

            private string GetPointTag() => 
                $"{Company}_{Name}!{Acronym}:ST{Index}";

            private int GetSignalTypeID()
            {
                const string SignalTypeIDFormat = "SELECT ID FROM SignalType WHERE Acronym = 'STAT'";
                return ExecuteScalar<int>(SignalTypeIDFormat);
            }

            private string GetSignalReference() => 
                $"{Name}!{Acronym}-ST{Index}";

            private string GetDescription() => 
                $"{Category} statistic for {m_statistic[nameof(Description)].ToNonNullString()}";

            private string GetCompany()
            {
                string company = null;

                bool isNodeOwner = !DeviceLookup.TryGetValue(Name, out DataRow device) ||
                                   !int.TryParse(device["CompanyID"].ToNonNullString(), out int companyID) ||
                                   !CompanyLookup.TryGetValue(companyID, out company);

                return isNodeOwner ? NodeOwner : company;
            }

            private string GetNodeOwner()
            {
                const string NodeCompanyIDFormat = "SELECT CompanyID FROM Node WHERE ID = {0}";
                const string CompanyAcronymFormat = "SELECT MapAcronym FROM Company WHERE ID = {0}";

                string companyAcronym;

                try
                {
                    int nodeCompanyID = ExecuteScalar<int>(NodeCompanyIDFormat, m_database.Guid(NodeID));
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
                    .ToDictionary(row => row[nameof(Acronym)].ToNonNullString());
            }

            private Dictionary<int, string> GetCompanyLookup()
            {
                const string CompanyLookupFormat = "SELECT ID, Acronym FROM Company";
                int id = 0;

                return RetrieveData(CompanyLookupFormat).Select()
                    .Where(row => int.TryParse(row[nameof(ID)].ToNonNullString(), out id))
                    .ToDictionary(_ => id, row => row[nameof(Acronym)].ToNonNullString());
            }

            public T ExecuteScalar<T>(string queryFormat, params object[] parameters)
            {
                string query = m_database.ParameterizedQueryString(queryFormat, parameters.Select((_, index) => $"p{index}").ToArray());
                return (T)Convert.ChangeType(m_database.Connection.ExecuteScalar(query, DataExtensions.DefaultTimeoutDuration, parameters), typeof(T));
            }

            public void ExecuteNonQuery(string queryFormat, params object[] parameters)
            {
                string query = m_database.ParameterizedQueryString(queryFormat, parameters.Select((_, index) => $"p{index}").ToArray());
                m_database.Connection.ExecuteNonQuery(query, DataExtensions.DefaultTimeoutDuration, parameters);
            }

            public DataTable RetrieveData(string queryFormat, params object[] parameters)
            {
                string query = m_database.ParameterizedQueryString(queryFormat, parameters.Select((_, index) => $"p{index}").ToArray());
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

        /// <summary>
        /// Event is raised when a new statistics source is registered.
        /// </summary>
        public static event EventHandler<EventArgs<object>> SourceRegistered;

        /// <summary>
        /// Event is raised when a statistics source is unregistered.
        /// </summary>
        public static event EventHandler<EventArgs<object>> SourceUnregistered;

        // Fields
        private readonly object m_statisticsLock;
        private readonly List<Statistic> m_statistics;
        private readonly ConcurrentDictionary<MeasurementKey, double> m_currentStatistics;

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
            m_currentStatistics = new ConcurrentDictionary<MeasurementKey, double>();
            m_reloadStatisticsTimer = new Timer();
            m_statisticCalculationTimer = new Timer();

            // Set up the statistic calculation timer
            m_reloadStatisticsTimer.Elapsed += ReloadStatisticsTimer_Elapsed;
            m_reloadStatisticsTimer.Interval = 1.0D;
            m_reloadStatisticsTimer.AutoReset = false;
            m_reloadStatisticsTimer.Enabled = false;

            m_updateStatisticMeasurementsOperation = new LongSynchronizedOperation(UpdateStatisticMeasurements, ex =>
            {
                string message = $"An error occurred while attempting to update statistic measurement definitions: {ex.Message}";
                OnProcessException(MessageLevel.Info, new InvalidOperationException(message, ex));
            });

            m_loadStatisticsOperation = new LongSynchronizedOperation(LoadStatistics, ex =>
            {
                string message = $"An error occurred while attempting to load statistic definitions: {ex.Message}";
                OnProcessException(MessageLevel.Info, new InvalidOperationException(message, ex));
            });

            m_calculateStatisticsOperation = new LongSynchronizedOperation(CalculateStatistics, ex =>
            {
                string message = $"An error occurred while attempting to calculate statistics: {ex.Message}";
                OnProcessException(MessageLevel.Info, new InvalidOperationException(message, ex));
            });

            m_validateSourceReferencesOperation = new ShortSynchronizedOperation(ValidateSourceReferences, ex =>
            {
                string message = $"An error occurred while attempting to validate statistic source references: {ex.Message}";
                OnProcessException(MessageLevel.Info, new InvalidOperationException(message, ex));
            });

            m_updateStatisticMeasurementsOperation.IsBackground = true;
            m_loadStatisticsOperation.IsBackground = true;
            m_calculateStatisticsOperation.IsBackground = true;

            m_performanceMonitor = new PerformanceMonitor();

            SourceRegistered += HandleSourceRegistered;

            // Track active statistics engine singleton instance
            s_activeEngine = this;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to the <see cref="StatisticsEngine"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get => base.DataSource;
            set
            {
                base.DataSource = value;
                RestartReloadStatisticsTimer();
            }
        }

        /// <inheritdoc />
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Returns the detailed status of the statistics engine.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new(base.Status);

                status.AppendLine($"          Statistics count: {m_statistics.Count:N0}");
                status.AppendLine($" Recently calculated stats: {m_lastStatisticCalculationCount:N0}");
                status.AppendLine($"     Last stat calculation: {m_lastStatisticCalculationTime:yyyy-MM-dd HH:mm:ss}");

                lock (s_statisticSources)
                    status.AppendLine($"    Statistic source count: {s_statisticSources.Count:N0}");

                status.AppendLine($"Forward statistics to SNMP: {s_forwardToSnmp}");

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
            base.Initialize();
            
            Dictionary<string, string> settings = Settings;

            // Load the statistic reporting interval
            m_reportingInterval = settings.TryGetValue("reportingInterval", out string setting) ? double.Parse(setting) * 1000.0 : 10000.0;

            if (UseLocalClockAsRealTime || !TrackLatestMeasurements)
            {
                // Set up the statistic calculation timer
                m_statisticCalculationTimer.Elapsed += StatisticCalculationTimer_Elapsed;
                m_statisticCalculationTimer.Interval = m_reportingInterval;
                m_statisticCalculationTimer.AutoReset = true;
                m_statisticCalculationTimer.Enabled = false;
            }

            // Register system as a statistics source
            Register(m_performanceMonitor, GetSystemName(), nameof(System), "SYSTEM");
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            foreach (IMeasurement measurement in measurements)
            {
                if (m_lastStatisticCalculationTime == default)
                    m_lastStatisticCalculationTime = measurement.Timestamp;

                base.QueueMeasurementsForProcessing(new [] { measurement });

                if (UseLocalClockAsRealTime || !TrackLatestMeasurements)
                    continue;

                if (RealTime >= m_lastStatisticCalculationTime.AddMilliseconds(m_reportingInterval).Ticks)
                    CalculateStatistics();
            }
        }

        /// <summary>
        /// Starts the <see cref="StatisticsEngine"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the statistics engine or restarts it if it is already running.", "Administrator", "Editor")]
        public override void Start()
        {
            base.Start();

            if (!UseLocalClockAsRealTime && TrackLatestMeasurements)
                return;

            m_statisticCalculationTimer.Start();
            OnStatusMessage(MessageLevel.Info, "Started statistics calculation timer.");
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
        public override string GetShortStatus(int maxLength) => $"Last published {m_lastStatisticCalculationCount} statistics".CenterText(maxLength);

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="StatisticsEngine"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (m_reloadStatisticsTimer is not null)
                {
                    m_reloadStatisticsTimer.Elapsed -= ReloadStatisticsTimer_Elapsed;
                    m_reloadStatisticsTimer.Dispose();
                    m_reloadStatisticsTimer = null;
                }

                if (m_statisticCalculationTimer is not null)
                {
                    m_statisticCalculationTimer.Elapsed -= StatisticCalculationTimer_Elapsed;
                    m_statisticCalculationTimer.Dispose();
                    m_statisticCalculationTimer = null;
                }

                Unregister(m_performanceMonitor);
            }
            finally
            {
                m_disposed = true;       // Prevent duplicate dispose.
                base.Dispose(disposing); // Call base class Dispose().
            }
        }

        private void UpdateStatisticMeasurements()
        {
            const string StatisticSelectFormat = "SELECT Source, SignalIndex, Arguments, Description FROM Statistic WHERE Enabled <> 0";
            const string StatisticMeasurementSelectFormat = "SELECT SignalReference FROM Measurement WHERE SignalReference IN ({0})";
            const string StatisticMeasurementInsertFormat = "INSERT INTO Measurement(HistorianID, DeviceID, PointTag, SignalTypeID, SignalReference, Description, Enabled) VALUES({0}, {1}, {2}, {3}, {4}, {5}, 1)";
            const string StatisticMeasurementCountFormat = "SELECT COUNT(*) FROM Measurement WHERE SignalReference = {0}";

            StatisticSource[] sources;
            bool configurationChanged = false;

            lock (s_statisticSources)
            {
                // Obtain a snapshot of the sources that are
                // currently registered with the statistics engine
                sources = s_statisticSources.ToArray();
            }

            using (AdoDataConnection database = new("systemSettings"))
            {
                // Handles database queries, caching, and lazy loading for
                // determining the parameters to send in to each INSERT query
                DBUpdateHelper helper = new(database);

                // Load statistics from the statistics table to determine
                // what statistics should be defined for each source
                Dictionary<string, List<DataRow>> statisticsLookup = helper.RetrieveData(StatisticSelectFormat).Select()
                    .GroupBy(row => row["Source"].ToNonNullString())
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

                // Make sure the full set of statistic measurements are defined for each source
                foreach (StatisticSource source in sources)
                {
                    // If statistic measurements have already been updated for this source,
                    // do not attempt to update them again. This helps to prevent race conditions
                    // between configuration changes and statistics engine registration
                    if (source.HasUpdatedStatisticMeasurements)
                        continue;

                    // If no statistics exist for this category,
                    // there are no statistics that can be created for this source
                    if (!statisticsLookup.TryGetValue(source.SourceCategory, out List<DataRow> statistics))
                        continue;

                    // Build a list of signal references for this source
                    // based on the statistics in this category
                    List<string> signalReferences = new();

                    helper.Source = source;

                    foreach (DataRow statistic in statistics)
                    {
                        helper.Statistic = statistic;
                        signalReferences.Add(helper.SignalReference);
                    }

                    // Get the statistic measurements from the database which have already been defined for this source
                    string args = string.Join(",", signalReferences.Select((_, index) => $"{{{index}}}"));
                    List<DataRow> statisticMeasurements = helper.RetrieveData(string.Format(StatisticMeasurementSelectFormat, args), signalReferences.ToArray<object>()).Select().ToList();

                    // If the number of statistics for the source category matches
                    // the number of statistic measurements for the source, assume
                    // all is well and move to the next source
                    if (statistics.Count == statisticMeasurements.Count)
                        continue;

                    // Get a collection of signal indexes already have statistic measurements
                    HashSet<int> existingIndexes = new(statisticMeasurements
                        .Select(measurement => measurement[nameof(SignalReference)].ToNonNullString())
                        .Select(str => new SignalReference(str))
                        .Select(signalReference => signalReference.Index));

                    // Create statistic measurements for statistics that do not have any
                    foreach (DataRow statistic in statistics)
                    {
                        helper.Statistic = statistic;

                        // If a measurement already exists for this statistic, skip it
                        if (existingIndexes.Contains(helper.Index))
                            continue;

                        // It has been observed in field deployments that the statistic measurement may already exist, but
                        // sometimes the above check will fail which creates duplicate measurements. To prevent duplicate
                        // statistic measurements from being created, the following code is a secondary check to make sure
                        // the statistic measurement does not already exist before attempting to insert it. Performance
                        // impact will be limited to the first time the statistic is created for a given source or in the
                        // rare cases when the above statistic measurement existing index check fails.
                        if (helper.ExecuteScalar<int>(StatisticMeasurementCountFormat, helper.SignalReference) > 0)
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
            lock (m_statisticsLock)
            {
                // Empty the statistics list
                m_statistics.Clear();

                // Load all defined statistics
                foreach (DataRow row in DataSource.Tables[nameof(Statistics)].Select("Enabled <> 0", "Source, SignalIndex"))
                {
                    // Create a new statistic
                    Statistic statistic = new()
                    {
                        // Load primary statistic parameters
                        Source = row["Source"].ToNonNullString(), 
                        Index = int.Parse(row["SignalIndex"].ToNonNullString("-1")), 
                        Arguments = row["Arguments"].ToNonNullString()
                    };

                    try
                    {
                        statistic.DataType = Type.GetType(row[nameof(DataType)].ToNonNullString());
                    }
                    catch
                    {
                        statistic.DataType = typeof(double);
                    }

                    // Load statistic's code location information
                    string assemblyName = row[nameof(AssemblyName)].ToNonNullString();
                    string typeName = row["TypeName"].ToNonNullString();
                    string methodName = row["MethodName"].ToNonNullString();

                    if (string.IsNullOrEmpty(assemblyName))
                        throw new InvalidOperationException("Statistic assembly name was not defined.");

                    if (string.IsNullOrEmpty(typeName))
                        throw new InvalidOperationException("Statistic type name was not defined.");

                    if (string.IsNullOrEmpty(methodName))
                        throw new InvalidOperationException("Statistic method name was not defined.");

                    try
                    {
                        // See if statistic is defined in this assembly (no need to reload)
                        if (string.Compare(GetType().FullName, typeName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // Assign statistic handler to local method (assumed to be private static)
                            MethodInfo method = GetType().GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

                            if (method is null)
                                throw new NullReferenceException($"Method info for \"{typeName}.{methodName}\" was null");

                            statistic.Method = (StatisticCalculationFunction)Delegate.CreateDelegate(typeof(StatisticCalculationFunction), method);
                        }
                        else
                        {
                            // Load statistic method from containing assembly and type
                            Assembly assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(assemblyName));
                            Type type = assembly.GetType(typeName);
                            MethodInfo method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod);

                            if (method is null)
                                throw new NullReferenceException($"Method info for \"{typeName}.{methodName}\" was null");

                            // Assign statistic handler to loaded assembly method
                            statistic.Method = (StatisticCalculationFunction)Delegate.CreateDelegate(typeof(StatisticCalculationFunction), method);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to load statistic handler \"{row[nameof(Name)].ToNonNullString("n/a")}\" from \"{assemblyName} [{typeName}::{methodName}()]\" due to exception: {ex.Message}", ex));
                    }

                    // Add statistic to list
                    m_statistics.Add(statistic);
                }
            }

            StatisticSource[] sources;

            lock (s_statisticSources)
            {
                // Obtain a snapshot of the sources that are
                // currently registered with the statistics engine
                sources = s_statisticSources.ToArray();
            }

            // Create a lookup table from signal reference to statistic source
            Dictionary<string, StatisticSource> sourceLookup = new();

            foreach (Tuple<StatisticSource, IEnumerable<Statistic>> mapping in sources.GroupJoin(m_statistics, src => src.SourceCategory, stat => stat.Source, Tuple.Create))
            {
                foreach (Statistic stat in mapping.Item2)
                {
                    string signalReference = GetSignalReference(stat, mapping.Item1);

                    if (sourceLookup.ContainsKey(signalReference))
                        OnStatusMessage(MessageLevel.Warning, $"Encountered duplicate signal reference statistic: {signalReference}");
                    else
                        sourceLookup.Add(signalReference, mapping.Item1);
                }
            }

            // Create a lookup table from statistic source to a
            // list of data rows from the ActiveMeasurements table
            Dictionary<StatisticSource, List<DataRow>> activeMeasurementsLookup = new();
            long statisticMeasurementCount = 0L;

            foreach (DataRow row in DataSource.Tables["ActiveMeasurements"].Select("SignalType = 'STAT'"))
            {
                if (!sourceLookup.TryGetValue(row.Field<string>(nameof(SignalReference)), out StatisticSource source))
                    continue;

                List<DataRow> statisticMeasurements = activeMeasurementsLookup.GetOrAdd(source, _ => new List<DataRow>());
                statisticMeasurements.Add(row);
                statisticMeasurementCount++;
            }

            // Update StatisticMeasurements collections for all sources
            foreach (StatisticSource src in sources)
            {
                if (activeMeasurementsLookup.TryGetValue(src, out List<DataRow> statisticMeasurements))
                    src.StatisticMeasurements = statisticMeasurements;
            }

            m_currentStatistics.Clear();

            OnStatusMessage(MessageLevel.Info, $"Loaded {m_statistics.Count} statistic calculation definitions and {statisticMeasurementCount} statistic measurement definitions.");
        }

        private void CalculateStatistics()
        {
            try
            {
                List<IMeasurement> calculatedStatistics = new();
                StatisticSource[] sources;
                Statistic[] statistics;

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
                DateTime serverTime = RealTime;

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
                string errorMessage = $"Error encountered while calculating statistics: {ex.Message}";
                OnProcessException(MessageLevel.Info, new Exception(errorMessage, ex));
            }
        }

        private IEnumerable<IMeasurement> CalculateStatistics(Statistic[] statistics, DateTime serverTime, StatisticSource source)
        {
            try
            {
                List<DataRow> measurements = source.StatisticMeasurements;

                // Calculate statistics
                if (measurements is not null)
                {
                    return measurements
                        .Select(measurement => CalculateStatistic(statistics, serverTime, source, measurement))
                        .Where(calculatedStatistic => calculatedStatistic is not null);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error calculating statistics for {source.SourceName}: {ex.Message}";
                OnProcessException(MessageLevel.Info, new Exception(errorMessage, ex));
            }

            return Enumerable.Empty<IMeasurement>();
        }

        private IMeasurement CalculateStatistic(Statistic[] statistics, DateTime serverTime, StatisticSource source, DataRow measurement)
        {
            if (source.SourceReference.TryGetTarget(out object target))
            {
                try
                {
                    // Get the signal ID and signal reference of the current measurement
                    Guid signalID = Guid.Parse(measurement["SignalID"].ToString());
                    string signalReference = measurement[nameof(SignalReference)].ToString();
                    int signalIndex = Convert.ToInt32(signalReference.Substring(signalReference.LastIndexOf("-ST", StringComparison.Ordinal) + 3));

                    // Find the statistic corresponding to the current measurement
                    Statistic statistic = statistics.FirstOrDefault(stat => source.SourceCategory == stat.Source && signalIndex == stat.Index);

                    if (statistic is not null)
                    {
                        MeasurementKey key = MeasurementKey.LookUpOrCreate(signalID, measurement[nameof(ID)].ToString());

                        double value = statistic.Method(target, statistic.Arguments);

                        if (s_forwardToSnmp && OID.SnmpStats.TryGetValue(source.SourceCategory, out uint[] categoryOID))
                        {
                            try
                            {
                                uint[] statisticOID = categoryOID.Append((uint)signalIndex);
                                uint[] statisticValueOID = statisticOID.Append(1U);
                                uint[] statisticReferenceOID = statisticOID.Append(2U);
                                Variable statisticReference = new(statisticReferenceOID, new OctetString(signalReference));

                                if (statistic.DataType == typeof(int) || statistic.DataType == typeof(bool))
                                    Snmp.SendTrap(new Variable(statisticValueOID, new Integer32((int)value)), statisticReference);
                                else if (statistic.DataType == typeof(double))
                                    Snmp.SendTrap(new Variable(statisticValueOID, new OctetString(value.ToString(CultureInfo.InvariantCulture))), statisticReference);
                                else if (statistic.DataType == typeof(DateTime))
                                    Snmp.SendTrap(new Variable(statisticValueOID, new OctetString(new DateTime((long)value).ToString("mm':'ss'.'fff"))), statisticReference);
                                else
                                    Snmp.SendTrap(new Variable(statisticValueOID, new OctetString(Convert.ChangeType(value, statistic.DataType).ToString())), statisticReference);
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to publish statistic \"{signalReference}\" via SNMP: {ex.Message}", ex));
                            }
                        }

                        // Track latest value of the statistic measurement
                        m_currentStatistics[key] = value;

                        // Calculate the current value of the statistic measurement
                        return new Measurement()
                        {
                            Metadata = key.Metadata,
                            Value = value,
                            Timestamp = serverTime
                        };
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Error calculating statistic for {measurement[nameof(SignalReference)]}: {ex.Message}";
                    OnProcessException(MessageLevel.Info, new Exception(errorMessage, ex));
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
            if (DataSource.Tables.Contains("NodeInfo"))
            {
                return DataSource.Tables["NodeInfo"].Rows[0][nameof(Name)]
                    .ToNonNullString()
                    .RemoveCharacters(c => !char.IsLetterOrDigit(c))
                    .Replace(' ', '_')
                    .ToUpper();
            }

            using AdoDataConnection database = new("systemSettings");

            return database.Connection.ExecuteScalar($"SELECT Name FROM Node WHERE ID = '{database.Guid(GetNodeID())}'").ToNonNullString().ToUpper();
        }

        private void RestartReloadStatisticsTimer()
        {
            try
            {
                Timer reloadStatisticsTimer = m_reloadStatisticsTimer;

                if (reloadStatisticsTimer is null)
                    return;

                reloadStatisticsTimer.Stop();
                reloadStatisticsTimer.Start();
            }
            catch (ObjectDisposedException ex)
            {
                string message = $"ReloadStatisticsTimer was disposed on another thread: {ex.Message}";
                Exception wrapper = new ObjectDisposedException(message, ex);
                OnProcessException(MessageLevel.Warning, wrapper);
            }
        }

        private void HandleSourceRegistered(object sender, EventArgs eventArgs)
        {
            if (DataSource is not null)
                RestartReloadStatisticsTimer();
        }

        private void ReloadStatisticsTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs) => 
            ReloadStatistics();

        // If multiple timer events overlap, try-run will make sure only one is running at once
        private void StatisticCalculationTimer_Elapsed(object sender, ElapsedEventArgs e) => 
            m_calculateStatisticsOperation.TryRunOnce();

        private void OnBeforeCalculate()
        {
            try
            {
                BeforeCalculate?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        private void OnCalculated()
        {
            try
            {
                Calculated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static StatisticsEngine s_activeEngine;
        private static readonly List<StatisticSource> s_statisticSources;
        private static readonly bool s_forwardToSnmp;

        // Static Constructor

        /// <summary>
        /// Static initialization of <see cref="StatisticsEngine"/> class.
        /// </summary>
        static StatisticsEngine()
        {
            s_statisticSources = new List<StatisticSource>();

            CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings["systemSettings"];

            settings.Add("ForwardStatisticsToSnmp", "false", "Defines flag that determines if statistics should be published as SNMP trap messages.");

            s_forwardToSnmp = settings["ForwardStatisticsToSnmp"].ValueAs(false);
        }

        // Static Properties

        /// <summary>
        /// Gets the statistic measurements and latest values that are registered with the active statistics engine.
        /// </summary>
        public static ReadOnlyDictionary<MeasurementKey, double> CurrentStatistics
        {
            get
            {
                StatisticsEngine instance = s_activeEngine;

                return instance is null ? 
                    new ReadOnlyDictionary<MeasurementKey, double>(new Dictionary<MeasurementKey, double>()) : 
                    new ReadOnlyDictionary<MeasurementKey, double>(instance.m_currentStatistics);
            }
        }

        // Static Methods

        /// <summary>
        /// Registers the given adapter with the statistics engine as a source of statistics.
        /// </summary>
        /// <param name="adapter">The source of the statistics.</param>
        /// <param name="sourceCategory">The category of the statistics.</param>
        /// <param name="sourceAcronym">The acronym used in signal references.</param>
        /// <param name="statisticMeasurementNameFormat">Format string used to name statistic measurements for this source.</param>
        public static void Register(IAdapter adapter, string sourceCategory, string sourceAcronym, string statisticMeasurementNameFormat = "{}") => 
            Register(adapter, adapter.Name, sourceCategory, sourceAcronym, statisticMeasurementNameFormat);

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
            RuntimeHelpers.RunClassConstructor(typeof(GlobalDeviceStatistics).TypeHandle);

            StatisticSource sourceInfo = new()
            {
                SourceReference = new WeakReference<object>(source),
                SourceName = sourceName,
                SourceCategory = sourceCategory,
                SourceAcronym = sourceAcronym,
                StatisticMeasurementNameFormat = statisticMeasurementNameFormat
            };

            lock (s_statisticSources)
            {
                if (s_statisticSources.Any(registeredSource => registeredSource.SourceReference.TryGetTarget(out object target) && target == source))
                    throw new InvalidOperationException($"Unable to register {sourceName} as statistic source because it is already registered.");

                if (source is IAdapter adapter)
                {
                    adapter.Disposed += (sender, _) => Unregister(sender);

                    if (adapter.IsDisposed)
                        return;
                }

                s_statisticSources.Add(sourceInfo);
            }

            OnSourceRegistered(source);
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
            if (source is null)
                return;

            lock (s_statisticSources)
            {
                for (int i = 0; i < s_statisticSources.Count; i++)
                {
                    if (!s_statisticSources[i].SourceReference.TryGetTarget(out object target) || target != source)
                        continue;

                    s_statisticSources.RemoveAt(i);
                    break;
                }
            }

            OnSourceUnregistered(source);
        }

        /// <summary>
        /// Attempts to lookup statistic source and signal index from a measurement <paramref name="signalReference"/>.
        /// </summary>
        /// <param name="signalReference">Signal reference.</param>
        /// <param name="sourceCategory">Statistic source category as defined in Statistics table Source column.</param>
        /// <param name="signalIndex">Statistic signal index as defined in Statistics table SignalIndex column.</param>
        /// <returns><c>true</c> if lookup succeeds; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The statistic source acronyms used in measurement signal references are defined dynamically by
        /// statistic sources during registration. This function returns the source category, as defined in
        /// the Statistic table, for a given signal reference to allow for reverse lookups. As a result,
        /// user will need to make sure that all statistic sources have been registered before calling
        /// this function in order to receive accurate results.
        /// </remarks>
        public static bool TryLookupStatisticSource(string signalReference, out string sourceCategory, out int signalIndex)
        {
            sourceCategory = null;
            signalIndex = 0;

            if (string.IsNullOrWhiteSpace(signalReference))
                return false;

            int statSuffix = signalReference.LastIndexOf('!');

            if (statSuffix < 0)
                return false;

            int statIndex = signalReference.LastIndexOf("-ST", StringComparison.Ordinal);

            if (statIndex < 0)
                return false;

            string acronym = signalReference.Substring(statSuffix + 1, statIndex - statSuffix - 1);
            signalIndex = Convert.ToInt32(signalReference.Substring(statIndex + 3));

            lock (s_statisticSources)
            {
                foreach (StatisticSource statisticSource in s_statisticSources)
                {
                    if (!statisticSource.SourceAcronym.Equals(acronym, StringComparison.OrdinalIgnoreCase))
                        continue;

                    sourceCategory = statisticSource.SourceCategory;
                    return true;
                }
            }

            return false;
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
            string regex = $@"!{suffix}-ST\d+$";
            return Regex.IsMatch(signalReference, regex);
        }

        // Gets the signal reference of the measurement associated with the given statistic and source pair.
        private static string GetSignalReference(Statistic statistic, StatisticSource source)
        {
            string arguments = statistic.Arguments;
            Dictionary<string, string> substitutions = arguments.ParseKeyValuePairs();
            TemplatedExpressionParser parser = new()
            {
                TemplatedExpression = source.StatisticMeasurementNameFormat
            };

            if (substitutions.Count == 0)
            {
                substitutions = arguments
                    .Split(',')
                    .Select((arg, index) => Tuple.Create(index.ToString(), arg))
                    .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
            }

            substitutions = substitutions.ToDictionary(kvp => $"{{{kvp.Key}}}", kvp => kvp.Value);
            substitutions["{}"] = source.SourceName;

            return $"{parser.Execute(substitutions)}!{source.SourceAcronym}-ST{statistic.Index}";
        }

        // Triggered when a source registers with the statistics engine.
        private static void OnSourceRegistered(object source)
        {
            try
            {
                SourceRegistered?.Invoke(typeof(StatisticsEngine), new EventArgs<object>(source));
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        // Triggered when a source unregisters with the statistics engine.
        private static void OnSourceUnregistered(object source)
        {
            try
            {
                SourceUnregistered?.Invoke(typeof(StatisticsEngine), new EventArgs<object>(source));
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        private static void ValidateSourceReferences()
        {
            List<int> expiredSources = new();

            lock (s_statisticSources)
            {
                for (int i = 0; i < s_statisticSources.Count; i++)
                {
                    if (!s_statisticSources[i].SourceReference.TryGetTarget(out object _))
                        expiredSources.Add(i);
                }

                for (int i = expiredSources.Count - 1; i >= 0; i--)
                    s_statisticSources.RemoveAt(expiredSources[i]);
            }
        }

        private static Guid GetNodeID() =>
            Guid.Parse(ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].Value);

        #endregion
    }
}
