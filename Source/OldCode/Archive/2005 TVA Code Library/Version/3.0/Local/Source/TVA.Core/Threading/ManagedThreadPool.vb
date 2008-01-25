Imports System.Threading
Imports System.Collections.Generic

Namespace Threading

    Public Class ManagedThreadPool

        Private Shared m_queuedCallBacks As New LinkedList(Of WorkItem)
        Private Shared m_activeThreads As New Dictionary(Of WorkItem, Thread)

        Public Shared Function QueueUserWorkItem(ByVal callback As ContextCallback) As WorkItem

            Return QueueUserWorkItem(callback, Nothing, False)

        End Function

        Public Shared Function QueueUserWorkItem(ByVal callback As ContextCallback, ByVal useCurrentExecutionContext As Boolean) As WorkItem

            Return QueueUserWorkItem(callback, Nothing, useCurrentExecutionContext)

        End Function

        Public Shared Function QueueUserWorkItem(ByVal callback As ContextCallback, ByVal state As Object) As WorkItem

            Return QueueUserWorkItem(callback, state, False)

        End Function

        Public Shared Function QueueUserWorkItem(ByVal callback As ContextCallback, ByVal state As Object, ByVal useCurrentExecutionContext As Boolean) As WorkItem

            If callback Is Nothing Then Throw New ArgumentNullException("callback")

            Dim item As WorkItem

            If useCurrentExecutionContext Then
                item = New WorkItem(callback, state, ExecutionContext.Capture)
            Else
                item = New WorkItem(callback, state, Nothing)
            End If

            SyncLock m_queuedCallBacks
                m_queuedCallBacks.AddLast(item)
            End SyncLock

            ThreadPool.QueueUserWorkItem(AddressOf HandleItem)

            Return item

        End Function

        Public Shared ReadOnly Property QueuedWorkItems() As WorkItem()
            Get
                With New List(Of WorkItem)
                    SyncLock m_queuedCallBacks
                        .AddRange(m_queuedCallBacks)
                        .AddRange(m_activeThreads.Keys)
                    End SyncLock

                    Return .ToArray()
                End With
            End Get
        End Property

        Private Shared Sub HandleItem(ByVal ignored As Object)

            Dim item As WorkItem

            Try
                SyncLock m_queuedCallBacks
                    If m_queuedCallBacks.Count > 0 Then
                        item = m_queuedCallBacks.First.Value
                        m_queuedCallBacks.RemoveFirst()
                    End If

                    If item Is Nothing Then Return

                    m_activeThreads.Add(item, Thread.CurrentThread)
                End SyncLock

                If item.Context Is Nothing Then
                    item.Callback.Invoke(item.State)
                Else
                    ExecutionContext.Run(item.Context, item.Callback, item.State)
                End If
            Finally
                SyncLock m_queuedCallBacks
                    If item IsNot Nothing Then m_activeThreads.Remove(item)
                End SyncLock
            End Try

        End Sub

        Public Shared Function QueryStatus(ByVal item As WorkItem) As WorkItemStatus

            SyncLock m_queuedCallBacks
                Dim node As LinkedListNode(Of WorkItem) = m_queuedCallBacks.Find(item)

                If node IsNot Nothing Then
                    Return WorkItemStatus.Queued
                ElseIf m_activeThreads.ContainsKey(item) Then
                    Return WorkItemStatus.Executing
                Else
                    Return WorkItemStatus.Completed
                End If
            End SyncLock

        End Function

        Public Shared Function Cancel(ByVal item As WorkItem, ByVal allowAbort As Boolean) As WorkItemStatus

            If item Is Nothing Then Throw New ArgumentNullException("item")

            SyncLock m_queuedCallBacks
                Dim node As LinkedListNode(Of WorkItem) = m_queuedCallBacks.Find(item)

                If node Is Nothing Then
                    Dim activeThread As Thread

                    If m_activeThreads.TryGetValue(item, activeThread) Then
                        If allowAbort Then
                            activeThread.Abort()
                            m_activeThreads.Remove(item)
                            Return WorkItemStatus.Aborted
                        Else
                            Return WorkItemStatus.Executing
                        End If
                    Else
                        Return WorkItemStatus.Completed
                    End If
                Else
                    m_queuedCallBacks.Remove(node)
                    Return WorkItemStatus.Queued
                End If
            End SyncLock

        End Function

    End Class

End Namespace
