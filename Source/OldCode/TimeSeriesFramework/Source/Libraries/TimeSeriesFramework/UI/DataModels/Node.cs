//******************************************************************************************************
//  Node.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Creates a new object that represents a Node
    /// </summary>
    public class Node : DataModelBase
    {        
        #region [ Members ]

        // Fields
        private string m_ID;
        private string m_name;
        private int? m_companyID;
        private decimal? m_longitude;
        private decimal? m_latitude;
        private string m_description;
        private string m_image;
        private bool m_master;
        private int m_loadOrder;
        private bool m_enabled;
        //private string m_timeSeriesDataServiceUrl;
        private string m_remoteStatusServiceUrl;
        private string m_realTimeStatisticServiceUrl;
        private string m_companyName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s ID.
        /// </summary>
        [StringLength(36, ErrorMessage="Node ID cannot exceed 36 characters")]
        public string ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Name.
        /// </summary>
        [Required(ErrorMessage="Node name is a required field, please provide a value")]
        [StringLength(100, ErrorMessage="Name cannot exceed 100 characters.")]
        public string Name
        {
            get 
            { 
                return m_name; 
            }
            set 
            { 
                m_name = value; 
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
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Longitude.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public decimal? Longitude
        {
            get 
            { 
                return m_longitude; 
            }
            set 
            { 
                m_longitude = value; 
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Latitude.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public decimal? Latitude
        {
            get 
            { 
                return m_latitude; 
            }
            set 
            { 
                m_latitude = value; 
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
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Image.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string Image
        {
            get 
            { 
                return m_image; 
            }
            set 
            {
                m_image = value; 
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Node"/> is the master <see cref="Node"/>.
        /// </summary>
        [DefaultValue(typeof(bool), "false")]
        public bool Master
        {
            get 
            { 
                return m_master; 
            }
            set 
            {
                m_master = value; 
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Load Order.
        /// </summary>
        [DefaultValue(typeof(int), "0")]
        public int LoadOrder
        {
            get 
            { 
                return m_loadOrder; 
            }
            set 
            { 
                m_loadOrder = value; 
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Node"/> is enabled.
        /// </summary>
        [DefaultValue(typeof(bool), "false")]
        public bool Enabled
        {
            get 
            { 
                return m_enabled; 
            }
            set 
            { 
                m_enabled = value; 
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
        [DataType(DataType.Url, ErrorMessage = "Remote Status Service URL is not formatted properly.")]
        public string RemoteStatusServiceUrl
        {
            get 
            { 
                return m_remoteStatusServiceUrl; 
            }
            set 
            { 
                m_remoteStatusServiceUrl = value; 
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Node"/>'s Real Time Statistic Service URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "Real Time Statistics Service URL is not formatted properly.")]
        public string RealTimeStatisticServiceUrl
        {
            get 
            { 
                return m_realTimeStatisticServiceUrl; 
            }
            set 
            { 
                m_realTimeStatisticServiceUrl = value; 
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
            set 
            {
                m_companyName = value; 
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
        /// Loads <see cref="Node"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>        
        /// <returns>Collection of <see cref="Node"/>.</returns>
        public static ObservableCollection<Node> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Node> nodeList = new ObservableCollection<Node>();
                DataTable nodeTable;

                nodeTable = database.Connection.RetrieveData(database.AdapterType, "Select ID, Name, CompanyID, " +
                        "Longitude, Latitude, Description, ImagePath, Master, LoadOrder, Enabled, RemoteStatusServiceUrl, " +
                        "RealTimeStatisticServiceUrl, CompanyName From NodeDetail Order By LoadOrder");

                foreach (DataRow row in nodeTable.Rows)
                {
                    nodeList.Add(new Node()
                        {
                            ID = row.Field<object>("ID").ToString(),
                            Name = row.Field<string>("Name"),
                            CompanyID = row.Field<int?>("CompanyID"),
                            Longitude = row.Field<decimal?>("Longitude"),
                            Latitude = row.Field<decimal?>("Latitude"),
                            Description = row.Field<string>("Description"),
                            Image = row.Field<string>("ImagePath"),
                            Master = Convert.ToBoolean(row.Field<object>("Master")),
                            LoadOrder = row.Field<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            //TimeSeriesDataServiceUrl = row.Field<string>("TimeSeriesDataServiceUrl"),
                            RemoteStatusServiceUrl = row.Field<string>("RemoteStatusServiceUrl"),
                            RealTimeStatisticServiceUrl = row.Field<string>("RealTimeStatisticServiceUrl"),
                            CompanyName = row.Field<string>("CompanyName")
                        });
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
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Node"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns>Dictionary<int, string> containing ID and Name of nodes defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> nodeList = new Dictionary<int, string>();
                if (isOptional)
                    nodeList.Add(0, "Select Node");

                DataTable nodeTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM Node Where Enabled = @enabled ORDER BY LoadOrder",
                    DefaultTimeout, true);

                foreach (DataRow row in nodeTable.Rows)
                    nodeList[row.Field<int>("ID")] = row.Field<string>("Name");

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
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Node node, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                if (isNew)
                    database.Connection.ExecuteNonQuery("Insert Into Node (Name, CompanyID, Longitude, Latitude, Description, ImagePath, Master, LoadOrder, " +
                        "Enabled, RemoteStatusServiceUrl, RealTimeStatisticServiceUrl, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@name, @companyID, " +
                        "@longitude, @latitude, @description, @image, @master, @loadOrder, @enabled, @remoteStatusServiceUrl, @realTimeStatisticServiceUrl, " +
                        "@updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, node.Name, node.CompanyID ?? (object)DBNull.Value, node.Longitude ?? (object)DBNull.Value,
                        node.Latitude ?? (object)DBNull.Value, node.Description, node.Image, node.Master, node.LoadOrder, node.Enabled, node.RemoteStatusServiceUrl,
                        node.m_realTimeStatisticServiceUrl, CommonFunctions.CurrentUser, database.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? DateTime.UtcNow.Date : DateTime.UtcNow,
                        CommonFunctions.CurrentUser, database.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? DateTime.UtcNow.Date : DateTime.UtcNow);
                else
                    database.Connection.ExecuteNonQuery("Update Node Set Name = @name, CompanyID = @companyID, Longitude = @longitude, Latitude = @latitude, " +
                        "Description = @description, ImagePath = @image, Master = @master, LoadOrder = @loadOrder, Enabled = @enabled, " +
                        "RemoteStatusServiceUrl = @remoteStatusServiceUrl, RealTimeStatisticServiceUrl = @realTimeStatisticServiceUrl, " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn Where ID = @id", DefaultTimeout, node.Name, node.CompanyID ?? (object)DBNull.Value, node.Longitude ?? (object)DBNull.Value,
                        node.Latitude ?? (object)DBNull.Value, node.Description, node.Image, node.Master, node.LoadOrder, node.Enabled, node.RemoteStatusServiceUrl,
                        node.m_realTimeStatisticServiceUrl, CommonFunctions.CurrentUser, database.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? DateTime.UtcNow.Date : DateTime.UtcNow,
                        database.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? "{" + node.ID + "}" : node.ID);

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
        public static string Delete(AdoDataConnection database, string nodeID)
        {
            bool createdConnection = false;

            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM Node WHERE ID = @nodeID", DefaultTimeout, database.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? "{" + nodeID + "}" : nodeID);

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
