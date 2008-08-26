using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Interop.BitwiseCast.vb - Bitwise Integer Data Type Conversion Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/05/2006 - J. Ritchie Carroll
//       Original version of source code generated
//
//*******************************************************************************************************

namespace TVA
{
	namespace Interop
	{
		
		/// <summary>Defines specialized bitwise integer data type conversion functions</summary>
		public sealed class BitwiseCast
		{
			
			
			private BitwiseCast()
			{
				
				// This class contains only global functions and is not meant to be instantiated
				
			}
			
			/// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
			/// <remarks>
			/// <para>This function is useful because Convert.ToInt16 will throw an OverflowException for values greater than Int16.MaxValue.</para>
			/// <para>For example, this function correctly converts unsigned 16-bit integer 65535 (i.e., UInt16.MaxValue) to signed 16-bit integer -1.</para>
			/// </remarks>
			public static short ToInt16(UInt16 unsignedInt)
			{
				return BitConverter.ToInt16(BitConverter.GetBytes(unsignedInt), 0);
			}
			
			/// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
			/// <remarks>
			/// <para>This function is useful because CType(n, Int24) will throw an OverflowException for values greater than Int24.MaxValue.</para>
			/// <para>For example, this function correctly converts unsigned 24-bit integer 16777215 (i.e., UInt24.MaxValue) to signed 24-bit integer -1.</para>
			/// </remarks>
			public static Int24 ToInt24(UInt24 unsignedInt)
			{
				return Int24.GetValue(UInt24.GetBytes(unsignedInt), 0);
			}
			
			/// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
			/// <remarks>
			/// <para>This function is useful because Convert.ToInt32 will throw an OverflowException for values greater than Int32.MaxValue.</para>
			/// <para>For example, this function correctly converts unsigned 32-bit integer 4294967295 (i.e., UInt16.MaxValue) to signed 32-bit integer -1.</para>
			/// </remarks>
			public static int ToInt32(UInt32 unsignedInt)
			{
				return BitConverter.ToInt32(BitConverter.GetBytes(unsignedInt), 0);
			}
			
			/// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
			/// <remarks>
			/// <para>This function is useful because Convert.ToInt64 will throw an OverflowException for values greater than Int64.MaxValue.</para>
			/// <para>For example, this function correctly converts unsigned 64-bit integer 18446744073709551615 (i.e., UInt64.MaxValue) to signed 64-bit integer -1.</para>
			/// </remarks>
			public static long ToInt64(UInt64 unsignedInt)
			{
				return BitConverter.ToInt64(BitConverter.GetBytes(unsignedInt), 0);
			}
			
			/// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
			/// <remarks>
			/// <para>This function is useful because Convert.ToUInt16 will throw an OverflowException for values less than zero.</para>
			/// <para>For example, this function correctly converts signed 16-bit integer -32768 (i.e., Int16.MinValue) to unsigned 16-bit integer 32768.</para>
			/// </remarks>
			public static UInt16 ToUInt16(short signedInt)
			{
				return BitConverter.ToUInt16(BitConverter.GetBytes(signedInt), 0);
			}
			
			/// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
			/// <remarks>
			/// <para>This function is useful because CType(n, UInt24) will throw an OverflowException for values less than zero.</para>
			/// <para>For example, this function correctly converts signed 24-bit integer -8388608 (i.e., Int24.MinValue) to unsigned 24-bit integer 8388608.</para>
			/// </remarks>
			public static UInt24 ToUInt24(Int24 signedInt)
			{
				return UInt24.GetValue(Int24.GetBytes(signedInt), 0);
			}
			
			/// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
			/// <remarks>
			/// <para>This function is useful because Convert.ToUInt32 will throw an OverflowException for values less than zero.</para>
			/// <para>For example, this function correctly converts signed 32-bit integer -2147483648 (i.e., Int32.MinValue) to unsigned 32-bit integer 2147483648.</para>
			/// </remarks>
			public static UInt32 ToUInt32(int signedInt)
			{
				return BitConverter.ToUInt32(BitConverter.GetBytes(signedInt), 0);
			}
			
			/// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
			/// <remarks>
			/// <para>This function is useful because Convert.ToUInt64 will throw an OverflowException for values less than zero.</para>
			/// <para>For example, this function correctly converts signed 64-bit integer -9223372036854775808 (i.e., Int64.MinValue) to unsigned 64-bit integer 9223372036854775808.</para>
			/// </remarks>
			public static UInt64 ToUInt64(long signedInt)
			{
				return BitConverter.ToUInt64(BitConverter.GetBytes(signedInt), 0);
			}
			
		}
		
	}
	
}
