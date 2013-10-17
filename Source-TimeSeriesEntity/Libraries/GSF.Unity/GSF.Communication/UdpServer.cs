//*******************************************************************************************************
//  UdpServer.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/09/2009 - Pinal C. Patel
//       Modified to attempt resuming reception on SocketException for non-Handshake enabled connection.
//  07/17/2009 - Pinal C. Patel
//       Added support to specify a specific interface address on a multiple interface machine.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/14/2009 - Pinal C. Patel
//       Fixed bug in the processing of Handshake messages.
//       Added null reference checks to Stop() and DisconnectOne() for safety.
//  10/30/2009 - Pinal C. Patel
//       Added support for one-way communication by specifying Port=-1 in ConfigurationString.
//  04/29/2010 - Pinal C. Patel
//       Modified Start() to parse client endpoint strings correctly to address IPv6 IP parsing issue.
//  02/13/2011 - Pinal C. Patel
//       Modified Start() to use "interface" in the creation of client endpoint.
//  03/10/2011 - Pinal C. Patel
//       Fixed a bug reported by Jeffrey Martin at Areva-TD (jeffrey.martin-econ@areva-td.com) that
//       prevented the ServerStopped event from being raised under certain configuration.
//  12/04/2011 - J. Ritchie Carroll
//       Modified to use concurrent dictionary.
//  07/23/2012 - Stephen C. Wills
//       Performed a full refactor to use the SocketAsyncEventArgs API calls.
//  10/31/2012 - Stephen C. Wills
//       Replaced single-threaded BlockingCollection pattern with asynchronous loop pattern.
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
 Original Software Title: The GSF Open Source Phasor Data Concentrator
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

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using GSF.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a UDP-based communication server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use <see cref="UdpServer"/> when the primary purpose is to transmit data.
    /// </para>
    /// <para>
    /// The <see cref="UdpServer.Server"/> socket can be bound to a specified interface on a machine with multiple interfaces by 
    /// specifying the interface in the <see cref="ServerBase.ConfigurationString"/> (Example: "Port=8888; Clients=localhost:8989; Interface=127.0.0.1")
    /// </para>
    /// <para>
    /// The <see cref="UdpServer.Server"/> socket can be used just for transmitting data without being bound to a local interface 
    /// by specifying -1 for the port number in the <see cref="ServerBase.ConfigurationString"/> (Example: "Port=-1; Clients=localhost:8989")
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="UdpServer"/> component:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Communication;
    /// using GSF.Security.Cryptography;
    /// using GSF.IO.Compression;
    /// 
    /// class Program
    /// {
    ///     static UdpServer m_server;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the server.
    ///         m_server = new UdpServer("Port=8888; Clients=localhost:8989");
    ///         m_server.Handshake = false;
    ///         m_server.ReceiveTimeout = -1;
    ///         m_server.Encryption = CipherStrength.None;
    ///         m_server.Compression = CompressionStrength.NoCompression;
    ///         m_server.SecureSession = false;
    ///         m_server.Initialize();
    ///         // Register event handlers.
    ///         m_server.ServerStarted += m_server_ServerStarted;
    ///         m_server.ServerStopped += m_server_ServerStopped;
    ///         m_server.ClientConnected += m_server_ClientConnected;
    ///         m_server.ClientDisconnected += m_server_ClientDisconnected;
    ///         m_server.ReceiveClientDataComplete += m_server_ReceiveClientDataComplete;
    ///         // Start the server.
    ///         m_server.Start();
    /// 
    ///         // Multicast user input to all connected clients.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_server.Multicast(input);
    ///         }
    /// 
    ///         // Stop the server on shutdown.
    ///         m_server.Stop();
    ///     }
    /// 
    ///     static void m_server_ServerStarted(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Server has been started!");
    ///     }
    /// 
    ///     static void m_server_ServerStopped(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Server has been stopped!");
    ///     }
    /// 
    ///     static void m_server_ClientConnected(object sender, EventArgs&lt;Guid&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Client connected - {0}.", e.Argument));
    ///     }
    /// 
    ///     static void m_server_ClientDisconnected(object sender, EventArgs&lt;Guid&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Client disconnected - {0}.", e.Argument));
    ///     }
    /// 
    ///     static void m_server_ReceiveClientDataComplete(object sender, EventArgs&lt;Guid, byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data from {0} - {1}.", e.Argument1, m_server.TextEncoding.GetString(e.Argument2, 0, e.Argument3)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class UdpServer : ServerBase
    {
        #region [ Members ]

        // Nested Types
        private class UdpClientInfo
        {
            public TransportProvider<EndPoint> Client;
            public SocketAsyncEventArgs SendArgs;
            public SpinLock SendLock;
            public ConcurrentQueue<UdpServerPayload> SendQueue;
            public int Sending;
        }

        private class UdpServerPayload
        {
            // Per payload state
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEventSlim WaitHandle;

            // Per client state
            public UdpClientInfo ClientInfo;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="AllowDualStackSocket"/> property.
        /// </summary>
        public const bool DefaultAllowDualStackSocket = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxSendQueueSize"/> property.
        /// </summary>
        public const int DefaultMaxSendQueueSize = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ServerBase.ConfigurationString"/> property.
        /// </summary>
        public const string DefaultConfigurationString = "Port=8888; Clients=localhost:8989";

        /// <summary>
        /// Specifies the constant to be used for disabling <see cref="SocketError.ConnectionReset"/> when endpoint is not listening.
        /// </summary>
        private const int SIO_UDP_CONNRESET = -1744830452;

        // Fields
        private TransportProvider<Socket> m_udpServer;
        private SocketAsyncEventArgs m_receiveArgs;
        private ConcurrentDictionary<Guid, UdpClientInfo> m_clientInfoLookup;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private Dictionary<string, string> m_configData;

        private EventHandler<SocketAsyncEventArgs> m_sendHandler;
        private EventHandler<SocketAsyncEventArgs> m_receiveHandler;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        public UdpServer()
            : this(DefaultConfigurationString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="configString">Config string of the <see cref="UdpServer"/>. See <see cref="DefaultConfigurationString"/> for format.</param>
        public UdpServer(string configString)
            : base(TransportProtocol.Udp, configString)
        {
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;
            m_clientInfoLookup = new ConcurrentDictionary<Guid, UdpClientInfo>();

            m_sendHandler = (sender, args) => ProcessSend(args);
            m_receiveHandler = (sender, args) => ProcessReceive(args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="UdpServer"/>.</param>
        public UdpServer(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that determines if dual-mode socket is allowed when endpoint address is IPv6.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultAllowDualStackSocket),
        Description("Determines if dual-mode socket is allowed when endpoint address is IPv6.")]
        public bool AllowDualStackSocket
        {
            get
            {
                return m_allowDualStackSocket;
            }
            set
            {
                m_allowDualStackSocket = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size for the send queue before payloads are dumped from the queue.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultMaxSendQueueSize),
        Description("The maximum size for the send queue before payloads are dumped from the queue.")]
        public int MaxSendQueueSize
        {
            get
            {
                return m_maxSendQueueSize;
            }
            set
            {
                m_maxSendQueueSize = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="UdpServer"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<Socket> Server
        {
            get
            {
                return m_udpServer;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);
                int count = 0;

                foreach (ConcurrentQueue<UdpServerPayload> sendQueue in m_clientInfoLookup.Values.Select(clientInfo => clientInfo.SendQueue))
                {
                    statusBuilder.AppendFormat("           Queued payloads: {0} for client {1}", sendQueue.Count, ++count);
                    statusBuilder.AppendLine();
                }

                statusBuilder.AppendFormat("     Wait handle pool size: {0}", ReusableObjectPool<ManualResetEventSlim>.Default.GetPoolSize());
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("         Payload pool size: {0}", ReusableObjectPool<UdpServerPayload>.Default.GetPoolSize());
                statusBuilder.AppendLine();

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="clientID">ID of the client from which data buffer should be read.</param>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ServerBase.ReceiveClientData"/> event handler. Calling this method
        /// outside this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// No received data buffer has been defined to read -or-
        /// Specified <paramref name="clientID"/> does not exist, cannot read buffer.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(Guid clientID, byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            UdpClientInfo clientInfo;
            TransportProvider<EndPoint> udpClient;

            if (m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
            {
                udpClient = clientInfo.Client;

                if ((object)udpClient.ReceiveBuffer != null)
                {
                    int readIndex = ReadIndicies[clientID];
                    int sourceLength = udpClient.BytesReceived - readIndex;
                    int readBytes = length > sourceLength ? sourceLength : length;
                    Buffer.BlockCopy(udpClient.ReceiveBuffer, readIndex, buffer, startIndex, readBytes);

                    // Update read index for next call
                    readIndex += readBytes;

                    if (readIndex >= udpClient.BytesReceived)
                        readIndex = 0;

                    ReadIndicies[clientID] = readIndex;

                    return readBytes;
                }

                throw new InvalidOperationException("No received data buffer has been defined to read.");
            }

            throw new InvalidOperationException("Specified client ID does not exist, cannot read buffer.");
        }

        /// <summary>
        /// Saves <see cref="TcpServer"/> settings to the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            //if (PersistSettings)
            //{
            //    // Save settings under the specified category.
            //    ConfigurationFile config = ConfigurationFile.Current;
            //    CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            //    settings["AllowDualStackSocket", true].Update(m_allowDualStackSocket);
            //    settings["MaxSendQueueSize", true].Update(m_maxSendQueueSize);
            //    config.Save();
            //}
        }

        /// <summary>
        /// Loads saved <see cref="TcpServer"/> settings from the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();
            //if (PersistSettings)
            //{
            //    // Load settings from the specified category.
            //    ConfigurationFile config = ConfigurationFile.Current;
            //    CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            //    settings.Add("AllowDualStackSocket", m_allowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
            //    settings.Add("MaxSendQueueSize", m_allowDualStackSocket, "The maximum size of the send queue before payloads are dumped from the queue.");
            //    AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(m_allowDualStackSocket);
            //    MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(m_maxSendQueueSize);
            //}
        }

        /// <summary>
        /// Stops the <see cref="UdpServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            if (CurrentState == ServerState.Running)
            {
                try
                {
                    // Disconnect all clients.
                    DisconnectAll();
                    m_udpServer.Reset();
                    OnServerStopped();
                }
                finally
                {
                    if ((object)m_receiveArgs != null)
                    {
                        m_receiveArgs.Dispose();
                        m_receiveArgs = null;
                    }
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="UdpServer"/> synchronously and begins accepting client connections asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to <see cref="Start()"/> the <see cref="UdpServer"/> when it is running.</exception>
        public override void Start()
        {
            if (CurrentState == ServerState.NotRunning)
            {
                int maxSendQueueSize;

                // Initialize if unitialized
                if (!Initialized)
                    Initialize();

                // Overwrite config file if max send queue size exists in connection string.
                if (m_configData.ContainsKey("maxSendQueueSize") && int.TryParse(m_configData["maxSendQueueSize"], out maxSendQueueSize))
                    m_maxSendQueueSize = maxSendQueueSize;

                // Bind server socket to local end-point
                m_udpServer = new TransportProvider<Socket>();
                m_udpServer.SetReceiveBuffer(ReceiveBufferSize);
                m_udpServer.Provider = Transport.CreateSocket(m_configData["interface"], int.Parse(m_configData["port"]), ProtocolType.Udp, m_ipStack, m_allowDualStackSocket);

                // Disable SocketError.ConnectionReset exception from being thrown when the endpoint is not listening
                m_udpServer.Provider.IOControl(SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

                // Notify that the server has been started successfully
                OnServerStarted();

                if ((object)m_udpServer.Provider.LocalEndPoint != null)
                {
                    m_receiveArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                    m_receiveArgs.SocketFlags = SocketFlags.None;
                    m_receiveArgs.Completed += m_receiveHandler;
                    ReceivePayloadAsync(m_receiveArgs);
                }

                // We process the static list to clients.
                foreach (string clientString in m_configData["clients"].Replace(" ", "").Split(','))
                {
                    try
                    {
                        Match endpoint = Regex.Match(clientString, Transport.EndpointFormatRegex);

                        if (endpoint != Match.Empty)
                        {
                            UdpClientInfo clientInfo;
                            TransportProvider<EndPoint> udpClient = new TransportProvider<EndPoint>();
                            IPEndPoint clientEndpoint = Transport.CreateEndPoint(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value), m_ipStack);

                            udpClient.SetReceiveBuffer(ReceiveBufferSize);
                            udpClient.SetSendBuffer(SendBufferSize);
                            udpClient.Provider = clientEndpoint;

                            // If the IP specified for the client is a multicast IP, subscribe to the specified multicast group.
                            if (Transport.IsMulticastIP(clientEndpoint.Address))
                            {
                                string multicastSource;

                                if (m_configData.TryGetValue("multicastSource", out multicastSource))
                                {
                                    IPAddress sourceAddress = IPAddress.Parse(multicastSource);
                                    IPAddress localAddress = (clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any);

                                    if (sourceAddress.AddressFamily != clientEndpoint.AddressFamily)
                                        throw new InvalidOperationException(string.Format("Source address \"{0}\" is not in the same IP format as server address \"{1}\"", sourceAddress, clientEndpoint.Address));

                                    if (localAddress.AddressFamily != clientEndpoint.AddressFamily)
                                        throw new InvalidOperationException(string.Format("Local address \"{0}\" is not in the same IP format as server address \"{1}\"", localAddress, clientEndpoint.Address));

                                    MemoryStream membershipAddresses = new MemoryStream();

                                    byte[] serverAddressBytes = clientEndpoint.Address.GetAddressBytes();
                                    byte[] sourceAddressBytes = sourceAddress.GetAddressBytes();
                                    byte[] localAddressBytes = localAddress.GetAddressBytes();

                                    membershipAddresses.Write(serverAddressBytes, 0, serverAddressBytes.Length);
                                    membershipAddresses.Write(sourceAddressBytes, 0, sourceAddressBytes.Length);
                                    membershipAddresses.Write(localAddressBytes, 0, localAddressBytes.Length);

                                    udpClient.MulticastMembershipAddresses = membershipAddresses.ToArray();

                                    // Execute multicast subscribe for specific source
                                    SocketOptionLevel level = clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP;
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.AddSourceMembership, udpClient.MulticastMembershipAddresses);
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.MulticastTimeToLive, int.Parse(m_configData["multicastTimeToLive"]));
                                }
                                else
                                {
                                    // Execute multicast subscribe for any source
                                    SocketOptionLevel level = clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP;
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.AddMembership, new MulticastOption(clientEndpoint.Address));
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.MulticastTimeToLive, int.Parse(m_configData["multicastTimeToLive"]));
                                }
                            }

                            clientInfo = new UdpClientInfo()
                            {
                                Client = udpClient,
                                SendArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction(),
                                SendLock = new SpinLock(),
                                SendQueue = new ConcurrentQueue<UdpServerPayload>()
                            };

                            clientInfo.SendArgs.RemoteEndPoint = udpClient.Provider;
                            clientInfo.SendArgs.SetBuffer(udpClient.SendBuffer, 0, udpClient.SendBufferSize);
                            clientInfo.SendArgs.Completed += m_sendHandler;

                            m_clientInfoLookup.TryAdd(udpClient.ID, clientInfo);
                            OnClientConnected(udpClient.ID);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = string.Format("Unable to connect to client {0}: {1}", clientString, ex.Message);
                        OnClientConnectingException(new Exception(errorMessage, ex));
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Server is currently running");
            }
        }

        /// <summary>
        /// Disconnects the specified connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public override void DisconnectOne(Guid clientID)
        {
            UdpClientInfo clientInfo;
            TransportProvider<EndPoint> client = null;

            try
            {
                if (!m_clientInfoLookup.TryRemove(clientID, out clientInfo))
                    return;

                client = clientInfo.Client;

                if ((object)client.Provider != null)
                {
                    // If the IP specified for the client is a multicast IP, unsubscribe from the specified multicast group.
                    IPEndPoint clientEndpoint = (IPEndPoint)client.Provider;

                    if (Transport.IsMulticastIP(clientEndpoint.Address))
                    {
                        if ((object)client.MulticastMembershipAddresses != null)
                        {
                            // Execute multicast unsubscribe for specific source
                            m_udpServer.Provider.SetSocketOption(clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.DropSourceMembership, client.MulticastMembershipAddresses);
                        }
                        else
                        {
                            // Execute multicast unsubscribe for any source
                            m_udpServer.Provider.SetSocketOption(clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(clientEndpoint.Address));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if ((object)client != null)
                    OnSendClientDataException(client.ID, new InvalidOperationException(string.Format("Failed to drop multicast membership: {0}", ex.Message), ex));
            }

            if ((object)client != null)
                client.Reset();

            OnClientDisconnected(clientID);
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{EndPoint}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="udpClient">The UDP client.</param>
        /// <returns>An <see cref="TransportProvider{EndPoint}"/> object.</returns>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public bool TryGetClient(Guid clientID, out TransportProvider<EndPoint> udpClient)
        {
            UdpClientInfo clientInfo;
            bool clientExists;

            clientExists = m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

            if (clientExists)
                udpClient = clientInfo.Client;
            else
                udpClient = null;

            return clientExists;
        }

        /// <summary>
        /// Validates the specified <paramref name="configurationString"/>.
        /// </summary>
        /// <param name="configurationString">Configuration string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConfigurationString(string configurationString)
        {
            string setting;
            int value;

            m_configData = configurationString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_configData);

            if (!m_configData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port is missing (Example: {0})", DefaultConfigurationString));

            if (!Transport.IsPortNumberValid(m_configData["port"]) && int.Parse(m_configData["port"]) != -1)
                throw new ArgumentOutOfRangeException("configurationString", string.Format("Port number must be {0} or between {1} and {2}", -1, Transport.PortRangeLow, Transport.PortRangeHigh));

            if (!m_configData.ContainsKey("multicastTimeToLive"))
                m_configData.Add("multicastTimeToLive", "10");

            // Make sure a valid multi-cast time-to-live value is defined in the configuration data
            if (!(m_configData.TryGetValue("multicastTimeToLive", out setting) && int.TryParse(setting, out value)))
                m_configData["multicastTimeToLive"] = "10";
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataToAsync(Guid clientID, byte[] data, int offset, int length)
        {
            UdpClientInfo clientInfo;
            ConcurrentQueue<UdpServerPayload> sendQueue;
            UdpServerPayload payload;
            ManualResetEventSlim handle;

            UdpServerPayload dequeuedPayload;
            bool lockTaken = false;

            if (!m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
                throw new InvalidOperationException(string.Format("No client found for ID {0}.", clientID));

            sendQueue = clientInfo.SendQueue;

            // Check to see if the client has reached the maximum send queue size.
            if (m_maxSendQueueSize > 0 && sendQueue.Count >= m_maxSendQueueSize)
            {
                for (int i = 0; i < m_maxSendQueueSize; i++)
                {
                    if (sendQueue.TryDequeue(out payload))
                    {
                        payload.WaitHandle.Set();
                        payload.WaitHandle.Dispose();
                        payload.WaitHandle = null;
                    }
                }

                throw new InvalidOperationException(string.Format("Client {0} connected to UDP server reached maximum send queue size. {1} payloads dumped from the queue.", clientID, m_maxSendQueueSize));
            }

            // Create payload and wait handle.
            payload = ReusableObjectPool<UdpServerPayload>.Default.TakeObject();
            handle = ReusableObjectPool<ManualResetEventSlim>.Default.TakeObject();

            payload.Data = data;
            payload.Offset = offset;
            payload.Length = length;
            payload.WaitHandle = handle;
            payload.ClientInfo = clientInfo;
            handle.Reset();

            // Queue payload for sending.
            sendQueue.Enqueue(payload);

            try
            {
                clientInfo.SendLock.Enter(ref lockTaken);

                // Send next queued payload.
                if (Interlocked.CompareExchange(ref clientInfo.Sending, 1, 0) == 0)
                {
                    if (sendQueue.TryDequeue(out dequeuedPayload))
                        ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), dequeuedPayload);
                    else
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
            finally
            {
                if (lockTaken)
                    clientInfo.SendLock.Exit();
            }

            // Notify that the send operation has started.
            OnSendClientDataStart(clientID);

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle.WaitHandle;
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.SendClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ServerBase.SendClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="ServerBase.SendClientDataException"/> event.</param>
        protected override void OnSendClientDataException(Guid clientID, Exception ex)
        {
            if (m_clientInfoLookup.ContainsKey(clientID) && CurrentState == ServerState.Running)
                base.OnSendClientDataException(clientID, ex);
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.ReceiveClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ServerBase.ReceiveClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="ServerBase.ReceiveClientDataException"/> event.</param>
        protected override void OnReceiveClientDataException(Guid clientID, Exception ex)
        {
            if (m_clientInfoLookup.ContainsKey(clientID) && CurrentState == ServerState.Running)
                base.OnReceiveClientDataException(clientID, ex);
        }

        /// <summary>
        /// Loops waiting for payloads and sends them on the socket.
        /// </summary>
        private void SendPayload(UdpServerPayload payload)
        {
            UdpClientInfo clientInfo = null;
            TransportProvider<EndPoint> client = null;
            SocketAsyncEventArgs args;
            ManualResetEventSlim handle;
            int copyLength;

            try
            {
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                args = clientInfo.SendArgs;
                handle = payload.WaitHandle;
                args.UserToken = payload;

                // Copy payload into send buffer.
                copyLength = Math.Min(payload.Length, client.SendBufferSize);
                Buffer.BlockCopy(payload.Data, payload.Offset, client.SendBuffer, 0, copyLength);

                // Set buffer and user token of send args.
                args.SetBuffer(0, copyLength);

                // Update offset and length.
                payload.Offset += copyLength;
                payload.Length -= copyLength;

                // Send data over socket.
                if (!m_udpServer.Provider.SendToAsync(args))
                    ProcessSend(args);
            }
            catch (Exception ex)
            {
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);

                if ((object)clientInfo != null)
                {
                    // Assume process send was not able
                    // to continue the asynchronous loop.
                    Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void ProcessSend(SocketAsyncEventArgs args)
        {
            UdpServerPayload payload = null;
            UdpClientInfo clientInfo = null;
            TransportProvider<EndPoint> client = null;
            ConcurrentQueue<UdpServerPayload> sendQueue = null;
            ManualResetEventSlim handle = null;
            bool lockTaken = false;

            try
            {
                payload = (UdpServerPayload)args.UserToken;
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                sendQueue = clientInfo.SendQueue;
                handle = payload.WaitHandle;

                // Determine whether we are finished with this
                // payload and, if so, set the wait handle.
                if (payload.Length <= 0)
                    handle.Set();

                // Check for errors during send operation.
                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                // Update statistics on the client.
                client.Statistics.UpdateBytesSent(args.BytesTransferred);

                // Send operation is complete.
                if (payload.Length <= 0)
                    OnSendClientDataComplete(client.ID);
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);
            }
            finally
            {
                try
                {
                    if (payload.Length > 0)
                    {
                        // Still more to send for this payload.
                        ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), payload);
                    }
                    else
                    {
                        payload.WaitHandle = null;
                        payload.ClientInfo = null;

                        // Return payload and wait handle to their respective object pools.
                        ReusableObjectPool<UdpServerPayload>.Default.ReturnObject(payload);
                        ReusableObjectPool<ManualResetEventSlim>.Default.ReturnObject(handle);

                        // Begin sending next client payload.
                        if (sendQueue.TryDequeue(out payload))
                        {
                            ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), payload);
                        }
                        else
                        {
                            try
                            {
                                clientInfo.SendLock.Enter(ref lockTaken);

                                if (sendQueue.TryDequeue(out payload))
                                    ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), payload);
                                else
                                    Interlocked.Exchange(ref clientInfo.Sending, 0);
                            }
                            finally
                            {
                                if (lockTaken)
                                    clientInfo.SendLock.Exit();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Exception encountered while attempting to send next payload: {0}", ex.Message);

                    if ((object)client != null)
                        OnSendClientDataException(client.ID, new Exception(errorMessage, ex));

                    if ((object)clientInfo != null)
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data from any endpoint.
        /// </summary>
        private void ReceivePayloadAsync(SocketAsyncEventArgs args)
        {
            // Attempt to receive data on the socket.
            byte[] buffer = m_udpServer.ReceiveBuffer;
            int length = m_udpServer.ReceiveBufferSize;

            args.SetBuffer(buffer, 0, length);
            args.RemoteEndPoint = m_udpServer.Provider.LocalEndPoint;

            if (!m_udpServer.Provider.ReceiveFromAsync(args))
                ThreadPool.QueueUserWorkItem(state => ProcessReceive((SocketAsyncEventArgs)state), args);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data from any endpoint.
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            Guid clientID = default(Guid);

            try
            {
                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                // Update statistics and pointers.
                m_udpServer.Statistics.UpdateBytesReceived(args.BytesTransferred);
                m_udpServer.BytesReceived = args.BytesTransferred;

                // Search connected clients for a client connected to the end-point from where this data is received.
                foreach (TransportProvider<EndPoint> client in m_clientInfoLookup.Values.Select(clientInfo => clientInfo.Client))
                {
                    if (client.Provider.Equals(args.RemoteEndPoint))
                    {
                        // Found a match, notify of data.
                        clientID = client.ID;
                        client.Statistics.UpdateBytesReceived(args.BytesTransferred);
                        OnReceiveClientDataComplete(client.ID, m_udpServer.ReceiveBuffer, m_udpServer.BytesReceived);
                        break;
                    }
                }

                // Resume receive operation on the server socket.
                ReceivePayloadAsync(args);
            }
            catch (Exception ex)
            {
                OnReceiveClientDataException(clientID, ex);
            }
        }

        #endregion
    }
}