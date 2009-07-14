//*******************************************************************************************************
//  EmailNotifier.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/28/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using TVA.Configuration;
using TVA.Net.Smtp;

namespace TVA.Historian.Notifiers
{
    /// <summary>
    /// Represents a notifier that can send notifications in email messages.
    /// </summary>
    public class EmailNotifier : NotifierBase
    {
        #region [ Members ]

        // Fields
        private string m_emailServer;
        private string m_emailSender;
        private string m_emailRecipients;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotifier"/> class.
        /// </summary>
        public EmailNotifier()
            : base(true, true, true, false)
        {
            m_emailServer = Mail.DefaultSmtpServer;
            m_emailSender = string.Format("{0}@{1}.local", Environment.UserName, Environment.UserDomainName);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the SMTP server to use for sending the email messages.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string EmailServer
        {
            get
            {
                return m_emailServer;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_emailServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the email address to be used for sending the email messages.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string EmailSender
        {
            get
            {
                return m_emailSender;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_emailSender = value;
            }
        }

        /// <summary>
        /// Gets or sets the email addresses (comma or semicolon delimited) where the email messages are to be sent.
        /// </summary>
        /// <remarks>
        /// Email address can be provided in the &lt;Email Address&gt;:sms format (Example: 123456789@provider.com:sms), 
        /// to indicate that the reciepient is a mobile device and a very brief email message is to be sent.
        /// </remarks>
        public string EmailRecipients
        {
            get
            {
                return m_emailRecipients;
            }
            set
            {
                m_emailRecipients = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves <see cref="EmailNotifier"/> settings to the config file if the <see cref="NotifierBase.PersistSettings"/> property is set to true.
        /// </summary>        
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(SettingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                element = settings["EmailServer", true];
                element.Update(m_emailServer, element.Description, element.Encrypted);
                element = settings["EmailSender", true];
                element.Update(m_emailSender, element.Description, element.Encrypted);
                element = settings["EmailRecipients", true];
                element.Update(m_emailRecipients, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="EmailNotifier"/> settings from the config file if the <see cref="NotifierBase.PersistSettings"/> property is set to true.
        /// </summary>        
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(SettingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("EmailServer", m_emailServer, "SMTP server to use for sending the email notifications.");
                settings.Add("EmailSender", m_emailSender, "Email address to be used for sending the email notifications.");
                settings.Add("EmailRecipients", m_emailRecipients, "Email addresses (comma or semicolon delimited) where the email notifications are to be sent.");
                EmailServer = settings["EmailServer"].ValueAs(m_emailServer);
                EmailSender = settings["EmailSender"].ValueAs(m_emailSender);
                EmailRecipients = settings["EmailRecipients"].ValueAs(m_emailRecipients);
            }
        }

        /// <summary>
        /// Processes a <see cref="NotificationType.Alarm"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected override bool NotifyAlarm(string subject, string message, string details)
        {
            subject = "ALARM: " + subject;
            return SendEmail(subject, message, details);
        }

        /// <summary>
        /// Processes a <see cref="NotificationType.Warning"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected override bool NotifyWarning(string subject, string message, string details)
        {
            subject = "WARNING: " + subject;
            return SendEmail(subject, message, details);
        }

        /// <summary>
        /// Processes a <see cref="NotificationType.Information"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected override bool NotifyInformation(string subject, string message, string details)
        {
            subject = "INFO: " + subject;
            return SendEmail(subject, message, details);
        }

        /// <summary>
        /// Processes a <see cref="NotificationType.Heartbeat"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected override bool NotifyHeartbeat(string subject, string message, string details)
        {
            throw new NotSupportedException();
        }

        private bool SendEmail(string subject, string message, string details)
        {
            if (string.IsNullOrEmpty(m_emailRecipients))
                return false;

            Mail briefMessage = new Mail(m_emailSender, m_emailSender, m_emailServer);
            Mail detailedMessage = new Mail(m_emailSender, m_emailSender, m_emailServer);

            briefMessage.Subject = subject;
            detailedMessage.Subject = subject;
            detailedMessage.Body = message + "\r\n\r\n" + details;
            foreach (string recipient in m_emailRecipients.Replace(" ", "").Split(';', ','))
            {
                string[] addressParts = recipient.Split(':');
                if (addressParts.Length > 1)
                {
                    if (string.Compare(addressParts[1], "sms", true) == 0)
                    {
                        // A brief message is to be sent.
                        briefMessage.ToRecipients = addressParts[0];
                        briefMessage.Send();
                    }
                }
                else
                {
                    // A detailed message is to be sent.
                    detailedMessage.ToRecipients = recipient;
                    detailedMessage.Send();
                }
            }

            return true;
        }

        #endregion
    }
}