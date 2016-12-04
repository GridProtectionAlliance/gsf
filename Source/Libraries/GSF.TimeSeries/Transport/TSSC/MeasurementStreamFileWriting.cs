//******************************************************************************************************
//  MeasurementStreamFileWriting.cs - Gbtc
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GSF.TimeSeries.Transport.TSSC
{
    internal class MeasurementStreamFileWriting
    {
        private class PointMetaData
        {
            const uint Bits28 = 0xFFFFFFFu;
            const uint Bits24 = 0xFFFFFFu;
            const uint Bits20 = 0xFFFFFu;
            const uint Bits16 = 0xFFFFu;
            const uint Bits12 = 0xFFFu;
            const uint Bits8 = 0xFFu;
            const uint Bits4 = 0xFu;
            const uint Bits0 = 0x0u;

            public ushort PrevNextPointId1;

            public uint PrevQuality1;
            public uint PrevQuality2;
            public uint PrevValue1;
            public uint PrevValue2;
            public uint PrevValue3;

            private readonly byte[] m_commandStats;
            private int m_commandsSentSinceLastChange = 0;

            private byte m_mode;

            //Mode 1 means no prefix.
            private byte m_mode21;

            private byte m_mode31;
            private byte m_mode301;

            private byte m_mode41;
            private byte m_mode401;
            private byte m_mode4001;

            private int m_startupMode = 0;

            private MeasurementStreamFileWriting m_parent;

            

            public PointMetaData(MeasurementStreamFileWriting parent)
            {
                m_parent = parent;
                m_commandsSentSinceLastChange = 0;
                m_commandStats = new byte[32];
                m_mode = 4;
                m_mode41 = MeasurementStreamCodes.Value1;
                m_mode401 = MeasurementStreamCodes.Value2;
                m_mode4001 = MeasurementStreamCodes.Value3;
            }

            public unsafe void WriteValue(float currentValue)
            {
                uint value = *(uint*)&currentValue;

                if (PrevValue1 == value)
                {
                    m_parent.WriteCode(MeasurementStreamCodes.Value1);
                }
                else if (PrevValue2 == value)
                {
                    m_parent.WriteCode(MeasurementStreamCodes.Value2);
                    PrevValue2 = PrevValue1;
                    PrevValue1 = value;
                }
                else if (PrevValue3 == value)
                {
                    m_parent.WriteCode(MeasurementStreamCodes.Value3);
                    PrevValue3 = PrevValue2;
                    PrevValue2 = PrevValue1;
                    PrevValue1 = value;
                }
                else if (value == 0)
                {
                    m_parent.WriteCode(MeasurementStreamCodes.ValueZero);
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
                        m_parent.WriteCode4(MeasurementStreamCodes.ValueXOR4, (byte)bitsChanged);
                    }
                    else if (bitsChanged <= Bits8)
                    {
                        m_parent.WriteCode(MeasurementStreamCodes.ValueXOR8);

                        m_parent.m_data[m_parent.m_position] = (byte)bitsChanged;
                        m_parent.m_position++;
                    }
                    else if (bitsChanged <= Bits12)
                    {
                        m_parent.WriteCode4(MeasurementStreamCodes.ValueXOR12, (byte)bitsChanged);

                        m_parent.m_data[m_parent.m_position] = (byte)(bitsChanged >> 4);
                        m_parent.m_position++;
                    }
                    else if (bitsChanged <= Bits16)
                    {
                        m_parent.WriteCode(MeasurementStreamCodes.ValueXOR16);
                        m_parent.m_data[m_parent.m_position] = (byte)bitsChanged;
                        m_parent.m_data[m_parent.m_position + 1] = (byte)(bitsChanged >> 8);
                        m_parent.m_position = m_parent.m_position + 2;
                    }
                    else if (bitsChanged <= Bits20)
                    {
                        m_parent.WriteCode4(MeasurementStreamCodes.ValueXOR20, (byte)bitsChanged);

                        m_parent.m_data[m_parent.m_position] = (byte)(bitsChanged >> 4);
                        m_parent.m_data[m_parent.m_position + 1] = (byte)(bitsChanged >> 12);
                        m_parent.m_position = m_parent.m_position + 2;
                    }
                    else if (bitsChanged <= Bits24)
                    {
                        m_parent.WriteCode(MeasurementStreamCodes.ValueXOR24);

                        m_parent.m_data[m_parent.m_position] = (byte)bitsChanged;
                        m_parent.m_data[m_parent.m_position + 1] = (byte)(bitsChanged >> 8);
                        m_parent.m_data[m_parent.m_position + 2] = (byte)(bitsChanged >> 16);
                        m_parent.m_position = m_parent.m_position + 3;
                    }
                    else if (bitsChanged <= Bits28)
                    {
                        m_parent.WriteCode4(MeasurementStreamCodes.ValueXOR28, (byte)bitsChanged);

                        m_parent.m_data[m_parent.m_position] = (byte)(bitsChanged >> 4);
                        m_parent.m_data[m_parent.m_position + 1] = (byte)(bitsChanged >> 12);
                        m_parent.m_data[m_parent.m_position + 2] = (byte)(bitsChanged >> 20);
                        m_parent.m_position = m_parent.m_position + 3;
                    }
                    else
                    {
                        m_parent.WriteCode(MeasurementStreamCodes.ValueXOR32);

                        m_parent.m_data[m_parent.m_position] = (byte)bitsChanged;
                        m_parent.m_data[m_parent.m_position + 1] = (byte)(bitsChanged >> 8);
                        m_parent.m_data[m_parent.m_position + 2] = (byte)(bitsChanged >> 16);
                        m_parent.m_data[m_parent.m_position + 3] = (byte)(bitsChanged >> 24);
                        m_parent.m_position = m_parent.m_position + 4;
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

                switch (m_mode)
                {
                    case 1:
                        len = 5;
                        break;
                    case 2:
                        if (code == m_mode21)
                        {
                            code = 1;
                            len = 1;
                        }
                        else
                        {
                            len = 6;
                        }
                        break;
                    case 3:
                        if (code == m_mode31)
                        {
                            code = 1;
                            len = 1;
                        }
                        else if (code == m_mode301)
                        {
                            code = 1;
                            len = 2;
                        }
                        else
                        {
                            len = 7;
                        }
                        break;
                    case 4:
                        if (code == m_mode41)
                        {
                            code = 1;
                            len = 1;
                        }
                        else if (code == m_mode401)
                        {
                            code = 1;
                            len = 2;
                        }
                        else if (code == m_mode4001)
                        {
                            code = 1;
                            len = 3;
                        }
                        else
                        {
                            len = 8;
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


        const uint Bits12 = 0xFFFu;
        const uint Bits8 = 0xFFu;
        const uint Bits4 = 0xFu;
        const uint Bits0 = 0x0u;

        private byte[] m_data;
        private int m_position;

        private long m_prevTimestamp1;
        private long m_prevTimestamp2;

        private long m_prevTimeDelta1;
        private long m_prevTimeDelta2;
        private long m_prevTimeDelta3;
        private long m_prevTimeDelta4;

        private PointMetaData m_lastPoint;
        private List<PointMetaData> m_pointById;

        public MeasurementStreamFileWriting()
        {
            m_data = new byte[short.MaxValue];
            m_pointById = new List<PointMetaData>();
            BitStreamReinitialize();
            Reinitialize();
        }

        public long MeasurementCount;

        /// <summary>
        /// The number of bytes consumed by the stream. 
        /// </summary>
        public int BufferLength => m_position;

        /// <summary>
        /// Resets the stream so it can be reused. All measurements must be registered again.
        /// </summary>
        public void Reinitialize()
        {
            MeasurementCount = 0;
            BitStreamReinitialize();
            m_position = 0;

            m_prevTimestamp1 = 0;
            m_prevTimestamp2 = 0;

            m_prevTimeDelta1 = long.MaxValue;
            m_prevTimeDelta2 = long.MaxValue;
            m_prevTimeDelta3 = long.MaxValue;
            m_prevTimeDelta4 = long.MaxValue;

            m_pointById.Clear();

            m_lastPoint = new PointMetaData(this);
        }

        /// <summary>
        /// Makes sure that there are at least 300 free bytes available. If not, it will grow the buffer.
        /// </summary>
        /// <remarks>
        /// This number is high because there can be up to 255 bytes of user data in a custom command. 
        /// </remarks>
        private void EnsureCapacity()
        {
            if (m_data.Length - m_position < 300)
            {
                Grow();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow()
        {
           throw new NotSupportedException();
        }

        public byte[] GetBuffer()
        {
            Flush();
            byte[] data = new byte[BufferLength];
            Array.Copy(m_data, 0, data, 0, BufferLength);
            return data;
        }

        /// <summary>
        /// Ensures that the data stream is properly ended.
        /// </summary>
        public void Flush()
        {
            EnsureCapacity();
            BitStreamFlush();
        }

        public void AddMeasurement(ushort id, long timestamp, uint quality, float value)
        {
            PointMetaData point = m_pointById[id];
            if (point == null)
            {
                point = new PointMetaData(this);
                point.PrevNextPointId1 = (ushort)(id + 1);
                m_pointById[id] = point;

            }

            EnsureCapacity();

            if (m_lastPoint.PrevNextPointId1 != id)
            {
                WritePointIdChange(id);
                m_lastPoint.PrevNextPointId1 = id;
            }

            if (m_prevTimestamp1 != timestamp)
            {
                WriteTimestampChange(timestamp);

                //Save the smallest delta time
                long minDelta = Math.Abs(m_prevTimestamp1 - timestamp);

                if (minDelta < m_prevTimeDelta4
                    && minDelta != m_prevTimeDelta1
                    && minDelta != m_prevTimeDelta2
                    && minDelta != m_prevTimeDelta3)
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
            }

            if (point.PrevQuality1 != quality)
            {
                WriteQualityChange(quality, point);
                point.PrevQuality2 = point.PrevQuality1;
                point.PrevQuality1 = quality;
            }

            point.WriteValue(value);

            m_lastPoint = point;
            MeasurementCount++;
        }

        private void WritePointIdChange(ushort id)
        {
            uint bitsChanged = (uint)(id ^ m_lastPoint.PrevNextPointId1);

            if (bitsChanged == Bits0)
                throw new Exception("Programming Error. This method should not have been entered");

            if (bitsChanged <= Bits4)
            {
                WriteCode4(MeasurementStreamCodes.PointIDXOR4, (byte)bitsChanged);
            }
            else if (bitsChanged <= Bits8)
            {
                WriteCode(MeasurementStreamCodes.PointIDXOR8);
                m_data[m_position] = (byte)bitsChanged;
                m_position++;
            }
            else if (bitsChanged <= Bits12)
            {
                WriteCode4(MeasurementStreamCodes.PointIDXOR12, (byte)bitsChanged);

                m_data[m_position] = (byte)(bitsChanged >> 4);
                m_position++;
            }
            else
            {
                WriteCode(MeasurementStreamCodes.PointIDXOR16);
                m_data[m_position] = (byte)bitsChanged;
                m_data[m_position + 1] = (byte)(bitsChanged >> 8);
                m_position += 2;
            }

        }

        private void WriteTimestampChange(long timestamp)
        {
            if (m_prevTimestamp1 == timestamp)
                throw new Exception("Coding Error");

            if (m_prevTimestamp2 == timestamp)
            {
                WriteCode(MeasurementStreamCodes.Timestamp2);
                return;
            }

            if (m_prevTimestamp1 < timestamp)
            {
                if (m_prevTimestamp1 + m_prevTimeDelta1 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta1Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta2 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta2Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta3 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta3Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta4 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta4Forward);
                }
                else
                {
                    WriteCode(MeasurementStreamCodes.TimeXOR7Bit);
                    Encoding7Bit.Write(m_data, ref m_position, (ulong)(timestamp ^ m_prevTimestamp1));
                }
            }
            else
            {
                if (m_prevTimestamp1 - m_prevTimeDelta1 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta1Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta2 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta2Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta3 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta3Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta4 == timestamp)
                {
                    WriteCode(MeasurementStreamCodes.TimeDelta4Reverse);
                }
                else
                {
                    WriteCode(MeasurementStreamCodes.TimeXOR7Bit);
                    Encoding7Bit.Write(m_data, ref m_position, (ulong)(timestamp ^ m_prevTimestamp1));
                }
            }
        }

        private void WriteQualityChange(uint quality, PointMetaData point)
        {
            if (point.PrevQuality2 == quality)
            {
                WriteCode(MeasurementStreamCodes.Quality2);
            }
            else
            {
                WriteCode(MeasurementStreamCodes.Quality7Bit32);
                Encoding7Bit.Write(m_data, ref m_position, quality);
            }
        }

        #region [ Bit Stream ]

        /// <summary>
        /// The position in m_buffer where the bit stream should be flushed
        /// -1 means no bit stream position has been assigned. 
        /// </summary>
        private int m_bitStreamBufferIndex;
        /// <summary>
        /// The number of bits in m_bitStreamCache that are valid. 0 Means the bitstream is empty.
        /// </summary>
        private int m_bitStreamCacheBitCount;
        /// <summary>
        /// A cache of bits that need to be flushed to m_buffer when full. Bits filled starting from the right moving left.
        /// </summary>
        private int m_bitStreamCache;
        
        /// <summary>
        /// Resets the stream so it can be reused. All measurements must be registered again.
        /// </summary>
        private void BitStreamReinitialize()
        {
            m_bitStreamBufferIndex = -1;
            m_bitStreamCacheBitCount = 0;
            m_bitStreamCache = 0;
        }

        private void WriteCode(int code)
        {
            int len;
            m_lastPoint.TransformCode(ref code, out len);

            WriteCode(code, len);
        }

        private void WriteCode4(int code, int value)
        {
            int len;
            m_lastPoint.TransformCode(ref code, out len);

            WriteCode((code << 4) | (value & 15), len + 4);
        }

        private void WriteCode(int code, int len)
        {
            if (m_bitStreamBufferIndex < 0)
            {
                m_bitStreamBufferIndex = m_position++;
            }

            m_bitStreamCache = (m_bitStreamCache << len) | code;
            m_bitStreamCacheBitCount += len;

            if (m_bitStreamCacheBitCount > 7)
            {
                BitStreamEnd2();
            }
        }

        private void BitStreamEnd2()
        {
            while (m_bitStreamCacheBitCount > 7)
            {
                m_data[m_bitStreamBufferIndex] = (byte)(m_bitStreamCache >> (m_bitStreamCacheBitCount - 8));
                m_bitStreamCacheBitCount -= 8;

                if (m_bitStreamCacheBitCount > 0)
                {
                    m_bitStreamBufferIndex = m_position++;
                }
                else
                {
                    m_bitStreamBufferIndex = -1;
                }
            }
        }

        private void BitStreamFlush()
        {
            if (m_bitStreamCacheBitCount > 0)
            {
                if (m_bitStreamBufferIndex < 0)
                {
                    m_bitStreamBufferIndex = m_position++;
                }

                WriteCode(MeasurementStreamCodes.EndOfStream);

                if (m_bitStreamCacheBitCount > 7)
                {
                    BitStreamEnd2();
                }

                if (m_bitStreamCacheBitCount > 0)
                {
                    //Make up 8 bits by padding.
                    m_bitStreamCache <<= 8 - m_bitStreamCacheBitCount;
                    m_data[m_bitStreamBufferIndex] = (byte)m_bitStreamCache;
                    m_bitStreamCache = 0;
                    m_bitStreamBufferIndex = -1;
                    m_bitStreamCacheBitCount = 0;
                }
            }
        }

        #endregion

    }
}
