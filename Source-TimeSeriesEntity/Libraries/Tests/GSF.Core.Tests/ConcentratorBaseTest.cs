#region [ Code Modification History ]
/*
 * 04/18/2012 Denis Kholine
 *   Generated Original version of source code.
 *
 * 04/19/2012 Denis Kholine
 *  Added unit test description.
 *
 * 04/25/2012 Denis Kholine
 *  Update tests initialization and cleanup procedures.
 *
 * 05/07/2012 Denis Kholine
 *  Relocating git branch
 *
 * 06/20/2012 Denis Kholine
 *  Update ConcentratorBase wrapper
 */
#endregion

#region  [ UIUC NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois>
All rights reserved.

Developed by: <ITI>
<University of Illinois>
<http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
• Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/
#endregion

#region [ Code Using ]
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
using GSF.Units;
using GSF;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for ConcentratorBaseTest and is intended
    ///to contain all ConcentratorBaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConcentratorBaseTest
    {
        #region [ Class Wrappers ]
        /// <summary>
        /// Wrapper for ConcentratorBase class
        /// </summary>
        public class ConcentratorBaseWrapper : ConcentratorBase
        {
            #region [ Public Declarations ]
            public bool IsStatisticsReset { get; set; }

            #endregion [ Public Declarations ]

            public ConcentratorBaseWrapper()
                : base()
            {
                this.IsStatisticsReset = false;
            }

            public ConcentratorBaseWrapper(int framesPerSecond, double lagTime, double leadTime)
                : base(framesPerSecond, lagTime, leadTime)
            {
                this.IsStatisticsReset = false;
                this.PublishFrame(new Frame(DateTime.Now, 1), 1);
                this.CreateNewFrame(DateTime.Now);
                base.ExpectedMeasurements = 1;
                base.ProcessingInterval = 10000;
                //base.Enabled = true;
            }

            public override void ResetStatistics()
            {
                this.IsStatisticsReset = true;
            }

            protected override void PublishFrame(IFrame frame, int index)
            {
            }
        }

        #endregion

        #region [ Members ]
        /// <summary>
        /// Gets or sets flag that allows system to preemptively publish frames assuming all <see cref="ExpectedMeasurements"/> have arrived.
        /// </summary>
        /// <remarks>
        /// In order for this property to used, the <see cref="ExpectedMeasurements"/> must be defined.
        /// </remarks>
        public bool AllowPreemptivePublishing = true;

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
        /// Value will be forced to <c>false</c> if <see cref="ProcessByReceivedTimestamp"/> is <c>true</c>.
        /// </para>
        /// </remarks>
        public bool AllowSortsByArrival = true;

        /// <summary>
        /// Gets the average required frame publication time, in seconds.
        /// </summary>
        /// <remarks>
        /// If user publication function, <see cref="ConcentratorBase.PublishFrame"/>, consistently exceeds available publishing time
        /// (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c>), concentration will fall behind.
        /// </remarks>
        public Time AveragePublicationTimePerFrame;

        /// <summary>
        /// Gets the total number of measurements that have been discarded because of old timestamps
        /// (i.e., measurements that were outside the time deviation tolerance from base time, past or future).
        /// </summary>
        public long DiscardedMeasurements = 1;

        /// <summary>
        /// Gets the total number of downsampled measurements processed by the concentrator.
        /// </summary>
        public long DownsampledMeasurements = 1;

        /// <summary>
        /// Gets or sets the <see cref="DownsamplingMethod"/> to be used by the concentrator.
        /// </summary>
        /// <remarks>
        /// The downsampling method determines the algorithm to use if input is being received at a higher-resolution than the defined output.
        /// </remarks>
        public DownsamplingMethod DownsamplingMethod = DownsamplingMethod.BestQuality;

        /// <summary>
        /// Gets or sets the current enabled state of concentrator.
        /// </summary>
        /// <returns>Current enabled state of concentrator</returns>
        /// <remarks>
        /// Concentrator must be started by calling <see cref="ConcentratorBase.Start"/> method or setting
        /// <c><see cref="ConcentratorBase.Enabled"/> = true</c>) before concentration will begin.
        /// </remarks>
        public bool Enabled = true;

        /// <summary>
        /// Gets or sets the expected number of measurements to be assigned to a single frame.
        /// </summary>
        public int ExpectedMeasurements = 10;

        /// <summary>
        /// Gets the total number of frames ahead of schedule processed by the concentrator.
        /// </summary>
        public long FramesAheadOfSchedule = 3;

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        public int FramesPerSecond = 30;

        /// <summary>
        /// Gets or sets flag that determines if bad timestamps (as determined by measurement's timestamp quality)
        /// should be ignored when sorting measurements.
        /// </summary>
        /// <remarks>
        /// Setting this property to <c>true</c> forces system to use timestamps as-is without checking quality.
        /// If this property is <c>true</c>, it will supercede operation of <see cref="AllowSortsByArrival"/>.
        /// </remarks>
        public bool IgnoreBadTimestamps = false;

        /// <summary>
        /// Gets defined past time deviation tolerance, in ticks.
        /// </summary>
        public Ticks LagTicks = 1000;

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        public double LagTime = 0.1;

        /// <summary>
        /// Gets a reference the last <see cref="IMeasurement"/> that was discarded by the concentrator.
        /// </summary>
        public IMeasurement LastDiscardedMeasurement;

        /// <summary>
        /// Gets the calculated latency of the last <see cref="IMeasurement"/> that was discarded by the concentrator.
        /// </summary>
        public Ticks LastDiscardedMeasurementLatency;

        /// <summary>
        /// Gets reference to the last published <see cref="IFrame"/>.
        /// </summary>
        public IFrame LastFrame;

        /// <summary>
        /// Gets reference to the collection of absolute latest received measurement values.
        /// </summary>
        public ImmediateMeasurements LatestMeasurements;

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        public double LeadTime = 0.2;

        /// <summary>
        /// Gets or sets the maximum frame publication timeout in milliseconds, set to <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The concentrator automatically defines a precision timer to provide the heatbeat for frame publication, however if the system
        /// gets busy the heartbeat signals can be missed. This property defines a maximum wait timeout before reception of the heartbeat
        /// signal to make sure frame publications continue to occur in a timely fashion even when a system is under stress.
        /// </para>
        /// <para>
        /// This property is automatically defined as 2% more than the number of milliseconds per frame when the <see cref="FramesPerSecond"/>
        /// property is set. Users can override this default value to provide custom behavior for this timeout.
        /// </para>
        /// </remarks>
        public int MaximumPublicationTimeout = 2;

        /// <summary>
        /// Gets the total number of measurements that were sorted by arrival because the measurement reported a bad timestamp quality.
        /// </summary>
        public long MeasurementsSortedByArrival = 10;

        /// <summary>
        /// Gets the total number of missed sorts by timeout processed by the concentrator.
        /// </summary>
        public long MissedSortsByTimeout = 1;

        /// <summary>
        /// Gets or sets flag that determines if timestamp reasonability checks should be performed on incoming
        /// measurements (i.e., measurement timestamps are compared to system clock for reasonability using
        /// <see cref="LeadTime"/> tolerance).
        /// </summary>
        /// <remarks>
        /// Setting this value to <c>false</c> will make the concentrator use the latest value received as "real-time"
        /// without validation; this is not recommended in production since time reported by source devices may
        /// be grossly incorrect. For non-production configurations, setting this value to false will allow
        /// concentration of historical data.
        /// </remarks>
        public bool PerformTimestampReasonabilityCheck = true;

        /// <summary>
        /// Gets or sets flag that determines if concentrator should sort measurements by received time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this value to <c>true</c> will make concentrator use the timestamp of measurement
        /// reception, which is typically the <see cref="IMeasurement"/> creation time, for sorting and
        /// publication. This is useful in scenarios where the concentrator will be receiving very large
        /// volumes of data but not necessarily in real-time, such as, reading values from a file where
        /// you want data to be sorted and processed as fast as possible.
        /// </para>
        /// <para>
        /// Setting this value to <c>true</c> will force <see cref="UseLocalClockAsRealTime"/> to be <c>true</c>
        /// and <see cref="AllowSortsByArrival"/> to be <c>false</c>.
        /// </para>
        /// </remarks>
        public bool ProcessByReceivedTimestamp = true;

        /// <summary>
        /// Gets the total number of measurements successfully sorted.
        /// </summary>
        public long ProcessedMeasurements = 10;

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
        /// a timer interval, overwhich to process data. A value of -1 means to use the default processing interval, i.e., use
        /// the <see cref="FramesPerSecond"/>, while a value of 0 means to process data as fast as possible.
        /// </para>
        /// <para>
        /// From a real-time perspective the <see cref="ConcentratorBase"/> defines its general processing interval based on
        /// the defined <see cref="FramesPerSecond"/> property. The frames per second property, however, is more than a basic
        /// processing interval since it is used to define the intervals in one second that will become the time sorting
        /// destination "buckets" used by the concentrator irrespective of the data rate of the incoming data. As an example,
        /// if the frames per second of the concentrator is set to 30 and the source data rate is 60fps, then data will be
        /// downsampled to 30 frames of sorted incoming data but the assigned processing interval will be used to publish the
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
        ///         In this case the default processing interval has been requested, as a result the <see cref="ProcessByReceivedTimestamp"/>
        ///         will be set to <c>false</c> and the concentrator processing interval will be defined based on the currently defined
        ///         <see cref="FramesPerSecond"/> property, e.g., if the frames per second is 30 the processing interval will be 33.33ms.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>0</term>
        ///         <description>
        ///         In this case the processing interval has been defined to process data as fast as possible, as a result the
        ///         <see cref="ProcessByReceivedTimestamp"/> property will be set to <c>true</c> and <see cref="UsePrecisionTimer"/> property
        ///         will be set to <c>false</c>. With a processing interval of zero data is expected to flow into the concentrator as quick as
        ///         it can be provided. The <see cref="FramesPerSecond"/> property will still be used to sort data by time into appropriate
        ///         frames, but the concentrator will use the reception time of the measurements against the defined lag-time to make sure
        ///         needed data has arrived before publication and frames will be published at the same rate of data arrival.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>&gt; 0</term>
        ///         <description>
        ///         In this case a specific processing interval has been defined for processing data, as a result both the
        ///         <see cref="ProcessByReceivedTimestamp"/> and <see cref="UsePrecisionTimer"/> properties will be set to <c>true</c>. With
        ///         a specifically defined processing interval, data is expected to flow into the concentrator at a similar rate. The
        ///         <see cref="FramesPerSecond"/> property will still be used to sort data by time into appropriate frames, but the concentrator
        ///         will use the reception time of the measurements against the defined lag-time to make sure needed data has arrived before
        ///         publication and frames will be published on the specified interval. If multiple frames are ready for publication when the
        ///         processing interval executes, then all the ready frames will be published sequentially as quickly as possible.
        ///         </description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public int ProcessingInterval = 1;

        /// <summary>
        /// Gets the total number of published frames.
        /// </summary>
        public long PublishedFrames = 2;

        /// <summary>
        /// Gets the total number of published measurements.
        /// </summary>
        public long PublishedMeasurements = 11;

        /// <summary>
        /// Gets detailed state of concentrator frame queue.
        /// </summary>
        public string QueueState;

        /// <summary>
        /// Gets the the most accurate time value that is available. If <see cref="UseLocalClockAsRealTime"/> = <c>true</c>, then
        /// this function will return <see cref="DateTime.UtcNow"/>. Otherwise, this function will return the timestamp of the
        /// most recent measurement, or <see cref="DateTime.UtcNow"/> if no measurement timestamps are within time deviation
        /// tolerances as specified by the <see cref="LeadTime"/> value.
        /// </summary>
        /// <remarks>
        /// Because the measurements being received by remote devices are often measured relative to GPS time, these timestamps
        /// are typically more accurate than the local clock. As a result, we can use the latest received timestamp as the best
        /// local time measurement we have (ignoring transmission delays); but, even these times can be incorrect so we still have
        /// to apply reasonability checks to these times. To do this, we use the local system time and the <see cref="LeadTime"/>
        /// value to validate the latest measured timestamp. If the newest received measurement timestamp gets too old or creeps
        /// too far into the future (both validated + and - against defined lead time property value), we will fall back on local
        /// system time. Note that this creates a dependency on a fairly accurate local clock - the smaller the lead time deviation
        /// tolerance, the better the needed local clock accuracy. For example, a lead time deviation tolerance of a few seconds
        /// might only require keeping the local clock synchronized to an NTP time source; but, a sub-second tolerance would
        /// require that the local clock be very close to GPS time.
        /// </remarks>
        public Ticks RealTime;

        /// <summary>
        /// Gets the total number of measurements ever requested for sorting.
        /// </summary>
        public long ReceivedMeasurements = 11;

        /// <summary>
        /// Gets the total amount of time, in seconds, that the concentrator has been active.
        /// </summary>
        public Time RunTime;

        /// <summary>
        /// Gets the UTC time the concentrator was started.
        /// </summary>
        public Ticks StartTime;

        /// <summary>
        /// Gets current detailed state and status of concentrator for display purposes.
        /// </summary>
        public string Status;

        /// <summary>
        /// Gets the UTC time the concentrator was stopped.
        /// </summary>
        public Ticks StopTime;

        /// <summary>
        /// Gets the number of ticks per frame.
        /// </summary>
        public double TicksPerFrame = 1000;

        /// <summary>
        /// Gets or sets the maximum time resolution, in ticks, to use when sorting measurements by timestamps into their proper destination frame.
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
        public long TimeResolution = 33;

        /// <summary>
        /// Gets the total number of seconds frames have spent in the publication process since concentrator started.
        /// </summary>
        public Time TotalPublicationTime;

        /// <summary>
        /// Gets or sets flag to start tracking the absolute latest received measurement values.
        /// </summary>
        /// <remarks>
        /// Lastest received measurement value will be available via the <see cref="LatestMeasurements"/> property.
        /// Note that enabling this option will slightly increase the required sorting time.
        /// </remarks>
        public bool TrackLatestMeasurements = true;

        /// <summary>
        /// Gets or sets flag that determines if system should track timestamp of publication for all frames and measurements.
        /// </summary>
        /// <remarks>
        /// Setting this value to <c>true</c> will cause the concentrator to mark the timestamp of publication in each
        /// <see cref="IFrame.PublishedTimestamp"/> and its measurement's <see cref="IMeasurement.PublishedTimestamp"/>.
        /// Since this is extra processing time that may not be needed except in cases of calculating statistics for
        /// system performance, this must be explicitly enabled.
        /// </remarks>
        public bool TrackPublishedTimestamp = true;

        /// <summary>
        /// Gets or sets flag that determines whether or not to use the local clock time as real-time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use your local system clock as real-time only if the time is locally GPS-synchronized,
        /// or if the measurement values being sorted were not measured relative to a GPS-synchronized clock.
        /// </para>
        /// <para>
        /// If <see cref="ProcessByReceivedTimestamp"/> is <c>true</c>, <see cref="UseLocalClockAsRealTime"/> will
        /// always be set to <c>true</c>, even if you try to set it to <c>false</c>.
        /// </para>
        /// </remarks>
        public bool UseLocalClockAsRealTime = true;

        /// <summary>
        /// Gets or sets flag that determines if precision timer should be used for frame publication.
        /// </summary>
        public bool UsePrecisionTimer = true;

        /// <summary>
        /// Gets the total number of wait handle expirations encounted due to delayed precision timer releases.
        /// </summary>
        public long WaitHandleExpirations = 2;

        /// <summary>
        /// Gets or sets collection of <see cref="AutoResetEvent"/> wait handles used to synchronize concentration publication with external events.
        /// </summary>
        protected AutoResetEvent[] ExternalEventHandles;

        /// <summary>
        /// Gets or sets maximum to time to wait, in milliseconds, for external events before publishing proceeds.
        /// </summary>
        protected int ExternalEventTimeout = 1;

        /// <summary>
        /// Assert expected
        /// </summary>
        private bool expected;

        /// <summary>
        /// Target to test
        /// </summary>
        private ConcentratorBase target;
        #endregion

        #region [ Context ]
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for AllowPreemptivePublishing
        ///</summary>
        [TestMethod()]
        public void AllowPreemptivePublishingTest()
        {
            bool actual;
            target.AllowPreemptivePublishing = expected;
            actual = target.AllowPreemptivePublishing;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for AllowSortsByArrival
        ///</summary>
        [TestMethod()]
        public void AllowSortsByArrivalTest()
        {
            bool actual;
            target.AllowSortsByArrival = expected;
            actual = target.AllowSortsByArrival;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for AveragePublicationTimePerFrame
        ///</summary>
        [TestMethod()]
        public void AveragePublicationTimePerFrameTest()
        {
            Time actual;
            actual = target.AveragePublicationTimePerFrame;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for DiscardedMeasurements
        ///</summary>
        [TestMethod()]
        public void DiscardedMeasurementsTest()
        {
            long actual;
            actual = target.DiscardedMeasurements;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            try
            {
                target.Dispose();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for DownsampledMeasurements
        ///</summary>
        [TestMethod()]
        public void DownsampledMeasurementsTest()
        {
            long actual;
            actual = target.DownsampledMeasurements;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for DownsamplingMethod
        ///</summary>
        [TestMethod()]
        public void DownsamplingMethodTest()
        {
            DownsamplingMethod expected = new DownsamplingMethod();
            DownsamplingMethod actual;
            target.DownsamplingMethod = expected;
            actual = target.DownsamplingMethod;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Enabled
        ///</summary>
        [TestMethod()]
        public void EnabledTest()
        {
            bool actual;
            target.Enabled = expected;
            actual = target.Enabled;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ExpectedMeasurements
        /// Gets or sets the expected number of measurements to be assigned to a single frame.
        ///</summary>
        [TestMethod()]
        public void ExpectedMeasurementsTest()
        {
            target.ExpectedMeasurements = 10;
            expected = (target.ExpectedMeasurements == 10);
            Assert.IsTrue(expected);
            expected = (target.ExpectedMeasurements.GetType() == typeof(int));
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for FramesAheadOfSchedule
        /// Gets the total number of frames ahead of schedule processed by the concentrator.
        ///</summary>
        [TestMethod()]
        public void FramesAheadOfScheduleTest()
        {
            expected = (target.FramesAheadOfSchedule >= 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for FramesPerSecond        ///
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        [TestMethod()]
        public void FramesPerSecondTest()
        {
            expected = (target.FramesPerSecond > 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for IgnoreBadTimestamps
        /// Gets or sets flag that determines if bad timestamps (as determined by measurement's timestamp quality) should be ignored when sorting measurements.
        /// Setting this property to true forces system to use timestamps as-is without checking quality. If this property is true, it will supercede operation of AllowSortsByArrival
        ///</summary>
        [TestMethod()]
        public void IgnoreBadTimestampsTest()
        {
            target.IgnoreBadTimestamps = true;
            expected = (target.IgnoreBadTimestamps == true);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for LagTicks
        /// Gets defined past time deviation tolerance, in ticks.
        ///</summary>
        [TestMethod()]
        public void LagTicksTest()
        {
            expected = (target.LagTicks >= 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for LagTime
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        [TestMethod()]
        public void LagTimeTest()
        {
            target.LagTime = 5;
            expected = (target.LagTime == 5);
            Assert.IsTrue(expected);
            expected = (target.LagTime.GetType() == typeof(double));
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for LastDiscardedMeasurementLatency
        /// Gets the calculated latency of the last <see cref="IMeasurement"/> that was discarded by the concentrator.
        /// </summary>
        [TestMethod()]
        public void LastDiscardedMeasurementLatencyTest()
        {
            expected = (target.LastDiscardedMeasurementLatency.GetType() == typeof(Ticks));
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for LastDiscardedMeasurement
        /// Gets a reference the last <see cref="IMeasurement"/> that was discarded by the concentrator.
        /// </summary>
        [TestMethod()]
        public void LastDiscardedMeasurementTest()
        {
            expected = (target.LastDiscardedMeasurement == null);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for LastFrame
        /// Gets reference to the last published IFrame.
        ///</summary>
        [TestMethod()]
        public void LastFrameTest()
        {
            expected = (target.LastFrame == null);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for LatestMeasurements
        /// Gets reference to the collection of absolute latest received measurement values.
        ///</summary>
        [TestMethod()]
        public void LatestMeasurementsTest()
        {
            expected = (target.LatestMeasurements.GetType() == typeof(ImmediateMeasurements));
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for LeadTime
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be subsecond).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        [TestMethod()]
        public void LeadTimeTest()
        {
            target.LeadTime = 0.01;
            expected = (target.LeadTime > 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for MaximumPublicationTimeout
        /// Gets or sets the maximum frame publication timeout in milliseconds, set to Infinite(-1) to wait indefinitely
        ///</summary>
        [TestMethod()]
        public void MaximumPublicationTimeoutTest()
        {
            target.MaximumPublicationTimeout = 10;
            expected = (target.MaximumPublicationTimeout == 10);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for MeasurementsSortedByArrival
        ///</summary>
        [TestMethod()]
        public void MeasurementsSortedByArrivalTest()
        {
            long actual;
            actual = target.MeasurementsSortedByArrival;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// A test for MillisecondsFromRealTime
        /// Returns the deviation, in milliseconds, that the given number of ticks is from real-time (i.e., <see cref="ConcentratorBase.RealTime"/>).
        /// </summary>
        /// <param name="timestamp">Timestamp to calculate distance from real-time.</param>
        /// <returns>A <see cref="Double"/> value indicating the deviation in milliseconds.</returns>
        [TestMethod()]
        public void MillisecondsFromRealTimeTest()
        {
            double actual = target.MillisecondsFromRealTime(this.StartTime);
            expected = (actual > 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for MissedSortsByTimeout
        ///</summary>
        [TestMethod()]
        public void MissedSortsByTimeoutTest()
        {
            long actual;
            actual = target.MissedSortsByTimeout;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// Test Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            // tc.Dispose();
            target.Dispose();
        }

        //
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        /// <summary>
        /// Test Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new ConcentratorBaseWrapper(33, 1, 1);
            expected = false;
            //   timestamp = new Ticks(tc.TicksInLong);
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        /// A test for PerformTimestampReasonabilityCheck
        /// Gets or sets flag that determines if timestamp reasonability checks should be performed on
        /// incoming measurements (i.e., measurement timestamps are compared to system clock for
        /// reasonability using LeadTime tolerance).
        /// Setting this value to false will make the concentrator use the latest value received as
        /// "real-time" without validation; this is not recommended in production since time reported
        /// by source devices may be grossly incorrect. For non-production configurations,
        /// setting this value to false will allow concentration of historical data.
        ///</summary>
        [TestMethod()]
        public void PerformTimestampReasonabilityCheckTest()
        {
            target.PerformTimestampReasonabilityCheck = false;
            expected = (target.PerformTimestampReasonabilityCheck == false);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for ProcessByReceivedTimestamp
        /// Gets or sets flag that determines if concentrator should sort measurements by received time.
        /// Setting this value to true will make concentrator use the timestamp of measurement reception,
        /// which is typically the IMeasurement creation time, for sorting and publication.
        /// This is useful in scenarios where the concentrator will be receiving very large volumes
        /// of data but not necessarily in real-time, such as, reading values from a file where you
        /// want data to be sorted and processed as fast as possible. Setting this value to true
        /// will force UseLocalClockAsRealTime to be true and AllowSortsByArrival to be false.
        ///</summary>
        [TestMethod()]
        public void ProcessByReceivedTimestampTest()
        {
            target.ProcessByReceivedTimestamp = true;
            expected = (target.UseLocalClockAsRealTime == true && target.AllowSortsByArrival == false);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for ProcessedMeasurements
        ///</summary>
        [TestMethod()]
        public void ProcessedMeasurementsTest()
        {
            long actual;
            actual = target.ProcessedMeasurements;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// A test for ProcessingInterval
        /// Gets the processing interval defined for the FrameRateTimer.
        ///</summary>
        [TestMethod()]
        public void ProcessingIntervalTest()
        {
            target.ProcessingInterval = this.ProcessingInterval;
            expected = (target.ProcessingInterval == this.ProcessingInterval);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for PublishedFrames
        ///</summary>
        [TestMethod()]
        public void PublishedFramesTest()
        {
            long actual;
            actual = target.PublishedFrames;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for PublishedMeasurements
        ///</summary>
        [TestMethod()]
        public void PublishedMeasurementsTest()
        {
            long actual;
            actual = target.PublishedMeasurements;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for QueueState
        ///</summary>
        [TestMethod()]
        public void QueueStateTest()
        {
            string actual;
            actual = target.QueueState;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for RealTime
        ///</summary>
        [TestMethod()]
        public void RealTimeTest()
        {
            Ticks actual;
            actual = target.RealTime;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for ReceivedMeasurements
        ///</summary>
        [TestMethod()]
        public void ReceivedMeasurementsTest()
        {
            long actual;
            actual = target.ReceivedMeasurements;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for ResetStatistics
        ///</summary>
        [TestMethod()]
        public void ResetStatisticsTest()
        {
            try
            {
                target.ResetStatistics();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for RunTime
        ///</summary>
        [TestMethod()]
        public void RunTimeTest()
        {
            Time actual;
            actual = target.RunTime;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for SecondsFromRealTime
        ///</summary>
        [TestMethod()]
        public void SecondsFromRealTimeTest()
        {
            Ticks timestamp = new Ticks(10);
            double expected = 63470449938.2336;
            double actual;
            actual = target.SecondsFromRealTime(timestamp);
            Assert.AreNotEqual(expected, actual);
        }

        /// <summary>
        ///A test for SortMeasurements
        ///</summary>
        [TestMethod()]
        public void SortMeasurementsTest()
        {
            List<Measurement> items = new List<Measurement>();
            items.Add(new Measurement());
            IEnumerable<IMeasurement> measurements = items;
            target.SortMeasurements(measurements);
        }

        /// <summary>
        ///A test for SortMeasurement
        ///</summary>
        [TestMethod()]
        public void SortMeasurementTest()
        {
            IMeasurement measurement = new Measurement();
            target.SortMeasurement(measurement);
            Assert.IsTrue(true);
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            target.Start();
            expected = (target.RunTime > 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for StartTime
        ///</summary>
        [TestMethod()]
        public void StartTimeTest()
        {
            Ticks actual;
            actual = target.StartTime;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for Status
        ///</summary>
        [TestMethod()]
        public void StatusTest()
        {
            string actual;
            actual = target.Status;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod()]
        public void StopTest()
        {
            target.Stop();
            expected = (target.Enabled == false);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for StopTime
        ///</summary>
        [TestMethod()]
        public void StopTimeTest()
        {
            Ticks actual;
            actual = target.StopTime;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for TicksPerFrame
        ///</summary>
        [TestMethod()]
        public void TicksPerFrameTest()
        {
            double actual;
            actual = target.TicksPerFrame;
            Assert.AreEqual(actual, target.TicksPerFrame);
        }

        /// <summary>
        /// A test for TimeResolution
        /// Gets or sets the maximum time resolution, in ticks,
        /// to use when sorting measurements by timestamps into their proper destination frame.
        /// Assigning values less than zero will be set to zero since minimum possible concentrator
        /// resolution is one tick (100-nanoseconds). Assigning values values greater than
        /// Ticks.PerSecond() will be set to Ticks.PerSecond() since maximum possible concentrator
        /// resolution is one second (i.e., 1 frame per second).
        ///</summary>
        [TestMethod()]
        public void TimeResolutionTest()
        {
            target.TimeResolution = 1;
            expected = (target.TimeResolution == 1);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for TotalPublicationTime
        ///</summary>
        [TestMethod()]
        public void TotalPublicationTimeTest()
        {
            Time actual;
            actual = target.TotalPublicationTime;
        }

        /// <summary>
        ///A test for TrackLatestMeasurements
        ///</summary>
        [TestMethod()]
        public void TrackLatestMeasurementsTest()
        {
            bool actual;
            target.TrackLatestMeasurements = expected;
            actual = target.TrackLatestMeasurements;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TrackPublishedTimestamp
        ///</summary>
        [TestMethod()]
        public void TrackPublishedTimestampTest()
        {
            target.TrackPublishedTimestamp = this.TrackLatestMeasurements;
            expected = (target.TrackPublishedTimestamp == this.TrackLatestMeasurements);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for UseLocalClockAsRealTime
        ///</summary>
        [TestMethod()]
        public void UseLocalClockAsRealTimeTest()
        {
            bool actual;
            expected = true;
            target.UseLocalClockAsRealTime = expected;
            actual = target.UseLocalClockAsRealTime;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UsePrecisionTimer
        ///</summary>
        [TestMethod()]
        public void UsePrecisionTimerTest()
        {
            target.UsePrecisionTimer = this.UsePrecisionTimer;
            expected = (target.UsePrecisionTimer == this.UsePrecisionTimer);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for WaitHandleExpirations
        ///</summary>
        [TestMethod()]
        public void WaitHandleExpirationsTest()
        {
            expected = (target.WaitHandleExpirations >= 0);
            Assert.IsTrue(expected);
        }

        #endregion
    }
}