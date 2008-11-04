//*******************************************************************************************************
//  SessionConnected.cs
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
//
//*******************************************************************************************************

using System;
using System.Threading;

namespace PCS.Net.Ftp
{
    internal class SessionConnected : ISessionState
    {
        #region [ Members ]

        // Fields
        private Session m_host;
        private FtpControlChannel m_ctrlChannel;
        private FtpDirectory m_root;
        private FtpDirectory m_current;
        private FtpDataStream m_dataStream;
        private bool m_caseInsensitive;

        #endregion

        #region [ Constructors ]

        internal SessionConnected(Session h, FtpControlChannel ctrl, bool caseInsensitive)
        {
            m_host = h;
            m_ctrlChannel = ctrl;
            m_ctrlChannel.Session = this;
            m_caseInsensitive = caseInsensitive;
        }

        #endregion

        #region [ Properties ]

        public string Server
        {
            get
            {
                return m_ctrlChannel.Server;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public int Port
        {
            get
            {
                return m_ctrlChannel.Port;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public FtpDirectory RootDirectory
        {
            get
            {
                return m_root;
            }
        }

        public FtpDirectory CurrentDirectory
        {
            get
            {
                return m_current;
            }
            set
            {
                m_ctrlChannel.CWD(value.FullPath);
                m_current = value;
                m_current.ClearItems();
            }
        }

        public bool IsBusy
        {
            get
            {
                return (m_dataStream != null);
            }
        }

        internal Session Host
        {
            get
            {
                return m_host;
            }
        }

        public FtpControlChannel ControlChannel
        {
            get
            {
                return m_ctrlChannel;
            }
        }

        #endregion

        #region [ Methods ]

        internal void InitRootDirectory()
        {
            m_root = new FtpDirectory(this, m_caseInsensitive, m_ctrlChannel.PWD());
            m_current = m_root;
        }

        // You can only aborting file transfer started by
        // BeginPutFile and BeginGetFile
        public void AbortTransfer()
        {
            // Save a copy of m_dataStream since it will be set
            // to null when DataStream call EndDataTransfer
            FtpDataStream tempDataStream = m_dataStream;

            if (!(tempDataStream == null))
            {
                tempDataStream.Abort();

                while (!tempDataStream.IsClosed)
                    Thread.Sleep(1);
            }
        }

        public void Close()
        {
            m_host.State = new SessionDisconnected(m_host, m_caseInsensitive);
            m_host.Server = m_ctrlChannel.Server;
            m_host.Port = m_ctrlChannel.Port;

            try
            {
                m_ctrlChannel.QUIT();
            }
            finally
            {
                m_ctrlChannel.Close();
            }
        }

        public void Connect(string UserName, string Password)
        {
            throw new InvalidOperationException();
        }

        internal void BeginDataTransfer(FtpDataStream stream)
        {
            lock (this)
            {
                if (m_dataStream != null)
                    throw new FtpDataTransferException();

                m_dataStream = stream;
            }
        }

        internal void EndDataTransfer()
        {
            lock (this)
            {
                if (m_dataStream == null)
                    throw new InvalidOperationException();

                m_dataStream = null;
            }
        }

        #endregion
    }
}