//******************************************************************************************************
//  ListCollectionTests.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  05/09/2014 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GSF.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.Collections
{
    [TestClass]
    public class ListCollectionTests
    {
        [TestMethod]
        public void TestAdd()
        {
            var rand = new Random(3);

            ListCollection<string> list1 = new ListCollection<string>();
            Collection<string> list2 = new Collection<string>();

            //Check Add
            for (int x = 0; x < 100; x++)
            {
                var str = x.ToString();
                list1.Add(str);
                list2.Add(str);

                CheckEquals1(list1, list2);
            }

            //Check Remove
            for (int x = 100; x < 200; x++)
            {
                var str = x.ToString();
                var removeItem = list1[rand.Next(list1.Count)];
                list1.Remove(removeItem);
                list2.Remove(removeItem);

                CheckEquals1(list1, list2);

                list1.Add(str);
                list2.Add(str);

                CheckEquals1(list1, list2);
            }

            //Check RemoveAt
            for (int x = 0; x < 100; x++)
            {
                int index = rand.Next(list1.Count);
                list1.RemoveAt(index);
                list2.RemoveAt(index);

                CheckEquals1(list1, list2);
            }

            //Check Insert
            for (int x = 0; x < 100; x++)
            {
                int index = rand.Next(list1.Count);
                list1.Insert(index, x.ToString());
                list2.Insert(index, x.ToString());

                CheckEquals1(list1, list2);
            }

            //Check set
            for (int x = 0; x < 100; x++)
            {
                int index = rand.Next(list1.Count);
                list1[index] = x.ToString();
                list2[index] = x.ToString();

                CheckEquals1(list1, list2);
            }

            list1.Clear();
            list2.Clear();
            CheckEquals1(list1, list2);

            //Check Add
            for (int x = 0; x < 100; x++)
            {
                var str = x.ToString();
                list1.Add(str);
                list2.Add(str);

                CheckEquals1(list1, list2);
            }

            //Check indexOf
            for (int x = 0; x < 100; x++)
            {
                int index = rand.Next(list1.Count * 2);

                if (list1.IndexOf(index.ToString()) != list2.IndexOf(index.ToString()))
                    throw new Exception();

                CheckEquals1(list1, list2);
            }

            string[] lst1 = new string[list1.Count];
            string[] lst2 = new string[list2.Count];

            list1.CopyTo(lst1, 0);
            list2.CopyTo(lst2, 0);

            CheckEquals3(list1, list2);

            for (int x = 0; x < 100; x++)
            {
                int index = rand.Next(list1.Count * 2);

                if (list1.Contains(index.ToString()) != list2.Contains(index.ToString()))
                    throw new Exception();

                CheckEquals1(list1, list2);
            }


        }



        void CheckEquals1(ListCollection<string> list1, Collection<string> list2)
        {
            if (list1.Count != list2.Count)
                throw new Exception();
            for (int x = 0; x < list1.Count; x++)
            {
                if (list1[x] != list2[x])
                    throw new Exception();
            }

            var enum1 = list1.GetEnumerator();
            var enum2 = list2.GetEnumerator();

            while (enum1.MoveNext())
            {
                if (!enum2.MoveNext())
                    throw new Exception();

                if (enum1.Current != enum2.Current)
                    throw new Exception();
            }
            if (enum2.MoveNext())
                throw new Exception();

            if (!list1.SequenceEqual(list2))
                throw new Exception();

            CheckEquals2(list1, list2);
        }

        void CheckEquals2(Collection<string> list1, Collection<string> list2)
        {
            if (list1.Count != list2.Count)
                throw new Exception();
            for (int x = 0; x < list1.Count; x++)
            {
                if (list1[x] != list2[x])
                    throw new Exception();
            }

            var enum1 = list1.GetEnumerator();
            var enum2 = list2.GetEnumerator();

            while (enum1.MoveNext())
            {
                if (!enum2.MoveNext())
                    throw new Exception();

                if (enum1.Current != enum2.Current)
                    throw new Exception();
            }
            if (enum2.MoveNext())
                throw new Exception();

            CheckEquals3(list1, list2);

        }

        void CheckEquals3(IList<string> list1, IList<string> list2)
        {
            if (list1.Count != list2.Count)
                throw new Exception();
            for (int x = 0; x < list1.Count; x++)
            {
                if (list1[x] != list2[x])
                    throw new Exception();
            }

            var enum1 = list1.GetEnumerator();
            var enum2 = list2.GetEnumerator();

            while (enum1.MoveNext())
            {
                if (!enum2.MoveNext())
                    throw new Exception();

                if (enum1.Current != enum2.Current)
                    throw new Exception();
            }
            if (enum2.MoveNext())
                throw new Exception();
        }

    }
}

