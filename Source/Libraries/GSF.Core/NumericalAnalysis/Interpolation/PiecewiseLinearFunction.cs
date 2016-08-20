//******************************************************************************************************
//  PiecewiseLinearFunction.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  08/20/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.NumericalAnalysis.Interpolation
{
    /// <summary>
    /// Represents a piecewise linear function for
    /// calculating values between pivot points.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The conversion function returned by this class uses a
    /// binary search algorithm to find the appropriate line segment
    /// to use for the calculation. Therefore, the domain must
    /// be specified either in increasing or decreasing order.
    /// </para>
    /// 
    /// <para>
    /// Here is an example of how to use this class.
    /// </para>
    /// 
    /// <code>
    /// Func&lt;double, double&gt; piecewiseLinearFunc = new PiecewiseLinearFunction()
    ///     .SetDomain(-1, 0, 1)
    ///     .SetRange(0, 1, 0);
    ///     
    /// Console.WriteLine(piecewiseLinearFunc(-10));   // -9
    /// Console.WriteLine(piecewiseLinearFunc(-1));    // 0 
    /// Console.WriteLine(piecewiseLinearFunc(-0.5));  // 0.5
    /// Console.WriteLine(piecewiseLinearFunc(0));     // 1
    /// Console.WriteLine(piecewiseLinearFunc(0.5));   // 0.5
    /// Console.WriteLine(piecewiseLinearFunc(1));     // 0
    /// Console.WriteLine(piecewiseLinearFunc(10));    // -9
    /// </code>
    /// </remarks>
    public class PiecewiseLinearFunction
    {
        private Func<double, double> m_converter;
        private double[] m_domainValues;
        private double[] m_rangeValues;

        /// <summary>
        /// Sets the x-values of the pivot points in the piecewise linear function.
        /// </summary>
        /// <param name="domainValues">The x-values of the pivot points.</param>
        /// <returns>A reference to the piecewise linear function.</returns>
        public PiecewiseLinearFunction SetDomain(params double[] domainValues)
        {
            m_converter = null;
            m_domainValues = domainValues;
            return this;
        }

        /// <summary>
        /// Sets the y-values of the pivot points in the piecewise linear function.
        /// </summary>
        /// <param name="rangeValues">The y-values of the pivot points.</param>
        /// <returns>A refernce to the picewise linear function.</returns>
        public PiecewiseLinearFunction SetRange(params double[] rangeValues)
        {
            m_converter = null;
            m_rangeValues = rangeValues;
            return this;
        }

        private Func<double, double> GetConverter()
        {
            double[] domain = m_domainValues ?? new double[0];
            double[] range = m_rangeValues ?? new double[0];

            if (domain.Length != range.Length)
                throw new InvalidOperationException($"Domain of size {domain.Length} does not match range of size {range.Length}.");

            if (domain.Length < 2)
                throw new InvalidOperationException($"At least two pivot points must be defined. Defined: {domain.Length}");

            return m_converter ?? (m_converter = x =>
            {
                int i = 0;
                int j = domain.Length - 1;
                double di = domain[i];
                double dj = domain[j];

                while (j - i > 1)
                {
                    int mid = (i + j) / 2;
                    double dmid = domain[mid];

                    if ((di < dmid && x <= dmid) || (di > dmid && x >= dmid))
                    {
                        j = mid;
                        dj = dmid;
                    }
                    else
                    {
                        i = mid;
                        di = dmid;
                    }
                }

                if (di == dj)
                    return range[i];

                double ri = range[i];
                double rj = range[j];
                return (((x - di) / (dj - di)) * (rj - ri)) + ri;
            });
        }

        /// <summary>
        /// Converts the <see cref="PiecewiseLinearFunction"/> object to a
        /// <see cref="Func{Double, Double}"/> to start converting values.
        /// </summary>
        /// <param name="func">The piecewise linear function to be converted.</param>
        /// <exception cref="InvalidOperationException">
        /// <para>the size of the domain does not equal the size of the range</para>
        /// <para>- or -</para>
        /// <para>less than two pivot points are defined</para>
        /// </exception>
        public static implicit operator Func<double, double>(PiecewiseLinearFunction func)
        {
            return func.GetConverter();
        }
    }
}
