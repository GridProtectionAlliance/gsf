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
Imports TVA.Common
Imports TVA.Collections.Common
Imports TVA.Measurements
Imports TVA.DatAWare
Imports TVA.Text.Common
Imports TVA.Communication

Public Class Version3Adapter

    Inherits HistorianAdapterBase

    Private m_archiverIP As String
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
        m_archiverPort = 5000
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

        m_bufferSize = StandardEvent.BinaryLength * m_maximumEvents
        m_buffer = CreateArray(Of Byte)(m_bufferSize)

    End Sub

    Protected Overrides Sub AttemptConnection()

        ' Connect to DatAWare archiver using TCP
        m_connection = New TcpClient("server=" & m_archiverIP & "; port=" & m_archiverPort)
        m_connection.Handshake = True
        m_connection.HandshakePassphrase = "DatAWareArchiver"
        m_connection.PayloadAware = True
        m_connection.MaximumConnectionAttempts = 1

        m_connectionException = Nothing
        m_connection.Connect()

        ' Wait until connection is finished (by success or failure)...
        m_waitHandle.WaitOne()

        ' If there was a connection exception, re-throw to restart connect cycle
        If m_connectionException IsNot Nothing Then Throw m_connectionException

    End Sub

    Protected Overrides Sub AttemptDisconnection()

        If m_connection IsNot Nothing Then If m_connection.IsConnected Then m_connection.Disconnect()

    End Sub

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "DW Archiver V3 """ & m_archiverIP & ":" & m_archiverPort & """"
        End Get
    End Property

    Protected Overrides Sub ArchiveMeasurements()

        Dim x, totalPoints As Integer

        ' Retrieve data points to be archived
        SyncLock Measurements
            With Measurements
                totalPoints = Minimum(m_maximumEvents, .Count)

                If totalPoints > 0 Then
                    ' Load binary standard event images into local buffer
                    For x = 0 To totalPoints - 1
                        With New StandardEvent(.Item(x))
                            System.Buffer.BlockCopy(.BinaryImage, 0, m_buffer, x * StandardEvent.BinaryLength, StandardEvent.BinaryLength)
                        End With
                    Next

                    ' Remove measurements being processed
                    .RemoveRange(0, totalPoints)
                End If
            End With
        End SyncLock

        If totalPoints > 0 Then
            ' Post data to TCP stream
            m_connection.Send(m_buffer, 0, totalPoints * StandardEvent.BinaryLength)
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

        Connect()

    End Sub

End Class
