'*******************************************************************************************************
'  ListeningAdapter.vb - DatAWare.NET historian listening adpater
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/01/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports InterfaceAdapters
Imports System.Text
Imports TVA
Imports TVA.Measurements
Imports TVA.Text.Common
Imports TVA.Communication
Imports DatAWare.Packets

Public Class ListeningAdapter

    Inherits ListeningAdapterBase

    Private m_serverID As String
    Private m_archiverPort As Integer
    Private m_connectionTimeout As Integer
    Private m_connectionException As Exception
    Private WithEvents m_client As UdpClient
    Private WithEvents m_parser As DataParser
    Private m_disposed As Boolean

    Public Sub New()

        MyBase.New()

        m_archiverPort = 2001
        m_connectionTimeout = 2000

    End Sub

    Public Overrides Sub Initialize(ByVal connectionString As String)

        Dim value As String

        ' Example connection string:
        ' Port=1003; ServerID=P3
        With ParseKeyValuePairs(connectionString)
            If .TryGetValue("port", value) Then m_archiverPort = Convert.ToInt32(value)
            If .TryGetValue("serverid", value) Then m_serverID = value.Trim().ToUpper()
            If .TryGetValue("timeout", value) Then m_connectionTimeout = Convert.ToInt32(value)
        End With

    End Sub

    Protected Overrides Sub AttemptConnection()

        ' Create new DatAWare data parser
        m_parser = New DataParser()
        m_parser.UnparsedDataReuseLimit = 1
        m_parser.Start()

        ' Connect to DatAWare archiver using TCP
        m_client = New UdpClient("localport=" & m_archiverPort)
        m_client.PayloadAware = False
        m_client.Handshake = False

        m_connectionException = Nothing

        m_client.Connect()

        ' Block calling thread until connection succeeds or fails
        If Not m_client.WaitForConnection(m_connectionTimeout) Then
            If m_connectionException Is Nothing Then
                ' Failed to connect for unknown reason - restart connection cycle
                Throw New InvalidOperationException("Failed to connect")
            End If
        End If

        ' If there was a connection exception, re-throw to restart connect cycle
        If m_connectionException IsNot Nothing Then Throw m_connectionException

    End Sub

    Protected Overrides Sub AttemptDisconnection()

        If m_client IsNot Nothing Then m_client.Dispose()
        m_client = Nothing

        If m_parser IsNot Nothing Then m_parser.Dispose()
        m_parser = Nothing

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)

        AttemptDisconnection()
        MyBase.Dispose(disposing)

    End Sub

    Protected Overrides ReadOnly Property IsConnected() As Boolean
        Get
            If m_client Is Nothing Then
                Return False
            Else
                Return m_client.IsConnected
            End If
        End Get
    End Property

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "DW Listener V3 " & m_serverID & ":" & m_archiverPort
        End Get
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("    DatAWare parser active: ")
                If m_parser Is Nothing Then
                    .Append("Inactive - not parsing")
                Else
                    .Append("Active")
                End If
                .AppendLine()
                If m_client IsNot Nothing Then .Append(m_client.Status)
                .Append(MyBase.Status)
                Return .ToString()
            End With

            Return MyBase.Status
        End Get
    End Property

    Private Sub m_connection_ConnectingException(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of Exception)) Handles m_client.ConnectingException

        ' Take note of connection exception
        m_connectionException = e.Argument

    End Sub

    Private Sub m_client_ReceivedData(ByVal sender As Object, ByVal e As GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))) Handles m_client.ReceivedData

        m_parser.DataQueue.Add(e.Argument)

    End Sub

    Private Sub m_parser_DataParsed(ByVal sender As Object, ByVal e As GenericEventArgs(Of IdentifiableItem(Of Guid, List(Of PacketBase)))) Handles m_parser.DataParsed

        Dim measurements As New List(Of IMeasurement)
        Dim packet As PacketType1

        ' We have new DatAWare packets, convert all PacketType1's to measurements
        For x As Integer = 0 To e.Argument.Item.Count - 1
            packet = TryCast(e.Argument.Item(x), PacketType1)
            If packet IsNot Nothing Then measurements.Add(New Measurement(packet.PointID, m_serverID, packet.Value, packet.TimeTag.ToDateTime()))
        Next

        ' Publish new measurements to consumer...
        PublishNewEvents(measurements)

    End Sub

End Class
