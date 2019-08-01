//******************************************************************************************************
//  ApplicationRole.cs - Gbtc
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
//  04/13/2011 - Aniket Salver
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL value and Guid parameter handling for Save() operation.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using GSF.Data;
using GSF.Identity;

namespace GSF.TimeSeries.UI.DataModels
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
        private Dictionary<Guid, string> m_currentGroups;
        private Dictionary<Guid, string> m_possibleGroups;
        private Dictionary<Guid, string> m_currentUsers;
        private Dictionary<Guid, string> m_possibleUsers;

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
        [StringLength(200, ErrorMessage = "Application role name cannot exceed 200 characters.")]
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
        // Because of database design, no validation attributes are applied.
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
        public Dictionary<Guid, string> CurrentGroups
        {
            get
            {
                return m_currentGroups;
            }
            set
            {
                m_currentGroups = value;
                OnPropertyChanged("CurrentGroups");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> PossibleRoleGroups.
        /// </summary>
        public Dictionary<Guid, string> PossibleGroups
        {
            get
            {
                return m_possibleGroups;
            }
            set
            {
                m_possibleGroups = value;
                OnPropertyChanged("PossibleGroups");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> CurrentRoleUsers.
        /// </summary>
        public Dictionary<Guid, string> CurrentUsers
        {
            get
            {
                return m_currentUsers;
            }
            set
            {
                m_currentUsers = value;
                OnPropertyChanged("CurrentUsers");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> PossibleRoleUsers.
        /// </summary>
        public Dictionary<Guid, string> PossibleUsers
        {
            get
            {
                return m_possibleUsers;
            }
            set
            {
                m_possibleUsers = value;
                OnPropertyChanged("PossibleUsers");
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
                string query = database.ParameterizedQueryString("SELECT * FROM ApplicationRole WHERE NodeID = {0} ORDER BY Name", "nodeID");
                DataTable applicationRoleTable = database.Connection.RetrieveData(database.AdapterType, query, database.CurrentNodeID());

                foreach (DataRow row in applicationRoleTable.Rows)
                {
                    applicationRoleList.Add(new ApplicationRole()
                    {
                        ID = database.Guid(row, "ID"),  //Guid.Parse(row.Field<object>("ID").ToString()),
                        Name = row.Field<string>("Name"),
                        Description = row.Field<string>("Description"),
                        NodeID = Guid.Parse(row.Field<object>("NodeID").ToString()),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy"),
                        CurrentUsers = GetCurrentUsers(database, Guid.Parse(row.Field<object>("ID").ToString())),
                        PossibleUsers = GetPossibleUsers(database, Guid.Parse(row.Field<object>("ID").ToString())),
                        CurrentGroups = GetCurrentGroups(database, Guid.Parse(row.Field<object>("ID").ToString())),
                        PossibleGroups = GetPossibleGroups(database, Guid.Parse(row.Field<object>("ID").ToString()))
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
        /// Retrieves collection of <see cref="UserAccount"/>s assigned to <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of the <see cref="ApplicationRole"/> to search for.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection of <see cref="UserAccount"/>.</returns>
        public static Dictionary<Guid, string> GetCurrentUsers(AdoDataConnection database, Guid roleID)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> currentUsers = new Dictionary<Guid, string>();
                string query = database.ParameterizedQueryString("SELECT * FROM AppRoleUserAccountDetail WHERE ApplicationRoleID = {0} ORDER BY UserName", "applicationRoleID");
                DataTable currentUsersTable = database.Connection.RetrieveData(database.AdapterType, query, database.Guid(roleID));

                foreach (DataRow row in currentUsersTable.Rows)
                    currentUsers[database.Guid(row, "UserAccountID")] = UserInfo.SIDToAccountName(row.Field<string>("UserName"));

                return currentUsers;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves collection of <see cref="UserAccount"/>s NOT assigned to <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of the <see cref="ApplicationRole"/> to search for.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection of <see cref="UserAccount"/>.</returns>
        public static Dictionary<Guid, string> GetPossibleUsers(AdoDataConnection database, Guid roleID)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> possibleUsers = new Dictionary<Guid, string>();
                string query = database.ParameterizedQueryString("SELECT * FROM UserAccount WHERE ID NOT IN (SELECT UserAccountID FROM ApplicationRoleUserAccount WHERE ApplicationRoleID = {0}) ORDER BY Name", "applicationRoleID");
                DataTable possibleUsersTable = database.Connection.RetrieveData(database.AdapterType, query, database.Guid(roleID));

                foreach (DataRow row in possibleUsersTable.Rows)
                    possibleUsers[database.Guid(row, "ID")] = UserInfo.SIDToAccountName(row.Field<string>("Name"));

                return possibleUsers;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves collection of <see cref="SecurityGroup"/>s assigned to <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of the <see cref="ApplicationRole"/> to search for.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection of <see cref="SecurityGroup"/>.</returns>
        public static Dictionary<Guid, string> GetCurrentGroups(AdoDataConnection database, Guid roleID)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> currentGroups = new Dictionary<Guid, string>();
                string query = database.ParameterizedQueryString("SELECT * FROM AppRoleSecurityGroupDetail WHERE ApplicationRoleID = {0} ORDER BY SecurityGroupName", "applicationRoleID");
                DataTable currentGroupsTable = database.Connection.RetrieveData(database.AdapterType, query, database.Guid(roleID));

                foreach (DataRow row in currentGroupsTable.Rows)
                    currentGroups[database.Guid(row, "SecurityGroupID")] = UserInfo.SIDToAccountName(row.Field<string>("SecurityGroupName"));

                return currentGroups;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves collection of <see cref="SecurityGroup"/>s NOT assigned to <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of the <see cref="ApplicationRole"/> to search for.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection of <see cref="SecurityGroup"/>.</returns>
        public static Dictionary<Guid, string> GetPossibleGroups(AdoDataConnection database, Guid roleID)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> possibleGroups = new Dictionary<Guid, string>();
                string query = database.ParameterizedQueryString("SELECT * FROM SecurityGroup WHERE ID NOT IN (SELECT SecurityGroupID FROM ApplicationRoleSecurityGroup WHERE ApplicationRoleID = {0}) ORDER BY Name", "applicationRoleID");
                DataTable possibleGroupsTable = database.Connection.RetrieveData(database.AdapterType, query, database.Guid(roleID));

                foreach (DataRow row in possibleGroupsTable.Rows)
                    possibleGroups[database.Guid(row, "ID")] = UserInfo.SIDToAccountName(row.Field<string>("Name"));

                return possibleGroups;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Adds <see cref="UserAccount"/> to <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of <see cref="ApplicationRole"/> to which <see cref="UserAccount"/>s are being added.</param>
        /// <param name="usersToBeAdded">List of <see cref="UserAccount"/> IDs to be added.</param>
        /// <returns>string, for display use, indicating success.</returns>
        public static string AddUsers(AdoDataConnection database, Guid roleID, List<Guid> usersToBeAdded)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                foreach (Guid id in usersToBeAdded)
                {
                    string userName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM UserAccount WHERE ID = {0}", "userAccountID"), DefaultTimeout, database.Guid(id)).ToNonNullString();
                    string roleName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM ApplicationRole WHERE ID = {0}", "applicationRoleID"), DefaultTimeout, database.Guid(roleID)).ToNonNullString();
                    string query = database.ParameterizedQueryString("INSERT INTO ApplicationRoleUserAccount (ApplicationRoleID, UserAccountID) VALUES ({0}, {1})", "roleID", "userID");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.Guid(roleID), database.Guid(id));
                    CommonFunctions.LogEvent($"User \"{UserInfo.SIDToAccountName(userName)}\" added to role \"{roleName}\" by user \"{CommonFunctions.CurrentUser}\".", 4);
                }

                return "User accounts added to role successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes <see cref="UserAccount"/> from <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of <see cref="ApplicationRole"/> from which <see cref="UserAccount"/>s are being deleted.</param>
        /// <param name="usersToBeDeleted">List of <see cref="UserAccount"/> IDs to be deleted.</param>
        /// <returns>string, for display use, indicating success.</returns>
        public static string RemoveUsers(AdoDataConnection database, Guid roleID, List<Guid> usersToBeDeleted)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);
                foreach (Guid id in usersToBeDeleted)
                {
                    string userName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM UserAccount WHERE ID = {0}", "userAccountID"), DefaultTimeout, database.Guid(id)).ToNonNullString();
                    string roleName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM ApplicationRole WHERE ID = {0}", "applicationRoleID"), DefaultTimeout, database.Guid(roleID)).ToNonNullString();
                    string query = database.ParameterizedQueryString("DELETE FROM ApplicationRoleUserAccount WHERE ApplicationRoleID = {0} AND UserAccountID = {1}", "roleID", "userID");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.Guid(roleID), database.Guid(id));
                    CommonFunctions.LogEvent($"User \"{UserInfo.SIDToAccountName(userName)}\" removed from role \"{roleName}\" by user \"{CommonFunctions.CurrentUser}\".", 5);
                }

                return "User accounts deleted from role successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Adds <see cref="SecurityGroup"/> to <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of <see cref="ApplicationRole"/> to which <see cref="SecurityGroup"/>s are being added.</param>
        /// <param name="groupsToBeAdded">List of <see cref="SecurityGroup"/> IDs to be added.</param>
        /// <returns>string, for display use, indicating success.</returns>
        public static string AddGroups(AdoDataConnection database, Guid roleID, List<Guid> groupsToBeAdded)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);
                foreach (Guid id in groupsToBeAdded)
                {
                    string groupName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM SecurityGroup WHERE ID = {0}", "securityGroupID"), DefaultTimeout, database.Guid(id)).ToNonNullString();
                    string roleName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM ApplicationRole WHERE ID = {0}", "applicationRoleID"), DefaultTimeout, database.Guid(roleID)).ToNonNullString();
                    string query = database.ParameterizedQueryString("INSERT INTO ApplicationRoleSecurityGroup (ApplicationRoleID, SecurityGroupID) Values ({0}, {1})", "roleID", "groupID");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.Guid(roleID), database.Guid(id));
                    CommonFunctions.LogEvent($"Group \"{UserInfo.SIDToAccountName(groupName)}\" added to role \"{roleName}\" by user \"{CommonFunctions.CurrentUser}\".", 10);
                }

                return "Security groups added to role successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes <see cref="SecurityGroup"/> from <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="roleID">ID of <see cref="ApplicationRole"/> from which <see cref="SecurityGroup"/>s are being deleted.</param>
        /// <param name="groupsToBeDeleted">List of <see cref="SecurityGroup"/> IDs to be deleted.</param>
        /// <returns>string, for display use, indicating success.</returns>
        public static string RemoveGroups(AdoDataConnection database, Guid roleID, List<Guid> groupsToBeDeleted)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);
                foreach (Guid id in groupsToBeDeleted)
                {
                    string groupName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM SecurityGroup WHERE ID = {0}", "securityGroupID"), DefaultTimeout, database.Guid(id)).ToNonNullString();
                    string roleName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM ApplicationRole WHERE ID = {0}", "applicationRoleID"), DefaultTimeout, database.Guid(roleID)).ToNonNullString();
                    string query = database.ParameterizedQueryString("DELETE FROM ApplicationRoleSecurityGroup WHERE ApplicationRoleID = {0} AND SecurityGroupID = {1}", "roleID", "groupID");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.Guid(roleID), database.Guid(id));
                    CommonFunctions.LogEvent($"Group \"{UserInfo.SIDToAccountName(groupName)}\" removed from role \"{roleName}\" by user \"{CommonFunctions.CurrentUser}\".", 11);
                }

                return "Security groups deleted from role successfully";
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
                    applicationRoleList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

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
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, ApplicationRole applicationRole)
        {
            if (applicationRole == null)
                throw new ArgumentNullException(nameof(applicationRole));

            bool createdConnection = false;
            string query;

            try
            {

                createdConnection = CreateConnection(ref database);

                if (applicationRole.ID == Guid.Empty)
                {
                    query = database.ParameterizedQueryString("INSERT INTO ApplicationRole (Name, Description, NodeID, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values ({0}, {1}, {2}, {3}, {4}, {5}, {6})", "name", "description", "nodeID", "updatedBy", "updatedBy", "createdBy", "createdOn");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, applicationRole.Name, applicationRole.Description.ToNotNull(), database.CurrentNodeID(), CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE ApplicationRole SET Name = {0}, Description = {1}, NodeID = {2}, UpdatedBy = {3}, UpdatedOn = {4} WHERE ID = {5}", "name", "description", "nodeID", "updatedBy", "updatedOn", "id");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, applicationRole.Name, applicationRole.Description.ToNotNull(), applicationRole.NodeID, CommonFunctions.CurrentUser, database.UtcNow, database.Guid(applicationRole.ID));
                }

                return "Application role information saved successfully";
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="ApplicationRole"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="applicationRoleID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, Guid applicationRoleID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM ApplicationRole WHERE ID = {0}", "applicationRoleID"), DefaultTimeout, database.Guid(applicationRoleID));

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
