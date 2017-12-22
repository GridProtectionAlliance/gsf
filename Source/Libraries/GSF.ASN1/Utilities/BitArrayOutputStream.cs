//******************************************************************************************************
//  BitArrayOutputStream.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
    public class BitArrayOutputStream : Stream
    {
        private byte[] buf = new byte[1024];
        private int count;
        internal byte currentBit = 0;


        public BitArrayOutputStream()
        {
        }

        public BitArrayOutputStream(int initialSize)
        {
            if (initialSize > 100)
                buf = new byte[initialSize];
        }

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

        public virtual void align()
        {
            currentBit = 0;
        }

        protected void ResizeBuffer(int newcount)
        {
            if (newcount > buf.Length)
            {
                byte[] newbuf = new byte[Math.Max(buf.Length << 1, newcount)];
                Array.Copy(buf, 0, newbuf, 0, count);
                buf = newbuf;
            }
        }

        protected void pushByteToBuffer(byte bt)
        {
            int newcount = count + 1;
            ResizeBuffer(newcount);
            buf[count] = bt;
            count = newcount;
        }


		public override void  WriteByte(byte b)
        {
            lock (this)
            {
                if (currentBit == 0) {
                    pushByteToBuffer( (byte) b);
                }
                else
                {
                    byte lBt = buf[count - 1];
                    byte nBt = (byte) (lBt | (b >> currentBit));
                    buf[count - 1] = nBt;
                    lBt = (byte) (b << (8 - currentBit));
                    pushByteToBuffer(lBt);
                }
            }
        }


        public void WriteByte(int b)
        {
            base.WriteByte((byte)b);
        }

        public override void Write(byte[] b, int off, int len)
        {
            if (len <= 0)
                return;
            if (currentBit == 0)
            {
                int newcount = count + len;
                ResizeBuffer(newcount);
                Array.Copy(b, off, buf, count, len);
                count = newcount;
            }
            else
            {
                byte lBt = buf[count - 1];
                for (int i = off; i < off + len; i++)
                {
                    int bufByte = b[i] < 0 ? 256 + b[i] : b[i];
                    byte nBt = (byte)(lBt | (bufByte >> currentBit));
                    if (i == off)
                    {
                        buf[count - 1] = nBt;
                    }
                    else
                    {
                        pushByteToBuffer(nBt);
                    }
                    lBt = (byte)(bufByte << (8 - currentBit));
                }
                pushByteToBuffer((byte)lBt);
            }
        }

        public virtual void writeBit(bool val)
        {
            writeBit(val ? 1 : 0);
        }

        public virtual void writeBit(int bit)
        {
            lock (this)
            {
                if (currentBit < 8 && currentBit > 0)
                {
                    if (bit != 0)
                    {
                        buf[count - 1] |= (byte)(0x80 >> currentBit);
                    }
                }
                else
                {
                    pushByteToBuffer((byte)(bit == 0 ? 0 : 0x80));
                }
                currentBit++;
                if (currentBit >= 8)
                {
                    currentBit = 0;
                }
            }
        }

        public void writeBits(int bt, int count)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                writeBit((bt >> i) & 0x1);
            }
        }


        public byte[] ToArray()
        {
            byte[] newbuf = new byte[count];
            Array.Copy(buf, 0, newbuf, 0, count);
            return newbuf;
        }


        public void WriteTo(Stream stream)
        {
            byte[] bufTmp = ToArray();
            stream.Write(bufTmp, 0, bufTmp.Length);
        }

        public void reset()
        {
            currentBit = 0;
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