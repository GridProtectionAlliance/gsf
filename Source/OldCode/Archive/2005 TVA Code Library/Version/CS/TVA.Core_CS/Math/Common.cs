using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Common;
using TVA.Interop;

//*******************************************************************************************************
//  TVA.Math.Common.vb - Math Functions
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Math).
//  01/04/2006 - J. Ritchie Carroll
//       Added crytographically strong random number generation functions.
//  01/24/2006 - J. Ritchie Carroll
//       Added curve fit function (courtesy of Brian Fox from DatAWare client code).
//  11/08/2006 - J. Ritchie Carroll
//       Added standard devitaion and average functions.
//  12/07/2006 - J. Ritchie Carroll
//       Added strongly-typed generic Not "comparator" functions (e.g., NotEqualTo).
//  08/23/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
	namespace Math
	{
		
		/// <summary>Defines common math functions.</summary>
		public sealed class Common
		{
			
			
			private static System.Security.Cryptography.RNGCryptoServiceProvider m_randomNumberGenerator = new System.Security.Cryptography.RNGCryptoServiceProvider();
			
			private Common()
			{
				
				// This class contains only global functions and is not meant to be instantiated.
				
			}
			
			/// <summary>Ensures parameter passed to function is not zero. Returns -1
			/// if <paramref name="testValue">testValue</paramref> is zero.</summary>
			/// <param name="testValue">Value to test for zero.</param>
			/// <returns>A non-zero value.</returns>
			public static double NotZero(double testValue)
			{
				
				return NotZero(testValue, - 1.0);
				
			}
			
			/// <summary>Ensures parameter passed to function is not zero.</summary>
			/// <param name="testValue">Value to test for zero.</param>
			/// <param name="nonZeroReturnValue">Value to return if <paramref name="testValue">testValue</paramref> is
			/// zero.</param>
			/// <returns>A non-zero value.</returns>
			/// <remarks>To optimize performance, this function does not validate that the notZeroReturnValue is not
			/// zero.</remarks>
			public static double NotZero(double testValue, double nonZeroReturnValue)
			{
				
				return TVA.Common.IIf(testValue == 0.0, nonZeroReturnValue, testValue);
				
			}
			
			/// <summary>Ensures test parameter passed to function is not equal to the specified value.</summary>
			/// <param name="testValue">Value to test.</param>
			/// <param name="notEqualToValue">Value that represents the undesired value (e.g., zero).</param>
			/// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is equal
			/// to the undesired value.</param>
			/// <typeparam name="T">Structure or class that implements IEquatable(Of T) (e.g., Double, Single,
			/// Integer, etc.).</typeparam>
			/// <returns>A value not equal to notEqualToValue.</returns>
			/// <remarks>To optimize performance, this function does not validate that the notEqualToValue is not equal
			/// to the alternateValue.</remarks>
			public static T NotEqualTo<T>(T testValue, T notEqualToValue, T alternateValue) where T : IEquatable<T>
			{
				
				return Interaction.IIf<T>(((IEquatable<T>) testValue).Equals(notEqualToValue), alternateValue, testValue);
				
			}
			
			/// <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
			/// <param name="testValue">Value to test.</param>
			/// <param name="notLessThanValue">Value that represents the lower limit for the testValue. This value
			/// is returned if testValue is less than notLessThanValue.</param>
			/// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
			/// Integer, etc.).</typeparam>
			/// <returns>A value not less than notLessThanValue.</returns>
			/// <remarks>If testValue is less than notLessThanValue, then notLessThanValue is returned.</remarks>
			public static T NotLessThan<T>(T testValue, T notLessThanValue) where T : IComparable<T>
			{
				
				return Interaction.IIf<T>(((IComparable<T>) testValue).CompareTo(notLessThanValue) < 0, notLessThanValue, testValue);
				
			}
			
			/// <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
			/// <param name="testValue">Value to test.</param>
			/// <param name="notLessThanValue">Value that represents the lower limit for the testValue.</param>
			/// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
			/// less than <paramref name="notLessThanValue">notLessThanValue</paramref>.</param>
			/// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
			/// Integer, etc.).</typeparam>
			/// <returns>A value not less than notLessThanValue.</returns>
			/// <remarks>To optimize performance, this function does not validate that the notLessThanValue is not
			/// less than the alternateValue.</remarks>
			public static T NotLessThan<T>(T testValue, T notLessThanValue, T alternateValue) where T : IComparable<T>
			{
				
				return Interaction.IIf<T>(((IComparable<T>) testValue).CompareTo(notLessThanValue) < 0, alternateValue, testValue);
				
			}
			
			/// <summary>Ensures test parameter passed to function is not less than or equal to the specified value.</summary>
			/// <param name="testValue">Value to test.</param>
			/// <param name="notLessThanOrEqualToValue">Value that represents the lower limit for the testValue.</param>
			/// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
			/// less than or equal to <paramref name="notLessThanOrEqualToValue">notLessThanOrEqualToValue</paramref>.</param>
			/// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
			/// Integer, etc.).</typeparam>
			/// <returns>A value not less than or equal to notLessThanOrEqualToValue.</returns>
			/// <remarks>To optimize performance, this function does not validate that the notLessThanOrEqualToValue is
			/// not less than or equal to the alternateValue.</remarks>
			public static T NotLessThanOrEqualTo<T>(T testValue, T notLessThanOrEqualToValue, T alternateValue) where T : IComparable<T>
			{
				
				return Interaction.IIf<T>(((IComparable<T>) testValue).CompareTo(notLessThanOrEqualToValue) <= 0, alternateValue, testValue);
				
			}
			
			/// <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
			/// <param name="testValue">Value to test.</param>
			/// <param name="notGreaterThanValue">Value that represents the upper limit for the testValue. This
			/// value is returned if testValue is greater than notGreaterThanValue.</param>
			/// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
			/// Integer, etc.).</typeparam>
			/// <returns>A value not greater than notGreaterThanValue.</returns>
			/// <remarks>If testValue is greater than notGreaterThanValue, then notGreaterThanValue is returned.</remarks>
			public static T NotGreaterThan<T>(T testValue, T notGreaterThanValue) where T : IComparable<T>
			{
				
				return Interaction.IIf<T>(((IComparable<T>) testValue).CompareTo(notGreaterThanValue) > 0, notGreaterThanValue, testValue);
				
			}
			
			/// <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
			/// <param name="testValue">Value to test.</param>
			/// <param name="notGreaterThanValue">Value that represents the upper limit for the testValue.</param>
			/// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
			/// greater than <paramref name="notGreaterThanValue">notGreaterThanValue</paramref>.</param>
			/// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
			/// Integer, etc.).</typeparam>
			/// <returns>A value not greater than notGreaterThanValue.</returns>
			/// <remarks>To optimize performance, this function does not validate that the notGreaterThanValue is
			/// not greater than the alternateValue</remarks>
			public static T NotGreaterThan<T>(T testValue, T notGreaterThanValue, T alternateValue) where T : IComparable<T>
			{
				
				return Interaction.IIf<T>(((IComparable<T>) testValue).CompareTo(notGreaterThanValue) > 0, alternateValue, testValue);
				
			}
			
			/// <summary>Ensures test parameter passed to function is not greater than or equal to the specified value.</summary>
			/// <param name="testValue">Value to test.</param>
			/// <param name="notGreaterThanOrEqualToValue">Value that represents the upper limit for the testValue.</param>
			/// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
			/// greater than or equal to <paramref name="notGreaterThanOrEqualToValue">notGreaterThanOrEqualToValue</paramref>.</param>
			/// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
			/// Integer, etc.).</typeparam>
			/// <returns>A value not greater than or equal to notGreaterThanOrEqualToValue.</returns>
			/// <remarks>To optimize performance, this function does not validate that the notGreaterThanOrEqualToValue
			/// is not greater than or equal to the alternateValue.</remarks>
			public static T NotGreaterThanOrEqualTo<T>(T testValue, T notGreaterThanOrEqualToValue, T alternateValue) where T : IComparable<T>
			{
				
				return Interaction.IIf<T>(((IComparable<T>) testValue).CompareTo(notGreaterThanOrEqualToValue) >= 0, alternateValue, testValue);
				
			}
			
			/// <summary>Calculates byte length (8-bit) XOR-based check-sum on specified portion of a buffer.</summary>
			/// <param name="data">Data buffer to perform XOR check-sum on.</param>
			/// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
			/// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
			/// perform XOR check-sum over.</param>
			/// <returns>Byte length XOR check-sum.</returns>
			public static byte Xor8BitCheckSum(byte[] data, int startIndex, int length)
			{
				
				byte sum;
				
				for (int x = 0; x <= length - 1; x++)
				{
					sum = sum ^ data[startIndex + x];
				}
				
				return sum;
				
			}
			
			/// <summary>Calculates word length (16-bit) XOR-based check-sum on specified portion of a buffer.</summary>
			/// <param name="data">Data buffer to perform XOR check-sum on.</param>
			/// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
			/// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
			/// perform XOR check-sum overs</param>
			/// <returns>Word length XOR check-sum.</returns>
			[CLSCompliant(false)]public static UInt16 Xor16BitCheckSum(byte[] data, int startIndex, int length)
			{
				
				UInt16 sum;
				
				for (int x = 0; x <= length - 1; x += 2)
				{
					sum = sum ^ BitConverter.ToUInt16(data, startIndex + x);
				}
				
				return sum;
				
			}
			
			/// <summary>Calculates double-word length (32-bit) XOR-based check-sum on specified portion of a buffer.</summary>
			/// <param name="data">Data buffer to perform XOR check-sum on.</param>
			/// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
			/// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
			/// perform XOR check-sum over.</param>
			/// <returns>Double-word length XOR check-sum.</returns>
			[CLSCompliant(false)]public static UInt32 Xor32BitCheckSum(byte[] data, int startIndex, int length)
			{
				
				UInt32 sum;
				
				for (int x = 0; x <= length - 1; x += 4)
				{
					sum = sum ^ BitConverter.ToUInt32(data, startIndex + x);
				}
				
				return sum;
				
			}
			
			/// <summary>Calculates quad-word length (64-bit) XOR-based check-sum on specified portion of a buffer.</summary>
			/// <param name="data">Data buffer to perform XOR check-sum on.</param>
			/// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
			/// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
			/// perform XOR check-sum over.</param>
			/// <returns>Quad-word length XOR check-sum.</returns>
			[CLSCompliant(false)]public static UInt64 Xor64BitCheckSum(byte[] data, int startIndex, int length)
			{
				
				UInt64 sum;
				
				for (int x = 0; x <= length - 1; x += 8)
				{
					sum = sum ^ BitConverter.ToUInt64(data, startIndex + x);
				}
				
				return sum;
				
			}
			
			/// <summary>Generates a cryptographically strong floating-point random number between zero and one.</summary>
			public static double RandomNumber
			{
				get
				{
					return BitwiseCast.ToUInt32(RandomInt32) / UInt32.MaxValue;
				}
			}
			
			/// <summary>Generates a cryptographically strong random decimal between zero and one.</summary>
			public static decimal RandomDecimal
			{
				get
				{
					return Convert.ToDecimal(BitwiseCast.ToUInt64(RandomInt64)) / Convert.ToDecimal(UInt64.MaxValue);
				}
			}
			
			/// <summary>Generates a cryptographically strong random integer between specified values.</summary>
			public static double RandomBetween(double startNumber, double stopNumber)
			{
				if (stopNumber < startNumber)
				{
					throw (new ArgumentException("stopNumber must be greater than startNumber"));
				}
				return RandomNumber * (stopNumber - startNumber) + startNumber;
			}
			
			/// <summary>Generates a cryptographically strong random boolean (i.e., a coin toss).</summary>
			public static bool RandomBoolean
			{
				get
				{
					byte[] value = TVA.Common.CreateArray<byte>(1);
					
					m_randomNumberGenerator.GetBytes(value);
					
					return TVA.Common.IIf(value(0) % 2 == 0, true, false);
				}
			}
			
			/// <summary>Generates a cryptographically strong 8-bit random integer.</summary>
			public static byte RandomByte
			{
				get
				{
					byte[] value = TVA.Common.CreateArray<byte>(1);
					
					m_randomNumberGenerator.GetBytes(value);
					
					return value(0);
				}
			}
			
			/// <summary>Generates a cryptographically strong 8-bit random integer between specified values.</summary>
			public static byte RandomByteBetween(byte startNumber, byte stopNumber)
			{
				if (stopNumber < startNumber)
				{
					throw (new ArgumentException("stopNumber must be greater than startNumber"));
				}
				return Convert.ToByte(RandomNumber * (stopNumber - startNumber) + startNumber);
			}
			
			/// <summary>Generates a cryptographically strong 16-bit random integer.</summary>
			public static short RandomInt16
			{
				get
				{
					byte[] value = TVA.Common.CreateArray<byte>(2);
					
					m_randomNumberGenerator.GetBytes(value);
					
					return BitConverter.ToInt16(value, 0);
				}
			}
			
			/// <summary>Generates a cryptographically strong 16-bit random integer between specified values.</summary>
			public static short RandomInt16Between(short startNumber, short stopNumber)
			{
				if (stopNumber < startNumber)
				{
					throw (new ArgumentException("stopNumber must be greater than startNumber"));
				}
				return Convert.ToInt16(RandomNumber * (stopNumber - startNumber) + startNumber);
			}
			
			/// <summary>Generates a cryptographically strong 32-bit random integer.</summary>
			public static int RandomInt32
			{
				get
				{
					byte[] value = TVA.Common.CreateArray<byte>(4);
					
					m_randomNumberGenerator.GetBytes(value);
					
					return BitConverter.ToInt32(value, 0);
				}
			}
			
			/// <summary>Generates a cryptographically strong 32-bit random integer between specified values.</summary>
			public static int RandomInt32Between(int startNumber, int stopNumber)
			{
				if (stopNumber < startNumber)
				{
					throw (new ArgumentException("stopNumber must be greater than startNumber"));
				}
				return Convert.ToInt32(RandomNumber * (stopNumber - startNumber) + startNumber);
			}
			
			/// <summary>Generates a cryptographically strong 64-bit random integer.</summary>
			public static long RandomInt64
			{
				get
				{
					byte[] value = TVA.Common.CreateArray<byte>(8);
					
					m_randomNumberGenerator.GetBytes(value);
					
					return BitConverter.ToInt64(value, 0);
				}
			}
			
			/// <summary>Generates a cryptographically strong 64-bit random integer between specified values.</summary>
			public static long RandomInt64Between(long startNumber, long stopNumber)
			{
				if (stopNumber < startNumber)
				{
					throw (new ArgumentException("stopNumber must be greater than startNumber"));
				}
				return Convert.ToInt64(RandomNumber * (stopNumber - startNumber) + startNumber);
			}
			
			/// <summary>Linear regression algorithm.</summary>
			public static double[] CurveFit(int polynomialOrder, int pointCount, IList<double> xValues, IList<double> yValues)
			{
				
				double[] coeffs = new double[8];
				double[] sum = new double[22];
				double[] v = new double[12];
				double[,] b = new double[12, 13];
				double p;
				double divB;
				double fMultB;
				double sigma;
				int ls;
				int lb;
				int lv;
				int i1;
				int i;
				int j;
				int k;
				int l;
				
				if (!(pointCount >= polynomialOrder + 1))
				{
					throw (new ArgumentException("Point count must be greater than requested polynomial order"));
				}
				if (!(polynomialOrder >= 1) && (polynomialOrder <= 7))
				{
					throw (new ArgumentOutOfRangeException("polynomialOrder", "Polynomial order must be between 1 and 7"));
				}
				
				ls = polynomialOrder * 2;
				lb = polynomialOrder + 1;
				lv = polynomialOrder;
				sum[0] = pointCount;
				
				for (i = 0; i <= pointCount - 1; i++)
				{
					p = 1.0;
					v[0] = v[0] + yValues[i];
					for (j = 1; j <= lv; j++)
					{
						p = xValues[i] * p;
						sum[j] = sum[j] + p;
						v[j] = v[j] + yValues[i] * p;
					}
					for (j = lb; j <= ls; j++)
					{
						p = xValues[i] * p;
						sum[j] = sum[j] + p;
					}
				}
				
				for (i = 0; i <= lv; i++)
				{
					for (k = 0; k <= lv; k++)
					{
						b[k, i] = sum[k + i];
					}
				}
				
				for (k = 0; k <= lv; k++)
				{
					b[k, lb] = v[k];
				}
				
				for (l = 0; l <= lv; l++)
				{
					divB = b[0, 0];
					for (j = l; j <= lb; j++)
					{
						if (divB == 0)
						{
							divB = 1;
						}
						b[l, j] = b[l, j] / divB;
					}
					
					i1 = l + 1;
					
					if (i1 - lb < 0)
					{
						for (i = i1; i <= lv; i++)
						{
							fMultB = b[i, l];
							for (j = l; j <= lb; j++)
							{
								b[i, j] = b[i, j] - b[l, j] * fMultB;
							}
						}
					}
					else
					{
						break;
					}
				}
				
				coeffs[lv] = b[lv, lb];
				i = lv;
				
				do
				{
					sigma = 0;
					for (j = i; j <= lv; j++)
					{
						sigma = sigma + b[i - 1, j] * coeffs[j];
					}
					i--;
					coeffs[i] = b[i, lb] - sigma;
				} while (i - 1 > 0);
				
				//    For i = 1 To 7
				//        Debug.Print "Coeffs(" & i & ") = " & Coeffs(i)
				//    Next i
				
				//For i = 1 To 60
				//    '        CalcY(i).TTag = xValues(1) + ((i - 1) / (xValues(pointCount) - xValues(1)))
				
				//    CalcY(i).TTag = ((i - 1) / 59) * xValues(pointCount) - xValues(1)
				//    CalcY(i).Value = Coeffs(1)
				
				//    For j = 1 To polynomialOrder
				//        CalcY(i).Value = CalcY(i).Value + Coeffs(j + 1) * CalcY(i).TTag ^ j
				//    Next
				//Next
				
				//    SSERROR = 0
				//    For i = 1 To pointCount
				//        SSERROR = SSERROR + (yValues(i) - CalcY(i).Value) * (yValues(i) - CalcY(i).Value)
				//    Next i
				//    SSERROR = SSERROR / (pointCount - polynomialOrder)
				//    sError = SSERROR
				
				// Return slopes...
				return coeffs;
				
			}
			
			public static double StandardDeviation(IList<double> dataSample)
			{
				
				if (dataSample.Count == 0)
				{
					return 0.0;
				}
				
				double sampleAverage = Average(dataSample);
				double totalVariance;
				double dataPointDeviation;
				
				for (int x = 0; x <= dataSample.Count - 1; x++)
				{
					dataPointDeviation = dataSample[x] - sampleAverage;
					totalVariance += dataPointDeviation * dataPointDeviation;
				}
				
				return System.Math.Sqrt(totalVariance / dataSample.Count);
				
			}
			
			public static double Average(IList<double> dataSample)
			{
				
				if (dataSample.Count == 0)
				{
					return 0.0;
				}
				
				double sampleTotal;
				
				for (int x = 0; x <= dataSample.Count - 1; x++)
				{
					sampleTotal += dataSample[x];
				}
				
				return sampleTotal / dataSample.Count;
				
			}
			
		}
		
	}
	
}
