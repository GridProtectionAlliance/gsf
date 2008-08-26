using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.DirectoryServices;
using TVA.Interop.WindowsApi;

//*******************************************************************************************************
//  TVA.Identity.Common.vb - Common User Identity Functions
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
//  ??/??/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/03/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Identity).
//  09/29/2006 - Pinal C. Patel
//       Added overload for ImpersonateUser() that does not take the "domain" parameter.
//  12/12/2007 - Darrell Zuercher
//       Edited Code Comments.
//
//*******************************************************************************************************


namespace TVA
{
	namespace Identity
	{
		
		public sealed class Common
		{
			
			
			[DllImport("advapi32.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
			private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
			[DllImport("kernel32.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
			private static extern bool CloseHandle(IntPtr handle);
			[DllImport("advapi32.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
			private static extern bool DuplicateToken(IntPtr ExistingTokenHandle, int SecurityImpersonationLevel, ref IntPtr DuplicateTokenHandle);
			
			private const int LOGON32_PROVIDER_DEFAULT = 0;
			private const int LOGON32_LOGON_INTERACTIVE = 2;
			private const int LOGON32_LOGON_NETWORK = 3;
			private const int SECURITY_IMPERSONATION = 2;
			
			private static UserInfo m_currentUserInfo;
			
			private Common()
			{
				
				// This class contains only global functions and is not meant to be instantiated.
				
			}
			
			/// <summary>Gets the current user's information.</summary>
			public static UserInfo CurrentUser
			{
				get
				{
					if (m_currentUserInfo == null)
					{
						m_currentUserInfo = new UserInfo(CurrentUserID);
					}
					return m_currentUserInfo;
				}
			}
			
			/// <summary>Gets the current user's NT ID.</summary>
			public static string CurrentUserID
			{
				get
				{
					return WindowsIdentity.GetCurrent().Name;
				}
			}
			
			/// <summary>Validates NT authentication, given the specified credentials.</summary>
			public static bool AuthenticateUser(string username, string password, string domain)
			{
				
				IntPtr tokenHandle = IntPtr.Zero;
				bool authenticated;
				
				try
				{
					// Call LogonUser to attempt authentication
					authenticated = LogonUser(username, domain, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);
				}
				catch
				{
					// We rethrow any exceptions back to user, we are just using try/catch so we can clean up in finally
					throw;
				}
				finally
				{
					// Free the token
					if (! IntPtr.op_Equality(tokenHandle, IntPtr.Zero))
					{
						CloseHandle(tokenHandle);
					}
				}
				
				return authenticated;
				
			}
			
			public static WindowsImpersonationContext ImpersonateUser(string username, string password)
			{
				
				return ImpersonateUser(username, password, "TVA");
				
			}
			
			/// <summary>Impersonates the specified user.</summary>
			/// <param name="username">The user to be ompersonated.</param>
			public static WindowsImpersonationContext ImpersonateUser(string username, string password, string domain)
			{
				
				WindowsImpersonationContext impersonatedUser;
				IntPtr tokenHandle = IntPtr.Zero;
				IntPtr dupeTokenHandle = IntPtr.Zero;
				
				try
				{
					// Calls LogonUser to obtain a handle to an access token.
					if (! LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle))
					{
						throw (new InvalidOperationException("Failed to impersonate user " + domain + "\\" + username + ".  " + GetErrorMessage(Marshal.GetLastWin32Error())));
					}
					
					if (! DuplicateToken(tokenHandle, SECURITY_IMPERSONATION, ref dupeTokenHandle))
					{
						CloseHandle(tokenHandle);
						throw (new InvalidOperationException("Failed to impersonate user " + domain + "\\" + username + ".  Exception thrown while trying to duplicate token."));
					}
					
					// The token that is passed into WindowsIdentity must be a primary token in order to use it for impersonation.
					impersonatedUser = WindowsIdentity.Impersonate(dupeTokenHandle);
				}
				catch
				{
					// Rethrows any exceptions back to user. This is a try/catch, so it can be cleaned up in finally.
					throw;
				}
				finally
				{
					// Frees the tokens.
					if (! IntPtr.op_Equality(tokenHandle, IntPtr.Zero))
					{
						CloseHandle(tokenHandle);
					}
					if (! IntPtr.op_Equality(dupeTokenHandle, IntPtr.Zero))
					{
						CloseHandle(dupeTokenHandle);
					}
				}
				
				return impersonatedUser;
				
			}
			
			/// <summary>Ends impersonation of the specified user.</summary>
			public static void EndImpersonation(WindowsImpersonationContext impersonatedUser)
			{
				
				if (impersonatedUser != null)
				{
					impersonatedUser.Undo();
				}
				impersonatedUser = null;
				
			}
			
		}
		
	}
	
}
