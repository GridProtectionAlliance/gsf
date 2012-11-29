//*******************************************************************************************************
//  UdpClient.cs - Gbtc
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
//  10/30/2009 - Pinal C. Patel
//       Added true multicast support by allowing for socket level subscription to a multicast group.
//  11/17/2009 - Pinal C. Patel
//       Fixed a bug in the creation of random server endpoint when server endpoint information is 
//       omitted from the ConnectionString.
//  03/24/2010 - Pinal C. Patel
//       Updated the interpretation of server property in ConnectionString to correctly interpret 
//       IPv6 IP addresses according to IETF - A Recommendation for IPv6 Address Text Representation.
//  11/29/2010 - Pinal C. Patel
//       Corrected the implementation of ConnectAsync() method.
//  02/13/2011 - Pinal C. Patel
//       Modified ConnectAsync() to handle loopback address resolution failure on IPv6 enabled OSes.
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
    /// Use <see cref="UdpClient"/> when the primary purpose is to receive data.
    /// </para>
    /// <para>
    /// The <see cref="UdpClient.Client"/> socket can be bound to a specified interface on a machine with multiple interfaces by 
    /// specifying the interface in the <see cref="ClientBase.ConnectionString"/> (Example: "Server=localhost:8888; Port=8989; Interface=127.0.0.1")
    /// </para>
    /// <para>
    /// The <see cref="UdpClient.Client"/> socket can be used just for transmitting data without being bound to a local interface 
    /// by specifying -1 for the port number in the <see cref="ClientBase.ConnectionString"/> (Example: "Server=localhost:8888; Port=-1")
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="UdpClient"/> component:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Communication;
    /// using GSF.Security.Cryptography;
    /// using GSF.IO.Compression;
    /// 
    /// class Program
    /// {
    ///     static UdpClient s_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         s_client = new UdpClient("Server=localhost:8888; Port=8989");
    ///         s_client.Handshake = false;
    ///         s_client.ReceiveTimeout = -1;
    ///         s_client.Encryption = CipherStrength.None;
    ///         s_client.Compression = CompressionStrength.NoCompression;
    ///         s_client.SecureSession = false;
    ///         s_client.Initialize();
    ///         // Register event handlers.
    ///         s_client.ConnectionAttempt += s_client_ConnectionAttempt;
    ///         s_client.ConnectionEstablished += s_client_ConnectionEstablished;
    ///         s_client.ConnectionTerminated += s_client_ConnectionTerminated;
    ///         s_client.ReceiveDataComplete += s_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         s_client.Connect();
    /// 
    ///         // Transmit user input to the server.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             s_client.Send(input);
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         s_client.Dispose();
    ///     }
    /// 
    ///     static void s_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to server.");
    ///     }
    /// 
    ///     static void s_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to server.");
    ///     }
    /// 
    ///     static void s_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from server.");
    ///     }
    /// 
    ///     static void s_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}.", s_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class UdpClient : ClientBase
    {
        #region [ Members ]

        // Nested Types

        private class UdpClientPayload
        {
            public EndPoint Destination;
            public byte[] Data;
            public int Offset;
            public int Length;

            public ManualResetEventSlim WaitHandle;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ReceiveBufferSize"/> property.
        /// </summary>
        public new const int DefaultReceiveBufferSize = 32768;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowDualStackSocket"/> property.
        /// </summary>
        public const bool DefaultAllowDualStackSocket = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxSendQueueSize"/> property.
        /// </summary>
        public const int DefaultMaxSendQueueSize = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Server=localhost:8888; Port=8989";

        /// <summary>
        /// Specifies the constant to be used for disabling <see cref="SocketError.ConnectionReset"/> when endpoint is not listening.
        /// </summary>
        private const int SIO_UDP_CONNRESET = -1744830452;

        // Events

        /// <summary>
        /// Occurs when unprocessed data has been received from the server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event can be used to receive a notification that server data has arrived. The <see cref="Read"/> method can then be used
        /// to copy data to an existing buffer. In many cases it will be optimal to use an existing buffer instead of subscribing to the
        /// <see cref="ReceiveDataFromComplete"/> event.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the end-point that data has been received from.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of bytes received in the buffer from the server.
        /// </para>
        /// </remarks>
        [Category("Data"),
        Description("Occurs when unprocessed data has been received from the server.")]
        public event EventHandler<EventArgs<EndPoint, int>> ReceiveDataFrom;

        /// <summary>
        /// Occurs when data received from the server has been processed and is ready for consumption.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2,T3}.Argument1"/> is the end-point that data has been received from.<br/>
        /// <see cref="EventArgs{T1,T2,T3}.Argument2"/> is a new buffer containing post-processed data received from the server starting at index zero.<br/>
        /// <see cref="EventArgs{T1,T2,T3}.Argument3"/> is the number of post-processed bytes received in the buffer from the server.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when data received from the server has been processed and is ready for consumption.")]
        public event EventHandler<EventArgs<EndPoint, byte[], int>> ReceiveDataFromComplete;

        // Fields
        //private bool m_destinationReachableCheck;
        private IPEndPoint m_udpServer;
        private TransportProvider<Socket> m_udpClient;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectionHandle;
#if ThreadTracking
        private ManagedThread m_connectionThread;
#else
        private Thread m_connectionThread;
#endif

        private int m_sending;
        private SpinLock m_sendLock;
        private ConcurrentQueue<UdpClientPayload> m_sendQueue;
        private SocketAsyncEventArgs m_sendArgs;
        private SocketAsyncEventArgs m_receiveArgs;
        private EventHandler<SocketAsyncEventArgs> m_sendHandler;
        private EventHandler<SocketAsyncEventArgs> m_receiveHandler;

        private bool m_disposed;

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
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;

            m_sendLock = new SpinLock();
            m_sendQueue = new ConcurrentQueue<UdpClientPayload>();
            m_sendHandler += (sender, args) => ProcessSend();
            m_receiveHandler += (sender, args) => ProcessReceive();
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

        /// <summary>
        /// Gets or sets the size of the buffer used by the client for receiving data from the server.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
        public override int ReceiveBufferSize
        {
            get
            {
                return base.ReceiveBufferSize;
            }
            set
            {
                base.ReceiveBufferSize = value;

                if ((object)m_udpClient != null)
                    m_udpClient.SetReceiveBuffer(value);
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

                if ((object)m_sendQueue != null)
                {
                    statusBuilder.AppendFormat("           Queued payloads: {0}", m_sendQueue.Count);
                    statusBuilder.AppendLine();
                }

                statusBuilder.AppendFormat("     Wait handle pool size: {0}", ReusableObjectPool<ManualResetEventSlim>.Default.GetPoolSize());
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("         Payload pool size: {0}", ReusableObjectPool<UdpClientPayload>.Default.GetPoolSize());
                statusBuilder.AppendLine();

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ClientBase.ReceiveData"/> event handler. Calling this method outside
        /// this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">No received data buffer has been defined to read.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if ((object)m_udpClient.ReceiveBuffer != null)
            {
                int sourceLength = m_udpClient.BytesReceived - ReadIndex;
                int readBytes = length > sourceLength ? sourceLength : length;
                Buffer.BlockCopy(m_udpClient.ReceiveBuffer, ReadIndex, buffer, startIndex, readBytes);

                // Update read index for next call
                ReadIndex += readBytes;

                if (ReadIndex >= m_udpClient.BytesReceived)
                    ReadIndex = 0;

                return readBytes;
            }

            throw new InvalidOperationException("No received data buffer has been defined to read.");
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
            int maxSendQueueSize;

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

            //    // Overwrite config file if max send queue size exists in connection string.
            //    if (m_connectData.ContainsKey("maxSendQueueSize") && int.TryParse(m_connectData["maxSendQueueSize"], out maxSendQueueSize))
            //        m_maxSendQueueSize = maxSendQueueSize;
            //}
        }

        /// <summary>
        /// Disconnects the <see cref="UdpClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                try
                {
                    if ((object)m_udpServer != null && (object)m_udpClient.Provider != null)
                    {
                        // If the IP specified for the server is a multicast IP, unsubscribe from the specified multicast group.
                        IPEndPoint serverEndpoint = (IPEndPoint)m_udpServer;

                        if (Transport.IsMulticastIP(serverEndpoint.Address))
                        {
                            if ((object)m_udpClient.MulticastMembershipAddresses != null)
                            {
                                // Execute multicast unsubscribe for specific source
                                m_udpClient.Provider.SetSocketOption(serverEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.DropSourceMembership, m_udpClient.MulticastMembershipAddresses);
                            }
                            else
                            {
                                // Execute multicast unsubscribe for any source
                                m_udpClient.Provider.SetSocketOption(serverEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(serverEndpoint.Address));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnSendDataException(new InvalidOperationException(string.Format("Failed to drop multicast membership: {0}", ex.Message), ex));
                }

                m_udpClient.Reset();

                if ((object)m_connectionThread != null)
                    m_connectionThread.Abort();

                base.Disconnect();
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
            m_connectionHandle = (ManualResetEvent)base.ConnectAsync();

            m_udpClient = new TransportProvider<Socket>();
            m_udpClient.SetReceiveBuffer(ReceiveBufferSize);

            // Create a server endpoint.
            if (m_connectData.ContainsKey("server"))
            {
                // Client has a server endpoint specified.
                Match endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);

                if (endpoint != Match.Empty)
                    m_udpServer = Transport.CreateEndPoint(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value), m_ipStack);
                else
                    throw new FormatException(string.Format("Server property in ConnectionString is invalid (Example: {0})", DefaultConnectionString));
            }
            else
            {
                // Create a random server endpoint since one is not specified.
                m_udpServer = Transport.CreateEndPoint(m_connectData["interface"], 0, m_ipStack);
            }

#if ThreadTracking
            m_connectionThread = new ManagedThread(OpenPort);
            m_connectionThread.Name = "TVA.Communication.UdpClient.OpenPort()";
#else
            m_connectionThread = new Thread(OpenPort);
#endif
            m_connectionThread.Start();

            return m_connectionHandle;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="UdpClient"/> and optionally releases the managed resources.
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
                        if ((object)m_connectionHandle != null)
                        {
                            m_connectionHandle.Close();
                            m_connectionHandle = null;
                        }

                        if ((object)m_sendArgs != null)
                        {
                            m_sendArgs.Dispose();
                            m_sendArgs = null;
                        }

                        if ((object)m_receiveArgs != null)
                        {
                            m_receiveArgs.Dispose();
                            m_receiveArgs = null;
                        }
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
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_connectData);

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888; Port=8989
            // Old Format: Server=localhost; RemotePort=8888; LocalPort=8888
            if (m_connectData.ContainsKey("localport") && !m_connectData.ContainsKey("port"))
                m_connectData.Add("port", m_connectData["localport"]);

            if (m_connectData.ContainsKey("server") && m_connectData.ContainsKey("remoteport"))
                m_connectData["server"] = string.Format("{0}:{1}", m_connectData["server"], m_connectData["remoteport"]);

            // Check if 'port' property is missing.
            if (!m_connectData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port property is missing (Example: {0})", DefaultConnectionString));

            // Check if 'port' property is valid.
            if (!Transport.IsPortNumberValid(m_connectData["port"]) && int.Parse(m_connectData["port"]) != -1)
                throw new ArgumentOutOfRangeException("connectionString", string.Format("Port number must be {0} or between {1} and {2}", -1, Transport.PortRangeLow, Transport.PortRangeHigh));
        }

        /// <summary>
        /// Connects to the <see cref="UdpClient"/>.
        /// </summary>
        private void OpenPort()
        {
            int connectionAttempts = 0;

            while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
            {
                try
                {
                    OnConnectionAttempt();

                    // Disable SocketError.ConnectionReset exception from being thrown when the enpoint is not listening.
                    m_udpClient.Provider = Transport.CreateSocket(m_connectData["interface"], int.Parse(m_connectData["port"]), ProtocolType.Udp, m_ipStack, m_allowDualStackSocket);
                    m_udpClient.Provider.IOControl(SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

                    // If the IP specified for the server is a multicast IP, subscribe to the specified multicast group.
                    IPEndPoint serverEndpoint = (IPEndPoint)m_udpServer;

                    if (Transport.IsMulticastIP(serverEndpoint.Address))
                    {
                        string multicastSource;

                        if (m_connectData.TryGetValue("multicastSource", out multicastSource))
                        {
                            IPAddress sourceAddress = IPAddress.Parse(multicastSource);
                            IPAddress localAddress = ((IPEndPoint)m_udpClient.Provider.LocalEndPoint).Address;

                            if (sourceAddress.AddressFamily != serverEndpoint.AddressFamily)
                                throw new InvalidOperationException(string.Format("Source address \"{0}\" is not in the same IP format as server address \"{1}\"", sourceAddress, serverEndpoint.Address));

                            if (localAddress.AddressFamily != serverEndpoint.AddressFamily)
                                throw new InvalidOperationException(string.Format("Local address \"{0}\" is not in the same IP format as server address \"{1}\"", localAddress, serverEndpoint.Address));

                            MemoryStream membershipAddresses = new MemoryStream();

                            byte[] serverAddressBytes = serverEndpoint.Address.GetAddressBytes();
                            byte[] sourceAddressBytes = sourceAddress.GetAddressBytes();
                            byte[] localAddressBytes = localAddress.GetAddressBytes();

                            membershipAddresses.Write(serverAddressBytes, 0, serverAddressBytes.Length);
                            membershipAddresses.Write(sourceAddressBytes, 0, sourceAddressBytes.Length);
                            membershipAddresses.Write(localAddressBytes, 0, localAddressBytes.Length);

                            m_udpClient.MulticastMembershipAddresses = membershipAddresses.ToArray();

                            // Execute multicast subscribe for specific source
                            m_udpClient.Provider.SetSocketOption(serverEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.AddSourceMembership, m_udpClient.MulticastMembershipAddresses);
                        }
                        else
                        {
                            // Execute multicast subscribe for any source
                            m_udpClient.Provider.SetSocketOption(serverEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(serverEndpoint.Address));
                        }
                    }

                    // Listen for data to send.
                    using (SocketAsyncEventArgs sendArgs = m_sendArgs)
                    {
                        m_sendArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                    }

                    m_udpClient.SetSendBuffer(SendBufferSize);
                    m_sendArgs.SetBuffer(m_udpClient.SendBuffer, 0, m_udpClient.SendBufferSize);
                    m_sendArgs.Completed += m_sendHandler;

                    m_connectionHandle.Set();
                    OnConnectionEstablished();

                    // Listen for incoming data only if endpoint is bound to a local interface.
                    if (m_udpClient.Provider.LocalEndPoint != null)
                    {
                        using (SocketAsyncEventArgs receiveArgs = m_receiveArgs)
                        {
                            m_receiveArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                        }

                        m_receiveArgs.Completed += m_receiveHandler;
                        ReceivePayloadAsync();
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

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            return SendDataToAsync(data, offset, length, m_udpServer);
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="destination">The end point which identifies the destination for the data.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public WaitHandle SendDataToAsync(byte[] data, int offset, int length, EndPoint destination)
        {
            UdpClientPayload payload;
            UdpClientPayload dequeuedPayload;
            ManualResetEventSlim handle;
            bool lockTaken = false;

            // Check to see if the client has reached the maximum send queue size.
            if (m_maxSendQueueSize > 0 && m_sendQueue.Count >= m_maxSendQueueSize)
            {
                for (int i = 0; i < m_maxSendQueueSize; i++)
                {
                    if (m_sendQueue.TryDequeue(out payload))
                    {
                        payload.WaitHandle.Set();
                        payload.WaitHandle.Dispose();
                        payload.WaitHandle = null;
                    }
                }

                throw new InvalidOperationException(string.Format("UDP client reached maximum send queue size. {0} payloads dumped from the queue.", m_maxSendQueueSize));
            }

            // Create payload and wait handle.
            payload = ReusableObjectPool<UdpClientPayload>.Default.TakeObject();
            handle = ReusableObjectPool<ManualResetEventSlim>.Default.TakeObject();

            payload.Destination = destination;
            payload.Data = data;
            payload.Offset = offset;
            payload.Length = length;
            payload.WaitHandle = handle;
            handle.Reset();

            // Queue payload for sending.
            m_sendQueue.Enqueue(payload);

            try
            {
                m_sendLock.Enter(ref lockTaken);

                // Send the next queued payload.
                if (Interlocked.CompareExchange(ref m_sending, 1, 0) == 0)
                {
                    if (m_sendQueue.TryDequeue(out dequeuedPayload))
                        ThreadPool.QueueUserWorkItem(state => SendPayload((UdpClientPayload)state), dequeuedPayload);
                    else
                        Interlocked.Exchange(ref m_sending, 0);
                }
            }
            finally
            {
                if (lockTaken)
                    m_sendLock.Exit();
            }

            // Notify that the send operation has started.
            OnSendDataStart();

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle.WaitHandle;
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.SendDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.SendDataException"/> event.</param>
        protected override void OnSendDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnSendDataException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected override void OnReceiveDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnReceiveDataException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ConnectionTerminated"/> event.
        /// </summary>
        protected override void OnConnectionTerminated()
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnConnectionTerminated();
        }

        /// <summary>
        /// Sends a payload on the socket.
        /// </summary>
        private void SendPayload(UdpClientPayload payload)
        {
            int copyLength;

            try
            {
                // Set the user token of the socket args.
                m_sendArgs.UserToken = payload;

                // Copy payload into send buffer.
                copyLength = Math.Min(payload.Length, m_udpClient.SendBufferSize);
                Buffer.BlockCopy(payload.Data, payload.Offset, m_udpClient.SendBuffer, 0, copyLength);

                // Set buffer and end point of send args.
                m_sendArgs.SetBuffer(0, copyLength);
                m_sendArgs.RemoteEndPoint = payload.Destination;

                // Update payload offset and length.
                payload.Offset += copyLength;
                payload.Length -= copyLength;

                // Send data over socket.
                if (!m_udpClient.Provider.SendToAsync(m_sendArgs))
                    ProcessSend();
            }
            catch (Exception ex)
            {
                OnSendDataException(ex);

                // Assume process send was not able
                // to continue the asynchronous loop.
                Interlocked.Exchange(ref m_sending, 0);
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void ProcessSend()
        {
            UdpClientPayload payload = null;
            ManualResetEventSlim handle = null;
            bool lockTaken = false;

            try
            {
                // Get the payload and its wait handle.
                payload = (UdpClientPayload)m_sendArgs.UserToken;
                handle = payload.WaitHandle;

                // Determine whether we are finished with this
                // payload and, if so, set the wait handle.
                if (payload.Length <= 0)
                    handle.Set();

                // Check for errors during send operation.
                if (m_sendArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)m_sendArgs.SocketError);

                // Update statistics.
                m_udpClient.Statistics.UpdateBytesSent(m_sendArgs.BytesTransferred);

                // Send operation is complete.
                if (payload.Length <= 0)
                    OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
            finally
            {
                try
                {
                    if (payload.Length > 0)
                    {
                        // Still more to send for this payload.
                        ThreadPool.QueueUserWorkItem(state => SendPayload((UdpClientPayload)state), payload);
                    }
                    else
                    {
                        payload.WaitHandle = null;

                        // Return payload and wait handle to their respective object pools.
                        ReusableObjectPool<UdpClientPayload>.Default.ReturnObject(payload);
                        ReusableObjectPool<ManualResetEventSlim>.Default.ReturnObject(handle);

                        // Begin sending next client payload.
                        if (m_sendQueue.TryDequeue(out payload))
                        {
                            ThreadPool.QueueUserWorkItem(state => SendPayload((UdpClientPayload)state), payload);
                        }
                        else
                        {
                            try
                            {
                                m_sendLock.Enter(ref lockTaken);

                                if (m_sendQueue.TryDequeue(out payload))
                                    ThreadPool.QueueUserWorkItem(state => SendPayload((UdpClientPayload)state), payload);
                                else
                                    Interlocked.Exchange(ref m_sending, 0);
                            }
                            finally
                            {
                                if (lockTaken)
                                    m_sendLock.Exit();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Exception encountered while attempting to send next payload: {0}", ex.Message);
                    OnSendDataException(new Exception(errorMessage, ex));
                    Interlocked.Exchange(ref m_sending, 0);
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsync()
        {
            // Set up event args for receive operation.
            m_receiveArgs.SetBuffer(m_udpClient.ReceiveBuffer, 0, m_udpClient.ReceiveBufferSize);
            m_receiveArgs.RemoteEndPoint = m_udpServer;

            if (!m_udpClient.Provider.ReceiveFromAsync(m_receiveArgs))
                ThreadPool.QueueUserWorkItem(state => ProcessReceive());
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data.
        /// </summary>
        private void ProcessReceive()
        {
            try
            {
                if (m_receiveArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)m_receiveArgs.SocketError);

                // Update statistics and pointers.
                m_udpClient.Statistics.UpdateBytesReceived(m_receiveArgs.BytesTransferred);
                m_udpClient.BytesReceived = m_udpClient.Statistics.LastBytesReceived;

                // Notify of received data and resume receive operation.
                OnReceive(m_receiveArgs.RemoteEndPoint, m_udpClient.ReceiveBuffer, m_udpClient.BytesReceived);
                ReceivePayloadAsync();
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed.
                TerminateConnection(true);
            }
            catch (SocketException ex)
            {
                try
                {
                    // Notify and resume receive.
                    OnReceiveDataException(ex);
                    ReceivePayloadAsync();
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(true);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveDataException(ex);
                    ReceivePayloadAsync();
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(true);
                }
            }
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(bool raiseEvent)
        {
            m_udpClient.Reset();

            if (raiseEvent)
                OnConnectionTerminated();
        }

        /// <summary>
        /// Calls all the receive handlers in sequence.
        /// </summary>
        private void OnReceive(EndPoint remoteEndPoint, byte[] data, int size)
        {
            OnReceiveDataFrom(remoteEndPoint, size);
            OnReceiveDataFromComplete(remoteEndPoint, data, size);
            OnReceiveDataComplete(data, size);
        }

        /// <summary>
        /// Raises the <see cref="ReceiveDataFrom"/> event.
        /// </summary>
        /// <param name="remoteEndPoint">End-point from which data has been received.</param>
        /// <param name="size">Number of bytes received from the client.</param>
        private void OnReceiveDataFrom(EndPoint remoteEndPoint, int size)
        {
            try
            {
                if ((object)ReceiveDataFrom != null)
                    ReceiveDataFrom(this, new EventArgs<EndPoint, int>(remoteEndPoint, size));
            }
            catch (Exception userException)
            {
                OnUnhandledUserException(userException);
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceiveDataFrom"/> event.
        /// </summary>
        /// <param name="remoteEndPoint">End-point from which data has been received.</param>
        /// <param name="data">Data received from the client.</param>
        /// <param name="size">Number of bytes received from the client.</param>
        private void OnReceiveDataFromComplete(EndPoint remoteEndPoint, byte[] data, int size)
        {
            try
            {
                if ((object)ReceiveDataFromComplete != null)
                    ReceiveDataFromComplete(this, new EventArgs<EndPoint, byte[], int>(remoteEndPoint, data, size));
            }
            catch (Exception userException)
            {
                OnUnhandledUserException(userException);
            }
        }

        #endregion
    }
}