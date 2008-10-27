//*******************************************************************************************************
//  FileTransferer.cs
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

using System.IO;
using PCS.IO;
using PCS.Threading;

namespace PCS.Net.Ftp
{
    #region [ Enumerations ]

    public enum TransferDirection
    {
        Upload,
        Download
    }

    #endregion

    internal class FileTransferer
    {
        #region [ Members ]

        // Delegates
        private delegate void FileCommandDelegate(string remoteFileName);
        private delegate void StreamCopyDelegate(Stream remote, Stream local);

        // Fields
        private StreamCopyDelegate m_streamCopyRoutine;
        private FileCommandDelegate m_ftpFileCommandRoutine;
        private Directory m_transferStarter;
        private SessionConnected m_session;
        private string m_localFile;
        private string m_remoteFile;
        private long m_totalBytes;
        private long m_totalBytesTransfered;
        private int m_transferedPercentage;
        private TransferDirection m_transferDirection;
        private FileMode m_localFileOpenMode;
        private AsyncResult m_transferResult;

        #endregion

        #region [ Constructors ]

        internal FileTransferer(Directory transferStarter, string localFile, string remoteFile, long totalBytes, TransferDirection dir)
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
                m_localFileOpenMode = FileMode.Open;
            }
            else
            {
                m_streamCopyRoutine = RemoteToLocal;
                m_ftpFileCommandRoutine = m_session.ControlChannel.RETR;
                m_localFileOpenMode = FileMode.Create;
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

        public AsyncResult TransferResult
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
                m_transferResult = new AsyncResult("Success.", AsyncResult.Complete);
                m_session.Host.OnFileTransferNotification(m_transferResult);
            }
            catch (ExceptionBase e)
            {
                m_transferResult = new AsyncResult("Transfer fail: " + e.Message, AsyncResult.Fail);
                m_session.Host.OnFileTransferNotification(m_transferResult);
            }
        }

        internal void StartTransfer()
        {
            FileStream localStream = null;
            DataStream remoteStream = null;

            try
            {
                // Files just created may still have a file lock, we'll wait a few seconds for read access if needed...
                if (m_transferDirection == TransferDirection.Upload)
                    FilePath.WaitForReadLock(m_localFile, m_session.Host.WaitLockTimeout);

                m_session.Host.OnBeginFileTransfer(m_localFile, m_remoteFile, m_transferDirection);

                localStream = new FileStream(m_localFile, m_localFileOpenMode);
                remoteStream = m_session.ControlChannel.GetPassiveDataStream(m_transferDirection);

                m_ftpFileCommandRoutine(m_remoteFile);
                m_streamCopyRoutine(remoteStream, localStream);

                remoteStream.Close();
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
                if (remoteStream != null)
                    remoteStream.Close();

                if (localStream != null)
                    localStream.Close();
            }
        }

        internal void StartAsyncTransfer()
        {
#if ThreadTracking
            ManagedThread thread = new ManagedThread(TransferThreadProc);
            thread.Name = "PCS.Net.Ftp.FileTransferer.TransferThreadProc() [" + m_remoteFile + "]";
#else
            Thread thread = new Thread(TransferThreadProc);
            thread.Name = "Transfer file thread: " + m_remoteFile;
#endif
            thread.Start();
        }

        private void TestTransferResult()
        {
            int responseCode = m_session.ControlChannel.LastResponse.Code;

            if (responseCode == Response.ClosingDataChannel)
                return;

            if (responseCode == Response.RequestFileActionComplete)
                return;

            throw new DataTransferException("Failed to transfer file.", m_session.ControlChannel.LastResponse);
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
                    m_session.Host.OnFileTransferProgress(m_totalBytes, m_totalBytesTransfered, m_transferDirection);
                    bytesReadFromLastProgressEvent = 0;
                }

                dest.Write(buffer, 0, byteRead);
                byteRead = source.Read(buffer, 0, 4 * 1024);
            }
        }

        #endregion
    }
}