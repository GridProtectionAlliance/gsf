//******************************************************************************************************
//  MeasurementCompressionBlock.cs - Gbtc
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
    internal class PointMetaData
    {
        public int PointID;
        public uint LastValue;
        public uint LastQuality;
        public int ExpectedNextPointID;
        public byte ExpectedNextCode;
        public ushort SignalID;
    }

    internal class MeasurementCompressionBlock
    {
        private const int BufferSize = DataPublisher.MaxPacketSize;

        private readonly byte[] m_buffer;
        private int m_index;
        private int m_lastMeasurementHeaderIndex;
        private long m_previousTimestamp;
        private PointMetaData m_lastPoint;
        private readonly Dictionary<ushort, PointMetaData> m_points;
        private readonly List<PointMetaData> m_pointByID;

        public MeasurementCompressionBlock()
        {
            m_lastPoint = new PointMetaData();
            m_lastPoint.PointID = -1;
            m_buffer = new byte[BufferSize];
            m_previousTimestamp = 0;
            m_points = new Dictionary<ushort, PointMetaData>();
            m_pointByID = new List<PointMetaData>();
            Clear();
        }

        // Maximum is 41 there abouts, but leaving room for commands and error calculating.
        public bool CanAddMeasurements => m_index < BufferSize - 60;

        // Maximum is 2, but wanted to leave room for error calculating.
        public bool CanAddCommands => m_index < BufferSize - 4;

        public int BufferLength => m_index;

        public void Reset()
        {
            m_lastPoint = new PointMetaData();
            m_lastPoint.PointID = -1;
            m_previousTimestamp = 0;
            m_points.Clear();
            m_pointByID.Clear();
            Clear();
        }

        public void Clear()
        {
            m_lastMeasurementHeaderIndex = -1;
            m_index = 0;
        }

        public void CopyTo(Stream stream)
        {
            if (m_index > 0)
                stream.Write(m_buffer, 0, m_index);
        }

        public void CopyTo(byte[] buffer, int startingIndex)
        {
            if (m_index > 0)
                Buffer.BlockCopy(m_buffer, 0, buffer, startingIndex, m_index);
        }

        public void AddCommand(byte command)
        {
            if (!CanAddCommands)
                throw new Exception("Not enough buffer space to add command.");

            m_lastMeasurementHeaderIndex = -1;
            m_buffer[m_index++] = 255;
            m_buffer[m_index++] = command;
        }

        public unsafe void AddMeasurement(ushort id, long timestamp, uint quality, float value)
        {
            if (!CanAddMeasurements)
                throw new Exception("Not enough buffer space to add a new measurement.");

            byte[] buffer = m_buffer;
            int index = m_index;

            PointMetaData point;

            // Due to the nature of measurements routing in a common order (C37 always generates measurements in the same order)
            // there is an ~80% chance that a dictionary lookup can be avoided
            if (m_lastPoint.ExpectedNextPointID < m_pointByID.Count && m_pointByID[m_lastPoint.ExpectedNextPointID].SignalID == id)
            {
                point = m_pointByID[m_lastPoint.ExpectedNextPointID];
            }
            else if (!m_points.TryGetValue(id, out point))
            {
                m_lastMeasurementHeaderIndex = -1;
                point = new PointMetaData();
                point.PointID = m_points.Count;
                m_lastPoint.ExpectedNextPointID = point.PointID;
                m_points.Add(id, point);
                m_pointByID.Add(point);
                buffer[index++] = 5;
                LittleEndian.CopyBytes(id, buffer, index);
                index += 2;
            }

            byte code = 0;

            if (m_lastPoint.ExpectedNextPointID != point.PointID)
            {
                code |= 32;
                m_lastPoint.ExpectedNextPointID = point.PointID;
            }

            if (quality != point.LastQuality)
            {
                code |= 16;
                point.LastQuality = quality;
            }

            if (timestamp != m_previousTimestamp)
                code |= 8;

            uint bitsChanged = (*(uint*)&value) ^ point.LastValue;

            point.LastValue ^= bitsChanged;

            if (bitsChanged > 0xFFFFFFu)
                code |= 4;
            else if (bitsChanged > 0xFFFFu)
                code |= 3;
            else if (bitsChanged > 0xFFu)
                code |= 2;
            else if (bitsChanged > 0u)
                code |= 1;
            else
                code |= 0;

            // If the computed code is the same as what is expected based on the last measurement,
            // and there are enough bits available in the last encoded header to skip writing this code then do so.
            if (m_lastPoint.ExpectedNextCode == code && m_lastMeasurementHeaderIndex >= 0)
            {
                // Increment the previous code to increase the run length of headers that don't need to be changed.
                buffer[m_lastMeasurementHeaderIndex] += 64;

                if (buffer[m_lastMeasurementHeaderIndex] >= 192)
                    m_lastMeasurementHeaderIndex = -1;
            }
            else
            {
                // Write the code to the stream and updated expected values.
                m_lastMeasurementHeaderIndex = index;
                m_lastPoint.ExpectedNextCode = code;
                buffer[index++] = code;
            }

            if ((code & 32) != 0)
                Encoding7Bit.Write(buffer, ref index, (uint)point.PointID);

            if ((code & 16) != 0)
            {
                // The implemented method is faster than this method commented out
                // because not incrementing the index every time allows the CPU
                // to execute these operations simultaneously rather than
                //having to wait for a write barrier after every write.
                //buffer[index++] = (byte)quality;
                //buffer[index++] = (byte)(quality >> 8);
                //buffer[index++] = (byte)(quality >> 16);
                //buffer[index++] = (byte)(quality >> 24);

                buffer[index] = (byte)quality;
                buffer[index + 1] = (byte)(quality >> 8);
                buffer[index + 2] = (byte)(quality >> 16);
                buffer[index + 3] = (byte)(quality >> 24);
                
                index += 4;
            }

            if (bitsChanged > 0xFFFFFFu)
            {
                buffer[index] = (byte)bitsChanged;
                buffer[index + 1] = (byte)(bitsChanged >> 8);
                buffer[index + 2] = (byte)(bitsChanged >> 16);
                buffer[index + 3] = (byte)(bitsChanged >> 24);
                index += 4;
            }
            else if (bitsChanged > 0xFFFFu)
            {
                buffer[index] = (byte)bitsChanged;
                buffer[index + 1] = (byte)(bitsChanged >> 8);
                buffer[index + 2] = (byte)(bitsChanged >> 16);
                index += 3;
            }
            else if (bitsChanged > 0xFFu)
            {
                buffer[index] = (byte)bitsChanged;
                buffer[index + 1] = (byte)(bitsChanged >> 8);
                index += 2;
            }
            else if (bitsChanged > 0u)
            {
                buffer[index++] = (byte)bitsChanged;
            }

            if (timestamp != m_previousTimestamp)
            {
                Encoding7Bit.Write(buffer, ref index, (ulong)(timestamp ^ m_previousTimestamp));
                m_previousTimestamp = timestamp;
            }

            m_lastPoint = point;
            m_index = index;
        }
    }
}
