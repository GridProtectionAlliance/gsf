//******************************************************************************************************
//  VolumeTest.cs - Gbtc
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
    ///This is a test class for VolumeTest and is intended
    ///to contain all VolumeTest Unit Tests
    ///</summary>
    [TestClass]
    public class VolumeTest
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
        /// A test for Volume Constructor
        /// Creates a new <see cref="Volume"/>.
        /// </summary>
        /// <param name="value">New volume value in cubic meters.</param>
        [TestMethod]
        public void VolumeConstructorTest()
        {
            List<Double> values = new List<Double>();
            //Initialization
            values.Add(0);

            foreach (Double value in values)
            {
                Volume target = new Volume(value);
                Assert.IsInstanceOfType(target, typeof(Volume));
                Assert.IsNotNull(target);
            }
            values.Clear();
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
            Volume target = new Volume(10F);
            double value = 10F;
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified <see cref="Volume"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Volume"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToVolumeTest()
        {
            Volume target = new Volume(10F);
            Volume value = new Volume(10F);
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
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Volume"/>.</exception>
        [TestMethod]
        public void CompareToObjectTest()
        {
            Volume target = new Volume(10F);
            object value = new Volume(10F);
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Volume"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Volume"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsVolumeTest()
        {
            Volume target = new Volume(10F);
            Volume obj = new Volume(10F);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
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
            Volume target = new Volume(10F);
            double obj = 10F;
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
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Volume"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsObjectTest()
        {
            Volume target = new Volume(10F);
            object obj = new Volume(10F);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromCubicFeet
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in cubic feet.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in cubic feet.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in cubic feet.</returns>
        [TestMethod]
        public void FromCubicFeetTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.28316846592);
            Volume actual;
            actual = Volume.FromCubicFeet(value);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for FromCubicInches
        ///// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in cubic inches.
        ///// </summary>
        ///// <param name="value">New <see cref="Volume"/> value in cubic inches.</param>
        ///// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in cubic inches.</returns>
        //[TestMethod]
        //public void FromCubicInchesTest()
        //{
        //    double value = 10F;
        //    Volume expected = new Volume(0.00016387064);
        //    Volume actual;
        //    actual = Volume.FromCubicInches(value);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for FromCups
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US cups.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US cups.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US cups.</returns>
        [TestMethod]
        public void FromCupsTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.002365882365);
            Volume actual;
            actual = Volume.FromCups(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromFluidOunces
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid ounces.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid ounces.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid ounces.</returns>
        [TestMethod]
        public void FromFluidOuncesTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.000295735295625);
            Volume actual;
            actual = Volume.FromFluidOunces(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromGallons
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid gallons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid gallons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid gallons.</returns>
        [TestMethod]
        public void FromGallonsTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.03785411784);
            Volume actual;
            actual = Volume.FromGallons(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromLiters
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in liters.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in liters.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in liters.</returns>
        [TestMethod]
        public void FromLitersTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.01);
            Volume actual;
            actual = Volume.FromLiters(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromMetricCups
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in metric cups.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in metric cups.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in metric cups.</returns>
        [TestMethod]
        public void FromMetricCupsTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.0025);
            Volume actual;
            actual = Volume.FromMetricCups(value);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for FromMetricTablespoons
        ///// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in metric tablespoons.
        ///// </summary>
        ///// <param name="value">New <see cref="Volume"/> value in metric tablespoons.</param>
        ///// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in metric tablespoons.</returns>
        //[TestMethod]
        //public void FromMetricTablespoonsTest()
        //{
        //    double value = 10F;
        //    Volume expected = new Volume(0.00015);
        //    Volume actual;
        //    actual = Volume.FromMetricTablespoons(value);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for FromMetricTeaspoons
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in metric teaspoons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in metric teaspoons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in metric teaspoons.</returns>
        [TestMethod]
        public void FromMetricTeaspoonsTest()
        {
            double value = 10F;
            Volume expected = new Volume(5E-05);
            Volume actual;
            actual = Volume.FromMetricTeaspoons(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromPints
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid pints.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid pints.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid pints.</returns>
        [TestMethod]
        public void FromPintsTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.00473176473);
            Volume actual;
            actual = Volume.FromPints(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromQuarts
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US fluid quarts.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US fluid quarts.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US fluid quarts.</returns>
        [TestMethod]
        public void FromQuartsTest()
        {
            double value = 10F;
            Volume expected = new Volume(0.00946352946);
            Volume actual;
            actual = Volume.FromQuarts(value);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for FromTablespoons
        ///// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US tablespoons.
        ///// </summary>
        ///// <param name="value">New <see cref="Volume"/> value in US tablespoons.</param>
        ///// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US tablespoons.</returns>
        //[TestMethod]
        //public void FromTablespoonsTest()
        //{
        //    double value = 10F;
        //    Volume expected = new Volume(0.000147867647825);
        //    Volume actual;
        //    actual = Volume.FromTablespoons(value);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for FromTeaspoons
        /// Creates a new <see cref="Volume"/> value from the specified <paramref name="value"/> in US teaspoons.
        /// </summary>
        /// <param name="value">New <see cref="Volume"/> value in US teaspoons.</param>
        /// <returns>New <see cref="Volume"/> object from the specified <paramref name="value"/> in US teaspoons.</returns>
        [TestMethod]
        public void FromTeaspoonsTest()
        {
            double value = 10F;
            Volume expected = new Volume(4.928921595E-05);
            Volume actual;
            actual = Volume.FromTeaspoons(value);
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
            Volume target = new Volume(10F);
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
            Volume target = new Volume(10F);
            TypeCode expected = new TypeCode();
            expected = Type.GetTypeCode(typeof(Double));
            TypeCode actual;
            actual = target.GetTypeCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            Volume expected = new Volume(value);
            Volume actual;
            actual = Volume.Parse(s, style);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseStringTest()
        {
            double value = 10F;
            string s = value.ToString();
            Volume expected = new Volume(value);
            Volume actual;
            actual = Volume.Parse(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            IFormatProvider provider = null;
            Volume expected = new Volume(value);
            Volume actual;
            actual = Volume.Parse(s, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Volume"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Volume"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            IFormatProvider provider = null;
            Volume expected = new Volume(value);
            Volume actual;
            actual = Volume.Parse(s, style, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToBoolean
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToBooleanTest()
        {
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            Assert.Inconclusive("Can't cast from 'Double' to 'Char'");
        }

        /// <summary>
        ///A test for System.IConvertible.ToDateTime
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDateTimeTest()
        {
            Assert.Inconclusive("Can't cast from 'Double' to 'DateTime'.");
        }

        /// <summary>
        ///A test for System.IConvertible.ToDecimal
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDecimalTest()
        {
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
            Type conversionType = typeof(Double);
            IFormatProvider provider = null;
            object expected = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
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
            IConvertible target = new Volume(10F);
            IFormatProvider provider = null;
            ulong expected = 10;
            ulong actual;
            actual = target.ToUInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToCubicFeet
        /// Gets the <see cref="Volume"/> value in cubic feet.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in cubic feet.</returns>
        [TestMethod]
        public void ToCubicFeetTest()
        {
            Volume target = new Volume(10F);
            double expected = 353.14666721488589;
            double actual;
            actual = target.ToCubicFeet();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToCubicInches
        /// Gets the <see cref="Volume"/> value in cubic inches.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in cubic inches.</returns>
        [TestMethod]
        public void ToCubicInchesTest()
        {
            Volume target = new Volume(10F);
            double expected = 610237.44094732287;
            double actual;
            actual = target.ToCubicInches();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToCups
        /// Gets the <see cref="Volume"/> value in US cups.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US cups.</returns>
        [TestMethod]
        public void ToCupsTest()
        {
            Volume target = new Volume(10F);
            double expected = 42267.528377303744;
            double actual;
            actual = target.ToCups();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToFluidOunces
        ///</summary>
        [TestMethod]
        public void ToFluidOuncesTest()
        {
            Volume target = new Volume(10F);
            double expected = 338140.22701842996;
            double actual;
            actual = target.ToFluidOunces();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToGallons
        /// Gets the <see cref="Volume"/> value in US fluid gallons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US fluid gallons.</returns>
        [TestMethod]
        public void ToGallonsTest()
        {
            Volume target = new Volume(10F);
            double expected = 2641.720523581484;
            double actual;
            actual = target.ToGallons();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToLiters
        /// Gets the <see cref="Volume"/> value in liters.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in liters.</returns>
        [TestMethod]
        public void ToLitersTest()
        {
            Volume target = new Volume(10F);
            double expected = 10000.0;
            double actual;
            actual = target.ToLiters();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToMetricCupsv
        /// Gets the <see cref="Volume"/> value in metric cups.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in metric cups.</returns>
        [TestMethod]
        public void ToMetricCupsTest()
        {
            Volume target = new Volume(10F);
            double expected = 40000.0;
            double actual;
            actual = target.ToMetricCups();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToMetricTablespoons
        /// Gets the <see cref="Volume"/> value in metric tablespoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in metric tablespoons.</returns>
        [TestMethod]
        public void ToMetricTablespoonsTest()
        {
            Volume target = new Volume(10F);
            double expected = 666666.66666666663;
            double actual;
            actual = target.ToMetricTablespoons();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToMetricTeaspoons
        /// Gets the <see cref="Volume"/> value in metric teaspoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in metric teaspoons.</returns>
        [TestMethod]
        public void ToMetricTeaspoonsTest()
        {
            Volume target = new Volume(10F);
            double expected = 1999999.9999999998;
            double actual;
            actual = target.ToMetricTeaspoons();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToPints
        /// Gets the <see cref="Volume"/> value in US fluid pints.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US fluid pints.</returns>
        [TestMethod]
        public void ToPintsTest()
        {
            Volume target = new Volume(10F);
            double expected = 21133.764188651872;
            double actual;
            actual = target.ToPints();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToQuarts
        /// Gets the <see cref="Volume"/> value in US fluid quarts.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US fluid quarts.</returns>
        [TestMethod]
        public void ToQuartsTest()
        {
            Volume target = new Volume(10F);
            double expected = 10566.882094325936;
            double actual;
            actual = target.ToQuarts();
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
            Volume target = new Volume(value);
            string format = string.Empty;
            IFormatProvider provider = null;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(format, provider);
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
            Volume target = new Volume(value);
            string expected = value.ToString();
            string actual;
            actual = target.ToString();
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
            Volume target = new Volume(value);
            string format = string.Empty;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(format);
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
            Volume target = new Volume(value);
            IFormatProvider provider = null;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToTablespoons
        /// Gets the <see cref="Volume"/> value in US tablespoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US tablespoons.</returns>
        [TestMethod]
        public void ToTablespoonsTest()
        {
            Volume target = new Volume(10F);
            double expected = 676280.45397969056;
            double actual;
            actual = target.ToTablespoons();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToTeaspoons
        /// Gets the <see cref="Volume"/> value in US teaspoons.
        /// </summary>
        /// <returns>Value of <see cref="Volume"/> in US teaspoons.</returns>
        [TestMethod]
        public void ToTeaspoonsTest()
        {
            Volume target = new Volume(10F);
            double expected = 2028841.3615960551;
            double actual;
            actual = target.ToTeaspoons();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number to its <see cref="Volume"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Volume"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s paraampere is null,
        /// is not of the correct format, or represents a number less than <see cref="Volume.MinValue"/> or greater than <see cref="Volume.MaxValue"/>.
        /// This paraampere is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        [TestMethod]
        public void TryParseResultTest()
        {
            double value = 10F;
            string s = value.ToString();
            Volume result = new Volume(value);
            Volume resultExpected = new Volume(value);
            bool expected = true;
            bool actual;
            actual = Volume.TryParse(s, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Volume"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Volume"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s paraampere is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Volume.MinValue"/> or
        /// greater than <see cref="Volume.MaxValue"/>. This paraampere is passed uninitialized.
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
        public void TryParseStyleProviderResultTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            IFormatProvider provider = null;
            Volume result = new Volume(value);
            Volume resultExpected = new Volume(value);
            bool expected = true;
            bool actual;
            actual = Volume.TryParse(s, style, provider, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [TestMethod]
        public void op_AdditionTest()
        {
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
            Volume expected = new Volume(20F);
            Volume actual;
            actual = (value1 + value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [TestMethod]
        public void op_DivisionTest()
        {
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
            Volume expected = new Volume(1F);
            Volume actual;
            actual = (value1 / value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod]
        public void op_EqualityTest()
        {
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
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
            Volume value1 = new Volume(2F);
            Volume value2 = new Volume(3F);
            double expected = 8F;
            double actual;
            actual = Volume.op_Exponent(value1, value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod]
        public void op_GreaterThanTest()
        {
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(9F);
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
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
            bool expected = true;
            bool actual;
            actual = (value1 >= value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitTest()
        {
            Volume value = new Volume(10F);
            double expected = 10F;
            double actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitTest1()
        {
            double value = 10F;
            Volume expected = new Volume(10F);
            Volume actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod]
        public void op_InequalityTest()
        {
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
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
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(11F);
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
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
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
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
            Volume expected = new Volume(0F);
            Volume actual;
            actual = (value1 % value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [TestMethod]
        public void op_MultiplyTest()
        {
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
            Volume expected = new Volume(100F);
            Volume actual;
            actual = (value1 * value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [TestMethod]
        public void op_SubtractionTest()
        {
            Volume value1 = new Volume(10F);
            Volume value2 = new Volume(10F);
            Volume expected = new Volume(0F);
            Volume actual;
            actual = (value1 - value2);
            Assert.AreEqual(expected, actual);
        }
    }
}
