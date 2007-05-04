'*******************************************************************************************************
'  TVA.Services.ClientHelper.vb - Client Request to Service
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
Imports TVA.Communication
Imports TVA.Serialization
Imports TVA.Configuration.Common

<ToolboxBitmap(GetType(ClientHelper))> _
Public Class ClientHelper
    Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

    Private m_serviceName As String
    Private m_persistSettings As Boolean
    Private m_settingsCategoryName As String

    Private WithEvents m_communicationClient As ICommunicationClient

#End Region

#Region " Event Declaration "

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

#End Region

#Region " Code Scope: Public "

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

    ''' <summary>
    ''' Gets or sets the instance of TCP client used for communicating with the service.
    ''' </summary>
    ''' <value></value>
    ''' <returns>An instance of TCP client.</returns>
    Public Property CommunicationClient() As ICommunicationClient
        Get
            Return m_communicationClient
        End Get
        Set(ByVal value As ICommunicationClient)
            m_communicationClient = value
        End Set
    End Property

    ''' <summary>
    ''' Attempts to connect to the service.
    ''' </summary>
    ''' <remarks>This method must be called in order to establish connection with the service.</remarks>
    Public Sub Connect()

        If m_communicationClient IsNot Nothing Then
            UpdateStatus(String.Format("Connecting to {0} [{1}]", m_serviceName, Date.Now.ToString()), 2)

            ' We'll always use handshaking to ensure the availability of SecureSession.
            m_communicationClient.Handshake = True
            m_communicationClient.HandshakePassphrase = m_serviceName

            ' Initiate connection to the service's communication server.
            m_communicationClient.Connect()
        Else
            UpdateStatus(String.Format("Cannot connect to {0}. No communication client is specified.", m_serviceName))
        End If

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

#Region " Interface Implementation "

#Region " IPersistSettings "

    Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
        Get
            Return m_persistSettings
        End Get
        Set(ByVal value As Boolean)
            m_persistSettings = value
        End Set
    End Property

    Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
        Get
            Return m_settingsCategoryName
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_settingsCategoryName = value
            Else
                Throw New ArgumentNullException("ConfigurationCategory")
            End If
        End Set
    End Property

    Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

        Try
            With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                If .Count > 0 Then
                    ServiceName = .Item("ServiceName").GetTypedValue(m_serviceName)
                End If
            End With
        Catch ex As Exception
            ' We'll encounter exceptions if the settings are not present in the config file.
        End Try

    End Sub

    Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

        If m_persistSettings Then
            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    .Clear()
                    With .Item("ServiceName", True)
                        .Value = m_serviceName
                        .Description = ""
                    End With
                End With
                TVA.Configuration.Common.SaveSettings()
            Catch ex As Exception
                ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
            End Try
        End If

    End Sub

#End Region

#Region " ISupportInitialize "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            LoadSettings()  ' Load settings from the config file.
        End If

    End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

#Region " Event Handlers "

#Region " CommunicationClient "

    Private Sub m_communicationClient_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Connected

        ' Upon establishing connection with the service's communication client, we'll send our information to the 
        ' service so the service can keep track of all the client that are connected to its communication server.
        m_communicationClient.Send(New ClientInfo(m_communicationClient.ClientID))

        With New StringBuilder()
            .AppendFormat("Connected to {0} [{1}]", m_serviceName, Date.Now.ToString())
            .AppendLine()
            .AppendLine()
            .Append(m_communicationClient.Status)

            UpdateStatus(.ToString(), 2)
        End With

    End Sub

    Private Sub m_communicationClient_Connecting(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Connecting

        UpdateStatus(".")

    End Sub

    Private Sub m_communicationClient_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationClient.Disconnected

        With New StringBuilder()
            .AppendFormat("Disconnected from {0} [{1}]", m_serviceName, Date.Now.ToString())
            .AppendLine()
            .AppendLine()
            .Append(m_communicationClient.Status)

            UpdateStatus(.ToString(), 2)
        End With

    End Sub

    Private Sub m_communicationClient_ReceivedData(ByVal sender As Object, ByVal e As IdentifiableItemEventArgs(Of Byte())) Handles m_communicationClient.ReceivedData

        Dim response As ServiceResponse = GetObject(Of ServiceResponse)(e.Item)
        If response IsNot Nothing Then
            RaiseEvent ReceivedServiceResponse(Me, New ServiceResponseEventArgs(response))
            Select Case response.Type
                Case "UPDATECLIENTSTATUS"
                    UpdateStatus(response.Message)
                Case "SERVICESTATECHANGED"
                    Dim messageSegments As String() = response.Message.Split(">"c)
                    If messageSegments.Length = 2 Then
                        ' Notify change in service state by raising the ServiceStateChanged event.
                        Dim newServiceState As ServiceState = DirectCast(System.Enum.Parse(GetType(ServiceState), messageSegments(1)), ServiceState)
                        RaiseEvent ServiceStateChanged(Me, New ObjectStateChangedEventArgs(Of ServiceState)(messageSegments(0), newServiceState))

                        UpdateStatus(String.Format("State of service ""{0}"" has changed to ""{1}"".", messageSegments(0), messageSegments(1)), 3)
                    End If
                Case "PROCESSSTATECHANGED"
                    Dim messageSegments As String() = response.Message.Split(">"c)
                    If messageSegments.Length = 2 Then
                        ' Notify change in process state by raising the ProcessStateChanged event.
                        Dim newProcessState As ProcessState = DirectCast(System.Enum.Parse(GetType(ProcessState), messageSegments(1)), ProcessState)
                        RaiseEvent ProcessStateChanged(Me, New ObjectStateChangedEventArgs(Of ProcessState)(messageSegments(0), newProcessState))

                        UpdateStatus(String.Format("State of process ""{0}"" has changed to ""{1}"".", messageSegments(0), messageSegments(1)), 3)
                    End If
            End Select
        End If

    End Sub

#End Region

#End Region

#End Region

End Class