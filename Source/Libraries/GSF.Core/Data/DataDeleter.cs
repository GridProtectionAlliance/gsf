//******************************************************************************************************
//  DataDeleter.cs - Gbtc
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
//  11/08/2018 - Stephen Wills
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
    //  Note: user must define primary key fields in the database or through code to be used for deletes from each table
    //  in the table collection in order for the deletes to occur, hence any tables which have no key fields defined
    //  yet appear in the table collection will not have their records removed...

    /// <summary>
    /// This class defines a common set of functionality that any data operation implementation can use 
    /// </summary>
    public class DataDeleter : BulkDataOperationBase
    {
        #region [ Members ]

        // Events
        public event EventHandler Disposed;

        #endregion

        #region [ Constructors ]

        public DataDeleter()
        {
        }

        public DataDeleter(string fromConnectString, string toConnectString)
            : base(fromConnectString, toConnectString)
        {
        }

        public DataDeleter(Schema fromSchema, Schema toSchema)
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
        /// Execute this <see cref="DataDeleter"/>
        /// </summary>
        public override void Execute()
        {
            List<Table> tablesList = new List<Table>();
            Table tableLookup;
            Table table;
            int x;

            if (m_tableCollection.Count == 0)
                Analyze();

            if (m_tableCollection.Count == 0)
                throw new NullReferenceException("No tables to process even after analyze.");

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

            // Begin deleting data from destination tables - we process deletes in reverse
            // referential integrity order...
            for (x = tablesList.Count - 1; x >= 0; x--)
            {
                table = (Table)tablesList[x];

                // Lookup table name in destination data source
                tableLookup = m_toSchema.Tables.FindByMapName(table.MapName);

                if ((object)tableLookup != null)
                {
                    // We can only do Sql deletes where primary key fields are defined...
                    if (table.RowCount > 0 && (m_useFromSchemaRI ? table.PrimaryKeyFieldCount : tableLookup.PrimaryKeyFieldCount) > 0)
                    {
                        // Inform clients of table deletes
                        OnTableProgress(table.Name, true, x + 1, tablesList.Count);

                        // Delete data from destination table based on source table records
                        ExecuteDeletes(table, tableLookup);
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

                // Perform final update of progress information
                OnTableProgress("", false, tablesList.Count, tablesList.Count);
            }
        }

        /// <summary>
        /// Execute a command to delete data from source to destination table
        /// </summary>
        /// <param name="fromTable">Source table</param>
        /// <param name="toTable">Destination table</param>
        private void ExecuteDeletes(Table fromTable, Table toTable)
        {
            Table sourceTable = m_useFromSchemaRI ? fromTable : toTable;
            Field lookupField;
            Field commonField;

            // Progress process variables
            int progress = 0;
            int totalProgress;

            // Bulk delete variables
            string deleteSQLStub;
            StringBuilder deleteSQL;
            StringBuilder whereSQL;
            string value;
            bool isPrimary;

            // Create a field list of all of the common fields in both tables
            // Use toTable as the parent to retrieve the appropriate SQLEncodedValues
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

            totalProgress = fromTable.RowCount;
            OnRowProgress(fromTable.Name, 0, totalProgress);
            OnOverallProgress((int)m_overallProgress, (int)m_overallTotal);

            // Execute source query
            using (IDataReader fromReader = fromTable.Connection.ExecuteReader("SELECT " + fieldCollection.GetList(sqlEscapeFunction: fromTable.Parent.Parent.SQLEscapeName) + " FROM " + fromTable.SQLEscapedName, CommandBehavior.SequentialAccess, Timeout))
            {
                // Create Sql delete stub
                deleteSQLStub = "DELETE FROM " + toTable.SQLEscapedName;

                // Delete data for each row...
                while (fromReader.Read())
                {
                    deleteSQL = new StringBuilder(deleteSQLStub);
                    whereSQL = new StringBuilder();

                    // Coerce needed field data into proper Sql format
                    foreach (Field field in fieldCollection)
                    {
                        try
                        {
                            field.Value = fromReader[field.Name];
                        }
                        catch (Exception ex)
                        {
                            field.Value = "";
                            OnSQLFailure("Failed to get field value for [" + sourceTable.Name + "." + field.Name + "]", ex);
                        }

                        // Check to see if this is a key field
                        lookupField = sourceTable.Fields[field.Name];

                        if ((object)lookupField != null)
                            isPrimary = lookupField.IsPrimaryKey;
                        else
                            isPrimary = false;

                        // We only deal with primary key field values for Sql deletes...
                        if (isPrimary)
                        {
                            value = field.SQLEncodedValue;

                            if (!value.Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (whereSQL.Length == 0)
                                    whereSQL.Append(" WHERE ");
                                else
                                    whereSQL.Append(" AND ");

                                whereSQL.Append(field.SQLEscapedName);
                                whereSQL.Append(" = ");
                                whereSQL.Append(value);
                            }
                        }
                    }

                    if (whereSQL.Length > 0)
                    {
                        // Add where criteria to Sql delete statement
                        deleteSQL.Append(whereSQL.ToString());

                        try
                        {
                            // Delete record in destination table
                            toTable.Connection.ExecuteNonQuery(deleteSQL.ToString(), Timeout);
                        }
                        catch (Exception ex)
                        {
                            OnSQLFailure(deleteSQL.ToString(), new DataException("ERROR: No \"WHERE\" criteria was generated for Sql delete statement, primary key value missing?  Delete not performed"));
                        }
                    }

                    progress++;
                    m_overallProgress++;

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
