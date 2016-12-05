using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OGE.MeasurementStream
{
    internal class ByteBuffer
    {
        public byte[] Data;
        public int Position;

        public ByteBuffer(int size)
        {
            Data = new byte[size];
        }

        public void Grow()
        {
            var data = new byte[Data.Length*2];
            Data.CopyTo(data,0);
            Data = data;
        }
    }
}
