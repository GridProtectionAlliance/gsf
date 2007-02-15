' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.ComponentModel
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Tcp
Imports System.Runtime.Remoting.Messaging
Imports System.Runtime.Remoting.Services
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Reflection
Imports System.Reflection.BindingFlags
Imports TVA.Shared.DateTime
Imports TVA.Shared.FilePath
Imports TVA.Threading

Namespace Remoting

    ' This is the base for a remoteable server interface used to pass events between a host and any number of clients
    <Serializable()> _
    Public MustInherit Class ServerBase

        Inherits MarshalByRefObject
        Implements IServer
        Implements IDisposable

        <NonSerialized()> Private ServerHostID As String                                ' ID the host identifies itself as, connecting client can validate this
        <NonSerialized()> Private ServerURI As String                                   ' URI to use for remoting server (e.g., would be only "ServerURI" in tcp://localhost:8090/ServerURI)
        <NonSerialized()> Private ServerTCPPort As Integer                              ' TCP communications port used by this instance
        <NonSerialized()> Private ServerResponseTimeout As Integer                      ' Maximum number of milliseconds allowed for remote clients to respond to a notification before timing-out
        <NonSerialized()> Private ServerQueueInterval As Integer                        ' Maximum number of milliseconds to wait before processing a remote client's notification queue
        <NonSerialized()> Private ServerShutdownTimeout As Integer                      ' Maximum number of seconds allowed for server to shutdown
        <NonSerialized()> Private ServerMaxClients As Integer                           ' Maximum number of clients one server will process
        <NonSerialized()> Private ServerMaxServers As Integer                           ' Maximum number of new servers that will be created to handle new client connections - set to -1 for no limit
        <NonSerialized()> Private WithEvents ServerNextRef As ServerClientReference     ' Client instance to next server in the server chain
        <NonSerialized()> Private ServerBase As IServer                                 ' First server (primary event host) in the server chain
        <NonSerialized()> Private CreateServerHost As CreateServerHostFunction          ' User overridable function to create the next server host
        <NonSerialized()> Private TerminateServerHost As TerminateServerHostFunction    ' User overridable function to terminate the server host
        <NonSerialized()> Private CommunicationsChannel As TcpChannel                   ' Server's TCP based communications channel
        <NonSerialized()> Private ServerClients As LocalClients                         ' Collection of connected client information
        <NonSerialized()> Private WithEvents ConnectionTimer As Timers.Timer            ' Creates a connections setup loop - loop terminates with successful established connection
        <NonSerialized()> Private WithEvents StartupDelayTimer As Timers.Timer          ' Initiates connections setup loop after allowing ample start time (TCP ports can be slow to release)
        <NonSerialized()> Private HostProcesses As Hashtable                            ' Defines a reference to all of the locally created host process ID's - only created in base server

        Public Event ServerEstablished(ByVal ServerID As Guid, ByVal TCPPort As Integer) Implements IServer.ServerEstablished
        Public Event ClientNotification(ByVal Client As LocalClient, ByVal sender As Object, ByVal e As EventArgs) Implements IServer.ClientNotification
        Public Event ClientConnected(ByVal Client As LocalClient) Implements IServer.ClientConnected
        Public Event ClientDisconnected(ByVal Client As LocalClient) Implements IServer.ClientDisconnected
        Public Event UserEventHandlerException(ByVal EventName As String, ByVal ex As Exception) Implements IServer.UserEventHandlerException

        Private Const ExternalRemotingHost As String = "TVA.Remoting.Host.exe"

        Protected Class StartArgs

            Implements ICollection, IEnumerable

            Private Args As Hashtable

            Friend Sub New()

                Args = New Hashtable

            End Sub

            Public Sub Add(ByVal Name As String, ByVal Value As Object)

                SyncLock Args.SyncRoot
                    Args.Add(Name, Value)
                End SyncLock

            End Sub

            Public ReadOnly Property Count() As Integer Implements System.Collections.ICollection.Count
                Get
                    Return Args.Count
                End Get
            End Property

            Default Public Property Item(ByVal Name As String) As Object
                Get
                    Return Args(Name)
                End Get
                Set(ByVal Value As Object)
                    Args(Name) = Value
                End Set
            End Property

            Public ReadOnly Property Keys() As ICollection
                Get
                    Return Args.Keys
                End Get
            End Property

            Friend Function GetArgList() As String

                Dim ArgList As New StringBuilder
                Dim Item As DictionaryEntry
                Dim FirstAdded As Boolean

                For Each Item In Args
                    If FirstAdded Then ArgList.Append(";")
                    ArgList.Append(Item.Key)
                    ArgList.Append("=")
                    ArgList.Append(Item.Value)
                    FirstAdded = True
                Next

                Return ArgList.ToString()

            End Function

            Public Sub CopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo

                Args.CopyTo(array, index)

            End Sub

            Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
                Get
                    Return Args.IsSynchronized
                End Get
            End Property

            Public ReadOnly Property SyncRoot() As Object Implements System.Collections.ICollection.SyncRoot
                Get
                    Return Args.SyncRoot
                End Get
            End Property

            Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator

                Return Args.GetEnumerator()

            End Function

        End Class

        'Private Class ClientTrackingHandler

        '    Implements ITrackingHandler

        '    Public Sub MarshaledObject(ByVal obj As Object, ByVal [or] As ObjRef) Implements ITrackingHandler.MarshaledObject

        '        Dim ChannelData As Object() = [or].ChannelInfo.ChannelData
        '        'Dim ObservedIP As Object = CallContext.GetData("ObservedIP")

        '        'If ObservedIP Is Nothing Then Return
        '        If [or].ChannelInfo Is Nothing Then Return

        '        'Dim Address As String = ObservedIP.ToString()
        '        Dim URI As String
        '        Dim x As Integer

        '        Debug.WriteLine("Server ChannelData Length Now = " & ChannelData.Length)

        '        For x = 0 To ChannelData.Length - 1
        '            Debug.WriteLine("     Item " & x & " is a " & TypeName(ChannelData(x)))
        '            If TypeOf ChannelData(x) Is ChannelDataStore Then
        '                For Each URI In DirectCast(ChannelData(x), ChannelDataStore).ChannelUris
        '                    Debug.WriteLine(URI)
        '                Next
        '            End If
        '        Next

        '    End Sub

        '    Public Sub UnmarshaledObject(ByVal obj As Object, ByVal [or] As ObjRef) Implements ITrackingHandler.UnmarshaledObject

        '    End Sub

        '    Public Sub DisconnectedObject(ByVal obj As Object) Implements ITrackingHandler.DisconnectedObject

        '    End Sub

        'End Class

        <Serializable()> _
        Private Class ServerClientReference

            Inherits ClientBase

            ' We delegate all calls and events to this remoting client
            <NonSerialized()> Private WithEvents InternalRemotingClient As IClient

            Public Sub New(ByVal Parent As ServerBase, ByVal NewURI As String)

                MyBase.New()
                MyBase.NoActivityTestInterval = 0
                RegisterClientFunction = Nothing
                UnregisterClientFunction = Nothing

                Dim ClientStartArgs As New StartArgs
                Dim ClientAssembly As [Assembly]
                Dim ClientType As Type
                Dim PropertyCall As PropertyInfo
                Dim FieldCall As FieldInfo
                Dim KeyValue As DictionaryEntry

                ' We have to keep an internal reference to newly created server hosts so we can cascade messages
                ' to and from the new host.  We just create a standard client of this server to do this...
                ClientAssembly = [Assembly].LoadFrom(Parent.ClientAssembly)
                ClientType = ClientAssembly.GetType(Parent.ClientType)
                InternalRemotingClient = Activator.CreateInstance(ClientType)

                ' This client instance wrapper will be the IClient remoted to the server, however the internal remoting
                ' client will be the one actually handling the communications so we change the client registration
                ' functions to implement this change in functionality...
                With InternalRemotingClient
                    .RegisterClientFunction = AddressOf RegisterInternalServerReferenceClient
                    .UnregisterClientFunction = AddressOf UnregisterInternalServerReferenceClient
                End With

                ClientStartArgs.Add("URI", NewURI)
                Parent.InitializeClientStartArgs(ClientStartArgs)

                ' Step through each key/value pairs and initialize each specifed property value on new client object
                For Each KeyValue In ClientStartArgs
                    ' See if we can lookup given method as a property
                    PropertyCall = ClientType.GetProperty(KeyValue.Key, [Public] Or [Instance] Or [Static] Or [IgnoreCase])
                    If PropertyCall Is Nothing Then
                        ' If not, see if it is a field type propery
                        FieldCall = ClientType.GetField(KeyValue.Key, [Public] Or [Instance] Or [Static] Or [IgnoreCase])
                        If FieldCall Is Nothing Then
                            Parent.RaiseEvent_UserEventHandlerException("InitializeClientStartArgs", New ArgumentException("Property [" & KeyValue.Key & "] not found in [" & Parent.ClientType & "] class!"))
                        Else
                            FieldCall.SetValue(InternalRemotingClient, Convert.ChangeType(KeyValue.Value, FieldCall.FieldType))
                        End If
                    Else
                        PropertyCall.SetValue(InternalRemotingClient, Convert.ChangeType(KeyValue.Value, PropertyCall.PropertyType), Nothing)
                    End If
                Next

                InternalRemotingClient.Connect()

            End Sub

            ' We delegate all overridable properties to the internal remoting client...
            Public Overrides Sub Shutdown()

                InternalRemotingClient.Shutdown()

            End Sub

            Public Overrides Sub Connect(Optional ByVal StartDelay As Boolean = False)

                InternalRemotingClient.Connect(StartDelay)

            End Sub

            Public Overrides Sub Disconnect()

                InternalRemotingClient.Disconnect()

            End Sub

            Public Overrides Property URI() As String
                Get
                    Return InternalRemotingClient.URI
                End Get
                Set(ByVal Value As String)
                    InternalRemotingClient.URI = Value
                End Set
            End Property

            Public Overrides ReadOnly Property BaseURI() As String
                Get
                    Return InternalRemotingClient.BaseURI
                End Get
            End Property

            Public Overrides Property TCPPort() As Integer
                Get
                    Return InternalRemotingClient.TCPPort
                End Get
                Set(ByVal Value As Integer)
                    InternalRemotingClient.TCPPort = Value
                End Set
            End Property

            Public Overrides ReadOnly Property Connected() As Boolean
                Get
                    Return InternalRemotingClient.Connected
                End Get
            End Property

            Public Overrides ReadOnly Property ConnectTime() As DateTime
                Get
                    Return InternalRemotingClient.ConnectTime
                End Get
            End Property

            ' In order to better identify this as a "server-side" connection, we extend the internal client's description
            Public Overrides ReadOnly Property Description() As String
                Get
                    Return "Internal Server Reference extending " & InternalRemotingClient.Description
                End Get
            End Property

            Public Overrides ReadOnly Property ID() As System.Guid
                Get
                    Return InternalRemotingClient.ID
                End Get
            End Property

            Public Overrides ReadOnly Property HostID() As String
                Get
                    Return InternalRemotingClient.HostID
                End Get
            End Property

            Public Overrides Sub NewActivity()

                InternalRemotingClient.NewActivity()

            End Sub

            Public Overrides Property NoActivityTestInterval() As Integer
                Get
                    Return InternalRemotingClient.NoActivityTestInterval
                End Get
                Set(ByVal Value As Integer)
                    InternalRemotingClient.NoActivityTestInterval = Value
                End Set
            End Property

            Public Overrides Property RestartDelayInterval() As Integer
                Get
                    Return InternalRemotingClient.RestartDelayInterval
                End Get
                Set(ByVal Value As Integer)
                    InternalRemotingClient.RestartDelayInterval = Value
                End Set
            End Property

            Public Overrides Property ConnectionAttemptInterval() As Integer
                Get
                    Return InternalRemotingClient.ConnectionAttemptInterval
                End Get
                Set(ByVal Value As Integer)
                    InternalRemotingClient.ConnectionAttemptInterval = Value
                End Set
            End Property

            Public Overrides Sub SendStatusMessage(ByVal Message As String, ByVal NewLine As Boolean)

                InternalRemotingClient.SendStatusMessage(Message, NewLine)

            End Sub

            Public Overrides Sub SendClientNotification(ByVal sender As Object, ByVal e As EventArgs)

                InternalRemotingClient.SendClientNotification(sender, e)

            End Sub

            Public Overrides Sub SendNotification(ByVal sender As Object, ByVal e As System.EventArgs)

                InternalRemotingClient.SendNotification(sender, e)

            End Sub

            Public Overrides Function SendSafeNotification(ByVal sender As Object, ByVal e As EventArgs) As Boolean

                Return InternalRemotingClient.SendSafeNotification(sender, e)

            End Function

            Public Overrides Function ServerIsResponding(Optional ByVal Timeout As Integer = 5) As Boolean

                Return InternalRemotingClient.ServerIsResponding(Timeout)

            End Function

            Public Overrides Function CreateClientProviderChain() As IClientChannelSinkProvider

                Throw New NotImplementedException("This functionality is delegated to the internal remoting client...")

            End Function

            Public Overrides Function CreateServerProviderChain() As IServerChannelSinkProvider

                Throw New NotImplementedException("This functionality is delegated to the internal remoting client...")

            End Function

            Protected Overrides Sub StartCommunications()

                Throw New NotImplementedException("This functionality is delegated to the internal remoting client...")

            End Sub

            Protected Overrides Sub StopCommunications()

                Throw New NotImplementedException("This functionality is delegated to the internal remoting client...")

            End Sub

            Public Overrides Sub UpdateCurrentURI(ByVal URI As String)

                ' Since we are the primary server link for our remote host, we ignore any requests to
                ' redirect us to another server - we *must* keep trying to connect to our base server

            End Sub

            Public Overrides Property RemoteReference() As IServer
                Get
                    Return InternalRemotingClient.RemoteReference
                End Get
                Set(ByVal Value As IServer)
                    InternalRemotingClient.RemoteReference = Value
                End Set
            End Property

            ' Since this is the "connector" client between servers - we want to gurantee our connection to the server
            ' so we identify ourselves as an internal client...
            Public Overrides Function IsInternalClient() As Boolean

                Return True

            End Function

            ' We register the "external" class instance as the remoted IClient instance instead of the internal class
            ' instance so we can intercept and delegate functionality as needed - but we let the internal remoting
            ' class handle all of the communications work...
            Private Function RegisterInternalServerReferenceClient() As Boolean

                Return InternalRemotingClient.RemoteRegisterClient(ID, Me)

            End Function

            Private Function UnregisterInternalServerReferenceClient() As Boolean

                Return InternalRemotingClient.RemoteUnregisterClient(ID)

            End Function

            ' We just bubble internal events up through this class...
            Private Sub InternalRemotingClient_ServerNotification(ByVal sender As Object, ByVal e As EventArgs) Handles InternalRemotingClient.ServerNotification

                RaiseEvent_ServerNotification(sender, e)

            End Sub

            Private Sub InternalRemotingClient_AttemptingConnection() Handles InternalRemotingClient.AttemptingConnection

                RaiseEvent_AttemptingConnection()

            End Sub

            Private Sub InternalRemotingClient_ConnectionAttemptFailed(ByVal ex As Exception) Handles InternalRemotingClient.ConnectionAttemptFailed

                RaiseEvent_ConnectionAttemptFailed(ex)

            End Sub

            Private Sub InternalRemotingClient_ConnectionEstablished() Handles InternalRemotingClient.ConnectionEstablished

                RaiseEvent_ConnectionEstablished()

            End Sub

            Private Sub InternalRemotingClient_ConnectionTerminated() Handles InternalRemotingClient.ConnectionTerminated

                RaiseEvent_ConnectionTerminated()

            End Sub

            Private Sub InternalRemotingClient_StatusMessage(ByVal Message As String, ByVal NewLine As Boolean) Handles InternalRemotingClient.StatusMessage

                RaiseEvent_StatusMessage(Message, NewLine)

            End Sub

            Private Sub InternalRemotingClient_UserEventHandlerException(ByVal EventName As String, ByVal ex As System.Exception) Handles InternalRemotingClient.UserEventHandlerException

                RaiseEvent_UserEventHandlerException(EventName, ex)

            End Sub

        End Class

        Public Sub New()

            ServerClients = New LocalClients
            ConnectionTimer = New Timers.Timer
            StartupDelayTimer = New Timers.Timer
            ServerResponseTimeout = 10000
            ServerQueueInterval = 250
            ServerShutdownTimeout = 15000
            ServerMaxClients = 5
            ServerMaxServers = -1
            CreateServerHost = AddressOf DefaultCreateServerHost
            TerminateServerHost = AddressOf DefaultTerminateServerHost

            ' Define a timer for communications setup
            With ConnectionTimer
                .AutoReset = False
                .Interval = 5000
                .Enabled = False
            End With

            ' Define a start up delay timer
            With StartupDelayTimer
                .AutoReset = False
                .Interval = 15000
                .Enabled = False
            End With

        End Sub

        Public Sub New(ByVal HostID As String, ByVal URI As String, ByVal TCPPort As Integer)

            MyClass.New()
            ServerHostID = HostID
            ServerURI = URI
            ServerTCPPort = TCPPort

        End Sub

        Protected Overrides Sub Finalize()

            Shutdown()

        End Sub

        Public Sub Start(Optional ByVal StartDelay As Integer = 0) Implements IServer.Start

            If Not NextServer Is Nothing Then NextServer.Start(StartDelay)

            If StartDelay > 0 Then
                StartupDelayTimer.Interval = StartDelay * 1000
                StartupDelayTimer.Enabled = True
            Else
                ConnectionTimer_Elapsed(Nothing, Nothing)
            End If

        End Sub

        Public Overridable Sub Shutdown() Implements IServer.Shutdown, IDisposable.Dispose

            On Error Resume Next

            ' We spawn the server shutdown code on a separate thread just so we can timeout if it takes too long...
            With New ServerShutdownThread(Me)
                .ExecuteShutdown()
            End With

            GC.SuppressFinalize(Me)
            FlushNotifications()

            ' We handle shutdown in an orderly fashion
            ConnectionTimer.Enabled = False
            StartupDelayTimer.Enabled = False
            StopCommunications()

        End Sub

        ' This internal class incapsulates the execution thread of the server shutdown procedure
        Private Class ServerShutdownThread

            Inherits ThreadBase

            Private Parent As ServerBase

            Public Sub New(ByVal Parent As ServerBase)

                Me.Parent = Parent

            End Sub

            Public Sub ExecuteShutdown()

                Start()

                With WorkerThread
                    If Not .Join(Parent.ShutdownTimeout) Then .Abort()
                End With

            End Sub

            Protected Overrides Sub ThreadProc()

                Dim NextServer As IServer = Parent.NextServer
                Dim Port As Integer
                Dim URI As String

                ' Shutdown next server
                If Not NextServer Is Nothing Then
                    With NextServer
                        Port = .TCPPort
                        URI = .URI
                        .Shutdown()
                    End With

                    Try
                        ' Terminate server host
                        Parent.TerminateServerHost.Invoke(Port, URI)
                    Catch ex As Exception
                        ' Don't want to stop shutting down if we failed to terminate out remote host, but we
                        ' do want to let user know that it failed...
                        Parent.RaiseEvent_UserEventHandlerException("TerminateServerHost", New RemotingException("Server failed to terminate remote server host: " & ex.Message, ex))
                    End Try
                End If

                ' Shutdown internal remoting client reference that referenced next server
                If Not Parent.ServerNextRef Is Nothing Then
                    Parent.ServerNextRef.Shutdown()
                    Parent.ServerNextRef = Nothing
                End If

            End Sub

        End Class

        <Browsable(True), Category("Connection"), Description("ID the host identifies itself as, connecting client can validate this.")> _
        Public Property HostID() As String Implements IServer.HostID
            Get
                Return ServerHostID
            End Get
            Set(ByVal Value As String)
                If Not NextServer Is Nothing Then NextServer.HostID = Value
                ServerHostID = Value
            End Set
        End Property

        <Browsable(True), Category("Connection"), Description("TCP communications port used by this server instance."), DefaultValue(8090)> _
        Public Property TCPPort() As Integer Implements IServer.TCPPort
            Get
                Return ServerTCPPort
            End Get
            Set(ByVal Value As Integer)
                If Not NextServer Is Nothing Then NextServer.TCPPort = Value
                ServerTCPPort = Value
            End Set
        End Property

        <Browsable(True), Category("Connection"), Description("URI to use for remoting server (e.g., would be only ""ServerURI"" in tcp://localhost:8090/ServerURI)."), DefaultValue("ServerURI")> _
        Public Property URI() As String Implements IServer.URI
            Get
                Return ServerURI
            End Get
            Set(ByVal Value As String)
                If Not NextServer Is Nothing Then NextServer.URI = Value
                ServerURI = Value
            End Set
        End Property

        <Browsable(True), Category("Event Processing"), Description("Maximum number of milliseconds allowed for remote clients to respond to a notification before timing-out"), DefaultValue(10000)> _
        Public Property ResponseTimeout() As Integer Implements IServer.ResponseTimeout
            Get
                Return ServerResponseTimeout
            End Get
            Set(ByVal Value As Integer)
                If Not NextServer Is Nothing Then NextServer.ResponseTimeout = Value
                ServerResponseTimeout = Value
            End Set
        End Property

        <Browsable(True), Category("Event Processing"), Description("Maximum number of milliseconds to wait before processing a remote client's notification queue"), DefaultValue(250)> _
        Public Property QueueInterval() As Integer Implements IServer.QueueInterval
            Get
                Return ServerQueueInterval
            End Get
            Set(ByVal Value As Integer)
                If Not NextServer Is Nothing Then NextServer.QueueInterval = Value
                ServerQueueInterval = Value
            End Set
        End Property

        <Browsable(True), Category("Event Processing"), Description("Maximum number of milliseconds allowed for server to shutdown"), DefaultValue(15000)> _
        Public Property ShutdownTimeout() As Integer Implements IServer.ShutdownTimeout
            Get
                Return ServerShutdownTimeout
            End Get
            Set(ByVal Value As Integer)
                If Not NextServer Is Nothing Then NextServer.ShutdownTimeout = Value
                ServerShutdownTimeout = Value
            End Set
        End Property

        <Browsable(True), Category("Scalability"), Description("Maximum number of clients one server will process"), DefaultValue(5)> _
        Public Property MaximumClients() As Integer Implements IServer.MaximumClients
            Get
                Return ServerMaxClients
            End Get
            Set(ByVal Value As Integer)
                If ServerMaxClients < 0 Then ServerMaxClients = 0
                If Not NextServer Is Nothing Then NextServer.MaximumClients = Value
                ServerMaxClients = Value
            End Set
        End Property

        <Browsable(True), Category("Scalability"), Description("Maximum number of servers that will be created to handle new client connections.  Set to -1 for no limit."), DefaultValue(-1)> _
        Public Property MaximumServers() As Integer Implements IServer.MaximumServers
            Get
                Return ServerMaxServers
            End Get
            Set(ByVal Value As Integer)
                If ServerMaxServers = 0 Then ServerMaxServers = 1
                If ServerMaxServers < 0 Then ServerMaxServers = -1
                If Not NextServer Is Nothing Then NextServer.MaximumServers = Value
                ServerMaxServers = Value
            End Set
        End Property

        ' Reference to the base server in the server chain (the event server instance)
        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overridable Property BaseServer() As IServer Implements IServer.BaseServer
            Get
                If ServerBase Is Nothing Then
                    Return Me
                Else
                    Return ServerBase
                End If
            End Get
            Set(ByVal Value As IServer)
                ' Do not cascade this value!
                ServerBase = Value
            End Set
        End Property

        ' Reference to the next server in the server chain
        <Browsable(False)> _
        Public Overridable ReadOnly Property NextServer() As IServer Implements IServer.NextServer
            Get
                If ServerNextRef Is Nothing Then
                    Return Nothing
                Else
                    Return ServerNextRef.RemoteReference
                End If
            End Get
        End Property

        ' Unique ID for this server instance
        <Browsable(False)> _
        Public Overridable ReadOnly Property ID() As Guid Implements IServer.ID
            Get
                Static ServerID As Guid = Guid.NewGuid()
                Return ServerID
            End Get
        End Property

        ' Collection of locally cached remote client connections for this server instance
        <Browsable(False)> _
        Public Overridable ReadOnly Property Clients() As LocalClients Implements IServer.Clients
            Get
                Return ServerClients
            End Get
        End Property

        ' Returns True if this server instance is established
        <Browsable(False)> _
        Public Overridable ReadOnly Property Established() As Boolean Implements IServer.Established
            Get
                ' We derive the established state by whether or not the connection loop is active
                Return Not ConnectionTimer.Enabled
            End Get
        End Property

        ' Returns True if all servers in the server chain are established
        <Browsable(False)> _
        Public Overridable ReadOnly Property AllServersEstablished() As Boolean Implements IServer.AllServersEstablished
            Get
                If ServerBase Is Nothing Then
                    Dim flgEstablished As Boolean = Established()
                    Dim NextRef As IServer = NextServer

                    Do While flgEstablished And Not NextRef Is Nothing
                        flgEstablished = NextRef.Established()
                        NextRef = NextRef.NextServer
                    Loop

                    Return flgEstablished
                Else
                    Return ServerBase.AllServersEstablished()
                End If
            End Get
        End Property

        ' Flush messages from notification queue
        Public Overridable Sub FlushNotifications() Implements IServer.FlushNotifications

            If Not NextServer Is Nothing Then NextServer.FlushNotifications()
            ServerClients.FlushNotifications()

        End Sub

        ' Searches the server chain starting with the parent for the given server ID - returns Nothing if not found
        Public Function FindServer(ByVal ServerID As Guid) As IServer Implements IServer.FindServer

            Return FindServer(ServerID, Nothing)

        End Function

        ' Searches the server chain starting where specified for the given server ID - returns Nothing if not found
        Public Function FindServer(ByVal ServerID As Guid, ByVal StartingWith As IServer) As IServer Implements IServer.FindServer

            If StartingWith Is Nothing Then
                If ServerBase Is Nothing Then
                    StartingWith = Me
                Else
                    StartingWith = ServerBase
                End If
            End If

            If StartingWith.ID.CompareTo(ID) = 0 Then
                Return StartingWith
            ElseIf StartingWith.NextServer Is Nothing Then
                Return Nothing
            Else
                Return FindServer(ServerID, StartingWith.NextServer)
            End If

        End Function

        ' Returns the cumulative client list for all servers in the server chain
        Public Function GetFullClientList() As String Implements IServer.GetFullClientList

            Dim strClientList As New StringBuilder
            Dim intIndex As Integer
            Dim NextRef As IServer = BaseServer

            Do While Not NextRef Is Nothing
                intIndex += 1
                strClientList.Append(vbCrLf & "Client list for server #" & intIndex & " [" & NextRef.ID.ToString() & "] on port [" & NextRef.TCPPort & "]" & vbCrLf)
                strClientList.Append(NextRef.Clients.GetClientList())
                NextRef = NextRef.NextServer
            Loop

            Return strClientList.ToString()

        End Function

        ' Total number of server instances in use
        <Browsable(False)> _
        Public ReadOnly Property ServerCount() As Integer Implements IServer.ServerCount
            Get
                Return GetServerCount(Nothing)
            End Get
        End Property

        ' Total number of server instances in use starting where specified
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Function GetServerCount(ByVal StartingWith As IServer) As Integer Implements IServer.GetServerCount

            Dim intCount As Integer = 1
            Dim NextRef As IServer

            If StartingWith Is Nothing Then
                If ServerBase Is Nothing Then
                    StartingWith = Me
                Else
                    StartingWith = ServerBase
                End If
            End If

            NextRef = StartingWith.NextServer

            Do Until NextRef Is Nothing
                intCount += 1
                NextRef = NextRef.NextServer
            Loop

            Return intCount

        End Function

        ' User overridable function to create the next server host
        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property CreateServerHostFunction() As CreateServerHostFunction Implements IServer.CreateServerHostFunction
            Get
                Return CreateServerHost
            End Get
            Set(ByVal Value As CreateServerHostFunction)
                If Value Is Nothing Then
                    CreateServerHost = AddressOf DefaultCreateServerHost
                Else
                    CreateServerHost = Value
                End If
            End Set
        End Property

        ' User overridable function to terminate the server host
        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property TerminateServerHostFunction() As TerminateServerHostFunction Implements IServer.TerminateServerHostFunction
            Get
                Return TerminateServerHost
            End Get
            Set(ByVal Value As TerminateServerHostFunction)
                If Value Is Nothing Then
                    TerminateServerHost = AddressOf DefaultTerminateServerHost
                Else
                    TerminateServerHost = Value
                End If
            End Set
        End Property

        Private Sub ConnectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ConnectionTimer.Elapsed

            If CommunicationsChannel Is Nothing Then
                Try
                    StartCommunications()
                Catch ex As Exception
                    ' If this fails, we'll try again in a moment.  This can be common when the host of the server
                    ' instance restarts because the OS is often slow to release TCP/IP ports
                    ConnectionTimer.Enabled = True
                End Try
            End If

            If Not CommunicationsChannel Is Nothing Then
                Try
                    ' Remote this object instance to make it available to any client
                    If Not RemotingServices.Marshal(Me, ServerURI) Is Nothing Then RaiseEvent_ServerEstablished(ID, ServerTCPPort)
                Catch ex As Exception
                    ConnectionTimer.Enabled = True
                End Try
            End If

        End Sub

        Private Sub StartupDelayTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles StartupDelayTimer.Elapsed

            ' We delayed before we started trying to establish a connection to give the server plenty
            ' of time for startup/shutdown, now we can establish a connection
            ConnectionTimer.Enabled = True

        End Sub

        Protected Sub StartCommunications()

            ' Create and register new communications channel
            Dim props As New Hashtable

            With props
                .Add("name", "tcp" & Guid.NewGuid.ToString())
                .Add("port", ServerTCPPort)
                .Add("rejectRemoteRequests", False)
            End With

            CommunicationsChannel = New TcpChannel(props, CreateClientProviderChain(), CreateServerProviderChain())
            ChannelServices.RegisterChannel(CommunicationsChannel)
            'TrackingServices.RegisterTrackingHandler(New ClientTrackingHandler)

        End Sub

        Protected Sub StopCommunications()

            If Not CommunicationsChannel Is Nothing Then
                ChannelServices.UnregisterChannel(CommunicationsChannel)
            End If

            CommunicationsChannel = Nothing

        End Sub

        ' This function is used to register a client (this is typically used by remote clients)
        <MethodImpl(MethodImplOptions.Synchronized), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Function RegisterClient(ByVal ID As Guid, ByVal RemotingClient As IClient) As Boolean Implements IServer.RegisterClient

            Dim lccClient As LocalClient

            ' Internal clients are *always* allowed to connect and are not counted against server's connected client count
            If ServerClients.Count - ServerClients.InternalCount < ServerMaxClients Or RemotingClient.IsInternalClient Then
                ' See if client is connected to this server instance
                If (New System.Uri(RemotingClient.URI)).Port = ServerTCPPort Then
                    ' Add client to server's local cache
                    Try
                        ServerClients.Add(New LocalClient(ID, RemotingClient, ServerResponseTimeout, ServerQueueInterval))

                        lccClient = ServerClients.Find(ID)
                        If Not lccClient Is Nothing Then
                            RaiseEvent_ClientConnected(lccClient)
                            Return True
                        Else
                            Return False
                        End If
                    Catch
                        Return False
                    End Try
                Else
                    ' If client is not connected to this server instance, but we have an available connection, then we
                    ' update the client's URI to redirect its connection attempts to this server instance instead
                    With New System.Uri(RemotingClient.URI)
                        RemotingClient.UpdateCurrentURI(.Scheme & "://" & .Host & ":" & ServerTCPPort & "/" & ServerURI)
                    End With
                    Return False
                End If
            Else
                ' This server instance has reached its maximum client capacity, see if we can create a new server instance
                ' to handle more client connections...
                If NextServer Is Nothing Then
                    If ServerCount < ServerMaxServers Or ServerMaxServers = -1 Then
                        If ServerNextRef Is Nothing Then
                            CreateNextServer(RemotingClient)
                            Return False
                        Else
                            If ServerNextRef.Connected Then
                                ' Server has been connected now, make client reconnect...
                                Return False
                            Else
                                Throw New RemotingClientNotConnectedException("Waiting for new server connection...")
                            End If
                        End If
                    Else
                        Throw New RemotingServerConnectionsAtCapacityException("Server has reached its maximum client capacity.")
                    End If
                Else
                    ' We've already established the next server and we're out of connections so we pass this client request along the chain
                    Return NextServer.RegisterClient(ID, RemotingClient)
                End If
            End If

        End Function

        Private Sub CreateNextServer(ByVal RemotingClient As IClient)

            Dim ServerStartArgs As New StartArgs
            Dim NewServerArgs As New StringBuilder
            Dim NewPort As Integer = GetNextServerPort()
            Dim NewURI As String = "tcp://localhost:" & NewPort & "/" & ServerURI

            ServerStartArgs.Add("TCPPort", NewPort)
            ServerStartArgs.Add("URI", ServerURI)
            InitializeServerStartArgs(ServerStartArgs)

            With NewServerArgs
                .Append(GetCurrentCodeBase())           ' Arg(0) = Full path to assembly for host to load
                .Append("|")
                .Append(Me.GetType.FullName)            ' Arg(1) = Type name for host to load from assembly
                .Append("|")
                .Append(ServerStartArgs.GetArgList())   ' Arg(2) = Key/value initialization properties
            End With

            ' We call user-overridable delegate to handle starting of new host process...
            CreateServerHost.Invoke(NewServerArgs.ToString(), NewURI)

            ' Go ahead and tell remote client to redirect to this new URI
            RemotingClient.UpdateCurrentURI(NewURI)

            ' Create a new internal client reference to new server host
            ServerNextRef = New ServerClientReference(Me, NewURI)

        End Sub

        Private Sub NextServerReference_ConnectionEstablished() Handles ServerNextRef.ConnectionEstablished

            ' Copy any remaining properties that couldn't be passed via start arguments
            NextServer.BaseServer = BaseServer
            NextServer.CreateServerHostFunction = CreateServerHostFunction
            NextServer.TerminateServerHostFunction = TerminateServerHostFunction

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Sub DefaultCreateServerHost(ByVal StartArgs As String, ByRef NewURI As String)

            Try
                Dim HostExecutable As String = GetRemotingHostFileName()
                Dim HostProcess As Process
                Dim StartInfo As ProcessStartInfo

                StartInfo = New ProcessStartInfo(HostExecutable, StartArgs)
#If DEBUG Then
                StartInfo.WindowStyle = ProcessWindowStyle.Normal
#Else
                StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                StartInfo.CreateNoWindow = True
#End If
                HostProcess = Process.Start(StartInfo)
                HostProcess.Refresh()

                If HostProcesses Is Nothing Then HostProcesses = New Hashtable
                HostProcesses.Add(NewURI, HostProcess.Id)
            Catch ex As Exception
                Throw New RemotingServerFailedToCreateNewHostException("Server failed to create new server host: " & ex.Message, ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Sub DefaultTerminateServerHost(ByVal Port As Integer, ByVal URI As String)

            If Not HostProcesses Is Nothing Then
                Dim HostProcessID As Integer = HostProcesses("tcp://localhost:" & Port & "/" & URI)

                If HostProcessID > 0 Then
                    Dim HostProcess As Process = Process.GetProcessById(HostProcessID)
                    If Not HostProcess Is Nothing Then HostProcess.Kill()
                End If
            End If

        End Sub

        Private Function GetCurrentCodeBase() As String

            Dim strRemotingHost As String = Me.GetType.Assembly.GetExecutingAssembly.CodeBase

            ' Remove "file" prefix from location
            If Left(strRemotingHost, 8) = "file:///" Then strRemotingHost = Mid(strRemotingHost, 9)
            strRemotingHost = Replace(strRemotingHost, "/", "\")

            Return strRemotingHost

        End Function

        Private Function GetRemotingHostFileName() As String

            ' Remove file name from location (code base will be source DLL)
            Return JustPath(GetCurrentCodeBase()) & ExternalRemotingHost

        End Function

        Private Function GetNextServerPort() As Integer

            Dim NextRef As IServer = BaseServer
            Dim MaxPort As Integer = TCPPort

            Do While Not NextRef Is Nothing
                MaxPort = NextRef.TCPPort
                NextRef = NextRef.NextServer
            Loop

            Return MaxPort + 1

        End Function

        ' This function copies all of the typcal "known" server properties, if derived classes define more
        ' properties then they must override this function to copy those as well...
        Protected Overridable Sub InitializeServerStartArgs(ByVal ServerStartArgs As StartArgs)

            With ServerStartArgs
                .Add("HostID", HostID)
                .Add("ResponseTimeout", ResponseTimeout)
                .Add("QueueInterval", QueueInterval)
                .Add("ShutdownTimeout", ShutdownTimeout)
                .Add("MaximumClients", MaximumClients)
                .Add("MaximumServers", MaximumServers)
            End With

        End Sub

        ' This function copies all of the typcal "known" client properties, if derived classes define more
        ' properties then they must override this function to copy those as well...
        Protected Overridable Sub InitializeClientStartArgs(ByVal ClientStartArgs As StartArgs)

            ClientStartArgs.Add("TCPPort", 0)

        End Sub

        ' This function is used to unregister a client (this is typically used by remote clients)
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Function UnregisterClient(ByVal ID As Guid) As Boolean Implements IServer.UnregisterClient

            Dim lccClient As LocalClient

            lccClient = ServerClients.Find(ID)
            If Not lccClient Is Nothing Then
                ServerClients.Remove(lccClient)
                RaiseEvent_ClientDisconnected(lccClient)
                Return True
            Else
                Return False
            End If

        End Function

        ' This function is used to send an event notification to the server (this is typically used by remote clients)
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub SendServerNotification(ByVal ID As Guid, ByVal sender As Object, ByVal e As EventArgs) Implements IServer.SendServerNotification

            Dim lccClient As LocalClient

            lccClient = ServerClients.Find(ID)
            If lccClient Is Nothing Then
                Throw New RemotingClientNotRegisteredException("Cannot process server notifications from unregistered clients.")
            Else
                ' We simply bubble this message up to the server through an event - to the server, this is a
                ' notification coming in from the client - hence the nomenclature change
                RaiseEvent_ClientNotification(lccClient, sender, e)
            End If

        End Sub

        ' This function is used to send an event notification to all clients
        Public Overridable Sub SendNotification(ByVal sender As Object, ByVal e As EventArgs) Implements IServer.SendNotification

            If Not NextServer Is Nothing Then NextServer.SendNotification(sender, e)
            ServerClients.SendNotification(sender, e)

        End Sub

        ' This function is used to send an event notification to a specific client
        Public Overridable Sub SendPrivateNotification(ByVal ID As Guid, ByVal sender As Object, ByVal e As EventArgs) Implements IServer.SendPrivateNotification

            If Not NextServer Is Nothing Then NextServer.SendPrivateNotification(ID, sender, e)
            ServerClients.SendPrivateNotification(ID, sender, e)

        End Sub

        ' Override the lease settings for this object
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function InitializeLifetimeService() As Object

            ' This is to insure that when created as a Singleton, the instance never dies,
            ' no matter how long between client calls
            Dim lease As Lifetime.ILease = MyBase.InitializeLifetimeService()

            If lease.CurrentState = Lifetime.LeaseState.Initial Then
                lease.InitialLeaseTime = TimeSpan.Zero
            End If

            Return lease

        End Function

        Public MustOverride Function CreateClientProviderChain() As IClientChannelSinkProvider

        Public MustOverride Function CreateServerProviderChain() As IServerChannelSinkProvider

        <Browsable(False)> _
        Public Overridable ReadOnly Property ClientAssembly() As String Implements IServer.ClientAssembly
            Get
                Return GetCurrentCodeBase()
            End Get
        End Property

        <Browsable(False)> _
        Public MustOverride ReadOnly Property ClientType() As String Implements IServer.ClientType

        ' These event raising wrappers make sure RPC calls don't stop processing because of end user code
        ' boo-boo's - an event is provided so that the user can know when they've got an issue.
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ServerEstablished(ByVal ServerID As Guid, ByVal TCPPort As Integer) Implements IServer.RaiseEvent_ServerEstablished

            Try
                If ServerBase Is Nothing Then
                    RaiseEvent ServerEstablished(ServerID, TCPPort)
                Else
                    ServerBase.RaiseEvent_ServerEstablished(ServerID, TCPPort)
                End If
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ServerBase::ServerEstablished", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ClientNotification(ByVal Client As LocalClient, ByVal sender As Object, ByVal e As System.EventArgs) Implements IServer.RaiseEvent_ClientNotification

            Try
                If ServerBase Is Nothing Then
                    RaiseEvent ClientNotification(Client, sender, e)
                Else
                    ServerBase.RaiseEvent_ClientNotification(Client, sender, e)
                End If
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ServerBase::ClientNotification", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ClientConnected(ByVal Client As LocalClient) Implements IServer.RaiseEvent_ClientConnected

            Try
                If ServerBase Is Nothing Then
                    RaiseEvent ClientConnected(Client)
                Else
                    ServerBase.RaiseEvent_ClientConnected(Client)
                End If
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ServerBase::ClientConnected", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_ClientDisconnected(ByVal Client As LocalClient) Implements IServer.RaiseEvent_ClientDisconnected

            Try
                If ServerBase Is Nothing Then
                    RaiseEvent ClientDisconnected(Client)
                Else
                    ServerBase.RaiseEvent_ClientDisconnected(Client)
                End If
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("ServerBase::ClientDisconnected", ex)
            End Try

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Protected Sub RaiseEvent_UserEventHandlerException(ByVal EventName As String, ByVal ex As Exception) Implements IServer.RaiseEvent_UserEventHandlerException

            Try
                If ServerBase Is Nothing Then
                    RaiseEvent UserEventHandlerException(EventName, ex)
                Else
                    ServerBase.RaiseEvent_UserEventHandlerException(EventName, ex)
                End If
            Catch
                ' Not stopping for bad code
            End Try

        End Sub

    End Class

End Namespace