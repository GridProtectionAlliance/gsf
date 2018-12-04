//******************************************************************************************************
//  DataRow.cpp - Gbtc
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
//  11/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "DataRow.h"
#include "DataTable.h"

using namespace std;
using namespace GSF::TimeSeries;
using namespace GSF::DataSet;

DataRowException::DataRowException(string message) noexcept :
    m_message(move(message))
{
}

const char* DataRowException::what() const noexcept
{
    return m_message.c_str();
}

const DataRowPtr DataRow::NullPtr = nullptr;

DataRow::DataRow(const DataTablePtr& parent) :
    m_parent(parent),
    m_values(parent->ColumnCount())
{
    for (uint32_t i = 0; i < m_values.size(); i++)
        m_values[i] = nullptr;
}

DataRow::~DataRow()
{
    for (uint32_t i = 0; i < m_values.size(); i++)
    {
        if (m_values[i])
            free(m_values[i]);
    }
}

void DataRow::ValidateColumnType(int32_t index, DataType targetType, bool read) const
{
    const DataType columnType = m_parent->Column(index)->Type();

    if (columnType != targetType)
    {
        stringstream errorMessageStream;
        errorMessageStream << "Cannot" << (read ? " read " : " assign ") << EnumName(targetType)  << " value" << (read ? " from " : " to ") << "DataColumn " << index << ", column data type is " << EnumName(columnType);
        throw DataRowException(errorMessageStream.str());
    }
}

template<typename T>
Nullable<T> DataRow::GetValue(int32_t index, DataType targetType) const
{
    ValidateColumnType(index, targetType, true);

    T* value = static_cast<T*>(m_values[index]);

    if (value)
        return *value;

    return nullptr;
}

template<typename T>
void DataRow::SetValue(int32_t index, const Nullable<T>& value, DataType targetType)
{
    ValidateColumnType(index, targetType);

    if (value.HasValue())
    {
        uint8_t* copy = static_cast<uint8_t*>(malloc(sizeof(T)));
        memcpy(copy, &value.Value, sizeof(T));
        m_values[index] = copy;
    }
    else
    {
        m_values[index] = nullptr;
    }
}

const DataTablePtr& DataRow::Parent() const
{
    return m_parent;
}

Nullable<string> DataRow::ValueAsString(int32_t index) const
{
    ValidateColumnType(index, DataType::String, true);

    const char* value  = static_cast<const char*>(m_values[index]);

    if (value)
        return string(value);

    return nullptr;
}

void DataRow::SetStringValue(int32_t index, const Nullable<string>& value)
{
    ValidateColumnType(index, DataType::String);

    if (value.HasValue())
    {
        const string& strval = value.Value;
        const int32_t length = strval.size();
        char* copy = static_cast<char*>(malloc(length + 1));
        strcpy_s(copy, length, strval.c_str());
        m_values[index] = copy;
    }
    else
    {
        m_values[index] = nullptr;
    }
}

Nullable<bool> DataRow::ValueAsBoolean(int32_t index) const
{
    ValidateColumnType(index, DataType::Boolean, true);

    uint8_t* value = static_cast<uint8_t*>(m_values[index]);
    
    if (value)
        return *value != 0;
    
    return nullptr;
}

void DataRow::SetBooleanValue(int32_t index, const Nullable<bool>& value)
{
    ValidateColumnType(index, DataType::Boolean);

    if (value.HasValue())
    {
        uint8_t* copy = static_cast<uint8_t*>(malloc(1));
        *copy = static_cast<bool>(value.Value) ? 1 : 0;
        m_values[index] = copy;
    }
    else
    {
        m_values[index] = nullptr;
    }
}

Nullable<time_t> DataRow::ValueAsDateTime(int32_t index) const
{
    return GetValue<time_t>(index, DataType::DateTime);
}

void DataRow::SetDateTimeValue(int32_t index, const Nullable<time_t>& value)
{
    SetValue<time_t>(index, value, DataType::DateTime);
}

Nullable<float32_t> DataRow::ValueAsSingle(int32_t index) const
{
    return GetValue<float32_t>(index, DataType::Single);
}

void DataRow::SetSingleValue(int32_t index, const Nullable<float32_t>& value)
{
    SetValue<float32_t>(index, value, DataType::Single);
}

Nullable<float64_t> DataRow::ValueAsDouble(int32_t index) const
{
    return GetValue<float64_t>(index, DataType::Double);
}

void DataRow::SetDoubleValue(int32_t index, const Nullable<float64_t>& value)
{
    SetValue<float64_t>(index, value, DataType::Double);
}

Nullable<decimal_t> DataRow::ValueAsDecimal(int32_t index) const
{
    ValidateColumnType(index, DataType::Decimal, true);
 
    const char* value = static_cast<const char*>(m_values[index]);

    if (value)
        return decimal_t(value);

    return nullptr;
}

void DataRow::SetDecimalValue(int32_t index, const Nullable<decimal_t>& value)
{
    ValidateColumnType(index, DataType::Decimal);

    if (value.HasValue())
    {
        // The boost decimal type has a very complex internal representation,
        // although slower, it's safer just to store this as a string for now
        const string& strval = static_cast<decimal_t>(value.Value).str();
        const int32_t length = strval.size();
        char* copy = static_cast<char*>(malloc(length + 1));
        strcpy_s(copy, length, strval.c_str());
        m_values[index] = copy;
    }
    else
    {
        m_values[index] = nullptr;
    }
}

Nullable<Guid> DataRow::ValueAsGuid(int32_t index) const
{
    ValidateColumnType(index, DataType::Guid, true);

    int8_t* data = static_cast<int8_t*>(m_values[index]);

    if (data)
    {
        Guid value;
        memcpy(value.data, data, 16);
        return value;
    }

    return nullptr;
}

void DataRow::SetGuidValue(int32_t index, const Nullable<Guid>& value)
{
    ValidateColumnType(index, DataType::Guid);

    if (value.HasValue())
    {
        int8_t* copy = static_cast<int8_t*>(malloc(16));
        memcpy(copy, static_cast<Guid>(value.Value).data, 16);
        m_values[index] = copy;
    }
    else
    {
        m_values[index] = nullptr;
    }
}

Nullable<int8_t> DataRow::ValueAsInt8(int32_t index) const
{
    return GetValue<int8_t>(index, DataType::Int8);
}

void DataRow::SetInt8Value(int32_t index, const Nullable<int8_t>& value)
{
    SetValue<int8_t>(index, value, DataType::Int8);
}

Nullable<int16_t> DataRow::ValueAsInt16(int32_t index) const
{
    return GetValue<int16_t>(index, DataType::Int16);
}

void DataRow::SetInt16Value(int32_t index, const Nullable<int16_t>& value)
{
    SetValue<int16_t>(index, value, DataType::Int16);
}

Nullable<int32_t> DataRow::ValueAsInt32(int32_t index) const
{
    return GetValue<int32_t>(index, DataType::Int32);
}

void DataRow::SetInt32Value(int32_t index, const Nullable<int32_t>& value)
{
    SetValue<int32_t>(index, value, DataType::Int32);
}

Nullable<int64_t> DataRow::ValueAsInt64(int32_t index) const
{
    return GetValue<int64_t>(index, DataType::Int64);
}

void DataRow::SetInt64Value(int32_t index, const Nullable<int64_t>& value)
{
    SetValue<int64_t>(index, value, DataType::Int64);
}

Nullable<uint8_t> DataRow::ValueAsUInt8(int32_t index) const
{
    return GetValue<uint8_t>(index, DataType::UInt8);
}

void DataRow::SetUInt8Value(int32_t index, const Nullable<uint8_t>& value)
{
    SetValue<uint8_t>(index, value, DataType::UInt8);
}

Nullable<uint16_t> DataRow::ValueAsUInt16(int32_t index) const
{
    return GetValue<uint16_t>(index, DataType::UInt16);
}

void DataRow::SetUInt16Value(int32_t index, const Nullable<uint16_t>& value)
{
    SetValue<uint16_t>(index, value, DataType::UInt16);
}

Nullable<uint32_t> DataRow::ValueAsUInt32(int32_t index) const
{
    return GetValue<uint32_t>(index, DataType::UInt32);
}

void DataRow::SetUInt32Value(int32_t index, const Nullable<uint32_t>& value)
{
    SetValue<uint32_t>(index, value, DataType::UInt32);
}

Nullable<uint64_t> DataRow::ValueAsUInt64(int32_t index) const
{
    return GetValue<uint64_t>(index, DataType::UInt64);
}

void DataRow::SetUInt64Value(int32_t index, const Nullable<uint64_t>& value)
{
    SetValue<uint64_t>(index, value, DataType::UInt64);
}