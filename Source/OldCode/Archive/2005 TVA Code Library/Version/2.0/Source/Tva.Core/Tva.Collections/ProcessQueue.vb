'*******************************************************************************************************
'  Tva.Collections.ProcessQueue.vb - Multi-threaded Item Processing Queue
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
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
Imports System.Text
Imports Tva.DateTime.Common

Namespace Collections

    ' TODO: Add bulk processing functionality that would process all currently queued items (Array of T) at each pass
    ' once added - import LogFile from C37.118 Proxy Service and implement based on this class

    ''' <summary>
    ''' <para>This class will process a collection of items on independent threads</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Consumers are expected to create new instances of this class through the static construction functions (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class ProcessQueue(Of T)

        Implements IList(Of T), ICollection

#Region " Internal Thread Processing Class "

        ' This internal class is used to limit item processing time if requested
        Private Class ProcessThread

            Private m_parent As ProcessQueue(Of T)
            Private m_thread As Thread
            Private m_item As T

            Public Sub New(ByVal parent As ProcessQueue(Of T), ByVal item As T)

                m_parent = parent
                m_item = item
                m_thread = New Thread(AddressOf ProcessItem)
                m_thread.Start()

            End Sub

            ' Block calling thread until specified time has expired
            Public Function WaitUntil(ByVal timeout As Integer) As Boolean

                Dim threadComplete As Boolean = m_thread.Join(timeout)
                If Not threadComplete Then m_thread.Abort()
                Return threadComplete

            End Function

            Private Sub ProcessItem()

                With m_parent
                    Try
                        ' Invoke user function to process item
                        .ProcessItemFunction.Invoke(m_item)
                        .IncrementItemsProcessed()
                        .RaiseItemProcessed(m_item)
                    Catch ex As ThreadAbortException
                        ' We egress gracefully if the thread's being aborted
                        Exit Sub
                    Catch ex As Exception
                        ' Processing won't stop for any errors thrown by the user function, but we will report them...
                        .RaiseProcessException(ex)
                    End Try
                End With

            End Sub

        End Class

#End Region

#Region " Public Member Declarations "

        ''' <summary>
        ''' This is the function signature used for defining a method to process items
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is required unless ProcessItemsFunction is implemented</para>
        ''' <para>This function is used when creating a queue to process one item at a time</para>
        ''' </remarks>
        ''' <param name="item">Item to be processed</param>
        Public Delegate Sub ProcessItemFunctionSignature(ByVal item As T)

        ''' <summary>
        ''' This is the function signature used for defining a method to process multiple items
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is required unless ProcessItemFunction is implemented</para>
        ''' <para>This function is used when creating a queue to process multiple items at once</para>
        ''' </remarks>
        ''' <param name="item">Item to be processed</param>
        Public Delegate Sub ProcessItemsFunctionSignature(ByVal item As T())

        ''' <summary>
        ''' This is the function signature used for determining if an item can be currently processed
        ''' </summary>
        ''' <remarks>
        ''' Implementation of this function is optional; it will be assumed that an item can be processed if this function is not defined
        ''' </remarks>
        ''' <returns>Function should return True if item can be processed</returns>
        Public Delegate Function CanProcessItemFunctionSignature(ByVal item As T) As Boolean

        ''' <summary>
        ''' This event will be raised after an item has been successfully processed
        ''' </summary>
        ''' <param name="item">Reference to item that has been successfully processed</param>
        ''' <remarks>
        ''' <para>This event allows custom handling of successfully processed items</para>
        ''' <para>When a process timeout is specified, this event allows you to know when the item completed processing in the allowed amount of time</para>
        ''' </remarks>
        Public Event ItemProcessed(ByVal item As T)

        ''' <summary>
        ''' This event will be raised if an item's processing time exceeds the specified process timeout
        ''' </summary>
        ''' <remarks>
        ''' This event allows custom handling of items that took too long to process
        ''' </remarks>
        ''' <param name="item">Reference to item that took too long to process</param>
        Public Event ItemProcessingTimedOut(ByVal item As T)

        ''' <summary>
        ''' This event will be raised if there is an exception encountered while attempting to processing an item in the list
        ''' </summary>
        ''' <remarks>
        ''' Processing won't stop for any exceptions thrown by the user function, but any captured exceptions will be exposed through this event
        ''' </remarks>
        Public Event ProcessException(ByVal ex As Exception)

        Public Const DefaultProcessInterval As Integer = 100
        Public Const DefaultMaximumThreads As Integer = 5
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite
        Public Const DefaultRequeueOnTimeout As Boolean = False
        Public Const RealTimeProcessInterval As Double = 0

#End Region

#Region " Private Member Declarations "

        Private m_processItemFunction As ProcessItemFunctionSignature
        Private m_canProcessItemFunction As CanProcessItemFunctionSignature
        Private m_processQueue As IList(Of T)
        Private m_maximumThreads As Integer
        Private m_processTimeout As Integer
        Private m_requeueOnTimeout As Boolean
        Private m_processingIsRealTime As Boolean
        Private m_threadCount As Integer
        Private m_processing As Boolean
        Private m_itemsProcessed As Long
        Private m_startTime As Long
        Private m_stopTime As Long
        Private m_realTimeProcessThread As Thread
        Private WithEvents m_processTimer As System.Timers.Timer

#End Region

#Region " Construction Functions "

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, Nothing, processInterval, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, Nothing, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' This constructor creates a ProcessQueue based on the generic List class
        ''' </summary>
        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            Me.New(processItemFunction, canProcessItemFunction, New List(Of T), processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Sub

        ''' <summary>
        ''' This constructor allows derived classes to define their own IList instance if desired
        ''' </summary>
        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processQueue As IList(Of T), ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            m_processItemFunction = processItemFunction
            m_canProcessItemFunction = canProcessItemFunction
            m_processQueue = processQueue
            m_maximumThreads = maximumThreads
            m_processTimeout = processTimeout
            m_requeueOnTimeout = requeueOnTimeout

            If processInterval = RealTimeProcessInterval Then
                ' Instantiate process list for real-time item processing
                m_processingIsRealTime = True
                m_maximumThreads = 1
            Else
                ' Instantiate process list for intervaled item processing
                m_processTimer = New System.Timers.Timer

                With m_processTimer
                    .Interval = processInterval
                    .AutoReset = True
                    .Enabled = False
                End With
            End If

        End Sub

#End Region

#Region " Public Methods Implementation "

        ''' <summary>
        ''' This property defines the user function used to process items in the list
        ''' </summary>
        Public Overridable Property ProcessItemFunction() As ProcessItemFunctionSignature
            Get
                Return m_processItemFunction
            End Get
            Set(ByVal value As ProcessItemFunctionSignature)
                m_processItemFunction = value
            End Set
        End Property

        Public Overridable Property CanProcessItemFunction() As CanProcessItemFunctionSignature
            Get
                Return m_canProcessItemFunction
            End Get
            Set(ByVal value As CanProcessItemFunctionSignature)
                m_canProcessItemFunction = value
            End Set
        End Property

        ''' <summary>
        ''' This property determines if items will be processed in real-time
        ''' </summary>
        Public Overridable ReadOnly Property ProcessingIsRealTime() As Boolean
            Get
                Return m_processingIsRealTime
            End Get
        End Property

        ''' <summary>
        ''' This property defines the interval, in milliseconds, on which new items begin processing
        ''' </summary>
        Public Overridable Property ProcessInterval() As Double
            Get
                If m_processingIsRealTime Then
                    Return RealTimeProcessInterval
                Else
                    Return m_processTimer.Interval
                End If
            End Get
            Set(ByVal value As Double)
                If m_processingIsRealTime Then
                    Throw New InvalidOperationException("Cannot change process interval when " & Name & " is configured for real-time processing")
                Else
                    m_processTimer.Interval = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Defines the maximum number of threads to process simultaneously
        ''' </summary>
        ''' <value>Sets the maximum number of processing threads</value>
        ''' <returns>Maximum number of processing threads</returns>
        ''' <remarks>If you set maximum threads to one, item processing will be synchronous</remarks>
        Public Overridable Property MaximumThreads() As Integer
            Get
                Return m_maximumThreads
            End Get
            Set(ByVal value As Integer)
                If m_processingIsRealTime Then
                    Throw New InvalidOperationException("Cannot change the maximum number of threads when " & Name & " is configured for real-time processing")
                Else
                    m_maximumThreads = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Defines the maximum time, in milliseconds, allowed for processing an item
        ''' </summary>
        ''' <value>Sets the maximum number of milliseconds allowed to process an item</value>
        ''' <returns>Maximum number of milliseconds allowed to process an item</returns>
        ''' <remarks>Set to Timeout.Infinite (i.e., -1) to allow processing to take as long as needed</remarks>
        Public Overridable Property ProcessTimeout() As Integer
            Get
                Return m_processTimeout
            End Get
            Set(ByVal value As Integer)
                m_processTimeout = value
            End Set
        End Property

        ''' <summary>
        ''' This property determines whether or not to automatically place an item back into the list if the processing times out
        ''' </summary>
        ''' <remarks>This property is ignored if the ProcessTimeout is set to Timeout.Infinite (i.e., -1)</remarks>
        Public Overridable Property RequeueOnTimeout() As Boolean
            Get
                Return m_requeueOnTimeout
            End Get
            Set(ByVal value As Boolean)
                m_requeueOnTimeout = value
            End Set
        End Property

        ''' <summary>
        ''' Starts item processing
        ''' </summary>
        Public Overridable Sub Start()

            m_processing = True
            m_threadCount = 0
            m_itemsProcessed = 0
            m_stopTime = 0
            m_startTime = Date.Now.Ticks

            If m_processingIsRealTime Then
                ' Start real-time processing thread
                m_realTimeProcessThread = New Thread(AddressOf RealTimeThreadProc)
                m_realTimeProcessThread.Priority = ThreadPriority.Highest
                m_realTimeProcessThread.Start()
            Else
                ' Start intervaled process timer
                m_processTimer.Enabled = True
            End If

        End Sub

        ''' <summary>
        ''' Stops item processing
        ''' </summary>
        Public Overridable Sub [Stop]()

            If m_processingIsRealTime Then
                ' Stop real-time processing thread
                If m_realTimeProcessThread IsNot Nothing Then m_realTimeProcessThread.Abort()
                m_realTimeProcessThread = Nothing
            Else
                ' Stop intervaled process timer
                m_processTimer.Enabled = False
            End If

            m_processing = False
            m_stopTime = Date.Now.Ticks

        End Sub

        ''' <summary>
        ''' Determines if the list is currently processing
        ''' </summary>
        Public Overridable Property Processing() As Boolean
            Get
                Return m_processing
            End Get
            Protected Set(ByVal value As Boolean)
                m_processing = value
            End Set
        End Property

        ''' <summary>
        ''' Returns the total number of items processed so far
        ''' </summary>
        Public Overridable ReadOnly Property ItemsProcessed() As Long
            Get
                Return m_itemsProcessed
            End Get
        End Property

        ''' <summary>
        ''' Returns the current number of active threads
        ''' </summary>
        Public Overridable ReadOnly Property ThreadCount() As Integer
            Get
                Return m_threadCount
            End Get
        End Property

        ''' <summary>
        ''' Returns the total amount of time, in seconds, that the process list has been active
        ''' </summary>
        Public Overridable ReadOnly Property RunTime() As Double
            Get
                Dim processingTime As Long

                If m_startTime > 0 Then
                    If m_stopTime > 0 Then
                        processingTime = m_stopTime - m_startTime
                    Else
                        processingTime = Date.Now.Ticks - m_startTime
                    End If
                End If

                If processingTime < 0 Then processingTime = 0

                Return TicksToSeconds(processingTime)
            End Get
        End Property

        Public Overridable ReadOnly Property Name() As String
            Get
                Return Me.GetType.Name
            End Get
        End Property

        Public Overridable ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("  Current processing state: ")
                    If m_processing Then
                        .Append("Executing")
                    Else
                        .Append("Idle")
                    End If
                    .Append(vbCrLf)
                    .Append("       Processing interval: ")
                    If m_processingIsRealTime Then
                        .Append("real-time")
                    Else
                        .Append(ProcessInterval)
                        .Append(" milliseconds")
                    End If
                    .Append(vbCrLf)
                    .Append("    Total process run time: ")
                    .Append(SecondsToText(RunTime))
                    .Append(vbCrLf)
                    .Append("   Queued items to process: ")
                    .Append(Count)
                    .Append(vbCrLf)
                    .Append("      Total active threads: ")
                    .Append(m_threadCount)
                    .Append(vbCrLf)
                    .Append("     Total items processed: ")
                    .Append(m_itemsProcessed)
                    .Append(vbCrLf)

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

#Region " Protected Methods Implementation "

        ' This property allows derived classes to access the interfaced internal process queue directly
        Protected ReadOnly Property InternalList() As IList(Of T)
            Get
                Return m_processQueue
            End Get
        End Property

        ' Perform thread-safe atomic increment on active thread count
        Protected Sub IncrementThreadCount()

            Interlocked.Increment(m_threadCount)

        End Sub

        ' Perform thread-safe atomic decrement on active thread count
        Protected Sub DecrementThreadCount()

            Interlocked.Decrement(m_threadCount)

        End Sub

        ' Perform thread-safe atomic increment on total processed items count
        Protected Sub IncrementItemsProcessed()

            Interlocked.Increment(m_itemsProcessed)

        End Sub

        ' You should use this function instead of invoking the m_canProcessItemFunction pointer
        ' directly since implementation of this delegate is optional
        Protected Function CanProcessItem(ByVal item As T) As Boolean

            If m_canProcessItemFunction Is Nothing Then
                ' If user provided no implementation for this function, we assume item can be processed
                Return True
            Else
                ' Otherwise we call user function to determine if item should be processed at this time
                Return m_canProcessItemFunction(item)
            End If

        End Function

        ' Derived classes can't raise event of their base classes so we expose these wrapper methods to accomodate as needed
        Protected Sub RaiseItemProcessed(ByVal item As T)

            RaiseEvent ItemProcessed(item)

        End Sub

        Protected Sub RaiseItemProcessingTimedOut(ByVal item As T)

            RaiseEvent ItemProcessingTimedOut(item)

        End Sub

        Protected Sub RaiseProcessException(ByVal ex As Exception)

            RaiseEvent ProcessException(ex)

        End Sub

#End Region

#Region " Item Processing Thread Procs "

        Private Sub RealTimeThreadProc()

            ' Create a real-time processing loop which will process items as fast as possible
            Do While True
                Try
                    ProcessNextItem()

                    ' We sleep the thread between each loop to help minimize CPU loading...
                    Thread.Sleep(1)
                Catch ex As ThreadAbortException
                    ' We egress gracefully if the thread's being aborted
                    Exit Do
                Catch ex As Exception
                    ' We won't stop for any errors thrown by the user function, but we will report them...
                    RaiseProcessException(ex)
                End Try
            Loop

        End Sub

        Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed

            ' The system timer creates an intervaled processing loop which will distribute processing of items across multiple threads if needed
            ProcessNextItem()

        End Sub

        Private Sub ProcessNextItem()

            Dim nextItem As T

            Try
                ' Handle all queue operations for getting next item in a single synchronous operation.
                ' We keep work to be done here down to a mimimum amount of time
                SyncLock m_processQueue
                    With m_processQueue
                        ' We get next item to be processed if the number of current process threads is less
                        ' than the maximum allowable number of process threads.
                        If .Count > 0 AndAlso ThreadCount < m_maximumThreads Then
                            ' Retrieve first item to be processed
                            nextItem = .Item(0)

                            ' Call optional user function to see if we should process this item
                            If CanProcessItem(nextItem) Then
                                ' We increment the thread counter using a thread safe operation
                                IncrementThreadCount()

                                ' Remove the item about to be processed from the queue
                                .RemoveAt(0)
                            Else
                                ' User opted not to process item at this time - we'll try again later
                                nextItem = Nothing
                            End If
                        End If
                    End With
                End SyncLock

                If nextItem IsNot Nothing Then
                    If m_processTimeout = Timeout.Infinite Then
                        ' If we have an item to process and the process queue wasn't setup with a process timeout, we just use
                        ' the current thread (i.e., the timer event or real-time thread) to process the next item taking as long
                        ' as we need for it to complete.  For timer events, the next item in the queue will begin processing even
                        ' if this item isn't completed - but no more than the specified number of maximum threads will ever be
                        ' spawned at once.
                        m_processItemFunction(nextItem)
                        IncrementItemsProcessed()
                        RaiseEvent ItemProcessed(nextItem)
                    Else
                        ' If we have an item to process and specified a process timeout we create a new thread to handle the
                        ' processing.  The timer event or real-time thread that invoked this method is already a new thread so
                        ' the only reason we create another thread is so that we can implement the process timeout if the
                        ' process takes to long to run.  We do this by joining the current thread (which will block it) until
                        ' the specified interval has passed or the process thread completes, whichever comes first.  This is a
                        ' safe operation since the current thread (i.e., the timer event or real-time thread) was already an
                        ' independent thread and won't block any other processing, including another timer event.
                        With New ProcessThread(Me, nextItem)
                            If Not .WaitUntil(m_processTimeout) Then
                                ' We notify user of process timeout in case they want to do anything special
                                RaiseEvent ItemProcessingTimedOut(nextItem)

                                ' We requeue item on processing timeout if requested
                                If m_requeueOnTimeout Then Insert(0, nextItem)
                            End If
                        End With
                    End If
                End If
            Catch ex As ThreadAbortException
                ' Rethrow thread abort so calling method can respond appropriately
                Throw ex
            Catch ex As Exception
                ' Processing won't stop for any errors thrown by the user function, but we will report them...
                RaiseProcessException(ex)
            Finally
                ' Decrement thread count if item was retrieved for processing
                If nextItem IsNot Nothing Then DecrementThreadCount()
            End Try

        End Sub

#End Region

#Region " Handy List(Of T) Functions Implementation "

        ' TODO: Implement all methods exposed by List(Of T) that aren't in IList(Of T) interface
        Public Overridable Sub InsertRange(ByVal index As Integer, ByVal collection As IEnumerable(Of T))

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if avaialble
                    DirectCast(m_processQueue, List(Of T)).InsertRange(index, collection)
                Else
                    ' Otherwise, we manually implement this feature...
                    For Each item As T In collection
                        m_processQueue.Insert(index, item)
                        index += 1
                    Next
                End If
            End SyncLock

        End Sub

#End Region

#Region " Generic IList(Of T) Implementation "

        ' Note: all IList(Of T) implementations should be synchronized as necessary

        ''' <summary>
        '''  Adds a new item to the list to be processed
        ''' </summary>
        Public Overridable Sub Add(ByVal item As T) Implements System.Collections.Generic.IList(Of T).Add

            SyncLock m_processQueue
                m_processQueue.Add(item)
            End SyncLock

        End Sub

        ''' <summary>
        ''' Inserts a new item to be processed at the top of the list
        ''' </summary>
        Public Overridable Sub Push(ByVal item As T)

            SyncLock m_processQueue
                m_processQueue.Insert(0, item)
            End SyncLock

        End Sub

        ''' <summary>
        ''' Inserts a new item to be processed at the specified location
        ''' </summary>
        Public Overridable Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert

            SyncLock m_processQueue
                m_processQueue.Insert(index, item)
            End SyncLock

        End Sub

        Public Overridable Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements System.Collections.Generic.IList(Of T).CopyTo

            SyncLock m_processQueue
                m_processQueue.CopyTo(array, arrayIndex)
            End SyncLock

        End Sub

        Public Overridable Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator

            Return m_processQueue.GetEnumerator()

        End Function

        ''' <summary>
        '''  Removes the first item from the list and returns its value
        ''' </summary>
        ''' <exception cref="IndexOutOfRangeException">
        ''' If there are no items in the list, this function will throw an IndexOutOfRangeException
        ''' </exception>
        Public Overridable Function Pop() As T

            SyncLock m_processQueue
                If m_processQueue.Count > 0 Then
                    Dim poppedItem As T = m_processQueue(0)
                    m_processQueue.RemoveAt(0)
                    Return poppedItem
                Else
                    Throw New IndexOutOfRangeException("The " & Name & " is empty")
                End If
            End SyncLock

        End Function

        ''' <summary>
        '''  Removes the last item from the list and returns its value
        ''' </summary>
        ''' <exception cref="IndexOutOfRangeException">
        ''' If there are no items in the list, this function will throw an IndexOutOfRangeException
        ''' </exception>
        Public Overridable Function Poop() As T

            SyncLock m_processQueue
                If m_processQueue.Count > 0 Then
                    Dim lastIndex As Integer = m_processQueue.Count - 1
                    Dim poopedItem As T = m_processQueue(lastIndex)
                    m_processQueue.RemoveAt(lastIndex)
                    Return poopedItem
                Else
                    Throw New IndexOutOfRangeException("The " & Name & " is empty")
                End If
            End SyncLock

        End Function

        Default Public Overridable Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
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

        Public Overridable Function IndexOf(ByVal item As T) As Integer Implements System.Collections.Generic.IList(Of T).IndexOf

            SyncLock m_processQueue
                Return m_processQueue.IndexOf(item)
            End SyncLock

        End Function

        Public Overridable ReadOnly Property Count() As Integer Implements System.Collections.Generic.IList(Of T).Count
            Get
                SyncLock m_processQueue
                    Return m_processQueue.Count
                End SyncLock
            End Get
        End Property

        Public Overridable Sub Clear() Implements System.Collections.Generic.IList(Of T).Clear

            SyncLock m_processQueue
                m_processQueue.Clear()
            End SyncLock

        End Sub

        Public Overridable Function Contains(ByVal item As T) As Boolean Implements System.Collections.Generic.IList(Of T).Contains

            SyncLock m_processQueue
                Return m_processQueue.Contains(item)
            End SyncLock

        End Function

        Public Overridable Function Remove(ByVal item As T) As Boolean Implements System.Collections.Generic.IList(Of T).Remove

            SyncLock m_processQueue
                m_processQueue.Remove(item)
            End SyncLock

        End Function

        Public Overridable Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt

            SyncLock m_processQueue
                m_processQueue.RemoveAt(index)
            End SyncLock

        End Sub

        Public Overridable ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.IList(Of T).IsReadOnly
            Get
                Return m_processQueue.IsReadOnly
            End Get
        End Property

#End Region

#Region " IEnumerable Implementation "

        Private Function IEnumerableGetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator

            Return DirectCast(m_processQueue, IEnumerable).GetEnumerator()

        End Function

#End Region

#Region " ICollection Implementation "

        Public Overridable ReadOnly Property SyncRoot() As Object Implements System.Collections.ICollection.SyncRoot
            Get
                Return m_processQueue
            End Get
        End Property

        ' This collection is effectively "synchronized" since all functions synclock operations internally
        Public Overridable ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
            Get
                Return True
            End Get
        End Property

        Private Sub ICollectionCopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo

            CopyTo(array, index)

        End Sub

        Private ReadOnly Property ICollectionCount() As Integer Implements System.Collections.ICollection.Count
            Get
                Return Count
            End Get
        End Property

#End Region

    End Class

End Namespace