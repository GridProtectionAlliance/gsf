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
using System.Collections.Generic;
using System.IO;
using GSF;
using GSF.IO;

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
        private List<PointMetaData> m_points;

        #region [ Returned Value ]

        public int ID;
        public long Timestamp;
        public uint Quality;
        public float Value;
        public int NewMeasurementRegisteredId;

        private BitStreamReader m_bitStream;
        #endregion

        public MeasurementStreamFileReading()
        {
            m_points = new List<PointMetaData>();
            m_buffer = new ByteBuffer(4096);
            m_bitStream = new BitStreamReader(this);
        }

        private void Reset()
        {
            m_lastPoint = new PointMetaData(m_buffer, m_bitStream, -1);
            m_points.Clear();
            m_prevTimeDelta1 = long.MaxValue;
            m_prevTimeDelta2 = long.MaxValue;
            m_prevTimeDelta3 = long.MaxValue;
            m_prevTimeDelta4 = long.MaxValue;
            m_prevTimestamp1 = 0;
            m_prevTimestamp2 = 0;
        }

        public void Load(byte[] data)
        {
            while (m_buffer.Data.Length < data.Length)
                m_buffer.Grow();

            m_buffer.Position = 0;
            m_length = data.Length;
            data.CopyTo(m_buffer.Data, 0);

            Reset();
        }

        public void GetMeasurement()
        {
            TryAgain:

            PointMetaData nextPoint = null;

            if (m_buffer.Position == m_length && m_bitStream.IsEmpty)
                throw new EndOfStreamException("The end of the stream has been encountered.");

            int code = m_lastPoint.ReadCode(m_bitStream);

            if (code >= MeasurementStreamCodes.NewPointId && code <= MeasurementStreamCodes.FlushBits)
            {
                if (code == MeasurementStreamCodes.NewPointId)
                {
                    PointMetaData point = new PointMetaData(m_buffer, m_bitStream, m_points.Count);
                    NewMeasurementRegisteredId = point.ReferenceId;
                    m_lastPoint.PrevNextPointId1 = point.ReferenceId;
                    m_points.Add(point);

                    point.SignalID = LittleEndian.ToUInt16(m_buffer.Data, m_buffer.Position);
                    m_buffer.Position += 2;
                    goto TryAgain;
                }
                else if (code == MeasurementStreamCodes.FlushBits)
                {
                    m_bitStream.Clear();
                    goto TryAgain;
                }
                else
                {
                    throw new Exception("Programming Error.");
                }
            }

            if (code < MeasurementStreamCodes.PointIDXOR4)
                throw new Exception("Expecting higher code");

            if (code <= MeasurementStreamCodes.PointIDXOR32)
            {
                if (code == MeasurementStreamCodes.PointIDXOR4)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_bitStream.ReadBits4();
                }
                else if (code == MeasurementStreamCodes.PointIDXOR8)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                }
                else if (code == MeasurementStreamCodes.PointIDXOR12)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_bitStream.ReadBits4();
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 4;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR16)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 8;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR20)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_bitStream.ReadBits4();
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 4;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 12;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR24)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 8;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 16;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR32)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 8;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 16;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 24;
                }
                else
                {
                    throw new Exception("Programming Error.");
                }

                code = m_lastPoint.ReadCode(m_bitStream);
            }

            if (code < MeasurementStreamCodes.TimeDelta1Forward)
                throw new Exception("Expecting higher code");

            ID = m_lastPoint.PrevNextPointId1;
            nextPoint = m_points[m_lastPoint.PrevNextPointId1];
            Quality = nextPoint.PrevQuality1;

            if (code <= MeasurementStreamCodes.TimeXOR7Bit)
            {
                if (code == MeasurementStreamCodes.TimeDelta1Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta1;
                }
                else if (code == MeasurementStreamCodes.TimeDelta2Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta2;
                }
                else if (code == MeasurementStreamCodes.TimeDelta3Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta3;
                }
                else if (code == MeasurementStreamCodes.TimeDelta4Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta4;
                }
                else if (code == MeasurementStreamCodes.TimeDelta1Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta1;
                }
                else if (code == MeasurementStreamCodes.TimeDelta2Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta2;
                }
                else if (code == MeasurementStreamCodes.TimeDelta3Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta3;
                }
                else if (code == MeasurementStreamCodes.TimeDelta4Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta4;
                }
                else if (code == MeasurementStreamCodes.Timestamp2)
                {
                    Timestamp = m_prevTimestamp2;
                }
                else if (code == MeasurementStreamCodes.TimeXOR7Bit)
                {
                    Timestamp = m_prevTimestamp1 ^ (long)Encoding7Bit.ReadUInt64(m_buffer.Data, ref m_buffer.Position);
                }
                else
                {
                    throw new Exception("Programming Error.");
                }

                //Save the smallest delta time
                long minDelta = Math.Abs(m_prevTimestamp1 - Timestamp);

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
                m_prevTimestamp1 = Timestamp;
                code = m_lastPoint.ReadCode(m_bitStream);
            }
            else
            {
                Timestamp = m_prevTimestamp1;
            }

            if (code < MeasurementStreamCodes.Quality2)
                throw new Exception("Expecting higher code");

            if (code <= MeasurementStreamCodes.Quality7Bit32)
            {
                if (code == MeasurementStreamCodes.Quality2)
                {
                    Quality = nextPoint.PrevQuality2;
                }
                else if (code == MeasurementStreamCodes.Quality7Bit32)
                {
                    Quality = Encoding7Bit.ReadUInt32(m_buffer.Data, ref m_buffer.Position);
                }
                nextPoint.PrevQuality2 = nextPoint.PrevQuality1;
                nextPoint.PrevQuality1 = Quality;
                code = m_lastPoint.ReadCode(m_bitStream);
            }
            else
            {
                Quality = nextPoint.PrevQuality1;
            }

            if (code < 32)
                throw new Exception("Programming Error. Expecting a value quality code.");

            nextPoint.ReadValue(code, out Value);
            m_lastPoint = nextPoint;
        }

    }

}
