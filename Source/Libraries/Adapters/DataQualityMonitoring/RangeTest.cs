//******************************************************************************************************
//  RangeTest.cs - Gbtc
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
//  11/25/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using DataQualityMonitoring.Services;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Tests measurements to determine whether their values satisfy a range condition.
    /// </summary>
    [Description("Range Test: Notifies when the values of received measurements are outside of a given range")]
    public class RangeTest : ActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default low range for frequency measurements.
        /// </summary>
        public const double DEFAULT_FREQ_LOW_RANGE = 59.95;

        /// <summary>
        /// Default high range for frequency measurements.
        /// </summary>
        public const double DEFAULT_FREQ_HIGH_RANGE = 60.05;

        /// <summary>
        /// Default low range for voltage phasor magnitudes.
        /// </summary>
        public const double DEFAULT_VPHM_LOW_RANGE = 475000.0;

        /// <summary>
        /// Default high range for voltage phasor magnitudes.
        /// </summary>
        public const double DEFAULT_VPHM_HIGH_RANGE = 525000.0;

        /// <summary>
        /// Default low range for current phasor magnitudes.
        /// </summary>
        public const double DEFAULT_IPHM_LOW_RANGE = 0.0;

        /// <summary>
        /// Default high range for current phasor magnitudes.
        /// </summary>
        public const double DEFAULT_IPHM_HIGH_RANGE = 3000.0;

        /// <summary>
        /// Default low range for voltage phasor angles.
        /// </summary>
        public const double DEFAULT_VPHA_LOW_RANGE = -180.0;

        /// <summary>
        /// Default high range for voltage phasor angles.
        /// </summary>
        public const double DEFAULT_VPHA_HIGH_RANGE = 180.0;

        /// <summary>
        /// Default low range for current phasor angles.
        /// </summary>
        public const double DEFAULT_IPHA_LOW_RANGE = -180.0;

        /// <summary>
        /// Default high range for current phasor angles.
        /// </summary>
        public const double DEFAULT_IPHA_HIGH_RANGE = 180.0;

        // Fields
        private readonly Dictionary<MeasurementKey, LinkedList<IMeasurement>> m_outOfRangeMeasurements;
        private string m_signalType;
        private double m_lowRange;
        private double m_highRange;
        private Ticks m_timeToPurge;
        private Ticks m_warnInterval;
        private readonly Timer m_warningTimer;
        private readonly Timer m_purgeTimer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RangeTest"/> class.
        /// </summary>
        public RangeTest()
        {
            m_outOfRangeMeasurements = new Dictionary<MeasurementKey, LinkedList<IMeasurement>>();
            m_timeToPurge = Ticks.FromSeconds(1.0);
            m_warnInterval = Ticks.FromSeconds(4.0);
            m_warningTimer = new Timer();
            m_purgeTimer = new Timer();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the signal type of the measurements sent to this range test.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the signal type of the measurements sent to this range test. Valid values are FREQ, VPHM, IPHM, VPHA, IPHA. IMPORTANT: If this is not defined, LowRange and HighRange are required parameters."),
        DefaultValue(null)]
        public string SignalType
        {
            get
            {
                return m_signalType;
            }
            set
            {
                m_signalType = value;
            }
        }

        /// <summary>
        /// Gets or sets the low range of the measurements sent to this range test.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the low range of the measurements sent to this range test."),
        DefaultValue("")]
        public double LowRange
        {
            get
            {
                return m_lowRange;
            }
            set
            {
                m_lowRange = value;
            }
        }

        /// <summary>
        /// Gets or sets the high range of the measurements sent to this range test.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the high range of the measurements sent to this range test."),
        DefaultValue("")]
        public double HighRange
        {
            get
            {
                return m_highRange;
            }
            set
            {
                m_highRange = value;
            }
        }

        /// <summary>
        /// Gets or sets the age, in seconds, at which out-of-range measurements are purged.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the age, in seconds, at which out-of-range measurements are purged from the adapter."),
        DefaultValue(1.0)]
        public double TimeToPurge
        {
            get
            {
                return m_timeToPurge.ToSeconds();
            }
            set
            {
                m_timeToPurge = Ticks.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets how often the <see cref="RangeTest"/> sends out-of-range notifications to the console.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the amount of time, in seconds, between console updates."),
        DefaultValue(4.0)]
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
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes this <see cref="RangeTest"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            string errorMessage = "{0} is missing from Settings - Example: lowRange=59.95; highRange=60.05";
            bool rangeSet = false;

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue("signalType", out m_signalType))
                rangeSet = TrySetRange(m_signalType);

            if (!rangeSet)
            {
                // Load required parameters
                if (!settings.TryGetValue("lowRange", out setting))
                    throw new ArgumentException(string.Format(errorMessage, "lowRange"));

                m_lowRange = double.Parse(setting);

                if (!settings.TryGetValue("highRange", out setting))
                    throw new ArgumentException(string.Format(errorMessage, "highRange"));

                m_highRange = double.Parse(setting);
            }

            // Load optional parameters
            if (settings.TryGetValue("timeToPurge", out setting))
                m_timeToPurge = Ticks.FromSeconds(double.Parse(setting));

            if (settings.TryGetValue("warnInterval", out setting))
                m_warnInterval = Ticks.FromSeconds(double.Parse(setting));

            m_warningTimer.Interval = m_warnInterval.ToMilliseconds();
            m_warningTimer.Elapsed += m_warningTimer_Elapsed;

            m_purgeTimer.Interval = m_timeToPurge.ToMilliseconds();
            m_purgeTimer.Elapsed += m_purgeTimer_Elapsed;
        }

        /// <summary>
        /// Starts the <see cref="RangeTest"/>.
        /// </summary>
        public override void Start()
        {
            base.Start();

            AttachToService();
            m_warningTimer.Start();
        }

        /// <summary>
        /// Stops the <see cref="RangeTest"/>.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            DetachFromService();
            m_warningTimer.Stop();
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        /// <remarks>
        /// If user implemented publication function consistently exceeds available publishing time (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds),
        /// concentration will fall behind. A small amount of this time is required by the <see cref="ConcentratorBase"/> for processing overhead, so actual total time
        /// available for user function process will always be slightly less than <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds.
        /// </remarks>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IMeasurement measurement;

            foreach (MeasurementKey key in frame.Measurements.Keys)
            {
                measurement = frame.Measurements[key];

                if (measurement.AdjustedValue < m_lowRange || measurement.AdjustedValue > m_highRange)
                    AddOutOfRangeMeasurement(key, measurement);
            }
        }

        /// <summary>
        /// Get a dictionary of <see cref="MeasurementKey"/>,<see cref="System.Int32"/> pairs representing
        /// the number of times each key received has been out-of-range within the last <see cref="TimeToPurge"/> seconds.
        /// </summary>
        /// <returns>A dictionary of count values for each out-of-range measurement.</returns>
        public Dictionary<MeasurementKey, int> GetOutOfRangeCounts()
        {
            Dictionary<MeasurementKey, int> outOfRangeCounts = new Dictionary<MeasurementKey, int>();

            lock (m_outOfRangeMeasurements)
            {
                PurgeOldMeasurements();

                int count;

                foreach (MeasurementKey key in m_outOfRangeMeasurements.Keys)
                {
                    count = m_outOfRangeMeasurements[key].Count;

                    if (count > 0)
                        outOfRangeCounts.Add(key, count);
                }
            }

            return outOfRangeCounts;
        }

        /// <summary>
        /// Get the full collection of out-of-range <see cref="IMeasurement"/>s.
        /// </summary>
        /// <returns>The full collection of out-of-range <see cref="IMeasurement"/>s.</returns>
        public ICollection<IMeasurement> GetOutOfRangeMeasurements()
        {
            ICollection<IMeasurement> allOutOfRangeMeasurements = new LinkedList<IMeasurement>();

            lock (m_outOfRangeMeasurements)
            {
                PurgeOldMeasurements();

                foreach (LinkedList<IMeasurement> measurementList in m_outOfRangeMeasurements.Values)
                {
                    foreach (IMeasurement measurement in measurementList)
                        allOutOfRangeMeasurements.Add(measurement);
                }
            }

            return allOutOfRangeMeasurements;
        }

        /// <summary>
        /// Get a collection of out-of-range <see cref="IMeasurement"/>s with the given key.
        /// </summary>
        /// <param name="key">The <see cref="MeasurementKey"/> corresponding to the desired measurements.</param>
        /// <returns>A collection of out-of-range <see cref="IMeasurement"/>s.</returns>
        public ICollection<IMeasurement> GetOutOfRangeMeasurements(MeasurementKey key)
        {
            lock (m_outOfRangeMeasurements)
            {
                PurgeOldMeasurements(key);
                return new LinkedList<IMeasurement>(m_outOfRangeMeasurements[key]);
            }
        }

        // Purge all old out-of-range measurements.
        private void PurgeOldMeasurements()
        {
            lock (m_outOfRangeMeasurements)
            {
                foreach (MeasurementKey key in m_outOfRangeMeasurements.Keys)
                    PurgeOldMeasurements(key);
            }
        }

        // Purge old, out-of-range measurements with the given key.
        private void PurgeOldMeasurements(MeasurementKey key)
        {
            lock (m_outOfRangeMeasurements)
            {
                LinkedList<IMeasurement> measurements = m_outOfRangeMeasurements[key];
                bool donePurging = false;

                // Purge old measurements to prevent redundant warnings.
                while (measurements.Count > 0 && !donePurging)
                {
                    IMeasurement measurement = measurements.First.Value;
                    Ticks diff = base.RealTime - measurement.Timestamp;

                    if (diff >= base.LagTicks + m_timeToPurge)
                        measurements.RemoveFirst();
                    else
                        donePurging = true;
                }
            }
        }

        // Try to set the testing range based on the given signal type.
        private bool TrySetRange(string signalType)
        {
            switch (signalType.ToUpper())
            {
                case "FREQ":
                    m_lowRange = DEFAULT_FREQ_LOW_RANGE;
                    m_highRange = DEFAULT_FREQ_HIGH_RANGE;
                    break;

                case "VPHM":
                    m_lowRange = DEFAULT_VPHM_LOW_RANGE;
                    m_highRange = DEFAULT_VPHM_HIGH_RANGE;
                    break;

                case "IPHM":
                    m_lowRange = DEFAULT_IPHM_LOW_RANGE;
                    m_highRange = DEFAULT_IPHM_HIGH_RANGE;
                    break;

                case "VPHA":
                    m_lowRange = DEFAULT_VPHA_LOW_RANGE;
                    m_highRange = DEFAULT_VPHA_HIGH_RANGE;
                    break;

                case "IPHA":
                    m_lowRange = DEFAULT_IPHA_LOW_RANGE;
                    m_highRange = DEFAULT_IPHA_HIGH_RANGE;
                    break;

                default:
                    return false;
            }

            return true;
        }

        // Add a measurement to the list of out of range measurements corresponding to the given key.
        private void AddOutOfRangeMeasurement(MeasurementKey key, IMeasurement measurement)
        {
            lock (m_outOfRangeMeasurements)
            {
                LinkedList<IMeasurement> outOfRangeList;

                if (!m_outOfRangeMeasurements.TryGetValue(key, out outOfRangeList))
                {
                    outOfRangeList = new LinkedList<IMeasurement>();
                    m_outOfRangeMeasurements.Add(key, outOfRangeList);
                }

                outOfRangeList.AddLast(measurement);
            }
        }

        private void AttachToService()
        {
            lock (s_service)
            {
                s_service.AttachRangeTest(this);

                if (s_exceptionProcessor == null)
                    s_exceptionProcessor = this;
            }
        }

        private void DetachFromService()
        {
            lock (s_service)
            {
                s_service.DetachRangeTest(this);

                if (this != s_exceptionProcessor)
                    return;

                ICollection<RangeTest> tests = s_service.Tests;
                s_exceptionProcessor = tests.Count > 0 ? tests.GetEnumerator().Current : null;
            }
        }

        // Periodically send warnings to the console about out-of-range measurements.
        private void m_warningTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dictionary<MeasurementKey, int> measurementCounts = GetOutOfRangeCounts();

            foreach (MeasurementKey key in measurementCounts.Keys)
            {
                int count = measurementCounts[key];
                OnStatusMessage(MessageLevel.Info, "RangeTest", $"Measurement {key} arrived out-of-range {count:N0} times within the last {(int)m_timeToPurge.ToSeconds()} seconds.");
            }
        }

        // Periodically purge old measurements to lighten system load and speed up out-of-range measurement retrieval.
        private void m_purgeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PurgeOldMeasurements();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly OutOfRangeService s_service;
        private static RangeTest s_exceptionProcessor;

        // Static Constructor
        static RangeTest()
        {
            s_service = new OutOfRangeService();
            s_service.ServiceProcessException += s_service_ServiceProcessException;
            s_service.Initialize();
        }

        // Static Methods
        private static void s_service_ServiceProcessException(object sender, EventArgs<Exception> e)
        {
            s_exceptionProcessor?.OnProcessException(MessageLevel.Warning, "RangeTest", e.Argument);
        }

        #endregion
    }
}
