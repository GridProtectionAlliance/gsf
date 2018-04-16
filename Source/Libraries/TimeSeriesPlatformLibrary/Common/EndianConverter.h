//******************************************************************************************************
//  EndianConverter.h - Gbtc
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
//  03/19/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __ENDIAN_CONVERTER_H
#define __ENDIAN_CONVERTER_H

#include <cstddef>

#include "CommonTypes.h"

namespace GSF {
namespace TimeSeries
{
    // Converts values between the system's native byte
    // order and the big and little endian byte orders.
    class EndianConverter
    {
    private:
        int m_nativeOrder;

        // Swaps the bytes in a character array.
        // Used for conversion between different byte orders.
        static void ByteSwap(uint8_t* value, size_t length);

    public:
        // Creates a new instance.
        EndianConverter();

        // Converts between big endian and
        // the system's native byte order.
        template <class T>
        T ConvertBigEndian(T value) const;

        // Converts between little endian and
        // the system's native byte order.
        template <class T>
        T ConvertLittleEndian(T value) const;

        // Returns an integer value indicating which byte order is
        // the native byte order used by the system. The value is
        // defined by the BigEndian and LittleEndian class constants.
        int NativeOrder() const;

        // Class constant defining the big endian byte order.
        static const int BigEndian = 0;

        // Class constant defining the little endian byte order.
        static const int LittleEndian = 1;
    };

    // Converts between big endian and
    // the system's native byte order.
    template <class T>
    T EndianConverter::ConvertBigEndian(T value) const
    {
        if (m_nativeOrder != EndianConverter::BigEndian)
            ByteSwap(reinterpret_cast<uint8_t*>(&value), sizeof(T));

        return value;
    }

    // Converts between little endian and
    // the system's native byte order.
    template <class T>
    T EndianConverter::ConvertLittleEndian(T value) const
    {
        if (m_nativeOrder != EndianConverter::LittleEndian)
            ByteSwap(reinterpret_cast<uint8_t*>(&value), sizeof(T));

        return value;
    }
}}

#endif