//******************************************************************************************************
//  NumberExtensions.java - Gbtc
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
//  04/12/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.util;

import java.math.BigInteger;

/**
 * Utility class that provides additional functionality for numbers.
 * 
 * @author Stephen C. Wills
 */
public class NumberExtensions
{
	/**
	 * Gets a string representation of a long integer
	 * in base 10 as if it were an unsigned long integer.
	 * 
	 * @param num the long integer to be converted to a string
	 * @return a string representation of the given long integer
	 * @see Long#toString(long)
	 */
	public static String toUnsignedString(long num)
	{
		return toUnsignedString(num, 10);
	}
	
	/**
	 * Gets a string representation of a long integer
	 * as if it were an unsigned long integer.
	 * 
	 * @param num the long integer to be converted to a string
	 * @param radix the radix to use in the string representation
	 * @return a string representation of the given long integer
	 * @see Long#toString(long, int)
	 */
	public static String toUnsignedString(long num, int radix)
	{
		BigInteger bigNum;
		BigInteger bigMod;
		
		if (num > 0L)
			return Long.toString(radix);

		bigNum = BigInteger.valueOf(num >>> 1);
		bigMod = BigInteger.valueOf(num & 1L);
		
		return bigNum.shiftLeft(1).add(bigMod).toString(radix);
	}
	
	/**
	 * Gets a string representation of an integer in
	 * base 10 as if it were an unsigned integer.
	 * 
	 * @param num the integer to be converted to a string
	 * @return a string representation of the given integer
	 * @see Integer#toString(int)
	 */
	public static String toUnsignedString(int num)
	{
		return toUnsignedString(num, 10);
	}
	
	/**
	 * Gets a string representation of an integer
	 * as if it were an unsigned integer.
	 * 
	 * @param num the integer to be converted to a string
	 * @param radix the radix to use in the string representation
	 * @return a string representation of the given integer
	 * @see Integer#toString(int, int)
	 */
	public static String toUnsignedString(int num, int radix)
	{
		if (num > 0)
			return Integer.toString(num, radix);
		
		return Long.toString(num & Masks.getLongMask(Integer.SIZE), radix);
	}
	
	/**
	 * Gets a string representation of a short integer
	 * in base 10 as if it were an unsigned short integer.
	 * 
	 * @param num the short integer to be converted to a string
	 * @return a string representation of the given short integer
	 * @see Short#toString(short)
	 */
	public static String toUnsignedString(short num)
	{
		if (num > 0)
			return Short.toString(num);
		
		return Integer.toString(num & Masks.getIntMask(Short.SIZE));
	}
	
	/**
	 * Gets a string representation of a byte in
	 * base 10 as if it were an unsigned byte.
	 * 
	 * @param num the byte to be converted to a string
	 * @return a string representation of the given byte
	 * @see Byte#toString(byte)
	 */
	public static String toUnsignedString(byte num)
	{
		if (num > 0)
			return Byte.toString(num);
		
		return Integer.toString(num & Masks.getIntMask(Byte.SIZE));
	}
}
