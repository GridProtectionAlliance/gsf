using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using TVA.Communication;
//using TVA.Serialization;
using TVA.Configuration;

//*******************************************************************************************************
//  TVA.Services.ClientHelper.vb - Helper class for windows service client
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//
//*******************************************************************************************************


namespace TVA.Services
{
	[ToolboxBitmap(typeof(ClientHelper))]public partial class ClientHelper : TVA.Configuration.IPersistSettings, ISupportInitialize
	{
		
		
		#region " Variables "
		
		private string m_serviceName;
		private bool m_persistSettings;
		private string m_settingsCategoryName;
		
		private CommunicationClientBase m_remotingClient;
		
		#endregion
		
		#region " Constants "
		
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
		
		#endregion
		
		#region " Events "
		
		/// <summary>
		/// Occurs when the service client must update its status.
		/// </summary>
		public delegate void UpdateClientStatusEventHandler(object Of);
		private UpdateClientStatusEventHandler UpdateClientStatusEvent;
		
		public event UpdateClientStatusEventHandler UpdateClientStatus
		{
			add
			{
				UpdateClientStatusEvent = (UpdateClientStatusEventHandler) System.Delegate.Combine(UpdateClientStatusEvent, value);
			}
			remove
			{
				UpdateClientStatusEvent = (UpdateClientStatusEventHandler) System.Delegate.Remove(UpdateClientStatusEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when a response is received from the service.
		/// </summary>
		public delegate void ReceivedServiceResponseEventHandler(object Of);
		private ReceivedServiceResponseEventHandler ReceivedServiceResponseEvent;
		
		public event ReceivedServiceResponseEventHandler ReceivedServiceResponse
		{
			add
			{
				ReceivedServiceResponseEvent = (ReceivedServiceResponseEventHandler) System.Delegate.Combine(ReceivedServiceResponseEvent, value);
			}
			remove
			{
				ReceivedServiceResponseEvent = (ReceivedServiceResponseEventHandler) System.Delegate.Remove(ReceivedServiceResponseEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service state changes.
		/// </summary>
		public delegate void ServiceStateChangedEventHandler(object Of);
		private ServiceStateChangedEventHandler ServiceStateChangedEvent;
		
		public event ServiceStateChangedEventHandler ServiceStateChanged
		{
			add
			{
				ServiceStateChangedEvent = (ServiceStateChangedEventHandler) System.Delegate.Combine(ServiceStateChangedEvent, value);
			}
			remove
			{
				ServiceStateChangedEvent = (ServiceStateChangedEventHandler) System.Delegate.Remove(ServiceStateChangedEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the state of a process changes.
		/// </summary>
		public delegate void ProcessStateChangedEventHandler(object Of);
		private ProcessStateChangedEventHandler ProcessStateChangedEvent;
		
		public event ProcessStateChangedEventHandler ProcessStateChanged
		{
			add
			{
				ProcessStateChangedEvent = (ProcessStateChangedEventHandler) System.Delegate.Combine(ProcessStateChangedEvent, value);
			}
			remove
			{
				ProcessStateChangedEvent = (ProcessStateChangedEventHandler) System.Delegate.Remove(ProcessStateChangedEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when a remote command session has been established.
		/// </summary>
		private EventHandler CommandSessionEstablishedEvent;
		public event EventHandler CommandSessionEstablished
		{
			add
			{
				CommandSessionEstablishedEvent = (EventHandler) System.Delegate.Combine(CommandSessionEstablishedEvent, value);
			}
			remove
			{
				CommandSessionEstablishedEvent = (EventHandler) System.Delegate.Remove(CommandSessionEstablishedEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when a remote command session has been terminated.
		/// </summary>
		private EventHandler CommandSessionTerminatedEvent;
		public event EventHandler CommandSessionTerminated
		{
			add
			{
				CommandSessionTerminatedEvent = (EventHandler) System.Delegate.Combine(CommandSessionTerminatedEvent, value);
			}
			remove
			{
				CommandSessionTerminatedEvent = (EventHandler) System.Delegate.Remove(CommandSessionTerminatedEvent, value);
			}
		}
		
		
		#endregion
		
		#region " Properties "
		
		[Category("Service"), DefaultValue(DefaultServiceName)]public string ServiceName
		{
			get
			{
				return m_serviceName;
			}
			set
			{
				if (! string.IsNullOrEmpty(value))
				{
					m_serviceName = value;
				}
				else
				{
					throw (new ArgumentNullException("ServiceName"));
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the instance of TCP client used for communicating with the service.
		/// </summary>
		/// <value></value>
		/// <returns>An instance of TCP client.</returns>
		[Category("Service")]public CommunicationClientBase RemotingClient
		{
			get
			{
				return m_remotingClient;
			}
			set
			{
				m_remotingClient = value;
				m_remotingClient.Connected += new System.EventHandler(m_remotingClient_Connected);
				m_remotingClient.Connecting += new System.EventHandler(m_remotingClient_Connecting);
				m_remotingClient.Disconnected += new System.EventHandler(m_remotingClient_Disconnected);
				m_remotingClient.ReceivedData += new System.EventHandler`1[[TVA.GenericEventArgs`1[[TVA.IdentifiableItem`2[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Byte[], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_remotingClient_ReceivedData);
			}
		}
		
		[Category("Persistance"), DefaultValue(DefaultPersistSettings)]public bool PersistSettings
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
		
		[Category("Persistance"), DefaultValue(DefaultSettingsCategoryName)]public string SettingsCategoryName
		{
			get
			{
				return m_settingsCategoryName;
			}
			set
			{
				if (! string.IsNullOrEmpty(value))
				{
					m_settingsCategoryName = value;
				}
				else
				{
					throw (new ArgumentNullException("SettingsCategoryName"));
				}
			}
		}
		
		#endregion
		
		#region " Methods "
		
		/// <summary>
		/// Attempts to connect to the service.
		/// </summary>
		/// <remarks>This method must be called in order to establish connection with the service.</remarks>
		public void Connect()
		{
			
			if (m_remotingClient != null)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.AppendFormat("Connecting to {0} [{1}]", m_serviceName, DateTime.Now.ToString());
				with_1.AppendLine();
				with_1.Append(".");
				with_1.AppendLine();
				with_1.Append(".");
				
				UpdateStatus(with_1.ToString(), 1);
				
				// We'll always use handshaking to ensure the availability of SecureSession.
				m_remotingClient.Handshake = true;
				m_remotingClient.HandshakePassphrase = m_serviceName;
				
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
			{
				SendRequest(requestInstance);
			}
			else
			{
				UpdateStatus(string.Format("Request command \"{0}\" is invalid", request), ServiceHelper.UpdateCrlfCount);
			}
			
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
			
			System.Text.StringBuilder with_1 = new StringBuilder();
			with_1.Append(message);
			
			for (int i = 0; i <= crlfCount - 1; i++)
			{
				with_1.AppendLine();
			}
			
			if (UpdateClientStatusEvent != null)
				UpdateClientStatusEvent(this, new GenericEventArgs<string>(with_1.ToString()));
			
		}
		
		public void LoadSettings()
		{
			
			try
			{
				TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
				if (with_1.Count > 0)
				{
					ServiceName = with_1.Item("ServiceName").GetTypedValue(m_serviceName);
				}
			}
			catch (Exception)
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
					TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
					with_1.Clear();
					object with_2 = with_1.Item("ServiceName", true);
					with_2.Value = m_serviceName;
					with_2.Description = "";
					TVA.Configuration.Common.SaveSettings();
				}
				catch (Exception)
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
		
		#endregion
		
		#region " Handlers "
		
		private void m_remotingClient_Connected(object sender, System.EventArgs e)
		{
			
			// Upon establishing connection with the service's communication client, we'll send our information to the
			// service so the service can keep track of all the client that are connected to its communication server.
			m_remotingClient.Send(new ClientInfo(m_remotingClient.ClientID));
			
			System.Text.StringBuilder with_1 = new StringBuilder();
			with_1.AppendFormat("Connected to {0} [{1}]", m_serviceName, DateTime.Now.ToString());
			with_1.AppendLine();
			with_1.AppendLine();
			with_1.Append(m_remotingClient.Status);
			
			UpdateStatus(with_1.ToString(), 1);
			
		}
		
		private void m_remotingClient_Connecting(object sender, System.EventArgs e)
		{
			
			UpdateStatus(".", 1);
			
		}
		
		private void m_remotingClient_Disconnected(object sender, System.EventArgs e)
		{
			
			System.Text.StringBuilder with_1 = new StringBuilder();
			with_1.AppendFormat("Disconnected from {0} [{1}]", m_serviceName, DateTime.Now.ToString());
			with_1.AppendLine();
			with_1.AppendLine();
			with_1.Append(m_remotingClient.Status);
			
			UpdateStatus(with_1.ToString(), 1);
			
		}
		
		private void m_remotingClient_ReceivedData(object sender, GenericEventArgs<IdentifiableItem<System.Guid, byte[]>> e)
		{
			
			ServiceResponse response = TVA.Serialization.GetObject<ServiceResponse>(e.Argument.Item);
			if (response != null)
			{
				if (ReceivedServiceResponseEvent != null)
					ReceivedServiceResponseEvent(this, new GenericEventArgs<ServiceResponse>(response));
				switch (response.Type)
				{
					case "UPDATECLIENTSTATUS":
						UpdateStatus(response.Message);
						break;
					case "SERVICESTATECHANGED":
						if (response.Attachments.Count > 0)
						{
							ObjectState<ServiceState> state = response.Attachments(0) as ObjectState<ServiceState>;
							
							if (state != null)
							{
								// Notify change in service state by raising the ServiceStateChanged event.
								if (ServiceStateChangedEvent != null)
									ServiceStateChangedEvent(this, new GenericEventArgs<ObjectState<ServiceState>>(state));
								
								UpdateStatus(string.Format("State of service \"{0}\" has changed to \"{1}\".", state.ObjectName, state.CurrentState), ServiceHelper.UpdateCrlfCount);
							}
						}
						break;
					case "PROCESSSTATECHANGED":
						if (response.Attachments.Count > 0)
						{
							ObjectState<ProcessState> state = response.Attachments(0) as ObjectState<ProcessState>;
							
							if (state != null)
							{
								// Notify change in process state by raising the ProcessStateChanged event.
								if (ProcessStateChangedEvent != null)
									ProcessStateChangedEvent(this, new GenericEventArgs<ObjectState<ProcessState>>(state));
								
								UpdateStatus(string.Format("State of process \"{0}\" has changed to \"{1}\".", state.ObjectName, state.CurrentState), ServiceHelper.UpdateCrlfCount);
							}
						}
						break;
					case "COMMANDSESSION":
						switch (response.Message.ToUpper())
						{
							case "ESTABLISHED":
								if (CommandSessionEstablishedEvent != null)
									CommandSessionEstablishedEvent(this, EventArgs.Empty);
								break;
							case "TERMINATED":
								if (CommandSessionTerminatedEvent != null)
									CommandSessionTerminatedEvent(this, EventArgs.Empty);
								break;
						}
						break;
				}
			}
			
		}
		
		#endregion
		
		#region " Obsolete "
		
		[Obsolete("Property is replaced by RemotingClient and will be deleted in version 3.3."), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]public CommunicationClientBase CommunicationClient
		{
			get
			{
				return m_remotingClient;
			}
			set
			{
				m_remotingClient = value;
				m_remotingClient.Connected += new System.EventHandler(m_remotingClient_Connected);
				m_remotingClient.Connecting += new System.EventHandler(m_remotingClient_Connecting);
				m_remotingClient.Disconnected += new System.EventHandler(m_remotingClient_Disconnected);
				m_remotingClient.ReceivedData += new System.EventHandler`1[[TVA.GenericEventArgs`1[[TVA.IdentifiableItem`2[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Byte[], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_remotingClient_ReceivedData);
			}
		}
		
		#endregion
		
	}
}
