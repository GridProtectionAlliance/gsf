//******************************************************************************************************
//  TimeOffset.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  09/29/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Newtonsoft.Json;

namespace GSF.COMTRADE
{
    /// <summary>
    /// Represents a UTC time offset for COMTRADE - format HhMM.
    /// </summary>
    public class TimeOffset
    {
        #region [ Members ]

        // Fields
        private int m_hours;
        private int m_minutes;

        #endregion

        #region [ Constructors ]
        
            /// <summary>
        /// Creates a new instance of the <see cref="TimeOffset"/>
        /// </summary>
        public TimeOffset()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TimeOffset"/>
        /// </summary>
        /// <param name="lineImage">Line image to parse.</param>
        public TimeOffset(string lineImage)
        {
            bool validFormat;
            int hours = 0, minutes = 0;

            if (string.Compare(lineImage, "x", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // An "x" value means not applicable
                NotApplicable = true;
                validFormat = true;
            }
            else
            {
                // Image example: -5h30
                string[] parts = lineImage.Split('h');

                switch (parts.Length)
                {
                    case 1:
                        validFormat = int.TryParse(lineImage, out hours);
                        break;
                    case 2:
                    {
                        validFormat = int.TryParse(parts[0], out hours);

                        if (validFormat)
                            validFormat = int.TryParse(parts[1], out minutes);
                        break;
                    }
                    default:
                        validFormat = false;
                        break;
                }
            }

            if (validFormat)
            {
                Hours = hours;
                Minutes = minutes;                
            }
            else
            {
                throw new FormatException($"Invalid line image format for time offset: expected \"[+/-]HhMM\"{Environment.NewLine}Image = {lineImage}");
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets UTC time offset hours.
        /// </summary>
        public int Hours
        {
            get => m_hours;
            set
            {
                if (Math.Abs(value) > 23)
                    throw new ArgumentOutOfRangeException(nameof(value), "Maximum value for hours is 23.");

                m_hours = value;
            }
        }

        /// <summary>
        /// Gets or sets UTC time offset minutes.
        /// </summary>
        public int Minutes
        {
            get => m_minutes;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Minutes cannot be negative.");

                if (value > 59)
                    throw new ArgumentOutOfRangeException(nameof(value), "Maximum value for minutes is 59.");

                m_minutes = value;
            }
        }

        /// <summary>
        /// Gets or sets value that determines if offset is not applicable.
        /// </summary>
        public bool NotApplicable { get; set; }

        /// <summary>
        /// Gets the total <see cref="TimeOffset"/> value in ticks.
        /// </summary>
        [JsonIgnore]
        public long TickOffset
        {
            get
            {
                long hourOffset = m_hours * Ticks.PerHour;
                long minuteOffset = m_minutes * Ticks.PerMinute;
                return hourOffset < 0 ? hourOffset - minuteOffset : hourOffset + minuteOffset;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts <see cref="TimeOffset"/> to its string format.
        /// </summary>
        public override string ToString() => 
            NotApplicable ? "x" : $"{Hours}h{Minutes:00}";

        #endregion
    }
}