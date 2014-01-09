#region [ Code Modification History ]
/*
 *  07/02/2012 Denis Kholine
 *    Generated original version of source code
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for CompactMeasurementTest and is intended
    ///to contain all CompactMeasurementTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CompactMeasurementTest
    {
        #region [ Members ]
        private static Guid m_guid = new Guid("3647f729-d0ed-4f79-85ad-dae2149cd432");
        private static Ticks m_ticks = DateTime.Now.Ticks;
        private static double m_value = 10;
        private static MeasurementStateFlags m_flags = MeasurementStateFlags.Normal;
        #endregion

        #region [ Members ]
        private DateTime datetime1;
        private Measurement<double> measurement1;
        private MeasurementKey measurementkey1;
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
        //

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
        ///A test for CompactMeasurement<double> Constructor
        ///</summary>
        [TestMethod()]
        public void CompactMeasurementConstructorTest()
        {
            IMeasurement<double> measurement = measurement1;
            SignalIndexCache signalIndexCache = new SignalIndexCache();
            bool includeTime = false;
            long[] baseTimeOffsets = new long[8];
            int timeIndex = 10;
            bool useMillisecondResolution = false;
            CompactMeasurement target = new CompactMeasurement(measurement, signalIndexCache, includeTime, baseTimeOffsets, timeIndex, useMillisecondResolution);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(CompactMeasurement));
        }

        /// <summary>
        ///A test for CompactMeasurement<double> Constructor
        ///</summary>
        [TestMethod()]
        public void CompactMeasurementConstructorTest1()
        {
            SignalIndexCache signalIndexCache = new SignalIndexCache();
            bool includeTime = false;
            long[] baseTimeOffsets = new long[8];
            int timeIndex = 10;
            bool useMillisecondResolution = false;
            CompactMeasurement target = new CompactMeasurement(signalIndexCache, includeTime, baseTimeOffsets, timeIndex, useMillisecondResolution);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(CompactMeasurement));
        }

        /// <summary>
        ///A test for GenerateBinaryImage
        ///</summary>
        [TestMethod()]
        public void GenerateBinaryImageTest()
        {
            SignalIndexCache signalIndexCache = new SignalIndexCache();
            bool includeTime = false;
            long[] baseTimeOffsets = new long[8];
            int timeIndex = 10;
            bool useMillisecondResolution = false;
            CompactMeasurement target = new CompactMeasurement(signalIndexCache, includeTime, baseTimeOffsets, timeIndex, useMillisecondResolution);
            byte[] buffer = new byte[8];
            int startIndex = 1;
            int expected = 7;
            int actual;
            actual = target.GenerateBinaryImage(buffer, startIndex);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IncludeTime
        ///</summary>
        [TestMethod()]
        public void IncludeTimeTest()
        {
            SignalIndexCache signalIndexCache = new SignalIndexCache();
            bool includeTime = false;
            long[] baseTimeOffsets = new long[8];
            int timeIndex = 0;
            bool useMillisecondResolution = false;
            CompactMeasurement target = new CompactMeasurement(signalIndexCache, includeTime, baseTimeOffsets, timeIndex, useMillisecondResolution);
            bool actual;
            actual = target.IncludeTime;
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            measurement1 = new Measurement<double>(m_guid, m_ticks,m_flags, m_value);
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for ParseBinaryImage
        ///</summary>
        [TestMethod()]
        public void ParseBinaryImageTest()
        {
            Tuple<Guid, string, uint> reference = new Tuple<Guid, string, uint>(Guid.NewGuid(), "UnitTest", 10);
            ushort id = 515;
            Func<ushort, Tuple<Guid, string, uint>> addValue = (x) => { return reference; };
            Func<ushort, Tuple<Guid, string, uint>, Tuple<Guid, string, uint>> updateValue = (x, y) => { return y; };
            ConcurrentDictionary<ushort, Tuple<Guid, string, uint>> dictionary = new ConcurrentDictionary<ushort, Tuple<Guid, string, uint>>();
            dictionary.AddOrUpdate(id, addValue, updateValue);
            SignalIndexCache signalIndexCache = new SignalIndexCache();
            signalIndexCache.Reference = dictionary;
            bool includeTime = false;
            long[] baseTimeOffsets = new long[8];
            int timeIndex = 10;
            bool useMillisecondResolution = false;
            CompactMeasurement target = new CompactMeasurement(signalIndexCache, includeTime, baseTimeOffsets, timeIndex, useMillisecondResolution);
            byte[] buffer = new byte[8] { 1, 2, 3, 4, 2, 3, 1, 1 };
            int startIndex = 0;
            int length = 8;
            int expected = 7;
            int actual;
            target.ID = Guid.NewGuid();
            actual = target.ParseBinaryImage(buffer, startIndex, length);

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}