'*******************************************************************************************************
'  Tva.Data.Transport.ServerBase.vb - Base functionality of a server for transporting data
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
'
'*******************************************************************************************************

Option Strict On

Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.Common
Imports Tva.Serialization
Imports Tva.DateTime.Common

Namespace Data.Transport

    <ToolboxBitmap(GetType(ServerBase))> _
    Public MustInherit Class ServerBase

        Private m_configurationString As String
        Private m_receiveBufferSize As Integer
        Private m_maximumClients As Integer
        Private m_handshake As Boolean
        Private m_enabled As Boolean
        Private m_textEncoding As Encoding
        Private m_protocol As TransportProtocol
        Private m_serverID As Guid
        Private m_clientIDs As List(Of Guid)
        Private m_isRunning As Boolean
        Private m_startTime As Long
        Private m_stopTime As Long

        ''' <summary>
        ''' Occurs when the server is started.
        ''' </summary>
        <Description("Occurs when the server is started.")> _
        Public Event ServerStarted As EventHandler

        ''' <summary>
        ''' Occurs when the server is stopped.
        ''' </summary>
        <Description("Occurs when the server is stopped.")> _
        Public Event ServerStopped As EventHandler

        ''' <summary>
        ''' Occurs when a client is connected to the server.
        ''' </summary>
        ''' <param name="clientID">ID of the client that was connected.</param>
        <Description("Occurs when a client is connected to the server.")> _
        Public Event ClientConnected(ByVal clientID As Guid)

        ''' <summary>
        ''' Occurs when a client is disconnected from the server.
        ''' </summary>
        ''' <param name="clientID">ID of the client that was disconnected.</param>
        <Description("Occurs when a client is disconnected from the server.")> _
        Public Event ClientDisconnected(ByVal clientID As Guid)

        ''' <summary>
        ''' Occurs when data is received from a client.
        ''' </summary>
        ''' <param name="clientID">ID of the client from which the data is received.</param>
        ''' <param name="data">The data that was received from the client.</param>
        <Description("Occurs when data is received from a client.")> _
        Public Event ReceivedClientData(ByVal clientID As Guid, ByVal data() As Byte)

        ''' <summary>
        ''' Gets or sets the data that is required by the server to initialize.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The data that is required by the server to initialize.</returns>
        <Description("The data that is required by the server to initialize."), Category("Configuration")> _
        Public Property ConfigurationString() As String
            Get
                Return m_configurationString
            End Get
            Set(ByVal value As String)
                If ValidConfigurationString(value) Then
                    m_configurationString = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
        ''' </summary>
        ''' <value>The maximum number of bytes that can be received at a time by the server from the clients.</value>
        ''' <returns></returns>
        <Description("The maximum number of bytes that can be received at a time by the server from the clients."), Category("Configuration"), DefaultValue(GetType(Integer), "4096")> _
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
        ''' Gets or sets the maximum number of clients that can connect to the server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The maximum number of clients that can connect to the server.</returns>
        ''' <remarks>
        ''' Set MaximumClients = -1 for infinite client connections.
        ''' </remarks>
        <Description("The maximum number of clients that can connect to the server. Set MaximumClients = -1 for infinite client connections."), Category("Configuration"), DefaultValue(GetType(Integer), "-1")> _
        Public Property MaximumClients() As Integer
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
        ''' Gets or sets a boolean value indication whether the server will do a handshake with the client after 
        ''' accepting its connection.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True is the server will do a handshake with the client; otherwise False.</returns>
        <Description("Indicates whether the server will do a handshake with the client after accepting its connection."), Category("Configuration"), DefaultValue(GetType(Boolean), "True")> _
        Public Property Handshake() As Boolean
            Get
                Return m_handshake
            End Get
            Set(ByVal value As Boolean)
                m_handshake = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the server is enabled.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the server is enabled; otherwise False.</returns>
        <Description("Indicates whether the server is enabled."), Category("Configuration"), DefaultValue(GetType(Boolean), "True")> _
        Public Property Enabled() As Boolean
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
        ''' Gets the protocol used by the server for transferring data to and from the clients.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The protocol used by the server for transferring data to and from the clients.</returns>
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
        ''' Gets the server's ID.
        ''' </summary>
        ''' <value></value>
        ''' <returns>ID of the server.</returns>
        <Browsable(False)> _
        Public ReadOnly Property ServerID() As Guid
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
        Public ReadOnly Property ClientIDs() As List(Of Guid)
            Get
                Return m_clientIDs
            End Get
        End Property

        ''' <summary>
        ''' Gets whether the server is currently running.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the server is running; otherwise False.</returns>
        <Browsable(False)> _
        Public ReadOnly Property IsRunning() As Boolean
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
        Public ReadOnly Property RunTime() As Double
            Get
                Dim serverRunTime As Double = 0
                If m_startTime > 0 Then
                    If m_isRunning Then ' Server is running.
                        serverRunTime = (Date.Now.Ticks() - m_startTime) / 10000000L
                    Else    ' Server is not running.
                        serverRunTime = (m_stopTime - m_startTime) / 10000000L
                    End If
                End If
                Return serverRunTime
            End Get
        End Property

        ''' <summary>
        ''' Gets the current status of the server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current status of the server.</returns>
        <Browsable(False)> _
        Public ReadOnly Property Status() As String
            Get
                With New StringBuilder()
                    .Append("         Server ID: " & ServerID.ToString())
                    .Append(Environment.NewLine())
                    .Append("      Server state: " & IIf(IsRunning(), "Running", "Not Running"))
                    .Append(Environment.NewLine())
                    .Append("    Server runtime: " & SecondsToText(RunTime()))
                    .Append(Environment.NewLine())
                    .Append("Subscribed clients: " & ClientIDs.Count())
                    .Append(Environment.NewLine())
                    .Append("   Maximum clients: " & IIf(MaximumClients() = -1, "Infinite", MaximumClients.ToString()))
                    .Append(Environment.NewLine())
                    .Append("    Receive buffer: " & ReceiveBufferSize.ToString())
                    .Append(Environment.NewLine())
                    .Append("Transport protocol: " & Protocol.ToString())
                    .Append(Environment.NewLine())
                    .Append("Text encoding used: " & TextEncoding.EncodingName())
                    .Append(Environment.NewLine())
                    Return .ToString()
                End With
            End Get
        End Property

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ServerStarted event.
        ''' </summary>
        ''' <param name="e">An System.EventArgs that contains the event data.</param>
        ''' <remarks>This method is to be called after the server has been started.</remarks>
        Public Overridable Sub OnServerStarted(ByVal e As EventArgs)

            m_isRunning = True
            m_startTime = Date.Now.Ticks()  ' Save the time when server is started.
            m_stopTime = 0
            RaiseEvent ServerStarted(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ServerStopped event.
        ''' </summary>
        ''' <param name="e">An System.EventArgs that contains the event data.</param>
        ''' <remarks>This method is to be called after the server has been stopped.</remarks>
        Public Overridable Sub OnServerStopped(ByVal e As EventArgs)

            m_isRunning = False
            m_stopTime = Date.Now.Ticks()   ' Save the time when server is stopped.
            RaiseEvent ServerStopped(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ClientConnected event.
        ''' </summary>
        ''' <param name="clientID">ID of the client that was connected.</param>
        ''' <remarks>This method is to be called when a client is connected to the server.</remarks>
        Public Overridable Sub OnClientConnected(ByVal clientID As Guid)

            m_clientIDs.Add(clientID)
            RaiseEvent ClientConnected(clientID)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ClientDisconnected event.
        ''' </summary>
        ''' <param name="clientID">ID of the client that was disconnected.</param>
        ''' <remarks>This method is to be called when a client has disconnected from the server.</remarks>
        Public Overridable Sub OnClientDisconnected(ByVal clientID As Guid)

            m_clientIDs.Remove(clientID)
            RaiseEvent ClientDisconnected(clientID)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ReceivedClientData event.
        ''' </summary>
        ''' <param name="clientID">ID of the client from which the data is received.</param>
        ''' <param name="data">The data that was received from the client.</param>
        ''' <remarks>This method is to be called when the server receives data from a client.</remarks>
        Public Overridable Sub OnReceivedClientData(ByVal clientID As Guid, ByVal data() As Byte)

            RaiseEvent ReceivedClientData(clientID, data)

        End Sub

        ''' <summary>
        ''' Starts the server.
        ''' </summary>
        Public MustOverride Sub Start()

        ''' <summary>
        ''' Stops the server.
        ''' </summary>
        Public MustOverride Sub [Stop]()

        ''' <summary>
        ''' Sends data to the specified client.
        ''' </summary>
        ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
        ''' <param name="data">The data that is to be sent to the client.</param>
        Public Sub SendTo(ByVal clientID As Guid, ByVal data As String)

            SendTo(clientID, m_textEncoding.GetBytes(data))

        End Sub

        ''' <summary>
        ''' Sends data to the specified client.
        ''' </summary>
        ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
        ''' <param name="serializableObject">The serializable object that is to be sent to the client.</param>
        Public Sub SendTo(ByVal clientID As Guid, ByVal serializableObject As Object)

            SendTo(clientID, GetBytes(serializableObject))

        End Sub

        ''' <summary>
        ''' Sends data to the specified client.
        ''' </summary>
        ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
        ''' <param name="data">The data that is to be sent to the client.</param>
        Public MustOverride Sub SendTo(ByVal clientID As Guid, ByVal data() As Byte)

        ''' <summary>
        ''' Sends data to all of the clients.
        ''' </summary>
        ''' <param name="data">The data that is to sent to the clients.</param>
        Public Sub Broadcast(ByVal data As String)

            Broadcast(m_textEncoding.GetBytes(data))

        End Sub

        ''' <summary>
        ''' Sends data to all of the clients.
        ''' </summary>
        ''' <param name="data">The data that is to sent to the clients.</param>
        Public Sub Broadcast(ByVal data() As Byte)

            If Enabled() AndAlso IsRunning() Then
                For Each clientID As Guid In m_clientIDs
                    SendTo(clientID, data)
                Next
            End If

        End Sub

        ''' <summary>
        ''' Determines whether specified configuration string, required for the server to initialize, is valid.
        ''' </summary>
        ''' <param name="configurationString">The configuration string to be validated.</param>
        ''' <returns>True is the configuration string is valid; otherwise False.</returns>
        Protected MustOverride Function ValidConfigurationString(ByVal configurationString As String) As Boolean

    End Class

End Namespace
