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
        /// Base on the <see cref="EPSG3857"/> reference system.
        /// </summary>
        /// <param name="other">Other <see cref="GeoCoordinate"/>.</param>
        /// <returns>Distance between two <see cref="GeoCoordinate"/> values.</returns>
        public double Distance(GeoCoordinate other)
        {
            CoordinateReferenceSystem referenceSystem = new EPSG3857();
            return Distance(other, referenceSystem);
        }

        /// <summary>
        /// Calculates distance between this and another <see cref="GeoCoordinate"/> value.
        /// using the specified <see cref="CoordinateReferenceSystem"/>
        /// </summary>
        /// <param name="other">Other <see cref="GeoCoordinate"/>.</param>
        /// <param name="referenceSystem">The <see cref="CoordinateReferenceSystem"/> used.</param>
        /// <returns>Distance between two <see cref="GeoCoordinate"/> values.</returns>
        public double Distance(GeoCoordinate other, CoordinateReferenceSystem referenceSystem) => referenceSystem.Distance(this, other);
        #endregion
    }
}
