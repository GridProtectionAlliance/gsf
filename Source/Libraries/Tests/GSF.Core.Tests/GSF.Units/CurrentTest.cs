//******************************************************************************************************
//  CurrentTest.cs - Gbtc
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
    ///This is a test class for CurrentTest and is intended
    ///to contain all CurrentTest Unit Tests
    ///</summary>
    [TestClass]
    public class CurrentTest
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
        ///A test for Current Constructor
        ///</summary>
        [TestMethod]
        public void CurrentConstructorTest()
        {

            List<Double> values = new List<Double>();

            //Initialization
            values.Add(0);

            foreach (Double value in values)
            {
                Current target = new Current(value);
                Assert.IsInstanceOfType(target, typeof(Current));
                Assert.IsNotNull(target);
            }
            values.Clear();
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified <see cref="Current"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Current"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToCurrentTest()
        {
            //equality
            Current target = new Current(10F);
            Current value = new Current(10F);
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified <see cref="double"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToDoubleTest()
        {
            //equality
            Current target = new Current(10F);
            double value = 10F;
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
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
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Current"/>.</exception>
        [TestMethod]
        public void CompareToObjectTest()
        {
            //equality
            Current target = new Current(10F);
            object value = new Current(10F);
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Double"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Double"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsDoubleTest()
        {
            Current target = new Current(10F);
            double obj = 10F;
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Current"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Current"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsCurrentTest()
        {
            Current target = new Current(10F);
            Current obj = new Current(10F);
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
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Current"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsObjectTest()
        {
            Current target = new Current(10F);
            object obj = new Current(10F);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for FromAbamperes
        ///// Creates a new <see cref="Current"/> value from the specified <paramref name="value"/> in abamperes.
        ///// </summary>
        ///// <param name="value">New <see cref="Current"/> value in abamperes.</param>
        ///// <returns>New <see cref="Current"/> object from the specified <paramref name="value"/> in abamperes.</returns>
        //[TestMethod]
        //public void FromAbamperesTest()
        //{
        //    double value = 10F;
        //    Current expected = new Current();
        //    Current actual;
        //    actual = Current.FromAbamperes(value);
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        ///// A test for FromStatamperes
        ///// Creates a new <see cref="Current"/> value from the specified <paramref name="value"/> in statamperes.
        ///// </summary>
        ///// <param name="value">New <see cref="Current"/> value in statamperes.</param>
        ///// <returns>New <see cref="Current"/> object from the specified <paramref name="value"/> in statamperes.</returns>
        //[TestMethod]
        //public void FromStatamperesTest()
        //{
        //    double value = 10F;
        //    Current expected = new Current(10F);
        //    Current actual;
        //    actual = Current.FromStatamperes(value);
        //    Assert.AreEqual(expected, actual);
        //}

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
            Current target = new Current(10F);
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
            Current target = new Current(10F);
            TypeCode expected = new TypeCode();
            expected = TypeCode.Double;
            TypeCode actual;
            actual = target.GetTypeCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style to its <see cref="Current"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// A <see cref="Current"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Current.MinValue"/> or greater than <see cref="Current.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            style = NumberStyles.Any;
            Current expected = new Current(10F);
            Current actual;
            actual = Current.Parse(s, style);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Current"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Current"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Current.MinValue"/> or greater than <see cref="Current.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            IFormatProvider provider = null;
            Current expected = new Current(10F);
            Current actual;
            actual = Current.Parse(s, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Current"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Current"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Current.MinValue"/> or greater than <see cref="Current.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            style = NumberStyles.Any;
            IFormatProvider provider = null;
            Current expected = new Current(10F);
            Current actual;
            actual = Current.Parse(s, style, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number to its <see cref="Current"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A <see cref="Current"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Current.MinValue"/> or greater than <see cref="Current.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseTest()
        {
            double value = 10F;
            string s = value.ToString();
            Current expected = new Current(10F);
            Current actual;
            actual = Current.Parse(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToBoolean
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToBooleanTest()
        {
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
            IFormatProvider provider = null;
            byte expected = 10;
            byte actual;
            actual = target.ToByte(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToChar
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToCharTest()
        {
            Assert.Inconclusive("Invalid cast from 'Double' to 'Char'.");
        }

        /// <summary>
        ///A test for System.IConvertible.ToDateTime
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDateTimeTest()
        {
            Assert.Inconclusive("Invalid cast from 'Double' to 'DateTime'.");
        }

        /// <summary>
        ///A test for System.IConvertible.ToDecimal
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDecimalTest()
        {
            IConvertible target = new Current(10F);
            IFormatProvider provider = null;
            Decimal expected = new Decimal(10);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
            Type conversionType = typeof(Double);
            IFormatProvider provider = null;
            object expected = new Current(10F);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
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
            IConvertible target = new Current(10F);
            IFormatProvider provider = null;
            ulong expected = 10;
            ulong actual;
            actual = target.ToUInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToAbamperes
        /// Gets the <see cref="Current"/> value in abamperes.
        /// </summary>
        /// <returns>Value of <see cref="Current"/> in abamperes.</returns>
        [TestMethod]
        public void ToAbamperesTest()
        {
            Current target = new Current(10F);
            double expected = 1F;
            double actual;
            actual = target.ToAbamperes();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToStatamperes
        /// Gets the <see cref="Current"/> value in statamperes.
        /// </summary>
        /// <returns>Value of <see cref="Current"/> in statamperes.</returns>
        [TestMethod]
        public void ToStatamperesTest()
        {
            Current target = new Current(10F);
            double expected = 29979245368.431435;
            double actual;
            actual = target.ToStatamperes();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// The string representation of the value of this instance, consisting of a minus sign if
        /// the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.
        /// </returns>
        [TestMethod]
        public void ToStringTest()
        {
            double value = 10F;
            Current target = new Current(10F);
            string expected = value.ToString();
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString
        /// Converts the numeric value of this instance to its equivalent string representation using the
        /// specified culture-specific format information.
        /// </summary>
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
            Current target = new Current(10F);
            IFormatProvider provider = null;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString
        /// Converts the numeric value of this instance to its equivalent string representation using the
        /// specified format and culture-specific format information.
        /// </summary>
        /// <param name="format">A format specification.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format and provider.
        /// </returns>
        [TestMethod]
        public void ToStringFormatProviderTest()
        {
            double value = 10F;
            Current target = new Current(value);
            string format = string.Empty;
            IFormatProvider provider = null;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(format, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString
        /// Converts the numeric value of this instance to its equivalent string representation, using
        /// the specified format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format.
        /// </returns>
        [TestMethod]
        public void ToStringFormatTest()
        {
            double value = 10F;
            Current target = new Current(value);
            string format = string.Empty;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(format);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Current"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Current"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s paraampere is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Current.MinValue"/> or
        /// greater than <see cref="Current.MaxValue"/>. This paraampere is passed uninitialized.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> object that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        [TestMethod]
        public void TryParseTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            IFormatProvider provider = null;
            Current result = new Current(value);
            Current resultExpected = new Current(value);
            bool expected = true;
            bool actual;
            actual = Current.TryParse(s, style, provider, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number to its <see cref="Current"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Current"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s paraampere is null,
        /// is not of the correct format, or represents a number less than <see cref="Current.MinValue"/> or greater than <see cref="Current.MaxValue"/>.
        /// This paraampere is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        [TestMethod]
        public void TryParseOutTest()
        {
            double value = 10F;
            string s = value.ToString();
            Current result = new Current(value);
            Current resultExpected = new Current(value);
            bool expected = true;
            bool actual;
            actual = Current.TryParse(s, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [TestMethod]
        public void op_AdditionTest()
        {
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
            Current expected = new Current(20F);
            Current actual;
            actual = (value1 + value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [TestMethod]
        public void op_DivisionTest()
        {
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
            Current expected = new Current(1F);
            Current actual;
            actual = (value1 / value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod]
        public void op_EqualityTest()
        {
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
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
            Current value1 = new Current(2F);
            Current value2 = new Current(3F);
            double expected = 8F;
            double actual;
            actual = Current.op_Exponent(value1, value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod]
        public void op_GreaterThanTest()
        {
            Current value1 = new Current(10F);
            Current value2 = new Current(9F);
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
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
            bool expected = true;
            bool actual;
            actual = (value1 >= value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitCurrentTest()
        {
            Current value = new Current(10F);
            double expected = 10F;
            double actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitDoubleTest()
        {
            double value = 10F;
            Current expected = new Current(value);
            Current actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod]
        public void op_InequalityTest()
        {
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
            bool expected = false;
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
            Current value1 = new Current(10F);
            Current value2 = new Current(11F);
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
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
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
            Current value1 = new Current(10F);
            Current value2 = new Current(20F);
            Current expected = new Current(10F);
            Current actual;
            actual = (value1 % value2);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [TestMethod]
        public void op_MultiplyTest()
        {
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
            Current expected = new Current(100F);
            Current actual;
            actual = (value1 * value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [TestMethod]
        public void op_SubtractionTest()
        {
            Current value1 = new Current(10F);
            Current value2 = new Current(10F);
            Current expected = new Current(0F);
            Current actual;
            actual = (value1 - value2);
            Assert.AreEqual(expected, actual);
        }
    }
}
