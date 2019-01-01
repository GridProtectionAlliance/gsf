//******************************************************************************************************
//  DataSet.cpp - Gbtc
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

#include "DataSet.h"

using namespace std;
using namespace GSF::DataSet;

DataSet::DataSet()
{
}

DataSet::~DataSet()
{
}

const DataTablePtr& DataSet::Table(const string& tableName) const
{
    const auto iterator = m_tables.find(tableName);

    if (iterator != m_tables.end())
        return iterator->second;

    return DataTable::NullPtr;
}

const DataTablePtr& DataSet::operator[](const std::string& tableName) const
{
    return Table(tableName);
}

void DataSet::IterateTables(TableIteratorHandlerFunction iteratorHandler, void* userData)
{
    for (auto const& item : m_tables)
        iteratorHandler(item.second, userData);
}

bool DataSet::AddTable(const DataTablePtr& table)
{
    // Returns true on insert, false on update
    return m_tables.insert_or_assign(table->Name(), table).second;
}

bool DataSet::RemoveTable(const std::string& tableName)
{
    return m_tables.erase(tableName) > 0;
}
