' 02/11/2007

Public Class ClientRequestInfo

    Public Sub New(ByVal command As String, ByVal sender As ClientInfo, ByVal receivedAt As Date)

        MyBase.New()
        Me.Command = command
        Me.Sender = sender
        Me.ReceivedAt = receivedAt

    End Sub

    Public Command As String
    Public Sender As ClientInfo
    Public ReceivedAt As Date

End Class
