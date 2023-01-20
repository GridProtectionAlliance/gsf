//******************************************************************************************************
//  FFT.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  01/20/2023 - C. Lacker
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GSF.NumericalAnalysis
{
    // <summary>
    /// Contains implementation of a Fast Fourier Transform (FFT)
    /// </summary>
    public class FFT
    {
        #region[ Properties ]

        /// <summary>
        /// The compelx result of the FFT
        /// </summary>
        public Complex[] ComplexMagnitude { get; }

        /// <summary>
        /// The Center of the Frequency Bins used by the FFT
        /// </summary>
        public double[] Frequency { get; }

        /// <summary>
        /// The width of the Frequency bins used by the FFT
        /// </summary>
        public double FrequencyBinWidth { get; }

        #endregion[ Properties ]


        #region[ Constructor ]

        /// <summary>
        /// `
        /// https://en.wikipedia.org/wiki/Cooley%E2%80%93Tukey_FFT_algorithm
        /// </remarks>
        public FFT(double[] data)
        {
            int n = data.Length;
            ComplexMagnitude = new Complex[n];
            Frequency = new double[n];

            double pi_div = 2.0 * Math.PI / n;
            FrequencyBinWidth = pi_div;

            if (n == 1)
            {
                Frequency[0] = 0;
                ComplexMagnitude[0] = new Complex(data[0], 0.0D);
                return;
            }

            List<double> dTemp = data.ToList();
            while (!IsPower2(n))
            {
                dTemp.Add(0.0D);
                n = dTemp.Count();
            }

            ComplexMagnitude = new Complex[n];
            Frequency = new double[n];

            pi_div = 2.0 * Math.PI / n;
            FrequencyBinWidth = pi_div;

            double[] result = new double[n];

            FFT fft_even = new FFT(dTemp.Where((item, index) => index % 2 == 0).ToArray());
            FFT fft_odd = new FFT(dTemp.Where((item, index) => index % 2 == 1).ToArray());

            for (int w = 0; w < (n / 2); w++)
            {
                double a = w * pi_div;
                Frequency[w] = a;
                Frequency[2 * w] = a * 2;

                Complex p = fft_even.ComplexMagnitude[w];
                Complex q = new Complex(Math.Cos(-a), Math.Sin(-a)) * fft_odd.ComplexMagnitude[w];
                ComplexMagnitude[w] = p + q;
                ComplexMagnitude[w + n / 2] = p - q;
            }
        }



        #endregion [ Constructor ]

        #region [ Methods ]

        /// <summary>
        /// Determines if an <see cref="int"/> is a power of 2
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if the integer is a clean power of 2</returns>
        private bool IsPower2(int value)
        {
            ulong v = (ulong)Math.Abs(value);
            return ((v & (v - 1)) == 0);
        }
        #endregion [ Methods ]

    }
}
