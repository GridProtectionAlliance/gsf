//******************************************************************************************************
//  SecurityPortal.aspx.cs - Gbtc
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
//  05/18/2010 - Pinal C. Patel
//       Generated original version of source code.
//  06/02/2010 - Pinal C. Patel
//       Added sample customization config file entries to code comments.
//       Modified to set the text cursor focus on input fields for ease of use.
//  01/05/2011 - Pinal C. Patel
//       Added HelpPage and FooterText customization settings that can be specified in the config file.
//       Added the ability to change and reset passwords.
//       Updated UI for compatibility across multiple browsers including mobile devices.
//  01/06/2011 - Pinal C. Patel
//       Fixed a issue that required users to login before password could be reset.
//  07/20/2011 - Pinal C. Patel
//       Added tracing for diagnosing unexpected error conditions.
//  10/20/2011 - Pinal C. Patel
//       Updated GetReferrerUrl() and GetRedirectUrl() methods to generate relative URLs instead
//       of absolute ones so redirection would work when reverse proxy is involved.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using GSF.Configuration;
using GSF.Security;
using GSF.Web.UI;

namespace GSF.Web.Embedded
{
    /// <summary>
    /// Embedded web page used by secure ASP.NET web sites for security related tasks.
    /// </summary>
    /// <example>
    /// Config file entries for customizing the page:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityPortal>
    ///       <add name="CompanyLogo" value="~/images/MyCompanyLogo.png" description="Image file of the company's logo." encrypted="false" />
    ///       <add name="CompanyLink" value="http://www.mycompany.com" description="Link to the company's web site." encrypted="false" />
    ///       <add name="HelpPage" value="~/files/Help.pdf" description="Link to the help page." encrypted="false" />
    ///       <add name="FooterText" value="(C) My Company. All rights reserved." description="Text to be displayed in the footer." encrypted="false" />
    ///     </securityPortal>
    ///   </categorizedSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public partial class SecurityPortal : Page
    {
        #region [ Members ]

        // Constants
        private const string UsernameKey = "Username";
        private const string CookieName = "SecurityPortal";
        private const string StaticPageTitle = "Security Portal";
        private const string SettingsCategory = "SecurityPortal";
        private const string StatusCodeRequestKey = "s";
        private const string ReturnUrlRequestKey = "r";
        private const string UnauthorizedStatusCode = "401";
        private const string AccessDeniedStatusCode = "403";
        private const string PasswordResetStatusCode = "401.11";
        private const string PasswordChangeStatusCode = "403.11";
        private const string EmbeddedHelpFile = "GSF.Web.Embedded.Files.Help.pdf";
        private const string EmbeddedHelpImage = "GSF.Web.Embedded.Images.Help.png";
        private const string EmbeddedWarningImage = "GSF.Web.Embedded.Images.Warning.png";
        private const string EmbeddedCompanyLogo = "GSF.Web.Embedded.Images.Logo.png";
        private const string EmbeddedStyleSheet = "GSF.Web.Embedded.Styles.SecurityPortal.css";
        private const string DefaultCompanyLink = "http://www.gridprotectionalliance.org/";
        private const string DefaultFooterText = "© Grid Protection Alliance. All rights reserved.";

        #endregion

        #region [ Properties ]

        private ISecurityProvider CurrentProvider
        {
            get
            {
                return (Thread.CurrentPrincipal as SecurityPrincipal)?.Identity.Provider;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the web page.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            CategorizedSettingsElement setting;

            // Setup company logo.
            setting = settings["CompanyLogo"];

            if ((object)setting != null)
                LogoImage.ImageUrl = setting.Value;
            else
                LogoImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(SecurityPortal), EmbeddedCompanyLogo);

            // Setup company link.
            setting = settings["CompanyLink"];

            if ((object)setting != null)
                LogoLink.NavigateUrl = setting.Value;
            else
                LogoLink.NavigateUrl = DefaultCompanyLink;

            // Setup help link.
            setting = settings["HelpPage"];

            if ((object)setting != null)
                HelpLink.NavigateUrl = setting.Value;
            else
                HelpLink.NavigateUrl = Page.ClientScript.GetWebResourceUrl(typeof(SecurityPortal), EmbeddedHelpFile);

            // Setup footer information.
            setting = settings["FooterText"];

            if ((object)setting != null)
                FooterLabel.Text = setting.Value;
            else
                FooterLabel.Text = DefaultFooterText;

            HelpImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(SecurityPortal), EmbeddedHelpImage);
            WarningImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(SecurityPortal), EmbeddedWarningImage);
            StyleSheet.Attributes["href"] = Page.ClientScript.GetWebResourceUrl(typeof(SecurityPortal), EmbeddedStyleSheet);

            if (Request[StatusCodeRequestKey] == AccessDeniedStatusCode)
            {
                // Show access denied.
                Page.Title = StaticPageTitle + " :: Access Denied";
                AccessDeniedPanel.Visible = true;
                ContentPlaceHolder.Controls.Add(AccessDeniedPanel);
            }
            else if (Request[StatusCodeRequestKey] == PasswordChangeStatusCode)
            {
                // Show change password.
                Page.Title = StaticPageTitle + " :: Change Password";
                ChangePasswordPanel.Visible = true;
                ContentPlaceHolder.Controls.Add(ChangePasswordPanel);

                // Setup UI.
                ChangeButton.SetSubmitOnce();
                ChangePasswordPanel.DefaultButton = ChangeButton.ID;

                if (!Page.IsPostBack)
                {
                    ChangePasswordUsername.Text = GetSavedUsername();

                    if (string.IsNullOrEmpty(ChangePasswordUsername.Text))
                        ChangePasswordUsername.Focus();
                    else
                        ChangePasswordOldPassword.Focus();
                }
            }
            else if (Request[StatusCodeRequestKey] == PasswordResetStatusCode)
            {
                // Show reset password.
                Page.Title = StaticPageTitle + " :: Reset Password";

                if ((object)ViewState[UsernameKey] == null)
                {
                    // Check for reset support.
                    ResetPasswordCheckPanel.Visible = true;
                    ContentPlaceHolder.Controls.Add(ResetPasswordCheckPanel);

                    // Setup UI.
                    ResetCheckButton.SetSubmitOnce();
                    ResetPasswordUsername.Focus();
                    ResetPasswordCheckPanel.DefaultButton = ResetCheckButton.ID;
                }
                else
                {
                    // Perform password reset.
                    ResetPasswordFinalPanel.Visible = true;
                    ContentPlaceHolder.Controls.Add(ResetPasswordFinalPanel);

                    // Setup UI.
                    ResetFinalButton.SetSubmitOnce();
                    ResetPasswordSecurityAnswer.Focus();
                    ResetPasswordFinalPanel.DefaultButton = ResetFinalButton.ID;
                    ResetPasswordSecurityQuestion.Text = ViewState["SecurityQuestion"].ToString();
                    MessageLabel.Text = string.Empty;
                }
            }
            else if (Request[StatusCodeRequestKey] == UnauthorizedStatusCode || (object)CurrentProvider == null || !User.Identity.IsAuthenticated)
            {
                // Show login.
                Page.Title = StaticPageTitle + " :: Login";
                LoginPanel.Visible = true;
                ContentPlaceHolder.Controls.Add(LoginPanel);

                // Setup UI.
                LoginButton.SetSubmitOnce();
                LoginPanel.DefaultButton = LoginButton.ID;
                ForgotPassword.NavigateUrl = GetRedirectUrl(PasswordResetStatusCode);
                ChangePassword.NavigateUrl = GetRedirectUrl(PasswordChangeStatusCode);

                if (!Page.IsPostBack)
                {
                    LoginUsername.Text = GetSavedUsername();

                    if (string.IsNullOrEmpty(LoginUsername.Text))
                    {
                        LoginUsername.Focus();
                    }
                    else
                    {
                        LoginPassword.Focus();
                        RememberUsername.Checked = true;
                    }
                }
            }
            else
            {
                // Show my account.
                Page.Title = StaticPageTitle + " :: My Account";
                MyAccountPanel.Visible = true;
                ContentPlaceHolder.Controls.Clear();
                ContentPlaceHolder.Controls.Add(MyAccountPanel);

                // Setup UI.
                UpdateButton.SetSubmitOnce();
                MyAccountPanel.DefaultButton = UpdateButton.ID;

                if (!Page.IsPostBack)
                {
                    ISecurityProvider provider = CurrentProvider;
                    ShowUserData(provider);

                    // The UpdateData() method was never implemented in
                    // any security provider so it has been removed
                    //if (!provider.CanUpdateData)
                    //{
                    //    AccountUserFirstName.Enabled = false;
                    //    AccountUserLastName.Enabled = false;
                    //    AccountUserEmailAddress.Enabled = false;
                    //    AccountUserPhoneNumber.Enabled = false;
                    //    AccountUserSecurityAnswer.Enabled = false;
                    //    UpdateButton.Enabled = false;
                    //}
                }
            }
        }

        /// <summary>
        /// Logins the user.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        protected void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Initialize the security provider.
                ISecurityProvider securityProvider = SecurityProviderUtility.CreateProvider(LoginUsername.Text);
                securityProvider.Password = LoginPassword.Text;

                if (securityProvider.Authenticate())
                {
                    // Credentials were authenticated successfully.
                    SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                    Thread.CurrentPrincipal = new SecurityPrincipal(securityIdentity);

                    if (RememberUsername.Checked)
                    {
                        Response.Cookies[CookieName][UsernameKey] = LoginUsername.Text;
                        Response.Cookies[CookieName].Expires = DateTime.Now.AddYears(1);
                    }
                    else
                    {
                        Response.Cookies[CookieName][UsernameKey] = string.Empty;
                        Response.Cookies[CookieName].Expires = DateTime.Now.AddYears(-1);
                    }

                    // Redirect to the referring page.
                    Response.Redirect(GetReferrerUrl(), false);
                }
                else
                {
                    // Check why authentication failed.
                    if (securityProvider.UserData.PasswordChangeDateTime != DateTime.MinValue &&
                        securityProvider.UserData.PasswordChangeDateTime <= DateTime.UtcNow)
                    {
                        // User must change password.
                        if (securityProvider.CanChangePassword)
                            Response.Redirect(GetRedirectUrl(PasswordChangeStatusCode), false);
                        else
                            ShowMessage("Account password has expired.", true);
                    }
                    else
                    {
                        // Show why login failed.
                        if (!ShowFailureReason(securityProvider))
                            ShowMessage("Authentication was not successful.", true);
                    }
                }
            }
            catch (SecurityException ex)
            {
                // Show security related error messages.
                ShowMessage(ex.Message.EnsureEnd('.'), true);
            }
            catch (Exception ex)
            {
                // Show ambiguous message for other errors.
                ShowMessage("Login failed due to an unexpected error.", true);
                System.Diagnostics.Trace.WriteLine(string.Format("Login error: \r\n  {0}", ex));
            }
            finally
            {
                LoginPassword.Focus();
            }
        }

        /// <summary>
        /// Updates user data.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            ISecurityProvider provider = null;

            try
            {
                provider = CurrentProvider;

                // The UpdateData() method was never implemented in
                // any security provider so it has been removed
                //if (provider.CanUpdateData)
                //{
                //    provider.UserData.FirstName = AccountUserFirstName.Text;
                //    provider.UserData.LastName = AccountUserLastName.Text;
                //    provider.UserData.EmailAddress = AccountUserEmailAddress.Text;
                //    provider.UserData.PhoneNumber = AccountUserPhoneNumber.Text;
                //    provider.UserData.SecurityAnswer = AccountUserSecurityAnswer.Text;
                //    provider.UpdateData();

                //    ShowMessage("Information has been updated successfully!", false);
                //}
                //else
                {
                    ShowMessage("Account does not support updating of information.", true);
                }
            }
            catch (SecurityException ex)
            {
                // Show security related error messages.
                ShowMessage(ex.Message.EnsureEnd('.'), true);
            }
            catch (Exception ex)
            {
                // Show ambiguous message for other errors.
                ShowMessage("Update failed due to an unexpected error.", true);
                System.Diagnostics.Trace.WriteLine(string.Format("Update information error: \r\n  {0}", ex));
            }
            finally
            {
                ShowUserData(provider);
            }
        }

        /// <summary>
        /// Changes user password.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        protected void ChangeButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Initialize the security provider.
                ISecurityProvider securityProvider = SecurityProviderUtility.CreateProvider(ChangePasswordUsername.Text);

                if (securityProvider.CanChangePassword)
                {
                    // Attempt to change password.
                    if (securityProvider.ChangePassword(ChangePasswordOldPassword.Text, ChangePasswordNewPassword.Text))
                    {
                        securityProvider.Password = ChangePasswordNewPassword.Text;

                        // Password changed successfully.
                        if (securityProvider.Authenticate())
                        {
                            // Password authenticated successfully.
                            SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                            Thread.CurrentPrincipal = new SecurityPrincipal(securityIdentity);
                            Response.Redirect(GetReferrerUrl(), false);
                        }
                        else
                        {
                            // Show why authentication failed.
                            if (!ShowFailureReason(securityProvider))
                                ShowMessage("Authentication was not successful.", true);
                        }
                    }
                    else
                    {
                        // Show why password change failed.
                        if (!ShowFailureReason(securityProvider))
                            ShowMessage("Password change was not successful.", true);
                    }
                }
                else
                {
                    // Changing password is not supported.
                    ShowMessage("Account does not support password change.", true);
                }
            }
            catch (SecurityException ex)
            {
                // Show security related error messages.
                ShowMessage(ex.Message.EnsureEnd('.'), true);
            }
            catch (Exception ex)
            {
                // Show ambiguous message for other errors.
                ShowMessage("Password change failed due to an unexpected error.", true);
                System.Diagnostics.Trace.WriteLine(string.Format("Password change error: \r\n  {0}", ex));
            }
            finally
            {
                ChangePasswordOldPassword.Focus();
            }
        }

        /// <summary>
        /// Checks if user password can be reset.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        protected void ResetCheckButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Initialize the security provider.
                ISecurityProvider provider = SecurityProviderUtility.CreateProvider(ResetPasswordUsername.Text);

                if (provider.CanResetPassword)
                {
                    // Proceed to resetting password.
                    if (!string.IsNullOrEmpty(provider.UserData.SecurityQuestion) && !string.IsNullOrEmpty(provider.UserData.SecurityAnswer))
                    {
                        ViewState.Add(UsernameKey, ResetPasswordUsername.Text);
                        ViewState.Add("SecurityQuestion", provider.UserData.SecurityQuestion);

                        Page.ClientScript.RegisterStartupScript(Page.GetType(), "PostBack", Page.ClientScript.GetPostBackEventReference(Page, null), true);
                    }
                    else
                    {
                        ShowMessage("Security question and answer must be set to reset password.", true);
                    }
                }
                else
                {
                    // Resetting password is not supported.
                    ShowMessage("Account does not support password reset.", true);
                }
            }
            catch (SecurityException ex)
            {
                // Show security related error messages.
                ShowMessage(ex.Message.EnsureEnd('.'), true);
            }
            catch (Exception ex)
            {
                // Show ambiguous message for other errors.
                ShowMessage("Password reset failed due to an unexpected error.", true);
                System.Diagnostics.Trace.WriteLine(string.Format("Password reset error: \r\n  {0}", ex));
            }
            finally
            {
                ResetPasswordUsername.Focus();
            }
        }

        /// <summary>
        /// Resets user password.
        /// </summary>
        /// <param name="sender">Source of this event.</param>
        /// <param name="e">Arguments of this event.</param>
        protected void ResetFinalButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Initialize the security provider.
                ISecurityProvider provider = SecurityProviderUtility.CreateProvider(ResetPasswordUsername.Text);

                if (provider.CanResetPassword)
                {
                    // Attempt to reset password.
                    if (provider.ResetPassword(ResetPasswordSecurityAnswer.Text))
                    {
                        // Password reset was successful.
                        ShowMessage("Password reset request has been processed.", false);
                    }
                    else
                    {
                        // Show why password reset failed.
                        if (!ShowFailureReason(provider))
                            ShowMessage("Password reset was not successful.", true);
                    }
                }
                else
                {
                    // Resetting password is not supported.
                    ShowMessage("Account does not support password reset.", true);
                }
            }
            catch (SecurityException ex)
            {
                // Show security related error messages.
                ShowMessage(ex.Message.EnsureEnd('.'), true);
            }
            catch (Exception ex)
            {
                // Show ambiguous message for other errors.
                ShowMessage("Password reset failed due to an unexpected error.", true);
                System.Diagnostics.Trace.WriteLine(string.Format("Password reset error: \r\n  {0}", ex));
            }
            finally
            {
                ResetPasswordSecurityAnswer.Focus();
            }
        }

        private string GetSavedUsername()
        {
            if (Request.Cookies[CookieName] == null)
                return string.Empty;

            return Request.Cookies[CookieName][UsernameKey];
        }

        private string GetReferrerUrl()
        {
            // Redirect to specified return URL.
            if (Request[ReturnUrlRequestKey] != null)
                return Request[ReturnUrlRequestKey];

            // Redirect to referring URL.
            if (Request.UrlReferrer != null)
                return VirtualPathUtility.ToAppRelative(Request.UrlReferrer.AbsolutePath);

            // Redirect to self.
            return VirtualPathUtility.ToAppRelative(Request.Url.AbsolutePath);
        }

        private string GetRedirectUrl(string newStatusCode)
        {
            StringBuilder url = new StringBuilder();
            url.AppendFormat("{0}?", VirtualPathUtility.ToAppRelative(Request.Url.AbsolutePath));

            if (!string.IsNullOrEmpty(Request.Url.Query))
            {
                // Query parameters are present in the current request.
                foreach (string pair in Request.Url.Query.TrimStart('?').Split('&'))
                {
                    string[] pairSplit = pair.Split('=');

                    if (string.Compare(pairSplit[0], StatusCodeRequestKey, StringComparison.OrdinalIgnoreCase) != 0)
                        url.AppendFormat("{0}={1}&", pairSplit[0], pairSplit[1]);
                }
            }

            if (!string.IsNullOrEmpty(newStatusCode))
                url.AppendFormat("{0}={1}", StatusCodeRequestKey, newStatusCode);

            return url.ToString().TrimEnd('?', '&');
        }

        private void ShowUserData(ISecurityProvider provider)
        {
            AccountUserFirstName.Focus();
            AccountUsername.Text = provider.UserData.Username;
            AccountUserCompany.Text = provider.UserData.CompanyName;
            AccountUserFirstName.Text = provider.UserData.FirstName;
            AccountUserLastName.Text = provider.UserData.LastName;
            AccountUserEmailAddress.Text = provider.UserData.EmailAddress;
            AccountUserPhoneNumber.Text = provider.UserData.PhoneNumber;

            if (string.IsNullOrEmpty(provider.UserData.SecurityQuestion))
            {
                AccountUserSecurityQuestion.Visible = false;
                AccountUserSecurityAnswer.Visible = false;
                AccountUserSecurityAnswerValidator.Visible = false;
            }
            else
            {
                AccountUserSecurityQuestion.Text = provider.UserData.SecurityQuestion;
                AccountUserSecurityAnswer.Text = provider.UserData.SecurityAnswer;
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            MessageLabel.Text = string.Format("{0}<br /><br />", message);

            if (isError)
                MessageLabel.CssClass = "ErrorMessage";
            else
                MessageLabel.CssClass = "InformationMessage";
        }

        private bool ShowFailureReason(ISecurityProvider provider)
        {
            if (!provider.UserData.IsDefined)
                ShowMessage("Account does not exist.", true);           // No such account.
            else if (provider.UserData.IsDisabled)
                ShowMessage("Account is currently disabled.", true);    // Account is disabled.
            else if (provider.UserData.IsLockedOut)
                ShowMessage("Account is currently locked out.", true);  // Account is locked out.
            else
                return false;

            return true;
        }

        #endregion
    }
}
