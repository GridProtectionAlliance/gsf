Imports System.Threading

Namespace Threading

    ''' <summary>
    ''' Defines a managed thread pool
    ''' </summary>
    ''' <remarks>
    ''' This class works like the normal thread pool but provides the benefit of automatic tracking
    ''' of queued threads through the ManagedThreads collection, returns a reference to the
    ''' queued thread with the ability to dequeue and/or abort, total thread runtime and the
    ''' ability to run the queued thread in an alternate execution context
    ''' </remarks>
    Public Class ManagedThreadPool

        ''' <summary>
        ''' Queues a work item for processing on the managed thread pool
        ''' </summary>
        ''' <param name="callback">A WaitCallback representing the method to execute.</param>
        ''' <returns>Reference to queued thread</returns>
        ''' <remarks>
        ''' This differs from the normal thread pool QueueUserWorkItem function in that it does
        ''' not return a success value determing if item was queued, but rather a reference to
        ''' to the managed thread that was actually placed on the queue.
        ''' </remarks>
        Public Shared Function QueueUserWorkItem(ByVal callback As ThreadStart) As ManagedThread

            If callback Is Nothing Then Throw New ArgumentNullException("callback")

            Dim item As New ManagedThread(ThreadType.QueuedThread, callback, Nothing, Nothing)

            ManagedThreads.Queue(item)

            ThreadPool.QueueUserWorkItem(AddressOf HandleItem)

            Return item

        End Function

        ''' <summary>
        ''' Queues a work item for processing on the managed thread pool
        ''' </summary>
        ''' <param name="callback">A WaitCallback representing the method to execute.</param>
        ''' <returns>Reference to queued thread</returns>
        ''' <remarks>
        ''' This differs from the normal thread pool QueueUserWorkItem function in that it does
        ''' not return a success value determing if item was queued, but rather a reference to
        ''' to the managed thread that was actually placed on the queue.
        ''' </remarks>
        Public Shared Function QueueUserWorkItem(ByVal callback As ParameterizedThreadStart) As ManagedThread

            Return QueueUserWorkItem(callback, Nothing)

        End Function

        ''' <summary>
        ''' Queues a work item for processing on the managed thread pool
        ''' </summary>
        ''' <param name="callback">A WaitCallback representing the method to execute.</param>
        ''' <param name="state">An object containing data to be used by the method.</param>
        ''' <returns>Reference to queued thread</returns>
        ''' <remarks>
        ''' This differs from the normal thread pool QueueUserWorkItem function in that it does
        ''' not return a success value determing if item was queued, but rather a reference to
        ''' to the managed thread that was actually placed on the queue.
        ''' </remarks>
        Public Shared Function QueueUserWorkItem(ByVal callback As ParameterizedThreadStart, ByVal state As Object) As ManagedThread

            If callback Is Nothing Then Throw New ArgumentNullException("callback")

            Dim item As New ManagedThread(ThreadType.QueuedThread, callback, state, Nothing)

            ManagedThreads.Queue(item)

            ThreadPool.QueueUserWorkItem(AddressOf HandleItem)

            Return item

        End Function

        ''' <summary>
        ''' Queues a work item for processing on the managed thread pool
        ''' </summary>
        ''' <param name="callback">A WaitCallback representing the method to execute.</param>
        ''' <param name="ctx">Alternate execution context in which to run the thread.</param>
        ''' <returns>Reference to queued thread</returns>
        ''' <remarks>
        ''' This differs from the normal thread pool QueueUserWorkItem function in that it does
        ''' not return a success value determing if item was queued, but rather a reference to
        ''' to the managed thread that was actually placed on the queue.
        ''' </remarks>
        Public Shared Function QueueUserWorkItem(ByVal callback As ContextCallback, ByVal ctx As ExecutionContext) As ManagedThread

            Return QueueUserWorkItem(callback, Nothing, ctx)

        End Function

        ''' <summary>
        ''' Queues a work item for processing on the managed thread pool
        ''' </summary>
        ''' <param name="callback">A WaitCallback representing the method to execute.</param>
        ''' <param name="state">An object containing data to be used by the method.</param>
        ''' <param name="ctx">Alternate execution context in which to run the thread.</param>
        ''' <returns>Reference to queued thread</returns>
        ''' <remarks>
        ''' This differs from the normal thread pool QueueUserWorkItem function in that it does
        ''' not return a success value determing if item was queued, but rather a reference to
        ''' to the managed thread that was actually placed on the queue.
        ''' </remarks>
        Public Shared Function QueueUserWorkItem(ByVal callback As ContextCallback, ByVal state As Object, ByVal ctx As ExecutionContext) As ManagedThread

            If callback Is Nothing Then Throw New ArgumentNullException("callback")

            Dim item As New ManagedThread(ThreadType.QueuedThread, callback, state, ctx)

            ManagedThreads.Queue(item)

            ThreadPool.QueueUserWorkItem(AddressOf HandleItem)

            Return item

        End Function

        Private Shared Sub HandleItem(ByVal state As Object)

            ' Get next queued item
            Dim item As ManagedThread = ManagedThreads.Pop()

            ' Execute callback...
            If item IsNot Nothing Then item.HandleItem()

        End Sub

    End Class

End Namespace
