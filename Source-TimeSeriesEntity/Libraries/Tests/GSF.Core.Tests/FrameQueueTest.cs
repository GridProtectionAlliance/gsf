#region [ Code Modification History ]
/*
 * 4/23/2012 Denis Kholine
 *   Generated Original version of source code.
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
using GSF;


#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for FrameQueueTest and is intended
    ///to contain all FrameQueueTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FrameQueueTest
    {
        #region [ Delegates ]
        private IFrame MyFrame(Ticks timestamp)
        {
            IFrame frame1 = new Frame(timestamp, -1);
            return frame1;
        }

        #endregion [ Delegates ]

        #region [ Members ]
        private bool expected;
        private FrameQueue target;
        private Ticks timestamp;

        #endregion

        #region [ Context ]
        private TestContext testContextInstance;

        ///<summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
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
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            expected = false;
            timestamp = new Ticks(new DateTime(1000));
            target = new FrameQueue(new FrameQueue.CreateNewFrameFunction(MyFrame));
            target.FramesPerSecond = 30;
            target.TimeResolution = Ticks.PerMillisecond;
            target.DownsamplingMethod = DownsamplingMethod.BestQuality;
            target.GetFrame(Ticks.PerSecond);
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for Clear
        ///</summary>
        [TestMethod()]
        public void ClearTest()
        {
            target.Clear();
            expected = (target.Count == 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for Count
        ///</summary>
        [TestMethod()]
        public void CountTest()
        {
            expected = (target.Count == 1);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            try
            {
                target.Dispose();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for DownsamplingMethod
        ///</summary>
        [TestMethod()]
        public void DownsamplingMethodTest()
        {
            DownsamplingMethod expected = new DownsamplingMethod();
            DownsamplingMethod actual;
            target.DownsamplingMethod = expected;
            actual = target.DownsamplingMethod;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetFrame
        ///</summary>
        [TestMethod()]
        public void GetFrameTest()
        {
            long ticks = DateTime.UtcNow.Ticks;
            Frame item = new Frame(ticks, -1);
            TrackingFrame expected = new TrackingFrame(item, DownsamplingMethod.Closest);
            TrackingFrame actual;
            actual = target.GetFrame(ticks);
            bool result = (actual.DownsampledMeasurements == expected.DownsampledMeasurements);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///A test for Last
        ///</summary>
        [TestMethod()]
        public void LastTest()
        {
            expected = (target.Last == null);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for Pop
        ///</summary>
        [TestMethod()]
        public void PopTest()
        {
            target.Pop();
            expected = (target.Head == null);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for TimeResolution
        ///</summary>
        [TestMethod()]
        public void TimeResolutionTest()
        {
            long expected = 0;
            long actual;
            target.TimeResolution = expected;
            actual = target.TimeResolution;
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}