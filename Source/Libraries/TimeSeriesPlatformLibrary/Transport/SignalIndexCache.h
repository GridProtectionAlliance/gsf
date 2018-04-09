//******************************************************************************************************
//  SignalIndexCache.h - Gbtc
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
//  03/08/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __SIGNAL_INDEX_CACHE_H
#define __SIGNAL_INDEX_CACHE_H

#include <map>
#include <vector>
#include <string>

#include "../Common/Types.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    // Maps 16-bit runtime IDs to 128-bit globally unique IDs.
    // Additionally provides reverse lookup and an extra mapping
    // to human-readable measurement keys.
    class SignalIndexCache
    {
    private:
        map<uint16_t, size_t> m_reference;
        vector<Guid> m_signalIDList;
        vector<string> m_sourceList;
        vector<uint32_t> m_idList;

        map<Guid, uint16_t> m_signalIDCache;

    public:
        // Adds a measurement key to the cache.
        void AddMeasurementKey(
            uint16_t signalIndex,
            Guid signalID,
            string source,
            uint32_t id);

        // Empties the cache.
        void Clear();

        // Determines whether an element with the given runtime ID exists in the signal index cache.
        bool Contains(uint16_t signalIndex) const;

        // Gets the globally unique signal ID associated with the given 16-bit runtime ID.
        Guid GetSignalID(uint16_t signalIndex) const;

        // Gets the first half of the human-readable measurement
        // key associated with the given 16-bit runtime ID.
        const string& GetSource(uint16_t signalIndex) const;

        // Gets the second half of the human-readable measurement
        // key associated with the given 16-bit runtime ID.
        uint32_t GetID(uint16_t signalIndex) const;

        // Gets the globally unique signal ID as well as the human-readable
        // measurement key associated with the given 16-bit runtime ID.
        void GetMeasurementKey(
            uint16_t signalIndex,
            Guid& signalID,
            string& source,
            uint32_t& id) const;

        // Gets the 16-bit runtime ID associated with the given globally unique signal ID.
        uint16_t GetSignalIndex(Guid signalID) const;
    };
}}}

#endif