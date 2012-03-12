//******************************************************************************************************
//  StatisticsEngine.cs - Gbtc
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
//  02/22/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using TimeSeriesFramework.Adapters;
using TimeSeriesFramework.Transport;
using TVA;
using TVA.Configuration;
using TVA.Diagnostics;
using TVA.IO;

namespace TimeSeriesFramework.Statistics
{
    /// <summary>
    /// Represents the engine that computes statistics within applications of the TimeSeriesFramework.
    /// </summary>
    public class StatisticsEngine : FacileActionAdapterBase
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents arguments to an event that needs to pass a read-only
        /// collection of unmapped measurements to its subscribers.
        /// </summary>
        public class UnmappedMeasurementsEventArgs : EventArgs
        {
            private ReadOnlyCollection<IMeasurement> m_unmappedMeasurements;

            /// <summary>
            /// Creates a new instance of the <see cref="UnmappedMeasurementsEventArgs"/> class.
            /// </summary>
            /// <param name="undefinedMeasurements">The undefined measurements.</param>
            public UnmappedMeasurementsEventArgs(ReadOnlyCollection<IMeasurement> undefinedMeasurements)
            {
                m_unmappedMeasurements = undefinedMeasurements;
            }

            /// <summary>
            /// Gets the collection of undefined measurements.
            /// </summary>
            public ReadOnlyCollection<IMeasurement> UnmappedMeasurements
            {
                get
                {
                    return m_unmappedMeasurements;
                }
            }
        }


        // Events

        /// <summary>
        /// Event is raised when statistics are loaded from the database.
        /// </summary>
        public event EventHandler<UnmappedMeasurementsEventArgs> Loaded;

        /// <summary>
        /// Event is raised before statistics calculation.
        /// </summary>
        public event EventHandler BeforeCalculate;

        /// <summary>
        /// Event is raised after statistics calculation.
        /// </summary>
        public event EventHandler Calculated;


        // Fields
        private object m_timerLock;
        private System.Timers.Timer m_statisticCalculationTimer;
        private PerformanceMonitor m_performanceMonitor;
        private ManualResetEvent m_loadWaitHandle;

        private List<Statistic> m_statistics;
        private List<IMeasurement> m_definedMeasurements;
        private Dictionary<MeasurementKey, string> m_measurementSignalReferenceMap;
        private Dictionary<MeasurementKey, string> m_measurementSourceMap;
        private Dictionary<string, ICollection<object>> m_statisticSources;

        private InputAdapterCollection m_inputAdapters;
        private ActionAdapterCollection m_actionAdapters;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="StatisticsEngine"/> class.
        /// </summary>
        public StatisticsEngine()
        {
            if ((object)Default != null)
                throw new Exception("More than one statistics engine detected. Only one statistics engine is allowed to exist.");

            m_timerLock = new object();
            m_performanceMonitor = new PerformanceMonitor();
            m_loadWaitHandle = new ManualResetEvent(false);
            Default = this;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the collection of statistics measurements defined in the system.
        /// </summary>
        public ReadOnlyCollection<IMeasurement> DefinedMeasurements
        {
            get
            {
                return new ReadOnlyCollection<IMeasurement>(m_definedMeasurements);
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

            // Reset the wait handle so that the adapter cannot
            // start until loading of statistics is complete
            m_loadWaitHandle.Reset();

            // Set up the statistic calculation timer
            m_statisticCalculationTimer = new System.Timers.Timer();
            m_statisticCalculationTimer.Elapsed += StatisticCalculationTimer_Elapsed;
            m_statisticCalculationTimer.Interval = reportingInterval;
            m_statisticCalculationTimer.AutoReset = true;
            m_statisticCalculationTimer.Enabled = false;

            // Initialize collections
            m_statistics = new List<Statistic>();
            m_definedMeasurements = new List<IMeasurement>();
            m_measurementSignalReferenceMap = new Dictionary<MeasurementKey, string>();
            m_measurementSourceMap = new Dictionary<MeasurementKey, string>();
            m_statisticSources = new Dictionary<string, ICollection<object>>();

            // Kick off initial load of statistics from
            // thread pool since this may take a while
            ThreadPool.QueueUserWorkItem(LoadStatistics);
        }

        /// <summary>
        /// Starts the <see cref="StatisticsEngine"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the statistics engine or restarts it if it is already running.")]
        public override void Start()
        {
            base.Start();

            if (m_loadWaitHandle.WaitOne(DefaultInitializationTimeout))
            {
                lock (m_timerLock)
                {
                    if (!m_statisticCalculationTimer.Enabled)
                        m_statisticCalculationTimer.Start();
                }
            }
            else
            {
                OnProcessException(new Exception("Timeout waiting for loaded statistics."));
            }
        }

        /// <summary>
        /// Stops the <see cref="StatisticsEngine"/>.
        /// </summary>		
        [AdapterCommand("Stops the statistics engine.")]
        public override void Stop()
        {
            base.Stop();

            lock (m_timerLock)
            {
                if (m_statisticCalculationTimer.Enabled)
                    m_statisticCalculationTimer.Stop();
            }
        }

        /// <summary>
        /// Loads or reloads system statistics.
        /// </summary>
        [AdapterCommand("Reloads system statistics."), SuppressMessage("Microsoft.Reliability", "CA2001"), SuppressMessage("Microsoft.Maintainability", "CA1502")]
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
                Measurement definedMeasurement;
                Guid signalID;
                string assemblyName, typeName, methodName, signalReference;
                bool reenable;

                lock (m_timerLock)
                {
                    // Empty the statistics and measurements lists
                    m_statistics.Clear();
                    m_definedMeasurements.Clear();

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

                    OnStatusMessage("Loaded {0} statistic calculation definitions...", m_statistics.Count);

                    // Load statistical measurements
                    foreach (DataRow row in DataSource.Tables["ActiveMeasurements"].Select("SignalType='STAT'"))
                    {
                        signalReference = row["SignalReference"].ToString();

                        if (!string.IsNullOrEmpty(signalReference))
                        {
                            try
                            {
                                // Get measurement's point ID formatted as a measurement key
                                signalID = new Guid(row["SignalID"].ToNonNullString(Guid.NewGuid().ToString()));

                                // Create a measurement with a reference associated with this adapter
                                definedMeasurement = new Measurement()
                                {
                                    ID = signalID,
                                    Key = MeasurementKey.Parse(row["ID"].ToString(), signalID),
                                    TagName = signalReference,
                                    Adder = double.Parse(row["Adder"].ToNonNullString("0.0")),
                                    Multiplier = double.Parse(row["Multiplier"].ToNonNullString("1.0"))
                                };

                                // Add measurement to definition list keyed by signal reference
                                m_definedMeasurements.Add(definedMeasurement);
                                m_measurementSignalReferenceMap[definedMeasurement.Key] = signalReference;

                                // Map known measurement types to their sources
                                // TODO: Add a StatisticsCategory table to do mapping automatically
                                if (RegexMatch(signalReference, "SYSTEM"))
                                    MapMeasurement(definedMeasurement, "System");
                                else if (RegexMatch(signalReference, "PUB"))
                                    MapMeasurement(definedMeasurement, "Publisher");
                                else if (RegexMatch(signalReference, "SUB"))
                                    MapMeasurement(definedMeasurement, "Subscriber");
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(new InvalidOperationException(string.Format("Failed to load signal reference \"{0}\" due to exception: {1}", signalReference, ex.Message), ex));
                            }
                        }
                    }

                    OnLoaded();
                    OnStatusMessage("Loaded {0} statistic measurements...", m_definedMeasurements.Count);

                    if (reenable)
                    {
                        m_statisticCalculationTimer.Enabled = true;
                    }
                }
            }
            else
            {
                // Make sure statistic calculation timer is off since statistics aren't being processed
                Stop();
            }
        }

        /// <summary>
        /// Maps the given measurement to the given source name.
        /// </summary>
        /// <param name="measurement">The measurement to be mapped.</param>
        /// <param name="source">The name of the source of the measurement.</param>
        public void MapMeasurement(IMeasurement measurement, string source)
        {
            m_measurementSourceMap[measurement.Key] = source;
        }

        /// <summary>
        /// Adds the given source under the source collection with the given source name.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="source">The source to be added.</param>
        public void AddSource(string sourceName, object source)
        {
            ICollection<object> sourceCollection;

            if (!m_statisticSources.TryGetValue(sourceName, out sourceCollection))
            {
                sourceCollection = new List<object>();
                m_statisticSources.Add(sourceName, sourceCollection);
            }

            sourceCollection.Add(source);
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="StatisticsEngine"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="StatisticsEngine"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            IEnumerable<IMeasurement> publishedMeasurements;
            int publishedMeasurementCount = 0;

            if (Enabled)
            {
                publishedMeasurements = m_definedMeasurements.Where(measurement => m_measurementSourceMap.ContainsKey(measurement.Key));
                publishedMeasurementCount = publishedMeasurements.Count();
            }

            return string.Format("Currently publishing {0} statistics", publishedMeasurementCount).CenterText(maxLength);
        }

        /// <summary>
        /// Assigns the reference to the parent <see cref="IAdapterCollection"/> that will contain this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="parent">Parent adapter collection.</param>
        protected override void AssignParentCollection(IAdapterCollection parent)
        {
            base.AssignParentCollection(parent);

            if ((object)parent != null)
            {
                m_inputAdapters = parent.Parent.First(collection => collection is InputAdapterCollection) as InputAdapterCollection;
                m_actionAdapters = parent.Parent.First(collection => collection is ActionAdapterCollection) as ActionAdapterCollection;
            }
            else
            {
                m_inputAdapters = null;
                m_actionAdapters = null;
            }
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
                        if ((object)m_loadWaitHandle != null)
                        {
                            m_loadWaitHandle.Set();
                            m_loadWaitHandle.Dispose();
                            m_loadWaitHandle = null;
                        }

                        if ((object)m_statisticCalculationTimer != null)
                        {
                            m_statisticCalculationTimer.Elapsed -= StatisticCalculationTimer_Elapsed;
                            m_statisticCalculationTimer.Dispose();
                            m_statisticCalculationTimer = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    Default = null;             // Unset singleton instance.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        private void LoadStatistics(object state)
        {
            try
            {
                if (WaitForExternalEvents(DefaultInitializationTimeout))
                {
                    ReloadStatistics();
                    m_loadWaitHandle.Set();
                }
                else
                {
                    throw new Exception("Timeout waiting for external events.");
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void ClearSources()
        {
            m_statisticSources.Clear();
        }

        private void StatisticCalculationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<IMeasurement> calculatedStatistics = new List<IMeasurement>();
            DateTime serverTime;

            IEnumerable<IMeasurement> sourceMeasurements;
            IEnumerable<object> sources;
            IMeasurement clone;

            lock (m_timerLock)
            {
                OnBeforeCalculate();
                serverTime = DateTime.UtcNow;

                foreach (Statistic stat in m_statistics)
                {
                    sourceMeasurements = GetStatisticMeasurements(stat);
                    sources = GetSourceCollection(stat);

                    // Run calculations
                    foreach (object source in sources)
                    {
                        foreach (IMeasurement measurement in sourceMeasurements)
                        {
                            try
                            {
                                clone = Measurement.Clone(measurement);
                                clone.Timestamp = serverTime;
                                clone.Value = stat.Method(source, stat.Arguments);
                                calculatedStatistics.Add(clone);
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(new Exception(string.Format("Exception encountered while calculating statistic {0}: {1}", stat.Index, ex.Message), ex));
                            }
                        }
                    }
                }

                OnNewMeasurements(calculatedStatistics);
                OnCalculated();
            }
        }

        private IEnumerable<IMeasurement> GetStatisticMeasurements(Statistic stat)
        {
            string signalReferenceSuffix = string.Format("-ST{0}", stat.Index);

            return m_definedMeasurements.Where(definedMeasurement => m_measurementSourceMap.ContainsKey(definedMeasurement.Key))
                .Where(definedMeasurement => m_measurementSourceMap[definedMeasurement.Key] == stat.Source)
                .Where(definedMeasurement => m_measurementSignalReferenceMap[definedMeasurement.Key].EndsWith(signalReferenceSuffix));
        }

        private IEnumerable<object> GetSourceCollection(Statistic stat)
        {
            ICollection<object> sourceCollection;

            if (!m_statisticSources.TryGetValue(stat.Source, out sourceCollection))
                sourceCollection = new List<object>();

            return sourceCollection;
        }

        private void OnLoaded()
        {
            if ((object)Loaded != null)
            {
                List<IMeasurement> unmappedMeasurementList = m_definedMeasurements.Where(m => !m_measurementSourceMap.ContainsKey(m.Key)).ToList();
                ReadOnlyCollection<IMeasurement> unmappedMeasurements = new ReadOnlyCollection<IMeasurement>(unmappedMeasurementList);
                UnmappedMeasurementsEventArgs args = new UnmappedMeasurementsEventArgs(unmappedMeasurements);

                Loaded(this, args);
            }
        }

        private void OnBeforeCalculate()
        {
            // Sources are likely to change throughout the lifetime of
            // the statistics engine. In order to keep them up to date,
            // they must be refreshed each time we calculated statistics.
            ClearSources();

            // Add the performance monitor as the source for system statistics
            AddSource("System", m_performanceMonitor);

            // Add all data subscribers as sources for subscriber statistics
            foreach (IAdapter subscriber in m_inputAdapters.Where<IInputAdapter>(adapter => adapter is DataSubscriber))
                AddSource("Subscriber", subscriber);

            // Add all data publishers as sources for publisher statistics
            foreach (IAdapter publisher in m_actionAdapters.Where<IActionAdapter>(adapter => adapter is DataPublisher))
                AddSource("Publisher", publisher);

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
        private static StatisticsEngine s_default;
        private static ManualResetEventSlim s_defaultWaitHandle;

        // Static Constructor

        /// <summary>
        /// Static initialization of <see cref="StatisticsEngine"/> class.
        /// </summary>
        static StatisticsEngine()
        {
            s_defaultWaitHandle = new ManualResetEventSlim();
        }

        // Static Properties

        /// <summary>
        /// Gets the <see cref="StatisticsEngine"/> singleton instance.
        /// </summary>
        public static StatisticsEngine Default
        {
            get
            {
                return s_default;
            }
            private set
            {
                if ((object)value == null)
                    s_defaultWaitHandle.Reset();

                s_default = value;

                if ((object)value != null)
                    s_defaultWaitHandle.Set();
            }
        }

        // Static Methods

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
            return Regex.IsMatch(signalReference, suffix);
        }

        /// <summary>
        /// Waits for the default instance to be created.
        /// </summary>
        /// <param name="timeout">The amount of time to wait before giving up.</param>
        /// <returns>Boolean to indicate whether the default instance was created or not.</returns>
        public static bool WaitForDefaultInstance(int timeout)
        {
            return s_defaultWaitHandle.Wait(timeout);
        }

        #endregion
    }
}
