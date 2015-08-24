using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using TVA.Security.Application;

// PCP - 12/14/2006
// PCP - 05/21/2008: Added back a obsolte contructor to maintain backwards compatibility.


namespace TVA.Web
{
	namespace UI
	{
		
		public class SecureUserControl : System.Web.UI.UserControl
		{
			
			
			#region " Member Declaration "
			
			private bool m_loginUnsuccessful;
			
			private WebSecurityProvider m_securityProvider;
			
			#endregion
			
			#region " Event Declaration "
			
			/// <summary>
			/// Occurs when the login process is complete and  the current user has access to the application.
			/// </summary>
			[Description("Occurs when the login process is complete and  the current user has access to the application."), Category("Security")]private EventHandler LoginSuccessfulEvent;
			public event EventHandler LoginSuccessful
			{
				add
				{
					LoginSuccessfulEvent = (EventHandler) System.Delegate.Combine(LoginSuccessfulEvent, value);
				}
				remove
				{
					LoginSuccessfulEvent = (EventHandler) System.Delegate.Remove(LoginSuccessfulEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the login process is complete and the current user does not have access to the application.
			/// </summary>
			[Description("Occurs when the login process is complete and the current user does not have access to the application."), Category("Security")]private EventHandler LoginUnsuccessfulEvent;
			public event EventHandler LoginUnsuccessful
			{
				add
				{
					LoginUnsuccessfulEvent = (EventHandler) System.Delegate.Combine(LoginUnsuccessfulEvent, value);
				}
				remove
				{
					LoginUnsuccessfulEvent = (EventHandler) System.Delegate.Remove(LoginUnsuccessfulEvent, value);
				}
			}
			
			
			#endregion
			
			#region " Code Scope: Public Code "
			
			/// <summary>
			/// Initializes a new instance of TVA.Web.UI.SecureUserControl class.
			/// </summary>
			public SecureUserControl() : this("")
			{
				
				
			}
			
			/// <summary>
			/// Initializes a new instance of TVA.Web.UI.SecureUserControl class.
			/// </summary>
			/// <param name="applicationName">Name of the application as in the security database.</param>
			public SecureUserControl(string applicationName) : this(applicationName, SecurityServer.Development)
			{
				
				
			}
			
			/// <summary>
			/// Initializes a new instance of TVA.Web.UI.SecureUserControl class.
			/// </summary>
			/// <param name="applicationName">Name of the application as in the security database.</param>
			/// <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
			public SecureUserControl(string applicationName, SecurityServer securityServer) : this(applicationName, securityServer, AuthenticationMode.AD)
			{
				
				
			}
			
			/// <summary>
			/// Initializes a new instance of TVA.Web.UI.SecureUserControl class.
			/// </summary>
			/// <param name="applicationName">Name of the application as in the security database.</param>
			/// <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
			/// <param name="authenticationMode">One of the TVA.Security.Application.AuthenticationMode values.</param>
			public SecureUserControl(string applicationName, SecurityServer securityServer, AuthenticationMode authenticationMode)
			{
				
				m_securityProvider = new WebSecurityProvider();
				m_securityProvider.BeforeLoginPrompt += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_BeforeLoginPrompt);
				m_securityProvider.AccessDenied += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessDenied);
				m_securityProvider.AccessGranted += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessGranted);
				m_securityProvider.PersistSettings = true;
				m_securityProvider.ApplicationName = applicationName;
				m_securityProvider.Server = securityServer;
				m_securityProvider.AuthenticationMode = authenticationMode;
				
			}
			
			/// <summary>
			/// Gets the TVA.Security.Application.WebSecurityProvider component that handles the security.
			/// </summary>
			/// <value></value>
			/// <returns>The TVA.Security.Application.WebSecurityProvider component.</returns>
			[Browsable(false)]public WebSecurityProvider SecurityProvider
			{
				get
				{
					return m_securityProvider;
				}
			}
			
			#endregion
			
			#region " Code Scope: Protected Code "
			
			/// <summary>
			/// Raises the TVA.Web.UI.SecureUserControl.LoginSuccessful event.
			/// </summary>
			/// <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
			/// <remarks>
			/// This method is to be called when the login process is complete and  the current user has access to the
			/// application.
			/// </remarks>
			protected void OnLoginSuccessful(CancelEventArgs e)
			{
				
				if (LoginSuccessfulEvent != null)
					LoginSuccessfulEvent(this, e);
				
			}
			
			/// <summary>
			/// Raises the TVA.Web.UI.SecureUserControl.LoginUnsuccessful event.
			/// </summary>
			/// <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
			/// <remarks>
			/// This method is to be called when the login process is complete and the current user does not have
			/// access to the application.
			/// </remarks>
			protected void OnLoginUnsuccessful(CancelEventArgs e)
			{
				
				if (LoginUnsuccessfulEvent != null)
					LoginUnsuccessfulEvent(this, e);
				
			}
			
			#endregion
			
			#region " Code Scope: Private Code "
			
			private void Page_Init(object sender, System.EventArgs e)
			{
				
				// This is the earliest stage in the control life-cycle we can engage the security.
				m_securityProvider.Parent = this.Page;
				m_securityProvider.LoginUser();
				
			}
			
			private void Page_Unload(object sender, System.EventArgs e)
			{
				
				// We're done with the security control so we'll set the member variable to Nothing. This will cause
				// all the event handlers to the security control events to be removed. If we don't do this then the
				// the security control will have reference to this control's page via the event handlers and since
				// it is cached, the page will also be cached - which we don't want to happen.
				m_securityProvider = null;
				m_securityProvider.BeforeLoginPrompt += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_BeforeLoginPrompt);
				m_securityProvider.AccessDenied += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessDenied);
				m_securityProvider.AccessGranted += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessGranted);
				
			}
			
			private void Page_PreRender(object sender, System.EventArgs e)
			{
				
				if (m_loginUnsuccessful)
				{
					// It has been determined that this secure control is not to be made visible.
					this.Visible = false;
				}
				
			}
			
			private void m_securityProvider_BeforeLoginPrompt(object sender, System.ComponentModel.CancelEventArgs e)
			{
				
				// This will only happen when:
				// 1) This secure control is being used inside of an unsecure page AND
				// 2) This control is secured using RSA authentication or AD authentication but we don't the login
				//    ID of the current user.
				// Since, we need user credentials for authentication, we'll need to lock-down (remove all page controls)
				// to show the login prompt. But we cannot do that because it is prohibited during the page's DataBind,
				// Init, Load, PreRender and Unload phases (remember that LoginUser() was invoked from the PageInit event,
				// so we're still in that event). So, what we'll do instead is hide this secure control.
				e.Cancel = true;
				m_loginUnsuccessful = true;
				
				// NOTE TO SELF: If an implementer wants to use a secure control inside of an unsecure page, and still
				// keep the secure control visible, they'll have to cancel the login process through an advanced
				// implementation of security at the control level.
				
			}
			
			private void m_securityProvider_AccessDenied(object sender, System.ComponentModel.CancelEventArgs e)
			{
				
				// This will only happen when:
				// 1) This secure control is being used inside of an unsecure page AND
				// 2) This control is secured using AD authentication AND
				// 3) We have the login ID of the current user AND
				// 4) Current user does not have access to the application to which this secure control belongs.
				// So, what we're going to do is instead of locking down the entire page because the user does not have
				// access to the application, we just hide this secure control (done during PreRender phase). But, even
				// if we wanted to lock-down the page (remove all page control), we couldn't because this is prohibited
				// during the page's DataBind, Init, Load, PreRender and Unload phases (remember that LoginUser() was
				// invoked from the PageInit event, so we're still in that event).
				e.Cancel = true;
				m_loginUnsuccessful = true;
				
				OnLoginUnsuccessful(e);
				
			}
			
			private void m_securityProvider_AccessGranted(object sender, System.ComponentModel.CancelEventArgs e)
			{
				
				OnLoginSuccessful(e);
				
			}
			
			#endregion
			
			#region " Obsolete "
			
			[Obsolete("This constructor will be removed in a future build.")]public SecureUserControl(string applicationName, SecurityServer securityServer, bool enableCaching) : this(applicationName, securityServer)
			{
				
				
			}
			
			#endregion
			
		}
		
	}
}
