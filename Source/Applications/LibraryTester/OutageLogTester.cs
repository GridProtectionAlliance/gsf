//******************************************************************************************************
//  OutageLogTester.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/27/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.IO;
using System;

namespace LibraryTester
{
    class OutageLogTester
    {
        static void Main(string[] args)
        {
            string line;

            Console.WriteLine("Type HELP for a list of options.");
            Console.WriteLine();

            using (OutageLog log = new OutageLog())
            {
                log.FileName = @"C:\Users\swills\test.txt";
                log.LogModified += (sender, arg) => Console.WriteLine("Modified!");
                log.Initialize();

                while (!(line = Console.ReadLine()).Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                {
                    switch (line.ToUpper())
                    {
                        case "ADD":
                            log.Add(DateTimeOffset.UtcNow.AddSeconds(-1.0D), DateTimeOffset.UtcNow);
                            Console.WriteLine($"Count: {log.Count}");
                            break;

                        case "REMOVE":
                            log.Remove(log.First());
                            Console.WriteLine($"Count: {log.Count}");
                            break;

                        case "DUMP":
                            foreach (Outage outage in log.Outages)
                                Console.WriteLine($"{outage.Start:yyyy-MM-dd HH:mm:ss.fff};{outage.End:yyyy-MM-dd HH:mm:ss.fff}");

                            Console.WriteLine();
                            break;

                        case "STATUS":
                            Console.WriteLine(log.Status);
                            Console.WriteLine();
                            break;

                        case "HELP":
                            Console.WriteLine("ADD    - Adds a new outage to the log");
                            Console.WriteLine("REMOVE - Removes the first outage from the log");
                            Console.WriteLine("DUMP   - Displays the contents of the log");
                            Console.WriteLine("STATUS - Displays detailed status of the log");
                            Console.WriteLine("EXIT   - Exits this application");
                            Console.WriteLine();
                            break;
                    }
                }
            }
        }
    }
}
