Public Class ScheduleEventArgs
    Inherits EventArgs

    Private m_schedule As Schedule

    Public Sub New(ByVal schedule As Schedule)

        MyBase.New()
        m_schedule = schedule

    End Sub

    Public Property Schedule() As Schedule
        Get
            Return m_schedule
        End Get
        Set(ByVal value As Schedule)
            m_schedule = value
        End Set
    End Property

End Class
