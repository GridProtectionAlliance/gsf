//*******************************************************************************************************
//  UserInfo.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
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
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.DirectoryServices;
using System.Security.Principal;
using System.Runtime.InteropServices;
using TVA.Interop;

namespace TVA.Identity
{
    /// <summary>
    /// User Information Class.
    /// </summary>
    public class UserInfo
    {
        #region [ Members ]

        // Constants
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int SECURITY_IMPERSONATION = 2;

        // Fields
        private string m_username;
        private string m_domain;
        private DirectoryEntry m_userEntry;
        private bool m_usePrivilegedAccount;
        private string m_previlegedUserName;
        private string m_previlegedPassword;
        private string m_previlegedDomain;

        #endregion

        #region [ Constructors ]

        /// <summary>Initializes a new instance of the user information class.</summary>
        public UserInfo(string username, string domain)
            : this(username, domain, false)
        {
        }

        /// <summary>Initializes a new instance of the user information class.</summary>
        /// <remarks>Specify login information as domain\username.</remarks>
        public UserInfo(string username, string domain, bool usePrivilegedAccount)
        {
            m_username = username;
            m_domain = domain;
            m_usePrivilegedAccount = usePrivilegedAccount;
        }

        /// <summary>Initializes a new instance of the user information class.</summary>
        public UserInfo(string loginID)
            : this(loginID, false)
        {
        }

        public UserInfo(string loginID, bool usePrivilegedAccount)
        {
            string[] parts = loginID.Split('\\');

            if (parts.Length == 2)
            {
                m_domain = parts[0];
                m_username = parts[1];
                m_usePrivilegedAccount = usePrivilegedAccount;
            }
            else
            {
                throw new ArgumentException("Expected login ID in format of domain\\username");
            }
        }

        #endregion

        #region [ Properties ]

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

                        // Impersonate to the privileged account if specified.
                        if (m_usePrivilegedAccount)
                        {
                            if (string.IsNullOrEmpty(m_previlegedUserName))
                                throw new ArgumentNullException("PrevilegedUserName", "Privileged account information has not been defined - call DefinePrivilegedAccount first.");

                            currentContext = ImpersonateUser(m_previlegedUserName, m_previlegedPassword, m_previlegedDomain);
                        }

                        // 02/27/2007 - PCP: Using the default directory entry instead of specifying the domain name.
                        // This is done to overcome "The server is not operational" COM exception that was being
                        // encountered when a domain name was being specified.
                        DirectoryEntry entry = new DirectoryEntry();

                        //Dim entry As New DirectoryEntry("LDAP://" & m_domain)
                        DirectorySearcher searcher = new DirectorySearcher(entry);
                        searcher.Filter = "(SAMAccountName=" + m_username + ")";
                        m_userEntry = searcher.FindOne().GetDirectoryEntry();
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
                            EndImpersonation(currentContext);
                    }
                }

                return m_userEntry;
            }
        }

        public string LoginID
        {
            get
            {
                return m_domain + "\\" + m_username;
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
                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                {
                    if (!string.IsNullOrEmpty(MiddleInitial))
                        return FirstName + " " + MiddleInitial + " " + LastName;
                    else
                        return FirstName + " " + LastName;
                }
                else
                {
                    return m_domain + "\\" + m_username;
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


        #endregion

        #region [ Methods ]

        /// <summary>
        /// Authenticates the current user using the specified password.
        /// </summary>
        /// <param name="password">The password to be used for authentication.</param>
        /// <returns>True is the user can be authenticated; otherwise False.</returns>
        public bool Authenticate(string password)
        {
            return UserInfo.AuthenticateUser(m_username, password, m_domain);
        }

        /// <summary>
        /// Defines privileged account information
        /// </summary>
        /// <param name="username">Username of privileged account</param>
        /// <param name="password">Password of privileged account</param>
        /// <param name="domain">Domain of privileged account</param>
        public void DefinePrivilegedAccount(string username, string password, string domain)
        {
            m_previlegedUserName = username;
            m_previlegedPassword = password;
            m_previlegedDomain = domain;
        }

        /// <summary>Returns adctive directory value for specified property</summary>
        public string UserProperty(string propertyName)
        {
            WindowsImpersonationContext currentContext = null;

            try
            {
                // Impersonate to the privileged account if specified.
                if (m_usePrivilegedAccount)
                {
                    if (string.IsNullOrEmpty(m_previlegedUserName))
                        throw new ArgumentNullException("PrevilegedUserName", "Privileged account information has not been defined - call DefinePrivilegedAccount first.");

                    currentContext = ImpersonateUser(m_previlegedUserName, m_previlegedPassword, m_previlegedDomain);
                }

                return UserEntry.Properties[propertyName][0].ToString().Replace("  ", " ").Trim();
            }
            catch
            {
                return "";
            }
            finally
            {
                // Undo impersonation if it was performed.
                if (currentContext != null)
                    EndImpersonation(currentContext);
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static UserInfo m_currentUserInfo;

        // Static Properties

        /// <summary>Gets the current user's information.</summary>
        public static UserInfo CurrentUser
        {
            get
            {
                if (m_currentUserInfo == null)
                    m_currentUserInfo = new UserInfo(CurrentUserID);

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

        // Static Methods

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
            finally
            {
                // Free the token
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);
            }

            return authenticated;
        }

        /// <summary>Impersonates the specified user.</summary>
        /// <param name="username">Name of user to impersonate.</param>
        /// <param name="password">Password of user to impersonate.</param>
        public static WindowsImpersonationContext ImpersonateUser(string username, string password)
        {
            return ImpersonateUser(username, password, "TVA");
        }

        /// <summary>Impersonates the specified user.</summary>
        /// <param name="username">Name of user to impersonate.</param>
        /// <param name="password">Password of user to impersonate.</param>
        /// <param name="domain">Domain of user to impersonate.</param>
        public static WindowsImpersonationContext ImpersonateUser(string username, string password, string domain)
        {
            WindowsImpersonationContext impersonatedUser;
            IntPtr tokenHandle = IntPtr.Zero;
            IntPtr dupeTokenHandle = IntPtr.Zero;

            try
            {
                // Calls LogonUser to obtain a handle to an access token.
                if (!LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle))
                    throw new InvalidOperationException("Failed to impersonate user " + domain + "\\" + username + ".  " + WindowsApi.GetErrorMessage(Marshal.GetLastWin32Error()));

                if (!DuplicateToken(tokenHandle, SECURITY_IMPERSONATION, ref dupeTokenHandle))
                {
                    CloseHandle(tokenHandle);
                    throw new InvalidOperationException("Failed to impersonate user " + domain + "\\" + username + ".  Exception thrown while trying to duplicate token.");
                }

                // The token that is passed into WindowsIdentity must be a primary token in order to use it for impersonation.
                impersonatedUser = WindowsIdentity.Impersonate(dupeTokenHandle);
            }
            finally
            {
                // Frees the tokens.
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);

                if (dupeTokenHandle != IntPtr.Zero)
                    CloseHandle(dupeTokenHandle);
            }

            return impersonatedUser;
        }

        /// <summary>Ends impersonation of the specified user.</summary>
        public static void EndImpersonation(WindowsImpersonationContext impersonatedUser)
        {
            if (impersonatedUser != null)
                impersonatedUser.Undo();

            impersonatedUser = null;
        }

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DuplicateToken(IntPtr ExistingTokenHandle, int SecurityImpersonationLevel, ref IntPtr DuplicateTokenHandle);

        #endregion
    }
}