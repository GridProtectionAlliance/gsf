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
using System.Net;
using System.Threading;

namespace GSF.Net.Ftp
{
    internal class FtpSessionConnected : IFtpSessionState
    {
        #region [ Members ]

        // Fields
        private FtpDirectory m_current;
        private FtpDataStream m_dataStream;
        private readonly bool m_caseInsensitive;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        internal FtpSessionConnected(FtpClient h, FtpControlChannel ctrl, bool caseInsensitive)
        {
            Host = h;
            ControlChannel = ctrl;
            ControlChannel.Session = this;
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
            get => ControlChannel.Server;
            set => throw new InvalidOperationException();
        }

        public int Port
        {
            get => ControlChannel.Port;
            set => throw new InvalidOperationException();
        }

        public int Timeout
        {
            get => ControlChannel.Timeout;
            set => throw new InvalidOperationException();
        }

        public bool Passive
        {
            get => ControlChannel.Passive;
            set => throw new InvalidOperationException();
        }

        public IPAddress ActiveAddress
        {
            get => ControlChannel.ActiveAddress;
            set => throw new InvalidOperationException();
        }

        public int MinActivePort
        {
            get => ControlChannel.DataChannelPortRange.Start;
            set => throw new InvalidOperationException();
        }

        public int MaxActivePort
        {
            get => ControlChannel.DataChannelPortRange.End;
            set => throw new InvalidOperationException();
        }

        public FtpDirectory RootDirectory { get; private set; }

        public FtpDirectory CurrentDirectory
        {
            get => m_current;
            set
            {
                ControlChannel.CWD(value.FullPath);
                m_current = value;
                m_current.ClearItems();
            }
        }

        public bool IsBusy => m_dataStream != null;

        internal FtpClient Host { get; private set; }

        public FtpControlChannel ControlChannel { get; private set; }

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
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                Host = null;
                RootDirectory = null;
                m_current = null;

                ControlChannel?.Close();
                ControlChannel = null;

                m_dataStream?.Dispose();
                m_dataStream = null;
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        internal void InitRootDirectory()
        {
            RootDirectory = new FtpDirectory(this, m_caseInsensitive, ControlChannel.PWD());
            m_current = RootDirectory;
        }

        // You can only aborting file transfer started by
        // BeginPutFile and BeginGetFile
        public void AbortTransfer()
        {
            // Save a copy of m_dataStream since it will be set
            // to null when DataStream call EndDataTransfer
            FtpDataStream tempDataStream = m_dataStream;

            if (tempDataStream == null)
                return;

            tempDataStream.Abort();

            while (!tempDataStream.IsClosed)
                Thread.Sleep(1);
        }

        public void Close()
        {
            Host.State = new FtpSessionDisconnected(Host, m_caseInsensitive);
            Host.Server = ControlChannel.Server;
            Host.Port = ControlChannel.Port;

            try
            {
                ControlChannel.QUIT();
            }
            finally
            {
                ControlChannel.Close();
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