//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/30/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using TimeSeriesFramework;
using TimeSeriesFramework.Transport;
using TVA;

namespace DataSubscriberTest
{
    class Program
    {
        static DataSubscriber subscriber = new DataSubscriber();
        static long dataCount = 0;
        static System.Timers.Timer timer = new System.Timers.Timer(10000);
        static object displayLock = new object();

        static void Main(string[] args)
        {
            // Attach to subscriber events
            subscriber.StatusMessage += subscriber_StatusMessage;
            subscriber.ProcessException += subscriber_ProcessException;
            subscriber.ConnectionEstablished += subscriber_ConnectionEstablished;
            subscriber.ConnectionTerminated += subscriber_ConnectionTerminated;
            subscriber.NewMeasurements += subscriber_NewMeasurements;

            // Initialize subscriber
            subscriber.ConnectionString = "server=localhost:6177";
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
                    if (TVA.Security.Cryptography.Random.Boolean)
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating remotely synchronized subscription...");
                        }
                        subscriber.RemotelySynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1;DEVARCHIVE:2");
                    }
                    else
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating locally synchronized subscription...");
                        }
                        subscriber.LocallySynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1;DEVARCHIVE:2");
                    }
                }
                else
                {
                    if (TVA.Security.Cryptography.Random.Boolean)
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating on-change unsynchronized subscription...");
                        }
                        subscriber.UnsynchronizedSubscribe(true, false, "DEVARCHIVE:1;DEVARCHIVE:2");
                    }
                    else
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating throttled unsynchronized subscription...");
                        }
                        subscriber.UnsynchronizedSubscribe(true, true, "DEVARCHIVE:1;DEVARCHIVE:2", null, true, 5.0D, 1.0D, false);
                    }
                }
            }
        }

        static void subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            // Check to see if total number of added points will exceed process interval used to show periodic
            // messages of how many points have been archived so far...
            const int interval = 5 * 60;

            bool showMessage = dataCount + e.Argument.Count >= (dataCount / interval + 1) * interval;

            dataCount += e.Argument.Count;

            if (showMessage)
            {
                lock (displayLock)
                {
                    Console.WriteLine(string.Format("{0:N0} measurements have been processed so far...", dataCount));
                }
            }
        }

        static void subscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            //subscriber.SynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1;DEVARCHIVE:2;DEVARCHIVE:3;DEVARCHIVE:4;DEVARCHIVE:5");
            //subscriber.SynchronizedSubscribe(true, 30, 0.5D, 1.0D, "DEVARCHIVE:1");
            //subscriber.UnsynchronizedSubscribe(true, "DEVARCHIVE:1;DEVARCHIVE:2;DEVARCHIVE:3;DEVARCHIVE:4;DEVARCHIVE:5");
        }

        static void subscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            subscriber.Start();

            lock (displayLock)
            {
                Console.WriteLine("Connection to publisher was terminated, restarting connection cycle...");
            }
        }

        static void subscriber_ProcessException(object sender, TVA.EventArgs<Exception> e)
        {
            lock (displayLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("EXCEPTION: " + e.Argument.Message);
                Console.ResetColor();
            }
        }

        static void subscriber_StatusMessage(object sender, TVA.EventArgs<string> e)
        {
            lock (displayLock)
            {
                Console.WriteLine(e.Argument);
            }
        }
    }
}
