//******************************************************************************************************
//  DataColumn.cpp - Gbtc
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

#include "DataColumn.h"
#include "DataTable.h"
#include "DataSet.h"

using namespace std;
using namespace GSF::Data;

const char* GSF::Data::DataTypeAcronym[] =
{
    "String",
    "Boolean",
    "DateTime",
    "Single",
    "Double",
    "Decimal",
    "Guid",
    "Int8",
    "Int16",
    "Int32",
    "Int64",
    "UInt8",
    "UInt16",
    "UInt32",
    "UInt64"
};

const char* GSF::Data::EnumName(DataType type)
{
    return DataTypeAcronym[static_cast<int32_t>(type)];
}

const DataColumnPtr DataColumn::NullPtr = nullptr;

DataColumn::DataColumn(DataTablePtr parent, string name, DataType type, string expression) :
    m_parent(std::move(parent)),
    m_name(std::move(name)),
    m_type(type),
    m_expression(std::move(expression)),
    m_computed(!m_expression.empty()),
    m_index(-1)
{
    if (m_parent == nullptr)
        throw DataSetException("DataTable parent is null");
}

DataColumn::~DataColumn() = default;

const DataTablePtr& DataColumn::Parent() const
{
    return m_parent;
}

const string& DataColumn::Name() const
{
    return m_name;
}

DataType DataColumn::Type() const
{
    return m_type;
}

const std::string& DataColumn::Expression() const
{
    return m_expression;
}

bool DataColumn::Computed() const
{
    return m_computed;
}

int32_t DataColumn::Index() const
{
    return m_index;
}
