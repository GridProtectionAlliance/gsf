//*******************************************************************************************************
//  DataStream.cs
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
using System.Net.Sockets;

namespace PCS.Net.Ftp
{
    public class DataStream : Stream
    {
        #region [ Members ]

        // Fields
        private ControlChannel m_ctrl;
        private SessionConnected m_session;
        private TcpClient m_tcpClient;
        private Stream m_stream;
        private bool m_userAbort;

        #endregion

        #region [ Constructors ]

        internal DataStream(ControlChannel ctrl, TcpClient client)
        {
            m_session = ctrl.Session;
            m_ctrl = ctrl;
            m_tcpClient = client;
            m_stream = client.GetStream();
            m_session.BeginDataTransfer(this);
        }

        #endregion

        #region [ Properties ]

        internal bool IsClosed
        {
            get
            {
                return (m_tcpClient == null);
            }
        }

        internal ControlChannel ControlChannel
        {
            get
            {
                return m_ctrl;
            }
        }

        public override bool CanRead
        {
            get
            {
                return m_stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return m_stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return m_stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return m_stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return m_stream.Position;
            }
            set
            {
                m_stream.Position = value;
            }
        }

        #endregion

        #region [ Methods ]

        public override void Close()
        {
            if (!IsClosed)
            {
                CloseConnection();
                m_ctrl.RefreshResponse();
                m_ctrl.Session.EndDataTransfer();
            }
        }

        internal void Abort()
        {
            m_userAbort = true;
        }

        private void CloseConnection()
        {
            m_stream.Close();
            m_tcpClient.Close();
            m_tcpClient = null;
        }

        public override void Flush()
        {
            m_stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_stream.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return m_stream.Read(buffer, offset, count);
            }
            finally
            {
                if (m_userAbort)
                    throw new UserAbortException();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                m_stream.Write(buffer, offset, count);
            }
            finally
            {
                if (m_userAbort)
                    throw new UserAbortException();
            }
        }

        public override int ReadByte()
        {
            try
            {
                return m_stream.ReadByte();
            }
            finally
            {
                if (m_userAbort)
                    throw new UserAbortException();
            }
        }

        public override void WriteByte(byte b)
        {
            try
            {
                m_stream.WriteByte(b);
            }
            finally
            {
                if (m_userAbort)
                    throw new UserAbortException();
            }
        }

        public override void SetLength(long len)
        {
            m_stream.SetLength(len);
        }

        #endregion
    }
}