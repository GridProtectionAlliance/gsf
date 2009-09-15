//*******************************************************************************************************
//  UdpClient.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access.
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/08/2009 - J. Ritchie Carroll
//       Added WaitHandle return value from asynchronous connection.
//  07/09/2009 - Pinal C. Patel
//       Modified to attempt resuming reception on SocketException for non-Handshake enabled connection.
//  07/15/2009 - Pinal C. Patel
//       Modified Disconnect() to add error checking.
//  07/17/2009 - Pinal C. Patel
//       Added support to specify a specific interface address on a multiple interface machine.
//  07/20/2009 - Pinal C. Patel
//       Allowed for UDP endpoint to not be bound to a local interface by specifying -1 for port number.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TVA.Communication
{
    /// <summary>
    /// Represents a UDP-based communication server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use <see cref="UdpClient"/> when the primary purpose is to receive data.
    /// </para>
    /// <para>
    /// The <see cref="UdpClient.Client"/> socket can be bound to a specified interface on a machine with multiple interfaces by 
    /// specifying the interface in the <see cref="ClientBase.ConnectionString"/> (Example: "Server=localhost:8888; Port=8989; Interface=127.0.0.1")
    /// </para>
    /// <para>
    /// The <see cref="UdpClient.Client"/> socket can be used just for transmitting data without being bound to a local interface 
    /// by specifying -1 for the local port number in the <see cref="ClientBase.ConnectionString"/> (Example: "Server=localhost:8888; Port=-1")
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="UdpClient"/> component:
    /// <code>
    /// using System;
    /// using TVA;
    /// using TVA.Communication;
    /// using TVA.Security.Cryptography;
    /// using TVA.IO.Compression;
    /// 
    /// class Program
    /// {
    ///     static UdpClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         m_client = new UdpClient("Server=localhost:8888; Port=8989");
    ///         m_client.Handshake = false;
    ///         m_client.ReceiveTimeout = -1;
    ///         m_client.Encryption = CipherStrength.None;
    ///         m_client.Compression = CompressionStrength.NoCompression;
    ///         m_client.SecureSession = false;
    ///         m_client.Initialize();
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.ReceiveDataComplete += m_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         m_client.Connect();
    /// 
    ///         // Transmit user input to the server.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_client.Send(input);
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to server.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to server.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from server.");
    ///     }
    /// 
    ///     static void m_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}.", m_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class UdpClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ReceiveBufferSize"/> property.
        /// </summary>
        public new const int DefaultReceiveBufferSize = 32768;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Server=localhost:8888; Port=8989";

        /// <summary>
        /// Specifies the constant to be used for disabling <see cref="SocketError.ConnectionReset"/> when endpoint is not listening.
        /// </summary>
        private const int SIO_UDP_CONNRESET = -1744830452;

        // Fields
        //private bool m_destinationReachableCheck;
        private EndPoint m_udpServer;
        private TransportProvider<Socket> m_udpClient;
        private Dictionary<string, string> m_connectData;
        private Func<TransportProvider<Socket>, bool> m_receivedGoodbye;
#if ThreadTracking
        private ManagedThread m_connectionThread;
#else
        private Thread m_connectionThread;
#endif

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        public UdpClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="UdpClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public UdpClient(string connectString)
            : base(TransportProtocol.Udp, connectString)
        {
            base.ReceiveBufferSize = DefaultReceiveBufferSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="UdpClient"/>.</param>
        public UdpClient(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="TransportProvider{Socket}"/> object for the <see cref="UdpClient"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<Socket> Client
        {
            get
            {
                return m_udpClient;
            }
        }

        /// <summary>
        /// Gets the server URI of the <see cref="UdpClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri
        {
            get
            {
                return string.Format("{0}://{1}", TransportProtocol, m_connectData["server"]).ToLower();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Disconnects the <see cref="UdpClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                if (Handshake && m_udpClient.Provider != null)
                {
                    // Handshake is enabled so we'll notify the client.
                    m_udpClient.SendBuffer = new GoodbyeMessage(m_udpClient.ID).BinaryImage;
                    m_udpClient.SendBufferOffset = 0;
                    m_udpClient.SendBufferLength = m_udpClient.SendBuffer.Length;
                    Payload.ProcessTransmit(ref m_udpClient.SendBuffer, ref m_udpClient.SendBufferOffset, ref m_udpClient.SendBufferLength, Encryption, m_udpClient.Secretkey, Compression);

                    m_udpClient.Provider.SendTo(m_udpClient.SendBuffer, m_udpServer);
                }

                m_udpClient.Reset();

                if (m_connectionThread != null)
                    m_connectionThread.Abort();
            }
        }

        /// <summary>
        /// Connects the <see cref="UdpClient"/> to the server asynchronously.
        /// </summary>
        /// <exception cref="FormatException">Server property in <see cref="ClientBase.ConnectionString"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="UdpClient"/> when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            WaitHandle handle = base.ConnectAsync();

            m_udpClient = new TransportProvider<Socket>();
            m_udpClient.ID = this.ClientID;
            m_udpClient.Secretkey = SharedSecret;
            m_udpClient.ReceiveBuffer = new byte[ReceiveBufferSize];
            
            // Create a server endpoint.
            if (m_connectData.ContainsKey("server"))
            {
                // Client has a server endpoint specified.
                string[] parts = m_connectData["server"].Split(':');
                if (parts.Length == 2)
                    m_udpServer = Transport.CreateEndPoint(parts[0], int.Parse(parts[1]));
                else
                    throw new FormatException(string.Format("Server property in ConnectionString is invalid. Example: {0}.", DefaultConnectionString));
            }
            else
            {
                if (Handshake)
                    throw new InvalidOperationException("Handshake requires Server property in the ConnectionString.");

                // Create a random server endpoint since one is not specified.
                m_udpServer = Transport.CreateEndPoint(string.Empty, 0);
            }

#if ThreadTracking
            m_connectionThread = new ManagedThread(OpenPort);
            m_connectionThread.Name = "TVA.Communication.UdpClient.OpenPort()";
#else
            m_connectionThread = new Thread(OpenPort);
#endif
            m_connectionThread.Start();
                
            return handle;
        }

        /// <summary>
        /// Connects to the <see cref="UdpClient"/>.
        /// </summary>
        private void OpenPort()
        {
            int connectionAttempts = 0;

            if (Handshake)
            {
                // Handshaking must be performed. 
                m_receivedGoodbye = DoGoodbyeCheck;
                HandshakeMessage handshake = new HandshakeMessage();
                handshake.ID = this.ClientID;
                handshake.Secretkey = this.SharedSecret;

                // Prepare binary image of handshake to be transmitted.
                m_udpClient.Provider = Transport.CreateSocket(m_connectData["interface"], 0, ProtocolType.Udp);
                m_udpClient.SendBuffer = handshake.BinaryImage;
                m_udpClient.SendBufferOffset = 0;
                m_udpClient.SendBufferLength = m_udpClient.SendBuffer.Length;
                Payload.ProcessTransmit(ref m_udpClient.SendBuffer, ref m_udpClient.SendBufferOffset, ref m_udpClient.SendBufferLength, Encryption, SharedSecret, Compression);

                while (true)
                {
                    try
                    {
                        connectionAttempts++;
                        OnConnectionAttempt();

                        // Transmit the prepared and processed handshake message.
                        m_udpClient.Provider.SendTo(m_udpClient.SendBuffer, m_udpServer);

                        // Wait for the server's reponse to the handshake message.
                        Thread.Sleep(1000);
                        ReceiveHandshakeAsync(m_udpClient);
                        break;
                    }
                    catch (SocketException ex)
                    {
                        OnConnectionException(ex);
                        if (ex.SocketErrorCode == SocketError.ConnectionReset &&
                            (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts))
                        {
                            // Server is unavailable, so keep retrying connection to the server.                                  
                            continue;
                        }
                        else
                        {
                            // For any other reason, clean-up as if the client was disconnected.
                            TerminateConnection(m_udpClient, false);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // This is highly unlikely, but we must handle this situation just-in-case.
                        OnConnectionException(ex);
                        TerminateConnection(m_udpClient, false);
                        break;
                    }
                }
            }
            else
            {
                while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
                {
                    try
                    {
                        OnConnectionAttempt();

                        // Disable SocketError.ConnectionReset exception from being thrown when the enpoint is not listening.
                        m_udpClient.Provider = Transport.CreateSocket(m_connectData["interface"], int.Parse(m_connectData["port"]), ProtocolType.Udp);
                        m_udpClient.Provider.IOControl(SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

                        OnConnectionEstablished();
                        // Listen for incoming data only if endpoint is bound to a local interface.
                        if (m_udpClient.Provider.LocalEndPoint != null)
                        {
                            m_receivedGoodbye = NoGoodbyeCheck;
                            ReceivePayloadAsync(m_udpClient);
                        }
                        break;
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        connectionAttempts++;
                        OnConnectionException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            if (!m_connectData.ContainsKey("interface"))
                m_connectData.Add("interface", string.Empty);

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888; Port=8989
            // Old Format: Server=localhost; RemotePort=8888; LocalPort=8888
            if (m_connectData.ContainsKey("localport") && !m_connectData.ContainsKey("port"))
                m_connectData.Add("port", m_connectData["localport"]);

            if (m_connectData.ContainsKey("server") && m_connectData.ContainsKey("remoteport"))
                m_connectData["server"] = m_connectData["server"] + ":" + m_connectData["remoteport"];

            if (!m_connectData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port property is missing. Example: {0}.", DefaultConnectionString));

            if (!Transport.IsPortNumberValid(m_connectData["port"]) && int.Parse(m_connectData["port"]) != -1)
                throw new ArgumentOutOfRangeException("connectionString", string.Format("Port number must be {0} or between {1} and {2}.", -1, Transport.PortRangeLow, Transport.PortRangeHigh));
        }

        /// <summary>
        /// Gets the secret key to be used for ciphering client data.
        /// </summary>
        /// <returns>Cipher secret key.</returns>
        protected override string GetSessionSecret()
        {
            return m_udpClient.Secretkey;
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            WaitHandle handle;

            // Send payload to the client asynchronously.
            handle = m_udpClient.Provider.BeginSendTo(data, offset, length, SocketFlags.None, m_udpServer, SendPayloadAsyncCallback, m_udpClient).AsyncWaitHandle;

            // Notify that the send operation has started.
            m_udpClient.SendBuffer = data;
            m_udpClient.SendBufferOffset = offset;
            m_udpClient.SendBufferLength = length;
            OnSendDataStart();

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void SendPayloadAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            try
            {
                // Send operation is complete.
                udpClient.Statistics.UpdateBytesSent(udpClient.Provider.EndSendTo(asyncResult));
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of handshake data.
        /// </summary>
        private void ReceiveHandshakeAsync(TransportProvider<Socket> worker)
        {
            // Receive data asynchronously with a timeout.
            worker.WaitAsync(HandshakeTimeout,
                             ReceiveHandshakeAsyncCallback,
                             worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                              worker.ReceiveBufferOffset,
                                                              worker.ReceiveBuffer.Length,
                                                              SocketFlags.None,
                                                              ref m_udpServer,
                                                              ReceiveHandshakeAsyncCallback,
                                                              worker));
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of handshake data.
        /// </summary>
        private void ReceiveHandshakeAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Handshake response is not recevied in a timely fashion.
                TerminateConnection(udpClient, false);
                OnHandshakeProcessTimeout();
            }
            else
            {
                // Received handshake response from server so we'll process it.
                try
                {
                    // Update statistics and pointers.
                    udpClient.Statistics.UpdateBytesReceived(udpClient.Provider.EndReceiveFrom(asyncResult, ref m_udpServer));
                    udpClient.ReceiveBufferLength = udpClient.Statistics.LastBytesReceived;

                    // Process the received handshake response message.
                    Payload.ProcessReceived(ref udpClient.ReceiveBuffer, ref udpClient.ReceiveBufferOffset, ref udpClient.ReceiveBufferLength, Encryption, SharedSecret, Compression);

                    HandshakeMessage handshake = new HandshakeMessage();
                    if (handshake.Initialize(udpClient.ReceiveBuffer, udpClient.ReceiveBufferOffset, udpClient.ReceiveBufferLength) != -1)
                    {
                        // Received handshake response message could be parsed.
                        this.ServerID = handshake.ID;
                        udpClient.Secretkey = handshake.Secretkey;

                        // Client is now considered to be connected to the server.
                        OnConnectionEstablished();                        
                        ReceivePayloadAsync(udpClient);
                    }
                    else
                    {
                        // Received handshake response message could not be parsed.
                        TerminateConnection(udpClient, false);
                        OnHandshakeProcessUnsuccessful();
                    }
                }
                catch
                {
                    // This is most likely because the server forcibly disconnected the client.
                    TerminateConnection(udpClient, false);
                    OnHandshakeProcessUnsuccessful();
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsync(TransportProvider<Socket> worker)
        {
            if (ReceiveTimeout == -1)
            {
                // Wait for data indefinitely.
                worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                 worker.ReceiveBufferOffset,
                                                 worker.ReceiveBuffer.Length,
                                                 SocketFlags.None,
                                                 ref m_udpServer,
                                                 ReceivePayloadAsyncCallback,
                                                 worker);
            }
            else
            {
                // Wait for data with a timeout.
                worker.WaitAsync(ReceiveTimeout,
                                 ReceivePayloadAsyncCallback,
                                 worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                                  worker.ReceiveBufferOffset,
                                                                  worker.ReceiveBuffer.Length,
                                                                  SocketFlags.None,
                                                                  ref m_udpServer,
                                                                  ReceivePayloadAsyncCallback,
                                                                  worker));
            }
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Timed-out on reception of data so notify via event and continue waiting for data.
                OnReceiveDataTimeout();
                udpClient.WaitAsync(ReceiveTimeout, ReceivePayloadAsyncCallback, asyncResult);
            }
            else
            {
                try
                {
                    // Update statistics and pointers.
                    udpClient.Statistics.UpdateBytesReceived(udpClient.Provider.EndReceiveFrom(asyncResult, ref m_udpServer));
                    udpClient.ReceiveBufferLength = udpClient.Statistics.LastBytesReceived;

                    // Received a goodbye message from the server.
                    if (m_receivedGoodbye(udpClient))
                        throw new SocketException((int)SocketError.ConnectionReset);

                    // Notify of received data and resume receive operation.
                    OnReceiveDataComplete(udpClient.ReceiveBuffer, udpClient.ReceiveBufferLength);
                    ReceivePayloadAsync(udpClient);
                }
                catch (ObjectDisposedException ex)
                {
                    // Terminate connection when client is disposed.
                    OnReceiveDataException(ex);
                    TerminateConnection(udpClient, true);
                }
                catch (SocketException ex)
                {
                    if (Handshake && ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        // Terminate the connection if:
                        // 1) Handshake is enabled and the endpoint is not listening for data.
                        // 2) Handshake is enabled and the endpoint has notified of shutdown via a "goodbye" message.
                        OnReceiveDataException(ex);
                        TerminateConnection(udpClient, true);
                    }
                    else
                    {
                        try
                        {
                            // For any other exception, notify and resume receive.
                            OnReceiveDataException(ex);
                            ReceivePayloadAsync(udpClient);
                        }
                        catch
                        {
                            // Terminate connection if resuming receiving fails.
                            TerminateConnection(udpClient, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        // For any other exception, notify and resume receive.
                        OnReceiveDataException(ex);
                        ReceivePayloadAsync(udpClient);
                    }
                    catch
                    {
                        // Terminate connection if resuming receiving fails.
                        TerminateConnection(udpClient, true);
                    }
                }
            }
        }

        /// <summary>
        /// Delegate that gets called to verify client disconnect when <see cref="ClientBase.Handshake"/> is turned off.
        /// </summary>
        private bool NoGoodbyeCheck(TransportProvider<Socket> client)
        {
            return false;
        }

        /// <summary>
        /// Delegate that gets called to verify client disconnect when <see cref="ClientBase.Handshake"/> is turned on.
        /// </summary>
        private bool DoGoodbyeCheck(TransportProvider<Socket> client)
        {
            // Process data received in the buffer.
            int offset = client.ReceiveBufferOffset;
            int length = client.ReceiveBufferLength;
            byte[] buffer = client.ReceiveBuffer.BlockCopy(0, length);
            Payload.ProcessReceived(ref buffer, ref offset, ref length, Encryption, client.Secretkey, Compression);

            // Check if data is for goodbye message.
            return (new GoodbyeMessage().Initialize(buffer, offset, length) != -1);
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(TransportProvider<Socket> client, bool raiseEvent)
        {
            client.Reset();
            if (raiseEvent)
                OnConnectionTerminated();
        }

        #endregion
    }
}