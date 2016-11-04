//******************************************************************************************************
//  SecureServiceHostFactory.cs - Gbtc
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
//  10/06/2011 - Pinal C. Patel
//       Generated original version of source code.
//  10/12/2011 - Pinal C. Patel
//       Added the ability to specify endpoint binding information.
//  11/16/2011 - Pinal C. Patel
//       Added PublishMetadata and DisableSecurity properties.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  01/12/2015 - Pinal C. Patel
//       Added AuthorizationPolicy property to allow customization of IAuthorizationPolicy used for 
//       enabling security.
//
//******************************************************************************************************

using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
#if !MONO
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
#endif

namespace GSF.ServiceModel.Activation
{
    /// <summary>
    /// A service host factory for WCF Services that enables role-based security using <see cref="IAuthorizationPolicy"/>.
    /// </summary>
    /// <see cref="SecurityPolicy"/>
    public class SecureServiceHostFactory : ServiceHostFactory
    {
        #region [ Members ]

        // Fields
        private string m_protocol;
        private readonly string m_address;
        private bool m_publishMetadata;
        private bool m_disableSecurity;
        private Type m_authorizationPolicy;
#if !MONO
        private List<IEndpointBehavior> m_endpointBehaviors;
        private List<IServiceBehavior> m_serviceBehaviors;
#endif

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureServiceHostFactory"/> class.
        /// </summary>
        public SecureServiceHostFactory()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureServiceHostFactory"/> class.
        /// </summary>
        /// <param name="protocol">Protocol used by the service.</param>
        public SecureServiceHostFactory(string protocol)
            : this(protocol, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureServiceHostFactory"/> class.
        /// </summary>
        /// <param name="protocol">Protocol used by the service.</param>
        /// <param name="address">Address of the service.</param>
        public SecureServiceHostFactory(string protocol, string address)
#if !MONO
            : this(protocol, address, new List<IEndpointBehavior>(), new List<IServiceBehavior>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureServiceHostFactory"/> class.
        /// </summary>
        /// <param name="protocol">Protocol used by the service.</param>
        /// <param name="address">Address of the service.</param>
        /// <param name="endpointBehaviors">Endpoint behaviors to be added to the created service.</param>
        /// <param name="serviceBehaviors">Service behaviors to be added to the created service.</param>
        public SecureServiceHostFactory(string protocol, string address, List<IEndpointBehavior> endpointBehaviors, List<IServiceBehavior> serviceBehaviors)
#endif
        {
            m_protocol = protocol;
            m_address = address;
            m_publishMetadata = true;
            m_authorizationPolicy = typeof(SecurityPolicy);
#if !MONO
            m_endpointBehaviors = endpointBehaviors;
            m_serviceBehaviors = serviceBehaviors;
#endif
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether service meta-data is to be published.
        /// </summary>
        public bool PublishMetadata
        {
            get
            {
                return m_publishMetadata;
            }
            set
            {
                m_publishMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether security on the service is to be disabled. 
        /// </summary>
        public bool DisableSecurity
        {
            get
            {
                return m_disableSecurity;
            }
            set
            {
                m_disableSecurity = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAuthorizationPolicy"/> used for securing the service.
        /// </summary>
        public Type AuthorizationPolicy
        {
            get
            {
                return m_authorizationPolicy;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                m_authorizationPolicy = value;
            }
        }

#if !MONO

        /// <summary>
        /// Gets or sets the list of endpoint behaviors added to the services created
        /// </summary>
        public List<IEndpointBehavior> EndpointBehaviors
        {
            get
            {
                return m_endpointBehaviors;
            }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("Behaviors value");
                }
                m_endpointBehaviors = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of service behaviors added to the services created
        /// </summary>
        public List<IServiceBehavior> ServiceBehaviors
        {
            get
            {
                return m_serviceBehaviors;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Behaviors value");
                }
                m_serviceBehaviors = value;
            }
        }

#endif

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a new <see cref="ServiceHost"/> from the URI.
        /// </summary>
        /// <param name="serviceType">Specifies the type of WCF service to host.</param>
        /// <param name="baseAddresses">An array of base addresses for the service.</param>
        /// <returns>New <see cref="ServiceHost"/>.</returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
#if MONO
            throw new NotSupportedException("Not supported under Mono.");
#else
            // Check security requirement.
            bool integratedSecurity = (Service.GetAuthenticationSchemes(baseAddresses[0]) & AuthenticationSchemes.Anonymous) != AuthenticationSchemes.Anonymous;

            // Create service host.
            ServiceHost host = base.CreateServiceHost(serviceType, baseAddresses);

            // Enable meta-data publishing.
            if (m_publishMetadata)
            {
                ServiceMetadataBehavior metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();

                if (metadataBehavior == null)
                {
                    metadataBehavior = new ServiceMetadataBehavior();
                    host.Description.Behaviors.Add(metadataBehavior);
                }

                metadataBehavior.HttpGetEnabled = true;
            }

            // Enable security on the service.
            if (!m_disableSecurity)
            {
                ServiceAuthorizationBehavior authorizationBehavior = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();

                if (authorizationBehavior == null)
                {
                    authorizationBehavior = new ServiceAuthorizationBehavior();
                    host.Description.Behaviors.Add(authorizationBehavior);
                }

                authorizationBehavior.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
                policies.Add((IAuthorizationPolicy)Activator.CreateInstance(m_authorizationPolicy));
                authorizationBehavior.ExternalAuthorizationPolicies = policies.AsReadOnly();
            }

            // Create endpoint and configure security. (Not supported on Mono)
            host.AddDefaultEndpoints();

            if (string.IsNullOrEmpty(m_protocol))
            {
                // Use the default endpoint.
                foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                {
                    BasicHttpBinding basicBinding = endpoint.Binding as BasicHttpBinding;
                    if (basicBinding != null)
                    {
                        // Default endpoint uses BasicHttpBinding.
                        if (integratedSecurity)
                        {
                            // Enable security.
                            basicBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                            basicBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                        }
                        else
                        {
                            // Disable security.
                            basicBinding.Security.Mode = BasicHttpSecurityMode.None;
                            basicBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                        }
                        foreach(IEndpointBehavior behavior in m_endpointBehaviors ?? new List<IEndpointBehavior>())
                        {
                            endpoint.Behaviors.Add(behavior);
                        }
                    }
                }
            }
            else
            {
                // Create endpoint using the specifics.
                host.Description.Endpoints.Clear();

                Binding serviceBinding;
                ServiceEndpoint serviceEndpoint;
                serviceBinding = Service.CreateServiceBinding(ref m_protocol, integratedSecurity);

                if (serviceBinding != null)
                {
                    // Binding created for the endpoint.
                    Type contract = Service.GetServiceContract(serviceType);
                    if (!string.IsNullOrEmpty(m_address))
                        serviceEndpoint = host.AddServiceEndpoint(contract, serviceBinding, m_address);
                    else
                        serviceEndpoint = host.AddServiceEndpoint(contract, serviceBinding, string.Empty);

                    // Special handling for REST endpoint.
                    if (serviceBinding is WebHttpBinding)
                    {
                        WebHttpBehavior restBehavior = new WebHttpBehavior();
                        //#if !MONO
                        if (m_publishMetadata)
                            restBehavior.HelpEnabled = true;
                        //#endif
                        serviceEndpoint.Behaviors.Add(restBehavior);
                        serviceEndpoint.Behaviors.Add(new FormatSpecificationBehavior());
                    }

                    foreach (IEndpointBehavior behavior in m_endpointBehaviors ?? new List<IEndpointBehavior>())
                    {
                        serviceEndpoint.Behaviors.Add(behavior);
                    }
                }
            }

            foreach(var behavior in ServiceBehaviors ?? new List<IServiceBehavior>())
            {
                host.Description.Behaviors.Add(behavior);
            }

            return host;
#endif
        }

        #endregion
    }
}