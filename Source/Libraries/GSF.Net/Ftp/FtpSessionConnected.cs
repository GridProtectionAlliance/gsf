//******************************************************************************************************
//  FtpSessionConnected.cs - Gbtc
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
using System.Threading;

namespace GSF.Net.Ftp
{
    internal class FtpSessionConnected : IFtpSessionState
    {
        #region [ Members ]

        // Fields
        private FtpClient m_host;
        private FtpControlChannel m_ctrlChannel;
        private FtpDirectory m_root;
        private FtpDirectory m_current;
        private FtpDataStream m_dataStream;
        private readonly bool m_caseInsensitive;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        internal FtpSessionConnected(FtpClient h, FtpControlChannel ctrl, bool caseInsensitive)
        {
            m_host = h;
            m_ctrlChannel = ctrl;
            m_ctrlChannel.Session = this;
            m_caseInsensitive = caseInsensitive;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FtpSessionConnected"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FtpSessionConnected()
        {
            Dispose(false);
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

        public int Timeout
        {
            get
            {
                return m_ctrlChannel.Timeout;
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
                return ((object)m_dataStream != null);
            }
        }

        internal FtpClient Host
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

        /// <summary>
        /// Releases all the resources used by the <see cref="FtpSessionConnected"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FtpSessionConnected"/> object and optionally releases the managed resources.
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
                        m_host = null;
                        m_root = null;
                        m_current = null;

                        if ((object)m_ctrlChannel != null)
                            m_ctrlChannel.Close();

                        m_ctrlChannel = null;

                        if ((object)m_dataStream != null)
                            m_dataStream.Dispose();

                        m_dataStream = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

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

            if ((object)tempDataStream != null)
            {
                tempDataStream.Abort();

                while (!tempDataStream.IsClosed)
                    Thread.Sleep(1);
            }
        }

        public void Close()
        {
            m_host.State = new FtpSessionDisconnected(m_host, m_caseInsensitive);
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

        public void Connect(string userName, string password)
        {
            throw new InvalidOperationException();
        }

        internal void BeginDataTransfer(FtpDataStream stream)
        {
            lock (this)
            {
                if ((object)m_dataStream != null)
                    throw new FtpDataTransferException();

                m_dataStream = stream;
            }
        }

        internal void EndDataTransfer()
        {
            lock (this)
            {
                if ((object)m_dataStream == null)
                    throw new InvalidOperationException();

                m_dataStream = null;
            }
        }

        #endregion
    }
}