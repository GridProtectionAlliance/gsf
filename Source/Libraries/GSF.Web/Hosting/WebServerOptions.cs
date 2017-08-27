//******************************************************************************************************
//  WebServerOptions.cs - Gbtc
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
//  08/27/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Web.Security;

namespace GSF.Web.Hosting
{
    /// <summary>
    /// Represents configuration options for a <see cref="WebServer"/> instance.
    /// </summary>
    public class WebServerOptions
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="WebRootPath"/>.
        /// </summary>
        public const string DefaultWebRootPath = "wwwroot";

        /// <summary>
        /// Default value for <see cref="ClientCacheEnabled"/>.
        /// </summary>
        public const bool DefaultClientCacheEnabled = true;

        /// <summary>
        /// Default value for <see cref="MinifyJavascript"/>.
        /// </summary>
        public const bool DefaultMinifyJavascript = true;

        /// <summary>
        /// Default value for <see cref="MinifyStyleSheets"/>.
        /// </summary>
        public const bool DefaultMinifyStyleSheets = true;

        /// <summary>
        /// Default value for <see cref="UseMinifyInDebug"/>.
        /// </summary>
        public const bool DefaultUseMinifyInDebug = false;

        /// <summary>
        /// Default value for <see cref="AuthTestPage"/>.
        /// </summary>
        public const string DefaultAuthTestPage = "AuthTest";

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets root path defined for this <see cref="WebServer"/>.
        /// </summary>
        public string WebRootPath { get; set; } = DefaultWebRootPath;

        /// <summary>
        /// Gets or sets flag that determines if cache control is enabled for browser clients; default to <c>true</c>.
        /// </summary>
        public bool ClientCacheEnabled { get; set; } = DefaultClientCacheEnabled;

        /// <summary>
        /// Gets or sets value that determines if minification should be applied to rendered Javascript files.
        /// </summary>
        public bool MinifyJavascript { get; set; } = DefaultMinifyJavascript;

        /// <summary>
        /// Gets or sets value that determines if minification should be applied to rendered CSS files.
        /// </summary>
        public bool MinifyStyleSheets { get; set; } = DefaultMinifyStyleSheets;

        /// <summary>
        /// Gets or sets value that determines if minification should be applied when running a Debug build.
        /// </summary>
        public bool UseMinifyInDebug { get; set; } = DefaultUseMinifyInDebug;

        /// <summary>
        /// Gets or sets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken { get; set; } = SessionHandler.DefaultSessionToken;

        /// <summary>
        /// Gets or sets the page name used to test user authorization.
        /// </summary>
        public string AuthTestPage { get; set; } = DefaultAuthTestPage;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a copy of this <see cref="WebServerOptions"/> instance.
        /// </summary>
        /// <returns>Copy of this <see cref="WebServerOptions"/> instance.</returns>
        public WebServerOptions Clone()
        {
            return new WebServerOptions
            {
                WebRootPath = WebRootPath,
                ClientCacheEnabled = ClientCacheEnabled,
                MinifyJavascript = MinifyJavascript,
                MinifyStyleSheets = MinifyStyleSheets,
                UseMinifyInDebug = UseMinifyInDebug,
                SessionToken = SessionToken,
                AuthTestPage = AuthTestPage
            };
        }

        #endregion
    }
}
