//*******************************************************************************************************
//  USTimeZones.cs
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
//  09/12/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>Defines a few common US timezones.</summary>
    public static class USTimeZones
    {
        // We define a few common timezones for convenience.
        private static TimeZoneInfo m_easternTimeZone;
        private static TimeZoneInfo m_centralTimeZone;
        private static TimeZoneInfo m_mountainTimeZone;
        private static TimeZoneInfo m_pacificTimeZone;

        /// <summary>Gets Eastern Time Zone.</summary>
        public static TimeZoneInfo Eastern
        {
            get
            {
                if (m_easternTimeZone == null) m_easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return m_easternTimeZone;
            }
        }

        /// <summary>Gets Central Time Zone.</summary>
        public static TimeZoneInfo Central
        {
            get
            {
                if (m_centralTimeZone == null) m_centralTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return m_centralTimeZone;
            }
        }

        /// <summary>Gets Mountain Time Zone.</summary>
        public static TimeZoneInfo Mountain
        {
            get
            {
                if (m_mountainTimeZone == null) m_mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                return m_mountainTimeZone;
            }
        }

        /// <summary>Gets Pacific Standard Time Zone.</summary>
        public static TimeZoneInfo Pacific
        {
            get
            {
                if (m_pacificTimeZone == null) m_pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                return m_pacificTimeZone;
            }
        }
    }
}