#region [ Modification History ]
/*
 * 06/07/2012 Denis Kholine
 *  Generated Original version of source code.
 *
 * 06/11/2012 Denis Kholine
 *   Add initializatoin parameters into test case
 *
 * 07/02/2012 Denis Kholine
 *   Remove references to test suite.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for ActionAdapterCollectionTest and is intended
    ///to contain all ActionAdapterCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ActionAdapterCollectionTest
    {
        #region [ Classes ]
        /// <summary>
        /// Adapter Data Table Initialization
        /// </summary>
        private class AdapterInitializationHelper : IDisposable
        {
            #region [ Members ]
            private DataColumn[] m_DataColumns;
            private DataRow m_DataRow;
            private DataSet m_DataSet;
            private DataTable m_DataTable;
            #endregion

            #region [ Properties ]
            public DataRow adapterRow
            {
                get
                {
                    return m_DataRow;
                }
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
            /// <summary>
            /// Constructs in memory data set as required by ActionAdapterBase abstract class
            /// to successfully initialize adapter
            /// </summary>
            public AdapterInitializationHelper()
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
                m_DataSet = new DataSet("ActionAdapters");
                m_DataSet.Tables.Add(m_DataTable);
            }

            #endregion

            #region [ Dispose ]
            private bool isDisposed = false;

            ~AdapterInitializationHelper()
            {
                Dispose(false);
            }

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
                        m_DataTable.Clear();
                        m_DataTable.Dispose();
                        m_DataSet.Dispose();
                    }
                    isDisposed = false;
                }
            }

            #endregion
        }
        /// <summary>
        /// Measurement<double> Key Helper
        /// </summary>
        private class MeasurementKeyHelper : IDisposable
        {
            #region [ Static ]
            private static Guid m_guid = new Guid("3647f729-d0ed-4f79-85ad-dae2149cd432");
            private static Ticks m_ticks = DateTime.Now.Ticks;
            private static double m_value = 10;
            private static MeasurementStateFlags m_flags = MeasurementStateFlags.Normal;
            #endregion

            #region [ Members ]
            private DateTime datetime1;
            private Measurement<double> measurement1;
            private MeasurementKey measurementkey1;
            private Guid signalid1;

            #endregion

            #region [ Properties ]
            public Measurement<double> Measurement
            {
                get
                {
                    return measurement1;
                }
            }

            public MeasurementKey MeasurementKey
            {
                get
                {
                    return measurementkey1;
                }
            }

            public Guid SignalID
            {
                get
                {
                    return signalid1;
                }
            }

            #endregion

            #region [ Constructors ]
            public MeasurementKeyHelper()
            {
              measurement1 = new Measurement<double>(m_guid, m_ticks,m_flags, m_value);
            }
            #endregion

            #region [ Dispose ]
            private bool isDisposed = false;

            ~MeasurementKeyHelper()
            {
                Dispose(false);
            }

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
        /// Wait Handles Helper
        /// </summary>
        private class WaitHandlesHelper : IDisposable
        {
            #region [ Members ]
            private AutoResetEvent m_ARE;
            private string m_Key;
            private ConcurrentDictionary<string, AutoResetEvent> m_waitHandles;
            #endregion

            #region [ Properties ]
            public AutoResetEvent AutoResetEvent
            {
                get
                {
                    return m_ARE;
                }
            }

            public string Key
            {
                get
                {
                    return m_Key;
                }
            }

            public ConcurrentDictionary<string, AutoResetEvent> waitHandles
            {
                get
                {
                    return m_waitHandles;
                }
            }

            #endregion

            #region [ Constructors ]
            public WaitHandlesHelper()
            {
                m_Key = Guid.NewGuid().ToString();
                m_waitHandles = new ConcurrentDictionary<string, AutoResetEvent>();
                m_ARE = new AutoResetEvent(false);

                Func<string, AutoResetEvent> m_func_ARE = r =>
                {
                    return m_ARE;
                };

                Func<string, AutoResetEvent, AutoResetEvent> m_func_ARE_Updater = (in1, in2) =>
                {
                    m_ARE.Set();
                    return m_ARE;
                };

                m_waitHandles.AddOrUpdate(m_Key, m_func_ARE(m_Key),
                (string Key, AutoResetEvent m_ResetEvt) =>
                {
                    m_ARE.Set();
                    m_ARE.Dispose();
                    m_ARE = new AutoResetEvent(false);
                    return m_ARE;
                });
            }

            #endregion

            #region [ Dispose ]
            private bool isDisposed = false;

            ~WaitHandlesHelper()
            {
                Dispose(false);
            }

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
                        m_waitHandles.Clear();
                    }
                    isDisposed = false;
                }
            }

            #endregion
        }

        #endregion

        #region [ Members ]
        private AdapterInitializationHelper adapterinitializationhelper;
        private MeasurementKeyHelper measurementhelper;
        private WaitHandlesHelper waithandleshelper;
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
            waithandleshelper.Dispose();
            adapterinitializationhelper.Dispose();
            measurementhelper.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            waithandleshelper = new WaitHandlesHelper();
            adapterinitializationhelper = new AdapterInitializationHelper();
            measurementhelper = new MeasurementKeyHelper();
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for ActionAdapterCollection Constructor
        ///</summary>
        [TestMethod()]
        public void ActionAdapterCollectionConstructorTest()
        {
            ActionAdapterCollection target = new ActionAdapterCollection(false); //waithandleshelper.waitHandles);
            Assert.IsInstanceOfType(target, typeof(ActionAdapterCollection));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for ActionAdapterCollection Constructor
        ///</summary>
        [TestMethod()]
        public void ActionAdapterCollectionConstructorTest1()
        {
            ActionAdapterCollection target = new ActionAdapterCollection(false);
            Assert.IsInstanceOfType(target, typeof(ActionAdapterCollection));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            ActionAdapterCollection target = new ActionAdapterCollection(false);
            target.DataSource = adapterinitializationhelper.DataSource;
            target.Initialize();
            bool expected = target.Initialized;
            Assert.IsTrue(target.Initialized);
        }

        /// <summary>
        ///A test for QueueMeasurementsForProcessing
        ///</summary>
        //[TestMethod()]
        //public void QueueMeasurementsForProcessingTest()
        //{
        //    List<Measurement<double>> items = new List<Measurement<double>>();
        //    items.Add(measurementhelper.Measurement);
        //    ActionAdapterCollection target = new ActionAdapterCollection(false);
        //    IEnumerable<IMeasurement> measurements = new List<IMeasurement>(items);
        //    target.QueueMeasurementsForProcessing(measurements);
        //}

        /// <summary>
        ///A test for RespectInputDemands
        ///</summary>
        //[TestMethod()]
        //public void RespectInputDemandsTest()
        //{
        //    ActionAdapterCollection target = new ActionAdapterCollection(false);
        //    bool expected = false;
        //    bool actual;
        //    target.RespectInputDemands = expected;
        //    actual = target.RespectInputDemands;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for RespectOutputDemands
        ///</summary>
        //[TestMethod()]
        //public void RespectOutputDemandsTest()
        //{
        //    ActionAdapterCollection target = new ActionAdapterCollection(false);
        //    bool expected = false;
        //    bool actual;
        //    target.RespectOutputDemands = expected;
        //    actual = target.RespectOutputDemands;
        //    Assert.AreEqual(expected, actual);
        //}
        #endregion
    }
}