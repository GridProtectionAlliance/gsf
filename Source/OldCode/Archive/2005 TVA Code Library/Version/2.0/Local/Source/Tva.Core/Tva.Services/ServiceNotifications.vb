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

        Public Sub New(ByVal command As String)

        End Sub

        Public Function TranslateCommand(ByVal command As String) As ServiceCommands

            If String.IsNullOrEmpty(command) Then Return ServiceCommands.Undetermined

            Parameters = command.Trim().ToLower().Split(" "c)

            If Parameters.Length > 0 Then
                If Parameters.Length > 1 Then

                End If
                If Parameters(0).StartsWith("pause") Then
                    Return ServiceCommands.PauseService
                ElseIf Parameters(0).StartsWith("resume") Then
                    Return ServiceCommands.ResumeService
                ElseIf Parameters(0).StartsWith("stop") Then
                    Return ServiceCommands.StopService
                ElseIf Parameters(0).StartsWith("restart") Then
                    Return ServiceCommands.RestartService
                ElseIf Parameters(0).StartsWith("list") Then
                    Return ServiceCommands.ListProcesses
                ElseIf Parameters(0).StartsWith("exec") Then
                    Return ServiceCommands.StartProcess
                ElseIf Parameters(0).StartsWith("abort") Then
                    Return ServiceCommands.AbortProcess
                ElseIf Parameters(0).StartsWith("unsch") Then
                    Return ServiceCommands.UnscheduleProcess
                ElseIf Parameters(0).StartsWith("resch") Then
                    Return ServiceCommands.RescheduleProcess
                ElseIf Parameters(0).StartsWith("ping") Then
                    If Parameters.Length > 1 Then
                        If Parameters(1).StartsWith("all") Then
                            Return ServiceCommands.PingAllClients
                        Else
                            Return ServiceCommands.PingService
                        End If
                    Else
                        Return ServiceCommands.PingService
                    End If
                ElseIf Parameters(0).StartsWith("pingall") Then
                    Return ServiceCommands.PingAllClients
                ElseIf Parameters(0).StartsWith("clients") Then
                    Return ServiceCommands.ListClients
                ElseIf Parameters(0).StartsWith("status") Then
                    Return ServiceCommands.GetServiceStatus
                ElseIf Parameters(0).StartsWith("history") Then
                    Return ServiceCommands.GetCommandHistory
                ElseIf Parameters(0).StartsWith("dir") Then
                    Return ServiceCommands.GetDirectoryListing
                ElseIf Parameters(0).StartsWith("settings") Then
                    Return ServiceCommands.ListSettings
                ElseIf Parameters(0).StartsWith("update") Then
                    Return ServiceCommands.UpdateSetting
                ElseIf Parameters(0).StartsWith("save") Then
                    Return ServiceCommands.SaveSettings
                End If
            End If


        End Function

    End Class

    '' Define the event args notification structure for standard service monitor notifications
    '<Serializable()> _
    'Public Class ServiceMonitorNotificationEventArgs : Inherits EventArgs

    '    Public Notification As ServiceMonitorNotification

    '    Public Sub New()

    '        Notification = ServiceMonitorNotification.Undetermined

    '    End Sub

    '    Public Sub New(ByVal NotificationType As ServiceMonitorNotification)

    '        Notification = NotificationType

    '    End Sub

    '    Public Sub TranslateNotification(ByVal StrReference As String)

    '        Select Case LCase(Trim(StrReference))
    '            Case "processstarted", "process started", "started"
    '                Notification = ServiceMonitorNotification.ProcessStarted
    '            Case "processcompleted", "process completed", "completed"
    '                Notification = ServiceMonitorNotification.ProcessCompleted
    '            Case "processcanceled", "process canceled", "canceled"
    '                Notification = ServiceMonitorNotification.ProcessCanceled
    '            Case Else
    '                Notification = ServiceMonitorNotification.Undetermined
    '        End Select

    '    End Sub

    'End Class

    '' We define three standard client event notifications to use with services
    '<Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
    'Public Class ServiceMessageEventArgs : Inherits EventArgs

    '    Public Message As String
    '    Public LogMessage As Boolean

    '    Public Sub New(ByVal Message As String, ByVal LogMessage As Boolean)

    '        Me.Message = Message
    '        Me.LogMessage = LogMessage

    '    End Sub

    'End Class

    '<Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
    'Public Class ServiceProgressEventArgs : Inherits EventArgs

    '    Public BytesCompleted As Long
    '    Public BytesTotal As Long

    '    Public Sub New(ByVal BytesCompleted As Long, ByVal BytesTotal As Long)

    '        Me.BytesCompleted = BytesCompleted
    '        Me.BytesTotal = BytesTotal

    '    End Sub

    'End Class

    '<Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
    'Public Class ServiceStateChangedEventArgs : Inherits EventArgs

    '    Public NewState As ServiceState

    '    Public Sub New(ByVal NewState As ServiceState)

    '        Me.NewState = NewState

    '    End Sub

    'End Class

End Namespace