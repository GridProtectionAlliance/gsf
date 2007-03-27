Public Class ProgressEventArgs(Of T)
    Inherits EventArgs

    Private m_total As T
    Private m_completed As T

    Public Sub New(ByVal total As T, ByVal completed As T)

        MyBase.New()
        m_total = total
        m_completed = completed

    End Sub

    Public Property Total() As T
        Get
            Return m_total
        End Get
        Set(ByVal value As T)
            m_total = value
        End Set
    End Property

    Public Property Completed() As T
        Get
            Return m_completed
        End Get
        Set(ByVal value As T)
            m_completed = value
        End Set
    End Property

End Class
