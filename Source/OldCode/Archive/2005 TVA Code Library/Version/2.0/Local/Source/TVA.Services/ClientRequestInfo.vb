' 02/11/2007

Public Class ClientRequestInfo

    Public Sub New(ByVal request As ClientRequest, ByVal sender As ClientInfo)

        MyClass.New(request, sender, Date.Now)

    End Sub

    Public Sub New(ByVal request As ClientRequest, ByVal sender As ClientInfo, ByVal receivedAt As Date)

        MyBase.New()
        Me.Request = request
        Me.Sender = sender
        Me.ReceivedAt = receivedAt

    End Sub

    Public Request As ClientRequest
    Public Sender As ClientInfo
    Public ReceivedAt As Date

End Class
