//******************************************************************************************************
//  TsscDecoder.cs - Gbtc
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
using GSF.Collection;

namespace GSF.TimeSeries.Transport.TSSC
{
    /// <summary>
    /// The decoder for the TSSC protocol.
    /// </summary>
    public class TsscDecoder
    {
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
        /// Creates a decoder for the TSSC protocol.
        /// </summary>
        public TsscDecoder()
        {
            Reset();
        }

        /// <summary>
        /// Resets the TSSC Decoder to the initial state. 
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
            m_lastPoint = new TsscPointMetadata(null, ReadBit, ReadBits5);
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
        /// Sets the internal buffer to read data from.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startingPosition"></param>
        /// <param name="length"></param>
        public void SetBuffer(byte[] data, int startingPosition, int length)
        {
            data.ValidateParameters(startingPosition, length);
            ClearBitStream();
            m_data = data;
            m_position = startingPosition;
            m_lastPosition = startingPosition + length;
        }

        /// <summary>
        /// Reads the next measurement from the stream. If the end of the stream has been encountered, 
        /// return false.
        /// </summary>
        /// <param name="id">the id</param>
        /// <param name="timestamp">the timestamp in ticks</param>
        /// <param name="quality">the quality</param>
        /// <param name="value">the value</param>
        /// <returns>true if successful, false otherwise.</returns>
        public unsafe bool TryGetMeasurement(out ushort id, out long timestamp, out uint quality, out float value)
        {
            TsscPointMetadata nextPoint = null;

            if (m_position == m_lastPosition && BitStreamIsEmpty)
            {
                ClearBitStream();
                id = 0;
                timestamp = 0;
                quality = 0;
                value = 0;
                return false;
            }

            //Note: since I will not know the incoming pointID. The most recent
            //      measurement received will be the one that contains the 
            //      coding algorithm for this measurement. Since for the more part
            //      measurements generally have some sort of sequence to them, 
            //      this still ends up being a good enough assumption.

            int code = m_lastPoint.ReadCode();

            if (code == TsscCodeWords.EndOfStream)
            {
                ClearBitStream();
                id = 0;
                timestamp = 0;
                quality = 0;
                value = 0;
                return false;
            }

            if (code <= TsscCodeWords.PointIDXOR16)
            {
                DecodePointID(code, m_lastPoint);
                code = m_lastPoint.ReadCode();
                if (code < TsscCodeWords.TimeDelta1Forward)
                    throw new Exception($"Expecting code >= {TsscCodeWords.TimeDelta1Forward} Received {code} at position {m_position} with last position { m_lastPosition}");
            }

            id = m_lastPoint.PrevNextPointId1;
            nextPoint = m_points[m_lastPoint.PrevNextPointId1];
            if (nextPoint == null)
            {
                nextPoint = new TsscPointMetadata(null, ReadBit, ReadBits5);
                m_points[id] = nextPoint;
                nextPoint.PrevNextPointId1 = (ushort)(id + 1);
            }

            if (code <= TsscCodeWords.TimeXOR7Bit)
            {
                timestamp = DecodeTimestamp(code);
                code = m_lastPoint.ReadCode();
                if (code < TsscCodeWords.Quality2)
                    throw new Exception($"Expecting code >= {TsscCodeWords.Quality2} Received {code} at position {m_position} with last position { m_lastPosition}");
            }
            else
            {
                timestamp = m_prevTimestamp1;
            }

            if (code <= TsscCodeWords.Quality7Bit32)
            {
                quality = DecodeQuality(code, nextPoint);
                code = m_lastPoint.ReadCode();
                if (code < TsscCodeWords.Value1)
                    throw new Exception($"Expecting code >= {TsscCodeWords.Value1} Received {code} at position {m_position} with last position { m_lastPosition}");
            }
            else
            {
                quality = nextPoint.PrevQuality1;
            }

            //Since value will almost always change, 
            //This is not put inside a function call.
            uint valueRaw = 0;
            if (code == TsscCodeWords.Value1)
            {
                valueRaw = nextPoint.PrevValue1;
            }
            else if (code == TsscCodeWords.Value2)
            {
                valueRaw = nextPoint.PrevValue2;
                nextPoint.PrevValue2 = nextPoint.PrevValue1;
                nextPoint.PrevValue1 = valueRaw;
            }
            else if (code == TsscCodeWords.Value3)
            {
                valueRaw = nextPoint.PrevValue3;
                nextPoint.PrevValue3 = nextPoint.PrevValue2;
                nextPoint.PrevValue2 = nextPoint.PrevValue1;
                nextPoint.PrevValue1 = valueRaw;
            }
            else if (code == TsscCodeWords.ValueZero)
            {
                valueRaw = 0;
                nextPoint.PrevValue3 = nextPoint.PrevValue2;
                nextPoint.PrevValue2 = nextPoint.PrevValue1;
                nextPoint.PrevValue1 = valueRaw;
            }
            else
            {
                switch (code)
                {
                    case TsscCodeWords.ValueXOR4:
                        valueRaw = (uint)ReadBits4() ^ nextPoint.PrevValue1;
                        break;
                    case TsscCodeWords.ValueXOR8:
                        valueRaw = m_data[m_position] ^ nextPoint.PrevValue1;
                        m_position = m_position + 1;
                        break;
                    case TsscCodeWords.ValueXOR12:
                        valueRaw = (uint)ReadBits4() ^ (uint)(m_data[m_position] << 4) ^ nextPoint.PrevValue1;
                        m_position = m_position + 1;
                        break;
                    case TsscCodeWords.ValueXOR16:
                        valueRaw = m_data[m_position] ^ (uint)(m_data[m_position + 1] << 8) ^ nextPoint.PrevValue1;
                        m_position = m_position + 2;
                        break;
                    case TsscCodeWords.ValueXOR20:
                        valueRaw = (uint)ReadBits4() ^ (uint)(m_data[m_position] << 4) ^ (uint)(m_data[m_position + 1] << 12) ^ nextPoint.PrevValue1;
                        m_position = m_position + 2;
                        break;
                    case TsscCodeWords.ValueXOR24:
                        valueRaw = m_data[m_position] ^ (uint)(m_data[m_position + 1] << 8) ^ (uint)(m_data[m_position + 2] << 16) ^ nextPoint.PrevValue1;
                        m_position = m_position + 3;
                        break;
                    case TsscCodeWords.ValueXOR28:
                        valueRaw = (uint)ReadBits4() ^ (uint)(m_data[m_position] << 4) ^ (uint)(m_data[m_position + 1] << 12) ^ (uint)(m_data[m_position + 2] << 20) ^ nextPoint.PrevValue1;
                        m_position = m_position + 3;
                        break;
                    case TsscCodeWords.ValueXOR32:
                        valueRaw = m_data[m_position] ^ (uint)(m_data[m_position + 1] << 8) ^ (uint)(m_data[m_position + 2] << 16) ^ (uint)(m_data[m_position + 3] << 24) ^ nextPoint.PrevValue1;
                        m_position = m_position + 4;
                        break;
                    default:
                        throw new Exception($"Invalid code received {code} at position {m_position} with last position { m_lastPosition}");
                }

                nextPoint.PrevValue3 = nextPoint.PrevValue2;
                nextPoint.PrevValue2 = nextPoint.PrevValue1;
                nextPoint.PrevValue1 = valueRaw;
            }

            value = *(float*)&valueRaw;
            m_lastPoint = nextPoint;
            return true;
        }

        private void DecodePointID(int code, TsscPointMetadata lastPoint)
        {
            if (code == TsscCodeWords.PointIDXOR4)
            {
                lastPoint.PrevNextPointId1 ^= (ushort)ReadBits4();
            }
            else if (code == TsscCodeWords.PointIDXOR8)
            {
                lastPoint.PrevNextPointId1 ^= m_data[m_position++];
            }
            else if (code == TsscCodeWords.PointIDXOR12)
            {
                lastPoint.PrevNextPointId1 ^= (ushort)ReadBits4();
                lastPoint.PrevNextPointId1 ^= (ushort)(m_data[m_position++] << 4);
            }
            else
            {
                lastPoint.PrevNextPointId1 ^= m_data[m_position++];
                lastPoint.PrevNextPointId1 ^= (ushort)(m_data[m_position++] << 8);
            }
        }

        private long DecodeTimestamp(int code)
        {
            long timestamp;
            if (code == TsscCodeWords.TimeDelta1Forward)
            {
                timestamp = m_prevTimestamp1 + m_prevTimeDelta1;
            }
            else if (code == TsscCodeWords.TimeDelta2Forward)
            {
                timestamp = m_prevTimestamp1 + m_prevTimeDelta2;
            }
            else if (code == TsscCodeWords.TimeDelta3Forward)
            {
                timestamp = m_prevTimestamp1 + m_prevTimeDelta3;
            }
            else if (code == TsscCodeWords.TimeDelta4Forward)
            {
                timestamp = m_prevTimestamp1 + m_prevTimeDelta4;
            }
            else if (code == TsscCodeWords.TimeDelta1Reverse)
            {
                timestamp = m_prevTimestamp1 - m_prevTimeDelta1;
            }
            else if (code == TsscCodeWords.TimeDelta2Reverse)
            {
                timestamp = m_prevTimestamp1 - m_prevTimeDelta2;
            }
            else if (code == TsscCodeWords.TimeDelta3Reverse)
            {
                timestamp = m_prevTimestamp1 - m_prevTimeDelta3;
            }
            else if (code == TsscCodeWords.TimeDelta4Reverse)
            {
                timestamp = m_prevTimestamp1 - m_prevTimeDelta4;
            }
            else if (code == TsscCodeWords.Timestamp2)
            {
                timestamp = m_prevTimestamp2;
            }
            else
            {
                timestamp = m_prevTimestamp1 ^ (long)Encoding7Bit.ReadUInt64(m_data, ref m_position);
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
            return timestamp;
        }

        private uint DecodeQuality(int code, TsscPointMetadata nextPoint)
        {
            uint quality;
            if (code == TsscCodeWords.Quality2)
            {
                quality = nextPoint.PrevQuality2;
            }
            else
            {
                quality = Encoding7Bit.ReadUInt32(m_data, ref m_position);
            }
            nextPoint.PrevQuality2 = nextPoint.PrevQuality1;
            nextPoint.PrevQuality1 = quality;
            return quality;
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
