Imports System.Threading
Imports TVA.Common
Imports TVA.DateTime.Common

Namespace Threading

    ''' <summary>
    ''' Defines a managed thread
    ''' </summary>
    ''' <remarks>
    ''' This class works like any normal thread but provides the benefit of automatic tracking
    ''' through the ManagedThreads collection, total thread runtime and the ability to run
    ''' the thread in an alternate execution context
    ''' </remarks>
    Public NotInheritable Class ManagedThread

#Region " Member Declaration "

        Private m_thread As Thread
        Private m_type As ThreadType
        Private m_status As ThreadStatus
        Private m_name As String
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

        ''' <summary>
        ''' Initializes a new instance of the ManagedThread class.
        ''' </summary>
        Public Sub New(ByVal callback As ThreadStart)

            MyClass.New(ThreadType.StandardThread, callback, Nothing, Nothing)
            m_thread = New Thread(AddressOf HandleItem)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the ManagedThread class, specifying a delegate that allows an object to be passed to the thread when the thread is started.
        ''' </summary>
        Public Sub New(ByVal callback As ParameterizedThreadStart)

            MyClass.New(ThreadType.StandardThread, callback, Nothing, Nothing)
            m_thread = New Thread(AddressOf HandleItem)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the ManagedThread class, specifying a delegate that allows an object to be passed to the thread when the thread is started
        ''' and allowing the user to specify an alternate execution context for the thread.
        ''' </summary>
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

        ''' <summary>
        ''' An object containing data to be used by the thread's execution method.
        ''' </summary>
        Public Property State() As Object
            Get
                Return m_state
            End Get
            Set(ByVal value As Object)
                m_state = value
            End Set
        End Property

        ''' <summary>
        ''' An object that allows additional user defined information to be tracked along with this thread.
        ''' </summary>
        Public Property Tag() As Object
            Get
                Return m_tag
            End Get
            Set(ByVal value As Object)
                m_tag = value
            End Set
        End Property

        ''' <summary>
        ''' Returns the managed thread type (either StandardThread or QueuedThread)
        ''' </summary>
        Public ReadOnly Property Type() As ThreadType
            Get
                Return m_type
            End Get
        End Property

        ''' <summary>
        ''' Gets a value containing the curretn status of the current thread.
        ''' </summary>
        Public Property Status() As ThreadStatus
            Get
                Return m_status
            End Get
            Friend Set(ByVal value As ThreadStatus)
                m_status = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a value indicating the execution status of the current thread.
        ''' </summary>
        Public ReadOnly Property IsAlive() As Boolean
            Get
                Return (m_status = ThreadStatus.Started OrElse m_status = ThreadStatus.Executing)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the name of the thread.
        ''' </summary>
        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
                If m_type = ThreadType.StandardThread Then m_thread.Name = value
            End Set
        End Property

        ''' <summary>
        ''' Get the time, in ticks, that the thread started executing
        ''' </summary>
        Public ReadOnly Property StartTime() As Long
            Get
                Return m_startTime
            End Get
        End Property

        ''' <summary>
        ''' Get the time, in ticks, that the thread finished executing
        ''' </summary>
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

        ''' <summary>
        ''' Raises a ThreadAbortException in the thread on which it is invoked, to begin the process of terminating the thread. Calling this method usually terminates the thread.
        ''' </summary>
        Public Sub Abort()

            ManagedThreads.Cancel(Me, True, Nothing)

        End Sub

        ''' <summary>
        ''' Raises a ThreadAbortException in the thread on which it is invoked, to begin the process of terminating the thread. Calling this method usually terminates the thread.
        ''' </summary>
        ''' <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted.</param>
        Public Sub Abort(ByVal stateInfo As Object)

            ManagedThreads.Cancel(Me, True, stateInfo)

        End Sub

        ''' <summary>
        ''' Causes a thread to be scheduled for execution.
        ''' </summary>
        Public Sub Start()

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot manually start a thread that was queued into thread pool.")

            ManagedThreads.Add(Me)

            m_thread.Start()

        End Sub

        ''' <summary>
        ''' Causes a thread to be scheduled for execution.
        ''' </summary>
        ''' <param name="parameter">An object that contains data to be used by the method the thread executes.</param>
        Public Sub Start(ByVal parameter As Object)

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot manually start a thread that was queued into thread pool.")

            m_state = parameter

            ManagedThreads.Add(Me)

            m_thread.Start()

        End Sub

        ''' <summary>
        ''' Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.
        ''' </summary>
        ''' <remarks>
        ''' This is only available for standard threads - queued threads don't have an associated thread until they are executing.
        ''' </remarks>
        Public Sub Join()

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot join a thread that was queued into thread pool.")
            If Not IsAlive Then Throw New InvalidOperationException("Cannont join a thread that has not been started.")

            m_thread.Join()

        End Sub

        ''' <summary>
        ''' Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.
        ''' </summary>
        ''' <param name="millisecondsTimeout">The number of milliseconds to wait for the thread to terminate. </param>
        ''' <returns>true if the thread has terminated; false if the thread has not terminated after the amount of time specified by the millisecondsTimeout parameter has elapsed.</returns>
        ''' <remarks>
        ''' This is only available for standard threads - queued threads don't have an associated thread until they are executing.
        ''' </remarks>
        Public Function Join(ByVal millisecondsTimeout As Integer) As Boolean

            If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot join a thread that was queued into thread pool.")
            If Not IsAlive Then Throw New InvalidOperationException("Cannont join a thread that has not been started.")

            Return m_thread.Join(millisecondsTimeout)

        End Function

        ''' <summary>
        ''' Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.
        ''' </summary>
        ''' <param name="timeout">A TimeSpan set to the amount of time to wait for the thread to terminate. </param>
        ''' <returns>true if the thread terminated; false if the thread has not terminated after the amount of time specified by the timeout parameter has elapsed.</returns>
        ''' <remarks>
        ''' This is only available for standard threads - queued threads don't have an associated thread until they are executing.
        ''' </remarks>
        Public Function Join(ByVal timeout As TimeSpan) As Boolean

            Return Join(CInt(timeout.TotalMilliseconds))

        End Function

        ''' <summary>
        ''' Gets or sets a value indicating the scheduling priority of a thread.
        ''' </summary>
        ''' <returns>One of the ThreadPriority values. The default value is Normal.</returns>
        ''' <remarks>
        ''' Changing of this value is only available to standard threads - you can't change the priorty of queued threads since they are already
        ''' allocated and owned by the .NET thread pool.
        ''' </remarks>
        Public Property Priority() As ThreadPriority
            Get
                If m_type = ThreadType.QueuedThread Then
                    Return ThreadPriority.Normal
                Else
                    Return m_thread.Priority
                End If
            End Get
            Set(ByVal value As ThreadPriority)
                If m_type = ThreadType.QueuedThread Then Throw New InvalidOperationException("Cannot change priority of a thread that was queued into thread pool.")

                m_thread.Priority = value
            End Set
        End Property

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
                    ' If user specified an alternate execution context, we invoke
                    ' their delegate under that context
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
