using System;

namespace OGE.MeasurementStream
{
    internal class PointMetaDataInt32
        : PointMetaData
    {
        public class Codes32
        {
            public const byte Value1 = 32;
            public const byte Value2 = 33;
            public const byte Value3 = 34;
            public const byte ValueZero = 35;
            public const byte ValueXOR4 = 36;
            public const byte ValueXOR8 = 37;
            public const byte ValueXOR12 = 38;
            public const byte ValueXOR16 = 39;
            public const byte ValueXOR20 = 40;
            public const byte ValueXOR24 = 41;
            public const byte ValueXOR28 = 42;
            public const byte ValueXOR32 = 43;
        }

        const uint Bits28 = 0xFFFFFFFu;
        const uint Bits24 = 0xFFFFFFu;
        const uint Bits20 = 0xFFFFFu;
        const uint Bits16 = 0xFFFFu;
        const uint Bits12 = 0xFFFu;
        const uint Bits8 = 0xFFu;
        const uint Bits4 = 0xFu;
        const uint Bits0 = 0x0u;

        public uint PrevValue1;
        public uint PrevValue2;
        public uint PrevValue3;

        public PointMetaDataInt32(ByteBuffer buffer, BitStream bitStream, MeasurementTypeCode code, int referenceId)
            : base(buffer, bitStream, code, referenceId)
        {
            Mode = 4;
            Mode4_1 = Codes32.Value1;
            Mode4_01 = Codes32.Value2;
            Mode4_001 = Codes32.Value3;
        }

        public override void ReadValue(int code, UnionValues outValue)
        {
            uint valueRaw = 0;

            if (code == Codes32.Value1)
            {
                valueRaw = PrevValue1;
            }
            else if (code == Codes32.Value2)
            {
                valueRaw = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else if (code == Codes32.Value3)
            {
                valueRaw = PrevValue3;
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else if (code == Codes32.ValueZero)
            {
                valueRaw = 0;
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            else
            {
                switch (code)
                {
                    case Codes32.ValueXOR4:
                        valueRaw = (uint)BitStream.ReadBits4() ^ PrevValue1;
                        break;
                    case Codes32.ValueXOR8:
                        valueRaw = Buffer.Data[Buffer.Position] ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 1;
                        break;
                    case Codes32.ValueXOR12:
                        valueRaw = (uint)BitStream.ReadBits4() ^ (uint)(Buffer.Data[Buffer.Position] << 4) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 1;
                        break;
                    case Codes32.ValueXOR16:
                        valueRaw = Buffer.Data[Buffer.Position] ^ (uint)(Buffer.Data[Buffer.Position + 1] << 8) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 2;
                        break;
                    case Codes32.ValueXOR20:
                        valueRaw = (uint)BitStream.ReadBits4() ^ (uint)(Buffer.Data[Buffer.Position] << 4) ^ (uint)(Buffer.Data[Buffer.Position + 1] << 12) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 2;
                        break;
                    case Codes32.ValueXOR24:
                        valueRaw = Buffer.Data[Buffer.Position] ^ (uint)(Buffer.Data[Buffer.Position + 1] << 8) ^ (uint)(Buffer.Data[Buffer.Position + 2] << 16) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 3;
                        break;
                    case Codes32.ValueXOR28:
                        valueRaw = (uint)BitStream.ReadBits4() ^ (uint)(Buffer.Data[Buffer.Position] << 4) ^ (uint)(Buffer.Data[Buffer.Position + 1] << 12) ^ (uint)(Buffer.Data[Buffer.Position + 2] << 20) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 3;
                        break;
                    case Codes32.ValueXOR32:
                        valueRaw = Buffer.Data[Buffer.Position] ^ (uint)(Buffer.Data[Buffer.Position + 1] << 8) ^ (uint)(Buffer.Data[Buffer.Position + 2] << 16) ^ (uint)(Buffer.Data[Buffer.Position + 3] << 24) ^ PrevValue1;
                        Buffer.Position = Buffer.Position + 4;
                        break;
                    default:
                        throw new Exception("Code not valid");
                }

                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = valueRaw;
            }
            
            outValue.ValueUInt32 = valueRaw;
        }


        public override void WriteValue(UnionValues currentValue)
        {
            uint value = currentValue.ValueUInt32;

            if (PrevValue1 == value)
            {
                BitStream.WriteCode(Codes32.Value1);
            }
            else if (PrevValue2 == value)
            {
                BitStream.WriteCode(Codes32.Value2);
                PrevValue2 = PrevValue1;
                PrevValue1 = value;
            }
            else if (PrevValue3 == value)
            {
                BitStream.WriteCode(Codes32.Value3);
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = value;
            }
            else if (value == 0)
            {
                BitStream.WriteCode(Codes32.ValueZero);
                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = 0;
            }
            else
            {
                uint bitsChanged = value ^ PrevValue1;

                if (bitsChanged == Bits0)
                {
                    throw new Exception("Programming Error");
                }
                else if (bitsChanged <= Bits4)
                {
                    BitStream.WriteCode4(Codes32.ValueXOR4, (byte)bitsChanged);
                }
                else if (bitsChanged <= Bits8)
                {
                    BitStream.WriteCode(Codes32.ValueXOR8);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Position++;
                }
                else if (bitsChanged <= Bits12)
                {
                    BitStream.WriteCode4(Codes32.ValueXOR12, (byte)bitsChanged);

                    Buffer.Data[Buffer.Position] = (byte)(bitsChanged >> 4);
                    Buffer.Position++;
                }
                else if (bitsChanged <= Bits16)
                {
                    BitStream.WriteCode(Codes32.ValueXOR16);
                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Position = Buffer.Position + 2;
                }
                else if (bitsChanged <= Bits20)
                {
                    BitStream.WriteCode4(Codes32.ValueXOR20, (byte)bitsChanged);

                    Buffer.Data[Buffer.Position] = (byte)(bitsChanged >> 4);
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 12);
                    Buffer.Position = Buffer.Position + 2;
                }
                else if (bitsChanged <= Bits24)
                {
                    BitStream.WriteCode(Codes32.ValueXOR24);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Position = Buffer.Position + 3;
                }
                else if (bitsChanged <= Bits28)
                {
                    BitStream.WriteCode4(Codes32.ValueXOR28, (byte)bitsChanged);

                    Buffer.Data[Buffer.Position] = (byte)(bitsChanged >> 4);
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 12);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 20);
                    Buffer.Position = Buffer.Position + 3;
                }
                else
                {
                    BitStream.WriteCode(Codes32.ValueXOR32);

                    Buffer.Data[Buffer.Position] = (byte)bitsChanged;
                    Buffer.Data[Buffer.Position + 1] = (byte)(bitsChanged >> 8);
                    Buffer.Data[Buffer.Position + 2] = (byte)(bitsChanged >> 16);
                    Buffer.Data[Buffer.Position + 3] = (byte)(bitsChanged >> 24);
                    Buffer.Position = Buffer.Position + 4;
                }

                PrevValue3 = PrevValue2;
                PrevValue2 = PrevValue1;
                PrevValue1 = value;
            }
        }

    }
}