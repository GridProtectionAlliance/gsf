//*******************************************************************************************************
//  NumericExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/18/2008 - James R Carroll
//       Generated original version of source code.
//  02/16/2009 - Josh Patterson
//      Edited Code Comments
//
//*******************************************************************************************************

using System;

namespace PCS
{
    /// <summary>Defines extension functions related to numbers.</summary>
    public static class NumericExtensions
    {
        /// <summary>Ensures parameter passed to function is not zero. Returns -1
        /// if <paramref name="source">source</paramref> is zero.</summary>
        /// <param name="source">Value to test for zero.</param>
        /// <typeparam name="T">Return type used for immediate expression</typeparam>
        /// <returns>A non-zero value.</returns>
        public static T NotZero<T>(this T source) where T : struct, IEquatable<T>
        {
            return NotZero<T>(source, (T)Convert.ChangeType(-1, typeof(T)));
        }

        /// <summary>Ensures parameter passed to function is not zero.</summary>
        /// <param name="source">Value to test for zero.</param>
        /// <param name="nonZeroReturnValue">Value to return if <paramref name="source">source</paramref> is
        /// zero.</param>
        /// <typeparam name="T">Return type used for immediate expression</typeparam>
        /// <returns>A non-zero value.</returns>
        /// <remarks>To optimize performance, this function does not validate that the notZeroReturnValue is not
        /// zero.</remarks>
        public static T NotZero<T>(this T source, T nonZeroReturnValue) where T : struct, IEquatable<T>
        {
            return (((IEquatable<T>)source).Equals(default(T)) ? nonZeroReturnValue : source);
        }

        /// <summary>Ensures test parameter passed to function is not equal to the specified value.</summary>
        /// <param name="source">Value to test.</param>
        /// <param name="notEqualToValue">Value that represents the undesired value (e.g., zero).</param>
        /// <param name="alternateValue">Value to return if <paramref name="source">source</paramref> is equal
        /// to the undesired value.</param>
        /// <typeparam name="T">Structure or class that implements IEquatable(Of T) (e.g., Double, Single,
        /// Integer, etc.).</typeparam>
        /// <returns>A value not equal to notEqualToValue.</returns>
        /// <remarks>To optimize performance, this function does not validate that the notEqualToValue is not equal
        /// to the alternateValue.</remarks>
        public static T NotEqualTo<T>(this T source, T notEqualToValue, T alternateValue) where T : IEquatable<T>
        {
            return (((IEquatable<T>)source).Equals(notEqualToValue) ? alternateValue : source);
        }

        /// <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
        /// <param name="source">Value to test.</param>
        /// <param name="notLessThanValue">Value that represents the lower limit for the source. This value
        /// is returned if source is less than notLessThanValue.</param>
        /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
        /// Integer, etc.).</typeparam>
        /// <returns>A value not less than notLessThanValue.</returns>
        /// <remarks>If source is less than notLessThanValue, then notLessThanValue is returned.</remarks>
        public static T NotLessThan<T>(this T source, T notLessThanValue) where T : IComparable<T>
        {
            return (((IComparable<T>)source).CompareTo(notLessThanValue) < 0 ? notLessThanValue : source);
        }

        /// <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
        /// <param name="source">Value to test.</param>
        /// <param name="notLessThanValue">Value that represents the lower limit for the source.</param>
        /// <param name="alternateValue">Value to return if <paramref name="source">source</paramref> is
        /// less than <paramref name="notLessThanValue">notLessThanValue</paramref>.</param>
        /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
        /// Integer, etc.).</typeparam>
        /// <returns>A value not less than notLessThanValue.</returns>
        /// <remarks>To optimize performance, this function does not validate that the notLessThanValue is not
        /// less than the alternateValue.</remarks>
        public static T NotLessThan<T>(this T source, T notLessThanValue, T alternateValue) where T : IComparable<T>
        {
            return (((IComparable<T>)source).CompareTo(notLessThanValue) < 0 ? alternateValue : source);
        }

        /// <summary>Ensures test parameter passed to function is not less than or equal to the specified value.</summary>
        /// <param name="source">Value to test.</param>
        /// <param name="notLessThanOrEqualToValue">Value that represents the lower limit for the source.</param>
        /// <param name="alternateValue">Value to return if <paramref name="source">source</paramref> is
        /// less than or equal to <paramref name="notLessThanOrEqualToValue">notLessThanOrEqualToValue</paramref>.</param>
        /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
        /// Integer, etc.).</typeparam>
        /// <returns>A value not less than or equal to notLessThanOrEqualToValue.</returns>
        /// <remarks>To optimize performance, this function does not validate that the notLessThanOrEqualToValue is
        /// not less than or equal to the alternateValue.</remarks>
        public static T NotLessThanOrEqualTo<T>(this T source, T notLessThanOrEqualToValue, T alternateValue) where T : IComparable<T>
        {
            return (((IComparable<T>)source).CompareTo(notLessThanOrEqualToValue) <= 0 ? alternateValue : source);
        }

        /// <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
        /// <param name="source">Value to test.</param>
        /// <param name="notGreaterThanValue">Value that represents the upper limit for the source. This
        /// value is returned if source is greater than notGreaterThanValue.</param>
        /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
        /// Integer, etc.).</typeparam>
        /// <returns>A value not greater than notGreaterThanValue.</returns>
        /// <remarks>If source is greater than notGreaterThanValue, then notGreaterThanValue is returned.</remarks>
        public static T NotGreaterThan<T>(this T source, T notGreaterThanValue) where T : IComparable<T>
        {
            return (((IComparable<T>)source).CompareTo(notGreaterThanValue) > 0 ? notGreaterThanValue : source);
        }

        /// <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
        /// <param name="source">Value to test.</param>
        /// <param name="notGreaterThanValue">Value that represents the upper limit for the source.</param>
        /// <param name="alternateValue">Value to return if <paramref name="source">source</paramref> is
        /// greater than <paramref name="notGreaterThanValue">notGreaterThanValue</paramref>.</param>
        /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
        /// Integer, etc.).</typeparam>
        /// <returns>A value not greater than notGreaterThanValue.</returns>
        /// <remarks>To optimize performance, this function does not validate that the notGreaterThanValue is
        /// not greater than the alternateValue</remarks>
        public static T NotGreaterThan<T>(this T source, T notGreaterThanValue, T alternateValue) where T : IComparable<T>
        {
            return (((IComparable<T>)source).CompareTo(notGreaterThanValue) > 0 ? alternateValue : source);
        }

        /// <summary>Ensures test parameter passed to function is not greater than or equal to the specified value.</summary>
        /// <param name="source">Value to test.</param>
        /// <param name="notGreaterThanOrEqualToValue">Value that represents the upper limit for the source.</param>
        /// <param name="alternateValue">Value to return if <paramref name="source">source</paramref> is
        /// greater than or equal to <paramref name="notGreaterThanOrEqualToValue">notGreaterThanOrEqualToValue</paramref>.</param>
        /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
        /// Integer, etc.).</typeparam>
        /// <returns>A value not greater than or equal to notGreaterThanOrEqualToValue.</returns>
        /// <remarks>To optimize performance, this function does not validate that the notGreaterThanOrEqualToValue
        /// is not greater than or equal to the alternateValue.</remarks>
        public static T NotGreaterThanOrEqualTo<T>(this T source, T notGreaterThanOrEqualToValue, T alternateValue) where T : IComparable<T>
        {
            return (((IComparable<T>)source).CompareTo(notGreaterThanOrEqualToValue) >= 0 ? alternateValue : source);
        }
    }
}
