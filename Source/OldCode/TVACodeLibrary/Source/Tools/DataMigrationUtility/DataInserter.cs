//******************************************************************************************************
//  DataInserter.cs - Gbtc
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
//  -----------------------------------------------------------------------------------------------------
//  06/28/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/21/2008 - Mihir Brahmbhatt
//       Converted to C# extensions.
//  09/27/2010 - Mihir Brahmbhatt
//       Edited code comments.
//  10/12/2010 - Mihir Brahmbhatt
//       Updated preserve value functionality for auto-inc fields
//
//******************************************************************************************************

// James Ritchie Carroll - 2003
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Text;
using TVA.Data;
using TVA.Reflection;

namespace Database
{
    // Note: if you have triggers that insert records into other tables automatically that have defined records to
    // be inserted, this class will check for this occurance and do Sql updates instead of Sql inserts.  However,
    // just like in the DataUpdater class, you must define the primary key fields in the database or through code
    // to be used for updates for each table in the table collection in order for the updates to occur, hence any
    // tables which have no key fields defined yet appear in the table collection will not be updated...

    /// <summary>
    /// This class defines a common set of functionality that any data operation implementation can use 
    /// </summary>
    [ToolboxBitmap(typeof(DataInserter), "Database.DataInserter.bmp"), DefaultProperty("FromConnectString"), DefaultEvent("OverallProgress")]
    class DataInserter : BulkDataOperationBase, IComponent
    {
        #region [ Members ]

        //Fields
        private bool m_attemptBulkInsert = false;
        private bool m_forceBulkInsert = false;
        private string m_bulkInsertSettings = "FIELDTERMINATOR = '\\t', ROWTERMINATOR = '\\n', CODEPAGE = 'OEM', FIRE_TRIGGERS, KEEPNULLS";
        private Encoding m_bulkInsertEncoding = Encoding.ASCII;
        // Mehul GetApplicationPath()
        private string m_bulkInsertFilePath = AssemblyInfo.ExecutingAssembly.Location;
        private string m_delimeterReplacement = " - ";
        private bool m_clearDestinationTables = false;
        private bool m_attemptTruncateTable = false;
        private bool m_preservePrimaryKeyValue = false;
        private bool m_forceTruncateTable = false;

        //Delegates
        public event TableClearedEventHandler TableCleared;
        public delegate void TableClearedEventHandler(string TableName);
        public event BulkInsertExecutingEventHandler BulkInsertExecuting;
        public delegate void BulkInsertExecutingEventHandler(string TableName);
        public event BulkInsertCompletedEventHandler BulkInsertCompleted;
        public delegate void BulkInsertCompletedEventHandler(string TableName, int TotalRows, int TotalSeconds);
        public event BulkInsertExceptionEventHandler BulkInsertException;
        public delegate void BulkInsertExceptionEventHandler(string TableName, string Sql, Exception ex);
        public event EventHandler Disposed;

        #endregion

        #region [ Constructors ]

        public DataInserter()
            : base()
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
        /// Get or Set to attempt use of a BULK INSERT on a destinaction Sql Server connection
        /// </summary>
        [Browsable(true), Category("Sql Server Specific"), Description("Set to True to attempt use of a BULK INSERT on a destination Sql Server connection if it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(false)]
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
        /// Get or Set to force use of a BULK INSERT on a destination Sql Server connection regardless of whether or not it looks like the referential integrity definition supports this.
        /// </summary>
        [Browsable(true), Category("Sql Server Specific"), Description("Set to True to force use of a BULK INSERT on a destination Sql Server connection regardless of whether or not it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(false)]
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
        /// This setting defines the Sql Server BULK INSERT settings that will be used if a BULK INSERT is performed. 
        /// </summary>
        [Browsable(true), Category("Sql Server Specific"), Description("This setting defines the Sql Server BULK INSERT settings that will be used if a BULK INSERT is performed.")]
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
        /// This setting defines the text encoding that will be used when writing a temporary BULK INSERT file that will be needed if a Sql Server BULK INSERT is performed - make sure the encoding output matches the specified CODEPAGE value in the BulkInsertSettings property.
        /// </summary>
        [Browsable(false), Category("Sql Server Specific"), Description("This setting defines the text encoding that will be used when writing a temporary BULK INSERT file that will be needed if a Sql Server BULK INSERT is performed - make sure the encoding output matches the specified CODEPAGE value in the BulkInsertSettings property."), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        /// This setting defines the file path that will be used when writing a temporary BULK INSERT file that will be needed if a Sql Server BULK INSERT is performed - make sure the destination Sql Server has rights to this path.
        /// </summary>
        [Browsable(true), Category("Sql Server Specific"), Description("This setting defines the file path that will be used when writing a temporary BULK INSERT file that will be needed if a Sql Server BULK INSERT is performed - make sure the destination Sql Server has rights to this path.")]
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
        [Browsable(true), Category("Sql Server Specific"), Description("This specifies the string that will be substituted for the field terminator or row terminator if encountered in a database value while creating a BULK INSERT file.  The field terminator and row terminator values are defined in the BulkInsertSettings property specified by the FIELDTERMINATOR and ROWTERMINATOR keywords repectively.")]
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
        [Browsable(true), Category("Configuration"), Description("Set to True to clear all data from the destination database before processing data inserts."), DefaultValue(false)]
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
        /// Set to True to attempt use of a TRUNCATE TABLE on a destination Sql Server connection if ClearDestinationTables is True and it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation.
        /// </summary>
        [Browsable(true), Category("Sql Server Specific"), Description("Set to True to attempt use of a TRUNCATE TABLE on a destination Sql Server connection if ClearDestinationTables is True and it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(false)]
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
        /// Set to True to force use of a TRUNCATE TABLE on a destination Sql Server connection if ClearDestinationTables is True regardless of whether or not it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation
        /// </summary>
        [Browsable(true), Category("Sql Server Specific"), Description("Set to True to force use of a TRUNCATE TABLE on a destination Sql Server connection if ClearDestinationTables is True regardless of whether or not it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(false)]
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
        [Browsable(true), Category("Configuration"), Description("Set to True to preserve primary key value data to the destination database before processing data inserts."), DefaultValue(false)]
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

        /// <summary>
        /// Get or Set Site
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ISite Site
        {
            get
            {
                return ComponentSite;
            }

            set
            {
                ComponentSite = value;
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
            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Execute this <see cref="DataInserter"/>
        /// </summary>
        public override void Execute()
        {
            List<Table> tablesList = new List<Table>();
            Table tableLookup = null;
            Table table = null;
            int x = 0;

            intOverallProgress = 0;
            intOverallTotal = 0;

            if (colTables == null)
                Analyze();

            // We copy the tables into an array list so we can sort and process them in priority order
            foreach (Table sourceTable in colTables)
            {
                if (sourceTable.Process)
                    tablesList.Add(sourceTable);
            }

            tablesList.Sort((table1, table2) => table1.Priority > table2.Priority ? 1 : (table1.Priority < table2.Priority ? -1 : 0));

            // Clear data from destination tables, if requested - we do this in a child to parent
            // direction to help avoid potential constraint issues
            if (m_clearDestinationTables)
            {
                for (x = tablesList.Count - 1; x >= 0; x += -1)
                {
                    // Lookup table name in destination datasource
                    tableLookup = schTo.Tables.FindByMapName(((Table)tablesList[x]).MapName);
                    if ((tableLookup != null))
                        ClearTable(tableLookup);
                }
            }

            // Begin inserting data into destination tables
            for (x = 0; x <= tablesList.Count - 1; x++)
            {
                table = (Table)tablesList[x];

                // Lookup table name in destination datasource
                tableLookup = schTo.Tables.FindByMapName(table.MapName);

                if ((tableLookup != null))
                {
                    if (table.RowCount > 0)
                    {
                        // Inform clients of table copy
                        RaiseEvent_TableProgress(table.Name, true, (x + 1), tablesList.Count);

                        // Copy source table to destination
                        ExecuteInserts(table, tableLookup);
                    }
                    else
                    {
                        // Inform clients of table skip
                        RaiseEvent_TableProgress(table.Name, false, (x + 1), tablesList.Count);
                    }
                }
                else
                {
                    // Inform clients of table skip
                    RaiseEvent_TableProgress(table.Name, false, (x + 1), tablesList.Count);
                }
            }

            // Perform final update of progress information
            RaiseEvent_TableProgress("", false, tablesList.Count, tablesList.Count);

        }

        /// <summary>
        /// Clear destination schema table
        /// </summary>
        /// <param name="ToTable">schema table</param>
        private void ClearTable(Table ToTable)
        {
            string deleteSql = null;
            bool useTruncateTable = false;

            if (m_attemptTruncateTable | m_forceTruncateTable)
            {
                // We only attempt a truncate table if the destination data source type is Sql Server
                // and table has no foreign key dependencies (or user forces procedure)
                useTruncateTable = m_forceTruncateTable | (ToTable.Parent.Parent.DataSourceType == DatabaseType.SqlServer & !ToTable.IsForeignKeyTable);
            }

            if (useTruncateTable)
            {
                deleteSql = "TRUNCATE TABLE " + ToTable.FullName;
            }
            else
            {
                deleteSql = "DELETE FROM " + ToTable.FullName;
            }

            try
            {
                ToTable.Connection.ExecuteNonQuery(deleteSql, Timeout);
                if (TableCleared != null)
                {
                    TableCleared(ToTable.Name);
                }
            }
            catch (Exception ex)
            {
                RaiseEvent_SqlFailure(deleteSql, ex);
            }

        }

        /// <summary>
        /// Execute a command to insert or update data from source to destination table
        /// </summary>
        /// <param name="FromTable">Source table</param>
        /// <param name="ToTable">Destination table</param>
        private void ExecuteInserts(Table FromTable, Table ToTable)
        {
            Fields colFields = default(Fields);
            //Field fld = default(Field);
            Field fldLookup = null;
            Field fldCommon = null;
            string InsertSqlStub = null;
            StringBuilder InsertSql = null;
            string UpdateSqlStub = null;
            StringBuilder UpdateSql = null;
            string CountSqlStub = null;
            StringBuilder CountSql = null;
            StringBuilder WhereSql = null;
            string strValue = null;
            bool AddedFirstInsert = false;
            bool AddedFirstUpdate = false;
            bool IsPrimary = false;
            //Dim flgRecordExists As Boolean
            int ProgressIndex = 0;
            int Total = 0;
            Table tblSource = (flgUseFromSchemaRI ? FromTable : ToTable);
            Field fldAutoInc = null;
            bool UseBulkInsert = false;
            StringBuilder BulkInsertRow = null;
            string BulkInsertFile = "";
            string FieldTerminator = "";
            string RowTerminator = "";
            FileStream fsBulkInsert = null;
            byte[] bytDataRow = null;

            // Create a field list of all of the common fields in both tables
            colFields = new Fields(ToTable);

            foreach (Field fld in FromTable.Fields)
            {
                // Lookup field name in destination table                
                fldLookup = ToTable.Fields[fld.Name];

                if (fldLookup != null)
                {
                    //var _with1 = fldLookup;
                    // We currently don't handle binary fields...
                    if (!(fld.Type == OleDbType.Binary | fld.Type == OleDbType.LongVarBinary | fld.Type == OleDbType.VarBinary) & !(fldLookup.Type == OleDbType.Binary | fldLookup.Type == OleDbType.LongVarBinary | fldLookup.Type == OleDbType.VarBinary))
                    {
                        // Copy field information from destination field
                        if (flgUseFromSchemaRI)
                        {
                            fldCommon = new Field(colFields, fld.Name, fld.Type);
                            fldCommon.AutoIncrement = fld.AutoIncrement;
                        }
                        else
                        {
                            fldCommon = new Field(colFields, fldLookup.Name, fldLookup.Type);
                            fldCommon.AutoIncrement = fldLookup.AutoIncrement;
                        }

                        colFields.Add(fldCommon);
                    }
                }
            }

            // Exit if no common field names were found
            if (colFields.Count == 0)
            {
                intOverallProgress += FromTable.RowCount;
                return;
            }

            Total = FromTable.RowCount;
            RaiseEvent_RowProgress(FromTable.Name, 0, Total);
            RaiseEvent_OverallProgress((int)intOverallProgress, (int)intOverallTotal);

            // Setup to track to and from autoinc values if table has an identity field
            if (tblSource.HasAutoIncField)
            {
                foreach (Field fld in colFields)
                {
                    fldLookup = tblSource.Fields[fld.Name];
                    if ((fldLookup != null))
                    {
                        //var _with2 = fldLookup;
                        // We need only track autoinc translations when field is referenced by foreign keys
                        if (fldLookup.AutoIncrement & fldLookup.ForeignKeys.Count > 0)
                        {
                            // Create a new hashtable to hold autoinc translations
                            fldLookup.AutoIncrementTranslations = new Hashtable();

                            // Create a new autoinc field to hold source value
                            fldAutoInc = new Field(tblSource.Fields, fld.Name, fldLookup.Type);
                            fldAutoInc.AutoIncrementTranslations = fldLookup.AutoIncrementTranslations;
                            break;
                        }
                    }
                }
            }

            // See if this table is a candidate for bulk inserts
            if (m_attemptBulkInsert | m_forceBulkInsert)
            {
                //var _with3 = ToTable.Parent.Parent;
                Schema parentSchema = ToTable.Parent.Parent;
                // We only attempt a bulk insert if the destination data source type is Sql Server and we are inserting
                // fields into a table that has no auto-incs with foreign key dependencies (or user forces procedure)
                UseBulkInsert = m_forceBulkInsert | (parentSchema.DataSourceType == DatabaseType.SqlServer & (fldAutoInc == null | colTables.Count == 1));
                if (UseBulkInsert)
                {
                    ParseBulkInsertSettings(ref  FieldTerminator, ref  RowTerminator);
                    if (m_bulkInsertFilePath.Substring(m_bulkInsertFilePath.Length - 1) != "\\")
                        m_bulkInsertFilePath += "\\";
                    BulkInsertFile = m_bulkInsertFilePath + (new Guid()).ToString() + ".tmp";
                    fsBulkInsert = File.Create(BulkInsertFile);
                }
            }

            string selectString = "SELECT " + colFields.GetList() + " FROM " + FromTable.FullName;
            bool skipKeyValuePreservation = false;

            // Handle special case of self-referencing table
            if (tblSource.IsReferencedBy(tblSource))
            {
                // We need a special order-by for this scenario to make sure referenced rows are inserted before other rows - this also
                // means no auto-inc preservation is possible on this table
                skipKeyValuePreservation = true;
                selectString += " ORDER BY ";
                int index = 0;

                foreach (Field field in tblSource.Fields)
                {
                    foreach (ForeignKeyField foreignKey in field.ForeignKeys)
                    {
                        if (string.Compare(tblSource.Name, foreignKey.ForeignKey.Table.Name, true) == 0)
                        {
                            selectString += (index > 0 ? ", " : "") + foreignKey.ForeignKey.Name;
                            index++;
                        }
                    }
                }

            }
            else
            {
                // Order by auto increment field to help preserve the original value while transfering data to destination table
                if (fldAutoInc != null)
                    selectString += " ORDER BY " + fldAutoInc.Name;
            }

            // Execute source query
            OleDbDataReader fromReader = FromTable.Connection.ExecuteReader(selectString, CommandBehavior.SequentialAccess, Timeout);

            // Create Sql stubs
            InsertSqlStub = "INSERT INTO " + ToTable.FullName + " (" + colFields.GetList(false) + ") VALUES (";
            UpdateSqlStub = "UPDATE " + ToTable.FullName + " SET ";
            CountSqlStub = "SELECT COUNT(*) AS Total FROM " + ToTable.FullName;

            while (fromReader.Read())
            {
                if (UseBulkInsert)
                {
                    // Handle creating bulk insert file data for each row...
                    BulkInsertRow = new StringBuilder();
                    AddedFirstInsert = false;

                    // Get all field data to create row for bulk insert
                    foreach (Field fld in ToTable.Fields)
                    {
                        try
                        {
                            // Lookup field in common field list
                            fldCommon = colFields[fld.Name];
                            if ((fldCommon != null))
                            {
                                // Found it, so use it...
                                fldCommon.Value = fromReader[fld.Name];
                            }
                            else
                            {
                                // Otherwise just use existing destination field
                                fldCommon = fld;
                                fldCommon.Value = "";
                            }
                        }
                        catch (Exception ex)
                        {
                            fldCommon.Value = "";
                            RaiseEvent_SqlFailure("Failed to get field value for [" + tblSource.Name + "." + fldCommon.Name + "]", ex);
                        }

                        // Get translated auto-inc value for field if possible...
                        fldCommon.Value = DereferenceValue(tblSource, fldCommon.Name, fldCommon.Value);

                        // Get field value
                        strValue = Convert.ToString(Common.NotNull(fldCommon.Value)).Trim();

                        // We manually parse data type here instead of using SqlEncodedValue because data inserted
                        // into bulk insert file doesn't need Sql encoding...
                        switch (fldCommon.Type)
                        {
                            case OleDbType.Boolean:
                                if (strValue.Length > 0)
                                {
                                    int tempValue;
                                    if (int.TryParse(strValue, out tempValue)) //if (Information.IsNumeric(strValue))
                                    {
                                        if (Convert.ToInt32(tempValue) == 0)
                                        {
                                            strValue = "0";
                                        }
                                        else
                                        {
                                            strValue = "1";
                                        }
                                    }
                                    else if (Convert.ToBoolean(strValue))
                                    {
                                        strValue = "1";
                                    }
                                    else
                                    {
                                        switch (strValue.Substring(0, 1).ToUpper())
                                        {
                                            case "Y":
                                            case "T":
                                                strValue = "1";
                                                break;
                                            case "N":
                                            case "F":
                                                strValue = "0";
                                                break;
                                            default:
                                                strValue = "0";
                                                break;
                                        }
                                    }
                                }
                                break;
                            case OleDbType.DBTimeStamp:
                            case OleDbType.DBDate:
                            case OleDbType.Date:
                                if (strValue.Length > 0)
                                {
                                    DateTime tempValue;
                                    if (DateTime.TryParse(strValue, out tempValue)) //if (Information.IsDate(strValue))
                                    {
                                        strValue = tempValue.ToString("MM/dd/yyyy HH:mm:ss");
                                        //strValue = Strings.Format((DateTime)strValue, "MM/dd/yyyy HH:mm:ss");
                                    }
                                }
                                break;
                            case OleDbType.DBTime:
                                if (strValue.Length > 0)
                                {
                                    DateTime tempValue;
                                    if (DateTime.TryParse(strValue, out tempValue)) //if (Information.IsDate(strValue))
                                    {
                                        strValue = tempValue.ToString("HH:mm:ss");
                                        //Strings.Format((DateTime)strValue, "HH:mm:ss");
                                    }
                                }
                                break;
                        }

                        // Make sure field value does not contain field terminator or row terminator
                        strValue = strValue.Replace(FieldTerminator, m_delimeterReplacement);
                        strValue = strValue.Replace(RowTerminator, m_delimeterReplacement);

                        // Construct bulk insert row
                        if (AddedFirstInsert)
                        {
                            BulkInsertRow.Append(FieldTerminator);
                        }
                        else
                        {
                            AddedFirstInsert = true;
                        }
                        BulkInsertRow.Append(strValue);
                    }

                    BulkInsertRow.Append(RowTerminator);

                    // Add new row to temporary bulk insert file
                    bytDataRow = m_bulkInsertEncoding.GetBytes(BulkInsertRow.ToString());
                    fsBulkInsert.Write(bytDataRow, 0, bytDataRow.Length);
                }
                else
                {
                    // Handle creating Sql for inserts or updates for each row...
                    InsertSql = new StringBuilder(InsertSqlStub);
                    UpdateSql = new StringBuilder(UpdateSqlStub);
                    CountSql = new StringBuilder(CountSqlStub);
                    WhereSql = new StringBuilder();
                    AddedFirstInsert = false;
                    AddedFirstUpdate = false;

                    // Coerce all field data into proper Sql formats
                    foreach (Field fld in colFields)
                    {
                        try
                        {
                            fld.Value = fromReader[fld.Name];
                        }
                        catch (Exception ex)
                        {
                            fld.Value = "";
                            RaiseEvent_SqlFailure("Failed to get field value for [" + tblSource.Name + "." + fld.Name + "]", ex);
                        }

                        // Get translated auto-inc value for field if necessary...
                        fld.Value = DereferenceValue(tblSource, fld.Name, fld.Value);

                        // We don't attempt to insert values into auto-inc fields
                        if (fld.AutoIncrement)
                        {
                            if ((fldAutoInc != null))
                            {
                                // Even if database supports multiple autoinc fields, we can only support automatic
                                // ID translation for one because the identity Sql can only return one value...
                                if (String.Compare(fld.Name, fldAutoInc.Name) == 0) //, CompareMethod.Text) == 0)
                                {
                                    // Track original autoinc value
                                    fldAutoInc.Value = fld.Value;
                                }
                            }
                        }
                        else
                        {
                            // Get Sql encoded field value
                            strValue = fld.SqlEncodedValue;

                            // Construct Sql statements
                            if (AddedFirstInsert)
                            {
                                InsertSql.Append(", ");
                            }
                            else
                            {
                                AddedFirstInsert = true;
                            }
                            InsertSql.Append(strValue);

                            // Check to see if this is a key field
                            fldLookup = tblSource.Fields[fld.Name];
                            if ((fldLookup != null))
                            {
                                IsPrimary = fldLookup.IsPrimaryKey;
                            }
                            else
                            {
                                IsPrimary = false;
                            }

                            if (String.Compare(strValue, "NULL") != 0)//(Strings.StrComp(strValue, "NULL", CompareMethod.Text) != 0)
                            {
                                if (IsPrimary)
                                {
                                    if (WhereSql.Length == 0)
                                    {
                                        WhereSql.Append(" WHERE ");
                                    }
                                    else
                                    {
                                        WhereSql.Append(" AND ");
                                    }

                                    WhereSql.Append("[");
                                    WhereSql.Append(fld.Name);
                                    WhereSql.Append("] = ");
                                    WhereSql.Append(strValue);
                                }
                                else
                                {
                                    if (AddedFirstUpdate)
                                    {
                                        UpdateSql.Append(", ");
                                    }
                                    else
                                    {
                                        AddedFirstUpdate = true;
                                    }

                                    UpdateSql.Append("[");
                                    UpdateSql.Append(fld.Name);
                                    UpdateSql.Append("] = ");
                                    UpdateSql.Append(strValue);
                                }
                            }
                        }
                    }

                    InsertSql.Append(")");

                    if ((fldAutoInc != null) | WhereSql.Length == 0)
                    {
                        // For tables with autoinc fields that are referenced by foreign keys or
                        // tables that have no primary key fields defined, we can only do inserts...
                        try
                        {
                            // Insert record into destination table
                            if (AddedFirstInsert | (fldAutoInc != null))
                            {
                                // Added check to preserve ID number for auto-inc fields
                                if (!skipKeyValuePreservation && m_preservePrimaryKeyValue && fldAutoInc != null)
                                {
                                    //Commented Auto increment field for Primary key because if field is auto increment but not primary key then still need to preserve value before it insert to table
                                    //if (ToTable.Fields[fldAutoInc.Name].IsPrimaryKey)
                                    //{
                                    //if (ToTable.Name.ToString().ToUpper().Trim() == "Device".ToUpper())
                                    //{
                                    //Check Record Count for destination table before insert and auto increment field value

                                    OleDbCommand oCMD = ToTable.Connection.CreateCommand();
                                    oCMD.CommandText = "SELECT MAX(" + fldAutoInc.Name + ") from " + ToTable.Name;
                                    oCMD.CommandTimeout = Timeout;
                                    oCMD.CommandType = CommandType.Text;

                                    int ToTableRowCount = int.Parse(Common.NotNull(oCMD.ExecuteScalar(), "0")) + 1;
                                    int SourceTablePrimaryFldValue = int.Parse(Common.NotNull(fldAutoInc.Value, "0"));

                                    for (int i = ToTableRowCount; i < SourceTablePrimaryFldValue; i++)
                                    {
                                        // Insert record into destination table upto Identity Field Value
                                        ToTable.Connection.ExecuteNonQuery(InsertSql.ToString(), Timeout);
                                        int currentIdentityValue = int.Parse(Common.NotNull(ToTable.Connection.ExecuteScalar(ToTable.IdentitySql, Timeout), "0"));

                                        // Delete record which was just inserted
                                        ToTable.Connection.ExecuteNonQuery("DELETE FROM " + ToTable.Name + " WHERE " + fldAutoInc.Name + "=" + currentIdentityValue, Timeout);
                                    }

                                    //}
                                    //}
                                    //else
                                    //{
                                    //    //Just reset my auto increment number for Measuremnt Table Point ID currently because it is very importent field in system to keep value
                                    //    OleDbCommand oCMD = ToTable.Connection.CreateCommand();
                                    //    oCMD.CommandText = "SELECT MAX(" + fldAutoInc.Name + ") from " + ToTable.Name;
                                    //    oCMD.CommandTimeout = Timeout;
                                    //    oCMD.CommandType = CommandType.Text;
                                    //    int ToTableRowCount = int.Parse(Common.NotNull(oCMD.ExecuteScalar(), "0")) + 1;
                                    //    int SourceTablePrimaryFldValue = int.Parse(Common.NotNull(fldAutoInc.Value, "0"));
                                    //    for (int i = ToTableRowCount; i < SourceTablePrimaryFldValue; i++)
                                    //    {
                                    //        // Insert record into destination table upto Identity Field Value
                                    //        ToTable.Connection.ExecuteNonQuery(InsertSql.ToString(), Timeout);
                                    //        int currentIdentityValue = int.Parse(Common.NotNull(ToTable.Connection.ExecuteScalar(ToTable.IdentitySql, Timeout), "0"));
                                    //        //Delete record which is just inserted
                                    //        ToTable.Connection.ExecuteNonQuery("DELETE FROM " + ToTable.Name + " WHERE " + fldAutoInc.Name + "=" + currentIdentityValue, Timeout);
                                    //        //RaiseEvent_SqlFailure(ToTable.Name, new Exception("Data value set to table " + ToTable.Name + " for field " + fldAutoInc.Name));
                                    //    }
                                    //}
                                }
                                // Insert record into destination table
                                ToTable.Connection.ExecuteNonQuery(InsertSql.ToString(), Timeout);
                            }
                            // Save new destination autoinc value
                            if ((fldAutoInc != null))
                            {
                                try
                                {
                                    fldAutoInc.AutoIncrementTranslations.Add(Convert.ToString(fldAutoInc.Value), ToTable.Connection.ExecuteScalar(ToTable.IdentitySql, Timeout));
                                }
                                catch (Exception ex)
                                {
                                    RaiseEvent_SqlFailure(tblSource.IdentitySql, ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            RaiseEvent_SqlFailure(InsertSql.ToString(), ex);
                        }
                    }
                    else
                    {
                        // Add where criteria to Sql count statement
                        CountSql.Append(WhereSql.ToString());

                        // Make sure record doesn't already exist
                        try
                        {
                            // If record already exists due to triggers or other means we must update it instead of inserting it
                            OleDbCommand oCMD = ToTable.Connection.CreateCommand();
                            oCMD.CommandText = CountSql.ToString();
                            oCMD.CommandTimeout = Timeout;
                            oCMD.CommandType = CommandType.Text;
                            //if (Common.NotNull(ToTable.Connection.ExecuteScalar(CountSql.ToString(), Timeout), "0") > 0) {
                            if (int.Parse(Common.NotNull(oCMD.ExecuteScalar(), "0")) > 0)
                            {
                                // Add where criteria to Sql update statement
                                UpdateSql.Append(WhereSql.ToString());

                                try
                                {
                                    // Update record in destination table
                                    if (AddedFirstUpdate)
                                        ToTable.Connection.ExecuteNonQuery(UpdateSql.ToString(), Timeout);
                                }
                                catch (Exception ex)
                                {
                                    RaiseEvent_SqlFailure(UpdateSql.ToString(), ex);
                                }
                            }
                            else
                            {
                                try
                                {
                                    // Insert record into destination table
                                    if (AddedFirstInsert)
                                        ToTable.Connection.ExecuteNonQuery(InsertSql.ToString(), Timeout);
                                }
                                catch (Exception ex)
                                {
                                    RaiseEvent_SqlFailure(InsertSql.ToString(), ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            RaiseEvent_SqlFailure(CountSql.ToString(), ex);
                        }
                    }
                }

                ProgressIndex += 1;
                intOverallProgress += 1;
                //Raise event to check status of each row 
                RaiseEvent_RowProgress(FromTable.Name, ProgressIndex, Total);

                if (ProgressIndex % RowReportInterval == 0)
                {
                    RaiseEvent_RowProgress(FromTable.Name, ProgressIndex, Total);
                    RaiseEvent_OverallProgress((int)intOverallProgress, (int)intOverallTotal);
                }
            }

            fromReader.Close();

            if (UseBulkInsert & (fsBulkInsert != null))
            {
                string BulkInsertSql = "BULK INSERT " + ToTable.FullName + " FROM '" + BulkInsertFile + "'" + (m_bulkInsertSettings.Length > 0 ? " WITH (" + m_bulkInsertSettings + ")" : "");
                double dblStartTime = 0;
                double dblStopTime = 0;
                DateTime dtTodayMidNight = new System.DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                //DateTime dtYestMidnight = new System.DateTime(2006, 9, 12, 0, 0, 0);

                // Close bulk insert file stream
                fsBulkInsert.Close();
                fsBulkInsert = null;

                if (BulkInsertExecuting != null)
                {
                    BulkInsertExecuting(ToTable.Name);
                }

                try
                {
                    // Give system a few seconds to close bulk insert file (might have been real big)
                    TVA.IO.FilePath.WaitForReadLock(BulkInsertFile, 15);

                    TimeSpan diffResult = DateTime.Now - dtTodayMidNight;
                    dblStartTime = diffResult.TotalSeconds; //VB.DateAndTime.Timer;
                    ToTable.Connection.ExecuteNonQuery(BulkInsertSql, Timeout);
                }
                catch (Exception ex)
                {
                    if (BulkInsertException != null)
                    {
                        BulkInsertException(ToTable.Name, BulkInsertSql, ex);
                    }
                }
                finally
                {
                    TimeSpan diffResult = DateTime.Now - dtTodayMidNight;
                    dblStopTime = diffResult.TotalSeconds; // VB.DateAndTime.Timer;
                    if (Convert.ToInt32(dblStartTime) == 0)
                        dblStartTime = dblStopTime;
                }


                try
                {
                    TVA.IO.FilePath.WaitForWriteLock(BulkInsertFile, 15);
                    File.Delete(BulkInsertFile);
                }
                catch (Exception ex)
                {
                    if (BulkInsertException != null)
                    {
                        BulkInsertException(ToTable.Name, BulkInsertSql, new InvalidOperationException("Failed to delete temporary bulk insert file \"" + BulkInsertFile + "\" due to exception [" + ex.Message + "], file will need to be manually deleted.", ex));
                    }
                }

                if (BulkInsertCompleted != null)
                {
                    BulkInsertCompleted(ToTable.Name, ProgressIndex, Convert.ToInt32(dblStopTime - dblStartTime));
                }
            }

            RaiseEvent_RowProgress(FromTable.Name, Total, Total);
            RaiseEvent_OverallProgress((int)intOverallProgress, (int)intOverallTotal);

        }

        /// <summary>
        /// Lookup referencial value for source table and update their information
        /// </summary>
        /// <param name="SourceTable"></param>
        /// <param name="FieldName"></param>
        /// <param name="Value"></param>
        /// <param name="FieldStack"></param>
        /// <returns></returns>
        internal object DereferenceValue(Table SourceTable, string FieldName, object Value, ArrayList FieldStack = null)
        {

            Field fldLookup = default(Field);
            object objValue = null;

            // No need to attempt to deference null value
            if (Convert.IsDBNull(Value) || Value == null)
                return Value;

            // If this field is referenced as a foreign key field by a primary key field that is auto-incremented, we
            // translate the auto-inc value if possible
            fldLookup = SourceTable.Fields[FieldName];
            if ((fldLookup != null))
            {
                //var _with5 = fldLookup;
                if (fldLookup.IsForeignKey)
                {
                    Field referenceByField = fldLookup.ReferencedBy;
                    if (referenceByField.AutoIncrement)
                    {
                        // Return new auto-inc value
                        if (referenceByField.AutoIncrementTranslations == null)
                        {
                            return Value;
                        }
                        else
                        {
                            objValue = referenceByField.AutoIncrementTranslations[Convert.ToString(Value)];
                            return (objValue == null ? Value : objValue);
                        }
                    }
                    else
                    {
                        bool flgInStack = false;
                        int x = 0;

                        if (FieldStack == null)
                            FieldStack = new ArrayList();

                        // We don't want to circle back on ourselves
                        for (x = 0; x <= FieldStack.Count - 1; x++)
                        {
                            if (object.ReferenceEquals(fldLookup.ReferencedBy, FieldStack[x]))
                            {
                                flgInStack = true;
                                break;
                            }
                        }

                        // Traverse path to parent autoinc field if it exists
                        if (!flgInStack)
                        {
                            FieldStack.Add(fldLookup.ReferencedBy);
                            return DereferenceValue(referenceByField.Table, referenceByField.Name, Value, FieldStack);
                        }
                    }
                }
            }

            return Value;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FieldTerminator"></param>
        /// <param name="RowTerminator"></param>
        private void ParseBulkInsertSettings(ref string FieldTerminator, ref string RowTerminator)
        {
            string strSetting = null;
            string[] strKeyValue = null;

            FieldTerminator = "";
            RowTerminator = "";

            foreach (string strSetting_loopVariable in m_bulkInsertSettings.Split(','))
            {
                strSetting = strSetting_loopVariable;
                strKeyValue = strSetting.Split('=');
                if (strKeyValue.Length == 2)
                {
                    if (string.Compare(strKeyValue[0].Trim(), "FIELDTERMINATOR") == 0)
                    {
                        FieldTerminator = strKeyValue[1].Trim();
                    }
                    else if (string.Compare(strKeyValue[0].Trim(), "ROWTERMINATOR") == 0)
                    {
                        RowTerminator = strKeyValue[1].Trim();
                    }
                }
            }

            if (FieldTerminator.Length == 0)
                FieldTerminator = "\\t";
            if (RowTerminator.Length == 0)
                RowTerminator = "\\n";

            FieldTerminator = UnEncodeSetting(FieldTerminator);
            RowTerminator = UnEncodeSetting(RowTerminator);

        }

        /// <summary>
        /// Generate unencoded value for sql statement. <seealso cref="RemoveQuotes"/>
        /// </summary>
        /// <param name="Setting"></param>
        /// <returns></returns>
        private string UnEncodeSetting(string Setting)
        {

            Setting = RemoveQuotes(Setting);
            Setting = Setting.Replace("\\\\", "\\");
            Setting = Setting.Replace("\\'", "'");
            Setting = Setting.Replace("\\\"", "\"");
            Setting = Setting.Replace("\\t", "\t"); // VB.Constants.vbTab);
            Setting = Setting.Replace("\\n", "\n"); //VB.Constants.vbCrLf);
            return Setting;
        }

        /// <summary>
        /// Remove single quotes from sql statement
        /// </summary>
        /// <param name="Str">string to be checked</param>
        /// <returns></returns>
        private string RemoveQuotes(string Str)
        {
            if (Str.Substring(0, 1) == "'")
                Str = Str.Substring(2);
            if (Str.Substring(Str.Length - 1) == "'")
                Str = Str.Substring(0, Str.Length - 1);

            //if (Strings.Left(Str, 1) == "'")
            //    Str = Strings.Mid(Str, 2);
            //if (Strings.Right(Str, 1) == "'")
            //    Str = Strings.Mid(Str, 1, Strings.Len(Str) - 1);

            return Str;

        }

        #endregion
    }
}
