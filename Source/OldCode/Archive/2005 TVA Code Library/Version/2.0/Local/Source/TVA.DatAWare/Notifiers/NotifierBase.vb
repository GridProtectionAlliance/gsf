' 05/26/2007

Namespace Notifiers

    Public MustInherit Class NotifierBase
        Implements INotifier

#Region " Member Declaration "

        Private m_notifiesAlarms As Boolean
        Private m_notifiesWarnings As Boolean
        Private m_notifiesInformation As Boolean
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String

#End Region

#Region " Code Scope: Public "

        Public Sub New(ByVal notifiesInformation As Boolean, ByVal notifiesWarnings As Boolean, ByVal notifiesAlarms As Boolean)

            MyBase.New()
            m_notifiesInformation = notifiesInformation
            m_notifiesWarnings = notifiesWarnings
            m_notifiesAlarms = notifiesAlarms
            m_persistSettings = True
            m_settingsCategoryName = Me.GetType().Name

        End Sub

#Region " Interface Implementation "

#Region " INotifier "

        Public Property NotifiesAlarms() As Boolean Implements INotifier.NotifiesAlarms
            Get
                Return m_notifiesAlarms
            End Get
            Set(ByVal value As Boolean)
                m_notifiesAlarms = value
            End Set
        End Property

        Public Property NotifiesWarnings() As Boolean Implements INotifier.NotifiesWarnings
            Get
                Return m_notifiesWarnings
            End Get
            Set(ByVal value As Boolean)
                m_notifiesWarnings = value
            End Set
        End Property

        Public Property NotifiesInformation() As Boolean Implements INotifier.NotifiesInformation
            Get
                Return m_notifiesInformation
            End Get
            Set(ByVal value As Boolean)
                m_notifiesInformation = value
            End Set
        End Property

        Public Sub Notify(ByVal subject As String, ByVal message As String, ByVal notificationType As NotificationType) Implements INotifier.Notify

            Select Case True
                Case m_notifiesAlarms AndAlso notificationType = Notifiers.NotificationType.Alarm
                    NotifyAlarm(subject, message)
                Case m_notifiesWarnings AndAlso notificationType = Notifiers.NotificationType.Warning
                    NotifyWarning(subject, message)
                Case m_notifiesInformation AndAlso notificationType = Notifiers.NotificationType.Information
                    NotifyInformation(subject, message)
            End Select

        End Sub

#End Region

#Region " IPersistSettings "

        Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
            Get
                Return m_persistSettings
            End Get
            Set(ByVal value As Boolean)
                m_persistSettings = value
            End Set
        End Property

        Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
            Get
                Return m_settingsCategoryName
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_settingsCategoryName = value
                Else
                    Throw New ArgumentNullException("SettingsCategoryName")
                End If
            End Set
        End Property

        Public Overridable Sub LoadSettings() Implements IPersistSettings.LoadSettings

            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    If .Count > 0 Then
                        NotifiesAlarms = .Item("NotifiesAlarms").GetTypedValue(m_notifiesAlarms)
                        NotifiesWarnings = .Item("NotifiesWarnings").GetTypedValue(m_notifiesWarnings)
                        NotifiesInformation = .Item("NotifiesInformation").GetTypedValue(m_notifiesInformation)
                    End If
                End With
            Catch ex As Exception
                ' We'll encounter exceptions if the settings are not present in the config file.
            End Try

        End Sub

        Public Overridable Sub SaveSettings() Implements IPersistSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                        .Clear()
                        With .Item("NotifiesAlarms", True)
                            .Value = m_notifiesAlarms.ToString()
                            .Description = "True if alarm notifications are to be sent; otherwise False."
                        End With
                        With .Item("NotifiesWarnings", True)
                            .Value = m_notifiesWarnings.ToString()
                            .Description = "True if warning notifications are to be sent; otherwise False."
                        End With
                        With .Item("NotifiesInformation", True)
                            .Value = m_notifiesInformation.ToString()
                            .Description = "True if information notifications are to be sent; otherwise False."
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

#End Region

#Region " Code Scope: Protected "

        Protected MustOverride Sub NotifyAlarm(ByVal subject As String, ByVal message As String)

        Protected MustOverride Sub NotifyWarning(ByVal subject As String, ByVal message As String)

        Protected MustOverride Sub NotifyInformation(ByVal subject As String, ByVal message As String)

#End Region

    End Class

End Namespace