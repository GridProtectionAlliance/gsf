//******************************************************************************************************
//  InputStreamExtensions.java - Gbtc
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
//  04/16/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.util;

import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;

/**
 * Utility class that provides additional functionality for input streams.
 * 
 * @author Stephen C. Wills
 */
public class InputStreamExtensions
{
	/**
	 * This read method works approximately the same as the
	 * {@link BufferedInputStream#read(byte[], int, int)} method.
	 * The main difference is that this method will block until the
	 * requested amount of data is read from the input stream or the
	 * end of the stream has been reached, whereas BufferedInputStream
	 * will try to avoid blocking by checking the {@code available()}
	 * method of the underlying stream.
	 * 
	 * @param stream source of the data
	 * @param buffer destination buffer
	 * @param offset offset at which to start storing bytes
	 * @param length requested number of bytes
	 * @return the number of bytes read from the stream, or -1 if the end of the stream has been reached
	 * @throws IOException if this input stream has been closed by invoking its
	 *         {@link InputStream#close()} method, or an I/O error occurs
	 * @see java.io.BufferedInputStream
	 */
	public static int read(InputStream stream, byte[] buffer, int offset, int length) throws IOException
	{
		int index = offset;
		int readAmount = 0;
		int totalReadAmount = 0;
		
		while (readAmount >= 0 && totalReadAmount < length)
		{
			readAmount = stream.read(buffer, index, length - totalReadAmount);
			index += readAmount;
			totalReadAmount += readAmount;
		}
		
		if (readAmount < 0 && totalReadAmount == 0)
			totalReadAmount = -1;
		
		return totalReadAmount;
	}
}
