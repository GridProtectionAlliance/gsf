//******************************************************************************************************
//  NumericalAnalysisExtensions.cs - Gbtc
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
//  09/18/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  01/19/2017 - J. Ritchie Carroll
//       Added option to handle sample size calculations, i.e., n - 1
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.NumericalAnalysis
{
    /// <summary>
    /// Defines extension functions related to numerical analysis over a sequence of data.
    /// </summary>
    public static class NumericalAnalysisExtensions
    {
        /// <summary>
        /// Computes the standard deviation over a sequence of <see cref="double"/> values.
        /// </summary>
        /// <param name="source">Source data sample.</param>
        /// <param name="calculateForSample">Set to <c>true</c> to calculate for estimated population size, or <c>false</c> for full population.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> does not contain enough values to produce a result.</exception>
        public static double StandardDeviation(this IEnumerable<double> source, bool calculateForSample = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            double[] values = source as double[] ?? source.ToArray();
            int sampleCount = values.Length - (calculateForSample ? 1 : 0);

            if (sampleCount < 1)
                throw new ArgumentOutOfRangeException(nameof(source), "Not enough sample values provided to produce a result");

            double sampleAverage = values.Average();
            double totalVariance = values.Select(item => item - sampleAverage).Select(deviation => deviation * deviation).Sum();

            return Math.Sqrt(totalVariance / sampleCount);
        }

        /// <summary>
        /// Computes the standard deviation over a sequence of <see cref="double"/> values.
        /// </summary>
        /// <param name="source">Source data sample.</param>
        /// <param name="selector">Used to map value from enumerable of <typeparamref name="T"/> to enumerable of <see cref="double"/>.</param>
        /// <param name="calculateForSample">Set to <c>true</c> to calculate for estimated population size, or <c>false</c> for full population.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> does not contain enough values to produce a result.</exception>
        /// <typeparam name="T">Type of source used to extract double.</typeparam>
        public static double StandardDeviation<T>(this IEnumerable<T> source, Func<T, double> selector, bool calculateForSample = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Select(selector).StandardDeviation(calculateForSample);
        }

        /// <summary>
        /// Computes the standard deviation over a sequence of <see cref="decimal"/> values.
        /// </summary>
        /// <param name="source">Source data sample.</param>
        /// <param name="calculateForSample">Set to <c>true</c> to calculate for estimated population size, or <c>false</c> for full population.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> does not contain enough values to produce a result.</exception>
        public static decimal StandardDeviation(this IEnumerable<decimal> source, bool calculateForSample = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            decimal[] values = source as decimal[] ?? source.ToArray();
            int sampleCount = values.Length - (calculateForSample ? 1 : 0);

            if (sampleCount < 1)
                throw new ArgumentOutOfRangeException(nameof(source), "Not enough sample values provided to produce a result");

            decimal sampleAverage = values.Average();
            decimal totalVariance = values.Select(item => item - sampleAverage).Select(deviation => deviation * deviation).Sum();

            return (decimal)Math.Sqrt((double)(totalVariance / sampleCount));
        }

        /// <summary>
        /// Computes the standard deviation over a sequence of <see cref="decimal"/> values.
        /// </summary>
        /// <param name="source">Source data sample.</param>
        /// <param name="selector">Used to map value from enumerable of <typeparamref name="T"/> to enumerable of <see cref="decimal"/>.</param>
        /// <param name="calculateForSample">Set to <c>true</c> to calculate for estimated population size, or <c>false</c> for full population.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> does not contain enough values to produce a result.</exception>
        /// <typeparam name="T">Type of source used to extract decimal.</typeparam>
        public static decimal StandardDeviation<T>(this IEnumerable<T> source, Func<T, decimal> selector, bool calculateForSample = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Select(selector).StandardDeviation(calculateForSample);
        }

        /// <summary>
        /// Computes the standard deviation over a sequence of <see cref="float"/> values.
        /// </summary>
        /// <param name="source">Source data sample.</param>
        /// <param name="calculateForSample">Set to <c>true</c> to calculate for estimated population size, or <c>false</c> for full population.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> does not contain enough values to produce a result.</exception>
        public static float StandardDeviation(this IEnumerable<float> source, bool calculateForSample = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            float[] values = source as float[] ?? source.ToArray();
            int sampleCount = values.Length - (calculateForSample ? 1 : 0);

            if (sampleCount < 1)
                throw new ArgumentOutOfRangeException(nameof(source), "Not enough sample values provided to produce a result");

            float sampleAverage = values.Average();
            float totalVariance = values.Select(item => item - sampleAverage).Select(deviation => deviation * deviation).Sum();

            return (float)Math.Sqrt(totalVariance / sampleCount);
        }

        /// <summary>
        /// Computes the standard deviation over a sequence of <see cref="float"/> values.
        /// </summary>
        /// <param name="source">Source data sample.</param>
        /// <param name="selector">Used to map value from enumerable of <typeparamref name="T"/> to enumerable of <see cref="float"/>.</param>
        /// <param name="calculateForSample">Set to <c>true</c> to calculate for estimated population size, or <c>false</c> for full population.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> does not contain enough values to produce a result.</exception>
        /// <typeparam name="T">Type of source used to extract float.</typeparam>
        public static float StandardDeviation<T>(this IEnumerable<T> source, Func<T, float> selector, bool calculateForSample = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "source is null");

            return source.Select(selector).StandardDeviation(calculateForSample);
        }
    }
}
