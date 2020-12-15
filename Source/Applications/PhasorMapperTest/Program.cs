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
//  12/14/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using GSF;
using GSF.TimeSeries;
using PhasorProtocolAdapters;

namespace PhasorMapperTest
{
    internal class Program
    {
        private static readonly object s_displayLock = new();
        private static DateTime s_lastMessageTime;
        private static long s_dataCount;

        private static void Main(string[] args)
        {
            const string DeviceName = "SHELBY";
            const string C37TcpConnection = "TransportProtocol=TCP; PhasorProtocol=IEEEC37_118V1";

            DataSet dataSource = LoadDataSource();
            DataRow[] results = dataSource.Tables["InputAdapters"].Select($"AdapterName = '{DeviceName}'");
            uint adapterID = results.Length > 0 && uint.TryParse(results[0]["ID"].ToString(), out uint id) ? id : 0;

            using PhasorMeasurementMapper mapper = new()
            {
                Name = DeviceName,
                ID = adapterID,
                DataSource = dataSource,
                ConnectionString = $"{C37TcpConnection}; AccessID=236; Server=127.0.0.1:8900/235, 192.168.1.117:8900"
            };

            mapper.StatusMessage += MapperStatusMessage;
            mapper.ProcessException += MapperProcessException;
            mapper.NewMeasurements += MapperNewMeasurements;

            mapper.Initialize();
            mapper.Start();

            Console.ReadKey();
        }

        private static DataSet LoadDataSource()
        {
            DataSet dataSource = new();
            dataSource.ReadXml("SystemConfiguration.xml");

            foreach (DataRow measurement in dataSource.Tables["ActiveMeasurements"].Rows)
                MeasurementKey.CreateOrUpdate(measurement["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), measurement["ID"].ToString());

            return dataSource;
        }

        private static void MapperNewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            Interlocked.Add(ref s_dataCount, e.Argument.Count);

            if ((DateTime.UtcNow - s_lastMessageTime).TotalSeconds < 5.0D)
                return;

            s_lastMessageTime = DateTime.UtcNow;

            lock (s_displayLock)
                Console.WriteLine($"{Interlocked.Read(ref s_dataCount):N0} measurements have been processed so far...{Environment.NewLine}");
        }

        private static void MapperProcessException(object sender, EventArgs<Exception> e)
        {
            lock (s_displayLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"EXCEPTION: {e.Argument.Message}{Environment.NewLine}");
                Console.ResetColor();
            }
        }

        private static void MapperStatusMessage(object sender, EventArgs<string> e)
        {
            lock (s_displayLock)
                Console.WriteLine($"{e.Argument}{Environment.NewLine}");
        }
    }
}
