'*******************************************************************************************************
'  Tva.Net.Smtp.Common.vb - Common e-mail related functions
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
'  12/30/2005 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Net
Imports System.Net.Mime
Imports System.Net.Mail

Namespace Net.Smtp

    ''' <summary>Defines common e-mail related functions.</summary>
    Public NotInheritable Class Common

        Public Const DefaultSmtpServer As String = "mailhost.cha.tva.gov"

        Private Sub New()

            ' This class contains only shared functions.

        End Sub

        ''' <summary>Creates a mail message from the specified information and sends it to an SMTP server for delivery.</summary>
        ''' <param name="from">The address of the mail message sender.</param>
        ''' <param name="toRecipients">A comma-seperated address list of the mail message recipients.</param>
        ''' <param name="subject">The subject of the mail message.</param>
        ''' <param name="body">The body of the mail message.</param>
        ''' <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        ''' <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        Public Shared Sub SendMail(ByVal from As String, ByVal toRecipients As String, ByVal subject As String, _
                ByVal body As String, ByVal isBodyHtml As Boolean, ByVal smtpServer As String)

            SendMail(from, toRecipients, Nothing, Nothing, subject, body, isBodyHtml, smtpServer)

        End Sub

        ''' <summary>Creates a mail message from the specified information and sends it to an SMTP server for delivery.</summary>
        ''' <param name="from">The address of the mail message sender.</param>
        ''' <param name="toRecipients">A comma-seperated address list of the mail message recipients.</param>
        ''' <param name="ccRecipients">A comma-seperated address list of the mail message carbon copy (CC) recipients.</param>
        ''' <param name="bccRecipients">A comma-seperated address list of the mail message blank carbon copy (BCC) recipients.</param>
        ''' <param name="subject">The subject of the mail message.</param>
        ''' <param name="body">The body of the mail message.</param>
        ''' <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        ''' <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        Public Shared Sub SendMail(ByVal from As String, ByVal toRecipients As String, ByVal ccRecipients As String, _
                ByVal bccRecipients As String, ByVal subject As String, ByVal body As String, ByVal isBodyHtml As Boolean, _
                ByVal smtpServer As String)

            SendMail(from, toRecipients, ccRecipients, bccRecipients, subject, body, isBodyHtml, Nothing, smtpServer)

        End Sub

        ''' <summary>Creates a mail message from the specified information and sends it to an SMTP server for delivery.</summary>
        ''' <param name="from">The address of the mail message sender.</param>
        ''' <param name="toRecipients">A comma-seperated address list of the mail message recipients.</param>
        ''' <param name="subject">The subject of the mail message.</param>
        ''' <param name="body">The body of the mail message.</param>
        ''' <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        ''' <param name="attachments">A comma-seperated list of file names to be attached to the mail message.</param>
        ''' <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        Public Shared Sub SendMail(ByVal from As String, ByVal toRecipients As String, _
                ByVal subject As String, ByVal body As String, ByVal isBodyHtml As Boolean, _
                ByVal attachments As String, ByVal smtpServer As String)

            SendMail(from, toRecipients, Nothing, Nothing, subject, body, isBodyHtml, attachments, smtpServer)

        End Sub

        ''' <summary>Creates a mail message from the specified information and sends it to an SMTP server for delivery.</summary>
        ''' <param name="from">The address of the mail message sender.</param>
        ''' <param name="toRecipients">A comma-seperated address list of the mail message recipients.</param>
        ''' <param name="ccRecipients">A comma-seperated address list of the mail message carbon copy (CC) recipients.</param>
        ''' <param name="bccRecipients">A comma-seperated address list of the mail message blank carbon copy (BCC) recipients.</param>
        ''' <param name="subject">The subject of the mail message.</param>
        ''' <param name="body">The body of the mail message.</param>
        ''' <param name="isBodyHtml">A boolean value indicating whether the mail message body is in Html.</param>
        ''' <param name="attachments">A comma-seperated list of file names to be attached to the mail message.</param>
        ''' <param name="smtpServer">The name or IP address of the SMTP server. Pass null or Nothing to use the default SMTP server.</param>
        Public Shared Sub SendMail(ByVal from As String, ByVal toRecipients As String, ByVal ccRecipients As String, _
                ByVal bccRecipients As String, ByVal subject As String, ByVal body As String, ByVal isBodyHtml As Boolean, _
                ByVal attachments As String, ByVal smtpServer As String)

            Dim emailMessage As New MailMessage(from, toRecipients, subject, body)
            With emailMessage
                If Not String.IsNullOrEmpty(ccRecipients) Then
                    ' Specify the CC e-mail addresses for the e-mail message.
                    For Each ccRecipient As String In ccRecipients.Replace(" ", "").Split(New Char() {";"c, ","c})
                        .CC.Add(ccRecipient)
                    Next
                End If

                If Not String.IsNullOrEmpty(bccRecipients) Then
                    ' Specify the BCC e-mail addresses for the e-mail message.
                    For Each bccRecipient As String In bccRecipients.Replace(" ", "").Split(New Char() {";"c, ","c})
                        .Bcc.Add(bccRecipient)
                    Next
                End If

                If Not String.IsNullOrEmpty(attachments) Then
                    ' Attach all of the specified files to the e-mail message.
                    For Each attachment As String In attachments.Replace(" ", "").Split(New Char() {";"c, ","c})
                        ' Create the file attachment for the e-mail message.
                        Dim data As New Attachment(attachment, MediaTypeNames.Application.Octet)
                        With data.ContentDisposition
                            ' Add time stamp information for the file.
                            .CreationDate = File.GetCreationTime(attachment)
                            .ModificationDate = File.GetLastWriteTime(attachment)
                            .ReadDate = File.GetLastAccessTime(attachment)
                        End With

                        .Attachments.Add(data)  ' Attach the file.
                    Next
                End If

                .IsBodyHtml = isBodyHtml
            End With

            With New SmtpClient()
                If smtpServer IsNot Nothing Then
                    .Host = smtpServer  ' Use the specified SMTP server for sending the e-mail.
                Else
                    .Host = DefaultSmtpServer   ' Use the default SMTP server for sending the e-mail.
                End If
                .Send(emailMessage) ' Send the e-mail message.
            End With

        End Sub

    End Class

End Namespace
