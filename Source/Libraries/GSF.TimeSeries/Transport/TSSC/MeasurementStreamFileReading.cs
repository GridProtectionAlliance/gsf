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
    internal partial class MeasurementStreamFileReading
    {
        private ByteBuffer m_buffer;
        private int m_length;

        private long m_prevTimestamp1;
        private long m_prevTimestamp2;

        private long m_prevTimeDelta1;
        private long m_prevTimeDelta2;
        private long m_prevTimeDelta3;
        private long m_prevTimeDelta4;

        private PointMetaData m_lastPoint;
        private IndexedArray<PointMetaData> m_points;

        #region [ Returned Value ]

        private BitStreamReader m_bitStream;
        #endregion

        public MeasurementStreamFileReading()
        {
            m_points = new IndexedArray<PointMetaData>();
            m_buffer = new ByteBuffer(0);
            m_bitStream = new BitStreamReader(this);
            Reset();
        }

        public void Reset()
        {
            m_lastPoint = new PointMetaData(m_buffer, m_bitStream, ushort.MaxValue);
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
            m_buffer = new ByteBuffer(data, startingPosition);
            m_length = startingPosition + length;
        }

        public bool TryGetMeasurement(out ushort id, out long timestamp, out uint quality, out float value)
        {
            PointMetaData nextPoint = null;

            if (m_buffer.Position == m_length && m_bitStream.IsEmpty)
                throw new EndOfStreamException("The end of the stream has been encountered.");

            int code = m_lastPoint.ReadCode(m_bitStream);

            if (code == MeasurementStreamCodes.EndOfStream)
            {
                m_bitStream.Clear();
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
                    m_lastPoint.PrevNextPointId1 ^= (ushort)m_bitStream.ReadBits4();
                }
                else if (code == MeasurementStreamCodes.PointIDXOR8)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                }
                else if (code == MeasurementStreamCodes.PointIDXOR12)
                {
                    m_lastPoint.PrevNextPointId1 ^= (ushort)m_bitStream.ReadBits4();
                    m_lastPoint.PrevNextPointId1 ^= (ushort)(m_buffer.Data[m_buffer.Position++] << 4);
                }
                else if (code == MeasurementStreamCodes.PointIDXOR16)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                    m_lastPoint.PrevNextPointId1 ^= (ushort)(m_buffer.Data[m_buffer.Position++] << 8);
                }
                else
                {
                    throw new Exception("Programming Error.");
                }

                code = m_lastPoint.ReadCode(m_bitStream);
            }

            if (code < MeasurementStreamCodes.TimeDelta1Forward)
                throw new Exception("Expecting higher code");

            id = m_lastPoint.PrevNextPointId1;
            nextPoint = m_points[m_lastPoint.PrevNextPointId1];
            if (nextPoint == null)
            {
                nextPoint = new PointMetaData(m_buffer, m_bitStream, id);
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
                    timestamp = m_prevTimestamp1 ^ (long)Encoding7Bit.ReadUInt64(m_buffer.Data, ref m_buffer.Position);
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
                code = m_lastPoint.ReadCode(m_bitStream);
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
                    quality = Encoding7Bit.ReadUInt32(m_buffer.Data, ref m_buffer.Position);
                }
                nextPoint.PrevQuality2 = nextPoint.PrevQuality1;
                nextPoint.PrevQuality1 = quality;
                code = m_lastPoint.ReadCode(m_bitStream);
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

    }

}
