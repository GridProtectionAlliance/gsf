//******************************************************************************************************
//  SecurityService.svc.cs - Gbtc
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
//  05/18/2010 - Pinal C. Patel
//       Generated original version of source code.
//  05/24/2010 - Pinal C. Patel
//       Modified the service so it could be hosted in ASP.NET compatibility mode.
//       Modified Login() to initialize the provider once and cache it for subsequent uses.
//  12/09/2010 - Pinal C. Patel
//       Renamed Login operation to Authenticate and added GetUserData and RefreshUserData operations.
//  01/14/2011 - Pinal C. Patel
//       Added ResetPassword() and ChangePassword() methods.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Diagnostics.CodeAnalysis;
using System.ServiceModel.Activation;
using GSF.Security;
using GSF.ServiceModel;

namespace GSF.Web.Embedded
{
    /// <summary>
    /// Embedded WCF service that can be used for securing applications using role-based security.
    /// </summary>
    /// <remarks>
    /// This list shows the endpoints exposed by <see cref="SecurityService"/>:
    /// <list type="table">
    ///     <listheader>
    ///         <term>URI</term>
    ///         <description>Protocol</description>
    ///     </listheader>
    ///     <item>
    ///         <term><b>~/SecurityService.svc/soap</b></term>
    ///         <description>SOAP 1.1</description>
    ///     </item>
    ///     <item>
    ///         <term><b>~/SecurityService.svc/rest</b></term>
    ///         <description>REST</description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// This example shows how to consume the REST endpoint of <see cref="SecurityService"/> using <a href="http://jquery.com/" target="_blank">jQuery</a>:
    /// <code>
    /// <![CDATA[
    /// <script src="jquery.js" type="text/javascript" />
    /// <script language="javascript" type="text/javascript">
    ///     $(document).ready(function () { login(); });
    /// 
    ///     function login() {
    ///         $.get("SecurityService.svc/rest/getuserdata", loginCallback);
    ///     }
    /// 
    ///     function loginCallback(data) {
    ///         var user = new UserData(data);
    ///         if (!user.isAuthenticated) {
    ///             alert('Access is denied.');
    ///         }
    ///         else {
    ///             alert('Welcome ' + user.firstName + '!');
    ///         }
    ///     }
    /// 
    ///     function UserData(xml) {
    ///         this.username = $(xml).find('Username').text();
    ///         this.password = $(xml).find('Password').text();
    ///         this.firstName = $(xml).find('FirstName').text();
    ///         this.lastName = $(xml).find('LastName').text();
    ///         this.companyName = $(xml).find('CompanyName').text();
    ///         this.phoneNumber = $(xml).find('PhoneNumber').text();
    ///         this.emailAddress = $(xml).find('EmailAddress').text();
    ///         this.securityQuestion = $(xml).find('SecurityQuestion').text();
    ///         this.securityAnswer = $(xml).find('SecurityAnswer').text();
    ///         this.passwordChangeDateTime = $(xml).find('PasswordChangeDateTime').text();
    ///         this.accountCreatedDateTime = $(xml).find('AccountCreatedDateTime').text();
    ///         this.isDefined = $(xml).find('IsDefined').text() === 'true';
    ///         this.isExternal = $(xml).find('IsExternal').text() === 'true';
    ///         this.isDisabled = $(xml).find('IsDisabled').text() === 'true';
    ///         this.isLockedOut = $(xml).find('IsLockedOut').text() === 'true';
    ///         this.isAuthenticated = $(xml).find('IsAuthenticated').text() === 'true';
    ///         // Retrieve user groups.
    ///         this.groups = groups = new Array();
    ///         $(xml).find('Group').each(function () { groups.push($(this).text()); });
    ///         // Retrieve user roles.
    ///         this.roles = roles = new Array();
    ///         $(xml).find('Role').each(function () { roles.push($(this).text()); });
    ///     }
    /// </script>
    /// ]]>
    /// </code>
    /// </example>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public class SecurityService : SelfHostingService, ISecurityService
    {
        /// <summary>
        /// Returns information about the current user. 
        /// </summary>
        /// <returns>An <see cref="UserData"/> object of the user if user's security context has been initialized, otherwise null.</returns>
        public UserData GetUserData()
        {
            SecurityProviderCache.ValidateCurrentProvider();

            return SecurityProviderCache.CurrentProvider.UserData;
        }

        /// <summary>
        /// Refreshes and returns information about the current user. 
        /// </summary>
        /// <returns>An <see cref="UserData"/> object of the user if user's security context has been initialized, otherwise null.</returns>
        public UserData RefreshUserData()
        {
            SecurityProviderCache.ValidateCurrentProvider();

            if (SecurityProviderCache.CurrentProvider.CanRefreshData)
                SecurityProviderCache.CurrentProvider.RefreshData();

            return SecurityProviderCache.CurrentProvider.UserData;
        }

        /// <summary>
        /// Authenticates a user and caches the security context upon successful authentication for subsequent use.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <param name="password">Password of the user.</param>
        /// <returns>An <see cref="UserData"/> object of the user.</returns>
        public UserData Authenticate(string username, string password)
        {
            ISecurityProvider provider = SecurityProviderUtility.CreateProvider(username);

            if (provider.Authenticate(password))
                SecurityProviderCache.CurrentProvider = provider;

            return provider.UserData;
        }

        /// <summary>
        /// Resets user password.
        /// </summary>
        /// <param name="securityAnswer">Answer to user's security question.</param>
        /// <returns>true if password is reset, otherwise false.</returns>
        public bool ResetPassword(string securityAnswer)
        {
            SecurityProviderCache.ValidateCurrentProvider();

            if (!SecurityProviderCache.CurrentProvider.CanResetPassword)
                return false;

            return SecurityProviderCache.CurrentProvider.ResetPassword(securityAnswer);
        }

        /// <summary>
        /// Changes user password.
        /// </summary>
        /// <param name="oldPassword">User's current password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>true if the password is changed, otherwise false.</returns>
        public bool ChangePassword(string oldPassword, string newPassword)
        {
            SecurityProviderCache.ValidateCurrentProvider();

            if (!SecurityProviderCache.CurrentProvider.CanChangePassword)
                return false;

            return SecurityProviderCache.CurrentProvider.ChangePassword(oldPassword, newPassword);
        }
    }
}
