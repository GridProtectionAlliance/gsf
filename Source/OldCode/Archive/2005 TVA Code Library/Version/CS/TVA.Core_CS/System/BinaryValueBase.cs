//*******************************************************************************************************
//  BinaryValueBase.cs
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
    /// Represents a binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValueBase<TEndianOrder> where TEndianOrder : EndianOrder
    {
        #region [ Members ]

        // Fields
        protected byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        protected BinaryValueBase(byte[] buffer)
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
