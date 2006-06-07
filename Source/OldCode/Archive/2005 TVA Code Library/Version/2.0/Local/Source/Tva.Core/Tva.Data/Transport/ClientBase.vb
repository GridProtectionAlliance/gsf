' 06-01-06

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.ComponentModel

Namespace Data.Transport

    Public MustInherit Class ClientBase

        Private m_connectionString As String
        Private m_receiveBufferSize As Integer
        Private m_maximumConnectionAttempts As Integer
        Private m_textEncoding As Encoding
        Private m_protocol As TransportProtocol
        Private m_enabled As Boolean
        Private m_serverID As String
        Private m_clientID As String
        Private m_isConnected As Boolean
        Private m_connectTime As Long
        Private m_disconnectTime As Long
        Private m_totalBytesSent As Integer
        Private m_totalBytesReceived As Integer

        ''' <summary>
        ''' Occurs when the client is trying to connect to the server.
        ''' </summary>
        <Description("Occurs when the client is trying to connect to the server.")> _
        Public Event Connecting As EventHandler

        ''' <summary>
        ''' Occurs when the client has successfully connected to the server.
        ''' </summary>
        <Description("Occurs when the client has successfully connected to the server.")> _
        Public Event Connected As EventHandler

        ''' <summary>
        ''' Occurs when the client has disconnected from the server.
        ''' </summary>
        <Description("Occurs when the client has disconnected from the server.")> _
        Public Event Disconnected As EventHandler

        ''' <summary>
        ''' Occurs when the client begins sending data to the server.
        ''' </summary>
        ''' <param name="byteCount">Number of bytes the client will be sending to the server.</param>
        <Description("Occurs when the client begins sending data to the server.")> _
        Public Event SendBegin(ByVal byteCount As Integer)

        ''' <summary>
        ''' Occurs when the client has successfully send data to the server.
        ''' </summary>
        ''' <param name="byteCount">Number of bytes the client has sent to the server.</param>
        <Description("Occurs when the client has successfully send data to the server.")> _
        Public Event SendComplete(ByVal byteCount As Integer)

        ''' <summary>
        ''' Occurs when the client receives data from the server.
        ''' </summary>
        ''' <param name="data">The data that was received from the server.</param>
        <Description("Occurs when the client receives data from the server.")> _
        Public Event ReceivedData(ByVal data() As Byte)

        ''' <summary>
        ''' Gets or sets the data required by the client to connect to the server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The data required by the client to connect to the server.</returns>
        <Description("The data required by the client to connect to the server."), Category("Configuration")> _
        Public Property ConnectionString() As String
            Get
                Return m_connectionString
            End Get
            Set(ByVal value As String)
                If ValidConnectionString(value) Then
                    m_connectionString = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the maximum number of bytes that can be received at a time by the client from the server.
        ''' </summary>
        ''' <value>The maximum number of bytes that can be received at a time by the client from the server.</value>
        ''' <returns></returns>
        <Description("The maximum number of bytes that can be received at a time by the client from the server."), Category("Configuration"), DefaultValue(GetType(Integer), "4096")> _
        Public Property ReceiveBufferSize() As Integer
            Get
                Return m_receiveBufferSize
            End Get
            Set(ByVal value As Integer)
                If value > 0 Then
                    m_receiveBufferSize = value
                Else
                    Throw New ArgumentOutOfRangeException("value")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the maximum number of times the client will attempt to connect to the server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The maximum number of times the client will attempt to connect to the server.</returns>
        ''' <remarks>Set MaximumConnectionAttempts = 0 for infinite connection attempts.</remarks>
        <Description("The maximum number of times the client will attempt to connect to the server. Set MaximumConnectionAttempts = 0 for infinite connection attempts."), Category("Configuration"), DefaultValue(GetType(Integer), "0")> _
        Public Property MaximumConnectionAttempts() As Integer
            Get
                Return m_maximumConnectionAttempts
            End Get
            Set(ByVal value As Integer)
                If value >= 0 Then
                    m_maximumConnectionAttempts = value
                Else
                    Throw New ArgumentOutOfRangeException("value")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the encoding to be used for the text sent to the server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The encoding to be used for the text sent to the server.</returns>
        <Browsable(False)> _
        Public Property TextEncoding() As Encoding
            Get
                Return m_textEncoding
            End Get
            Set(ByVal value As Encoding)
                m_textEncoding = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the protocol used by the client for transferring data to and from the server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The protocol used by the client for transferring data to and from the server.</returns>
        <Browsable(False)> _
        Public Property Protocol() As TransportProtocol
            Get
                Return m_protocol
            End Get
            Protected Set(ByVal value As TransportProtocol)
                m_protocol = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value to indicate whether the client is enabled.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the client is enabled; otherwise False.</returns>
        <Description("Indicates whether the client is enabled."), Category("Configuration"), DefaultValue(GetType(Boolean), "True")> _
        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal value As Boolean)
                m_enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the ID of the server to which the client is connected.
        ''' </summary>
        ''' <value></value>
        ''' <returns>ID of the server to which the client is connected.</returns>
        <Browsable(False)> _
        Public ReadOnly Property ServerID() As String
            Get
                Return m_serverID
            End Get
        End Property

        ''' <summary>
        ''' Gets the client's ID.
        ''' </summary>
        ''' <value></value>
        ''' <returns>ID of the client.</returns>
        <Browsable(False)> _
        Public ReadOnly Property ClientID() As String
            Get
                Return m_clientID
            End Get
        End Property

        ''' <summary>
        ''' Gets whether the client is currently connected to the server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the client is connected; otherwise False.</returns>
        <Browsable(False)> _
        Public ReadOnly Property IsConnected() As Boolean
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
        Public ReadOnly Property ConnectionTime() As Double
            Get
                Dim clientConnectionTime As Double = 0
                If m_connectTime > 0 Then
                    If m_isConnected Then   ' Client is connected to the server.
                        clientConnectionTime = (Date.Now.Ticks() - m_connectTime) / 10000000L
                    Else    ' Client is not connected to the server.
                        clientConnectionTime = (m_disconnectTime - m_connectTime) / 10000000L
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
        Public ReadOnly Property TotalBytesSent() As Integer
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
        Public ReadOnly Property TotalBytesReceived() As Integer
            Get
                Return m_totalBytesReceived
            End Get
        End Property

        ''' <summary>
        ''' Gets the current status of the client.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current status of the client.</returns>
        <Browsable(False)> _
        Public ReadOnly Property Status() As String
            Get
                With New StringBuilder()
                    Return .ToString()
                End With
            End Get
        End Property

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ClientBase.Connecting event.
        ''' </summary>
        ''' <param name="e">An System.EventArgs that contains the event data.</param>
        ''' <remarks>This method is to be called when the client is attempting connection to the server.</remarks>
        Public Sub OnConnecting(ByVal e As EventArgs)

            RaiseEvent Connecting(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ClientBase.Connected event.
        ''' </summary>
        ''' <param name="e">An System.EventArgs that contains the event data.</param>
        ''' <remarks>This method is to be called when the client has successfully connected to the server.</remarks>
        Public Sub OnConnected(ByVal e As EventArgs)

            m_isConnected = True
            m_connectTime = Date.Now.Ticks
            m_disconnectTime = 0
            m_totalBytesSent = 0
            m_totalBytesReceived = 0
            RaiseEvent Connected(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ClientBase.Disconnected event.
        ''' </summary>
        ''' <param name="e">An System.EventArgs that contains the event data.</param>
        ''' <remarks>This method is to be called when the client has disconnected from the server.</remarks>
        Public Sub OnDisconnected(ByVal e As EventArgs)

            m_isConnected = False
            m_disconnectTime = Date.Now.Ticks()
            RaiseEvent Disconnected(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ClientBase.SendBegin event.
        ''' </summary>
        ''' <param name="byteCount">Number of bytes the client will be sending to the server.</param>
        ''' <remarks>This method is to be called when the client begins sending data to the server.</remarks>
        Public Sub OnSendBegin(ByVal byteCount As Integer)

            RaiseEvent SendBegin(byteCount)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ClientBase.SendComplete event.
        ''' </summary>
        ''' <param name="byteCount">Number of bytes the client has sent to the server.</param>
        ''' <remarks>This method is to be called when the client has finished sending data to the server.</remarks>
        Public Sub OnSendComplete(ByVal byteCount As Integer)

            m_totalBytesSent += byteCount
            RaiseEvent SendComplete(byteCount)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ClientBase.ReceivedData event.
        ''' </summary>
        ''' <param name="data">The data that was received from the server.</param>
        ''' <remarks>This method is to be called when the client receives data from the server.</remarks>
        Public Sub OnReceivedData(ByVal data() As Byte)

            m_totalBytesReceived += data.Length()
            RaiseEvent ReceivedData(data)

        End Sub

        ''' <summary>
        ''' Connects the client to the server.
        ''' </summary>
        Public MustOverride Sub Connect()

        ''' <summary>
        ''' Disconnects the client from the server it is connected to.
        ''' </summary>
        Public MustOverride Sub Disconnect()

        ''' <summary>
        ''' Sends data to the server.
        ''' </summary>
        ''' <param name="data">The data that is to be sent to the server.</param>
        Public MustOverride Sub Send(ByVal data() As Byte)

        ''' <summary>
        ''' Determines whether specified connection string, required for the client to connect to the server, 
        ''' is valid.
        ''' </summary>
        ''' <param name="connectionString">The connection string to be validated.</param>
        ''' <returns>True is the connection string is valid; otherwise False.</returns>
        Public MustOverride Function ValidConnectionString(ByVal connectionString As String) As Boolean

    End Class

End Namespace