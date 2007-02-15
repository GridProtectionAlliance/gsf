' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Serialization.Formatters
Imports TVA.Remoting.SecureProvider
Imports TVA.Remoting.SecureProvider.Common

Namespace Remoting

    <Serializable(), ToolboxBitmap(GetType(SecureServer), "SecureServer.bmp"), DefaultEvent("ClientNotification"), DefaultProperty("HostID")> _
    Public Class SecureServer

        Inherits ServerBase
        Implements IComponent

        <NonSerialized()> Private ServerPrivateKey As String                                            ' Private communications key
        <NonSerialized()> Private ServerNoActivityLimit As Integer                                      ' No activity limit for clients
        <NonSerialized()> Protected WithEvents ServerSinkProvider As SecureServerChannelSinkProvider    ' Secure server side communications sink provider
        <NonSerialized()> Protected WithEvents ServerSink As SecureServerChannelSink                    ' Secure server side communications sink

        Public Event SecureClientConnected(ByVal ID As Guid)        ' This event is optionally handled by the server host to know when a client channel sink has been connected
        Public Event SecureClientAuthenticated(ByVal ID As Guid)    ' This event is optionally handled by the server host to know when a client channel sink has been authenticated

        ' Component Implementation
        Private ComponentSite As ISite
        Public Event Disposed(ByVal sender As Object, ByVal e As EventArgs) Implements IComponent.Disposed

        Public Sub New()

            MyBase.New()
            TCPPort = 8090
            URI = "SecureServerURI"
            ServerPrivateKey = Guid.NewGuid().ToString()
            ServerNoActivityLimit = 600

        End Sub

        Public Sub New(ByVal HostID As String, ByVal URI As String, ByVal TCPPort As Integer, Optional ByVal PrivateKey As String = "", Optional ByVal NoActivityLimit As Integer = 600)

            MyBase.New(HostID, URI, TCPPort)

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
            ServerPrivateKey = PrivateKey
            ServerNoActivityLimit = NoActivityLimit

        End Sub

        <Browsable(True), Category("Authentication"), Description("Specifies the private authentication key to use.  When this value is not blank, only clients that know this key can connect to this server.  Setting this value to blank will create a server that any SecureClient can connect to making the authentication work like SSL.")> _
        Public Property PrivateKey() As String
            Get
                Return ServerPrivateKey
            End Get
            Set(ByVal Value As String)
                ServerPrivateKey = Value
            End Set
        End Property

        <Browsable(True), Category("Authentication"), Description("Specifies tolerance (in seconds) for maintaining a client connection with no activity.  Note: you can set value to -1 to never expire clients or 0 to force client authentication at every RPC request."), DefaultValue(600)> _
        Public Property NoActivityLimit() As Integer
            Get
                Return ServerNoActivityLimit
            End Get
            Set(ByVal Value As Integer)
                ServerNoActivityLimit = Value
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

        ' This will force all clients to reauthenticate - you can call this periodically to make
        ' the communications stream more secure...
        Public Sub ReauthenticateClients()

            If Not ServerSink Is Nothing Then ServerSink.ReauthenticateClients()

        End Sub

        Public Overrides Function CreateClientProviderChain() As IClientChannelSinkProvider

            ' The server acts as a "client" when a secure client sends the server a message
            Return CreateSecureClientProviderChain(ServerPrivateKey)

        End Function

        Public Overrides Function CreateServerProviderChain() As IServerChannelSinkProvider

            ' We hold on to a reference to the actual secure server channel sink provider because
            ' it will notify us when the actual server sink gets created...
            ServerSinkProvider = New SecureServerChannelSinkProvider(ServerPrivateKey, ServerNoActivityLimit)

            Dim formatter As New BinaryServerFormatterSinkProvider

#If FRAMEWORK_1_1 Then
            ' This is new a required RPC security setting in the .NET 1.1 framework...
            formatter.TypeFilterLevel = TypeFilterLevel.Full
#End If
            ServerSinkProvider.Next = formatter

            Return ServerSinkProvider

        End Function

        <Browsable(False)> _
        Public Overrides ReadOnly Property ClientType() As String
            Get
                Return GetType(SecureClient).FullName
            End Get
        End Property

        Private Sub ServerSinkProvider_ServerSinkCreated() Handles ServerSinkProvider.ServerSinkCreated

            Try
                ' Get reference to server sink once it is created
                ServerSink = ServerSinkProvider.SecureSink
            Catch
            End Try

        End Sub

        Private Sub ServerSink_ClientConnected(ByVal Client As SecureServerChannelSink.SecureClient) Handles ServerSink.ClientConnected

            RaiseEvent SecureClientConnected(Client.ID)

        End Sub

        Private Sub ServerSink_ClientAuthenticated(ByVal Client As SecureServerChannelSink.SecureClient) Handles ServerSink.ClientAuthenticated

            RaiseEvent SecureClientAuthenticated(Client.ID)

        End Sub

        Protected Overrides Sub InitializeServerStartArgs(ByVal ServerStartArgs As StartArgs)

            MyBase.InitializeServerStartArgs(ServerStartArgs)

            ' We also initialize relevant start properties for a secure server
            ServerStartArgs.Add("PrivateKey", PrivateKey)
            ServerStartArgs.Add("NoActivityLimit", NoActivityLimit)

        End Sub

        Protected Overrides Sub InitializeClientStartArgs(ByVal ClientStartArgs As StartArgs)

            MyBase.InitializeClientStartArgs(ClientStartArgs)

            ' We also initialize relevant start properties for a secure client
            ClientStartArgs.Add("PrivateKey", PrivateKey)

        End Sub

    End Class

End Namespace