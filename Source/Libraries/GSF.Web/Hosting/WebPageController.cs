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
using GSF.Web.Model;
using GSF.Web.Security;
using Owin;

namespace GSF.Web.Hosting
{
    /// <summary>
    /// Defines a mini-web server with Razor support using the self-hosted API controller.
    /// </summary>
    public class WebPageController : ApiController
    {
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
            WebServer = webServer ?? WebServer.Default;
        }

        #endregion

        #region [ Properties ]

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
        /// Gets the <see cref="Hosting.WebServer"/> instance used by this <see cref="WebPageController"/>.
        /// </summary>
        public WebServer WebServer { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Common page request handler.
        /// </summary>
        /// <param name="pageName">Page name to render.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        /// <returns>Rendered page result for given page.</returns>
        public Task<HttpResponseMessage> GetPage(string pageName, CancellationToken cancellationToken)
        {
            return WebServer.RenderResponse(Request, pageName, cancellationToken, Model, ModelType);
        }

        /// <summary>
        /// Common page post handler.
        /// </summary>
        /// <param name="pageName">Page name to render.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        /// <returns>Rendered page result for given page.</returns>
        /// <remarks>
        /// For Ajax post-based requests to pages handled by this controller, the Ajax request will need to specify
        /// verification token in the header and set flag that indicates an Ajax request. See example code below:
        /// <code>
        /// @{ 
        ///    // Setup AJAX based anti-forgery implementation
        ///    string verificationHeader = AuthenticationOptions.DefaultRequestVerificationToken;
        ///    string useAjaxVerfication = AuthenticationOptions.DefaultAjaxRequestVerificationToken;
        ///    ReadonlyAuthenticationOptions options = ViewBag.AuthenticationOptions;
        ///
        ///    if (options != null) {
        ///        if (!string.IsNullOrWhiteSpace(options.RequestVerificationToken)) {
        ///            verificationHeader = options.RequestVerificationToken;
        ///        }
        ///
        ///        if (!string.IsNullOrWhiteSpace(options.AjaxRequestVerificationToken)) {
        ///            useAjaxVerfication = options.AjaxRequestVerificationToken;
        ///        }
        ///    }
        ///
        ///    string verificationValue = Html.RequestVerificationHeaderToken();
        ///
        ///    string constants = string.Format(@"
        ///        const verificationHeader = ""{0}"";
        ///        const verificationValue = ""{1}"";
        ///        const useAjaxVerfication = ""{2}"";
        ///    ",
        ///        /* 0 */ verificationHeader.JavaScriptEncode(),
        ///        /* 1 */ verificationValue.JavaScriptEncode(),
        ///        /* 2 */ useAjaxVerfication.JavaScriptEncode()
        ///    );
        ///}
        ///@section Scripts {
        ///    <script>
        ///        "use strict";
        ///
        ///        @Raw(new Minifier().MinifyJavaScript(constants));
        ///
        ///        function doAjaxThing() {
        ///            $.ajax({
        ///                cache: false,
        ///                url: "AjaxThing.ashx",
        ///                method: "post",
        ///                data: { value: "some data to post" },
        ///                dataType: "application/json",
        ///                success: function (result) {
        ///                   console.log("Success: " + result);
        ///                },
        ///                beforeSend: function (xhr) {
        ///                    xhr.setRequestHeader(verificationHeader, verificationValue);
        ///                    xhr.setRequestHeader(useAjaxVerfication, "true");
        ///                }
        ///            });
        ///        }
        ///    </script>
        ///}
        /// </code>
        /// </remarks>
        [HttpPost]
        [ActionName("GetPage")]
        [ValidateRequestVerificationToken(FormValidation = true)]
        [SuppressMessage("Security", "SG0016", Justification = "CSRF vulnerability handled via ValidateRequestVerificationToken.")]
        public Task<HttpResponseMessage> PostPage(string pageName, CancellationToken cancellationToken)
        {
            return GetPage(pageName, cancellationToken);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        #endregion
    }

    /// <summary>
    /// Configures the <see cref="WebPageController"/> when adding it to the Owin pipeline.
    /// </summary>
    public interface IWebPageControllerBuilder
    {
        /// <summary>
        /// Use the given page as the default page to display on the default path.
        /// </summary>
        /// <param name="defaultWebPage">The default page to display on the default path.</param>
        /// <returns>The web page controller builder.</returns>
        IWebPageControllerBuilder UseDefaultWebPage(string defaultWebPage);

        /// <summary>
        /// Use the given model as the model to use when rendering Razor templates.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="model"/>.</typeparam>
        /// <param name="model">Reference to model to use when rendering Razor templates.</param>
        /// <returns>The web page controller builder.</returns>
        IWebPageControllerBuilder UseModel<T>(T model);

        /// <summary>
        /// Use the given authentication options for enabling sessions.
        /// </summary>
        /// <param name="authenticationOptions">Authentication options for enabling sessions.</param>
        /// <returns>The web page controller builder.</returns>
        IWebPageControllerBuilder UseAuthenticationOptions(AuthenticationOptions authenticationOptions);

        /// <summary>
        /// Use the given function to configure custom routes for selecting pages.
        /// </summary>
        /// <param name="routeConfig">Function that configures the custom routes.</param>
        /// <returns>The web page controller builder.</returns>
        /// <remarks>
        /// The routes should set the controller, action, and pageName as in the following example.
        ///
        /// <code>
        /// routes.MapHttpRoute(
        ///     name: "MyCustomRoute",
        ///     routeTemplate: "my/custom/route",
        ///     defaults: new
        ///     {
        ///         controller = "WebPage",
        ///         action = "GetPage",
        ///         pageName = "Index.html"
        ///     }
        /// );
        /// </code>
        /// </remarks>
        IWebPageControllerBuilder UseCustomRoutes(Action<HttpRouteCollection> routeConfig);
    }

    /// <summary>
    /// Defines extension function for registering <see cref="WebPageController"/> in web server pipeline.
    /// </summary>
    public static class WebPageControllerAppBuilderExtensions
    {
        // Web page controller dependency resolver
        private sealed class WebPageControllerResolver : IDependencyResolver
        {
            private readonly WebServer m_webServer;
            private readonly object m_model;
            private readonly Type m_modelType;

            public WebPageControllerResolver(WebServer webServer, object model, Type modelType)
            {
                m_webServer = webServer;
                m_model = model;
                m_modelType = modelType;
            }

            void IDisposable.Dispose()
            {
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(WebPageController))
                    return new WebPageController(m_webServer)
                    {
                        Model = m_model,
                        ModelType = m_modelType
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
                            Model = m_model,
                            ModelType = m_modelType
                        }
                    };

                return new List<object>();
            }

            public IDependencyScope BeginScope() => this;
        }

        // Configures the WebPageController when adding it to the Owin pipeline.
        private sealed class WebPageControllerBuilder : IWebPageControllerBuilder
        {
            public string DefaultWebPage { get; set; } = "Index.html";
            public object Model { get; set; }
            public Type ModelType { get; set; }
            public AuthenticationOptions AuthenticationOptions { get; set; }
            public Action<HttpRouteCollection> RouteConfig { get; set; } = DefaultRouteConfig;

            public IWebPageControllerBuilder UseDefaultWebPage(string defaultWebPage)
            {
                DefaultWebPage = defaultWebPage;
                return this;
            }

            public IWebPageControllerBuilder UseModel<T>(T model) =>
                UseModel(model, typeof(T));

            public IWebPageControllerBuilder UseModel(object model, Type modelType)
            {
                Model = model;
                ModelType = modelType;
                return this;
            }

            public IWebPageControllerBuilder UseAuthenticationOptions(AuthenticationOptions authenticationOptions)
            {
                AuthenticationOptions = authenticationOptions;
                return this;
            }

            public IWebPageControllerBuilder UseCustomRoutes(Action<HttpRouteCollection> routeConfig)
            {
                RouteConfig = routeConfig ?? DefaultRouteConfig;
                return this;
            }

            private static void DefaultRouteConfig(HttpRouteCollection _) { }
        }

        /// <summary>
        /// Registers web page controller in web server pipeline.
        /// </summary>
        /// <param name="app">The app builder for the web server pipeline.</param>
        /// <param name="webServer"><see cref="WebServer"/> instance to use for controller.</param>
        /// <param name="webPageControllerConfig">Function used to configure the web page controller.</param>
        public static void UseWebPageController(this IAppBuilder app, WebServer webServer, Action<IWebPageControllerBuilder> webPageControllerConfig)
        {
            WebPageControllerBuilder builder = new WebPageControllerBuilder();
            webPageControllerConfig(builder);

            HttpConfiguration httpConfig = new HttpConfiguration();
            httpConfig.DependencyResolver = GetDependencyResolver(webServer, builder.Model, builder.ModelType);

            AuthenticationOptions options = builder.AuthenticationOptions;

            if (options is not null)
                httpConfig.EnableSessions(options);

            builder.RouteConfig(httpConfig.Routes);

            httpConfig.Routes.MapHttpRoute(
                name: "WebPage",
                routeTemplate: "{*pageName}",
                defaults: new
                {
                    controller = "WebPage",
                    action = "GetPage",
                    pageName = builder.DefaultWebPage
                }
            );

            app.UseWebApi(httpConfig);

            httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            // Check for configuration issues before first request
            httpConfig.EnsureInitialized();
        }

        /// <summary>
        /// Registers web page controller in web server pipeline.
        /// </summary>
        /// <param name="app">The app builder for the web server pipeline.</param>
        /// <param name="webServer"><see cref="WebServer"/> instance to use for controller.</param>
        /// <param name="defaultWebPage">The default page to display on the default path.</param>
        /// <param name="model">Reference to model to use when rendering Razor templates, if any.</param>
        /// <param name="modelType">Type of <paramref name="model"/>, if any.</param>
        /// <param name="options">Authentication options for enabling sessions.</param>
        public static void UseWebPageController(this IAppBuilder app, WebServer webServer, string defaultWebPage = "Index.html", object model = null, Type modelType = null, AuthenticationOptions options = null)
        {
            void Configure(WebPageControllerBuilder builder) => builder
                .UseModel(model, modelType)
                .UseDefaultWebPage(defaultWebPage)
                .UseAuthenticationOptions(options);

            app.UseWebPageController(webServer, builder => Configure((WebPageControllerBuilder)builder));
        }

        /// <summary>
        /// Gets a dependency resolver.
        /// </summary>
        /// <param name="webServer"><see cref="WebServer"/> instance to use for controller.</param>
        /// <param name="model">Reference to model to use when rendering Razor templates, if any.</param>
        /// <param name="modelType">Type of <paramref name="model"/>, if any.</param>
        /// <returns>Dependency resolver for the specified parameters.</returns>
        private static IDependencyResolver GetDependencyResolver(WebServer webServer, object model = null, Type modelType = null)
        {
            return new WebPageControllerResolver(webServer, model, modelType);
        }
    }
}