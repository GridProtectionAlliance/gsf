//******************************************************************************************************
//  Timestamp.cs - Gbtc
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
    /// Represents a timestamp in the COMTRADE file standard format, IEEE Std C37.111-1999..
    /// </summary>
    public struct Timestamp
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Timestamp in ticks.
        /// </summary>
        public Ticks Value;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Timestamp"/> from an existing line image.
        /// </summary>
        /// <param name="lineImage">Line image to parse.</param>
        public Timestamp(string lineImage)
        {
            string[] parts = lineImage.Split(':');

            double seconds;
            double milliseconds = 0.0D;

            if (parts.Length == 4)
            {
                double.TryParse(parts[parts.Length - 1], out milliseconds);
                parts = new[] { parts[0], parts[1], parts[2] };
            }

            double.TryParse(parts[parts.Length - 1], out seconds);

            seconds += milliseconds;

            parts[parts.Length - 1] = seconds.ToString("00.000000");

            lineImage = string.Join(":", parts).RemoveWhiteSpace();

            DateTime result;

            DateTime.TryParseExact(lineImage, new[]
            {
                "d/M/yyyy,H:mm:ss",
                "d/M/yyyy,H:mm:ss.fff",
                "d/M/yyyy,H:mm:ss.ffffff",
                "M/d/yyyy,H:mm:ss",
                "M/d/yyyy,H:mm:ss.fff",
                "M/d/yyyy,H:mm:ss.ffffff"
            },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

            Value = result.Ticks;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts <see cref="Timestamp"/> to its string format.
        /// </summary>
        public override string ToString()
        {
            // dd/mm/yyyy,hh:mm:ss.ssssss
            return Value.ToString("dd/MM/yyyy,HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
