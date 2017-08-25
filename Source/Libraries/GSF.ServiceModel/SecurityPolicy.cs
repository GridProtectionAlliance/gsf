//******************************************************************************************************
//  SecurityPolicy.cs - Gbtc
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
//  04/22/2010 - Pinal C. Patel
//       Generated original version of source code.
//  05/27/2010 - Pinal C. Patel
//       Added usage example to code comments.
//       Modified Evaluate() to make use of IncludedResources and ExcludedResources config file settings.
//  06/02/2010 - Pinal C. Patel
//       Added authentication check to Evaluate() method.
//  08/11/2010 - Pinal C. Patel
//       Made key methods virtual for extensibility.
//  10/14/2010 - Pinal C. Patel
//       Modified GetResourceName() to return path and query for consistency across WCF hosting 
//       environments (i.e. ASP.NET, WAS, etc.).
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using GSF.Security;

namespace GSF.ServiceModel
{
    /// <summary>
    /// Represents an <see cref="IAuthorizationPolicy">authorization policy</see> that can be used by WCF services for enabling role-based security.
    /// </summary>
    /// <example>
    /// Common config file entries:
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
    ///       <add name="IncludedResources" value="*/*.*=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="*/SecurityService.svc*"
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
    /// Internal WCF service configuration:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <system.serviceModel>
    ///     <services>
    ///       <service name="WcfService1.Service1" behaviorConfiguration="serviceBehavior">
    ///         <endpoint address="" contract="WcfService1.IService1" binding="webHttpBinding" 
    ///                   bindingConfiguration="endpointBinding" behaviorConfiguration="endpointBehavior" />
    ///       </service>
    ///     </services>
    ///     <behaviors>
    ///       <endpointBehaviors>
    ///         <behavior name="endpointBehavior">
    ///           <webHttp/>
    ///         </behavior>
    ///       </endpointBehaviors>
    ///       <serviceBehaviors>
    ///         <behavior name="serviceBehavior">
    ///           <serviceAuthorization principalPermissionMode="Custom">
    ///             <authorizationPolicies>
    ///               <add policyType="GSF.ServiceModel.SecurityPolicy, GSF.ServiceModel" />
    ///             </authorizationPolicies>
    ///           </serviceAuthorization>
    ///         </behavior>
    ///       </serviceBehaviors>
    ///     </behaviors>
    ///     <bindings>
    ///       <webHttpBinding>
    ///         <binding name="endpointBinding">
    ///           <security mode="TransportCredentialOnly">
    ///             <transport clientCredentialType="Windows"/>
    ///           </security>
    ///         </binding>
    ///       </webHttpBinding>
    ///     </bindings>
    ///     <serviceHostingEnvironment aspNetCompatibilityEnabled="false" />
    ///   </system.serviceModel>
    /// </configuration>
    /// ]]>
    /// </code>
    /// External WCF service configuration:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <system.web>
    ///     <httpModules>
    ///       <add name="SecurityModule" type="GSF.ServiceModel.SecurityModule, GSF.ServiceModel" />
    ///     </httpModules>
    ///   </system.web>
    ///   <system.serviceModel>
    ///     <services>
    ///       <service name="WcfService1.Service1" behaviorConfiguration="serviceBehavior">
    ///         <endpoint address="" contract="WcfService1.IService1" binding="webHttpBinding" 
    ///                   bindingConfiguration="endpointBinding" behaviorConfiguration="endpointBehavior"/>
    ///       </service>
    ///     </services>
    ///     <behaviors>
    ///       <endpointBehaviors>
    ///         <behavior name="endpointBehavior">
    ///           <webHttp/>
    ///         </behavior>
    ///       </endpointBehaviors>
    ///       <serviceBehaviors>
    ///         <behavior name="serviceBehavior">
    ///           <serviceAuthorization principalPermissionMode="Custom">
    ///             <authorizationPolicies>
    ///               <add policyType="GSF.ServiceModel.SecurityPolicy, GSF.ServiceModel" />
    ///             </authorizationPolicies>
    ///           </serviceAuthorization>
    ///         </behavior>
    ///       </serviceBehaviors>
    ///     </behaviors>
    ///     <bindings>
    ///       <webHttpBinding>
    ///         <binding name="endpointBinding">
    ///           <security mode="None" />
    ///         </binding>
    ///       </webHttpBinding>
    ///     </bindings>
    ///     <serviceHostingEnvironment aspNetCompatibilityEnabled="true" />
    ///   </system.serviceModel>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ISecurityProvider"/>
    public class SecurityPolicy : IAuthorizationPolicy
    {
        #region [ Members ]

        // Fields
        private Guid m_id;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPolicy"/> class.
        /// </summary>
        public SecurityPolicy()
        {
            m_id = Guid.NewGuid();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the identifier of this <see cref="SecurityPolicy"/> instance.
        /// </summary>
        public string Id
        {
            get
            {
                return m_id.ToString();
            }
        }

        /// <summary>
        /// Gets a claim set that represents the issuer of this <see cref="SecurityPolicy"/>.
        /// </summary>
        public ClaimSet Issuer
        {
            get
            {
                return ClaimSet.System;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Evaluates the <paramref name="evaluationContext"/> and initializes security.
        /// </summary>
        /// <param name="evaluationContext">An <see cref="EvaluationContext"/> object.</param>
        /// <param name="state">Custom state of the <see cref="SecurityPolicy"/>.</param>
        /// <returns></returns>
        public virtual bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            // In order for this to work properly security on the binding must be configured to use windows security.
            // When this is done the caller's windows identity is available to us here and can be used to derive from 
            // it the security principal that can be used by WCF service code downstream for implementing security.
            object property;

            if (evaluationContext.Properties.TryGetValue("Identities", out property))
            {
                // Extract and assign the caller's windows identity to current thread if available.
                IList<IIdentity> identities = property as List<IIdentity>;

                if ((object)identities == null)
                    throw new SecurityException(string.Format("Null Identities in Evaluation Context for '{0}'", Thread.CurrentPrincipal.Identity));

                foreach (IIdentity identity in identities)
                {
                    if (identity is WindowsIdentity)
                    {
                        Thread.CurrentPrincipal = new WindowsPrincipal((WindowsIdentity)identity);
                        break;
                    }
                }
            }

            string resource = GetResourceName();

            if (SecurityProviderUtility.IsResourceSecurable(resource))
            {
                // Initialize the security principal from caller's windows identity if uninitialized.
                ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(Thread.CurrentPrincipal.Identity.Name);
                securityProvider.PassthroughPrincipal = Thread.CurrentPrincipal;
                securityProvider.Authenticate();

                // Set up the security principal to provide role-based security.
                SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                Thread.CurrentPrincipal = new SecurityPrincipal(securityIdentity);

                // Setup the principal to be attached to the thread on which WCF service will execute.
                evaluationContext.Properties["Principal"] = Thread.CurrentPrincipal;

                // Verify that the current thread principal has been authenticated.
                if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    throw new SecurityException(string.Format("Authentication failed for user '{0}'", Thread.CurrentPrincipal.Identity.Name));

                // Perform a top-level permission check on the resource being accessed.
                if (!SecurityProviderUtility.IsResourceAccessible(resource, Thread.CurrentPrincipal))
                    throw new SecurityException(string.Format("Access to '{0}' is denied", resource));

                return true;
            }

            // Setup the principal to be attached to the thread on which WCF service will execute.
            evaluationContext.Properties["Principal"] = Thread.CurrentPrincipal;

            return true;
        }

        /// <summary>
        /// Gets the name of resource being accessed.
        /// </summary>
        /// <returns><see cref="OperationContext.IncomingMessageHeaders"/>.<see cref="System.ServiceModel.Channels.MessageHeaders.To"/>.<see cref="Uri.PathAndQuery"/> property value.</returns>
        protected virtual string GetResourceName()
        {
            return OperationContext.Current.IncomingMessageHeaders.To.PathAndQuery;
        }

        #endregion
    }
}
