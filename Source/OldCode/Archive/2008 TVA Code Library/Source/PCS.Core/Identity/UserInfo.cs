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
//       2.0 version of source code migrated from 1.1 source (PCS.Shared.Identity)
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
//  10/06/2008 - Pinal C. Patel
//      Edited code comments.
//  10/06/2008 - Pinal C. Patel
//      Added static properties RemoteUserID and RemoteUserInfo.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using PCS.Configuration;
using PCS.Interop;

namespace PCS.Identity
{
    /// <summary>
    /// A class that can be used to retrieve information about a domain user from Active Directory.
    /// </summary>
    /// <remarks>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ms677980.aspx" target="_blank">http://msdn.microsoft.com/en-us/library/ms677980.aspx</a> for more information on active directory properties.
    /// </remarks>
    /// <example>
    /// This example shows how to retrieve user information from Active Directory:
    /// <code>
    /// using System;
    /// using PCS.Identity;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Retrieve and display user information from Active Directory.
    ///         using (UserInfo user = new UserInfo("XYZCorp\\johndoe"))
    ///         {
    ///             Console.WriteLine(string.Format("First Name: {0}", user.FirstName));
    ///             Console.WriteLine(string.Format("Last Name: {0}", user.LastName));
    ///             Console.WriteLine(string.Format("Middle Initial: {0}", user.MiddleInitial));
    ///             Console.WriteLine(string.Format("Email Address: {0}", user.Email));
    ///             Console.WriteLine(string.Format("Telephone Number: {0}", user.Telephone));
    ///         }
    ///
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class UserInfo : ISupportLifecycle, IPersistSettings
    {
        #region [ Members ]

        // Constants
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int SECURITY_IMPERSONATION = 2;

        // Fields
        private string m_domain;
        private string m_username;
        private DirectoryEntry m_userEntry;
        private string m_previlegedDomain;
        private string m_previlegedUserName;
        private string m_previlegedPassword;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        /// <param name="domain">Domain where the user's account exists.</param>
        /// <param name="username">Username of user's account whose information is to be retrieved.</param>
        public UserInfo(string domain, string username)
        {
            m_domain = domain;
            m_username = username;
            m_settingsCategory = this.GetType().Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        /// <param name="loginID">Login ID in "domain\username" format of the user's account whose information is to be retrieved.</param>
        public UserInfo(string loginID)
        {
            string[] parts = loginID.Split('\\');

            if (parts.Length != 2)
                throw new ArgumentException("Expected login ID in format of 'domain\\username'.");

            if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                throw new ArgumentException("Expected login ID in format of 'domain\\username'.");

            m_domain = parts[0];
            m_username = parts[1];
            m_settingsCategory = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="UserInfo"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~UserInfo()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="UserInfo"/> object is currently enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="Enabled"/> property is not be set by user-code directly.
        /// </remarks>
        [Browsable(false), 
        EditorBrowsable(EditorBrowsableState.Never), 
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="UserInfo"/> object are 
        /// to be saved to the config file.
        /// </summary>
        public bool PersistSettings
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
        /// Gets or sets the category under which the settings of <see cref="UserInfo"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets the Login ID of the user.
        /// </summary>
        /// <remarks>Returns the value provided in the <see cref="UserInfo(string)"/> constructor.</remarks>
        public string LoginID
        {
            get
            {
                return m_domain + "\\" + m_username;
            }
        }

        /// <summary>
        /// Gets the First Name of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "givenName" active directory property.</remarks>
        public string FirstName
        {
            get
            {
                return GetUserProperty("givenName");
            }
        }

        /// <summary>
        /// Gets the Last Name of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "sn" active directory property.</remarks>
        public string LastName
        {
            get
            {
                return GetUserProperty("sn");
            }
        }

        /// <summary>
        /// Gets the Middle Initial of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "initials" active directory property.</remarks>
        public string MiddleInitial
        {
            get
            {
                return GetUserProperty("initials");
            }
        }

        /// <summary>
        /// Gets the Full Name of the user.
        /// </summary>
        /// <remarks>Returns the concatenation of <see cref="FirstName"/>, <see cref="MiddleInitial"/> and <see cref="LastName"/> properties.</remarks>
        public string FullName
        {
            get
            {
                string fName = FirstName;
                string lName = LastName;
                string mInitial = MiddleInitial;
                if (!string.IsNullOrEmpty(fName) && !string.IsNullOrEmpty(lName))
                {
                    if (string.IsNullOrEmpty(mInitial))
                        return fName + " " + lName;

                    else
                        return fName + " " + mInitial + " " + lName;
                }
                else
                {
                    return m_domain + "\\" + m_username;
                }
            }
        }

        /// <summary>
        /// Gets the E-Mail address of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "mail" active directory property.</remarks>
        public string Email
        {
            get
            {
                return GetUserProperty("mail");
            }
        }

        /// <summary>
        /// Gets the Telephone Number of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "telephoneNumber" active directory property.</remarks>
        public string Telephone
        {
            get
            {
                return GetUserProperty("telephoneNumber");
            }
        }

        /// <summary>
        /// Gets the Title of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "title" active directory property.</remarks>
        public string Title
        {
            get
            {
                return GetUserProperty("title");
            }
        }

        /// <summary>
        /// Gets the Company of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "company" active directory property.</remarks>
        public string Company
        {
            get
            {
                return GetUserProperty("company");
            }
        }

        /// <summary>
        /// Gets the Office location of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "physicalDeliveryOfficeName" active directory property.</remarks>
        public string Office
        {
            get
            {
                return GetUserProperty("physicalDeliveryOfficeName");
            }
        }

        /// <summary>
        /// Gets the Department where the user works.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "department" active directory property.</remarks>
        public string Department
        {
            get
            {
                return GetUserProperty("department");
            }
        }

        /// <summary>
        /// Gets the City where the user works.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "l" active directory property.</remarks>
        public string City
        {
            get
            {
                return GetUserProperty("l");
            }
        }

        /// <summary>
        /// Gets the Mailbox address of where the user works.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "streetAddress" active directory property.</remarks>
        public string Mailbox
        {
            get
            {
                return GetUserProperty("streetAddress");
            }
        }

        /// <summary>
        /// Gets the <see cref="DirectoryEntry"/> object used for retrieving user information.
        /// </summary>
        public DirectoryEntry UserEntry
        {
            get
            {
                return m_userEntry;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="UserInfo"/> object.
        /// </summary>
        public void Initialize()
        {
            if (!m_initialized)
            {
                // Load settings from config file.
                LoadSettings();

                // Initialize the directory entry object used to retrieve AD info.
                WindowsImpersonationContext currentContext = null;
                try
                {
                    // 11/06/2007 - PCP: Some change in the AD now causes the searching the AD to fail also if
                    // this code is not being executed under a domain account which was not the case before;
                    // before only AD property lookup had this behavior.

                    // Impersonate to the privileged account if specified.
                    if (!string.IsNullOrEmpty(m_previlegedDomain) &&
                        !string.IsNullOrEmpty(m_previlegedUserName) &&
                        !string.IsNullOrEmpty(m_previlegedPassword))
                    {
                        currentContext = ImpersonateUser(m_previlegedDomain,
                                                         m_previlegedUserName,
                                                         m_previlegedPassword);
                    }

                    // 02/27/2007 - PCP: Using the default directory entry instead of specifying the domain name.
                    // This is done to overcome "The server is not operational" COM exception that was being
                    // encountered when a domain name was being specified.
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + m_domain);

                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = "(SAMAccountName=" + m_username + ")";
                        SearchResult result = searcher.FindOne();
                        if (result != null)
                            m_userEntry = result.GetDirectoryEntry();
                    }
                }
                catch
                {
                    m_userEntry = null;
                    throw;
                }
                finally
                {
                    EndImpersonation(currentContext);   // Undo impersonation if it was performed.
                }

                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="UserInfo"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="UserInfo"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();

                        if (m_userEntry != null)
                            m_userEntry.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Defines the credentials of a previleged domain account that can be used for impersonation prior to the 
        /// retrieval of user information from the Active Directory.
        /// </summary>
        /// <param name="domain">Domain of privileged domain user account.</param>
        /// <param name="username">Username of privileged domain user account.</param>
        /// <param name="password">Password of privileged domain user account.</param>
        /// <example>
        /// This example shows how to define the identity of the user to be used for retrieving information from Active Directory:
        /// <code>
        /// using System;
        /// using PCS.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         using (UserInfo user = new UserInfo("XYZCorp\\johndoe"))
        ///         {
        ///             // Define the identity to use for retrieving Active Directory information.
        ///             // Persist identity credentials encrypted to the config for easy access.
        ///             user.PersistSettings = true;
        ///             user.DefinePrivilegedAccount("XYZCorp", "admin", "Passw0rd");
        ///
        ///             // Retrieve and display user information from Active Directory.
        ///             Console.WriteLine(string.Format("First Name: {0}", user.FirstName));
        ///             Console.WriteLine(string.Format("Last Name: {0}", user.LastName));
        ///             Console.WriteLine(string.Format("Middle Initial: {0}", user.MiddleInitial));
        ///             Console.WriteLine(string.Format("Email Address: {0}", user.Email));
        ///             Console.WriteLine(string.Format("Telephone Number: {0}", user.Telephone));
        ///         }
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        ///</example>
        public void DefinePrivilegedAccount(string domain, string username, string password)
        {
            // Check input parameters.
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException("domain");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("userName");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            // Set the credentials for previleged domain user account.
            m_previlegedDomain = domain;
            m_previlegedUserName = username;
            m_previlegedPassword = password;
        }

        /// <summary>
        /// Returns the value for specified active directory property.
        /// </summary>
        /// <param name="propertyName">Name of the active directory property whose value is to be retrieved.</param>
        /// <returns>Value for the specified active directory property.</returns>
        public string GetUserProperty(string propertyName)
        {
            WindowsImpersonationContext currentContext = null;
            try
            {
                Initialize();   // Initialize if uninitialized.

                // Impersonate to the privileged account if specified.
                if (!string.IsNullOrEmpty(m_previlegedDomain) &&
                    !string.IsNullOrEmpty(m_previlegedUserName) &&
                    !string.IsNullOrEmpty(m_previlegedPassword))
                {
                    currentContext = ImpersonateUser(m_previlegedDomain,
                                                     m_previlegedUserName,
                                                     m_previlegedPassword);
                }

                return m_userEntry.Properties[propertyName][0].ToString().Replace("  ", " ").Trim();
            }
            catch
            {
                return "";
            }
            finally
            {
                EndImpersonation(currentContext);   // Undo impersonation if it was performed.
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="UserInfo"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["PrevilegedDomain", true].Update(m_previlegedDomain, "Domain of privileged domain user account.");
                settings["PrevilegedUserName", true].Update(m_previlegedUserName, "Username of privileged domain user account.");
                settings["PrevilegedPassword", true].Update(m_previlegedPassword, "Password of privileged domain user account.", true);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="UserInfo"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                m_previlegedDomain = settings["PrevilegedDomain", true].ValueAs(m_previlegedDomain);
                m_previlegedUserName = settings["PrevilegedUserName", true].ValueAs(m_previlegedUserName);
                m_previlegedPassword = settings["PrevilegedPassword", true].ValueAs(m_previlegedPassword);
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static UserInfo m_currentUserInfo;

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DuplicateToken(IntPtr ExistingTokenHandle, int SecurityImpersonationLevel, ref IntPtr DuplicateTokenHandle);

        // Static Properties

        /// <summary>
        /// Gets the <see cref="LoginID"/> of the current user.
        /// </summary>
        /// <remarks>
        /// The <see cref="LoginID"/> returned is that of the user account under which the code is executing.
        /// </remarks>
        public static string CurrentUserID
        {
            get
            {
                return WindowsIdentity.GetCurrent().Name;
            }
        }

        /// <summary>
        /// Gets the <see cref="UserInfo"/> object for the <see cref="CurrentUserID"/>.
        /// </summary>
        public static UserInfo CurrentUserInfo
        {
            get
            {
                if (m_currentUserInfo == null)
                    m_currentUserInfo = new UserInfo(CurrentUserID);

                return m_currentUserInfo;
            }
        }

        /// <summary>
        /// Gets the <see cref="LoginID"/> of the remote web user.
        /// </summary>
        /// <remarks>
        /// The <see cref="LoginID"/> returned is that of the remote user accessing the web application. This is 
        /// available only if the virtual directory hosting the web application is configured to use "Integrated 
        /// Windows Authentication".
        /// </remarks>
        public static string RemoteUserID
        {
            get
            {
                return Thread.CurrentPrincipal.Identity.Name;
            }
        }

        /// <summary>
        /// Gets the <see cref="UserInfo"/> object for the <see cref="RemoteUserID"/>.
        /// </summary>
        public static UserInfo RemoteUserInfo
        {
            get
            {
                string userID = RemoteUserID;
                if (userID == null)
                    return null;
                else
                    return new UserInfo(RemoteUserID);
            }
        }

        // Static Methods

        /// <summary>
        /// Authenticates the specified user credentials.
        /// </summary>
        /// <param name="domain">Domain of user to authenticate.</param>
        /// <param name="username">Username of user to authenticate.</param>
        /// <param name="password">Password of user to authenticate.</param>
        /// <returns>true if the user credentials are authenticated successfully; otherwise false.</returns>
        /// <example>
        /// This example shows how to validate a user's credentials:
        /// <code>
        /// using System;
        /// using PCS.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         string domain = "XYZCorp";
        ///         string username = "johndoe";
        ///         string password = "password";
        ///        
        ///         // Authenticate user credentials.
        ///         if (UserInfo.AuthenticateUser(domain, username, password))
        ///             Console.WriteLine("Successfully authenticated user \"{0}\\{1}\".", domain, username);
        ///         else
        ///             Console.WriteLine("Failed to authenticate user \"{0}\\{1}\".", domain, username);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool AuthenticateUser(string domain, string username, string password)
        {
            string errorMessage;
            return AuthenticateUser(domain, username, password, out errorMessage);
        }

        /// <summary>
        /// Authenticates the specified user credentials.
        /// </summary>
        /// <param name="domain">Domain of user to authenticate.</param>
        /// <param name="username">Username of user to authenticate.</param>
        /// <param name="password">Password of user to authenticate.</param>
        /// <param name="errorMessage">Error message returned, if authentication fails.</param>
        /// <returns>true if the user credentials are authenticated successfully; otherwise false.</returns>
        /// <example>
        /// This example shows how to validate a user's credentials and retrieve an error message if validation fails: 
        /// <code>
        /// using System;
        /// using PCS.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         string domain = "XYZCorp";
        ///         string username = "johndoe";
        ///         string password = "password";
        ///         string errorMessage;
        ///
        ///         // Authenticate user credentials.
        ///         if (UserInfo.AuthenticateUser(domain, username, password, out errorMessage))
        ///             Console.WriteLine("Successfully authenticated user \"{0}\\{1}\".", domain, username);
        ///         else
        ///             Console.WriteLine("Failed to authenticate user \"{0}\\{1}\" due to exception: {2}", domain, username, errorMessage);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool AuthenticateUser(string domain, string username, string password, out string errorMessage)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            bool authenticated;

            try
            {
                errorMessage = null;

                // Call LogonUser to attempt authentication
                authenticated = LogonUser(username, domain, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, out tokenHandle);

                if (!authenticated)
                    errorMessage = WindowsApi.GetLastErrorMessage();
            }
            finally
            {
                // Free the token
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);
            }

            return authenticated;
        }

        /// <summary>
        /// Impersonates the specified user.
        /// </summary>
        /// <param name="domain">Domain of user to impersonate.</param>
        /// <param name="username">Username of user to impersonate.</param>
        /// <param name="password">Password of user to impersonate.</param>
        /// <returns>A <see cref="WindowsImpersonationContext"/> object of the impersonated user.</returns>
        /// <remarks>After impersonating a user the code executes under the impersonated user's identity.</remarks>
        /// <example>
        /// This example shows how to impersonate a user:
        /// <code>
        /// using System;
        /// using PCS.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         Console.WriteLine(string.Format("User before impersonation: {0}", UserInfo.CurrentUserID));
        ///         UserInfo.ImpersonateUser("XYZCorp", "johndoe", "password"); // Impersonate user.
        ///         Console.WriteLine(string.Format("User after impersonation: {0}", UserInfo.CurrentUserID));
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static WindowsImpersonationContext ImpersonateUser(string domain, string username, string password)
        {
            WindowsImpersonationContext impersonatedUser;
            IntPtr tokenHandle = IntPtr.Zero;
            IntPtr dupeTokenHandle = IntPtr.Zero;

            try
            {
                // Calls LogonUser to obtain a handle to an access token.
                if (!LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHandle))
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\". {2}", domain, username, WindowsApi.GetLastErrorMessage()));

                if (!DuplicateToken(tokenHandle, SECURITY_IMPERSONATION, ref dupeTokenHandle))
                {
                    CloseHandle(tokenHandle);
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\". Exception thrown while trying to duplicate token.", domain, username));
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

        /// <summary>
        /// Ends the impersonation of the specified user.
        /// </summary>
        /// <param name="impersonatedUser"><see cref="WindowsImpersonationContext"/> of the impersonated user.</param>
        /// <example>
        /// This example shows how to terminate an active user impersonation:
        /// <code>
        /// using System;
        /// using System.IO;
        /// using System.Security.Principal;
        /// using PCS.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Impersonate user.
        ///         WindowsImpersonationContext context = UserInfo.ImpersonateUser("XYZCorp", "johndoe", "password");
        ///         // Perform operation requiring elevated previleges.
        ///         Console.WriteLine(File.ReadAllText(@"\\server\share\file.xml"));
        ///         // End the impersonation.
        ///         UserInfo.EndImpersonation(context);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static void EndImpersonation(WindowsImpersonationContext impersonatedUser)
        {
            if (impersonatedUser != null)
            {
                impersonatedUser.Undo();
                impersonatedUser.Dispose();
            }

            impersonatedUser = null;
        }

        #endregion
    }
}