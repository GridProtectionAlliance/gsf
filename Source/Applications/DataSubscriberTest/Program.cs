//******************************************************************************************************
//  Program.cs - Gbtc
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
//  06/30/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Timers;
using GSF;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
using Random = GSF.Security.Cryptography.Random;

namespace DataSubscriberTest
{
    class Program
    {
        // Define subscription modes for testing
        static readonly SynchronizedSubscriptionInfo remotelySynchronizedInfo = new SynchronizedSubscriptionInfo(true, 30);
        static readonly SynchronizedSubscriptionInfo locallySynchronizedInfo = new SynchronizedSubscriptionInfo(false, 30);
        static readonly UnsynchronizedSubscriptionInfo unsynchronizedInfo = new UnsynchronizedSubscriptionInfo(false);
        static readonly UnsynchronizedSubscriptionInfo throttledInfo = new UnsynchronizedSubscriptionInfo(true);

        static readonly DataSubscriber subscriber = new DataSubscriber();
        static long dataCount;
        static readonly Timer timer = new Timer(10000);
        static readonly object displayLock = new object();

        private const int MeasurementCount = 200;

        static void Main(string[] args)
        {
            // Attach to subscriber events
            subscriber.StatusMessage += subscriber_StatusMessage;
            subscriber.ProcessException += subscriber_ProcessException;
            subscriber.ConnectionEstablished += subscriber_ConnectionEstablished;
            subscriber.ConnectionTerminated += subscriber_ConnectionTerminated;
            subscriber.NewMeasurements += subscriber_NewMeasurements;
            subscriber.MetaDataReceived += subscriber_MetaDataReceived;

            StringBuilder connectionString = new StringBuilder();

            // Define connection string
            for (int i = 1; i <= MeasurementCount; i++)
            {
                connectionString.AppendFormat("DEVARCHIVE:{0};", i);
            }

            // Set up subscription info objects
            remotelySynchronizedInfo.LagTime = 0.5D;
            remotelySynchronizedInfo.LeadTime = 1.0D;
            remotelySynchronizedInfo.FilterExpression = connectionString.ToString();

            locallySynchronizedInfo.LagTime = 0.5D;
            locallySynchronizedInfo.LeadTime = 1.0D;
            locallySynchronizedInfo.FilterExpression = connectionString.ToString();

            unsynchronizedInfo.FilterExpression = connectionString.ToString();

            throttledInfo.LagTime = 5.0D;
            throttledInfo.LeadTime = 1.0D;
            throttledInfo.FilterExpression = connectionString.ToString();

            // Initialize subscriber
            subscriber.UseSimpleTcpClient = true;
            //subscriber.ConnectionString = "server=tcp://127.0.0.1:9898; useZeroMQChannel=true";
            subscriber.ConnectionString = "server=localhost:9898";
            subscriber.OperationalModes |= OperationalModes.UseCommonSerializationFormat | OperationalModes.CompressMetadata | OperationalModes.CompressSignalIndexCache | OperationalModes.CompressPayloadData;
            //subscriber.CompressionModes = CompressionModes.TSSC | CompressionModes.GZip;
            subscriber.Initialize();

            // Start subscriber connection cycle
            subscriber.Start();

            timer.Elapsed += timer_Elapsed;
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

        static void subscriber_MetaDataReceived(object sender, EventArgs<System.Data.DataSet> e)
        {
            DataSet dataSet = e.Argument;
            dataSet.WriteXml(FilePath.GetAbsolutePath("Metadata.xml"), XmlWriteMode.WriteSchema);
            Console.WriteLine("Data set serialized with {0} tables...", dataSet.Tables.Count);
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (subscriber.IsConnected)
            {
                if (Random.Boolean)
                {
                    if (Random.Boolean)
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating remotely synchronized subscription...");
                        }
                        subscriber.SynchronizedSubscribe(remotelySynchronizedInfo);
                    }
                    else
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating locally synchronized subscription...");
                        }
                        subscriber.SynchronizedSubscribe(locallySynchronizedInfo);
                    }
                }
                else
                {
                    if (Random.Boolean)
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating on-change unsynchronized subscription...");
                        }
                        subscriber.UnsynchronizedSubscribe(unsynchronizedInfo);
                    }
                    else
                    {
                        lock (displayLock)
                        {
                            Console.WriteLine("Initiating throttled unsynchronized subscription...");
                        }
                        subscriber.UnsynchronizedSubscribe(throttledInfo);
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

                //// Occasionally request another cipher key rotation
                //if (GSF.Security.Cryptography.Random.Boolean)
                //    subscriber.SendServerCommand(ServerCommand.RotateCipherKeys);
            }
        }

        static void subscriber_ConnectionEstablished(object sender, EventArgs e)
        {

            //subscriber.SendServerCommand(ServerCommand.MetaDataRefresh);

            //// Request cipher key rotation
            //subscriber.SendServerCommand(ServerCommand.RotateCipherKeys);

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

        static void subscriber_ProcessException(object sender, EventArgs<Exception> e)
        {
            lock (displayLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("EXCEPTION: " + e.Argument.Message);
                Console.ResetColor();
            }
        }

        static void subscriber_StatusMessage(object sender, EventArgs<string> e)
        {
            lock (displayLock)
            {
                Console.WriteLine(e.Argument);
            }
        }
    }
}
