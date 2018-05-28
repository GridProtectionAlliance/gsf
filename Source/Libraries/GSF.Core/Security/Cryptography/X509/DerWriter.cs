//******************************************************************************************************
//  DerWriter.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/27/2018 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GSF.Security.Cryptography.X509
{
    internal class DerWriter
    {
        private enum SectionMode
        {
            Sequence,
            Set,
            BitString,
            Tagged,
            Octet
        }
        private class Section : IDisposable
        {
            private DerWriter m_caller;
            private SectionMode m_mode;
            public Section(DerWriter caller, SectionMode mode)
            {
                m_caller = caller;
                m_mode = mode;
            }

            public void Dispose()
            {
                m_caller?.EndSection(m_mode);
                m_caller = null;
            }
        }
        private Stack<MemoryStream> m_stack = new Stack<MemoryStream>();

        private const int Integer = 0x02;
        private const int BitString = 0x03;
        private const int OctetString = 0x04;
        private const int Null = 0x05;
        private const int ObjectIdentifier = 0x06;
        private const int Sequence = 0x10;
        private const int Set = 0x11;

        private const int GeneralizedTime = 0x18;
        private const int Utf8String = 0x0c;

        private const int Constructed = 0x20;
        private const int Tagged = 0x80;

        private MemoryStream m_ms;

        public DerWriter()
        {
            m_ms = new MemoryStream();
        }

        private void EndSection(SectionMode mode)
        {
            byte[] data = m_ms.ToArray();
            m_ms = m_stack.Pop();
            switch (mode)
            {
                case SectionMode.Sequence:
                    WriteSequence(data);
                    break;
                case SectionMode.Set:
                    WriteSet(data);
                    break;
                case SectionMode.BitString:
                    Write(data);
                    break;
                case SectionMode.Octet:
                    WriteOctetString(data);
                    break;
                case SectionMode.Tagged:
                    WriteLength(data.Length);
                    m_ms.Write(data, 0, data.Length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

        }

        private void WriteLength(int length)
        {
            if (length > 127)
            {
                int size = 1;
                uint val = (uint)length;

                while ((val >>= 8) != 0)
                {
                    size++;
                }

                m_ms.WriteByte((byte)(size | 0x80));

                for (int i = (size - 1) * 8; i >= 0; i -= 8)
                {
                    m_ms.WriteByte((byte)(length >> i));
                }
            }
            else
            {
                m_ms.WriteByte((byte)length);
            }
        }

        private void WriteEncoded(int tag, byte[] bytes)
        {
            m_ms.WriteByte((byte)tag);
            WriteLength(bytes.Length);
            m_ms.Write(bytes, 0, bytes.Length);
        }

        public void Write(byte[] bitString)
        {
            m_ms.WriteByte(BitString);
            WriteLength(bitString.Length + 1);
            m_ms.WriteByte((byte)0);
            m_ms.Write(bitString, 0, bitString.Length);
        }

        public void Write(DateTime time)
        {
            WriteEncoded(GeneralizedTime, Encoding.ASCII.GetBytes(time.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "Z"));
        }

        public void WriteInteger(byte[] bigInt)
        {
            for (int x = 0; x < bigInt.Length; x++)
            {
                if (bigInt[x] != 0)
                {
                    if (bigInt[x] > 127) //Value is negative
                        x--;
                    byte[] newData = new byte[bigInt.Length - x];

                    if (x == -1)
                    {
                        Array.Copy(bigInt, 0, newData, 1, newData.Length-1);
                    }
                    else
                    {
                        Array.Copy(bigInt, x, newData, 0, newData.Length);
                    }

                    WriteEncoded(Integer, newData);
                    return;
                }
            }
            //The value is 0
            WriteEncoded(Integer, new byte[1]);
        }

        public void WriteInteger(long value)
        {
            WriteInteger(ToBytes(value));
        }
        private static byte[] ToBytes(long value)
        {
            if (value < 0)
                throw new Exception("Value must be positive");

            byte[] rv = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(rv);
            }
            return rv;
        }
        public void WriteNull()
        {
            WriteEncoded(Null, new byte[0]);
        }

        public IDisposable BeginTaggedObject(int tag)
        {
            m_ms.WriteByte((byte)(Constructed | Tagged | tag));
            m_stack.Push(m_ms);
            m_ms = new MemoryStream();
            return new Section(this, SectionMode.Tagged);
        }

        public IDisposable BeginSequence()
        {
            m_stack.Push(m_ms);
            m_ms = new MemoryStream();
            return new Section(this, SectionMode.Sequence);
        }

        public IDisposable BeginOctetString()
        {
            m_stack.Push(m_ms);
            m_ms = new MemoryStream();
            return new Section(this, SectionMode.Octet);
        }

        public void WriteOctetString(byte[] data)
        {
            m_ms.WriteByte(OctetString);
            WriteLength(data.Length);
            m_ms.Write(data, 0, data.Length);

        }

        public IDisposable BeginBitString()
        {
            m_stack.Push(m_ms);
            m_ms = new MemoryStream();
            return new Section(this, SectionMode.BitString);
        }

        public IDisposable BeginSet()
        {
            m_stack.Push(m_ms);
            m_ms = new MemoryStream();
            return new Section(this, SectionMode.Set);
        }

        public void WriteOID(string identifier)
        {
            string[] items = identifier.Split('.');

            var ms = new MemoryStream();
            for (int x = 1; x < items.Length; x++)
            {
                long value;
                if (x == 1)
                    value = int.Parse(items[0]) * 40 + int.Parse(items[1]);
                else
                    value = int.Parse(items[x]);

                WriteField(ms, value);
            }
            WriteEncoded(ObjectIdentifier, ms.ToArray());
        }

        private void WriteField(MemoryStream stream, long fieldValue)
        {
            //Write a 7-bit big endian value.
            byte[] result = new byte[9];
            int pos = 8;
            result[pos] = (byte)(fieldValue & 127);
            while (fieldValue >= 128)
            {
                fieldValue >>= 7;
                pos--;
                result[pos] = (byte)((fieldValue & 127) | 128);
            }
            stream.Write(result, pos, 9 - pos);
        }

        private void WriteSequence(byte[] data)
        {
            WriteEncoded(Sequence | Constructed, data);
        }

        private void WriteSet(byte[] data)
        {
            WriteEncoded(Set | Constructed, data);
        }

        public void Write(string value)
        {
            WriteEncoded(Utf8String, Encoding.UTF8.GetBytes(value));
        }

        public byte[] ToArray()
        {
            return m_ms.ToArray();
        }
    }
}
