using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;

// 04/03/2007


namespace TVA
{
	namespace Net
	{
		namespace Smtp
		{
			
			
			public class SimpleMailMessage
			{
				
				
				#region " Member Declaration "
				
				private string m_sender;
				private string m_recipients;
				private string m_ccRecipients;
				private string m_bccRecipients;
				private string m_subject;
				private string m_body;
				private string m_mailServer;
				private string m_attachments;
				private bool m_isBodyHtml;
				
				#endregion
				
				#region " Code Scope: Public "
				public const string DefaultSender = "postmaster@tva.gov";
				public const string DefaultMailServer = "mailhost.cha.tva.gov";
				
				public SimpleMailMessage()
				{
					
					m_sender = DefaultSender;
					m_mailServer = DefaultMailServer;
					
				}
				
				public SimpleMailMessage(string sender, string recipients, string subject, string body) : this()
				{
					
					m_sender = sender;
					m_recipients = recipients;
					m_subject = subject;
					m_body = body;
					
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
						if (! string.IsNullOrEmpty(value))
						{
							m_sender = value;
						}
						else
						{
							throw (new ArgumentNullException("Sender"));
						}
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
						if (! string.IsNullOrEmpty(value))
						{
							m_recipients = value;
						}
						else
						{
							throw (new ArgumentNullException("Recipients"));
						}
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
				public string MailServer
				{
					get
					{
						return m_mailServer;
					}
					set
					{
						if (! string.IsNullOrEmpty(value))
						{
							m_mailServer = value;
						}
						else
						{
							throw (new ArgumentNullException("MailServer"));
						}
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
					
					if (! string.IsNullOrEmpty(m_recipients))
					{
						// At the very least we require the recipients to be specified.
						MailMessage email = new MailMessage(m_sender, m_recipients, m_subject, m_body);
						SmtpClient emailSender = new SmtpClient(m_mailServer);
						
						if (! string.IsNullOrEmpty(CcRecipients))
						{
							// Specify the CC e-mail addresses for the e-mail message.
							foreach (string ccRecipient in CcRecipients.Replace(" ", "").Split(';', ','))
							{
								email.CC.Add(ccRecipient);
							}
						}
						
						if (! string.IsNullOrEmpty(BccRecipients))
						{
							// Specify the BCC e-mail addresses for the e-mail message.
							foreach (string bccRecipient in BccRecipients.Replace(" ", "").Split(';', ','))
							{
								email.Bcc.Add(bccRecipient);
							}
						}
						
						if (! string.IsNullOrEmpty(Attachments))
						{
							// Attach all of the specified files to the e-mail message.
							foreach (string attachment in Attachments.Split(';', ','))
							{
								if (File.Exists(attachment))
								{
									// Create the file attachment for the e-mail message.
									Attachment data = new Attachment(attachment, MediaTypeNames.Application.Octet);
									System.Net.Mime.ContentDisposition with_1 = Data.ContentDisposition;
									// Add time stamp information for the file.
									with_1.CreationDate = File.GetCreationTime(attachment);
									with_1.ModificationDate = File.GetLastWriteTime(attachment);
									with_1.ReadDate = File.GetLastAccessTime(attachment);
									
									email.Attachments.Add(data); // Attach the file.
								}
							}
						}
						
						email.IsBodyHtml = IsBodyHtml;
						
						try
						{
							emailSender.Send(email); // Send the e-mail message.
						}
						catch (Exception)
						{
							
						}
						finally
						{
							email.Dispose(); // Release the resources used by the e-mail message.
						}
					}
					else
					{
						throw (new InvalidOperationException("Recipients must be specified before sending the mail message."));
					}
					
				}
				
				#endregion
				
			}
			
		}
	}
}
