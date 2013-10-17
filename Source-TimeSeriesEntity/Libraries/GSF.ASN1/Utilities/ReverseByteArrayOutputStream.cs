//******************************************************************************************************
//  ReverseByteArrayOutputStream.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/24/2013 - J. Ritchie Carroll
//       Derived original version of source code from BinaryNotes (http://bnotes.sourceforge.net).
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/*
     Copyright 2006-2011 Abdulla Abdurakhmanov (abdulla@latestbit.com)
     Original sources are available at www.latestbit.com
 
     Licensed under the Apache License, Version 2.0 (the "License");
     you may not use this file except in compliance with the License.
     You may obtain a copy of the License at
 
             http://www.apache.org/licenses/LICENSE-2.0
 
     Unless required by applicable law or agreed to in writing, software
     distributed under the License is distributed on an "AS IS" BASIS,
     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     See the License for the specific language governing permissions and
     limitations under the License.
 */

#endregion

using System;
using System.IO;

namespace GSF.ASN1.Utilities
{
    /// <summary>
    /// This class implements an output stream in which the data is written
    /// into a reverse byte array. The buffer automatically grows as data is
    /// written to it. The data can be retrieved using <code>toByteArray()</code>
    /// and <code>toString()</code>. Closing a <tt>ByteArrayOutputStream</tt>
    /// has no effect. The methods in this class can be called after the stream
    /// has been closed without generating an <tt>IOException</tt>.
    /// </summary>
    public class ReverseByteArrayOutputStream : Stream
    {
        private byte[] buf = new byte[1024];
        private int count;

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return count;
            }
        }

        public override long Position
        {
            get
            {
                return count;
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void WriteTo(Stream stream)
        {
            byte[] bufTmp = ToArray();
            stream.Write(bufTmp, 0, bufTmp.Length);
        }

        public byte[] ToArray()
        {
            byte[] newbuf = new byte[count];
            Array.Copy(buf, buf.Length - count, newbuf, 0, count);
            return newbuf;
        }

        public char[] ToCharArray()
        {
            char[] newbuf = new char[count];
            Array.Copy(buf, buf.Length - count, newbuf, 0, count);
            return newbuf;
        }

        public override String ToString()
        {
            return new String(ToCharArray());
        }

        protected void ResizeBuffer(int newcount)
        {
            if (newcount > buf.Length)
            {
                byte[] newbuf = new byte[Math.Max(buf.Length << 1, newcount)];
                //Array.Copy(buf, 0, newbuf, 0, count);
                Array.Copy(buf, buf.Length - count, newbuf, newbuf.Length - count, count);
                buf = newbuf;
            }
        }

        public void WriteByte(int b)
        {
            WriteByte((byte)b);
        }

        public override void WriteByte(byte b)
        {
            lock (this)
            {
                int newcount = (int)count + 1;
                ResizeBuffer(newcount);
                buf[buf.Length - 1 - count] = b;
                count = newcount;
            }
        }

        public override void Write(byte[] b, int off, int len)
        {
            lock (this)
            {
                if ((off < 0) || (off > b.Length) || (len < 0) || ((off + len) > b.Length) || ((off + len) < 0))
                {
                    throw new IndexOutOfRangeException();
                }
                else if (len == 0)
                {
                    return;
                }
                int newcount = count + len;
                ResizeBuffer(newcount);
                Array.Copy(b, off, buf, buf.Length - count - len, len);
                count = newcount;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}