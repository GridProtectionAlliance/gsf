//******************************************************************************************************
//  SecurityProviderUtility.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/25/2010 - Pinal C. Patel
//       Generated original version of source code.
//  01/05/2011 - Pinal C. Patel
//       Added NotificationSmtpServer and NotificationSenderEmail settings to the config file along with
//       GeneratePassword() and SendNotification() utility methods.
//  01/24/2011 - Pinal C. Patel
//       Updated the logic in IsResourceAccessible() to stop looking at other included resources once a 
//       match is found for the resource being evaluated.
//  02/03/2011 - Pinal C. Patel
//       Updated the logic in IsResourceSecurable() and IsResourceAccessible() to allow for multiple
//       resources to be specified delimited by ',' with the same role requirements in the config file.
//  08/02/2011 - Pinal C. Patel
//       Modified IsResourceAccessible() to skip security check if no role is specified for a resource
//       in the config to allow security to be setup when accessing the resource but not enforced and 
//       leave it to the resource to enforce it.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Configuration;
using GSF.Net.Smtp;
using GSF.Security.Cryptography;

namespace GSF.Security
{
    /// <summary>
    /// A helper class containing methods used in the implementation of role-based security.
    /// </summary>
    public static class SecurityProviderUtility
    {
        #region [ Members ]

        //Constants
        private const string SettingsCategory = "SecurityProvider";
        private const string DefaultProviderType = "GSF.Security.LdapSecurityProvider, GSF.Security";
        private const string DefaultIncludedResources = "*=*";
        private const string DefaultExcludedResources = "";
        private const string DefaultNotificationSmtpServer = Mail.DefaultSmtpServer;
        private const string DefaultNotificationSenderEmail = "sender@company.com";

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly string s_providerType;
        private static readonly ICollection<string> s_excludedResources;
        private static readonly IDictionary<string, string> s_includedResources;
        private static readonly string s_notificationSmtpServer;
        private static readonly string s_notificationSenderEmail;

        private static Func<string, ISecurityProvider> s_providerFactory;

        // Static Constructor
        static SecurityProviderUtility()
        {
            // Load settings from config file.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            settings.Add("ProviderType", DefaultProviderType, "The type to be used for enforcing security.");
            settings.Add("IncludedResources", DefaultIncludedResources, "Semicolon delimited list of resources to be secured along with role names.");
            settings.Add("ExcludedResources", DefaultExcludedResources, "Semicolon delimited list of resources to be excluded from being secured.");
            settings.Add("NotificationSmtpServer", DefaultNotificationSmtpServer, "SMTP server to be used for sending out email notification messages.");
            settings.Add("NotificationSenderEmail", DefaultNotificationSenderEmail, "Email address of the sender of email notification messages.");

            s_providerType = settings["ProviderType"].ValueAsString(DefaultProviderType);
            s_includedResources = settings["IncludedResources"].ValueAsString().ParseKeyValuePairs();
            s_excludedResources = settings["ExcludedResources"].ValueAsString().Split(';');
            s_notificationSmtpServer = settings["NotificationSmtpServer"].ValueAsString();
            s_notificationSenderEmail = settings["NotificationSenderEmail"].ValueAsString();
        }

        // Static Properties

        private static Func<string, ISecurityProvider> ProviderFactory
        {
            get
            {
                return s_providerFactory ?? (s_providerFactory = CreateSecurityProviderFactory());
            }
        }

        // Static Methods

        /// <summary>
        /// Creates a new <see cref="ISecurityProvider"/> based on the settings in the config file.
        /// </summary>
        /// <param name="username">Username of the user for whom the <see cref="ISecurityProvider"/> is to be created.</param>
        /// <returns>An object that implements <see cref="ISecurityProvider"/>.</returns>
        public static ISecurityProvider CreateProvider(string username)
        {
            // Initialize the username
            if (string.IsNullOrEmpty(username))
                username = Thread.CurrentPrincipal.Identity.Name;

            // If an application is being launched from an installer it will have the NT AUTHORITY\System Identity which
            // will not have available user information - so we pickup username from Environment instead
            if (username.StartsWith("NT AUTHORITY\\", StringComparison.OrdinalIgnoreCase))
                username = Environment.UserDomainName + "\\" + Environment.UserName;

            // Instantiate the provider
            // ReSharper disable once AssignNullToNotNullAttribute
            ISecurityProvider provider = ProviderFactory(username);

            // Initialize the provider
            provider.LoadSettings();

            if (provider.CanRefreshData)
                provider.RefreshData();

            // Return initialized provider
            return provider;
        }

        /// <summary>
        /// Creates a new <see cref="ISecurityProvider"/> based on the settings in the config file.
        /// </summary>
        /// <param name="userData">Object that contains data about the user to be used by the security provider.</param>
        /// <returns>An object that implements <see cref="ISecurityProvider"/>.</returns>
        public static ISecurityProvider CreateProvider(UserData userData)
        {
            // Initialize the username
            string username = userData.Username;

            // Instantiate the provider
            // ReSharper disable once AssignNullToNotNullAttribute
            ISecurityProvider provider = ProviderFactory(username);

            // Initialize the provider
            provider.LoadSettings();
            provider.UserData.Clone(userData);

            // Return initialized provider
            return provider;
        }

        /// <summary>
        /// Determines if the specified <paramref name="resource"/> is to be secured based on settings in the config file.
        /// </summary>
        /// <param name="resource">Name of the resource to be checked.</param>
        /// <returns>true if the <paramref name="resource"/> is to be secured; otherwise false/</returns>
        public static bool IsResourceSecurable(string resource)
        {
            // Check if resource is excluded explicitly.
            foreach (string exclusion in s_excludedResources)
            {
                if (IsRegexMatch(exclusion, resource))
                    return false;
            }

            // Check if resource is included explicitly.
            foreach (KeyValuePair<string, string> inclusion in s_includedResources)
            {
                foreach (string item in inclusion.Key.Split(','))
                {
                    if (IsRegexMatch(item.Trim(), resource))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the current user, as defined by the <paramref name="principal"/>, has permission to access 
        /// the specified <paramref name="resource"/> based on settings in the config file.
        /// </summary>
        /// <param name="resource">Name of the resource to be checked.</param>
        /// <param name="principal">The principal providing the security context for the user.</param>
        /// <returns>true if the current user has permission to access the <paramref name="resource"/>; otherwise false.</returns>
        public static bool IsResourceAccessible(string resource, IPrincipal principal)
        {
            // Check if the resource has a role-based access restriction on it.
            foreach (KeyValuePair<string, string> inclusion in s_includedResources)
            {
                foreach (string item in inclusion.Key.Split(','))
                {
                    if (IsRegexMatch(item.Trim(), resource))
                    {
                        // Allow security to be implemented inside the resource.
                        if (string.IsNullOrEmpty(inclusion.Value))
                            return true;

                        // Check resource role requirements against user's role subscription.
                        return principal.IsInRole(inclusion.Value);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the specified <paramref name="target"/> matches the specified <paramref name="spec"/>.
        /// </summary>
        /// <param name="spec">Spec string that can include wildcards ('*'). For example, *.txt</param>
        /// <param name="target">Target string to be compared with the <paramref name="spec"/>.</param>
        /// <returns>true if the <paramref name="target"/> matches the <paramref name="spec"/>, otherwise false.</returns>
        public static bool IsRegexMatch(string spec, string target)
        {
            spec = spec.Replace(".", "\\.");    // Escape special regex character '.'.
            spec = spec.Replace("?", "\\?");    // Escape special regex character '?'.
            spec = spec.Replace("*", ".*");     // Convert '*' to its regex equivalent.

            // Perform a case-insensitive regex match.
            return Regex.IsMatch(target, string.Format("^{0}$", spec), RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Encrypts the password to a one-way hash using the SHA1 hash algorithm.
        /// </summary>
        /// <param name="password">Password to be encrypted.</param>
        /// <returns>Encrypted password.</returns>
        public static string EncryptPassword(string password)
        {
            return Cipher.GetPasswordHash(password);
        }

        /// <summary>
        /// Generates a random password of the specified <paramref name="length"/> with at least one uppercase letter, one lowercase letter, one special character and one digit.
        /// </summary>
        /// <param name="length">Length of the password to generate.</param>
        /// <returns>Randomly generated password of the specified <paramref name="length"/>.</returns>
        /// <exception cref="ArgumentException">A value of less than 8 is specified for the <paramref name="length"/>.</exception>
        public static string GeneratePassword(int length)
        {
            if (length < 8)
                throw new ArgumentException("Value must be at least 8", nameof(length));

            return PasswordGenerator.Default.GeneratePassword(length);
        }

        /// <summary>
        /// Sends email notification message to the specified <paramref name="recipient"/> using settings specified in the config file.
        /// </summary>
        /// <param name="recipient">Email address of the notification recipient.</param>
        /// <param name="subject">Subject of the notification.</param>
        /// <param name="body">Content of the notification.</param>
        public static void SendNotification(string recipient, string subject, string body)
        {
            Mail.Send(s_notificationSenderEmail, recipient, subject, body, false, s_notificationSmtpServer);
        }

        private static Func<string, ISecurityProvider> CreateSecurityProviderFactory()
        {
            Type providerType = Type.GetType(s_providerType);

            if ((object)providerType == null)
                throw new InvalidOperationException("The default security provider type defined by the system does not exist.");

            ConstructorInfo constructor = Type.GetType(s_providerType).GetConstructor(new Type[] { typeof(string) });

            if ((object)constructor == null)
                throw new InvalidOperationException("The default security provider type does not define a constructor with the appropriate signature.");

            ParameterExpression parameterExpression = Expression.Parameter(typeof(string));
            NewExpression newExpression = Expression.New(constructor, parameterExpression);
            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<string, ISecurityProvider>), newExpression, parameterExpression);

            return (Func<string, ISecurityProvider>)lambdaExpression.Compile();
        }

        #endregion
    }
}
