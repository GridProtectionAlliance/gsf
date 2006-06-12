' 06-01-06

Imports System.Net

Namespace Data.Transport

    Public Enum TransportProtocol As Integer
        Tcp
        Udp
        Serial
        File
    End Enum

    Friend NotInheritable Class Common

        ''' <summary>
        ''' Gets an IP endpoint for the specified host name and port number.
        ''' </summary>
        ''' <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        ''' <param name="port">The port number to be associated with the address.</param>
        ''' <returns>IP endpoint for the specified host name and port number.</returns>
        Friend Shared Function GetIpEndPoint(ByVal hostNameOrAddress As String, ByVal port As Integer) As IPEndPoint

            ' SocketException will be thrown is the host is not found.
            Return New IPEndPoint(Dns.GetHostEntry(hostNameOrAddress).AddressList(0), port)

        End Function

        ''' <summary>
        ''' Determines whether the specified port is valid.
        ''' </summary>
        ''' <param name="port">The port number to be validated.</param>
        ''' <returns>True if the port number is valid.</returns>
        Friend Shared Function ValidPortNumber(ByVal port As String) As Boolean

            Dim portNumber As Integer
            If Integer.TryParse(port, portNumber) Then
                ' The specified port is a valid integer value.
                If portNumber >= 0 AndAlso portNumber <= 65535 Then
                    ' The port number is within the valid range.
                    Return True
                Else
                    Throw New ArgumentOutOfRangeException("Port", "Port number must be between 0 and 65535.")
                End If
            Else
                Throw New ArgumentException("Port number is not a valid number.")
            End If

        End Function

        Friend Shared Function CreateIdentificationMessage(ByVal id As Guid) As IdentificationMessage

            Dim idMessage As New IdentificationMessage()
            idMessage.ID = id
            idMessage.System = My.Computer.Name()
            idMessage.NTUser = My.User.Name()
            idMessage.Assembly = Tva.Assembly.EntryAssembly.FullName()
            idMessage.Location = Tva.Assembly.EntryAssembly.Location()
            idMessage.Created = Tva.Assembly.EntryAssembly.BuildDate()
            Return idMessage

        End Function

    End Class

End Namespace