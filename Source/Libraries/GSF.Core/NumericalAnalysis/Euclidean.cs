//******************************************************************************************************
//  Euclidean.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  12/30/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.NumericalAnalysis
{
    /// <summary>
    /// Contains an implementation of greatest common denominator
    /// and least common multiple using the Euclidean algorithm.
    /// </summary>
    public static class Euclidean
    {
        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static int GreatestCommonDenominator(this IEnumerable<int> source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(GreatestCommonDenominator);
        }

        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static int GreatestCommonDenominator(params int[] source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(GreatestCommonDenominator);
        }

        /// <summary>
        /// Gets the greatest common denominator of the given integers.
        /// </summary>
        /// <param name="a">The first of the given integers.</param>
        /// <param name="b">The second of the given integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static int GreatestCommonDenominator(int a, int b)
        {
            return (b != 0) ? GreatestCommonDenominator(b, a % b) : a;
        }

        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static long GreatestCommonDenominator(this IEnumerable<long> source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(GreatestCommonDenominator);
        }

        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static long GreatestCommonDenominator(params long[] source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(GreatestCommonDenominator);
        }

        /// <summary>
        /// Gets the greatest common denominator of the given integers.
        /// </summary>
        /// <param name="a">The first of the given integers.</param>
        /// <param name="b">The second of the given integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static long GreatestCommonDenominator(long a, long b)
        {
            return (b != 0) ? GreatestCommonDenominator(b, a % b) : a;
        }

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static int LeastCommonMultiple(this IEnumerable<int> source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static int LeastCommonMultiple(params int[] source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of the given integers.
        /// </summary>
        /// <param name="a">The first of the given integers.</param>
        /// <param name="b">The second of the given integers.</param>
        /// <returns>The least common multiple.</returns>
        public static int LeastCommonMultiple(int a, int b)
        {
            return a * (b / GreatestCommonDenominator(a, b));
        }

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static long LeastCommonMultiple(this IEnumerable<long> source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static long LeastCommonMultiple(params long[] source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source", "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of the given integers.
        /// </summary>
        /// <param name="a">The first of the given integers.</param>
        /// <param name="b">The second of the given integers.</param>
        /// <returns>The least common multiple.</returns>
        public static long LeastCommonMultiple(long a, long b)
        {
            return a * (b / GreatestCommonDenominator(a, b));
        }
    }
}
