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
#include "DataSet.h"

using namespace std;
using namespace GSF;
using namespace GSF::Data;

const DataRowPtr DataRow::NullPtr = nullptr;

DataRow::DataRow(DataTablePtr parent) :
    m_parent(std::move(parent)),
    m_values(m_parent->ColumnCount())
{
    if (m_parent == nullptr)
        throw DataSetException("DataTable parent is null");

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

int32_t DataRow::GetColumnIndex(const string& columnName) const
{
    const DataColumnPtr column = m_parent->Column(columnName);

    if (column == nullptr)
        throw DataSetException("Column name \"" + columnName + "\" was not found in table \"" + m_parent->Name() + "\"");

    return column->Index();
}

void DataRow::ValidateColumnType(int32_t columnIndex, DataType targetType, bool read) const
{
    const DataColumnPtr column = m_parent->Column(columnIndex);

    if (column == nullptr)
        throw DataSetException("Column index " + ToString(columnIndex) + " is out of range for table \"" + m_parent->Name() + "\"");

    const DataType columnType = column->Type();

    if (columnType != targetType)
    {
        stringstream errorMessageStream;
        errorMessageStream << "Cannot" << (read ? " read " : " assign ") << "\"" << EnumName(targetType)  << "\" value" << (read ? " from " : " to ") << "DataColumn \"" << column->Name() << "\" for table \"" << m_parent->Name() << "\", column data type is \"" << EnumName(columnType) << "\"";
        throw DataSetException(errorMessageStream.str());
    }

    if (!read && column->Computed())
        throw DataSetException("Cannot assign value to DataColumn \"" + column->Name() + " for table \"" + m_parent->Name() + "\", column is computed with an expression");
}

template<typename T>
Nullable<T> DataRow::GetValue(int32_t columnIndex, DataType targetType) const
{
    ValidateColumnType(columnIndex, targetType, true);

    T* value = static_cast<T*>(m_values[columnIndex]);

    if (value)
        return *value;

    return nullptr;
}

template<typename T>
void DataRow::SetValue(int32_t columnIndex, const Nullable<T>& value, DataType targetType)
{
    ValidateColumnType(columnIndex, targetType);

    if (value.HasValue())
    {
        uint8_t* copy = static_cast<uint8_t*>(malloc(sizeof(T)));
        memcpy(copy, &value.Value, sizeof(T));
        m_values[columnIndex] = copy;
    }
    else
    {
        m_values[columnIndex] = nullptr;
    }
}

const DataTablePtr& DataRow::Parent() const
{
    return m_parent;
}

void DataRow::SetNullValue(int32_t columnIndex)
{
    const DataColumnPtr column = m_parent->Column(columnIndex);

    if (column == nullptr)
        throw DataSetException("Column index " + ToString(columnIndex) + " is out of range for table \"" + m_parent->Name() + "\"");

    if (column->Computed())
        throw DataSetException("Cannot assign NULL value to DataColumn \"" + column->Name() + " for table \"" + m_parent->Name() + "\", column is computed with an expression");

    switch (column->Type())
    {
        case DataType::String:                    
            SetStringValue(columnIndex, nullptr);
            break;
        case DataType::Boolean:
            SetBooleanValue(columnIndex, nullptr);
            break;
        case DataType::DateTime:
            SetDateTimeValue(columnIndex, nullptr);
            break;
        case DataType::Single:
            SetSingleValue(columnIndex, nullptr);
            break;
        case DataType::Double:
            SetDoubleValue(columnIndex, nullptr);
            break;
        case DataType::Decimal:
            SetDecimalValue(columnIndex, nullptr);
            break;
        case DataType::Guid:
            SetGuidValue(columnIndex, nullptr);
            break;
        case DataType::Int8:
            SetInt8Value(columnIndex, nullptr);
            break;
        case DataType::Int16:
            SetInt16Value(columnIndex, nullptr);
            break;
        case DataType::Int32:
            SetInt32Value(columnIndex, nullptr);
            break;
        case DataType::Int64:
            SetInt64Value(columnIndex, nullptr);
            break;
        case DataType::UInt8:
            SetUInt8Value(columnIndex, nullptr);
            break;
        case DataType::UInt16:
            SetUInt16Value(columnIndex, nullptr);
            break;
        case DataType::UInt32:
            SetUInt32Value(columnIndex, nullptr);
            break;
        case DataType::UInt64:
            SetUInt64Value(columnIndex, nullptr);
            break;
        default:
            throw DataSetException("Unexpected column data type encountered");
    }
}

void DataRow::SetNullValue(const string& columnName)
{
    SetNullValue(GetColumnIndex(columnName));
}

Nullable<string> DataRow::ValueAsString(int32_t columnIndex) const
{
    ValidateColumnType(columnIndex, DataType::String, true);

    const char* value  = static_cast<const char*>(m_values[columnIndex]);

    if (value)
        return string(value);

    return nullptr;
}

Nullable<string> DataRow::ValueAsString(const string& columnName) const
{
    return ValueAsString(GetColumnIndex(columnName));
}

void DataRow::SetStringValue(int32_t columnIndex, const Nullable<string>& value)
{
    ValidateColumnType(columnIndex, DataType::String);

    if (value.HasValue())
    {
        const string& strval = value.GetValueOrDefault();
        const int32_t length = strval.size() + 1;
        char* copy = static_cast<char*>(malloc(length * sizeof(char)));
        strcpy_s(copy, length, strval.c_str());
        m_values[columnIndex] = copy;
    }
    else
    {
        m_values[columnIndex] = nullptr;
    }
}

void DataRow::SetStringValue(const string& columnName, const Nullable<string>& value)
{
    SetStringValue(GetColumnIndex(columnName), value);
}

Nullable<bool> DataRow::ValueAsBoolean(int32_t columnIndex) const
{
    ValidateColumnType(columnIndex, DataType::Boolean, true);

    uint8_t* value = static_cast<uint8_t*>(m_values[columnIndex]);
    
    if (value)
        return *value != 0;
    
    return nullptr;
}

Nullable<bool> DataRow::ValueAsBoolean(const string& columnName) const
{
    return ValueAsBoolean(GetColumnIndex(columnName));
}

void DataRow::SetBooleanValue(int32_t columnIndex, const Nullable<bool>& value)
{
    ValidateColumnType(columnIndex, DataType::Boolean);

    if (value.HasValue())
    {
        uint8_t* copy = static_cast<uint8_t*>(malloc(1));
        *copy = value.GetValueOrDefault() ? 1 : 0;
        m_values[columnIndex] = copy;
    }
    else
    {
        m_values[columnIndex] = nullptr;
    }
}

void DataRow::SetBooleanValue(const string& columnName, const Nullable<bool>& value)
{
    SetBooleanValue(GetColumnIndex(columnName), value);
}

Nullable<DateTime> DataRow::ValueAsDateTime(int32_t columnIndex) const
{
    return GetValue<DateTime>(columnIndex, DataType::DateTime);
}

Nullable<DateTime> DataRow::ValueAsDateTime(const string& columnName) const
{
    return ValueAsDateTime(GetColumnIndex(columnName));
}

void DataRow::SetDateTimeValue(int32_t columnIndex, const Nullable<DateTime>& value)
{
    SetValue<DateTime>(columnIndex, value, DataType::DateTime);
}

void DataRow::SetDateTimeValue(const string& columnName, const Nullable<DateTime>& value)
{
    SetDateTimeValue(GetColumnIndex(columnName), value);
}

Nullable<float32_t> DataRow::ValueAsSingle(int32_t columnIndex) const
{
    return GetValue<float32_t>(columnIndex, DataType::Single);
}

Nullable<float32_t> DataRow::ValueAsSingle(const string& columnName) const
{
    return ValueAsSingle(GetColumnIndex(columnName));
}

void DataRow::SetSingleValue(int32_t columnIndex, const Nullable<float32_t>& value)
{
    SetValue<float32_t>(columnIndex, value, DataType::Single);
}

void DataRow::SetSingleValue(const string& columnName, const Nullable<float32_t>& value)
{
    SetSingleValue(GetColumnIndex(columnName), value);
}

Nullable<float64_t> DataRow::ValueAsDouble(int32_t columnIndex) const
{
    return GetValue<float64_t>(columnIndex, DataType::Double);
}

Nullable<float64_t> DataRow::ValueAsDouble(const string& columnName) const
{
    return ValueAsDouble(GetColumnIndex(columnName));
}

void DataRow::SetDoubleValue(int32_t columnIndex, const Nullable<float64_t>& value)
{
    SetValue<float64_t>(columnIndex, value, DataType::Double);
}

void DataRow::SetDoubleValue(const string& columnName, const Nullable<float64_t>& value)
{
    SetDoubleValue(GetColumnIndex(columnName), value);
}

Nullable<decimal_t> DataRow::ValueAsDecimal(int32_t columnIndex) const
{
    ValidateColumnType(columnIndex, DataType::Decimal, true);
 
    const char* value = static_cast<const char*>(m_values[columnIndex]);

    if (value)
        return decimal_t(value);

    return nullptr;
}

Nullable<decimal_t> DataRow::ValueAsDecimal(const string& columnName) const
{
    return ValueAsDecimal(GetColumnIndex(columnName));
}

void DataRow::SetDecimalValue(int32_t columnIndex, const Nullable<decimal_t>& value)
{
    ValidateColumnType(columnIndex, DataType::Decimal);

    if (value.HasValue())
    {
        // The boost decimal type has a very complex internal representation,
        // although slower, it's safer just to store this as a string for now
        const string& strval = value.GetValueOrDefault().str();
        const int32_t length = strval.size() + 1;
        char* copy = static_cast<char*>(malloc(length * sizeof(char)));
        strcpy_s(copy, length, strval.c_str());
        m_values[columnIndex] = copy;
    }
    else
    {
        m_values[columnIndex] = nullptr;
    }
}

void DataRow::SetDecimalValue(const string& columnName, const Nullable<decimal_t>& value)
{
    SetDecimalValue(GetColumnIndex(columnName), value);
}

Nullable<Guid> DataRow::ValueAsGuid(int32_t columnIndex) const
{
    ValidateColumnType(columnIndex, DataType::Guid, true);

    int8_t* data = static_cast<int8_t*>(m_values[columnIndex]);

    if (data)
    {
        Guid value;
        memcpy(value.data, data, 16);
        return value;
    }

    return nullptr;
}

Nullable<Guid> DataRow::ValueAsGuid(const string& columnName) const
{
    return ValueAsGuid(GetColumnIndex(columnName));
}

void DataRow::SetGuidValue(int32_t columnIndex, const Nullable<Guid>& value)
{
    ValidateColumnType(columnIndex, DataType::Guid);

    if (value.HasValue())
    {
        int8_t* copy = static_cast<int8_t*>(malloc(16));
        memcpy(copy, value.GetValueOrDefault().data, 16);
        m_values[columnIndex] = copy;
    }
    else
    {
        m_values[columnIndex] = nullptr;
    }
}

void DataRow::SetGuidValue(const string& columnName, const Nullable<Guid>& value)
{
    SetGuidValue(GetColumnIndex(columnName), value);
}

Nullable<int8_t> DataRow::ValueAsInt8(int32_t columnIndex) const
{
    return GetValue<int8_t>(columnIndex, DataType::Int8);
}

Nullable<int8_t> DataRow::ValueAsInt8(const string& columnName) const
{
    return ValueAsInt8(GetColumnIndex(columnName));
}

void DataRow::SetInt8Value(int32_t columnIndex, const Nullable<int8_t>& value)
{
    SetValue<int8_t>(columnIndex, value, DataType::Int8);
}

void DataRow::SetInt8Value(const string& columnName, const Nullable<int8_t>& value)
{
    SetInt8Value(GetColumnIndex(columnName), value);
}

Nullable<int16_t> DataRow::ValueAsInt16(int32_t columnIndex) const
{
    return GetValue<int16_t>(columnIndex, DataType::Int16);
}

Nullable<int16_t> DataRow::ValueAsInt16(const string& columnName) const
{
    return ValueAsInt16(GetColumnIndex(columnName));
}

void DataRow::SetInt16Value(int32_t columnIndex, const Nullable<int16_t>& value)
{
    SetValue<int16_t>(columnIndex, value, DataType::Int16);
}

void DataRow::SetInt16Value(const string& columnName, const Nullable<int16_t>& value)
{
    SetInt16Value(GetColumnIndex(columnName), value);
}

Nullable<int32_t> DataRow::ValueAsInt32(int32_t columnIndex) const
{
    return GetValue<int32_t>(columnIndex, DataType::Int32);
}

Nullable<int32_t> DataRow::ValueAsInt32(const string& columnName) const
{
    return ValueAsInt32(GetColumnIndex(columnName));
}

void DataRow::SetInt32Value(int32_t columnIndex, const Nullable<int32_t>& value)
{
    SetValue<int32_t>(columnIndex, value, DataType::Int32);
}

void DataRow::SetInt32Value(const string& columnName, const Nullable<int32_t>& value)
{
    SetInt32Value(GetColumnIndex(columnName), value);
}

Nullable<int64_t> DataRow::ValueAsInt64(int32_t columnIndex) const
{
    return GetValue<int64_t>(columnIndex, DataType::Int64);
}

Nullable<int64_t> DataRow::ValueAsInt64(const string& columnName) const
{
    return ValueAsInt64(GetColumnIndex(columnName));
}

void DataRow::SetInt64Value(int32_t columnIndex, const Nullable<int64_t>& value)
{
    SetValue<int64_t>(columnIndex, value, DataType::Int64);
}

void DataRow::SetInt64Value(const string& columnName, const Nullable<int64_t>& value)
{
    SetInt64Value(GetColumnIndex(columnName), value);
}

Nullable<uint8_t> DataRow::ValueAsUInt8(int32_t columnIndex) const
{
    return GetValue<uint8_t>(columnIndex, DataType::UInt8);
}

Nullable<uint8_t> DataRow::ValueAsUInt8(const string& columnName) const
{
    return ValueAsUInt8(GetColumnIndex(columnName));
}

void DataRow::SetUInt8Value(int32_t columnIndex, const Nullable<uint8_t>& value)
{
    SetValue<uint8_t>(columnIndex, value, DataType::UInt8);
}

void DataRow::SetUInt8Value(const string& columnName, const Nullable<uint8_t>& value)
{
    SetUInt8Value(GetColumnIndex(columnName), value);
}

Nullable<uint16_t> DataRow::ValueAsUInt16(int32_t columnIndex) const
{
    return GetValue<uint16_t>(columnIndex, DataType::UInt16);
}

Nullable<uint16_t> DataRow::ValueAsUInt16(const string& columnName) const
{
    return ValueAsUInt16(GetColumnIndex(columnName));
}

void DataRow::SetUInt16Value(int32_t columnIndex, const Nullable<uint16_t>& value)
{
    SetValue<uint16_t>(columnIndex, value, DataType::UInt16);
}

void DataRow::SetUInt16Value(const string& columnName, const Nullable<uint16_t>& value)
{
    SetUInt16Value(GetColumnIndex(columnName), value);
}

Nullable<uint32_t> DataRow::ValueAsUInt32(int32_t columnIndex) const
{
    return GetValue<uint32_t>(columnIndex, DataType::UInt32);
}

Nullable<uint32_t> DataRow::ValueAsUInt32(const string& columnName) const
{
    return ValueAsUInt32(GetColumnIndex(columnName));
}

void DataRow::SetUInt32Value(int32_t columnIndex, const Nullable<uint32_t>& value)
{
    SetValue<uint32_t>(columnIndex, value, DataType::UInt32);
}

void DataRow::SetUInt32Value(const string& columnName, const Nullable<uint32_t>& value)
{
    SetUInt32Value(GetColumnIndex(columnName), value);
}

Nullable<uint64_t> DataRow::ValueAsUInt64(int32_t columnIndex) const
{
    return GetValue<uint64_t>(columnIndex, DataType::UInt64);
}

Nullable<uint64_t> DataRow::ValueAsUInt64(const string& columnName) const
{
    return ValueAsUInt64(GetColumnIndex(columnName));
}

void DataRow::SetUInt64Value(int32_t columnIndex, const Nullable<uint64_t>& value)
{
    SetValue<uint64_t>(columnIndex, value, DataType::UInt64);
}

void DataRow::SetUInt64Value(const string& columnName, const Nullable<uint64_t>& value)
{
    SetUInt64Value(GetColumnIndex(columnName), value);
}