' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel

Namespace Services


    ''' <summary>Event arguments for standard service commands</summary>
    <Serializable()> _
    Public Class ServiceCommandEventArgs

        Inherits EventArgs

        Public Command As ServiceCommand
        Public Parameters As String()

        Public Sub New()

            Command = ServiceCommand.Undetermined

        End Sub

        Public Sub New(ByVal command As ServiceCommand)

            MyClass.Command = command

        End Sub

        Public Sub New(ByVal command As ServiceCommand, ByVal ParamArray parameters As String())

            MyClass.Command = command
            MyClass.Parameters = parameters

        End Sub

        Public Sub New(ByVal command As String)

            With ParseServiceCommand(command)
                MyClass.Command = .Command
                MyClass.Parameters = .Parameters
            End With

        End Sub

        Public ReadOnly Property This() As ServiceCommandEventArgs
            Get
                Return Me
            End Get
        End Property

        Public Shared Function ParseServiceCommand(ByVal command As String) As ServiceCommandEventArgs

            With New ServiceCommandEventArgs
                If String.IsNullOrEmpty(command) Then .Command = ServiceCommand.Undetermined

                .Parameters = command.Trim().ToLower().Split(" "c)

                If .Parameters.Length > 0 Then
                    command = .Parameters(0)

                    If command.StartsWith("pause") Then
                        .Command = ServiceCommand.PauseService
                    ElseIf command.StartsWith("resume") Then
                        .Command = ServiceCommand.ResumeService
                    ElseIf command.StartsWith("stop") Then
                        .Command = ServiceCommand.StopService
                    ElseIf command.StartsWith("restart") Then
                        .Command = ServiceCommand.RestartService
                    ElseIf command.StartsWith("list") Then
                        .Command = ServiceCommand.ListProcesses
                    ElseIf command.StartsWith("exec") Then
                        .Command = ServiceCommand.StartProcess
                    ElseIf command.StartsWith("abort") Then
                        .Command = ServiceCommand.AbortProcess
                    ElseIf command.StartsWith("unsch") Then
                        .Command = ServiceCommand.UnscheduleProcess
                    ElseIf command.StartsWith("resch") Then
                        .Command = ServiceCommand.RescheduleProcess
                    ElseIf command.StartsWith("ping") Then
                        If .Parameters.Length > 1 Then
                            If .Parameters(1).StartsWith("all") Then
                                .Command = ServiceCommand.PingAllClients
                            Else
                                .Command = ServiceCommand.PingService
                            End If
                        Else
                            .Command = ServiceCommand.PingService
                        End If
                    ElseIf command.StartsWith("pingall") Then
                        .Command = ServiceCommand.PingAllClients
                    ElseIf command.StartsWith("clients") Then
                        .Command = ServiceCommand.ListClients
                    ElseIf command.StartsWith("status") Then
                        .Command = ServiceCommand.GetServiceStatus
                    ElseIf command.StartsWith("history") Then
                        .Command = ServiceCommand.GetCommandHistory
                    ElseIf command.StartsWith("dir") Then
                        .Command = ServiceCommand.GetDirectoryListing
                    ElseIf command.StartsWith("settings") Then
                        .Command = ServiceCommand.ListSettings
                    ElseIf command.StartsWith("update") Then
                        .Command = ServiceCommand.UpdateSetting
                    ElseIf command.StartsWith("save") Then
                        .Command = ServiceCommand.SaveSettings
                    End If
                End If

                Return .This
            End With

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