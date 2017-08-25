//******************************************************************************************************
//  SecurityPortal.xaml.cs - Gbtc
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
//  12/14/2010 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  02/16/2011 - J. Ritchie Carroll
//       Added automatic password expiration handling and modified screen to accomodate default buttons.
//  09/22/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments since much of WPF is not currently available.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.Configuration;
using GSF.Identity;
using GSF.Security;

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
            Topmost = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the security principal used for role-based authorization.
        /// </summary>
        public SecurityPrincipal SecurityPrincipal
        {
            get;
            private set;
        }

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

                if (DialogResult == null)
                    DialogResult = value;
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
            else if (provider.UserData.Roles.Count == 0)
                // No roles are assigned
                DisplayErrorMessage("Account has not been assigned any roles and therefore has no rights. Contact your administrator.");
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

        private bool TryImpersonate(string loginID, string password, out WindowsImpersonationContext impersonationContext)
        {
            string domain;
            string username;
            string[] splitLoginID;

            try
            {
                splitLoginID = loginID.Split('\\');

                if (splitLoginID.Length == 2)
                {
                    domain = splitLoginID[0];
                    username = splitLoginID[1];
                    impersonationContext = UserInfo.ImpersonateUser(domain, username, password);

                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }

            impersonationContext = null;

            return false;
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
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        /// <summary>
        /// Logins the user.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            UserInfo userInfo;
            WindowsImpersonationContext impersonationContext = null;
            ISecurityProvider securityProvider;

            try
            {
                // Determine whether we need to try impersonating the user
                userInfo = new UserInfo(TextBoxUserName.Text);

                // If the application is unable to access the domain, possibly because the local user
                // running the application does not have access to domain objects, it's possible that
                // the user logging in does have access to the domain. So we attempt to impersonate the
                // user logging in to allow authentication to proceed
                if (!userInfo.DomainRespondsForUser && TryImpersonate(userInfo.LoginID, TextBoxPassword.Password, out impersonationContext))
                {
                    try
                    {
                        // Working around a known issue - DirectorySearcher will often throw
                        // an exception the first time it is used after impersonating another
                        // user so we get that out of the way here
                        userInfo.Initialize();
                    }
                    catch (InitializationException)
                    {
                        // Exception is expected so we ignore it
                    }
                }

                // Initialize the security provider
                securityProvider = SecurityProviderCache.CreateProvider(TextBoxUserName.Text);
                securityProvider.SecurePassword = TextBoxPassword.SecurePassword;

                // Attempt to authenticate user
                if (securityProvider.Authenticate())
                {
                    // Setup security principal for subsequent uses
                    SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                    SecurityPrincipal = new SecurityPrincipal(securityIdentity);
                    ClearErrorMessage();
                    ExitSuccess = true;
                }
                else
                {
                    // Verify their password hasn't expired
                    if (securityProvider.UserData.IsDefined && securityProvider.UserData.PasswordChangeDateTime <= DateTime.UtcNow)
                    {
                        // Display password expired message
                        DisplayErrorMessage(string.Format("Your password has expired. {0} You must change your password to continue.", securityProvider.AuthenticationFailureReason));
                        m_displayType = DisplayType.ChangePassword;
                        ManageScreenVisualization();
                        TextBoxPassword.Password = "";
                    }
                    else
                    {
                        // Display login failure message
                        DisplayErrorMessage("The username or password is invalid. " + securityProvider.AuthenticationFailureReason);

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
            finally
            {
                if ((object)impersonationContext != null)
                {
                    impersonationContext.Undo();
                    impersonationContext.Dispose();
                }
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

                ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(TextBoxChangePasswordUserName.Text);
                securityProvider.SecurePassword = TextBoxNewPassword.SecurePassword;

                if (securityProvider.CanChangePassword)
                {
                    // Attempt to change password
                    if (securityProvider.ChangePassword(TextBoxOldPassword.Password, TextBoxNewPassword.Password) &&
                        securityProvider.Authenticate())
                    {
                        // Password changed and authenticated successfully
                        DisplayErrorMessage("Password changed successfully.");

                        // Setup security principal for subsequent uses
                        SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                        SecurityPrincipal = new SecurityPrincipal(securityIdentity);
                        ClearErrorMessage();
                        ExitSuccess = true;
                    }
                    else
                    {
                        // Show why password change failed
                        if (!ShowFailureReason(securityProvider))
                        {
                            if (!securityProvider.IsUserAuthenticated)
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
            if (LoginSection.Visibility == Visibility.Visible && ReferenceEquals(e.Source, TextBoxUserName))
                TextBoxChangePasswordUserName.Text = TextBoxUserName.Text;
            else if (ChangePasswordSection.Visibility == Visibility.Visible && ReferenceEquals(e.Source, TextBoxChangePasswordUserName))
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
            if ((object)sender != null)
            {
                TextBox box = sender as TextBox;

                if ((object)box != null)
                {
                    box.SelectAll();
                }
                else
                {
                    PasswordBox passwordBox = sender as PasswordBox;

                    if ((object)passwordBox != null)
                        passwordBox.SelectAll();
                }
            }
        }

        #endregion

        #endregion
    }
#endif
}
