//******************************************************************************************************
//  Transformation.cs - Gbtc
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

using GSF.Drawing;

namespace GSF.Geo
{
    /// <summary>
    /// Represents a linear transformation over an xy-coordinate system.
    /// </summary>
    public class Transformation
    {
        #region [ Members ]

        // Fields
        private double m_xScale;
        private double m_xOffset;
        private double m_yScale;
        private double m_yOffset;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Transformation"/> class.
        /// </summary>
        /// <param name="xScale">The scale to be applied to the x-value of the point.</param>
        /// <param name="xOffset">The offset to be applied to the x-value of the point.</param>
        /// <param name="yScale">The scale to be applied to the y-value of the point.</param>
        /// <param name="yOffset">The offset to be applied to the y-value of the point.</param>
        public Transformation(double xScale, double xOffset, double yScale, double yOffset)
        {
            m_xScale = xScale;
            m_xOffset = xOffset;
            m_yScale = yScale;
            m_yOffset = yOffset;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Transforms the given point to another location.
        /// </summary>
        /// <param name="point">The point to be transformed.</param>
        /// <param name="scale">The scale to apply after the transformation.</param>
        /// <returns>The transformed point.</returns>
        public Point Transform(Point point, double scale = 1.0D)
        {
            double x = scale * (point.X * m_xScale + m_xOffset);
            double y = scale * (point.Y * m_yScale + m_yOffset);
            return new Point(x, y);
        }

        /// <summary>
        /// Untransforms the given point to its original location.
        /// </summary>
        /// <param name="point">The transformed point.</param>
        /// <param name="scale">The scale that was applied after the transformation.</param>
        /// <returns>The original point.</returns>
        public Point Untransform(Point point, double scale = 1.0D)
        {
            double x = (point.X / scale - m_xOffset) / m_xScale;
            double y = (point.Y / scale - m_yOffset) / m_yScale;
            return new Point(x, y);
        }

        #endregion
    }
}
