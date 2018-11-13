//******************************************************************************************************
//  DataUpdater.cs - Gbtc
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
//  11/12/2018 - Stephen Wills
//       Generated original version of source code from code written in 2003.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace GSF.Data
{
    // Note: user must define primary key fields in the database or through code to be used for updates for each table
    // in the table collection in order for the updates to occur, hence any tables which have no key fields defined
    // yet appear in the table collection will not be updated...

    /// <summary>
    /// This class defines a common set of functionality that any data operation implementation can use 
    /// </summary>
    public class DataUpdater : BulkDataOperationBase
    {
        #region [ Members ]

        // Events
        public event EventHandler Disposed;

        #endregion

        #region [ Constructors ]

        public DataUpdater()
        {
        }

        public DataUpdater(string fromConnectString, string toConnectString)
            : base(fromConnectString, toConnectString)
        {
        }

        public DataUpdater(Schema fromSchema, Schema toSchema)
            : base(fromSchema, toSchema)
        {
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
        /// Execute this <see cref="DataUpdater"/>
        /// </summary>
        public override void Execute()
        {
            List<Table> tablesList = new List<Table>();
            Table tableLookup;
            int x;

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

            // Begin updating data in destination tables
            for (x = 0; x <= tablesList.Count - 1; x++)
            {
                Table table = tablesList[x];

                // Lookup table name in destination datasource
                tableLookup = ToSchema.Tables.FindByMapName(table.MapName);

                if ((object)tableLookup != null)
                {
                    // We can only do Sql updates where key fields are defined...
                    if (table.RowCount > 0 && (m_useFromSchemaRI ? table.PrimaryKeyFieldCount : tableLookup.PrimaryKeyFieldCount) > 0)
                    {
                        // Inform clients of table update
                        OnTableProgress(table.Name, true, x + 1, tablesList.Count);

                        // Update destination table based on source table records
                        ExecuteUpdates(table, tableLookup);
                    }
                    else
                    {
                        // Inform clients of table skip
                        OnTableProgress(table.Name, false, x + 1, tablesList.Count);

                        // If we skipped rows because of lack of key fields, make sure and
                        // synchronize overall progress event
                        if (table.RowCount > 0)
                        {
                            m_overallProgress += table.RowCount;
                            OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);
                        }
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
        /// Execute a command to update data from source to destination table
        /// </summary>
        /// <param name="fromTable">Source table</param>
        /// <param name="toTable">Destination table</param>
        private void ExecuteUpdates(Table fromTable, Table toTable)
        {
            Table sourceTable = m_useFromSchemaRI ? fromTable : toTable;
            Field lookupField;
            Field commonField;

            // Progress process variables
            int progress = 0;
            int totalProgress;

            // Bulk update variables
            string updateSQLStub;
            StringBuilder updateSQL;
            StringBuilder whereSQL;
            string value;
            bool isPrimary;
            bool addedFirstUpdate;

            // Create a field list of all of the common fields in both tables
            Fields fieldsCollection = new Fields(sourceTable);

            foreach (Field fld in fromTable.Fields)
            {
                // Lookup field name in destination table
                lookupField = toTable.Fields[fld.Name];

                if ((object)lookupField != null)
                {
                    // We currently don't handle binary fields...
                    if (!(fld.Type == OleDbType.Binary || fld.Type == OleDbType.LongVarBinary || fld.Type == OleDbType.VarBinary) && !(lookupField.Type == OleDbType.Binary || lookupField.Type == OleDbType.LongVarBinary || lookupField.Type == OleDbType.VarBinary))
                    {
                        // Copy field information from destination field
                        if (m_useFromSchemaRI)
                        {
                            commonField = new Field(fieldsCollection, fld.Name, fld.Type);
                            commonField.AutoIncrement = fld.AutoIncrement;
                        }
                        else
                        {
                            commonField = new Field(fieldsCollection, lookupField.Name, lookupField.Type);
                            commonField.AutoIncrement = lookupField.AutoIncrement;
                        }

                        fieldsCollection.Add(commonField);
                    }
                }
            }

            // Exit if no common field names were found
            if (fieldsCollection.Count == 0)
            {
                m_overallProgress += fromTable.RowCount;
                return;
            }

            totalProgress = fromTable.RowCount;
            OnRowProgress(fromTable.Name, 0, totalProgress);
            OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);

            // Execute source query
            using (IDataReader fromReader = fromTable.Connection.ExecuteReader("SELECT " + fieldsCollection.GetList() + " FROM " + fromTable.FullName, CommandBehavior.SequentialAccess, Timeout))
            {
                // Create Sql update stub
                updateSQLStub = "UPDATE " + toTable.FullName + " SET ";

                // Insert data for each row...
                while (fromReader.Read())
                {
                    updateSQL = new StringBuilder(updateSQLStub);
                    whereSQL = new StringBuilder();
                    addedFirstUpdate = false;

                    // Coerce all field data into proper Sql format
                    foreach (Field fld in fieldsCollection)
                    {
                        try
                        {
                            fld.Value = fromReader[fld.Name];
                        }
                        catch (Exception ex)
                        {
                            fld.Value = "";
                            OnSQLFailure("Failed to get field value for [" + sourceTable.Name + "." + fld.Name + "]", ex);
                        }

                        // Check to see if this is a key field
                        lookupField = sourceTable.Fields[fld.Name];

                        if ((object)lookupField != null)
                            isPrimary = lookupField.IsPrimaryKey;
                        else
                            isPrimary = false;

                        value = fld.SQLEncodedValue;

                        if (!value.Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (isPrimary)
                            {
                                if (whereSQL.Length == 0)
                                    whereSQL.Append(" WHERE ");
                                else
                                    whereSQL.Append(" AND ");

                                whereSQL.Append("[");
                                whereSQL.Append(fld.Name);
                                whereSQL.Append("] = ");
                                whereSQL.Append(value);
                            }
                            else
                            {
                                if (addedFirstUpdate)
                                    updateSQL.Append(", ");
                                else
                                    addedFirstUpdate = true;

                                updateSQL.Append("[");
                                updateSQL.Append(fld.Name);
                                updateSQL.Append("] = ");
                                updateSQL.Append(value);
                            }
                        }
                    }

                    if (whereSQL.Length == 0)
                    {
                        // Add where criteria to Sql update statement
                        updateSQL.Append(whereSQL.ToString());

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
                        OnSQLFailure(updateSQL.ToString(), new DataException("ERROR: No \"WHERE\" criteria was generated for Sql update statement, primary key value missing?  Update not performed"));
                    }

                    progress += 1;
                    m_overallProgress += 1;

                    if (progress % RowReportInterval == 0)
                    {
                        OnRowProgress(fromTable.Name, progress, totalProgress);
                        OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);
                    }
                }

                fromReader.Close();
            }

            OnRowProgress(fromTable.Name, totalProgress, totalProgress);
            OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);
        }

        #endregion
    }
}
