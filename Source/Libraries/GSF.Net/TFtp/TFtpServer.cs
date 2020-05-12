//******************************************************************************************************
//  TFtpServer.cs - Gbtc
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
using System.Net;
using GSF.Net.TFtp.Channel;
using GSF.Net.TFtp.Commands;
using GSF.Net.TFtp.Transfer;

// ReSharper disable InconsistentNaming
namespace GSF.Net.TFtp
{
    public delegate void TFtpServerEventHandler(ITFtpTransfer transfer, EndPoint client);
    public delegate void TFtpServerErrorHandler(TFtpTransferError error);

    /// <summary>
    /// A simple TFTP server class. <code>Dispose()</code> the server to close the socket that it listens on.
    /// </summary>
    public class TFtpServer : IDisposable
    {
        public const int DefaultServerPort = 69;

        /// <summary>
        /// Fired when the server receives a new read request.
        /// </summary>
        public event TFtpServerEventHandler OnReadRequest;

        /// <summary>
        /// Fired when the server receives a new write request.
        /// </summary>
        public event TFtpServerEventHandler OnWriteRequest;

        /// <summary>
        /// Fired when the server encounters an error (for example, a non-parseable request)
        /// </summary>
        public event TFtpServerErrorHandler OnError;

        private readonly ITransferChannel m_serverSocket; // Server port that we're listening on.
        private bool m_disposed;

        public TFtpServer(IPEndPoint localAddress)
        {
            if (localAddress == null)
                throw new ArgumentNullException(nameof(localAddress));

            m_serverSocket = TransferChannelFactory.CreateServer(localAddress);
            m_serverSocket.OnCommandReceived += ServerSocket_OnCommandReceived;
            m_serverSocket.OnError += ServerSocket_OnError;
        }

        public TFtpServer(IPAddress localAddress)
            : this(localAddress, DefaultServerPort)
        {
        }

        public TFtpServer(IPAddress localAddress, int port)
            : this(new IPEndPoint(localAddress, port))
        {
        }

        public TFtpServer(int port)
            : this(new IPEndPoint(IPAddress.Any, port))
        {
        }

        public TFtpServer()
            : this(DefaultServerPort)
        {
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="TFtpServer"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TFtpServer"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (disposing)
                {
                    m_serverSocket?.Dispose();
                }
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Start accepting incoming connections.
        /// </summary>
        public void Start()
        {
            m_serverSocket.Open();
        }

        private void ServerSocket_OnError(TFtpTransferError error)
        {
            RaiseOnError(error);
        }

        private void ServerSocket_OnCommandReceived(ITFtpCommand command, EndPoint endpoint)
        {
            // Ignore all other commands
            if (!(command is ReadOrWriteRequest))
                return;

            // Open a connection to the client
            ITransferChannel channel = TransferChannelFactory.CreateConnection(endpoint);

            // Create a wrapper for the transfer request
            ReadOrWriteRequest request = (ReadOrWriteRequest)command;
            ITFtpTransfer transfer = request is ReadRequest ? (ITFtpTransfer)new LocalReadTransfer(channel, request.Filename, request.Options) : new LocalWriteTransfer(channel, request.Filename, request.Options);

            switch (command)
            {
                case ReadRequest _:
                    RaiseOnReadRequest(transfer, endpoint);
                    break;

                case WriteRequest _:
                    RaiseOnWriteRequest(transfer, endpoint);
                    break;

                default:
                    throw new Exception($"Unexpected TFTP transfer request: {command}");
            }
        }

        private void RaiseOnError(TFtpTransferError error)
        {
            OnError?.Invoke(error);
        }

        private void RaiseOnWriteRequest(ITFtpTransfer transfer, EndPoint client)
        {
            if (OnWriteRequest != null)
                OnWriteRequest(transfer, client);
            else
                transfer.Cancel(new TFtpErrorPacket(0, "Server did not provide a OnWriteRequest handler."));
        }

        private void RaiseOnReadRequest(ITFtpTransfer transfer, EndPoint client)
        {
            if (OnReadRequest != null)
                OnReadRequest(transfer, client);
            else
                transfer.Cancel(new TFtpErrorPacket(0, "Server did not provide a OnReadRequest handler."));
        }
    }
}