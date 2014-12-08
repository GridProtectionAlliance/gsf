//******************************************************************************************************
//  FileBackedHashSetTest.cs - Gbtc
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

using System.Linq;
using GSF.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.Collections
{
    [TestClass()]
    public class FileBackedHashSetTest
    {
        [TestMethod()]
        public void AddTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                hashSet.Add(0);
                Assert.IsTrue(hashSet.Contains(0));
                Assert.AreEqual(hashSet.Count, 1);
            }
        }

        [TestMethod()]
        public void RemoveTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                hashSet.Add(0);
                Assert.IsTrue(hashSet.Contains(0));
                hashSet.Remove(0);
                Assert.IsFalse(hashSet.Contains(0));
                Assert.AreEqual(hashSet.Count, 0);
            }
        }

        [TestMethod()]
        public void UnionWithTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 10; i++)
                    hashSet.Add(i);

                hashSet.UnionWith(Enumerable.Range(5, 10));

                for (int i = 0; i < 15; i++)
                    Assert.IsTrue(hashSet.Contains(i), i.ToString());

                Assert.AreEqual(hashSet.Count, 15);
            }
        }

        [TestMethod()]
        public void IntersectWithTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 10; i++)
                    hashSet.Add(i);

                hashSet.IntersectWith(Enumerable.Range(5, 10));

                for (int i = 5; i < 10; i++)
                    Assert.IsTrue(hashSet.Contains(i), i.ToString());

                Assert.AreEqual(hashSet.Count, 5);
            }
        }

        [TestMethod()]
        public void ExceptWithTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 10; i++)
                    hashSet.Add(i);

                hashSet.ExceptWith(Enumerable.Range(5, 10));

                for (int i = 0; i < 5; i++)
                    Assert.IsTrue(hashSet.Contains(i), i.ToString());

                Assert.AreEqual(hashSet.Count, 5);
            }
        }

        [TestMethod()]
        public void SymmetricExceptWithTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 10; i++)
                    hashSet.Add(i);

                hashSet.SymmetricExceptWith(Enumerable.Range(5, 10));

                for (int i = 0; i < 5; i++)
                    Assert.IsTrue(hashSet.Contains(i), i.ToString());

                for (int i = 10; i < 15; i++)
                    Assert.IsTrue(hashSet.Contains(i), i.ToString());

                Assert.AreEqual(hashSet.Count, 10);
            }
        }

        [TestMethod()]
        public void IsSubsetOfTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 5; i++)
                    hashSet.Add(i);

                Assert.IsTrue(hashSet.IsSubsetOf(Enumerable.Range(0, 5)), "Equal");
                Assert.IsTrue(hashSet.IsSubsetOf(Enumerable.Range(0, 10)), "Superset");
                Assert.IsFalse(hashSet.IsSubsetOf(Enumerable.Range(0, 3)), "Subset");
                Assert.IsFalse(hashSet.IsSubsetOf(Enumerable.Range(1, 5)), "Overlap");
                Assert.IsFalse(hashSet.IsSubsetOf(Enumerable.Range(5, 5)), "Disjoint");
            }
        }

        [TestMethod()]
        public void IsSupersetOfTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 5; i++)
                    hashSet.Add(i);

                Assert.IsTrue(hashSet.IsSupersetOf(Enumerable.Range(0, 5)), "Equal");
                Assert.IsFalse(hashSet.IsSupersetOf(Enumerable.Range(0, 10)), "Superset");
                Assert.IsTrue(hashSet.IsSupersetOf(Enumerable.Range(0, 3)), "Subset");
                Assert.IsFalse(hashSet.IsSupersetOf(Enumerable.Range(1, 5)), "Overlap");
                Assert.IsFalse(hashSet.IsSupersetOf(Enumerable.Range(5, 5)), "Disjoint");
            }
        }

        [TestMethod()]
        public void IsProperSupersetOfTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 5; i++)
                    hashSet.Add(i);

                Assert.IsFalse(hashSet.IsProperSupersetOf(Enumerable.Range(0, 5)), "Equal");
                Assert.IsFalse(hashSet.IsProperSupersetOf(Enumerable.Range(0, 10)), "Superset");
                Assert.IsTrue(hashSet.IsProperSupersetOf(Enumerable.Range(0, 3)), "Subset");
                Assert.IsFalse(hashSet.IsProperSupersetOf(Enumerable.Range(1, 5)), "Overlap");
                Assert.IsFalse(hashSet.IsProperSupersetOf(Enumerable.Range(5, 5)), "Disjoint");
            }
        }

        [TestMethod()]
        public void IsProperSubsetOfTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 5; i++)
                    hashSet.Add(i);

                Assert.IsFalse(hashSet.IsProperSubsetOf(Enumerable.Range(0, 5)), "Equal");
                Assert.IsTrue(hashSet.IsProperSubsetOf(Enumerable.Range(0, 10)), "Superset");
                Assert.IsFalse(hashSet.IsProperSubsetOf(Enumerable.Range(0, 3)), "Subset");
                Assert.IsFalse(hashSet.IsProperSubsetOf(Enumerable.Range(1, 5)), "Overlap");
                Assert.IsFalse(hashSet.IsProperSubsetOf(Enumerable.Range(5, 5)), "Disjoint");
            }
        }

        [TestMethod()]
        public void OverlapsTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 5; i++)
                    hashSet.Add(i);

                Assert.IsTrue(hashSet.Overlaps(Enumerable.Range(0, 5)), "Equal");
                Assert.IsTrue(hashSet.Overlaps(Enumerable.Range(0, 10)), "Superset");
                Assert.IsTrue(hashSet.Overlaps(Enumerable.Range(0, 3)), "Subset");
                Assert.IsTrue(hashSet.Overlaps(Enumerable.Range(1, 5)), "Overlap");
                Assert.IsFalse(hashSet.Overlaps(Enumerable.Range(5, 5)), "Disjoint");
            }
        }

        [TestMethod()]
        public void SetEqualsTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 5; i++)
                    hashSet.Add(i);

                Assert.IsTrue(hashSet.SetEquals(Enumerable.Range(0, 5)), "Equal");
                Assert.IsFalse(hashSet.SetEquals(Enumerable.Range(0, 10)), "Superset");
                Assert.IsFalse(hashSet.SetEquals(Enumerable.Range(0, 3)), "Subset");
                Assert.IsFalse(hashSet.SetEquals(Enumerable.Range(1, 5)), "Overlap");
                Assert.IsFalse(hashSet.SetEquals(Enumerable.Range(5, 5)), "Disjoint");
            }
        }

        [TestMethod()]
        public void ClearTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 100; i++)
                    hashSet.Add(i);

                Assert.AreEqual(hashSet.Count, 100);
                hashSet.Clear();
                Assert.AreEqual(hashSet.Count, 0);
            }
        }

        [TestMethod()]
        public void CopyToTest()
        {
            int[] array;

            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 1; i <= 100; i++)
                    hashSet.Add(i);

                Assert.AreEqual(hashSet.Count, 100);

                array = new int[hashSet.Count];
                hashSet.CopyTo(array, 0);

                foreach (int i in array)
                    Assert.IsTrue(hashSet.Contains(i));
            }
        }

        [TestMethod()]
        public void CompactTest()
        {
            using (FileBackedHashSet<int> hashSet = new FileBackedHashSet<int>())
            {
                for (int i = 0; i < 10000; i += 4)
                {
                    hashSet.Add(i);

                    if (i % 100 == 0)
                        hashSet.Remove(i);

                    if (i % 400 == 0)
                        hashSet.Add(i);
                }

                hashSet.Compact();

                for (int i = 0; i < 10000; i++)
                {
                    if (i % 400 == 0)
                        Assert.IsTrue(hashSet.Contains(i));
                    else if (i % 100 == 0)
                        Assert.IsFalse(hashSet.Contains(i));
                    else if (i % 4 == 0)
                        Assert.IsTrue(hashSet.Contains(i));
                    else
                        Assert.IsFalse(hashSet.Contains(i));
                }
            }
        }
    }
}
