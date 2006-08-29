'*******************************************************************************************************
'  Tva.Xml.Common.vb - Common XML Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - J. Ritchie Carroll
'       Original version of source code generated
'  01/23/2006 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
'
'*******************************************************************************************************

Imports System.ComponentModel
Imports Tva.Text.Common

Namespace Services

    ''' <summary>Event arguments for standard service commands</summary>
    <Serializable()> _
    Public Class ServiceCommandEventArgs

        Inherits EventArgs

        Public Command As ServiceRequestType
        Public Parameters As String()

        Public Sub New()

            Command = ServiceRequestType.Undetermined

        End Sub

        Public Sub New(ByVal command As ServiceRequestType)

            MyClass.Command = command

        End Sub

        Public Sub New(ByVal command As ServiceRequestType, ByVal ParamArray parameters As String())

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
                If String.IsNullOrEmpty(command) Then
                    .Command = ServiceRequestType.Undetermined
                    .Parameters = New String() {"Undetermined"}
                Else
                    .Parameters = RemoveDuplicateWhiteSpace(command).Trim().ToLower().Split(" "c)

                    If .Parameters.Length > 0 Then
                        command = .Parameters(0)

                        If command.StartsWith("pause") Then
                            .Command = ServiceRequestType.PauseService
                        ElseIf command.StartsWith("resume") Then
                            .Command = ServiceRequestType.ResumeService
                        ElseIf command.StartsWith("stop") Then
                            .Command = ServiceRequestType.StopService
                        ElseIf command.StartsWith("restart") Then
                            .Command = ServiceRequestType.RestartService
                        ElseIf command.StartsWith("list") Then
                            .Command = ServiceRequestType.ListProcesses
                        ElseIf command.StartsWith("exec") Then
                            .Command = ServiceRequestType.StartProcess
                        ElseIf command.StartsWith("abort") Then
                            .Command = ServiceRequestType.AbortProcess
                        ElseIf command.StartsWith("unsch") Then
                            .Command = ServiceRequestType.UnscheduleProcess
                        ElseIf command.StartsWith("resch") Then
                            .Command = ServiceRequestType.RescheduleProcess
                        ElseIf command.StartsWith("ping") Then
                            If .Parameters.Length > 1 Then
                                If .Parameters(1).StartsWith("all") Then
                                    .Command = ServiceRequestType.PingAllClients
                                Else
                                    .Command = ServiceRequestType.PingService
                                End If
                            Else
                                .Command = ServiceRequestType.PingService
                            End If
                        ElseIf command.StartsWith("pingall") Then
                            .Command = ServiceRequestType.PingAllClients
                        ElseIf command.StartsWith("clients") Then
                            .Command = ServiceRequestType.ListClients
                        ElseIf command.StartsWith("status") Then
                            .Command = ServiceRequestType.GetServiceStatus
                        ElseIf command.StartsWith("history") Then
                            .Command = ServiceRequestType.GetCommandHistory
                        ElseIf command.StartsWith("dir") Then
                            .Command = ServiceRequestType.GetDirectoryListing
                        ElseIf command.StartsWith("settings") Then
                            .Command = ServiceRequestType.ListSettings
                        ElseIf command.StartsWith("update") Then
                            .Command = ServiceRequestType.UpdateSetting
                        ElseIf command.StartsWith("save") Then
                            .Command = ServiceRequestType.SaveSettings
                        End If
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