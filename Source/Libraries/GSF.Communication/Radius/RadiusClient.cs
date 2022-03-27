//******************************************************************************************************
//  RadiusClient.cs - Gbtc
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
//  11/26/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  04/24/2014 - J. Ritchie Carroll
//       Code clean-up.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using GSF.Parsing;

namespace GSF.Communication.Radius
{
    /// <summary>
    /// Represents a RADIUS communication client.
    /// </summary>
    public class RadiusClient : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default port of the RADIUS server.
        /// </summary>
        public const int DefaultServerPort = 1812;

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 1 (ensuring that the user account is in
        /// the "New Pin" mode) of creating a new pin was successful.
        /// </summary>
        public const string DefaultNewPinModeMessage1 = "Enter a new PIN";

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 2 (new pin is accepted in attempt #1)
        ///  of creating a new pin was successful.
        /// </summary>
        public const string DefaultNewPinModeMessage2 = "Please re-enter new PIN";

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 3 (new pin is accepted in attempts #2)
        /// of creating a new pin was successful.
        /// </summary>
        public const string DefaultNewPinModeMessage3 = "PIN Accepted";

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not a user account is in the "Next Token" mode.
        /// </summary>
        public const string DefaultNextTokenModeMessage = "Wait for token to change";

        // Fields
        private short m_requestAttempts;
        private int m_reponseTimeout;
        private string m_sharedSecret;
        private string m_newPinModeMessage1;
        private string m_newPinModeMessage2;
        private string m_newPinModeMessage3;
        private string m_nextTokenModeMessage;
        private bool m_disposed;
        private byte[] m_responseBytes;
        private readonly UdpClient m_udpClient;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of RADIUS client for sending request to a RADIUS server.
        /// </summary>
        /// <param name="serverName">Name or address of the RADIUS server.</param>
        /// <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
        public RadiusClient(string serverName, string sharedSecret) : this(serverName, DefaultServerPort, sharedSecret)
        {
        }

        /// <summary>
        /// Creates an instance of RADIUS client for sending request to a RADIUS server.
        /// </summary>
        /// <param name="serverName">Name or address of the RADIUS server.</param>
        /// <param name="serverPort">Port number of the RADIUS server.</param>
        /// <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
        /// <remarks></remarks>
        public RadiusClient(string serverName, int serverPort, string sharedSecret)
        {
            SharedSecret = sharedSecret;
            RequestAttempts = 1;
            ReponseTimeout = 15000;
            NewPinModeMessage1 = DefaultNewPinModeMessage1;
            NewPinModeMessage2 = DefaultNewPinModeMessage2;
            NewPinModeMessage3 = DefaultNewPinModeMessage3;
            NextTokenModeMessage = DefaultNextTokenModeMessage;
            m_udpClient = new($"Server={serverName}; RemotePort={serverPort}; LocalPort=0");
            m_udpClient.ReceiveDataComplete += m_udpClient_ReceivedData;
            m_udpClient.Connect(); // Start the connection cycle.
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name or address of the RADIUS server.
        /// </summary>
        /// <value></value>
        /// <returns>Name or address of RADIUS server.</returns>
        public string ServerName
        {
            get => m_udpClient.ConnectionString.ParseKeyValuePairs()["server"];
            set
            {
                CheckDisposed();

                if (!string.IsNullOrEmpty(value))
                {
                    Dictionary<string, string> parts = m_udpClient.ConnectionString.ParseKeyValuePairs();
                    parts["server"] = value;
                    m_udpClient.ConnectionString = parts.JoinKeyValuePairs();
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the port number of the RADIUS server.
        /// </summary>
        /// <value></value>
        /// <returns>Port number of the RADIUS server.</returns>
        public int ServerPort
        {
            get => Convert.ToInt32(m_udpClient.ConnectionString.ParseKeyValuePairs()["remotePort"]);
            set
            {
                CheckDisposed();

                if (value is >= 0 and <= 65535)
                {
                    Dictionary<string, string> parts = m_udpClient.ConnectionString.ParseKeyValuePairs();
                    parts["remotePort"] = value.ToString();
                    m_udpClient.ConnectionString = parts.JoinKeyValuePairs();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 65535.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of time a request is to sent to the server until a valid response is received.
        /// </summary>
        /// <value></value>
        /// <returns>Number of time a request is to sent to the server until a valid response is received.</returns>
        public short RequestAttempts
        {
            get => m_requestAttempts;
            set
            {
                CheckDisposed();

                if (value is >= 1 and <= 10)
                    m_requestAttempts = value;
                else
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 1 and 10.");
            }
        }

        /// <summary>
        /// Gets or sets the time (in milliseconds) to wait for a response from server after sending a request.
        /// </summary>
        /// <value></value>
        /// <returns>Time (in milliseconds) to wait for a response from server after sending a request.</returns>
        public int ReponseTimeout
        {
            get => m_reponseTimeout;
            set
            {
                CheckDisposed();

                if (value is >= 1000 and <= 60000)
                    m_reponseTimeout = value;
                else
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 1000 and 60000.");
            }
        }

        /// <summary>
        /// Gets or sets the shared secret used between the client and server for encryption and authentication.
        /// </summary>
        /// <value></value>
        /// <returns>Shared secret used between the client and server for encryption and authentication.</returns>
        public string SharedSecret
        {
            get => m_sharedSecret;
            set
            {
                CheckDisposed();

                if (!string.IsNullOrEmpty(value))
                    m_sharedSecret = value;
                else
                    throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 1 (ensuring that the user account is in  the
        /// "New Pin" mode) of creating a new pin was successful.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "New Pin" mode's first message.</returns>
        public string NewPinModeMessage1
        {
            get => m_newPinModeMessage1;
            set
            {
                CheckDisposed();

                if (!string.IsNullOrEmpty(value))
                    m_newPinModeMessage1 = value;
                else
                    throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 2 (new pin is accepted in attempt #1) of creating
        /// a new pin was successful.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "New Pin" mode's second message.</returns>
        public string NewPinModeMessage2
        {
            get => m_newPinModeMessage2;
            set
            {
                CheckDisposed();

                if (!string.IsNullOrEmpty(value))
                    m_newPinModeMessage2 = value;
                else
                    throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 3 (new pin is accepted in attempts #2) of creating
        /// a new pin was successful.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "New Pin" mode's third message.</returns>
        public string NewPinModeMessage3
        {
            get => m_newPinModeMessage3;
            set
            {
                CheckDisposed();

                if (!string.IsNullOrEmpty(value))
                    m_newPinModeMessage3 = value;
                else
                    throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not a user account is in the "Next Token" mode.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "Next Token" mode.</returns>
        public string NextTokenModeMessage
        {
            get => m_nextTokenModeMessage;
            set
            {
                CheckDisposed();

                if (!string.IsNullOrEmpty(value))
                    m_nextTokenModeMessage = value;
                else
                    throw new ArgumentNullException(nameof(value));
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Send a request to the server and waits for a response back.
        /// </summary>
        /// <param name="request">Request to be sent to the server.</param>
        /// <returns>Response packet if a valid response is received from the server; otherwise Nothing.</returns>
        public RadiusPacket ProcessRequest(RadiusPacket request)
        {
            CheckDisposed();

            // We wait indefinitely for the connection to establish. But since this is UDP, the connection
            // will always be successful (locally we're binding to any available UDP port).
            if (m_udpClient.CurrentState != ClientState.Connected)
                return null;

            RadiusPacket response = null;

            // We have a UDP socket we can use for exchanging packets.
            for (int i = 1; i <= m_requestAttempts; i++)
            {
                m_responseBytes = null;
                m_udpClient.Send(request.BinaryImage());

                DateTime stopTime = DateTime.UtcNow.AddMilliseconds(m_reponseTimeout);

                while (true)
                {
                    Thread.Sleep(1);

                    // Stay in the loop until:
                    // 1) We receive a response OR
                    // 2) We exceed the response timeout duration
                    if (m_responseBytes is not null || DateTime.UtcNow > stopTime)
                        break;
                }

                if (m_responseBytes is not null)
                {
                    // The server sent a response.
                    response = new(m_responseBytes, 0, m_responseBytes.Length);

                    if (response.Identifier == request.Identifier && response.Authenticator.CompareTo(RadiusPacket.CreateResponseAuthenticator(m_sharedSecret, request, response)) == 0)
                        break;

                    // The response failed the verification, so we'll silently discard it.
                    response = null;
                }
            }

            return response;
        }

        /// <summary>
        /// Create a new pin for the user.
        /// </summary>
        /// <param name="username">Name of the user.</param>
        /// <param name="token">Current token of the user.</param>
        /// <param name="pin">New pin of the user.</param>
        /// <returns>True if a new pin is created for the user successfully; otherwise False.</returns>
        /// <remarks>NOTE: This method is specific to RSA RADIUS implementation.</remarks>
        public bool CreateNewPin(string username, string token, string pin)
        {
            CheckDisposed();

            if (string.IsNullOrEmpty(pin))
                throw new ArgumentNullException(nameof(pin));

            // Step 1: Send username and token for password, receive a challenge response with reply
            //         message worded "Enter a new PIN". [Verification]
            // Step 2: Send username and new ping for password, receive a challenge response with reply
            //         message worded "Please re-enter.  [Attempt #1]
            // Step 3: Send username and new ping for password, receive a challenge response with reply
            //         message worded "PIN Accepted".    [Attempt #2]

            RadiusPacket response = Authenticate(username, token);

            if (!IsUserInNewPinMode(response))
                return false;

            // User account is really in "New Pin" mode.
            response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State));
            byte[] reply = response.GetAttributeValue(AttributeType.ReplyMessage);

            if (!RadiusPacket.Encoding.GetString(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage2.ToLower()))
                return false; // New pin not accepted in attempt #1.

            response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State));
            reply = response.GetAttributeValue(AttributeType.ReplyMessage);

            return RadiusPacket.Encoding.GetString(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage3.ToLower());
        }

        /// <summary>
        /// Authenticates the username and password against the RADIUS server.
        /// </summary>
        /// <param name="username">Username to be authenticated.</param>
        /// <param name="password">Password to be authenticated.</param>
        /// <returns>Response packet received from the server for the authentication request.</returns>
        /// <remarks>
        /// <para>
        /// The type of response packet (if any) will be one of the following:
        /// <list>
        /// <item>AccessAccept: If the authentication is successful.</item>
        /// <item>AccessReject: If the authentication is not successful.</item>
        /// <item>AccessChallenge: If the server need more information from the user.</item>
        /// </list>
        /// </para>
        /// <para>
        /// When an AccessChallenge response packet is received from the server, it contains a State attribute
        /// that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
        /// response. So if this method returns an AccessChallenge packet, then this method is to be called again
        /// with the requested information (from ReplyMessage attribute) in the password field and the value State
        /// attribute.
        /// </para>
        /// </remarks>
        public RadiusPacket Authenticate(string username, string password) => Authenticate(username, password, null);

        /// <summary>
        /// Authenticates the username and password against the RADIUS server.
        /// </summary>
        /// <param name="username">Username to be authenticated.</param>
        /// <param name="password">Password to be authenticated.</param>
        /// <param name="state">State value from a previous challenge response.</param>
        /// <returns>Response packet received from the server for the authentication request.</returns>
        /// <remarks>
        /// <para>
        /// The type of response packet (if any) will be one of the following:
        /// <list>
        /// <item>AccessAccept: If the authentication is successful.</item>
        /// <item>AccessReject: If the authentication is not successful.</item>
        /// <item>AccessChallenge: If the server need more information from the user.</item>
        /// </list>
        /// </para>
        /// <para>
        /// When an AccessChallenge response packet is received from the server, it contains a State attribute
        /// that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
        /// response. So if this method returns an AccessChallenge packet, then this method is to be called again
        /// with the requested information (from ReplyMessage attribute) in the password field and the value State
        /// attribute.
        /// </para>
        /// </remarks>
        public RadiusPacket Authenticate(string username, string password, byte[] state)
        {
            CheckDisposed();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Username and Password cannot be null.");

            RadiusPacket request = new(PacketType.AccessRequest);
            byte[] authenticator = RadiusPacket.CreateRequestAuthenticator(m_sharedSecret);

            request.Authenticator = authenticator;
            request.Attributes.Add(new(AttributeType.UserName, username));
            request.Attributes.Add(new(AttributeType.UserPassword, RadiusPacket.EncryptPassword(password, m_sharedSecret, authenticator)));

            // State attribute is used when responding to a AccessChallenge response.
            if (state is not null)
                request.Attributes.Add(new(AttributeType.State, state));

            return ProcessRequest(request);
        }

        /// <summary>
        /// Determines whether or not the response indicates that the user account is in "New Pin" mode.
        /// </summary>
        /// <param name="response">Response packet sent by the server.</param>
        /// <returns>True if the user account is in "New Pin" mode; otherwise False.</returns>
        /// <remarks>
        /// <para>A user's account can be in the "New Pin" mode when set on the server.</para>
        /// <para>NOTE: This method is specific to RSA RADIUS implementation.</para>
        /// </remarks>
        public bool IsUserInNewPinMode(RadiusPacket response)
        {
            CheckDisposed();

            if (response is null)
                throw new ArgumentNullException(nameof(response));

            byte[] messageBytes = response.GetAttributeValue(AttributeType.ReplyMessage);

            if (messageBytes is null)
                throw new ArgumentException("ReplyMessage attribute is not present", nameof(response));

            // Unfortunately, the only way of determining whether or not a user account is in the
            // "New Pin" mode is from the text present in the ReplyMessage attribute of the
            // AccessChallenge response from server.
            string messageString = RadiusPacket.Encoding.GetString(messageBytes, 0, messageBytes.Length);
            return messageString.ToLower().Contains(m_newPinModeMessage1.ToLower());
        }

        /// <summary>
        /// Determines whether or not the response indicates that the user account is in "Next Token" mode.
        /// </summary>
        /// <param name="response">Response packet sent by the server.</param>
        /// <returns>True if the user account is in "Next Token" mode; otherwise False.</returns>
        /// <remarks>
        /// <para>
        /// A user's account can enter the "Next Token" mode after the user enters incorrect passwords for a few
        /// times (3 times by default) and then enters the correct password. Note that repeatedly entering
        /// incorrect passwords will disable the user account.
        /// </para>
        /// <para>NOTE: This method is specific to RSA RADIUS implementation.</para>
        /// </remarks>
        public bool IsUserInNextTokenMode(RadiusPacket response)
        {
            CheckDisposed();

            if (response is null)
                throw new ArgumentNullException(nameof(response));

            byte[] messageBytes = response.GetAttributeValue(AttributeType.ReplyMessage);

            if (messageBytes is null)
                throw new ArgumentException("ReplyMessage attribute is not present", nameof(response));

            // Unfortunately, the only way of determining whether or not a user account is in the
            // "Next Token" mode is from the text present in the ReplyMessage attribute of the
            // AccessChallenge response from server.
            string messageString = RadiusPacket.Encoding.GetString(messageBytes, 0, messageBytes.Length);
            return messageString.ToLower().Contains(m_nextTokenModeMessage.ToLower());
        }

        /// <summary>
        /// Releases the used resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Helper method to check whether or not the object instance has been disposed.
        /// </summary>
        /// <remarks>This method is to be called before performing any operation.</remarks>
        protected void CheckDisposed()
        {
            if (m_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// Releases the used resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    if (m_udpClient is not null)
                    {
                        m_udpClient.ReceiveDataComplete -= m_udpClient_ReceivedData;
                        m_udpClient.Dispose();
                    }
                }
            }
            m_disposed = true;
        }

        private void m_udpClient_ReceivedData(object sender, EventArgs<byte[], int> e) => m_responseBytes = e.Argument1;

        #endregion
    }
}