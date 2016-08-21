//******************************************************************************************************
//  IProjection.cs - Gbtc
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

using GSF.Drawing;

namespace GSF.Geo
{
    /// <summary>
    /// Defines a map projection to translate geographical
    /// coordinates to points in an xy-coordinate system.
    /// </summary>
    public interface IProjection
    {
        /// <summary>
        /// Projects the given coordinates onto the xy-coordinate system.
        /// </summary>
        /// <param name="coordinate">The geographical coordinates to be projected.</param>
        /// <returns>The given coordinates projected onto the xy-coordinate system.</returns>
        Point Project(GeoCoordinate coordinate);

        /// <summary>
        /// Unprojects the given point to the geographical coordinate system.
        /// </summary>
        /// <param name="point">The point to be unprojected.</param>
        /// <returns>The geographical coordinates of the given point.</returns>
        GeoCoordinate Unproject(Point point);
    }
}
