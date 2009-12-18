//*******************************************************************************************************
//  ServiceHelper.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager. UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  12/14/2007 - Pinal C. Patel
//       Made monitoring of service health optional via the MonitorServiceHealth property.
//  09/30/2008 - J. Ritchie Carroll
//       Converted to C#.
//  03/06/2009 - Pinal C. Patel
//       Edited code comments.
//  06/19/2009 - Pinal C. Patel
//       Modified Initialize() method to just load settings from the config file.
//  06/30/2009 - Pinal C. Patel
//       Changed ServiceComponents to be a collection of object instead of ISupportLifecycle so it can
//       host a wide range of object implementing various interfaces used by the ServiceHelper commands.
//  07/15/2009 - Pinal C. Patel
//       Added AllowedRemoteUsers and ImpersonateRemoteUser properties as part of security.
//  07/16/2009 - Pinal C. Patel
//       Added SupportTelnetSessions property so that telnet support can be optionally turned-on.
//  07/17/2009 - Pinal C. Patel
//       Modified MonitorServiceHealth to default to false as it is not always needed.
//  07/30/2009 - Pinal C. Patel
//       Modified to set the principal of the thread where client request are handled to that of the the 
//       process or remote client (if transmitted by the remote client upon connection).
//  08/07/2009 - Pinal C. Patel
//       Subscribed to ErrorLogger.LoggingException event.
//  08/10/2009 - Josh L. Patterson
//       Edited Comments
//  09/08/2009 - Pinal C. Patel
//       Modified GetProcess(), GetConnectedClient() and GetClientRequestHandler() to use lambda
//       functions instead of manual iteration of collection items.
//       Renamed GetProcess(), GetConnectedClient() and GetClientRequestHandler() to FindProcess(), 
//       FindConnectedClient() and FindClientRequestHandler() respectively.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/23/2009 - Pinal C. Patel
//       Modified UpdateStatus() method to allow the type of update to be specified.
//  12/18/2009 - Pinal C. Patel
//       Added message flooding control to UpdateStatus().
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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using TVA.Communication;
using TVA.Configuration;
using TVA.Diagnostics;
using TVA.ErrorManagement;
using TVA.IO;
using TVA.Scheduling;
using TVA.Collections;

namespace TVA.Services
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the state of a Windows Service.
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// Service has started.
        /// </summary>
        Started,
        /// <summary>
        /// Service has stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// Service has paused.
        /// </summary>
        Paused,
        /// <summary>
        /// Service has resumed.
        /// </summary>
        Resumed,
        /// <summary>
        /// Service has shutdown.
        /// </summary>
        Shutdown
    }

    /// <summary>
    /// Indicates the type of service update.
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// Update is informational.
        /// </summary>
        Information,
        /// <summary>
        /// Update is a warning message.
        /// </summary>
        Warning,
        /// <summary>
        /// Update is an alarm message.
        /// </summary>
        Alarm
    }

    #endregion

    /// <summary>
    /// Component that provides added functionality to a Windows Service.
    /// </summary>
    /// <seealso cref="ServiceProcess"/>
    /// <seealso cref="ServiceResponse"/>
    /// <seealso cref="ClientInfo"/>
    /// <seealso cref="ClientRequest"/>
    /// <seealso cref="ClientRequestHandler"/>
    [ToolboxBitmap(typeof(ServiceHelper))]
    public class ServiceHelper : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Nested Types
        private class StatusUpdate
        {
            public StatusUpdate(Guid client, UpdateType type, string message)
            {
                this.Client = client;
                this.Type = type;
                this.Message = message;
            }

            public Guid Client;
            public UpdateType Type;
            public string Message;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="LogStatusUpdates"/> property.
        /// </summary>
        public const bool DefaultLogStatusUpdates = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxStatusUpdatesLength"/> property.
        /// </summary>
        public const int DefaultMaxStatusUpdatesLength = 8192;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxStatusUpdatesFrequency"/> property.
        /// </summary>
        public const int DefaultMaxStatusUpdatesFrequency = 30;

        /// <summary>
        /// Specifies the default value for the <see cref="MonitorServiceHealth"/> property.
        /// </summary>
        public const bool DefaultMonitorServiceHealth = false;

        /// <summary>
        /// Specifies the default value for the <see cref="RequestHistoryLimit"/> property.
        /// </summary>
        public const int DefaultRequestHistoryLimit = 50;

        /// <summary>
        /// Specifies the default value for the <see cref="SupportTelnetSessions"/> property.
        /// </summary>
        public const bool DefaultSupportTelnetSessions = false;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowedRemoteUsers"/> property.
        /// </summary>
        public const string DefaultAllowedRemoteUsers = "*";

        /// <summary>
        /// Specifies the default value for the <see cref="ImpersonateRemoteUser"/> property.
        /// </summary>
        public const bool DefaultImpersonateRemoteUser = false;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ServiceHelper";

        // Events

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> is starting.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService is starting.")]
        public event EventHandler<EventArgs<string[]>> ServiceStarting;

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> has started.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has started.")]
        public event EventHandler ServiceStarted;

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> is stopping.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService is stopping.")]
        public event EventHandler ServiceStopping;

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> has stopped.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has stopped.")]
        public event EventHandler ServiceStopped;

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> is pausing.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService is pausing.")]
        public event EventHandler ServicePausing;

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> has paused.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has paused.")]
        public event EventHandler ServicePaused;

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> is resuming.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService is resuming.")]
        public event EventHandler ServiceResuming;

        /// <summary>
        /// Occurs when the <see cref="ParentService"/> has resumed.
        /// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has resumed.")]
        public event EventHandler ServiceResumed;

        /// <summary>
        /// Occurs when the system is being shutdown.
        /// </summary>
        [Category("System"),
        Description("Occurs when the system is being shutdown.")]
        public event EventHandler SystemShutdown;

        /// <summary>
        /// Occurs when a request is received from one of the <see cref="RemoteClients"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the remote client that sent the request.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="ClientRequest"/> sent by the remote client.
        /// </remarks>
        [Category("Client"),
        Description("Occurs when a request is received from one of the RemoteClients.")]
        public event EventHandler<EventArgs<Guid, ClientRequest>> ReceivedClientRequest;

        /// <summary>
        /// Occurs when the state of a defiend <see cref="ServiceProcess"/> changes.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="ServiceProcess.Name"/>.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="ServiceProcess.CurrentState"/>
        /// </remarks>
        [Category("Process"),
        Description("Occurs when the state of a defiend ServiceProcess changes.")]
        public event EventHandler<EventArgs<string, ServiceProcessState>> ProcessStateChanged;

        // Fields
        private bool m_logStatusUpdates;
        private int m_maxStatusUpdatesLength;
        private int m_maxStatusUpdatesFrequency;
        private bool m_monitorServiceHealth;
        private int m_requestHistoryLimit;
        private bool m_supportTelnetSessions;
        private string m_telnetSessionPassword;
        private string m_allowedRemoteUsers;
        private bool m_impersonateRemoteUser;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private ServiceBase m_parentService;
        private ServerBase m_remotingServer;
        private LogFile m_statusLog;
        private ScheduleManager m_processScheduler;
        private ErrorLogger m_errorLogger;
        private PerformanceMonitor m_performanceMonitor;
        private List<ServiceProcess> m_processes;
        private List<object> m_serviceComponents;
        private List<ClientInfo> m_remoteClients;
        private List<ClientRequestInfo> m_clientRequestHistory;
        private List<ClientRequestHandler> m_clientRequestHandlers;
        private Dictionary<ISupportLifecycle, bool> m_componentEnabledStates;
        private ProcessQueue<StatusUpdate> m_statusUpdateQueue;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;
        private bool m_suppressUpdates;
        private Guid m_remoteCommandClientID;
        private Process m_remoteCommandProcess;
        private Ticks m_lastStatusUpdateTime;
        private int m_statusUpdatesDisplayed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHelper"/> class.
        /// </summary>
        public ServiceHelper()
            : base()
        {
            m_telnetSessionPassword = "s3cur3";
            m_logStatusUpdates = DefaultLogStatusUpdates;
            m_maxStatusUpdatesLength = DefaultMaxStatusUpdatesLength;
            m_maxStatusUpdatesFrequency = DefaultMaxStatusUpdatesFrequency;
            m_monitorServiceHealth = DefaultMonitorServiceHealth;
            m_requestHistoryLimit = DefaultRequestHistoryLimit;
            m_supportTelnetSessions = DefaultSupportTelnetSessions;
            m_allowedRemoteUsers = DefaultAllowedRemoteUsers;
            m_impersonateRemoteUser = DefaultImpersonateRemoteUser;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_processes = new List<ServiceProcess>();
            m_remoteClients = new List<ClientInfo>();
            m_clientRequestHistory = new List<ClientRequestInfo>();
            m_serviceComponents = new List<object>();
            m_clientRequestHandlers = new List<ClientRequestHandler>();
            m_componentEnabledStates = new Dictionary<ISupportLifecycle, bool>();

            // Components
            m_statusUpdateQueue = ProcessQueue<StatusUpdate>.CreateSynchronousQueue(ProcessStatusUpdates);
            m_statusUpdateQueue.Name = "StatusUpdateQueue";
            m_statusUpdateQueue.ProcessException += StatusUpdateQueue_ProcessException;

            m_statusLog = new LogFile();
            m_statusLog.FileName = "StatusLog.txt";
            m_statusLog.SettingsCategory = "StatusLog";
            m_statusLog.LogException += StatusLog_LogException;

            m_processScheduler = new ScheduleManager();
            m_processScheduler.SettingsCategory = "ProcessScheduler";
            m_processScheduler.ScheduleDue += Scheduler_ScheduleDue;

            m_errorLogger = new ErrorLogger();
            m_errorLogger.ExitOnUnhandledException = false;
            m_errorLogger.SettingsCategory = "ErrorLogger";
            m_errorLogger.ErrorLog.SettingsCategory = "ErrorLog";
            m_errorLogger.LoggingException += ErrorLogger_LoggingException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHelper"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ServiceHelper"/>.</param>
        public ServiceHelper(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether messages sent using <see cref="UpdateStatus(UpdateType,string,object[])"/> 
        /// or <see cref="UpdateStatus(Guid,UpdateType,string,object[])"/> are to be logged to the <see cref="StatusLog"/>.
        /// </summary>
        [Category("Updates"),
        DefaultValue(DefaultLogStatusUpdates),
        Description("Indicates whether messages sent using UpdateStatus() method overloads are to be logged to the StatusLog.")]
        public bool LogStatusUpdates
        {
            get
            {
                return m_logStatusUpdates;
            }
            set
            {
                m_logStatusUpdates = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum numbers of characters allowed in update status messages without getting suppressed from being displayed.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is negative or zero.</exception>
        [Category("Updates"),
        DefaultValue(DefaultMaxStatusUpdatesLength),
        Description("Maximum numbers of characters allowed in update status messages without getting suppressed from being displayed.")]
        public int MaxStatusUpdatesLength 
        {
            get
            {
                return m_maxStatusUpdatesLength;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be positive");

                m_maxStatusUpdatesLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of status update messages that can be issued in a second without getting suppressed from being displayed.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is negative or zero.</exception>
        [Category("Updates"),
        DefaultValue(DefaultMaxStatusUpdatesFrequency),
        Description("Maximum number of status update messages that can be issued in a second without getting suppressed from being displayed.")]
        public int MaxStatusUpdatesFrequency
        {
            get
            {
                return m_maxStatusUpdatesFrequency;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be positive");

                m_maxStatusUpdatesFrequency = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the health of the <see cref="ParentService"/> is to be monitored.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultMonitorServiceHealth),
        Description("Indicates whether the health of the ParentService is to be monitored.")]
        public bool MonitorServiceHealth
        {
            get
            {
                return m_monitorServiceHealth;
            }
            set
            {
                m_monitorServiceHealth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of <see cref="ClientRequest"/> entries to be maintained in the <see cref="ClientRequestHistory"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is zero or negative.</exception>
        [Category("Settings"),
        DefaultValue(DefaultRequestHistoryLimit),
        Description("Maximum number of ClientRequest entries to be maintained in the ClientRequestHistory.")]
        public int RequestHistoryLimit
        {
            get
            {
                return m_requestHistoryLimit;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("RequestHistoryLimit", "Value must be greater that 0");

                m_requestHistoryLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ServiceHelper"/> will have support for remote telnet-like sessions.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultSupportTelnetSessions),
        Description("Indicates whether the ServiceHelper will have support for remote telnet-like sessions.")]
        public bool SupportTelnetSessions
        {
            get
            {
                return m_supportTelnetSessions;
            }
            set
            {
                m_supportTelnetSessions = value;
            }
        }

        /// <summary>
        /// Gets or sets a comma or semicolon delimited list of user logins allowed to connect to the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <remarks>Use '*' to allow access to any remote user.</remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Security"),
        DefaultValue(DefaultAllowedRemoteUsers),
        Description("Comma or semicolon delimited list of user logins allowed to connect to the ServiceHelper.")]
        public string AllowedRemoteUsers
        {
            get
            {
                return m_allowedRemoteUsers;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("AllowedRemoteUsers");

                m_allowedRemoteUsers = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether remote commands are to be executed under the identity of the remote user.
        /// </summary>
        [Category("Security"),
        DefaultValue(DefaultImpersonateRemoteUser),
        Description("Indicates whether remote commands are to be executed under the identity of the remote user.")]
        public bool ImpersonateRemoteUser
        {
            get
            {
                return m_impersonateRemoteUser;
            }
            set
            {
                m_impersonateRemoteUser = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ServiceHelper"/> are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of ServiceHelper are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of <see cref="ServiceHelper"/> are to be saved to the config file 
        /// if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of ServiceHelper are to be saved to the config file if the PersistSettings property is set to true.")]
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
        /// Gets or sets the <see cref="ServiceBase"/> to which the <see cref="ServiceHelper"/> will provided added functionality.
        /// </summary>
        [Category("Components"),
        Description("ServiceBase to which the ServiceHelper will provided added functionality.")]
        public ServiceBase ParentService
        {
            get
            {
                return m_parentService;
            }
            set
            {
                m_parentService = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ServerBase"/> component used for communicating with <see cref="RemoteClients"/>.
        /// </summary>
        [Category("Components"),
        Description("ServerBase component used for communicating with RemoteClients.")]
        public ServerBase RemotingServer
        {
            get
            {
                return m_remotingServer;
            }
            set
            {
                if (m_remotingServer != null)
                {
                    // Detach events from any existing instance
                    m_remotingServer.ClientDisconnected -= RemotingServer_ClientDisconnected;
                    m_remotingServer.ReceiveClientDataComplete -= RemotingServer_ReceiveClientDataComplete;
                }

                m_remotingServer = value;

                if (m_remotingServer != null)
                {
                    // Attach events to new instance
                    m_remotingServer.ClientDisconnected += RemotingServer_ClientDisconnected;
                    m_remotingServer.ReceiveClientDataComplete += RemotingServer_ReceiveClientDataComplete;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ScheduleManager"/> component used for scheduling defined <see cref="ServiceProcess"/>.
        /// </summary>
        [Category("Components"),
        Description("ScheduleManager component used for scheduling defined ServiceProcess."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ScheduleManager ProcessScheduler
        {
            get
            {
                return m_processScheduler;
            }
        }

        /// <summary>
        /// Gets the <see cref="LogFile"/> component used for logging status messages to a text file.
        /// </summary>
        [Category("Components"),
        Description("LogFile component used for logging status messages to a text file."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LogFile StatusLog
        {
            get
            {
                return m_statusLog;
            }
        }

        /// <summary>
        /// Gets the <see cref="ErrorLogger"/> component used for logging errors encountered in the <see cref="ParentService"/>.
        /// </summary>
        [Category("Components"),
        Description("ErrorLogger component used for logging errors encountered in the ParentService."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ErrorLogger ErrorLogger
        {
            get
            {
                return m_errorLogger;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ServiceHelper"/> is currently enabled.
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
                return m_enabled;
            }
            set
            {
                if (value)
                {
                    // Re-enable all service components.
                    bool state;
                    ISupportLifecycle typedComponent;
                    foreach (object component in m_serviceComponents)
                    {
                        typedComponent = component as ISupportLifecycle;
                        if (typedComponent != null)
                        {
                            // Restore previous state.
                            if (m_componentEnabledStates.TryGetValue(typedComponent, out state))
                                typedComponent.Enabled = state;
                        }
                    }
                }
                else
                {
                    // Disable all service components.
                    m_componentEnabledStates.Clear();
                    ISupportLifecycle typedComponent;
                    foreach (object component in m_serviceComponents)
                    {
                        typedComponent = component as ISupportLifecycle;
                        if (typedComponent != null)
                        {
                            // Save current state.
                            m_componentEnabledStates.Add(typedComponent, typedComponent.Enabled);
                            typedComponent.Enabled = false;
                        }
                    }
                }

                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets a list of components that implement the <see cref="ISupportLifecycle"/> interface used by the <see cref="ParentService"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<object> ServiceComponents
        {
            get
            {
                return m_serviceComponents;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ServiceProcess"/> defined in the <see cref="ServiceHelper"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ServiceProcess> Processes
        {
            get
            {
                return m_processes;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ClientInfo"/> for remote clients connected to the <see cref="RemotingServer"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ClientInfo> RemoteClients
        {
            get
            {
                return m_remoteClients;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ClientRequestInfo"/> for requests made by <see cref="RemoteClients"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ClientRequestInfo> ClientRequestHistory
        {
            get
            {
                return m_clientRequestHistory;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ClientRequestHandler"/> registered for handling requests from <see cref="RemoteClients"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ClientRequestHandler> ClientRequestHandlers
        {
            get
            {
                return m_clientRequestHandlers;
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceMonitor"/> object used for monitoring the health of the <see cref="ParentService"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public PerformanceMonitor PerformanceMonitor
        {
            get
            {
                return m_performanceMonitor;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="ServiceHelper"/>.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                if (m_parentService == null)
                    return m_settingsCategory;
                else
                    return m_parentService.ServiceName;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="ServiceHelper"/>.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                // Show the uptime for the windows service.
                if (m_remotingServer != null)
                {
                    status.AppendFormat("System Uptime: {0}", m_remotingServer.RunTime.ToString());
                    status.AppendLine();
                    status.AppendLine();
                }

                // Show the status of registered components.
                status.AppendFormat("Status of components used by {0}:", Name);
                status.AppendLine();
                status.AppendLine();
                IProvideStatus typedComponent;
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IProvideStatus;
                    if (typedComponent != null)
                    {
                        // This component provides status information.                       
                        status.AppendFormat("Status of {0}:", typedComponent.Name);
                        status.AppendLine();
                        status.Append(typedComponent.Status);
                        status.AppendLine();
                    }
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ServiceHelper"/> 
        /// object is not consumed through the designer surface of the IDE.
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
        /// Performs necessary operations before the <see cref="ServiceHelper"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ServiceHelper"/> is consumed through the designer surface of the IDE.
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
        /// Performs necessary operations after the <see cref="ServiceHelper"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ServiceHelper"/> is consumed through the designer surface of the IDE.
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
        /// Saves settings of the <see cref="ServiceHelper"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
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
                element = settings["LogStatusUpdates", true];
                element.Update(m_logStatusUpdates, element.Description, element.Encrypted);
                element = settings["MaxStatusUpdatesLength", true];
                element.Update(m_maxStatusUpdatesLength, element.Description, element.Encrypted);
                element = settings["MaxStatusUpdatesFrequency", true];
                element.Update(m_maxStatusUpdatesFrequency, element.Description, element.Encrypted);
                element = settings["MonitorServiceHealth", true];
                element.Update(m_monitorServiceHealth, element.Description, element.Encrypted);
                element = settings["RequestHistoryLimit", true];
                element.Update(m_requestHistoryLimit, element.Description, element.Encrypted);
                element = settings["SupportTelnetSessions", true];
                element.Update(m_supportTelnetSessions, element.Description, element.Encrypted);
                element = settings["AllowedRemoteUsers", true];
                element.Update(m_allowedRemoteUsers, element.Description, element.Encrypted);
                element = settings["ImpersonateRemoteUser", true];
                element.Update(m_impersonateRemoteUser, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Saves settings of the <see cref="ServiceHelper"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <param name="includeServiceComponents">A boolean value that indicates whether the settings of <see cref="ServiceComponents"/> are to be saved.</param>
        public void SaveSettings(bool includeServiceComponents)
        {
            SaveSettings();
            if (includeServiceComponents)
            {
                IPersistSettings typedComponent;
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IPersistSettings;
                    if (typedComponent != null)
                        typedComponent.SaveSettings();
                }
            }
        }

        /// <summary>
        /// Loads saved settings of the <see cref="ServiceHelper"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
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
                settings.Add("LogStatusUpdates", m_logStatusUpdates, "True if status update messages are to be logged to a text file; otherwise False.");
                settings.Add("MaxStatusUpdatesLength", m_maxStatusUpdatesLength, "Maximum numbers of characters allowed in update status messages without getting suppressed from being displayed.");
                settings.Add("MaxStatusUpdatesFrequency", m_maxStatusUpdatesFrequency, "Maximum number of status update messages that can be issued in a second without getting suppressed from being displayed.");
                settings.Add("MonitorServiceHealth", m_monitorServiceHealth, "True if the service health is to be monitored; otherwise False.");
                settings.Add("RequestHistoryLimit", m_requestHistoryLimit, "Number of client request entries to be kept in the history.");
                settings.Add("SupportTelnetSessions", m_supportTelnetSessions, "True to enable the support for remote telnet-like sessions; otherwise False.");
                settings.Add("AllowedRemoteUsers", m_allowedRemoteUsers, "Comma or semicolon delimited list of user logins allowed to connect to the service remotely.");
                settings.Add("ImpersonateRemoteUser", m_impersonateRemoteUser, "True to execute remote commands under the identity of the remote user; otherwise False.");
                if (settings["TelnetSessionPassword"] != null)
                    m_telnetSessionPassword = settings["TelnetSessionPassword"].ValueAs(m_telnetSessionPassword);
                LogStatusUpdates = settings["LogStatusUpdates"].ValueAs(m_logStatusUpdates);
                MaxStatusUpdatesLength = settings["MaxStatusUpdatesLength"].ValueAs(m_maxStatusUpdatesLength);
                MaxStatusUpdatesFrequency = settings["MaxStatusUpdatesFrequency"].ValueAs(m_maxStatusUpdatesFrequency);
                MonitorServiceHealth = settings["MonitorServiceHealth"].ValueAs(m_monitorServiceHealth);
                RequestHistoryLimit = settings["RequestHistoryLimit"].ValueAs(m_requestHistoryLimit);
                SupportTelnetSessions = settings["SupportTelnetSessions"].ValueAs(m_supportTelnetSessions);
                AllowedRemoteUsers = settings["AllowedRemoteUsers"].ValueAs(m_allowedRemoteUsers);
                ImpersonateRemoteUser = settings["ImpersonateRemoteUser"].ValueAs(m_impersonateRemoteUser);
            }
        }

        /// <summary>
        /// Loads saved settings of the <see cref="ServiceHelper"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <param name="includeServiceComponents">A boolean value that indicates whether the settings of <see cref="ServiceComponents"/> are to be loaded.</param>
        public void LoadSettings(bool includeServiceComponents)
        {
            LoadSettings();
            if (includeServiceComponents)
            {
                IPersistSettings typedComponent;
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IPersistSettings;
                    if (typedComponent != null)
                        typedComponent.LoadSettings();
                }
            }
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnStart(string[])"/> method of <see cref="ParentService"/>.
        /// </summary>
        /// <param name="args">Array of type <see cref="String"/>.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnStart(string[] args)
        {
            // Ensure required components are present.
            if (m_parentService == null)
                throw new InvalidOperationException("ParentService property of ServiceHelper component is not set");

            if (m_remotingServer == null)
                throw new InvalidOperationException("RemotingServer property of ServiceHelper component is not set");

            OnServiceStarting(args);

            m_clientRequestHandlers.Add(new ClientRequestHandler("Clients", "Displays list of clients connected to the service", ShowClients));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Settings", "Displays queryable service settings from config file", ShowSettings));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Processes", "Displays list of service or system processes", ShowProcesses));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Schedules", "Displays list of process schedules defined in the service", ShowSchedules));
            m_clientRequestHandlers.Add(new ClientRequestHandler("History", "Displays list of requests received from the clients", ShowRequestHistory));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Help", "Displays list of commands supported by the service", ShowRequestHelp));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Status", "Displays the current service status", ShowServiceStatus));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Start", "Start a service or system process", StartProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Abort", "Aborts a service or system process", AbortProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("UpdateSettings", "Updates service setting in the config file", UpdateSettings));
            m_clientRequestHandlers.Add(new ClientRequestHandler("ReloadSettings", "Reloads services settings from the config file", ReloadSettings));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Reschedule", "Reschedules a process defined in the service", RescheduleProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Unschedule", "Unschedules a process defined in the service", UnscheduleProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("SaveSchedules", "Saves process schedules to the config file", SaveSchedules));
            m_clientRequestHandlers.Add(new ClientRequestHandler("LoadSchedules", "Loads process schedules from the config file", LoadSchedules));
            // Enable telnet support if requested.
            if (m_supportTelnetSessions)
            {
                m_clientRequestHandlers.Add(new ClientRequestHandler("Telnet", "Allows for a telnet session to the service server", RemoteTelnetSession, false));
            }
            // Enable health monitoring if requested.
            if (m_monitorServiceHealth)
            {
                m_performanceMonitor = new PerformanceMonitor();
                m_clientRequestHandlers.Add(new ClientRequestHandler("Health", "Displays a report of resource utilization for the service", ShowHealthReport));
            }

            // Add internal components as service components by default.
            m_serviceComponents.Add(m_processScheduler);
            m_serviceComponents.Add(m_statusLog);
            m_serviceComponents.Add(m_errorLogger);
            m_serviceComponents.Add(m_errorLogger.ErrorLog);
            m_serviceComponents.Add(m_remotingServer);
            m_serviceComponents.Add(m_statusUpdateQueue);

            // Open log file if file logging is enabled.
            if (m_logStatusUpdates)
                m_statusLog.Open();

            // Start all of the core components.
            m_statusUpdateQueue.Start();
            m_processScheduler.Start();
            m_remotingServer.Start();

            m_enabled = true;
            OnServiceStarted();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnStop()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnStop()
        {
            OnServiceStopping();

            // Abort any processes that may be currently executing.
            foreach (ServiceProcess process in m_processes)
            {
                if (process != null)
                    process.Abort();
            }

            m_enabled = false;          // Mark as disabled.
            m_suppressUpdates = true;   // Suppress status updates.

            OnServiceStopped();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnPause()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnPause()
        {
            OnServicePausing();

            // Disable all service components when service is pausing
            Enabled = false;

            OnServicePaused();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnContinue()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnResume()
        {
            OnServiceResuming();

            // Re-enable all service components when service is resuming
            Enabled = true;

            OnServiceResumed();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnShutdown()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnShutdown()
        {
            OnSystemShutdown();
            OnStop();
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        public void AddProcess(Action<string, object[]> processExecutionMethod, string processName)
        {
            AddProcess(processExecutionMethod, processName, null);
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processArguments">Arguments to be passed in to the <paramref name="processExecutionMethod"/> during execution.</param>
        public void AddProcess(Action<string, object[]> processExecutionMethod, string processName, object[] processArguments)
        {
            processName = processName.Trim();

            if (FindProcess(processName) == null)
            {
                ServiceProcess process = new ServiceProcess(processExecutionMethod, processName, processArguments);
                process.StateChanged += Process_StateChanged;

                m_processes.Add(process);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Process \"{0}\" is already defined", processName));
            }
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/> that executes on a schedule.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processSchedule"><see cref="Schedule"/> for the execution of the <see cref="ServiceProcess"/>.</param>
        public void AddScheduledProcess(Action<string, object[]> processExecutionMethod, string processName, string processSchedule)
        {
            AddScheduledProcess(processExecutionMethod, processName, null, processSchedule);
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/> that executes on a schedule.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processArguments">Arguments to be passed in to the <paramref name="processExecutionMethod"/> during execution.</param>
        /// <param name="processSchedule"><see cref="Schedule"/> for the execution of the <see cref="ServiceProcess"/>.</param>
        public void AddScheduledProcess(Action<string, object[]> processExecutionMethod, string processName, object[] processArguments, string processSchedule)
        {
            AddProcess(processExecutionMethod, processName, processArguments);
            ScheduleProcess(processName, processSchedule);
        }

        /// <summary>
        /// Schedules an existing <see cref="ServiceProcess"/> for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be scheduled.</param>
        /// <param name="scheduleRule">Rule that defines the execution pattern of the <see cref="ServiceProcess"/>.</param>
        public void ScheduleProcess(string processName, string scheduleRule)
        {
            ScheduleProcess(processName, scheduleRule, false);
        }

        /// <summary>
        /// Schedules an existing <see cref="ServiceProcess"/> for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be scheduled.</param>
        /// <param name="scheduleRule">Rule that defines the execution pattern of the <see cref="ServiceProcess"/>.</param>
        /// <param name="updateExistingSchedule">true if the <see cref="ServiceProcess"/> is to be re-scheduled; otherwise false.</param>
        public void ScheduleProcess(string processName, string scheduleRule, bool updateExistingSchedule)
        {
            processName = processName.Trim();

            if (FindProcess(processName) != null)
            {
                // The specified process exists, so we'll schedule it, or update its schedule if it is acheduled already.
                Schedule existingSchedule = m_processScheduler.FindSchedule(processName);

                if (existingSchedule != null)
                {
                    // Update the process schedule if it is already exists.
                    if (updateExistingSchedule)
                        existingSchedule.Rule = scheduleRule;
                }
                else
                {
                    // Schedule the process if it is not scheduled already.
                    m_processScheduler.Schedules.Add(new Schedule(processName, scheduleRule));
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("Process \"{0}\" is not defined", processName));
            }
        }

        /// <summary>
        /// Sends the specified <paramref name="response"/> to all <see cref="RemoteClients"/>.
        /// </summary>
        /// <param name="response">The <see cref="ServiceResponse"/> to be sent to all <see cref="RemoteClients"/>.</param>
        public void SendResponse(ServiceResponse response)
        {
            SendResponse(Guid.Empty, response);
        }

        /// <summary>
        /// Sends the specified <paramref name="response"/> to the specified <paramref name="client"/> only.
        /// </summary>
        /// <param name="client">ID of the client to whom the <paramref name="response"/> is to be sent.</param>
        /// <param name="response">The <see cref="ServiceResponse"/> to be sent to the <paramref name="client"/>.</param>
        public void SendResponse(Guid client, ServiceResponse response)
        {
            try
            {
                if (client == Guid.Empty)
                {
                    // Multicast message to all clients.
                    if (m_remoteCommandClientID == Guid.Empty)
                        // Process if remote command session is not in progress.
                        m_remotingServer.MulticastAsync(response);
                }
                else
                {
                    // Send message directly to specified client.
                    m_remotingServer.SendToAsync(client, response);
                }
            }
            catch (Exception ex)
            {
                // Log the exception.
                m_errorLogger.Log(ex);
            }
        }

        /// <summary>
        /// Provides a status update to all <see cref="RemoteClients"/>.
        /// </summary>
        /// <param name="message">Text message to be transmitted to all <see cref="RemoteClients"/>.</param>
        /// <param name="type">One of the <see cref="UpdateType"/> values.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        public void UpdateStatus(UpdateType type, string message, params object[] args)
        {
            UpdateStatus(Guid.Empty, type, message, args);
        }

        /// <summary>
        /// Provides a status update to the specified <paramref name="client"/>.
        /// </summary>
        /// <param name="client">ID of the client to whom the <paramref name="message"/> is to be sent.</param>
        /// <param name="type">One of the <see cref="UpdateType"/> values.</param>
        /// <param name="message">Text message to be transmitted to the <paramref name="client"/>.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        public void UpdateStatus(Guid client, UpdateType type, string message, params object[] args)
        {
            if (!m_suppressUpdates)
            {
                // Queue the status update for processing.
                m_statusUpdateQueue.Add(new StatusUpdate(client, type, string.Format(message, args)));
            }
        }

        /// <summary>
        /// Returns the <see cref="ServiceProcess"/> for the specified <paramref name="processName"/>.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be retrieved.</param>
        /// <returns><see cref="ServiceProcess"/> object if found; otherwise null.</returns>
        public ServiceProcess FindProcess(string processName)
        {
            return m_processes.Find(process => string.Compare(process.Name, processName, true) == 0);
        }

        /// <summary>
        /// Returns the <see cref="ClientInfo"/> object for the specified <paramref name="client"/>.
        /// </summary>
        /// <param name="clientID">ID of the client whose <see cref="ClientInfo"/> object is to be retrieved.</param>
        /// <returns><see cref="ClientInfo"/> object if found; otherwise null.</returns>
        public ClientInfo FindConnectedClient(Guid clientID)
        {
            return m_remoteClients.Find(clientInfo => clientInfo.ClientID == clientID);
        }

        /// <summary>
        /// Returns the <see cref="ClientRequestHandler"/> object for the specified <paramref name="requestType"/>.
        /// </summary>
        /// <param name="handlerCommand">Request type whose <see cref="ClientRequestHandler"/> object is to be retrieved.</param>
        /// <returns><see cref="ClientRequestHandler"/> object if found; otherwise null.</returns>
        public ClientRequestHandler FindClientRequestHandler(string handlerCommand)
        {
            return m_clientRequestHandlers.Find(handler => string.Compare(handler.Command, handlerCommand, true) == 0);
        }

        /// <summary>
        /// Raises the <see cref="ServiceStarting"/> event.
        /// </summary>
        /// <param name="args">Arguments to be sent to <see cref="ServiceStarting"/> event.</param>
        protected virtual void OnServiceStarting(string[] args)
        {
            // Notify service event consumers of pending service start
            if (ServiceStarting != null)
                ServiceStarting(this, new EventArgs<string[]>(args));
        }

        /// <summary>
        /// Raises the <see cref="ServiceStarted"/> event.
        /// </summary>
        protected virtual void OnServiceStarted()
        {
            // Notify all remote clients that might possibly be connected at of service start (not likely)
            SendServiceStateChangedResponse(ServiceState.Started);

            // Notify service event consumers that service has started
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceStopping"/> event.
        /// </summary>
        protected virtual void OnServiceStopping()
        {
            // Notify service event consumers of pending service stop
            if (ServiceStopping != null)
                ServiceStopping(this, EventArgs.Empty);

            // Notify all remote clients of service stop
            SendServiceStateChangedResponse(ServiceState.Stopped);
        }

        /// <summary>
        /// Raises the <see cref="ServiceStopped"/> event.
        /// </summary>
        protected virtual void OnServiceStopped()
        {
            // Notify service event consumers that service has stopped
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServicePausing"/> event.
        /// </summary>
        protected virtual void OnServicePausing()
        {
            // Notify service event consumers of pending service stop
            if (ServicePausing != null)
                ServicePausing(this, EventArgs.Empty);

            // Notify all remote clients of service pause
            SendServiceStateChangedResponse(ServiceState.Paused);
        }

        /// <summary>
        /// Raises the <see cref="ServicePaused"/> event.
        /// </summary>
        protected virtual void OnServicePaused()
        {
            // Notify service event consumers that service has been paused
            if (ServicePaused != null)
                ServicePaused(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceResuming"/> event.
        /// </summary>
        protected virtual void OnServiceResuming()
        {
            // Notify service event consumers of pending service resume
            if (ServiceResuming != null)
                ServiceResuming(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceResumed"/> event.
        /// </summary>
        protected virtual void OnServiceResumed()
        {
            // Notify all remote clients of service resume
            SendServiceStateChangedResponse(ServiceState.Resumed);

            // Notify service event consumers that service has been resumed
            if (ServiceResumed != null)
                ServiceResumed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SystemShutdown"/> event.
        /// </summary>
        protected virtual void OnSystemShutdown()
        {
            // Notify service event consumers of pending service shutdown
            SendServiceStateChangedResponse(ServiceState.Shutdown);

            // Notify service event consumers that service has shutdown
            if (SystemShutdown != null)
                SystemShutdown(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ReceivedClientRequest"/> event.
        /// </summary>
        /// <param name="request">The <see cref="ClientRequest"/> that was received.</param>
        /// <param name="requestSender">The <see cref="ClientInfo"/> object of the <paramref name="request"/> sender.</param>
        protected virtual void OnReceivedClientRequest(ClientRequest request, ClientInfo requestSender)
        {
            if (ReceivedClientRequest != null)
                ReceivedClientRequest(this, new EventArgs<Guid, ClientRequest>(requestSender.ClientID, request));
        }

        /// <summary>
        /// Raises the <see cref="ProcessStateChanged"/> event.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> whose state changed.</param>
        /// <param name="processState">New <see cref="ServiceProcessState"/> of the <see cref="ServiceProcess"/>.</param>
        protected virtual void OnProcessStateChanged(string processName, ServiceProcessState processState)
        {
            // Notify all service event consumer of change in process state
            if (ProcessStateChanged != null)
                ProcessStateChanged(this, new EventArgs<string, ServiceProcessState>(processName, processState));

            // Notify all remote clients of change in process state
            SendProcessStateChangedResponse(processName, processState);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ServiceHelper"/> object and optionally releases the managed resources.
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
                        SaveSettings();

                        if (m_statusLog != null)
                        {
                            m_statusLog.LogException -= StatusLog_LogException;
                            m_statusLog.Dispose();
                        }

                        if (m_processScheduler != null)
                        {
                            m_processScheduler.ScheduleDue -= Scheduler_ScheduleDue;
                            m_processScheduler.Dispose();
                        }

                        if (m_errorLogger != null)
                        {
                            m_errorLogger.LoggingException -= ErrorLogger_LoggingException;
                            m_errorLogger.Dispose();
                        }

                        if (m_performanceMonitor != null)
                        {
                            m_performanceMonitor.Dispose();
                        }

                        if (m_remoteCommandProcess != null)
                        {
                            m_remoteCommandProcess.ErrorDataReceived -= RemoteCommandProcess_ErrorDataReceived;
                            m_remoteCommandProcess.OutputDataReceived -= RemoteCommandProcess_OutputDataReceived;

                            if (!m_remoteCommandProcess.HasExited)
                                m_remoteCommandProcess.Kill();

                            m_remoteCommandProcess.Dispose();
                        }

                        if (m_statusUpdateQueue != null)
                        {
                            m_statusUpdateQueue.ProcessException -= StatusUpdateQueue_ProcessException;
                            m_statusUpdateQueue.Flush();
                            m_statusUpdateQueue.Dispose();
                        }

                        // Service processes are created and owned by remoting server, so we dispose them
                        if (m_processes != null)
                        {
                            foreach (ServiceProcess process in m_processes)
                            {
                                process.StateChanged -= Process_StateChanged;
                                process.Dispose();
                            }

                            m_processes.Clear();
                        }

                        // Detach any remoting server events, we don't own this component so we don't dispose it
                        RemotingServer = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        private void ProcessStatusUpdates(StatusUpdate[] items)
        {
            bool displayUpdate = false;
            int suppressedUpdates = 0;
            foreach (StatusUpdate item in items)
            {
                if ((DateTime.Now.Ticks - m_lastStatusUpdateTime).ToSeconds() > 1)
                {
                    displayUpdate = true;
                    m_statusUpdatesDisplayed = 1;
                    m_lastStatusUpdateTime = DateTime.Now.Ticks;
                }
                else
                {
                    displayUpdate = m_statusUpdatesDisplayed <= m_maxStatusUpdatesFrequency;
                    m_statusUpdatesDisplayed++;
                }

                // Send the status update to specified client(s).
                if (displayUpdate)
                {
                    if (item.Message.Length <= m_maxStatusUpdatesLength)
                    {
                        SendUpdateClientStatusResponse(item.Client, item.Type, item.Message);
                    }
                    else
                    {
                        SendUpdateClientStatusResponse(item.Client, item.Type, item.Message.TruncateRight(m_maxStatusUpdatesLength));
                        SendUpdateClientStatusResponse(item.Client, UpdateType.Warning, string.Format("\r\nSuppressed {0:N0} status update character(s) from being displayed to avoid flooding.\r\n\r\n", item.Message.Length - m_maxStatusUpdatesLength));
                    }
                }
                else
                {
                    suppressedUpdates++;
                }

                // Log the status update to the log file if logging is enabled.
                if (m_logStatusUpdates)
                    m_statusLog.WriteTimestampedLine(item.Message);
            }

            if (suppressedUpdates > 0)
                SendUpdateClientStatusResponse(Guid.Empty, UpdateType.Warning, string.Format("\r\nSuppressed {0:N0} status update(s) from being displayed to avoid flooding.\r\n\r\n", suppressedUpdates));
        }

        private void SendUpdateClientStatusResponse(Guid clientID, UpdateType type, string response)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            serviceResponse.Type = "UPDATECLIENTSTATUS-" + type.ToString().ToUpper();
            serviceResponse.Message = response;
            SendResponse(clientID, serviceResponse);
        }

        private void SendServiceStateChangedResponse(ServiceState currentState)
        {
            ServiceResponse serviceResponse = new ServiceResponse("SERVICESTATECHANGED");
            serviceResponse.Attachments.Add(new ObjectState<ServiceState>(Name, currentState));
            SendResponse(serviceResponse);
        }

        private void SendProcessStateChangedResponse(string processName, ServiceProcessState currentState)
        {
            ServiceResponse serviceResponse = new ServiceResponse("PROCESSSTATECHANGED");
            serviceResponse.Attachments.Add(new ObjectState<ServiceProcessState>(processName, currentState));
            SendResponse(serviceResponse);
        }

        private void Process_StateChanged(object sender, EventArgs e)
        {
            ServiceProcess process = sender as ServiceProcess;
            OnProcessStateChanged(process.Name, process.CurrentState);
        }

        private void Scheduler_ScheduleDue(object sender, EventArgs<Schedule> e)
        {
            ServiceProcess scheduledProcess = FindProcess(e.Argument.Name);

            // Start the process execution if it exists.
            if (scheduledProcess != null)
                scheduledProcess.Start();
        }

        private void StatusUpdateQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            m_errorLogger.Log(e.Argument);
        }

        private void StatusLog_LogException(object sender, EventArgs<System.Exception> e)
        {
            // We'll let the connected clients know that we encountered an exception while logging the status update.
            m_logStatusUpdates = false;
            UpdateStatus(UpdateType.Alarm, "Error occurred while logging status update - {0}\r\n\r\n", e.Argument.Message);
            m_logStatusUpdates = true;
        }

        private void ErrorLogger_LoggingException(object sender, EventArgs<Exception> e)
        {
            UpdateStatus(UpdateType.Alarm, "Error occurred while logging an error - {0}\r\n\r\n", e.Argument.Message);
        }

        private void RemotingServer_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            ClientInfo disconnectedClient = FindConnectedClient(e.Argument);

            if (disconnectedClient != null)
            {
                if (e.Argument == m_remoteCommandClientID)
                {
                    try
                    {
                        RemoteTelnetSession(new ClientRequestInfo(disconnectedClient, ClientRequest.Parse("Telnet -disconnect")));
                    }
                    catch (Exception ex)
                    {
                        // We'll encounter an exception because we'll try to update the status of the client that had the
                        // remote command session open and this will fail since the client is disconnected.
                        m_errorLogger.Log(ex);
                    }
                }

                m_remoteClients.Remove(disconnectedClient);
                UpdateStatus(UpdateType.Information, "Remote client disconnected - {0} from {1}.\r\n\r\n", disconnectedClient.UserName, disconnectedClient.MachineName);
            }
        }

        private void RemotingServer_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            // Set the thread's principal to the current principal.
            Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            ClientInfo requestSender = FindConnectedClient(e.Argument1);
            if (requestSender == null)
            {
                // First message from a remote client should be its info.
                ClientInfo client = null;
                Serialization.TryGetObject<ClientInfo>(e.Argument2.BlockCopy(0, e.Argument3), out client);
                try
                {
                    if (client != null)
                    {
                        // Check remote user against valid user list.
                        string[] allowedRemoteUsers = m_allowedRemoteUsers.Split(';', ',');
                        if (m_allowedRemoteUsers == "*" ||
                            (client.DeserializedIdentityToken != null &&
                             client.DeserializedIdentityToken.Identity.IsAuthenticated &&
                             allowedRemoteUsers.FirstOrDefault(user => string.Compare(user.Trim(), client.DeserializedIdentityToken.Identity.Name, true) == 0) != null))
                        {
                            // Authentication successful.
                            client.ClientID = e.Argument1;
                            client.ConnectedAt = DateTime.Now;
                            m_remoteClients.Add(client);
                            SendResponse(e.Argument1, new ServiceResponse("AuthenticationSuccess"));
                            UpdateStatus(UpdateType.Information, "Remote client connected - {0} from {1}.\r\n\r\n", client.UserName, client.MachineName);
                        }
                        else
                        {
                            // Authentication failed.
                            throw new SecurityException("Remote client failed authentication");
                        }
                    }
                    else
                    {
                        // Required client information is missing.
                        throw new SecurityException("Remote client failed to transmit required information");
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        SendResponse(e.Argument1, new ServiceResponse("AuthenticationFailure"));
                        UpdateStatus(UpdateType.Warning, "Remote client connection rejected - {0} from {1}.\r\n\r\n", client.UserName, client.MachineName);
                        m_errorLogger.Log(ex);
                        m_remotingServer.DisconnectOne(e.Argument1);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                // All subsequest messages from a remote client would be requests.
                ClientRequest request = null;
                WindowsImpersonationContext context = null;
                Serialization.TryGetObject<ClientRequest>(e.Argument2.BlockCopy(0, e.Argument3), out request);
                if (request != null)
                {
                    try
                    {
                        ClientRequestInfo requestInfo = new ClientRequestInfo(requestSender, request);

                        if (requestInfo.Sender.DeserializedIdentityToken != null)
                            // Set the thread's principal to the remote user's principal.
                            Thread.CurrentPrincipal = requestInfo.Sender.DeserializedIdentityToken.Principal;

                        if (m_remoteCommandClientID == Guid.Empty)
                        {
                            // Process incoming requests when remote command session is not in progress.
                            m_clientRequestHistory.Add(requestInfo);

                            // Remove old request entries if we've exceeded the limit for request history.
                            if (m_clientRequestHistory.Count > m_requestHistoryLimit)
                                m_clientRequestHistory.RemoveRange(0, (m_clientRequestHistory.Count - m_requestHistoryLimit));

                            // Impersonate remote user for increased security when impersonation is enabled.
                            if (m_impersonateRemoteUser)
                            {
                                if (((WindowsIdentity)Thread.CurrentPrincipal.Identity).ImpersonationLevel != TokenImpersonationLevel.Impersonation)
                                    throw new InvalidOperationException("Impersonation is not supported");

                                context = ((WindowsIdentity)Thread.CurrentPrincipal.Identity).Impersonate();
                            }

                            // Notify the consumer about the incoming request from client.
                            OnReceivedClientRequest(request, requestSender);

                            ClientRequestHandler requestHandler = FindClientRequestHandler(request.Command);
                            if (requestHandler != null)
                            {
                                // Request handler exists.
                                requestHandler.HandlerMethod(requestInfo);
                            }
                            else
                            {
                                // No request handler exists.
                                throw new InvalidOperationException("Request is not supported");
                            }
                        }
                        else if (requestSender.ClientID == m_remoteCommandClientID)
                        {
                            // Redirect requests to remote command session if requests are from its originator.
                            RemoteTelnetSession(requestInfo);
                        }
                        else
                        {
                            // Reject all request from other clients since remote command session is in progress.
                            throw new InvalidOperationException("Remote telnet session is in progress");
                        }
                    }
                    catch (Exception ex)
                    {
                        m_errorLogger.Log(ex);
                        UpdateStatus(requestSender.ClientID, UpdateType.Alarm, "Failed to process request \"{0}\" - {1}.\r\n\r\n", request.Command, ex.Message);
                    }
                    finally
                    {
                        if (context != null)
                            context.Undo();
                    }
                }
                else
                {
                    UpdateStatus(requestSender.ClientID, UpdateType.Alarm, "Failed to process request - Request could not be deserialized.\r\n\r\n");
                }
            }
        }

        private void RemoteCommandProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, UpdateType.Alarm, e.Data + "\r\n");
        }

        private void RemoteCommandProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, UpdateType.Information, e.Data + "\r\n");
        }

        #region [ Client Request Handlers ]

        private void ShowClients(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of clients currently connected to the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Clients -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                if (m_remoteClients.Count > 0)
                {
                    // Display info about all of the clients connected to the service.
                    StringBuilder responseMessage = new StringBuilder();

                    responseMessage.AppendFormat("Clients connected to {0}:", Name);
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();
                    responseMessage.Append("Client".PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append("Machine".PadRight(15));
                    responseMessage.Append(' ');
                    responseMessage.Append("User".PadRight(15));
                    responseMessage.Append(' ');
                    responseMessage.Append("Connected".PadRight(20));
                    responseMessage.AppendLine();
                    responseMessage.Append(new string('-', 25));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 15));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 15));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 20));

                    foreach (ClientInfo clientInfo in m_remoteClients)
                    {
                        responseMessage.AppendLine();

                        if (!string.IsNullOrEmpty(clientInfo.ClientName))
                            responseMessage.Append(clientInfo.ClientName.PadRight(25));
                        else
                            responseMessage.Append("[Not Available]".PadRight(25));

                        responseMessage.Append(' ');
                        if (!string.IsNullOrEmpty(clientInfo.MachineName))
                            responseMessage.Append(clientInfo.MachineName.PadRight(15));
                        else
                            responseMessage.Append("[Not Available]".PadRight(15));

                        responseMessage.Append(' ');
                        if (!string.IsNullOrEmpty(clientInfo.UserName))
                            responseMessage.Append(clientInfo.UserName.PadRight(15));
                        else
                            responseMessage.Append("[Not Available]".PadRight(15));

                        responseMessage.Append(' ');
                        responseMessage.Append(clientInfo.ConnectedAt.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                    }
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "No clients are connected to {0}\r\n\r\n", Name);
                }
            }
        }

        private void ShowSettings(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of service settings from the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Settings -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                StringBuilder responseMessage = new StringBuilder();
                responseMessage.AppendFormat("Settings for {0}:", Name);
                responseMessage.AppendLine();
                responseMessage.AppendLine();
                responseMessage.Append("Category".PadRight(20));
                responseMessage.Append(' ');
                responseMessage.Append("Name".PadRight(25));
                responseMessage.Append(' ');
                responseMessage.Append("Value".PadRight(30));
                responseMessage.AppendLine();
                responseMessage.Append(new string('-', 20));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 25));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 30));

                IPersistSettings typedComponent;
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IPersistSettings;
                    if (typedComponent != null)
                    {
                        foreach (CategorizedSettingsElement setting in ConfigurationFile.Current.Settings[typedComponent.SettingsCategory])
                        {
                            // Skip encrypted settings for security purpose.
                            if (setting.Encrypted)
                                continue;

                            responseMessage.AppendLine();
                            responseMessage.Append(typedComponent.SettingsCategory.PadRight(20));
                            responseMessage.Append(' ');
                            responseMessage.Append(setting.Name.PadRight(25));
                            responseMessage.Append(' ');

                            if (!string.IsNullOrEmpty(setting.Value))
                                responseMessage.Append(setting.Value.PadRight(30));
                            else
                                responseMessage.Append("[Not Set]".PadRight(30));
                        }
                    }
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
            }
        }

        private void ShowProcesses(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of defined service processes or running system processes.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Processes -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");

                if (showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Displays system processes instead of service processes");
                }
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool listSystemProcesses = requestInfo.Request.Arguments.Exists("system");

                if (!listSystemProcesses)
                {
                    if (m_processes.Count > 0)
                    {
                        // Display info about all the processes defined in the service.
                        StringBuilder responseMessage = new StringBuilder();

                        responseMessage.AppendFormat("Processes defined in {0}:", Name);
                        responseMessage.AppendLine();
                        responseMessage.AppendLine();
                        responseMessage.Append("Name".PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append("State".PadRight(15));
                        responseMessage.Append(' ');
                        responseMessage.Append("Last Exec. Start".PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append("Last Exec. Stop".PadRight(20));
                        responseMessage.AppendLine();
                        responseMessage.Append(new string('-', 20));
                        responseMessage.Append(' ');
                        responseMessage.Append(new string('-', 15));
                        responseMessage.Append(' ');
                        responseMessage.Append(new string('-', 20));
                        responseMessage.Append(' ');
                        responseMessage.Append(new string('-', 20));

                        foreach (ServiceProcess process in m_processes)
                        {
                            responseMessage.AppendLine();
                            responseMessage.Append(process.Name.PadRight(20));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.CurrentState.ToString().PadRight(15));
                            responseMessage.Append(' ');

                            if (process.ExecutionStartTime != DateTime.MinValue)
                                responseMessage.Append(process.ExecutionStartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                            else
                                responseMessage.Append("[Not Executed]".PadRight(20));

                            responseMessage.Append(' ');

                            if (process.ExecutionStopTime != DateTime.MinValue)
                            {
                                responseMessage.Append(process.ExecutionStopTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                            }
                            else
                            {
                                if (process.ExecutionStartTime != DateTime.MinValue)
                                    responseMessage.Append("[Executing]".PadRight(20));
                                else
                                    responseMessage.Append("[Not Executed]".PadRight(20));
                            }
                        }
                        responseMessage.AppendLine();
                        responseMessage.AppendLine();

                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                    }
                    else
                    {
                        // No processes defined in the service to be displayed.
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "No processes are defined in {0}.\r\n\r\n", Name);
                    }
                }
                else
                {
                    // We enumerate "system" processes when -system parameter is specified
                    StringBuilder responseMessage = new StringBuilder();

                    responseMessage.AppendFormat("Processes running on {0}:", Environment.MachineName);
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();
                    responseMessage.Append("ID".PadRight(5));
                    responseMessage.Append(' ');
                    responseMessage.Append("Name".PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append("Priority".PadRight(15));
                    responseMessage.Append(' ');
                    responseMessage.Append("Responding".PadRight(10));
                    responseMessage.Append(' ');
                    responseMessage.Append("Start Time".PadRight(20));
                    responseMessage.AppendLine();
                    responseMessage.Append(new string('-', 5));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 25));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 15));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 10));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 20));

                    foreach (Process process in Process.GetProcesses())
                    {
                        try
                        {
                            responseMessage.Append(process.StartInfo.UserName);
                            responseMessage.AppendLine();
                            responseMessage.Append(process.Id.ToString().PadRight(5));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.ProcessName.PadRight(25));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.PriorityClass.ToString().PadRight(15));
                            responseMessage.Append(' ');
                            responseMessage.Append((process.Responding ? "Yes" : "No").PadRight(10));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.StartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                        }
                        catch
                        {
                        }
                    }
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                }
            }
        }

        private void ShowSchedules(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of schedules for processes defined in the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Schedules -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                if (m_processScheduler.Schedules.Count > 0)
                {
                    // Display info about all the process schedules defined in the service.
                    StringBuilder responseMessage = new StringBuilder();

                    responseMessage.AppendFormat("Process schedules defined in {0}:", Name);
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();
                    responseMessage.Append("Name".PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append("Rule".PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append("Last Due".PadRight(30));
                    responseMessage.AppendLine();
                    responseMessage.Append(new string('-', 20));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 20));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 30));

                    foreach (Schedule schedule in m_processScheduler.Schedules)
                    {
                        responseMessage.AppendLine();
                        responseMessage.Append(schedule.Name.PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append(schedule.Rule.PadRight(20));
                        responseMessage.Append(' ');

                        if (schedule.LastDueAt != DateTime.MinValue)
                            responseMessage.Append(schedule.LastDueAt.ToString().PadRight(30));
                        else
                            responseMessage.Append("[Never]".PadRight(30));
                    }
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "No process schedules are defined in {0}.\r\n\r\n", Name);
                }
            }
        }

        private void ShowRequestHistory(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of recent requests received from the clients.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       History -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                StringBuilder responseMessage = new StringBuilder();

                responseMessage.AppendFormat("History of requests received by {0}:", Name);
                responseMessage.AppendLine();
                responseMessage.AppendLine();
                responseMessage.Append("Command".PadRight(20));
                responseMessage.Append(' ');
                responseMessage.Append("Received".PadRight(25));
                responseMessage.Append(' ');
                responseMessage.Append("Sender".PadRight(30));
                responseMessage.AppendLine();
                responseMessage.Append(new string('-', 20));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 25));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 30));

                foreach (ClientRequestInfo historicRequest in m_clientRequestHistory)
                {
                    responseMessage.AppendLine();
                    responseMessage.Append(historicRequest.Request.Command.PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append(historicRequest.ReceivedAt.ToString().PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append(string.Format("{0} from {1}", historicRequest.Sender.UserName, historicRequest.Sender.MachineName).PadRight(30));
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
            }
        }

        private void ShowRequestHelp(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of commands supported by the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Help -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder responseMessage = new StringBuilder();

                responseMessage.AppendFormat("Commands supported by {0}:", Name);
                responseMessage.AppendLine();
                responseMessage.AppendLine();
                responseMessage.Append("Command".PadRight(20));
                responseMessage.Append(' ');
                responseMessage.Append("Description".PadRight(55));
                responseMessage.AppendLine();
                responseMessage.Append(new string('-', 20));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 55));

                foreach (ClientRequestHandler handler in m_clientRequestHandlers)
                {
                    if (handler.IsAdvertised || showAdvancedHelp)
                    {
                        responseMessage.AppendLine();
                        responseMessage.Append(handler.Command.PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append(handler.CommandDescription.PadRight(55));
                    }
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
            }
        }

        private void ShowHealthReport(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a resource utilization report for the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Health -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                if (m_performanceMonitor != null)
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, m_performanceMonitor.Status + "\r\n");
                else
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Warning, "Performance monitor is not available.");
            }
        }

        private void ShowServiceStatus(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays status of this service and its components.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Status -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, Status);
            }
        }

        private void ReloadSettings(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Reloads settings of the component whose settings are saved under the specified category in the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ReloadSettings \"Category Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("IMPORTANT: Only settings under the categories listed by the \"Settings\" command can be reloaded.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string categoryName = requestInfo.Request.Arguments["orderedarg1"];

                IPersistSettings typedComponent;
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IPersistSettings;
                    if (typedComponent != null &&
                        string.Compare(categoryName, typedComponent.SettingsCategory, true) == 0)
                    {
                        typedComponent.LoadSettings();
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully loaded settings from category \"{0}\".\r\n\r\n", categoryName);
                        return;
                    }
                }

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to load settings from category \"{0}\". No corresponding component exists.\r\n\r\n", categoryName);
            }
        }

        private void UpdateSettings(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 3)
            {
                // We'll display help about the request since we either don't have the required arguments or the user
                // has explicitly requested for the help to be displayed for this request type.
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Updates the specified setting under the specified category in the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       UpdateSettings \"Category Name\" \"Setting Name\" \"Setting Value\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -add".PadRight(20));
                helpMessage.Append("Adds specified setting to the specified category");
                helpMessage.AppendLine();
                helpMessage.Append("       -delete".PadRight(20));
                helpMessage.Append("Deletes specified setting from the specified category");
                helpMessage.AppendLine();
                helpMessage.Append("       -reload".PadRight(20));
                helpMessage.Append("Causes corresponding component to reload settings");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list all of the queryable settings");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("IMPORTANT: Only settings under the categories listed by the \"Settings\" command can be updated.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string categoryName = requestInfo.Request.Arguments["orderedarg1"];
                string settingName = requestInfo.Request.Arguments["orderedarg2"];
                string settingValue = requestInfo.Request.Arguments["orderedarg3"];
                bool addSetting = requestInfo.Request.Arguments.Exists("add");
                bool deleteSetting = requestInfo.Request.Arguments.Exists("delete");
                bool reloadSettings = requestInfo.Request.Arguments.Exists("reload");
                bool listSettings = requestInfo.Request.Arguments.Exists("list");

                IPersistSettings typedComponent;
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IPersistSettings;
                    if (typedComponent != null &&
                        string.Compare(categoryName, typedComponent.SettingsCategory, true) == 0)
                    {
                        ConfigurationFile config = ConfigurationFile.Current;
                        CategorizedSettingsElementCollection settings = config.Settings[categoryName];
                        CategorizedSettingsElement setting = settings[settingName];
                        if (addSetting)
                        {
                            // Add new setting.
                            if (setting == null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to add setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                settings.Add(settingName, settingValue);
                                config.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully added setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to add setting \"{0}\" under category \"{1}\". Setting already exists.\r\n\r\n", settingName, categoryName);
                                return;
                            }
                        }
                        else if (deleteSetting)
                        {
                            // Delete existing setting.
                            if (setting != null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to delete setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                settings.Remove(setting);
                                config.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully deleted setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to delete setting \"{0}\" under category \"{1}\". Setting does not exist.\r\n\r\n", settingName, categoryName);
                                return;
                            }
                        }
                        else
                        {
                            // Update existing setting.
                            if (setting != null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to update setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                setting.Value = settingValue;
                                config.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully updated setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to update value of setting \"{0}\" under category \"{1}\" . Setting does not exist.\r\n\r\n", settingName, categoryName);
                                return;
                            }
                        }

                        if (reloadSettings)
                        {
                            // The user has requested to reload settings for all the components.
                            requestInfo.Request = ClientRequest.Parse(string.Format("ReloadSettings {0}", categoryName));
                            ReloadSettings(requestInfo);
                        }

                        if (listSettings)
                        {
                            // The user has requested to list all of the queryable settings.
                            requestInfo.Request = ClientRequest.Parse("Settings");
                            ShowSettings(requestInfo);
                        }

                        return;
                    }
                }

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to update settings under category \"{0}\". No corresponding component exists.\r\n\r\n", categoryName);
            }
        }

        private void StartProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Starts execution of the specified service or system process.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Start \"Process Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -args".PadRight(20));
                helpMessage.Append("Arguments to be passed in to the process");
                helpMessage.AppendLine();
                helpMessage.Append("       -restart".PadRight(20));
                helpMessage.Append("Aborts the process if executing and start it again");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all service or system processes");
                if (showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Treats the specified process as a system process");
                }
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                string processArgs = requestInfo.Request.Arguments["args"];
                bool systemProcess = requestInfo.Request.Arguments.Exists("system");
                bool restartProcess = requestInfo.Request.Arguments.Exists("restart");
                bool listProcesses = requestInfo.Request.Arguments.Exists("list");

                if (restartProcess)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Abort \"{0}\" {1}", processName, systemProcess ? "-system" : ""));
                    AbortProcess(requestInfo);
                }

                if (!systemProcess)
                {
                    ServiceProcess processToStart = FindProcess(processName);

                    if (processToStart != null)
                    {
                        if (processToStart.CurrentState != ServiceProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to start service process \"{0}\"...\r\n\r\n", processName);
                            if (string.IsNullOrEmpty(processArgs))
                            {
                                processToStart.Start();
                            }
                            else
                            {
                                // Prepare the arguments.
                                string[] splitArgs = processArgs.Split(',');
                                Array.ForEach<string>(splitArgs, (string arg) => arg.Trim());

                                // Start the service process.
                                processToStart.Start(splitArgs);
                            }
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully started service process \"{0}\".\r\n\r\n", processName);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start process \"{0}\". Process is already executing.\r\n\r\n", processName);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start service process \"{0}\". Process is not defined.\r\n\r\n", processName);
                    }
                }
                else
                {
                    try
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to start system process \"{0}\"...\r\n\r\n", processName);
                        Process startedProcess = Process.Start(processName, processArgs);

                        if (startedProcess != null)
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully started system process \"{0}\".\r\n\r\n", processName);
                        else
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start system process \"{0}\".\r\n\r\n", processName);
                    }
                    catch (Exception ex)
                    {
                        m_errorLogger.Log(ex);
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start system process \"{0}\". {1}.\r\n\r\n", processName, ex.Message);
                    }
                }

                if (listProcesses)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                    ShowProcesses(requestInfo);
                }
            }
        }

        private void AbortProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Aborts the specified service or system process if executing.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Abort \"Process Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all service or system processes");

                if (showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Treats the specified process as a system process");
                    helpMessage.AppendLine();
                    helpMessage.AppendLine();
                    helpMessage.Append("NOTE: Specify process name of \"Me\" to kill current service process. ");
                }
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool systemProcess = requestInfo.Request.Arguments.Exists("system");
                bool listProcesses = requestInfo.Request.Arguments.Exists("list");

                if (!systemProcess)
                {
                    ServiceProcess processToAbort = FindProcess(processName);

                    if (processToAbort != null)
                    {
                        if (processToAbort.CurrentState == ServiceProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to abort service process \"{0}\"...\r\n\r\n", processName);
                            processToAbort.Abort();
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully aborted service process \"{0}\".\r\n\r\n", processName);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort service process \"{0}\". Process is not executing.\r\n\r\n", processName);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort service process \"{0}\". Process is not defined.\r\n\r\n", processName);
                    }
                }
                else
                {
                    Process processToAbort = null;

                    if (string.Compare(processName, "Me", true) == 0)
                    {
                        processName = Process.GetCurrentProcess().ProcessName;
                    }

                    foreach (Process process in Process.GetProcessesByName(processName))
                    {
                        // Lookup for the system process by name.
                        processToAbort = process;
                        break;
                    }

                    if (processToAbort == null)
                    {
                        int processID;

                        if (int.TryParse(processName, out processID) && processID > 0)
                        {
                            processToAbort = Process.GetProcessById(processID);
                            processName = processToAbort.ProcessName;
                        }
                    }

                    if (processToAbort != null)
                    {
                        try
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to abort system process \"{0}\"...\r\n\r\n", processName);
                            processToAbort.Kill();
                            if (processToAbort.WaitForExit(10000))
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully aborted system process \"{0}\".\r\n\r\n", processName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort system process \"{0}\". Process not responding.\r\n\r\n", processName);
                            }
                        }
                        catch (Exception ex)
                        {
                            m_errorLogger.Log(ex);
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort system process \"{0}\". {1}.\r\n\r\n", processName, ex.Message);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort system process \"{0}\". Process is not running.\r\n\r\n", processName);
                    }
                }

                if (listProcesses)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                    ShowProcesses(requestInfo);
                }
            }
        }

        private void RescheduleProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 2)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Schedules or re-schedules an existing process defined in the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Reschedule \"Process Name\" \"Schedule Rule\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -save".PadRight(20));
                helpMessage.Append("Saves all process schedules to the config file");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("NOTE: The schedule rule uses UNIX crontab syntax which consists of 5 parts (For example, \"* * * * *\"). ");
                helpMessage.Append("Following is a brief description of each of the 5 parts that make up the rule:");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Part 1 - Minute part; value range 0 to 59. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 2 - Hour part; value range 0 to 23. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 3 - Day of month part; value range 1 to 31. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 4 - Month part; value range 1 to 12. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 5 - Day of week part; value range 0 to 6 (0 = Sunday). ");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("Following is a description of valid syntax for all parts of the rule:");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   *       - Any value in the range for the date-time part.");
                helpMessage.AppendLine();
                helpMessage.Append("   */n     - Every nth value for the data-time part.");
                helpMessage.AppendLine();
                helpMessage.Append("   n1-n2   - Range of values (inclusive) for the date-time part.");
                helpMessage.AppendLine();
                helpMessage.Append("   n1,n2   - 1 or more specific values for the date-time part.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("Examples:");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   \"* * * * *\"       - Process executes every minute.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"*/5 * * * *\"     - Process executes every 5 minutes.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"5 * * * *\"       - Process executes 5 past every hour.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 * * *\"       - Process executes every day at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 1 * *\"       - Process executes 1st of every month at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 * * 0\"       - Process executes every Sunday at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 31 12 *\"     - Process executes on December 31 at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"5,10 0-2 * * *\"  - Process executes 5 and 10 past hours 12am to 2am.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                string scheduleRule = requestInfo.Request.Arguments["orderedarg2"];
                bool saveSchedules = requestInfo.Request.Arguments.Exists("save");
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                try
                {
                    // Schedule the process if not scheduled or update its schedule if scheduled.
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to schedule process \"{0}\" with rule \"{1}\"...\r\n\r\n", processName, scheduleRule);
                    ScheduleProcess(processName, scheduleRule, true);
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully scheduled process \"{0}\" with rule \"{1}\".\r\n\r\n", processName, scheduleRule);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                catch (Exception ex)
                {
                    m_errorLogger.Log(ex);
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to schedule process \"{0}\". {1}\r\n\r\n", processName, ex.Message);
                }

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void UnscheduleProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Unschedules a scheduled process defined in the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Unschedule \"Process Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -save".PadRight(20));
                helpMessage.Append("Saves all process schedules to the config file");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool saveSchedules = requestInfo.Request.Arguments.Exists("save");
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                Schedule scheduleToRemove = m_processScheduler.FindSchedule(processName);

                if (scheduleToRemove != null)
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to unschedule process \"{0}\"...\r\n\r\n", processName);
                    m_processScheduler.Schedules.Remove(scheduleToRemove);
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully unscheduled process \"{0}\".\r\n\r\n", processName);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to unschedule process \"{0}\". Process is not scheduled.\r\n\r\n", processName);
                }

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void SaveSchedules(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Saves all process schedules to the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       SaveSchedules -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to save process schedules to the config file...\r\n\r\n");
                m_processScheduler.SaveSettings();
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully saved process schedules to the config file.\r\n\r\n");

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void LoadSchedules(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Loads all process schedules from the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       LoadSchedules -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to load process schedules from the config file...\r\n\r\n");
                m_processScheduler.LoadSettings();
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully loaded process schedules from the config file.\r\n\r\n");

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void RemoteTelnetSession(ClientRequestInfo requestinfo)
        {
            if (m_remoteCommandProcess == null && requestinfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Allows for a telnet session to the service server.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Telnet -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -connect".PadRight(20));
                helpMessage.Append("Establishes a telnet session (requires password)");
                helpMessage.AppendLine();
                helpMessage.Append("       -disconnect".PadRight(20));
                helpMessage.Append("Terminates established telnet session");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestinfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool connectSession = requestinfo.Request.Arguments.Exists("connect");
                bool disconnectSession = requestinfo.Request.Arguments.Exists("disconnect");

                if (m_remoteCommandProcess == null && connectSession && !string.IsNullOrEmpty(requestinfo.Request.Arguments["connect"]))
                {
                    // User wants to establish a remote command session.
                    string password = requestinfo.Request.Arguments["connect"];

                    if (password == m_telnetSessionPassword)
                    {
                        // Establish remote command session
                        m_remoteCommandProcess = new Process();
                        m_remoteCommandProcess.ErrorDataReceived += RemoteCommandProcess_ErrorDataReceived;
                        m_remoteCommandProcess.OutputDataReceived += RemoteCommandProcess_OutputDataReceived;
                        m_remoteCommandProcess.StartInfo.FileName = "cmd.exe";
                        m_remoteCommandProcess.StartInfo.UseShellExecute = false;
                        m_remoteCommandProcess.StartInfo.RedirectStandardInput = true;
                        m_remoteCommandProcess.StartInfo.RedirectStandardOutput = true;
                        m_remoteCommandProcess.StartInfo.RedirectStandardError = true;
                        m_remoteCommandProcess.Start();
                        m_remoteCommandProcess.BeginOutputReadLine();
                        m_remoteCommandProcess.BeginErrorReadLine();

                        UpdateStatus(UpdateType.Information, "Remote command session established - status updates are suspended.\r\n\r\n");

                        m_remoteCommandClientID = requestinfo.Sender.ClientID;
                        SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("TelnetSession", "Established"));
                    }
                    else
                    {
                        UpdateStatus(requestinfo.Sender.ClientID, UpdateType.Alarm, "Failed to establish remote command session - Password is invalid.\r\n\r\n");
                    }
                }
                else if (string.Compare(requestinfo.Request.Command, "Telnet", true) == 0 && m_remoteCommandProcess != null && disconnectSession)
                {
                    // User wants to terminate an established remote command session.                   
                    m_remoteCommandProcess.ErrorDataReceived -= RemoteCommandProcess_ErrorDataReceived;
                    m_remoteCommandProcess.OutputDataReceived -= RemoteCommandProcess_OutputDataReceived;

                    if (!m_remoteCommandProcess.HasExited)
                        m_remoteCommandProcess.Kill();

                    m_remoteCommandProcess.Dispose();
                    m_remoteCommandProcess = null;

                    m_remoteCommandClientID = Guid.Empty;
                    SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("TelnetSession", "Terminated"));

                    UpdateStatus(UpdateType.Information, "Remote command session terminated - status updates are resumed.\r\n\r\n");
                }
                else if (m_remoteCommandProcess != null)
                {
                    // User has entered commands that must be redirected to the established command session.
                    string input = requestinfo.Request.Command + " " + requestinfo.Request.Arguments.ToString();
                    m_remoteCommandProcess.StandardInput.WriteLine(input);
                }
                else
                {
                    // User has provided insufficient information.
                    requestinfo.Request = ClientRequest.Parse("Telnet /?");
                    RemoteTelnetSession(requestinfo);
                }
            }
        }

        #endregion

        #endregion
    }
}
