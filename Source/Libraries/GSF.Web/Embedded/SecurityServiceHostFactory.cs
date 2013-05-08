//******************************************************************************************************
//  SecurityServiceHostFactory.cs - Gbtc
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
//  05/18/2010 - Pinal C. Patel
//       Generated original version of source code.
//  10/15/2010 - Pinal C. Patel
//       Updated to add a SOAP endpoint in addition to REST endpoint and enabled metadata publishing.
//  12/10/2010 - Pinal C. Patel
//       Updated SOAP endpoint to use BasicHttpBinding instead of WSHttpBinding which has security 
//       disabled by default and does not require SSL when security needs to be enabled on the client 
//       side when service is hosted in IIS with integrated security for intranet use.
//  01/07/2011 - Pinal C. Patel
//       Updated to sync endpoint security setting with that of the hosting environment so that the 
//       service can be used in both intranet and internet setting.
//  09/22/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using GSF.ServiceModel;

namespace GSF.Web.Embedded
{
    internal class SecurityServiceHostFactory : ServiceHostFactory
    {
        #region [ Methods ]

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            // Check security requirement.
            bool integratedSecurity = (Service.GetAuthenticationSchemes(baseAddresses[0]) & AuthenticationSchemes.Anonymous) != AuthenticationSchemes.Anonymous;

            // Initialize host and binding.
            ServiceHost host = new ServiceHost(serviceType, baseAddresses);

            // Enable metadata publishing.
            ServiceMetadataBehavior serviceBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (serviceBehavior == null)
            {
                serviceBehavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(serviceBehavior);
            }
            serviceBehavior.HttpGetEnabled = true;

            // Add REST endpoint.
            WebHttpBinding restBinding = new WebHttpBinding();
            WebHttpBehavior restBehavior = new WebHttpBehavior();
            ServiceEndpoint restEndpoint = host.AddServiceEndpoint(typeof(ISecurityService), restBinding, "rest");

            if (integratedSecurity)
            {
                // Enable security on the binding.
                restBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
                restBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }

#if !MONO
            restBehavior.HelpEnabled = true;
#endif
            restEndpoint.Behaviors.Add(restBehavior);

            // Add SOAP endpoint.
            BasicHttpBinding soapBinding = new BasicHttpBinding();
            if (integratedSecurity)
            {
                // Enable security on the binding.
                soapBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                soapBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            host.AddServiceEndpoint(typeof(ISecurityService), soapBinding, "soap");
            host.AddServiceEndpoint(typeof(IMetadataExchange), soapBinding, "soap/mex");

            return host;
        }

        #endregion
    }
}
