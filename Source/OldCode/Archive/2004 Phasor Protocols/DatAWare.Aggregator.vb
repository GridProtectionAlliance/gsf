'***********************************************************************
'  PrimaryServiceProcess.vb - TVA Service Template
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/5/2004 - James R Carroll
'       Initial version of source generated for new Windows service
'       project "DatAWare PDC".
'
'***********************************************************************

Imports System.Text
Imports TVA.Config.Common
Imports TVA.Shared.DateTime
Imports TVA.Services
Imports TVA.ESO.Ssam
Imports TVA.ESO.Ssam.SsamEntityTypeID
Imports TVA.ESO.Ssam.SsamEventTypeID

Namespace DatAWare

    Public Class Aggregator

        Implements IServiceComponent
        Implements IDisposable

#Region " Common Primary Service Process Code "

        Private m_parent As DatAWarePDC
        Private m_enabled As Boolean
        Private m_processing As Boolean
        Private m_startTime As Long
        Private m_stopTime As Long

        ' Class auto-generated using TVA service template at Fri Nov 5 09:43:23 EST 2004
        Public Sub New(ByVal parent As DatAWarePDC)

            m_parent = parent
            m_enabled = True
            m_processing = False

            Variables.Create("Ssam.DatAWareAggregator", "PR_PDCAGGREGATOR", VariableType.Text, "SSAM Entity ID: SSAM DatAWare PDC Aggregator Process")
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

            m_parent.LogSSAMEvent(EventType, ssamProcessEntity, Variables("Ssam.DatAWarePDCPrimaryProcess"), Message, Description)

        End Sub

        ' This function handles updating status for the primary service process
        Public Sub UpdateStatus(ByVal Status As String, Optional ByVal LogStatusToEventLog As Boolean = False, Optional ByVal EntryType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information)

            m_parent.UpdateStatus(Status, LogStatusToEventLog, EntryType)

            ' We send any logged error activity to SSAM - be careful when logging warnings as this will turn the SSAM light yellow...
            If LogStatusToEventLog Then
                Select Case EntryType
                    Case EventLogEntryType.Warning
                        LogSSAMEvent(ssamWarningEvent, "DatAWare PDC logged a warning message for the primary process", Status)
                    Case EventLogEntryType.Error, EventLogEntryType.FailureAudit
                        LogSSAMEvent(ssamErrorEvent, "DatAWare PDC logged an error message for the primary process", Status)
                End Select
            End If

        End Sub

        Public Sub UpdateProgress(ByVal current As Long, ByVal total As Long)

            m_parent.ServiceHelper.SendServiceProgress(current, total)

        End Sub

        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal Value As Boolean)
                m_enabled = Value
            End Set
        End Property

        Public Property Processing() As Boolean
            Get
                Return m_processing
            End Get
            Set(ByVal Value As Boolean)
                m_processing = Value

                If m_processing Then
                    m_startTime = DateTime.Now.Ticks
                    m_stopTime = 0
                Else
                    m_stopTime = DateTime.Now.Ticks
                End If
            End Set
        End Property

        Public ReadOnly Property RunTime() As Double
            Get
                Dim ProcessingTime As Long

                If m_startTime > 0 Then
                    If m_stopTime > 0 Then
                        ProcessingTime = m_stopTime - m_startTime
                    Else
                        ProcessingTime = DateTime.Now.Ticks - m_startTime
                    End If
                End If

                If ProcessingTime < 0 Then ProcessingTime = 0

                Return ProcessingTime / 10000000L
            End Get
        End Property

#End Region

        Public Sub ExecuteProcess(ByVal UserData As Object)

            ' This code archives aggregated PMU data into the long term PMU DatAWare archive...
            Processing = True

            Try
                ' TODO: Add code that executes your primary service process here...
                ' NOTE: Any code added here is being executed on an independent thread that will be suspended
                ' if the service is paused and resumed if the service is resumed

            Catch ex As Exception
                ' Log exception and report error to SSAM
                UpdateStatus("PMU data aggregation process failed due to exception: " & ex.Message, True, EventLogEntryType.Error)

                ' Rethrow this error back to service helper so it knows process failed
                Throw
            Finally
                Processing = False
            End Try

            ' Log successful process run to SSAM
            LogSSAMEvent(ssamSuccessEvent, "PMU data aggregation completed successfully")

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
                With New StringBuilder
                    .Append("    Aggregation process is: " & IIf(Enabled, "Enabled", "Disabled") & vbCrLf)
                    .Append("  Current processing state: " & IIf(Processing, "Executing", "Idle") & vbCrLf)
                    .Append("    Total process run time: " & SecondsToText(RunTime) & vbCrLf)

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

    End Class

End Namespace
