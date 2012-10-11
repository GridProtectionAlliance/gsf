//******************************************************************************************************
//  SecurityPortal.xaml.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/14/2010 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  02/16/2011 - J. Ritchie Carroll
//       Added automatic password expiration handling and modified screen to accomodate default buttons.
//  09/22/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments since much of WPF is not currently available.
//
//******************************************************************************************************

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
 Original Software Title: The GSF Open Source Phasor Data Concentrator
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

using GSF.Configuration;
using GSF.Security;
using System;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GSF.Windows
{
#if !MONO
    #region [ Enumerations ]

    /// <summary>
    /// Enumerable list of portlets to be displayed on the screen.
    /// </summary>
    public enum DisplayType
    {
        /// <summary>
        /// Login screen.
        /// </summary>
        Login,
        /// <summary>
        /// Access Denied screen.
        /// </summary>
        AccessDenied,
        /// <summary>
        /// Change Password screen.
        /// </summary>
        ChangePassword
    }

    #endregion

    /// <summary>
    /// Represents a WPF window used to request user credentials and other security related information.
    /// </summary>
    public partial class SecurityPortal : Window
    {
        #region [ Members ]

        // Fields
        private DisplayType m_displayType;
        private bool m_providerFailure;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new WPF window.
        /// </summary>
        /// <param name="displayType">Type of the message received from security API which is used to decide controls to be displayed on the screen.</param>
        public SecurityPortal(DisplayType displayType)
        {
            InitializeComponent();

            m_displayType = displayType;

            Closed += Window_Closed;
            MouseDown += Window_MouseDown;
            ButtonLogin.Click += ButtonLogin_Click;
            ButtonExit.Click += ButtonExit_Click;
            ButtonOK.Click += ButtonOK_Click;
            ButtonChange.Click += ButtonChange_Click;
            ButtonChangePasswordLink.Click += ButtonChangePasswordLink_Click;
            ButtonForgotPasswordLink.Click += ButtonForgotPasswordLink_Click;
            ButtonLoginLink.Click += ButtonLoginLink_Click;
            TextBoxUserName.TextChanged += TextBox_TextChanged;
            TextBoxPassword.PasswordChanged += PasswordBox_PasswordChanged;
            TextBoxChangePasswordUserName.TextChanged += TextBox_TextChanged;
            TextBoxOldPassword.PasswordChanged += PasswordBox_PasswordChanged;
            TextBoxNewPassword.PasswordChanged += PasswordBox_PasswordChanged;
            TextBoxConfirmPassword.PasswordChanged += PasswordBox_PasswordChanged;
            TextBoxUserName.GotFocus += TextBox_GotFocus;
            TextBoxPassword.GotFocus += TextBox_GotFocus;
            TextBoxChangePasswordUserName.GotFocus += TextBox_GotFocus;
            TextBoxOldPassword.GotFocus += TextBox_GotFocus;
            TextBoxNewPassword.GotFocus += TextBox_GotFocus;
            TextBoxConfirmPassword.GotFocus += TextBox_GotFocus;

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SecurityProviderBase.DefaultSettingsCategory];
            string setting = settings["ApplicationName"].Value;

            if (!string.IsNullOrWhiteSpace(setting))
            {
                TextBlockApplicationLogin.Text = setting + " :: Login";
                TextBlockAccessDenied.Text = setting + " :: Access Denied";
                TextBlockChangePassword.Text = setting + " :: Change Password";
            }

            // Load last user login ID setting
            settings.Add("LastLoginID", Thread.CurrentPrincipal.Identity.Name, "Last user login ID", false, SettingScope.User);
            setting = settings["LastLoginID"].Value;

            if (string.IsNullOrWhiteSpace(setting))
                setting = Thread.CurrentPrincipal.Identity.Name;

            TextBoxUserName.Text = setting;

            // Inititialize screen
            ClearErrorMessage();
            ManageScreenVisualization();

            // Open this window on top of all other windows
            this.Topmost = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that indicates if there was a failure during provider initialization.
        /// </summary>
        public bool ProviderFailure
        {
            get
            {
                return m_providerFailure;
            }
            set
            {
                m_providerFailure = value;
                TextBlockAccessDeniedMessage.Visibility = m_providerFailure ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        // Handles dialog exiting
        private bool ExitSuccess
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(TextBoxUserName.Text))
                {
                    ConfigurationFile.Current.Settings[SecurityProviderBase.DefaultSettingsCategory]["LastLoginID"].Value = TextBoxUserName.Text;
                    ConfigurationFile.Current.Save();
                }

                if (this.DialogResult == null)
                    this.DialogResult = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Displays requested screen section based on received request.
        /// </summary>
        private void ManageScreenVisualization()
        {
            // Initially hide everything on all screens
            LoginSection.Visibility = Visibility.Collapsed;
            AccessDeniedSection.Visibility = Visibility.Collapsed;
            ChangePasswordSection.Visibility = Visibility.Collapsed;

            TextBlockApplicationLogin.Visibility = Visibility.Collapsed;
            TextBlockAccessDenied.Visibility = Visibility.Collapsed;
            TextBlockChangePassword.Visibility = Visibility.Collapsed;

            // Reset all default buttons
            ButtonLogin.IsDefault = false;
            ButtonOK.IsDefault = false;
            ButtonChange.IsDefault = false;

            if (m_displayType == DisplayType.Login)
            {
                TextBoxPassword.Password = "";
                ButtonLogin.IsDefault = true;
                TextBlockApplicationLogin.Visibility = Visibility.Visible;
                LoginSection.Visibility = Visibility.Visible;

                if (string.IsNullOrWhiteSpace(TextBoxUserName.Text))
                    TextBoxUserName.Focus();
                else
                    TextBoxPassword.Focus();
            }
            else if (m_displayType == DisplayType.AccessDenied)
            {
                ButtonOK.IsDefault = true;
                TextBlockAccessDenied.Visibility = Visibility.Visible;
                AccessDeniedSection.Visibility = Visibility.Visible;
            }
            else if (m_displayType == DisplayType.ChangePassword)
            {
                TextBoxOldPassword.Password = "";
                TextBoxNewPassword.Password = "";
                TextBoxConfirmPassword.Password = "";
                ButtonChange.IsDefault = true;
                TextBlockChangePassword.Visibility = Visibility.Visible;
                ChangePasswordSection.Visibility = Visibility.Visible;

                if (string.IsNullOrWhiteSpace(TextBoxChangePasswordUserName.Text))
                    TextBoxChangePasswordUserName.Focus();
                else
                    TextBoxOldPassword.Focus();
            }
        }

        private bool ShowFailureReason(ISecurityProvider provider)
        {
            ClearErrorMessage();

            if (!provider.UserData.IsDefined)
                // No such account.
                DisplayErrorMessage("Account does not exist.");
            else if (provider.UserData.IsDisabled)
                // Account is disabled.
                DisplayErrorMessage("Account is currently disabled.");
            else if (provider.UserData.IsLockedOut)
                // Account is locked.
                DisplayErrorMessage("Account is currently locked.");
            else
                return false;

            return true;
        }

        /// <summary>
        /// Displays error message.
        /// </summary>
        /// <param name="message">Error message to display.</param>
        public void DisplayErrorMessage(string message)
        {
            if ((object)TextBlockGlobalMessage != null)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    TextBlockGlobalMessage.Text = "";
                    TextBlockGlobalMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlockGlobalMessage.Text = message;
                    TextBlockGlobalMessage.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Clears error message.
        /// </summary>
        public void ClearErrorMessage()
        {
            DisplayErrorMessage(null);
        }

        #region [ Event Handlers ]

        /// <summary>
        /// Handles window closed.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            ExitSuccess = false;
        }

        /// <summary>
        /// Handles mouse down event anywhere on the screen so user can click and drag this window around.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        /// <summary>
        /// Logins the user.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize the security provider
                ISecurityProvider provider = SecurityProviderUtility.CreateProvider(TextBoxUserName.Text);

                // Attempt to authenticate user
                if (provider.Authenticate(TextBoxPassword.Password))
                {
                    // Setup security provider for subsequent uses
                    SecurityProviderCache.CurrentProvider = provider;
                    ClearErrorMessage();
                    ExitSuccess = true;
                }
                else
                {
                    // Verify their password hasn't expired
                    if (provider.UserData.IsDefined && provider.UserData.PasswordChangeDateTime <= DateTime.UtcNow)
                    {
                        // Display password expired message
                        DisplayErrorMessage(string.Format("Your password has expired. {0} You must change your password to continue.", provider.AuthenticationFailureReason));
                        m_displayType = DisplayType.ChangePassword;
                        ManageScreenVisualization();
                        TextBoxPassword.Password = "";
                    }
                    else
                    {
                        // Display login failure message
                        DisplayErrorMessage("The username or password is invalid. " + provider.AuthenticationFailureReason);

                        if (string.IsNullOrWhiteSpace(TextBoxUserName.Text))
                            TextBoxUserName.Focus();
                        else
                            TextBoxPassword.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("Login failed: " + ex.Message);

                if (string.IsNullOrWhiteSpace(TextBoxUserName.Text))
                    TextBoxUserName.Focus();
                else
                    TextBoxPassword.Focus();
            }
        }

        /// <summary>
        /// Attempts to change user's password.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if old and new password are different
                if (TextBoxOldPassword.Password == TextBoxNewPassword.Password)
                    throw new Exception("New password cannot be same as old password.");

                // Check is new password and confirm password are same
                if (TextBoxNewPassword.Password != TextBoxConfirmPassword.Password)
                    throw new Exception("New password and confirm password should be same.");

                ISecurityProvider provider = SecurityProviderUtility.CreateProvider(TextBoxChangePasswordUserName.Text);

                if (provider.CanChangePassword)
                {
                    // Attempt to change password
                    if (provider.ChangePassword(TextBoxOldPassword.Password, TextBoxNewPassword.Password) &&
                        provider.Authenticate(TextBoxNewPassword.Password))
                    {
                        // Password changed and authenticated successfully
                        DisplayErrorMessage("Password changed successfully.");

                        // Setup security provider for subsequent uses
                        SecurityProviderCache.CurrentProvider = provider;
                        ClearErrorMessage();
                        ExitSuccess = true;
                    }
                    else
                    {
                        // Show why password change failed
                        if (!ShowFailureReason(provider))
                        {
                            if (!provider.UserData.IsAuthenticated)
                                DisplayErrorMessage("Authentication was not successful.");
                            else
                                DisplayErrorMessage("Password change was not successful.");

                            if (string.IsNullOrWhiteSpace(TextBoxChangePasswordUserName.Text))
                                TextBoxChangePasswordUserName.Focus();
                            else
                                TextBoxOldPassword.Focus();
                        }
                    }
                }
                else
                    DisplayErrorMessage("Account does not support password change.");
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("Change password failed: " + ex.Message);
                TextBoxOldPassword.Focus();
            }
        }

        /// <summary>
        /// Exits window.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            ExitSuccess = false;
        }

        /// <summary>
        /// Closes window.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (m_providerFailure)
            {
                // In case of provider failure, all we can do is exit with a failure code
                ExitSuccess = false;
            }
            else
            {
                // If user chooses, they can try a new set of credentials
                m_displayType = DisplayType.Login;
                ManageScreenVisualization();
            }
        }

        /// <summary>
        /// Displays help for the user on how to reset password.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please contact application administrator to reset your password.", "Forgot Password");
        }

        /// <summary>
        /// Navigates users to Change Password screen.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonChangePasswordLink_Click(object sender, RoutedEventArgs e)
        {
            ClearErrorMessage();
            m_displayType = DisplayType.ChangePassword;
            ManageScreenVisualization();
        }

        /// <summary>
        /// Navigates users back to Login screen.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonLoginLink_Click(object sender, RoutedEventArgs e)
        {
            ClearErrorMessage();
            m_displayType = DisplayType.Login;
            ManageScreenVisualization();
        }

        /// <summary>
        /// Keeps the user names on the separate sections synchronized and clears error messages when text changes.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LoginSection.Visibility == Visibility.Visible && e.Source == TextBoxUserName)
                TextBoxChangePasswordUserName.Text = TextBoxUserName.Text;
            else if (ChangePasswordSection.Visibility == Visibility.Visible && e.Source == TextBoxChangePasswordUserName)
                TextBoxUserName.Text = TextBoxChangePasswordUserName.Text;

            ClearErrorMessage();
        }

        /// <summary>
        /// Clears error messages when text changes.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ClearErrorMessage();
        }

        /// <summary>
        /// We make sure all text boxes "select-all" when they receive focus.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (sender is TextBox)
                    ((TextBox)sender).SelectAll();
                else if (sender is PasswordBox)
                    ((PasswordBox)sender).SelectAll();
            }
        }

        #endregion

        #endregion
    }
#endif
}
