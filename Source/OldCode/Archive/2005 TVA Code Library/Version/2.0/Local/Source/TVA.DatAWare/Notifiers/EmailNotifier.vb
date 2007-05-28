' 05/28/2007

Imports TVA.Net.Smtp

Namespace Notifiers

    Public Class EmailNotifier
        Inherits NotifierBase

#Region " Member Declaration "

        Private m_smtpServer As String
        Private m_emailAddresses As String

#End Region

#Region " Code Scope: Public "

        Public Sub New()

            MyBase.New(True, True, True)
            m_smtpServer = SimpleMailMessage.DefaultMailServer

        End Sub

        Public Property SmtpServer() As String
            Get
                Return m_smtpServer
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_smtpServer = value
                Else
                    Throw New ArgumentNullException()
                End If
            End Set
        End Property

        Public Property EmailAddresses() As String
            Get
                Return m_emailAddresses
            End Get
            Set(ByVal value As String)
                m_emailAddresses = value
            End Set
        End Property

#Region " Overrides "

        Public Overrides Sub LoadSettings()

            MyBase.LoadSettings()

            Try
                With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                    If .Count > 0 Then
                        SmtpServer = .Item("SmtpServer").GetTypedValue(m_smtpServer)
                        EmailAddresses = .Item("EmailAddresses").GetTypedValue(m_emailAddresses)
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
                        With .Item("SmtpServer", True)
                            .Value = m_smtpServer
                            .Description = ""
                        End With
                        With .Item("EmailAddresses", True)
                            .Value = m_emailAddresses
                            .Description = ""
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

        Protected Overrides Sub NotifyError(ByVal subject As String, ByVal message As String)

        End Sub

        Protected Overrides Sub NotifyWarning(ByVal subject As String, ByVal message As String)

        End Sub

        Protected Overrides Sub NotifyInformation(ByVal subject As String, ByVal message As String)

        End Sub

#End Region

#End Region

    End Class

End Namespace