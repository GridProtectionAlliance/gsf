//*******************************************************************************************************
//  SelfHostingService.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/27/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/02/2009 - Pinal C. Patel
//       Modified configuration of the default WebHttpBinding to enable receiving of large payloads.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/27/2009 - Pinal C. Patel
//       Fixed bug in the initialization of service contract name.
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
//       Modified GetUnusedPort() to use TVA.Security.Cryptography.Random instead of System.Random to
//       unsure unique numbers.
//  11/29/2011 - Pinal C. Patel
//       Modified InitializeServiceHost() to not use a random port number for the service host address
//       so that setting up security on the address is possible.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using TVA.Adapters;
using TVA.Configuration;

namespace TVA.ServiceModel
{
    /// <summary>
    /// A base class for web service that can send and receive data over REST (Representational State Transfer) interface.
    /// </summary>
    /// <example>
    /// This example shows how to create a WCF service derived from <see cref="SelfHostingService"/> that is capable of hosting itself:
    /// <code>
    /// using System.ServiceModel;
    /// using System.ServiceModel.Web;
    /// using TVA.ServiceModel;
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
    /// using TVA.ServiceModel;
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
    public class SelfHostingService : Adapter, ISelfHostingService
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
        private string m_contract;
        private bool m_singleton;
        private string m_securityPolicy;
        private bool m_publishMetadata;
        private bool m_disposed;
        private bool m_initialized;
        private ServiceHost m_serviceHost;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the web service.
        /// </summary>
        protected SelfHostingService()
            : base()
        {
            Type type = this.GetType();
            m_contract = type.Namespace + ".I" + type.Name + ", " + type.AssemblyQualifiedName.Split(',')[1].Trim();
        }

        /// <summary>
        /// Releases the unmanaged resources before the web service is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SelfHostingService()
        {
            Dispose(false);
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
                return (m_serviceHost != null && m_serviceHost.State == CommunicationState.Opened);
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
        public string Contract
        {
            get
            {
                return m_contract;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_contract = value;
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
                InitializeServiceHost();    // Initialize the service host.
                m_initialized = true;       // Initialize only once.
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
                settings["Contract", true].Update(m_contract);
                settings["Singleton", true].Update(m_singleton);
                settings["SecurityPolicy", true].Update(m_securityPolicy);
                settings["PublishMetadata", true].Update(m_publishMetadata);
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
                settings.Add("Contract", m_contract, "Assembly qualified name of the contract interface implemented by the web service.");
                settings.Add("Singleton", m_singleton, "True if the web service is singleton; otherwise False.");
                settings.Add("SecurityPolicy", m_securityPolicy, "Assembly qualified name of the authorization policy to be used for securing the web service.");
                settings.Add("PublishMetadata", m_publishMetadata, "True if the web service metadata is to be published at all the endpoints; otherwise False.");
                Endpoints = settings["Endpoints"].ValueAs(m_endpoints);
                Contract = settings["Contract"].ValueAs(m_contract);
                Singleton = settings["Singleton"].ValueAs(m_singleton);
                SecurityPolicy = settings["SecurityPolicy"].ValueAs(m_securityPolicy);
                PublishMetadata = settings["PublishMetadata"].ValueAs(m_publishMetadata);
            }
        }

        /// <summary>
        /// Get an unused port number.
        /// </summary>
        /// <returns>An <see cref="Int32"/> value.</returns>
        protected virtual int GetUnusedPort()
        {
            int randomPort = TVA.Security.Cryptography.Random.Int32Between(1024, 65535);
            IPEndPoint[] tcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            while (true)
            {
                if (tcpListeners.FirstOrDefault(ep => ep.Port == randomPort) != null)
                    // Port in use - pick another one.
                    randomPort++;
                else
                    // Port is not in use - use this.
                    return randomPort;
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
            string regex = @"(\w+\.*\w*\://)(?<host>\w+\:*\d*)";

            // Look through all of the specified endpoints.
            foreach (string endpoint in m_endpoints.Split(';'))
            {
                // Use endpoint host information including port number for service address.
                Match match = Regex.Match(endpoint, regex, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups["host"].Success)
                    return string.Format("http://{0}", match.Groups["host"]);
            }

            // Return an empty string if endpoints are not in a valid format.
            return string.Empty;
        }

        /// <summary>
        /// Initializes the <see cref="ServiceHost"/>.
        /// </summary>
        protected virtual void InitializeServiceHost()
        {
            if (!string.IsNullOrEmpty(m_endpoints))
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

                    if (serviceBehavior == null)
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

                    if (serviceBehavior == null)
                    {
                        serviceBehavior = new ServiceAuthorizationBehavior();
                        m_serviceHost.Description.Behaviors.Add(serviceBehavior);
                    }

                    serviceBehavior.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
                    List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
                    policies.Add((IAuthorizationPolicy)Activator.CreateInstance(System.Type.GetType(m_securityPolicy)));
                    serviceBehavior.ExternalAuthorizationPolicies = policies.AsReadOnly();
                }

                // Add specified service endpoints.
                string serviceAddress;
                string[] serviceAddresses;
                Binding serviceBinding;
                ServiceEndpoint serviceEndpoint;
                serviceAddresses = m_endpoints.Split(';');

                for (int i = 0; i < serviceAddresses.Length; i++)
                {
                    serviceAddress = serviceAddresses[i].Trim();
                    serviceBinding = Service.CreateServiceBinding(ref serviceAddress, !string.IsNullOrEmpty(m_securityPolicy));
                    if (serviceBinding != null)
                    {
                        serviceEndpoint = m_serviceHost.AddServiceEndpoint(System.Type.GetType(m_contract), serviceBinding, serviceAddress);

                        if (serviceBinding is WebHttpBinding)
                        {
                            // Special handling for REST endpoint.
                            WebHttpBehavior restBehavior = new WebHttpBehavior();

#if !MONO
                            if (m_publishMetadata)
                                restBehavior.HelpEnabled = true;
#endif

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
                }

                // Allow for customization.
                OnServiceHostCreated();

                // Start the service.
                m_serviceHost.Open();
                OnServiceHostStarted();
            }
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
                        if (m_serviceHost != null)
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
            if (ServiceHostCreated != null)
                ServiceHostCreated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceHostStarted"/> event.
        /// </summary>
        protected virtual void OnServiceHostStarted()
        {
            if (ServiceHostStarted != null)
                ServiceHostStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceProcessException"/> event.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to sent to <see cref="ServiceProcessException"/> event.</param>
        protected virtual void OnServiceProcessException(Exception exception)
        {
            if (ServiceProcessException != null)
                ServiceProcessException(this, new EventArgs<Exception>(exception));
        }

        #endregion
    }
}
