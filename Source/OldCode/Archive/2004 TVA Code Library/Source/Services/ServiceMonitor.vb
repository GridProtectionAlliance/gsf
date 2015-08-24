' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Drawing
Imports System.ServiceProcess
Imports System.Threading
Imports System.Runtime.Remoting.Channels
Imports TVA.Remoting
Imports TVA.Shared.String
Imports TVA.Shared.Common

Namespace Services

    ' The service monitor class simply "extends" the functionality of any standard remoting client
    ' to include handy Windows service related messages...
    <Serializable(), ToolboxBitmap(GetType(ServiceMonitor), "ServiceMonitor.bmp"), DefaultEvent("ServerNotification"), DefaultProperty("RemotingClient")> _
    Public Class ServiceMonitor

        Inherits ClientBase
        Implements IComponent

        Public CurrentServiceState As ServiceState                                              ' Current reported service state
        Public CurrentProcessingState As ServiceMonitorNotification                             ' Current reported processing state
        <NonSerialized()> Private AutoReconnect As Boolean = True                               ' Set to True to automatically start connect cycle after service stops
        <NonSerialized()> Private WithEvents InternalRemotingClient As IClient                  ' IClient implementation to use for the remote service monitor

        Public Event ServiceMessage(ByVal Message As String, ByVal LoggedMessage As Boolean)    ' Text based service message to display
        Public Event ServiceProgress(ByVal BytesCompleted As Long, ByVal BytesTotal As Long)    ' Current service processing progress
        Public Event ServiceStateChanged(ByVal NewState As ServiceState)                        ' New service state
        Public Event ServiceProcessStateChanged(ByVal NewState As ServiceMonitorNotification)   ' New service process state

        ' Component Implementation
        Private ComponentSite As ISite
        Public Event Disposed(ByVal sender As Object, ByVal e As EventArgs) Implements IComponent.Disposed

        Public Sub New()

            MyBase.New()
            MyBase.NoActivityTestInterval = 0

            RegisterClientFunction = Nothing
            UnregisterClientFunction = Nothing

            CurrentServiceState = ServiceState.Stopped
            CurrentProcessingState = ServiceMonitorNotification.Undetermined

        End Sub

        Public Sub New(ByVal RemotingClient As IClient)

            MyClass.New()

            ' We delegate all calls and events to this remoting client yet extend its functionality
            ' with Windows service related methods and events...
            Me.RemotingClient = RemotingClient

        End Sub

        <Browsable(True), Category("Service Monitor"), Description("IClient based class instance to use for handling the remote service connection.")> _
        Public Property RemotingClient() As IClient
            Get
                Return InternalRemotingClient
            End Get
            Set(ByVal Value As IClient)
                InternalRemotingClient = Value

                ' The service monitor instance will be the IClient remoted to the server, however the internal remoting
                ' client will be the one actually handling the communications so we change the client registration
                ' functions to implement this change in functionality...
                With InternalRemotingClient
                    .RegisterClientFunction = AddressOf RegisterServiceMonitorClient
                    .UnregisterClientFunction = AddressOf UnregisterServiceMonitorClient
                End With
            End Set
        End Property

        <Browsable(True), Category("Service Monitor"), Description("Set to True to automatically start connect cycle after service stops."), DefaultValue(True)> _
        Public Property AutoReconnectOnStop() As Boolean
            Get
                Return AutoReconnect
            End Get
            Set(ByVal Value As Boolean)
                AutoReconnect = Value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overridable Overloads Property Site() As ISite Implements IComponent.Site
            Get
                Return ComponentSite
            End Get
            Set(ByVal Value As ISite)
                ComponentSite = Value
            End Set
        End Property

        ' We delegate all overridable properties to the internal remoting client...
        Public Overrides Sub Shutdown()

            InternalRemotingClient.Shutdown()
            RaiseEvent Disposed(Me, EventArgs.Empty)

        End Sub

        Public Overrides Sub Connect(Optional ByVal StartDelay As Boolean = False)

            InternalRemotingClient.Connect(StartDelay)

        End Sub

        Public Overrides Sub Disconnect()

            InternalRemotingClient.Disconnect()

        End Sub

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overrides Property URI() As String
            Get
                If DesignMode Then
                    Return Nothing
                Else
                    Return InternalRemotingClient.URI
                End If
            End Get
            Set(ByVal Value As String)
                If Not DesignMode Then InternalRemotingClient.URI = Value
            End Set
        End Property

        <Browsable(False)> _
        Public Overrides ReadOnly Property BaseURI() As String
            Get
                Return InternalRemotingClient.BaseURI
            End Get
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overrides Property TCPPort() As Integer
            Get
                If DesignMode Then
                    Return 0
                Else
                    Return InternalRemotingClient.TCPPort
                End If
            End Get
            Set(ByVal Value As Integer)
                If Not DesignMode Then InternalRemotingClient.TCPPort = Value
            End Set
        End Property

        <Browsable(False)> _
        Public Overrides ReadOnly Property Connected() As Boolean
            Get
                Return InternalRemotingClient.Connected
            End Get
        End Property

        <Browsable(False)> _
        Public Overrides ReadOnly Property ConnectTime() As DateTime
            Get
                Return InternalRemotingClient.ConnectTime
            End Get
        End Property

        <Browsable(False)> _
        Public Overrides ReadOnly Property Description() As String
            Get
                Return "ServiceMonitor extending " & InternalRemotingClient.Description
            End Get
        End Property

        <Browsable(False)> _
        Public Overrides ReadOnly Property ID() As System.Guid
            Get
                Return InternalRemotingClient.ID
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ServiceName() As String
            Get
                Return HostID
            End Get
        End Property

        <Browsable(False)> _
        Public Overrides ReadOnly Property HostID() As String
            Get
                Return InternalRemotingClient.HostID
            End Get
        End Property

        Public Overrides Sub NewActivity()

            InternalRemotingClient.NewActivity()

        End Sub

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overrides Property NoActivityTestInterval() As Integer
            Get
                If DesignMode Then
                    Return 600
                Else
                    Return InternalRemotingClient.NoActivityTestInterval
                End If
            End Get
            Set(ByVal Value As Integer)
                If Not DesignMode Then InternalRemotingClient.NoActivityTestInterval = Value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overrides Property RestartDelayInterval() As Integer
            Get
                If DesignMode Then
                    Return 30
                Else
                    Return InternalRemotingClient.RestartDelayInterval
                End If
            End Get
            Set(ByVal Value As Integer)
                If Not DesignMode Then InternalRemotingClient.RestartDelayInterval = Value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overrides Property ConnectionAttemptInterval() As Integer
            Get
                If DesignMode Then
                    Return 2
                Else
                    Return InternalRemotingClient.ConnectionAttemptInterval
                End If
            End Get
            Set(ByVal Value As Integer)
                If Not DesignMode Then InternalRemotingClient.ConnectionAttemptInterval = Value
            End Set
        End Property

        Public Overrides Sub SendStatusMessage(ByVal Message As String, ByVal NewLine As Boolean)

            InternalRemotingClient.SendStatusMessage(Message, NewLine)

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub SendClientNotification(ByVal sender As Object, ByVal e As EventArgs)

            InternalRemotingClient.SendClientNotification(sender, e)

        End Sub

        Public Overrides Sub SendNotification(ByVal sender As Object, ByVal e As System.EventArgs)

            InternalRemotingClient.SendNotification(sender, e)

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub UpdateCurrentURI(ByVal URI As String)

            InternalRemotingClient.UpdateCurrentURI(URI)

        End Sub

        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overrides Property RemoteReference() As IServer
            Get
                Return InternalRemotingClient.RemoteReference
            End Get
            Set(ByVal Value As IServer)
                InternalRemotingClient.RemoteReference = Value
            End Set
        End Property

        Public Sub RequestCurrentProcessingState(Optional ByVal ProcessIndex As Integer = 0)

            SendSafeNotification(Nothing, New ServiceNotificationEventArgs(ServiceNotification.RequestProcessState, "", ProcessIndex))

        End Sub

        ' We override the standard send safe notification routine to handle "special" circustances associated with
        ' implementing a client connected to a remote server hosted by a "service"
        Public Overrides Function SendSafeNotification(ByVal sender As Object, ByVal e As EventArgs) As Boolean

            Dim flgSent As Boolean = False  ' Any failures should yield "false"
            Dim orgServiceState As ServiceState = CurrentServiceState

            Try
                SendNotification(sender, e)
                flgSent = True
            Catch ex As Exception
                If orgServiceState <> ServiceState.Stopped And orgServiceState <> ServiceState.ShutDown Then
                    If TypeOf e Is ServiceNotificationEventArgs Then
                        With DirectCast(e, ServiceNotificationEventArgs)
                            If .Notification = Services.ServiceNotification.RestartService Or .Notification = Services.ServiceNotification.StopService Then
                                ' Restarts and stops may correctly throw TCP channel termination errors,
                                ' so we just intercept these...
                                flgSent = True
                            End If
                        End With
                    End If
                End If

                If Not flgSent Then SendStatusMessage("Error reported from service during notification request: " & ex.Message, True)
            End Try

            Return flgSent

        End Function

        Public Overrides Function ServerIsResponding(Optional ByVal Timeout As Integer = 5) As Boolean

            Return InternalRemotingClient.ServerIsResponding(Timeout)

        End Function

        Public Overrides Function CreateClientProviderChain() As IClientChannelSinkProvider

            Throw New NotImplementedException("This functionality is delegated to remoting client passed into constructor...")

        End Function

        Public Overrides Function CreateServerProviderChain() As IServerChannelSinkProvider

            Throw New NotImplementedException("This functionality is delegated to remoting client passed into constructor...")

        End Function

        Protected Overrides Sub StartCommunications()

            Throw New NotImplementedException("This functionality is delegated to remoting client passed into constructor...")

        End Sub

        Protected Overrides Sub StopCommunications()

            Throw New NotImplementedException("This functionality is delegated to remoting client passed into constructor...")

        End Sub

        ' We register the "external" class instance as the remoted IClient instance instead of the internal class
        ' instance so we can intercept and delegate functionality as needed - but we let the internal remoting
        ' class handle all of the communications work...
        Private Function RegisterServiceMonitorClient() As Boolean

            If Not InternalRemotingClient Is Nothing Then
                Return InternalRemotingClient.RemoteRegisterClient(ID, Me)
            Else
                Return False
            End If

        End Function

        Private Function UnregisterServiceMonitorClient() As Boolean

            If Not InternalRemotingClient Is Nothing Then
                Return InternalRemotingClient.RemoteUnregisterClient(ID)
            Else
                Return False
            End If

        End Function

        ' We intercept remoting client server notifications to simplfy handling of messages related to Windows services
        Private Sub InternalRemotingClient_ServerNotification(ByVal sender As Object, ByVal e As EventArgs) Handles InternalRemotingClient.ServerNotification

            If Not e Is Nothing Then
                Try
                    If TypeOf e Is ServiceMessageEventArgs Then
                        With DirectCast(e, ServiceMessageEventArgs)
                            RaiseEvent ServiceMessage(.Message, .LogMessage)
                        End With
                    ElseIf TypeOf e Is ServiceProgressEventArgs Then
                        With DirectCast(e, ServiceProgressEventArgs)
                            RaiseEvent ServiceProgress(.BytesCompleted, .BytesTotal)
                        End With
                    ElseIf TypeOf e Is ServiceStateChangedEventArgs Then
                        With DirectCast(e, ServiceStateChangedEventArgs)
                            CurrentServiceState = .NewState
                            RaiseEvent ServiceStateChanged(CurrentServiceState)
                            If CurrentServiceState = ServiceState.Stopped Then
                                If AutoReconnect Then
                                    Connect(True)
                                Else
                                    Disconnect()
                                End If
                            End If
                        End With
                    ElseIf TypeOf e Is ServiceMonitorNotificationEventArgs Then
                        With DirectCast(e, ServiceMonitorNotificationEventArgs)
                            CurrentProcessingState = .Notification
                            RaiseEvent ServiceProcessStateChanged(CurrentProcessingState)
                        End With
                    End If
                Catch ex As Exception
                    ' We don't stop RPC processing because of end user code boo-boo's - base class provides an event
                    ' so that user can know when their code threw an exception...
                    If TypeOf e Is ServiceMessageEventArgs Then
                        RaiseEvent_UserEventHandlerException("ServiceMonitor::ServiceMessage", ex)
                    ElseIf TypeOf e Is ServiceProgressEventArgs Then
                        RaiseEvent_UserEventHandlerException("ServiceMonitor::ServiceProgress", ex)
                    ElseIf TypeOf e Is ServiceStateChangedEventArgs Then
                        RaiseEvent_UserEventHandlerException("ServiceMonitor::ServiceStateChanged", ex)
                    ElseIf TypeOf e Is ServiceMonitorNotificationEventArgs Then
                        RaiseEvent_UserEventHandlerException("ServiceMonitor::ServiceProcessStateChanged", ex)
                    End If
                End Try
            End If

            ' Pass message along through server notification event so end user custom messages can be picked up...
            RaiseEvent_ServerNotification(sender, e)

        End Sub

        ' For all other events, we just bubble these up through this class...
        Private Sub InternalRemotingClient_AttemptingConnection() Handles InternalRemotingClient.AttemptingConnection

            RaiseEvent_AttemptingConnection()

        End Sub

        Private Sub InternalRemotingClient_ConnectionAttemptFailed(ByVal ex As Exception) Handles InternalRemotingClient.ConnectionAttemptFailed

            RaiseEvent_ConnectionAttemptFailed(ex)

        End Sub

        Private Sub InternalRemotingClient_ConnectionEstablished() Handles InternalRemotingClient.ConnectionEstablished

            RaiseEvent_ConnectionEstablished()

        End Sub

        Private Sub InternalRemotingClient_ConnectionTerminated() Handles InternalRemotingClient.ConnectionTerminated

            RaiseEvent_ConnectionTerminated()

        End Sub

        Private Sub InternalRemotingClient_StatusMessage(ByVal Message As String, ByVal NewLine As Boolean) Handles InternalRemotingClient.StatusMessage

            RaiseEvent_StatusMessage(Message, NewLine)

        End Sub

        Private Sub InternalRemotingClient_UserEventHandlerException(ByVal EventName As String, ByVal ex As System.Exception) Handles InternalRemotingClient.UserEventHandlerException

            RaiseEvent_UserEventHandlerException(EventName, ex)

        End Sub

        ' Safely determines if system is in design mode
        Public ReadOnly Property DesignMode() As Boolean
            Get
                If ComponentSite Is Nothing Then
                    Return False
                Else
                    Return ComponentSite.DesignMode
                End If
            End Get
        End Property

        ' We also provide some handy service control functionality for remote service monitors...
        Public Shared Function HandleServiceControlCommands(ByVal CommandLine As String, ByVal ServiceName As String) As Boolean

            If Len(CommandLine) > 0 Then
                Dim intCommands As Integer = SubStrCount(CommandLine, " ")
                If intCommands > 2 Then
                    ' Handle /Command ServiceName MachineName
                    Select Case SubStr(CommandLine, 1, " ").ToLower()
                        Case "/stop"
                            StopService(SubStr(CommandLine, 2, " "), SubStr(CommandLine, 3, " "))
                            Return True
                        Case "/start"
                            StartService(SubStr(CommandLine, 2, " "), SubStr(CommandLine, 3, " "))
                            Return True
                        Case "/restart"
                            RestartService(SubStr(CommandLine, 2, " "), SubStr(CommandLine, 3, " "))
                            Return True
                    End Select
                ElseIf intCommands > 1 Then
                    ' Handle /Command ServiceName <using local machine>
                    Select Case SubStr(CommandLine, 1, " ").ToLower()
                        Case "/stop"
                            StopService(SubStr(CommandLine, 2, " "))
                            Return True
                        Case "/start"
                            StartService(SubStr(CommandLine, 2, " "))
                            Return True
                        Case "/restart"
                            RestartService(SubStr(CommandLine, 2, " "))
                            Return True
                        Case "/shell"
                            Shell(SubStr(CommandLine, 2, " "), AppWinStyle.NormalFocus, False)
                            Return True
                    End Select
                Else
                    ' Handle /Command <using default service name and local machine>
                    Select Case SubStr(CommandLine, 1, " ").ToLower()
                        Case "/stop"
                            StopService(ServiceName)
                            Return True
                        Case "/start"
                            StartService(ServiceName)
                            Return True
                        Case "/restart"
                            RestartService(ServiceName)
                            Return True
                    End Select
                End If
            End If

            Return False

        End Function

        Public Shared Function GetServiceState(ByVal ServiceName As String, Optional ByVal MachineName As String = "") As ServiceControllerStatus

            Return CreateServiceController(ServiceName, MachineName).Status

        End Function

        Public Shared Function StopService(ByVal ServiceName As String, Optional ByVal MachineName As String = "") As Boolean

            Dim flgStopped As Boolean

            With CreateServiceController(ServiceName, MachineName)
                If .CanStop Then
                    If .Status <> ServiceControllerStatus.Stopped And .Status <> ServiceControllerStatus.StopPending Then
                        .Stop()
                        flgStopped = True
                    End If
                End If
            End With

            Return flgStopped

        End Function

        Public Shared Function StartService(ByVal ServiceName As String, Optional ByVal MachineName As String = "") As Boolean

            Dim flgStarted As Boolean

            With CreateServiceController(ServiceName, MachineName)
                If .Status = ServiceControllerStatus.Stopped Then
                    .Start()
                    flgStarted = True
                End If
            End With

            Return flgStarted

        End Function

        Public Shared Function RestartService(ByVal ServiceName As String, Optional ByVal MachineName As String = "") As Boolean

            Dim flgRestarted As Boolean

            With CreateServiceController(ServiceName, MachineName)
                If .CanStop Then
                    If .Status <> ServiceControllerStatus.Stopped And .Status <> ServiceControllerStatus.StopPending Then
                        .Stop()

                        While .Status <> ServiceControllerStatus.Stopped
                            Thread.Sleep(0)
                            .Refresh()
                        End While

                        .Start()
                        flgRestarted = True
                    End If
                End If
            End With

            Return flgRestarted

        End Function

        Public Shared Function CreateServiceController(ByVal ServiceName As String, Optional ByVal MachineName As String = "") As ServiceController

            If Len(MachineName) > 0 Then
                Return New ServiceController(ServiceName, MachineName)
            Else
                Return New ServiceController(ServiceName)
            End If

        End Function

    End Class

End Namespace