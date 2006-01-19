'*******************************************************************************************************
'  Tva.Collections.ProcessList.vb - Multi-threaded Intervaled Processing List Class
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
'  01/07/2006 - James R Carroll
'       Reworked threading architecture
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    ''' <summary>
    ''' <para>This class will process a list of items on independent threads</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class ProcessList(Of T)

        Inherits ProcessListBase(Of T)

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

            Public Function WaitUntil(ByVal timeout As Integer) As Boolean

                Dim threadComplete As Boolean = m_thread.Join(timeout)

                If Not threadComplete Then m_thread.Abort()

                Return threadComplete

            End Function

            Private Sub Process()

                m_processItemFunction(m_processItem)

            End Sub

        End Class

        ''' <summary>
        ''' This event will be raised if an item's processing time exceeds the specified process timeout
        ''' </summary>
        ''' <param name="timedOutItem">Reference to item that took too long to process</param>
        Public Event ProcessItemTimeout(ByVal timedOutItem As T)

        Public Const DefaultProcessInterval As Integer = 100
        Public Const DefaultMaximumThreads As Integer = 5
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite
        Public Const DefaultRequeueOnTimeout As Boolean = False

        Private WithEvents m_processTimer As System.Timers.Timer
        Private m_maximumThreads As Integer
        Private m_processTimeout As Integer
        Private m_requeueOnTimeout As Boolean

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousProcessQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessList(Of T)

            Return CreateAsynchronousProcessQueue(processItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousProcessQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessList(Of T)

            Return CreateAsynchronousProcessQueue(processItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousProcessQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessList(Of T)

            Return New ProcessList(Of T)(processItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateSynchronousProcessQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessList(Of T)

            Return CreateSynchronousProcessQueue(processItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousProcessQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessList(Of T)

            Return New ProcessList(Of T)(processItemFunction, processInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a process queue using the specified settings
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            Me.New(processItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, New List(Of T))

        End Sub

        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal processList As IList(Of T))

            MyBase.New(processItemFunction, processList)

            m_maximumThreads = maximumThreads
            m_processTimeout = processTimeout
            m_requeueOnTimeout = requeueOnTimeout
            m_processTimer = New System.Timers.Timer

            With m_processTimer
                .Interval = processInterval
                .AutoReset = True
                .Enabled = False
            End With

        End Sub

        Public Property ProcessInterval() As Integer
            Get
                Return m_processTimer.Interval
            End Get
            Set(ByVal value As Integer)
                m_processTimer.Interval = value
            End Set
        End Property

        Public Overrides Sub Start()

            MyBase.Start()
            m_processTimer.Enabled = True

        End Sub

        Public Overrides Sub [Stop]()

            MyBase.Stop()
            m_processTimer.Enabled = False

        End Sub

        ''' <summary>
        ''' Defines the maximum number of threads to process simultaneously
        ''' </summary>
        ''' <value>Maximum number of processing threads</value>
        ''' <returns>Maximum number of processing threads</returns>
        ''' <remarks>If you set maximum threads to one, item processing will be synchronous</remarks>
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
                Return m_processTimeout
            End Get
            Set(ByVal value As Integer)
                m_processTimeout = value
            End Set
        End Property

        Public Property RequeueOnTimeout() As Boolean
            Get
                Return m_requeueOnTimeout
            End Get
            Set(ByVal value As Boolean)
                m_requeueOnTimeout = value
            End Set
        End Property

        Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed

            Dim nextItem As T

            Try
                ' Handle all queue operations for getting next item in a single synchronous operation.
                ' We keep work to be done here down to a mimimum amount of time
                SyncLock SyncRoot
                    With InternalList
                        ' We get next item to be processed if the number of current process threads is less
                        ' than the maximum allowable number of process threads.
                        If .Count > 0 AndAlso ThreadCount < m_maximumThreads Then
                            ' Retrieve first item to be processed
                            nextItem = .Item(0)

                            ' We increment the thread counter using a thread safe operation
                            IncrementThreadCount()

                            ' Remove the item about to be processed from the queue
                            .RemoveAt(0)
                        End If
                    End With
                End SyncLock

                If nextItem IsNot Nothing Then
                    If m_processTimeout = Timeout.Infinite Then
                        ' If we have an item to process and the process queue wasn't setup with a process timeout, we just use
                        ' the current timer thread to process the next item taking as long as we need for it to complete.  The
                        ' next item in the queue will begin processing even if this item isn't completed - but no more than
                        ' the specified number of maximum threads will be spawned at once.
                        ProcessItemFunction.Invoke(nextItem)
                    Else
                        ' If we have an item to process and specified a process timeout we create a new thread to handle the
                        ' processing.  The timer event is already running on a new thread so the only reason we create a another
                        ' thread is so that we can implement the process timeout if the process takes to long to run.  We do this
                        ' by joining the timer thread (which will block it) until the specified interval has passed or the process
                        ' thread completes, whichever comes first.  This is a safe operation since the timer event was already
                        ' an independent thread and won't block any other processing, including the next timer event.
                        With New ProcessThread(ProcessItemFunction, nextItem)
                            If Not .WaitUntil(m_processTimeout) Then
                                ' We notify user of process timeout in case they want to do anything special
                                RaiseEvent ProcessItemTimeout(nextItem)

                                ' We requeue item on processing timeout if requested
                                If m_requeueOnTimeout Then Insert(0, nextItem)
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                ' No exceptions are generally expected here, but if any occur we will report them
                RaiseProcessException(ex)
            Finally
                ' Decrement thread count if item was processed
                If nextItem IsNot Nothing Then DecrementThreadCount()
            End Try

        End Sub

    End Class

End Namespace