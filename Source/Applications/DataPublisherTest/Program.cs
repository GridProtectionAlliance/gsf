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
using System.Threading;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
using Random = GSF.Security.Cryptography.Random;

namespace DataPublisherTest
{
    class Program
    {
        static readonly DataPublisher publisher = new DataPublisher();
        static Ticks lastDisplayTime;
        static readonly object displayLock = new object();

        private const int MeasurementCount = 200;

        static void Main(string[] args)
        {
            // Attach to publisher events
            publisher.StatusMessage += publisher_StatusMessage;
            publisher.ProcessException += publisher_ProcessException;
            publisher.ClientConnected += publisher_ClientConnected;

            //using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            //{
            //    MeasurementKey.EstablishDefaultCache(database.Connection, database.AdapterType);

            // Initialize publisher
            //publisher.DataMember = "ActiveMeasurements";
            //publisher.DataSource = database.RetrieveDataSet("SELECT * FROM ActiveMeasurement");
            publisher.Name = "dataPublisher";
            publisher.UseBaseTimeOffsets = true;
            publisher.AllowPayloadCompression = true;
            publisher.AllowSynchronizedSubscription = true;
            publisher.ConnectionString = "commandChannel={port=9898}";
            //publisher.UseZeroMQChannel = true;
            //publisher.ConnectionString = "commandChannel={server=tcp://*:9898}";
            publisher.Initialize();

            // Start publisher
            publisher.Start();

            ThreadPool.QueueUserWorkItem(ProcessMeasurements);

            Console.ReadLine();

            publisher.Stop();
            //}

            publisher.StatusMessage -= publisher_StatusMessage;
            publisher.ProcessException -= publisher_ProcessException;
        }

        static void publisher_ProcessException(object sender, EventArgs<Exception> e)
        {
            lock (displayLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("EXCEPTION: " + e.Argument.Message);
                Console.ResetColor();
            }
        }

        static void publisher_StatusMessage(object sender, EventArgs<string> e)
        {
            lock (displayLock)
            {
                Console.WriteLine(e.Argument);
            }
        }

        static void publisher_ClientConnected(object sender, EventArgs<Guid, string, string> e)
        {
            ThreadPool.QueueUserWorkItem(ShowConnectedClients);
        }

        static void ShowConnectedClients(object state)
        {
            Ticks displayTime = DateTime.UtcNow.Ticks;

            // Don't show client enumeration more than every two seconds...
            if ((displayTime - lastDisplayTime).ToSeconds() > 2.0D)
            {
                lastDisplayTime = displayTime;
                publisher.EnumerateClients();
            }
        }

        static void ProcessMeasurements(object state)
        {
            while (true)
            {
                List<Measurement> measurements = new List<Measurement>();

                Measurement measurement;

                for (int i = 1; i <= MeasurementCount; i++)
                {
                    measurement = new Measurement
                    {
                        //Key = MeasurementKey.LookUpOrCreate("PPA", (uint)i),
                        MeasurementMetadata = MeasurementKey.LookUpOrCreate("DEVARCHIVE", (uint)i).MeasurementMetadata,
                        Value = Random.Between(-65535.0D, 65536.0D),
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
