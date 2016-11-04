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
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

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
        private FtpSessionConnected m_session;
        private TcpClient m_connection;
        private string m_server;
        private int m_port;
        private int m_timeout;
        private TransferMode m_currentTransferMode;
        private FtpResponse m_lastResponse;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        internal FtpControlChannel(FtpClient host)
        {
            m_connection = new TcpClient();
            m_server = "localhost";
            m_port = 21;
            m_timeout = 30000;
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
            get
            {
                return m_server;
            }
            set
            {
                if (value.Length == 0)
                    throw new ArgumentNullException(nameof(value), "Server property must not be blank.");

                m_server = value;
            }
        }

        internal int Port
        {
            get
            {
                return m_port;
            }
            set
            {
                m_port = value;
            }
        }

        internal int Timeout
        {
            get
            {
                return m_timeout;
            }
            set
            {
                m_timeout = value;
            }
        }

        /// <summary>
        /// Last response from control channel.
        /// </summary>
        public FtpResponse LastResponse
        {
            get
            {
                return m_lastResponse;
            }
        }

        internal FtpSessionConnected Session
        {
            get
            {
                return m_session;
            }
            set
            {
                m_session = value;
            }
        }

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
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
                        m_lastResponse = null;

                        if ((object)m_connection != null)
                            m_connection.Close();

                        m_connection = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Connects the <see cref="FtpControlChannel"/>.
        /// </summary>
        public void Connect()
        {
            if ((object)m_connection == null)
                return;

            m_connection.Connect(m_server, m_port);

            try
            {
                NetworkStream stream = m_connection.GetStream();

                stream.ReadTimeout = m_timeout;
                stream.WriteTimeout = m_timeout;
                m_lastResponse = new FtpResponse(stream);

                if (m_lastResponse.Code != FtpResponse.ServiceReady)
                    throw new FtpServerDownException("FTP service unavailable.", m_lastResponse);
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
            if ((object)m_connection == null)
                return;
            
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
            if ((object)m_connection == null)
                return;

            lock (this)
            {
                m_lastResponse = new FtpResponse(m_connection.GetStream());
            }

            foreach (string s in m_lastResponse.Respones)
            {
                m_sessionHost.OnResponseReceived(s);
            }
        }

        internal void REST(long offset)
        {
            Command("REST " + offset);

            if (m_lastResponse.Code != FtpResponse.RequestFileActionPending)
                throw new FtpResumeNotSupportedException(m_lastResponse);
        }

        internal void STOR(string name)
        {
            Type(TransferMode.Binary);
            Command("STOR " + name);

            if (m_lastResponse.Code != FtpResponse.DataChannelOpenedTransferStart && m_lastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                throw new FtpCommandException("Failed to send file " + name + ".", m_lastResponse);
        }

        internal void RETR(string name)
        {
            Type(TransferMode.Binary);
            Command("RETR " + name);

            if (m_lastResponse.Code != FtpResponse.DataChannelOpenedTransferStart && m_lastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                throw new FtpCommandException("Failed to retrieve file " + name + ".", m_lastResponse);
        }

        internal void DELE(string fileName)
        {
            Command("DELE " + fileName);

            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete) // 250)
                throw new FtpCommandException("Failed to delete file " + fileName + ".", m_lastResponse);
        }

        internal void RMD(string dirName)
        {
            Command("RMD " + dirName);

            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete) // 250)
                throw new FtpCommandException("Failed to remove subdirectory " + dirName + ".", m_lastResponse);
        }

        internal string PWD()
        {
            Command("PWD");

            if (m_lastResponse.Code != 257)
                throw new FtpCommandException("Cannot get current directory.", m_lastResponse);

            Match m = s_pwdExpression.Match(m_lastResponse.Message);
            return m.Groups[2].Value;
        }

        internal void CDUP()
        {
            Command("CDUP");

            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete)
                throw new FtpCommandException("Cannot move to parent directory (CDUP).", m_lastResponse);
        }

        internal void CWD(string path)
        {
            Command("CWD " + path);

            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete && m_lastResponse.Code != FtpResponse.ClosingDataChannel)
                throw new FtpCommandException("Cannot change directory to " + path + ".", m_lastResponse);
        }

        internal void QUIT()
        {
            Command("QUIT");
        }

        internal void Type(TransferMode mode)
        {
            if (mode == TransferMode.Unknown)
                return;

            if (mode == TransferMode.Ascii && m_currentTransferMode != TransferMode.Ascii)
                Command("TYPE A");
            else if (mode == TransferMode.Binary && m_currentTransferMode != TransferMode.Binary)
                Command("TYPE I");

            m_currentTransferMode = mode;
        }

        internal void Rename(string oldName, string newName)
        {
            Command("RNFR " + oldName);

            if (m_lastResponse.Code != FtpResponse.RequestFileActionPending)
                throw new FtpCommandException("Failed to rename file from " + oldName + " to " + newName + ".", m_lastResponse);

            Command("RNTO " + newName);
            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete)
                throw new FtpCommandException("Failed to rename file from " + oldName + " to " + newName + ".", m_lastResponse);
        }

        internal Queue List(bool passive)
        {
            const string errorMsgListing = "Error when listing server directory.";

            try
            {
                Type(TransferMode.Ascii);
                FtpDataStream dataStream = GetPassiveDataStream();
                Queue lineQueue = new Queue();

                Command("LIST");

                if (m_lastResponse.Code != FtpResponse.DataChannelOpenedTransferStart && m_lastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                    throw new FtpCommandException(errorMsgListing, m_lastResponse);

                StreamReader lineReader = new StreamReader(dataStream, Encoding.Default);
                string line = lineReader.ReadLine();

                while ((object)line != null)
                {
                    lineQueue.Enqueue(line);
                    line = lineReader.ReadLine();
                }

                lineReader.Close();

                if (m_lastResponse.Code != FtpResponse.ClosingDataChannel)
                    throw new FtpCommandException(errorMsgListing, m_lastResponse);

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
                else
                    return new FtpOutputDataStream(this, client);
            }
            catch (IOException ie)
            {
                throw new Exception("Failed to open passive port (" + port + ") data connection due to IO exception: " + ie.Message + ".", ie);
            }
            catch (SocketException se)
            {
                throw new Exception("Failed to open passive port (" + port + ") data connection due to socket exception: " + se.Message + ".", se);
            }
        }

        private int GetPassivePort()
        {
            Command("PASV");

            if (m_lastResponse.Code == FtpResponse.EnterPassiveMode)
            {
                string[] numbers = s_regularExpression.Match(m_lastResponse.Message).Groups[2].Value.Split(',');
                return int.Parse(numbers[4]) * 256 + int.Parse(numbers[5]);
            }
            else
            {
                throw new FtpCommandException("Failed to enter passive mode.", m_lastResponse);
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_regularExpression = new Regex("(\\()(.*)(\\))");
        private static readonly Regex s_pwdExpression = new Regex("(\")(.*)(\")");

        #endregion
    }
}