//******************************************************************************************************
//  ISecurityService.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  05/18/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/09/2010 - Pinal C. Patel
//       Renamed Login operation to Authenticate and added GetUserData and RefreshUserData operations.
//  01/14/2011 - Pinal C. Patel
//       Changed XML serializer from DataContractSerializer to XmlSerializer for more control over 
//       serialization of data.
//       Added ResetPassword() and ChangePassword() methods.
//  07/22/2011 - Pinal C. Patel
//       Switched REST endpoints to using query string for input parameters instead of passing them
//       as part of the URL so sensitive information like password gets encrypted when used with SSL.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.ServiceModel;
using System.ServiceModel.Web;
using GSF.Security;
using GSF.ServiceModel;

namespace GSF.Web.Embedded
{
    /// <summary>
    /// Embedded WCF service contract used for securing external facing WCF services.
    /// </summary>
    [ServiceContract, XmlSerializerFormat]
    public interface ISecurityService : ISelfHostingService
    {
        /// <summary>
        /// Returns information about the current user. 
        /// </summary>
        /// <returns>An <see cref="UserData"/> object of the user if user's security context has been initialized, otherwise null.</returns>
        [OperationContract, WebGet(UriTemplate = "/getuserdata")]
        UserData GetUserData();

        /// <summary>
        /// Refreshes and returns information about the current user. 
        /// </summary>
        /// <returns>An <see cref="UserData"/> object of the user if user's security context has been initialized, otherwise null.</returns>
        [OperationContract, WebGet(UriTemplate = "/refreshuserdata")]
        UserData RefreshUserData();

        /// <summary>
        /// Authenticates a user and caches the security context upon successful authentication for subsequent use.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <param name="password">Password of the user.</param>
        /// <returns>An <see cref="UserData"/> object of the user.</returns>
        [OperationContract, WebGet(UriTemplate = "/authenticate?username={username}&password={password}")]
        UserData Authenticate(string username, string password);

        /// <summary>
        /// Resets user password.
        /// </summary>
        /// <param name="securityAnswer">Answer to user's security question.</param>
        /// <returns>true if password is reset, otherwise false.</returns>
        [OperationContract, WebGet(UriTemplate = "/resetpassword?securityanswer={securityAnswer}")]
        bool ResetPassword(string securityAnswer);

        /// <summary>
        /// Changes user password.
        /// </summary>
        /// <param name="oldPassword">User's current password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>true if the password is changed, otherwise false.</returns>
        [OperationContract, WebGet(UriTemplate = "/changepassword?oldpassword={oldPassword}&newpassword={newPassword}")]
        bool ChangePassword(string oldPassword, string newPassword);
    }
}
