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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Adapters;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.TimeSeries;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for IaonSessionTest and is intended
    ///to contain all IaonSessionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IaonSessionTest
    {
        #region [ Members ]
        private IAllAdaptersCase m_IAdapterCase;
        private IMeasurementsCase m_IMeasurements;
        private IRealTimeConfigurationCase realTimeConfigurationCase;
        private IaonSession target;
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
            realTimeConfigurationCase.Dispose();
            m_IMeasurements.Dispose();
            m_IAdapterCase.Dispose();
            target.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_IMeasurements = new IMeasurementsCase();
            m_IAdapterCase = new IAllAdaptersCase();
            realTimeConfigurationCase = new IRealTimeConfigurationCase();
            target = new IaonSession();
            target.Initialize(true);
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for ActionAdapters
        ///</summary>
        [TestMethod()]
        public void ActionAdaptersTest()
        {
            ActionAdapterCollection actual;
            actual = target.ActionAdapters;
            Assert.AreEqual(target.ActionAdapters, actual);
        }

        /// <summary>
        ///A test for IaonSession Constructor
        ///</summary>
        [TestMethod()]
        public void IaonSessionConstructorTest()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(IaonSession));
        }

        /// <summary>
        ///A test for InputMeasurementKeysRestriction
        ///</summary>
        //[TestMethod()]
        //public void InputMeasurementKeysRestrictionTest()
        //{
        //    MeasurementKey[] expected = m_IMeasurements.MeasurementKeys;
        //    MeasurementKey[] actual;
        //    target.InputMeasurementKeysRestriction = expected;
        //    actual = target.InputMeasurementKeysRestriction;
        //    Assert.AreEqual(expected, actual);
        //}

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

        /// <summary>
        ///A test for NodeID
        ///</summary>
        [TestMethod()]
        public void NodeIDTest()
        {
            System.Guid expected = new System.Guid();
            System.Guid actual;
            target.NodeID = expected;
            actual = target.NodeID;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for OutputMeasurementsUpdatedHandler
        ///</summary>
        //[TestMethod()]
        //public void OutputMeasurementsUpdatedHandlerTest()
        //{
        //    object sender = new object();
        //    System.EventArgs e = System.EventArgs.Empty;
        //    target.OutputMeasurementsUpdatedHandler(sender, e);
        //}

        /// <summary>
        ///A test for ProcessingCompleteHandler
        ///</summary>
        [TestMethod()]
        public void ProcessingCompleteHandlerTest()
        {
            object sender = new object();
            System.EventArgs e = new System.EventArgs();
            target.ProcessingCompleteHandler(sender, e);
        }

        /// <summary>
        ///A test for RecalculateRoutingTables
        ///</summary>
        [TestMethod()]
        public void RecalculateRoutingTablesTest()
        {
            target.RecalculateRoutingTables();
        }

        /// <summary>
        ///A test for TemporalProcessingSupportExists
        ///</summary>
        [TestMethod()]
        public void TemporalProcessingSupportExistsTest()
        {
            string collection = string.Empty;
            bool expected = false;
            bool actual;
            actual = target.TemporalProcessingSupportExists(collection);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UseMeasurementRouting
        ///</summary>
        //[TestMethod()]
        //public void UseMeasurementRoutingTest()
        //{
        //    bool expected = false;
        //    bool actual;
        //    target.UseMeasurementRouting = expected;
        //    actual = target.UseMeasurementRouting;
        //    Assert.AreEqual(expected, actual);
        //}
        #endregion
    }
}