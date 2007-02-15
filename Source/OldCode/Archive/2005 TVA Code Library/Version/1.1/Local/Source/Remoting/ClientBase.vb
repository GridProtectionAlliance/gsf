' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.ComponentModel
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Tcp
Imports System.Runtime.Remoting.Services
Imports System.Runtime.Remoting.Messaging
Imports System.Runtime.CompilerServices
Imports TVA.Shared.Common
Imports TVA.Shared.DateTime
Imports TVA.Threading

Namespace Remoting

    ' This class is the base for a client that can connect to an established remoteable server interface
    <Serializable()> _
    Public MustInherit Class ClientBase

        Inherits MarshalByRefObject
        Implements IClient
        Implements IDisposable

        <NonSerialized()> Private RemoteServer As IServer                                   ' Our reference to the single instance of the host's proxy server class
        <NonSerialized()> Private CommunicationsChannel As IChannel                         ' Our client side TCP communications channel
        <NonSerialized()> Private ServerURI As String                                       ' This is the "current" full URI to established remoting server as specified by the server (e.g., tcp://localhost:8091/ServerURI)
        <NonSerialized()> Private BaseServerURI As String                                   ' This is the "base" full URI to established remoting server as specified by the user (e.g., tcp://localhost:8090/ServerURI)
        <NonSerialized()> Private RetryAtCapacity As Boolean                                ' Specifies whether or not to automatically retry client connection when server reports maximum client capacity
        <NonSerialized()> Private ClientTCPPort As Integer                                  ' TCP communications port used by this instance
        <NonSerialized()> Private ClientConnected As Boolean                                ' Client connection state flag
        <NonSerialized()> Private ClientConnectTime As DateTime                             ' Time client was connected to host
        <NonSerialized()> Private ClientNoActivityTestInterval As Integer                   ' Maximum allowed time for with no activity from server before restarting connect cycle
        <NonSerialized()> Private WithEvents ConnectionTimer As Timers.Timer                ' Creates a connection loop - loop terminates with successful connection
        <NonSerialized()> Private WithEvents RestartTimer As Timers.Timer                   ' Retarts connection loop timer after allowing server host ample restart time
        <NonSerialized()> Private WithEvents NoActivityTimer As Timers.Timer                ' After a period of no activity from host, restarts connect cycle
        <NonSerialized()> Private RegisterClient As IClient.ClientRegistrationFunction      ' Function pointer to the client registration function
        <NonSerialized()> Private UnregisterClient As IClient.ClientRegistrationFunction    ' Function pointer to the client unregistration function

        Public Event ServerNotification(ByVal sender As Object, ByVal e As EventArgs) Implements IClient.ServerNotification
        Public Event StatusMessage(ByVal Message As String, ByVal NewLine As Boolean) Implements IClient.StatusMessage
        Public Event AttemptingConnection() Implements IClient.AttemptingConnection
        Public Event ConnectionAttemptFailed(ByVal ex As Exception) Implements IClient.ConnectionAttemptFailed
        Public Event ConnectionEstablished() Implements IClient.ConnectionEstablished
        Public Event ConnectionTerminated() Implements IClient.ConnectionTerminated
        Public Event UserEventHandlerException(ByVal EventName As String, ByVal ex As Exception) Implements IClient.UserEventHandlerException

        Public Sub New()

            ' Define default client registration functions
            RegisterClient = AddressOf DefaultRegisterClient
            UnregisterClient = AddressOf DefaultUnregisterClient

            ConnectionTimer = New Timers.Timer
            RestartTimer = New Timers.Timer
            NoActivityTimer = New Timers.Timer
            RetryAtCapacity = True

            ' Define a connection loop timer to keep trying to connect every 2 seconds
            With ConnectionTimer
                .AutoReset = False
                .Interval = 2000
                .Enabled = False
            End With

            ' Define a restart timer
            With RestartTimer
                .AutoReset = False
                .Interval = 30000
                .Enabled = False
            End With

            ' Define an activity monitor timer, this defaults to running every 10 minutes
            ClientNoActivityTestInterval = 600
            With NoActivityTimer
                .AutoReset = True
                .Interval = 600000
                .Enabled = False
            End With

        End Sub

        Public Sub New(ByVal URI As String, ByVal TCPPort As Integer)

            MyClass.New()
            Me.URI = URI
            Me.TCPPort = TCPPort

        End Sub

        Protected Overrides Sub Finalize()

            Shutdown()

        End Sub

        Public Overridable Sub Shutdown() Implements IClient.Shutdown, IDisposable.Dispose

            On Error Resume Next

            GC.SuppressFinalize(Me)
            Disconnect()
            StopCommunications()

        End Sub

        <MethodImpl(MethodImplOptions.Synchronized)> _
        Public Overridable Sub Connect(Optional ByVal StartDelay As Boolean = False) Implements IClient.Connect

            Disconnect()

            If StartDelay Then
                SendStatusMessage("Waiting " & SecondsToText(RestartDelayInterval).ToLower() & " to allow time for TCP communications to restart properly", True)
                RestartTimer.Enabled = True
            Else
                RestartTimer_Elapsed(Nothing, Nothing)
            End If

        End Sub

        <MethodImpl(MethodImplOptions.Synchronized)> _
        Public Overridable Sub Disconnect() Implements IClient.Disconnect

            If ClientConnected Then
                RaiseEvent_ConnectionTerminated()

                Try
                    If Not RemoteServer Is Nothing Then
                        ' Unregister the event handlers (we do this to play nice)
                        UnregisterClient.Invoke()
                    End If
                Catch
                    ' Keep on going if this fails...
                End Try

                ' We restore base connection URI when we actually disconnect from the server so
                ' that all connection attempts start at the primary server instance allowing the
                ' server to better "balance the client load" if needed...
                ServerURI = BaseServerURI
            End If

            RestartTimer.Enabled = False
            ConnectionTimer.Enabled = False
            RemoteServer = Nothing
            ClientConnected = False

        End Sub

        <Browsable(True), Category("Connection"), Description("Specifies full URI used to connect to an established remoting server (e.g., tcp://localhost:8090/ServerURI).  Server may change this value dynamically to help with load balancing."), DefaultValue("tcp://localhost:8090/ServerURI")> _
        Public Overridable Property URI() As String Implements IClient.URI
            Get
                Return ServerURI
            End Get
            Set(ByVal Value As String)
                ServerURI = Value

                ' This becomes the official base URI as specified by the client user - server can change actual connect URI
                ' to help balance its load at any time, but any future reconnects will start with this URI
                BaseServerURI = Value
            End Set
        End Property

        <Browsable(False)> _
        Public Overridable ReadOnly Property BaseURI() As String Implements IClient.BaseURI
            Get
                Return BaseServerURI
            End Get
        End Property

        <Browsable(True), Category("Connection"), Description("This setting allows you to set a specific outgoing TCP communications port used by this client instance.  When this value is zero, the client will pick a random unused outgoing communications port - this is the typical setting.  The only time you will ever need to change this is if firewall settings restrict outgoing traffic to a specific port."), DefaultValue(0)> _
        Public Overridable Property TCPPort() As Integer Implements IClient.TCPPort
            Get
                Return ClientTCPPort
            End Get
            Set(ByVal Value As Integer)
                ClientTCPPort = Value
            End Set
        End Property

        <Browsable(True), Category("Connection"), Description("Specifies whether or not to automatically keep retrying client connection when server reports maximum client capacity."), DefaultValue(True)> _
        Public Property RetryConnectionAtServerCapacity() As Boolean
            Get
                Return RetryAtCapacity
            End Get
            Set(ByVal Value As Boolean)
                RetryAtCapacity = Value
            End Set
        End Property

        <Browsable(False)> _
        Public Overridable ReadOnly Property Connected() As Boolean Implements IClient.Connected
            Get
                Return ClientConnected
            End Get
        End Property

        <Browsable(False)> _
        Public Overridable ReadOnly Property ConnectTime() As DateTime Implements IClient.ConnectTime
            Get
                Return ClientConnectTime
            End Get
        End Property

        ' Returns the description of the client - you may want to override this to extend it with information about your client
        <Browsable(False)> _
        Public Overridable ReadOnly Property Description() As String Implements IClient.Description
            Get
                Dim strClientID As New StringBuilder

                With strClientID
                    .Append("Client Channel Sink:" & vbCrLf)
                    .Append("   Assembly: " & GetShortAssemblyName(System.Reflection.Assembly.GetExecutingAssembly) & vbCrLf)
                    .Append("   Location: " & System.Reflection.Assembly.GetExecutingAssembly.Location & vbCrLf)
                    .Append("    Created: " & File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly.Location) & vbCrLf)
                    .Append("    NT User: " & System.Security.Principal.WindowsIdentity.GetCurrent.Name & vbCrLf)
                    .Append("     System: " & System.Environment.MachineName & vbCrLf)
                    .Append("        URI: " & ServerURI & vbCrLf)
                    .Append("         ID: " & ID.ToString() & vbCrLf)
                End With

                Return strClientID.ToString()
            End Get
        End Property

        ' Number of seconds between connection attempts
        <Browsable(True), Category("Connection"), Description("Number of seconds to wait between connection attempts."), DefaultValue(2)> _
        Public Overridable Property ConnectionAttemptInterval() As Integer Implements IClient.ConnectionAttemptInterval
            Get
                Return ConnectionTimer.Interval / 1000
            End Get
            Set(ByVal Value As Integer)
                With ConnectionTimer
                    If Value < 1 Then
                        .Interval = 100
                    Else
                        .Interval = Value * 1000
                    End If
                End With
            End Set
        End Property

        ' Number of seconds between activity tests
        <Browsable(True), Category("Event Processing"), Description("Number of seconds to wait without any response from the server before assuming we're no longer connected to server and restarting the connect cycle.  Set to zero to disable this timer."), DefaultValue(600)> _
        Public Overridable Property NoActivityTestInterval() As Integer Implements IClient.NoActivityTestInterval
            Get
                Return ClientNoActivityTestInterval
            End Get
            Set(ByVal Value As Integer)
                ClientNoActivityTestInterval = Value

                With NoActivityTimer
                    If Value < 1 Then
                        .Interval = 600000
                        .Enabled = False
                    Else
                        .Interval = Value * 1000
                        NewActivity()
                    End If
                End With
            End Set
        End Property

        ' Number of seconds to wait for delayed restarts
        <Browsable(True), Category("Connection"), Description("Number of seconds to wait for delayed starts or restarts."), DefaultValue(30)> _
        Public Overridable Property RestartDelayInterval() As Integer Implements IClient.RestartDelayInterval
            Get
                Return RestartTimer.Interval / 1000
            End Get
            Set(ByVal Value As Integer)
                RestartTimer.Interval = Value * 1000
            End Set
        End Property

        ' We constantly monitor activity from the host, if we've not had any activity in
        ' a set amount of time, we restart the connect cycle
        Public Overridable Sub NewActivity() Implements IClient.NewActivity

            ' We reset the activity test timer any time there is activity, it should
            ' only run after a preset amount of "no activity"
            With NoActivityTimer
                .Enabled = False
                If ClientNoActivityTestInterval > 0 Then
                    .Enabled = True
                End If
            End With

        End Sub

        <Browsable(False)> _
        Public Overridable ReadOnly Property ID() As Guid Implements IClient.ID
            Get
                Static ClientID As Guid = Guid.NewGuid()
                Return ClientID
            End Get
        End Property

        ' Host ID, as reported by the server
        <Browsable(False)> _
        Public Overridable ReadOnly Property HostID() As String Implements IClient.HostID
            Get
                Static strHostID As String

                If Len(strHostID) = 0 Then
                    If Not RemoteServer Is Nothing Then
                        Try
                            strHostID = RemoteServer.HostID
                        Catch
                            strHostID = ""
                        End Try
                    End If
                End If

                Return strHostID
            End Get
        End Property

        ' This procedure allows clients to send an event message back to the server
        Public Overridable Sub SendNotification(ByVal sender As Object, ByVal e As EventArgs) Implements IClient.SendNotification

            If RemoteServer Is Nothing Then
                Throw New RemotingClientNotConnectedException("Notification not sent - not connected to remote server """ & ServerURI & """")
            Else
                Try
                    RemoteServer.SendServerNotification(ID, sender, e)
                Catch ex As RemotingClientNotRegisteredException
                    ' If the server no longer recognizes us then we need to reconnect...
                    Connect(False)
                    Throw New RemotingClientNotConnectedException("Notification not sent - no longer connected to remote server """ & ServerURI & """")
                Catch
                    ' All other exceptions just get rethrown
                    Throw
                End Try
            End If

        End Sub

        ' This function allows clients to send an event message back to the server without worrying about possible exceptions
        Public Overridable Function SendSafeNotification(ByVal sender As Object, ByVal e As EventArgs) As Boolean Implements IClient.SendSafeNotification

            Dim flgSent As Boolean = False  ' Any failures should yield "false"

            If Not RemoteServer Is Nothing Then
                Try
                    SendNotification(sender, e)
                    flgSent = True
                Catch ex As Exception
                    SendStatusMessage("Error reported during notification request: " & ex.Message, True)
                End Try
            End If

            Return flgSent

        End Function

        ' Returns True if server is responding (works by sending a test message to the remote server)
        Public Overridable Function ServerIsResponding(Optional ByVal Timeout As Integer = 5) As Boolean Implements IClient.ServerIsResponding

            If Connected Then
                ' Note that the client may "think" it's connected when it's not - the server host may have crashed and
                ' couldn't notify the client of the disconnect.  The client will figure this out by itself eventually
                ' (it has a no activity timer) - but if we want the "real" current status, we test server availability
                ' by sending a meaningless message to the server to see if it responds to RPC requests.  We spawn
                ' the test on a separate thread just so we can timeout if it takes too long...
                With New ServerCommTestThread(Me, Timeout)
                    If .ExecuteTest() Then
                        ' Server responded to RPC request, return current connection state
                        Return Connected
                    Else
                        Return False
                    End If
                End With
            Else
                ' If we're not connected to service, then it's unavailable as far as we're concerned...
                Return False
            End If

        End Function

        ' This internal class incapsulates the execution thread of a server communications test
        Private Class ServerCommTestThread

            Inherits ThreadBase

            Private Parent As ClientBase
            Private Timeout As Integer
            Private Success As Boolean

            Public Sub New(ByVal Parent As ClientBase, ByVal Timeout As Integer)

                Me.Parent = Parent
                Me.Timeout = Timeout * 1000

            End Sub

            Public Function ExecuteTest() As Boolean

                Start()

                With WorkerThread
                    If Not .Join(Timeout) Then .Abort()
                End With

                Return Success

            End Function

            Protected Overrides Sub ThreadProc()

                Parent.SendNotification(Nothing, EventArgs.Empty)
                Success = True

            End Sub

        End Class

        Public Overridable Sub SendStatusMessage(ByVal Message As String, ByVal NewLine As Boolean) Implements IClient.SendStatusMessage

            RaiseEvent_StatusMessage(Message, NewLine)

        End Sub

        Protected Overridable Sub StartCommunications()

            If CommunicationsChannel Is Nothing Then
                Try
                    ' Create and register new communications channel
                    Dim props As New Hashtable

                    With props
                        .Add("name", "tcp" & Guid.NewGuid.ToString())
                        .Add("port", ClientTCPPort)
                        .Add("rejectRemoteRequests", False)
                    End With

                    CommunicationsChannel = New TcpChannel(props, CreateClientProviderChain(), CreateServerProviderChain())
                    ChannelServices.RegisterChannel(CommunicationsChannel)
                Catch ex As Exception
                    SendStatusMessage("Failed to create client communications channel: " & ex.Message, True)
                End Try
            End If

        End Sub

        Protected Overridable Sub StopCommunications()

            Try
                ' Terminate client communications channel
                If Not CommunicationsChannel Is Nothing Then
                    ChannelServices.UnregisterChannel(CommunicationsChannel)
                End If

                CommunicationsChannel = Nothing
            Catch ex As Exception
                SendStatusMessage("Failed to terminate communications channel: " & ex.Message, True)
            End Try

        End Sub

        ' This function is used to send an event notification to the client (this is typically used by the remote server)
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub SendClientNotification(ByVal sender As Object, ByVal e As EventArgs) Implements IClient.SendClientNotification

            ' We always keep track of the fact that we received a response from the host so we can know
            ' that our connection is still valid
            NewActivity()

            ' We simply bubble this message up to the client through an event - to the client, this is a
            ' notification coming in from the server - hence the nomenclature change
            RaiseEvent_ServerNotification(sender, e)

        End Sub

        ' This function allows server to dynamically change the current client connection URI to help with load balancing
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub UpdateCurrentURI(ByVal URI As String) Implements IClient.UpdateCurrentURI

            ServerURI = URI

        End Sub

        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overridable Property RemoteReference() As IServer Implements IClient.RemoteReference
            Get
                Return RemoteServer
            End Get
            Set(ByVal Value As IServer)
                RemoteServer = Value
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Function IsInternalClient() As Boolean Implements IClient.IsInternalClient

            Return False

        End Function

        ' Override the lease settings for this object
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function InitializeLifetimeService() As Object

            ' Although this class only acts a server in the sense that it's receiving events,
            ' we still do this to insure that this instance never dies, no matter how long
            ' between calls
            Dim lease As Lifetime.ILease = MyBase.InitializeLifetimeService()

            If lease.CurrentState = Lifetime.LeaseState.Initial Then
                lease.InitialLeaseTime = TimeSpan.Zero
            End If

            Return lease

        End Function

        <MethodImpl(MethodImplOptions.Synchronized)> _
        Private Sub ConnectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ConnectionTimer.Elapsed

            If Not ClientConnected Then
                ' We keep trying to connect until the connection is established
                Try
                    If Len(ServerURI) > 0 Then
                        Dim strConnectionURI As String = ServerURI
                        RaiseEvent_AttemptingConnection()
                        StartCommunications()

                        ' Get the host's shared server instance
                        RemoteServer = Activator.GetObject(GetType(IServer), ServerURI)

                        If RemoteServer Is Nothing Then
                            ' We can't use an uninstantiated object, so we'll wait on host to provide one...
                            Throw New RemotingClientNotConnectedException("Failed to aquire object instance for """ & ServerURI & """")
                        Else
                            Try
                                ' Test a remote procedure call...
                                If Len(RemoteServer.HostID) > 0 Then
                                    ' Register remote client with server
                                    If RegisterClient.Invoke() Then
                                        ClientConnected = True
                                        ClientConnectTime = Now()
                                        RaiseEvent_ConnectionEstablished()
                                    Else
                                        If String.Compare(strConnectionURI, ServerURI, True) = 0 Then
                                            Throw New RemotingClientNotConnectedException("Failed to register client with remote server")
                                        Else
                                            Throw New RemotingClientNotConnectedException("Server redirecting client to """ & ServerURI & """ for load balancing")
                                        End If
                                    End If
                                Else
                                    Throw New RemotingClientNotConnectedException("Failed to retrieve valid host ID - host ID must not be blank")
                                End If
                            Catch ex As RemotingServerConnectionsAtCapacityException
                                If RetryAtCapacity Then
                                    Throw New RemotingClientNotConnectedException("Connection to remote server not established yet because """ & ex.Message & """, retrying connection...", ex)
                                Else
                                    RaiseEvent_ConnectionAttemptFailed(New RemotingServerConnectionsAtCapacityException("Client failed to connect because """ & ex.Message & """." & vbCrLf & vbCrLf & "Connection loop terminated.", ex))
                                End If
                            Catch ex As RemotingServerFailedToCreateNewHostException
                                ' When server reports failure to create a new host, we respond to this just as if it had reported client connections at capacity
                                If RetryAtCapacity Then
                                    Throw New RemotingClientNotConnectedException("Connection to remote server not established yet because """ & ex.Message & """, retrying connection...", ex)
                                Else
                                    RaiseEvent_ConnectionAttemptFailed(New RemotingServerFailedToCreateNewHostException("Client failed to connect because """ & ex.Message & """." & vbCrLf & vbCrLf & "Connection loop terminated.", ex))
                                End If
                            Catch ex As Exception
                                Throw New RemotingClientNotConnectedException("Connection to remote server not established yet because """ & ex.Message & """, retrying connection...", ex)
                            End Try
                        End If
                    Else
                        RaiseEvent_ConnectionAttemptFailed(New RemotingClientNotConnectedException("Client failed to connect because no ""URI"" string has been defined." & vbCrLf & vbCrLf & "Connection loop terminated."))
                    End If
                Catch ex As Exception
                    ' Try again
                    RaiseEvent_ConnectionAttemptFailed(ex)
                    Disconnect()
                    ConnectionTimer.Enabled = True
                End Try

                ' We count attempts to connect as an activity
                NewActivity()
            End If

        End Sub

        ' Default delegate implementations of the client registration functions
        Private Function DefaultRegisterClient() As Boolean

            Return RemoteRegisterClient(ID, Me)

        End Function

        Private Function DefaultUnregisterClient() As Boolean

            Return RemoteUnregisterClient(ID)

        End Function

        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property RegisterClientFunction() As IClient.ClientRegistrationFunction Implements IClient.RegisterClientFunction
            Get
                Return RegisterClient
            End Get
            Set(ByVal Value As IClient.ClientRegistrationFunction)
                RegisterClient = Value
            End Set
        End Property

        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property UnregisterClientFunction() As IClient.ClientRegistrationFunction Implements IClient.UnregisterClientFunction
            Get
                Return UnregisterClient
            End Get
            Set(ByVal Value As IClient.ClientRegistrationFunction)
                UnregisterClient = Value
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Function RemoteRegisterClient(ByVal ID As Guid, ByVal RemotingClient As IClient) As Boolean Implements IClient.RemoteRegisterClient

            If Not RemoteServer Is Nothing Then
                Return RemoteServer.RegisterClient(ID, RemotingClient)
            Else
                Return False
            End If

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Function RemoteUnregisterClient(ByVal ID As Guid) As Boolean Implements IClient.RemoteUnregisterClient

            If Not RemoteServer Is Nothing Then
                Return RemoteServer.UnregisterClient(ID)
            Else
                Return False
            End If

        End Function

        Private Sub RestartTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles RestartTimer.Elapsed

            ' We delayed before we started trying to reconnect again to give the server host plenty
            ' of time for startup/shutdown, now we can resume trying to reestablish a connection
            SendStatusMessage("Waiting for connection...", True)

            ' Start trying to reestablish connectitity again
            ConnectionTimer.Enabled = True

        End Sub

        Private Sub NoActivityTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles NoActivityTimer.Elapsed

            SendStatusMessage("No activity has occurred within last " & LCase(SecondsToText(NoActivityTestInterval())) & ", restarting connect cycle.", True)

            ' If we've not received any event notifications in the last 10 minutes, we've may have lost connectivity,
            ' so we will start trying to reconnnect...
            Connect(False)

        End Sub

        Public MustOverride Function CreateClientProviderChain() As IClientChannelSinkProvider

        Public MustOverride Function CreateServerProviderChain() As IServerChannelSinkProvider

        ' These RaiseEvent wrappers make sure RPC calls don't stop processing because of end user code
        ' boo-boo's - an event is provided so that the user can know when they've got an issue...
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ServerNotification(ByVal sender As Object, ByVal e As EventArgs)

            Try
                RaiseEvent ServerNotification(sender, e)
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ClientBase::ServerNotification", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_StatusMessage(ByVal Message As String, ByVal NewLine As Boolean)

            Try
                RaiseEvent StatusMessage(Message, NewLine)
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ClientBase::StatusMessage", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_AttemptingConnection()

            Try
                RaiseEvent AttemptingConnection()
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ClientBase::AttemptingConnection", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ConnectionAttemptFailed(ByVal ConnectionException As Exception)

            Try
                RaiseEvent ConnectionAttemptFailed(ConnectionException)
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ClientBase::ConnectionAttemptFailed", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ConnectionEstablished()

            Try
                RaiseEvent ConnectionEstablished()
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ClientBase::ConnectionEstablished", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ConnectionTerminated()

            Try
                RaiseEvent ConnectionTerminated()
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ClientBase::ConnectionTerminated", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_UserEventHandlerException(ByVal EventName As String, ByVal ex As Exception)

            Try
                RaiseEvent UserEventHandlerException(EventName, ex)
            Catch
                ' Not stopping for bad code
            End Try

        End Sub

    End Class

End Namespace