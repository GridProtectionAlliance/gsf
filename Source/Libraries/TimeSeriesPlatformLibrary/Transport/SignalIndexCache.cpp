//******************************************************************************************************
//  SignalIndexCache.cpp - Gbtc
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
//  03/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include "SignalIndexCache.h"

using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

// Adds a measurement key to the cache.
void SignalIndexCache::AddMeasurementKey(
	uint16_t signalIndex,
	Guid signalID,
	string source,
	uint32_t id)
{
	size_t vectorIndex = m_signalIDList.size();

	m_reference[signalIndex] = vectorIndex;
	m_signalIDList.push_back(signalID);
	m_sourceList.push_back(source);
	m_idList.push_back(id);

	m_signalIDCache[signalID] = signalIndex;
}

// Empties the cache.
void SignalIndexCache::Clear()
{
	m_reference.clear();
	m_signalIDList.clear();
	m_sourceList.clear();
	m_idList.clear();

	m_signalIDCache.clear();
}

// Determines whether an element with the given runtime ID exists in the signal index cache.
bool SignalIndexCache::Contains(uint16_t signalIndex) const
{
	return m_reference.find(signalIndex) != m_reference.end();
}

// Gets the globally unique signal ID associated with the given 16-bit runtime ID.
Guid SignalIndexCache::GetSignalID(uint16_t signalIndex) const
{
	size_t vectorIndex = m_reference.find(signalIndex)->second;
	return m_signalIDList[vectorIndex];
}

// Gets the first half of the human-readable measurement
// key associated with the given 16-bit runtime ID.
string SignalIndexCache::GetSource(uint16_t signalIndex) const
{
	size_t vectorIndex = m_reference.find(signalIndex)->second;
	return m_sourceList[vectorIndex];
}

// Gets the second half of the human-readable measurement
// key associated with the given 16-bit runtime ID.
uint32_t SignalIndexCache::GetID(uint16_t signalIndex) const
{
	size_t vectorIndex = m_reference.find(signalIndex)->second;
	return m_idList[vectorIndex];
}

// Gets the globally unique signal ID as well as the human-readable
// measurement key associated with the given 16-bit runtime ID.
void SignalIndexCache::GetMeasurementKey(
	uint16_t signalIndex,
	Guid& signalID,
	string& source,
	uint32_t& id) const
{
	size_t vectorIndex = m_reference.find(signalIndex)->second;

	signalID = m_signalIDList[vectorIndex];
	source = m_sourceList[vectorIndex];
	id = m_idList[vectorIndex];
}

// Gets the 16-bit runtime ID associated with the given globally unique signal ID.
uint16_t SignalIndexCache::GetSignalIndex(Guid signalID) const
{
	map<Guid, uint16_t>::const_iterator it;
	uint16_t signalIndex = 0xFFFF;

	it = m_signalIDCache.find(signalID);

	if(it != m_signalIDCache.end())
		signalIndex = it->second;

	return signalIndex;
}