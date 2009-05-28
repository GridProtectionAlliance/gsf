//*******************************************************************************************************
//  ConcentratorBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated initial version of source for Super Phasor Data Concentrator.
//  02/23/2006 - J. Ritchie Carroll
//       Abstracted classes for general use, and added to PCS code library.
//  04/23/2007 - J. Ritchie Carroll
//       Migrated concentrator to use a base class model instead of using delegates.
//  08/01/2007 - J. Ritchie Carroll
//       Completed extensive threading optimizations to ensure performance.
//  08/27/2007 - Darrell Zuercher
//       Edited code comments.
//  11/02/2007 - J. Ritchie Carroll
//       Changed code to use new FrameQueue class instead of KeyedProcessQueue to
//       allow more finite control of locking to reduce thread contention.
//  02/19/2008 - J. Ritchie Carroll
//       Added code to detect and avoid redundant calls to Dispose().
//  08/22/2008 - J. Ritchie Carroll
//       Replaced timing code using PrecisionTimer
//  09/16/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

// Define this constant to enable use of high-resolution time, undefine to use system time function
#define UseHighResolutionTime

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Units;

namespace PCS.Measurements
{
    /// <summary>
    /// Measurement concentrator base class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class synchronizes (i.e., sorts by timestamp) real-time measurements.
    /// </para>
    /// <para>
    /// Note that your lag time should be defined as it relates to the rate at which data data is coming
    /// into the concentrator. Make sure you allow enough time for transmission of data over the network
    /// allowing any needed time for possible network congestion.  Lead time should be defined as your
    /// confidence in the accuracy of your local clock (e.g., if you set lead time to 2, this means you
    /// trust that your local clock is within plus or minus 2 seconds of real-time.)
    /// </para>
    /// </remarks>
    [CLSCompliant(false)]
    public abstract class ConcentratorBase : IDisposable
    {        
        #region [ Members ]

        // Events

        /// <summary>
        /// This event is raised every second allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        /// <summary>
        /// This event is raised if there is an exception encountered while attempting to process a frame in the sample queue.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing the data stream.
        /// </para>
        /// <para>
        /// Processing will not stop for any exceptions thrown by the user function, but any captured exceptions will be exposed through this event.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Raised, for the benefit of dependent classes, when lead time is updated.
        /// </summary> 
        internal event Action<double> LeadTimeUpdated;

        /// <summary>
        /// Raised, for the benefit of dependent classes, when lag time is updated.
        /// </summary> 
        internal event Action<double> LagTimeUpdated;

        // Fields
        private FrameQueue m_frameQueue;                    // Queue of frames to be published
        private PrecisionTimer m_publicationTimer;          // High precision timer used for frame processing
        private System.Timers.Timer m_monitorTimer;         // Sample monitor - tracks total number of unpublished frames
        private int m_framesPerSecond;                      // Frames per second
        private decimal m_ticksPerFrame;                    // Frame rate - we use a 64-bit scaled integer to avoid round-off errors in calculations
        private int[] m_framePeriods;                       // Evenly distributed waiting times, in whole milliseconds, per frame
        private int m_lastFramePeriod;                      // Tracks last frame period
        private int m_frameIndex;                           // Determines current frame index
        private double m_lagTime;                           // Allowed past time deviation tolerance, in seconds
        private double m_leadTime;                          // Allowed future time deviation tolerance, in seconds
        private Ticks m_lagTicks;                           // Current lag time calculated in ticks
        private bool m_enabled;                             // Enabled state of concentrator
        private long m_startTime;                           // Start time of concentrator
        private long m_stopTime;                            // Stop time of concentrator
        private long m_realTimeTicks;                       // Timstamp of real-time or the most recently received measurement
        private bool m_allowSortsByArrival;                 // Determines whether or not to sort incoming measurements with a bad timestamp by arrival
        private bool m_useLocalClockAsRealTime;             // Determines whether or not to use local system clock as "real-time"
        private long m_totalMeasurements;                   // Total number of measurements ever requested for sorting
        private long m_measurementsSortedByArrival;         // Total number of measurements that were sorted by arrival
        private long m_discardedMeasurements;               // Total number of discarded measurements
        private long m_publishedMeasurements;               // Total number of published measurements
        private long m_missedSortsByTimeout;                // Total number of unsorted measurements due to timeout waiting for lock
        private long m_publishedFrames;                     // Total number of published frames
        private Ticks m_totalPublishTime;                   // Total cumulative frame user function publication time (in ticks) - used to calculate average
        private bool m_trackLatestMeasurements;             // Determines whether or not to track latest measurements
        private ImmediateMeasurements m_latestMeasurements; // Absolute latest received measurement values
        private IMeasurement m_lastDiscardedMeasurement;    // Last measurement that was discarded by the concentrator
        private bool m_disposed;                            // Disposed flag detects redundant calls to dispose method

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConcentratorBase"/>.
        /// </summary>
        /// <param name="framesPerSecond">Number of frames to publish per second.</param>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="framesPerSecond"/> must be at least one.
        /// </para>
        /// <para>
        /// <paramref name="lagTime"/> must be greater than zero, but can be specified in sub-second intervals (e.g., set to .25 for a quarter-second lag time).
        /// Note that this defines time sensitivity to past timestamps.
        /// </para>
        /// <para>
        /// <paramref name="leadTime"/> must be greater than zero, but can be specified in sub-second intervals (e.g., set to .5 for a half-second lead time).
        /// Note that this defines time sensitivity to future timestamps.
        /// </para>
        /// <para>
        /// Concentration will not begin until consumer "Starts" concentrator (i.e., calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Specified argument is outside of allowed value range (see remarks).</exception>
        protected ConcentratorBase(int framesPerSecond, double lagTime, double leadTime)
        {
            if (framesPerSecond < 1)
                throw new ArgumentOutOfRangeException("framesPerSecond", "framesPerSecond must be at least one");

            if (lagTime <= 0)
                throw new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one");

            if (leadTime <= 0)
                throw new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one");

            this.FramesPerSecond = framesPerSecond;
#if UseHighResolutionTime
            m_realTimeTicks = PrecisionTimer.UtcNow.Ticks;
#else
            m_realTime = DateTime.UtcNow.Ticks;
#endif
            m_allowSortsByArrival = true;
            m_lagTime = lagTime;
            m_leadTime = leadTime;
            m_lagTicks = (long)(m_lagTime * Ticks.PerSecond);
            m_latestMeasurements = new ImmediateMeasurements(this);

            // Create a new queue for managing real-time frames
            m_frameQueue = new FrameQueue(this);

            // Create high precision timer used for frame processing
            m_publicationTimer = new PrecisionTimer();
            m_publicationTimer.AutoReset = true;
            m_publicationTimer.Tick += PublishFrames;

            // This timer monitors the total number of unpublished samples every second. This is a useful statistic
            // to monitor: if total number of unpublished samples exceed lag time, measurement concentration could
            // be falling behind.
            m_monitorTimer = new System.Timers.Timer();
            m_monitorTimer.Interval = 1000;
            m_monitorTimer.AutoReset = true;
            m_monitorTimer.Elapsed += MonitorUnpublishedSamples;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ConcentratorBase"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ConcentratorBase()
        {
            // We implement finalizer for this class to ensure sample queue shuts down in an orderly fashion.
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        public double LagTime
        {
            get
            {
                return m_lagTime;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "LagTime must be greater than zero, but it can be less than one");

                m_lagTime = value;
                m_lagTicks = (long)(m_lagTime * Ticks.PerSecond);

                if (LagTimeUpdated != null)
                    LagTimeUpdated(m_lagTime);
            }
        }

        /// <summary>
        /// Gets defined past time deviation tolerance, in ticks.
        /// </summary>
        public Ticks LagTicks
        {
            get
            {
                return m_lagTicks;
            }
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        public double LeadTime
        {
            get
            {
                return m_leadTime;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "LeadTime must be greater than zero, but it can be less than one");

                m_leadTime = value;

                if (LeadTimeUpdated != null)
                    LeadTimeUpdated(m_leadTime);
            }
        }

        /// <summary>
        /// Gets or sets flag to start tracking the absolute latest received measurement values.
        /// </summary>
        /// <remarks>
        /// Enabling this option will slightly increase the required sorting time.
        /// </remarks>
        public bool TrackLatestMeasurements
        {
            get
            {
                return m_trackLatestMeasurements;
            }
            set
            {
                m_trackLatestMeasurements = value;
            }
        }

        /// <summary>
        /// Gets reference to the collection of absolute latest received measurement values.
        /// </summary>
        public ImmediateMeasurements LatestMeasurements
        {
            get
            {
                return m_latestMeasurements;
            }
        }

        /// <summary>
        /// Gets reference to the last published <see cref="IFrame"/>.
        /// </summary>
        public IFrame LastFrame
        {
            get
            {
                return m_frameQueue.Last;
            }
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        public int FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                m_framesPerSecond = value;
                m_ticksPerFrame = (decimal)Ticks.PerSecond / (decimal)m_framesPerSecond;

                if (m_frameQueue != null)
                    m_frameQueue.TicksPerFrame = m_ticksPerFrame;

                // Calculate new wait time periods for new number of frames per second
                int[] framePeriods = new int[m_framesPerSecond];

                for (int frameIndex = 0; frameIndex < m_framesPerSecond; frameIndex++)
                {
                    framePeriods[frameIndex] = CalcWaitTimeForFrameIndex(m_framesPerSecond, frameIndex);
                }

                Interlocked.Exchange(ref m_framePeriods, framePeriods);
            }
        }

        /// <summary>
        /// Gets the number of ticks per frame.
        /// </summary>
        public decimal TicksPerFrame
        {
            get
            {
                return m_ticksPerFrame;
            }
        }

        /// <summary>
        /// Gets or sets the current enabled state of concentrator.
        /// </summary>
        /// <returns>Current enabled state of concentrator</returns>
        /// <remarks>
        /// Concentrator must be started by calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>) before concentration will begin.
        /// </remarks>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }
        /// <summary>
        /// Gets the total amount of time, in seconds, that the concentrator has been active.
        /// </summary>
        public virtual Time RunTime
        {
            get
            {
                Ticks processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                    {
                        processingTime = m_stopTime - m_startTime;
                    }
                    else
                    {
#if UseHighResolutionTime
                        processingTime = PrecisionTimer.UtcNow.Ticks - m_startTime;
#else
                        processingTime = DateTime.UtcNow.Ticks - m_startTime;
#endif
                    }
                }

                if (processingTime < 0) processingTime = 0;

                return processingTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps
        /// to be sorted by arrival time.
        /// </summary>
        /// <remarks>
        /// Value defaults to <c>true</c>, so any incoming measurement with a bad timestamp quality will be sorted
        /// according to its arrival time. Setting the property to <c>false</c> will cause all measurements with a
        /// bad timestamp quality to be discarded.
        /// </remarks>
        public bool AllowSortsByArrival
        {
            get
            {
                return m_allowSortsByArrival;
            }
            set
            {
                m_allowSortsByArrival = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether or not to use the local clock time as real time.
        /// </summary>
        /// <remarks>
        /// Use your local system clock as real time only if the time is locally GPS-synchronized,
        /// or if the measurement values being sorted were not measured relative to a GPS-synchronized clock.
        /// </remarks>
        public bool UseLocalClockAsRealTime
        {
            get
            {
                return m_useLocalClockAsRealTime;
            }
            set
            {
                m_useLocalClockAsRealTime = value;
            }
        }

        /// <summary>
        /// If using local clock as real time, this function will return UtcNow.Ticks. Otherwise, this function will return ticks
        /// of most recent measurement, or local clock ticks if no measurements are within time deviation tolerances.
        /// </summary>
        /// <remarks>
        /// Because the measurements being received by remote devices are often measured relative to GPS time, these timestamps
        /// are typically more accurate than the local clock. As a result, we can use the latest received timestamp as the best
        /// local time measurement we have (this method ignores transmission delays); but, even these times can be incorrect so
        /// we still have to apply reasonability checks to these times. To do this, we use the local system time and the
        /// <see cref="ConcentratorBase.LeadTime"/> value to validate the latest measured timestamp. If the newest received
        /// measurement timestamp gets too old or creeps too far into the future (both validated + and - against defined lead
        /// time property value), we will fall back on local system time. Note that this creates a dependency on a fairly accurate
        /// local clock - the smaller the lead time deviation tolerance, the better the needed local clock acuracy. For example, a
        /// lead time deviation tolerance of a few seconds might only require keeping the local clock synchronized to an NTP time
        /// source; but, a sub-second tolerance would require that the local clock be very close to GPS time.
        /// </remarks>
        public Ticks RealTime
        {
            get
            {
                if (m_useLocalClockAsRealTime)
                {
                    // Assumes local system clock is the best value we have for real time.
#if UseHighResolutionTime
                    return PrecisionTimer.UtcNow.Ticks;
#else
                    return DateTime.UtcNow.Ticks;
#endif
                }
                else
                {
                    // If the current value for real-time is outside of the time deviation tolerance of the local
                    // clock, then we set latest measurement time (i.e., real-time) to be the current local clock
                    // time. Since the lead time typically defines the tolerated accuracy of the local clock to
                    // real-time we will use this value as the + and - timestamp tolerance to validate if the
                    // measurement time is reasonable.
#if UseHighResolutionTime
                    long currentTimeTicks = PrecisionTimer.UtcNow.Ticks;
#else
                    long currentTimeTicks = DateTime.UtcNow.Ticks;
#endif
                    long currentRealTimeTicks = m_realTimeTicks;
                    double distance = (currentTimeTicks - currentRealTimeTicks) / (double)Ticks.PerSecond;

                    if (distance > m_leadTime || distance < -m_leadTime)
                    {
                        // Set real time ticks to current ticks (as long as another thread hasn't changed it
                        // already), the interlocked compare exchange avoids an expensive synclock to update real
                        // time ticks.
                        Interlocked.CompareExchange(ref m_realTimeTicks, currentTimeTicks, currentRealTimeTicks);
                    }

                    // Assume lastest measurement timestamp is the best value we have for real-time.
                    return m_realTimeTicks;
                }
            }
        }

        /// <summary>
        /// Gets the total number of measurements that have ever been requested for sorting.
        /// </summary>
        public long TotalMeasurements
        {
            get
            {
                return m_totalMeasurements;
            }
        }

        /// <summary>
        /// Gets the total number of measurements that have been discarded because of old timestamps
        /// (i.e., measurements that were outside the time deviation tolerance from base time, past or future).
        /// </summary>
        public long DiscardedMeasurements
        {
            get
            {
                return m_discardedMeasurements;
            }
        }

        /// <summary>
        /// Gets a reference the last <see cref="IMeasurement"/> that was discarded by the concentrator.
        /// </summary>
        public IMeasurement LastDiscardedMeasurement
        {
            get
            {
                return m_lastDiscardedMeasurement;
            }
        }

        /// <summary>
        /// Gets the total number of published measurements.
        /// </summary>
        public long PublishedMeasurements
        {
            get
            {
                return m_publishedMeasurements;
            }
        }

        /// <summary>
        /// Gets the total number of published frames.
        /// </summary>
        public long PublishedFrames
        {
            get
            {
                return m_publishedFrames;
            }
        }

        /// <summary>
        /// Gets the total number of measurements that were sorted by arrival because the measurement reported a bad timestamp quality.
        /// </summary>
        public long MeasurementsSortedByArrival
        {
            get
            {
                return m_measurementsSortedByArrival;
            }
        }

        /// <summary>
        /// Gets the total number of seconds frames have spent in the publication process since concentrator started.
        /// </summary>
        public Time TotalPublicationTime
        {
            get
            {
                return m_totalPublishTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets the average required frame publication time, in seconds.
        /// </summary>
        /// <remarks>
        /// If user publication function, <see cref="ConcentratorBase.PublishFrame"/>, consistently exceeds available publishing time
        /// (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c>), concentration will fall behind.
        /// </remarks>
        public Time AveratePublicationTimePerFrame
        {
            get
            {
                return TotalPublicationTime / m_publishedFrames;
            }
        }

        /// <summary>
        /// Gets current detailed state and status of concentrator for display purposes.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                IFrame lastFrame = m_frameQueue.Last;
#if UseHighResolutionTime
                DateTime currentTime = PrecisionTimer.UtcNow;
#else
                DateTime currentTime = DateTime.UtcNow;
#endif

                status.AppendFormat("     Data concentration is: {0}", Enabled ? "Enabled" : "Disabled");
                status.AppendLine();
                status.AppendFormat("    Total process run time: {0}", RunTime.ToString());
                status.AppendLine();
                status.AppendFormat("    Measurement wait delay: {0} seconds (lag time)", m_lagTime);
                status.AppendLine();
                status.AppendFormat("     Local clock tolerance: {0} seconds (lead time)", m_leadTime);
                status.AppendLine();
                status.AppendFormat("    Local clock time (UTC): {0}", currentTime.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                status.AppendLine();
                status.AppendFormat("  Using clock as real-time: {0}", m_useLocalClockAsRealTime);
                status.AppendLine();
                if (!m_useLocalClockAsRealTime)
                {
                    status.Append("      Local clock accuracy: ");
#if UseHighResolutionTime
                    status.Append(SecondsFromRealTime(PrecisionTimer.UtcNow.Ticks).ToString("0.0000"));
#else
                    status.Append(SecondsFromRealTime(DateTime.UtcNow.Ticks).ToString("0.0000"));
#endif
                    status.Append(" second deviation from latest time");
                    status.AppendLine();
                }
                status.AppendFormat(" Allowing sorts by arrival: {0}", m_allowSortsByArrival);
                status.AppendLine();
                status.AppendFormat("        Total measurements: {0}", m_totalMeasurements);
                status.AppendLine();
                status.AppendFormat("    Published measurements: {0}", m_publishedMeasurements);
                status.AppendLine();
                status.AppendFormat("    Discarded measurements: {0}", m_discardedMeasurements);
                status.AppendLine();
                status.Append("Last discarded measurement: ");
                if (m_lastDiscardedMeasurement == null)
                {
                    status.Append("<none>");
                }
                else
                {
                    status.Append(Measurement.ToString(m_lastDiscardedMeasurement));
                    status.Append(" - ");
                    status.Append(((DateTime)m_lastDiscardedMeasurement.Timestamp).ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                }
                status.AppendLine();
                status.AppendFormat("    Total sorts by arrival: {0}", m_measurementsSortedByArrival);
                status.AppendLine();
                status.AppendFormat("   Missed sorts by timeout: {0}", m_missedSortsByTimeout);
                status.AppendLine();
                status.AppendFormat("  Average publication time: {0} milliseconds", (AveratePublicationTimePerFrame / SI.Milli).ToString("0.0000"));
                status.AppendLine();
                status.AppendFormat(" User function utilization: {0} of available time used", ((decimal)1.0 - (m_ticksPerFrame - (decimal)AveratePublicationTimePerFrame.ToTicks()) / m_ticksPerFrame).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("Published measurement loss: {0}", (m_discardedMeasurements / (double)m_totalMeasurements).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("      Loss due to timeouts: {0}", (m_missedSortsByTimeout / (double)m_totalMeasurements).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat(" Measurement time accuracy: {0}", (1.0D - m_measurementsSortedByArrival / (double)m_totalMeasurements).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("    Total published frames: {0}", m_publishedFrames);
                status.AppendLine();
                status.AppendFormat("        Defined frame rate: {0} frames/sec, {1} ticks/frame", m_framesPerSecond, m_ticksPerFrame.ToString("0.00"));
                status.AppendLine();
                status.AppendFormat("    Actual mean frame rate: {0} frames/sec", (m_publishedFrames / (RunTime - m_lagTime)).ToString("0.00"));
                status.AppendLine();
                status.AppendFormat("        Queued frame count: {0}", m_frameQueue.Count);
                status.AppendLine();
                status.Append("      Last published frame: ");

                if (lastFrame == null)
                {
                    status.Append("<none>");
                }
                else
                {
                    status.Append(((DateTime)lastFrame.Timestamp).ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                    status.AppendLine();
                    status.Append("   Last sorted measurement: ");
                    status.Append(Measurement.ToString(lastFrame.LastSortedMeasurement));
                }
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="FrameQueue"/>.
        /// </summary>
        protected FrameQueue FrameQueue
        {
            get
            {
                return m_frameQueue;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ConcentratorBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ConcentratorBase"/> object and optionally releases the managed resources.
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
                        if (m_publicationTimer != null)
                        {
                            m_publicationTimer.Tick -= PublishFrames;
                            m_publicationTimer.Dispose();
                        }
                        m_publicationTimer = null;

                        if (m_frameQueue != null)
                        {
                            m_frameQueue.Dispose();
                        }
                        m_frameQueue = null;

                        if (m_monitorTimer != null)
                        {
                            m_monitorTimer.Elapsed -= MonitorUnpublishedSamples;
                            m_monitorTimer.Dispose();
                        }
                        m_monitorTimer = null;

                        if (m_latestMeasurements != null)
                        {
                            m_latestMeasurements.Dispose();
                        }
                        m_latestMeasurements = null;

                        m_lastDiscardedMeasurement = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the concentrator, if it is not already running.
        /// </summary>
        /// <remarks>
        /// Concentrator must be started by calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>) before concentration will begin.
        /// </remarks>
        public virtual void Start()
        {
            if (!m_enabled)
            {
                // Reset statistics
                m_totalMeasurements = 0;
                m_measurementsSortedByArrival = 0;
                m_discardedMeasurements = 0;
                m_publishedMeasurements = 0;
                m_missedSortsByTimeout = 0;
                m_publishedFrames = 0;
                m_totalPublishTime = 0;
                m_stopTime = 0;
#if UseHighResolutionTime
                m_startTime = PrecisionTimer.UtcNow.Ticks;
#else
                m_startTime = DateTime.UtcNow.Ticks;
#endif
                m_frameQueue.Clear();

                // Start real-time frame publication
                m_frameIndex = 0;
                m_lastFramePeriod = (int)(m_ticksPerFrame / (decimal)Ticks.PerMillisecond);
                m_publicationTimer.Period = m_lastFramePeriod;
                m_publicationTimer.Start();
                m_monitorTimer.Start();
            }

            m_enabled = true;
        }

        /// <summary>
        /// Stops the concentrator.
        /// </summary>
        public virtual void Stop()
        {
            if (m_enabled)
            {
                m_publicationTimer.Stop();
                m_monitorTimer.Stop();
                m_frameQueue.Clear();
            }

            m_enabled = false;
#if UseHighResolutionTime
            m_stopTime = PrecisionTimer.UtcNow.Ticks;
#else
            m_stopTime = DateTime.UtcNow.Ticks;
#endif
        }

        /// <summary>
        /// Returns the deviation, in seconds, that the given number of ticks is from real-time (i.e., <see cref="ConcentratorBase.RealTime"/>).
        /// </summary>
        /// <param name="timestamp">Timestamp to calculate distance from real-time.</param>
        public double SecondsFromRealTime(Ticks timestamp)
        {
            return (RealTime - timestamp).ToSeconds();
        }

        /// <summary>
        /// Returns the deviation, in milliseconds, that the given number of ticks is from real-time (i.e., <see cref="ConcentratorBase.RealTime"/>).
        /// </summary>
        /// <param name="timestamp">Timestamp to calculate distance from real-time.</param>
        public double MillisecondsFromRealTime(Ticks timestamp)
        {
            return (RealTime - timestamp).ToMilliseconds();
        }

        /// <summary>
        /// Sorts the <see cref="IMeasurement"/> placing the data point in its proper <see cref="IFrame"/>.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> to sort.</param>
        public virtual void SortMeasurement(IMeasurement measurement)
        {
            SortMeasurements(new IMeasurement[] { measurement });
        }

        /// <summary>
        /// Sorts each <see cref="IMeasurement"/> placing each data point in its proper <see cref="IFrame"/>.
        /// </summary>
        /// <param name="measurements">Collection of <see cref="IMeasurement"/>'s to sort.</param>
        public virtual void SortMeasurements(IEnumerable<IMeasurement> measurements)
        {
            // This function is called continually with new measurements to handle "time-alignment" (i.e., sorting)
            // of these new values. Many threads will be waiting for frames of time aligned data so make sure any
            // work to be done here is executed as efficiently as possible.

            // Note that breaking up this function into several parts might help with readability and make it
            // easier to maintain, but to reduce function calls (and hence save time) the decision was made to
            // put the code into one larger more complex function...

            IFrame frame = null;
            Ticks timestamp = 0;
            Ticks lastTimestamp = 0;
            double distance;
            bool discardMeasurement;

            // Track the total number of measurements ever requested for sorting.
            Interlocked.Add(ref m_totalMeasurements, measurements.Count());

            // Measurements usually come in groups. This function processes all available measurements in the
            // collection here directly as an optimization which avoids the overhead of a function call for
            // each measurement.
            foreach (IMeasurement measurement in measurements)
            {
                // Reset flag for next measurement.
                discardMeasurement = false;

                // Check for a bad measurement timestamp.
                if (!measurement.TimestampQualityIsGood)
                {
                    if (m_allowSortsByArrival)
                    {
                        // Device reports measurement timestamp as bad; this typically means that the GPS timestamp of the
                        // source device is not accurate. If the concentrator is set to allow sorts by arrival then it is
                        // assumed that our local real time value is better than the device measurement, so we sort the
                        // measurement by arrival time.
                        timestamp = RealTime;
                        Interlocked.Increment(ref m_measurementsSortedByArrival);
                    }
                    else
                    {
                        // If sorting by arrival time is not allowed, measurements with bad timestamps are discarded.
                        discardMeasurement = true;
                    }
                }
                else
                {
                    // Timestamp quality is good, get ticks for this measurement.
                    timestamp = measurement.Timestamp;
                }

                if (!discardMeasurement)
                {
                    //
                    // *** Sort the measurement into proper frame ***
                    //

                    // Get the destination frame for the measurement. Note that groups of parsed measurements will
                    // typically be coming in from the same source and will have the same ticks. If we have already
                    // found the destination frame for the same ticks, then there is no need to lookup frame again.
                    if (frame == null || timestamp != lastTimestamp)
                    {
                        // Badly time-aligned measurements, or those coming in at a higher sample rate, may fall
                        // outside available frame buckets. To check for this, the difference between the measurement
                        // timestamp and real-time in seconds is calculated and validated between lag and lead times.
                        distance = SecondsFromRealTime(timestamp);

                        if (distance > m_lagTime || distance < -m_leadTime)
                        {
                            // This data has come in late or has a future timestamp.  For old timestamps, we're not
                            // going to create a frame for data that will never be processed.  For future dates we
                            // must assume that the clock from source device must be advanced and out-of-sync with
                            // real-time - either way this data will be discarded.
                            frame = null;
                        }
                        else
                        {
                            // Get a frame for this measurement
                            frame = m_frameQueue.GetFrame(timestamp);
                            lastTimestamp = timestamp;
                        }
                    }

                    if (frame == null)
                    {
                        // Measurement is discarded if no bucket (i.e., destination frame) was found for it.
                        discardMeasurement = true;
                        lastTimestamp = 0;
                    }
                    else
                    {
                        // Assign new measurement to its frame using user customizable function.
                        if (AssignMeasurementToFrame(frame, measurement))
                        {
                            frame.LastSortedMeasurement = measurement;
                        }
                        else
                        {
                            // Track the total number of measurements that failed to sort because the
                            // system ran out of time.
                            Interlocked.Increment(ref m_missedSortsByTimeout);

                            // Count this as a discarded measurement if it was never assigned to the frame.
                            discardMeasurement = true;
                        }

                        // If enabled, concentrator will track the absolute latest measurement values.
                        if (m_trackLatestMeasurements)
                            m_latestMeasurements.UpdateMeasurementValue(measurement);
                    }
                }

                if (discardMeasurement)
                {
                    // This measurement was marked to be discarded.
                    Interlocked.Increment(ref m_discardedMeasurements);
                    m_lastDiscardedMeasurement = measurement;
                }
                else
                {
                    //
                    // *** Manage "real-time" ticks ***
                    //

                    if (!m_useLocalClockAsRealTime)
                    {
                        // Algorithm:
                        //      If the measurement time is newer than the current real time value and within the
                        //      specified time deviation tolerance of the local clock time, then the measurement
                        //      timestamp is set as real time.
                        long realTimeTicks = m_realTimeTicks;

                        if (timestamp > m_realTimeTicks)
                        {
                            // Apply a resonability check to this value using the local clock. Since the lead time
                            // typically defines the tolerated accuracy of the local clock to real time, this value
                            // is used as the + and - timestamp tolerance to validate if the time is reasonable.
#if UseHighResolutionTime
                            long currentTimeTicks = PrecisionTimer.UtcNow.Ticks;
#else
                            long currentTimeTicks = DateTime.UtcNow.Ticks;
#endif
                            if (timestamp.TimeIsValid(currentTimeTicks, m_leadTime, m_leadTime))
                            {
                                // The new time measurement looks good, so this function assumes the time is
                                // "real time" so long as another thread has not changed the real time value
                                // already. Using the interlocked compare exchange method introduces the
                                // possibility that we may have had newer ticks than another thread that just
                                // updated real-time ticks, but if so the deviation will not be much since ticks
                                // were greater than current real-time ticks in all threads that got to this
                                // point. Besides, newer measurements are always coming in anyway and the compare
                                // exchange method saves a call to a monitor lock thereby reducing contention.
                                Interlocked.CompareExchange(ref m_realTimeTicks, timestamp, realTimeTicks);
                            }
                            else
                            {
                                // Measurement ticks were outside of time deviation tolerances so we'll also check to make
                                // sure current real-time ticks are within these tolerances as well
                                distance = (currentTimeTicks - m_realTimeTicks) / (double)Ticks.PerSecond;

                                if (distance > m_leadTime || distance < -m_leadTime)
                                {
                                    // New time measurement was invalid as was current real-time value so we have no choice but to
                                    // assume the current time as "real-time", so we set real time ticks to current ticks so long
                                    // as another thread hasn't changed it already
                                    Interlocked.CompareExchange(ref m_realTimeTicks, currentTimeTicks, realTimeTicks);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <remarks>
        /// If user implemented publication function consistently exceeds available publishing time (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds),
        /// concentration will fall behind. A small amount of this time is required by the <see cref="ConcentratorBase"/> for processing overhead, so actual total time
        /// available for user function process will always be slightly less than <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds.
        /// </remarks>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected abstract void PublishFrame(IFrame frame, int index);

        /// <summary>
        /// Consumers can choose to override this method to create a new custom <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// Override is optional. By default, the base class will create a basic <see cref="Frame"/> to hold synchronized measurements.
        /// </remarks>
        protected internal virtual IFrame CreateNewFrame(long ticks)
        {
            return new Frame(ticks);
        }

        /// <summary>
        /// Consumers can choose to override this method to handle custom assignment of a <see cref="IMeasurement"/> to its <see cref="IFrame"/>.
        /// </summary>
        /// <returns>True if <see cref="IMeasurement"/> was successfully assigned to its <see cref="IFrame"/>.</returns>
        /// <remarks>
        /// <para>
        /// Override is optional. By default, a measurement will simply be assigned to frame's keyed measurement dictionary.
        /// </para>
        /// <example>
        /// If overridden user must perform their own synchronization as needed, for example:
        /// <code>
        /// lock (frame.Measurements)
        /// {
        ///     if (!frame.Published)
        ///     {
        ///         frame.Measurements[measurement.Key] = measurement;
        ///         return true;
        ///     }
        ///     else
        ///     {
        ///         return false;
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <para>
        /// Note that the <see cref="IFrame.Measurements"/> dictionary is used internally to synchrnonize assignment
        /// of the <see cref="IFrame.Published"/> flag. If your custom <see cref="IFrame"/> makes use of the
        /// <see cref="IFrame.Measurements"/> dictionary you must implement a locking scheme similar to the sample
        /// code to prevent changes to the measurement dictionary during frame publication.
        /// </para>
        /// </remarks>
        protected virtual bool AssignMeasurementToFrame(IFrame frame, IMeasurement measurement)
        {
            IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;

            lock (measurements)
            {
                if (!frame.Published)
                {
                    measurements[measurement.Key] = measurement;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ProcessException"/> event.</param>
        /// <remarks>
        /// Allows derived classes to raise a processing exception.
        /// </remarks>
        protected void OnProcessException(Exception ex)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        // Member variables being updated are only updated here so we don't worry about atomic operations on these variables.
        // Note that this is the PrecisionTimer "Tick" delgate handler.
        private void PublishFrames(object sender, EventArgs e)
        {
            IFrame frame;
            Ticks ticks;
            int frameIndex, period;
            long distance;

            // First things first, prepare timer period for next call...
            m_frameIndex++;

            if (m_frameIndex >= m_framesPerSecond)
                m_frameIndex = 0;

            // Get the frame period for this frame index
            period = m_framePeriods[m_frameIndex];

            // We only update timer period if it has changed since last call. Note that this is necessary since
            // timer periods are defined as integers but actual period is typically uneven (e.g., 33.333 ms)
            if (m_lastFramePeriod != period)
                m_publicationTimer.Period = period;

            m_lastFramePeriod = period;

            // Keep publishing frames so long as they are ready for publication. This handles case where
            // system may be falling behind because user function is taking too long - exit when no
            // other frames are available to process

            while (true)
            {
                try
                {
                    // Get top frame
                    frame = m_frameQueue.Head;

                    if (frame == null)
                    {
                        // No frame ready to publish, exit
                        break;
                    }
                    else
                    {
                        // Get ticks for this frame
                        ticks = frame.Timestamp;

                        // See if any lagtime needs to pass before we begin publishing,
                        // distance is calculated in ticks
                        distance = m_lagTicks - (RealTime - ticks);

                        // Exit if it's not time to publish
                        if (distance > 0) break;

                        // Mark start time for publication
#if UseHighResolutionTime
                        distance = PrecisionTimer.UtcNow.Ticks;
#else
                        distance = DateTime.UtcNow.Ticks;
#endif

                        // Calculate index of this frame within its second - note that we have to calculate this
                        // value instead of using m_frameIndex since it is is possible for multiple frames to be
                        // published within one frame period if the system is stressed
                        frameIndex = (int)((decimal)ticks.DistanceBeyondSecond() / m_ticksPerFrame);

                        // Mark the frame as published to prevent any further sorting into this frame
                        lock (frame.Measurements)
                        {
                            // Setting this flag is in a critcal section to ensure that
                            // sorting into this frame has ceased prior to publication...
                            frame.Published = true;
                        }

                        try
                        {
                            // Publish the current frame (i.e., call user implemented publication function)
                            PublishFrame(frame, frameIndex);
                        }
                        finally
                        {
                            // Remove the frame from the queue whether it is successfully published or not
                            m_frameQueue.Pop();

                            // Update publication statistics
                            m_publishedFrames++;
                            m_publishedMeasurements += frame.PublishedMeasurements;

                            // Track total publication time
#if UseHighResolutionTime
                            m_totalPublishTime += PrecisionTimer.UtcNow.Ticks - distance;
#else
                            m_totalPublishTime += DateTime.UtcNow.Ticks - distance;
#endif
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Not stopping for exceptions - but we'll let user know there are issues...
                    OnProcessException(ex);
                    break;
                }
            }
        }

        // Exposes the number of unpublished seconds of data in the queue (note that first second of data will always be "publishing").
        private void MonitorUnpublishedSamples(object sender, System.Timers.ElapsedEventArgs e)
        {
            int secondsOfData = (m_frameQueue.Count / m_framesPerSecond) - 1;

            if (secondsOfData < 0) secondsOfData = 0;

            if (UnpublishedSamples != null)
                UnpublishedSamples(this, new EventArgs<int>(secondsOfData));
        }

        // Wait times are not necessarily perfectly even (e.g., at 30 samples per second wait time per frame is 33.333... milliseconds)
        // so we use this function to evenly distribute wait times across a second.
        private int CalcWaitTimeForFrameIndex(int framesPerSecond, int frameIndex)
        {
            // Jian Zuo...
            int millisecondsWaitTime;
            int frameRate;
            int deficit;

            frameRate = (int)(Math.Round(1000.0D / framesPerSecond));
            deficit = 1000 - frameRate * framesPerSecond;

            if (deficit == 0)
            {
                millisecondsWaitTime = frameRate;
            }
            else
            {
                if (frameIndex == 0)
                {
                    millisecondsWaitTime = frameRate;
                }
                else if (frameIndex == framesPerSecond - 1)
                {
                    millisecondsWaitTime = frameRate + (deficit > 0 ? 1 : -1);
                }
                else
                {
                    double interval = framesPerSecond / Math.Abs((double)deficit);
                    double pre_dis = mod_dis(frameIndex - 1, interval);
                    double cur_dis = mod_dis(frameIndex, interval);
                    double next_dis = mod_dis(frameIndex + 1, interval);

                    millisecondsWaitTime = frameRate + ((cur_dis <= pre_dis && cur_dis < next_dis) ? (deficit > 0 ? 1 : -1) : 0);
                }
            }

            return millisecondsWaitTime;
        }

        private double mod_dis(int framesIndex, double interval)
        {
            double dis2 = (framesIndex + 1) % interval;
            double dis1 = interval - dis2;

            return (dis1 < dis2 ? dis1 : dis2);
        }

        #region [ Old Code Reference ]

        //                        ' Makes sure the starting sort time for this frame is initialized.
        //                        If frame.StartSortTime = 0 Then
        //#If UseHighResolutionTime Then
        //                            frame.StartSortTime = PrecisionTimer.UtcNow.Ticks
        //#Else
        //                            frame.StartSortTime = Date.UtcNow.Ticks
        //#End If
        //                        End If

        //                            ' Tracks the last sorted measurement in this frame.
        //#If UseHighResolutionTime Then
        //                            frame.LastSortTime = PrecisionTimer.UtcNow.Ticks
        //#Else
        //                            frame.LastSortTime = Date.UtcNow.Ticks
        //#End If

        //.Append(lastFrame.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss.fff"))
        //.Append(" - sort time: ")

        //' Calculates maximum sort time for publishing frame.
        //If lastFrame.StartSortTime > 0 AndAlso lastFrame.LastSortTime > 0 Then
        //    .Append(TicksToSeconds(lastFrame.LastSortTime - lastFrame.StartSortTime).ToString("0.0000"))
        //    .Append(" seconds")
        //Else
        //    .Append("undetermined")
        //End If

        //' Calculates total time from last measurement ticks.
        //If lastFrame.LastSortTime > 0 Then
        //    .Append(" - ")
        //    .Append(TicksToSeconds(NotLessThan(lastFrame.LastSortTime - lastFrame.LastSortedMeasurement.Ticks, 0L)).ToString("0.0000"))
        //    .Append(" seconds from source time")
        //Else
        //    .Append(" - deviation from source time undetermined")
        //End If

        //' Calculate total time required to sort measurements into this frame.
        //If frame.StartSortTime > 0 AndAlso frame.LastSortTime > 0 Then m_totalSortTime += (frame.LastSortTime - frame.StartSortTime)

        //Private m_useHighResolutionTimer As Boolean                             ' Determines whether or not to use high-resolution timer

        //' <summary>
        //' Determines whether or not to use the high-resolution multi-media timer for frame publication.
        //' </summary>
        //' <remarks>
        //' Using the high-resolution timer will provide the highest accuracy on frame publication intervals and
        //' will allow the maximum amount of time for the user publication function.  However, this timer will
        //' cause an increase in the required system CPU resources.  This property allows multiple instances
        //' of the concentration class to be "tuned" for their function and criticality.
        //' </remarks>
        //Public Property UseHighResolutionTimer() As Boolean
        //    Get
        //        Return m_useHighResolutionTimer
        //    End Get
        //    Set(ByVal value As Boolean)
        //        m_useHighResolutionTimer = value
        //    End Set
        //End Property

        //.Append(" Using hi-resolution timer: ")
        //.Append(m_useHighResolutionTimer)
        //.AppendLine()


        //If m_waitTimer IsNot Nothing Then
        //    RemoveHandler m_waitTimer.Tick, AddressOf WaitTimeout
        //    m_waitTimer.Dispose()
        //End If
        //m_waitTimer = Nothing

        //Private Sub WaitTimeout(ByVal sender As Object, ByVal e As EventArgs)

        //    If Monitor.TryEnter(m_waitTimer) Then
        //        Try
        //            ' Release waiting thread
        //            Monitor.PulseAll(m_waitTimer)
        //        Finally
        //            Monitor.Exit(m_waitTimer)
        //        End Try
        //    End If

        //End Sub


        // Note: 08/01/2007 - Moved this code directly into the sorting function since this was the only place it was
        // being called to optimize by preventing function call overhead


        //' <summary>This critical function automatically manages the sample queue based on timestamps of incoming measurements</summary>
        //' <returns>The sample associated with the specified timestamp. If the sample is not found at timestamp, it will be created.</returns>
        //' <param name="ticks">Ticks of the timestamp of the sample to get</param>
        //' <remarks>Function will return null if timestamp is outside of the specified time deviation tolerance</remarks>
        //Protected Function GetSample(ByVal ticks As Long) As Sample

        //    ' Baseline measurement timestamp at bottom of the second
        //    Dim baseTimeTicks As Long = BaselinedTimestamp(ticks, BaselineTimeInterval.Second).Ticks
        //    Dim sample As Sample = LookupSample(baseTimeTicks)

        //    ' Enter loop to wait until the sample exists, we will attempt to enter critical section and create it ourselves
        //    Do Until sample IsNot Nothing
        //        ' We don't want to step on our own toes when creating new samples - so we create a critical section for
        //        ' this code - if another thread is busy creating samples, we'll just wait for it below
        //        If Monitor.TryEnter(m_sampleQueue.SyncRoot) Then
        //            Try
        //                ' Check difference between timestamp and current sample base-time in seconds and fill in any gaps.
        //                ' Note that current sample base-time will be validated against local clock
        //                Dim distance As Double = DistanceFromRealTime(ticks)

        //                If distance > m_lagTime OrElse distance < -m_leadTime Then
        //                    ' This data has come in late or has a future timestamp.  For old timestamps, we're not
        //                    ' going to create a sample for data that will never be processed.  For future dates we
        //                    ' must assume that the clock from source device must be advanced and out-of-sync with
        //                    ' real-time - either way this data will be discarded.
        //                    Exit Do
        //                ElseIf distance > 1 Then
        //                    ' Add intermediate samples as needed...
        //                    For x As Integer = 1 To Math.Floor(distance)
        //                        CreateSample(m_currentSampleTimestamp.AddSeconds(x).Ticks)
        //                    Next
        //                End If

        //                ' Create sample for new base time
        //                CreateSample(baseTimeTicks)
        //            Catch
        //                ' Rethrow any exceptions - we are just catching any exceptions so we can
        //                ' make sure to release thread lock in finally
        //                Throw
        //            Finally
        //                Monitor.Exit(m_sampleQueue.SyncRoot)
        //            End Try
        //        Else
        //            ' We sleep the thread between loops to help reduce CPU loading...
        //            Thread.Sleep(1)
        //        End If

        //        ' If we just created the sample we needed, then we'll get it here.  Otherwise the sample may have been
        //        ' created by another thread while we were sleeping, so we'll check again to see to see if sample exists.
        //        ' Additionally, the TryGetValue function (referenced from within LookupSample) internally performs a
        //        ' SyncLock on the SyncRoot and waits for it to be released, so if another thread was creating new
        //        ' samples then we'll definitely pick up our needed sample when the lock is released.  Nice and safe.
        //        sample = LookupSample(baseTimeTicks)
        //    Loop

        //    ' Return sample for this timestamp
        //    Return sample

        //End Function

        //' <summary>Gets the sample associated with the specified timestamp.</summary>
        //' <returns>The sample associated with the specified timestamp. If the specified timestamp is not found, property returns null.</returns>
        //' <param name="ticks">The ticks of the baselined timestamp of the sample to get.</param>
        //Protected Function LookupSample(ByVal ticks As Long) As Sample

        //    Dim foundSample As Sample

        //    ' Lookup sample with specified baselined ticks (this is internally SyncLock'd)
        //    If m_sampleQueue.TryGetValue(ticks, foundSample) Then
        //        Return foundSample
        //    Else
        //        Return Nothing
        //    End If

        //End Function

        //' Creates a new sample associated with the specified baselined timestamp ticks, if it doesn't already exist
        //Private Sub CreateSample(ByVal ticks As Long)

        //    If Not m_sampleQueue.ContainsKey(ticks) Then m_sampleQueue.Add(ticks, New Sample(Me, ticks))

        //End Sub

        //If measurements.TryGetValue(measurement.Key, foundMeasurement) Then
        //    ' Measurement already exists, so we just update with the latest value
        //    foundMeasurement.Value = measurement.Value
        //Else
        //    ' Create new frame measurement if it doesn't exist
        //    measurements.Add(measurement.Key, measurement)
        //End If

        //SyncLock m_setRealTimeTicksLock
        //    ' Since real-time ticks value may have been updated by other threads since we aquired the lock
        //    ' we'll check to see if we still need to set real-time ticks to the current time
        //    Dim updateRealTimeTicks As Boolean = (currentRealTimeTicks = m_realTimeTicks)

        //    If Not updateRealTimeTicks Then
        //        distance = (currentTimeTicks - m_realTimeTicks) / TicksPerSecond
        //        updateRealTimeTicks = (distance > m_lagTime OrElse distance < -m_leadTime)
        //    End If

        //    If updateRealTimeTicks Then m_realTimeTicks = currentTimeTicks
        //End SyncLock


        // Note: 11/02/2007 - removed the notion of a sample when KeyedProcessQueue was replaced by FrameQueue
        // this was to reduce contention caused by sync-locking the sample queue continually caused by calling
        // the "CanProcessItem" implementation...

        //Private WithEvents m_sampleQueue As KeyedProcessQueue(Of Long, Sample)  ' Sample processing queue (a sample represents one second of frames)
        //Private m_frameIndex As Integer                                         ' Current publishing frame index

        //' <summary>Gets the current publishing sample.</summary>
        //Public ReadOnly Property CurrentSample() As Sample
        //    Get
        //        If m_sampleQueue.Count > 0 Then
        //            Return m_sampleQueue(0%).Value
        //        Else
        //            Return Nothing
        //        End If
        //    End Get
        //End Property

        //' <summary>Gets the index of the frame that is currently, or about to be, publishing.</summary>
        //Public ReadOnly Property CurrentFrameIndex() As Integer
        //    Get
        //        Return m_frameIndex
        //    End Get
        //End Property

        //' <summary>This event is raised after a sample is published, so that consumers may handle any last
        //' minute operations on a sample before it gets released.</summary>
        //Public Event SamplePublished(ByVal sample As Sample)

        //' When all the frames have been published, this exposes the sample to consumer as the last step in sample
        //' publication cycle to allow for any last minute needed steps in measurement concentration (e.g., this
        //' could be used to step-down sample rate for data consumption by slower applications).
        //Private Sub PublishSample(ByVal ticks As Long, ByVal sample As Sample)

        //    ' Resets the frame index for the next sample after publishing all the frames of this sample.
        //    m_frameIndex = 0

        //    ' Sends out a notification that a new sample has been published, so that anyone can have a chance
        //    ' to perform any last steps with the data before we remove it from the sample queue.
        //    RaiseEvent SamplePublished(sample)

        //End Sub

        //' Each sample consists of an array of frames. The sample represents one second of data, so all frames are
        //' to get published during this second. Typically the process queue's "process item function" does the
        //' work of the queue - but in this case we use the "can process item function" to process each frame in
        //' the sample until all frames have been published. This function is executed on a real-time thread, so
        //' make sure any work to be done here is executed as efficiently as possible. This function returns True
        //' when all frames in the sample have been published. Note that this method will only be called by a
        //' single thread, and the member variables being updated are only updated here so we don't worry about
        //' atomic operations on these variables.
        //Private Function CanPublishSample(ByVal ticks As Long, ByVal sample As Sample) As Boolean

        //    Dim frame As IFrame = sample.Frames(m_frameIndex)
        //    Dim allFramesPublished As Boolean

        //    ' Frame timestamps are evenly distributed across their parent sample, so all we need to do
        //    ' is just wait for the lagtime to pass and begin publishing.
        //    If DistanceFromRealTime(frame.Ticks) >= m_lagTime Then
        //        ' Publishes the frame, after available sorting time has passed.
        //        Dim sortTime As Long
        //        Dim measurements As IDictionary(Of MeasurementKey, IMeasurement) = frame.Measurements

        //        ' Marks the frame as published to prevent any further sorting into this frame.
        //        frame.Published = True

        //        ' Publishes the current frame. Other threads handling measurement assignment are possibly still
        //        ' in motion, so it synchronizes access to the frame's measurements and sends in a copy of this
        //        ' frame and its measurements. This keeps synclock time down to a minimum and allows the user's
        //        ' frame publication method to take as long as it needs.
        //        Try
        //            PublishFrame(frame.Clone(), m_frameIndex)
        //        Catch ex As Exception
        //            RaiseEvent ProcessException(ex)
        //        End Try

        //        ' Calculates total time required to sort measurements into this frame so far.
        //        If frame.StartSortTime > 0 AndAlso frame.LastSortTime > 0 Then sortTime = frame.LastSortTime - frame.StartSortTime

        //        ' Updates publication statistics.
        //        m_totalSortTime += sortTime
        //        m_publishedFrames += 1
        //        m_publishedMeasurements += frame.PublishedMeasurements

        //        ' Increments the frame index.
        //        m_frameIndex += 1
        //        allFramesPublished = (m_frameIndex >= m_framesPerSecond)
        //    End If

        //    ' We will say sample is ready to be published (i.e., processed) once all frames have been published.
        //    Return allFramesPublished

        //End Function

        //' Exposes any process exceptions to user.
        //Private Sub m_sampleQueue_ProcessException(ByVal ex As System.Exception) Handles m_sampleQueue.ProcessException

        //    RaiseEvent ProcessException(ex)

        //End Sub

        //'
        //' *** Manages sample queue ***
        //'

        //' Groups of parsed measurements will typically be coming in from the same frame, and will
        //' have the same ticks. So, if we have already found the sample for the same ticks, then
        //' there is no need to look up the sample again.
        //If sample Is Nothing OrElse ticks <> measurement.Ticks Then
        //    ' Gets ticks for this measurement.
        //    ticks = measurement.Ticks
        //    lastTicks = 0

        //    ' Establishes the baseline measurement timestamp at bottom of the second to use as
        //    ' sample("key").
        //    baseTimeTicks = BaselinedTimestamp(ticks, BaselineTimeInterval.Second).Ticks

        //    ' Even if the measurements are sorting with different ticks, they will usually be headed
        //    ' for the same sample, so there is no need to keep looking up the same sample, if it has
        //    ' already been found.
        //    If sample Is Nothing OrElse sample.Ticks <> baseTimeTicks Then
        //        ' Automatically manages the sample queue based on timestamps of incoming measurements.
        //        SyncLock m_sampleQueue.SyncRoot
        //            If Not m_sampleQueue.TryGetValue(baseTimeTicks, sample) Then
        //                ' When a sample is not found, this function validates the measurement time by
        //                ' checking the difference between the measurement's timestamp and real time
        //                ' in seconds. Note that any timestamp within defined lead time and lag time
        //                ' is valid for sorting.
        //                distance = DistanceFromRealTime(ticks)

        //                If distance > m_lagTime OrElse distance < -m_leadTime Then
        //                    ' Discards data if either, 1) the data is late, and so the data will never
        //                    ' be processed, or 2) the data has a future timestamp and it is assumed
        //                    ' that the clock from source device must be advanced and out-of-sync with
        //                    ' real time. Sample reference will be null.
        //                    discardMeasurement = True
        //                Else
        //                    ' Creates sample for new base time.
        //                    sample = New Sample(Me, baseTimeTicks)
        //                    m_sampleQueue.Add(baseTimeTicks, sample)
        //                End If
        //            End If
        //        End SyncLock
        //    End If
        //End If

        //frameIndex = Convert.ToInt32((ticks - baseTimeTicks) / m_ticksPerFrame)

        //If frameIndex < m_framesPerSecond Then
        //    frame = sample.Frames(frameIndex)
        //    lastTicks = ticks
        //Else
        //    frame = Nothing
        //End If

        //Dim sampleDetail As New StringBuilder

        //With sampleDetail
        //    Dim currentSample As Sample

        //    SyncLock m_sampleQueue.SyncRoot
        //        Const MaximumSamplesToDisplay As Integer = 5

        //        Dim samplesToDisplay As Integer = Minimum(m_sampleQueue.Count, MaximumSamplesToDisplay)

        //        For x As Integer = 0 To samplesToDisplay - 1
        //            ' Get next sample
        //            currentSample = m_sampleQueue(x).Value

        //            .AppendLine()
        //            .Append("     Sample ")
        //            .Append(x)
        //            .Append(" @ ")
        //            .Append(currentSample.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss"))
        //            .Append(": ")

        //            If x = 0 Then
        //                Dim currentFrame As IFrame = currentSample.Frames(m_frameIndex)

        //                ' Tracks timestamp of sample being published.
        //                publishingSampleTimestamp = currentSample.Timestamp

        //                ' Appends current frame detail.
        //                .Append("publishing...")
        //                .AppendLine()
        //                .AppendLine()
        //                .Append("       Current frame = ")
        //                .Append(m_frameIndex + 1)
        //                .Append(" - sort time: ")

        //                ' Calculates maximum sort time for publishing frame.
        //                If currentFrame.StartSortTime > 0 AndAlso currentFrame.LastSortTime > 0 Then
        //                    .Append(TicksToSeconds(currentFrame.LastSortTime - currentFrame.StartSortTime).ToString("0.0000"))
        //                    .Append(" seconds")
        //                Else
        //                    .Append("undetermined")
        //                End If

        //                .AppendLine()
        //                .AppendLine()
        //                .Append("       Last measurement = ")
        //                .Append(Measurement.ToString(currentFrame.LastSortedMeasurement))

        //                ' Calculates total time from last measurement ticks.
        //                If currentFrame.LastSortTime > 0 Then
        //                    .Append(" - ")
        //                    .Append(TicksToSeconds(NotLessThan(currentFrame.LastSortTime - currentFrame.LastSortedMeasurement.Ticks, 0L)).ToString("0.0000"))
        //                    .Append(" seconds from source time")
        //                Else
        //                    .Append(" - deviation from source time undetermined")
        //                End If
        //            Else
        //                .Append("concentrating...")
        //            End If

        //            .AppendLine()
        //        Next

        //        If m_sampleQueue.Count > MaximumSamplesToDisplay Then
        //            Dim remainingSamples As Integer = m_sampleQueue.Count - MaximumSamplesToDisplay
        //            .AppendLine()
        //            .Append("     ")
        //            .Append(remainingSamples)
        //            If remainingSamples = 1 Then
        //                .Append(" more sample concentrating...")
        //            Else
        //                .Append(" more samples concentrating...")
        //            End If
        //            .AppendLine()
        //        End If
        //    End SyncLock
        //End With
        // If lock was not aquired during measurement assignment to frame while sorting, work was queued up
        // on an indepedent thread so it could take as long as necessary without delaying sort operations.

        //Private Sub AssignMeasurementToFrame(ByVal state As Object)

        //    Dim frame As IFrame
        //    Dim measurement As IMeasurement
        //    Dim measurements As IDictionary(Of MeasurementKey, IMeasurement)
        //    Dim publicationTime As Long
        //    Dim assigned As Boolean

        //    With DirectCast(state, KeyValuePair(Of IMeasurement, IFrame))
        //        measurement = .Key
        //        frame = .Value
        //    End With

        //    measurements = frame.Measurements
        //    publicationTime = measurement.Ticks + m_lagTicks

        //    ' Since this code is executing on an independent thread, it's now safe to keep
        //    ' trying for a lock until frame is published or it's passed publication time.
        //    Do Until frame.Published OrElse RealTimeTicks > publicationTime
        //        If Monitor.TryEnter(measurements) Then
        //            Try
        //                ' Calls user customizable assignment function.
        //                assigned = AssignMeasurementToFrame(frame, measurement)

        //                ' Tracks last sorted measurement in this frame.
        //                If assigned Then
        //                    frame.LastSortTime = Date.UtcNow.Ticks
        //                    frame.LastSortedMeasurement = measurement
        //                End If

        //                Exit Do
        //            Finally
        //                Monitor.Exit(measurements)
        //            End Try
        //        Else
        //            Thread.Sleep(1)
        //        End If
        //    Loop

        //    If Not assigned Then
        //        ' Count this as a discarded measurement if it was never assigned to the frame.
        //        Interlocked.Increment(m_discardedMeasurements)
        //        m_lastDiscardedMeasurement = measurement

        //        ' Track the total number of measurements that failed to sort because the
        //        ' system ran out of time trying to get a lock.
        //        Interlocked.Increment(m_missedSortsByLockTimeout)
        //    End If

        //End Sub

        //Private m_threadPoolSorts As Long                                       ' Total number of sorts deferred to thread pool because initial try lock failed

        //.Append("   Total thread pool sorts: ")
        //.Append(m_threadPoolSorts)
        //.AppendLine()

        //.Append("   Thread pool utilization: ")
        //.Append((m_threadPoolSorts / m_totalMeasurements).ToString("##0.0000%"))
        //.AppendLine()

        #endregion

        #endregion
    }
}