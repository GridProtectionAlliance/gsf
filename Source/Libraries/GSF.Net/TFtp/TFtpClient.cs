//******************************************************************************************************
//  TFtpClient.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        https://github.com/Callisto82/tftp.net
//  
//   Copyright © 2011, Michael Baer
//
//*******************************************************************************************************

#endregion

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using GSF.Net.TFtp.Channel;
using GSF.Net.TFtp.Transfer;

// ReSharper disable InconsistentNaming
namespace GSF.Net.TFtp
{
    /// <summary>
    /// A TFTP client that can connect to a TFTP server.
    /// </summary>
    public class TFtpClient
    {
        private const int DefaultServerPort = 69;
        private readonly IPEndPoint m_remoteAddress;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="remoteAddress">Address of the server that you would like to connect to.</param>
        public TFtpClient(IPEndPoint remoteAddress)
        {
            m_remoteAddress = remoteAddress;
        }

        /// <summary>
        /// Connects to a server
        /// </summary>
        /// <param name="ip">Address of the server that you want connect to.</param>
        /// <param name="port">Port on the server that you want connect to (default: 69)</param>
        public TFtpClient(IPAddress ip, int port)
            : this(new IPEndPoint(ip, port)) 
        { 
        }

        /// <summary>
        /// Connects to a server on port 69.
        /// </summary>
        /// <param name="ip">Address of the server that you want connect to.</param>
        public TFtpClient(IPAddress ip)
            : this(new IPEndPoint(ip, DefaultServerPort))
        {
        }

        /// <summary>
        /// Connect to a server by hostname.
        /// </summary>
        /// <param name="host">Hostname or IP to connect to</param>
        public TFtpClient(string host)
            : this(host, DefaultServerPort)
        {
        }

        /// <summary>
        /// Connect to a server by hostname and port .
        /// </summary>
        /// <param name="host">Hostname or IP to connect to</param>
        /// <param name="port">Port to connect to</param>
        public TFtpClient(string host, int port)
        {
            IPAddress ip = Dns.GetHostAddresses(host).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

            if (ip == null)
                throw new ArgumentException($"Could not convert '{host}' to an IPv4 address.", nameof(host));

            m_remoteAddress = new IPEndPoint(ip, port);
        }

        /// <summary>
        /// GET a file from the server.
        /// You have to call Start() on the returned ITFtpTransfer to start the transfer.
        /// </summary>
        public ITFtpTransfer Download(string filename)
        {
            ITransferChannel channel = TransferChannelFactory.CreateConnection(m_remoteAddress);
            return new RemoteReadTransfer(channel, filename);
        }

        /// <summary>
        /// PUT a file from the server.
        /// You have to call Start() on the returned ITFtpTransfer to start the transfer.
        /// </summary>
        public ITFtpTransfer Upload(string filename)
        {
            ITransferChannel channel = TransferChannelFactory.CreateConnection(m_remoteAddress);
            return new RemoteWriteTransfer(channel, filename);
        }
    }
}
