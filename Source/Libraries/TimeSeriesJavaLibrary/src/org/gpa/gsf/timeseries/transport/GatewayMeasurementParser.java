//******************************************************************************************************
//  GatewayMeasurementParser.java - Gbtc
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

package org.gpa.gsf.timeseries.transport;

import java.nio.ByteBuffer;
import java.util.Collection;

import org.gpa.gsf.timeseries.Measurement;

/**
 * Represents the interface used by the {@link DataSubscriber} to parse
 * measurements from the publisher.
 */
public interface GatewayMeasurementParser
{
	/**
	 * Parses a collection of serialized measurements from a byte array.
	 * 
	 * @param serializedMeasurements the serialized measurements to be parsed
	 * @param offset the offset into the array at which the collection starts
	 * @param length the byte length of the collection of serialized measurements
	 * @return the collection of parsed measurements
	 */
	Collection<Measurement> parseMeasurements(byte[] serializedMeasurements, int offset, int length);
	
	/**
	 * Parses a collection of serialized measurements from a byte buffer.
	 * 
	 * @param serializedMeasurementBuffer The serialized measurements to be parsed.
	 *        This method uses relative get operations on the buffer until there is
	 *        no longer enough data left to parse any more measurements.
	 * @return the collection of parsed measurements
	 */
	Collection<Measurement> parseMeasurements(ByteBuffer serializedMeasurementBuffer);
}
