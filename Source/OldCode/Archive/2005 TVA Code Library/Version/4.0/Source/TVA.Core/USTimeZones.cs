//*******************************************************************************************************
//  USTimeZones.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/12/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Defines a few common United States time zones.
    /// </summary>
    public static class USTimeZones
    {
        // We define a few common timezones for convenience.
        private static TimeZoneInfo m_atlanticTimeZone;
        private static TimeZoneInfo m_easternTimeZone;
        private static TimeZoneInfo m_centralTimeZone;
        private static TimeZoneInfo m_mountainTimeZone;
        private static TimeZoneInfo m_pacificTimeZone;
        private static TimeZoneInfo m_alaskanTimeZone;
        private static TimeZoneInfo m_hawaiianTimeZone;
        private static TimeZoneInfo m_westPacificTimeZone;
        private static TimeZoneInfo m_samoaTimeZone;

        /// <summary>
        /// Gets the Atlantic Time Zone.
        /// </summary>
        /// <remarks>This time zone is used by the Commonwealth of Puerto Rico and the United States Virgin Islands.</remarks>
        public static TimeZoneInfo Atlantic
        {
            get
            {
                if (m_atlanticTimeZone == null) m_atlanticTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
                return m_atlanticTimeZone;
            }
        }

        /// <summary>
        /// Gets the Eastern Time Zone.
        /// </summary>
        public static TimeZoneInfo Eastern
        {
            get
            {
                if (m_easternTimeZone == null) m_easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return m_easternTimeZone;
            }
        }

        /// <summary>
        /// Gets the Central Time Zone.
        /// </summary>
        public static TimeZoneInfo Central
        {
            get
            {
                if (m_centralTimeZone == null) m_centralTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return m_centralTimeZone;
            }
        }

        /// <summary>
        /// Gets the Mountain Time Zone.
        /// </summary>
        public static TimeZoneInfo Mountain
        {
            get
            {
                if (m_mountainTimeZone == null) m_mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                return m_mountainTimeZone;
            }
        }

        /// <summary>
        /// Gets the Pacific Standard Time Zone.
        /// </summary>
        public static TimeZoneInfo Pacific
        {
            get
            {
                if (m_pacificTimeZone == null) m_pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                return m_pacificTimeZone;
            }
        }

        /// <summary>
        /// Gets the Alaskan Standard Time Zone.
        /// </summary>
        public static TimeZoneInfo Alaskan
        {
            get
            {
                if (m_alaskanTimeZone == null) m_alaskanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time");
                return m_alaskanTimeZone;
            }
        }

        /// <summary>
        /// Gets the Hawaiian Standard Time Zone.
        /// </summary>
        public static TimeZoneInfo Hawaiian
        {
            get
            {
                if (m_hawaiianTimeZone == null) m_hawaiianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
                return m_hawaiianTimeZone;
            }
        }

        /// <summary>
        /// Gets the West Pacific Standard Time Zone.
        /// </summary>
        /// <remarks>
        /// <para>This time zone is used by Guam and the Commonwealth of the Northern Mariana Islands.</para>
        /// <para>This is also known as the Chamorro time zone.</para>
        /// </remarks>
        public static TimeZoneInfo WestPacific
        {
            get
            {
                if (m_westPacificTimeZone == null) m_westPacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("West Pacific Standard Time");
                return m_westPacificTimeZone;
            }
        }

        /// <summary>
        /// Gets the Samoa Standard Time Zone.
        /// </summary>
        /// <remarks>This time zone is used by the American Samoa.</remarks>
        public static TimeZoneInfo Samoa
        {
            get
            {
                if (m_samoaTimeZone == null) m_samoaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time");
                return m_samoaTimeZone;
            }
        }
    }
}