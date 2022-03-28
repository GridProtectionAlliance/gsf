//******************************************************************************************************
//  FtpClient.cs - Gbtc
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

using System;

// ReSharper disable InconsistentNaming
namespace GSF.Net.VirtualFtpClient
{
    #region [ Enumerations ]
    
    /// <summary>
    /// Defines support FTP types for virtual FTP client.
    /// </summary>
    public enum FtpType
    {
        /// <summary>
        /// Standard File Transport Protocol (FTP)
        /// </summary>
        Ftp,
        /// <summary>
        /// Trivial File Transport Protocol (TFTP)
        /// </summary>
        TFtp

        // Future: FTPS
        // Future: SFTP/SSH
    }

    /// <summary>
    /// FTP file transfer direction enumeration.
    /// </summary>
    public enum TransferDirection
    {
        /// <summary>
        /// FTP transfer direction set to upload.
        /// </summary>
        Upload,
        /// <summary>
        /// FTP transfer direction set to download.
        /// </summary>
        Download
    }
    
    #endregion

    /// <summary>
    /// Represents an virtual FTP session for the specified target <see cref="VirtualFtpClient.FtpType"/>.
    /// </summary>
    public class FtpClient : IDisposable
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Raised when file transfer begins.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2,T3}.Argument1"/> is local filename,
        /// <see cref="EventArgs{T1,T2,T3}.Argument2"/> is remote filename, 
        /// <see cref="EventArgs{T1,T2,T3}.Argument3"/> is file transfer direction.
        /// </remarks>
        public event EventHandler<EventArgs<string, string, TransferDirection>> BeginFileTransfer;

        /// <summary>
        /// Raised when file transfer completes.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2,T3}.Argument1"/> is local filename,
        /// <see cref="EventArgs{T1,T2,T3}.Argument2"/> is remote filename, 
        /// <see cref="EventArgs{T1,T2,T3}.Argument3"/> is file transfer direction.
        /// </remarks>
        public event EventHandler<EventArgs<string, string, TransferDirection>> EndFileTransfer;

        /// <summary>
        /// Raised as file transfer is progressing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is current file transfer progress,
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is file transfer direction.
        /// </remarks>
        public event EventHandler<EventArgs<ProcessProgress<long>, TransferDirection>> FileTransferProgress;

        /// <summary>
        /// Raised when file transfer process has completed (success or failure).
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is result of FTP transfer process.
        /// </remarks>
        public event EventHandler<EventArgs<FtpTransferResult>> FileTransferNotification;

        /// <summary>
        /// Raised when FTP response has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is received FTP response.
        /// </remarks>
        public event EventHandler<EventArgs<string>> ResponseReceived;

        /// <summary>
        /// Raised when FTP command has been sent.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is sent FTP command.
        /// </remarks>
        public event EventHandler<EventArgs<string>> CommandSent;

        /// <summary>
        /// Raised when class is disposed;
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private Ftp.FtpClient m_ftpClient;
        private TFtp.TFtpClient m_tftpClient;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FtpClient"/>.
        /// </summary>
        /// <param name="ftpType">Target <see cref="VirtualFtpClient.FtpType"/> for this <see cref="FtpClient"/></param>
        /// <param name="caseSensitive">Set to true to be case sensitive with FTP file and directory names.</param>
        public FtpClient(FtpType ftpType, bool caseSensitive = false)
        {
            FtpType = ftpType;
            CaseSensitive = caseSensitive;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="VirtualFtpClient.FtpType"/> for this <see cref="FtpClient"/>.
        /// </summary>
        public FtpType FtpType { get; }

        /// <summary>
        /// Gets or sets FTP server name (DNS name or IP).
        /// </summary>
        /// <remarks>
        /// FTP server name should not be prefixed with FTP://.
        /// </remarks>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets FTP server port to use, defaults to 21 for FTP and 69 for TFTP.
        /// </summary>
        /// <remarks>
        /// This only needs to be changed if the FTP server is established on a non-standard port number.
        /// </remarks>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the timeout, in milliseconds, for
        /// read and write operations, defaults to 30 seconds.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets FTP case sensitivity of file and directory names.
        /// </summary>
        /// <remarks>
        /// Set to true to be case sensitive with FTP file and directory names.
        /// </remarks>
        public bool CaseSensitive { get; }

        /// <summary>
        /// Gets or sets the passive/active mode of the server.
        /// </summary>
        public bool Passive { get; set; }

        /// <summary>
        /// Gets or sets the IP address to send with the PORT command.
        /// </summary>
        public string ActiveAddress { get; set; }

        /// <summary>
        /// Gets or sets the minimum value in the range of ports
        /// used when listening for connections in active mode.
        /// </summary>
        public int? MinActivePort { get; set; }

        /// <summary>
        /// Gets or sets the maximum value in the range of ports
        /// used when listening for connections in active mode.
        /// </summary>
        public int? MaxActivePort { get; set; }

        /// <summary>
        /// Returns true if FTP session is currently connected.
        /// </summary>
        public bool IsConnected => FtpClientConnection?.IsConnected ?? TFtpClientConnection != null;

        internal Ftp.FtpClient FtpClientConnection
        {
            get => m_ftpClient;
            set
            {
                if (m_ftpClient != null)
                {
                    // Detach from events on existing reference
                    m_ftpClient.BeginFileTransfer -= FtpClient_BeginFileTransfer;
                    m_ftpClient.EndFileTransfer -= FtpClient_EndFileTransfer;
                    m_ftpClient.FileTransferProgress -= FtpClient_FileTransferProgress;
                    m_ftpClient.FileTransferNotification -= FtpClient_FileTransferNotification;
                    m_ftpClient.ResponseReceived -= FtpClient_ResponseReceived;
                    m_ftpClient.CommandSent -= FtpClient_CommandSent;

                    if (m_ftpClient != value)
                        m_ftpClient?.Dispose();
                }

                // Assign new reference
                m_ftpClient = value;

                if (m_ftpClient == null)
                    return;

                // Attach to events on new reference
                m_ftpClient.BeginFileTransfer += FtpClient_BeginFileTransfer;
                m_ftpClient.EndFileTransfer += FtpClient_EndFileTransfer;
                m_ftpClient.FileTransferProgress += FtpClient_FileTransferProgress;
                m_ftpClient.FileTransferNotification += FtpClient_FileTransferNotification;
                m_ftpClient.ResponseReceived += FtpClient_ResponseReceived;
                m_ftpClient.CommandSent += FtpClient_CommandSent;
            }
        }

        internal TFtp.TFtpClient TFtpClientConnection
        {
            get => m_tftpClient;
            set
            {
                if (m_tftpClient != null && m_tftpClient != value)
                    m_ftpClient?.Dispose();

                // Assign new reference
                m_tftpClient = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="FtpClient"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FtpClient"/> object and optionally releases the managed resources.
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
                    m_ftpClient?.Dispose();
                }
            }
            finally
            {
                Disposed?.Invoke(this, EventArgs.Empty);
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Connects to FTP server using specified credentials.
        /// </summary>
        /// <param name="userName">User name used to authenticate to FTP server.</param>
        /// <param name="password">Password used to authenticate to FTP server.</param>
        public void Connect(string userName, string password)
        {
            switch (FtpType)
            {
                case FtpType.Ftp:
                {
                    FtpClientConnection = new Ftp.FtpClient(!CaseSensitive)
                    {
                        Server = Server,
                        Port = Port ?? 21,
                        Timeout = Timeout,
                        Passive = Passive,
                        ActiveAddress = ActiveAddress,
                        MinActivePort = MinActivePort ?? 0,
                        MaxActivePort = MaxActivePort ?? 0
                    };

                    FtpClientConnection.Connect(userName, password);
                    break;
                }

                case FtpType.TFtp:
                {
                    TFtpClientConnection = new TFtp.TFtpClient(Server, Port ?? TFtp.TFtpServer.DefaultServerPort);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Changes the current FTP session directory to the specified path.
        /// </summary>
        /// <param name="directoryPath">New directory.</param>
        public void SetCurrentDirectory(string directoryPath)
        {
            if (!IsConnected)
                throw new InvalidOperationException("You must be connected to the FTP server before you can set the current directory.");

            if (directoryPath.Length <= 0)
                return;

            //State.CurrentDirectory = new FtpDirectory((FtpSessionConnected)State, CaseInsensitive, directoryPath);
            //State.CurrentDirectory.Refresh();
        }

        private void OnBeginFileTransfer(string localFileName, string remoteFileName, TransferDirection transferDirection)
        {
            BeginFileTransfer?.Invoke(this, new EventArgs<string, string, TransferDirection>(localFileName, remoteFileName, transferDirection));
        }

        private void OnEndFileTransfer(string localFileName, string remoteFileName, TransferDirection transferDirection)
        {
            EndFileTransfer?.Invoke(this, new EventArgs<string, string, TransferDirection>(localFileName, remoteFileName, transferDirection));
        }

        private void OnFileTransferProgress(ProcessProgress<long> fileTransferProgress, TransferDirection transferDirection)
        {
            FileTransferProgress?.Invoke(this, new EventArgs<ProcessProgress<long>, TransferDirection>(fileTransferProgress, transferDirection));
        }

        private void OnFileTransferNotification(FtpTransferResult transferResult)
        {
            FileTransferNotification?.Invoke(this, new EventArgs<FtpTransferResult>(transferResult));
        }

        private void OnResponseReceived(string response)
        {
            ResponseReceived?.Invoke(this, new EventArgs<string>(response));
        }

        private void OnCommandSent(string command)
        {
            CommandSent?.Invoke(this, new EventArgs<string>(command));
        }

        private void FtpClient_BeginFileTransfer(object sender, EventArgs<string, string, Ftp.TransferDirection> e)
        {
            OnBeginFileTransfer(e.Argument1, e.Argument2, e.Argument3 == Ftp.TransferDirection.Upload ? TransferDirection.Upload : TransferDirection.Download);
        }

        private void FtpClient_EndFileTransfer(object sender, EventArgs<string, string, Ftp.TransferDirection> e)
        {
            OnEndFileTransfer(e.Argument1, e.Argument2, e.Argument3 == Ftp.TransferDirection.Upload ? TransferDirection.Upload : TransferDirection.Download);
        }

        private void FtpClient_FileTransferProgress(object sender, EventArgs<ProcessProgress<long>, Ftp.TransferDirection> e)
        {
            OnFileTransferProgress(e.Argument1, e.Argument2 == Ftp.TransferDirection.Upload ? TransferDirection.Upload : TransferDirection.Download);
        }

        private void FtpClient_FileTransferNotification(object sender, EventArgs<Ftp.FtpAsyncResult> e)
        {
            OnFileTransferNotification(new FtpTransferResult(e.Argument.Message, e.Argument.ResponseCode));
        }

        private void FtpClient_ResponseReceived(object sender, EventArgs<string> e)
        {
            OnResponseReceived(e.Argument);
        }

        private void FtpClient_CommandSent(object sender, EventArgs<string> e)
        {
            OnCommandSent(e.Argument);
        }

        #endregion
    }
}
