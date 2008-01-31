Imports System.Threading
Imports TVA.Common
Imports TVA.DateTime.Common

Namespace Threading

    Public NotInheritable Class ManagedThread

#Region " Member Declaration "

        Private m_thread As Thread
        Private m_type As ThreadType
        Private m_status As ThreadStatus
        Private m_startTime As Long
        Private m_stopTime As Long
        Private m_ctxCallback As ContextCallback
        Private m_tsCallback As ThreadStart
        Private m_ptsCallback As ParameterizedThreadStart
        Private m_ctx As ExecutionContext
        Private m_state As Object
        Private m_tag As Object

#End Region

#Region " Constructors "

        Public Sub New(ByVal callback As ThreadStart)

            MyClass.New(ThreadType.StandardThread, callback, Nothing, Nothing)
            m_thread = New Thread(AddressOf HandleItem)

        End Sub

        Public Sub New(ByVal callback As ParameterizedThreadStart)

            MyClass.New(ThreadType.StandardThread, callback, Nothing, Nothing)
            m_thread = New Thread(AddressOf HandleItem)

        End Sub

        Public Sub New(ByVal callback As ContextCallback, ByVal ctx As ExecutionContext)

            MyClass.New(ThreadType.StandardThread, callback, Nothing, ctx)
            m_thread = New Thread(AddressOf HandleItem)

        End Sub

        Friend Sub New(ByVal type As ThreadType, ByVal callback As ThreadStart, ByVal state As Object, ByVal ctx As ExecutionContext)

            m_type = type
            m_status = IIf(type = ThreadType.QueuedThread, ThreadStatus.Queued, ThreadStatus.Unstarted)
            m_tsCallback = callback
            m_state = state
            m_ctx = ctx

        End Sub

        Friend Sub New(ByVal type As ThreadType, ByVal callback As ParameterizedThreadStart, ByVal state As Object, ByVal ctx As ExecutionContext)

            m_type = type
            m_status = IIf(type = ThreadType.QueuedThread, ThreadStatus.Queued, ThreadStatus.Unstarted)
            m_ptsCallback = callback
            m_state = state
            m_ctx = ctx

        End Sub

        Friend Sub New(ByVal type As ThreadType, ByVal callback As ContextCallback, ByVal state As Object, ByVal ctx As ExecutionContext)

            m_type = type
            m_status = IIf(type = ThreadType.QueuedThread, ThreadStatus.Queued, ThreadStatus.Unstarted)
            m_ctxCallback = callback
            m_state = state
            m_ctx = ctx

        End Sub

#End Region

#Region " Code Scope: Public "

        Public Property State() As Object
            Get
                Return m_state
            End Get
            Set(ByVal value As Object)
                m_state = value
            End Set
        End Property

        Public Property Tag() As Object
            Get
                Return m_tag
            End Get
            Set(ByVal value As Object)
                m_tag = value
            End Set
        End Property

        Public ReadOnly Property Type() As ThreadType
            Get
                Return m_type
            End Get
        End Property

        Public Property Status() As ThreadStatus
            Get
                Return m_status
            End Get
            Friend Set(ByVal value As ThreadStatus)
                m_status = value
            End Set
        End Property

        Public ReadOnly Property StartTime() As Long
            Get
                Return m_startTime
            End Get
        End Property

        Public ReadOnly Property StopTime() As Long
            Get
                Return m_stopTime
            End Get
        End Property

        ''' <summary>
        ''' Gets the total amount of time, in seconds, that the managed thread has been active.
        ''' </summary>
        Public ReadOnly Property RunTime() As Double
            Get
                Dim processingTime As Long

                If m_startTime > 0 Then
                    If m_stopTime > 0 Then
                        processingTime = m_stopTime - m_startTime
                    Else
                        processingTime = Date.UtcNow.Ticks - m_startTime
                    End If
                End If

                If processingTime < 0 Then processingTime = 0

                Return TicksToSeconds(processingTime)
            End Get
        End Property

        Public Sub Abort()

            ManagedThreads.Cancel(Me, True)

        End Sub

        Public Sub Start()

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot manually start a thread that was queued into thread pool.")

            ManagedThreads.Add(Me)

            m_thread.Start()

        End Sub

        Public Sub Start(ByVal parameter As Object)

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot manually start a thread that was queued into thread pool.")

            m_state = parameter

            ManagedThreads.Add(Me)

            m_thread.Start()

        End Sub

        Public Sub Join()

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot join a thread that was queued into thread pool.")
            If Not (m_status = ThreadStatus.Started OrElse m_status = ThreadStatus.Executing) Then Throw New InvalidOperationException("Cannont join a thread that has not been started.")

            m_thread.Join()

        End Sub

        Public Function Join(ByVal millisecondsTimeout As Integer) As Boolean

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot join a thread that was queued into thread pool.")
            If Not (m_status = ThreadStatus.Started OrElse m_status = ThreadStatus.Executing) Then Throw New InvalidOperationException("Cannont join a thread that has not been started.")

            Return m_thread.Join(millisecondsTimeout)

        End Function

        Public Function Join(ByVal timeout As TimeSpan) As Boolean

            Return Join(CInt(timeout.TotalMilliseconds))

        End Function

#End Region

#Region " Code Scope: Friend "

        Friend Property Thread() As Thread
            Get
                Return m_thread
            End Get
            Set(ByVal value As Thread)
                m_thread = value
            End Set
        End Property

        Friend Sub HandleItem()

            ' Set start state
            m_startTime = System.DateTime.UtcNow.Ticks
            m_status = ThreadStatus.Executing

            Try
                ' Invoke the user's call back function
                If m_ctx Is Nothing Then
                    If m_tsCallback IsNot Nothing Then
                        m_tsCallback.Invoke()
                    ElseIf m_ptsCallback IsNot Nothing Then
                        m_ptsCallback.Invoke(m_state)
                    Else
                        m_ctxCallback.Invoke(m_state)
                    End If
                Else
                    ExecutionContext.Run(m_ctx, m_ctxCallback, m_state)
                End If
            Finally
                ' Set finish state
                If m_status = ThreadStatus.Executing Then m_status = ThreadStatus.Completed
                m_stopTime = System.DateTime.UtcNow.Ticks

                ManagedThreads.Remove(Me)
            End Try

        End Sub

#End Region

    End Class

End Namespace
