using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.ComponentModel;
//using PCS.Security.Cryptography.Common;
using PCS.Security.Application.Controls;

//*******************************************************************************************************
//  PCS.Security.Application.WebSecurityProvider.vb - Security provider for web applications
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/22/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/22/2006 - Pinal C. Patel
//       Added the flexibility of providing an absolute URL in config file for externally
//       facing web sites.
//  12/28/2006 - Pinal C. Patel
//       Modified the DatabaseException event handler to display the actual exception message instead
//       of the previously displayed message that assumed that database connectivity failed.
//  05/06/2008 - Pinal C. Patel
//       Replaced the user input method from embedded compiled web pages to composite controls that are
//       hosted inside of the web page being secured.
//       Made caching security data to be done here instead of base class and added shared methods
//       SaveToCache() and LoadFromCache() to facilitate this.
//  05/12/2008 - Pinal C. Patel
//       Added capability to allow a secure page to be locked forcefully using query string variable.
//       Added capability to allow logging out a user after certain period of inactivity.
//
//*******************************************************************************************************


namespace PCS.Security
{
    namespace Application
    {

        /// <summary>
        /// Security provider control for web applications.
        /// </summary>
        [ToolboxBitmap(typeof(WebSecurityProvider))]
        public partial class WebSecurityProvider
        {


            #region " Member Declaration "

            private bool m_locked;
            private int m_inactivityTimeout;

            private Page m_parent;

            /// <summary>
            /// ID of the server control used to capture user input.
            /// </summary>
            private const string SecurityControlID = "SecurityProvider";

            #endregion

            #region " Code Scope: Public Code "

            /// <summary>
            /// Key used for accessing security data in the session.
            /// </summary>
            public const string DataKey = "SP.Data";

            /// <summary>
            /// Key used for accessing the username in the session.
            /// </summary>
            public const string UsernameKey = "SP.Username";

            /// <summary>
            /// Key used for accessing the password in the session.
            /// </summary>
            public const string PasswordKey = "SP.Password";

            /// <summary>
            /// Name of the cookie that will contain the current user's credentials.
            /// </summary>
            /// <remarks>
            /// This cookie is used for "single-signon" purposes.
            /// </remarks>
            public const string CredentialCookie = "SP.Credentials";

            /// <summary>
            /// Query string key that can be used for force a secure page in lock-down mode.
            /// </summary>
            /// <remarks>Valid values are "Denied" and "Prompt".</remarks>
            public const string LockModeKey = "LockMode";

            /// <summary>
            /// Gets or sets the Web Page being secured.
            /// </summary>
            /// <value></value>
            /// <returns>Web Page being secured.</returns>
            [Category("Configuration")]
            public Page Parent
            {
                get
                {
                    return m_parent;
                }
                set
                {
                    m_parent = value;
                    m_parent.PreRender += new System.EventHandler(m_parent_PreRender);
                }
            }

            /// <summary>
            /// Gets or sets the time (in minutes) of inactivity after which a user will be automatically logged out.
            /// </summary>
            /// <value></value>
            /// <returns>Time (in minutes) of inactivity after which a user will be automatically logged out.</returns>
            /// <remarks>
            /// This feature can come in very handy for AJAX enabled web pages to shed unnecessary load off the web
            /// server of AJAX requests being made from inactive user sessions. Although basic user activity like
            /// user interaction with the web page is registered, other user activities can be registered manually by
            /// calling the client-side function RegisterActivity().
            /// </remarks>
            [Category("Configuration")]
            public int InactivityTimeout
            {
                get
                {
                    return m_inactivityTimeout;
                }
                set
                {
                    m_inactivityTimeout = value;
                }
            }

            /// <summary>
            /// Logs out the logged in user.
            /// </summary>
            public override void LogoutUser()
            {

                if ((User != null) && (m_parent != null))
                {
                    // Abandon the session so all of the session data is removed upon refresh.
                    m_parent.Session.Abandon();

                    // Delete the session cookie for "single-signon" purposes if one is created.
                    if (m_parent.Request.Cookies[CredentialCookie] != null)
                    {
                        System.Web.HttpCookie cookie = new System.Web.HttpCookie(CredentialCookie);
                        cookie.Expires = System.DateTime.Now.AddDays(-1);
                        m_parent.Response.Cookies.Add(cookie);
                    }

                    m_parent.Response.Redirect(m_parent.Request.Url.PathAndQuery); // Refresh.
                }

            }

            #region " Shared "

            /// <summary>
            /// Saves security data to the session.
            /// </summary>
            /// <param name="page">Page through which session can be accessed.</param>
            /// <param name="data">Security data to be saved in the session.</param>
            /// <returns>True if security data is saved; otherwise False.</returns>
            public static bool SaveToCache(Page page, WebSecurityProvider data)
            {

                if (page.Session[DataKey] == null)
                {
                    // Before caching the security control in the current user's session, we break-off the reference
                    // that the security control has to the page so that the page doesn't get cached unnecessarily.
                    data.Parent = null;
                    page.Session[WebSecurityProvider.DataKey] = data;

                    return true;
                }

            }

            /// <summary>
            /// Loads security data from the session.
            /// </summary>
            /// <param name="page">Page through which session can be accessed.</param>
            /// <returns>Security data if it exists in the session; otherwise Nothing.</returns>
            public static WebSecurityProvider LoadFromCache(Page page)
            {

                WebSecurityProvider data = page.Session[DataKey] as WebSecurityProvider;
                if (data != null)
                {
                    // The security control had been cached previously in the current user's session.
                    data.Parent = page;
                }

                return data;

            }

            #endregion

            #endregion

            #region " Code Scope: Protected Code "

            protected override void ShowLoginPrompt()
            {

                // Lock the page and show the "Login" control.
                LockPage("Login");

            }

            protected override void HandleAccessDenied()
            {

                // Lock the page show and show the "Access Denied" message.
                ControlContainer with_1 = LockPage(string.Empty);
                with_1.MessageText = "<h5>ACCESS DENIED</h5>You are not authorized to view this page.";

            }

            protected override void HandleAccessGranted()
            {

                if (m_parent != null)
                {
                    if (m_inactivityTimeout > 0 && !m_parent.ClientScript.IsClientScriptBlockRegistered("ActivityMonitor"))
                    {
                        // Upon successful login, we'll register client-side script that'll logout the user if no user
                        // activity takes place for the specified inavtivity period.
                        System.Text.StringBuilder with_1 = new System.Text.StringBuilder();
                        with_1.Append("<script type=\"text/javascript\">");
                        with_1.AppendLine();
                        with_1.Append("   var timeoutID;");
                        with_1.AppendLine();
                        // This is the client-side method that will logout the user if inactive.
                        with_1.Append("   function Logout()");
                        with_1.AppendLine();
                        with_1.Append("   {");
                        with_1.AppendLine();
                        with_1.Append("       window.alert(\'Your session has been timed out due to inactivity.\');");
                        with_1.AppendLine();
                        with_1.AppendFormat("       window.location = \'?{0}=Prompt\';", LockModeKey);
                        with_1.AppendLine();
                        with_1.Append("   }");
                        with_1.AppendLine();
                        // This is the client-side method that can be called to register user activity.
                        with_1.Append("   function RegisterActivity()");
                        with_1.AppendLine();
                        with_1.Append("   {");
                        with_1.AppendLine();
                        with_1.AppendFormat("       var timeout = {0};", m_inactivityTimeout);
                        with_1.AppendLine();
                        with_1.Append("       if (timeoutID != null) {window.clearTimeout(timeoutID);}");
                        with_1.AppendLine();
                        with_1.Append("       timeoutID = window.setTimeout(\'Logout()\', timeout * 60 * 1000);");
                        with_1.AppendLine();
                        with_1.Append("   }");
                        with_1.AppendLine();
                        // We handle basic page-level client-side events to register basic user activiry.
                        with_1.Append("   window.onload = RegisterActivity;");
                        with_1.AppendLine();
                        with_1.Append("   window.onblur = RegisterActivity;");
                        with_1.AppendLine();
                        with_1.Append("   window.onfocus = RegisterActivity;");
                        with_1.AppendLine();
                        with_1.Append("</script>");
                        with_1.AppendLine();

                        m_parent.ClientScript.RegisterClientScriptBlock(@PCS.Security.Cryptography.Common.GetType(), "ActivityMonitor", with_1.ToString());
                    }
                }
                else
                {
                    throw (new InvalidOperationException("Parent property is not set."));
                }

            }

            protected override string GetUsername()
            {

                if (m_parent != null)
                {
                    string username = "";
                    try
                    {
                        if (m_parent.Session[UsernameKey] != null)
                        {
                            // Retrieve previously saved username from session.
                            username = m_parent.Session[UsernameKey].ToString();
                        }
                        else if (AuthenticationMode != AuthenticationMode.RSA && (m_parent.Request.Cookies[CredentialCookie] != null))
                        {
                            // Retrieve previously saved username from cookie, but not when RSA security is employed.
                            username = m_parent.Request.Cookies[CredentialCookie][UsernameKey].ToString();
                        }
                    }
                    catch (Exception)
                    {
                        // If we fail to get the username, we'll return an empty string (this way login will fail).
                    }

                    return PCS.Security.Cryptography.Common.Decrypt(username);
                }
                else
                {
                    throw (new InvalidOperationException("Parent must be set in order to retrieve the username."));
                }

            }

            protected override string GetPassword()
            {

                if (m_parent != null)
                {
                    string password = "";
                    try
                    {
                        if (m_parent.Session[UsernameKey] != null)
                        {
                            // Retrieve previously saved password from session.
                            password = m_parent.Session[PasswordKey].ToString();
                        }
                        else if (AuthenticationMode != AuthenticationMode.RSA && (m_parent.Request.Cookies[CredentialCookie] != null))
                        {
                            // Retrieve previously saved password from cookie, but not when RSA security is employed.
                            password = m_parent.Request.Cookies[CredentialCookie][PasswordKey].ToString();
                        }
                    }
                    catch (Exception)
                    {
                        // If we fail to get the username, we'll return an empty string (this way login will fail).
                    }

                    return PCS.Security.Cryptography.Common.Decrypt(password);
                }
                else
                {
                    throw (new InvalidOperationException("Parent must be set in order to retrieve the password."));
                }

            }

            #endregion

            #region " Code Scope: Private Code "

            private ControlContainer LockPage(string activeControl)
            {

                if (m_parent != null)
                {
                    // First we have to find the page's form. We cannot use Page.Form property because it's not set yet.
                    HtmlForm form = null;
                    if (m_parent.Master == null)
                    {
                        // Page doesn't have a Master Page.
                        foreach (Control ctrl in m_parent.Controls)
                        {
                            form = ctrl as HtmlForm;
                            if (form != null)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Page has a Master Page, so the form resides there.
                        foreach (Control ctrl in m_parent.Master.Controls)
                        {
                            form = ctrl as HtmlForm;
                            if (form != null)
                            {
                                break;
                            }
                        }
                    }

                    // Next we check if the page has been locked previously. If so, we don't need to repeat the process,
                    // instead we find the security control and return it.
                    Table controlTable = form.FindControl(SecurityControlID) as Table;
                    if (controlTable == null)
                    {
                        // Page has not been locked yet.
                        ControlContainer control = new ControlContainer(this, activeControl);

                        // Add control to the table.
                        controlTable = ControlContainer.NewTable(1, 1);
                        controlTable.ID = SecurityControlID;
                        controlTable.HorizontalAlign = HorizontalAlign.Center;
                        controlTable.Rows[0].Cells[0].Controls.Add(control);

                        form.Controls.Clear(); // Clear all controls.
                        form.Controls.Add(controlTable); // Add the container control.

                        m_locked = true; // Indicates that page is in lock-down mode.

                        return control;
                    }
                    else
                    {
                        // Page has been locked previously.
                        return ((ControlContainer)(controlTable.Rows[0].Cells[0].Controls[0]));
                    }
                }
                else
                {
                    throw (new InvalidOperationException("Parent property is not set."));
                }

            }

            #region " Event Handlers "

            private void m_parent_PreRender(object sender, System.EventArgs e)
            {

                if (m_locked)
                {
                    // This is the last stop before the page and all of its controls get rendered. It is here that we
                    // make sure that any dynamic controls that got added after we first locked the page are removed
                    // from the page.
                    List<Control> controls = new List<Control>();
                    foreach (Control ctrl in m_parent.Form.Controls)
                    {
                        // Get a local copy of all page controls.
                        controls.Add(ctrl);
                    }
                    foreach (Control ctrl in controls)
                    {
                        // Remove all controls other than the security control.
                        if (ctrl.ID != SecurityControlID)
                        {
                            m_parent.Form.Controls.Remove(ctrl);
                        }
                    }
                }

            }

            private void WebSecurityProvider_BeforeLogin(object sender, System.ComponentModel.CancelEventArgs e)
            {

                if (m_parent != null)
                {
                    // Right before the login process starts, we'll check to see if we have the security data cached in
                    // the session (done when user has access to the application). If so, we'll use the cached data and
                    // save us credential verification and a trip to the database. This is primarily done to improves
                    // performance in scenarios where a secure web page contains many secure user controls.
                    WebSecurityProvider cachedData = WebSecurityProvider.LoadFromCache(m_parent);
                    if (cachedData != null)
                    {
                        // We have cached data.
                        User = cachedData.User; // Here's where we save credential verification and database trip.
                        Server = cachedData.Server;
                        ApplicationName = cachedData.ApplicationName;
                        AuthenticationMode = cachedData.AuthenticationMode;
                    }
                    else
                    {
                        // We don't have cached data, so we'll load settings from config file and continue.
                        LoadSettings();
                    }

                    if (m_parent.Request[LockModeKey] != null)
                    {
                        // User wants to force the page in lock-down mode.
                        if (m_parent.Request[LockModeKey] == "Denied")
                        {
                            // "Access Denied" message is to be displayed.
                            e.Cancel = true;
                            HandleAccessDenied();
                        }
                        else if (m_parent.Request[LockModeKey] == "Prompt")
                        {
                            // Input prompt is to be displayed. This can be used by the user to enter credentials
                            // of a different user to gain access to the site. In order to achieve this, we have to
                            // logout the current user if the user is logged-in already (most likely to be the case).
                            e.Cancel = true;
                            ShowLoginPrompt();
                            if (User != null)
                            {
                                LogoutUser();
                            }
                        }
                    }
                }
                else
                {
                    throw (new InvalidOperationException("Parent property is not set."));
                }

            }

            private void WebSecurityProvider_AfterLogin(object sender, System.EventArgs e)
            {

                if (m_parent != null)
                {
                    if (UserHasApplicationAccess())
                    {
                        // User has access to the application, so we'll cache the security data for subsequent uses.
                        // We cache the data here instead of the AccessGranted phase in order to allow the implementer 
                        // to stop the login process before it completes.
                        WebSecurityProvider.SaveToCache(m_parent, this);
                    }
                }
                else
                {
                    throw (new InvalidOperationException("Parent property is not set."));
                }

            }

            private void WebSecurityProvider_DatabaseException(object sender, GenericEventArgs<System.Exception> e)
            {

                if (m_parent != null)
                {
                    System.Text.StringBuilder with_1 = new StringBuilder();
                    with_1.Append("<html>");
                    with_1.AppendLine();
                    with_1.Append("<head>");
                    with_1.AppendLine();
                    with_1.Append("<Title>Login Aborted</Title>");
                    with_1.AppendLine();
                    with_1.Append("</head>");
                    with_1.AppendLine();
                    with_1.Append("<body>");
                    with_1.AppendLine();
                    with_1.Append("<div style=\"font-family: Tahoma; font-size: 8pt; font-weight: bold; text-align: center;\">");
                    with_1.AppendLine();
                    with_1.Append("<span style=\"font-size: 22pt; color: red;\">");
                    with_1.Append("Login Process Aborted");
                    with_1.Append("</span><br /><br />");
                    with_1.AppendLine();
                    with_1.AppendFormat("[{0}]", e.Argument.Message);
                    with_1.AppendLine();
                    with_1.Append("</div>");
                    with_1.AppendLine();
                    with_1.Append("</body>");
                    with_1.AppendLine();
                    with_1.Append("</html>");

                    m_parent.Response.Clear();
                    m_parent.Response.Write(with_1.ToString());
                    m_parent.Response.End();
                }
                else
                {
                    throw (new InvalidOperationException("Parent property is not set."));
                }

            }

            #endregion

            #endregion

        }

    }
}
