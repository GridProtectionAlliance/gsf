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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using TVA.Communication;

namespace TVA.Services
{
    /// <summary>
    /// Component that provides client-side functionality to <see cref="ServiceHelper"/>.
    /// </summary>
	[ToolboxBitmap(typeof(ClientHelper))]
    public class ClientHelper : Component
	{
	    #region [ Members ]

        // Events

        /// <summary>
		/// Occurs when a status update is received from the <see cref="ServiceHelper"/>.
		/// </summary>
        [Category("Client"), 
        Description("Occurs when a status update is received from the ServiceHelper.")]
        public event EventHandler<EventArgs<string>> ReceivedServiceUpdate;
				
		/// <summary>
		/// Occurs when a <see cref="ServiceResponse"/> is received from the <see cref="ServiceHelper"/>.
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
        private bool m_disposed;
		
        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientHelper"/> class.
        /// </summary>
        public ClientHelper()
            : base()
        {
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Connects <see cref="RemotingClient"/> to <see cref="ServiceHelper.RemotingServer"/>.
        /// </summary>
        public void Connect()
        {
            if (m_remotingClient == null)
                throw new InvalidOperationException("RemotingClient property of ClientHelper component is not set.");

            m_remotingClient.Connect();
        }

        /// <summary>
        /// Disconnects <see cref="RemotingClient"/> from <see cref="ServiceHelper.RemotingServer"/>.
        /// </summary>
        public void Disconnect()
        {
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
                UpdateStatus(string.Format("Request command \"{0}\" is invalid\r\n\r\b", request));
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
            m_remotingClient.Send(new ClientInfo());

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
        }

        private void m_remotingClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            ServiceResponse response = null;
            Serialization.TryGetObject<ServiceResponse>(e.Argument1.BlockCopy(0, e.Argument2), out response);

            if (response != null)
            {
                OnReceivedServiceResponse(response);

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
                }
            }
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

        #endregion
	}
}
