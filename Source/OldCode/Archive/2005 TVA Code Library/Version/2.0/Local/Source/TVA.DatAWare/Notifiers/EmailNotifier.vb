' 05/28/2007

Imports TVA.Net.Smtp

Namespace Notifiers

    Public Class EmailNotifier
        Inherits NotifierBase

#Region " Member Declaration "

        Private m_emailServer As String
        Private m_emailRecipients As String

#End Region

#Region " Code Scope: Public "

        Public Sub New()

            MyBase.New(True, True, True)
            m_emailServer = SimpleMailMessage.DefaultMailServer

        End Sub

        Public Property EmailServer() As String
            Get
                Return m_emailServer
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_emailServer = value
                Else
                    Throw New ArgumentNullException()
                End If
            End Set
        End Property

        Public Property EmailRecipients() As String
            Get
                Return m_emailRecipients
            End Get
            Set(ByVal value As String)
                m_emailRecipients = value
            End Set
        End Property

#Region " Overrides "

        Public Overrides Sub LoadSettings()

            MyBase.LoadSettings()

            Try
                With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                    If .Count > 0 Then
                        EmailServer = .Item("EmailServer").GetTypedValue(m_emailServer)
                        EmailRecipients = .Item("EmailRecipients").GetTypedValue(m_emailRecipients)
                    End If
                End With
            Catch ex As Exception
                ' We'll encounter exceptions if the settings are not present in the config file.
            End Try

        End Sub

        Public Overrides Sub SaveSettings()

            MyBase.SaveSettings()

            If PersistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                        With .Item("EmailServer", True)
                            .Value = m_emailServer
                            .Description = "SMTP server to use for sending the email notifications."
                        End With
                        With .Item("EmailRecipients", True)
                            .Value = m_emailRecipients
                            .Description = "Email addresses of the recipients for the notifications."
                        End With
                    End With
                    TVA.Configuration.Common.SaveSettings()
                Catch ex As Exception
                    ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
                End Try
            End If

        End Sub

#End Region

#End Region

#Region " Code Scope: Protected "

#Region " Overrides "

        Protected Overrides Sub NotifyAlarm(ByVal subject As String, ByVal message As String)

            subject = "ALARM: " & subject
            SendEmail(subject, message)

        End Sub

        Protected Overrides Sub NotifyWarning(ByVal subject As String, ByVal message As String)

            subject = "WARNING: " & subject
            SendEmail(subject, message)

        End Sub

        Protected Overrides Sub NotifyInformation(ByVal subject As String, ByVal message As String)

            subject = "INFO: " & subject
            SendEmail(subject, message)

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub SendEmail(ByVal subject As String, ByVal message As String)

            If Not String.IsNullOrEmpty(m_emailRecipients) Then
                With New SimpleMailMessage()
                    ' Prepare the email message.
                    .MailServer = m_emailServer
                    .Sender = String.Format("{0}@tva.gov", Environment.MachineName)
                    .Recipients = m_emailRecipients
                    .Subject = subject
                    .Body = message

                    ' Send the prepared email message.
                    .Send()
                End With
            End If

        End Sub

#End Region

#End Region

    End Class

End Namespace