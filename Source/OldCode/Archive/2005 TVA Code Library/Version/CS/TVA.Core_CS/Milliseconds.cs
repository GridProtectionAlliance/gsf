//*******************************************************************************************************
//  Milliseconds.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/18/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>Defines functions related to milliseconds.</summary>
    /// <remarks>One millisecond is defined as 1/1000th of a second.</remarks>
    public static class Milliseconds
    {
        /// <summary>Number of milliseconds in one second.</summary>
        public const int PerSecond = 1000;

        /// <summary>Fractional number of milliseconds in one tick.</summary>
        public const double PerTick = 1.0D / Ticks.PerMillisecond;

        /// <summary>Converts milliseconds to 100-nanosecond tick intervals.</summary>
        public static long ToTicks(double milliseconds)
        {
            return (long)(milliseconds * Ticks.PerMillisecond);
        }

        /// <summary>Converts milliseconds to seconds.</summary>
        public static double ToSeconds(double milliseconds)
        {
            return milliseconds / Milliseconds.PerSecond;
        }

        /// <summary>Turns the given number of milliseconds into textual representation of years, days, hours, minutes and seconds.</summary>
        /// <param name="milliseconds">Milliseconds to be converted.</param>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        public static string ToText(double milliseconds, int secondPrecision)
        {
            return Seconds.ToText(Milliseconds.ToSeconds(milliseconds), secondPrecision);
        }
    }
}
