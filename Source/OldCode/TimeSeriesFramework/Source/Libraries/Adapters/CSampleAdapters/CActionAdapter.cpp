//******************************************************************************************************
//  CActionAdapter.cpp - Gbtc
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
#include "CActionAdapter.h"

using namespace CSampleAdapters;

#pragma region [ Property Implementations ]

// Gets the flag indicating if this CActionAdapter supports temporal processing.
bool CActionAdapter::SupportsTemporalProcessing::get()
{
    return false;
}

#pragma endregion

#pragma region [ Method Implementations ]

// Initializes the CActionAdapter.
void CActionAdapter::Initialize()
{
    __super::Initialize();
}

// Gets a short one-line status of this CActionAdapter.
String^ CActionAdapter::GetShortStatus(int maxLength)
{
    return "C++.NET action adapter happily exists...";
}

// Publish frame of time-aligned collection of measurement values that arrived within the concentrator's defined lag-time.
void CActionAdapter::PublishFrame(IFrame^ frame, int index)
{
}

#pragma endregion