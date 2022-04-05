//******************************************************************************************************
//  Spline.cs - Gbtc
//
//  Copyright © 2022, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/04/2022 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.NumericalAnalysis
{
    /// <summary>
    /// One piece of a piecewise polynomial function.
    /// </summary>
    public class Spline
    {
        /// <summary>
        /// The starting value for the dependent
        /// variable at which the spline was computed.
        /// </summary>
        public double XValue { get; }

        /// <summary>
        /// The coefficients of each term in the polynomial.
        /// </summary>
        /// <remarks>
        /// <para>The array index matches the exponent to which the x-value is raised.</para>
        /// 
        /// <code>term[i] = Coefficients[i] * Math.Pow(x - XValue, i);</code>
        /// </remarks>
        public double[] Coefficients { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="Spline"/> class.
        /// </summary>
        /// <param name="xValue">The starting value for the dependent variable at which the spline was computed.</param>
        /// <param name="coefficients">The coefficients of each term in the polynomial.</param>
        public Spline(double xValue, double[] coefficients)
        {
            XValue = xValue;
            Coefficients = coefficients;
        }

        /// <summary>
        /// Calculates the y-value of the polynomial at the given x-value.
        /// </summary>
        /// <param name="x">The x-value at which the polynomial should be used to calculate a y-value.</param>
        /// <returns>The y-value of the polynomial at the given x-value.</returns>
        public double CalculateY(double x) => Coefficients
            .Select((c, i) => c * Math.Pow(x - XValue, i))
            .Sum();
    }

    /// <summary>
    /// A collection of splines computed through spline interpolation.
    /// </summary>
    public class SplineFit
    {
        /// <summary>
        /// The collection of splines used to interpolate a curve.
        /// </summary>
        public Spline[] Splines { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SplineFit"/> class.
        /// </summary>
        /// <param name="splines">The collection of splines.</param>
        public SplineFit(Spline[] splines) =>
            Splines = splines;

        /// <summary>
        /// Calculates the interpolated y-value at the given x-value using the appropriate spline.
        /// </summary>
        /// <param name="x">The x-value from which to calculate the interpolated y-value.</param>
        /// <returns>The interpolated y-value at the given x-value.</returns>
        public double CalculateY(double x)
        {
            if (!Splines.Any())
                return double.NaN;

            for (int i = 0; i < Splines.Length - 1; i++)
            {
                if (x < Splines[i + 1].XValue)
                    return Splines[i].CalculateY(x);
            }

            return Splines.Last().CalculateY(x);
        }

        /// <summary>
        /// Computes a cubic spline for interpolating values from the given data set.
        /// </summary>
        /// <param name="xValues">The x-values of each sampled data point.</param>
        /// <param name="yValues">The y-values of each sampled data point.</param>
        /// <returns>A <see cref="SplineFit"/> representing the collection of splines to be used for interpolation.</returns>
        public static SplineFit ComputeCubicSplines(IList<double> xValues, IList<double> yValues)
        {
            if (xValues.Count != yValues.Count)
                throw new ArgumentException("The number of x-values must match the number of y-values");

            IList<double> x = xValues;
            IList<double> y = yValues;
            int n = xValues.Count - 1;

            double[] a = new double[n + 1];
            double[] b = new double[n];
            double[] c = new double[n + 1];
            double[] d = new double[n];
            for (int i = 0; i < y.Count; i++)
                a[i] = y[i];

            double[] h = new double[n];
            for (int i = 0; i < n; i++)
                h[i] = x[i + 1] - x[i];

            double[] alpha = new double[n];
            for (int i = 1; i < n; i++)
                alpha[i] = 3.0D / h[i] * (a[i + 1] - a[i]) - 3.0D / h[i - 1] * (a[i] - a[i - 1]);

            double[] l = new double[n + 1];
            double[] mu = new double[n + 1];
            double[] z = new double[n + 1];

            l[0] = 1.0D;
            mu[0] = 0.0D;
            z[0] = 0.0D;
            for (int i = 1; i < n; i++)
            {
                l[i] = 2.0D * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                mu[i] = h[i] / l[i];
                z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
            }

            l[n] = 1.0D;
            z[n] = 0.0D;
            c[n] = 0.0D;
            for (int j = n - 1; j >= 0; j--)
            {
                c[j] = z[j] - mu[j] * c[j + 1];
                b[j] = (a[j + 1] - a[j]) / h[j] - h[j] * (c[j + 1] + 2 * c[j]) / 3.0D;
                d[j] = (c[j + 1] - c[j]) / (3.0D * h[j]);
            }

            Spline[] splines = new Spline[n];
            for (int i = 0; i < n; i++)
            {
                double[] coefficients = { a[i], b[i], c[i], d[i] };
                splines[i] = new Spline(x[i], coefficients);
            }

            return new SplineFit(splines);
        }
    }
}
