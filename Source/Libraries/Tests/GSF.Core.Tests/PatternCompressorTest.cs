//******************************************************************************************************
//  PatternCompressorTest.cs - Gbtc
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
//  10/05/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using GSF.IO.Compression;
using GSF.Units;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS0618 // Type or member is obsolete

namespace GSF.Core.Tests
{
    /// <summary>
    /// Summary description for PatternCompressorTest
    /// </summary>
    [TestClass]
    public class PatternCompressorTest
    {
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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private const int TotalTestSampleSize = 500000; // int.MaxValue / 500;

        [TestMethod]
        public void TestCompressionCases()
        {
            uint[] case1 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] case2 = { 0xFF, 0xAD, 0xBC, 0x5D, 0x99, 0x84, 0xA8, 0x3D, 0x45, 0x02, 0 };
            uint[] case3 = { 0xFFFF, 0xABCD, 0x1234, 0x9876, 0x1A2B, 0x1928, 0x9182, 0x6666, 0x5294, 0xAFBD, 0 };
            uint[] case4 = { 0xFFFFFF, 0xABCDEF, 0xFEDCBA, 0xAFBECD, 0xFAEBDC, 0x123456, 0x654321, 0x162534, 0x615243, 0x987654, 0 };
            uint[] case5 = { 0xFFFFFFFF, 0xABCDEFAB, 0xFEDCBAFE, 0xAFBECDAF, 0xFAEBDCFA, 0x12345678, 0x87654321, 0x18273645, 0x81726354, 0x98765432, 0 };

            MemoryStream buffer;
            byte[] data;
            int compressedLen;

            // --- Test case1 ---
            buffer = new MemoryStream();

            foreach (uint i in case1)
                buffer.Write(BitConverter.GetBytes(i), 0, 4);

            data = buffer.ToArray();
            compressedLen = PatternCompressor.CompressBuffer(data, 0, data.Length - 4, data.Length);

            Assert.AreEqual(14, compressedLen);
            // ------------------

            // --- Test case2 ---
            buffer = new MemoryStream();

            foreach (uint i in case2)
                buffer.Write(BitConverter.GetBytes(i), 0, 4);

            data = buffer.ToArray();
            compressedLen = PatternCompressor.CompressBuffer(data, 0, data.Length - 4, data.Length);

            Assert.AreEqual(23, compressedLen);
            // ------------------

            // --- Test case3 ---
            buffer = new MemoryStream();

            foreach (uint i in case3)
                buffer.Write(BitConverter.GetBytes(i), 0, 4);

            data = buffer.ToArray();
            compressedLen = PatternCompressor.CompressBuffer(data, 0, data.Length - 4, data.Length);

            Assert.AreEqual(32, compressedLen);
            // ------------------

            // --- Test case4 ---
            buffer = new MemoryStream();

            foreach (uint i in case4)
                buffer.Write(BitConverter.GetBytes(i), 0, 4);

            data = buffer.ToArray();
            compressedLen = PatternCompressor.CompressBuffer(data, 0, data.Length - 4, data.Length);

            Assert.AreEqual(41, compressedLen);
            // ------------------

            // --- Test case5 ---
            buffer = new MemoryStream();

            foreach (uint i in case5)
                buffer.Write(BitConverter.GetBytes(i), 0, 4);

            data = buffer.ToArray();
            compressedLen = PatternCompressor.CompressBuffer(data, 0, data.Length - 4, data.Length);

            Assert.AreEqual(41, compressedLen);
            // ------------------
        }

        [TestMethod]
        public void TestStreamCompression()
        {
            const int compressionStrength = 31;

            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            PatternCompressor compressor = new PatternCompressor
            {
                CompressedBuffer = new byte[4 * TotalTestSampleSize],
                CompressionStrength = compressionStrength
            };

            byte[] arrayOfInts;
            int bufferLength;
            int dataLength;
            int compressedLen;

            //bool match;

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                uint value = (uint)(rnd.NextDouble() * 100000);
                buffer.Write(BitConverter.GetBytes(value), 0, 4);
                compressor.Compress(value);
            }

            // Add one byte of extra space to accommodate compression algorithm
            buffer.WriteByte(0xFF);

            arrayOfInts = buffer.ToArray();
            bufferLength = arrayOfInts.Length;
            dataLength = bufferLength - 1;
            compressedLen = PatternCompressor.CompressBuffer(arrayOfInts, 0, dataLength, bufferLength, compressionStrength);

            // Compressed arrays do not match. This is because the streaming compression
            // searches the back buffer queue starting from index 0, regardless of the
            // index of the start of the queue. The static method searches from the start
            // of the queue and wraps around in a circular fashion. This discrepancy does
            // not affect decompression.
            //
            //match = compressedLen == compressor.CompressedBufferLength;
            //for (int i = 0; match && i < compressedLen; i++)
            //{
            //    match = arrayOfInts[i] == compressor.CompressedBuffer[i];
            //}

            Assert.AreEqual(compressedLen, compressor.CompressedBufferLength);
            //Assert.IsTrue(match);
        }

        [TestMethod]
        public void TestStreamDecompression()
        {
            MemoryStream memStream = new MemoryStream();
            Random rnd = new Random();

            byte[] original;
            byte[] decompressed;

            bool match;

            PatternCompressor compressor = new PatternCompressor
            {
                CompressedBuffer = new byte[4 * TotalTestSampleSize],
                CompressionStrength = 31
            };

            PatternDecompressor decompressor = new PatternDecompressor();

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                uint value = (uint)(rnd.NextDouble() * 100000);
                memStream.Write(BitConverter.GetBytes(value), 0, 4);
                compressor.Compress(value);
            }

            original = memStream.ToArray();
            memStream = new MemoryStream();
            decompressor.AugmentBuffer(compressor.CompressedBuffer, compressor.CompressedBufferLength);

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                uint value;
                decompressor.Decompress(out value);
                memStream.Write(BitConverter.GetBytes(value), 0, 4);
            }

            decompressed = memStream.ToArray();
            match = original.Length == decompressed.Length;
            for (int i = 0; match && i < original.Length; i++)
            {
                match = original[i] == decompressed[i];
            }

            Assert.AreEqual(original.Length, decompressed.Length);
            Assert.IsTrue(match);
        }

        [TestMethod]
        public void TestDecompressionOfStreamCompression()
        {
            MemoryStream memStream = new MemoryStream();
            Random rnd = new Random();

            byte[] original;
            byte[] decompressed;
            int decompressedLen;

            bool match;

            PatternCompressor compressor = new PatternCompressor
                {
                    CompressedBuffer = new byte[4 * TotalTestSampleSize],
                    CompressionStrength = 31
                };

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                uint value = (uint)(rnd.NextDouble() * 100000);
                memStream.Write(BitConverter.GetBytes(value), 0, 4);
                compressor.Compress(value);
            }

            original = memStream.ToArray();
            decompressed = new byte[PatternDecompressor.MaximumSizeDecompressed(compressor.CompressedBufferLength)];
            Buffer.BlockCopy(compressor.CompressedBuffer, 0, decompressed, 0, compressor.CompressedBufferLength);
            decompressedLen = PatternDecompressor.DecompressBuffer(decompressed, 0, compressor.CompressedBufferLength, decompressed.Length);

            match = decompressedLen == original.Length;
            for (int i = 0; match && i < decompressedLen; i++)
            {
                match = decompressed[i] == original[i];
            }

            Assert.AreEqual(original.Length, decompressedLen);
            Assert.IsTrue(match);
        }

        [TestMethod]
        public void TestArrayOfFloatCompressionOnSequentialData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            float value = (float)(rnd.NextDouble() * 99999.99D);

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                value += 0.055F;

                if (i % 10 == 0)
                    value = (float)(rnd.NextDouble() * 99999.99D);

                buffer.Write(BitConverter.GetBytes(value), 0, 4);
            }

            // Add one byte of extra space to accommodate compression algorithm
            buffer.WriteByte(0xFF);

            byte[] arrayOfFloats = buffer.ToArray();
            byte[] copy = arrayOfFloats.BlockCopy(0, arrayOfFloats.Length);

            int bufferLen = arrayOfFloats.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfFloats.Compress().Length;
            int compressedLen, decompressedLen, maxDecompressedLen;

            Ticks compressTime, decompressTime;
            Ticks stopTime, startTime;

            bool lossless;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            compressedLen = PatternCompressor.CompressBuffer(arrayOfFloats, 0, dataLen, bufferLen, 31);
            stopTime = DateTime.UtcNow.Ticks;
            compressTime = stopTime - startTime;

            maxDecompressedLen = PatternDecompressor.MaximumSizeDecompressed(compressedLen);
            if (arrayOfFloats.Length < maxDecompressedLen)
            {
                byte[] temp = new byte[maxDecompressedLen];
                Buffer.BlockCopy(arrayOfFloats, 0, temp, 0, compressedLen);
                arrayOfFloats = temp;
            }

            startTime = DateTime.UtcNow.Ticks;
            decompressedLen = PatternDecompressor.DecompressBuffer(arrayOfFloats, 0, compressedLen, maxDecompressedLen);
            stopTime = DateTime.UtcNow.Ticks;
            decompressTime = stopTime - startTime;

            lossless = decompressedLen == dataLen;
            for (int i = 0; lossless && i < Math.Min(decompressedLen, dataLen); i++)
            {
                lossless = arrayOfFloats[i] == copy[i];
            }

            // Publish results to debug window
            results.AppendFormat("Results of floating point compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples:  \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:    \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total compression time:   \t{0}\r\n", compressTime.ToElapsedTimeString(4));
            results.AppendFormat("Compression speed:        \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / compressTime.ToSeconds());
            results.AppendFormat("Total decompression time: \t{0}\r\n", decompressTime.ToElapsedTimeString(4));
            results.AppendFormat("Decompression speed:      \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / decompressTime.ToSeconds());
            results.AppendFormat("Compression results:      \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:    \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreEqual(dataLen, decompressedLen);
            Assert.IsTrue(lossless);
        }

        [TestMethod]
        public void TestArrayOfIntCompressionOnSequentialData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            uint value = (uint)(rnd.NextDouble() * 100000);

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                unchecked
                {
                    value++;
                }

                if (i % 10 == 0)
                    value = (uint)(rnd.NextDouble() * 100000);

                buffer.Write(BitConverter.GetBytes(value), 0, 4);
            }

            // Add one byte of extra space to accommodate compression algorithm
            buffer.WriteByte(0xFF);

            byte[] arrayOfInts = buffer.ToArray();
            byte[] copy = arrayOfInts.BlockCopy(0, arrayOfInts.Length);

            int bufferLen = arrayOfInts.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfInts.Compress().Length;
            int compressedLen, decompressedLen, maxDecompressedLen;

            Ticks compressTime, decompressTime;
            Ticks stopTime, startTime;

            bool lossless;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            compressedLen = PatternCompressor.CompressBuffer(arrayOfInts, 0, dataLen, bufferLen, 31);
            stopTime = DateTime.UtcNow.Ticks;
            compressTime = stopTime - startTime;

            maxDecompressedLen = PatternDecompressor.MaximumSizeDecompressed(compressedLen);
            if (arrayOfInts.Length < maxDecompressedLen)
            {
                byte[] temp = new byte[maxDecompressedLen];
                Buffer.BlockCopy(arrayOfInts, 0, temp, 0, compressedLen);
                arrayOfInts = temp;
            }

            startTime = DateTime.UtcNow.Ticks;
            decompressedLen = PatternDecompressor.DecompressBuffer(arrayOfInts, 0, compressedLen, maxDecompressedLen);
            stopTime = DateTime.UtcNow.Ticks;
            decompressTime = stopTime - startTime;

            lossless = decompressedLen == dataLen;
            for (int i = 0; lossless && i < Math.Min(decompressedLen, dataLen); i++)
            {
                lossless = arrayOfInts[i] == copy[i];
            }

            // Publish results to debug window
            results.AppendFormat("Results of integer compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples:  \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:    \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total compression time:   \t{0}\r\n", compressTime.ToElapsedTimeString(4));
            results.AppendFormat("Compression speed:        \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / compressTime.ToSeconds());
            results.AppendFormat("Total decompression time: \t{0}\r\n", decompressTime.ToElapsedTimeString(4));
            results.AppendFormat("Decompression speed:      \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / decompressTime.ToSeconds());
            results.AppendFormat("Compression results:      \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:    \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreEqual(dataLen, decompressedLen);
            Assert.IsTrue(lossless);
        }

        [TestMethod]
        public void TestArrayOfFloatCompressionOnRandomData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            float value;

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                value = (float)(rnd.NextDouble() * 99999.99D);
                buffer.Write(BitConverter.GetBytes(value), 0, 4);
            }

            // Add one byte of extra space to accommodate compression algorithm
            buffer.WriteByte(0xFF);

            byte[] arrayOfFloats = buffer.ToArray();
            byte[] copy = arrayOfFloats.BlockCopy(0, arrayOfFloats.Length);

            int bufferLen = arrayOfFloats.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfFloats.Compress().Length;
            int compressedLen, decompressedLen, maxDecompressedLen;

            Ticks compressTime, decompressTime;
            Ticks stopTime, startTime;

            bool lossless;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            compressedLen = PatternCompressor.CompressBuffer(arrayOfFloats, 0, dataLen, bufferLen, 31);
            stopTime = DateTime.UtcNow.Ticks;
            compressTime = stopTime - startTime;

            maxDecompressedLen = PatternDecompressor.MaximumSizeDecompressed(compressedLen);
            if (arrayOfFloats.Length < maxDecompressedLen)
            {
                byte[] temp = new byte[maxDecompressedLen];
                Buffer.BlockCopy(arrayOfFloats, 0, temp, 0, compressedLen);
                arrayOfFloats = temp;
            }

            startTime = DateTime.UtcNow.Ticks;
            decompressedLen = PatternDecompressor.DecompressBuffer(arrayOfFloats, 0, compressedLen, maxDecompressedLen);
            stopTime = DateTime.UtcNow.Ticks;
            decompressTime = stopTime - startTime;

            lossless = decompressedLen == dataLen;
            for (int i = 0; lossless && i < Math.Min(decompressedLen, dataLen); i++)
            {
                lossless = arrayOfFloats[i] == copy[i];
            }

            // Publish results to debug window
            results.AppendFormat("Results of floating point compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples:  \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:    \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total compression time:   \t{0}\r\n", compressTime.ToElapsedTimeString(4));
            results.AppendFormat("Compression speed:        \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / compressTime.ToSeconds());
            results.AppendFormat("Total decompression time: \t{0}\r\n", decompressTime.ToElapsedTimeString(4));
            results.AppendFormat("Decompression speed:      \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / decompressTime.ToSeconds());
            results.AppendFormat("Compression results:      \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:    \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreEqual(dataLen, decompressedLen);
            Assert.IsTrue(lossless);
        }

        [TestMethod]
        public void TestArrayOfIntCompressionOnRandomData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            uint value;

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                value = (uint)(rnd.NextDouble() * 100000);
                buffer.Write(BitConverter.GetBytes(value), 0, 4);
            }

            // Add one byte of extra space to accommodate compression algorithm
            buffer.WriteByte(0xFF);

            byte[] arrayOfInts = buffer.ToArray();
            byte[] copy = arrayOfInts.BlockCopy(0, arrayOfInts.Length);

            int bufferLen = arrayOfInts.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfInts.Compress().Length;
            int compressedLen, decompressedLen, maxDecompressedLen;

            Ticks compressTime, decompressTime;
            Ticks stopTime, startTime;

            bool lossless;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = DateTime.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = DateTime.UtcNow.Ticks;
            compressedLen = PatternCompressor.CompressBuffer(arrayOfInts, 0, dataLen, bufferLen, 31);
            stopTime = DateTime.UtcNow.Ticks;
            compressTime = stopTime - startTime;

            maxDecompressedLen = PatternDecompressor.MaximumSizeDecompressed(compressedLen);
            if (arrayOfInts.Length < maxDecompressedLen)
            {
                byte[] temp = new byte[maxDecompressedLen];
                Buffer.BlockCopy(arrayOfInts, 0, temp, 0, compressedLen);
                arrayOfInts = temp;
            }

            startTime = DateTime.UtcNow.Ticks;
            decompressedLen = PatternDecompressor.DecompressBuffer(arrayOfInts, 0, compressedLen, maxDecompressedLen);
            stopTime = DateTime.UtcNow.Ticks;
            decompressTime = stopTime - startTime;

            lossless = decompressedLen == dataLen;
            for (int i = 0; lossless && i < Math.Min(decompressedLen, dataLen); i++)
            {
                lossless = arrayOfInts[i] == copy[i];
            }

            // Publish results to debug window
            results.AppendFormat("Results of floating point compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples:  \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:    \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total compression time:   \t{0}\r\n", compressTime.ToElapsedTimeString(4));
            results.AppendFormat("Compression speed:        \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / compressTime.ToSeconds());
            results.AppendFormat("Total decompression time: \t{0}\r\n", decompressTime.ToElapsedTimeString(4));
            results.AppendFormat("Decompression speed:      \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / decompressTime.ToSeconds());
            results.AppendFormat("Compression results:      \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:    \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreEqual(dataLen, decompressedLen);
            Assert.IsTrue(lossless);
        }

        //[TestMethod]
        //public void TestArrayOfDoubleCompressionOnSequentialData()
        //{
        //    StringBuilder results = new StringBuilder();
        //    MemoryStream buffer = new MemoryStream();
        //    Random rnd = new Random();

        //    double value = (double)(rnd.NextDouble() * 99999.99D);

        //    for (int i = 0; i < TotalTestSampleSize; i++)
        //    {
        //        value += 0.0055D;

        //        if (i % 10 == 0)
        //            value = (rnd.NextDouble() * 99999.99D);

        //        buffer.Write(BitConverter.GetBytes(value), 0, 8);
        //    }

        //    // Add one byte of extra space to accommodate compression algorithm
        //    buffer.WriteByte(0xff);

        //    byte[] arrayOfDoubles = buffer.ToArray();
        //    int bufferLen = arrayOfDoubles.Length;
        //    int dataLen = bufferLen - 1;
        //    int gzipLen = arrayOfDoubles.Compress().Length;
        //    Ticks stopTime, startTime;

        //    // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    int compressedLen = arrayOfDoubles.Compress64bitEnumeration(0, dataLen, bufferLen, 5);
        //    stopTime = DateTime.UtcNow.Ticks;

        //    // Publish results to debug window
        //    results.AppendFormat("Results of double precision floating point compression algorithm over sequential data:\r\n\r\n");
        //    results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
        //    results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
        //    results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
        //    results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
        //    results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
        //    results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
        //    Debug.WriteLine(results.ToString());

        //    Assert.AreNotEqual(compressedLen, dataLen);
        //}

        //[TestMethod]
        //public void TestArrayOfLongCompressionOnSequentialData()
        //{
        //    StringBuilder results = new StringBuilder();
        //    MemoryStream buffer = new MemoryStream();
        //    Random rnd = new Random();

        //    ulong value = (ulong)(rnd.NextDouble() * 100000);

        //    for (int i = 0; i < TotalTestSampleSize; i++)
        //    {
        //        unchecked
        //        {
        //            value++;
        //        }

        //        if (i % 10 == 0)
        //            value = (ulong)(rnd.NextDouble() * 100000);

        //        buffer.Write(BitConverter.GetBytes(value), 0, 8);
        //    }

        //    // Add one byte of extra space to accomodate compression algorithm
        //    buffer.WriteByte(0xff);

        //    byte[] arrayOfLongs = buffer.ToArray();
        //    int bufferLen = arrayOfLongs.Length;
        //    int dataLen = bufferLen - 1;
        //    int gzipLen = arrayOfLongs.Compress().Length;
        //    Ticks stopTime, startTime;

        //    // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    int compressedLen = arrayOfLongs.Compress64bitEnumeration(0, dataLen, bufferLen, 5);
        //    stopTime = DateTime.UtcNow.Ticks;

        //    // Publish results to debug window
        //    results.AppendFormat("Results of long integer compression algorithm over sequential data:\r\n\r\n");
        //    results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
        //    results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
        //    results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
        //    results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
        //    results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
        //    results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
        //    Debug.WriteLine(results.ToString());

        //    Assert.AreNotEqual(compressedLen, dataLen);
        //}

        //[TestMethod]
        //public void TestArrayOfDoubleCompressionOnRandomData()
        //{
        //    StringBuilder results = new StringBuilder();
        //    MemoryStream buffer = new MemoryStream();
        //    Random rnd = new Random();

        //    double value;

        //    for (int i = 0; i < TotalTestSampleSize; i++)
        //    {
        //        value = (rnd.NextDouble() * 99999.99D);
        //        buffer.Write(BitConverter.GetBytes(value), 0, 8);
        //    }

        //    // Add one byte of extra space to accomodate compression algorithm
        //    buffer.WriteByte(0xff);

        //    byte[] arrayOfDoubles = buffer.ToArray();
        //    int bufferLen = arrayOfDoubles.Length;
        //    int dataLen = bufferLen - 1;
        //    int gzipLen = arrayOfDoubles.Compress().Length;
        //    Ticks stopTime, startTime;

        //    // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    int compressedLen = arrayOfDoubles.Compress64bitEnumeration(0, dataLen, bufferLen, 150);
        //    stopTime = DateTime.UtcNow.Ticks;

        //    // Publish results to debug window
        //    results.AppendFormat("Results of double precision floating point compression algorithm over random data:\r\n\r\n");
        //    results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
        //    results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
        //    results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
        //    results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
        //    results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
        //    results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
        //    Debug.WriteLine(results.ToString());

        //    Assert.AreNotEqual(compressedLen, dataLen);
        //}

        //[TestMethod]
        //public void TestArrayOfLongCompressionOnRandomData()
        //{
        //    StringBuilder results = new StringBuilder();
        //    MemoryStream buffer = new MemoryStream();
        //    Random rnd = new Random();

        //    ulong value;

        //    for (int i = 0; i < TotalTestSampleSize; i++)
        //    {
        //        value = (ulong)(rnd.NextDouble() * 100000);
        //        buffer.Write(BitConverter.GetBytes(value), 0, 8);
        //    }

        //    // Add one byte of extra space to accomodate compression algorithm
        //    buffer.WriteByte(0xff);

        //    byte[] arrayOfLongs = buffer.ToArray();
        //    int bufferLen = arrayOfLongs.Length;
        //    int dataLen = bufferLen - 1;
        //    int gzipLen = arrayOfLongs.Compress().Length;
        //    Ticks stopTime, startTime;

        //    // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
        //    stopTime = DateTime.UtcNow.Ticks;
        //    results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

        //    startTime = DateTime.UtcNow.Ticks;
        //    int compressedLen = arrayOfLongs.Compress64bitEnumeration(0, dataLen, bufferLen, 150);
        //    stopTime = DateTime.UtcNow.Ticks;

        //    // Publish results to debug window
        //    results.AppendFormat("Results of long integer compression algorithm over random data:\r\n\r\n");
        //    results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
        //    results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
        //    results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
        //    results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
        //    results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
        //    results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
        //    Debug.WriteLine(results.ToString());

        //    Assert.AreNotEqual(compressedLen, dataLen);
        //}
    }
}
