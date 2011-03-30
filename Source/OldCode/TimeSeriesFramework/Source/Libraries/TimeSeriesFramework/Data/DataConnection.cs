//******************************************************************************************************
//  DataConnection.cs - Gbtc
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
//  03/23/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using TVA;
using TVA.Configuration;

namespace TimeSeriesFramework.Data
{
    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> to configured ADO.NET data source.
    /// </summary>
    public class DataConnection : IDisposable
    {
        #region [ Members ]
             
        // Fields
        private IDbConnection m_connection;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataConnection"/>.
        /// </summary>
        public DataConnection()
        {
            // Only need to establish data types and load settings once
            if (s_connectionType == null || string.IsNullOrEmpty(s_connectionString))
            {
                try
                {
                    // Load connection settings from the system settings category				
                    ConfigurationFile config = ConfigurationFile.Current; //new ConfigurationFile("~/web.config", ApplicationType.Web);
                    CategorizedSettingsElementCollection configSettings = config.Settings["systemSettings"];

                    string dataProviderString = configSettings["DataProviderString"].Value;
                    s_connectionString = configSettings["ConnectionString"].Value;

                    if (string.IsNullOrEmpty(s_connectionString))
                        throw new NullReferenceException("ConnectionString setting was undefined.");

                    if (string.IsNullOrEmpty(dataProviderString))
                        throw new NullReferenceException("DataProviderString setting was undefined.");

                    // Attempt to load configuration from an ADO.NET database connection
                    Dictionary<string, string> settings;
                    string assemblyName, connectionTypeName, adapterTypeName;
                    Assembly assembly;

                    settings = dataProviderString.ParseKeyValuePairs();
                    assemblyName = settings["AssemblyName"].ToNonNullString();
                    connectionTypeName = settings["ConnectionType"].ToNonNullString();
                    adapterTypeName = settings["AdapterType"].ToNonNullString();

                    if (string.IsNullOrEmpty(connectionTypeName))
                        throw new NullReferenceException("Database connection type was undefined.");

                    if (string.IsNullOrEmpty(adapterTypeName))
                        throw new NullReferenceException("Database adapter type was undefined.");

                    assembly = Assembly.Load(new AssemblyName(assemblyName));
                    s_connectionType = assembly.GetType(connectionTypeName);
                    s_adapterType = assembly.GetType(adapterTypeName);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to load defined data provider - check \"DataProviderString\" in configuration file: " + ex.Message, ex);
                }
            }

            try
            {
                // Open ADO.NET provider connection
                m_connection = (IDbConnection)Activator.CreateInstance(s_connectionType);
                m_connection.ConnectionString = s_connectionString;
                m_connection.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open data connection - check \"ConnectionString\" in configuration file: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="DataConnection"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~DataConnection()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets an open <see cref="IDbConnection"/> to configured ADO.NET data source.
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return m_connection;
            }
        }

        public Type AdapterType
        {
            get { return s_adapterType; }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataConnection"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataConnection"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_connection != null)
                            m_connection.Dispose();
                        m_connection = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion
        
        #region [ Static ]

        // Static Fields
        static Type s_connectionType;
        static Type s_adapterType;
        static string s_connectionString;

        #endregion
        
    }
}
