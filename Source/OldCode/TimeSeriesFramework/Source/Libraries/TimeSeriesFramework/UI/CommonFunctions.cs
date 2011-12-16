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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TimeSeriesFramework.UI.DataModels;
using TVA;
using TVA.Data;
using TVA.Security;

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
        // TODO: Remove s_remoteStatusServerConnectionString and s_dataPublisherPort and corresponding references.
        //private static string s_remoteStatusServerConnectionString;
        //private static string s_dataPublisherPort;
        private static string s_serviceConnectionString;
        private static string s_dataPublisherConnectionString;
        private static string s_realTimeStatisticServiceUrl;
        private static string s_timeSeriesDataServiceUrl;
        private static WindowsServiceClient s_windowsServiceClient;
        private static bool s_retryServiceConnection;

        // Static Properties


        public static SecurityPrincipal CurrentPrincipal = Thread.CurrentPrincipal as SecurityPrincipal;

        /// <summary>
        /// Defines the current user name as defined in the Thread.CurrentPrincipal.Identity.
        /// </summary>
        public static string CurrentUser = Thread.CurrentPrincipal.Identity.Name;

        // Events

        /// <summary>
        /// Used to notify main window that connection to service has changed.
        /// </summary>
        public static event EventHandler ServiceConntectionRefreshed = delegate { };

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
        /// For Oracle server it sets user name into context package.
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
                        case "oracleconnection":
                            command = database.Connection.CreateCommand();
                            command.CommandText = "BEGIN context.set_current_user('" + CurrentUser + "'); END;";
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

        ///// <summary>
        ///// Retrieves connection string to connect to backend windows service.
        ///// </summary>
        ///// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        ///// <returns>IP address and port on which backend windows service is running.</returns>
        //public static string RemoteStatusServerConnectionString(this AdoDataConnection database)
        //{
        //    if (string.IsNullOrEmpty(s_remoteStatusServerConnectionString))
        //        database.GetNodeSettings();

        //    return s_remoteStatusServerConnectionString;
        //}

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

        ///// <summary>
        ///// Retrieves a port number on which back end service is publishing data.
        ///// </summary>
        ///// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        ///// <returns>port number on which data is being published.</returns>
        //public static string DataPublisherPort(this AdoDataConnection database)
        //{
        //    if (string.IsNullOrEmpty(s_dataPublisherPort))
        //        database.GetNodeSettings();

        //    return s_dataPublisherPort;
        //}

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
        /// Retrieves connection string information to connect to backed windows service from UI.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <param name="overwrite">Flag that determines if cached connection string should be overwritten.</param>
        /// <returns>Connection string to connect to backend windows service.</returns>
        public static string ServiceConnectionString(this AdoDataConnection database, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(s_serviceConnectionString) || overwrite)
                database.GetNodeSettings();

            return s_serviceConnectionString;
        }

        /// <summary>
        /// Retrieves connection string to subscribe data from data published hosted by the backend windows service.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database.</param>
        /// <returns>Connection string to subscribe from data publisher.</returns>
        public static string DataPublisherConnectionString(this AdoDataConnection database)
        {
            if (string.IsNullOrEmpty(s_dataPublisherConnectionString))
                database.GetNodeSettings();

            return s_dataPublisherConnectionString;
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
                string interfaceValue = string.Empty;
                Dictionary<string, string> settings = node.Settings.ToLower().ParseKeyValuePairs();

                if (settings.ContainsKey("realtimestatisticserviceurl"))
                    s_realTimeStatisticServiceUrl = settings["realtimestatisticserviceurl"];

                if (settings.ContainsKey("timeseriesdataserviceurl"))
                    s_timeSeriesDataServiceUrl = settings["timeseriesdataserviceurl"];

                if (settings.ContainsKey("interface"))
                    interfaceValue = settings["interface"];

                if (settings.ContainsKey("remotestatusserverconnectionstring"))
                {
                    //s_remoteStatusServerConnectionString = settings["remotestatusserverconnectionstring"];
                    Dictionary<string, string> serviceSettings = settings["remotestatusserverconnectionstring"].ParseKeyValuePairs();

                    if (serviceSettings.ContainsKey("interface"))
                        interfaceValue = serviceSettings["interface"];

                    if (serviceSettings.ContainsKey("server"))
                    {
                        string server = serviceSettings["server"];

                        s_serviceConnectionString = "server=" + server;

                        if (!string.IsNullOrEmpty(interfaceValue))
                            s_serviceConnectionString += ";interface=" + interfaceValue;

                        if (serviceSettings.ContainsKey("datapublisherport"))
                        {
                            s_dataPublisherConnectionString = "server=" + server.Substring(0, server.LastIndexOf(":") + 1) + serviceSettings["datapublisherport"];

                            if (!string.IsNullOrEmpty(interfaceValue))
                                s_dataPublisherConnectionString += ";interface=" + interfaceValue;
                        }
                        else if (settings.ContainsKey("datapublisherport"))
                        {
                            s_dataPublisherConnectionString = "server=" + server.Substring(0, server.LastIndexOf(":") + 1) + settings["datapublisherport"];

                            if (!string.IsNullOrEmpty(interfaceValue))
                                s_dataPublisherConnectionString += ";interface=" + interfaceValue;
                        }
                    }
                }
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

        #endregion

        /// <summary>
        /// Assigns <see cref="CurrentNodeID"/> based ID of currently active node.
        /// </summary>
        /// <param name="nodeID">Current node ID <see cref="CurrentNodeID"/> to assign.</param>
        public static void SetAsCurrentNodeID(this Guid nodeID)
        {
            s_currentNodeID = nodeID;

            // When node selection changes, reset other static members related to node.
            //s_remoteStatusServerConnectionString = string.Empty;
            s_serviceConnectionString = string.Empty;
            s_dataPublisherConnectionString = string.Empty;
            s_realTimeStatisticServiceUrl = string.Empty;
            //s_dataPublisherPort = string.Empty;
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
                string query = database.ParameterizedQueryString("SELECT ID FROM Runtime WHERE SourceTable = {0} AND SourceID = {1}", "sourceTable", "sourceID");
                object id = database.Connection.ExecuteScalar(query, sourceTable, sourceID);
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
        public static void ConnectWindowsServiceClient(bool overwrite = false)
        {
            if (overwrite)
            {
                DisconnectWindowsServiceClient();
                ServiceConntectionRefreshed(null, EventArgs.Empty);
            }
            else
            {
                if (s_windowsServiceClient == null || s_windowsServiceClient.Helper.RemotingClient.CurrentState != TVA.Communication.ClientState.Connected)
                {
                    if (s_windowsServiceClient != null)
                        DisconnectWindowsServiceClient();

                    AdoDataConnection database = new AdoDataConnection(DefaultSettingsCategory);
                    try
                    {
                        string connectionString = database.ServiceConnectionString(true);   //.RemoteStatusServerConnectionString();

                        if (!string.IsNullOrWhiteSpace(connectionString))
                        {
                            s_windowsServiceClient = new WindowsServiceClient(connectionString);
                            s_windowsServiceClient.Helper.RemotingClient.MaxConnectionAttempts = -1;
                            s_windowsServiceClient.Helper.RemotingClient.ConnectionEstablished += new EventHandler(RemotingClient_ConnectionEstablished);
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
        }

        static void RemotingClient_ConnectionEstablished(object sender, EventArgs e)
        {
            ServiceConntectionRefreshed(null, EventArgs.Empty);
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
        /// Returns min and max point id values for a given node.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="nodeID">ID of the node to filter data.</param>
        /// <returns>KeyValuePair containing min and max point id.</returns>
        public static KeyValuePair<int?, int?> GetMinMaxPointIDs(AdoDataConnection connection, Guid nodeID)
        {
            KeyValuePair<int?, int?> minMaxPointIDs = new KeyValuePair<int?, int?>(1, 5000);
            bool createdConnection = false;

            try
            {
                createdConnection = DataModelBase.CreateConnection(ref connection);

                string query = connection.ParameterizedQueryString("SELECT MIN(PointID) AS MinPointID, MAX(PointID) AS MaxPointID FROM MeasurementDetail WHERE NodeID = {0}", "nodeID");
                DataTable results = connection.Connection.RetrieveData(connection.AdapterType, query, connection.Guid(nodeID));

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
                string query;

                createdConnection = DataModelBase.CreateConnection(ref connection);
                query = connection.ParameterizedQueryString("INSERT INTO ErrorLog (Source, Message, Detail) VALUES ({0}, {1}, {2})", "source", "message", "detail");
                connection.Connection.ExecuteNonQuery(query, DataModelBase.DefaultTimeout, source, ex.Message, ex.ToString());
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

        /// <summary>
        /// Loads provided user control into the frame control inside main window.
        /// </summary>
        /// <param name="userControl">User control to be loaded.</param>
        /// <param name="title">Title of the user control to be loaded.</param>
        public static void LoadUserControl(object userControl, string title)
        {
            UIElement frame = null;
            UIElement groupBox = null;
            TimeSeriesFramework.UI.CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(System.Windows.Controls.Frame), ref frame);
            TimeSeriesFramework.UI.CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(GroupBox), ref groupBox);

            if (frame != null)
            {
                Run run = new Run();
                run.FontWeight = FontWeights.Bold;
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                run.Text = title;

                TextBlock txt = new TextBlock();
                txt.Padding = new Thickness(5.0);
                txt.Inlines.Add(run);

                ((System.Windows.Controls.Frame)frame).Navigate(userControl, txt);

                if (groupBox != null)
                    ((GroupBox)groupBox).Header = txt;
            }
        }

        #endregion

    }
}
