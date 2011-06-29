using System;
using System.Collections.Generic;
using System.Threading;
using TimeSeriesFramework;
using TimeSeriesFramework.Transport;
using TVA;


namespace DataPublisherTest
{
    class Program
    {
        static DataPublisher publisher = new DataPublisher();

        static void Main(string[] args)
        {
            // Attach to publisher events
            publisher.StatusMessage += publisher_StatusMessage;
            publisher.ProcessException += publisher_ProcessException;
            publisher.ProcessMeasurementFilter = true;
            //publisher.SharedSecret = "TimeSeriesLibraryTest";

            // Initialize publisher
            publisher.Initialize();

            // Start publisher
            publisher.Start();

            ThreadPool.QueueUserWorkItem(ProcessMeasurements);

            Console.ReadLine();

            publisher.Stop();

            publisher.StatusMessage -= publisher_StatusMessage;
            publisher.ProcessException -= publisher_ProcessException;
        }

        static void publisher_ProcessException(object sender, EventArgs<Exception> e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("EXCEPTION: " + e.Argument.Message);
            Console.ResetColor();
        }

        static void publisher_StatusMessage(object sender, EventArgs<string> e)
        {
            Console.WriteLine(e.Argument);
        }

        static void ProcessMeasurements(object state)
        {
            while (true)
            {
                List<Measurement> measurements = new List<Measurement>();

                Measurement measurement;

                for (int i = 1; i < 11; i++)
                {
                    measurement = new Measurement()
                    {
                        Key = new MeasurementKey((uint)i, "DEVARCHIVE"),
                        Value = TVA.Security.Cryptography.Random.Between(-65535.0D, 65536.0D),
                        Timestamp = DateTime.UtcNow.Ticks
                    };

                    measurements.Add(measurement);
                }

                publisher.QueueMeasurementsForProcessing(measurements);

                Thread.Sleep(33);
            }
        }
    }
}
