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

class XMLVectorWriter: public xml_writer
{
private:
    vector<uint8_t>& m_target;

public:
    XMLVectorWriter(vector<uint8_t>& target) :
        m_target(target)
    {
    }

    void write(const void* data, size_t size) override
    {
        const char* chunk = static_cast<const char*>(data);

        for (size_t i = 0; i < size; i++)
            m_target.push_back(static_cast<uint8_t>(chunk[i]));
    }
};

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

vector<string> DataSet::TableNames() const
{
    vector<string> tableNames;

    for (auto const& item : m_tables)
        tableNames.push_back(item.first);

    return tableNames;
}

vector<DataTablePtr> DataSet::Tables() const
{
    vector<DataTablePtr> tables;

    for (auto const& item : m_tables)
        tables.push_back(item.second);

    return tables;
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

void DataSet::ReadXml(const string& fileName)
{
    xml_document document;

    const xml_parse_result result = document.load_file(fileName.c_str());

    if (result.status != xml_parse_status::status_ok)
        throw DataSetException("Failed to load XML from file: " + string(result.description()));

    ParseXml(document);
}

void DataSet::ReadXml(const vector<uint8_t>& buffer)
{
    ReadXml(buffer.data(), buffer.size());
}

void DataSet::ReadXml(const uint8_t* buffer, uint32_t length)
{
    xml_document document;

    const xml_parse_result result = document.load_buffer_inplace(const_cast<uint8_t*>(buffer), length);

    if (result.status != xml_parse_status::status_ok)
        throw DataSetException("Failed to load XML from buffer: " + string(result.description()));

    ParseXml(document);
}

void DataSet::ReadXml(const pugi::xml_document& document)
{
    ParseXml(document);
}

void DataSet::WriteXml(const string& fileName, const string& dataSetName) const
{
    xml_document document;    
    GenerateXml(document, dataSetName);
    document.save_file(fileName.c_str(), "  ", format_default | format_save_file_text);
}

void DataSet::WriteXml(vector<uint8_t>& buffer, const string& dataSetName) const
{
    xml_document document;
    XMLVectorWriter writer(buffer);
    GenerateXml(document, dataSetName);
    document.save(writer, "  ");
}

void DataSet::WriteXml(pugi::xml_document& document, const std::string& dataSetName) const
{
    GenerateXml(document, dataSetName);
}

DataSetPtr DataSet::FromXml(const std::string& fileName)
{
    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    dataSet->ReadXml(fileName);
    return dataSet;
}

DataSetPtr DataSet::FromXml(const std::vector<uint8_t>& buffer)
{
    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    dataSet->ReadXml(buffer);
    return dataSet;
}

DataSetPtr DataSet::FromXml(const uint8_t* buffer, uint32_t length)
{
    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    dataSet->ReadXml(buffer, length);
    return dataSet;}

DataSetPtr DataSet::FromXml(const pugi::xml_document& document)
{
    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    dataSet->ReadXml(document);
    return dataSet;
}

void DataSet::ParseXml(const xml_document& document)
{
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

    string extDataTypeAttributeName{}, extExpressionAttributeName{};

    if (!extSchemaDataPrefix.empty())
    {
        extDataTypeAttributeName = extSchemaDataPrefix + "DataType";
        extExpressionAttributeName = extSchemaDataPrefix + "Expression";
    }

    xml_attribute nameAttribute, typeAttribute, extDataTypeAttribute, extExpressionAttribute;

    // Find schema element node
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

        const DataTablePtr dataTable = CreateTable(tableName);

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

            string extDataType{};

            if (!extDataTypeAttributeName.empty())
            {
                extDataTypeAttribute = node.attribute(extDataTypeAttributeName.c_str());

                if (extDataTypeAttribute)
                    extDataType = extDataTypeAttribute.value();
            }

            string columnExpression{};

            if (!extExpressionAttributeName.empty())
            {
                extExpressionAttribute = node.attribute(extExpressionAttributeName.c_str());

                if (extExpressionAttribute)
                    columnExpression = extExpressionAttribute.value();
            }

            DataType dataType;

            if (IsEqual(typeName, "string", false))
            {
                if (!extDataType.empty() && StartsWith(extDataType, "System.Guid", false))
                    dataType = DataType::Guid;
                else
                    dataType = DataType::String;
            }
            else if (IsEqual(typeName, "boolean", false))
            {
                dataType = DataType::Boolean;
            }
            else if (IsEqual(typeName, "dateTime", false))
            {
                dataType = DataType::DateTime;
            }
            else if (IsEqual(typeName, "float", false))
            {
                dataType = DataType::Single;
            }
            else if (IsEqual(typeName, "double", false))
            {
                dataType = DataType::Double;
            }
            else if (IsEqual(typeName, "decimal", false))
            {
                dataType = DataType::Decimal;
            }
            else if (IsEqual(typeName, "byte", false))
            {
                dataType = DataType::Int8;
            }
            else if (IsEqual(typeName, "short", false))
            {
                dataType = DataType::Int16;
            }
            else if (IsEqual(typeName, "int", false))
            {
                dataType = DataType::Int32;
            }
            else if (IsEqual(typeName, "long", false))
            {
                dataType = DataType::Int64;
            }
            else if (IsEqual(typeName, "unsignedByte", false))
            {
                dataType = DataType::UInt8;
            }
            else if (IsEqual(typeName, "unsignedShort", false))
            {
                dataType = DataType::UInt16;
            }
            else if (IsEqual(typeName, "unsignedInt", false))
            {
                dataType = DataType::UInt32;
            }
            else if (IsEqual(typeName, "unsignedLong", false))
            {
                dataType = DataType::UInt64;
            }
            else
            {
                // Columns with unsupported XMLSchema data types are skipped,
                // full list here: https://www.w3.org/TR/xmlschema-2/
                continue;
            }

            // Create column 
            const DataColumnPtr dataColumn = dataTable->CreateColumn(columnName, dataType, columnExpression);
            dataTable->AddColumn(dataColumn);
        }

        AddOrUpdateTable(dataTable);
    }

    // Each root node child that matches a table name represents a record
    for (xml_node recordNode = rootNode.first_child(); recordNode; recordNode = recordNode.next_sibling())
    {
        const DataTablePtr table = Table(recordNode.name());

        if (table == nullptr)
            continue;

        const DataRowPtr row = table->CreateRow();

        // Each record node child represents a field value
        for (xml_node fieldNode = recordNode.first_child(); fieldNode; fieldNode = fieldNode.next_sibling())
        {
            const DataColumnPtr column = table->Column(fieldNode.name());

            if (column == nullptr)
                continue;

            const int32_t columnIndex = column->Index();
            const xml_text nodeText = fieldNode.text();
            
            switch (column->Type())
            {
                case DataType::String:                    
                    row->SetStringValue(columnIndex, string(nodeText.as_string("")));
                    break;
                case DataType::Boolean:
                    row->SetBooleanValue(columnIndex, nodeText.as_bool());
                    break;
                case DataType::DateTime:
                    if (nodeText.empty())
                        row->SetDateTimeValue(columnIndex, Empty::DateTime);
                    else
                        row->SetDateTimeValue(columnIndex, ParseTimestamp(nodeText.as_string()));
                    break;
                case DataType::Single:
                    row->SetSingleValue(columnIndex, nodeText.as_float());
                    break;
                case DataType::Double:
                    row->SetDoubleValue(columnIndex, nodeText.as_double());
                    break;
                case DataType::Decimal:
                    if (nodeText.empty())
                        row->SetDecimalValue(columnIndex, decimal_t(0));
                    else
                        row->SetDecimalValue(columnIndex, decimal_t(nodeText.as_string()));
                    break;
                case DataType::Guid:
                    if (nodeText.empty())
                        row->SetGuidValue(columnIndex, Empty::Guid);
                    else
                        row->SetGuidValue(columnIndex, ParseGuid(nodeText.as_string()));
                    break;
                case DataType::Int8:
                    row->SetInt8Value(columnIndex, static_cast<int8_t>(nodeText.as_int()));
                    break;
                case DataType::Int16:
                    row->SetInt16Value(columnIndex, static_cast<int16_t>(nodeText.as_int()));
                    break;
                case DataType::Int32:
                    row->SetInt32Value(columnIndex, nodeText.as_int());
                    break;
                case DataType::Int64:
                    row->SetInt64Value(columnIndex, nodeText.as_llong());
                    break;
                case DataType::UInt8:
                    row->SetUInt8Value(columnIndex, static_cast<uint8_t>(nodeText.as_uint()));
                    break;
                case DataType::UInt16:
                    row->SetUInt16Value(columnIndex, static_cast<uint16_t>(nodeText.as_uint()));
                    break;
                case DataType::UInt32:
                    row->SetUInt32Value(columnIndex, nodeText.as_uint());
                    break;
                case DataType::UInt64:
                    row->SetUInt64Value(columnIndex, nodeText.as_ullong());
                    break;
                default:
                    throw DataSetException("Unexpected column data type encountered");
            }
        }

        table->AddRow(row);
    }
}

void DataSet::GenerateXml(xml_document& document, const string& dataSetName) const
{
    static const char* schemaNodeName = "xs:schema";
    static const char* elementNodeName = "xs:element";
    static const char* complexNodeName = "xs:complexType";
    static const char* choiceNodeName = "xs:choice";
    static const char* sequenceNodeName = "xs:sequence";
    static const char* extDataTypeAttributeName = "ext:DataType";
    static const char* extExpressionAttributeName = "ext:Expression";

    const char* rootNodeName = dataSetName.c_str();
    const vector<DataTablePtr> tables = Tables();

    // <?xml version="1.0" standalone="yes"?>
    xml_node declaration = document.prepend_child(node_declaration);
    declaration.append_attribute("version") = "1.0";
    declaration.append_attribute("standalone") = "yes";

    // <DataSet>
    xml_node rootNode = document.append_child(rootNodeName);

    //   <xs:schema id="DataSet" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:ext="urn:schemas-microsoft-com:xml-msdata">
    xml_node schemaNode = rootNode.append_child(schemaNodeName);
    schemaNode.append_attribute("id") = rootNodeName;
    schemaNode.append_attribute("xmlns:xs") = XmlSchemaNamespace.c_str();
    schemaNode.append_attribute("xmlns:ext") = ExtXmlSchemaDataNamespace.c_str();

    //     <xs:element name="DataSet">
    xml_node elementNode = schemaNode.append_child(elementNodeName);
    elementNode.append_attribute("name") = rootNodeName;

    //       <xs:complexType>
    xml_node complexNode = elementNode.append_child(complexNodeName);

    //         <xs:choice minOccurs="0" maxOccurs="unbounded">
    xml_node choiceNode = complexNode.append_child(choiceNodeName);
    choiceNode.append_attribute("minOccurs") = "0";
    choiceNode.append_attribute("maxOccurs") = "unbounded";

    // Write schema definition for each table
    for (auto const& table : tables)
    {
        //       <xs:element name="TableName">
        elementNode = choiceNode.append_child(elementNodeName);
        elementNode.append_attribute("name") = table->Name().c_str();

        //         <xs:complexType>
        complexNode = elementNode.append_child(complexNodeName);

        //           <xs:sequence>
        xml_node sequenceNode = complexNode.append_child(sequenceNodeName);

        // Write schema definition for each table field
        for (int32_t columnIndex = 0; columnIndex < table->ColumnCount(); columnIndex++)
        {
            const DataColumnPtr& column = table->Column(columnIndex);

            //         <xs:element name="FieldName" type="xs:string" minOccurs="0" />
            elementNode = sequenceNode.append_child(elementNodeName);
            elementNode.append_attribute("name") = column->Name().c_str();

            // Map DataType to XML schema type
            const char* dataType = [column]()
            {
                switch (column->Type())
                {
                    case DataType::String:
                    case DataType::Guid:
                        return "xs:string";
                    case DataType::Boolean:
                        return "xs:boolean";
                    case DataType::DateTime:
                        return "xs:dateTime";
                    case DataType::Single:
                        return "xs:float";
                    case DataType::Double:
                        return "xs:double";
                    case DataType::Decimal:
                        return "xs:decimal";
                    case DataType::Int8:
                        return "xs:byte";
                    case DataType::Int16:
                        return "xs:short";
                    case DataType::Int32:
                        return "xs:int";
                    case DataType::Int64:
                        return "xs:long";
                    case DataType::UInt8:
                        return "xs:unsignedByte";
                    case DataType::UInt16:
                        return "xs:unsignedShort";
                    case DataType::UInt32:
                        return "xs:unsignedInt";
                    case DataType::UInt64:
                        return "xs:unsignedLong";
                    default:
                        throw DataSetException("Unexpected column data type encountered");
                }
            }();

            // Guid is an extended schema data type: ext:DataType="System.Guid"
            if (column->Type() == DataType::Guid)
                elementNode.append_attribute(extDataTypeAttributeName) = "System.Guid";

            // Computed columns define an expression: ext:Expression="FieldNameA + FieldNameB"
            if (column->Computed())
                elementNode.append_attribute(extExpressionAttributeName) = column->Expression().c_str();

            elementNode.append_attribute("type") = dataType;
            elementNode.append_attribute("minOccurs") = "0";
        }
    }

    // Write records for each table
    for (auto const& table : tables)
    {
        const char* tableName = table->Name().c_str();

        for (int32_t rowIndex = 0; rowIndex < table->RowCount(); rowIndex++)
        {
            const DataRowPtr& row = table->Row(rowIndex);

            if (row == nullptr)
                continue;

            xml_node recordNode = rootNode.append_child(tableName);

            for (int32_t columnIndex = 0; columnIndex < table->ColumnCount(); columnIndex++)
            {
                // Null records are not written into the XML document
                if (row->IsNull(columnIndex))
                    continue;

                const DataColumnPtr& column = table->Column(columnIndex);

                // Computed records are not written into the XML document
                if (column->Computed())
                    continue;

                xml_node fieldNode = recordNode.append_child(column->Name().c_str());
                xml_text nodeText = fieldNode.text();

                switch (column->Type())
                {
                    case DataType::String:
                    {
                        auto result = row->ValueAsString(columnIndex);
                        nodeText.set(result.GetValueOrDefault().c_str());
                        break;
                    }
                    case DataType::Boolean:
                    {
                        auto result = row->ValueAsBoolean(columnIndex);
                        nodeText.set(result.GetValueOrDefault() ? "true" : "false");
                        break;
                    }
                    case DataType::DateTime:
                    {
                        auto result = row->ValueAsDateTime(columnIndex);
                        string dateTime = ToString(result.GetValueOrDefault(), "%Y-%m-%dT%H:%M:%S%F");

                        if (Contains(dateTime, ".", false))
                            dateTime = TrimRight(dateTime, "0");

                        dateTime.append("Z");

                        nodeText.set(dateTime.c_str());
                        break;
                    }
                    case DataType::Single:
                    {
                        auto result = row->ValueAsSingle(columnIndex);
                        nodeText.set(result.GetValueOrDefault());
                        break;
                    }
                    case DataType::Double:
                    {
                        auto result = row->ValueAsDouble(columnIndex);
                        nodeText.set(result.GetValueOrDefault());
                        break;
                    }
                    case DataType::Decimal:
                    {
                        auto result = row->ValueAsDecimal(columnIndex);
                        nodeText.set(result.GetValueOrDefault().str().c_str());
                        break;
                    }
                    case DataType::Guid:
                    {
                        auto result = row->ValueAsGuid(columnIndex);
                        nodeText.set(ToString(result.GetValueOrDefault()).c_str());
                        break;
                    }
                    case DataType::Int8:
                    {
                        auto result = row->ValueAsInt8(columnIndex);
                        nodeText.set(static_cast<int>(result.GetValueOrDefault()));
                        break;
                    }
                    case DataType::Int16:
                    {
                        auto result = row->ValueAsInt16(columnIndex);
                        nodeText.set(static_cast<int>(result.GetValueOrDefault()));
                        break;
                    }
                    case DataType::Int32:
                    {
                        auto result = row->ValueAsInt32(columnIndex);
                        nodeText.set(result.GetValueOrDefault());
                        break;
                    }
                    case DataType::Int64:
                    {
                        auto result = row->ValueAsInt64(columnIndex);
                        nodeText.set(result.GetValueOrDefault());
                        break;
                    }
                    case DataType::UInt8:
                    {
                        auto result = row->ValueAsUInt8(columnIndex);
                        nodeText.set(static_cast<unsigned>(result.GetValueOrDefault()));
                        break;
                    }
                    case DataType::UInt16:
                    {
                        auto result = row->ValueAsUInt16(columnIndex);
                        nodeText.set(static_cast<unsigned>(result.GetValueOrDefault()));
                        break;
                    }
                    case DataType::UInt32:
                    {
                        auto result = row->ValueAsUInt32(columnIndex);
                        nodeText.set(result.GetValueOrDefault());
                        break;
                    }
                    case DataType::UInt64:
                    {
                        auto result = row->ValueAsUInt64(columnIndex);
                        nodeText.set(result.GetValueOrDefault());
                        break;
                    }
                    default:
                        throw DataSetException("Unexpected column data type encountered");
                }
            }
        }
    }
}