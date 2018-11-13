//******************************************************************************************************
//  SqlGenerator.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/28/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/15/2008 - Mihir Brahmbhatt
//       Converted to C# extensions.
//  09/27/2010 - Mihir Brahmbhatt
//       Edited code comments.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Data;
using System.Text;
using GSF.Data;

namespace DataMigrationUtility
{
    /// <summary>
    /// Common database functions
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Converts value to string, null objects (or DBNull objects) will return an empty string (""). 
        /// </summary>
        /// <param name="value">Value of <see cref="object"/> to convert to string.</param>
        /// <param name="nonNullValue"><see cref="string"/> to return if <paramref name="value"/> is null.</param>
        /// <returns><paramref name="value"/> as a string; if <paramref name="value"/> is null, <paramref name="nonNullValue"/> will be returned.</returns>
        public static string NotNull(object value, string nonNullValue = null)
        {
            if ((object)nonNullValue == null)
                return GSF.Common.ToNonNullString(value);

            return GSF.Common.ToNonNullString(value, nonNullValue);
        }
    }

    /// <summary>
    /// Generates a SQL statement for data processing.
    /// </summary>
    public class SQLGenerator
    {
        #region [ Members ]

        private Schema m_sourceSchema;
        private Table m_currentTable;
        private readonly ArrayList m_fieldList = new ArrayList();
        private string m_whereSQL;
        private bool m_includeNulls;

        #endregion

        #region [ Constructors ]

        public SQLGenerator()
        {
            m_whereSQL = "";
            m_includeNulls = false;
        }

        public SQLGenerator(Schema SourceSchema)
        {
            m_sourceSchema = SourceSchema;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or set Source <see cref="Schema"/> object to use for SQL generation.
        /// </summary>
        public Schema SourceSchema
        {
            get
            {
                return m_sourceSchema;
            }
            set
            {
                m_sourceSchema = value;
            }
        }

        /// <summary>
        /// Get or set - WHERE clause SQL to append to generated SQL statements.
        /// </summary>
        public string WhereSQL
        {
            get
            {
                return m_whereSQL;
            }
            set
            {
                m_whereSQL = value;
            }
        }

        /// <summary>
        /// Get or set - to include NULL values in generated SQL statements.
        /// </summary>
        public bool IncludeNulls
        {
            get
            {
                return m_includeNulls;
            }
            set
            {
                m_includeNulls = value;
            }
        }


        /// <summary>
        /// Get <see cref="IDbConnection"/> setting for source <see cref="Schema"/>
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return m_sourceSchema.Connection;
            }
        }

        /// <summary>
        /// Get or set source <see cref="Schema"/> table name.
        /// </summary>
        public string TableName
        {
            get
            {
                return m_currentTable.Name;
            }
            set
            {
                // Lookup table in collection
                m_currentTable = m_sourceSchema.Tables[value];

                if ((object)m_currentTable != null)
                {
                    // Clear any existing field values
                    // Field field = default(Field);
                    foreach (Field field in m_currentTable.Fields)
                    {
                        field.Value = null;
                    }

                    m_fieldList.Clear();
                }
                else
                {
                    throw new InvalidOperationException("Table '" + value + "' not found in schema");
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// set value to current table column value
        /// </summary>
        /// <param name="FieldName">Set <paramref name="FieldValue"/> to <see cref="SourceSchema"/> table's field value</param>
        /// <param name="FieldValue">Set <see cref="object"/> value to <paramref name="FieldName"/> of current table</param>
        public void SetField(string FieldName, object FieldValue)
        {
            m_currentTable.Fields[FieldName].Value = FieldValue;

            if (m_fieldList.BinarySearch(FieldName, CaseInsensitiveComparer.Default) < 0)
            {
                m_fieldList.Add(FieldName);
                m_fieldList.Sort(CaseInsensitiveComparer.Default);
            }
        }

        /// <summary>
        /// Generate insert SQL statement
        /// </summary>
        /// <returns>insert SQL statement for current table to process</returns>
        public string InsertSQL()
        {
            StringBuilder insertSQL = new StringBuilder();
            string fieldName;
            bool setComma = false;

            insertSQL.Append("INSERT INTO ");
            insertSQL.Append(m_currentTable.SQLEscapedName);
            insertSQL.Append(" (");

            foreach (string currentFieldName in m_fieldList)
            {
                fieldName = currentFieldName;

                if (m_includeNulls || !Convert.IsDBNull(m_currentTable.Fields[fieldName].Value))
                {
                    if (setComma)
                        insertSQL.Append(", ");

                    insertSQL.Append(m_currentTable.Fields[fieldName].SQLEscapedName);
                    setComma = true;
                }
            }

            insertSQL.Append(") VALUES (");
            setComma = false;

            foreach (string currentFieldName in m_fieldList)
            {
                fieldName = currentFieldName;

                if (m_includeNulls || !Convert.IsDBNull(m_currentTable.Fields[fieldName].Value))
                {
                    if (setComma)
                        insertSQL.Append(", ");

                    insertSQL.Append(m_currentTable.Fields[fieldName].SQLEncodedValue);
                    setComma = true;
                }
            }

            insertSQL.Append(")");

            if (!setComma)
                throw new InvalidOperationException("No field values were specified in insert SQL");

            if (m_whereSQL.Length > 0)
            {
                insertSQL.Append(" ");
                insertSQL.Append(m_whereSQL);
            }

            return insertSQL.ToString();

        }

        /// <summary>
        /// Generate update SQL statement
        /// </summary>
        /// <returns>Update SQL statement to data processing</returns>
        public string UpdateSQL()
        {
            StringBuilder updateSQL = new StringBuilder();
            string fieldName;
            bool setComma = false;

            updateSQL.Append("UPDATE ");
            updateSQL.Append(m_currentTable.SQLEscapedName);
            updateSQL.Append(" SET ");

            foreach (string currentFieldName in m_fieldList)
            {
                fieldName = currentFieldName;

                if (m_includeNulls || !Convert.IsDBNull(m_currentTable.Fields[fieldName].Value))
                {
                    if (setComma)
                        updateSQL.Append(", ");

                    updateSQL.Append(m_currentTable.Fields[fieldName].SQLEscapedName);
                    updateSQL.Append(" = ");
                    updateSQL.Append(m_currentTable.Fields[fieldName].SQLEncodedValue);
                    setComma = true;
                }
            }

            if (!setComma)
                throw new InvalidOperationException("No field values were specified in update SQL");

            if (m_whereSQL.Length > 0)
            {
                updateSQL.Append(" ");
                updateSQL.Append(m_whereSQL);
            }

            return updateSQL.ToString();

        }

        #endregion
    }
}
