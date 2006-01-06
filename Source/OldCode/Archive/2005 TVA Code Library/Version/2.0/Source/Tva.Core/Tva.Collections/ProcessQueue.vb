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
'  01/05/2006 - James R Carroll
'       Made process queue a generic collection
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    ''' <summary>
    ''' <para>Processes a series of items on independent threads</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed</para>
    ''' </remarks>
    Public Class ProcessQueue(Of T)

        Implements IList(Of T)

        Private Class ProcessThread

            Private m_thread As Thread
            Private m_processItemFunction As ProcessItemFunctionSignature
            Private m_processItem As T

            Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processItem As T)

                m_processItemFunction = processItemFunction
                m_processItem = processItem
                m_thread = New Thread(AddressOf Process)
                m_thread.Start()

            End Sub

            Public Sub Join(ByVal timeout As Integer)

                m_thread.Join(timeout)

            End Sub

            Private Sub Process()

                m_processItemFunction(m_processItem)

            End Sub

        End Class

        Public Delegate Sub ProcessItemFunctionSignature(ByVal item As T)
        Public Event ProcessException(ByVal ex As Exception)

        Public Const DefaultProcessInterval As Integer = 100
        Public Const DefaultMaximumThreads As Integer = 5
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite

        Private m_processItemFunction As ProcessItemFunctionSignature
        Private WithEvents m_processTimer As System.Timers.Timer
        Private m_threadCount As Int32
        Private m_maximumThreads As Int32
        Private m_timeoutDuration As Int32
        Private m_processQueue As List(Of T)    ' TODO: Synchronize process queue throughout this class...

        ''' <summary>
        ''' Create a process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature)

            Me.New(processItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout)

        End Sub

        ''' <summary>
        ''' Create a process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer)

            Me.New(processItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout)

        End Sub

        ''' <summary>
        ''' Create a process queue using the specified settings
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer)

            m_processItemFunction = processItemFunction
            m_processTimer = New System.Timers.Timer
            m_maximumThreads = maximumThreads
            m_timeoutDuration = processTimeout
            m_processQueue = New List(Of T)

            With m_processTimer
                .AutoReset = False
                .Interval = processInterval
                .Enabled = True
            End With

        End Sub

        Public Property ProcessItemFunction() As ProcessItemFunctionSignature
            Get
                Return m_processItemFunction
            End Get
            Set(ByVal value As ProcessItemFunctionSignature)
                m_processItemFunction = value
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

        Public Sub Add(ByVal item As T) Implements System.Collections.Generic.IList(Of T).Add

            SyncLock m_processQueue
                m_processQueue.Add(item)
            End SyncLock

        End Sub

        Public Sub Push(ByVal item As T)

            SyncLock m_processQueue
                m_processQueue.Insert(0, item)
            End SyncLock

        End Sub

        Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert

            SyncLock m_processQueue
                m_processQueue.Insert(index, item)
            End SyncLock

        End Sub

        Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements System.Collections.Generic.IList(Of T).CopyTo

            SyncLock m_processQueue
                m_processQueue.CopyTo(array, arrayIndex)
            End SyncLock

        End Sub

        Private Function GetIEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator

            Return m_processQueue.GetEnumerator()

        End Function

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator

            Return m_processQueue.GetEnumerator()

        End Function

        Public Function Pop() As T

            SyncLock m_processQueue
                Dim poppedItem As T = m_processQueue(0)
                m_processQueue.RemoveAt(0)
                Return poppedItem
            End SyncLock

        End Function

        Public Function Poop() As T

            SyncLock m_processQueue
                Dim lastIndex As Integer = m_processQueue.Count() - 1
                Dim poopedItem As T = m_processQueue(lastIndex)
                m_processQueue.RemoveAt(lastIndex)
                Return poopedItem
            End SyncLock

        End Function

        Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
            Get
                SyncLock m_processQueue
                    Return m_processQueue(index)
                End SyncLock
            End Get
            Set(ByVal value As T)
                SyncLock m_processQueue
                    m_processQueue(index) = value
                End SyncLock
            End Set
        End Property

        Public Function IndexOf(ByVal item As T) As Integer Implements System.Collections.Generic.IList(Of T).IndexOf

            SyncLock m_processQueue
                Return m_processQueue.IndexOf(item)
            End SyncLock

        End Function

        Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.IList(Of T).Count
            Get
                SyncLock m_processQueue
                    Return m_processQueue.Count()
                End SyncLock
            End Get
        End Property

        Public Sub Clear() Implements System.Collections.Generic.IList(Of T).Clear

            SyncLock m_processQueue
                m_processQueue.Clear()
            End SyncLock

        End Sub

        Public Function Contains(ByVal item As T) As Boolean Implements System.Collections.Generic.IList(Of T).Contains

            SyncLock m_processQueue
                Return m_processQueue.Contains(item)
            End SyncLock

        End Function

        Public Function Remove(ByVal item As T) As Boolean Implements System.Collections.Generic.IList(Of T).Remove

            SyncLock m_processQueue
                m_processQueue.Remove(item)
            End SyncLock

        End Function

        Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt

            SyncLock m_processQueue
                m_processQueue.RemoveAt(index)
            End SyncLock

        End Sub

        Public ReadOnly Property SyncRoot() As Object
            Get
                Return m_processQueue
            End Get
        End Property

        Public Sub AddRange(ByVal collection As IEnumerable(Of T))

            SyncLock m_processQueue
                m_processQueue.AddRange(collection)
            End SyncLock

        End Sub

        Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.IList(Of T).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed
            Try
                ' Spawn a new process thread to process event to be processed if the number of current 
                ' process threads is less than the maximum allowable process threads.
                If Count > 0 AndAlso m_threadCount < m_maximumThreads Then
                    With New ProcessThread(m_processItemFunction, Pop())
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

    End Class

End Namespace