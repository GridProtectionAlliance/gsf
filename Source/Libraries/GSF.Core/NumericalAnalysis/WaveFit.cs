//******************************************************************************************************
//  WaveFit.cs - Gbtc
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
//  06/19/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.NumericalAnalysis
{
    /// <summary>
    /// Represents a sine wave of the form <c>y=A*sin(ω*t+Φ)+δ</c>.
    /// </summary>
    public struct SineWave
    {
        /// <summary>
        /// 2 * pi
        /// </summary>
        public const double TwoPi = 2.0 * Math.PI;

        /// <summary>
        /// Amplitude (A) of the sine wave.
        /// </summary>
        public double Amplitude;

        /// <summary>
        /// Frequency (ω) of the sine wave, in Hz.
        /// </summary>
        public double Frequency;

        /// <summary>
        /// Phase (Φ) shift of the sine wave.
        /// </summary>
        public double Phase;

        /// <summary>
        /// Vertical offset (δ) of the sine wave.
        /// </summary>
        public double Bias;

        /// <summary>
        /// Calculates the y-value for the given time.
        /// </summary>
        /// <param name="t">The time, in seconds.</param>
        /// <returns><c>A*sin(ω*t+Φ)+δ</c></returns>
        public double CalculateY(double t)
        {
            return Amplitude * Math.Sin(TwoPi * Frequency * t + Phase) + Bias;
        }
    }

    /// <summary>
    /// Linear regression algorithm for sine waves.
    /// </summary>
    public static class WaveFit
    {
        /// <summary>
        /// Uses least squares linear regression to calculate the best fit sine wave for the given data.
        /// </summary>
        /// <param name="yValues">The y values of the data points.</param>
        /// <param name="tValues">The time values of the data points, in seconds.</param>
        /// <param name="frequency">The frequency of the sine wave, in Hz.</param>
        /// <returns>A <see cref="SineWave"/> approximated from the given data points.</returns>
        public static SineWave SineFit(double[] yValues, double[] tValues, double frequency)
        {
            double[] z = yValues;
            double[] x = new double[tValues.Length];
            double[] y = new double[tValues.Length];
            double a, b, d;

            double rad = 2.0D * Math.PI * frequency;

            for (int i = 0; i < tValues.Length; i++)
            {
                double angle = rad * tValues[i];
                x[i] = Math.Sin(angle);
                y[i] = Math.Cos(angle);
            }

            CurveFit.LeastSquares(z, x, y, out d, out a, out b);

            return new SineWave
            {
                Amplitude = Math.Sqrt(a * a + b * b),
                Frequency = frequency,
                Phase = Math.Atan2(b, a),
                Bias = d
            };
        }
    }
}
