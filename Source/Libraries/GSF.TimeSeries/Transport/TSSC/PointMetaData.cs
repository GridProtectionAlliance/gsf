//******************************************************************************************************
//  PointMetaData.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  12/02/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries.Transport.TSSC
{
    internal class PointMetaData
    {
        const uint Bits28 = 0xFFFFFFFu;
        const uint Bits24 = 0xFFFFFFu;
        const uint Bits20 = 0xFFFFFu;
        const uint Bits16 = 0xFFFFu;
        const uint Bits12 = 0xFFFu;
        const uint Bits8 = 0xFFu;
        const uint Bits4 = 0xFu;
        const uint Bits0 = 0x0u;

        public ushort SignalID;
        /// <summary>
        /// The local reference ID
        /// </summary>
        public readonly int ReferenceId;
        private readonly byte[] m_commandStats;

        public int PrevNextPointId1;

        public uint PrevQuality1;
        public uint PrevQuality2;

        private int m_commandsSentSinceLastChange = 0;

        private byte Mode;

        //Mode 1 means no prefix.
        private byte Mode2_1;

        private byte Mode3_1;
        private byte Mode3_01;

        private byte Mode4_1;
        private byte Mode4_01;
        private byte Mode4_001;

        private int m_startupMode = 0;

        private ByteBuffer Buffer;
        private BitStream BitStream;

        public uint PrevValue1;
        public uint PrevValue2;
        public uint PrevValue3;

        public PointMetaData(ByteBuffer buffer, BitStream bitStream, int referenceId)
        {
            Buffer = buffer;
            BitStream = bitStream;
            m_commandsSentSinceLastChange = 0;
            m_commandStats = new byte[64];
            Mode = 1;
            ReferenceId = referenceId;
            Mode = 4;
            Mode4_1 = MeasurementStreamCodes.Value1;
            Mode4_01 = MeasurementStreamCodes.Value2;
            Mode4_001 = MeasurementStreamCodes.Value3;
        }

        public unsafe void ReadValue(int code, out float outValue)
        {
            uint valueRaw = 0;

            if (code == MeasurementStreamCodes.Value1)
            {
                valueRaw = PrevValue1;
            }
            else if (code == MeasurementStreamCodes.Value2)
            {
                valueRaw = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else if (code == MeasurementStreamCodes.Value3)
            {
                valueRaw = PrevValue3;
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else if (code == MeasurementStreamCodes.ValueZero)
            {
                valueRaw = 0;
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else
            {
                switch (code)
                {
                    case MeasurementStreamCodes.ValueXOR4:
                        valueRaw = (uint)BitStream.ReadBits4() ^ PrevValue1;
                        break;
                    case MeasurementStreamCodes.ValueXOR8:
                        valueRaw = Buffer.Data[Buffer.Position] ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 1;
                        break;
                    case MeasurementStreamCodes.ValueXOR12:
                        valueRaw = (uint)BitStream.ReadBits4() ^ (uint)(Buffer.Data[Buffer.Position] << 4) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 1;
                        break;
                    case MeasurementStreamCodes.ValueXOR16:
                        valueRaw = Buffer.Data[Buffer.Position] ^ (uint)(Buffer.Data[Buffer.Position + 1] << 8) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 2;
                        break;
                    case MeasurementStreamCodes.ValueXOR20:
                        valueRaw = (uint)BitStream.ReadBits4() ^ (uint)(Buffer.Data[Buffer.Position] << 4) ^ (uint)(Buffer.Data[Buffer.Position + 1] << 12) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 2;
                        break;
                    case MeasurementStreamCodes.ValueXOR24:
                        valueRaw = Buffer.Data[Buffer.Position] ^ (uint)(Buffer.Data[Buffer.Position + 1] << 8) ^ (uint)(Buffer.Data[Buffer.Position + 2] << 16) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 3;
                        break;
                    case MeasurementStreamCodes.ValueXOR28:
                        valueRaw = (uint)BitStream.ReadBits4() ^ (uint)(Buffer.Data[Buffer.Position] << 4) ^ (uint)(Buffer.Data[Buffer.Position + 1] << 12) ^ (uint)(Buffer.Data[Buffer.Position + 2] << 20) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 3;
                        break;
                    case MeasurementStreamCodes.ValueXOR32:
                        valueRaw = Buffer.Data[Buffer.Position] ^ (uint)(Buffer.Data[Buffer.Position + 1] << 8) ^ (uint)(Buffer.Data[Buffer.Position + 2] << 16) ^ (uint)(Buffer.Data[Buffer.Position + 3] << 24) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 4;
                        break;
                    default:
                        throw new Exception("Code not valid");
                }

                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }

            outValue = *(float*)&valueRaw;
        }


        public unsafe void WriteValue(float currentValue)
        {
            uint value = *(uint*)&currentValue;

            if (PrevValue1 == value)
            {
                BitStream.WriteCode(MeasurementStreamCodes.Value1);
            }
            else if (PrevValue2 == value)
            {
                BitStream.WriteCode(MeasurementStreamCodes.Value2);
                PrevValue2 = PrevValue1;
                PrevValue1 = value;
            }
            else if (PrevValue3 == value)
            {
                BitStream.WriteCode(MeasurementStreamCodes.Value3);
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = value;
            }
            else if (value == 0)
            {
                BitStream.WriteCode(MeasurementStreamCodes.ValueZero);
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = 0;
            }
            else
            {
                uint bitsChanged = value ^ PrevValue1;

                if (bitsChanged == Bits0)
                {
                    throw new Exception("Programming Error");
                }
                else if (bitsChanged <= Bits4)
                {
                    BitStream.WriteCode4(MeasurementStreamCodes.ValueXOR4, (byte)bitsChanged);
                }
                else if (bitsChanged <= Bits8)
                {
                    BitStream.WriteCode(MeasurementStreamCodes.ValueXOR8);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Position++;
                }
                else if (bitsChanged <= Bits12)
                {
                    BitStream.WriteCode4(MeasurementStreamCodes.ValueXOR12, (byte)bitsChanged);

                    Buffer.Data[Buffer.Position] = (byte)(bitsChanged >> 4);
                    Buffer.Position++;
                }
                else if (bitsChanged <= Bits16)
                {
                    BitStream.WriteCode(MeasurementStreamCodes.ValueXOR16);
                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Position = Buffer.Position + 2;
                }
                else if (bitsChanged <= Bits20)
                {
                    BitStream.WriteCode4(MeasurementStreamCodes.ValueXOR20, (byte)bitsChanged);

                    Buffer.Data[Buffer.Position] = (byte)(bitsChanged >> 4);
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 12);
                    Buffer.Position = Buffer.Position + 2;
                }
                else if (bitsChanged <= Bits24)
                {
                    BitStream.WriteCode(MeasurementStreamCodes.ValueXOR24);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Position = Buffer.Position + 3;
                }
                else if (bitsChanged <= Bits28)
                {
                    BitStream.WriteCode4(MeasurementStreamCodes.ValueXOR28, (byte)bitsChanged);

                    Buffer.Data[Buffer.Position] = (byte)(bitsChanged >> 4);
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 12);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 20);
                    Buffer.Position = Buffer.Position + 3;
                }
                else
                {
                    BitStream.WriteCode(MeasurementStreamCodes.ValueXOR32);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Data[Buffer.Position + 3] = (byte)(bitsChanged >> 24);
                    Buffer.Position = Buffer.Position + 4;
                }

                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = value;
            }
        }

        public void TransformCode(ref int code, out int len)
        {
            m_commandsSentSinceLastChange++;
            m_commandStats[code]++;

            switch (Mode)
            {
                case 1:
                    len = 6;
                    break;
                case 2:
                    if (code == Mode2_1)
                    {
                        code = 1;
                        len = 1;
                    }
                    else
                    {
                        len = 7;
                    }
                    break;
                case 3:
                    if (code == Mode3_1)
                    {
                        code = 1;
                        len = 1;
                    }
                    else if (code == Mode3_01)
                    {
                        code = 1;
                        len = 2;
                    }
                    else
                    {
                        len = 8;
                    }
                    break;
                case 4:
                    if (code == Mode4_1)
                    {
                        code = 1;
                        len = 1;
                    }
                    else if (code == Mode4_01)
                    {
                        code = 1;
                        len = 2;
                    }
                    else if (code == Mode4_001)
                    {
                        code = 1;
                        len = 3;
                    }
                    else
                    {
                        len = 9;
                    }
                    break;
                default:
                    throw new Exception("Coding Error");
            }

            if (m_startupMode == 0 && m_commandsSentSinceLastChange > 5)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 1 && m_commandsSentSinceLastChange > 20)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 2 && m_commandsSentSinceLastChange > 100)
            {
                AdaptCommands2();
            }
        }

        public int ReadCode(BitStream bitStream)
        {
            int code = 0;
            switch (Mode)
            {
                case 1:
                    code = bitStream.ReadBits6();
                    break;
                case 2:
                    if (bitStream.ReadBit() == 1)
                    {
                        code = Mode2_1;
                    }
                    else
                    {
                        code = bitStream.ReadBits6();
                    }
                    break;
                case 3:
                    if (bitStream.ReadBit() == 1)
                    {
                        code = Mode3_1;
                    }
                    else if (bitStream.ReadBit() == 1)
                    {
                        code = Mode3_01;
                    }
                    else
                    {
                        code = bitStream.ReadBits6();
                    }
                    break;
                case 4:
                    if (bitStream.ReadBit() == 1)
                    {
                        code = Mode4_1;
                    }
                    else if (bitStream.ReadBit() == 1)
                    {
                        code = Mode4_01;
                    }
                    else if (bitStream.ReadBit() == 1)
                    {
                        code = Mode4_001;
                    }
                    else
                    {
                        code = bitStream.ReadBits6();
                    }
                    break;
                default:
                    throw new Exception("Unsupported compression mode");
            }
            m_commandStats[code]++;
            m_commandsSentSinceLastChange++;

            if (m_startupMode == 0 && m_commandsSentSinceLastChange > 5)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 1 && m_commandsSentSinceLastChange > 20)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 2 && m_commandsSentSinceLastChange > 100)
            {
                AdaptCommands2();
            }
            return code;
        }

        private void AdaptCommands2()
        {
            byte code1 = 0;
            int count1 = 0;

            byte code2 = 1;
            int count2 = 0;

            byte code3 = 2;
            int count3 = 0;

            int total = 0;

            for (int x = 0; x < m_commandStats.Length; x++)
            {
                int cnt = m_commandStats[x];
                m_commandStats[x] = 0;

                total += cnt;

                if (cnt > count3)
                {
                    if (cnt > count1)
                    {
                        code3 = code2;
                        count3 = count2;

                        code2 = code1;
                        count2 = count1;

                        code1 = (byte)x;
                        count1 = cnt;
                    }
                    else if (cnt > count2)
                    {
                        code3 = code2;
                        count3 = count2;

                        code2 = (byte)x;
                        count2 = cnt;
                    }
                    else
                    {
                        code3 = (byte)x;
                        count3 = cnt;
                    }
                }
            }

            int mode1Size = total * 6;
            int mode2Size = count1 * 1 + (total - count1) * 7;
            int mode3Size = count1 * 1 + count2 * 2 + (total - count1 - count2) * 8;
            int mode4Size = count1 * 1 + count2 * 2 + count3 * 3 + (total - count1 - count2 - count3) * 9;

            int minSize = int.MaxValue;
            minSize = Math.Min(minSize, mode1Size);
            minSize = Math.Min(minSize, mode2Size);
            minSize = Math.Min(minSize, mode3Size);
            minSize = Math.Min(minSize, mode4Size);

            if (minSize == mode1Size)
            {
                Mode = 1;
            }
            else if (minSize == mode2Size)
            {
                Mode = 2;
                Mode2_1 = code1;
            }
            else if (minSize == mode3Size)
            {
                Mode = 3;
                Mode3_1 = code1;
                Mode3_01 = code2;
            }
            else if (minSize == mode4Size)
            {
                Mode = 4;
                Mode4_1 = code1;
                Mode4_01 = code2;
                Mode4_001 = code3;
            }
            else
            {
                throw new Exception("Coding Error");
            }

            m_commandsSentSinceLastChange = 0;
        }

    }
}