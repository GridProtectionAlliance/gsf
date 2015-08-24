'*******************************************************************************************************
'  FrameParser.vb - IEEE1344 Frame Parser
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.ComponentModel
Imports System.IO
Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor.IEEEC37_118

    ' This class parses frames and returns the appropriate data via events
    Public Class FrameParser

        'Inherits BaseFrame

        Private m_pmuID As Int16
        Private m_ipAddress As IPAddress
        Private m_ipPort As Integer
        Private m_tcpSocket As Socket
        Private m_parseThread As Thread
        Private m_phasorFormat As CoordinateFormat
        Private m_totalFrames As Long
        'Private m_headerFile As HeaderFile
        Private m_configFile1 As ConfigurationFrame
        Private m_configFile2 As ConfigurationFrame

        'Public Event ReceivedHeaderFile(ByVal headerFile As HeaderFile)
        Public Event ReceivedConfigFile1(ByVal frame As ConfigurationFrame)
        Public Event ReceivedConfigFile2(ByVal frame As ConfigurationFrame)
        Public Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Event ReceivedUnknownFrame(ByVal frame As IChannelFrame)
        Public Event DataStreamException(ByVal ex As Exception)

        Private Const BufferSize As Integer = 4096   ' 4Kb buffer

        Public Sub New()

            m_tcpSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            m_ipAddress = Dns.Resolve("127.0.0.1").AddressList(0)
            m_phasorFormat = CoordinateFormat.Rectangular

        End Sub

        Public Sub New(ByVal pmuID As Int16, ByVal pmuIPAddress As String, ByVal pmuIPPort As Integer, ByVal phasorFormat As CoordinateFormat)

            Me.New()
            m_pmuID = pmuID
            m_ipAddress = Dns.Resolve(pmuIPAddress).AddressList(0)
            m_ipPort = pmuIPPort
            m_phasorFormat = phasorFormat

        End Sub

        'Private Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        '    MyBase.New(binaryImage, startIndex)

        'End Sub

        Public Sub Connect()

            m_tcpSocket.Connect(CType(New IPEndPoint(m_ipAddress, m_ipPort), EndPoint))

            If Not m_tcpSocket.Connected Then
                Throw New InvalidOperationException("Failed to connect to PMU: " & Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()))
            End If


        End Sub

        Public Sub ReadStream()

            If Not m_parseThread Is Nothing Then Throw New InvalidOperationException("Real-time data stream is already enabled")

            m_parseThread = New Thread(AddressOf ReadDataStream)
            m_parseThread.Start()

        End Sub

        Public Sub CloseStream()

            If Not m_parseThread Is Nothing Then m_parseThread.Abort()
            m_parseThread = Nothing

        End Sub

        Public Sub Disconnect()

            m_tcpSocket.Close()

        End Sub

        Public Property PmuID() As Int16
            Get
                Return m_pmuID
            End Get
            Set(ByVal Value As Int16)
                m_pmuID = Value
            End Set
        End Property

        Public Property PmuIPAddress() As IPAddress
            Get
                Return m_ipAddress
            End Get
            Set(ByVal Value As IPAddress)
                m_ipAddress = Value
            End Set
        End Property

        Public Property PmuIPPort() As Integer
            Get
                Return m_ipPort
            End Get
            Set(ByVal Value As Integer)
                m_ipPort = Value
            End Set
        End Property

        Public ReadOnly Property TcpSocket() As Socket
            Get
                Return m_tcpSocket
            End Get
        End Property

        Public Property CoordinateFormat() As CoordinateFormat
            Get
                Return m_phasorFormat
            End Get
            Set(ByVal Value As CoordinateFormat)
                m_phasorFormat = Value
            End Set
        End Property

        Public ReadOnly Property TotalFramesReceived() As Long
            Get
                Return m_totalFrames
            End Get
        End Property

        Public Sub EnableRealTimeData()

            If Not m_parseThread Is Nothing Then Throw New InvalidOperationException("Real-time data stream is already enabled")

            ' TODO: Just doing this so we can manually watch for config file 2...
            'SendCommand(Command.DisableRealTimeData)

            'm_parseThread = New Thread(AddressOf ReadDataStream)
            'm_parseThread.Start()

            'If m_configFile2 Is Nothing Then RetrieveConfigFile2()

            'SendCommand(Command.DisableRealTimeData)
            SendCommand(Command.SendConfigFile2)
            ReadDataStream()

            'SendCommand(Command.EnableRealTimeData)
            'ReadDataStream()

        End Sub

        Public Sub DisableRealTimeData()

            If Not m_parseThread Is Nothing Then
                m_parseThread.Abort()
                m_parseThread = Nothing
            End If

            SendCommand(Command.DisableRealTimeData)

        End Sub

        Public Sub RetrieveHeaderFile()

            SendCommand(Command.SendHeaderFile)

        End Sub

        Public Sub RetrieveConfigFile1()

            SendCommand(Command.SendConfigFile1)

        End Sub

        Public Sub RetrieveConfigFile2()

            SendCommand(Command.SendConfigFile2)

        End Sub

        Public Sub SendCommand(ByVal command As Command)

            Dim cmdFrame As New CommandFrame(PmuID, command)

            If m_tcpSocket.Send(cmdFrame.BinaryImage) <> cmdFrame.FrameLength Then
                Throw New InvalidOperationException("Failed to send proper number of bytes for command frame")
            End If

        End Sub

        Private Sub ReadDataStream()

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4096)
            Dim received, startIndex As Integer
            Dim parsedFrameHeader As FrameHeader
            Dim dataStream As MemoryStream

            'Try
            Do While True
                ' Blocks until a message returns on this socket from a remote host
                received = m_tcpSocket.Receive(buffer)
                m_totalFrames += 1
                startIndex = 0

                If Not dataStream Is Nothing Then
                    dataStream.Write(buffer, 0, received)
                    buffer = dataStream.ToArray()
                    dataStream = Nothing
                End If

                Do Until startIndex >= received
                    If buffer(startIndex) <> Common.SyncByte Then
                        Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in received frame, got " & buffer(startIndex).ToString("x"c).PadLeft(2, "0"c))
                    End If

                    If startIndex + FrameHeader.BinaryLength > received Then
                        ' Save off remaining buffer to prepend onto next read
                        dataStream = New MemoryStream
                        dataStream.Write(buffer, startIndex, received - startIndex + 1)
                        Exit Do
                    End If

                    parsedFrameHeader = New FrameHeader(buffer, startIndex)

                    If startIndex + parsedFrameHeader.FrameLength > received Then
                        ' Save off remaining buffer to prepend onto next read
                        dataStream = New MemoryStream
                        dataStream.Write(buffer, startIndex, received - startIndex + 1)
                        Exit Do
                    End If

                    Select Case parsedFrameHeader.FrameType
                        Case FrameType.DataFrame
                            ' We can only start parsing data frames once we have successfully received configuration file 2...
                            If Not m_configFile2 Is Nothing Then
                                With New DataFrame(parsedFrameHeader, m_configFile2, buffer, startIndex)
                                    RaiseEvent ReceivedDataFrame(.This)
                                End With
                            End If
                            'Case FrameType.HeaderFrame
                            '    With New HeaderFrame(parsedImage, buffer, startIndex + CommonBinaryLength)
                            '        If .IsFirstFrame Then m_headerFile = New HeaderFile

                            '        m_headerFile.AppendNextFrame(.This)

                            '        If .IsLastFrame Then
                            '            RaiseEvent ReceivedHeaderFile(m_headerFile)
                            '            m_headerFile = Nothing
                            '        End If

                            '        startIndex += .FrameLength
                            '    End With
                        Case FrameType.ConfigurationFrame1
                            With New ConfigurationFrame(parsedFrameHeader, buffer, startIndex)
                                m_configFile1 = .This
                                RaiseEvent ReceivedConfigFile1(.This)
                            End With
                        Case FrameType.ConfigurationFrame2
                            With New ConfigurationFrame(parsedFrameHeader, buffer, startIndex)
                                m_configFile2 = .This
                                RaiseEvent ReceivedConfigFile2(.This)
                            End With
                        Case Else
                            'RaiseEvent ReceivedUnknownFrame(parsedImage)
                    End Select

                    startIndex += parsedFrameHeader.FrameLength
                Loop
            Loop
            'Catch ex As ThreadAbortException
            '    Exit Sub
            'Catch ex As Exception
            '    RaiseEvent DataStreamException(ex)
            'Finally
            '    'dataStream.Close()
            'End Try

        End Sub

    End Class

End Namespace