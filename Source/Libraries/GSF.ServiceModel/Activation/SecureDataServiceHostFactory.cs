//******************************************************************************************************
//  SecureDataServiceHostFactory.cs - Gbtc
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
//  06/09/2011 - Pinal C. Patel
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  01/12/2015 - Pinal C. Patel
//       Added AuthorizationPolicy property to allow customization of IAuthorizationPolicy used for 
//       enabling security.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data.Services;
using System.IdentityModel.Policy;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace GSF.ServiceModel.Activation
{
    /// <summary>
    /// A service host factory for WCF Data Services that enables role-based security using <see cref="IAuthorizationPolicy"/>.
    /// </summary>
    /// <see cref="SecurityPolicy"/>
    public class SecureDataServiceHostFactory : DataServiceHostFactory
    {
        #region [ Members ]

        // Fields
        private Type m_authorizationPolicy;

        private List<IEndpointBehavior> m_endpointBehaviors;

        private List<IServiceBehavior> m_serviceBehaviors;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureDataServiceHostFactory"/> class.
        /// </summary>
        public SecureDataServiceHostFactory()
        {
            m_authorizationPolicy = typeof(SecurityPolicy);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureDataServiceHostFactory"/> class.
        /// </summary>
        /// <param name="serviceBehaviors">A collection of service behaviors that will be applied to newly created services.</param>
        /// <param name="endpointBehaviors">A collection of endpoint behaviors that will be applied to newly created endpoints.</param>
        public SecureDataServiceHostFactory(List<IServiceBehavior> serviceBehaviors, List<IEndpointBehavior> endpointBehaviors)
            : this()
        {
            m_endpointBehaviors = endpointBehaviors;
            m_serviceBehaviors = serviceBehaviors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureDataServiceHostFactory"/> class.
        /// </summary>
        /// <param name="behaviors">A collection of endpoint behaviors that will be applied to newly created endpoints.</param>
        public SecureDataServiceHostFactory(List<IEndpointBehavior> behaviors)
            : this(new List<IServiceBehavior>(), behaviors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureDataServiceHostFactory"/> class.
        /// </summary>
        /// <param name="behaviors">A collection of service behaviors that will be applied to newly created services.</param>
        public SecureDataServiceHostFactory(List<IServiceBehavior> behaviors)
            : this(behaviors, new List<IEndpointBehavior>())
        {
        }

        #endregion

        #region [ Properties ]

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

        /// <summary>
        /// Gets or sets the list of endpoint behaviors applied to endpoints created for the service.
        /// </summary>
        public List<IEndpointBehavior> EndpointBehaviors
        {
            get
            {
                return m_endpointBehaviors;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                m_endpointBehaviors = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="IServiceBehavior"/> applied to the service.
        /// </summary>
        /// <exception cref="ArgumentNullException">Assigned value cannot be null</exception>
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
                    throw new ArgumentNullException(nameof(value));
                }
                m_serviceBehaviors = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a new <see cref="DataServiceHost"/> from the URI.
        /// </summary>
        /// <param name="serviceType">Specifies the type of WCF service to host.</param>
        /// <param name="baseAddresses">An array of base addresses for the service.</param>
        /// <returns>New <see cref="DataServiceHost"/>.</returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            // Create data service host.
            ServiceHost host = base.CreateServiceHost(serviceType, baseAddresses);

            // Enable security on the data service.
            ServiceAuthorizationBehavior serviceBehavior = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
            if (serviceBehavior == null)
            {
                serviceBehavior = new ServiceAuthorizationBehavior();
                host.Description.Behaviors.Add(serviceBehavior);
            }
            serviceBehavior.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add((IAuthorizationPolicy)Activator.CreateInstance(m_authorizationPolicy));
            serviceBehavior.ExternalAuthorizationPolicies = policies.AsReadOnly();

            foreach (var behavior in m_serviceBehaviors ?? new List<IServiceBehavior>())
            {
                host.Description.Behaviors.Add(behavior);
            }

            foreach (var endpoint in host.Description.Endpoints)
            {
                foreach (var behavior in m_endpointBehaviors ?? new List<IEndpointBehavior>())
                {
                    endpoint.EndpointBehaviors.Add(behavior);
                }
            }

            return host;
        }

        #endregion
    }
}
