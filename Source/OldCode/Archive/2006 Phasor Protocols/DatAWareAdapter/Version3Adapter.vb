'*******************************************************************************************************
'  LegacyAdapter.vb - DatAWare legacy system adpater
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
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
    Private m_bufferSize As Integer
    Private m_buffer As Byte()
    Private m_waitHandle As AutoResetEvent
    Private m_connectionException As Exception
    Private WithEvents m_connection As TcpClient

    Public Sub New()

        MyBase.New()

        m_archiverIP = "127.0.0.1"
        m_archiverPort = 1003
        m_maximumEvents = 100000
        m_waitHandle = New AutoResetEvent(False)

    End Sub

    Public Overrides Sub Initialize(ByVal connectionString As String)

        Dim value As String

        ' Example connection string:
        ' IP=localhost; Port=1002; MaxEvents=50; UseTimeout=True
        With ParseKeyValuePairs(connectionString)
            If .TryGetValue("ip", value) Then m_archiverIP = value
            If .TryGetValue("port", value) Then m_archiverPort = Convert.ToInt32(value)
            If .TryGetValue("maxevents", value) Then m_maximumEvents = Convert.ToInt32(value)
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
        m_connection.PayloadAware = True
        m_connection.MaximumConnectionAttempts = 1
        m_connection.Handshake = True
        m_connection.HandshakePassphrase = "DatAWareArchiver"

        m_connectionException = Nothing
        m_connection.Connect()

        ' Wait until connection is finished (by success or failure)...
        m_waitHandle.WaitOne()

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

        If m_connection IsNot Nothing Then
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

    Private Sub m_connection_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_connection.Connected

        ' Signal completion of connection cycle
        m_waitHandle.Set()

    End Sub

    Private Sub m_connection_ConnectingCancelled(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_connection.ConnectingCancelled

        ' Signal completion of connection cycle
        m_waitHandle.Set()

    End Sub

    Private Sub m_connection_ConnectingException(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of Exception)) Handles m_connection.ConnectingException

        ' Take note of connection exception
        m_connectionException = e.Argument

        ' Signal completion of connection cycle
        m_waitHandle.Set()

    End Sub

    Private Sub m_connection_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_connection.Disconnected

        ' Signal completion of connection cycle
        m_waitHandle.Set()

        Connect()

    End Sub

End Class
