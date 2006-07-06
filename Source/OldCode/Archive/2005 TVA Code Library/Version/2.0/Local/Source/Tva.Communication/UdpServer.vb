' 07-06-06

Public Class UdpServer

    Public Overrides Sub Start()

    End Sub

    Public Overrides Sub [Stop]()

    End Sub

    Protected PacketSize As Integer = 0

    Protected Marker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As System.Guid, ByVal data() As Byte)

    End Sub

    Protected Overrides Function ValidConfigurationString(ByVal configurationString As String) As Boolean

    End Function

End Class
