//******************************************************************************************************
//  UdpChannel.cs - Gbtc
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
using System.Net.Sockets;
using System.Net;
using System.IO;
using GSF.Net.TFtp.Commands;

namespace GSF.Net.TFtp.Channel
{
    internal class UdpChannel : ITransferChannel
    {
        public event TFtpCommandHandler OnCommandReceived;
        public event TFtpChannelErrorHandler OnError;

        private IPEndPoint m_endpoint;
        private UdpClient m_client;
        private readonly CommandSerializer m_serializer = new CommandSerializer();
        private readonly CommandParser m_parser = new CommandParser();

        public UdpChannel(UdpClient client)
        {
            m_client = client;
            m_endpoint = null;
        }

        public void Open()
        {
            if (m_client == null)
                throw new ObjectDisposedException(nameof(UdpChannel));

            m_client.BeginReceive(UdpReceivedCallback, null);
        }

        private void UdpReceivedCallback(IAsyncResult result)
        {
            IPEndPoint endpoint = new IPEndPoint(0, 0);
            ITFtpCommand command = null;

            try
            {
                byte[] data;

                lock (this)
                {
                    if (m_client == null)
                        return;

                    data = m_client.EndReceive(result, ref endpoint);
                }

                command = m_parser.Parse(data);
            }
            catch (SocketException e)
            {
                // Handle receive error
                RaiseOnError(new NetworkError(e));
            }
            catch (TFtpParserException e2)
            {
                // Handle parser error
                RaiseOnError(new NetworkError(e2));
            }

            if (command != null)
                RaiseOnCommand(command, endpoint);

            lock (this)
            {
                m_client?.BeginReceive(UdpReceivedCallback, null);
            }
        }

        private void RaiseOnCommand(ITFtpCommand command, IPEndPoint endpoint)
        {
            OnCommandReceived?.Invoke(command, endpoint);
        }

        private void RaiseOnError(TFtpTransferError error)
        {
            OnError?.Invoke(error);
        }

        public void Send(ITFtpCommand command)
        {
            if (m_client == null)
                throw new ObjectDisposedException(nameof(UdpChannel));

            if (m_endpoint == null)
                throw new InvalidOperationException("RemoteEndpoint needs to be set before you can send TFTP commands.");

            using (MemoryStream stream = new MemoryStream())
            {
                m_serializer.Serialize(command, stream);
                byte[] data = stream.GetBuffer();
                m_client.Send(data, (int)stream.Length, m_endpoint);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (m_client == null)
                    return;

                m_client.Close();
                m_client = null;
            }
        }

        public EndPoint RemoteEndpoint
        {
            get => m_endpoint;

            set
            {
                if (m_client == null)
                    throw new ObjectDisposedException(nameof(UdpChannel));

                m_endpoint = value as IPEndPoint ?? throw new NotSupportedException("UdpChannel can only connect to IPEndPoints.");
            }
        }
    }
}
