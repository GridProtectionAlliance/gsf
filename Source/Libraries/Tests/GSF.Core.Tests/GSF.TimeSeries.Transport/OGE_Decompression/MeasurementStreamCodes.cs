namespace OGE.MeasurementStream
{
    internal class MeasurementStreamCodes
    {
        /// <summary>
        /// A new measurement has been registered.
        /// <para>byte MeasurementTypeCode</para>
        /// <para>7BitUInt MetadataLength</para>
        /// <para>byte[] Metadata</para>
        /// </summary>
        public const byte NewPointId = 0;
        /// <summary>
        /// A custom command sent by upper layer.
        /// <para>7BitULong Command</para>
        /// </summary>
        public const byte UserCommand = 1;
        /// <summary>
        /// A custom command sent by upper layer.
        /// <para>7BitULong Command</para>
        /// <para>7BitUInt DataLength</para>
        /// <para>byte[] Data</para>
        /// </summary>
        public const byte UserCommandWithData = 2;

        public const byte FlushBits = 3;

        public const byte PointIDXOR4 = 4;
        public const byte PointIDXOR8 = 5;
        public const byte PointIDXOR12 = 6;
        public const byte PointIDXOR16 = 7;
        public const byte PointIDXOR20 = 8;
        public const byte PointIDXOR24 = 9;
        public const byte PointIDXOR32 = 10;

        public const byte TimeDelta1Forward = 11;
        public const byte TimeDelta2Forward = 12;
        public const byte TimeDelta3Forward = 13;
        public const byte TimeDelta4Forward = 14;

        public const byte TimeDelta1Reverse = 15;
        public const byte TimeDelta2Reverse = 16;
        public const byte TimeDelta3Reverse = 17;
        public const byte TimeDelta4Reverse = 18;

        public const byte Timestamp2 = 19;
        public const byte TimeXOR7Bit = 20;

        public const byte Quality2 = 21;
        public const byte Quality7Bit32 = 22;
    }
}
