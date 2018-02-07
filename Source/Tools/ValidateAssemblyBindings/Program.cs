//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/07/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using GSF.IO;
using GSF.Web;

namespace ValidateAssemblyBindings
{
    // NOTE: The build output folder for this application is intentionally the "Libraries" location
    // so that the tool will be available for GSF.Web dependent downstream applications as part of
    // the nightly build roll-down process.
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("USAGE: ValidateAssemblyBindings <appConfigFileName>");
                Console.WriteLine();
                Console.WriteLine("    Example: ValidateAssemblyBindings openPDC.exe.config");
                Console.WriteLine();
                return 1;
            }

            try
            {
                string appConfigFileName = FilePath.GetAbsolutePath(args[0]);

                if (!File.Exists(appConfigFileName))
                    throw new FileNotFoundException($"Application configuration file name \"{appConfigFileName}\" not found.");

                Console.WriteLine($"Updating assembly bindings for \"{appConfigFileName}\"");
                bool result = WebExtensions.ValidateAssemblyBindings(appConfigFileName);
                Console.WriteLine($"Assembly bindings update {(result ? "succeeded" : "failed")}.");

                return result ? 0 : 3;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to validate assembly bindings: {ex.Message}");
                Console.WriteLine();
                return 2;
            }
        }
    }
}