//******************************************************************************************************
//  MeasurementDecompressionBlock.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/11/2015 - Steven Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;

namespace GSF.TimeSeries.Transport
{
    internal enum DecompressionExitCode
    {
        EndOfStreamOccured,
        CommandRead,
        MeasurementRead
    }

    internal class MeasurementDecompressionBlock
    {
        private const int BufferSize = DataPublisher.MaxPacketSize;

        private readonly byte[] m_buffer;
        private int m_startIndex;
        private int m_stopIndex;
        private int m_nextRunLength;
        private long m_timeBucket;
        private PointMetaData m_lastPoint;
        private readonly List<PointMetaData> m_points;

        public MeasurementDecompressionBlock()
        {
            m_lastPoint = new PointMetaData();
            m_lastPoint.PointID = -1;

            m_points = new List<PointMetaData>();
            m_timeBucket = 0;
            m_nextRunLength = 0;

            m_buffer = new byte[BufferSize];
        }

        public void Reset()
        {
            m_lastPoint = new PointMetaData();
            m_lastPoint.PointID = -1;
            m_timeBucket = 0;
            m_nextRunLength = 0;
            m_startIndex = 0;
            m_stopIndex = 0;
            m_points.Clear();
        }

        public void Fill(Stream stream)
        {
            if (m_startIndex != m_stopIndex)
            {
                // I have data to retain.
                Buffer.BlockCopy(m_buffer, m_startIndex, m_buffer, 0, m_stopIndex - m_startIndex);
                m_stopIndex = m_stopIndex - m_startIndex;
                m_startIndex = 0;
            }
            else
            {
                m_startIndex = 0;
                m_stopIndex = 0;
            }

            int bytesToCopy = BufferSize - 60 - m_stopIndex;

            if (bytesToCopy > 0)
            {
                int bytesRead = stream.Read(m_buffer, m_stopIndex, bytesToCopy);

                if (bytesRead == 0)
                    throw new EndOfStreamException("End of stream has been encountered");

                m_stopIndex += bytesRead;
            }
        }

        public int Fill(byte[] buffer, int position, int length)
        {
            if (m_startIndex != m_stopIndex)
            {
                // I have data to retain.
                Buffer.BlockCopy(m_buffer, m_startIndex, m_buffer, 0, m_stopIndex - m_startIndex);
                m_stopIndex = m_stopIndex - m_startIndex;
                m_startIndex = 0;
            }
            else
            {
                m_startIndex = 0;
                m_stopIndex = 0;
            }

            int bytesToCopy = Math.Min(BufferSize - 60 - m_stopIndex, length);

            if (bytesToCopy > 0)
            {
                Buffer.BlockCopy(buffer, position, m_buffer, m_stopIndex, bytesToCopy);
                m_stopIndex += bytesToCopy;
                return bytesToCopy;
            }
            return 0;
        }

        private int RemainingBytes(int currentIndex) => m_stopIndex - currentIndex;

        public unsafe DecompressionExitCode GetMeasurement(out ushort id, out long timestamp, out uint quality, out float value, out byte userCommand)
        {
            id = 0;
            timestamp = 0;
            quality = 0;
            value = 0;
            userCommand = 0;

            TryAgain:
            int index = m_startIndex;

            if (RemainingBytes(index) <= 25) //Avoids a function call that almost always returns false
            {
                if (IsEndOfStream())
                    return DecompressionExitCode.EndOfStreamOccured;
            }

            byte code;

            if (m_nextRunLength > 0)
            {
                m_nextRunLength--;
                code = m_lastPoint.ExpectedNextCode;
            }
            else
            {
                code = m_buffer[index++];
                m_lastPoint.ExpectedNextCode = code;
                m_nextRunLength = code >> 6;
            }

            PointMetaData point;

            if ((code & 7) == 5)
            {
                m_nextRunLength = -1;
                point = new PointMetaData();
                point.SignalID = LittleEndian.ToUInt16(m_buffer, index);
                index += 2;

                point.PointID = m_points.Count;
                m_lastPoint.ExpectedNextPointID = m_points.Count;
                m_points.Add(point);
                m_startIndex = index;

                goto TryAgain;
            }

            if ((code & 7) == 6)
                throw new NotSupportedException();

            if ((code & 7) == 7)
            {
                m_nextRunLength = -1;
                userCommand = m_buffer[index++];
                m_startIndex = index;

                return DecompressionExitCode.CommandRead;
            }

            if ((code & 32) == 0)
            {
                point = m_points[m_lastPoint.ExpectedNextPointID];
            }
            else
            {
                point = m_points[(int)Encoding7Bit.ReadUInt32(m_buffer, ref index)];
                m_lastPoint.ExpectedNextPointID = point.PointID;
            }

            if ((code & 16) != 0)
            {
                point.LastQuality = (uint)(m_buffer[index] | m_buffer[index + 1] << 8 | m_buffer[index + 2] << 16 | m_buffer[index + 3] << 24);
                index += 4;
            }


            if ((code & 7) == 1)
            {
                point.LastValue ^= m_buffer[index];
                index += 1;
            }
            else if ((code & 7) == 2)
            {
                point.LastValue ^= (uint)(m_buffer[index] | m_buffer[index + 1] << 8);
                index += 2;
            }
            else if ((code & 7) == 3)
            {
                point.LastValue ^= (uint)(m_buffer[index] | m_buffer[index + 1] << 8 | m_buffer[index + 2] << 16);
                index += 3;
            }
            else if ((code & 7) == 4)
            {
                point.LastValue ^= (uint)(m_buffer[index] | m_buffer[index + 1] << 8 | m_buffer[index + 2] << 16 | m_buffer[index + 3] << 24);
                index += 4;
            }

            if ((code & 8) == 0)
            {
                timestamp = m_timeBucket;
            }
            else
            {
                timestamp = m_timeBucket ^ (long)Encoding7Bit.ReadUInt64(m_buffer, ref index);
                m_timeBucket = timestamp;
            }

            id = point.SignalID;
            quality = point.LastQuality;
            uint lastValue = point.LastValue;
            value = *(float*)&lastValue;

            m_lastPoint = point;
            m_startIndex = index;

            return DecompressionExitCode.MeasurementRead;
        }

        public bool IsEndOfStream()
        {
            int index = m_startIndex;

            if (RemainingBytes(index) > 25)
                return false;

            if (RemainingBytes(index) < 0)
                return true;

            byte code;

            if (m_nextRunLength > 0)
            {
                code = m_lastPoint.ExpectedNextCode;
            }
            else
            {
                if (RemainingBytes(index) < 1)
                    return true;

                code = m_buffer[index++];
            }

            if ((code & 7) == 5)
                return RemainingBytes(index) < 16;

            if ((code & 7) == 6)
                return false;

            if ((code & 7) == 7)
                return RemainingBytes(index) < 1;

            if ((code & 32) != 0)
            {
                index += Encoding7Bit.MeasureUInt32(m_buffer, index);

                if (RemainingBytes(index) < 0)
                    return true;
            }

            if ((code & 16) != 0)
            {
                if (RemainingBytes(index) < 4)
                    return true;

                index += 4;
            }

            if ((code & 7) == 1)
            {
                if (RemainingBytes(index) < 1)
                    return true;

                index += 1;
            }
            else if ((code & 7) == 2)
            {
                if (RemainingBytes(index) < 2)
                    return true;

                index += 2;
            }
            else if ((code & 7) == 3)
            {
                if (RemainingBytes(index) < 3)
                    return true;

                index += 3;
            }
            else if ((code & 7) == 4)
            {
                if (RemainingBytes(index) < 4)
                    return true;

                index += 4;
            }

            if ((code & 8) != 0)
            {
                index += Encoding7Bit.MeasureUInt64(m_buffer, index);

                if (RemainingBytes(index) < 0)
                    return true;
            }

            return false;
        }
    }
}
