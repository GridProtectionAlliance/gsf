'***********************************************************************
'  PrimaryServiceProcess.vb - TVA Service Template
'  Copyright © [!output CURR_YEAR] - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: [!output DEV_NAME]
'      Office: [!output DEV_OFFICE]
'       Phone: [!output DEV_PHONE]
'       Email: [!output DEV_EMAIL]
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  [!output CURR_DATE] - [!output USER_NAME]
'       Initial version of source generated for new Windows service
'       project "[!output PROJECT_NAME]".
'
'***********************************************************************

Imports System.Text
Imports TVA.Config.Common
Imports TVA.Shared.DateTime
Imports TVA.Services
Imports TVA.ESO.Ssam
Imports TVA.ESO.Ssam.SsamEntityTypeID
Imports TVA.ESO.Ssam.SsamEventTypeID

Public Class PrimaryServiceProcess

    Implements IServiceComponent
    Implements IDisposable

#Region " Common Primary Service Process Code "

    Private Parent As [!output PROJECT_ID]
    Private IsEnabled As Boolean
    Private IsProcessing As Boolean
    Private StartTime As Single
    Private StopTime As Single

    ' Class auto-generated using TVA service template at [!output GEN_TIME]
    Public Sub New(ByVal Parent As [!output PROJECT_ID])

        Me.Parent = Parent
        Me.IsEnabled = True
        Me.IsProcessing = False
        Variables.Create("Ssam.[!output PROJECT_ID]PrimaryProcess", "PR_[!output CAP_PROJECT_ID]_PRIMARYPROCESS", VariableType.Text, "SSAM Entity ID: SSAM [!output PROJECT_NAME] Primary Service Process")
        Variables.Save() ' Flush new variable to config file at creation, if added

    End Sub

    Protected Overrides Sub Finalize()

        MyBase.Finalize()
        Dispose()

    End Sub

    Public Overridable Sub Dispose() Implements IServiceComponent.Dispose, IDisposable.Dispose

        GC.SuppressFinalize(Me)

        ' Any needed shutdown code for your primary service process should be added here - note that this class
        ' instance is available for the duration of the service lifetime...

    End Sub

    ' This function will log a SSAM event for the primary service process
    Public Sub LogSSAMEvent(ByVal EventType As SsamEventTypeID, Optional ByVal Message As String = Nothing, Optional ByVal Description As String = Nothing)

        Parent.LogSSAMEvent(EventType, ssamProcessEntity, Variables("Ssam.[!output PROJECT_ID]PrimaryProcess"), Message, Description)

    End Sub

    ' This function handles updating status for the primary service process
    Public Sub UpdateStatus(ByVal Status As String, Optional ByVal LogStatusToEventLog As Boolean = False, Optional ByVal EntryType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information)

        Parent.UpdateStatus(Status, LogStatusToEventLog, EntryType)

        ' We send any logged error activity to SSAM - be careful when logging warnings as this will turn the SSAM light yellow...
        If LogStatusToEventLog Then
            Select Case EntryType
                Case EventLogEntryType.Warning
                    LogSSAMEvent(ssamWarningEvent, "[!output PROJECT_NAME] logged a warning message for the primary process", Status)
                Case EventLogEntryType.Error, EventLogEntryType.FailureAudit
                    LogSSAMEvent(ssamErrorEvent, "[!output PROJECT_NAME] logged an error message for the primary process", Status)
            End Select
        End If

    End Sub

    Public Sub UpdateProgress(ByVal current As Long, ByVal total As Long)

        Parent.ServiceHelper.SendServiceProgress(current, total)

    End Sub

    Public Property Enabled() As Boolean
        Get
            Return IsEnabled
        End Get
        Set(ByVal Value As Boolean)
            IsEnabled = Value
        End Set
    End Property

    Public Property Processing() As Boolean
        Get
            Return IsProcessing
        End Get
        Set(ByVal Value As Boolean)
            IsProcessing = Value

            If IsProcessing Then
                StartTime = Timer
                StopTime = 0
            Else
                StopTime = Timer
            End If
        End Set
    End Property

    Public ReadOnly Property RunTime() As String
        Get
            Dim ProcessingTime As Single

            If StartTime > 0 Then
                If StopTime > 0 Then
                    ProcessingTime = StopTime - StartTime
                Else
                    ProcessingTime = Timer - StartTime
                End If
            End If

            If ProcessingTime < 0 Then ProcessingTime = 0

            Return SecondsToText(ProcessingTime)
        End Get
    End Property

#End Region

    Public Sub ExecuteProcess(ByVal UserData As Object)

        UpdateStatus("Executing primary service process...")
        Processing = True

        Try
            ' TODO: Add code that executes your primary service process here...
            ' NOTE: Any code added here is being executed on an independent thread that will be suspended
            ' if the service is paused and resumed if the service is resumed

        Catch ex As Exception
            ' Log exception and report error to SSAM
            UpdateStatus("Primary service process failed due to exception: " & ex.Message, True, EventLogEntryType.Error)

            ' Rethrow this error back to service helper so it knows process failed
            Throw
        Finally
            Processing = False
        End Try

        ' Log successful process run to SSAM
        LogSSAMEvent(ssamSuccessEvent, "Primary service process completed successfully")

    End Sub

#Region " IService Component Implementation "

    ' Service component implementation
    Public ReadOnly Property Name() As String Implements IServiceComponent.Name
        Get
            Return Me.GetType.Name
        End Get
    End Property

    Public Sub ProcessStateChanged(ByVal NewState As ProcessState) Implements IServiceComponent.ProcessStateChanged

        ' This class executes as a result of a change in process state, so nothing to do...

    End Sub

    Public Sub ServiceStateChanged(ByVal NewState As ServiceState) Implements IServiceComponent.ServiceStateChanged

        ' We respect changes in service state by enabling or disabling our processing state as needed...
        Select Case NewState
            Case ServiceState.Paused
                Enabled = False
            Case ServiceState.Resumed
                Enabled = True
        End Select

    End Sub

    Public ReadOnly Property Status() As String Implements IServiceComponent.Status
        Get
            Dim statusText As New StringBuilder

            statusText.Append("Primary service process is: " & IIf(Enabled, "Enabled", "Disabled") & vbCrLf)
            statusText.Append("  Current processing state: " & IIf(Processing, "Executing", "Idle") & vbCrLf)
            statusText.Append("    Total process run time: " & RunTime() & vbCrLf)

            Return statusText.ToString()
        End Get
    End Property

#End Region

End Class