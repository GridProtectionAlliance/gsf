' 08-01-06

Imports Tva.Configuration
Imports Tva.Configuration.Common

Public Class ScheduleManager

    Private m_autoSaveSchedules As Boolean
    Private m_schedules As Dictionary(Of String, Schedule)
    Private WithEvents m_timer As System.Timers.Timer
    Private Const ConfigElement As String = "ScheduleManager"

    Public Event ProcessSchedule(ByVal scheduleName As String, ByVal schedule As Schedule)

    Public Sub New()
        m_autoSaveSchedules = True
        m_schedules = New Dictionary(Of String, Schedule)()
        m_timer = New System.Timers.Timer(60000)
        LoadSchedules()
    End Sub

    Public Property AutoSaveSchedules() As Boolean
        Get
            Return m_autoSaveSchedules
        End Get
        Set(ByVal value As Boolean)
            m_autoSaveSchedules = value
        End Set
    End Property

    Public ReadOnly Property Schedules() As Dictionary(Of String, Schedule)
        Get
            Return m_schedules
        End Get
    End Property

    Public Sub Start()

        Dim startTimerThread As New System.Threading.Thread(AddressOf StartTimer)
        startTimerThread.Start()

    End Sub

    Public Sub [Stop]()

        If m_timer.Enabled Then
            m_timer.Stop()
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

        AddSchedule(scheduleName, New Schedule(minutes, hours, days, months, daysOfWeek))

    End Sub

    Public Sub AddSchedule(ByVal scheduleName As String, ByVal schedule As Schedule)

        m_schedules.Add(scheduleName, schedule)
        If m_autoSaveSchedules Then
            DefaultConfigFile.CategorizedSettings(ConfigElement).Add(scheduleName, schedule.ToString())
            SaveSettings()
        End If

    End Sub

    Public Sub RemoveSchedule(ByVal scheduleName As String)

        m_schedules.Remove(scheduleName)
        If m_autoSaveSchedules Then
            DefaultConfigFile.CategorizedSettings(ConfigElement).Remove(scheduleName)
            SaveSettings()
        End If

    End Sub

    Public Sub LoadSchedules()

        For Each schedule As CategorizedSettingsElement In DefaultConfigFile.CategorizedSettings(ConfigElement)
            m_schedules.Add(schedule.Name, New Schedule(schedule.Value))
        Next

    End Sub

    Public Sub SaveSchedules()

        DefaultConfigFile.CategorizedSettings(ConfigElement).Clear()
        For Each scheduleName As String In m_schedules.Keys
            DefaultConfigFile.CategorizedSettings(ConfigElement).Add(scheduleName, m_schedules(scheduleName).ToString())
        Next
        SaveSettings()

    End Sub

    Private Sub StartTimer()

        If Not m_timer.Enabled Then
            Do While True
                If System.DateTime.Now.Second = 0 Then
                    m_timer.Start()
                    Exit Do
                End If
            Loop
        End If

    End Sub

    Private Sub m_timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_timer.Elapsed

        For Each scheduleName As String In m_schedules.Keys
            If m_schedules(scheduleName).IsDue() Then
                RaiseEvent ProcessSchedule(scheduleName, m_schedules(scheduleName))
            End If
        Next

    End Sub

End Class
