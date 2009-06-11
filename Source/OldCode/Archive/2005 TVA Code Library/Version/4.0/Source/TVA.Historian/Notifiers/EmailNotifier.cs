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
    public class EmailNotifier : NotifierBase
    {
        #region [ Members ]

        // Fields
        private string m_emailServer;
        private string m_emailRecipients;

        #endregion

        #region [ Constructors ]

        public EmailNotifier()
            : base(true, true, true, false)
        {
            m_emailServer = Mail.DefaultSmtpServer;
        }

        #endregion

        #region [ Properties ]

        public string EmailServer
        {
            get
            {
                return m_emailServer;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    m_emailServer = value;
                }
                else
                {
                    throw (new ArgumentNullException());
                }
            }
        }

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

        public override void LoadSettings()
        {
            base.LoadSettings();

            try
            {
                TVA.Configuration.CategorizedSettingsElementCollection with_1 = ConfigurationFile.Current.Settings[SettingsCategory];
                if (with_1.Count > 0)
                {
                    EmailServer = with_1["EmailServer"].ValueAs(m_emailServer);
                    EmailRecipients = with_1["EmailRecipients"].ValueAs(m_emailRecipients);
                }
            }
            catch (Exception)
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            if (PersistSettings)
            {
                try
                {
                    TVA.Configuration.CategorizedSettingsElementCollection with_1 = ConfigurationFile.Current.Settings[SettingsCategory];
                    TVA.Configuration.CategorizedSettingsElement with_2 = with_1["EmailServer", true];
                    with_2.Value = m_emailServer;
                    with_2.Description = "SMTP server to use for sending the email notifications.";
                    TVA.Configuration.CategorizedSettingsElement with_3 = with_1["EmailRecipients", true];
                    with_3.Value = m_emailRecipients;
                    with_3.Description = "Email addresses of the recipients for the email notifications.";
                    ConfigurationFile.Current.Save();
                }
                catch (Exception)
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        protected override bool NotifyAlarm(string subject, string message, string details)
        {
            subject = "ALARM: " + subject;
            return SendEmail(subject, message, details);
        }

        protected override bool NotifyWarning(string subject, string message, string details)
        {
            subject = "WARNING: " + subject;
            return SendEmail(subject, message, details);
        }

        protected override bool NotifyInformation(string subject, string message, string details)
        {
            subject = "INFO: " + subject;
            return SendEmail(subject, message, details);
        }

        protected override bool NotifyHeartbeat(string subject, string message, string details)
        {
            throw (new NotSupportedException());
        }

        private bool SendEmail(string subject, string message, string details)
        {
            if (string.IsNullOrEmpty(m_emailRecipients))
                return false;

            string sender = string.Format("{0}@tva.gov", Environment.MachineName);
            Mail briefMessage = new Mail(sender, sender, m_emailServer);
            Mail detailedMessage = new Mail(sender, sender, m_emailServer);

            briefMessage.Subject = subject;
            detailedMessage.Subject = subject;
            detailedMessage.Body = message + Environment.NewLine + Environment.NewLine + details;
            foreach (string recipient in m_emailRecipients.Replace(" ", "").Split(';', ','))
            {
                // We allow recipient email address in a special format to indicate that the recipient
                // would like to receive short SMS type messages. Following is an example of this format:
                // 123456789@provider.com:sms
                string[] addressParts = recipient.Split(':');
                if (addressParts.Length > 1)
                {
                    if (string.Compare(addressParts[1], "sms", true) == 0)
                    {
                        //We'll send a brief message to the recipient.
                        briefMessage.ToRecipients = addressParts[0];
                        briefMessage.Send();
                    }
                }
                else
                {
                    // We'll send a detailed message to the recipient.
                    detailedMessage.ToRecipients = recipient;
                    detailedMessage.Send();
                }
            }

            return true;
        }

        #endregion
    }
}