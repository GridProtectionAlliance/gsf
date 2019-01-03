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

void Evaluate(const FilterExpressionParserPtr& parser)
{
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
}

// Sample application to test the filter expression parser.
int main(int argc, char* argv[])
{
    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    DataTablePtr dataTable = NewSharedPtr<DataTable>(dataSet, "ActiveMeasurements");
    DataColumnPtr dataColumn;
    DataRowPtr dataRow;
    int32_t test = 0;

    dataColumn = NewSharedPtr<DataColumn>(dataTable, "SignalID", DataType::Guid);
    dataTable->AddColumn(dataColumn);
    const int32_t signalIDField = dataTable->Column("SignalID")->Index();

    dataColumn = NewSharedPtr<DataColumn>(dataTable, "SignalType", DataType::String);
    dataTable->AddColumn(dataColumn);
    const int32_t signalTypeField = dataTable->Column("SignalType")->Index();

    const GSF::TimeSeries::Guid statID = NewGuid();
    dataRow = NewSharedPtr<DataRow>(dataTable);
    dataRow->SetGuidValue(signalIDField, statID);
    dataRow->SetStringValue(signalTypeField, string("STAT"));
    dataTable->AddRow(dataRow);
    cout << "Row 1 Statistic SignalID = " << ToString(statID) << endl;

    const GSF::TimeSeries::Guid freqID = NewGuid();
    dataRow = NewSharedPtr<DataRow>(dataTable);
    dataRow->SetGuidValue(signalIDField, freqID);
    dataRow->SetStringValue(signalTypeField, string("FREQ"));
    dataTable->AddRow(dataRow);
    cout << "Row 2 Frequency SignalID = " << ToString(freqID) << endl;

    dataSet->AddOrUpdateTable(dataTable);
    cout << endl;

    FilterExpressionParserPtr parser;

    // Test 1
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 2
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'STAT'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 3
    parser = NewSharedPtr<FilterExpressionParser>(";;{" + ToString(statID) + "};;;");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 4
    parser = NewSharedPtr<FilterExpressionParser>(ToString(statID));
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 5
    parser = NewSharedPtr<FilterExpressionParser>("'" + ToString(freqID) + "' ; ");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 6
    parser = NewSharedPtr<FilterExpressionParser>("{" + ToString(statID) + "}; {" + ToString(freqID) + "}; {" + ToString(statID) + "};");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 7
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; '" + ToString(statID) + "'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Expressions will parse literals before filter statements
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 8
    parser = NewSharedPtr<FilterExpressionParser>(ToString(freqID) + "; FILTER ActiveMeasurements WHERE SignalType = 'STAT'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 9
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; FILTER ActiveMeasurements WHERE SignalType = 'STAT' ORDER BY SignalID");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 10
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ' OR SignalType = 'STAT'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Row with stat comes before row with freq
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 11
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ' OR SignalType = 'STAT' ORDER BY SignalType");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // FREQ should sort before STAT with order by
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 12
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'STAT' OR SignalType = 'FREQ' ORDER BY SignalType DESC");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Now descending
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 13
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = '" + ToString(freqID) + "'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 14
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = " + ToString(freqID) + " OR SignalID = {" + ToString(statID) + "}");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Row with stat comes before row with freq
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 15
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = {" + ToString(statID) + "} OR SignalID = '" + ToString(freqID) + "' ORDER BY SignalType");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // FREQ should sort before STAT with order by
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 16
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = {" + ToString(statID) + "} OR SignalID = '" + ToString(freqID) + "' ORDER BY SignalType; " + ToString(statID));
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Because expression includes STAT as a literal (at the end), it will parse first
    // regardless of order by in filter statement
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 17
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE True");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 18
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE IsNull(NULL, False) OR Coalesce(Null, true)");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 19
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE IIf(IsNull(NULL, False) OR Coalesce(Null, true), Len(SignalType) == 4, false)");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 20
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType IS NOT NULL");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 21
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE Len(SubStr(Coalesce(Trim(SignalType), 'OTHER'), 0, 2)) = 2");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 22
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE LEN(SignalTYPE) > 3.5");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 23
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE Len(SignalType) & 4 == 4");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 24
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE RegExVal('ST.+', SignalType) == 'STAT'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 25
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE IsRegExMatch('FR.+', SignalType)");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 26
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType IN ('FREQ', 'STAT') ORDER BY SignalType");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 27
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID IN ({" + ToString(statID) + "}, {" + ToString(freqID) + "})");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 28
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType LIKE 'ST%'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 29
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType LIKE '*EQ'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 30
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType LIKE '*TA%'");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 31
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE -Len(SignalType) <= 0");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 32 - number converted to string and compared
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType == 0");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 0);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 33 - number converted to string and compared
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType > 99");
    parser->SetDataSet(dataSet);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 34
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE Len(SignalType) / 2 = 2");
    parser->SetDataSet(dataSet);
    parser->SetTrackFilteredRows(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    assert(parser->FilteredRows().size() == 2);
    assert(parser->FilteredRows()[0]->ValueAsGuid(signalIDField).GetValueOrDefault() == statID);
    assert(parser->FilteredRows()[0]->ValueAsString(signalTypeField).GetValueOrDefault() == "STAT");
    assert(parser->FilteredRows()[1]->ValueAsGuid(signalIDField).GetValueOrDefault() == freqID);
    assert(parser->FilteredRows()[1]->ValueAsString(signalTypeField).GetValueOrDefault() == "FREQ");
    cout << "Test " << ++test << " succeeded..." << endl;

    // Wait until the user presses enter before quitting.
    cout << endl << "Tests complete. Press enter to exit." << endl;
    string line;
    getline(cin, line);

    return 0;
}