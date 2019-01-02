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

// Simple exception type thrown by DataRow
class DataRowException : public TimeSeries::Exception
{
private:
    std::string m_message;

public:
    DataRowException(std::string message) noexcept;
    const char* what() const noexcept;
};

class DataRow // NOLINT
{
private:
    DataTablePtr m_parent;
    std::vector<void*> m_values;

    void ValidateColumnType(int32_t index, DataType targetType, bool read = false) const;

    template<typename T>
    GSF::TimeSeries::Nullable<T> GetValue(int32_t index, DataType targetType) const;

    template<typename T>
    void SetValue(int32_t index, const GSF::TimeSeries::Nullable<T>& value, DataType targetType);

public:
    DataRow(DataTablePtr parent);
    ~DataRow();

    const DataTablePtr& Parent() const;

    GSF::TimeSeries::Nullable<std::string> ValueAsString(int32_t index) const;
    void SetStringValue(int32_t index, const GSF::TimeSeries::Nullable<std::string>& value);

    GSF::TimeSeries::Nullable<bool> ValueAsBoolean(int32_t index) const;
    void SetBooleanValue(int32_t index, const GSF::TimeSeries::Nullable<bool>& value);

    GSF::TimeSeries::Nullable<time_t> ValueAsDateTime(int32_t index) const;
    void SetDateTimeValue(int32_t index, const GSF::TimeSeries::Nullable<time_t>& value);

    GSF::TimeSeries::Nullable<TimeSeries::float32_t> ValueAsSingle(int32_t index) const;
    void SetSingleValue(int32_t index, const GSF::TimeSeries::Nullable<TimeSeries::float32_t>& value);

    GSF::TimeSeries::Nullable<TimeSeries::float64_t> ValueAsDouble(int32_t index) const;
    void SetDoubleValue(int32_t index, const GSF::TimeSeries::Nullable<TimeSeries::float64_t>& value);

    GSF::TimeSeries::Nullable<TimeSeries::decimal_t> ValueAsDecimal(int32_t index) const;
    void SetDecimalValue(int32_t index, const GSF::TimeSeries::Nullable<TimeSeries::decimal_t>& value);

    GSF::TimeSeries::Nullable<TimeSeries::Guid> ValueAsGuid(int32_t index) const;
    void SetGuidValue(int32_t index, const GSF::TimeSeries::Nullable<TimeSeries::Guid>& value);

    GSF::TimeSeries::Nullable<int8_t> ValueAsInt8(int32_t index) const;
    void SetInt8Value(int32_t index, const GSF::TimeSeries::Nullable<int8_t>& value);

    GSF::TimeSeries::Nullable<int16_t> ValueAsInt16(int32_t index) const;
    void SetInt16Value(int32_t index, const GSF::TimeSeries::Nullable<int16_t>& value);

    GSF::TimeSeries::Nullable<int32_t> ValueAsInt32(int32_t index) const;
    void SetInt32Value(int32_t index, const GSF::TimeSeries::Nullable<int32_t>& value);

    GSF::TimeSeries::Nullable<int64_t> ValueAsInt64(int32_t index) const;
    void SetInt64Value(int32_t index, const GSF::TimeSeries::Nullable<int64_t>& value);

    GSF::TimeSeries::Nullable<uint8_t> ValueAsUInt8(int32_t index) const;
    void SetUInt8Value(int32_t index, const GSF::TimeSeries::Nullable<uint8_t>& value);

    GSF::TimeSeries::Nullable<uint16_t> ValueAsUInt16(int32_t index) const;
    void SetUInt16Value(int32_t index, const GSF::TimeSeries::Nullable<uint16_t>& value);

    GSF::TimeSeries::Nullable<uint32_t> ValueAsUInt32(int32_t index) const;
    void SetUInt32Value(int32_t index, const GSF::TimeSeries::Nullable<uint32_t>& value);

    GSF::TimeSeries::Nullable<uint64_t> ValueAsUInt64(int32_t index) const;
    void SetUInt64Value(int32_t index, const GSF::TimeSeries::Nullable<uint64_t>& value);

    static const DataRowPtr NullPtr;

    friend class DataTable;
};

}}

#endif
