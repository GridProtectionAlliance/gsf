//******************************************************************************************************
//  DataInserter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//       Generated original version of source code from code written in 2003.
//  08/21/2008 - Mihir Brahmbhatt
//       Converted to C# extensions.
//  09/27/2010 - Mihir Brahmbhatt
//       Edited code comments.
//  10/12/2010 - Mihir Brahmbhatt
//       Updated preserve value functionality for auto-inc fields
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using GSF;
using GSF.Data;
using GSF.IO;
using GSF.Reflection;

namespace DataMigrationUtility
{
    // Note: if you have triggers that insert records into other tables automatically that have defined records to
    // be inserted, this class will check for this occurrence and do SQL updates instead of SQL inserts.  However,
    // just like in the DataUpdater class, you must define the primary key fields in the database or through code
    // to be used for updates for each table in the table collection in order for the updates to occur, hence any
    // tables which have no key fields defined yet appear in the table collection will not be updated...

    /// <summary>
    /// This class defines a common set of functionality that any data operation implementation can use 
    /// </summary>
    class DataInserter : BulkDataOperationBase
    {
        #region [ Members ]

        //Fields
        private bool m_attemptBulkInsert;
        private bool m_forceBulkInsert;
        private string m_bulkInsertSettings = "FIELDTERMINATOR = '\\t', ROWTERMINATOR = '\\n', CODEPAGE = 'OEM', FIRE_TRIGGERS, KEEPNULLS";
        private Encoding m_bulkInsertEncoding = Encoding.ASCII;

        private string m_bulkInsertFilePath = AssemblyInfo.ExecutingAssembly.Location;
        private string m_delimeterReplacement = " - ";
        private bool m_clearDestinationTables;
        private bool m_attemptTruncateTable;
        private bool m_preservePrimaryKeyValue;
        private bool m_forceTruncateTable;

        public event EventHandler<EventArgs<string>> TableCleared;
        public event EventHandler<EventArgs<string>> BulkInsertExecuting;
        public event EventHandler<EventArgs<string, int, int>> BulkInsertCompleted;
        public event EventHandler<EventArgs<string, string, Exception>> BulkInsertException;
        public event EventHandler Disposed;

        #endregion

        #region [ Constructors ]

        public DataInserter()
        {

        }

        public DataInserter(string FromConnectString, string ToConnectString)
            : base(FromConnectString, ToConnectString)
        {

        }

        public DataInserter(Schema FromSchema, Schema ToSchema)
            : base(FromSchema, ToSchema)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or set to attempt use of a BULK INSERT on a destination SQL Server connection
        /// </summary>
        public bool AttemptBulkInsert
        {
            get
            {
                return m_attemptBulkInsert;
            }
            set
            {
                m_attemptBulkInsert = value;
            }
        }

        /// <summary>
        /// Get or set to force use of a BULK INSERT on a destination SQL Server connection regardless of whether or not it looks like the referential integrity definition supports this.
        /// </summary>
        public bool ForceBulkInsert
        {
            get
            {
                return m_forceBulkInsert;
            }
            set
            {
                m_forceBulkInsert = value;
            }
        }

        /// <summary>
        /// This setting defines the SQL Server BULK INSERT settings that will be used if a BULK INSERT is performed. 
        /// </summary>
        public string BulkInsertSettings
        {
            get
            {
                return m_bulkInsertSettings;
            }
            set
            {
                m_bulkInsertSettings = value;
            }
        }

        /// <summary>
        /// This setting defines the text encoding that will be used when writing a temporary BULK INSERT file that will be needed if a SQL Server BULK INSERT is performed - make sure the encoding output matches the specified CODEPAGE value in the BulkInsertSettings property.
        /// </summary>
        public Encoding BulkInsertEncoding
        {
            get
            {
                return m_bulkInsertEncoding;
            }
            set
            {
                m_bulkInsertEncoding = value;
            }
        }

        /// <summary>
        /// This setting defines the file path that will be used when writing a temporary BULK INSERT file that will be needed if a SQL Server BULK INSERT is performed - make sure the destination SQL Server has rights to this path.
        /// </summary>
        public string BulkInsertFilePath
        {
            get
            {
                return m_bulkInsertFilePath;
            }
            set
            {
                m_bulkInsertFilePath = value;
            }
        }

        /// <summary>
        /// This specifies the string that will be substituted for the field terminator or row terminator if encountered in a database value while creating a BULK INSERT file.  The field terminator and row terminator values are defined in the BulkInsertSettings property specified by the FIELDTERMINATOR and ROWTERMINATOR keywords repectively.
        /// </summary>
        public string DelimeterReplacement
        {
            get
            {
                return m_delimeterReplacement;
            }
            set
            {
                m_delimeterReplacement = value;
            }
        }


        /// <summary>
        /// Set to True to clear all data from the destination database before processing data inserts.
        /// </summary>
        public bool ClearDestinationTables
        {
            get
            {
                return m_clearDestinationTables;
            }
            set
            {
                m_clearDestinationTables = value;
            }
        }

        /// <summary>
        /// Set to True to attempt use of a TRUNCATE TABLE on a destination SQL Server connection if ClearDestinationTables is True and it looks like the referential integrity definition supports this.  Your SQL Server connection will need the rights to perform this operation.
        /// </summary>
        public bool AttemptTruncateTable
        {
            get
            {
                return m_attemptTruncateTable;
            }
            set
            {
                m_attemptTruncateTable = value;
            }
        }

        /// <summary>
        /// Set to True to force use of a TRUNCATE TABLE on a destination SQL Server connection if ClearDestinationTables is True regardless of whether or not it looks like the referential integrity definition supports this.  Your SQL Server connection will need the rights to perform this operation
        /// </summary>
        public bool ForceTruncateTable
        {
            get
            {
                return m_forceTruncateTable;
            }
            set
            {
                m_forceTruncateTable = value;
            }
        }

        /// <summary>
        /// Set to True to preserve primary key value data to the destination database before processing data inserts.
        /// </summary>
        public bool PreservePrimaryKeyValue
        {
            get
            {
                return m_preservePrimaryKeyValue;
            }
            set
            {
                m_preservePrimaryKeyValue = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Close
        /// </summary>
        public override void Close()
        {
            base.Close();

            if ((object)Disposed != null)
                Disposed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Execute this <see cref="DataInserter"/>
        /// </summary>
        public override void Execute()
        {
            List<Table> tablesList = new List<Table>();
            Table tableLookup;
            Table table;
            int x;

            m_overallProgress = 0;
            m_overallTotal = 0;

            if ((object)m_tableCollection == null)
                Analyze();

            if ((object)m_tableCollection == null)
                throw new NullReferenceException("No table collection found even after analyze.");

            // We copy the tables into an array list so we can sort and process them in priority order
            foreach (Table sourceTable in m_tableCollection)
            {
                if (sourceTable.Process)
                {
                    sourceTable.CalculateRowCount();

                    if (sourceTable.RowCount > 0)
                    {
                        tablesList.Add(sourceTable);
                        m_overallTotal += sourceTable.RowCount;
                    }
                }
            }

            tablesList.Sort((table1, table2) => table1.Priority > table2.Priority ? 1 : (table1.Priority < table2.Priority ? -1 : 0));

            // Clear data from destination tables, if requested - we do this in a child to parent
            // direction to help avoid potential constraint issues
            if (m_clearDestinationTables)
            {
                for (x = tablesList.Count - 1; x >= 0; x += -1)
                {
                    // Lookup table name in destination datasource
                    tableLookup = m_toSchema.Tables.FindByMapName(((Table)tablesList[x]).MapName);

                    if ((object)tableLookup != null)
                        ClearTable(tableLookup);
                }
            }

            // Begin inserting data into destination tables
            for (x = 0; x <= tablesList.Count - 1; x++)
            {
                table = (Table)tablesList[x];

                // Lookup table name in destination datasource
                tableLookup = m_toSchema.Tables.FindByMapName(table.MapName);

                if ((object)tableLookup != null)
                {
                    if (table.RowCount > 0)
                    {
                        // Inform clients of table copy
                        OnTableProgress(table.Name, true, x + 1, tablesList.Count);

                        // Copy source table to destination
                        ExecuteInserts(table, tableLookup);
                    }
                    else
                    {
                        // Inform clients of table skip
                        OnTableProgress(table.Name, false, x + 1, tablesList.Count);
                    }
                }
                else
                {
                    // Inform clients of table skip
                    OnTableProgress(table.Name, false, x + 1, tablesList.Count);
                }
            }

            // Perform final update of progress information
            OnTableProgress("", false, tablesList.Count, tablesList.Count);

        }

        /// <summary>
        /// Clear destination schema table
        /// </summary>
        /// <param name="ToTable">schema table</param>
        private void ClearTable(Table ToTable)
        {
            string deleteSql;
            bool useTruncateTable = false;

            if (m_attemptTruncateTable || m_forceTruncateTable)
            {
                // We only attempt a truncate table if the destination data source type is SQL Server
                // and table has no foreign key dependencies (or user forces procedure)
                useTruncateTable = m_forceTruncateTable || (ToTable.Parent.Parent.DataSourceType == DatabaseType.SQLServer & !ToTable.IsForeignKeyTable);
            }

            if (useTruncateTable)
            {
                deleteSql = "TRUNCATE TABLE " + ToTable.SQLEscapedName;
            }
            else
            {
                deleteSql = "DELETE FROM " + ToTable.SQLEscapedName;
            }

            try
            {
                ToTable.Connection.ExecuteNonQuery(deleteSql, Timeout);

                if ((object)TableCleared != null)
                    TableCleared(this, new EventArgs<string>(ToTable.Name));
            }
            catch (Exception ex)
            {
                OnSQLFailure(deleteSql, ex);
            }
        }

        /// <summary>
        /// Execute a command to insert or update data from source to destination table
        /// </summary>
        /// <param name="fromTable">Source table</param>
        /// <param name="toTable">Destination table</param>
        private void ExecuteInserts(Table fromTable, Table toTable)
        {
            Fields fieldCollection;
            Field lookupField;
            Field commonField = null;
            string insertSQLStub;
            StringBuilder insertSQL;
            string updateSQLStub;
            StringBuilder updateSQL;
            string countSQLStub;
            StringBuilder countSQL;
            StringBuilder whereSQL;
            string value;
            bool addedFirstInsert;
            bool addedFirstUpdate;
            bool isPrimary;

            int progressIndex = 0;
            int progressTotal;
            Table sourceTable = (m_useFromSchemaRI ? fromTable : toTable);
            Field autoIncField = null;

            bool useBulkInsert = false;
            StringBuilder bulkInsertRow;
            string bulkInsertFile = "";
            string fieldTerminator = "";
            string rowTerminator = "";
            FileStream bulkInsertFileStream = null;
            byte[] dataRow;

            // Create a field list of all of the common fields in both tables
            fieldCollection = new Fields(toTable);

            foreach (Field field in fromTable.Fields)
            {
                // Lookup field name in destination table                
                lookupField = toTable.Fields[field.Name];

                if ((object)lookupField != null)
                {
                    // We currently don't handle binary fields...
                    if (!(field.Type == OleDbType.Binary || field.Type == OleDbType.LongVarBinary || field.Type == OleDbType.VarBinary) & !(lookupField.Type == OleDbType.Binary || lookupField.Type == OleDbType.LongVarBinary || lookupField.Type == OleDbType.VarBinary))
                    {
                        // Copy field information from destination field
                        if (m_useFromSchemaRI)
                        {
                            commonField = new Field(fieldCollection, field.Name, field.Type);
                            commonField.AutoIncrement = field.AutoIncrement;
                        }
                        else
                        {
                            commonField = new Field(fieldCollection, lookupField.Name, lookupField.Type);
                            commonField.AutoIncrement = lookupField.AutoIncrement;
                        }

                        fieldCollection.Add(commonField);
                    }
                }
            }

            // Exit if no common field names were found
            if (fieldCollection.Count == 0)
            {
                m_overallProgress += fromTable.RowCount;
                return;
            }

            progressTotal = fromTable.RowCount;
            OnRowProgress(fromTable.Name, 0, progressTotal);
            OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);

            // Setup to track to and from autoinc values if table has an identity field
            if (sourceTable.HasAutoIncField)
            {
                foreach (Field field in fieldCollection)
                {
                    lookupField = sourceTable.Fields[field.Name];

                    if ((object)lookupField != null)
                    {
                        // We need only track auto inc translations when field is referenced by foreign keys
                        if (lookupField.AutoIncrement & lookupField.ForeignKeys.Count > 0)
                        {
                            // Create a new hashtable to hold autoinc translations
                            lookupField.AutoIncrementTranslations = new Hashtable();

                            // Create a new autoinc field to hold source value
                            autoIncField = new Field(toTable.Fields, field.Name, lookupField.Type);
                            autoIncField.AutoIncrementTranslations = lookupField.AutoIncrementTranslations;
                            break;
                        }
                    }
                }
            }

            // See if this table is a candidate for bulk inserts
            if (m_attemptBulkInsert || m_forceBulkInsert)
            {
                Schema parentSchema = toTable.Parent.Parent;

                // We only attempt a bulk insert if the destination data source type is SQL Server and we are inserting
                // fields into a table that has no auto-inc fields with foreign key dependencies (or user forces procedure)
                useBulkInsert = m_forceBulkInsert || (parentSchema.DataSourceType == DatabaseType.SQLServer & ((object)autoIncField == null || m_tableCollection.Count == 1));

                if (useBulkInsert)
                {
                    ParseBulkInsertSettings(out fieldTerminator, out rowTerminator);
                    if (m_bulkInsertFilePath.Substring(m_bulkInsertFilePath.Length - 1) != "\\")
                        m_bulkInsertFilePath += "\\";
                    bulkInsertFile = m_bulkInsertFilePath + (new Guid()) + ".tmp";
                    bulkInsertFileStream = File.Create(bulkInsertFile);
                }
            }

            string selectString = "SELECT " + fieldCollection.GetList(sqlEscapeFunction: m_fromSchema.SQLEscapeName) + " FROM " + fromTable.SQLEscapedName;
            bool skipKeyValuePreservation = false;

            // Handle special case of self-referencing table
            if (sourceTable.IsReferencedBy(sourceTable))
            {
                // We need a special order-by for this scenario to make sure referenced rows are inserted before other rows - this also
                // means no auto-inc preservation is possible on this table
                skipKeyValuePreservation = true;
                selectString += " ORDER BY ";
                int index = 0;

                foreach (Field field in sourceTable.Fields)
                {
                    foreach (ForeignKeyField foreignKey in field.ForeignKeys)
                    {
                        if (string.Compare(sourceTable.Name, foreignKey.ForeignKey.Table.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            selectString += (index > 0 ? ", " : "") + m_fromSchema.SQLEscapeName(foreignKey.ForeignKey.Name);
                            index++;
                        }
                    }
                }

            }
            else
            {
                // Order by auto increment field to help preserve the original value while transferring data to destination table
                if ((object)autoIncField != null)
                    selectString += " ORDER BY " + m_fromSchema.SQLEscapeName(autoIncField.Name);
            }

            // Execute source query
            using (IDataReader fromReader = fromTable.Connection.ExecuteReader(selectString, CommandBehavior.SequentialAccess, Timeout))
            {
                insertSQLStub = "INSERT INTO " + toTable.SQLEscapedName + " (" + fieldCollection.GetList(false) + ") VALUES (";
                updateSQLStub = "UPDATE " + toTable.SQLEscapedName + " SET ";
                countSQLStub = "SELECT COUNT(*) AS Total FROM " + toTable.SQLEscapedName;

                while (fromReader.Read())
                {
                    if (useBulkInsert)
                    {
                        // Handle creating bulk insert file data for each row...
                        bulkInsertRow = new StringBuilder();
                        addedFirstInsert = false;

                        // Get all field data to create row for bulk insert
                        foreach (Field field in toTable.Fields)
                        {
                            try
                            {
                                // Lookup field in common field list
                                commonField = fieldCollection[field.Name];

                                if ((object)commonField != null)
                                {
                                    // Found it, so use it...
                                    commonField.Value = fromReader[field.Name];
                                }
                                else
                                {
                                    // Otherwise just use existing destination field
                                    commonField = field;
                                    commonField.Value = "";
                                }
                            }
                            catch (Exception ex)
                            {
                                commonField.Value = "";
                                OnSQLFailure("Failed to get field value for '" + toTable.Name + "." + commonField.Name + "'", ex);
                            }

                            // Get translated auto-inc value for field if possible...
                            commonField.Value = DereferenceValue(sourceTable, commonField.Name, commonField.Value);

                            // Get field value
                            value = Convert.ToString(Common.NotNull(commonField.Value)).Trim();

                            // We manually parse data type here instead of using SqlEncodedValue because data inserted
                            // into bulk insert file doesn't need SQL encoding...
                            switch (commonField.Type)
                            {
                                case OleDbType.Boolean:
                                    if (value.Length > 0)
                                    {
                                        int tempValue;

                                        if (int.TryParse(value, out tempValue))
                                        {
                                            if (Convert.ToInt32(tempValue) == 0)
                                            {
                                                value = "0";
                                            }
                                            else
                                            {
                                                value = "1";
                                            }
                                        }
                                        else if (Convert.ToBoolean(value))
                                        {
                                            value = "1";
                                        }
                                        else
                                        {
                                            switch (value.Substring(0, 1).ToUpper())
                                            {
                                                case "Y":
                                                case "T":
                                                    value = "1";
                                                    break;
                                                case "N":
                                                case "F":
                                                    value = "0";
                                                    break;
                                                default:
                                                    value = "0";
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                case OleDbType.DBTimeStamp:
                                case OleDbType.DBDate:
                                case OleDbType.Date:
                                    if (value.Length > 0)
                                    {
                                        DateTime tempValue;
                                        if (DateTime.TryParse(value, out tempValue))
                                            value = tempValue.ToString("MM/dd/yyyy HH:mm:ss");
                                    }
                                    break;
                                case OleDbType.DBTime:
                                    if (value.Length > 0)
                                    {
                                        DateTime tempValue;
                                        if (DateTime.TryParse(value, out tempValue))
                                            value = tempValue.ToString("HH:mm:ss");
                                    }
                                    break;
                            }

                            // Make sure field value does not contain field terminator or row terminator
                            value = value.Replace(fieldTerminator, m_delimeterReplacement);
                            value = value.Replace(rowTerminator, m_delimeterReplacement);

                            // Construct bulk insert row
                            if (addedFirstInsert)
                            {
                                bulkInsertRow.Append(fieldTerminator);
                            }
                            else
                            {
                                addedFirstInsert = true;
                            }
                            bulkInsertRow.Append(value);
                        }

                        bulkInsertRow.Append(rowTerminator);

                        // Add new row to temporary bulk insert file
                        dataRow = m_bulkInsertEncoding.GetBytes(bulkInsertRow.ToString());
                        bulkInsertFileStream.Write(dataRow, 0, dataRow.Length);
                    }
                    else
                    {
                        // Handle creating SQL for inserts or updates for each row...
                        insertSQL = new StringBuilder(insertSQLStub);
                        updateSQL = new StringBuilder(updateSQLStub);
                        countSQL = new StringBuilder(countSQLStub);
                        whereSQL = new StringBuilder();
                        addedFirstInsert = false;
                        addedFirstUpdate = false;

                        // Coerce all field data into proper SQL formats
                        foreach (Field field in fieldCollection)
                        {
                            try
                            {
                                field.Value = fromReader[field.Name];
                            }
                            catch (Exception ex)
                            {
                                field.Value = "";
                                OnSQLFailure("Failed to get field value for '" + toTable.Name + "." + field.Name + "'", ex);
                            }

                            // Get translated auto-inc value for field if necessary...
                            field.Value = DereferenceValue(sourceTable, field.Name, field.Value);

                            // We don't attempt to insert values into auto-inc fields
                            if (field.AutoIncrement)
                            {
                                if ((object)autoIncField != null)
                                {
                                    // Even if database supports multiple autoinc fields, we can only support automatic
                                    // ID translation for one because the identity SQL can only return one value...
                                    if (string.Compare(field.Name, autoIncField.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        // Track original autoinc value
                                        autoIncField.Value = field.Value;
                                    }
                                }
                            }
                            else
                            {
                                // Get SQL encoded field value
                                value = field.SQLEncodedValue;

                                // Reference source field to check RI properties
                                lookupField = sourceTable.Fields[field.Name];

                                if ((object)lookupField != null)
                                {
                                    // Check for cases where a NULL value is not allowed
                                    if (!lookupField.AllowsNulls && value.Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
                                        value = lookupField.NonNullNativeValue;

                                    // Check for possible values that should be interpreted as NULL values in nullable foreign key fields
                                    if (lookupField.AllowsNulls && lookupField.IsForeignKey && value.Equals(lookupField.NonNullNativeValue, StringComparison.OrdinalIgnoreCase))
                                        value = "NULL";

                                    // Check to see if this is a key field
                                    isPrimary = lookupField.IsPrimaryKey;
                                }
                                else
                                {
                                    isPrimary = false;
                                }

                                // Construct SQL statements
                                if (addedFirstInsert)
                                    insertSQL.Append(", ");
                                else
                                    addedFirstInsert = true;

                                insertSQL.Append(value);

                                if (string.Compare(value, "NULL", StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    if (isPrimary)
                                    {
                                        if (whereSQL.Length == 0)
                                        {
                                            whereSQL.Append(" WHERE ");
                                        }
                                        else
                                        {
                                            whereSQL.Append(" AND ");
                                        }

                                        whereSQL.Append(field.SQLEscapedName);
                                        whereSQL.Append(" = ");
                                        whereSQL.Append(value);
                                    }
                                    else
                                    {
                                        if (addedFirstUpdate)
                                        {
                                            updateSQL.Append(", ");
                                        }
                                        else
                                        {
                                            addedFirstUpdate = true;
                                        }

                                        updateSQL.Append(field.SQLEscapedName);
                                        updateSQL.Append(" = ");
                                        updateSQL.Append(value);
                                    }
                                }
                            }
                        }

                        insertSQL.Append(")");

                        if ((object)autoIncField != null || whereSQL.Length == 0)
                        {
                            // For tables with autoinc fields that are referenced by foreign keys or
                            // tables that have no primary key fields defined, we can only do inserts...
                            try
                            {
                                // Insert record into destination table
                                if (addedFirstInsert || (object)autoIncField != null)
                                {
                                    // Added check to preserve ID number for auto-inc fields
                                    if (!skipKeyValuePreservation && m_preservePrimaryKeyValue && (object)autoIncField != null)
                                    {
                                        IDbCommand command = toTable.Connection.CreateCommand();

                                        command.CommandText = "SELECT MAX(" + autoIncField.SQLEscapedName + ") FROM " + toTable.SQLEscapedName;
                                        command.CommandTimeout = Timeout;
                                        command.CommandType = CommandType.Text;

                                        int toTableRowCount = int.Parse(Common.NotNull(command.ExecuteScalar(), "0")) + 1;
                                        int sourceTablePrimaryFieldValue = int.Parse(Common.NotNull(autoIncField.Value, "0"));

                                        for (int i = toTableRowCount; i < sourceTablePrimaryFieldValue; i++)
                                        {
                                            // Insert record into destination table upto Identity Field Value
                                            toTable.Connection.ExecuteNonQuery(insertSQL.ToString(), Timeout);
                                            int currentIdentityValue = int.Parse(Common.NotNull(toTable.Connection.ExecuteScalar(toTable.IdentitySQL, Timeout), "0"));

                                            // Delete record which was just inserted
                                            toTable.Connection.ExecuteNonQuery("DELETE FROM " + toTable.SQLEscapedName + " WHERE " + autoIncField.SQLEscapedName + " = " + currentIdentityValue, Timeout);
                                        }
                                    }

                                    // Insert record into destination table
                                    toTable.Connection.ExecuteNonQuery(insertSQL.ToString(), Timeout);
                                }
                                // Save new destination autoinc value
                                if ((object)autoIncField != null)
                                {
                                    try
                                    {
                                        autoIncField.AutoIncrementTranslations.Add(Convert.ToString(autoIncField.Value), toTable.Connection.ExecuteScalar(toTable.IdentitySQL, Timeout));
                                    }
                                    catch (Exception ex)
                                    {
                                        OnSQLFailure(toTable.IdentitySQL, ex);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnSQLFailure(insertSQL.ToString(), ex);
                            }
                        }
                        else
                        {
                            // Add where criteria to SQL count statement
                            countSQL.Append(whereSQL);

                            // Make sure record doesn't already exist
                            try
                            {
                                // If record already exists due to triggers or other means we must update it instead of inserting it
                                IDbCommand command = toTable.Connection.CreateCommand();

                                command.CommandText = countSQL.ToString();
                                command.CommandTimeout = Timeout;
                                command.CommandType = CommandType.Text;

                                if (int.Parse(Common.NotNull(command.ExecuteScalar(), "0")) > 0)
                                {
                                    // Add where criteria to SQL update statement
                                    updateSQL.Append(whereSQL);

                                    try
                                    {
                                        // Update record in destination table
                                        if (addedFirstUpdate)
                                            toTable.Connection.ExecuteNonQuery(updateSQL.ToString(), Timeout);
                                    }
                                    catch (Exception ex)
                                    {
                                        OnSQLFailure(updateSQL.ToString(), ex);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        // Insert record into destination table
                                        if (addedFirstInsert)
                                            toTable.Connection.ExecuteNonQuery(insertSQL.ToString(), Timeout);
                                    }
                                    catch (Exception ex)
                                    {
                                        OnSQLFailure(insertSQL.ToString(), ex);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnSQLFailure(countSQL.ToString(), ex);
                            }
                        }
                    }

                    progressIndex += 1;
                    m_overallProgress += 1;

                    //Raise event to check status of each row 
                    OnRowProgress(fromTable.Name, progressIndex, progressTotal);

                    if (progressIndex % RowReportInterval == 0)
                    {
                        OnRowProgress(fromTable.Name, progressIndex, progressTotal);
                        OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);
                    }
                }

                fromReader.Close();
            }

            if (useBulkInsert & (object)bulkInsertFileStream != null)
            {
                string bulkInsertSql = "BULK INSERT " + toTable.SQLEscapedName + " FROM '" + bulkInsertFile + "'" + (m_bulkInsertSettings.Length > 0 ? " WITH (" + m_bulkInsertSettings + ")" : "");

                double startTime = 0;
                double stopTime;

                DateTime todayMidNight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

                // Close bulk insert file stream
                bulkInsertFileStream.Close();

                if ((object)BulkInsertExecuting != null)
                    BulkInsertExecuting(this, new EventArgs<string>(toTable.Name));

                try
                {
                    // Give system a few seconds to close bulk insert file (might have been real big)
                    FilePath.WaitForReadLock(bulkInsertFile, 15);

                    TimeSpan difference = DateTime.Now - todayMidNight;
                    startTime = difference.TotalSeconds; //VB.DateAndTime.Timer;

                    toTable.Connection.ExecuteNonQuery(bulkInsertSql, Timeout);
                }
                catch (Exception ex)
                {
                    if ((object)BulkInsertException != null)
                        BulkInsertException(this, new EventArgs<string, string, Exception>(toTable.Name, bulkInsertSql, ex));
                }
                finally
                {
                    TimeSpan difference = DateTime.Now - todayMidNight;

                    stopTime = difference.TotalSeconds; // VB.DateAndTime.Timer;

                    if (Convert.ToInt32(startTime) == 0)
                        startTime = stopTime;
                }

                try
                {
                    FilePath.WaitForWriteLock(bulkInsertFile, 15);
                    File.Delete(bulkInsertFile);
                }
                catch (Exception ex)
                {
                    if ((object)BulkInsertException != null)
                        BulkInsertException(this, new EventArgs<string, string, Exception>(toTable.Name, bulkInsertSql, new InvalidOperationException("Failed to delete temporary bulk insert file \"" + bulkInsertFile + "\" due to exception [" + ex.Message + "], file will need to be manually deleted.", ex)));
                }

                if ((object)BulkInsertCompleted != null)
                    BulkInsertCompleted(this, new EventArgs<string, int, int>(toTable.Name, progressIndex, Convert.ToInt32(stopTime - startTime)));
            }

            OnRowProgress(fromTable.Name, progressTotal, progressTotal);
            OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);

        }

        /// <summary>
        /// Lookup referential value for source table and update their information
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="fieldStack"></param>
        /// <returns></returns>
        internal object DereferenceValue(Table sourceTable, string fieldName, object value, ArrayList fieldStack = null)
        {
            Field lookupField;
            object tempValue;

            // No need to attempt to deference null value
            if (Convert.IsDBNull(value) || (object)value == null)
                return value;

            // If this field is referenced as a foreign key field by a primary key field that is auto-incremented, we
            // translate the auto-inc value if possible
            lookupField = sourceTable.Fields[fieldName];

            if ((object)lookupField != null)
            {
                if (lookupField.IsForeignKey)
                {
                    Field referenceByField = lookupField.ReferencedBy;

                    if (referenceByField.AutoIncrement)
                    {
                        // Return new auto-inc value
                        if ((object)referenceByField.AutoIncrementTranslations == null)
                            return value;

                        tempValue = referenceByField.AutoIncrementTranslations[Convert.ToString(value)];
                        return tempValue ?? value;
                    }

                    bool inStack = false;
                    int x;

                    if ((object)fieldStack == null)
                        fieldStack = new ArrayList();

                    // We don't want to circle back on ourselves
                    for (x = 0; x <= fieldStack.Count - 1; x++)
                    {
                        if (ReferenceEquals(lookupField.ReferencedBy, fieldStack[x]))
                        {
                            inStack = true;
                            break;
                        }
                    }

                    // Traverse path to parent autoinc field if it exists
                    if (!inStack)
                    {
                        fieldStack.Add(lookupField.ReferencedBy);
                        return DereferenceValue(referenceByField.Table, referenceByField.Name, value, fieldStack);
                    }
                }
            }

            return value;

        }

        private void ParseBulkInsertSettings(out string fieldTerminator, out string rowTerminator)
        {
            string setting;
            string[] keyValue;

            fieldTerminator = "";
            rowTerminator = "";

            foreach (string strSetting_loopVariable in m_bulkInsertSettings.Split(','))
            {
                setting = strSetting_loopVariable;
                keyValue = setting.Split('=');

                if (keyValue.Length == 2)
                {
                    if (string.Compare(keyValue[0].Trim(), "FIELDTERMINATOR", StringComparison.OrdinalIgnoreCase) == 0)
                        fieldTerminator = keyValue[1].Trim();
                    else if (string.Compare(keyValue[0].Trim(), "ROWTERMINATOR", StringComparison.OrdinalIgnoreCase) == 0)
                        rowTerminator = keyValue[1].Trim();
                }
            }

            if (fieldTerminator.Length == 0)
                fieldTerminator = "\\t";

            if (rowTerminator.Length == 0)
                rowTerminator = "\\n";

            fieldTerminator = UnEncodeSetting(fieldTerminator);
            rowTerminator = UnEncodeSetting(rowTerminator);
        }

        /// <summary>
        /// Generate unencoded value for SQL statement. <seealso cref="RemoveQuotes"/>
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private string UnEncodeSetting(string setting)
        {

            setting = RemoveQuotes(setting);
            setting = setting.Replace("\\\\", "\\");
            setting = setting.Replace("\\'", "'");
            setting = setting.Replace("\\\"", "\"");
            setting = setting.Replace("\\t", "\t"); // VB.Constants.vbTab);
            setting = setting.Replace("\\n", "\n"); //VB.Constants.vbCrLf);
            return setting;
        }

        /// <summary>
        /// Remove single quotes from SQL statement
        /// </summary>
        /// <param name="value">string to be checked</param>
        /// <returns></returns>
        private string RemoveQuotes(string value)
        {
            if (value.Substring(0, 1) == "'")
                value = value.Substring(2);
            if (value.Substring(value.Length - 1) == "'")
                value = value.Substring(0, value.Length - 1);

            //if (Strings.Left(Str, 1) == "'")
            //    Str = Strings.Mid(Str, 2);
            //if (Strings.Right(Str, 1) == "'")
            //    Str = Strings.Mid(Str, 1, Strings.Len(Str) - 1);

            return value;

        }

        #endregion
    }
}
