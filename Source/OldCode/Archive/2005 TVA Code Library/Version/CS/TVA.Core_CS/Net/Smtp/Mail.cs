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
//       Converted to C#.
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
    public static class Mail
    {
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
    }
}