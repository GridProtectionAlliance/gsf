//******************************************************************************************************
//  TimeExtensions.java - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  04/23/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.util;

import java.util.Calendar;

/**
 * Utility class which provides additional
 * functionality for time representations.
 */
public class TimeExtensions
{
	/**
	 * Converts a value in ticks (100-nanosecond intervals since midnight,
	 * January 1, 0001) into a {@link Calendar} instance. Some loss of
	 * precision is incurred as {@code Calendar}s store up to millisecond
	 * precision.
	 * 
	 * @param ticks the value in ticks to be converted
	 * @return the equivalent {@code Calendar} instance
	 * @see #toTicks(Calendar)
	 */
	public static Calendar fromTicks(long ticks)
	{
		Calendar calendar = Calendar.getInstance();
		calendar.setTimeInMillis((ticks - TicksAtEpoch) / TicksPerMillisecond);
		return calendar;
	}
	
	/**
	 * Converts a {@link Calendar} instance into a value in ticks.
	 * 
	 * @param calendar the {@code Calendar} instance to be converted
	 * @return the equivalent value in ticks
	 * @see #fromTicks(long)
	 */
	public static long toTicks(Calendar calendar)
	{
		return (calendar.getTimeInMillis() * TicksPerMillisecond) + TicksAtEpoch;
	}
	
	private static final long TicksAtEpoch = 621355968000000000L;
	private static final long TicksPerMillisecond = 10000L;
}
