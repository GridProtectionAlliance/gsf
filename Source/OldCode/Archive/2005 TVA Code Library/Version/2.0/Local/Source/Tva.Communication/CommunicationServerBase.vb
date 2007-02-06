'*******************************************************************************************************
'  Tva.Communication.ServerBase.vb - Base functionality of a server for transporting data
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
'       Added bypass optimizations for high-speed server data access
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.Common
Imports Tva.Serialization
Imports Tva.Services
Imports Tva.IO.Common
Imports Tva.DateTime.Common
Imports Tva.Communication.CommunicationHelper
Imports Tva.Communication.Common

''' <summary>
''' Represents a server involved in the transportation of data.
''' </summary>
<ToolboxBitmap(GetType(CommunicationServerBase)), DefaultEvent("ReceivedClientData")> _
Public MustInherit Class CommunicationServerBase
    Implements ICommunicationServer

    Private m_configurationString As String
    Private m_receiveBufferSize As Integer
    Private m_maximumClients As Integer
    Private m_secureSession As Boolean
    Private m_handshake As Boolean
    Private m_handshakePassphrase As String
    Private m_encryption As Tva.Security.Cryptography.EncryptLevel
    Private m_compression As Tva.IO.Compression.CompressLevel
    Private m_crcCheck As CRCCheckType
    Private m_enabled As Boolean
    Private m_textEncoding As Encoding
    Private m_protocol As TransportProtocol
    Private m_serverID As Guid
    Private m_clientIDs As List(Of Guid)
    Private m_isRunning As Boolean
    Private m_startTime As Long
    Private m_stopTime As Long

    ' We expose these two members to derived classes for their own internal use
    Protected m_receiveRawDataFunction As ReceiveRawDataFunctionSignature
    Protected m_buffer As Byte()

    ''' <summary>
    ''' The maximum number of bytes that can be sent from the server to clients in a single send operation.
    ''' </summary>
    Public Const MaximumDataSize As Integer = 524288000  ' 500 MB

    ''' <summary>
    ''' Occurs when the server is started.
    ''' </summary>
    <Description("Occurs when the server is started."), Category("Server")> _
    Public Event ServerStarted(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommunicationServer.ServerStarted

    ''' <summary>
    ''' Occurs when the server is stopped.
    ''' </summary>
    <Description("Occurs when the server is stopped."), Category("Server")> _
    Public Event ServerStopped(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommunicationServer.ServerStopped

    ''' <summary>
    ''' Occurs when an exception is encountered while starting up the server.
    ''' </summary>
    <Description("Occurs when an exception is encountered while starting up the server."), Category("Server")> _
    Public Event ServerStartupException(ByVal sender As Object, ByVal e As ExceptionEventArgs) Implements ICommunicationServer.ServerStartupException

    ''' <summary>
    ''' Occurs when a client is connected to the server.
    ''' </summary>
    <Description("Occurs when a client is connected to the server."), Category("Client")> _
    Public Event ClientConnected(ByVal sender As Object, ByVal e As IdentifiableSourceEventArgs) Implements ICommunicationServer.ClientConnected

    ''' <summary>
    ''' Occurs when a client is disconnected from the server.
    ''' </summary>
    <Description("Occurs when a client is disconnected from the server."), Category("Client")> _
    Public Event ClientDisconnected(ByVal sender As Object, ByVal e As IdentifiableSourceEventArgs) Implements ICommunicationServer.ClientDisconnected

    ''' <summary>
    ''' Occurs when data is received from a client.
    ''' </summary>
    <Description("Occurs when data is received from a client."), Category("Data")> _
    Public Event ReceivedClientData(ByVal sender As Object, ByVal e As DataEventArgs) Implements ICommunicationServer.ReceivedClientData

    ''' <summary>
    ''' Gets or sets the data that is required by the server to initialize.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The data that is required by the server to initialize.</returns>
    <Description("The data that is required by the server to initialize."), Category("Configuration")> _
    Public Overridable Property ConfigurationString() As String Implements ICommunicationServer.ConfigurationString
        Get
            Return m_configurationString
        End Get
        Set(ByVal value As String)
            If ValidConfigurationString(value) Then
                m_configurationString = value
                If IsRunning() Then
                    ' Restart the server when configuration data is changed.
                    [Stop]()
                    Start()
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the maximum number of clients that can connect to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The maximum number of clients that can connect to the server.</returns>
    ''' <remarks>Set MaximumClients = -1 for infinite client connections.</remarks>
    <Description("The maximum number of clients that can connect to the server. Set MaximumClients = -1 for infinite client connections."), Category("Configuration"), DefaultValue(GetType(Integer), "-1")> _
    Public Overridable Property MaximumClients() As Integer Implements ICommunicationServer.MaximumClients
        Get
            Return m_maximumClients
        End Get
        Set(ByVal value As Integer)
            If value = -1 OrElse value > 0 Then
                m_maximumClients = value
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
    Public Overridable Property SecureSession() As Boolean Implements ICommunicationServer.SecureSession
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
    Public Overridable Property Handshake() As Boolean Implements ICommunicationServer.Handshake
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
    ''' Gets or sets the passpharse that the clients must provide for authentication during the handshake process.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The passpharse that the clients must provide for authentication during the handshake process.</returns>
    <Description("The passpharse that the clients must provide for authentication during the handshake process."), Category("Security"), DefaultValue(GetType(String), "")> _
    Public Overridable Property HandshakePassphrase() As String Implements ICommunicationServer.HandshakePassphrase
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
    ''' Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
    ''' </summary>
    ''' <value>Receive buffer size</value>
    ''' <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while server is running</exception>
    ''' <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
    ''' <returns>The maximum number of bytes that can be received at a time by the server from the clients.</returns>
    <Description("The maximum number of bytes that can be received at a time by the server from the clients."), Category("Data"), DefaultValue(GetType(Integer), "8192")> _
    Public Overridable Property ReceiveBufferSize() As Integer Implements ICommunicationServer.ReceiveBufferSize
        Get
            Return m_receiveBufferSize
        End Get
        Set(ByVal value As Integer)
            If m_isRunning Then Throw New InvalidOperationException("Cannot change receive buffer size while server is running")
            If value > 0 Then
                m_receiveBufferSize = value
                m_buffer = CreateArray(Of Byte)(value)
            Else
                Throw New ArgumentOutOfRangeException("value")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the encryption level to be used for encrypting the data exchanged between the server and 
    ''' clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encryption level to be used for encrypting the data exchanged between the server and clients.</returns>
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
    <Description("The encryption level to be used for encrypting the data exchanged between the server and clients."), Category("Data"), DefaultValue(GetType(Tva.Security.Cryptography.EncryptLevel), "None")> _
    Public Overridable Property Encryption() As Tva.Security.Cryptography.EncryptLevel Implements ICommunicationServer.Encryption
        Get
            Return m_encryption
        End Get
        Set(ByVal value As Tva.Security.Cryptography.EncryptLevel)
            If (Not m_secureSession) OrElse _
                    (m_secureSession AndAlso value <> Security.Cryptography.EncryptLevel.None) Then
                m_encryption = value
            Else
                Throw New ArgumentException("SecureSession session must be disabled before disabling Encryption.")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the compression level to be used for compressing the data exchanged between the server and 
    ''' clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The compression level to be used for compressing the data exchanged between the server and clients.</returns>
    ''' <remarks>Set Compression = NoCompression to disable compression.</remarks>
    <Description("The compression level to be used for compressing the data exchanged between the server and clients."), Category("Data"), DefaultValue(GetType(Tva.IO.Compression.CompressLevel), "NoCompression")> _
    Public Overridable Property Compression() As Tva.IO.Compression.CompressLevel Implements ICommunicationServer.Compression
        Get
            Return m_compression
        End Get
        Set(ByVal value As Tva.IO.Compression.CompressLevel)
            m_compression = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the server is enabled.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the server is enabled; otherwise False.</returns>
    <Description("Indicates whether the server is enabled."), Category("Behavior"), DefaultValue(GetType(Boolean), "True")> _
    Public Overridable Property Enabled() As Boolean Implements ICommunicationServer.Enabled
        Get
            Return m_enabled
        End Get
        Set(ByVal value As Boolean)
            m_enabled = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the encoding to be used for the text sent to the connected clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encoding to be used for the text sent to the connected clients.</returns>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Overridable Property TextEncoding() As Encoding Implements ICommunicationServer.TextEncoding
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
    ''' This property only needs to be implemented if you need data from the clients absolutelty as fast as possible, for most uses this
    ''' will not be necessary.  Setting this property gives the consumer access to the data stream as soon as it's available, but this also
    ''' bypasses all of the advanced convience properties (e.g., PayloadAware, Handshake, Encryption, Compression, etc.)
    ''' </remarks>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Overridable Property ReceiveRawDataFunction() As ReceiveRawDataFunctionSignature Implements ICommunicationServer.ReceiveRawDataFunction
        Get
            Return m_receiveRawDataFunction
        End Get
        Set(ByVal value As ReceiveRawDataFunctionSignature)
            m_receiveRawDataFunction = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the protocol used by the server for transferring data to and from the clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The protocol used by the server for transferring data to and from the clients.</returns>
    <Browsable(False)> _
    Public Overridable Property Protocol() As TransportProtocol Implements ICommunicationServer.Protocol
        Get
            Return m_protocol
        End Get
        Protected Set(ByVal value As TransportProtocol)
            m_protocol = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the current instance of communication server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current instance communication server.</returns>
    <Browsable(False)> _
    Public ReadOnly Property This() As ICommunicationServer Implements ICommunicationServer.This
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Gets the server's ID.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ID of the server.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property ServerID() As Guid Implements ICommunicationServer.ServerID
        Get
            Return m_serverID
        End Get
    End Property

    ''' <summary>
    ''' Gets a collection of client IDs that are connected to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>A collection of client IDs that are connected to the server.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property ClientIDs() As List(Of Guid) Implements ICommunicationServer.ClientIDs
        Get
            Return m_clientIDs
        End Get
    End Property

    ''' <summary>
    ''' Gets a boolean value indicating whether the server is currently running.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the server is running; otherwise False.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property IsRunning() As Boolean Implements ICommunicationServer.IsRunning
        Get
            Return m_isRunning
        End Get
    End Property

    ''' <summary>
    ''' Gets the time in seconds for which the server has been running.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The time in seconds for which the server has been running.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property RunTime() As Double Implements ICommunicationServer.RunTime
        Get
            Dim serverRunTime As Double
            If m_startTime > 0 Then
                If m_isRunning Then ' Server is running.
                    serverRunTime = TicksToSeconds(System.DateTime.Now.Ticks() - m_startTime)
                Else    ' Server is not running.
                    serverRunTime = TicksToSeconds(m_stopTime - m_startTime)
                End If
            End If
            Return serverRunTime
        End Get
    End Property

    ''' <summary>
    ''' Starts the server.
    ''' </summary>
    Public MustOverride Sub Start() Implements ICommunicationServer.Start

    ''' <summary>
    ''' Stops the server.
    ''' </summary>
    Public MustOverride Sub [Stop]() Implements ICommunicationServer.Stop

    ''' <summary>
    ''' Sends data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The plain-text data that is to be sent to the client.</param>
    Public Overridable Sub SendTo(ByVal clientID As Guid, ByVal data As String) Implements ICommunicationServer.SendTo

        SendTo(clientID, m_textEncoding.GetBytes(data))

    End Sub

    ''' <summary>
    ''' Sends data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="serializableObject">The serializable object that is to be sent to the client.</param>
    Public Overridable Sub SendTo(ByVal clientID As Guid, ByVal serializableObject As Object) Implements ICommunicationServer.SendTo

        SendTo(clientID, GetBytes(serializableObject))

    End Sub

    ''' <summary>
    ''' Sends data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The binary data that is to be sent to the client.</param>
    Public Overridable Sub SendTo(ByVal clientID As Guid, ByVal data As Byte()) Implements ICommunicationServer.SendTo

        SendTo(clientID, data, 0, data.Length())

    End Sub

    ''' <summary>
    ''' Sends the specified subset of data from the data buffer to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The buffer that contains the binary data to be sent.</param>
    ''' <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
    ''' <param name="size">The number of bytes to be sent.</param>
    Public Overridable Sub SendTo(ByVal clientID As System.Guid, ByVal data As Byte(), ByVal offset As Integer, ByVal size As Integer) Implements ICommunicationServer.SendTo

        If m_enabled AndAlso m_isRunning Then
            If data Is Nothing Then Throw New ArgumentNullException("data")
            If size > 0 Then
                Dim dataToSend As Byte() = GetPreparedData(CopyBuffer(data, offset, size))
                If dataToSend.Length() <= MaximumDataSize Then
                    'SendPreparedDataTo(clientID, dataToSend)
                    ' JRC: Removed reflective thread invocation and changed to thread pool for speed...
                    '   Tva.Threading.RunThread.ExecuteNonPublicMethod(Me, "SendPreparedDataTo", clientID, dataToSend)

                    ' Begin sending data on a seperate thread.
                    ThreadPool.QueueUserWorkItem(AddressOf SendPreparedDataTo, New Object() {clientID, dataToSend})
                Else
                    ' Prepared data is too large to be sent.
                    Throw New ArgumentException("Size of the data to be sent exceeds the maximum data size of " & MaximumDataSize & " bytes.")
                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' Sends data to all of the subscribed clients.
    ''' </summary>
    ''' <param name="data">The plain-text data that is to sent to the subscribed clients.</param>
    Public Overridable Sub Multicast(ByVal data As String) Implements ICommunicationServer.Multicast

        Multicast(m_textEncoding.GetBytes(data))

    End Sub

    ''' <summary>
    ''' Sends data to all of the subscribed clients.
    ''' </summary>
    ''' <param name="serializableObject">The serializable object that is to be sent to the subscribed clients.</param>
    Public Overridable Sub Multicast(ByVal serializableObject As Object) Implements ICommunicationServer.Multicast

        Multicast(GetBytes(serializableObject))

    End Sub

    ''' <summary>
    ''' Sends data to all of the subscribed clients.
    ''' </summary>
    ''' <param name="data">The binary data that is to sent to the subscribed clients.</param>
    Public Overridable Sub Multicast(ByVal data As Byte()) Implements ICommunicationServer.Multicast

        Multicast(data, 0, data.Length())

    End Sub

    ''' <summary>
    ''' Sends the specified subset of data from the data buffer to all of the subscribed clients.
    ''' </summary>
    ''' <param name="data">The buffer that contains the binary data to be sent.</param>
    ''' <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
    ''' <param name="size">The number of bytes to be sent.</param>
    Public Overridable Sub Multicast(ByVal data As Byte(), ByVal offset As Integer, ByVal size As Integer) Implements ICommunicationServer.Multicast

        If m_enabled AndAlso m_isRunning Then
            If data Is Nothing Then Throw New ArgumentNullException("data")
            If size > 0 Then
                Dim dataToSend As Byte() = GetPreparedData(CopyBuffer(data, offset, size))
                If dataToSend.Length() <= MaximumDataSize Then
                    For Each clientID As Guid In m_clientIDs
                        'SendPreparedDataTo(clientID, dataToSend)
                        ' JRC: Removed reflective thread invocation and changed to thread pool for speed...
                        '   Tva.Threading.RunThread.ExecuteNonPublicMethod(Me, "SendPreparedDataTo", clientID, dataToSend)

                        ' Begin sending data on a seperate thread.
                        ThreadPool.QueueUserWorkItem(AddressOf SendPreparedDataTo, New Object() {clientID, dataToSend})
                    Next
                Else
                    ' Prepared data is too large to be sent.
                    Throw New ArgumentException("Size of the data to be sent exceeds the maximum data size of " & MaximumDataSize & " bytes.")
                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' The key used for encryption and decryption when Encryption is enabled but HandshakePassphrase is not set.
    ''' </summary>
    Protected Const DefaultCryptoKey As String = "6572a33d-826f-4d96-8c28-8be66bbc700e"

    ''' <summary>
    ''' Raises the Tva.Communication.ServerBase.ServerStarted event.
    ''' </summary>
    ''' <param name="e">A System.EventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called after the server has been started.</remarks>
    Protected Overridable Sub OnServerStarted(ByVal e As EventArgs)

        m_isRunning = True
        m_startTime = System.DateTime.Now.Ticks()  ' Save the time when server is started.
        m_stopTime = 0
        RaiseEvent ServerStarted(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the Tva.Communication.ServerBase.ServerStopped event.
    ''' </summary>
    ''' <param name="e">A System.EventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called after the server has been stopped.</remarks>
    Protected Overridable Sub OnServerStopped(ByVal e As EventArgs)

        m_isRunning = False
        m_stopTime = System.DateTime.Now.Ticks()   ' Save the time when server is stopped.
        RaiseEvent ServerStopped(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the Tva.Communication.ServerBase.ServerStartupException event.
    ''' </summary>
    ''' <param name="e">A Tva.ExceptionEventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called if the server throws an exception during startup.</remarks>
    Protected Overridable Sub OnServerStartupException(ByVal e As ExceptionEventArgs)

        RaiseEvent ServerStartupException(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the Tva.Communication.ServerBase.ClientConnected event.
    ''' </summary>
    ''' <param name="e">A Tva.IdentifiableSourceEventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when a client is connected to the server.</remarks>
    Protected Overridable Sub OnClientConnected(ByVal e As IdentifiableSourceEventArgs)

        m_clientIDs.Add(e.Source)
        RaiseEvent ClientConnected(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the Tva.Communication.ServerBase.ClientDisconnected event.
    ''' </summary>
    ''' <param name="e">A Tva.IdentifiableSourceEventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when a client has disconnected from the server.</remarks>
    Protected Overridable Sub OnClientDisconnected(ByVal e As IdentifiableSourceEventArgs)

        m_clientIDs.Remove(e.Source)
        RaiseEvent ClientDisconnected(Me, e)

    End Sub

    ''' <summary>
    ''' Raises the Tva.Communication.ServerBase.ReceivedClientData event.
    ''' </summary>
    ''' <param name="e">A Tva.DataEventArgs that contains the event data.</param>
    ''' <remarks>This method is to be called when the server receives data from a client.</remarks>
    Protected Overridable Sub OnReceivedClientData(ByVal e As DataEventArgs)

        Try
            e.Data = GetActualData(e.Data)
        Catch ex As Exception
            ' We'll just pass on the data that we received.
        End Try
        RaiseEvent ReceivedClientData(Me, e)

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
    ''' Sends prepared data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The prepared data that is to be sent to the client.</param>
    Protected MustOverride Sub SendPreparedDataTo(ByVal clientID As Guid, ByVal data As Byte())

    ''' <summary>
    ''' Determines whether specified configuration string required for the server to initialize is valid.
    ''' </summary>
    ''' <param name="configurationString">The configuration string to be validated.</param>
    ''' <returns>True is the configuration string is valid; otherwise False.</returns>
    Protected MustOverride Function ValidConfigurationString(ByVal configurationString As String) As Boolean

    ''' <summary>
    ''' This function proxies data to proper derived class function from thread pool.
    ''' </summary>
    ''' <param name="state"></param>
    Private Sub SendPreparedDataTo(ByVal state As Object)

        Try
            With DirectCast(state, Object())
                SendPreparedDataTo(DirectCast(.GetValue(0), Guid), DirectCast(.GetValue(1), Byte()))
            End With
        Catch
            ' We can safely ignore errors here
        End Try

    End Sub

#Region " IServiceComponent Implementation "

    Private m_previouslyEnabled As Boolean = False

    <Browsable(False)> _
    Public Overridable ReadOnly Property Name() As String Implements Services.IServiceComponent.Name
        Get
            Return Me.GetType().Name
        End Get
    End Property

    ''' <summary>
    ''' Gets the current status of the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current status of the server.</returns>
    <Browsable(False)> _
    Public Overridable ReadOnly Property Status() As String Implements Services.IServiceComponent.Status
        Get
            With New StringBuilder()
                .Append("                 Server ID: ")
                .Append(m_serverID.ToString())
                .Append(Environment.NewLine)
                .Append("              Server state: ")
                .Append(IIf(m_isRunning, "Running", "Not Running"))
                .Append(Environment.NewLine)
                .Append("            Server runtime: ")
                .Append(SecondsToText(RunTime()))
                .Append(Environment.NewLine)
                .Append("        Subscribed clients: ")
                .Append(m_clientIDs.Count())
                .Append(Environment.NewLine)
                .Append("           Maximum clients: ")
                .Append(IIf(m_maximumClients = -1, "Infinite", m_maximumClients.ToString()))
                .Append(Environment.NewLine)
                .Append("            Receive buffer: ")
                .Append(m_receiveBufferSize.ToString())
                .Append(Environment.NewLine)
                .Append("        Transport protocol: ")
                .Append(m_protocol.ToString())
                .Append(Environment.NewLine)
                .Append("        Text encoding used: ")
                .Append(m_textEncoding.EncodingName())
                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Public Overridable Sub ProcessStateChanged(ByVal processName As String, ByVal newState As Services.ProcessState) Implements Services.IServiceComponent.ProcessStateChanged

    End Sub

    Public Overridable Sub ServiceStateChanged(ByVal newState As Services.ServiceState) Implements Services.IServiceComponent.ServiceStateChanged

        Select Case newState
            Case ServiceState.Started
                Me.Start()
            Case ServiceState.Stopped, ServiceState.Shutdown
                Me.Stop()
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

End Class