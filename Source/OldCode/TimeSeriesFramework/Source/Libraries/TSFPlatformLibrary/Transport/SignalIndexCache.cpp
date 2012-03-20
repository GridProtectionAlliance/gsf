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

#include <climits>
#include "SignalIndexCache.h"

namespace tsf = TimeSeriesFramework;

// Adds a measurement key to the cache.
void tsf::Transport::SignalIndexCache::AddMeasurementKey(
	unsigned short signalIndex,
	tsf::Guid signalID,
	std::string source,
	unsigned int id)
{
	int vectorIndex = m_signalIDList.size();

	m_reference[signalIndex] = vectorIndex;
	m_signalIDList.push_back(signalID);
	m_sourceList.push_back(source);
	m_idList.push_back(id);

	m_signalIDCache[signalID] = signalIndex;
}

// Empties the cache.
void tsf::Transport::SignalIndexCache::Clear()
{
	m_reference.clear();
	m_signalIDList.clear();
	m_sourceList.clear();
	m_idList.clear();

	m_signalIDCache.clear();
}

// Gets the globally unique signal ID associated with the given 16-bit runtime ID.
tsf::Guid tsf::Transport::SignalIndexCache::GetSignalID(unsigned short signalIndex) const
{
	int vectorIndex = m_reference[signalIndex];
	return m_signalIDList[vectorIndex];
}

// Gets the first half of the human-readable measurement
// key associated with the given 16-bit runtime ID.
std::string tsf::Transport::SignalIndexCache::GetSource(unsigned short signalIndex) const
{
	int vectorIndex = m_reference[signalIndex];
	return m_sourceList[vectorIndex];
}

// Gets the second half of the human-readable measurement
// key associated with the given 16-bit runtime ID.
unsigned int tsf::Transport::SignalIndexCache::GetID(unsigned short signalIndex) const
{
	int vectorIndex = m_reference[signalIndex];
	return m_idList[vectorIndex];
}

// Gets the globally unique signal ID as well as the human-readable
// measurement key associated with the given 16-bit runtime ID.
void tsf::Transport::SignalIndexCache::GetMeasurementKey(
	unsigned short signalIndex,
	tsf::Guid& signalID,
	std::string& source,
	unsigned int& id) const
{
	int vectorIndex = m_reference[signalIndex];

	signalID = m_signalIDList[vectorIndex];
	source = m_sourceList[vectorIndex];
	id = m_idList[vectorIndex];
}

// Gets the 16-bit runtime ID associated with the given globally unique signal ID.
unsigned short TimeSeriesFramework::Transport::SignalIndexCache::GetSignalIndex(tsf::Guid signalID)
{
	std::map<tsf::Guid, unsigned short>::iterator it;
	unsigned short signalIndex = USHRT_MAX;

	it = m_signalIDCache.find(signalID);

	if(it != m_signalIDCache.end())
		signalIndex = it->second;

	return signalIndex;
}