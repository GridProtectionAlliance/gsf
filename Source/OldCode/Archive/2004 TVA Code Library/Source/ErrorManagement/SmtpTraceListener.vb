'Author: Pinal Patel
'Created: 03/18/05
'Modified: 03/21/05
'Description:   This is a trace listner that listens for trace messages and emails them to the specified recipient.
'               To use this trace listner a reference must be added to TVA.ErrorManagement class library and it
'               can then be used in one of the following two ways:
'               1) Add the following code to the config file:
'                   <system.diagnostics>
'                     <trace>
'                       <listeners>
'                         <add name="oSmtpListener" type="TVA.ErrorManagement.SmtpTraceListener, TVA.ErrorManagement" initializeData="sender@tva.gov,recipient@tva.gov,mailhost.cha.tva.gov" />
'                       </listeners>
'                     </trace>
'                   </system.diagnostics>
'               2) Instantiate and add the trace listener.
'                   Dim oSmtpListener As TVA.ErrorManagement.SmtpTraceListener = New TVA.ErrorManagement.SmtpTraceListener("sender@tva.gov,recipient@tva.gov,mailhost.cha.tva.gov")
'                   System.Diagnostics.Trace.Listeners.Add(oSmtpListener)
'               After setting-up the SmtpTraceListner, a trace email message can be sent like:
'                   System.Diagnostics.Trace.Write("Your trace message.")



'Namespaces used.
Imports System.Diagnostics
Imports TVA.Shared.Mail

Namespace ErrorManagement

    Public Class SmtpTraceListener
        Inherits TraceListener

        Private _Sender As String
        Private _Recipient As String
        Private _SmtpServer As String

        Public Sub New()

            MyBase.New()
            'Initialization data was not provided.
            Throw New ApplicationException("Missing initialization data for SmtpTraceListner. Initialization data must be provided in the following format: 'sender@email.com,recipient@email.com,smpt.email.com'.")

        End Sub

        Public Sub New(ByVal InitializationData As String)

            MyBase.New()

            Dim SmtpData As String() = InitializationData.Split(",")
            If SmtpData.Length < 3 Then
                'Insufficient initialization data was provided.
                Throw New ApplicationException("Insufficient initialization data provided for SmtpTraceListner. Initialization data must be provided in the following format: 'sender@email.com,recipient@email.com,smpt.email.com'.")
            End If

            'Initialize private variables.
            Me._Sender = SmtpData(0)
            Me._Recipient = SmtpData(1)
            Me._SmtpServer = SmtpData(2)

        End Sub

        Public Overloads Overrides Sub Write(ByVal message As String)

            'Append standard information to the bottom of the message.
            message &= vbCrLf & vbCrLf & "<H5>This trace message was sent from the machine " & System.Net.Dns.GetHostName() & " (" & System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList(0).ToString() & ") at " & DateTime.Now() & ".</H5>"

            'Apply proper html formatting to the message.
            message = System.Text.RegularExpressions.Regex.Replace(message, vbCrLf, "<BR />")

            'Email the trace message.
            SendMail(Me._SmtpServer, Me._Sender, Me._Recipient, "Trace Message From " & Me._Sender, message)

        End Sub

        Public Overloads Overrides Sub WriteLine(ByVal message As String)

            'Email the trace message.
            Me.Write(message)

        End Sub
    End Class

End Namespace
