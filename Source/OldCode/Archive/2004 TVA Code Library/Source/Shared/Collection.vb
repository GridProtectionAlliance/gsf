Imports System.Threading

Namespace [Shared]

    Public Class Collection

        Public Class ProcessQueue

            Implements ICollection

            Public Delegate Sub ProcessEventSignature(ByVal Item As Object)
            Public Event ProcessException(ByVal ex As Exception)

            Private m_ProcessEvent As ProcessEventSignature
            Private WithEvents m_ProcessTimer As System.Timers.Timer
            Private m_ThreadCount As Integer
            Private m_MaximumThreads As Integer
            Private m_ProcessTimeout As Integer
            Private m_Queue As ArrayList

            Public Sub New(ByVal ProcessEventMethod As ProcessEventSignature)

                m_ProcessEvent = ProcessEventMethod
                m_ProcessTimer = New System.Timers.Timer
                m_MaximumThreads = 5
                m_ProcessTimeout = Timeout.Infinite

                With m_ProcessTimer
                    .Interval = 100
                    .AutoReset = False
                    .Enabled = True
                End With

                m_Queue = ArrayList.Synchronized(New ArrayList)

            End Sub

            Public Sub New(ByVal ProcessEventMethod As ProcessEventSignature, ByVal ProcessInterval As Integer, ByVal MaximumThreads As Integer, ByVal ProcessTimeout As Integer)

                m_ProcessEvent = ProcessEventMethod
                m_ProcessTimer = New System.Timers.Timer
                m_MaximumThreads = MaximumThreads
                m_ProcessTimeout = ProcessTimeout

                With m_ProcessTimer
                    .AutoReset = False
                    .Interval = ProcessInterval
                    .Enabled = True
                End With

                m_Queue = ArrayList.Synchronized(New ArrayList)

            End Sub

            Public Property ProcessEventMethod() As ProcessEventSignature
                Get
                    Return m_ProcessEvent
                End Get
                Set(ByVal Value As ProcessEventSignature)
                    m_ProcessEvent = Value
                End Set
            End Property

            Public Property ProcessInterval() As Integer
                Get
                    Return m_ProcessTimer.Interval
                End Get
                Set(ByVal Value As Integer)
                    m_ProcessTimer.Interval = Value
                End Set
            End Property

            Public Property MaximumThreads() As Integer
                Get
                    Return m_MaximumThreads
                End Get
                Set(ByVal Value As Integer)
                    m_MaximumThreads = Value
                End Set
            End Property

            Public Property ProcessTimeout() As Integer
                Get
                    Return m_ProcessTimeout
                End Get
                Set(ByVal Value As Integer)
                    m_ProcessTimeout = Value
                End Set
            End Property

            Public ReadOnly Property ThreadCount() As Integer
                Get
                    Return m_ThreadCount
                End Get
            End Property

            Public Sub Add(ByVal Item As Object)

                m_Queue.Add(Item)

            End Sub

            Public Sub Push(ByVal Item As Object)

                m_Queue.Insert(0, Item)

            End Sub

            Public Sub Insert(ByVal Index As Integer, ByVal Item As Object)

                m_Queue.Insert(Index, Item)

            End Sub

            Public ReadOnly Property SyncRoot() As Object Implements ICollection.SyncRoot
                Get
                    Return m_Queue.SyncRoot()
                End Get
            End Property

            Public Sub CopyTo(ByVal Array As System.Array, ByVal Index As Integer) Implements System.Collections.ICollection.CopyTo

                m_Queue.CopyTo(Array, Index)

            End Sub

            Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
                Get
                    Return m_Queue.IsSynchronized()
                End Get
            End Property

            Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator

                Return m_Queue.GetEnumerator

            End Function

            Public Function Pop() As Object

                Pop = m_Queue(0)
                m_Queue.RemoveAt(0)

            End Function

            Public Function Poop() As Object

                Dim intLastIndex As Integer = m_Queue.Count - 1
                Poop = m_Queue(intLastIndex)
                m_Queue.RemoveAt(intLastIndex)

            End Function

            Default Public Property Item(ByVal Index As Integer) As Object
                Get
                    Return m_Queue(Index)
                End Get
                Set(ByVal Value As Object)
                    m_Queue(Index) = Value
                End Set
            End Property

            Public ReadOnly Property Count() As Integer Implements ICollection.Count
                Get
                    Return m_Queue.Count
                End Get
            End Property

            Public Sub Clear()

                m_Queue.Clear()

            End Sub

            Public Sub RemoveAt(ByVal Index As Integer)

                m_Queue.RemoveAt(Index)

            End Sub

            Public Sub AddRange(ByVal Collection As ICollection)

                m_Queue.AddRange(Collection)

            End Sub

            Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_ProcessTimer.Elapsed
                Try
                    ' Spawn a new process thread to process event to be processed if the number of current 
                    ' process threads is less than the maximum allowable process threads.
                    If Count > 0 AndAlso m_ThreadCount < m_MaximumThreads Then
                        With New ProcessThread(m_ProcessEvent, Pop())
                            Try
                                m_ThreadCount += 1
                                .Join(m_ProcessTimeout)
                            Catch ex As Exception
                                RaiseEvent ProcessException(ex)
                            Finally
                                m_ThreadCount -= 1
                            End Try
                        End With
                    End If
                Catch ex As Exception
                    RaiseEvent ProcessException(ex)
                Finally
                    ' Keep process timer running...
                    m_ProcessTimer.Enabled = True
                End Try

            End Sub

            Private Class ProcessThread

                Private m_Thread As Thread
                Private m_ProcessEvent As ProcessEventSignature
                Private m_ProcessItem As Object

                Public Sub New(ByVal ProcessEvent As ProcessEventSignature, ByVal ProcessItem As Object)

                    m_ProcessEvent = ProcessEvent
                    m_ProcessItem = ProcessItem
                    m_Thread = New Thread(AddressOf ThreadProc)
                    m_Thread.Start()

                End Sub

                Public Sub Join(ByVal Timeout As Integer)

                    m_Thread.Join(Timeout)

                End Sub

                Private Sub ThreadProc()

                    m_ProcessEvent(m_ProcessItem)

                End Sub

            End Class

        End Class

    End Class

End Namespace

