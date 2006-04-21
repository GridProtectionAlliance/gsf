'*******************************************************************************************************
'  Tva.Net.Smtp.TraceListener.vb - Defines an e-mail based trace listener
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  03/18/2005 - Pinal C Patel
'       Original version of source code generated
'  01/04/2006 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.ErrorManagement.SmtpTraceListener)
'
'*******************************************************************************************************

Imports System.Diagnostics
Imports Tva.Net.Smtp.Common

Namespace ErrorManagement

    ''' <summary>Defines an e-mail based trace listener</summary>
    Public Class SmtpTraceListener

        Inherits Diagnostics.TraceListener

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

            'Initialize private variables.
            m_sender = smtpData(0)
            m_recipient = smtpData(1)
            m_smtpServer = smtpData(2)

        End Sub

        Public Overloads Overrides Sub Write(ByVal message As String)

            'Append standard information to the bottom of the message.
            message &= Environment.NewLine & Environment.NewLine & "<H5>This trace message was sent from the machine " & System.Net.Dns.GetHostName() & " (" & System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList(0).ToString() & ") at " & System.DateTime.Now() & ".</H5>"

            'Apply proper html formatting to the message.
            message = System.Text.RegularExpressions.Regex.Replace(message, Environment.NewLine, "<BR />")

            'Email the trace message.
            SendMail(m_sender, m_recipient, "Trace Message From " & m_sender, message, True, m_smtpServer)

        End Sub

        Public Overloads Overrides Sub WriteLine(ByVal message As String)

            ' Email the trace message.
            Write(message)

        End Sub

    End Class

End Namespace