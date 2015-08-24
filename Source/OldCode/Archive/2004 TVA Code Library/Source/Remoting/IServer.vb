' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel

Namespace Remoting

    ' The following delegates allow end users to customize server scaling
    Public Delegate Sub CreateServerHostFunction(ByVal StartArgs As String, ByRef NewURI As String)
    Public Delegate Sub TerminateServerHostFunction(ByVal Port As Integer, ByVal URI As String)

    Public Interface IServer

        Property HostID() As String                             ' ID the host identifies itself as, connecting client can validate this
        Property URI() As String                                ' URI to use for remoting server (e.g., would be only "ServerURI" in tcp://localhost:8090/ServerURI)
        Property TCPPort() As Integer                           ' TCP communications port used by this instance
        Property ResponseTimeout() As Integer                   ' Maximum number of milliseconds allowed for remote clients to respond to a notification before timing-out
        Property QueueInterval() As Integer                     ' Maximum number of milliseconds to wait before processing a remote client's notification queue
        Property ShutdownTimeout() As Integer                   ' Maximum number of milliseconds allowed for server shutdown
        Property MaximumClients() As Integer                    ' Maximum number of clients one server will process
        Property MaximumServers() As Integer                    ' Maximum number of servers that will be created to handle new client connections - set to -1 for no limit
        Property BaseServer() As IServer                        ' Reference to the base server in the server chain (the event server instance)
        ReadOnly Property NextServer() As IServer               ' Reference to the next server in the server chain
        ReadOnly Property Clients() As LocalClients             ' Collection of locally cached remote client connections for this server instance
        ReadOnly Property Established() As Boolean              ' Returns True if this server instance is established
        ReadOnly Property ID() As Guid                          ' Unique ID for this server instance
        ReadOnly Property ServerCount() As Integer              ' Total number of server instances in use
        ReadOnly Property AllServersEstablished() As Boolean    ' Returns True if all servers in the server chain are established
        ReadOnly Property ClientAssembly() As String            ' Returns full path to client's assembly
        ReadOnly Property ClientType() As String                ' Returns client's full type name

        ' User overridable functions to create and terminate new server hosts
        Property CreateServerHostFunction() As CreateServerHostFunction
        Property TerminateServerHostFunction() As TerminateServerHostFunction

        Sub Start(Optional ByVal StartDelay As Integer = 0)                                                 ' Establish server connection with optional delay
        Sub Shutdown()                                                                                      ' Terminate server connection
        Sub FlushNotifications()                                                                            ' Flushes messages from each client's notification queue
        Sub SendNotification(ByVal sender As Object, ByVal e As EventArgs)                                  ' This function is used to send an event notification to all clients
        Sub SendPrivateNotification(ByVal ID As Guid, ByVal sender As Object, ByVal e As EventArgs)         ' This function is used to send an event notification to a specific client
        Function FindServer(ByVal ServerID As Guid) As IServer                                              ' Searches the server chain starting with the parent for the given server ID - returns Nothing if not found
        Function FindServer(ByVal ServerID As Guid, ByVal StartingWith As IServer) As IServer               ' Searches the server chain starting where specified for the given server ID - returns Nothing if not found
        Function GetFullClientList() As String                                                              ' Returns the cumulative client list for all servers in the server chain

        Event ServerEstablished(ByVal ServerID As Guid, ByVal TCPPort As Integer)                           ' This event is optionally handled by the server host to know when the remote server instance has been established
        Event ClientNotification(ByVal Client As LocalClient, ByVal sender As Object, ByVal e As EventArgs) ' This event is handled by the server host to pick up any notifications sent from a client
        Event ClientConnected(ByVal Client As LocalClient)                                                  ' This event is optionally handled by the server host to know when clients have successfully connected
        Event ClientDisconnected(ByVal Client As LocalClient)                                               ' This event is optionally handled by the server host to know when clients have disconnected
        Event UserEventHandlerException(ByVal EventName As String, ByVal ex As Exception)                   ' This event is optionally handled by the server host to know when an exception has occurred in an event handler

        ' The following are intended for internal use only, so they are marked to be hidden from an editor
        <EditorBrowsable(EditorBrowsableState.Never)> Function RegisterClient(ByVal ID As Guid, ByVal RemotingClient As IClient) As Boolean         ' This function is used to register a client (this is typically used by remote clients)
        <EditorBrowsable(EditorBrowsableState.Never)> Function UnregisterClient(ByVal ID As Guid) As Boolean                                        ' This function is used to unregister a client (this is typically used by remote clients)
        <EditorBrowsable(EditorBrowsableState.Never)> Function GetServerCount(ByVal StartingWith As IServer) As Integer                             ' This function returns the total number of server instances in use starting where specified
        <EditorBrowsable(EditorBrowsableState.Never)> Sub SendServerNotification(ByVal ID As Guid, ByVal sender As Object, ByVal e As EventArgs)    ' This method is used to send an event notification to the server (this is typically used by remote clients)
        <EditorBrowsable(EditorBrowsableState.Never)> Sub RaiseEvent_ServerEstablished(ByVal ServerID As Guid, ByVal TCPPort As Integer)
        <EditorBrowsable(EditorBrowsableState.Never)> Sub RaiseEvent_ClientNotification(ByVal Client As LocalClient, ByVal sender As Object, ByVal e As System.EventArgs)
        <EditorBrowsable(EditorBrowsableState.Never)> Sub RaiseEvent_ClientConnected(ByVal Client As LocalClient)
        <EditorBrowsable(EditorBrowsableState.Never)> Sub RaiseEvent_ClientDisconnected(ByVal Client As LocalClient)
        <EditorBrowsable(EditorBrowsableState.Never)> Sub RaiseEvent_UserEventHandlerException(ByVal EventName As String, ByVal ex As Exception)

    End Interface

End Namespace