' James Ritchie Carroll - 2003
Option Explicit On 
Imports System.Web.Mail
Namespace [Shared]
    ''' <summary>
    ''' <para>
    ''' Defines common global functions related to sending e-mail from .NET
    ''' </para>
    ''' </summary>
    Public Class Mail
        Private Sub New()
            ' This class contains only global functions and is not meant to be instantiated
        End Sub
        ''' <summary>
        ''' <para>Sends SMTP e-mail - separate multiple e-mail recipents with a semi-colon</para>
        ''' </summary>
        ''' <param name="SMTPServer"> Required. SMTP relay mail server to use to send e-mail. </param>
        '''<param name="From"> Required.The address of the e-mail sender.</param>
        ''' <param name="ToRecipients"> Required.The address of the e-mail recipient.</param>
        ''' <param name="Subject"> Required.The subject line of the e-mail message.</param>
        ''' <param name="HTMLMessage"> Required.Specifies that the e-mail format is HTML.</param>
        ''' <param name="Importance"> Optional.Specifies that the e-mail message has normal priority.</param>
        Public Shared Sub SendMail(ByVal SMTPServer As System.String, ByVal From As System.String, ByVal ToRecipients As System.String, ByVal Subject As System.String, ByVal HTMLMessage As System.String, Optional ByVal Importance As System.Web.Mail.MailPriority = MailPriority.Normal)
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
        ''' <summary>
        ''' <para>Sends SMTP e-mail - separate multiple e-mail recipents with a semi-colon,carbon copy and Blind carbon copy</para>
        ''' </summary>
        ''' <param name="SMTPServer"> Required. SMTP relay mail server to use to send e-mail. </param>
        '''<param name="From"> Required.The address of the e-mail sender.</param>
        ''' <param name="ToRecipients"> Required.The address of the e-mail recipient.</param>
        ''' <param name="CCRecipients"> Required.List of email addresses that receive a carbon copy (CC) of the e-mail message</param>
        ''' <param name="BCCRecipients"> Required.List of email addresses that receive a blind carbon copy (BCC) of the e-mail message</param>
        ''' <param name="Subject"> Required.The subject line of the e-mail message.</param>
        ''' <param name="Message"> Required.The body of the e-mail message.</param>
        ''' <param name="MailFormat"> Optional.Specifies that the e-mail format is HTML</param>
        ''' <param name="Importance"> Optional.Specifies that the e-mail message has normal priority.</param>
        Public Shared Sub SendMail(ByVal SMTPServer As System.String, ByVal From As System.String, ByVal ToRecipients As System.String, ByVal CCRecipients As System.String, ByVal BCCRecipients As System.String, ByVal Subject As System.String, ByVal Message As System.String, Optional ByVal Format As System.Web.Mail.MailFormat = MailFormat.Html, Optional ByVal Importance As System.Web.Mail.MailPriority = MailPriority.Normal)
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