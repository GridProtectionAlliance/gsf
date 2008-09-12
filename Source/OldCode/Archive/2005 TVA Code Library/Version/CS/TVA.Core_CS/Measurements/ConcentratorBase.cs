using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
using TVA.Collections;
//using TVA.Collections.Common;
using TVA.DateTime;
//using TVA.DateTime.Common;
//using TVA.Math.Common;
using TVA.Threading;

//*******************************************************************************************************
//  TVA.Measurements.ConcentratorBase.vb - Measurement concentrator base class
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: jrcarrol@tva.gov
//
//  This class synchronizes (i.e., sorts by timestamp) real-time measurements
//
//  Note that your lag time should be defined as it relates to the rate at which data data is coming
//  into the concentrator. Make sure you allow enough time for transmission of data over the network
//  allowing any needed time for possible network congestion.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated initial version of source for Super Phasor Data Concentrator.
//  02/23/2006 - J. Ritchie Carroll
//       Abstracted classes for general use, and added to TVA code library.
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
//
//*******************************************************************************************************


namespace TVA
{
    namespace Measurements
    {

        public abstract class ConcentratorBase : IDisposable
        {



            #region " Public Member Declarations "

            /// <summary>This event is raised every second allowing consumer to track current number of unpublished
            /// seconds of data in the queue.</summary>
            public delegate void UnpublishedSamplesEventHandler(int total);
            private UnpublishedSamplesEventHandler UnpublishedSamplesEvent;

            public event UnpublishedSamplesEventHandler UnpublishedSamples
            {
                add
                {
                    UnpublishedSamplesEvent = (UnpublishedSamplesEventHandler)System.Delegate.Combine(UnpublishedSamplesEvent, value);
                }
                remove
                {
                    UnpublishedSamplesEvent = (UnpublishedSamplesEventHandler)System.Delegate.Remove(UnpublishedSamplesEvent, value);
                }
            }


            /// <summary>This event is raised if there is an exception encountered while attempting to process a
            /// frame in the sample queue.</summary>
            /// <remarks>Processing will not stop for any exceptions thrown by the user function, but any captured
            /// exceptions will be exposed through this event.</remarks>
            public delegate void ProcessExceptionEventHandler(Exception ex);
            private ProcessExceptionEventHandler ProcessExceptionEvent;

            public event ProcessExceptionEventHandler ProcessException
            {
                add
                {
                    ProcessExceptionEvent = (ProcessExceptionEventHandler)System.Delegate.Combine(ProcessExceptionEvent, value);
                }
                remove
                {
                    ProcessExceptionEvent = (ProcessExceptionEventHandler)System.Delegate.Remove(ProcessExceptionEvent, value);
                }
            }


            #endregion

            #region " Private Member Declarations "

            internal delegate void LeadTimeUpdatedEventHandler(double leadTime); // Raised, for the benefit of dependent classes, when lead time is updated
            private LeadTimeUpdatedEventHandler LeadTimeUpdatedEvent;

            internal event LeadTimeUpdatedEventHandler LeadTimeUpdated
            {
                add
                {
                    LeadTimeUpdatedEvent = (LeadTimeUpdatedEventHandler)System.Delegate.Combine(LeadTimeUpdatedEvent, value);
                }
                remove
                {
                    LeadTimeUpdatedEvent = (LeadTimeUpdatedEventHandler)System.Delegate.Remove(LeadTimeUpdatedEvent, value);
                }
            }

            internal delegate void LagTimeUpdatedEventHandler(double lagTime); // Raised, for the benefit of dependent classes, when lag time is updated
            private LagTimeUpdatedEventHandler LagTimeUpdatedEvent;

            internal event LagTimeUpdatedEventHandler LagTimeUpdated
            {
                add
                {
                    LagTimeUpdatedEvent = (LagTimeUpdatedEventHandler)System.Delegate.Combine(LagTimeUpdatedEvent, value);
                }
                remove
                {
                    LagTimeUpdatedEvent = (LagTimeUpdatedEventHandler)System.Delegate.Remove(LagTimeUpdatedEvent, value);
                }
            }


            private FrameQueue m_frameQueue; // Queue of frames to be published
#if ThreadTracking
            private ManagedThread m_publicationThread; // Managed Real-time thread that handles frame publication
#else
			private Thread m_publicationThread; // Real-time thread that handles frame publication
#endif
            private int m_framesPerSecond; // Frames per second
            private decimal m_ticksPerFrame; // Frame rate - we use a 64-bit scaled integer to avoid round-off errors in calculations
            private double m_lagTime; // Allowed past time deviation tolerance
            private double m_leadTime; // Allowed future time deviation tolerance
            private long m_lagTicks; // Current lag time in ticks
            private bool m_enabled; // Enabled state of concentrator
            private long m_startTime; // Start time of concentrator
            private long m_stopTime; // Stop time of concentrator
            private long m_realTimeTicks; // Ticks of the most recently received measurement
            private bool m_allowSortsByArrival; // Determines whether or not to sort incoming measurements with a bad timestamp by arrival
            private bool m_useLocalClockAsRealTime; // Determines whether or not to use local system clock as "real-time"
            private long m_totalMeasurements; // Total number of measurements ever requested for sorting
            private long m_measurementsSortedByArrival; // Total number of measurements that were sorted by arrival
            private long m_discardedMeasurements; // Total number of discarded measurements
            private long m_publishedMeasurements; // Total number of published measurements
            private long m_missedSortsByTimeout; // Total number of unsorted measurements due to timeout waiting for lock
            private long m_publishedFrames; // Total number of published frames
            private long m_totalWaitTime; // Total cumulative frame wait time (till publish) - used to calculate average
            private long m_totalSortTime; // Total cumulative frame sorting times (in ticks) - used to calculate average
            private bool m_trackLatestMeasurements; // Determines whether or not to track latest measurements
            private ImmediateMeasurements m_latestMeasurements; // Absolute latest received measurement values
            private IMeasurement m_lastDiscardedMeasurement; // Last measurement that was discarded by the concentrator
            private bool m_disposed; // Disposed flag detects redundant calls to dispose method
            private System.Timers.Timer m_monitorTimer; // Sample monitor

            #endregion

            #region " Construction Functions "

            /// <summary>Creates a new measurement concentrator.</summary>
            /// <param name="framesPerSecond">Number of frames to publish per second.</param>
            /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of
            /// time to wait before publishing begins.</param>
            /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the
            /// tolerated +/- accuracy of the local clock to real-time.</param>
            /// <remarks>
            /// <para>framesPerSecond must be at least one.</para>
            /// <para>lagTime must be greater than zero, but can be specified in sub-second intervals (e.g., set
            /// to .25 for a quarter-second lag time). Note that this defines time sensitivity to past timestamps.</para>
            /// <para>leadTime must be greater than zero, but can be specified in sub-second intervals (e.g., set
            /// to .5 for a half-second lead time). Note that this defines time sensitivity to future timestamps.</para>
            /// <para>Note that concentration will not begin until consumer "Starts" concentrator (i.e., calling
            /// Start method or setting Enabled = True).</para>
            /// </remarks>
            /// <exception cref="ArgumentOutOfRangeException">Specified argument is outside of allowed value range
            /// (see remarks).</exception>
            protected ConcentratorBase(int framesPerSecond, double lagTime, double leadTime)
            {

                if (framesPerSecond < 1)
                {
                    throw (new ArgumentOutOfRangeException("framesPerSecond", "framesPerSecond must be at least one"));
                }
                if (lagTime <= 0)
                {
                    throw (new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one"));
                }
                if (leadTime <= 0)
                {
                    throw (new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one"));
                }

                m_framesPerSecond = framesPerSecond;
                m_ticksPerFrame = (System.Convert.ToDecimal(TVA.DateTime.Common.TicksPerSecond)) / m_framesPerSecond;
                m_realTimeTicks = DateTime.UtcNow.Ticks;
                m_allowSortsByArrival = true;
                m_lagTime = lagTime;
                m_leadTime = leadTime;
                m_latestMeasurements = new ImmediateMeasurements(this);
                m_monitorTimer = new System.Timers.Timer();
                m_monitorTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_monitorTimer_Elapsed);

                // Creates a new queue for managing real-time frames
                m_frameQueue = new FrameQueue(m_ticksPerFrame, (1 + m_lagTime + m_leadTime) * framesPerSecond, new TVA.Measurements.FrameQueue.CreateNewFrameFunctionSignature(CreateNewFrame));

                // Monitors the total number of unpublished samples every second. This is a useful statistic to
                // monitor, if total number of unpublished samples exceed lag time, measurement concentration could
                // be falling behind.
                m_monitorTimer.Interval = 1000;
                m_monitorTimer.AutoReset = true;
                m_monitorTimer.Enabled = false;

            }

            #endregion

            #region " Public Methods Implementation "

            /// <summary>Reference to this concentrator instance.</summary>
            public ConcentratorBase This
            {
                get
                {
                    return this;
                }
            }

            /// <summary>Gets or sets the allowed past time deviation tolerance, in seconds (can be subsecond).</summary>
            /// <remarks>
            /// <para>Defines the time sensitivity to past measurement timestamps.</para>
            /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
            /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow
            /// into the system.</para>
            /// </remarks>
            /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less
            /// than one.</exception>
            public double LagTime
            {
                get
                {
                    return m_lagTime;
                }
                set
                {
                    if (value <= 0)
                    {
                        throw (new ArgumentOutOfRangeException("value", "LagTime must be greater than zero, but it can be less than one"));
                    }
                    m_lagTime = value;
                    m_lagTicks = Convert.ToInt64(m_lagTime) * TVA.DateTime.Common.TicksPerSecond;
                    if (LagTimeUpdatedEvent != null)
                        LagTimeUpdatedEvent(m_lagTime);
                }
            }

            /// <summary>Gets or sets the allowed future time deviation tolerance, in seconds (can be subsecond).</summary>
            /// <remarks>
            /// <para>Defines the time sensitivity to future measurement timestamps.</para>
            /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
            /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
            /// </remarks>
            /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less
            /// than one.</exception>
            public double LeadTime
            {
                get
                {
                    return m_leadTime;
                }
                set
                {
                    if (value <= 0)
                    {
                        throw (new ArgumentOutOfRangeException("value", "LeadTime must be greater than zero, but it can be less than one"));
                    }
                    m_leadTime = value;
                    if (LeadTimeUpdatedEvent != null)
                        LeadTimeUpdatedEvent(m_leadTime);
                }
            }

            /// <summary>Gets or sets the absolute latest received measurement values.</summary>
            /// <remarks>Increases the required sorting time.</remarks>
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

            /// <summary>Gets the absolute latest received measurement values.</summary>
            public ImmediateMeasurements LatestMeasurements
            {
                get
                {
                    return m_latestMeasurements;
                }
            }

            /// <summary>Gets the current publishing frame.</summary>
            /// <remarks>This value may be null right after frame has published.</remarks>
            public IFrame CurrentFrame
            {
                get
                {
                    return m_frameQueue.Head;
                }
            }

            /// <summary>Gets the last published frame.</summary>
            public IFrame LastFrame
            {
                get
                {
                    return m_frameQueue.Last;
                }
            }

            /// <summary>Gets or sets the number of frames per second.</summary>
            public int FramesPerSecond
            {
                get
                {
                    return m_framesPerSecond;
                }
                set
                {
                    m_framesPerSecond = value;
                    m_ticksPerFrame = (System.Convert.ToDecimal(TVA.DateTime.Common.TicksPerSecond)) / m_framesPerSecond;
                    m_frameQueue.TicksPerFrame = m_ticksPerFrame;
                }
            }

            /// <summary>Gets the number of ticks per frame.</summary>
            public decimal TicksPerFrame
            {
                get
                {
                    return m_ticksPerFrame;
                }
            }

            /// <summary>Gets or sets the current enabled state of concentrator.</summary>
            /// <returns>Current enabled state of concentrator</returns>
            /// <remarks>Concentrator must be started (e.g., call Start method or set Enabled = True) before
            /// concentration will begin.</remarks>
            public virtual bool Enabled
            {
                get
                {
                    return m_enabled;
                }
                set
                {
                    if (value)
                    {
                        Start();
                    }
                    else
                    {
                        @Stop();
                    }
                }
            }

            /// <summary>Starts the concentrator, if it is not already running.</summary>
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
                    m_totalSortTime = 0;
                    m_totalWaitTime = 0;
                    m_stopTime = 0;
                    m_startTime = DateTime.UtcNow.Ticks;

                    // Start real-time frame publication thread
#if ThreadTracking
                    m_publicationThread = new ManagedThread(PublishFrames);
                    m_publicationThread.Name = "TVA.Measurements.ConcentratorBase.PublishFrames() [" + this.GetType().Name + "]";
#else
					m_publicationThread = new Thread(new System.Threading.ThreadStart(PublishFrames));
#endif
                    m_publicationThread.Priority = ThreadPriority.Highest;
                    m_publicationThread.Start();
                    m_monitorTimer.Start();
                    m_frameQueue.Start();
                }

                m_enabled = true;

            }

            /// <summary>Stops the concentrator.</summary>
            public virtual void @Stop()
            {

                if (m_enabled)
                {
                    m_frameQueue.Stop();
                    if (m_publicationThread != null)
                    {
                        m_publicationThread.Abort();
                    }
                    m_publicationThread = null;
                    m_monitorTimer.Stop();
                }

                m_enabled = false;
                m_stopTime = DateTime.UtcNow.Ticks;

            }

            /// <summary>
            /// Gets the total amount of time, in seconds, that the concentrator has been active.
            /// </summary>
            public virtual double RunTime
            {
                get
                {
                    long processingTime;

                    if (m_startTime > 0)
                    {
                        if (m_stopTime > 0)
                        {
                            processingTime = m_stopTime - m_startTime;
                        }
                        else
                        {
                            processingTime = DateTime.UtcNow.Ticks - m_startTime;
                        }
                    }

                    if (processingTime < 0)
                    {
                        processingTime = 0;
                    }

                    return TVA.DateTime.Common.TicksToSeconds(processingTime);
                }
            }

            /// <summary>Determines whether or not to allow incoming measurements with bad timestamps to be sorted
            /// by arrival time.</summary>
            /// <remarks>
            /// Defaults to True, so that any incoming measurement with a bad timestamp quality
            /// will be sorted according to its arrival time. Setting the property to False will cause all
            /// measurements with a bad timestamp quality to be discarded.
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

            /// <summary>Determines whether or not to use the local clock time as real time.</summary>
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
            /// If using local clock as real time, this function will return UtcNow.Ticks. Otherwise, this function
            /// will return ticks of most recent measurement, or local clock ticks if no measurements are within
            /// time deviation tolerances.
            /// </summary>
            /// <remarks>
            /// Because the measurements being received by remote devices are often measured relative to GPS time,
            /// these timestamps are typically more accurate than the local clock. As a result, we can use the
            /// latest received timestamp as the best local time measurement we have (this method ignores
            /// transmission delays); but, even these times can be incorrect so we still have to apply reasonability
            /// checks to these times. To do this, we use the local time and the lead time value to validate the
            /// latest measured timestamp. If the newest received measurement timestamp gets too old or creeps too
            /// far into the future (both validated + and - against defined lead time property value), we will fall
            /// back on local system time. Note that this creates a dependency on a fairly accurate local clock - the
            /// smaller the lead time deviation tolerance, the better the needed local clock acuracy. For example, a
            /// lead time deviation tolerance of a few seconds might only require keeping the local clock
            /// synchronized to an NTP time source; but, a sub-second tolerance would require that the local clock be
            /// very close to GPS time.
            /// </remarks>
            public long RealTimeTicks
            {
                get
                {
                    if (m_useLocalClockAsRealTime)
                    {
                        // Assumes local system clock is the best value we have for real time.
                        return DateTime.UtcNow.Ticks;
                    }
                    else
                    {
                        // If the current value for real-time is outside of the time deviation tolerance of the local
                        // clock, then we set latest measurement time (i.e., real-time) to be the current local clock
                        // time. Because of the frequency with which this function gets called, we do not call the
                        // TimeIsValid nor the DistanceFromRealTime functions to determine if the real-time ticks are
                        // valid. Instead, we manually implement the code here to avoid function call overhead. Since
                        // the lead time typically defines the tolerated accuracy of the local clock to real-time
                        // we will use this value as the + and - timestamp tolerance to validate if the measurement
                        // time is reasonable.
                        long currentTimeTicks = DateTime.UtcNow.Ticks;
                        long currentRealTimeTicks = m_realTimeTicks;
                        double distance = (currentTimeTicks - currentRealTimeTicks) / TVA.DateTime.Common.TicksPerSecond;

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

            /// <summary>Returns the deviation in seconds that the given number of ticks is from real time.</summary>
            public double DistanceFromRealTime(long ticks)
            {

                return (RealTimeTicks - ticks) / TVA.DateTime.Common.TicksPerSecond;

            }

            /// <summary>Gets the total number of measurements that have ever been requested for sorting.</summary>
            public long TotalMeasurements
            {
                get
                {
                    return m_totalMeasurements;
                }
            }

            /// <summary>Gets the total number of measurements that have been discarded because of old timestamps
            /// (i.e., measurements that were outside the time deviation tolerance from base time, past or future).</summary>
            public long DiscardedMeasurements
            {
                get
                {
                    return m_discardedMeasurements;
                }
            }

            /// <summary>Gets the last measurement that was discarded by the concentrator.</summary>
            public IMeasurement LastDiscardedMeasurement
            {
                get
                {
                    return m_lastDiscardedMeasurement;
                }
            }

            /// <summary>Gets the total number of published measurements.</summary>
            public long PublishedMeasurements
            {
                get
                {
                    return m_publishedMeasurements;
                }
            }

            /// <summary>Gets the total number of published frames.</summary>
            public long PublishedFrames
            {
                get
                {
                    return m_publishedFrames;
                }
            }

            /// <summary>Gets the total number of measurements that were sorted by arrival because the measurement
            /// reported a bad timestamp quality.</summary>
            public long MeasurementsSortedByArrival
            {
                get
                {
                    return m_measurementsSortedByArrival;
                }
            }

            /// <summary>Gets the total seconds of required sorting time since concentrator started.</summary>
            public double TotalSortingTime
            {
                get
                {
                    return TVA.DateTime.Common.TicksToSeconds(m_totalSortTime);
                }
            }

            /// <summary>Gets the average required sorting time per frame, in seconds.</summary>
            /// <remarks>Slow reporting device measurements will have a negative impact on this statistic.</remarks>
            public double AverageSortingTimePerFrame
            {
                get
                {
                    return TotalSortingTime / m_publishedFrames;
                }
            }

            /// <summary>Gets the total number of milliseconds frames have waiting for publication since concentrator started.</summary>
            public long TotalWaitingTime
            {
                get
                {
                    return m_totalWaitTime;
                }
            }

            /// <summary>Gets the average required waiting time till frame is published, in milliseconds.</summary>
            /// <remarks>If user publication function exceed available publishing time (1/framesPerSecond), concentration will fall behind.</remarks>
            public double AverateWaitingTimePerFrame
            {
                get
                {
                    return TotalWaitingTime / m_publishedFrames;
                }
            }

            /// <summary>Places measurement data point in its proper row/cell position.</summary>
            public virtual void SortMeasurement(IMeasurement measurement)
            {

                SortMeasurements(new IMeasurement[] { measurement });

            }

            /// <summary>Places multiple measurement data points in their proper row/cell positions.</summary>
            public virtual void SortMeasurements(ICollection<IMeasurement> measurements)
            {

                // This function is called continually with new measurements and handles the "time-alignment"
                // (i.e., sorting) of these new values. Many threads will be waiting for frames of time aligned data
                // so make sure any work to be done here is executed as efficiently as possible.
                IFrame frame;
                long ticks;
                long lastTicks;
                double distance;
                bool discardMeasurement;

                // Tracks the total number of measurements requested for sorting.
                Interlocked.Add(ref m_totalMeasurements, measurements.Count);

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
                            // Device reports measurement timestamp as bad. Since the measurement may have been
                            // delayed by prior concentration or long network distance, this function assumes
                            // that our local real time value is better than the device measurement, so we set
                            // the measurement's timestamp to real time and sort the measurement by arrival time.
                            measurement.Ticks = RealTimeTicks;
                            Interlocked.Increment(ref m_measurementsSortedByArrival);
                        }
                        else
                        {
                            // If sorting by arrival time is not allowed, data with bad timestamps is discarded.
                            discardMeasurement = true;
                        }
                    }

                    if (!discardMeasurement)
                    {
                        // Get ticks for this measurement.
                        ticks = measurement.Ticks;

                        //
                        // *** Sort the measurement into proper frame ***
                        //

                        // Get the destination frame for the measurement. Note that groups of  parsed measurements will
                        // typically be coming in from the same source and will have the same ticks. If we have already
                        // found the destination frame for the same ticks, then there is no need to lookup frame again.
                        if (frame == null || ticks != lastTicks)
                        {
                            // Badly time-aligned measurements, or those coming in at a higher sample rate, may fall
                            // outside available frame buckets. To check for this, the difference between the measurement
                            // timestamp and real-time in seconds is calculated and validated between lag and lead times.
                            distance = DistanceFromRealTime(ticks);

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
                                frame = m_frameQueue.GetFrame(ticks);
                                lastTicks = ticks;
                            }
                        }

                        if (frame == null)
                        {
                            // Discards the data item if no bucket for it is found.
                            discardMeasurement = true;
                            lastTicks = 0;
                        }
                        else
                        {
                            // Makes sure the starting sort time for this frame is initialized.
                            if (frame.StartSortTime == 0)
                            {
                                frame.StartSortTime = DateTime.UtcNow.Ticks;
                            }

                            // Calls user customizable function to assign new measurement to its frame.
                            if (AssignMeasurementToFrame(frame, measurement))
                            {
                                // Tracks the last sorted measurement in this frame.
                                frame.LastSortTime = DateTime.UtcNow.Ticks;
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

                            // Tracks the absolute latest measurement values.
                            if (m_trackLatestMeasurements)
                            {
                                m_latestMeasurements.UpdateMeasurementValue(measurement);
                            }
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
                            // If the measurement time is newer than the current real time value, and it is within the
                            // specified(time) deviation tolerance of the local clock time, then it sets the
                            // measurement time as real time.
                            long realTimeTicks = m_realTimeTicks;

                            if (ticks > m_realTimeTicks)
                            {
                                // Applies a resonability check to this value. This is done using the local clock.
                                // Because of the frequency with which this function gets called, it does not call the
                                // TimeIsValid nor the DistanceFromRealTime functions to determine if the real time
                                // ticks are valid. Instead, it manually implements the code here to avoid the function
                                // call overhead. Since the lead time typically defines the tolerated accuracy of the
                                // local clock to real time, it uses this value as the + and - timestamp tolerance to
                                // validate if the measurement time is reasonable.
                                long currentTimeTicks = DateTime.UtcNow.Ticks;
                                distance = (currentTimeTicks - ticks) / TVA.DateTime.Common.TicksPerSecond;

                                if (distance <= m_leadTime && distance >= -m_leadTime)
                                {
                                    // The new time measurement looks good, so this function assumes the time is
                                    // "real time," so long as another thread has not changed the real time value
                                    // already. Using the interlocked compare exchange method introduces the
                                    // possibility that we may have had newer ticks than another thread that just
                                    // updated real-time ticks, but if so the deviation will not be much since ticks
                                    // were greater than current real-time ticks in all threads that got to this
                                    // point. Besides, newer measurements are always coming in anyway and the compare
                                    // exchange method saves a call to a monitor lock reducing possible contention.
                                    Interlocked.CompareExchange(ref m_realTimeTicks, ticks, realTimeTicks);
                                }
                                else
                                {
                                    // Measurement ticks were outside of time deviation tolerances so we'll also check to make
                                    // sure current real-time ticks are within these tolerances as well
                                    distance = (currentTimeTicks - m_realTimeTicks) / TVA.DateTime.Common.TicksPerSecond;

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

            /// <summary>Gets detailed current state and status of concentrator.</summary>
            public virtual string Status
            {
                get
				{
					DateTime currentTime = DateTime.UtcNow;
					IFrame lastFrame = m_frameQueue.Last;
					
					System.Text.StringBuilder with_1 = new StringBuilder;
					with_1.Append("     Data concentration is: ");
					if (m_enabled)
					{
						with_1.Append("Enabled");
					}
					else
					{
						with_1.Append("Disabled");
					}
					with_1.AppendLine();
					with_1.Append("    Total process run time: ");
					with_1.Append(TVA.DateTime.Common.SecondsToText(RunTime));
					with_1.AppendLine();
					with_1.Append("    Measurement wait delay: ");
					with_1.Append(m_lagTime);
					with_1.Append(" seconds (lag time)");
					with_1.AppendLine();
					with_1.Append("     Local clock tolerance: ");
					with_1.Append(m_leadTime);
					with_1.Append(" seconds (lead time)");
					with_1.AppendLine();
					with_1.Append("    Local clock time (UTC): ");
					with_1.Append(currentTime.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
					with_1.AppendLine();
					with_1.Append("  Using clock as real-time: ");
					with_1.Append(m_useLocalClockAsRealTime);
					with_1.AppendLine();
					if (! m_useLocalClockAsRealTime)
					{
						with_1.Append("      Local clock accuracy: ");
						with_1.Append(DistanceFromRealTime(DateTime.UtcNow.Ticks).ToString("0.0000"));
						with_1.Append(" second deviation from latest time");
						with_1.AppendLine();
					}
					with_1.Append(" Allowing sorts by arrival: ");
					with_1.Append(m_allowSortsByArrival);
					with_1.AppendLine();
					with_1.Append("        Total measurements: ");
					with_1.Append(m_totalMeasurements);
					with_1.AppendLine();
					with_1.Append("    Published measurements: ");
					with_1.Append(m_publishedMeasurements);
					with_1.AppendLine();
					with_1.Append("    Discarded measurements: ");
					with_1.Append(m_discardedMeasurements);
					with_1.AppendLine();
					with_1.Append("Last discarded measurement: ");
					if (m_lastDiscardedMeasurement == null)
					{
						with_1.Append("<none>");
					}
					else
					{
						with_1.Append(Measurement.ToString(m_lastDiscardedMeasurement));
						with_1.Append(" - ");
						with_1.Append(m_lastDiscardedMeasurement.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
					}
					with_1.AppendLine();
					with_1.Append("    Total sorts by arrival: ");
					with_1.Append(m_measurementsSortedByArrival);
					with_1.AppendLine();
					with_1.Append("   Missed sorts by timeout: ");
					with_1.Append(m_missedSortsByTimeout);
					with_1.AppendLine();
					with_1.Append("Average sorting time/frame: ");
					with_1.Append(AverageSortingTimePerFrame.ToString("0.0000"));
					with_1.Append(" seconds");
					with_1.AppendLine();
					with_1.Append(" Average time till publish: ");
					with_1.Append(AverateWaitingTimePerFrame.ToString("0.0000"));
					with_1.Append(" milliseconds");
					with_1.AppendLine();
					with_1.Append(" User function utilization: ");
					with_1.Append(((m_ticksPerFrame - TVA.DateTime.Common.SecondsToTicks(AverateWaitingTimePerFrame / 1000.0)) / m_ticksPerFrame).ToString("##0.0000%"));
					with_1.Append(" of available time used");
					with_1.AppendLine();
					with_1.Append("Published measurement loss: ");
					with_1.Append((m_discardedMeasurements / m_totalMeasurements).ToString("##0.0000%"));
					with_1.AppendLine();
					with_1.Append("      Loss due to timeouts: ");
					with_1.Append((m_missedSortsByTimeout / m_totalMeasurements).ToString("##0.0000%"));
					with_1.AppendLine();
					with_1.Append(" Measurement time accuracy: ");
					with_1.Append((1.0 - m_measurementsSortedByArrival / m_totalMeasurements).ToString("##0.0000%"));
					with_1.AppendLine();
					with_1.Append("    Total published frames: ");
					with_1.Append(m_publishedFrames);
					with_1.AppendLine();
					with_1.Append("        Defined frame rate: ");
					with_1.Append(m_framesPerSecond);
					with_1.Append(" frames/sec, ");
					with_1.Append(m_ticksPerFrame.ToString("0.00"));
					with_1.Append(" ticks/frame");
					with_1.AppendLine();
					with_1.Append("    Actual mean frame rate: ");
					with_1.Append((m_publishedFrames / (RunTime - m_lagTime)).ToString("0.00"));
					with_1.Append(" frames/sec");
					with_1.AppendLine();
					with_1.Append("        Queued frame count: ");
					with_1.Append(m_frameQueue.Count);
					with_1.AppendLine();
					with_1.AppendLine();
					with_1.Append("Current frame publishing detail:");
					with_1.AppendLine();
					with_1.AppendLine();
					with_1.Append("       Last frame = ");
					
					if (lastFrame == null)
					{
						with_1.Append("<none>");
					}
					else
					{
						with_1.Append(lastFrame.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
						with_1.Append(" - sort time: ");
						
						// Calculates maximum sort time for publishing frame.
						if (lastFrame.StartSortTime > 0 && lastFrame.LastSortTime > 0)
						{
							with_1.Append(TVA.DateTime.Common.TicksToSeconds(lastFrame.LastSortTime - lastFrame.StartSortTime).ToString("0.0000"));
							with_1.Append(" seconds");
						}
						else
						{
							with_1.Append("undetermined");
						}
						
						with_1.AppendLine();
						with_1.Append(" Last measurement = ");
						with_1.Append(Measurement.ToString(lastFrame.LastSortedMeasurement));
						
						// Calculates total time from last measurement ticks.
						if (lastFrame.LastSortTime > 0)
						{
							with_1.Append(" - ");
							with_1.Append(TVA.DateTime.Common.TicksToSeconds(TVA.Math.Common.NotLessThan(lastFrame.LastSortTime - lastFrame.LastSortedMeasurement.Ticks, 0)).ToString("0.0000"));
							with_1.Append(" seconds from source time");
						}
						else
						{
							with_1.Append(" - deviation from source time undetermined");
						}
					}
					
					with_1.AppendLine();
					
					return with_1.ToString();
				}
            }

            /// <summary>Shuts down concentrator in an orderly fashion.</summary>
            public virtual void Dispose()
            {

                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) below.
                Dispose(true);
                GC.SuppressFinalize(this);

            }

            #endregion

            #region " Protected Methods Implementation "

            /// <summary>Shuts down concentrator in an orderly fashion.</summary>
            protected virtual void Dispose(bool disposing)
            {

                if (!m_disposed)
                {
                    if (disposing)
                    {
                        @Stop();
                        if (m_frameQueue != null)
                        {
                            m_frameQueue.Dispose();
                        }
                        m_frameQueue = null;
                    }
                }

                m_disposed = true;

            }

            /// <summary>Consumers must override this method in order to publish a frame.</summary>
            protected abstract void PublishFrame(IFrame frame, int index);

            /// <summary>Consumers can choose to override this method to create a new custom frame.</summary>
            /// <remarks>Override is optional. The base class will create a basic frame to hold synchronized
            /// measurements.</remarks>
            protected internal virtual IFrame CreateNewFrame(long ticks)
            {

                return new Frame(ticks);

            }

            /// <summary>Consumers can choose to override this method to handle custom assignment of a measurement
            /// to its frame.</summary>
            /// <returns>True if measurement was successfully assigned to frame</returns>
            /// <remarks>
            /// <para>Override is optional. A measurement will simply be assigned to frame's keyed measurement
            /// dictionary otherwise.</para>
            /// <para>
            /// If overridden user must perform their own synchronization as needed, for example:
            /// <code>
            /// SyncLock frame.Measurements
            ///     If frame.Published Then
            ///         Return False
            ///     Else
            ///         frame.Measurements(measurement.Key) = measurement
            ///         Return True
            ///     End If
            /// End Synclock
            /// </code>
            /// </para>
            /// <para>Note that the frame.Measurements dictionary is used internally to synchrnonize assignment
            /// of the frame.Published flag. If your custom frame makes use of the frame.Measurements
            /// dictionary you must implement a locking scheme similar to the sample code above to
            /// prevent changes to the measurement dictionary during frame publication.</para>
            /// </remarks>
            protected virtual bool AssignMeasurementToFrame(IFrame frame, IMeasurement measurement)
            {

                IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements_Renamed;

                lock (measurements)
                {
                    if (frame.Published)
                    {
                        return false;
                    }
                    else
                    {
                        measurements[measurement.Key] = measurement;
                        return true;
                    }
                }

            }

            /// <summary>We implement finalizer for this class to ensure sample queue shuts down in an orderly
            /// fashion.</summary>
            ~ConcentratorBase()
            {

                Dispose(true);

            }

            /// <summary>Allows derived class access to frame queue.</summary>
            protected FrameQueue FrameQueue
            {
                get
                {
                    return m_frameQueue;
                }
            }

            /// <summary>Allows derived classes to raise a processing exception.</summary>
            protected void RaiseProcessException(Exception ex)
            {

                if (ProcessExceptionEvent != null)
                    ProcessExceptionEvent(ex);

            }

            #endregion

            #region " Private Methods Implementation "

            // This function is executed on a real-time thread, so make sure any work to be done here is executed
            // as efficiently as possible. Member variables being updated are only updated here so we don't worry
            // about atomic operations on these variables.
            private void PublishFrames()
            {

                IFrame frame;
                long ticks;
                int frameIndex;
                int distance;

                while (true)
                {
                    try
                    {
                        // Get top frame, this blocks current thread until new head is assigned
                        frame = m_frameQueue.Head;

                        if (frame != null)
                        {
                            // Get ticks for this frame
                            ticks = frame.Ticks;

                            // Wait for needed lagtime to pass before we begin publishing
                            distance = System.Convert.ToInt32((m_lagTime - DistanceFromRealTime(ticks)) * 1000.0);

                            if (distance > 0)
                            {
                                if (distance > 1)
                                {
                                    Thread.Sleep(distance - 1);
                                }

                                while (!(DistanceFromRealTime(ticks) >= m_lagTime))
                                {
                                    Thread.Sleep(1);
                                }
                            }

                            // Mark the frame as published to prevent any further sorting into this frame.
                            // Assignment of this flag is synchronized to ensure sorting into frame ceases.
                            lock (frame.Measurements_Renamed)
                            {
                                frame.Published = true;
                            }

                            // Calculate index of this frame within its second
                            frameIndex = Convert.ToInt32((ticks - TVA.DateTime.Common.BaselinedTimestamp(ticks, BaselineTimeInterval.Second).Ticks) / m_ticksPerFrame);

                            // Publish the current frame.
                            try
                            {
                                PublishFrame(frame, frameIndex);
                            }
                            finally
                            {
                                // Remove the frame from the queue whether it successfully published or not
                                m_frameQueue.Pop();
                            }

                            // Calculate total time required to sort measurements into this frame so far.
                            if (frame.StartSortTime > 0 && frame.LastSortTime > 0)
                            {
                                m_totalSortTime += frame.LastSortTime - frame.StartSortTime;
                            }

                            // Update publication statistics.
                            m_publishedFrames++;
                            m_publishedMeasurements += frame.PublishedMeasurements;
                            m_totalWaitTime += distance;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        // Time to leave...
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Not stopping for exceptions - but we'll let user know there are issues...
                        if (ProcessExceptionEvent != null)
                            ProcessExceptionEvent(ex);
                    }
                }

            }

            // Exposes the number of unpublished seconds of data in the queue (note that first second of data will always be "publishing").
            private void m_monitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {

                int secondsOfData = (m_frameQueue.Count / m_framesPerSecond) - 1;
                if (secondsOfData < 0)
                {
                    secondsOfData = 0;
                }
                if (UnpublishedSamplesEvent != null)
                    UnpublishedSamplesEvent(secondsOfData);

            }

            #region " Old Code "

            // Note: 08/01/2007 - Moved this code directly into the sorting function since this was the only place it was
            // being called to optimize by preventing function call overhead


            ///' <summary>This critical function automatically manages the sample queue based on timestamps of incoming measurements</summary>
            ///' <returns>The sample associated with the specified timestamp. If the sample is not found at timestamp, it will be created.</returns>
            ///' <param name="ticks">Ticks of the timestamp of the sample to get</param>
            ///' <remarks>Function will return null if timestamp is outside of the specified time deviation tolerance</remarks>
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
            //                    For x As Integer = 1 To System.Math.Floor(distance)
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

            ///' <summary>Gets the sample associated with the specified timestamp.</summary>
            ///' <returns>The sample associated with the specified timestamp. If the specified timestamp is not found, property returns null.</returns>
            ///' <param name="ticks">The ticks of the baselined timestamp of the sample to get.</param>
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

            ///' <summary>Gets the current publishing sample.</summary>
            //Public ReadOnly Property CurrentSample() As Sample
            //    Get
            //        If m_sampleQueue.Count > 0 Then
            //            Return m_sampleQueue(0%).Value
            //        Else
            //            Return Nothing
            //        End If
            //    End Get
            //End Property

            ///' <summary>Gets the index of the frame that is currently, or about to be, publishing.</summary>
            //Public ReadOnly Property CurrentFrameIndex() As Integer
            //    Get
            //        Return m_frameIndex
            //    End Get
            //End Property

            ///' <summary>This event is raised after a sample is published, so that consumers may handle any last
            ///' minute operations on a sample before it gets released.</summary>
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

}
