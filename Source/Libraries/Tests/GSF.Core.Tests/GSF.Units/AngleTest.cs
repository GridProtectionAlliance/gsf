//******************************************************************************************************
//  AngleTest.cs - Gbtc
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
    ///This is a test class for AngleTest and is intended
    ///to contain all AngleTest Unit Tests
    ///</summary>
    [TestClass]
    public class AngleTest
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
        ///A test for Angle Constructor
        ///</summary>
        [TestMethod]
        public void AngleConstructorTest()
        {
            //Test Values Collection
            List<Double> values = new List<Double>();

            //Initialization
            values.Add(0);

            foreach (Double value in values)
            {
                Angle target = new Angle(value);
                Assert.IsInstanceOfType(target, typeof(Angle));
                Assert.IsNotNull(target);
            }

            values.Clear();

        }

        /// <summary>
        /// A test for CompareTo
        /// A signed number indicating the relative values of this instance and value.
        /// </summary>
        /// <param name="value">An object to compare, or null.</param>
        /// <returns>
        /// Returns negative if this instance is less than value.
        /// Returns zero if this instance is equal to value.
        /// Returns greater than zero if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Angle"/>.</exception>
        [TestMethod]
        public void CompareToTest()
        {
            double value_target = 10F;
            double value_exp_negative = 11F;
            double value_exp_zero = 10F;
            double value_exp_pos = 9F;

            Angle angle = new Angle(value_target);
            Angle angle_exp_negative = new Angle(value_exp_negative);
            Angle angle_exp_zero = new Angle(value_exp_zero);
            Angle angle_exp_pos = new Angle(value_exp_pos);

            bool target = true;
            bool expected = (angle.CompareTo(angle_exp_negative) < 0) && (angle.CompareTo(angle_exp_zero) == 0) && (angle.CompareTo(angle_exp_pos) > 0);
            Assert.AreEqual(target, expected);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Angle"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsTest()
        {
            object angle = 10D;
            Angle target = new Angle(10D);
            bool expected = true;
            Assert.AreEqual(target.Equals(angle), expected);
        }

        ///// <summary>
        ///// A test for FromAngularMil
        ///// Creates a new <see cref="Angle"/> value from the specified <paramref name="value"/> in angular mil.
        ///// </summary>
        ///// <param name="value">New <see cref="Angle"/> value in angular mil.</param>
        ///// <returns>New <see cref="Angle"/> object from the specified <paramref name="value"/> in angular mil.</returns>
        //[TestMethod]
        //public void FromAngularMilTest()
        //{
        //    Angle target = Angle.FromAngularMil(10F);
        //    Angle expected = new Angle(0.0098174770424681);
        //    Assert.AreEqual(target, expected);
        //}

        ///// <summary>
        ///// A test for FromArcMinutes
        ///// Creates a new <see cref="Angle"/> value from the specified <paramref name="value"/> in arcminutes.
        ///// </summary>
        ///// <param name="value">New <see cref="Angle"/> value in arcminutes.</param>
        ///// <returns>New <see cref="Angle"/> object from the specified <paramref name="value"/> in arcminutes.</returns>
        //[TestMethod]
        //public void FromArcMinutesTest()
        //{
        //    Angle target = Angle.FromArcMinutes(10F);
        //    Angle expected = new Angle(0.00290888208665722);
        //    Assert.AreEqual(target, expected);
        //}

        ///// <summary>
        ///// A test for FromArcSeconds
        ///// Creates a new <see cref="Angle"/> value from the specified <paramref name="value"/> in arcseconds.
        ///// </summary>
        ///// <param name="value">New <see cref="Angle"/> value in arcseconds.</param>
        ///// <returns>New <see cref="Angle"/> object from the specified <paramref name="value"/> in arcseconds.</returns>
        //[TestMethod]
        //public void FromArcSecondsTest()
        //{
        //    Angle target = Angle.FromArcSeconds(1000F);
        //    Angle expected = new Angle(0.00484813681109536);
        //    Assert.AreEqual(target, expected);
        //}

        /// <summary>
        /// A test for FromDegrees
        /// Creates a new <see cref="Angle"/> value from the specified <paramref name="value"/> in degrees.
        /// </summary>
        /// <param name="value">New <see cref="Angle"/> value in degrees.</param>
        /// <returns>New <see cref="Angle"/> object from the specified <paramref name="value"/> in degrees.</returns>
        [TestMethod]
        public void FromDegreesTest()
        {
            Angle target = Angle.FromDegrees(10D);
            Angle expected = new Angle(0.174532925199433);
            Assert.AreEqual(expected, target, 0.000000000000001);
        }

        ///// <summary>
        ///// A test for FromGrads
        ///// Creates a new <see cref="Angle"/> value from the specified <paramref name="value"/> in grads.
        ///// </summary>
        ///// <param name="value">New <see cref="Angle"/> value in grads.</param>
        ///// <returns>New <see cref="Angle"/> object from the specified <paramref name="value"/> in grads.</returns>
        //[TestMethod]
        //public void FromGradsTest()
        //{
        //    Angle target = Angle.FromGrads(10F);
        //    Angle expected = new Angle(0.15707963267949);
        //    Assert.AreEqual(target, expected);
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
            double value = 10;
            Angle target = new Angle(value);
            int expected = 1076101120;
            Assert.AreEqual(target.GetHashCode(), expected);
        }

        /// <summary>
        /// A test for GetTypeCode
        /// Returns the <see cref="TypeCode"/> for value type <see cref="Double"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.Double"/>.</returns>
        [TestMethod]
        public void GetTypeCodeTest()
        {
            double value = 10F;
            Angle target = new Angle(value);
            TypeCode expected = value.GetTypeCode();
            Assert.AreEqual(target.GetTypeCode(), expected);
        }

        /// <summary>
        /// A test for Parse (String)
        /// Converts the string representation of a number in a specified style to its <see cref="Angle"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// An <see cref="Angle"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Angle.MinValue"/> or greater than <see cref="Angle.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            style = NumberStyles.Any;
            Angle expected = new Angle(value);
            Angle actual = Angle.Parse(s, style);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse (String)
        /// Converts the string representation of a number to its <see cref="Angle"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// An <see cref="Angle"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Angle.MinValue"/> or greater than <see cref="Angle.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseStringTest()
        {
            double value = 10F;
            string s = value.ToString();
            Angle expected = new Angle(value);
            Angle actual = Angle.Parse(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToBoolean
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToBooleanTest()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            bool expected = true;
            bool actual;
            actual = target.ToBoolean(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToByte
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToByteTest()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            byte expected = 10;
            byte actual;
            actual = target.ToByte(provider);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for System.IConvertible.ToChar
        ///// These are explicitly implemented on the native System.Double implementations, so we do the same...
        ///// </summary>
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void ToCharTest()
        //{
        //    /*
        //    double value = 10F;
        //    IConvertible target = new Angle(value);
        //    IFormatProvider provider = null;
        //    char expected = '\0';
        //    char actual= target.ToChar(provider);
        //    Assert.AreEqual(expected, actual);
        //    * */
        //    Assert.Inconclusive("Invalid cast from 'Double' to 'Char'.");

        //}

        ///// <summary>
        ///// A test for System.IConvertible.ToDateTime
        ///// These are explicitly implemented on the native System.Double implementations, so we do the same...
        ///// </summary>
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void ToDateTimeTest()
        //{
        //    /*
        //    double value = 10F;
        //    IConvertible target = new Angle(value);
        //    IFormatProvider provider = null;
        //    DateTime expected = new DateTime();
        //    DateTime actual = target.ToDateTime(provider);
        //    Assert.AreEqual(expected, actual);
        //    */
        //    Assert.Inconclusive("Invalid cast from 'Double' to 'DateTime'.");
        //}

        /// <summary>
        /// A test for System.IConvertible.ToDecimal
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDecimalTest()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            Decimal expected = new Decimal(value);
            Decimal actual = target.ToDecimal(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToDouble
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDoubleTest()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            double expected = 10F;
            double actual = target.ToDouble(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToInt16
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToInt16Test()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            short expected = 10;
            short actual;
            actual = target.ToInt16(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToInt32
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToInt32Test()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            int expected = 10;
            int actual;
            actual = target.ToInt32(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToInt64
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToInt64Test()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            long expected = 10;
            long actual;
            actual = target.ToInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToSByte
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToSByteTest()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            sbyte expected = 10;
            sbyte actual;
            actual = target.ToSByte(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToSingle
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToSingleTest()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            float expected = 10F;
            float actual;
            actual = target.ToSingle(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToType
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToTypeTest()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            Type conversionType = typeof(Double);
            IFormatProvider provider = null;
            object actual = target.ToType(conversionType, provider);
            Assert.IsInstanceOfType(actual, conversionType);
        }

        /// <summary>
        /// A test for System.IConvertible.ToUInt16
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToUInt16Test()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            ushort expected = 10;
            ushort actual;
            actual = target.ToUInt16(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToUInt32
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToUInt32Test()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            uint expected = 10;
            uint actual;
            actual = target.ToUInt32(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for System.IConvertible.ToUInt64
        /// These are explicitly implemented on the native System.Double implementations, so we do the same...
        /// </summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToUInt64Test()
        {
            double value = 10F;
            IConvertible target = new Angle(value);
            IFormatProvider provider = null;
            ulong expected = 10;
            ulong actual;
            actual = target.ToUInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToAngularMil
        /// </summary>
        [TestMethod]
        public void ToAngularMilTest()
        {
            double value = 10F;
            Angle target = new Angle(value);
            double expected = 10185.9163578813;
            double actual;
            actual = target.ToAngularMil();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToArcMinutes
        /// </summary>
        [TestMethod]
        public void ToArcMinutesTest()
        {
            double value = 10F;
            Angle target = new Angle(value);
            double expected = 34377.467707849391;
            double actual;
            actual = target.ToArcMinutes();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToArcSeconds
        /// </summary>
        [TestMethod]
        public void ToArcSecondsTest()
        {
            double value = 10F;
            Angle target = new Angle(value);
            double expected = 2062648.0624709637;
            double actual;
            actual = target.ToArcSeconds();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToDegrees
        /// Gets the <see cref="Angle"/> value in degrees.
        /// </summary>
        /// <returns>Value of <see cref="Angle"/> in degrees.</returns>
        [TestMethod]
        public void ToDegreesTest()
        {
            double value = 10F;
            Angle target = new Angle(value);
            double expected = 572.95779513082323;
            double actual = target.ToDegrees();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToGrads
        /// Gets the <see cref="Angle"/> value in grads.
        /// </summary>
        /// <returns>Value of <see cref="Angle"/>in grads.</returns>
        [TestMethod]
        public void ToGradsTest()
        {
            double value = 10F;
            Angle target = new Angle(value);
            double expected = 636.61977236758128;
            double actual = target.ToGrads();
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
        public void ToStringTest()
        {
            double value = 10F;
            Angle target = new Angle(value);
            string format = string.Empty;
            IFormatProvider provider = null;
            string expected = value.ToString();
            string actual = target.ToString(format, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Angle"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Angle"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Angle.MinValue"/> or
        /// greater than <see cref="Angle.MaxValue"/>. This parameter is passed uninitialized.
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
        public void TryParseStyleProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            style = NumberStyles.Any;
            IFormatProvider provider = null;
            Angle result = new Angle(value);
            Angle resultExpected = new Angle(value);
            bool expected = true;
            bool actual = Angle.TryParse(s, style, provider, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number to its <see cref="Angle"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Angle"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not of the correct format, or represents a number less than <see cref="Angle.MinValue"/> or greater than <see cref="Angle.MaxValue"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        [TestMethod]
        public void TryParseResultTest()
        {
            double value = 10F;
            string s = value.ToString();
            Angle result = new Angle(value);
            Angle resultExpected = new Angle(value);
            bool expected = true;
            bool actual = Angle.TryParse(s, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [TestMethod]
        public void op_AdditionTest()
        {
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(10F);
            Angle expected = new Angle(20F);

            Angle actual = (value1 + value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [TestMethod]
        public void op_DivisionTest()
        {

            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(20F);
            Angle expected = new Angle(1 / 2F);

            Angle actual = (value1 / value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod]
        public void op_EqualityTest()
        {
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(10F);

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
            Angle value1 = new Angle(2F);
            Angle value2 = new Angle(3F);
            double expected = 8F;
            double actual = Angle.op_Exponent(value1, value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod]
        public void op_GreaterThanTest()
        {
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(9F);
            bool expected = true;
            bool actual = (value1 > value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod]
        public void op_GreaterThanOrEqualTest()
        {
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(10F);
            bool expected = true;
            bool actual;
            actual = (value1 >= value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitAngleTest()
        {
            Angle expected = new Angle(10F);
            Angle actual = new Angle(10F);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitDoubleTest()
        {
            Angle value = new Angle(10F);
            double expected = 10F;
            double actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod]
        public void op_InequalityTest()
        {
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(11F);
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
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(11F);
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
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(10F);
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
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(10F);
            Angle expected = new Angle(0F);
            Angle actual;
            actual = (value1 % value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [TestMethod]
        public void op_MultiplyTest()
        {
            Angle value1 = new Angle(10F);
            Angle value2 = new Angle(10F);
            Angle expected = new Angle(100F);
            Angle actual = (value1 * value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [TestMethod]
        public void op_SubtractionTest()
        {
            Angle target1 = new Angle(10F);
            Angle target2 = new Angle(10F);

            Angle actual = new Angle(0F);
            Angle expected = target1 - target2;

            Assert.AreEqual(expected, actual);

        }
    }
}