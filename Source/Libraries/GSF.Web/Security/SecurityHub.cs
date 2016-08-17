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
using GSF.Web.Hubs;
using GSF.Web.Model;
using Microsoft.AspNet.SignalR.Hubs;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines a SignalR security hub for managing users, groups and SID management.
    /// </summary>
    [AuthorizeHubRole]
    public class SecurityHub : RecordOperationsHub<SecurityHub>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SecurityHub"/>.
        /// </summary>
        public SecurityHub() : 
            this(null, null, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SecurityHub"/> with the specified logging functions.
        /// </summary>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public SecurityHub(Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) :
            this(null, logStatusMessageFunction, logExceptionFunction)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SecurityHub"/> with the specified <see cref="DataContext"/> and logging functions.
        /// </summary>
        /// <param name="dataContext">Data context to use for this hub; set to <c>null</c> for default security provider context.</param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public SecurityHub(DataContext dataContext, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) : 
            this(dataContext, logStatusMessageFunction, logExceptionFunction, true)
        {
            // Capture initial defaults
            if ((object)logStatusMessageFunction != null && (object)s_logStatusMessageFunction == null)
                s_logStatusMessageFunction = logStatusMessageFunction;

            if ((object)logExceptionFunction != null && (object)s_logExceptionFunction == null)
                s_logExceptionFunction = logExceptionFunction;
        }

        private SecurityHub(DataContext dataContext, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction, bool overload) :
            base(dataContext ?? new DataContext("securityProvider"), logStatusMessageFunction ?? s_logStatusMessageFunction, logExceptionFunction ?? s_logExceptionFunction)
        {
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static Action<string, UpdateType> s_logStatusMessageFunction;
        private static Action<Exception> s_logExceptionFunction;

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
            using (AdoDataConnection connection = new AdoDataConnection("securityProvider"))
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
            return DataContext.Table<UserAccount>().LoadRecord(id);
        }

        /// <summary>
        /// Finds the specified user account record by SID or database account name.
        /// </summary>
        /// <param name="accountName">SID or database account name of requested user.</param>
        /// <returns>Specified user account record.</returns>
        public UserAccount QueryUserAccountByName(string accountName)
        {
            return DataContext.Table<UserAccount>().QueryRecords(restriction: new RecordRestriction("Name = {0}", accountName)).FirstOrDefault();
        }
        
        /// <summary>
        /// Searches user accounts by resolved names with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "IDLabel" instances - serialized as JSON [{ id : "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchUserAccounts(string searchText)
        {
            return SearchUserAccounts(searchText, -1);
        }

        /// <summary>
        /// Searches user accounts by resolved names limited to the specified number of records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="limit">Limit of number of record to return.</param>
        /// <returns>Search results as "IDLabel" instances - serialized as JSON [{ id : "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchUserAccounts(string searchText, int limit)
        {
            if (limit < 1)
                return DataContext
                    .Table<UserAccount>()
                    .QueryRecords()
                    .Select(record =>
                    {
                        record.Name = UserInfo.SIDToAccountName(record.Name ?? "");
                        return record;
                    })
                    .Where(record => record.Name?.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase) ?? false)
                    .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));

            return DataContext
                .Table<UserAccount>()
                .QueryRecords()
                .Select(record =>
                {
                    record.Name = UserInfo.SIDToAccountName(record.Name ?? "");
                    return record;
                })
                .Where(record => record.Name?.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase) ?? false)
                .Take(limit)
                .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));
        }

        /// <summary>
        /// Finds the specified security group record.
        /// </summary>
        /// <param name="id">ID of requested group.</param>
        /// <returns>Specified security group record.</returns>
        public SecurityGroup QuerySecurityGroup(Guid id)
        {
            return DataContext.Table<SecurityGroup>().LoadRecord(id);
        }

        /// <summary>
        /// Finds the specified security group record by SID or database account name.
        /// </summary>
        /// <param name="accountName">SID or database account name of requested group.</param>
        /// <returns>Specified security group record.</returns>
        public SecurityGroup QuerySecurityGroupByName(string accountName)
        {
            return DataContext.Table<SecurityGroup>().QueryRecords(restriction:
                new RecordRestriction("Name = {0}", accountName)).FirstOrDefault();
        }

        /// <summary>
        /// Searches security groups by resolved names.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "Labels" - serialized as JSON [{ label : "value" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<Label> SearchSecurityGroups(string searchText)
        {
            return DataContext
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
            return DataContext.Table<ApplicationRole>().QueryRecords("Name", new RecordRestriction("NodeID={0}", DefaultNodeID));
        }

        /// <summary>
        /// Determines if user is in role based on database ID values.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user is in role; otherwise, <c>false</c>.</returns>
        public bool UserIsInRole(Guid userID, Guid roleID)
        {
            return DataContext.Table<ApplicationRoleUserAccount>().QueryRecordCount(new RecordRestriction("UserAccountID={0} AND ApplicationRoleID={1}", userID, roleID)) > 0;
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

            return DataContext.Table<ApplicationRoleUserAccount>().AddNewRecord(new ApplicationRoleUserAccount
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

            return DataContext.Table<ApplicationRoleUserAccount>().DeleteRecord(new RecordRestriction("UserAccountID={0} AND ApplicationRoleID={1}", userID, roleID)) > 0;
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
            return DataContext.Table<ApplicationRoleSecurityGroup>().QueryRecordCount(new RecordRestriction("SecurityGroupID={0} AND ApplicationRoleID={1}", groupID, roleID)) > 0;
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

            return DataContext.Table<ApplicationRoleSecurityGroup>().AddNewRecord(new ApplicationRoleSecurityGroup
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

            return DataContext.Table<ApplicationRoleSecurityGroup>().DeleteRecord(new RecordRestriction("SecurityGroupID={0} AND ApplicationRoleID={1}", groupID, roleID)) > 0;
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
        /// <param name="filterText">Text to use for filtering.</param>
        /// <returns>Count of user accounts.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.QueryRecordCount)]
        public int QueryUserAccountCount(string filterText)
        {
            return DataContext.Table<UserAccount>().QueryRecordCount();
        }

        /// <summary>
        /// Queries page of user accounts.
        /// </summary>
        /// <param name="sortField">Current sort field.</param>
        /// <param name="ascending">Current sort direction.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="pageSize">Current page size.</param>
        /// <param name="filterText">Text to use for filtering.</param>
        /// <returns>Page of user accounts.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.QueryRecords)]
        public IEnumerable<UserAccount> QueryUserAccounts(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<UserAccount>().QueryRecords(sortField, ascending, page, pageSize);
        }

        /// <summary>
        /// Deletes user account.
        /// </summary>
        /// <param name="id">Unique ID of user account.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.DeleteRecord)]
        public void DeleteUserAccount(Guid id)
        {
            DataContext.Table<UserAccount>().DeleteRecord(id);
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

            record.DefaultNodeID = DefaultNodeID;
            record.CreatedBy = UserInfo.CurrentUserID;
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedBy = record.CreatedBy;
            record.UpdatedOn = record.CreatedOn;
            DataContext.Table<UserAccount>().AddNewRecord(record);
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

            record.DefaultNodeID = DefaultNodeID;
            record.UpdatedBy = UserInfo.CurrentUserID;
            record.UpdatedOn = DateTime.UtcNow;
            DataContext.Table<UserAccount>().UpdateRecord(record);
        }

        #endregion

        #region [ SecurityGroup Table Operations ]

        /// <summary>
        /// Queries count of security groups.
        /// </summary>
        /// <param name="filterText">Text to use for filtering.</param>
        /// <returns>Count of security groups.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.QueryRecordCount)]
        public int QuerySecurityGroupCount(string filterText)
        {
            return DataContext.Table<SecurityGroup>().QueryRecordCount();
        }

        /// <summary>
        /// Queries page of security groups.
        /// </summary>
        /// <param name="sortField">Current sort field.</param>
        /// <param name="ascending">Current sort direction.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="pageSize">Current page size.</param>
        /// <param name="filterText">Text to use for filtering.</param>
        /// <returns>Page of security groups.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.QueryRecords)]
        public IEnumerable<SecurityGroup> QuerySecurityGroups(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<SecurityGroup>().QueryRecords(sortField, ascending, page, pageSize);
        }

        /// <summary>
        /// Deletes security group.
        /// </summary>
        /// <param name="id">Unique ID of security group.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.DeleteRecord)]
        public void DeleteSecurityGroup(Guid id)
        {
            DataContext.Table<SecurityGroup>().DeleteRecord(id);
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
            DataContext.Table<SecurityGroup>().AddNewRecord(record);
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
            DataContext.Table<SecurityGroup>().UpdateRecord(record);
        }

        #endregion
    }
}
