//******************************************************************************************************
//  CoordinateReferenceSystem.cs - Gbtc
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
    /// Abstract class that defines coordinate reference systems for projecting
    /// geographical points into pixel (screen) coordinates and back.
    /// </summary>
    /// <remarks>http://en.wikipedia.org/wiki/Coordinate_reference_system</remarks>
    public abstract class CoordinateReferenceSystem
    {
        /// <summary>
        /// Gets the projection used by the CRS.
        /// </summary>
        public abstract IProjection Projection { get; }

        /// <summary>
        /// Gets the transformation used by the CRS.
        /// </summary>
        public abstract Transformation Transformation { get; }

        /// <summary>
        /// Translates the given geographical coordinate
        /// to a cartesian point at the given zoom level.
        /// </summary>
        /// <param name="coordinate">The coordinate to be translated.</param>
        /// <param name="zoom">The zoom level.</param>
        /// <returns>The cartesian point corresponding to the given coordinate.</returns>
        public virtual Point Translate(GeoCoordinate coordinate, double zoom)
        {
            Point projectedPoint = Projection.Project(coordinate);
            double scale = Scale(zoom);
            return Transformation.Transform(projectedPoint, scale);
        }

        /// <summary>
        /// Translates the given cartesian point to a
        /// geographical coordinate at the given zoom level.
        /// </summary>
        /// <param name="point">The point to be translated.</param>
        /// <param name="zoom">The zoom level.</param>
        /// <returns>The geographical location of the point.</returns>
        public virtual GeoCoordinate Translate(Point point, double zoom)
        {
            double scale = Scale(zoom);
            Point untransformedPoint = Transformation.Untransform(point, scale);
            return Projection.Unproject(untransformedPoint);
        }

        /// <summary>
        /// Returns the scale used when transforming projected coordinates into
        /// pixel coordinates for a particular zoom. For example, it returns
        /// <c>256 * 2^zoom</c> for Mercator-based CRS.
        /// </summary>
        /// <param name="zoom">The zoom level.</param>
        /// <returns>The scale at the given zoom level.</returns>
        public virtual double Scale(double zoom)
        {
            return 256 * Math.Pow(2, zoom);
        }

        /// <summary>
        /// Returns the zoom level corresponding to the given scale factor.
        /// </summary>
        /// <param name="scale">The scale factor.</param>
        /// <returns>The zoom level corresponding to the given scale factor.</returns>
        public virtual double Zoom(double scale)
        {
            return Math.Log(scale / 256) / Math.Log(2);
        }

        /// <summary>
        /// Returns the distance between two geographical coordinates.
        /// </summary>
        /// <param name="coordinate1">The first geographical coordinate.</param>
        /// <param name="coordinate2">The second geographical coordinate.</param>
        /// <returns>The distance between two geographical coordinates.</returns>
        public abstract double Distance(GeoCoordinate coordinate1, GeoCoordinate coordinate2);
    }
}
