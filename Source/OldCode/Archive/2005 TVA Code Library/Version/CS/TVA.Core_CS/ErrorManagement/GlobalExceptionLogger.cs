using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Net;
using System.Web;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using TVA.Net.Smtp;
using TVA.IO;
using TVA.Identity;
using TVA.Configuration;

//*******************************************************************************************************
//  TVA.ErrorManagement.GlobalExceptionLogger.vb - Global exception logger for windows and web apps
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/03/2007 - Pinal C. Patel
//       Original version of source code generated
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//
//*******************************************************************************************************



namespace TVA
{
    namespace ErrorManagement
    {

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// Adapted from exception handling code by Jeff Atwood of CodingHorror.com. Demo projects for handling unhandled
        /// exception in both windows and web environment by Jeff Atwood are available at The Code Project web site.
        /// See: http://www.codeproject.com/script/articles/list_articles.asp?userid=450027
        /// </remarks>
        [ToolboxBitmap(typeof(GlobalExceptionLogger))]
        public partial class GlobalExceptionLogger : IPersistSettings, ISupportInitialize
        {


            #region " Variables "

            private bool m_autoRegister;
            private bool m_exitOnUnhandledException;
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
            private string m_settingsCategoryName;
            private System.Reflection.Assembly m_parentAssembly;
            private UITextMethodSignature m_errorTextMethod;
            private UITextMethodSignature m_scopeTextMethod;
            private UITextMethodSignature m_actionTextMethod;
            private UITextMethodSignature m_moreInfoTextMethod;
            private List<LoggerMethodSignature> m_loggers;

            private bool m_logToFileOK;
            private bool m_logToEmailOK;
            private bool m_logToPhoneOK;
            private bool m_logToEventLogOK;
            private bool m_logToScreenshotOK;
            private Exception m_lastException;
            private ApplicationType m_applicationType;

            #endregion

            #region " Constants "

            /// <summary>
            /// Default value for AutoRegister property.
            /// </summary>
            public const bool DefaultAutoRegister = true;

            /// <summary>
            /// Default value for ExitOnUnhandledException property.
            /// </summary>
            public const bool DefaultExitOnUnhandledException = false;

            /// <summary>
            /// Default value for LogToUI property.
            /// </summary>
            public const bool DefaultLogToUI = false;

            /// <summary>
            /// Default value for LogToFile property.
            /// </summary>
            public const bool DefaultLogToFile = true;

            /// <summary>
            /// Default value for LogToEmail property.
            /// </summary>
            public const bool DefaultLogToEmail = false;

            /// <summary>
            /// Default value for LogToEventLog property.
            /// </summary>
            public const bool DefaultLogToEventLog = true;

            /// <summary>
            /// Default value for LogToScreenshot property.
            /// </summary>
            public const bool DefaultLogToScreenshot = false;

            /// <summary>
            /// Default value for SmtpServer property.
            /// </summary>
            public const string DefaultSmtpServer = "mailhost.cha.tva.gov";

            /// <summary>
            /// Default value for ContactName property.
            /// </summary>
            public const string DefaultContactName = "";

            /// <summary>
            /// Default value for ContactEmail property.
            /// </summary>
            public const string DefaultContactEmail = "";

            /// <summary>
            /// Default value for ContactPhone property.
            /// </summary>
            public const string DefaultContactPhone = "";

            /// <summary>
            /// Default value for PersistSettings property.
            /// </summary>
            public const bool DefaultPersistSettings = false;

            /// <summary>
            /// Default value for SettingsCategoryName property.
            /// </summary>
            public const string DefaultSettingsCategoryName = "ErrorLogger";

            #endregion

            #region " Delegates "

            public delegate string UITextMethodSignature();
            public delegate void LoggerMethodSignature(Exception ex);

            #endregion

            #region " Properties "

            [Category("Behavior"), DefaultValue(DefaultAutoRegister)]
            public bool AutoRegister
            {
                get
                {
                    return m_autoRegister;
                }
                set
                {
                    m_autoRegister = value;
                }
            }

            [Category("Behavior"), DefaultValue(DefaultExitOnUnhandledException)]
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

            [Category("Logging"), DefaultValue(DefaultLogToUI)]
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

            [Category("Logging"), DefaultValue(DefaultLogToFile)]
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

            [Category("Logging"), DefaultValue(DefaultLogToEmail)]
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

            [Category("Logging"), DefaultValue(DefaultLogToEventLog)]
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

            [Category("Logging"), DefaultValue(DefaultLogToScreenshot)]
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

            [Category("Settings"), DefaultValue(DefaultSmtpServer)]
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

            [Category("Settings"), DefaultValue(DefaultContactName)]
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

            [Category("Settings"), DefaultValue(DefaultContactEmail)]
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

            [Category("Settings"), DefaultValue(DefaultContactPhone)]
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

            [Category("Persistance"), DefaultValue(DefaultPersistSettings)]
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

            [Category("Persistance"), DefaultValue(DefaultSettingsCategoryName)]
            public string SettingsCategoryName
            {
                get
                {
                    return m_settingsCategoryName;
                }
                set
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        m_settingsCategoryName = value;
                    }
                    else
                    {
                        throw (new ArgumentNullException("SettingsCategoryName"));
                    }
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public System.Reflection.Assembly ParentAssembly
            {
                get
                {
                    return m_parentAssembly;
                }
                set
                {
                    m_parentAssembly = value;
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public UITextMethodSignature ErrorTextMethod
            {
                get
                {
                    return m_errorTextMethod;
                }
                set
                {
                    m_errorTextMethod = value;
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public UITextMethodSignature ScopeTextMethod
            {
                get
                {
                    return m_scopeTextMethod;
                }
                set
                {
                    m_scopeTextMethod = value;
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public UITextMethodSignature ActionTextMethod
            {
                get
                {
                    return m_actionTextMethod;
                }
                set
                {
                    m_actionTextMethod = value;
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public UITextMethodSignature MoreInfoTextMethod
            {
                get
                {
                    return m_moreInfoTextMethod;
                }
                set
                {
                    m_moreInfoTextMethod = value;
                }
            }

            [Browsable(false)]
            public string ApplicationName
            {
                get
                {
                    switch (ApplicationType)
                    {
                        case TVA.ApplicationType.WindowsCui:
                        case TVA.ApplicationType.WindowsGui:
                            return NoFileExtension(AppDomain.CurrentDomain.FriendlyName);
                        case TVA.ApplicationType.Web:
                            return HttpContext.Current.Request.ApplicationPath.Replace("/", "");
                        default:
                            return "";
                    }
                }
            }

            [Browsable(false)]
            public ApplicationType ApplicationType
            {
                get
                {
                    if (m_applicationType == TVA.ApplicationType.Unknown)
                    {
                        m_applicationType = TVA.Common.GetApplicationType();
                    }
                    return m_applicationType;
                }
            }

            [Browsable(false)]
            public LogFile LogFile
            {
                get
                {
                    return _LogFile;
                }
            }

            [Browsable(false)]
            public Exception LastException
            {
                get
                {
                    return m_lastException;
                }
            }

            [Browsable(false)]
            public List<LoggerMethodSignature> Loggers
            {
                get
                {
                    return m_loggers;
                }
            }

            private string LogFileName
            {
                get
                {
                    return AbsolutePath(ApplicationName + ".ExceptionLog.txt");
                }
            }

            private string ScreenshotFileName
            {
                get
                {
                    return AbsolutePath(ApplicationName + ".ExceptionScreenshot.png");
                }
            }

            #endregion

            #region " Methods "

            public void Register()
            {

                if (!Debugger.IsAttached)
                {
                    // For winform applications.
                    System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(UnhandledThreadException);

                    // For console applications.
                    System.AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(UnhandledException);
                }

            }

            public void Unregister()
            {

                if (!Debugger.IsAttached)
                {
                    System.Windows.Forms.Application.ThreadException -= new System.Threading.ThreadExceptionEventHandler(UnhandledThreadException);
                    System.AppDomain.CurrentDomain.UnhandledException -= new System.UnhandledExceptionEventHandler(UnhandledException);
                }

            }

            public void Log(Exception ex)
            {

                Log(ex, false);

            }

            public void Log(Exception ex, bool exitApplication)
            {

                HandleException(ex, exitApplication);

            }

            public void LoadSettings()
            {

                try
                {
                    CategorizedSettingsElementCollection with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
                    if (with_1.Count > 0)
                    {
                        AutoRegister = with_1.Item("AutoRegister").GetTypedValue(m_autoRegister);
                        LogToUI = with_1.Item("LogToUI").GetTypedValue(m_logToUI);
                        LogToFile = with_1.Item("LogToFile").GetTypedValue(m_logToFile);
                        LogToEmail = with_1.Item("LogToEmail").GetTypedValue(m_logToEmail);
                        LogToEventLog = with_1.Item("LogToEventLog").GetTypedValue(m_logToEventLog);
                        LogToScreenshot = with_1.Item("LogToScreenshot").GetTypedValue(m_logToScreenshot);
                        SmtpServer = with_1.Item("SmtpServer").GetTypedValue(m_smtpServer);
                        ContactEmail = with_1.Item("ContactEmail").GetTypedValue(m_contactEmail);
                        ContactName = with_1.Item("ContactName").GetTypedValue(m_contactName);
                        ContactPhone = with_1.Item("ContactPhone").GetTypedValue(m_contactPhone);
                        ExitOnUnhandledException = with_1.Item("ExitOnUnhandledException").GetTypedValue(m_exitOnUnhandledException);
                    }
                }
                catch
                {
                    // We'll encounter exceptions if the settings are not present in the config file.
                }

            }

            public void SaveSettings()
            {

                if (m_persistSettings)
                {
                    try
                    {
                        CategorizedSettingsElementCollection with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
                        with_1.Clear();
                        object with_2 = with_1.Item("AutoRegister", true);
                        with_2.Value = m_autoRegister.ToString();
                        with_2.Description = "True if the logger is to be automatically registered for handling unhandled exceptions after initialization is complete; otherwise False.";
                        object with_3 = with_1.Item("LogToUI", true);
                        with_3.Value = m_logToUI.ToString();
                        with_3.Description = "True if an encountered exception is to be logged to the User Interface; otherwise False.";
                        object with_4 = with_1.Item("LogToFile", true);
                        with_4.Value = m_logToFile.ToString();
                        with_4.Description = "True if an encountered exception is to be logged to a file; otherwise False.";
                        object with_5 = with_1.Item("LogToEmail", true);
                        with_5.Value = m_logToEmail.ToString();
                        with_5.Description = "True if an email is to be sent to ContactEmail with the details of an encountered exception; otherwise False.";
                        object with_6 = with_1.Item("LogToEventLog", true);
                        with_6.Value = m_logToEventLog.ToString();
                        with_6.Description = "True if an encountered exception is to be logged to the Event Log; otherwise False.";
                        object with_7 = with_1.Item("LogToScreenshot", true);
                        with_7.Value = m_logToScreenshot.ToString();
                        with_7.Description = "True if a screenshot is to be taken when an exception is encountered; otherwise False.";
                        object with_8 = with_1.Item("SmtpServer", true);
                        with_8.Value = m_smtpServer;
                        with_8.Description = "Name of the SMTP server to be used for sending the email message.";
                        object with_9 = with_1.Item("ContactName", true);
                        with_9.Value = m_contactName;
                        with_9.Description = "Name of the person that the end-user can contact when an exception is encountered.";
                        object with_10 = with_1.Item("ContactEmail", true);
                        with_10.Value = m_contactEmail;
                        with_10.Description = "Comma-seperated list of recipient email addresses for the email message.";
                        object with_11 = with_1.Item("ContactPhone", true);
                        with_11.Value = m_contactPhone;
                        with_11.Description = "Phone number of the person that the end-user can contact when an exception is encountered.";
                        object with_12 = with_1.Item("ExitOnUnhandledException", true);
                        with_12.Value = m_exitOnUnhandledException.ToString();
                        with_12.Description = "True if the application must exit when an unhandled exception is encountered; otherwise False.";
                        TVA.Configuration.Common.SaveSettings();
                    }
                    catch
                    {
                        // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                    }
                }

            }

            public void BeginInit()
            {

            }

            public void EndInit()
            {

                if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                {
                    LoadSettings(); // Load settings from the config file.
                    if (m_autoRegister)
                    {
                        Register(); // Start the logger automatically if specified.
                    }
                    m_parentAssembly = System.Reflection.Assembly.GetCallingAssembly();
                }

            }

            private void HandleException(Exception ex, bool exitApplication)
            {

                m_lastException = ex;

                try
                {
                    if (!m_loggers.Contains(new System.EventHandler(ExceptionToScreenshot)))
                    {
                        m_loggers.Add(new System.EventHandler(ExceptionToScreenshot));
                    }
                    if (!m_loggers.Contains(new System.EventHandler(ExceptionToEventLog)))
                    {
                        m_loggers.Add(new System.EventHandler(ExceptionToEventLog));
                    }
                    if (!m_loggers.Contains(new System.EventHandler(ExceptionToEmail)))
                    {
                        m_loggers.Add(new System.EventHandler(ExceptionToEmail));
                    }
                    if (!m_loggers.Contains(new System.EventHandler(ExceptionToFile)))
                    {
                        m_loggers.Add(new System.EventHandler(ExceptionToFile));
                    }
                    if (!m_loggers.Contains(new System.EventHandler(ExceptionToUI)))
                    {
                        m_loggers.Add(new System.EventHandler(ExceptionToUI));
                    }
                }
                catch
                {

                }

                foreach (LoggerMethodSignature logger in m_loggers)
                {
                    try
                    {
                        logger.Invoke(ex);
                    }
                    catch
                    {

                    }
                }

                if (exitApplication && (ApplicationType == TVA.ApplicationType.WindowsCui || ApplicationType == TVA.ApplicationType.WindowsGui))
                {
                    Application.Exit();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }

            }

            private void ExceptionToUI(Exception exception)
            {

                if (m_logToUI)
                {
                    try
                    {
                        switch (ApplicationType)
                        {
                            case TVA.ApplicationType.WindowsCui:
                                ExceptionToWindowsCui();
                                break;
                            case TVA.ApplicationType.WindowsGui:
                                ExceptionToWindowsGui();
                                break;
                            case TVA.ApplicationType.Web:
                                ExceptionToWebPage();
                                break;
                        }
                    }
                    catch
                    {

                    }
                }

            }

            private void ExceptionToWindowsGui()
            {

                object with_1 = new GelDialog();
                with_1.Text = string.Format(with_1.Text, ApplicationName, null);
                with_1.PictureBoxIcon.Image = System.Drawing.SystemIcons.Error.ToBitmap();
                with_1.RichTextBoxError.Text = m_errorTextMethod;
                with_1.RichTextBoxScope.Text = m_scopeTextMethod;
                with_1.RichTextBoxAction.Text = m_actionTextMethod;
                with_1.RichTextBoxMoreInfo.Text = m_moreInfoTextMethod;

                with_1.ShowDialog();

            }

            private void ExceptionToWindowsCui()
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                with_1.AppendFormat("{0} has encountered a problem", ApplicationName);
                with_1.AppendLine();
                with_1.AppendLine();
                with_1.Append("What happened:");
                with_1.AppendLine();
                with_1.Append(m_errorTextMethod);
                with_1.AppendLine();
                with_1.AppendLine();
                with_1.Append("How this will affect you:");
                with_1.AppendLine();
                with_1.Append(m_scopeTextMethod);
                with_1.AppendLine();
                with_1.AppendLine();
                with_1.Append("What you can do about it:");
                with_1.AppendLine();
                with_1.Append(m_actionTextMethod);
                with_1.AppendLine();
                with_1.AppendLine();
                with_1.Append("More information:");
                with_1.AppendLine();
                with_1.Append(m_moreInfoTextMethod);
                with_1.AppendLine();

                System.Console.Write(with_1.ToString());

            }

            private void ExceptionToWebPage()
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                with_1.Append("<HTML>");
                with_1.AppendLine();
                with_1.Append("<HEAD>");
                with_1.AppendLine();
                with_1.Append("<TITLE>");
                with_1.AppendLine();
                with_1.AppendFormat("{0} has encountered a problem", ApplicationName);
                with_1.AppendLine();
                with_1.Append("</TITLE>");
                with_1.AppendLine();
                with_1.Append("<STYLE>");
                with_1.AppendLine();
                with_1.Append("body {font-family:\"Verdana\";font-weight:normal;font-size: .7em;color:black; background-color:white;}");
                with_1.AppendLine();
                with_1.Append("b {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}");
                with_1.AppendLine();
                with_1.Append("H1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }");
                with_1.AppendLine();
                with_1.Append("H2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }");
                with_1.AppendLine();
                with_1.Append("pre {font-family:\"Lucida Console\";font-size: .9em}");
                with_1.AppendLine();
                with_1.Append("</STYLE>");
                with_1.AppendLine();
                with_1.Append("</HEAD>");
                with_1.AppendLine();
                with_1.Append("<BODY>");
                with_1.AppendLine();
                with_1.Append("<H1>");
                with_1.AppendLine();
                with_1.AppendFormat("The {0} website has encountered a problem", ApplicationName);
                with_1.AppendLine();
                with_1.Append("<hr width=100% size=1 color=silver></H1>");
                with_1.AppendLine();
                with_1.Append("<H2>What Happened</H2>");
                with_1.AppendLine();
                with_1.Append("<BLOCKQUOTE>");
                with_1.AppendLine();
                with_1.Append(m_errorTextMethod);
                with_1.AppendLine();
                with_1.Append("</BLOCKQUOTE>");
                with_1.AppendLine();
                with_1.Append("<H2>How this will affect you</H2>");
                with_1.AppendLine();
                with_1.Append("<BLOCKQUOTE>");
                with_1.AppendLine();
                with_1.Append(m_scopeTextMethod);
                with_1.AppendLine();
                with_1.Append("</BLOCKQUOTE>");
                with_1.AppendLine();
                with_1.Append("<H2>What you can do about it</H2>");
                with_1.AppendLine();
                with_1.Append("<BLOCKQUOTE>");
                with_1.AppendLine();
                with_1.Append(m_actionTextMethod);
                with_1.AppendLine();
                with_1.Append("</BLOCKQUOTE>");
                with_1.AppendLine();
                with_1.Append("<INPUT type=button value=\"More Information &gt;&gt;\" onclick=\"this.style.display=\'none\'; document.getElementById(\'MoreInfo\').style.display=\'block\'\">");
                with_1.AppendLine();
                with_1.Append("<DIV style=\'display:none;\' id=\'MoreInfo\'>");
                with_1.AppendLine();
                with_1.Append("<H2>More information</H2>");
                with_1.AppendLine();
                with_1.Append("<TABLE width=\"100%\" bgcolor=\"#ffffcc\">");
                with_1.AppendLine();
                with_1.Append("<TR><TD>");
                with_1.AppendLine();
                with_1.Append("<CODE><PRE>");
                with_1.AppendLine();
                with_1.Append(m_moreInfoTextMethod);
                with_1.AppendLine();
                with_1.Append("</PRE></CODE>");
                with_1.AppendLine();
                with_1.Append("<TD><TR>");
                with_1.AppendLine();
                with_1.Append("</DIV>");
                with_1.AppendLine();
                with_1.Append("</BODY>");
                with_1.AppendLine();
                with_1.Append("</HTML>");
                with_1.AppendLine();

                HttpContext.Current.Response.Write(with_1.ToString());
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
                HttpContext.Current.Server.ClearError();

            }

            private void ExceptionToFile(Exception exception)
            {

                if (m_logToFile)
                {
                    try
                    {
                        m_logToFileOK = false;

                        _LogFile.Name = LogFileName;
                        if (!_LogFile.IsOpen)
                        {
                            _LogFile.Open();
                        }
                        _LogFile.WriteTimestampedLine(ExceptionToString(exception, m_parentAssembly));

                        m_logToFileOK = true;
                    }
                    catch
                    {

                    }
                    finally
                    {
                        _LogFile.Close();
                    }
                }

            }

            private void ExceptionToEmail(Exception exception)
            {

                if (m_logToEmail && !string.IsNullOrEmpty(m_contactEmail))
                {
                    try
                    {
                        m_logToEmailOK = false;

                        object with_1 = new SimpleMailMessage();
                        with_1.Sender = string.Format("{0}@tva.gov", Environment.MachineName);
                        with_1.Recipients = m_contactEmail;
                        with_1.Subject = string.Format("Exception in {0} at {1}", ApplicationName, DateTime.Now.ToString());
                        with_1.Body = ExceptionToString(exception, m_parentAssembly);
                        with_1.Attachments = AbsolutePath(ScreenshotFileName);
                        with_1.MailServer = m_smtpServer;
                        with_1.Send();

                        m_logToEmailOK = true;
                    }
                    catch
                    {

                    }
                }

            }

            private void ExceptionToEventLog(Exception exception)
            {

                if (m_logToEventLog)
                {
                    try
                    {
                        m_logToEventLogOK = false;

                        // Write the formatted exception message to the event log.
                        EventLog.WriteEntry(ApplicationName, ExceptionToString(exception, m_parentAssembly), EventLogEntryType.Error);

                        m_logToEventLogOK = true;
                    }
                    catch
                    {

                    }
                }

            }

            private void ExceptionToScreenshot(Exception exception)
            {

                if (m_logToScreenshot && (ApplicationType == ApplicationType.WindowsCui || ApplicationType == ApplicationType.WindowsGui))
                {
                    try
                    {
                        m_logToScreenshotOK = false;

                        Size fullScreen = new Size(0, 0);
                        foreach (Screen myScreen in Screen.AllScreens)
                        {
                            if (fullScreen.IsEmpty)
                            {
                                fullScreen = myScreen.Bounds.Size;
                            }
                            else
                            {
                                if (myScreen.Bounds.Location.X > 0)
                                {
                                    fullScreen.Width += myScreen.Bounds.Width;
                                }
                                if (myScreen.Bounds.Location.Y > 0)
                                {
                                    fullScreen.Height += myScreen.Bounds.Height;
                                }
                            }
                        }

                        using (Bitmap screenshot = TVA.Drawing.Image.CaptureScreenshot(fullScreen, System.Drawing.Imaging.ImageFormat.Png))
                        {
                            screenshot.Save(ScreenshotFileName);
                        }


                        m_logToScreenshotOK = true;
                    }
                    catch
                    {

                    }
                }

            }

            #endregion

            #region " Handlers "

            private string GetErrorText()
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                with_1.AppendFormat("An unexpected exception has occurred in {0}. ", ApplicationName);
                with_1.Append("This may be due to an inconsistent system state or a programming error.");

                return with_1.ToString();

            }

            private string GetScopeText()
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                switch (ApplicationType)
                {
                    case TVA.ApplicationType.WindowsCui:
                    case TVA.ApplicationType.WindowsGui:
                        with_1.Append("The action you requested was not performed.");
                        break;
                    case TVA.ApplicationType.Web:
                        with_1.Append("The current page will not load.");
                        break;
                }

                return with_1.ToString();

            }

            private string GetActionText()
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                switch (ApplicationType)
                {
                    case TVA.ApplicationType.WindowsCui:
                    case TVA.ApplicationType.WindowsGui:
                        with_1.AppendFormat("Restart {0}, and try repeating your last action. ", ApplicationName);
                        break;
                    case TVA.ApplicationType.Web:
                        with_1.AppendFormat("Close your browser, navigate back to the {0} website, and try repeating you last action. ", ApplicationName);
                        break;
                }
                with_1.Append("Try alternative methods of performing the same action. ");
                if (!string.IsNullOrEmpty(m_contactName) && (!string.IsNullOrEmpty(m_contactPhone) || !string.IsNullOrEmpty(m_contactPhone)))
                {
                    with_1.AppendFormat("If you need immediate assistance, contact {0} ", m_contactName);
                    if (!string.IsNullOrEmpty(m_contactEmail))
                    {
                        with_1.AppendFormat("via email at {0}", m_contactEmail);
                        if (!string.IsNullOrEmpty(m_contactPhone))
                        {
                            with_1.Append(" or ");
                        }
                    }
                    if (!string.IsNullOrEmpty(m_contactPhone))
                    {
                        with_1.AppendFormat("via phone at {0}", m_contactPhone);
                    }
                    with_1.Append(".");
                }

                return with_1.ToString();

            }

            private string GetMoreInfoText()
            {

                string bullet;
                switch (ApplicationType)
                {
                    case TVA.ApplicationType.WindowsCui:
                        bullet = "-";
                        break;
                    case TVA.ApplicationType.Web:
                    case TVA.ApplicationType.WindowsGui:
                        bullet = "•";
                        break;
                }

                System.Text.StringBuilder with_1 = new StringBuilder();
                with_1.Append("The following information about the error was automatically captured:");
                with_1.AppendLine();
                with_1.AppendLine();
                if (m_logToScreenshot)
                {
                    with_1.AppendFormat(" {0} ", bullet);
                    if (m_logToScreenshotOK)
                    {
                        with_1.Append("a screenshot was taken of the desktop at:");
                        with_1.AppendLine();
                        with_1.Append("   ");
                        with_1.Append(ScreenshotFileName);
                    }
                    else
                    {
                        with_1.Append("a screenshot could NOT be taken of the desktop.");
                    }
                    with_1.AppendLine();
                }
                if (m_logToEventLog)
                {
                    with_1.AppendFormat(" {0} ", bullet);
                    if (m_logToEventLogOK)
                    {
                        with_1.Append("an event was written to the application log");
                    }
                    else
                    {
                        with_1.Append("an event could NOT be written to the application log");
                    }
                    with_1.AppendLine();
                }
                if (m_logToFile)
                {
                    with_1.AppendFormat(" {0} ", bullet);
                    if (m_logToFileOK)
                    {
                        with_1.Append("details were written to a text log at:");
                    }
                    else
                    {
                        with_1.Append("details could NOT be written to the text log at:");
                    }
                    with_1.AppendLine();
                    with_1.Append("   ");
                    with_1.Append(LogFileName);
                    with_1.AppendLine();
                }
                if (m_logToEmail)
                {
                    with_1.AppendFormat(" {0} ", bullet);
                    if (m_logToEmailOK)
                    {
                        with_1.Append("an email has been sent to:");
                    }
                    else
                    {
                        with_1.Append("an email could NOT be sent to:");
                    }
                    with_1.AppendLine();
                    with_1.Append("   ");
                    with_1.Append(m_contactEmail);
                    with_1.AppendLine();
                }
                with_1.AppendLine();
                with_1.AppendLine();
                with_1.Append("Detailed error information follows:");
                with_1.AppendLine();
                with_1.AppendLine();
                with_1.Append(ExceptionToString(m_lastException, m_parentAssembly));

                return with_1.ToString();

            }

            private void UnhandledThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
            {

                HandleException(e.Exception, m_exitOnUnhandledException);

            }

            private void UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
            {

                HandleException(((Exception)e.ExceptionObject), m_exitOnUnhandledException);

            }

            #endregion

            #region " Shared "

            public static string ExceptionToString(Exception ex)
            {

                System.Reflection.Assembly parentAssembly;
                switch (TVA.Common.GetApplicationType())
                {
                    case TVA.ApplicationType.WindowsCui:
                    case ApplicationType.WindowsGui:
                        parentAssembly = System.Reflection.Assembly.GetEntryAssembly();
                        break;
                    case ApplicationType.Web:
                        parentAssembly = System.Reflection.Assembly.GetCallingAssembly();
                        break;
                }

                return ExceptionToString(ex, parentAssembly);

            }

            public static string SystemInfo()
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                with_1.AppendFormat("Date and Time:         {0}", DateTime.Now);
                with_1.AppendLine();
                switch (TVA.Common.GetApplicationType())
                {
                    case TVA.ApplicationType.WindowsCui:
                    case ApplicationType.WindowsGui:
                        TVA.Identity.UserInformation currentUserInfo = new TVA.Identity.UserInformation(System.Threading.Thread.CurrentPrincipal.Identity.Name);
                        with_1.AppendFormat("Machine Name:          {0}", Environment.MachineName);
                        with_1.AppendLine();
                        with_1.AppendFormat("Machine IP:            {0}", Dns.GetHostEntry(Environment.MachineName).AddressList(0).ToString());
                        with_1.AppendLine();
                        with_1.AppendFormat("Current User ID:       {0}", currentUserInfo.LoginID);
                        with_1.AppendLine();
                        with_1.AppendFormat("Current User Name:     {0}", currentUserInfo.FullName);
                        with_1.AppendLine();
                        with_1.AppendFormat("Current User Phone:    {0}", currentUserInfo.Telephone);
                        with_1.AppendLine();
                        with_1.AppendFormat("Current User Email:    {0}", currentUserInfo.Email);
                        with_1.AppendLine();
                        break;
                    case ApplicationType.Web:
                        TVA.Identity.UserInformation remoteUserInfo = new TVA.Identity.UserInformation(System.Threading.Thread.CurrentPrincipal.Identity.Name, true);
                        with_1.AppendFormat("Server Name:           {0}", Environment.MachineName);
                        with_1.AppendLine();
                        with_1.AppendFormat("Server IP:             {0}", Dns.GetHostEntry(Environment.MachineName).AddressList(0).ToString());
                        with_1.AppendLine();
                        with_1.AppendFormat("Process User:          {0}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                        with_1.AppendLine();
                        with_1.AppendFormat("Remote User ID:        {0}", remoteUserInfo.LoginID);
                        with_1.AppendLine();
                        with_1.AppendFormat("Remote User Name:      {0}", remoteUserInfo.FullName);
                        with_1.AppendLine();
                        with_1.AppendFormat("Remote User Phone:     {0}", remoteUserInfo.Telephone);
                        with_1.AppendLine();
                        with_1.AppendFormat("Remote User Email:     {0}", remoteUserInfo.Email);
                        with_1.AppendLine();
                        with_1.AppendFormat("Remote Host:           {0}", HttpContext.Current.Request.ServerVariables["REMOTE_HOST"]);
                        with_1.AppendLine();
                        with_1.AppendFormat("Remote Address:        {0}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
                        with_1.AppendLine();
                        with_1.AppendFormat("HTTP Agent:            {0}", HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"]);
                        with_1.AppendLine();
                        with_1.AppendFormat("HTTP Referer:          {0}", HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]);
                        with_1.AppendLine();
                        with_1.AppendFormat("Web Page URL:          {0}", HttpContext.Current.Request.Url.ToString());
                        with_1.AppendLine();
                        break;
                }

                return with_1.ToString();

            }

            public static string ApplicationInfo()
            {

                System.Reflection.Assembly parentAssembly;
                switch (TVA.Common.GetApplicationType())
                {
                    case TVA.ApplicationType.WindowsCui:
                    case ApplicationType.WindowsGui:
                        // For a windows application the entry assembly will be the executable.
                        parentAssembly = System.Reflection.Assembly.GetEntryAssembly();
                        break;
                    case ApplicationType.Web:
                        // For a web site in .Net 2.0 we don't have an entry assembly. However, at this point the
                        // calling assembly will be consumer of this function (i.e. one of the web site DLLs).
                        // See: http://msdn.microsoft.com/msdnmag/issues/06/01/ExtremeASPNET/
                        parentAssembly = System.Reflection.Assembly.GetCallingAssembly();
                        break;
                }

                return ApplicationInfo(parentAssembly);

            }

            public static string ExceptionGeneralInfo(Exception ex)
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                with_1.AppendFormat("Exception Source:      {0}", ex.Source);
                with_1.AppendLine();
                with_1.AppendFormat("Exception Type:        {0}", ex.GetType().FullName);
                with_1.AppendLine();
                with_1.AppendFormat("Exception Message:     {0}", ex.Message);
                with_1.AppendLine();
                if (ex.TargetSite != null)
                {
                    with_1.AppendFormat("Exception Target Site: {0}", ex.TargetSite.Name);
                    with_1.AppendLine();
                }

                return with_1.ToString();

            }

            public static string ExceptionStackTrace(Exception ex)
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                StackTrace stack = new StackTrace(ex, true);
                for (int i = 0; i <= stack.FrameCount - 1; i++)
                {
                    StackFrame stackFrame = stack.GetFrame(i);
                    MemberInfo method = stackFrame.GetMethod();
                    string codeFileName = stackFrame.GetFileName();

                    // build method name
                    with_1.AppendFormat("   {0}.{1}.{2}", method.DeclaringType.Namespace, method.DeclaringType.Name, method.Name);

                    // build method params
                    with_1.Append("(");
                    int parameterCount = 0;
                    foreach (ParameterInfo parameter in stackFrame.GetMethod().GetParameters())
                    {
                        parameterCount++;
                        if (parameterCount > 1)
                        {
                            with_1.Append(", ");
                        }
                        with_1.AppendFormat("{0} As {1}", parameter.Name, parameter.ParameterType.Name);
                    }
                    with_1.Append(")");
                    with_1.AppendLine();

                    // if source code is available, append location info
                    with_1.Append("       ");

                    if (!string.IsNullOrEmpty(codeFileName))
                    {
                        with_1.Append(System.IO.Path.GetFileName(codeFileName));
                        with_1.AppendFormat(": Ln {0:#0000}", stackFrame.GetFileLineNumber());
                        with_1.AppendFormat(", Col {0:#00}", stackFrame.GetFileColumnNumber());
                        // if IL is available, append IL location info
                        if (stackFrame.GetILOffset() != stackFrame.OFFSET_UNKNOWN)
                        {
                            with_1.AppendFormat(", IL {0:#0000}", stackFrame.GetILOffset());
                        }
                    }
                    else
                    {
                        ApplicationType appType = TVA.Common.GetApplicationType();
                        if (appType == ApplicationType.WindowsCui || appType == TVA.ApplicationType.WindowsGui)
                        {
                            with_1.Append(System.IO.Path.GetFileName(TVA.AssemblyInformation.EntryAssembly.CodeBase));
                        }
                        else
                        {
                            with_1.Append("(unknown file)");
                        }
                        // native code offset is always available
                        with_1.AppendFormat(": N {0:#00000}", stackFrame.GetNativeOffset());
                    }
                    with_1.AppendLine();
                }

                return with_1.ToString();

            }

            private static string ExceptionToString(Exception ex, System.Reflection.Assembly parentAssembly)
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                if (ex.InnerException != null)
                {
                    // sometimes the original exception is wrapped in a more relevant outer exception
                    // the detail exception is the "inner" exception
                    // see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/exceptdotnet.asp
                    with_1.Append("(Inner Exception)");
                    with_1.AppendLine();
                    with_1.Append(ExceptionToString(ex.InnerException));
                    with_1.AppendLine();
                    with_1.Append("(Outer Exception)");
                    with_1.AppendLine();
                }

                // Get general system information.
                with_1.Append(SystemInfo());
                with_1.AppendLine();
                // Get general application information.
                with_1.Append(ApplicationInfo(parentAssembly));
                with_1.AppendLine();
                // Get general exception information.
                with_1.Append(ExceptionGeneralInfo(ex));
                with_1.AppendLine();
                // Get the stack trace for the exception.
                with_1.Append("---- Stack Trace ----");
                with_1.AppendLine();
                with_1.Append(ExceptionStackTrace(ex));
                with_1.AppendLine();

                return with_1.ToString();

            }

            private static string ApplicationInfo(System.Reflection.Assembly parentAssembly)
            {

                System.Text.StringBuilder with_1 = new StringBuilder();
                TVA.AssemblyInformation parentAssemblyInfo = new TVA.AssemblyInformation(parentAssembly);
                with_1.AppendFormat("Application Domain:    {0}", System.AppDomain.CurrentDomain.FriendlyName);
                with_1.AppendLine();
                with_1.AppendFormat("Assembly Codebase:     {0}", parentAssemblyInfo.CodeBase);
                with_1.AppendLine();
                with_1.AppendFormat("Assembly Full Name:    {0}", parentAssemblyInfo.FullName);
                with_1.AppendLine();
                with_1.AppendFormat("Assembly Version:      {0}", parentAssemblyInfo.Version.ToString());
                with_1.AppendLine();
                with_1.AppendFormat("Assembly Build Date:   {0}", parentAssemblyInfo.BuildDate.ToString());
                with_1.AppendLine();
                with_1.AppendFormat(".Net Runtime Version:  {0}", Environment.Version.ToString());
                with_1.AppendLine();

                return with_1.ToString();

            }

            #endregion

        }

    }
}
