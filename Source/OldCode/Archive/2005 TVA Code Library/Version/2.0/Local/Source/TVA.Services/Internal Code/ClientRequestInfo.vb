' 02/11/2007

Friend Class RequestInfo

    Public Sub New(ByVal text As String, ByVal sender As Guid, ByVal receivedAt As System.DateTime)

        RequestType = text
        RequestSender = sender
        RequestReceivedAt = receivedAt

    End Sub

    Public RequestType As String
    Public RequestSender As Guid
    Public RequestReceivedAt As System.DateTime

End Class
