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
#include <boost/filesystem.hpp>

#include "../Transport/FilterExpressions/FilterExpressionParser.h"
#include "../Data/DataSet.h"

using namespace std;
using namespace boost::gregorian;
using namespace boost::posix_time;
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
    catch (const FilterExpressionParserException& ex)
    {
        cerr << "FilterExpressionParser exception: " << ex.what() << endl;
    }
    catch (const ExpressionTreeException& ex)
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
    cout << "Current path: " << boost::filesystem::current_path() << endl << endl;

    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    DataTablePtr dataTable = dataSet->CreateTable("ActiveMeasurements");
    DataColumnPtr dataColumn;
    DataRowPtr dataRow;
    int32_t test = 0;

    dataColumn = dataTable->CreateColumn("SignalID", DataType::Guid);
    dataTable->AddColumn(dataColumn);
    const int32_t signalIDField = dataTable->Column("SignalID")->Index();

    dataColumn = dataTable->CreateColumn("SignalType", DataType::String);
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
    TableIDFieldsPtr activeMeasurementsIDFields = NewSharedPtr<TableIDFields>();

    activeMeasurementsIDFields->SignalIDFieldName = "SignalID";
    activeMeasurementsIDFields->MeasurementKeyFieldName = "ID";
    activeMeasurementsIDFields->PointTagFieldName = "PointTag";    

    // Test 1
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 2
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'STAT'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 3
    parser = NewSharedPtr<FilterExpressionParser>(";;{" + ToString(statID) + "};;;");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 4
    parser = NewSharedPtr<FilterExpressionParser>(ToString(statID));
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 5
    parser = NewSharedPtr<FilterExpressionParser>("'" + ToString(freqID) + "' ; ");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 6
    parser = NewSharedPtr<FilterExpressionParser>("{" + ToString(statID) + "}; {" + ToString(freqID) + "}; {" + ToString(statID) + "};");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 7
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; '" + ToString(statID) + "'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Expressions will parse literals before filter statements
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 8
    parser = NewSharedPtr<FilterExpressionParser>(ToString(freqID) + "; FILTER ActiveMeasurements WHERE SignalType = 'STAT'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 9
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; FILTER ActiveMeasurements WHERE SignalType = 'STAT' ORDER BY SignalID");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 10
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ' OR SignalType = 'STAT'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Row with stat comes before row with freq
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 11
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'FREQ' OR SignalType = 'STAT' ORDER BY SignalType");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // FREQ should sort before STAT with order by
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 12
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType = 'STAT' OR SignalType = 'FREQ' ORDER BY SignalType DESC");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Now descending
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 13
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = '" + ToString(freqID) + "'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 14
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = " + ToString(freqID) + " OR SignalID = {" + ToString(statID) + "}");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // Row with stat comes before row with freq
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 15
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = {" + ToString(statID) + "} OR SignalID = '" + ToString(freqID) + "' ORDER BY SignalType");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);

    // FREQ should sort before STAT with order by
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 16
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID = {" + ToString(statID) + "} OR SignalID = '" + ToString(freqID) + "' ORDER BY SignalType; " + ToString(statID));
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
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
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 18
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE IsNull(NULL, False) OR Coalesce(Null, true)");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 19
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE IIf(IsNull(NULL, False) OR Coalesce(Null, true), Len(SignalType) == 4, false)");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 20
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType IS !NULL");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 21
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE Len(SubStr(Coalesce(Trim(SignalType), 'OTHER'), 0, 0X2)) = 2");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 22
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE LEN(SignalTYPE) > 3.5");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 23
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE Len(SignalType) & 0x4 == 4");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 24
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE RegExVal('ST.+', SignalType) == 'STAT'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 25
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE RegExMatch('FR.+', SignalType)");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 26
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType IN ('FREQ', 'STAT') ORDER BY SignalType");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    assert(parser->FilteredSignalIDs()[1] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 27
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalID IN ({" + ToString(statID) + "}, {" + ToString(freqID) + "})");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 28
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType LIKE 'ST%'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 29
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType LIKE '*EQ'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 30
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType LIKE '*TA%'");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 1);
    assert(parser->FilteredSignalIDs()[0] == statID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 31
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE -Len(SignalType) <= 0");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 32 - number converted to string and compared
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType == 0");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().empty());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 33 - number converted to string and compared
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE SignalType > 99");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredRows(false);
    parser->SetTrackFilteredSignalIDs(true);
    Evaluate(parser);

    assert(parser->FilteredSignalIDs().size() == 2);
    assert(parser->FilteredSignalIDs()[0] == statID);
    assert(parser->FilteredSignalIDs()[1] == freqID);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 34
    parser = NewSharedPtr<FilterExpressionParser>("FILTER ActiveMeasurements WHERE Len(SignalType) / 0x2 = 2");
    parser->SetDataSet(dataSet);
    parser->SetTableIDFields("ActiveMeasurements", activeMeasurementsIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");
    parser->SetTrackFilteredSignalIDs(true);
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

    const string MetadataSampleFileName[2] = { "MetadataSample1.xml", "MetadataSample2.xml" };

    for (int32_t i = 0; i < 2; i++)
    {
        // Prep new dataset
        cout << endl << "Loading XML metadata from \"" << MetadataSampleFileName[i] << "\"..." << endl;

        ifstream metadataStream(MetadataSampleFileName[i], ios::binary);
        metadataStream >> noskipws;
        vector<uint8_t> xmlMetadata;
        CopyStream(metadataStream, xmlMetadata);

        if (xmlMetadata.empty())
        {
            cerr << "Failed to load XML metadata from \"" + MetadataSampleFileName[i] + "\"" << endl;
            return 1;
        }

        cout << "Loaded " << xmlMetadata.size() << " bytes of data." << endl;
        cout << "Parsing XML metadata into data set..." << endl;

        dataSet = nullptr;

        try
        {
            dataSet = DataSet::ParseXmlDataSet(xmlMetadata);
        }
        catch (const DataSetException& ex)
        {
            cerr << "DataSet exception: " <<  ex.what() << endl;
        }
        catch (...)
        {
            cerr << boost::current_exception_diagnostic_information(true) << endl;
        }

        if (dataSet == nullptr)
            return 1;

        cout << "Loaded " << dataSet->TableCount() << " tables from XML metadata." << endl << endl;

        // Test 35 - validate schema load
        assert(dataSet->TableCount() == 4);

        assert(dataSet->Table("MeasurementDetail"));
        assert(dataSet->Table("MeasurementDetail")->ColumnCount() == 11);
        assert(dataSet->Table("MeasurementDetail")->Column("ID"));
        assert(dataSet->Table("MeasurementDetail")->Column("id")->Type() == DataType::String);
        assert(dataSet->Table("MeasurementDetail")->Column("SignalID"));
        assert(dataSet->Table("MeasurementDetail")->Column("signalID")->Type() == DataType::Guid);
        assert(dataSet->Table("MeasurementDetail")->RowCount() > 0);

        assert(dataSet->Table("DeviceDetail"));
        assert(dataSet->Table("DeviceDetail")->ColumnCount() == 19);
        assert(dataSet->Table("DeviceDetail")->Column("Acronym"));
        assert(dataSet->Table("DeviceDetail")->Column("Acronym")->Type() == DataType::String);
        assert(dataSet->Table("DeviceDetail")->Column("Name"));
        assert(dataSet->Table("DeviceDetail")->Column("Name")->Type() == DataType::String);
        assert(dataSet->Table("DeviceDetail")->RowCount() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 36 - validate data load
        assert(IsEqual(
            dataSet->Table("DeviceDetail")->Row(0)->ValueAsString("Acronym").GetValueOrDefault(),
            dataSet->Table("DeviceDetail")->Row(0)->ValueAsString("Name").GetValueOrDefault()
        ));

        // In test data set, DeviceDetail.OriginalSource is null
        assert(!dataSet->Table("DeviceDetail")->Row(0)->ValueAsString("OriginalSource").HasValue());

        // In test data set, DeviceDetail.ParentAcronym is not null, but is an empty string
        assert(dataSet->Table("DeviceDetail")->Row(0)->ValueAsString("ParentAcronym").HasValue());
        assert(static_cast<string>(dataSet->Table("DeviceDetail")->Row(0)->ValueAsString("ParentAcronym").Value).empty());
        cout << "Test " << ++test << " succeeded..." << endl;

        TableIDFieldsPtr measurementDetailIDFields = NewSharedPtr<TableIDFields>();
        measurementDetailIDFields->SignalIDFieldName = "SignalID";
        measurementDetailIDFields->MeasurementKeyFieldName = "ID";
        measurementDetailIDFields->PointTagFieldName = "PointTag";

        // Test 37
        parser = NewSharedPtr<FilterExpressionParser>("FILTER MeasurementDetail WHERE SignalAcronym = 'FREQ'");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("MeasurementDetail", measurementDetailIDFields);
        parser->SetPrimaryTableName("MeasurementDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 38
        parser = NewSharedPtr<FilterExpressionParser>("FILTER TOP 8 MeasurementDetail WHERE SignalAcronym = 'STAT'");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("MeasurementDetail", measurementDetailIDFields);
        parser->SetPrimaryTableName("MeasurementDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 8);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 39
        parser = NewSharedPtr<FilterExpressionParser>("FILTER TOP 0 MeasurementDetail WHERE SignalAcronym = 'STAT'");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("MeasurementDetail", measurementDetailIDFields);
        parser->SetPrimaryTableName("MeasurementDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().empty());
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 40
        parser = NewSharedPtr<FilterExpressionParser>("FILTER TOP -1 MeasurementDetail WHERE SignalAcronym = 'STAT'");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("MeasurementDetail", measurementDetailIDFields);
        parser->SetPrimaryTableName("MeasurementDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(!parser->FilteredSignalIDs().empty());
        cout << "Test " << ++test << " succeeded..." << endl;

        TableIDFieldsPtr deviceDetailIDFields = NewSharedPtr<TableIDFields>();
        deviceDetailIDFields->SignalIDFieldName = "UniqueID";
        deviceDetailIDFields->MeasurementKeyFieldName = "Name";
        deviceDetailIDFields->PointTagFieldName = "Acronym";

        // Test 41
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Convert(Longitude, 'System.Int32') = -89");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 42
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Convert(latitude, 'int16') = 35");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 43
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Convert(Latitude, 'Int32') = '35'");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 44
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Convert(Convert(Latitude, 'Int32'), 'String') = 35");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 45
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Convert(Latitude, 'single') >= 35");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 46 - test decimal comparison
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Longitude < 0.0");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 47
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Acronym IN ('Test', 'Shelby')");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 48
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE Acronym not IN ('Test', 'Apple')");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 49 - unary negative
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE NOT (Acronym IN ('Shelby', 'Apple'))");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().empty());
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 50 - unary positive
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE !Acronym IN ('Test', 'Apple')");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 51 - unary positive (negating negative)
        parser = NewSharedPtr<FilterExpressionParser>("FILTER DeviceDetail WHERE NOT Acronym !IN ('Shelby', 'Apple')");
        parser->SetDataSet(dataSet);
        parser->SetTableIDFields("DeviceDetail", deviceDetailIDFields);
        parser->SetPrimaryTableName("DeviceDetail");
        parser->SetTrackFilteredRows(false);
        parser->SetTrackFilteredSignalIDs(true);
        Evaluate(parser);

        assert(parser->FilteredSignalIDs().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;

        // Test 52
        parser = NewSharedPtr<FilterExpressionParser>("Acronym LIKE 'Shel%'");
        parser->SetDataSet(dataSet);
        parser->SetPrimaryTableName("DeviceDetail");
        Evaluate(parser);

        assert(parser->FilteredRows().size() == 1);
        cout << "Test " << ++test << " succeeded..." << endl;
    }
    // loops 2 times with last test @ 70

    cout << endl << "Basic Expression Tests..." << endl << endl;

    // Test 71
    vector<DataRowPtr> dataRows = FilterExpressionParser::Select(dataSet->Table("MeasurementDetail"), "SignalAcronym = 'STAT'");

    assert(dataRows.size() == 116);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 72
    dataRows = FilterExpressionParser::Select(dataSet->Table("PhasorDetail"), "Type = 'V'");

    assert(dataRows.size() == 2);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 73
    ValueExpressionPtr valueExpression = FilterExpressionParser::Evaluate(dataSet->Table("SchemaVersion")->Row(0), "VersionNumber > 0");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 74
    dataRow = dataSet->Table("DeviceDetail")->Row(0);
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "AccessID % 2 = 0 AND FramesPerSecond % 4 <> 2 OR AccessID % 1 = 0");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 75
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "AccessID % 2 = 0 AND (FramesPerSecond % 4 <> 2 OR -AccessID % 1 = 0)");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 76
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "AccessID % 2 = 0 AND (FramesPerSecond % 4 <> 2 AND AccessID % 1 = 0)");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 77
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "AccessID % 2 >= 0 || (FramesPerSecond % 4 <> 2 AND AccessID % 1 = 0)");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 78
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "AccessID % 2 = 0 OR FramesPerSecond % 4 != 2 && AccessID % 1 == 0");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 79
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "!AccessID % 2 = 0 || FramesPerSecond % 4 = 0x2 && AccessID % 1 == 0");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 80
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "NOT AccessID % 2 = 0 OR FramesPerSecond % 4 >> 0x1 = 1 && AccessID % 1 == 0x0");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 81
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "!AccessID % 2 = 0 OR FramesPerSecond % 4 >> 1 = 1 && AccessID % 3 << 1 & 4 >= 4");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 82
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "OriginalSource IS NULL");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 83
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "ParentAcronym IS NOT NULL");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 84
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "NOT ParentAcronym IS NULL && Len(parentAcronym) == 0");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 85
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "-FramesPerSecond");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == -30);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 86
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "~FramesPerSecond");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == -31);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 87
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "~FramesPerSecond * -1 - 1 << -2");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == -2147483648LL);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 88
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "NOT True");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 89
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "!True");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 90
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "~True");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 91
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "Len(IsNull(OriginalSource, 'A')) = 1");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 92
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "RegExMatch('SH', Acronym)");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 93
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "RegExMatch('SH', Name)");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 94
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "RegExMatch('S[hH]', Name) && RegExMatch('S[hH]', Acronym)");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 95
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "RegExVal('Sh\\w+', Name)");

    assert(valueExpression->ValueType == ExpressionValueType::String);
    assert(IsEqual(valueExpression->ValueAsString(), "Shelby", false));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 96
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "SubStr(RegExVal('Sh\\w+', Name), 2) == 'ElbY'");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 97
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "SubStr(RegExVal('Sh\\w+', Name), 3, 2) == 'lB'");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 98
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "RegExVal('Sh\\w+', Name) IN ('NT', Acronym, 'NT')");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 99
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "RegExVal('Sh\\w+', Name) IN ===('NT', Acronym, 'NT')");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 100
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "RegExVal('Sh\\w+', Name) IN BINARY ('NT', Acronym, 3.05)");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 101
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "Name IN===(0x9F, Acronym, 'Shelby')");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 102
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "Acronym LIKE === 'Sh*'");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 103
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "name LiKe binaRY 'SH%'");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(!valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 104
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "Name === 'Shelby' && Name== 'SHelBy' && Name !=='SHelBy'");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 105
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "Now()");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() <= second_clock::local_time());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 106
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "UtcNow()");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() <= second_clock::universal_time());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 107
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "#2019-02-04T03:00:52.73-05:00#");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(DatePart(valueExpression->ValueAsDateTime(), TimeInterval::Month) == 2);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 108
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "#2019-2-4T3:00:52.73-05:00#");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(DatePart(valueExpression->ValueAsDateTime(), TimeInterval::Day) == 4);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 109
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#02/04/2019T03:00:52.73-05:00#, 'Year')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 2019);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 110
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#02/04/2019 03:00:52.73-05:00#, 'Month')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 2);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 111
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#2019-02-04 03:00:52.73-05:00#, 'Day')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 4);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 112
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#2019-02-04 3:00#, 'Hour')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 3);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 113
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#2019-02-04 3:00:52.73-05:00#, 'Hour')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 8);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 114
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#2/4/2019 3:21:55#, 'Minute')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 21);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 115
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#02/04/2019 03:21:55.33#, 'Second')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 55);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 116
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#02/04/2019 3:21:5.033#, 'Millisecond')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 33);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 117
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(DateAdd('2019-02-04 03:00:52.73-05:00', 1, 'Year'), 'year')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 2020);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 118
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd('2019-02-04', 2, 'Month')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 4, 4)));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 119
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd(#1/31/2019#, 1, 'Day')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 2, 1)));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 120
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd(#2019-01-31#, 2, 'Week')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 2, 14)));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 121
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd(#2019-01-31#, 25, 'Hour')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 2, 1), time_duration(1, 0, 0)));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 122
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd(#2018-12-31 23:58#, 3, 'Minute')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 1, 1), time_duration(0, 1, 0)));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 123
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd('2019-01-1 00:59', 61, 'Second')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 1, 1), time_duration(1, 0, 1)));
    cout << "Test " << ++test << " succeeded..." << endl;

    const float64_t baseFraction = pow(10.0, time_duration::num_fractional_digits());
    #define fracSecond(ms) static_cast<int64_t>((ms) / 1000.0 * baseFraction)

    // Test 124
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd('2019-01-1 00:00:59.999', 2, 'Millisecond')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 1, 1), time_duration(0, 1, 0, fracSecond(1))));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 125
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateAdd(#1/1/2019 0:0:1.029#, -FramesPerSecond, 'Millisecond')");

    assert(valueExpression->ValueType == ExpressionValueType::DateTime);
    assert(valueExpression->ValueAsDateTime() == DateTime(date(2019, 1, 1), time_duration(0, 0, 0, fracSecond(999))));
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 126
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'Year')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 2);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 127
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'month')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 35);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 128
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'DAY')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 1095);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 129
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'Week')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 156);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 130
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'WeekDay')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 1095);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 131
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'Hour')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 26280);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 132
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'Minute')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 1576800);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 133
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2006-01-01 00:00:00#, #2008-12-31 00:00:00#, 'Second')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 94608000);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 134
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DateDiff(#2008-12-30 00:02:50.546#, '2008-12-31', 'Millisecond')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 86229454);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 135
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#2019-02-04 03:00:52.73-05:00#, 'DayOfyear')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 35);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 136
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#2019-02-04 03:00:52.73-05:00#, 'Week')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 6);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 137
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "DatePart(#2019-02-04 03:00:52.73-05:00#, 'WeekDay')");

    assert(valueExpression->ValueType == ExpressionValueType::Int32);
    assert(valueExpression->ValueAsInt32() == 2);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 138
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "IsDate(#2019-02-04 03:00:52.73-05:00#) AND IsDate('2/4/2019') && !ISDATE(2.5) && !IsDate('ImNotADate')");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 139
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "IsInteger(32768) AND IsInteger('1024') and ISinTegeR(FaLsE) && !ISINTEGER(2.5) && !IsInteger('ImNotAnInteger')");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 140
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "IsGuid({9448a8b5-35c1-4dc7-8c42-8712153ac08a}) AND IsGuid('9448a8b5-35c1-4dc7-8c42-8712153ac08a') and isGuid(9448a8b5-35c1-4dc7-8c42-8712153ac08a) AND IsGuid(Convert(9448a8b5-35c1-4dc7-8c42-8712153ac08a, 'string')) && !ISGUID(2.5) && !IsGuid('ImNotAGuid')");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 141
    valueExpression = FilterExpressionParser::Evaluate(dataRow, "IsNumeric(32768) && isNumeric(123.456e-67) AND IsNumeric(3.14159265) and ISnumeric(true) AND IsNumeric('1024' ) and IsNumeric(2.5) && !ISNUMERIC(9448a8b5-35c1-4dc7-8c42-8712153ac08a) && !IsNumeric('ImNotNumeric')");

    assert(valueExpression->ValueType == ExpressionValueType::Boolean);
    assert(valueExpression->ValueAsBoolean());
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 142 - negative tests
    bool result;

    try
    {
        FilterExpressionParser::Evaluate(dataRow, "Convert(123, 'unknown')");
        result = false;
    }
    catch (ExpressionTreeException&)
    {
        result = true;
    }

    assert(result);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Test 143
    try
    {
        FilterExpressionParser::Evaluate(dataRow, "I am a bad expression", true);
        result = false;
    }
    catch (FilterExpressionParserException&)
    {
        result = true;
    }

    assert(result);
    cout << "Test " << ++test << " succeeded..." << endl;

    // Wait until the user presses enter before quitting.
    cout << endl << "Tests complete. Press enter to exit." << endl;
    string line;
    getline(cin, line);

    return 0;
}