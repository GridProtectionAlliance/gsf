' 11/22/2006

Public Class StateChangedEventArgs(Of T)
    Inherits EventArgs

    Private m_sourceName As String
    Private m_newState As T

    Public Sub New(ByVal newState As T)

        m_newState = newState

    End Sub

    Public Property ObjectName() As String
        Get
            Return m_sourceName
        End Get
        Set(ByVal value As String)
            m_sourceName = value
        End Set
    End Property

    Public Property NewState() As T
        Get
            Return m_newState
        End Get
        Set(ByVal value As T)
            m_newState = value
        End Set
    End Property

End Class
