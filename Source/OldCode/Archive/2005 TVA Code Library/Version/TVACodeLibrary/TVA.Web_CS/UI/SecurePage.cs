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
		
		public class SecurePage : System.Web.UI.Page
		{
			
			
			#region " Member Declaration "
			
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
			/// Initializes a new instance of TVA.Web.UI.SecurePage class.
			/// </summary>
			public SecurePage() : this("")
			{
				
				
			}
			
			/// <summary>
			/// Initializes a new instance of TVA.Web.UI.SecurePage class.
			/// </summary>
			/// <param name="applicationName">Name of the application as in the security database.</param>
			public SecurePage(string applicationName) : this(applicationName, SecurityServer.Development)
			{
				
				
			}
			
			/// <summary>
			/// Initializes a new instance of TVA.Web.UI.SecurePage class.
			/// </summary>
			/// <param name="applicationName">Name of the application as in the security database.</param>
			/// <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
			public SecurePage(string applicationName, SecurityServer securityServer) : this(applicationName, securityServer, AuthenticationMode.AD)
			{
				
				
			}
			
			/// <summary>
			/// Initializes a new instance of TVA.Web.UI.SecurePage class.
			/// </summary>
			/// <param name="applicationName">Name of the application as in the security database.</param>
			/// <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
			/// <param name="authenticationMode">One of the TVA.Security.Application.AuthenticationMode values.</param>
			public SecurePage(string applicationName, SecurityServer securityServer, AuthenticationMode authenticationMode)
			{
				
				m_securityProvider = new WebSecurityProvider();
				m_securityProvider.AccessDenied += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessDenied);
				m_securityProvider.AccessGranted += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessGranted);
				m_securityProvider.Parent = this;
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
			
			private void Page_PreInit(object sender, System.EventArgs e)
			{
				
				// This is the earliest stage in the page life-cycle we can engage the security.
				m_securityProvider.LoginUser();
				
			}
			
			private void Page_Unload(object sender, System.EventArgs e)
			{
				
				// We're done with the security control so we'll set the member variable to Nothing. This will cause
				// all the event handlers to the security control events to be removed. If we don't do this then the
				// the security control will have reference to this page via the event handlers and since it is cached,
				// this page will also be cached - which we don't want to happen.
				m_securityProvider = null;
				m_securityProvider.AccessDenied += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessDenied);
				m_securityProvider.AccessGranted += new System.EventHandler`1[[System.ComponentModel.CancelEventArgs, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]](m_securityProvider_AccessGranted);
				
			}
			
			private void m_securityProvider_AccessDenied(object sender, System.ComponentModel.CancelEventArgs e)
			{
				
				OnLoginUnsuccessful(e);
				
			}
			
			private void m_securityProvider_AccessGranted(object sender, System.ComponentModel.CancelEventArgs e)
			{
				
				OnLoginSuccessful(e);
				
			}
			
			#endregion
			
			#region " Obsolete "
			
			[Obsolete("This constructor will be removed in a future build.")]public SecurePage(string applicationName, SecurityServer securityServer, bool enableCaching) : this(applicationName, securityServer)
			{
				
				
			}
			
			#endregion
			
		}
		
	}
}
