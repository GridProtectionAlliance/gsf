//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/03/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.PQDIF.Physical;

namespace PQDIFDump
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName;
            PhysicalParser parser;
            Record record;

            if (args.Length < 1)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("    PQDIFDump FILENAME");
                Environment.Exit(0);
            }

            fileName = args[0];
            parser = new PhysicalParser(fileName);
            parser.Open();

            while (parser.HasNextRecord())
            {
                record = parser.NextRecord();
                Console.WriteLine(record);
                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}
