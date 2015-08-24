' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encoding
Imports System.Threading
Imports System.ComponentModel
Imports System.Drawing
Imports TVA.Threading

Namespace Utilities.Web

    <ToolboxBitmap(GetType(Server), "Server.bmp"), DefaultProperty("Port"), DefaultEvent("ProcessRequest")> _
    Public Class Server

        Inherits Component

        Private serverPort As Integer
        Private serverIsRunning As Boolean
        Private serverTextEncoding As Encoding
        Private serverThreads As Hashtable
        Private Const bufferSize As Integer = 524288 ' 512Kb buffer

        Public Enum HttpResponseCode
            Continue = 100
            SwitchingProtocols = 101
            OK = 200
            Created = 201
            Accepted = 202
            NonAuthoritativeInformation = 203
            NoContent = 204
            ResetContent = 205
            PartialContent = 206
            MultipleChoices = 300
            MovedPermanently = 301
            Found = 302
            SeeOther = 303
            NotModified = 304
            UseProxy = 305
            Reserved = 306
            TemporaryRedirect = 307
            BadRequest = 400
            Unauthorized = 401
            PaymentRequired = 402
            Forbidden = 403
            NotFound = 404
            MethodNotAllowed = 405
            NotAcceptable = 406
            ProxyAuthenticationRequired = 407
            RequestTimeout = 408
            Conflict = 409
            Gone = 410
            LengthRequired = 411
            PreconditionFailed = 412
            RequestEntityTooLarge = 413
            RequestURITooLong = 414
            UnsupportedMediaType = 415
            RequestedRangeNotSatisfiable = 416
            ExpectationFailed = 417
            InternalServerError = 500
            NotImplemented = 501
            BadGateway = 502
            ServiceUnavailable = 503
            GatewayTimeout = 504
            HttpVersionNotSupported = 505
        End Enum

        Public Class HttpHeaders

            Implements IEnumerable

            Dim headers As Hashtable

            Public Sub New()

                headers = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)

            End Sub

            Public Sub Add(ByVal name As String, ByVal value As String)

                headers.Add(name, value)

            End Sub

            Default Public Property Item(ByVal name As String) As String
                Get
                    Return headers(name)
                End Get
                Set(ByVal Value As String)
                    headers(name) = Value
                End Set
            End Property

            Public Function NameExists(ByVal name As String) As Boolean

                Return headers.ContainsKey(name)

            End Function

            Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

                Return headers.GetEnumerator()

            End Function

            Public ReadOnly Property Names() As ICollection
                Get
                    Return headers.Keys
                End Get
            End Property

            Public ReadOnly Property Values() As ICollection
                Get
                    Return headers.Values
                End Get
            End Property

            Public ReadOnly Property SyncRoot() As Object
                Get
                    Return headers.SyncRoot
                End Get
            End Property

        End Class

        Public Class HttpRequest

            Public RequestedURI As String
            Public Headers As HttpHeaders
            Public Content As MemoryStream

            Public Sub New()

                Me.Content = New MemoryStream

            End Sub

        End Class

        Public Class HttpResponse

            Public Version As String
            Public Code As HttpResponseCode
            Public Headers As HttpHeaders
            Public Content As Stream

            Public Sub New()

                Me.New(HttpResponseCode.Accepted)

            End Sub

            Public Sub New(ByVal Code As HttpResponseCode)

                Me.Version = "HTTP/1.1"
                Me.Code = Code
                Me.Headers = New HttpHeaders

                With Me.Headers
                    .Add("Server", "TVA.NET Web Server/1.0")
                    .Add("Content-Length", 0)
                End With

            End Sub

            Public Property ContentLength() As Long
                Get
                    If Not Headers.NameExists("Content-Length") Then Headers.Add("", 0)
                    Return Headers("Content-Length")
                End Get
                Set(ByVal Value As Long)
                    If Not Headers.NameExists("Content-Length") Then
                        Headers.Add("", Value)
                    Else
                        Headers("Content-Length") = Value
                    End If
                End Set
            End Property

            Friend ReadOnly Property ResponseHeader() As String
                Get
                    With New StringBuilder
                        .Append("HTTP/1.1 ")
                        .Append(Code)
                        .Append(" "c)
                        .Append([Enum].GetName(GetType(HttpResponseCode), Code))
                        .Append(vbCrLf)
                        For Each header As DictionaryEntry In Headers
                            .Append(header.Key)
                            .Append(": ")
                            .Append(header.Value)
                            .Append(vbCrLf)
                        Next
                        .Append("Date: ")
                        .Append(DateTime.Now.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss"))
                        .Append(" GMT")
                        .Append(vbCrLf)
                        .Append(vbCrLf)
                        Return .ToString()
                    End With
                End Get
            End Property

        End Class

        Public Event StatusMessage(ByVal message As String)
        Public Event ProcessRequest(ByVal request As HttpRequest, ByVal response As HttpResponse)

        Public Sub New()

            serverPort = 80
            serverTextEncoding = UTF8
            serverThreads = New Hashtable

        End Sub

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)

            If disposing Then
                GC.SuppressFinalize(Me)
                [Stop]()
            End If

        End Sub

        Protected Overrides Sub Finalize()

            Dispose(True)

        End Sub

        <Browsable(True), Category("Configuration"), Description("Web server port.  Most web servers run on port 80."), DefaultValue(80)> _
        Public Property Port() As Integer
            Get
                Return serverPort
            End Get
            Set(ByVal Value As Integer)
                serverPort = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsRunning() As Boolean
            Get
                Return serverIsRunning
            End Get
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property TextEncoding() As Encoding
            Get
                Return serverTextEncoding
            End Get
            Set(ByVal Value As Encoding)
                serverTextEncoding = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ThreadsInUse() As Integer
            Get
                If IsRunning Then
                    Return serverThreads.Count + 1
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Sub Start()

            If serverIsRunning Then
                Throw New InvalidOperationException("Web server is already started.")
            Else
                ' Start listening thread
                RunThread.ExecuteNonPublicMethod(Me, "ListenForConnections")
                serverIsRunning = True
            End If

        End Sub

        Public Sub [Stop]()

            ' Stop listening thread
            serverIsRunning = False

            ' Stop any processing threads
            SyncLock serverThreads.SyncRoot
                For Each de As DictionaryEntry In serverThreads
                    CType(de.Value, RunThread).Abort()
                Next

                serverThreads.Clear()
            End SyncLock

        End Sub

        Private Sub ListenForConnections()

            Dim listener As New TcpListener(Dns.Resolve("localhost").AddressList(0), serverPort)

            Try
                ' Start the web server
                listener.Start()

                ' Enter the listening loop
                LogStatus("Server started")

                Do While serverIsRunning
                    ' Wait until next client connection is available
                    Do Until listener.Pending Or Not serverIsRunning
                        Thread.Sleep(100)
                    Loop

                    If Not serverIsRunning Then Exit Do

                    ' Process client request on an independent thread so we can keep listening for new requests
                    SyncLock serverThreads.SyncRoot
                        Dim threadID As Guid = Guid.NewGuid
                        serverThreads.Add(threadID, RunThread.ExecuteNonPublicMethod(Me, "ProcessClientRequest", listener.AcceptTcpClient(), threadID))
                    End SyncLock
                Loop
            Catch ex As Exception
                LogStatus("Server exception: " & ex.Message)
            Finally
                If Not listener Is Nothing Then listener.Stop()
                LogStatus("Server stopped")
            End Try

        End Sub

        Private Sub ProcessClientRequest(ByVal client As TcpClient, ByVal threadID As Guid)

            Dim clientStream As NetworkStream
            Dim request As New HttpRequest
            Dim response As New HttpResponse
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), bufferSize)
            Dim read As Integer

            Try
                ' Get client data
                clientStream = client.GetStream()

                Do
                    read = clientStream.Read(buffer, 0, bufferSize)
                    request.Content.Write(buffer, 0, read)

                    If request.Headers Is Nothing Then
                        ' Attempt to parse out request headers
                        ParseRequestHeaders(request)

                        ' If we have our headers, check for post continuation request from client (new to HTTP 1.1 protocol...)
                        If Not request.Headers Is Nothing Then
                            If request.Headers.NameExists("Expect") Then
                                If request.Headers("Expect").StartsWith("100") Then
                                    ' Client is requesting that server accept or reject post data, we have no reason to reject
                                    ' it, so we'll tell the client to continue with the post...
                                    SendResponse(clientStream, HttpResponseCode.Continue, request)
                                    Thread.Sleep(500)
                                End If
                            End If
                        End If
                    End If
                Loop While clientStream.DataAvailable

                ' Move content stream back to beginning
                request.Content.Seek(0, SeekOrigin.Begin)

                ' Allow control user to process web request
                RaiseEvent ProcessRequest(request, response)

                ' Send control user's response back to client
                SendResponse(clientStream, response, request)
            Catch ex As Exception
                LogStatus("Request exception: " & ex.Message)

                ' Send an error response back to client
                SendResponse(clientStream, HttpResponseCode.InternalServerError, request)
            Finally
                ' Close client connection
                If Not client Is Nothing Then client.Close()

                ' Thread is complete, remove it from the running thread list
                SyncLock serverThreads.SyncRoot
                    serverThreads.Remove(threadID)
                End SyncLock
            End Try

        End Sub

        Private Sub SendResponse(ByVal dataStream As NetworkStream, ByVal responseCode As HttpResponseCode, ByVal request As HttpRequest)

            SendResponse(dataStream, New HttpResponse(responseCode), request)

        End Sub

        Private Sub SendResponse(ByVal dataStream As NetworkStream, ByVal response As HttpResponse, ByVal request As HttpRequest)

            Dim responseHeader As Byte() = serverTextEncoding.GetBytes(response.ResponseHeader)

            dataStream.Write(responseHeader, 0, responseHeader.Length)

            If Not response.Content Is Nothing Then
                If response.Content.CanRead Then
                    Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), bufferSize)
                    Dim read As Integer

                    read = response.Content.Read(buffer, 0, bufferSize)

                    Do While read > 0
                        dataStream.Write(buffer, 0, read)
                        read = response.Content.Read(buffer, 0, bufferSize)
                    Loop
                End If
            End If

            LogStatus(request.RequestedURI & " - " & response.Code & " " & request.Headers("User-Agent"))

        End Sub

        Private Sub ParseRequestHeaders(ByVal request As HttpRequest)

            With request
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), .Content.Length)
                Dim headerData As String
                Dim contentPos As Integer

                ' Read the data stream thus far
                .Content.Seek(0, SeekOrigin.Begin)
                .Content.Read(buffer, 0, .Content.Length)

                ' Convert the stream to Unicode text
                headerData = serverTextEncoding.GetString(buffer)

                ' We will know we have the entire header when we find the content separation marker (two crlf's)
                contentPos = headerData.IndexOf(vbCrLf & vbCrLf)

                ' We've received the header portion of the client data, so let's parse out the headers
                If contentPos > -1 Then
                    Dim header As String()
                    Dim reader As New StringReader(headerData.Substring(0, contentPos))

                    ' Get client request (first line)
                    .RequestedURI = reader.ReadLine()

                    ' Parse remaining request headers
                    .Headers = New HttpHeaders

                    Do While reader.Peek <> -1
                        header = reader.ReadLine().Split(":"c)
                        If header.Length = 2 Then .Headers.Add(header(0).Trim(), header(1).Trim())
                    Loop

                    ' Update data stream so that stream begins at content
                    Dim contentOffset As Integer = contentPos + 2
                    .Content = New MemoryStream
                    .Content.Write(buffer, contentOffset, buffer.Length - contentOffset)
                End If
            End With

        End Sub

        Private Sub LogStatus(ByVal status As String)

            RaiseEvent StatusMessage(DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss ") & status)

        End Sub

    End Class

End Namespace
