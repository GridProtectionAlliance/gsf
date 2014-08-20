//******************************************************************************************************
//  GuidExtensionsTest.cs - Gbtc
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
//  05/07/2014 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************


using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable 618

namespace GSF.Core.Tests
{
    [TestClass]
    public class GuidExtensionsTest
    {
        [TestMethod]
        public void TestLittleEndianOrderBytes()
        {
            Guid guid = Guid.NewGuid();
            byte[] data = guid.ToByteArray();
            byte[] data2 = GuidExtensions.ToLittleEndianBytes(guid);
            byte[] data3 = EndianOrder.LittleEndian.GetBytes(guid);
            Guid guid2 = GuidExtensions.ToLittleEndianGuid(data2);
            Guid guid3 = EndianOrder.LittleEndian.ToGuid(data3, 0);

            foreach (var b in data)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            foreach (var b in data2)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            foreach (var b in data3)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            System.Console.WriteLine(guid);
            System.Console.WriteLine(guid2);
            System.Console.WriteLine(guid3);

            Assert.AreEqual(guid, guid2);
            Assert.AreEqual(guid, guid3);
            Assert.IsTrue(data.SequenceEqual(data2));
            Assert.IsTrue(data.SequenceEqual(data3));
        }

        [TestMethod]
        public void TestBigEndianOrderBytes()
        {
            Guid guid = Guid.NewGuid();
            byte[] data = guid.ToByteArray();
            Array.Reverse(data);

            byte[] data2 = GuidExtensions.__ToBigEndianOrderBytes(guid);
            byte[] data3 = EndianOrder.BigEndian.GetBytes(guid);
            Guid guid2 = GuidExtensions.__ToBigEndianOrderGuid(data2);
            Guid guid3 = EndianOrder.BigEndian.ToGuid(data3, 0);

            foreach (var b in data)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            foreach (var b in data2)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            foreach (var b in data3)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            System.Console.WriteLine(guid);
            System.Console.WriteLine(guid2);
            System.Console.WriteLine(guid3);

            Assert.AreEqual(guid, guid2);
            Assert.AreEqual(guid, guid3);
            Assert.IsTrue(data.SequenceEqual(data2));
            Assert.IsTrue(data2.SequenceEqual(data3));
        }

        [TestMethod]
        public void TestRfcOrderBytes()
        {
            Guid guid = Guid.NewGuid();
            byte[] data = guid.ToByteArray();
            byte[] data2 = GuidExtensions.ToRfcBytes(guid);
            Guid guid2 = GuidExtensions.ToRfcGuid(data2);

            foreach (var b in data)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            foreach (var b in data2)
                System.Console.Write("{0:X2} ", b);
            System.Console.WriteLine();

            StringBuilder sb = new StringBuilder();
            foreach (var b in data2)
                sb.AppendFormat("{0:X2}", b);

            System.Console.WriteLine(sb.ToString());
            System.Console.WriteLine(guid.ToString("N").ToUpper());
            System.Console.WriteLine(guid);
            System.Console.WriteLine(guid2);

            Assert.AreEqual(guid, guid2);
            Assert.AreEqual(guid.ToString("N").ToUpper(), sb.ToString());
        }

    }
}
