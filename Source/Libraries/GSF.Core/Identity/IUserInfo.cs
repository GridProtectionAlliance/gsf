//******************************************************************************************************
//  IUserInfo.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  08/27/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Identity
{
    /// <summary>
    /// Internal interface that represents common properties and methods needed by <see cref="UserInfo"/>
    /// implemented in both Windows and Unix flavors.
    /// </summary>
    internal interface IUserInfo : IDisposable
    {
        /// <summary>
        /// Gets flag that determines if domain is responding to user existence.
        /// </summary>
        /// <returns><c>true</c> if domain responds and user exists; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Note that when the domain is unavailable, this function will return <c>false</c>.
        /// </remarks>
        bool DomainRespondsForUser { get; }

        /// <summary>
        /// Gets flag that determines if user exists.
        /// </summary>
        /// <returns><c>true</c> if user is found to exist; otherwise <c>false</c>.</returns>
        bool Exists{ get; }

        /// <summary>
        /// Gets or sets enabled state.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the last login time of the user.
        /// </summary>
        DateTime LastLogon { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when the account was created.
        /// </summary>
        DateTime AccountCreationDate { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/>, in UTC, of next password change for the user.
        /// </summary>
        DateTime NextPasswordChangeDate { get; }

        /// <summary>
        /// Gets the account control information of the local user.
        /// </summary>
        int LocalUserAccountControl { get; }

        /// <summary>
        /// Gets this maximum password age for the user.
        /// </summary>
        Ticks MaximumPasswordAge { get; }

        /// <summary>
        /// Gets all the groups associated with the user - this includes local groups and Active Directory groups if applicable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Groups names are prefixed with their associated domain, computer name or BUILTIN.
        /// </para>
        /// </remarks>
        string[] Groups { get; }

        /// <summary>
        /// Gets the local groups the user is a member of.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Groups names are prefixed with BUILTIN or computer name.
        /// </para>
        /// </remarks>
        string[] LocalGroups { get; }

        /// <summary>
        /// Gets the full user name for local accounts.
        /// </summary>
        string FullLocalUserName { get; }

        /// <summary>
        /// Gets flag that determines if this <see cref="UserInfo"/> instance is based on a local WinNT account instead of found through LDAP.
        /// </summary>
        bool IsLocalAccount { get; }

        /// <summary>
        /// Initializes the <see cref="UserInfo"/> object.
        /// </summary>
        /// <returns><c>true</c> if successfully initialized; otherwise, <c>false</c>.</returns>
        bool Initialize();

        /// <summary>
        /// Attempts to change the user's password.
        /// </summary>
        /// <param name="oldPassword">Old password.</param>
        /// <param name="newPassword">New password.</param>
        void ChangePassword(string oldPassword, string newPassword);

        /// <summary>
        /// Returns the value for specified active directory property.
        /// </summary>
        /// <param name="propertyName">Name of the active directory property whose value is to be retrieved.</param>
        /// <returns><see cref="String"/> value for the specified active directory property.</returns>
        string GetUserPropertyValue(string propertyName);
    }
}