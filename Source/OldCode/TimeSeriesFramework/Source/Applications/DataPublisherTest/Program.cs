using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TimeSeriesFramework;
using TimeSeriesFramework.Transport;
using TVA;
using TVA.Collections;

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
                Measurement newMeasurement, measurement = new Measurement(0, "DEVARCHIVE", double.NaN, DateTime.UtcNow.Ticks);

                for (int i = 1; i < 11; i++)
                {
                    newMeasurement = Measurement.Clone(measurement);
                    newMeasurement.ID = (uint)i;
                    newMeasurement.Value = TVA.Security.Cryptography.Random.Between(-65535.0D, 65536.0D);
                    measurements.Add(newMeasurement);
                }

                publisher.QueueMeasurementsForProcessing(measurements);

                Thread.Sleep(33);
            }
        }
    }
}
