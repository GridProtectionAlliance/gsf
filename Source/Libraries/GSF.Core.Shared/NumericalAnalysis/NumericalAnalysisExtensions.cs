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
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.NumericalAnalysis
{
    /// <summary>Defines extension functions related to numerical analysis over a sequence of data.</summary>
    public static class NumericalAnalysisExtensions
    {
        /// <summary>Computes the standard deviation over a sequence of double values.</summary>
        /// <param name="source">Source data sample.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException">source is null</exception>
        public static double StandardDeviation(this IEnumerable<double> source)
        {
            if ((object)source == null)
                throw new ArgumentNullException(nameof(source), "source is null");

            double sampleAverage = source.Average();
            double totalVariance = 0.0D;
            double dataPointDeviation;
            int sampleCount = 0;

            foreach (double item in source)
            {
                dataPointDeviation = item - sampleAverage;
                totalVariance += dataPointDeviation * dataPointDeviation;
                sampleCount++;
            }

            if (sampleCount > 0)
                return Math.Sqrt(totalVariance / sampleCount);
            else
                return 0.0D;
        }

        /// <summary>Computes the standard deviation over a sequence of decimal values.</summary>
        /// <param name="source">Source data sample.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException">source is null</exception>
        public static decimal StandardDeviation(this IEnumerable<decimal> source)
        {
            if ((object)source == null)
                throw new ArgumentNullException(nameof(source), "source is null");

            decimal sampleAverage = source.Average();
            decimal totalVariance = 0;
            decimal dataPointDeviation;
            int sampleCount = 0;

            foreach (decimal item in source)
            {
                dataPointDeviation = item - sampleAverage;
                totalVariance += dataPointDeviation * dataPointDeviation;
                sampleCount++;
            }

            if (sampleCount > 0)
                return (decimal)Math.Sqrt((double)(totalVariance / sampleCount));
            else
                return 0;
        }

        /// <summary>Computes the standard deviation over a sequence of float values.</summary>
        /// <param name="source">Source data sample.</param>
        /// <returns>The standard deviation of the sequence.</returns>
        /// <exception cref="ArgumentNullException">source is null</exception>
        public static float StandardDeviation(this IEnumerable<float> source)
        {
            if ((object)source == null)
                throw new ArgumentNullException(nameof(source), "source is null");

            float sampleAverage = source.Average();
            float totalVariance = 0.0F;
            float dataPointDeviation;
            int sampleCount = 0;

            foreach (float item in source)
            {
                dataPointDeviation = item - sampleAverage;
                totalVariance += dataPointDeviation * dataPointDeviation;
                sampleCount++;
            }

            if (sampleCount > 0)
                return (float)Math.Sqrt((double)(totalVariance / sampleCount));
            else
                return 0.0F;
        }
    }
}
