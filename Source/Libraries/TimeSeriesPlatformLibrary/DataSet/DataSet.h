//******************************************************************************************************
//  DataSet.h - Gbtc
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

#ifndef __DATA_SET_H
#define __DATA_SET_H

#include <map>
#include "../Common/CommonTypes.h"
#include "DataTable.h"

namespace GSF {
namespace DataSet
{

class DataSet : public boost::enable_shared_from_this<DataSet> // NOLINT
{
private:
    std::map<std::string, DataTablePtr> m_tables;

public:
     DataSet();
    ~DataSet();

    typedef void(*TableIteratorHandlerFunction)(const DataTablePtr&, void* userData);

    DataTablePtr Table(std::string& tableName);

    void IterateTables(TableIteratorHandlerFunction iteratorHandler, void* userData);
};

typedef TimeSeries::SharedPtr<DataSet> DataSetPtr;

}}

#endif