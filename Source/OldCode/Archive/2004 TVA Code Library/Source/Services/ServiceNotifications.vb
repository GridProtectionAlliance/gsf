' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel

Namespace Services

    ' Define possible service states
    <Serializable()> _
    Public Enum ServiceState
        Started
        Stopped
        Paused
        Resumed
        ShutDown
    End Enum

    ' Define possible process states for service threads that will execute code
    <Serializable()> _
    Public Enum ProcessState
        Unprocessed
        Processing
        Processed
        Aborted
    End Enum

    ' Define some standard service related notifications that can be sent from a remote client monitor
    ' that are to be handled by the service.  Note that it makes no sense for a client to request a
    ' "Start Service" since service must already be started in order for there to be any clients...
    <Serializable()> _
    Public Enum ServiceNotification
        OnDemandProcess
        AbortProcess
        PauseService
        ResumeService
        StopService
        RestartService
        PingService
        PingAllClients
        GetClientList
        GetServiceStatus
        GetCommandHistory
        GetProcessSchedule
        ReloadProcessSchedule
        RequestProcessState
        DirectoryListing
        GetAllVariables
        GetVariable
        SetVariable
        KillProcess
        Undetermined
    End Enum

    ' Define some standard notifications that will be sent from the service that can be handled by remote monitoring clients
    <Serializable()> _
    Public Enum ServiceMonitorNotification
        ProcessStarted
        ProcessCompleted
        ProcessCanceled
        Undetermined
    End Enum

    ' Define the event args notification structure for standard service notifications
    <Serializable()> _
    Public Class ServiceNotificationEventArgs : Inherits EventArgs

        Public Notification As ServiceNotification
        Public EventItemName As String
        Public EventItemData As Object

        Public Sub New()

            Notification = ServiceNotification.Undetermined

        End Sub

        Public Sub New(ByVal NotificationType As ServiceNotification)

            Notification = NotificationType

        End Sub

        Public Sub New(ByVal NotificationType As ServiceNotification, ByVal ItemName As String, ByVal ItemData As Object)

            Notification = NotificationType
            EventItemName = ItemName
            EventItemData = ItemData

        End Sub

        Public Sub TranslateNotification(ByVal StrReference As String)

            Dim strRef As String = Trim(StrReference)

            Select Case LCase(strRef)
                Case "abortprocess", "abort process", "abort", "stop process"
                    Notification = ServiceNotification.AbortProcess
                Case "pauseservice", "pause service", "pause"
                    Notification = ServiceNotification.PauseService
                Case "resumeservice", "resume service", "resume"
                    Notification = ServiceNotification.ResumeService
                Case "stopservice", "stop service", "stop"
                    Notification = ServiceNotification.StopService
                Case "restartservice", "restart service", "restart"
                    Notification = ServiceNotification.RestartService
                Case "pingservice", "ping service", "ping", "service ping"
                    Notification = ServiceNotification.PingService
                Case "pingallclients", "ping all clients", "pingall", "ping all"
                    Notification = ServiceNotification.PingAllClients
                Case "getclientlist", "get client list", "clients", "client list"
                    Notification = ServiceNotification.GetClientList
                Case "getcommandhistory", "commandhistory", "get command history", "command history", "history"
                    Notification = ServiceNotification.GetCommandHistory
                Case "getprocessschedule", "get process schedule", "schedule", "show schedule"
                    Notification = ServiceNotification.GetProcessSchedule
                Case "reloadprocessschedule", "reload process schedule", "reload", "reload schedule"
                    Notification = ServiceNotification.ReloadProcessSchedule
                Case "requestprocessstate", "request process state", "get process state"
                    Notification = ServiceNotification.RequestProcessState
                Case "directorylisting", "directory listing", "directory", "dir"
                    Notification = ServiceNotification.DirectoryListing
                Case "getallvariables", "get all variables", "list variables", "settings"
                    Notification = ServiceNotification.GetAllVariables
                Case Else
                    ' Handle notifications with parameters...
                    Dim strParam As String

                    If FindItem(strRef, strParam, "OnDemandProcess", "On Demand Process", "Process", "Start Process") Then
                        EventItemData = strParam
                        Notification = ServiceNotification.OnDemandProcess
                    ElseIf FindItem(strRef, strParam, "GetServiceStatus", "GetStatus", "Get Service Status", "Get Status", "Service Status", "Status") Then
                        EventItemName = strParam
                        Notification = ServiceNotification.GetServiceStatus
                    ElseIf FindItem(strRef, strParam, "GetVariable", "Get Variable", "Get Value", "Get Setting") Then
                        EventItemName = strParam
                        Notification = ServiceNotification.GetVariable
                    ElseIf FindItem(strRef, strParam, "SetVariable", "Set Variable", "Set Value", "Setting") Then
                        Dim strItems() As String = strParam.Split("=")
                        If strItems.Length = 2 Then
                            EventItemName = Trim(strItems(0))
                            EventItemData = Trim(strItems(1))
                            Notification = ServiceNotification.SetVariable
                        Else
                            Notification = ServiceNotification.Undetermined
                        End If
                    ElseIf FindItem(strRef, strParam, "KillProcess", "Kill Process", "Terminate Process") Then
                        EventItemName = strParam
                        Notification = ServiceNotification.KillProcess
                    Else
                        Notification = ServiceNotification.Undetermined
                    End If
            End Select

        End Sub

        Private Function FindItem(ByVal StrReference As String, ByRef ItemParameter As String, ByVal ParamArray EnumAlias() As String) As Boolean

            Dim flgFound As Boolean
            Dim intPos As Integer
            Dim str As String

            For Each str In EnumAlias
                intPos = InStr(StrReference, str, CompareMethod.Text)
                If intPos > 0 Then
                    ItemParameter = Trim(Mid(StrReference, intPos + Len(str)))
                    flgFound = True
                    Exit For
                End If
            Next

            Return flgFound

        End Function

    End Class

    ' Define the event args notification structure for standard service monitor notifications
    <Serializable()> _
    Public Class ServiceMonitorNotificationEventArgs : Inherits EventArgs

        Public Notification As ServiceMonitorNotification

        Public Sub New()

            Notification = ServiceMonitorNotification.Undetermined

        End Sub

        Public Sub New(ByVal NotificationType As ServiceMonitorNotification)

            Notification = NotificationType

        End Sub

        Public Sub TranslateNotification(ByVal StrReference As String)

            Select Case LCase(Trim(StrReference))
                Case "processstarted", "process started", "started"
                    Notification = ServiceMonitorNotification.ProcessStarted
                Case "processcompleted", "process completed", "completed"
                    Notification = ServiceMonitorNotification.ProcessCompleted
                Case "processcanceled", "process canceled", "canceled"
                    Notification = ServiceMonitorNotification.ProcessCanceled
                Case Else
                    Notification = ServiceMonitorNotification.Undetermined
            End Select

        End Sub

    End Class

    ' We define three standard client event notifications to use with services
    <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Class ServiceMessageEventArgs : Inherits EventArgs

        Public Message As String
        Public LogMessage As Boolean

        Public Sub New(ByVal Message As String, ByVal LogMessage As Boolean)

            Me.Message = Message
            Me.LogMessage = LogMessage

        End Sub

    End Class

    <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Class ServiceProgressEventArgs : Inherits EventArgs

        Public BytesCompleted As Long
        Public BytesTotal As Long

        Public Sub New(ByVal BytesCompleted As Long, ByVal BytesTotal As Long)

            Me.BytesCompleted = BytesCompleted
            Me.BytesTotal = BytesTotal

        End Sub

    End Class

    <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Class ServiceStateChangedEventArgs : Inherits EventArgs

        Public NewState As ServiceState

        Public Sub New(ByVal NewState As ServiceState)

            Me.NewState = NewState

        End Sub

    End Class

End Namespace