//******************************************************************************************************
//  ErrorLogger.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/23/2013 - J. Ritchie Carroll
//       Migrated to be a Windows Forms specific error logging engine (instead of part of core).
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GSF.Configuration;
using GSF.IO;
using GSF.Net.Smtp;
using GSF.Windows.Forms;

namespace GSF.Windows.ErrorManagement
{
    /// <summary>
    /// Represents a logger that can be used for logging handled as well as unhandled exceptions across multiple 
    /// application types (Windows Application, Console Application, Windows Service, Web Application, Web Service).
    /// </summary>
    /// <example>
    /// This example shows how to use the <see cref="ErrorLogger"/> component to log handled and unhandled exceptions:
    /// <code>
    /// using System;
    /// using System.IO;
    /// using GSF.Windows.ErrorManagement;
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
    ///         s_logger.LogToScreenshot = true;            // Take a screenshot of desktop on exception.
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
    /// <seealso cref="GSF.ErrorManagement.ErrorModule"/>
    [ToolboxBitmap(typeof(GSF.ErrorManagement.ErrorLogger))]
    public class ErrorLogger : GSF.ErrorManagement.ErrorLogger
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="LogToScreenshot"/> property.
        /// </summary>
        public const bool DefaultLogToScreenshot = false;

        // Fields
        private bool m_logToScreenshot;
        private bool m_logToScreenshotOK;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLogger"/> class.
        /// </summary>
        public ErrorLogger()
        {
            m_logToScreenshot = DefaultLogToScreenshot;

            // Initialize all logger methods.
            Loggers.Add(ExceptionToScreenshot);
        }

        #endregion

        #region [ Properties ]

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
        /// Gets the descriptive status of the <see cref="ErrorLogger"/> object.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.Append("       Error to Screenshot: ");
                status.Append(m_logToScreenshot ? "Enabled" : "Disabled");
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves settings for the <see cref="ErrorLogger"/> object to the config file if the <see cref="GSF.ErrorManagement.ErrorLogger.PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="GSF.ErrorManagement.ErrorLogger.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void SaveSettings()
        {
            if (PersistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(SettingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

                settings["LogToScreenshot", true].Update(m_logToScreenshot);

                base.SaveSettings();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="ErrorLogger"/> object from the config file if the <see cref="GSF.ErrorManagement.ErrorLogger.PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="GSF.ErrorManagement.ErrorLogger.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void LoadSettings()
        {
            if (PersistSettings)
            {
                base.LoadSettings();

                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(SettingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

                settings.Add("LogToScreenshot", m_logToScreenshot, "True if a screenshot is to be taken when an exception is encountered; otherwise False.");
                LogToScreenshot = settings["LogToScreenshot"].ValueAs(m_logToScreenshot);
            }
        }

        /// <summary>
        /// Registers the <see cref="GSF.ErrorManagement.ErrorLogger"/> object to handle unhandled <see cref="Exception"/> if the 
        /// <see cref="GSF.ErrorManagement.ErrorLogger.HandleUnhandledException"/> property is set to true.
        /// </summary>
        /// <returns><c>true</c> if handlers were registered; otherwise <c>false</c>.</returns>
        protected override bool Register()
        {
            if (base.Register())
            {
                Application.ThreadException += ThreadException;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unregister the <see cref="GSF.ErrorManagement.ErrorLogger"/> object from handling unhandled <see cref="Exception"/>.
        /// </summary>
        /// <returns><c>true</c> if handlers were unregistered; otherwise <c>false</c>.</returns>
        protected override bool Unregister()
        {
            if (base.Unregister())
            {
                Application.ThreadException -= ThreadException;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows <see cref="Exception"/> information in a Windows Application.
        /// </summary>
        protected override void ExceptionToWindowsGui()
        {
            // Use the ErrorDialog to show exception information.
            ErrorDialog dialog = new ErrorDialog();

            dialog.Text = string.Format(dialog.Text, ApplicationName, null);
            dialog.PictureBoxIcon.Image = SystemIcons.Error.ToBitmap();
            dialog.RichTextBoxError.Text = ErrorTextMethod();
            dialog.RichTextBoxScope.Text = ScopeTextMethod();
            dialog.RichTextBoxAction.Text = ActionTextMethod();
            dialog.RichTextBoxMoreInfo.Text = MoreInfoTextMethod();

            dialog.ShowDialog();
        }

        /// <summary>
        /// Takes a screenshot of the user's desktop when the <see cref="Exception"/> is encountered.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> that was encountered.</param>
        protected virtual void ExceptionToScreenshot(Exception exception)
        {
            // Log if enabled.
            if (m_logToScreenshot && (ApplicationType == ApplicationType.WindowsCui || ApplicationType == ApplicationType.WindowsGui))
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
        /// Gets or sets the comma-separated or semicolon-separated list of file names to be attached to the <see cref="Mail"/> message.
        /// </summary>
        protected override string GetEmailAttachments()
        {
            // If logging to screen shot, attach it to e-mail exceptions
            if (m_logToScreenshot)
                return GetScreenshotFileName();

            return null;
        }

        /// <summary>
        /// Allows other loggers to extend "more info text".
        /// </summary>
        /// <param name="bullet">Type of bullet to use for extended info text.</param>
        protected override string GetExtendedMoreInfoText(string bullet)
        {
            if (m_logToScreenshot)
            {
                StringBuilder moreInfoText = new StringBuilder();

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

                return moreInfoText.ToString();
            }

            return null;
        }

        private string GetScreenshotFileName()
        {
            return FilePath.GetAbsolutePath(ApplicationName + ".ErrorState.png");
        }

        private void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            SuppressInteractiveLogging = false;
            Log(new Exception("ThreadException", e.Exception), ExitOnUnhandledException);
        }

        #endregion
    }
}
