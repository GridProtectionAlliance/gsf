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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.TimeSeries;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for OutputAdapterBaseTest and is intended
    ///to contain all OutputAdapterBaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OutputAdapterBaseTest
    {
        #region [ Members ]
        private IAllAdaptersCase adapterCase;
        private IMeasurementsCase m_IMeasurements;
        private IMeasurementCase measurementCase;
        private IOutputAdapterCase target;
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
            m_IMeasurements.Dispose();
            measurementCase.Dispose();
            target.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            measurementCase = new IMeasurementCase();
            m_IMeasurements = new IMeasurementsCase();
            adapterCase = new IAllAdaptersCase();
            target = new IOutputAdapterCase();
            target.SetInitializedState(true);
            target.Enabled = true;
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for Flush
        ///</summary>
        [TestMethod()]
        public void FlushTest()
        {
            target.Flush();
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
        ///A test for QueueMeasurementsForProcessing
        ///</summary>
        //[TestMethod()]
        //public void QueueMeasurementsForProcessingTest()
        //{
        //    IEnumerable<IMeasurement> measurements = m_IMeasurements.Measurements;
        //    try
        //    {
        //        target.QueueMeasurementsForProcessing(measurements);
        //        Assert.IsTrue(true);
        //    }
        //    catch
        //    {
        //        Assert.IsTrue(false);
        //    }
        //}

        /// <summary>
        ///A test for RequestedInputMeasurementKeys
        ///</summary>
        //[TestMethod()]
        //public void RequestedInputMeasurementKeysTest()
        //{
        //    MeasurementKey[] expected = m_IMeasurements.MeasurementKeys;
        //    MeasurementKey[] actual;
        //    target.RequestedInputMeasurementKeys = expected;
        //    actual = target.RequestedInputMeasurementKeys;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for SupportsTemporalProcessing
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