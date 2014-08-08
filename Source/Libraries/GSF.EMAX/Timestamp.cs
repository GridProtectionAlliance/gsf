//******************************************************************************************************
//  Timestamp.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/06/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.EMAX
{
    /// <summary>
    /// Represents an EMAX timestamp.
    /// </summary>
    public struct Timestamp
    {
        public DateTime Value;

        public Timestamp(DateTime baseTime, ushort[] clockWords)
        {
            if ((object)clockWords == null)
                throw new NullReferenceException("Clock words array was null - cannot parse timestamp");

            if (clockWords.Length != 4)
                throw new InvalidOperationException("Clock works array must have four values - cannot parse timestamp");

            int days, hours, minutes, seconds, milliseconds, microseconds;
            byte highByte, lowByte;

            highByte = clockWords[0].HighByte();
            lowByte = clockWords[0].LowByte();

            days = highByte.LowNibble() * 100 + highByte.HighNibble() * 10 + lowByte.LowNibble();
            hours = lowByte.HighNibble() * 10;

            highByte = clockWords[1].HighByte();
            lowByte = clockWords[1].LowByte();

            hours += highByte.LowNibble();
            minutes = highByte.HighNibble() * 10 + lowByte.LowNibble();
            seconds = lowByte.HighNibble() * 10;

            highByte = clockWords[2].HighByte();
            lowByte = clockWords[2].LowByte();

            seconds += highByte.LowNibble();
            milliseconds = highByte.HighNibble() * 100 + lowByte.LowNibble() * 10 + lowByte.HighNibble();

            highByte = clockWords[3].HighByte();
            lowByte = clockWords[3].LowByte();

            microseconds = highByte.LowNibble() * 100 + highByte.HighNibble() * 10 + lowByte.LowNibble();

            Value = baseTime.BaselinedTimestamp(BaselineTimeInterval.Year)
                        .AddDays(days)
                        .AddHours(hours)
                        .AddMinutes(minutes)
                        .AddSeconds(seconds)
                        .AddMilliseconds(milliseconds)
                        .AddTicks(Ticks.FromMicroseconds(microseconds));
        }
    }
}
