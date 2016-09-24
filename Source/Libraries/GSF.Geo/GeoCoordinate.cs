//******************************************************************************************************
//  GeoCoordinate.cs - Gbtc
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

namespace GSF.Geo
{
    /// <summary>
    /// Represents a location in the geographical coordinate system.
    /// </summary>
    public class GeoCoordinate
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="GeoCoordinate"/> class.
        /// </summary>
        /// <param name="latitude">The latitude of the geographical coordinate.</param>
        /// <param name="longitude">The longitude of the geographical coordinate.</param>
        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the latitude of the geographical coordinate.
        /// </summary>
        public double Latitude
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the longitude of the geographical coordinate.
        /// </summary>
        public double Longitude
        {
            get;
            private set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calculates distance between this and another <see cref="GeoCoordinate"/> value.
        /// </summary>
        /// <param name="other">Other <see cref="GeoCoordinate"/>.</param>
        /// <returns>Distance between two <see cref="GeoCoordinate"/> values.</returns>
        public double Distance(GeoCoordinate other)
        {
            const double RadianConversionFactor = Math.PI / 180.0D;

            double r = 6378137;
            double lambda1 = Longitude * RadianConversionFactor;
            double lambda2 = other.Longitude * RadianConversionFactor;
            double phi1 = Latitude * RadianConversionFactor;
            double phi2 = other.Latitude * RadianConversionFactor;
            double dPhi = phi2 - phi1;
            double dLambda = lambda2 - lambda1;

            double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return r * c;
        }

        #endregion
    }
}
