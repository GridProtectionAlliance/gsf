' 07-06-06

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports Tva.Communication.SocketHelper

Public Class UdpServer

    Private m_udpServer As Socket
    Private m_udpClients As Dictionary(Of Guid, IPEndPoint)
    Private m_pendingUdpClients As List(Of IPEndPoint)
    Private m_configurationData As Dictionary(Of String, String)
    Private m_packetMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    Private Const PacketSize As Integer = 65536

    Public Sub New(ByVal configurationString As String)
        MyClass.New()
        MyBase.ConfigurationString = configurationString
    End Sub

    Public Overrides Sub Start()

        If Enabled() AndAlso Not IsRunning() AndAlso ValidConfigurationString(ConfigurationString()) Then
            m_udpServer = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            m_udpServer.Bind(New IPEndPoint(IPAddress.Any, Convert.ToInt32(m_configurationData("port"))))

            For Each clientIP As String In m_configurationData("clients").Split(","c)
                Dim udpClient As IPEndPoint = GetIpEndPoint(clientIP.Trim(), Convert.ToInt32(m_configurationData("port")))
                If Not Handshake() OrElse udpClient.Address.Equals(IPAddress.Broadcast) Then
                    Dim udpClientID As Guid = Guid.NewGuid()
                    m_udpClients.Add(udpClientID, udpClient)
                    OnClientConnected(udpClientID)
                Else
                    m_pendingUdpClients.Add(udpClient)
                End If
            Next

            If Handshake() AndAlso m_pendingUdpClients.Count() > 0 Then
                ' Listen for handshake messages from the listed UDP clients.
            End If

            OnServerStarted(EventArgs.Empty)
        End If

    End Sub

    Public Overrides Sub [Stop]()

        If Enabled() AndAlso IsRunning() Then
            If m_udpServer IsNot Nothing Then m_udpServer.Close()
            OnServerStopped(EventArgs.Empty)

            For Each udpClientID As Guid In m_udpClients.Keys()
                OnClientDisconnected(udpClientID)
            Next
            m_udpClients.Clear()

            m_pendingUdpClients.Clear()
        End If

    End Sub

    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As System.Guid, ByVal data() As Byte)

        If Enabled() AndAlso IsRunning() Then
            Dim udpClient As IPEndPoint = Nothing
            If m_udpClients.TryGetValue(clientID, udpClient) Then
                m_udpServer.BeginSendTo(data, 0, data.Length(), SocketFlags.None, udpClient, Nothing, Nothing)
            Else
                Throw New ArgumentException("Client ID '" & clientID.ToString() & "' is invalid.")
            End If
        End If

    End Sub

    Protected Overrides Function ValidConfigurationString(ByVal configurationString As String) As Boolean

        If Not String.IsNullOrEmpty(configurationString) Then
            m_configurationData = Tva.Text.Common.ParseKeyValuePairs(configurationString)
            If m_configurationData.ContainsKey("port") AndAlso _
                    ValidPortNumber(Convert.ToString(m_configurationData("port"))) AndAlso _
                    m_configurationData.ContainsKey("clients") AndAlso _
                    m_configurationData("clients").Split(","c).Length() > 0 Then
                Return True
            Else
                ' Configuration string is not in the expected format.
                With New StringBuilder()
                    .Append("Configuration string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Port=<Port Number>; Clients=<Client name or IP, ..., Client name or IP>")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

End Class
