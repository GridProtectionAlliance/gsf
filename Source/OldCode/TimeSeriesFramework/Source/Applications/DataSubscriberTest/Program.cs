using System;
using System.Collections.Generic;
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
        static System.Timers.Timer timer = new System.Timers.Timer(10000);

        static void Main(string[] args)
        {
            // Attach to subscriber events
            subscriber.StatusMessage += subscriber_StatusMessage;
            subscriber.ProcessException += subscriber_ProcessException;
            subscriber.ConnectionEstablished += subscriber_ConnectionEstablished;
            subscriber.ConnectionTerminated += subscriber_ConnectionTerminated;
            subscriber.NewMeasurements += subscriber_NewMeasurements;

            // Initialize subscriber
            subscriber.ConnectionString = "server=localhost:6165";
            subscriber.Initialize();

            // Start subscriber connection cycle
            subscriber.Start();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            Console.ReadLine();

            subscriber.Unsubscribe();

            subscriber.StatusMessage -= subscriber_StatusMessage;
            subscriber.ProcessException -= subscriber_ProcessException;
            subscriber.ConnectionEstablished -= subscriber_ConnectionEstablished;
            subscriber.ConnectionTerminated -= subscriber_ConnectionTerminated;
            subscriber.NewMeasurements -= subscriber_NewMeasurements;

            timer.Elapsed -= timer_Elapsed;
            timer.Stop();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (subscriber.IsConnected)
            {
                if (TVA.Security.Cryptography.Random.Boolean)
                {
                    Console.WriteLine("Initiating synchronized subscription...");
                    subscriber.SynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1;DEVARCHIVE:2");
                }
                else
                {
                    if (TVA.Security.Cryptography.Random.Boolean)
                    {
                        Console.WriteLine("Initiating on-change unsynchronized subscription...");
                        subscriber.UnsynchronizedSubscribe(true, false, "DEVARCHIVE:1;DEVARCHIVE:2");
                    }
                    else
                    {
                        Console.WriteLine("Initiating throttled unsynchronized subscription...");
                        subscriber.UnsynchronizedSubscribe(true, true, "DEVARCHIVE:1;DEVARCHIVE:2", null, true, 5.0D, 1.0D, false);
                    }
                }
            }
        }

        static void subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            //Console.WriteLine(string.Format("{0} total measurements received so far: {1}", e.Argument.Count, e.Argument.ToDelimitedString(", ")));

            dataCount += e.Argument.Count;

            if (dataCount % (5 * 60) == 0)
                Console.WriteLine(string.Format("{0} total measurements received so far: {1}", dataCount, e.Argument.ToDelimitedString(", ")));
        }

        static void subscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            //subscriber.SynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1;DEVARCHIVE:2;DEVARCHIVE:3;DEVARCHIVE:4;DEVARCHIVE:5");
            //subscriber.SynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1");
            //subscriber.UnsynchronizedSubscribe(true, "DEVARCHIVE:1;DEVARCHIVE:2;DEVARCHIVE:3;DEVARCHIVE:4;DEVARCHIVE:5");
        }

        static void subscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            Console.WriteLine("Connection to publisher was terminated, restarting connection cycle...");
            subscriber.Start();
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
