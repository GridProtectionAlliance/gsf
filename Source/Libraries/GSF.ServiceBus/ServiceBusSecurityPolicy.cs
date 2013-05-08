//******************************************************************************************************
//  ServiceBusSecurityPolicy.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/18/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.IdentityModel.Policy;
using System.ServiceModel;
using System.Text.RegularExpressions;
using GSF.ServiceModel;

namespace GSF.ServiceBus
{
    /// <summary>
    /// Represents an <see cref="IAuthorizationPolicy">authorization policy</see> that can be used for enabling role-based security on <see cref="ServiceBusService"/>.
    /// </summary>
    /// <example>
    /// This example shows the required config file entries when securing <see cref="ServiceBusService"/>:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ApplicationName" value="" description="Name of the application being secured as defined in the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ConnectionString" value="" description="Connection string to be used for connection to the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ProviderType" value="GSF.Security.LdapSecurityProvider, GSF.Security"
    ///         description="The type to be used for enforcing security." encrypted="false" />
    ///       <add name="IncludedResources" value="Topic.Name=*;Queue.Name=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="*/mex;*/Publish;*/GetClients;*/GetQueues;*/GetTopics;*/SecurityService.svc/*"
    ///         description="Semicolon delimited list of resources to be excluded from being secured."
    ///         encrypted="false" />
    ///       <add name="NotificationSmtpServer" value="localhost" description="SMTP server to be used for sending out email notification messages."
    ///         encrypted="false" />
    ///       <add name="NotificationSenderEmail" value="sender@company.com" description="Email address of the sender of email notification messages." 
    ///         encrypted="false" />
    ///     </securityProvider>
    ///     <activeDirectory>
    ///       <add name="PrivilegedDomain" value="" description="Domain of privileged domain user account."
    ///         encrypted="false" />
    ///       <add name="PrivilegedUserName" value="" description="Username of privileged domain user account."
    ///         encrypted="false" />
    ///       <add name="PrivilegedPassword" value="" description="Password of privileged domain user account."
    ///         encrypted="true" />
    ///     </activeDirectory>
    ///   </categorizedSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ServiceBusService"/>
    public class ServiceBusSecurityPolicy : SecurityPolicy
    {
        #region [ Members ]

        // Constants
        private const string NameRegex = @"<(?'Name'b:MessageName)\b[^>]*>(?'Value'.*?)</\1>";

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the name of resource being accessed.
        /// </summary>
        /// <returns>
        /// <see cref="Message"/>.<see cref="Message.Name"/> property value for <see cref="ServiceBusService.Publish"/> operations, 
        /// otherwise <see cref="System.ServiceModel.Channels.MessageHeaders.Action"/> property value for all other operations.
        /// </returns>
        protected override string GetResourceName()
        {
            System.ServiceModel.Channels.Message message = OperationContext.Current.RequestContext.RequestMessage;
            if (message.Headers.Action.EndsWith("Register"))
            {
                // For register operation use the topic and queue name for applying security.
                MatchCollection matches = Regex.Matches(message.ToString(), NameRegex);
                if (matches.Count > 0)
                    return matches[0].Groups["Value"].Value;
            }

            // For all other operations simply use the operation name for applying security.
            return OperationContext.Current.RequestContext.RequestMessage.Headers.Action;
        }

        #endregion
    }
}
