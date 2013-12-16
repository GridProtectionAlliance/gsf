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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Adapters;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.TimeSeries;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for AdapterCollectionBaseTest and is intended
    ///to contain all AdapterCollectionBaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AdapterCollectionBaseTest
    {
        #region [ Members ]
        private IAllAdaptersCase m_IAdapterUnitTesting;

        private IMeasurementsCase m_IMeasurements;
        private IMeasurementCase measurementCase;
        private AdapterCollectionBase<IAdapter> target;
        private IWaitHandlesCase waitHandlesCase;
        #endregion

        #region [ Properties ]
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
            target.Clear();
            target.Dispose();

            waitHandlesCase.Dispose();
            measurementCase.Dispose();
            m_IMeasurements.Dispose();
            m_IMeasurements.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            //Initialize Wait Handles
            waitHandlesCase = new IWaitHandlesCase();
            //Initialize Measurement
            measurementCase = new IMeasurementCase();
            //Initialize Measurements
            m_IMeasurements = new IMeasurementsCase();
            //Initialize Adapter
            m_IAdapterUnitTesting = new IAllAdaptersCase();

            //Initialize target
            target = new IAdapterCollectionBaseCase(waitHandlesCase.waitHandles);
            target.DataSource = m_IAdapterUnitTesting.InputAdapter.DataSource;
            target.DataMember = m_IAdapterUnitTesting.InputAdapter.DataSource.DataSetName;
            target.Initialize();

            //target.TryCreateAdapter(adapterhelper.adapterRow,out adapter);
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for System.Collections.Generic.ICollection<TimeSeriesFramework.Adapters.IAdapter>.Add
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void AddTest()
        {
            ICollection<IAdapter> target = new List<IAdapter>();
            IAdapter item = new IAdapterBaseCase();
            target.Add(item);
            bool expected = (target.Count > 0);
            Assert.IsTrue(expected);
        } 

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
            string expected = string.Empty;
            string actual;
            target.ConnectionString = expected;
            actual = target.ConnectionString;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.Collections.Generic.ICollection<TimeSeriesFramework.Adapters.IAdapter>.Contains
        ///</summary>
        [TestMethod()]
        public void ContainsTest()
        {
            ICollection<IAdapter> target = new List<IAdapter>();
            IAdapter item = new IAdapterBaseCase();
            bool expected = false;
            bool actual;
            actual = target.Contains(item);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.Collections.Generic.ICollection<TimeSeriesFramework.Adapters.IAdapter>.CopyTo
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void CopyToTest()
        {
            //ICollection<IAdapter> items = new List<IAdapter>();

            IAdapter[] array = new IAdapterBaseCase[1];
            int arrayIndex = 0;
            target.CopyTo(array, arrayIndex);
            Assert.IsNotNull(array);
        }

        /// <summary>
        ///A test for DataMember
        ///</summary>
        [TestMethod()]
        public void DataMemberTest()
        {
            string expected = string.Empty;
            string actual;
            target.DataMember = expected;
            actual = target.DataMember;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DataSource
        ///</summary>
        [TestMethod()]
        public void DataSourceTest()
        {
            System.Data.DataSet expected = m_IAdapterUnitTesting.InputAdapter.DataSource;
            System.Data.DataSet actual;
            target.DataSource = expected;
            actual = target.DataSource;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DisposeTest1()
        {
            target.Dispose();
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
        ///A test for IEnumerable<TimeSeriesFramework.Adapters.IAdapter>.GetEnumerator
        ///</summary>
        [TestMethod()]
        public void GetEnumeratorTest()
        {
            IEnumerable<IAdapter> target = new List<IAdapter>();
            List<IAdapterBaseCase> items = new List<IAdapterBaseCase>();
            items.Add(new IAdapterBaseCase());
            IEnumerator<IAdapter> actual = target.GetEnumerator();

            //IEnumerator<IAdapter> expected = items;
            //<IAdapter> actual;
            //actual = target.GetEnumerator();
            //Assert.AreEqual(expected, actual);
        } 

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

        [TestMethod()]
        public void InitializeTest()
        {
            target.Initialize();
            Assert.IsTrue(target.Initialized);
        }

        /// <summary>
        ///A test for InputMeasurementKeys
        ///</summary>
        [TestMethod()]
        public void InputMeasurementKeysTest()
        {
            MeasurementKey[] expected = target.InputMeasurementKeys;
            MeasurementKey[] actual;
            target.InputMeasurementKeys = expected;
            actual = target.InputMeasurementKeys;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InputSourceIDs
        ///</summary>
        [TestMethod()]
        public void InputSourceIDsTest()
        {
            string[] expected = new string[] { Guid.NewGuid().ToString() };
            string[] actual;
            target.InputSourceIDs = expected;
            actual = target.InputSourceIDs;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.Collections.Generic.IList<TimeSeriesFramework.Adapters.IAdapter>.Insert
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void InsertTest()
        {
            IList<IAdapter> target = new List<IAdapter>();
            int index = 0;
            IAdapter item = new IAdapterBaseCase();
            target.Insert(index, item);
        }

        /// <summary>
        ///A test for IsReadOnly
        ///</summary>
        [TestMethod()]
        public void IsReadOnlyTest()
        {
            bool actual;
            actual = target.IsReadOnly;
        }

        /// <summary>
        ///A test for System.Collections.Generic.IList<TimeSeriesFramework.Adapters.IAdapter>.Item
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void ItemTest()
        {
            IList<IAdapter> target = new List<IAdapter>();
            target.Add(new IAdapterBaseCase());
            int index = 0;
            IAdapter expected = default(IAdapter);
            IAdapter actual;
            target[index] = expected;
            actual = target[index];
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            string expected = string.Empty;
            string actual;
            target.Name = expected;
            actual = target.Name;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void OnProcessExceptionTest()
        {
            System.Exception ex = new System.Exception("Unit Testing Exception");
            target.OnProcessException(ex);
        }

        [TestMethod()]
        public void OnStatusMessageTest1()
        {
            string formattedStatus = string.Empty;
            object[] args = new object[1] { "unit test" };
            target.OnStatusMessage(formattedStatus, args);
        }

        /// <summary>
        ///A test for OutputMeasurements
        ///</summary>
        [TestMethod()]
        public void OutputMeasurementsTest()
        {
            IMeasurement[] expected = new IMeasurement[1] { measurementCase.Measurement };
            IMeasurement[] actual;
            target.OutputMeasurements = expected;
            actual = target.OutputMeasurements;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for OutputSourceIDs
        ///</summary>
        [TestMethod()]
        public void OutputSourceIDsTest()
        {
            string[] expected = new string[1] { Guid.NewGuid().ToString() };
            string[] actual;
            target.OutputSourceIDs = expected;
            actual = target.OutputSourceIDs;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ProcessedMeasurements
        ///</summary>
        [TestMethod()]
        public void ProcessedMeasurementsTest()
        {
            long actual;
            actual = target.ProcessedMeasurements;
        }

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
        [TestMethod()]
        public void ProcessMeasurementFilterTest()
        {
            bool expected = false;
            bool actual;
            target.ProcessMeasurementFilter = expected;
            actual = target.ProcessMeasurementFilter;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.Collections.Generic.ICollection<TimeSeriesFramework.Adapters.IAdapter>.Remove
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void RemoveTest()
        {
            ICollection<IAdapter> target = new List<IAdapter>();
            IAdapter item = new IAdapterBaseCase();
            target.Add(item);
            bool expected = true;
            bool actual;
            actual = target.Remove(item);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RequestedInputMeasurementKeys
        ///</summary>
        [TestMethod()]
        public void RequestedInputMeasurementKeysTest()
        {
            MeasurementKey[] expected = m_IMeasurements.MeasurementKeys;
            MeasurementKey[] actual;
            target.RequestedInputMeasurementKeys = expected;
            actual = target.RequestedInputMeasurementKeys;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RequestedOutputMeasurementKeys
        ///</summary>
        [TestMethod()]
        public void RequestedOutputMeasurementKeysTest()
        {
            MeasurementKey[] expected = m_IMeasurements.MeasurementKeys;
            MeasurementKey[] actual;
            target.RequestedOutputMeasurementKeys = expected;
            actual = target.RequestedOutputMeasurementKeys;
            Assert.AreEqual(expected, actual);
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
        ///A test for SetTemporalConstraint
        ///</summary>
        [TestMethod()]
        public void SetTemporalConstraintTest()
        {
            string startTime = DateTime.UtcNow.ToString();
            string stopTime = DateTime.UtcNow.ToString();
            string constraintParameters = "Unit Tests";
            target.SetTemporalConstraint(startTime, stopTime, constraintParameters);
            bool expected = target.TemporalConstraintIsDefined();
            Assert.IsTrue(expected);
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
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            try
            {
                target.Start();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
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
        ///A test for Status
        ///</summary>
        [TestMethod()]
        public void StatusTest()
        {
            string actual;
            actual = target.Status;
        }

        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod()]
        public void StopTest()
        {
            target.Stop();
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
        ///A test for SupportsTemporalProcessing
        ///</summary>
        [TestMethod()]
        public void SupportsTemporalProcessingTest()
        {
            bool actual;
            actual = target.SupportsTemporalProcessing;
        }

        /// <summary>
        ///A test for TryCreateAdapter
        ///</summary>
        [TestMethod()]
        public void TryCreateAdapterTest1()
        {
            System.Data.DataRow adapterRow = m_IAdapterUnitTesting.InputAdapter.DataSource.Tables["InputAdapter"].Rows[0];
            IAdapter adapter = default(IAdapter);
            IAdapter adapterExpected = default(IAdapter);
            bool expected = false;
            bool actual;
            actual = target.TryCreateAdapter(adapterRow, out adapter);
            Assert.AreEqual(adapterExpected, adapter);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TryGetAdapterByID
        ///</summary>
        [TestMethod()]
        public void TryGetAdapterByIDTest1()
        {
            uint id = 0;
            IAdapter adapter = default(IAdapter);
            IAdapter adapterExpected = default(IAdapter);
            bool expected = false;
            bool actual;
            actual = target.TryGetAdapterByID(id, out adapter);
            Assert.AreEqual(adapterExpected, adapter);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TryGetAdapterByName
        ///</summary>
        [TestMethod()]
        public void TryGetAdapterByNameTest1()
        {
            string name = string.Empty;
            IAdapter adapter = default(IAdapter);
            IAdapter adapterExpected = default(IAdapter);
            bool expected = false;
            bool actual;
            actual = target.TryGetAdapterByName(name, out adapter);
            Assert.AreEqual(adapterExpected, adapter);
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}