using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net.Sockets;

// James Ritchie Carroll - 2003


namespace TVA
{
    namespace Net
    {
        namespace Ftp
        {


            public class DataStream : Stream
            {



                private ControlChannel m_ctrl;
                private SessionConnected m_session;
                private System.Net.Sockets.TcpClient m_tcpClient;
                private Stream m_stream;
                private bool m_userAbort;

                internal DataStream(ControlChannel ctrl, System.Net.Sockets.TcpClient client)
                {

                    m_session = ctrl.Session;
                    m_ctrl = ctrl;
                    m_tcpClient = client;
                    m_stream = client.GetStream();
                    m_session.BeginDataTransfer(this);

                }

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

                internal bool IsClosed
                {
                    get
                    {
                        return m_tcpClient == null;
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
                        {
                            throw (new UserAbortException());
                        }
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
                        {
                            throw (new UserAbortException());
                        }
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
                        {
                            throw (new UserAbortException());
                        }
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
                        {
                            throw (new UserAbortException());
                        }
                    }

                }

                public override void SetLength(long len)
                {

                    m_stream.SetLength(len);

                }

            }

            public class InputDataStream : DataStream
            {



                internal InputDataStream(ControlChannel ctrl, System.Net.Sockets.TcpClient client)
                    : base(ctrl, client)
                {


                }

                public override void Write(byte[] buffer, int offset, int count)
                {

                    throw (new NotSupportedException());

                }

                public override void WriteByte(byte b)
                {

                    throw (new NotSupportedException());

                }

                public override bool CanWrite
                {
                    get
                    {
                        return false;
                    }
                }

            }

            public class OutputDataStream : DataStream
            {



                internal OutputDataStream(ControlChannel ctrl, System.Net.Sockets.TcpClient client)
                    : base(ctrl, client)
                {


                }

                public override int Read(byte[] buffer, int offset, int count)
                {

                    throw (new NotSupportedException());

                }

                public override int ReadByte()
                {

                    throw (new NotSupportedException());

                }

                public override bool CanRead
                {
                    get
                    {
                        return false;
                    }
                }

            }

        }
    }
}
