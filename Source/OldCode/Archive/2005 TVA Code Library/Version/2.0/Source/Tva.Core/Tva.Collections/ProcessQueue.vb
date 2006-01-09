'*******************************************************************************************************
'  Tva.Collections.AsynchronousProcessQueue.vb - Multi-threaded Processing Queue Class
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
    ''' <para>Processes a series of items on independent threads</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class AsynchronousProcessQueue(Of T)

        Inherits ProcessQueueBase(Of T)

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

            Public Sub WaitUntil(ByVal timeout As Integer)

                If Not m_thread.Join(timeout) Then m_thread.Abort()

            End Sub

            Private Sub Process()

                m_processItemFunction(m_processItem)

            End Sub

        End Class

        Public Const DefaultProcessInterval As Integer = 100
        Public Const DefaultMaximumThreads As Integer = 5
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite

        Private WithEvents m_processTimer As System.Timers.Timer
        Private m_maximumThreads As Integer
        Private m_processTimeout As Integer

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

            MyBase.New(processItemFunction)
            m_maximumThreads = maximumThreads
            m_processTimeout = processTimeout
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

            m_processTimer.Enabled = True

        End Sub

        Public Overrides Sub [Stop]()

            m_processTimer.Enabled = False

        End Sub

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

        Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed

            Dim nextItem As T

            Try
                ' Handle all queue operations for getting next item in a single synchronous operation.
                ' We keep work to be done here down to a mimimum amount of time
                SyncLock m_processQueue
                    ' We get next item to be processed if the number of current process threads is less
                    ' than the maximum allowable number of process threads.
                    If m_processQueue.Count > 0 AndAlso m_threadCount < m_maximumThreads Then
                        ' Retrieve first item to be processed
                        nextItem = m_processQueue(0)

                        ' We increment the thread counter using a thread safe operation
                        Interlocked.Increment(m_threadCount)

                        ' Remove the item about to be processed from the queue
                        m_processQueue.RemoveAt(0)
                    End If
                End SyncLock

                If nextItem IsNot Nothing Then
                    If m_processTimeout = Timeout.Infinite Then
                        ' If we have an item to process and the process queue wasn't setup with a process timeout, we just use
                        ' the current timer thread to process the next item taking as long as we need for it to complete.  The
                        ' next item in the queue will begin processing even if this item isn't completed - but no more than
                        ' the specified number of maximum threads will be spawned at once.
                        m_processItemFunction(nextItem)
                    Else
                        ' If we have an item to process and specified a process timeout we create a new thread to handle the
                        ' processing.  The timer event is already running on a new thread so the only reason we create a another
                        ' thread is so that we can implement the process timeout if the process takes to long to run.  We do this
                        ' by joining the timer thread (which will block it) until the specified interval has passed or the process
                        ' thread completes, whichever comes first.  This is a safe operation since the timer event was already
                        ' an independent thread and won't block any other processing, including the next timer event.
                        With New ProcessThread(m_processItemFunction, nextItem)
                            .WaitUntil(m_processTimeout)
                        End With
                    End If
                End If
            Catch ex As Exception
                ' No exceptions are generally expected here, but if any occur we will report them
                RaiseProcessException(ex)
            Finally
                ' Decrement thread count if item was processed
                If nextItem IsNot Nothing Then Interlocked.Decrement(m_threadCount)
            End Try

        End Sub

    End Class

End Namespace