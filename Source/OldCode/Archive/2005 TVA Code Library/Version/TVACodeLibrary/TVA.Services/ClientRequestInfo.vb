' 02/11/2007

Public Class ClientRequestInfo

    Public Sub New(ByVal sender As ClientInfo, ByVal request As ClientRequest)

        MyClass.New(sender, request, Date.Now)

    End Sub

    Public Sub New(ByVal sender As ClientInfo, ByVal request As ClientRequest, ByVal receivedAt As Date)

        MyBase.New()
        Me.Request = request
        Me.Sender = sender
        Me.ReceivedAt = receivedAt

    End Sub

    Public Request As ClientRequest
    Public Sender As ClientInfo
    Public ReceivedAt As Date

End Class
