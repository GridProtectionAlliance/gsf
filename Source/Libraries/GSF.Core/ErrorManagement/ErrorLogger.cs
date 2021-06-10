//******************************************************************************************************
//  ErrorLogger.cs - Gbtc
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
//  04/03/2007 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  09/17/2008 - Pinal C. Patel
//       Converted code to C#.
//  10/16/2008 - Pinal C. Patel
//       Edited code comments.
//  06/18/2009 - Pinal C. Patel
//       Modified Log() to quit if Enabled is false.
//  07/27/2009 - Pinal C. Patel
//       Made logging user information along with exception information optional via LogUserInfo property.
//  08/06/2009 - Pinal C. Patel
//       Enabled upon initialization rather than instantiation.
//  08/07/2009 - Pinal C. Patel
//       Added LoggingException event to notify about exceptions encountered when logging exceptions.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  02/11/2010 - Pinal C. Patel
//       Added OS version to the system information being logged.
//  03/27/2012 - Prasanth GS
//       Exceptions logged through ErrorLogger are saved to backend db too.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  08/23/2013 - J. Ritchie Carroll
//       Adjusted algorithm for database inserts and curtailment to be more friendly to DB engine.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        User Friendly ASP.NET Exception Handling
//        http://www.codeproject.com/KB/aspnet/ASPNETExceptionHandling.aspx
//
//   Copyright Jeff Atwood
//  
//   The Code Project Open License (CPOL):
//        http://www.codeproject.com/info/cpol10.aspx
//
//*******************************************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Identity;
using GSF.IO;
using GSF.Net.Smtp;
using GSF.Reflection;
using Timer = System.Timers.Timer;

namespace GSF.ErrorManagement
{
    /// <summary>
    /// Represents a logger that can be used for logging handled as well as unhandled exceptions across multiple 
    /// application types (Console Application, Windows Service, Web Application, Web Service).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Adapted from exception handling code by Jeff Atwood of CodingHorror.com. Demo projects for handling unhandled
    /// exception in both windows and web environment by Jeff Atwood are available at The Code Project web site.
    /// See: http://www.codeproject.com/script/articles/list_articles.asp?userid=450027
    /// </para>
    /// <para>
    /// Error logger with Windows Forms capabilities can be found in the GSF.Windows assembly.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="ErrorLogger"/> component to log handled and unhandled exceptions:
    /// <code>
    /// using System;
    /// using System.IO;
    /// using GSF.ErrorManagement;
    ///
    /// class Program
    /// {
    ///     static ErrorLogger s_logger;
    ///
    ///     static Program()
    ///     {
    ///         s_logger = new ErrorLogger();
    ///         s_logger.LogToUI = true;                    // Show exception info on the UI.
    ///         s_logger.LogToFile = true;                  // Log exception info to a file.
    ///         s_logger.LogToEmail = true;                 // Send exception info in an e-mail.
    ///         s_logger.LogToEventLog = true;              // Log exception info to the event log.
    ///         s_logger.ContactEmail = "dev@email.com";    // Provide an e-mail address.
    ///         s_logger.HandleUnhandledException = true;   // Configure to handle unhandled exceptions.
    ///         s_logger.PersistSettings = true;            // Save settings to the config file.
    ///         s_logger.Initialize();                      // Initialize ErrorLogger component for use.
    ///     }
    ///
    ///     static void Main(string[] args)
    ///     {
    ///         try
    ///         {
    ///             // This may cause a handled FileNotFoundException if the file doesn't exist.
    ///             string data = File.ReadAllText(@"c:\NonExistentFile.txt");
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             // When logging handled exceptions we want to disable loggers (UI logger and E-mail logger) that
    ///             // may require interaction either directly or indirectly as it can be annoying. All the loggers
    ///             // are enabled automatically after the handled exception has been logged.
    ///             s_logger.SuppressInteractiveLogging();
    ///             s_logger.Log(ex);
    ///         }
    ///
    ///         int numerator = 1;
    ///         int denominator = 0;
    ///         int result = numerator / denominator;   // This will cause an unhandled DivideByZeroException.
    ///
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ErrorModule"/>
    [ToolboxBitmap(typeof(ErrorLogger))]
    public class ErrorLogger : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="LogToUI"/> property.
        /// </summary>
        public const bool DefaultLogToUI = false;

        /// <summary>
        /// Specifies the default value for the <see cref="LogToFile"/> property.
        /// </summary>
        public const bool DefaultLogToFile = true;

        /// <summary>
        /// Specifies the default value for the <see cref="LogToEmail"/> property.
        /// </summary>
        public const bool DefaultLogToEmail = false;

        /// <summary>
        /// Specifies the default value for the <see cref="LogToEventLog"/> property.
        /// </summary>
        public const bool DefaultLogToEventLog = true;

        /// <summary>
        /// Specifies the default value for the <see cref="LogToDatabase"/> property.
        /// </summary>
        public const bool DefaultLogToDatabase = false;

        /// <summary>
        /// Specifies the default value for the <see cref="LogUserInfo"/> property.
        /// </summary>
        public const bool DefaultLogUserInfo = false;

        /// <summary>
        /// Specifies the default value for the <see cref="DefaultDatabaseLogSize"/> property.
        /// </summary>
        private const int DefaultDatabaseLogSize = 2000;

        /// <summary>
        /// Specifies the default value for the <see cref="SmtpServer"/> property.
        /// </summary>
        public const string DefaultSmtpServer = Mail.DefaultSmtpServer;

        /// <summary>
        /// Specifies the default value for the <see cref="ContactName"/> property.
        /// </summary>
        public const string DefaultContactName = "";

        /// <summary>
        /// Specifies the default value for the <see cref="ContactEmail"/> property.
        /// </summary>
        public const string DefaultContactEmail = "";

        /// <summary>
        /// Specifies the default value for the <see cref="ContactPhone"/> property.
        /// </summary>
        public const string DefaultContactPhone = "";

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ErrorLogger";

        /// <summary>
        /// Specifies the default value for the <see cref="HandleUnhandledException"/> property.
        /// </summary>
        public const bool DefaultHandleUnhandledException = true;

        /// <summary>
        /// Specifies the default value for the <see cref="ExitOnUnhandledException"/> property.
        /// </summary>
        public const bool DefaultExitOnUnhandledException = false;

        // Events

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while logging an <see cref="Exception"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> that was encountered while logging an <see cref="Exception"/>.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> LoggingException;

        // Fields
        private bool m_logToUI;
        private bool m_logToFile;
        private bool m_logToEmail;
        private bool m_logToEventLog;
        private bool m_logToDatabase;
        private bool m_logUserInfo;
        private int m_databaseLogSize;
        private string m_smtpServer;
        private string m_contactName;
        private string m_contactEmail;
        private string m_contactPhone;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private bool m_enabled;
        private bool m_handleUnhandledException;
        private bool m_exitOnUnhandledException;
        private readonly LogFile m_errorLog;
        private Exception m_lastException;
        private ApplicationType m_appType;
        private Func<string> m_errorTextMethod;
        private Func<string> m_scopeTextMethod;
        private Func<string> m_actionTextMethod;
        private Func<string> m_moreInfoTextMethod;
        private readonly List<Action<Exception>> m_loggers;
        private bool m_logToFileOK;
        private bool m_logToEmailOK;
        private bool m_logToEventLogOK;
        private bool m_logToDatabaseOK;
        private bool m_disposed;
        private bool m_initialized;
        private bool m_suppressInteractiveLogging;
        private ProcessQueue<Tuple<string, string, string, string>> m_databaseErrorQueue;
        private Timer m_tableSizeCurtailmentTimer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLogger"/> class.
        /// </summary>
        public ErrorLogger()
        {
            m_logToUI = DefaultLogToUI;
            m_logToFile = DefaultLogToFile;
            m_logToEmail = DefaultLogToEmail;
            m_logToEventLog = DefaultLogToEventLog;
            m_logToDatabase = DefaultLogToDatabase;
            m_logUserInfo = DefaultLogUserInfo;
            m_databaseLogSize = DefaultDatabaseLogSize;
            m_smtpServer = DefaultSmtpServer;
            m_contactName = DefaultContactName;
            m_contactEmail = DefaultContactEmail;
            m_contactPhone = DefaultContactPhone;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_handleUnhandledException = DefaultHandleUnhandledException;
            m_exitOnUnhandledException = DefaultExitOnUnhandledException;

            // Initialize delegate methods.
            m_errorTextMethod = GetErrorText;
            m_scopeTextMethod = GetScopeText;
            m_actionTextMethod = GetActionText;
            m_moreInfoTextMethod = GetMoreInfoText;

            // Initialize the error log file.
            m_errorLog = new LogFile();
            m_errorLog.FileName = "ErrorLog.txt";

            // Initialize all logger methods.
            m_loggers = new List<Action<Exception>>();
            m_loggers.Add(ExceptionToDatabase);
            m_loggers.Add(ExceptionToEventLog);
            m_loggers.Add(ExceptionToEmail);
            m_loggers.Add(ExceptionToFile);
            m_loggers.Add(ExceptionToUI);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLogger"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ErrorLogger"/>.</param>
        public ErrorLogger(IContainer container)
            : this()
        {
            if ((object)container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Exception"/> information is to be 
        /// displayed on the application UI.
        /// </summary>
        /// <remarks>
        /// This setting is ignored in Windows Service and Web Service application types.
        /// </remarks>
        [Category("Logging"),
        DefaultValue(DefaultLogToUI),
        Description("Indicates whether Exception information is to be displayed on the application UI.")]
        public bool LogToUI
        {
            get
            {
                return m_logToUI;
            }
            set
            {
                m_logToUI = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Exception"/> information is to be 
        /// written to the <see cref="ErrorLog"/>.
        /// </summary>
        [Category("Logging"),
        DefaultValue(DefaultLogToFile),
        Description("Indicates whether Exception information is to be written to the ErrorLog.")]
        public bool LogToFile
        {
            get
            {
                return m_logToFile;
            }
            set
            {
                m_logToFile = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Exception"/> information is to be sent 
        /// in an e-mail to the <see cref="ContactEmail"/> address.
        /// </summary>
        [Category("Logging"),
        DefaultValue(DefaultLogToEmail),
        Description("Indicates whether Exception information is to be sent in an e-mail to the ContactEmail address.")]
        public bool LogToEmail
        {
            get
            {
                return m_logToEmail;
            }
            set
            {
                m_logToEmail = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Exception"/> information is to be 
        /// written to the <see cref="EventLog"/>.
        /// </summary>
        [Category("Logging"),
        DefaultValue(DefaultLogToEventLog),
        Description("Indicates whether Exception information is to be written to the EventLog.")]
        public bool LogToEventLog
        {
            get
            {
                return m_logToEventLog;
            }
            set
            {
                m_logToEventLog = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Exception"/> information is to be 
        /// written to the database.
        /// </summary>
        [Category("Logging"),
        DefaultValue(DefaultLogToDatabase),
        Description("Indicates whether Exception information is to be written to the database.")]
        public bool LogToDatabase
        {
            get
            {
                return m_logToDatabase;
            }
            set
            {
                m_logToDatabase = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether information about local user (for windows applications) 
        /// or remote user (for web applications) is to be logged when logging an <see cref="Exception"/>.
        /// </summary>
        [Category("Logging"),
        DefaultValue(DefaultLogUserInfo),
        Description("Indicates whether information about a local user (for windows application) or remote user (for web application) is to be logged when logging an Exception.")]
        public bool LogUserInfo
        {
            get
            {
                return m_logUserInfo;
            }
            set
            {
                m_logUserInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum exception log size to maintain when logging exceptions to the database.
        /// </summary>
        [Category("Logging"),
        DefaultValue(DefaultDatabaseLogSize),
        Description("Defines the maximum exception log size to maintain when logging exceptions to the database.")]
        public int DatabaseLogSize
        {
            get
            {
                return m_databaseLogSize;
            }
            set
            {
                m_databaseLogSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the SMTP server to be used for sending e-mail messages containing <see cref="Exception"/> 
        /// information to the <see cref="ContactEmail"/> address.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultSmtpServer),
        Description("The SMTP server to be used for sending e-mail messages containing Exception information to the ContactEmail address.")]
        public string SmtpServer
        {
            get
            {
                return m_smtpServer;
            }
            set
            {
                m_smtpServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the person who can be contacted by the end-user in case of an <see cref="Exception"/>.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultContactName),
        Description("Name of the person who can be contacted by the end-user in case of an Exception.")]
        public string ContactName
        {
            get
            {
                return m_contactName;
            }
            set
            {
                m_contactName = value;
            }
        }

        /// <summary>
        /// Gets or sets the e-mail address where e-mail messages containing <see cref="Exception"/> information are 
        /// to be sent when the <see cref="LogToEmail"/> property is set to true.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultContactEmail),
        Description("E-mail address where e-mail messages containing Exception information are to be sent when the LogToEmail property is set to true.")]
        public string ContactEmail
        {
            get
            {
                return m_contactEmail;
            }
            set
            {
                m_contactEmail = value;
            }
        }

        /// <summary>
        /// Gets or sets the phone number that can be used by the end-user to communicate about an encountered <see cref="Exception"/>.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultContactPhone),
        Description("Phone number that can be used by the end-user to communicate about an encountered Exception.")]
        public string ContactPhone
        {
            get
            {
                return m_contactPhone;
            }
            set
            {
                m_contactPhone = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ErrorLogger"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistence"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of ErrorLogger object are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of <see cref="ErrorLogger"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Persistence"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of ErrorLogger object are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ErrorLogger"/> object must register 
        /// itself to handle <see cref="Exception"/> that are not trapped inside of a try-catch block.
        /// </summary>
        [Category("Behavior"),
        DefaultValue(DefaultHandleUnhandledException),
        Description("Indicates whether the ErrorLogger object must register itself to handle Exception that are not trapped inside of a try-catch block.")]
        public bool HandleUnhandledException
        {
            get
            {
                return m_handleUnhandledException;
            }
            set
            {
                m_handleUnhandledException = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the application will terminate after logging an 
        /// unhandled <see cref="Exception"/>.
        /// </summary>
        [Category("Behavior"),
        DefaultValue(DefaultExitOnUnhandledException),
        Description("Indicates whether the application will terminate after logging an unhandled Exception.")]
        public bool ExitOnUnhandledException
        {
            get
            {
                return m_exitOnUnhandledException;
            }
            set
            {
                m_exitOnUnhandledException = value;
            }
        }

        /// <summary>
        /// Get the <see cref="LogFile"/> object used for logging <see cref="Exception"/> information to a file.
        /// </summary>
        [Category("Components"),
        Description("Get the LogFile object used for logging Exception information to a file."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LogFile ErrorLog
        {
            get
            {
                return m_errorLog;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ErrorLogger"/> object is currently enabled.
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
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that provides common text stating what could have possibly 
        /// caused the <see cref="Exception"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        /// <example>
        /// Sample text:
        /// <para>
        /// An unexpected exception has occurred in RogueApplication. This may be due to an inconsistent system 
        /// state or a programming error.
        /// </para>
        /// </example>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string> ErrorTextMethod
        {
            get
            {
                return m_errorTextMethod;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                m_errorTextMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that provides text stating what is going to happen as a result 
        /// of the <see cref="Exception"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        /// <example>
        /// Sample text:
        /// <para>The action you requested was not performed.</para>
        /// </example>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string> ScopeTextMethod
        {
            get
            {
                return m_scopeTextMethod;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                m_scopeTextMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that provides text stating the action(s) that can be taken by 
        /// the end-user after an <see cref="Exception"/> is encountered.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        /// <example>
        /// Sample text:
        /// <para>Close your browser, navigate back to the website, and try repeating you last action.</para>
        /// </example>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string> ActionTextMethod
        {
            get
            {
                return m_actionTextMethod;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                m_actionTextMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that provides text containing detailed information about the 
        /// encountered <see cref="Exception"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string> MoreInfoTextMethod
        {
            get
            {
                return m_moreInfoTextMethod;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                m_moreInfoTextMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that controls if <see cref="Loggers"/> requiring interaction either directly or indirectly
        /// when logging handled <see cref="Exception"/> using the <see cref="Log(Exception)"/> method are enabled.
        /// </summary>
        public bool SuppressInteractiveLogging
        {
            get
            {
                return m_suppressInteractiveLogging;
            }
            protected set
            {
                m_suppressInteractiveLogging = value;
            }
        }

        /// <summary>
        /// Gets the name of the currently executing application.
        /// </summary>
        [Browsable(false)]
        public string ApplicationName
        {
            get
            {
                switch (ApplicationType)
                {
                    case ApplicationType.WindowsCui:
                    case ApplicationType.WindowsGui:
                        return FilePath.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
                    case ApplicationType.Web:
                        return HttpContext.Current.Request.ApplicationPath.ToNonNullString("").Replace("/", "");
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the type of the currently executing application.
        /// </summary>
        [Browsable(false)]
        public ApplicationType ApplicationType
        {
            get
            {
                if (m_appType == ApplicationType.Unknown)
                    m_appType = Common.GetApplicationType();

                return m_appType;
            }
        }

        /// <summary>
        /// Get the last encountered <see cref="Exception"/>.
        /// </summary>
        [Browsable(false)]
        public Exception LastException
        {
            get
            {
                return m_lastException;
            }
        }

        /// <summary>
        /// Gets a list of methods registered for logging information about an encountered <see cref="Exception"/>.
        /// </summary>
        /// <remarks>
        /// This property can be used to register additional methods for logging information about an encountered
        /// <see cref="Exception"/>. When an <see cref="Exception"/> is logged, all registered methods that take 
        /// <see cref="Exception"/> as a parameter are invoked.
        /// </remarks>
        [Browsable(false)]
        public List<Action<Exception>> Loggers
        {
            get
            {
                return m_loggers;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="ErrorLogger"/> object.
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
        /// Gets the descriptive status of the <see cref="ErrorLogger"/> object.
        /// </summary>
        [Browsable(false)]
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("               Error to UI: ");
                status.Append(m_logToUI ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("             Error to File: ");
                status.Append(m_logToFile ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("            Error to Email: ");
                status.Append(m_logToEmail ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("        Error to Event Log: ");
                status.Append(m_logToEventLog ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("         Error to Database: ");
                status.Append(m_logToDatabase ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("         Database log size: ");
                status.Append(m_databaseLogSize + " records");
                status.AppendLine();
                status.Append("               Mail Server: ");
                status.Append(!string.IsNullOrEmpty(m_smtpServer) ? m_smtpServer : "[Not Set]");
                status.AppendLine();
                status.Append("              Contact Name: ");
                status.Append(!string.IsNullOrEmpty(m_contactName) ? m_contactName : "[Not Set]");
                status.AppendLine();
                status.Append("             Contact Email: ");
                status.Append(!string.IsNullOrEmpty(m_contactEmail) ? m_contactEmail : "[Not Set]");
                status.AppendLine();
                status.Append("             Contact Phone: ");
                status.Append(!string.IsNullOrEmpty(m_contactPhone) ? m_contactPhone : "[Not Set]");
                status.AppendLine();
                status.Append("    Handle Unhandled Error: ");
                status.Append(m_handleUnhandledException ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("  Exit On Unhandled Errors: ");
                status.Append(m_exitOnUnhandledException ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("    Last Encountered Error: ");
                status.Append((object)m_lastException != null ? m_lastException.GetType().FullName : "[None]");
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ErrorLogger"/> object and optionally releases the 
        /// managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    Unregister();

                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();

                        if ((object)m_errorLog != null)
                            m_errorLog.Dispose();

                        if ((object)m_tableSizeCurtailmentTimer != null)
                        {
                            m_tableSizeCurtailmentTimer.Elapsed -= m_tableSizeCurtailmentTimer_Elapsed;
                            m_tableSizeCurtailmentTimer.Dispose();
                            m_tableSizeCurtailmentTimer = null;
                        }

                        if ((object)m_databaseErrorQueue != null)
                        {
                            m_databaseErrorQueue.ProcessException -= m_databaseErrorQueue_ProcessException;
                            m_databaseErrorQueue.Dispose();
                        }
                        m_databaseErrorQueue = null;
                    }
                }
                finally
                {
                    m_enabled = false;          // Mark as disabled.
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="ErrorLogger"/> object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ErrorLogger"/> 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();                                     // Load settings from the config file.
                Register();                                         // Register the logger for unhandled exceptions.
                m_enabled = true;                                   // Mark as enabled.
                m_initialized = true;                               // Initialize only once.
            }
        }

        /// <summary>
        /// Performs necessary operations before the <see cref="ErrorLogger"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ErrorLogger"/> object is consumed through the designer surface of the IDE.
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
        /// Performs necessary operations after the <see cref="ErrorLogger"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ErrorLogger"/> object is consumed through the designer surface of the IDE.
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
        /// Saves settings for the <see cref="ErrorLogger"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                settings["LogToUI", true].Update(m_logToUI);
                settings["LogToFile", true].Update(m_logToFile);
                settings["LogToEmail", true].Update(m_logToEmail);
                settings["LogToEventLog", true].Update(m_logToEventLog);
                settings["LogToDatabase", true].Update(m_logToDatabase);
                settings["LogUserInfo", true].Update(m_logUserInfo);
                settings["DatabaseLogSize", true].Update(m_databaseLogSize);
                settings["SmtpServer", true].Update(m_smtpServer);
                settings["ContactName", true].Update(m_contactName);
                settings["ContactEmail", true].Update(m_contactEmail);
                settings["ContactPhone", true].Update(m_contactPhone);
                settings["HandleUnhandledException", true].Update(m_handleUnhandledException);
                settings["ExitOnUnhandledException", true].Update(m_exitOnUnhandledException);

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="ErrorLogger"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                settings.Add("LogToUI", m_logToUI, "True if an encountered exception is to be logged to the User Interface; otherwise False.");
                settings.Add("LogToFile", m_logToFile, "True if an encountered exception is to be logged to a file; otherwise False.");
                settings.Add("LogToEmail", m_logToEmail, "True if an email is to be sent to ContactEmail with the details of an encountered exception; otherwise False.");
                settings.Add("LogToEventLog", m_logToEventLog, "True if an encountered exception is to be logged to the Event Log; otherwise False.");
                settings.Add("LogToDatabase", m_logToDatabase, "True if an encountered exception is logged to the database; otherwise False.");
                settings.Add("DatabaseLogSize", m_databaseLogSize, "Maximum exception log size to maintain when logging exceptions to the database.");
                settings.Add("LogUserInfo", m_logUserInfo, "True if user information is to be logged along with exception information; otherwise False.");
                settings.Add("SmtpServer", m_smtpServer, "Name of the SMTP server to be used for sending the email messages.");
                settings.Add("ContactName", m_contactName, "Name of the person that the end-user can contact when an exception is encountered.");
                settings.Add("ContactEmail", m_contactEmail, "Comma-separated list of recipient email addresses for the email message.");
                settings.Add("ContactPhone", m_contactPhone, "Phone number of the person that the end-user can contact when an exception is encountered.");
                settings.Add("HandleUnhandledException", m_handleUnhandledException, "True if unhandled exceptions are to be handled automatically; otherwise False.");
                settings.Add("ExitOnUnhandledException", m_exitOnUnhandledException, "True if the application must exit when an unhandled exception is encountered; otherwise False.");

                LogToUI = settings["LogToUI"].ValueAs(m_logToUI);
                LogToFile = settings["LogToFile"].ValueAs(m_logToFile);
                LogToEmail = settings["LogToEmail"].ValueAs(m_logToEmail);
                LogToEventLog = settings["LogToEventLog"].ValueAs(m_logToEventLog);
                LogToDatabase = settings["LogToDatabase"].ValueAs(m_logToDatabase);
                LogUserInfo = settings["LogUserInfo"].ValueAs(m_logUserInfo);
                DatabaseLogSize = settings["DatabaseLogSize"].ValueAs(m_databaseLogSize);
                SmtpServer = settings["SmtpServer"].ValueAs(m_smtpServer);
                ContactEmail = settings["ContactEmail"].ValueAs(m_contactEmail);
                ContactName = settings["ContactName"].ValueAs(m_contactName);
                ContactPhone = settings["ContactPhone"].ValueAs(m_contactPhone);
                HandleUnhandledException = settings["HandleUnhandledException"].ValueAs(m_handleUnhandledException);
                ExitOnUnhandledException = settings["ExitOnUnhandledException"].ValueAs(m_exitOnUnhandledException);
            }
        }

        /// <summary>
        /// Logs information about the encountered <see cref="Exception"/>.
        /// </summary>
        /// <param name="exception">Encountered <see cref="Exception"/> whose information is to be logged.</param>
        public void Log(Exception exception)
        {
            Log(exception, false);
        }

        /// <summary>
        /// Logs information about the encountered <see cref="Exception"/>.
        /// </summary>
        /// <param name="exception">Encountered <see cref="Exception"/> whose information is to be logged.</param>
        /// <param name="exitApplication">true to exit the application; otherwise false.</param>
        public void Log(Exception exception, bool exitApplication)
        {
            // Initialize if uninitialized.
            Initialize();

            // Quit if disabled.
            if (!m_enabled)
                return;

            // Save the encountered exception.
            m_lastException = exception;

            // Iterate through all of the registered logger methods and invoke them for processing the exception.
            foreach (Action<Exception> logger in m_loggers)
            {
                try
                {
                    logger(exception);
                }
                catch (Exception ex)
                {
                    OnLoggingException(ex);
                }
            }

            m_suppressInteractiveLogging = false;   // Enable interactive logging if disabled.

            // Exit the current application if specified.
            if (exitApplication)
            {
                ErrorLog.Flush();
                switch (ApplicationType)
                {
                    // In windows environment we can simply call Environment.Exit() to exit the application. This
                    // will terminate the application regardless of active foreground/background threads.
                    case ApplicationType.WindowsGui:
                    case ApplicationType.WindowsCui:
                        Environment.Exit(-1);
                        break;
                    // In web environment we unload the app domain of the current application instead of terminating
                    // the entire process, because in ASP.NET a single process (App Pool) can host one or more 
                    // app domains (web sites or web services).
                    case ApplicationType.Web:
                        HttpRuntime.UnloadAppDomain();
                        break;
                }
            }
        }

        /// <summary>
        /// Registers the <see cref="ErrorLogger"/> object to handle unhandled <see cref="Exception"/> if the 
        /// <see cref="HandleUnhandledException"/> property is set to true.
        /// </summary>
        /// <returns><c>true</c> if handlers were registered; otherwise <c>false</c>.</returns>
        protected virtual bool Register()
        {
            if (m_handleUnhandledException && !Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += UnhandledException;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unregister the <see cref="ErrorLogger"/> object from handling unhandled <see cref="Exception"/>.
        /// </summary>
        /// <returns><c>true</c> if handlers were unregistered; otherwise <c>false</c>.</returns>
        protected virtual bool Unregister()
        {
            if (m_handleUnhandledException && !Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException -= UnhandledException;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Logs encountered <see cref="Exception"/> to the application UI.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> that was encountered.</param>
        protected virtual void ExceptionToUI(Exception exception)
        {
            // Log if enabled.
            if (m_logToUI && !m_suppressInteractiveLogging)
            {
                switch (ApplicationType)
                {
                    case ApplicationType.Web:           // Web Application
                        ExceptionToWebPage();
                        break;
                    case ApplicationType.WindowsCui:    // Console Application
                        ExceptionToWindowsCui();
                        break;
                    case ApplicationType.WindowsGui:    // Windows Application
                        ExceptionToWindowsGui();
                        break;
                }
            }
        }

        /// <summary>
        /// Shows <see cref="Exception"/> information in a Windows Application.
        /// </summary>
        protected virtual void ExceptionToWindowsGui()
        {
            throw new NotImplementedException("Exceptions for Windows Forms should be referenced from GSF.Windows.dll");
        }

        /// <summary>
        /// Shows <see cref="Exception"/> information in a Console Application.
        /// </summary>
        protected virtual void ExceptionToWindowsCui()
        {
            // Prepare the exception information.
            StringBuilder message = new StringBuilder();

            message.AppendFormat("{0} has encountered a problem", ApplicationName);
            message.AppendLine();
            message.AppendLine();
            message.Append("What happened:");
            message.AppendLine();
            message.Append(m_errorTextMethod());
            message.AppendLine();
            message.AppendLine();
            message.Append("How this will affect you:");
            message.AppendLine();
            message.Append(m_scopeTextMethod());
            message.AppendLine();
            message.AppendLine();
            message.Append("What you can do about it:");
            message.AppendLine();
            message.Append(m_actionTextMethod());
            message.AppendLine();
            message.AppendLine();
            message.Append("More information:");
            message.AppendLine();
            message.Append(m_moreInfoTextMethod());
            message.AppendLine();

            // Display the exception information.
            System.Console.Write(message.ToString());
            System.Console.ReadLine();
        }

        /// <summary>
        /// Shows <see cref="Exception"/> information in a Web Site.
        /// </summary>
        protected virtual void ExceptionToWebPage()
        {
            // Prepare the exception information.
            StringBuilder html = new StringBuilder();

            html.Append("<HTML>");
            html.AppendLine();
            html.Append("<HEAD>");
            html.AppendLine();
            html.Append("<TITLE>");
            html.AppendLine();
            html.AppendFormat("{0} has encountered a problem", ApplicationName);
            html.AppendLine();
            html.Append("</TITLE>");
            html.AppendLine();
            html.Append("<STYLE>");
            html.AppendLine();
            html.Append("body {font-family:\"Verdana\";font-weight:normal;font-size: .7em;color:black; background-color:white;}");
            html.AppendLine();
            html.Append("b {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}");
            html.AppendLine();
            html.Append("H1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }");
            html.AppendLine();
            html.Append("H2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }");
            html.AppendLine();
            html.Append("pre {font-family:\"Lucida Console\";font-size: .9em}");
            html.AppendLine();
            html.Append("</STYLE>");
            html.AppendLine();
            html.Append("</HEAD>");
            html.AppendLine();
            html.Append("<BODY>");
            html.AppendLine();
            html.Append("<H1>");
            html.AppendLine();
            html.AppendFormat("The {0} website has encountered a problem", ApplicationName);
            html.AppendLine();
            html.Append("<hr width=100% size=1 color=silver></H1>");
            html.AppendLine();
            html.Append("<H2>What Happened</H2>");
            html.AppendLine();
            html.Append("<BLOCKQUOTE>");
            html.AppendLine();
            html.Append(m_errorTextMethod());
            html.AppendLine();
            html.Append("</BLOCKQUOTE>");
            html.AppendLine();
            html.Append("<H2>How this will affect you</H2>");
            html.AppendLine();
            html.Append("<BLOCKQUOTE>");
            html.AppendLine();
            html.Append(m_scopeTextMethod());
            html.AppendLine();
            html.Append("</BLOCKQUOTE>");
            html.AppendLine();
            html.Append("<H2>What you can do about it</H2>");
            html.AppendLine();
            html.Append("<BLOCKQUOTE>");
            html.AppendLine();
            html.Append(m_actionTextMethod());
            html.AppendLine();
            html.Append("</BLOCKQUOTE>");
            html.AppendLine();
            html.Append("<INPUT type=button value=\"More Information &gt;&gt;\" onclick=\"this.style.display=\'none\'; document.getElementById(\'MoreInfo\').style.display=\'block\'\">");
            html.AppendLine();
            html.Append("<DIV style=\'display:none;\' id=\'MoreInfo\'>");
            html.AppendLine();
            html.Append("<H2>More information</H2>");
            html.AppendLine();
            html.Append("<TABLE width=\"100%\" bgcolor=\"#ffffcc\">");
            html.AppendLine();
            html.Append("<TR><TD>");
            html.AppendLine();
            html.Append("<CODE><PRE>");
            html.AppendLine();
            html.Append(m_moreInfoTextMethod());
            html.AppendLine();
            html.Append("</PRE></CODE>");
            html.AppendLine();
            html.Append("<TD><TR>");
            html.AppendLine();
            html.Append("</DIV>");
            html.AppendLine();
            html.Append("</BODY>");
            html.AppendLine();
            html.Append("</HTML>");
            html.AppendLine();

            // Display the exception information.
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(html.ToString());
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
            HttpContext.Current.Server.ClearError();
        }

        /// <summary>
        /// Logs encountered <see cref="Exception"/> to the <see cref="ErrorLog"/>.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> that was encountered.</param>
        protected virtual void ExceptionToFile(Exception exception)
        {
            // Log if enabled.
            if (m_logToFile)
            {
                m_logToFileOK = false;

                if (!m_errorLog.IsOpen)
                    m_errorLog.Open();

                m_errorLog.WriteTimestampedLine(GetExceptionInfo(exception, m_logUserInfo));

                m_logToFileOK = true;
            }
        }

        /// <summary>
        /// Logs encountered <see cref="Exception"/> to an e-mail message.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> that was encountered.</param>
        protected virtual void ExceptionToEmail(Exception exception)
        {
            // Log if enabled.
            if (m_logToEmail && !string.IsNullOrEmpty(m_contactEmail) && !m_suppressInteractiveLogging)
            {
                m_logToEmailOK = false;

                Mail email = new Mail(m_contactEmail, m_contactEmail);

                email.Subject = string.Format("Exception in {0} at {1}", ApplicationName, DateTime.Now.ToString());
                email.Body = GetExceptionInfo(exception, m_logUserInfo);
                email.Attachments = GetEmailAttachments();
                email.SmtpServer = m_smtpServer;
                email.Send();

                m_logToEmailOK = true;
            }
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-separated list of file names to be attached to the <see cref="Mail"/> message.
        /// </summary>
        protected virtual string GetEmailAttachments()
        {
            return null;
        }

        /// <summary>
        /// Logs encountered <see cref="Exception"/> to the <see cref="EventLog"/>.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> that was encountered.</param>
        protected virtual void ExceptionToEventLog(Exception exception)
        {
            // Log if enabled.
            if (m_logToEventLog)
            {
                m_logToEventLogOK = false;

                EventLog.WriteEntry(ApplicationName, GetExceptionInfo(exception, m_logUserInfo), EventLogEntryType.Error);

                m_logToEventLogOK = true;
            }
        }

        /// <summary>
        /// Logs encountered <see cref="Exception"/> to the database.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> that was encountered.</param>
        protected virtual void ExceptionToDatabase(Exception exception)
        {
            // Log if enabled.
            if (m_logToDatabase)
            {
                m_logToDatabaseOK = false;

                // Make sure a database processing queue exists to process items every two seconds
                if ((object)m_databaseErrorQueue == null)
                {
                    m_databaseErrorQueue = ProcessQueue<Tuple<string, string, string, string>>.CreateSynchronousQueue(ProcessExceptionsToDatabase, 2.0D, 30, false, false);
                    m_databaseErrorQueue.ProcessException += m_databaseErrorQueue_ProcessException;
                    m_databaseErrorQueue.Start();
                }

                // Make sure table size curtailment timer exists to maintain error log table size
                if ((object)m_tableSizeCurtailmentTimer == null)
                {
                    m_tableSizeCurtailmentTimer = new Timer();

                    m_tableSizeCurtailmentTimer.AutoReset = false;
                    m_tableSizeCurtailmentTimer.Interval = 15000;
                    m_tableSizeCurtailmentTimer.Enabled = false;

                    m_tableSizeCurtailmentTimer.Elapsed += m_tableSizeCurtailmentTimer_Elapsed;
                }

                // Queue up exception to be logged into the database - we only process database inserts once every two
                // seconds in case there are many errors, this way database thrashing is kept to a minimum
                m_databaseErrorQueue.Add(new Tuple<string, string, string, string>(
                        exception.Source.ToNonNullNorEmptyString("No Source"),
                        exception.GetType().FullName,
                        exception.Message.ToNonNullNorEmptyString("No Message"),
                        GetExceptionInfo(exception, true)));

                m_logToDatabaseOK = true;
            }
        }

        // Handle logging of queued exceptions to database
        private void ProcessExceptionsToDatabase(Tuple<string, string, string, string>[] exceptions)
        {
            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            using (IDbCommand command = database.Connection.CreateCommand())
            using (IDbTransaction transaction = database.Connection.BeginTransaction())
            {
                try
                {
                    string parameterizedQueryString = database.ParameterizedQueryString("INSERT INTO ErrorLog (Source, Type, Message, Detail) VALUES ({0}, {1}, {2}, {3})", "source", "type", "message", "detail");

                    // Associate all commands with a single transaction
                    command.Transaction = transaction;

                    // Insert new exceptions
                    foreach (Tuple<string, string, string, string> exception in exceptions)
                        command.ExecuteNonQuery(parameterizedQueryString, exception.Item1, exception.Item2, exception.Item3, exception.Item4);

                    // Commit the transaction
                    transaction.Commit();

                    // Since we have now added records, kick off lazy timer to check error log size
                    if ((object)m_tableSizeCurtailmentTimer != null && !m_tableSizeCurtailmentTimer.Enabled)
                        m_tableSizeCurtailmentTimer.Start();
                }
                catch
                {
                    // Rollback transaction if there is an exception (e.g., thread is being aborted)
                    transaction.Rollback();

                    // Re-throw any captured exception, process queue will handle
                    throw;
                }
            }
        }

        // Maintain log size
        private void m_tableSizeCurtailmentTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                using (IDbCommand command = database.Connection.CreateCommand())
                using (IDbTransaction transaction = database.Connection.BeginTransaction(database.DefaultIsolationLevel))
                {
                    bool executingTransaction = false;

                    try
                    {
                        // Associate command with transaction
                        command.Transaction = transaction;

                        // Get min and max IDs from error log
                        int minID = Convert.ToInt32(command.ExecuteScalar(string.Format("SELECT MIN(ID) FROM ErrorLog")));
                        int maxID = Convert.ToInt32(command.ExecuteScalar(string.Format("SELECT MAX(ID) FROM ErrorLog")));
                        int errorLogSize = maxID - minID; // Roughly, assuming no manual deletions

                        // When exception log is larger than desired size - delete roughly 25% of the records,
                        // timeout for this action currently hard-coded to one minute
                        if (errorLogSize >= m_databaseLogSize)
                        {
                            executingTransaction = true;
                            command.ExecuteNonQuery("DELETE FROM ErrorLog WHERE ID <= " + (int)(errorLogSize / 4 + minID), 60);
                            transaction.Commit();
                        }
                    }
                    catch
                    {
                        // Rollback any executing transaction if there is an exception
                        if (executingTransaction)
                            transaction.Rollback();

                        // Re-throw any captured exception, outer try will handle
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                OnLoggingException(ex);
            }
        }

        private void m_databaseErrorQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnLoggingException(e.Argument);
        }

        /// <summary>
        /// Raises the <see cref="LoggingException"/> event.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="LoggingException"/> event.</param>
        protected virtual void OnLoggingException(Exception exception)
        {
            if ((object)LoggingException != null)
                LoggingException(this, new EventArgs<Exception>(exception));
        }

        /// <summary>
        /// Default <see cref="Delegate"/> for <see cref="ErrorTextMethod"/>.
        /// </summary>
        private string GetErrorText()
        {
            StringBuilder errorText = new StringBuilder();

            errorText.AppendFormat("An unexpected exception has occurred in {0}. ", ApplicationName);
            errorText.Append("This may be due to an inconsistent system state or a programming error.");

            return errorText.ToString();
        }

        /// <summary>
        /// Default <see cref="Delegate"/> for <see cref="ScopeTextMethod"/>.
        /// </summary>
        private string GetScopeText()
        {
            StringBuilder scopeText = new StringBuilder();

            switch (ApplicationType)
            {
                case ApplicationType.WindowsCui:
                case ApplicationType.WindowsGui:
                    scopeText.Append("The action you requested was not performed.");
                    break;
                case ApplicationType.Web:
                    scopeText.Append("The current page will not load.");
                    break;
            }

            return scopeText.ToString();
        }

        /// <summary>
        /// Default <see cref="Delegate"/> for <see cref="ActionTextMethod"/>.
        /// </summary>
        private string GetActionText()
        {
            StringBuilder actionText = new StringBuilder();

            switch (ApplicationType)
            {
                case ApplicationType.WindowsCui:
                case ApplicationType.WindowsGui:
                    actionText.AppendFormat("Restart {0}, and try repeating your last action. ", ApplicationName);
                    break;
                case ApplicationType.Web:
                    actionText.AppendFormat("Close your browser, navigate back to the {0} website, and try repeating you last action. ", ApplicationName);
                    break;
            }

            actionText.Append("Try alternative methods of performing the same action. ");

            // Add information about the contact person if provided.
            if (!string.IsNullOrEmpty(m_contactName) && (!string.IsNullOrEmpty(m_contactEmail) || !string.IsNullOrEmpty(m_contactPhone)))
            {
                actionText.AppendFormat("If you need immediate assistance, contact {0} ", m_contactName);

                if (!string.IsNullOrEmpty(m_contactEmail))
                {
                    actionText.AppendFormat("via email at {0}", m_contactEmail);

                    if (!string.IsNullOrEmpty(m_contactPhone))
                        actionText.Append(" or ");
                }

                if (!string.IsNullOrEmpty(m_contactPhone))
                    actionText.AppendFormat("via phone at {0}", m_contactPhone);

                actionText.Append(".");
            }

            return actionText.ToString();
        }

        /// <summary>
        /// Default <see cref="Delegate"/> for <see cref="MoreInfoTextMethod"/>.
        /// </summary>
        protected string GetMoreInfoText()
        {
            string bullet = string.Empty;

            switch (ApplicationType)
            {
                case ApplicationType.WindowsCui:
                    bullet = "-";
                    break;
                case ApplicationType.Web:
                case ApplicationType.WindowsGui:
                    bullet = "•";
                    break;
            }

            StringBuilder moreInfoText = new StringBuilder();

            moreInfoText.Append("The following information about the error was automatically captured:");
            moreInfoText.AppendLine();
            moreInfoText.AppendLine();

            if (m_logToEventLog)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                moreInfoText.Append(m_logToEventLogOK ? "an event was written to the application log" : "an event could NOT be written to the application log");
                moreInfoText.AppendLine();
            }

            if (m_logToFile)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                moreInfoText.Append(m_logToFileOK ? "details were written to a text log at:" : "details could NOT be written to the text log at:");
                moreInfoText.AppendLine();
                moreInfoText.Append("   ");
                moreInfoText.Append(m_errorLog.Name);
                moreInfoText.AppendLine();
            }

            if (m_logToDatabase)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                moreInfoText.Append(m_logToDatabaseOK ? "details were queued to be written to the database log" : "details could NOT be queued to be written to the database log");
                moreInfoText.AppendLine();
            }

            if (m_logToEmail)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                moreInfoText.Append(m_logToEmailOK ? "an email has been sent to:" : "an email could NOT be sent to:");
                moreInfoText.AppendLine();
                moreInfoText.Append("   ");
                moreInfoText.Append(m_contactEmail);
                moreInfoText.AppendLine();
            }

            string extendedInfoText = GetExtendedMoreInfoText(bullet);

            if (!string.IsNullOrWhiteSpace(extendedInfoText))
                moreInfoText.Append(extendedInfoText);

            moreInfoText.AppendLine();
            moreInfoText.AppendLine();
            moreInfoText.Append("Detailed error information follows:");
            moreInfoText.AppendLine();
            moreInfoText.AppendLine();
            moreInfoText.Append(GetExceptionInfo(m_lastException, m_logUserInfo));

            return moreInfoText.ToString();
        }

        /// <summary>
        /// Allows other loggers to extend "more info text".
        /// </summary>
        /// <param name="bullet">Type of bullet to use for extended info text.</param>
        protected virtual string GetExtendedMoreInfoText(string bullet)
        {
            return null;
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Almost always (starting .NET 2.0) when the AppDomain.UnhandledException event is raised, .NET CLR 
            // will terminate the application after all the handlers for the event have been processed. So, if 
            // .NET CLR is going to terminate the application, we terminate the application before .NET CLR does
            // in order to prevent the .NET "Application has encountered a problem and needs to close" error.
            // See: http://msdn.microsoft.com/en-us/library/system.unhandledexceptioneventargs.isterminating.aspx
            m_suppressInteractiveLogging = false;
            Log(new Exception("UnhandledException", (Exception)e.ExceptionObject), e.IsTerminating);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets information about an <see cref="Exception"/> complete with system and application information.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> whose information is to be retrieved.</param>
        /// <param name="includeUserInfo">true if user information is to be include; otherwise false.</param>
        /// <returns><see cref="Exception"/> information in text.</returns>
        public static string GetExceptionInfo(Exception ex, bool includeUserInfo)
        {
            StringBuilder info = new StringBuilder();

            AggregateException aggregateException = ex as AggregateException;

            if ((object)aggregateException != null && (object)aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count > 0)
            {
                int aggregateCount = 1;

                foreach (Exception innerException in aggregateException.InnerExceptions)
                {
                    info.AppendFormat("(Aggregate Exception {0})", aggregateCount++);
                    info.AppendLine();
                    info.Append(GetExceptionInfo(innerException, includeUserInfo));
                }
            }
            else
            {
                if ((object)ex.InnerException != null)
                {
                    // Sometimes the original exception is wrapped in a more relevant outer exception
                    // the detail exception is the "inner" exception
                    // See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/exceptdotnet.asp
                    info.Append("(Inner Exception)");
                    info.AppendLine();
                    info.Append(GetExceptionInfo(ex.InnerException, includeUserInfo));
                    info.AppendLine();
                    info.Append("(Outer Exception)");
                    info.AppendLine();
                }

                // Get general system information.
                info.Append(GetSystemInfo(includeUserInfo));
                info.AppendLine();

                // Get general application information.
                info.Append(GetApplicationInfo());
                info.AppendLine();

                // Get general exception information.
                info.Append(GetExceptionGeneralInfo(ex));
                info.AppendLine();

                // Get the stack trace for the exception.
                info.Append("---- Stack Trace ----");
                info.AppendLine();
                info.Append(GetExceptionStackTrace(ex));
                info.AppendLine();
            }

            return info.ToString();
        }

        /// <summary>
        /// Gets information about the system where current application is executing.
        /// </summary>
        /// <param name="includeUserInfo">true if user information is to be include; otherwise false.</param>
        /// <returns>System information in text.</returns>
        private static string GetSystemInfo(bool includeUserInfo)
        {
            StringBuilder info = new StringBuilder();

            info.AppendFormat("Date and Time:         {0}", DateTime.Now);
            info.AppendLine();

            switch (Common.GetApplicationType())
            {
                case ApplicationType.Web:

                    info.AppendFormat("Server Name:           {0}", Environment.MachineName);
                    info.AppendLine();
                    info.AppendFormat("Server IP:             {0}", Dns.GetHostEntry(Environment.MachineName).AddressList[0]);
                    info.AppendLine();
                    info.AppendFormat("Server OS:             {0}", Environment.OSVersion.VersionString);
                    info.AppendLine();

                    if (includeUserInfo)
                    {
                        UserInfo remoteUserInfo = UserInfo.RemoteUserInfo;
                        info.AppendFormat("Process User:          {0}", UserInfo.CurrentUserID);
                        info.AppendLine();
                        info.AppendFormat("Remote User ID:        {0}", remoteUserInfo.LoginID);
                        info.AppendLine();
                        info.AppendFormat("Remote User Name:      {0}", remoteUserInfo.FullName);
                        info.AppendLine();
                        info.AppendFormat("Remote User Phone:     {0}", remoteUserInfo.Telephone);
                        info.AppendLine();
                        info.AppendFormat("Remote User Email:     {0}", remoteUserInfo.Email);
                        info.AppendLine();
                    }

                    info.AppendFormat("Remote Host:           {0}", HttpContext.Current.Request.ServerVariables["REMOTE_HOST"]);
                    info.AppendLine();
                    info.AppendFormat("Remote Address:        {0}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
                    info.AppendLine();
                    info.AppendFormat("HTTP Agent:            {0}", HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"]);
                    info.AppendLine();
                    info.AppendFormat("HTTP Referrer:         {0}", HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]);
                    info.AppendLine();
                    info.AppendFormat("Web Page URL:          {0}", HttpContext.Current.Request.Url);
                    info.AppendLine();

                    break;

                default:

                    info.AppendFormat("Machine Name:          {0}", Environment.MachineName);
                    info.AppendLine();
                    info.AppendFormat("Machine IP:            {0}", Dns.GetHostEntry(Environment.MachineName).AddressList[0]);
                    info.AppendLine();
                    info.AppendFormat("Machine OS:            {0}", Environment.OSVersion.VersionString);
                    info.AppendLine();

                    if (includeUserInfo)
                    {
                        UserInfo currentUserInfo = UserInfo.CurrentUserInfo;
                        info.AppendFormat("Current User ID:       {0}", currentUserInfo.LoginID);
                        info.AppendLine();
                        info.AppendFormat("Current User Name:     {0}", currentUserInfo.FullName);
                        info.AppendLine();
                        info.AppendFormat("Current User Phone:    {0}", currentUserInfo.Telephone);
                        info.AppendLine();
                        info.AppendFormat("Current User Email:    {0}", currentUserInfo.Email);
                        info.AppendLine();
                    }

                    break;
            }

            return info.ToString();
        }

        /// <summary>
        /// Gets information about the current application.
        /// </summary>
        /// <returns>Application information in text.</returns>
        private static string GetApplicationInfo()
        {
            Assembly parentAssembly;

            switch (Common.GetApplicationType())
            {
                case ApplicationType.Web:
                    // For a web site in .Net 2.0 we don't have an entry assembly. However, at this point the
                    // calling assembly will be consumer of this function (i.e. one of the web site DLLs).
                    // See: http://msdn.microsoft.com/msdnmag/issues/06/01/ExtremeASPNET/
                    parentAssembly = Assembly.GetCallingAssembly();
                    break;
                default:
                    // For a windows application the entry assembly will be the executable.
                    parentAssembly = Assembly.GetEntryAssembly();
                    break;
            }

            StringBuilder info = new StringBuilder();
            AssemblyInfo parentAssemblyInfo = new AssemblyInfo(parentAssembly);

            info.AppendFormat("Application Domain:    {0}", AppDomain.CurrentDomain.FriendlyName);
            info.AppendLine();
            info.AppendFormat("Assembly Codebase:     {0}", parentAssemblyInfo.CodeBase);
            info.AppendLine();
            info.AppendFormat("Assembly Full Name:    {0}", parentAssemblyInfo.FullName);
            info.AppendLine();
            info.AppendFormat("Assembly Version:      {0}", parentAssemblyInfo.Version);
            info.AppendLine();
            info.AppendFormat("Assembly Build Date:   {0}", parentAssemblyInfo.BuildDate.ToString());
            info.AppendLine();
            info.AppendFormat(".Net Runtime Version:  {0}", Environment.Version);
            info.AppendLine();

            return info.ToString();
        }

        /// <summary>
        /// Gets common information about an <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> whose common information is to be retrieved.</param>
        /// <returns>Common <see cref="Exception"/> information in text.</returns>
        private static string GetExceptionGeneralInfo(Exception ex)
        {
            StringBuilder info = new StringBuilder();

            info.AppendFormat("Exception Source:      {0}", ex.Source);
            info.AppendLine();
            info.AppendFormat("Exception Type:        {0}", ex.GetType().FullName);
            info.AppendLine();
            info.AppendFormat("Exception Message:     {0}", ex.Message);
            info.AppendLine();

            if ((object)ex.TargetSite != null)
            {
                info.AppendFormat("Exception Target Site: {0}", ex.TargetSite.Name);
                info.AppendLine();
            }

            return info.ToString();
        }

        /// <summary>
        /// Gets stack trace information about an <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> whose stack trace information is to be retrieved.</param>
        /// <returns><see cref="Exception"/> stack trace in text.</returns>
        private static string GetExceptionStackTrace(Exception ex)
        {
            StringBuilder trace = new StringBuilder();
            StackTrace stack = new StackTrace(ex, true);

            for (int i = 0; i < stack.FrameCount; i++)
            {
                StackFrame stackFrame = stack.GetFrame(i);
                MemberInfo method = stackFrame.GetMethod();
                string codeFileName = stackFrame.GetFileName();

                if (method == null) {
                    trace.AppendFormat("   Missing Method");
                    continue;
                }

                // Build method name.
                if ((object)method.DeclaringType != null)
                    trace.AppendFormat("   {0}.{1}.{2}", method.DeclaringType.Namespace, method.DeclaringType.Name, method.Name);

                // Build method parameters
                trace.Append("(");
                int parameterCount = 0;

                foreach (ParameterInfo parameter in stackFrame.GetMethod().GetParameters())
                {
                    parameterCount++;

                    if (parameterCount > 1)
                        trace.Append(", ");

                    trace.AppendFormat("{0} As {1}", parameter.Name, parameter.ParameterType.Name);
                }

                trace.Append(")");
                trace.AppendLine();

                // If source code is available, append location info.
                trace.Append("       ");

                if (!string.IsNullOrEmpty(codeFileName))
                {
                    trace.Append(FilePath.GetFileName(codeFileName));
                    trace.AppendFormat(": Ln {0:#0000}", stackFrame.GetFileLineNumber());
                    trace.AppendFormat(", Col {0:#00}", stackFrame.GetFileColumnNumber());

                    // If IL is available, append IL location info.
                    if (stackFrame.GetILOffset() != StackFrame.OFFSET_UNKNOWN)
                    {
                        trace.AppendFormat(", IL {0:#0000}", stackFrame.GetILOffset());
                    }
                }
                else
                {
                    try
                    {
                        trace.Append(FilePath.GetFileName(Assembly.GetEntryAssembly().CodeBase));
                    }
                    catch
                    {
                        trace.Append("(unknown file)");
                    }

                    // Native code offset is always available.
                    trace.AppendFormat(": N {0:#00000}", stackFrame.GetNativeOffset());
                }
                trace.AppendLine();
            }

            return trace.ToString();
        }

        #endregion
    }
}
