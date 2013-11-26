//******************************************************************************************************
//  ConcentratorBase.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  02/08/2011 - J. Ritchie Carroll
//       Added QueueState property to expose real-time queue state analysis.
//  04/14/2011 - J. Ritchie Carroll
//       Added ProcessByReceivedTimestamp property to sort and publish measurements by received time.
//  05/06/2011 - J. Ritchie Carroll / Jian (Ryan) Zuo
//       Updated to reduce lock contention and added volatile reads on stats for better accuracy.
//  10/31/2013 - Stephen C. Wills
//       Updated to concentrate time-series entities.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF.Units;
using Timer = System.Timers.Timer;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Method signature for function used to apply a down-sampling filter over a sequence of <see cref="ITimeSeriesEntity"/> objects.
    /// </summary>
    /// <param name="targetFrameTimestamp">The timestamp of the destination frame.</param>
    /// <param name="entities">Sequence of <see cref="ITimeSeriesEntity"/> objects over which to apply filter.</param>
    /// <returns>Result of filter applied to sequence of <see cref="ITimeSeriesEntity"/> objects.</returns>
    public delegate ITimeSeriesEntity TimeSeriesFilterFunction(long targetFrameTimestamp, IEnumerable<ITimeSeriesEntity> entities);

    /// <summary>
    /// Time-series concentrator base class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class synchronizes (i.e., sorts by timestamp) real-time time-series entities.
    /// </para>
    /// <para>
    /// Note that your lag time should be defined as it relates to the rate at which data data is coming into the concentrator.
    /// Make sure you allow enough time for transmission of data over the network allowing any needed time for possible network
    /// congestion.  Lead time should be defined as your confidence in the accuracy of your local clock (e.g., if you set lead
    /// time to 2, this means you trust that your local clock is within plus or minus 2 seconds of real-time.)
    /// </para>
    /// <para>
    /// This concentrator is designed to sort entities being transmitted in real-time for data being sent at rates of at least
    /// 1 sample per second. Slower rates (e.g., once every few seconds) are not supported since sorting data at these speeds
    /// would be trivial. There is no defined maximum number of supported samples per second - but keep in mind that CPU
    /// utilization will increase as the volume of time-series objects and frame rate increase.
    /// </para>
    /// </remarks>
    public abstract class ConcentratorBase : IDisposable
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Frame rate timer.
        /// </summary>
        /// <remarks>
        /// One static instance of this internal class is created per encountered frame rate / processing interval.
        /// </remarks>
        private sealed class FrameRateTimer : IDisposable
        {
            #region [ Members ]

            // Fields
            private PrecisionTimer m_timer;
            private readonly int m_framesPerSecond;
            private readonly int m_processingInterval;
            private readonly int[] m_framePeriods;
            private int m_frameIndex;
            private int m_lastFramePeriod;
            private int m_referenceCount;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Create a new <see cref="FrameRateTimer"/> class.
            /// </summary>
            /// <param name="framesPerSecond">Desired frame rate for <see cref="PrecisionTimer"/>.</param>
            /// <param name="processingInterval">Desired processing interval, if applicable.</param>
            /// <remarks>
            /// When the <paramref name="processingInterval"/> is set to -1, the frame rate timer interval will be calculated as a distribution
            /// of whole milliseconds over the specified number of <paramref name="framesPerSecond"/>. Otherwise the specified
            /// <paramref name="processingInterval"/> will be used as the timer interval.
            /// </remarks>
            public FrameRateTimer(int framesPerSecond, int processingInterval)
            {
                if (processingInterval == 0)
                    throw new InvalidOperationException("A frame rate timer should not be created when using a processing interval of zero, i.e., processing data as fast as possible.");

                m_framesPerSecond = framesPerSecond;
                m_processingInterval = processingInterval;

                // Create a new precision timer for this timer state
                m_timer = new PrecisionTimer();
                m_timer.AutoReset = true;

                if (processingInterval > 0)
                {
                    // Establish fixed timer period
                    m_timer.Period = processingInterval;
                }
                else
                {
                    // Attach handler for timer period assignments
                    m_timer.Tick += SetTimerPeriod;

                    // Calculate distributed wait time periods over specified number of frames per second
                    m_framePeriods = new int[framesPerSecond];

                    for (int frameIndex = 0; frameIndex < framesPerSecond; frameIndex++)
                    {
                        m_framePeriods[frameIndex] = CalcWaitTimeForFrameIndex(frameIndex);
                    }

                    // Establish initial timer period
                    m_lastFramePeriod = m_framePeriods[0];
                    m_timer.Period = m_lastFramePeriod;
                }

                // Start timer
                m_timer.Start();
            }

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="FrameRateTimer"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~FrameRateTimer()
            {
                Dispose(false);
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets frames per second for this <see cref="FrameRateTimer"/>.
            /// </summary>
            public int FramesPerSecond
            {
                get
                {
                    return m_framesPerSecond;
                }
            }

            /// <summary>
            /// Gets the processing interval defined for the <see cref="FrameRateTimer"/>.
            /// </summary>
            public int ProcessingInterval
            {
                get
                {
                    return m_processingInterval;
                }
            }

            /// <summary>
            /// Gets reference count for this <see cref="FrameRateTimer"/>.
            /// </summary>
            public int ReferenceCount
            {
                get
                {
                    return m_referenceCount;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases all the resources used by the <see cref="FrameRateTimer"/> object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="FrameRateTimer"/> object and optionally releases the managed resources.
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
                                if (m_processingInterval == -1)
                                    m_timer.Tick -= SetTimerPeriod;

                                m_timer.Dispose();
                            }
                            m_timer = null;
                        }
                    }
                    finally
                    {
                        m_disposed = true; // Prevent duplicate dispose.
                    }
                }
            }

            /// <summary>
            /// Adds a reference to this <see cref="FrameRateTimer"/>.
            /// </summary>
            /// <param name="tickFunction">Tick function to add to event list.</param>
            public void AddReference(EventHandler tickFunction)
            {
                m_timer.Tick += tickFunction;
                m_referenceCount++;
            }

            /// <summary>
            /// Removes a reference to this <see cref="FrameRateTimer"/>.
            /// </summary>
            /// <param name="tickFunction">Tick function to remove from event list.</param>
            public void RemoveReference(EventHandler tickFunction)
            {
                m_timer.Tick -= tickFunction;
                m_referenceCount--;
            }

            // Handler to assign next timer period
            private void SetTimerPeriod(object sender, EventArgs e)
            {
                int period;

                // First things first, prepare timer period for next call...
                m_frameIndex++;

                if (m_frameIndex >= m_framesPerSecond)
                    m_frameIndex = 0;

                // Get the frame period for this frame index
                period = m_framePeriods[m_frameIndex];

                // We only update timer period if it has changed since last call. Note that this is necessary since
                // timer periods are defined as integers but actual period is typically uneven (e.g., 33.333 ms)
                if (m_lastFramePeriod != period)
                    m_timer.Period = period;

                m_lastFramePeriod = period;
            }

            // Wait times are not necessarily perfectly even (e.g., at 30 samples per second wait time per frame is 33.333... milliseconds)
            // so we use this function to evenly distribute integer based millisecond wait times across a second.
            private int CalcWaitTimeForFrameIndex(int frameIndex)
            {
                // Jian Zuo...
                int millisecondsWaitTime;
                int frameRate;
                int deficit;

                frameRate = (int)(Math.Round(1000.0D / m_framesPerSecond));
                deficit = 1000 - frameRate * m_framesPerSecond;

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
                    else if (frameIndex == m_framesPerSecond - 1)
                    {
                        millisecondsWaitTime = frameRate + (deficit > 0 ? 1 : -1);
                    }
                    else
                    {
                        double interval = m_framesPerSecond / Math.Abs((double)deficit);
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

            #endregion
        }

        // Events

        /// <summary>
        /// This event is raised every five seconds allowing consumer to track current number of unpublished seconds of data in the queue.
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
        /// This event is raised if there are any time-series entities being discarded during the sorting process.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the enumeration of <see cref="ITimeSeriesEntity"/> objects that are being discarded during the sorting process.
        /// </remarks>
        public event EventHandler<EventArgs<IEnumerable<ITimeSeriesEntity>>> EntitiesDiscarded;

        /// <summary>
        /// This event is raised when <see cref="ConcentratorBase"/> is disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Raised, for the benefit of dependent classes, when lag time is updated.
        /// </summary> 
        internal event Action<double> LagTimeUpdated;

        /// <summary>
        /// Raised, for the benefit of dependent classes, when lead time is updated.
        /// </summary> 
        internal event Action<double> LeadTimeUpdated;

        // Fields
        private FrameQueue m_frameQueue; // Queue of frames to be published
        private Thread m_publicationThread; // Thread that handles frame publication
        private AutoResetEvent m_publicationWaitHandle; // Inter-frame publication wait handle
        private bool m_usePrecisionTimer; // Flag that enables use of precision timer (over just simple thread sleep)
        private bool m_attachedToFrameRateTimer; // Flag that tracks if instance is attached to a frame rate timer
        private Timer m_monitorTimer; // Sample monitor - tracks total number of unpublished frames
        private int m_framesPerSecond; // Frames per second
        private double m_ticksPerFrame; // Ticks per frame
        private double m_lagTime; // Allowed past time deviation tolerance, in seconds
        private double m_leadTime; // Allowed future time deviation tolerance, in seconds
        private long m_timeResolution; // Maximum sorting resolution in ticks
        private int m_processingInterval; // Defines a specific processing interval for data, if desired
        private TimeSeriesFilterFunction m_filterFunction; // Down-sampling method to use if input is at a higher-resolution than output
        private double m_timeOffset; // Half the distance of the time resolution used for index calculation
        private int m_maximumPublicationTimeout; // Maximum publication wait timeout
        private Ticks m_lagTicks; // Current lag time calculated in ticks
        private volatile bool m_enabled; // Enabled state of concentrator
        private long m_startTime; // Start time of concentrator
        private long m_stopTime; // Stop time of concentrator
        private long m_realTimeTicks; // Timestamp of real-time or the most recently received entity
        private bool m_ignoreBadTimestamps; // Determines whether or not to ignore bad timestamps when sorting measurements
        private bool m_allowSortsByArrival; // Determines whether or not to sort incoming measurements with a bad timestamp by arrival
        private bool m_useLocalClockAsRealTime; // Determines whether or not to use local system clock as "real-time"
        private bool m_allowPreemptivePublishing; // Determines whether or not to preemptively publish frame if expected time-series entities arrive
        private bool m_performTimestampReasonabilityCheck; // Determines whether or not to execute timestamp reasonability checks (i.e., lead time validation)
        private bool m_processByCreationTime; // Determines whether or not to publish frames by their CreatedTimestamp
        private int m_expectedEntities; // Expected number of time-series entities to be sorted into a frame
        private long m_receivedEntities; // Total number of time-series entities ever received for sorting
        private long m_processedEntities; // Total number of time-series entities ever successfully sorted
        private long m_discardedEntities; // Total number of discarded time-series entities
        private long m_measurementsSortedByArrival; // Total number of measurements that were sorted by arrival
        private long m_publishedEntities; // Total number of published time-series entities
        private long m_downsampledEntities; // Total number of down-sampled time-series entities
        private long m_missedSortsByTimeout; // Total number of unsorted time-series entities due to timeout waiting for lock
        private long m_waitHandleExpirations; // Total number of wait handle expirations encountered due to delayed precision timer releases
        private long m_framesAheadOfSchedule; // Total number of frames published ahead of schedule
        private long m_publishedFrames; // Total number of published frames
        private long m_totalPublishTime; // Total cumulative frame user function publication time (in ticks) - used to calculate average
        private bool m_trackLatestEntities; // Determines whether or not to track latest time-series entities
        private readonly IDictionary<Guid, ITimeSeriesEntity> m_latestEntities; // Absolute latest received time-series entities
        private ITimeSeriesEntity m_lastDiscardedEntity; // Last time-series entity that was discarded by the concentrator
        private long m_latencyOfLastDiscardedEntity; // Latency of last time-series entity that was discarded by the concentrator
        private bool m_disposed; // Disposed flag detects redundant calls to dispose method

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConcentratorBase"/>.
        /// </summary>
        /// <remarks>
        /// Concentration will not begin until consumer "Starts" concentrator (i.e., calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>).
        /// </remarks>
        protected ConcentratorBase()
        {
            m_usePrecisionTimer = true;
            m_allowSortsByArrival = true;
            m_allowPreemptivePublishing = true;
            m_performTimestampReasonabilityCheck = true;
            m_processingInterval = -1;
            m_filterFunction = LastReceived;
            m_latestEntities = new Dictionary<Guid, ITimeSeriesEntity>();
            m_maximumPublicationTimeout = Timeout.Infinite;

            // Create a new queue for managing real-time frames
            m_frameQueue = new FrameQueue(CreateNewFrame);

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);

            // Create publication wait handle
            m_publicationWaitHandle = new AutoResetEvent(false);

            // Create publication thread
            m_publicationThread = new Thread(PublishFrames);
            m_publicationThread.Start();

            // This timer monitors the total number of unpublished samples every 5 seconds. This is a useful statistic
            // to monitor: if total number of unpublished samples exceed lag time, concentration could be falling behind
            m_monitorTimer = new Timer();
            m_monitorTimer.Interval = 5000;
            m_monitorTimer.AutoReset = true;
            m_monitorTimer.Elapsed += MonitorUnpublishedSamples;
        }

        /// <summary>
        /// Creates a new <see cref="ConcentratorBase"/> from specified parameters.
        /// </summary>
        /// <param name="framesPerSecond">Number of frames to publish per second.</param>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="framesPerSecond"/> must be greater then 0.
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
            : this()
        {
            FramesPerSecond = framesPerSecond;
            LagTime = lagTime;
            LeadTime = leadTime;
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
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to timestamps in the past.</para>
        /// <para>The number of seconds allowed before assuming a timestamp is too old.</para>
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
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to timestamps in the future.</para>
        /// <para>The number of seconds allowed before assuming a timestamp is too advanced.</para>
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
        /// Gets or sets flag to start tracking the absolute latest received time-series entities.
        /// </summary>
        /// <remarks>
        /// Latest received entity will be available via the <see cref="LatestEntities"/> property.
        /// Note that enabling this option will slightly increase the required sorting time.
        /// </remarks>
        public bool TrackLatestEntities
        {
            get
            {
                return m_trackLatestEntities;
            }
            set
            {
                m_trackLatestEntities = value;
            }
        }

        /// <summary>
        /// Gets reference to the collection of absolute latest received time-series entities.
        /// </summary>
        public IList<ITimeSeriesEntity> LatestEntities
        {
            get
            {
                return m_latestEntities.Values.ToList();
            }
        }

        /// <summary>
        /// Gets reference to the last published <see cref="IFrame"/>.
        /// </summary>
        public IFrame LastFrame
        {
            get
            {
                if ((object)m_frameQueue != null)
                {
                    TrackingFrame last = m_frameQueue.Last;

                    if ((object)last != null)
                        return last.SourceFrame;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if precision timer should be used for frame publication.
        /// </summary>
        public bool UsePrecisionTimer
        {
            get
            {
                return m_usePrecisionTimer;
            }
            set
            {
                if (!value && m_processingInterval > 0)
                    throw new InvalidOperationException("A precision timer must be used when a specific processing interval has been defined.");

                if (value && m_processingInterval == 0)
                    throw new InvalidOperationException("A precision timer cannot be used when the processing interval is set to zero, i.e., process data as fast as possible.");

                if (m_usePrecisionTimer != value)
                {
                    m_usePrecisionTimer = value;

                    if (m_usePrecisionTimer)
                    {
                        // Subscribe to frame rate timer, creating it if it doesn't exist
                        AttachToFrameRateTimer(m_framesPerSecond, m_processingInterval);
                    }
                    else
                    {
                        // Unsubscribe from last frame rate timer, if any
                        DetachFromFrameRateTimer(m_framesPerSecond, m_processingInterval);

                        // Make sure to release publication wait handle if it's currently waiting...
                        if (m_publicationWaitHandle != null)
                            m_publicationWaitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        public int FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "Frames per second must be greater than 0");

                if (m_framesPerSecond != value)
                {
                    // Unsubscribe from last frame rate timer, if any
                    DetachFromFrameRateTimer(m_framesPerSecond, m_processingInterval);

                    m_framesPerSecond = value;
                    m_ticksPerFrame = Ticks.PerSecond / (double)m_framesPerSecond;

                    // We calculate the default maximum wait time for frame publication in whole milliseconds per frame plus 20%,
                    // this comes out to be 40 milliseconds at 30 frames per second and 20 milliseconds at 60 frames per second
                    m_maximumPublicationTimeout = (int)Math.Round((m_ticksPerFrame + m_ticksPerFrame * 0.2D) / Ticks.PerMillisecond);

                    if (m_frameQueue != null)
                        m_frameQueue.FramesPerSecond = m_framesPerSecond;

                    if (m_usePrecisionTimer)
                    {
                        // Subscribe to frame rate timer, creating it if it doesn't exist
                        AttachToFrameRateTimer(m_framesPerSecond, m_processingInterval);
                    }
                    else
                    {
                        // Make sure to release publication wait handle if it's currently waiting...
                        if (m_publicationWaitHandle != null)
                            m_publicationWaitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is normally only used when you need the concentrator to send data at faster than real-time speeds,
        /// e.g., faster than the defined <see cref="FramesPerSecond"/>. A use case would be pushing historical data through
        /// the concentrator where you want to sort and publish data as quickly as possible.
        /// </para>
        /// <para>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, e.g.,
        /// a timer interval, over which to process data. A value of -1 means to use the default processing interval, i.e., use
        /// the <see cref="FramesPerSecond"/>, while a value of 0 means to process data as fast as possible.
        /// </para>
        /// <para>
        /// From a real-time perspective the <see cref="ConcentratorBase"/> defines its general processing interval based on
        /// the defined <see cref="FramesPerSecond"/> property. The frames per second property, however, is more than a basic
        /// processing interval since it is used to define the intervals in one second that will become the time sorting
        /// destination "buckets" used by the concentrator irrespective of the data rate of the incoming data. As an example,
        /// if the frames per second of the concentrator is set to 30 and the source data rate is 60fps, then data will be
        /// down-sampled to 30 frames of sorted incoming data but the assigned processing interval will be used to publish the
        /// frames at the specified rate.
        /// </para>
        /// <para>
        /// The implemented functionality of the process interval property will be to respond to values in the following way:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Response</description>
        ///     </listheader>
        ///     <item>
        ///         <term>&lt; 0</term>
        ///         <description>
        ///         In this case the default processing interval has been requested, as a result the <see cref="ProcessByCreationTime"/>
        ///         will be set to <c>false</c> and the concentrator processing interval will be defined based on the currently defined
        ///         <see cref="FramesPerSecond"/> property, e.g., if the frames per second is 30 the processing interval will be 33.33ms.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>0</term>
        ///         <description>
        ///         In this case the processing interval has been defined to process data as fast as possible, as a result the
        ///         <see cref="ProcessByCreationTime"/> property will be set to <c>true</c> and <see cref="UsePrecisionTimer"/> property
        ///         will be set to <c>false</c>. With a processing interval of zero data is expected to flow into the concentrator as quick as
        ///         it can be provided. The <see cref="FramesPerSecond"/> property will still be used to sort data by time into appropriate
        ///         frames, but the concentrator will use the creation time of the frame against the defined lag-time to make sure
        ///         needed data has arrived before publication and frames will be published at the same rate of data arrival.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>&gt; 0</term>
        ///         <description>
        ///         In this case a specific processing interval has been defined for processing data, as a result both the
        ///         <see cref="ProcessByCreationTime"/> and <see cref="UsePrecisionTimer"/> properties will be set to <c>true</c>. With
        ///         a specifically defined processing interval, data is expected to flow into the concentrator at a similar rate. The
        ///         <see cref="FramesPerSecond"/> property will still be used to sort data by time into appropriate frames, but the concentrator
        ///         will use the creation time of the frame against the defined lag-time to make sure needed data has arrived before
        ///         publication and frames will be published on the specified interval. If multiple frames are ready for publication when the
        ///         processing interval executes, then all the ready frames will be published sequentially as quickly as possible.
        ///         </description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public virtual int ProcessingInterval
        {
            get
            {
                return m_processingInterval;
            }
            set
            {
                if (m_processingInterval != value)
                {
                    DetachFromFrameRateTimer(m_framesPerSecond, m_processingInterval);

                    m_processingInterval = value;

                    if (m_processingInterval < -1)
                        m_processingInterval = -1;

                    ProcessByCreationTime = (m_processingInterval > -1);

                    if (m_processingInterval == 0)
                    {
                        m_usePrecisionTimer = false;

                        // Make sure to release publication wait handle if it's currently waiting...
                        if (m_publicationWaitHandle != null)
                            m_publicationWaitHandle.Set();
                    }
                    else if (m_processingInterval > 0)
                    {
                        m_usePrecisionTimer = true;

                        // Subscribe to frame rate timer, creating it if it doesn't exist
                        AttachToFrameRateTimer(m_framesPerSecond, m_processingInterval);
                    }
                    else
                    {
                        if (m_usePrecisionTimer)
                        {
                            // Subscribe to frame rate timer, creating it if it doesn't exist
                            AttachToFrameRateTimer(m_framesPerSecond, m_processingInterval);
                        }
                        else
                        {
                            // Unsubscribe from last frame rate timer, if any
                            DetachFromFrameRateTimer(m_framesPerSecond, m_processingInterval);

                            // Make sure to release publication wait handle if it's currently waiting...
                            if (m_publicationWaitHandle != null)
                                m_publicationWaitHandle.Set();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum frame publication timeout in milliseconds, set to <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The concentrator automatically defines a precision timer to provide the heartbeat for frame publication, however if the system
        /// gets busy the heartbeat signals can be missed. This property defines a maximum wait timeout before reception of the heartbeat
        /// signal to make sure frame publications continue to occur in a timely fashion even when a system is under stress.
        /// </para>
        /// <para>
        /// This property is automatically defined as 2% more than the number of milliseconds per frame when the <see cref="FramesPerSecond"/>
        /// property is set. Users can override this default value to provide custom behavior for this timeout.
        /// </para>
        /// </remarks>
        public int MaximumPublicationTimeout
        {
            get
            {
                return m_maximumPublicationTimeout;
            }
            set
            {
                m_maximumPublicationTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum time resolution, in ticks, to use when sorting time-series entities by timestamps into their proper destination frame.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Desired maximum resolution</term>
        ///         <description>Value to assign</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Seconds</term>
        ///         <description><see cref="Ticks"/>.<see cref="Ticks.PerSecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Milliseconds</term>
        ///         <description><see cref="Ticks"/>.<see cref="Ticks.PerMillisecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Microseconds</term>
        ///         <description><see cref="Ticks"/>.<see cref="Ticks.PerMicrosecond"/></description>
        ///     </item>
        ///     <item>
        ///         <term>100-Nanoseconds</term>
        ///         <description>0</description>
        ///     </item>
        /// </list>
        /// Assigning values less than zero will be set to zero since minimum possible concentrator resolution is one tick (100-nanoseconds). Assigning
        /// values values greater than <see cref="Ticks"/>.<see cref="Ticks.PerSecond"/> will be set to <see cref="Ticks"/>.<see cref="Ticks.PerSecond"/>
        /// since maximum possible concentrator resolution is one second (i.e., 1 frame per second).
        /// </remarks>
        public long TimeResolution
        {
            get
            {
                return m_timeResolution;
            }
            set
            {
                if (value < 0)
                    m_timeResolution = 0;
                if (value > Ticks.PerSecond)
                    m_timeResolution = Ticks.PerSecond;
                else
                    m_timeResolution = value;

                // Calculate half the distance of the time resolution for use as an offset
                m_timeOffset = (m_timeResolution > 1 ? m_timeResolution / 2 : 1);

                // Assign desired time resolution to frame queue
                if ((object)m_frameQueue != null)
                    m_frameQueue.TimeResolution = m_timeResolution;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeSeriesFilterFunction"/> to be used by the concentrator.
        /// </summary>
        /// <remarks>
        /// The down-sampling method determines the algorithm to use if input is being received at a higher-resolution than the defined output.
        /// </remarks>
        public TimeSeriesFilterFunction FilterFunction
        {
            get
            {
                return m_filterFunction;
            }
            set
            {
                m_filterFunction = value ?? LastReceived;
            }
        }

        /// <summary>
        /// Gets the number of ticks per frame.
        /// </summary>
        public double TicksPerFrame
        {
            get
            {
                return m_ticksPerFrame;
            }
        }

        /// <summary>
        /// Gets or sets the expected number of time-series entities to be assigned to a single frame.
        /// </summary>
        public int ExpectedEntities
        {
            get
            {
                return m_expectedEntities;
            }
            set
            {
                m_expectedEntities = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that allows system to preemptively publish frames assuming all <see cref="ExpectedEntities"/> have arrived.
        /// </summary>
        /// <remarks>
        /// In order for this property to used, the <see cref="ExpectedEntities"/> must be defined.
        /// </remarks>
        public bool AllowPreemptivePublishing
        {
            get
            {
                return m_allowPreemptivePublishing;
            }
            set
            {
                m_allowPreemptivePublishing = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp reasonability checks should be performed on incoming
        /// time-series entities (i.e., timestamps are compared to system clock for reasonability using
        /// <see cref="LeadTime"/> tolerance).
        /// </summary>
        /// <remarks>
        /// Setting this value to <c>false</c> will make the concentrator use the latest value received as "real-time"
        /// without validation; this is not recommended in production since time reported by source devices may
        /// be grossly incorrect. For non-production configurations, setting this value to false will allow
        /// concentration of historical data.
        /// </remarks>
        public bool PerformTimestampReasonabilityCheck
        {
            get
            {
                return m_performTimestampReasonabilityCheck;
            }
            set
            {
                m_performTimestampReasonabilityCheck = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if concentrator should publish frames by created time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this value to <c>true</c> will make concentrator use the timestamp of frame creation
        /// for publication. This is useful in scenarios where the concentrator will be receiving very
        /// large volumes of data but not necessarily in real-time, such as, reading values from a file
        /// where you want data to be sorted and processed as fast as possible.
        /// </para>
        /// <para>
        /// Setting this value to <c>true</c> will force <see cref="UseLocalClockAsRealTime"/> to be <c>true</c>
        /// and <see cref="AllowSortsByArrival"/> to be <c>false</c>.
        /// </para>
        /// </remarks>
        public bool ProcessByCreationTime
        {
            get
            {
                return m_processByCreationTime;
            }
            set
            {
                if (!value && m_processingInterval > -1)
                    throw new InvalidOperationException("Processing by creation time cannot be disabled when a processing interval is defined.");

                m_processByCreationTime = value;

                if (m_processByCreationTime)
                {
                    m_useLocalClockAsRealTime = true;
                    m_allowSortsByArrival = false;
                }
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
        /// Gets the UTC time the concentrator was started.
        /// </summary>
        public Ticks StartTime
        {
            get
            {
                return m_startTime;
            }
        }

        /// <summary>
        /// Gets the UTC time the concentrator was stopped.
        /// </summary>
        public Ticks StopTime
        {
            get
            {
                return m_stopTime;
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
                        processingTime = m_stopTime - m_startTime;
                    else
                        processingTime = DateTime.UtcNow.Ticks - m_startTime;
                }

                if (processingTime < 0)
                    processingTime = 0;

                return processingTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if bad timestamps (as determined by measurement's timestamp quality)
        /// should be ignored when sorting measurements.
        /// </summary>
        /// <remarks>
        /// Setting this property to <c>true</c> forces system to use timestamps as-is without checking quality.
        /// If this property is <c>true</c>, it will supersede operation of <see cref="AllowSortsByArrival"/>.
        /// </remarks>
        public bool IgnoreBadTimestamps
        {
            get
            {
                return m_ignoreBadTimestamps;
            }
            set
            {
                m_ignoreBadTimestamps = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps
        /// to be sorted by arrival time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Value defaults to <c>true</c>, so any incoming measurement with a bad timestamp quality will be sorted
        /// according to its arrival time. Setting the property to <c>false</c> will cause all measurements with a
        /// bad timestamp quality to be discarded. This property will only be considered when
        /// <see cref="IgnoreBadTimestamps"/> is <c>false</c>.
        /// </para>
        /// <para>
        /// Value will be forced to <c>false</c> if <see cref="ProcessByCreationTime"/> is <c>true</c>.
        /// </para>
        /// </remarks>
        public bool AllowSortsByArrival
        {
            get
            {
                return m_allowSortsByArrival;
            }
            set
            {
                if (m_processByCreationTime)
                    m_allowSortsByArrival = false;
                else
                    m_allowSortsByArrival = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether or not to use the local clock time as real-time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use your local system clock as real-time only if the time is locally GPS-synchronized,
        /// or if the time-series entities being sorted were not measured relative to a GPS-synchronized clock.
        /// </para>
        /// <para>
        /// If <see cref="ProcessByCreationTime"/> is <c>true</c>, <see cref="UseLocalClockAsRealTime"/> will
        /// always be set to <c>true</c>, even if you try to set it to <c>false</c>.
        /// </para>
        /// </remarks>
        public bool UseLocalClockAsRealTime
        {
            get
            {
                return m_useLocalClockAsRealTime;
            }
            set
            {
                if (m_processByCreationTime)
                    m_useLocalClockAsRealTime = true;
                else
                    m_useLocalClockAsRealTime = value;
            }
        }

        /// <summary>
        /// Gets the the most accurate time value that is available. If <see cref="UseLocalClockAsRealTime"/> = <c>true</c>, then
        /// this function will return <see cref="DateTime.UtcNow"/>. Otherwise, this function will return the timestamp of the
        /// most recent time-series entity, or <see cref="DateTime.UtcNow"/> if no timestamps are within time deviation
        /// tolerances as specified by the <see cref="LeadTime"/> value.
        /// </summary>
        /// <remarks>
        /// Because the measurements being received by remote devices are often measured relative to GPS time, these timestamps
        /// are typically more accurate than the local clock. As a result, we can use the latest received timestamp as the best
        /// local time measurement we have (ignoring transmission delays); but, even these times can be incorrect so we still have
        /// to apply reasonability checks to these times. To do this, we use the local system time and the <see cref="LeadTime"/>
        /// value to validate the latest measured timestamp. If the newest received timestamp gets too old or creeps too far into
        /// the future (both validated + and - against defined lead time property value), we will fall back on local system time.
        /// Note that this creates a dependency on a fairly accurate local clock - the smaller the lead time deviation tolerance,
        /// the better the needed local clock accuracy. For example, a lead time deviation tolerance of a few seconds might only
        /// require keeping the local clock synchronized to an NTP time source; but, a sub-second tolerance would require that
        /// the local clock be very close to GPS time.
        /// </remarks>
        public Ticks RealTime
        {
            get
            {
                // Assume local system clock is the best value we have for real-time when using local clock as real-time.
                if (m_useLocalClockAsRealTime)
                    return DateTime.UtcNow.Ticks;

                if (m_performTimestampReasonabilityCheck)
                {
                    // If the current value for real-time is outside of the time deviation tolerance of the local
                    // clock, then we set real-time to be the current local clock time. Since the lead time typically
                    // defines the tolerated accuracy of the local clock to real-time we will use this value as the
                    // +/- timestamp tolerance to validate if the timestamp is reasonable.
                    long currentTimeTicks = DateTime.UtcNow.Ticks;
                    double distance = (currentTimeTicks - Thread.VolatileRead(ref m_realTimeTicks)) / (double)Ticks.PerSecond;

                    // Set real-time ticks to current ticks if value is outside of tolerances
                    if (distance > m_leadTime || distance < -m_leadTime)
                        Thread.VolatileWrite(ref m_realTimeTicks, currentTimeTicks);
                }

                // Assume latest time-series entity's timestamp is the best value we have for real-time.
                return Thread.VolatileRead(ref m_realTimeTicks);
            }
        }

        /// <summary>
        /// Gets the total number of time-series entities ever requested for sorting.
        /// </summary>
        public long ReceivedEntities
        {
            get
            {
                return Thread.VolatileRead(ref m_receivedEntities);
            }
        }

        /// <summary>
        /// Gets the total number of time-series entities successfully sorted.
        /// </summary>
        public long ProcessedEntities
        {
            get
            {
                return Thread.VolatileRead(ref m_processedEntities);
            }
        }

        /// <summary>
        /// Gets the total number of time-series entities that have been discarded because of old timestamps
        /// (i.e., entities that were outside the time deviation tolerance from base time, past or future).
        /// </summary>
        public long DiscardedEntities
        {
            get
            {
                return Thread.VolatileRead(ref m_discardedEntities);
            }
        }

        /// <summary>
        /// Gets a reference the last <see cref="ITimeSeriesEntity"/> that was discarded by the concentrator.
        /// </summary>
        public ITimeSeriesEntity LastDiscardedEntity
        {
            get
            {
                return m_lastDiscardedEntity;
            }
        }

        /// <summary>
        /// Gets the calculated latency of the last <see cref="ITimeSeriesEntity"/> that was discarded by the concentrator.
        /// </summary>
        public Ticks LatencyOfLastDiscardedEntity
        {
            get
            {
                return Thread.VolatileRead(ref m_latencyOfLastDiscardedEntity);
            }
        }

        /// <summary>
        /// Gets the total number of published time-series entities.
        /// </summary>
        public long PublishedEntities
        {
            get
            {
                return Thread.VolatileRead(ref m_publishedEntities);
            }
        }

        /// <summary>
        /// Gets the total number of published frames.
        /// </summary>
        public long PublishedFrames
        {
            get
            {
                return Thread.VolatileRead(ref m_publishedFrames);
            }
        }

        /// <summary>
        /// Gets the total number of measurements that were sorted by arrival because the measurement reported a bad timestamp quality.
        /// </summary>
        public long MeasurementsSortedByArrival
        {
            get
            {
                return Thread.VolatileRead(ref m_measurementsSortedByArrival);
            }
        }

        /// <summary>
        /// Gets the total number of down-sampled time-series entities processed by the concentrator.
        /// </summary>
        public long DownsampledEntities
        {
            get
            {
                return Thread.VolatileRead(ref m_downsampledEntities);
            }
        }

        /// <summary>
        /// Gets the total number of missed sorts by timeout processed by the concentrator.
        /// </summary>
        public long MissedSortsByTimeout
        {
            get
            {
                return Thread.VolatileRead(ref m_missedSortsByTimeout);
            }
        }

        /// <summary>
        /// Gets the total number of wait handle expirations encountered due to delayed precision timer releases.
        /// </summary>
        public long WaitHandleExpirations
        {
            get
            {
                return Thread.VolatileRead(ref m_waitHandleExpirations);
            }
        }

        /// <summary>
        /// Gets the total number of frames ahead of schedule processed by the concentrator.
        /// </summary>
        public long FramesAheadOfSchedule
        {
            get
            {
                return Thread.VolatileRead(ref m_framesAheadOfSchedule);
            }
        }

        /// <summary>
        /// Gets the total number of seconds frames have spent in the publication process since concentrator started.
        /// </summary>
        public Time TotalPublicationTime
        {
            get
            {
                return ((Ticks)Thread.VolatileRead(ref m_totalPublishTime)).ToSeconds();
            }
        }

        /// <summary>
        /// Gets the average required frame publication time, in seconds.
        /// </summary>
        /// <remarks>
        /// If user publication function, <see cref="ConcentratorBase.PublishFrame"/>, consistently exceeds available publishing time
        /// (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c>), concentration will fall behind.
        /// </remarks>
        public Time AveragePublicationTimePerFrame
        {
            get
            {
                return TotalPublicationTime / PublishedFrames;
            }
        }

        /// <summary>
        /// Gets detailed state of concentrator frame queue.
        /// </summary>
        public string QueueState
        {
            get
            {
                if (m_frameQueue != null)
                    return m_frameQueue.ExamineQueueState(m_expectedEntities);

                return "";
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
                ITimeSeriesEntity lastDiscardedEntity = null;
                DateTime currentTime = DateTime.UtcNow;

                status.AppendFormat("     Data concentration is: {0}", Enabled ? "Enabled" : "Disabled");
                status.AppendLine();
                status.AppendFormat("    Total process run time: {0}", RunTime.ToString());
                status.AppendLine();
                status.AppendFormat("        Maximum wait delay: {0} seconds (lag time)", m_lagTime);
                status.AppendLine();
                status.AppendFormat("     Local clock tolerance: {0} seconds (lead time)", m_leadTime);
                status.AppendLine();
                status.AppendFormat("   Maximum time resolution: {0} ticks", m_timeResolution);
                status.AppendLine();
                status.AppendFormat("      Down-sampling method: {0}", m_filterFunction);
                status.AppendLine();
                status.AppendFormat("    Local clock time (UTC): {0}", currentTime.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                status.AppendLine();
                status.AppendFormat("  Using clock as real-time: {0}", m_useLocalClockAsRealTime);
                status.AppendLine();

                if (!m_useLocalClockAsRealTime)
                {
                    status.Append("      Local clock accuracy: ");
                    status.Append(SecondsFromRealTime(DateTime.UtcNow.Ticks).ToString("0.0000"));
                    status.Append(" second deviation from latest time");
                    status.AppendLine();
                }

                status.AppendFormat("     Ignore bad timestamps: {0}", m_ignoreBadTimestamps);
                status.AppendLine();
                status.AppendFormat("    Allow sorts by arrival: {0}", !m_ignoreBadTimestamps && m_allowSortsByArrival);
                status.AppendLine();
                status.AppendFormat(" Use preemptive publishing: {0}", m_allowPreemptivePublishing);
                status.AppendLine();
                status.AppendFormat("  Time reasonability check: {0}", m_performTimestampReasonabilityCheck ? "Enabled" : "Disabled");
                status.AppendLine();
                status.AppendFormat("  Process by received time: {0}", m_processByCreationTime);
                status.AppendLine();
                status.AppendFormat("         Received entities: {0}", ReceivedEntities);
                status.AppendLine();
                status.AppendFormat("        Processed entities: {0}", ProcessedEntities);
                status.AppendLine();
                status.AppendFormat("        Discarded entities: {0}", DiscardedEntities);
                status.AppendLine();
                status.AppendFormat("     Down-sampled entities: {0}", DownsampledEntities);
                status.AppendLine();
                status.AppendFormat("        Published entities: {0}", PublishedEntities);
                status.AppendLine();
                status.AppendFormat("         Expected entities: {0} ({1} / frame)", PublishedFrames * m_expectedEntities, m_expectedEntities);
                status.AppendLine();
                status.Append("Last discarded entity: ");

                Interlocked.Exchange(ref lastDiscardedEntity, m_lastDiscardedEntity);
                if (lastDiscardedEntity == null)
                {
                    status.Append("<none>");
                }
                else
                {
                    status.Append(lastDiscardedEntity);
                    status.Append(" - ");
                    status.Append(((DateTime)lastDiscardedEntity.Timestamp).ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                    status.AppendLine();
                    status.AppendFormat(" Latency of last discarded: {0} seconds", LatencyOfLastDiscardedEntity.ToSeconds().ToString("0.0000"));
                }
                status.AppendLine();
                status.AppendFormat("  Average publication time: {0} milliseconds", (AveragePublicationTimePerFrame / SI.Milli).ToString("0.0000"));
                status.AppendLine();
                status.AppendFormat("  Pre-lag-time publication: {0}", (FramesAheadOfSchedule / (double)PublishedFrames).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat(" Down-sampling application: {0}", (DownsampledEntities / (double)ProcessedEntities).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat(" User function utilization: {0} of available time used", (1.0D - (m_ticksPerFrame - (double)AveragePublicationTimePerFrame.ToTicks()) / m_ticksPerFrame).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("     Published entity loss: {0}", (DiscardedEntities / (double)ProcessedEntities).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("    Total sorts by arrival: {0}", MeasurementsSortedByArrival);
                status.AppendLine();
                status.AppendFormat(" Measurement time accuracy: {0}", (1.0D - MeasurementsSortedByArrival / (double)ReceivedEntities).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("   Missed sorts by timeout: {0}", MissedSortsByTimeout);
                status.AppendLine();
                status.AppendFormat("      Loss due to timeouts: {0}", (MissedSortsByTimeout / (double)ProcessedEntities).ToString("##0.0000%"));
                status.AppendLine();
                status.AppendFormat("     Using precision timer: {0}", m_usePrecisionTimer);
                status.AppendLine();
                status.AppendFormat("       Wait handle timeout: {0} milliseconds", m_maximumPublicationTimeout);
                status.AppendLine();
                status.AppendFormat("   Wait handle expirations: {0}", WaitHandleExpirations);
                status.AppendLine();
                status.AppendFormat("    Total published frames: {0}", PublishedFrames);
                status.AppendLine();
                status.AppendFormat("        Defined frame rate: {0} frames/sec, {1} ticks/frame", m_framesPerSecond, m_ticksPerFrame.ToString("0.00"));
                status.AppendLine();
                status.AppendFormat(" Estimated mean frame rate: {0} frames/sec", (PublishedFrames / (RunTime - m_lagTime)).ToString("0.00"));
                status.AppendLine();
                status.AppendFormat("       Processing interval: {0}", ProcessingInterval < 0 ? ((Ticks)m_ticksPerFrame).ToMilliseconds().ToString("0.00") + " milliseconds" : (ProcessingInterval == 0 ? "As fast as possible" : ProcessingInterval + " milliseconds"));
                status.AppendLine();

                lock (s_frameRateTimers)
                {
                    Tuple<int, int> key = new Tuple<int, int>(m_framesPerSecond, m_processingInterval);
                    FrameRateTimer timer;

                    if (s_frameRateTimers.TryGetValue(key, out timer))
                    {
                        status.AppendFormat("     Timer reference count: {0} concentrator{1} for the {2}fps @ {3:0.00}ms timer", timer.ReferenceCount, timer.ReferenceCount > 1 ? "s" : "", m_framesPerSecond, ProcessingInterval < 0 ? ((Ticks)m_ticksPerFrame).ToMilliseconds() : (double)ProcessingInterval);
                        status.AppendLine();
                    }

                    status.AppendFormat("   Total frame rate timers: {0}", s_frameRateTimers.Count);
                    status.AppendLine();
                }

                status.AppendFormat("        Queued frame count: {0}", m_frameQueue.Count);
                status.AppendLine();

                return status.ToString();
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
                        // Make sure concentrator is stopped
                        Stop();

                        DetachFromFrameRateTimer(m_framesPerSecond, m_processingInterval);

                        m_publicationThread = null;

                        if (m_publicationWaitHandle != null)
                        {
                            AutoResetEvent publicationWaitHandle = m_publicationWaitHandle;
                            m_publicationWaitHandle = null;
                            publicationWaitHandle.Set();
                            publicationWaitHandle.Dispose();
                        }

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

                        m_lastDiscardedEntity = null;

                        // Clear minimum timer resolution.
                        PrecisionTimer.ClearMinimumTimerResolution(1);
                    }
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
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
                ResetStatistics();

                m_stopTime = 0;
                m_startTime = DateTime.UtcNow.Ticks;
                m_frameQueue.Clear();
                m_monitorTimer.Start();

                // Start real-time frame publication
                m_enabled = true;
            }
        }

        /// <summary>
        /// Stops the concentrator.
        /// </summary>
        public virtual void Stop()
        {
            if (m_enabled)
            {
                m_enabled = false;

                if (m_monitorTimer != null)
                    m_monitorTimer.Stop();

                if (m_frameQueue != null)
                    m_frameQueue.Clear();

                m_stopTime = DateTime.UtcNow.Ticks;
            }
        }

        /// <summary>
        /// Resets the statistics of the concentrator.
        /// </summary>
        public virtual void ResetStatistics()
        {
            m_receivedEntities = 0;
            m_processedEntities = 0;
            m_discardedEntities = 0;
            m_downsampledEntities = 0;
            m_publishedEntities = 0;
            m_measurementsSortedByArrival = 0;
            m_missedSortsByTimeout = 0;
            m_waitHandleExpirations = 0;
            m_framesAheadOfSchedule = 0;
            m_publishedFrames = 0;
            m_totalPublishTime = 0;
            m_lastDiscardedEntity = null;
            m_latencyOfLastDiscardedEntity = 0;
        }

        /// <summary>
        /// Returns the deviation, in seconds, that the given number of ticks is from real-time (i.e., <see cref="ConcentratorBase.RealTime"/>).
        /// </summary>
        /// <param name="timestamp">Timestamp to calculate distance from real-time.</param>
        /// <returns>A <see cref="Double"/> value indicating the deviation, in seconds, from real-time.</returns>
        public double SecondsFromRealTime(Ticks timestamp)
        {
            // Make sure real-time is initialized for initial distance calculation
            if (Thread.VolatileRead(ref m_realTimeTicks) == 0)
            {
                long currentTimeTicks;

                if (m_performTimestampReasonabilityCheck)
                    currentTimeTicks = DateTime.UtcNow.Ticks;
                else
                    currentTimeTicks = timestamp;

                Thread.VolatileWrite(ref m_realTimeTicks, currentTimeTicks);
            }

            return (RealTime - timestamp).ToSeconds();
        }

        /// <summary>
        /// Returns the deviation, in milliseconds, that the given number of ticks is from real-time (i.e., <see cref="ConcentratorBase.RealTime"/>).
        /// </summary>
        /// <param name="timestamp">Timestamp to calculate distance from real-time.</param>
        /// <returns>A <see cref="Double"/> value indicating the deviation in milliseconds.</returns>
        public double MillisecondsFromRealTime(Ticks timestamp)
        {
            return SecondsFromRealTime(timestamp) / SI.Milli;
        }

        /// <summary>
        /// Sorts each <see cref="ITimeSeriesEntity"/> placing each data point in its proper <see cref="IFrame"/>.
        /// </summary>
        /// <param name="entities">Collection of <see cref="ITimeSeriesEntity"/> objects to sort.</param>
        public virtual void SortEntities(IEnumerable<ITimeSeriesEntity> entities)
        {
            if (!m_enabled)
                return;

            // This function is called continually with new time-series entities to handle "time-alignment" (i.e., sorting)
            // of these new entities. Many threads will be waiting for frames of time aligned data so make sure any
            // work to be done here is executed as efficiently as possible.

            // Note that breaking up this function into several parts might help with readability and make it
            // easier to maintain, but to reduce function calls (and hence save time) the decision was made to
            // put the code into one larger more complex function...

            TrackingFrame frame = null;
            ITimeSeriesEntity latestEntity;

            Ticks timestamp = 0, lastTimestamp = 0;
            double distance;
            bool discardEntity;

            List<ITimeSeriesEntity> discardedEntities = null;

            // Track the total number of time-series entities ever received for sorting.
            Interlocked.Add(ref m_receivedEntities, entities.Count());

            // Time-series entities usually come in groups. This function processes all available entities
            // in the collection here directly as an optimization which avoids the overhead of a function
            // call for each entity.
            foreach (ITimeSeriesEntity entity in entities)
            {
                // Reset flag for next entity.
                discardEntity = false;

                if (m_trackLatestEntities)
                {
                    // If this entity is more recent than the latest entity for the same signal, make this entity the latest entity
                    if (!m_latestEntities.TryGetValue(entity.ID, out latestEntity) || entity.Timestamp > latestEntity.Timestamp)
                        m_latestEntities[entity.ID] = entity;
                }

                // Attempt to cast the entity as a measurement
                IMeasurement measurement = entity as IMeasurement;

                // Check for a bad measurement timestamp.
                if (!m_ignoreBadTimestamps && (object)measurement != null && !measurement.TimestampQualityIsGood())
                {
                    if (m_allowSortsByArrival)
                    {
                        // Device reports measurement timestamp as bad; this typically means that the GPS timestamp of the
                        // source device is not accurate. If the concentrator is set to allow sorts by arrival then it is
                        // assumed that our local real-time value is better than the device measurement, so we sort the
                        // measurement by arrival time.
                        timestamp = RealTime;
                        Interlocked.Increment(ref m_measurementsSortedByArrival);
                    }
                    else
                    {
                        // If sorting by arrival time is not allowed,
                        // measurements with bad timestamps are discarded.
                        discardEntity = true;
                    }
                }
                else
                {
                    // Timestamp quality is good, get ticks for this entity.
                    timestamp = entity.Timestamp;
                }

                if (!discardEntity)
                {
                    //
                    // *** Sort the time-series entity into the proper frame ***
                    //

                    // Get the destination frame for the time-series entity. Note that groups of parsed entities will
                    // typically be coming in from the same source and will have the same ticks. If we have already
                    // found the destination frame for the same ticks, then there is no need to lookup frame again.
                    if ((object)frame == null || timestamp != lastTimestamp)
                    {
                        if (m_processByCreationTime)
                        {
                            // If sorting by creation timestamp, simply get a frame for this entity since
                            // this is used in scenarios where very large volumes of data need concentration
                            frame = m_frameQueue.GetFrame(timestamp);
                            lastTimestamp = timestamp;
                        }
                        else
                        {
                            // Badly time-aligned entities, or those coming in at a higher sample rate, may fall
                            // outside available frame buckets. To check for this, the difference between the entity's
                            // timestamp and real-time in seconds is calculated and validated between lag and lead times.
                            distance = SecondsFromRealTime(timestamp);

                            if (distance > m_lagTime || (m_performTimestampReasonabilityCheck && distance < -m_leadTime))
                            {
                                // This data has come in late or has a future timestamp.  For old timestamps, we're not
                                // going to create a frame for data that will never be processed.  For future dates we
                                // must assume that the clock from source device must be advanced and out-of-sync with
                                // real-time - either way this data will be discarded.
                                frame = null;
                            }
                            else
                            {
                                // Get a frame for this time-series entity
                                frame = m_frameQueue.GetFrame(timestamp);
                                lastTimestamp = timestamp;
                            }
                        }
                    }

                    if ((object)frame == null)
                    {
                        // Time-series entity is discarded if no bucket (i.e., destination frame) was found for it.
                        discardEntity = true;
                        lastTimestamp = 0;
                    }
                    else if (frame.Add(entity))
                    {
                        // Track the total number of time-series entities successfully requested for sorting.
                        Interlocked.Increment(ref m_processedEntities);
                    }
                    else
                    {
                        // Track the total number of time-series entities that
                        // failed to sort because the system ran out of time.
                        Interlocked.Increment(ref m_missedSortsByTimeout);

                        // Count this as a discarded entity if it was never assigned to the frame.
                        discardEntity = true;
                    }
                }

                if (discardEntity)
                {
                    // This time-series entity was marked to be discarded.
                    Interlocked.Exchange(ref m_lastDiscardedEntity, entity);
                    Interlocked.Exchange(ref m_latencyOfLastDiscardedEntity, RealTime - m_lastDiscardedEntity.Timestamp);

                    // Track total number of discarded entities
                    Interlocked.Increment(ref m_discardedEntities);

                    // Make sure discarded entity collection exists
                    if (discardedEntities == null)
                        discardedEntities = new List<ITimeSeriesEntity>();

                    // Add discarded entity to local collection
                    discardedEntities.Add(entity);
                }
                else
                {
                    //
                    // *** Manage "real-time" ticks ***
                    //

                    if (!m_useLocalClockAsRealTime)
                    {
                        // Algorithm:
                        //      If the time-series entity's timestamp is newer than the current real-time value,
                        //      and it is within the specified time deviation tolerance of the local clock time,
                        //      then the entity's timestamp is set as real-time.
                        if (timestamp > Thread.VolatileRead(ref m_realTimeTicks))
                        {
                            if (m_performTimestampReasonabilityCheck)
                            {
                                // Apply a reasonability check to this value using the local clock. Since the lead time
                                // typically defines the tolerated accuracy of the local clock to real-time, this value
                                // is used as the + and - timestamp tolerance to validate if the time is reasonable.
                                long currentTimeTicks = DateTime.UtcNow.Ticks;

                                if (timestamp.TimeIsValid(currentTimeTicks, m_leadTime, m_leadTime))
                                {
                                    // The new time timestamp looks good, so this function assumes the time is "real-time"
                                    Thread.VolatileWrite(ref m_realTimeTicks, timestamp);
                                }
                                else
                                {
                                    // Entity's timestamp was outside of time deviation tolerances so we'll also check to make
                                    // sure current real-time ticks are within these tolerances as well
                                    distance = (currentTimeTicks - Thread.VolatileRead(ref m_realTimeTicks)) / (double)Ticks.PerSecond;

                                    if (distance > m_leadTime || distance < -m_leadTime)
                                    {
                                        // New time was invalid, as was current real-time value, so we have no choice but to
                                        // assume the current time as "real-time", so we set real-time ticks to current ticks
                                        Thread.VolatileWrite(ref m_realTimeTicks, currentTimeTicks);
                                    }
                                }
                            }
                            else
                            {
                                // Reasonability checks are disabled, assume newest time is real-time...
                                Thread.VolatileWrite(ref m_realTimeTicks, timestamp);
                            }
                        }
                    }
                }
            }

            // Provide discarded entities to consumers, if any
            if ((object)discardedEntities != null)
                OnDiscardedEntities(discardedEntities);
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="ITimeSeriesEntity"/> objects that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of time-series entities with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        /// <remarks>
        /// If user implemented publication function consistently exceeds available publishing time (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds),
        /// concentration will fall behind. A small amount of this time is required by the <see cref="ConcentratorBase"/> for processing overhead, so actual total time
        /// available for user function process will always be slightly less than <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds.
        /// </remarks>
        protected abstract void PublishFrame(IFrame frame, int index);

        /// <summary>
        /// Creates a new <see cref="IFrame"/> for the given <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">Timestamp for new <see cref="IFrame"/> in <see cref="Ticks"/>.</param>
        /// <returns>New <see cref="IFrame"/> at given <paramref name="timestamp"/>.</returns>
        /// <remarks>
        /// Derived classes can override this method to create a new custom <see cref="IFrame"/>.
        /// Default behavior creates a basic <see cref="Frame"/> to hold synchronized time-series entities.
        /// </remarks>
        protected internal virtual IFrame CreateNewFrame(Ticks timestamp)
        {
            return new Frame(timestamp, m_expectedEntities);
        }

        /// <summary>
        /// Assigns <see cref="ITimeSeriesEntity"/> to its associated <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes can choose to override this method to handle custom assignment of a <see cref="ITimeSeriesEntity"/> to
        /// its <see cref="IFrame"/>. Default behavior simply assigns entity to frame's keyed entity dictionary:
        /// <code>frame.Entities[entity.ID] = entity;</code>
        /// </remarks>
        /// <param name="frame">The <see cref="IFrame"/> that is used.</param>
        /// <param name="entity">The <see cref="ITimeSeriesEntity"/> to be assigned to the frame."/></param>
        protected virtual void AssignEntityToFrame(IFrame frame, ITimeSeriesEntity entity)
        {
            frame.Entities[entity.ID] = entity;
        }

        /// <summary>
        /// Raises the <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ProcessException"/> event.</param>
        /// <remarks>
        /// Allows derived classes to raise a processing exception.
        /// </remarks>
        protected virtual void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="seconds">Total number of unpublished seconds of data.</param>
        protected virtual void OnUnpublishedSamples(int seconds)
        {
            if ((object)UnpublishedSamples != null)
                UnpublishedSamples(this, new EventArgs<int>(seconds));
        }

        /// <summary>
        /// Raises the <see cref="DiscardedEntities"/> event.
        /// </summary>
        /// <param name="entities">Enumeration of <see cref="ITimeSeriesEntity"/> objects being discarded.</param>
        /// <remarks>
        /// Allows derived classes to raise an entities discarded event.
        /// </remarks>
        protected virtual void OnDiscardedEntities(IEnumerable<ITimeSeriesEntity> entities)
        {
            if ((object)EntitiesDiscarded != null)
                EntitiesDiscarded(this, new EventArgs<IEnumerable<ITimeSeriesEntity>>(entities));
        }

        // Tick handler for frame rate timer simply signals waiting thread to publish
        private void StartFramePublication(object sender, EventArgs e)
        {
            if (m_enabled)
                m_publicationWaitHandle.Set();
        }

        // Frame publication handler
        private void PublishFrames()
        {
            TrackingFrame frame;
            IFrame sourceFrame;
            Ticks timestamp;
            long startTime, stopTime;
            int frameIndex;

            int downsampledEntities;

            // Keep thread alive...
            while (m_publicationWaitHandle != null)
            {
                // Keep publishing frames so long as they are ready for publication. This handles case where
                // system may be falling behind because user function is taking too long - exit when no
                // other frames are available to process
                while (m_enabled)
                {
                    try
                    {
                        // Get top frame
                        frame = m_frameQueue.Head;
                        downsampledEntities = 0;

                        // If no frame is ready to publish, exit
                        if (frame == null)
                            break;

                        // Get ticks for this frame
                        sourceFrame = frame.SourceFrame;
                        timestamp = sourceFrame.Timestamp;

                        if (m_processByCreationTime)
                        {
                            // When processing by received timestamp, we need to test received timestamp against lag-time
                            // to make sure there has been time enough to publish frame:
                            if (m_lagTicks - (RealTime - frame.CreatedTimestamp) > 0)
                                break;
                        }
                        else
                        {
                            // See if any lag-time needs to pass before we begin publishing, exiting if it's not time to publish
                            if (m_lagTicks - (RealTime - timestamp) > 0)
                            {
                                // It's not the scheduled time to publish this frame, however, if preemptive publishing is enabled,
                                // an expected number of time-series entities per-frame have been defined and the frame has received
                                // this expected number of entities, we can go ahead and publish the frame ahead of schedule. This
                                // is useful if the lag time is high to ensure no data is missed but it's desirable to publish the
                                // frame as soon as the expected data has arrived.
                                if (m_expectedEntities < 1 || !m_allowPreemptivePublishing || frame.SignalCount < m_expectedEntities)
                                    break;

                                // All data has been received for this frame, so we'll go ahead and publish ahead-of-schedule
                                Interlocked.Increment(ref m_framesAheadOfSchedule);
                            }
                        }

                        // Mark start time for publication
                        startTime = DateTime.UtcNow.Ticks;

                        // Calculate index of this frame within its second - note that we have to calculate this
                        // value instead of using frameIndex since it is is possible for multiple frames to be
                        // published within one frame period if the system is stressed
                        frameIndex = (int)(((double)timestamp.DistanceBeyondSecond() + m_timeOffset) / m_ticksPerFrame);

                        foreach (IList<ITimeSeriesEntity> signal in frame.GetEntities().Values)
                        {
                            downsampledEntities += signal.Count - 1;
                            AssignEntityToFrame(sourceFrame, m_filterFunction(sourceFrame.Timestamp, signal));
                        }

                        try
                        {
                            // Publish the current frame (i.e., call user implemented publication function)
                            PublishFrame(sourceFrame, frameIndex);
                        }
                        finally
                        {
                            // Remove the frame from the queue whether it is successfully published or not
                            m_frameQueue.Pop();

                            // Update publication statistics
                            Interlocked.Increment(ref m_publishedFrames);
                            Interlocked.Add(ref m_publishedEntities, frame.SignalCount);
                            Interlocked.Add(ref m_downsampledEntities, downsampledEntities);

                            // Mark stop time for publication
                            stopTime = DateTime.UtcNow.Ticks;

                            // Track total publication time
                            Interlocked.Add(ref m_totalPublishTime, stopTime - startTime);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Not stopping for exceptions - but we'll let user know there are issues...
                        OnProcessException(ex);
                        break;
                    }
                }

                // Wait for next publication signal, timing out if signal takes too long
                if (m_usePrecisionTimer && m_publicationWaitHandle != null && !m_publicationWaitHandle.WaitOne(m_maximumPublicationTimeout))
                    m_waitHandleExpirations++;
                else
                    Thread.Sleep(1);
            }
        }

        // Exposes the number of unpublished seconds of data in the queue (note that first second of data will always be "publishing").
        private void MonitorUnpublishedSamples(object sender, ElapsedEventArgs e)
        {
            int secondsOfData = (m_frameQueue.Count / m_framesPerSecond) - 1;

            if (secondsOfData < 0)
                secondsOfData = 0;

            OnUnpublishedSamples(secondsOfData);
        }

        // Handle attach to frame rate timer
        private void AttachToFrameRateTimer(int framesPerSecond, int processingInterval)
        {
            Tuple<int, int> key = new Tuple<int, int>(framesPerSecond, processingInterval);

            lock (s_frameRateTimers)
            {
                if (!m_attachedToFrameRateTimer)
                {
                    FrameRateTimer timer;

                    // Get static frame rate timer for given frames per second creating it if needed
                    if (!s_frameRateTimers.TryGetValue(key, out timer))
                    {
                        // Create a new frame rate timer which includes a high-precision timer for frame processing
                        timer = new FrameRateTimer(framesPerSecond, processingInterval);

                        // Add timer state for given rate to static collection
                        s_frameRateTimers.Add(key, timer);
                    }

                    // Increment reference count and attach instance method "StartFramePublication" to static timer event list
                    timer.AddReference(StartFramePublication);
                    m_attachedToFrameRateTimer = true;
                }
            }
        }

        // Handle detach from frame rate timer
        private void DetachFromFrameRateTimer(int framesPerSecond, int processingInterval)
        {
            Tuple<int, int> key = new Tuple<int, int>(framesPerSecond, processingInterval);

            lock (s_frameRateTimers)
            {
                if (m_attachedToFrameRateTimer)
                {
                    FrameRateTimer timer;

                    // Look up static frame rate timer for given frames per second
                    if (s_frameRateTimers.TryGetValue(key, out timer))
                    {
                        // Decrement reference count and detach instance method "StartFramePublication" from static timer event list
                        timer.RemoveReference(StartFramePublication);
                        m_attachedToFrameRateTimer = false;

                        // If timer is no longer being referenced we stop it and remove it from static collection
                        if (timer.ReferenceCount == 0)
                        {
                            timer.Dispose();
                            s_frameRateTimers.Remove(key);
                        }
                    }
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Dictionary<Tuple<int, int>, FrameRateTimer> s_frameRateTimers;

        // Static Constructor
        static ConcentratorBase()
        {
            s_frameRateTimers = new Dictionary<Tuple<int, int>, FrameRateTimer>();
        }

        // Static Methods

        /// <summary>
        /// Returns the most recently received time-series entity in the given collection of time-series entities.
        /// </summary>
        /// <param name="targetFrameTimestamp">The timestamp of the destination frame.</param>
        /// <param name="entities">Sequence of <see cref="ITimeSeriesEntity"/> objects over which to apply this filter.</param>
        /// <returns>The last entity in the given collection.</returns>
        public static ITimeSeriesEntity LastReceived(long targetFrameTimestamp, IEnumerable<ITimeSeriesEntity> entities)
        {
            return entities.Last();
        }

        #endregion
    }
}
