using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TVA.IO;
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
            //
            // TODO: Add constructor logic here
            //
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
        // Compression on same buffer using GZip has less than 1% compression (0.11%)
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

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));

            Ticks stopTime, startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfFloats.CompressFloatEnumeration(0, dataLen, bufferLen, 5);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of floating point compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression strength:    \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Compared to gzip:        \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)gzipLen);
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

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + TotalTestSampleSize));

            Ticks stopTime, startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfFloats.CompressFloatEnumeration(0, dataLen, bufferLen, 15);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of floating point compression algorithm over random data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression strength:    \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Compared to gzip:        \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)gzipLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Sequential data averages ~30% with compression strength of 5
        // Compression on same buffer using GZip has no compression (-0.12%)
        // Sample calculation speed = 21.6 MB/s
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

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));

            Ticks stopTime, startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfDoubles.CompressDoubleEnumeration(0, dataLen, bufferLen, 5);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of double precision floating point compression algorithm over sequential data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression strength:    \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Compared to gzip:        \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)gzipLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }

        [TestMethod]
        // Random data averages ~10% compression with compression strength of 150
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

            // Make sure a buffer exists in the buffer pool so that operation time will not be skewed by buffer initialization:
            BufferPool.ReturnBuffer(BufferPool.TakeBuffer(dataLen + 2 * TotalTestSampleSize));

            Ticks stopTime, startTime = PrecisionTimer.UtcNow.Ticks;
            int compressedLen = arrayOfDoubles.CompressDoubleEnumeration(0, dataLen, bufferLen, 150);
            stopTime = PrecisionTimer.UtcNow.Ticks;

            // Publish results to debug window
            results.AppendFormat("Results of double precision floating point compression algorithm over random data:\r\n\r\n");
            results.AppendFormat("Total number of samples: \t{0:#,##0}\r\n", TotalTestSampleSize);
            results.AppendFormat("Total number of bytes:   \t{0:#,##0}\r\n", dataLen);
            results.AppendFormat("Total Calculation time:  \t{0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendFormat("Calculation speed:       \t{0:#,##0.0000} MB/sec\r\n", (dataLen / (double)SI2.Mega) / (stopTime - startTime).ToSeconds());
            results.AppendFormat("Compression strength:    \t{0:0.00%}\r\n", (dataLen - compressedLen) / (double)dataLen);
            results.AppendFormat("Compared to gzip:        \t{0:0.00%}\r\n", (dataLen - gzipLen) / (double)gzipLen);
            Debug.WriteLine(results.ToString());

            Assert.AreNotEqual(compressedLen, dataLen);
        }
    }
}
