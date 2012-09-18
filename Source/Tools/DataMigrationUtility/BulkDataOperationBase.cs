//******************************************************************************************************
//  BulkDataOperationBase.cs - Gbtc
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
//  08/21/2008 - Mihir Brahmbhatt
//       Converted to C# extensions.
//  09/27/2010 - Mihir Brahmbhatt
//       Edited code comments.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Database
{
    #region [ TableProgressEventHandler ]

    /// <summary>
    /// Initializes a new instance of the <see cref="TableProgressEventHandler"/> class.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    public class TableProgressEventHandler<T1, T2, T3, T4> : EventArgs
    {
        #region [ Members ]

        //Variables declaration
        public string TableName;
        public bool Executed;
        public int CurrentTable;
        public int TotalTables;


        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Default constructor
        /// </summary>
        public TableProgressEventHandler()
            : this(default(string), default(bool), default(int), default(int))
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="tableName">Table is currently in progress for migration</param>
        /// <param name="executed">Status of table execution</param>
        /// <param name="currentTable">Current index of table count</param>
        /// <param name="totalTables">Total tables need to be processed</param>
        public TableProgressEventHandler(string tableName, bool executed, int currentTable, int totalTables)
        {
            TableName = tableName;
            Executed = executed;
            CurrentTable = currentTable;
            TotalTables = totalTables;
        }

        #endregion
    }

    #endregion

    #region  [ RowProgressEventHandler ]

    /// <summary>
    /// Initializes a new instance of the <see cref="RowProgressEventHandler"/> class.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class RowProgressEventHandler<T1, T2, T3> : EventArgs
    {
        #region [ Members ]

        //Variables Declaration
        public string TableName;
        public int CurrentRow;
        public int TotalRows;


        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Default constructor
        /// </summary>
        public RowProgressEventHandler()
            : this(default(string), default(int), default(int))
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="tableName">Table name for which rows are in progress</param>
        /// <param name="currentRow">Current index of row</param>
        /// <param name="totalRows">Total rows in table</param>
        public RowProgressEventHandler(string tableName, int currentRow, int totalRows)
        {
            TableName = tableName;
            CurrentRow = currentRow;
            TotalRows = totalRows;
        }

        #endregion
    }

    #endregion

    #region [ OverallProgressEventHandler ]

    /// <summary>
    /// Initializes a new instance of the <see cref="OverallProgressEventHandler"/> class.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class OverallProgressEventHandler<T1, T2> : EventArgs
    {
        #region [ Members ]

        //Variables Declaration
        public int Current;
        public int Total;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Default constructor
        /// </summary>
        public OverallProgressEventHandler()
            : this(default(int), default(int))
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="current">Currenct Index of overall progress</param>
        /// <param name="total">Total Table of overall progress</param>
        public OverallProgressEventHandler(int current, int total)
        {
            Current = current;
            Total = total;
        }

        #endregion
    }

    #endregion

    #region [ OverallProgressEventHandler ]

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlFailureEventHandler"/> class.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class SqlFailureEventHandler<T1, T2> : EventArgs
    {
        #region [ Members ]

        //Variables Declaration
        public string Sql;
        public Exception Ex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlFailureEventHandler()
            : this(default(string), default(Exception))
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="sql">sql statement that executed</param>
        /// <param name="ex">Exception occured while executing a sql statement</param>
        public SqlFailureEventHandler(string sql, Exception ex)
        {
            Sql = sql;
            Ex = ex;
        }

        #endregion

    }

    #endregion


    #region [ IBulkDataOperation ]

    /// <summary>
    /// This is the common interface for any bulk data operation
    /// </summary>
    public interface IBulkDataOperation
    {
        /// <summary>
        /// Get the status information of table progress
        /// </summary>
        event EventHandler<TableProgressEventHandler<string, bool, int, int>> TableProgress;

        /// <summary>
        /// Get the information of Row progress of table
        /// </summary>
        event EventHandler<RowProgressEventHandler<string, int, int>> RowProgress;

        /// <summary>
        /// Get the information of overall progress of Migration utility
        /// </summary>
        event EventHandler<OverallProgressEventHandler<int, int>> OverallProgress;

        /// <summary>
        /// Get the information of exception while processing sql statement
        /// </summary>
        event EventHandler<SqlFailureEventHandler<string, Exception>> SqlFailure;

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
        /// Get or set time out for sql statement
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
        protected ISite ComponentSite;
        protected Schema schFrom;
        protected Schema schTo;

        /// <summary>
        /// Implemetor can use this variable to track overall progress 
        /// </summary>
        protected long intOverallProgress;

        /// <summary>
        /// This is initialized to the overall total number of records to be processed 
        /// </summary>
        protected long intOverallTotal;

        /// <summary>
        /// Defines interval for reporting row progress 
        /// </summary>
        private int intRowReportInterval;

        /// <summary>
        /// Timeout value for Sql operation 
        /// </summary>
        protected int intTimeout;

        /// <summary>
        /// Tables value 
        /// </summary>
        protected Tables colTables;

        /// <summary>
        /// Flag to check Referencial Integrity
        /// </summary>
        protected bool flgUseFromSchemaRI;

        /// <summary>
        /// List of exclude tables
        /// </summary>
        private List<string> m_excludedTables = new List<string>();

        // Events
        public event EventHandler<TableProgressEventHandler<string, bool, int, int>> TableProgress;
        public event EventHandler<RowProgressEventHandler<string, int, int>> RowProgress; //.RowProgress;
        public event EventHandler<OverallProgressEventHandler<int, int>> OverallProgress;
        public event EventHandler<SqlFailureEventHandler<string, Exception>> SqlFailure;
        // Delegates
        public delegate void TableProgressEventHandler(string TableName, bool Executed, int CurrentTable, int TotalTables);
        public delegate void RowProgressEventHandler(string TableName, int CurrentRow, int TotalRows);
        public delegate void OverallProgressEventHandler(int Current, int Total);
        public delegate void SqlFailureEventHandler(string Sql, Exception ex);


        #endregion



        #region [ Constructors ]

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BulkDataOperationBase()
        {
            intRowReportInterval = 5;
            intTimeout = 120;
            flgUseFromSchemaRI = true;

        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="FromConnectString">Source database connection string</param>
        /// <param name="ToConnectString">Destination database connection string</param>
        public BulkDataOperationBase(string FromConnectString, string ToConnectString)
            : this()
        {
            schFrom = new Schema(FromConnectString, TableType.Table, false, false);
            schTo = new Schema(ToConnectString, TableType.Table, false, false);

        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="FromSchema">Source Schema</param>
        /// <param name="ToSchema">Destination Schema</param>
        public BulkDataOperationBase(Schema FromSchema, Schema ToSchema)
            : this()
        {
            schFrom = FromSchema;
            schTo = ToSchema;

        }

        #endregion

        //protected override void Finalize()
        //{
        //    Dispose(true);

        //}

        #region [ Properties ]

        /// <summary>
        /// Get or Set Source schema
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Source schema definition.")]
        public virtual Schema FromSchema
        {
            get
            {
                return schFrom;
            }
            set
            {
                schFrom = value;
                if ((ComponentSite != null))
                    if (ComponentSite.DesignMode)
                        schFrom.ImmediateClose = false;
            }
        }

        /// <summary>
        /// Get or Set destination schema
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Destination schema definition.")]
        public virtual Schema ToSchema
        {
            get
            {
                return schTo;
            }
            set
            {
                schTo = value;
                if ((ComponentSite != null))
                    if (ComponentSite.DesignMode)
                        schTo.ImmediateClose = false;
            }
        }

        /// <summary>
        /// Get or Set number of rows to process before raising progress events
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Number of rows to process before raising progress events."), DefaultValue(5)]
        public virtual int RowReportInterval
        {
            get
            {
                return intRowReportInterval;
            }
            set
            {
                intRowReportInterval = value;
            }
        }

        /// <summary>
        /// Get or Set Maximum number of seconds to wait when processing a Sql command before timing out.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Maximum number of seconds to wait when processing a Sql command before timing out."), DefaultValue(120)]
        public virtual int Timeout
        {
            get
            {
                return intTimeout;
            }
            set
            {
                intTimeout = value;
            }
        }

        /// <summary>
        /// Get or Set - use referential integrity information from source/to destination database during data processing
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Set to True to use referential integrity information from source database during data processing, set to False to use destination database."), DefaultValue(true)]
        public virtual bool UseFromSchemaReferentialIntegrity
        {
            get
            {
                return flgUseFromSchemaRI;
            }
            set
            {
                flgUseFromSchemaRI = value;
            }
        }

        /// <summary>
        /// These are the tables that were found in both source and dest to be used for data operation...
        /// </summary>
        [Browsable(false)]
        public virtual Tables WorkTables
        {
            get
            {
                return colTables;
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
        /// Close source and destinaction schema
        /// </summary>
        public virtual void Close()
        {
            if ((schFrom != null))
                schFrom.Close();
            if ((schTo != null))
                schTo.Close();
            schFrom = null;
            schTo = null;
            GC.SuppressFinalize(this);

        }
        /// <summary>
        /// Dispose
        /// </summary>
        void IDisposable.Dispose()
        {
            Close();
        }

        /// <summary>
        /// Analyze data schema before data processing
        /// </summary>
        public virtual void Analyze()
        {
            //Table tbl;// = default(Table);
            Table tblLookup;// = default(Table);

            schFrom.ImmediateClose = false;
            schFrom.Analyze();

            schTo.ImmediateClose = false;
            schTo.Analyze();

            m_excludedTables.Sort();

            colTables = new Tables(schFrom);

            // We preprocess which tables we are going to access for data operation...
            foreach (Table tbl in schFrom.Tables)
            {
                // Bypass excluded tables
                if (m_excludedTables.BinarySearch(tbl.MapName) < 0)
                {
                    // Lookup table name in destination datasource by map name
                    tblLookup = schTo.Tables.FindByMapName(tbl.MapName);

                    if ((tblLookup != null))
                    {
                        intOverallTotal += tbl.RowCount;

                        // If user requested to use referential integrity of destination tables then
                        // we use process priority of those tables instead...
                        if (!flgUseFromSchemaRI)
                            tbl.Priority = tblLookup.Priority;

                        tbl.Process = true;
                        colTables.Add(tbl);
                    }
                }
            }

        }

        public abstract void Execute();


        //Derived classes can't directly raise base class events, hence the following... 

        /// <summary>
        /// Raise an event if table change in data processing
        /// </summary>
        /// <param name="TableName">Table name in data processing</param>
        /// <param name="Executed">Status of data processing on table</param>
        /// <param name="CurrentTable">current table index in data processing</param>
        /// <param name="TotalTables">total table count in data processing</param>
        protected virtual void RaiseEvent_TableProgress(string TableName, bool Executed, int CurrentTable, int TotalTables)
        {
            if (TableProgress != null)
            {
                TableProgress(this, new TableProgressEventHandler<string, bool, int, int>(TableName, Executed, CurrentTable, TotalTables));
                //TableProgress(this, new TableProgressEventHandler(this,<TableName, Executed, CurrentTable, TotalTables>)); //(eventhandler(new EventArgs{TableName, Executed, CurrentTable, TotalTables});
            }

        }

        /// <summary>
        /// Raise an event while chage row in data processing
        /// </summary>
        /// <param name="TableName">Table name in data processing</param>
        /// <param name="CurrentRow">current row index in data processing</param>
        /// <param name="TotalRows">total rows needs to be process in data processing</param>
        protected virtual void RaiseEvent_RowProgress(string TableName, int CurrentRow, int TotalRows)
        {
            if (RowProgress != null)
            {
                RowProgress(this, new RowProgressEventHandler<string, int, int>(TableName, CurrentRow, TotalRows));
                //RowProgress(TableName, CurrentRow, TotalRows);
            }

        }

        /// <summary>
        /// Raise an event to show overall progress of data processing
        /// </summary>
        /// <param name="Current">Current index of tables in data processing</param>
        /// <param name="Total">Total table count in data processing</param>
        protected virtual void RaiseEvent_OverallProgress(int Current, int Total)
        {
            if (OverallProgress != null)
            {
                OverallProgress(this, new OverallProgressEventHandler<int, int>(Current, Total));
                //OverallProgress(Current, Total);
            }

        }

        /// <summary>
        /// Raise an event if sql statement fail
        /// </summary>
        /// <param name="Sql">sql statement information</param>
        /// <param name="ex">exception information</param>
        protected virtual void RaiseEvent_SqlFailure(string Sql, Exception ex)
        {
            if (SqlFailure != null)
            {
                SqlFailure(this, new SqlFailureEventHandler<string, Exception>(Sql, ex));
                //SqlFailure(Sql, ex);
            }

        }

        #endregion
    }

}