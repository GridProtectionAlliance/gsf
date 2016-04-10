//******************************************************************************************************
//  SecurityHub.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/03/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF.Configuration;
using GSF.Data;
using GSF.Data.Model;
using GSF.Identity;
using GSF.Security;
using GSF.Security.Model;
using GSF.Web.Model;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines a SignalR security hub for managing users, groups and SID management.
    /// </summary>
    [AuthorizeHubRole]
    public class SecurityHub : Hub, IRecordOperationsHub
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SecurityHub"/>.
        /// </summary>
        public SecurityHub()
        {
            m_dataContext = new DataContext();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="IRecordOperationsHub.RecordOperationsCache"/> for SignalR hub.
        /// </summary>
        public RecordOperationsCache RecordOperationsCache => s_recordOperationsCache;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SecurityHub"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_dataContext?.Dispose();
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                    base.Dispose(disposing); // Call base class Dispose().
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly RecordOperationsCache s_recordOperationsCache;

        /// <summary>
        /// Gets current default Node ID for security.
        /// </summary>
        public static readonly Guid DefaultNodeID;

        // Static Constructor
        static SecurityHub()
        {
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];

            // Retrieve default NodeID
            DefaultNodeID = Guid.Parse(systemSettings["NodeID"].Value.ToNonNullString(Guid.NewGuid().ToString()));

            // Determine whether the node exists in the database and create it if it doesn't
            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                const string NodeCountFormat = "SELECT COUNT(*) FROM Node";
                const string NodeInsertFormat = "INSERT INTO Node(Name, Description, Enabled) VALUES('Default', 'Default node', 1)";
                const string NodeUpdateFormat = "UPDATE Node SET ID = {0}";

                int nodeCount = connection.ExecuteScalar<int?>(NodeCountFormat) ?? 0;

                if (nodeCount == 0)
                {
                    connection.ExecuteNonQuery(NodeInsertFormat);
                    connection.ExecuteNonQuery(NodeUpdateFormat, connection.Guid(DefaultNodeID));
                }
            }

            // Analyze and cache record operations of security hub
            s_recordOperationsCache = new RecordOperationsCache(typeof(SecurityHub));
        }

        #endregion

        // Client-side script functionality

        #region [ Security Functions ]

        /// <summary>
        /// Finds the specified user account record.
        /// </summary>
        /// <param name="id">ID of requested user.</param>
        /// <returns>Specified user account record.</returns>
        public UserAccount QueryUserAccount(Guid id)
        {
            return m_dataContext.Table<UserAccount>().LoadRecord(id);
        }

        /// <summary>
        /// Finds the specified user account record by SID or database account name.
        /// </summary>
        /// <param name="accountName">SID or database account name of requested user.</param>
        /// <returns>Specified user account record.</returns>
        public UserAccount QueryUserAccountByName(string accountName)
        {
            return m_dataContext.Table<UserAccount>().QueryRecords(restriction:
                new RecordRestriction("Name = {0}", accountName)).FirstOrDefault();
        }

        /// <summary>
        /// Searches user accounts by resolved names.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "IDLabel" instances - serialized as JSON [{ id : "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchUserAccounts(string searchText)
        {
            return m_dataContext
                .Table<UserAccount>()
                .QueryRecords()
                .Select(record =>
                {
                    record.Name = UserInfo.SIDToAccountName(record.Name ?? "");
                    return record;
                })
                .Where(record => record.Name?.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase) ?? false)
                .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));
        }

        /// <summary>
        /// Finds the specified security group record.
        /// </summary>
        /// <param name="id">ID of requested group.</param>
        /// <returns>Specified security group record.</returns>
        public SecurityGroup QuerySecurityGroup(Guid id)
        {
            return m_dataContext.Table<SecurityGroup>().LoadRecord(id);
        }

        /// <summary>
        /// Finds the specified security group record by SID or database account name.
        /// </summary>
        /// <param name="accountName">SID or database account name of requested group.</param>
        /// <returns>Specified security group record.</returns>
        public SecurityGroup QuerySecurityGroupByName(string accountName)
        {
            return m_dataContext.Table<SecurityGroup>().QueryRecords(restriction:
                new RecordRestriction("Name = {0}", accountName)).FirstOrDefault();
        }

        /// <summary>
        /// Searches security groups by resolved names.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "Labels" - serialized as JSON [{ label : "value" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<Label> SearchSecurityGroups(string searchText)
        {
            return m_dataContext
                .Table<SecurityGroup>()
                .QueryRecords()
                .Select(record => UserInfo.SIDToAccountName(record.Name ?? ""))
                .Where(name => name.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase))
                .Select(Label.Create);
        }

        /// <summary>
        /// Gets the current application role records.
        /// </summary>
        /// <returns>Current application role records.</returns>
        public IEnumerable<ApplicationRole> QueryApplicationRoles()
        {
            return m_dataContext.Table<ApplicationRole>().QueryRecords("Name", new RecordRestriction("NodeID={0}", DefaultNodeID));
        }

        /// <summary>
        /// Determines if user is in role based on database ID values.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user is in role; otherwise, <c>false</c>.</returns>
        public bool UserIsInRole(Guid userID, Guid roleID)
        {
            return m_dataContext.Table<ApplicationRoleUserAccount>().QueryRecordCount(new RecordRestriction("UserAccountID={0} AND ApplicationRoleID={1}", userID, roleID)) > 0;
        }

        /// <summary>
        /// Adds user to a role.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user was added; otherwise <c>false</c>.</returns>
        [AuthorizeHubRole("Administrator")]
        public bool AddUserToRole(Guid userID, Guid roleID)
        {
            // Nothing to do if user is already in role
            if (UserIsInRole(userID, roleID))
                return false;

            return m_dataContext.Table<ApplicationRoleUserAccount>().AddNewRecord(new ApplicationRoleUserAccount
            {
                ApplicationRoleID = roleID,
                UserAccountID = userID
            }) > 0;
        }

        /// <summary>
        /// Removes user from a role.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user was removed; otherwise <c>false</c>.</returns>
        [AuthorizeHubRole("Administrator")]
        public bool RemoveUserFromRole(Guid userID, Guid roleID)
        {
            // Nothing to do if user is not currently in role
            if (!UserIsInRole(userID, roleID))
                return false;

            return m_dataContext.Table<ApplicationRoleUserAccount>().DeleteRecord(new RecordRestriction("UserAccountID={0} AND ApplicationRoleID={1}", userID, roleID)) > 0;
        }

        /// <summary>
        /// Gets SID for a given user name.
        /// </summary>
        /// <param name="userName">User name to convert to SID.</param>
        /// <returns>SID for a given user name.</returns>
        public string UserNameToSID(string userName)
        {
            return UserInfo.UserNameToSID(userName);
        }

        /// <summary>
        /// Determines if SID is for a user.
        /// </summary>
        /// <param name="sid">Security identifier to test.</param>
        /// <returns><c>true</c>if <paramref name="sid"/> is for a user; otherwise, <c>false</c>.</returns>
        [HubMethodName("isUserSID")]
        public bool IsUserSID(string sid)
        {
            return UserInfo.IsUserSID(sid);
        }

        /// <summary>
        /// Determines if group is in role based on database ID values.
        /// </summary>
        /// <param name="groupID">Group ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if group is in role; otherwise, <c>false</c>.</returns>
        public bool GroupIsInRole(Guid groupID, Guid roleID)
        {
            return m_dataContext.Table<ApplicationRoleSecurityGroup>().QueryRecordCount(new RecordRestriction("SecurityGroupID={0} AND ApplicationRoleID={1}", groupID, roleID)) > 0;
        }

        /// <summary>
        /// Adds group to a role.
        /// </summary>
        /// <param name="groupID">Group ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if group was added; otherwise <c>false</c>.</returns>
        [AuthorizeHubRole("Administrator")]
        public bool AddGroupToRole(Guid groupID, Guid roleID)
        {
            // Nothing to do if group is already in role
            if (GroupIsInRole(groupID, roleID))
                return false;

            return m_dataContext.Table<ApplicationRoleSecurityGroup>().AddNewRecord(new ApplicationRoleSecurityGroup
            {
                ApplicationRoleID = roleID,
                SecurityGroupID = groupID
            }) > 0;
        }

        /// <summary>
        /// Removes group from a role.
        /// </summary>
        /// <param name="groupID">Group ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if group was removed; otherwise <c>false</c>.</returns>
        [AuthorizeHubRole("Administrator")]
        public bool RemoveGroupFromRole(Guid groupID, Guid roleID)
        {
            // Nothing to do if group is not currently in role
            if (!GroupIsInRole(groupID, roleID))
                return false;

            return m_dataContext.Table<ApplicationRoleSecurityGroup>().DeleteRecord(new RecordRestriction("SecurityGroupID={0} AND ApplicationRoleID={1}", groupID, roleID)) > 0;
        }

        /// <summary>
        /// Gets SID for a given group name.
        /// </summary>
        /// <param name="groupName">Group name to convert to SID.</param>
        /// <returns>SID for a given group name.</returns>
        public string GroupNameToSID(string groupName)
        {
            return UserInfo.GroupNameToSID(groupName ?? "");
        }

        /// <summary>
        /// Determines if SID is for a group.
        /// </summary>
        /// <param name="sid">Security identifier to test.</param>
        /// <returns><c>true</c>if <paramref name="sid"/> is for a group; otherwise, <c>false</c>.</returns>
        [HubMethodName("isGroupSID")]
        public bool IsGroupSID(string sid)
        {
            return UserInfo.IsGroupSID(sid);
        }

        /// <summary>
        /// Gets account name for a given SID.
        /// </summary>
        /// <param name="sid">SID to convert to a account name.</param>
        /// <returns>Account name for a given SID.</returns>
        [HubMethodName("sidToAccountName")]
        public string SIDToAccountName(string sid)
        {
            return UserInfo.SIDToAccountName(sid ?? "");
        }

        #endregion

        #region [ UserAccount Table Operations ]

        /// <summary>
        /// Queries count of user accounts.
        /// </summary>
        /// <returns>Count of user accounts.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.QueryRecordCount)]
        public int QueryUserAccountCount()
        {
            return m_dataContext.Table<UserAccount>().QueryRecordCount();
        }

        /// <summary>
        /// Queries page of user accounts.
        /// </summary>
        /// <param name="sortField">Current sort field.</param>
        /// <param name="ascending">Current sort direction.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="pageSize">Current page size.</param>
        /// <returns>Page of user accounts.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.QueryRecords)]
        public IEnumerable<UserAccount> QueryUserAccounts(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<UserAccount>().QueryRecords(sortField, ascending, page, pageSize);
        }

        /// <summary>
        /// Deletes user account.
        /// </summary>
        /// <param name="id">Unique ID of user account.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.DeleteRecord)]
        public void DeleteUserAccount(Guid id)
        {
            m_dataContext.Table<UserAccount>().DeleteRecord(id);
        }

        /// <summary>
        /// Creates a new user account model instance.
        /// </summary>
        /// <returns>A new user account model instance.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.CreateNewRecord)]
        public UserAccount NewUserAccount()
        {
            return new UserAccount();
        }

        /// <summary>
        /// Adds a new user account.
        /// </summary>
        /// <param name="record">User account model instance to add.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.AddNewRecord)]
        public void AddNewUserAccount(UserAccount record)
        {
            if (!record.UseADAuthentication && !string.IsNullOrWhiteSpace(record.Password))
                record.Password = SecurityProviderUtility.EncryptPassword(record.Password);

            record.DefaultNodeID = SecurityHub.DefaultNodeID;
            record.CreatedBy = UserInfo.CurrentUserID;
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedBy = record.CreatedBy;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<UserAccount>().AddNewRecord(record);
        }

        /// <summary>
        /// Updates user account.
        /// </summary>
        /// <param name="record">User account model instance to update.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.UpdateRecord)]
        public void UpdateUserAccount(UserAccount record)
        {
            if (!record.UseADAuthentication && !string.IsNullOrWhiteSpace(record.Password))
                record.Password = SecurityProviderUtility.EncryptPassword(record.Password);

            record.DefaultNodeID = SecurityHub.DefaultNodeID;
            record.UpdatedBy = UserInfo.CurrentUserID;
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<UserAccount>().UpdateRecord(record);
        }

        #endregion

        #region [ SecurityGroup Table Operations ]

        /// <summary>
        /// Queries count of security groups.
        /// </summary>
        /// <returns>Count of security groups.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.QueryRecordCount)]
        public int QuerySecurityGroupCount()
        {
            return m_dataContext.Table<SecurityGroup>().QueryRecordCount();
        }

        /// <summary>
        /// Queries page of security groups.
        /// </summary>
        /// <param name="sortField">Current sort field.</param>
        /// <param name="ascending">Current sort direction.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="pageSize">Current page size.</param>
        /// <returns>Page of security groups.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.QueryRecords)]
        public IEnumerable<SecurityGroup> QuerySecurityGroups(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<SecurityGroup>().QueryRecords(sortField, ascending, page, pageSize);
        }

        /// <summary>
        /// Deletes security group.
        /// </summary>
        /// <param name="id">Unique ID of security group.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.DeleteRecord)]
        public void DeleteSecurityGroup(Guid id)
        {
            m_dataContext.Table<SecurityGroup>().DeleteRecord(id);
        }

        /// <summary>
        /// Creates a new security group model instance.
        /// </summary>
        /// <returns>A new security group model instance.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.CreateNewRecord)]
        public SecurityGroup NewSecurityGroup()
        {
            return new SecurityGroup();
        }

        /// <summary>
        /// Adds a new security group.
        /// </summary>
        /// <param name="record">Security group model instance to add.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.AddNewRecord)]
        public void AddNewSecurityGroup(SecurityGroup record)
        {
            record.CreatedBy = UserInfo.CurrentUserID;
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedBy = record.CreatedBy;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<SecurityGroup>().AddNewRecord(record);
        }

        /// <summary>
        /// Updates security group.
        /// </summary>
        /// <param name="record">Security group model instance to update.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.UpdateRecord)]
        public void UpdateSecurityGroup(SecurityGroup record)
        {
            record.UpdatedBy = UserInfo.CurrentUserID;
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<SecurityGroup>().UpdateRecord(record);
        }

        #endregion
    }
}
