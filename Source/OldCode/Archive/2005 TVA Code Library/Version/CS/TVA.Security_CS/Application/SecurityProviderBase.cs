using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.ComponentModel;
using System.Data.SqlClient;
using TVA.Configuration;

//*******************************************************************************************************
//  TVA.Security.Application.SecurityProviderBase.vb - Base class for application security provider
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/22/2006 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  12/28/2007 - Pinal C. Patel
//       Modified the ConnectionString property to use the backup SQL Server database in case if any of
//       the primary databases are unavailable or offline.
//       Renamed the DbConnectionException event to DatabaseException as this event is raised in the
//       event of any SQL Server exception that is encountered.
//  05/06/2008 - Pinal C. Patel
//       Added AuthenticationMode property and modified LoginUser() method to add support for RSA
//       authentication.
//       Implemented TVA.Configuration.IPersistSettings interface to allow for the property values to
//       saved and retrieved from the config file.
//       Moved security database connection strings to the User class - better fit.
//  05/13/2008 - Pinal C. Patel
//       Added overload for LoginUser() that allows the user's username and password to be passed in.
//
//*******************************************************************************************************


namespace TVA.Security
{
	namespace Application
	{
		
		/// <summary>
		/// Base class of the security provider control.
		/// </summary>
		[ProvideProperty("ValidRoles", typeof(object)), ProvideProperty("ValidRoleAction", typeof(object))]public abstract partial class SecurityProviderBase : IExtenderProvider, ISupportInitialize, TVA.Configuration.IPersistSettings
		{
			
			
			#region " Member Declaration "
			
			private User m_user;
			private SecurityServer m_server;
			private string m_applicationName;
			private AuthenticationMode m_authenticationMode;
			private bool m_persistSettings;
			private string m_settingsCategory;
			private Hashtable m_extendeeControls;
			
			#endregion
			
			#region " Event Declaration "
			
			/// <summary>
			/// Occurs before the login process is started.
			/// </summary>
			[Description("Occurs before the login process is started.")]public delegate void BeforeLoginEventHandler(object Of);
			private BeforeLoginEventHandler BeforeLoginEvent;
			
			public event BeforeLoginEventHandler BeforeLogin
			{
				add
				{
					BeforeLoginEvent = (BeforeLoginEventHandler) System.Delegate.Combine(BeforeLoginEvent, value);
				}
				remove
				{
					BeforeLoginEvent = (BeforeLoginEventHandler) System.Delegate.Remove(BeforeLoginEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs after the login process is complete.
			/// </summary>
			[Description("Occurs after the login process is complete.")]private EventHandler AfterLoginEvent;
			public event EventHandler AfterLogin
			{
				add
				{
					AfterLoginEvent = (EventHandler) System.Delegate.Combine(AfterLoginEvent, value);
				}
				remove
				{
					AfterLoginEvent = (EventHandler) System.Delegate.Remove(AfterLoginEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs before the login prompt is shown.
			/// </summary>
			[Description("Occurs before the login prompt is shown.")]public delegate void BeforeLoginPromptEventHandler(object Of);
			private BeforeLoginPromptEventHandler BeforeLoginPromptEvent;
			
			public event BeforeLoginPromptEventHandler BeforeLoginPrompt
			{
				add
				{
					BeforeLoginPromptEvent = (BeforeLoginPromptEventHandler) System.Delegate.Combine(BeforeLoginPromptEvent, value);
				}
				remove
				{
					BeforeLoginPromptEvent = (BeforeLoginPromptEventHandler) System.Delegate.Remove(BeforeLoginPromptEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs after the login prompt has been shown.
			/// </summary>
			[Description("Occurs after the login prompt has been shown.")]private EventHandler AfterLoginPromptEvent;
			public event EventHandler AfterLoginPrompt
			{
				add
				{
					AfterLoginPromptEvent = (EventHandler) System.Delegate.Combine(AfterLoginPromptEvent, value);
				}
				remove
				{
					AfterLoginPromptEvent = (EventHandler) System.Delegate.Remove(AfterLoginPromptEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs before user data is initialized.
			/// </summary>
			[Description("Occurs before user data is initialized.")]private EventHandler BeforeInitializeDataEvent;
			public event EventHandler BeforeInitializeData
			{
				add
				{
					BeforeInitializeDataEvent = (EventHandler) System.Delegate.Combine(BeforeInitializeDataEvent, value);
				}
				remove
				{
					BeforeInitializeDataEvent = (EventHandler) System.Delegate.Remove(BeforeInitializeDataEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs after user data has been initialized.
			/// </summary>
			[Description("Occurs after user data has been initialized.")]private EventHandler AfterInitializeDataEvent;
			public event EventHandler AfterInitializeData
			{
				add
				{
					AfterInitializeDataEvent = (EventHandler) System.Delegate.Combine(AfterInitializeDataEvent, value);
				}
				remove
				{
					AfterInitializeDataEvent = (EventHandler) System.Delegate.Remove(AfterInitializeDataEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs before user is authenticated for application access.
			/// </summary>
			[Description("Occurs before user is authenticated for application access.")]public delegate void BeforeAuthenticateEventHandler(object Of);
			private BeforeAuthenticateEventHandler BeforeAuthenticateEvent;
			
			public event BeforeAuthenticateEventHandler BeforeAuthenticate
			{
				add
				{
					BeforeAuthenticateEvent = (BeforeAuthenticateEventHandler) System.Delegate.Combine(BeforeAuthenticateEvent, value);
				}
				remove
				{
					BeforeAuthenticateEvent = (BeforeAuthenticateEventHandler) System.Delegate.Remove(BeforeAuthenticateEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs after user has been authenticated for application access.
			/// </summary>
			[Description("Occurs after user has been authenticated for application access.")]private EventHandler AfterAuthenticateEvent;
			public event EventHandler AfterAuthenticate
			{
				add
				{
					AfterAuthenticateEvent = (EventHandler) System.Delegate.Combine(AfterAuthenticateEvent, value);
				}
				remove
				{
					AfterAuthenticateEvent = (EventHandler) System.Delegate.Remove(AfterAuthenticateEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when user has access to the application.
			/// </summary>
			[Description("Occurs when user has access to the application.")]public delegate void AccessGrantedEventHandler(object Of);
			private AccessGrantedEventHandler AccessGrantedEvent;
			
			public event AccessGrantedEventHandler AccessGranted
			{
				add
				{
					AccessGrantedEvent = (AccessGrantedEventHandler) System.Delegate.Combine(AccessGrantedEvent, value);
				}
				remove
				{
					AccessGrantedEvent = (AccessGrantedEventHandler) System.Delegate.Remove(AccessGrantedEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when user does not have access to the application.
			/// </summary>
			[Description("Occurs when user does not have access to the application.")]public delegate void AccessDeniedEventHandler(object Of);
			private AccessDeniedEventHandler AccessDeniedEvent;
			
			public event AccessDeniedEventHandler AccessDenied
			{
				add
				{
					AccessDeniedEvent = (AccessDeniedEventHandler) System.Delegate.Combine(AccessDeniedEvent, value);
				}
				remove
				{
					AccessDeniedEvent = (AccessDeniedEventHandler) System.Delegate.Remove(AccessDeniedEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when a database exception is encountered during the login process.
			/// </summary>
			[Description("Occurs when a database exception is encountered during the login process.")]public delegate void DatabaseExceptionEventHandler(object Of);
			private DatabaseExceptionEventHandler DatabaseExceptionEvent;
			
			public event DatabaseExceptionEventHandler DatabaseException
			{
				add
				{
					DatabaseExceptionEvent = (DatabaseExceptionEventHandler) System.Delegate.Combine(DatabaseExceptionEvent, value);
				}
				remove
				{
					DatabaseExceptionEvent = (DatabaseExceptionEventHandler) System.Delegate.Remove(DatabaseExceptionEvent, value);
				}
			}
			
			
			#endregion
			
			#region " Code Scope: Public Code "
			
			/// <summary>
			/// Gets or sets the security database server against which users are authenticated.
			/// </summary>
			/// <value></value>
			/// <returns>One of the TVA.Security.Application.SecurityServer values.</returns>
			[Category("Configuration")]public SecurityServer Server
			{
				get
				{
					return m_server;
				}
				set
				{
					m_server = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the application name as defined in the security database that is being secured.
			/// </summary>
			/// <value></value>
			/// <returns>Name of the application as defined in the security database that is being secured.</returns>
			[Category("Configuration")]public string ApplicationName
			{
				get
				{
					return m_applicationName;
				}
				set
				{
					m_applicationName = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the mode of authentication to be used for authenticating users of the application.
			/// </summary>
			/// <value></value>
			/// <returns>One of the TVA.Security.Application.AuthenticationMode values.</returns>
			[Category("Configuration")]public AuthenticationMode AuthenticationMode
			{
				get
				{
					return m_authenticationMode;
				}
				set
				{
					m_authenticationMode = value;
				}
			}
			
			/// <summary>
			/// Gets data about the current user.
			/// </summary>
			/// <value></value>
			/// <returns>Instance of TVA.Security.Application.User class.</returns>
			[Browsable(false)]public User User
			{
				get
				{
					return m_user;
				}
				protected set
				{
					// We'll only allow derived class to update this data.
					m_user = value;
				}
			}
			
			/// <summary>
			/// Determines whether or not the current user has access to the application.
			/// </summary>
			/// <returns>True if the current user has access to the application; otherwise False.</returns>
			public bool UserHasApplicationAccess()
			{
				
				return ((m_user != null)&& m_user.IsDefined && ! m_user.IsLockedOut && m_user.IsAuthenticated && (m_user.FindApplication(m_applicationName) != null));
				
			}
			
			/// <summary>
			/// Logs in the current user.
			/// </summary>
			public void LoginUser()
			{
				
				LoginUser(string.Empty, string.Empty);
				
			}
			
			/// <summary>
			/// Logs in the specified user.
			/// </summary>
			/// <param name="username">Username of the user to login.</param>
			/// <param name="password">Password of the user to login.</param>
			public void LoginUser(string username, string password)
			{
				
				if (! string.IsNullOrEmpty(m_applicationName))
				{
					CancelEventArgs beforeLoginEventData = new CancelEventArgs();
					if (BeforeLoginEvent != null)
						BeforeLoginEvent(this, beforeLoginEventData);
					if (beforeLoginEventData.Cancel)
					{
						return;
					}
					
					if (m_user == null)
					{
						// We don't have data about the user, so we must get it.
						if (string.IsNullOrEmpty(username))
						{
							username = GetUsername(); // Get username from inheriting class if it has.
						}
						if (string.IsNullOrEmpty(password))
						{
							password = GetPassword(); // Get password from inheriting class if it has.
						}
						
						switch (m_authenticationMode)
						{
							case Security.Application.AuthenticationMode.AD:
								// This will get us the login ID of the current user. This will be null in case of web
								// application if:
								// 1) Secured web page is being accessed from outside.
								// 2) Secured web page is being accessed from inside, but
								//    "Integrated Windows Authentication" is turned off for the web site.
								// Note: In case of a windows app, we'll always get the login ID of the current user.
								string userLoginID = System.Threading.Thread.CurrentPrincipal.Identity.Name;
								
								// The order of conditional execution is important for the following scenarios to work:
								// o Internal user wants to access a secure page for which he/she does not have access,
								//   but has the credentials of a user who has access to this page and want to use the
								//   credentials in order to access the secure web page.
								// o Developer of an externally facing web site ("Anonymous access" is on) wants to test
								//   the security without turning-off "Integrated Windows Authentication" for the web
								//   site, as doing so will disable the debugging capabilities from the Visual Studio IDE.
								// Note: Both of the scenarios above require that the person trying do access the secured
								//       web page with someone else's credentials does not have access to the web page.
								if (! string.IsNullOrEmpty(username) && ! string.IsNullOrEmpty(password))
								{
									// We have the username and password provided to us by the derived class. Since the
									// username and password have been captured and verified by the derived class, we
									// will not authenticate these credentials again.
									InitializeUser(username, password, false);
								}
								else if (! string.IsNullOrEmpty(userLoginID))
								{
									// We don't have the username and password from the derived class, but we have the
									// login ID of the current user. Since no authentication has been performed yet, we
									// will authenticate the login ID just to comfirm.
									InitializeUser(userLoginID.Split('\\')[1], string.Empty, true);
								}
								else
								{
									// We don't have any option other than prompting for credentials.
									if (! CaptureCredentials())
									{
										return; // Implementer wants to abort.
									}
								}
								break;
							case Security.Application.AuthenticationMode.RSA:
								// In the case of RSA authentication mode, we must always prompt the user for the
								// credentials when the user accesses a secure application for the first time.
								if (! string.IsNullOrEmpty(username) && ! string.IsNullOrEmpty(password))
								{
									// Derived class has captured user credentials and authenticated them successfully.
									InitializeUser(username, password, false);
								}
								else
								{
									// User is accessing the secure application for the first time, so the derived class
									// must capture user credentials by prompting them for it and authenticate them.
									if (! CaptureCredentials())
									{
										return; // Implementer wants to abort.
									}
								}
								break;
						}
					}
					
					if (m_user != null)
					{
						CancelEventArgs beforeAuthenticateEventData = new CancelEventArgs();
						if (BeforeAuthenticateEvent != null)
							BeforeAuthenticateEvent(this, beforeAuthenticateEventData);
						if (beforeAuthenticateEventData.Cancel)
						{
							return;
						}
						
						if (UserHasApplicationAccess())
						{
							// User has been authenticated successfully and has access to the specified application.
							CancelEventArgs accessGrantedEventData = new CancelEventArgs();
							if (AccessGrantedEvent != null)
								AccessGrantedEvent(this, accessGrantedEventData);
							if (accessGrantedEventData.Cancel)
							{
								return;
							}
							
							ProcessControls();
							HandleAccessGranted();
						}
						else
						{
							// User could not be autheticated or doesn't have access to the specified application.
							// Most likely user authentication will never fail because if the user is external, the
							// login page will verify the user's password before this process kicks in.
							CancelEventArgs accessDeniedEventData = new CancelEventArgs();
							if (AccessDeniedEvent != null)
								AccessDeniedEvent(this, accessDeniedEventData);
							if (accessDeniedEventData.Cancel)
							{
								return;
							}
							
							HandleAccessDenied();
						}
						
						if (AfterAuthenticateEvent != null)
							AfterAuthenticateEvent(this, EventArgs.Empty);
					}
					
					if (AfterLoginEvent != null)
						AfterLoginEvent(this, EventArgs.Empty);
				}
				else
				{
					throw (new InvalidOperationException("ApplicationName must be set in order to login the user."));
				}
				
			}
			
			/// <summary>
			/// Logs out the logged in user.
			/// </summary>
			public abstract void LogoutUser();
			
			#region " Interface Implementation "
			
			#region " IExtenderProvider "
			
			public bool CanExtend(object extendee)
			{
				
				return (extendee is System.Web.UI.Control|| extendee is System.Windows.Forms.Control);
				
			}
			
			public string GetValidRoles(object extendee)
			{
				
				return GetProperties(extendee).ValidRoles;
				
			}
			
			public void SetValidRoles(object extendee, string value)
			{
				
				ControlProperties extendedProperties = GetProperties(extendee);
				extendedProperties.ValidRoles = value;
				
				ProcessControl(extendee, extendedProperties);
				
			}
			
			public ValidRoleAction GetValidRoleAction(object extendee)
			{
				
				return GetProperties(extendee).ValidRoleAction;
				
			}
			
			public void SetValidRoleAction(object extendee, ValidRoleAction value)
			{
				
				ControlProperties extendedProperties = GetProperties(extendee);
				extendedProperties.ValidRoleAction = value;
				
				ProcessControl(extendee, extendedProperties);
				
			}
			
			private ControlProperties GetProperties(object extendee)
			{
				
				ControlProperties properties = (ControlProperties) (m_extendeeControls[extendee]);
				if (properties == null)
				{
					properties = new ControlProperties();
					m_extendeeControls.Add(extendee, properties);
				}
				
				return properties;
				
			}
			
			private class ControlProperties
			{
				
				
				public string ValidRoles;
				public ValidRoleAction ValidRoleAction;
				public bool ActionTaken;
				
			}
			
			#endregion
			
			#region " ISupportInitialize "
			
			/// <summary>
			/// To be called before the control is initialized.
			/// </summary>
			public void BeginInit()
			{
				
				// Nothing needs to be done when the component begins initializing.
				
			}
			
			/// <summary>
			/// To be called after the control is initialized.
			/// </summary>
			/// <remarks>Loads property values from the config file and performs the login operation.</remarks>
			public void EndInit()
			{
				
				if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
				{
					LoadSettings();
					LoginUser();
				}
				
			}
			
			#endregion
			
			#region " IPersistSettings "
			
			/// <summary>
			/// Gets or sets a boolean value indicating whether or not property values are to be saved in the config file.
			/// </summary>
			/// <value></value>
			/// <returns>True if property values are to be saved in the config file; otherwise False.</returns>
			[Category("Persistance")]public bool PersistSettings
			{
				get
				{
					return m_persistSettings;
				}
				set
				{
					m_persistSettings = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the category name under which the property values are to be saved in the config file if
			/// they are to be saved in the config file.
			/// </summary>
			/// <value></value>
			/// <returns>Category name under which the property values are to be saved in the config file.</returns>
			[Category("Persistance")]public string SettingsCategory
			{
				get
				{
					return m_settingsCategory;
				}
				set
				{
					if (! string.IsNullOrEmpty(value))
					{
						m_settingsCategory = value;
					}
					else
					{
						throw (new ArgumentNullException("SettingsCategoryName"));
					}
				}
			}
			
			/// <summary>
			/// Loads property values from the config file.
			/// </summary>
			public virtual void LoadSettings()
			{
				
				if (m_persistSettings)
				{
					try
					{
						TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategory);
						Server = with_1.Item("Server", true).GetTypedValue(m_server);
						ApplicationName = with_1.Item("ApplicationName", true).GetTypedValue(m_applicationName);
						AuthenticationMode = with_1.Item("AuthenticationMode", true).GetTypedValue(m_authenticationMode);
					}
					catch (Exception)
					{
						// Absorb any encountered exception.
					}
				}
				
			}
			
			/// <summary>
			/// Saves property values to the config file.
			/// </summary>
			public virtual void SaveSettings()
			{
				
				if (m_persistSettings)
				{
					try
					{
						TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategory);
						with_1.Clear();
						object with_2 = with_1.Item("Server", true);
						with_2.Value = m_server.ToString();
						with_2.Description = "Security database server (Development; Acceptance; Production) against which users are authenticated.";
						object with_3 = with_1.Item("ApplicationName", true);
						with_3.Value = m_applicationName;
						with_3.Description = "Name of the application as defined in the security database that is being secured.";
						object with_4 = with_1.Item("AuthenticationMode", true);
						with_4.Value = m_authenticationMode.ToString();
						with_4.Description = "Mode of authentication (AD; RSA) to be used for authenticating users of the application.";
						TVA.Configuration.Common.SaveSettings();
					}
					catch (Exception)
					{
						// Absorb any encountered exception.
					}
				}
				
			}
			
			#endregion
			
			#endregion
			
			#endregion
			
			#region " Code Scope: Protected Code "
			
			/// <summary>
			/// Shows a login prompt where user can enter his/her credentials.
			/// </summary>
			/// <remarks></remarks>
			protected abstract void ShowLoginPrompt();
			
			/// <summary>
			/// Performs any necessary actions that must be performed upon unsuccessful login.
			/// </summary>
			protected abstract void HandleAccessDenied();
			
			/// <summary>
			/// Performs any necessary actions that must be performed upon successful login.
			/// </summary>
			protected abstract void HandleAccessGranted();
			
			/// <summary>
			/// Gets the name that the user provided on the login screen.
			/// </summary>
			/// <returns></returns>
			protected abstract string GetUsername();
			
			/// <summary>
			/// Gets the password that the user provided on the login screen.
			/// </summary>
			/// <returns></returns>
			protected abstract string GetPassword();
			
			#endregion
			
			#region " Code Scope: Private Code "
			
			private void ProcessControls()
			{
				
				foreach (object extendee in m_extendeeControls.Keys)
				{
					ProcessControl(extendee, ((ControlProperties) (m_extendeeControls[extendee])));
				}
				
			}
			
			private void ProcessControl(object extendee, ControlProperties extendedProperties)
			{
				
				if (! extendedProperties.ActionTaken && extendedProperties.ValidRoleAction != ValidRoleAction.None && (extendedProperties.ValidRoles != null))
				{
					PropertyInfo controlProperty = extendee.GetType().GetProperty(extendedProperties.ValidRoleAction.ToString());
					
					if ((m_user != null)&& (controlProperty != null))
					{
						// User has been logged in and the control property exists.
						controlProperty.SetValue(extendee, false, null); // By default we'll set the property to False.
						
						foreach (string role in extendedProperties.ValidRoles.Replace(" ", "").Replace(",", ";").Split(';'))
						{
							if (m_user.FindRole(role, m_applicationName) != null)
							{
								// We'll set the property to True if the current user belongs either one of the valid roles.
								controlProperty.SetValue(extendee, true, null);
								break;
							}
						}
					}
				}
				
			}
			
			private void InitializeUser(string username, string password, bool authenticate)
			{
				
				try
				{
					if (BeforeInitializeDataEvent != null)
						BeforeInitializeDataEvent(this, EventArgs.Empty);
					
					m_user = new User(username, password, m_applicationName, m_server, m_authenticationMode, authenticate);
					
					if (AfterInitializeDataEvent != null)
						AfterInitializeDataEvent(this, EventArgs.Empty);
					
					m_user.LogAccess(! UserHasApplicationAccess()); // Log access attempt to security database.
				}
				catch (SqlException ex)
				{
					// We'll notifying about the excountered SQL exception by rasing an event.
					if (DatabaseExceptionEvent != null)
						DatabaseExceptionEvent(this, new GenericEventArgs<Exception>(ex));
				}
				catch (Exception)
				{
					// We'll bubble-up all other encountered exceptions.
					throw;
				}
				
			}
			
			private bool CaptureCredentials()
			{
				
				CancelEventArgs beforeLoginPromptEventData = new CancelEventArgs();
				if (BeforeLoginPromptEvent != null)
					BeforeLoginPromptEvent(this, beforeLoginPromptEventData);
				if (beforeLoginPromptEventData.Cancel)
				{
					return false;
				}
				
				ShowLoginPrompt(); // Prompt user for credentials.
				
				if (AfterLoginPromptEvent != null)
					AfterLoginPromptEvent(this, EventArgs.Empty);
				
				return true; // Indicate that credentials are/will be captured.
				
			}
			
			#endregion
			
		}
		
	}
}
