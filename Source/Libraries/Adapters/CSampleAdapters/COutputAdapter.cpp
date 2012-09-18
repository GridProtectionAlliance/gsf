//******************************************************************************************************
//  COutputAdapter.cpp - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  01/27/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "stdafx.h"
#include "COutputAdapter.h"

using namespace CSampleAdapters;

#pragma region [ Property Implementations ]

// Returns a flag that determines if measurements sent to this COutputAdapter are destined for archival.
bool COutputAdapter::OutputIsForArchive::get()
{
    return false;
}

// Gets the flag indicating if this COutputAdapter supports temporal processing.
bool COutputAdapter::SupportsTemporalProcessing::get()
{
    return false;
}

// Gets flag that determines if this COutputAdapter uses an asynchronous connection.
bool COutputAdapter::UseAsyncConnect::get()
{
    return false;
}

#pragma endregion

#pragma region [ Method Implementations ]

// Initializes the COutputAdapter.
void COutputAdapter::Initialize()
{
    __super::Initialize();
}

// Gets a short one-line status of this COutputAdapter.
String^ COutputAdapter::GetShortStatus(int maxLength)
{
    return "C++.NET output adapter happily exists...";
}

// Attempts to connect this COutputAdapter.
void COutputAdapter::AttemptConnection()
{
}

// Attempts to disconnect this COutputAdapter.
void COutputAdapter::AttemptDisconnection()
{
}

// Serializes measurements to data output stream.
void COutputAdapter::ProcessMeasurements(array<IMeasurement^>^ measurements)
{
}

#pragma endregion
