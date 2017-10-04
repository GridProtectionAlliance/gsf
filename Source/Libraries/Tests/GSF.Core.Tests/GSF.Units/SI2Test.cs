//******************************************************************************************************
//  SI2Test.cs - Gbtc
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
using GSF.Units;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{
    /*
    Prefixes for binary multiples
    --------------------------------------------------------------------------------

    Factor Name Symbol Origin Derivation
    2 10 kibi Ki kilobinary: (2 10)1 kilo: (103)1
    2 20 mebi Mi megabinary: (2 10)2 mega: (103)2
    2 30 gibi Gi gigabinary: (2 10)3 giga: (103)3
    2 40 tebi Ti terabinary: (2 10)4 tera: (103)4
    2 50 pebi Pi petabinary: (2 10)5 peta: (103)5
    2 60 exbi Ei exabinary: (2 10)6 exa: (103)6

    */

    /// <summary>
    ///This is a test class for SI2Test and is intended
    ///to contain all SI2Test Unit Tests
    ///</summary>
    [TestClass]
    public class SI2Test
    {

        // virtual structure for SI unit
        struct sSI2
        {

            #region DECLARATION
            private const int binary = 2;
            double value
            {
                get
                {
                    return value;
                }
                set
                {
                    value = Math.Pow(binary, factor);
                }
            }
            int factor
            {
                get;
                set;
            }
            String name
            {
                get;
                set;
            }
            String symbol
            {
                get;
                set;
            }
            String origin
            {
                get;
                set;
            }
            String derivation
            {
                get;
                set;
            }
            #endregion

            #region CONSTRUCTOR
            public void Set(int factor, String name, String symbol, String origin, String derivation)
            {
                this.factor = factor;
                this.name = name;
                this.symbol = symbol;
                this.origin = origin;
                this.derivation = derivation;
            }
            #endregion

            #region METHODS
            public bool Find(Double value)
            {
                return Find(value.ToString());
            }
            public bool Find(String value)
            {
                bool status = false;
                if (value.ToLower() == value.ToLower() ||
                value.ToLower() == factor.ToString().ToLower() ||
                value.ToLower() == name.ToLower() ||
                value.ToLower() == symbol.ToLower() ||
                value.ToLower() == origin.ToLower() ||
                value.ToLower() == derivation.ToLower())
                {
                    status = true;
                }
                return status;
            }
            #endregion
        }

        //virtual class to hold SI units table
        class oSI2
        {
            #region DECLARATION

            readonly sSI2 virt_SI2 = new sSI2();
            readonly List<sSI2> obj_SI2 = new List<sSI2>(22);
            #endregion

            #region CONSTRUCTOR
            public oSI2()
            {
                virt_SI2.Set(10, "kibi", "Ki", "kilobinary", "kilo");
                obj_SI2.Add(virt_SI2);
                virt_SI2.Set(20, "mebi", "Mi", "megabinary", "mega");
                obj_SI2.Add(virt_SI2);
                virt_SI2.Set(30, "gibi", "Gi", "gigabinary", "giga");
                obj_SI2.Add(virt_SI2);
                virt_SI2.Set(40, "tebi", "Ti", "terabinary", "tera");
                obj_SI2.Add(virt_SI2);
                virt_SI2.Set(50, "pebi", "Pi", "petabinary", "peta");
                obj_SI2.Add(virt_SI2);
                virt_SI2.Set(60, "exbi", "Ei", "exabinary", "exa");
                obj_SI2.Add(virt_SI2);
            }
            #endregion CONSTRUCTOR

            #region METHODS
            private IEnumerable<bool> IsExists(Double value)
            {
                foreach (sSI2 obj in obj_SI2)
                {
                    yield return obj.Find(value);
                }
            }
            private IEnumerable<bool> IsExists(String value)
            {
                foreach (sSI2 obj in obj_SI2)
                {
                    yield return obj.Find(value);
                }
            }
            private bool DoesExists(String value)
            {
                bool status = false;
                foreach (bool Exists in IsExists(value))
                {
                    if (Exists)
                    {
                        status = true;
                        break;
                    }
                }
                return status;
            }
            private bool DoesExists(Double value)
            {
                bool status = false;
                foreach (bool Exists in IsExists(value))
                {
                    if (Exists)
                    {
                        status = true;
                        break;
                    }
                }
                return status;
            }
            public bool IsExists(String[] values)
            {
                bool status = true;
                for (int i = 0; i < values.Length; i++)
                {
                    if (DoesExists(values[i]) == false)
                    {
                        status = false;
                        break;
                    }
                }
                return status;
            }
            public bool IsExists(Double[] values)
            {
                bool status = true;
                for (int i = 0; i < values.Length; i++)
                {
                    if (DoesExists(values[i]) == false)
                    {
                        status = false;
                        break;
                    }
                }
                return status;
            }
            #endregion

            #region DISPOSE
            private bool isDisposed;
            ~oSI2()
            {
                Dispose(false);
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool isDisposing)
            {
                if (!isDisposed)
                {
                    if (isDisposing)
                    {
                        obj_SI2.Clear();
                    }
                    isDisposed = false;
                }
            }
            #endregion
        }

        private oSI2 baseSI2;

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
        [TestInitialize]
        public void MyTestInitialize()
        {
            baseSI2 = new oSI2();
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            baseSI2.Dispose();
        }
        //
        #endregion

        ///// <summary>
        ///// A test for ToScaledIECString
        ///// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        ///// and IEC named representation (e.g., KiB, MiB, GiB, TiB, etc.).
        ///// </summary>
        ///// <param name="totalUnits">Total units to represent textually.</param>
        ///// <param name="unitName">Name of unit display (e.g., you could use "B" for bytes).</param>
        ///// <remarks>
        ///// <see cref="IECSymbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        ///// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        ///// </remarks>
        ///// <returns>A <see cref="String"/> representation of the number of units.</returns>
        //[TestMethod]
        //public void ToScaledIECStringDecPlacesUnitNameTest()
        //{
        //    long totalUnits = long.Parse(Math.Pow(2, 10).ToString());
        //    int decimalPlaces = 3;
        //    string unitName = " kibi";
        //    string expected = "1.000 Ki kibi";
        //    string actual;
        //    actual = SI2.ToScaledString(totalUnits, decimalPlaces, unitName.ToLowerInvariant(), SI2.IECSymbols);
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        ///// A test for ToScaledIECString
        ///// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        ///// and IEC named representation (e.g., KiB, MiB, GiB, TiB, etc.).
        ///// </summary>
        ///// <param name="totalUnits">Total units to represent textually.</param>
        ///// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        ///// <param name="unitName">Name of unit display (e.g., you could use "B" for bytes).</param>
        ///// <remarks>
        ///// <see cref="IECSymbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        ///// </remarks>
        ///// <returns>A <see cref="String"/> representation of the number of units.</returns>
        //[TestMethod]
        //public void ToScaledIECStringFormatUnitNameTest()
        //{
        //    long totalUnits = long.Parse(Math.Pow(2, 10).ToString());
        //    string format = string.Empty;
        //    string unitName = " kibi";
        //    string expected = "1 Ki kibi";
        //    string actual;
        //    actual = SI2.ToScaledIECString(totalUnits, format, unitName);
        //    Assert.AreEqual(expected, actual);
        //    //Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        ///// A test for ToScaledIECString
        ///// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        ///// and common named representation (e.g., KB, MB, GB, TB, etc.).
        ///// </summary>
        ///// <param name="totalUnits">Total units to represent textually.</param>
        ///// <param name="unitName">Name of unit display (e.g., you could use "B" for bytes).</param>
        ///// <remarks>
        ///// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        ///// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        ///// </remarks>
        ///// <returns>A <see cref="String"/>representation of the number of units.</returns>
        //[TestMethod]
        //public void ToScaledIECStringUnitNameTest()
        //{
        //    long totalUnits = long.Parse(Math.Pow(2, 10).ToString());
        //    string unitName = " kilo";
        //    string expected = "1.000 Ki kilo";
        //    string actual;
        //    actual = SI2.ToScaledIECString(totalUnits, unitName);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A test for ToScaledString
        ///</summary>
        [TestMethod]
        public void ToScaledStringTest()
        {
            long totalUnits = long.Parse(Math.Pow(2, 10).ToString());
            string format = string.Empty;
            string unitName = "";
            string[] symbolNames = new[] { "kilo", "mega", "giga", "tera", "peta", "exa" };
            string expected = "1 kilo";
            string actual;
            actual = SI2.ToScaledString(totalUnits, format, unitName, symbolNames);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        /// A test for ToScaledString
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// and common named representation (e.g., KB, MB, GB, TB, etc.).
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "B" for bytes).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        /// <returns>A <see cref="String"/> representation of the number of units.</returns>
        [TestMethod]
        public void ToScaledStringFormatUnitNameTest()
        {
            long totalUnits = long.Parse(Math.Pow(2, 10).ToString());
            string format = string.Empty;
            string unitName = " kibi";
            string expected = "1 K kibi";
            string actual;
            actual = SI2.ToScaledString(totalUnits, format, unitName);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for ToScaledString
        ///// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        ///// and common named representation (e.g., KB, MB, GB, TB, etc.).
        ///// </summary>
        ///// <param name="totalUnits">Total units to represent textually.</param>
        ///// <param name="decimalPlaces">Number of decimal places to display.</param>
        ///// <param name="unitName">Name of unit display (e.g., you could use "B" for bytes).</param>
        ///// <remarks>
        ///// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        ///// </remarks>
        ///// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> cannot be negative.</exception>
        ///// <returns>A <see cref="String"/> representation of the number of units.</returns>
        //[TestMethod]
        //public void ToScaledStringDecPlacesUnitNameTest()
        //{
        //    long totalUnits = long.Parse(Math.Pow(2, 10).ToString());
        //    int decimalPlaces = 3;
        //    string unitName = " kibi";
        //    string expected = "1.000 K kibi";
        //    string actual;
        //    actual = SI2.ToScaledString(totalUnits, decimalPlaces, unitName);
        //    Assert.AreEqual(expected, actual);
        //    // Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        ///// A test for ToScaledString
        ///// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        ///// and IEC named representation (e.g., KiB, MiB, GiB, TiB, etc.).
        ///// </summary>
        ///// <param name="totalUnits">Total units to represent textually.</param>
        ///// <param name="unitName">Name of unit display (e.g., you could use "B" for bytes).</param>
        ///// <remarks>
        ///// <see cref="IECSymbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        ///// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        ///// </remarks>
        ///// <returns>A <see cref="String"/> representation of the number of units.</returns>
        //[TestMethod]
        //public void ToScaledStringUnitNameTest()
        //{
        //    long totalUnits = long.Parse(Math.Pow(2, 10).ToString());
        //    string unitName = " kibi";
        //    string expected = "1.000 K kibi";
        //    string actual;
        //    actual = SI2.ToScaledString(totalUnits, unitName);
        //    Assert.AreEqual(expected, actual);
        //    //Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        /// <summary>
        /// A test for Factors
        /// Gets an array of all the defined binary SI unit factors ordered from least (<see cref="Kilo"/>) to greatest (<see cref="Exa"/>).
        /// </summary>
        [TestMethod]
        public void FactorsTest()
        {

            long[] actual;
            actual = SI2.Factors;
            // Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        /// A test for IECNames
        /// Gets an array of all the defined IEC binary unit factor SI names ordered from least (<see cref="Kibi"/>) to greatest (<see cref="Exbi"/>).
        /// </summary>
        [TestMethod]
        public void IECNamesTest()
        {
            bool expected = true;
            bool actual = baseSI2.IsExists(SI2.IECNames);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for IECSymbols
        /// Gets an array of all the defined IEC binary unit factor SI prefix symbols ordered from least (<see cref="Kibi"/>) to greatest (<see cref="Exbi"/>).
        /// </summary>
        [TestMethod]
        public void IECSymbolsTest()
        {
            bool expected = true;
            bool actual = baseSI2.IsExists(SI2.IECSymbols);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Names
        /// Gets an array of all the defined common binary unit factor SI names ordered from least (<see cref="Kilo"/>) to greatest (<see cref="Exa"/>).
        /// </summary>
        [TestMethod]
        public void NamesTest()
        {

            bool expected = true;
            bool actual = baseSI2.IsExists(SI2.Names);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Symbols
        /// Gets an array of all the defined common binary unit factor SI prefix symbols ordered from least (<see cref="Kilo"/>) to greatest (<see cref="Exa"/>).
        /// </summary>
        [TestMethod]
        public void SymbolsTest()
        {
            bool expected = true;
            bool actual = baseSI2.IsExists(SI2.Symbols);
            Assert.AreEqual(expected, actual);
        }
    }
}
