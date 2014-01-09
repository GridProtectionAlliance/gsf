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
using GSF.TimeSeries.Adapters;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.TimeSeries;
using GSF;
using CsvAdapters;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for OutputAdapterCollectionTest and is intended
    ///to contain all OutputAdapterCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OutputAdapterCollectionTest
    {
        #region [ Members ]
        private IMeasurementsCase m_IMeasurements;
        private OutputAdapterCollection target;
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
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            target.Clear();
            target.Dispose();
            waitHandlesCase.Dispose();
            m_IMeasurements.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            waitHandlesCase = new IWaitHandlesCase();
            m_IMeasurements = new IMeasurementsCase();
            target = new OutputAdapterCollection(false); // (waitHandlesCase.waitHandles);
            //TODO
            //target.Add(new CsvOutputAdapter());
        }

        #endregion

        /// <summary>
        ///A test for OutputAdapterCollection Constructor
        ///</summary>
        [TestMethod()]
        public void OutputAdapterCollectionConstructorTest()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(OutputAdapterCollection));
        }

        /// <summary>
        ///A test for OutputAdapterCollection Constructor
        ///</summary>
        [TestMethod()]
        public void OutputAdapterCollectionConstructorTest1()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for OutputIsForArchive
        ///</summary>
        [TestMethod()]
        public void OutputIsForArchiveTest()
        {
            bool actual;
            actual = target.OutputIsForArchive;
        }

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
        ///A test for QueueMeasurementsForProcessing
        ///</summary>
        //[TestMethod()]
        //public void QueueMeasurementsForProcessingTest()
        //{
        //    IEnumerable<IMeasurement> measurements = m_IMeasurements.ReadOnlyMeasurements;
        //    target.QueueMeasurementsForProcessing(measurements);
        //    bool expected = (target.ProcessedMeasurements > 0);
        //    Assert.IsTrue(expected);
        //}

        /// <summary>
        ///A test for RemoveMeasurements
        ///</summary>
        //[TestMethod()]
        //public void RemoveMeasurementsTest()
        //{
        //    int total = 1;
        //    target.RemoveMeasurements(total);
        //    Assert.IsTrue(target.ProcessedMeasurements == 0);
        //}
    }
}