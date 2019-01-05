//******************************************************************************************************
//  DataTable.cpp - Gbtc
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

#include "DataTable.h"
#include "DataSet.h"

using namespace std;
using namespace GSF::TimeSeries;
using namespace GSF::Data;

const DataTablePtr DataTable::NullPtr = nullptr;

DataTable::DataTable(DataSetPtr parent, string name) :
    m_parent(std::move(parent)),
    m_name(std::move(name))
{
    if (m_parent == nullptr)
        throw DataSetException("DataSet parent is null");
}

DataTable::~DataTable() = default;

const DataSetPtr& DataTable::Parent() const
{
    return m_parent;
}

const string& DataTable::Name() const
{
    return m_name;
}

void DataTable::AddColumn(DataColumnPtr column)
{
    column->m_index = m_columns.size();
    m_columnIndexes.insert(pair<string, int32_t>(column->Name(), column->m_index));
    m_columns.push_back(std::move(column));
}

const DataColumnPtr& DataTable::Column(const string& columnName) const
{
    const auto iterator = m_columnIndexes.find(columnName);

    if (iterator != m_columnIndexes.end())
        return Column(iterator->second);

    return DataColumn::NullPtr;
}

const DataColumnPtr& DataTable::Column(int32_t index) const
{
    if (index < 0 || index >= static_cast<int32_t>(m_columns.size()))
        return DataColumn::NullPtr;

    return m_columns[index];
}

const DataColumnPtr& DataTable::operator[](const string& columnName) const
{
    return Column(columnName);
}

const DataColumnPtr& DataTable::operator[](int32_t index) const
{
    return Column(index);
}

DataColumnPtr DataTable::CreateColumn(const string& name, DataType type)
{
    return NewSharedPtr<DataColumn, DataTablePtr, string, DataType>(shared_from_this(), name, type);
}

int32_t DataTable::ColumnCount() const
{
    return m_columns.size();
}

const DataRowPtr& DataTable::Row(int32_t index)
{
    if (index < 0 || index >= static_cast<int32_t>(m_rows.size()))
        return DataRow::NullPtr;
    
    return m_rows[index];
}

void DataTable::AddRow(DataRowPtr row)
{
    m_rows.push_back(std::move(row));
}

DataRowPtr DataTable::CreateRow()
{
    return NewSharedPtr<DataRow, DataTablePtr>(shared_from_this());
}

int32_t DataTable::RowCount() const
{
    return m_rows.size();
}
