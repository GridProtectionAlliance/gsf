//*******************************************************************************************************
//  Mail.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/30/2005 - Pinal C. Patel
//       Generated original version of source code.
//  12/12/2007 - Darrell Zuercher
//       Edited Code Comments.
//  09/22/2008 - James R Carroll
//       Converted to C# - restructured.
//  10/15/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;

namespace TVA.Net.Smtp
{
    /// <summary>
    /// A wrapper class to the <see cref="MailMessage"/> class that simplifies sending mail messages.
    /// </summary>
    /// <example>
    /// This example shows how to send an email message with attachment:
    /// <code>
    /// using System;
    /// using TVA.Net.Smtp;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         Mail email = new Mail("sender@email.com", "recipient@email.com", "smtp.email.com");
    ///         email.Subject = "Test Message";
    ///         email.Body = "This is a test message.";
    ///         email.IsBodyHtml = true;
    ///         email.Attachments = @"c:\attachment.txt";
    ///         email.Send();
    ///
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class Mail
    {
        #region [ Members ]

        // Constants
        /// <summary>
        /// Default <see cref="SmtpServer"/> to be used if one is not specified.
        /// </summary>
        public const string DefaultSmtpServer = "mailhost.cha.tva.gov";

        // Fields
        private string m_from;
        private string m_toRecipients;
        private string m_ccRecipients;
        private string m_bccRecipients;
        private string m_subject;
        private string m_body;
        private string m_smtpServer;
        private string m_attachments;
        private bool m_isBodyHtml;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="Mail"/> class.
        /// </summary>
        /// <param name="from">The e-mail address of the <see cref="Mail"/> message sender.</param>
        /// <param name="toRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message recipients.</param>
        public Mail(string from, string toRecipients)
            : this(from, toRecipients, DefaultSmtpServer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mail"/> class.
        /// </summary>
        /// <param name="from">The e-mail address of the <see cref="Mail"/> message sender.</param>
        /// <param name="toRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message recipients.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server to be used for sending the <see cref="Mail"/> message.</param>
        public Mail(string from, string toRecipients, string smtpServer)
        {
            this.From = from;
            this.ToRecipients = toRecipients;
            this.SmtpServer = smtpServer;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the e-mail address of the <see cref="Mail"/> message sender.
        /// </summary>
        public string From
        {
            get
            {
                return m_from;
            }
            set
            {
                // This is a required field.
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_from = value;
            }
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message recipients.
        /// </summary>
        public string ToRecipients
        {
            get
            {
                return m_toRecipients;
            }
            set
            {
                // This is a required field.
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_toRecipients = value;
            }
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message carbon copy (CC) recipients.
        /// </summary>
        public string CcRecipients
        {
            get
            {
                return m_ccRecipients;
            }
            set
            {
                m_ccRecipients = value;
            }
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message blank carbon copy (BCC) recipients.
        /// </summary>
        public string BccRecipients
        {
            get
            {
                return m_bccRecipients;
            }
            set
            {
                m_bccRecipients = value;
            }
        }

        /// <summary>
        /// Gets or sets the subject of the <see cref="Mail"/> message.
        /// </summary>
        public string Subject
        {
            get
            {
                return m_subject;
            }
            set
            {
                m_subject = value;
            }
        }

        /// <summary>
        /// Gets or sets the body of the <see cref="Mail"/> message.
        /// </summary>
        public string Body
        {
            get
            {
                return m_body;
            }
            set
            {
                m_body = value;
            }
        }

        /// <summary>
        /// Gets or sets the name or IP address of the SMTP server to be used for sending the <see cref="Mail"/> message.
        /// </summary>
        public string SmtpServer
        {
            get
            {
                return m_smtpServer;
            }
            set
            {
                // This is a required field.
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_smtpServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-seperated list of file names to be attached to the <see cref="Mail"/> message.
        /// </summary>
        public string Attachments
        {
            get
            {
                return m_attachments;
            }
            set
            {
                m_attachments = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicating whether the <see cref="Mail"/> message <see cref="Body"/> is to be formatted as HTML.
        /// </summary>
        public bool IsBodyHtml
        {
            get
            {
                return m_isBodyHtml;
            }
            set
            {
                m_isBodyHtml = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Send the <see cref="Mail"/> message with <see cref="Attachments"/> to the <see cref="ToRecipients"/>, 
        /// <see cref="CcRecipients"/> and <see cref="BccRecipients"/> using the specified <see cref="SmtpServer"/>.
        /// </summary>
        public void Send()
        {
            MailMessage emailMessage = new MailMessage(m_from, m_toRecipients, m_subject, m_body);
            emailMessage.IsBodyHtml = m_isBodyHtml;

            if (!string.IsNullOrEmpty(m_ccRecipients))
            {
                // Add the specified CC recipients for the mail message.
                foreach (string ccRecipient in m_ccRecipients.Split(new char[] { ';', ',' }))
                {
                    emailMessage.CC.Add(ccRecipient.Trim());
                }
            }

            if (!string.IsNullOrEmpty(m_bccRecipients))
            {
                // Add the specified BCC recipients for the mail message.
                foreach (string bccRecipient in m_bccRecipients.Split(new char[] { ';', ',' }))
                {
                    emailMessage.Bcc.Add(bccRecipient.Trim());
                }
            }

            if (!string.IsNullOrEmpty(m_attachments))
            {
                // Attach the specified files to the mail message.
                foreach (string attachment in m_attachments.Split(new char[] { ';', ',' }))
                {
                    // Create the file attachment for the mail message.
                    Attachment data = new Attachment(attachment.Trim(), MediaTypeNames.Application.Octet);
                    ContentDisposition header = data.ContentDisposition;

                    // Add time stamp information for the file.
                    header.CreationDate = File.GetCreationTime(attachment);
                    header.ModificationDate = File.GetLastWriteTime(attachment);
                    header.ReadDate = File.GetLastAccessTime(attachment);

                    emailMessage.Attachments.Add(data); // Attach the file.
                }
            }

            SmtpClient smtpClient = new SmtpClient(m_smtpServer);
            smtpClient.Send(emailMessage);  // Send the mail.
            emailMessage.Dispose();         // Clean-up.
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Sends a <see cref="Mail"/> message.
        /// </summary>
        /// <param name="from">The e-mail address of the <see cref="Mail"/> message sender.</param>
        /// <param name="toRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message recipients.</param>
        /// <param name="subject">The subject of the <see cref="Mail"/> message.</param>
        /// <param name="body">The body of the <see cref="Mail"/> message.</param>
        /// <param name="isBodyHtml">true if the <see cref="Mail"/> message body is to be formated as HTML; otherwise false.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server to be used for sending the <see cref="Mail"/> message.</param>
        public static void Send(string from, string toRecipients, string subject, string body, bool isBodyHtml, string smtpServer)
        {
            Send(from, toRecipients, null, null, subject, body, isBodyHtml, smtpServer);
        }

        /// <summary>
        /// Sends a <see cref="Mail"/> message.
        /// </summary>
        /// <param name="from">The e-mail address of the <see cref="Mail"/> message sender.</param>
        /// <param name="toRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message recipients.</param>
        /// <param name="ccRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message carbon copy (CC) recipients.</param>
        /// <param name="bccRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message blank carbon copy (BCC) recipients.</param>
        /// <param name="subject">The subject of the <see cref="Mail"/> message.</param>
        /// <param name="body">The body of the <see cref="Mail"/> message.</param>
        /// <param name="isBodyHtml">true if the <see cref="Mail"/> message body is to be formated as HTML; otherwise false.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server to be used for sending the <see cref="Mail"/> message.</param>
        public static void Send(string from, string toRecipients, string ccRecipients, string bccRecipients, string subject, string body, bool isBodyHtml, string smtpServer)
        {
            Send(from, toRecipients, ccRecipients, bccRecipients, subject, body, isBodyHtml, null, smtpServer);
        }

        /// <summary>
        /// Sends a <see cref="Mail"/> message.
        /// </summary>
        /// <param name="from">The e-mail address of the <see cref="Mail"/> message sender.</param>
        /// <param name="toRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message recipients.</param>
        /// <param name="subject">The subject of the <see cref="Mail"/> message.</param>
        /// <param name="body">The body of the <see cref="Mail"/> message.</param>
        /// <param name="isBodyHtml">true if the <see cref="Mail"/> message body is to be formated as HTML; otherwise false.</param>
        /// <param name="attachments">A comma-separated or semicolon-seperated list of file names to be attached to the <see cref="Mail"/> message.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server to be used for sending the <see cref="Mail"/> message.</param>
        public static void Send(string from, string toRecipients, string subject, string body, bool isBodyHtml, string attachments, string smtpServer)
        {
            Send(from, toRecipients, null, null, subject, body, isBodyHtml, attachments, smtpServer);
        }

        /// <summary>
        /// Sends a <see cref="Mail"/> message.
        /// </summary>
        /// <param name="from">The e-mail address of the <see cref="Mail"/> message sender.</param>
        /// <param name="toRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message recipients.</param>
        /// <param name="ccRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message carbon copy (CC) recipients.</param>
        /// <param name="bccRecipients">A comma-separated or semicolon-seperated e-mail address list of the <see cref="Mail"/> message blank carbon copy (BCC) recipients.</param>
        /// <param name="subject">The subject of the <see cref="Mail"/> message.</param>
        /// <param name="body">The body of the <see cref="Mail"/> message.</param>
        /// <param name="isBodyHtml">true if the <see cref="Mail"/> message body is to be formated as HTML; otherwise false.</param>
        /// <param name="attachments">A comma-separated or semicolon-seperated list of file names to be attached to the <see cref="Mail"/> message.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server to be used for sending the <see cref="Mail"/> message.</param>
        public static void Send(string from, string toRecipients, string ccRecipients, string bccRecipients, string subject, string body, bool isBodyHtml, string attachments, string smtpServer)
        {
            Mail email = new Mail(from, toRecipients, smtpServer);
            email.CcRecipients = ccRecipients;
            email.BccRecipients = bccRecipients;
            email.Subject = subject;
            email.Body = body;
            email.IsBodyHtml = isBodyHtml;
            email.Attachments = attachments;
            email.Send();
        }

        #endregion
    }
}