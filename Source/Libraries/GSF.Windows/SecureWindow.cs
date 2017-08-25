//******************************************************************************************************
//  SecureWindow.cs - Gbtc
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
//  06/17/2010 - Pinal C. Patel
//       Generated original version of source code.
//  08/11/2010 - Pinal C. Patel
//       Made key methods virtual for extensibility.
//  02/16/2011 - J. Ritchie Carroll
//       Added ForceLoginDisplay dependency property to allow application control over pass through
//       authentication via WPF XAML.
//  09/22/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments since much of WPF is not currently available.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#if !MONO
using System;
using System.ComponentModel;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using GSF.Security;

namespace GSF.Windows
{
    #region [ Enumerations ]

    /// <summary>
    /// Defines an enumeration of secure resource accessibility modes.
    /// </summary>
    public enum ResourceAccessiblityMode
    {
        /// <summary>
        /// Resource security is controlled via configuration.
        /// </summary>
        Configurable,
        /// <summary>
        /// Resource is always secured.
        /// </summary>
        AlwaysIncluded,
        /// <summary>
        /// Resource is never secured.
        /// </summary>
        AlwaysExcluded
    }

    #endregion

    /// <summary>
    /// Represents a WPF window secured using role-based security.
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
    ///       <add name="IncludedResources" value="*Window*=*" description="Semicolon delimited list of resources to be secured along with role names."
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
    /// XAML to be used for the WPF window that inherits from <see cref="SecureWindow"/>:
    /// <code>
    /// <![CDATA[
    /// <src:SecureWindow x:Class="SecureWpfApplication.Window1"
    ///     xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
    ///     xmlns:src="clr-namespace:GSF.Windows;assembly=GSF.Windows"
    ///     Title="Window1" Height="300" Width="300">
    ///     <Grid>
    /// 
    ///     </Grid>
    /// </src:SecureWindow>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ISecurityProvider"/>
    public class SecureWindow : Window
    {
        #region [ Members ]

        // Fields
        private bool m_shutdownRequested;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureWindow"/> class.
        /// </summary>
        public SecureWindow()
        {
            this.Initialized += SecureWindow_Initialized;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flags to force login display.
        /// </summary>
        public bool ForceLoginDisplay
        {
            get
            {
                return (bool)GetValue(ForceLoginDisplayProperty);
            }
            set
            {
                SetValue(ForceLoginDisplayProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets window's secure <see cref="ResourceAccessiblityMode"/>.
        /// </summary>
        public ResourceAccessiblityMode ResourceAccessiblity
        {
            get
            {
                return (ResourceAccessiblityMode)GetValue(ResourceAccessiblityProperty);
            }
            set
            {
                SetValue(ResourceAccessiblityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets applicable roles when <see cref="ResourceAccessiblity"/> == <see cref="ResourceAccessiblityMode.AlwaysIncluded"/>.
        /// </summary>
        public string IncludedRoles
        {
            get
            {
                return (string)GetValue(IncludedRolesProperty);
            }
            set
            {
                SetValue(IncludedRolesProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the security principal used to test authentication and role membership.
        /// </summary>
        public SecurityPrincipal SecurityPrincipal
        {
            get
            {
                return (SecurityPrincipal)GetValue(SecurityPrincipalProperty);
            }
            set
            {
                SetValue(SecurityPrincipalProperty, value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the name of resource being accessed.
        /// </summary>
        /// <returns>Name of the resource being accessed.</returns>
        protected virtual string GetResourceName()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return Name;

            return GetType().Name;
        }

        private void SecureWindow_Initialized(object sender, EventArgs e)
        {
            // Don't proceed if the window is opened in design mode
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // Check if the resource is excluded from being secured
            string resource = GetResourceName();

            if (ResourceAccessiblity != ResourceAccessiblityMode.AlwaysIncluded &&
                (ResourceAccessiblity == ResourceAccessiblityMode.AlwaysExcluded ||
                !SecurityProviderUtility.IsResourceSecurable(resource)))
                return;

            try
            {
                // Setup the security provider for role-based security
                ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(WindowsIdentity.GetCurrent().Name);
                securityProvider.PassthroughPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                securityProvider.Authenticate();

                SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                SecurityPrincipal = new SecurityPrincipal(securityIdentity);
            }
            catch (Exception ex)
            {
                ShowSecurityDialog(DisplayType.AccessDenied, "Error loading security provider: " + ex.Message);
                return;
            }

            // Verify that the security principal has been authenticated
            if (!SecurityPrincipal.Identity.IsAuthenticated || ForceLoginDisplay)
            {
                ISecurityProvider securityProvider = SecurityPrincipal.Identity.Provider;

                // See if user's password has expired
                if (securityProvider.UserData.IsDefined && securityProvider.UserData.PasswordChangeDateTime <= DateTime.UtcNow)
                    ShowSecurityDialog(DisplayType.ChangePassword, string.Format("Your password has expired. {0} You must change your password to continue.", securityProvider.AuthenticationFailureReason));
                else
                    ShowSecurityDialog(DisplayType.Login);
            }

            // Perform a top-level permission check on the resource being accessed
            if (!string.IsNullOrEmpty(resource))
            {
                // Stay in a dialog display loop until either access to resource is available or user exits
                while (!m_shutdownRequested && !IsResourceAccessible(resource))
                {
                    // Access to resource is denied
                    ShowSecurityDialog(DisplayType.AccessDenied);
                }
            }
        }

        private bool IsResourceAccessible(string resource)
        {
            if (ResourceAccessiblity == ResourceAccessiblityMode.AlwaysIncluded)
                return SecurityPrincipal.IsInRole(IncludedRoles);

            return SecurityProviderUtility.IsResourceAccessible(resource, SecurityPrincipal);
        }

        private void ShowSecurityDialog(DisplayType displayType, string errorMessage = null)
        {
            SecurityPortal securityDialog = new SecurityPortal(displayType);
            ISecurityProvider securityProvider = SecurityPrincipal.Identity.Provider;

            // Show authentication failure reason if one was defined and user didn't force another message
            if ((object)errorMessage == null && (object)securityProvider != null)
                errorMessage = securityProvider.AuthenticationFailureReason;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                securityDialog.ProviderFailure = true;
                securityDialog.DisplayErrorMessage(errorMessage);
            }

            // Show the WPF security dialog
            if (!securityDialog.ShowDialog().GetValueOrDefault())
            {
                // User chose to cancel security action. If the secure window has no parent,
                // this is root window so exit application, otherwise just close the window
                if ((object)this.Owner == null)
                {
                    m_shutdownRequested = true;
                    Application.Current.Shutdown();
                }
                else
                {
                    this.Close();
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Identifies the <see cref="ForceLoginDisplay"/> dependency property.
        /// </summary>
        /// <returns>identifier for the <see cref="ForceLoginDisplay"/> dependency property.</returns>
        public static readonly DependencyProperty ForceLoginDisplayProperty = DependencyProperty.Register("ForceLoginDisplay", typeof(bool), typeof(SecureWindow), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="ResourceAccessiblity"/> dependency property.
        /// </summary>
        /// <returns>identifier for the <see cref="ResourceAccessiblity"/> dependency property.</returns>
        public static readonly DependencyProperty ResourceAccessiblityProperty = DependencyProperty.Register("ResourceAccessiblity", typeof(ResourceAccessiblityMode), typeof(SecureWindow), new PropertyMetadata(ResourceAccessiblityMode.Configurable));

        /// <summary>
        /// Identifies the <see cref="IncludedRoles"/> dependency property.
        /// </summary>
        /// <returns>identifier for the <see cref="IncludedRoles"/> dependency property.</returns>
        public static readonly DependencyProperty IncludedRolesProperty = DependencyProperty.Register("IncludedRoles", typeof(string), typeof(SecureWindow), new PropertyMetadata("*"));

        /// <summary>
        /// Identifies the <see cref="SecurityPrincipal"/> dependency property.
        /// </summary>
        /// <returns>identifier for the <see cref="SecurityPrincipal"/> dependency property.</returns>
        public static readonly DependencyProperty SecurityPrincipalProperty = DependencyProperty.Register("SecurityPrincipal", typeof(SecurityPrincipal), typeof(SecureWindow), new PropertyMetadata("*"));

        #endregion
    }
}
#endif
