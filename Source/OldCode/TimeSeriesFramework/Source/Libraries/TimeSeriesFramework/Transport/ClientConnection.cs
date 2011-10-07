//******************************************************************************************************
//  ClientConnection.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/24/2011 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using TVA;
using TVA.Communication;
using TVA.Security.Cryptography;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents a <see cref="DataSubscriber"/> client connection to the <see cref="DataPublisher"/>.
    /// </summary>
    public class ClientConnection : IProvideStatus, IDisposable
    {
        #region [ Members ]

        // Constants
        private const int EvenKey = 0;      // Even key/IV index
        private const int OddKey = 1;       // Odd key/IV index
        private const int KeyIndex = 0;     // Index of cipher key component in keyIV array
        private const int IVIndex = 1;      // Index of initialization vector component in keyIV array

        // Fields
        private DataPublisher m_parent;
        private Guid m_clientID;
        private Guid m_subscriberID;
        private string m_connectionID;
        private string m_hostName;
        private string m_subscriberAcronym;
        private string m_subscriberName;
        private string m_sharedSecret;
        private IClientSubscription m_subscription;
        private volatile bool m_authenticated;
        private volatile byte[][][] m_keyIVs;
        private volatile int m_cipherIndex;
        private IPAddress m_ipAddress;
        private TcpServer m_commandChannel;
        private UdpServer m_dataChannel;
        private System.Timers.Timer m_pingTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ClientConnection"/> instance.
        /// </summary>
        /// <param name="parent">Parent data publisher.</param>
        /// <param name="clientID">Client ID of associated connection.</param>
        /// <param name="commandChannel"><see cref="TcpServer"/> command channel used to lookup connection information.</param>
        public ClientConnection(DataPublisher parent, Guid clientID, TcpServer commandChannel)
        {
            m_parent = parent;
            m_clientID = clientID;
            m_commandChannel = commandChannel;
            m_subscriberID = clientID;
            m_keyIVs = null;
            m_cipherIndex = 0;

            m_pingTimer = new System.Timers.Timer();
            m_pingTimer.Interval = 5000.0D;
            m_pingTimer.AutoReset = true;
            m_pingTimer.Elapsed += m_pingTimer_Elapsed;
            m_pingTimer.Start();

            // Attempt to lookup remote connection identification for logging purposes
            try
            {
                IPEndPoint remoteEndPoint = null;

                if (commandChannel != null)
                    remoteEndPoint = commandChannel.Client(clientID).Provider.RemoteEndPoint as IPEndPoint;

                if (remoteEndPoint != null)
                {
                    m_ipAddress = remoteEndPoint.Address;

                    if (remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                        m_connectionID = "[" + m_ipAddress + "]:" + remoteEndPoint.Port;
                    else
                        m_connectionID = m_ipAddress + ":" + remoteEndPoint.Port;

                    try
                    {
                        IPHostEntry ipHost = Dns.GetHostEntry(remoteEndPoint.Address);

                        if (!string.IsNullOrWhiteSpace(ipHost.HostName))
                        {
                            m_hostName = ipHost.HostName;
                            m_connectionID = m_hostName + " (" + m_connectionID + ")";
                        }
                    }
                    catch
                    {
                        // Just ignoring possible DNS lookup failures...
                    }
                }
            }
            catch
            {
                // At worst we'll just use the client GUID for identification
                m_connectionID = (m_subscriberID == Guid.Empty ? clientID.ToString() : m_subscriberID.ToString());
            }

            if (string.IsNullOrWhiteSpace(m_connectionID))
                m_connectionID = "unavailable";

            if (string.IsNullOrWhiteSpace(m_hostName))
            {
                if (m_ipAddress != null)
                    m_hostName = m_ipAddress.ToString();
                else
                    m_hostName = m_connectionID;
            }

            if (m_ipAddress == null)
                m_ipAddress = System.Net.IPAddress.None;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ClientConnection"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ClientConnection()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets client ID of this <see cref="ClientConnection"/>.
        /// </summary>
        public Guid ClientID
        {
            get
            {
                return m_clientID;
            }
        }

        /// <summary>
        /// Gets or sets reference to <see cref="UdpServer"/> data channel, attaching to or detaching from events as needed, associated with this <see cref="ClientConnection"/>.
        /// </summary>
        public UdpServer DataChannel
        {
            get
            {
                return m_dataChannel;
            }
            set
            {
                if (m_dataChannel != null)
                {
                    // Detach from events on existing data channel reference
                    m_dataChannel.SendClientDataException -= m_dataChannel_SendClientDataException;
                    m_dataChannel.ServerStarted -= m_dataChannel_ServerStarted;
                    m_dataChannel.ServerStopped -= m_dataChannel_ServerStopped;

                    if (m_dataChannel != value)
                        m_dataChannel.Dispose();
                }

                // Assign new data channel reference
                m_dataChannel = value;

                if (m_dataChannel != null)
                {
                    // Attach to events on new data channel reference
                    m_dataChannel.SendClientDataException += m_dataChannel_SendClientDataException;
                    m_dataChannel.ServerStarted += m_dataChannel_ServerStarted;
                    m_dataChannel.ServerStopped += m_dataChannel_ServerStopped;
                }
            }
        }

        /// <summary>
        /// Gets <see cref="IServer"/> publication channel - that is, data channel if defined otherwise command channel.
        /// </summary>
        public IServer PublishChannel
        {
            get
            {
                return (m_dataChannel == null ? (IServer)m_commandChannel : (IServer)m_dataChannel);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based subscriber ID of this <see cref="ClientConnection"/>.
        /// </summary>
        public Guid SubscriberID
        {
            get
            {
                return m_subscriberID;
            }
            set
            {
                m_subscriberID = value;
            }
        }

        /// <summary>
        /// Gets or sets the subscriber acronym of this <see cref="ClientConnection"/>.
        /// </summary>
        public string SubscriberAcronym
        {
            get
            {
                return m_subscriberAcronym;
            }
            set
            {
                m_subscriberAcronym = value;
            }
        }

        /// <summary>
        /// Gets or sets the subscriber name of this <see cref="ClientConnection"/>.
        /// </summary>
        public string SubscriberName
        {
            get
            {
                return m_subscriberName;
            }
            set
            {
                m_subscriberName = value;
            }
        }

        /// <summary>
        /// Gets the connection identification of this <see cref="ClientConnection"/>.
        /// </summary>
        public string ConnectionID
        {
            get
            {
                return m_connectionID;
            }
        }

        /// <summary>
        /// Gets or sets authenticated state of this <see cref="ClientConnection"/>.
        /// </summary>
        public bool Authenticated
        {
            get
            {
                return m_authenticated;
            }
            set
            {
                m_authenticated = value;
            }
        }

        /// <summary>
        /// Gets or sets shared secret used to lookup cipher keys only known to client and server.
        /// </summary>
        public string SharedSecret
        {
            get
            {
                return m_sharedSecret;
            }
            set
            {
                m_sharedSecret = value;
            }
        }

        /// <summary>
        /// Gets active cipher key, if any.
        /// </summary>
        public byte[] Key
        {
            get
            {
                if (m_keyIVs != null)
                    return m_keyIVs[m_cipherIndex][KeyIndex];

                return null;
            }
        }

        /// <summary>
        /// Gets active intitialization vector, if any.
        /// </summary>
        public byte[] IV
        {
            get
            {
                if (m_keyIVs != null)
                    return m_keyIVs[m_cipherIndex][IVIndex];

                return null;
            }
        }

        /// <summary>
        /// Gets active and standby keys and initialization vectors.
        /// </summary>
        public byte[][][] KeyIVs
        {
            get
            {
                return m_keyIVs;
            }
        }

        /// <summary>
        /// Gets current cipher index.
        /// </summary>
        public int CipherIndex
        {
            get
            {
                return m_cipherIndex;
            }
        }

        /// <summary>
        /// Gets the IP address of the remote client connection.
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                return m_ipAddress;
            }
        }

        /// <summary>
        /// Gets or sets subscription associated with this <see cref="ClientConnection"/>.
        /// </summary>
        public IClientSubscription Subscription
        {
            get
            {
                return m_subscription;
            }
            set
            {
                m_subscription = value;
                m_subscription.HostName = m_hostName;
            }
        }

        /// <summary>
        /// Gets the subscriber name of this <see cref="ClientConnection"/>.
        /// </summary>
        public string Name
        {
            get
            {
                return SubscriberName;
            }
        }

        /// <summary>
        /// Gets a formatted message describing the status of this <see cref="ClientConnection"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                string formatString = "{0,26}: {1}";

                status.AppendLine();
                status.AppendFormat(formatString, "Subscriber ID", m_connectionID);
                status.AppendLine();
                status.AppendFormat(formatString, "Subscriber name", SubscriberName);
                status.AppendLine();
                status.AppendFormat(formatString, "Subscriber acronym", SubscriberAcronym);
                status.AppendLine();
                status.AppendFormat(formatString, "Publish channel protocol", PublishChannel.TransportProtocol);
                status.AppendLine();

                if (m_dataChannel != null)
                {
                    status.AppendLine();
                    status.Append(m_dataChannel.Status);
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ClientConnection"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ClientConnection"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_pingTimer != null)
                        {
                            m_pingTimer.Elapsed -= m_pingTimer_Elapsed;
                            m_pingTimer.Dispose();
                        }
                        m_pingTimer = null;

                        DataChannel = null;
                        m_commandChannel = null;
                        m_ipAddress = null;
                        m_subscription = null;
                        m_parent = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Creates or updates cipher keys.
        /// </summary>
        public void UpdateKeyIVs()
        {
            AesManaged symmetricAlgorithm = new AesManaged();

            symmetricAlgorithm.KeySize = 256;
            symmetricAlgorithm.GenerateKey();
            symmetricAlgorithm.GenerateIV();

            if (m_keyIVs == null)
            {
                // Initialize new key set
                m_keyIVs = new byte[2][][];
                m_keyIVs[EvenKey] = new byte[2][];
                m_keyIVs[OddKey] = new byte[2][];

                m_keyIVs[EvenKey][KeyIndex] = symmetricAlgorithm.Key;
                m_keyIVs[EvenKey][IVIndex] = symmetricAlgorithm.IV;

                symmetricAlgorithm.GenerateKey();
                symmetricAlgorithm.GenerateIV();

                m_keyIVs[OddKey][KeyIndex] = symmetricAlgorithm.Key;
                m_keyIVs[OddKey][IVIndex] = symmetricAlgorithm.IV;

                m_cipherIndex = EvenKey;
            }
            else
            {
                // Generate a new key set for current cipher index
                m_keyIVs[m_cipherIndex][KeyIndex] = symmetricAlgorithm.Key;
                m_keyIVs[m_cipherIndex][IVIndex] = symmetricAlgorithm.IV;

                // Set run-time to the other key set
                m_cipherIndex = (m_cipherIndex == EvenKey ? OddKey : EvenKey);
            }
        }

        /// <summary>
        /// Rotates or initializes the crypto keys for this <see cref="ClientConnection"/>.
        /// </summary>
        public void RotateCipherKeys()
        {
            MemoryStream response = new MemoryStream();

            // Create or update cipher keys and initialization vectors 
            UpdateKeyIVs();

            // Add current cipher index to response
            response.Write(EndianOrder.BigEndian.GetBytes(m_cipherIndex), 0, 4);

            // Serialize new keys
            byte[] bytes = Serialization.Serialize(m_keyIVs, TVA.SerializationFormat.Binary);

            // Encrypt keys using private keys known only to current client and server
            if (m_authenticated && !string.IsNullOrWhiteSpace(m_sharedSecret))
                bytes = bytes.Encrypt(m_sharedSecret, CipherStrength.Aes256);

            // Add serialized key response
            response.Write(bytes, 0, bytes.Length);

            // Send cipher key updates
            m_parent.SendClientResponse(m_clientID, ServerResponse.UpdateCipherKeys, ServerCommand.Subscribe, response.ToArray());
        }

        private void m_pingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Send a no-op keep-alive ping to make sure the client is still connected
            m_parent.SendClientResponse(m_clientID, ServerResponse.NoOP, ServerCommand.Subscribe);
        }

        private void m_dataChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;
            m_parent.OnProcessException(new InvalidOperationException(string.Format("Data channel exception occurred while sending client data to \"{0}\": {1}", m_connectionID, ex.Message), ex));
        }

        private void m_dataChannel_ServerStarted(object sender, EventArgs e)
        {
            m_parent.OnStatusMessage("Data channel started.");
        }

        private void m_dataChannel_ServerStopped(object sender, EventArgs e)
        {
            m_parent.OnStatusMessage("Data channel stopped.");
        }

        #endregion
    }
}
