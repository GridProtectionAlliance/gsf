//******************************************************************************************************
//  FtpDataStream.cs - Gbtc
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

using System.IO;
using System.Net.Sockets;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// FTP data stream.
    /// </summary>
    public class FtpDataStream : Stream
    {
        #region [ Members ]

        // Fields
        private readonly FtpControlChannel m_ctrl;
        private readonly FtpSessionConnected m_session;
        private TcpClient m_tcpClient;
        private readonly Stream m_stream;
        private bool m_userAbort;

        #endregion

        #region [ Constructors ]

        internal FtpDataStream(FtpControlChannel ctrl, TcpClient client)
        {
            m_session = ctrl.Session;
            m_ctrl = ctrl;
            m_tcpClient = client;
            m_stream = client.GetStream();
            m_stream.ReadTimeout = ctrl.Timeout;
            m_stream.WriteTimeout = ctrl.Timeout;
            TryBeginDataTransfer();
        }

        #endregion

        #region [ Properties ]

        internal bool IsClosed
        {
            get
            {
                return ((object)m_tcpClient == null);
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
            Close(false);
        }

        internal void Close(bool error)
        {
            if (!IsClosed)
            {
                CloseConnection();

                if (!error)
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
            if (m_userAbort)
                throw new FtpUserAbortException();

            return m_stream.Read(buffer, offset, count);
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
            if (m_userAbort)
                throw new FtpUserAbortException();

            m_stream.Write(buffer, offset, count);
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
            if (m_userAbort)
                throw new FtpUserAbortException();

            return m_stream.ReadByte();
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position
        /// within the stream by one byte.
        /// </summary>
        /// <param name="b">The byte to write to the stream.</param>
        public override void WriteByte(byte b)
        {
            if (m_userAbort)
                throw new FtpUserAbortException();

            m_stream.WriteByte(b);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="len">The desired length of the current stream in bytes.</param>
        public override void SetLength(long len)
        {
            m_stream.SetLength(len);
        }

        private void TryBeginDataTransfer()
        {
            try
            {
                m_session.BeginDataTransfer(this);
            }
            catch
            {
                CloseConnection();
                throw;
            }
        }

        #endregion
    }
}