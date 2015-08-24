' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel

Namespace Remoting

    Public Interface IClient

        Property URI() As String                            ' This is the "current" full URI to the remoting server as specified by the server (e.g., tcp://localhost:8091/ServerURI)
        Property TCPPort() As Integer                       ' TCP communications port used by this instance
        Property ConnectionAttemptInterval() As Integer     ' Number of seconds to wait between connection attempts
        Property NoActivityTestInterval() As Integer        ' Number of seconds to wait between activity tests
        Property RestartDelayInterval() As Integer          ' Number of seconds to wait for delayed starts or restarts

        ReadOnly Property Connected() As Boolean            ' Client connection state flag
        ReadOnly Property ConnectTime() As DateTime         ' Time client was connected to host
        ReadOnly Property Description() As String           ' Returns the description of the client
        ReadOnly Property ID() As Guid                      ' Unique ID for the client instance
        ReadOnly Property HostID() As String                ' Host ID, as reported by the server
        ReadOnly Property BaseURI() As String               ' This is the "base" full URI to the remoting server as established by the user (e.g., tcp://localhost:8090/ServerURI)

        Sub Connect(Optional ByVal StartDelay As Boolean = False)                                   ' Start (or restart) connection cycle with optional delay
        Sub Disconnect()                                                                            ' Disconnect client session
        Sub Shutdown()                                                                              ' Dispose of client by disconnecting and terminating communications
        Sub NewActivity()                                                                           ' Called when there is new activity from the host, if no activity has been registered since last no-activity test interval, the connect cycle is restarted
        Sub SendStatusMessage(ByVal Message As String, ByVal NewLine As Boolean)                    ' Raises StatusMessage event to send message to remote client
        Sub SendNotification(ByVal sender As Object, ByVal e As EventArgs)                          ' Send an event message back to the remote server
        Function SendSafeNotification(ByVal sender As Object, ByVal e As EventArgs) As Boolean      ' Send an event message back to the remote server without worrying about possible exceptions
        Function ServerIsResponding(Optional ByVal Timeout As Integer = 5) As Boolean               ' Returns True if server is responding (works by sending a test message to the remote server)

        Event ServerNotification(ByVal sender As Object, ByVal e As EventArgs)                      ' This event is handled by the host of the remoting client to pick up any notifications sent from the server
        Event StatusMessage(ByVal Message As String, ByVal NewLine As Boolean)                      ' This event is optionally handled by the host of the remoting client to receive status notifications messages from the client
        Event AttemptingConnection()                                                                ' This event is optionally handled by the host of the remoting client to know when a connection is being attempted
        Event ConnectionAttemptFailed(ByVal ex As Exception)                                        ' This event is optionally handled by the host of the remoting client to know when a connection attempt failed
        Event ConnectionEstablished()                                                               ' This event is optionally handled by the host of the remoting client to know when a connection has been established
        Event ConnectionTerminated()                                                                ' This event is optionally handled by the host of the remoting client to know when a connection is no longer active
        Event UserEventHandlerException(ByVal EventName As String, ByVal ex As Exception)           ' This event is optionally handled by the host of the remoting client to know when an exception has occurred in an event handler

        ' The following are intended for internal use only, so they are marked to be hidden from an editor
        <EditorBrowsable(EditorBrowsableState.Never)> Delegate Function ClientRegistrationFunction() As Boolean                                     ' This is the function signature for a method used to allow other classes to change a client's registration process
        <EditorBrowsable(EditorBrowsableState.Never)> Property RemoteReference() As IServer                                                         ' Reference to remote server instance
        <EditorBrowsable(EditorBrowsableState.Never)> Property RegisterClientFunction() As ClientRegistrationFunction                               ' Function pointer to the client registration function
        <EditorBrowsable(EditorBrowsableState.Never)> Property UnregisterClientFunction() As ClientRegistrationFunction                             ' Function pointer to the client unregistration function
        <EditorBrowsable(EditorBrowsableState.Never)> Sub SendClientNotification(ByVal sender As Object, ByVal e As EventArgs)                      ' This function is used to send an event notification to the client (this is typically used by the remote server)
        <EditorBrowsable(EditorBrowsableState.Never)> Function RemoteRegisterClient(ByVal ID As Guid, ByVal RemotingClient As IClient) As Boolean   ' This function is used to register a client (this typically just delegates to the IServer instance's RegisterClient function)
        <EditorBrowsable(EditorBrowsableState.Never)> Function RemoteUnregisterClient(ByVal ID As Guid) As Boolean                                  ' This function is used to unregister a client (this typically just delegates to the IServer instance's UnregisterClient function)
        <EditorBrowsable(EditorBrowsableState.Never)> Sub UpdateCurrentURI(ByVal URI As String)                                                     ' This function allows server to dynamically change the current client connection URI to help with load balancing
        <EditorBrowsable(EditorBrowsableState.Never)> Function IsInternalClient() As Boolean

    End Interface

End Namespace