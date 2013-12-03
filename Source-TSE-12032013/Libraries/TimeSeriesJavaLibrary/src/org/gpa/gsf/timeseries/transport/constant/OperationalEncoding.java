//******************************************************************************************************
//  OperationalEncoding.java - Gbtc
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

package org.gpa.gsf.timeseries.transport.constant;

/**
 * Defines character encodings supported by the Gateway Exchange Protocol.
 */
public class OperationalEncoding
{
	/**
	 * Little-endian UTF-16.
	 */
	public static final int Unicode = 0x00000000;
	
	/**
	 * Big-endian UTF-16.
	 */
	public static final int BigEndianUnicode = 0x00000100;
	
	/**
	 * UTF-8
	 */
	public static final int UTF8 = 0x00000200;
	
	/**
	 * The default character encoding used by the operating system.
	 * This option may not work properly if the two systems that
	 * are communicating are not using the same operating system default.
	 * On Windows systems, this will often be compatible with the ANSI
	 * option defined in the C# implementation of the Publisher API.
	 */
	public static final int OperatingSystemDefault = 0x00000300;
}
