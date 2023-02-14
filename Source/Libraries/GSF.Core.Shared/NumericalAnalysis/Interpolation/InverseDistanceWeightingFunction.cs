//******************************************************************************************************
//  InverseDistanceWeightingFunction.cs - Gbtc
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
    /// Function definition for the inverse distance weighting algorithm.
    /// </summary>
    /// <param name="x">The x-coordinate of the point at which the value is to be calculated.</param>
    /// <param name="y">The y-coordinate of the point at which the value is to be calculated.</param>
    /// <returns>The calculated value at the given location.</returns>
    public delegate double IDWFunc(double x, double y);

    /// <summary>
    /// Function defintion for calculating the distance between two points.
    /// </summary>
    /// <param name="x1">The x-coordinate of the first point.</param>
    /// <param name="y1">The y-coordinate of the first point.</param>
    /// <param name="x2">The x-coordinate of the second point.</param>
    /// <param name="y2">The y-coordinate of the second point.</param>
    /// <returns>The distance between the two given points.</returns>
    public delegate double DistanceFunc(double x1, double y1, double x2, double y2);

    /// <summary>
    /// Represents a function for calculating values
    /// at given coordinates based on sparse data sets.
    /// </summary>
    /// <remarks>
    /// Usage is similar to the <see cref="PiecewiseLinearFunction"/> class.
    /// </remarks>
    public class InverseDistanceWeightingFunction
    {
        #region [ Members ]

        // Fields
        private IDWFunc m_converter;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="InverseDistanceWeightingFunction"/> class.
        /// </summary>
        public InverseDistanceWeightingFunction()
        {
            DistanceFunction = DefaultDistanceFunction;
            Power = 1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the collection of x-coordinates of points at which the values are known.
        /// </summary>
        public double[] XCoordinates { get; private set; }

        /// <summary>
        /// Gets the collection of y-coordinates of points at which the values are known.
        /// </summary>
        public double[] YCoordinates { get; private set; }

        /// <summary>
        /// Gets the collection of values of points at which the values are known.
        /// </summary>
        public double[] Values { get; private set; }

        /// <summary>
        /// Gets the power applied to the inverse distance to control the speed of value's decay.
        /// </summary>
        public double Power { get; private set; }

        /// <summary>
        /// Gets the function to be used to calculate the distance between two points.
        /// </summary>
        public DistanceFunc DistanceFunction { get; private set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets the collection of x-coordinates of points at which the values are known.
        /// </summary>
        /// <param name="xCoordinates">The x-coordinates of points at which the values are known.</param>
        /// <returns>A reference to the inverse distance weighting function.</returns>
        public InverseDistanceWeightingFunction SetXCoordinates(params double[] xCoordinates)
        {
            m_converter = null;
            XCoordinates = xCoordinates;
            return this;
        }

        /// <summary>
        /// Sets the collection of y-coordinates of points at which the values are known.
        /// </summary>
        /// <param name="yCoordinates">The y-coordinates of points at which the values are known.</param>
        /// <returns>A reference to the inverse distance weighting function.</returns>
        public InverseDistanceWeightingFunction SetYCoordinates(params double[] yCoordinates)
        {
            m_converter = null;
            YCoordinates = yCoordinates;
            return this;
        }

        /// <summary>
        /// Sets the collection of values of points at which the values are known.
        /// </summary>
        /// <param name="values">The values of points at which the values are known.</param>
        /// <returns>A reference to the inverse distance weighting function.</returns>
        public InverseDistanceWeightingFunction SetValues(params double[] values)
        {
            m_converter = null;
            Values = values;
            return this;
        }

        /// <summary>
        /// Sets the power applied to the inverse distance to control the speed of value's decay.
        /// </summary>
        /// <param name="power">The power applied to the inverse of the distance.</param>
        /// <returns>A reference to the inverse distance weighting function.</returns>
        /// <remarks>Larger values increase the speed of decay such that a known value affects a smaller area.</remarks>
        public InverseDistanceWeightingFunction SetPower(double power)
        {
            m_converter = null;
            Power = power;
            return this;
        }

        /// <summary>
        /// Sets the function to be used to calculate the distance between two points.
        /// </summary>
        /// <param name="distanceFunction">The function used to calculate distance between two points.</param>
        /// <returns>A reference to the inverse distance weighting function.</returns>
        public InverseDistanceWeightingFunction SetDistanceFunction(DistanceFunc distanceFunction)
        {
            m_converter = null;
            DistanceFunction = distanceFunction ?? DefaultDistanceFunction;
            return this;
        }

        private IDWFunc GetConverter()
        {
            DistanceFunc distanceFunction = DistanceFunction;
            double[] xCoordinates = XCoordinates ?? Array.Empty<double>();
            double[] yCoordinates = YCoordinates ?? Array.Empty<double>();
            double[] values = Values ?? Array.Empty<double>();
            double power = Power;

            if (xCoordinates.Length != yCoordinates.Length)
                throw new InvalidOperationException($"The number of x-coordinates must match the number of y-coordinates. ({xCoordinates.Length} != {yCoordinates.Length})");

            if (xCoordinates.Length != values.Length)
                throw new InvalidOperationException($"The number of coordinates must match the number of values. ({xCoordinates.Length} != {values.Length})");

            if (xCoordinates.Length == 0)
                return (_, _) => 0.0D;

            return m_converter ??= (x, y) =>
            {
                double numerator = 0.0D;
                double denominator = 0.0D;

                for (int i = 0; i < xCoordinates.Length; i++)
                {
                    double distance = distanceFunction(x, y, xCoordinates[i], yCoordinates[i]);

                    if (distance == 0.0D)
                        return values[i];

                    double inverseDistance = Math.Pow(1.0D / distance, power);
                    numerator += values[i] * inverseDistance;
                    denominator += inverseDistance;
                }

                return numerator / denominator;
            };
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Converts the <see cref="InverseDistanceWeightingFunction"/>
        /// object to an <see cref="IDWFunc"/> to start converting values.
        /// </summary>
        /// <param name="idwFunction">The inverse distance wieghting function to be converted.</param>
        /// <exception cref="InvalidOperationException">
        /// <para>the number of x-coordinates does not equal the number of y-coordinates</para>
        /// <para>- or -</para>
        /// <para>the number of coordinates does not equal the number of values</para>
        /// </exception>
        public static implicit operator IDWFunc(InverseDistanceWeightingFunction idwFunction) => 
            idwFunction.GetConverter();

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <returns></returns>
        public static double DefaultDistanceFunction(double x1, double y1, double x2, double y2)
        {
            double xDiff = x1 - x2;
            double yDiff = y1 - y2;
            return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
        }

        #endregion
    }
}
