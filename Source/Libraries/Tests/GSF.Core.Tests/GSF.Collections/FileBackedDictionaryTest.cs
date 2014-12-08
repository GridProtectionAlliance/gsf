//******************************************************************************************************
//  FileBackedDictionaryTest.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  12/03/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using GSF.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.Collections
{
    [TestClass()]
    public class FileBackedDictionaryTest
    {
        [TestMethod()]
        public void AddTest()
        {
            using (FileBackedDictionary<int, int> dictionary = new FileBackedDictionary<int, int>())
            {
                dictionary.Add(0, 0);
                Assert.IsTrue(dictionary.ContainsKey(0));
                Assert.AreEqual(dictionary.Count, 1);
            }
        }

        [TestMethod()]
        public void RemoveTest()
        {
            using (FileBackedDictionary<int, int> dictionary = new FileBackedDictionary<int, int>())
            {
                dictionary.Add(0, 0);
                Assert.IsTrue(dictionary.ContainsKey(0));
                dictionary.Remove(0);
                Assert.IsFalse(dictionary.ContainsKey(0));
                Assert.AreEqual(dictionary.Count, 0);
            }
        }

        [TestMethod()]
        public void TryGetValueTest()
        {
            int value;

            using (FileBackedDictionary<int, int> dictionary = new FileBackedDictionary<int, int>())
            {
                dictionary.Add(0, 0);
                Assert.IsTrue(dictionary.TryGetValue(0, out value));
                Assert.AreEqual(value, 0);
            }
        }

        [TestMethod()]
        public void ClearTest()
        {
            using (FileBackedDictionary<int, int> dictionary = new FileBackedDictionary<int, int>())
            {
                for (int i = 0; i < 100; i++)
                    dictionary.Add(i, i);

                Assert.AreEqual(dictionary.Count, 100);
                dictionary.Clear();
                Assert.AreEqual(dictionary.Count, 0);
            }
        }

        [TestMethod()]
        public void CopyToTest()
        {
            KeyValuePair<int, int>[] array;

            using (FileBackedDictionary<int, int> dictionary = new FileBackedDictionary<int, int>())
            {
                for (int i = 1; i <= 100; i++)
                    dictionary.Add(i, i);

                Assert.AreEqual(dictionary.Count, 100);

                array = new KeyValuePair<int, int>[dictionary.Count];
                dictionary.CopyTo(array, 0);

                foreach (KeyValuePair<int, int> kvp in array)
                {
                    Assert.IsTrue(dictionary.Contains(kvp), kvp.Key.ToString());
                    Assert.AreEqual(dictionary[kvp.Key], kvp.Value);
                }
            }
        }

        [TestMethod()]
        public void CompactTest()
        {
            using (FileBackedDictionary<int, int> dictionary = new FileBackedDictionary<int, int>())
            {
                for (int i = 0; i < 10000; i += 4)
                {
                    dictionary.Add(i, 4);

                    if (i % 400 == 0)
                        dictionary[i] = 400;
                    else if (i % 100 == 0)
                        dictionary.Remove(i);
                }

                dictionary.Compact();

                for (int i = 0; i < 10000; i++)
                {
                    if (i % 400 == 0)
                        Assert.AreEqual(dictionary[i], 400);
                    else if (i % 100 == 0)
                        Assert.IsFalse(dictionary.ContainsKey(i), i.ToString());
                    else if (i % 4 ==  0)
                        Assert.AreEqual(dictionary[i], 4);
                    else
                        Assert.IsFalse(dictionary.ContainsKey(i), i.ToString());
                }
            }
        }
    }
}
