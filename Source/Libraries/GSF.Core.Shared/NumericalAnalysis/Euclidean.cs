//******************************************************************************************************
//  Euclidean.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
        /// Implementation of the modulo operator using Euclidean division.
        /// </summary>
        /// <param name="numerator">The number to be divided.</param>
        /// <param name="denominator">The number to divide by.</param>
        /// <returns></returns>
        public static double Mod(double numerator, double denominator)
        {
            double quotient = Math.Floor(numerator / denominator);
            return numerator - quotient * denominator;
        }

        /// <summary>
        /// Wraps a value to a range of values defined
        /// by the given minimum value and range.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        /// <param name="minimum">The minimum value of the range.</param>
        /// <param name="range">The size of the range.</param>
        /// <returns>The given value wrapped to the given range.</returns>
        /// <remarks>
        /// This method wraps the given value based on the assumption that
        /// for every pair of values x and y where x-y=range, the values are
        /// equivalent. This is probably most widely understood in terms of
        /// angles, where 0, 360, 720, etc. are all equivalent angles. If
        /// you wanted to wrap an angle such that it is between 120 and 480,
        /// for instance, you could call Euclidean.Wrap(angle, 120, 360).
        /// </remarks>
        public static double Wrap(double value, double minimum, double range)
        {
            double transform = value - minimum;
            double remainder = Mod(transform, range);
            return remainder + minimum;
        }

        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static int GreatestCommonDenominator(this IEnumerable<int> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Aggregate(GreatestCommonDenominator);
        }

        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static int GreatestCommonDenominator(params int[] source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

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
            while (true)
            {
                if (b == 0)
                    return a;

                int a1 = a;
                a = b;
                b = a1 % b;
            }
        }

        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static long GreatestCommonDenominator(this IEnumerable<long> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Aggregate(GreatestCommonDenominator);
        }

        /// <summary>
        /// Gets the greatest common denominator of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The greatest common denominator.</returns>
        public static long GreatestCommonDenominator(params long[] source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

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
            while (true)
            {
                if (b == 0)
                    return a;

                long a1 = a;
                a = b;
                b = a1 % b;
            }
        }

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static int LeastCommonMultiple(this IEnumerable<int> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static int LeastCommonMultiple(params int[] source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of the given integers.
        /// </summary>
        /// <param name="a">The first of the given integers.</param>
        /// <param name="b">The second of the given integers.</param>
        /// <returns>The least common multiple.</returns>
        public static int LeastCommonMultiple(int a, int b) => 
            a * (b / GreatestCommonDenominator(a, b));

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static long LeastCommonMultiple(this IEnumerable<long> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of all the integers in the source collection.
        /// </summary>
        /// <param name="source">The collection of integers.</param>
        /// <returns>The least common multiple.</returns>
        public static long LeastCommonMultiple(params long[] source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Aggregate(LeastCommonMultiple);
        }

        /// <summary>
        /// Gets the least common multiple of the given integers.
        /// </summary>
        /// <param name="a">The first of the given integers.</param>
        /// <param name="b">The second of the given integers.</param>
        /// <returns>The least common multiple.</returns>
        public static long LeastCommonMultiple(long a, long b) => 
            a * (b / GreatestCommonDenominator(a, b));
    }
}
