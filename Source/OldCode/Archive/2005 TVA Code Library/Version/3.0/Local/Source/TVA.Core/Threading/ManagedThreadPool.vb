Imports System.Threading

Namespace Threading

    Public Class ManagedThreadPool

        Public Shared Function QueueUserWorkItem(ByVal callback As ThreadStart) As ManagedThread

            If callback Is Nothing Then Throw New ArgumentNullException("callback")

            Dim item As New ManagedThread(ThreadType.QueuedThread, callback, Nothing, Nothing)

            ManagedThreads.Queue(item)

            ThreadPool.QueueUserWorkItem(AddressOf HandleItem)

            Return item

        End Function

        Public Shared Function QueueUserWorkItem(ByVal callback As ParameterizedThreadStart) As ManagedThread

            Return QueueUserWorkItem(callback, Nothing)

        End Function

        Public Shared Function QueueUserWorkItem(ByVal callback As ParameterizedThreadStart, ByVal state As Object) As ManagedThread

            If callback Is Nothing Then Throw New ArgumentNullException("callback")

            Dim item As New ManagedThread(ThreadType.QueuedThread, callback, state, Nothing)

            ManagedThreads.Queue(item)

            ThreadPool.QueueUserWorkItem(AddressOf HandleItem)

            Return item

        End Function

        Public Shared Function QueueUserWorkItem(ByVal callback As ContextCallback, ByVal ctx As ExecutionContext) As ManagedThread

            Return QueueUserWorkItem(callback, Nothing, ctx)

        End Function

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
