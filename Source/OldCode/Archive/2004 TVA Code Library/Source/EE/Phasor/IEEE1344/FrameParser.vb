'*******************************************************************************************************
'  FrameParser.vb - IEEE1344 Frame Parser
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
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

Namespace EE.Phasor.IEEE1344

    ' This class parses frames and returns the appropriate data via events
    Public Class FrameParser

        Inherits BaseFrame

        Private m_pmuID As Int64
        Private m_ipAddress As IPAddress
        Private m_ipPort As Integer
        Private m_tcpSocket As Socket
        Private m_parseThread As Thread
        Private m_phasorFormat As CoordinateFormat
        Private m_totalFrames As Long
        Private m_configuration As ConfigurationFile
        Private m_headerFile As HeaderFile
        Private m_configFile1 As ConfigurationFile
        Private m_configFile2 As ConfigurationFile
        Private m_awaitingConfigFile1 As Boolean
        Private m_awaitingConfigFile2 As Boolean
        Private WithEvents m_eventInstance As FrameParser

        Public Event ReceivedHeaderFile(ByVal headerFile As HeaderFile)
        Public Event ReceivedConfigFile1(ByVal configFile As ConfigurationFile)
        Public Event ReceivedConfigFile2(ByVal configFile As ConfigurationFile)
        Public Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Event ReceivedUnknownFrame(ByVal frame As BaseFrame)
        Public Event DataStreamException(ByVal ex As Exception)

        Public Sub New()

            m_eventInstance = Me
            m_tcpSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            m_ipAddress = Dns.Resolve("127.0.0.1").AddressList(0)
            m_phasorFormat = CoordinateFormat.Rectangular

        End Sub

        Public Sub New(ByVal pmuID As Int64, ByVal pmuIPAddress As String, ByVal pmuIPPort As Integer, ByVal phasorFormat As CoordinateFormat)

            Me.New()
            m_pmuID = pmuID
            m_ipAddress = Dns.Resolve(pmuIPAddress).AddressList(0)
            m_ipPort = pmuIPPort
            m_phasorFormat = phasorFormat

        End Sub

        Private Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(binaryImage, startIndex)

        End Sub

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

        Public Property PmuID() As Int64
            Get
                Return m_pmuID
            End Get
            Set(ByVal Value As Int64)
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

            If m_configuration Is Nothing Then RetrieveConfigFile2()

            m_parseThread = New Thread(AddressOf ReadDataStream)
            m_parseThread.Start()

            SendCommand(PMUCommand.EnableRealTimeData)

        End Sub

        Public Sub DisableRealTimeData()

            If Not m_parseThread Is Nothing Then
                m_parseThread.Abort()
                m_parseThread = Nothing
            End If

            SendCommand(PMUCommand.DisableRealTimeData)

        End Sub

        Public Sub RetrieveHeaderFile()

            SendCommand(PMUCommand.SendHeaderFile)

        End Sub

        Public Sub RetrieveConfigFile1()

            If m_awaitingConfigFile2 Then Throw New InvalidOperationException("Cannot receive config file 1 while we are awaiting config file 2")

            m_awaitingConfigFile1 = True
            SendCommand(PMUCommand.SendConfigFile1)

        End Sub

        Public Sub RetrieveConfigFile2()

            If m_awaitingConfigFile1 Then Throw New InvalidOperationException("Cannot receive config file 2 while we are awaiting config file 1")

            m_awaitingConfigFile2 = True
            SendCommand(PMUCommand.SendConfigFile2)

        End Sub

        Public Sub AbortRetrieveConfigFile1()

            m_awaitingConfigFile1 = False

        End Sub

        Public Sub AbortRetrieveConfigFile2()

            m_awaitingConfigFile2 = False

        End Sub

        Public Sub SendReferencePhasor(ByVal referencePhasor As DataFrame)

            SendCommand(PMUCommand.ReceiveReferencePhasor)

            If m_tcpSocket.Send(referencePhasor.BinaryImage) <> referencePhasor.FrameLength Then
                Throw New InvalidOperationException("Failed to send proper number of bytes for reference phasor")
            End If

        End Sub

        Public Sub SendCommand(ByVal command As PMUCommand)

            Dim cmdFrame As New CommandFrame(PmuID, command)

            If m_tcpSocket.Send(cmdFrame.BinaryImage) <> cmdFrame.FrameLength Then
                Throw New InvalidOperationException("Failed to send proper number of bytes for command frame")
            End If

        End Sub

        Public FileName As String


        Private Sub ReadDataStream()

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4096)
            Dim received, startIndex As Integer
            Dim parsedImage As FrameParser

            Dim dataStream As FileStream

            Try

                dataStream = File.OpenWrite(FileName)

                Do While True
                    ' Blocks until a message returns on this socket from a remote host
                    received = TcpSocket.Receive(buffer)

                    dataStream.Write(BitConverter.GetBytes(received), 0, 4)
                    dataStream.Write(buffer, 0, received)

                    m_totalFrames += 1
                Loop

                'startIndex = 0

                'Do Until startIndex >= received
                '    parsedImage = New FrameParser(buffer, startIndex)

                '    Select Case parsedImage.FrameType
                '        Case PMUFrameType.DataFrame
                '            With New DataFrame(parsedImage, m_configuration, buffer, startIndex + CommonBinaryLength, m_phasorFormat)
                '                ' We can only start parsing data frames once we have successfully received configuration file 2...
                '                If Not m_configuration Is Nothing Then
                '                    RaiseEvent ReceivedDataFrame(.This)
                '                End If

                '                startIndex += .FrameLength
                '            End With
                '        Case PMUFrameType.HeaderFrame
                '            With New HeaderFrame(parsedImage, buffer, startIndex + CommonBinaryLength)
                '                If .IsFirstFrame Then m_headerFile = New HeaderFile

                '                m_headerFile.AppendNextFrame(.This)

                '                If .IsLastFrame Then
                '                    RaiseEvent ReceivedHeaderFile(m_headerFile)
                '                    m_headerFile = Nothing
                '                End If

                '                startIndex += .FrameLength
                '            End With
                '        Case PMUFrameType.ConfigurationFrame
                '            With New ConfigurationFrame(parsedImage, buffer, startIndex + CommonBinaryLength)
                '                If m_awaitingConfigFile1 Then
                '                    If .IsFirstFrame Then m_configFile1 = New ConfigurationFile

                '                    m_configFile1.AppendNextFrame(.This)

                '                    If .IsLastFrame Then
                '                        m_awaitingConfigFile1 = False
                '                        RaiseEvent ReceivedConfigFile1(m_configFile1)
                '                        m_configFile1 = Nothing
                '                    End If
                '                ElseIf m_awaitingConfigFile2 Then
                '                    If .IsFirstFrame Then m_configFile2 = New ConfigurationFile

                '                    m_configFile2.AppendNextFrame(.This)

                '                    If .IsLastFrame Then
                '                        m_awaitingConfigFile2 = False
                '                        RaiseEvent ReceivedConfigFile2(m_configFile2)
                '                        m_configFile2 = Nothing
                '                    End If
                '                Else
                '                    RaiseEvent ReceivedUnknownFrame(.This)
                '                End If

                '                startIndex += .FrameLength
                '            End With
                '        Case Else
                '            RaiseEvent ReceivedUnknownFrame(parsedImage)
                '    End Select
                'Loop
            Catch ex As ThreadAbortException
                Exit Sub
            Catch ex As Exception
                RaiseEvent DataStreamException(ex)
            Finally
                dataStream.Close()
            End Try

        End Sub

        ' We pick up our config file 2 when we enable the real time stream...
        Private Sub m_eventInstance_ReceivedConfigFile2(ByVal configFile As ConfigurationFile) Handles m_eventInstance.ReceivedConfigFile2

            If m_configuration Is Nothing Then m_configuration = configFile

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Property DataImage() As Byte()
            Get
                Throw New NotImplementedException
            End Get
            Set(ByVal Value As Byte())
                Throw New NotImplementedException
            End Set
        End Property

    End Class

End Namespace