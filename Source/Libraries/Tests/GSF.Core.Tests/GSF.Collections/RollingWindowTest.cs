//******************************************************************************************************
//  RollingWindowTest.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/03/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.Collections
{
    [TestClass()]
    public class RollingWindowTest
    {
        [TestMethod()]
        public void AddTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
            {
                Assert.AreEqual(i, rollingWindow.Count);
                rollingWindow.Add(i);
                Assert.AreEqual(i, rollingWindow[i]);
            }

            Assert.AreEqual(rollingWindow.WindowSize, rollingWindow.Count);
            rollingWindow.Add(rollingWindow.WindowSize);
            Assert.AreEqual(rollingWindow.WindowSize, rollingWindow.Count);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
                Assert.AreEqual(i + 1, rollingWindow[i]);
        }

        [TestMethod()]
        public void InsertTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);

            rollingWindow.Insert(0, 1);
            rollingWindow.Insert(1, 3);
            rollingWindow.Insert(0, 0);
            rollingWindow.Insert(2, 2);
            rollingWindow.Insert(4, 4);

            for (int i = 0; i < rollingWindow.Count; i++)
                Assert.AreEqual(i, rollingWindow[i]);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
                rollingWindow.Add(i);

            rollingWindow.Remove(1);
            rollingWindow.Remove(3);

            Assert.AreEqual(3, rollingWindow.Count);
            Assert.AreEqual(0, rollingWindow[0]);
            Assert.AreEqual(2, rollingWindow[1]);
            Assert.AreEqual(4, rollingWindow[2]);
        }

        [TestMethod()]
        public void RemoveAtTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
                rollingWindow.Add(i);

            rollingWindow.RemoveAt(3);
            rollingWindow.RemoveAt(1);

            Assert.AreEqual(3, rollingWindow.Count);
            Assert.AreEqual(0, rollingWindow[0]);
            Assert.AreEqual(2, rollingWindow[1]);
            Assert.AreEqual(4, rollingWindow[2]);
        }

        [TestMethod()]
        public void IndexOfTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
                rollingWindow.Add(i);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
                Assert.AreEqual(i, rollingWindow.IndexOf(i));
        }

        [TestMethod()]
        public void ContainsTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
                rollingWindow.Add(i);

            for (int i = -5; i < 10; i++)
                Assert.AreEqual((i >= 0) && (i < 5), rollingWindow.Contains(i));
        }

        [TestMethod()]
        public void ClearTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);

            for (int i = 0; i < rollingWindow.WindowSize; i++)
                rollingWindow.Add(i);

            Assert.AreEqual(rollingWindow.WindowSize, rollingWindow.Count);
            rollingWindow.Clear();
            Assert.AreEqual(0, rollingWindow.Count);
        }

        [TestMethod()]
        public void CopyToTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);
            int[] array = new int[rollingWindow.WindowSize];

            for (int i = 0; i < rollingWindow.WindowSize + 1; i++)
                rollingWindow.Add(i);

            rollingWindow.CopyTo(array, 0);

            for (int i = 0; i < array.Length; i++)
                Assert.AreEqual(i + 1, array[i]);
        }

        [TestMethod()]
        public void GetEnumeratorTest()
        {
            RollingWindow<int> rollingWindow = new RollingWindow<int>(5);
            int count = 0;

            for (int i = 0; i < rollingWindow.WindowSize + 1; i++)
                rollingWindow.Add(i);

            foreach (int i in rollingWindow)
                Assert.AreEqual(++count, i);
        }
    }
}
