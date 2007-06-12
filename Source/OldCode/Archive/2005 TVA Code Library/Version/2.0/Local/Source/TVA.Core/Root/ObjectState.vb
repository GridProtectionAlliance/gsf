' 05/23/2007

<Serializable()> _
Public Class ObjectState(Of TState)

#Region " Member Declaration "

    Private m_objectName As String
    Private m_currentState As TState
    Private m_previousState As TState

#End Region

#Region " Code Scope: Public "

    Public Sub New(ByVal objectName As String)

        MyClass.New(objectName, Nothing)

    End Sub

    Public Sub New(ByVal objectName As String, ByVal currentState As TState)

        MyClass.New(objectName, Nothing, currentState)

    End Sub

    Public Sub New(ByVal objectName As String, ByVal previousState As TState, ByVal currentState As TState)

        MyBase.New()
        m_objectName = objectName
        m_currentState = currentState
        m_previousState = previousState

    End Sub

    Public Property ObjectName() As String
        Get
            Return m_objectName
        End Get
        Set(ByVal value As String)
            m_objectName = value
        End Set
    End Property

    Public Property CurrentState() As TState
        Get
            Return m_currentState
        End Get
        Set(ByVal value As TState)
            m_currentState = value
        End Set
    End Property

    Public Property PreviousState() As TState
        Get
            Return m_previousState
        End Get
        Set(ByVal value As TState)
            m_previousState = value
        End Set
    End Property

#End Region

End Class
