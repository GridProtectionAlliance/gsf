using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.DirectoryServices;
using System.Security.Principal;

//*******************************************************************************************************
//  TVA.Identity.UserInfo.vb - ActiveDirectory User Information Class
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/10/2004 - J. Ritchie Carroll
//       Original version of source code generated
//  01/03/2006 - Pinal C. Patel
//       2.0 version of source code migrated from 1.1 source (TVA.Shared.Identity)
//  09/27/2006 - Pinal C. Patel
//       Added Authenticate() function.
//  09/29/2006 - Pinal C. Patel
//       Added support to impersonate privileged user for retrieving user information
//  11/06/2007 - Pinal C. Patel
//       Modified the logic of Authenticate method to use the user's DirectoryEntry instance
//       Modified UserEntry property to impersonate privileged user if configured for the instance
//  11/08/2007 - J. Ritchie Carroll
//       Corrected spelling of "Previleged" - this was a breaking change.
//       Also, implemented user customizable implementation of previleged account credentials.
//
//*******************************************************************************************************


namespace TVA
{
	namespace Identity
	{
		
		public class UserInfo
		{
			
			
			private string m_loginID;
			private string m_domain;
			private string m_username;
			private bool m_usePrivilegedAccount;
			private DirectoryEntry m_userEntry;
			
			private string m_previlegedUserName;
			private string m_previlegedPassword;
			private string m_previlegedDomain;
			
			// TODO: As soon as "esocss" account goes away - the default entries will need to be removed and users will
			// have to call the "DefinePriviledgedAccount" function with their own credientials...
			private const string DefaultPrevilegedUserName = "esocss";
			private const string DefaultPrevilegedPassword = "pwd4ctrl";
			private const string DefaultPrevilegedDomain = "TVA";
			
			public UserInfo(string username, string domain) : this(username, domain, false)
			{
				
				
			}
			
			/// <summary>Initializes a new instance of the user information class.</summary>
			
			
			
		}
		
		public UserInfo(string loginID) : this(loginID, false)
		{
			
			
		}
		
		/// <summary>Initializes a new instance of the user information class.</summary>
		/// <remarks>Specify login information as domain\username.</remarks>
		public UserInfo(string loginID, bool usePrivilegedAccount)
		{
			
			string loginIDParts = loginID.Split('\\');
			if (loginIDParts.Length == 2)
			{
				m_domain = loginIDParts[0];
				m_username = loginIDParts[1];
			}
			
			m_loginID = loginID;
			m_usePrivilegedAccount = usePrivilegedAccount;
			
			m_previlegedUserName = DefaultPrevilegedUserName;
			m_previlegedPassword = DefaultPrevilegedPassword;
			m_previlegedDomain = DefaultPrevilegedDomain;
			
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether a privileged account will be used for retrieving
		/// information about the user from the Active Directory.
		/// </summary>
		/// <value></value>
		/// <returns>True if privileged account is to be used; otherwise False.</returns>
		public bool UsePrivilegedAccount
		{
			get
			{
				return m_usePrivilegedAccount;
			}
			set
			{
				m_usePrivilegedAccount = value;
			}
		}
		
		/// <summary>
		/// Defines priviledged account information
		/// </summary>
		/// <param name="username">Username of priviledged account</param>
		/// <param name="password">Password of priviledged account</param>
		/// <param name="domain">Domain of priviledged account</param>
		public void DefinePriviledgedAccount(string username, string password, string domain)
		{
			
			m_previlegedUserName = username;
			m_previlegedPassword = password;
			m_previlegedDomain = domain;
			
		}
		
		/// <summary>Gets the login ID of the user.</summary>
		public string LoginID
		{
			get
			{
				return m_loginID;
			}
		}
		
		/// <summary>Gets the System.DirectoryServices.DirectoryEntry of the user</summary>
		public DirectoryEntry UserEntry
		{
			get
			{
				if (m_userEntry == null)
				{
					WindowsImpersonationContext currentContext = null;
					try
					{
						// 11/06/2007 - PCP: Some change in the AD now causes the searching the AD to fail also if
						// this code is not being executed under a domain account which was not the case before;
						// before only AD property lookup had this behavior.
						if (m_usePrivilegedAccount)
						{
							// Impersonate to the privileged account if specified.
							currentContext = Common.ImpersonateUser(m_previlegedUserName, m_previlegedPassword, m_previlegedDomain);
						}
						
						// 02/27/2007 - PCP: Using the default directory entry instead of specifying the domain name.
						// This is done to overcome "The server is not operational" COM exception that was being
						// encountered when a domain name was being specified.
						DirectoryEntry entry = new DirectoryEntry();
						//Dim entry As New DirectoryEntry("LDAP://" & m_domain)
						
						System.DirectoryServices.DirectorySearcher with_1 = new DirectorySearcher(entry);
						with_1.Filter = "(SAMAccountName=" + m_username + ")";
						m_userEntry = with_1.FindOne().GetDirectoryEntry();
					}
					catch
					{
						m_userEntry = null;
						throw;
					}
					finally
					{
						// Undo impersonation if it was performed.
						if (currentContext != null)
						{
							Common.EndImpersonation(currentContext);
						}
					}
				}
				
				return m_userEntry;
			}
		}
		
		/// <summary>Returns adctive directory value for specified property</summary>
		public string UserProperty(System.String propertyName)
		{
			WindowsImpersonationContext currentContext = null;
			try
			{
				if (m_usePrivilegedAccount)
				{
					// Impersonate to the privileged account if specified.
					currentContext = Common.ImpersonateUser(m_previlegedUserName, m_previlegedPassword, m_previlegedDomain);
				}
				
				return UserEntry.Properties[propertyName][0].ToString().Replace("  ", " ").Trim();
			}
			catch (Exception)
			{
				return "";
			}
			finally
			{
				// Undo impersonation if it was performed.
				if (currentContext != null)
				{
					Common.EndImpersonation(currentContext);
				}
			}
		}
		
		public string FirstName
		{
			get
			{
				return UserProperty("givenName");
			}
		}
		
		public string LastName
		{
			get
			{
				return UserProperty("sn");
			}
		}
		
		public string MiddleInitial
		{
			get
			{
				return UserProperty("initials");
			}
		}
		
		/// <summary>Gets the full name of the user</summary>
		public string FullName
		{
			get
			{
				if (! string.IsNullOrEmpty(FirstName) && ! string.IsNullOrEmpty(LastName) && ! string.IsNullOrEmpty(MiddleInitial))
				{
					return FirstName + " " + MiddleInitial + " " + LastName;
				}
				else
				{
					return m_loginID;
				}
			}
		}
		
		/// <summary>Gets the e-mail address of the user</summary>
		public string Email
		{
			get
			{
				return UserProperty("mail");
			}
		}
		
		/// <summary>Gets the telephone number of the user</summary>
		public string Telephone
		{
			get
			{
				return UserProperty("telephoneNumber");
			}
		}
		
		/// <summary>Gets the title of the user</summary>
		public string Title
		{
			get
			{
				return UserProperty("title");
			}
		}
		
		/// <summary>Gets the company of the user</summary>
		public string Company
		{
			get
			{
				return UserProperty("company");
			}
		}
		
		/// <summary>Returns the office location of the user</summary>
		public string Office
		{
			get
			{
				return UserProperty("physicalDeliveryOfficeName");
			}
		}
		
		/// <summary>Gets the department name where the user works</summary>
		public string Department
		{
			get
			{
				return UserProperty("department");
			}
		}
		
		/// <summary>Gets the city where the user works</summary>
		public string City
		{
			get
			{
				return UserProperty("l");
			}
		}
		
		/// <summary>Returns the mailbox of where the user works</summary>
		public string Mailbox
		{
			get
			{
				return UserProperty("streetAddress");
			}
		}
		
		/// <summary>
		/// Authenticates the user against Active Directory with the specified password.
		/// </summary>
		/// <param name="password">The password to be used for authentication.</param>
		/// <returns>True is the user can be authenticated; otherwise False.</returns>
		public bool Authenticate(string password)
		{
			
			try
			{
				string lookupResult;
				
				// Set the credentials to use for looking up AD info.
				UserEntry.Username = m_username;
				UserEntry.Password = password;
				
				// We'll lookup a AD property which will fail if the credentials are incorrect.
				lookupResult = UserProperty("displayName");
				
				// Remove the username and password we used for authentication.
				UserEntry.Username = null;
				UserEntry.Password = null;
				
				// AD property value will be null string if the AD property lookup failed.
				return ! string.IsNullOrEmpty(lookupResult);
			}
			catch (Exception)
			{
				// The one exception we might get is when we're getting the user's AD entry.
			}
			
		}
		
	}
	
}

}
