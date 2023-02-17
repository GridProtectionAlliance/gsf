//******************************************************************************************************
//  DigitalFilter.cs - Gbtc
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
//  01/20/2023 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;

namespace GSF.NumericalAnalysis
{
    /// <summary>
    /// Contains an implementation of a digital LTI Filter.
    /// </summary>
    public class DigitalFilter
    {
        #region[ Properties ]

        private readonly double[] m_a;
        private readonly double[] m_b;
        private readonly double m_gain;

        /// <summary>
        /// The input coefficients used to multiply the input signal.
        /// </summary>
        public double[] InputCoefficients => m_b;

        /// <summary>
        /// The output coefficients used to multiply the output signal.
        /// </summary>
        public double[] OutputCoefficients => m_a.Select(a => a * m_gain).ToArray();

        /// <summary>
        /// The order of the filter.
        /// </summary>
        public int Order => Math.Max(m_a.Length, m_b.Length) - 1;

        #endregion

        #region [ Constructors ]
        
        /// <summary>
        /// Creates a new <see cref="DigitalFilter"/> based on the coefficients of
        /// a[0] y[k] + a[1] y[k-1].... = b[0] x[k] + b[1] x[k-1]... b[n] x[k-n]
        /// </summary>
        /// <param name="a"> the output coefficients (a[0] through a[n])</param>
        /// <param name="b"> the input coefficients (b[0] through b[n])</param>
        public DigitalFilter(double[] b, double[] a)
        {
            m_a = a;
            m_b = b;
            m_gain = 1.0;
        }

        /// <summary>
        /// Creates a new <see cref="DigitalFilter"/> based on coeficents and gain
        /// a[0] y[k] + a[1] y[k-1].... = K * (b[0] x[k] + b[1] x[k-1]... b[n] x[k-n]).
        /// </summary>
        /// <param name="a">The output coefficients (a[0] through a[n])</param>
        /// <param name="b">The input coefficients (b[0] through b[n])</param>
        /// <param name="k">The gain of the filter</param>
        public DigitalFilter(double[] b, double[] a, double k) : this(b, a) => 
            m_gain = k;

        #endregion

        #region[ Methods ]

        /// <summary>
        /// applies this filter to an evenly sampled signal f(t).
        /// </summary>
        /// <param name="signal"> f(t) for the signal</param>
        /// <returns>The output of the filter y(t)</returns>
        public double[] Filter(double[] signal)
        {
            int n = signal.Length;
            double[] output = new double[n];

            FilterState state = new();

            for (int i = 0; i < n; i++)
                output[i] = Filter(signal[i], state, out state);

            return output;
        }

        /// <summary>
        /// Computes the output of the filter for the given single input and <see cref="FilterState"/>.
        /// </summary>
        /// <param name="value"> The input value </param>
        /// <param name="initialState">The initial state of the filter </param>
        /// <param name="finalState"> The final State of the Filter</param>
        /// <returns> the value of the filtered signal</returns>
        public double Filter(double value, FilterState initialState, out FilterState finalState)
        {
            double[] s = initialState.StateValue;

            if (s.Length < m_a.Length + m_b.Length - 2)
            {
                s = Enumerable.Repeat(0.0D, m_a.Length + m_b.Length - 2 - s.Length).ToArray();
                s = initialState.StateValue.Concat(s).ToArray();
            }

            double fx = value * m_b[0] + m_b.Select((z, i) => i > 0 ? z * s[i - 1] : 0.0D).Sum();
            fx += m_a.Select((z, i) => i > 0 ? z * -s[i + m_b.Length - 2] : 0.0D).Sum();
            fx /= m_a[0];

            finalState = new FilterState
            {
                StateValue = new double[] { value }
                    .Concat(s.Take(m_b.Length - 2).ToArray())
                    .Concat(new double[] { fx })
                    .Concat(s.Skip(m_b.Length - 1).Take(m_a.Length - 2).ToArray())
                    .ToArray()
            };

            return fx * m_gain;
        }

        /// <summary>
        /// applies the filter to a signal and reverse signal to remove any Phasr shift
        /// </summary>
        /// <param name="signal"> f(t) for the signal</param>
        /// <returns>The output of teh filter y(t)</returns>
        ///<remarks> this is the equvivalent of Matlabs filtfilt </remarks>
        public double FiltFilt(double[] signal)
        {
            double forwardFiltered = Filter(signal);
            forwardFiltered.Reverse();
            return Filter(forwardFiltered);
        }
        #endregion
    }
}
