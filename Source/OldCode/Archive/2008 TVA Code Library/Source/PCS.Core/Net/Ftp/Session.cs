//*******************************************************************************************************
//  Session.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/22/2003 - James R Carroll
//       Generated original version of source code.
//  09/23/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing;

// This FTP library is based on a similar C# library found on "The Code Project" web site originally written by
// Alex Kwok, the enhanced by Uwe Keim.  It was translated into VB with most of classes being renamed
// (removed Ftp prefix) and the namespace was changed to PCS.Ftp. Many bug fixes, additions and modifications
// have been made to this code as well as extensive testing.  Note worthy changes:  converted the C# delegates
// to standard .NET events for ease of use, made the library work with IIS based FTP servers that were in Unix
// mode, added detailed file system information for FTP files and directories (size, timestamp, etc), coverted
// FTP session into a component that could be dragged onto a design surface, created an FTP FileWatcher
// component and an FTP file system crawler based on this library - JRC

// JRC 2008: Now we're back to C# - lol - wonder if the code is any worse for the wear?  Oh well, too
// many bug fixes made to the code revert back to original code...

namespace PCS.Net.Ftp
{
    /// <summary>
    /// FTP Session
    /// </summary>
    [ToolboxBitmap(typeof(Session)), DefaultProperty("Server"), DefaultEvent("FileTransferProgress"), Description("Creates a client connection to an FTP server")]
    public class Session : Component
    {
        #region [ Members ]

        // Delegates
        public delegate void BeginFileTransferEventHandler(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection);
        public delegate void EndFileTransferEventHandler(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection);
        public delegate void FileTransferProgressEventHandler(long TotalBytes, long TotalBytesTransfered, TransferDirection TransferDirection);

        // Events
        public event BeginFileTransferEventHandler BeginFileTransfer;
        public event EndFileTransferEventHandler EndFileTransfer;
        public event FileTransferProgressEventHandler FileTransferProgress;
        public event Action<FtpAsyncResult> FileTransferNotification;
        public event Action<string> ResponseReceived;
        public event Action<string> CommandSent;

        // Fields
        private bool m_caseInsensitive;
        private ISessionState m_currentState;
        private int m_waitLockTimeOut;

        #endregion

        #region [ Constructors ]

        public Session()
            : this(false)
        {
        }

        public Session(bool caseInsensitive)
        {
            m_caseInsensitive = caseInsensitive;
            m_waitLockTimeOut = 10;
            m_currentState = new SessionDisconnected(this, m_caseInsensitive);
        }

        #endregion

        #region [ Properties ]

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

        [Browsable(true), Category("Configuration"), Description("Specify FTP server post if needed."), DefaultValue(21)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Directory CurrentDirectory
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

        [Browsable(false)]
        public Directory RootDirectory
        {
            get
            {
                return m_currentState.RootDirectory;
            }
        }

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

        public void SetCurrentDirectory(string directoryPath)
        {
            if (!IsConnected)
                throw new InvalidOperationException("You must be connected to the FTP server before you can set the current directory.");

            if (directoryPath.Length > 0)
            {
                m_currentState.CurrentDirectory = new Directory((SessionConnected)m_currentState, CaseInsensitive, directoryPath);
                m_currentState.CurrentDirectory.Refresh();
            }
        }

        [Browsable(false)]
        public FtpControlChannel ControlChannel
        {
            get
            {
                return m_currentState.ControlChannel;
            }
        }

        [Browsable(false)]
        public bool IsConnected
        {
            get
            {
                return (m_currentState is SessionConnected);
            }
        }

        [Browsable(false)]
        public bool IsBusy
        {
            get
            {
                return m_currentState.IsBusy;
            }
        }

        internal ISessionState State
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

        public void AbortTransfer()
        {
            m_currentState.AbortTransfer();
        }

        public void Connect(string UserName, string Password)
        {
            m_currentState.Connect(UserName, Password);
        }

        public void Close()
        {
            m_currentState.Close();
        }

        internal void OnResponseReceived(string response)
        {
            if (ResponseReceived != null)
                ResponseReceived(response);
        }

        internal void OnCommandSent(string command)
        {
            if (CommandSent != null)
                CommandSent(command);
        }

        internal void OnBeginFileTransfer(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection)
        {
            if (BeginFileTransfer != null)
                BeginFileTransfer(LocalFileName, RemoteFileName, TransferDirection);
        }

        internal void OnEndFileTransfer(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection)
        {
            if (EndFileTransfer != null)
                EndFileTransfer(LocalFileName, RemoteFileName, TransferDirection);
        }

        internal void OnFileTransferProgress(long TotalBytes, long TotalBytesTransfered, TransferDirection TransferDirection)
        {
            if (FileTransferProgress != null)
                FileTransferProgress(TotalBytes, TotalBytesTransfered, TransferDirection);
        }

        internal void OnFileTransferNotification(FtpAsyncResult TransferResult)
        {
            if (FileTransferNotification != null)
                FileTransferNotification(TransferResult);
        }

        #endregion
    }
}