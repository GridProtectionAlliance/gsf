using System;

namespace OGE.MeasurementStream
{
    internal abstract class PointMetaData
    {
        public readonly MeasurementTypeCode Code;

        /// <summary>
        /// The local reference ID
        /// </summary>
        public readonly int ReferenceId;
        private readonly byte[] m_commandStats;

        public int PrevNextPointId1;

        public uint PrevQuality1;
        public uint PrevQuality2;

        private int m_commandsSentSinceLastChange = 0;

        protected byte Mode;

        //Mode 1 means no prefix.

        protected byte Mode2_1;

        protected byte Mode3_1;
        protected byte Mode3_01;

        protected byte Mode4_1;
        protected byte Mode4_01;
        protected byte Mode4_001;

        private int m_startupMode = 0;

        protected ByteBuffer Buffer;
        protected BitStream BitStream;

        protected PointMetaData(ByteBuffer buffer, BitStream bitStream, MeasurementTypeCode code, int referenceId)
        {
            Buffer = buffer;
            BitStream = bitStream;
            m_commandsSentSinceLastChange = 0;
            m_commandStats = new byte[64];
            Mode = 1;
            Code = code;
            ReferenceId = referenceId;
        }

        public abstract void WriteValue(UnionValues currentValue);

        public abstract void ReadValue(int code, UnionValues outValue);

        private void AdaptCommands2()
        {
            byte code1 = 0;
            int count1 = 0;

            byte code2 = 1;
            int count2 = 0;

            byte code3 = 2;
            int count3 = 0;

            int total = 0;

            for (int x = 0; x < m_commandStats.Length; x++)
            {
                int cnt = m_commandStats[x];
                m_commandStats[x] = 0;

                total += cnt;

                if (cnt > count3)
                {
                    if (cnt > count1)
                    {
                        code3 = code2;
                        count3 = count2;

                        code2 = code1;
                        count2 = count1;

                        code1 = (byte)x;
                        count1 = cnt;
                    }
                    else if (cnt > count2)
                    {
                        code3 = code2;
                        count3 = count2;

                        code2 = (byte)x;
                        count2 = cnt;
                    }
                    else
                    {
                        code3 = (byte)x;
                        count3 = cnt;
                    }
                }
            }

            int mode1Size = total * 6;
            int mode2Size = count1 * 1 + (total - count1) * 7;
            int mode3Size = count1 * 1 + count2 * 2 + (total - count1 - count2) * 8;
            int mode4Size = count1 * 1 + count2 * 2 + count3 * 3 + (total - count1 - count2 - count3) * 9;

            int minSize = int.MaxValue;
            minSize = Math.Min(minSize, mode1Size);
            minSize = Math.Min(minSize, mode2Size);
            minSize = Math.Min(minSize, mode3Size);
            minSize = Math.Min(minSize, mode4Size);

            if (minSize == mode1Size)
            {
                Mode = 1;
            }
            else if (minSize == mode2Size)
            {
                Mode = 2;
                Mode2_1 = code1;
            }
            else if (minSize == mode3Size)
            {
                Mode = 3;
                Mode3_1 = code1;
                Mode3_01 = code2;
            }
            else if (minSize == mode4Size)
            {
                Mode = 4;
                Mode4_1 = code1;
                Mode4_01 = code2;
                Mode4_001 = code3;
            }
            else
            {
                throw new Exception("Coding Error");
            }

            m_commandsSentSinceLastChange = 0;
        }

        public void TransformCode(ref int code, out int len)
        {
            m_commandsSentSinceLastChange++;
            m_commandStats[code]++;

            switch (Mode)
            {
                case 1:
                    len = 6;
                    break;
                case 2:
                    if (code == Mode2_1)
                    {
                        code = 1;
                        len = 1;
                    }
                    else
                    {
                        len = 7;
                    }
                    break;
                case 3:
                    if (code == Mode3_1)
                    {
                        code = 1;
                        len = 1;
                    }
                    else if (code == Mode3_01)
                    {
                        code = 1;
                        len = 2;
                    }
                    else
                    {
                        len = 8;
                    }
                    break;
                case 4:
                    if (code == Mode4_1)
                    {
                        code = 1;
                        len = 1;
                    }
                    else if (code == Mode4_01)
                    {
                        code = 1;
                        len = 2;
                    }
                    else if (code == Mode4_001)
                    {
                        code = 1;
                        len = 3;
                    }
                    else
                    {
                        len = 9;
                    }
                    break;
                default:
                    throw new Exception("Coding Error");
            }

            if (m_startupMode == 0 && m_commandsSentSinceLastChange > 5)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 1 && m_commandsSentSinceLastChange > 20)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 2 && m_commandsSentSinceLastChange > 100)
            {
                AdaptCommands2();
            }
        }

        public int ReadCode(BitStream bitStream)
        {
            int code = 0;
            switch (Mode)
            {
                case 1:
                    code = bitStream.ReadBits6();
                    break;
                case 2:
                    if (bitStream.ReadBit() == 1)
                    {
                        code = Mode2_1;
                    }
                    else
                    {
                        code = bitStream.ReadBits6();
                    }
                    break;
                case 3:
                    if (bitStream.ReadBit() == 1)
                    {
                        code = Mode3_1;
                    }
                    else if (bitStream.ReadBit() == 1)
                    {
                        code = Mode3_01;
                    }
                    else
                    {
                        code = bitStream.ReadBits6();
                    }
                    break;
                case 4:
                    if (bitStream.ReadBit() == 1)
                    {
                        code = Mode4_1;
                    }
                    else if (bitStream.ReadBit() == 1)
                    {
                        code = Mode4_01;
                    }
                    else if (bitStream.ReadBit() == 1)
                    {
                        code = Mode4_001;
                    }
                    else
                    {
                        code = bitStream.ReadBits6();
                    }
                    break;
                default:
                    throw new Exception("Unsupported compression mode");
            }
            m_commandStats[code]++;
            m_commandsSentSinceLastChange++;

            if (m_startupMode == 0 && m_commandsSentSinceLastChange > 5)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 1 && m_commandsSentSinceLastChange > 20)
            {
                m_startupMode++;
                AdaptCommands2();
            }
            else if (m_startupMode == 2 && m_commandsSentSinceLastChange > 100)
            {
                AdaptCommands2();
            }
            return code;
        }
    }
}