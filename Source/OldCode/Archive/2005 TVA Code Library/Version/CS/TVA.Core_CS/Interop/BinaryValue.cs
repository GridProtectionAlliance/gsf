//*******************************************************************************************************
//  BinaryData.cs
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
//  10/03/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Interop
{
    /// <summary>Represents a binary data sample stored as a byte array, but implicitly castable to most common native types.</summary>
    public class BinaryData
    {
        #region [ Members ]

        // Fields
        private byte[] m_dataSample;

        #endregion

        #region [ Constructors ]

        public BinaryData(byte[] dataSample)
        {
            m_dataSample = dataSample;
        }

        #endregion

        #region [ Properties ]

        public byte[] Value
        {
            get
            {
                return m_dataSample;
            }
            set
            {
                m_dataSample = value;
            }
        }

        #endregion

        #region [ Methods ]

        public Byte ToByte()
        {
            return (Byte)this;
        }

        public Int16 ToInt16()
        {
            return (Int16)this;
        }

        [CLSCompliant(false)]
        public UInt16 ToUInt16()
        {
            return (UInt16)this;
        }

        public Int24 ToInt24()
        {
            return (Int24)this;
        }

        [CLSCompliant(false)]
        public UInt24 ToUInt24()
        {
            return (UInt24)this;
        }

        public Int32 ToInt32()
        {
            return (Int32)this;
        }

        [CLSCompliant(false)]
        public UInt32 ToUInt32()
        {
            return (UInt32)this;
        }

        public Int64 ToInt64()
        {
            return (Int64)this;
        }

        [CLSCompliant(false)]
        public UInt64 ToUInt64()
        {
            return (UInt64)this;
        }

        public Single ToSingle()
        {
            return (Single)this;
        }

        public Double ToDouble()
        {
            return (Double)this;
        }

        #endregion

        #region [ Operators ]

        public static implicit operator Byte(BinaryData value)
        {
            return value.m_dataSample[0];
        }

        public static implicit operator BinaryData(Byte value)
        {
            return new BinaryData(new byte[] { value });
        }

        public static implicit operator Int16(BinaryData value)
        {
            return BitConverter.ToInt16(value.m_dataSample, 0);
        }

        public static implicit operator BinaryData(Int16 value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(BinaryData value)
        {
            return BitConverter.ToUInt16(value.m_dataSample, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryData(UInt16 value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        public static implicit operator Int24(BinaryData value)
        {
            return Int24.GetValue(value.m_dataSample, 0);
        }

        public static implicit operator BinaryData(Int24 value)
        {
            return new BinaryData(Int24.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(BinaryData value)
        {
            return UInt24.GetValue(value.m_dataSample, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryData(UInt24 value)
        {
            return new BinaryData(UInt24.GetBytes(value));
        }

        public static implicit operator Int32(BinaryData value)
        {
            return BitConverter.ToInt32(value.m_dataSample, 0);
        }

        public static implicit operator BinaryData(Int32 value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(BinaryData value)
        {
            return BitConverter.ToUInt32(value.m_dataSample, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryData(UInt32 value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        public static implicit operator Int64(BinaryData value)
        {
            return BitConverter.ToInt64(value.m_dataSample, 0);
        }

        public static implicit operator BinaryData(Int64 value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(BinaryData value)
        {
            return BitConverter.ToUInt64(value.m_dataSample, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryData(UInt64 value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        public static implicit operator Single(BinaryData value)
        {
            return BitConverter.ToSingle(value.m_dataSample, 0);
        }

        public static implicit operator BinaryData(Single value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        public static implicit operator Double(BinaryData value)
        {
            return BitConverter.ToDouble(value.m_dataSample, 0);
        }

        public static implicit operator BinaryData(Double value)
        {
            return new BinaryData(BitConverter.GetBytes(value));
        }

        #endregion
    }
}
