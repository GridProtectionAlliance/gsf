#region [ Modification History ]
/*
 * 07/07/2012 Denis Kholine
 *  Generated Original version of source code.
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

#region [ Using ]
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Statistics;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF;
using GSF.TimeSeries;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for StatisticsEngineTest and is intended
    ///to contain all StatisticsEngineTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StatisticsEngineTest
    {
        #region [ Members ]
        private IAllAdaptersCase m_IAdapterCase;
        private IMeasurementCase measurement;
        private IMeasurementsCase measurements;
        private StatisticsEngine target;
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
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            m_IAdapterCase.Dispose();
            measurement.Dispose();
            measurements.Dispose();
            target.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_IAdapterCase = new IAllAdaptersCase();
            measurement = new IMeasurementCase();
            measurements = new IMeasurementsCase();
            target = new StatisticsEngine();

            target.DataSource = m_IAdapterCase.InputAdapter.DataSource;
            target.InitializationTimeout = 1000;
            target.Initialize();
            target.QueueMeasurementsForProcessing(measurements.Measurements);
            target.SetInitializedState(true);
            // target.Enabled = true;
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for AddSource
        ///</summary>
        [TestMethod()]
        public void AddSourceTest()
        {
            m_IAdapterCase = new IAllAdaptersCase();
            string sourceName = "InputAdapter";
            object source = new object();
            target.DataSource = m_IAdapterCase.InputAdapter.DataSource;
            bool expected = (target.DataSource.DataSetName == sourceName);
            Assert.IsTrue(expected);
        }
 

        /// <summary>
        ///A test for GetShortStatus
        ///</summary>
        [TestMethod()]
        public void GetShortStatusTest()
        {
            int maxLength = 0;
            string expected = string.Empty;
            string actual;
            actual = target.GetShortStatus(maxLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            try
            {
                target.Initialize();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }


        /// <summary>
        ///A test for RegexMatch
        ///</summary>
        [TestMethod()]
        public void RegexMatchTest()
        {
            string signalReference = string.Empty;
            string suffix = string.Empty;
            bool expected = false;
            bool actual;
            actual = StatisticsEngine.RegexMatch(signalReference, suffix);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            Ticks startTime = DateTime.UtcNow.Ticks;
            target.Start();
            int actual = target.StartTime.CompareTo(startTime);
            bool expected = (actual >= 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for StatisticsEngine Constructor
        ///</summary>
        [TestMethod()]
        public void StatisticsEngineConstructorTest()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(StatisticsEngine));
        }

        /// <summary>
        ///A test for SupportsTemporalProcessing default is false
        ///</summary>
        [TestMethod()]
        public void SupportsTemporalProcessingTest()
        {
            bool actual;
            actual = target.SupportsTemporalProcessing;
            Assert.IsFalse(actual);
        }

        #endregion
    }
}