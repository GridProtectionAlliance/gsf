//******************************************************************************************************
//  ServerResponse.java - Gbtc
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
 * Defines response codes that can be sent by the publisher back to the
 * subscriber.
 */
public class ServerResponse
{
	/**
	 * Indicates that a command sent by the subscriber has been successfully
	 * processed and handled.
	 */
	public static final byte Succeeded = (byte) 0x80;

	/**
	 * Indicates that the publisher was unable to properly process and handle a
	 * command sent by the publisher.
	 */
	public static final byte Failed = (byte) 0x81;

	/**
	 * Indicates that the packet sent with this code contains measurement data.
	 */
	public static final byte DataPacket = (byte) 0x82;

	/**
	 * Indicates that the packet sent with this code contains the serialized
	 * signal index cache.
	 */
	public static final byte UpdateSignalIndexCache = (byte) 0x83;

	/**
	 * Indicates that the packet sent with this code contains the serialized
	 * base time offsets.
	 */
	public static final byte UpdateBaseTimes = (byte) 0x84;

	/**
	 * Indicates that the packet sent with this code contains the cipher keys
	 * used for encryption.
	 */
	public static final byte UpdateCipherKeys = (byte) 0x85;

	/**
	 * Indicates that the publisher has started sending measurement data, and
	 * the message includes the server's local time taken when the data stream
	 * was started.
	 */
	public static final byte DataStartTime = (byte) 0x86;

	/**
	 * Indicates that a temporal session has been completed.
	 */
	public static final byte ProcessingComplete = (byte) 0x87;
	
	/**
	 * Indicates that no action should be taken by the subscriber.
	 * This message is sent periodically by the publisher over the
	 * command channel in order to verify that the connection is
	 * still open.
	 */
	public static final byte NoOP = (byte) 0xFF;
}
