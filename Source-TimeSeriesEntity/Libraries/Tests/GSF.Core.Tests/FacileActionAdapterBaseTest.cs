#region [ Code Modification History ]
/*
 *  06/22/2012 Denis Kholine
 *    Generated original version of source code
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
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries;
using GSF;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for FacileActionAdapterBaseTest and is intended
    ///to contain all FacileActionAdapterBaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FacileActionAdapterBaseTest
    {
        #region [ Members ]
        private Measurement measurement;
        private MeasurementKey measurementkey;
        private List<Measurement> measurements;
        private FacileActionAdapterWrapper target;
        #endregion

        #region [ Properties ]
        public uint id
        {
            get
            {
                return 10;
            }
        }

        public Guid SignalID
        {
            get
            {
                return Guid.NewGuid();
            }
        }

        public string source
        {
            get
            {
                return "UnitTesting";
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
        //

        /// <summary>
        ///A test for FramesPerSecond
        ///</summary>
        [TestMethod()]
        public void FramesPerSecondTest()
        {
            int expected = 33;
            int actual;
            target.FramesPerSecond = expected;
            actual = target.FramesPerSecond;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            target.Initialize();
            target.Initialized = true;
            Assert.IsTrue(target.Initialized);
        }

        /// <summary>
        ///A test for InputMeasurementKeys
        ///</summary>
        [TestMethod()]
        public void InputMeasurementKeysTest()
        {
            MeasurementKey[] expected = new MeasurementKey[] { measurementkey };
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
            string[] expected = new string[] { source };
            string[] actual;
            target.InputSourceIDs = expected;
            actual = target.InputSourceIDs;
            if (actual.Count() == expected.Count() && actual.Count() > 0)
            {
                for (int i = 0; i < actual.Count(); i++)
                {
                    Assert.AreEqual(actual.ElementAt(i), expected.ElementAt(i));
                }
            }
            else
            {
            }
        }

        public bool IsDateTime(Ticks ticks, out DateTime datetime)
        {
            datetime = new DateTime(ticks);
            return datetime.LocalTimeIsValid(1, 1);
        }

        /// <summary>
        ///A test for LatestMeasurements
        ///</summary>
        [TestMethod()]
        public void LatestMeasurementsTest()
        {
            AdapterInitializationMeasurement aim = new AdapterInitializationMeasurement();
            ImmediateMeasurements actual;
            target.TrackLatestMeasurements = true;
            target.Initialize();//required for successful test initialization
            target.QueueMeasurementForProcessing(aim.Measurement);
            actual = target.LatestMeasurements;
            bool expected = (actual.ElementAt(0).ID == aim.Measurement.ID);
            Assert.IsTrue(expected);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            target.Dispose();
            measurements.Clear();
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new FacileActionAdapterWrapper();
            measurement = new Measurement();
            measurements = new List<Measurement>();
            measurements.Add(measurement);
            measurementkey = new MeasurementKey(SignalID, id, source);
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for OutputSourceIDs
        ///</summary>
        [TestMethod()]
        public void OutputSourceIDsTest()
        {
            string[] expected = new string[] { source };
            string[] actual;
            target.OutputSourceIDs = expected;
            actual = target.OutputSourceIDs;
            bool result = (actual.ElementAt(0) == expected.ElementAt(0));
            Assert.IsTrue(result);
        }

        /// <summary>
        ///A test for QueueMeasurementForProcessing
        ///</summary>
        [TestMethod()]
        public void QueueMeasurementForProcessingTest()
        {
            target.QueueMeasurementForProcessing(measurement);
        }

        /// <summary>
        ///A test for QueueMeasurementsForProcessing
        ///</summary>
        [TestMethod()]
        public void QueueMeasurementsForProcessingTest()
        {
            target.QueueMeasurementsForProcessing(measurements);
        }

        /// <summary>
        ///A test for RealTime
        ///</summary>
        [TestMethod()]
        public void RealTimeTest()
        {
            DateTime now = DateTime.Now;
            Ticks actual = new Ticks(now);
            actual = target.RealTime;
        }

        /// <summary>
        ///A test for RequestedInputMeasurementKeys
        ///</summary>
        [TestMethod()]
        public void RequestedInputMeasurementKeysTest()
        {
            MeasurementKey[] expected = new MeasurementKey[1];
            expected[0] = measurement.Key;
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
            MeasurementKey item = new MeasurementKey();
            MeasurementKey[] expected = new MeasurementKey[1] { item };
            MeasurementKey[] actual;
            target.RequestedOutputMeasurementKeys = expected;
            actual = target.RequestedOutputMeasurementKeys;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RespectInputDemands
        ///</summary>
        [TestMethod()]
        public void RespectInputDemandsTest()
        {
            bool expected = false;
            bool actual;
            target.RespectInputDemands = expected;
            actual = target.RespectInputDemands;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RespectOutputDemands
        ///</summary>
        [TestMethod()]
        public void RespectOutputDemandsTest()
        {
            bool expected = false;
            bool actual;
            target.RespectOutputDemands = expected;
            actual = target.RespectOutputDemands;
            Assert.AreEqual(expected, actual);
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
        ///A test for TrackLatestMeasurements
        ///</summary>
        [TestMethod()]
        public void TrackLatestMeasurementsTest()
        {
            bool expected = false;
            bool actual;
            target.TrackLatestMeasurements = expected;
            actual = target.TrackLatestMeasurements;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UseLocalClockAsRealTime
        ///</summary>
        [TestMethod()]
        public void UseLocalClockAsRealTimeTest()
        {
            bool expected = false;
            bool actual;
            target.UseLocalClockAsRealTime = expected;
            actual = target.UseLocalClockAsRealTime;
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }

    /// <summary>
    /// Adapter Data Table Initialization
    /// </summary>
    internal class AdapterInitializationDataTable : IDisposable
    {
        #region [ Members ]
        private bool isDisposed = false;
        private DataColumn[] m_DataColumns;
        private DataRow m_DataRow;
        private DataSet m_DataSet;
        private DataTable m_DataTable;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Constructs in memory data set as required by ActionAdapterBase abstract class
        /// to successfully initialize adapter
        /// </summary>
        public AdapterInitializationDataTable()
        {
            m_DataColumns = new DataColumn[5];
            m_DataColumns[0] = new DataColumn("ID");
            m_DataColumns[1] = new DataColumn("AdapterName");
            m_DataColumns[2] = new DataColumn("AssemblyName");
            m_DataColumns[3] = new DataColumn("TypeName");
            m_DataColumns[4] = new DataColumn("ConnectinString");

            m_DataTable = new DataTable("ActionAdapters");
            m_DataTable.Columns.AddRange(m_DataColumns);
            m_DataRow = m_DataTable.NewRow();
            m_DataRow["ID"] = Guid.NewGuid().ToString();
            m_DataRow["AdapterName"] = "UnitTestingAdapter";
            m_DataRow["AssemblyName"] = "PhasorProtocols.dll";
            m_DataRow["TypeName"] = "PhasorProtocols.IeeeC37_118.Concentrator";
            m_DataRow["ConnectinString"] = "requireAuthentication=false; allowSynchronizedSubscription=false; useBaseTimeOffsets=true";

            m_DataTable.Rows.Add(m_DataRow);
            m_DataSet = new DataSet("UnitTesting");
            m_DataSet.Tables.Add(m_DataTable);
        }

        ~AdapterInitializationDataTable()
        {
            Dispose(false);
        }

        public DataSet DataSource
        {
            get
            {
                return m_DataSet;
            }
        }

        #endregion

        #region [ Constructors ]
        #endregion

        #region [ Dispose ]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    m_DataSet.Dispose();
                }
                isDisposed = false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Adapter Measurement Initialization
    /// </summary>
    internal class AdapterInitializationMeasurement
    {
        #region [ Members ]
        private bool isDisposed = false;
        private uint m_id = 10;
        private Guid m_ID = Guid.NewGuid();
        private IMeasurement m_measurement;
        private Guid m_SignalID = Guid.NewGuid();
        private string m_Source = "TestCase";
        private MeasurementStateFlags m_StateFlag;
        private string m_TagName = "UnitTesting";
        private Ticks m_ticks;
        private DateTime m_UtcNow;
        private double m_Value = 120.40;
        #endregion

        #region [ Properties ]
        public AdapterInitializationMeasurement()
        {
            m_measurement = new Measurement();
            m_StateFlag = new MeasurementStateFlags();
            m_StateFlag = MeasurementStateFlags.Normal;
            m_UtcNow = new DateTime();
            m_UtcNow = DateTime.UtcNow;

            m_measurement.ID = m_ID;
            m_measurement.Key = new MeasurementKey(m_SignalID, m_id, m_Source);
            m_measurement.Value = m_Value;
            m_measurement.TagName = m_TagName;
            m_measurement.StateFlags = m_StateFlag;
            m_ticks = new Ticks(m_UtcNow);
            m_measurement.Timestamp = m_ticks;
        }

        ~AdapterInitializationMeasurement()
        {
            Dispose(false);
        }

        public IMeasurement Measurement
        {
            get
            {
                return m_measurement;
            }
        }

        #endregion

        #region [ Constructors ]
        #endregion

        #region [ Dispose ]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                }
                isDisposed = false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Class Wrapper
    /// </summary>
    internal class FacileActionAdapterWrapper : FacileActionAdapterBase
    {
        #region [ Constructors ]
        public FacileActionAdapterWrapper()
            : base()
        { }

        #endregion

        #region [ Methods ]
        public override bool SupportsTemporalProcessing
        {
            get { return true; }
        }

        public override string GetShortStatus(int maxLength)
        {
            return maxLength.ToString();
        }

        #endregion
    }
}