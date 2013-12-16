#region [ Modification History ]
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
using GSF;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for TemporalMeasurementTest and is intended
    ///to contain all TemporalMeasurementTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TemporalMeasurementTest
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

        #region [ Properties ]
        public IFrame Frame
        {
            get
            {
                return this.m_frame;
            }
        }

        public uint id
        {
            get
            {
                return 1;
            }
        }

        public Guid ID
        {
            get
            {
                return Guid.Parse("9cdd4506-9490-11e1-922f-0024e856a96e");
            }
        }

        public double LagTimeSeconds
        {
            get
            {
                return 60;
            }
        }

        public Ticks LagTimeTicks
        {
            get
            {
                return Ticks - 60 * Ticks.PerSecond;
            }
        }

        public double LeadTimeSeconds
        {
            get
            {
                return 60;
            }
        }

        public Ticks LeadTimeTicks
        {
            get
            {
                return Ticks + 60 * Ticks.PerSecond;
            }
        }

        public IMeasurement Measurement
        {
            get
            {
                return m_measurement;
            }
        }

        public Guid SignalID
        {
            get
            {
                return Guid.Parse("7aaf0a8f-3a4f-4c43-ab43-ed9d1e64a255");
            }
        }

        public string source
        {
            get
            {
                return "TVA-SHELBY";
            }
        }

        public MeasurementStateFlags StateFlags
        {
            get
            {
                return MeasurementStateFlags.Normal;
            }
        }

        public string TagName
        {
            get
            {
                return "Unit-Testing";
            }
        }

        public Ticks Ticks
        {
            get
            {
                return m_ticks;
            }
        }

        public float Value
        {
            get
            {
                return 102F;
            }
        }

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

        #region
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
        ///A test for GetAdjustedValue
        ///</summary>
        [TestMethod()]
        public void GetAdjustedValueTest()
        {
            double lagTime = LagTimeSeconds;
            double leadTime = LeadTimeSeconds;
            TemporalMeasurement target = new TemporalMeasurement(Measurement, lagTime, leadTime);
            double expected = Value;
            double actual;
            actual = target.GetAdjustedValue(Ticks);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest()
        {
            double lagTime = LagTimeSeconds;
            double leadTime = LeadTimeSeconds;
            TemporalMeasurement target = new TemporalMeasurement(lagTime, leadTime);
            Ticks timestamp = new Ticks();
            double expected = 0F;
            double actual;
            actual = target.GetValue(timestamp);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for LagTime
        ///</summary>
        [TestMethod()]
        public void LagTimeTest()
        {
            double lagTime = LagTimeSeconds;
            double leadTime = LeadTimeSeconds;
            TemporalMeasurement target = new TemporalMeasurement(lagTime, leadTime);
            double expected = LagTimeSeconds;
            double actual;
            target.LagTime = expected;
            actual = target.LagTime;
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for LeadTime
        ///</summary>
        [TestMethod()]
        public void LeadTimeTest()
        {
            double lagTime = LagTimeSeconds;
            double leadTime = LeadTimeSeconds;
            TemporalMeasurement target = new TemporalMeasurement(lagTime, leadTime);
            double expected = LagTimeSeconds;
            double actual;
            target.LeadTime = expected;
            actual = target.LeadTime;
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        /// <summary>
        /// Initialization
        /// </summary>
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
        ///A test for SetValue
        ///</summary>
        [TestMethod()]
        public void SetValueTest()
        {
            double lagTime = LagTimeSeconds;
            double leadTime = LeadTimeSeconds;
            TemporalMeasurement target = new TemporalMeasurement(lagTime, leadTime);
            bool expected = true;
            bool actual;
            actual = target.SetValue(Ticks, Value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TemporalMeasurement Constructor
        ///</summary>
        [TestMethod()]
        public void TemporalMeasurementConstructorTest()
        {
            IMeasurement measurement = Measurement;
            double lagTime = LagTimeSeconds;
            double leadTime = LeadTimeSeconds;
            TemporalMeasurement target = new TemporalMeasurement(measurement, lagTime, leadTime);
            Assert.IsInstanceOfType(target, typeof(TemporalMeasurement));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for TemporalMeasurement Constructor
        ///</summary>
        [TestMethod()]
        public void TemporalMeasurementConstructorTest1()
        {
            double lagTime = LagTimeSeconds;
            double leadTime = LeadTimeSeconds;
            TemporalMeasurement target = new TemporalMeasurement(lagTime, leadTime);
            Assert.IsInstanceOfType(target, typeof(TemporalMeasurement));
            Assert.IsNotNull(target);
        }

        #endregion
    }
}