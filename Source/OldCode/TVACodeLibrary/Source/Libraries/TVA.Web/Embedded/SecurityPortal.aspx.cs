//*******************************************************************************************************
//  SecurityPortal.aspx.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
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
//       Fixed a bug that required users to login before password could be reset.
//  07/20/2011 - Pinal C. Patel
//       Added tracing for diagnosing unexpected error conditions.
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

using System;
using System.Security;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVA.Configuration;
using TVA.Security;
using TVA.Web.UI;

namespace TVA.Web.Embedded
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
    ///     <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
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
    public partial class SecurityPortal : System.Web.UI.Page
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
        private const string EmbeddedHelpFile = "TVA.Web.Embedded.Files.Help.pdf";
        private const string EmbeddedHelpImage = "TVA.Web.Embedded.Images.Help.png";
        private const string EmbeddedWarningImage = "TVA.Web.Embedded.Images.Warning.png";
        private const string EmbeddedCompanyLogo = "TVA.Web.Embedded.Images.TVALogo.png";
        private const string EmbeddedStyleSheet = "TVA.Web.Embedded.Styles.SecurityPortal.css";
        private const string DefaultCompanyLink = "http://www.tva.gov";
        private const string DefaultFooterText = "© Tennessee Valley Authority. All rights reserved.";

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
            CategorizedSettingsElement setting = null;

            // Setup company logo.
            setting = settings["CompanyLogo"];
            if (setting != null)
                LogoImage.ImageUrl = setting.Value;
            else
                LogoImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(SecurityPortal), EmbeddedCompanyLogo);

            // Setup company link.
            setting = settings["CompanyLink"];
            if (setting != null)
                LogoLink.NavigateUrl = setting.Value;
            else
                LogoLink.NavigateUrl = DefaultCompanyLink;

            // Setup help link.
            setting = settings["HelpPage"];
            if (setting != null)
                HelpLink.NavigateUrl = setting.Value;
            else
                HelpLink.NavigateUrl = Page.ClientScript.GetWebResourceUrl(typeof(SecurityPortal), EmbeddedHelpFile);

            // Setup footer information.
            setting = settings["FooterText"];
            if (setting != null)
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
                if (ViewState[UsernameKey] == null)
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
            else if (Request[StatusCodeRequestKey] == UnauthorizedStatusCode || SecurityProviderCache.CurrentProvider == null || !User.Identity.IsAuthenticated)
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
                    ISecurityProvider provider = SecurityProviderCache.CurrentProvider;
                    ShowUserData(provider);
                    if (!provider.CanUpdateData)
                    {
                        AccountUserFirstName.Enabled = false;
                        AccountUserLastName.Enabled = false;
                        AccountUserEmailAddress.Enabled = false;
                        AccountUserPhoneNumber.Enabled = false;
                        AccountUserSecurityAnswer.Enabled = false;
                        UpdateButton.Enabled = false;
                    }
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
                ISecurityProvider provider = SecurityProviderUtility.CreateProvider(LoginUsername.Text);
                provider.Initialize();
                if (provider.Authenticate(LoginPassword.Text))
                {
                    // Credentials were authenticated successfully.
                    SecurityProviderCache.CurrentProvider = provider;
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
                    if (provider.UserData.PasswordChangeDateTime != DateTime.MinValue &&
                        provider.UserData.PasswordChangeDateTime <= DateTime.UtcNow)
                    {
                        // User must change password.
                        if (provider.CanChangePassword)
                            Response.Redirect(GetRedirectUrl(PasswordChangeStatusCode), false);
                        else
                            ShowMessage("Account password has expired.", true);
                    }
                    else
                    {
                        // Show why login failed.
                        if (!ShowFailureReason(provider))
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
                System.Diagnostics.Trace.WriteLine(string.Format("Login error: \r\n  {0}", ex.ToString()));
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
                provider = SecurityProviderCache.CurrentProvider;
                if (provider.CanUpdateData)
                {
                    provider.UserData.FirstName = AccountUserFirstName.Text;
                    provider.UserData.LastName = AccountUserLastName.Text;
                    provider.UserData.EmailAddress = AccountUserEmailAddress.Text;
                    provider.UserData.PhoneNumber = AccountUserPhoneNumber.Text;
                    provider.UserData.SecurityAnswer = AccountUserSecurityAnswer.Text;
                    provider.UpdateData();

                    ShowMessage("Information has been updated successfully!", false);
                }
                else
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
                System.Diagnostics.Trace.WriteLine(string.Format("Update information error: \r\n  {0}", ex.ToString()));
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
                ISecurityProvider provider = SecurityProviderUtility.CreateProvider(ChangePasswordUsername.Text);
                provider.Initialize();
                if (provider.CanChangePassword)
                {
                    // Attempt to change password.
                    if (provider.ChangePassword(ChangePasswordOldPassword.Text, ChangePasswordNewPassword.Text))
                    {
                        // Password changed successfully.
                        if (provider.Authenticate(ChangePasswordNewPassword.Text))
                        {
                            // Password authenticated successfully.
                            SecurityProviderCache.CurrentProvider = provider;
                            Response.Redirect(GetReferrerUrl(), false);
                        }
                        else
                        {
                            // Show why authentication failed.
                            if (!ShowFailureReason(provider))
                                ShowMessage("Authentication was not successful.", true);
                        }
                    }
                    else
                    {
                        // Show why password change failed.
                        if (!ShowFailureReason(provider))
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
                System.Diagnostics.Trace.WriteLine(string.Format("Password change error: \r\n  {0}", ex.ToString()));
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
                provider.Initialize();
                if (provider.CanResetPassword)
                {
                    // Proceed to resetting password.
                    if (!string.IsNullOrEmpty(provider.UserData.SecurityQuestion) &&
                        !string.IsNullOrEmpty(provider.UserData.SecurityAnswer))
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
                System.Diagnostics.Trace.WriteLine(string.Format("Password reset error: \r\n  {0}", ex.ToString()));
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
                provider.Initialize();
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
                System.Diagnostics.Trace.WriteLine(string.Format("Password reset error: \r\n  {0}", ex.ToString()));
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
            else
                return Request.Cookies[CookieName][UsernameKey];
        }

        private string GetReferrerUrl()
        {
            if (Request[ReturnUrlRequestKey] != null)
                return Request[ReturnUrlRequestKey];
            else if (Request.UrlReferrer != null)
                return Request.UrlReferrer.AbsolutePath;
            else
                return Request.Url.AbsolutePath;
        }

        private string GetRedirectUrl(string newStatusCode)
        {
            StringBuilder url = new StringBuilder();
            if (string.IsNullOrEmpty(Request.Url.Query))
            {
                url.AppendFormat("{0}?", Request.Url.AbsoluteUri.TrimEnd('/'));
            }
            else
            {
                url.AppendFormat("{0}?", Request.Url.AbsoluteUri.Replace(Request.Url.Query, ""));

                foreach (string pair in Request.Url.Query.TrimStart('?').Split('&'))
                {
                    string[] pairSplit = pair.Split('=');
                    if (string.Compare(pairSplit[0], StatusCodeRequestKey, true) != 0)
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
                // No such account.
                ShowMessage("Account does not exist.", true);
            else if (provider.UserData.IsDisabled)
                // Account is disabled.
                ShowMessage("Account is currently disabled.", true);
            else if (provider.UserData.IsLockedOut)
                // Account is locked.
                ShowMessage("Account is currently locked.", true);
            else
                return false;

            return true;
        }

        #endregion
    }
}
