#region [ Modification History ]
/*
 * 07/07/2012 Denis Kholine
 *  Generated Original version of source code.
 *
 * 08/29/2012 Denis Kholine
 *  Relocate class wrappers
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
using GSF.TimeSeries.Adapters;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.Units;
using GSF.TimeSeries;
using GSF;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for AdapterBaseTest and is intended
    ///to contain all AdapterBaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AdapterBaseTest
    {
        #region [ Members ]
        private IAllAdaptersCase m_IAdapterCase;
        private IMeasurementCase m_IMeasurement;
        private IMeasurementsCase m_IMeasurements;
        private IWaitHandlesCase m_IWaitHandlesCase;
        private IAdapterBaseCase target;
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
        //

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            m_IWaitHandlesCase.Dispose();
            m_IAdapterCase.Dispose();
            m_IMeasurements.Dispose();
            m_IMeasurement.Dispose();
            target.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_IWaitHandlesCase = new IWaitHandlesCase();
            m_IMeasurements = new IMeasurementsCase();
            m_IMeasurement = new IMeasurementCase();
            m_IAdapterCase = new IAllAdaptersCase();

            target = new IAdapterBaseCase();
        }
        #endregion

        #region [ Methods ]
 

        /// <summary>
        ///A test for AutoStart
        ///</summary>
        [TestMethod()]
        public void AutoStartTest()
        {
            bool expected = false;
            bool actual;
            target.AutoStart = expected;
            actual = target.AutoStart;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ConnectionString
        ///</summary>
        [TestMethod()]
        public void ConnectionStringTest()
        {
            string expected = "Unit Testing";
            string actual;
            target.ConnectionString = expected;
            actual = target.ConnectionString;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DataSource
        ///</summary>
        [TestMethod()]
        public void DataSourceTest()
        {
            System.Data.DataSet expected = m_IAdapterCase.InputAdapter.DataSource;
            System.Data.DataSet actual;
            target.DataSource = expected;
            actual = target.DataSource;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Enabled
        ///</summary>
        [TestMethod()]
        public void EnabledTest()
        {
            bool expected = false;
            bool actual;
            target.Enabled = expected;
            actual = target.Enabled;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetShortStatus
        ///</summary>
        [TestMethod()]
        public void GetShortStatusTest()
        {
            int maxLength = 10;
            string actual;
            actual = target.GetShortStatus(maxLength);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Length > 0);
        }

        /// <summary>
        ///A test for ID
        ///</summary>
        [TestMethod()]
        public void IDTest()
        {
            uint expected = 0;
            uint actual;
            target.ID = expected;
            actual = target.ID;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InitializationTimeout
        ///</summary>
        [TestMethod()]
        public void InitializationTimeoutTest()
        {
            int expected = 0;
            int actual;
            target.InitializationTimeout = expected;
            actual = target.InitializationTimeout;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Initialized
        ///</summary>
        [TestMethod()]
        public void InitializedTest()
        {
            bool expected = false;
            bool actual;
            target.Initialized = expected;
            actual = target.Initialized;
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
                target.SetInitializedState(true);
                Assert.IsTrue(target.Initialized);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for InputMeasurementKeys
        ///</summary>
        //[TestMethod()]
        //public void InputMeasurementKeysTest()
        //{
        //    MeasurementKey[] expected = m_IMeasurements.MeasurementKeys;
        //    MeasurementKey[] actual;
        //    target.InputMeasurementKeys = expected;
        //    actual = target.InputMeasurementKeys;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for IsInputMeasurement
        ///</summary>
        //[TestMethod()]
        //public void IsInputMeasurementTest()
        //{
        //    bool expected = true;
        //    bool actual;
        //    actual = target.IsInputMeasurement(m_IMeasurement.MeasurementKey);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for MeasurementReportingInterval
        ///</summary>
        //[TestMethod()]
        //public void MeasurementReportingIntervalTest()
        //{
        //    int expected = 0;
        //    int actual;
        //    target.MeasurementReportingInterval = expected;
        //    actual = target.MeasurementReportingInterval;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            string expected = "Unit Testing";
            string actual;
            target.Name = expected;
            actual = target.Name;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for OutputMeasurements
        ///</summary>
        //[TestMethod()]
        //public void OutputMeasurementsTest()
        //{
        //    IMeasurement[] expected = new IMeasurement[] { m_IMeasurement.Measurement };
        //    IMeasurement[] actual;
        //    target.OutputMeasurements = expected;
        //    actual = target.OutputMeasurements;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for ProcessedMeasurements
        ///</summary>
        //[TestMethod()]
        //public void ProcessedMeasurementsTest()
        //{
        //    long actual;
        //    actual = target.ProcessedMeasurements;
        //}

        /// <summary>
        ///A test for ProcessingInterval
        ///</summary>
        [TestMethod()]
        public void ProcessingIntervalTest()
        {
            int expected = 0;
            int actual;
            target.ProcessingInterval = expected;
            actual = target.ProcessingInterval;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ProcessMeasurementFilter
        ///</summary>
        //[TestMethod()]
        //public void ProcessMeasurementFilterTest()
        //{
        //    bool expected = false;
        //    bool actual;
        //    target.ProcessMeasurementFilter = expected;
        //    actual = target.ProcessMeasurementFilter;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for RunTime
        ///</summary>
        [TestMethod()]
        public void RunTimeTest()
        {
            Time actual;
            actual = target.RunTime;
        }

        /// <summary>
        ///A test for SetInitializedState
        ///</summary>
        [TestMethod()]
        public void SetInitializedStateTest()
        {
            bool initialized = true;
            target.SetInitializedState(initialized);
            Assert.IsTrue(initialized);
        }

        /// <summary>
        ///A test for SetTemporalConstraint
        ///</summary>
        [TestMethod()]
        public void SetTemporalConstraintTest()
        {
            string startTime = string.Empty;
            string stopTime = string.Empty;
            string constraintParameters = string.Empty;
            try
            {
                //TODO;
                target.SetTemporalConstraint(startTime, stopTime, constraintParameters);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod()]
        public void SettingsTest()
        {
            System.Collections.Generic.Dictionary<string, string> actual;
            actual = target.Settings;
        }

        /// <summary>
        ///A test for StartTimeConstraint
        ///</summary>
        [TestMethod()]
        public void StartTimeConstraintTest()
        {
            System.DateTime actual;
            actual = target.StartTimeConstraint;
        }

        /// <summary>
        ///A test for StartTime
        ///</summary>
        [TestMethod()]
        public void StartTimeTest()
        {
            Ticks actual;
            actual = target.StartTime;
        }

        /// <summary>
        ///A test for Status
        ///</summary>
        [TestMethod()]
        public void StatusTest()
        {
            //TODO:
            string actual;
            actual = target.Status;
        }

        /// <summary>
        ///A test for StopTimeConstraint
        ///</summary>
        [TestMethod()]
        public void StopTimeConstraintTest()
        {
            System.DateTime actual;
            actual = target.StopTimeConstraint;
        }

        /// <summary>
        ///A test for StopTime
        ///</summary>
        [TestMethod()]
        public void StopTimeTest()
        {
            try
            {
                Ticks.Parse(DateTime.Parse(target.StopTime.ToString()).Ticks.ToString());
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for SupportsTemporalProcessing
        ///</summary>
        [TestMethod()]
        public void SupportsTemporalProcessingTest()
        {
            bool actual;
            actual = target.SupportsTemporalProcessing;
            Assert.IsTrue(actual);
        }
        #endregion
    }
}