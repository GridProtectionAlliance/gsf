'*******************************************************************************************************
'  TVA.Security.Radius.RadiusClient.vb - RADIUS authentication client
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
'  04/11/2008 - Pinal C. Patel
'       Original version of source code generated.
'
'*******************************************************************************************************

Imports System.Threading
Imports TVA.IO.Common
Imports TVA.Text.Common
Imports TVA.Communication

Namespace Radius

    Public Class RadiusClient
        Implements IDisposable

#Region " Member Declaration "

        Private m_requestAttempts As Short
        Private m_reponseTimeout As Integer
        Private m_sharedSecret As String
        Private m_newPinModeMessage1 As String
        Private m_newPinModeMessage2 As String
        Private m_newPinModeMessage3 As String
        Private m_nextTokenModeMessage As String
        Private m_disposed As Boolean
        Private m_responseBytes As Byte()

        Private WithEvents m_udpClient As UdpClient

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Default port of the RADIUS server.
        ''' </summary>
        Public Const DefaultServerPort As Integer = 1812

        ''' <summary>
        ''' Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not Step 1 (ensuring that the user account is in 
        ''' the "New Pin" mode) of creating a new pin was successful.
        ''' </summary>
        Public Const DefaultNewPinModeMessage1 As String = "Enter a new PIN"

        ''' <summary>
        ''' Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not Step 2 (new pin is accepted in attempt #1)
        '''  of creating a new pin was successful.
        ''' </summary>
        Public Const DefaultNewPinModeMessage2 As String = "Please re-enter new PIN"

        ''' <summary>
        ''' Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not Step 3 (new pin is accepted in attempts #2) 
        ''' of creating a new pin was successful.
        ''' </summary>
        Public Const DefaultNewPinModeMessage3 As String = "PIN Accepted"

        ''' <summary>
        ''' Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not a user account is in the "Next Token" mode.
        ''' </summary>
        Public Const DefaultNextTokenModeMessage As String = "Wait for token to change"

        ''' <summary>
        ''' Creates an instance of RADIUS client for sending request to a RADIUS server.
        ''' </summary>
        ''' <param name="serverName">Name or address of the RADIUS server.</param>
        ''' <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
        Public Sub New(ByVal serverName As String, ByVal sharedSecret As String)

            MyClass.New(serverName, DefaultServerPort, sharedSecret)

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS client for sending request to a RADIUS server.
        ''' </summary>
        ''' <param name="serverName">Name or address of the RADIUS server.</param>
        ''' <param name="serverPort">Port number of the RADIUS server.</param>
        ''' <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal serverName As String, ByVal serverPort As Integer, ByVal sharedSecret As String)

            Me.SharedSecret = sharedSecret
            Me.RequestAttempts = 1
            Me.ReponseTimeout = 30000
            Me.NewPinModeMessage1 = DefaultNewPinModeMessage1
            Me.NewPinModeMessage2 = DefaultNewPinModeMessage2
            Me.NewPinModeMessage3 = DefaultNewPinModeMessage3
            Me.NextTokenModeMessage = DefaultNextTokenModeMessage
            m_udpClient = New UdpClient(String.Format("Server={0}; RemotePort={1}; LocalPort=0", serverName, serverPort))
            m_udpClient.Handshake = False
            m_udpClient.PayloadAware = False
            m_udpClient.Connect()   ' Start the connection cycle.

        End Sub

        ''' <summary>
        ''' Gets or sets the name or address of the RADIUS server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Name or address of RADIUS server.</returns>
        Public Property ServerName() As String
            Get
                Return ParseKeyValuePairs(m_udpClient.ConnectionString)("server")
            End Get
            Set(ByVal value As String)
                CheckDisposed()
                If Not String.IsNullOrEmpty(value) Then
                    Dim parts As Dictionary(Of String, String) = ParseKeyValuePairs(m_udpClient.ConnectionString)
                    parts("server") = value
                    m_udpClient.ConnectionString = JoinKeyValuePairs(parts)
                Else
                    Throw New ArgumentNullException("ServerName")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the port number of the RADIUS server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Port number of the RADIUS server.</returns>
        Public Property ServerPort() As Integer
            Get
                Return CInt(ParseKeyValuePairs(m_udpClient.ConnectionString)("remoteport"))
            End Get
            Set(ByVal value As Integer)
                CheckDisposed()
                If value >= 0 AndAlso value <= 65535 Then
                    Dim parts As Dictionary(Of String, String) = ParseKeyValuePairs(m_udpClient.ConnectionString)
                    parts("remoteport") = value.ToString()
                    m_udpClient.ConnectionString = JoinKeyValuePairs(parts)
                Else
                    Throw New ArgumentOutOfRangeException("ServerPort", "Value must be between 0 and 65535.")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the number of time a request is to sent to the server until a valid response is received.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Number of time a request is to sent to the server until a valid response is received.</returns>
        Public Property RequestAttempts() As Short
            Get
                Return m_requestAttempts
            End Get
            Set(ByVal value As Short)
                CheckDisposed()
                If value >= 1 AndAlso value <= 10 Then
                    m_requestAttempts = value
                Else
                    Throw New ArgumentOutOfRangeException("RequestAttempts", "Value must be between 1 and 10.")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the time (in milliseconds) to wait for a response from server after sending a request.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Time (in milliseconds) to wait for a response from server after sending a request.</returns>
        Public Property ReponseTimeout() As Integer
            Get
                Return m_reponseTimeout
            End Get
            Set(ByVal value As Integer)
                CheckDisposed()
                If value >= 1000 AndAlso value <= 60000 Then
                    m_reponseTimeout = value
                Else
                    Throw New ArgumentOutOfRangeException("ResponseTimeout", "Value must be between 1000 and 60000.")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the shared secret used between the client and server for encryption and authentication.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Shared secret used between the client and server for encryption and authentication.</returns>
        Public Property SharedSecret() As String
            Get
                Return m_sharedSecret
            End Get
            Set(ByVal value As String)
                CheckDisposed()
                If Not String.IsNullOrEmpty(value) Then
                    m_sharedSecret = value
                Else
                    Throw New ArgumentNullException("SharedSecret")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not Step 1 (ensuring that the user account is in  the 
        ''' "New Pin" mode) of creating a new pin was successful.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Text for "New Pin" mode's first message.</returns>
        Public Property NewPinModeMessage1() As String
            Get
                Return m_newPinModeMessage1
            End Get
            Set(ByVal value As String)
                CheckDisposed()
                If Not String.IsNullOrEmpty(value) Then
                    m_newPinModeMessage1 = value
                Else
                    Throw New ArgumentNullException("NewPinModeMessage1")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not Step 2 (new pin is accepted in attempt #1) of creating 
        ''' a new pin was successful.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Text for "New Pin" mode's second message.</returns>
        Public Property NewPinModeMessage2() As String
            Get
                Return m_newPinModeMessage2
            End Get
            Set(ByVal value As String)
                CheckDisposed()
                If Not String.IsNullOrEmpty(value) Then
                    m_newPinModeMessage2 = value
                Else
                    Throw New ArgumentNullException("NewPinModeMessage2")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not Step 3 (new pin is accepted in attempts #2) of creating 
        ''' a new pin was successful.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Text for "New Pin" mode's third message.</returns>
        Public Property NewPinModeMessage3() As String
            Get
                Return m_newPinModeMessage3
            End Get
            Set(ByVal value As String)
                CheckDisposed()
                If Not String.IsNullOrEmpty(value) Then
                    m_newPinModeMessage3 = value
                Else
                    Throw New ArgumentNullException("NewPinModeMessage3")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        ''' server response to determine whether or not a user account is in the "Next Token" mode.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Text for "Next Token" mode.</returns>
        Public Property NextTokenModeMessage() As String
            Get
                Return m_nextTokenModeMessage
            End Get
            Set(ByVal value As String)
                CheckDisposed()
                If Not String.IsNullOrEmpty(value) Then
                    m_nextTokenModeMessage = value
                Else
                    Throw New ArgumentNullException("NextTokenModeMessage")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Send a request to the server and waits for a response back.
        ''' </summary>
        ''' <param name="request">Request to be sent to the server.</param>
        ''' <returns>Response packet if a valid reponse is received from the server; otherwise Nothing.</returns>
        Public Function ProcessRequest(ByVal request As RadiusPacket) As RadiusPacket

            CheckDisposed()
            Dim response As RadiusPacket = Nothing
            ' We wait indefinately for the connection to establish. But since this is UDP, the connection
            ' will always be successful (locally we're binding to any available UDP port).
            If m_udpClient.WaitForConnection(-1) AndAlso m_udpClient.IsConnected Then
                ' We have a UDP socket we can use for exchanging packets.
                Dim stopTime As Date
                For i As Integer = 1 To m_requestAttempts
                    m_responseBytes = Nothing
                    m_udpClient.Send(request.BinaryImage)

                    stopTime = Date.Now.AddMilliseconds(m_reponseTimeout)
                    Do While True
                        Thread.Sleep(1)
                        ' Stay in the loop until:
                        ' 1) We receive a response OR
                        ' 2) We exceed the response timeout duration
                        If m_responseBytes IsNot Nothing OrElse Date.Now > stopTime Then
                            Exit Do
                        End If
                    Loop

                    If m_responseBytes IsNot Nothing Then
                        ' The server sent a response.
                        response = New RadiusPacket(m_responseBytes, 0)
                        If response.Identifier = request.Identifier AndAlso _
                                CompareBuffers(response.Authenticator, RadiusPacket.CreateResponseAuthenticator(m_sharedSecret, request, response)) = 0 Then
                            ' The response has passed the verification.
                            Exit For
                        Else
                            ' The response failed the verification, so we'll silently discard it.
                            response = Nothing
                        End If
                    End If
                Next
            End If

            Return response

        End Function

        ''' <summary>
        ''' Create a new pin for the user.
        ''' </summary>
        ''' <param name="username">Name of the user.</param>
        ''' <param name="token">Current token of the user.</param>
        ''' <param name="pin">New pin of the user.</param>
        ''' <returns>True if a new pin is created for the user successfully; otherwise False.</returns>
        Public Function CreateNewPin(ByVal username As String, ByVal token As String, ByVal pin As String) As Boolean

            CheckDisposed()
            If Not String.IsNullOrEmpty(pin) Then
                Dim reply As Byte()
                Dim response As RadiusPacket

                ' Step 1: Send username and token for password, receive a challenge response with reply 
                '         message worded "Enter a new PIN". [Verification]
                ' Step 2: Send username and new ping for password, receive a challenge response with reply
                '         message worded "Please re-enter.  [Attempt #1]
                ' Step 3: Send username and new ping for password, receive a challenge response with reply
                '         message worded "PIN Accepted".    [Attempt #2]

                response = Authenticate(username, token)
                If IsUserInNewPinMode(response) Then
                    ' User account is really in "New Pin" mode.
                    response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State))
                    reply = response.GetAttributeValue(AttributeType.ReplyMessage)
                    If Not RadiusPacket.ToText(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage2.ToLower()) Then
                        Return False    ' New pin not accepted in attempt #1.
                    End If

                    response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State))
                    reply = response.GetAttributeValue(AttributeType.ReplyMessage)
                    If Not RadiusPacket.ToText(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage3.ToLower()) Then
                        Return False    ' New pin not accepted in attempt #2.
                    End If

                    Return True         ' All is good - new pin is created for the user.
                Else
                    Return False
                End If
            Else
                Throw New ArgumentNullException("pin")
            End If

        End Function

        ''' <summary>
        ''' Authenticates the username and password against the RADIUS server.
        ''' </summary>
        ''' <param name="username">Username to be authenticated.</param>
        ''' <param name="password">Password to be authenticated.</param>
        ''' <returns>Response packet received from the server for the authentication request.</returns>
        ''' <remarks>
        ''' <para>
        ''' The type of response packet (if any) will be one of the following:
        ''' <list>
        ''' <item>AccessAccept: If the authentication is successful.</item>
        ''' <item>AccessReject: If the authentication is not successful.</item>
        ''' <item>AccessChallenge: If the server need more information from the user.</item>
        ''' </list>
        ''' </para>
        ''' <para>
        ''' When an AccessChallenge response packet is received from the server, it contains a State attribute
        ''' that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
        ''' response. So if this method returns an AccessChallenge packet, then this method is to be called again
        ''' with the requested information (from ReplyMessage attribute) in the password field and the value State 
        ''' attribute.
        ''' </para>
        ''' </remarks>
        Public Function Authenticate(ByVal username As String, ByVal password As String) As RadiusPacket

            Return Authenticate(username, password, Nothing)

        End Function

        ''' <summary>
        ''' Authenticates the username and password against the RADIUS server.
        ''' </summary>
        ''' <param name="username">Username to be authenticated.</param>
        ''' <param name="password">Password to be authenticated.</param>
        ''' <param name="state">State value from a previous challenge response.</param>
        ''' <returns>Response packet received from the server for the authentication request.</returns>
        ''' <remarks>
        ''' <para>
        ''' The type of response packet (if any) will be one of the following:
        ''' <list>
        ''' <item>AccessAccept: If the authentication is successful.</item>
        ''' <item>AccessReject: If the authentication is not successful.</item>
        ''' <item>AccessChallenge: If the server need more information from the user.</item>
        ''' </list>
        ''' </para>
        ''' <para>
        ''' When an AccessChallenge response packet is received from the server, it contains a State attribute
        ''' that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
        ''' response. So if this method returns an AccessChallenge packet, then this method is to be called again
        ''' with the requested information (from ReplyMessage attribute) in the password field and the value State 
        ''' attribute.
        ''' </para>
        ''' </remarks>
        Public Function Authenticate(ByVal username As String, ByVal password As String, ByVal state As Byte()) As RadiusPacket

            CheckDisposed()
            If Not String.IsNullOrEmpty(username) AndAlso Not String.IsNullOrEmpty(password) Then
                Dim request As New RadiusPacket(PacketType.AccessRequest)
                Dim authenticator As Byte() = RadiusPacket.CreateRequestAuthenticator(m_sharedSecret)

                request.Authenticator = authenticator
                request.Attributes.Add(New RadiusPacketAttribute(AttributeType.UserName, username))
                request.Attributes.Add(New RadiusPacketAttribute(AttributeType.UserPassword, _
                                                                 RadiusPacket.EncryptPassword(password, _
                                                                                              m_sharedSecret, _
                                                                                              authenticator)))
                If state IsNot Nothing Then
                    ' State attribute is used when responding to a AccessChallenge reponse.
                    request.Attributes.Add(New RadiusPacketAttribute(AttributeType.State, state))
                End If

                Return ProcessRequest(request)
            Else
                Throw New ArgumentException("Username and Password cannot be null.")
            End If

        End Function

        ''' <summary>
        ''' Determines whether or not the response indicates that the user account is in "New Pin" mode.
        ''' </summary>
        ''' <param name="response">Response packet sent by the server.</param>
        ''' <returns>True if the user account is in "New Pin" mode; otherwise False.</returns>
        ''' <remarks>
        ''' A user's account can be in the "New Pin" mode when set on the server.
        ''' </remarks>
        Public Function IsUserInNewPinMode(ByVal response As RadiusPacket) As Boolean

            CheckDisposed()
            If response IsNot Nothing Then
                Dim messageBytes As Byte() = response.GetAttributeValue(AttributeType.ReplyMessage)
                If messageBytes IsNot Nothing Then
                    ' Unfortunately, the only way of determining whether or not a user account is in the 
                    ' "New Pin" mode is from the text present in the ReplyMessage attribute of the 
                    ' AccessChallenge response from server. 
                    Dim messageString As String = RadiusPacket.ToText(messageBytes, 0, messageBytes.Length)
                    If messageString.ToLower().Contains(m_newPinModeMessage1.ToLower()) Then
                        Return True ' User account is in "New Pin" mode.
                    End If
                End If
            Else
                Throw New ArgumentNullException("response")
            End If

        End Function

        ''' <summary>
        ''' Determines whether or not the response indicates that the user account is in "Next Token" mode.
        ''' </summary>
        ''' <param name="response">Response packet sent by the server.</param>
        ''' <returns>True if the user account is in "Next Token" mode; otherwise False.</returns>
        ''' <remarks>
        ''' A user's account can enter the "Next Token" mode after the user enters incorrect passwords for a few 
        ''' times (3 times by default) and then enters the correct password. Note that repeatedly entering
        ''' incorrect passwords will disable the user account.
        ''' </remarks>
        Public Function IsUserInNextTokenMode(ByVal response As RadiusPacket) As Boolean

            CheckDisposed()
            If response IsNot Nothing Then
                Dim messageBytes As Byte() = response.GetAttributeValue(AttributeType.ReplyMessage)
                If messageBytes IsNot Nothing Then
                    ' Unfortunately, the only way of determining whether or not a user account is in the 
                    ' "Next Token" mode is from the text present in the ReplyMessage attribute of the 
                    ' AccessChallenge response from server. 
                    Dim messageString As String = RadiusPacket.ToText(messageBytes, 0, messageBytes.Length)
                    If messageString.ToLower().Contains(m_nextTokenModeMessage.ToLower()) Then
                        Return True ' User account is in "Next Token" mode.
                    End If
                End If
            Else
                Throw New ArgumentNullException("response")
            End If

        End Function

#Region " Interface Implementation "

#Region " IDisposable "

        ''' <summary>
        ''' Releases the used resources.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            Dispose(True)
            GC.SuppressFinalize(Me)

        End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Protected "

        ''' <summary>
        ''' Helper method to check whether or not the object instance has been disposed.
        ''' </summary>
        ''' <remarks>This method is to be called before performing any operation.</remarks>
        Protected Sub CheckDisposed()

            If m_disposed Then Throw New ObjectDisposedException(Me.GetType().Name)

        End Sub

        ''' <summary>
        ''' Releases the used resources.
        ''' </summary>
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)

            If Not m_disposed Then
                If disposing Then
                    m_udpClient.Dispose()
                    m_udpClient = Nothing
                End If
            End If
            m_disposed = True

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub m_udpClient_ReceivedData(ByVal sender As Object, ByVal e As GenericEventArgs(Of IdentifiableItem(Of System.Guid, Byte()))) Handles m_udpClient.ReceivedData

            m_responseBytes = e.Argument.Item

        End Sub

#End Region

    End Class

End Namespace