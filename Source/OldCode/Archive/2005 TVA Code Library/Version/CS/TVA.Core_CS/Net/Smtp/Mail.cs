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
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Mail;
using System.Collections.Generic;

namespace TVA.Net.Smtp
{
    /// <summary>Defines common e-mail related functions.</summary>
    public class Mail
    {
        #region [ Members ]

        // Fields
        private string m_from;
        private string m_recipients;
        private string m_ccRecipients;
        private string m_bccRecipients;
        private string m_subject;
        private string m_body;
        private string m_smtpServer;
        private string m_attachments;
        private bool m_isBodyHtml;

        #endregion

        #region [ Constructors ]

        public Mail()
        {
        }

        public Mail(string smtpServer)
        {
            m_smtpServer = smtpServer;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the sender's address for this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The sender's address for this e-mail message.</returns>
        public string From
        {
            get
            {
                return m_from;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_from = value;
                else
                    throw new ArgumentNullException("From");
            }
        }

        /// <summary>
        /// Gets the comma (,) or semicolon (;) delimited list of recipients for this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The comma (,) or semicolon (;) delimited list of recipients for this e-mail message.</returns>
        public string Recipients
        {
            get
            {
                return m_recipients;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_recipients = value;
                else
                    throw new ArgumentNullException("Recipients");
            }
        }

        /// <summary>
        /// Gets the comma (,) or semicolon (;) delimited list of carbon copy recipients for this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The comma (,) or semicolon (;) delimited list of carbon copy recipients for this e-mail message.</returns>
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
        /// Gets the comma (,) or semicolon (;) delimited list of blind carbon copy recipients for this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The comma (,) or semicolon (;) delimited list of blind carbon copy recipients for this e-mail message.</returns>
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
        /// Gets or sets the subject line for this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The subject line for this e-mail message.</returns>
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
        /// Gets or sets the message body.
        /// </summary>
        /// <value></value>
        /// <returns>The body text.</returns>
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
        /// Gets or sets the name or IP address of the mail server for this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The name or IP address of the mail server for this e-mail message.</returns>
        public string SmtpServer
        {
            get
            {
                return m_smtpServer;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_smtpServer = value;
                else
                    throw new ArgumentNullException("SmtpServer");
            }
        }

        /// <summary>
        /// Gets or sets the comma (,) or semicolon (;) delimited list of file names that are to be attached to this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The comma (,) or semicolon (;) delimited list of file names that are to be attached to this e-mail message.</returns>
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
        /// Gets or sets a boolean value indicating whether the mail message body is in Html.
        /// </summary>
        /// <value></value>
        /// <returns>True if the message body is in Html; otherwise False.</returns>
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
        /// Send this e-mail message to its recipients.
        /// </summary>
        public void Send()
        {
            Mail.Send(m_from, m_recipients, m_ccRecipients, m_bccRecipients, m_subject, m_body, m_isBodyHtml, m_attachments, m_smtpServer);
        }

        #endregion

        #region [ Static ]

        /// <summary>Creates a mail message from the specified information, and sends it to an SMTP server for delivery.</summary>
        /// <param name="from">The address of the mail message sender.</param>
        /// <param name="toRecipients">A comma-separated address list of the mail message recipients.</param>
        /// <param name="subject">The subject of the mail message.</param>
        /// <param name="body">The body of the mail message.</param>
        /// <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        public static void Send(string from, string toRecipients, string subject, string body, bool isBodyHtml, string smtpServer)
        {
            Send(from, toRecipients, null, null, subject, body, isBodyHtml, smtpServer);
        }

        /// <summary>Creates a mail message from the specified information, and sends it to an SMTP server for delivery.</summary>
        /// <param name="from">The address of the mail message sender.</param>
        /// <param name="toRecipients">A comma-separated address list of the mail message recipients.</param>
        /// <param name="ccRecipients">A comma-separated address list of the mail message carbon copy (CC) recipients.</param>
        /// <param name="bccRecipients">A comma-separated address list of the mail message blank carbon copy (BCC) recipients.</param>
        /// <param name="subject">The subject of the mail message.</param>
        /// <param name="body">The body of the mail message.</param>
        /// <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        public static void Send(string from, string toRecipients, string ccRecipients, string bccRecipients, string subject, string body, bool isBodyHtml, string smtpServer)
        {
            Send(from, toRecipients, ccRecipients, bccRecipients, subject, body, isBodyHtml, null, smtpServer);
        }

        /// <summary>Creates a mail message from the specified information, and sends it to an SMTP server for delivery.</summary>
        /// <param name="from">The address of the mail message sender.</param>
        /// <param name="toRecipients">A comma-separated address list of the mail message recipients.</param>
        /// <param name="subject">The subject of the mail message.</param>
        /// <param name="body">The body of the mail message.</param>
        /// <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        /// <param name="attachments">A comma-separated list of file names to be attached to the mail message.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        public static void Send(string from, string toRecipients, string subject, string body, bool isBodyHtml, string attachments, string smtpServer)
        {
            Send(from, toRecipients, null, null, subject, body, isBodyHtml, attachments, smtpServer);
        }

        /// <summary>Creates a mail message from the specified information, and sends it to an SMTP server for delivery.</summary>
        /// <param name="from">The address of the mail message sender.</param>
        /// <param name="toRecipients">A comma-separated address list of the mail message recipients.</param>
        /// <param name="ccRecipients">A comma-separated address list of the mail message carbon copy (CC) recipients.</param>
        /// <param name="bccRecipients">A comma-separated address list of the mail message blank carbon copy (BCC) recipients.</param>
        /// <param name="subject">The subject of the mail message.</param>
        /// <param name="body">The body of the mail message.</param>
        /// <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        /// <param name="attachments">A comma-separated list of file names to be attached to the mail message.</param>
        /// <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        public static void Send(string from, string toRecipients, string ccRecipients, string bccRecipients, string subject, string body, bool isBodyHtml, string attachments, string smtpServer)
        {
            if (smtpServer == null)
                throw new ArgumentNullException("smtpServer", "No SMTP server was specified");

            MailMessage emailMessage = new MailMessage(from, toRecipients, subject, body);

            if (!string.IsNullOrEmpty(ccRecipients))
            {
                // Specifies the CC e-mail addresses for the e-mail message.
                foreach (string ccRecipient in ccRecipients.Replace(" ", "").Split(new char[] { ';', ',' }))
                {
                    emailMessage.CC.Add(ccRecipient);
                }
            }

            if (!string.IsNullOrEmpty(bccRecipients))
            {
                // Specifies the BCC e-mail addresses for the e-mail message.
                foreach (string bccRecipient in bccRecipients.Replace(" ", "").Split(new char[] { ';', ',' }))
                {
                    emailMessage.Bcc.Add(bccRecipient);
                }
            }

            if (!string.IsNullOrEmpty(attachments))
            {
                // Attaches all of the specified files to the e-mail message.
                foreach (string attachment in attachments.Replace(" ", "").Split(new char[] { ';', ',' }))
                {
                    // Creates the file attachment for the e-mail message.
                    Attachment data = new Attachment(attachment, MediaTypeNames.Application.Octet);
                    ContentDisposition header = data.ContentDisposition;

                    // Adds time stamp information for the file.
                    header.CreationDate = File.GetCreationTime(attachment);
                    header.ModificationDate = File.GetLastWriteTime(attachment);
                    header.ReadDate = File.GetLastAccessTime(attachment);

                    emailMessage.Attachments.Add(data); // Attaches the file.
                }
            }

            emailMessage.IsBodyHtml = isBodyHtml;

            SmtpClient smtpClient = new SmtpClient(smtpServer);

            smtpClient.Send(emailMessage);
        }

        #endregion
   }
}