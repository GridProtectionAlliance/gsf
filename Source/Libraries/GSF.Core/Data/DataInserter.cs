//******************************************************************************************************
//  DataInserter.cs - Gbtc
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using GSF.IO;
using GSF.Reflection;

namespace GSF.Data
{
    // Note: if you have triggers that insert records into other tables automatically that have defined records to
    // be inserted, this class will check for this occurrence and do SQL updates instead of SQL inserts.  However,
    // just like in the DataUpdater class, you must define the primary key fields in the database or through code
    // to be used for updates for each table in the table collection in order for the updates to occur, hence any
    // tables which have no key fields defined yet appear in the table collection will not be updated...

    /// <summary>
    /// This class defines a common set of functionality that any data operation implementation can use 
    /// </summary>
    public class DataInserter : BulkDataOperationBase
    {
        #region [ Members ]

        //Fields
        private bool m_attemptBulkInsert;
        private bool m_forceBulkInsert;
        private string m_bulkInsertSettings = "FIELDTERMINATOR = '\\t', ROWTERMINATOR = '\\n', CODEPAGE = 'OEM', FIRE_TRIGGERS, KEEPNULLS";
        private Encoding m_bulkInsertEncoding = Encoding.ASCII;

        private string m_bulkInsertFilePath = AssemblyInfo.ExecutingAssembly.Location;
        private string m_delimiterReplacement = " - ";
        private bool m_clearDestinationTables;
        private bool m_attemptTruncateTable;
        private bool m_preserveAutoIncValues;
        private bool m_forceTruncateTable;

        /// <summary>
        /// Table cleared event.
        /// </summary>
        public event EventHandler<EventArgs<string>> TableCleared;
        
        /// <summary>
        /// Bulk-insert executing event.
        /// </summary>
        public event EventHandler<EventArgs<string>> BulkInsertExecuting;
        
        /// <summary>
        /// Bulk-insert completed event.
        /// </summary>
        public event EventHandler<EventArgs<string, int, int>> BulkInsertCompleted;
        
        /// <summary>
        /// Bulk-insert exception event.
        /// </summary>
        public event EventHandler<EventArgs<string, string, Exception>> BulkInsertException;

        /// <summary>
        /// Disposed event.
        /// </summary>
        public event EventHandler Disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataInserter"/>.
        /// </summary>
        public DataInserter()
        {

        }

        /// <summary>
        /// Creates a new <see cref="DataInserter"/>.
        /// </summary>
        public DataInserter(string FromConnectString, string ToConnectString)
            : base(FromConnectString, ToConnectString)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DataInserter"/>.
        /// </summary>
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
        /// Get or set to force use of a BULK INSERT on a destination SQL Server connection regardless of whether or not it looks
        /// like the referential integrity definition supports this.
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
        /// This setting defines the text encoding that will be used when writing a temporary BULK INSERT file that will be needed
        /// if a SQL Server BULK INSERT is performed - make sure the encoding output matches the specified CODEPAGE value in the
        /// BulkInsertSettings property.
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
        /// This setting defines the file path that will be used when writing a temporary BULK INSERT file that will be needed if a
        /// SQL Server BULK INSERT is performed - make sure the destination SQL Server has rights to this path.
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
        /// This specifies the string that will be substituted for the field terminator or row terminator if encountered in a database
        /// value while creating a BULK INSERT file.  The field terminator and row terminator values are defined in the BulkInsertSettings
        /// property specified by the FIELDTERMINATOR and ROWTERMINATOR keywords respectively.
        /// </summary>
        public string DelimiterReplacement
        {
            get
            {
                return m_delimiterReplacement;
            }
            set
            {
                m_delimiterReplacement = value;
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
        /// Set to True to attempt use of a TRUNCATE TABLE on a destination SQL Server connection if ClearDestinationTables is True
        /// and it looks like the referential integrity definition supports this.  Your SQL Server connection will need the rights
        /// to perform this operation.
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
        /// Set to True to force use of a TRUNCATE TABLE on a destination SQL Server connection if ClearDestinationTables is True regardless
        /// of whether or not it looks like the referential integrity definition supports this.  Your SQL Server connection will need the
        /// rights to perform this operation
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
        public bool PreserveAutoIncValues
        {
            get
            {
                return m_preserveAutoIncValues;
            }
            set
            {
                m_preserveAutoIncValues = value;
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
                Disposed(this, EventArgs.Empty); //-V3083
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

            if (m_tableCollection.Count == 0)
                Analyze();

            if (m_tableCollection.Count == 0)
                throw new NullReferenceException("No tables to process even after analyze.");

            // Clear data from destination tables, if requested
            if (m_clearDestinationTables)
            {
                // We do not consider table exclusions when deleting data from destination tables as these may have triggered inserts
                List<Table> allSourceTables = new List<Table>(m_fromSchema.Tables.Cast<Table>());

                // Clear data in a child to parent direction to help avoid potential constraint issues
                allSourceTables.Sort((table1, table2) => table1.Priority > table2.Priority ? 1 : (table1.Priority < table2.Priority ? -1 : 0));

                for (x = allSourceTables.Count - 1; x >= 0; x += -1)
                {
                    // Lookup table name in destination data source
                    tableLookup = m_toSchema.Tables.FindByMapName(((Table)allSourceTables[x]).MapName);

                    if ((object)tableLookup != null)
                    {
                        if (ClearTable(tableLookup) && tableLookup.HasAutoIncField)
                            ResetAutoIncValues(tableLookup);
                    }
                }
            }

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

            // Begin inserting data into destination tables
            for (x = 0; x <= tablesList.Count - 1; x++)
            {
                table = (Table)tablesList[x];

                // Lookup table name in destination data source
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
        /// <param name="table">schema table</param>
        private bool ClearTable(Table table)
        {
            string deleteSql;
            bool useTruncateTable = false;

            if (m_attemptTruncateTable || m_forceTruncateTable)
            {
                // We only attempt a truncate table if the destination data source type is SQL Server
                // and table has no foreign key dependencies (or user forces procedure)
                useTruncateTable = m_forceTruncateTable || (table.Parent.Parent.DataSourceType == DatabaseType.SQLServer && !table.ReferencedByForeignKeys);
            }

            if (useTruncateTable)
                deleteSql = "TRUNCATE TABLE " + table.SQLEscapedName;
            else
                deleteSql = "DELETE FROM " + table.SQLEscapedName;

            try
            {
                table.Connection.ExecuteNonQuery(deleteSql, Timeout);

                if ((object)TableCleared != null)
                    TableCleared(this, new EventArgs<string>(table.Name)); //-V3083

                return true;
            }
            catch (Exception ex)
            {
                if (useTruncateTable)
                {
                    // SQL Server connection may not have rights to use TRUNCATE TABLE, fall back on DELETE FROM
                    m_attemptTruncateTable = false;
                    m_forceTruncateTable = false;
                    return ClearTable(table);
                }

                OnSQLFailure(deleteSql, ex);
            }

            return false;
        }

        private void ResetAutoIncValues(Table table)
        {
            string resetAutoIncValueSQL = "unknown";

            try
            {
                switch (table.Parent.Parent.DataSourceType) //-V3002
                {
                    case DatabaseType.SQLServer:
                        resetAutoIncValueSQL = "DBCC CHECKIDENT('" + table.SQLEscapedName + "', RESEED)";
                        table.Connection.ExecuteNonQuery(resetAutoIncValueSQL, Timeout);
                        break;
                    case DatabaseType.MySQL:
                        resetAutoIncValueSQL = "ALTER TABLE " + table.SQLEscapedName + " AUTO_INCREMENT = 1";
                        table.Connection.ExecuteNonQuery(resetAutoIncValueSQL, Timeout);
                        break;
                    case DatabaseType.SQLite:
                        resetAutoIncValueSQL = "DELETE FROM sqlite_sequence WHERE name = '" + table.Name + "'";
                        table.Connection.ExecuteNonQuery(resetAutoIncValueSQL, Timeout);
                        break;
                    case DatabaseType.PostgreSQL:
                        // The escaping of names here is very deliberate; for certain table names,
                        // it is necessary to escape the table name in the pg_get_serial_sequence() call,
                        // but the call will fail if you attempt to escape the autoIncField name
                        resetAutoIncValueSQL = $"SELECT setval(pg_get_serial_sequence('{table.SQLEscapedName}', '{table.AutoIncField.Name.ToLower()}'), (SELECT MAX({table.AutoIncField.SQLEscapedName}) FROM {table.SQLEscapedName}))";
                        table.Connection.ExecuteNonQuery(resetAutoIncValueSQL, Timeout);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnSQLFailure(resetAutoIncValueSQL, new InvalidOperationException(string.Format("Failed to reset auto-increment seed for table \"{0}\": {1}", table.Name, ex.Message), ex));
            }
        }

        /// <summary>
        /// Execute a command to insert or update data from source to destination table
        /// </summary>
        /// <param name="fromTable">Source table</param>
        /// <param name="toTable">Destination table</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ExecuteInserts(Table fromTable, Table toTable)
        {
            Table sourceTable = (m_useFromSchemaRI ? fromTable : toTable);
            Field autoIncField = null;
            Field lookupField;
            Field commonField;
            bool usingIdentityInsert;

            // Progress process variables
            int progressIndex = 0;
            int progressTotal;

            // Bulk insert variables
            bool useBulkInsert = false;
            string bulkInsertFile = "";
            string fieldTerminator = "";
            string rowTerminator = "";
            FileStream bulkInsertFileStream = null;

            // Create a field list of all of the common fields in both tables
            Fields fieldCollection = new Fields(toTable);

            foreach (Field field in fromTable.Fields)
            {
                // Lookup field name in destination table                
                lookupField = toTable.Fields[field.Name];

                if ((object)lookupField != null)
                {
                    // We currently don't handle binary fields...
                    if (!(field.Type == OleDbType.Binary || field.Type == OleDbType.LongVarBinary || field.Type == OleDbType.VarBinary) && !(lookupField.Type == OleDbType.Binary || lookupField.Type == OleDbType.LongVarBinary || lookupField.Type == OleDbType.VarBinary))
                    {
                        // Copy field information from destination field
                        if (m_useFromSchemaRI)
                        {
                            commonField = new Field(field.Name, field.Type);
                            commonField.AutoIncrement = field.AutoIncrement;
                        }
                        else
                        {
                            commonField = new Field(lookupField.Name, lookupField.Type);
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

            // Setup to track to and from auto-inc values if table has an identity field
            if (sourceTable.HasAutoIncField)
            {
                foreach (Field field in fieldCollection)
                {
                    lookupField = sourceTable.Fields[field.Name];

                    if ((object)lookupField != null)
                    {
                        // We need only track auto inc translations when field is referenced by foreign keys
                        if (lookupField.AutoIncrement && lookupField.ForeignKeys.Count > 0)
                        {
                            // Create a new hash-table to hold auto-inc translations
                            lookupField.AutoIncrementTranslations = new Hashtable();

                            // Create a new auto-inc field to hold source value
                            autoIncField = new Field(field.Name, lookupField.Type);
                            autoIncField.AutoIncrementTranslations = lookupField.AutoIncrementTranslations;
                            break;
                        }
                    }
                }
            }

            // See if this table is a candidate for bulk inserts
            if (m_attemptBulkInsert || m_forceBulkInsert)
                useBulkInsert = SetupBulkInsert(toTable, autoIncField, ref bulkInsertFile, ref fieldTerminator, ref rowTerminator, ref bulkInsertFileStream);

            string selectString = "SELECT " + fieldCollection.GetList(sqlEscapeFunction: m_fromSchema.SQLEscapeName) + " FROM " + fromTable.SQLEscapedName;
            bool skipKeyValuePreservation = false;

            // Handle special case of self-referencing table
            if (sourceTable.IsReferencedBy(sourceTable)) //-V3062
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
                            // If Oracle, force it to sort NULLs at a higher level - note coalesce may fail for non-integer based primary keys for self-referencing tables
                            if (m_fromSchema.DataSourceType == DatabaseType.Oracle || m_fromSchema.DataSourceType == DatabaseType.PostgreSQL)
                                selectString += (index > 0 ? ", " : "") + "COALESCE(" + m_fromSchema.SQLEscapeName(foreignKey.ForeignKey.Name) + ", 0)";
                            else
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

            // We use an optimization available to some databases when we are preserving the original primary key values
            if (!skipKeyValuePreservation && m_preserveAutoIncValues && (object)autoIncField != null)
            {
                switch (m_toSchema.DataSourceType)
                {
                    case DatabaseType.SQLServer:
                        try
                        {
                            toTable.Connection.ExecuteNonQuery("SET IDENTITY_INSERT " + toTable.SQLEscapedName + " ON", Timeout);
                            usingIdentityInsert = true;
                        }
                        catch
                        {
                            // This may fail if connected user doesn't have alter rights to destination connection or has
                            // selected the wrong destination database type, in these cases we just fall back on the
                            // brute force method of auto-inc identity synchronization
                            usingIdentityInsert = false;
                        }
                        break;
                    case DatabaseType.MySQL:
                    case DatabaseType.SQLite:
                    case DatabaseType.PostgreSQL:
                        usingIdentityInsert = true;
                        break;
                    default:
                        usingIdentityInsert = false;
                        break;
                }
            }
            else
            {
                usingIdentityInsert = false;
            }

            string insertSQLStub = "INSERT INTO " + toTable.SQLEscapedName + " (" + fieldCollection.GetList(usingIdentityInsert) + ") VALUES (";
            string updateSQLStub = "UPDATE " + toTable.SQLEscapedName + " SET ";
            string countSQLStub = "SELECT COUNT(*) AS Total FROM " + toTable.SQLEscapedName;

            // Execute source query
            using (IDataReader fromReader = fromTable.Connection.ExecuteReader(selectString, CommandBehavior.SequentialAccess, Timeout))
            {
                // Read source records and write each to destination
                while (fromReader.Read())
                {
                    if (useBulkInsert)
                        WriteBulkInsertRecord(toTable, fieldCollection, sourceTable, fieldTerminator, rowTerminator, bulkInsertFileStream, fromReader);
                    else
                        InsertDestinationRecord(toTable, fieldCollection, insertSQLStub, updateSQLStub, countSQLStub, usingIdentityInsert, sourceTable, autoIncField, skipKeyValuePreservation, fromReader);

                    progressIndex++;
                    m_overallProgress++;

                    OnRowProgress(fromTable.Name, progressIndex, progressTotal);

                    if (progressIndex % RowReportInterval == 0)
                        OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);
                }
            }

            // Turn off identity inserts and reset auto-inc values if needed
            if (usingIdentityInsert)
            {
                if (m_toSchema.DataSourceType == DatabaseType.SQLServer)
                {
                    string setIndentityInsertSQL = "SET IDENTITY_INSERT " + toTable.SQLEscapedName + " OFF";

                    try
                    {
                        // Turn off identity inserts
                        toTable.Connection.ExecuteNonQuery(setIndentityInsertSQL, Timeout);
                    }
                    catch (Exception ex)
                    {
                        OnSQLFailure(setIndentityInsertSQL, new InvalidOperationException(string.Format("Failed to turn off identity inserts on table \"{0}\": {1}", toTable.Name, ex.Message), ex));
                    }
                }

                ResetAutoIncValues(toTable);
            }

            if (useBulkInsert && (object)bulkInsertFileStream != null)
                CompleteBulkInsert(toTable, progressIndex, bulkInsertFile, bulkInsertFileStream);

            OnRowProgress(fromTable.Name, progressTotal, progressTotal);
            OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void InsertDestinationRecord(Table toTable, Fields fieldCollection, string insertSQLStub, string updateSQLStub, string countSQLStub, bool usingIdentityInsert, Table sourceTable, Field autoIncField, bool skipKeyValuePreservation, IDataReader fromReader)
        {
            StringBuilder insertSQL;
            StringBuilder updateSQL;
            StringBuilder countSQL;
            StringBuilder whereSQL;

            Field lookupField;
            string value;
            bool isPrimary;

            bool addedFirstInsertField = false;
            bool addedFirstUpdateField = false;

            // Handle creating SQL for inserts or updates for each row...
            insertSQL = new StringBuilder(insertSQLStub);
            updateSQL = new StringBuilder(updateSQLStub);
            countSQL = new StringBuilder(countSQLStub);
            whereSQL = new StringBuilder();

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

                // If this field is auto-inc we need to track original value
                if (field.AutoIncrement)
                {
                    if ((object)autoIncField != null)
                    {
                        // Even if database supports multiple auto-inc fields, we can only support automatic
                        // ID translation for one because the identity SQL can only return one value...
                        if (string.Compare(field.Name, autoIncField.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // Track original auto-inc value
                            autoIncField.Value = field.Value;
                        }
                    }
                }

                // We don't attempt to insert values into auto-inc fields unless we are using identity inserts
                if (usingIdentityInsert || !field.AutoIncrement)
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
                    if (addedFirstInsertField)
                        insertSQL.Append(", ");
                    else
                        addedFirstInsertField = true;

                    insertSQL.Append(value);

                    if (string.Compare(value, "NULL", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (isPrimary)
                        {
                            if (whereSQL.Length == 0)
                                whereSQL.Append(" WHERE ");
                            else
                                whereSQL.Append(" AND ");

                            whereSQL.Append(field.SQLEscapedName);
                            whereSQL.Append(" = ");
                            whereSQL.Append(value);
                        }
                        else
                        {
                            if (addedFirstUpdateField)
                                updateSQL.Append(", ");
                            else
                                addedFirstUpdateField = true;

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
                try
                {
                    // Insert record into destination table
                    if (addedFirstInsertField || (object)autoIncField != null)
                    {
                        // Added check to preserve ID number for auto-inc fields
                        if (!usingIdentityInsert && !skipKeyValuePreservation && m_preserveAutoIncValues && (object)autoIncField != null)
                        {
                            int toTableRowCount = int.Parse(Common.ToNonNullString(toTable.Connection.ExecuteScalar("SELECT MAX(" + autoIncField.SQLEscapedName + ") FROM " + toTable.SQLEscapedName, Timeout), "0")) + 1;
                            int sourceTablePrimaryFieldValue = int.Parse(Common.ToNonNullString(autoIncField.Value, "0"));
                            int synchronizations = 0;

                            for (int i = toTableRowCount; i < sourceTablePrimaryFieldValue; i++)
                            {
                                // Insert record into destination table up to identity field value
                                toTable.Connection.ExecuteNonQuery(insertSQL.ToString(), Timeout);
                                int currentIdentityValue = int.Parse(Common.ToNonNullString(toTable.Connection.ExecuteScalar(toTable.IdentitySQL, Timeout), "0"));

                                // Delete record which was just inserted
                                toTable.Connection.ExecuteNonQuery("DELETE FROM " + toTable.SQLEscapedName + " WHERE " + autoIncField.SQLEscapedName + " = " + currentIdentityValue, Timeout);

                                // For very long spans of auto-inc identity gaps we at least provide some level of feedback
                                if (synchronizations++ % 50 == 0)
                                    OnTableProgress("Processed " + synchronizations + " auto-increment identity synchronizations...", false, 0, 0);
                            }
                        }

                        // Insert record into destination table
                        if (whereSQL.Length > 0)
                            InsertOrUpdate(toTable, insertSQL, updateSQL, countSQL, whereSQL, addedFirstInsertField, addedFirstUpdateField);
                        else
                            toTable.Connection.ExecuteNonQuery(insertSQL.ToString(), Timeout);
                    }

                    // Save new destination auto-inc value
                    if ((object)autoIncField != null)
                    {
                        try
                        {
                            if (usingIdentityInsert || whereSQL.Length > 0)
                                autoIncField.AutoIncrementTranslations.Add(Convert.ToString(autoIncField.Value), Convert.ToString(autoIncField.Value));
                            else
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
                InsertOrUpdate(toTable, insertSQL, updateSQL, countSQL, whereSQL, addedFirstInsertField, addedFirstUpdateField);
            }
        }

        private void InsertOrUpdate(Table toTable, StringBuilder insertSQL, StringBuilder updateSQL, StringBuilder countSQL, StringBuilder whereSQL, bool addedFirstInsertField, bool addedFirstUpdateField)
        {
            // Add where criteria to SQL count statement
            countSQL.Append(whereSQL);

            // Make sure record doesn't already exist
            try
            {
                // If record already exists due to triggers or other means we must update it instead of inserting it
                if (int.Parse(Common.ToNonNullString(toTable.Connection.ExecuteScalar(countSQL.ToString(), Timeout), "0")) > 0)
                {
                    // Add where criteria to SQL update statement
                    updateSQL.Append(whereSQL);

                    try
                    {
                        // Update record in destination table
                        if (addedFirstUpdateField)
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
                        if (addedFirstInsertField)
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

        private bool SetupBulkInsert(Table toTable, Field autoIncField, ref string bulkInsertFile, ref string fieldTerminator, ref string rowTerminator, ref FileStream bulkInsertFileStream)
        {
            Schema parentSchema = toTable.Parent.Parent;

            // We only attempt a bulk insert if the destination data source type is SQL Server and we are inserting
            // fields into a table that has no auto-inc fields with foreign key dependencies (or user forces procedure)
            bool useBulkInsert = m_forceBulkInsert || (parentSchema.DataSourceType == DatabaseType.SQLServer && ((object)autoIncField == null || m_tableCollection.Count == 1));

            if (useBulkInsert)
            {
                ParseBulkInsertSettings(out fieldTerminator, out rowTerminator);

                if (m_bulkInsertFilePath.Substring(m_bulkInsertFilePath.Length - 1) != "\\")
                    m_bulkInsertFilePath += "\\";

                bulkInsertFile = m_bulkInsertFilePath + (new Guid()) + ".tmp";
                bulkInsertFileStream = File.Create(bulkInsertFile);
            }

            return useBulkInsert;
        }

        private void WriteBulkInsertRecord(Table toTable, Fields fieldCollection, Table sourceTable, string fieldTerminator, string rowTerminator, FileStream bulkInsertFileStream, IDataReader fromReader)
        {
            Field commonField = new Field("Unused", OleDbType.Integer);
            byte[] dataRow;
            StringBuilder bulkInsertRow = new StringBuilder();
            string value;
            bool addedFirstInsertField;

            // Handle creating bulk insert file data for each row...
            addedFirstInsertField = false;

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
                    if ((object)commonField != null)
                    {
                        commonField.Value = "";
                        OnSQLFailure("Failed to get field value for '" + toTable.Name + "." + commonField.Name + "'", ex);
                    }
                    else
                    {
                        OnSQLFailure("Failed to get field value - field unknown.", ex);
                    }
                }

                if ((object)commonField == null)
                    continue;

                // Get translated auto-inc value for field if possible...
                commonField.Value = DereferenceValue(sourceTable, commonField.Name, commonField.Value);

                // Get field value
                value = Convert.ToString(Common.ToNonNullString(commonField.Value)).Trim();

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
                value = value.Replace(fieldTerminator, m_delimiterReplacement);
                value = value.Replace(rowTerminator, m_delimiterReplacement);

                // Construct bulk insert row
                if (addedFirstInsertField)
                    bulkInsertRow.Append(fieldTerminator);
                else
                    addedFirstInsertField = true;

                bulkInsertRow.Append(value);
            }

            bulkInsertRow.Append(rowTerminator);

            // Add new row to temporary bulk insert file
            dataRow = m_bulkInsertEncoding.GetBytes(bulkInsertRow.ToString());
            bulkInsertFileStream.Write(dataRow, 0, dataRow.Length);
        }

        private void CompleteBulkInsert(Table toTable, int progressIndex, string bulkInsertFile, FileStream bulkInsertFileStream)
        {
            string bulkInsertSql = "BULK INSERT " + toTable.SQLEscapedName + " FROM '" + bulkInsertFile + "'" + (m_bulkInsertSettings.Length > 0 ? " WITH (" + m_bulkInsertSettings + ")" : "");

            double startTime = 0;
            double stopTime;

            DateTime todayMidNight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

            // Close bulk insert file stream
            bulkInsertFileStream.Close();

            if ((object)BulkInsertExecuting != null)
                BulkInsertExecuting(this, new EventArgs<string>(toTable.Name)); //-V3083

            try
            {
                // Give system a few seconds to close bulk insert file (might have been real big)
                FilePath.WaitForReadLock(bulkInsertFile, 15);

                TimeSpan difference = DateTime.Now - todayMidNight;
                startTime = difference.TotalSeconds;

                toTable.Connection.ExecuteNonQuery(bulkInsertSql, Timeout);
            }
            catch (Exception ex)
            {
                if ((object)BulkInsertException != null)
                    BulkInsertException(this, new EventArgs<string, string, Exception>(toTable.Name, bulkInsertSql, ex)); //-V3083
            }
            finally
            {
                TimeSpan difference = DateTime.Now - todayMidNight;
                stopTime = difference.TotalSeconds;

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
                BulkInsertCompleted(this, new EventArgs<string, int, int>(toTable.Name, progressIndex, Convert.ToInt32(stopTime - startTime))); //-V3083
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

                    // Traverse path to parent auto-inc field if it exists
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
            string[] keyValue;

            fieldTerminator = "";
            rowTerminator = "";

            foreach (string setting in m_bulkInsertSettings.Split(','))
            {
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

        // Generate un-encoded value for SQL statement
        private string UnEncodeSetting(string setting)
        {
            setting = RemoveQuotes(setting);

            setting = setting.Replace("\\\\", "\\");
            setting = setting.Replace("\\'", "'");
            setting = setting.Replace("\\\"", "\"");
            setting = setting.Replace("\\t", "\t");
            setting = setting.Replace("\\n", "\n");

            return setting;
        }

        // Remove single quotes from SQL statement
        private string RemoveQuotes(string value)
        {
            if (value.Substring(0, 1) == "'")
                value = value.Substring(2);

            if (value.Substring(value.Length - 1) == "'")
                value = value.Substring(0, value.Length - 1);

            return value;
        }

        #endregion
    }
}
