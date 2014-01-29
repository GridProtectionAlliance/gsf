#region [ Modification History ]
/*
 * 03/05/2013 Denis Kholine
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
using GSF.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
#endregion

namespace GSF.PhasorProtocols.Tests
{

    /// <summary>
    ///This is a test class for HeaderFrameTest and is intended
    ///to contain all HeaderFrameTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HeaderFrameTest
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

        #region [ Additional test attributes ]
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        //
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for HeaderFrame Constructor
        ///</summary>
        [TestMethod()]
        public void HeaderFrameConstructorTest1()
        {
            string headerData = "HeaderUnitTesting";
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame(headerData);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(HeaderFrame));

        }

        /// <summary>
        ///A test for HeaderFrame Constructor
        ///</summary>
        [TestMethod()]
        public void HeaderFrameConstructorTest2()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(HeaderFrame));

        }

        /// <summary>
        ///A test for GetObjectData
        ///</summary>
        [TestMethod()]
        public void GetObjectDataTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            System.Runtime.Serialization.FormatterConverter converter = new System.Runtime.Serialization.FormatterConverter();
            System.Runtime.Serialization.SerializationInfo info = new System.Runtime.Serialization.SerializationInfo(typeof(HeaderFrame),converter);
            System.Runtime.Serialization.StreamingContext context = new System.Runtime.Serialization.StreamingContext();
            try
            {
                target.GetObjectData(info, context);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for Attributes
        ///</summary>
        [TestMethod()]
        public void AttributesTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            System.Collections.Generic.Dictionary<string, string> actual;
            actual = target.Attributes;
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Count > 0);
        }

        /// <summary>
        ///A test for CommonHeader
        ///</summary>
        [TestMethod()]
        public void CommonHeaderTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            GSF.PhasorProtocols.IEEEC37_118.CommonFrameHeader expected = new CommonFrameHeader(FrameType.ConfigurationFrame1,target.IDCode,target.TimeBase);
            GSF.PhasorProtocols.IEEEC37_118.CommonFrameHeader actual;
            target.CommonHeader = expected;
            actual = target.CommonHeader;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for GSF.Parsing.ISupportFrameImage<GSF.PhasorProtocols.IEEEC37_118.FrameType>.CommonHeader
        ///TODO: common header initialization
        ///</summary>
        [TestMethod()]
        [DeploymentItem("GSF.PhasorProtocols.dll")]
        public void CommonHeaderTest1()
        {
            GSF.Parsing.ISupportFrameImage<GSF.PhasorProtocols.IEEEC37_118.FrameType> target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            GSF.Parsing.ICommonHeader<GSF.PhasorProtocols.IEEEC37_118.FrameType> expected = target.CommonHeader;
            GSF.Parsing.ICommonHeader<GSF.PhasorProtocols.IEEEC37_118.FrameType> actual;
            target.CommonHeader = expected;
            actual = target.CommonHeader;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for TimeBase
        ///</summary>
        [TestMethod()]
        public void TimeBaseTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            uint actual;
            actual = target.TimeBase;
            Assert.AreEqual((uint)100000, actual);
        }

        /// <summary>
        ///A test for TimeQualityFlags
        ///</summary>
        [TestMethod()]
        public void TimeQualityFlagsTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            GSF.PhasorProtocols.IEEEC37_118.TimeQualityFlags expected = new GSF.PhasorProtocols.IEEEC37_118.TimeQualityFlags();
            GSF.PhasorProtocols.IEEEC37_118.TimeQualityFlags actual;
            target.TimeQualityFlags = expected;
            actual = target.TimeQualityFlags;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for TimeQualityIndicatorCode
        ///</summary>
        [TestMethod()]
        public void TimeQualityIndicatorCodeTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            GSF.PhasorProtocols.IEEEC37_118.TimeQualityIndicatorCode expected = new GSF.PhasorProtocols.IEEEC37_118.TimeQualityIndicatorCode();
            GSF.PhasorProtocols.IEEEC37_118.TimeQualityIndicatorCode actual;
            target.TimeQualityIndicatorCode = expected;
            actual = target.TimeQualityIndicatorCode;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for Timestamp
        ///</summary>
        [TestMethod()]
        public void TimestampTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            GSF.Ticks expected = new GSF.Ticks();
            GSF.Ticks actual;
            target.Timestamp = expected;
            actual = target.Timestamp;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for TypeID
        ///</summary>
        [TestMethod()]
        public void TypeIDTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            GSF.PhasorProtocols.IEEEC37_118.FrameType actual;
            actual = target.TypeID;
            Assert.IsInstanceOfType(actual, typeof(FrameType));
        }

        /// <summary>
        ///A test for Version
        ///</summary>
        [TestMethod()]
        public void VersionTest()
        {
            GSF.PhasorProtocols.IEEEC37_118.HeaderFrame target = new GSF.PhasorProtocols.IEEEC37_118.HeaderFrame();
            byte actual;
            actual = target.Version;
            Assert.IsNotNull(actual);

        }
        #endregion
    }
}
