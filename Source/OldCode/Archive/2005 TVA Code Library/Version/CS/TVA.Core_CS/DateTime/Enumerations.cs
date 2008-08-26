using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  Enumerations.vb - Global enumerations for this namespace
//  Copyright © 2005 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/11/2007 - J. Ritchie Carroll
//       Moved all namespace level enumerations into "Enumerations.vb" file
//  08/15/2007 - J. Ritchie Carroll
//       Added BaselineTimeInterval enumeration to support multiple time intervals
//       in the BaselinedTimestamp function.
//  08/31/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************

namespace TVA
{
	namespace DateTime
	{
		
		/// <summary>Time names enumeration used by SecondsToText function.</summary>
		public enum TimeName
		{
			Year,
			Years,
			Day,
			Days,
			Hour,
			Hours,
			Minute,
			Minutes,
			Second,
			Seconds,
			LessThan60Seconds,
			NoSeconds
		}
		
		/// <summary>Time zone names enumeration used to look up desired time zone in GetWin32TimeZone function.</summary>
		public enum TimeZoneName
		{
			DaylightName,
			DaylightAbbreviation,
			DisplayName,
			StandardName,
			StandardAbbreviation
		}
		
		/// <summary>Time intervals enumeration used by BaselinedTimestamp function.</summary>
		public enum BaselineTimeInterval
		{
			/// <summary>Baseline timestamp to the second (i.e., starting at zero milliseconds).</summary>
			Second,
			/// <summary>Baseline timestamp to the minute (i.e., starting at zero seconds and milliseconds).</summary>
			Minute,
			/// <summary>Baseline timestamp to the hour (i.e., starting at zero minutes, seconds and milliseconds).</summary>
			Hour,
			/// <summary>Baseline timestamp to the day (i.e., starting at zero hours, minutes, seconds and
			/// milliseconds).</summary>
			Day,
			/// <summary>Baseline timestamp to the month (i.e., starting at day one, zero hours, minutes, seconds
			/// and milliseconds).</summary>
			Month,
			/// <summary>Baseline timestamp to the year (i.e., starting at month one, day one, zero hours, minutes,
			/// seconds and milliseconds).</summary>
			Year
		}
		
		/// <summary>
		/// The various parts of System.DateTime type.
		/// </summary>
		public enum DateTimePart
		{
			/// <summary>
			/// Minute part.
			/// </summary>
			Minute,
			/// <summary>
			/// Hour part.
			/// </summary>
			Hour,
			/// <summary>
			/// Day part.
			/// </summary>
			Day,
			/// <summary>
			/// Month part.
			/// </summary>
			Month,
			/// <summary>
			/// Day of week part.
			/// </summary>
			DayOfWeek
		}
		
	}
}
