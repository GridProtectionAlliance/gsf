' 05/26/2007

Namespace Notifiers

    Public Interface INotifier

        Property NotifiesErrors() As Boolean

        Property NotifiesWarnings() As Boolean

        Property NotifiesInformation() As Boolean

        Sub Notify(ByVal subject As String, ByVal message As String, ByVal notificationType As NotificationType)

    End Interface

End Namespace