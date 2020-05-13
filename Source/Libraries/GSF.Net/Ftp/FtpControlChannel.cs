//******************************************************************************************************
//  FtpControlChannel.cs - Gbtc
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
//  05/22/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        http://www.codeproject.com/KB/IP/net_ftp_upload.aspx
//  
//   Copyright Alex Kwok & Uwe Keim 
//
//   The Code Project Open License (CPOL):
//        http://www.codeproject.com/info/cpol10.aspx
//
//*******************************************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Random = GSF.Security.Cryptography.Random;

namespace GSF.Net.Ftp
{
    #region [ Enumerations ]

    /// <summary>
    /// FTP transfer mode enumeration.
    /// </summary>
    public enum TransferMode
    {
        /// <summary>
        /// Transfer files in ASCII mode.
        /// </summary>
        Ascii,
        /// <summary>
        /// Transfer files in binary mode.
        /// </summary>
        Binary,
        /// <summary>
        /// File transfer mode is undetermined.
        /// </summary>
        Unknown
    }

    #endregion

    /// <summary>
    /// FTP control channel.
    /// </summary>
    public class FtpControlChannel : IDisposable
    {
        #region [ Members ]

        // Fields
        private readonly FtpClient m_sessionHost;
        private TcpClient m_connection;
        private string m_server;
        private IPAddress m_activeAddress;
        private Range<int> m_activePortRange;
        private int m_lastActivePort;
        private TransferMode m_currentTransferMode;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        internal FtpControlChannel(FtpClient host)
        {
            m_connection = new TcpClient();
            m_server = "localhost";
            Port = 21;
            Timeout = 30000;
            Passive = true;
            m_sessionHost = host;
            m_currentTransferMode = TransferMode.Unknown;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FtpControlChannel"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FtpControlChannel()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        internal string Server
        {
            get => m_server;
            set
            {
                if (value.Length == 0)
                    throw new ArgumentNullException(nameof(value), "Server property must not be blank.");

                m_server = value;
            }
        }

        internal int Port { get; set; }

        internal int Timeout { get; set; }

        internal bool Passive { get; set; }

        internal IPAddress ActiveAddress
        {
            get => m_activeAddress ?? (m_activeAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork));
            set => m_activeAddress = value;
        }

        internal Range<int> DataChannelPortRange
        {
            get => m_activePortRange;
            set
            {
                if (value.Start <= IPEndPoint.MinPort)
                    throw new ArgumentException($"Minimum port in range must be greater than {IPEndPoint.MinPort}.", nameof(value));

                if (value.End > IPEndPoint.MaxPort)
                    throw new ArgumentException($"Maximum port in range must be less than or equal to {IPEndPoint.MaxPort}.", nameof(value));

                if (value.Start > value.End)
                    throw new ArgumentException("Start port must be less than or equal to end port.");

                m_activePortRange = value;
            }
        }

        /// <summary>
        /// Last response from control channel.
        /// </summary>
        public FtpResponse LastResponse { get; private set; }

        internal FtpSessionConnected Session { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="FtpControlChannel"/> object.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="FtpControlChannel"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FtpControlChannel"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                LastResponse = null;

                m_connection?.Close();
                m_connection = null;
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Connects the <see cref="FtpControlChannel"/>.
        /// </summary>
        public void Connect()
        {
            if (m_connection == null)
                return;

            m_connection.Connect(m_server, Port);

            try
            {
                NetworkStream stream = m_connection.GetStream();

                stream.ReadTimeout = Timeout;
                stream.WriteTimeout = Timeout;
                LastResponse = new FtpResponse(stream);

                if (LastResponse.Code != FtpResponse.ServiceReady)
                    throw new FtpServerDownException("FTP service unavailable.", LastResponse);
            }
            catch
            {
                Close();
                throw;
            }
        }

        /// <summary>
        /// Send FTP command to control channel.
        /// </summary>
        /// <param name="cmd">A <see cref="String"/> representing the command to send.</param>
        public void Command(string cmd)
        {
            if (m_connection == null)
                return;

            // If the client receives an exception when establishing a data channel,
            // it may not have checked for the server's response when handling the exception
            FlushResponses();

            byte[] buff = Encoding.Default.GetBytes(cmd + Environment.NewLine);
            NetworkStream stream = m_connection.GetStream();
            
            m_sessionHost.OnCommandSent(cmd);
            stream.Write(buff, 0, buff.Length);
            
            RefreshResponse();
        }

        /// <summary>
        /// Refresh response from control channel.
        /// </summary>
        public void RefreshResponse()
        {
            if (m_connection == null)
                return;

            lock (this)
                LastResponse = new FtpResponse(m_connection.GetStream());

            foreach (string s in LastResponse.Responses)
                m_sessionHost.OnResponseReceived(s);
        }

        /// <summary>
        /// Flushes data from the control channel to get ready for the next response.
        /// </summary>
        public void FlushResponses()
        {
            StringBuilder responses = new StringBuilder();

            lock (this)
            {
                NetworkStream stream = m_connection.GetStream();

                while (stream.DataAvailable)
                {
                    int b = stream.ReadByte();

                    if (b < 0)
                        break;

                    char c = Encoding.ASCII.GetChars(new[] { (byte)b })[0];
                    responses.Append(c);
                }
            }

            List<string> lines = responses
                .ToString()
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => $"{line}\n")
                .ToList();

            foreach (string line in lines)
                m_sessionHost.OnResponseReceived(line);
        }

        internal void REST(long offset)
        {
            Command($"REST {offset}");

            if (LastResponse.Code != FtpResponse.RequestFileActionPending)
                throw new FtpResumeNotSupportedException(LastResponse);
        }

        internal void STOR(string name)
        {
            Type(TransferMode.Binary);

            Command($"STOR {name}");

            if (LastResponse.Code != FtpResponse.DataChannelOpenedTransferStart && LastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                throw new FtpCommandException($"Failed to send file {name}.", LastResponse);
        }

        internal void RETR(string name)
        {
            Type(TransferMode.Binary);

            Command($"RETR {name}");

            if (LastResponse.Code != FtpResponse.DataChannelOpenedTransferStart && LastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                throw new FtpCommandException($"Failed to retrieve file {name}.", LastResponse);
        }

        internal void DELE(string fileName)
        {
            Command($"DELE {fileName}");

            if (LastResponse.Code != FtpResponse.RequestFileActionComplete) // 250)
                throw new FtpCommandException($"Failed to delete file {fileName}.", LastResponse);
        }

        internal void RMD(string dirName)
        {
            Command($"RMD {dirName}");

            if (LastResponse.Code != FtpResponse.RequestFileActionComplete) // 250)
                throw new FtpCommandException($"Failed to remove subdirectory {dirName}.", LastResponse);
        }

        internal string PWD()
        {
            Command("PWD");

            if (LastResponse.Code != 257)
                throw new FtpCommandException("Cannot get current directory.", LastResponse);

            Match m = s_pwdExpression.Match(LastResponse.Message);
            return m.Groups[2].Value;
        }

        internal void CDUP()
        {
            Command("CDUP");

            if (LastResponse.Code != FtpResponse.RequestFileActionComplete)
                throw new FtpCommandException("Cannot move to parent directory (CDUP).", LastResponse);
        }

        internal void CWD(string path)
        {
            Command($"CWD {path}");

            if (LastResponse.Code != FtpResponse.RequestFileActionComplete && LastResponse.Code != FtpResponse.ClosingDataChannel)
                throw new FtpCommandException($"Cannot change directory to {path}.", LastResponse);
        }

        internal void QUIT()
        {
            Command("QUIT");
        }

        internal void Type(TransferMode mode)
        {
            switch (mode)
            {
                case TransferMode.Unknown:
                    return;

                case TransferMode.Ascii when m_currentTransferMode != TransferMode.Ascii:
                    Command("TYPE A");
                    break;

                case TransferMode.Binary when m_currentTransferMode != TransferMode.Binary:
                    Command("TYPE I");
                    break;
            }

            m_currentTransferMode = mode;
        }

        internal void Rename(string oldName, string newName)
        {
            Command($"RNFR {oldName}");

            if (LastResponse.Code != FtpResponse.RequestFileActionPending)
                throw new FtpCommandException($"Failed to rename file from {oldName} to {newName}.", LastResponse);

            Command($"RNTO {newName}");
            
            if (LastResponse.Code != FtpResponse.RequestFileActionComplete)
                throw new FtpCommandException($"Failed to rename file from {oldName} to {newName}.", LastResponse);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "StreamReader leaves stream open so that outer using can close")]
        internal Queue List(bool passive)
        {
            const string errorMsgListing = "Error when listing server directory.";

            try
            {
                Type(TransferMode.Ascii);
                
                Queue lineQueue = new Queue();

                using (FtpDataStream dataStream = GetDataStream())
                using (StreamReader lineReader = new StreamReader(dataStream, Encoding.Default, true, 1024, true))
                {
                    Command("LIST");

                    if (LastResponse.Code != FtpResponse.DataChannelOpenedTransferStart && LastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                    {
                        dataStream.Close(true);
                        throw new FtpCommandException(errorMsgListing, LastResponse);
                    }

                    string line = lineReader.ReadLine();

                    while (line != null)
                    {
                        lineQueue.Enqueue(line);
                        line = lineReader.ReadLine();
                    }
                }

                if (LastResponse.Code != FtpResponse.ClosingDataChannel)
                    throw new FtpCommandException(errorMsgListing, LastResponse);

                return lineQueue;
            }
            catch (IOException ie)
            {
                throw new Exception(errorMsgListing, ie);
            }
            catch (SocketException se)
            {
                throw new Exception(errorMsgListing, se);
            }
        }

        internal FtpDataStream GetDataStream()
        {
            return GetDataStream(TransferDirection.Download);
        }

        internal FtpDataStream GetDataStream(TransferDirection direction)
        {
            return Passive ? GetPassiveDataStream(direction) : GetActiveDataStream(direction);
        }

        internal FtpDataStream GetActiveDataStream()
        {
            return GetActiveDataStream(TransferDirection.Download);
        }

        internal FtpDataStream GetActiveDataStream(TransferDirection direction)
        {
            IPAddress address = ActiveAddress;
            byte[] addressBytes = address.GetAddressBytes();

            int firstPort = GetActivePort();
            int port = firstPort;

            while (true)
            {
                try
                {
                    TcpListener listener = new TcpListener(address, port);

                    listener.Start();

                    try
                    {
                        port = (listener.LocalEndpoint as IPEndPoint)?.Port ?? 0;
                        Command($"PORT {addressBytes[0]},{addressBytes[1]},{addressBytes[2]},{addressBytes[3]},{port / 256},{port % 256}");

                        for (int i = 0; !listener.Pending(); i++)
                        {
                            if (i * 200 >= Timeout)
                                throw new TimeoutException("Timeout expired while waiting for connection on active FTP data channel.");

                            Thread.Sleep(200);
                        }

                        TcpClient client = listener.AcceptTcpClient();

                        if (direction == TransferDirection.Download)
                            return new FtpInputDataStream(this, client);
                        else
                            return new FtpOutputDataStream(this, client);
                    }
                    finally
                    {
                        listener.Stop();
                    }
                }
                catch (IOException ie)
                {
                    throw new Exception($"Failed to open active port ({port}) data connection due to IO exception: {ie.Message}.", ie);
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode != SocketError.AddressAlreadyInUse)
                        throw new Exception($"Failed to open active port ({port}) data connection due to socket exception: {se.Message}.", se);
                }

                port = GetActivePort();

                if (port == firstPort)
                    throw new Exception($"All ports in active port range ({m_activePortRange.Start}-{m_activePortRange.End}) are already in use.");
            }
        }

        internal FtpDataStream GetPassiveDataStream()
        {
            return GetPassiveDataStream(TransferDirection.Download);
        }

        internal FtpDataStream GetPassiveDataStream(TransferDirection direction)
        {
            TcpClient client = new TcpClient();
            int port = 0;

            try
            {
                port = GetPassivePort();
                client.Connect(m_server, port);

                if (direction == TransferDirection.Download)
                    return new FtpInputDataStream(this, client);
                
                return new FtpOutputDataStream(this, client);
            }
            catch (IOException ie)
            {
                throw new Exception($"Failed to open passive port ({port}) data connection due to IO exception: {ie.Message}.", ie);
            }
            catch (SocketException se)
            {
                throw new Exception($"Failed to open passive port ({port}) data connection due to socket exception: {se.Message}.", se);
            }
        }

        private int GetActivePort()
        {
            if (m_activePortRange == null)
                return 0;

            if (m_lastActivePort == 0)
                m_lastActivePort = Random.Int32Between(m_activePortRange.Start, m_activePortRange.End);
            else if (m_lastActivePort == m_activePortRange.End)
                m_lastActivePort = m_activePortRange.Start;
            else
                m_lastActivePort++;

            return m_lastActivePort;
        }

        private int GetPassivePort()
        {
            Command("PASV");

            if (LastResponse.Code != FtpResponse.EnterPassiveMode)
                throw new FtpCommandException($"Failed to enter passive mode. - {LastResponse.Code} - {LastResponse.Message}", LastResponse);

            string[] numbers = s_regularExpression.Match(LastResponse.Message).Groups[2].Value.Split(',');
            
            return int.Parse(numbers[4]) * 256 + int.Parse(numbers[5]);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_regularExpression = new Regex("(\\()(.*)(\\))");
        private static readonly Regex s_pwdExpression = new Regex("(\")(.*)(\")");

        #endregion
    }
}