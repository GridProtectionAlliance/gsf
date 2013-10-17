//******************************************************************************************************
//  Masks.java - Gbtc
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

/**
 * Utility class to properly generate masks for different sizes of integers.
 * 
 * @author Stephen C. Wills
 */
public class Masks
{
	/**
	 * Gets a 64-bit integer mask with the given
	 * number of least significant bits turned on.
	 * 
	 * @param numberOfOnBits The number of bits to be turned on.
	 * @return a 64-bit integer mask
	 */
	public static long getLongMask(int numberOfOnBits)
	{
		if(numberOfOnBits < 0 || numberOfOnBits > Long.SIZE)
			throw new IllegalArgumentException("Number of masked bits must be between 0 and " + Long.SIZE);
		
		return ~(~0L << numberOfOnBits);
	}

	/**
	 * Gets a 32-bit integer mask with the given
	 * number of least significant bits turned on.
	 * 
	 * @param numberOfOnBits The number of bits to be turned on.
	 * @return a 32-bit integer mask
	 */
	public static int getIntMask(int numberOfOnBits)
	{
		if(numberOfOnBits < 0 || numberOfOnBits > Integer.SIZE)
			throw new IllegalArgumentException("Number of masked bits must be between 0 and " + Integer.SIZE);
		
		return ~(~0 << numberOfOnBits);
	}

	/**
	 * Gets a 16-bit integer mask with the given
	 * number of least significant bits turned on.
	 * 
	 * @param numberOfOnBits The number of bits to be turned on.
	 * @return a 16-bit integer mask
	 */
	public static short getShortMask(int numberOfOnBits)
	{
		if(numberOfOnBits < 0 || numberOfOnBits > Short.SIZE)
			throw new IllegalArgumentException("Number of masked bits must be between 0 and " + Short.SIZE);
		
		return (short)getIntMask(numberOfOnBits);
	}

	/**
	 * Gets a 8-bit integer mask with the given
	 * number of least significant bits turned on.
	 * 
	 * @param numberOfOnBits The number of bits to be turned on.
	 * @return a 8-bit integer mask
	 */
	public static byte getByteMask(int numberOfOnBits)
	{
		if(numberOfOnBits < 0 || numberOfOnBits > Byte.SIZE)
			throw new IllegalArgumentException("Number of masked bits must be between 0 and " + Byte.SIZE);
		
		return (byte)getIntMask(numberOfOnBits);
	}
}
