//******************************************************************************************************
//  StringExtensions.java - Gbtc
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
//  04/18/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.util;

/**
 * Utility class that provides additional functionality for strings.
 */
public class StringExtensions
{
	/**
	 * Determines whether the given string is null or the empty string.
	 * 
	 * @param str the string to be checked
	 * @return true if the string is null or empty; false otherwise
	 */
	public static boolean isNullOrEmpty(String str)
	{
		return (str == null) || str.isEmpty();
	}
	
	/**
	 * Determines whether the given string is null or contains
	 * nothing but whitespace characters.
	 * 
	 * @param str the string to be checked
	 * @return true if the string is null or whitespace; false otherwise
	 */
	public static boolean isNullOrWhitespace(String str)
	{
		return (str == null) || str.trim().isEmpty();
	}
	
	/**
	 * Converts null strings to the empty string. If a string is provided
	 * which is not null, no operation is performed and the string is
	 * returned as-is.
	 * 
	 * @param str the string to be checked
	 * @return the empty string if the parameter is null; otherwise the
	 *         parameter itself is returned
	 */
	public static String toNonNull(String str)
	{
		return toNonNull(str, "");
	}
	
	/**
	 * Converts null strings to the given default string value. If a
	 * string is provided which is not null, no operation is performed
	 * and the string is returned as-is.
	 *  
	 * @param str the string to be checked
	 * @param defaultValue the default value to return if the provided
	 *        string happens to be null
	 * @return the default value if the first parameter is null; otherwise
	 * 	       the first parameter itself is returned
	 */
	public static String toNonNull(String str, String defaultValue)
	{
		if (str != null)
			return str;
		else if (defaultValue != null)
			return defaultValue;
		else
			return "";
	}
}
