'*******************************************************************************************************
'  MultiProtocolFrameParser.vb - Protocol independent frame parser
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  03/16/2006 - J. Ritchie Carroll
'       Initial version of source generated
'  06/26/2006 - Pinal C. Patel
'       Changed out the socket code with TcpClient and UdpClient components from Tva.Communication
'  01/31/2007 - J. Ritchie Carroll
'       Added TCP "server" support to allow listening connections from devices that act as data
'       clients, e.g., FNET devices
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Net
Imports System.Threading
Imports System.ComponentModel
Imports Tva.Collections
Imports Tva.DateTime.Common
Imports Tva.Phasors
Imports Tva.Communication
Imports Tva.Communication.Common
Imports Tva.IO.Common
Imports Tva.Text.Common

''' <summary>Protocol independent frame parser</summary>
<CLSCompliant(False)> _
Public Class MultiProtocolFrameParser

    Implements IFrameParser

#Region " Public Member Declarations "

    Public Event ReceivedConfigurationFrame(ByVal frame As IConfigurationFrame) Implements IFrameParser.ReceivedConfigurationFrame
    Public Event ReceivedDataFrame(ByVal frame As IDataFrame) Implements IFrameParser.ReceivedDataFrame
    Public Event ReceivedHeaderFrame(ByVal frame As IHeaderFrame) Implements IFrameParser.ReceivedHeaderFrame
    Public Event ReceivedCommandFrame(ByVal frame As ICommandFrame) Implements IFrameParser.ReceivedCommandFrame
    Public Event ReceivedUndeterminedFrame(ByVal frame As IChannelFrame) Implements IFrameParser.ReceivedUndeterminedFrame
    Public Event ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer) Implements IFrameParser.ReceivedFrameBufferImage
    Public Event ConfigurationChanged() Implements IFrameParser.ConfigurationChanged
    Public Event DataStreamException(ByVal ex As Exception) Implements IFrameParser.DataStreamException
    Public Event ConnectionException(ByVal ex As Exception, ByVal connectionAttempts As Integer)
    Public Event AttemptingConnection()
    Public Event Connected()
    Public Event Disconnected()
    Public Event ServerStarted()
    Public Event ServerStopped()

    Public Const DefaultBufferSize As Int32 = 262144    ' 256K
    Public Const DefaultFrameRate As Double = 1 / 30

#End Region

#Region " Private Member Declarations "

    ' Connection properties
    Private m_phasorProtocol As PhasorProtocol
    Private m_transportProtocol As TransportProtocol
    Private m_connectionString As String
    Private m_maximumConnectionAttempts As Integer
    Private m_pmuID As UInt16
    Private m_bufferSize As Int32

    ' We internalize protocol specfic processing to simplfy end user consumption
    Private WithEvents m_frameParser As IFrameParser
    Private WithEvents m_communicationClient As ICommunicationClient
    Private WithEvents m_communicationServer As ICommunicationServer
    Private WithEvents m_rateCalcTimer As Timers.Timer

    Private m_configurationFrame As IConfigurationFrame
    Private m_dataStreamStartTime As Long
    Private m_executeParseOnSeperateThread As Boolean
    Private m_totalFramesReceived As Long
    Private m_frameRateTotal As Int32
    Private m_byteRateTotal As Int32
    Private m_totalBytesReceived As Long
    Private m_frameRate As Double
    Private m_byteRate As Double
    Private m_sourceName As String
    Private m_definedFrameRate As Double
    Private m_lastFrameReceivedTime As Long
    Private m_autoStartDataParsingSequence As Boolean
    Private m_initiatingDataStream As Boolean
    Private m_initialBytesReceived As Long
    Private m_deviceSupportsCommands As Boolean
    Private m_enabled As Boolean

#End Region

#Region " Construction Functions "

    Public Sub New()

        m_connectionString = "server=127.0.0.1; port=4712"
        m_pmuID = 1
        m_bufferSize = DefaultBufferSize
        m_definedFrameRate = DefaultFrameRate
        m_rateCalcTimer = New Timers.Timer
        m_maximumConnectionAttempts = -1
        m_autoStartDataParsingSequence = True

        m_phasorProtocol = PhasorProtocol.IeeeC37_118V1
        m_transportProtocol = TransportProtocol.Tcp

        With m_rateCalcTimer
            .Interval = 1000
            .AutoReset = True
            .Enabled = False
        End With

    End Sub

    Public Sub New(ByVal phasorProtocol As PhasorProtocol, ByVal transportProtocol As TransportProtocol)

        MyClass.New()
        m_phasorProtocol = phasorProtocol
        m_transportProtocol = transportProtocol

    End Sub

#End Region

#Region " Public Methods Implementation "

    Public Property PhasorProtocol() As PhasorProtocol
        Get
            Return m_phasorProtocol
        End Get
        Set(ByVal value As PhasorProtocol)
            m_phasorProtocol = value
            m_deviceSupportsCommands = GetDerivedCommandSupport()
        End Set
    End Property

    Property TransportProtocol() As TransportProtocol
        Get
            Return m_transportProtocol
        End Get
        Set(ByVal value As TransportProtocol)
            ' UDP transport has special requirements on buffer size
            If value = Communication.TransportProtocol.Udp Then
                If m_bufferSize < UdpClient.MinimumUdpBufferSize Then m_bufferSize = UdpClient.MinimumUdpBufferSize
                If m_bufferSize > UdpClient.MaximumUdpDatagramSize Then m_bufferSize = UdpClient.MaximumUdpDatagramSize
            Else
                m_bufferSize = DefaultBufferSize
            End If

            m_transportProtocol = value
            m_deviceSupportsCommands = GetDerivedCommandSupport()
        End Set
    End Property

    Public Property ConnectionString() As String
        Get
            Return m_connectionString
        End Get
        Set(ByVal value As String)
            m_connectionString = value
            m_deviceSupportsCommands = GetDerivedCommandSupport()
        End Set
    End Property

    Public Property DeviceSupportsCommands() As Boolean
        Get
            Return m_deviceSupportsCommands
        End Get
        Set(ByVal value As Boolean)
            ' Consumers can choose to override command support if needed
            m_deviceSupportsCommands = value
        End Set
    End Property

    Public Property PmuID() As UInt16
        Get
            Return m_pmuID
        End Get
        Set(ByVal value As UInt16)
            m_pmuID = value
        End Set
    End Property

    Public Property BufferSize() As Int32
        Get
            Return m_bufferSize
        End Get
        Set(ByVal value As Int32)
            m_bufferSize = value

            ' UDP has special restrictions on overall buffer size
            If m_transportProtocol = Communication.TransportProtocol.Udp Then
                If m_bufferSize < UdpClient.MinimumUdpBufferSize Then m_bufferSize = UdpClient.MinimumUdpBufferSize
                If m_bufferSize > UdpClient.MaximumUdpDatagramSize Then m_bufferSize = UdpClient.MaximumUdpDatagramSize
            End If
        End Set
    End Property

    Public Property DefinedFrameRate() As Double
        Get
            Return m_definedFrameRate
        End Get
        Set(ByVal value As Double)
            m_definedFrameRate = value
        End Set
    End Property

    Public Property MaximumConnectionAttempts() As Integer
        Get
            Return m_maximumConnectionAttempts
        End Get
        Set(ByVal value As Integer)
            m_maximumConnectionAttempts = value
        End Set
    End Property

    Public Property AutoStartDataParsingSequence() As Boolean
        Get
            Return m_autoStartDataParsingSequence
        End Get
        Set(ByVal value As Boolean)
            m_autoStartDataParsingSequence = value
        End Set
    End Property

    Public Property SourceName() As String
        Get
            Return m_sourceName
        End Get
        Set(ByVal value As String)
            m_sourceName = value
        End Set
    End Property

    Public ReadOnly Property ConnectionName() As String
        Get
            If m_sourceName Is Nothing Then
                Return m_pmuID & " (" & m_connectionString & ")"
            Else
                Return m_sourceName & ", ID " & m_pmuID & " (" & m_connectionString & ")"
            End If
        End Get
    End Property

    Public Sub Start() Implements IFrameParser.Start

        [Stop]()
        m_totalFramesReceived = 0
        m_frameRateTotal = 0
        m_byteRateTotal = 0
        m_totalBytesReceived = 0
        m_frameRate = 0.0#
        m_byteRate = 0.0#

        Try
            ' Instantiate protocol specific frame parser
            Select Case m_phasorProtocol
                Case Phasors.PhasorProtocol.IeeeC37_118V1
                    m_frameParser = New IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft7)
                Case Phasors.PhasorProtocol.IeeeC37_118D6
                    m_frameParser = New IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft6)
                Case Phasors.PhasorProtocol.Ieee1344
                    m_frameParser = New Ieee1344.FrameParser
                Case Phasors.PhasorProtocol.BpaPdcStream
                    m_frameParser = New BpaPdcStream.FrameParser
                Case Phasors.PhasorProtocol.FNet
                    m_frameParser = New FNet.FrameParser
            End Select

            m_frameParser.ExecuteParseOnSeperateThread = m_executeParseOnSeperateThread
            m_frameParser.Start()

            ' Instantiate selected transport layer
            Select Case m_transportProtocol
                Case TransportProtocol.Tcp
                    ' The TCP transport may be set up as a server or as a client, we distinguish
                    ' this simply by deriving the value of an added key/value pair in the
                    ' connection string called "IsListener"
                    With ParseKeyValuePairs(m_connectionString)
                        If .ContainsKey("islistener") Then
                            If ParseBoolean(.Item("islistener")) Then
                                m_communicationServer = New TcpServer
                            Else
                                m_communicationClient = New TcpClient
                            End If
                        Else
                            ' If the key doesn't exist, we assume it's a client connection
                            ' (this way old connections strings are still backwards compatible)
                            m_communicationClient = New TcpClient
                        End If
                    End With
                Case TransportProtocol.Udp
                    m_communicationClient = New UdpClient
                Case TransportProtocol.Serial
                    m_communicationClient = New SerialClient
                Case TransportProtocol.File
                    m_communicationClient = New FileClient
            End Select

            If m_communicationClient IsNot Nothing Then
                ' Attempting connection to device
                With m_communicationClient
                    .ReceiveRawDataFunction = AddressOf IFrameParserWrite
                    .ReceiveBufferSize = m_bufferSize
                    .ConnectionString = m_connectionString
                    .MaximumConnectionAttempts = m_maximumConnectionAttempts
                    .Handshake = False
                    .Connect()
                End With
            ElseIf m_communicationServer IsNot Nothing Then
                ' Listening for device connection
                With m_communicationServer
                    .ReceiveRawDataFunction = AddressOf IFrameParserWrite
                    .ReceiveBufferSize = m_bufferSize
                    .ConfigurationString = m_connectionString
                    .MaximumClients = 1
                    .Handshake = False
                    .Start()
                End With
            Else
                Throw New InvalidOperationException("No communications layer was initialized, cannot start parser")
            End If

            m_rateCalcTimer.Enabled = True
            m_enabled = True
        Catch
            [Stop]()
            Throw
        End Try

    End Sub

    Public Sub [Stop]() Implements IFrameParser.Stop

        m_enabled = False
        m_rateCalcTimer.Enabled = False

        If m_communicationClient IsNot Nothing Then m_communicationClient.Disconnect()
        If m_communicationServer IsNot Nothing Then m_communicationServer.Stop()
        If m_frameParser IsNot Nothing Then m_frameParser.Stop()

        m_lastFrameReceivedTime = 0
        m_configurationFrame = Nothing
        m_frameParser = Nothing
        m_communicationClient = Nothing
        m_communicationServer = Nothing

    End Sub

    Public Property ExecuteParseOnSeperateThread() As Boolean Implements IFrameParser.ExecuteParseOnSeperateThread
        Get
            Return m_executeParseOnSeperateThread
        End Get
        Set(ByVal value As Boolean)
            m_executeParseOnSeperateThread = value
        End Set
    End Property

    Public Property ConfigurationFrame() As IConfigurationFrame Implements IFrameParser.ConfigurationFrame
        Get
            Return m_configurationFrame
        End Get
        Set(ByVal value As IConfigurationFrame)
            m_configurationFrame = value

            ' Pass new config frame onto appropriate parser, casting into appropriate protocol if needed...
            If m_frameParser IsNot Nothing Then m_frameParser.ConfigurationFrame = value
        End Set
    End Property

    Public ReadOnly Property IsIEEEProtocol() As Boolean
        Get
            Return m_phasorProtocol = Phasors.PhasorProtocol.IeeeC37_118V1 OrElse _
                m_phasorProtocol = Phasors.PhasorProtocol.IeeeC37_118D6 OrElse _
                m_phasorProtocol = Phasors.PhasorProtocol.Ieee1344
        End Get
    End Property

    Public ReadOnly Property Enabled() As Boolean Implements IFrameParser.Enabled
        Get
            Return m_enabled
        End Get
    End Property

    Public ReadOnly Property QueuedBuffers() As Int32 Implements IFrameParser.QueuedBuffers
        Get
            If m_frameParser IsNot Nothing Then Return m_frameParser.QueuedBuffers
        End Get
    End Property

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public ReadOnly Property InternalFrameParser() As IFrameParser
        Get
            Return m_frameParser
        End Get
    End Property

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public ReadOnly Property InternalCommunicationClient() As ICommunicationClient
        Get
            Return m_communicationClient
        End Get
    End Property

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public ReadOnly Property InternalCommunicationServer() As ICommunicationServer
        Get
            Return m_communicationServer
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

    Public Sub SendDeviceCommand(ByVal command As DeviceCommand)

        If m_deviceSupportsCommands AndAlso (m_communicationClient IsNot Nothing OrElse m_communicationServer IsNot Nothing) Then
            Dim binaryImage As Byte()
            Dim binaryLength As Int32

            ' Only the IEEE protocols support commands
            Select Case m_phasorProtocol
                Case Phasors.PhasorProtocol.IeeeC37_118V1, Phasors.PhasorProtocol.IeeeC37_118D6
                    With New IeeeC37_118.CommandFrame(m_pmuID, command, 1)
                        binaryImage = .BinaryImage
                        binaryLength = binaryImage.Length
                    End With
                Case Phasors.PhasorProtocol.Ieee1344
                    With New Ieee1344.CommandFrame(m_pmuID, command)
                        binaryImage = .BinaryImage
                        binaryLength = binaryImage.Length
                    End With
                Case Else
                    binaryImage = Nothing
                    binaryLength = 0
            End Select

            If binaryLength > 0 Then
                If m_communicationClient IsNot Nothing Then
                    m_communicationClient.Send(binaryImage)
                Else
                    m_communicationServer.Multicast(binaryImage)
                End If
            End If
        End If

    End Sub

    Public ReadOnly Property Status() As String Implements IFrameParser.Status
        Get
            With New StringBuilder
                .Append("     PDC/PMU Connection ID: ")
                .Append(m_pmuID)
                .Append(Environment.NewLine)
                .Append("         Connection string: ")
                .Append(m_connectionString)
                .Append(Environment.NewLine)
                .Append("           Phasor protocol: ")
                .Append([Enum].GetName(GetType(PhasorProtocol), PhasorProtocol))
                .Append(Environment.NewLine)
                .Append("               Buffer size: ")
                .Append(BufferSize)
                .Append(Environment.NewLine)
                .Append("     Total frames received: ")
                .Append(TotalFramesReceived)
                .Append(Environment.NewLine)
                .Append("     Calculated frame rate: ")
                .Append(FrameRate)
                .Append(Environment.NewLine)
                .Append("      Calculated byte rate: ")
                .Append(ByteRate)
                .Append(Environment.NewLine)
                .Append("   Calculated MegaBit rate: ")
                .Append(MegaBitRate.ToString("0.0000") & " mbps")
                .Append(Environment.NewLine)

                If m_frameParser IsNot Nothing Then .Append(m_frameParser.Status)
                If m_communicationClient IsNot Nothing Then .Append(m_communicationClient.Status)
                If m_communicationServer IsNot Nothing Then .Append(m_communicationServer.Status)

                Return .ToString()
            End With
        End Get
    End Property

#End Region

#Region " Private Methods Implementation "

    Private Sub m_rateCalcTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_rateCalcTimer.Elapsed

        Dim time As Double = TicksToSeconds(Date.Now.Ticks - m_dataStreamStartTime)

        m_frameRate = m_frameRateTotal / time
        m_byteRate = m_byteRateTotal / time
        m_totalBytesReceived += m_byteRateTotal

        m_frameRateTotal = 0
        m_byteRateTotal = 0
        m_dataStreamStartTime = Date.Now.Ticks

    End Sub

    ' We access the "raw data function" of the ICommunicationClient and ICommunicationServer for a speed boost in communications processing...
    Private Sub IFrameParserWrite(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer) Implements IFrameParser.Write

        ' Pass data from communications client into protocol specific frame parser
        m_frameParser.Write(buffer, offset, count)
        m_byteRateTotal += count
        If m_initiatingDataStream Then m_initialBytesReceived += count

    End Sub

    Private Sub ClientConnected()

        RaiseEvent Connected()

        ' Begin data parsing sequence to handle reception of configuration frame
        If m_deviceSupportsCommands AndAlso m_autoStartDataParsingSequence Then
            m_initialBytesReceived = 0
            m_initiatingDataStream = True
            ThreadPool.QueueUserWorkItem(AddressOf StartDataParsingSequence)
        End If

    End Sub

    Private Sub StartDataParsingSequence(ByVal state As Object)

        Dim attempts As Integer

        ' Some devices will only send a config frame once data streaming has been disabled, so
        ' we use this code to disable real-time data and wait for data to stop streaming...
        Try
            ' Make sure data stream is disabled
            SendDeviceCommand(DeviceCommand.DisableRealTimeData)
            Thread.Sleep(300)

            ' Wait for real-time data stream to cease for up to two seconds
            Do While m_initialBytesReceived > 0
                m_initialBytesReceived = 0
                Thread.Sleep(100)

                attempts += 1
                If attempts >= 20 Then Exit Do
            Loop
        Catch
            Throw
        Finally
            m_initiatingDataStream = False
        End Try

        ' Request configuration frame once real-time data has been disabled
        SendDeviceCommand(DeviceCommand.SendConfigurationFrame2)

    End Sub

    Private Function GetDerivedCommandSupport() As Boolean

        ' Command support is based on phasor protocol, transport protocol and connection style
        If IsIEEEProtocol Then
            ' IEEE protocols using TCP or Serial connection support device commands
            If m_transportProtocol = Communication.TransportProtocol.Tcp OrElse _
                m_transportProtocol = Communication.TransportProtocol.Serial Then _
                    Return True

            ' IEEE protocols "can" use UDP connection to support devices commands, but only
            ' when remote device acts as a UDP listener (i.e., a "server" connection)
            If m_transportProtocol = Communication.TransportProtocol.Udp Then
                If Not String.IsNullOrEmpty(m_connectionString) Then
                    With ParseKeyValuePairs(m_connectionString)
                        Return .ContainsKey("server")
                    End With
                End If
            End If
        End If

        Return False

    End Function

#Region " Communications Client Event Handlers "

    Private Sub m_communicationClient_Connected(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_communicationClient.Connected

        ClientConnected()

    End Sub

    Private Sub m_communicationClient_Connecting(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Connecting

        RaiseEvent AttemptingConnection()

    End Sub

    Private Sub m_communicationClient_ConnectingException(ByVal sender As Object, ByVal e As ExceptionEventArgs) Handles m_communicationClient.ConnectingException

        RaiseEvent ConnectionException(e.Exception, e.OccurrenceCount)

    End Sub

    Private Sub m_communicationClient_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Disconnected

        RaiseEvent Disconnected()
        m_communicationClient = Nothing

    End Sub

#End Region

#Region " Communications Server Event Handlers "

    Private Sub m_communicationServer_ClientConnected(ByVal sender As Object, ByVal e As IdentifiableSourceEventArgs) Handles m_communicationServer.ClientConnected

        ClientConnected()

    End Sub

    Private Sub m_communicationServer_ClientDisconnected(ByVal sender As Object, ByVal e As IdentifiableSourceEventArgs) Handles m_communicationServer.ClientDisconnected

        RaiseEvent Disconnected()

    End Sub

    Private Sub m_communicationServer_ServerStarted(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationServer.ServerStarted

        RaiseEvent ServerStarted()

    End Sub

    Private Sub m_communicationServer_ServerStopped(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationServer.ServerStopped

        RaiseEvent ServerStopped()

    End Sub

    Private Sub m_communicationServer_ServerStartupException(ByVal sender As Object, ByVal e As ExceptionEventArgs) Handles m_communicationServer.ServerStartupException

        RaiseEvent ConnectionException(e.Exception, e.OccurrenceCount)

    End Sub

#End Region

#Region " Frame Parser Event Handlers "

    Private Sub m_frameParser_ReceivedCommandFrame(ByVal frame As ICommandFrame) Handles m_frameParser.ReceivedCommandFrame

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedCommandFrame(frame)

    End Sub

    Private Sub m_frameParser_ReceivedConfigurationFrame(ByVal frame As IConfigurationFrame) Handles m_frameParser.ReceivedConfigurationFrame

        ' We automatically request enabling of real-time data upon reception of config frame if requested
        If m_configurationFrame Is Nothing AndAlso m_deviceSupportsCommands AndAlso m_autoStartDataParsingSequence Then
            SendDeviceCommand(DeviceCommand.EnableRealTimeData)
        End If

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        m_configurationFrame = frame
        RaiseEvent ReceivedConfigurationFrame(frame)

    End Sub

    Private Sub m_frameParser_ReceivedDataFrame(ByVal frame As IDataFrame) Handles m_frameParser.ReceivedDataFrame

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedDataFrame(frame)

        If m_transportProtocol = Communication.TransportProtocol.File AndAlso m_lastFrameReceivedTime > 0 Then
            ' To keep precise timing on "frames per second", we wait for defined frame rate interval
            Dim sleepTime As Double = m_definedFrameRate - TicksToSeconds(Date.Now.Ticks - m_lastFrameReceivedTime)
            If sleepTime > 0 Then Thread.Sleep(sleepTime * 1000)
        End If

        m_lastFrameReceivedTime = Date.Now.Ticks

    End Sub

    Private Sub m_frameParser_ReceivedHeaderFrame(ByVal frame As IHeaderFrame) Handles m_frameParser.ReceivedHeaderFrame

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedHeaderFrame(frame)

    End Sub

    Private Sub m_frameParser_ReceivedUndeterminedFrame(ByVal frame As IChannelFrame) Handles m_frameParser.ReceivedUndeterminedFrame

        m_totalFramesReceived += 1
        m_frameRateTotal += 1
        RaiseEvent ReceivedUndeterminedFrame(frame)

    End Sub

    Private Sub m_frameParser_ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage() As Byte, ByVal offset As Integer, ByVal length As Integer) Handles m_frameParser.ReceivedFrameBufferImage

        RaiseEvent ReceivedFrameBufferImage(frameType, binaryImage, offset, length)

    End Sub

    Private Sub m_frameParser_ConfigurationChanged() Handles m_frameParser.ConfigurationChanged

        RaiseEvent ConfigurationChanged()

    End Sub

    Private Sub m_frameParser_DataStreamException(ByVal ex As System.Exception) Handles m_frameParser.DataStreamException

        RaiseEvent DataStreamException(ex)

    End Sub

#End Region

#Region " Old Socket Code "

    'Private m_socketThread As Thread
    'Private m_tcpSocket As Sockets.TcpClient
    'Private m_udpSocket As Socket
    'Private m_receptionPoint As EndPoint
    'Private m_clientStream As NetworkStream

    '' Validate minimal connection parameters required for TCP connection
    'If String.IsNullOrEmpty(m_hostIP) Then Throw New InvalidOperationException("Cannot start TCP stream listener without specifing a host IP")
    'If m_port = 0 Then Throw New InvalidOperationException("Cannot start TCP stream listener without specifing a port")

    '' Connect to PDC/PMU using TCP
    'm_tcpSocket = New Sockets.TcpClient
    'm_tcpSocket.ReceiveBufferSize = m_bufferSize
    'm_tcpSocket.Connect(m_hostIP, m_port)
    'm_clientStream = m_tcpSocket.GetStream()

    '' Start listening to TCP data stream
    'm_socketThread = New Thread(AddressOf ProcessTcpStream)
    'm_socketThread.Start()

    '' Validate minimal connection parameters required for UDP connection
    'If m_port = 0 Then Throw New InvalidOperationException("Cannot start UDP stream listener without specifing a valid port")

    '' Connect to PDC/PMU using UDP (just listening to incoming stream on specified port)
    'm_udpSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
    'm_receptionPoint = CType(New IPEndPoint(IPAddress.Any, m_port), System.Net.EndPoint)
    'm_udpSocket.ReceiveBufferSize = m_bufferSize
    'm_udpSocket.Bind(m_receptionPoint)

    '' Start listening to UDP data stream
    'm_socketThread = New Thread(AddressOf ProcessUdpStream)
    'm_socketThread.Start()

    'If m_socketThread IsNot Nothing Then m_socketThread.Abort()
    'm_socketThread = Nothing

    'If m_tcpSocket IsNot Nothing Then m_tcpSocket.Close()
    'm_tcpSocket = Nothing

    'If m_udpSocket IsNot Nothing Then m_udpSocket.Close()
    'm_udpSocket = Nothing

    'm_clientStream = Nothing
    'm_receptionPoint = Nothing

    'Private Sub ProcessUdpStream()

    'Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
    'Dim received As Int32

    '' Enter the data read loop
    'Do While True
    '    Try
    '        ' Block thread until we've received some data...
    '        received = m_udpSocket.ReceiveFrom(buffer, m_receptionPoint)

    '        ' Provide received buffer to protocol specific frame parser
    '        If received > 0 Then Write(buffer, 0, received)
    '    Catch ex As ThreadAbortException
    '        ' If we received an abort exception, we'll egress gracefully
    '        Exit Do
    '    Catch ex As IOException
    '        ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
    '        ' in this case we'll bow out gracefully as well...
    '        Exit Do
    '    Catch ex As Exception
    '        RaiseEvent DataStreamException(ex)
    '        Exit Do
    '    End Try
    'Loop

    'End Sub

    'Private Sub ProcessTcpStream()

    'Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
    'Dim received, attempts As Integer

    '' Handle reception of configuration frame - in case of device that only responds to commands when not sending real-time data,
    '' such as the SEL 421, we disable real-time data stream first...
    'Try
    '    ' Make sure data stream is disabled
    '    SendPmuCommand(DeviceCommand.DisableRealTimeData)

    '    ' Wait for real-time data stream to cease
    '    Do While m_clientStream.DataAvailable
    '        ' Remove all existing data from stream
    '        Do While m_clientStream.DataAvailable
    '            received = m_clientStream.Read(buffer, 0, buffer.Length)
    '        Loop

    '        Thread.Sleep(100)

    '        attempts += 1
    '        If attempts >= 50 Then Exit Do
    '    Loop

    '    ' Request configuration frame 2 (we'll try a few times)
    '    attempts = 0
    '    m_configurationFrame = Nothing

    '    For x As Integer = 1 To 4
    '        SendPmuCommand(DeviceCommand.SendConfigurationFrame2)

    '        Do While m_configurationFrame Is Nothing
    '            ' So long as we are receiving data, we'll push it to the frame parser
    '            Do While m_clientStream.DataAvailable
    '                ' Block thread until we've read some data...
    '                received = m_clientStream.Read(buffer, 0, buffer.Length)

    '                ' Send received data to frame parser
    '                If received > 0 Then Write(buffer, 0, received)
    '            Loop

    '            ' Hang out for a little while so config frame can be parsed
    '            Thread.Sleep(100)

    '            attempts += 1
    '            If attempts >= 50 Then Exit Do
    '        Loop

    '        If m_configurationFrame IsNot Nothing Then Exit For
    '    Next

    '    ' Enable data stream
    '    SendPmuCommand(DeviceCommand.EnableRealTimeData)
    'Catch ex As ThreadAbortException
    '    ' If we received an abort exception, we'll egress gracefully
    '    Exit Sub
    'Catch ex As IOException
    '    ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
    '    ' in this case we'll bow out gracefully as well...
    '    Exit Sub
    'Catch ex As Exception
    '    RaiseEvent DataStreamException(ex)
    '    Exit Sub
    'End Try

    '' Enter the data read loop
    'Do While True
    '    Try
    '        ' Block thread until we've received some data...
    '        received = m_clientStream.Read(buffer, 0, buffer.Length)

    '        ' Provide received buffer to protocol specific frame parser
    '        If received > 0 Then Write(buffer, 0, received)
    '    Catch ex As ThreadAbortException
    '        ' If we received an abort exception, we'll egress gracefully
    '        Exit Do
    '    Catch ex As IOException
    '        ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
    '        ' in this case we'll bow out gracefully as well...
    '        Exit Do
    '    Catch ex As Exception
    '        RaiseEvent DataStreamException(ex)
    '        Exit Do
    '    End Try
    'Loop

    'End Sub

    'Private Sub m_communicationClient_ReceivedData(ByVal sender As Object, ByVal e As DataEventArgs) Handles m_communicationClient.ReceivedData

    '    With e
    '        ' Pass data from communications client into protocol specific frame parser
    '        m_frameParser.Write(.Data, 0, .Data.Length)
    '        m_byteRateTotal += .Data.Length
    '        If m_initiatingDataStream Then m_initialBytesReceived += .Data.Length
    '    End With

    'End Sub

#End Region

#End Region

End Class
