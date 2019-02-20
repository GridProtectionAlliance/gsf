//******************************************************************************************************
//  BulkDataOperationBase.cs - Gbtc
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
//  08/21/2008 - Mihir Brahmbhatt
//       Converted to C# extensions.
//  09/27/2010 - Mihir Brahmbhatt
//       Edited code comments.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Data
{
    #region [ IBulkDataOperation ]

    /// <summary>
    /// This is the common interface for any bulk data operation
    /// </summary>
    public interface IBulkDataOperation
    {
        /// <summary>
        /// Get the status information of table progress
        /// </summary>
        event EventHandler<EventArgs<string, bool, int, int>> TableProgress;

        /// <summary>
        /// Get the information of Row progress of table
        /// </summary>
        event EventHandler<EventArgs<string, int, int>> RowProgress;

        /// <summary>
        /// Get the information of overall progress of Migration utility
        /// </summary>
        event EventHandler<EventArgs<int, int>> OverallProgress;

        /// <summary>
        /// Get the information of exception while processing SQL statement
        /// </summary>
        event EventHandler<EventArgs<string, Exception>> SQLFailure;

        /// <summary>
        /// Get the work table information
        /// </summary>
        Tables WorkTables
        {
            get;
        }

        /// <summary>
        /// From schema information
        /// </summary>
        Schema FromSchema
        {
            get;
            set;
        }

        /// <summary>
        /// To Schema information
        /// </summary>
        Schema ToSchema
        {
            get;
            set;
        }

        /// <summary>
        /// Get the row report interval information
        /// </summary>
        int RowReportInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set time out for SQL statement
        /// </summary>
        int Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// Execute a method of object
        /// </summary>
        void Execute();


        /// <summary>
        /// Close the object connection
        /// </summary>
        void Close();
    }

    #endregion

    /// <summary>
    /// This class defines a common set of functionality that any bulk data operation implementation can use 
    /// </summary>
    public abstract class BulkDataOperationBase : IBulkDataOperation, IDisposable
    {
        #region [ Members ]

        // Fields - Variables Declaration

        /// <summary>
        /// From schema.
        /// </summary>
        protected Schema m_fromSchema;

        /// <summary>
        /// To schema.
        /// </summary>
        protected Schema m_toSchema;

        /// <summary>
        /// Implementer can use this variable to track overall progress 
        /// </summary>
        protected long m_overallProgress;

        /// <summary>
        /// This is initialized to the overall total number of records to be processed 
        /// </summary>
        protected long m_overallTotal;

        /// <summary>
        /// Defines interval for reporting row progress 
        /// </summary>
        private int m_rowReportInterval;

        /// <summary>
        /// Timeout value for SQL operation 
        /// </summary>
        protected int m_timeout;

        /// <summary>
        /// Tables value 
        /// </summary>
        protected Tables m_tableCollection;

        /// <summary>
        /// Flag to check referential integrity
        /// </summary>
        protected bool m_useFromSchemaRI;

        /// <summary>
        /// List of exclude tables
        /// </summary>
        private readonly List<string> m_excludedTables = new List<string>();

        // Events

        /// <summary>
        /// Get the status information of table progress
        /// </summary>
        public event EventHandler<EventArgs<string, bool, int, int>> TableProgress;

        /// <summary>
        /// Get the information of Row progress of table
        /// </summary>
        public event EventHandler<EventArgs<string, int, int>> RowProgress;

        /// <summary>
        /// Get the information of overall progress of Migration utility
        /// </summary>
        public event EventHandler<EventArgs<int, int>> OverallProgress;

        /// <summary>
        /// Get the information of exception while processing SQL statement
        /// </summary>
        public event EventHandler<EventArgs<string, Exception>> SQLFailure;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Default Constructor
        /// </summary>
        protected BulkDataOperationBase()
        {
            m_rowReportInterval = 5;
            m_timeout = 120;
            m_useFromSchemaRI = true;
            m_tableCollection = new Tables(m_fromSchema);
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="fromConnectString">Source database connection string</param>
        /// <param name="toConnectString">Destination database connection string</param>
        protected BulkDataOperationBase(string fromConnectString, string toConnectString)
            : this()
        {
            m_fromSchema = new Schema(fromConnectString, TableType.Table, false, false);
            m_toSchema = new Schema(toConnectString, TableType.Table, false, false);
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="fromSchema">Source Schema</param>
        /// <param name="toSchema">Destination Schema</param>
        protected BulkDataOperationBase(Schema fromSchema, Schema toSchema)
            : this()
        {
            m_fromSchema = fromSchema;
            m_toSchema = toSchema;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or set Source schema
        /// </summary>
        public virtual Schema FromSchema
        {
            get
            {
                return m_fromSchema;
            }
            set
            {
                m_fromSchema = value;
                m_tableCollection.Parent = value;
            }
        }

        /// <summary>
        /// Get or set destination schema
        /// </summary>
        public virtual Schema ToSchema
        {
            get
            {
                return m_toSchema;
            }
            set
            {
                m_toSchema = value;
            }
        }

        /// <summary>
        /// Get or set number of rows to process before raising progress events
        /// </summary>
        public virtual int RowReportInterval
        {
            get
            {
                return m_rowReportInterval;
            }
            set
            {
                m_rowReportInterval = value;
            }
        }

        /// <summary>
        /// Get or set Maximum number of seconds to wait when processing a SQL command before timing out.
        /// </summary>
        public virtual int Timeout
        {
            get
            {
                return m_timeout;
            }
            set
            {
                m_timeout = value;
            }
        }

        /// <summary>
        /// Get or set - use referential integrity information from source/to destination database during data processing
        /// </summary>
        public virtual bool UseFromSchemaReferentialIntegrity
        {
            get
            {
                return m_useFromSchemaRI;
            }
            set
            {
                m_useFromSchemaRI = value;
            }
        }

        /// <summary>
        /// These are the tables that were found in both source and destination to be used for data operation...
        /// </summary>
        public virtual Tables WorkTables
        {
            get
            {
                return m_tableCollection;
            }
        }

        /// <summary>
        /// Get list of tables to be excluded during data processing
        /// </summary>
        public List<string> ExcludedTables
        {
            get
            {
                return m_excludedTables;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            Close();
        }

        /// <summary>
        /// Close source and destination schema
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        public virtual void Close()
        {
            if ((object)m_fromSchema != null)
                m_fromSchema.Close();

            if ((object)m_toSchema != null)
                m_toSchema.Close();

            m_fromSchema = null;
            m_toSchema = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        void IDisposable.Dispose()
        {
            Close();
        }

        /// <summary>
        /// Analyze data schema before data processing
        /// </summary>
        public virtual void Analyze()
        {
            Table lookupTable;

            m_fromSchema.ImmediateClose = false;
            m_fromSchema.Analyze();

            m_toSchema.ImmediateClose = false;
            m_toSchema.Analyze();

            m_excludedTables.Sort();

            m_tableCollection.Clear();

            // We preprocess which tables we are going to access for data operation...
            foreach (Table table in m_fromSchema.Tables)
            {
                // Bypass excluded tables
                if (m_excludedTables.BinarySearch(table.MapName) < 0)
                {
                    // Lookup table name in destination data source by map name
                    lookupTable = m_toSchema.Tables.FindByMapName(table.MapName);

                    if ((object)lookupTable != null)
                    {
                        // If user requested to use referential integrity of destination tables then
                        // we use process priority of those tables instead...
                        if (!m_useFromSchemaRI)
                            table.Priority = lookupTable.Priority;

                        table.Process = true;
                        m_tableCollection.Add(table);
                    }
                }
            }

        }

        /// <summary>
        /// Executes bulk data operation.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Raise an event if table change in data processing
        /// </summary>
        /// <param name="TableName">Table name in data processing</param>
        /// <param name="Executed">Status of data processing on table</param>
        /// <param name="CurrentTable">current table index in data processing</param>
        /// <param name="TotalTables">total table count in data processing</param>
        protected virtual void OnTableProgress(string TableName, bool Executed, int CurrentTable, int TotalTables)
        {
            if ((object)TableProgress != null)
                TableProgress(this, new EventArgs<string, bool, int, int>(TableName, Executed, CurrentTable, TotalTables)); //-V3083
        }

        /// <summary>
        /// Raise an event while change row in data processing
        /// </summary>
        /// <param name="TableName">Table name in data processing</param>
        /// <param name="CurrentRow">current row index in data processing</param>
        /// <param name="TotalRows">total rows needs to be process in data processing</param>
        protected virtual void OnRowProgress(string TableName, int CurrentRow, int TotalRows)
        {
            if ((object)RowProgress != null)
                RowProgress(this, new EventArgs<string, int, int>(TableName, CurrentRow, TotalRows)); //-V3083
        }

        /// <summary>
        /// Raise an event to show overall progress of data processing
        /// </summary>
        /// <param name="Current">Current index of tables in data processing</param>
        /// <param name="Total">Total table count in data processing</param>
        protected virtual void OnOverallProgress(int Current, int Total)
        {
            if ((object)OverallProgress != null)
                OverallProgress(this, new EventArgs<int, int>(Current, Total)); //-V3083
        }

        /// <summary>
        /// Raise an event if SQL statement fail
        /// </summary>
        /// <param name="SQL">SQL statement information</param>
        /// <param name="ex">exception information</param>
        protected virtual void OnSQLFailure(string SQL, Exception ex)
        {
            if ((object)SQLFailure != null)
                SQLFailure(this, new EventArgs<string, Exception>(SQL, ex)); //-V3083
        }

        #endregion
    }
}