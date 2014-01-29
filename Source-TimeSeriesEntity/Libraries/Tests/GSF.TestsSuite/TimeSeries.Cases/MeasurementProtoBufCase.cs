using System;
using System.Diagnostics;
using System.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Serialization;
using GSF.TimeSeries.Serialization.ProtoBufWrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.TestsSuite.GSF.TimeSeries.Cases
{
    [TestClass]
    public class MeasurementProtoBufTests
    {
        [TestMethod]
        public void BasicSerializationTest()
        {
            // Memory seems like a good compromise between speed of access
            // and space available.
            MemoryStream stream = new MemoryStream();
            Random rand = new Random();

            // This is fast enough with a single core, though not by much, 
            // but with threading we shouldn't have any issue hitting
            // our 500,000 measurements/second throughput target
            // Debug Build
            // 2.2820839 s to instantiate and serialize 1,000,000 random
            // Measurement<int>s
            // Write: 438,195.98 measurements/sec
            // 1.2432899 s to deserialize 1,000,000 measurements
            // Read: 804,317.64 measurements/sec
            //
            // Release Build
            // 2.4735471 s to instantiate and serialize 1,000,000 random 
            // Measurement<int>s
            // Write: 404,277.72 measurements/sec
            // 1.2524553 s to deserialize 1,000,000 measurements
            // Read: 798,431.68 measurements/sec
            // Noise is from running this on a multiuser system.
            //
            // We're now getting 351,493.8 measurements/second.  That's 
            // too slow but is fine as long as all measurements
            // aren't from a single stream, since then we can parallelize
            uint COUNT = 1000000;

            ISerializer serializer = new Serializer();

            byte[][] serializedItems = new byte[COUNT][];
            Measurement<float>[] measurementsToSerialize = new Measurement<float>[COUNT];

            Stopwatch writeTime = new Stopwatch();
            writeTime.Start();
            for (uint i = 0; i < COUNT; i++)
            {
                measurementsToSerialize[i] = new Measurement<float>(Guid.NewGuid(), new Ticks(DateTime.Now), rand.Next());
                serializedItems[i] = serializer.Serialize<float>(measurementsToSerialize[i]);
            }
            writeTime.Stop();

            Measurement<float>[] deserializedMeasurements = new Measurement<float>[COUNT];

            stream.Position = 0;
            Stopwatch readTime = new Stopwatch();
            readTime.Start();
            for (uint i = 0; i < COUNT; i++)
            {
                deserializedMeasurements[i] = serializer.Deserialize<float>(serializedItems[i]);
            }
            readTime.Stop();

            float readSamplesPerSecond = (((float)COUNT) / ((float)readTime.ElapsedMilliseconds)) * 1000f;
            float writeSamplesPerSecond = (((float)COUNT) / ((float)writeTime.ElapsedMilliseconds)) * 1000f;
            TraceListener listener = new ConsoleTraceListener();
            Trace.Listeners.Add(listener);
            Trace.WriteLine("Write duration: " + writeTime.Elapsed + ", read duration: " + readTime.Elapsed);
            Trace.WriteLine("Read samples per second: " + readSamplesPerSecond);
            Trace.WriteLine("Write samples per second: " + writeSamplesPerSecond);
            listener.Flush();

            // Comparing in a loop with four asserts so that the error message indicates which one didn't work.
            // The .Equals vs == problem is preventing this from having a single assert
            for (uint i = 0; i < COUNT; i++)
            {
                Assert.AreEqual(measurementsToSerialize[i].ID, deserializedMeasurements[i].ID, "Serialized measurements " + i + " ID are not the same");
                Assert.AreEqual(measurementsToSerialize[i].Timestamp, deserializedMeasurements[i].Timestamp, "Serialized measurements " + i + " timestamp are not the same");
                Assert.AreEqual(measurementsToSerialize[i].StateFlags, deserializedMeasurements[i].StateFlags, "Serialized measurements " + i + " StateFlags are not the same");
                Assert.AreEqual(measurementsToSerialize[i].Value, deserializedMeasurements[i].Value, "Serialized measurements " + i + " Value are not the same");
            }
            Assert.IsTrue(readSamplesPerSecond > 500000, "Need to read at least 500,000 samples/second (got " + readSamplesPerSecond + ")");
        }
    }
}
