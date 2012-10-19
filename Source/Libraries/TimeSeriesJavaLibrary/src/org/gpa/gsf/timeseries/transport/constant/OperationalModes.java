//******************************************************************************************************
//  OperationalModes.java - Gbtc
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
 * Defines flags and bitmasks used in the operational modes
 * that define how the subscriber and publisher communicate.
 * Operational modes are the first things that get transmitted
 * to the publisher after a successful connection.
 */
public class OperationalModes
{
	/**
	 * Bitmask to extract the protocol version from the operational modes.
	 */
	public static final int VersionMask = 0x0000001F;
	
	/**
	 * Bitmask to extract the compression mode from the operational modes.
	 * Compression is currently not supported; these bits are reserved.
	 */
	public static final int CompressionModeMask = 0x000000E0;
	
	/**
	 * Bitmask to extract the operational encoding from the operational modes.
	 * 
	 * @see OperationalEncoding
	 */
	public static final int EncodingMask = 0x00000300;
	
	/**
	 * Flag that determines whether to use the common serialization format.
	 * If the bit is set, the common serialization format is used. If the
	 * flag is unset, the .NET serialization format is used. This Java
	 * implementation of the Subscriber API only supports the common
	 * serialization format.
	 */
	public static final int UseCommonSerializationFormat = 0x01000000;
	
	/**
	 * Flag that determines whether to compress the signal index cache.
	 * This Java implementation of the Subscriber API does not yet support
	 * compression of the signal index cache.
	 */
	public static final int CompressSignalIndexCache = 0x40000000;
	
	/**
	 * Flag that determines whether to compress the metadata transmitted
	 * from the publisher to the subscriber.
	 */
	public static final int CompressMetadata = 0x80000000;
	
	/**
	 * No flags.
	 */
	public static final int NoFlags = 0x00000000;
}
