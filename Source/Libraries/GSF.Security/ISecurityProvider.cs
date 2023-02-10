//******************************************************************************************************
//  ISecurityProvider.cs - Gbtc
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
//  06/25/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/03/2010 - Pinal C. Patel
//       Added TranslateRole() to allow providers to perform translation on role name.
//  01/05/2011 - Pinal C. Patel
//       Added CanRefreshData, CanUpdateData, CanResetPassword and CanChangePassword properties along 
//       with accompanying RefreshData(), UpdateData(), ResetPassword()  and ChangePassword() methods.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using GSF.Configuration;

namespace GSF.Security
{
    /// <summary>
    /// Defines the function signature delegate used for logging events from the <see cref="ISecurityProvider"/>.
    /// </summary>
    /// <param name="source">The source by which the application is registered on the specified computer.</param>
    /// <param name="message">The string to write to the event log.</param>
    /// <param name="type">One of the <see cref="EventLogEntryType"/> values.</param>
    /// <param name="eventID">The application-specific identifier for the event.</param>
    public delegate void LogEventFunctionSignature(string source, string message, EventLogEntryType type, int eventID);

    /// <summary>
    /// Defines a provider of role-based security in applications.
    /// </summary>
    public interface ISecurityProvider : IPersistSettings
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the application being secured as defined in the backend security data store.
        /// </summary>
        string ApplicationName{ get; set; }

        /// <summary>
        /// Gets or sets the connection string to be used for connection to the backend security data store.
        /// </summary>
        string ConnectionString{ get; set; }

        /// <summary>
        /// Gets or sets the principal used for passthrough authentication.
        /// </summary>
        IPrincipal PassthroughPrincipal{ get; set; }

        /// <summary>
        /// Gets or sets the password as a <see cref="SecureString"/>.
        /// </summary>
        SecureString SecurePassword{ get; set; }

        /// <summary>
        /// Gets or sets <see cref="SecurePassword"/> as clear text password.
        /// </summary>
        string Password{ get; set; }

        /// <summary>
        /// Gets the <see cref="UserData"/> object containing information about the user.
        /// </summary>
        UserData UserData{ get; }

        /// <summary>
        /// Gets the flag that indicates whether the user was
        /// authenticated during the last authentication attempt.
        /// </summary>
        bool IsUserAuthenticated{ get; }

        /// <summary>
        /// Gets the flag that indicates whether the user 
        /// needs to be redirected after the Authentication attempt. 
        /// Could be used for asking Users to confirm Messages etc.
        /// </summary>
        bool IsRedirectRequested{ get; }
        /// <summary>
        /// Gets the URI that user will be redirected to if <see cref="IsRedirectRequested"/> is set.
        /// </summary>
        string RequestedRedirect{ get; }       
        
        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="RefreshData"/> operation is supported.
        /// </summary>
        bool CanRefreshData{ get; }

        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="ResetPassword"/> operation is supported.
        /// </summary>
        bool CanResetPassword{ get; }

        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="ChangePassword"/> operation is supported.
        /// </summary>
        bool CanChangePassword{ get; }

        /// <summary>
        /// Gets an authentication failure reason, if set by the provider when authentication fails.
        /// </summary>
        string AuthenticationFailureReason{ get; }

        /// <summary>
        /// Gets or sets the <see cref="LogEventFunctionSignature"/> to use for logging security events for the <see cref="SecurityProviderBase"/> implementation.
        /// </summary>
        /// <remarks>
        /// Set <see cref="LogEvent"/> to <c>null</c> to use default handler, i.e., <see cref="EventLog.WriteEntry(string,string,EventLogEntryType,int)"/>.
        /// </remarks>
        LogEventFunctionSignature LogEvent{ get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        bool Authenticate();

        /// <summary>
        /// Refreshes the <see cref="UserData"/> from the backend data store.
        /// </summary>
        /// <returns>true if <see cref="UserData"/> is refreshed, otherwise false.</returns>
        bool RefreshData();

        /// <summary>
        /// Resets user password in the backend data store.
        /// </summary>
        /// <param name="securityAnswer">Answer to the user's security question.</param>
        /// <returns>true if the password is reset, otherwise false.</returns>
        bool ResetPassword(string securityAnswer);

        /// <summary>
        /// Changes user password in the backend data store.
        /// </summary>
        /// <param name="oldPassword">User's current password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>true if the password is changed, otherwise false.</returns>
        bool ChangePassword(string oldPassword, string newPassword);

        /// <summary>
        /// Performs a translation of the specified user <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The user role to be translated.</param>
        /// <returns>The user role that the specified user <paramref name="role"/> translates to.</returns>
        string TranslateRole(string role);

        /// <summary>
        /// Performs a translation of the default login page to a different endpoint.
        /// </summary>
        /// <param name="loginUrl"> The URI of the login page specified in the AppSettings </param>
        /// <param name="uri"> The URI originally requested. </param>
        /// <param name="encodedPath"> The URI requested by the client </param>
        /// <param name="referrer"> The Referrer as specified in the request header </param>
        /// <returns> The URI to be redirected to</returns>
        string TranslateRedirect(string loginUrl, Uri uri, string encodedPath, string referrer);

        /// <summary>
        /// Gets a list of Roles for this user for a specified ApplicationId.
        /// </summary>
        /// <param name="applicationId">The applicationId for the roles to be returned.</param>
        /// <returns>The roles that the specified user has.</returns>
        List<string> GetUserRoles(string applicationId);

        #endregion
    }
}
