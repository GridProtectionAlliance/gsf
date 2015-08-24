' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Messaging
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.CompilerServices
Imports TVA.Shared.Common
Imports TVA.Shared.Crypto
Imports TVA.Shared.String
Imports TVA.Remoting.SecureProvider.SecureRemotingMessages
Imports TVA.Threading

Namespace Remoting.SecureProvider

    Public Class SecureClientChannelSink

        Inherits BaseChannelSinkWithProperties
        Implements IClientChannelSink

        Public ID As Guid                                   ' Unique client identifier - should not change throughout client lifetime...
        Public PrivateKey As Byte()                         ' Private encryption key - only known to client and server for handshake negotiations
        Public PublicKey As Byte()                          ' Public encryption key - client generated for authentication test
        Public SharedKey As Byte()                          ' Shared encryption key - server generated for secure communications
        Public IsConnected As Boolean                       ' Client connected flag
        Public IsAuthenticated As Boolean                   ' Client authenticated flag
        Public ResponseTimeout As Integer                   ' Maximum time in milliseconds allowed for RPC message response from server
        Private NextSink As IClientChannelSink              ' Next sink in the channel
        Private WithEvents NoActivityTimer As Timers.Timer  ' Client must know to reauthenticate itself after a period of no activity
        Private NoActivityInterval As Integer               ' No activity interval (stored in member variable so value can be negative)
        Private Const DefaultInterval As Integer = 600000   ' Default value for no activity interval

        Public Event ServerConnection()
        Public Event ServerAuthentication()

        Friend Sub New(ByVal NextSink As IClientChannelSink, ByVal PrivateKey As String)

            MyBase.New()

            Me.NextSink = NextSink

            If Len(PrivateKey) > 0 Then Me.PrivateKey = Encoding.ASCII.GetBytes(PrivateKey)

            ID = Guid.NewGuid()
            PublicKey = Encoding.ASCII.GetBytes(GenerateKey())
            ResponseTimeout = 5000
            NoActivityTimer = New Timers.Timer

            With NoActivityTimer
                .AutoReset = True
                .Interval = DefaultInterval
                .Enabled = False
            End With

        End Sub

        ' Number of seconds between activity tests
        Public Property NoActivityTestInterval() As Integer
            Get
                Return NoActivityInterval
            End Get
            Set(ByVal Value As Integer)
                ' Store actual value for no activity interval
                '   A positive value means server will expire clients after specified period of no activity
                '   A value of zero means server is requesting authentication at "every" message processing request
                '   A negative value means server will never expire clients
                NoActivityInterval = Value

                If Value > 0 Then
                    ' We knock off half a millisecond to ensure we are deauthenticated before we are sweeped by the server
                    NoActivityTimer.Interval = NoActivityInterval * 1000 - 500
                    NoActivityTimer.Enabled = True
                Else
                    NoActivityTimer.Interval = DefaultInterval
                    NoActivityTimer.Enabled = False
                End If
            End Set
        End Property

        ' We constantly monitor when we perform any activity with the host, if we've not had any activity in
        ' a set amount of time, we will need to reauthenticate with the host because it will have
        ' removed us from its authenticated client list...
        Public Sub NewActivity()

            If NoActivityInterval > 0 And IsAuthenticated Then
                ' We reset the activity test timer any time there is activity, it should
                ' only run after a preset amount of "no activity"
                NoActivityTimer.Enabled = False
                NoActivityTimer.Enabled = True
            End If

        End Sub

        Public Sub Reauthenticate()

            NoActivityTimer.Enabled = False
            IsConnected = False
            IsAuthenticated = False

        End Sub

        Public Overrides ReadOnly Property Properties() As IDictionary Implements IChannelSinkBase.Properties
            Get
                Return MyBase.Properties
            End Get
        End Property

        Public Function GetRequestStream(ByVal Msg As IMessage, ByVal headers As ITransportHeaders) As Stream Implements IClientChannelSink.GetRequestStream

            Return Nothing

        End Function

        Public ReadOnly Property NextChannelSink() As IClientChannelSink Implements IClientChannelSink.NextChannelSink
            Get
                Return NextSink
            End Get
        End Property

        Public Sub AsyncProcessRequest(ByVal SinkStack As IClientChannelSinkStack, ByVal Msg As IMessage, ByVal RequestHeaders As ITransportHeaders, ByVal RequestStream As Stream) Implements IClientChannelSink.AsyncProcessRequest

            ' Any request being sent to server counts for new activity...
            NewActivity()

            If Connected(Msg) Then
                If Authenticated(Msg) Then
                    ' Create a client execution request in the transport headers
                    CreateClientRequest(RequestHeaders, ClientRequestType.Execution)

                    ' Encrypt request stream headed to server
                    Dim EncryptedStream As Stream = Encrypt(RequestStream, SharedKey, SharedKey, MessageEncryptionLevel)

                    ' Close the existing request stream since it won't be used anymore
                    RequestStream.Close()

                    ' Pass the encrypted stream along to the next sink
                    SinkStack.Push(Me, Nothing)
                    NextSink.AsyncProcessRequest(SinkStack, Msg, RequestHeaders, EncryptedStream)
                Else
                    Throw New SecureRemotingAuthenticationException("Failed to authenticate with secure server")
                End If
            Else
                Throw New SecureRemotingConnectionException("Failed to connect to secure server")
            End If

        End Sub

        Public Sub AsyncProcessResponse(ByVal SinkStack As IClientResponseChannelSinkStack, ByVal State As Object, ByVal ResponseHeaders As ITransportHeaders, ByVal ResponseStream As Stream) Implements IClientChannelSink.AsyncProcessResponse

            If IsConnected And IsAuthenticated Then
                Dim ErrorMessage As String

                ' Check for error response
                CheckForServerErrorResponse(ResponseHeaders)

                ' Decrypt response stream coming in from server
                Dim DecryptedStream As Stream = Decrypt(ResponseStream, SharedKey, SharedKey, MessageEncryptionLevel)

                ' Close the existing response stream since it won't be used anymore
                ResponseStream.Close()

                ' Pass decrypted stream through to the rest of the sinks in the stack
                SinkStack.AsyncProcessResponse(ResponseHeaders, DecryptedStream)

                ' If server's no activity interval is zero, server is requesting authentication at "every" message processing request
                If NoActivityInterval = 0 Then Reauthenticate()
            Else
                Reauthenticate()
            End If

        End Sub

        Public Sub ProcessMessage(ByVal Msg As IMessage, ByVal RequestHeaders As ITransportHeaders, ByVal RequestStream As Stream, ByRef ResponseHeaders As ITransportHeaders, ByRef ResponseStream As Stream) Implements IClientChannelSink.ProcessMessage

            ' Any request being sent to server counts for new activity...
            NewActivity()

            If Connected(Msg) Then
                If Authenticated(Msg) Then
                    If ResponseTimeout <= 0 Then
                        ProcessSecureMessage(Msg, RequestHeaders, RequestStream, ResponseHeaders, ResponseStream)
                    Else
                        ' If requested, we process secure message on a separate thread so we can timeout if request takes too long
                        With New ProcessMessageThread(Me, Msg, RequestHeaders, RequestStream)
                            .Execute(ResponseHeaders, ResponseStream)
                        End With
                    End If

                    ' If server's no activity interval is zero, server is requesting authentication at "every" message processing request
                    If NoActivityInterval = 0 Then Reauthenticate()
                Else
                    Throw New SecureRemotingAuthenticationException("Failed to authenticate with secure server")
                End If
            Else
                Throw New SecureRemotingConnectionException("Failed to connect to secure server")
            End If

        End Sub

        ' This internal class incapsulates the execution thread of an RPC message call
        Private Class ProcessMessageThread

            Inherits ThreadBase

            Private Parent As SecureClientChannelSink
            Private Msg As IMessage
            Private RequestHeaders As ITransportHeaders
            Private RequestStream As Stream
            Private ResponseHeaders As ITransportHeaders
            Private ResponseStream As Stream
            Private ex As Exception

            Public Sub New(ByVal Parent As SecureClientChannelSink, ByVal Msg As IMessage, ByVal RequestHeaders As ITransportHeaders, ByVal RequestStream As Stream)

                Me.Parent = Parent
                Me.Msg = Msg
                Me.RequestHeaders = RequestHeaders
                Me.RequestStream = RequestStream

            End Sub

            Public Sub Execute(ByRef ResponseHeaders As ITransportHeaders, ByRef ResponseStream As Stream)

                Start()

                With WorkerThread
                    If Not .Join(Parent.ResponseTimeout) Then .Abort()
                End With

                If Me.ResponseHeaders Is Nothing Then
                    ResponseHeaders = New TransportHeaders
                Else
                    ResponseHeaders = Me.ResponseHeaders
                End If

                If Me.ResponseStream Is Nothing Then
                    ResponseStream = New MemoryStream
                Else
                    ResponseStream = Me.ResponseStream
                End If

                If Not ex Is Nothing Then Throw New SecureRemotingException("Exception occurred during secure message processing: " & ex.Message, ex)

            End Sub

            Protected Overrides Sub ThreadProc()

                Try
                    Parent.ProcessSecureMessage(Msg, RequestHeaders, RequestStream, ResponseHeaders, ResponseStream)
                Catch ex As Exception
                    Me.ex = ex
                End Try

            End Sub

        End Class

        Friend Function Connected(ByVal Msg As IMessage) As Boolean

            If Not IsConnected Then
                Dim RequestHeaders As New TransportHeaders
                Dim RequestStream As New MemoryStream
                Dim ResponseHeaders As ITransportHeaders
                Dim ResponseStream As Stream
                Dim ErrorMessage As String
                Dim Response As ServerResponse

                ' Create a client connect request in the transport headers
                CreateClientRequest(RequestHeaders, ClientRequestType.ConnectionRequest)

                ' Send the connection request on to the secure server
                NextSink.ProcessMessage(Msg, RequestHeaders, RequestStream, ResponseHeaders, ResponseStream)

                ' Check for error response
                CheckForServerErrorResponse(ResponseHeaders)

                ' Check for connection response encoded with public key
                If ServerReturnedResponse(ResponseHeaders, PublicKey, Response) Then
                    With Response
                        If .Type = ServerResponseType.ConnectionGranted And ID.CompareTo(.ResponseID) = 0 Then
                            ' Connection was granted, lets see if we already have a server generated key
                            If SharedKey Is Nothing Then
                                ' Connection was granted with no existing shared key, store server generated shared key
                                IsConnected = True
                                SharedKey = .Key
                                RaiseEvent ServerConnection()
                            Else
                                ' Because of the way .NET reuses client providers, this provider may process messages
                                ' for many servers, so if we already have a server generated shared key - we request
                                ' that the server use this key instead so that the client is not having to constantly
                                ' reauthenticate between every server call to different servers...
                                RequestHeaders = New TransportHeaders
                                RequestStream = New MemoryStream
                                ResponseHeaders = Nothing
                                ResponseStream = Nothing

                                ' Create new shared key request encoded with server specified shared key
                                CreateNewSharedKeyRequest(RequestHeaders, .Key)

                                ' Send the new shared key request on to the secure server
                                NextSink.ProcessMessage(Msg, RequestHeaders, RequestStream, ResponseHeaders, ResponseStream)

                                ' Check for error response
                                CheckForServerErrorResponse(ResponseHeaders)

                                ' Check for new shared key response
                                If ServerReturnedResponse(ResponseHeaders, SharedKey, Response) Then
                                    With Response
                                        If .Type = ServerResponseType.NewSharedKeyAccepted And ID.CompareTo(.ResponseID) = 0 Then
                                            ' Server granted client a connection and has agreed to use client's existing shared key
                                            IsConnected = True
                                            RaiseEvent ServerConnection()
                                        Else
                                            ' Server returned unexpected response
                                            IsConnected = False
                                            Throw New SecureRemotingConnectionException("Unexpected response received from server connection request - connection attempt failed.")
                                        End If
                                    End With
                                Else
                                    ' Server did not respond
                                    IsConnected = False
                                    Throw New SecureRemotingConnectionException("Server failed to respond to connection request - connection attempt failed.")
                                End If
                            End If
                        Else
                            ' Server returned unexpected response
                            IsConnected = False
                            Throw New SecureRemotingConnectionException("Unexpected response received from server connection request - connection attempt failed.")
                        End If
                    End With
                Else
                    ' Server did not respond
                    IsConnected = False
                    Throw New SecureRemotingConnectionException("Server failed to respond to connection request - connection attempt failed.")
                End If
            End If

            Return IsConnected

        End Function

        Friend Function Authenticated(ByVal Msg As IMessage) As Boolean

            If IsConnected And Not IsAuthenticated Then
                Dim RequestHeaders As New TransportHeaders
                Dim RequestStream As New MemoryStream
                Dim ResponseHeaders As ITransportHeaders
                Dim ResponseStream As Stream
                Dim ErrorMessage As String
                Dim Response As ServerResponse

                ' Create a client authentication request in the transport headers
                CreateClientRequest(RequestHeaders, ClientRequestType.AuthenticationRequest)

                ' Create the actual authentication request header that the server will validate
                CreateAuthenticationRequest(RequestHeaders)

                ' Send the authentication request on to the secure server
                NextSink.ProcessMessage(Msg, RequestHeaders, RequestStream, ResponseHeaders, ResponseStream)

                ' Check for error response
                CheckForServerErrorResponse(ResponseHeaders)

                ' Check for authentication response encoded with shared key
                If ServerReturnedResponse(ResponseHeaders, SharedKey, Response) Then
                    With Response
                        If .Type = ServerResponseType.AuthenticationSucceeded And ID.CompareTo(.ResponseID) = 0 Then
                            ' Client was successfully authenticated
                            IsAuthenticated = True
                            NoActivityTestInterval = CInt(Encoding.ASCII.GetString(.Key))
                            RaiseEvent ServerAuthentication()
                        ElseIf .Type = ServerResponseType.AuthenticationFailed And ID.CompareTo(.ResponseID) = 0 Then
                            ' For whatever reason, server failed to authenticate client
                            Reauthenticate()
                            Throw New SecureRemotingAuthenticationException("Failed to authenticate.")
                        Else
                            ' Server returned unexpected response
                            Reauthenticate()
                            Throw New SecureRemotingAuthenticationException("Unexpected response received from server authentication request - authentication attempt failed.")
                        End If
                    End With
                Else
                    ' Server did not respond
                    Reauthenticate()
                    Throw New SecureRemotingAuthenticationException("Server failed to respond to authentication request - authentication attempt failed.")
                End If
            End If

            Return IsAuthenticated

        End Function

        Friend Sub ProcessSecureMessage(ByVal Msg As IMessage, ByVal RequestHeaders As ITransportHeaders, ByVal RequestStream As Stream, ByRef ResponseHeaders As ITransportHeaders, ByRef ResponseStream As Stream)

            Dim ErrorMessage As String

            ' Create a client execution request in the transport headers
            CreateClientRequest(RequestHeaders, ClientRequestType.Execution)

            ' Encrypt request stream headed to server
            Dim EncryptedStream As Stream = Encrypt(RequestStream, SharedKey, SharedKey, MessageEncryptionLevel)

            ' Close the existing request stream since it won't be used anymore
            RequestStream.Close()

            ' Pass the encrypted message along to the next sink
            NextSink.ProcessMessage(Msg, RequestHeaders, EncryptedStream, ResponseHeaders, ResponseStream)

            ' Check for error response
            CheckForServerErrorResponse(ResponseHeaders)

            ' Decrypt response stream coming in from server
            Dim DecryptedStream As Stream = Decrypt(ResponseStream, SharedKey, SharedKey, MessageEncryptionLevel)

            ' Close the existing response stream since it won't be used anymore
            ResponseStream.Close()

            ' Make sure new decrypted stream gets returned in response stream parameter
            ResponseStream = DecryptedStream

        End Sub

        Private Sub CreateClientRequest(ByVal RequestHeaders As ITransportHeaders, ByVal RequestType As ClientRequestType)

            Dim request As New ClientRequest

            With request
                .RequestID = ID
                .Type = RequestType
                .Key = PublicKey
            End With

            RequestHeaders(RequestInfoHeader) = ClientRequest.Encode(request, PrivateKey)

        End Sub

        Private Sub CreateNewSharedKeyRequest(ByVal RequestHeaders As ITransportHeaders, ByVal EncodeKey As Byte())

            Dim request As New ClientRequest

            ' We encrypt the server's new shared key with the server's existing shared key
            With request
                .RequestID = ID
                .Type = ClientRequestType.NewSharedKeyRequest
                .Key = Encrypt(SharedKey, EncodeKey, EncodeKey, MessageEncryptionLevel)
            End With

            RequestHeaders(RequestInfoHeader) = ClientRequest.Encode(request, PrivateKey)

        End Sub

        Private Sub CreateAuthenticationRequest(ByVal RequestHeaders As ITransportHeaders)

            ' We create authentication request based on server generated shared key
            Dim strKey As String = Encoding.ASCII.GetString(SharedKey)
            Dim intKeyLen As Integer = Len(strKey)
            Dim strAuthPrefix As New String(" "c, intKeyLen)
            Dim strAuthSuffix As New String(" "c, intKeyLen)
            Dim strAuthRequest As String
            Dim chrCurrent As Char
            Dim x, y As Integer

            ' Generate random data
            Randomize()

            For x = 1 To intKeyLen
                Mid(strAuthPrefix, x, 1) = Chr(CInt(Int(Rnd() * 255) + 1))
                Mid(strAuthSuffix, x, 1) = Chr(CInt(Int(Rnd() * 255) + 1))
            Next

            ' We hide the private key and the client ID among random data
            strAuthRequest = strAuthPrefix & strKey & ID.ToString() & strAuthSuffix

            ' Re-seed random number generator to create a repeatable scramble
            Rnd(-1)
            Randomize(SharedKey((GetKeyCycles(SharedKey(0)) Mod intKeyLen) - 1))

            ' Mix up the random data, private key and client ID
            For x = 1 To intKeyLen
                y = CInt(Int(Rnd() * intKeyLen) + 1)
                If x <> y Then
                    chrCurrent = Mid(strAuthRequest, x, 1)
                    Mid(strAuthRequest, x, 1) = Mid(strAuthRequest, y, 1)
                    Mid(strAuthRequest, y, 1) = chrCurrent
                End If
            Next

            ' Return encrypted authentication request
            RequestHeaders(RequestAuthHeader) = Encrypt(strAuthRequest, strKey, (SharedKey(0) Mod 3) + 1)

        End Sub

        Private Sub CheckForServerErrorResponse(ByVal ResponseHeaders As ITransportHeaders)

            Dim Response As ServerErrorResponse = GetServerErrorResponse(ResponseHeaders(ResponseErrorHeader), PrivateKey)

            If Not Response Is Nothing Then
                ' Force client to reauthenticate when the server returns an error
                Reauthenticate()

                With Response
                    ' Throw appropriate exception as specified by server error response
                    Select Case .Type
                        Case ServerErrorResponseType.RequestUnidentified
                            Throw New SecureRemotingRequestUnidentifiedException(.Message)
                        Case ServerErrorResponseType.ClientUnidentified
                            Throw New SecureRemotingClientUnidentifiedException(.Message)
                        Case ServerErrorResponseType.ClientUnauthenticated
                            Throw New SecureRemotingClientUnauthenticatedException(.Message)
                        Case Else
                            Throw New SecureRemotingException(.Message)
                    End Select
                End With
            End If

        End Sub

        Private Function ServerReturnedResponse(ByVal ResponseHeaders As ITransportHeaders, ByVal DecodeKey As Byte(), ByRef Response As ServerResponse) As Boolean

            Response = GetServerResponse(ResponseHeaders(ResponseInfoHeader), DecodeKey)
            Return (Not Response Is Nothing)

        End Function

        Private Function GetServerResponse(ByVal Response As String, ByVal DecodeKey As Byte()) As ServerResponse

            If Len(Response) > 0 Then
                Try
                    Return ServerResponse.Decode(Response, DecodeKey)
                Catch ex As Exception
                    Throw New SecureRemotingException("Failed to decode server response received from client request due to exception: " & ex.Message, ex)
                End Try
            End If

            Return Nothing

        End Function

        Private Function GetServerErrorResponse(ByVal Response As String, ByVal DecodeKey As Byte()) As ServerErrorResponse

            If Len(Response) > 0 Then
                Try
                    Return ServerErrorResponse.Decode(Response, DecodeKey)
                Catch ex As Exception
                    Throw New SecureRemotingException("Failed to decode server error response received from client request due to exception: " & ex.Message, ex)
                End Try
            End If

            Return Nothing

        End Function

        Private Sub NoActivityTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles NoActivityTimer.Elapsed

            Reauthenticate()

        End Sub

    End Class

End Namespace