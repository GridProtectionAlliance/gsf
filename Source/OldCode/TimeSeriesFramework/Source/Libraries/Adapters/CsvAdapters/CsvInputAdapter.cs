//******************************************************************************************************
//  CsvInputAdapter.cs - Gbtc
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
//  04/06/2010 - Stephen C. Wills
//       Generated original version of source code.
//  07/03/2012 - J. Ritchie Carroll
//       Added high-resolution input timer, auto-repeat and transverse operational mode.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a CSV file.
    /// </summary>
    [Description("CSV: reads measurements from a CSV file.")]
    public class CsvInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Nested Types

        // TODO: Move this into its own public class for use by MPFP and CSV input timer...
        /// <summary>
        /// Precision input timer.
        /// </summary>
        /// <remarks>
        /// This class is used to create highly accurate simulated data inputs aligned to the local clock.<br/>
        /// One static instance of this internal class is created per encountered frame rate.
        /// </remarks>
        private sealed class PrecisionInputTimer : IDisposable
        {
            #region [ Members ]

            // Fields
            private PrecisionTimer m_timer;
            private bool m_useWaitHandleA;
            private SpinLock m_timerTickLock;
            private ManualResetEventSlim m_frameWaitHandleA;
            private ManualResetEventSlim m_frameWaitHandleB;
            private int m_framesPerSecond;
            private int m_frameWindowSize;
            private int[] m_frameMilliseconds;
            private int m_lastFrameIndex;
            private long m_lastFrameTime;
            private long m_missedPublicationWindows;
            private long m_lastMissedWindowTime;
            private long m_resynchronizations;
            private int m_referenceCount;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Create a new <see cref="PrecisionInputTimer"/> class.
            /// </summary>
            /// <param name="framesPerSecond">Desired frame rate for <see cref="PrecisionTimer"/>.</param>
            public PrecisionInputTimer(int framesPerSecond)
            {
                // Create synchronization objects
                m_timerTickLock = new SpinLock();
                m_frameWaitHandleA = new ManualResetEventSlim(false);
                m_frameWaitHandleB = new ManualResetEventSlim(false);
                m_useWaitHandleA = true;
                m_framesPerSecond = framesPerSecond;

                // Create a new precision timer for this timer state
                m_timer = new PrecisionTimer();
                m_timer.Resolution = 1;
                m_timer.Period = 1;
                m_timer.AutoReset = true;

                // Attach handler for timer ticks
                m_timer.Tick += m_timer_Tick;

                m_frameWindowSize = (int)Math.Round(1000.0D / framesPerSecond) * 2;
                m_frameMilliseconds = new int[framesPerSecond];

                for (int frameIndex = 0; frameIndex < framesPerSecond; frameIndex++)
                {
                    m_frameMilliseconds[frameIndex] = (int)(1.0D / framesPerSecond * (frameIndex * 1000.0D));
                }

                // Start high resolution timer on a separate thread so the start
                // time can synchronized to the top of the millisecond
                ThreadPool.QueueUserWorkItem(SynchronizeInputTimer);
            }

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="PrecisionInputTimer"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~PrecisionInputTimer()
            {
                Dispose(false);
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets frames per second for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public int FramesPerSecond
            {
                get
                {
                    return m_framesPerSecond;
                }
            }

            /// <summary>
            /// Gets array of frame millisecond times for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public int[] FrameMilliseconds
            {
                get
                {
                    return m_frameMilliseconds;
                }
            }

            /// <summary>
            /// Gets reference count for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public int ReferenceCount
            {
                get
                {
                    return m_referenceCount;
                }
            }

            /// <summary>
            /// Gets number of resynchronizations that have occurred for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public long Resynchronizations
            {
                get
                {
                    return m_resynchronizations;
                }
            }

            /// <summary>
            /// Gets time of last frame, in ticks.
            /// </summary>
            public long LastFrameTime
            {
                get
                {
                    return m_lastFrameTime;
                }
            }

            /// <summary>
            /// Gets a reference to the frame wait handle.
            /// </summary>
            public ManualResetEventSlim FrameWaitHandle
            {
                get
                {
                    if (m_useWaitHandleA)
                        return m_frameWaitHandleA;

                    return m_frameWaitHandleB;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases all the resources used by the <see cref="PrecisionInputTimer"/> object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="PrecisionInputTimer"/> object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            private void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if (m_timer != null)
                            {
                                m_timer.Tick -= m_timer_Tick;
                                m_timer.Dispose();
                            }
                            m_timer = null;

                            if (m_frameWaitHandleA != null)
                            {
                                m_frameWaitHandleA.Set();
                                m_frameWaitHandleA.Dispose();
                            }
                            m_frameWaitHandleA = null;

                            if (m_frameWaitHandleB != null)
                            {
                                m_frameWaitHandleB.Set();
                                m_frameWaitHandleB.Dispose();
                            }
                            m_frameWaitHandleB = null;
                        }
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.
                    }
                }
            }

            /// <summary>
            /// Adds a reference to this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public void AddReference()
            {
                m_referenceCount++;
            }

            /// <summary>
            /// Removes a reference to this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public void RemoveReference()
            {
                m_referenceCount--;
            }

            // This timer function is called every millisecond so that frames can be published at the exact desired time 
            private void m_timer_Tick(object sender, EventArgs e)
            {
                // Slower systems or systems under stress may have trouble keeping up with a 1-ms timer, so
                // we only process this code if it's not already processing...
                bool locked = false;

                try
                {
                    m_timerTickLock.TryEnter(2, ref locked);

                    if (locked)
                    {
                        DateTime now = PrecisionTimer.UtcNow;
                        int frameMilliseconds, milliseconds = now.Millisecond;
                        long ticks = now.Ticks;
                        bool releaseTimer = false, resync = false;

                        // Make sure current time is reasonably close to current frame index
                        if (Math.Abs(milliseconds - m_frameMilliseconds[m_lastFrameIndex]) > m_frameWindowSize)
                            m_lastFrameIndex = 0;

                        // See if it is time to publish
                        for (int frameIndex = m_lastFrameIndex; frameIndex < m_frameMilliseconds.Length; frameIndex++)
                        {
                            frameMilliseconds = m_frameMilliseconds[frameIndex];

                            if (frameMilliseconds == milliseconds)
                            {
                                // See if system skipped a publication window
                                if (m_lastFrameIndex != frameIndex)
                                {
                                    // We monitor for missed windows in quick succession (within 1.5 seconds)
                                    if (ticks - m_lastMissedWindowTime > 15000000L)
                                    {
                                        // Threshold has passed since last missed window, so we reset counters
                                        m_lastMissedWindowTime = ticks;
                                        m_missedPublicationWindows = 0;
                                    }

                                    m_missedPublicationWindows++;

                                    // If the system is starting to skip publications it could need resynchronization,
                                    // so in this case we restart the high-resolution timer to get the timer started
                                    // closer to the top of the millisecond
                                    resync = (m_missedPublicationWindows > 4);
                                }

                                // Prepare index for next check, time moving forward
                                m_lastFrameIndex = frameIndex + 1;

                                if (m_lastFrameIndex >= m_frameMilliseconds.Length)
                                    m_lastFrameIndex = 0;

                                if (resync)
                                {
                                    if (m_timer != null)
                                    {
                                        m_timer.Stop();
                                        ThreadPool.QueueUserWorkItem(SynchronizeInputTimer);
                                        m_resynchronizations++;
                                    }
                                }

                                releaseTimer = true;
                                break;
                            }
                            else if (frameMilliseconds > milliseconds)
                            {
                                // Time has yet to pass, wait till the next tick
                                break;
                            }
                        }

                        if (releaseTimer)
                        {
                            // Baseline timestamp to the top of the millisecond for frame publication
                            m_lastFrameTime = ticks - ticks % Ticks.PerMillisecond;

                            // Pulse all waiting threads toggling between ready handles
                            if (m_useWaitHandleA)
                            {
                                m_frameWaitHandleB.Reset();
                                m_useWaitHandleA = false;
                                m_frameWaitHandleA.Set();
                            }
                            else
                            {
                                m_frameWaitHandleA.Reset();
                                m_useWaitHandleA = true;
                                m_frameWaitHandleB.Set();
                            }
                        }
                    }
                }
                finally
                {
                    if (locked)
                        m_timerTickLock.Exit(true);
                }
            }

            private void SynchronizeInputTimer(object state)
            {
                // Start timer at as close to the top of the millisecond as possible 
                bool repeat = true;
                long last = 0, next;

                while (repeat)
                {
                    next = PrecisionTimer.UtcNow.Ticks % Ticks.PerMillisecond % 1000;
                    repeat = (next > last);
                    last = next;
                }

                m_lastMissedWindowTime = 0;
                m_missedPublicationWindows = 0;

                if (m_timer != null)
                    m_timer.Start();
            }

            #endregion
        }

        // Fields
        private string m_fileName;
        private StreamReader m_inStream;
        private string m_header;
        private Dictionary<string, int> m_columns;
        private Dictionary<int, IMeasurement> m_columnMappings;
        private double m_inputInterval;
        private int m_measurementsPerInterval;
        private bool m_simulateTimestamp;
        private bool m_transverse;
        private bool m_autoRepeat;
        private System.Timers.Timer m_looseTimer;
        private PrecisionInputTimer m_precisionTimer;
        private bool m_attachedToInputTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvInputAdapter"/> class.
        /// </summary>
        public CsvInputAdapter()
        {
            m_fileName = "measurements.csv";
            m_columns = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            m_columnMappings = new Dictionary<int, IMeasurement>();
            m_inputInterval = 33.333333;
            m_measurementsPerInterval = 5;

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the CSV file from which measurements will be read.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the CSV file from which measurements will be read."),
        DefaultValue("measurements.csv")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval of time between sending frames into the concentrator.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval of time, in milliseconds, between sending frames into the concentrator."),
        DefaultValue(33.333333)]
        public double InputInterval
        {
            get
            {
                return m_inputInterval;
            }
            set
            {
                m_inputInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets value that determines if the CSV input file data should be replayed repeatedly.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define if the CSV input file data should be replayed repeatedly."),
        DefaultValue(false)]
        public bool AutoRepeat
        {
            get
            {
                return m_autoRepeat;
            }
            set
            {
                m_autoRepeat = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if a high-resolution precision timer should be used for CSV file based input.
        /// </summary>
        /// <remarks>
        /// Useful when input frames need be accurately time-aligned to the local clock to better simulate
        /// an input device and calculate downstream latencies.<br/>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        [ConnectionStringParameter,
        Description("Determines if a high-resolution precision timer should be used for CSV file based input."),
        DefaultValue(false)]
        public bool UseHighResolutionInputTimer
        {
            get
            {
                return (m_precisionTimer != null);
            }
            set
            {
                // Note that a 1-ms timer and debug mode don't mix, so the high-resolution timer is disabled while debugging
                if (value && m_precisionTimer == null && !System.Diagnostics.Debugger.IsAttached)
                    m_precisionTimer = AttachToInputTimer((int)(1000.0D / m_inputInterval));
                else if (!value && m_precisionTimer != null)
                    DetachFromInputTimer(ref m_precisionTimer);
            }
        }

        /// <summary>
        /// Gets or sets the number of measurements that are read from the CSV file in each frame.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of measurements measurements that are read from the CSV file in each frame."),
        DefaultValue(5)]
        public int MeasurementsPerInterval
        {
            get
            {
                return m_measurementsPerInterval;
            }
            set
            {
                m_measurementsPerInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether CSV file is in transverse mode for real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether CSV file is in transverse mode for real-time concentration."),
        DefaultValue(false)]
        public bool TransverseMode
        {
            get
            {
                return m_transverse;
            }
            set
            {
                m_transverse = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether timestamps are
        /// simulated for the purposes of real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether timestamps are simulated for real-time concentration."),
        DefaultValue(false)]
        public bool SimulateTimestamp
        {
            get
            {
                return m_simulateTimestamp;
            }
            set
            {
                m_simulateTimestamp = value;
            }
        }

        /// <summary>
        /// Defines the column mappings must defined: e.g., 0=Timestamp; 1=PPA:12; 2=PPA13.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the column mappings must defined: e.g., \"0=Timestamp; 1=PPA:12; 2=PPA13\"."),
        DefaultValue("")]
        public int ColumnMappings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a flag that determines if this <see cref="CsvInputAdapter"/>
        /// uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the detailed status of this <see cref="CsvInputAdapter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("                 File name: {0}", m_fileName);
                status.AppendLine();
                status.AppendFormat("               File header: {0}", m_header);
                status.AppendLine();
                status.AppendFormat("            Input interval: {0}", m_inputInterval);
                status.AppendLine();
                status.AppendFormat(" Measurements per interval: {0}", m_measurementsPerInterval);
                status.AppendLine();
                status.AppendFormat("     Using traverse format: {0}", m_transverse);
                status.AppendLine();
                status.AppendFormat("               Auto-repeat: {0}", m_autoRepeat);
                status.AppendLine();
                status.AppendFormat("     Precision input timer: {0}", UseHighResolutionInputTimer ? "Enabled" : "Offline");
                status.AppendLine();
                if (m_precisionTimer != null)
                {
                    status.AppendFormat("  Timer resynchronizations: {0}", m_precisionTimer.Resynchronizations);
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CsvInputAdapter"/> object and optionally releases the managed resources.
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
                        if (UseHighResolutionInputTimer)
                        {
                            DetachFromInputTimer(ref m_precisionTimer);
                        }
                        else if ((object)m_looseTimer != null)
                        {
                            m_looseTimer.Stop();
                            m_looseTimer.Dispose();
                        }
                        m_looseTimer = null;

                        if ((object)m_inStream != null)
                        {
                            m_inStream.Close();
                            m_inStream.Dispose();
                        }

                        m_inStream = null;

                        // Clear minimum timer resolution.
                        PrecisionTimer.ClearMinimumTimerResolution(1);
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
        /// Initializes this <see cref="CsvInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters

            if (settings.TryGetValue("fileName", out setting))
                m_fileName = setting;

            if (settings.TryGetValue("inputInterval", out setting))
                m_inputInterval = double.Parse(setting);

            if (settings.TryGetValue("measurementsPerInterval", out setting))
                m_measurementsPerInterval = int.Parse(setting);

            if (settings.TryGetValue("simulateTimestamp", out setting))
                m_simulateTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("transverse", out setting) || settings.TryGetValue("transverseMode", out setting))
                m_transverse = setting.ParseBoolean();

            if (settings.TryGetValue("autoRepeat", out setting))
                m_autoRepeat = setting.ParseBoolean();

            settings.TryGetValue("useHighResolutionInputTimer", out setting);

            if (string.IsNullOrEmpty(setting))
                setting = "false";

            UseHighResolutionInputTimer = setting.ParseBoolean();

            if (!UseHighResolutionInputTimer)
                m_looseTimer = new System.Timers.Timer();

            if (m_transverse)
            {
                // Load column mappings:
                if (settings.TryGetValue("columnMappings", out setting))
                {
                    Dictionary<int, string> columnMappings = new Dictionary<int, string>();
                    int index;

                    foreach (KeyValuePair<string, string> mapping in setting.ParseKeyValuePairs())
                    {
                        if (int.TryParse(mapping.Key, out index))
                            columnMappings[index] = mapping.Value;
                    }

                    if (!m_simulateTimestamp && !columnMappings.Values.Contains("Timestamp", StringComparer.InvariantCultureIgnoreCase))
                        throw new InvalidOperationException("One of the column mappings must be defined as a \"Timestamp\": e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");

                    // In transverse mode, maximum measurements per interval is set to maximum columns in input file
                    m_measurementsPerInterval = columnMappings.Keys.Max() + 1;

                    // Auto-assign output measurements based on column mappings
                    OutputMeasurements = columnMappings.Where(kvp => string.Compare(kvp.Value, "Timestamp", true) != 0).Select(kvp =>
                    {
                        string measurementID = kvp.Value;
                        IMeasurement measurement = new Measurement();
                        MeasurementKey key;
                        Guid id;

                        if (Guid.TryParse(measurementID, out id))
                        {
                            measurement.ID = id;
                            measurement.Key = MeasurementKey.LookupBySignalID(id);
                        }
                        else if (MeasurementKey.TryParse(measurementID, Guid.Empty, out key))
                        {
                            measurement.Key = key;
                            measurement.ID = key.SignalID;
                        }

                        try
                        {
                            DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalID = '{0}'", measurement.ID));

                            if (filteredRows.Length > 0)
                            {
                                DataRow row = filteredRows[0];

                                // Assign other attributes
                                measurement.TagName = row["PointTag"].ToNonNullString();
                                measurement.Multiplier = double.Parse(row["Multiplier"].ToString());
                                measurement.Adder = double.Parse(row["Adder"].ToString());
                            }
                        }
                        catch
                        {
                            // Failure to lookup extra metadata is not catastrophic
                        }

                        // Associate measurement with column index
                        m_columnMappings[kvp.Key] = measurement;

                        return measurement;
                    }).ToArray();

                    int timestampColumn = columnMappings.First(kvp => string.Compare(kvp.Value, "Timestamp", true) == 0).Key;

                    // Reserve a column mapping for timestamp value
                    IMeasurement timestampMeasurement = new Measurement()
                    {
                        TagName = "Timestamp"
                    };

                    m_columnMappings[timestampColumn] = timestampMeasurement;
                }
                else
                {
                    throw new InvalidOperationException("Column mappings must be defined when using transverse format: e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");
                }
            }

            // Override input interval based on temporal processing interval if it's not set to default
            if (ProcessingInterval > -1)
            {
                if (ProcessingInterval == 0)
                    m_inputInterval = 1;
                else
                    m_inputInterval = ProcessingInterval;
            }

            if ((object)m_looseTimer != null)
            {
                m_looseTimer.Interval = m_inputInterval;
                m_looseTimer.AutoReset = true;
                m_looseTimer.Elapsed += m_looseTimer_Elapsed;
            }
        }

        /// <summary>
        /// Attempts to connect to this <see cref="CsvInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_inStream = new StreamReader(m_fileName);

            m_header = m_inStream.ReadLine();

            string[] headings = m_header.ToNonNullString().Split(',');

            for (int i = 0; i < headings.Length; i++)
            {
                m_columns.Add(headings[i], i);
            }

            if (UseHighResolutionInputTimer)
            {
                // Start a new thread to process measurements using precision timer
                (new Thread(ProcessMeasurements)).Start();
            }
            else
            {
                // Start common timer
                m_looseTimer.Start();
            }
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="CsvInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_inStream != null)
            {
                m_inStream.Close();
                m_inStream.Dispose();
            }

            m_inStream = null;

            if (!UseHighResolutionInputTimer)
                m_looseTimer.Stop();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="CsvInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("{0} measurements read from CSV file.", ProcessedMeasurements).CenterText(maxLength);
        }

        private void ProcessMeasurements()
        {
            while (Enabled && ReadNextRecord(m_precisionTimer.LastFrameTime))
            {
                // When high resolution input timing is requested, we only need to wait for the next signal...
                m_precisionTimer.FrameWaitHandle.Wait();
            }

            if (Enabled)
            {
                Stop();

                if (m_autoRepeat)
                    Start();
            }
        }

        private void m_looseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!ReadNextRecord(DateTime.UtcNow.Ticks) && Enabled)
            {
                Stop();

                if (m_autoRepeat)
                    Start();
            }
        }

        // Attempt to read the next record
        private bool ReadNextRecord(long currentTime)
        {
            try
            {
                List<IMeasurement> newMeasurements = new List<IMeasurement>();
                long fileTime = 0;
                int timestampColumn = 0;
                string[] fields = m_inStream.ReadLine().ToNonNullString().Split(',');

                if (fields.Length <= 0)
                    return false;

                // Read time from Timestamp column in transverse mode
                if (m_transverse)
                {
                    if (m_simulateTimestamp)
                    {
                        fileTime = currentTime;
                    }
                    else
                    {
                        timestampColumn = m_columnMappings.First(kvp => string.Compare(kvp.Value.TagName, "Timestamp", true) == 0).Key;
                        fileTime = long.Parse(fields[timestampColumn]);
                    }
                }

                for (int i = 0; i < m_measurementsPerInterval; i++)
                {
                    IMeasurement measurement;

                    if (m_transverse)
                    {
                        // No measurement will be defined for timestamp column
                        if (i == timestampColumn)
                            continue;

                        if (m_columnMappings.TryGetValue(i, out measurement))
                        {
                            measurement = Measurement.Clone(measurement);
                            measurement.Value = double.Parse(fields[i]);
                        }
                        else
                        {
                            measurement = new Measurement();
                            measurement.ID = Guid.Empty;
                            measurement.Key = MeasurementKey.Undefined;
                            measurement.Value = double.NaN;
                        }

                        if (m_simulateTimestamp)
                            measurement.Timestamp = currentTime;
                        else if (m_columns.ContainsKey("Timestamp"))
                            measurement.Timestamp = fileTime;
                    }
                    else
                    {
                        measurement = new Measurement();

                        if (m_columns.ContainsKey("Signal ID"))
                            measurement.ID = new Guid(fields[m_columns["Signal ID"]]);

                        if (m_columns.ContainsKey("Measurement Key"))
                            measurement.Key = MeasurementKey.Parse(fields[m_columns["Measurement Key"]], measurement.ID);

                        if (m_simulateTimestamp)
                            measurement.Timestamp = currentTime;
                        else if (m_columns.ContainsKey("Timestamp"))
                            measurement.Timestamp = long.Parse(fields[m_columns["Timestamp"]]);

                        if (m_columns.ContainsKey("Value"))
                            measurement.Value = double.Parse(fields[m_columns["Value"]]);
                    }

                    newMeasurements.Add(measurement);
                }

                OnNewMeasurements(newMeasurements);
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }

            return true;
        }

        // Handle attach to input timer
        private PrecisionInputTimer AttachToInputTimer(int framesPerSecond)
        {
            PrecisionInputTimer timer;

            lock (s_inputTimers)
            {
                // Get static input timer for given frames per second creating it if needed
                if (!s_inputTimers.TryGetValue(framesPerSecond, out timer))
                {
                    // Create a new precision input timer
                    timer = new PrecisionInputTimer(framesPerSecond);

                    // Add timer state for given rate to static collection
                    s_inputTimers.Add(framesPerSecond, timer);
                }

                // Increment reference count for input timer at given frame rate
                timer.AddReference();
                m_attachedToInputTimer = true;
            }

            return timer;
        }

        // Handle detach from input timer
        private void DetachFromInputTimer(ref PrecisionInputTimer timer)
        {
            if (timer != null)
            {
                lock (s_inputTimers)
                {
                    if (m_attachedToInputTimer)
                    {
                        // Verify static frame rate timer for given frames per second exists
                        if (s_inputTimers.ContainsKey(timer.FramesPerSecond))
                        {
                            // Decrement reference count
                            timer.RemoveReference();
                            m_attachedToInputTimer = false;

                            // If timer is no longer being referenced we stop it and remove it from static collection
                            if (timer.ReferenceCount == 0)
                            {
                                timer.Dispose();
                                s_inputTimers.Remove(timer.FramesPerSecond);
                            }
                        }
                    }
                }
            }

            timer = null;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static Dictionary<int, PrecisionInputTimer> s_inputTimers = new Dictionary<int, PrecisionInputTimer>();

        #endregion
    }
}
