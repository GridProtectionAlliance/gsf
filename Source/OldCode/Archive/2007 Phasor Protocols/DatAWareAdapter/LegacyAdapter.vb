'*******************************************************************************************************
'  LegacyAdapter.vb - DatAWare legacy system adpater
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
Imports System.Net.Sockets
Imports TVA.Common
Imports TVA.Collections.Common
Imports TVA.Measurements
Imports TVA.Text.Common
Imports DatAWare.Packets

Public Class LegacyAdapter

    Inherits HistorianAdapterBase

    Private m_archiverIP As String
    Private m_archiverPort As Integer
    Private m_maximumEvents As Integer
    Private m_useTimeout As Boolean
    Private m_tcpSocket As TcpClient
    Private m_clientStream As NetworkStream
    Private m_bufferSize As Integer
    Private m_buffer As Byte()

    Public Sub New()

        MyBase.New()

        m_archiverIP = "127.0.0.1"
        m_archiverPort = 1002
        m_maximumEvents = 50
        m_useTimeout = True

    End Sub

    Public Overrides Sub Initialize(ByVal connectionString As String)

        Dim value As String

        ' Example connection string:
        ' IP=localhost; Port=1002; MaxEvents=50; UseTimeout=True
        With ParseKeyValuePairs(connectionString)
            If .TryGetValue("ip", value) Then m_archiverIP = value
            If .TryGetValue("port", value) Then m_archiverPort = Convert.ToInt32(value)
            If .TryGetValue("maxevents", value) Then m_maximumEvents = Convert.ToInt32(value)
            If .TryGetValue("usetimeout", value) Then m_useTimeout = ParseBoolean(value)
        End With

        If String.IsNullOrEmpty(m_archiverIP) Then Throw New InvalidOperationException("Cannot start TCP stream listener connection to DatAWare Archiver without specifing a host IP")

        m_bufferSize = PacketType1.Size * m_maximumEvents
        m_buffer = CreateArray(Of Byte)(m_bufferSize)

    End Sub

    Protected Overrides Sub AttemptConnection()

        ' Connect to DatAWare archiver using TCP
        m_tcpSocket = New TcpClient
        m_tcpSocket.Connect(m_archiverIP, m_archiverPort)

        m_clientStream = m_tcpSocket.GetStream()
        m_clientStream.WriteTimeout = 2000
        m_clientStream.ReadTimeout = 2000

    End Sub

    Protected Overrides Sub AttemptDisconnection()

        If m_clientStream IsNot Nothing Then m_clientStream.Close()
        m_clientStream = Nothing

        If m_tcpSocket IsNot Nothing AndAlso m_tcpSocket.Connected Then m_tcpSocket.Close()
        m_tcpSocket = Nothing

    End Sub

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "DW Legacy Archiver " & m_archiverIP & ":" & m_archiverPort
        End Get
    End Property

    Protected Overrides Sub ArchiveMeasurements(ByVal measurements As IMeasurement())

        If m_clientStream IsNot Nothing Then
            Dim measurement As IMeasurement
            Dim remainingPoints As Integer = measurements.Length
            Dim pointsToArchive, arrayIndex, bufferIndex, received, x As Integer
            Dim response As String

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
                If bufferIndex > 0 Then
                    m_clientStream.Write(m_buffer, 0, bufferIndex * PacketType1.Size)

                    ' Handle acknowledgement if enabled
                    If m_useTimeout Then
                        Try
                            ' Wait for acknowledgement (limited to readtimeout)...
                            received = m_clientStream.Read(m_buffer, 0, m_buffer.Length)

                            If received > 0 Then
                                ' Interpret response as a string
                                response = Encoding.Default.GetString(m_buffer, 0, received)

                                ' Verify archiver response
                                If Not response.StartsWith("ACK", True, Nothing) Then Throw New InvalidOperationException("DatAWare archiver failed to acknowledge packet transmission: " & response)
                            End If
                        Catch ex As IOException
                            ' This exception will get thrown if we timeout waiting for acknowlegdement from server - there's nothing to do, so we just go on...
                        Catch
                            Throw
                        End Try
                    End If
                End If
            Loop
        End If

    End Sub

End Class
