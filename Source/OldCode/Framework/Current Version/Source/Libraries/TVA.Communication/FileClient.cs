//*******************************************************************************************************
//  FileClient.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/24/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed file data access.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/08/2009 - J. Ritchie Carroll
//       Added WaitHandle return value from asynchronous connection.
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
using System.IO;
using System.Threading;
using TVA.Configuration;

namespace TVA.Communication
{
    /// <summary>
    /// Represents a communication client based on <see cref="FileStream"/>.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="FileClient"/> for writing data to a file:
    /// <code>
    /// using System;
    /// using TVA.Communication;
    /// 
    /// class Program
    /// {
    ///     static FileClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         m_client = new FileClient(@"File=c:\File.txt");
    ///         m_client.Initialize();
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.SendDataComplete += m_client_SendDataComplete;
    ///         // Connect the client.
    ///         m_client.Connect();
    /// 
    ///         // Write user input to the file.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_client.Send(input + "\r\n");
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from file.");
    ///     }
    /// 
    ///     static void m_client_SendDataComplete(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine(string.Format("Sent data - {0}", m_client.TextEncoding.GetString(m_client.Client.SendBuffer)));
    ///     }
    /// }
    /// </code>
    /// This example shows how to use <see cref="FileClient"/> for reading data to a file:
    /// <code>
    /// using System;
    /// using TVA.Communication;
    /// 
    /// class Program
    /// {
    ///     static FileClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         m_client = new FileClient(@"File=c:\File.txt");
    ///         m_client.Initialize();
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.ReceiveDataComplete += m_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         m_client.Connect();
    /// 
    ///         // Wait for client to read data.
    ///         Console.ReadLine();
    /// 
    ///         // Disconnect the client on shutdown.
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from file.");
    ///     }
    /// 
    ///     static void m_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}", m_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class FileClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="AutoRepeat"/> property.
        /// </summary>
        public const bool DefaultAutoRepeat = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveOnDemand"/> property.
        /// </summary>
        public const bool DefaultReceiveOnDemand = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveInterval"/> property.
        /// </summary>
        public const int DefaultReceiveInterval = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="StartingOffset"/> property.
        /// </summary>
        public const long DefaultStartingOffset = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="FileOpenMode"/> property.
        /// </summary>
        public const FileMode DefaultFileOpenMode = FileMode.OpenOrCreate;

        /// <summary>
        /// Specifies the default value for the <see cref="FileShareMode"/> property.
        /// </summary>
        public const FileShare DefaultFileShareMode = FileShare.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="FileAccessMode"/> property.
        /// </summary>
        public const FileAccess DefaultFileAccessMode = FileAccess.ReadWrite;
        
        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "File=DataFile.txt";

        // Fields
        private bool m_autoRepeat;
        private bool m_receiveOnDemand;
        private double m_receiveInterval;
        private long m_startingOffset;
        private FileMode m_fileOpenMode;
        private FileShare m_fileShareMode;
        private FileAccess m_fileAccessMode;
        private TransportProvider<FileStream> m_fileClient;
        private Dictionary<string, string> m_connectData;
        private System.Timers.Timer m_receiveDataTimer;
#if ThreadTracking
        private ManagedThread m_connectionThread;
#else
        private Thread m_connectionThread;
#endif
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        public FileClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="FileClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public FileClient(string connectString)
            : base(TransportProtocol.File, connectString)
        {
            m_autoRepeat = DefaultAutoRepeat;
            m_receiveOnDemand = DefaultReceiveOnDemand;
            m_receiveInterval = DefaultReceiveInterval;
            m_startingOffset = DefaultStartingOffset;
            m_fileOpenMode = DefaultFileOpenMode;
            m_fileShareMode = DefaultFileShareMode;
            m_fileAccessMode = DefaultFileAccessMode;
            m_fileClient = new TransportProvider<FileStream>();
            m_receiveDataTimer = new System.Timers.Timer();
            m_receiveDataTimer.Elapsed += m_receiveDataTimer_Elapsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="FileClient"/>.</param>
        public FileClient(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether receiving (reading) of data is to be repeated endlessly.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="AutoRepeat"/> is enabled when <see cref="FileAccessMode"/> is <see cref="FileAccess.ReadWrite"/></exception>
        [Category("Data"),
        DefaultValue(DefaultAutoRepeat),
        Description("Indicates whether receiving (reading) of data is to be repeated endlessly.")]
        public bool AutoRepeat
        {
            get
            {
                return m_autoRepeat;
            }
            set
            {
                if (value && m_fileAccessMode == FileAccess.ReadWrite)
                    throw new InvalidOperationException("AutoRepeat cannot be enabled when FileAccessMode is FileAccess.ReadWrite");

                m_autoRepeat = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether receiving (reading) of data will be initiated manually by calling <see cref="ReceiveData()"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ReceiveInterval"/> will be set to -1 when <see cref="ReceiveOnDemand"/> is enabled.
        /// </remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveTimeout),
        Description("Indicates whether receiving (reading) of data will be initiated manually by calling ReceiveData().")]
        public bool ReceiveOnDemand
        {
            get
            {
                return m_receiveOnDemand;
            }
            set
            {
                m_receiveOnDemand = value;

                if (m_receiveOnDemand)
                    // We'll disable receiving data at a set interval if user wants to receive data on demand.
                    m_receiveInterval = -1;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds to pause before receiving (reading) the next available set of data.
        /// </summary>
        /// <remarks>
        /// Set <see cref="ReceiveInterval"/> = -1 to receive (read) data continuously without pausing.
        /// </remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveInterval),
        Description("The number of milliseconds to pause before receiving (reading) the next available set of data. Set ReceiveInterval = -1 to receive data continuously without pausing.")]
        public double ReceiveInterval
        {
            get
            {
                return m_receiveInterval;
            }
            set
            {
                if (value < 1)
                    m_receiveInterval = -1;
                else
                    m_receiveInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting point relative to the beginning of the file from where the data is to be received (read).
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not a positive number.</exception>
        [Category("File"),
        DefaultValue(DefaultStartingOffset),
        Description("The starting point relative to the beginning of the file from where the data is to be received (read).")]
        public long StartingOffset
        {
            get
            {
                return m_startingOffset;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive");

                m_startingOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileMode"/> value to be used when opening the file.
        /// </summary>
        [Category("File"),
        DefaultValue(DefaultFileOpenMode),
        Description("The System.IO.FileMode value to be used when opening the file.")]
        public FileMode FileOpenMode
        {
            get
            {
                return m_fileOpenMode;
            }
            set
            {
                m_fileOpenMode = value;
            }
        }

        /// <summary>
        /// Gets or set the <see cref="FileShare"/> value to be used when opening the file.
        /// </summary>
        [Category("File"),
        DefaultValue(DefaultFileShareMode),
        Description("The System.IO.FileShare value to be used when opening the file.")]
        public FileShare FileShareMode
        {
            get
            {
                return m_fileShareMode;
            }
            set
            {
                m_fileShareMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileAccess"/> value to be used when opening the file.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="FileAccessMode"/> is set to <see cref="FileAccess.ReadWrite"/> when <see cref="AutoRepeat"/> is enabled.</exception>
        [Category("File"),
        DefaultValue(DefaultFileAccessMode),
        Description("The System.IO.FileAccess value to be used when opening the file.")]
        public FileAccess FileAccessMode
        {
            get
            {
                return m_fileAccessMode;
            }
            set
            {
                if (value == FileAccess.ReadWrite && m_autoRepeat)
                    throw new InvalidOperationException("FileAccessMode cannot be set to FileAccess.ReadWrite when AutoRepeat is enabled");

                m_fileAccessMode = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{FileStream}"/> object for the <see cref="FileClient"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<FileStream> Client
        {
            get
            {
                return m_fileClient;
            }
        }

        /// <summary>
        /// Gets the server URI of the <see cref="FileClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri
        {
            get
            {
                return string.Format("{0}://{1}", TransportProtocol, m_connectData["file"]).ToLower();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Receives (reads) data from the <see cref="FileStream"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="ReceiveData()"/> is called when <see cref="FileClient"/> is not connected.</exception>
        /// <exception cref="InvalidOperationException"><see cref="ReceiveData()"/> is called when <see cref="ReceiveOnDemand"/> is disabled.</exception>
        public void ReceiveData()
        {
            if (m_receiveOnDemand)
            {
                if (CurrentState == ClientState.Connected)
                {
                    ReadData();
                }
                else
                {
                    throw new InvalidOperationException("Client is currently not connected");
                }
            }
            else
            {
                throw new InvalidOperationException("ReceiveData() cannot be used when ReceiveOnDemand is disabled");
            }
        }

        /// <summary>
        /// Disconnects the <see cref="FileClient"/> from the <see cref="FileStream"/>.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                m_fileClient.Reset();
                m_receiveDataTimer.Stop();

                if (m_connectionThread != null)
                    m_connectionThread.Abort();

                OnConnectionTerminated();
            }
        }

        /// <summary>
        /// Connects the <see cref="FileClient"/> to the <see cref="FileStream"/> asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="FileClient"/> when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            WaitHandle handle = base.ConnectAsync();

            m_fileClient.ID = this.ClientID;
            m_fileClient.Secretkey = this.SharedSecret;
            m_fileClient.ReceiveBuffer = new byte[ReceiveBufferSize];
#if ThreadTracking
            m_connectionThread = new ManagedThread(OpenFile);
            m_connectionThread.Name = "TVA.Communication.FileClient.OpenFile()";
#else
            m_connectionThread = new Thread(OpenFile);
#endif
            m_connectionThread.Start();

            return handle;
        }

        /// <summary>
        /// Saves <see cref="FileClient"/> settings to the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                element = settings["AutoRepeat", true];
                element.Update(m_autoRepeat, element.Description, element.Encrypted);
                element = settings["ReceiveOnDemand", true];
                element.Update(m_receiveOnDemand, element.Description, element.Encrypted);
                element = settings["ReceiveInterval", true];
                element.Update(m_receiveInterval, element.Description, element.Encrypted);
                element = settings["StartingOffset", true];
                element.Update(m_startingOffset, element.Description, element.Encrypted);
                element = settings["FileOpenMode", true];
                element.Update(m_fileOpenMode, element.Description, element.Encrypted);
                element = settings["FileShareMode", true];
                element.Update(m_fileShareMode, element.Description, element.Encrypted);
                element = settings["FileAccessMode", true];
                element.Update(m_fileAccessMode, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="FileClient"/> settings from the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("AutoRepeat", m_autoRepeat, "True if receiving (reading) of data is to be repeated endlessly, otherwise False.");
                settings.Add("ReceiveOnDemand", m_receiveOnDemand, "True if receiving (reading) of data will be initiated manually, otherwise False.");
                settings.Add("ReceiveInterval", m_receiveInterval, "Number of milliseconds to pause before receiving (reading) the next available set of data.");
                settings.Add("StartingOffset", m_startingOffset, "Starting point relative to the beginning of the file from where the data is to be received (read).");
                settings.Add("FileOpenMode", m_fileOpenMode, "Open mode (CreateNew; Create; Open; OpenOrCreate; Truncate; Append) to be used when opening the file.");
                settings.Add("FileShareMode", m_fileShareMode, "Share mode (None; Read; Write; ReadWrite; Delete; Inheritable) to be used when opening the file.");
                settings.Add("FileAccessMode", m_fileAccessMode, "Access mode (Read; Write; ReadWrite) to be used when opening the file.");
                AutoRepeat = settings["AutoRepeat"].ValueAs(m_autoRepeat);
                ReceiveOnDemand = settings["ReceiveOnDemand"].ValueAs(m_receiveOnDemand);
                ReceiveInterval = settings["ReceiveInterval"].ValueAs(m_receiveInterval);
                StartingOffset = settings["StartingOffset"].ValueAs(m_startingOffset);
                FileOpenMode = settings["FileOpenMode"].ValueAs(m_fileOpenMode);
                FileShareMode = settings["FileShareMode"].ValueAs(m_fileShareMode);
                FileAccessMode = settings["FileAccessMode"].ValueAs(m_fileAccessMode);
            }
        }
       
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileClient"/> and optionally releases the managed resources.
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
                        if (m_receiveDataTimer != null)
                        {
                            m_receiveDataTimer.Elapsed -= m_receiveDataTimer_Elapsed;
                            m_receiveDataTimer.Dispose();
                        }
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
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">File property is missing.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            if (!m_connectData.ContainsKey("file"))
                throw new ArgumentException(string.Format("File property is missing (Example: {0})", DefaultConnectionString));
        }

        /// <summary>
        /// Gets the secret key to be used for ciphering client data.
        /// </summary>
        /// <returns>Cipher secret key.</returns>
        protected override string GetSessionSecret()
        {
            return m_fileClient.Secretkey;
        }

        /// <summary>
        /// Sends (writes) data to the file asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent (written).</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending (writing) data.</param>
        /// <param name="length">The number of bytes to be sent (written) from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            WaitHandle handle;

            // Send data to the file asynchronously.
            lock (m_fileClient.Provider)
            {
                handle = m_fileClient.Provider.BeginWrite(data, offset, length, SendDataAsyncCallback, null).AsyncWaitHandle;
            }

            // Notify that the send operation has started.
            m_fileClient.SendBuffer = data;
            m_fileClient.SendBufferOffset = offset;
            m_fileClient.SendBufferLength = length;
            m_fileClient.Statistics.UpdateBytesSent(m_fileClient.SendBufferLength);
            OnSendDataStart();

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void SendDataAsyncCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Send operation is complete.
                lock (m_fileClient)
                {
                    m_fileClient.Provider.EndWrite(asyncResult);
                    m_fileClient.Provider.Flush();
                }
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
        }

        /// <summary>
        /// Connects to the <see cref="FileStream"/>.
        /// </summary>
        private void OpenFile()
        {
            int connectionAttempts = 0;
            while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
            {
                try
                {
                    OnConnectionAttempt(); ;

                    // Open the file.
                    m_fileClient.Provider = new FileStream(m_connectData["file"], m_fileOpenMode, m_fileAccessMode, m_fileShareMode);
                    // Move to the specified offset.
                    m_fileClient.Provider.Seek(m_startingOffset, SeekOrigin.Begin); 

                    OnConnectionEstablished();

                    if (!m_receiveOnDemand)
                    {
                        if (m_receiveInterval > 0)
                        {
                            // Start receiving data at interval.
                            m_receiveDataTimer.Interval = m_receiveInterval;
                            m_receiveDataTimer.Start();
                        }
                        else
                        {
                            // Start receiving data continuously.
                            while (true)
                            {
                                ReadData();         // Read all available data.
                                Thread.Sleep(1000); // Wait for more data to be available.
                            }
                        }
                    }

                    break;  // We're done here.
                }
                catch (ThreadAbortException)
                {
                    // Exit gracefully.
                    break;
                }
                catch (Exception ex)
                {
                    // Keep retrying connecting to the file.
                    connectionAttempts++;
                    OnConnectionException(ex);
                }
            }
        }

        /// <summary>
        /// Receive (reads) data from the <see cref="FileStream"/>.
        /// </summary>
        private void ReadData()
        {
            try
            {
                // Process the entire file content
                while (m_fileClient.Provider.Position < m_fileClient.Provider.Length)
                {
                    // Retrieve data from the file.
                    lock (m_fileClient.Provider)
                    {
                        m_fileClient.ReceiveBufferLength = m_fileClient.Provider.Read(m_fileClient.ReceiveBuffer, m_fileClient.ReceiveBufferOffset, m_fileClient.ReceiveBuffer.Length);
                    }
                    m_fileClient.Statistics.UpdateBytesReceived(m_fileClient.ReceiveBufferLength);

                    // Notify of the retrieved data.
                    OnReceiveDataComplete(m_fileClient.ReceiveBuffer, m_fileClient.ReceiveBufferLength);

                    // Re-read the file if the user wants to repeat when done reading the file.
                    if (m_autoRepeat && m_fileClient.Provider.Position == m_fileClient.Provider.Length)
                    {
                        lock (m_fileClient.Provider)
                        {
                            m_fileClient.Provider.Seek(m_startingOffset, SeekOrigin.Begin);
                        }
                    }

                    // Stop processing the file if user has either opted to receive data on demand or receive data at a predefined interval.
                    if (m_receiveOnDemand || m_receiveInterval > 0)
                    {
                        break;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Exit gracefully.
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                OnReceiveDataException(ex);
            }
        }

        private void m_receiveDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ReadData();
        }

        #endregion
    }
}