'*******************************************************************************************************
'  TVA.ErrorManagement.SmtpTraceListener.vb - Defines an e-mail based trace listener
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  03/18/2005 - Pinal C. Patel
'       Original version of source code generated
'  01/04/2006 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.ErrorManagement.SmtpTraceListener)
'
'*******************************************************************************************************

Imports System.Text
Imports System.Diagnostics
Imports TVA.Net.Smtp

Namespace ErrorManagement

    ''' <summary>Defines an e-mail based trace listener.</summary>
    Public Class SmtpTraceListener

        Inherits TraceListener

        Private m_sender As String
        Private m_recipient As String
        Private m_smtpServer As String

        Public Sub New()

            MyBase.New()

        End Sub

        Public Sub New(ByVal initializationData As String)

            MyBase.New()

            Dim smtpData As String() = initializationData.Split(",")

            If smtpData.Length < 3 Then
                ' Insufficient initialization data was provided.
                Throw New ArgumentException("Insufficient initialization data provided for Smtp.TraceListner. Initialization data must be provided in the following format: 'sender@email.com, recipient@email.com, smtp.email.com'.")
            End If

            ' Initialize private variables.
            m_sender = smtpData(0)
            m_recipient = smtpData(1)
            m_smtpServer = smtpData(2)

        End Sub

        Public Overloads Overrides Sub Write(ByVal message As String)

            Dim messageBuilder As New StringBuilder()
            messageBuilder.Append(message)
            ' Append standard information to the bottom of the message.
            messageBuilder.Append(Environment.NewLine() & Environment.NewLine())
            messageBuilder.Append("This trace message was sent from the machine ")
            messageBuilder.Append(System.Net.Dns.GetHostName())
            messageBuilder.Append(" (")
            messageBuilder.Append(System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList(0).ToString())
            messageBuilder.Append(") at ")
            messageBuilder.Append(System.DateTime.Now)

            Dim mailMessage As New SimpleMailMessage()
            mailMessage.Sender = m_sender
            mailMessage.Recipients = m_recipient
            mailMessage.Subject = "Trace message for " & System.AppDomain.CurrentDomain.FriendlyName
            mailMessage.Body = messageBuilder.ToString()
            mailMessage.Send()

        End Sub

        Public Overloads Overrides Sub WriteLine(ByVal message As String)

            ' Email the trace message.
            Write(message)

        End Sub

    End Class

End Namespace