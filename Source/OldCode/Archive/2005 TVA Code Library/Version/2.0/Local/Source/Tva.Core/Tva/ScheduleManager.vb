' 08-01-06

Imports Tva.Collections

Public Class ScheduleManager

    Private m_schedules As Dictionary(Of String, Schedule)
    Private WithEvents m_timer As System.Timers.Timer

    'Public Event RunTask(ByVal taskName As String, ByVal taskSchedule As Schedule)

    Public Sub New()
        m_schedules = New Dictionary(Of String, Schedule)()
        m_timer = New System.Timers.Timer(60000)
    End Sub

    Public Property PersistSchedules() As Boolean
        Get

        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public ReadOnly Property Schedules() As Dictionary(Of String, Schedule)
        Get
            Return m_schedules
        End Get
    End Property

    Public Sub Start()

        Do While True
            ' We'll start the time at top of the minute.
            If System.DateTime.Now.Minute = 0 Then
                m_timer.Start()
                Exit Do
            End If
        Loop

    End Sub

    Public Sub [Stop]()

    End Sub

    Public Sub AddSchedule()

    End Sub

    Private Sub m_timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_timer.Elapsed

        For Each scheduleName As String In m_schedules.Keys
            If m_schedules(scheduleName).IsDue() Then
                'RaiseEvent
            End If
        Next

    End Sub

End Class
