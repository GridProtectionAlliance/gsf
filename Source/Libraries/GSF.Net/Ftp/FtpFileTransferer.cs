//******************************************************************************************************
//  FtpFileTransferer.cs - Gbtc
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
//  12/09/2009 - Pinal C. Patel
//       Modified StartTransfer() to open local file in read-only mode for uploads.
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

using System.IO;
using System.Threading;
using GSF.IO;

namespace GSF.Net.Ftp
{
    #region [ Enumerations ]

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

    // Internal FTP file transfer class
    internal class FtpFileTransferer
    {
        #region [ Members ]

        // Delegates
        private delegate void FileCommandDelegate(string remoteFileName);
        private delegate void StreamCopyDelegate(Stream remote, Stream local);

        // Fields
        private readonly StreamCopyDelegate m_streamCopyRoutine;
        private readonly FileCommandDelegate m_ftpFileCommandRoutine;
        private FtpDirectory m_transferStarter;
        private readonly FtpSessionConnected m_session;
        private readonly string m_localFile;
        private readonly string m_remoteFile;
        private readonly long m_totalBytes;
        private long m_totalBytesTransfered;
        private int m_transferedPercentage;
        private readonly TransferDirection m_transferDirection;
        private FtpAsyncResult m_transferResult;

        #endregion

        #region [ Constructors ]

        internal FtpFileTransferer(FtpDirectory transferStarter, string localFile, string remoteFile, long totalBytes, TransferDirection dir)
        {
            m_transferStarter = transferStarter;
            m_transferDirection = dir;
            m_session = transferStarter.Session;
            m_localFile = localFile;
            m_remoteFile = remoteFile;
            m_totalBytes = totalBytes;

            if (dir == TransferDirection.Upload)
            {
                m_streamCopyRoutine = LocalToRemote;
                m_ftpFileCommandRoutine = m_session.ControlChannel.STOR;
            }
            else
            {
                m_streamCopyRoutine = RemoteToLocal;
                m_ftpFileCommandRoutine = m_session.ControlChannel.RETR;
            }
        }

        #endregion

        #region [ Properties ]

        public string LocalFileName
        {
            get
            {
                return m_localFile;
            }
        }

        public string RemoteFileName
        {
            get
            {
                return m_remoteFile;
            }
        }

        public long TotalBytes
        {
            get
            {
                return m_totalBytes;
            }
        }

        public long TotalBytesTransfered
        {
            get
            {
                return m_totalBytesTransfered;
            }
        }

        public TransferDirection TransferDirection
        {
            get
            {
                return m_transferDirection;
            }
        }

        public FtpAsyncResult TransferResult
        {
            get
            {
                return m_transferResult;
            }
        }

        public int TransferedPercentage
        {
            get
            {
                return m_transferedPercentage;
            }
        }

        #endregion

        #region [ Methods ]

        private void TransferThreadProc()
        {
            try
            {
                StartTransfer();
                m_transferResult = new FtpAsyncResult("Success.", FtpAsyncResult.Complete);
                m_session.Host.OnFileTransferNotification(m_transferResult);
            }
            catch (FtpExceptionBase e)
            {
                m_transferResult = new FtpAsyncResult("Transfer fail: " + e.Message, FtpAsyncResult.Fail);
                m_session.Host.OnFileTransferNotification(m_transferResult);
            }
        }

        internal void StartTransfer()
        {
            FileStream localStream = null;
            FtpDataStream remoteStream = null;

            try
            {
                // Files just created may still have a file lock, we'll wait a few seconds for read access if needed...
                if (m_transferDirection == TransferDirection.Upload)
                    FilePath.WaitForReadLock(m_localFile, m_session.Host.WaitLockTimeout);

                m_session.Host.OnBeginFileTransfer(m_localFile, m_remoteFile, m_transferDirection);

                remoteStream = m_session.ControlChannel.GetDataStream(m_transferDirection);
                m_ftpFileCommandRoutine(m_remoteFile);

                if (m_transferDirection == TransferDirection.Download)
                    localStream = new FileStream(m_localFile, FileMode.Create);
                else
                    localStream = new FileStream(m_localFile, FileMode.Open, FileAccess.Read);

                m_streamCopyRoutine(remoteStream, localStream);

                // Dispose remote stream before testing file transfer result to ensure
                // we have received the server's response to the file transfer command
                remoteStream.Dispose();
                TestTransferResult();

                m_session.Host.OnEndFileTransfer(m_localFile, m_remoteFile, m_transferDirection);
            }
            catch
            {
                m_session.Host.OnEndFileTransfer(m_localFile, m_remoteFile, m_transferDirection);
                throw;
            }
            finally
            {
                remoteStream?.Dispose();
                localStream?.Dispose();
            }
        }

        internal void StartAsyncTransfer()
        {
#if ThreadTracking
            ManagedThread thread = new ManagedThread(TransferThreadProc);
            thread.Name = "GSF.Net.Ftp.FileTransferer.TransferThreadProc() [" + m_remoteFile + "]";
#else
            Thread thread = new Thread(TransferThreadProc);
            thread.Name = "Transfer file thread: " + m_remoteFile;
#endif
            thread.Start();
        }

        private void TestTransferResult()
        {
            int responseCode = m_session.ControlChannel.LastResponse.Code;

            if (responseCode == FtpResponse.ClosingDataChannel)
                return;

            if (responseCode == FtpResponse.RequestFileActionComplete)
                return;

            throw new FtpDataTransferException("Failed to transfer file.", m_session.ControlChannel.LastResponse);
        }

        private void RemoteToLocal(Stream remote, Stream local)
        {
            StreamCopy(local, remote);
        }

        private void LocalToRemote(Stream remote, Stream local)
        {
            StreamCopy(remote, local);
        }

        private void StreamCopy(Stream dest, Stream source)
        {
            int byteRead;
            long onePercentage;
            long bytesReadFromLastProgressEvent;
            byte[] buffer = new byte[4 * 1024 + 1];
            ProcessProgress<long> progress = new ProcessProgress<long>("FTP " + m_transferDirection + " File Transfer", "Transferring file \"" + m_remoteFile + "\"...", m_totalBytes, 0);

            onePercentage = m_totalBytes / 100;
            bytesReadFromLastProgressEvent = 0;
            byteRead = source.Read(buffer, 0, 4 * 1024);

            while (byteRead != 0)
            {
                m_totalBytesTransfered += byteRead;
                bytesReadFromLastProgressEvent += byteRead;

                if (bytesReadFromLastProgressEvent > onePercentage)
                {
                    m_transferedPercentage = (int)(((float)m_totalBytesTransfered) / ((float)m_totalBytes) * 100);
                    progress.Complete = m_totalBytesTransfered;
                    m_session.Host.OnFileTransferProgress(progress, m_transferDirection);
                    bytesReadFromLastProgressEvent = 0;
                }

                dest.Write(buffer, 0, byteRead);
                byteRead = source.Read(buffer, 0, 4 * 1024);
            }
        }

        #endregion
    }
}