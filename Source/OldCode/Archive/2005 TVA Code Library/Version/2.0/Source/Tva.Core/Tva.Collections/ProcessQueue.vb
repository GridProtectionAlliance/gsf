'*******************************************************************************************************
'  Tva.Collections.ProcessQueue.vb - Multi-threaded Item Processing Queue
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/07/2006 - James R Carroll
'       Original version of source code generated
'  02/12/2006 - James R Carroll
'       Added multi-item bulk processing functionality
'
'*******************************************************************************************************

Imports System.Threading
Imports System.Text
Imports Tva.DateTime.Common

Namespace Collections

#Region " Process Queue Enumerations "

    ''' <summary>
    ''' Enumeration of possible queue threading modes
    ''' </summary>
    Public Enum QueueThreadingMode
        Asynchronous
        Synchronous
    End Enum

    ''' <summary>
    ''' Enumeration of possible queue processing styles
    ''' </summary>
    Public Enum QueueProcessingStyle
        OneAtATime
        ManyAtOnce
    End Enum

#End Region

    ''' <summary>
    ''' <para>This class will process a collection of items on independent threads</para>
    ''' <para>Consumer must implement a function to process items</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Consumers are expected to create new instances of this class through the static construction functions (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class ProcessQueue(Of T)

        Implements IList(Of T), ICollection

#Region " Public Member Declarations "

        ''' <summary>
        ''' This is the function signature used for defining a method to process items one at a time
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is required unless ProcessItemsFunction is implemented</para>
        ''' <para>This function is used when creating a queue to process one item at a time</para>
        ''' <para>Asynchronous queues will process individual items on multiple threads</para>
        ''' </remarks>
        ''' <param name="item">Item to be processed</param>
        Public Delegate Sub ProcessItemFunctionSignature(ByVal item As T)

        ''' <summary>
        ''' This is the function signature used for defining a method to process multiple items at once
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is required unless ProcessItemFunction is implemented</para>
        ''' <para>This function is used when creating a queue to process multiple items at once</para>
        ''' <para>Asynchronous queues will process groups of items on multiple threads</para>
        ''' </remarks>
        ''' <param name="item">Item to be processed</param>
        Public Delegate Sub ProcessItemsFunctionSignature(ByVal item As T())

        ''' <summary>
        ''' This is the function signature used for determining if an item can be currently processed
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is optional; it will be assumed that an item can be processed if this function is not defined</para>
        ''' <para>Items must eventually get to a state where they can be processed or they will remain in the queue forever</para>
        ''' <para>
        ''' Note that when this function is implemented and ProcessingStyle = ManyAtOnce (i.e., ProcessItemsFunction is defined)
        ''' then each item presented for processing must evaluate as "CanProcessItem = True" before any items are processed
        ''' </para>
        ''' </remarks>
        ''' <param name="item">Item to be checked for processing availablity</param>
        ''' <returns>Function should return True if item can be processed</returns>
        Public Delegate Function CanProcessItemFunctionSignature(ByVal item As T) As Boolean

        ''' <summary>
        ''' This event will be raised after an item has been successfully processed
        ''' </summary>
        ''' <param name="item">Reference to item that has been successfully processed</param>
        ''' <remarks>
        ''' <para>This event allows custom handling of successfully processed items</para>
        ''' <para>When a process timeout is specified, this event allows you to know when the item completed processing in the allowed amount of time</para>
        ''' <para>This function will only be raised when ProcessingStyle = OneAtATime (i.e., ProcessItemFunction is defined)</para>
        ''' </remarks>
        Public Event ItemProcessed(ByVal item As T)

        ''' <summary>
        ''' This event will be raised after an array of items have been successfully processed
        ''' </summary>
        ''' <param name="items">Reference to items that have been successfully processed</param>
        ''' <remarks>
        ''' <para>This event allows custom handling of successfully processed items</para>
        ''' <para>When a process timeout is specified, this event allows you to know when the item completed processing in the allowed amount of time</para>
        ''' <para>This function will only be raised when ProcessingStyle = ManyAtOnce (i.e., ProcessItemsFunction is defined)</para>
        ''' </remarks>
        Public Event ItemsProcessed(ByVal items As T())

        ''' <summary>
        ''' This event will be raised if an item's processing time exceeds the specified process timeout
        ''' </summary>
        ''' <remarks>
        ''' <para>This event allows custom handling of items that took too long to process</para>
        ''' <para>This function will only be raised when ProcessingStyle = OneAtATime (i.e., ProcessItemFunction is defined)</para>
        ''' </remarks>
        ''' <param name="item">Reference to item that took too long to process</param>
        Public Event ItemTimedOut(ByVal item As T)

        ''' <summary>
        ''' This event will be raised if processing time for an array of items exceeds the specified process timeout
        ''' </summary>
        ''' <remarks>
        ''' <para>This event allows custom handling of items that took too long to process</para>
        ''' <para>This function will only be raised when ProcessingStyle = ManyAtOnce (i.e., ProcessItemsFunction is defined)</para>
        ''' </remarks>
        ''' <param name="items">Reference to items that took too long to process</param>
        Public Event ItemsTimedOut(ByVal items As T())

        ''' <summary>
        ''' This event will be raised if there is an exception encountered while attempting to processing an item in the list
        ''' </summary>
        ''' <remarks>
        ''' Processing won't stop for any exceptions thrown by the user function, but any captured exceptions will be exposed through this event
        ''' </remarks>
        Public Event ProcessException(ByVal ex As Exception)

        ''' <summary>Default processing interval (in milliseconds)</summary>
        Public Const DefaultProcessInterval As Integer = 100

        ''' <summary>Default maximum number of processing threads</summary>
        Public Const DefaultMaximumThreads As Integer = 5

        ''' <summary>Default processing timeout (in milliseconds)</summary>
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite

        ''' <summary>Default setting for requeuing items on processing timeout</summary>
        Public Const DefaultRequeueOnTimeout As Boolean = False

        ''' <summary>Default setting for requeuing items on processing exceptions</summary>
        Public Const DefaultRequeueOnException As Boolean = False

        ''' <summary>Default real-time processing interval (in milliseconds)</summary>
        Public Const RealTimeProcessInterval As Double = 0.0#

#End Region

#Region " Private Member Declarations "

        Private m_processItemFunction As ProcessItemFunctionSignature
        Private m_processItemsFunction As ProcessItemsFunctionSignature
        Private m_canProcessItemFunction As CanProcessItemFunctionSignature
        Private m_processQueue As IList(Of T)
        Private m_maximumThreads As Integer
        Private m_processTimeout As Integer
        Private m_requeueOnTimeout As Boolean
        Private m_requeueOnException As Boolean
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

#Region " Single-Item Processing Constructors "

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, Nothing, processInterval, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, Nothing, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

#End Region

#Region " Multi-Item Processing Constructors "

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemsFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemsFunction, Nothing, processInterval, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemsFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemsFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemsFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemsFunction, Nothing, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemsFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

#End Region

#Region " Protected Constructors "

        ''' <summary>
        ''' This constructor creates a ProcessQueue based on the generic List(Of T) class
        ''' </summary>
        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean)

            MyClass.New(processItemFunction, Nothing, canProcessItemFunction, New List(Of T), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Sub

        ''' <summary>
        ''' This constructor creates a bulk-item ProcessQueue based on the generic List(Of T) class
        ''' </summary>
        Protected Sub New(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean)

            MyClass.New(Nothing, processItemsFunction, canProcessItemFunction, New List(Of T), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Sub

        ''' <summary>
        ''' This constructor allows derived classes to define their own IList instance if desired
        ''' </summary>
        Protected Sub New( _
                ByVal processItemFunction As ProcessItemFunctionSignature, _
                ByVal processItemsFunction As ProcessItemsFunctionSignature, _
                ByVal canProcessItemFunction As CanProcessItemFunctionSignature, _
                ByVal processQueue As IList(Of T), _
                ByVal processInterval As Double, _
                ByVal maximumThreads As Integer, _
                ByVal processTimeout As Integer, _
                ByVal requeueOnTimeout As Boolean, _
                ByVal requeueOnException As Boolean)

            m_processItemFunction = processItemFunction         ' Defining this function creates a ProcessingStyle = OneAtATime process queue
            m_processItemsFunction = processItemsFunction       ' Defining this function creates a ProcessingStyle = ManyAtOnce process queue
            m_canProcessItemFunction = canProcessItemFunction
            m_processQueue = processQueue
            m_maximumThreads = maximumThreads
            m_processTimeout = processTimeout
            m_requeueOnTimeout = requeueOnTimeout
            m_requeueOnException = requeueOnException

            If processInterval = RealTimeProcessInterval Then
                ' Instantiate process queue for real-time item processing
                m_processingIsRealTime = True
                m_maximumThreads = 1
            Else
                ' Instantiate process queue for intervaled item processing
                m_processTimer = New System.Timers.Timer

                With m_processTimer
                    .Interval = processInterval
                    .AutoReset = True
                    .Enabled = False
                End With
            End If

        End Sub

#End Region

#End Region

#Region " Public Methods Implementation "

        ''' <summary>
        ''' This property defines the user function used to process items in the list one at a time
        ''' </summary>
        ''' <remarks>
        ''' <para>This function and ProcessItemsFunction cannot be defined at the same time</para>
        ''' <para>A queue must be defined to process a single item at a time or many items at once</para>
        ''' <para>Implementation of this function makes ProcessingStyle = OneAtATime</para>
        ''' </remarks>
        Public Overridable Property ProcessItemFunction() As ProcessItemFunctionSignature
            Get
                Return m_processItemFunction
            End Get
            Set(ByVal value As ProcessItemFunctionSignature)
                If value IsNot Nothing Then
                    m_processItemFunction = value
                    m_processItemsFunction = Nothing
                End If
            End Set
        End Property

        ''' <summary>
        ''' This property defines the user function used to process multiple items in the list at once
        ''' </summary>
        ''' <remarks>
        ''' <para>This function and ProcessItemFunction cannot be defined at the same time</para>
        ''' <para>A queue must be defined to process a single item at a time or many items at once</para>
        ''' <para>Implementation of this function makes ProcessingStyle = ManyAtOnce</para>
        ''' </remarks>
        Public Overridable Property ProcessItemsFunction() As ProcessItemsFunctionSignature
            Get
                Return m_processItemsFunction
            End Get
            Set(ByVal value As ProcessItemsFunctionSignature)
                If value IsNot Nothing Then
                    m_processItemsFunction = value
                    m_processItemFunction = Nothing
                End If
            End Set
        End Property

        ''' <summary>
        ''' This property defines the user function used to determine if an item is ready to be processed
        ''' </summary>
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
        ''' This property determines the threading mode for the process queue (i.e., synchronous or asynchronous)
        ''' </summary>
        ''' <remarks>
        ''' <para>The maximum number of processing threads determines the threading mode</para>
        ''' <para>If the maximum threads are set to one, item processing will be synchronous (i.e., ThreadingMode = Synchronous)</para>
        ''' <para>If the maximum threads are more than one, item processing will be asynchronous (i.e., ThreadingMode = Asynchronous)</para>
        ''' <para>
        ''' Note that for asynchronous queues the processing interval will control how many threads are spawned
        ''' at once.  If items are processed faster than the specified processing interval, only one process thread
        ''' will ever be spawned at a time.  To ensure multiple threads are utilized to process queue items, lower
        ''' the process interval (minimum process interval is 1 millisecond).
        ''' </para>
        ''' </remarks>
        Public Overridable ReadOnly Property ThreadingMode() As QueueThreadingMode
            Get
                If m_maximumThreads > 1 Then
                    Return QueueThreadingMode.Asynchronous
                Else
                    Return QueueThreadingMode.Synchronous
                End If
            End Get
        End Property

        ''' <summary>
        ''' This property determines the item processing style for the process queue (i.e., one at a time or many at once)
        ''' </summary>
        ''' <remarks>
        ''' <para>The implemented item processing function determines the processing style</para>
        ''' <para>If the ProcessItemFunction is implemented, the processing style will be one at a time (i.e., ProcessingStyle = OneAtATime)</para>
        ''' <para>If the ProcessItemsFunction is implemented, the processing style will be many at once (i.e., ProcessingStyle = ManyAtOnce)</para>
        ''' <para>
        ''' Note that if the processing style is many at once, all available items in the queue are presented for processing
        ''' at each processing interval.  If you expect items to be processed in the order in which they were received, make
        ''' sure you use a synchronous queue.  Real-time queues are inheriently synchronous.
        ''' </para>
        ''' </remarks>
        Public Overridable ReadOnly Property ProcessingStyle() As QueueProcessingStyle
            Get
                If m_processItemFunction Is Nothing Then
                    Return QueueProcessingStyle.ManyAtOnce
                Else
                    Return QueueProcessingStyle.OneAtATime
                End If
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
        ''' <remarks>If you set maximum threads to one, item processing will be synchronous (i.e., ThreadingMode = Synchronous)</remarks>
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
        ''' This property determines whether or not to automatically place an item back into the list if an exception occurs while processing
        ''' </summary>
        Public Overridable Property RequeueOnException() As Boolean
            Get
                Return m_requeueOnException
            End Get
            Set(ByVal value As Boolean)
                m_requeueOnException = value
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

            ' Note that real-time queues have their main thread running continually, but for
            ' intervaled queues processing occurs only when data is available to processed
            If m_processingIsRealTime Then
                ' Start real-time processing thread
                m_realTimeProcessThread = New Thread(AddressOf RealTimeThreadProc)
                m_realTimeProcessThread.Priority = ThreadPriority.Highest
                m_realTimeProcessThread.Start()
                IncrementThreadCount()
            End If

        End Sub

        ''' <summary>
        ''' Stops item processing
        ''' </summary>
        Public Overridable Sub [Stop]()

            m_processing = False

            If m_processingIsRealTime Then
                ' Stop real-time processing thread
                If m_realTimeProcessThread IsNot Nothing Then
                    m_realTimeProcessThread.Abort()
                    DecrementThreadCount()
                End If

                m_realTimeProcessThread = Nothing
            Else
                ' Stop intervaled processing, if active
                m_processTimer.Enabled = False
            End If

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
        Public Overridable ReadOnly Property TotalProcessedItems() As Long
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

        ''' <summary>
        ''' Returns class name
        ''' </summary>
        ''' <remarks>
        ''' <para>This name is used for class identification in strings (e.g., used in error message)</para>
        ''' <para>Derived classes should override this method with a proper class name</para>
        ''' </remarks>
        Public Overridable ReadOnly Property Name() As String
            Get
                Return Me.GetType.Name
            End Get
        End Property

        ''' <summary>
        ''' Returns current status of processing queue
        ''' </summary>
        ''' <remarks>
        ''' This is useful for checking on the current status of the queue
        ''' </remarks>
        Public Overridable ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("  Current processing state: ")
                    If m_processing Then
                        .Append("Executing")
                    Else
                        .Append("Idle")
                    End If
                    .Append(Environment.NewLine)
                    .Append("       Processing interval: ")
                    If m_processingIsRealTime Then
                        .Append("Real-time")
                    Else
                        .Append(ProcessInterval)
                        .Append(" milliseconds")
                    End If
                    .Append(Environment.NewLine)
                    .Append("        Processing timeout: ")
                    If m_processTimeout = Timeout.Infinite Then
                        .Append("Infinite")
                    Else
                        .Append(m_processTimeout)
                        .Append(" milliseconds")
                    End If
                    .Append(Environment.NewLine)
                    .Append("      Queue threading mode: ")
                    If ThreadingMode = QueueThreadingMode.Asynchronous Then
                        .Append("Asynchronous - ")
                        .Append(m_maximumThreads)
                        .Append(" maximum threads")
                    Else
                        .Append("Synchronous")
                    End If
                    .Append(Environment.NewLine)
                    .Append("    Queue processing style: ")
                    If ProcessingStyle = QueueProcessingStyle.OneAtATime Then
                        .Append("One at a time")
                    Else
                        .Append("Many at once")
                    End If
                    .Append(Environment.NewLine)
                    .Append("    Total process run time: ")
                    .Append(SecondsToText(RunTime))
                    .Append(Environment.NewLine)
                    .Append("   Queued items to process: ")
                    .Append(Count)
                    .Append(Environment.NewLine)
                    .Append("      Total active threads: ")
                    .Append(m_threadCount)
                    .Append(Environment.NewLine)
                    .Append("     Total items processed: ")
                    .Append(m_itemsProcessed)
                    .Append(Environment.NewLine)

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

#Region " Protected Methods Implementation "

        ''' <summary>
        ''' This property allows derived classes to access the interfaced internal process queue directly
        ''' </summary>
        Protected ReadOnly Property InternalList() As IList(Of T)
            Get
                Return m_processQueue
            End Get
        End Property

        ''' <summary>
        ''' This method is used to let the class know that data was added so it can begin processing data
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Derived classes *must* make sure to call this method after data gets added so that the
        ''' process timer can be enabled for intervaled queues and data processing can begin
        ''' </para>
        ''' <para>
        ''' To make sure items in the queue always get processed, this function is expected to be
        ''' invoked from within a SyncLock of the exposed SyncRoot (i.e., m_processQueue)
        ''' </para>
        ''' </remarks>
        Protected Sub DataAdded()

            ' For queues that are not processing in real-time, we start the intervaled process timer
            ' when data is added, if it's not running already
            If Not m_processingIsRealTime AndAlso Not m_processTimer.Enabled Then m_processTimer.Enabled = m_processing

        End Sub

        ''' <summary>
        ''' Performs thread-safe atomic increment on active thread count
        ''' </summary>
        Protected Sub IncrementThreadCount()

            Interlocked.Increment(m_threadCount)

        End Sub

        ''' <summary>
        ''' Performs thread-safe atomic decrement on active thread count
        ''' </summary>
        Protected Sub DecrementThreadCount()

            Interlocked.Decrement(m_threadCount)

        End Sub

        ''' <summary>
        ''' Performs thread-safe atomic increment on total processed items count
        ''' </summary>
        Protected Sub IncrementItemsProcessed()

            Interlocked.Increment(m_itemsProcessed)

        End Sub

        ''' <summary>
        ''' Performs thread-safe atomic addition on total processed items count
        ''' </summary>
        Protected Sub IncrementItemsProcessed(ByVal totalProcessed As Integer)

            Interlocked.Add(m_itemsProcessed, totalProcessed)

        End Sub

        ''' <summary>
        ''' Determines if an item can be processed
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' If user provided no implementation for the CanProcessItemFunction, we assume item can be processed
        ''' </para>
        ''' <para>
        ''' You should use this function instead of invoking the CanProcessItemFunction pointer
        ''' directly since implementation of this delegate is optional
        ''' </para>
        ''' </remarks>
        Protected Overridable Function CanProcessItem(ByVal item As T) As Boolean

            If m_canProcessItemFunction Is Nothing Then
                ' If user provided no implementation for this function, we assume item can be processed
                Return True
            Else
                ' Otherwise we call user function to determine if item should be processed at this time
                Return m_canProcessItemFunction(item)
            End If

        End Function

        ''' <summary>
        ''' Determines if all items can be processed
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' If user provided no implementation for the CanProcessItemFunction, we assume all items can be processed
        ''' </para>
        ''' <para>
        ''' You should use this function instead of invoking the CanProcessItemFunction pointer
        ''' directly since implementation of this delegate is optional
        ''' </para>
        ''' </remarks>
        Protected Overridable Function CanProcessItems(ByVal items As T()) As Boolean

            If m_canProcessItemFunction Is Nothing Then
                ' If user provided no implementation for this function, we assume all items can be processed
                Return True
            Else
                ' Otherwise we call user function for each item to determine if all items are ready for processing
                Dim allItemsCanBeProcessed As Boolean = True

                For Each item As T In items
                    If Not m_canProcessItemFunction(item) Then
                        allItemsCanBeProcessed = False
                        Exit For
                    End If
                Next

                Return allItemsCanBeProcessed
            End If

        End Function

        ''' <summary>
        ''' Raises the base class ItemProcessed event
        ''' </summary>
        ''' <remarks>
        ''' Derived classes can't raise events of their base classes so we expose event wrapper methods to accomodate as needed
        ''' </remarks>
        Protected Sub RaiseItemProcessed(ByVal item As T)

            RaiseEvent ItemProcessed(item)

        End Sub

        ''' <summary>
        ''' Raises the base class ItemsProcessed event
        ''' </summary>
        ''' <remarks>
        ''' Derived classes can't raise events of their base classes so we expose event wrapper methods to accomodate as needed
        ''' </remarks>
        Protected Sub RaiseItemsProcessed(ByVal items As T())

            RaiseEvent ItemsProcessed(items)

        End Sub

        ''' <summary>
        ''' Raises the base class ItemTimedOut event
        ''' </summary>
        ''' <remarks>
        ''' Derived classes can't raise events of their base classes so we expose event wrapper methods to accomodate as needed
        ''' </remarks>
        Protected Sub RaiseItemTimedOut(ByVal item As T)

            RaiseEvent ItemTimedOut(item)

        End Sub

        ''' <summary>
        ''' Raises the base class ItemsTimedOut event
        ''' </summary>
        ''' <remarks>
        ''' Derived classes can't raise events of their base classes so we expose event wrapper methods to accomodate as needed
        ''' </remarks>
        Protected Sub RaiseItemsTimedOut(ByVal items As T())

            RaiseEvent ItemsTimedOut(items)

        End Sub

        ''' <summary>
        ''' Raises the base class ProcessException event
        ''' </summary>
        ''' <remarks>
        ''' Derived classes can't raise events of their base classes so we expose event wrapper methods to accomodate as needed
        ''' </remarks>
        Protected Sub RaiseProcessException(ByVal ex As Exception)

            RaiseEvent ProcessException(ex)

        End Sub

#End Region

#Region " Private Methods Implementation "

#Region " Internal Thread Processing Class "

        ' This internal class is used to limit item processing time if requested
        Private Class ProcessThread

            Private m_parent As ProcessQueue(Of T)
            Private m_thread As Thread
            Private m_item As T
            Private m_items As T()

            Public Sub New(ByVal parent As ProcessQueue(Of T), ByVal item As T)

                m_parent = parent
                m_item = item
                m_thread = New Thread(AddressOf ProcessItem)
                m_thread.Start()

            End Sub

            Public Sub New(ByVal parent As ProcessQueue(Of T), ByVal items As T())

                m_parent = parent
                m_items = items
                m_thread = New Thread(AddressOf ProcessItems)
                m_thread.Start()

            End Sub

            ' Block calling thread until specified time has expired
            Public Function WaitUntil(ByVal timeout As Integer) As Boolean

                Dim threadComplete As Boolean = m_thread.Join(timeout)
                If Not threadComplete Then m_thread.Abort()
                Return threadComplete

            End Function

            Private Sub ProcessItem()

                m_parent.ProcessItem(m_item)

            End Sub

            Private Sub ProcessItems()

                m_parent.ProcessItems(m_items)

            End Sub

        End Class

#End Region

#Region " Item Processing Thread Procs "

        ' This function handles standard processing of a single item
        Private Sub ProcessItem(ByVal item As T)

            Try
                ' Invoke user function to process item
                m_processItemFunction(item)
                IncrementItemsProcessed()
                RaiseEvent ItemProcessed(item)
            Catch ex As ThreadAbortException
                ' Rethrow thread abort so calling method can respond appropriately
                Throw ex
            Catch ex As Exception
                ' We requeue item on processing exception if requested
                If m_requeueOnException Then Insert(0, item)

                ' Processing won't stop for any errors thrown by the user function, but we will report them...
                RaiseEvent ProcessException(ex)
            End Try

        End Sub

        ' This function handles standard processing of multiple items
        Private Sub ProcessItems(ByVal items As T())

            Try
                ' Invoke user function to process items
                m_processItemsFunction(items)
                IncrementItemsProcessed(items.Length)
                RaiseEvent ItemsProcessed(items)
            Catch ex As ThreadAbortException
                ' Rethrow thread abort so calling method can respond appropriately
                Throw ex
            Catch ex As Exception
                ' We requeue items on processing exception if requested
                If m_requeueOnException Then InsertRange(0, items)

                ' Processing won't stop for any errors thrown by the user function, but we will report them...
                RaiseEvent ProcessException(ex)
            End Try

        End Sub

        ' This method creates a real-time thread for processing items
        Private Sub RealTimeThreadProc()

            ' Create a real-time processing loop which will process items as fast as possible
            Do While True
                Try
                    If m_processItemsFunction Is Nothing Then
                        ' Process one item at a time
                        ProcessNextItem()
                    Else
                        ' Process multiple items at once
                        ProcessNextItems()
                    End If

                    ' We sleep the thread between each loop to help minimize CPU loading...
                    Thread.Sleep(1)
                Catch ex As ThreadAbortException
                    ' We egress gracefully if the thread's being aborted
                    Exit Do
                Catch ex As Exception
                    ' We won't stop for any errors thrown by the user function, but we will report them...
                    RaiseEvent ProcessException(ex)
                End Try
            Loop

        End Sub

        ' This method is invoked on an interval for processing items
        Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed

            ' The system timer creates an intervaled processing loop which will distribute processing of items across multiple threads if needed
            If m_processItemsFunction Is Nothing Then
                ' Process one item at a time
                ProcessNextItem()
            Else
                ' Process multiple items at once
                ProcessNextItems()
            End If

            SyncLock m_processQueue
                ' We go ahead and stop the process timer if there's no more data to process (no need to waste CPU cycles)...
                If m_processQueue.Count = 0 Then m_processTimer.Enabled = False
            End SyncLock

        End Sub

        ' Process next item - handles processing of items one at a time (i.e., ProcessingStyle = OneAtATime)
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
                        ProcessItem(nextItem)
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
                                RaiseEvent ItemTimedOut(nextItem)

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
                ' Processing won't stop for any errors encountered here, but we will report them...
                RaiseEvent ProcessException(ex)
            Finally
                ' Decrement thread count if item was retrieved for processing
                If nextItem IsNot Nothing Then DecrementThreadCount()
            End Try

        End Sub

        ' Process next items - handles processing of an array of items at once (i.e., ProcessingStyle = ManyAtOnce)
        Private Sub ProcessNextItems()

            Dim nextItems As T()

            Try
                ' Handle all queue operations for getting next items in a single synchronous operation.
                ' We keep work to be done here down to a mimimum amount of time
                SyncLock m_processQueue
                    With m_processQueue
                        ' We get next items to be processed if the number of current process threads is less
                        ' than the maximum allowable number of process threads.
                        If .Count > 0 AndAlso ThreadCount < m_maximumThreads Then
                            ' Retrieve items to be processed
                            nextItems = ToArray()

                            ' Call optional user function to see if we should process these items
                            If CanProcessItems(nextItems) Then
                                ' We increment the thread counter using a thread safe operation
                                IncrementThreadCount()

                                ' Clear all items from the queue
                                .Clear()
                            Else
                                ' User opted not to process items at this time - we'll try again later
                                nextItems = Nothing
                            End If
                        End If
                    End With
                End SyncLock

                If nextItems IsNot Nothing Then
                    If m_processTimeout = Timeout.Infinite Then
                        ' If we have items to process and the process queue wasn't setup with a process timeout, we just use the
                        ' current thread (i.e., the timer event or real-time thread) to process the next items taking as long as
                        ' we need for them to complete.  For timer events, any new items available in the queue will be processed
                        ' even if the current items haven't completed - but no more than the specified number of maximum threads
                        ' will ever be spawned at once.
                        ProcessItems(nextItems)
                    Else
                        ' If we have items to process and specified a process timeout we create a new thread to handle the
                        ' processing.  The timer event or real-time thread that invoked this method is already a new thread so
                        ' the only reason we create another thread is so that we can implement the process timeout if the
                        ' process takes to long to run.  We do this by joining the current thread (which will block it) until
                        ' the specified interval has passed or the process thread completes, whichever comes first.  This is a
                        ' safe operation since the current thread (i.e., the timer event or real-time thread) was already an
                        ' independent thread and won't block any other processing, including another timer event.
                        With New ProcessThread(Me, nextItems)
                            If Not .WaitUntil(m_processTimeout) Then
                                ' We notify user of process timeout in case they want to do anything special
                                RaiseEvent ItemsTimedOut(nextItems)

                                ' We requeue items on processing timeout if requested
                                If m_requeueOnTimeout Then InsertRange(0, nextItems)
                            End If
                        End With
                    End If
                End If
            Catch ex As ThreadAbortException
                ' Rethrow thread abort so calling method can respond appropriately
                Throw ex
            Catch ex As Exception
                ' Processing won't stop for any errors encountered here, but we will report them...
                RaiseEvent ProcessException(ex)
            Finally
                ' Decrement thread count if items were retrieved for processing
                If nextItems IsNot Nothing Then DecrementThreadCount()
            End Try

        End Sub

#End Region

#End Region

#Region " Handy List(Of T) Functions Implementation "

        ' The internal list is declared as an IList(Of T) - this way derived classes (e.g., KeyedProcessQueue) can
        ' use their own list implementation for process functionality.  However, the regular List(Of T) provides
        ' many handy functions not required to be exposed by the IList(Of T) interface.  So if the implemented list
        ' happens to be a List(Of T), we'll expose this native functionality and otherwise we implement it for you.
        ' Now, wasn't that nice of me!?  You'll thank me later. :)

        ' Note: all List(Of T) implementations should be synchronized as necessary

        ''' <summary>
        ''' Adds the elements of the specified collection to the end of the queue.
        ''' </summary>
        ''' <param name="collection">
        ''' The collection whose elements should be added to the end of the queue.
        ''' The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        ''' </param>
        ''' <exception cref="ArgumentNullException">collection is null.</exception>
        Public Overridable Sub AddRange(ByVal collection As IEnumerable(Of T))

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    DirectCast(m_processQueue, List(Of T)).AddRange(collection)
                Else
                    ' Otherwise, we manually implement this feature...
                    If collection Is Nothing Then Throw New ArgumentNullException("collection", "collection is null")

                    For Each item As T In collection
                        m_processQueue.Add(item)
                    Next
                End If

                DataAdded()
            End SyncLock

        End Sub

        '''	<summary>
        ''' Searches the entire sorted queue using a binary search algorithm for an element using the
        ''' default comparer and returns the zero-based index of the element.
        ''' </summary>
        ''' <remarks>
        ''' Queue must be sorted in order for this function to return an accurate result
        ''' </remarks>
        '''	<param name="item">The object to locate. The value can be null for reference types.</param>
        ''' <returns>
        ''' The zero-based index of item in the sorted queue, if item is found; otherwise, a negative number that is the
        ''' bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        ''' the bitwise complement of count.
        ''' </returns>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Function BinarySearch(ByVal item As T) As Integer

            SyncLock m_processQueue
                Return BinarySearch(0, m_processQueue.Count, item, Nothing)
            End SyncLock

        End Function

        '''	<summary>
        ''' Searches the entire sorted queue using a binary search algorithm for an element using the
        ''' specified comparer and returns the zero-based index of the element.
        ''' </summary>
        ''' <remarks>
        ''' Queue must be sorted in order for this function to return an accurate result
        ''' </remarks>
        '''	<param name="item">The object to locate. The value can be null for reference types.</param>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- null to use the default comparer: Generic.Comparer(Of T).Default</param>
        ''' <returns>
        ''' The zero-based index of item in the sorted queue, if item is found; otherwise, a negative number that is the
        ''' bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        ''' the bitwise complement of count.
        ''' </returns>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Function BinarySearch(ByVal item As T, ByVal comparer As IComparer(Of T)) As Integer

            SyncLock m_processQueue
                Return BinarySearch(0, m_processQueue.Count, item, comparer)
            End SyncLock

        End Function

        '''	<summary>
        ''' Searches a range of elements in the sorted queue using a binary search algorithm for an
        ''' element using the specified comparer and returns the zero-based index of the element.
        ''' </summary>
        ''' <remarks>
        ''' Queue must be sorted in order for this function to return an accurate result
        ''' </remarks>
        ''' <param name="index">The zero-based starting index of the range to search.</param>
        ''' <param name="count">The length of the range to search.</param>
        '''	<param name="item">The object to locate. The value can be null for reference types.</param>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- null to use the default comparer: Generic.Comparer(Of T).Default</param>
        ''' <returns>
        ''' The zero-based index of item in the sorted queue, if item is found; otherwise, a negative number that is the
        ''' bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        ''' the bitwise complement of count.
        ''' </returns>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue -or- count is less than 0 -or- startIndex and count do not specify a valid section in the queue</exception>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Function BinarySearch(ByVal index As Integer, ByVal count As Integer, ByVal item As T, ByVal comparer As IComparer(Of T)) As Integer

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).BinarySearch(index, count, item, comparer)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.BinarySearch(Of T)(m_processQueue, index, count, item, comparer)

                    'Dim foundIndex As Integer = -1
                    'Dim startIndex As Integer = index
                    'Dim stopIndex As Integer = index + count - 1
                    'Dim currentIndex As Integer
                    'Dim result As Integer

                    '' Validate start and stop index...
                    'If startIndex < 0 OrElse count < 0 OrElse stopIndex > m_processQueue.Count - 1 Then Throw New ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue")
                    'If comparer Is Nothing Then comparer = Generic.Comparer(Of T).Default

                    'If count > 0 Then
                    '    Do While True
                    '        ' Find next mid point
                    '        currentIndex = startIndex + (stopIndex - startIndex) \ 2

                    '        ' Compare item at mid-point
                    '        result = comparer.Compare(item, m_processQueue(currentIndex))

                    '        If result = 0 Then
                    '            ' Found item, return located index
                    '            foundIndex = currentIndex
                    '            Exit Do
                    '        ElseIf startIndex = stopIndex Then
                    '            ' Met in the middle and didn't find match - so we're finished
                    '            foundIndex = startIndex Xor -1
                    '            Exit Do
                    '        ElseIf result > 0 Then
                    '            If currentIndex < count - 1 Then
                    '                ' Item is beyond current item, so we start search at next item
                    '                startIndex = currentIndex + 1
                    '            Else
                    '                ' Looked to the end and didn't find match - so we're finished
                    '                foundIndex = (count - 1) Xor -1
                    '                Exit Do
                    '            End If
                    '        Else
                    '            If currentIndex > 0 Then
                    '                ' Item is before current item, so we will stop search at current item
                    '                ' Note that because of the way the math works, you don't stop at the
                    '                ' prior item as you might guess - it can cause you to skip an item
                    '                stopIndex = currentIndex
                    '            Else
                    '                ' Looked to the top and didn't find match - so we're finished
                    '                foundIndex = 0 Xor -1
                    '                Exit Do
                    '            End If
                    '        End If
                    '    Loop
                    'End If

                    'Return foundIndex
                End If
            End SyncLock

        End Function

        ''' <summary>Converts the elements in the current queue to another type, and returns a list containing the converted elements.</summary>
        ''' <returns>A generic list of the target type containing the converted elements from the current queue.</returns>
        ''' <param name="converter">A Converter delegate that converts each element from one type to another type.</param>
        ''' <exception cref="ArgumentNullException">converter is null.</exception>
        Public Overridable Function ConvertAll(Of TOutput)(ByVal converter As Converter(Of T, TOutput)) As List(Of TOutput)

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).ConvertAll(converter)
                Else
                    ' Otherwise, we manually implement this feature...
                    If converter Is Nothing Then Throw New ArgumentNullException("converter", "converter is null")

                    Dim result As New List(Of TOutput)

                    For Each item As T In m_processQueue
                        result.Add(converter(item))
                    Next

                    Return result
                End If
            End SyncLock

        End Function

        ''' <summary>Determines whether the queue contains elements that match the conditions defined by the specified predicate.</summary>
        ''' <returns>true if the queue contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function Exists(ByVal match As Predicate(Of T)) As Boolean

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).Exists(match)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.Exists(Of T)(m_processQueue, match)

                    'If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    '' Otherwise, we manually implement this feature...
                    'Dim found As Boolean

                    'With m_processQueue
                    '    For x As Integer = 0 To .Count - 1
                    '        If match(.Item(x)) Then
                    '            found = True
                    '            Exit For
                    '        End If
                    '    Next
                    'End With

                    'Return found
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire queue.</summary>
        ''' <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function Find(ByVal match As Predicate(Of T)) As T

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).Find(match)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.Find(Of T)(m_processQueue, match)

                    'If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    'Dim foundItem As T
                    'Dim foundIndex As Integer = FindIndex(match)

                    'If foundIndex >= 0 Then foundItem = m_processQueue(foundIndex)

                    'Return foundItem
                End If
            End SyncLock

        End Function

        ''' <summary>Retrieves the all the elements that match the conditions defined by the specified predicate.</summary>
        ''' <returns>A generic list containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty list.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindAll(ByVal match As Predicate(Of T)) As List(Of T)

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).FindAll(match)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return New List(Of T)(Array.FindAll(Of T)(m_processQueue, match))

                    'If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    'Dim foundItems As New List(Of T)

                    'For Each item As T In m_processQueue
                    '    If match(item) Then foundItems.Add(item)
                    'Next

                    'Return foundItems
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the queue that extends from the specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindIndex(ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                Return FindIndex(0, m_processQueue.Count, match)
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the queue that extends from the specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
        ''' <param name="startIndex">The zero-based starting index of the search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue.</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindIndex(ByVal startIndex As Integer, ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                Return FindIndex(startIndex, m_processQueue.Count, match)
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the queue that extends from the specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
        ''' <param name="startIndex">The zero-based starting index of the search.</param>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue -or- count is less than 0 -or- startIndex and count do not specify a valid section in the queue</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindIndex(ByVal startIndex As Integer, ByVal count As Integer, ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).FindIndex(startIndex, count, match)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.FindIndex(Of T)(m_processQueue, startIndex, count, match)

                    'If startIndex < 0 OrElse count < 0 OrElse startIndex + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue")
                    'If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    'Dim foundindex As Integer = -1

                    'With m_processQueue
                    '    For x As Integer = startIndex To startIndex + count - 1
                    '        If match(.Item(x)) Then
                    '            foundindex = x
                    '            Exit For
                    '        End If
                    '    Next
                    'End With

                    'Return foundindex
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire queue.</summary>
        ''' <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLast(ByVal match As Predicate(Of T)) As T

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).FindLast(match)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.FindLast(Of T)(m_processQueue, match)

                    'If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    'Dim foundItem As T
                    'Dim foundIndex As Integer = FindLastIndex(match)

                    'If foundIndex >= 0 Then foundItem = m_processQueue(foundIndex)

                    'Return foundItem
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire queue.</summary>
        ''' <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLastIndex(ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                Return FindLastIndex(0, m_processQueue.Count, match)
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the queue that extends from the first element to the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
        ''' <param name="startIndex">The zero-based starting index of the backward search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue.</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLastIndex(ByVal startIndex As Integer, ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                Return FindLastIndex(startIndex, m_processQueue.Count, match)
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the queue that contains the specified number of elements and ends at the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <param name="startIndex">The zero-based starting index of the backward search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue -or- count is less than 0 -or- startIndex and count do not specify a valid section in the queue.</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLastIndex(ByVal startIndex As Integer, ByVal count As Integer, ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).FindLastIndex(startIndex, count, match)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.FindLastIndex(Of T)(m_processQueue, startIndex, count, match)

                    'If startIndex < 0 OrElse count < 0 OrElse startIndex + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue")
                    'If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    'Dim foundindex As Integer = -1

                    'With m_processQueue
                    '    For x As Integer = startIndex + count - 1 To startIndex Step -1
                    '        If match(.Item(x)) Then
                    '            foundindex = x
                    '            Exit For
                    '        End If
                    '    Next
                    'End With

                    'Return foundindex
                End If
            End SyncLock

        End Function

        ''' <summary>Performs the specified action on each element of the queue.</summary>
        ''' <param name="action">The Action delegate to perform on each element of the queue.</param>
        ''' <exception cref="ArgumentNullException">action is null.</exception>
        Public Overridable Sub ForEach(ByVal action As Action(Of T))

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    DirectCast(m_processQueue, List(Of T)).ForEach(action)
                Else
                    ' Otherwise, we manually implement this feature...
                    Array.ForEach(Of T)(m_processQueue, action)

                    'If action Is Nothing Then Throw New ArgumentNullException("action", "action is null")

                    'For Each item As T In m_processQueue
                    '    action(item)
                    'Next
                End If
            End SyncLock

        End Sub

        ''' <summary>Creates a shallow copy of a range of elements in the source queue.</summary>
        ''' <returns>A shallow copy of a range of elements in the source queue.</returns>
        ''' <param name="count">The number of elements in the range.</param>
        ''' <param name="index">The zero-based queue index at which the range starts.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        ''' <exception cref="ArgumentException">index and count do not denote a valid range of elements in the queue.</exception>
        Public Overridable Function GetRange(ByVal index As Integer, ByVal count As Integer) As List(Of T)

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).GetRange(index, count)
                Else
                    ' Otherwise, we manually implement this feature...
                    If index + count > m_processQueue.Count Then Throw New ArgumentException("Index and count do not denote a valid range of elements in the queue")
                    If index < 0 OrElse count < 0 Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    Dim items As New List(Of T)

                    For x As Integer = index To index + count - 1
                        items.Add(m_processQueue(x))
                    Next

                    Return items
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the queue that extends from the specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of item within the range of elements in the queue that extends from index to the last element, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue.</exception>
        Public Overridable Function IndexOf(ByVal item As T, ByVal index As Integer) As Integer

            SyncLock m_processQueue
                Return IndexOf(item, index, m_processQueue.Count)
            End SyncLock

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the queue that starts at the specified index and contains the specified number of elements.</summary>
        ''' <returns>The zero-based index of the first occurrence of item within the range of elements in the queue that starts at index and contains count number of elements, if found; otherwise, –1.</returns>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue -or- count is less than 0 -or- index and count do not specify a valid section in the queue.</exception>
        Public Overridable Function IndexOf(ByVal item As T, ByVal index As Integer, ByVal count As Integer) As Integer

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).IndexOf(item, index, count)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.IndexOf(Of T)(m_processQueue, item, index, count)

                    'If index < 0 OrElse count < 0 OrElse index + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    'Dim foundindex As Integer = -1
                    'Dim comparer As Generic.Comparer(Of T) = Generic.Comparer(Of T).Default

                    'With m_processQueue
                    '    For x As Integer = index To index + count - 1
                    '        If comparer.Compare(item, .Item(x)) = 0 Then
                    '            foundindex = x
                    '            Exit For
                    '        End If
                    '    Next
                    'End With

                    'Return foundindex
                End If
            End SyncLock

        End Function

        ''' <summary>Inserts the elements of a collection into the queue at the specified index.</summary>
        ''' <param name="collection">The collection whose elements should be inserted into the queue. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        ''' <param name="index">The zero-based index at which the new elements should be inserted.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than queue length.</exception>
        ''' <exception cref="ArgumentNullException">collection is null.</exception>
        Public Overridable Sub InsertRange(ByVal index As Integer, ByVal collection As IEnumerable(Of T))

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    DirectCast(m_processQueue, List(Of T)).InsertRange(index, collection)
                Else
                    ' Otherwise, we manually implement this feature...
                    If index < 0 OrElse index > m_processQueue.Count - 1 Then Throw New ArgumentOutOfRangeException("index", "index is outside the range of valid indexes for the queue")
                    If collection Is Nothing Then Throw New ArgumentNullException("collection", "collection is null")

                    For Each item As T In collection
                        m_processQueue.Insert(index, item)
                        index += 1
                    Next
                End If

                DataAdded()
            End SyncLock

        End Sub

        ''' <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire queue.</summary>
        ''' <returns>The zero-based index of the last occurrence of item within the entire the queue, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        Public Overridable Function LastIndexOf(ByVal item As T) As Integer

            SyncLock m_processQueue
                Return LastIndexOf(item, 0, m_processQueue.Count)
            End SyncLock

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the queue that extends from the first element to the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of item within the range of elements in the queue that extends from the first element to index, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the backward search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue. </exception>
        Public Overridable Function LastIndexOf(ByVal item As T, ByVal index As Integer) As Integer

            SyncLock m_processQueue
                Return LastIndexOf(item, index, m_processQueue.Count)
            End SyncLock

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the queue that contains the specified number of elements and ends at the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of item within the range of elements in the queue that contains count number of elements and ends at index, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the backward search.</param>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue -or- count is less than 0 -or- index and count do not specify a valid section in the queue.</exception>
        Public Overridable Function LastIndexOf(ByVal item As T, ByVal index As Integer, ByVal count As Integer) As Integer

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).LastIndexOf(item, index, count)
                Else
                    ' Otherwise, we manually implement this feature...
                    Return Array.LastIndexOf(Of T)(m_processQueue, item, index, count)

                    'If index < 0 OrElse count < 0 OrElse index + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    'Dim foundindex As Integer = -1
                    'Dim comparer As Generic.Comparer(Of T) = Generic.Comparer(Of T).Default

                    'With m_processQueue
                    '    For x As Integer = index + count - 1 To index Step -1
                    '        If comparer.Compare(item, .Item(x)) = 0 Then
                    '            foundindex = x
                    '            Exit For
                    '        End If
                    '    Next
                    'End With

                    'Return foundindex
                End If
            End SyncLock

        End Function

        ''' <summary>Removes the all the elements that match the conditions defined by the specified predicate.</summary>
        ''' <returns>The number of elements removed from the queue .</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function RemoveAll(ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).RemoveAll(match)
                Else
                    ' Otherwise, we manually implement this feature...
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim removedItems As Integer

                    With m_processQueue
                        For x As Integer = .Count - 1 To 0 Step -1
                            If match(.Item(x)) Then
                                .RemoveAt(x)
                                removedItems += 1
                            End If
                        Next
                    End With

                    Return removedItems
                End If
            End SyncLock

        End Function

        ''' <summary>Removes a range of elements from the queue.</summary>
        ''' <param name="count">The number of elements to remove.</param>
        ''' <param name="index">The zero-based starting index of the range of elements to remove.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        ''' <exception cref="ArgumentException">index and count do not denote a valid range of elements in the queue.</exception>
        Public Overridable Sub RemoveRange(ByVal index As Integer, ByVal count As Integer)

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    DirectCast(m_processQueue, List(Of T)).RemoveRange(index, count)
                Else
                    ' Otherwise, we manually implement this feature...
                    If index < 0 OrElse count < 0 OrElse index + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    For x As Integer = index + count - 1 To index Step -1
                        m_processQueue.RemoveAt(x)
                    Next
                End If
            End SyncLock

        End Sub

        ''' <summary>Reverses the order of the elements in the entire queue.</summary>
        Public Overridable Sub Reverse()

            SyncLock m_processQueue
                Reverse(0, m_processQueue.Count)
            End SyncLock

        End Sub

        ''' <summary>Reverses the order of the elements in the specified range.</summary>
        ''' <param name="count">The number of elements in the range to reverse.</param>
        ''' <param name="index">The zero-based starting index of the range to reverse.</param>
        ''' <exception cref="ArgumentException">index and count do not denote a valid range of elements in the queue. </exception>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        Public Overridable Sub Reverse(ByVal index As Integer, ByVal count As Integer)

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    DirectCast(m_processQueue, List(Of T)).Reverse(index, count)                    
                Else
                    ' Otherwise, we manually implement this feature...
                    If index + count > m_processQueue.Count Then Throw New ArgumentException("Index and count do not denote a valid range of elements in the queue")
                    If index < 0 OrElse count < 0 Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    Dim item As T
                    Dim stopIndex As Integer = index + count - 1

                    With m_processQueue
                        For x As Integer = index To (index + count - 1) \ 2
                            If x < stopIndex Then
                                ' Swap items top to bottom to reverse order
                                item = .Item(x)
                                .Item(x) = .Item(stopIndex)
                                .Item(stopIndex) = item
                                stopIndex -= 1
                            End If
                        Next
                    End With
                End If
            End SyncLock

        End Sub

        ''' <summary>Sorts the elements in the entire queue using the default comparer.</summary>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Sub Sort()

            SyncLock m_processQueue
                Sort(0, m_processQueue.Count, Nothing)
            End SyncLock

        End Sub

        ''' <summary>Sorts the elements in the entire queue using the specified comparer.</summary>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use the default comparer: Generic.Comparer.Default.</param>
        ''' <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
        '''	<exception cref="InvalidOperationException">the comparer is null and the default comparer, Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Sub Sort(ByVal comparer As IComparer(Of T))

            SyncLock m_processQueue
                Sort(0, m_processQueue.Count, comparer)
            End SyncLock

        End Sub

        ''' <summary>Sorts the elements in a range of elements in the queue using the specified comparer.</summary>
        ''' <param name="count">The length of the range to sort.</param>
        ''' <param name="index">The zero-based starting index of the range to sort.</param>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use the default comparer: Generic.Comparer.Default.</param>
        ''' <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
        '''	<exception cref="InvalidOperationException">the comparer is null and the default comparer, Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        Public Overridable Sub Sort(ByVal index As Integer, ByVal count As Integer, ByVal comparer As IComparer(Of T))

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    DirectCast(m_processQueue, List(Of T)).Sort(index, count, comparer)
                Else
                    ' Otherwise, we manually implement this feature...
                    Array.Sort(Of T)(m_processQueue, index, count, comparer)
                End If
            End SyncLock

        End Sub

        ''' <summary>Sorts the elements in the entire queue using the specified Comparison.</summary>
        ''' <param name="comparison">The Comparison to use when comparing elements.</param>
        ''' <exception cref="ArgumentException">The implementation of comparison caused an error during the sort. For example, comparison might not return 0 when comparing an item with itself.</exception>
        ''' <exception cref="ArgumentNullException">comparison is null.</exception>
        Public Overridable Sub Sort(ByVal comparison As Comparison(Of T))

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    DirectCast(m_processQueue, List(Of T)).Sort(comparison)
                Else
                    ' Otherwise, we manually implement this feature...                    
                    Array.Sort(Of T)(DirectCast(m_processQueue, T()), comparison)
                End If
            End SyncLock

        End Sub

        ''' <summary>Copies the elements of the queue to a new array.</summary>
        ''' <returns>An array containing copies of the elements of the queue.</returns>
        Public Overridable Function ToArray() As T()

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).ToArray()
                Else
                    ' Otherwise, we manually implement this feature...
                    With m_processQueue
                        Dim items As T() = Array.CreateInstance(GetType(T), .Count)

                        For x As Integer = 0 To .Count - 1
                            items(x) = .Item(x)
                        Next

                        Return items
                    End With
                End If
            End SyncLock

        End Function

        ''' <summary>Determines whether every element in the queue matches the conditions defined by the specified predicate.</summary>
        ''' <returns>true if every element in the queue matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions to check against the elements.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function TrueForAll(ByVal match As Predicate(Of T)) As Boolean

            SyncLock m_processQueue
                If TypeOf m_processQueue Is List(Of T) Then
                    ' We'll call native implementation if available
                    Return DirectCast(m_processQueue, List(Of T)).TrueForAll(match)
                Else
                    ' Otherwise, we manually implement this feature...                    
                    Return Array.TrueForAll(Of T)(m_processQueue, match)
                End If
            End SyncLock

        End Function

#End Region

#Region " Handy Queue Functions Implementation "

        ' Note: all queue function implementations should be synchronized as necessary

        ''' <summary>Inserts an item onto the top of the queue</summary>
        ''' <param name="item">The item to push onto the queue.</param>
        Public Overridable Sub Push(ByVal item As T)

            SyncLock m_processQueue
                m_processQueue.Insert(0, item)
                DataAdded()
            End SyncLock

        End Sub

        ''' <summary>Removes the first item from the queue and returns its value</summary>
        ''' <exception cref="IndexOutOfRangeException">there are no items in the queue</exception>
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

        ''' <summary>Removes the last item from the queue and returns its value</summary>
        ''' <exception cref="IndexOutOfRangeException">there are no items in the queue</exception>
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

#End Region

#Region " Generic IList(Of T) Implementation "

        ' Note: all IList(Of T) implementations should be synchronized as necessary

        ''' <summary>Adds an item to the queue.</summary>
        ''' <param name="item">The item to add to the queue.</param>
        Public Overridable Sub Add(ByVal item As T) Implements IList(Of T).Add

            SyncLock m_processQueue
                m_processQueue.Add(item)
                DataAdded()
            End SyncLock

        End Sub

        ''' <summary>Inserts an element into the queue at the specified index.</summary>
        ''' <param name="item">The object to insert. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based index at which item should be inserted.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than queue length.</exception>
        Public Overridable Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert

            SyncLock m_processQueue
                m_processQueue.Insert(index, item)
                DataAdded()
            End SyncLock

        End Sub

        ''' <summary>Copies the entire queue to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
        ''' <param name="array">The one-dimensional array that is the destination of the elements copied from queue. The array must have zero-based indexing.</param>
        ''' <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        ''' <exception cref="ArgumentException">arrayIndex is equal to or greater than the length of array -or- the number of elements in the source queue is greater than the available space from arrayIndex to the end of the destination array.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        ''' <exception cref="ArgumentNullException">array is null.</exception>
        Public Overridable Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements IList(Of T).CopyTo

            SyncLock m_processQueue
                m_processQueue.CopyTo(array, arrayIndex)
            End SyncLock

        End Sub

        ''' <summary>Returns an enumerator that iterates through the queue.</summary>
        ''' <returns>An enumerator for the queue.</returns>
        Public Overridable Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator

            Return m_processQueue.GetEnumerator()

        End Function

        ''' <summary>Gets or sets the element at the specified index.</summary>
        ''' <returns>The element at the specified index.</returns>
        ''' <param name="index">The zero-based index of the element to get or set.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than queue length. </exception>
        Default Public Overridable Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
            Get
                SyncLock m_processQueue
                    Return m_processQueue(index)
                End SyncLock
            End Get
            Set(ByVal value As T)
                SyncLock m_processQueue
                    m_processQueue(index) = value
                    DataAdded()
                End SyncLock
            End Set
        End Property

        ''' <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire queue.</summary>
        ''' <returns>The zero-based index of the first occurrence of item within the entire queue, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        Public Overridable Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf

            SyncLock m_processQueue
                Return m_processQueue.IndexOf(item)
            End SyncLock

        End Function

        ''' <summary>Gets the number of elements actually contained in the queue.</summary>
        ''' <returns>The number of elements actually contained in the queue.</returns>
        Public Overridable ReadOnly Property Count() As Integer Implements IList(Of T).Count
            Get
                SyncLock m_processQueue
                    Return m_processQueue.Count
                End SyncLock
            End Get
        End Property

        ''' <summary>Removes all elements from the queue.</summary>
        Public Overridable Sub Clear() Implements IList(Of T).Clear

            SyncLock m_processQueue
                m_processQueue.Clear()
            End SyncLock

        End Sub

        ''' <summary>Determines whether an element is in the queue.</summary>
        ''' <returns>true if item is found in the queue; otherwise, false.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        Public Overridable Function Contains(ByVal item As T) As Boolean Implements IList(Of T).Contains

            SyncLock m_processQueue
                Return m_processQueue.Contains(item)
            End SyncLock

        End Function

        ''' <summary>Removes the first occurrence of a specific object from the queue.</summary>
        ''' <returns>true if item is successfully removed; otherwise, false.  This method also returns false if item was not found in the queue.</returns>
        ''' <param name="item">The object to remove from the queue. The value can be null for reference types.</param>
        Public Overridable Function Remove(ByVal item As T) As Boolean Implements IList(Of T).Remove

            SyncLock m_processQueue
                m_processQueue.Remove(item)
            End SyncLock

        End Function

        ''' <summary>Removes the element at the specified index of the queue.</summary>
        ''' <param name="index">The zero-based index of the element to remove.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than queue length.</exception>
        Public Overridable Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt

            SyncLock m_processQueue
                m_processQueue.RemoveAt(index)
            End SyncLock

        End Sub

        ''' <summary>Gets a value indicating whether the queue is read-only.</summary>
        ''' <returns>true if the queue is read-only; otherwise, false.  In the default implementation, this property always returns false.</returns>
        Public Overridable ReadOnly Property IsReadOnly() As Boolean Implements IList(Of T).IsReadOnly
            Get
                Return m_processQueue.IsReadOnly
            End Get
        End Property

#End Region

#Region " IEnumerable Implementation "

        Private Function IEnumerableGetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return DirectCast(m_processQueue, IEnumerable).GetEnumerator()

        End Function

#End Region

#Region " ICollection Implementation "

        ''' <summary>Gets an object that can be used to synchronize access to the queue.</summary>
        ''' <returns>An object that can be used to synchronize access to the queue.  In the default implementation, this property always returns the current instance.</returns>
        ''' <remarks>
        ''' Note that all the methods of this class are already individually synchronized, however to safely enumerate through each queue element 
        ''' (i.e., to make sure list elements don't change during enumeration), derived classes and end users should perform their own
        ''' synchronization by implementing a SyncLock using this SyncRoot property
        ''' </remarks>
        Public Overridable ReadOnly Property SyncRoot() As Object Implements ICollection.SyncRoot
            Get
                Return m_processQueue
            End Get
        End Property

        ''' <summary>Gets a value indicating whether access to the queue is synchronized (thread safe).</summary>
        ''' <returns>true if access to the queue is synchronized (thread safe); otherwise, false.  In the default implementation, this property always returns true.</returns>
        ''' <remarks>This queue is effectively "synchronized" since all functions synclock operations internally</remarks>
        Public Overridable ReadOnly Property IsSynchronized() As Boolean Implements ICollection.IsSynchronized
            Get
                Return True
            End Get
        End Property

        Private Sub ICollectionCopyTo(ByVal array As System.Array, ByVal index As Integer) Implements ICollection.CopyTo

            CopyTo(array, index)

        End Sub

        Private ReadOnly Property ICollectionCount() As Integer Implements ICollection.Count
            Get
                Return Count
            End Get
        End Property

#End Region

    End Class

End Namespace