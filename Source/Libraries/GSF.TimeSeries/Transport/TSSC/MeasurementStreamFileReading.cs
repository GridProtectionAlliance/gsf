//******************************************************************************************************
//  MeasurementStreamFileReading.cs - Gbtc
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
using System.IO;
using GSF.Collection;

namespace GSF.TimeSeries.Transport.TSSC
{
    internal class MeasurementStreamFileReading
    {
        private class PointMetaData
        {
            public ushort PrevNextPointId1;
            public uint PrevQuality1;
            public uint PrevQuality2;
            public uint PrevValue1;
            public uint PrevValue2;
            public uint PrevValue3;

            private readonly byte[] m_commandStats;
            private int m_commandsSentSinceLastChange = 0;
            private byte m_mode;

            //Bit codes for the 4 modes of encoding. 
            //(Mode 1 means no prefix.)
            private byte m_mode21;

            private byte m_mode31;
            private byte m_mode301;

            private byte m_mode41;
            private byte m_mode401;
            private byte m_mode4001;
            private int m_startupMode = 0;

            private readonly MeasurementStreamFileReading m_parent;

            public PointMetaData(MeasurementStreamFileReading parent)
            {
                m_parent = parent;
                m_commandsSentSinceLastChange = 0;
                m_commandStats = new byte[32];
                m_mode = 4;
                m_mode41 = MeasurementStreamCodes.Value1;
                m_mode401 = MeasurementStreamCodes.Value2;
                m_mode4001 = MeasurementStreamCodes.Value3;
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
                            valueRaw = (uint)m_parent.ReadBits4() ^ PrevValue1;
                            break;
                        case MeasurementStreamCodes.ValueXOR8:
                            valueRaw = m_parent.m_data[m_parent.m_position] ^ PrevValue1;
                            m_parent.m_position = m_parent.m_position + 1;
                            break;
                        case MeasurementStreamCodes.ValueXOR12:
                            valueRaw = (uint)m_parent.ReadBits4() ^ (uint)(m_parent.m_data[m_parent.m_position] << 4) ^ PrevValue1;
                            m_parent.m_position = m_parent.m_position + 1;
                            break;
                        case MeasurementStreamCodes.ValueXOR16:
                            valueRaw = m_parent.m_data[m_parent.m_position] ^ (uint)(m_parent.m_data[m_parent.m_position + 1] << 8) ^ PrevValue1;
                            m_parent.m_position = m_parent.m_position + 2;
                            break;
                        case MeasurementStreamCodes.ValueXOR20:
                            valueRaw = (uint)m_parent.ReadBits4() ^ (uint)(m_parent.m_data[m_parent.m_position] << 4) ^ (uint)(m_parent.m_data[m_parent.m_position + 1] << 12) ^ PrevValue1;
                            m_parent.m_position = m_parent.m_position + 2;
                            break;
                        case MeasurementStreamCodes.ValueXOR24:
                            valueRaw = m_parent.m_data[m_parent.m_position] ^ (uint)(m_parent.m_data[m_parent.m_position + 1] << 8) ^ (uint)(m_parent.m_data[m_parent.m_position + 2] << 16) ^ PrevValue1;
                            m_parent.m_position = m_parent.m_position + 3;
                            break;
                        case MeasurementStreamCodes.ValueXOR28:
                            valueRaw = (uint)m_parent.ReadBits4() ^ (uint)(m_parent.m_data[m_parent.m_position] << 4) ^ (uint)(m_parent.m_data[m_parent.m_position + 1] << 12) ^ (uint)(m_parent.m_data[m_parent.m_position + 2] << 20) ^ PrevValue1;
                            m_parent.m_position = m_parent.m_position + 3;
                            break;
                        case MeasurementStreamCodes.ValueXOR32:
                            valueRaw = m_parent.m_data[m_parent.m_position] ^ (uint)(m_parent.m_data[m_parent.m_position + 1] << 8) ^ (uint)(m_parent.m_data[m_parent.m_position + 2] << 16) ^ (uint)(m_parent.m_data[m_parent.m_position + 3] << 24) ^ PrevValue1;
                            m_parent.m_position = m_parent.m_position + 4;
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

            public int ReadCode()
            {
                int code = 0;
                switch (m_mode)
                {
                    case 1:
                        code = m_parent.ReadBits5();
                        break;
                    case 2:
                        if (m_parent.ReadBit() == 1)
                        {
                            code = m_mode21;
                        }
                        else
                        {
                            code = m_parent.ReadBits5();
                        }
                        break;
                    case 3:
                        if (m_parent.ReadBit() == 1)
                        {
                            code = m_mode31;
                        }
                        else if (m_parent.ReadBit() == 1)
                        {
                            code = m_mode301;
                        }
                        else
                        {
                            code = m_parent.ReadBits5();
                        }
                        break;
                    case 4:
                        if (m_parent.ReadBit() == 1)
                        {
                            code = m_mode41;
                        }
                        else if (m_parent.ReadBit() == 1)
                        {
                            code = m_mode401;
                        }
                        else if (m_parent.ReadBit() == 1)
                        {
                            code = m_mode4001;
                        }
                        else
                        {
                            code = m_parent.ReadBits5();
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

                int mode1Size = total * 5;
                int mode2Size = count1 * 1 + (total - count1) * 6;
                int mode3Size = count1 * 1 + count2 * 2 + (total - count1 - count2) * 7;
                int mode4Size = count1 * 1 + count2 * 2 + count3 * 3 + (total - count1 - count2 - count3) * 8;

                int minSize = int.MaxValue;
                minSize = Math.Min(minSize, mode1Size);
                minSize = Math.Min(minSize, mode2Size);
                minSize = Math.Min(minSize, mode3Size);
                minSize = Math.Min(minSize, mode4Size);

                if (minSize == mode1Size)
                {
                    m_mode = 1;
                }
                else if (minSize == mode2Size)
                {
                    m_mode = 2;
                    m_mode21 = code1;
                }
                else if (minSize == mode3Size)
                {
                    m_mode = 3;
                    m_mode31 = code1;
                    m_mode301 = code2;
                }
                else if (minSize == mode4Size)
                {
                    m_mode = 4;
                    m_mode41 = code1;
                    m_mode401 = code2;
                    m_mode4001 = code3;
                }
                else
                {
                    throw new Exception("Coding Error");
                }

                m_commandsSentSinceLastChange = 0;
            }
        }

        private byte[] m_data;
        private int m_position;
        private int m_length;

        private long m_prevTimestamp1;
        private long m_prevTimestamp2;

        private long m_prevTimeDelta1;
        private long m_prevTimeDelta2;
        private long m_prevTimeDelta3;
        private long m_prevTimeDelta4;

        private PointMetaData m_lastPoint;
        private IndexedArray<PointMetaData> m_points;

        public MeasurementStreamFileReading()
        {
            m_points = new IndexedArray<PointMetaData>();
            m_data = EmptyArray<byte>.Empty;
            m_position = 0;
            m_length = 0;
            ClearBitStream();
            Reset();
        }

        public void Reset()
        {
            m_lastPoint = new PointMetaData(this);
            m_points.Clear();
            m_prevTimeDelta1 = long.MaxValue;
            m_prevTimeDelta2 = long.MaxValue;
            m_prevTimeDelta3 = long.MaxValue;
            m_prevTimeDelta4 = long.MaxValue;
            m_prevTimestamp1 = 0;
            m_prevTimestamp2 = 0;
        }

        public void Load(byte[] data, int startingPosition, int length)
        {
            m_data = data;
            m_position = startingPosition;
            m_length = startingPosition + length;
        }

        public bool TryGetMeasurement(out ushort id, out long timestamp, out uint quality, out float value)
        {
            PointMetaData nextPoint = null;

            if (m_position == m_length && BitStreamIsEmpty)
            {
                ClearBitStream();
                id = 0;
                timestamp = 0;
                quality = 0;
                value = 0;
                return false;
            }

            int code = m_lastPoint.ReadCode();

            if (code == MeasurementStreamCodes.EndOfStream)
            {
                ClearBitStream();
                id = 0;
                timestamp = 0;
                quality = 0;
                value = 0;
                return false;
            }

            if (code <= MeasurementStreamCodes.PointIDXOR16)
            {
                if (code == MeasurementStreamCodes.PointIDXOR4)
                {
                    m_lastPoint.PrevNextPointId1 ^= (ushort)ReadBits4();
                }
                else if (code == MeasurementStreamCodes.PointIDXOR8)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_data[m_position++];
                }
                else if (code == MeasurementStreamCodes.PointIDXOR12)
                {
                    m_lastPoint.PrevNextPointId1 ^= (ushort)ReadBits4();
                    m_lastPoint.PrevNextPointId1 ^= (ushort)(m_data[m_position++] << 4);
                }
                else if (code == MeasurementStreamCodes.PointIDXOR16)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_data[m_position++];
                    m_lastPoint.PrevNextPointId1 ^= (ushort)(m_data[m_position++] << 8);
                }
                else
                {
                    throw new Exception("Programming Error.");
                }

                code = m_lastPoint.ReadCode();
            }

            if (code < MeasurementStreamCodes.TimeDelta1Forward)
                throw new Exception("Expecting higher code");

            id = m_lastPoint.PrevNextPointId1;
            nextPoint = m_points[m_lastPoint.PrevNextPointId1];
            if (nextPoint == null)
            {
                nextPoint = new PointMetaData(this);
                m_points[id] = nextPoint;
                nextPoint.PrevNextPointId1 = (ushort)(id + 1);
            }

            quality = nextPoint.PrevQuality1;

            if (code <= MeasurementStreamCodes.TimeXOR7Bit)
            {
                if (code == MeasurementStreamCodes.TimeDelta1Forward)
                {
                    timestamp = m_prevTimestamp1 + m_prevTimeDelta1;
                }
                else if (code == MeasurementStreamCodes.TimeDelta2Forward)
                {
                    timestamp = m_prevTimestamp1 + m_prevTimeDelta2;
                }
                else if (code == MeasurementStreamCodes.TimeDelta3Forward)
                {
                    timestamp = m_prevTimestamp1 + m_prevTimeDelta3;
                }
                else if (code == MeasurementStreamCodes.TimeDelta4Forward)
                {
                    timestamp = m_prevTimestamp1 + m_prevTimeDelta4;
                }
                else if (code == MeasurementStreamCodes.TimeDelta1Reverse)
                {
                    timestamp = m_prevTimestamp1 - m_prevTimeDelta1;
                }
                else if (code == MeasurementStreamCodes.TimeDelta2Reverse)
                {
                    timestamp = m_prevTimestamp1 - m_prevTimeDelta2;
                }
                else if (code == MeasurementStreamCodes.TimeDelta3Reverse)
                {
                    timestamp = m_prevTimestamp1 - m_prevTimeDelta3;
                }
                else if (code == MeasurementStreamCodes.TimeDelta4Reverse)
                {
                    timestamp = m_prevTimestamp1 - m_prevTimeDelta4;
                }
                else if (code == MeasurementStreamCodes.Timestamp2)
                {
                    timestamp = m_prevTimestamp2;
                }
                else if (code == MeasurementStreamCodes.TimeXOR7Bit)
                {
                    timestamp = m_prevTimestamp1 ^ (long)Encoding7Bit.ReadUInt64(m_data, ref m_position);
                }
                else
                {
                    throw new Exception("Programming Error.");
                }

                //Save the smallest delta time
                long minDelta = Math.Abs(m_prevTimestamp1 - timestamp);

                if (minDelta < m_prevTimeDelta4 && minDelta != m_prevTimeDelta1 && minDelta != m_prevTimeDelta2 && minDelta != m_prevTimeDelta3)
                {
                    if (minDelta < m_prevTimeDelta1)
                    {
                        m_prevTimeDelta4 = m_prevTimeDelta3;
                        m_prevTimeDelta3 = m_prevTimeDelta2;
                        m_prevTimeDelta2 = m_prevTimeDelta1;
                        m_prevTimeDelta1 = minDelta;
                    }
                    else if (minDelta < m_prevTimeDelta2)
                    {
                        m_prevTimeDelta4 = m_prevTimeDelta3;
                        m_prevTimeDelta3 = m_prevTimeDelta2;
                        m_prevTimeDelta2 = minDelta;
                    }
                    else if (minDelta < m_prevTimeDelta3)
                    {
                        m_prevTimeDelta4 = m_prevTimeDelta3;
                        m_prevTimeDelta3 = minDelta;
                    }
                    else
                    {
                        m_prevTimeDelta4 = minDelta;
                    }
                }

                m_prevTimestamp2 = m_prevTimestamp1;
                m_prevTimestamp1 = timestamp;
                code = m_lastPoint.ReadCode();
            }
            else
            {
                timestamp = m_prevTimestamp1;
            }

            if (code < MeasurementStreamCodes.Quality2)
                throw new Exception("Expecting higher code");

            if (code <= MeasurementStreamCodes.Quality7Bit32)
            {
                if (code == MeasurementStreamCodes.Quality2)
                {
                    quality = nextPoint.PrevQuality2;
                }
                else if (code == MeasurementStreamCodes.Quality7Bit32)
                {
                    quality = Encoding7Bit.ReadUInt32(m_data, ref m_position);
                }
                nextPoint.PrevQuality2 = nextPoint.PrevQuality1;
                nextPoint.PrevQuality1 = quality;
                code = m_lastPoint.ReadCode();
            }
            else
            {
                quality = nextPoint.PrevQuality1;
            }

            if (code < MeasurementStreamCodes.Value1)
                throw new Exception("Programming Error. Expecting a value quality code.");

            nextPoint.ReadValue(code, out value);
            m_lastPoint = nextPoint;
            return true;
        }

        #region [ BitStream ]


        /// <summary>
        /// The number of bits in m_bitStreamCache that are valid. 0 Means the bitstream is empty.
        /// </summary>
        private int m_bitStreamCount;
        /// <summary>
        /// A cache of bits that need to be flushed to m_buffer when full. Bits filled starting from the right moving left.
        /// </summary>
        private int m_bitStreamCache;

        private bool BitStreamIsEmpty => m_bitStreamCount == 0;

        /// <summary>
        /// Resets the stream so it can be reused. All measurements must be registered again.
        /// </summary>
        private void ClearBitStream()
        {
            m_bitStreamCount = 0;
            m_bitStreamCache = 0;
        }

        private int ReadBit()
        {
            if (m_bitStreamCount == 0)
            {
                m_bitStreamCount = 8;
                m_bitStreamCache = m_data[m_position++];
            }
            m_bitStreamCount--;
            return (m_bitStreamCache >> m_bitStreamCount) & 1;
        }

        private int ReadBits4()
        {
            return ReadBit() << 3 | ReadBit() << 2 | ReadBit() << 1 | ReadBit();
            //if (m_bitCount < 4)
            //{
            //    m_bitCount += 8;
            //    m_cache = m_cache << 8 | m_parent.m_buffer.Data[m_parent.m_buffer.Position++];
            //}
            //m_bitCount -= 4;
            //return (m_cache >> m_bitCount) & 15;
        }

        private int ReadBits5()
        {
            return ReadBit() << 4 | ReadBit() << 3 | ReadBit() << 2 | ReadBit() << 1 | ReadBit();
            //if (m_bitCount < 5)
            //{
            //    m_bitCount += 8;
            //    m_cache = m_cache << 8 | m_parent.m_buffer.Data[m_parent.m_buffer.Position++];
            //}
            //m_bitCount -= 5;
            //return (m_cache >> m_bitCount) & 31;
        }

        #endregion
    }

}
