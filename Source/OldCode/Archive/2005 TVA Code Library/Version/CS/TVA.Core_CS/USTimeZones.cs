//*******************************************************************************************************
//  TVA.USTimeZones.cs - Defines a few common US timezones.
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  This is the location for handy miscellaneous functions that are difficult to categorize elsewhere
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/12/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    public static class USTimeZones
    {
        // We define a few common timezones for convenience.
        private static TimeZoneInfo m_easternTimeZone;
        private static TimeZoneInfo m_centralTimeZone;
        private static TimeZoneInfo m_mountainTimeZone;
        private static TimeZoneInfo m_pacificTimeZone;

        /// <summary>Gets Eastern Time Zone.</summary>
        public static TimeZoneInfo EasternTimeZone
        {
            get
            {
                if (m_easternTimeZone == null) m_easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return m_easternTimeZone;
            }
        }

        /// <summary>Gets Central Time Zone.</summary>
        public static TimeZoneInfo CentralTimeZone
        {
            get
            {
                if (m_centralTimeZone == null) m_centralTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return m_centralTimeZone;
            }
        }

        /// <summary>Gets Mountain Time Zone.</summary>
        public static TimeZoneInfo MountainTimeZone
        {
            get
            {
                if (m_mountainTimeZone == null) m_mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                return m_mountainTimeZone;
            }
        }

        /// <summary>Gets Pacific Standard Time Zone.</summary>
        public static TimeZoneInfo PacificTimeZone
        {
            get
            {
                if (m_pacificTimeZone == null) m_pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                return m_pacificTimeZone;
            }
        }
    }
}