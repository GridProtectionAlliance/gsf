Imports System.Threading
Imports System.Text
Imports TVA.DateTime.Common

Namespace Threading

    Public Class ManagedThreads

        Private Shared m_queuedThreads As LinkedList(Of ManagedThread)
        Private Shared m_activeThreads As LinkedList(Of ManagedThread)

        Shared Sub New()

            m_queuedThreads = New LinkedList(Of ManagedThread)
            m_activeThreads = New LinkedList(Of ManagedThread)

        End Sub

        Friend Shared Sub Add(ByVal item As ManagedThread)

            ' Standard threads are simply added to the active thread list when started
            SyncLock m_queuedThreads
                item.Status = ThreadStatus.Started
                m_activeThreads.AddLast(item)
            End SyncLock

        End Sub

        Friend Shared Sub Remove(ByVal item As ManagedThread)

            SyncLock m_queuedThreads
                m_activeThreads.Remove(item)
            End SyncLock

        End Sub

        Friend Shared Sub Queue(ByVal item As ManagedThread)

            SyncLock m_queuedThreads
                m_queuedThreads.AddLast(item)
            End SyncLock

        End Sub

        Friend Shared Function Pop() As ManagedThread

            Dim item As ManagedThread

            ' Transfer next queued thread to the active thread list
            SyncLock m_queuedThreads
                If m_queuedThreads.Count > 0 Then
                    item = m_queuedThreads.First.Value
                    m_queuedThreads.RemoveFirst()
                End If

                If item IsNot Nothing Then
                    ' Capture current thread (this is owned by .NET ThreadPool)
                    item.Thread = Thread.CurrentThread
                    item.Status = ThreadStatus.Started
                    m_activeThreads.AddLast(item)
                End If
            End SyncLock

            Return item

        End Function

        Public Shared ReadOnly Property ActiveThreadStatus() As String
            Get
                With New StringBuilder
                    Dim items As ManagedThread() = QueuedThreads
                    Dim index As Integer

                    ' Managed Thread Count: 1
                    '
                    ' Thread 1 - Completed in 25 seconds
                    '      Type: Standard Thread
                    '      Name: TVASPDC.Service.CalculatedMeasurementInitialization.Initialize()

                    .AppendFormat("Managed Thread Count: {0}{1}", items.Length, Environment.NewLine)
                    .AppendLine()

                    For Each item As ManagedThread In items
                        index += 1

                        .AppendFormat("Thread {0} - {1}{2}", _
                                    index, _
                                    ThreadStatusText(item), _
                                    Environment.NewLine)

                        .AppendFormat("     Type: {0}{1}", _
                                    [Enum].GetName(GetType(ThreadType), item.Type), _
                                    Environment.NewLine)

                        .AppendFormat("     Name: {0}{1}", _
                                      item.Tag.ToString(), _
                                      Environment.NewLine)
                        .AppendLine()
                    Next

                    Return .ToString()
                End With
            End Get
        End Property

        Private Shared ReadOnly Property ThreadStatusText(ByVal item As ManagedThread) As String
            Get
                Dim runtime As String = SecondsToText(item.RunTime)

                Select Case item.Status
                    Case ThreadStatus.Unstarted
                        Return "Not Started"
                    Case ThreadStatus.Queued
                        Return "Queued"
                    Case ThreadStatus.Executing
                        Return "Executing for " & runtime
                    Case ThreadStatus.Completed
                        Return "Completed in " & runtime
                    Case ThreadStatus.Aborted
                        Return "Aborted, ran for " & runtime
                    Case Else
                        Return "Status Unknown"
                End Select
            End Get
        End Property

        Public Shared ReadOnly Property QueuedThreads() As ManagedThread()
            Get
                With New List(Of ManagedThread)
                    SyncLock m_queuedThreads
                        .AddRange(m_queuedThreads)
                        .AddRange(m_activeThreads)
                    End SyncLock

                    Return .ToArray()
                End With
            End Get
        End Property

        Public Shared Sub Cancel(ByVal item As ManagedThread, ByVal allowAbort As Boolean)

            If item Is Nothing Then Throw New ArgumentNullException("item")

            Dim node As LinkedListNode(Of ManagedThread)

            SyncLock m_queuedThreads
                ' Change thread status to aborted
                item.Status = ThreadStatus.Aborted

                ' See if item is still queued for execution in thread pool
                node = m_queuedThreads.Find(item)

                ' Handle abort or dequeue
                If node Is Nothing Then
                    If allowAbort AndAlso item.IsAlive Then
                        ' Started items may be aborted, even if running in thread pool
                        item.Thread.Abort()
                    End If
                Else
                    ' Remove item from queue if queued thread has yet to start
                    m_queuedThreads.Remove(node)
                End If
            End SyncLock
        End Sub

    End Class

End Namespace
