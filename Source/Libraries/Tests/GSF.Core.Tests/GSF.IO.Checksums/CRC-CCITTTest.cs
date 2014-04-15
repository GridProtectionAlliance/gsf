//******************************************************************************************************
//  CRC_CCITTTest.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  04/15/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Text;
using GSF.IO.Checksums;
using GSF.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.IO.Checksums
{
    /// <summary>
    /// This is a test class for CRC CCITT and is intended to contain all CRC CCITT Unit Tests
    /// </summary>
    [TestClass]
    public class CRC_CCITTTest
    {
        // Sample text used to test checksum algorithm.
        private const string SampleText = "lsdfjkh0p9ujveroilkjcp09;3iewrfj;lskdjf[p0349ifjwa;liu2903ru[qpa;kjcm;lskm";

        // UTF-8 encoding of the sample text.
        private readonly byte[] SampleData = Encoding.UTF8.GetBytes(SampleText);

        // Precomputed checksum calculated by a separate checksum calculator.
        private const ushort SampleDataChecksum = 0xE3FD;

        /// <summary>
        /// Tests the extension method which utilizes the most straightforward use of the checksum API.
        /// </summary>
        [TestMethod]
        public void ExtensionMethodTest()
        {
            Assert.AreEqual(SampleDataChecksum, SampleData.CrcCCITTChecksum(0, SampleData.Length));
        }

        /// <summary>
        /// Tests the Update method that accepts a single byte as a parameter.
        /// </summary>
        [TestMethod]
        public void UpdateByteTest()
        {
            CrcCCITT checksum = new CrcCCITT();

            foreach (byte d in SampleData)
                checksum.Update(d);

            Assert.AreEqual(SampleDataChecksum, checksum.Value);
        }

        /// <summary>
        /// Tests the use of calls to the two separate implementations of Update in one checksum calculation.
        /// </summary>
        [TestMethod]
        public void MixedUpdateTest()
        {
            CrcCCITT checksum = new CrcCCITT();
            int i = 0;

            checksum.Reset();

            while (i < SampleData.Length / 4)
                checksum.Update(SampleData[i++]);

            for (int j = 0; j < 2; j++)
            {
                checksum.Update(SampleData, i, SampleData.Length / 4);
                i += SampleData.Length / 4;
            }

            while (i < SampleData.Length)
                checksum.Update(SampleData[i++]);

            Assert.AreEqual(SampleDataChecksum, checksum.Value);
        }

        /// <summary>
        /// Tests table-based CRC CCITT computation to a simple non-table based version found in the IEEE C37.118 standard.
        /// </summary>
        [TestMethod]
        public void TableToNoTableCRC_CCITTComputationComparisonTest()
        {
            // Compare sample data
            Assert.AreEqual(NoTableComputeCRC_CCITT(SampleData, SampleData.Length), SampleData.CrcCCITTChecksum(0, SampleData.Length));

            byte[] largeBuffer = new byte[ushort.MaxValue / 2];

            Random.GetBytes(largeBuffer);

            // Compare 32K worth of data
            Assert.AreEqual(NoTableComputeCRC_CCITT(largeBuffer, largeBuffer.Length), largeBuffer.CrcCCITTChecksum(0, largeBuffer.Length));

            largeBuffer = new byte[ushort.MaxValue];

            Random.GetBytes(largeBuffer);

            // Compare 64K worth of data
            Assert.AreEqual(NoTableComputeCRC_CCITT(largeBuffer, largeBuffer.Length), largeBuffer.CrcCCITTChecksum(0, largeBuffer.Length));

            largeBuffer = new byte[ushort.MaxValue * 2];

            Random.GetBytes(largeBuffer);

            // Compare 128K worth of data
            Assert.AreEqual(NoTableComputeCRC_CCITT(largeBuffer, largeBuffer.Length), largeBuffer.CrcCCITTChecksum(0, largeBuffer.Length));
        }
        
        private ushort NoTableComputeCRC_CCITT(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            ushort temp;
            ushort quick;
            int i;
            
            for (i = 0; i < length; i++)
            {
                temp = (ushort)((crc >> 8) ^ data[i]);
                crc <<= 8;
                quick = (ushort)(temp ^ (temp >> 4));
                crc ^= quick;
                quick <<= 5;
                crc ^= quick;
                quick <<= 7;
                crc ^= quick;
            }

            return crc;
        }
    }
}
