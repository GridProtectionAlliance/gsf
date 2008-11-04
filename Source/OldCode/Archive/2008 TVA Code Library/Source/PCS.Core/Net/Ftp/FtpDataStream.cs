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
    /// <summary>
    /// FTP data stream.
    /// </summary>
    public class FtpDataStream : Stream
    {
        #region [ Members ]

        // Fields
        private FtpControlChannel m_ctrl;
        private FtpSessionConnected m_session;
        private TcpClient m_tcpClient;
        private Stream m_stream;
        private bool m_userAbort;

        #endregion

        #region [ Constructors ]

        internal FtpDataStream(FtpControlChannel ctrl, TcpClient client)
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

        internal FtpControlChannel ControlChannel
        {
            get
            {
                return m_ctrl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        public override bool CanRead
        {
            get
            {
                return m_stream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        public override bool CanSeek
        {
            get
            {
                return m_stream.CanSeek;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        public override bool CanWrite
        {
            get
            {
                return m_stream.CanWrite;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Stream does not support seeking.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Length
        {
            get
            {
                return m_stream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">Stream does not support seeking.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
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

        /// <summary>
        /// Closes the FTP stream and releases any resources associated with the stream.
        /// </summary>
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

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Flush()
        {
            m_stream.Flush();
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">
        /// A value of type <see cref="System.IO.SeekOrigin"/> indicating the reference
        /// point used to obtain the new position.
        /// </param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">Stream does not support seeking.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_stream.Seek(offset, origin);
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the
        /// position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the <paramref name="buffer"/>
        /// contains the specified byte array with the values between
        /// <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1)
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">
        /// The zero-based byte offset in <paramref name="buffer"/> at which to begin
        /// storing the data read from the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <returns>
        /// The total number of bytes read into the <paramref name="buffer"/>. This can be less than the 
        /// number of bytes requested if that many bytes are not currently available, or zero (0) if the
        /// end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return m_stream.Read(buffer, offset, count);
            }
            finally
            {
                if (m_userAbort)
                    throw new FtpUserAbortException();
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the <paramref name="buffer"/>
        /// contains the specified byte array with the values between
        /// <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1)
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">
        /// The zero-based byte offset in <paramref name="buffer"/> at which to begin
        /// storing the data read from the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                m_stream.Write(buffer, offset, count);
            }
            finally
            {
                if (m_userAbort)
                    throw new FtpUserAbortException();
            }
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream
        /// by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        public override int ReadByte()
        {
            try
            {
                return m_stream.ReadByte();
            }
            finally
            {
                if (m_userAbort)
                    throw new FtpUserAbortException();
            }
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position
        /// within the stream by one byte.
        /// </summary>
        /// <param name="b">The byte to write to the stream.</param>
        public override void WriteByte(byte b)
        {
            try
            {
                m_stream.WriteByte(b);
            }
            finally
            {
                if (m_userAbort)
                    throw new FtpUserAbortException();
            }
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="len">The desired length of the current stream in bytes.</param>
        public override void SetLength(long len)
        {
            m_stream.SetLength(len);
        }

        #endregion
    }
}