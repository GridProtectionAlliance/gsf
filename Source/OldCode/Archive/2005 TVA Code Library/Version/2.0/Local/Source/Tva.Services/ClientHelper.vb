'*******************************************************************************************************
'  Tva.Services.ClientHelper.vb - Client Request to Service
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
'  08/29/2006 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.Communication
Imports Tva.Serialization
Imports Tva.Configuration.Common

<ToolboxBitmap(GetType(ClientHelper))> _
Public Class ClientHelper
    Implements ISupportInitialize

    Private m_serviceName As String
    Private m_connectionString As String
    Private m_encryption As Tva.Security.Cryptography.EncryptLevel
    Private m_secureSession As Boolean

    Private WithEvents m_communicationClient As ICommunicationClient

    ''' <summary>
    ''' Occurs when the service client must update its status.
    ''' </summary>
    Public Event UpdateClientStatus(ByVal sender As Object, ByVal e As UpdateClientStatusEventArgs)

    ''' <summary>
    ''' Occurs when a response is received from the service.
    ''' </summary>
    Public Event ReceivedServiceResponse(ByVal sender As Object, ByVal e As ServiceResponseEventArgs)

    ''' <summary>
    ''' Occurs when the service state changes.
    ''' </summary>
    Public Event ServiceStateChanged(ByVal sender As Object, ByVal e As ObjectStateChangedEventArgs(Of ServiceState))

    ''' <summary>
    ''' Occurs when the state of a process changes.
    ''' </summary>
    Public Event ProcessStateChanged(ByVal sender As Object, ByVal e As ObjectStateChangedEventArgs(Of ProcessState))

    <Category("Client Helper")> _
    Public Property ServiceName() As String
        Get
            Return m_serviceName
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_serviceName = value
            Else
                Throw New ArgumentNullException("ServiceName")
            End If
        End Set
    End Property

    <Category("Communication"), DefaultValue(GetType(String), "Protocol=Tcp; Server=localhost; Port=6500")> _
    Public Property ConnectionString() As String
        Get
            Return m_connectionString
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                If Tva.Text.Common.ParseKeyValuePairs(value).ContainsKey("protocol") Then
                    m_connectionString = value
                Else
                    Throw New ArgumentException("Communication protocol must be specified.", "ConnectionString")
                End If
            Else
                Throw New ArgumentNullException("ConnectionString")
            End If
        End Set
    End Property

    <Category("Communication"), DefaultValue(GetType(Tva.Security.Cryptography.EncryptLevel), "Level1")> _
    Public Property Encryption() As Tva.Security.Cryptography.EncryptLevel
        Get
            Return m_encryption
        End Get
        Set(ByVal value As Tva.Security.Cryptography.EncryptLevel)
            m_encryption = value
        End Set
    End Property

    <Category("Communication"), DefaultValue(GetType(Boolean), "True")> _
    Public Property SecureSession() As Boolean
        Get
            Return m_secureSession
        End Get
        Set(ByVal value As Boolean)
            m_secureSession = value
        End Set
    End Property

    <Browsable(False)> _
    Public ReadOnly Property CommunicationUri() As String
        Get
            Dim connectionString As Dictionary(Of String, String) = Tva.Text.Common.ParseKeyValuePairs(m_connectionString)
            Return String.Format("{0}://{1}:{2}/{3}", connectionString("protocol").ToLower(), connectionString("server").ToLower(), connectionString("port").ToLower(), m_serviceName)
        End Get
    End Property

    ''' <summary>
    ''' Gets the instance of TCP client used for communicating with the service.
    ''' </summary>
    ''' <value></value>
    ''' <returns>An instance of TCP client.</returns>
    <Browsable(False)> _
    Public ReadOnly Property CommunicationClient() As ICommunicationClient
        Get
            Return m_communicationClient
        End Get
    End Property

    ''' <summary>
    ''' Attempts to connect to the service.
    ''' </summary>
    ''' <remarks>This method must be called in order to establish connection with the service.</remarks>
    Public Sub Connect()

        UpdateStatus(String.Format("Attempting connection to ""{0}""...", Me.CommunicationUri), 2)

        m_communicationClient = Tva.Communication.Common.CreateCommunicationClient(m_connectionString)
        ' We'll always use handshaking to ensure the availability of SecureSession.
        m_communicationClient.Handshake = True
        m_communicationClient.HandshakePassphrase = m_serviceName
        m_communicationClient.Encryption = m_encryption
        m_communicationClient.SecureSession = m_secureSession
        Select Case m_communicationClient.Protocol
            Case TransportProtocol.Tcp
                DirectCast(m_communicationClient, TcpClient).PayloadAware = True
            Case TransportProtocol.Udp
                DirectCast(m_communicationClient, UdpClient).PayloadAware = True
        End Select

        ' Initiate connection to the service's communication server.
        m_communicationClient.Connect()

    End Sub

    Public Sub Disconnect()

        m_communicationClient.Disconnect()

    End Sub

    Public Sub SendRequest(ByVal request As String)

        Dim requestInstance As ClientRequest = ClientRequest.Parse(request)
        If requestInstance IsNot Nothing Then
            SendRequest(requestInstance)
        Else
            UpdateStatus(String.Format("Request command ""{0}"" is invalid", request), 2)
        End If

    End Sub

    ''' <summary>
    ''' Sends a request to the service.
    ''' </summary>
    ''' <param name="request">The request to be sent to the service.</param>
    Public Sub SendRequest(ByVal request As ClientRequest)

        m_communicationClient.Send(request)

    End Sub

    Public Sub UpdateStatus(ByVal message As String)

        UpdateStatus(message, 1)

    End Sub

    Public Sub UpdateStatus(ByVal message As String, ByVal crlfCount As Integer)

        With New StringBuilder()
            .Append(message)

            For i As Integer = 0 To crlfCount - 1
                .Append(Environment.NewLine)
            Next

            RaiseEvent UpdateClientStatus(Me, New UpdateClientStatusEventArgs(.ToString()))
        End With

    End Sub

#Region " CommunicationClient Events "

    Private Sub m_communicationClient_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Connected

        ' Upon establishing connection with the service's communication client, we'll send our information to the 
        ' service so the service can keep track of all the client that are connected to its communication server.
        m_communicationClient.Send(New ClientInfo())

        With New StringBuilder()
            .Append(String.Format("Connected to {0} [{1}]", m_serviceName, System.DateTime.Now.ToString()))
            .Append(Environment.NewLine)
            .Append(Environment.NewLine)
            .Append(m_communicationClient.Status)

            UpdateStatus(.ToString())
        End With

    End Sub

    Private Sub m_communicationClient_Connecting(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Connecting

        UpdateStatus(".")

    End Sub

    Private Sub m_communicationClient_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Disconnected

        With New StringBuilder()
            .Append(String.Format("Disconnected from {0} [{1}]", m_serviceName, System.DateTime.Now.ToString()))
            .Append(Environment.NewLine)
            .Append(Environment.NewLine)
            .Append(m_communicationClient.Status)

            UpdateStatus(.ToString())
        End With

    End Sub

    Private Sub m_communicationClient_ReceivedData(ByVal sender As Object, ByVal e As DataEventArgs) Handles m_communicationClient.ReceivedData

        Dim response As ServiceResponse = GetObject(Of ServiceResponse)(e.Data)
        If response IsNot Nothing Then
            RaiseEvent ReceivedServiceResponse(Me, New ServiceResponseEventArgs(response))
            Select Case response.Type
                Case "UPDATECLIENTSTATUS"
                    UpdateStatus(response.Message, 1)
                Case "SERVICESTATECHANGED"
                    Dim messageSegments As String() = response.Message.Split(">"c)
                    If messageSegments.Length = 2 Then
                        ' Notify change in service state by raising the ServiceStateChanged event.
                        Dim newServiceState As ServiceState = DirectCast(System.Enum.Parse(GetType(ServiceState), messageSegments(1)), ServiceState)
                        RaiseEvent ServiceStateChanged(Me, New ObjectStateChangedEventArgs(Of ServiceState)(messageSegments(0), newServiceState))

                        With New StringBuilder()
                            .Append("State of the following service has changed:")
                            .Append(Environment.NewLine)
                            .Append("              Service Name: ")
                            .Append(messageSegments(0))
                            .Append(Environment.NewLine)
                            .Append("             Service State: ")
                            .Append(messageSegments(1))
                            .Append(Environment.NewLine)

                            UpdateStatus(.ToString())
                        End With
                    End If
                Case "PROCESSSTATECHANGED"
                    Dim messageSegments As String() = response.Message.Split(">"c)
                    If messageSegments.Length = 2 Then
                        ' Notify change in process state by raising the ProcessStateChanged event.
                        Dim newProcessState As ProcessState = DirectCast(System.Enum.Parse(GetType(ProcessState), messageSegments(1)), ProcessState)
                        RaiseEvent ProcessStateChanged(Me, New ObjectStateChangedEventArgs(Of ProcessState)(messageSegments(0), newProcessState))

                        With New StringBuilder()
                            .Append("State of the following process has changed:")
                            .Append(Environment.NewLine)
                            .Append("              Process Name: ")
                            .Append(messageSegments(0))
                            .Append(Environment.NewLine)
                            .Append("             Process State: ")
                            .Append(messageSegments(1))
                            .Append(Environment.NewLine)

                            UpdateStatus(.ToString())
                        End With
                    End If
            End Select
        End If

    End Sub

#End Region

#Region " ISupportInitialize Implementation "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            ' Make sure that all of the required settings exist in the config file.
            CategorizedSettings("ClientHelper").Add("ServiceName", m_serviceName, "Name of the service that the client will be connecting to.")
            CategorizedSettings("Communication").Add("ConnectionString", m_connectionString, "The connection string that defines how the client will communicate with the service.")
            CategorizedSettings("Communication").Add("Encryption", m_encryption.ToString(), "Level of encryption to be used for the communication between the client and the service (None, Level1, Level2, Level3, Level4).")
            CategorizedSettings("Communication").Add("SecureSession", m_secureSession.ToString(), "True if SSL level encryption is to be used for communication between the client and the service; otherwise False.")
            SaveSettings()

            ' Update the variable with values that are defined in the config file.
            m_serviceName = CategorizedSettings("ClientHelper")("ServiceName").Value
            m_connectionString = CategorizedSettings("Communication")("ConnectionString").Value
            m_encryption = CategorizedSettings("Communication")("Encryption").GetTypedValue(Tva.Security.Cryptography.EncryptLevel.Level1)
            m_secureSession = CategorizedSettings("Communication")("SecureSession").GetTypedValue(True)
        End If

    End Sub

#End Region

End Class