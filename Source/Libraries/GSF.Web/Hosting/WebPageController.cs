//******************************************************************************************************
//  WebPageController.cs - Gbtc
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
//  01/12/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using GSF.Data;
using GSF.Web.Model;
using GSF.Web.Security;

namespace GSF.Web.Hosting
{
    /// <summary>
    /// Defines a mini-web server with Razor support using the self-hosted API controller.
    /// </summary>
    public class WebPageController : ApiController
    {
        #region [ Members ]

        // Nested Types

        // Web page controller dependency resolver
        private sealed class WebPageControllerResolver : IDependencyResolver
        {
            private readonly WebServer m_webServer;
            private readonly string m_defaultWebPage;
            private readonly object m_model;
            private readonly Type m_modelType;
            private readonly AdoDataConnection m_database;

            public WebPageControllerResolver(WebServer webServer, string defaultWebPage, object model, Type modelType, AdoDataConnection database)
            {
                m_webServer = webServer;
                m_defaultWebPage = defaultWebPage;
                m_model = model;
                m_modelType = modelType;
                m_database = database;
            }

            void IDisposable.Dispose()
            {
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(WebPageController))
                    return new WebPageController(m_webServer)
                    {
                        DefaultWebPage = m_defaultWebPage,
                        Model = m_model,
                        ModelType = m_modelType,
                        Database = m_database
                    };

                return null;
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                if (serviceType == typeof(WebPageController))
                    return new[]
                    {
                        new WebPageController(m_webServer)
                        {
                            DefaultWebPage = m_defaultWebPage,
                            Model = m_model,
                            ModelType = m_modelType,
                            Database = m_database
                        }
                    };

                return new List<object>();
            }

            public IDependencyScope BeginScope() => this;
        }

        // Fields
        private readonly WebServer m_webServer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="WebPageController"/> using the default configured <see cref="WebServer"/> instance.
        /// </summary>
        public WebPageController() : this(null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="WebPageController"/> using specified <paramref name="webServer"/>.
        /// </summary>
        /// <param name="webServer"><see cref="WebServer"/> instance to use for controller; uses default instance if not provided.</param>
        public WebPageController(WebServer webServer)
        {
            m_webServer = webServer ?? WebServer.Default;
            DefaultWebPage = "Index.html";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets default web page to use for this <see cref="WebPageController"/>.
        /// </summary>
        public string DefaultWebPage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="RazorView"/> model instance for this <see cref="WebPageController"/>, if any.
        /// </summary>
        public object Model
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="RazorView"/> model <see cref="Type"/> for this <see cref="WebPageController"/>, if any.
        /// </summary>
        public Type ModelType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets database connection to provide to <see cref="RazorView"/> instances in this <see cref="WebPageController"/>, if any.
        /// </summary>
        public AdoDataConnection Database
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="Hosting.WebServer"/> instance used by this <see cref="WebPageController"/>.
        /// </summary>
        public WebServer WebServer => m_webServer;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Default page request handler.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        /// <returns>Rendered page result for given page.</returns>
        [Route, HttpGet]
        public Task<HttpResponseMessage> GetPage(CancellationToken cancellationToken)
        {
            return GetPage(DefaultWebPage, cancellationToken);
        }

        /// <summary>
        /// Common page request handler.
        /// </summary>
        /// <param name="pageName">Page name to render.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        /// <returns>Rendered page result for given page.</returns>
        [Route("{*pageName}"), HttpGet]
        public Task<HttpResponseMessage> GetPage(string pageName, CancellationToken cancellationToken)
        {
            return m_webServer.RenderResponse(Request, pageName, cancellationToken, Model, ModelType, Database);
        }
        /// <summary>
        /// Common page post handler.
        /// </summary>
        /// <param name="pageName">Page name to render.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        /// <returns>Rendered page result for given page.</returns>
        [Route("{*pageName}"), HttpPost]
        [ValidateRequestVerificationToken(FormValidation = true)]
        [SuppressMessage("Security", "SG0016", Justification = "CSRF vulnerability handled via ValidateRequestVerificationToken.")]
        public Task<HttpResponseMessage> PostPage(string pageName, CancellationToken cancellationToken)
        {
            return GetPage(pageName, cancellationToken);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets a dependency resolver.
        /// </summary>
        /// <param name="webServer"><see cref="WebServer"/> instance to use for controller; uses default instance if not provided.</param>
        /// <param name="defaultWebPage">Default web page name to use for controller; uses "index.html" if not provided.</param>
        /// <param name="model">Reference to model to use when rendering Razor templates, if any.</param>
        /// <param name="modelType">Type of <paramref name="model"/>, if any.</param>
        /// <param name="connection"><see cref="AdoDataConnection"/> to use, if any.</param>
        /// <returns>Dependency resolver for the specified parameters.</returns>
        public static IDependencyResolver GetDependencyResolver(WebServer webServer, string defaultWebPage = null, object model = null, Type modelType = null, AdoDataConnection connection = null)
        {
            return new WebPageControllerResolver(webServer, defaultWebPage, model, modelType, connection);
        }

        #endregion
    }
}