'*******************************************************************************************************
'  Tva.Collections.ProcessQueue.vb - Multi-threaded Processing Queue Class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/04/2006 - Pinal C Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    Public Class ProcessQueue
        Implements ICollection

        Public Delegate Sub ProcessEventHandler(ByVal item As Object)
        Public Event ProcessException(ByVal ex As Exception)

        Private m_processMethod As ProcessEventHandler
        Private WithEvents m_processTimer As System.Timers.Timer
        Private m_threadCount As Int32
        Private m_maximumThreads As Int32
        Private m_timeoutDuration As Int32
        Private m_processQueue As ArrayList

        Public Sub New(ByVal processMethod As ProcessEventHandler)

            Me.New(processMethod, 100, 5, Timeout.Infinite)

        End Sub

        Public Sub New(ByVal processMethod As ProcessEventHandler, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer)

            m_processMethod = processMethod
            m_processTimer = New System.Timers.Timer()
            m_maximumThreads = maximumThreads
            m_timeoutDuration = processTimeout

            With m_processTimer
                .AutoReset = False
                .Interval = processInterval
                .Enabled = True
            End With

            m_processQueue = ArrayList.Synchronized(New ArrayList())

        End Sub

        Public Property ProcessMethod() As ProcessEventHandler
            Get
                Return m_processMethod
            End Get
            Set(ByVal value As ProcessEventHandler)
                m_processMethod = value
            End Set
        End Property

        Public Property ProcessInterval() As Integer
            Get
                Return m_processTimer.Interval
            End Get
            Set(ByVal value As Integer)
                m_processTimer.Interval = value
            End Set
        End Property

        Public Property MaximumThreads() As Integer
            Get
                Return m_maximumThreads
            End Get
            Set(ByVal value As Integer)
                m_maximumThreads = value
            End Set
        End Property

        Public Property ProcessTimeout() As Integer
            Get
                Return m_timeoutDuration
            End Get
            Set(ByVal value As Integer)
                m_timeoutDuration = value
            End Set
        End Property

        Public ReadOnly Property ThreadCount() As Integer
            Get
                Return m_threadCount
            End Get
        End Property

        Public Sub Add(ByVal item As Object)

            m_processQueue.Add(item)

        End Sub

        Public Sub Push(ByVal item As Object)

            m_processQueue.Insert(0, item)

        End Sub

        Public Sub Insert(ByVal index As Integer, ByVal item As Object)

            m_processQueue.Insert(index, item)

        End Sub

        Public ReadOnly Property SyncRoot() As Object Implements ICollection.SyncRoot
            Get
                Return m_processQueue.SyncRoot()
            End Get
        End Property

        Public Sub CopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo

            m_processQueue.CopyTo(array, index)

        End Sub

        Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
            Get
                Return m_processQueue.IsSynchronized()
            End Get
        End Property

        Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator

            Return m_processQueue.GetEnumerator()

        End Function

        Public Function Pop() As Object

            Pop = m_processQueue(0)
            m_processQueue.RemoveAt(0)

        End Function

        Public Function Poop() As Object

            Dim lastIndex As Integer = m_processQueue.Count() - 1
            Poop = m_processQueue(lastIndex)
            m_processQueue.RemoveAt(lastIndex)

        End Function

        Default Public Property Item(ByVal index As Integer) As Object
            Get
                Return m_processQueue(index)
            End Get
            Set(ByVal value As Object)
                m_processQueue(index) = value
            End Set
        End Property

        Public ReadOnly Property Count() As Integer Implements ICollection.Count
            Get
                Return m_processQueue.Count()
            End Get
        End Property

        Public Sub Clear()

            m_processQueue.Clear()

        End Sub

        Public Sub RemoveAt(ByVal index As Integer)

            m_processQueue.RemoveAt(index)

        End Sub

        Public Sub AddRange(ByVal collection As ICollection)

            m_processQueue.AddRange(collection)

        End Sub

        Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed
            Try
                ' Spawn a new process thread to process event to be processed if the number of current 
                ' process threads is less than the maximum allowable process threads.
                If Me.Count() > 0 AndAlso m_threadCount < m_maximumThreads Then
                    With New ProcessThread(m_processMethod, Me.Pop())
                        Try
                            m_threadCount += 1
                            .Join(m_timeoutDuration)
                        Catch ex As Exception
                            RaiseEvent ProcessException(ex)
                        Finally
                            m_threadCount -= 1
                        End Try
                    End With
                End If
            Catch ex As Exception
                RaiseEvent ProcessException(ex)
            Finally
                ' Keep process timer running...
                m_processTimer.Enabled = True
            End Try

        End Sub

        Private Class ProcessThread

            Private m_thread As Thread
            Private m_processMethod As ProcessEventHandler
            Private m_processItem As Object

            Public Sub New(ByVal processMethod As ProcessEventHandler, ByVal processItem As Object)

                m_processMethod = processMethod
                m_processItem = processItem
                m_thread = New Thread(AddressOf Process)
                m_thread.Start()

            End Sub

            Public Sub Join(ByVal timeout As Integer)

                m_thread.Join(timeout)

            End Sub

            Private Sub Process()

                m_processMethod.Invoke(m_processItem)

            End Sub

        End Class

    End Class

End Namespace