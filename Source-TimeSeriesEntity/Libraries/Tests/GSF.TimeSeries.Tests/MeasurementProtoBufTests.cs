using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

namespace GSF.TimeSeries.Tests
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
            uint COUNT = 1000000;

            Stopwatch writeTime = new Stopwatch();
            writeTime.Start();
            for (uint i = 0; i < COUNT; i++)
            {
                Measurement<float> measurement = new Measurement<float>(Guid.NewGuid(), new Ticks(DateTime.Now), rand.Next());

                Serializer.Serialize<Measurement<float>>(stream, measurement);
            }
            writeTime.Stop();

            stream.Position = 0;
            Stopwatch readTime = new Stopwatch();
            readTime.Start();
            for (uint i = 0; i < COUNT; i++)
            {
                Measurement<float> deserializedMeasurement = Serializer.Deserialize<Measurement<float>>(stream);
            }
            readTime.Stop();

            float samplesPerSecond = (((float)COUNT) / ((float)readTime.ElapsedMilliseconds)) * 1000f;
            TraceListener listener = new ConsoleTraceListener();
            Trace.Listeners.Add(listener);
            Trace.WriteLine("Write duration: " + writeTime.Elapsed + ", read duration: " + readTime.Elapsed);
            Trace.WriteLine("Samples per second: " + samplesPerSecond);
            listener.Flush();

            Assert.IsTrue(samplesPerSecond > 500000, "Need at least 500,000 samples/second (got " + samplesPerSecond + ")");
        }
    }
}
