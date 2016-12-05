//******************************************************************************************************
//  TsscEncoder.cs - Gbtc
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
using GSF.Collection;

namespace GSF.TimeSeries.Transport.TSSC
{
    /// <summary>
    /// An encoder for the TSSC protocol.
    /// </summary>
    public class TsscEncoder
    {
        const uint Bits28 = 0xFFFFFFFu;
        const uint Bits24 = 0xFFFFFFu;
        const uint Bits20 = 0xFFFFFu;
        const uint Bits16 = 0xFFFFu;
        const uint Bits12 = 0xFFFu;
        const uint Bits8 = 0xFFu;
        const uint Bits4 = 0xFu;
        const uint Bits0 = 0x0u;

        private byte[] m_data;
        private int m_position;
        private int m_lastPosition;

        private long m_prevTimestamp1;
        private long m_prevTimestamp2;

        private long m_prevTimeDelta1;
        private long m_prevTimeDelta2;
        private long m_prevTimeDelta3;
        private long m_prevTimeDelta4;

        private TsscPointMetadata m_lastPoint;
        private IndexedArray<TsscPointMetadata> m_points;

        /// <summary>
        /// Creates a encoder for the TSSC protocol.
        /// </summary>
        public TsscEncoder()
        {
            Reset();
        }


        /// <summary>
        /// Resets the TSSC Encoder to the initial state. 
        /// </summary>
        /// <remarks>
        /// TSSC is a stateful encoder that requires a state
        /// of the previous data to be maintained. Therefore, if 
        /// the state ever becomes corrupt (out of order, dropped, corrupted, or duplicated)
        /// the state must be reset on both ends.
        /// </remarks>
        public void Reset()
        {
            m_points = new IndexedArray<TsscPointMetadata>();
            m_lastPoint = new TsscPointMetadata(WriteBits, null, null);
            m_data = EmptyArray<byte>.Empty;
            m_position = 0;
            m_lastPosition = 0;
            ClearBitStream();

            m_prevTimeDelta1 = long.MaxValue;
            m_prevTimeDelta2 = long.MaxValue;
            m_prevTimeDelta3 = long.MaxValue;
            m_prevTimeDelta4 = long.MaxValue;
            m_prevTimestamp1 = 0;
            m_prevTimestamp2 = 0;

        }

        /// <summary>
        /// Sets the internal buffer to write data to.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startingPosition"></param>
        /// <param name="length"></param>
        public void SetBuffer(byte[] data, int startingPosition, int length)
        {
            m_data = data;
            m_position = startingPosition;
            m_lastPosition = startingPosition + length;
        }

        /// <summary>
        /// Finishes the current block and returns position after the last byte written.
        /// </summary>
        /// <returns></returns>
        public int FinishBlock()
        {
            BitStreamFlush();
            return m_position;
        }

        /// <summary>
        /// Adds the supplied measurement to the stream. If the stream is full,
        /// this method returns false.
        /// </summary>
        /// <param name="id">the id</param>
        /// <param name="timestamp">the timestamp in ticks</param>
        /// <param name="quality">the quality</param>
        /// <param name="value">the value</param>
        /// <returns>true if successful, false otherwise.</returns>
        public unsafe bool TryAddMeasurement(ushort id, long timestamp, uint quality, float value)
        {
            //if there are fewer than 100 bytes available on the buffer
            //assume that we cannot add any more.
            if (m_lastPosition - m_position < 100) 
                return false;

            TsscPointMetadata point = m_points[id];
            if (point == null)
            {
                point = new TsscPointMetadata(WriteBits, null, null);
                point.PrevNextPointId1 = (ushort)(id + 1);
                m_points[id] = point;
            }
            
            //Note: since I will not know the incoming pointID. The most recent
            //      measurement received will be the one that contains the 
            //      coding algorithm for this measurement. Since for the more part
            //      measurements generally have some sort of sequence to them, 
            //      this still ends up being a good enough assumption.

            if (m_lastPoint.PrevNextPointId1 != id)
            {
                WritePointIdChange(id);
            }

            if (m_prevTimestamp1 != timestamp)
            {
                WriteTimestampChange(timestamp);
            }

            if (point.PrevQuality1 != quality)
            {
                WriteQualityChange(quality, point);
            }

            uint valueRaw = *(uint*)&value;
            if (point.PrevValue1 == valueRaw)
            {
                m_lastPoint.WriteCode(TsscCodeWords.Value1);
            }
            else if (point.PrevValue2 == valueRaw)
            {
                m_lastPoint.WriteCode(TsscCodeWords.Value2);
                point.PrevValue2 = point.PrevValue1;
                point.PrevValue1 = valueRaw;
            }
            else if (point.PrevValue3 == valueRaw)
            {
                m_lastPoint.WriteCode(TsscCodeWords.Value3);
                point.PrevValue3 = point.PrevValue2;
                point.PrevValue2 = point.PrevValue1;
                point.PrevValue1 = valueRaw;
            }
            else if (valueRaw == 0)
            {
                m_lastPoint.WriteCode(TsscCodeWords.ValueZero);
                point.PrevValue3 = point.PrevValue2;
                point.PrevValue2 = point.PrevValue1;
                point.PrevValue1 = 0;
            }
            else
            {
                uint bitsChanged = valueRaw ^ point.PrevValue1;

                if (bitsChanged <= Bits4)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR4);
                    WriteBits((byte)bitsChanged, 4);
                }
                else if (bitsChanged <= Bits8)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR8);

                    m_data[m_position] = (byte)bitsChanged;
                    m_position++;
                }
                else if (bitsChanged <= Bits12)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR12);
                    WriteBits((byte)bitsChanged, 4);

                    m_data[m_position] = (byte)(bitsChanged >> 4);
                    m_position++;
                }
                else if (bitsChanged <= Bits16)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR16);
                    m_data[m_position] = (byte)bitsChanged;
                    m_data[m_position + 1] = (byte)(bitsChanged >> 8);
                    m_position = m_position + 2;
                }
                else if (bitsChanged <= Bits20)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR20);
                    WriteBits((byte)bitsChanged, 4);

                    m_data[m_position] = (byte)(bitsChanged >> 4);
                    m_data[m_position + 1] = (byte)(bitsChanged >> 12);
                    m_position = m_position + 2;
                }
                else if (bitsChanged <= Bits24)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR24);

                    m_data[m_position] = (byte)bitsChanged;
                    m_data[m_position + 1] = (byte)(bitsChanged >> 8);
                    m_data[m_position + 2] = (byte)(bitsChanged >> 16);
                    m_position = m_position + 3;
                }
                else if (bitsChanged <= Bits28)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR28);
                    WriteBits((byte)bitsChanged, 4);

                    m_data[m_position] = (byte)(bitsChanged >> 4);
                    m_data[m_position + 1] = (byte)(bitsChanged >> 12);
                    m_data[m_position + 2] = (byte)(bitsChanged >> 20);
                    m_position = m_position + 3;
                }
                else
                {
                    m_lastPoint.WriteCode(TsscCodeWords.ValueXOR32);

                    m_data[m_position] = (byte)bitsChanged;
                    m_data[m_position + 1] = (byte)(bitsChanged >> 8);
                    m_data[m_position + 2] = (byte)(bitsChanged >> 16);
                    m_data[m_position + 3] = (byte)(bitsChanged >> 24);
                    m_position = m_position + 4;
                }

                point.PrevValue3 = point.PrevValue2;
                point.PrevValue2 = point.PrevValue1;
                point.PrevValue1 = valueRaw;
            }

            m_lastPoint = point;

            return true;
        }

        private void WritePointIdChange(ushort id)
        {
            uint bitsChanged = (uint)(id ^ m_lastPoint.PrevNextPointId1);

            if (bitsChanged <= Bits4)
            {
                m_lastPoint.WriteCode(TsscCodeWords.PointIDXOR4);
                WriteBits((byte)bitsChanged, 4);
            }
            else if (bitsChanged <= Bits8)
            {
                m_lastPoint.WriteCode(TsscCodeWords.PointIDXOR8);
                m_data[m_position] = (byte)bitsChanged;
                m_position++;
            }
            else if (bitsChanged <= Bits12)
            {
                m_lastPoint.WriteCode(TsscCodeWords.PointIDXOR12);
                WriteBits((byte)bitsChanged, 4);

                m_data[m_position] = (byte)(bitsChanged >> 4);
                m_position++;
            }
            else
            {
                m_lastPoint.WriteCode(TsscCodeWords.PointIDXOR16);
                m_data[m_position] = (byte)bitsChanged;
                m_data[m_position + 1] = (byte)(bitsChanged >> 8);
                m_position += 2;
            }

            m_lastPoint.PrevNextPointId1 = id;
        }

        private void WriteTimestampChange(long timestamp)
        {
            if (m_prevTimestamp2 == timestamp)
            {
                m_lastPoint.WriteCode(TsscCodeWords.Timestamp2);
            }
            else if (m_prevTimestamp1 < timestamp)
            {
                if (m_prevTimestamp1 + m_prevTimeDelta1 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta1Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta2 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta2Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta3 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta3Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta4 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta4Forward);
                }
                else
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeXOR7Bit);
                    Encoding7Bit.Write(m_data, ref m_position, (ulong)(timestamp ^ m_prevTimestamp1));
                }
            }
            else
            {
                if (m_prevTimestamp1 - m_prevTimeDelta1 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta1Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta2 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta2Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta3 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta3Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta4 == timestamp)
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeDelta4Reverse);
                }
                else
                {
                    m_lastPoint.WriteCode(TsscCodeWords.TimeXOR7Bit);
                    Encoding7Bit.Write(m_data, ref m_position, (ulong)(timestamp ^ m_prevTimestamp1));
                }
            }

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

        private void WriteQualityChange(uint quality, TsscPointMetadata point)
        {
            if (point.PrevQuality2 == quality)
            {
                m_lastPoint.WriteCode(TsscCodeWords.Quality2);
            }
            else
            {
                m_lastPoint.WriteCode(TsscCodeWords.Quality7Bit32);
                Encoding7Bit.Write(m_data, ref m_position, quality);
            }
            point.PrevQuality2 = point.PrevQuality1;
            point.PrevQuality1 = quality;
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
        private void ClearBitStream()
        {
            m_bitStreamBufferIndex = -1;
            m_bitStreamCacheBitCount = 0;
            m_bitStreamCache = 0;
        }

        private void WriteBits(int code, int len)
        {
            if (m_bitStreamBufferIndex < 0)
            {
                m_bitStreamBufferIndex = m_position++;
            }

            m_bitStreamCache = (m_bitStreamCache << len) | code;
            m_bitStreamCacheBitCount += len;

            if (m_bitStreamCacheBitCount > 7)
            {
                BitStreamEnd();
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

                m_lastPoint.WriteCode(TsscCodeWords.EndOfStream);

                if (m_bitStreamCacheBitCount > 7)
                {
                    BitStreamEnd();
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

        private void BitStreamEnd()
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

        #endregion

    }
}
