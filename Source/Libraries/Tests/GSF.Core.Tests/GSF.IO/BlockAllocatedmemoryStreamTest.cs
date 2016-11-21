//******************************************************************************************************
//  BlockAllocatedMemoryStreamTest.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/21/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Random = GSF.Security.Cryptography.Random;

namespace GSF.Core.Tests
{
    [TestClass]
    public class BlockAllocatedMemoryStreamTest
    {
        [TestMethod]
        public void Test()
        {
            MemoryStream ms = new MemoryStream();
            BlockAllocatedMemoryStream ms2 = new BlockAllocatedMemoryStream();

            for (int x = 0; x < 10000; x++)
            {
                int value = Random.Int32;
                ms.Write(value);
                ms.Write((byte)value);
                ms2.Write(value);
                ms2.Write((byte)value);
            }

            Compare(ms, ms2);
        }

        [TestMethod]
        public void Test2()
        {
            MemoryStream ms = new MemoryStream();
            BlockAllocatedMemoryStream ms2 = new BlockAllocatedMemoryStream();

            for (int x = 0; x < 10000; x++)
            {
                int value = Random.Int32;
                ms.Write(value);
                ms2.Write(value);

                int seek = Random.Int16Between(-10, 20);
                if (ms.Position + seek < 0)
                {
                    seek = -seek;
                }
                ms.Position += seek;
                ms2.Position += seek;
            }

            Compare(ms, ms2);
        }

        [TestMethod]
        public void Test3()
        {
            MemoryStream ms = new MemoryStream();
            BlockAllocatedMemoryStream ms2 = new BlockAllocatedMemoryStream();

            for (int x = 0; x < 10000; x++)
            {
                long position = Random.Int64Between(0, 100000);
                ms.Position = position;
                ms2.Position = position;

                int value = Random.Int32;
                ms.Write(value);
                ms2.Write(value);

                long length = Random.Int64Between(100000 >> 1, 100000);
                ms.SetLength(length);
                ms2.SetLength(length);
            }

            Compare(ms, ms2);
        }

        [TestMethod]
        public void Test4()
        {
            MemoryStream ms = new MemoryStream();
            BlockAllocatedMemoryStream ms2 = new BlockAllocatedMemoryStream();

            for (int x = 0; x < 10000; x++)
            {
                int value = Random.Int32;
                ms.Write(value);
                ms.Write((byte)value);
                ms2.Write(value);
                ms2.Write((byte)value);
            }

            for (int x = 0; x < 10000; x++)
            {
                long position = Random.Int64Between(0, ms.Length - 5);
                ms.Position = position;
                ms2.Position = position;

                if (ms.ReadInt32() != ms2.ReadInt32())
                    throw new Exception();
                if (ms.ReadNextByte() != ms2.ReadNextByte())
                    throw new Exception();
            }

            for (int x = 0; x < 10000; x++)
            {
                byte[] buffer1 = new byte[100];
                byte[] buffer2 = new byte[100];

                long position = Random.Int64Between(0, (long)(ms.Length * 1.1));
                int readLength = Random.Int32Between(0, 100);
                ms.Position = position;
                ms2.Position = position;

                if (ms.Read(buffer1, 99 - readLength, readLength) != ms2.Read(buffer2, 99 - readLength, readLength))
                {
                    CompareBytes(buffer1, buffer2);
                }

            }

            Compare(ms, ms2);
        }

        private static void Compare(MemoryStream ms, BlockAllocatedMemoryStream ms2)
        {
            if (ms.Position != ms2.Position)
                throw new Exception();
            if (ms.Length != ms2.Length)
                throw new Exception();

            byte[] data1 = ms.ToArray();
            byte[] data2 = ms2.ToArray();

            CompareBytes(data1, data2);
        }

        private static void CompareBytes(byte[] byte1, byte[] byte2)
        {
            if (byte1.Length != byte2.Length)
                throw new Exception();
            for (int x = 0; x < byte1.Length; x++)
            {
                if (byte1[x] != byte2[x])
                    throw new Exception();
            }
        }
    }
}
