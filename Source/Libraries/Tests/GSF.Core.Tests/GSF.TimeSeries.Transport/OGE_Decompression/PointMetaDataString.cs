using System;
using System.Net;

namespace OGE.MeasurementStream
{
    internal class PointMetaDataString
        : PointMetaData
    {
        public class Codes64
        {
            public const byte Value1 = 32;
            public const byte Value2 = 33;
            public const byte Value3 = 34;
            public const byte ValueZero = 35;
            public const byte ValueXOR8 = 36;
            public const byte ValueXOR16 = 37;
            public const byte ValueXOR24 = 38;
            public const byte ValueXOR32 = 39;
            public const byte ValueXOR40 = 40;
            public const byte ValueXOR48 = 41;
            public const byte ValueXOR56 = 42;
            public const byte ValueXOR64 = 43;
        }

        const ulong Bits56 = 0xFFFFFFFFFFFFFFu;
        const ulong Bits48 = 0xFFFFFFFFFFFFu;
        const ulong Bits40 = 0xFFFFFFFFFFu;
        const uint Bits32 = 0xFFFFFFFFu;
        const uint Bits24 = 0xFFFFFFu;
        const uint Bits16 = 0xFFFFu;
        const uint Bits8 = 0xFFu;
        const uint Bits0 = 0x0u;

        public ulong PrevValue1;
        public ulong PrevValue2;
        public ulong PrevValue3;

        public PointMetaDataString(ByteBuffer buffer, BitStream bitStream, MeasurementTypeCode code, int referenceId)
            : base(buffer, bitStream, code, referenceId)
        {
            Mode = 4;
            Mode4_1 = Codes64.Value1;
            Mode4_01 = Codes64.Value2;
            Mode4_001 = Codes64.Value3;
        }

        public override void WriteValue(UnionValues currentValue)
        {
            ulong valueRaw = currentValue.ValueUInt64;

            if (PrevValue1 == valueRaw)
            {
                BitStream.WriteCode(Codes64.Value1);
            }
            else if (PrevValue2 == valueRaw)
            {
                BitStream.WriteCode(Codes64.Value2);
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else if (PrevValue3 == valueRaw)
            {
                BitStream.WriteCode(Codes64.Value3);
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else if (valueRaw == 0)
            {
                BitStream.WriteCode(Codes64.ValueZero);
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = 0;
            }
            else
            {
                ulong bitsChanged = valueRaw ^ PrevValue1;

                if (bitsChanged == Bits0)
                {
                    throw new Exception("Programming Error");
                }
                else if (bitsChanged <= Bits8)
                {
                    BitStream.WriteCode(Codes64.ValueXOR8);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Position++;
                }
                else if (bitsChanged <= Bits16)
                {
                    BitStream.WriteCode(Codes64.ValueXOR16);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Position = Buffer.Position + 2;
                }
                else if (bitsChanged <= Bits24)
                {
                    BitStream.WriteCode(Codes64.ValueXOR24);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Position = Buffer.Position + 3;
                }
                else if (bitsChanged <= Bits32)
                {
                    BitStream.WriteCode(Codes64.ValueXOR32);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Data[Buffer.Position + 3] = (byte)(bitsChanged >> 24);
                    Buffer.Position = Buffer.Position + 4;
                }
                else if (bitsChanged <= Bits40)
                {
                    BitStream.WriteCode(Codes64.ValueXOR40);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Data[Buffer.Position + 3] = (byte)(bitsChanged >> 24);
                    Buffer.Data[Buffer.Position + 4] = (byte)(bitsChanged >> 32);
                    Buffer.Position = Buffer.Position + 5;
                }
                else if (bitsChanged <= Bits48)
                {
                    BitStream.WriteCode(Codes64.ValueXOR48);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Data[Buffer.Position + 3] = (byte)(bitsChanged >> 24);
                    Buffer.Data[Buffer.Position + 4] = (byte)(bitsChanged >> 32);
                    Buffer.Data[Buffer.Position + 5] = (byte)(bitsChanged >> 40);
                    Buffer.Position = Buffer.Position + 6;
                }
                else if (bitsChanged <= Bits56)
                {
                    BitStream.WriteCode(Codes64.ValueXOR56);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Data[Buffer.Position + 3] = (byte)(bitsChanged >> 24);
                    Buffer.Data[Buffer.Position + 4] = (byte)(bitsChanged >> 32);
                    Buffer.Data[Buffer.Position + 5] = (byte)(bitsChanged >> 40);
                    Buffer.Data[Buffer.Position + 6] = (byte)(bitsChanged >> 48);
                    Buffer.Position = Buffer.Position + 7;
                }
                else
                {
                    BitStream.WriteCode(Codes64.ValueXOR64);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Data[Buffer.Position + 3] = (byte)(bitsChanged >> 24);
                    Buffer.Data[Buffer.Position + 4] = (byte)(bitsChanged >> 32);
                    Buffer.Data[Buffer.Position + 5] = (byte)(bitsChanged >> 40);
                    Buffer.Data[Buffer.Position + 6] = (byte)(bitsChanged >> 48);
                    Buffer.Data[Buffer.Position + 7] = (byte)(bitsChanged >> 56);
                    Buffer.Position = Buffer.Position + 8;
                }

                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }

        }

        public override void ReadValue(int code, UnionValues outValue)
        {
            throw new NotImplementedException();
        }
    }
}