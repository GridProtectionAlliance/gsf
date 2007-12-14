'*******************************************************************************************************
'  Version3Adapter.vb - DatAWare.NET historian adpater
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
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Net
Imports TVA.Common
Imports TVA.Collections.Common
Imports TVA.Measurements
Imports TVA.Text.Common
Imports TVA.Communication
Imports DatAWare.Packets

Public Class Version3Adapter

    Inherits HistorianAdapterBase

    Private m_archiverIP As String
    Private m_archiverHostName As String
    Private m_archiverPort As Integer
    Private m_maximumEvents As Integer
    Private m_connectionTimeout As Integer
    Private m_bufferSize As Integer
    Private m_buffer As Byte()
    Private m_connectionException As Exception
    Private WithEvents m_connection As TcpClient

    Public Sub New()

        MyBase.New()

        m_archiverIP = "127.0.0.1"
        m_archiverPort = 1003
        m_maximumEvents = 100000
        m_connectionTimeout = 2000

    End Sub

    Public Overrides Sub Initialize(ByVal connectionString As String)

        Dim value As String

        ' Example connection string:
        ' IP=localhost; Port=1003; MaxEvents=100000
        With ParseKeyValuePairs(connectionString)
            If .TryGetValue("ip", value) Then m_archiverIP = value
            If .TryGetValue("port", value) Then m_archiverPort = Convert.ToInt32(value)
            If .TryGetValue("maxevents", value) Then m_maximumEvents = Convert.ToInt32(value)
            If .TryGetValue("timeout", value) Then m_connectionTimeout = Convert.ToInt32(value)
        End With

        If String.IsNullOrEmpty(m_archiverIP) Then Throw New InvalidOperationException("Cannot start TCP stream listener connection to DatAWare Archiver without specifing a host IP")

        m_bufferSize = PacketType1.Size * m_maximumEvents
        m_buffer = CreateArray(Of Byte)(m_bufferSize)

        ' Attempt to lookup DNS host name for given IP
        Try
            m_archiverHostName = Dns.GetHostEntry(m_archiverIP).HostName
            If String.IsNullOrEmpty(m_archiverHostName) Then m_archiverHostName = m_archiverIP
        Catch
            m_archiverHostName = m_archiverIP
        End Try

    End Sub

    Protected Overrides Sub AttemptConnection()

        ' Connect to DatAWare archiver using TCP
        m_connection = New TcpClient("server=" & m_archiverIP & "; port=" & m_archiverPort)
        m_connection.MaximumConnectionAttempts = 1
        m_connection.PayloadAware = True
        m_connection.Handshake = False

        m_connectionException = Nothing

        m_connection.Connect()

        ' Block calling thread until connection succeeds or fails
        If Not m_connection.WaitForConnection(m_connectionTimeout) Then
            If m_connectionException Is Nothing Then
                ' Failed to connect for unknown reason - restart connection cycle
                Throw New InvalidOperationException("Failed to connect")
            End If
        End If

        ' If there was a connection exception, re-throw to restart connect cycle
        If m_connectionException IsNot Nothing Then Throw m_connectionException

    End Sub

    Protected Overrides Sub AttemptDisconnection()

        If m_connection IsNot Nothing Then If m_connection.IsConnected Then m_connection.Disconnect()
        m_connection = Nothing

    End Sub

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "DW Archiver V3 " & m_archiverHostName & ":" & m_archiverPort
        End Get
    End Property

    Protected Overrides Sub ArchiveMeasurements(ByVal measurements As IMeasurement())

        If m_connection IsNot Nothing AndAlso m_connection.IsConnected Then
            Dim measurement As IMeasurement
            Dim remainingPoints As Integer = measurements.Length
            Dim pointsToArchive, arrayIndex, bufferIndex, x As Integer

            Do While remainingPoints > 0
                pointsToArchive = Minimum(m_maximumEvents, remainingPoints)
                remainingPoints -= pointsToArchive

                ' Load binary standard event images into local buffer
                bufferIndex = 0

                For x = arrayIndex To arrayIndex + pointsToArchive - 1
                    measurement = measurements(x)
                    If measurement.Ticks > 0 Then
                        Buffer.BlockCopy((New PacketType1(measurement)).BinaryImage, 0, m_buffer, bufferIndex * PacketType1.Size, PacketType1.Size)
                        bufferIndex += 1
                    End If
                Next

                arrayIndex += pointsToArchive

                ' Post data to TCP stream
                If bufferIndex > 0 Then m_connection.Send(m_buffer, 0, bufferIndex * PacketType1.Size)
            Loop
        End If

    End Sub

    Private Sub m_connection_ConnectingCancelled(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_connection.ConnectingCancelled

        ' Signal time-out of connection attempt
        m_connectionException = New InvalidOperationException("Timed-out waiting for connection")

    End Sub

    Private Sub m_connection_ConnectingException(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of Exception)) Handles m_connection.ConnectingException

        ' Take note of connection exception
        m_connectionException = e.Argument

    End Sub

    Private Sub m_connection_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_connection.Disconnected

        ' Make sure connection cycle gets restarted when we get disconnected from archiver...
        Connect()

    End Sub

End Class
