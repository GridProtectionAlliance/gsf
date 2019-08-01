//******************************************************************************************************
//  Node.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  04/18/2011 - Mehulbhai P Thakkar
//       Added static methods for database operations.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.Data;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Node"/> information as defined in the database.
    /// </summary>
    public class Node : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_id;
        private string m_name;
        private int? m_companyID;
        private decimal? m_longitude;
        private decimal? m_latitude;
        private string m_description;
        private string m_imagePath;
        private string m_settings;
        private string m_menuType;
        private string m_menuData;
        private bool m_master;
        private int m_loadOrder;
        private bool m_enabled;
        private string m_remoteStatusServiceUrl;
        private string m_realTimeStatisticServiceUrl;
        private string m_companyName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        private bool m_settingsUpdated;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s ID.
        /// </summary>
        public Guid ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Name.
        /// </summary>
        [Required(ErrorMessage = "Node name is a required field, please provide a value")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Comapny ID.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public int? CompanyID
        {
            get
            {
                return m_companyID;
            }
            set
            {
                m_companyID = value;
                OnPropertyChanged("CompanyID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Longitude.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        [RegularExpression(@"^[-]?([0-9]{1,3})?([.][0-9]{1,6})?$", ErrorMessage = "Invalid value. Please provide value in decimal(9,6) format.")]
        public decimal? Longitude
        {
            get
            {
                return m_longitude;
            }
            set
            {
                m_longitude = value;
                OnPropertyChanged("Longitude");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Latitude.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        [RegularExpression(@"^[-]?([0-9]{1,3})?([.][0-9]{1,6})?$", ErrorMessage = "Invalid value. Please provide value in decimal(9,6) format.")]
        public decimal? Latitude
        {
            get
            {
                return m_latitude;
            }
            set
            {
                m_latitude = value;
                OnPropertyChanged("Latitude");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Description.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Image.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string ImagePath
        {
            get
            {
                return m_imagePath;
            }
            set
            {
                m_imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Settings.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string Settings
        {
            get
            {
                return m_settings;
            }
            set
            {
                if (m_settings != value)
                    m_settingsUpdated = true;

                m_settings = value;
                OnPropertyChanged("Settings");
            }
        }

        /// <summary>
        /// Gets flag that determines if settings have been updated.
        /// </summary>
        public bool SettingsUpdated
        {
            get
            {
                return m_settingsUpdated;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s MenuType.
        /// </summary>
        [Required(ErrorMessage = "Node menuType is a required field, please provide a value")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        [DefaultValue("File")]
        public string MenuType
        {
            get
            {
                return m_menuType;
            }
            set
            {
                m_menuType = value;
                OnPropertyChanged("MenuType");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s MenuData.
        /// </summary>
        [Required(ErrorMessage = "Node MenuData is a required field, please provide a value")]
        [DefaultValue("Menu.xml")]
        public string MenuData
        {
            get
            {
                return m_menuData;
            }
            set
            {
                m_menuData = value;
                OnPropertyChanged("MenuData");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Node"/> is the master <see cref="Node"/>.
        /// </summary>
        [DefaultValue(0)]
        public bool Master
        {
            get
            {
                return m_master;
            }
            set
            {
                m_master = value;
                OnPropertyChanged("Master");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Load Order.
        /// </summary>
        [DefaultValue(0)]
        public int LoadOrder
        {
            get
            {
                return m_loadOrder;
            }
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Node"/> is enabled.
        /// </summary>
        [DefaultValue(0)]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        ///// <summary>
        ///// Gets or sets the current <see cref="Node"/>'s Time Series Data Service URL.
        ///// </summary>
        //[DataType(DataType.Url, ErrorMessage="Time Series Data Service URL is not formatted properly.")]
        //public string TimeSeriesDataServiceUrl
        //{
        //    get 
        //    { 
        //        return m_timeSeriesDataServiceUrl; 
        //    }
        //    set 
        //    { 
        //        m_timeSeriesDataServiceUrl = value; 
        //    }
        //}

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Remote Status Service URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "Remote status service URL is not formatted properly.")]
        public string RemoteStatusServiceUrl
        {
            get
            {
                return m_remoteStatusServiceUrl;
            }
            set
            {
                m_remoteStatusServiceUrl = value;
                OnPropertyChanged("RemoteStatusServiceUrl");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Real-time Statistic Service URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "Real-time statistics service URL is not formatted properly.")]
        public string RealTimeStatisticServiceUrl
        {
            get
            {
                return m_realTimeStatisticServiceUrl;
            }
            set
            {
                m_realTimeStatisticServiceUrl = value;
                OnPropertyChanged("RealTimeStatisticServiceUrl");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Company Name.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string CompanyName
        {
            get
            {
                return m_companyName;
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time the current <see cref="Node"/> was created on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn
        {
            get
            {
                return m_createdOn;
            }
            set
            {
                m_createdOn = value;
            }
        }

        /// <summary>
        /// Gets or sets who the current <see cref="Node"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get
            {
                return m_createdBy;
            }
            set
            {
                m_createdBy = value;
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time when the current <see cref="Node"/> was updated on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_updatedOn;
            }
            set
            {
                m_updatedOn = value;
            }
        }

        /// <summary>
        /// Gets or sets who the current <see cref="Node"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get
            {
                return m_updatedBy;
            }
            set
            {
                m_updatedBy = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads <see cref="Node"/> IDs as an <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Guid"/>.</returns>
        public static IList<Guid> LoadKeys(AdoDataConnection database, string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<Guid> nodeList = new List<Guid>();
                string sortClause = string.Empty;
                DataTable nodeTable;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = $"ORDER BY {sortMember} {sortDirection}";

                nodeTable = database.Connection.RetrieveData(database.AdapterType, $"Select ID From NodeDetail {sortClause}");


                foreach (DataRow row in nodeTable.Rows)
                {
                    nodeList.Add(database.Guid(row, "ID"));
                }

                return nodeList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="Node"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the nodes to be loaded from the database.</param>
        /// <returns>Collection of <see cref="Node"/>.</returns>
        public static ObservableCollection<Node> Load(AdoDataConnection database, IList<Guid> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                Node[] nodeList = null;
                DataTable nodeTable;
                Guid id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => "'" + key.ToString() + "'").Aggregate((str1, str2) => str1 + "," + str2);
                    query = "Select ID, Name, CompanyID, Longitude, Latitude, Description, ImagePath, Settings, MenuData, " + $"MenuType, Master, LoadOrder, Enabled, CompanyName From NodeDetail WHERE ID IN ({commaSeparatedKeys})";

                    nodeTable = database.Connection.RetrieveData(database.AdapterType, query);
                    nodeList = new Node[nodeTable.Rows.Count];

                    foreach (DataRow row in nodeTable.Rows)
                    {
                        id = database.Guid(row, "ID");

                        nodeList[keys.IndexOf(id)] = new Node()
                        {
                            ID = id,
                            Name = row.Field<string>("Name"),
                            CompanyID = row.ConvertNullableField<int>("CompanyID"),
                            Longitude = row.ConvertNullableField<decimal>("Longitude"),
                            Latitude = row.ConvertNullableField<decimal>("Latitude"),
                            Description = row.Field<string>("Description"),
                            ImagePath = row.Field<string>("ImagePath"),
                            Settings = row.Field<string>("Settings"),
                            MenuType = row.Field<string>("MenuType"),
                            MenuData = row.Field<string>("MenuData"),
                            Master = Convert.ToBoolean(row.Field<object>("Master")),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            m_companyName = row.Field<string>("CompanyName")
                        };
                    }
                }

                return new ObservableCollection<Node>(nodeList ?? new Node[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Node"/> defined in the database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns><see cref="Node"/> information.</returns>
        public static Node GetCurrentNode(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                DataTable nodeTable;
                string query;

                query = database.ParameterizedQueryString("Select ID, Name, CompanyID, Longitude, Latitude, Description, " +
                    "ImagePath, Settings, MenuData, MenuType, Master, LoadOrder, Enabled, CompanyName " +
                    "From NodeDetail WHERE ID = {0} ORDER BY LoadOrder", "id");

                nodeTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());

                if (nodeTable.Rows.Count == 0)
                    return null;

                DataRow row = nodeTable.Rows[0];

                Node node = new Node
                    {
                        ID = database.Guid(row, "ID"),
                        Name = row.Field<string>("Name"),
                        CompanyID = row.ConvertNullableField<int>("CompanyID"),
                        Longitude = row.ConvertNullableField<decimal>("Longitude"),
                        Latitude = row.ConvertNullableField<decimal>("Latitude"),
                        Description = row.Field<string>("Description"),
                        ImagePath = row.Field<string>("ImagePath"),
                        Settings = row.Field<string>("Settings"),
                        MenuType = row.Field<string>("MenuType"),
                        MenuData = row.Field<string>("MenuData"),
                        Master = Convert.ToBoolean(row.Field<object>("Master")),
                        LoadOrder = row.ConvertField<int>("LoadOrder"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_companyName = row.Field<string>("CompanyName")
                    };

                return node;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Node"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of nodes defined in the database.</returns>
        public static Dictionary<Guid, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> nodeList = new Dictionary<Guid, string>();

                if (isOptional)
                    nodeList.Add(Guid.Empty, "Select Node");

                string query = database.ParameterizedQueryString("SELECT ID, Name FROM Node WHERE Enabled = {0} ORDER BY LoadOrder", "enabled");
                DataTable nodeTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.Bool(true));

                foreach (DataRow row in nodeTable.Rows)
                {
                    nodeList[database.Guid(row, "ID")] = row.Field<string>("Name");
                }

                return nodeList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Node"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="node">Information about <see cref="Node"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Node node)
        {
            if (node == null)
                throw new ArgumentException(nameof(node));

            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (node.ID == Guid.Empty)
                {
                    query = database.ParameterizedQueryString("INSERT INTO Node (Name, CompanyID, Longitude, Latitude, Description, ImagePath, Settings, MenuType, MenuData, " +
                        "Master, LoadOrder, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, " +
                        "{13}, {14}, {15})", "name", "companyID", "longitude", "latitude", "description", "imagePath", "settings", "menuType", "menuData", "master",
                        "loadOrder", "enabled", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, node.Name, node.CompanyID.ToNotNull(), node.Longitude.ToNotNull(), node.Latitude.ToNotNull(),
                        node.Description.ToNotNull(), node.ImagePath.ToNotNull(), node.Settings.ToNotNull(), node.MenuType, node.MenuData, database.Bool(node.Master), node.LoadOrder,
                        database.Bool(node.Enabled), CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = $"SELECT Name FROM NodeDetail WHERE ID IN ('{node.ID}')";
                    DataTable nodeTable = database.Connection.RetrieveData(database.AdapterType, query);

                    query = "SELECT SignalIndex FROM Statistic WHERE Source = \'System\'";
                    DataTable systemTable = database.Connection.RetrieveData(database.AdapterType, query);

                    query = database.ParameterizedQueryString("UPDATE Node SET Name = {0}, CompanyID = {1}, Longitude = {2}, Latitude = {3}, " +
                        "Description = {4}, ImagePath = {5}, Settings = {6}, MenuType = {7}, MenuData = {8}, Master = {9}, LoadOrder = {10}, Enabled = {11}, " +
                        "UpdatedBy = {12}, UpdatedOn = {13} WHERE ID = {14}", "name", "companyID", "longitude", "latitude", "description", "imagePath",
                        "Settings", "MenuType", "MenuData", "master", "loadOrder", "enabled", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, node.Name, node.CompanyID.ToNotNull(), node.Longitude.ToNotNull(), node.Latitude.ToNotNull(),
                        node.Description.ToNotNull(), node.ImagePath.ToNotNull(), node.Settings.ToNotNull(), node.MenuType, node.MenuData, database.Bool(node.Master), node.LoadOrder,
                        database.Bool(node.Enabled), CommonFunctions.CurrentUser, database.UtcNow, database.Guid(node.ID));

                    if (nodeTable.Rows.Count > 0)
                    {
                        string newNodeName = node.Name
                            .RemoveCharacters(c => !char.IsLetterOrDigit(c))
                            .Replace(' ', '_')
                            .ToUpper();

                        string oldNodeName = nodeTable.Rows[0]["Name"].ToString()
                            .RemoveCharacters(c => !char.IsLetterOrDigit(c))
                            .Replace(' ', '_')
                            .ToUpper();

                        //SystemTable is read from the database. 
                        for (int i = 0; i < systemTable.Rows.Count; i++)
                        {
                            string signalIndex = systemTable.Rows[i]["SignalIndex"].ToString();
                            string pointTag = $"{newNodeName}!SYSTEM:ST{signalIndex}";
                            string newSignalReference = $"{newNodeName}!SYSTEM-ST{signalIndex}";
                            string oldSignalReference = $"{oldNodeName}!SYSTEM-ST{signalIndex}";

                            query = database.ParameterizedQueryString("UPDATE Measurement SET PointTag = {0}, SignalReference = {1} WHERE SignalReference = {2}", "name", "newSignalReference", "oldSignalReference");
                            database.Connection.ExecuteNonQuery(query, DefaultTimeout, pointTag, newSignalReference, oldSignalReference);
                        }
                    }
                }
                return "Node information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Node"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="nodeID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, Guid nodeID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                // Before we delete node, make sure to delete all the devices associated with this node.
                // All other node related items get deleted by cascade delete.
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Device WHERE NodeID = {0}", "nodeID"), DefaultTimeout, database.Guid(nodeID));

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Node WHERE ID = {0}", "nodeID"), DefaultTimeout, database.Guid(nodeID));



                return "Node deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        #endregion
    }
}
