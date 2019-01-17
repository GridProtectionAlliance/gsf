//******************************************************************************************************
//  DataRow.h - Gbtc
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

#ifndef __DATA_ROW_H
#define __DATA_ROW_H

#include <vector>

#include "../Common/CommonTypes.h"
#include "../Common/Nullable.h"

namespace GSF {
namespace Data
{

enum class DataType;

class DataTable;
typedef TimeSeries::SharedPtr<DataTable> DataTablePtr;

class DataRow;
typedef TimeSeries::SharedPtr<DataRow> DataRowPtr;

class DataRow // NOLINT
{
private:
    DataTablePtr m_parent;
    std::vector<void*> m_values;

    int32_t GetColumnIndex(const std::string& columnName) const;
    void ValidateColumnType(int32_t columnIndex, DataType targetType, bool read = false) const;

    template<typename T>
    GSF::TimeSeries::Nullable<T> GetValue(int32_t columnIndex, DataType targetType) const;

    template<typename T>
    void SetValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<T>& value, DataType targetType);

public:
    DataRow(DataTablePtr parent);
    ~DataRow();

    const DataTablePtr& Parent() const;

    void SetNullValue(int32_t columnIndex);
    void SetNullValue(const std::string& columnName);

    GSF::TimeSeries::Nullable<std::string> ValueAsString(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<std::string> ValueAsString(const std::string& columnName) const;
    void SetStringValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<std::string>& value);
    void SetStringValue(const std::string& columnName, const GSF::TimeSeries::Nullable<std::string>& value);

    GSF::TimeSeries::Nullable<bool> ValueAsBoolean(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<bool> ValueAsBoolean(const std::string& columnName) const;
    void SetBooleanValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<bool>& value);
    void SetBooleanValue(const std::string& columnName, const GSF::TimeSeries::Nullable<bool>& value);

    GSF::TimeSeries::Nullable<GSF::TimeSeries::DateTime> ValueAsDateTime(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<GSF::TimeSeries::DateTime> ValueAsDateTime(const std::string& columnName) const;
    void SetDateTimeValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<GSF::TimeSeries::DateTime>& value);
    void SetDateTimeValue(const std::string& columnName, const GSF::TimeSeries::Nullable<GSF::TimeSeries::DateTime>& value);

    GSF::TimeSeries::Nullable<TimeSeries::float32_t> ValueAsSingle(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<TimeSeries::float32_t> ValueAsSingle(const std::string& columnName) const;
    void SetSingleValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<TimeSeries::float32_t>& value);
    void SetSingleValue(const std::string& columnName, const GSF::TimeSeries::Nullable<TimeSeries::float32_t>& value);

    GSF::TimeSeries::Nullable<TimeSeries::float64_t> ValueAsDouble(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<TimeSeries::float64_t> ValueAsDouble(const std::string& columnName) const;
    void SetDoubleValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<TimeSeries::float64_t>& value);
    void SetDoubleValue(const std::string& columnName, const GSF::TimeSeries::Nullable<TimeSeries::float64_t>& value);

    GSF::TimeSeries::Nullable<TimeSeries::decimal_t> ValueAsDecimal(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<TimeSeries::decimal_t> ValueAsDecimal(const std::string& columnName) const;
    void SetDecimalValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<TimeSeries::decimal_t>& value);
    void SetDecimalValue(const std::string& columnName, const GSF::TimeSeries::Nullable<TimeSeries::decimal_t>& value);

    GSF::TimeSeries::Nullable<TimeSeries::Guid> ValueAsGuid(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<TimeSeries::Guid> ValueAsGuid(const std::string& columnName) const;
    void SetGuidValue(int32_t columnIndex, const GSF::TimeSeries::Nullable<TimeSeries::Guid>& value);
    void SetGuidValue(const std::string& columnName, const GSF::TimeSeries::Nullable<TimeSeries::Guid>& value);

    GSF::TimeSeries::Nullable<int8_t> ValueAsInt8(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<int8_t> ValueAsInt8(const std::string& columnName) const;
    void SetInt8Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<int8_t>& value);
    void SetInt8Value(const std::string& columnName, const GSF::TimeSeries::Nullable<int8_t>& value);

    GSF::TimeSeries::Nullable<int16_t> ValueAsInt16(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<int16_t> ValueAsInt16(const std::string& columnName) const;
    void SetInt16Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<int16_t>& value);
    void SetInt16Value(const std::string& columnName, const GSF::TimeSeries::Nullable<int16_t>& value);

    GSF::TimeSeries::Nullable<int32_t> ValueAsInt32(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<int32_t> ValueAsInt32(const std::string& columnName) const;
    void SetInt32Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<int32_t>& value);
    void SetInt32Value(const std::string& columnName, const GSF::TimeSeries::Nullable<int32_t>& value);

    GSF::TimeSeries::Nullable<int64_t> ValueAsInt64(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<int64_t> ValueAsInt64(const std::string& columnName) const;
    void SetInt64Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<int64_t>& value);
    void SetInt64Value(const std::string& columnName, const GSF::TimeSeries::Nullable<int64_t>& value);

    GSF::TimeSeries::Nullable<uint8_t> ValueAsUInt8(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<uint8_t> ValueAsUInt8(const std::string& columnName) const;
    void SetUInt8Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<uint8_t>& value);
    void SetUInt8Value(const std::string& columnName, const GSF::TimeSeries::Nullable<uint8_t>& value);

    GSF::TimeSeries::Nullable<uint16_t> ValueAsUInt16(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<uint16_t> ValueAsUInt16(const std::string& columnName) const;
    void SetUInt16Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<uint16_t>& value);
    void SetUInt16Value(const std::string& columnName, const GSF::TimeSeries::Nullable<uint16_t>& value);

    GSF::TimeSeries::Nullable<uint32_t> ValueAsUInt32(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<uint32_t> ValueAsUInt32(const std::string& columnName) const;
    void SetUInt32Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<uint32_t>& value);
    void SetUInt32Value(const std::string& columnName, const GSF::TimeSeries::Nullable<uint32_t>& value);

    GSF::TimeSeries::Nullable<uint64_t> ValueAsUInt64(int32_t columnIndex) const;
    GSF::TimeSeries::Nullable<uint64_t> ValueAsUInt64(const std::string& columnName) const;
    void SetUInt64Value(int32_t columnIndex, const GSF::TimeSeries::Nullable<uint64_t>& value);
    void SetUInt64Value(const std::string& columnName, const GSF::TimeSeries::Nullable<uint64_t>& value);

    static const DataRowPtr NullPtr;

    friend class DataTable;
};

}}

#endif
