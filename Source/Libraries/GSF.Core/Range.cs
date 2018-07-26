//******************************************************************************************************
//  Range.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/30/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF
{
    /// <summary>
    /// Represents a range of values defined by start and end value.
    /// </summary>
    public class Range<T>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Range{T}"/> class using the default comparer.
        /// </summary>
        /// <param name="start">The start value of the range.</param>
        /// <param name="end">The end value of the range.</param>
        public Range(T start, T end)
        {
            Start = start;
            End = end;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the start value of the range.
        /// </summary>
        public T Start { get; }

        /// <summary>
        /// Gets the end value of the range.
        /// </summary>
        public T End { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the range contains the given value.
        /// </summary>
        /// <param name="value">The value to be compared with.</param>
        /// <returns>True if the value exists within the range; false otherwise.</returns>
        public bool Contains(T value)
        {
            return Contains(value, Comparer<T>.Default);
        }

        /// <summary>
        /// Determines whether the range contains the given value.
        /// </summary>
        /// <param name="value">The value to be compared with.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the value exists within the range; false otherwise.</returns>
        public bool Contains(T value, IComparer<T> comparer)
        {
            return Contains(value, comparer.Compare);
        }

        /// <summary>
        /// Determines whether the range contains the given value.
        /// </summary>
        /// <param name="value">The value to be compared with.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the value exists within the range; false otherwise.</returns>
        public bool Contains(T value, Comparison<T> comparison)
        {
            return comparison(Start, value) <= 0 && comparison(value, End) <= 0;
        }

        /// <summary>
        /// Determines whether the range contains the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <returns>True if the given range exists within this range; false otherwise.</returns>
        public bool Contains(Range<T> range)
        {
            return Contains(range, Comparer<T>.Default);
        }

        /// <summary>
        /// Determines whether the range contains the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the given range exists within this range; false otherwise.</returns>
        public bool Contains(Range<T> range, IComparer<T> comparer)
        {
            return Contains(range, comparer.Compare);
        }

        /// <summary>
        /// Determines whether the range contains the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the given range exists within this range; false otherwise.</returns>
        public bool Contains(Range<T> range, Comparison<T> comparison)
        {
            return comparison(Start, range.Start) <= 0 && comparison(range.End, End) <= 0;
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<T> range)
        {
            return Overlaps(range, Comparer<T>.Default);
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<T> range, IComparer<T> comparer)
        {
            return Overlaps(range, comparer.Compare);
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<T> range, Comparison<T> comparison)
        {
            return comparison(Start, range.End) <= 0 && comparison(range.Start, End) <= 0;
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        public Range<T> Merge(Range<T> range)
        {
            return Merge(range, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        public Range<T> Merge(Range<T> range, IComparer<T> comparer)
        {
            return Merge(range, comparer.Compare);
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        public Range<T> Merge(Range<T> range, Comparison<T> comparison)
        {
            T start = comparison(Start, range.Start) <= 0 ? Start : range.Start;
            T end = comparison(End, range.End) >= 0 ? End : range.End;
            return new Range<T>(start, end);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<T> Merge(IEnumerable<Range<T>> ranges)
        {
            return Merge(ranges, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<T> Merge(IEnumerable<Range<T>> ranges, Comparer<T> comparer)
        {
            return Merge(ranges, comparer.Compare);
        }

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<T> Merge(IEnumerable<Range<T>> ranges, Comparison<T> comparison)
        {
            return ranges.Aggregate((range1, range2) => range1.Merge(range2, comparison));
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<T>> MergeConsecutiveOverlapping(IEnumerable<Range<T>> ranges)
        {
            return MergeConsecutiveOverlapping(ranges, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<T>> MergeConsecutiveOverlapping(IEnumerable<Range<T>> ranges, IComparer<T> comparer)
        {
            return MergeConsecutiveOverlapping(ranges, comparer.Compare);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<T>> MergeConsecutiveOverlapping(IEnumerable<Range<T>> ranges, Comparison<T> comparison)
        {
            Range<T> currentRange = null;

            foreach (Range<T> range in ranges)
            {
                if ((object)currentRange == null)
                {
                    currentRange = range;
                }
                else if (currentRange.Overlaps(range, comparison))
                {
                    currentRange = currentRange.Merge(range, comparison);
                }
                else
                {
                    yield return currentRange;
                    currentRange = range;
                }
            }

            if ((object)currentRange != null)
                yield return currentRange;
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<T>> MergeAllOverlapping(IEnumerable<Range<T>> ranges)
        {
            return MergeAllOverlapping(ranges, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<T>> MergeAllOverlapping(IEnumerable<Range<T>> ranges, Comparison<T> comparison)
        {
            return MergeAllOverlapping(ranges, Comparer<T>.Create(comparison));
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<T>> MergeAllOverlapping(IEnumerable<Range<T>> ranges, IComparer<T> comparer)
        {
            return MergeConsecutiveOverlapping(ranges.OrderBy(range => range.Start, comparer));
        }

        #endregion
    }

    /// <summary>
    /// Represents a range of values defined by a start and end value with encapsulated state.
    /// </summary>
    /// <remarks>
    /// This class provides flexibility in allowing the user to define their own state and how it is merged.
    /// Because of potentially undesirable behavior which could result from merging objects inherited from
    /// this class, the class has been sealed to prevent inheritance.
    /// </remarks>
    public sealed class Range<TRange, TState>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Range{TRange, TState}"/> class using the default comparer.
        /// </summary>
        /// <param name="start">The start value of the range.</param>
        /// <param name="end">The end value of the range.</param>
        /// <param name="state">The state object encapsulated by the range.</param>
        public Range(TRange start, TRange end, TState state)
        {
            Start = start;
            End = end;
            State = state;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the start value of the range.
        /// </summary>
        public TRange Start { get; }

        /// <summary>
        /// Gets the end value of the range.
        /// </summary>
        public TRange End { get; }

        /// <summary>
        /// Gets the state encapsulated by the range.
        /// </summary>
        public TState State { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the range contains the given value.
        /// </summary>
        /// <param name="value">The value to be compared with.</param>
        /// <returns>True if the value exists within the range; false otherwise.</returns>
        public bool Contains(TRange value)
        {
            return Contains(value, Comparer<TRange>.Default);
        }

        /// <summary>
        /// Determines whether the range contains the given value.
        /// </summary>
        /// <param name="value">The value to be compared with.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>True if the value exists within the range; false otherwise.</returns>
        public bool Contains(TRange value, IComparer<TRange> comparer)
        {
            return Contains(value, comparer.Compare);
        }

        /// <summary>
        /// Determines whether the range contains the given value.
        /// </summary>
        /// <param name="value">The value to be compared with.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>True if the value exists within the range; false otherwise.</returns>
        public bool Contains(TRange value, Comparison<TRange> comparison)
        {
            return comparison(Start, value) <= 0 && comparison(value, End) <= 0;
        }

        /// <summary>
        /// Determines whether the range contains the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <returns>True if the given range exists within this range; false otherwise.</returns>
        public bool Contains(Range<TRange, TState> range)
        {
            return Contains(range, Comparer<TRange>.Default);
        }

        /// <summary>
        /// Determines whether the range contains the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>True if the given range exists within this range; false otherwise.</returns>
        public bool Contains(Range<TRange, TState> range, IComparer<TRange> comparer)
        {
            return Contains(range, comparer.Compare);
        }

        /// <summary>
        /// Determines whether the range contains the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>True if the given range exists within this range; false otherwise.</returns>
        public bool Contains(Range<TRange, TState> range, Comparison<TRange> comparison)
        {
            return comparison(Start, range.Start) <= 0 && comparison(range.End, End) <= 0;
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<TRange, TState> range)
        {
            return Overlaps(range, Comparer<TRange>.Default);
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<TRange, TState> range, IComparer<TRange> comparer)
        {
            return Overlaps(range, comparer.Compare);
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<TRange, TState> range, Comparison<TRange> comparison)
        {
            return comparison(Start, range.End) <= 0 && comparison(range.Start, End) <= 0;
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        /// <exception cref="InvalidOperationException"><typeparamref name="TState"/> is not <see cref="IMergeable{TState}"/>.</exception>
        public Range<TRange, TState> Merge(Range<TRange, TState> range)
        {
            return Merge(range, Comparer<TRange>.Default);
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        /// <exception cref="InvalidOperationException"><typeparamref name="TState"/> is not <see cref="IMergeable{TState}"/>.</exception>
        public Range<TRange, TState> Merge(Range<TRange, TState> range, IComparer<TRange> comparer)
        {
            return Merge(range, comparer.Compare);
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        /// <exception cref="InvalidOperationException"><typeparamref name="TState"/> is not <see cref="IMergeable{TState}"/>.</exception>
        public Range<TRange, TState> Merge(Range<TRange, TState> range, Comparison<TRange> comparison)
        {
            if (!(State is IMergeable<TState>))
                throw new InvalidOperationException("Unable to merge ranges because TState is not IMergeable<TState>.");

            TRange start = comparison(Start, range.Start) <= 0 ? Start : range.Start;
            TRange end = comparison(End, range.End) >= 0 ? End : range.End;
            IMergeable<TState> mergeableState = (IMergeable<TState>)State;
            TState state = mergeableState.Merge(range.State);
            return new Range<TRange, TState>(start, end, state);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<TRange, TState> Merge(IEnumerable<Range<TRange, TState>> ranges)
        {
            return Merge(ranges, Comparer<TRange>.Default);
        }

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<TRange, TState> Merge(IEnumerable<Range<TRange, TState>> ranges, Comparer<TRange> comparer)
        {
            return Merge(ranges, comparer.Compare);
        }

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<TRange, TState> Merge(IEnumerable<Range<TRange, TState>> ranges, Comparison<TRange> comparison)
        {
            return ranges.Aggregate((range1, range2) => range1.Merge(range2, comparison));
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<TRange, TState>> MergeConsecutiveOverlapping(IEnumerable<Range<TRange, TState>> ranges)
        {
            return MergeConsecutiveOverlapping(ranges, Comparer<TRange>.Default);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<TRange, TState>> MergeConsecutiveOverlapping(IEnumerable<Range<TRange, TState>> ranges, IComparer<TRange> comparer)
        {
            return MergeConsecutiveOverlapping(ranges, comparer.Compare);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<TRange, TState>> MergeConsecutiveOverlapping(IEnumerable<Range<TRange, TState>> ranges, Comparison<TRange> comparison)
        {
            Range<TRange, TState> currentRange = null;

            foreach (Range<TRange, TState> range in ranges)
            {
                if ((object)currentRange == null)
                {
                    currentRange = range;
                }
                else if (currentRange.Overlaps(range, comparison))
                {
                    currentRange = currentRange.Merge(range, comparison);
                }
                else
                {
                    yield return currentRange;
                    currentRange = range;
                }
            }

            if ((object)currentRange != null)
                yield return currentRange;
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<TRange, TState>> MergeAllOverlapping(IEnumerable<Range<TRange, TState>> ranges)
        {
            return MergeAllOverlapping(ranges, Comparer<TRange>.Default);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<TRange, TState>> MergeAllOverlapping(IEnumerable<Range<TRange, TState>> ranges, Comparison<TRange> comparison)
        {
            return MergeAllOverlapping(ranges, Comparer<TRange>.Create(comparison));
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="TRange"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<TRange, TState>> MergeAllOverlapping(IEnumerable<Range<TRange, TState>> ranges, IComparer<TRange> comparer)
        {
            return MergeConsecutiveOverlapping(ranges.OrderBy(range => range.Start, comparer));
        }

        #endregion
    }

    /// <summary>
    /// Represents a type of object that is mergeable with another type of object.
    /// </summary>
    /// <typeparam name="T">The type of object to be merged with.</typeparam>
    /// <remarks>
    /// This interface was primarily designed for objects which are mergeable with other objects of the same type.
    /// Therefore, <typeparamref name="T"/> should typically be the actual type of the object.
    /// </remarks>
    public interface IMergeable<T>
    {
        /// <summary>
        /// Merges this object with another.
        /// </summary>
        /// <param name="other">The other mergeable object.</param>
        /// <returns>The result of merging this object with another.</returns>
        T Merge(T other);
    }
}
