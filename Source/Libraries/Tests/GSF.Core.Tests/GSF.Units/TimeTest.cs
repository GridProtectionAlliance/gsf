//******************************************************************************************************
//  TimeTest.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/01/2011 - Denis Kholine
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ University of Illinois/NCSA Open Source License ]
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

using System;
using System.Collections.Generic;
using System.Globalization;
using GSF.Units;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{

    /// <summary>
    ///This is a test class for TimeTest and is intended
    ///to contain all TimeTest Unit Tests
    ///</summary>
    [TestClass]
    public class TimeTest
    {

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

        #region Additional test attributes
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

        /// <summary>
        /// A test for Time Constructor
        /// Creates a new <see cref="Time"/>.
        /// </summary>
        /// <param name="value">New time value in seconds.</param>
        [TestMethod]
        public void TimeConstructorTest()
        {
            List<Double> values = new List<Double>();

            //Initialization
            values.Add(0);

            foreach (Double value in values)
            {
                Time target = new Time(value);
                Assert.IsInstanceOfType(target, typeof(Time));
                Assert.IsNotNull(target);
            }
            values.Clear();
        }

        /// <summary>
        /// A test for Time Constructor
        /// Creates a new <see cref="Time"/>.
        /// </summary>
        /// <param name="value">New time value as a <see cref="TimeSpan"/>.</param>
        [TestMethod]
        public void TimeSpanConstructorTest()
        {

            List<Double> values = new List<Double>();

            //Initialization
            values.Add(0);

            foreach (Double value in values)
            {
                TimeSpan target = new TimeSpan(1, 1, 1);
                Assert.IsInstanceOfType(target, typeof(TimeSpan));
                Assert.IsNotNull(target);
            }
            values.Clear();
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Time"/>.</exception>
        [TestMethod]
        public void CompareToTest()
        {
            Time target = new Time(10F);
            object value = new Time(10F);
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified <see cref="Double"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An <see cref="Double"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToDoubleTest()
        {
            Time target = new Time(10F);
            double value = 10F;
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified <see cref="TimeSpan"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="TimeSpan"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToTimeSpanTest()
        {
            Time target = new Time(10F);
            TimeSpan value = new TimeSpan(0, 0, 10);
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified <see cref="Time"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Time"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToTimeTest()
        {
            Time target = new Time(10F);
            Time value = new Time(10F);
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Double"/> value.
        /// </summary>
        /// <param name="obj">An <see cref="Double"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsDoubleTest()
        {
            Time target = new Time(10F);
            double obj = 10F;
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="TimeSpan"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsTimeSpanTest()
        {
            Time target = new Time(10F);
            TimeSpan obj = new TimeSpan(0, 0, 10);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Time"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Time"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsTest()
        {
            Time target = new Time(10F);
            Time obj = new Time(10F);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Time"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsObjectTest()
        {
            Time target = new Time(10F);
            object obj = new Time(10F);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromAtomicUnitsOfTime (Statis method)
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in atomic units of time.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in atomic units of time.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in atomic units of time.</returns>
        [TestMethod]
        public void FromAtomicUnitsOfTimeTest()
        {
            double value = 10F;
            Time expected = new Time(0.0000000000000002418884254);
            Time actual;
            actual = Time.FromAtomicUnitsOfTime(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromDays
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in days.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in days.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in days.</returns>
        [TestMethod]
        public void FromDaysTest()
        {
            double value = 10F;
            Time expected = new Time(864000.0);
            Time actual;
            actual = Time.FromDays(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromHours
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in hours.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in hours.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in hours.</returns>
        [TestMethod]
        public void FromHoursTest()
        {
            double value = 1F;
            Time expected = new Time(3600);
            Time actual;
            actual = Time.FromHours(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromKe
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in ke,
        /// the traditional Chinese unit of decimal time.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in ke.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in ke.</returns>
        [TestMethod]
        public void FromKeTest()
        {
            double value = 10F;
            Time expected = new Time(8640.0);
            Time actual;
            actual = Time.FromKe(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in minutes.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in minutes.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in minutes.</returns>
        [TestMethod]
        public void FromMinutesTest()
        {
            double value = 10F;
            Time expected = new Time(600.0);
            Time actual;
            actual = Time.FromMinutes(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromPlanckTime
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in Planck time.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in Planck time.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in Planck time.</returns>
        [TestMethod]
        public void FromPlanckTimeTest()
        {
            double value = 10F;
            Time expected = new Time(1.3512118179999999E-42);
            Time actual;
            actual = Time.FromPlanckTime(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromWeeks
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in weeks.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in weeks.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in weeks.</returns>
        [TestMethod]
        public void FromWeeksTest()
        {
            double value = 10F;
            Time expected = new Time(6048000.0);
            Time actual;
            actual = Time.FromWeeks(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for GetHashCode
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        [TestMethod]
        public void GetHashCodeTest()
        {
            Time target = new Time(10);
            int expected = 1076101120;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for GetTypeCode
        /// Returns the <see cref="TypeCode"/> for value type <see cref="Double"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.Double"/>.</returns>
        [TestMethod]
        public void GetTypeCodeTest()
        {
            Time target = new Time(10F);
            TypeCode expected = new TypeCode();
            expected = Type.GetTypeCode(typeof(Double));
            TypeCode actual;
            actual = target.GetTypeCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            Time expected = new Time(value);
            Time actual;
            actual = Time.Parse(s, style);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseTimeTest()
        {
            double value = 10F;
            string s = value.ToString();
            Time expected = new Time(value);
            Time actual;
            actual = Time.Parse(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            IFormatProvider provider = null;
            Time expected = new Time(value);
            Time actual;
            actual = Time.Parse(s, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            IFormatProvider provider = null;
            Time expected = new Time(value);
            Time actual;
            actual = Time.Parse(s, style, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for SecondsPerMonth
        /// Returns the number of seconds in the specified month and year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (a number ranging from 1 to 12).</param>
        /// <returns>
        /// The number of seconds, as a <see cref="Time"/>, in the month for the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Month is less than 1 or greater than 12. -or- year is less than 1 or greater than 9999.
        /// </exception>
        [TestMethod]
        public void SecondsPerMonthTest()
        {
            int year = 2012;
            int month = 1;
            int expected = 2678400;
            int actual;
            actual = Time.SecondsPerMonth(year, month);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for SecondsPerYear
        /// Returns the number of seconds in the specified year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The number of seconds in the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Year is less than 1 or greater than 9999.
        /// </exception>
        [TestMethod]
        public void SecondsPerYearTest()
        {
            int year = 1;
            long expected = 31536000;
            long actual;
            actual = Time.SecondsPerYear(year);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToBoolean
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToBooleanTest()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            bool expected = true;
            bool actual;
            actual = target.ToBoolean(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToByte
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToByteTest()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            byte expected = 10;
            byte actual;
            actual = target.ToByte(provider);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        /////A test for System.IConvertible.ToChar
        /////</summary>
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void ToCharTest()
        //{
        //    Assert.Inconclusive("Can't cast from 'Double' to 'Char'.");
        //}

        ///// <summary>
        /////A test for System.IConvertible.ToDateTime
        /////</summary>
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void ToDateTimeTest()
        //{

        //    Assert.Inconclusive("Can't cast from 'Double' to 'Char'.");
        //}

        /// <summary>
        ///A test for System.IConvertible.ToDecimal
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDecimalTest()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            Decimal expected = new Decimal(10F);
            Decimal actual;
            actual = target.ToDecimal(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToDouble
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDoubleTest()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            double expected = 10F;
            double actual;
            actual = target.ToDouble(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToInt16
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToInt16Test()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            short expected = 10;
            short actual;
            actual = target.ToInt16(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToInt32
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToInt32Test()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            int expected = 10;
            int actual;
            actual = target.ToInt32(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToInt64
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToInt64Test()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            long expected = 10;
            long actual;
            actual = target.ToInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToSByte
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToSByteTest()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            sbyte expected = 10;
            sbyte actual;
            actual = target.ToSByte(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToSingle
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToSingleTest()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            float expected = 10F;
            float actual;
            actual = target.ToSingle(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToType
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToTypeTest()
        {
            double value = 10F;
            IConvertible target = new Time(value);
            Type conversionType = typeof(Double);
            IFormatProvider provider = null;
            object expected = new Time(value);
            object actual;
            actual = target.ToType(conversionType, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToUInt16
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToUInt16Test()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            ushort expected = 10;
            ushort actual;
            actual = target.ToUInt16(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToUInt32
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToUInt32Test()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            uint expected = 10;
            uint actual;
            actual = target.ToUInt32(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToUInt64
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToUInt64Test()
        {
            IConvertible target = new Time(10F);
            IFormatProvider provider = null;
            ulong expected = 10;
            ulong actual;
            actual = target.ToUInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToAtomicUnitsOfTime
        /// Gets the <see cref="Time"/> value in atomic units of time.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in atomic units of time.</returns>
        [TestMethod]
        public void ToAtomicUnitsOfTimeTest()
        {
            Time target = new Time(10F);
            double expected = 4.1341374575750989E+17;
            double actual;
            actual = target.ToAtomicUnitsOfTime();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToDays
        ///</summary>
        [TestMethod]
        public void ToDaysTest()
        {
            Time target = new Time(10F);
            double expected = 0.00011574074074074075;
            double actual;
            actual = target.ToDays();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToHours
        ///</summary>
        [TestMethod]
        public void ToHoursTest()
        {
            Time target = new Time(10F);
            double expected = 0.0027777777777777779;
            double actual;
            actual = target.ToHours();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToKe
        /// Gets the <see cref="Time"/> value in ke, the traditional Chinese unit of decimal time.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in ke.</returns>
        [TestMethod]
        public void ToKeTest()
        {
            Time target = new Time(10F);
            double expected = 0.011574074074074073;
            double actual;
            actual = target.ToKe();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToMinutes
        /// Gets the <see cref="Time"/> value in minutes.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in minutes.</returns>
        [TestMethod]
        public void ToMinutesTest()
        {
            Time target = new Time(10F);
            double expected = 0.16666666666666666;
            double actual;
            actual = target.ToMinutes();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToPlanckTime
        /// Gets the <see cref="Time"/> value in Planck time.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in Planck time.</returns>
        [TestMethod]
        public void ToPlanckTimeTest()
        {
            Time target = new Time(10F);
            double expected = 7.4007641635354617E+43;
            double actual;
            actual = target.ToPlanckTime();
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for ToString
        ///// Converts the numeric value of this instance to its equivalent string representation using the
        ///// specified format and culture-specific format information.
        ///// </summary>
        ///// <remarks>
        ///// Note that this ToString overload matches <see cref="Double.ToString(string,IFormatProvider)"/>, use
        ///// <see cref="Time.ToString(int)"/> to convert <see cref="Time"/> value into a textual representation
        ///// of years, days, hours, minutes and seconds.
        ///// </remarks>
        ///// <param name="format">A format specification.</param>
        ///// <param name="provider">
        ///// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        ///// </param>
        ///// <returns>
        ///// The string representation of the value of this instance as specified by format and provider.
        ///// </returns>
        //[TestMethod]
        //public void ToStringFormatProviderTest()
        //{
        //    double value = 10F;
        //    Time target = new Time(value);
        //    string format = string.Empty;
        //    IFormatProvider provider = null;
        //    string expected = value.ToString();
        //    string actual;
        //    actual = target.ToString(format, provider);
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        ///// A test for ToString
        ///// Converts the numeric value of this instance to its equivalent string representation, using
        ///// the specified format.
        ///// </summary>
        ///// <remarks>
        ///// Note that this ToString overload matches <see cref="Double.ToString(string)"/>, use
        ///// <see cref="Time.ToString(int)"/> to convert <see cref="Time"/> value into a textual
        ///// representation of years, days, hours, minutes and seconds.
        ///// </remarks>
        ///// <param name="format">A format string.</param>
        ///// <returns>
        ///// The string representation of the value of this instance as specified by format.
        ///// </returns>
        //[TestMethod]
        //public void ToStringFormatTest()
        //{
        //    double value = 10F;
        //    Time target = new Time(value);
        //    string format = string.Empty;
        //    string expected = value.ToString();
        //    string actual;
        //    actual = target.ToString(format);
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        ///// A test for ToString
        ///// Converts the <see cref="Time"/> value into a textual representation of years, days, hours,
        ///// minutes and seconds with the specified number of fractional digits given string array of
        ///// time names.
        ///// </summary>
        ///// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        ///// <param name="timeNames">Time names array to use during textual conversion.</param>
        ///// <remarks>
        ///// <para>Set second precision to -1 to suppress seconds display.</para>
        ///// <para>
        ///// <paramref name="timeNames"/> array needs one string entry for each of the following names:<br/>
        ///// "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds",
        ///// "Less Than 60 Seconds", "0 Seconds".
        ///// </para>
        ///// </remarks>
        ///// <returns>
        ///// The string representation of the value of this instance, consisting of the number of
        ///// years, days, hours, minutes and seconds represented by this value.
        ///// </returns>
        //[TestMethod]
        //public void ToStringSecondPrecisionTimeNamesTest()
        //{
        //    Time target = new Time(10F);
        //    int secondPrecision = 2;
        //    string[] timeNames = { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds" };
        //    string expected = "10.00 Seconds";
        //    string actual;
        //    actual = target.ToString(secondPrecision, timeNames);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for ToString
        /// Converts the numeric value of this instance to its equivalent string representation using the
        /// specified culture-specific format information.
        /// </summary>
        /// <remarks>
        /// Note that this ToString overload matches <see cref="Double.ToString(IFormatProvider)"/>, use
        /// <see cref="Time.ToString(int)"/> to convert <see cref="Time"/> value into a textual
        /// representation of years, days, hours, minutes and seconds.
        /// </remarks>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by provider.
        /// </returns>
        [TestMethod]
        public void ToStringProviderTest()
        {
            double value = 10F;
            Time target = new Time(value);
            IFormatProvider provider = null;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(provider);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for ToString
        ///// Converts the <see cref="Time"/> value into a textual representation of years, days, hours,
        ///// minutes and seconds with the specified number of fractional digits.
        ///// </summary>
        ///// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        ///// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        ///// <returns>
        ///// The string representation of the value of this instance, consisting of the number of
        ///// years, days, hours, minutes and seconds represented by this value.
        ///// </returns>
        //[TestMethod]
        //public void ToStringSecondPrecisionTest()
        //{
        //    double value = 10F;
        //    Time target = new Time(value);
        //    int secondPrecision = 2;
        //    string expected = "10.00 Seconds";
        //    string actual;
        //    actual = target.ToString(secondPrecision);
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        ///// A test for ToString
        ///// Converts the <see cref="Time"/> value into a textual representation of years, days, hours,
        ///// minutes and seconds.
        ///// </summary>
        ///// <remarks>
        ///// Note that this ToString overload will not display fractional seconds. To allow display of
        ///// fractional seconds, or completely remove second resolution from the textual representation,
        ///// use the <see cref="Time.ToString(int)"/> overload instead.
        ///// </remarks>
        ///// <returns>
        ///// The string representation of the value of this instance, consisting of the number of
        ///// years, days, hours, minutes and seconds represented by this value.
        ///// </returns>
        //[TestMethod]
        //public void ToStringTest()
        //{
        //    double value = 10F;
        //    Time target = new Time(value);
        //    string expected = "10 Seconds";
        //    string actual;
        //    actual = target.ToString();
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for ToTicks
        /// Converts the <see cref="Time"/> value, in seconds, to 100-nanosecond tick intervals.
        /// </summary>
        /// <returns>A <see cref="Ticks"/> object.</returns>
        [TestMethod]
        public void ToTicksTest()
        {
            Time target = new Time(10F);
            Ticks expected = new Ticks(100000000);
            Ticks actual;
            actual = target.ToTicks();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToWeeks
        /// Gets the <see cref="Time"/> value in weeks.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in weeks.</returns>
        [TestMethod]
        public void ToWeeksTest()
        {
            Time target = new Time(1000000F);
            double expected = 1.6534391534391535;
            double actual;
            actual = target.ToWeeks();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number to its <see cref="Time"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Time"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not of the correct format, or represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        [TestMethod]
        public void TryParseTest()
        {
            double value = 10F;
            string s = value.ToString();
            Time result = new Time(value);
            Time resultExpected = new Time(value);
            bool expected = true;
            bool actual;
            actual = Time.TryParse(s, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for TryParse
        ///// Converts the string representation of a number to its <see cref="Time"/> equivalent. A return value
        ///// indicates whether the conversion succeeded or failed.
        ///// </summary>
        ///// <param name="s">A string containing a number to convert.</param>
        ///// <param name="result">
        ///// When this method returns, contains the <see cref="Time"/> value equivalent to the number contained in s,
        ///// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        ///// is not of the correct format, or represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        ///// This parameter is passed uninitialized.
        ///// </param>
        ///// <returns>true if s was converted successfully; otherwise, false.</returns>
        //[TestMethod]
        //public void TryParseStyleProviderResultTest()
        //{
        //    double value = 10F;
        //    string s = value.ToString();
        //    NumberStyles style = new NumberStyles();
        //    IFormatProvider provider = null;
        //    Time result = new Time();
        //    Time resultExpected = new Time();
        //    bool expected = true;
        //    bool actual;
        //    actual = Time.TryParse(s, style, provider, out result);
        //    Assert.AreEqual(resultExpected, result);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [TestMethod]
        public void op_AdditionTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            Time expected = new Time(20F);
            Time actual;
            actual = (value1 + value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [TestMethod]
        public void op_DivisionTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            Time expected = new Time(1F);
            Time actual;
            actual = (value1 / value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod]
        public void op_EqualityTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            bool expected = true;
            bool actual;
            actual = (value1 == value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Exponent
        ///</summary>
        [TestMethod]
        public void op_ExponentTest()
        {
            Time value1 = new Time(2F);
            Time value2 = new Time(3F);
            double expected = 8F;
            double actual;
            actual = Time.op_Exponent(value1, value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod]
        public void op_GreaterThanTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(9F);
            bool expected = true;
            bool actual;
            actual = (value1 > value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod]
        public void op_GreaterThanOrEqualTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            bool expected = true;
            bool actual;
            actual = (value1 >= value2);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        /////A test for op_Implicit
        /////</summary>
        //[TestMethod]
        //public void op_ImplicitTimeSpanTest()
        //{
        //    Time value = new Time(10F);
        //    TimeSpan expected = new TimeSpan(1, 1, 1);
        //    TimeSpan actual;
        //    actual = value;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitDoubleTest()
        {
            Time value = new Time(10F);
            double expected = 10F;
            double actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitTest2()
        {
            TimeSpan value = new TimeSpan(0, 0, 10);
            Time expected = new Time(10F);
            Time actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitTest3()
        {
            double value = 10F;
            Time expected = new Time(10F);
            Time actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod]
        public void op_InequalityTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(9F);
            bool expected = true;
            bool actual;
            actual = (value1 != value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod]
        public void op_LessThanTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(11F);
            bool expected = true;
            bool actual;
            actual = (value1 < value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [TestMethod]
        public void op_LessThanOrEqualTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            bool expected = true;
            bool actual;
            actual = (value1 <= value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Modulus
        ///</summary>
        [TestMethod]
        public void op_ModulusTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            Time expected = new Time(0F);
            Time actual;
            actual = (value1 % value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [TestMethod]
        public void op_MultiplyTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            Time expected = new Time(100F);
            Time actual;
            actual = (value1 * value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [TestMethod]
        public void op_SubtractionTest()
        {
            Time value1 = new Time(10F);
            Time value2 = new Time(10F);
            Time expected = new Time(0F);
            Time actual;
            actual = (value1 - value2);
            Assert.AreEqual(expected, actual);
        }
    }
}
