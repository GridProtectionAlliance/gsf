' 11/22/2006

Public Class ObjectStateChangedEventArgs(Of T)
    Inherits EventArgs

    Private m_objectName As String
    Private m_newState As T

    Public Sub New(ByVal objectName As String, ByVal newState As T)

        m_objectName = objectName
        m_newState = newState

    End Sub

    Public Property ObjectName() As String
        Get
            Return m_objectName
        End Get
        Set(ByVal value As String)
            m_objectName = value
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
