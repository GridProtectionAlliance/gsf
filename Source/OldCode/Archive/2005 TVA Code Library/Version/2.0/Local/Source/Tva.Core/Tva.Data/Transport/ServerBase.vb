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

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.ComponentModel

Namespace Data.Transport

    Public MustInherit Class ServerBase

        Private m_configurationString As String
        Private m_readBufferSize As Integer
        Private m_maximumClients As Integer
        Private m_enabled As Boolean
        Private m_textEncoding As Encoding
        Private m_protocol As TransportProtocol
        Private m_serverID As String
        Private m_clientIDs As List(Of String)
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
        Public Event ClientConnected(ByVal clientID As String)

        ''' <summary>
        ''' Occurs when a client is disconnected from the server.
        ''' </summary>
        ''' <param name="clientID">ID of the client that was disconnected.</param>
        <Description("Occurs when a client is disconnected from the server.")> _
        Public Event ClientDisconnected(ByVal clientID As String)

        ''' <summary>
        ''' Occurs when data is received from a client.
        ''' </summary>
        ''' <param name="clientID">ID of the client from which the data is received.</param>
        ''' <param name="data">The data that was received from the client.</param>
        <Description("Occurs when data is received from a client.")> _
        Public Event ReceivedClientData(ByVal clientID As String, ByVal data() As Byte)

        ''' <summary>
        ''' Occurs when sending data to a client fails.
        ''' </summary>
        ''' <param name="ex">The exception that was encountered when sending data.</param>
        <Description("Occurs when sending data to a client fails.")> _
        Public Event SendFailed(ByVal ex As Exception)

        ''' <summary>
        ''' Occurs when broadcasting data to all connected clients fails.
        ''' </summary>
        ''' <param name="ex">The exception that was encountered when broadcasting data.</param>
        <Description("Occurs when broadcasting data to all connected clients fails.")> _
        Public Event BroadcastFailed(ByVal ex As Exception)

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
        ''' Gets or sets the maximum number of bytes that can be read by the server from clients and buffered.
        ''' </summary>
        ''' <value>The maximum number of bytes that can be read and buffered by the server from clients.</value>
        ''' <returns></returns>
        <Description("The maximum number of bytes that can be read and buffered by the server from clients."), Category("Configuration"), DefaultValue(GetType(Integer), "4096")> _
        Public Property ReadBufferSize() As Integer
            Get
                Return m_readBufferSize
            End Get
            Set(ByVal value As Integer)
                If value > 0 Then
                    m_readBufferSize = value
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
        ''' Set MaximumClients to 0 to allow infinite number clients to be connected to the server.
        ''' </remarks>
        <Description("The maximum number of clients that can connect to the server."), Category("Configuration"), DefaultValue(GetType(Integer), "0")> _
        Public Property MaximumClients() As Integer
            Get
                Return m_maximumClients
            End Get
            Set(ByVal value As Integer)
                If value >= 0 Then
                    m_maximumClients = value
                Else
                    Throw New ArgumentOutOfRangeException("value")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value to indicate whether the server is enabled.
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
        ''' Gets the protocol used by the server for transferring data to and from the client.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The protocol used by the server for transferring data to and from the client.</returns>
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
        Public ReadOnly Property ServerID() As String
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
        Public ReadOnly Property ClientIDs() As List(Of String)
            Get
                Return m_clientIDs
            End Get
        End Property

        ''' <summary>
        ''' Gets the server run time in seconds.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The server run time in seconds.</returns>
        <Browsable(False)> _
        Public ReadOnly Property RunTime() As Double
            Get
                Dim serverRunTime As Double = 0
                If m_startTime > 0 Then
                    If IsRunning() Then
                        serverRunTime = (Date.Now.Ticks() - m_startTime) / 10000000L
                    Else
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
                    Return .ToString()
                End With
            End Get
        End Property

        ''' <summary>
        ''' Gets whether the server is currently running.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the server is running; otherwise False.</returns>
        <Browsable(False)> _
        Public MustOverride ReadOnly Property IsRunning() As Boolean

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ServerStarted event.
        ''' </summary>
        ''' <param name="e">An System.EventArgs that contains the event data.</param>
        Public Overridable Sub OnServerStarted(ByVal e As EventArgs)

            m_startTime = Date.Now.Ticks()  ' Save the time when server is started.
            m_stopTime = 0
            RaiseEvent ServerStarted(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ServerStopped event.
        ''' </summary>
        ''' <param name="e">An System.EventArgs that contains the event data.</param>
        Public Overridable Sub OnServerStopped(ByVal e As EventArgs)

            m_stopTime = Date.Now.Ticks()   ' Save the time when server is stopped.
            RaiseEvent ServerStopped(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ClientConnected event.
        ''' </summary>
        ''' <param name="clientID">ID of the client that was connected.</param>
        Public Overridable Sub OnClientConnected(ByVal clientID As String)

            m_clientIDs.Add(clientID)
            RaiseEvent ClientConnected(clientID)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ClientDisconnected event.
        ''' </summary>
        ''' <param name="clientID">ID of the client that was disconnected.</param>
        Public Overridable Sub OnClientDisconnected(ByVal clientID As String)

            m_clientIDs.Remove(clientID)
            RaiseEvent ClientDisconnected(clientID)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.ReceivedClientData event.
        ''' </summary>
        ''' <param name="clientID">ID of the client from which the data is received.</param>
        ''' <param name="data">The data that was received from the client.</param>
        Public Overridable Sub OnReceivedClientData(ByVal clientID As String, ByVal data() As Byte)

            RaiseEvent ReceivedClientData(clientID, data)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.SendFailed event.
        ''' </summary>
        ''' <param name="ex">The exception that was encountered when sending data.</param>
        Public Overridable Sub OnSendFailed(ByVal ex As Exception)

            RaiseEvent SendFailed(ex)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Data.Transport.ServerBase.BroadcastFailed event.
        ''' </summary>
        ''' <param name="ex">The exception that was encountered when broadcasting data.</param>
        Public Overridable Sub OnBroadcastFailed(ByVal ex As Exception)

            RaiseEvent BroadcastFailed(ex)

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
        Public Sub SendTo(ByVal clientID As String, ByVal data As String)

            SendTo(clientID, m_textEncoding.GetBytes(data))

        End Sub

        ''' <summary>
        ''' Sends data to the specified client.
        ''' </summary>
        ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
        ''' <param name="data">The data that is to be sent to the client.</param>
        Public MustOverride Sub SendTo(ByVal clientID As String, ByVal data() As Byte)

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
                Try
                    For Each clientID As String In m_clientIDs
                        SendTo(clientID, data)
                    Next
                Catch ex As Exception
                    OnBroadcastFailed(ex)
                End Try
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
