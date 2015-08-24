'*******************************************************************************************************
'  TVA.Services.ClientHelper.vb - Helper class for windows service client
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
'  11/30/2007 - Pinal C. Patel
'       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
'       instead of DesignMode property as the former is more accurate than the latter
'
'*******************************************************************************************************

Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports TVA.Communication
Imports TVA.Serialization
Imports TVA.Configuration

<ToolboxBitmap(GetType(ClientHelper))> _
Public Class ClientHelper
    Implements IPersistSettings, ISupportInitialize

#Region " Variables "

    Private m_serviceName As String
    Private m_persistSettings As Boolean
    Private m_settingsCategoryName As String

    Private WithEvents m_remotingClient As CommunicationClientBase

#End Region

#Region " Constants "

    ''' <summary>
    ''' Default value for ServiceName property.
    ''' </summary>
    Public Const DefaultServiceName As String = "WindowsService"

    ''' <summary>
    ''' Default value for PersistSettings property.
    ''' </summary>
    Public Const DefaultPersistSettings As Boolean = False

    ''' <summary>
    ''' Default value for SettingsCategoryName property.
    ''' </summary>
    Public Const DefaultSettingsCategoryName As String = "ClientHelper"

#End Region

#Region " Events "

    ''' <summary>
    ''' Occurs when the service client must update its status.
    ''' </summary>
    Public Event UpdateClientStatus As EventHandler(Of GenericEventArgs(Of String))

    ''' <summary>
    ''' Occurs when a response is received from the service.
    ''' </summary>
    Public Event ReceivedServiceResponse As EventHandler(Of GenericEventArgs(Of ServiceResponse))

    ''' <summary>
    ''' Occurs when the service state changes.
    ''' </summary>
    Public Event ServiceStateChanged As EventHandler(Of GenericEventArgs(Of ObjectState(Of ServiceState)))

    ''' <summary>
    ''' Occurs when the state of a process changes.
    ''' </summary>
    Public Event ProcessStateChanged As EventHandler(Of GenericEventArgs(Of ObjectState(Of ProcessState)))

    ''' <summary>
    ''' Occurs when a remote command session has been established.
    ''' </summary>
    Public Event CommandSessionEstablished As EventHandler

    ''' <summary>
    ''' Occurs when a remote command session has been terminated.
    ''' </summary>
    Public Event CommandSessionTerminated As EventHandler

#End Region

#Region " Properties "

    <Category("Service"), DefaultValue(DefaultServiceName)> _
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
    <Category("Service")> _
    Public Property RemotingClient() As CommunicationClientBase
        Get
            Return m_remotingClient
        End Get
        Set(ByVal value As CommunicationClientBase)
            m_remotingClient = value
        End Set
    End Property

    <Category("Persistance"), DefaultValue(DefaultPersistSettings)> _
    Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
        Get
            Return m_persistSettings
        End Get
        Set(ByVal value As Boolean)
            m_persistSettings = value
        End Set
    End Property

    <Category("Persistance"), DefaultValue(DefaultSettingsCategoryName)> _
    Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
        Get
            Return m_settingsCategoryName
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_settingsCategoryName = value
            Else
                Throw New ArgumentNullException("SettingsCategoryName")
            End If
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Attempts to connect to the service.
    ''' </summary>
    ''' <remarks>This method must be called in order to establish connection with the service.</remarks>
    Public Sub Connect()

        If m_remotingClient IsNot Nothing Then
            With New StringBuilder()
                .AppendFormat("Connecting to {0} [{1}]", m_serviceName, Date.Now.ToString())
                .AppendLine()
                .Append(".")
                .AppendLine()
                .Append(".")

                UpdateStatus(.ToString(), 1)
            End With

            ' We'll always use handshaking to ensure the availability of SecureSession.
            m_remotingClient.Handshake = True
            m_remotingClient.HandshakePassphrase = m_serviceName

            ' Initiate connection to the service's communication server.
            m_remotingClient.Connect()
        Else
            UpdateStatus(String.Format("Cannot connect to {0}. No communication client is specified.", m_serviceName))
        End If

    End Sub

    Public Sub Disconnect()

        m_remotingClient.Disconnect()

    End Sub

    Public Sub SendRequest(ByVal request As String)

        Dim requestInstance As ClientRequest = ClientRequest.Parse(request)
        If requestInstance IsNot Nothing Then
            SendRequest(requestInstance)
        Else
            UpdateStatus(String.Format("Request command ""{0}"" is invalid", request), ServiceHelper.UpdateCrlfCount)
        End If

    End Sub

    ''' <summary>
    ''' Sends a request to the service.
    ''' </summary>
    ''' <param name="request">The request to be sent to the service.</param>
    Public Sub SendRequest(ByVal request As ClientRequest)

        m_remotingClient.Send(request)

    End Sub

    Public Sub UpdateStatus(ByVal message As String)

        UpdateStatus(message, 0)

    End Sub

    Public Sub UpdateStatus(ByVal message As String, ByVal crlfCount As Integer)

        With New StringBuilder()
            .Append(message)

            For i As Integer = 0 To crlfCount - 1
                .AppendLine()
            Next

            RaiseEvent UpdateClientStatus(Me, New GenericEventArgs(Of String)(.ToString()))
        End With

    End Sub

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

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If LicenseManager.UsageMode = LicenseUsageMode.Runtime Then
            LoadSettings()  ' Load settings from the config file.
        End If

    End Sub

#End Region

#Region " Handlers "

    Private Sub m_remotingClient_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_remotingClient.Connected

        ' Upon establishing connection with the service's communication client, we'll send our information to the 
        ' service so the service can keep track of all the client that are connected to its communication server.
        m_remotingClient.Send(New ClientInfo(m_remotingClient.ClientID))

        With New StringBuilder()
            .AppendFormat("Connected to {0} [{1}]", m_serviceName, Date.Now.ToString())
            .AppendLine()
            .AppendLine()
            .Append(m_remotingClient.Status)

            UpdateStatus(.ToString(), 1)
        End With

    End Sub

    Private Sub m_remotingClient_Connecting(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_remotingClient.Connecting

        UpdateStatus(".", 1)

    End Sub

    Private Sub m_remotingClient_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_remotingClient.Disconnected

        With New StringBuilder()
            .AppendFormat("Disconnected from {0} [{1}]", m_serviceName, Date.Now.ToString())
            .AppendLine()
            .AppendLine()
            .Append(m_remotingClient.Status)

            UpdateStatus(.ToString(), 1)
        End With

    End Sub

    Private Sub m_remotingClient_ReceivedData(ByVal sender As Object, ByVal e As GenericEventArgs(Of IdentifiableItem(Of System.Guid, Byte()))) Handles m_remotingClient.ReceivedData

        Dim response As ServiceResponse = GetObject(Of ServiceResponse)(e.Argument.Item)
        If response IsNot Nothing Then
            RaiseEvent ReceivedServiceResponse(Me, New GenericEventArgs(Of ServiceResponse)(response))
            Select Case response.Type
                Case "UPDATECLIENTSTATUS"
                    UpdateStatus(response.Message)
                Case "SERVICESTATECHANGED"
                    If response.Attachments.Count > 0 Then
                        Dim state As ObjectState(Of ServiceState) = TryCast(response.Attachments(0), ObjectState(Of ServiceState))

                        If state IsNot Nothing Then
                            ' Notify change in service state by raising the ServiceStateChanged event.
                            RaiseEvent ServiceStateChanged(Me, New GenericEventArgs(Of ObjectState(Of ServiceState))(state))

                            UpdateStatus(String.Format("State of service ""{0}"" has changed to ""{1}"".", state.ObjectName, state.CurrentState), ServiceHelper.UpdateCrlfCount)
                        End If
                    End If
                Case "PROCESSSTATECHANGED"
                    If response.Attachments.Count > 0 Then
                        Dim state As ObjectState(Of ProcessState) = TryCast(response.Attachments(0), ObjectState(Of ProcessState))

                        If state IsNot Nothing Then
                            ' Notify change in process state by raising the ProcessStateChanged event.
                            RaiseEvent ProcessStateChanged(Me, New GenericEventArgs(Of ObjectState(Of ProcessState))(state))

                            UpdateStatus(String.Format("State of process ""{0}"" has changed to ""{1}"".", state.ObjectName, state.CurrentState), ServiceHelper.UpdateCrlfCount)
                        End If
                    End If
                Case "COMMANDSESSION"
                    Select Case response.Message.ToUpper()
                        Case "ESTABLISHED"
                            RaiseEvent CommandSessionEstablished(Me, EventArgs.Empty)
                        Case "TERMINATED"
                            RaiseEvent CommandSessionTerminated(Me, EventArgs.Empty)
                    End Select
            End Select
        End If

    End Sub

#End Region

#Region " Obsolete "

    <Obsolete("Property is replaced by RemotingClient and will be deleted in version 3.3."), Browsable(False), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property CommunicationClient() As CommunicationClientBase
        Get
            Return m_remotingClient
        End Get
        Set(ByVal value As CommunicationClientBase)
            m_remotingClient = value
        End Set
    End Property

#End Region

End Class