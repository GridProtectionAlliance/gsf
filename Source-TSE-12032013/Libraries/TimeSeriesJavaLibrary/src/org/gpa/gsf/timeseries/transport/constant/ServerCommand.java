//******************************************************************************************************
//  ServerCommand.java - Gbtc
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
 * Defines command codes for the commands that can
 * be sent to the publisher via the command channel.
 */
public class ServerCommand
{
	/**
	 * Indicates that the subscriber would like to authenticate.
	 * <p>
	 * The data that accompanies this message is the encrypted
	 * authentication string prefixed with its byte length. The
	 * byte length should be unencrypted. The authentication
	 * string consists of the subscriber's authentication ID
	 * prefixed with a random, 8-byte salt.
	 */
	public static final byte Authenticate = 0x00;
	
	/**
	 * Indicates that the subscriber would like
	 * to receive metadata from the publisher.
	 */
	public static final byte MetadataRefresh = 0x01;
	
	/**
	 * Indicates that the subscriber would like
	 * to receive measurements from the publisher.
	 * <p>
	 * The data that accompanies this message is the connection string
	 * which defines several configuration options that can be asked
	 * of the publisher.
	 */
	public static final byte Subscribe = 0x02;
	
	/**
	 * Indicates that the subscriber would like to stop
	 * receiving measurements from the publisher.
	 */
	public static final byte Unsubscribe = 0x03;
	
	/**
	 * Indicates that the subscriber would
	 * like to initiate a cipher key rotation.
	 */
	public static final byte RotateCipherKeys = 0x04;
	
	/**
	 * Indicates that the subscriber would like
	 * to change its measurement processing interval.
	 */
	public static final byte UpdateProcessingInterval = 0x05;
	
	/**
	 * Indicates that the subscriber would like to define
	 * or change the way it is communicating with the publisher.
	 */
	public static final byte DefineOperationalModes = 0x06;
}
