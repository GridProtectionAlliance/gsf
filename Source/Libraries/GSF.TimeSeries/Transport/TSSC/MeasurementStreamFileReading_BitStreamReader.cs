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

namespace GSF.TimeSeries.Transport.TSSC
{
    internal partial class MeasurementStreamFileReading
    {
        private class BitStreamReader : BitStream
        {
            /// <summary>
            /// The number of bits in m_bitStreamCache that are valid. 0 Means the bitstream is empty.
            /// </summary>
            private int m_bitCount;
            /// <summary>
            /// A cache of bits that need to be flushed to m_buffer when full. Bits filled starting from the right moving left.
            /// </summary>
            private int m_cache;

            private readonly MeasurementStreamFileReading m_parent;

            public BitStreamReader(MeasurementStreamFileReading parent)
            {
                m_parent = parent;
                Clear();
            }

            public bool IsEmpty => m_bitCount == 0;

            /// <summary>
            /// Resets the stream so it can be reused. All measurements must be registered again.
            /// </summary>
            public void Clear()
            {
                m_bitCount = 0;
                m_cache = 0;
            }

            public override int ReadBit()
            {
                if (m_bitCount == 0)
                {
                    m_bitCount = 8;
                    m_cache = m_parent.m_buffer.Data[m_parent.m_buffer.Position++];
                }
                m_bitCount--;
                return (m_cache >> m_bitCount) & 1;
            }

            public override void WriteCode(int code)
            {
                throw new NotSupportedException();
            }

            public override void WriteCode4(int code, int value)
            {
                throw new NotSupportedException();
            }

            public override int ReadBits4()
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

            public override int ReadBits5()
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


        }

    }

}
