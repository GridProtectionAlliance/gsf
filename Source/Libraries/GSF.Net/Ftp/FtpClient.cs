//******************************************************************************************************
//  FtpClient.cs - Gbtc
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
//  09/23/2008 - J. Ritchie Carroll
//       Generated original version of source code.
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
using System.ComponentModel;
using System.Drawing;
using System.Net;

// This FTP library is based on a similar C# library found on "The Code Project" web site originally written by
// Alex Kwok (no license specified), then enhanced by Uwe Keim (licensed under The Code Project Open License).
// GSF translated the code into VB with most of classes being renamed (removed Ftp prefix) and the namespace was
// changed to GSF.Ftp. Many bug fixes, additions and modifications have been made to this code as well as extensive
// testing.  Note worthy changes:  converted the C# delegates to standard .NET events for ease of use, made the
// library work with IIS based FTP servers that were in Unix mode, added detailed file system information for FTP
// files and directories (size, timestamp, etc), converted FTP session into a component that could be dragged onto
// a design surface, created an FTP FileWatcher component and an FTP file system crawler based on this library.
// In 2008 we migrated the entire code set to C# and restored the original "Ftp" prefix to the classes for to
// satisfy uniqueness in type name constraint coming from code analysis.

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Represents a FTP session.
    /// </summary>
    /// <remarks>
    /// Creates a client connection to an FTP server for uploading or downloading files.
    /// </remarks>
    [ToolboxBitmap(typeof(FtpClient)), DefaultProperty("Server"), DefaultEvent("FileTransferProgress"), Description("Creates a client connection to an FTP server")]
    public class FtpClient : Component
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
        /// Raised when ansynchronous file transfer process has completed (success or failure).
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is result of asynchronous FTP transfer process.
        /// </remarks>
        public event EventHandler<EventArgs<FtpAsyncResult>> FileTransferNotification;

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

        // Fields
        private bool m_caseInsensitive;
        private IFtpSessionState m_currentState;
        private int m_waitLockTimeOut;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new FTP session using the default settings.
        /// </summary>
        public FtpClient()
            : this(false)
        {
        }

        /// <summary>
        /// Constructs a new FTP session using the specified settings.
        /// </summary>
        /// <param name="caseInsensitive">Set to true to not be case sensitive with FTP file and directory names.</param>
        public FtpClient(bool caseInsensitive)
        {
            m_caseInsensitive = caseInsensitive;
            m_waitLockTimeOut = 10;
            m_currentState = new FtpSessionDisconnected(this, m_caseInsensitive);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="FtpClient"/>.</param>
        public FtpClient(IContainer container)
            : this()
        {
            if ((object)container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets FTP server name (DNS name or IP).
        /// </summary>
        /// <remarks>
        /// FTP server name should not be prefixed with FTP://.
        /// </remarks>
        [Browsable(true), Category("Configuration"), Description("Specify FTP server name (do not prefix with FTP://).")]
        public string Server
        {
            get
            {
                return m_currentState.Server;
            }
            set
            {
                m_currentState.Server = value;
            }
        }

        /// <summary>
        /// Gets or sets FTP case sensitivity of file and directory names.
        /// </summary>
        /// <remarks>
        /// Set to true to not be case sensitive with FTP file and directory names.
        /// </remarks>
        [Browsable(true), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file names."), DefaultValue(false)]
        public bool CaseInsensitive
        {
            get
            {
                return m_caseInsensitive;
            }
            set
            {
                m_caseInsensitive = value;
            }
        }

        /// <summary>
        /// Gets or sets FTP server port to use, defaults to 21.
        /// </summary>
        /// <remarks>
        /// This only needs to be changed if the FTP server is established on a non-standard port number.
        /// </remarks>
        [Browsable(true), Category("Configuration"), Description("Specify FTP server port, if needed."), DefaultValue(21)]
        public int Port
        {
            get
            {
                return m_currentState.Port;
            }
            set
            {
                m_currentState.Port = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout, in milliseconds, for
        /// read and write operations, defaults to 30 seconds.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Specify FTP server read/write timeout, if needed."), DefaultValue(30000)]
        public int Timeout
        {
            get
            {
                return m_currentState.Timeout;
            }
            set
            {
                m_currentState.Timeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the passive/active mode of the server.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Specify passive/active mode of FTP server."), DefaultValue(true)]
        public bool Passive
        {
            get
            {
                return m_currentState.Passive;
            }
            set
            {
                m_currentState.Passive = value;
            }
        }

        /// <summary>
        /// Gets or sets the IP address to send with the PORT command.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Specify IP address to send with PORT command."), DefaultValue(null)]
        string ActiveAddress
        {
            get
            {
                return m_currentState.ActiveAddress.ToString();
            }
            set
            {
                IPAddress address;

                if (IPAddress.TryParse(value, out address))
                    m_currentState.ActiveAddress = address;
                else
                    m_currentState.ActiveAddress = null;
            }
        }

        /// <summary>
        /// Gets or sets the minimum value in the range of ports
        /// used when listening for connections in active mode.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Specify minimum port in range of active ports."), DefaultValue(0)]
        int MinActivePort
        {
            get
            {
                return m_currentState.MinActivePort;
            }
            set
            {
                m_currentState.MinActivePort = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value in the range of ports
        /// used when listening for connections in active mode.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Specify maximum port in range of active ports."), DefaultValue(0)]
        int MaxActivePort
        {
            get
            {
                return m_currentState.MaxActivePort;
            }
            set
            {
                m_currentState.MaxActivePort = value;
            }
        }

        /// <summary>
        /// Gets or sets current FTP session directory.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FtpDirectory CurrentDirectory
        {
            get
            {
                return m_currentState.CurrentDirectory;
            }
            set
            {
                m_currentState.CurrentDirectory = value;
            }
        }

        /// <summary>
        /// Gets FTP session root directory entry.
        /// </summary>
        [Browsable(false)]
        public FtpDirectory RootDirectory
        {
            get
            {
                return m_currentState.RootDirectory;
            }
        }

        /// <summary>
        /// Gets or sets maximum number of seconds to wait for read lock for files to be uploaded. Defaults to 10 seconds.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Specify the maximum number of seconds to wait for read lock for files to be uploaded."), DefaultValue(10)]
        public int WaitLockTimeout
        {
            get
            {
                return m_waitLockTimeOut;
            }
            set
            {
                m_waitLockTimeOut = value;
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

            if (directoryPath.Length > 0)
            {
                m_currentState.CurrentDirectory = new FtpDirectory((FtpSessionConnected)m_currentState, CaseInsensitive, directoryPath);
                m_currentState.CurrentDirectory.Refresh();
            }
        }

        /// <summary>
        /// Gets the current FTP control channel.
        /// </summary>
        [Browsable(false)]
        public FtpControlChannel ControlChannel
        {
            get
            {
                return m_currentState.ControlChannel;
            }
        }

        /// <summary>
        /// Returns true if FTP session is currently connected.
        /// </summary>
        [Browsable(false)]
        public bool IsConnected
        {
            get
            {
                return (m_currentState is FtpSessionConnected);
            }
        }

        /// <summary>
        /// Returns true if FTP session is currently busy.
        /// </summary>
        [Browsable(false)]
        public bool IsBusy
        {
            get
            {
                return m_currentState.IsBusy;
            }
        }

        internal IFtpSessionState State
        {
            get
            {
                return m_currentState;
            }
            set
            {
                m_currentState = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FtpClient"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_currentState != null)
                            m_currentState.Dispose();

                        m_currentState = null;
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Aborts current file transfer.
        /// </summary>
        public void AbortTransfer()
        {
            m_currentState.AbortTransfer();
        }

        /// <summary>
        /// Connects to FTP server using specified credentials.
        /// </summary>
        /// <param name="userName">User name used to authenticate to FTP server.</param>
        /// <param name="password">Password used to authenticate to FTP server.</param>
        public void Connect(string userName, string password)
        {
            m_currentState.Connect(userName, password);
        }

        /// <summary>
        /// Closes current FTP session.
        /// </summary>
        public void Close()
        {
            m_currentState.Close();
        }

        internal void OnResponseReceived(string response)
        {
            if ((object)ResponseReceived != null)
                ResponseReceived(this, new EventArgs<string>(response));
        }

        internal void OnCommandSent(string command)
        {
            if ((object)CommandSent != null)
                CommandSent(this, new EventArgs<string>(command));
        }

        internal void OnBeginFileTransfer(string localFileName, string remoteFileName, TransferDirection transferDirection)
        {
            if ((object)BeginFileTransfer != null)
                BeginFileTransfer(this, new EventArgs<string, string, TransferDirection>(localFileName, remoteFileName, transferDirection));
        }

        internal void OnEndFileTransfer(string localFileName, string remoteFileName, TransferDirection transferDirection)
        {
            if ((object)EndFileTransfer != null)
                EndFileTransfer(this, new EventArgs<string, string, TransferDirection>(localFileName, remoteFileName, transferDirection));
        }

        internal void OnFileTransferProgress(ProcessProgress<long> fileTransferProgress, TransferDirection transferDirection)
        {
            if ((object)FileTransferProgress != null)
                FileTransferProgress(this, new EventArgs<ProcessProgress<long>, TransferDirection>(fileTransferProgress, transferDirection));
        }

        internal void OnFileTransferNotification(FtpAsyncResult transferResult)
        {
            if ((object)FileTransferNotification != null)
                FileTransferNotification(this, new EventArgs<FtpAsyncResult>(transferResult));
        }

        #endregion
    }
}