//******************************************************************************************************
//  EnergyTest.cs - Gbtc
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
    ///This is a test class for EnergyTest and is intended
    ///to contain all EnergyTest Unit Tests
    ///</summary>
    [TestClass]
    public class EnergyTest
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
        ///A test for Energy Constructor
        ///</summary>
        [TestMethod]
        public void EnergyConstructorTest()
        {
            List<Double> values = new List<Double>();

            //Initialization
            values.Add(0);

            foreach (Double value in values)
            {
                Energy target = new Energy(value);
                Assert.IsInstanceOfType(target, typeof(Energy));
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
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Energy"/>.</exception>
        [TestMethod]
        public void CompareToObjectTest()
        {

            Energy target = new Energy(10F);
            object value = new Energy(10F);
            int expected = 0;
            int actual;
            actual = target.CompareTo(value);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        /// A test for CompareTo
        /// Compares this instance to a specified <see cref="Energy"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An <see cref="Energy"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToEnergyTest()
        {
            Energy target = new Energy(10F);
            Energy value = new Energy(10F);
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
        /// <param name="value">A <see cref="Double"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        [TestMethod]
        public void CompareToDoubleTest()
        {
            Energy target = new Energy(10F);
            double value = 10F;
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
            Energy target = new Energy(10F);
            double obj = 10F;
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Energy"/> value.
        /// </summary>
        /// <param name="obj">An <see cref="Energy"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsEnergyTest()
        {
            Energy target = new Energy(10F);
            Energy obj = new Energy(10F);
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
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Energy"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        [TestMethod]
        public void EqualsObjectTest()
        {
            Energy target = new Energy(10F);
            object obj = new Energy(10F);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromBTU
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in BTU (International Table).
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in BTU.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in BTU.</returns>
        [TestMethod]
        public void FromBTUTest()
        {
            double value = 10F;
            Energy expected = new Energy(10550.5585262);
            Energy actual;
            actual = Energy.FromBTU(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromBarrelsOfOil
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in equivalent barrels of oil.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in equivalent barrels of oil.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in equivalent barrels of oil.</returns>
        [TestMethod]
        public void FromBarrelsOfOilTest()
        {
            double value = 10F;
            Energy expected = new Energy(61200000000);
            Energy actual;
            actual = Energy.FromBarrelsOfOil(value);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for FromCalories
        ///// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in calories (International Table).
        ///// </summary>
        ///// <param name="value">New <see cref="Energy"/> value in calories.</param>
        ///// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in calories.</returns>
        //[TestMethod]
        //public void FromCaloriesTest()
        //{
        //    double value = 10F;
        //    Energy expected = new Energy(41.868);
        //    Energy actual;
        //    actual = Energy.FromCalories(value);
        //    Assert.AreEqual(expected, actual);

        //}

        /// <summary>
        /// A test for FromCelsiusHeatUnits
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in Celsius heat units (International Table).
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in Celsius heat units.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in Celsius heat units.</returns>
        [TestMethod]
        public void FromCelsiusHeatUnitsTest()
        {
            double value = 10F;
            Energy expected = new Energy(18991.00534716);
            Energy actual;
            actual = Energy.FromCelsiusHeatUnits(value);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for FromHorsepowerHours
        ///// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in horsepower-hours.
        ///// </summary>
        ///// <param name="value">New <see cref="Energy"/> value in horsepower-hours.</param>
        ///// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in horsepower-hours.</returns>
        //[TestMethod]
        //public void FromHorsepowerHoursTest()
        //{
        //    double value = 10F;
        //    Energy expected = new Energy(26845195.3769617);
        //    Energy actual;
        //    actual = Energy.FromHorsepowerHours(value);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for FromLitersAtmosphere
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in liters-atmosphere.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in liters-atmosphere.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in liters-atmosphere.</returns>
        [TestMethod]
        public void FromLitersAtmosphereTest()
        {
            double value = 10F;
            Energy expected = new Energy(1013.25);
            Energy actual;
            actual = Energy.FromLitersAtmosphere(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromTonOfCoal
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in equivalent tons of coal.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in equivalent tons of coal.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in equivalent tons of coal.</returns>
        [TestMethod]
        public void FromTonOfCoalTest()
        {
            double value = 10F;
            Energy expected = new Energy(293076000000);
            Energy actual;
            actual = Energy.FromTonOfCoal(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for FromWattHours
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in watt-hours.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in watt-hours.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in watt-hours.</returns>
        [TestMethod]
        public void FromWattHoursTest()
        {
            double value = 10F;
            Energy expected = new Energy(36000);
            Energy actual;
            actual = Energy.FromWattHours(value);
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
            Energy target = new Energy(10F);
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
            Energy target = new Energy(10F);
            TypeCode expected = new TypeCode();
            expected = Type.GetTypeCode(typeof(Double));
            TypeCode actual;
            actual = target.GetTypeCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            IFormatProvider provider = null;
            Energy expected = new Energy(value);
            Energy actual;
            actual = Energy.Parse(s, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            Energy expected = new Energy(value);
            Energy actual;
            actual = Energy.Parse(s, style);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        [TestMethod]
        public void ParseTest()
        {
            double value = 10F;
            string s = value.ToString();
            Energy expected = new Energy(value);
            Energy actual;
            actual = Energy.Parse(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Parse
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        [TestMethod]
        public void ParseStyleProviderTest()
        {
            double value = 10F;
            string s = value.ToString();
            NumberStyles style = new NumberStyles();
            IFormatProvider provider = null;
            Energy expected = new Energy(value);
            Energy actual;
            actual = Energy.Parse(s, style, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToBoolean
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToBooleanTest()
        {
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10F);
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
        //    /*
        //    IConvertible target = new Energy(10F);
        //    IFormatProvider provider = null;
        //    char expected = '\0';
        //    char actual;
        //    actual = target.ToChar(provider);
        //    Assert.AreEqual(expected, actual);
        //    */
        //    Assert.Inconclusive("Invalid cast from 'Double' to 'Char'.");
        //}

        ///// <summary>
        /////A test for System.IConvertible.ToDateTime
        /////</summary>
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void ToDateTimeTest()
        //{
        //    /*
        //    IConvertible target = new Energy(10F);
        //    IFormatProvider provider = null;
        //    DateTime expected = new DateTime();
        //    DateTime actual;
        //    actual = target.ToDateTime(provider);
        //    Assert.AreEqual(expected, actual);
        //    */
        //    Assert.Inconclusive("Invalid cast from 'Double' to 'DateTime'.");
        //}

        /// <summary>
        ///A test for System.IConvertible.ToDecimal
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToDecimalTest()
        {
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10);
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
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10F);
            IFormatProvider provider = null;
            float expected = 10F;
            float actual;
            actual = target.ToSingle(provider);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        /////A test for System.IConvertible.ToType
        /////</summary>
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void ToTypeTest()
        //{
        //    IConvertible target = new Energy(10F);
        //    Type conversionType = target.GetType();
        //    IFormatProvider provider = null;
        //    object expected = new Energy(10F);
        //    object actual;
        //    actual = target.ToType(conversionType, provider);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for System.IConvertible.ToUInt16
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void ToUInt16Test()
        {
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10F);
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
            IConvertible target = new Energy(10F);
            IFormatProvider provider = null;
            ulong expected = 10;
            ulong actual;
            actual = target.ToUInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToBTU
        /// Gets the <see cref="Energy"/> value in BTU (International Table).
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in BTU.</returns>
        [TestMethod]
        public void ToBTUTest()
        {
            Energy target = new Energy(10F);
            double expected = 0.0094781712031331723;
            double actual;
            actual = target.ToBTU();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToBarrelsOfOil
        /// Gets the <see cref="Energy"/> value in equivalent barrels of oil.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in equivalent barrels of oil.</returns>
        [TestMethod]
        public void ToBarrelsOfOilTest()
        {
            Energy target = new Energy(10F);
            double expected = 0.0000000016339869281045752;
            double actual;
            actual = target.ToBarrelsOfOil();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToCalories
        /// Gets the <see cref="Energy"/> value in calories (International Table).
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in calories.</returns>
        [TestMethod]
        public void ToCaloriesTest()
        {
            Energy target = new Energy(10F);
            double expected = 2.3884589662749596;
            double actual;
            actual = target.ToCalories();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToCelsiusHeatUnits
        /// Gets the <see cref="Energy"/> value in Celsius heat units (International Table).
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in Celsius heat units.</returns>
        [TestMethod]
        public void ToCelsiusHeatUnitsTest()
        {
            Energy target = new Energy(10F);
            double expected = 0.0052656506684073175;
            double actual;
            actual = target.ToCelsiusHeatUnits();
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for ToCoulombs
        ///// Gets the <see cref="Charge"/> value in coulombs given the specified <paramref name="volts"/>.
        ///// </summary>
        ///// <param name="volts">Source <see cref="Voltage"/> used to calculate <see cref="Charge"/> value.</param>
        ///// <returns><see cref="Charge"/> value in coulombs given the specified <paramref name="volts"/>.</returns>
        //[TestMethod]
        //public void ToCoulombsTest()
        //{
        //    Energy target = new Energy(2F);
        //    Voltage volts = new Voltage(3F);
        //    Charge expected = new Charge(0.666666666666667);
        //    Charge actual;
        //    actual = target.ToCoulombs(volts);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for ToHorsepowerHours
        /// Gets the <see cref="Energy"/> value in horsepower-hours.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in horsepower-hours.</returns>
        [TestMethod]
        public void ToHorsepowerHoursTest()
        {
            Energy target = new Energy(10F);
            double expected = 0.0000037250613599861884;
            double actual;
            actual = target.ToHorsepowerHours();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToLitersAtmosphere
        /// Gets the <see cref="Energy"/> value in liters-atmosphere.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in liters-atmosphere.</returns>
        [TestMethod]
        public void ToLitersAtmosphereTest()
        {
            Energy target = new Energy(10F);
            double expected = 0.098692326671601285;
            double actual;
            actual = target.ToLitersAtmosphere();
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
            Energy target = new Energy(value);
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
            Energy target = new Energy(value);
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
            Energy target = new Energy(value);
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
            Energy target = new Energy(value);
            string format = string.Empty;
            string expected = value.ToString();
            string actual;
            actual = target.ToString(format);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToTonsOfCoal
        /// Gets the <see cref="Energy"/> value in equivalent tons of coal.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in equivalent tons of coal.</returns>
        [TestMethod]
        public void ToTonsOfCoalTest()
        {
            Energy target = new Energy(10F);
            double expected = 0.00000000034120842375356565;
            double actual;
            actual = target.ToTonsOfCoal();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToWattHours
        /// Gets the <see cref="Energy"/> value in watt-hours.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in watt-hours.</returns>
        [TestMethod]
        public void ToWattHoursTest()
        {
            Energy target = new Energy(10F);
            double expected = 0.0027777777777777779;
            double actual;
            actual = target.ToWattHours();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number to its <see cref="Energy"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Energy"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parajoule is null,
        /// is not of the correct format, or represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// This parajoule is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        [TestMethod]
        public void TryParseResultTest()
        {
            double value = 10F;
            string s = value.ToString();
            Energy result = new Energy(value);
            Energy resultExpected = new Energy(value);
            bool expected = true;
            bool actual;
            actual = Energy.TryParse(s, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for TryParse
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Energy"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Energy"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parajoule is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Energy.MinValue"/> or
        /// greater than <see cref="Energy.MaxValue"/>. This parajoule is passed uninitialized.
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
            IFormatProvider provider = null;
            Energy result = new Energy(value);
            Energy resultExpected = new Energy(value);
            bool expected = true;
            bool actual;
            actual = Energy.TryParse(s, style, provider, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [TestMethod]
        public void op_AdditionTest()
        {
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(10F);
            Energy expected = new Energy(20F);
            Energy actual;
            actual = (value1 + value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [TestMethod]
        public void op_DivisionTest()
        {
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(20F);
            Energy expected = new Energy(1 / 2F);
            Energy actual;
            actual = (value1 / value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod]
        public void op_EqualityTest()
        {
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(10F);
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
            Energy value1 = new Energy(2F);
            Energy value2 = new Energy(3F);
            double expected = 8F;
            double actual;
            actual = Energy.op_Exponent(value1, value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod]
        public void op_GreaterThanTest()
        {
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(9F);
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
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(10F);
            bool expected = true;
            bool actual;
            actual = (value1 >= value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitDoubleTest()
        {
            Energy value = new Energy(10F);
            double expected = 10F;
            double actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod]
        public void op_ImplicitEnergyTest()
        {
            double value = 10F;
            Energy expected = new Energy(value);
            Energy actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod]
        public void op_InequalityTest()
        {
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(10F);
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
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(11F);
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
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(11F);
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
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(20F);
            Energy expected = new Energy(10F);
            Energy actual;
            actual = (value1 % value2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [TestMethod]
        public void op_MultiplyTest()
        {
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(10F);
            Energy expected = new Energy(100F);
            Energy actual;
            actual = (value1 * value2);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [TestMethod]
        public void op_SubtractionTest()
        {
            Energy value1 = new Energy(10F);
            Energy value2 = new Energy(10F);
            Energy expected = new Energy(0F);
            Energy actual;
            actual = (value1 - value2);
            Assert.AreEqual(expected, actual);
        }
    }
}
