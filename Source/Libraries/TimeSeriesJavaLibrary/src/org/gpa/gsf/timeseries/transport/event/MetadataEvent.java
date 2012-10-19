//******************************************************************************************************
//  MetadataEvent.java - Gbtc
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

package org.gpa.gsf.timeseries.transport.event;

import java.util.EventObject;

/**
 * Event that occurs when the subscriber receives metadata from the publisher.
 */
public class MetadataEvent extends EventObject
{
	private byte[] m_compressedMetadata;
	private String m_xmlMetadata;

	/**
	 * Constructs a new instance.
	 * 
	 * @param source the source of the event
	 * @param compressedMetadata the compressed metadata
	 */
	public MetadataEvent(Object source, byte[] compressedMetadata)
	{
		super(source);
		
		if (compressedMetadata == null)
			throw new IllegalArgumentException("compressedMetadata cannot be null");
		
		m_compressedMetadata = compressedMetadata;
	}
	
	/**
	 * Constructs a new instance.
	 * 
	 * @param source the source of the event
	 * @param xmlMetadata the uncompressed metadata
	 */
	public MetadataEvent(Object source, String xmlMetadata)
	{
		super(source);
		
		if (xmlMetadata == null)
			throw new IllegalArgumentException("xmlMetadata cannot be null");
		
		m_xmlMetadata = xmlMetadata;
	}

	/**
	 * Gets the compressed metadata received from the publisher.
	 * If the metadata received was uncompressed, this will return null.
	 * 
	 * @return byte array containing the compressed metadata
	 */
	public byte[] getCompressedMetadata()
	{
		return m_compressedMetadata;
	}

	/**
	 * Gets the uncompressed metadata received from the publisher.
	 * If the metadata received was compressed, this will return null.
	 * 
	 * @return the uncompressed XML metadata
	 */
	public String getXmlMetadata()
	{
		return m_xmlMetadata;
	}

	private static final long serialVersionUID = -1703909541078304245L;
}
