using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
//using TVA.Common;
using TVA.Identity;
//using TVA.Data.Common;
//using TVA.Math.Common;
using TVA.Security.Radius;
//using TVA.Security.Cryptography.Common;

//*******************************************************************************************************
//  TVA.Security.Application.User.vb - User defined in the security database
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
//  09/26/2006 - Pinal C. Patel
//       Original version of source code generated.
//  10/22/2007 - Pinal C. Patel
//       Added GeneratePassword() shared function and enforced strong password rule in EncryptPassword().
//  05/06/2008 - Pinal C. Patel
//       Modified contructors so security database connection string is not to be passed in.
//       Added methods: RefreshData, Authenticate, ChangePassword, LogAccess and LogError.
//       Added support to authenticate user credentials against RSA server using RADIUS.
//
//*******************************************************************************************************


namespace TVA.Security
{
	namespace Application
	{
		
		/// <summary>
		/// Represents a user defined in the security database.
		/// </summary>
		[Serializable()]public class User
		{
			
			
			#region " Member Declaration "
			
			private string m_username;
			private string m_password;
			private string m_firstName;
			private string m_lastName;
			private string m_companyName;
			private string m_phoneNumber;
			private string m_emailAddress;
			private string m_securityQuestion;
			private string m_securityAnswer;
			private bool m_isExternal;
			private bool m_isLockedOut;
			private System.DateTime m_passwordChangeDateTime;
			private System.DateTime m_accountCreatedDateTime;
			private bool m_isDefined;
			private bool m_isAuthenticated;
			private List<Group> m_groups;
			private List<Role> m_roles;
			private List<Application> m_applications;
			private string m_applicationName;
			private SecurityServer m_securityServer;
			private AuthenticationMode m_authenticationMode;
			
			private const int MinimumPasswordLength = 8;
			private const string StrongPasswordRegex = "^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$";
			
			/// <summary>
			/// Connection string for the development security database.
			/// </summary>
			private const string DevConnectionString = "KACpAHIAWwDRAFIAGADyAEMAZwBsAEsAcAAQAFIAbwBNAE0ANwD9AFkAIwBUAG0AOQCSAEgAMADgAFAAWwB6AGAAOwBeAE0AMwAiAG0APQA4ADwAAwCPADMARwBqAFQAYABiADIASgBHAEoADgAFAG0AHQABAF8AawBrACQADAD6AC8ACQAGAFsAVQA=";
			
			/// <summary>
			/// Connection string for the acceptance security database.
			/// </summary>
			private const string AcpConnectionString = "KACpAHIAWwDRAFIAGADlAFcAZwBuAFwAcgANAFkAEQAjAE0AFwDdAGQABgBwAFAAGADuAF0AewCwAHgAUwBtAGAALQBWAFEAOABMAEkALgA9ACIAAwCYACsACAAjAG4ARwB1AGoASABCAEgAFAAUAHcAHQABAFoAdQBrACQAXAC4AGwAVwAbAEEAFAC3AH8AeQBwADkACwDrACkADAB0AEEAVgA=";
			
			/// <summary>
			/// Connection string for the production security database.
			/// </summary>
			private const string PrdConnectionString = "KACpAHIAWwDRAFIAGAD0AFYAZwBgAF8AZgATAF8AVAAkAEYAGADaAGQACAB7AFgADgDuAFoAFADCAGkAcQBNAFQAHQByABkAfQA1AGkAKgAsACwACwCIAC8AQQALAHEAWQBKAGYASABWAE4AFAAPAGAAdQBEAGwASQBdAHAASQCxACcABAArAGsAawCqAE4AXgBEAHcAXwC6ACEAAQBcAG8AaAAUAB8AaQCTADIASABTAD0A";
			
			/// <summary>
			/// Connection string for the backup security database.
			/// </summary>
			private const string BakConnectionString = "KACpAHIAWwDRAFIAGADlAFcAZwBgAF8AcAAQAFIAZQBWACkAEgDoAEwAIABXAH8ALwDKADQAAQDgAEwAXgBwAGIALgBDAEsAMgAfAFsAOwAuADsAGACSAD4ABQBxACEAfABvAEsAFgBWAEoADQATAGsARQAaAC8AbAB4AF0APQD5AC4AFwBTAFoAVgDtAA==";
			
			/// <summary>
			/// Primary RSA server used for RSA authentication.
			/// </summary>
			private const string RsaServer1 = "tro-rsa-1";
			
			/// <summary>
			/// Backup RSA server used for RSA authentication.
			/// </summary>
			private const string RsaServer2 = "tro-rsa-2";
			
			/// <summary>
			/// Shared secret for RSA authentication using the RADIUS protocol.
			/// </summary>
			private const string RadiusSharedSecret = "LACNADkAWQDAAFEAFgDMAGoASQBDAHgAWwAiAHwAPwA=";
			
			#endregion
			
			#region " Code Scope: Public "
			
			/// <summary>
			/// Creates an instance of a user defined in the security database.
			/// </summary>
			/// <param name="username">Username of the user.</param>
			/// <param name="password">Password of the user.</param>
			public User(string username, string password) : this(username, password, string.Empty)
			{
				
				
			}
			
			/// <summary>
			/// Creates an instance of a user defined in the security database.
			/// </summary>
			/// <param name="username">Username of the user.</param>
			/// <param name="password">Password of the user.</param>
			/// <param name="applicationName">Name of the application for which user data is to be retrieved.</param>
			public User(string username, string password, string applicationName) : this(username, password, applicationName, SecurityServer.Development, AuthenticationMode.AD)
			{
				
				
			}
			
			/// <summary>
			/// Creates an instance of a user defined in the security database.
			/// </summary>
			/// <param name="username">Username of the user.</param>
			/// <param name="password">Password of the user.</param>
			/// <param name="securityServer">Security server from which user data is to be retrieved.</param>
			public User(string username, string password, SecurityServer securityServer) : this(username, password, string.Empty, securityServer, AuthenticationMode.AD)
			{
				
				
			}
			
			/// <summary>
			/// Creates an instance of a user defined in the security database.
			/// </summary>
			/// <param name="username">Username of the user.</param>
			/// <param name="password">Password of the user.</param>
			/// <param name="applicationName">Name of the application for which user data is to be retrieved.</param>
			/// <param name="securityServer">Security server from which user data is to be retrieved.</param>
			/// <param name="authenticationMode">Mode of authentication to be used for authenticating credentials.</param>
			public User(string username, string password, string applicationName, SecurityServer securityServer, AuthenticationMode authenticationMode) : this(username, password, applicationName, securityServer, authenticationMode, true)
			{
				
				
			}
			
			/// <summary>
			/// Gets the user's username.
			/// </summary>
			/// <value></value>
			/// <returns>Username of the user.</returns>
			public string Username
			{
				get
				{
					return m_username;
				}
			}
			
			/// <summary>
			/// Gets the user's password.
			/// </summary>
			/// <value></value>
			/// <returns>Password of the user.</returns>
			public string Password
			{
				get
				{
					return m_password;
				}
			}
			
			/// <summary>
			/// Gets the user's first name.
			/// </summary>
			/// <value></value>
			/// <returns>First name of the user.</returns>
			public string FirstName
			{
				get
				{
					return m_firstName;
				}
			}
			
			/// <summary>
			/// Gets the user's last name.
			/// </summary>
			/// <value></value>
			/// <returns>Last name of the user.</returns>
			public string LastName
			{
				get
				{
					return m_lastName;
				}
			}
			
			/// <summary>
			/// Gets the user's company name.
			/// </summary>
			/// <value></value>
			/// <returns>Company name of the user.</returns>
			public string CompanyName
			{
				get
				{
					return m_companyName;
				}
			}
			
			/// <summary>
			/// Gets the user's phone number.
			/// </summary>
			/// <value></value>
			/// <returns>Phone number of the user.</returns>
			public string PhoneNumber
			{
				get
				{
					return m_phoneNumber;
				}
			}
			
			/// <summary>
			/// Gets the user's email address.
			/// </summary>
			/// <value></value>
			/// <returns>Email address of the user.</returns>
			public string EmailAddress
			{
				get
				{
					return m_emailAddress;
				}
			}
			
			/// <summary>
			/// Gets the user's security question.
			/// </summary>
			/// <value></value>
			/// <returns>Security question of the user.</returns>
			public string SecurityQuestion
			{
				get
				{
					return m_securityQuestion;
				}
			}
			
			/// <summary>
			/// Gets the user's security answer.
			/// </summary>
			/// <value></value>
			/// <returns>Security answer of the user.</returns>
			public string SecurityAnswer
			{
				get
				{
					return m_securityAnswer;
				}
			}
			
			/// <summary>
			/// Gets a boolean value indicating whether or not the user is defined as an external user in the security
			/// database. An external user is someone outside of TVA who does not have a TVA domain account.
			/// </summary>
			/// <value></value>
			/// <returns>True if user is an external user; otherwise False.</returns>
			public bool IsExternal
			{
				get
				{
					return m_isExternal;
				}
			}
			
			/// <summary>
			/// Gets a boolean value indicating whether or not the user's has been locked because of numerous
			/// unsuccessful login attempts.
			/// </summary>
			/// <value></value>
			/// <returns>True if the user account is locked; otherwise False.</returns>
			public bool IsLockedOut
			{
				get
				{
					return m_isLockedOut;
				}
			}
			
			/// <summary>
			/// Gets the date and time when user must change the password.
			/// </summary>
			/// <value></value>
			/// <returns>The date and time when user must change the password.</returns>
			public System.DateTime PasswordChangeDateTime
			{
				get
				{
					return m_passwordChangeDateTime;
				}
			}
			
			/// <summary>
			/// Gets the date and time when user account was created.
			/// </summary>
			/// <value></value>
			/// <returns>The date and time when user account was created.</returns>
			public System.DateTime AccountCreatedDateTime
			{
				get
				{
					return m_accountCreatedDateTime;
				}
			}
			
			/// <summary>
			/// Gets a boolean value indicating whether or not the user is defined in the security database.
			/// </summary>
			/// <value></value>
			/// <returns>True if the user is defined in the security database; otherwise False.</returns>
			public bool IsDefined
			{
				get
				{
					return m_isDefined;
				}
			}
			
			/// <summary>
			/// Gets a boolean value indicating whether or not the user's credentials have been authenticated.
			/// </summary>
			/// <value></value>
			/// <returns>True if the user's credentials have been authenticated; otherwise False.</returns>
			public bool IsAuthenticated
			{
				get
				{
					return m_isAuthenticated;
				}
			}
			
			/// <summary>
			/// Gets a list of all the groups the user belongs to.
			/// </summary>
			/// <value></value>
			/// <returns>Groups to which the user belongs.</returns>
			public List<Group> Groups
			{
				get
				{
					return m_groups;
				}
			}
			
			/// <summary>
			/// Gets a list of roles that belong to the specified application, or all roles if no application is
			/// specified, to which the user is assigned.
			/// </summary>
			/// <value></value>
			/// <returns>List of roles to which the user is assigned.</returns>
			public List<Role> Roles
			{
				get
				{
					return m_roles;
				}
			}
			
			/// <summary>
			/// Gets a list of roles that belong to the specified application to which the user is assigned.
			/// </summary>
			/// <param name="applicationName">Name of the application for which user roles are to be retrieved.</param>
			/// <value></value>
			/// <returns>List of roles to which the user is assigned.</returns>
			public List Roles(string applicationName)
			{
				//**** Added By Mehul
				List<Role> applicationRoles = new List<Role>();
				
				if (m_roles != null)
				{
					for (int i = 0; i <= m_roles.Count - 1; i++)
					{
						if (string.Compare(m_roles(i).Application.Name, applicationName, true) == 0)
						{
							applicationRoles.Add(m_roles(i));
						}
					}
				}
				
				return applicationRoles;
			}
			
			/// <summary>
			/// Gets a list of all applications to which the user has access if application name is not specified.
			/// </summary>
			/// <value></value>
			/// <returns>List of all applications to which the user has access.</returns>
			public List<Application> Applications
			{
				get
				{
					return m_applications;
				}
			}
			
			/// <summary>
			/// Refreshes user data from the security database.
			/// </summary>
			public void RefreshData()
			{
				
				// Initialize data.
				m_firstName = "";
				m_lastName = "";
				m_companyName = "";
				m_phoneNumber = "";
				m_emailAddress = "";
				m_securityQuestion = "";
				m_securityAnswer = "";
				m_isExternal = false;
				m_isLockedOut = false;
				m_passwordChangeDateTime = DateTime.MinValue;
				m_accountCreatedDateTime = DateTime.MinValue;
				m_isDefined = false;
				m_isAuthenticated = false;
				m_groups.Clear();
				m_roles.Clear();
				m_applications.Clear();
				
				DataSet userData;
				using (SqlConnection dbConnection = GetDatabaseConnection())
				{
					// We'll retrieve all the data we need in a single trip to the database by calling the stored
					// procedure 'RetrieveApiData' that will return 3 tables to us:
					// Table1 (Index 0): Information about the user.
					// Table2 (Index 1): Groups the user is a member of.
					// Table3 (Index 2): Roles that are assigned to the user either directly or through a group.
					userData = TVA.Data.Common.RetrieveDataSet("dbo.RetrieveApiData", dbConnection, m_username, m_applicationName);
				}
				
				
				if (userData.Tables[0].Rows.Count > 0)
				{
					// User does exist in the security database.
					m_isDefined = true;
					System.Data.DataTable with_1 = userData.Tables[0];
					m_username = with_1.Rows[0]["UserName"].ToString();
					m_password = with_1.Rows[0]["UserPassword"].ToString();
					m_firstName = with_1.Rows[0]["UserFirstName"].ToString();
					m_lastName = with_1.Rows[0]["UserLastName"].ToString();
					m_companyName = with_1.Rows[0]["UserCompanyName"].ToString();
					m_phoneNumber = with_1.Rows[0]["UserPhoneNumber"].ToString();
					m_emailAddress = with_1.Rows[0]["UserEmailAddress"].ToString();
					m_securityQuestion = with_1.Rows[0]["UserSecurityQuestion"].ToString();
					m_securityAnswer = with_1.Rows[0]["UserSecurityAnswer"].ToString();
					if (!(with_1.Rows[0]["UserIsExternal"] is DBNull.Value))
					{
						m_isExternal = Convert.ToBoolean(with_1.Rows[0]["UserIsExternal"]);
					}
					if (!(with_1.Rows[0]["UserIsLockedOut"] is DBNull.Value))
					{
						m_isLockedOut = Convert.ToBoolean(with_1.Rows[0]["UserIsLockedOut"]);
					}
					if (!(with_1.Rows[0]["UserPasswordChangeDateTime"] is DBNull.Value))
					{
						m_passwordChangeDateTime = Convert.ToDateTime(with_1.Rows[0]["UserPasswordChangeDateTime"]);
					}
					m_accountCreatedDateTime = Convert.ToDateTime(with_1.Rows[0]["UserAccountCreatedDateTime"]);
					
					System.Data.DataTable with_2 = userData.Tables[1];
					for (int i = 0; i <= with_2.Rows.Count - 1; i++)
					{
						m_groups.Add(new Group(with_2.Rows[i]["GroupName"].ToString(), with_2.Rows[i]["GroupDescription"].ToString()));
					}
					
					System.Data.DataTable with_3 = userData.Tables[2];
					for (int i = 0; i <= with_3.Rows.Count - 1; i++)
					{
						Application application = new Application(with_3.Rows[i]["ApplicationName"].ToString(), with_3.Rows[i]["ApplicationDescription"].ToString());
						m_roles.Add(new Role(with_3.Rows[i]["RoleName"].ToString(), with_3.Rows[i]["RoleDescription"].ToString(), application));
						
						// Since an application can have multiple roles, we're going to add an application to the list
						// of application only if it doesn't exist already.
						if (! m_applications.Contains(application))
						{
							m_applications.Add(application);
						}
					}
					
					userData.Dispose();
				}
				
			}
			
			/// <summary>
			/// Authenticates user's credentials.
			/// </summary>
			/// <param name="password">Password to be authenticated.</param>
			/// <returns>True if authentication was successful; otherwise False.</returns>
			public bool Authenticate(string password)
			{
				
				// We will not authenticate if user account doesn't exist or is locked.
				if (! m_isDefined || m_isLockedOut)
				{
					return false;
				}
				
				// Authenticate based on the specified authentication mode.
				switch (m_authenticationMode)
				{
					case AuthenticationMode.AD:
						if (m_isExternal)
						{
							// User is external according to the security database.
							if (! string.IsNullOrEmpty(password))
							{
								// We'll validate the password against the security database.
								m_isAuthenticated = EncryptPassword(password) == m_password;
							}
						}
						else
						{
							// User is internal according to the security database.
							if (! string.IsNullOrEmpty(password))
							{
								// We'll validate the password against the Active Directory.
                                m_isAuthenticated = new UserInfo("TVA", m_username, true).Authenticate(password);
							}
							else
							{
								// The user to be authenticated is defined as an internal user in the security database,
								// but we don't have a password to authenticate against the Active Directory. ' In this
								// case the authentication requirement becomes that the user we're authenticating must
								// be user executing the request (i.e. accessing secure the app).
								string loginID = System.Threading.Thread.CurrentPrincipal.Identity.Name;
								if (! string.IsNullOrEmpty(loginID))
								{
									m_isAuthenticated = string.Compare(m_username, loginID.Split('\\')[1], true) == 0;
								}
							}
						}
						break;
					case AuthenticationMode.RSA:
						TVA.Security.Radius.RadiusClient client = null;
						RadiusPacket response = null;
						try
						{
							// We first try to authenticate against the primary RSA server.
							client = new TVA.Security.Radius.RadiusClient(RsaServer1, TVA.Security.Cryptography.Common.Decrypt(RadiusSharedSecret));
							response = client.Authenticate(m_username, password);
							
							if (response == null)
							{
								// We didn't get a response back from the primary RSA server. This is most likely
								// to happen when the primary server is unavailable, so we attempt to authenticate
								// against the backup RSA server.
								client.Dispose();
								client = new TVA.Security.Radius.RadiusClient(RsaServer2, TVA.Security.Cryptography.Common.Decrypt(RadiusSharedSecret));
								response = client.Authenticate(m_username, password);
							}
							
							if (response != null)
							{
								// We received a response back from the RSA server.
								switch (response.Type)
								{
									case PacketType.AccessAccept:
										// Credentials were accepted by the RSA server.
										m_isAuthenticated = true;
										break;
									case PacketType.AccessChallenge:
										// RSA server challenged our authentication request.
										if (client.IsUserInNewPinMode(response))
										{
											// If the user's account is in the "new pin" mode, we treat it as if it's
											// time for the user to change the password so appropriate input form is
											// served to the user.
											m_passwordChangeDateTime = DateTime.Now;
										}
										else if (client.IsUserInNextTokenMode(response))
										{
											// If the user's account is in the "next token" mode, we treat the account
											// as if it is disabled so the user must either email or call-in to get the
											// account enabled.
											m_isLockedOut = true;
										}
										break;
								}
							}
						}
						catch (Exception)
						{
							throw;
						}
						finally
						{
							if (client != null)
							{
								client.Dispose();
							}
						}
						break;
				}
				
				// Log successful or unsuccessful authentication result to the security database so that a user account
				// gets locked-out automatically after a set number of unsuccessful login attempts.
				using (SqlConnection dbConnection = GetDatabaseConnection())
				{
					TVA.Data.Common.ExecuteNonQuery("dbo.LogLogin", dbConnection, m_username, ! m_isAuthenticated);
				}
				
				
				return m_isAuthenticated;
				
			}
			
			/// <summary>
			/// Changes the user's current password under AD authentication mode if the user is external, or creates
			/// a new pin (if account is in "new pin" mode) under RSA authentication mode.
			/// </summary>
			/// <param name="oldPassword">
			/// Current password under AD authentication. Current token under RSA authntication.</param>
			/// <param name="newPassword">
			/// New password under AD authentication; new pin under RSA authentication.
			/// </param>
			/// <returns>
			/// True if password was changed successfully under AD authentication mode, or new pin was created
			/// successfully under RSA authentication mode; otherwise False.
			/// </returns>
			public bool ChangePassword(string oldPassword, string newPassword)
			{
				
				// Don't proceed to change password or create pin if user account is not defined or is locked.
				if (! m_isDefined || m_isLockedOut)
				{
					return false;
				}
				
				switch (m_authenticationMode)
				{
					case AuthenticationMode.AD:
						// Under AD authentication mode, we allow the password to be changed only if the user is
						// an external user and after the user's current password has been verified.
						if (m_isExternal && Authenticate(oldPassword))
						{
							using (SqlConnection dbConnection = GetDatabaseConnection())
							{
								TVA.Data.Common.ExecuteScalar("dbo.ChangePassword", dbConnection, m_username, m_password, EncryptPassword(newPassword));
							}
							
							
							return true;
						}
						break;
					case AuthenticationMode.RSA:
						// Under RSA authentication mode, in order to create a pin, the account must actually be in
						// the "new pin" mode. If it is not, then repeated attempts to create a pin may result in the
						// account being placed in "next token" mode (i.e. equivalent to account being disabled). And
						// the problem is that we cannot verify if a account is actually in the "new pin" mode, because
						// a token can be used only once and no more than once.
						TVA.Security.Radius.RadiusClient client = null;
						try
						{
							// First, we try creating a pin against the primary RSA server.
							client = new TVA.Security.Radius.RadiusClient(RsaServer1, TVA.Security.Cryptography.Common.Decrypt(RadiusSharedSecret));
							
							return client.CreateNewPin(m_username, oldPassword, newPassword);
						}
						catch (ArgumentNullException)
						{
							// When we encounter this exception, the primary RSA server didn't respond.
							try
							{
								// Next, we try creating a pin against the backup RSA server.
								client.Dispose();
								client = new TVA.Security.Radius.RadiusClient(RsaServer2, TVA.Security.Cryptography.Common.Decrypt(RadiusSharedSecret));
								
								return client.CreateNewPin(m_username, oldPassword, newPassword);
							}
							catch (Exception)
							{
								// Absorb any other exception.
							}
						}
						catch (Exception)
						{
							// Absorb any other exception.
						}
						finally
						{
							if (client != null)
							{
								client.Dispose();
							}
						}
						break;
				}
				
			}
			
			/// <summary>
			/// Logs successful or unsuccessful attempt of accessing a secure application.
			/// </summary>
			/// <param name="accessDenied">True if user does not have access to the application; otherwise False.</param>
			/// <returns>True if logging was successful; otherwise False.</returns>
			public bool LogAccess(bool accessDenied)
			{
				
				// We will not log access attempt if user is not defined in the security database.
				if (! m_isDefined)
				{
					return false;
				}
				
				if (! string.IsNullOrEmpty(m_applicationName))
				{
					// In order to log an access attempt to the security data, we must have the name of the application
					// user was trying to access. If we don't have that then we cannot log access attempt to the database.
					using (SqlConnection dbConnection = GetDatabaseConnection())
					{
						TVA.Data.Common.ExecuteNonQuery("dbo.LogAccess", dbConnection, m_username, m_applicationName, accessDenied);
					}
					
					
					return true;
				}
				else
				{
					throw (new InvalidOperationException("Application name is not set."));
				}
				
			}
			
			/// <summary>
			/// Logs information about an encountered exception to the security database.
			/// </summary>
			/// <param name="source">Source of the exception.</param>
			/// <param name="message">Detailed description of the exception.</param>
			/// <returns>True if information was logged successfully; otherwise False.</returns>
			public bool LogError(string source, string message)
			{
				
				try
				{
					using (SqlConnection dbConnection = GetDatabaseConnection())
					{
						TVA.Data.Common.ExecuteScalar("dbo.LogError", dbConnection, m_applicationName, source, message);
					}
					
					
					return true;
				}
				catch (Exception)
				{
					// Absorb any exception we might encounter.
				}
				
			}
			
			/// <summary>
			/// Finds the specified group.
			/// </summary>
			/// <param name="groupName">Name of the group to be found.</param>
			/// <returns>Group if one is found; otherwise Nothing.</returns>
			public Group FindGroup(string groupName)
			{
				
				if (m_groups != null)
				{
					for (int i = 0; i <= m_groups.Count - 1; i++)
					{
						if (string.Compare(m_groups(i).Name, groupName, true) == 0)
						{
							// User does belong to the specified group.
							return m_groups(i);
						}
					}
				}
				return null;
				
			}
			
			/// <summary>
			/// Finds the specified role.
			/// </summary>
			/// <param name="roleName">Name of the role to be found.</param>
			/// <returns>Role if one is found; otherwise Nothing.</returns>
			public Role FindRole(string roleName)
			{
				
				if (m_roles != null)
				{
					for (int i = 0; i <= m_roles.Count - 1; i++)
					{
						if (string.Compare(m_roles(i).Name, roleName, true) == 0)
						{
							// User is in the specified role.
							return m_roles(i);
						}
					}
				}
				return null;
				
			}
			
			/// <summary>
			/// Finds the specified role.
			/// </summary>
			/// <param name="roleName">Name of the role to be found.</param>
			/// <param name="applicationName">Name of the application to which the role belongs.</param>
			/// <returns>Role if one is found; otherwise Nothing.</returns>
			public Role FindRole(string roleName, string applicationName)
			{
				
				Role role = FindRole(roleName);
				if ((role != null)&& string.Compare(role.Application.Name, applicationName, true) == 0)
				{
					// User is in the specified role and the specified role belongs to the specified application.
					return role;
				}
				return null;
				
			}
			
			/// <summary>
			/// Finds the specified application.
			/// </summary>
			/// <param name="applicationName">Name of the application to be found.</param>
			/// <returns>Application if one is found; otherwise Nothing.</returns>
			public Application FindApplication(string applicationName)
			{
				
				if (m_applications != null)
				{
					for (int i = 0; i <= m_applications.Count - 1; i++)
					{
						if (string.Compare(m_applications(i).Name, applicationName, true) == 0)
						{
							// User has access to the specified application.
							return m_applications(i);
						}
					}
				}
				return null;
				
			}
			
			/// <summary>
			/// Returns users roles for an application.
			/// </summary>
			/// <param name="applicationName">Application Name</param>
			/// <returns>List of roles for specified application</returns>
			/// <remarks></remarks>
			[Obsolete("Use the Roles property that takes an application name as a parameter instead. This function will be removed in a future release.", true)]public List<Role> FindApplicationRoles(string applicationName)
			{
				
				//**** Added By Mehul
				List<Role> applicationRoles = new List<Role>();
				
				if (m_roles != null)
				{
					for (int i = 0; i <= m_roles.Count - 1; i++)
					{
						if (string.Compare(m_roles(i).Application.Name, applicationName, true) == 0)
						{
							applicationRoles.Add(m_roles(i));
						}
					}
				}
				
				return applicationRoles;
				
			}
			
			#region " Shared "
			
			/// <summary>
			/// Generates a random password of specified length with at least one uppercase character, one lowercase
			/// character and one digit.
			/// </summary>
			/// <param name="length">Length of the random password.</param>
			/// <returns>Random password of the specified lenght.</returns>
			public static string GeneratePassword(int length)
			{
				
				if (length >= MinimumPasswordLength)
				{
					char[] password = TVA.Common.CreateArray<char>(length);
					
					// ASCII character ranges:
					// Digits - 48 to 57
					// Upper case - 65 to 90
					// Lower case - 97 to 122
					
					// Out of the minimum of 8 characters in the password, we'll make sure that the password contains
					// at least 2 digits and 2 upper case letter, so that the password meets the strong password rule.
					int digits = 0;
					int upperCase = 0;
					int minSpecialChars = 2;
					for (int i = 0; i <= password.Length - 1; i++)
					{
						if (digits < minSpecialChars)
						{
							password[i] = Strings.Chr((int) (TVA.Math.Common.RandomBetween(48, 57)));
							digits++;
						}
						else if (upperCase < minSpecialChars)
						{
							password[i] = Strings.Chr((int) (TVA.Math.Common.RandomBetween(65, 90)));
							upperCase++;
						}
						else
						{
							password[i] = Strings.Chr((int) (TVA.Math.Common.RandomBetween(97, 122)));
						}
					}
					
					// We have a random password that meets the strong password rule, now we'll shuffle it to make it
					// even more random.
					char temp;
					int swapIndex;
					for (int i = 0; i <= password.Length - 1; i++)
					{
						swapIndex = (int) (TVA.Math.Common.RandomBetween(0, password.Length - 1));
						temp = password[swapIndex];
						password[swapIndex] = password[i];
						password[i] = temp;
					}
					
					return new string(password);
				}
				else
				{
					throw (new ArgumentException(string.Format("Password length should be at least {0} characters.", MinimumPasswordLength)));
				}
				
			}
			
			/// <summary>
			/// Encrypts the password to a one-way hash using the SHA1 hash algorithm.
			/// </summary>
			/// <param name="password">Password to be encrypted.</param>
			/// <returns>Encrypted password.</returns>
			public static string EncryptPassword(string password)
			{
				
				if (Regex.IsMatch(password, StrongPasswordRegex))
				{
					// We prepend salt text to the password and then has it to make it even more secure.
					return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile("O3990\\P78f9E66b:a35_V©6M13©6~2&[" + password, "SHA1");
				}
				else
				{
					// Password does not meet the strong password rule defined below, so we don't encrypt the password.
					System.Text.StringBuilder with_1 = new StringBuilder();
					with_1.Append("Password does not meet the following criteria:");
					with_1.AppendLine();
					with_1.Append("- Password must be at least 8 characters");
					with_1.AppendLine();
					with_1.Append("- Password must contain at least 1 digit");
					with_1.AppendLine();
					with_1.Append("- Password must contain at least 1 upper case letter");
					with_1.AppendLine();
					with_1.Append("- Password must contain at least 1 lower case letter");
					
					throw (new InvalidOperationException(with_1.ToString()));
				}
				
			}
			
			#endregion
			
			#endregion
			
			#region " Code Scope: Friend "
			
			/// <summary>
			/// Creates an instance of a user defined in the security database.
			/// </summary>
			/// <param name="username">Username of the user.</param>
			/// <param name="password">Password of the user.</param>
			/// <param name="applicationName">Name of the application for which user data is to be retrieved.</param>
			/// <param name="securityServer">Security server from which user data is to be retrieved.</param>
			/// <param name="authenticationMode">Mode of authentication to be used for authenticating credentials.</param>
			/// <param name="authenticate">True if user credentials are to be authenticated; otherwise False.</param>
			/// <remarks>
			/// This constructor is only to be used internally by the security provider control and its sub-components.
			/// </remarks>
			internal User(string username, string password, string applicationName, SecurityServer securityServer, AuthenticationMode authenticationMode, bool authenticate)
			{
				
				m_username = username;
				m_password = password;
				m_applicationName = applicationName;
				m_securityServer = securityServer;
				m_authenticationMode = authenticationMode;
				m_groups = new List<Group>();
				m_roles = new List<Role>();
				m_applications = new List<Application>();
				
				this.RefreshData(); // Retrieve user data.
				if (authenticate)
				{
					this.Authenticate(password); // Authenticate user crendentials.
				}
				else
				{
					m_isAuthenticated = true; // Pretend user credentials are authenticated.
				}
				
			}
			
			#endregion
			
			#region " Code Scope: Private "
			
			private SqlConnection GetDatabaseConnection()
			{
				
				SqlConnection connection = null;
				try
				{
					switch (m_securityServer)
					{
						case SecurityServer.Development:
							connection = new SqlConnection(TVA.Security.Cryptography.Common.Decrypt(DevConnectionString));
							break;
						case SecurityServer.Acceptance:
							connection = new SqlConnection(TVA.Security.Cryptography.Common.Decrypt(AcpConnectionString));
							break;
						case SecurityServer.Production:
							connection = new SqlConnection(TVA.Security.Cryptography.Common.Decrypt(PrdConnectionString));
							break;
					}
					
					connection.Open();
				}
				catch (SqlException)
				{
					if (m_securityServer != SecurityServer.Production)
					{
						throw;
					}
					
					// Failed to open the connection, so we'll use the backup in production.
					connection = new SqlConnection(TVA.Security.Cryptography.Common.Decrypt(BakConnectionString));
					connection.Open();
				}
				catch (Exception)
				{
					throw;
				}
				
				return connection;
				
			}
			
			#endregion
			
		}
		
	}
}
