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
    internal partial class MeasurementStreamFileWriting
    {
        const uint Bits12 = 0xFFFu;
        const uint Bits8 = 0xFFu;
        const uint Bits4 = 0xFu;
        const uint Bits0 = 0x0u;

        private readonly ByteBuffer m_buffer;
        private readonly BitStreamWriter m_bitStream;

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
            m_buffer = new ByteBuffer(4096);
            m_pointById = new List<PointMetaData>();
            m_bitStream = new BitStreamWriter(this);
            Reinitialize();
        }

        public long MeasurementCount;

        /// <summary>
        /// The number of bytes consumed by the stream. 
        /// </summary>
        public int BufferLength => m_buffer.Position;

        /// <summary>
        /// Resets the stream so it can be reused. All measurements must be registered again.
        /// </summary>
        public void Reinitialize()
        {
            MeasurementCount = 0;
            m_bitStream.Reinitialize();
            m_buffer.Position = 0;

            m_prevTimestamp1 = 0;
            m_prevTimestamp2 = 0;

            m_prevTimeDelta1 = long.MaxValue;
            m_prevTimeDelta2 = long.MaxValue;
            m_prevTimeDelta3 = long.MaxValue;
            m_prevTimeDelta4 = long.MaxValue;

            m_pointById.Clear();

            m_lastPoint = new PointMetaData(m_buffer, m_bitStream, ushort.MaxValue);
        }

        /// <summary>
        /// Makes sure that there are at least 300 free bytes available. If not, it will grow the buffer.
        /// </summary>
        /// <remarks>
        /// This number is high because there can be up to 255 bytes of user data in a custom command. 
        /// </remarks>
        private void EnsureCapacity()
        {
            if (m_buffer.Data.Length - m_buffer.Position < 300)
            {
                Grow();
            }
        }

        /// <summary>
        /// Makes sure that at least 100 + length bytes are able to be written to the stream.
        /// </summary>
        /// <param name="length"></param>
        private void EnsureCapacity(int length)
        {
            while (m_buffer.Data.Length - m_buffer.Position < 100 + length)
            {
                Grow();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow()
        {
            m_buffer.Grow();
        }

        public byte[] GetBuffer()
        {
            Flush();
            byte[] data = new byte[BufferLength];
            Array.Copy(m_buffer.Data, 0, data, 0, BufferLength);
            return data;
        }

        /// <summary>
        /// Ensures that the data stream is properly ended.
        /// </summary>
        public void Flush()
        {
            EnsureCapacity();
            m_bitStream.Flush();
        }

        public void AddMeasurement(ushort id, long timestamp, uint quality, float value)
        {
            PointMetaData point = m_pointById[id];
            if (point == null)
            {
                point = new PointMetaData(m_buffer, m_bitStream, id);
                point.PrevNextPointId1 = (ushort)(id + 1);
                m_pointById[id] = point;

            }


            EnsureCapacity();

            if (m_lastPoint.PrevNextPointId1 != point.ReferenceId)
            {
                WritePointIdChange(point);
                m_lastPoint.PrevNextPointId1 = point.ReferenceId;
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

        private void WritePointIdChange(PointMetaData point)
        {
            uint bitsChanged = (uint)(point.ReferenceId ^ m_lastPoint.PrevNextPointId1);

            if (bitsChanged == Bits0)
                throw new Exception("Programming Error. This method should not have been entered");

            if (bitsChanged <= Bits4)
            {
                m_bitStream.WriteCode4(MeasurementStreamCodes.PointIDXOR4, (byte)bitsChanged);
            }
            else if (bitsChanged <= Bits8)
            {
                m_bitStream.WriteCode(MeasurementStreamCodes.PointIDXOR8);

                m_buffer.Data[m_buffer.Position] = (byte)bitsChanged;
                m_buffer.Position++;
            }
            else if (bitsChanged <= Bits12)
            {
                m_bitStream.WriteCode4(MeasurementStreamCodes.PointIDXOR12, (byte)bitsChanged);

                m_buffer.Data[m_buffer.Position] = (byte)(bitsChanged >> 4);
                m_buffer.Position++;
            }
            else 
            {
                m_bitStream.WriteCode(MeasurementStreamCodes.PointIDXOR16);
                m_buffer.Data[m_buffer.Position] = (byte)bitsChanged;
                m_buffer.Data[m_buffer.Position + 1] = (byte)(bitsChanged >> 8);
                m_buffer.Position += 2;
            }

        }

        private void WriteTimestampChange(long timestamp)
        {
            if (m_prevTimestamp1 == timestamp)
                throw new Exception("Coding Error");

            if (m_prevTimestamp2 == timestamp)
            {
                m_bitStream.WriteCode(MeasurementStreamCodes.Timestamp2);
                return;
            }

            if (m_prevTimestamp1 < timestamp)
            {
                if (m_prevTimestamp1 + m_prevTimeDelta1 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta1Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta2 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta2Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta3 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta3Forward);
                }
                else if (m_prevTimestamp1 + m_prevTimeDelta4 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta4Forward);
                }
                else
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeXOR7Bit);
                    Encoding7Bit.Write(m_buffer.Data, ref m_buffer.Position, (ulong)(timestamp ^ m_prevTimestamp1));
                }
            }
            else
            {
                if (m_prevTimestamp1 - m_prevTimeDelta1 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta1Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta2 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta2Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta3 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta3Reverse);
                }
                else if (m_prevTimestamp1 - m_prevTimeDelta4 == timestamp)
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeDelta4Reverse);
                }
                else
                {
                    m_bitStream.WriteCode(MeasurementStreamCodes.TimeXOR7Bit);
                    Encoding7Bit.Write(m_buffer.Data, ref m_buffer.Position, (ulong)(timestamp ^ m_prevTimestamp1));
                }
            }
        }

        private void WriteQualityChange(uint quality, PointMetaData point)
        {
            if (point.PrevQuality2 == quality)
            {
                m_bitStream.WriteCode(MeasurementStreamCodes.Quality2);
            }
            else
            {
                m_bitStream.WriteCode(MeasurementStreamCodes.Quality7Bit32);
                Encoding7Bit.Write(m_buffer.Data, ref m_buffer.Position, quality);
            }
        }

    }
}
