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
                throw new InvalidOperationException("Clock words array must have four values - cannot parse timestamp");

            int days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0, microseconds = 0;
            ushort[] clockNibbles = new ushort[16];

            //            typedef unsigned short clockBytes[16];
            //  // ***** Get IRIG from data sample *****
            //  for (ii = 0; ii < 16; ii++)
            //    {
            //      clockBytes[ii] = Fifo_Buffer[vIrigOfs + ii/4 + sys.channel_offset + word_offset1];
            //      temp = 12 - (4 * (ii & 0x0003));
            //      clockBytes[ii] = clockBytes[ii] >> temp;
            //      clockBytes[ii] = clockBytes[ii] & 0x000f;
            //    }

            for (int i = 0; i < clockNibbles.Length; i++)
            {
                clockNibbles[i] = clockWords[i / 4];
                clockNibbles[i] >>= (12 - (4 * (i & 0x0003)));
                clockNibbles[i] &= 0x000F;
            }

            int timeIsValid = (clockNibbles[3] & 0x000c);

            if (((timeIsValid & 0x0008) == 0) && (clockNibbles[7] != 0xFF)) //  || irigWasValidOnce
            {
                //irigWasValidOnce = true;
                days = ((clockNibbles[0] & 0x0003) * 100) + ((clockNibbles[1] & 0x000f) * 10) + (clockNibbles[2] & 0x000f);
                hours = ((clockNibbles[3] & 0x0003) * 10) + (clockNibbles[4] & 0x000f);
                minutes = ((clockNibbles[5] & 0x0007) * 10) + (clockNibbles[6] & 0x000f);
                seconds = ((clockNibbles[7] & 0x0007) * 10) + (clockNibbles[8] & 0x000f);

                if (((clockNibbles[9] & 0x000f) < 10) && ((clockNibbles[10] & 0x000f) < 10) && ((clockNibbles[11] & 0x000f) < 10))
                    milliseconds = ((clockNibbles[9] & 0x000f) * 100) + ((clockNibbles[10] & 0x000f) * 10) + (clockNibbles[11] & 0x000f);

                if (((clockNibbles[12] & 0x000f) < 10) && ((clockNibbles[13] & 0x000f) < 10) && ((clockNibbles[14] & 0x000f) < 10))
                    microseconds = ((clockNibbles[12] & 0x000f) * 100) + ((clockNibbles[13] & 0x000f) * 10) + (clockNibbles[14] & 0x000f);
            }

            Value = baseTime.BaselinedTimestamp(BaselineTimeInterval.Year)
                        .AddDays(days)
                        .AddHours(hours)
                        .AddMinutes(minutes)
                        .AddSeconds(seconds)
                        .AddMilliseconds(milliseconds)
                        .AddTicks(Ticks.FromMicroseconds(microseconds));
        }

        //public Timestamp(DateTime baseTime, ushort[] clockWords)
        //{
        //    if ((object)clockWords == null)
        //        throw new NullReferenceException("Clock words array was null - cannot parse timestamp");

        //    if (clockWords.Length != 4)
        //        throw new InvalidOperationException("Clock words array must have four values - cannot parse timestamp");

        //    int days, hours, minutes, seconds, milliseconds, microseconds;
        //    byte highByte, lowByte;

        //    highByte = clockWords[0].HighByte();
        //    lowByte = clockWords[0].LowByte();

        //    days = (highByte.HighNibble() & 0x03) * 100 + highByte.LowNibble() * 10 + lowByte.HighNibble();
        //    hours = (lowByte.LowNibble() & 0x03) * 10;

        //    highByte = clockWords[1].HighByte();
        //    lowByte = clockWords[1].LowByte();

        //    hours += highByte.HighNibble();
        //    minutes = (highByte.LowNibble() & 0x07) * 10 + lowByte.HighNibble();
        //    seconds = (lowByte.LowNibble() & 0x07) * 10;

        //    highByte = clockWords[2].HighByte();
        //    lowByte = clockWords[2].LowByte();

        //    seconds += highByte.HighNibble();

        //    milliseconds = highByte.LowNibble() * 100 + lowByte.HighNibble() * 10 + lowByte.LowNibble();

        //    if (milliseconds > 999)
        //        milliseconds = 0;

        //    highByte = clockWords[3].HighByte();
        //    lowByte = clockWords[3].LowByte();

        //    microseconds = highByte.HighNibble() * 100 + highByte.LowNibble() * 10 + lowByte.HighNibble();

        //    if (microseconds > 999)
        //        microseconds = 0;

        //    Value = baseTime.BaselinedTimestamp(BaselineTimeInterval.Year)
        //                .AddDays(days)
        //                .AddHours(hours)
        //                .AddMinutes(minutes)
        //                .AddSeconds(seconds)
        //                .AddMilliseconds(milliseconds)
        //                .AddTicks(Ticks.FromMicroseconds(microseconds));
        //}
    }
}
