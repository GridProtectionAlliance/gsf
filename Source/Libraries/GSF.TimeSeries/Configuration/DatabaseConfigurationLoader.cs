//******************************************************************************************************
//  DatabaseConfigurationLoader.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  04/04/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Security;
using GSF.Units;

// ReSharper disable AccessToModifiedClosure
namespace GSF.TimeSeries.Configuration
{
    /// <summary>
    /// Defines a method signature for a bootstrap data source operation.
    /// </summary>
    /// <param name="database">Connection to database.</param>
    /// <param name="nodeIDQueryString">Formatted node ID Guid query string.</param>
    /// <param name="trackingVersion">Latest version of the configuration to which data operations were previously applied.</param>
    /// <param name="arguments">Optional data operation arguments.</param>
    /// <param name="statusMessage">Reference to host status message function.</param>
    /// <param name="processException">Reference to host process exception function.</param>
    public delegate void DataOperationFunction(AdoDataConnection database, string nodeIDQueryString, ulong trackingVersion, string arguments, Action<string> statusMessage, Action<Exception> processException);

    /// <summary>
    /// Represents a configuration loader that gets its configuration from a database connection.
    /// </summary>
    public class DatabaseConfigurationLoader : ConfigurationLoaderBase, IDisposable
    {
        #region [ Members ]

        // Fields
        private string m_connectionString;
        private string m_dataProviderString;
        private string m_nodeIDQueryString;

        private AdoDataConnection m_database;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the connection string which
        /// defines how to connect to the database.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
            }
        }

        /// <summary>
        /// Gets or sets the data provider string, which determines the
        /// .NET types to use when opening connections to the database.
        /// </summary>
        public string DataProviderString
        {
            get
            {
                return m_dataProviderString;
            }
            set
            {
                m_dataProviderString = value;
            }
        }

        /// <summary>
        /// Gets or sets the string to use in queries when filtering results by node ID.
        /// </summary>
        public string NodeIDQueryString
        {
            get
            {
                return m_nodeIDQueryString;
            }
            set
            {
                m_nodeIDQueryString = value;
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether augmentation is supported by this configuration loader.
        /// </summary>
        public override bool CanAugment
        {
            get
            {
                try
                {
                    int trackedTables = 0;
                    Execute(database => trackedTables = Convert.ToInt32(database.Connection.ExecuteScalar("SELECT COUNT(*) FROM TrackedTable")));
                    return trackedTables > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        public void Open()
        {
            using (m_database)
            {
                m_database = new AdoDataConnection(m_connectionString, m_dataProviderString);
                OnStatusMessage(MessageLevel.Info, "Database connection opened.");
            }
        }

        /// <summary>
        /// Loads the entire configuration data set from scratch.
        /// </summary>
        /// <returns>The configuration data set.</returns>
        public override DataSet Load()
        {
            DataSet configuration = null;

            Execute(database =>
            {
                ulong latestVersion;
                DataTable entities;

                configuration = new DataSet("Iaon");
                latestVersion = GetLatestVersion(0LU);
                ExecuteDataOperations();

                // Load configuration entities defined in database
                entities = GetEntities();

                // Add configuration entities table to system configuration for reference
                configuration.Tables.Add(entities.Copy());

                // Add each configuration entity to the system configuration
                foreach (DataRow entityRow in entities.Rows)
                    configuration.Tables.Add(LoadTable(entityRow));

                AddVersion(configuration, latestVersion);
                ExtractSecurityContext();
            });

            return configuration;
        }

        /// <summary>
        /// Augments the given configuration data set with the changes
        /// tracked since the version of the given configuration data set.
        /// </summary>
        /// <param name="configuration">The configuration data set to be augmented.</param>
        public override void Augment(DataSet configuration)
        {
            // Get the version of the configuration so we know
            // which changes in the change table need to be updated
            ulong currentVersion = GetVersion(configuration);

            Execute(database =>
            {
                ulong latestVersion;

                DataTable entities;
                string sourceName;
                string runtimeName;

                DataTable trackedChanges;
                string[] trackedTables;
                int changeCount;

                DataTable entityTable;
                string primaryKeyColumn;
                HashSet<string> primaryKeys;

                DataRow[] entityChanges;
                Dictionary<string, int> unchangedRecordIndexes;
                Dictionary<string, DataRow> changedRecords;
                List<int> rowsToRemove;

                Ticks operationStartTime;
                Time operationElapsedTime;
                bool success;

                // First check if we can augment
                if (!CanAugment)
                    throw new InvalidOperationException("Unable to augment configuration because it is not supported.");

                // First thing we do is get the latest version available from the TrackedChanges
                // table. This will be the version we are upgrading to. We need to do this first
                // to prevent potential race conditions that might occur between this and other
                // processes that may be modifying the database that could result in failure to
                // update changes to an augmented table
                latestVersion = GetLatestVersion(currentVersion);

                // Execute data operations before loading anything else from the database
                ExecuteDataOperations(currentVersion);

                // Load configuration entities defined in database
                entities = GetEntities();

                // If any tables exist in the configuration data set that do not exist
                // in the ConfigurationEntity table, they were either added at runtime
                // or removed from ConfigurationEntity. In either case, they can be
                // safely removed from the configuration data set
                foreach (DataTable table in configuration.Tables.Cast<DataTable>().ToList())
                {
                    if (entities.Select().All(row => row["RuntimeName"].ToNonNullString() != table.TableName))
                        configuration.Tables.Remove(table);
                }

                // Add configuration entities table to system configuration for reference
                configuration.Tables.Add(entities.Copy());

                // Get the changes since the current version of the configuration data set
                trackedChanges = GetTrackedChanges(currentVersion);

                if (TrackedChangesAreValid(currentVersion))
                {
                    // If there is a gap between the version of this configuration and the minimum version
                    // in the changes table, there might be some changes that were unaccounted for. Therefore,
                    // we pretend there are no tracked tables so that all tables will be reloaded from scratch
                    if (trackedChanges.Select().Select(row => Convert.ToUInt64(row["ID"])).DefaultIfEmpty(currentVersion + 1LU).Min() != currentVersion + 1LU)
                    {
                        currentVersion = ulong.MinValue;
                        trackedTables = new string[0];
                    }
                    else
                    {
                        trackedTables = database.Connection.RetrieveData(database.AdapterType, "SELECT Name FROM TrackedTable").Select()
                            .Select(row => row["Name"].ToNonNullString())
                            .ToArray();
                    }
                }
                else
                {
                    // If there is any tracked change version in the changes table that is smaller than the current
                    // version, there may have been an unexpected manual database change or migration. Therefore,
                    // we pretend there are no tracked tables so that all tables will be reloaded from scratch
                    currentVersion = ulong.MinValue;
                    trackedTables = new string[0];
                }

                foreach (DataRow entityRow in entities.Rows)
                {
                    success = false;

                    // Get the source name and runtime name of the table we are attempting to augment
                    sourceName = entityRow["SourceName"].ToNonNullString();
                    runtimeName = entityRow["RuntimeName"].ToNonNullString();

                    // A table can only be augmented if the table is tracked and if it exists in the current configuration
                    if (configuration.Tables.Contains(runtimeName) && trackedTables.Contains(sourceName))
                    {
                        try
                        {
                            // Get the list of changes specific to this table
                            entityChanges = trackedChanges.Select($"TableName = '{sourceName}'");

                            // If there are no changes to this table, there is nothing to do
                            if (entityChanges.Length > 0)
                            {
                                // Track the amount of time it takes to do this operation
                                operationStartTime = DateTime.UtcNow.Ticks;

                                // Get the actual table to be augmented out of the current configuration
                                entityTable = configuration.Tables[runtimeName];

                                // Get the name of the column containing the primary key for that table
                                primaryKeyColumn = entityChanges.Select(row => row["PrimaryKeyColumn"].ToNonNullString()).First();

                                // Get the distinct list of valid primary keys
                                primaryKeys = new HashSet<string>(entityChanges.Select(row => row["PrimaryKeyValue"]).Where(primaryKey => primaryKey != null && primaryKey != DBNull.Value).Select(primaryKey => primaryKey.ToString()), StringComparer.CurrentCultureIgnoreCase);

                                // Get the relevant records from the entity table as well as the updated values from the database
                                unchangedRecordIndexes = entityTable.Rows.Cast<DataRow>().Select((row, index) => Tuple.Create(row[primaryKeyColumn].ToString(), index)).Where(tuple => primaryKeys.Contains(tuple.Item1)).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.CurrentCultureIgnoreCase);
                                changedRecords = GetChangedRecords(sourceName, primaryKeyColumn, currentVersion).Rows.Cast<DataRow>().ToDictionary(row => row[primaryKeyColumn].ToString(), row => row, StringComparer.CurrentCultureIgnoreCase);
                                rowsToRemove = new List<int>();

                                // Go through the list of distinct primary key values and query
                                // the database for the latest changes to each of the modified records
                                changeCount = 0;

                                foreach (string primaryKey in primaryKeys)
                                {
                                    int unchangedIndex;
                                    DataRow changedRow;

                                    if (changedRecords.TryGetValue(primaryKey, out changedRow))
                                    {
                                        // Record was inserted or modified - add or
                                        // update it in the augmented configuration
                                        if (!unchangedRecordIndexes.TryGetValue(primaryKey, out unchangedIndex))
                                        {
                                            unchangedIndex = entityTable.Rows.Count;
                                            entityTable.Rows.Add(entityTable.NewRow());
                                        }

                                        Update(entityTable.Rows[unchangedIndex], changedRow);
                                    }
                                    else
                                    {
                                        // Record was removed so we hang onto the index so we can
                                        // remove it from the augmented configuration at the end
                                        if (unchangedRecordIndexes.TryGetValue(primaryKey, out unchangedIndex))
                                            rowsToRemove.Add(unchangedIndex);
                                    }

                                    changeCount++;
                                }

                                // Remove rows in descending index order so that indexes
                                // don't change for rows that need to be removed
                                foreach (int index in rowsToRemove.OrderByDescending(index => index))
                                {
                                    int lastIndex = entityTable.Rows.Count - 1;

                                    if (index != lastIndex)
                                        Update(entityTable.Rows[index], entityTable.Rows[lastIndex]);

                                    entityTable.Rows.RemoveAt(lastIndex);
                                }

                                // Get the amount of time it took to augment the table
                                operationElapsedTime = (DateTime.UtcNow.Ticks - operationStartTime).ToSeconds();

                                // Display information to the user
                                OnStatusMessage(MessageLevel.Info, $"Loaded {changeCount} change{(changeCount == 1 ? "" : "s")} to \"{runtimeName}\" in {operationElapsedTime.ToString(3)}...");
                            }

                            success = true;
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Unable to augment {runtimeName} table due to exception: {ex.Message}", ex));
                        }
                    }

                    if (!success)
                    {
                        // If we were unable to augment the table, we need to
                        // drop the current configuration and load the whole table
                        if (configuration.Tables.Contains(runtimeName))
                            configuration.Tables.Remove(runtimeName);

                        configuration.Tables.Add(LoadTable(entityRow));
                    }
                }

                AddVersion(configuration, latestVersion);

                try
                {
                    database.Connection.ExecuteNonQuery($"DELETE FROM TrackedChange WHERE ID <= {latestVersion}");
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Unable to curtail the TrackedChange table due to exception: {ex.Message}", ex));
                }
            });
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public void Close()
        {
            if ((object)m_database != null)
            {
                m_database.Dispose();
                m_database = null;

                OnStatusMessage(MessageLevel.Info, "Database connection closed.");
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="DatabaseConfigurationLoader"/> object.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        private void ExecuteDataOperations(ulong trackingVersion = ulong.MinValue)
        {
            Execute(database =>
            {
                string assemblyName = "", typeName = "", methodName = "", arguments;
                Assembly assembly;
                Type type;
                MethodInfo method;

                foreach (DataRow row in database.Connection.RetrieveData(database.AdapterType, $"SELECT * FROM DataOperation WHERE (NodeID IS NULL OR NodeID={m_nodeIDQueryString}) AND Enabled <> 0 ORDER BY LoadOrder").Rows)
                {
                    try
                    {
                        OnStatusMessage(MessageLevel.Info, $"Executing startup data operation \"{row["Description"].ToNonNullString("Unlabeled")}\".");

                        // Load data operation parameters
                        assemblyName = row["AssemblyName"].ToNonNullString();
                        typeName = row["TypeName"].ToNonNullString();
                        methodName = row["MethodName"].ToNonNullString();
                        arguments = row["Arguments"].ToNonNullString();

                        if (string.IsNullOrWhiteSpace(assemblyName))
                            throw new InvalidOperationException("Data operation assembly name was not defined.");

                        if (string.IsNullOrWhiteSpace(typeName))
                            throw new InvalidOperationException("Data operation type name was not defined.");

                        if (string.IsNullOrWhiteSpace(methodName))
                            throw new InvalidOperationException("Data operation method name was not defined.");

                        // Load data operation from containing assembly and type
                        assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(assemblyName));
                        type = assembly.GetType(typeName);
                        method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod);

                        // Execute data operation via loaded assembly method
                        ((DataOperationFunction)Delegate.CreateDelegate(typeof(DataOperationFunction), method))(database, m_nodeIDQueryString, trackingVersion, arguments, status => OnStatusMessage(MessageLevel.Info, status), ex => OnProcessException(MessageLevel.Warning, ex));
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to execute startup data operation \"{assemblyName} [{typeName}::{methodName}()]\" due to exception: {ex.Message}", ex));
                    }
                }
            });
        }

        private DataTable LoadTable(DataRow entityRow)
        {
            DataTable destination = null;

            Execute(database =>
            {
                DataTable source;
                DataColumnCollection columns;
                Dictionary<int, int> columnIndex;

                Ticks operationStartTime;
                Time operationElapsedTime;

                // Load configuration entity data filtered by node ID
                operationStartTime = DateTime.UtcNow.Ticks;
                source = database.Connection.RetrieveData(database.AdapterType, $"SELECT * FROM {entityRow["SourceName"]} WHERE NodeID={m_nodeIDQueryString}");
                operationElapsedTime = (DateTime.UtcNow.Ticks - operationStartTime).ToSeconds();

                // Update table name as defined in configuration entity
                source.TableName = entityRow["RuntimeName"].ToString();

                OnStatusMessage(MessageLevel.Info, $"Loaded {source.Rows.Count} row{(source.Rows.Count == 1 ? "" : "s")} from \"{source.TableName}\" in {operationElapsedTime.ToString(3)}...");

                operationStartTime = DateTime.UtcNow.Ticks;

                // Clone data source
                destination = source.Clone();

                // Get destination column collection
                columns = destination.Columns;

                // Remove redundant node ID column
                columns.Remove("NodeID");

                // Pre-cache column index translation after removal of NodeID column to speed data copy
                columnIndex = new Dictionary<int, int>();

                foreach (DataColumn column in columns)
                {
                    columnIndex[column.Ordinal] = source.Columns[column.ColumnName].Ordinal;
                }

                // Manually copy-in each row into table
                foreach (DataRow sourceRow in source.Rows)
                {
                    DataRow newRow = destination.NewRow();

                    // Copy each column of data in the current row
                    for (int x = 0; x < columns.Count; x++)
                    {
                        newRow[x] = sourceRow[columnIndex[x]];
                    }

                    // Add new row to destination table
                    destination.Rows.Add(newRow);
                }

                operationElapsedTime = (DateTime.UtcNow.Ticks - operationStartTime).ToSeconds();

                OnStatusMessage(MessageLevel.Info, $"{source.TableName} configuration pre-cache completed in {operationElapsedTime.ToString(3)}.");
            });

            return destination;
        }

        private DataTable GetEntities()
        {
            DataTable entities = null;

            Execute(database =>
            {
                entities = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM ConfigurationEntity WHERE Enabled <> 0 ORDER BY LoadOrder");
                entities.TableName = "ConfigurationEntity";
            });

            return entities;
        }

        private ulong GetVersion(DataSet configuration)
        {
            try
            {
                return configuration.Tables["ConfigurationDataSet"].Select()
                    .Select(row => row.ConvertField<ulong>("Version"))
                    .First();
            }
            catch
            {
                return ulong.MinValue;
            }
        }

        private ulong GetLatestVersion(ulong currentVersion)
        {
            ulong version = ulong.MinValue;

            Execute(database =>
            {
                try
                {
                    string query = $"SELECT CASE WHEN COUNT(ID) = 0 THEN {currentVersion} ELSE MAX(ID) END FROM TrackedChange";
                    version = Convert.ToUInt64(database.Connection.ExecuteScalar(query));
                }
                catch
                {
                    version = ulong.MinValue;
                }
            });

            return version;
        }

        private bool TrackedChangesAreValid(ulong currentVersion)
        {
            bool changesAreValid = false;

            Execute(database =>
            {
                try
                {
                    string query = $"SELECT COUNT(ID) FROM TrackedChange WHERE ID < {currentVersion}";
                    changesAreValid = Convert.ToInt32(database.Connection.ExecuteScalar(query)) == 0;
                }
                catch
                {
                    changesAreValid = false;
                }
            });

            return changesAreValid;
        }

        private DataTable GetTrackedChanges(ulong currentVersion)
        {
            DataTable table = null;
            Execute(database => table = database.Connection.RetrieveData(database.AdapterType, $"SELECT * FROM TrackedChange WHERE ID > {currentVersion}"));
            return table;
        }

        private DataTable GetChangedRecords(string tableName, string primaryKeyColumn, ulong currentVersion)
        {
            DataTable changes = null;

            Execute(database =>
            {
                string query = string.Format("SELECT * FROM {0} WHERE {1} IN (SELECT PrimaryKeyValue FROM TrackedChange WHERE TableName = '{0}' AND ID > {2}) AND NodeID = {3}", tableName, primaryKeyColumn, currentVersion, m_nodeIDQueryString);
                changes = database.Connection.RetrieveData(database.AdapterType, query);
            });

            return changes;
        }

        private void Update(DataRow row, DataRow updates)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (updates.Table.Columns.Contains(column.ColumnName))
                    row[column] = updates[column.ColumnName];
            }
        }

        private void AddVersion(DataSet configuration, ulong version)
        {
            DataTable configurationDataSetTable = configuration.Tables.Add("ConfigurationDataSet");
            configurationDataSetTable.Columns.Add("Version");
            configurationDataSetTable.Rows.Add(version);
        }

        private void ExtractSecurityContext()
        {
            Execute(database =>
            {
                Ticks operationStartTime;
                Time operationElapsedTime;

                // Extract and begin cache of current security context - this does not require an existing security provider
                OnStatusMessage(MessageLevel.Info, "Preparing current security context...", flags: MessageFlags.SecurityMessage);

                operationStartTime = DateTime.UtcNow.Ticks;
                AdoSecurityProvider.ExtractSecurityContext(database.Connection, ex => OnProcessException(MessageLevel.Warning, ex, flags: MessageFlags.SecurityMessage));
                operationElapsedTime = (DateTime.UtcNow.Ticks - operationStartTime).ToSeconds();

                OnStatusMessage(MessageLevel.Info, $"Security context prepared in {operationElapsedTime.ToString(3)}.");
                OnStatusMessage(MessageLevel.Info, "Database configuration successfully loaded.", flags: MessageFlags.SecurityMessage);
            });
        }

        private void Execute(Action<AdoDataConnection> action)
        {
            bool isOpen = (object)m_database != null;

            try
            {
                if (!isOpen)
                    Open();

                action(m_database);
            }
            finally
            {
                if (!isOpen)
                    Close();
            }
        }

        #endregion
    }
}
