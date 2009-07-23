//*******************************************************************************************************
//  ClientHelper.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//  09/30/2008 - James R. Carroll
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
//
//*******************************************************************************************************

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
                    m_remotingClient.ConnectionEstablished -= m_remotingClient_ConnectionEstablished;
                    m_remotingClient.ConnectionAttempt -= m_remotingClient_ConnectionAttempt;
                    m_remotingClient.ConnectionTerminated -= m_remotingClient_ConnectionTerminated;
                    m_remotingClient.ReceiveDataComplete -= m_remotingClient_ReceiveDataComplete;
                }

                m_remotingClient = value;

                if (m_remotingClient != null)
                {
                    // Attach events to new instance
                    m_remotingClient.ConnectionEstablished += m_remotingClient_ConnectionEstablished;
                    m_remotingClient.ConnectionAttempt += m_remotingClient_ConnectionAttempt;
                    m_remotingClient.ConnectionTerminated += m_remotingClient_ConnectionTerminated;
                    m_remotingClient.ReceiveDataComplete += m_remotingClient_ReceiveDataComplete;
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
                    throw new ArgumentNullException();

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
            try
            {
                // Nothing needs to be done before component is initialized.
            }
            catch (Exception)
            {
                // Prevent the IDE from crashing when component is in design mode.
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
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

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
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

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
                throw new InvalidOperationException("RemotingClient property of ClientHelper component is not set.");

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

        private void m_remotingClient_ConnectionAttempt(object sender, System.EventArgs e)
        {
            UpdateStatus("Connecting to {0}...\r\n\r\n", m_remotingClient.ServerUri);
        }

        private void m_remotingClient_ConnectionEstablished(object sender, System.EventArgs e)
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

        private void m_remotingClient_ConnectionTerminated(object sender, System.EventArgs e)
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

        private void m_remotingClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
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
