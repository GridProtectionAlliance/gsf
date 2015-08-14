//******************************************************************************************************
//  GaussianDistribution.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  04/30/2014 - Stephen E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

//------------------------------------------------------------------------------------------------------
// Code base on Wikipedia article http://en.wikipedia.org/wiki/Box–Muller_transform
//------------------------------------------------------------------------------------------------------

using System;

namespace GSF.NumericalAnalysis
{
    /// <summary>
    /// Implements a BoxMuller method for generating statistically normal random numbers.
    /// </summary>
    public class GaussianDistribution
    {
        #region [ Members ]

        // Fields
        private readonly Random m_random;
        private readonly double m_mean;
        private readonly double m_standardDeviation;
        private readonly double m_min;
        private readonly double m_max;
        private bool m_z1IsValid;
        private double m_z1;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="GaussianDistribution"/>
        /// </summary>
        /// <param name="mean">the mean of the distribution</param>
        /// <param name="standardDeviation">the standard deviation</param>
        /// <param name="min">a clipping boundary</param>
        /// <param name="max">a clipping boundary</param>
        public GaussianDistribution(double mean, double standardDeviation, double min, double max)
        {
            standardDeviation = Math.Abs(standardDeviation);

            //These limits are set to prevent excessive looping when a value is calculated outside this range.
            if (min > mean - 0.25 * standardDeviation)
                throw new ArgumentOutOfRangeException("min", "must be less than 1/4 standard deviations away from the mean");

            if (max < mean + 0.25 * standardDeviation)
                throw new ArgumentOutOfRangeException("max", "must be greater than 1/4 standard deviations away from the mean");

            m_random = new Random(Guid.NewGuid().GetHashCode());
            m_mean = mean;
            m_standardDeviation = standardDeviation;
            m_min = min;
            m_max = max;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the next random value.
        /// </summary>
        /// <returns></returns>
        public double Next()
        {
            double value;

        TryAgain:

            if (m_z1IsValid)
            {
                m_z1IsValid = false;
                value = m_mean + m_standardDeviation * m_z1;
            }
            else
            {
                double u1 = m_random.NextDouble();
                double u2 = m_random.NextDouble();

                if (u1 < 1e-100)
                    u1 = 1e-100;

                if (u2 < 1e-100)
                    u1 = 1e-100;

                double sqrt = Math.Sqrt(-2 * Math.Log(u1));
                double z0 = sqrt * Math.Sin(2 * Math.PI * u2);

                m_z1 = sqrt * Math.Cos(2 * Math.PI * u2);
                m_z1IsValid = true;

                value = m_mean + m_standardDeviation * z0;
            }

            if (value < m_min || value > m_max)
                goto TryAgain;

            return value;
        }

        #endregion
    }
}
