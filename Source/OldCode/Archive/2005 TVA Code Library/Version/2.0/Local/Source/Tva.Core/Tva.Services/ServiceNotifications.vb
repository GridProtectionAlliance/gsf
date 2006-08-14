' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel

Namespace Services


    ' Define the event args notification structure for standard service notifications
    <Serializable()> _
    Public Class ServiceNotificationEventArgs

        Inherits EventArgs

        Public Command As ServiceCommands
        Public Parameters As String()

        Public Sub New()

            Command = ServiceCommands.Undetermined

        End Sub

        Public Sub New(ByVal command As ServiceCommands)

            MyClass.Command = command

        End Sub

        Public Sub New(ByVal command As ServiceCommands, ByVal ParamArray parameters As String())

            MyClass.Command = command
            MyClass.Parameters = parameters

        End Sub

        Public Function TranslateCommand(ByVal command As String) As ServiceCommands

            If String.IsNullOrEmpty(command) Then Return ServiceCommands.Undetermined

            Parameters = command.Trim().ToLower().Split(" "c)

            If Parameters(0).StartsWith("pause", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.PauseService
            ElseIf Parameters(0).StartsWith("resume", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.ResumeService
            ElseIf Parameters(0).StartsWith("stop", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.StopService
            ElseIf Parameters(0).StartsWith("restart", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.RestartService
            ElseIf Parameters(0).StartsWith("list proc", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.ListProcesses
            ElseIf Parameters(0).StartsWith("start", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.StartProcess
            ElseIf Parameters(0).StartsWith("abort", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.AbortProcess
            ElseIf Parameters(0).StartsWith("unsch", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.UnscheduleProcess
            ElseIf Parameters(0).StartsWith("resch", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.RescheduleProcess
            ElseIf Parameters(0).StartsWith("ping", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.PingService
            ElseIf Parameters(0).StartsWith("ping all", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.PingAllClients
            ElseIf Parameters(0).StartsWith("list clie", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.ListClients
            ElseIf Parameters(0).StartsWith("status", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.GetServiceStatus
            ElseIf Parameters(0).StartsWith("procstat", StringComparison.InvariantCultureIgnoreCase) Then
            ElseIf Parameters(0).StartsWith("history", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.GetCommandHistory
            ElseIf Parameters(0).StartsWith("dir", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.GetDirectoryListing
            ElseIf Parameters(0).StartsWith("settings", StringComparison.InvariantCultureIgnoreCase) Then
                Return ServiceCommands.ListSettings
            ElseIf Parameters(0).StartsWith("update", StringComparison.InvariantCultureIgnoreCase) Then
            End If


            Select Case command.Trim.ToLower
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

        End Function

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