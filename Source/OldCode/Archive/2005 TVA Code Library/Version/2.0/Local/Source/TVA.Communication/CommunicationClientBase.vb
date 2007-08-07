'*******************************************************************************************************
'  TVA.Communication.ClientBase.vb - Base functionality of a client for transporting data
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/01/2006 - Pinal C. Patel
'       Original version of source code generated
'  09/06/2006 - J. Ritchie Carroll
'       Added bypass optimizations for high-speed client data access
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports System.Drawing
Imports System.ComponentModel
Imports TVA.Common
Imports TVA.Serialization
Imports TVA.Services
Imports TVA.IO.Common
Imports TVA.DateTime.Common
Imports TVA.Communication.CommunicationHelper
Imports TVA.Communication.Common

''' <summary>
''' Represents a client involved in the transportation of data.
''' </summary>
<ToolboxBitmap(GetType(CommunicationClientBase)), DefaultEvent("ReceivedData")> _
Public MustInherit Class CommunicationClientBase
    Implements ICommunicationClient, IPersistSettings, ISupportInitialize

#Region " Member Declaration "

    Private m_connectionString As String
    Private m_receiveBufferSize As Integer
    Private m_receiveTimeout As Integer
    Private m_maximumConnectionAttempts As Integer
    Private m_textEncoding As Encoding
    Private m_protocol As TransportProtocol
    Private m_secureSession As Boolean
    Private m_handshake As Boolean
    Private m_handshakePassphrase As String
    Private m_encryption As TVA.Security.Cryptography.EncryptLevel
    Private m_compression As TVA.IO.Compression.CompressLevel
    Private m_crcCheck As CRCCheckType
    Private m_enabled As Boolean
    Private m_serverID As Guid
    Private m_clientID As Guid
    Private m_isConnected As Boolean
    Private m_totalBytesSent As Long
    Private m_persistSettings As Boolean
    Private m_settingsCategoryName As String

    Private m_connectTime As Long
    Private m_disconnectTime As Long
    Private m_connectionWaitHandle As ManualResetEvent

#End Region

#Region " Event Declaration "

    ''' <summary>
    ''' Occurs when the client is trying to connect to the server.
    ''' </summary>
    <Description("Occurs when the client is trying to connect to the server."), Category("Connection")> _
    Public Event Connecting(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommunicationClient.Connecting

    ''' <summary>
    ''' Occurs when connecting of the client to the server has been cancelled.
    ''' </summary>
    <Description("Occurs when connecting of the client to the server has been cancelled."), Category("Connection")> _
    Public Event ConnectingCancelled(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommunicationClient.ConnectingCancelled

    ''' <summary>
    ''' Occurs when an exception is encountered while connecting to the server.
    ''' </summary>
    <Description("Occurs when an exception occurs while connecting to the server."), Category("Connection")> _
    Public Event ConnectingException(ByVal sender As Object, ByVal e As GenericEventArgs(Of Exception)) Implements ICommunicationClient.ConnectingException

    ''' <summary>
    ''' Occurs when the client has successfully connected to the server.
    ''' </summary>
    <Description("Occurs when the client has successfully connected to the server."), Category("Connection")> _
    Public Event Connected(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommunicationClient.Connected

    ''' <summary>
    ''' Occurs when the client has disconnected from the server.
    ''' </summary>
    <Description("Occurs when the client has disconnected from the server."), Category("Connection")> _
    Public Event Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommunicationClient.Disconnected

    ''' <summary>
    ''' Occurs when the client begins sending data to the server.
    ''' </summary>
    <Description("Occurs when the client begins sending data to the server."), Category("Data")> _
    Public Event SendDataBegin(ByVal sender As Object, ByVal e As GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))) Implements ICommunicationClient.SendDataBegin

    ''' <summary>
    ''' Occurs when the client has successfully send data to the server.
    ''' </summary>
    <Description("Occurs when the client has successfully send data to the server."), Category("Data")> _
    Public Event SendDataComplete(ByVal sender As Object, ByVal e As GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))) Implements ICommunicationClient.SendDataComplete

    ''' <summary>
    ''' Occurs when the client receives data from the server.
    ''' </summary>
    <Description("Occurs when the client receives data from the server."), Category("Data")> _
    Public Event ReceivedData(ByVal sender As Object, ByVal e As GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))) Implements ICommunicationClient.ReceivedData

    ''' <summary>
    ''' Occurs when no data is received from the server after waiting for the specified time.
    ''' </summary>
    <Description("Occurs when no data is received from the server after waiting for the specified time."), Category("Data")> _
    Public Event ReceiveTimedOut(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommunicationClient.ReceiveTimedOut

#End Region

#Region " Code Scope: Public "

    ''' <summary>
    ''' The maximum number of bytes that can be sent from the client to server in a single send operation.
    ''' </summary>
    Public Const MaximumDataSize As Integer = 524288000  ' 500 MB

    Public Sub New(ByVal connectionString As String)

        MyClass.New()
        m_connectionString = connectionString

    End Sub

    ''' <summary>
    ''' Gets or sets the data required by the client to connect to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The data required by the client to connect to the server.</returns>
    <Description("The data required by the client to connect to the server."), Category("Configuration")> _
    Public Overridable Property ConnectionString() As String Implements ICommunicationClient.ConnectionString
        Get
            Return m_connectionString
        End Get
        Set(ByVal value As String)
            If ValidConnectionString(value) Then
                m_connectionString = value
                If IsConnected() Then
                    ' Reconnect the client when connection data is changed.
                    Disconnect()
                    Connect()
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the maximum number of times the client will attempt to connect to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The maximum number of times the client will attempt to connect to the server.</returns>
    ''' <remarks>Set MaximumConnectionAttempts = -1 for infinite connection attempts.</remarks>
    <Description("The maximum number of times the client will attempt to connect to the server. Set MaximumConnectionAttempts = -1 for infinite connection attempts."), Category("Configuration"), DefaultValue(GetType(Integer), "-1")> _
    Public Overridable Property MaximumConnectionAttempts() As Integer Implements ICommunicationClient.MaximumConnectionAttempts
        Get
            Return m_maximumConnectionAttempts
        End Get
        Set(ByVal value As Integer)
            If value = -1 OrElse value > 0 Then
                m_maximumConnectionAttempts = value
            Else
                Throw New ArgumentOutOfRangeException("value")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the data exchanged between the server and clients
    ''' will be encrypted using a private session passphrase.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' True if the data exchanged between the server and clients will be encrypted using a private session 
    ''' passphrase; otherwise False.
    ''' </returns>
    '''<remarks>Handshake and Encryption must be enabled in order to use SecureSession.</remarks>
    <Description("Indicates whether the data exchanged between the server and clients will be encrypted using a private session passphrase."), Category("Security"), DefaultValue(GetType(Boolean), "False")> _
    Public Overridable Property SecureSession() As Boolean Implements ICommunicationClient.SecureSession
        Get
            Return m_secureSession
        End Get
        Set(ByVal value As Boolean)
            If (Not value) OrElse _
                    (value AndAlso m_handshake AndAlso m_encryption <> Security.Cryptography.EncryptLevel.None) Then
                m_secureSession = value
            Else
                Throw New InvalidOperationException("Handshake and Encryption must be enabled in order to use SecureSession.")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the server will do a handshake with the client after 
    ''' accepting its connection.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the server will do a handshake with the client; otherwise False.</returns>
    ''' <remarks>SecureSession must be disabled before disabling Handshake.</remarks>
    <Description("Indicates whether the server will do a handshake with the client after accepting its connection."), Category("Security"), DefaultValue(GetType(Boolean), "True")> _
    Public Overridable Property Handshake() As Boolean Implements ICommunicationClient.Handshake
        Get
            Return m_handshake
        End Get
        Set(ByVal value As Boolean)
            If (value) OrElse (Not value AndAlso Not m_secureSession) Then
                m_handshake = value
                If Not m_handshake Then
                    ' Handshake passphrase will have no effect if handshaking is disabled.
                    m_handshakePassphrase = ""
                End If
            Else
                Throw New ArgumentException("SecureSession must be disabled before disabling Handshake.")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the passpharse that will be provided to the server for authentication during the handshake 
    ''' process.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The passpharse that will provided to the server for authentication during the handshake process.</returns>
    <Description("The passpharse that will provided to the server for authentication during the handshake process."), Category("Security"), DefaultValue(GetType(String), "")> _
    Public Overridable Property HandshakePassphrase() As String Implements ICommunicationClient.HandshakePassphrase
        Get
            Return m_handshakePassphrase
        End Get
        Set(ByVal value As String)
            m_handshakePassphrase = value
            If Not String.IsNullOrEmpty(m_handshakePassphrase) Then
                ' Handshake password has no effect until handshaking is enabled.
                m_handshake = True
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the maximum number of bytes that can be received at a time by the client from the server.
    ''' </summary>
    ''' <value>Receive buffer size</value>
    ''' <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while client is connected</exception>
    ''' <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
    ''' <returns>The maximum number of bytes that can be received at a time by the client from the server.</returns>
    <Description("The maximum number of bytes that can be received at a time by the client from the server."), Category("Data"), DefaultValue(GetType(Integer), "8192")> _
    Public Overridable Property ReceiveBufferSize() As Integer Implements ICommunicationClient.ReceiveBufferSize
        Get
            Return m_receiveBufferSize
        End Get
        Set(ByVal value As Integer)
            If value > 0 Then
                If m_isConnected Then Throw New InvalidOperationException("Cannot change receive buffer size while client is connected")
                m_receiveBufferSize = value
                m_buffer = CreateArray(Of Byte)(value)
            Else
                Throw New ArgumentOutOfRangeException("value")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the time to wait in milliseconds for data to be received from the server before timing out.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The time to wait in milliseconds for data to be received from the server before timing out.</returns>
    ''' <remarks>Set ReceiveTimeout = -1 to disable timeout for receiving data.</remarks>
    <Description("The time to wait in milliseconds for data to be received from the server before timing out. Set ReceiveTimeout = -1 to disable timeout for receiving data."), Category("Data"), DefaultValue(GetType(Integer), "-1")> _
    Public Overridable Property ReceiveTimeout() As Integer Implements ICommunicationClient.ReceiveTimeout
        Get
            Return m_receiveTimeout
        End Get
        Set(ByVal value As Integer)
            If value = -1 OrElse value > 0 Then
                m_receiveTimeout = value
            Else
                Throw New ArgumentOutOfRangeException("value")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the encryption level to be used for encrypting the data exchanged between the client and 
    ''' server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encryption level to be used for encrypting the data exchanged between the client and server.</returns>
    ''' <remarks>
    ''' <para>Set Encryption = None to disable encryption.</para>
    ''' <para>
    ''' The key used for performing cryptography will be different in different senarios.
    ''' 1) HandshakePassphrase will be used as the key when HandshakePassphrase is set and SecureSession is
    '''    disabled.
    ''' 2) DefaultCryptoKey will be used as the key when HandshakePassphrase is not set and SecureSession is
    '''    disabled.
    ''' 3) A private session key will be used as the key when SecureSession is enabled.
    ''' </para>
    ''' </remarks>
    <Description("The encryption level to be used for encrypting the data exchanged between the client and server."), Category("Data"), DefaultValue(GetType(TVA.Security.Cryptography.EncryptLevel), "None")> _
    Public Overridable Property Encryption() As TVA.Security.Cryptography.EncryptLevel Implements ICommunicationClient.Encryption
        Get
            Return m_encryption
        End Get
        Set(ByVal value As TVA.Security.Cryptography.EncryptLevel)
            If (Not m_secureSession) OrElse _
                    (m_secureSession AndAlso value <> Security.Cryptography.EncryptLevel.None) Then
                m_encryption = value
            Else
                Throw New ArgumentException("SecureSession session must be disabled before disabling Encryption.")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the compression level to be used for compressing the data exchanged between the client and 
    ''' server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The compression level to be used for compressing the data exchanged between the client and server.</returns>
    ''' <remarks>Set Compression = NoCompression to disable compression.</remarks>
    <Description("The compression level to be used for compressing the data exchanged between the client and server."), Category("Data"), DefaultValue(GetType(TVA.IO.Compression.CompressLevel), "NoCompression")> _
    Public Overridable Property Compression() As TVA.IO.Compression.CompressLevel Implements ICommunicationClient.Compression
        Get
            Return m_compression
        End Get
        Set(ByVal value As TVA.IO.Compression.CompressLevel)
            m_compression = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the client is enabled.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the client is enabled; otherwise False.</returns>
    <Description("Indicates whether the client is enabled."), Category("Behavior"), DefaultValue(GetType(Boolean), "True")> _
    Public Overridable Property Enabled() As Boolean Implements ICommunicationClient.Enabled
        Get
            Return m_enabled
        End Get
        Set(ByVal value As Boolean)
            m_enabled = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the encoding to be used for the text sent to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encoding to be used for the text sent to the server.</returns>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Overridable Property TextEncoding() As Encoding Implements ICommunicationClient.TextEncoding
        Get
            Return m_textEncoding
        End Get
        Set(ByVal value As Encoding)
            m_textEncoding = value
        End Set
    End Property

    ''' <summary>
    ''' Setting this property allows consumer to "intercept" data before it goes through normal processing
    ''' </summary>
    ''' <remarks>
    ''' This property only needs to be implemented if you need data from the server absolutelty as fast as possible, for most uses this
    ''' will not be necessary.  Setting this property gives the consumer access to the data stream as soon as it's available, but this also
    ''' bypasses all of the advanced convience properties (e.g., PayloadAware, Handshake, Encryption, Compression, etc.)
    ''' </remarks>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Overridable Property ReceiveRawDataFunction() As ReceiveRawDataFunctionSignature Implements ICommunicationClient.ReceiveRawDataFunction
        Get
            Return m_receiveRawDataFunction
        End Get
        Set(ByVal value As ReceiveRawDataFunctionSignature)
            m_receiveRawDataFunction = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the protocol used by the client for transferring data to and from the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The protocol used by the client for transferring data to and from the server.</returns>
    <Browsable(False)> _
    Public Overridable Property Protocol() As TransportProtocol Implements ICommunicationClient.Protocol
        Get
            Return m_protocol
        End Get
        Protected Set(ByVal value As TransportProtocol)
            m_protocol = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the ID of the server to which the client is connected.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ID of the server to which the client is connected.</returns>
    <Browsable(False)> _
    Public Overridable Property ServerID() As Guid Implements ICommunicationClient.ServerID
        Get
            Return m_serverID
        End Get
        Protected Set(ByVal value As Guid)
            m_serverID = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the ID of the client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ID of the client.</returns>
    <Browsable(False)> _
    Public Overridable Property ClientID() As Guid Implements ICommunicationClient.ClientID
        Get
            Return m_clientID
        End Get
        Protected Set(ByVal value As Guid)
            m_clientID = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the current instance of communication client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current instance communication client.</returns>
    <Browsable(False)> _
    Public ReadOnly Property This() As ICommunicationClient Implements ICommunicationClient.This
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Gets a boolean value indicating whether the client is currently connected to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the client is connected; otherwise False.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property IsConnected() As Boolean Implements ICommunicationClient.IsConnected
        Get
            Return m_isConnected
        End Get
    End Property

    ''' <summary>
    ''' Gets the time in seconds for which the client has been connected to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The time in seconds for which the client has been connected to the server.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property ConnectionTime() As Double Implements ICommunicationClient.ConnectionTime
        Get
            Dim clientConnectionTime As Double
            If m_connectTime > 0 Then
                If m_isConnected Then   ' Client is connected to the server.
                    clientConnectionTime = TicksToSeconds(System.DateTime.Now.Ticks() - m_connectTime)
                Else    ' Client is not connected to the server.
                    clientConnectionTime = TicksToSeconds(m_disconnectTime - m_connectTime)
                End If
            End If
            Return clientConnectionTime
        End Get
    End Property

    ''' <summary>
    ''' Gets the total number of bytes sent by the client to the server since the connection is established.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The total number of bytes sent by the client to the server since the connection is established.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property TotalBytesSent() As Long Implements ICommunicationClient.TotalBytesSent
        Get
            Return m_totalBytesSent
        End Get
    End Property

    ''' <summary>
    ''' Gets the total number of bytes received by the client from the server since the connection is established.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The total number of bytes received by the client from the server since the connection is established.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property TotalBytesReceived() As Long Implements ICommunicationClient.TotalBytesReceived
        Get
            Return m_totalBytesReceived
        End Get
    End Property

    ''' <summary>
    ''' Connects the client to the server.
    ''' </summary>
    Public MustOverride Sub Connect() Implements ICommunicationClient.Connect

    ''' <summary>
    ''' Cancels connecting to the server.
    ''' </summary>
    Public MustOverride Sub CancelConnect() Implements ICommunicationClient.CancelConnect

    ''' <summary>
    ''' Disconnects the client from the connected server
    ''' </summary>
    Public MustOverride Sub Disconnect() Implements ICommunicationClient.Disconnect

    ''' <summary>
    ''' Sends data to the server.
    ''' </summary>
    ''' <param name="data">The plain-text data that is to be sent to the server.</param>
    Public Overridable Sub Send(ByVal data As String) Implements ICommunicationClient.Send

        Send(m_textEncoding.GetBytes(data))

    End Sub

    ''' <summary>
    ''' Sends data to the server.
    ''' </summary>
    ''' <param name="serializableObject">The serializable object that is to be sent to the server.</param>
    Public Overridable Sub Send(ByVal serializableObject As Object) Implements ICommunicationClient.Send

        Send(GetBytes(serializableObject))

    End Sub

    ''' <summary>
    ''' Sends data to the server.
    ''' </summary>
    ''' <param name="data">The binary data that is to be sent to the server.</param>
    Public Overridable Sub Send(ByVal data As Byte()) Implements ICommunicationClient.Send

        Send(data, 0, data.Length())

    End Sub

    ''' <summary>
    ''' Sends the specified subset of data from the data buffer to the server.
    ''' </summary>
    ''' <param name="data">The buffer that contains the binary data to be sent.</param>
    ''' <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
    ''' <param name="size">The number of bytes to be sent.</param>
    Public Overridable Sub Send(ByVal data As Byte(), ByVal offset As Integer, ByVal size As Integer) Implements ICommunicationClient.Send

        If m_enabled AndAlso m_isConnected Then
            If data Is Nothing Then Throw New ArgumentNullException("data")
            If size > 0 Then
                Dim dataToSend As Byte() = GetPreparedData(CopyBuffer(data, offset, size))
                If dataToSend.Length() <= MaximumDataSize Then
                    ' PCP - 05/24/2007 : Reverting to synchronous send to avoid out-of-sequence transmissions.
                    SendPreparedData(dataToSend)

                    ' JRC: Removed reflective thread invocation and changed to thread pool for speed...
                    '   TVA.Threading.RunThread.ExecuteNonPublicMethod(Me, "SendPreparedData", dataToSend)

                    ' Begin sending data on a seperate thread.
                    'ThreadPool.QueueUserWorkItem(AddressOf SendPreparedData, dataToSend)
                Else
                    ' Prepared data is too large to be sent.
                    Throw New ArgumentException("Size of the data to be sent exceeds the maximum data size of " & MaximumDataSize & " bytes.")
                End If
            Else
                Throw New ArgumentNullException("data")
            End If
        End If

    End Sub

    ''' <summary>
    ''' Waits for the client to connect to the server for the specified time and optionally stop the client from
    ''' retrying connection attempts if the client is unable to connect to the server within the specified time.
    ''' </summary>
    ''' <param name="waitTime">
    ''' The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1  
    ''' will cause this method to wait indefinately until the client establishes connection with the server.
    ''' </param>
    ''' <returns>True if the connection was successful; otherwise False.</returns>
    Public Function WaitForConnection(ByVal waitTime As Integer) As Boolean Implements ICommunicationClient.WaitForConnection

        Return WaitForConnection(waitTime, True)

    End Function

    ''' <summary>
    ''' Waits for the client to connect to the server for the specified time and optionally stop the client from
    ''' retrying connection attempts if the client is unable to connect to the server within the specified time.
    ''' </summary>
    ''' <param name="waitTime">
    ''' The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1  
    ''' will cause this method to wait indefinately until the client establishes connection with the server.
    ''' </param>
    ''' <param name="stopRetrying">
    ''' Boolean value indicating whether the client should stop trying to connect to the server if it is unable to 
    ''' connect to the server after waiting for the specified duration.
    ''' </param>
    ''' <returns>True if the connection was successful; otherwise False.</returns>
    Public Function WaitForConnection(ByVal waitTime As Integer, ByVal stopRetrying As Boolean) As Boolean Implements ICommunicationClient.WaitForConnection

        If Not m_isConnected Then
            ' We'll wait until client has connected or until the time to wait for connection is reached.
            Dim connected As Boolean = m_connectionWaitHandle.WaitOne(waitTime, False)

            ' If the client hasn't connected after waiting for the specified time and if it is specified to stop 
            ' attempting to connect to the server, then we'll call the CancelConnect() method.
            If stopRetrying AndAlso Not connected Then CancelConnect()

            Return connected
        Else
            Return True
        End If

    End Function

#Region " Interface Implementation"

#Region " IServiceComponent "

    Private m_previouslyEnabled As Boolean = False

    <Browsable(False)> _
    Public Overridable ReadOnly Property Name() As String Implements Services.IServiceComponent.Name
        Get
            Return Me.GetType().Name
        End Get
    End Property

    ''' <summary>
    ''' Gets the current status of the client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current status of the client.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property Status() As String Implements Services.IServiceComponent.Status
        Get
            With New StringBuilder()
                .Append("                 Server ID: ")
                .Append(m_serverID.ToString())
                .AppendLine()
                .Append("                 Client ID: ")
                .Append(m_clientID.ToString())
                .AppendLine()
                .Append("              Client state: ")
                .Append(IIf(m_isConnected, "Connected", "Not Connected"))
                .AppendLine()
                .Append("           Connection time: ")
                .Append(SecondsToText(ConnectionTime()))
                .AppendLine()
                .Append("            Receive buffer: ")
                .Append(m_receiveBufferSize.ToString())
                .AppendLine()
                .Append("        Transport protocol: ")
                .Append(m_protocol.ToString())
                .AppendLine()
                .Append("        Text encoding used: ")
                .Append(m_textEncoding.EncodingName())
                .AppendLine()
                .Append("          Total bytes sent: ")
                .Append(m_totalBytesSent)
                .AppendLine()
                .Append("      Total bytes received: ")
                .Append(m_totalBytesReceived)
                .AppendLine()

                Return .ToString()
            End With
        End Get
    End Property

    Public Overridable Sub ProcessStateChanged(ByVal processName As String, ByVal newState As Services.ProcessState) Implements Services.IServiceComponent.ProcessStateChanged

    End Sub

    Public Overridable Sub ServiceStateChanged(ByVal newState As Services.ServiceState) Implements Services.IServiceComponent.ServiceStateChanged

        Select Case newState
            Case ServiceState.Started
                Me.Connect()
            Case ServiceState.Stopped, ServiceState.Shutdown
                Me.Disconnect()
            Case ServiceState.Paused
                m_previouslyEnabled = Me.Enabled
                Me.Enabled = False
            Case ServiceState.Resumed
                Me.Enabled = m_previouslyEnabled
            Case ServiceState.Shutdown
                Me.Dispose()
        End Select

    End Sub

#End Region

#Region " IPersistSettings "

    <Category("Settings")> _
    Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
        Get
            Return m_persistSettings
        End Get
        Set(ByVal value As Boolean)
            m_persistSettings = value
        End Set
    End Property

    <Category("Settings")> _
    Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
        Get
            Return m_settingsCategoryName
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_settingsCategoryName = value
            Else
                Throw New ArgumentNullException("SettingsCategoryName")
            End If
        End Set
    End Property

    Public Overridable Sub LoadSettings() Implements IPersistSettings.LoadSettings

        Try
            With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                If .Count > 0 Then
                    ConnectionString = .Item("ConnectionString").GetTypedValue(m_connectionString)
                    ReceiveBufferSize = .Item("ReceiveBufferSize").GetTypedValue(m_receiveBufferSize)
                    ReceiveTimeout = .Item("ReceiveTimeout").GetTypedValue(m_receiveTimeout)
                    MaximumConnectionAttempts = .Item("MaximumConnectionAttempts").GetTypedValue(m_maximumConnectionAttempts)
                    SecureSession = .Item("SecureSession").GetTypedValue(m_secureSession)
                    Handshake = .Item("Handshake").GetTypedValue(m_handshake)
                    HandshakePassphrase = .Item("HandshakePassphrase").GetTypedValue(m_handshakePassphrase)
                    Encryption = .Item("Encryption").GetTypedValue(m_encryption)
                    Compression = .Item("Compression").GetTypedValue(m_compression)
                    Enabled = .Item("Enabled").GetTypedValue(m_enabled)
                End If
            End With
        Catch ex As Exception
            ' We'll encounter exceptions if the settings are not present in the config file.
        End Try

    End Sub

    Public Overridable Sub SaveSettings() Implements IPersistSettings.SaveSettings

        If m_persistSettings Then
            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    .Clear()
                    With .Item("ConnectionString", True)
                        .Value = m_connectionString
                        .Description = "Data required by the client to connect to the server."
                    End With
                    With .Item("ReceiveBufferSize", True)
                        .Value = m_receiveBufferSize.ToString()
                        .Description = "Maximum number of bytes that can be received at a time by the client from the server."
                    End With
                    With .Item("ReceiveTimeout", True)
                        .Value = m_receiveTimeout.ToString()
                        .Description = "Time to wait in milliseconds for data to be received from the server before timing out."
                    End With
                    With .Item("MaximumConnectionAttempts", True)
                        .Value = m_maximumConnectionAttempts.ToString()
                        .Description = "Maximum number of times the client will attempt to connect to the server."
                    End With
                    With .Item("SecureSession", True)
                        .Value = m_secureSession.ToString()
                        .Description = "True if the data exchanged between the client and server will be encrypted using a private session passphrase; otherwise False."
                    End With
                    With .Item("Handshake", True)
                        .Value = m_handshake.ToString()
                        .Description = "True if the client will do a handshake with the server; otherwise False."
                    End With
                    With .Item("HandshakePassphrase", True)
                        .Value = m_handshakePassphrase
                        .Description = "Passpharse that will provided to the server for authentication during the handshake process."
                    End With
                    With .Item("Encryption", True)
                        .Value = m_encryption.ToString()
                        .Description = "Encryption level (None; Level1; Level2; Level3; Level4) to be used for encrypting the data exchanged between the client and server."
                    End With
                    With .Item("Compression", True)
                        .Value = m_compression.ToString()
                        .Description = "Compression level (NoCompression; DefaultCompression; BestSpeed; BestCompression; MultiPass) to be used for compressing the data exchanged between the client and server."
                    End With
                    With .Item("Enabled", True)
                        .Value = m_enabled.ToString()
                        .Description = "True if the client is enabled; otherwise False."
                    End With
                End With
                TVA.Configuration.Common.SaveSettings()
            Catch ex As Exception
                ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
            End Try
        End If

    End Sub

#End Region

#Region " ISupportInitialize "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            LoadSettings()  ' Load settings from the config file.
        End If

    End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Protected "

    ' We expose these two members to derived classes for their own internal use
    Protected m_totalBytesReceived As Long
    Protected m_receiveRawDataFunction As ReceiveRawDataFunctionSignature
    Protected m_buffer As Byte()

    ''' <summary>
    ''' The key used for encryption and decryption when Encryption is enabled but HandshakePassphrase is not set.
    ''' </summary>
    Protected Const DefaultCryptoKey As String = "6572a33d-826f-4d96-8c28-8be66bbc700e"

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.Connecting event.
    ''' </summary>
    ''' <param name="e">A System.EventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when the client is attempting connection to the server.</remarks>
    Protected Overridable Sub OnConnecting(ByVal e As EventArgs)

        m_connectionWaitHandle.Reset()
        RaiseEvent Connecting(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.ConnectingCancelled event.
    ''' </summary>
    ''' <param name="e">A System.EventArgs that contains the event data.</param>
    ''' <remarks>
    ''' This method is to be called when attempts for connecting the client to the server are stopped on user's
    ''' request (i.e. When CancelConnect() is called before client is connected to the server).
    ''' </remarks>
    Protected Overridable Sub OnConnectingCancelled(ByVal e As EventArgs)

        RaiseEvent ConnectingCancelled(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.ConnectingException event.
    ''' </summary>
    ''' <param name="e">A TVA.ExceptionEventArgs that contains the event data.</param>
    ''' <remarks>
    ''' This method is to be called when all attempts for connecting to the server have been made but failed 
    ''' due to exceptions.
    ''' </remarks>
    Protected Overridable Sub OnConnectingException(ByVal e As Exception)

        RaiseEvent ConnectingException(Me, New GenericEventArgs(Of Exception)(e))

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.Connected event.
    ''' </summary>
    ''' <param name="e">A System.EventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when the client has successfully connected to the server.</remarks>
    Protected Overridable Sub OnConnected(ByVal e As EventArgs)

        m_isConnected = True
        m_connectTime = System.DateTime.Now.Ticks  ' Save the time when the client connected to the server.
        m_disconnectTime = 0
        m_totalBytesSent = 0    ' Reset the number of bytes sent and received between the client and server.
        m_totalBytesReceived = 0
        m_connectionWaitHandle.Set()
        RaiseEvent Connected(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.Disconnected event.
    ''' </summary>
    ''' <param name="e">A System.EventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when the client has disconnected from the server.</remarks>
    Protected Overridable Sub OnDisconnected(ByVal e As EventArgs)

        m_serverID = Guid.Empty
        m_isConnected = False
        m_disconnectTime = System.DateTime.Now.Ticks() ' Save the time when client was disconnected from the server.
        RaiseEvent Disconnected(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.SendBegin event.
    ''' </summary>
    ''' <param name="e">A TVA.DataEventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when the client begins sending data to the server.</remarks>
    Protected Overridable Sub OnSendDataBegin(ByVal e As IdentifiableItem(Of Guid, Byte()))

        RaiseEvent SendDataBegin(Me, New GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))(e))

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.SendComplete event.
    ''' </summary>
    ''' <param name="e">A TVA.DataEventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when the client has finished sending data to the server.</remarks>
    Protected Overridable Sub OnSendDataComplete(ByVal e As IdentifiableItem(Of Guid, Byte()))

        m_totalBytesSent += e.Item.Length()
        RaiseEvent SendDataComplete(Me, New GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))(e))

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.ReceivedData event.
    ''' </summary>
    ''' <param name="e">A TVA.DataEventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when the client receives data from the server.</remarks>
    Protected Overridable Sub OnReceivedData(ByVal e As IdentifiableItem(Of Guid, Byte()))

        m_totalBytesReceived += e.Item.Length()

        Try
            e.Item = GetActualData(e.Item)
        Catch ex As Exception
            ' We'll just pass on the data that we received.
        End Try
        RaiseEvent ReceivedData(Me, New GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))(e))

    End Sub

    ''' <summary>
    ''' Raises the TVA.Communication.ClientBase.ReceiveTimedOut event.
    ''' </summary>
    ''' <param name="e">A System.EventArgs that contains the event data.</param>
    ''' <remarks>
    ''' This method is to be called when no data is received from the server after waiting for the specified time.
    ''' </remarks>
    Protected Overridable Sub OnReceiveTimedOut(ByVal e As EventArgs)

        RaiseEvent ReceiveTimedOut(Me, e)

    End Sub

    ''' <summary>
    ''' Performs the necessary compression and encryption on the specified data and returns it.
    ''' </summary>
    ''' <param name="data">The data on which compression and encryption is to be performed.</param>
    ''' <returns>Compressed and encrypted data.</returns>
    ''' <remarks>No encryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
    Protected Overridable Function GetPreparedData(ByVal data As Byte()) As Byte()

        data = CompressData(data, m_compression)
        If Not m_secureSession Then
            Dim key As String = m_handshakePassphrase
            If String.IsNullOrEmpty(key) Then
                key = DefaultCryptoKey
            End If
            data = EncryptData(data, key, m_encryption)
        End If
        Return data

    End Function

    ''' <summary>
    ''' Performs the necessary uncompression and decryption on the specified data and returns it.
    ''' </summary>
    ''' <param name="data">The data on which uncompression and decryption is to be performed.</param>
    ''' <returns>Uncompressed and decrypted data.</returns>
    ''' <remarks>No decryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
    Protected Overridable Function GetActualData(ByVal data As Byte()) As Byte()

        If Not m_secureSession Then
            Dim key As String = m_handshakePassphrase
            If String.IsNullOrEmpty(key) Then
                key = DefaultCryptoKey
            End If
            data = DecryptData(data, key, m_encryption)
        End If

        data = UncompressData(data, m_compression)

        Return data

    End Function

    ''' <summary>
    ''' Sends prepared data to the server.
    ''' </summary>
    ''' <param name="data">The prepared data that is to be sent to the server.</param>
    Protected MustOverride Sub SendPreparedData(ByVal data As Byte())

    ''' <summary>
    ''' Determines whether specified connection string required for the client to connect to the server is valid.
    ''' </summary>
    ''' <param name="connectionString">The connection string to be validated.</param>
    ''' <returns>True is the connection string is valid; otherwise False.</returns>
    Protected MustOverride Function ValidConnectionString(ByVal connectionString As String) As Boolean

#End Region

#Region " Code Scope: Private "

    '''' <summary>
    '''' This function proxies data to proper derived class function from thread pool.
    '''' </summary>
    '''' <param name="state"></param>
    'Private Sub SendPreparedData(ByVal state As Object)

    '    Try
    '        SendPreparedData(DirectCast(state, Byte()))
    '    Catch
    '        ' We can safely ignore errors here
    '    End Try

    'End Sub

#End Region

End Class