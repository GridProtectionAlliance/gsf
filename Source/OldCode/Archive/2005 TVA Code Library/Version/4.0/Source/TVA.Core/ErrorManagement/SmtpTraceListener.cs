//*******************************************************************************************************
//  SmtpTraceListener.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/18/2005 - Pinal C. Patel
//       Generated original version of source code.
//  01/04/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.ErrorManagement.SmtpTraceListener).
//  09/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/23/2008 - James R Carroll
//       Converted to C#.
//  10/17/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using TVA.Net.Smtp;

namespace TVA.ErrorManagement
{
    /// <summary>
    /// Defines an e-mail based <see cref="TraceListener"/>.
    /// </summary>
    /// <example>
    /// Below is the config file entry required for enabling e-mail based tracing using <see cref="SmtpTraceListener"/>:
    /// <code>
    /// <![CDATA[
    /// <configuration>
    ///   <system.diagnostics>
    ///     <trace>
    ///       <listeners>
    ///         <add name="SmtpTraceListener" type="TVA.ErrorManagement.SmtpTraceListener,TVA.Core" initializeData="sender@email.com,recipient@email.com,smtp.email.com"/>
    ///       </listeners>
    ///     </trace>
    ///   </system.diagnostics>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class SmtpTraceListener : TraceListener
    {
        #region [ Members ]

        // Fields
        private string m_sender;
        private string m_recipient;
        private string m_smtpServer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpTraceListener"/> class.
        /// </summary>
        public SmtpTraceListener()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpTraceListener"/> class.
        /// </summary>
        /// <param name="initializationData">Initialization text in the format of "sender@email.com,recipient@email.com,smtp.email.com".</param>
        public SmtpTraceListener(string initializationData)
        {
            string[] smtpData = initializationData.Split(',');

            // Check to see if sufficient initialization data was provided.
            if (smtpData.Length < 3)
                throw new ArgumentException("Insufficient initialization data provided. Initialization data must be provided in the following format: \"sender@email.com,recipient@email.com,smtp.email.com\".");

            // Initializes private variables.
            m_sender = smtpData[0].Trim();
            m_recipient = smtpData[1].Trim();
            m_smtpServer = smtpData[2].Trim();
        }

        #endregion

        #region [ Methods ]
        
        /// <summary>
        /// Sends an e-mail message containing the specified message.
        /// </summary>
        /// <param name="message">The message to be sent in the e-mail message.</param>
        public override void Write(string message)
        {
            StringBuilder messageBuilder = new StringBuilder();

            // Appends standard information to the bottom of the message.
            messageBuilder.Append(message);
            messageBuilder.AppendLine();
            messageBuilder.AppendLine();
            messageBuilder.Append("This trace message was sent from the machine ");
            messageBuilder.Append(Dns.GetHostName());
            messageBuilder.Append(" (");
            messageBuilder.Append(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
            messageBuilder.Append(") at ");
            messageBuilder.Append(DateTime.Now);

            Mail mailMessage = new Mail(m_sender, m_recipient, m_smtpServer);
            mailMessage.Subject = "Trace message for " + AppDomain.CurrentDomain.FriendlyName;
            mailMessage.Body = messageBuilder.ToString();
            mailMessage.Send();
        }

        /// <summary>
        /// Sends an e-mail message containing the specified message.
        /// </summary>
        /// <param name="message">The message to be sent in the e-mail message.</param>
        public override void WriteLine(string message)
        {
            Write(message);
        }

        #endregion
    }
}