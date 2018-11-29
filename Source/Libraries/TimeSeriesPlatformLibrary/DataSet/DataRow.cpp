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

DataRow::DataRow(const DataTablePtr& parent) :
    m_parent(parent),
    m_values(parent->ColumnCount())
{
    for (uint32_t i = 0; i < m_values.size(); i++)
    {
        m_values[i] = nullptr;
    }
}

DataRow::~DataRow()
{
    for (uint32_t i = 0; i < m_values.size(); i++)
    {
        if (m_values[i] != nullptr)
            free(m_values[i]);
    }
}

void DataRow::ValidateColumnType(int32_t index, DataType targetType) const
{
    const DataType columnType = m_parent->Column(index)->Type();

    if (columnType != targetType)
    {
        stringstream errorMessageStream;
        errorMessageStream << "Cannot assign " << EnumName(targetType)  << " value to DataColumn " << index << ", data type is " << EnumName(columnType);
        throw DataRowException(errorMessageStream.str());
    }
}

template<typename T>
T DataRow::GetValue(int32_t index) const
{
    return *static_cast<T*>(m_values[index]);
}

template<typename T>
void DataRow::SetValue(int32_t index, T value, DataType targetType)
{
    ValidateColumnType(index, targetType);

    uint8_t* copy = static_cast<uint8_t*>(malloc(sizeof(T)));
    memcpy(copy, &value, sizeof(T));

    m_values[index] = copy;
}

const DataTablePtr& DataRow::Parent() const
{
    return m_parent;
}

const char* DataRow::ValueAsString(int32_t index) const
{
    return static_cast<const char*>(m_values[index]);
}

void DataRow::SetStringValue(int32_t index, const char* value)
{
    ValidateColumnType(index, DataType::String);

    const int32_t length = strlen(value);
    char* copy = static_cast<char*>(malloc(length + 1));
    strcpy_s(copy, length, value);

    m_values[index] = copy;
}

bool DataRow::ValueAsBoolean(int32_t index) const
{
    return *static_cast<uint8_t*>(m_values[index]) != 0;
}

void DataRow::SetBooleanValue(int32_t index, bool value)
{
    ValidateColumnType(index, DataType::Boolean);

    uint8_t* copy = static_cast<uint8_t*>(malloc(1));
    *copy = value ? 1 : 0;

    m_values[index] = copy;
}

time_t DataRow::ValueAsDateTime(int32_t index) const
{
    return GetValue<time_t>(index);
}

void DataRow::SetDateTimeValue(int32_t index, time_t value)
{
    SetValue<time_t>(index, value, DataType::DateTime);
}

float DataRow::ValueAsSingle(int32_t index) const
{
    return GetValue<float>(index);
}

void DataRow::SetSingleValue(int32_t index, float value)
{
    SetValue<float>(index, value, DataType::Single);
}

double DataRow::ValueAsDouble(int32_t index) const
{
    return GetValue<double>(index);
}

void DataRow::SetDoubleValue(int32_t index, double value)
{
    SetValue<double>(index, value, DataType::Double);
}

Guid DataRow::ValueAsGuid(int32_t index) const
{
    return GetValue<Guid>(index);
}

void DataRow::SetGuidValue(int32_t index, Guid value)
{
    SetValue<Guid>(index, value, DataType::Guid);
}

int8_t DataRow::ValueAsInt8(int32_t index) const
{
    return GetValue<int8_t>(index);
}

void DataRow::SetInt8Value(int32_t index, int8_t value)
{
    SetValue<int8_t>(index, value, DataType::Int8);
}

int16_t DataRow::ValueAsInt16(int32_t index) const
{
    return GetValue<int16_t>(index);
}

void DataRow::SetInt16Value(int32_t index, int16_t value)
{
    SetValue<int16_t>(index, value, DataType::Int16);
}

int32_t DataRow::ValueAsInt32(int32_t index) const
{
    return GetValue<int32_t>(index);
}

void DataRow::SetInt32Value(int32_t index, int32_t value)
{
    SetValue<int32_t>(index, value, DataType::Int32);
}

int64_t DataRow::ValueAsInt64(int32_t index) const
{
    return GetValue<int64_t>(index);
}

void DataRow::SetInt64Value(int32_t index, int64_t value)
{
    SetValue<int64_t>(index, value, DataType::Int64);
}

uint8_t DataRow::ValueAsUInt8(int32_t index) const
{
    return GetValue<uint8_t>(index);
}

void DataRow::SetUInt8Value(int32_t index, uint8_t value)
{
    SetValue<uint8_t>(index, value, DataType::UInt8);
}

uint16_t DataRow::ValueAsUInt16(int32_t index) const
{
    return GetValue<uint16_t>(index);
}

void DataRow::SetUInt16Value(int32_t index, uint16_t value)
{
    SetValue<uint16_t>(index, value, DataType::UInt16);
}

uint32_t DataRow::ValueAsUInt32(int32_t index) const
{
    return GetValue<uint32_t>(index);
}

void DataRow::SetUInt32Value(int32_t index, uint32_t value)
{
    SetValue<uint32_t>(index, value, DataType::UInt32);
}

uint64_t DataRow::ValueAsUInt64(int32_t index) const
{
    return GetValue<uint64_t>(index);
}

void DataRow::SetUInt64Value(int32_t index, uint64_t value)
{
    SetValue<uint64_t>(index, value, DataType::UInt64);
}
