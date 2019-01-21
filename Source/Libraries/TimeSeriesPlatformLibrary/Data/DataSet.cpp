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
#include "../Common/pugixml.hpp"

using namespace std;
using namespace pugi;
using namespace GSF;
using namespace GSF::Data;

DataSetException::DataSetException(string message) noexcept :
    m_message(move(message))
{
}

const char* DataSetException::what() const noexcept
{
    return &m_message[0];
}

const string DataSet::XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

const string DataSet::ExtXmlSchemaDataNamespace = "urn:schemas-microsoft-com:xml-msdata";

DataSet::DataSet() = default;

DataSet::~DataSet() = default;

const DataTablePtr& DataSet::Table(const string& tableName) const
{
    const auto iterator = m_tables.find(tableName);

    if (iterator != m_tables.end())
        return iterator->second;

    return DataTable::NullPtr;
}

const DataTablePtr& DataSet::operator[](const string& tableName) const
{
    return Table(tableName);
}

DataTablePtr DataSet::CreateTable(const string& name)
{
    return NewSharedPtr<DataTable, DataSetPtr, string>(shared_from_this(), name);
}

int32_t DataSet::TableCount() const
{
    return m_tables.size();
}

void DataSet::IterateTables(TableIteratorHandlerFunction iteratorHandler, void* userData)
{
    for (auto const& item : m_tables)
        iteratorHandler(item.second, userData);
}

bool DataSet::AddOrUpdateTable(DataTablePtr table)
{
    // Returns true on insert, false on update
    return m_tables.insert_or_assign(table->Name(), std::move(table)).second;
}

bool DataSet::RemoveTable(const string& tableName)
{
    return m_tables.erase(tableName) > 0;
}

DataSetPtr DataSet::ParseXmlDataSet(const vector<uint8_t>& xmlDataSet)
{
    xml_document document;

    // Load dataset into an XML parser
    const xml_parse_result result = document.load_buffer_inplace(const_cast<uint8_t*>(xmlDataSet.data()), xmlDataSet.size());

    if (result.status != xml_parse_status::status_ok)
        throw DataSetException("Failed to parse dataset XML: " + string(result.description()));

    DataSetPtr dataSet = NewSharedPtr<DataSet>();

    // Find root node
    xml_node rootNode = document.document_element();
    const string rootNodeName = string(rootNode.name());

    // Find schema node
    xml_node schemaNode;

    for (xml_node node = rootNode.first_child(); node; node = node.next_sibling())
    {
        const string name = node.name();

        if (!EndsWith(name, "schema", false))
            continue;

        xml_attribute idAttribute = node.attribute("id");

        if (IsEqual(idAttribute.value(), rootNodeName, false))
        {
            schemaNode = node;
            break;
        }
    }

    if (schemaNode == nullptr)
        throw DataSetException("Failed to parse dataset XML: cannot find schema node for \"" + rootNodeName + "\"");

    string schemaPrefix{}, extSchemaDataPrefix{};

    // Validate schema namespaces
    for (xml_attribute attr = schemaNode.first_attribute(); attr; attr = attr.next_attribute())
    {
        const string name = attr.name();
        const string value = attr.value();

        if (IsEqual(value, XmlSchemaNamespace) && StartsWith(name, "xmlns:"))
            schemaPrefix = name.substr(6) + ":";

        if (IsEqual(value, ExtXmlSchemaDataNamespace) && StartsWith(name, "xmlns:"))
            extSchemaDataPrefix = name.substr(6) + ":";
    }

    if (schemaPrefix.empty())
        throw DataSetException("Failed to parse dataset XML: cannot find schema namespace \"" + XmlSchemaNamespace + "\"");

    if (!StartsWith(schemaNode.name(), schemaPrefix, false))
        throw DataSetException("Failed to parse dataset XML: schema node \"" + string(schemaNode.name()) + "\" prefix is not \"" + schemaPrefix + "\"");

    string extDataTypeAttributeName{};

    if (!extSchemaDataPrefix.empty())
        extDataTypeAttributeName = extSchemaDataPrefix + "DataType";

    xml_attribute nameAttribute, typeAttribute, extDataTypeAttribute;

    // Find element node
    xml_node elementNode;
    const string elementNodeName = schemaPrefix + "element";

    for (xml_node node = schemaNode.first_child(); node; node = node.next_sibling())
    {
        const string name = node.name();

        if (!IsEqual(name, elementNodeName, false))
            continue;
        
        nameAttribute = node.attribute("name");

        if (nameAttribute && IsEqual(nameAttribute.value(), rootNodeName, false))
        {
            elementNode = node;
            break;
        }
    }

    if (elementNode == nullptr)
        throw DataSetException("Failed to parse dataset XML: cannot find schema element node for \"" + rootNodeName + "\"");

    // Find complex-type node - expected to be first child of element node
    xml_node complexTypeNode = elementNode.first_child();

    if (complexTypeNode == nullptr)
        throw DataSetException("Failed to parse dataset XML: cannot find schema element complex-type node for \"" + rootNodeName + "\"");

    if (!IsEqual(complexTypeNode.name(), schemaPrefix + "complexType", false))
        throw DataSetException("Failed to parse dataset XML: unexpected schema element node child encountered \"" + string(complexTypeNode.name()) + "\", expected \"" + schemaPrefix + "complexType\"");

    // Find choice node - expected to be first child of complex-type node
    const xml_node choiceNode = complexTypeNode.first_child();

    if (choiceNode == nullptr)
        throw DataSetException("Failed to parse dataset XML: cannot find schema element complex-type choice node for \"" + rootNodeName + "\"");

    if (!IsEqual(choiceNode.name(), schemaPrefix + "choice", false))
        throw DataSetException("Failed to parse dataset XML: unexpected schema element complex-type node child encountered \"" + string(choiceNode.name()) + "\", expected \"" + schemaPrefix + "choice\"");

    const xml_attribute maxOccursAttribute = choiceNode.attribute("maxOccurs");

    if (maxOccursAttribute == nullptr)
        throw DataSetException("Failed to parse dataset XML: cannot find schema element complex-type choice node maxOccurs attribute value for \"" + rootNodeName + "\"");

    if (!IsEqual(maxOccursAttribute.value(), "unbounded", false))
        throw DataSetException("Failed to parse dataset XML: unexpected schema element complex-type choice node maxOccurs attribute value encountered \"" + string(maxOccursAttribute.value()) + "\", expected \"unbounded\"");

    // Each choice node child element node represents a table definition
    for (elementNode = choiceNode.first_child(); elementNode; elementNode = elementNode.next_sibling())
    {
        if (!IsEqual(elementNode.name(), elementNodeName, false))
            continue;

        nameAttribute = elementNode.attribute("name");

        if (nameAttribute == nullptr)
            continue;
            
        const string tableName = nameAttribute.value();

        if (tableName.empty())
            continue;

        // Find complex-type node - expected to be first child of element node
        complexTypeNode = elementNode.first_child();

        if (complexTypeNode == nullptr)
            continue;

        if (!IsEqual(complexTypeNode.name(), schemaPrefix + "complexType", false))
            continue;

        // Find sequence node - expected to be first child of complex-type node
        const xml_node sequenceNode = complexTypeNode.first_child();

        if (sequenceNode == nullptr)
            continue;

        if (!IsEqual(sequenceNode.name(), schemaPrefix + "sequence", false))
            continue;

        const DataTablePtr dataTable = NewSharedPtr<DataTable>(dataSet, tableName);

        // Each sequence node child element node represents a table field definition
        for (xml_node node = sequenceNode.first_child(); node; node = node.next_sibling())
        {
            const string name = node.name();

            if (!IsEqual(name, elementNodeName, false))
                continue;

            nameAttribute = node.attribute("name");

            if (nameAttribute == nullptr)
                continue;

            const string columnName = nameAttribute.value();

            if (columnName.empty())
                continue;

            typeAttribute = node.attribute("type");

            if (typeAttribute == nullptr)
                continue;

            string typeName = typeAttribute.value();

            if (typeName.empty())
                continue;

            // Remove schema prefix from type name
            if (StartsWith(typeName, schemaPrefix))
                typeName = typeName.substr(schemaPrefix.size());

            string extDataTypeName{};

            if (!extDataTypeAttributeName.empty())
            {
                extDataTypeAttribute = node.attribute(extDataTypeAttributeName.c_str());

                if (extDataTypeAttribute)
                    extDataTypeName = extDataTypeAttribute.value();
            }

            DataType columnDataType;

            if (IsEqual(typeName, "string", false))
            {
                if (!extDataTypeName.empty() && StartsWith(extDataTypeName, "System.Guid", false))
                    columnDataType = DataType::Guid;
                else
                    columnDataType = DataType::String;
            }
            else if (IsEqual(typeName, "boolean", false))
            {
                columnDataType = DataType::Boolean;
            }
            else if (IsEqual(typeName, "dateTime", false))
            {
                columnDataType = DataType::DateTime;
            }
            else if (IsEqual(typeName, "float", false))
            {
                columnDataType = DataType::Single;
            }
            else if (IsEqual(typeName, "double", false))
            {
                columnDataType = DataType::Double;
            }
            else if (IsEqual(typeName, "decimal", false))
            {
                columnDataType = DataType::Decimal;
            }
            else if (IsEqual(typeName, "byte", false))
            {
                columnDataType = DataType::Int8;
            }
            else if (IsEqual(typeName, "short", false))
            {
                columnDataType = DataType::Int16;
            }
            else if (IsEqual(typeName, "int", false))
            {
                columnDataType = DataType::Int32;
            }
            else if (IsEqual(typeName, "long", false))
            {
                columnDataType = DataType::Int64;
            }
            else if (IsEqual(typeName, "unsignedByte", false))
            {
                columnDataType = DataType::UInt8;
            }
            else if (IsEqual(typeName, "unsignedShort", false))
            {
                columnDataType = DataType::UInt16;
            }
            else if (IsEqual(typeName, "unsignedInt", false))
            {
                columnDataType = DataType::UInt32;
            }
            else if (IsEqual(typeName, "unsignedLong", false))
            {
                columnDataType = DataType::UInt64;
            }
            else
            {
                // Columns with unsupported XMLSchema data types are skipped,
                // full list here: https://www.w3.org/TR/xmlschema-2/
                continue;
            }

            // Create column 
            const DataColumnPtr dataColumn = dataTable->CreateColumn(columnName, columnDataType);
            dataTable->AddColumn(dataColumn);
        }

        dataSet->AddOrUpdateTable(dataTable);
    }

    // Each root node child that matches a table name represents a record
    for (xml_node recordNode = rootNode.first_child(); recordNode; recordNode = recordNode.next_sibling())
    {
        const DataTablePtr dataTable = dataSet->Table(recordNode.name());

        if (dataTable == nullptr)
            continue;

        const DataRowPtr dataRow = dataTable->CreateRow();

        // Each record node child represents a field value
        for (xml_node fieldNode = recordNode.first_child(); fieldNode; fieldNode = fieldNode.next_sibling())
        {
            const DataColumnPtr dataColumn = dataTable->Column(fieldNode.name());

            if (dataColumn == nullptr)
                continue;

            const int32_t index = dataColumn->Index();
            const xml_text value = fieldNode.text();
            
            switch (dataColumn->Type())
            {
                case DataType::String:                    
                    dataRow->SetStringValue(index, string(value.as_string("")));
                    break;
                case DataType::Boolean:
                    dataRow->SetBooleanValue(index, value.as_bool());
                    break;
                case DataType::DateTime:
                    if (value.empty())
                        dataRow->SetNullValue(index);
                    else
                        dataRow->SetDateTimeValue(index, ParseTimestamp(value.as_string()));
                    break;
                case DataType::Single:
                    dataRow->SetSingleValue(index, value.as_float());
                    break;
                case DataType::Double:
                    dataRow->SetDoubleValue(index, value.as_double());
                    break;
                case DataType::Decimal:
                    if (value.empty())
                        dataRow->SetDecimalValue(index, decimal_t(0));
                    else
                        dataRow->SetDecimalValue(index, decimal_t(value.as_string()));
                    break;
                case DataType::Guid:
                    if (value.empty())
                        dataRow->SetNullValue(index);
                    else
                        dataRow->SetGuidValue(index, ParseGuid(value.as_string()));
                    break;
                case DataType::Int8:
                    dataRow->SetInt8Value(index, static_cast<int8_t>(value.as_int()));
                    break;
                case DataType::Int16:
                    dataRow->SetInt16Value(index, static_cast<int16_t>(value.as_int()));
                    break;
                case DataType::Int32:
                    dataRow->SetInt32Value(index, value.as_int());
                    break;
                case DataType::Int64:
                    dataRow->SetInt64Value(index, value.as_llong());
                    break;
                case DataType::UInt8:
                    dataRow->SetUInt8Value(index, static_cast<uint8_t>(value.as_uint()));
                    break;
                case DataType::UInt16:
                    dataRow->SetUInt16Value(index, static_cast<uint16_t>(value.as_uint()));
                    break;
                case DataType::UInt32:
                    dataRow->SetUInt32Value(index, value.as_uint());
                    break;
                case DataType::UInt64:
                    dataRow->SetUInt64Value(index, value.as_ullong());
                    break;
                default:
                    throw DataSetException("Unexpected column data type encountered");
            }
        }

        dataTable->AddRow(dataRow);
    }

    return dataSet;
}

vector<uint8_t> DataSet::GenerateXmlDataSet(const DataSetPtr& dataSet, const string& dataSetName)
{
    xml_document document;
    const char* rootNodeName = dataSetName.c_str();

    xml_node rootNode = document.append_child(rootNodeName);

    // <xs:schema id="DataSet" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:ext="urn:schemas-microsoft-com:xml-msdata">
    xml_node schemaNode = rootNode.append_child("xs:schema");

    schemaNode.append_attribute("id") = rootNodeName;
    schemaNode.append_attribute("xmlns:xs") = XmlSchemaNamespace.c_str();
    schemaNode.append_attribute("xmlns:ext") = ExtXmlSchemaDataNamespace.c_str();

    vector<uint8_t> xmlDataSet;

    return xmlDataSet;
}
