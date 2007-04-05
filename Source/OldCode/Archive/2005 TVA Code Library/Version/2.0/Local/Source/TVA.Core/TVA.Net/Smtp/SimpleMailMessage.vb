' 04/03/2007

Imports System.IO
Imports System.Net.Mail
Imports System.Net.Mime

Namespace Net.Smtp

    Public Class SimpleMailMessage

#Region " Member Declaration "

        Private m_sender As String
        Private m_recipients As String
        Private m_ccRecipients As String
        Private m_bccRecipients As String
        Private m_subject As String
        Private m_body As String
        Private m_mailServer As String
        Private m_attachments As String
        Private m_isBodyHtml As Boolean

#End Region

#Region " Code Scope: Public "
        Public Const DefaultSender As String = "postmaster@tva.gov"
        Public Const DefaultMailServer As String = "mailhost.cha.tva.gov"

        Public Sub New()

            MyBase.New()
            m_sender = DefaultSender
            m_mailServer = DefaultMailServer

        End Sub

        Public Sub New(ByVal sender As String, ByVal recipients As String, ByVal subject As String, ByVal body As String)

            MyClass.New()
            m_recipients = recipients
            m_subject = subject
            m_body = body

        End Sub

        ''' <summary>
        ''' Gets or sets the sender's address for this e-mail message.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The sender's address for this e-mail message.</returns>
        Public Property Sender() As String
            Get
                Return m_sender
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_sender = value
                Else
                    Throw New ArgumentNullException("Sender")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets the comma (,) or semicolon (;) delimited list of recipients for this e-mail message.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The comma (,) or semicolon (;) delimited list of recipients for this e-mail message.</returns>
        Public Property Recipients() As String
            Get
                Return m_recipients
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_recipients = value
                Else
                    Throw New ArgumentNullException("Recipients")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets the comma (,) or semicolon (;) delimited list of carbon copy recipients for this e-mail message.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The comma (,) or semicolon (;) delimited list of carbon copy recipients for this e-mail message.</returns>
        Public Property CcRecipients() As String
            Get
                Return m_ccRecipients
            End Get
            Set(ByVal value As String)
                m_ccRecipients = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the comma (,) or semicolon (;) delimited list of blind carbon copy recipients for this e-mail message.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The comma (,) or semicolon (;) delimited list of blind carbon copy recipients for this e-mail message.</returns>
        Public Property BccRecipients() As String
            Get
                Return m_bccRecipients
            End Get
            Set(ByVal value As String)
                m_bccRecipients = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the subject line for this e-mail message.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The subject line for this e-mail message.</returns>
        Public Property Subject() As String
            Get
                Return m_subject
            End Get
            Set(ByVal value As String)
                m_subject = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the message body.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The body text.</returns>
        Public Property Body() As String
            Get
                Return m_body
            End Get
            Set(ByVal value As String)
                m_body = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name or IP address of the mail server for this e-mail message.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The name or IP address of the mail server for this e-mail message.</returns>
        Public Property MailServer() As String
            Get
                Return m_mailServer
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_mailServer = value
                Else
                    Throw New ArgumentNullException("MailServer")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the comma (,) or semicolon (;) delimited list of file names that are to be attached to this e-mail message.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The comma (,) or semicolon (;) delimited list of file names that are to be attached to this e-mail message.</returns>
        Public Property Attachments() As String
            Get
                Return m_attachments
            End Get
            Set(ByVal value As String)
                m_attachments = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the mail message body is in Html.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the message body is in Html; otherwise False.</returns>
        Public Property IsBodyHtml() As Boolean
            Get
                Return m_isBodyHtml
            End Get
            Set(ByVal value As Boolean)
                m_isBodyHtml = value
            End Set
        End Property

        ''' <summary>
        ''' Send this e-mail message to its recipients.
        ''' </summary>
        Public Sub Send()

            If Not String.IsNullOrEmpty(m_recipients) Then
                ' At the very least we require the recipients to be specified.
                Dim email As New MailMessage(m_sender, m_recipients, m_subject, m_body)
                Dim emailSender As New SmtpClient(m_mailServer)

                If Not String.IsNullOrEmpty(CcRecipients) Then
                    ' Specify the CC e-mail addresses for the e-mail message.
                    For Each ccRecipient As String In CcRecipients.Replace(" ", "").Split(New Char() {";"c, ","c})
                        email.CC.Add(ccRecipient)
                    Next
                End If

                If Not String.IsNullOrEmpty(BccRecipients) Then
                    ' Specify the BCC e-mail addresses for the e-mail message.
                    For Each bccRecipient As String In BccRecipients.Replace(" ", "").Split(New Char() {";"c, ","c})
                        email.Bcc.Add(bccRecipient)
                    Next
                End If

                If Not String.IsNullOrEmpty(Attachments) Then
                    ' Attach all of the specified files to the e-mail message.
                    For Each attachment As String In Attachments.Split(New Char() {";"c, ","c})
                        ' Create the file attachment for the e-mail message.
                        Dim data As New Attachment(attachment, MediaTypeNames.Application.Octet)
                        With data.ContentDisposition
                            ' Add time stamp information for the file.
                            .CreationDate = File.GetCreationTime(attachment)
                            .ModificationDate = File.GetLastWriteTime(attachment)
                            .ReadDate = File.GetLastAccessTime(attachment)
                        End With

                        email.Attachments.Add(data)  ' Attach the file.
                    Next
                End If

                email.IsBodyHtml = IsBodyHtml

                emailSender.Send(email) ' Send the e-mail message.
            Else
                Throw New InvalidOperationException("Recipients must be specified before sending the mail message.")
            End If

        End Sub

#End Region

    End Class

End Namespace