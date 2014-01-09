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

#region [ Code Using ]
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries;
using GSF;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for MeasurementStateMappingExtensionsTest and is intended
    ///to contain all MeasurementStateMappingExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeasurementStateMappingExtensionsTest
    {
        #region [ Members ]
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
        ///A test for MapToCompactFlags
        ///</summary>
        [TestMethod()]
        public void MapToCompactFlagsTest()
        {
            MeasurementStateFlags stateFlags = new MeasurementStateFlags();
            CompactMeasurementStateFlags expected = new CompactMeasurementStateFlags();
            CompactMeasurementStateFlags actual;
            actual = MeasurementStateMappingExtensions.MapToCompactFlags(stateFlags);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for MapToFullFlags
        ///</summary>
        [TestMethod()]
        public void MapToFullFlagsTest()
        {
            CompactMeasurementStateFlags stateFlags = new CompactMeasurementStateFlags();
            MeasurementStateFlags expected = new MeasurementStateFlags();
            MeasurementStateFlags actual;
            actual = MeasurementStateMappingExtensions.MapToFullFlags(stateFlags);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }

        #endregion
    }
}