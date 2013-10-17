//******************************************************************************************************
//  ArrayExtensions.java - Gbtc
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
 * Utility class that provides additional functionality for arrays.
 */
public class ArrayExtensions
{
	/**
	 * Copies the specified number of bytes from the source array to the
	 * destination array, starting from the given offsets in each array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(byte[] source, int srcOffset, byte[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of short integers from the source array
	 * to the destination array, starting from the given offsets in each
	 * array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(short[] source, int srcOffset, short[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of integers from the source array to the
	 * destination array, starting from the given offsets in each array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(int[] source, int srcOffset, int[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of long integers from the source array
	 * to the destination array, starting from the given offsets in each
	 * array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(long[] source, int srcOffset, long[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of floats from the source array to the
	 * destination array, starting from the given offsets in each array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(float[] source, int srcOffset, float[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of doubles from the source array to the
	 * destination array, starting from the given offsets in each array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(double[] source, int srcOffset, double[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of boolean values from the source array
	 * to the destination array, starting from the given offsets in each
	 * array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(boolean[] source, int srcOffset, boolean[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of characters from the source array to the
	 * destination array, starting from the given offsets in each array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static void blockCopy(char[] source, int srcOffset, char[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}

	/**
	 * Copies the specified number of objects from the source array to the
	 * destination array, starting from the given offsets in each array.
	 * 
	 * @param source the source array
	 * @param srcOffset the offset into the source array where the data
	 *        to be copied starts
	 * @param dest the destination array
	 * @param destOffset the offset into the destination array where the
	 *        data is to be copied
	 * @param length the amount of data to be copied
	 */
	public static <T> void blockCopy(T[] source, int srcOffset, T[] dest, int destOffset, int length)
	{
		for (int i = 0; i < length; i++)
			dest[destOffset + i] = source[srcOffset + i];
	}
}
