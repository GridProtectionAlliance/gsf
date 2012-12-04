//******************************************************************************************************
//  ErrorMonitor.cs - Gbtc
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
//  3/26/2012 - prasanthgs
//       Generated original version of source code.
//  04/12/2012 - prasanthgs
//       Reworked as per the comments of codeplex reviewers.
//       Reusing ErrorLog instead of creating new table ExceptionLog.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using TimeSeriesFramework.UI.DataModels;
using TVA.Data;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Provides Error Monitor that checks the ErrorLog
    /// to keep the Error Log list updated.
    /// </summary>
    /// <remarks>
    /// Typically class should be implemented as a singleton since one instance will create.
    /// </remarks>
    public class ErrorMonitor : IDisposable
    {
        #region [ Members ]

        // Constants
        private const int DefaultRefreshInterval = 10;

        /// <summary>
        /// Specifies the default value for the <see cref="DefaultErrorLogSize"/> property.
        /// </summary>
        private const int DefaultErrorLogSize = 2000;

        /// <summary>
        /// Event raised when the ErrorList is updated.
        /// </summary>
        public event EventHandler<EventArgs> UpdatedErrors;

        // Fields
        private int m_refreshInterval;
        private object m_errLock;
        private List<ErrorLog> m_ErrorList;
        private System.Timers.Timer m_refreshTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorMonitor"/> class.
        /// </summary>
        /// <param name="singleton">Indicates whether this instance should 
        /// be singleton. Default value false</param>
        public ErrorMonitor(bool singleton = false)
        {
            m_refreshInterval = DefaultRefreshInterval;

            m_errLock = new object();
            m_ErrorList = new List<ErrorLog>();
            m_refreshTimer = new System.Timers.Timer(m_refreshInterval * 1000);
            m_refreshTimer.Elapsed += m_refreshTimer_Elapsed;

            if (singleton)
                Default = this;
        }

        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets or sets the interval, in seconds,
        /// between refreshing the error list.
        /// </summary>
        public int RefreshInterval
        {
            get
            {
                return m_refreshInterval;
            }
            set
            {
                m_refreshInterval = value;
                m_refreshTimer.Interval = m_refreshInterval * 1000;
            }
        }

        #endregion

        #region [ Methods ]

        private void m_refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshErrorsList();
        }

        /// <summary>
        /// Reset the refresh interval of Error Monitor to 
        /// default value =  10 sec(s)
        /// </summary>
        public void ResetRefreshInterval()
        {
            RefreshInterval = DefaultRefreshInterval;
        }
        /// <summary>
        /// Starts the refresh timer that checks recent errors.
        /// </summary>
        public void Start()
        {
            if (m_disposed)
            {
                m_refreshTimer = new System.Timers.Timer(m_refreshInterval * 1000);
                m_refreshTimer.Elapsed += m_refreshTimer_Elapsed;
                m_disposed = false;
            }
            if (!m_refreshTimer.Enabled)
            {
                RefreshErrorsList();
                m_refreshTimer.Start();
            }
        }

        /// <summary>
        /// Stops the refresh timer.
        /// </summary>
        public void Stop()
        {
            if (m_refreshTimer != null)
            {
                if (m_refreshTimer.Enabled)
                    m_refreshTimer.Stop();
            }
        }

        /// <summary>
        /// Gets current Errors.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<ErrorLog> GetRecentErrors()
        {
            lock (m_errLock)
            {
                return new ObservableCollection<ErrorLog>(m_ErrorList);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ErrorMonitor"/> 
        /// object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_refreshTimer != null)
                        {
                            m_refreshTimer.Dispose();
                            m_refreshTimer = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;
                }
            }
        }

        /// <summary>
        /// Fetch recent error details from ErrorLog table.
        /// </summary>
        private void RefreshErrorsList()
        {
            bool createdConnection = false;
            AdoDataConnection database = null;
            List<ErrorLog> newTempList = null;

            try
            {
                createdConnection = DataModelBase.CreateConnection(ref database);

                DataTable ErrorLogTable = database.Connection.RetrieveData(database.AdapterType,
                    "SELECT ID, Source, Type, Message, Detail, CreatedOn FROM ErrorLog ORDER BY ID DESC");

                // Load only first 2000 Error information to list.
                newTempList = ErrorLogTable.Rows.Cast<DataRow>()
                    .Select(row => new ErrorLog()
                    {
                        ID = row.ConvertField<int>("ID"),
                        Source = row.Field<String>("Source"),
                        Type = row.Field<String>("Type"),
                        Message = row.Field<String>("Message"),
                        Detail = row.Field<String>("Detail"),
                        CreatedOn = row.Field<DateTime>("CreatedOn")
                    })
                    .Distinct()
                    .Take(DefaultErrorLogSize)
                    .ToList();

                // Update the error list
                lock (m_errLock)
                {
                    m_ErrorList = newTempList;
                }

                // Notify that the ErrorList have been updated
                OnUpdatedErrors();
            }
            catch (Exception)
            {
                // Do nothing, if exception raised while logging Errors
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        // Triggers the updated ErrorList event.
        private void OnUpdatedErrors()
        {
            if (UpdatedErrors != null)
                UpdatedErrors(this, new EventArgs());
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Gets ErrorMonitor object.
        /// </summary>
        public static ErrorMonitor Default { get; set; }

        #endregion
    }
}
