//******************************************************************************************************
//  SecureForm.cs - Gbtc
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
//  05/26/2010 - Pinal C. Patel
//       Generated original version of source code.
//  06/02/2010 - Pinal C. Patel
//       Added authentication check in Form.Load event.
//  06/09/2010 - Pinal C. Patel
//       Added design-time check in Form.Load event to skip authentication when in design mode.
//  08/11/2010 - Pinal C. Patel
//       Made key methods virtual for extensibility.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using GSF.Security;

namespace GSF.Windows.Forms
{
    /// <summary>
    /// Represents a windows form secured using role-based security.
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
    ///       <add name="IncludedResources" value="*Form*=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="" description="Semicolon delimited list of resources to be excluded from being secured."
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
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ISecurityProvider"/>
    public partial class SecureForm : Form
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureForm"/> class.
        /// </summary>
        public SecureForm()
        {
            // Initialize form components.
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets current security principal for secure form.
        /// </summary>
        public SecurityPrincipal SecurityPrincipal
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets exception handler for secure form.
        /// </summary>
        public Action<Exception> ExceptionHandler
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the name of resource being accessed.
        /// </summary>
        /// <returns>Name of the resource being accessed.</returns>
        protected virtual string GetResourceName()
        {
            return Name;
        }

        private void SecureForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Don't proceed if the form is opened in design mode
                if (DesignMode)
                    return;

                // Check if the resource is excluded from being secured
                string resource = GetResourceName();

                if (!SecurityProviderUtility.IsResourceSecurable(resource))
                    return;

                // Set up security provider for passthrough authentication
                ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(WindowsIdentity.GetCurrent().Name);
                securityProvider.PassthroughPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                securityProvider.Authenticate();

                // Setup the security principal for role-based security
                SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                SecurityPrincipal = new SecurityPrincipal(securityIdentity);

                // Verify that the current thread principal has been authenticated
                if (!SecurityPrincipal.Identity.IsAuthenticated)
                    throw new SecurityException($"Authentication failed for user '{SecurityPrincipal.Identity.Name}'");

                // Perform a top-level permission check on the resource being accessed
                if (!SecurityProviderUtility.IsResourceAccessible(resource, SecurityPrincipal))
                    throw new SecurityException($"Access to '{resource}' is denied");

                // Set up the current thread principal
                // NOTE: Provided for backwards compatibility;
                //       recommended to use the SecurityPrincipal instead
                Thread.CurrentPrincipal = SecurityPrincipal;
            }
            catch (Exception ex)
            {
                if (ExceptionHandler is null)
                    throw;

                ExceptionHandler(ex);
            }
        }

        #endregion
    }
}
