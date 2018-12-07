//******************************************************************************************************
//  EndianConverter.cpp - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/19/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include "EndianConverter.h"

using namespace GSF::TimeSeries;

// Creates a new instance of the EndianConverter.
EndianConverter::EndianConverter()
{
    const union
    {
        uint32_t num;
        uint8_t bytes[4];
    }
    endianTest = { 0x00000001 };

    if (endianTest.bytes[0] == 1)
        m_nativeOrder = EndianConverter::LittleEndian;
    else
        m_nativeOrder = EndianConverter::BigEndian;
}

// Swaps the bytes in a character array.
// Used for conversion between different byte orders.
void EndianConverter::ByteSwap(uint8_t* value, uint32_t length)
{
    uint8_t *start, *end;
    uint8_t temp;

    for (start = value, end = value + length - 1; start < end; ++start, --end)
    {
        temp = *start;
        *start = *end;
        *end = temp;
    }
}

int EndianConverter::NativeOrder() const
{
    return m_nativeOrder;
}