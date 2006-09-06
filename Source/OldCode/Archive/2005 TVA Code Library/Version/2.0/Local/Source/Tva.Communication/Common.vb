' 08-04-06

Imports Tva.Text.Common

Public Class Common

    Delegate Sub ReceiveRawDataFunctionSignature(ByVal data As Byte(), ByVal offset As Integer, ByVal length As Integer)

    Public Shared Function CreateCommunicationServer(ByVal configurationString As String) As ICommunicationServer

        Dim server As ICommunicationServer = Nothing
        Dim protocol As String = ""
        Dim configurationData As Dictionary(Of String, String) = ParseKeyValuePairs(configurationString)

        If configurationData.TryGetValue("protocol", protocol) Then
            configurationData.Remove("protocol")
            With New System.Text.StringBuilder()
                For Each key As String In configurationData.Keys
                    .Append(key)
                    .Append("=")
                    .Append(configurationData(key))
                    .Append(";")
                Next
                Select Case protocol.ToLower()
                    Case "tcp"
                        server = New TcpServer(.ToString())
                    Case "udp"
                        server = New UdpServer(.ToString())
                    Case Else
                        Throw New ArgumentException("Transport protocol '" & protocol & "' is not valid.")
                End Select
            End With
        Else
            Throw New ArgumentException("Transport protocol must be specified.")
        End If

        Return server

    End Function

    Public Shared Function CreateCommunicationClient(ByVal connectionString As String) As ICommunicationClient

        Dim client As ICommunicationClient = Nothing
        Dim protocol As String = ""
        Dim connectionData As Dictionary(Of String, String) = ParseKeyValuePairs(connectionString)

        If connectionData.TryGetValue("protocol", protocol) Then
            connectionData.Remove("protocol")
            With New System.Text.StringBuilder()
                For Each key As String In connectionData.Keys
                    .Append(key)
                    .Append("=")
                    .Append(connectionData(key))
                    .Append(";")
                Next
                Select Case protocol.ToLower()
                    Case "tcp"
                        client = New TcpClient(.ToString())
                    Case "udp"
                        client = New UdpClient(.ToString())
                    Case "serial"
                        client = New SerialClient(.ToString())
                    Case "file"
                        client = New FileClient(.ToString())
                    Case Else
                        Throw New ArgumentException("Transport protocol '" & protocol & "' is not valid.")
                End Select
            End With
        Else
            Throw New ArgumentException("Transport protocol must be specified.")
        End If

        Return client

    End Function

End Class
