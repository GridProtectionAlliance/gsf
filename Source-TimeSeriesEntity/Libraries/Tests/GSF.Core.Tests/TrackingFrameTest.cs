#region [ Code Modification History ]
/*
 * 06/04/2012 Denis Kholine
 *   Generated Original version of source code.
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
using GSF.Threading;
using GSF;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for TrackingFrameTest and is intended
    ///to contain all TrackingFrameTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TrackingFrameTest
    {
        #region [ Members ]
        /// <summary>
        /// Measurements Frame
        /// </summary>
        private IFrame m_frame;

        /// <summary>
        /// Measurement
        /// </summary>
        private IMeasurement m_measurement;

        /// <summary>
        /// Dictionary of measurements to test frames
        /// </summary>
        private Dictionary<MeasurementKey, IMeasurement> m_measurements;

        /// <summary>
        /// Sorted measurements
        /// </summary>
        private int m_SortedMeasurements;

        /// <summary>
        /// Ticks
        /// </summary>
        private Ticks m_ticks;

        /// <summary>
        /// Current time
        /// </summary>
        private DateTime m_UtcNow;

        /// <summary>
        /// Adapter Collections of data source
        /// </summary>

        #endregion

        #region [ Context ]
        private TestContext testContextInstance;

        #region [ Properties ]
        private IFrame Frame
        {
            get
            {
                return this.m_frame;
            }
        }

        private uint id
        {
            get
            {
                return 1;
            }
        }

        private Guid ID
        {
            get
            {
                return Guid.Parse("9cdd4506-9490-11e1-922f-0024e856a96e");
            }
        }

        private double LagTimeSeconds
        {
            get
            {
                return 60;
            }
        }

        private Ticks LagTimeTicks
        {
            get
            {
                return Ticks - 60 * Ticks.PerSecond;
            }
        }

        private double LeadTimeSeconds
        {
            get
            {
                return 60;
            }
        }

        private Ticks LeadTimeTicks
        {
            get
            {
                return Ticks + 60 * Ticks.PerSecond;
            }
        }

        private IMeasurement Measurement
        {
            get
            {
                return m_measurement;
            }
        }

        private Guid SignalID
        {
            get
            {
                return Guid.Parse("7aaf0a8f-3a4f-4c43-ab43-ed9d1e64a255");
            }
        }

        private string source
        {
            get
            {
                return "TVA-SHELBY";
            }
        }

        private MeasurementStateFlags StateFlags
        {
            get
            {
                return MeasurementStateFlags.Normal;
            }
        }

        private string TagName
        {
            get
            {
                return "Unit-Testing";
            }
        }

        private Ticks Ticks
        {
            get
            {
                return m_ticks;
            }
        }

        private float Value
        {
            get
            {
                return 102F;
            }
        }

        #endregion

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

        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            this.m_UtcNow = new DateTime();
            this.m_UtcNow = DateTime.UtcNow;
            this.m_measurement = new Measurement();
            this.m_measurement.ID = this.ID;
            this.m_measurement.Key = new MeasurementKey(this.SignalID, this.id, this.source);
            this.m_measurement.Value = this.Value;
            this.m_measurement.TagName = this.TagName;
            this.m_measurement.StateFlags = this.StateFlags;
            this.m_ticks = new Ticks(m_UtcNow);
            this.m_measurement.Timestamp = this.m_ticks;
            this.m_measurements = new Dictionary<MeasurementKey, IMeasurement>();
            this.m_measurements.Add(new MeasurementKey(), m_measurement);
            this.m_frame = new Frame(this.m_ticks, m_measurements);
            this.m_frame.LastSortedMeasurement = this.m_measurement;
            this.m_SortedMeasurements = m_frame.SortedMeasurements;
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for DeriveMeasurementValue
        ///</summary>
        [TestMethod()]
        public void DeriveMeasurementValueTest()
        {
            IFrame sourceFrame = Frame;
            DownsamplingMethod downsamplingMethod = new DownsamplingMethod();
            TrackingFrame target = new TrackingFrame(sourceFrame, downsamplingMethod);
            IMeasurement measurement = Measurement;
            IMeasurement expected = Measurement;
            IMeasurement actual;
            actual = target.DeriveMeasurementValue(measurement);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DownsampledMeasurements
        ///</summary>
        [TestMethod()]
        public void DownsampledMeasurementsTest()
        {
            IFrame sourceFrame = Frame;
            DownsamplingMethod downsamplingMethod = new DownsamplingMethod();
            TrackingFrame target = new TrackingFrame(sourceFrame, downsamplingMethod);
            long actual;
            actual = target.DownsampledMeasurements;
            Assert.AreEqual(actual, -1);
        }

        /// <summary>
        ///A test for Lock
        ///</summary>
        [TestMethod()]
        public void LockTest()
        {
            IFrame sourceFrame = Frame;
            DownsamplingMethod downsamplingMethod = new DownsamplingMethod();
            downsamplingMethod = DownsamplingMethod.LastReceived;
            TrackingFrame target = new TrackingFrame(sourceFrame, downsamplingMethod);

            ReaderWriterSpinLock actual = new ReaderWriterSpinLock();
            actual = target.Lock;
            try
            {
                actual.EnterReadLock();
                actual.ExitReadLock();
                actual.EnterWriteLock();
                actual.ExitWriteLock();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for SourceFrame
        ///</summary>
        [TestMethod()]
        public void SourceFrameTest()
        {
            IFrame sourceFrame = Frame;
            DownsamplingMethod downsamplingMethod = new DownsamplingMethod();
            TrackingFrame target = new TrackingFrame(sourceFrame, downsamplingMethod);
            IFrame actual;
            actual = target.SourceFrame;
            Assert.AreEqual(actual, Frame);
        }

        /// <summary>
        ///A test for Timestamp
        ///</summary>
        [TestMethod()]
        public void TimestampTest()
        {
            IFrame sourceFrame = Frame;
            DownsamplingMethod downsamplingMethod = new DownsamplingMethod();
            TrackingFrame target = new TrackingFrame(sourceFrame, downsamplingMethod);
            long actual;
            actual = target.Timestamp;
            Assert.AreEqual(actual, (long)Ticks);
        }

        /// <summary>
        ///A test for TrackingFrame Constructor
        ///</summary>
        [TestMethod()]
        public void TrackingFrameConstructorTest()
        {
            IFrame sourceFrame = Frame;
            DownsamplingMethod downsamplingMethod = new DownsamplingMethod();
            TrackingFrame target = new TrackingFrame(sourceFrame, downsamplingMethod);
            Assert.IsInstanceOfType(target, typeof(TrackingFrame));
            Assert.IsNotNull(target);
        }
        #endregion
    }
}