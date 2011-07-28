//******************************************************************************************************
//  CommonFunctions.cs - Gbtc
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
//  03/31/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using TimeSeriesFramework.UI.DataModels;
using TVA;
using TVA.Data;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents a static class containing common methods.
    /// </summary>
    public static class CommonFunctions
    {
        #region [ Members ]

        /// <summary>
        /// Defines the default settings category for TimeSeriesFramework data connections.
        /// </summary>
        public const string DefaultSettingsCategory = "SystemSettings";

        #endregion

        #region [ Static ]

        // Static Fields

        private static Guid s_currentNodeID;
        private static string s_remoteStatusServerConnectionString;
        private static string s_dataPublisherPort;
        private static string s_realTimeStatisticServiceUrl;
        private static string s_timeSeriesDataServiceUrl;
        private static WindowsServiceClient s_windowsServiceClient;
        private static bool s_retryServiceConnection;

        // Static Properties

        /// <summary>
        /// Defines the current user name as defined in the Thread.CurrentPrincipal.Identity.
        /// </summary>
        public static readonly string CurrentUser = Thread.CurrentPrincipal.Identity.Name;

        // Static Methods

        #region [AdoDataConnection Extension Methods]

        /// <summary>
        /// Sets the current user context for the database.
        /// </summary>
        /// <remarks>
        /// Purpose of this method is to supply current user information from the UI to DELETE trigger for change logging.
        /// This method must be called before any delete operation on the database in order to log who deleted this record.
        /// For SQL server it sets user name into CONTEXT_INFO().
        /// For MySQL server it sets user name into session variable @context.
        /// MS Access is not supported for change logging.
        /// For any other database in the future, such as Oracle, this logic must be extended to support change log in the database.
        /// </remarks>
        /// <param name="database"><see cref="AdoDataConnection"/> used to set user context before any delete operation.</param>
        public static void SetCurrentUserContext(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                if (!string.IsNullOrEmpty(CurrentUser))
                {
                    if (database == null)
                    {
                        database = new AdoDataConnection(DefaultSettingsCategory);
                        createdConnection = true;
                    }

                    IDbCommand command;
                    string connectionType = database.Connection.GetType().Name.ToLower();

                    // Set Current User for the database session for this connection.

                    switch (connectionType)
                    {
                        case "sqlconnection":
                            string contextSql = "DECLARE @context VARBINARY(128)\n SELECT @context = CONVERT(VARBINARY(128), CONVERT(VARCHAR(128), @userName))\n SET CONTEXT_INFO @context";
                            command = database.Connection.CreateCommand();
                            command.CommandText = contextSql;
                            command.AddParameterWithValue("@userName", CurrentUser);
                            command.ExecuteNonQuery();
                            break;
                        case "mysqlconnection":
                            command = database.Connection.CreateCommand();
                            command.CommandText = "SET @context = '" + CurrentUser + "';";
                            command.ExecuteNonQuery();
                            break;
                        default:
                            break;
                    }
                }
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Method to check if source database is Microsoft Access.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>Boolean, indicating if source database is Microsoft Access.</returns>
        public static bool IsJetEngine(this AdoDataConnection database)
        {
            // TODO: Make this a cached property of AdoDataConnection as an optimization...
            return database.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB");
        }

        /// <summary>
        /// Method to check if source database is MySQL.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>Boolean, indicating if source database is MySQL.</returns>
        public static bool IsMySQL(this AdoDataConnection database)
        {
            return database.AdapterType.Name == "MySqlDataAdapter";
        }

        /// <summary>
        /// Method to check if source database is SQLite.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>Boolean, indicating if source database is MySQL.</returns>
        public static bool IsSqlite(this AdoDataConnection database)
        {
            return database.AdapterType.Name == "SQLiteDataAdapter";
        }

        /// <summary>
        /// Retrieves connection string to connect to backend windows service.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>IP address and port on which backend windows service is running.</returns>
        public static string RemoteStatusServerConnectionString(this AdoDataConnection database)
        {
            if (string.IsNullOrEmpty(s_remoteStatusServerConnectionString))
                database.GetNodeSettings();

            return s_remoteStatusServerConnectionString;
        }

        /// <summary>
        /// Retrieves web service url to query real time statistics values.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>string, url to web service.</returns>
        public static string RealTimeStatisticServiceUrl(this AdoDataConnection database)
        {
            if (string.IsNullOrEmpty(s_realTimeStatisticServiceUrl))
                database.GetNodeSettings();

            return s_realTimeStatisticServiceUrl;
        }

        /// <summary>
        /// Retrieves a port number on which back end service is publishing data.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>port number on which data is being published.</returns>
        public static string DataPublisherPort(this AdoDataConnection database)
        {
            if (string.IsNullOrEmpty(s_dataPublisherPort))
                database.GetNodeSettings();

            return s_dataPublisherPort;
        }

        /// <summary>
        /// Retrieves web serivce url to query real time data.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>string, url to web service.</returns>
        public static string TimeSeriesDataServiceUrl(this AdoDataConnection database)
        {
            if (string.IsNullOrEmpty(s_timeSeriesDataServiceUrl))
                database.GetNodeSettings();

            return s_timeSeriesDataServiceUrl;
        }

        /// <summary>
        /// Method to parse Settings field value for current node defined in the database and extract various parameters to communicate with backend windows service.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        private static void GetNodeSettings(this AdoDataConnection database)
        {
            Node node = Node.GetCurrentNode(database);
            if (node != null)
            {
                Dictionary<string, string> settings = node.Settings.ToLower().ParseKeyValuePairs();

                if (settings.ContainsKey("remotestatusserverconnectionstring"))
                    s_remoteStatusServerConnectionString = settings["remotestatusserverconnectionstring"];

                if (settings.ContainsKey("realtimestatisticserviceurl"))
                    s_realTimeStatisticServiceUrl = settings["realtimestatisticserviceurl"];

                if (settings.ContainsKey("datapublisherport"))
                    s_dataPublisherPort = settings["datapublisherport"];

                if (settings.ContainsKey("timeseriesdataserviceurl"))
                    s_timeSeriesDataServiceUrl = settings["timeseriesdataserviceurl"];
            }
        }

        /// <summary>
        /// Returns current node id <see cref="System.Guid"/> UI is connected to.
        /// </summary>
        /// <param name="database">Connected <see cref="AdoDataConnection"/></param>
        /// <returns>Proper <see cref="System.Guid"/> implementation for current node id.</returns>
        public static object CurrentNodeID(this AdoDataConnection database)
        {
            if (s_currentNodeID == null)
                return database.Guid(System.Guid.Empty);

            return database.Guid(s_currentNodeID);
        }

        /// <summary>
        /// Returns proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="database">Connected <see cref="AdoDataConnection"/>.</param>
        /// <param name="guid"><see cref="System.Guid"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public static object Guid(this AdoDataConnection database, Guid guid)
        {
            if (database.IsJetEngine())
                return "{" + guid.ToString() + "}";

            if (database.IsSqlite())
                return guid.ToString();

            //return "P" + guid.ToString();

            return guid;
        }

        /// <summary>
        /// Retrieves <see cref="System.Guid"/> based on database type.
        /// </summary>
        /// <param name="database">Connected <see cref="AdoDataConnection"/>.</param>
        /// <param name="row"><see cref="DataRow"/> from which value needs to be retrieved.</param>
        /// <param name="fieldName">Name of the field which contains <see cref="System.Guid"/>.</param>
        /// <returns><see cref="System.Guid"/>.</returns>
        public static Guid Guid(this AdoDataConnection database, DataRow row, string fieldName)
        {
            if (database.IsJetEngine() || database.IsMySQL() || database.IsSqlite())
                return System.Guid.Parse(row.Field<object>(fieldName).ToString());

            return row.Field<Guid>(fieldName);
        }

        /// <summary>
        /// Returns current UTC time in implementation that is proper for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="database">Connected <see cref="AdoDataConnection"/>.</param>
        /// <param name="usePrecisionTime">Set to <c>true</c> to use precision time.</param>
        /// <returns>Current UTC time in implementation that is proper for connected <see cref="AdoDataConnection"/> database type.</returns>
        public static object UtcNow(this AdoDataConnection database, bool usePrecisionTime = false)
        {
            if (usePrecisionTime)
            {
                if (database.IsJetEngine())
                    return PrecisionTimer.UtcNow.ToOADate();

                return PrecisionTimer.UtcNow;
            }

            if (database.IsJetEngine())
                return DateTime.UtcNow.ToOADate();

            return DateTime.UtcNow;
        }

        #endregion

        /// <summary>
        /// Assigns <see cref="CurrentNodeID"/> based ID of currently active node.
        /// </summary>
        /// <param name="nodeID">Current node ID <see cref="CurrentNodeID"/> to assign.</param>
        public static void SetAsCurrentNodeID(this Guid nodeID)
        {
            s_currentNodeID = nodeID;

            // When node selection changes, reset other static members related to node.
            s_remoteStatusServerConnectionString = string.Empty;
            s_realTimeStatisticServiceUrl = string.Empty;
            s_dataPublisherPort = string.Empty;
            s_timeSeriesDataServiceUrl = string.Empty;
            SetRetryServiceConnection(true);
            DisconnectWindowsServiceClient();
            ConnectWindowsServiceClient();
        }

        /// <summary>
        /// Returns <see cref="DBNull"/> if given <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="value">Value to test for null.</param>
        /// <returns><see cref="DBNull"/> if <paramref name="value"/> is <c>null</c>; otherwise <paramref name="value"/>.</returns>
        public static object ToNotNull(this object value)
        {
            if (value == null)
                return (object)DBNull.Value;
            if (value is int && (int)value == 0)
                return (object)DBNull.Value;

            return value;
        }

        /// <summary>
        /// Returns a collection of down sampling methods.
        /// </summary>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection of down sampling methods.</returns>
        public static Dictionary<string, string> GetDownsamplingMethodLookupList()
        {
            Dictionary<string, string> downsamplingLookupList = new Dictionary<string, string>();

            downsamplingLookupList.Add("LastReceived", "LastReceived");
            downsamplingLookupList.Add("Closest", "Closest");
            downsamplingLookupList.Add("Filtered", "Filtered");
            downsamplingLookupList.Add("BestQuality", "BestQuality");

            return downsamplingLookupList;
        }

        /// <summary>
        /// Returns a collection of system time zones.
        /// </summary>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection of system time zones.</returns>
        public static Dictionary<string, string> GetTimeZones(bool isOptional)
        {
            Dictionary<string, string> timeZonesList = new Dictionary<string, string>();

            if (isOptional)
                timeZonesList.Add("", "Select Time Zone");

            foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
            {
                if (!timeZonesList.ContainsKey(timeZoneInfo.Id))
                    timeZonesList.Add(timeZoneInfo.Id, timeZoneInfo.DisplayName);
            }

            return timeZonesList;
        }

        /// <summary>
        /// Retrieves children of an UIElement based on type.
        /// </summary>
        /// <param name="parent">Parent UIElement.</param>
        /// <param name="targetType">Type of child UIElement looking for within parent UIElement.</param>
        /// <param name="children">Reference paramter to return child collection.</param>
        public static void GetChildren(UIElement parent, Type targetType, ref List<UIElement> children)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);
                    if (child.GetType() == targetType)
                    {
                        children.Add(child);
                    }
                    GetChildren(child, targetType, ref children);
                }
            }
        }

        /// <summary>
        /// Retrieves first child of an UIElement based on type.
        /// </summary>
        /// <param name="parent">Parent UIElement</param>
        /// <param name="targetType">Type of the child UIElement.</param>
        /// <param name="element">Reference parameter to return UIElement.</param>
        public static void GetFirstChild(UIElement parent, Type targetType, ref UIElement element)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);
                    if (child.GetType() == targetType)
                    {
                        element = child;
                        break;
                    }

                    GetFirstChild(child, targetType, ref element);
                }
            }
        }

        /// <summary>
        /// Retrieves runtime id for an object.
        /// </summary>
        /// <param name="sourceTable">Table where object has been defined.</param>
        /// <param name="sourceID">ID of an object in source table.</param>
        /// <returns>string, id of an object in Runtime table.</returns>
        public static string GetRuntimeID(string sourceTable, int sourceID)
        {
            string runtimeID = string.Empty;
            AdoDataConnection database = null;
            try
            {
                database = new AdoDataConnection(DefaultSettingsCategory);
                object id = database.Connection.ExecuteScalar("SELECT ID FROM Runtime WHERE SourceTable = @sourceTable AND SourceID = @sourceID", sourceTable, sourceID);
                if (id != null)
                    runtimeID = id.ToString();

                return runtimeID;
            }
            finally
            {
                if (database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Sets a boolean flag indicating if connection cycle should be continued.
        /// </summary>
        /// <param name="retry"></param>
        public static void SetRetryServiceConnection(bool retry)
        {
            s_retryServiceConnection = retry;
            if (!retry)
                DisconnectWindowsServiceClient();
        }

        /// <summary>
        /// Retrieves <see cref="WindowsServiceClient"/> object.
        /// </summary>
        /// <returns><see cref="WindowsServiceClient"/> object.</returns>
        public static WindowsServiceClient GetWindowsServiceClient()
        {
            ConnectWindowsServiceClient();
            return s_windowsServiceClient;
        }

        /// <summary>
        /// Connects to backend windows service.
        /// </summary>
        public static void ConnectWindowsServiceClient()
        {
            if (s_windowsServiceClient == null || s_windowsServiceClient.Helper.RemotingClient.CurrentState != TVA.Communication.ClientState.Connected)
            {
                if (s_windowsServiceClient != null)
                    DisconnectWindowsServiceClient();

                AdoDataConnection database = new AdoDataConnection(DefaultSettingsCategory);
                try
                {
                    string connectionString = database.RemoteStatusServerConnectionString();

                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        s_windowsServiceClient = new WindowsServiceClient(connectionString);
                        s_windowsServiceClient.Helper.RemotingClient.MaxConnectionAttempts = -1;

                        System.Threading.ThreadPool.QueueUserWorkItem(ConnectAsync, null);
                    }
                }
                finally
                {
                    if (database != null)
                        database.Dispose();
                }
            }
        }

        /// <summary>
        /// Connects asynchronously to backend windows service.
        /// </summary>
        /// <param name="state">paramter used.</param>
        private static void ConnectAsync(object state)
        {
            try
            {
                if (s_windowsServiceClient != null && s_retryServiceConnection)
                    s_windowsServiceClient.Helper.Connect();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to connect to service: " + ex.Message, ex.InnerException ?? ex);
            }
        }

        /// <summary>
        /// Disconnects from backend windows service.
        /// </summary>
        public static void DisconnectWindowsServiceClient()
        {
            try
            {
                if (s_windowsServiceClient != null)
                {
                    s_windowsServiceClient.Dispose();
                    s_windowsServiceClient = null;
                }
            }
            catch
            {
                // TODO: Log into database error log.
            }
        }

        /// <summary>
        /// Sends command to backend windows service via <see cref="WindowsServiceClient"/> object.
        /// </summary>
        /// <param name="command">command to be sent.</param>
        /// <returns>string, indicating success.</returns>
        public static string SendCommandToService(string command)
        {
            if (s_windowsServiceClient != null && s_windowsServiceClient.Helper.RemotingClient.CurrentState == TVA.Communication.ClientState.Connected)
                s_windowsServiceClient.Helper.SendRequest(command);
            else
                throw new ApplicationException("Application is currently disconnected from service.");

            return "Successfully sent " + command + " command.";
        }

        /// <summary>
        /// Retrieves a list of <see cref="StopBits"/>.
        /// </summary>
        /// <returns>Collection of <see cref="StopBits"/> as a <see cref="List{T}"/>.</returns>
        public static List<string> GetStopBits()
        {
            List<string> stopBitsList = new List<string>();

            foreach (string stopBit in Enum.GetNames(typeof(StopBits)))
                stopBitsList.Add(stopBit);

            return stopBitsList;
        }

        /// <summary>
        /// Retrieves a list of <see cref="Parity"/>.
        /// </summary>
        /// <returns>Collection of <see cref="Parity"/> as a <see cref="List{T}"/>.</returns>
        public static List<string> GetParities()
        {
            List<string> parityList = new List<string>();

            foreach (string parity in Enum.GetNames(typeof(Parity)))
                parityList.Add(parity);

            return parityList;
        }

        /// <summary>
        /// Converts xml element to datatype
        /// </summary>
        /// <param name="xmlValue"></param>
        /// <param name="xmlDataType"></param>
        /// <returns></returns>
        public static object ConvertValueToType(string xmlValue, string xmlDataType)
        {
            Type dataType = Type.GetType(xmlDataType);
            float value;

            if (float.TryParse(xmlValue, out value))
            {
                switch (xmlDataType)
                {
                    case "System.DateTime":
                        return new DateTime((long)value);
                    default:
                        return Convert.ChangeType(value, dataType);
                }
            }

            return "".ConvertToType<object>(dataType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public static KeyValuePair<int?, int?> GetMinMaxPointIDs(AdoDataConnection connection, Guid nodeID)
        {
            KeyValuePair<int?, int?> minMaxPointIDs = new KeyValuePair<int?, int?>(1, 5000);
            bool createdConnection = false;

            try
            {
                createdConnection = DataModelBase.CreateConnection(ref connection);

                DataTable results = connection.Connection.RetrieveData(connection.AdapterType, "SELECT MIN(PointID) AS MinPointID, MAX(PointID) AS MaxPointID FROM MeasurementDetail WHERE NodeID = @nodeID", connection.Guid(nodeID));

                foreach (DataRow row in results.Rows)
                {
                    minMaxPointIDs = new KeyValuePair<int?, int?>(row.ConvertNullableField<int>("MinPointID"), row.ConvertNullableField<int>("MaxPointID"));
                }
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }

            return minMaxPointIDs;
        }

        /// <summary>
        /// Stores exception in the database
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> object to connect to database</param>
        /// <param name="source">Source of exception</param>
        /// <param name="ex">Exception to be logged</param>
        public static void LogException(AdoDataConnection connection, string source, Exception ex)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = DataModelBase.CreateConnection(ref connection);

                connection.Connection.ExecuteNonQuery("INSERT INTO ErrorLog (Source, Message, Detail) VALUES (@source, @message, @detail", DataModelBase.DefaultTimeout, source, ex.Message, ex.ToString());
            }
            catch
            {
                //Do nothing.  Don't worry about it
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }
        }

        #endregion

    }
}
