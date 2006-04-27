'*******************************************************************************************************
'  FrameParser.vb - Protocol independent frame parser
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  03/16/2006 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.IO
Imports Tva.Collections
Imports Tva.DateTime.Common
Imports Tva.Phasors

''' <summary>Protocol independent frame parser</summary>
<CLSCompliant(False)> _
Public Class FrameParser

#Region " Public Member Declarations "

    Public Event ReceivedFrameBufferImage(ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer)
    Public Event ReceivedConfigurationFrame(ByVal frame As IConfigurationFrame)
    Public Event ReceivedDataFrame(ByVal frame As IDataFrame)
    Public Event ReceivedHeaderFrame(ByVal frame As IHeaderFrame)
    Public Event ReceivedCommandFrame(ByVal frame As ICommandFrame)
    Public Event ReceivedUndeterminedFrame(ByVal frame As IChannelFrame)
    Public Event DataStreamException(ByVal ex As Exception)

    Public Const DefaultBufferSize As Int32 = 262144    ' 256K

#End Region

#Region " Private Member Declarations "

    ' Connection properties
    Private m_protocol As Protocol
    Private m_transportLayer As DataTransportLayer
    Private m_hostIP As String
    Private m_port As Int32
    Private m_pmuID As Int32
    Private m_bufferSize As Int32

    ' We internalize protocol specfic processing to simplfy end user consumption
    Private WithEvents m_ieeeC37_118FrameParser As IeeeC37_118.FrameParser
    Private WithEvents m_ieee1344FrameParser As Ieee1344.FrameParser
    Private WithEvents m_bpaPdcStreamFrameParser As BpaPdcStream.FrameParser
    Private WithEvents m_rateCalcTimer As Timers.Timer

    Private m_socketThread As Thread
    Private m_tcpSocket As TcpClient
    Private m_udpSocket As Socket
    Private m_receptionPoint As EndPoint
    Private m_clientStream As NetworkStream
    Private m_configFrame As IConfigurationFrame
    Private m_dataStreamStartTime As Long
    Private m_totalFramesReceived As Long
    Private m_totalBytesReceived As Long
    Private m_frameRateTotal As Int32
    Private m_byteRateTotal As Int32
    Private m_frameRate As Double
    Private m_byteRate As Double

#End Region

#Region " Construction Functions "

    Public Sub New()

        m_bufferSize = DefaultBufferSize
        m_rateCalcTimer = New Timers.Timer

        With m_rateCalcTimer
            .Interval = 1000
            .AutoReset = True
            .Enabled = False
        End With

        m_protocol = Phasors.Protocol.IeeeC37_118V1
        m_transportLayer = DataTransportLayer.Tcp

    End Sub

    Public Sub New(ByVal protocol As Protocol, ByVal transportLayer As DataTransportLayer)

        MyClass.New()
        m_protocol = protocol
        m_transportLayer = transportLayer

    End Sub

#End Region

#Region " Public Methods Implementation "

    Public Property Protocol() As Protocol
        Get
            Return m_protocol
        End Get
        Set(ByVal value As Protocol)
            m_protocol = value
        End Set
    End Property

    Property TransportLayer() As DataTransportLayer
        Get
            Return m_transportLayer
        End Get
        Set(ByVal value As DataTransportLayer)
            m_transportLayer = value
        End Set
    End Property

    Public Property HostIP() As String
        Get
            Return m_hostIP
        End Get
        Set(ByVal value As String)
            m_hostIP = value
        End Set
    End Property

    Public Property Port() As Int32
        Get
            Return m_port
        End Get
        Set(ByVal value As Int32)
            m_port = value
        End Set
    End Property

    Public Property PmuID() As Int32
        Get
            Return m_pmuID
        End Get
        Set(ByVal value As Int32)
            m_pmuID = value
        End Set
    End Property

    Public Property BufferSize() As Int32
        Get
            Return m_bufferSize
        End Get
        Set(ByVal value As Int32)
            m_bufferSize = value
        End Set
    End Property

    Public Sub Connect()

        Disconnect()
        m_totalFramesReceived = 0
        m_totalBytesReceived = 0
        m_frameRateTotal = 0
        m_byteRateTotal = 0
        m_frameRate = 0.0#
        m_byteRate = 0.0#

        Try
            ' Instantiate protocol specific frame parser
            Select Case m_protocol
                Case Phasors.Protocol.IeeeC37_118V1
                    m_ieeeC37_118FrameParser = New IeeeC37_118.FrameParser(IeeeC37_118.RevisionNumber.RevisionV1)
                    m_ieeeC37_118FrameParser.Start()
                Case Phasors.Protocol.IeeeC37_118D6
                    m_ieeeC37_118FrameParser = New IeeeC37_118.FrameParser(IeeeC37_118.RevisionNumber.RevisionD6)
                    m_ieeeC37_118FrameParser.Start()
                Case Phasors.Protocol.Ieee1344
                    m_ieee1344FrameParser = New Ieee1344.FrameParser
                    m_ieee1344FrameParser.Start()
                Case Phasors.Protocol.BpaPdcStream
                    m_bpaPdcStreamFrameParser = New BpaPdcStream.FrameParser
                    m_bpaPdcStreamFrameParser.Start()
            End Select

            ' Start reading data from selected transport layer
            Select Case m_transportLayer
                Case DataTransportLayer.Tcp
                    ' Validate minimal connection parameters required for TCP connection
                    If String.IsNullOrEmpty(m_hostIP) Then Throw New InvalidOperationException("Cannot start TCP stream listener without specifing a host IP")
                    If m_port = 0 Then Throw New InvalidOperationException("Cannot start TCP stream listener without specifing a port")

                    ' Connect to PMU using TCP
                    m_tcpSocket = New TcpClient
                    m_tcpSocket.ReceiveBufferSize = m_bufferSize
                    m_tcpSocket.Connect(m_hostIP, m_port)
                    m_clientStream = m_tcpSocket.GetStream()

                    ' Start listening to TCP data stream
                    m_socketThread = New Thread(AddressOf ProcessTcpStream)
                    m_socketThread.Start()
                Case DataTransportLayer.Udp
                    ' Validate minimal connection parameters required for UDP connection
                    If m_port = 0 Then Throw New InvalidOperationException("Cannot start UDP stream listener without specifing a valid port")

                    ' Connect to PMU using UDP (just listening to incoming stream on specified port)
                    m_udpSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                    m_receptionPoint = CType(New IPEndPoint(IPAddress.Any, m_port), System.Net.EndPoint)
                    m_udpSocket.ReceiveBufferSize = m_bufferSize
                    m_udpSocket.Bind(m_receptionPoint)

                    ' Start listening to UDP data stream
                    m_socketThread = New Thread(AddressOf ProcessUdpStream)
                    m_socketThread.Start()
                Case DataTransportLayer.Com
                    ' TODO: Determine minimal needed connection parameters for COM link...
            End Select

            m_rateCalcTimer.Enabled = True
        Catch
            Disconnect()
            Throw
        End Try

    End Sub

    Public Sub Disconnect()

        m_rateCalcTimer.Enabled = False

        If m_socketThread IsNot Nothing Then m_socketThread.Abort()
        m_socketThread = Nothing

        If m_tcpSocket IsNot Nothing Then m_tcpSocket.Close()
        m_tcpSocket = Nothing

        If m_udpSocket IsNot Nothing Then m_udpSocket.Close()
        m_udpSocket = Nothing

        m_clientStream = Nothing
        m_receptionPoint = Nothing

        If m_ieeeC37_118FrameParser IsNot Nothing Then m_ieeeC37_118FrameParser.Stop()
        m_ieeeC37_118FrameParser = Nothing

        If m_ieee1344FrameParser IsNot Nothing Then m_ieee1344FrameParser.Stop()
        m_ieee1344FrameParser = Nothing

        If m_bpaPdcStreamFrameParser IsNot Nothing Then m_bpaPdcStreamFrameParser.Stop()
        m_bpaPdcStreamFrameParser = Nothing

        m_configFrame = Nothing

    End Sub

    Public ReadOnly Property Enabled() As Boolean
        Get
            If m_ieeeC37_118FrameParser IsNot Nothing Then
                Return m_ieeeC37_118FrameParser.Enabled
            ElseIf m_ieee1344FrameParser IsNot Nothing Then
                Return m_ieee1344FrameParser.Enabled
            ElseIf m_bpaPdcStreamFrameParser IsNot Nothing Then
                Return m_bpaPdcStreamFrameParser.Enabled
            End If
        End Get
    End Property

    Public ReadOnly Property QueuedBuffers() As Int32
        Get
            If m_ieeeC37_118FrameParser IsNot Nothing Then
                Return m_ieeeC37_118FrameParser.QueuedBuffers
            ElseIf m_ieee1344FrameParser IsNot Nothing Then
                Return m_ieee1344FrameParser.QueuedBuffers
            ElseIf m_bpaPdcStreamFrameParser IsNot Nothing Then
                Return m_bpaPdcStreamFrameParser.QueuedBuffers
            End If
        End Get
    End Property

    Public ReadOnly Property InternalFrameParser() As Object
        Get
            If m_ieeeC37_118FrameParser IsNot Nothing Then
                Return m_ieeeC37_118FrameParser
            ElseIf m_ieee1344FrameParser IsNot Nothing Then
                Return m_ieee1344FrameParser
            Else
                Return m_bpaPdcStreamFrameParser
            End If
        End Get
    End Property

    Public ReadOnly Property TotalFramesReceived() As Long
        Get
            Return m_totalFramesReceived
        End Get
    End Property

    Public ReadOnly Property TotalBytesReceived() As Long
        Get
            Return m_totalBytesReceived
        End Get
    End Property

    Public ReadOnly Property FrameRate() As Double
        Get
            Return m_frameRate
        End Get
    End Property

    Public ReadOnly Property ByteRate() As Double
        Get
            Return m_byteRate
        End Get
    End Property

    Public ReadOnly Property BitRate() As Double
        Get
            Return m_byteRate * 8
        End Get
    End Property

    Public ReadOnly Property KiloBitRate() As Double
        Get
            Return m_byteRate * 8 / 1024
        End Get
    End Property

    Public ReadOnly Property MegaBitRate() As Double
        Get
            Return m_byteRate * 8 / 1048576
        End Get
    End Property

    Public Sub SendPmuCommand(ByVal command As Command)

        If m_clientStream IsNot Nothing Then
            Dim binaryImage As Byte()
            Dim binaryLength As Int32

            ' Only the IEEE protocols support commands
            Select Case m_protocol
                Case Phasors.Protocol.IeeeC37_118V1
                    With New IeeeC37_118.CommandFrame(IeeeC37_118.RevisionNumber.RevisionV1, m_pmuID, command)
                        binaryImage = .BinaryImage
                        binaryLength = .BinaryLength
                    End With
                Case Phasors.Protocol.IeeeC37_118D6
                    With New IeeeC37_118.CommandFrame(IeeeC37_118.RevisionNumber.RevisionD6, m_pmuID, command)
                        binaryImage = .BinaryImage
                        binaryLength = .BinaryLength
                    End With
                Case Phasors.Protocol.Ieee1344
                    With New Ieee1344.CommandFrame(m_pmuID, command)
                        binaryImage = .BinaryImage
                        binaryLength = .BinaryLength
                    End With
                Case Else
                    binaryImage = Nothing
                    binaryLength = 0
            End Select

            'With System.IO.File.CreateText(IO.FilePath.GetApplicationPath & [Enum].GetName(GetType(Command), command) & "Image.txt")
            '    .WriteLine([Enum].GetName(GetType(Command), command) & " Binary Image - Created: " & Date.Now.ToString)
            '    .WriteLine("Image Length: " & binaryLength & Environment.NewLine)
            '    .WriteLine("Hexadecimal Image:")
            '    .WriteLine(ByteEncoding.Hexadecimal.GetString(binaryImage, " "c))
            '    .WriteLine("Decimal Image:")
            '    .WriteLine(ByteEncoding.Decimal.GetString(binaryImage, " "c))
            '    .WriteLine("Big-endian Binary Image:")
            '    .WriteLine(ByteEncoding.BigEndianBinary.GetString(binaryImage, " "c))
            '    .Close()
            'End With

            If binaryLength > 0 Then m_clientStream.Write(binaryImage, 0, binaryLength)
        End If

    End Sub

#End Region

#Region " Private Methods Implementation "

    Private Sub m_rateCalcTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_rateCalcTimer.Elapsed

        Dim time As Double = TicksToSeconds(Date.Now.Ticks - m_dataStreamStartTime)

        m_frameRate = m_frameRateTotal / time
        m_byteRate = m_byteRateTotal / time

        m_dataStreamStartTime = Date.Now.Ticks
        m_frameRateTotal = 0
        m_byteRateTotal = 0

    End Sub

    Private Sub ProcessFrame(ByVal frame As IConfigurationFrame)

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        m_configFrame = frame
        RaiseEvent ReceivedConfigurationFrame(frame)

    End Sub

    Private Sub ProcessFrame(ByVal frame As IDataFrame)

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedDataFrame(frame)

    End Sub

    Private Sub ProcessFrame(ByVal frame As IHeaderFrame)

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedHeaderFrame(frame)

    End Sub

    Private Sub ProcessFrame(ByVal frame As ICommandFrame)

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedCommandFrame(frame)

    End Sub

    Private Sub ProcessFrame(ByVal frame As IChannelFrame)

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedUndeterminedFrame(frame)

    End Sub

    Private Sub Write(ByVal buffer As Byte(), ByVal received As Int32)

        m_totalBytesReceived += received
        m_byteRateTotal += received

        Select Case m_protocol
            Case Phasors.Protocol.IeeeC37_118V1, Phasors.Protocol.IeeeC37_118D6
                m_ieeeC37_118FrameParser.Write(buffer, 0, received)
            Case Phasors.Protocol.Ieee1344
                m_ieee1344FrameParser.Write(buffer, 0, received)
            Case Phasors.Protocol.BpaPdcStream
                m_bpaPdcStreamFrameParser.Write(buffer, 0, received)
        End Select

    End Sub

    Private Sub ProcessUdpStream()

        Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
        Dim received As Int32

        ' Enter the data read loop
        Do While True
            Try
                ' Block thread until we've received some data...
                received = m_udpSocket.ReceiveFrom(buffer, m_receptionPoint)

                ' Provide received buffer to protocol specific frame parser
                If received > 0 Then Write(buffer, received)
            Catch ex As ThreadAbortException
                ' If we received an abort exception, we'll egress gracefully
                Exit Do
            Catch ex As IOException
                ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
                ' in this case we'll bow out gracefully as well...
                Exit Do
            Catch ex As Exception
                RaiseEvent DataStreamException(ex)
                Exit Do
            End Try
        Loop

    End Sub

    Private Sub ProcessTcpStream()

        Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
        Dim received, attempts As Integer

        ' Handle reception of configuration frame - in case of device that only responds to commands when not sending real-time data,
        ' such as the SEL 421, we disable real-time data stream first...
        Try
            ' Make sure data stream is disabled
            SendPmuCommand(Command.DisableRealTimeData)

            ' Wait for real-time data stream to cease
            Do While m_clientStream.DataAvailable
                ' Remove all existing data from stream
                Do While m_clientStream.DataAvailable
                    received = m_clientStream.Read(buffer, 0, buffer.Length)
                Loop

                Thread.Sleep(100)

                attempts += 1
                If attempts >= 50 Then Exit Do
            Loop

            ' Request configuration frame 2
            attempts = 0
            m_configFrame = Nothing
            SendPmuCommand(Command.SendConfigurationFrame2)

            Do While m_configFrame Is Nothing
                ' So long as we are receiving data, we'll push it to the frame parser
                Do While m_clientStream.DataAvailable
                    ' Block thread until we've read some data...
                    received = m_clientStream.Read(buffer, 0, buffer.Length)

                    ' Send received data to frame parser
                    If received > 0 Then Write(buffer, received)
                Loop

                ' Hang out for a little while so config frame can be parsed
                Thread.Sleep(100)

                attempts += 1
                If attempts >= 50 Then Exit Do
            Loop

            ' Enable data stream
            SendPmuCommand(Command.EnableRealTimeData)
        Catch ex As ThreadAbortException
            ' If we received an abort exception, we'll egress gracefully
            Exit Sub
        Catch ex As IOException
            ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
            ' in this case we'll bow out gracefully as well...
            Exit Sub
        Catch ex As Exception
            RaiseEvent DataStreamException(ex)
            Exit Sub
        End Try

        ' Enter the data read loop
        Do While True
            Try
                ' Block thread until we've received some data...
                received = m_clientStream.Read(buffer, 0, buffer.Length)

                ' Provide received buffer to protocol specific frame parser
                If received > 0 Then Write(buffer, received)
            Catch ex As ThreadAbortException
                ' If we received an abort exception, we'll egress gracefully
                Exit Do
            Catch ex As IOException
                ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
                ' in this case we'll bow out gracefully as well...
                Exit Do
            Catch ex As Exception
                RaiseEvent DataStreamException(ex)
                Exit Do
            End Try
        Loop

    End Sub

#Region " Protocol Specific Event Handlers "

    Private Sub m_ieeeC37_118FrameParser_DataStreamException(ByVal ex As System.Exception) Handles m_ieeeC37_118FrameParser.DataStreamException

        RaiseEvent DataStreamException(ex)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedCommandFrame(ByVal frame As Tva.Phasors.IeeeC37_118.CommandFrame) Handles m_ieeeC37_118FrameParser.ReceivedCommandFrame

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedCommonFrameHeader(ByVal frame As IeeeC37_118.ICommonFrameHeader) Handles m_ieeeC37_118FrameParser.ReceivedCommonFrameHeader

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedConfigurationFrame1(ByVal frame As Tva.Phasors.IeeeC37_118.ConfigurationFrame) Handles m_ieeeC37_118FrameParser.ReceivedConfigurationFrame1

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedConfigurationFrame2(ByVal frame As Tva.Phasors.IeeeC37_118.ConfigurationFrame) Handles m_ieeeC37_118FrameParser.ReceivedConfigurationFrame2

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedDataFrame(ByVal frame As Tva.Phasors.IeeeC37_118.DataFrame) Handles m_ieeeC37_118FrameParser.ReceivedDataFrame

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedFrameBufferImage(ByVal binaryImage() As Byte, ByVal offset As Integer, ByVal length As Integer) Handles m_ieeeC37_118FrameParser.ReceivedFrameBufferImage

        RaiseEvent ReceivedFrameBufferImage(binaryImage, offset, length)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedHeaderFrame(ByVal frame As Tva.Phasors.IeeeC37_118.HeaderFrame) Handles m_ieeeC37_118FrameParser.ReceivedHeaderFrame

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_DataStreamException(ByVal ex As System.Exception) Handles m_ieee1344FrameParser.DataStreamException

        RaiseEvent DataStreamException(ex)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedCommonFrameHeader(ByVal frame As Ieee1344.ICommonFrameHeader) Handles m_ieee1344FrameParser.ReceivedCommonFrameHeader

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedConfigurationFrame(ByVal frame As Tva.Phasors.Ieee1344.ConfigurationFrame) Handles m_ieee1344FrameParser.ReceivedConfigurationFrame

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedDataFrame(ByVal frame As Tva.Phasors.Ieee1344.DataFrame) Handles m_ieee1344FrameParser.ReceivedDataFrame

        ProcessFrame(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedFrameBufferImage(ByVal binaryImage() As Byte, ByVal offset As Integer, ByVal length As Integer) Handles m_ieee1344FrameParser.ReceivedFrameBufferImage

        RaiseEvent ReceivedFrameBufferImage(binaryImage, offset, length)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedHeaderFrame(ByVal frame As Tva.Phasors.Ieee1344.HeaderFrame) Handles m_ieee1344FrameParser.ReceivedHeaderFrame

        ProcessFrame(frame)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_DataStreamException(ByVal ex As System.Exception) Handles m_bpaPdcStreamFrameParser.DataStreamException

        RaiseEvent DataStreamException(ex)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_ReceivedConfigurationFrame1(ByVal frame As Tva.Phasors.BpaPdcStream.ConfigurationFrame) Handles m_bpaPdcStreamFrameParser.ReceivedConfigurationFrame1

        ProcessFrame(frame)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_ReceivedConfigurationFrame2(ByVal frame As Tva.Phasors.BpaPdcStream.ConfigurationFrame) Handles m_bpaPdcStreamFrameParser.ReceivedConfigurationFrame2

        ProcessFrame(frame)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_ReceivedDataFrame(ByVal frame As Tva.Phasors.BpaPdcStream.DataFrame) Handles m_bpaPdcStreamFrameParser.ReceivedDataFrame

        ProcessFrame(frame)

    End Sub

#End Region

#End Region

End Class
