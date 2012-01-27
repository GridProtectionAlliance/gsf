//******************************************************************************************************
//  CInputAdapter.cpp - Gbtc
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
//  01/26/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "stdafx.h"
#include "CInputAdapter.h"

using namespace CSampleAdapters;

#pragma region [ Property Implementations ]

// Gets the flag indicating if this CInputAdapter supports temporal processing.
bool CInputAdapter::SupportsTemporalProcessing::get()
{
    return false;
}

// Gets flag that determines if this CInputAdapter uses an asynchronous connection.
bool CInputAdapter::UseAsyncConnect::get()
{
    return false;
}

#pragma endregion

#pragma region [ Method Implementations ]

// Initializes the CInputAdapter.
void CInputAdapter::Initialize()
{
    __super::Initialize();
}

// Gets a short one-line status of this CInputAdapter.
String^ CInputAdapter::GetShortStatus(int maxLength)
{
    return "C++.NET input adapter happily exists...";
}

// Attempts to connect this CInputAdapter.
void CInputAdapter::AttemptConnection()
{
}

// Attempts to disconnect this CInputAdapter.
void CInputAdapter::AttemptDisconnection()
{
}

#pragma endregion
