using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Globalization;
//using TVA.Common;
using TVA.Interop.Bit;

//*******************************************************************************************************
//  TVA.UInt24.vb - Representation of a 3-byte, 24-bit unsigned integer
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/09/2007 - J. Ritchie Carroll
//       Original version of source code generated
//
//*******************************************************************************************************



/// <summary>Represents a 24-bit unsigned integer.</summary>
/// <remarks>
/// <para>
/// This class behaves like most other intrinsic unsigned integers but allows a 3-byte, 24-bit integer implementation
/// that is often found in many digital-signal processing arenas and different kinds of protocol parsing.  An unsigned
/// 24-bit integer is typically used to save storage space on disk where its value range of 0 to 16777215 is sufficient,
/// but the unsigned Int16 value range of 0 to 65535 is too small.
/// </para>
/// <para>
/// This structure uses an UInt32 internally for storage and most other common expected integer functionality, so using
/// a 24-bit integer will not save memory.  However, if the 24-bit unsigned integer range (0 to 16777215) suits your
/// data needs you can save disk space by only storing the three bytes that this integer actually consumes.  You can do
/// this by calling the UInt24.GetBytes function to return a three byte binary array that can be serialized to the desired
/// destination and then calling the UInt24.GetValue function to restore the UInt24 value from those three bytes.
/// </para>
/// <para>
/// All the standard operators for the UInt24 have been fully defined for use with both UInt24 and UInt32 unsigned integers;
/// you should find that without the exception UInt24 can be compared and numerically calculated with an UInt24 or UInt32.
/// Necessary casting should be minimal and typical use should be very simple - just as if you are using any other native
/// unsigned integer.
/// </para>
/// </remarks>
namespace TVA
{
	[Serializable(), CLSCompliant(false)]public struct UInt24
	{
		
		
		#region " Public Constants "
		
		/// <summary>High byte bit-mask used when a 24-bit integer is stored within a 32-bit integer. This field is constant.</summary>
		public const UInt32 BitMask = 4278190080;
		
		/// <summary>Represents the largest possible value of an UInt24 as an UInt32. This field is constant.</summary>
		public const UInt32 MaxValue32 = 16777215;
		
		/// <summary>Represents the smallest possible value of an UInt24 as an UInt32. This field is constant.</summary>
		public const UInt32 MinValue32 = 0;
		
		#endregion
		
		#region " Member Fields "
		
		// We internally store the UInt24 value in a 4-byte integer for convenience
		private UInt32 m_value;
		
		private static UInt24 m_maxValue;
		private static UInt24 m_minValue;
		
		#endregion
		
		#region " Constructors "
		
		static UInt24()
		{
			
			m_maxValue = new UInt24(MaxValue32);
			m_minValue = new UInt24(MinValue32);
			
		}
		
		/// <summary>Creates 24-bit unsigned integer from an existing 24-bit unsigned integer.</summary>
		public UInt24(UInt24 value)
		{
			
			m_value = ApplyBitMask(System.Convert.ToUInt32(value));
			
		}
		
		/// <summary>Creates 24-bit unsigned integer from a 32-bit unsigned integer.</summary>
		/// <param name="value">32-bit unsigned integer to use as new 24-bit unsigned integer value.</param>
		/// <exception cref="OverflowException">Source values over 24-bit max range will cause an overflow exception.</exception>
		public UInt24(UInt32 value)
		{
			
			ValidateNumericRange(value);
			m_value = ApplyBitMask(value);
			
		}
		
		/// <summary>Creates 24-bit unsigned integer from three bytes at a specified position in a byte array.</summary>
		/// <param name="value">An array of bytes.</param>
		/// <param name="startIndex">The starting position within value.</param>
		/// <remarks>
		/// <para>You can use this constructor in-lieu of a System.BitConverter.ToUInt24 function.</para>
		/// <para>Bytes endian order assumed to match that of currently executing process architecture (little-endian on Intel platforms).</para>
		/// </remarks>
		public UInt24(byte[] value, int startIndex)
		{
			
			m_value = ApplyBitMask(System.Convert.ToUInt32(UInt24.GetValue(value, startIndex)));
			
		}
		
		#endregion
		
		#region " BitConverter Stand-in Operations "
		
		/// <summary>Returns the UInt24 value as an array of three bytes.</summary>
		/// <returns>An array of bytes with length 3.</returns>
		/// <remarks>
		/// <para>You can use this function in-lieu of a System.BitConverter.GetBytes function.</para>
		/// <para>Bytes will be returned in endian order of currently executing process architecture (little-endian on Intel platforms).</para>
		/// </remarks>
		public byte[] GetBytes()
		{
			
			// Return serialized 3-byte representation of UInt24
			return UInt24.GetBytes(this);
			
		}
		
		/// <summary>Returns the specified UInt24 value as an array of three bytes.</summary>
		/// <param name="value">UInt24 value to </param>
		/// <returns>An array of bytes with length 3.</returns>
		/// <remarks>
		/// <para>You can use this function in-lieu of a System.BitConverter.GetBytes function.</para>
		/// <para>Bytes will be returned in endian order of currently executing process architecture (little-endian on Intel platforms).</para>
		/// </remarks>
		public static byte[] GetBytes(UInt24 value)
		{
			
			// We use a 32-bit integer to store 24-bit integer internally
			byte[] int32Bytes = BitConverter.GetBytes(System.Convert.ToUInt32(value));
			byte[] int24Bytes = TVA.Common.CreateArray<byte>(3);
			
			if (BitConverter.IsLittleEndian)
			{
				// Copy little-endian bytes starting at index 0
				Buffer.BlockCopy(int32Bytes, 0, int24Bytes, 0, 3);
			}
			else
			{
				// Copy big-endian bytes starting at index 1
				Buffer.BlockCopy(int32Bytes, 1, int24Bytes, 0, 3);
			}
			
			// Return serialized 3-byte representation of UInt24
			return int24Bytes;
			
		}
		
		/// <summary>Returns a 24-bit unsigned integer from three bytes at a specified position in a byte array.</summary>
		/// <param name="value">An array of bytes.</param>
		/// <param name="startIndex">The starting position within value.</param>
		/// <returns>A 24-bit unsigned integer formed by three bytes beginning at startIndex.</returns>
		/// <remarks>
		/// <para>You can use this function in-lieu of a System.BitConverter.ToUInt24 function.</para>
		/// <para>Bytes endian order assumed to match that of currently executing process architecture (little-endian on Intel platforms).</para>
		/// </remarks>
		public static UInt24 GetValue(byte[] value, int startIndex)
		{
			
			// We use a 32-bit integer to store 24-bit integer internally
			byte[] bytes = TVA.Common.CreateArray<byte>(4);
			
			if (BitConverter.IsLittleEndian)
			{
				// Copy little-endian bytes starting at index 0 leaving byte at index 3 blank
				Buffer.BlockCopy(value, 0, bytes, 0, 3);
			}
			else
			{
				// Copy big-endian bytes starting at index 1 leaving byte at index 0 blank
				Buffer.BlockCopy(value, 0, bytes, 1, 3);
			}
			
			// Deserialize value
			return ((UInt24) (ApplyBitMask(BitConverter.ToUInt32(bytes, 0))));
			
		}
		
		#endregion
		
		#region " UInt24 Operators "
		
		// Every effort has been made to make UInt24 as cleanly interoperable with UInt32 as possible...
		
		#region " Comparison Operators "
		
		public static bool operator ==(UInt24 value1, UInt24 value2)
		{
			
			return value1.Equals(value2);
			
		}
		
		public static bool operator ==(UInt32 value1, UInt24 value2)
		{
			
			return value1.Equals(System.Convert.ToUInt32(value2));
			
		}
		
		public static bool operator ==(UInt24 value1, UInt32 value2)
		{
			
			return System.Convert.ToUInt32(value1).Equals(value2);
			
		}
		
		public static bool operator !=(UInt24 value1, UInt24 value2)
		{
			
			return ! value1.Equals(value2);
			
		}
		
		public static bool operator !=(UInt32 value1, UInt24 value2)
		{
			
			return ! value1.Equals(System.Convert.ToUInt32(value2));
			
		}
		
		public static bool operator !=(UInt24 value1, UInt32 value2)
		{
			
			return ! System.Convert.ToUInt32(value1).Equals(value2);
			
		}
		
		public static bool operator <(UInt24 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(value2) < 0);
			
		}
		
		public static bool operator <(UInt32 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(System.Convert.ToUInt32(value2)) < 0);
			
		}
		
		public static bool operator <(UInt24 value1, UInt32 value2)
		{
			
			return (value1.CompareTo(value2) < 0);
			
		}
		
		public static bool operator <=(UInt24 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(value2) <= 0);
			
		}
		
		public static bool operator <=(UInt32 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(System.Convert.ToUInt32(value2)) <= 0);
			
		}
		
		public static bool operator <=(UInt24 value1, UInt32 value2)
		{
			
			return (value1.CompareTo(value2) <= 0);
			
		}
		
		public static bool operator >(UInt24 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(value2) > 0);
			
		}
		
		public static bool operator >(UInt32 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(System.Convert.ToUInt32(value2)) > 0);
			
		}
		
		public static bool operator >(UInt24 value1, UInt32 value2)
		{
			
			return (value1.CompareTo(value2) > 0);
			
		}
		
		public static bool operator >=(UInt24 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(value2) >= 0);
			
		}
		
		public static bool operator >=(UInt32 value1, UInt24 value2)
		{
			
			return (value1.CompareTo(System.Convert.ToUInt32(value2)) >= 0);
			
		}
		
		public static bool operator >=(UInt24 value1, UInt32 value2)
		{
			
			return (value1.CompareTo(value2) >= 0);
			
		}
		
		#endregion
		
		#region " Type Conversion Operators "
		
		#region " Narrowing Conversions "
		
		public static explicit operator UInt24 (string value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static explicit operator UInt24 (decimal value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static explicit operator UInt24 (double value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static explicit operator UInt24 (float value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static explicit operator UInt24 (UInt64 value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static explicit operator UInt24 (UInt32 value)
		{
			
			return new UInt24(value);
			
		}
		
		public static explicit operator UInt24 (Int24 value)
		{
			
			return new UInt24(System.Convert.ToUInt32(value));
			
		}
		
		public static explicit operator Int24 (UInt24 value)
		{
			
			return new Int24(System.Convert.ToInt32(value));
			
		}
		
		public static explicit operator short (UInt24 value)
		{
			
			return ((short) (System.Convert.ToUInt32(value)));
			
		}
		
		public static explicit operator UInt16 (UInt24 value)
		{
			
			return System.Convert.ToUInt16(System.Convert.ToUInt32(value));
			
		}
		
		public static explicit operator byte (UInt24 value)
		{
			
			return ((byte) (System.Convert.ToUInt32(value)));
			
		}
		
		#endregion
		
		#region " Widening Conversions "
		
		public static UInt24 operator operator(byte value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static UInt24 operator operator(char value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static UInt24 operator operator(UInt16 value)
		{
			
			return new UInt24(Convert.ToUInt32(value));
			
		}
		
		public static int operator operator(UInt24 value)
		{
			
			return value.ToInt32(null);
			
		}
		
		public static UInt32 operator operator(UInt24 value)
		{
			
			return value.ToUInt32(null);
			
		}
		
		public static long operator operator(UInt24 value)
		{
			
			return value.ToInt64(null);
			
		}
		
		public static UInt64 operator operator(UInt24 value)
		{
			
			return value.ToUInt64(null);
			
		}
		
		public static double operator operator(UInt24 value)
		{
			
			return value.ToDouble(null);
			
		}
		
		public static float operator operator(UInt24 value)
		{
			
			return value.ToSingle(null);
			
		}
		
		public static decimal operator operator(UInt24 value)
		{
			
			return value.ToDecimal(null);
			
		}
		
		public static string operator operator(UInt24 value)
		{
			
			return value.ToString();
			
		}
		
		#endregion
		
		#endregion
		
		#region " Boolean and Bitwise Operators "
		
		public static bool operator IsTrue(UInt24 value)
		{
			
			return (value > 0);
			
		}
		
		public static bool operator IsFalse(UInt24 value)
		{
			
			return (value == 0);
			
		}
		
		public static UInt24 operator !(UInt24 value)
		{
			
			return ((UInt24) (ApplyBitMask(! System.Convert.ToUInt32(value))));
			
		}
		
		public static UInt24 operator &(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (ApplyBitMask(System.Convert.ToUInt32(value1) && System.Convert.ToUInt32(value2))));
			
		}
		
		public static UInt32 operator &(UInt32 value1, UInt24 value2)
		{
			
			return (value1 && System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator &(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) && value2);
			
		}
		
		public static UInt24 operator |(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (ApplyBitMask(System.Convert.ToUInt32(value1) || System.Convert.ToUInt32(value2))));
			
		}
		
		public static UInt32 operator |(UInt32 value1, UInt24 value2)
		{
			
			return (value1 || System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator |(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) || value2);
			
		}
		
		public static UInt24 operator ^(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (ApplyBitMask(System.Convert.ToUInt32(value1) ^ System.Convert.ToUInt32(value2))));
			
		}
		
		public static UInt32 operator ^(UInt32 value1, UInt24 value2)
		{
			
			return (value1 ^ System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator ^(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) ^ value2);
			
		}
		
		#endregion
		
		#region " Arithmetic Operators "
		
		public static UInt24 operator %(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (System.Convert.ToUInt32(value1) % System.Convert.ToUInt32(value2)));
			
		}
		
		public static UInt32 operator %(UInt32 value1, UInt24 value2)
		{
			
			return (value1 % System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator %(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) % value2);
			
		}
		
		public static UInt24 operator +(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (System.Convert.ToUInt32(value1) + System.Convert.ToUInt32(value2)));
			
		}
		
		public static UInt32 operator +(UInt32 value1, UInt24 value2)
		{
			
			return (value1 + System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator +(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) + value2);
			
		}
		
		public static UInt24 operator -(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (System.Convert.ToUInt32(value1) - System.Convert.ToUInt32(value2)));
			
		}
		
		public static UInt32 operator -(UInt32 value1, UInt24 value2)
		{
			
			return (value1 - System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator -(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) - value2);
			
		}
		
		public static UInt24 operator *(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (System.Convert.ToUInt32(value1) * System.Convert.ToUInt32(value2)));
			
		}
		
		public static UInt32 operator *(UInt32 value1, UInt24 value2)
		{
			
			return (value1 * System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator *(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) * value2);
			
		}
		
		public static UInt24 operator /(UInt24 value1, UInt24 value2)
		{
			
			return ((UInt24) (System.Convert.ToUInt32(value1) / System.Convert.ToUInt32(value2)));
			
		}
		
		public static UInt32 operator /(UInt32 value1, UInt24 value2)
		{
			
			return (value1 / System.Convert.ToUInt32(value2));
			
		}
		
		public static UInt32 operator /(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToUInt32(value1) / value2);
			
		}
		
		public static double operator /(UInt24 value1, UInt24 value2)
		{
			
			return (System.Convert.ToDouble(value1) / System.Convert.ToDouble(value2));
			
		}
		
		public static double operator /(UInt32 value1, UInt24 value2)
		{
			
			return (System.Convert.ToDouble(value1) / System.Convert.ToDouble(value2));
			
		}
		
		public static double operator /(UInt24 value1, UInt32 value2)
		{
			
			return (System.Convert.ToDouble(value1) / System.Convert.ToUInt32(value2));
			
		}
		
		public static double operator ^^(UInt24 value1, UInt24 value2)
		{
			
			return Math.Pow(System.Convert.ToDouble(value1), System.Convert.ToDouble(value2));
			
		}}
		
		public static double operator ^^(UInt32 value1, UInt24 value2)
		{
			
			return Math.Pow(System.Convert.ToDouble(value1), System.Convert.ToDouble(value2));
			
		}}
		
		public static double operator ^^(UInt24 value1, UInt32 value2)
		{
			
			return Math.Pow(System.Convert.ToDouble(value1), System.Convert.ToDouble(value2));
			
		}}
		
		public static UInt24 operator >>(UInt24 value, int shifts)
		{
			
			return ((UInt24) (ApplyBitMask(System.Convert.ToUInt32(value) >> shifts)));
			
		}
		
		public static UInt24 operator <<(UInt24 value, int shifts)
		{
			
			return ((UInt24) (ApplyBitMask(System.Convert.ToUInt32(value) << shifts)));
			
		}
		
		#endregion
		
		#endregion
		
		#region " UInt24 Specific Functions "
		
		/// <summary>Represents the largest possible value of an UInt24. This field is constant.</summary>
		public static UInt24 MaxValue
		{
			get
			{
				return m_maxValue;
			}
		}
		
		/// <summary>Represents the smallest possible value of an UInt24. This field is constant.</summary>
		public static UInt24 MinValue
		{
			get
			{
				return m_minValue;
			}
		}
		
		private static void ValidateNumericRange(UInt32 value)
		{
			
			if (value > MaxValue32)
			{
				throw (new OverflowException(string.Format("Value of {0} will not fit in a 24-bit unsigned integer", value)));
			}
			
		}
		
		private static UInt32 ApplyBitMask(UInt32 value)
		{
			
			// For unsigned values, all we do is clear all the high bits (keeps 32-bit unsigned number in 24-bit unsigned range)...
			return (value && ! BitMask);
			
		}
		
		#endregion
		
		#region " Standard Numeric Operations "
		
		/// <summary>
		/// Compares this instance to a specified object and returns an indication of their relative values.
		/// </summary>
		/// <param name="value">An object to compare, or null.</param>
		/// <returns>
		/// An unsigned number indicating the relative values of this instance and value. Returns less than zero
		/// if this instance is less than value, zero if this instance is equal to value, or greater than zero
		/// if this instance is greater than value.
		/// </returns>
		/// <exception cref="ArgumentException">value is not an UInt32 or UInt24.</exception>
		public int CompareTo(object value)
		{
			
			if (value == null)
			{
				return 1;
			}
			if (! value is UInt32&& ! value is UInt24)
			{
				throw (new ArgumentException("Argument must be an UInt32 or an UInt24"));
			}
			
			UInt32 num = System.Convert.ToUInt32(value);
			
			if (m_value < num)
			{
				return - 1;
			}
			if (m_value > num)
			{
				return 1;
			}
			
			return 0;
			
		}
		
		/// <summary>
		/// Compares this instance to a specified 32-bit unsigned integer and returns an indication of their
		/// relative values.
		/// </summary>
		/// <param name="value">An integer to compare.</param>
		/// <returns>
		/// An unsigned number indicating the relative values of this instance and value. Returns less than zero
		/// if this instance is less than value, zero if this instance is equal to value, or greater than zero
		/// if this instance is greater than value.
		/// </returns>
		public int CompareTo(UInt24 value)
		{
			
			return CompareTo(System.Convert.ToUInt32(value));
			
		}
		
		/// <summary>
		/// Compares this instance to a specified 32-bit unsigned integer and returns an indication of their
		/// relative values.
		/// </summary>
		/// <param name="value">An integer to compare.</param>
		/// <returns>
		/// An unsigned number indicating the relative values of this instance and value. Returns less than zero
		/// if this instance is less than value, zero if this instance is equal to value, or greater than zero
		/// if this instance is greater than value.
		/// </returns>
		public int CompareTo(UInt32 value)
		{
			
			if (m_value < value)
			{
				return - 1;
			}
			if (m_value > value)
			{
				return 1;
			}
			
			return 0;
			
		}
		
		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">An object to compare, or null.</param>
		/// <returns>
		/// True if obj is an instance of UInt32 or UInt24 and equals the value of this instance;
		/// otherwise, False.
		/// </returns>
		public override bool Equals(object obj)
		{
			
			if (obj is UInt32|| obj is UInt24)
			{
				return Equals(System.Convert.ToUInt32(obj));
			}
			return false;
			
		}
		
		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified UInt24 value.
		/// </summary>
		/// <param name="obj">An UInt24 value to compare to this instance.</param>
		/// <returns>
		/// True if obj has the same value as this instance; otherwise, False.
		/// </returns>
		public bool Equals(UInt24 obj)
		{
			
			return Equals(System.Convert.ToUInt32(obj));
			
		}
		
		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified UInt32 value.
		/// </summary>
		/// <param name="obj">An UInt32 value to compare to this instance.</param>
		/// <returns>
		/// True if obj has the same value as this instance; otherwise, False.
		/// </returns>
		public bool Equals(UInt32 obj)
		{
			
			return (m_value == obj);
			
		}
		
		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit unsigned integer hash code.
		/// </returns>
		public override int GetHashCode()
		{
			
			return TVA.Interop.BitwiseCast.ToInt32(m_value);
			
		}
		
		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation.
		/// </summary>
		/// <returns>
		/// The string representation of the value of this instance, consisting of a minus sign if
		/// the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.
		/// </returns>
		public override string ToString()
		{
			
			return m_value.ToString();
			
		}
		
		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation, using
		/// the specified format.
		/// </summary>
		/// <param name="format">A format string.</param>
		/// <returns>
		/// The string representation of the value of this instance as specified by format.
		/// </returns>
		public string ToString(string format)
		{
			
			return m_value.ToString(format);
			
		}
		
		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation using the
		/// specified culture-specific format information.
		/// </summary>
		/// <param name="provider">
		/// An System.IFormatProvider that supplies culture-specific formatting information.
		/// </param>
		/// <returns>
		/// The string representation of the value of this instance as specified by provider.
		/// </returns>
		public string ToString(IFormatProvider provider)
		{
			return this.ToString(provider);
		}
		
		public string ToString(IFormatProvider provider)
		{
			
			return m_value.ToString(provider);
			
		}
		
		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation using the
		/// specified format and culture-specific format information.
		/// </summary>
		/// <param name="format">A format specification.</param>
		/// <param name="provider">
		/// An System.IFormatProvider that supplies culture-specific formatting information.
		/// </param>
		/// <returns>
		/// The string representation of the value of this instance as specified by format and provider.
		/// </returns>
		public string ToString(string format, IFormatProvider provider)
		{
			return this.ToString(format, provider);
		}
		
		public string ToString(string format, IFormatProvider provider)
		{
			
			return m_value.ToString(format, provider);
			
		}
		
		/// <summary>
		/// Converts the string representation of a number to its 24-bit unsigned integer equivalent.
		/// </summary>
		/// <param name="s">A string containing a number to convert.</param>
		/// <returns>
		/// A 24-bit unsigned integer equivalent to the number contained in s.
		/// </returns>
		/// <exception cref="ArgumentNullException">s is null.</exception>
		/// <exception cref="OverflowAction">
		/// s represents a number less than UInt24.MinValue or greater than UInt24.MaxValue.
		/// </exception>
		/// <exception cref="FormatException">s is not in the correct format.</exception>
		public static UInt24 Parse(string s)
		{
			
			return ((UInt24) (UInt32.Parse(s)));
			
		}
		
		/// <summary>
		/// Converts the string representation of a number in a specified style to its 24-bit unsigned integer equivalent.
		/// </summary>
		/// <param name="s">A string containing a number to convert.</param>
		/// <param name="style">
		/// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
		/// A typical value to specify is System.Globalization.NumberStyles.Integer.
		/// </param>
		/// <returns>
		/// A 24-bit unsigned integer equivalent to the number contained in s.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
		/// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
		/// </exception>
		/// <exception cref="ArgumentNullException">s is null.</exception>
		/// <exception cref="OverflowAction">
		/// s represents a number less than UInt24.MinValue or greater than UInt24.MaxValue.
		/// </exception>
		/// <exception cref="FormatException">s is not in a format compliant with style.</exception>
		public static UInt24 Parse(string s, NumberStyles style)
		{
			
			return ((UInt24) (UInt32.Parse(s, style)));
			
		}
		
		/// <summary>
		/// Converts the string representation of a number in a specified culture-specific format to its 24-bit
		/// unsigned integer equivalent.
		/// </summary>
		/// <param name="s">A string containing a number to convert.</param>
		/// <param name="provider">
		/// An System.IFormatProvider that supplies culture-specific formatting information about s.
		/// </param>
		/// <returns>
		/// A 24-bit unsigned integer equivalent to the number contained in s.
		/// </returns>
		/// <exception cref="ArgumentNullException">s is null.</exception>
		/// <exception cref="OverflowAction">
		/// s represents a number less than UInt24.MinValue or greater than UInt24.MaxValue.
		/// </exception>
		/// <exception cref="FormatException">s is not in the correct format.</exception>
		public static UInt24 Parse(string s, IFormatProvider provider)
		{
			
			return ((UInt24) (UInt32.Parse(s, provider)));
			
		}
		
		/// <summary>
		/// Converts the string representation of a number in a specified style and culture-specific format to its 24-bit
		/// unsigned integer equivalent.
		/// </summary>
		/// <param name="s">A string containing a number to convert.</param>
		/// <param name="style">
		/// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
		/// A typical value to specify is System.Globalization.NumberStyles.Integer.
		/// </param>
		/// <param name="provider">
		/// An System.IFormatProvider that supplies culture-specific formatting information about s.
		/// </param>
		/// <returns>
		/// A 24-bit unsigned integer equivalent to the number contained in s.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
		/// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
		/// </exception>
		/// <exception cref="ArgumentNullException">s is null.</exception>
		/// <exception cref="OverflowAction">
		/// s represents a number less than UInt24.MinValue or greater than UInt24.MaxValue.
		/// </exception>
		/// <exception cref="FormatException">s is not in a format compliant with style.</exception>
		public static UInt24 Parse(string s, NumberStyles style, IFormatProvider provider)
		{
			
			return ((UInt24) (UInt32.Parse(s, style, provider)));
			
		}
		
		/// <summary>
		/// Converts the string representation of a number to its 24-bit unsigned integer equivalent. A return value
		/// indicates whether the conversion succeeded or failed.
		/// </summary>
		/// <param name="s">A string containing a number to convert.</param>
		/// <param name="result">
		/// When this method returns, contains the 24-bit unsigned integer value equivalent to the number contained in s,
		/// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
		/// is not of the correct format, or represents a number less than UInt24.MinValue or greater than UInt24.MaxValue.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>true if s was converted successfully; otherwise, false.</returns>
		public static bool TryParse(string s, ref UInt24 result)
		{
			
			UInt32 parseResult;
			bool parseResponse;
			
			parseResponse = UInt32.TryParse(s, ref parseResult);
			
			try
			{
				result = (UInt24) parseResult;
			}
			catch
			{
				result = (UInt24) (0);
				parseResponse = false;
			}
			
			return parseResponse;
			
		}
		
		/// <summary>
		/// Converts the string representation of a number in a specified style and culture-specific format to its
		/// 24-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.
		/// </summary>
		/// <param name="s">A string containing a number to convert.</param>
		/// <param name="style">
		/// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
		/// A typical value to specify is System.Globalization.NumberStyles.Integer.
		/// </param>
		/// <param name="result">
		/// When this method returns, contains the 24-bit unsigned integer value equivalent to the number contained in s,
		/// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
		/// is not in a format compliant with style, or represents a number less than UInt24.MinValue or greater than
		/// UInt24.MaxValue. This parameter is passed uninitialized.
		/// </param>
		/// <param name="provider">
		/// An System.IFormatProvider objectthat supplies culture-specific formatting information about s.
		/// </param>
		/// <returns>true if s was converted successfully; otherwise, false.</returns>
		/// <exception cref="ArgumentException">
		/// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
		/// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
		/// </exception>
		public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, ref UInt24 result)
		{
			
			UInt32 parseResult;
			bool parseResponse;
			
			parseResponse = UInt32.TryParse(s, style, provider, ref parseResult);
			
			try
			{
				result = (UInt24) parseResult;
			}
			catch
			{
				result = (UInt24) (0);
				parseResponse = false;
			}
			
			return parseResponse;
			
		}
		
		/// <summary>
		/// Returns the System.TypeCode for value type System.UInt32 (there is no defined type code for an UInt24).
		/// </summary>
		/// <returns>The enumerated constant, System.TypeCode.UInt32.</returns>
		/// <remarks>
		/// There is no defined UInt24 type code and since an UInt24 will easily fit inside an UInt32, the
		/// UInt32 type code is returned.
		/// </remarks>
		public TypeCode GetTypeCode()
		{
			
			// There is no UInt24 type code, and an UInt24 will fit inside an UInt32 - so we return an UInt32 type code
			return TypeCode.UInt32;
			
		}
		
		#region " Private IConvertible Implementation "
		
		// These are are private on the native integer implementations, so we just make them private as well...
		
		public bool ToBoolean(IFormatProvider provider)
		{
			
			return Convert.ToBoolean(m_value, provider);
			
		}
		
		public char ToChar(IFormatProvider provider)
		{
			
			return Convert.ToChar(m_value, provider);
			
		}
		
		public SByte ToSByte(IFormatProvider provider)
		{
			
			return Convert.ToSByte(m_value, provider);
			
		}
		
		public byte ToByte(IFormatProvider provider)
		{
			
			return Convert.ToByte(m_value, provider);
			
		}
		
		public short ToInt16(IFormatProvider provider)
		{
			
			return Convert.ToInt16(m_value, provider);
			
		}
		
		public UInt16 ToUInt16(IFormatProvider provider)
		{
			
			return Convert.ToUInt16(m_value, provider);
			
		}
		
		public int ToInt32(IFormatProvider provider)
		{
			
			return Convert.ToInt32(m_value, provider);
			
		}
		
		public UInt32 ToUInt32(IFormatProvider provider)
		{
			
			return m_value;
			
		}
		
		public long ToInt64(IFormatProvider provider)
		{
			
			return Convert.ToInt64(m_value, provider);
			
		}
		
		public UInt64 ToUInt64(IFormatProvider provider)
		{
			
			return Convert.ToUInt64(m_value, provider);
			
		}
		
		public float ToSingle(IFormatProvider provider)
		{
			
			return Convert.ToSingle(m_value, provider);
			
		}
		
		public double ToDouble(IFormatProvider provider)
		{
			
			return Convert.ToDouble(m_value, provider);
			
		}
		
		public decimal ToDecimal(IFormatProvider provider)
		{
			
			return Convert.ToDecimal(m_value, provider);
			
		}
		
		public System.DateTime ToDateTime(IFormatProvider provider)
		{
			
			return Convert.ToDateTime(m_value, provider);
			
		}
		
		public object ToType(Type type, IFormatProvider provider)
		{
			
			return Convert.ChangeType(m_value, type, provider);
			
		}
		
		#endregion
		
		#endregion
		
	}
	
}
