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
//  09/30/2008 - James R Carroll
//       Convert to C#.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using TVA.Communication;
using TVA.Configuration;

namespace TVA.Services
{
    /// <summary>Helper class for windows service client.</summary>
	[ToolboxBitmap(typeof(ClientHelper))]
    public class ClientHelper : Component, IPersistSettings, ISupportInitialize
	{
	    #region [ Members ]

        // Constants

		/// <summary>
        /// Default value for ServiceName property.
        /// </summary>
		public const string DefaultServiceName = "WindowsService";
		
		/// <summary>
        /// Default value for PersistSettings property.
        /// </summary>
		public const bool DefaultPersistSettings = false;
		
		/// <summary>
        /// Default value for SettingsCategoryName property.
        /// </summary>
		public const string DefaultSettingsCategoryName = "ClientHelper";

        // Events

        /// <summary>
		/// Occurs when the service client must update its status.
		/// </summary>
		public event EventHandler<EventArgs<string>> UpdateClientStatus;
				
		/// <summary>
		/// Occurs when a response is received from the service.
		/// </summary>
		public event EventHandler<EventArgs<ServiceResponse>> ReceivedServiceResponse;
		
		/// <summary>
		/// Occurs when the service state changes.
		/// </summary>
		public event EventHandler<EventArgs<ObjectState<ServiceState>>> ServiceStateChanged;
		
		/// <summary>
		/// Occurs when the state of a process changes.
		/// </summary>
		public event EventHandler<EventArgs<ObjectState<ProcessState>>> ProcessStateChanged;
		
		/// <summary>
		/// Occurs when a remote command session has been established.
		/// </summary>
		public event EventHandler CommandSessionEstablished;	
		
		/// <summary>
		/// Occurs when a remote command session has been terminated.
		/// </summary>
		public event EventHandler CommandSessionTerminated;

        // Fields
		private string m_serviceName;
		private bool m_persistSettings;
		private string m_settingsCategoryName;		
		private CommunicationClientBase m_remotingClient;
        private bool m_disposed;
		
        #endregion

        #region [ Constructors ]
		
        public ClientHelper()
		{
			m_serviceName = DefaultServiceName;
			m_persistSettings = DefaultPersistSettings;
			m_settingsCategoryName = DefaultSettingsCategoryName;
		}

        #endregion

        #region [ Properties ]

        [Category("Service"), DefaultValue(DefaultServiceName)]
        public string ServiceName
        {
            get
            {
                return m_serviceName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_serviceName = value;
                else
                    throw new ArgumentNullException("ServiceName");
            }
        }

        /// <summary>
        /// Gets or sets the instance of TCP client used for communicating with the service.
        /// </summary>
        /// <value></value>
        /// <returns>An instance of TCP client.</returns>
        [Category("Service")]
        public CommunicationClientBase RemotingClient
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
                    m_remotingClient.Connected -= m_remotingClient_Connected;
                    m_remotingClient.Connecting -= m_remotingClient_Connecting;
                    m_remotingClient.Disconnected -= m_remotingClient_Disconnected;
                    m_remotingClient.ReceivedData -= m_remotingClient_ReceivedData;
                }

                m_remotingClient = value;

                if (m_remotingClient != null)
                {
                    // Attach events to new instance
                    m_remotingClient.Connected += m_remotingClient_Connected;
                    m_remotingClient.Connecting += m_remotingClient_Connecting;
                    m_remotingClient.Disconnected += m_remotingClient_Disconnected;
                    m_remotingClient.ReceivedData += m_remotingClient_ReceivedData;
                }
            }
        }

        [Category("Persistance"), DefaultValue(DefaultPersistSettings)]
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

        [Category("Persistance"), DefaultValue(DefaultSettingsCategoryName)]
        public string SettingsCategoryName
        {
            get
            {
                return m_settingsCategoryName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_settingsCategoryName = value;
                else
                    throw new ArgumentNullException("SettingsCategoryName");
            }
        }

        #endregion

        #region [ Methods ]
				
		/// <summary>
		/// Releases the unmanaged resources used by an instance of the <see cref="ClientHelper" /> class and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!m_disposed)
			{
			    try
			    {			
				    if (disposing)
				    {
				        Disconnect();
				        SaveSettings();

                        // Detach any remoting client events, we don't own this component so we don't dispose it
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

        /// <summary>
        /// Attempts to connect to the service.
        /// </summary>
        /// <remarks>This method must be called in order to establish connection with the service.</remarks>
        public void Connect()
        {
            if (m_remotingClient != null)
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("Connecting to {0} [{1}]", m_serviceName, DateTime.Now.ToString());
                status.AppendLine();
                status.Append(".");
                status.AppendLine();
                status.Append(".");

                UpdateStatus(status.ToString(), 1);

                // JRC: Disabled override of remoting client handshake operations since ClientHelper doesn't
                // own remoting server - consumer does - and they may have specially defined transport options                
                //m_remotingClient.Handshake = true;
                //m_remotingClient.HandshakePassphrase = m_serviceName;

                // Initiate connection to the service's communication server.
                m_remotingClient.Connect();
            }
            else
            {
                UpdateStatus(string.Format("Cannot connect to {0}. No communication client is specified.", m_serviceName));
            }
        }

        public void Disconnect()
        {
            m_remotingClient.Disconnect();
        }

        public void SendRequest(string request)
        {
            ClientRequest requestInstance = ClientRequest.Parse(request);

            if (requestInstance != null)
                SendRequest(requestInstance);
            else
                UpdateStatus(string.Format("Request command \"{0}\" is invalid", request), ServiceHelper.UpdateCrlfCount);
        }

        /// <summary>
        /// Sends a request to the service.
        /// </summary>
        /// <param name="request">The request to be sent to the service.</param>
        public void SendRequest(ClientRequest request)
        {
            m_remotingClient.Send(request);
        }

        public void UpdateStatus(string message)
        {
            UpdateStatus(message, 0);
        }

        public void UpdateStatus(string message, int crlfCount)
        {
            StringBuilder status = new StringBuilder();

            status.Append(message);

            for (int i = 0; i <= crlfCount - 1; i++)
            {
                status.AppendLine();
            }

            if (UpdateClientStatus != null)
                UpdateClientStatus(this, new EventArgs<string>(status.ToString()));
        }

        public void LoadSettings()
        {
            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];

                if (settings.Count > 0)
                {
                    ServiceName = settings["ServiceName"].ValueAs(m_serviceName);
                }
            }
            catch
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];
                    CategorizedSettingsElement setting;

                    settings.Clear();
                    setting = settings["ServiceName", true];
                    setting.Value = m_serviceName;
                    setting.Description = "";

                    ConfigurationFile.Current.Save();
                }
                catch
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        public void BeginInit()
        {
            // We don't need to do anything before the component is initialized.
        }

        public void EndInit()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                LoadSettings(); // Load settings from the config file.
            }
        }

        private void m_remotingClient_Connected(object sender, System.EventArgs e)
        {
            // Upon establishing connection with the service's communication client, we'll send our information to the
            // service so the service can keep track of all the client that are connected to its communication server.
            m_remotingClient.Send(new ClientInfo(m_remotingClient.ClientID));

            StringBuilder status = new StringBuilder();

            status.AppendFormat("Connected to {0} [{1}]", m_serviceName, DateTime.Now.ToString());
            status.AppendLine();
            status.AppendLine();
            status.Append(m_remotingClient.Status);

            UpdateStatus(status.ToString(), 1);
        }

        private void m_remotingClient_Connecting(object sender, System.EventArgs e)
        {
            UpdateStatus(".", 1);
        }

        private void m_remotingClient_Disconnected(object sender, System.EventArgs e)
        {
            StringBuilder status = new StringBuilder();

            status.AppendFormat("Disconnected from {0} [{1}]", m_serviceName, DateTime.Now.ToString());
            status.AppendLine();
            status.AppendLine();
            status.Append(m_remotingClient.Status);

            UpdateStatus(status.ToString(), 1);
        }

        private void m_remotingClient_ReceivedData(object sender, EventArgs<IdentifiableItem<System.Guid, byte[]>> e)
        {
            ServiceResponse response = Serialization.GetObject<ServiceResponse>(e.Argument.Item);

            if (response != null)
            {
                if (ReceivedServiceResponse != null)
                    ReceivedServiceResponse(this, new EventArgs<ServiceResponse>(response));

                switch (response.Type)
                {
                    case "UPDATECLIENTSTATUS":
                        UpdateStatus(response.Message);
                        break;
                    case "SERVICESTATECHANGED":
                        if (response.Attachments.Count > 0)
                        {
                            ObjectState<ServiceState> state = response.Attachments[0] as ObjectState<ServiceState>;

                            if (state != null)
                            {
                                // Notify change in service state by raising the ServiceStateChanged event.
                                if (ServiceStateChanged != null)
                                    ServiceStateChanged(this, new EventArgs<ObjectState<ServiceState>>(state));

                                UpdateStatus(string.Format("State of service \"{0}\" has changed to \"{1}\".", state.ObjectName, state.CurrentState), ServiceHelper.UpdateCrlfCount);
                            }
                        }
                        break;
                    case "PROCESSSTATECHANGED":
                        if (response.Attachments.Count > 0)
                        {
                            ObjectState<ProcessState> state = response.Attachments[0] as ObjectState<ProcessState>;

                            if (state != null)
                            {
                                // Notify change in process state by raising the ProcessStateChanged event.
                                if (ProcessStateChanged != null)
                                    ProcessStateChanged(this, new EventArgs<ObjectState<ProcessState>>(state));

                                UpdateStatus(string.Format("State of process \"{0}\" has changed to \"{1}\".", state.ObjectName, state.CurrentState), ServiceHelper.UpdateCrlfCount);
                            }
                        }
                        break;
                    case "COMMANDSESSION":
                        switch (response.Message.ToUpper())
                        {
                            case "ESTABLISHED":
                                if (CommandSessionEstablished != null)
                                    CommandSessionEstablished(this, EventArgs.Empty);
                                break;
                            case "TERMINATED":
                                if (CommandSessionTerminated != null)
                                    CommandSessionTerminated(this, EventArgs.Empty);
                                break;
                        }
                        break;
                }
            }
        }

        #endregion
	}
}
