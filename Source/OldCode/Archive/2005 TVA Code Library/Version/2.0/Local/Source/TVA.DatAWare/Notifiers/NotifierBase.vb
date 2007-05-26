' 05/26/2007

Namespace Notifiers

    Public MustInherit Class NotifierBase
        Implements INotifier, IPersistSettings

#Region " Member Declaration "

        Private m_notifiesErrors As Boolean
        Private m_notifiesWarnings As Boolean
        Private m_notifiesInformation As Boolean

#End Region

#Region " Code Scope: Public "

        Public Sub New(ByVal notifiesInformation As Boolean, ByVal notifiesWarnings As Boolean, ByVal notifiesErrors As Boolean)

            MyBase.New()
            m_notifiesInformation = notifiesInformation
            m_notifiesWarnings = notifiesWarnings
            m_notifiesErrors = notifiesErrors

        End Sub

        Public Property NotifiesErrors() As Boolean Implements INotifier.NotifiesErrors
            Get
                Return m_notifiesErrors
            End Get
            Set(ByVal value As Boolean)
                m_notifiesErrors = value
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
                Case m_notifiesErrors AndAlso notificationType = Notifiers.NotificationType.Error
                    NotifyError(subject, message)
                Case m_notifiesWarnings AndAlso notificationType = Notifiers.NotificationType.Warning
                    NotifyWarning(subject, message)
                Case m_notifiesInformation AndAlso notificationType = Notifiers.NotificationType.Information
                    NotifyInformation(subject, message)
            End Select

        End Sub

#End Region

#Region " Code Scope: Protected "

        Protected MustOverride Sub NotifyError(ByVal subject As String, ByVal message As String)

        Protected MustOverride Sub NotifyWarning(ByVal subject As String, ByVal message As String)

        Protected MustOverride Sub NotifyInformation(ByVal subject As String, ByVal message As String)

#End Region

        Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

        End Sub

        Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
            Get

            End Get
            Set(ByVal value As Boolean)

            End Set
        End Property

        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

        End Sub

        Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
            Get

            End Get
            Set(ByVal value As String)

            End Set
        End Property

    End Class

End Namespace