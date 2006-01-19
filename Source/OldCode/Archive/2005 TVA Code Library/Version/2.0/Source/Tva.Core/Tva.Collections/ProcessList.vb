'*******************************************************************************************************
'  Tva.Collections.ProcessList.vb - Multi-threaded Intervaled Item Processing Class
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
Imports System.Text
Imports Tva.DateTime.Common

Namespace Collections

    ''' <summary>
    ''' <para>This class will process a list of items on independent threads</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Consumers are expected to create new instances of this class through the static construction functions (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class ProcessList(Of T)

        Implements IList(Of T), ICollection

        ' This internal class is used to limit item processing time if requested
        Private Class ProcessThread

            Private m_parent As ProcessList(Of T)
            Private m_thread As Thread
            Private m_item As T

            Public Sub New(ByVal parent As ProcessList(Of T), ByVal item As T)

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

                Try
                    ' Invoke user function to process item
                    m_parent.ProcessItemFunction.Invoke(m_item)
                    m_parent.IncrementItemsProcessed()
                Catch ex As ThreadAbortException
                    ' We egress gracefully if the thread's being aborted
                    Exit Sub
                Catch ex As Exception
                    ' Processing won't stop for any errors thrown by the user function, but we will report them...
                    m_parent.RaiseProcessException(ex)
                End Try

            End Sub

        End Class

        ' **************************************
        '
        '       Public Member Declarations
        '
        ' **************************************

        ''' <summary>
        ''' This is the function signature used for defining a method to process items
        ''' </summary>
        ''' <param name="item">Item to be processed</param>
        Public Delegate Sub ProcessItemFunctionSignature(ByVal item As T)

        ''' <summary>
        ''' This event will be raised if there is an exception encountered while attempting to processing an item in the list
        ''' </summary>
        Public Event ProcessException(ByVal ex As Exception)

        ''' <summary>
        ''' This event will be raised if an item's processing time exceeds the specified process timeout
        ''' </summary>
        ''' <remarks>
        ''' This event allows custom handling of items that took too long to process
        ''' </remarks>
        ''' <param name="timedOutItem">Reference to item that took too long to process</param>
        Public Event ProcessItemTimeout(ByVal timedOutItem As T)

        Public Const DefaultProcessInterval As Integer = 100
        Public Const DefaultMaximumThreads As Integer = 5
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite
        Public Const DefaultRequeueOnTimeout As Boolean = False
        Public Const RealTimeProcessInterval As Double = 0

        ' **************************************
        '
        '      Private Member Declarations
        '
        ' **************************************

        Private m_processItemFunction As ProcessItemFunctionSignature
        Private m_processList As IList(Of T)
        Private WithEvents m_processTimer As System.Timers.Timer
        Private m_realTimeThread As Thread
        Private m_processingIsRealTime As Boolean
        Private m_threadCount As Integer
        Private m_processing As Boolean
        Private m_itemsProcessed As Long
        Private m_startTime As Long
        Private m_stopTime As Long
        Private m_maximumThreads As Integer
        Private m_processTimeout As Integer
        Private m_requeueOnTimeout As Boolean

        ' **************************************
        '
        '        Construction Functions
        '
        ' **************************************

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessList(Of T)

            Return CreateAsynchronousQueue(processItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessList(Of T)

            Return CreateAsynchronousQueue(processItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessList(Of T)

            Return New ProcessList(Of T)(processItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessList(Of T)

            Return CreateSynchronousQueue(processItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessList(Of T)

            Return New ProcessList(Of T)(processItemFunction, processInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessList(Of T)

            Return CreateRealTimeQueue(processItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessList(Of T)

            Return New ProcessList(Of T)(processItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            Me.New(processItemFunction, New List(Of T), processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Sub

        ' This constructor allows derived classes to define their own IList instance if desired
        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processList As IList(Of T), ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            m_processItemFunction = processItemFunction
            m_processList = processList
            m_maximumThreads = maximumThreads
            m_processTimeout = processTimeout
            m_requeueOnTimeout = requeueOnTimeout

            If processInterval = RealTimeProcessInterval Then
                ' Instantiate class for real-time item processing
                m_processingIsRealTime = True
                m_maximumThreads = 1
            Else
                ' Instantiate class for intervaled item processing
                m_processTimer = New System.Timers.Timer

                With m_processTimer
                    .Interval = processInterval
                    .AutoReset = True
                    .Enabled = False
                End With
            End If

        End Sub

        ' **************************************
        '
        '      Public Member Implementation
        '
        ' **************************************

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
            m_itemsProcessed = 0
            m_stopTime = 0
            m_startTime = Date.Now.Ticks

            If m_processingIsRealTime Then
                ' Start real-time processing thread
                m_realTimeThread = New Thread(AddressOf RealTimeThreadProc)
                m_realTimeThread.Start()
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
                If m_realTimeThread IsNot Nothing Then m_realTimeThread.Abort()
                m_realTimeThread = Nothing
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

                Return processingTime / 10000000L
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
                        .Append("Real-Time")
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

        ' **************************************
        '
        '     Protected Member Implementation
        '
        ' **************************************

        Protected ReadOnly Property InternalList() As IList(Of T)
            Get
                Return m_processList
            End Get
        End Property

        Protected Sub IncrementThreadCount()

            Interlocked.Increment(m_threadCount)

        End Sub

        Protected Sub DecrementThreadCount()

            Interlocked.Decrement(m_threadCount)

        End Sub

        Protected Sub IncrementItemsProcessed()

            Interlocked.Increment(m_itemsProcessed)

        End Sub

        Protected Sub RaiseProcessException(ByVal ex As Exception)

            RaiseEvent ProcessException(ex)

        End Sub

        ' **************************************
        '
        '      Item Processing Thread Procs
        '
        ' **************************************

        Private Sub RealTimeThreadProc()

            ' Create a real-time processing loop which will process items as fast as possible
            Do While True
                Try
                    ProcessNextItem()

                    ' We sleep the thread between each loop to help reduce CPU loading...
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
                SyncLock m_processList
                    With m_processList
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
                        ' the current thread (i.e., the timer event or real-time thread) to process the next item taking as long
                        ' as we need for it to complete.  For timer events, the next item in the queue will begin processing even
                        ' if this item isn't completed - but no more than the specified number of maximum threads will ever be
                        ' spawned at once.
                        m_processItemFunction(nextItem)
                        IncrementItemsProcessed()
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
                                RaiseEvent ProcessItemTimeout(nextItem)

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

        ' **************************************
        '
        '      Generic IList Implementation
        '
        ' **************************************

        ''' <summary>
        '''  Adds a new item to the list to be processed
        ''' </summary>
        Public Sub Add(ByVal item As T) Implements System.Collections.Generic.IList(Of T).Add

            SyncLock m_processList
                m_processList.Add(item)
            End SyncLock

        End Sub

        ''' <summary>
        ''' Inserts a new item to be processed at the top of the list
        ''' </summary>
        Public Sub Push(ByVal item As T)

            SyncLock m_processList
                m_processList.Insert(0, item)
            End SyncLock

        End Sub

        ''' <summary>
        ''' Inserts a new item to be processed at the specified location
        ''' </summary>
        Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert

            SyncLock m_processList
                m_processList.Insert(index, item)
            End SyncLock

        End Sub

        Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements System.Collections.Generic.IList(Of T).CopyTo

            SyncLock m_processList
                m_processList.CopyTo(array, arrayIndex)
            End SyncLock

        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator

            Return m_processList.GetEnumerator()

        End Function

        ''' <summary>
        '''  Removes the first item from the list and returns its value
        ''' </summary>
        ''' <exception cref="IndexOutOfRangeException">
        ''' If there are no items in the list, this function will throw an IndexOutOfRangeException
        ''' </exception>
        Public Function Pop() As T

            SyncLock m_processList
                If m_processList.Count > 0 Then
                    Dim poppedItem As T = m_processList(0)
                    m_processList.RemoveAt(0)
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
        Public Function Poop() As T

            SyncLock m_processList
                If m_processList.Count > 0 Then
                    Dim lastIndex As Integer = m_processList.Count - 1
                    Dim poopedItem As T = m_processList(lastIndex)
                    m_processList.RemoveAt(lastIndex)
                    Return poopedItem
                Else
                    Throw New IndexOutOfRangeException("The " & Name & " is empty")
                End If
            End SyncLock

        End Function

        Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
            Get
                SyncLock m_processList
                    Return m_processList(index)
                End SyncLock
            End Get
            Set(ByVal value As T)
                SyncLock m_processList
                    m_processList(index) = value
                End SyncLock
            End Set
        End Property

        Public Function IndexOf(ByVal item As T) As Integer Implements System.Collections.Generic.IList(Of T).IndexOf

            SyncLock m_processList
                Return m_processList.IndexOf(item)
            End SyncLock

        End Function

        Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.IList(Of T).Count
            Get
                SyncLock m_processList
                    Return m_processList.Count
                End SyncLock
            End Get
        End Property

        Public Sub Clear() Implements System.Collections.Generic.IList(Of T).Clear

            SyncLock m_processList
                m_processList.Clear()
            End SyncLock

        End Sub

        Public Function Contains(ByVal item As T) As Boolean Implements System.Collections.Generic.IList(Of T).Contains

            SyncLock m_processList
                Return m_processList.Contains(item)
            End SyncLock

        End Function

        Public Function Remove(ByVal item As T) As Boolean Implements System.Collections.Generic.IList(Of T).Remove

            SyncLock m_processList
                m_processList.Remove(item)
            End SyncLock

        End Function

        Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt

            SyncLock m_processList
                m_processList.RemoveAt(index)
            End SyncLock

        End Sub

        Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.IList(Of T).IsReadOnly
            Get
                Return False
            End Get
        End Property

        ' **************************************
        '
        '       ICollection Implementation
        '
        ' **************************************

        Private ReadOnly Property ICollection() As ICollection
            Get
                Return DirectCast(m_processList, ICollection)
            End Get
        End Property

        Public ReadOnly Property SyncRoot() As Object Implements System.Collections.ICollection.SyncRoot
            Get
                Return m_processList
            End Get
        End Property

        ' This collection is effectively "synchronized" since all functions synclock operations internally
        Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
            Get
                Return True
            End Get
        End Property

        Private Sub ICollectionCopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo

            SyncLock m_processList
                ICollection.CopyTo(array, index)
            End SyncLock

        End Sub

        Private Function ICollectionGetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator

            Return ICollection.GetEnumerator()

        End Function

        Private ReadOnly Property ICollectionCount() As Integer Implements System.Collections.ICollection.Count
            Get
                SyncLock m_processList
                    Return ICollection.Count
                End SyncLock
            End Get
        End Property

    End Class

End Namespace