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
Imports VB = Microsoft.VisualBasic

Namespace Remoting.SecureProvider

    Public Class SecureServerChannelSink

        Inherits BaseChannelSinkWithProperties
        Implements IServerChannelSink

        Public Clients As SecureClients         ' Secure client list
        Private NextSink As IServerChannelSink  ' Next sink in the channel
        Private PrivateKey As Byte()            ' Private encryption key - only known to client and sever for handshake negotiations
        Private NoActivityLimit As Integer      ' Maximum number of seconds to allow client to be authenticated with no activity

        Public Event ClientConnected(ByVal Client As SecureClient)
        Public Event ClientAuthenticated(ByVal Client As SecureClient)

        Public Class SecureClient

            Implements IComparable

            Public ID As Guid                   ' Client ID
            Public SharedKey As Byte()          ' Shared encryption key - server generated for secure communications
            Public Authenticated As Boolean     ' Determines if client has been authenticated
            Public LastActivity As Double       ' Tracks last client activity time

            Friend Sub New(ByVal ID As Guid, Optional ByVal SharedKey As Byte() = Nothing)

                Me.ID = ID
                Me.SharedKey = SharedKey

            End Sub

            Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

                ' Client are sorted by their ID's
                If TypeOf obj Is SecureClient Then
                    Return ID.CompareTo(DirectCast(obj, SecureClient).ID)
                Else
                    Throw New ArgumentException("SecureClient can only be compared to other SecureClients")
                End If

            End Function

            Public Sub NewActivity()

                LastActivity = VB.Timer

            End Sub

        End Class

        ' Threading is a real concern for this class - make sure to synclock base collection as necessary...
        Public Class SecureClients

            Private Parent As SecureServerChannelSink       ' Parent sink
            Private Clients As ArrayList                    ' List of secure clients
            Private WithEvents Sweeper As Timers.Timer      ' Periodic dead client remover

            Friend Sub New(ByVal ParentSink As SecureServerChannelSink)

                Parent = ParentSink
                Clients = New ArrayList()
                Sweeper = New Timers.Timer()

                With Sweeper
                    .AutoReset = False
                    .Interval = 60000
                    .Enabled = False
                End With

            End Sub

            Public Sub Add(ByVal Value As SecureClient)

                Dim intIndex As Integer

                SyncLock Clients.SyncRoot
                    intIndex = Clients.BinarySearch(Value)
                    If intIndex >= 0 Then Clients.RemoveAt(intIndex)
                    Clients.Add(Value)
                    Clients.Sort()
                End SyncLock

                Sweeper.Enabled = (Parent.NoActivityLimit >= 0)

            End Sub

            Default Public ReadOnly Property Item(ByVal Index As Integer) As SecureClient
                Get
                    Dim secClient As SecureClient

                    SyncLock Clients.SyncRoot
                        If Index > -1 And Index < Clients.Count Then
                            secClient = DirectCast(Clients.Item(Index), SecureClient)
                        End If
                    End SyncLock

                    Return secClient
                End Get
            End Property

            Public Function Find(ByVal ID As Guid) As SecureClient

                Dim intIndex As Integer

                SyncLock Clients.SyncRoot
                    intIndex = Clients.BinarySearch(New SecureClient(ID))
                End SyncLock

                If intIndex < 0 Then
                    Return Nothing
                Else
                    Return Me(intIndex)
                End If

            End Function

            Public ReadOnly Property Count() As Integer
                Get
                    Dim intCount As Integer

                    SyncLock Clients.SyncRoot
                        intCount = Clients.Count
                    End SyncLock

                    Return intCount
                End Get
            End Property

            Public Sub Clear()

                SyncLock Clients.SyncRoot
                    Clients.Clear()
                End SyncLock

            End Sub

            ' We periodically remove any client that has had no activity in a while - this essentially "deauthenticates" the client
            <MethodImpl(MethodImplOptions.Synchronized)> _
            Private Sub Sweeper_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles Sweeper.Elapsed

                Dim colDeadClients As New ArrayList()
                Dim x As Integer

                SyncLock Clients.SyncRoot
                    For x = 0 To Clients.Count - 1
                        SyncLock Clients(x)
                            If DirectCast(Clients(x), SecureClient).LastActivity + Parent.NoActivityLimit < VB.Timer Then
                                colDeadClients.Add(x)
                            End If
                        End SyncLock
                    Next

                    If colDeadClients.Count > 0 Then
                        colDeadClients.Sort()

                        ' Remove clients in reverse order to preserve index integrity
                        For x = colDeadClients.Count - 1 To 0 Step -1
                            Clients.RemoveAt(colDeadClients(x))
                        Next
                    End If

                    ' Keep sweeper alive so long as there are active clients
                    Sweeper.Enabled = (Clients.Count > 0)
                End SyncLock

            End Sub

        End Class

        Friend Sub New(ByVal NextSink As IServerChannelSink, ByVal PrivateKey As String, ByVal NoActivityLimit As Integer)

            MyBase.New()

            Me.NextSink = NextSink

            If Len(PrivateKey) > 0 Then Me.PrivateKey = Encoding.ASCII.GetBytes(PrivateKey)

            Me.NoActivityLimit = NoActivityLimit

            Clients = New SecureClients(Me)

        End Sub

        Public Overrides ReadOnly Property Properties() As IDictionary Implements IChannelSinkBase.Properties
            Get
                Return MyBase.Properties
            End Get
        End Property

        Public Function GetResponseStream(ByVal sinkStack As IServerResponseChannelSinkStack, ByVal state As Object, ByVal Msg As IMessage, ByVal headers As ITransportHeaders) As Stream Implements IServerChannelSink.GetResponseStream

            Return Nothing

        End Function

        Public ReadOnly Property NextChannelSink() As IServerChannelSink Implements IServerChannelSink.NextChannelSink
            Get
                Return NextSink
            End Get
        End Property

        Public Sub AsyncProcessResponse(ByVal sinkStack As IServerResponseChannelSinkStack, ByVal state As Object, ByVal Msg As IMessage, ByVal headers As ITransportHeaders, ByVal stream As Stream) Implements IServerChannelSink.AsyncProcessResponse

            ' Asynchronous processing is not supported on the server in custom channel sinks
            Throw New NotSupportedException()

        End Sub

        Public Function ProcessMessage(ByVal SinkStack As IServerChannelSinkStack, ByVal RequestMsg As IMessage, ByVal RequestHeaders As ITransportHeaders, ByVal RequestStream As Stream, ByRef ResponseMsg As IMessage, ByRef ResponseHeaders As ITransportHeaders, ByRef ResponseStream As Stream) As ServerProcessing Implements IServerChannelSink.ProcessMessage

            Dim ProcessingResponse As ServerProcessing = ServerProcessing.Complete

            ' We return all response messages in the transport headers
            ResponseMsg = Nothing
            ResponseStream = New MemoryStream
            ResponseHeaders = New TransportHeaders

            With ClientRequest.Decode(RequestHeaders(RequestInfoHeader), PrivateKey)
                Select Case .Type
                    Case ClientRequestType.ConnectionRequest
                        ' Client is requesting a connection
                        CreateServerConnectionResponse(.RequestID, .Key, ResponseHeaders)
                    Case ClientRequestType.NewSharedKeyRequest
                        ' Client is requesting to use an existing shared key
                        CreateNewSharedKeyResponse(.RequestID, .Key, ResponseHeaders)
                    Case ClientRequestType.AuthenticationRequest
                        ' Client is requesting authentication
                        CreateServerAuthenticationResponse(.RequestID, RequestHeaders(RequestAuthHeader), ResponseHeaders)
                    Case ClientRequestType.Execution
                        ' Client is requesting message processing
                        ProcessingResponse = ProcessSecureMessage(.RequestID, SinkStack, RequestMsg, RequestHeaders, RequestStream, ResponseMsg, ResponseHeaders, ResponseStream)
                    Case Else
                        ' Unidentified client request
                        CreateServerErrorResponse(.RequestID, "Unidentified client request encountered by secure provider: " & .Type, ServerErrorResponseType.RequestUnidentified, ResponseHeaders)
                End Select
            End With

            Return ProcessingResponse

        End Function

        Private Sub CreateServerConnectionResponse(ByVal RequestID As Guid, ByVal PublicKey As Byte(), ByVal ResponseHeaders As ITransportHeaders)

            Try
                Dim secClient As New SecureClient(RequestID, Encoding.ASCII.GetBytes(GenerateKey()))
                Dim svrResponse As New ServerResponse

                ' Create response message
                With svrResponse
                    .ResponseID = RequestID
                    .Type = ServerResponseType.ConnectionGranted
                    .Key = secClient.SharedKey
                End With

                ' Track new client information
                Clients.Add(secClient)
                RaiseEvent ClientConnected(secClient)

                ResponseHeaders(ResponseInfoHeader) = ServerResponse.Encode(svrResponse, PublicKey)
            Catch ex As Exception
                CreateServerErrorResponse(RequestID, "Failed to create connection response: " & ex.Message, ServerErrorResponseType.Undetermined, ResponseHeaders)
            End Try

        End Sub

        Private Sub CreateNewSharedKeyResponse(ByVal RequestID As Guid, ByVal NewSharedKey As Byte(), ByVal ResponseHeaders As ITransportHeaders)

            Dim secClient As SecureClient
            Dim svrResponse As New ServerResponse

            Try
                ' See if client requesting to use an existing shared key has previously been granted a connection
                secClient = Clients.Find(RequestID)
                If Not secClient Is Nothing Then
                    SyncLock secClient
                        With svrResponse
                            .ResponseID = RequestID
                            .Type = ServerResponseType.NewSharedKeyAccepted
                        End With

                        ' Decrypt and store new shared key to use for client
                        secClient.SharedKey = Decrypt(NewSharedKey, secClient.SharedKey, secClient.SharedKey, MessageEncryptionLevel)

                        ' Encode the response using the new shared key
                        ResponseHeaders(ResponseInfoHeader) = ServerResponse.Encode(svrResponse, secClient.SharedKey)
                    End SyncLock
                Else
                    CreateServerErrorResponse(RequestID, "Server refused to use client's existing shared key - client was not known to the server.", ServerErrorResponseType.ClientUnidentified, ResponseHeaders)
                End If
            Catch ex As Exception
                CreateServerErrorResponse(RequestID, "Failed to create new shared key response: " & ex.Message, ServerErrorResponseType.Undetermined, ResponseHeaders)
            End Try

        End Sub

        Private Sub CreateServerAuthenticationResponse(ByVal RequestID As Guid, ByVal AuthRequest As String, ByVal ResponseHeaders As ITransportHeaders)

            Dim secClient As SecureClient
            Dim svrResponse As New ServerResponse

            Try
                With svrResponse
                    .ResponseID = RequestID
                    .Type = ServerResponseType.AuthenticationFailed
                End With

                ' See if client requesting authentication has previously been granted a connection
                secClient = Clients.Find(RequestID)
                If Not secClient Is Nothing Then
                    SyncLock secClient
                        ' See if client knew how to create a valid authentication request
                        If ValidateAuthenticationRequest(secClient, AuthRequest) Then
                            svrResponse.Type = ServerResponseType.AuthenticationSucceeded
                            svrResponse.Key = Encoding.ASCII.GetBytes(NoActivityLimit.ToString())
                            secClient.Authenticated = True
                            secClient.NewActivity()
                            RaiseEvent ClientAuthenticated(secClient)
                        End If
                    End SyncLock
                End If

                ResponseHeaders(ResponseInfoHeader) = ServerResponse.Encode(svrResponse, secClient.SharedKey)
            Catch ex As Exception
                CreateServerErrorResponse(RequestID, "Failed to create authentication response: " & ex.Message, ServerErrorResponseType.Undetermined, ResponseHeaders)
            End Try

        End Sub

        Private Sub CreateServerErrorResponse(ByVal RequestID As Guid, ByVal Message As String, ByVal ExceptionType As ServerErrorResponseType, ByVal ResponseHeaders As ITransportHeaders)

            Try
                Dim svrResponse As New ServerErrorResponse

                ' Create response message
                With svrResponse
                    .ResponseID = RequestID
                    .Type = ExceptionType
                    .Message = Message
                End With

                ResponseHeaders(ResponseErrorHeader) = ServerErrorResponse.Encode(svrResponse, PrivateKey)
            Catch ex As Exception
                ' Ouch...
                Throw New SecureRemotingException("Failed to create server error response: " & ex.Message)
            End Try

        End Sub

        Private Function ProcessSecureMessage(ByVal RequestID As Guid, ByVal SinkStack As IServerChannelSinkStack, ByVal RequestMsg As IMessage, ByVal RequestHeaders As ITransportHeaders, ByVal RequestStream As Stream, ByRef ResponseMsg As IMessage, ByRef ResponseHeaders As ITransportHeaders, ByRef ResponseStream As Stream) As ServerProcessing

            Dim ProcessingResponse As ServerProcessing = ServerProcessing.Complete
            Dim secClient As SecureClient

            Try
                ' See if client requesting authentication has previously been granted a connection and has been authenticated
                secClient = Clients.Find(RequestID)
                If Not secClient Is Nothing Then
                    SyncLock secClient
                        With secClient
                            If .Authenticated Then
                                ' Track new activity for the client
                                .NewActivity()

                                ' Decrypt request stream coming in from client
                                Dim DecryptedStream As Stream = Decrypt(RequestStream, .SharedKey, .SharedKey, MessageEncryptionLevel)

                                ' Close the existing request stream since it won't be used anymore
                                RequestStream.Close()

                                ' Pass the decrypted message along to the next sink
                                ProcessingResponse = NextSink.ProcessMessage(SinkStack, RequestMsg, RequestHeaders, DecryptedStream, ResponseMsg, ResponseHeaders, ResponseStream)

                                ' Encrypt response stream headed back to client
                                Dim EncryptedStream As Stream = Encrypt(ResponseStream, .SharedKey, .SharedKey, MessageEncryptionLevel)

                                ' Close the existing response stream since it won't be used anymore
                                ResponseStream.Close()

                                ' Make sure new encrypted stream gets returned in response stream parameter
                                ResponseStream = EncryptedStream
                            Else
                                CreateServerErrorResponse(RequestID, "Client request for secured process message execution was denied - client has not been authenticated.", ServerErrorResponseType.ClientUnauthenticated, ResponseHeaders)
                            End If
                        End With
                    End SyncLock
                Else
                    CreateServerErrorResponse(RequestID, "Client request for secured process execution was denied - client was not known to the server.", ServerErrorResponseType.ClientUnidentified, ResponseHeaders)
                End If
            Catch ex As Exception
                CreateServerErrorResponse(RequestID, "Failed to process secure message: " & ex.Message, ServerErrorResponseType.Undetermined, ResponseHeaders)
            End Try

            Return ProcessingResponse

        End Function

        Private Function ValidateAuthenticationRequest(ByVal Client As SecureClient, ByVal AuthRequest As String) As Boolean

            ' Authenticate client...
            Dim strKey As String = Encoding.ASCII.GetString(Client.SharedKey)
            Dim intKeyLen As Integer = Len(strKey)
            Dim intSequence As Integer()
            Dim chrCurrent As Char
            Dim x, y As Integer

            ' Decrypt authentication request...
            AuthRequest = Decrypt(AuthRequest, strKey, (Client.SharedKey(0) Mod 3) + 1)

            ' Re-seed random number generator to create repeatable descramble
            Rnd(-1)
            Randomize(Client.SharedKey((GetKeyCycles(Client.SharedKey(0)) Mod intKeyLen) - 1))
            ReDim intSequence(intKeyLen)

            For x = 0 To intKeyLen - 1
                intSequence(x) = CInt(Int(Rnd() * intKeyLen) + 1)
            Next

            For x = intKeyLen To 1 Step -1
                y = intSequence(x - 1)
                If x <> y Then
                    chrCurrent = Mid(AuthRequest, x, 1)
                    Mid(AuthRequest, x, 1) = Mid(AuthRequest, y, 1)
                    Mid(AuthRequest, y, 1) = chrCurrent
                End If
            Next

            ' Validate private key
            If StrComp(Mid(AuthRequest, intKeyLen + 1, intKeyLen), strKey, CompareMethod.Binary) = 0 Then
                ' Validate authentication request ID
                If Client.ID.CompareTo(New Guid(Mid(AuthRequest, intKeyLen * 2 + 1, Len(AuthRequest) - intKeyLen * 3))) = 0 Then
                    ' Authentication succeeded
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If

        End Function

        Public Sub ReauthenticateClients()

            ' Clearing client list will force all clients to reauthenticate
            Clients.Clear()

        End Sub

    End Class

End Namespace