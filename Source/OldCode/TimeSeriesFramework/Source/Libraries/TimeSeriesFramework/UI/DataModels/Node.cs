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
//  04/18/2011 - Mehulbhai P Thakkar
//       Added static methods for database operations.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
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
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
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
        public string Image
        {
            get
            {
                return m_image;
            }
            set
            {
                m_image = value;
                OnPropertyChanged("Image");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Node"/> is the master <see cref="Node"/>.
        /// </summary>
        [DefaultValue(false)]
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
        [DefaultValue(false)]
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
        /// Gets or sets the current <see cref="Node"/>'s Real Time Statistic Service URL.
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
                        "RealTimeStatisticServiceUrl, CompanyName From NodeDetail ORDER BY LoadOrder");

                foreach (DataRow row in nodeTable.Rows)
                {
                    nodeList.Add(new Node()
                        {
                            ID = row.Field<Guid>("ID"),
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
                            m_companyName = row.Field<string>("CompanyName")
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

                DataTable nodeTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM Node WHERE Enabled = @enabled ORDER BY LoadOrder", DefaultTimeout, true);

                foreach (DataRow row in nodeTable.Rows)
                {
                    nodeList[Guid.Parse(row.Field<object>("ID").ToString())] = row.Field<string>("Name");
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
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (node.ID == Guid.Empty)
                    database.Connection.ExecuteNonQuery("INSERT INTO Node (Name, CompanyID, Longitude, Latitude, Description, ImagePath, Master, LoadOrder, " +
                        "Enabled, RemoteStatusServiceUrl, RealTimeStatisticServiceUrl, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES (@name, @companyID, " +
                        "@longitude, @latitude, @description, @image, @master, @loadOrder, @enabled, @remoteStatusServiceUrl, @realTimeStatisticServiceUrl, " +
                        "@updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, node.Name, node.CompanyID.ToNotNull(), node.Longitude.ToNotNull(),
                        node.Latitude.ToNotNull(), node.Description.ToNotNull(), node.Image.ToNotNull(), node.Master, node.LoadOrder, node.Enabled,
                        node.RemoteStatusServiceUrl.ToNotNull(), node.m_realTimeStatisticServiceUrl.ToNotNull(), CommonFunctions.CurrentUser,
                        database.UtcNow(), CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE Node SET Name = @name, CompanyID = @companyID, Longitude = @longitude, Latitude = @latitude, " +
                        "Description = @description, ImagePath = @image, Master = @master, LoadOrder = @loadOrder, Enabled = @enabled, " +
                        "RemoteStatusServiceUrl = @remoteStatusServiceUrl, RealTimeStatisticServiceUrl = @realTimeStatisticServiceUrl, " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, node.Name, node.CompanyID.ToNotNull(), node.Longitude.ToNotNull(),
                        node.Latitude.ToNotNull(), node.Description.ToNotNull(), node.Image.ToNotNull(), node.Master, node.LoadOrder, node.Enabled,
                        node.RemoteStatusServiceUrl.ToNotNull(), node.m_realTimeStatisticServiceUrl.ToNotNull(), CommonFunctions.CurrentUser,
                        database.UtcNow(), node.ID);

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

                database.Connection.ExecuteNonQuery("DELETE FROM Node WHERE ID = @nodeID", DefaultTimeout, database.Guid(nodeID));

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
