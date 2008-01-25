Imports System.Threading

Namespace Threading

    Public NotInheritable Class WorkItem

        Private m_startTicks As Long
        Private m_callback As ContextCallback
        Private m_state As Object
        Private m_ctx As ExecutionContext
        Private m_tag As Object

        Friend Sub New(ByVal wc As ContextCallback, ByVal state As Object, ByVal ctx As ExecutionContext)

            m_startTicks = System.DateTime.UtcNow.Ticks
            m_callback = wc
            m_state = state
            m_ctx = ctx

        End Sub

        Public ReadOnly Property Callback() As ContextCallback
            Get
                Return m_callback
            End Get
        End Property

        Public ReadOnly Property State() As Object
            Get
                Return m_state
            End Get
        End Property

        Public ReadOnly Property Context() As ExecutionContext
            Get
                Return m_ctx
            End Get
        End Property

        Public Property Tag() As Object
            Get
                Return m_tag
            End Get
            Set(ByVal value As Object)
                m_tag = value
            End Set
        End Property

        Public ReadOnly Property StartTicks() As Long
            Get
                Return m_startTicks
            End Get
        End Property

        Public Sub Abort()

            ManagedThreadPool.Cancel(Me, True)

        End Sub

        Public ReadOnly Property Status() As WorkItemStatus
            Get
                Return ManagedThreadPool.QueryStatus(Me)
            End Get
        End Property

    End Class

End Namespace
