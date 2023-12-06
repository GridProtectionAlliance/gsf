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
using System.Threading.Tasks;
using System.Web;
using GSF.Data.Model;
using GSF.Identity;
using GSF.Security;
using GSF.Security.Model;
using GSF.Web.Hubs;
using GSF.Web.Model;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Graph;
using RecordOperation = GSF.Data.Model.RecordOperation;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines a SignalR security hub for managing users, groups and SID management.
    /// </summary>
    [AuthorizeHubRole]
    public class SecurityHub : RecordOperationsHub<SecurityHub>
    {
        #region [ Members ]

        private AzureADSettings m_azureADSettings;
        private GraphServiceClient m_graphClient;

        #endregion

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
        /// <param name="settingsCategory">Setting category that contains the connection settings. Defaults to "securityProvider".</param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public SecurityHub(string settingsCategory, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) : 
            this(settingsCategory, logStatusMessageFunction, logExceptionFunction, true)
        {
            // Capture initial defaults
            if (logStatusMessageFunction is not null && s_logStatusMessageFunction is null)
                s_logStatusMessageFunction = logStatusMessageFunction;

            if (logExceptionFunction is not null && s_logExceptionFunction is null)
                s_logExceptionFunction = logExceptionFunction;
        }

        // ReSharper disable once UnusedParameter.Local
        private SecurityHub(string settingsCategory, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction, bool overload) :
            base(settingsCategory ?? "securityProvider", logStatusMessageFunction ?? s_logStatusMessageFunction, logExceptionFunction ?? s_logExceptionFunction)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets Azure AD settings.
        /// </summary>
        public AzureADSettings AzureADSettings => m_azureADSettings ??= AzureADSettings.Load();

        /// <summary>
        /// Gets Graph client.
        /// </summary>
        public GraphServiceClient GraphClient => m_graphClient ??= AzureADSettings.GetGraphClient();

        #endregion

        #region [ Static ]

        // Static Fields
        private static Action<string, UpdateType> s_logStatusMessageFunction;
        private static Action<Exception> s_logExceptionFunction;

        // Static Properties

        /// <summary>
        /// Gets current default Node ID for security.
        /// </summary>
        public static readonly Guid DefaultNodeID = AdoSecurityProvider.DefaultNodeID;

        #endregion

        // Client-side script functionality

        #region [ Security Functions ]
        
        /// <summary>
        /// Resets the current provider cache.
        /// </summary>
        /// <param name="sessionCookieValue">Session ID as it appears in the cookie header value.</param>
        /// <returns><c>true</c> if session was cleared; otherwise, <c>false</c>.</returns>
        public bool Logout(string sessionCookieValue)
        {
            if (!Guid.TryParse(sessionCookieValue, out Guid sessionID))
                return false;

            // Flush any cached information that has been saved for this user including in the alternate SecurityProvider
            if (AuthenticationHandler.TryGetPrincipal(sessionID, false, out SecurityPrincipal securityPrincipal))
                SecurityProviderCache.Flush(securityPrincipal.Identity.Name);

            if (AuthenticationHandler.TryGetPrincipal(sessionID, true, out securityPrincipal))
                SecurityProviderCache.Flush(securityPrincipal.Identity.Name);

            // Clear any cached session state for user (this also clears any cached authorizations)
            return SessionHandler.ClearSessionCache(sessionID);
        }

        /// <summary>
        /// Finds the specified user account record.
        /// </summary>
        /// <param name="id">ID of requested user.</param>
        /// <returns>Specified user account record.</returns>
        public UserAccount QueryUserAccount(Guid id) => 
            DataContext.Table<UserAccount>().LoadRecord(id);

        /// <summary>
        /// Finds the specified user account record by SID or database account name.
        /// </summary>
        /// <param name="accountName">SID or database account name of requested user.</param>
        /// <returns>Specified user account record.</returns>
        public UserAccount QueryUserAccountByName(string accountName) => 
            DataContext.Table<UserAccount>().QueryRecordWhere("Name = {0}", accountName);

        /// <summary>
        /// Searches user accounts by resolved names with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "IDLabel" instances - serialized as JSON [{ id : "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchUserAccounts(string searchText) => 
            SearchUserAccounts(searchText, -1);

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
        public SecurityGroup QuerySecurityGroup(Guid id) => 
            DataContext.Table<SecurityGroup>().LoadRecord(id);

        /// <summary>
        /// Finds the specified security group record by SID or database account name.
        /// </summary>
        /// <param name="accountName">SID or database account name of requested group.</param>
        /// <returns>Specified security group record.</returns>
        public SecurityGroup QuerySecurityGroupByName(string accountName) => 
            DataContext.Table<SecurityGroup>().QueryRecordWhere("Name = {0}", accountName);

        /// <summary>
        /// Searches security groups by resolved names.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "Labels" - serialized as JSON [{ label : "value" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<Label> SearchSecurityGroups(string searchText) =>
            DataContext
                .Table<SecurityGroup>()
                .QueryRecords()
                .Select(record => UserInfo.SIDToAccountName(record.Name ?? ""))
                .Where(name => name.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase))
                .Select(Label.Create);

        /// <summary>
        /// Gets the current application role records.
        /// </summary>
        /// <returns>Current application role records.</returns>
        public IEnumerable<ApplicationRole> QueryApplicationRoles() => 
            DataContext.Table<ApplicationRole>().QueryRecords("Name", new RecordRestriction("NodeID={0}", DefaultNodeID));

        /// <summary>
        /// Determines if user is in role based on database ID values.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user is in role; otherwise, <c>false</c>.</returns>
        public bool UserIsInRole(Guid userID, Guid roleID) => 
            DataContext.Table<ApplicationRoleUserAccount>().QueryRecordCountWhere("UserAccountID={0} AND ApplicationRoleID={1}", userID, roleID) > 0;

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

            return DataContext.Table<ApplicationRoleUserAccount>().DeleteRecordWhere("UserAccountID={0} AND ApplicationRoleID={1}", userID, roleID) > 0;
        }

        /// <summary>
        /// Gets SID for a given user name.
        /// </summary>
        /// <param name="userName">User name to convert to SID.</param>
        /// <returns>SID for a given user name.</returns>
        public string UserNameToSID(string userName) => 
            UserInfo.UserNameToSID(userName);

        /// <summary>
        /// Determines if SID is for a user.
        /// </summary>
        /// <param name="sid">Security identifier to test.</param>
        /// <returns><c>true</c>if <paramref name="sid"/> is for a user; otherwise, <c>false</c>.</returns>
        [HubMethodName("isUserSID")]
        public bool IsUserSID(string sid) => 
            UserInfo.IsUserSID(sid);

        /// <summary>
        /// Determines if group is in role based on database ID values.
        /// </summary>
        /// <param name="groupID">Group ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if group is in role; otherwise, <c>false</c>.</returns>
        public bool GroupIsInRole(Guid groupID, Guid roleID) => 
            DataContext.Table<ApplicationRoleSecurityGroup>().QueryRecordCountWhere("SecurityGroupID={0} AND ApplicationRoleID={1}", groupID, roleID) > 0;

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

            return DataContext.Table<ApplicationRoleSecurityGroup>().DeleteRecordWhere("SecurityGroupID={0} AND ApplicationRoleID={1}", groupID, roleID) > 0;
        }

        /// <summary>
        /// Gets SID for a given group name.
        /// </summary>
        /// <param name="groupName">Group name to convert to SID.</param>
        /// <returns>SID for a given group name.</returns>
        public string GroupNameToSID(string groupName) => 
            UserInfo.GroupNameToSID(groupName ?? "");

        /// <summary>
        /// Determines if SID is for a group.
        /// </summary>
        /// <param name="sid">Security identifier to test.</param>
        /// <returns><c>true</c>if <paramref name="sid"/> is for a group; otherwise, <c>false</c>.</returns>
        [HubMethodName("isGroupSID")]
        public bool IsGroupSID(string sid) => 
            UserInfo.IsGroupSID(sid);

        /// <summary>
        /// Gets account name for a given SID.
        /// </summary>
        /// <param name="sid">SID to convert to a account name.</param>
        /// <returns>Account name for a given SID.</returns>
        [HubMethodName("sidToAccountName")]
        public string SIDToAccountName(string sid) => 
            UserInfo.SIDToAccountName(sid ?? "");

        /// <summary>
        /// Gets the default NodeID based on the current configuration.
        /// </summary>
        /// <returns>Default NodeID based on the current configuration.</returns>
        public Guid GetDefaultNodeID() => 
            DefaultNodeID;
        
        /// <summary>
        /// Gets the list of Azure AD users that are defined.
        /// </summary>
        /// <returns>Azure AD user list.</returns>
        public async Task<List<string>> GetAzureADUserList()
        {
            List<string> userList = new();
            GraphServiceClient graphClient = GraphClient;

            if (graphClient is null)
                return userList;

            IGraphServiceUsersCollectionPage users = await graphClient.Users.Request().GetAsync();
                
            while (users.Count > 0)
            {
                userList.AddRange(users.Select(user => user.DisplayName));

                if (users.NextPageRequest is not null)
                    users = await users.NextPageRequest.GetAsync();
                else
                    break;
            }

            return userList;
        }
        
        /// <summary>
        /// Gets the list of Azure AD groups that are defined.
        /// </summary>
        /// <returns>Azure AD group list.</returns>
        public async Task<List<string>> GetAzureADGroupList()
        {
            List<string> groupList = new();
            GraphServiceClient graphClient = GraphClient;

            if (graphClient is null)
                return groupList;

            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().GetAsync();
            
            while (groups.Count > 0)
            {
                groupList.AddRange(groups.Select(group => group.DisplayName));

                if (groups.NextPageRequest is not null)
                    groups = await groups.NextPageRequest.GetAsync();
                else
                    break;
            }

            return groupList;
        }
        
        /// <summary>
        /// Gets flag that determines if specified user name can be found on Azure AD.
        /// </summary>
        /// <param name="userName">User name to lookup.</param>
        /// <returns><c>true</c> if user name was found in Azure AD; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsValidAzureADUserName(string userName)
        {
            GraphServiceClient graphClient = GraphClient;

            if (graphClient is null)
                return false;

            // Load user data - note that external users need to be looked up by userPrincipalName
            User user = (await graphClient.Users.Request().Filter($"mail eq '{userName}'").GetAsync()).FirstOrDefault() 
            return user is not null;
        }

        /// <summary>
        /// Gets flag that determines if specified group name can be found on Azure AD.
        /// </summary>
        /// <param name="groupName">Group name to lookup</param>
        /// <returns><c>true</c> if group name was found in Azure AD; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsValidAzureADGroupName(string groupName)
        {
            // Either of the following returns false positives:
            //return graphClient is not null && await graphClient.Groups[groupName].Request().GetAsync() is not null;
            //return graphClient is not null && await graphClient.Groups.Request().Filter($"displayName eq '{groupName}'").GetAsync() is not null;

            GraphServiceClient graphClient = GraphClient;

            if (graphClient is null)
                return false;

            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().GetAsync();
            
            while (groups.Count > 0)
            {
                if (groups.Any(group => group.DisplayName.Equals(groupName, StringComparison.OrdinalIgnoreCase)))
                    return true;

                if (groups.NextPageRequest is not null)
                    groups = await groups.NextPageRequest.GetAsync();
                else
                    break;
            }

            return false;
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
        public int QueryUserAccountCount(string filterText) => 
            DataContext.Table<UserAccount>().QueryRecordCount(filterText);

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
        public IEnumerable<UserAccount> QueryUserAccounts(string sortField, bool ascending, int page, int pageSize, string filterText) => 
            DataContext.Table<UserAccount>().QueryRecords(sortField, ascending, page, pageSize, filterText);

        /// <summary>
        /// Deletes user account.
        /// </summary>
        /// <param name="id">Unique ID of user account.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.DeleteRecord)]
        public void DeleteUserAccount(Guid id) => 
            DataContext.Table<UserAccount>().DeleteRecord(id);

        /// <summary>
        /// Creates a new user account model instance.
        /// </summary>
        /// <returns>A new user account model instance.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.CreateNewRecord)]
        public UserAccount NewUserAccount() => 
            DataContext.Table<UserAccount>().NewRecord();

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
        public int QuerySecurityGroupCount(string filterText) => 
            DataContext.Table<SecurityGroup>().QueryRecordCount();

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
        public IEnumerable<SecurityGroup> QuerySecurityGroups(string sortField, bool ascending, int page, int pageSize, string filterText) => 
            DataContext.Table<SecurityGroup>().QueryRecords(sortField, ascending, page, pageSize, filterText);

        /// <summary>
        /// Deletes security group.
        /// </summary>
        /// <param name="id">Unique ID of security group.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.DeleteRecord)]
        public void DeleteSecurityGroup(Guid id) => 
            DataContext.Table<SecurityGroup>().DeleteRecord(id);

        /// <summary>
        /// Creates a new security group model instance.
        /// </summary>
        /// <returns>A new security group model instance.</returns>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.CreateNewRecord)]
        public SecurityGroup NewSecurityGroup() => 
            DataContext.Table<SecurityGroup>().NewRecord();

        /// <summary>
        /// Adds a new security group.
        /// </summary>
        /// <param name="record">Security group model instance to add.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.AddNewRecord)]
        public void AddNewSecurityGroup(SecurityGroup record) => 
            DataContext.Table<SecurityGroup>().AddNewRecord(record);

        /// <summary>
        /// Updates security group.
        /// </summary>
        /// <param name="record">Security group model instance to update.</param>
        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(SecurityGroup), RecordOperation.UpdateRecord)]
        public void UpdateSecurityGroup(SecurityGroup record) => 
            DataContext.Table<SecurityGroup>().UpdateRecord(record);

        #endregion
    }
}
