#region [ Code Modification History ]
/*
 * 04/23/2012 Denis Kholine
 *   Generated Original version of source code.
 *
 * 05/22/2012 Denis Kholine
 *   Add descriptions.
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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for MeasurementKeyTest and is intended
    ///to contain all MeasurementKeyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeasurementKeyTest
    {
        #region [ Properties ]
        /// <summary>
        /// Gets or sets the numeric ID of this <see cref="MeasurementKey"/>.
        /// </summary>
        public uint ID { get; set; }

        /// <summary>
        /// Gets or sets <see cref="Guid"/> ID of signal associated with this <see cref="MeasurementKey"/>.
        /// </summary>
        public Guid SignalID { get; set; }

        /// <summary>
        /// Gets or sets the source of this <see cref="MeasurementKey"/>.
        /// </summary>
        /// <remarks>
        /// This value is typically used to track the archive name in which the measurement, that this <see cref="MeasurementKey"/> represents, is stored.
        /// </remarks>
        public string Source { get; set; }

        private bool Expected { get; set; }

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

        #region [ Context ]
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
        ///A test for CompareTo
        ///</summary>
        //[TestMethod()]
        //public void CompareToTest()
        //{
        //    MeasurementKey target = new MeasurementKey();
        //    object obj = new MeasurementKey();
        //    int expected = 0;
        //    int actual;
        //    actual = target.CompareTo(obj);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        //[TestMethod()]
        //public void CompareToTest1()
        //{
        //    MeasurementKey target = new MeasurementKey();
        //    MeasurementKey other = new MeasurementKey();
        //    int expected = 0;
        //    int actual;
        //    actual = target.CompareTo(other);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            MeasurementKey target = new MeasurementKey();
            object obj = new MeasurementKey();
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest1()
        {
            MeasurementKey target = new MeasurementKey();
            MeasurementKey other = new MeasurementKey();
            bool expected = true;
            bool actual;
            actual = target.Equals(other);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            MeasurementKey target = new MeasurementKey();
            int expected = 0;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        /////A test for ID
        /////</summary>
        //[TestMethod()]
        //public void IDTest()
        //{
        //    MeasurementKey target = new MeasurementKey();
        //    uint expected = 0;
        //    uint actual;
        //    target.ID = expected;
        //    actual = target.ID;
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        /////A test for LookupBySignalID
        /////</summary>
        //[TestMethod()]
        //public void LookupBySignalIDTest()
        //{
        //    System.Guid signalID = new System.Guid();
        //    MeasurementKey expected = new MeasurementKey();
        //    MeasurementKey actual;
        //    actual = MeasurementKey.LookupBySignalID(signalID);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for MeasurementKey Constructor
        ///</summary>
        //[TestMethod()]
        //public void MeasurementKeyConstructorTest()
        //{
        //    System.Guid signalID = new System.Guid();
        //    signalID = Guid.NewGuid();
        //    uint id = 0;
        //    string source = "Unit Test".ToUpper();
        //    MeasurementKey target = new MeasurementKey(signalID, id, source);
        //    Expected = (target.ID == ID && target.SignalID == signalID && target.Source == source);
        //    Assert.IsTrue(Expected);
        //}

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
            Expected = false;
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for op_Equality
        ///</summary>
        //[TestMethod()]
        //public void op_EqualityTest()
        //{
        //    MeasurementKey key1 = new MeasurementKey();
        //    MeasurementKey key2 = new MeasurementKey();
        //    bool expected = true;
        //    bool actual;
        //    actual = (key1 == key2);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        //[TestMethod()]
        //public void op_GreaterThanOrEqualTest()
        //{
        //    MeasurementKey key1 = new MeasurementKey();
        //    MeasurementKey key2 = new MeasurementKey();
        //    bool expected = true;
        //    bool actual;
        //    actual = (key1 >= key2);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        //[TestMethod()]
        //public void op_GreaterThanTest()
        //{
        //    MeasurementKey key1 = new MeasurementKey();
        //    MeasurementKey key2 = new MeasurementKey();
        //    bool expected = false;
        //    bool actual;
        //    actual = (key1 > key2);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        //[TestMethod()]
        //public void op_InequalityTest()
        //{
        //    MeasurementKey key1 = new MeasurementKey();
        //    MeasurementKey key2 = new MeasurementKey();
        //    bool expected = false;
        //    bool actual;
        //    actual = (key1 != key2);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        //[TestMethod()]
        //public void op_LessThanOrEqualTest()
        //{
        //    MeasurementKey key1 = new MeasurementKey();
        //    MeasurementKey key2 = new MeasurementKey();
        //    bool expected = true;
        //    bool actual;
        //    actual = (key1 <= key2);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        //[TestMethod()]
        //public void op_LessThanTest()
        //{
        //    MeasurementKey key1 = new MeasurementKey();
        //    MeasurementKey key2 = new MeasurementKey();
        //    bool expected = false;
        //    bool actual;
        //    actual = (key1 < key2);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Parse
        ///</summary>
        //[TestMethod()]
        //public void ParseTest()
        //{
        //    string value = "12:13".ToUpper();
        //    System.Guid signalID = new System.Guid();
        //    signalID = Guid.NewGuid();
        //    MeasurementKey expected = new MeasurementKey(signalID, 12, value);
        //    MeasurementKey actual;
        //    actual = MeasurementKey.Parse(value, signalID);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for SignalID
        ///</summary>
        //[TestMethod()]
        //public void SignalIDTest()
        //{
        //    MeasurementKey target = new MeasurementKey();
        //    System.Guid expected = new System.Guid();
        //    System.Guid actual;
        //    target.SignalID = expected;
        //    actual = target.SignalID;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for Source
        ///</summary>
        //[TestMethod()]
        //public void SourceTest()
        //{
        //    MeasurementKey target = new MeasurementKey();
        //    string expected = string.Empty;
        //    string actual;
        //    target.Source = expected;
        //    actual = target.Source;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            MeasurementKey target = new MeasurementKey();
            string expected = ":0";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TryParse
        ///</summary>
        [TestMethod()]
        public void TryParseTest()
        {
            string value = "Unit Test".ToUpper();
            System.Guid signalID = new System.Guid();
            signalID = Guid.NewGuid();
            MeasurementKey key = new MeasurementKey();
            MeasurementKey keyExpected = new MeasurementKey();
            bool expected = false;
            bool actual;
            actual = MeasurementKey.TryParse(value.ToString(), out key);
            Assert.AreEqual(keyExpected, key);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UpdateSignalID
        ///</summary>
        //[TestMethod()]
        //public void UpdateSignalIDTest()
        //{
        //    MeasurementKey target = new MeasurementKey();
        //    System.Guid signalID = new System.Guid();
        //    target.UpdateSignalID(signalID);
        //}

        #endregion
    }
}