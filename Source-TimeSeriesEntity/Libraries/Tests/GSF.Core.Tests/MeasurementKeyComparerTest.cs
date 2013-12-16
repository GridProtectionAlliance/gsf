#region [ Code Modification History ]
/*
 * 04/23/2012 Denis Kholine
 *   Generated Original version of source code.
 *
 * 05/22/2012 Denis Kholine
 *   Git repository relocation
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
using GSF;
using GSF.TimeSeries;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for MeasurementKeyComparerTest and is intended
    ///to contain all MeasurementKeyComparerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeasurementKeyComparerTest
    {
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
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for Default
        ///</summary>
        [TestMethod()]
        public void DefaultTest()
        {
            MeasurementKeyComparer actual;
            actual = MeasurementKeyComparer.Default;
            Assert.AreEqual(actual, MeasurementKeyComparer.Default);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            MeasurementKeyComparer target = new MeasurementKeyComparer();
            MeasurementKey x = new MeasurementKey();
            MeasurementKey y = new MeasurementKey();
            bool expected = true;
            bool actual;
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            MeasurementKeyComparer target = new MeasurementKeyComparer();
            MeasurementKey obj = new MeasurementKey();
            int expected = 0;
            int actual;
            actual = target.GetHashCode(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for MeasurementKeyComparer Constructor
        ///</summary>
        [TestMethod()]
        public void MeasurementKeyComparerConstructorTest()
        {
            MeasurementKeyComparer target = new MeasurementKeyComparer();
            Assert.IsInstanceOfType(target, typeof(MeasurementKeyComparer));
            Assert.IsNotNull(target);
        }

        #endregion
    }
}