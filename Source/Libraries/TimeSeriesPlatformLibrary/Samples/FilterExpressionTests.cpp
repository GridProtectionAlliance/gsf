//******************************************************************************************************
//  FilterExpressionTests.cpp - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  01/01/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include <iostream>
#include <string>

#include "../Transport/FilterExpressions/FilterExpressionParser.h"
#include "../Data/DataSet.h"

using namespace std;
using namespace GSF;
using namespace GSF::Data;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

// Sample application to test the filter expression parser.
int main(int argc, char* argv[])
{
    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    DataTablePtr dataTable = NewSharedPtr<DataTable>(dataSet, "ActiveMeasurements");
    DataColumnPtr dataColumn;
    DataRowPtr dataRow;

    dataColumn = NewSharedPtr<DataColumn>(dataTable, "SignalID", DataType::Guid);
    dataTable->AddColumn(dataColumn);
    const int32_t signalIDField = dataTable->Column("SignalID")->Index();

    dataColumn = NewSharedPtr<DataColumn>(dataTable, "SignalType", DataType::String);
    dataTable->AddColumn(dataColumn);
    const int32_t signalTypeField = dataTable->Column("SignalType")->Index();

    dataRow = NewSharedPtr<DataRow>(dataTable);
    dataRow->SetGuidValue(signalIDField, GSF::TimeSeries::Guid());
    dataRow->SetStringValue(signalTypeField, string("STAT"));
    dataTable->AddRow(dataRow);

    dataRow = NewSharedPtr<DataRow>(dataTable);
    dataRow->SetGuidValue(signalIDField, GSF::TimeSeries::Guid());
    dataRow->SetStringValue(signalTypeField, string("FREQ"));
    dataTable->AddRow(dataRow);

    dataSet->AddOrUpdateTable(dataTable);

    FilterExpressionParserPtr parser;

    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ'");
    parser->AssignDataSet(dataSet);

    try
    {
        parser->Evaluate();
    }
    catch (FilterExpressionParserException& ex)
    {
        cerr << "FilterExpressionParser exception: " << ex.what() << endl;
    }
    catch (ExpressionTreeException& ex)
    {
        cerr << "ExpressionTree exception: " <<  ex.what() << endl;
    }
    catch (...)
    {
        cerr << boost::current_exception_diagnostic_information(true) << endl;
    }

    cout << "Filtered signal ID count = " << parser->FilteredSignalIDs().size() << endl;

    // Wait until the user presses enter before quitting.
    string line;
    getline(cin, line);

    cout << "Tests complete." << endl;

    return 0;
}