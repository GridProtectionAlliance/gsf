//******************************************************************************************************
//  sqlGenerator.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
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
//
//******************************************************************************************************

using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Data.OleDb;

namespace Database
{
    /// <summary>
    /// Common database functions
    /// </summary>
    public static class Common
    {
        /// <summary>
        ///  Converts value to string, null objects (or DBNull objects) will return an empty string (""). 
        /// </summary>
        /// <param name="value">Value of <see cref="Object"/> to convert to string.</param>
        /// <param name="nonNullValue"><see cref="String"/> to return if <paramref name="value"/> is null.</param>
        /// <returns><paramref name="value"/> as a string; if <paramref name="value"/> is null, <paramref name="nonNullValue"/> will be returned.</returns>
        public static string NotNull(object value, string nonNullValue = null)
        {
            if (nonNullValue == null)
            {
                return TVA.Common.ToNonNullString<object>(value);
            }
            else
            {
                return TVA.Common.ToNonNullString<object>(value, nonNullValue);
            }
        }

    }

    /// <summary>
    /// Generates a sql statement for data processing.
    /// </summary>
    [ToolboxBitmap(typeof(sqlGenerator), "Database.SqlGenerator.bmp"), DefaultProperty("SourceSchema")]
    public partial class sqlGenerator : Component
    {
        #region [ Members ]

        private Schema m_sourceSchema;
        private Table m_currentTable;
        private ArrayList m_fieldList = new ArrayList();
        private string m_whereSql;
        private bool m_includeNulls;

        #endregion

        #region [ Constructors ]

        public sqlGenerator()
        {
            InitializeComponent();
            m_whereSql = "";
            m_includeNulls = false;
        }

        public sqlGenerator(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        public sqlGenerator(Schema SourceSchema)
        {
            InitializeComponent();
            m_sourceSchema = SourceSchema;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or Set Source <see cref="Schema"/> object to use for Sql generation.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Source Schema object to use for Sql generation.")]
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
        /// Get or Set - WHERE clause Sql to append to generated Sql statements.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("WHERE clause Sql to append to generated Sql statements."), DefaultValue("")]
        public string WhereSql
        {
            get
            {
                return m_whereSql;
            }
            set
            {
                m_whereSql = value;
            }
        }

        /// <summary>
        /// Get or Set - to include NULL values in generated Sql statements.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Set to True to include NULL values in generated Sql statements."), DefaultValue(false)]
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
        /// Get <see cref="OleDbConnection"/> setting for source <see cref="Schema"/>
        /// </summary>
        [Browsable(false)]
        public OleDbConnection Connection
        {
            get
            {
                return m_sourceSchema.Connection;
            }
        }

        /// <summary>
        /// Get or Set source <see cref="Schema"/> table name.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

                if (m_currentTable != null)
                {
                    // Clear any existing field values
                    // Field fld = default(Field);
                    foreach (Field fld in m_currentTable.Fields)
                    {
                        fld.Value = null;
                    }

                    m_fieldList.Clear();
                }
                else
                {
                    throw new InvalidOperationException("Table [" + value + "] not found in schema");
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// set value to current table column value
        /// </summary>
        /// <param name="FieldName">Set <paramref name="FieldValue"/> to <see cref="SourceSchema"/> table's field value</param>
        /// <param name="FieldValue">Set <see cref="Object"/> value to <paramref name="FieldName"/> of current table</param>
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
        /// Generate insert sql statement
        /// </summary>
        /// <returns>insert Sql statment for current table to process</returns>
        public string InsertSql()
        {
            StringBuilder insertSql = new StringBuilder();
            string fieldName = null;
            bool setComma = false;

            insertSql.Append("INSERT INTO ");
            insertSql.Append(m_currentTable.FullName);
            insertSql.Append(" (");

            foreach (string currentFieldName in m_fieldList)
            {
                fieldName = currentFieldName;
                if (m_includeNulls || (Common.NotNull(m_currentTable.Fields[fieldName].Value) != null))
                {
                    if (setComma)
                        insertSql.Append(", [");
                    else
                        insertSql.Append("[");

                    insertSql.Append(m_currentTable.Fields[fieldName].Name);
                    insertSql.Append("]");
                    setComma = true;
                }
            }

            insertSql.Append(") VALUES (");
            setComma = false;

            foreach (string currentFieldName in m_fieldList)
            {
                fieldName = currentFieldName;

                if (m_includeNulls | (Common.NotNull(m_currentTable.Fields[fieldName].Value) != null))
                {
                    if (setComma)
                        insertSql.Append(", ");

                    insertSql.Append(m_currentTable.Fields[fieldName].SqlEncodedValue);
                    setComma = true;
                }
            }

            insertSql.Append(")");

            if (!setComma)
                throw new InvalidOperationException("No field values were specified in insert Sql");

            if (m_whereSql.Length > 0)
            {
                insertSql.Append(" ");
                insertSql.Append(m_whereSql);
            }

            return insertSql.ToString();

        }

        /// <summary>
        /// Generate update sql statement
        /// </summary>
        /// <returns>Update sql statement to data processing</returns>
        public string UpdateSql()
        {
            StringBuilder updateSql = new StringBuilder();
            string fieldName = null;
            bool setComma = false;

            updateSql.Append("UPDATE ");
            updateSql.Append(m_currentTable.FullName);
            updateSql.Append(" SET ");

            foreach (string currentFieldName in m_fieldList)
            {
                fieldName = currentFieldName;
                if (m_includeNulls | (Common.NotNull(m_currentTable.Fields[fieldName].Value) != null))
                {
                    if (setComma)
                        updateSql.Append(", [");
                    else
                        updateSql.Append("[");

                    updateSql.Append(m_currentTable.Fields[fieldName].Name);
                    updateSql.Append("] = ");
                    updateSql.Append(m_currentTable.Fields[fieldName].SqlEncodedValue);
                    setComma = true;
                }
            }

            if (!setComma)
                throw new InvalidOperationException("No field values were specified in update Sql");

            if (m_whereSql.Length > 0)
            {
                updateSql.Append(" ");
                updateSql.Append(m_whereSql);
            }

            return updateSql.ToString();

        }

        #endregion
    }
}
