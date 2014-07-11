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
//  05/21/2014 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF;
using GSF.Communication;

namespace LibraryTester
{
    class Program
    {
        //private enum GetFormattedNameTest
        //{
        //    ABClittleXYZ,
        //    ABClittleXYZo,
        //    AssociatingMeasurementsToConnectionPoints,
        //    CollatingMeasurementsIntoPointValues,
        //    ArchivingPointValuesToXYServer
        //}

        static void Main(string[] args)
        {
            // Add references for projects as needed, then add a simple call so that immediate window
            // will have access to assembly. Only a single call per assembly is needed.

            Common.IsDefaultValue(true);            // Call to load GSF.Core
            Transport.GetDefaultIPStack();          // Call to load GSF.Communications

            //foreach (GetFormattedNameTest stage in Enum.GetValues(typeof(GetFormattedNameTest)).Cast<GetFormattedNameTest>())
            //{
            //    Console.WriteLine(stage.GetFormattedName());
            //}

            //Console.WriteLine();

            Console.WriteLine("Library Testing Host Application");
            Console.WriteLine();
            Console.WriteLine("This application simply references GSF libraries so that when set as the");
            Console.WriteLine("\"StartUp Project\" in the solution you can use the \"Immediate Window\" to");
            Console.WriteLine("test a function in the referenced libraries.");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
