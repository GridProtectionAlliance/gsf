//*******************************************************************************************************
//  ErrorLogger.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR 2W-C
//       Phone: 423-751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/03/2007 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  09/17/2008 - Pinal C Patel
//       Converted code to C#.
//  10/16/2008 - Pinal C Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Windows.Forms;
using TVA.Configuration;
using TVA.Identity;
using TVA.IO;
using TVA.Net.Smtp;
using TVA.Reflection;
using TVA.Windows.Forms;

namespace TVA.ErrorManagement
{
    /// <summary>
    /// Implements a logger that can be used for logging handled as well as unhandled exceptions across multiple 
    /// application types (Windows Application, Console Application, Windows Service, Web Application, Web Service).
    /// </summary>
    /// <remarks>
    /// Adapted from exception handling code by Jeff Atwood of CodingHorror.com. Demo projects for handling unhandled
    /// exception in both windows and web environment by Jeff Atwood are available at The Code Project web site.
    /// See: http://www.codeproject.com/script/articles/list_articles.asp?userid=450027
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="ErrorLogger"/> component to log handled and unhandled exceptions:
    /// <code>
    /// using System;
    /// using System.IO;
    /// using TVA.ErrorManagement;
    ///
    /// class Program
    /// {
    ///     private static ErrorLogger m_logger;
    ///
    ///     static Program()
    ///     {
    ///         m_logger = new ErrorLogger();
    ///         m_logger.LogToUI = true;                    // Show exception info on the UI.
    ///         m_logger.LogToFile = true;                  // Log exception info to a file.
    ///         m_logger.LogToEmail = true;                 // Send exception info in an e-mail.
    ///         m_logger.LogToEventLog = true;              // Log exception info to the event log.
    ///         m_logger.LogToScreenshot = true;            // Take a screenshot of desktop on exception.
    ///         m_logger.ContactEmail = "dev@xyzcorp.com";  // Provide an e-mail address.
    ///         m_logger.HandleUnhandledException = true;   // Configure to handle unhandled exceptions.
    ///         m_logger.PersistSettings = true;            // Save settings to the config file.
    ///         m_logger.Initialize();                      // Initialize ErrorLogger component for use.
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
    ///             m_logger.SuppressInteractiveLogging();
    ///             m_logger.Log(ex);
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
    public class ErrorLogger : Component, ISupportLifecycle, ISupportInitialize, IPersistSettings
    {
        #region [ Members ]

        /// <summary>
        /// Default value for the <see cref="LogToUI"/> property.
        /// </summary>
        public const bool DefaultLogToUI = false;

        /// <summary>
        /// Default value for the <see cref="LogToFile"/> property.
        /// </summary>
        public const bool DefaultLogToFile = true;

        /// <summary>
        /// Default value for the <see cref="LogToEmail"/> property.
        /// </summary>
        public const bool DefaultLogToEmail = false;

        /// <summary>
        /// Default value for the <see cref="LogToEventLog"/> property.
        /// </summary>
        public const bool DefaultLogToEventLog = true;

        /// <summary>
        /// Default value for the <see cref="LogToScreenshot"/> property.
        /// </summary>
        public const bool DefaultLogToScreenshot = false;

        /// <summary>
        /// Default value for the <see cref="SmtpServer"/> property.
        /// </summary>
        public const string DefaultSmtpServer = Mail.DefaultSmtpServer;

        /// <summary>
        /// Default value for the <see cref="ContactName"/> property.
        /// </summary>
        public const string DefaultContactName = "";

        /// <summary>
        /// Default value for the <see cref="ContactEmail"/> property.
        /// </summary>
        public const string DefaultContactEmail = "";

        /// <summary>
        /// Default value for the <see cref="ContactPhone"/> property.
        /// </summary>
        public const string DefaultContactPhone = "";

        /// <summary>
        /// Default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ErrorLogger";

        /// <summary>
        /// Default value for the <see cref="HandleUnhandledException"/> property.
        /// </summary>
        public const bool DefaultHandleUnhandledException = true;

        /// <summary>
        /// Default value for the <see cref="ExitOnUnhandledException"/> property.
        /// </summary>
        public const bool DefaultExitOnUnhandledException = false;

        // Fields
        private bool m_logToUI;
        private bool m_logToFile;
        private bool m_logToEmail;
        private bool m_logToEventLog;
        private bool m_logToScreenshot;
        private string m_smtpServer;
        private string m_contactName;
        private string m_contactEmail;
        private string m_contactPhone;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private bool m_enabled;
        private bool m_handleUnhandledException;
        private bool m_exitOnUnhandledException;
        private LogFile m_errorLog;
        private Exception m_lastException;
        private ApplicationType m_appType;
        private Func<string> m_errorTextMethod;
        private Func<string> m_scopeTextMethod;
        private Func<string> m_actionTextMethod;
        private Func<string> m_moreInfoTextMethod;
        private List<Action<Exception>> m_loggers;
        private bool m_logToFileOK;
        private bool m_logToEmailOK;
        private bool m_logToEventLogOK;
        private bool m_logToScreenshotOK;
        private bool m_disposed;
        private bool m_initialized;
        private bool m_suppressInteractiveLogging;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLogger"/> class.
        /// </summary>
        public ErrorLogger()
        {
            m_errorLog = new LogFile();
            m_logToUI = DefaultLogToUI;
            m_logToFile = DefaultLogToFile;
            m_logToEmail = DefaultLogToEmail;
            m_logToEventLog = DefaultLogToEventLog;
            m_logToScreenshot = DefaultLogToScreenshot;
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
            // Initialize all logger methods.
            m_loggers = new List<Action<Exception>>();
            m_loggers.Add(ExceptionToScreenshot);
            m_loggers.Add(ExceptionToEventLog);
            m_loggers.Add(ExceptionToEmail);
            m_loggers.Add(ExceptionToFile);
            m_loggers.Add(ExceptionToUI);
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
        /// Gets or sets a boolean value that indicates whether a screenshot of the user's desktop is to be taken
        /// when an <see cref="Exception"/> is logged.
        /// </summary>
        /// <remarks>
        /// This setting is ignored in Web Application and Web Service application types.
        /// </remarks>
        [Category("Logging"),
        DefaultValue(DefaultLogToScreenshot),
        Description("Indicates whether a screenshot of the user's desktop is to be taken when an Exception is logged.")]
        public bool LogToScreenshot
        {
            get
            {
                return m_logToScreenshot;
            }
            set
            {
                m_logToScreenshot = value;
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
        /// Gets or sets the e-mail address where e-mail messages contaning <see cref="Exception"/> information are 
        /// to be sent when the <see cref="LogToEmail"/> property is set to true.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultContactEmail),
        Description("E-mail address where e-mail messages contaning Exception information are to be sent when the LogToEmail property is set to true.")]
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
        [Category("Persistance"),
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
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        [Category("Persistance"),
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
                    throw (new ArgumentNullException());

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
        /// Gets or sets the <see cref="Delegate"/> that provides common text stating what could have possibly 
        /// caused the <see cref="Exception"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null.</exception>
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
                if (value == null)
                    throw new ArgumentNullException();

                m_errorTextMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that provides text stating what is going to happen as a result 
        /// of the <see cref="Exception"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null.</exception>
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
                if (value == null)
                    throw new ArgumentNullException();

                m_scopeTextMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that provides text stating the action(s) that can be taken by 
        /// the end-user after an <see cref="Exception"/> is encountered.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null.</exception>
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
                if (value == null)
                    throw new ArgumentNullException();

                m_actionTextMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that provides text contaning detailed information about the 
        /// encountered <see cref="Exception"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null.</exception>
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
                if (value == null)
                    throw new ArgumentNullException();

                m_moreInfoTextMethod = value;
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
                        return FilePath.NoFileExtension(AppDomain.CurrentDomain.FriendlyName);
                    case ApplicationType.Web:
                        return HttpContext.Current.Request.ApplicationPath.Replace("/", "");
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
        /// Get the <see cref="LogFile"/> object used for logging <see cref="Exception"/> information to a file.
        /// </summary>
        [Browsable(false)]
        public LogFile ErrorLog
        {
            get
            {
                return m_errorLog;
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Logs information about the encountered <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">Encountered <see cref="Exception"/> whose information is to be logged.</param>
        public void Log(Exception ex)
        {
            Log(ex, false);
        }

        /// <summary>
        /// Logs information about the encountered <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">Encountered <see cref="Exception"/> whose information is to be logged.</param>
        /// <param name="exitApplication">true to exit the application; otherwise false.</param>
        public void Log(Exception ex, bool exitApplication)
        {
            Initialize();           // Initialize if uninitialized.

            m_lastException = ex;   // Save the encountered exception.

            // Iterate through all of the registered logger methods and invoke them for processing the exception.
            foreach (Action<Exception> logger in m_loggers)
            {
                try
                {
                    logger(ex);
                }
                catch
                {
                    // Absorb any exception.
                }
            }
            m_suppressInteractiveLogging = false;   // Enable interactive logging if disabled.

            // Exit the current application if specified.
            if (exitApplication)
            {
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
        /// Disables <see cref="Loggers"/> that require interaction either directly or indirectly when logging 
        /// handled <see cref="Exception"/> using the <see cref="Log(Exception)"/> method. All loggers are enabled
        /// automatically after a handled <see cref="Exception"/> has been logged.
        /// </summary>
        public void SuppressInteractiveLogging()
        {
            m_suppressInteractiveLogging = true;
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
                LoadSettings();                                 // Load settings from the config file.
                Register();                                     // Register the logger for unhandled exceptions.
                m_errorLog.Name = GetDefaultLogFileName();      // Assign a default filename for the error log.
                m_errorLog.EndInit();                           // Initialize the log file. // TODO: Remove this!
                m_errorLog.Open();                              // Open the log file.
                m_initialized = true;                           // Initialize only once.
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="ErrorLogger"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["LogToUI", true].Update(m_logToUI, "True if an encountered exception is to be logged to the User Interface; otherwise False.");
                settings["LogToFile", true].Update(m_logToFile, "True if an encountered exception is to be logged to a file; otherwise False.");
                settings["LogToEmail", true].Update(m_logToEmail, "True if an email is to be sent to ContactEmail with the details of an encountered exception; otherwise False.");
                settings["LogToEventLog", true].Update(m_logToEventLog, "True if an encountered exception is to be logged to the Event Log; otherwise False.");
                settings["LogToScreenshot", true].Update(m_logToScreenshot, "True if a screenshot is to be taken when an exception is encountered; otherwise False.");
                settings["SmtpServer", true].Update(m_smtpServer, "Name of the SMTP server to be used for sending the email messages.");
                settings["ContactName", true].Update(m_contactName, "Name of the person that the end-user can contact when an exception is encountered.");
                settings["ContactEmail", true].Update(m_contactEmail, "Comma-seperated list of recipient email addresses for the email message.");
                settings["ContactPhone", true].Update(m_contactPhone, "Phone number of the person that the end-user can contact when an exception is encountered.");
                settings["HandleUnhandledException", true].Update(m_handleUnhandledException, "True if unhandled exceptions are to be handled automatically; otherwise False.");
                settings["ExitOnUnhandledException", true].Update(m_exitOnUnhandledException, "True if the application must exit when an unhandled exception is encountered; otherwise False.");
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="ErrorLogger"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                LogToUI = settings["LogToUI", true].ValueAs(m_logToUI);
                LogToFile = settings["LogToFile", true].ValueAs(m_logToFile);
                LogToEmail = settings["LogToEmail", true].ValueAs(m_logToEmail);
                LogToEventLog = settings["LogToEventLog", true].ValueAs(m_logToEventLog);
                LogToScreenshot = settings["LogToScreenshot", true].ValueAs(m_logToScreenshot);
                SmtpServer = settings["SmtpServer", true].ValueAs(m_smtpServer);
                ContactEmail = settings["ContactEmail", true].ValueAs(m_contactEmail);
                ContactName = settings["ContactName", true].ValueAs(m_contactName);
                ContactPhone = settings["ContactPhone", true].ValueAs(m_contactPhone);
                HandleUnhandledException = settings["HandleUnhandledException", true].ValueAs(m_handleUnhandledException);
                ExitOnUnhandledException = settings["ExitOnUnhandledException", true].ValueAs(m_exitOnUnhandledException);
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
            // Nothing needs to be done before component is initialized.
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
                Initialize();
            }
        }

        /// <summary>
        /// Registers the <see cref="ErrorLogger"/> object to handle unhandled <see cref="Exception"/> if the 
        /// <see cref="HandleUnhandledException"/> property is set to true.
        /// </summary>
        protected virtual void Register()
        {
            if (m_handleUnhandledException && !Debugger.IsAttached)
            {
                // For Windows Application.
                Application.ThreadException += ThreadException;
                // For Windows Service and Console Application.
                AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            }
        }

        /// <summary>
        /// Unregister the <see cref="ErrorLogger"/> object from handling unhandled <see cref="Exception"/>.
        /// </summary>
        protected virtual void Unregister()
        {
            if (m_handleUnhandledException && !Debugger.IsAttached)
            {
                Application.ThreadException -= ThreadException;
                AppDomain.CurrentDomain.UnhandledException -= UnhandledException;
            }
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
            // Use the ErrorDialog to show exception information.
            ErrorDialog dialog = new ErrorDialog();
            dialog.Text = string.Format(dialog.Text, ApplicationName, null);
            dialog.PictureBoxIcon.Image = SystemIcons.Error.ToBitmap();
            dialog.RichTextBoxError.Text = m_errorTextMethod();
            dialog.RichTextBoxScope.Text = m_scopeTextMethod();
            dialog.RichTextBoxAction.Text = m_actionTextMethod();
            dialog.RichTextBoxMoreInfo.Text = m_moreInfoTextMethod();

            dialog.ShowDialog();
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
                m_errorLog.WriteTimestampedLine(GetExceptionInfo(exception));
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
                email.Body = GetExceptionInfo(exception);
                email.Attachments = GetScreenshotFileName();
                email.SmtpServer = m_smtpServer;
                email.Send();
                m_logToEmailOK = true;
            }
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
                EventLog.WriteEntry(ApplicationName, GetExceptionInfo(exception), EventLogEntryType.Error);
                m_logToEventLogOK = true;
            }
        }

        /// <summary>
        /// Takes a screenshot of the user's desktop when the <see cref="Exception"/> is encountered.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> that was encountered.</param>
        protected virtual void ExceptionToScreenshot(Exception exception)
        {
            // Log if enabled.
            if (m_logToScreenshot && 
                (ApplicationType == ApplicationType.WindowsCui || ApplicationType == ApplicationType.WindowsGui))
            {
                m_logToScreenshotOK = false;
                using (Bitmap screenshot = ScreenArea.Capture(ImageFormat.Png))
                {
                    screenshot.Save(GetScreenshotFileName());
                }
                m_logToScreenshotOK = true;
            }
        }

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
                    SaveSettings();
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_errorLog != null)
                            m_errorLog.Dispose();
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
            if (!string.IsNullOrEmpty(m_contactName) &&
                (!string.IsNullOrEmpty(m_contactPhone) || !string.IsNullOrEmpty(m_contactPhone)))
            {
                actionText.AppendFormat("If you need immediate assistance, contact {0} ", m_contactName);
                if (!string.IsNullOrEmpty(m_contactEmail))
                {
                    actionText.AppendFormat("via email at {0}", m_contactEmail);
                    if (!string.IsNullOrEmpty(m_contactPhone))
                    {
                        actionText.Append(" or ");
                    }
                }
                if (!string.IsNullOrEmpty(m_contactPhone))
                {
                    actionText.AppendFormat("via phone at {0}", m_contactPhone);
                }
                actionText.Append(".");
            }

            return actionText.ToString();
        }

        /// <summary>
        /// Default <see cref="Delegate"/> for <see cref="MoreInfoTextMethod"/>.
        /// </summary>
        private string GetMoreInfoText()
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
            if (m_logToScreenshot)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                if (m_logToScreenshotOK)
                {
                    moreInfoText.Append("a screenshot was taken of the desktop at:");
                    moreInfoText.AppendLine();
                    moreInfoText.Append("   ");
                    moreInfoText.Append(GetScreenshotFileName());
                }
                else
                {
                    moreInfoText.Append("a screenshot could NOT be taken of the desktop.");
                }
                moreInfoText.AppendLine();
            }
            if (m_logToEventLog)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                if (m_logToEventLogOK)
                {
                    moreInfoText.Append("an event was written to the application log");
                }
                else
                {
                    moreInfoText.Append("an event could NOT be written to the application log");
                }
                moreInfoText.AppendLine();
            }
            if (m_logToFile)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                if (m_logToFileOK)
                {
                    moreInfoText.Append("details were written to a text log at:");
                }
                else
                {
                    moreInfoText.Append("details could NOT be written to the text log at:");
                }
                moreInfoText.AppendLine();
                moreInfoText.Append("   ");
                moreInfoText.Append(m_errorLog.Name);
                moreInfoText.AppendLine();
            }
            if (m_logToEmail)
            {
                moreInfoText.AppendFormat(" {0} ", bullet);
                if (m_logToEmailOK)
                {
                    moreInfoText.Append("an email has been sent to:");
                }
                else
                {
                    moreInfoText.Append("an email could NOT be sent to:");
                }
                moreInfoText.AppendLine();
                moreInfoText.Append("   ");
                moreInfoText.Append(m_contactEmail);
                moreInfoText.AppendLine();
            }
            moreInfoText.AppendLine();
            moreInfoText.AppendLine();
            moreInfoText.Append("Detailed error information follows:");
            moreInfoText.AppendLine();
            moreInfoText.AppendLine();
            moreInfoText.Append(GetExceptionInfo(m_lastException));

            return moreInfoText.ToString();
        }

        private string GetDefaultLogFileName()
        {
            return FilePath.AbsolutePath(ApplicationName + ".ErrorLog.txt");
        }

        private string GetScreenshotFileName()
        {
            return FilePath.AbsolutePath(ApplicationName + ".ErrorState.png");
        }

        private void ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            m_suppressInteractiveLogging = false;
            Log(new Exception("ThreadException", e.Exception), m_exitOnUnhandledException);
        }

        private void UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
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

        /// <summary>
        /// Gets information about the system where current application is executing.
        /// </summary>
        /// <returns>System information in text.</returns>
        public static string GetSystemInfo()
        {
            StringBuilder info = new StringBuilder();
            info.AppendFormat("Date and Time:         {0}", DateTime.Now);
            info.AppendLine();
            switch (Common.GetApplicationType())
            {
                case ApplicationType.WindowsCui:
                case ApplicationType.WindowsGui:
                    UserInfo currentUserInfo = UserInfo.CurrentUserInfo;
                    info.AppendFormat("Machine Name:          {0}", Environment.MachineName);
                    info.AppendLine();
                    info.AppendFormat("Machine IP:            {0}", Dns.GetHostEntry(Environment.MachineName).AddressList[0].ToString());
                    info.AppendLine();
                    info.AppendFormat("Current User ID:       {0}", currentUserInfo.LoginID);
                    info.AppendLine();
                    info.AppendFormat("Current User Name:     {0}", currentUserInfo.FullName);
                    info.AppendLine();
                    info.AppendFormat("Current User Phone:    {0}", currentUserInfo.Telephone);
                    info.AppendLine();
                    info.AppendFormat("Current User Email:    {0}", currentUserInfo.Email);
                    info.AppendLine();
                    break;
                case ApplicationType.Web:
                    UserInfo remoteUserInfo = UserInfo.RemoteUserInfo;
                    info.AppendFormat("Server Name:           {0}", Environment.MachineName);
                    info.AppendLine();
                    info.AppendFormat("Server IP:             {0}", Dns.GetHostEntry(Environment.MachineName).AddressList[0].ToString());
                    info.AppendLine();
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
                    info.AppendFormat("Remote Host:           {0}", HttpContext.Current.Request.ServerVariables["REMOTE_HOST"]);
                    info.AppendLine();
                    info.AppendFormat("Remote Address:        {0}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
                    info.AppendLine();
                    info.AppendFormat("HTTP Agent:            {0}", HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"]);
                    info.AppendLine();
                    info.AppendFormat("HTTP Referer:          {0}", HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]);
                    info.AppendLine();
                    info.AppendFormat("Web Page URL:          {0}", HttpContext.Current.Request.Url.ToString());
                    info.AppendLine();
                    break;
            }

            return info.ToString();
        }

        /// <summary>
        /// Gets information about the current application.
        /// </summary>
        /// <returns>Application information in text.</returns>
        public static string GetApplicationInfo()
        {
            Assembly parentAssembly = null;
            switch (Common.GetApplicationType())
            {
                case ApplicationType.WindowsCui:
                case ApplicationType.WindowsGui:
                    // For a windows application the entry assembly will be the executable.
                    parentAssembly = Assembly.GetEntryAssembly();
                    break;
                case ApplicationType.Web:
                    // For a web site in .Net 2.0 we don't have an entry assembly. However, at this point the
                    // calling assembly will be consumer of this function (i.e. one of the web site DLLs).
                    // See: http://msdn.microsoft.com/msdnmag/issues/06/01/ExtremeASPNET/
                    parentAssembly = Assembly.GetCallingAssembly();
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
            info.AppendFormat("Assembly Version:      {0}", parentAssemblyInfo.Version.ToString());
            info.AppendLine();
            info.AppendFormat("Assembly Build Date:   {0}", parentAssemblyInfo.BuildDate.ToString());
            info.AppendLine();
            info.AppendFormat(".Net Runtime Version:  {0}", Environment.Version.ToString());
            info.AppendLine();

            return info.ToString();
        }

        /// <summary>
        /// Gets information about an <see cref="Exception"/> complete with system and application information.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> whose information is to be retrieved.</param>
        /// <returns><see cref="Exception"/> information in text.</returns>
        public static string GetExceptionInfo(Exception ex)
        {
            StringBuilder info = new StringBuilder();
            if (ex.InnerException != null)
            {
                // Sometimes the original exception is wrapped in a more relevant outer exception
                // the detail exception is the "inner" exception
                // See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/exceptdotnet.asp
                info.Append("(Inner Exception)");
                info.AppendLine();
                info.Append(GetExceptionInfo(ex.InnerException));
                info.AppendLine();
                info.Append("(Outer Exception)");
                info.AppendLine();
            }

            // Get general system information.
            info.Append(GetSystemInfo());
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

            return info.ToString();
        }

        /// <summary>
        /// Gets common information about an <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> whose common information is to be retrieved.</param>
        /// <returns>Common <see cref="Exception"/> information in text.</returns>
        public static string GetExceptionGeneralInfo(Exception ex)
        {
            StringBuilder info = new StringBuilder();
            info.AppendFormat("Exception Source:      {0}", ex.Source);
            info.AppendLine();
            info.AppendFormat("Exception Type:        {0}", ex.GetType().FullName);
            info.AppendLine();
            info.AppendFormat("Exception Message:     {0}", ex.Message);
            info.AppendLine();
            if (ex.TargetSite != null)
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
        public static string GetExceptionStackTrace(Exception ex)
        {
            StringBuilder trace = new StringBuilder();
            StackTrace stack = new StackTrace(ex, true);
            for (int i = 0; i <= stack.FrameCount - 1; i++)
            {
                StackFrame stackFrame = stack.GetFrame(i);
                MemberInfo method = stackFrame.GetMethod();
                string codeFileName = stackFrame.GetFileName();

                // Build method name.
                trace.AppendFormat("   {0}.{1}.{2}", method.DeclaringType.Namespace, method.DeclaringType.Name, method.Name);

                // Build method params.
                trace.Append("(");
                int parameterCount = 0;
                foreach (ParameterInfo parameter in stackFrame.GetMethod().GetParameters())
                {
                    parameterCount++;
                    if (parameterCount > 1)
                    {
                        trace.Append(", ");
                    }
                    trace.AppendFormat("{0} As {1}", parameter.Name, parameter.ParameterType.Name);
                }
                trace.Append(")");
                trace.AppendLine();

                // If source code is available, append location info.
                trace.Append("       ");

                if (!string.IsNullOrEmpty(codeFileName))
                {
                    trace.Append(Path.GetFileName(codeFileName));
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
                    ApplicationType appType = Common.GetApplicationType();
                    if (appType == ApplicationType.WindowsCui || appType == ApplicationType.WindowsGui)
                    {
                        trace.Append(Path.GetFileName(Assembly.GetEntryAssembly().CodeBase));
                    }
                    else
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
