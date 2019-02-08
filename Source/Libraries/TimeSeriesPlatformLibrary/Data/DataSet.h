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

#include "../Common/CommonTypes.h"
#include "DataTable.h"

namespace pugi
{
    class xml_document;
}

namespace GSF {
namespace Data
{
    // Simple exception type thrown by data set operations
    class DataSetException : public GSF::Exception
    {
    private:
        std::string m_message;

    public:
        DataSetException(std::string message) noexcept;
        const char* what() const noexcept;
    };

    class DataSet;
    typedef GSF::SharedPtr<DataSet> DataSetPtr;

    class DataSet : public GSF::EnableSharedThisPtr<DataSet> // NOLINT
    {
    private:
        GSF::StringMap<DataTablePtr> m_tables;

        void ParseXml(const pugi::xml_document& document);
        void GenerateXml(pugi::xml_document& document, const std::string& dataSetName) const;

    public:
         DataSet();
        ~DataSet();

        const DataTablePtr& Table(const std::string& tableName) const;

        const DataTablePtr& operator[](const std::string& tableName) const;

        DataTablePtr CreateTable(const std::string& name);

        int32_t TableCount() const;

        std::vector<std::string> TableNames() const;

        std::vector<DataTablePtr> Tables() const;

        bool AddOrUpdateTable(DataTablePtr table);

        bool RemoveTable(const std::string& tableName);

        void ReadXml(const std::string& fileName);
        void ReadXml(const std::vector<uint8_t>& buffer);
        void ReadXml(const uint8_t* buffer, uint32_t length);
        void ReadXml(const pugi::xml_document& document);

        void WriteXml(const std::string& fileName, const std::string& dataSetName = "DataSet") const;
        void WriteXml(std::vector<uint8_t>& buffer, const std::string& dataSetName = "DataSet") const;
        void WriteXml(pugi::xml_document& document, const std::string& dataSetName = "DataSet") const;

        static DataSetPtr FromXml(const std::string& fileName);
        static DataSetPtr FromXml(const std::vector<uint8_t>& buffer);
        static DataSetPtr FromXml(const uint8_t* buffer, uint32_t length);
        static DataSetPtr FromXml(const pugi::xml_document& document);

        static const std::string XmlSchemaNamespace;
        static const std::string ExtXmlSchemaDataNamespace;
    };
}}

#endif