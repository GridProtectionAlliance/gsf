//******************************************************************************************************
//  BitArrayInputStream.cs - Gbtc
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
    public class BitArrayInputStream : Stream
    {
        private readonly Stream byteStream;
        private int currentBit, currentByte;

        public BitArrayInputStream(Stream byteStream)
        {
            this.byteStream = byteStream;
        }

        public override Boolean CanRead
        {
            get
            {
                return true;
            }
        }

        public override Boolean CanSeek
        {
            get
            {
                return false;
            }
        }

        public override Boolean CanWrite
        {
            get
            {
                return false;
            }
        }

        public override Int64 Length
        {
            get
            {
                return byteStream.Length;
            }
        }

        public override Int64 Position
        {
            get
            {
                return byteStream.Position;
            }

            set
            {
            }
        }

        public override int ReadByte()
        {
            if (currentBit == 0)
            {
                return byteStream.ReadByte();
            }
            else
            {
                int nextByte = byteStream.ReadByte();
                int result = ((currentByte << currentBit) | (nextByte >> (8 - currentBit))) & 0xFF;
                currentByte = nextByte;
                return result;
            }
        }

        public virtual int readBit()
        {
            lock (this)
            {
                if (currentBit == 0)
                {
                    currentByte = byteStream.ReadByte();
                }
                currentBit++;
                int result = currentByte >> (8 - currentBit) & 0x1;
                if (currentBit > 7)
                    currentBit = 0;
                return result;
            }
        }

        public virtual int readBits(int nBits)
        {
            lock (this)
            {
                int result = 0;
                for (int i = 0; i < nBits && i <= 32; i++)
                {
                    result = ((result << 1) | readBit());
                }
                return result;
            }
        }

        public virtual void skipUnreadedBits()
        {
            currentBit = 0;
        }

        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (currentBit == 0)
            {
                return byteStream.Read(buffer, offset, count);
            }
            else
            {
                int readCnt = 0;
                for (; readCnt < buffer.Length && readCnt < byteStream.Length && readCnt < count; readCnt++)
                {
                    buffer[readCnt] = (byte)ReadByte();
                }
                return readCnt;
            }
        }


        public override void Flush()
        {
        }

        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            return -1;
        }

        public override void SetLength(Int64 value)
        {
        }

        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
        }
    }
}