' 08-01-06

Imports System.Drawing
Imports System.ComponentModel
Imports System.Threading
Imports Tva.Configuration
Imports Tva.Configuration.Common

<ToolboxBitmap(GetType(ScheduleManager)), DefaultEvent("ProcessSchedule")> _
Public Class ScheduleManager

    Private m_configurationElement As String
    Private m_autoSaveSchedules As Boolean
    Private m_enabled As Boolean
    Private m_schedules As Dictionary(Of String, Schedule)
    Private m_startTimerThread As Thread
    Private WithEvents m_timer As System.Timers.Timer

    Public Event Starting As EventHandler
    Public Event Started As EventHandler
    Public Event Stopped As EventHandler
    Public Event CheckingSchedule(ByVal scheduleName As String)
    Public Event ProcessSchedule(ByVal scheduleName As String, ByVal schedule As Schedule)

    Public Sub New(ByVal autoSaveSchedules As Boolean)
        MyBase.New()
        MyClass.ConfigurationElement = "ScheduleManager"
        MyClass.AutoSaveSchedules = autoSaveSchedules
        MyClass.Enabled = True
        m_schedules = New Dictionary(Of String, Schedule)()
        m_timer = New System.Timers.Timer(60000)
        LoadSchedules()
    End Sub

    Public Property ConfigurationElement() As String
        Get
            Return m_configurationElement
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_configurationElement = value
            Else
                Throw New ArgumentNullException("value")
            End If
        End Set
    End Property

    Public Property AutoSaveSchedules() As Boolean
        Get
            Return m_autoSaveSchedules
        End Get
        Set(ByVal value As Boolean)
            m_autoSaveSchedules = value
        End Set
    End Property

    Public Property Enabled() As Boolean
        Get
            Return m_enabled
        End Get
        Set(ByVal value As Boolean)
            m_enabled = value
        End Set
    End Property

    <Browsable(False)> _
    Public ReadOnly Property Schedules() As Dictionary(Of String, Schedule)
        Get
            Return m_schedules
        End Get
    End Property

    Public Sub Start()

        If m_enabled Then
            m_startTimerThread = New Thread(AddressOf StartTimer)
            m_startTimerThread.Start()
        End If

    End Sub

    Public Sub [Stop]()

        If m_enabled Then
            If m_startTimerThread IsNot Nothing Then m_startTimerThread.Abort()
            If m_timer.Enabled Then
                m_timer.Stop()
                If m_autoSaveSchedules Then SaveSchedules()
                RaiseEvent Stopped(Me, EventArgs.Empty)
            End If
        End If

    End Sub

    Public Sub AddSchedule(ByVal scheduleName As String)

        AddSchedule(scheduleName, "*")

    End Sub

    Public Sub AddSchedule(ByVal scheduleName As String, ByVal minutes As String)

        AddSchedule(scheduleName, minutes, "*")

    End Sub

    Public Sub AddSchedule(ByVal scheduleName As String, ByVal minutes As String, ByVal hours As String)

        AddSchedule(scheduleName, minutes, hours, "*")

    End Sub

    Public Sub AddSchedule(ByVal scheduleName As String, ByVal minutes As String, ByVal hours As String, _
            ByVal days As String)

        AddSchedule(scheduleName, minutes, hours, days, "*")

    End Sub

    Public Sub AddSchedule(ByVal scheduleName As String, ByVal minutes As String, ByVal hours As String, _
            ByVal days As String, ByVal months As String)

        AddSchedule(scheduleName, minutes, hours, days, months, "*")

    End Sub

    Public Sub AddSchedule(ByVal scheduleName As String, ByVal minutes As String, ByVal hours As String, _
            ByVal days As String, ByVal months As String, ByVal daysOfWeek As String)

        AddSchedule(New Schedule(scheduleName, minutes, hours, days, months, daysOfWeek))

    End Sub

    Public Sub AddSchedule(ByVal schedule As Schedule)

        If Not m_schedules.ContainsKey(schedule.Name) Then m_schedules.Add(schedule.Name, schedule)
        If m_autoSaveSchedules Then
            DefaultConfigFile.CategorizedSettings(m_configurationElement).Add(schedule.Name, schedule.Rule)
            SaveSettings()
        End If

    End Sub

    Public Sub RemoveSchedule(ByVal scheduleName As String)

        m_schedules.Remove(scheduleName)
        If m_autoSaveSchedules Then
            DefaultConfigFile.CategorizedSettings(m_configurationElement).Remove(scheduleName)
            SaveSettings()
        End If

    End Sub

    ''' <summary>
    ''' Loads previously saved schedules from the application configuration file.
    ''' </summary>
    Public Sub LoadSchedules()

        If m_enabled Then
            For Each schedule As CategorizedSettingsElement In DefaultConfigFile.CategorizedSettings(m_configurationElement)
                m_schedules.Add(schedule.Name, New Schedule(schedule.Name, schedule.Value))
            Next
        End If

    End Sub

    ''' <summary>
    ''' Saves all schedules to the application configuration file.
    ''' </summary>
    Public Sub SaveSchedules()

        If m_enabled Then
            DefaultConfigFile.CategorizedSettings(m_configurationElement).Clear()
            For Each scheduleName As String In m_schedules.Keys
                DefaultConfigFile.CategorizedSettings(m_configurationElement).Add(scheduleName, m_schedules(scheduleName).Rule)
            Next
            SaveSettings()
        End If

    End Sub

    Public Sub CheckSchedule(ByVal scheduleName As String)

        If m_enabled Then
            RaiseEvent CheckingSchedule(scheduleName)
            If m_schedules(scheduleName).IsDue() Then
                ThreadPool.QueueUserWorkItem(AddressOf AsynchronousProcessSchedule, scheduleName)
            End If
        End If

    End Sub

    Public Sub CheckAllSchedules()

        If m_enabled Then
            For Each scheduleName As String In m_schedules.Keys
                CheckSchedule(scheduleName)
            Next
        End If

    End Sub

    Private Sub AsynchronousProcessSchedule(ByVal state As Object)

        Dim scheduleName As String = Convert.ToString(state)
        RaiseEvent ProcessSchedule(scheduleName, m_schedules(scheduleName))

    End Sub

    Private Sub StartTimer()

        If Not m_timer.Enabled Then
            Do While True
                RaiseEvent Starting(Me, EventArgs.Empty)
                If System.DateTime.Now.Second = 0 Then
                    m_timer.Start()
                    RaiseEvent Started(Me, EventArgs.Empty)
                    CheckAllSchedules()
                    Exit Do
                End If
            Loop
        End If
        m_startTimerThread = Nothing

    End Sub

    Private Sub m_timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_timer.Elapsed

        CheckAllSchedules()

    End Sub

End Class
