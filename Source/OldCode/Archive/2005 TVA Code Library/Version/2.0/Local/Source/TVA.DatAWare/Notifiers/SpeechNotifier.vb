' 05/29/2007

Imports System.Text
Imports System.Messaging

Namespace Notifiers

    Public Class SpeechNotifier
        Inherits NotifierBase

#Region " Member Declaration "

        Private m_speechMsmqPath As String
        Private m_speechApplicationUrl As String
        Private m_messageRecipients As String

#End Region

#Region " Code Scope: Public "

        Public Sub New()

            MyBase.New(True, True, True)

        End Sub

        Public Property SpeechMsmqPath() As String
            Get
                Return m_speechMsmqPath
            End Get
            Set(ByVal value As String)
                m_speechMsmqPath = value
            End Set
        End Property

        Public Property SpeechApplicationUrl() As String
            Get
                Return m_speechApplicationUrl
            End Get
            Set(ByVal value As String)
                m_speechApplicationUrl = value
            End Set
        End Property

        Public Property MessageRecipients() As String
            Get
                Return m_messageRecipients
            End Get
            Set(ByVal value As String)
                m_messageRecipients = value
            End Set
        End Property

#Region " Overrides "

        Public Overrides Sub LoadSettings()

            MyBase.LoadSettings()

            Try
                With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                    If .Count > 0 Then
                        SpeechMsmqPath = .Item("SpeechMsmqPath").GetTypedValue(m_speechMsmqPath)
                        SpeechApplicationUrl = .Item("SpeechApplicationUrl").GetTypedValue(m_speechApplicationUrl)
                        MessageRecipients = .Item("MessageRecipients").GetTypedValue(m_messageRecipients)
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
                        With .Item("SpeechMsmqPath", True)
                            .Value = m_speechMsmqPath
                            .Description = "Path of the MSMQ that will be used for sending speech notification messages to the speech server."
                        End With
                        With .Item("SpeechApplicationUrl", True)
                            .Value = m_speechApplicationUrl
                            .Description = "URL of the speech application that will be sent to the speech server as speech notification message using MSMQ."
                        End With
                        With .Item("MessageRecipients", True)
                            .Value = m_messageRecipients
                            .Description = "Phone numbers of the recipients for the speech notifications."
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

        Protected Overrides Sub NotifyAlarm(ByVal subject As String, ByVal message As String, ByVal details As String)

            MakeCall(message, "Alarm")

        End Sub

        Protected Overrides Sub NotifyWarning(ByVal subject As String, ByVal message As String, ByVal details As String)

            MakeCall(message, "Warning")

        End Sub

        Protected Overrides Sub NotifyInformation(ByVal subject As String, ByVal message As String, ByVal details As String)

            MakeCall(message, "Information")

        End Sub

#End Region

#End Region

#Region " Code Scope: Private "

        Private Sub MakeCall(ByVal message As String, ByVal messageType As String)

            If Not String.IsNullOrEmpty(m_speechMsmqPath) AndAlso _
                    Not String.IsNullOrEmpty(m_speechApplicationUrl) AndAlso _
                    Not String.IsNullOrEmpty(m_messageRecipients) Then
                Dim finalMessage As New StringBuilder()
                finalMessage.AppendFormat("The following is an automated {0} message from {1}.", messageType, MachineName)
                finalMessage.Append("   ")
                finalMessage.Append(message)

                Dim queue As MessageQueue = New MessageQueue(m_speechMsmqPath) '("Formatname:DIRECT=OS:rgocmsspeech2\acts")
                Dim qMessage As String = String.Format(m_speechApplicationUrl, "{0}", finalMessage.ToString()) '"http://localhost/ELCPSpeechTest/Default.aspx?PhNum={0}&Msg={1}"
                For Each recipient As String In m_messageRecipients.Replace(" ", "").Split(";"c, ","c)
                    queue.Send(New Message(String.Format(qMessage, recipient)))
                Next
                queue.Dispose()
            End If

        End Sub

        Private Function MachineName() As String

            With New StringBuilder()
                For Each letter As Char In Environment.MachineName.ToCharArray()
                    .Append(letter)
                    .Append(" ")
                Next

                Return .ToString()
            End With

        End Function

#End Region

    End Class

End Namespace