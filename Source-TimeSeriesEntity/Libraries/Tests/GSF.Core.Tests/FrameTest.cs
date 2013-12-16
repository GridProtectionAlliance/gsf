#region [ Code Modification History ]
/*
 * 04/20/2012 Denis Kholine
 *   Generated Original version of source code.
 *   Update unit tests.
 *
 * 04/23/2012 Denis Kholine
 *   Update Equals & CompareTo unit tests
 *
 * 05/07/2012 Denis Kholine
 *   Git relocation updates.
 *
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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
using GSF;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for FrameTest and is intended
    ///to contain all FrameTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FrameTest
    {
        #region [ Members ]
        private Frame frame1;
        private Frame frame2;
        private IDictionary<MeasurementKey, IMeasurement> measurements;
        private KeyValuePair<MeasurementKey, IMeasurement> pair;
        private Frame target;

        #endregion

        #region [ Properties ]
        /// <summary>
        /// Expected
        /// </summary>
        public bool expected;

        /// <summary>
        /// Expected Measurements
        /// </summary>
        public int expectedMeasurements = 2;

        /// <summary>
        /// Gets or sets reference to last measurement that was sorted into this <see cref="Frame"/>.
        /// </summary>
        public IMeasurement LastSortedMeasurement = new Measurement();

        /// <summary>
        /// Keyed measurements in this <see cref="Frame"/>.
        /// </summary>
        public ConcurrentDictionary<MeasurementKey, IMeasurement> Measurements;

        /// <summary>
        /// Gets or sets published state of this <see cref="Frame"/> (pre-processing).
        /// </summary>
        public bool Published = false;

        /// <summary>
        /// Gets or sets exact Timestamp, in ticks, of when this <see cref="Frame"/> was published (post-processing).
        /// </summary>
        /// <remarks>
        /// <para>In the default implementation, setting this property will update all associated <see cref="IMeasurement.PublishedTimestamp"/>.</para>
        /// <para>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.</para>
        /// </remarks>
        public Ticks PublishedTimestamp = new Ticks(1000);

        /// <summary>
        /// Gets or sets exact Timestamp, in ticks, of when this <see cref="Frame"/> was received (i.e., created).
        /// </summary>
        /// <remarks>
        /// <para>In the default implementation, this Timestamp will simply be the ticks of <see cref="PrecisionTimer.UtcNow"/> of when this class was created.</para>
        /// <para>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.</para>
        /// </remarks>
        public Ticks ReceivedTimestamp = new Ticks(1000);

        /// <summary>
        /// Gets or sets total number of measurements that have been sorted into this <see cref="Frame"/>.
        /// </summary>
        /// <remarks>
        /// If this property has not been assigned a value, the property will return measurement count.
        /// </remarks>
        public int SortedMeasurements = 1;

        /// <summary>
        /// Gets or sets exact Timestamp, in <see cref="Ticks"/>, of the data represented in this <see cref="Frame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks Timestamp = new Ticks(new DateTime(1000));

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
        /// A test for Clone
        /// Create a copy of this <see cref="Frame"/> and its measurements.
        /// </summary>
        /// <remarks>
        /// The measurement dictionary of this <see cref="Frame"/> is sync locked during copy.
        /// </remarks>
        /// <returns>A cloned <see cref="Frame"/>.</returns>
        [TestMethod()]
        public void CloneTest()
        {
            Frame expected = target;
            Frame actual;
            actual = target.Clone();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares the <see cref="Frame"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IFrame"/>.</exception>
        /// <remarks>This implementation of a basic frame compares itself by Timestamp.</remarks>
        [TestMethod()]
        public void CompareToTest()
        {
            object obj = target;
            int expected = 0;
            int actual;
            actual = target.CompareTo(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares the <see cref="Frame"/> with an <see cref="IFrame"/>.
        /// </summary>
        /// <param name="other">The <see cref="IFrame"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>This implementation of a basic frame compares itself by Timestamp.</remarks>
        [TestMethod()]
        public void CompareToTest1()
        {
            expected = (frame2.CompareTo(frame1) == 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            IFrame other = frame1;
            expected = (frame2.Equals(other));
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest1()
        {
            object obj = frame1;
            expected = frame2.Equals(obj);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for Frame Constructor
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="Timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        /// <param name="measurements">Initial set of measurements to load into the <see cref="Frame"/>, if any.</param>
        [TestMethod()]
        public void FrameConstructorTest()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(Frame));
        }

        /// <summary>
        ///  A test for Frame Constructor
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="Timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        /// <param name="expectedMeasurements">Expected number of measurements for the <see cref="Frame"/>.</param>
        [TestMethod()]
        public void FrameConstructorTestExpected()
        {
            int expectedMeasurements = 0;
            target = new Frame(Timestamp, expectedMeasurements);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(Frame));
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            expected = (target.GetHashCode() >= 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for LastSortedMeasurement
        ///</summary>
        [TestMethod()]
        public void LastSortedMeasurementTest()
        {
            IMeasurement expected = new Measurement();
            IMeasurement actual;
            target.LastSortedMeasurement = expected;
            actual = target.LastSortedMeasurement;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Measurements
        ///</summary>
        [TestMethod()]
        public void MeasurementsTest()
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> actual;
            actual = target.Measurements;
            Assert.AreEqual(actual, target.Measurements);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            target = null;
            measurements.Clear();
            frame1 = null;
            frame2 = null;
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
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            measurements = new Dictionary<MeasurementKey, IMeasurement>();
            pair = new KeyValuePair<MeasurementKey, IMeasurement>(new MeasurementKey(), new Measurement());
            measurements.Add(pair);

            target = new Frame(Timestamp, measurements);
            frame1 = new Frame(Timestamp, measurements);
            frame2 = new Frame(Timestamp, measurements);
            expected = false;
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest()
        {
            bool expected = true;
            bool actual;
            actual = (frame1 == frame2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanOrEqualTest()
        {
            bool expected = true;
            bool actual;
            actual = (frame1 >= frame2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanTest()
        {
            bool expected = false;
            bool actual;
            actual = (frame1 > frame2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest()
        {
            bool expected = false;
            bool actual;
            actual = (frame1 != frame2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_LessThanOrEqualTest()
        {
            bool expected = true;
            bool actual;
            actual = (frame1 <= frame2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void op_LessThanTest()
        {
            bool expected = false;
            bool actual;
            actual = (frame1 < frame2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Published
        ///</summary>
        [TestMethod()]
        public void PublishedTest()
        {
            target.Published = this.Published;
            expected = (target.Published == this.Published);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for PublishedTimestamp
        ///</summary>
        [TestMethod()]
        public void PublishedTimestampTest()
        {
            target.PublishedTimestamp = this.PublishedTimestamp;
            expected = (target.PublishedTimestamp == this.PublishedTimestamp);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for ReceivedTimestamp
        ///</summary>
        [TestMethod()]
        public void ReceivedTimestampTest()
        {
            target.ReceivedTimestamp = this.ReceivedTimestamp;
            expected = (target.ReceivedTimestamp == this.ReceivedTimestamp);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for SortedMeasurements
        ///</summary>
        [TestMethod()]
        public void SortedMeasurementsTest()
        {
            target.SortedMeasurements = this.SortedMeasurements;
            expected = (target.SortedMeasurements == this.SortedMeasurements);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for Timestamp
        ///</summary>
        [TestMethod()]
        public void TimestampTest()
        {
            target.Timestamp = this.Timestamp;
            expected = (target.Timestamp == this.Timestamp);
            Assert.IsTrue(expected);
        }

        #endregion
    }
}