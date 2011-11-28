using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TVA.IO.Compression;
using TVA.Units;

namespace TVA.Core.Tests
{
    /// <summary>
    /// Summary description for PatternCompressorTest
    /// </summary>
    [TestClass]
    public class PatternCompressorTest
    {
        public PatternCompressorTest()
        {
        }

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

        private const int TotalTestSampleSize = int.MaxValue / 500;

        [TestMethod]
        // Sequential data averages ~66% with a compression stength of 5
        // Compression on same buffer using GZip has less than 1% compression (0.19%)
        // Sample calculation speed = 17.9 MB/s
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

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfFloats = buffer.ToArray();
            int bufferLen = arrayOfFloats.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfFloats.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfFloats.Compress32bitEnumeration(0, dataLen, bufferLen, 5);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of floating point compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Sequential data averages ~71% with a compression stength of 5
        // Compression on same buffer using GZip averages ~16% compression
        // Sample calculation speed = 18.5 MB/s
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

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfInts = buffer.ToArray();
            int bufferLen = arrayOfInts.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfInts.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfInts.Compress32bitEnumeration(0, dataLen, bufferLen, 5);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of integer compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Random data averages ~25% compression with a compression strength of 15
        // Compression on same buffer using GZip has no compression (-0.12%)
        // Sample calculation speed = 5.5 MB/s
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

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfFloats = buffer.ToArray();
            int bufferLen = arrayOfFloats.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfFloats.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfFloats.Compress32bitEnumeration(0, dataLen, bufferLen, 15);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of floating point compression algorithm over random data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Random data averages ~51% compression with a compression strength of 15
        // Compression on same buffer using GZip has no compression (-0.12%)
        // Sample calculation speed = 5.1 MB/s
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

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfInts = buffer.ToArray();
            int bufferLen = arrayOfInts.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfInts.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfInts.Compress32bitEnumeration(0, dataLen, bufferLen, 15);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of integer compression algorithm over random data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Sequential data averages ~30.5% with compression strength of 5
        // Compression on same buffer using GZip has no compression (-0.12%)
        // Sample calculation speed = 20 MB/s
        public void TestArrayOfDoubleCompressionOnSequentialData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            double value = (double)(rnd.NextDouble() * 99999.99D);

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                value += 0.0055D;

                if (i % 10 == 0)
                    value = (rnd.NextDouble() * 99999.99D);

                buffer.Write(BitConverter.GetBytes(value), 0, 8);
            }

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfDoubles = buffer.ToArray();
            int bufferLen = arrayOfDoubles.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfDoubles.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfDoubles.Compress64bitEnumeration(0, dataLen, bufferLen, 5);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of double precision floating point compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Sequential data averages ~73% with compression strength of 5
        // Compression on same buffer using GZip averages ~59% compression
        // Sample calculation speed = 30 MB/s
        public void TestArrayOfLongCompressionOnSequentialData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            ulong value = (ulong)(rnd.NextDouble() * 100000);

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                unchecked
                {
                    value++;
                }

                if (i % 10 == 0)
                    value = (ulong)(rnd.NextDouble() * 100000);

                buffer.Write(BitConverter.GetBytes(value), 0, 8);
            }

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfLongs = buffer.ToArray();
            int bufferLen = arrayOfLongs.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfLongs.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfLongs.Compress64bitEnumeration(0, dataLen, bufferLen, 5);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of long integer compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Random data averages ~11% compression with compression strength of 150
        // Compression on same buffer using GZip has no compression (-0.12%)
        // Sample calculation speed = 1.4 MB/s
        public void TestArrayOfDoubleCompressionOnRandomData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            double value;

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                value = (rnd.NextDouble() * 99999.99D);
                buffer.Write(BitConverter.GetBytes(value), 0, 8);
            }

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfDoubles = buffer.ToArray();
            int bufferLen = arrayOfDoubles.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfDoubles.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfDoubles.Compress64bitEnumeration(0, dataLen, bufferLen, 150);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of double precision floating point compression algorithm over random data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Random data averages ~66.5% compression with compression strength of 150
        // Compression on same buffer using GZip averages 50% compression
        // Sample calculation speed = 1 MB/s
        public void TestArrayOfLongCompressionOnRandomData()
        {
            StringBuilder results = new StringBuilder();
            MemoryStream buffer = new MemoryStream();
            Random rnd = new Random();

            ulong value;

            for (int i = 0; i < TotalTestSampleSize; i++)
            {
                value = (ulong)(rnd.NextDouble() * 100000);
                buffer.Write(BitConverter.GetBytes(value), 0, 8);
            }

            // Add one byte of extra space to accomodate compression algorithm
            buffer.WriteByte(0xff);

            byte[] arrayOfLongs = buffer.ToArray();
            int bufferLen = arrayOfLongs.Length;
            int dataLen = bufferLen - 1;
            int gzipLen = arrayOfLongs.Compress().Length;
            Ticks stopTime, startTime;

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool initial take time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));
            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Buffer Pool cached take time: {0}\r\n\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfLongs.Compress64bitEnumeration(0, dataLen, bufferLen, 150);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of long integer compression algorithm over random data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression results:     \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Standard gzip results:   \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)dataLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }
    }
}
