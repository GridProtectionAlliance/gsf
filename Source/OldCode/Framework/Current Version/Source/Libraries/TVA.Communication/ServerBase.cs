//*******************************************************************************************************
//  ServerBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/01/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed server data access.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  02/19/2008 - Pinal C. Patel
//       Added code to detect and avoid redundant calls to Dispose().
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  06/18/2009 - Pinal C. Patel
//       Fixed the implementation of Enabled property.
//  07/02/2009 - Pinal C. Patel
//       Modified state alterning properties to restart the server when changed.
//  07/17/2009 - Pinal C. Patel
//       Modified SharedSecret to be persisted as an encrypted value.
//  08/05/2009 - Josh L. Patterson
//      Edited Comments.
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
using System.Drawing;
using System.Text;
using System.Threading;
using TVA.Configuration;
using TVA.IO.Compression;
using TVA.Security.Cryptography;
using TVA.Units;

namespace TVA.Communication
{
    /// <summary>
    /// Base class for a server involved in server-client communication.
    /// </summary>
    [ToolboxBitmap(typeof(ServerBase))]
    public abstract partial class ServerBase : Component, IServer, ISupportInitialize, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="MaxClientConnections"/> property.
        /// </summary>
        public const int DefaultMaxClientConnections = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="Handshake"/> property.
        /// </summary>
        public const bool DefaultHandshake = false;

        /// <summary>
        /// Specifies the default value for the <see cref="HandshakeTimeout"/> property.
        /// </summary>
        public const int DefaultHandshakeTimeout = 3000;

        /// <summary>
        /// Specifies the default value for the <see cref="SharedSecret"/> property.
        /// </summary>
        public const string DefaultSharedSecret = "6572a33d-826f-4d96-8c28-8be66bbc700e";

        /// <summary>
        /// Specifies the default value for the <see cref="Encryption"/> property.
        /// </summary>
        public const CipherStrength DefaultEncryption = CipherStrength.None;

        /// <summary>
        /// Specifies the default value for the <see cref="SecureSession"/> property.
        /// </summary>
        public const bool DefaultSecureSession = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveTimeout"/> property.
        /// </summary>
        public const int DefaultReceiveTimeout = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveBufferSize"/> property.
        /// </summary>
        public const int DefaultReceiveBufferSize = 8192;

        /// <summary>
        /// Specifies the default value for the <see cref="Compression"/> property.
        /// </summary>
        public const CompressionStrength DefaultCompression = CompressionStrength.NoCompression;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "CommunicationServer";

        // Events

        /// <summary>
        /// Occurs when the server is started.
        /// </summary>
        [Category("Server"),
        Description("Occurs when the server is started.")]
        public event EventHandler ServerStarted;

        /// <summary>
        /// Occurs when the server is stopped.
        /// </summary>
        [Category("Server"),
        Description("Occurs when the server is stopped.")]
        public event EventHandler ServerStopped;

        /// <summary>
        /// Occurs when server-client handshake, when enabled, cannot be performed within the specified <see cref="HandshakeTimeout"/> time.
        /// </summary>
        [Category("Server"),
        Description("Occurs when server-client handshake, when enabled, cannot be performed within the specified HandshakeTimeout time.")]
        public event EventHandler HandshakeProcessTimeout;

        /// <summary>
        /// Occurs when server-client handshake, when enabled, cannot be performed successfully due to information mismatch.
        /// </summary>
        [Category("Server"),
        Description("Occurs when server-client handshake, when enabled, cannot be performed successfully due to information mismatch.")]
        public event EventHandler HandshakeProcessUnsuccessful;

        /// <summary>
        /// Occurs when a client connects to the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client that connected to the server.
        /// </remarks>
        [Category("Client"),
        Description("Occurs when a client connects to the server.")]
        public event EventHandler<EventArgs<Guid>> ClientConnected;

        /// <summary>
        /// Occurs when a client disconnects from the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client that disconnected from the server.
        /// </remarks>
        [Category("Client"),
        Description("Occurs when a client disconnects from the server.")]
        public event EventHandler<EventArgs<Guid>> ClientDisconnected;

        /// <summary>
        /// Occurs when data is being sent to a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client to which the data is being sent.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when data is being sent to a client.")]
        public event EventHandler<EventArgs<Guid>> SendClientDataStart;

        /// <summary>
        /// Occurs when data has been sent to a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client to which the data has been sent.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when data has been sent to a client.")]
        public event EventHandler<EventArgs<Guid>> SendClientDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when sending data to a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the client to which the data was being sent.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when sending data to a client.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when an Exception is encountered when sending data to a client.")]
        public event EventHandler<EventArgs<Guid, Exception>> SendClientDataException;

        /// <summary>
        /// Occurs when no data is received from a client for the <see cref="ReceiveTimeout"/> time.
        /// </summary>
        [Category("Data"),
        Description("Occurs when no data is received from a client for the ReceiveTimeout time.")]
        public event EventHandler<EventArgs<Guid>> ReceiveClientDataTimeout;

        /// <summary>
        /// Occurs when data is received from a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2,T3}.Argument1"/> is the ID of the client from which data is received.<br/>
        /// <see cref="EventArgs{T1,T2,T3}.Argument2"/> is the buffer containing data received from the client starting at index zero.<br/>
        /// <see cref="EventArgs{T1,T2,T3}.Argument3"/> is the number of bytes received in the buffer from the client.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when data is received from a client.")]
        public event EventHandler<EventArgs<Guid, byte[], int>> ReceiveClientDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when receiving data from a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the client from which the data was being received.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when receiving data from a client.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when an Exception is encountered when receiving data from a client.")]
        public event EventHandler<EventArgs<Guid, Exception>> ReceiveClientDataException;

        // Fields
        private string m_configurationString;
        private int m_maxClientConnections;
        private bool m_handshake;
        private int m_handshakeTimeout;
        private string m_sharedSecret;
        private CipherStrength m_encryption;
        private bool m_secureSession;
        private int m_receiveTimeout;
        private int m_receiveBufferSize;
        private CompressionStrength m_compression;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private Encoding m_textEncoding;
        private Action<Guid, byte[], int, int> m_receiveClientDataHandler;
        private ServerState m_currentState;
        private TransportProtocol m_transportProtocol;
        private Guid m_serverID;
        private List<Guid> m_clientIDs;
        private Ticks m_stopTime;
        private Ticks m_startTime;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the server.
        /// </summary>
        protected ServerBase()
            : base()
        {
            m_serverID = Guid.NewGuid();
            m_clientIDs = new List<Guid>();
            m_textEncoding = Encoding.ASCII;
            m_currentState = ServerState.NotRunning;
            m_maxClientConnections = DefaultMaxClientConnections;
            m_handshake = DefaultHandshake;
            m_handshakeTimeout = DefaultHandshakeTimeout;
            m_sharedSecret = DefaultSharedSecret;
            m_encryption = DefaultEncryption;
            m_secureSession = DefaultSecureSession;
            m_receiveTimeout = DefaultReceiveTimeout;
            m_receiveBufferSize = DefaultReceiveBufferSize;
            m_compression = DefaultCompression;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
        }

        /// <summary>
        /// Initializes a new instance of the server.
        /// </summary>
        /// <param name="transportProtocol">One of the <see cref="TransportProtocol"/> values.</param>
        /// <param name="configurationString">The data used by the server for initialization.</param>
        protected ServerBase(TransportProtocol transportProtocol, string configurationString)
            : this()
        {
            m_transportProtocol = transportProtocol;
            this.ConfigurationString = configurationString;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the data required by the server to initialize.
        /// </summary>
        [Category("Settings"),
        Description("The data that is required by the server to initialize.")]
        public virtual string ConfigurationString
        {
            get
            {
                return m_configurationString;
            }
            set
            {
                ValidateConfigurationString(value);

                m_configurationString = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of clients that can connect to the server.
        /// </summary>
        /// <remarks>
        /// Set <see cref="MaxClientConnections"/> to -1 to allow infinite client connections.
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultMaxClientConnections),
        Description("The maximum number of clients that can connect to the server. Set MaxClientConnections to -1 to allow infinite client connections.")]
        public virtual int MaxClientConnections
        {
            get
            {
                return m_maxClientConnections;
            }
            set
            {
                if (value < 1)
                    m_maxClientConnections = -1;
                else
                    m_maxClientConnections = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the server will do a handshake with the clients after the connection has been established.
        /// </summary>
        /// <remarks>
        /// <see cref="Handshake"/> is required when <see cref="SecureSession"/> is enabled.
        /// </remarks>
        /// <exception cref="InvalidOperationException"><see cref="Handshake"/> is being disabled while <see cref="SecureSession"/> is enabled.</exception>
        [Category("Security"),
        DefaultValue(DefaultHandshake),
        Description("Indicates whether the server will do a handshake with the clients after the connection has been established.")]
        public virtual bool Handshake
        {
            get
            {
                return m_handshake;
            }
            set
            {
                // Can't disable handshake when secure session is enabled.
                if (!value && m_secureSession)
                    throw new InvalidOperationException("Handshake is required when SecureSession is enabled");

                m_handshake = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds that the server will wait for the clients to initiate the <see cref="Handshake"/> process.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
        [Category("Security"),
        DefaultValue(DefaultHandshakeTimeout),
        Description("The number of milliseconds that the server will wait for the clients to initiate the handshake process.")]
        public virtual int HandshakeTimeout
        {
            get
            {
                return m_handshakeTimeout;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative");

                m_handshakeTimeout = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets the key to be used for ciphering the data exchanged between the server and clients.
        /// </summary>
        [Category("Security"),
        DefaultValue(DefaultSharedSecret),
        Description("The key to be used for ciphering the data exchanged between the server and clients.")]
        public virtual string SharedSecret
        {
            get
            {
                return m_sharedSecret;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_sharedSecret = value;
                else
                    m_sharedSecret = DefaultSharedSecret;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CipherStrength"/> to be used for ciphering the data exchanged between the server and clients.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Encryption"/> is being disabled while <see cref="SecureSession"/> is enabled.</exception>
        /// <remarks>
        /// <list type="table">
        ///     <listheader>
        ///         <term><see cref="SecureSession"/></term>
        ///         <description>Key used for <see cref="Encryption"/></description>
        ///     </listheader>
        ///     <item>
        ///         <term>Disabled</term>
        ///         <description><see cref="SharedSecret"/> is used.</description>
        ///     </item>
        ///     <item>
        ///         <term>Enabled</term>
        ///         <description>A private key exchanged between the server and client during the <see cref="Handshake"/> process is used.</description>
        ///     </item>
        /// </list>
        /// </remarks>
        [Category("Security"),
        DefaultValue(DefaultEncryption),
        Description("The CipherStrength to be used for ciphering the data exchanged between the server and clients.")]
        public virtual CipherStrength Encryption
        {
            get
            {
                return m_encryption;
            }
            set
            {
                // Can't disable encryption when secure session is enabled.
                if (value == CipherStrength.None && m_secureSession)
                    throw new InvalidOperationException("Encryption is required when SecureSession is enabled");

                m_encryption = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data exchanged between the server and clients will be encrypted using a private session passphrase.
        /// </summary>
        ///<remarks>
        ///<see cref="Handshake"/> and <see cref="Encryption"/> must be enabled in order to use <see cref="SecureSession"/>.
        ///</remarks>
        ///<exception cref="InvalidOperationException"><see cref="SecureSession"/> is being enabled before enabling <see cref="Handshake"/>.</exception>
        ///<exception cref="InvalidOperationException"><see cref="SecureSession"/> is being enabled before enabling <see cref="Encryption"/>.</exception>
        [Category("Security"),
        DefaultValue(DefaultSecureSession),
        Description("Indicates whether the data exchanged between the server and clients will be encrypted using a private session passphrase.")]
        public virtual bool SecureSession
        {
            get
            {
                return m_secureSession;
            }
            set
            {
                // Handshake is required for SecureSession.
                if (value && !m_handshake)
                    throw new InvalidOperationException("Handshake must be enabled in order to use SecureSession");

                // Encryption is required for SecureSession.
                if (value && m_encryption == CipherStrength.None)
                    throw new InvalidOperationException("Encryption must be enabled in order to use SecureSession");

                m_secureSession = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds after which the server will raise the <see cref="ReceiveClientDataTimeout"/> event if no data is received from a client.
        /// </summary>
        /// <remarks>Set <see cref="ReceiveTimeout"/> to -1 to disable this feature.</remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveTimeout),
        Description("The number of milliseconds after which the server will raise the ReceiveClientDataTimeout event if no data is received from a client. Set ReceiveTimeout to -1 to disable this feature.")]
        public virtual int ReceiveTimeout
        {
            get
            {
                return m_receiveTimeout;
            }
            set
            {
                if (value < 1)
                    m_receiveTimeout = -1;
                else
                    m_receiveTimeout = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer used by the server for receiving data from the clients.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
        [Category("Data"),
        DefaultValue(DefaultReceiveBufferSize),
        Description("The size of the buffer used by the server for receiving data from the clients.")]
        public virtual int ReceiveBufferSize
        {
            get
            {
                return m_receiveBufferSize;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative");

                m_receiveBufferSize = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CompressionStrength"/> to be used for compressing the data exchanged between the server and clients.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultCompression),
        Description("The CompressionStrength to be used for compressing the data exchanged between the server and clients.")]
        public virtual CompressionStrength Compression
        {
            get
            {
                return m_compression;
            }
            set
            {
                m_compression = value;
                ReStart();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the server settings are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the server settings are to be saved to the config file.")]
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
        /// Gets or sets the category under which the server settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the server settings are to be saved to the config file if the PersistSettings property is set to true.")]
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
        /// Gets or sets a boolean value that indicates whether the server is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will start the server if it is not running, setting
        /// to false will stop the server if it is running.
        /// </remarks>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_currentState == ServerState.Running;
            }
            set
            {
                if (value && !Enabled)
                    Start();
                else if (!value && Enabled)
                    Stop();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the connected clients.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get
            {
                return m_textEncoding;
            }
            set
            {
                m_textEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="Delegate"/> to be invoked instead of the <see cref="ReceiveClientDataComplete"/> event when data is received from clients.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property only needs to be implemented if you need data from the clients absolutelty as fast as possible, for most uses this will not be necessary.  
        /// Setting this property gives the consumer access to the data stream as soon as it's available, but this also bypasses <see cref="Encryption"/> and 
        /// <see cref="Compression"/> on received data.
        /// </para>
        /// <para>
        /// arg1 in <see cref="ReceiveClientDataHandler"/> is the ID or the client from which data is received.<br/>
        /// arg2 in <see cref="ReceiveClientDataHandler"/> is the buffer containing data received from the client starting at index zero.<br/>
        /// arg3 in <see cref="ReceiveClientDataHandler"/> is the zero based starting offset into the buffer containing the data received from the server.<br/>
        /// arg4 in <see cref="ReceiveClientDataHandler"/> is the number of bytes received from the client that is stored in the buffer (arg2).
        /// </para>
        /// </remarks>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Action<Guid, byte[], int, int> ReceiveClientDataHandler
        {
            get
            {
                return m_receiveClientDataHandler;
            }
            set
            {
                m_receiveClientDataHandler = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="ServerState"/>.
        /// </summary>
        [Browsable(false)]
        public ServerState CurrentState
        {
            get
            {
                return m_currentState;
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProtocol"/> used by the server for the transportation of data with the clients.
        /// </summary>
        [Browsable(false)]
        public virtual TransportProtocol TransportProtocol
        {
            get
            {
                return m_transportProtocol;
            }
        }

        /// <summary>
        /// Gets the server's ID.
        /// </summary>
        [Browsable(false)]
        public virtual Guid ServerID
        {
            get
            {
                return m_serverID;
            }
        }

        /// <summary>
        /// Gets the IDs of clients connected to the server.
        /// </summary>
        [Browsable(false)]
        public virtual Guid[] ClientIDs
        {
            get
            {
                lock (m_clientIDs)
                {
                    return m_clientIDs.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Time"/> for which the server has been running.
        /// </summary>
        [Browsable(false)]
        public virtual Time RunTime
        {
            get
            {
                Time serverRunTime = 0.0D;

                if (m_startTime > 0)
                {
                    if (m_currentState == ServerState.Running)
                        // Server is running.
                        serverRunTime = (DateTime.Now.Ticks - m_startTime).ToSeconds();
                    else
                        // Server is not running.
                        serverRunTime = (m_stopTime - m_startTime).ToSeconds();
                }
                return serverRunTime;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the server.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return m_settingsCategory;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the server.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                if (m_handshake)
                {
                    // Display ID only if handshaking is enabled.
                    status.Append("                 Server ID: ");
                    status.Append(m_serverID.ToString());
                    status.AppendLine();
                }
                status.Append("              Server state: ");
                status.Append(m_currentState);
                status.AppendLine();
                status.Append("            Server runtime: ");
                status.Append(RunTime.ToString());
                status.AppendLine();
                status.Append("      Configuration string: ");
                status.Append(m_configurationString);
                status.AppendLine();
                status.Append("         Connected clients: ");
                lock (m_clientIDs)
                {
                    status.Append(m_clientIDs.Count);
                }
                status.AppendLine();
                status.Append("           Maximum clients: ");
                status.Append(m_maxClientConnections == -1 ? "Infinite" : m_maxClientConnections.ToString());
                status.AppendLine();
                status.Append("            Receive buffer: ");
                status.Append(m_receiveBufferSize.ToString());
                status.AppendLine();
                status.Append("        Transport protocol: ");
                status.Append(m_transportProtocol.ToString());
                status.AppendLine();
                status.Append("        Text encoding used: ");
                status.Append(m_textEncoding.EncodingName);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, starts the server.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// When overridden in a derived class, stops the server.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// When overridden in a derived class, disconnects a connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        public abstract void DisconnectOne(Guid clientID);

        /// <summary>
        /// When overridden in a derived class, validates the specified <paramref name="configurationString"/>.
        /// </summary>
        /// <param name="configurationString">The configuration string to be validated.</param>
        protected abstract void ValidateConfigurationString(string configurationString);

        /// <summary>
        /// When overridden in a derived class, returns the secret key used for ciphering client data.
        /// </summary>
        /// <param name="clientID">ID of the client whose secret key is to be retrieved.</param>
        /// <returns>Secret key of the specified client.</returns>
        protected abstract string GetSessionSecret(Guid clientID);

        /// <summary>
        /// When overridden in a derived class, sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected abstract WaitHandle SendDataToAsync(Guid clientID, byte[] data, int offset, int length);

        #endregion

        /// <summary>
        /// Initializes the server.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the server is not consumed through the designer surface of the IDE.
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
        /// Performs necessary operations before the server properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use by the designer if the server is consumed through 
        /// the designer surface of the IDE.
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
        /// Performs necessary operations after the server properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use by the designer if the server is consumed through the 
        /// designer surface of the IDE.
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
        /// Saves server settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void SaveSettings()
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
                element = settings["ConfigurationString", true];
                element.Update(m_configurationString, element.Description, element.Encrypted);
                element = settings["MaxClientConnections", true];
                element.Update(m_maxClientConnections, element.Description, element.Encrypted);
                element = settings["Handshake", true];
                element.Update(m_handshake, element.Description, element.Encrypted);
                element = settings["HandshakeTimeout", true];
                element.Update(m_handshakeTimeout, element.Description, element.Encrypted);
                element = settings["SharedSecret", true];
                element.Update(m_sharedSecret, element.Description, element.Encrypted);
                element = settings["Encryption", true];
                element.Update(m_encryption, element.Description, element.Encrypted);
                element = settings["SecureSession", true];
                element.Update(m_secureSession, element.Description, element.Encrypted);
                element = settings["ReceiveTimeout", true];
                element.Update(m_receiveTimeout, element.Description, element.Encrypted);
                element = settings["ReceiveBufferSize", true];
                element.Update(m_receiveBufferSize, element.Description, element.Encrypted);
                element = settings["Compression", true];
                element.Update(m_compression, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved server settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("ConfigurationString", m_configurationString, "Data required by the server to initialize.");
                settings.Add("MaxClientConnections", m_maxClientConnections, "Maximum number of clients that can connect to the server.");
                settings.Add("Handshake", m_handshake, "True if the server will do a handshake with the client after the connection has been established; otherwise False.");
                settings.Add("HandshakeTimeout", m_handshakeTimeout, "Number of milliseconds the server will wait for the clients to initiate the Handshake process.");
                settings.Add("SharedSecret", m_sharedSecret, "Key to be used for ciphering the data exchanged between the server and clients.", true);
                settings.Add("Encryption", m_encryption, "Cipher strength (None; Aes128; Aes256) to be used for ciphering the data exchanged between the server and clients.");
                settings.Add("SecureSession", m_secureSession, "True if the data exchanged between the server and clients will be encrypted using a private session passphrase; otherwise False.");
                settings.Add("ReceiveTimeout", m_receiveTimeout, "Number of milliseconds the server will wait for data to be received from the clients.");
                settings.Add("ReceiveBufferSize", m_receiveBufferSize, "Size of the buffer used by the server for receiving data from the clients.");
                settings.Add("Compression", m_compression, "Compression strength (NoCompression; DefaultCompression; BestSpeed; BestCompression; MultiPass) to be used for compressing the data exchanged between the server and clients.");
                ConfigurationString = settings["ConfigurationString"].ValueAs(m_configurationString);
                MaxClientConnections = settings["MaxClientConnections"].ValueAs(m_maxClientConnections);
                Handshake = settings["Handshake"].ValueAs(m_handshake);
                HandshakeTimeout = settings["HandshakeTimeout"].ValueAs(m_handshakeTimeout);
                SharedSecret = settings["SharedSecret"].ValueAs(m_sharedSecret);
                Encryption = settings["Encryption"].ValueAs(m_encryption);
                SecureSession = settings["SecureSession"].ValueAs(m_secureSession);
                ReceiveTimeout = settings["ReceiveTimeout"].ValueAs(m_receiveTimeout);
                ReceiveBufferSize = settings["ReceiveBufferSize"].ValueAs(m_receiveBufferSize);
                Compression = settings["Compression"].ValueAs(m_compression);
            }
        }

        /// <summary>
        /// Sends data to the specified client synchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The plain-text data that is to be sent.</param>
        public virtual void SendTo(Guid clientID, string data)
        {
            SendTo(clientID, m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to the specified client synchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        public virtual void SendTo(Guid clientID, object serializableObject)
        {
            SendTo(clientID, Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to the specified client synchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The binary data that is to be sent.</param>
        public virtual void SendTo(Guid clientID, byte[] data)
        {
            SendTo(clientID, data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to the specified client synchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        public virtual void SendTo(Guid clientID, byte[] data, int offset, int length)
        {
            SendToAsync(clientID, data, offset, length).WaitOne();
        }

        /// <summary>
        /// Sends data to all of the connected clients synchronously.
        /// </summary>
        /// <param name="data">The plain-text data that is to be sent.</param>
        public virtual void Multicast(string data)
        {
            Multicast(m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to all of the connected clients synchronously.
        /// </summary>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        public virtual void Multicast(object serializableObject)
        {
            Multicast(Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to all of the connected clients synchronously.
        /// </summary>
        /// <param name="data">The binary data that is to be sent.</param>
        public virtual void Multicast(byte[] data)
        {
            Multicast(data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to all of the connected clients synchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        public virtual void Multicast(byte[] data, int offset, int length)
        {
            // Perform asynchronous transmissions.
            WaitHandle[] handles = MulticastAsync(data, offset, length);

            // Wait for transmissions to complete.
            if (handles.Length > 0)
                WaitHandle.WaitAll(handles);
        }

        /// <summary>
        /// Disconnects all of the connected clients.
        /// </summary>
        public virtual void DisconnectAll()
        {
            foreach (Guid clientID in ClientIDs)
            {
                DisconnectOne(clientID);
            }
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The plain-text data that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendToAsync(Guid clientID, string data)
        {
            return SendToAsync(clientID, m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendToAsync(Guid clientID, object serializableObject)
        {
            return SendToAsync(clientID, Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The binary data that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendToAsync(Guid clientID, byte[] data)
        {
            return SendToAsync(clientID, data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendToAsync(Guid clientID, byte[] data, int offset, int length)
        {
            if (m_currentState == ServerState.Running)
            {
                // Pre-condition data as needed and then send it.
                Payload.ProcessTransmit(ref data, ref offset, ref length, m_encryption, GetSessionSecret(clientID), m_compression);
                return SendDataToAsync(clientID, data, offset, length);
            }
            else
            {
                throw new InvalidOperationException("Server is not running");
            }
        }

        /// <summary>
        /// Sends data to all of the connected clients asynchronously.
        /// </summary>
        /// <param name="data">The plain-text data that is to be sent.</param>
        /// <returns>Array of <see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle[] MulticastAsync(string data)
        {
            return MulticastAsync(m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to all of the connected clients asynchronously.
        /// </summary>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        /// <returns>Array of <see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle[] MulticastAsync(object serializableObject)
        {
            return MulticastAsync(Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to all of the connected clients asynchronously.
        /// </summary>
        /// <param name="data">The binary data that is to be sent.</param>
        /// <returns>Array of <see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle[] MulticastAsync(byte[] data)
        {
            return MulticastAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to all of the connected clients asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns>Array of <see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle[] MulticastAsync(byte[] data, int offset, int length)
        {
            List<WaitHandle> handles = new List<WaitHandle>();
            foreach (Guid clientID in ClientIDs)
            {
                handles.Add(SendToAsync(clientID, data, offset, length));
            }

            return handles.ToArray();
        }

        /// <summary>
        /// Raises the <see cref="ServerStarted"/> event.
        /// </summary>
        protected virtual void OnServerStarted()
        {
            m_currentState = ServerState.Running;
            m_stopTime = 0;
            m_startTime = DateTime.Now.Ticks;   // Save the time when server is started.

            if (ServerStarted != null)
                ServerStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServerStopped"/> event.
        /// </summary>
        protected virtual void OnServerStopped()
        {
            m_currentState = ServerState.NotRunning;
            m_stopTime = DateTime.Now.Ticks;    // Save the time when server is stopped.

            if (ServerStopped != null)
                ServerStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="HandshakeProcessTimeout"/> event.
        /// </summary>
        protected virtual void OnHandshakeProcessTimeout()
        {
            if (HandshakeProcessTimeout != null)
                HandshakeProcessTimeout(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="HandshakeProcessUnsuccessful"/> event.
        /// </summary>
        protected virtual void OnHandshakeProcessUnsuccessful()
        {
            if (HandshakeProcessUnsuccessful != null)
                HandshakeProcessUnsuccessful(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ClientConnected"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ClientConnected"/> event.</param>
        protected virtual void OnClientConnected(Guid clientID)
        {
            lock (m_clientIDs)
            {
                m_clientIDs.Add(clientID);
            }

            if (ClientConnected != null)
                ClientConnected(this, new EventArgs<Guid>(clientID));
        }

        /// <summary>
        /// Raises the <see cref="ClientDisconnected"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ClientDisconnected"/> event.</param>
        protected virtual void OnClientDisconnected(Guid clientID)
        {
            lock (m_clientIDs)
            {
                m_clientIDs.Remove(clientID);
            }

            if (ClientDisconnected != null)
                ClientDisconnected(this, new EventArgs<Guid>(clientID));
        }

        /// <summary>
        /// Raises the <see cref="SendClientDataStart"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="SendClientDataStart"/> event.</param>
        protected virtual void OnSendClientDataStart(Guid clientID)
        {
            if (SendClientDataStart != null)
                SendClientDataStart(this, new EventArgs<Guid>(clientID));
        }

        /// <summary>
        /// Raises the <see cref="SendClientDataComplete"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="SendClientDataComplete"/> event.</param>
        protected virtual void OnSendClientDataComplete(Guid clientID)
        {
            if (SendClientDataComplete != null)
                SendClientDataComplete(this, new EventArgs<Guid>(clientID));
        }

        /// <summary>
        /// Raises the <see cref="SendClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="SendClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="SendClientDataException"/> event.</param>
        protected virtual void OnSendClientDataException(Guid clientID, Exception ex)
        {
            if (SendClientDataException != null)
                SendClientDataException(this, new EventArgs<Guid, Exception>(clientID, ex));
        }

        /// <summary>
        /// Raises the <see cref="ReceiveClientDataTimeout"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ReceiveClientDataTimeout"/> event.</param>
        protected virtual void OnReceiveClientDataTimeout(Guid clientID)
        {
            if (ReceiveClientDataTimeout != null)
                ReceiveClientDataTimeout(this, new EventArgs<Guid>(clientID));
        }

        /// <summary>
        /// Raises the <see cref="ReceiveClientDataComplete"/> event.
        /// </summary>
        /// <param name="clientID">ID of the client from which data is received.</param>
        /// <param name="data">Data received from the client.</param>
        /// <param name="size">Number of bytes received from the client.</param>
        protected virtual void OnReceiveClientDataComplete(Guid clientID, byte[] data, int size)
        {
            if (m_receiveClientDataHandler != null)
            {
                m_receiveClientDataHandler(clientID, data, 0, size);
            }
            else
            {
                if (ReceiveClientDataComplete != null)
                {
                    try
                    {
                        int offset = 0; // Received buffer will always have valid data starting at offset zero.
                        Payload.ProcessReceived(ref data, ref offset, ref size, m_encryption, GetSessionSecret(clientID), m_compression);
                    }
                    catch
                    {
                        // Ignore encountered exception and pass-on the raw data.
                    }
                    finally
                    {
                        ReceiveClientDataComplete(this, new EventArgs<Guid, byte[], int>(clientID, data, size));
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceiveClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ReceiveClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="ReceiveClientDataException"/> event.</param>
        protected virtual void OnReceiveClientDataException(Guid clientID, Exception ex)
        {
            if (ReceiveClientDataException != null)
                ReceiveClientDataException(this, new EventArgs<Guid, Exception>(clientID, ex));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the server and optionally releases the managed resources.
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
                        Stop();
                        SaveSettings();
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
        /// Re-starts the server if currently running.
        /// </summary>
        private void ReStart()
        {
            if (m_currentState == ServerState.Running)
            {
                Stop();
                while (m_currentState != ServerState.NotRunning)
                {
                    Thread.Sleep(100);
                }
                Start();
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Create a communications server
        /// </summary>
        /// <remarks>
        /// Note that typical configuration string should be prefixed with a "protocol=tcp" or a "protocol=udp"
        /// </remarks>
        /// <param name="configurationString">The configuration string for the server.</param>
        /// <returns>A communications server.</returns>
        public static IServer Create(string configurationString)
        {
            Dictionary<string, string> configurationData = configurationString.ParseKeyValuePairs();
            IServer server = null;
            string protocol;

            if (configurationData.TryGetValue("protocol", out protocol))
            {
                configurationData.Remove("protocol");
                StringBuilder settings = new StringBuilder();

                foreach (string key in configurationData.Keys)
                {
                    settings.Append(key);
                    settings.Append("=");
                    settings.Append(configurationData[key]);
                    settings.Append(";");
                }

                switch (protocol.ToLower())
                {
                    case "tcp":
                        server = new TcpServer(settings.ToString());
                        break;
                    case "udp":
                        server = new UdpServer(settings.ToString());
                        break;
                    default:
                        throw new ArgumentException("Transport protocol \'" + protocol + "\' is not valid");
                }
            }
            else
            {
                throw new ArgumentException("Transport protocol must be specified");
            }

            return server;
        }

        #endregion
    }
}