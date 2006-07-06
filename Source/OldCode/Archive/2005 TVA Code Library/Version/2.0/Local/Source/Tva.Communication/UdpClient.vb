' 07-06-06

Public Class UdpClient

    Public Overrides Sub CancelConnect()

    End Sub

    Public Overrides Sub Connect()

    End Sub

    Public Overrides Sub Disconnect()

    End Sub

    Protected Overrides Sub SendPreparedData(ByVal data() As Byte)

    End Sub

    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

    End Function

End Class
