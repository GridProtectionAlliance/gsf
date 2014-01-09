#region [ Code Modification History ]
/*
 * 07/02/2012 Denis Kholine
 *   Generated original source code
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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
using GSF;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for MeasurementTest and is intended
    ///to contain all MeasurementTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeasurementTest
    {
        #region [ Static ]
        private static Guid m_guid = new Guid("3647f729-d0ed-4f79-85ad-dae2149cd432");
        private static Ticks m_ticks = DateTime.Now.Ticks;
        private static double m_value = 10;
        private static MeasurementStateFlags m_flags = MeasurementStateFlags.Normal;
        #endregion

        #region [ Members ]
        private DateTime datetime1;
        private DateTime datetime2;
        private Measurement<double> measurement1;
        private Measurement<double> measurement2;
        private MeasurementKey measurementkey1;
        private MeasurementKey measurementkey2;
        private List<Measurement<double>> measurements;
      

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
        ///A test for AdjustedValue
        ///</summary>
        //[TestMethod()]
        //public void AdjustedValueTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    double actual;
        //    double expected = 10;
        //    actual = target.AdjustedValue;
        //    Assert.AreEqual(actual, expected);
        //}

        /// <summary>
        ///A test for Clone
        ///</summary>
        //[TestMethod()]
        //public void CloneTest()
        //{
        //    IMeasurement<double> measurementToClone = this.measurement1;
        //    measurementToClone = this.measurement1;
        //    double value = measurement1.Value;
        //    Ticks timestamp = measurement1.Timestamp;
        //    Measurement<double> expected = this.measurement1;
        //    Measurement<double> actual;
        //    actual = Measurement.Clone(measurementToClone, value, timestamp);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Clone
        ///</summary>
        //[TestMethod()]
        //public void CloneTest1()
        //{
        //    IMeasurement<double> measurementToClone = measurement1;
        //    Ticks timestamp = measurement1.Timestamp;
        //    Measurement<double> expected = measurement1;
        //    Measurement<double> actual;
        //    actual = easurement.Clone(measurementToClone, timestamp);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Clone
        ///</summary>
        //[TestMethod()]
        //public void CloneTest2()
        //{
        //    IMeasurement<double> measurementToClone = measurement1;
        //    Measurement<double> expected = measurement1;
        //    Measurement<double> actual;
        //    actual = Measurement.Clone(measurementToClone);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        //[TestMethod()]
        //public void CompareToTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    ITimeSeriesValue other = this.measurement1;
        //    int expected = 0;
        //    int actual;
        //    actual = target.CompareTo(other);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        //[TestMethod()]
        //public void CompareToTest1()
        //{
        //    Measurement<double> target = measurement1;
        //    object obj = measurement1;
        //    int expected = 0;
        //    int actual;
        //    actual = target.CompareTo(obj);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Equals
        ///</summary>
        //[TestMethod()]
        //public void EqualsTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    ITimeSeriesValue other = this.measurement1;
        //    bool expected = true;
        //    bool actual;
        //    actual = target.Equals(other);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest1()
        {
            Measurement<double> target = measurement1;
            object obj = measurement2;
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            Measurement<double> target = this.measurement1;
            int expected = this.measurement1.GetHashCode();
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ID
        ///</summary>
        //[TestMethod()]
        //public void IDTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    System.Guid expected = this.measurement1.ID;
        //    System.Guid actual;
        //    target.ID = expected;
        //    actual = target.ID;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Key
        ///</summary>
        //[TestMethod()]
        //public void KeyTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    MeasurementKey expected = this.measurement1.Key;
        //    MeasurementKey actual;
        //    target.Key = expected;
        //    actual = target.Key;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for MajorityValueFilter
        ///</summary>
        //[TestMethod()]
        //public void MajorityValueFilterTest()
        //{
        //    IEnumerable<IMeasurement> source = measurements;
        //    double expected = 0F;
        //    double actual;
        //    actual = Measurement.MajorityValueFilter(source);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Measurement<double> Constructor
        ///</summary>
        [TestMethod()]
        public void MeasurementConstructorTest()
        {
            Measurement<double> target = this.measurement1;
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(Measurement<double>));
        }

        /// <summary>
        ///A test for MeasurementValueFilter
        ///</summary>
        //[TestMethod()]
        //public void MeasurementValueFilterTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    MeasurementValueFilterFunction expected = new MeasurementValueFilterFunction(MeasurementValueFilterFunctionHelper);
        //    MeasurementValueFilterFunction actual;
        //    target.MeasurementValueFilter = MeasurementValueFilterFunctionHelper;
        //    actual = target.MeasurementValueFilter;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Multiplier
        ///</summary>
        //[TestMethod()]
        //public void MultiplierTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    double expected = 0F;
        //    double actual;
        //    target.Multiplier = expected;
        //    actual = target.Multiplier;
        //    Assert.AreEqual(expected, actual);
        //}

        [TestCleanup()]
        public void MyTestCleanup()
        {
            measurements.Clear();
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            datetime1 = DateTime.UtcNow;
            datetime2 = datetime1;

            measurement1 = new Measurement<double>(m_guid, m_ticks,m_flags, m_value);
            measurement2 = new Measurement<double>(m_guid, m_ticks,m_flags, m_value);
            measurements = new List<Measurement<double>>();

            measurements.Add(measurement1);
            measurements.Add(measurement2);
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest()
        {
            Measurement<double> measurement1 = this.measurement1;
            Measurement<double> measurement2 = this.measurement2;
            bool expected = true;
            bool actual;
            actual = (measurement1 == measurement2);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Equality Guid comparison inconclusive");
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        //[TestMethod()]
        //public void op_GreaterThanOrEqualTest()
        //{
        //    Measurement<double> measurement1 = this.measurement1;
        //    Measurement<double> measurement2 = this.measurement2;
        //    bool expected = true;
        //    bool actual;
        //    actual = (measurement1 >= measurement2);
        //    Assert.AreEqual(expected, actual);
        //    //Assert.Inconclusive("Greater than or equal Guid comparison inconclusive");
        //}

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        //[TestMethod()]
        //public void op_GreaterThanTest()
        //{
        //    Measurement<double> measurement1 = this.measurement1;
        //    Measurement<double> measurement2 = this.measurement2;
        //    bool expected = false;
        //    bool actual;
        //    actual = (measurement1 > measurement2);
        //    Assert.AreEqual(expected, actual);
        //    //Assert.Inconclusive("Greater than Guid comparison inconclusive");
        //}

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest()
        {
            Measurement<double> measurement1 = this.measurement1;
            Measurement<double> measurement2 = this.measurement2;
            bool expected = false;
            bool actual;
            actual = (measurement1 != measurement2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        //[TestMethod()]
        //public void op_LessThanOrEqualTest()
        //{
        //    Measurement<double> measurement1 = this.measurement1;
        //    Measurement<double> measurement2 = this.measurement2;
        //    bool expected = true;
        //    bool actual;
        //    actual = (measurement1 <= measurement2);
        //    Assert.AreEqual(expected, actual);
        //    //Assert.Inconclusive("Less than or equal Guid comparison inconclusive");
        //}

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        //[TestMethod()]
        //public void op_LessThanTest()
        //{
        //    Measurement<double> measurement1 = this.measurement1;
        //    Measurement<double> measurement2 = this.measurement2;
        //    bool expected = false;
        //    bool actual;
        //    actual = (measurement1 < measurement2);
        //    Assert.AreEqual(expected, actual);
        //    //Assert.Inconclusive("Less than Guid comparison inconclusive");
        //}

        /// <summary>
        ///A test for PublishedTimestamp
        ///</summary>
        //[TestMethod()]
        //public void PublishedTimestampTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    Ticks expected = this.measurement1.Timestamp;
        //    Ticks actual;
        //    target.PublishedTimestamp = expected;
        //    actual = target.PublishedTimestamp;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for ReceivedTimestamp
        ///</summary>
        //[TestMethod()]
        //public void ReceivedTimestampTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    Ticks expected = this.measurement1.Timestamp;
        //    Ticks actual;
        //    target.ReceivedTimestamp = expected;
        //    actual = target.ReceivedTimestamp;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for StateFlags
        ///</summary>
        [TestMethod()]
        public void StateFlagsTest()
        {
            Measurement<double> target = this.measurement1;
            MeasurementStateFlags expected = this.measurement1.StateFlags;
            MeasurementStateFlags actual;
            actual = target.StateFlags;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TagName
        ///</summary>
        //[TestMethod()]
        //public void TagNameTest()
        //{
        //    Measurement<double> target = this.measurement1;
        //    string expected = string.Empty;
        //    string actual;
        //    target.TagName = expected;
        //    actual = target.TagName;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Timestamp
        ///</summary>
        [TestMethod()]
        public void TimestampTest()
        {
            Measurement<double> target = this.measurement1;
            Ticks expected = this.measurement1.Timestamp;
            Ticks actual;
            actual = target.Timestamp;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        //[TestMethod()]
        //public void ToStringTest()
        //{
        //    IMeasurement<double> measurement= measurement1;
        //    bool includeTagName = false;
        //    string expected = "UNITTEST:10";
        //    string actual;
        //    actual = measurement.ToString(measurement, includeTagName);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest1()
        {
            Measurement<double> target = measurement1;
            string expected = "M1 [UNITTEST:10]";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ITimeSeriesValue.Value
        ///</summary>
        //[TestMethod()]
        //[DeploymentItem("TimeSeriesFramework.dll")]
        //public void ValueTest()
        //{
        //    ITimeSeriesValue target = measurement1;
        //    BigBinaryValue expected = measurement1.Value;
        //    BigBinaryValue actual;
        //    actual = target.Value;
        //    Assert.AreEqual(expected.ToString(), actual.ToString());
        //}

        /// <summary>
        ///A test for Value
        ///</summary>
        //[TestMethod()]
        //public void ValueTest1()
        //{
        //    Measurement<double> target = this.measurement1;
        //    double expected = 0F;
        //    double actual;
        //    target.Value = expected;
        //    actual = target.Value;
        //    Assert.AreEqual(expected, actual);
        //}

        private double MeasurementValueFilterFunctionHelper(IEnumerable<IMeasurement> items)
        {
            return 10;
        }

        #endregion
    }
}