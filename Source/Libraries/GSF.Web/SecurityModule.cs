//******************************************************************************************************
//  SecurityModule.cs - Gbtc
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
//  03/31/2010 - Pinal C. Patel
//       Generated original version of source code.
//  05/27/2010 - Pinal C. Patel
//       Added usage example to code comments.
//  06/30/2010 - Pinal C. Patel
//       Modified redirection logic to support security of static resources (*.txt, *.pdf, *.exe).
//  07/01/2010 - Pinal C. Patel
//       Modified redirection logic to allow for custom redirection using customErrors settings.
//  08/11/2010 - Pinal C. Patel
//       Made key methods virtual for extensibility.
//  10/14/2010 - Pinal C. Patel
//       Modified GetResourceName() to return path and query for consistency with SecurityPolicy.
//  01/06/2011 - Pinal C. Patel
//       Implemented logic to use RestrictAccessAttribute for access control on the requested resource.
//  06/09/2011 - Pinal C. Patel
//       Added null reference check in IsAccessRestricted() method.
//  07/29/2011 - Pinal C. Patel
//       Made the referral URL passed to SecurityPortal.aspx page relative so redirection work correctly
//       when reverse proxy is involved where requested URL that the web server sees is different than 
//       what the user requested.
//  08/18/2011 - Pinal C. Patel
//       Made Redirect() overridable by deriving classes to control the redirection behavior.
//  08/19/2011 - Pinal C. Patel
//       Changed the type input parameter for Redirect() from int to HttpStatusCode.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.Configuration;
using GSF.Security;
using GSF.Web.Hosting;
using System;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using System.Threading;

namespace GSF.Web
{
    #region [ Enumerations ]

    #endregion

    /// <summary>
    /// Represents an <see cref="IHttpModule">HTTP module</see> that can be used to enable site-wide role-based security.
    /// </summary>
    /// <example>
    /// Required config file entries:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ApplicationName" value="" description="Name of the application being secured as defined in the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ConnectionString" value="" description="Connection string to be used for connection to the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ProviderType" value="GSF.Security.LdapSecurityProvider, GSF.Security"
    ///         description="The type to be used for enforcing security." encrypted="false" />
    ///       <add name="IncludedResources" value="*/*.*=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="*/WebResource.axd*;*/SecurityPortal.aspx*"
    ///         description="Semicolon delimited list of resources to be excluded from being secured."
    ///         encrypted="false" />
    ///       <add name="NotificationSmtpServer" value="localhost" description="SMTP server to be used for sending out email notification messages."
    ///         encrypted="false" />
    ///       <add name="NotificationSenderEmail" value="sender@company.com" description="Email address of the sender of email notification messages." 
    ///         encrypted="false" />
    ///     </securityProvider>
    ///     <activeDirectory>
    ///       <add name="PrivilegedDomain" value="" description="Domain of privileged domain user account."
    ///         encrypted="false" />
    ///       <add name="PrivilegedUserName" value="" description="Username of privileged domain user account."
    ///         encrypted="false" />
    ///       <add name="PrivilegedPassword" value="" description="Password of privileged domain user account."
    ///         encrypted="true" />
    ///     </activeDirectory>
    ///   </categorizedSettings>
    ///   <system.web>
    ///     <authentication mode="Windows"/>
    ///     <httpModules>
    ///       <add name="SecurityModule" type="GSF.Web.SecurityModule, GSF.Web" />
    ///     </httpModules>
    ///   </system.web>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ISecurityProvider"/>
    public class SecurityModule : IHttpModule
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// A handler used to force the SessionStateModule to load session state.
        /// </summary>
        private class SessionEnabledHandler : IHttpHandler, IRequiresSessionState
        {
            public readonly IHttpHandler OriginalHandler;

            /// <summary>
            /// Initializes a new instance of the <see cref="SessionEnabledHandler"/> class.
            /// </summary>
            /// <param name="originalHandler">The original handler object.</param>
            public SessionEnabledHandler(IHttpHandler originalHandler)
            {
                OriginalHandler = originalHandler;
            }

            /// <summary>
            /// This method will never get called.
            /// </summary>
            public void ProcessRequest(HttpContext context)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Returns false since class has a member.
            /// </summary>
            public bool IsReusable
            {
                get { return false; }
            }
        }

        // Fields
        private HttpApplication m_application;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the current <see cref="HttpApplication"/> instance.
        /// </summary>
        protected HttpApplication Application
        {
            get 
            {
                return m_application;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="SecurityModule"/>.
        /// </summary>
        /// <param name="context">An <see cref="HttpApplication"/> object.</param>
        public virtual void Init(HttpApplication context)
        {
            m_application = context;
            m_application.PostMapRequestHandler += Application_PostMapRequestHandler;
            m_application.PostAcquireRequestState += Application_PostAcquireRequestState;
            m_application.PreRequestHandlerExecute += Application_PreRequestHandlerExecute;

            if (!(HostingEnvironment.VirtualPathProvider is EmbeddedResourcePathProvider))
                HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourcePathProvider());
        }

        /// <summary>
        /// Releases the resources used by <see cref="SecurityModule"/>.
        /// </summary>
        public virtual void Dispose()
        {
            m_application.PostMapRequestHandler -= Application_PostMapRequestHandler;
            m_application.PostAcquireRequestState -= Application_PostAcquireRequestState;
            m_application.PreRequestHandlerExecute -= Application_PreRequestHandlerExecute;
        }

        /// <summary>
        /// Redirects the client browser based on the specified <paramref name="httpStatusCode"/>
        /// </summary>
        /// <param name="httpStatusCode"><see cref="HttpStatusCode"/> to be used for the redirect.</param>
        protected virtual void Redirect(HttpStatusCode httpStatusCode)
        {
            int statusCode = (int)httpStatusCode;
            if (m_application.Context.IsCustomErrorEnabled)
            {
                // Defer redirect to customErrors settings in the config file to allow for custom error pages.
                ConfigurationFile configFile = ConfigurationFile.Current;
                CustomErrorsSection customErrors = configFile.Configuration.GetSection("system.web/customErrors") as CustomErrorsSection;
                if (customErrors != null && customErrors.Errors[statusCode.ToString()] != null)
                {
                    // Set status code for the response.
                    m_application.Context.Response.StatusCode = statusCode;
                    // Throw exception for ASP.NET pipeline to takeover processing.
                    throw new HttpException(statusCode, string.Format("Security exception (HTTP status code: {0})", statusCode));
                }
            }

            // Abruptly ending the processing caused by a redirect does not work well when processing static content.
            string redirectUrl = string.Format("~/SecurityPortal.aspx?s={0}&r={1}", statusCode, HttpUtility.UrlEncode(VirtualPathUtility.ToAppRelative(m_application.Request.Url.PathAndQuery)));
            if (m_application.Context.Handler is DefaultHttpHandler)
                // Accessed resource is static.
                m_application.Context.Response.Redirect(redirectUrl, false);
            else
                // Accessed resource is dynamic.
                m_application.Context.Response.Redirect(redirectUrl, true);
        }

        /// <summary>
        /// Gets the name of resource being accessed.
        /// </summary>
        /// <returns><see cref="HttpApplication.Request"/>.<see cref="HttpRequest.Url"/>.<see cref="Uri.PathAndQuery"/> property value.</returns>
        protected virtual string GetResourceName()
        {
            return m_application.Request.Url.PathAndQuery;
        }

        /// <summary>
        /// Determines if access to the requested resource is restricted by <see cref="RestrictAccessAttribute"/>.
        /// </summary>
        /// <returns>true if access to the requested resource is restricted, otherwise false.</returns>
        protected virtual bool IsAccessRestricted()
        {
            // Check for the request handler.
            IHttpHandler handler = m_application.Context.Handler;
            if (handler == null)
                return false;

            // Evaluate access restriction if defined.
            object[] attributes = handler.GetType().GetCustomAttributes(typeof(RestrictAccessAttribute), true);
            if (attributes.Length > 0)
                return !((RestrictAccessAttribute)attributes[0]).CheckAccess();
            else
                return false;
        }

        /// <summary>
        /// Determines if access to the requested <paramref name="resource"/> is to be secured.
        /// </summary>
        /// <param name="resource">Name of the resource being requested.</param>
        /// <returns>True if access to the resource is to be secured, otherwise False.</returns>
        protected virtual bool IsAccessSecured(string resource)
        {
            return SecurityProviderUtility.IsResourceSecurable(GetResourceName());
        }

        private void Application_PostMapRequestHandler(object sender, EventArgs e)
        {
            // Check if access to resource is to be secured.
            if (!IsAccessSecured(GetResourceName()))
                return;

            if (m_application.Context.Handler is IReadOnlySessionState ||
                m_application.Context.Handler is IRequiresSessionState)
                // No need to replace the current handler 
                return;

            // Swap the current handler 
            m_application.Context.Handler = new SessionEnabledHandler(m_application.Context.Handler);
        }

        private void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            // Check if access to resource is to be secured.
            if (!IsAccessSecured(GetResourceName()))
                return;

            SessionEnabledHandler handler = HttpContext.Current.Handler as SessionEnabledHandler;
            if (handler != null)
                // Set the original handler back 
                HttpContext.Current.Handler = handler.OriginalHandler;
        }

        private void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            // Check if access to resource is to be secured.
            string resource = GetResourceName();

            if (!IsAccessSecured(resource))
                return;

            SecurityPrincipal securityPrincipal = Thread.CurrentPrincipal as SecurityPrincipal;

            if ((object)securityPrincipal == null)
            {
                ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(Thread.CurrentPrincipal.Identity.Name);
                securityProvider.PassthroughPrincipal = Thread.CurrentPrincipal;
                securityProvider.Authenticate();

                SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                securityPrincipal = new SecurityPrincipal(securityIdentity);

                Thread.CurrentPrincipal = securityPrincipal;
            }

            if (!m_application.User.Identity.IsAuthenticated)
                // Failed to authenticate user.
                Redirect(HttpStatusCode.Unauthorized);

            if (IsAccessRestricted() || 
                !SecurityProviderUtility.IsResourceAccessible(resource, securityPrincipal))
                // User does not have access to the resource.
                Redirect(HttpStatusCode.Forbidden);
        }

        #endregion
    }
}
