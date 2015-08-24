//*******************************************************************************************************
//  NumericalAnalysisExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/18/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace TVA.NumericalAnalysis
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
            if (source == null) throw new ArgumentNullException("source", "source is null");

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
            if (source == null) throw new ArgumentNullException("source", "source is null");

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
            if (source == null) throw new ArgumentNullException("source", "source is null");

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
