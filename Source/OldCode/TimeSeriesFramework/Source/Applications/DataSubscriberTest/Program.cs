using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSeriesFramework;
using TimeSeriesFramework.Transport;
using TVA;
using TVA.Collections;

namespace DataSubscriberTest
{
    class Program
    {
        static DataSubscriber subscriber = new DataSubscriber();
        static long dataCount = 0;

        static void Main(string[] args)
        {
            // Attach to subscriber events
            subscriber.StatusMessage += subscriber_StatusMessage;
            subscriber.ProcessException += subscriber_ProcessException;
            subscriber.ConnectionEstablished += subscriber_ConnectionEstablished;
            subscriber.NewMeasurements += subscriber_NewMeasurements;

            // Initialize subscriber
            subscriber.ConnectionString = "server=localhost:6165";
            subscriber.Initialize();

            // Start subscriber connection cycle
            subscriber.Start();

            Console.ReadLine();

            subscriber.Unsubscribe();

            subscriber.StatusMessage -= subscriber_StatusMessage;
            subscriber.ProcessException -= subscriber_ProcessException;
            subscriber.ConnectionEstablished -= subscriber_ConnectionEstablished;
            subscriber.NewMeasurements -= subscriber_NewMeasurements;
        }

        static void subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            dataCount += e.Argument.Count;

            if (dataCount % (5 * 60) == 0)
                Console.WriteLine(string.Format("{0} total measurements received so far: {1}", dataCount, e.Argument.ToDelimitedString(", ")));
        }

        static void subscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            subscriber.SynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1;DEVARCHIVE:2;DEVARCHIVE:3;DEVARCHIVE:4;DEVARCHIVE:5");
        }

        static void subscriber_ProcessException(object sender, TVA.EventArgs<Exception> e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("EXCEPTION: " + e.Argument.Message);
            Console.ResetColor();
        }

        static void subscriber_StatusMessage(object sender, TVA.EventArgs<string> e)
        {
            Console.WriteLine(e.Argument);
        }
    }
}
