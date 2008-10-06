//*******************************************************************************************************
//  BinaryValue.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/06/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace System
{
    /// <summary>
    /// Represents a binary data sample stored as a byte array ordered in the
    /// endianness of the OS, but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValue : BinaryValue<NativeEndianOrder>
    {
        #region [ Constructors ]

        public BinaryValue(byte[] buffer)
            : base(buffer)
        {
        }

        #endregion

        #region [ Operators ]

        // Operators cannot be inherited from the base class, they are static methods tied to their class instance

        public static implicit operator Byte(BinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator BinaryValue(Byte value)
        {
            return new BinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(BinaryValue value)
        {
            return value.ToInt16();
        }

        public static implicit operator BinaryValue(Int16 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(BinaryValue value)
        {
            return value.ToUInt16();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt16 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int24(BinaryValue value)
        {
            return value.ToInt24();
        }

        public static implicit operator BinaryValue(Int24 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(BinaryValue value)
        {
            return value.ToUInt24();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt24 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int32(BinaryValue value)
        {
            return value.ToInt32();
        }

        public static implicit operator BinaryValue(Int32 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(BinaryValue value)
        {
            return value.ToUInt32();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt32 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int64(BinaryValue value)
        {
            return value.ToInt64();
        }

        public static implicit operator BinaryValue(Int64 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(BinaryValue value)
        {
            return value.ToUInt64();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt64 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Single(BinaryValue value)
        {
            return value.ToSingle();
        }

        public static implicit operator BinaryValue(Single value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Double(BinaryValue value)
        {
            return value.ToDouble();
        }

        public static implicit operator BinaryValue(Double value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static BinaryValue()
        {
            m_endianOrder = NativeEndianOrder.Default;
        }

        #endregion
    }

    /// <summary>
    /// Represents a little-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class LittleBinaryValue : BinaryValue<LittleEndianOrder>
    {
        #region [ Constructors ]

        public LittleBinaryValue(byte[] buffer)
            : base(buffer)
        {
        }

        #endregion

        #region [ Operators ]

        // Operators cannot be inherited from the base class, they are static methods tied to their class instance

        public static implicit operator Byte(LittleBinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator LittleBinaryValue(Byte value)
        {
            return new LittleBinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(LittleBinaryValue value)
        {
            return value.ToInt16();
        }

        public static implicit operator LittleBinaryValue(Int16 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(LittleBinaryValue value)
        {
            return value.ToUInt16();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt16 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int24(LittleBinaryValue value)
        {
            return value.ToInt24();
        }

        public static implicit operator LittleBinaryValue(Int24 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(LittleBinaryValue value)
        {
            return value.ToUInt24();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt24 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int32(LittleBinaryValue value)
        {
            return value.ToInt32();
        }

        public static implicit operator LittleBinaryValue(Int32 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(LittleBinaryValue value)
        {
            return value.ToUInt32();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt32 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int64(LittleBinaryValue value)
        {
            return value.ToInt64();
        }

        public static implicit operator LittleBinaryValue(Int64 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(LittleBinaryValue value)
        {
            return value.ToUInt64();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt64 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Single(LittleBinaryValue value)
        {
            return value.ToSingle();
        }

        public static implicit operator LittleBinaryValue(Single value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Double(LittleBinaryValue value)
        {
            return value.ToDouble();
        }

        public static implicit operator LittleBinaryValue(Double value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static LittleBinaryValue()
        {
            m_endianOrder = LittleEndianOrder.Default;
        }

        #endregion
    }

    /// <summary>
    /// Represents a big-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BigBinaryValue : BinaryValue<BigEndianOrder>
    {
        #region [ Constructors ]

        public BigBinaryValue(byte[] buffer)
            : base(buffer)
        {
        }

        #endregion

        #region [ Operators ]

        // Operators cannot be inherited from the base class, they are static methods tied to their class instance

        public static implicit operator Byte(BigBinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator BigBinaryValue(Byte value)
        {
            return new BigBinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(BigBinaryValue value)
        {
            return value.ToInt16();
        }

        public static implicit operator BigBinaryValue(Int16 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(BigBinaryValue value)
        {
            return value.ToUInt16();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt16 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int24(BigBinaryValue value)
        {
            return value.ToInt24();
        }

        public static implicit operator BigBinaryValue(Int24 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(BigBinaryValue value)
        {
            return value.ToUInt24();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt24 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int32(BigBinaryValue value)
        {
            return value.ToInt32();
        }

        public static implicit operator BigBinaryValue(Int32 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(BigBinaryValue value)
        {
            return value.ToUInt32();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt32 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int64(BigBinaryValue value)
        {
            return value.ToInt64();
        }

        public static implicit operator BigBinaryValue(Int64 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(BigBinaryValue value)
        {
            return value.ToUInt64();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt64 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Single(BigBinaryValue value)
        {
            return value.ToSingle();
        }

        public static implicit operator BigBinaryValue(Single value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Double(BigBinaryValue value)
        {
            return value.ToDouble();
        }

        public static implicit operator BigBinaryValue(Double value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static BigBinaryValue()
        {
            m_endianOrder = BigEndianOrder.Default;
        }

        #endregion
    }

    /// <summary>
    /// Represents a binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValue<TEndianOrder> where TEndianOrder : EndianOrder
    {
        #region [ Members ]

        // Fields
        protected byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        protected BinaryValue(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            m_buffer = buffer;
        }

        #endregion

        #region [ Properties ]

        public byte[] Buffer
        {
            get
            {
                return m_buffer;
            }
            set
            {
                m_buffer = value;
            }
        }

        #endregion

        #region [ Methods ]

        public Byte ToByte()
        {
            ValidateBufferLength(TypeCode.Byte, sizeof(Byte));
            return m_buffer[0];
        }

        public Int16 ToInt16()
        {
            ValidateBufferLength(TypeCode.Int16, sizeof(Int16));
            return m_endianOrder.ToInt16(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt16 ToUInt16()
        {
            ValidateBufferLength(TypeCode.UInt16, sizeof(UInt16));
            return m_endianOrder.ToUInt16(m_buffer, 0);
        }

        public Int24 ToInt24()
        {
            // There is no system type code for Int24
            if (m_buffer.Length < 3)
                throw new InvalidOperationException("Binary value buffer is too small to represent a Int24 - buffer length needs to be at least 3");

            return m_endianOrder.ToInt24(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt24 ToUInt24()
        {
            // There is no system type code for UInt24
            if (m_buffer.Length < 3)
                throw new InvalidOperationException("Binary value buffer is too small to represent a UInt24 - buffer length needs to be at least 3");

            return m_endianOrder.ToUInt24(m_buffer, 0);
        }

        public Int32 ToInt32()
        {
            ValidateBufferLength(TypeCode.Int32, sizeof(Int32));
            return m_endianOrder.ToInt32(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt32 ToUInt32()
        {
            ValidateBufferLength(TypeCode.UInt32, sizeof(UInt32));
            return m_endianOrder.ToUInt32(m_buffer, 0);
        }

        public Int64 ToInt64()
        {
            ValidateBufferLength(TypeCode.Int64, sizeof(Int64));
            return m_endianOrder.ToInt64(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt64 ToUInt64()
        {
            ValidateBufferLength(TypeCode.UInt64, sizeof(UInt64));
            return m_endianOrder.ToUInt64(m_buffer, 0);
        }

        public Single ToSingle()
        {
            ValidateBufferLength(TypeCode.Single, sizeof(Single));
            return m_endianOrder.ToSingle(m_buffer, 0);
        }

        public Double ToDouble()
        {
            ValidateBufferLength(TypeCode.Double, sizeof(Double));
            return m_endianOrder.ToDouble(m_buffer, 0);
        }

        private void ValidateBufferLength(TypeCode typeCode, int size)
        {
            if (m_buffer.Length < size)
                throw new InvalidOperationException(string.Format("Binary value buffer is too small to represent a {0} - buffer length needs to be at least {1}", typeCode, size));
        }

        #endregion

        #region [ Static ]

        // Because each "typed" instance will be it's own class - each will have its own static variable instance
        protected static TEndianOrder m_endianOrder;

        #endregion
    }
}
