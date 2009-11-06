//*******************************************************************************************************
//  DataListener.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/12/2007 - Pinal C. Patel
//       Generated original version of source code.
//  11/26/2007 - Pinal C. Patel
//       Added overloaded Data property to retrieve current data for a point.
//  01/15/2008 - Pinal C. Patel
//       Removed logic to timeout waiting for TCP/UDP connection to complete. As a result the 
//       ListenerStartFailure event no longer exists.
//  04/02/2008 - Pinal C. Patel
//       Added SocketConnecting event to notify that socket connection is being attempted.
//  04/23/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/15/2009 - Pinal C. Patel
//       Made caching of data locally optional so DataListener can be used just for getting real-time
//       time-series data that is now being made available via the new DataExtracted event.
//  09/17/2009 - Pinal C. Patel
//       Added check to prevent raising DataExtracted and DataChanged events if no time-series data
//       was present in the received packets.
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
using TVA.Communication;
using TVA.Configuration;
using TVA.Historian.Files;
using TVA.Historian.Packets;
using TVA.Units;

namespace TVA.Historian
{
    /// <summary>
    /// Represents a listener that can receive time-series data in real-time using <see cref="System.Net.Sockets.Socket"/>s.
    /// </summary>
    /// <seealso cref="IDataPoint"/>
    /// <seealso cref="PacketParser"/>
    [ToolboxBitmap(typeof(DataListener))]
    public class DataListener : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ID"/> property.
        /// </summary>
        public const string DefaultID = "Default";

        /// <summary>
        /// Specifies the default value for the <see cref="Server"/> property.
        /// </summary>
        public const string DefaultServer = "localhost";

        /// <summary>
        /// Specifies the default value for the <see cref="Port"/> property.
        /// </summary>
        public const int DefaultPort = 1004;

        /// <summary>
        /// Specifies the default value for the <see cref="Protocol"/> property.
        /// </summary>
        public const TransportProtocol DefaultProtocol = TransportProtocol.Udp;

        /// <summary>
        /// Specifies the default value for the <see cref="ConnectToServer"/> property.
        /// </summary>
        public const bool DefaultConnectToServer = true;

        /// <summary>
        /// Specifies the default value for the <see cref="CacheData"/> property.
        /// </summary>
        public const bool DefaultCacheData = true;

        /// <summary>
        /// Specifies the default value for the <see cref="InitializeData"/> property.
        /// </summary>
        public const bool DefaultInitializeData = true;

        /// <summary>
        /// Specifies the default value for the <see cref="InitializeDataTimeout"/> property.
        /// </summary>
        public const int DefaultInitializeDataTimeout = 60000;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "DataListener";

        // Events

        /// <summary>
        /// Occurs when the <see cref="DataListener"/> is starting up.
        /// </summary>
        [Category("Listener"),
        Description("Occurs when the DataListener is starting up.")]
        public event EventHandler ListenerStarting;

        /// <summary>
        /// Occurs when the <see cref="DataListener"/> has started.
        /// </summary>
        [Category("Listener"),
        Description("Occurs when the DataListener has started.")]
        public event EventHandler ListenerStarted;

        /// <summary>
        /// Occurs when the <see cref="DataListener"/> is being stopped.
        /// </summary>
        [Category("Listener"),
        Description("Occurs when the DataListener is being stopped.")]
        public event EventHandler ListenerStopping;

        /// <summary>
        /// Occurs when <see cref="DataListener"/> has stopped.
        /// </summary>
        [Category("Listener"),
        Description("Occurs when DataListener has stopped.")]
        public event EventHandler ListenerStopped;

        /// <summary>
        /// Occurs when the underlying <see cref="System.Net.Sockets.Socket"/> connection for receiving time-series data is being attempted.
        /// </summary>
        [Category("Socket"),
        Description("Occurs when the underlying Socket connection for receiving time-series data is being attempted.")]
        public event EventHandler SocketConnecting;

        /// <summary>
        /// Occurs when the underlying <see cref="System.Net.Sockets.Socket"/> connection for receiving time-series data is established.
        /// </summary>
        [Category("Socket"),
        Description("Occurs when the underlying Socket connection for receiving time-series data is established.")]
        public event EventHandler SocketConnected;

        /// <summary>
        /// Occurs when the underlying <see cref="System.Net.Sockets.Socket"/> connection for receiving time-series data is terminated.
        /// </summary>
        [Category("Socket"),
        Description("Occurs when the underlying Socket connection for receiving time-series data is terminated.")]
        public event EventHandler SocketDisconnected;

        /// <summary>
        /// Occurs when the <see cref="Data"/> is being populated on <see cref="Start"/>up.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the Data is being populated on Startup.")]
        public event EventHandler DataInitStart;

        /// <summary>
        /// Occurs when the <see cref="Data"/> is populated completely on <see cref="Start"/>up.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the Data is populated completely on Startup.")]
        public event EventHandler DataInitComplete;

        /// <summary>
        /// Occurs when the <see cref="Data"/> cannot be populated completely on <see cref="Start"/>up.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the Data cannot be populated completely on Startup.")]
        public event EventHandler DataInitPartial;

        /// <summary>
        /// Occurs when the <see cref="Data"/> cannot be populated on <see cref="Start"/>up due to the unavailability of the <see cref="Server"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the Data cannot be populated on Startup due to the unavailability of the Server.")]
        public event EventHandler DataInitFailure;

        /// <summary>
        /// Occurs when time-series data is extracted from the received packets.
        /// </summary>
        [Category("Data"),
        Description("Occurs when time-series data is extracted from the received packets.")]
        public event EventHandler<EventArgs<IList<IDataPoint>>> DataExtracted;

        /// <summary>
        /// Occurs when the <see cref="Data"/> has changed.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the Data has changed.")]
        public event EventHandler DataChanged;

        // Fields
        private string m_id;
        private string m_server;
        private int m_port;
        private TransportProtocol m_protocol;
        private bool m_connectToServer;
        private bool m_cacheData;
        private bool m_initializeData;
        private int m_initializeDataTimeout;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private long m_totalBytesReceived;
        private long m_totalPacketsReceived;
        private List<IDataPoint> m_data;
        private bool m_listenerStopping;
        private Thread m_startupThread;
        private AutoResetEvent m_initializeWaitHandle;
        private bool m_disposed;
        private bool m_initialized;
        // WithEvents
        private PacketParser m_parser;
        private TcpClient m_tcpClient;
        private UdpClient m_udpClient;
        private TcpServer m_tcpServer;
        private TcpClient m_dataInitClient;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="DataListener"/> class.
        /// </summary>
        public DataListener()
            : base()
        {
            m_id = DefaultID;
            m_server = DefaultServer;
            m_port = DefaultPort;
            m_protocol = DefaultProtocol;
            m_connectToServer = DefaultConnectToServer;
            m_cacheData = DefaultCacheData;
            m_initializeData = DefaultInitializeData;
            m_initializeDataTimeout = DefaultInitializeDataTimeout;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_data = new List<IDataPoint>();
            m_initializeWaitHandle = new AutoResetEvent(false);

            m_parser = new PacketParser();
            m_parser.DataParsed += PacketParser_DataParsed;

            m_tcpClient = new TcpClient();
            m_tcpClient.ConnectionAttempt += ClientSocket_ConnectionAttempt;
            m_tcpClient.ConnectionEstablished += ClientSocket_ConnectionEstablished;
            m_tcpClient.ConnectionTerminated += ClientSocket_ConnectionTerminated;
            m_tcpClient.ReceiveDataComplete += ClientSocket_ReceiveDataComplete;

            m_udpClient = new UdpClient();
            m_udpClient.ConnectionAttempt += ClientSocket_ConnectionAttempt;
            m_udpClient.ConnectionEstablished += ClientSocket_ConnectionEstablished;
            m_udpClient.ConnectionTerminated += ClientSocket_ConnectionTerminated;
            m_udpClient.ReceiveDataComplete += ClientSocket_ReceiveDataComplete;

            m_tcpServer = new TcpServer();
            m_tcpServer.ServerStarted += ServerSocket_ServerStarted;
            m_tcpServer.ServerStopped += ServerSocket_ServerStopped;
            m_tcpServer.ReceiveClientDataComplete += ServerSocket_ReceiveClientDataComplete;

            m_dataInitClient = new TcpClient();
            m_dataInitClient.ConnectionString = "Server={0}:1003";
            m_dataInitClient.PayloadAware = true;
            m_dataInitClient.MaxConnectionAttempts = 10;
            m_dataInitClient.ReceiveDataComplete += DataInitClient_ReceiveDataComplete;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataListener"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="DataListener"/>.</param>
        public DataListener(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the alpha-numeric identifier of the <see cref="DataListener"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Identity"),
        DefaultValue(DefaultID),
        Description("Alpha-numeric identifier of the DataListener.")]
        public string ID
        {
            get
            {
                return m_id;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets the DNS name or IP address of the server from where the <see cref="DataListener"/> will get the time-series data.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Connection"),
        DefaultValue(DefaultServer),
        Description("DNS name or IP address of the server from where the DataListener will get the time-series data.")]
        public string Server
        {
            get
            {
                return m_server;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_server = value;
            }
        }

        /// <summary>
        /// Gets or sets the network port of the <see cref="Server"/> where the <see cref="DataListener"/> will connect to get the time-series data.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 0 and 65535.</exception>
        [Category("Connection"),
        DefaultValue(DefaultPort),
        Description("Network port of the Server where the DataListener will connect to get the time-series data.")]
        public int Port
        {
            get
            {
                return m_port;
            }
            set
            {
                if (!Transport.IsPortNumberValid(value.ToString()))
                    throw new ArgumentException("Value must be between 0 and 65535");

                m_port = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TransportProtocol"/> to be used for receiving time-series data from the <see cref="Server"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not Tcp or Udp.</exception>
        [Category("Connection"),
        DefaultValue(DefaultProtocol),
        Description("Protocol to be used for receiving time-series data from the Server.")]
        public TransportProtocol Protocol
        {
            get
            {
                return m_protocol;
            }
            set
            {
                if (!(value == TransportProtocol.Tcp || value == TransportProtocol.Udp))
                    throw new ArgumentException("Value must be Tcp or Udp");

                m_protocol = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="DataListener"/> will connect to the <see cref="Server"/> 
        /// for receiving the time-series data or the <see cref="Server"/> will make a connection to the <see cref="DataListener"/> on 
        /// the specified <see cref="Port"/> for sending time-series data.
        /// </summary>
        [Category("Connection"),
        DefaultValue(DefaultConnectToServer),
        Description("Indicates whether the DataListener will connect to the Server for receiving the time-series data or the Server will make a connection to the DataListener on the specified Port for sending time-series data.")]
        public bool ConnectToServer
        {
            get
            {
                return m_connectToServer;
            }
            set
            {
                m_connectToServer = value;
            }
        }
       
        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="Data"/> is to be updated with the latest time-series data.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="CacheData"/> is being disabled when <see cref="InitializeData"/> is enabled.</exception>
        [Category("Data"),
        DefaultValue(DefaultCacheData),
        Description("Indicates whether the Data is to be updated with the latest time-series data.")]
        public bool CacheData 
        {
            get
            {
                return m_cacheData;
            }
            set
            {
                if (!value && m_initializeData)
                    throw new InvalidOperationException("CacheData cannot be disabled when InitializeData is enabled");

                m_cacheData = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="DataListener"/> will initialize the <see cref="Data"/> from the <see cref="Server"/> on startup.
        /// </summary>
        /// <remarks>
        /// <see cref="InitializeData"/> should be enabled only if the <see cref="Server"/> software on port 1003 is programmed to accept <see cref="PacketType11"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException"><see cref="InitializeData"/> is being enabled when <see cref="CacheData"/> is disabled.</exception>
        [Category("Data"),
        DefaultValue(DefaultInitializeData),
        Description("Indicates whether the DataListener will initialize the Data from the Server on startup.")]
        public bool InitializeData
        {
            get
            {
                return m_initializeData;
            }
            set
            {
                if (value && !m_cacheData)
                    throw new InvalidOperationException("InitializeData cannot be enabled when CacheData is disabled");

                m_initializeData = value;
            }
        }

        /// <summary>
        /// Gets or sets the time (in milliseconds) to wait for the <see cref="Data"/> to be initialized from the <see cref="Server"/> on <see cref="Start"/>up.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
        [Category("Data"),
        DefaultValue(DefaultInitializeDataTimeout),
        Description("Time (in milliseconds) to wait for the Data to be initialized from the Server on Startup.")]
        public int InitializeDataTimeout
        {
            get
            {
                return m_initializeDataTimeout;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive");

                m_initializeDataTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="DataListener"/> are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of DataListener are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of <see cref="DataListener"/> are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of DataListener are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="DataListener"/> is currently enabled.
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
                return (RunTime != 0.0);
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
        /// Gets the newest time-series data received by the <see cref="DataListener"/>.
        /// </summary>
        /// <remarks>
        /// WARNING: <see cref="Data"/> is thread unsafe. Synchronized access is required.
        /// </remarks>
        [Browsable(false)]
        public IList<IDataPoint> Data
        {
            get
            {
                return m_data.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="PacketParser"/> used the <see cref="DataListener"/> for extracting the time-series data.
        /// </summary>
        [Browsable(false)]
        public PacketParser Parser
        {
            get
            {
                return m_parser;
            }
        }

        /// <summary>
        /// Gets the up-time (in seconds) of the <see cref="DataListener"/>.
        /// </summary>
        [Browsable(false)]
        public Time RunTime
        {
            get
            {
                if (m_tcpServer.CurrentState == ServerState.Running)
                    return m_tcpServer.RunTime;
                else if (m_tcpClient.CurrentState == ClientState.Connected)
                    return m_tcpClient.ConnectionTime;
                else if (m_udpClient.CurrentState == ClientState.Connected)
                    return m_udpClient.ConnectionTime;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the total number of bytes received by the <see cref="DataListener"/> since it was <see cref="Start"/>ed.
        /// </summary>
        [Browsable(false)]
        public long TotalBytesReceived
        {
            get
            {
                return m_totalBytesReceived;
            }
        }

        /// <summary>
        /// Gets the total number of packets received by the <see cref="DataListener"/> since it was <see cref="Start"/>ed.
        /// </summary>
        [Browsable(false)]
        public long TotalPacketsReceived
        {
            get
            {
                return m_totalPacketsReceived;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="DataListener"/>.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return m_id;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="DataListener"/>.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.AppendFormat("                      Name: {0}", m_id);
                status.AppendLine();
                status.AppendFormat("            Server address: {0}", m_server);
                status.AppendLine();
                status.AppendFormat("               Server port: {0}", m_port);
                status.AppendLine();
                status.AppendFormat("        Transport protocol: {0}", m_protocol);
                status.AppendLine();
                status.AppendFormat("      Total bytes received: {0}", m_totalBytesReceived);
                status.AppendLine();
                status.AppendFormat("    Total packets received: {0}", m_totalPacketsReceived);
                status.AppendLine();
                status.AppendFormat("                  Run time: {0}", RunTime.ToString());
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="DataListener"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="DataListener"/> is not consumed through the designer surface of the IDE.
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
        /// Performs necessary operations before the <see cref="DataListener"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="DataListener"/> is consumed through the designer surface of the IDE.
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
        /// Performs necessary operations after the <see cref="DataListener"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="DataListener"/> is consumed through the designer surface of the IDE.
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
        /// Saves settings for the <see cref="DataListener"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void SaveSettings()
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
                element = settings["ID", true];
                element.Update(m_id, element.Description, element.Encrypted);
                element = settings["Server", true];
                element.Update(m_server, element.Description, element.Encrypted);
                element = settings["Port", true];
                element.Update(m_port, element.Description, element.Encrypted);
                element = settings["Protocol", true];
                element.Update(m_protocol, element.Description, element.Encrypted);
                element = settings["ConnectToServer", true];
                element.Update(m_connectToServer, element.Description, element.Encrypted);
                element = settings["CacheData", true];
                element.Update(m_cacheData, element.Description, element.Encrypted);
                element = settings["InitializeData", true];
                element.Update(m_initializeData, element.Description, element.Encrypted);
                element = settings["InitializeDataTimeout", true];
                element.Update(m_initializeDataTimeout, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="DataListener"/> from the config file if the <see cref="PersistSettings"/>  property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("ID", m_id, "Alpha-numeric identifier of the listener.");
                settings.Add("Server", m_server, "DNS name or IP address of the server providing the time-series data.");
                settings.Add("Port", m_port, "Network port at the server where the time-series data is being server.");
                settings.Add("Protocol", m_protocol, "Protocol (Tcp; Udp) to be used for receiving time-series data.");
                settings.Add("ConnectToServer", m_connectToServer, "True is the listener to initiate connection to the server; otherwise False;");
                settings.Add("CacheData", m_cacheData, "True if newest data is to be cached locally; otherwise False.");
                settings.Add("InitializeData", m_initializeData, "True if data is to be initialized from the server on startup; otherwise False.");
                settings.Add("InitializeDataTimeout", m_initializeDataTimeout, "Number of milliseconds to wait for data to be initialized from the server on startup.");
                ID = settings["ID"].ValueAs(m_id);
                Server = settings["Server"].ValueAs(m_server);
                Port = settings["Port"].ValueAs(m_port);
                Protocol = settings["Protocol"].ValueAs(m_protocol);
                ConnectToServer = settings["ConnectToServer"].ValueAs(m_connectToServer);
                CacheData = settings["CacheData"].ValueAs(m_cacheData);
                InitializeData = settings["InitializeData"].ValueAs(m_initializeData);
                InitializeDataTimeout = settings["InitializeDataTimeout"].ValueAs(m_initializeDataTimeout);
            }
        }

        /// <summary>
        /// Starts the <see cref="DataListener"/> synchronously.
        /// </summary>
        public void Start()
        {
            if (!Enabled)
            {
                m_listenerStopping = false;

                OnListenerStarting();

                if (m_initializeData)
                {
                    // Attempt to initialize data from the server.
                    OnDataInitStart();
                    m_dataInitClient.ConnectionString = string.Format(m_dataInitClient.ConnectionString, Server);
                    m_dataInitClient.Connect();
                    if (m_dataInitClient.CurrentState == ClientState.Connected)
                    {
                        // Wait enough for the handshaking to complete.
                        Thread.Sleep(1000);

                        // We'll request current data for all points.
                        PacketType11 request = new PacketType11();
                        request.RequestIDs.Add(-1);
                        m_dataInitClient.Send(request.BinaryImage);

                        // Wait for the data to be initialized and timeout if it takes too long.
                        if (!m_initializeWaitHandle.WaitOne(m_initializeDataTimeout, false))
                            OnDataInitPartial();
                        else
                            OnDataInitComplete();
                        m_dataInitClient.Disconnect();
                    }
                    else
                    {
                        // Archiver is either not running, or is running but is a legacy Archiver.
                        OnDataInitFailure();
                    }
                }

                // Start-up the appropriate communication component that'll get the raw data.
                if (Protocol == TransportProtocol.Udp)
                {
                    m_udpClient.ConnectionString = string.Format("Port={0}", Port);
                    m_udpClient.Connect(); // Start the connection cycle - try indefinately.
                }
                else
                {
                    if (m_connectToServer)
                    {
                        // TCP connection - going out to the server for connection.
                        m_tcpClient.ConnectionString = string.Format("Server={0}:{1}", Server, Port);
                        m_tcpClient.Connect(); // Start the connection cycle - try indefinately.
                    }
                    else
                    {
                        // TCP connection - client coming in for connection.
                        m_tcpServer.ConfigurationString = string.Format("Port={0}", Port);
                        m_tcpServer.Start(); // Start the connection cycle - try indefinately.
                        OnSocketConnecting();
                    }
                }

                // Start the parser that'll parse the raw data.
                m_parser.Start();
            }
        }

        /// <summary>
        /// Starts the <see cref="DataListener"/> asynchronously.
        /// </summary>
        public void StartAsync()
        {
            // Only allow one async startup attempt.
            if (m_startupThread != null && m_startupThread.IsAlive)
                return;

            m_startupThread = new Thread(Start);
            m_startupThread.Start();
        }

        /// <summary>
        /// Stops the <see cref="DataListener"/>.
        /// </summary>
        public void @Stop()
        {
            OnListenerStopping();

            // Abort async startup process if it is running.
            if (m_startupThread != null)
                m_startupThread.Abort();

            // Prevent communication clients from reconnecting.
            m_listenerStopping = true;

            m_tcpServer.Stop();
            m_tcpClient.Disconnect();
            m_udpClient.Disconnect();
            m_parser.Stop();
            m_totalBytesReceived = 0;
            m_totalPacketsReceived = 0;

            OnListenerStopped();
        }

        /// <summary>
        /// Gets the current data for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier whose current data is to be retrieved.</param>
        /// <returns><see cref="IDataPoint"/> if a match is found; otherwise null.</returns>
        public IDataPoint FindData(int historianID)
        {
            IDataPoint currentData = null;
            lock (m_data)
            {
                if (historianID > 0 && historianID <= m_data.Count)
                {
                    // Valid id is specified, so we'll lookup it's current data.
                    currentData = m_data[historianID - 1];
                }
            }
            return currentData;
        }

        /// <summary>
        /// Determines whether the current <see cref="DataListener"/> object is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="DataListener"/> object is to be compared for equality.</param>
        /// <returns>true if the current <see cref="DataListener"/> object is equal to <paramref name="obj"/>; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            DataListener other = obj as DataListener;
            if (other == null)
                return false;
            else
                return string.Compare(m_id, other.ID, true) == 0;
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="DataListener"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer value.</returns>
        public override int GetHashCode()
        {
            return m_id.GetHashCode();
        }

        /// <summary>
        /// Raises the <see cref="ListenerStarting"/> event.
        /// </summary>
        protected virtual void OnListenerStarting()
        {
            if (ListenerStarting != null)
                ListenerStarting(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ListenerStarted"/> event.
        /// </summary>
        protected virtual void OnListenerStarted()
        {
            if (ListenerStarted != null)
                ListenerStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ListenerStopping"/> event.
        /// </summary>
        protected virtual void OnListenerStopping()
        {
            if (ListenerStopping != null)
                ListenerStopping(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ListenerStopped"/> event.
        /// </summary>
        protected virtual void OnListenerStopped()
        {
            if (ListenerStopped != null)
                ListenerStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SocketConnecting"/> event.
        /// </summary>
        protected virtual void OnSocketConnecting()
        {
            if (SocketConnecting != null)
                SocketConnecting(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SocketConnected"/> event.
        /// </summary>
        protected virtual void OnSocketConnected()
        {
            if (SocketConnected != null)
                SocketConnected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SocketDisconnected"/> event.
        /// </summary>
        protected virtual void OnSocketDisconnected()
        {
            if (SocketDisconnected != null)
                SocketDisconnected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataInitStart"/> event.
        /// </summary>
        protected virtual void OnDataInitStart()
        {
            if (DataInitStart != null)
                DataInitStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataInitComplete"/> event.
        /// </summary>
        protected virtual void OnDataInitComplete()
        {
            if (DataInitComplete != null)
                DataInitComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataInitPartial"/> event.
        /// </summary>
        protected virtual void OnDataInitPartial()
        {
            if (DataInitPartial != null)
                DataInitPartial(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataInitFailure"/> event.
        /// </summary>
        protected virtual void OnDataInitFailure()
        {
            if (DataInitFailure != null)
                DataInitFailure(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataExtracted"/> event.
        /// </summary>
        /// <param name="data">Extracted time-series data to send to <see cref="DataExtracted"/> event.</param>
        protected virtual void OnDataExtracted(IList<IDataPoint> data)
        {
            if (DataExtracted != null)
                DataExtracted(this, new EventArgs<IList<IDataPoint>>(data));
        }
        
        /// <summary>
        /// Raises the <see cref="DataChanged"/> event.
        /// </summary>
        protected virtual void OnDataChanged()
        {
            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataListener"/> and optionally releases the managed resources.
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

                        if (m_data != null)
                            m_data.Clear();

                        if (m_parser != null)
                        {
                            m_parser.DataParsed -= PacketParser_DataParsed;
                            m_parser.Dispose();
                        }

                        if (m_tcpServer != null)
                        {
                            m_tcpServer.ServerStarted -= ServerSocket_ServerStarted;
                            m_tcpServer.ServerStopped -= ServerSocket_ServerStopped;
                            m_tcpServer.ReceiveClientDataComplete -= ServerSocket_ReceiveClientDataComplete;
                            m_tcpServer.Dispose();
                        }

                        if (m_tcpClient != null)
                        {
                            m_tcpClient.ConnectionAttempt -= ClientSocket_ConnectionAttempt;
                            m_tcpClient.ConnectionEstablished -= ClientSocket_ConnectionEstablished;
                            m_tcpClient.ConnectionTerminated -= ClientSocket_ConnectionTerminated;
                            m_tcpClient.ReceiveDataComplete -= ClientSocket_ReceiveDataComplete;
                            m_tcpClient.Dispose();
                        }

                        if (m_udpClient != null)
                        {
                            m_udpClient.ConnectionAttempt -= ClientSocket_ConnectionAttempt;
                            m_udpClient.ConnectionEstablished -= ClientSocket_ConnectionEstablished;
                            m_udpClient.ConnectionTerminated -= ClientSocket_ConnectionTerminated;
                            m_udpClient.ReceiveDataComplete -= ClientSocket_ReceiveDataComplete;
                            m_udpClient.Dispose();
                        }

                        if (m_dataInitClient != null)
                        {
                            m_dataInitClient.ReceiveDataComplete -= DataInitClient_ReceiveDataComplete;
                            m_dataInitClient.Dispose();
                        }

                        if (m_initializeWaitHandle != null)
                            m_initializeWaitHandle.Close();
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        private void DataInitClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            StateRecordSummary state = new StateRecordSummary(e.Argument1, 0, e.Argument2);
            if (state.HistorianID > 0)
            {
                lock (m_data)
                {
                    m_data.Add(new ArchiveDataPoint(state.HistorianID, state.CurrentData.Time, state.CurrentData.Value, state.CurrentData.Quality));
                }
            }
            else
            {
                // This is the end-of-transmission to our request for current data from the server.
                m_initializeWaitHandle.Set();
            }
        }

        private void ServerSocket_ServerStarted(object sender, EventArgs e)
        {
            OnSocketConnected();
            OnListenerStarted();
        }

        private void ServerSocket_ServerStopped(object sender, EventArgs e)
        {
            OnSocketDisconnected();
        }

        private void ServerSocket_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            m_totalPacketsReceived++;
            m_totalBytesReceived += e.Argument3;
            m_parser.Parse(e.Argument1, e.Argument2, 0, e.Argument3);
        }

        private void ClientSocket_ConnectionAttempt(object sender, EventArgs e)
        {
            OnSocketConnecting();
        }

        private void ClientSocket_ConnectionEstablished(object sender, EventArgs e)
        {
            OnSocketConnected();
            OnListenerStarted();
        }

        private void ClientSocket_ConnectionTerminated(object sender, EventArgs e)
        {
            OnSocketDisconnected();

            // Re-attempt connection to the server if the disconnect was not deliberate.
            if (!m_listenerStopping)
                ((IClient)sender).Connect();
        }

        private void ClientSocket_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            m_totalPacketsReceived++;
            m_totalBytesReceived += e.Argument2;
            m_parser.Parse(((IClient)sender).ClientID, e.Argument1, 0, e.Argument2);
        }

        private void PacketParser_DataParsed(object sender, EventArgs<Guid, IList<IPacket>> e)
        {
            // Extract data from the packets.
            IEnumerable<IDataPoint> extractedData;
            List<IDataPoint> dataPoints = new List<IDataPoint>();
            foreach (IPacket packet in e.Argument2)
            {
                    extractedData = packet.ExtractTimeSeriesData();
                    if (extractedData != null)
                        dataPoints.AddRange(extractedData);
            }

            if (dataPoints.Count > 0)
            {
                // Published the extracted data.
                OnDataExtracted(dataPoints);

                // Cache extracted data for reuse.
                if (m_cacheData)
                {
                    lock (m_data)
                    {
                        foreach (IDataPoint dataPoint in dataPoints)
                        {
                            if (dataPoint.HistorianID > m_data.Count)
                            {
                                // No data exists for the id, so add one for it and others in-between.
                                for (int i = m_data.Count + 1; i <= dataPoint.HistorianID; i++)
                                {
                                    m_data.Add(new ArchiveDataPoint(i));
                                }
                            }

                            // Replace existing data with the new data.
                            m_data[dataPoint.HistorianID - 1] = dataPoint;
                        }
                    }
                    OnDataChanged();
                }
            }
        }

        #endregion
    }
}
