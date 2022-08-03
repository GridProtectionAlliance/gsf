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

using System.Text.RegularExpressions;
using GSF.IO;
using GSF.Web.Security;
using GSF.Web.Shared;

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
        /// Default value for <see cref="ErrorTemplateName"/>.
        /// </summary>
        public const string DefaultErrorTemplateName = Resources.DefaultRoot + "/Shared/Views/Error.cshtml";

        /// <summary>
        /// Default value for <see cref="ClientCacheEnabled"/>.
        /// </summary>
        public const bool DefaultClientCacheEnabled = true;

        /// <summary>
        /// Default value for <see cref="MinifyJavascript"/>.
        /// </summary>
        public const bool DefaultMinifyJavascript = true;

        /// <summary>
        /// Default value for the <see cref="MinifyJavascriptExclusionExpression"/>.
        /// </summary>
        public const string DefaultMinifyJavascriptExclusionExpression = "";

        /// <summary>
        /// Default value for <see cref="MinifyStyleSheets"/>.
        /// </summary>
        public const bool DefaultMinifyStyleSheets = true;

        /// <summary>
        /// Default value for the <see cref="MinifyStyleSheetsExclusionExpression"/>.
        /// </summary>
        public const string DefaultMinifyStyleSheetsExclusionExpression = "";

        /// <summary>
        /// Default value for <see cref="UseMinifyInDebug"/>.
        /// </summary>
        public const bool DefaultUseMinifyInDebug = false;

        // Fields
        private string m_authTestPage;
        private string m_webRootPath;
        private string m_minifyJavascriptExclusionExpression;
        private string m_minifyStyleSheetsExclusionExpression;
        private Regex m_minifyJavascriptExclusionRegex;
        private Regex m_minifyStyleSheetsExclusionRegex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="WebServerOptions"/> instance.
        /// </summary>
        public WebServerOptions()
        {
            WebRootPath = DefaultWebRootPath;
            MinifyJavascriptExclusionExpression = DefaultMinifyJavascriptExclusionExpression;
            MinifyStyleSheetsExclusionExpression = DefaultMinifyStyleSheetsExclusionExpression;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets root path defined for this <see cref="WebServer"/>.
        /// </summary>
        public string WebRootPath
        {
            get => m_webRootPath;
            set
            {
                m_webRootPath = value;
                PhysicalWebRootPath = FilePath.AddPathSuffix(FilePath.GetAbsolutePath(m_webRootPath));
            }
        }

        /// <summary>
        /// Gets physical path for <see cref="WebRootPath"/>.
        /// </summary>
        public string PhysicalWebRootPath { get; private set; }

        /// <summary>
        /// Gets or sets flag that determines if cache control is enabled for browser clients; default to <c>true</c>.
        /// </summary>
        public bool ClientCacheEnabled { get; set; } = DefaultClientCacheEnabled;

        /// <summary>
        /// Gets or sets value that determines if minification should be applied to rendered Javascript files.
        /// </summary>
        public bool MinifyJavascript { get; set; } = DefaultMinifyJavascript;

        /// <summary>
        /// Gets or sets the regular expression that will exclude Javascript files from being minified.
        /// Assigning empty or <c>null</c> value results in all files targeted for minification.
        /// </summary>
        public string MinifyJavascriptExclusionExpression
        {
            get => m_minifyJavascriptExclusionExpression;
            set
            {
                m_minifyJavascriptExclusionExpression = value;
                m_minifyJavascriptExclusionRegex = string.IsNullOrWhiteSpace(value) ?
                    null : 
                    new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Gets or sets value that determines if minification should be applied to rendered CSS files.
        /// </summary>
        public bool MinifyStyleSheets { get; set; } = DefaultMinifyStyleSheets;

        /// <summary>
        /// Gets or sets the regular expression that will exclude CSS files from being minified.
        /// Assigning empty or <c>null</c> value results in all files targeted for minification.
        /// </summary>
        public string MinifyStyleSheetsExclusionExpression
        {
            get => m_minifyStyleSheetsExclusionExpression;
            set
            { 
                m_minifyStyleSheetsExclusionExpression = value;
                m_minifyStyleSheetsExclusionRegex = string.IsNullOrWhiteSpace(value) ?
                    null :
                    new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

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
        /// <remarks>
        /// Page name for web server use will not be prefixed with slash. Any slash
        /// prefix will automatically be removed during value assignment.
        /// </remarks>
        public string AuthTestPage
        {
            get
            {
                if (m_authTestPage is null)
                    AuthTestPage = AuthenticationOptions.DefaultAuthTestPage;

                return m_authTestPage;
            }
            set
            {
                m_authTestPage = value;

                // Remove any slash prefix for WebServer use
                if (!string.IsNullOrEmpty(m_authTestPage) && m_authTestPage.StartsWith("/"))
                    m_authTestPage = m_authTestPage.Length > 1 ? m_authTestPage.Substring(1) : "";
            }
        }

        /// <summary>
        /// Gets or sets template file name to use when a Razor compile or execution exception occurs.
        /// </summary>
        public string ErrorTemplateName { get; set; } = DefaultErrorTemplateName;

        /// <summary>
        /// Gets an immutable version of the web server options.
        /// </summary>
        public ReadonlyWebServerOptions Readonly => new(this);

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines if Javascript file referenced by <paramref name="urlPath"/> should be minified
        /// according to <see cref="MinifyJavascriptExclusionExpression"/>.
        /// </summary>
        /// <param name="urlPath">Javascript filename to check.</param>
        /// <returns><c>true</c> if <paramref name="urlPath"/> should be minified; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Result will always be <c>false</c> if <see cref="MinifyJavascript"/> is not <c>true</c>.
        /// </remarks>
        public bool MinifyJavascriptResource(string urlPath)
        {
            if (!MinifyJavascript)
                return false;

            if (m_minifyJavascriptExclusionRegex is null)
                return true;

            return !m_minifyJavascriptExclusionRegex.IsMatch(urlPath);
        }

        /// <summary>
        /// Determines if CSS file referenced by <paramref name="urlPath"/> should be minified
        /// according to <see cref="MinifyStyleSheetsExclusionExpression"/>.
        /// </summary>
        /// <param name="urlPath">CSS filename to check.</param>
        /// <returns><c>true</c> if <paramref name="urlPath"/> should be minified; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Result will always be <c>false</c> if <see cref="MinifyStyleSheets"/> is not <c>true</c>.
        /// </remarks>
        public bool MinifyStyleSheetResource(string urlPath)
        {
            if (!MinifyStyleSheets)
                return false;

            if (m_minifyStyleSheetsExclusionRegex is null)
                return true;

            return !m_minifyStyleSheetsExclusionRegex.IsMatch(urlPath);
        }

        #endregion
    }

    /// <summary>
    /// Represents an immutable version of <see cref="WebServerOptions"/>.
    /// </summary>
    public class ReadonlyWebServerOptions
    {
        #region [ Members ]

        // Fields
        private readonly WebServerOptions m_webServerOptions;

        #endregion

        #region [ Constructors ]

        internal ReadonlyWebServerOptions(WebServerOptions webServerOptions)
        {
            // Make sure exposed properties cannot change source web server options
            m_webServerOptions = webServerOptions;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets root path defined for this <see cref="WebServer"/>.
        /// </summary>
        public string WebRootPath => m_webServerOptions.WebRootPath;

        /// <summary>
        /// Gets physical path for <see cref="WebRootPath"/>.
        /// </summary>
        public string PhysicalWebRootPath => m_webServerOptions.PhysicalWebRootPath;

        /// <summary>
        /// Gets flag that determines if cache control is enabled for browser clients; default to <c>true</c>.
        /// </summary>
        public bool ClientCacheEnabled => m_webServerOptions.ClientCacheEnabled;

        /// <summary>
        /// Gets value that determines if minification should be applied to rendered Javascript files.
        /// </summary>
        public bool MinifyJavascript => m_webServerOptions.MinifyJavascript;

        /// <summary>
        /// Gets regular expression that will exclude Javascript files from being minified.
        /// </summary>
        public string MinifyJavascriptExclusionExpression => m_webServerOptions.MinifyJavascriptExclusionExpression;

        /// <summary>
        /// Gets value that determines if minification should be applied to rendered CSS files.
        /// </summary>
        public bool MinifyStyleSheets => m_webServerOptions.MinifyStyleSheets;

        /// <summary>
        /// Gets regular expression that will exclude CSS files from being minified.
        /// </summary>
        public string MinifyStyleSheetsExclusionExpression => m_webServerOptions.MinifyStyleSheetsExclusionExpression;

        /// <summary>
        /// Gets value that determines if minification should be applied when running a Debug build.
        /// </summary>
        public bool UseMinifyInDebug => m_webServerOptions.UseMinifyInDebug;

        /// <summary>
        /// Gets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken => m_webServerOptions.SessionToken;

        /// <summary>
        /// Gets the page name used to test user authorization.
        /// </summary>
        /// <remarks>
        /// Page name for web server use will not be prefixed with slash.
        /// </remarks>
        public string AuthTestPage => m_webServerOptions.AuthTestPage;

        /// <summary>
        /// Gets template file name to use when a Razor compile or execution exception occurs.
        /// </summary>
        public string ErrorTemplateName => m_webServerOptions.ErrorTemplateName;

        #endregion
    }
}
