//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  01/26/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using GSF.Console;

namespace MonoGenCert
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("USAGE: mono MonoGenCert <certName>");
                Console.WriteLine();
                Console.WriteLine("    Example: MonoGenCert openPDC");
                Console.WriteLine();
                return 1;
            }

            try
            {
                string issuer = args[0];
                string commonNames = GetCommonNameList(issuer);

                // Generate private key file and p12 file (i.e., pub and private in one file)
                int result = Command.Execute("makecert", string.Format("-r -n \"{0}\" -p12 {1}.p12 \"\" -sv {1}.pvk {1}.cer", commonNames, issuer)).ExitCode;

                // Generate public key certificate file
                if (result == 0)
                    result = Command.Execute("makecert", string.Format("-r -n \"{0}\" -sv {1}.pvk {1}.cer", commonNames, issuer)).ExitCode;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to generate certificate: " + ex.Message);
                return 2;
            }
        }

        // Gets the list of common names to be passed to
        // Mono's makecert when generating self-signed certificates.
        private static string GetCommonNameList(string issuer)
        {
            return string.Join(",", new[] { issuer }.Concat(GetSubjectNames()).Distinct().Select(name => "CN=" + name));
        }

        // Gets the default value for the subject names.
        // This uses a DNS lookup to determine the host name of the system and
        // all the possible IP addresses and aliases that the system may go by.
        private static string[] GetSubjectNames()
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

                return hostEntry.AddressList
                    .Select(address => address.ToString())
                    .Concat(hostEntry.Aliases)
                    .Concat(new[] { Environment.MachineName, hostEntry.HostName })
                    .ToArray();
            }
            catch (SocketException)
            {
                // If DNS operations fails, at least return machine name
                return new[] { Environment.MachineName };
            }
        }
    }
}
