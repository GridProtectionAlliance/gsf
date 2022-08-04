//******************************************************************************************************
//  Earth.cs - Gbtc
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

namespace GSF.Geo
{
    /// <summary>
    /// Serves as the base for CRS that are global such that they cover the earth.
    /// </summary>
    public abstract class Earth : CoordinateReferenceSystem
    {
        /// <summary>
        /// Returns the distance, in meters, between two geographical coordinates.
        /// </summary>
        /// <param name="coordinate1">The first geographical coordinate.</param>
        /// <param name="coordinate2">The second geographical coordinate.</param>
        /// <returns>The distance between two geographical coordinates.</returns>
        public override double Distance(GeoCoordinate coordinate1, GeoCoordinate coordinate2)
        {
            const double RadianConversionFactor = Math.PI / 180.0D;
            const double R = 6378137;

            double phi1 = coordinate1.Latitude * RadianConversionFactor;
            double phi2 = coordinate2.Latitude * RadianConversionFactor;
            double lambda1 = coordinate1.Longitude * RadianConversionFactor;
            double lambda2 = coordinate2.Longitude * RadianConversionFactor;
            double dPhi = phi2 - phi1;
            double dLambda = lambda2 - lambda1;

            double sinPhi = Math.Sin(dPhi / 2);
            double sinLambda = Math.Sin(dLambda / 2);

            double a = sinPhi * sinPhi +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    sinLambda * sinLambda;

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}
