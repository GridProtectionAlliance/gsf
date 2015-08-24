//*******************************************************************************************************
//  SecureWindow.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/17/2010 - Pinal C. Patel
//       Generated original version of source code.
//  08/11/2010 - Pinal C. Patel
//       Made key methods virtual for extensibility.
//  02/16/2011 - J. Ritchie Carroll
//       Added ForceLoginDisplay dependency property to allow application control over pass through
//       authentication via WPF XAML.
//  09/22/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments since much of WPF is not currently available.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using System.ComponentModel;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using TVA.Security;

namespace TVA.Windows
{
#if !MONO
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
    ///     <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ApplicationName" value="" description="Name of the application being secured as defined in the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ConnectionString" value="" description="Connection string to be used for connection to the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ProviderType" value="TVA.Security.LdapSecurityProvider, TVA.Security"
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
    ///     xmlns:src="clr-namespace:TVA.Windows;assembly=TVA.Windows"
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the name of resource being accessed.
        /// </summary>
        /// <returns>Name of the resource being accessed.</returns>
        protected virtual string GetResourceName()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            else
                return this.GetType().Name;
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

            // Setup thread principal to current windows principal.
            if (!(Thread.CurrentPrincipal is WindowsPrincipal) && !(Thread.CurrentPrincipal is TVA.Security.SecurityPrincipal))
                Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            // Setup the security provider for role-based security, if needed
            if (SecurityProviderCache.CurrentProvider == null)
            {
                try
                {
                    SecurityProviderCache.CurrentProvider = SecurityProviderUtility.CreateProvider(string.Empty);
                }
                catch (Exception ex)
                {
                    ShowSecurityDialog(DisplayType.AccessDenied, "Error loading security provider: " + ex.Message);
                }
            }

            ISecurityProvider provider = SecurityProviderCache.CurrentProvider;

            if ((object)provider == null)
            {
                ShowSecurityDialog(DisplayType.AccessDenied, "Cannot authenticate users: Failed to load security provider, please check configuration.");
                return;
            }

            // Verify that the current thread principal has been authenticated
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated || ForceLoginDisplay)
            {
                // See if user's password has expired
                if (provider.UserData.IsDefined && provider.UserData.PasswordChangeDateTime <= DateTime.UtcNow)
                    ShowSecurityDialog(DisplayType.ChangePassword, string.Format("Your password has expired. {0} You must change your password to continue.", provider.AuthenticationFailureReason));
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
                return Thread.CurrentPrincipal.IsInRole(IncludedRoles);

            return SecurityProviderUtility.IsResourceAccessible(resource);
        }

        private void ShowSecurityDialog(DisplayType displayType, string errorMessage = null)
        {
            SecurityPortal securityDialog = new SecurityPortal(displayType);

            // Show authentication failure reason if one was defined and user didn't force another message
            if ((object)errorMessage == null && (object)SecurityProviderCache.CurrentProvider != null)
                errorMessage = SecurityProviderCache.CurrentProvider.AuthenticationFailureReason;

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

        #endregion
    }
#endif
}
