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
Imports Tva.Collections
Imports Tva.DateTime.Common
Imports Tva.Phasors

''' <summary>Protocol independent frame parser</summary>
<CLSCompliant(False)> _
Public Class FrameParser

#Region " Public Member Declarations "

    Public Event ReceivedConfigurationFrame(ByVal frame As IConfigurationFrame)
    Public Event ReceivedDataFrame(ByVal frame As IDataFrame)
    Public Event ReceivedHeaderFrame(ByVal frame As IHeaderFrame)
    Public Event ReceivedCommandFrame(ByVal frame As ICommandFrame)
    Public Event ReceivedUndeterminedFrame(ByVal frame As IChannelFrame)
    Public Event DataStreamException(ByVal ex As Exception)

    Public Const DefaultBufferSize As Integer = 32768

#End Region

#Region " Private Member Declarations "

    ' Connection properties
    Private m_protocol As PmuProtocol
    Private m_transportLayer As DataTransportLayer
    Private m_hostIP As String
    Private m_port As Integer
    Private m_pmuID As Integer
    Private m_bufferSize As Integer = DefaultBufferSize

    ' It is unknown how end users will consume parsed data, so to make sure that data parsing is not slowed down by
    ' end user processing, we post parsed frames into their own real-time process queue for end user consumption
    Private WithEvents m_frameQueue As ProcessQueue(Of IChannelFrame)

    ' We internalize protocol specfic processing to simplfy end user consumption
    Private WithEvents m_ieeeC37_118FrameParser As IeeeC37_118.FrameParser
    Private WithEvents m_ieee1344FrameParser As Ieee1344.FrameParser
    Private WithEvents m_bpaPdcStreamFrameParser As BpaPdcStream.FrameParser

    Private m_socketThread As Thread
    Private m_tcpSocket As TcpClient
    Private m_udpSocket As Socket
    Private m_receptionPoint As EndPoint
    Private m_clientStream As NetworkStream
    Private m_dataStreamStartTime As Long
    Private m_totalFramesReceived As Long
    Private m_totalBytesReceived As Long

#End Region

#Region " Construction Functions "

    Public Sub New()

        m_protocol = Phasors.PmuProtocol.IeeeC37_118V1
        m_transportLayer = DataTransportLayer.Tcp

    End Sub

    Public Sub New(ByVal protocol As PmuProtocol, ByVal transportLayer As DataTransportLayer)

        m_protocol = protocol
        m_transportLayer = transportLayer

    End Sub

#End Region

#Region " Public Methods Implementation "

    Public Property Protocol() As PmuProtocol
        Get
            Return m_protocol
        End Get
        Set(ByVal value As PmuProtocol)
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

    Public Property Port() As Integer
        Get
            Return m_port
        End Get
        Set(ByVal value As Integer)
            m_port = value
        End Set
    End Property

    Public Property PmuID() As Integer
        Get
            Return m_pmuID
        End Get
        Set(ByVal value As Integer)
            m_pmuID = value
        End Set
    End Property

    Public Property BufferSize() As Integer
        Get
            Return m_bufferSize
        End Get
        Set(ByVal value As Integer)
            m_bufferSize = value
        End Set
    End Property

    Public Sub Connect()

        Disconnect()
        m_dataStreamStartTime = Date.Now.Ticks
        m_totalFramesReceived = 0

        ' Setup frame queue to hold parsed frames
        m_frameQueue = ProcessQueue(Of IChannelFrame).CreateRealTimeQueue(AddressOf ProcessFrame)
        m_frameQueue.Start()

        ' Instantiate protocol specific frame parser
        Select Case m_protocol
            Case Phasors.PmuProtocol.IeeeC37_118V1
                m_ieeeC37_118FrameParser = New IeeeC37_118.FrameParser(IeeeC37_118.RevisionNumber.RevisionV1)
                m_ieeeC37_118FrameParser.Start()
            Case Phasors.PmuProtocol.IeeeC37_118D6
                m_ieeeC37_118FrameParser = New IeeeC37_118.FrameParser(IeeeC37_118.RevisionNumber.RevisionD6)
                m_ieeeC37_118FrameParser.Start()
            Case Phasors.PmuProtocol.Ieee1344
                m_ieee1344FrameParser = New Ieee1344.FrameParser
                m_ieee1344FrameParser.Start()
            Case Phasors.PmuProtocol.BpaPdcStream
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

                ' Request configuration frame and make sure real-time data is enabled
                SendPmuCommand(PmuCommand.SendConfigurationFrame2)
                SendPmuCommand(PmuCommand.EnableRealTimeData)
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

    End Sub

    Public Sub Disconnect()

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

        If m_frameQueue IsNot Nothing Then m_frameQueue.Stop()
        m_frameQueue = Nothing

    End Sub

    Public ReadOnly Property Enabled() As Boolean
        Get
            Return m_frameQueue IsNot Nothing
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
            Return m_totalFramesReceived / TicksToSeconds(Date.Now.Ticks - m_dataStreamStartTime)
        End Get
    End Property

    Public ReadOnly Property ByteRate() As Double
        Get
            Return m_totalBytesReceived / TicksToSeconds(Date.Now.Ticks - m_dataStreamStartTime)
        End Get
    End Property

    Public ReadOnly Property QueuedFrames() As Integer
        Get
            Return m_frameQueue.Count
        End Get
    End Property

    Public Sub SendPmuCommand(ByVal command As PmuCommand)

        If m_clientStream IsNot Nothing Then
            Dim binaryImage As Byte()
            Dim binaryLength As Integer

            ' Only the IEEE protocols support commands
            Select Case m_protocol
                Case Phasors.PmuProtocol.IeeeC37_118V1, Phasors.PmuProtocol.IeeeC37_118D6
                    With New IeeeC37_118.CommandFrame(m_pmuID, command)
                        binaryImage = .BinaryImage
                        binaryLength = .BinaryLength
                    End With
                Case Phasors.PmuProtocol.Ieee1344
                    With New Ieee1344.CommandFrame(m_pmuID, command)
                        binaryImage = .BinaryImage
                        binaryLength = .BinaryLength
                    End With
            End Select

            If binaryLength > 0 Then m_clientStream.Write(binaryImage, 0, binaryLength)
        End If

    End Sub

#End Region

#Region " Private Methods Implementation "

    ' The is the frame queue processing procedure - it just ferries queued frames back out through their native fundamental interfaces
    Private Sub ProcessFrame(ByVal frame As IChannelFrame)

        m_totalFramesReceived += 1

        ' We expose fundamental frame through is base interface regardless of protocol - this lets end user
        ' write a single application for any given protocol...
        Select Case frame.FrameType
            Case FundamentalFrameType.ConfigurationFrame
                RaiseEvent ReceivedConfigurationFrame(frame)
            Case FundamentalFrameType.DataFrame
                RaiseEvent ReceivedDataFrame(frame)
            Case FundamentalFrameType.HeaderFrame
                RaiseEvent ReceivedHeaderFrame(frame)
            Case FundamentalFrameType.CommandFrame
                RaiseEvent ReceivedCommandFrame(frame)
            Case Else
                RaiseEvent ReceivedUndeterminedFrame(frame)
        End Select

    End Sub

    Private Sub Write(ByVal buffer As Byte(), ByVal received As Integer)

        Select Case m_protocol
            Case Phasors.PmuProtocol.IeeeC37_118V1, Phasors.PmuProtocol.IeeeC37_118D6
                m_ieeeC37_118FrameParser.Write(buffer, 0, received)
            Case Phasors.PmuProtocol.Ieee1344
                m_ieee1344FrameParser.Write(buffer, 0, received)
            Case Phasors.PmuProtocol.BpaPdcStream
                m_bpaPdcStreamFrameParser.Write(buffer, 0, received)
        End Select

    End Sub

    Private Sub ProcessUdpStream()

        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), m_bufferSize)
        Dim received As Integer

        ' Enter the data read loop
        Do While True
            Try
                ' Block thread until we've received some data...
                received = m_udpSocket.ReceiveFrom(buffer, m_receptionPoint)

                ' If we get no data we'll just go back to listening...
                If received > 0 Then
                    ' Provide received buffer to frame parser
                    Write(buffer, received)
                    m_totalBytesReceived += received
                End If
            Catch ex As Exception
                If Not TypeOf ex Is ThreadAbortException AndAlso Not TypeOf ex Is System.IO.IOException Then
                    RaiseEvent DataStreamException(ex)
                End If
                Exit Do
            End Try
        Loop

    End Sub

    Private Sub ProcessTcpStream()

        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), m_bufferSize)
        Dim received As Integer

        ' Enter the data read loop
        Do While True
            Try
                ' Block thread until we've received some data...
                received = m_clientStream.Read(buffer, 0, m_bufferSize)

                ' If we get no data we'll just go back to listening...
                If received > 0 Then
                    ' Provide received buffer to frame parser
                    Write(buffer, received)
                    m_totalBytesReceived += received
                End If
            Catch ex As Exception
                If Not TypeOf ex Is ThreadAbortException AndAlso Not TypeOf ex Is System.IO.IOException Then
                    RaiseEvent DataStreamException(ex)
                End If
                Exit Do
            End Try
        Loop

    End Sub

#Region " Protocol Specific Event Handlers "

    Private Sub m_frameQueue_ProcessException(ByVal ex As System.Exception) Handles m_frameQueue.ProcessException

        RaiseEvent DataStreamException(ex)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_DataStreamException(ByVal ex As System.Exception) Handles m_ieeeC37_118FrameParser.DataStreamException

        RaiseEvent DataStreamException(ex)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedCommandFrame(ByVal frame As Tva.Phasors.IeeeC37_118.CommandFrame) Handles m_ieeeC37_118FrameParser.ReceivedCommandFrame

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedConfigurationFrame1(ByVal frame As Tva.Phasors.IeeeC37_118.ConfigurationFrame) Handles m_ieeeC37_118FrameParser.ReceivedConfigurationFrame1

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedConfigurationFrame2(ByVal frame As Tva.Phasors.IeeeC37_118.ConfigurationFrame) Handles m_ieeeC37_118FrameParser.ReceivedConfigurationFrame2

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedDataFrame(ByVal frame As Tva.Phasors.IeeeC37_118.DataFrame) Handles m_ieeeC37_118FrameParser.ReceivedDataFrame

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieeeC37_118FrameParser_ReceivedHeaderFrame(ByVal frame As Tva.Phasors.IeeeC37_118.HeaderFrame) Handles m_ieeeC37_118FrameParser.ReceivedHeaderFrame

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_DataStreamException(ByVal ex As System.Exception) Handles m_ieee1344FrameParser.DataStreamException

        RaiseEvent DataStreamException(ex)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedCommandFrame(ByVal frame As Tva.Phasors.Ieee1344.CommandFrame) Handles m_ieee1344FrameParser.ReceivedCommandFrame

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedConfigurationFrame1(ByVal frame As Tva.Phasors.Ieee1344.ConfigurationFrame) Handles m_ieee1344FrameParser.ReceivedConfigurationFrame1

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedConfigurationFrame2(ByVal frame As Tva.Phasors.Ieee1344.ConfigurationFrame) Handles m_ieee1344FrameParser.ReceivedConfigurationFrame2

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedDataFrame(ByVal frame As Tva.Phasors.Ieee1344.DataFrame) Handles m_ieee1344FrameParser.ReceivedDataFrame

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_ieee1344FrameParser_ReceivedHeaderFrame(ByVal frame As Tva.Phasors.Ieee1344.HeaderFrame) Handles m_ieee1344FrameParser.ReceivedHeaderFrame

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_DataStreamException(ByVal ex As System.Exception) Handles m_bpaPdcStreamFrameParser.DataStreamException

        RaiseEvent DataStreamException(ex)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_ReceivedConfigurationFrame1(ByVal frame As Tva.Phasors.BpaPdcStream.ConfigurationFrame) Handles m_bpaPdcStreamFrameParser.ReceivedConfigurationFrame1

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_ReceivedConfigurationFrame2(ByVal frame As Tva.Phasors.BpaPdcStream.ConfigurationFrame) Handles m_bpaPdcStreamFrameParser.ReceivedConfigurationFrame2

        m_frameQueue.Add(frame)

    End Sub

    Private Sub m_bpaPdcStreamFrameParser_ReceivedDataFrame(ByVal frame As Tva.Phasors.BpaPdcStream.DataFrame) Handles m_bpaPdcStreamFrameParser.ReceivedDataFrame

        m_frameQueue.Add(frame)

    End Sub

#End Region

#End Region

End Class
