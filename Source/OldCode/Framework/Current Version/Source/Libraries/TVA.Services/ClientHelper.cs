//*******************************************************************************************************
//  ClientHelper.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  09/30/2008 - J. Ritchie Carroll
//       Convert to C#.
//  07/15/2009 - Pinal C. Patel
//       Added AuthenticationMethod, AuthenticationUsername and AuthenticationPassword properties to
//       provision for the authentication process as part of security.
//       Added ISupportLifecycle, ISupportInitialize and IPersistSettings interface implementations
//       to support the persistance and retrieval of settings from the config file.
//  07/17/2009 - Pinal C. Patel
//       Added static PretendRequest() method that can be used to create pretend request for manually
//       invoking request handlers registered with the ServiceHelper.
//  07/21/2009 - Pinal C. Patel
//       Replace AuthenticationUsername and AuthenticationPassword properties with AuthenticationInput
//       to allow for input text to be specified for any AuthenticationMethod instead of just Ntml.
//  07/23/2009 - Pinal C. Patel
//       ReceivedServiceResponse is now raised only for custom service responses instead of all.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using TVA.Communication;
using TVA.Configuration;

namespace TVA.Services
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the type of <see cref="Microsoft.Web.Services3.Security.Tokens.SecurityToken"/> to be sent to the <see cref="ServiceHelper"/> for authentication.
    /// </summary>
    public enum IdentityToken
    {
        /// <summary>
        /// No <see cref="Microsoft.Web.Services3.Security.Tokens.SecurityToken"/> is to be sent.
        /// </summary>
        None,
        /// <summary>
        /// A <see cref="Microsoft.Web.Services3.Security.Tokens.UsernameToken"/> is to be sent.
        /// </summary>
        Ntlm,
        /// <summary>
        /// A <see cref="Microsoft.Web.Services3.Security.Tokens.KerberosToken"/> is to be sent.
        /// </summary>
        Kerberos
    }

    #endregion

    /// <summary>
    /// Component that provides client-side functionality to <see cref="ServiceHelper"/>.
    /// </summary>
	[ToolboxBitmap(typeof(ClientHelper))]
    public class ClientHelper : Component, ISupportLifecycle, ISupportInitialize, IPersistSettings
	{
	    #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="AuthenticationMethod"/> property.
        /// </summary>
        public const IdentityToken DefaultAuthenticationMethod = IdentityToken.None;

        /// <summary>
        /// Specifies the default value for the <see cref="AuthenticationInput"/> property.
        /// </summary>
        public const string DefaultAuthenticationInput = "";

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ClientHelper";

        // Events

        /// <summary>
		/// Occurs when a status update is received from the <see cref="ServiceHelper"/>.
		/// </summary>
        [Category("Client"), 
        Description("Occurs when a status update is received from the ServiceHelper.")]
        public event EventHandler<EventArgs<string>> ReceivedServiceUpdate;
				
		/// <summary>
		/// Occurs when a custom <see cref="ServiceResponse"/> is received from the <see cref="ServiceHelper"/>.
		/// </summary>
        [Category("Service"), 
        Description("Occurs when a ServiceResponse is received from the ServiceHelper.")]
        public event EventHandler<EventArgs<ServiceResponse>> ReceivedServiceResponse;
		
		/// <summary>
		/// Occurs when the state of the <see cref="ServiceHelper"/> is changed.
		/// </summary>
        [Category("Service"), 
        Description("Occurs when the state of the ServiceHelper is changed.")]
        public event EventHandler<EventArgs<ObjectState<ServiceState>>> ServiceStateChanged;
		
		/// <summary>
		/// Occurs when the state of a <see cref="ServiceProcess"/> is changed.
		/// </summary>
        [Category("Service"), 
        Description("Occurs when the state of a ServiceProcess is changed.")]
        public event EventHandler<EventArgs<ObjectState<ServiceProcessState>>> ProcessStateChanged;

        /// <summary>
        /// Occurs when the <see cref="ServiceHelper"/> successfully authenticates the <see cref="ClientHelper"/>.
        /// </summary>
        [Category("Security"),
        Description("Occurs when the ServiceHelper successfully authenticates the ClientHelper.")]
        public event EventHandler AuthenticationSuccess;

        /// <summary>
        /// Occurs when the <see cref="ServiceHelper"/> fails to authenticate the <see cref="ClientHelper"/>.
        /// </summary>
        /// <remarks>
        /// Set <see cref="CancelEventArgs.Cancel"/> to <b>true</b> to continue with connection attempts even after authentication fails. 
        /// This can be useful for re-authenticating using different <see cref="AuthenticationMethod"/> and <see cref="AuthenticationInput"/>.
        /// </remarks>
        [Category("Security"),
        Description("Occurs when the ServiceHelper fails to authenticate the ClientHelper.")]
        public event EventHandler<CancelEventArgs> AuthenticationFailure;

		/// <summary>
		/// Occurs when a telnet session has been established.
		/// </summary>
		[Category("Command"),
        Description("Occurs when a telnet session has been established.")]
        public event EventHandler TelnetSessionEstablished;	
		
		/// <summary>
		/// Occurs when a telnet session has been terminated.
		/// </summary>
        [Category("Command"),
        Description("Occurs when a telnet session has been terminated.")]
        public event EventHandler TelnetSessionTerminated;

        // Fields
        private ClientBase m_remotingClient;
        private IdentityToken m_authenticationMethod;
        private string m_authenticationInput;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private bool m_attemptReconnection;
        private bool m_authenticationComplete;
        private bool m_disposed;
        private bool m_initialized;
		
        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientHelper"/> class.
        /// </summary>
        public ClientHelper()
            : base()
        {
            m_authenticationMethod = DefaultAuthenticationMethod;
            m_authenticationInput = DefaultAuthenticationInput;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientHelper"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ClientHelper"/>.</param>
        public ClientHelper(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ClientBase"/> object used for communicating with the <see cref="ServiceHelper"/>.
        /// </summary>
        [Category("Components"),
        Description("ClientBase object used for communicating with the ServiceHelper.")]
        public ClientBase RemotingClient
        {
            get
            {
                return m_remotingClient;
            }
            set
            {
                if (m_remotingClient != null)
                {
                    // Detach events from any existing instance
                    m_remotingClient.ConnectionEstablished -= RemotingClient_ConnectionEstablished;
                    m_remotingClient.ConnectionAttempt -= RemotingClient_ConnectionAttempt;
                    m_remotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;
                    m_remotingClient.ReceiveDataComplete -= RemotingClient_ReceiveDataComplete;
                }

                m_remotingClient = value;

                if (m_remotingClient != null)
                {
                    // Attach events to new instance
                    m_remotingClient.ConnectionEstablished += RemotingClient_ConnectionEstablished;
                    m_remotingClient.ConnectionAttempt += RemotingClient_ConnectionAttempt;
                    m_remotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;
                    m_remotingClient.ReceiveDataComplete += RemotingClient_ReceiveDataComplete;
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of <see cref="IdentityToken"/> to be sent to the <see cref="ServiceHelper"/> for authentication.
        /// </summary>
        [Category("Security"),
        DefaultValue(DefaultAuthenticationMethod),
        Description("Type of IdentityToken to be sent to the ServiceHelper for authentication.")]
        public IdentityToken AuthenticationMethod
        {
            get
            {
                return m_authenticationMethod;
            }
            set 
            {
                m_authenticationMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets input text for the current <see cref="AuthenticationMethod"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        [Category("Security"),
        DefaultValue(DefaultAuthenticationInput),
        Description("Input text for the current AuthenticationMethod.")]
        public string AuthenticationInput
        {
            get
            {
                return m_authenticationInput;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_authenticationInput = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ClientHelper"/> are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of ClientHelper are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of <see cref="ClientHelper"/> are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of ClientHelper are to be saved to the config file if the PersistSettings property is set to true.")]
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
        /// Gets or sets a boolean value that indicates whether the <see cref="ClientHelper"/> is currently enabled.
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
                if (m_remotingClient == null)
                    return false;
                else
                    return m_remotingClient.Enabled;
            }
            set
            {
                if (m_remotingClient != null)
                    m_remotingClient.Enabled = value;
            }
        }


        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ClientHelper"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ClientHelper"/> is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Performs necessary operations before the <see cref="ClientHelper"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ClientHelper"/> is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            if (!DesignMode)
            {
                try
                {
                    // Nothing needs to be done before component is initialized.
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Performs necessary operations after the <see cref="ClientHelper"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ClientHelper"/> is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                try
                {
                    Initialize();
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="ClientHelper"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                element = settings["AuthenticationMethod", true];
                element.Update(m_authenticationMethod, element.Description, element.Encrypted);
                element = settings["AuthenticationInput", true];
                element.Update(m_authenticationInput, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="ClientHelper"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("AuthenticationMethod", m_authenticationMethod, "Authentication method (None; Ntlm; Kerberos) used for security.");
                settings.Add("AuthenticationInput", m_authenticationInput, "Input text for the current AuthenticationMethod.", true);
                AuthenticationMethod = settings["AuthenticationMethod"].ValueAs(m_authenticationMethod);
                AuthenticationInput = settings["AuthenticationInput"].ValueAs(m_authenticationInput);
            }
        }

        /// <summary>
        /// Connects <see cref="RemotingClient"/> to <see cref="ServiceHelper.RemotingServer"/> and wait until authentication is complete.
        /// </summary>
        public void Connect()
        {           
            if (m_remotingClient == null)
                throw new InvalidOperationException("RemotingClient property of ClientHelper component is not set");

            m_attemptReconnection = true;
            m_authenticationComplete = false;
            m_remotingClient.Connect();                                     // Wait for connection.
            if (m_remotingClient.Enabled)
                while (!m_authenticationComplete) { Thread.Sleep(100); }    // Wait for authentication.
        }

        /// <summary>
        /// Disconnects <see cref="RemotingClient"/> from <see cref="ServiceHelper.RemotingServer"/>.
        /// </summary>
        public void Disconnect()
        {
            m_attemptReconnection = false;
            m_remotingClient.Disconnect();
        }

        /// <summary>
        /// Sends a request to the <see cref="ServiceHelper"/> using <see cref="RemotingClient"/>.
        /// </summary>
        /// <param name="request">Request text to be sent.</param>
        public void SendRequest(string request)
        {
            ClientRequest requestInstance = ClientRequest.Parse(request);

            if (requestInstance != null)
                SendRequest(requestInstance);
            else
                UpdateStatus(string.Format("Request command \"{0}\" is invalid\r\n\r\n", request));
        }

        /// <summary>
        /// Sends a request to the <see cref="ServiceHelper"/> using <see cref="RemotingClient"/>.
        /// </summary>
        /// <param name="request"><see cref="ClientRequest"/> object to be sent.</param>
        public void SendRequest(ClientRequest request)
        {
            m_remotingClient.SendAsync(request);
        }

        /// <summary>
        /// Raises the <see cref="ReceivedServiceUpdate"/> event.
        /// </summary>
        /// <param name="update">Update message received.</param>
        protected virtual void OnReceivedServiceUpdate(string update)
        {
            if (ReceivedServiceUpdate != null)
                ReceivedServiceUpdate(this, new EventArgs<string>(update));
        }

        /// <summary>
        /// Raises the <see cref="ReceivedServiceResponse"/> event.
        /// </summary>
        /// <param name="response"><see cref="ServiceResponse"/> received.</param>
        protected virtual void OnReceivedServiceResponse(ServiceResponse response)
        {
            if (ReceivedServiceResponse != null)
                ReceivedServiceResponse(this, new EventArgs<ServiceResponse>(response));
        }

        /// <summary>
        /// Raises the <see cref="ServiceStateChanged"/> event.
        /// </summary>
        /// <param name="state">New <see cref="ServiceState"/>.</param>
        protected virtual void OnServiceStateChanged(ObjectState<ServiceState> state)
        {
            if (ServiceStateChanged != null)
                ServiceStateChanged(this, new EventArgs<ObjectState<ServiceState>>(state));
        }

        /// <summary>
        /// Raises the <see cref="ProcessStateChanged"/> event.
        /// </summary>
        /// <param name="state">New <see cref="ServiceProcessState"/>.</param>
        protected virtual void OnProcessStateChanged(ObjectState<ServiceProcessState> state)
        {
            if (ProcessStateChanged != null)
                ProcessStateChanged(this, new EventArgs<ObjectState<ServiceProcessState>>(state));
        }

        /// <summary>
        /// Raises the <see cref="AuthenticationSuccess"/> event.
        /// </summary>
        protected virtual void OnAuthenticationSuccess()
        {
            m_authenticationComplete = true;
            if (AuthenticationSuccess != null)
                AuthenticationSuccess(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="AuthenticationFailure"/> event.
        /// </summary>
        protected virtual void OnAuthenticationFailure()
        {
            CancelEventArgs args = new CancelEventArgs(true);
            if (AuthenticationFailure != null)
                AuthenticationFailure(this, args);

            // Continue connection attempts if requested.
            if (args.Cancel)
            {
                m_attemptReconnection = false;
                m_authenticationComplete = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="TelnetSessionEstablished"/> event.
        /// </summary>
        protected virtual void OnTelnetSessionEstablished()
        {           
            if (TelnetSessionEstablished != null)
                TelnetSessionEstablished(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="TelnetSessionTerminated"/> event.
        /// </summary>
        protected virtual void OnTelnetSessionTerminated()
        {
            if (TelnetSessionTerminated != null)
                TelnetSessionTerminated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ClientHelper"/> object and optionally releases the managed resources.
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
                        Disconnect();
                        SaveSettings();
                        RemotingClient = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        private void UpdateStatus(string message, params object[] args)
        {
            OnReceivedServiceUpdate(string.Format(message, args));
        }

        private void RemotingClient_ConnectionAttempt(object sender, System.EventArgs e)
        {
            UpdateStatus("Connecting to {0}...\r\n\r\n", m_remotingClient.ServerUri);
        }

        private void RemotingClient_ConnectionEstablished(object sender, System.EventArgs e)
        {
            // Upon establishing connection with the service's communication client, we'll send our information to the
            // service so the service can keep track of all the client that are connected to its communication server.
            m_remotingClient.Send(new ClientInfo(this));

            StringBuilder status = new StringBuilder();
            status.AppendFormat("Connected to {0}:", m_remotingClient.ServerUri);
            status.AppendLine();
            status.AppendLine();
            status.Append(m_remotingClient.Status);
            status.AppendLine();
            UpdateStatus(status.ToString());
        }

        private void RemotingClient_ConnectionTerminated(object sender, System.EventArgs e)
        {
            StringBuilder status = new StringBuilder();
            status.AppendFormat("Disconnected from {0}:", m_remotingClient.ServerUri);
            status.AppendLine();
            status.AppendLine();
            status.Append(m_remotingClient.Status);
            status.AppendLine();
            UpdateStatus(status.ToString());

            // Attempt reconnection on a seperate thread.
            if (m_attemptReconnection)
                new Thread((ThreadStart)delegate(){ Connect(); }).Start();
        }

        private void RemotingClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            ServiceResponse response = null;
            Serialization.TryGetObject<ServiceResponse>(e.Argument1.BlockCopy(0, e.Argument2), out response);

            if (response != null)
            {              
                switch (response.Type)
                {
                    case "UPDATECLIENTSTATUS":
                        UpdateStatus(response.Message);
                        break;
                    case "AUTHENTICATIONSUCCESS":
                        OnAuthenticationSuccess();
                        break;
                    case "AUTHENTICATIONFAILURE":
                        OnAuthenticationFailure();
                        break;
                    case "SERVICESTATECHANGED":
                        if (response.Attachments.Count > 0)
                        {
                            ObjectState<ServiceState> state = response.Attachments[0] as ObjectState<ServiceState>;

                            if (state != null)
                            {
                                // Notify change in service state by raising the ServiceStateChanged event.
                                OnServiceStateChanged(state);

                                UpdateStatus(string.Format("State of service \"{0}\" has changed to \"{1}\".\r\n\r\n", state.ObjectName, state.CurrentState));
                            }
                        }
                        break;
                    case "PROCESSSTATECHANGED":
                        if (response.Attachments.Count > 0)
                        {
                            ObjectState<ServiceProcessState> state = response.Attachments[0] as ObjectState<ServiceProcessState>;

                            if (state != null)
                            {
                                // Notify change in process state by raising the ProcessStateChanged event.
                                OnProcessStateChanged(state);

                                UpdateStatus(string.Format("State of process \"{0}\" has changed to \"{1}\".\r\n\r\n", state.ObjectName, state.CurrentState));
                            }
                        }
                        break;
                    case "TELNETSESSION":
                        switch (response.Message.ToUpper())
                        {
                            case "ESTABLISHED":
                                OnTelnetSessionEstablished();
                                break;
                            case "TERMINATED":
                                OnTelnetSessionTerminated();
                                break;
                        }
                        break;
                    default:
                        OnReceivedServiceResponse(response);
                        break;
                }
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Returns an <see cref="ClientRequestInfo"/> object for the specified <paramref name="requestCommand"/> that can be used 
        /// to invoke <see cref="ServiceHelper.ClientRequestHandlers"/> manually as if the request was sent by a <see cref="ClientHelper"/> remotely.
        /// </summary>
        /// <param name="requestCommand">Command for which an <see cref="ClientRequestInfo"/> object is to be created.</param>
        /// <returns>An <see cref="ClientRequestInfo"/> object.</returns>
        public static ClientRequestInfo PretendRequest(string requestCommand)
        {
            ClientRequest request = ClientRequest.Parse(requestCommand);
            ClientRequestInfo requestInfo = new ClientRequestInfo(new ClientInfo(), request);

            return requestInfo;
        }

        #endregion
    }
}
