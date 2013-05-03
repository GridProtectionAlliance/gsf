//******************************************************************************************************
//  TimeSpanExtensions.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  -----------------------------------------------------------------------------------------------------
//  12/14/2008 - F. Russell Robertson
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;


namespace GSF
{
    /// <summary>
    /// Extends the TimeSpan Class
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Returns a simple string format for a broad range of timespans.
        /// </summary>
        /// <param name="value">The timespan to process.</param>
        /// <returns>A formated string with time lables.</returns>
        /// <example>
        ///   DateTime g_start = DateTime.Now;
        ///   ...
        ///   DateTime EndTime = DateTime.Now;
        ///   TimeSpan duration = EndTime.Subtract(g_start);
        ///   Console.WriteLine("Elasped Time = " + duration.ToFormatedString());
        ///   
        ///   --- OR --- (See DateTimeExtensions, ElaspedTime)
        /// 
        ///   TimeSpan duration = g_start.ElaspedTime();
        ///   Console.WriteLine("Elasped Time = " + duration.ToFormatedString();
        ///
        /// </example>
        public static string ToFormattedString(this TimeSpan value)
        {
            string sign = "";

            if (value.TotalMilliseconds < 0)
            {
                value = value.Negate();
                sign = "-";
            }

            if (value.TotalMilliseconds < 0.001)
                return "0.0 sec";

            if (value.TotalMilliseconds < 0.5)
            {
                double t = value.TotalMilliseconds * 1000.0; // convert to nanoseconds
                return string.Concat(sign, string.Format("{0:0} nsec", t));
            }

            if (value.TotalMilliseconds < 10.0)
                return string.Concat(sign, string.Format("{0:0.00} msec", value.TotalMilliseconds));

            if (value.TotalMilliseconds < 100.0)
                return string.Concat(sign, string.Format("{0:0.0} msec", value.TotalMilliseconds));

            if (value.TotalMilliseconds < 1000.0)
                return string.Concat(sign, string.Format("{0:000} msec", value.TotalMilliseconds));

            if (value.TotalSeconds < 10.0)
                return string.Concat(sign, string.Format("{0:0.00} sec", value.TotalSeconds));

            if (value.TotalMinutes < 60.0)
                return string.Concat(sign, string.Format("{0} min {1} sec", value.Minutes, value.Seconds));

            return string.Concat(sign, string.Format("{0:#,##0} hr {1} min", value.Hours, value.Minutes));
        }
    }
}