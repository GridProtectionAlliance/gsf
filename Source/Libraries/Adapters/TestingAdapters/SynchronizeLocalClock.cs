//******************************************************************************************************
//  SynchronizeLocalClock.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/20/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace TestingAdapters
{
    /// <summary>
    /// Represents a class used to synchronize the local system clock time to the time from input measurements.
    /// </summary>
    [Description("System Time Sync: Synchronizes local system clock time to the time from input measurements")]
    public class SynchronizeLocalClock : FacileActionAdapterBase
    {
        #region [ Members ]

        // Nested Types

        #pragma warning disable 169
        #pragma warning disable 414
        #pragma warning disable 649
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }
        #pragma warning restore 169
        #pragma warning restore 414
        #pragma warning restore 649

        // Constants

        /// <summary>
        /// Default value for the <see cref="UpdateFrequency"/> property.
        /// </summary>
        public const int DefaultUpdateFrequency = 1000;

        /// <summary>
        /// Default value for the <see cref="UpdateTolerance"/> property.
        /// </summary>
        public const double DefaultUpdateTolerance = 0.001D; // One millisecond

        // Fields
        private Timer m_updateTimer;
        private long m_skippedUpdates;
        private long m_failedUpdates;
        private long m_successfulUpdates;
        private long m_timerEvents;
        private long m_updateTolerance;
        private bool m_goodSourceTime;
        private Ticks m_latestTime;
        private bool m_disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the update frequency, in milliseconds, for setting the local system clock time.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultUpdateFrequency)]
        [Description("Defines the update frequency, in milliseconds, for setting the local system clock time.")]
        public int UpdateFrequency { get; set; } = DefaultUpdateFrequency;

        /// <summary>
        /// Gets or sets the minimum update tolerance, in seconds, as deviation to current local time to check before updating the local clock.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultUpdateTolerance)]
        [Description("Defines the minimum update tolerance, in seconds, as deviation to current local time to check before updating the local clock.")]
        public double UpdateTolerance
        {
            get => new Ticks(m_updateTolerance).ToSeconds();
            set => m_updateTolerance = Ticks.FromSeconds(value);
        }

        /// <summary>
        /// Gets or sets flag that determines whether or not to use the local clock time as real time.
        /// </summary>
        /// <remarks>
        /// Use your local system clock as real time only if the time is locally GPS-synchronized,
        /// or if the measurement values being sorted were not measured relative to a GPS-synchronized clock.
        /// Turn this off if the class is intended to process historical data.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)] // Redeclared to hide property - value should not be changed by user
        public override bool UseLocalClockAsRealTime
        {
            get => base.UseLocalClockAsRealTime;
            set => base.UseLocalClockAsRealTime = value;
        }

        /// <summary>
        /// Gest or sets flag that determines whether to fall back on local clock time as real time when time is unreasonable.
        /// </summary>
        /// <remarks>
        /// This property is only applicable when <see cref="FacileActionAdapterBase.UseLocalClockAsRealTime"/> is <c>false</c>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)] // Redeclared to hide property - value should not be changed by user
        public override bool FallBackOnLocalClock
        { 
            get => base.FallBackOnLocalClock; 
            set => base.FallBackOnLocalClock = value;
        }

        /// <summary>
        /// Gets or sets flag to start tracking the absolute latest received measurement values.
        /// </summary>
        /// <remarks>
        /// Latest received measurement value will be available via the <see cref="FacileActionAdapterBase.LatestMeasurements"/> property.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)] // Redeclared to hide property - value should not be changed by user
        public override bool TrackLatestMeasurements
        {
            get => base.TrackLatestMeasurements;
            set => base.TrackLatestMeasurements = value;
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine($"Absolute latest time value: {(m_latestTime.Value > 0L ? $"{m_latestTime:yyyy-MM-dd HH:mm:ss.fff}" : "No time value has been received")}");
                status.AppendLine($"          Update tolerance: {UpdateTolerance:N3} seconds ({TimeSpan.FromSeconds(UpdateTolerance).TotalMilliseconds:N3} milliseconds)");
                status.AppendLine($"     Skipped clock updates: {m_skippedUpdates:N0} were within update tolerance");
                status.AppendLine($"      Failed clock updates: {m_failedUpdates:N0}");
                status.AppendLine($"  Successful clock updates: {m_successfulUpdates:N0}");

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SynchronizeLocalClock"/> object and optionally releases the managed resources.
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

                if (!(m_updateTimer is null))
                {
                    m_updateTimer.Stop();
                    m_updateTimer.Elapsed -= UpdateTimer_Elapsed;
                    m_updateTimer.Dispose();
                }
            }
            finally
            {
                m_disposed = true;       // Prevent duplicate dispose.
                base.Dispose(disposing); // Call base class Dispose().
            }
        }

        /// <summary>
        /// Initializes <see cref="SynchronizeLocalClock"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings = Settings;

            // Force facile adapter to use incoming timestamps as time source, i.e., "RealTime"
            settings[nameof(UseLocalClockAsRealTime)] = false.ToString();
            settings[nameof(FallBackOnLocalClock)] = true.ToString();
            settings[nameof(TrackLatestMeasurements)] = true.ToString();

            base.Initialize();

            if (settings.TryGetValue(nameof(UpdateFrequency), out string setting))
            {
                if (int.TryParse(setting, out int updateFrequency) && updateFrequency > 0)
                    UpdateFrequency = updateFrequency;
            }

            if (settings.TryGetValue(nameof(UpdateTolerance), out setting) && double.TryParse(setting, out double tolerance))
                UpdateTolerance = tolerance;
            else
                UpdateTolerance = DefaultUpdateTolerance;

            m_updateTimer = new Timer(UpdateFrequency);
            m_updateTimer.Elapsed += UpdateTimer_Elapsed;
            m_updateTimer.Start();
        }

        /// <summary>
        /// Stops the <see cref="SynchronizeLocalClock"/>.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            m_updateTimer.Stop();
            
            OnStatusMessage(MessageLevel.Info, "Clock synchronizations paused...");
        }

        /// <summary>
        /// Starts the <see cref="SynchronizeLocalClock"/>.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_updateTimer.Start();

            OnStatusMessage(MessageLevel.Info, "Clock synchronizations resumed...");
        }

        /// <summary>
        /// Manually synchronizes local to specified date/time.
        /// </summary>
        [AdapterCommand("Manually synchronizes local to specified date/time.", "Administrator")]
        public void ManualSync(string dateTime)
        {
            if (!DateTime.TryParse(dateTime, out DateTime targetTime))
            {
                OnStatusMessage(MessageLevel.Warning, $"Failed to parse \"{dateTime}\" as a valid DateTime value.");
                return;
            }

            try
            {
                SetSystemTime(targetTime);
                m_successfulUpdates++;
                OnStatusMessage(MessageLevel.Info, $"Manually updated local clock to {targetTime:yyyy-MM-dd HH:mm:ss.fff}");
            }
            catch (Exception ex)
            {
                m_failedUpdates++;
                OnStatusMessage(MessageLevel.Error, $"Failed to manually update local clock to {targetTime:yyyy-MM-dd HH:mm:ss.fff}: {ex.Message}");
            }
        }

        /// <summary>
        /// Forces local clock synchronization to latest time value without reasonability considerations.
        /// </summary>
        [AdapterCommand("Forces local clock synchronization to absolute latest received time value without reasonability considerations.", "Administrator")]
        public void ForceSync()
        {
            Ticks targetTime = m_latestTime;

            if (targetTime.Value > 0L)
            {
                try
                {
                    SetSystemTime(targetTime);
                    m_successfulUpdates++;
                    OnStatusMessage(MessageLevel.Info, $"Forced local clock to {targetTime:yyyy-MM-dd HH:mm:ss.fff}");
                }
                catch (Exception ex)
                {
                    m_failedUpdates++;
                    OnStatusMessage(MessageLevel.Error, $"Failed to force local clock to {targetTime:yyyy-MM-dd HH:mm:ss.fff}: {ex.Message}");
                }
            }
            else
            {
                int count = InputMeasurementKeys.Length;

                if (count > 0)
                    OnStatusMessage(MessageLevel.Warning, $"Cannot force clock update - no time has been received from any of the {count:N0} defined input sources.");
                else
                    OnStatusMessage(MessageLevel.Warning, "Cannot force clock update - no input sources have been defined.");
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength) => $"Updated clock {m_successfulUpdates:N0} times out of {m_timerEvents:N0} checks so far...".CenterText(maxLength);

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            List<IMeasurement> measurementsWithGoodTime = new List<IMeasurement>();

            foreach (IMeasurement measurement in measurements)
            {
                if (measurement.TimestampQualityIsGood())
                    measurementsWithGoodTime.Add(measurement);

                // Track absolute latest time, regardless of resonability or quality
                if (measurement.Timestamp.Value > m_latestTime.Value)
                    m_latestTime = measurement.Timestamp;
            }

            if (measurementsWithGoodTime.Count == 0)
            {
                base.QueueMeasurementsForProcessing(measurements);
                m_goodSourceTime = false;
            }
            else
            {
                base.QueueMeasurementsForProcessing(measurementsWithGoodTime);
                m_goodSourceTime = true;
            }
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                m_timerEvents++;
                
                Ticks newSystemTime = RealTime;

                if (newSystemTime <= 0L)
                    return;

                if (Math.Abs(newSystemTime.Value - DateTime.UtcNow.Ticks) < m_updateTolerance)
                {
                    m_skippedUpdates++;
                    return;
                }

                SetSystemTime(RealTime);
                m_successfulUpdates++;

                if (!m_goodSourceTime)
                    OnStatusMessage(MessageLevel.Warning, "WARNING: Clock set with measurement that has bad quality -- increase measurement sources.");
            }
            catch (Exception ex)
            {
                m_failedUpdates++;
                OnProcessException(MessageLevel.Error, ex);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME lpSystemTime);

        private static void SetSystemTime(DateTime newSystemTime)
        {
            SYSTEMTIME systime = new SYSTEMTIME
            {
                wYear = (ushort)newSystemTime.Year,
                wMonth = (ushort)newSystemTime.Month,
                wDay = (ushort)newSystemTime.Day,
                wHour = (ushort)newSystemTime.Hour,
                wMinute = (ushort)newSystemTime.Minute,
                wSecond = (ushort)newSystemTime.Second,
                wMilliseconds = (ushort)newSystemTime.Millisecond
            };

            if (!SetSystemTime(ref systime))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to set local system time, verify service account has SE_SYSTEMTIME_NAME privilege.");
        }

        #endregion
    }
}