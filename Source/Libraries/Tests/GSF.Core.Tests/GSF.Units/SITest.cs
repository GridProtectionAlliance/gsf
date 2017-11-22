//******************************************************************************************************
//  SITest.cs - Gbtc
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

    /* test table used
    Prefix symbol x from 10x
    yotta Y 24 1,000,000,000,000,000,000,000,000
    zetta Z 21 1,000,000,000,000,000,000,000
    exa E 18 1,000,000,000,000,000,000
    peta P 15 1,000,000,000,000,000
    tera T 12 1,000,000,000,000
    giga G 9 1,000,000,000
    mega M 6 1,000,000
    kilo k 3 1,000
    hecto h 2 100
    deca da 1 10
    base 0 1
    deci d -1 0.1
    centi c -2 0.01
    milli m -3 0.001
    micro μ -6 0.000001
    nano n -9 0.000000001
    pico p -12 0.000000000001
    femto f -15 0.000000000000001
    atto a -18 0.000000000000000001
    zepto z -21 0.000000000000000000001
    yocto y -24 0.000000000000000000000001
    */

    /// <summary>
    ///This is a test class for SITest and is intended
    ///to contain all SITest Unit Tests
    ///</summary>
    [TestClass]
    public class SITest
    {
        // virtual structure for SI unit
        struct sSI
        {
            #region DECLARATION
            String prefix { get; set; }
            String symbol { get; set; }
            int factor { get; set; }
            double value { get; set; }
            #endregion

            #region CONSTRUCTOR
            public void Set(String prefix, String symbol, int factor, double value)
            {
                this.prefix = prefix;
                this.symbol = symbol;
                this.factor = factor;
                this.value = value;
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
                if (value.ToLower() == prefix.ToLower() ||
                value.ToLower() == symbol.ToLower() ||
                value.ToLower() == factor.ToString().ToLower() ||
                value.ToLower() == value.ToLower())
                {
                    status = true;
                }
                return status;
            }
            #endregion
        }

        //virtual class to hold SI units table
        class oSI
        {
            #region DECLARATION

            readonly sSI virt_SI = new sSI();
            readonly List<sSI> obj_SI = new List<sSI>(22);
            #endregion

            #region CONSTRUCTOR
            public oSI()
            {
                virt_SI.Set("exa", "E", 18, 1000000000000000000); obj_SI.Add(virt_SI);
                virt_SI.Set("peta", "P", 15, 1000000000000000); obj_SI.Add(virt_SI);
                virt_SI.Set("tera", "T", 12, 1000000000000); obj_SI.Add(virt_SI);
                virt_SI.Set("giga", "G", 9, 1000000000); obj_SI.Add(virt_SI);
                virt_SI.Set("mega", "M", 6, 1000000); obj_SI.Add(virt_SI);
                virt_SI.Set("kilo", "k", 3, 1000); obj_SI.Add(virt_SI);
                virt_SI.Set("hecto", "h", 2, 100); obj_SI.Add(virt_SI);
                virt_SI.Set("deca", "da", 1, 10); obj_SI.Add(virt_SI);
                virt_SI.Set("base", "", 0, 1); obj_SI.Add(virt_SI);
                virt_SI.Set("deci", "d", -1, 0.1); obj_SI.Add(virt_SI);
                virt_SI.Set("centi", "c", -2, 0.01); obj_SI.Add(virt_SI);
                virt_SI.Set("milli", "m", -3, 0.001); obj_SI.Add(virt_SI);
                virt_SI.Set("micro", "μ", -6, 0.000001); obj_SI.Add(virt_SI);
                virt_SI.Set("nano", "n", -9, 0.000000001); obj_SI.Add(virt_SI);
                virt_SI.Set("pico", "p", -12, 0.000000000001); obj_SI.Add(virt_SI);
                virt_SI.Set("femto", "f", -15, 0.000000000000001); obj_SI.Add(virt_SI);
                virt_SI.Set("atto", "a", -18, 0.000000000000000001); obj_SI.Add(virt_SI);
            }
            #endregion CONSTRUCTOR

            #region METHODS
            private IEnumerable<bool> IsExists(Double value)
            {
                foreach (sSI obj in obj_SI)
                {
                    yield return obj.Find(value);
                }
            }
            private IEnumerable<bool> IsExists(String value)
            {
                foreach (sSI obj in obj_SI)
                {
                    yield return obj.Find(value);
                }
            }
            private bool DoesExists(String value)
            {
                bool status = false;
                foreach (bool Exists in IsExists(value))
                {
                    if (Exists) { status = true; break; }
                }
                return status;
            }
            private bool DoesExists(Double value)
            {
                bool status = false;
                foreach (bool Exists in IsExists(value))
                {
                    if (Exists) { status = true; break; }
                }
                return status;
            }
            public bool IsExists(String[] values)
            {
                bool status = true;
                for (int i = 0; i < values.Length; i++)
                {
                    if (DoesExists(values[i]) == false) { status = false; break; }
                }
                return status;
            }
            public bool IsExists(Double[] values)
            {
                bool status = true;
                for (int i = 0; i < values.Length; i++)
                {
                    if (DoesExists(values[i]) == false) { status = false; break; }
                }
                return status;
            }
            #endregion

            #region DISPOSE
            private bool isDisposed;
            ~oSI()
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
                        obj_SI.Clear();
                    }
                    isDisposed = false;
                }
            }
            #endregion
        }

        private oSI baseSI;

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
            baseSI = new oSI();
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            baseSI.Dispose();
        }
        //
        #endregion

        ///// <summary>
        ///// A test for ToScaledString
        ///// Turns the given number of units into a textual representation with an appropriate unit scaling.
        ///// </summary>
        ///// <param name="totalUnits">Total units to represent textually.</param>
        ///// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        ///// <remarks>
        ///// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        ///// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        ///// </remarks>
        ///// <returns>A <see cref="String"/> representation of the number of units.</returns>
        //[TestMethod]
        //public void ToScaledStringTest()
        //{
        //    double totalUnits = 10F;
        //    int decimalPlaces = 3;
        //    string unitName = " nano";
        //    string expected = "1.000 da nano";
        //    string actual = string.Empty;
        //    actual = SI.ToScaledString(totalUnits, decimalPlaces, unitName);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// A test for ToScaledString
        /// Turns the given number of units into a textual representation with an appropriate unit scaling
        /// given string array of factor names or symbols.
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <param name="symbolNames">SI factor symbol or name array to use during textual conversion.</param>
        /// <remarks>
        /// The <paramref name="symbolNames"/> array needs one string entry for each defined SI item ordered from
        /// least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>), see <see cref="Names"/> or <see cref="Symbols"/>
        /// arrays for examples.
        /// </remarks>
        /// <returns>A <see cref="String"/> representation of the number of units.</returns>
        [TestMethod]
        public void ToScaledStringFormatUnitSymbolTest()
        {
            double totalUnits = 1F;
            string format = string.Empty;
            string unitName = " deco";
            string[] symbolNames = SI.Symbols;
            string expected = "10 d deco";
            string actual;
            actual = SI.ToScaledString(totalUnits, format, unitName, symbolNames);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        ///// A test for ToScaledString
        ///// Turns the given number of units into a textual representation with an appropriate unit scaling.
        ///// </summary>
        ///// <param name="totalUnits">Total units to represent textually.</param>
        ///// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        ///// <remarks>
        ///// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        ///// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        ///// </remarks>
        ///// <returns>A <see cref="String"/> representation of the number of units.</returns>
        //[TestMethod]
        //public void ToScaledStringUnitTest()
        //{
        //    double totalUnits = 1F;
        //    string unitName = " deco";
        //    string expected = "10.000 d deco";
        //    string actual;
        //    actual = SI.ToScaledString(totalUnits, unitName);
        //    Assert.AreEqual(expected, actual);

        //}

        /// <summary>
        /// A test for ToScaledString
        /// Turns the given number of units into a textual representation with an appropriate unit scaling.
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        /// <returns>A <see cref="String"/> representation of the number of units.</returns>
        [TestMethod]
        public void ToScaledStringFormatUnitNameTest()
        {

            double totalUnits = 1F;
            string format = string.Empty;
            string unitName = " deco";
            string expected = "10 d deco";
            string actual;
            actual = SI.ToScaledString(totalUnits, format, unitName);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        /// A test for Factors
        /// Gets an array of all the defined SI unit factors ordered from least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>).
        /// </summary>
        [TestMethod]
        public void FactorsTest()
        {
            bool expected = true;
            bool actual = baseSI.IsExists(SI.Factors);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Names
        /// Gets an array of all the defined unit factor SI names ordered from least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>).
        /// </summary>
        [TestMethod]
        public void NamesTest()
        {
            bool expected = true;
            bool actual = baseSI.IsExists(SI.Names);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Symbols
        /// Gets an array of all the defined unit factor SI prefix symbols ordered from least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>).
        /// </summary>
        [TestMethod]
        public void SymbolsTest()
        {
            bool expected = true;
            bool actual = baseSI.IsExists(SI.Symbols);
            Assert.AreEqual(expected, actual);
        }
    }
}
