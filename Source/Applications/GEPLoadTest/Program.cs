//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  05/13/2020 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Timers;
using GSF;
using GSF.Configuration;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
using Random = GSF.Security.Cryptography.Random;

namespace GEPLoadTest
{
    class Program
    {
        // Define subscription modes for testing
        
        static readonly UnsynchronizedSubscriptionInfo unsynchronizedInfo = new UnsynchronizedSubscriptionInfo(false);

        static readonly DataSubscriber subscriber = new DataSubscriber();
        static long dataCount;
        static readonly object displayLock = new object();

        static int nSubscribed = 0;

        static void Main(string[] args)
        {
            // Attach to subscriber events
            subscriber.StatusMessage += subscriber_StatusMessage;
            subscriber.ProcessException += subscriber_ProcessException;
            subscriber.ConnectionEstablished += subscriber_ConnectionEstablished;
            subscriber.ConnectionTerminated += subscriber_ConnectionTerminated;
            subscriber.NewMeasurements += subscriber_NewMeasurements;
            subscriber.MetaDataReceived += subscriber_MetaDataReceived;

            nSubscribed = 0;


            // Initialize subscriber
            //subscriber.ConnectionString = "server=tcp://127.0.0.1:9898; useZeroMQChannel=true";

            // Access needed settings from specified categories in configuration file
            CategorizedSettingsElementCollection reportSettings = ConfigurationFile.Current.Settings["systemSettings"];
            reportSettings.Add("server", "localhost:6165", "Server and Port to Load Test");
            string server = reportSettings["server"].ValueAs("PPA");

            subscriber.ConnectionString = $"server={server}";
            subscriber.OperationalModes |= OperationalModes.UseCommonSerializationFormat | OperationalModes.CompressMetadata | OperationalModes.CompressSignalIndexCache | OperationalModes.CompressPayloadData;
            //subscriber.CompressionModes = CompressionModes.TSSC | CompressionModes.GZip;
            subscriber.Initialize();

            // Start subscriber connection cycle
            subscriber.Start();

            Console.ReadLine();

            subscriber.Unsubscribe();

            subscriber.StatusMessage -= subscriber_StatusMessage;
            subscriber.ProcessException -= subscriber_ProcessException;
            subscriber.ConnectionEstablished -= subscriber_ConnectionEstablished;
            subscriber.ConnectionTerminated -= subscriber_ConnectionTerminated;
            subscriber.NewMeasurements -= subscriber_NewMeasurements;


        }

        static void subscriber_MetaDataReceived(object sender, EventArgs<System.Data.DataSet> e)
        {
            DataSet dataSet = e.Argument;
            dataSet.WriteXml(FilePath.GetAbsolutePath("Metadata.xml"), XmlWriteMode.WriteSchema);
            Console.WriteLine("Data set serialized with {0} tables...", dataSet.Tables.Count);
            DataTable measurementTbl = e.Argument.Tables["MeasurementDetail"];
            int nMeas = measurementTbl.Rows.Count;

            if (nMeas != nSubscribed)
            {
                StringBuilder connectionString = new StringBuilder();

                // Define connection string
                foreach (DataRow row in measurementTbl.Rows)
                {
                    connectionString.AppendFormat("{0};", row["ID"]);
                }
               


                Console.WriteLine("Attempt to Subscribe to {0} Points", nMeas);
                unsynchronizedInfo.FilterExpression = connectionString.ToString();
                subscriber.UnsynchronizedSubscribe(unsynchronizedInfo);
                nSubscribed = nMeas;
            }

            


        }

        static void subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            // Check to see if total number of added points will exceed process interval used to show periodic
            // messages of how many points have been archived so far...
            int interval = 30*nSubscribed;

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

            subscriber.SendServerCommand(ServerCommand.MetaDataRefresh);

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
