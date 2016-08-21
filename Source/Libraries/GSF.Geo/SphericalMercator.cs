//******************************************************************************************************
//  SphericalMercator.cs - Gbtc
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

#region [ Contributor License Agreements ]

/*
    Copyright (c) 2010-2016, Vladimir Agafonkin
    Copyright (c) 2010-2011, CloudMade
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are
    permitted provided that the following conditions are met:

       1. Redistributions of source code must retain the above copyright notice, this list of
          conditions and the following disclaimer.

       2. Redistributions in binary form must reproduce the above copyright notice, this list
          of conditions and the following disclaimer in the documentation and/or other materials
          provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
    EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
    COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
    EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
    SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
    TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#endregion

using System;
using GSF.Drawing;

namespace GSF.Geo
{
    /// <summary>
    /// Spherical Mercator projection; the most common projection for online maps.
    /// Assumes that Earth is a sphere. Used by the <c>EPSG:3857</c> CRS.
    /// </summary>
    public class SphericalMercator : IProjection
    {
        /// <summary>
        /// Radius of the Earth (meters).
        /// </summary>
        public const double R = 6378137;

        /// <summary>
        /// The maximum latitude.
        /// </summary>
        public const double MaxLatitude = 85.0511287798;

        /// <summary>
        /// Projects the given coordinates onto the xy-coordinate system.
        /// </summary>
        /// <param name="coordinate">The geographical coordinates to be projected.</param>
        /// <returns>The given coordinates projected onto the xy-coordinate system.</returns>
        public Point Project(GeoCoordinate coordinate)
        {
            const double RadianConversionFactor = Math.PI / 180;

            double lat = Math.Max(Math.Min(MaxLatitude, coordinate.Latitude), -MaxLatitude);
            double sin = Math.Sin(lat * RadianConversionFactor);
            double x = R * coordinate.Longitude * RadianConversionFactor;
            double y = R * Math.Log((1 + sin) / (1 - sin)) / 2;

            return new Point(x, y);
        }

        /// <summary>
        /// Unprojects the given point to the geographical coordinate system.
        /// </summary>
        /// <param name="point">The point to be unprojected.</param>
        /// <returns>The geographical coordinates of the given point.</returns>
        public GeoCoordinate Unproject(Point point)
        {
            const double DegreeConversionFactor = 180 / Math.PI;

            double latitude = (2 * Math.Atan(Math.Exp(point.Y / R)) - (Math.PI / 2)) * DegreeConversionFactor;
            double longitude = point.X * DegreeConversionFactor / R;

            return new GeoCoordinate(latitude, longitude);
        }
    }
}
