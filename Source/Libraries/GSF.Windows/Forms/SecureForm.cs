//******************************************************************************************************
//  SecureForm.cs - Gbtc
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
        #region [ Methods ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureForm"/> class.
        /// </summary>
        public SecureForm()
        {
            // Initialize form components.
            InitializeComponent();
        }

        /// <summary>
        /// Gets the name of resource being accessed.
        /// </summary>
        /// <returns>Name of the resource being accessed.</returns>
        protected virtual string GetResourceName()
        {
            return this.Name;
        }

        private void SecureForm_Load(object sender, EventArgs e)
        {
            // Don't proceed if the form is opened in design mode.
            if (DesignMode)
                return;

            // Check if the resource is excluded from being secured.
            string resource = GetResourceName();
            if (!SecurityProviderUtility.IsResourceSecurable(resource))
                return;

            // Setup thread principal to current windows principal.
            if (!(Thread.CurrentPrincipal is WindowsPrincipal))
                Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            // Setup the security provider for role-based security.
            if ((object)SecurityProviderCache.CurrentProvider == null)
                SecurityProviderCache.CurrentProvider = SecurityProviderUtility.CreateProvider(string.Empty);

            // Verify that the current thread principal has been authenticated.
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                throw new SecurityException(string.Format("Authentication failed for user '{0}'", Thread.CurrentPrincipal.Identity.Name));

            // Perform a top-level permission check on the resource being accessed.
            if (!SecurityProviderUtility.IsResourceAccessible(resource))
                throw new SecurityException(string.Format("Access to '{0}' is denied", resource));
        }

        #endregion
    }
}
