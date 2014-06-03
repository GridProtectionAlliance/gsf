//******************************************************************************************************
//  ScalarElement.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/02/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;

namespace GSF.PQDIF.Physical
{
    /// <summary>
    /// Represents an <see cref="Element"/> which is a single value in a
    /// PQDIF file. Scalar elements are part of the physical structure of
    /// a PQDIF file. They exist within the body of a <see cref="Record"/>
    /// (contained by a <see cref="CollectionElement"/>).
    /// </summary>
    public class ScalarElement : Element
    {
        #region [ Members ]

        // Fields
        private byte[] m_value;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ScalarElement"/> class.
        /// </summary>
        public ScalarElement()
        {
            m_value = new byte[16];
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the type of the element.
        /// Returns <see cref="ElementType.Scalar"/>.
        /// </summary>
        public override ElementType TypeOfElement
        {
            get
            {
                return ElementType.Scalar;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the value of the scalar as the physical type defined
        /// by <see cref="Element.TypeOfValue"/> and returns it as a generic
        /// <see cref="object"/>.
        /// </summary>
        /// <returns>The value of the scalar.</returns>
        public object Get()
        {
            switch (TypeOfValue)
            {
                case PhysicalType.Boolean1:
                    return m_value[0] != 0;

                case PhysicalType.Boolean2:
                    return GetInt2() != 0;

                case PhysicalType.Boolean4:
                    return GetInt4() != 0;

                case PhysicalType.Char1:
                    return Encoding.ASCII.GetString(m_value, 0, 1);

                case PhysicalType.Char2:
                    return Encoding.Unicode.GetString(m_value, 0, 2);

                case PhysicalType.Integer1:
                    return (sbyte)m_value[0];

                case PhysicalType.Integer2:
                    return GetInt2();

                case PhysicalType.Integer4:
                    return GetInt4();

                case PhysicalType.UnsignedInteger1:
                    return m_value[0];

                case PhysicalType.UnsignedInteger2:
                    return GetUInt2();

                case PhysicalType.UnsignedInteger4:
                    return GetUInt4();

                case PhysicalType.Real4:
                    return GetReal4();

                case PhysicalType.Real8:
                    return GetReal8();

                case PhysicalType.Complex8:
                    return GetComplex8();

                case PhysicalType.Complex16:
                    return GetComplex16();

                case PhysicalType.Timestamp:
                    return GetTimestamp();

                case PhysicalType.Guid:
                    return GetGuid();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the value of this scalar as a 16-bit unsigned integer.
        /// </summary>
        /// <returns>The value as a 16-bit unsigned integer.</returns>
        public ushort GetUInt2()
        {
            return LittleEndian.ToUInt16(m_value, 0);
        }

        /// <summary>
        /// Sets the value of this scalar as a 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">The new value as a 16-bit unsigned integer.</param>
        public void SetUInt2(ushort value)
        {
            LittleEndian.CopyBytes(value, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as a 16-bit signed integer.
        /// </summary>
        /// <returns>The value as a 16-bit signed integer.</returns>
        public short GetInt2()
        {
            return LittleEndian.ToInt16(m_value, 0);
        }

        /// <summary>
        /// Sets the value of this scalar as a 16-bit signed integer.
        /// </summary>
        /// <param name="value">The new value as a 16-bit signed integer.</param>
        public void SetInt2(short value)
        {
            LittleEndian.CopyBytes(value, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as a 32-bit unsigned integer.
        /// </summary>
        /// <returns>The value as a 32-bit unsigned integer.</returns>
        public uint GetUInt4()
        {
            return LittleEndian.ToUInt32(m_value, 0);
        }

        /// <summary>
        /// Sets the value of this scalar as a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The new value as a 32-bit unsigned integer.</param>
        public void SetUInt4(uint value)
        {
            LittleEndian.CopyBytes(value, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as a 32-bit signed integer.
        /// </summary>
        /// <returns>The value as a 32-bit signed integer.</returns>
        public int GetInt4()
        {
            return LittleEndian.ToInt32(m_value, 0);
        }

        /// <summary>
        /// Sets the value of this scalar as a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The new value as a 32-bit signed integer.</param>
        public void SetInt4(int value)
        {
            LittleEndian.CopyBytes(value, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as a 4-byte boolean.
        /// </summary>
        /// <returns>The value as a 4-byte boolean.</returns>
        public bool GetBool4()
        {
            return LittleEndian.ToInt32(m_value, 0) != 0;
        }

        /// <summary>
        /// Sets the value of this scalar as a 4-byte boolean.
        /// </summary>
        /// <param name="value">The new value as a 4-byte boolean.</param>
        public void SetBool4(bool value)
        {
            LittleEndian.CopyBytes(value ? 1 : 0, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as a 32-bit floating point number.
        /// </summary>
        /// <returns>The value as a 32-bit floating point number.</returns>
        public float GetReal4()
        {
            return LittleEndian.ToSingle(m_value, 0);
        }

        /// <summary>
        /// Sets the value of this scalar as a 32-bit floating point number.
        /// </summary>
        /// <param name="value">The new value as a 32-bit floating point number.</param>
        public void SetReal4(float value)
        {
            LittleEndian.CopyBytes(value, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as a 64-bit floating point number.
        /// </summary>
        /// <returns>The value as a 64-bit floating point number.</returns>
        public double GetReal8()
        {
            return LittleEndian.ToDouble(m_value, 0);
        }

        /// <summary>
        /// Sets the value of this scalar as a 64-bit floating point number.
        /// </summary>
        /// <param name="value">The new value as a 64-bit floating point number.</param>
        public void SetReal8(double value)
        {
            LittleEndian.CopyBytes(value, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as an 8-byte complex number.
        /// </summary>
        /// <returns>The value as an 8-byte complex number.</returns>
        public ComplexNumber GetComplex8()
        {
            double real = LittleEndian.ToSingle(m_value, 0);
            double imaginary = LittleEndian.ToSingle(m_value, 4);
            return new ComplexNumber(real, imaginary);
        }

        /// <summary>
        /// Sets the value of this scalar as an 8-byte complex number.
        /// </summary>
        /// <param name="value">The new value as an 8-byte complex number.</param>
        public void SetComplex8(ComplexNumber value)
        {
            LittleEndian.CopyBytes((float)value.Real, m_value, 0);
            LittleEndian.CopyBytes((float)value.Imaginary, m_value, 4);
        }

        /// <summary>
        /// Gets the value of this scalar as a 16-byte complex number.
        /// </summary>
        /// <returns>The value as a 16-byte complex number.</returns>
        public ComplexNumber GetComplex16()
        {
            double real = LittleEndian.ToDouble(m_value, 0);
            double imaginary = LittleEndian.ToDouble(m_value, 8);
            return new ComplexNumber(real, imaginary);
        }

        /// <summary>
        /// Sets the value of this scalar as a 16-byte complex number.
        /// </summary>
        /// <param name="value">The new value as a 16-byte complex number.</param>
        public void SetComplex16(ComplexNumber value)
        {
            LittleEndian.CopyBytes(value.Real, m_value, 0);
            LittleEndian.CopyBytes(value.Imaginary, m_value, 0);
        }

        /// <summary>
        /// Gets the value of this scalar as a globally unique identifier.
        /// </summary>
        /// <returns>The value as a globally unique identifier.</returns>
        public Guid GetGuid()
        {
            return new Guid(m_value);
        }

        /// <summary>
        /// Sets the value of this scalar as a globally unique identifier.
        /// </summary>
        /// <param name="value">The new value as a globally unique identifier.</param>
        public void SetGuid(Guid value)
        {
            m_value = value.ToByteArray();
        }

        /// <summary>
        /// Gets the value of this scalar as <see cref="DateTime"/>.
        /// </summary>
        /// <returns>The value of this scalar as a <see cref="DateTime"/>.</returns>
        public DateTime GetTimestamp()
        {
            DateTime epoch = new DateTime(1900, 1, 1);
            uint days = LittleEndian.ToUInt32(m_value, 0);
            double seconds = LittleEndian.ToDouble(m_value, 4);

            // Timestamps in a PQDIF file are represented by two separate numbers, one being the number of
            // days since January 1, 1900 and the other being the number of seconds since midnight. The
            // standard implementation also includes a constant for the number of days between January 1,
            // 1900 and January 1, 1970 to facilitate the conversion between PQDIF timestamps and UNIX
            // timestamps. However, the constant defined in the standard is 25569 days, whereas the actual
            // number of days between those two dates is 25567 days; a two day difference. That is why we
            // need to also subtract two days here when parsing PQDIF timestamps.
            return DateTime.SpecifyKind(epoch.AddDays(days - 2u).AddSeconds(seconds), DateTimeKind.Utc);
        }

        /// <summary>
        /// Sets the value of this scalar as a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">The new value of this scalar as a <see cref="DateTime"/>.</param>
        public void SetTimestamp(DateTime value)
        {
            DateTime epoch = new DateTime(1900, 1, 1);
            TimeSpan sinceEpoch = value - epoch;
            TimeSpan daySpan = TimeSpan.FromDays(Math.Floor(sinceEpoch.TotalDays));
            TimeSpan secondSpan = sinceEpoch - daySpan;

            // Timestamps in a PQDIF file are represented by two separate numbers, one being the number of
            // days since January 1, 1900 and the other being the number of seconds since midnight. The
            // standard implementation also includes a constant for the number of days between January 1,
            // 1900 and January 1, 1970 to facilitate the conversion between PQDIF timestamps and UNIX
            // timestamps. However, the constant defined in the standard is 25569 days, whereas the actual
            // number of days between those two dates is 25567 days; a two day difference. That is why we
            // need to also add two days here when creating PQDIF timestamps.
            LittleEndian.CopyBytes((uint)daySpan.TotalDays + 2u, m_value, 0);
            LittleEndian.CopyBytes(secondSpan.TotalSeconds, m_value, 0);
        }

        /// <summary>
        /// Gets the raw bytes of the value that this scalar represents.
        /// </summary>
        /// <returns>The value in bytes.</returns>
        public byte[] GetValue()
        {
            return m_value.BlockCopy(0, TypeOfValue.GetByteSize());
        }

        /// <summary>
        /// Sets the raw bytes of the value that this scalar represents.
        /// </summary>
        /// <param name="value">The array containing the bytes.</param>
        /// <param name="offset">The offset into the array at which the value starts.</param>
        public void SetValue(byte[] value, int offset)
        {
            Buffer.BlockCopy(value, offset, m_value, 0, TypeOfValue.GetByteSize());
        }

        /// <summary>
        /// Returns a string representation of the scalar.
        /// </summary>
        /// <returns>A string representation of the scalar.</returns>
        public override string ToString()
        {
            return string.Format("Scalar -- Type: {0}, Tag: {1}", TypeOfValue, TagOfElement);
        }

        #endregion
    }
}