//*******************************************************************************************************
//  SimpleMailMessage.cs
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
//  04/03/2007 - Pinal C. Patel
//       Generated original version of source code.
//  12/12/2007 - Darrell Zuercher
//       Edited Code Comments.
//  09/22/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Collections.Generic;

namespace TVA.Net.Smtp
{
    public class SimpleMailMessage
    {
        private string m_sender;
        private string m_recipients;
        private string m_ccRecipients;
        private string m_bccRecipients;
        private string m_subject;
        private string m_body;
        private string m_smtpServer;
        private string m_attachments;
        private bool m_isBodyHtml;

        public SimpleMailMessage()
        {
        }

        public SimpleMailMessage(string smtpServer)
        {
            m_smtpServer = smtpServer;
        }

        /// <summary>
        /// Gets or sets the sender's address for this e-mail message.
        /// </summary>
        /// <value></value>
        /// <returns>The sender's address for this e-mail message.</returns>
        public string Sender
        {
            get
            {
                return m_sender;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_sender = value;
                else
                    throw new ArgumentNullException("Sender");
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

        /// <summary>
        /// Send this e-mail message to its recipients.
        /// </summary>
        public void Send()
        {
            Mail.Send(m_sender, m_recipients, m_ccRecipients, m_bccRecipients, m_subject, m_body, m_isBodyHtml, m_attachments, m_smtpServer);
        }
    }
}