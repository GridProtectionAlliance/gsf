//******************************************************************************************************
//  AuthenticationOptions.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/25/2017 - Stephen C. sWills
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.Web.Security
{
    /// <summary>
    /// Options for authentication using <see cref="AuthenticationHandler"/>.
    /// </summary>
    public class AuthenticationOptions : Microsoft.Owin.Security.AuthenticationOptions
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="AnonymousResources"/>.
        /// </summary>
        public const string DefaultAnonymousResources = "/Login.cshtml,/favicon.ico";

        /// <summary>
        /// Default value for <see cref="LoginPage"/>.
        /// </summary>
        public const string DefaultLoginPage = "/Login.cshtml";

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        public AuthenticationOptions()
            : base("x-gsf-auth")
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the paths to the resources on the web
        /// server that can be provided without checking credentials.
        /// </summary>
        public string[] AnonymousResources { get; set; } = DefaultAnonymousResources.Split(',');

        /// <summary>
        /// Gets or sets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken { get; set; } = SessionHandler.DefaultSessionToken;

        /// <summary>
        /// Gets or sets the login page used as a redirect location when authentication fails.
        /// </summary>
        public string LoginPage { get; set; } = DefaultLoginPage;

        #endregion
    }
}
