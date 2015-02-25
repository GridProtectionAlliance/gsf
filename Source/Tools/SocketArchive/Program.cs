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
//  06/11/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/19/2012 - J. Ritchie Carroll
//       Added support for UDP and Multicast.
//
//******************************************************************************************************

using System;
using System.IO;
using GSF;
using GSF.Communication;
using GSF.IO;

namespace SocketArchive
{
    class Program
    {
        static IClient socket;
        static FileStream stream;
        static long buffers;

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                ShowHelp();
                return;
            }

            string protocol = args[0].ToUpper();
            string connectionString, fileName;
            string networkInterface = "0.0.0.0";

            switch (protocol)
            {
                case "TCP":
                    if (args.Length > 3)
                        networkInterface = args[3];

                    connectionString = string.Format("protocol=TCP; server={0}; interface={1}", args[1], networkInterface);
                    fileName = FilePath.GetAbsolutePath(args[2]);
                    break;
                case "UDP":
                    if (args.Length > 3)
                        networkInterface = args[3];

                    connectionString = string.Format("protocol=UDP; port={0}; interface={1}", args[1], networkInterface);
                    fileName = FilePath.GetAbsolutePath(args[2]);
                    break;
                case "MULTICAST":
                    if (args.Length < 5)
                    {
                        ShowHelp();
                        return;
                    }

                    if (args.Length > 5)
                        networkInterface = args[5];

                    connectionString = string.Compare(args[3], "ANY", true) == 0 ?
                        string.Format("protocol=UDP; port={0}; server={1}; interface={2}", args[1], args[2], networkInterface) :
                        string.Format("protocol=UDP; port={0}; server={1}; multicastSource={2}; interface={3}", args[1], args[2], args[3], networkInterface);

                    fileName = FilePath.GetAbsolutePath(args[4]);
                    break;
                default:
                    ShowHelp();
                    return;
            }

            Console.WriteLine("\r\nCapturing {0} stream \"{1}\" to file \"{2}\"...\r\n\r\nPress any key to complete capture...\r\n", protocol, connectionString, fileName);

            stream = File.Create(fileName);
            socket = ClientBase.Create(connectionString);

            socket.MaxConnectionAttempts = -1;
            socket.ConnectionAttempt += socket_ConnectionAttempt;
            socket.ConnectionEstablished += socket_ConnectionEstablished;
            socket.ConnectionException += socket_ConnectionException;
            socket.ConnectionTerminated += socket_ConnectionTerminated;
            socket.ReceiveData += socket_ReceiveData;
            socket.ReceiveDataException += socket_ReceiveDataException;

            socket.ConnectAsync();

            Console.ReadKey();

            socket.Dispose();
            socket.ConnectionAttempt -= socket_ConnectionAttempt;
            socket.ConnectionEstablished -= socket_ConnectionEstablished;
            socket.ConnectionException -= socket_ConnectionException;
            socket.ConnectionTerminated -= socket_ConnectionTerminated;
            socket.ReceiveData -= socket_ReceiveData;
            socket.ReceiveDataException -= socket_ReceiveDataException;

            stream.Close();
        }

        static void ShowHelp()
        {
            Console.WriteLine("\r\nSocketArchive Utility - Captures raw socket data to a file.\r\n");
            Console.WriteLine("Copyright 2012, Grid Protection Alliance.\r\n\r\n");
            Console.WriteLine("Usage:\r\n\r\n");
            Console.WriteLine("TCP:\r\n\r\n  SocketArchive TCP IP:Port FileName [Interface]\r\n");
            Console.WriteLine("  SocketArchive TCP 127.0.0.1:6165 Capture.bin");
            Console.WriteLine("  SocketArchive TCP ::1:6165 Capture.bin ::0\r\n");
            Console.WriteLine("UDP:\r\n\r\n  SocketArchive UDP Port FileName [Interface]\r\n");
            Console.WriteLine("  SocketArchive UDP 5000 Capture.bin");
            Console.WriteLine("  SocketArchive UDP 9168 Capture.bin 172.21.1.109\r\n");
            Console.WriteLine("Multicast:\r\n\r\n  SocketArchive MULTICAST LocalPort IP:RemotePort SourceIP FileName [Interface]\r\n");
            Console.WriteLine("  SocketArchive MULTICAST 5000 233.124.124.124:5000 ANY Capture.bin");
            Console.WriteLine("  SocketArchive MULTICAST 102 232.123.123.123:102 172.21.1.102 Capture.bin");
            Console.WriteLine("  SocketArchive MULTICAST 6015 host.dns.org:997 ANY Capture.bin ::0\r\n");
        }

        private static void socket_ReceiveData(object sender, EventArgs<int> e)
        {
            int length = e.Argument;
            byte[] buffer = new byte[length];

            length = socket.Read(buffer, 0, length);
            stream.Write(buffer, 0, length);
            buffers++;

            if (buffers % 120 == 0)
                Console.WriteLine("{0} buffers received...", buffers);
        }

        static void socket_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Console.Error.WriteLine("Receive exception: " + e.Argument.Message);
        }

        static void socket_ConnectionTerminated(object sender, EventArgs e)
        {
            Console.WriteLine("Connection terminated.");
        }

        static void socket_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Console.Error.WriteLine("Connection exception: " + e.Argument.Message);
        }

        static void socket_ConnectionEstablished(object sender, EventArgs e)
        {
            Console.WriteLine("Connection established.\r\n");
        }

        static void socket_ConnectionAttempt(object sender, EventArgs e)
        {
            Console.WriteLine("Connection attempt...");
        }
    }
}
