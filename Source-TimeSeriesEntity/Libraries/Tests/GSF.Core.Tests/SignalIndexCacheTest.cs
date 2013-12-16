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
using System.Collections.Concurrent;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for SignalIndexCacheTest and is intended
    ///to contain all SignalIndexCacheTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SignalIndexCacheTest
    {
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

        #region [ Members ]
        private AdapterInitializationHelper adapterinitializationhelper;
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
            adapterinitializationhelper.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            adapterinitializationhelper = new AdapterInitializationHelper();
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for AuthorizedSignalIDs
        ///</summary>
        [TestMethod()]
        public void AuthorizedSignalIDsTest()
        {
            SignalIndexCache target = new SignalIndexCache();
            System.Guid[] actual;
            actual = target.AuthorizedSignalIDs;
        }

        /// <summary>
        ///A test for BinaryLength
        ///</summary>
        [TestMethod()]
        public void BinaryLengthTest()
        {
            SignalIndexCache signalIndexCache = new SignalIndexCache();
            bool includeTime = false;
            long[] baseTimeOffsets = new long[8];
            int timeIndex = 10;
            bool useMillisecondResolution = false;
            CompactMeasurement target = new CompactMeasurement(signalIndexCache, includeTime, baseTimeOffsets, timeIndex, useMillisecondResolution);
            int actual;
            actual = target.BinaryLength;
        }

        /// <summary>
        ///A test for Encoding
        ///</summary>
        [TestMethod()]
        public void EncodingTest()
        {
            SignalIndexCache target = new SignalIndexCache();
            System.Text.Encoding expected = System.Text.Encoding.ASCII;
            System.Text.Encoding actual;
            target.Encoding = expected;
            actual = target.Encoding;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetSignalIndex
        ///</summary>
        [TestMethod()]
        public void GetSignalIndexTest()
        {
            SignalIndexCache target = new SignalIndexCache();
            System.Guid signalID = new System.Guid();
            ushort expected = 0;
            ushort actual;
            actual = target.GetSignalIndex(signalID);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for MaximumIndex
        ///</summary>
        [TestMethod()]
        public void MaximumIndexTest()
        {
            SignalIndexCache target = new SignalIndexCache();
            ushort actual;
            actual = target.MaximumIndex;
        }

        /// <summary>
        ///A test for Reference
        ///</summary>
        [TestMethod()]
        public void ReferenceTest()
        {
            SignalIndexCache target = new SignalIndexCache();
            ConcurrentDictionary<ushort, System.Tuple<System.Guid, string, uint>> expected = new ConcurrentDictionary<ushort, Tuple<Guid, string, uint>>();
            ConcurrentDictionary<ushort, System.Tuple<System.Guid, string, uint>> actual;
            target.Reference = expected;
            actual = target.Reference;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SignalIndexCache Constructor
        ///</summary>
        [TestMethod()]
        public void SignalIndexCacheConstructorTest1()
        {
            try
            {
                SignalIndexCache target = new SignalIndexCache();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for SubscriberID
        ///</summary>
        [TestMethod()]
        public void SubscriberIDTest()
        {
            SignalIndexCache target = new SignalIndexCache();
            System.Guid expected = new System.Guid();
            System.Guid actual;
            target.SubscriberID = expected;
            actual = target.SubscriberID;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UnauthorizedSignalIDs
        ///</summary>
        [TestMethod()]
        public void UnauthorizedSignalIDsTest()
        {
            SignalIndexCache target = new SignalIndexCache();
            System.Guid[] expected = new Guid[1] { Guid.NewGuid() };
            System.Guid[] actual;
            target.UnauthorizedSignalIDs = expected;
            actual = target.UnauthorizedSignalIDs;
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}