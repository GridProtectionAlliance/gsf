' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Serialization.Formatters
Imports TVA.Remoting.SecureProvider
Imports TVA.Remoting.SecureProvider.Common

Namespace Remoting

    <Serializable(), ToolboxBitmap(GetType(SecureClient), "SecureClient.bmp"), DefaultEvent("ServerNotification"), DefaultProperty("URI")> _
    Public Class SecureClient

        Inherits ClientBase
        Implements IComponent

        ' Note that .NET only uses one provider sink per process
        <NonSerialized()> Private ClientPrivateKey As String                                            ' Private communications key
        <NonSerialized()> Protected WithEvents ClientSinkProvider As SecureClientChannelSinkProvider    ' Secure client side communications sink provider
        <NonSerialized()> Protected WithEvents ClientSink As SecureClientChannelSink                    ' Secure client side communications sink
        <NonSerialized()> Private ResponseTimeout As Integer = 5000                                     ' Cached server response timeout pushed to sink

        Public Event SecureServerConnection()       ' This event is optionally handled by the client host to know when the client channel sink has been connected
        Public Event SecureServerAuthentication()   ' This event is optionally handled by the client host to know when the client channel sink has been authenticated

        ' Component Implementation
        Private ComponentSite As ISite
        Public Event Disposed(ByVal sender As Object, ByVal e As EventArgs) Implements IComponent.Disposed

        Public Sub New()

            MyBase.New()
            GetClientSinkReference()
            URI = "tcp://localhost:8090/SecureServerURI"

        End Sub

        Public Sub New(ByVal URI As String, Optional ByVal PrivateKey As String = "", Optional ByVal TCPPort As Integer = 0)

            MyBase.New(URI, TCPPort)
            GetClientSinkReference()

            ' Define the private key only known to host and client applications.  Not specifying
            ' this key means that initial handshake negotiations will lack an additional layer of
            ' security allowing any client using this library to connect to the host so long as
            ' it doesn't specify a PrivateKey.  This makes the secure handshake negotiation work
            ' like a web browser trying to connect to a secure server over SSL - the PrivateKey
            ' is not a required parameter in case this is the actual desired behavior.  Specifying
            ' a key makes this process more secure by first encrypting the public key negotiation
            ' with a private key (the process is still encrypted if no private key is specified,
            ' it just uses an internal key) thereby making it more difficult to intercept the shared
            ' key negotiations and secondly only allows connections by clients that actually know
            ' the private key.  Also, it should be noted that .NET will only create a single provider
            ' per process - so this will force users to use the same private key for *all* secure
            ' servers and clients that will be running within the same process.
            ClientPrivateKey = PrivateKey

        End Sub

        Public Overrides Sub Connect(Optional ByVal StartDelay As Boolean = False)

            Reauthenticate()
            MyBase.Connect(StartDelay)

        End Sub

        Public Overrides Sub Disconnect()

            Reauthenticate()
            MyBase.Disconnect()

        End Sub

        <Browsable(True), Category("Authentication"), Description("Specifies the private authentication key to use.")> _
        Public Property PrivateKey() As String
            Get
                Return ClientPrivateKey
            End Get
            Set(ByVal Value As String)
                ClientPrivateKey = Value
            End Set
        End Property

        Public Overrides Sub Shutdown()

            MyBase.Shutdown()
            RaiseEvent Disposed(Me, EventArgs.Empty)

        End Sub

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overridable Overloads Property Site() As ISite Implements IComponent.Site
            Get
                Return ComponentSite
            End Get
            Set(ByVal Value As ISite)
                ComponentSite = Value
            End Set
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return "Secure " & MyBase.Description
            End Get
        End Property

        ' We override the standard send notification routine to handle situtations where the secure server
        ' may have removed the client from its secure client list (for whatever reason) - in this case, we
        ' will want to "retry" the notification so that the client will automatically reauthenticate
        Public Overrides Sub SendNotification(ByVal sender As Object, ByVal e As EventArgs)

            Try
                MyBase.SendNotification(sender, e)
            Catch ex As SecureRemotingClientUnauthenticatedException
                ' If client was unauthenticated it might have been swept by server, so we'll try again...
                RetryNotification(sender, e, 2)
            Catch ex As SecureRemotingClientUnidentifiedException
                ' If client was unidentified it might have been swept by server, so we'll try again...
                RetryNotification(sender, e, 2)
            Catch
                ' For other errors we'll just manually reauthenticate and try again...
                Reauthenticate()
                RetryNotification(sender, e, 2)
            End Try

        End Sub

        Private Sub RetryNotification(ByVal sender As Object, ByVal e As EventArgs, ByVal Attempt As Integer)

            Try
                MyBase.SendNotification(sender, e)
            Catch ex As SecureRemotingClientUnauthenticatedException
                If Attempt < 3 Then
                    ' If client was unauthenticated it might have been swept by server, so we'll try again...
                    RetryNotification(sender, e, Attempt + 1)
                Else
                    ' Give up after three tries
                    Throw New SecureRemotingClientUnauthenticatedException(ex.Message & " - retried 3 times...", ex)
                End If
            Catch ex As SecureRemotingClientUnidentifiedException
                If Attempt < 3 Then
                    ' If client was unidentified it might have been swept by server, so we'll try again...
                    RetryNotification(sender, e, Attempt + 1)
                Else
                    ' Give up after three tries
                    Throw New SecureRemotingClientUnidentifiedException(ex.Message & " - retried 3 times...", ex)
                End If
            Catch
                If Attempt < 3 Then
                    ' For other errors we'll just manually reauthenticate and try again...
                    Reauthenticate()
                    RetryNotification(sender, e, Attempt + 1)
                Else
                    ' After three tries, all unrecoverable exceptions just get rethrown
                    Throw
                End If
            End Try

        End Sub

        <Browsable(True), Category("Event Processing"), Description("Maximum number of milliseconds to wait for a server response - set to zero to wait indefinately"), DefaultValue(5000)> _
        Public Property ServerResponseTimeout() As Integer
            Get
                Return ResponseTimeout
            End Get
            Set(ByVal Value As Integer)
                ResponseTimeout = Value               
                If Not ClientSink Is Nothing Then ClientSink.ResponseTimeout = Value
            End Set
        End Property

        Public Sub Reauthenticate()

            If Not ClientSink Is Nothing Then ClientSink.Reauthenticate()

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub UpdateCurrentURI(ByVal URI As String)

            Reauthenticate()
            MyBase.UpdateCurrentURI(URI)

        End Sub

        Public Overrides Function CreateClientProviderChain() As IClientChannelSinkProvider

            Dim chain As New BinaryClientFormatterSinkProvider

            ' We hold on to a reference to the actual secure client channel sink provider because
            ' it will notify us when the actual client sink gets created...
            ClientSinkProvider = New SecureClientChannelSinkProvider(ClientPrivateKey)

            chain.Next = ClientSinkProvider

            Return chain

        End Function

        Public Overrides Function CreateServerProviderChain() As IServerChannelSinkProvider

            ' When communicating with the server (i.e., client sends a message to the server), the
            ' client becomes the "server" - since the remote server will be the only "client" in
            ' this relationship, we specify that we never want to the server to "sweep" its clients
            ' after a period of no activity - so we set the NoActivityLimit to -1 (never expire).
            Return CreateSecureServerProviderChain(ClientPrivateKey, -1)

        End Function

        Private Sub GetClientSinkReference() Handles ClientSinkProvider.ClientSinkCreated

            Try
                ' Get reference to client sink once it is created
                If Not ClientSink Is Nothing Then
                    ClientSink = SecureClientChannelSinkProvider.SecureSink
                    ClientSink.ResponseTimeout = ResponseTimeout
                End If
            Catch
            End Try

        End Sub

        Private Sub ClientSink_ServerConnection() Handles ClientSink.ServerConnection

            Try
                RaiseEvent SecureServerConnection()
            Catch
            End Try

        End Sub

        Private Sub ClientSink_ServerAuthentication() Handles ClientSink.ServerAuthentication

            Try
                RaiseEvent SecureServerAuthentication()
            Catch
            End Try

        End Sub

        ' We add ourselves as a client to the base class connect events so that we can let the
        ' secure client know that it needs to reauthenticate - we do this just to prevent the
        ' extra RPC cycle needed when a client discovers that it is no longer authenticated
        Private Sub SecureClient_AttemptingConnection() Handles MyBase.AttemptingConnection

            Reauthenticate()

        End Sub

        Private Sub SecureClient_ConnectionAttemptFailed(ByVal ex As Exception) Handles MyBase.ConnectionAttemptFailed

            Reauthenticate()

        End Sub

    End Class

End Namespace