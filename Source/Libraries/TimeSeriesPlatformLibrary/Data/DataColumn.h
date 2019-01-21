//******************************************************************************************************
//  DataColumn.h - Gbtc
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

#ifndef __DATA_COLUMN_H
#define __DATA_COLUMN_H

#include <string>

#include "../Common/CommonTypes.h"

namespace GSF {
namespace Data
{

enum class DataType
{
    String,
    Boolean,
    DateTime,
    Single,
    Double,
    Decimal,
    Guid,
    Int8,
    Int16,
    Int32,
    Int64,
    UInt8,
    UInt16,
    UInt32,
    UInt64
};

const char* DataTypeAcronym[];
const char* EnumName(DataType type);

class DataTable;
typedef GSF::SharedPtr<DataTable> DataTablePtr;

class DataColumn;
typedef GSF::SharedPtr<DataColumn> DataColumnPtr;

class DataColumn // NOLINT
{
private:
    DataTablePtr m_parent;
    std::string m_name;
    DataType m_type;
    std::string m_expression;
    bool m_computed;
    int32_t m_index;

public:
    DataColumn(DataTablePtr parent, std::string name, DataType type, std::string expression = std::string {});
    ~DataColumn();

    const DataTablePtr& Parent() const;

    const std::string& Name() const;

    DataType Type() const;

    const std::string& Expression() const;

    bool Computed() const;

    int32_t Index() const;

    static const DataColumnPtr NullPtr;

    friend class DataTable;
};

}}

#endif