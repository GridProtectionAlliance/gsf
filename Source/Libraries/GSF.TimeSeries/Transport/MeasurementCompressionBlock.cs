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

        public MeasurementCompressionBlock()
        {
            m_lastPoint = new PointMetaData();
            m_lastPoint.PointID = -1;
            m_buffer = new byte[BufferSize];
            m_previousTimestamp = 0;
            m_points = new Dictionary<ushort, PointMetaData>();

            Clear();
        }

        // Maximum is 41 there abouts, but leaving room for commands and error calculating.
        public bool CanAddMeasurements => m_index < BufferSize - 60;

        // Maximum is 2, but wanted to leave room for error calculating.
        public bool CanAddCommands => m_index < BufferSize - 4;

        public int BufferLength => m_index;

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

            if (!m_points.TryGetValue(id, out point))
            {
                m_lastMeasurementHeaderIndex = -1;
                point = new PointMetaData();
                point.PointID = m_points.Count;
                m_lastPoint.ExpectedNextPointID = point.PointID;
                m_points.Add(id, point);
                buffer[index++] = 5;
                Buffer.BlockCopy(BitConverter.GetBytes(id), 0, buffer, index, 2);
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
                buffer[index++] = (byte)quality;
                buffer[index++] = (byte)(quality >> 8);
                buffer[index++] = (byte)(quality >> 16);
                buffer[index++] = (byte)(quality >> 24);
            }

            if (bitsChanged > 0xFFFFFFu)
            {
                buffer[index++] = (byte)bitsChanged;
                buffer[index++] = (byte)(bitsChanged >> 8);
                buffer[index++] = (byte)(bitsChanged >> 16);
                buffer[index++] = (byte)(bitsChanged >> 24);
            }
            else if (bitsChanged > 0xFFFFu)
            {
                buffer[index++] = (byte)bitsChanged;
                buffer[index++] = (byte)(bitsChanged >> 8);
                buffer[index++] = (byte)(bitsChanged >> 16);
            }
            else if (bitsChanged > 0xFFu)
            {
                buffer[index++] = (byte)bitsChanged;
                buffer[index++] = (byte)(bitsChanged >> 8);
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
