//******************************************************************************************************
//  SampleRate.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  05/19/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Globalization;

namespace GSF.COMTRADE
{
    /// <summary>
    /// Represents a sample rate with associated ending sample using the COMTRADE file standard, IEEE Std C37.111-1999.
    /// </summary>
    public struct SampleRate
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Sample rate in Hertz (Hz).
        /// </summary>
        public double Rate;

        /// <summary>
        /// Last sample number at sample rate.
        /// </summary>
        public long EndSample;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SampleRate"/> from an existing line image.
        /// </summary>
        /// <param name="lineImage">Line image to parse.</param>
        /// <param name="useRelaxedValidation">Indicates whether to relax validation on the number of line image elements.</param>
        public SampleRate(string lineImage, bool useRelaxedValidation = false)
        {
            // samp,endsamp
            string[] parts = lineImage.Split(',');

            if (parts.Length < 2 || (!useRelaxedValidation && parts.Length != 2))
                throw new InvalidOperationException($"Unexpected number of line image elements for sample rate definition: {parts.Length} - expected 2{Environment.NewLine}Image = {lineImage}");

            Rate = double.Parse(parts[0].Trim());
            EndSample = long.Parse(parts[1].Trim());
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts <see cref="SampleRate"/> to its string format.
        /// </summary>
        public override string ToString()
        {
            // samp,endsamp
            return $"{Rate.ToString(CultureInfo.InvariantCulture)},{EndSample}";
        }

        #endregion
    }
}
