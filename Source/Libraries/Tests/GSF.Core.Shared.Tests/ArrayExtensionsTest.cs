//******************************************************************************************************
//  ArrayExtensionsTest.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  11/27/2023 - AJ Stadlin
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Shared.Tests
{
    [TestClass]
    public class ArrayExtensionsTest
    {
        public TestContext TestContext { get; set; }

        private const string TestString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789GHI";

        /// <summary>
        /// Count the instances of a sequence in the array
        /// When the sequence is not found
        /// </summary>
        [TestMethod]
        public void CountOfSequence0NotFound()
        {
            const string lookFor = "def";
            int actual = TestString.ToArray().CountOfSequence(lookFor.ToCharArray(), 0);
            int expected = 0;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Count the instances of a sequence in the array
        /// When the sequence is found at the start of the array
        /// </summary>
        [TestMethod]
        public void CountOfSequence1AtIndex0()
        {
            const string lookFor = "ABC";
            int actual = TestString.ToArray().CountOfSequence(lookFor.ToCharArray(), 0);
            int expected = 1;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Count the instances of a sequence in the array
        /// When the sequence is found in the middle of the array
        /// </summary>
        [TestMethod]
        public void CountOfSequence1Mid()
        {
            const string lookFor = "DEF";
            int actual = TestString.ToArray().CountOfSequence(lookFor.ToCharArray(), 0);
            int expected = 1;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Count the instances of a sequence in the array
        /// When search starts in the middle of the array
        /// And the sequence is found at the end of the array
        /// </summary>
        [TestMethod]
        public void CountOfSequence1Start20MidAndEnd()
        {
            const string lookFor = "GHI";
            int actual = TestString.ToArray().CountOfSequence(lookFor.ToCharArray(), 20);
            int expected = 1;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Count the instances of a sequence in the array
        /// When multiple instances of the sequence are found
        /// </summary>
        [TestMethod]
        public void CountOfSequence2MidAndEnd()
        {
            const string lookFor = "GHI";
            int actual = TestString.ToArray().CountOfSequence(lookFor.ToCharArray(), 0);
            int expected = 2;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Count the instances of a sequence in the array
        /// When search starts in the middle of the array
        /// And multiple instances of the sequence are found
        /// </summary>
        [TestMethod]
        public void CountOfSequence2Start5MidAndEnd()
        {
            const string lookFor = "GHI";
            int actual = TestString.ToArray().CountOfSequence(lookFor.ToCharArray(), 5);
            int expected = 2;
            Assert.AreEqual(expected, actual);
        }
    }
}
