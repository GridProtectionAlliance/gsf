using System;

namespace OGE.MeasurementStream
{
    internal abstract class BitStream
    {
        public abstract void WriteCode(int code);
        public abstract void WriteCode4(int code, int value);
        public abstract int ReadBits6();
        public abstract int ReadBits4();
        public abstract int ReadBit();
    }

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
                if (m_bitCount < 4)
                {
                    m_bitCount += 8;
                    m_cache = m_cache << 8 | m_parent.m_buffer.Data[m_parent.m_buffer.Position++];
                }
                m_bitCount -= 4;
                return (m_cache >> m_bitCount) & 15;
            }

            public override int ReadBits6()
            {
                return ReadBit() << 5 | ReadBit() << 4 | ReadBit() << 3 | ReadBit() << 2 | ReadBit() << 1 | ReadBit();
                if (m_bitCount < 5)
                {
                    m_bitCount += 8;
                    m_cache = m_cache << 8 | m_parent.m_buffer.Data[m_parent.m_buffer.Position++];
                }
                m_bitCount -= 5;
                return (m_cache >> m_bitCount) & 31;
            }


        }

    }

}
