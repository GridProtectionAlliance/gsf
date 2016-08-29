//******************************************************************************************************
//  VectorElement.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
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
using System.IO;
using System.Text;

namespace GSF.PQDIF.Physical
{
    /// <summary>
    /// Represents an <see cref="Element"/> which is a collection of values
    /// in a PQDIF file. Vector elements are part of the physical structure
    /// of a PQDIF file. They exist within the body of a <see cref="Record"/>
    /// (contained by a <see cref="CollectionElement"/>).
    /// </summary>
    public class VectorElement : Element
    {
        #region [ Members ]

        // Fields
        private int m_size;
        private byte[] m_values;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of values in the vector.
        /// </summary>
        public int Size
        {
            get
            {
                return m_size;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Size must be >= 0", nameof(value));

                if (m_size != value)
                {
                    m_size = value;
                    Reallocate();
                }
            }
        }

        /// <summary>
        /// Gets the type of the element.
        /// Returns <see cref="ElementType.Vector"/>.
        /// </summary>
        public override ElementType TypeOfElement
        {
            get
            {
                return ElementType.Vector;
            }
        }

        /// <summary>
        /// Gets or sets the physical type of the value or values contained
        /// by the element.
        /// </summary>
        /// <remarks>
        /// This determines the data type and size of the
        /// value or values. The value of this property is only relevant when
        /// <see cref="TypeOfElement"/> is either <see cref="ElementType.Scalar"/>
        /// or <see cref="ElementType.Vector"/>.
        /// </remarks>
        public override PhysicalType TypeOfValue
        {
            get
            {
                return base.TypeOfValue;
            }
            set
            {
                if (base.TypeOfValue != value)
                {
                    base.TypeOfValue = value;
                    Reallocate();
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the value at the given index as the physical type defined
        /// by <see cref="TypeOfValue"/> and returns it as a generic
        /// <see cref="object"/>.
        /// </summary>
        /// <param name="index">The index of the value to be retrieved.</param>
        /// <returns>The value that was retrieved from the vector.</returns>
        public object Get(int index)
        {
            switch (TypeOfValue)
            {
                case PhysicalType.Boolean1:
                    return GetUInt1(index) != 0;

                case PhysicalType.Boolean2:
                    return GetInt2(index) != 0;

                case PhysicalType.Boolean4:
                    return GetInt4(index) != 0;

                case PhysicalType.Char1:
                    return Encoding.ASCII.GetString(m_values)[index];

                case PhysicalType.Char2:
                    return Encoding.Unicode.GetString(m_values)[index];

                case PhysicalType.Integer1:
                    return (sbyte)GetUInt1(index);

                case PhysicalType.Integer2:
                    return GetInt2(index);

                case PhysicalType.Integer4:
                    return GetInt4(index);

                case PhysicalType.UnsignedInteger1:
                    return GetUInt1(index);

                case PhysicalType.UnsignedInteger2:
                    return (ushort)GetInt2(index);

                case PhysicalType.UnsignedInteger4:
                    return GetUInt4(index);

                case PhysicalType.Real4:
                    return GetReal4(index);

                case PhysicalType.Real8:
                    return GetReal8(index);

                case PhysicalType.Complex8:
                    return new ComplexNumber((double)GetReal4(index * 2), (double)GetReal4(index * 2 + 1));

                case PhysicalType.Complex16:
                    return new ComplexNumber(GetReal8(index * 2), GetReal8(index * 2 + 1));

                case PhysicalType.Timestamp:
                    return GetTimestamp(index);

                case PhysicalType.Guid:
                    return new Guid(m_values.BlockCopy(index * 16, 16));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>                
        /// Sets the value at the given index as the physical type defined by <see cref="TypeOfValue"/>.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value to be stored.</param>
        public void Set(int index, object value)
        {
            char c;
            byte[] bytes;
            ComplexNumber complexNumber;

            switch (TypeOfValue)
            {
                case PhysicalType.Boolean1:
                    SetUInt1(index, Convert.ToBoolean(value) ? (byte)1 : (byte)0);
                    break;

                case PhysicalType.Boolean2:
                    SetInt2(index, Convert.ToBoolean(value) ? (short)1 : (short)0);
                    break;

                case PhysicalType.Boolean4:
                    SetInt4(index, Convert.ToBoolean(value) ? 1 : 0);
                    break;

                case PhysicalType.Char1:
                    c = Convert.ToChar(value);
                    bytes = Encoding.ASCII.GetBytes(c.ToString());
                    SetUInt1(index, bytes[0]);
                    break;

                case PhysicalType.Char2:
                    c = Convert.ToChar(value);
                    bytes = Encoding.Unicode.GetBytes(c.ToString());
                    SetInt2(index, BitConverter.ToInt16(bytes, 0));
                    break;

                case PhysicalType.Integer1:
                    SetInt1(index, Convert.ToSByte(value));
                    break;

                case PhysicalType.Integer2:
                    SetInt2(index, Convert.ToInt16(value));
                    break;

                case PhysicalType.Integer4:
                    SetInt4(index, Convert.ToInt32(value));
                    break;

                case PhysicalType.UnsignedInteger1:
                    SetUInt1(index, Convert.ToByte(value));
                    break;

                case PhysicalType.UnsignedInteger2:
                    SetUInt2(index, Convert.ToUInt16(value));
                    break;

                case PhysicalType.UnsignedInteger4:
                    SetUInt4(index, Convert.ToUInt32(value));
                    break;

                case PhysicalType.Real4:
                    SetReal4(index, Convert.ToSingle(value));
                    break;

                case PhysicalType.Real8:
                    SetReal8(index, Convert.ToDouble(value));
                    break;

                case PhysicalType.Complex8:
                    complexNumber = (ComplexNumber)value;
                    SetReal4(index * 2, (float)complexNumber.Real);
                    SetReal4(index * 2 + 1, (float)complexNumber.Imaginary);
                    break;

                case PhysicalType.Complex16:
                    complexNumber = (ComplexNumber)value;
                    SetReal8(index * 2, complexNumber.Real);
                    SetReal8(index * 2 + 1, complexNumber.Imaginary);
                    break;

                case PhysicalType.Timestamp:
                    SetTimestamp(index, Convert.ToDateTime(value));
                    break;

                case PhysicalType.Guid:
                    SetGuid(index, (Guid)value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets a value in this vector as an 8-bit unsigned integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as an 8-bit unsigned integer.</returns>
        public byte GetUInt1(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            return m_values[index];
        }

        /// <summary>
        /// Sets a value in this vector as an 8-bit unsigned integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of an 8-bit unsigned integer.</param>
        public void SetUInt1(int index, byte value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            m_values[index] = value;
        }

        /// <summary>
        /// Gets a value in this vector as a 16-bit unsigned integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a 16-bit unsigned integer.</returns>
        public ushort GetUInt2 (int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 2;
            return LittleEndian.ToUInt16(m_values, byteIndex);
        }

        /// <summary>
        /// Sets a value in this vector as a 16-bit unsigned integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of a 16-bit unsigned integer.</param>
        public void SetUInt2(int index, ushort value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 2;
            LittleEndian.CopyBytes(value, m_values, byteIndex);
        }

        /// <summary>
        /// Gets a value in this vector as a 32-bit unsigned integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a 32-bit unsigned integer.</returns>
        public uint GetUInt4(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 4;
            return LittleEndian.ToUInt32(m_values, byteIndex);
        }

        /// <summary>
        /// Sets a value in this vector as a 32-bit unsigned integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of a 32-bit unsigned integer.</param>
        public void SetUInt4(int index, uint value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 4;
            LittleEndian.CopyBytes(value, m_values, byteIndex);
        }

        /// <summary>
        /// Gets a value in this vector as an 8-bit signed integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as an 8-bit signed integer.</returns>
        public sbyte GetInt1(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            return (sbyte)m_values[index];
        }

        /// <summary>
        /// Sets a value in this vector as an 8-bit signed integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of an 8-bit signed integer.</param>
        public void SetInt1(int index, sbyte value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            m_values[index] = (byte)value;
        }

        /// <summary>
        /// Gets a value in this vector as a 16-bit signed integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a 16-bit signed integer.</returns>
        public short GetInt2(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 2;
            return LittleEndian.ToInt16(m_values, byteIndex);
        }

        /// <summary>
        /// Sets a value in this vector as a 16-bit signed integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of a 16-bit signed integer.</param>
        public void SetInt2(int index, short value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 2;
            LittleEndian.CopyBytes(value, m_values, byteIndex);
        }

        /// <summary>
        /// Gets a value in this vector as a 32-bit signed integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a 32-bit signed integer.</returns>
        public int GetInt4(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 4;
            return LittleEndian.ToInt32(m_values, byteIndex);
        }

        /// <summary>
        /// Sets a value in this vector as a 32-bit signed integer.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of a 32-bit signed integer.</param>
        public void SetInt4(int index, int value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 4;
            LittleEndian.CopyBytes(value, m_values, byteIndex);
        }

        /// <summary>
        /// Gets a value in this vector as a 32-bit floating point number.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a 32-bit floating point number.</returns>
        public float GetReal4(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 4;
            return LittleEndian.ToSingle(m_values, byteIndex);
        }

        /// <summary>
        /// Sets a value in this vector as a 32-bit floating point number.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of a 32-bit floating point number.</param>
        public void SetReal4(int index, float value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 4;
            LittleEndian.CopyBytes(value, m_values, byteIndex);
        }

        /// <summary>
        /// Gets a value in this vector as a 64-bit floating point number.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a 64-bit floating point number.</returns>
        public double GetReal8(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 8;
            return LittleEndian.ToDouble(m_values, byteIndex);
        }

        /// <summary>
        /// Sets a value in this vector as a 64-bit floating point number.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of a 64-bit floating point number.</param>
        public void SetReal8(int index, double value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 8;
            LittleEndian.CopyBytes(value, m_values, byteIndex);
        }

        /// <summary>
        /// Gets a value in this vector as a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a <see cref="DateTime"/>.</returns>
        public DateTime GetTimestamp(int index)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to retrieve values from uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 12;

            DateTime epoch = new DateTime(1900, 1, 1);
            uint days = LittleEndian.ToUInt32(m_values, byteIndex);
            double seconds = LittleEndian.ToDouble(m_values, byteIndex + 4);

            // Timestamps in a PQDIF file are represented by two separate numbers, one being the number of
            // days since January 1, 1900 and the other being the number of seconds since midnight. The
            // standard implementation also includes a constant for the number of days between January 1,
            // 1900 and January 1, 1970 to facilitate the conversion between PQDIF timestamps and UNIX
            // timestamps. However, the constant defined in the standard is 25569 days, whereas the actual
            // number of days between those two dates is 25567 days; a two day difference. That is why we
            // need to also subtract two days here when parsing PQDIF timestamps.
            return epoch.AddDays(days - 2u).AddSeconds(seconds);
        }

        /// <summary>
        /// Sets a value in this vector as a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value of a <see cref="DateTime"/>.</param>
        public void SetTimestamp(int index, DateTime value)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            int byteIndex = index * 12;

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
            LittleEndian.CopyBytes((uint)daySpan.TotalDays + 2u, m_values, byteIndex);
            LittleEndian.CopyBytes(secondSpan.TotalSeconds, m_values, byteIndex + 4);
        }

        /// <summary>
        /// Gets the value in this vector as a globally unique identifier.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value as a globally unique identifier.</returns>
        public Guid GetGuid(int index)
        {
            int byteIndex = index * 16;
            byte[] bytes = m_values.BlockCopy(byteIndex, 16);
            return new Guid(bytes);
        }

        /// <summary>
        /// Sets the value in this vector as a globally unique identifier.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The new value as a globally unique identifier.</param>
        public void SetGuid(int index, Guid value)
        {
            byte[] bytes = value.ToByteArray();
            int byteIndex = index * bytes.Length;
            Buffer.BlockCopy(bytes, 0, m_values, byteIndex, bytes.Length);
        }

        /// <summary>
        /// Gets the raw bytes of the values contained by this vector.
        /// </summary>
        /// <returns>The raw bytes of the values contained by this vector.</returns>
        public byte[] GetValues()
        {
            return m_values;
        }

        /// <summary>
        /// Sets the raw bytes of the values contained by this vector.
        /// </summary>
        /// <param name="values">The array that contains the raw bytes.</param>
        /// <param name="offset">The offset into the array at which the values start.</param>
        public void SetValues(byte[] values, int offset)
        {
            if ((object)m_values == null)
                throw new InvalidOperationException("Unable to insert values into uninitialized vector; set the size and physical type of the vector first");

            Buffer.BlockCopy(values, offset, m_values, 0, m_size * TypeOfValue.GetByteSize());
        }

        /// <summary>
        /// Returns a string representation of this vector.
        /// </summary>
        /// <returns>A string representation of this vector.</returns>
        public override string ToString()
        {
            return string.Format("Vector -- Type: {0}, Size: {1}, Tag: {2}", TypeOfValue, m_size, TagOfElement);
        }

        // Reallocates the byte array containing the vector data based on
        // the size of the vector and the physical type of the values.
        private void Reallocate()
        {
            if (TypeOfValue != 0)
                m_values = new byte[m_size * TypeOfValue.GetByteSize()];
        }

        #endregion
    }
}