'*******************************************************************************************************
'  TVA.Scheduling.ScheduleManager.vb - Monitors multiples schedules defined as TVA.Scheduling.Schedule
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
'  08/01/2006 - Pinal C. Patel
'       Original version of source code generated
'  04/23/2007 - Pinal C. Patel
'       Made the schedules dictionary case-insensitive
'  04/24/2007 - Pinal C. Patel
'       Implemented the IPersistSettings and ISupportInitialize interfaces
'  05/02/2007 - Pinal C. Patel
'       Converted schedules to a list instead of a dictionary
'  11/30/2007 - Pinal C. Patel
'       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
'       instead of DesignMode property as the former is more accurate than the latter
'       Modified the Stop() method to avoid a  null reference exception that was most likely causing
'       the IDE of the component's consumer to crash in design-mode
'
'*******************************************************************************************************

Option Strict On

Imports System.Drawing
Imports System.ComponentModel
Imports System.Threading
Imports TVA.Services
Imports TVA.Configuration
Imports TVA.Threading

Namespace Scheduling

    <ToolboxBitmap(GetType(ScheduleManager)), DisplayName("ScheduleManager"), DefaultEvent("ScheduleDue")> _
    Public Class ScheduleManager
        Implements IServiceComponent, IPersistSettings, ISupportInitialize

#Region " Member Declaration "

        Private m_enabled As Boolean
        Private m_schedules As List(Of Schedule)
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String
#If ThreadTracking Then
        Private m_startTimerThread As ManagedThread
#Else
        Private m_startTimerThread As Thread
#End If
        Private m_scheduleDueEventHandlerList As List(Of EventHandler(Of ScheduleEventArgs))

        Private WithEvents m_timer As System.Timers.Timer

#End Region

#Region " Event Declaration "

        ''' <summary>
        ''' Occurs while the schedule manager is waiting to start at top of the minute.
        ''' </summary>
        <Category("State")> _
        Public Event Starting As EventHandler

        ''' <summary>
        ''' Occurs when the schedule manager has started.
        ''' </summary>
        <Category("State")> _
        Public Event Started As EventHandler

        ''' <summary>
        ''' Occurs when the schedule manager has stopped.
        ''' </summary>
        <Category("State")> _
        Public Event Stopped As EventHandler

        ''' <summary>
        ''' Occurs when the a particular schedule is being checked to see if it is due.
        ''' </summary>
        <Category("Schedules")> _
        Public Event CheckingSchedule As EventHandler(Of ScheduleEventArgs)

        ''' <summary>
        ''' Occurs when a schedule is due according to the rule specified for the schedule.
        ''' </summary>
        <Category("Schedules")> _
        Public Custom Event ScheduleDue As EventHandler(Of ScheduleEventArgs)
            AddHandler(ByVal value As EventHandler(Of ScheduleEventArgs))
                m_scheduleDueEventHandlerList.Add(value)
            End AddHandler

            RemoveHandler(ByVal value As EventHandler(Of ScheduleEventArgs))
                m_scheduleDueEventHandlerList.Remove(value)
            End RemoveHandler

            RaiseEvent(ByVal sender As Object, ByVal e As ScheduleEventArgs)
                For Each handler As EventHandler(Of ScheduleEventArgs) In m_scheduleDueEventHandlerList
                    handler.BeginInvoke(sender, e, Nothing, Nothing)
                Next
            End RaiseEvent
        End Event

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the schedule manager is enabled.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the schedule manager is enabled; otherwise False.</returns>
        <Category("Behavior"), DefaultValue(GetType(Boolean), "True")> _
        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal value As Boolean)
                m_enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating whether the schedule manager is running.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the schedule manager is running; otherwise False.</returns>
        <Browsable(False)> _
        Public ReadOnly Property IsRunning() As Boolean
            Get
                Return m_timer.Enabled
            End Get
        End Property

        ''' <summary>
        ''' Gets a list of all the schedules.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A list of the schedules.</returns>
        <Browsable(False)> _
        Public ReadOnly Property Schedules() As List(Of Schedule)
            Get
                Return m_schedules
            End Get
        End Property

        ''' <summary>
        ''' Gets the schedule with the specified schedule name.
        ''' </summary>
        ''' <param name="scheduleName">Name of the schedule that is to be found.</param>
        ''' <value></value>
        ''' <returns>The TVA.Scheduling.Schedule instance for the specified schedule name if found; otherwise Nothing.</returns>
        <Browsable(False)> _
        Public ReadOnly Property Schedules(ByVal scheduleName As String) As Schedule
            Get
                Dim match As Schedule = Nothing
                For Each schedule As Schedule In m_schedules
                    If String.Compare(schedule.Name, scheduleName, True) = 0 Then
                        match = schedule
                        Exit For
                    End If
                Next
                Return match
            End Get
        End Property

        ''' <summary>
        ''' Starts the schedule manager asynchronously.
        ''' </summary>
        Public Sub Start()

            If m_enabled AndAlso Not m_timer.Enabled Then
#If ThreadTracking Then
                m_startTimerThread = New ManagedThread(AddressOf StartTimer)
                m_startTimerThread.Name = "TVA.Scheduling.ScheduleManager.StartTimer()"
#Else
                m_startTimerThread = New Thread(AddressOf StartTimer)
#End If
                m_startTimerThread.Start()
            End If

        End Sub

        ''' <summary>
        ''' Stops the schedule manager.
        ''' </summary>
        Public Sub [Stop]()

            If m_enabled Then
                If m_startTimerThread IsNot Nothing AndAlso m_startTimerThread.IsAlive Then
                    m_startTimerThread.Abort()
                End If
                If m_timer.Enabled Then
                    m_timer.Stop()
                    RaiseEvent Stopped(Me, EventArgs.Empty)
                End If
            End If

        End Sub

        ''' <summary>
        ''' Checks all of the schedules to determine if they are due.
        ''' </summary>
        Public Sub CheckAllSchedules()

            If m_enabled Then
                For Each schedule As Schedule In m_schedules
                    RaiseEvent CheckingSchedule(Me, New ScheduleEventArgs(schedule))
                    If schedule.IsDue() Then
                        RaiseEvent ScheduleDue(Me, New ScheduleEventArgs(schedule))   ' Event raised asynchronously.
                    End If
                Next
            End If

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub StartTimer()

            Do While True
                RaiseEvent Starting(Me, EventArgs.Empty)
                If System.DateTime.Now.Second = 0 Then
                    ' We'll start the timer that will check the schedules at top of the minute.
                    m_timer.Start()
                    RaiseEvent Started(Me, EventArgs.Empty)
                    CheckAllSchedules()

                    Exit Do
                Else
                    System.Threading.Thread.Sleep(500)
                End If
            Loop

        End Sub

        Private Sub m_timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_timer.Elapsed

            CheckAllSchedules()

        End Sub

#End Region

#Region " Interface Implementation "

#Region " IServiceComponent "

        Private m_previouslyEnabled As Boolean = False

        <Browsable(False)> _
        Public ReadOnly Property Name() As String Implements Services.IServiceComponent.Name
            Get
                Return Me.GetType().Name
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Status() As String Implements Services.IServiceComponent.Status
            Get
                With New System.Text.StringBuilder()
                    .Append("        Number of schedules: ")
                    .Append(m_schedules.Count)
                    .AppendLine()
                    .AppendLine()
                    For Each schedule As Schedule In m_schedules
                        .Append(schedule.Status)
                        .AppendLine()
                    Next

                    Return .ToString()
                End With
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal processName As String, ByVal newState As Services.ProcessState) Implements Services.IServiceComponent.ProcessStateChanged

        End Sub

        Public Sub ServiceStateChanged(ByVal newState As Services.ServiceState) Implements Services.IServiceComponent.ServiceStateChanged

            Select Case newState
                Case ServiceState.Started
                    Me.Start()
                Case ServiceState.Stopped, ServiceState.Shutdown
                    Me.Stop()
                Case ServiceState.Paused
                    m_previouslyEnabled = Enabled
                    Me.Enabled = False
                Case ServiceState.Resumed
                    Me.Enabled = m_previouslyEnabled
                Case ServiceState.Shutdown
                    Me.Dispose()
            End Select

        End Sub

#End Region

#Region " IPersistSettings "

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the component settings are to be persisted to the config file.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the component settings are to be persisted to the config file; otherwise False.</returns>
        <Description("Indicates whether the component settings are to be persisted to the config file."), DefaultValue(GetType(Boolean), "False")> _
        Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
            Get
                Return m_persistSettings
            End Get
            Set(ByVal value As Boolean)
                m_persistSettings = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the category name under which the component settings are to be saved in the config file.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The category name under which the component settings are to be saved in the config file.</returns>
        <Description("The category name under which the component settings are to be saved in the config file."), DefaultValue(GetType(String), "LogFile")> _
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

        ''' <summary>
        ''' Loads previously saved schedules from the config file.
        ''' </summary>
        Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

            Try
                For Each schedule As CategorizedSettingsElement In TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    ' Add the schedule if it doesn't exist or update it otherwise with data from the config file.
                    Dim existingSchedule As Schedule = Schedules(schedule.Name)
                    If existingSchedule Is Nothing Then
                        ' Schedule doesn't exist, so we'll add it.
                        m_schedules.Add(New Schedule(schedule.Name, schedule.Value, schedule.Description))
                    Else
                        ' Schedule does exist, so we'll update it.
                        existingSchedule.Name = schedule.Name
                        existingSchedule.Rule = schedule.Value
                        existingSchedule.Description = schedule.Description
                    End If
                Next
            Catch ex As Exception
                ' We'll encounter exceptions if the settings are not present in the config file.
            End Try

        End Sub

        ''' <summary>
        ''' Saves all schedules to the config file.
        ''' </summary>
        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                        .Clear()
                        For Each schedule As Schedule In m_schedules
                            .Add(schedule.Name, schedule.Rule, schedule.Description)
                        Next
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

            If LicenseManager.UsageMode = LicenseUsageMode.Runtime Then
                LoadSettings()            ' Load settings from the config file.
            End If

        End Sub

#End Region

#End Region

    End Class

End Namespace