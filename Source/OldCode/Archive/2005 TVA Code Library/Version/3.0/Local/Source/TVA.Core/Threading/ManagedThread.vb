Imports System.Threading

Namespace Threading

    Public NotInheritable Class WorkItem

        Private m_type As WorkItemType
        Private m_startTicks As Long
        Private m_stopTicks As Long
        Private m_ctxCallback As ContextCallback
        Private m_tsCallback As ThreadStart
        Private m_ptsCallback As ParameterizedThreadStart
        Private m_ctx As ExecutionContext
        Private m_state As Object
        Private m_tag As Object

        Private Sub New()

            m_startTicks = System.DateTime.UtcNow.Ticks

        End Sub

        Friend Sub New(ByVal type As WorkItemType, ByVal callback As ContextCallback, ByVal state As Object, ByVal ctx As ExecutionContext)

            MyClass.New()
            m_type = type
            m_ctxCallback = callback
            m_state = state
            m_ctx = ctx

        End Sub

        Friend Sub New(ByVal type As WorkItemType, ByVal callback As ThreadStart, ByVal state As Object, ByVal ctx As ExecutionContext)

            MyClass.New()
            m_type = type
            m_tsCallback = callback
            m_state = state
            m_ctx = ctx

        End Sub

        Friend Sub New(ByVal type As WorkItemType, ByVal callback As ParameterizedThreadStart, ByVal state As Object, ByVal ctx As ExecutionContext)

            MyClass.New()
            m_type = type
            m_ptsCallback = callback
            m_state = state
            m_ctx = ctx

        End Sub

        Friend ReadOnly Property ContextCallBack() As ContextCallback
            Get
                Return m_ctxCallback
            End Get
        End Property

        Friend ReadOnly Property ThreadStart() As ThreadStart
            Get
                Return m_tsCallback
            End Get
        End Property

        Friend ReadOnly Property ParameterizedThreadStart() As ParameterizedThreadStart
            Get
                Return m_ptsCallback
            End Get
        End Property

        Friend ReadOnly Property ExecutionContext() As ExecutionContext
            Get
                Return m_ctx
            End Get
        End Property

        Public ReadOnly Property State() As Object
            Get
                Return m_state
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

        Public ReadOnly Property Type() As WorkItemType
            Get
                Return m_type
            End Get
        End Property

        Public ReadOnly Property StartTicks() As Long
            Get
                Return m_startTicks
            End Get
        End Property

        Public Property StopTicks() As Long
            Get
                Return m_stopTicks
            End Get
            Friend Set(ByVal value As Long)
                m_stopTicks = value
            End Set
        End Property

        Public Sub Abort()

            ManagedThreads.Cancel(Me, True)

        End Sub

        Public ReadOnly Property Status() As WorkItemStatus
            Get
                Return ManagedThreads.QueryStatus(Me)
            End Get
        End Property

    End Class

End Namespace
