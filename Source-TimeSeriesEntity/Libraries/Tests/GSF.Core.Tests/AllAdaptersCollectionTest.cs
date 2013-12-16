#region [ Code Modification History ]
/*
 *  06/22/2012 Denis Kholine
 *   Generated original version of source code
 *
 *  07/02/2012 Denis Kholine
 *   Add code coverage statistics
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
using System.Data;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Adapters;

#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for AllAdaptersCollectionTest and is intended
    ///to contain all AllAdaptersCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AllAdaptersCollectionTest
    {
        #region [ Members ]
        private AutoResetEvent m_ARE;
        private DataColumn[] m_DataColumns;
        private DataRow m_DataRow;
        private DataSet m_DataSet;
        private DataTable m_DataTable;
        private string m_Key;
        private ConcurrentDictionary<string, AutoResetEvent> m_waitHandles;
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
        /// <summary>
        ///A test for AllAdaptersCollection Constructor
        ///</summary>
        [TestMethod()]
        public void AllAdaptersCollectionConstructorTest()
        {
            ConcurrentDictionary<string, AutoResetEvent> waitHandles = m_waitHandles;
            AllAdaptersCollection target = new AllAdaptersCollection(); //waitHandles);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(AllAdaptersCollection));
        }

        /// <summary>
        ///A test for AllAdaptersCollection Constructor
        ///</summary>
        [TestMethod()]
        public void AllAdaptersCollectionConstructorTest1()
        {
            AllAdaptersCollection target = new AllAdaptersCollection();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(AllAdaptersCollection));
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            AllAdaptersCollection target = new AllAdaptersCollection();
            target.Initialize();
            Assert.IsTrue(target.Initialized);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            m_DataSet.Dispose();
            m_waitHandles.Clear();
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
        /// Initialization
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Key = Guid.NewGuid().ToString();
            m_DataColumns = new DataColumn[5];
            m_DataColumns[0] = new DataColumn("ID");
            m_DataColumns[1] = new DataColumn("AdapterName");
            m_DataColumns[2] = new DataColumn("AssemblyName");
            m_DataColumns[3] = new DataColumn("TypeName");
            m_DataColumns[4] = new DataColumn("ConnectinString");

            m_DataTable = new DataTable("ActionAdapters");
            m_DataTable.Columns.AddRange(m_DataColumns);
            m_DataRow = m_DataTable.NewRow();
            m_DataRow["ID"] = m_Key;
            m_DataRow["AdapterName"] = "UnitTestingAdapter";
            m_DataRow["AssemblyName"] = "PhasorProtocols.dll";
            m_DataRow["TypeName"] = "PhasorProtocols.IeeeC37_118.Concentrator";
            m_DataRow["ConnectinString"] = "requireAuthentication=false; allowSynchronizedSubscription=false; useBaseTimeOffsets=true";

            m_DataTable.Rows.Add(m_DataRow);
            m_DataSet = new DataSet("UnitTesting");
            m_DataSet.Tables.Add(m_DataTable);

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

        #region [ Methods ]
        /// <summary>
        ///A test for TryInitializeAdapterByID
        ///</summary>
        [TestMethod()]
        public void TryInitializeAdapterByIDTest()
        {
            AllAdaptersCollection target = new AllAdaptersCollection();
            uint id = 0;
            bool expected = false;
            bool actual;
            actual = target.TryInitializeAdapterByID(id);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UpdateCollectionConfigurations
        ///</summary>
        [TestMethod()]
        public void UpdateCollectionConfigurationsTest()
        {
            AllAdaptersCollection target = new AllAdaptersCollection();
            try
            {
                target.UpdateCollectionConfigurations();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        #endregion
    }
}