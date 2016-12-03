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

namespace GSF.TimeSeries.Transport.TSSC
{
    internal partial class MeasurementStreamFileWriting
    {
        private class BitStreamWriter : BitStream
        {
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

            private MeasurementStreamFileWriting m_parent;

            public BitStreamWriter(MeasurementStreamFileWriting parent)
            {
                m_parent = parent;
                Reinitialize();
            }

            /// <summary>
            /// Resets the stream so it can be reused. All measurements must be registered again.
            /// </summary>
            public void Reinitialize()
            {
                m_bitStreamBufferIndex = -1;
                m_bitStreamCacheBitCount = 0;
                m_bitStreamCache = 0;
            }

            public override void WriteCode(int code)
            {
                int len;
                m_parent.m_lastPoint.TransformCode(ref code, out len);

                WriteCode(code, len);
            }

            public override void WriteCode4(int code, int value)
            {
                int len;
                m_parent.m_lastPoint.TransformCode(ref code, out len);

                WriteCode((code << 4) | (value & 15), len + 4);
            }

            private void WriteCode(int code, int len)
            {
                if (m_bitStreamBufferIndex < 0)
                {
                    m_bitStreamBufferIndex = m_parent.m_buffer.Position++;
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
                    m_parent.m_buffer.Data[m_bitStreamBufferIndex] = (byte)(m_bitStreamCache >> (m_bitStreamCacheBitCount - 8));
                    m_bitStreamCacheBitCount -= 8;

                    if (m_bitStreamCacheBitCount > 0)
                    {
                        m_bitStreamBufferIndex = m_parent.m_buffer.Position++;
                    }
                    else
                    {
                        m_bitStreamBufferIndex = -1;
                    }
                }
            }

            public void Flush()
            {
                if (m_bitStreamCacheBitCount > 0)
                {
                    if (m_bitStreamBufferIndex < 0)
                    {
                        m_bitStreamBufferIndex = m_parent.m_buffer.Position++;
                    }

                    WriteCode(MeasurementStreamCodes.FlushBits);

                    if (m_bitStreamCacheBitCount > 7)
                    {
                        BitStreamEnd2();
                    }

                    if (m_bitStreamCacheBitCount > 0)
                    {
                        //Make up 8 bits by padding.
                        m_bitStreamCache <<= 8 - m_bitStreamCacheBitCount;
                        m_parent.m_buffer.Data[m_bitStreamBufferIndex] = (byte)m_bitStreamCache;
                        m_bitStreamCache = 0;
                        m_bitStreamBufferIndex = -1;
                        m_bitStreamCacheBitCount = 0;
                    }
                }
            }

            public override int ReadBits6()
            {
                throw new NotSupportedException();
            }

            public override int ReadBits4()
            {
                throw new NotSupportedException();
            }

            public override int ReadBit()
            {
                throw new NotSupportedException();
            }

        }

    }

}
