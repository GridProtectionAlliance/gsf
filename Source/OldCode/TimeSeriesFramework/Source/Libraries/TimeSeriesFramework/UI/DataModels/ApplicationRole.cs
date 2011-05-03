//******************************************************************************************************
//  ErrorLog.cs - Gbtc
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
//  04/13/2011 - Aniket Salver
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    ///  Represents a record of <see cref="ApplicationRole"/> information as defined in the database.
    /// </summary>
    public class ApplicationRole : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_id;
        private Guid m_nodeID;
        private string m_name;
        private string m_description;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        private ObservableCollection<ApplicationRole> m_currentRoleGroups;
        private ObservableCollection<ApplicationRole> m_possibleRoleGroups;
        private ObservableCollection<ApplicationRole> m_currentRoleUsers;
        private ObservableCollection<ApplicationRole> m_possibleRoleUsers;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> ID.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="ApplicationRole"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = "Application role node ID is a required field, please select a value.")]
        public Guid NodeID
        {
            get
            {
                return m_nodeID;
            }
            set
            {
                m_nodeID = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> Name.
        /// </summary>
        [Required(ErrorMessage = " Application role name is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "Application role name cannot exceed 50 characters.")]
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
        /// Gets or sets <see cref="ApplicationRole"/> Description.
        /// </summary>
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
        /// Gets or sets <see cref="ApplicationRole"/> CreatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="ApplicationRole"/> CreatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="ApplicationRole"/> UpdatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="ApplicationRole"/> UpdatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
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

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> CurrentRoleGroups.
        /// </summary>
        public ObservableCollection<ApplicationRole> CurrentRoleGroups
        {
            get
            {
                return m_currentRoleGroups;
            }
            set
            {
                m_currentRoleGroups = value;
                OnPropertyChanged("CurrentRoleGroups");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> PossibleRoleGroups.
        /// </summary>
        public ObservableCollection<ApplicationRole> PossibleRoleGroups
        {
            get
            {
                return m_possibleRoleGroups;
            }
            set
            {
                m_possibleRoleGroups = value;
                OnPropertyChanged("PossibleRoleGroups");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> CurrentRoleUsers.
        /// </summary>
        public ObservableCollection<ApplicationRole> CurrentRoleUsers
        {
            get
            {
                return m_currentRoleUsers;
            }
            set
            {
                m_currentRoleUsers = value;
                OnPropertyChanged("CurrentRoleUsers");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> PossibleRoleUsers.
        /// </summary>
        public ObservableCollection<ApplicationRole> PossibleRoleUsers
        {
            get
            {
                return m_possibleRoleUsers;
            }
            set
            {
                m_possibleRoleUsers = value;
                OnPropertyChanged("PossibleRoleUsers");
            }
        }

        #endregion

        #region [Static]

        // Static Methods

        /// <summary>
        /// Loads <see cref="ApplicationRole"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="ApplicationRole"/>.</returns>
        public static ObservableCollection<ApplicationRole> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<ApplicationRole> applicationRoleList = new ObservableCollection<ApplicationRole>();

                DataTable applicationRoleTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM ApplicationRole WHERE NodeID = @nodeID ORDER BY Name");

                foreach (DataRow row in applicationRoleTable.Rows)
                {
                    applicationRoleList.Add(new ApplicationRole()
                    {
                        ID = row.Field<Guid>("ID"),
                        Name = row.Field<string>("Name"),
                        Description = row.Field<string>("Description"),
                        NodeID = row.Field<Guid>("NodeID"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy")
                    });
                }

                return applicationRoleList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="ApplicationRole"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of application roles defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> applicationRoleList = new Dictionary<int, string>();

                if (isOptional)
                    applicationRoleList.Add(0, "Select Application Role");

                DataTable applicationRoleTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM ApplicationRole ORDER BY Name");

                foreach (DataRow row in applicationRoleTable.Rows)
                    applicationRoleList[row.Field<int>("ID")] = row.Field<string>("Name");

                return applicationRoleList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="ApplicationRole"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="applicationRole">Information about <see cref="ApplicationRole"/>.</param>
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, ApplicationRole applicationRole, bool isNew)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (isNew)
                    database.Connection.ExecuteNonQuery("INSERT INTO ApplicationRole (Name, Description, NodeID, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@name, @description, @nodeID, @updatedBy, @updatedOn, @createdBy, @createdOn)",
                        DefaultTimeout, applicationRole.Name, applicationRole.Description, applicationRole.NodeID, applicationRole.UpdatedBy, applicationRole.UpdatedOn, applicationRole.CreatedBy, applicationRole.CreatedOn);
                else
                    database.Connection.ExecuteNonQuery("UPDATE ApplicationRole SET Name = @name, Description = @description, NodeID = @nodeID, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout,
                        applicationRole.Name, applicationRole.Description, applicationRole.NodeID, applicationRole.UpdatedBy, applicationRole.UpdatedOn, applicationRole.ID);

                return "Application role information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="ApplicationRole"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="applicationRoleID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int applicationRoleID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM ApplicationRole WHERE ID = @applicationRoleID", DefaultTimeout, applicationRoleID);

                return "Application role deleted successfully";
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
