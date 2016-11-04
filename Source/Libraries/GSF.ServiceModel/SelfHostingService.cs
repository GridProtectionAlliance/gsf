//******************************************************************************************************
//  SelfHostingService.cs - Gbtc
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
//  08/27/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/02/2009 - Pinal C. Patel
//       Modified configuration of the default WebHttpBinding to enable receiving of large payloads.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/27/2009 - Pinal C. Patel
//       Fixed issue in the initialization of service contract name.
//  03/30/2010 - Pinal C. Patel
//       Updated CanRead and CanWrite to not include Enabled in its evaluation.
//  05/28/2010 - Pinal C. Patel
//       Added an endpoint for web service help.
//  06/21/2010 - Pinal C. Patel
//       Added Singleton property for added control over the hosting process.
//  10/08/2010 - Pinal C. Patel
//       Removed REST web service help endpoint since a similar feature is now part of WCF 4.0.
//  10/14/2010 - Pinal C. Patel
//       Made changes for hosting flexibility and enabling security:
//         Deleted DataFlow since access restriction can now be imposed by enabling security.
//         Added SecurityPolicy and PublishMetadata.
//         Renamed ServiceUri to Endpoints and ServiceContract to Contract.
//  10/26/2010 - Pinal C. Patel
//       Modified the implementation of ISupportLifecycle.Enabled property.
//  10/29/2010 - Pinal C. Patel
//       Modified CreateServiceBinding() to explicitly disable security on created binding if specified.
//  11/19/2010 - Pinal C. Patel
//       Changed to inherit from Adapter to take advantage of app domain isolation through AdapterLoader.
//  12/09/2010 - Pinal C. Patel
//       Modified GetUnusedPort() to use GSF.Security.Cryptography.Random instead of System.Random to
//       unsure unique numbers.
//  11/29/2011 - Pinal C. Patel
//       Modified InitializeServiceHost() to not use a random port number for the service host address
//       so that setting up security on the address is possible.
//  08/02/2012 - J. Ritchie Carroll
//       Added cross-domain access support for Silverlight and Flash application using self hosted
//       web services (enables access to clientaccesspolicy.xml and crossdomain.xml).
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  12/11/2013 - Pinal C. Patel
//       Added WindowsAuthentication property to allow Windows Authentication to be enabled explicitly
//       even if SecurityPolicy is not configured to allow for more flexibility.
//  09/28/2016 - J. Ritchie Carroll
//       Added AutomaticFormatSelectionEnabled property to allow for non-XML based, e.g., JSON,
//       formats for exception messages.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using GSF.Adapters;
using GSF.Configuration;
using Random = GSF.Security.Cryptography.Random;

namespace GSF.ServiceModel
{
    /// <summary>
    /// A base class for web service that can send and receive data over REST (Representational State Transfer) interface.
    /// </summary>
    /// <example>
    /// This example shows how to create a WCF service derived from <see cref="SelfHostingService"/> that is capable of hosting itself:
    /// <code>
    /// using System.ServiceModel;
    /// using System.ServiceModel.Web;
    /// using GSF.ServiceModel;
    /// 
    /// namespace Services
    /// {
    ///     [ServiceContract()]
    ///     public interface IService : ISelfHostingService
    ///     {
    ///         [OperationContract(), WebGet(UriTemplate = "/hello/{name}")]
    ///         string Hello(string name);
    ///     }
    /// 
    ///     public class Service : SelfHostingService, IService
    ///     {
    ///         public string Hello(string name)
    ///         {
    ///             return string.Format("Hello {0}!", name);
    ///         }
    ///     }
    /// }
    /// </code>
    /// This example shows how to activate a WCF service derived from <see cref="SelfHostingService"/> that is capable of hosting itself:
    /// <code>
    /// using System;
    /// using System.ServiceModel;
    /// using System.ServiceModel.Description;
    /// using System.ServiceModel.Web;
    /// using Services;
    /// using GSF.ServiceModel;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize web service.
    ///         Service service = new Service();
    ///         service.PublishMetadata = true;
    ///         service.Endpoints = "http.soap11://localhost:4500/soap; http.rest://localhost:4500/rest";
    ///         service.Initialize();
    /// 
    ///         // Show web service status.
    ///         if (service.ServiceHost.State == CommunicationState.Opened)
    ///         {
    ///             Console.WriteLine("\r\n{0} is running:", service.GetType().Name);
    ///             foreach (ServiceEndpoint endpoint in service.ServiceHost.Description.Endpoints)
    ///             {
    ///                 Console.WriteLine("- {0} ({1})", endpoint.Address, endpoint.Binding.GetType().Name);
    ///             }
    ///             Console.Write("\r\nPress Enter key to stop...");
    ///         }
    ///         else
    ///         {
    ///             Console.WriteLine("\r\n{0} could not be started", service.GetType().Name);
    ///         }
    /// 
    ///         // Shutdown.
    ///         Console.ReadLine();
    ///         service.Dispose();
    ///     }
    /// }
    /// </code>
    /// This example shows how to host a WCF service derived from <see cref="SelfHostingService"/> inside ASP.NET:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <system.serviceModel>
    ///     <services>
    ///       <service name="Services.Service">
    ///         <endpoint address="soap" contract="Services.IService" binding="basicHttpBinding"/>
    ///         <endpoint address="rest" contract="Services.IService" binding="webHttpBinding" behaviorConfiguration="restBehavior"/>
    ///       </service>
    ///     </services>
    ///     <behaviors>
    ///       <serviceBehaviors>
    ///         <behavior>
    ///           <serviceMetadata httpGetEnabled="true"/>
    ///         </behavior>
    ///       </serviceBehaviors>
    ///       <endpointBehaviors>
    ///         <behavior name="restBehavior">
    ///           <webHttp helpEnabled="true"/>
    ///         </behavior>
    ///       </endpointBehaviors>
    ///     </behaviors>
    ///     <serviceHostingEnvironment multipleSiteBindingsEnabled="true">
    ///       <serviceActivations>
    ///         <add relativeAddress="Service.svc" service="Services.Service, Services"/>
    ///       </serviceActivations>
    ///     </serviceHostingEnvironment>
    ///   </system.serviceModel>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class SelfHostingService : Adapter, ISelfHostingService, IPolicyRetriever
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> has been created with the specified <see cref="Endpoints"/>.
        /// </summary>
        /// <remarks>
        /// When <see cref="ServiceHostCreated"/> event is fired, changes like adding new endpoints can be made to the <see cref="ServiceHost"/>.
        /// </remarks>
        public event EventHandler ServiceHostCreated;

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> has can process requests via all of its endpoints.
        /// </summary>
        public event EventHandler ServiceHostStarted;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when processing a request.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception encountered when processing a request.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ServiceProcessException;

        // Fields
        private string m_endpoints;
        private string m_contractInterface;
        private bool m_singleton;
        private string m_securityPolicy;
        private bool m_publishMetadata;
        private bool m_allowCrossDomainAccess;
        private string m_allowedDomainList;
        private bool m_windowsAuthentication;
        private bool m_jsonFaultHandlingEnabled;
        private bool m_faultExceptionEnabled;
        private bool m_automaticFormatSelectionEnabled;
        private WebMessageFormat m_defaultOutgoingRequestFormat;
        private WebMessageFormat m_defaultOutgoingResponseFormat;
        private bool m_serviceEnabled;
        private bool m_disposed;
        private bool m_initialized;
        private ServiceHost m_serviceHost;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the web service.
        /// </summary>
        protected SelfHostingService()
        {
            Type type = GetType();
            m_endpoints = string.Empty;
            m_contractInterface = type.Namespace + ".I" + type.Name + ", " + type.AssemblyQualifiedName.ToNonNullString().Split(',')[1].Trim();
            m_allowCrossDomainAccess = false;
            m_allowedDomainList = "*";
            m_serviceEnabled = true;
            m_faultExceptionEnabled = true;
            m_defaultOutgoingRequestFormat = WebMessageFormat.Xml;
            m_defaultOutgoingResponseFormat = WebMessageFormat.Xml;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the web service is currently enabled.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return ((object)m_serviceHost != null && m_serviceHost.State == CommunicationState.Opened);
            }
            set
            {
                if (value && !Enabled)
                {
                    // Enable
                    if (!m_initialized)
                        Initialize();
                    else
                        InitializeServiceHost();
                }
                else if (!value && Enabled)
                {
                    // Disable
                    m_serviceHost.Close();
                }
            }
        }

        /// <summary>
        /// Gets or sets a semicolon delimited list of URIs where the web service can be accessed.
        /// </summary>
        /// <remarks>
        /// Set <see cref="Endpoints"/> to a null or empty string to disable web service hosting. Refer to <see cref="Service.CreateServiceBinding"/> 
        /// for a list of supported URI formats.
        /// </remarks>
        public string Endpoints
        {
            get
            {
                return m_endpoints;
            }
            set
            {
                m_endpoints = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Type.FullName"/> of the contract interface implemented by the web service.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string ContractInterface
        {
            get
            {
                return m_contractInterface;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_contractInterface = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ServiceHost"/> will use the current instance of the web service for processing 
        /// requests or base the web service instance creation on <see cref="InstanceContextMode"/> specified in its <see cref="ServiceBehaviorAttribute"/>.
        /// </summary>
        public bool Singleton
        {
            get
            {
                return m_singleton;
            }
            set
            {
                m_singleton = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Type.FullName"/> of <see cref="IAuthorizationPolicy"/> to be used for securing all web service <see cref="Endpoints"/>.
        /// </summary>
        public string SecurityPolicy
        {
            get
            {
                return m_securityPolicy;
            }
            set
            {
                m_securityPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether web service metadata is to made available at all web service <see cref="Endpoints"/>.
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
        /// Gets or sets flag that indicates if web services will enable cross-domain access for Silverlight and Flash applications. 
        /// </summary>
        public bool AllowCrossDomainAccess
        {
            get
            {
                return m_allowCrossDomainAccess;
            }
            set
            {
                m_allowCrossDomainAccess = value;
            }
        }

        /// <summary>
        /// Gets or sets comma separated list of allowed domains when <see cref="AllowCrossDomainAccess"/> is <c>true</c>. Use * for domain wildcards, e.g., *.consoto.com.
        /// </summary>
        /// <remarks>
        /// To allow all domains just set this property to "*".
        /// </remarks>
        public string AllowedDomainList
        {
            get
            {
                return m_allowedDomainList;
            }
            set
            {
                m_allowedDomainList = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether Windows Authentication is to be enabled.
        /// </summary>
        public bool WindowsAuthentication
        {
            get
            {
                return m_windowsAuthentication;
            }
            set
            {
                m_windowsAuthentication = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the web service is to be enabled at startup.
        /// </summary>
        public bool ServiceEnabled
        {
            get
            {
                return m_serviceEnabled;
            }
            set
            {
                m_serviceEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if JSON formatted fault messages should be returned during exceptions.
        /// </summary>
        public bool JsonFaultHandlingEnabled
        {
            get
            {
                return m_jsonFaultHandlingEnabled;
            }
            set
            {
                m_jsonFaultHandlingEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if automatic format selection is enabled for Web HTTP bindings.
        /// </summary>
        public bool AutomaticFormatSelectionEnabled
        {
            get
            {
                return m_automaticFormatSelectionEnabled;
            }
            set
            {
                m_automaticFormatSelectionEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that specifies whether a FaultException is generated when an internal server error(HTTP status code: 500) occurs for Web HTTP bindings.
        /// </summary>
        public bool FaultExceptionEnabled
        {
            get
            {
                return m_faultExceptionEnabled;
            }
            set
            {
                m_faultExceptionEnabled = value;
            }
        }

        /// <summary>
        /// Gets and sets the default outgoing request format for Web HTTP bindings.
        /// </summary>
        public WebMessageFormat DefaultOutgoingRequestFormat
        {
            get
            {
                return m_defaultOutgoingRequestFormat;
            }
            set
            {
                m_defaultOutgoingRequestFormat = value;
            }
        }

        /// <summary>
        /// Gets and sets the default outgoing response format for Web HTTP bindings.
        /// </summary>
        public WebMessageFormat DefaultOutgoingResponseFormat
        {
            get
            {
                return m_defaultOutgoingResponseFormat;
            }
            set
            {
                m_defaultOutgoingResponseFormat = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="WebServiceHost"/> hosting the web service.
        /// </summary>
        /// <remarks>
        /// By default, the <see cref="ServiceHost"/> only has <see cref="WebHttpBinding"/> endpoint at the <see cref="Endpoints"/>. 
        /// Additional endpoints can be added to the <see cref="ServiceHost"/> when <see cref="ServiceHostCreated"/> event is fired.
        /// </remarks>
        public ServiceHost ServiceHost
        {
            get
            {
                return m_serviceHost;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the web service.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (!m_initialized)
            {
                // Initialize the service host.
                InitializeServiceHost();

                // Initialize only once.
                m_initialized = true;
            }
        }

        /// <summary>
        /// Saves web service settings to the config file if the <see cref="Adapter.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();

            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

                settings["Endpoints", true].Update(m_endpoints);
                settings["Contract", true].Update(m_contractInterface);
                settings["Singleton", true].Update(m_singleton);
                settings["SecurityPolicy", true].Update(m_securityPolicy);
                settings["PublishMetadata", true].Update(m_publishMetadata);
                settings["AllowCrossDomainAccess", true].Update(m_allowCrossDomainAccess);
                settings["AllowedDomainList", true].Update(m_allowedDomainList);
                settings["Enabled", true].Update(m_serviceEnabled);

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved web service settings from the config file if the <see cref="Adapter.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

                settings.Add("Endpoints", m_endpoints, "Semicolon delimited list of URIs where the web service can be accessed.");
                settings.Add("Contract", m_contractInterface, "Assembly qualified name of the contract interface implemented by the web service.");
                settings.Add("Singleton", m_singleton, "True if the web service is singleton; otherwise False.");
                settings.Add("SecurityPolicy", m_securityPolicy, "Assembly qualified name of the authorization policy to be used for securing the web service.");
                settings.Add("PublishMetadata", m_publishMetadata, "True if the web service metadata is to be published at all the endpoints; otherwise False.");
                settings.Add("AllowCrossDomainAccess", m_allowCrossDomainAccess, "True to allow Silverlight and Flash cross-domain access to the web service.");
                settings.Add("AllowedDomainList", m_allowedDomainList, "Comma separated list of domain names for Silverlight and Flash cross-domain access to use when allowCrossDomainAccess is true. Use * for domain wildcards, e.g., *.consoto.com.");
                settings.Add("Enabled", ServiceEnabled.ToString(), "Determines if web service should be enabled at startup.");

                Endpoints = settings["Endpoints"].ValueAs(m_endpoints);
                ContractInterface = settings["Contract"].ValueAs(m_contractInterface);
                Singleton = settings["Singleton"].ValueAs(m_singleton);
                SecurityPolicy = settings["SecurityPolicy"].ValueAs(m_securityPolicy);
                PublishMetadata = settings["PublishMetadata"].ValueAs(m_publishMetadata);
                AllowCrossDomainAccess = settings["AllowCrossDomainAccess"].ValueAs(m_allowCrossDomainAccess);
                AllowedDomainList = settings["AllowedDomainList"].ValueAs(m_allowedDomainList);

                // Technically removing all end points will "disable" a web service since it would bind to nothing, however,
                // this allows you to keep configured end points and still disable the service from configuration
                m_serviceEnabled = settings["Enabled"].ValueAsBoolean(ServiceEnabled);
            }
        }

        /// <summary>
        /// Get an unused port number.
        /// </summary>
        /// <returns>An <see cref="Int32"/> value.</returns>
        protected virtual int GetUnusedPort()
        {
            int randomPort = Random.Int32Between(1024, 65535);
            IPEndPoint[] tcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();

            while (true)
            {
                if ((object)tcpListeners.FirstOrDefault(ep => ep.Port == randomPort) != null)
                    randomPort++;       // Port in use - pick another one.
                else
                    return randomPort;  // Port is not in use - use this.
            }
        }

        /// <summary>
        /// Gets an address where the <see cref="ServiceHost"/> will host the service.
        /// </summary>
        /// <returns>A <see cref="String"/> value.</returns>
        protected virtual string GetServiceAddress()
        {
            // Regex matches:
            // [http | ftp][.<protocol>]://localhost[:<port>][/<path>]
            const string regex = @"(\w+\.*\w*\://)(?<host>\w+\:*\d*)";

            // Look through all of the specified endpoints.
            foreach (string endpoint in m_endpoints.Split(';'))
            {
                // Use endpoint host information including port number for service address.
                Match match = Regex.Match(endpoint, regex, RegexOptions.IgnoreCase);

                if (match.Success && match.Groups["host"].Success)
                    return $"http://{match.Groups["host"]}";
            }

            // Return an empty string if endpoints are not in a valid format.
            return string.Empty;
        }

        /// <summary>
        /// Initializes the <see cref="ServiceHost"/>.
        /// </summary>
        protected virtual void InitializeServiceHost()
        {
            if (m_serviceEnabled && !string.IsNullOrEmpty(m_endpoints))
            {
                // Initialize service host.
                string serviceUri = GetServiceAddress();

                if (m_singleton)
                    m_serviceHost = new ServiceHost(this, new Uri(serviceUri));
                else
                    m_serviceHost = new ServiceHost(this.GetType(), new Uri(serviceUri));

                // Enable metadata publishing.
                if (m_publishMetadata)
                {
                    ServiceMetadataBehavior serviceBehavior = m_serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                    if ((object)serviceBehavior == null)
                    {
                        serviceBehavior = new ServiceMetadataBehavior();
                        m_serviceHost.Description.Behaviors.Add(serviceBehavior);
                    }

                    serviceBehavior.HttpGetEnabled = true;
                }

                // Enable security on the service.
                if (!string.IsNullOrEmpty(m_securityPolicy))
                {
                    ServiceAuthorizationBehavior serviceBehavior = m_serviceHost.Description.Behaviors.Find<ServiceAuthorizationBehavior>();

                    if ((object)serviceBehavior == null)
                    {
                        serviceBehavior = new ServiceAuthorizationBehavior();
                        m_serviceHost.Description.Behaviors.Add(serviceBehavior);
                    }

                    serviceBehavior.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
                    List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();

                    Type securityPolicyType = Type.GetType(m_securityPolicy);

                    if ((object)securityPolicyType == null)
                        throw new NullReferenceException($"Failed get security policy type '{m_securityPolicy.ToNonNullNorWhiteSpace("[undefined]")}' for self-hosting service - check config file settings.");

                    policies.Add((IAuthorizationPolicy)Activator.CreateInstance(securityPolicyType));
                    serviceBehavior.ExternalAuthorizationPolicies = policies.AsReadOnly();
                }

                // Add specified service endpoints.
                string serviceAddress;
                string[] serviceAddresses;
                Binding serviceBinding;
                ServiceEndpoint serviceEndpoint;
                serviceAddresses = m_endpoints.Split(';');

                // Enable Windows Authentication if authorization policy is configured.
                if (!string.IsNullOrEmpty(m_securityPolicy))
                    m_windowsAuthentication = true;

                for (int i = 0; i < serviceAddresses.Length; i++)
                {
                    serviceAddress = serviceAddresses[i].Trim();
                    serviceBinding = Service.CreateServiceBinding(ref serviceAddress, m_windowsAuthentication);

                    if ((object)serviceBinding == null)
                        continue;

                    Type contractInterfaceType = Type.GetType(m_contractInterface);

                    if ((object)contractInterfaceType == null)
                        throw new NullReferenceException($"Failed to get contract interface type '{m_contractInterface.ToNonNullNorWhiteSpace("[undefined]")}' for self-hosting service - check config file settings.");

                    serviceEndpoint = m_serviceHost.AddServiceEndpoint(contractInterfaceType, serviceBinding, serviceAddress);

                    if (serviceBinding is WebHttpBinding)
                    {
                        // Special handling for REST endpoint.
                        WebHttpBehavior restBehavior = m_jsonFaultHandlingEnabled ? new JsonFaultWebHttpBehavior() : new WebHttpBehavior();
#if !MONO
                        if (m_publishMetadata)
                            restBehavior.HelpEnabled = true;
#endif
                        restBehavior.FaultExceptionEnabled = m_faultExceptionEnabled;
                        restBehavior.AutomaticFormatSelectionEnabled = m_automaticFormatSelectionEnabled;
                        restBehavior.DefaultOutgoingRequestFormat = m_defaultOutgoingRequestFormat;
                        restBehavior.DefaultOutgoingResponseFormat = m_defaultOutgoingResponseFormat;

                        serviceEndpoint.Behaviors.Add(restBehavior);
                    }
                    else if (m_publishMetadata)
                    {
                        // Add endpoint for service metadata.
                        if (serviceAddress.StartsWith("http://"))
                            m_serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), serviceAddress + "/mex");
                        else if (serviceAddress.StartsWith("net.tcp://"))
                            m_serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), serviceAddress + "/mex");
                        else if (serviceAddress.StartsWith("net.pipe://"))
                            m_serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexNamedPipeBinding(), serviceAddress + "/mex");
                    }
                }

                // Enable cross domain access.
                if (m_allowCrossDomainAccess)
                    m_serviceHost.AddServiceEndpoint(typeof(IPolicyRetriever), new WebHttpBinding(), "").Behaviors.Add(new WebHttpBehavior());

                // Allow for customization.
                OnServiceHostCreated();

                // Start the service.
                m_serviceHost.Open();
                OnServiceHostStarted();
            }
        }

        // Converts a string of data to a UTF8 stream using "application/xml" as the content type
        private Stream StringToStream(string result)
        {
            if ((object)WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";

            // Using UTF8 result buffer as non-expandable memory stream for string-as-stream
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        /// <summary>
        /// Gets policy stream for Silverlight applications.
        /// </summary>
        /// <returns>Stream containing clientaccesspolicy.xml.</returns>
        public Stream GetSilverlightPolicy()
        {
            if (m_allowCrossDomainAccess)
            {
                const string result = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<access-policy>\r\n" +
                    "  <cross-domain-access>\r\n" +
                    "    <policy>\r\n" +
                    "      <allow-from http-request-headers=\"*\">\r\n" +
                    "        <domain uri=\"{0}\"/>\r\n" +
                    "      </allow-from>\r\n" +
                    "      <grant-to>\r\n" +
                    "        <resource path=\"/\" include-subpaths=\"true\"/>\r\n" +
                    "      </grant-to>\r\n" +
                    "      </policy>\r\n" +
                    "    </cross-domain-access>\r\n" +
                    "</access-policy>";

                return StringToStream(string.Format(result, m_allowedDomainList));
            }

            return StringToStream("");
        }

        /// <summary>
        /// Gets policy stream for Flash applications.
        /// </summary>
        /// <returns>Stream containing crossdomain.xml.</returns>
        public Stream GetFlashPolicy()
        {
            if (m_allowCrossDomainAccess)
            {
                const string result = "<?xml version=\"1.0\"?>\r\n" +
                    "<!DOCTYPE cross-domain-policy SYSTEM \"http://www.macromedia.com/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                    "<cross-domain-policy>\r\n{0}</cross-domain-policy>";

                const string allowAccessTag = "  <allow-access-from domain=\"{0}\" to-ports=\"*\" />\r\n";

                StringBuilder allowedDomains = new StringBuilder();
                string[] domains = m_allowedDomainList.ToNonNullString("*").Split(',');

                foreach (string domain in domains)
                {
                    allowedDomains.AppendFormat(allowAccessTag, domain);
                }

                return StringToStream(string.Format(result, allowedDomains));
            }

            return StringToStream("");
        }

        /// <summary>
        /// Releases the unmanaged resources used by the web service and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if ((object)m_serviceHost != null && m_serviceHost.State != CommunicationState.Faulted)
                            m_serviceHost.Close();
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ServiceHostCreated"/> event.
        /// </summary>
        protected virtual void OnServiceHostCreated()
        {
            if ((object)ServiceHostCreated != null)
                ServiceHostCreated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceHostStarted"/> event.
        /// </summary>
        protected virtual void OnServiceHostStarted()
        {
            if ((object)ServiceHostStarted != null)
                ServiceHostStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceProcessException"/> event.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to sent to <see cref="ServiceProcessException"/> event.</param>
        protected virtual void OnServiceProcessException(Exception exception)
        {
            if ((object)ServiceProcessException != null)
                ServiceProcessException(this, new EventArgs<Exception>(exception));
        }

        #endregion
    }
}
