//******************************************************************************************************
//  Point.cs - Gbtc
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

namespace GSF.Drawing
{
    /// <summary>
    /// Represents a point in an xy-coordinate system.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the x-coordinate of the point.
        /// </summary>
        public double X
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the y-coordinate of the point.
        /// </summary>
        public double Y
        {
            get;
            private set;
        }
    }
}
