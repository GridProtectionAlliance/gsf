using System;
using TVA.Historian.Files;

namespace TVA.Historian.Packets
{
    /// <summary>
    /// Represents time series data transmitted in <see cref="PacketType101"/>.
    /// </summary>
    /// <seealso cref="PacketType101"/>
    public class PacketType101Data : ArchiveData
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 4          0-3        Int32      DatAWareId                                                    *
        // * 4          4-7        Int32      Time                                                          *
        // * 2          8-9        Int16      Flags (Quality & Milliseconds)                                *
        // * 4          10-13      Single     Value                                                         *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="PacketType101Data"/>.
        /// </summary>
        public new const int ByteCount = 14;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketType101Data"/> class.
        /// </summary>
        /// <param name="dataPoint">A time series data point.</param>
        public PacketType101Data(IDataPoint dataPoint)
            : base(dataPoint)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketType101Data"/> class.
        /// </summary>
        /// <param name="datawareId">DatAWare identifier of <see cref="PacketType101Data"/>.</param>
        /// <param name="time"><see cref="TimeTag"/> of <see cref="PacketType101Data"/>.</param>
        /// <param name="value">Floating-point value of <see cref="PacketType101Data"/>.</param>
        /// <param name="quality"><see cref="Quality"/> of <see cref="PacketType101Data"/>.</param>
        public PacketType101Data(int datawareId, TimeTag time, float value, Quality quality)
            : base(datawareId, time, value, quality)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketType101Data"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="PacketType101Data"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public PacketType101Data(byte[] binaryImage, int startIndex, int length)
            : base(1, binaryImage, startIndex, length)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public override int BinaryLength
        {
            get
            {
                return ByteCount;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="PacketType101Data"/>.
        /// </summary>
        public override byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(DatAWareId), 0, image, 0, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes((int)Time.Value), 0, image, 4, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes((short)Flags), 0, image, 8, 2);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(Value), 0, image, 10, 4);

                return image;
            }
        }

        /// <summary>
        /// Initializes <see cref="PacketType101Data"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="PacketType101Data"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="PacketType101Data"/>.</returns>
        public override int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= ByteCount)
            {
                // Binary image has sufficient data.
                DatAWareId = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex);
                Flags = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex + 8);
                Value = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 10);
                Time = new TimeTag(EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 4) +  // Seconds
                                   ((double)(Flags.GetMaskedValue(MillisecondMask) >> 5) / 1000));  // Milliseconds

                return ByteCount;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        #endregion
    }
}
