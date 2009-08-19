//*******************************************************************************************************
//  FtpSession.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/22/2003 - James R. Carroll
//       Generated original version of source code.
//  09/23/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing;

// This FTP library is based on a similar C# library found on "The Code Project" web site originally written by
// Alex Kwok, the enhanced by Uwe Keim.  It was translated into VB with most of classes being renamed
// (removed Ftp prefix) and the namespace was changed to TVA.Ftp. Many bug fixes, additions and modifications
// have been made to this code as well as extensive testing.  Note worthy changes:  converted the C# delegates
// to standard .NET events for ease of use, made the library work with IIS based FTP servers that were in Unix
// mode, added detailed file system information for FTP files and directories (size, timestamp, etc), coverted
// FTP session into a component that could be dragged onto a design surface, created an FTP FileWatcher
// component and an FTP file system crawler based on this library - JRC

// JRC 2008: Now we're back to C# and I replaced the "Ftp" prefix to the classes for to satisfy uniqueness
// in type name constraint coming from code analysis.

namespace TVA.Net.Ftp
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
            : base()
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
            if (container != null)
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
                        if (m_currentState != null)
                            m_currentState.Dispose();

                        m_currentState = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
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
            if (ResponseReceived != null)
                ResponseReceived(this, new EventArgs<string>(response));
        }

        internal void OnCommandSent(string command)
        {
            if (CommandSent != null)
                CommandSent(this, new EventArgs<string>(command));
        }

        internal void OnBeginFileTransfer(string localFileName, string remoteFileName, TransferDirection transferDirection)
        {
            if (BeginFileTransfer != null)
                BeginFileTransfer(this, new EventArgs<string,string,TransferDirection>(localFileName, remoteFileName, transferDirection));
        }

        internal void OnEndFileTransfer(string localFileName, string remoteFileName, TransferDirection transferDirection)
        {
            if (EndFileTransfer != null)
                EndFileTransfer(this, new EventArgs<string,string,TransferDirection>(localFileName, remoteFileName, transferDirection));
        }

        internal void OnFileTransferProgress(ProcessProgress<long> fileTransferProgress, TransferDirection transferDirection)
        {
            if (FileTransferProgress != null)
                FileTransferProgress(this, new EventArgs<ProcessProgress<long>,TransferDirection>(fileTransferProgress, transferDirection));
        }

        internal void OnFileTransferNotification(FtpAsyncResult transferResult)
        {
            if (FileTransferNotification != null)
                FileTransferNotification(this, new EventArgs<FtpAsyncResult>(transferResult));
        }

        #endregion
    }
}