' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Web.Mail

Namespace [Shared]

    Public Class Mail

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Sends SMTP e-mail - separate multiple e-mail recipents with a semi-colon
        Public Shared Sub SendMail(ByVal SMTPServer As String, ByVal From As String, ByVal ToRecipients As String, ByVal Subject As String, ByVal HTMLMessage As String, Optional ByVal Importance As MailPriority = MailPriority.Normal)

            Dim msg As New MailMessage

            With msg
                .From = From
                .To = ToRecipients
                .Subject = Subject
                .Priority = Importance
                .BodyFormat = MailFormat.Html
                .Body = HTMLMessage
            End With

            SmtpMail.SmtpServer = SMTPServer
            SmtpMail.Send(msg)

        End Sub

        ' Sends SMTP e-mail - separate multiple e-mail recipents with a semi-colon
        Public Shared Sub SendMail(ByVal SMTPServer As String, ByVal From As String, ByVal ToRecipients As String, ByVal CCRecipients As String, ByVal BCCRecipients As String, ByVal Subject As String, ByVal Message As String, Optional ByVal Format As MailFormat = MailFormat.Html, Optional ByVal Importance As MailPriority = MailPriority.Normal)

            Dim msg As New MailMessage

            With msg
                .From = From
                .To = ToRecipients
                .Cc = CCRecipients
                .Bcc = BCCRecipients
                .Subject = Subject
                .Priority = Importance
                .BodyFormat = Format
                .Body = Message
            End With

            SmtpMail.SmtpServer = SMTPServer
            SmtpMail.Send(msg)

        End Sub

    End Class

End Namespace