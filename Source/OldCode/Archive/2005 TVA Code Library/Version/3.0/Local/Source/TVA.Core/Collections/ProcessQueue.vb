'*******************************************************************************************************
'  TVA.Collections.ProcessQueue.vb - Multi-threaded Item Processing Queue
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/07/2006 - J. Ritchie Carroll
'       Generated original version of source code.
'  02/12/2006 - J. Ritchie Carroll
'       Added multi-item bulk processing functionality.
'  04/10/2006 - J. Ritchie Carroll
'       Added "DebugMode" property to disable "catch" so exceptions are debugged in originating source.
'  03/21/2007 - J. Ritchie Carroll
'       Added "ItemsBeingProcessed" property to return current total number of items being processed.
'       Added "Flush" method to allow any remaining items in queue to be processed before shutdown.
'  04/05/2007 - J. Ritchie Carroll
'       Added "RequeueMode" properties to allow users to specify how data gets reinserted back into
'       the list (prefix or suffix) after processing timeouts or exceptions.
'  07/12/2007 - Pinal C. Patel
'       Modified the code for "Flush" method to correctly implement IDisposable interface.
'  08/01/2007 - J. Ritchie Carroll
'       Added some minor optimizations where practical.
'  08/17/2007 - J. Ritchie Carroll
'       Removed IDisposable implementation because of continued flushing errors.
'  08/17/2007 - Darrell Zuercher
'       Edited code comments.
'  11/05/2007 - J. Ritchie Carroll
'       Modified flush to complete tasks on calling thread - this avoids errors when timer
'       gets disposed before flush call.
'
'*******************************************************************************************************

Imports System.Threading
Imports System.Text
Imports TVA.Common
Imports TVA.DateTime.Common

Namespace Collections

    ''' <summary>
    ''' <para>This class processes a collection of items on independent threads.</para>
    ''' <para>The consumer must implement a function to process items.</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly-typed collection of objects to be processed.</para>
    ''' <para>Consumers are expected to create new instances of this class through the static construction functions 
    ''' (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class ProcessQueue(Of T)

        Implements IList(Of T), ICollection

#Region " Public Member Declarations "

        ''' <summary>
        ''' Function signature that defines a method to process items one at a time.
        ''' </summary>
        ''' <param name="item">Item to be processed.</param>
        ''' <remarks>
        ''' <para>Required unless ProcessItemsFunction is implemented.</para>
        ''' <para>Creates an asynchronous queue to process individual items - one item at a time - on multiple threads.</para>
        ''' </remarks>
        Public Delegate Sub ProcessItemFunctionSignature(ByVal item As T)

        ''' <summary>
        ''' Function signature that defines a method to process multiple items at once.
        ''' </summary>
        ''' <param name="items">Items to be processed.</param>
        ''' <remarks>
        ''' <para>Required unless ProcessItemFunction is implemented.</para>
        ''' <para>Creates an asynchronous queue to process groups of items simultaneously on multiple threads.</para>
        ''' </remarks>
        Public Delegate Sub ProcessItemsFunctionSignature(ByVal items As T())

        ''' <summary>
        ''' Function signature that determines if an item can be currently processed.
        ''' </summary>
        ''' <param name="item">Item to be checked for processing availablity.</param>
        ''' <returns>True, if item can be processed. The default is true.</returns>
        ''' <remarks>
        ''' <para>Implementation of this function is optional. It is assumed that an item can be processed if this 
        ''' function is not defined</para>
        ''' <para>Items must eventually get to a state where they can be processed, or they will remain in the queue
        ''' indefinitely.</para>
        ''' <para>
        ''' Note that when this function is implemented and ProcessingStyle = ManyAtOnce (i.e., ProcessItemsFunction 
        ''' is defined), then each item presented for processing must evaluate as "CanProcessItem = True" before any 
        ''' items are processed.
        ''' </para>
        ''' </remarks>
        Public Delegate Function CanProcessItemFunctionSignature(ByVal item As T) As Boolean

        ''' <summary>
        ''' Event that is raised after an item has been successfully processed.
        ''' </summary>
        ''' <param name="item">Item that has been successfully processed.</param>
        ''' <remarks>
        ''' <para>Allows custom handling of successfully processed items.</para>
        ''' <para>Allows notification when an item has completed processing in the allowed amount of time, if a process 
        ''' timeout is specified.</para>
        ''' <para>Raised only when ProcessingStyle = OneAtATime (i.e., ProcessItemFunction is defined).</para>
        ''' </remarks>
        Public Event ItemProcessed(ByVal item As T)

        ''' <summary>
        ''' Event that is raised after an array of items have been successfully processed.
        ''' </summary>
        ''' <param name="items">Items that have been successfully processed.</param>
        ''' <remarks>
        ''' <para>Allows custom handling of successfully processed items.</para>
        ''' <para>Allows notification when an item has completed processing in the allowed amount of time, if a process 
        ''' timeout is specified.</para>
        ''' <para>Raised only when when ProcessingStyle = ManyAtOnce (i.e., ProcessItemsFunction is defined).</para>
        ''' </remarks>
        Public Event ItemsProcessed(ByVal items As T())

        ''' <summary>
        ''' Event that is raised if an item's processing time exceeds the specified process timeout.
        ''' </summary>
        ''' <param name="item">Item that took too long to process.</param>
        ''' <remarks>
        ''' <para>Allows custom handling of items that took too long to process.</para>
        ''' <para>Raised only when ProcessingStyle = OneAtATime (i.e., ProcessItemFunction is defined).</para>
        ''' </remarks>
        Public Event ItemTimedOut(ByVal item As T)

        ''' <summary>
        ''' Event that is raised if the processing time for an array of items exceeds the specified process timeout.
        ''' </summary>
        ''' <param name="items">Items that took too long to process</param>
        ''' <remarks>
        ''' <para>Allows custom handling of items that took too long to process.</para>
        ''' <para>Raised only when ProcessingStyle = ManyAtOnce (i.e., ProcessItemsFunction is defined).</para>
        ''' </remarks>
        Public Event ItemsTimedOut(ByVal items As T())

        ''' <summary>
        ''' Event that is raised if an exception is encountered while attempting to processing an item in the list.
        ''' </summary>
        ''' <remarks>
        ''' Processing will not stop for any exceptions thrown by the user function, but any captured exceptions will 
        ''' be exposed through this event.
        ''' </remarks>
        Public Event ProcessException(ByVal ex As Exception)

        ''' <summary>Default processing interval (in milliseconds).</summary>
        Public Const DefaultProcessInterval As Integer = 100

        ''' <summary>Default maximum number of processing threads.</summary>
        Public Const DefaultMaximumThreads As Integer = 5

        ''' <summary>Default processing timeout (in milliseconds).</summary>
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite

        ''' <summary>Default setting for requeuing items on processing timeout.</summary>
        Public Const DefaultRequeueOnTimeout As Boolean = False

        ''' <summary>Default setting for requeuing mode on processing timeout.</summary>
        Public Const DefaultRequeueModeOnTimeout As RequeueMode = RequeueMode.Prefix

        ''' <summary>Default setting for requeuing items on processing exceptions.</summary>
        Public Const DefaultRequeueOnException As Boolean = False

        ''' <summary>Default setting for requeuing mode on processing exceptions.</summary>
        Public Const DefaultRequeueModeOnException As RequeueMode = RequeueMode.Prefix

        ''' <summary>Default real-time processing interval (in milliseconds).</summary>
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
        Private m_requeueModeOnTimeout As RequeueMode
        Private m_requeueOnException As Boolean
        Private m_requeueModeOnException As RequeueMode
        Private m_processingIsRealTime As Boolean
        Private m_threadCount As Integer
        Private m_enabled As Boolean
        Private m_itemsProcessing As Long
        Private m_itemsProcessed As Long
        Private m_startTime As Long
        Private m_stopTime As Long
        Private m_debugMode As Boolean
        Private m_isDisposed As Boolean
        Private m_realTimeProcessThread As Thread
        Private m_realTimeProcessThreadPriority As ThreadPriority
        Private WithEvents m_processTimer As System.Timers.Timer

#End Region

#Region " Construction Functions "

#Region " Single-Item Processing Constructors "

        ''' <summary>
        ''' Creates a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, 
        ''' ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, 
        ''' ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous process queue with the default settings: ProcessInterval = 100, 
        ''' ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous process queue with the default settings: ProcessInterval = 100, 
        ''' ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous process queue using specified settings.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous process queue using  specified settings.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous process queue (i.e., single process thread) with the default settings: 
        ''' ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous process queue (i.e., single process thread) with the default settings: 
        ''' ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous process queue (i.e., single process thread) using specified settings.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemFunction, Nothing, processInterval, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous process queue (i.e., single process thread) using specified settings.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time process queue with the default settings: ProcessTimeout = Infinite, 
        ''' RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time process queue with the default settings: ProcessTimeout = Infinite, 
        ''' RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time process queue using specified settings.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemFunction, Nothing, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time process queue using specified settings.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

#End Region

#Region " Multi-Item Processing Constructors "

        ''' <summary>
        ''' Creates a new asynchronous, bulk item process queue with the default settings: ProcessInterval = 100, 
        ''' MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous, bulk item process queue with the default settings: ProcessInterval = 100, 
        ''' MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous, bulk item process queue with the default settings: ProcessInterval = 100, 
        ''' ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous, bulk item process queue with the default settings: ProcessInterval = 100, 
        ''' ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous, bulk item process queue using specified settings.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new asynchronous, bulk item process queue using specified settings.
        ''' </summary>
        Public Shared Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemsFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous, bulk item process queue (i.e., single process thread) with the default settings: 
        ''' ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous, bulk item process queue (i.e., single process thread) with the default settings: 
        ''' ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous, bulk item process queue (i.e., single process thread) using specified settings.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateSynchronousQueue(processItemsFunction, Nothing, processInterval, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new synchronous, bulk item process queue (i.e., single process thread) using specified settings.
        ''' </summary>
        Public Shared Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemsFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time, bulk item process queue with the default settings: ProcessTimeout = Infinite, 
        ''' RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemsFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time, bulk item process queue with the default settings: ProcessTimeout = Infinite, 
        ''' RequeueOnTimeout = False, RequeueOnException = False.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemsFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time, bulk item process queue using specified settings.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return CreateRealTimeQueue(processItemsFunction, Nothing, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Creates a new real-time, bulk item process queue using specified settings.
        ''' </summary>
        Public Shared Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As ProcessQueue(Of T)

            Return New ProcessQueue(Of T)(processItemsFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

#End Region

#Region " Protected Constructors "

        ''' <summary>
        ''' Creates a process queue based on the generic List(Of T) class.
        ''' </summary>
        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean)

            MyClass.New(processItemFunction, Nothing, canProcessItemFunction, New List(Of T), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Sub

        ''' <summary>
        ''' Creates a bulk item process queue based on the generic List(Of T) class.
        ''' </summary>
        Protected Sub New(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean)

            MyClass.New(Nothing, processItemsFunction, canProcessItemFunction, New List(Of T), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Sub

        ''' <summary>
        ''' Allows derived classes to define their own IList instance, if desired.
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
            m_requeueModeOnTimeout = DefaultRequeueModeOnTimeout
            m_requeueOnException = requeueOnException
            m_requeueModeOnException = DefaultRequeueModeOnException
            m_realTimeProcessThreadPriority = ThreadPriority.Highest

            If processInterval = RealTimeProcessInterval Then
                ' Instantiates process queue for real-time item processing
                m_processingIsRealTime = True
                m_maximumThreads = 1
            Else
                ' Instantiates process queue for intervaled item processing
                m_processTimer = New System.Timers.Timer
                m_processTimer.Interval = processInterval
                m_processTimer.AutoReset = True
                m_processTimer.Enabled = False
            End If

        End Sub

#End Region

#End Region

#Region " Public Methods Implementation "

        ''' <summary>
        ''' Gets or sets the user function for processing individual items in the list one at a time.
        ''' </summary>
        ''' <remarks>
        ''' <para>Cannot be defined simultaneously with ProcessItemsFunction.</para>
        ''' <para>A queue must be defined to process a single item at a time or many items at once.</para>
        ''' <para>Implementation makes ProcessingStyle = OneAtATime.</para>
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
        ''' Gets or sets the user function for processing multiple items in the list at once.
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
        ''' Gets or sets the user function determining if an item is ready to be processed.
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
        ''' Gets indicator that items will be processed in real-time.
        ''' </summary>
        Public Overridable ReadOnly Property ProcessingIsRealTime() As Boolean
            Get
                Return m_processingIsRealTime
            End Get
        End Property

        ''' <summary>
        ''' Gets the current threading mode for the process queue (i.e., synchronous or asynchronous).
        ''' </summary>
        ''' <remarks>
        ''' <para>The maximum number of processing threads determines the threading mode.</para>
        ''' <para>If the maximum threads are set to one, item processing will be synchronous 
        ''' (i.e., ThreadingMode = Synchronous).</para>
        ''' <para>If the maximum threads are more than one, item processing will be asynchronous 
        ''' (i.e., ThreadingMode = Asynchronous).</para>
        ''' <para>
        ''' Note that for asynchronous queues, the processing interval will control how many threads are spawned
        ''' at once. If items are processed faster than the specified processing interval, only one process thread
        ''' will ever be spawned at a time. To ensure multiple threads are utilized to process queue items, lower
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
        ''' Gets the item processing style for the process queue (i.e., one at a time or many at once).
        ''' </summary>
        ''' <returns>
        ''' <para>OneAtATime, if the ProcessItemFunction is implemented.</para> 
        ''' <para>ManyAtOnce, if the ProcessItemsFunction is implemented.</para>
        ''' </returns>
        ''' <remarks>
        ''' <para>The implemented item processing function determines the processing style.</para>
        ''' <para>
        ''' If the processing style is ManyAtOnce, all available items in the queue are presented for processing
        ''' at each processing interval. If you expect items to be processed in the order in which they were received, make
        ''' sure you use a synchronous queue. Real-time queues are inherently synchronous.
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
        ''' Gets or sets the interval, in milliseconds, on which new items begin processing.
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
        ''' Gets or sets the maximum number of threads to process simultaneously.
        ''' </summary>
        ''' <value>Sets the maximum number of processing threads.</value>
        ''' <returns>Maximum number of processing threads.</returns>
        ''' <remarks>If MaximumThreads is set to one, item processing will be synchronous (i.e., ThreadingMode = Synchronous)</remarks>
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
        ''' Gets or sets the maximum time, in milliseconds, allowed for processing an item.
        ''' </summary>
        ''' <value>Sets the maximum number of milliseconds allowed to process an item.</value>
        ''' <returns>Gets the maximum number of milliseconds allowed to process an item.</returns>
        ''' <remarks>Set to Timeout.Infinite (i.e., -1) to allow processing to take as long as needed.</remarks>
        Public Overridable Property ProcessTimeout() As Integer
            Get
                Return m_processTimeout
            End Get
            Set(ByVal value As Integer)
                m_processTimeout = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets whether or not to automatically place an item back into the list if the processing times out.
        ''' </summary>
        ''' <remarks>Ignored if the ProcessTimeout is set to Timeout.Infinite (i.e., -1).</remarks>
        Public Overridable Property RequeueOnTimeout() As Boolean
            Get
                Return m_requeueOnTimeout
            End Get
            Set(ByVal value As Boolean)
                m_requeueOnTimeout = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the mode of insertion used (prefix or suffix) when at item is placed back into the list 
        ''' after processing times out.
        ''' </summary>
        ''' <remarks>Only relevant when RequeueOnTimeout = True.</remarks>
        Public Overridable Property RequeueModeOnTimeout() As RequeueMode
            Get
                Return m_requeueModeOnTimeout
            End Get
            Set(ByVal value As RequeueMode)
                m_requeueModeOnTimeout = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets whether or not to automatically place an item back into the list if an exception occurs 
        ''' while processing.
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
        ''' Gets or sets the mode of insertion used (prefix or suffix) when at item is placed back into the 
        ''' list after an exception occurs while processing.
        ''' </summary>
        ''' <remarks>Only relevant when RequeueOnException = True.</remarks>
        Public Overridable Property RequeueModeOnException() As RequeueMode
            Get
                Return m_requeueModeOnException
            End Get
            Set(ByVal value As RequeueMode)
                m_requeueModeOnException = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets debug mode for the process queue when handling exceptions.
        ''' </summary>
        ''' <value>True to enable debug mode.</value>
        ''' <returns>True if debug mode is enabled. Otherwise, False.</returns>
        ''' <remarks>
        ''' When debug mode is True, all internal "Catch ex As Exception" statements will be ignored, allowing the 
        ''' development environment to stop directly on the line of code that threw the exception (e.g., in user's 
        ''' process item function).
        ''' </remarks>
        Public Overridable Property DebugMode() As Boolean
            Get
                Return m_debugMode
            End Get
            Set(ByVal value As Boolean)
                m_debugMode = value
            End Set
        End Property

        ''' <summary>
        ''' Starts item processing.
        ''' </summary>
        Public Overridable Sub Start()

            m_enabled = True
            m_threadCount = 0
            m_itemsProcessed = 0
            m_stopTime = 0
            m_startTime = Date.Now.Ticks

            ' Note that real-time queues have their main thread running continually, but for
            ' intervaled queues, processing occurs only when data is available to be processed.
            If m_processingIsRealTime Then
                ' Start real-time processing thread
                m_realTimeProcessThread = New Thread(AddressOf RealTimeThreadProc)
                m_realTimeProcessThread.Priority = m_realTimeProcessThreadPriority
                m_realTimeProcessThread.Start()
            Else
                ' Start intervaled processing, if there items in the queue
                m_processTimer.Enabled = (Count > 0)
            End If

        End Sub

        ''' <summary>
        ''' Stops item processing.
        ''' </summary>
        Public Overridable Sub [Stop]()

            m_enabled = False

            If m_processingIsRealTime Then
                ' Stops real-time processing thread.
                If m_realTimeProcessThread IsNot Nothing Then m_realTimeProcessThread.Abort()
                m_realTimeProcessThread = Nothing
            Else
                ' Stops intervaled processing, if active.
                m_processTimer.Enabled = False
            End If

            m_stopTime = Date.Now.Ticks

        End Sub

        ''' <summary>
        ''' Gets or sets indicator that the list is currently enabled.
        ''' </summary>
        Public Overridable Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Protected Set(ByVal value As Boolean)
                m_enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Blocks the current thread, if the queue is active (i.e., user has called "Start" method), until all items 
        ''' in process queue are processed, and then stops the queue.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Begins processing items as quickly as possible, regardless of currently defined process interval, until all 
        ''' items in the queue have been processed. Stops the queue when this function ends. This method is typically 
        ''' called on shutdown to make sure any remaining queued items get processed before the process queue is destructed.
        ''' </para>
        ''' <para>
        ''' It is possible for items to be added to the queue while the flush is executing. The flush will continue to 
        ''' process items as quickly as possible until the queue is empty. Unless the user stops queueing items to be 
        ''' processed, the flush call may never return (not a happy situtation on shutdown). For this reason, during this 
        ''' function call, requeueing of items on exception or process timeout is temporarily disabled.
        ''' </para>
        ''' <para>
        ''' The process queue does not implement a finalizer. If the user fails to call this method before the class
        ''' is destructed, there may be items that remain unprocessed in the queue.
        ''' </para>
        ''' </remarks>
        Public Sub Flush()

            Dim enabled As Boolean = m_enabled

            ' Stop all queue processing...
            [Stop]()

            If enabled Then
                Dim originalRequeueOnTimeout As Boolean = m_requeueOnTimeout
                Dim originalRequeueOnException As Boolean = m_requeueOnException

                ' We must disable requeueing of items or this method could continue indefinitely.
                m_requeueOnTimeout = False
                m_requeueOnException = False

                ' Only waits around if there is something to process.
                Do While Count > 0
                    ' Create a real-time processing loop that will process remaining items as quickly as possible.
                    SyncLock m_processQueue
                        Do While m_processQueue.Count > 0
                            If m_processItemsFunction Is Nothing Then
                                ' Processes one item at a time.
                                ProcessNextItem()
                            Else
                                ' Processes multiple items at once.
                                ProcessNextItems()
                            End If
                        Loop
                    End SyncLock
                Loop

                ' Just in case user continues to use queue after flush, this restores original states.
                m_requeueOnTimeout = originalRequeueOnTimeout
                m_requeueOnException = originalRequeueOnException
            End If

        End Sub

        ''' <summary>
        ''' Gets indicator that the list is actively processing items.
        ''' </summary>
        Public ReadOnly Property Processing() As Boolean
            Get
                If m_processingIsRealTime Then
                    Return (m_realTimeProcessThread IsNot Nothing)
                Else
                    Return m_processTimer.Enabled
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets the total number of items currently being processed.
        ''' </summary>
        ''' <returns>Total number of items currently being processed.</returns>
        Public ReadOnly Property ItemsBeingProcessed() As Long
            Get
                Return m_itemsProcessing
            End Get
        End Property

        ''' <summary>
        ''' Gets the total number of items processed so far.
        ''' </summary>
        ''' <returns>Total number of items processed so far.</returns>
        Public ReadOnly Property TotalProcessedItems() As Long
            Get
                Return m_itemsProcessed
            End Get
        End Property

        ''' <summary>
        ''' Gets the current number of active threads.
        ''' </summary>
        ''' <returns>Current number of active threads.</returns>
        Public ReadOnly Property ThreadCount() As Integer
            Get
                Return m_threadCount
            End Get
        End Property

        ''' <summary>
        ''' Gets the total amount of time, in seconds, that the process list has been active.
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
        ''' Gets or sets adjustment of real-time process thread priority.
        ''' </summary>
        ''' <remarks>
        ''' <para>Only affects real-time queues.</para>
        ''' <para>Only takes effect when set before calling the "Start" method.</para> 
        ''' </remarks>
        Public Property RealTimeProcessThreadPriority() As ThreadPriority
            Get
                Return m_realTimeProcessThreadPriority
            End Get
            Set(ByVal value As ThreadPriority)
                m_realTimeProcessThreadPriority = value
            End Set
        End Property

        ''' <summary>
        ''' Gets class name.
        ''' </summary>
        ''' <remarks>
        ''' <para>This name is used for class identification in strings (e.g., used in error message).</para>
        ''' <para>Derived classes can override this method, if needed, with a proper class name - defaults to 
        ''' Me.GetType().Name.</para>
        ''' </remarks>
        Public Overridable ReadOnly Property Name() As String
            Get
                Return Me.GetType().Name
            End Get
        End Property

        ''' <summary>
        ''' Gets current status of processing queue.
        ''' </summary>
        Public Overridable ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("       Queue processing is: ")
                    If m_enabled Then
                        .Append("Enabled")
                    Else
                        .Append("Disabled")
                    End If
                    .AppendLine()
                    .Append("  Current processing state: ")
                    If Processing Then
                        .Append("Executing")
                    Else
                        .Append("Idle")
                    End If
                    .AppendLine()
                    .Append("       Processing interval: ")
                    If m_processingIsRealTime Then
                        .Append("Real-time")
                    Else
                        .Append(ProcessInterval)
                        .Append(" milliseconds")
                    End If
                    .AppendLine()
                    .Append("        Processing timeout: ")
                    If m_processTimeout = Timeout.Infinite Then
                        .Append("Infinite")
                    Else
                        .Append(m_processTimeout)
                        .Append(" milliseconds")
                    End If
                    .AppendLine()
                    .Append("      Queue threading mode: ")
                    If ThreadingMode = QueueThreadingMode.Asynchronous Then
                        .Append("Asynchronous - ")
                        .Append(m_maximumThreads)
                        .Append(" maximum threads")
                    Else
                        .Append("Synchronous")
                    End If
                    .AppendLine()
                    .Append("    Queue processing style: ")
                    If ProcessingStyle = QueueProcessingStyle.OneAtATime Then
                        .Append("One at a time")
                    Else
                        .Append("Many at once")
                    End If
                    .AppendLine()
                    .Append("    Total process run time: ")
                    .Append(SecondsToText(RunTime))
                    .AppendLine()
                    .Append("      Total active threads: ")
                    .Append(m_threadCount)
                    .AppendLine()
                    .Append("   Queued items to process: ")
                    .Append(Count)
                    .AppendLine()
                    .Append("     Items being processed: ")
                    .Append(m_itemsProcessing)
                    .AppendLine()
                    .Append("     Total items processed: ")
                    .Append(m_itemsProcessed)
                    .AppendLine()

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

#Region " Protected Methods Implementation "

        ''' <summary>
        ''' Allows derived classes to access the interfaced internal process queue directly.
        ''' </summary>
        Protected ReadOnly Property InternalList() As IList(Of T)
            Get
                Return m_processQueue
            End Get
        End Property

        ''' <summary>
        ''' Notifies a class that data was added, so it can begin processing data.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Derived classes *must* make sure to call this method after data gets added, so that the
        ''' process timer can be enabled for intervaled queues and data processing can begin.
        ''' </para>
        ''' <para>
        ''' To make sure items in the queue always get processed, this function is expected to be
        ''' invoked from within a SyncLock of the exposed SyncRoot (i.e., m_processQueue).
        ''' </para>
        ''' </remarks>
        Protected Sub DataAdded()

            ' For queues that are not processing in real-time, we start the intervaled process timer
            ' when data is added, if it's not running already
            If Not m_processingIsRealTime AndAlso Not m_processTimer.Enabled Then m_processTimer.Enabled = m_enabled

        End Sub

        ''' <summary>
        ''' Determines if an item can be processed.
        ''' </summary>
        ''' <values>True, if user provided no implementation for the CanProcessItemFunction.</values>
        ''' <remarks>
        ''' <para>
        ''' Use this function instead of invoking the CanProcessItemFunction pointer
        ''' directly, since implementation of this delegate is optional.
        ''' </para>
        ''' </remarks>
        Protected Overridable Function CanProcessItem(ByVal item As T) As Boolean

            If m_canProcessItemFunction Is Nothing Then
                ' If user provided no implementation for this function or function failed, we assume item can be processed.
                Return True
            Else
                Try
                    ' When user function is provided, we call it to determine if item should be processed at this time.
                    Return m_canProcessItemFunction(item)
                Catch ex As ThreadAbortException
                    ' Rethrow thread abort so calling method can respond appropriately
                    Throw ex
                Catch ex As Exception When Not m_debugMode
                    ' Processing will not stop for any errors thrown by the user function, but errors will be reported.
                    RaiseEvent ProcessException(ex)
                End Try
            End If

        End Function

        ''' <summary>
        ''' Determines if all items can be processed.
        ''' </summary>
        ''' <values>True, if user provided no implementation for the CanProcessItemFunction.</values>
        ''' <remarks>
        ''' <para>
        ''' Use this function instead of invoking the CanProcessItemFunction pointer
        ''' directly, since implementation of this delegate is optional.
        ''' </para>
        ''' </remarks>
        Protected Overridable Function CanProcessItems(ByVal items As T()) As Boolean

            If m_canProcessItemFunction Is Nothing Then
                ' If user provided no implementation for this function or function failed, we assume item can be processed.
                Return True
            Else
                ' Otherwise we call user function for each item to determine if all items are ready for processing.
                Dim allItemsCanBeProcessed As Boolean = True

                For Each item As T In items
                    If Not CanProcessItem(item) Then
                        allItemsCanBeProcessed = False
                        Exit For
                    End If
                Next

                Return allItemsCanBeProcessed
            End If

        End Function

        ''' <summary>
        ''' Requeues item into list according to specified requeue mode.
        ''' </summary>
        Protected Overridable Sub RequeueItem(ByVal item As T, ByVal mode As RequeueMode)

            If mode = RequeueMode.Prefix Then
                Insert(0, item)
            Else
                Add(item)
            End If

        End Sub

        ''' <summary>
        ''' Requeues items into list according to specified requeue mode.
        ''' </summary>
        Protected Overridable Sub RequeueItems(ByVal items As T(), ByVal mode As RequeueMode)

            If mode = RequeueMode.Prefix Then
                InsertRange(0, items)
            Else
                AddRange(items)
            End If

        End Sub

        ''' <summary>
        ''' Raises the base class ItemProcessed event.
        ''' </summary>
        ''' <remarks>
        ''' Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate 
        ''' as needed.
        ''' </remarks>
        Protected Sub RaiseItemProcessed(ByVal item As T)

            RaiseEvent ItemProcessed(item)

        End Sub

        ''' <summary>
        ''' Raises the base class ItemsProcessed event.
        ''' </summary>
        ''' <remarks>
        ''' Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate 
        ''' as needed.
        ''' </remarks>
        Protected Sub RaiseItemsProcessed(ByVal items As T())

            RaiseEvent ItemsProcessed(items)

        End Sub

        ''' <summary>
        ''' Raises the base class ItemTimedOut event.
        ''' </summary>
        ''' <remarks>
        ''' Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate 
        ''' as needed.
        ''' </remarks>
        Protected Sub RaiseItemTimedOut(ByVal item As T)

            RaiseEvent ItemTimedOut(item)

        End Sub

        ''' <summary>
        ''' Raises the base class ItemsTimedOut event.
        ''' </summary>
        ''' <remarks>
        ''' Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate 
        ''' as needed.
        ''' </remarks>
        Protected Sub RaiseItemsTimedOut(ByVal items As T())

            RaiseEvent ItemsTimedOut(items)

        End Sub

        ''' <summary>
        ''' Raises the base class ProcessException event.
        ''' </summary>
        ''' <remarks>
        ''' Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate 
        ''' as needed.
        ''' </remarks>
        Protected Sub RaiseProcessException(ByVal ex As Exception)

            RaiseEvent ProcessException(ex)

        End Sub

#End Region

#Region " Private Methods Implementation "

#Region " Internal Thread Processing Class "

        ' Limits item processing time, if requested.
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

            ' Blocks calling thread until specified time has expired.
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

        ' Handles standard processing of a single item.
        Private Sub ProcessItem(ByVal item As T)

            Try
                ' Invokes user function to process item.
                m_processItemFunction(item)
                Interlocked.Increment(m_itemsProcessed)

                ' Notifies consumers of successfully processed items.
                RaiseEvent ItemProcessed(item)
            Catch ex As ThreadAbortException
                ' Rethrows thread abort, so calling method can respond appropriately.
                Throw ex
            Catch ex As Exception When Not m_debugMode
                ' Requeues item on processing exception, if requested.
                If m_requeueOnException Then RequeueItem(item, m_requeueModeOnException)

                ' Processing will not stop for any errors thrown by the user function, but errors will be reported.
                RaiseEvent ProcessException(ex)
            End Try

        End Sub

        ' Handles standard processing of multiple items.
        Private Sub ProcessItems(ByVal items As T())

            Try
                ' Invokes user function to process items.
                m_processItemsFunction(items)
                Interlocked.Add(m_itemsProcessed, CLng(items.Length))

                ' Notifies consumers of successfully processed items.
                RaiseEvent ItemsProcessed(items)
            Catch ex As ThreadAbortException
                ' Rethrows thread abort, so calling method can respond appropriately.
                Throw ex
            Catch ex As Exception When Not m_debugMode
                ' Requeues items on processing exception, if requested.
                If m_requeueOnException Then RequeueItems(items, m_requeueModeOnException)

                ' Processing will not stop for any errors thrown by the user function, but errors will be reported.
                RaiseEvent ProcessException(ex)
            End Try

        End Sub

        ' Creates a real-time thread for processing items.
        Private Sub RealTimeThreadProc()

            ' Creates a real-time processing loop that will process items as quickly as possible.
            Do While True
                If m_processItemsFunction Is Nothing Then
                    ' Processes one item at a time.
                    ProcessNextItem()
                Else
                    ' Processes multiple items at once.
                    ProcessNextItems()
                End If

                ' Sleeps the thread between each loop to help minimize CPU loading.
                Thread.Sleep(1)
            Loop

        End Sub

        ' Processes queued items on an interval.
        Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed

            ' The system timer creates an intervaled processing loop that distributes item processing across multiple threads if needed
            If m_processItemsFunction Is Nothing Then
                ' Processes one item at a time.
                ProcessNextItem()
            Else
                ' Processes multiple items at once.
                ProcessNextItems()
            End If

            SyncLock m_processQueue
                ' Stops the process timer if there is no more data to process.
                If m_processQueue.Count = 0 Then m_processTimer.Enabled = False
            End SyncLock

        End Sub

        ' Processes next item in queue, one at a time (i.e., ProcessingStyle = OneAtATime).
        Private Sub ProcessNextItem()

            Dim nextItem As T
            Dim processingItem As Boolean

            Try
                ' Handles all queue operations for getting next item in a single synchronous operation.
                ' We keep work to be done here down to a mimimum amount of time
                SyncLock m_processQueue
                    ' Retrieves the next item to be processed if the number of current process threads is less
                    ' than the maximum allowable number of process threads.
                    If m_processQueue.Count > 0 AndAlso m_threadCount < m_maximumThreads Then
                        ' Retrieves first item to be processed.
                        nextItem = m_processQueue(0)

                        ' Calls optional user function to see if we should process this item.
                        If CanProcessItem(nextItem) Then
                            Interlocked.Increment(m_threadCount)

                            ' Removes the item about to be processed from the queue.
                            m_processQueue.RemoveAt(0)

                            processingItem = True
                            Interlocked.Increment(m_itemsProcessing)
                        End If
                    End If
                End SyncLock

                If processingItem Then
                    If m_processTimeout = Timeout.Infinite Then
                        ' If an item is in the queue to process, and the process queue was not set up with a process 
                        ' timeout, we use the current thread (i.e., the timer event or real-time thread) to process the 
                        ' next item taking as long as we need for it to complete. For timer events, the next item in 
                        ' the queue will begin processing even if this item is not completed, but no more than the 
                        ' specified number of maximum threads will ever be spawned at once.
                        ProcessItem(nextItem)
                    Else
                        ' If an item is in the queue to process, with a specified process timeout, a new thread is 
                        ' created to handle the processing. The timer event or real-time thread that invoked this method 
                        ' is already a new thread, so the only reason to create another thread is to implement the 
                        ' process timeout if the process takes too long to run. This is done by joining the current 
                        ' thread (which will block it) until the specified interval has passed or the process thread 
                        ' completes, whichever comes first. This is a safe operation since the current thread 
                        ' (i.e., the timer event or real-time thread) was already an independent thread and will not 
                        ' block any other processing, including another timer event.
                        With New ProcessThread(Me, nextItem)
                            If Not .WaitUntil(m_processTimeout) Then
                                ' Notifies user of process timeout, in case they want to do anything special.
                                RaiseEvent ItemTimedOut(nextItem)

                                ' Requeues item on processing timeout, if requested.
                                If m_requeueOnTimeout Then RequeueItem(nextItem, m_requeueModeOnTimeout)
                            End If
                        End With
                    End If
                End If
            Catch ex As ThreadAbortException
                ' Rethrows thread abort, so calling method can respond appropriately.
                Throw ex
            Catch ex As Exception When Not m_debugMode
                ' Processing will not stop for any errors encountered here, but errors will be reported.
                RaiseEvent ProcessException(ex)
            Finally
                ' Decrements thread count, if item was retrieved for processing.
                If processingItem Then
                    Interlocked.Decrement(m_threadCount)
                    Interlocked.Decrement(m_itemsProcessing)
                End If
            End Try

        End Sub

        ' Processes next items in an array of items as a group (i.e., ProcessingStyle = ManyAtOnce).
        Private Sub ProcessNextItems()

            Dim nextItems As T()
            Dim processingItems As Boolean

            Try
                ' Handles all queue operations for getting next items in a single synchronous operation.
                ' We keep work to be done here down to a mimimum amount of time.
                SyncLock m_processQueue
                    ' Gets next items to be processed, if the number of current process threads is less
                    ' than the maximum allowable number of process threads.
                    If m_processQueue.Count > 0 AndAlso m_threadCount < m_maximumThreads Then
                        ' Retrieves items to be processed.
                        nextItems = ToArray()

                        ' Calls optional user function to see if these items should be processed.
                        If CanProcessItems(nextItems) Then
                            Interlocked.Increment(m_threadCount)

                            ' Clears all items from the queue
                            m_processQueue.Clear()

                            processingItems = True
                            Interlocked.Add(m_itemsProcessing, CLng(nextItems.Length))
                        End If
                    End If
                End SyncLock

                If processingItems Then
                    If m_processTimeout = Timeout.Infinite Then
                        ' If items are in the queue to process, and the process queue was not set up with a process 
                        ' timeout, the current thread (i.e., the timer event or real-time thread) is used to process the 
                        ' next items taking as long as necessary to complete. For timer events, any new items available 
                        ' in the queue will be processed, even if the current items have not completed, but no more than 
                        ' the specified number of maximum threads will ever be spawned at once.
                        ProcessItems(nextItems)
                    Else
                        ' If items are in the queue to process, and a process timeout was specified, a new thread is 
                        ' created to handle the processing. The timer event or real-time thread that invoked this method 
                        ' is already a new thread, so the only reason to create another thread is to implement the 
                        ' process timeout if the process takes too long to run. We do this by joining the current thread 
                        ' (which will block it) until the specified interval has passed or the process thread completes, 
                        ' whichever comes first. This is a safe operation, since the current thread (i.e., the timer 
                        ' event or real-time thread) was already an independent thread and will not block any other 
                        ' processing, including another timer event.
                        With New ProcessThread(Me, nextItems)
                            If Not .WaitUntil(m_processTimeout) Then
                                ' Notifies the user of the process timeout, in case they want to do anything special.
                                RaiseEvent ItemsTimedOut(nextItems)

                                ' Requeues items on processing timeout, if requested.
                                If m_requeueOnTimeout Then RequeueItems(nextItems, m_requeueModeOnTimeout)
                            End If
                        End With
                    End If
                End If
            Catch ex As ThreadAbortException
                ' Rethrows thread abort, so calling method can respond appropriately.
                Throw ex
            Catch ex As Exception When Not m_debugMode
                ' Processing will not stop for any errors encountered here, but errors will be reported.
                RaiseEvent ProcessException(ex)
            Finally
                ' Decrements thread count, if items were retrieved for processing.
                If processingItems Then
                    Interlocked.Decrement(m_threadCount)
                    Interlocked.Add(m_itemsProcessing, CLng(-nextItems.Length))
                End If
            End Try

        End Sub

#End Region

#End Region

#Region " Handy List(Of T) Functions Implementation "

        ' The internal list is declared as an IList(Of T). Derived classes (e.g., KeyedProcessQueue) can use their own 
        ' list implementation for process functionality. However, the regular List(Of T) provides many handy functions 
        ' that are not required to be exposed by the IList(Of T) interface. So, if the implemented list is a List(Of T), 
        ' we'll expose this native functionality; otherwise, we implement it for you.

        ' Note: All List(Of T) implementations should be synchronized, as necessary.

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
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature if process queue is not a List(Of T).
                    If collection Is Nothing Then Throw New ArgumentNullException("collection", "collection is null")

                    For Each item As T In collection
                        m_processQueue.Add(item)
                    Next
                Else
                    ' Otherwise, we'll call native implementation.
                    processQueue.AddRange(collection)
                End If

                DataAdded()
            End SyncLock

        End Sub

        '''	<summary>
        ''' Searches the entire sorted queue, using a binary search algorithm, for an element using the
        ''' default comparer and returns the zero-based index of the element.
        ''' </summary>
        ''' <remarks>
        ''' Queue must be sorted in order for this function to return an accurate result.
        ''' </remarks>
        '''	<param name="item">The object to locate. The value can be null for reference types.</param>
        ''' <returns>
        ''' The zero-based index of item in the sorted queue, if item is found; otherwise, a negative number that is the
        ''' bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        ''' the bitwise complement of count.
        ''' </returns>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an 
        ''' implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Function BinarySearch(ByVal item As T) As Integer

            Return BinarySearch(0, m_processQueue.Count, item, Nothing)

        End Function

        '''	<summary>
        ''' Searches the entire sorted queue, using a binary search algorithm, for an element using the
        ''' specified comparer and returns the zero-based index of the element.
        ''' </summary>
        ''' <remarks>
        ''' Queue must be sorted in order for this function to return an accurate result.
        ''' </remarks>
        '''	<param name="item">The object to locate. The value can be null for reference types.</param>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- 
        ''' null to use the default comparer: Generic.Comparer(Of T).Default</param>
        ''' <returns>
        ''' The zero-based index of item in the sorted queue, if item is found; otherwise, a negative number that is the
        ''' bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        ''' the bitwise complement of count.
        ''' </returns>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an 
        ''' implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Function BinarySearch(ByVal item As T, ByVal comparer As IComparer(Of T)) As Integer

            Return BinarySearch(0, m_processQueue.Count, item, comparer)

        End Function

        '''	<summary>
        ''' Searches a range of elements in the sorted queue, using a binary search algorithm, for an
        ''' element using the specified comparer and returns the zero-based index of the element.
        ''' </summary>
        ''' <remarks>
        ''' Queue must be sorted in order for this function to return an accurate result.
        ''' </remarks>
        ''' <param name="index">The zero-based starting index of the range to search.</param>
        ''' <param name="count">The length of the range to search.</param>
        '''	<param name="item">The object to locate. The value can be null for reference types.</param>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- null to use 
        ''' the default comparer: Generic.Comparer(Of T).Default</param>
        ''' <returns>
        ''' The zero-based index of item in the sorted queue, if item is found; otherwise, a negative number that is the
        ''' bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        ''' the bitwise complement of count.
        ''' </returns>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue 
        ''' -or- count is less than 0 -or- startIndex and count do not specify a valid section in the queue</exception>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an 
        ''' implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Function BinarySearch(ByVal index As Integer, ByVal count As Integer, ByVal item As T, ByVal comparer As IComparer(Of T)) As Integer

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    Dim foundIndex As Integer = -1
                    Dim startIndex As Integer = index
                    Dim stopIndex As Integer = index + count - 1
                    Dim currentIndex As Integer
                    Dim result As Integer

                    ' Validates start and stop index.
                    If startIndex < 0 OrElse count < 0 OrElse stopIndex > m_processQueue.Count - 1 Then Throw New ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue")
                    If comparer Is Nothing Then comparer = Generic.Comparer(Of T).Default

                    If count > 0 Then
                        Do While True
                            ' Finds next mid point.
                            currentIndex = startIndex + (stopIndex - startIndex) \ 2

                            ' Compares item at mid-point
                            result = comparer.Compare(item, m_processQueue(currentIndex))

                            If result = 0 Then
                                ' For a found item, returns located index.
                                foundIndex = currentIndex
                                Exit Do
                            ElseIf startIndex = stopIndex Then
                                ' Met in the middle and didn't find match, so we are finished,
                                foundIndex = startIndex Xor -1
                                Exit Do
                            ElseIf result > 0 Then
                                If currentIndex < count - 1 Then
                                    ' Item is beyond current item, so we start search at next item.
                                    startIndex = currentIndex + 1
                                Else
                                    ' Looked to the end and did not find match, so we are finished.
                                    foundIndex = (count - 1) Xor -1
                                    Exit Do
                                End If
                            Else
                                If currentIndex > 0 Then
                                    ' Item is before current item, so we will stop search at current item.
                                    ' Note that because of the way the math works, you do not stop at the
                                    ' prior item, as you might guess. It can cause you to skip an item.
                                    stopIndex = currentIndex
                                Else
                                    ' Looked to the top and did not find match, so we are finished.
                                    foundIndex = 0 Xor -1
                                    Exit Do
                                End If
                            End If
                        Loop
                    End If

                    Return foundIndex
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.BinarySearch(index, count, item, comparer)
                End If
            End SyncLock

        End Function

        ''' <summary>Converts the elements in the current queue to another type, and returns a list containing the 
        ''' converted elements.</summary>
        ''' <returns>A generic list of the target type containing the converted elements from the current queue.</returns>
        ''' <param name="converter">A Converter delegate that converts each element from one type to another type.</param>
        ''' <exception cref="ArgumentNullException">converter is null.</exception>
        Public Overridable Function ConvertAll(Of TOutput)(ByVal converter As Converter(Of T, TOutput)) As List(Of TOutput)

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If converter Is Nothing Then Throw New ArgumentNullException("converter", "converter is null")

                    Dim result As New List(Of TOutput)

                    For Each item As T In m_processQueue
                        result.Add(converter(item))
                    Next

                    Return result
                Else
                    ' Otherwise, we will call native implementation
                    Return processQueue.ConvertAll(converter)
                End If
            End SyncLock

        End Function

        ''' <summary>Determines whether the queue contains elements that match the conditions defined by the specified 
        ''' predicate.</summary>
        ''' <returns>True, if the queue contains one or more elements that match the conditions defined by the specified 
        ''' predicate; otherwise, false.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function Exists(ByVal match As Predicate(Of T)) As Boolean

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim found As Boolean

                    For x As Integer = 0 To m_processQueue.Count - 1
                        If match(m_processQueue(x)) Then
                            found = True
                            Exit For
                        End If
                    Next

                    Return found
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.Exists(match)
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns 
        ''' the first occurrence within the entire queue.</summary>
        ''' <returns>The first element that matches the conditions defined by the specified predicate, if found; 
        ''' otherwise, the default value for type T.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function Find(ByVal match As Predicate(Of T)) As T

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim foundItem As T
                    Dim foundIndex As Integer = FindIndex(match)

                    If foundIndex >= 0 Then foundItem = m_processQueue(foundIndex)

                    Return foundItem
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.Find(match)
                End If
            End SyncLock

        End Function

        ''' <summary>Retrieves all elements that match the conditions defined by the specified predicate.</summary>
        ''' <returns>A generic list containing all elements that match the conditions defined by the specified predicate, 
        ''' if found; otherwise, an empty list.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindAll(ByVal match As Predicate(Of T)) As List(Of T)

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim foundItems As New List(Of T)

                    For Each item As T In m_processQueue
                        If match(item) Then foundItems.Add(item)
                    Next

                    Return foundItems
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.FindAll(match)
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns 
        ''' the zero-based index of the first occurrence within the range of elements in the queue that extends from the 
        ''' specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by 
        ''' match, if found; otherwise, –1.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindIndex(ByVal match As Predicate(Of T)) As Integer

            Return FindIndex(0, m_processQueue.Count, match)

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns 
        ''' the zero-based index of the first occurrence within the range of elements in the queue that extends from the 
        ''' specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by 
        ''' match, if found; otherwise, –1.</returns>
        ''' <param name="startIndex">The zero-based starting index of the search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue.</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindIndex(ByVal startIndex As Integer, ByVal match As Predicate(Of T)) As Integer

            Return FindIndex(startIndex, m_processQueue.Count, match)

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns 
        ''' the zero-based index of the first occurrence within the range of elements in the queue that extends from the 
        ''' specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by 
        ''' match, if found; otherwise, –1.</returns>
        ''' <param name="startIndex">The zero-based starting index of the search.</param>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue 
        ''' -or- count is less than 0 -or- startIndex and count do not specify a valid section in the queue.</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindIndex(ByVal startIndex As Integer, ByVal count As Integer, ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If startIndex < 0 OrElse count < 0 OrElse startIndex + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue")
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim foundindex As Integer = -1

                    For x As Integer = startIndex To startIndex + count - 1
                        If match(m_processQueue(x)) Then
                            foundindex = x
                            Exit For
                        End If
                    Next

                    Return foundindex
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.FindIndex(startIndex, count, match)
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire queue.</summary>
        ''' <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLast(ByVal match As Predicate(Of T)) As T

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature if process queue is not a List(Of T)
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim foundItem As T
                    Dim foundIndex As Integer = FindLastIndex(match)

                    If foundIndex >= 0 Then foundItem = m_processQueue(foundIndex)

                    Return foundItem
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.FindLast(match)
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns 
        ''' the zero-based index of the last occurrence within the entire queue.</summary>
        ''' <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by 
        ''' match, if found; otherwise, –1.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLastIndex(ByVal match As Predicate(Of T)) As Integer

            Return FindLastIndex(0, m_processQueue.Count, match)

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns 
        ''' the zero-based index of the last occurrence within the range of elements in the queue that extends from the 
        ''' first element to the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by 
        ''' match, if found; otherwise, –1.</returns>
        ''' <param name="startIndex">The zero-based starting index of the backward search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue.</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLastIndex(ByVal startIndex As Integer, ByVal match As Predicate(Of T)) As Integer

            Return FindLastIndex(startIndex, m_processQueue.Count, match)

        End Function

        ''' <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns 
        ''' the zero-based index of the last occurrence within the range of elements in the queue that contains the 
        ''' specified number of elements and ends at the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by 
        ''' match, if found; otherwise, –1.</returns>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <param name="startIndex">The zero-based starting index of the backward search.</param>
        ''' <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        ''' <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the queue 
        ''' -or- count is less than 0 -or- startIndex and count do not specify a valid section in the queue.</exception>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function FindLastIndex(ByVal startIndex As Integer, ByVal count As Integer, ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If startIndex < 0 OrElse count < 0 OrElse startIndex + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue")
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim foundindex As Integer = -1

                    For x As Integer = startIndex + count - 1 To startIndex Step -1
                        If match(m_processQueue(x)) Then
                            foundindex = x
                            Exit For
                        End If
                    Next

                    Return foundindex
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.FindLastIndex(startIndex, count, match)
                End If
            End SyncLock

        End Function

        ''' <summary>Performs the specified action on each element of the queue.</summary>
        ''' <param name="action">The Action delegate to perform on each element of the queue.</param>
        ''' <exception cref="ArgumentNullException">action is null.</exception>
        Public Overridable Sub ForEach(ByVal action As Action(Of T))

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If action Is Nothing Then Throw New ArgumentNullException("action", "action is null")

                    For Each item As T In m_processQueue
                        action(item)
                    Next
                Else
                    ' Otherwise, we will call native implementation.
                    processQueue.ForEach(action)
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
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If index + count > m_processQueue.Count Then Throw New ArgumentException("Index and count do not denote a valid range of elements in the queue")
                    If index < 0 OrElse count < 0 Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    Dim items As New List(Of T)

                    For x As Integer = index To index + count - 1
                        items.Add(m_processQueue(x))
                    Next

                    Return items
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.GetRange(index, count)
                End If
            End SyncLock

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the first occurrence within 
        ''' the range of elements in the queue that extends from the specified index to the last element.</summary>
        ''' <returns>The zero-based index of the first occurrence of item within the range of elements in the queue that 
        ''' extends from index to the last element, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue.</exception>
        Public Overridable Function IndexOf(ByVal item As T, ByVal index As Integer) As Integer

            Return IndexOf(item, index, m_processQueue.Count)

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the first occurrence within 
        ''' the range of elements in the queue that starts at the specified index and contains the specified number of 
        ''' elements.</summary>
        ''' <returns>The zero-based index of the first occurrence of item within the range of elements in the queue that 
        ''' starts at index and contains count number of elements, if found; otherwise, –1.</returns>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue 
        ''' -or- count is less than 0 -or- index and count do not specify a valid section in the queue.</exception>
        Public Overridable Function IndexOf(ByVal item As T, ByVal index As Integer, ByVal count As Integer) As Integer

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If index < 0 OrElse count < 0 OrElse index + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    Dim foundindex As Integer = -1
                    Dim comparer As Generic.Comparer(Of T) = Generic.Comparer(Of T).Default

                    For x As Integer = index To index + count - 1
                        If comparer.Compare(item, m_processQueue(x)) = 0 Then
                            foundindex = x
                            Exit For
                        End If
                    Next

                    Return foundindex
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.IndexOf(item, index, count)
                End If
            End SyncLock

        End Function

        ''' <summary>Inserts the elements of a collection into the queue at the specified index.</summary>
        ''' <param name="collection">The collection whose elements should be inserted into the queue. The collection 
        ''' itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        ''' <param name="index">The zero-based index at which the new elements should be inserted.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than queue length.</exception>
        ''' <exception cref="ArgumentNullException">collection is null.</exception>
        Public Overridable Sub InsertRange(ByVal index As Integer, ByVal collection As IEnumerable(Of T))

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If index < 0 OrElse index > m_processQueue.Count - 1 Then Throw New ArgumentOutOfRangeException("index", "index is outside the range of valid indexes for the queue")
                    If collection Is Nothing Then Throw New ArgumentNullException("collection", "collection is null")

                    For Each item As T In collection
                        m_processQueue.Insert(index, item)
                        index += 1
                    Next
                Else
                    ' Otherwise, we will call native implementation.
                    processQueue.InsertRange(index, collection)
                End If

                DataAdded()
            End SyncLock

        End Sub

        ''' <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the 
        ''' entire queue.</summary>
        ''' <returns>The zero-based index of the last occurrence of item within the entire the queue, if found; 
        ''' otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        Public Overridable Function LastIndexOf(ByVal item As T) As Integer

            Return LastIndexOf(item, 0, m_processQueue.Count)

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the 
        ''' range of elements in the queue that extends from the first element to the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of item within the range of elements in the queue that 
        ''' extends from the first element to index, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the backward search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue. </exception>
        Public Overridable Function LastIndexOf(ByVal item As T, ByVal index As Integer) As Integer

            Return LastIndexOf(item, index, m_processQueue.Count)

        End Function

        ''' <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the 
        ''' range of elements in the queue that contains the specified number of elements and ends at the specified index.</summary>
        ''' <returns>The zero-based index of the last occurrence of item within the range of elements in the queue that 
        ''' contains count number of elements and ends at index, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based starting index of the backward search.</param>
        ''' <param name="count">The number of elements in the section to search.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the queue -or- 
        ''' count is less than 0 -or- index and count do not specify a valid section in the queue.</exception>
        Public Overridable Function LastIndexOf(ByVal item As T, ByVal index As Integer, ByVal count As Integer) As Integer

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If index < 0 OrElse count < 0 OrElse index + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    Dim foundindex As Integer = -1
                    Dim comparer As Generic.Comparer(Of T) = Generic.Comparer(Of T).Default

                    For x As Integer = index + count - 1 To index Step -1
                        If comparer.Compare(item, m_processQueue(x)) = 0 Then
                            foundindex = x
                            Exit For
                        End If
                    Next

                    Return foundindex
                Else
                    ' Otherwise, we'll call native implementation.
                    Return processQueue.LastIndexOf(item, index, count)
                End If
            End SyncLock

        End Function

        ''' <summary>Removes the all the elements that match the conditions defined by the specified predicate.</summary>
        ''' <returns>The number of elements removed from the queue.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function RemoveAll(ByVal match As Predicate(Of T)) As Integer

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")

                    Dim removedItems As Integer

                    For x As Integer = m_processQueue.Count - 1 To 0 Step -1
                        If match(m_processQueue(x)) Then
                            m_processQueue.RemoveAt(x)
                            removedItems += 1
                        End If
                    Next
                    
                    Return removedItems
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.RemoveAll(match)
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
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If index < 0 OrElse count < 0 OrElse index + count > m_processQueue.Count Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    For x As Integer = index + count - 1 To index Step -1
                        m_processQueue.RemoveAt(x)
                    Next
                Else
                    ' Otherwise, we will call native implementation.
                    processQueue.RemoveRange(index, count)
                End If
            End SyncLock

        End Sub

        ''' <summary>Reverses the order of the elements in the entire queue.</summary>
        Public Overridable Sub Reverse()

            Reverse(0, m_processQueue.Count)

        End Sub

        ''' <summary>Reverses the order of the elements in the specified range.</summary>
        ''' <param name="count">The number of elements in the range to reverse.</param>
        ''' <param name="index">The zero-based starting index of the range to reverse.</param>
        ''' <exception cref="ArgumentException">index and count do not denote a valid range of elements in the queue. </exception>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        Public Overridable Sub Reverse(ByVal index As Integer, ByVal count As Integer)

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If index + count > m_processQueue.Count Then Throw New ArgumentException("Index and count do not denote a valid range of elements in the queue")
                    If index < 0 OrElse count < 0 Then Throw New ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue")

                    Dim item As T
                    Dim stopIndex As Integer = index + count - 1

                    For x As Integer = index To (index + count - 1) \ 2
                        If x < stopIndex Then
                            ' Swaps items top to bottom to reverse order.
                            item = m_processQueue(x)
                            m_processQueue(x) = m_processQueue(stopIndex)
                            m_processQueue(stopIndex) = item
                            stopIndex -= 1
                        End If
                    Next
                Else
                    ' Otherwise, we will call native implementation.
                    processQueue.Reverse(index, count)
                End If
            End SyncLock

        End Sub

        ''' <summary>Sorts the elements in the entire queue, using the default comparer.</summary>
        '''	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an 
        ''' implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        Public Overridable Sub Sort()

            Sort(0, m_processQueue.Count, Nothing)

        End Sub

        ''' <summary>Sorts the elements in the entire queue, using the specified comparer.</summary>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use 
        ''' the default comparer: Generic.Comparer.Default.</param>
        ''' <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For 
        ''' example, comparer might not return 0 when comparing an item with itself.</exception>
        '''	<exception cref="InvalidOperationException">the comparer is null and the default comparer, 
        ''' Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the 
        ''' IComparable interface for type T.</exception>
        Public Overridable Sub Sort(ByVal comparer As IComparer(Of T))

            Sort(0, m_processQueue.Count, comparer)

        End Sub

        ''' <summary>Sorts the elements in a range of elements in the queue, using the specified comparer.</summary>
        ''' <param name="count">The length of the range to sort.</param>
        ''' <param name="index">The zero-based starting index of the range to sort.</param>
        ''' <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use 
        ''' the default comparer: Generic.Comparer.Default.</param>
        ''' <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For 
        ''' example, comparer might not return 0 when comparing an item with itself.</exception>
        '''	<exception cref="InvalidOperationException">the comparer is null and the default comparer, 
        ''' Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the 
        ''' IComparable interface for type T.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        Public Overridable Sub Sort(ByVal index As Integer, ByVal count As Integer, ByVal comparer As IComparer(Of T))

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If comparer Is Nothing Then comparer = Generic.Comparer(Of T).Default

                    ' This sort implementation is a little harsh, but the normal process queue uses List(Of T) and the
                    ' keyed process queue is based on a sorted list anyway (i.e., no sorting needed); so, this alternate
                    ' sort implementation exists for any future derived process queue possibly based on a non List(Of T)
                    ' queue and will at least ensure that the function will perform as expected.
                    Dim items As T() = ToArray()
                    Array.Sort(Of T)(items, index, count, comparer)
                    m_processQueue.Clear()
                    AddRange(items)
                Else
                    ' Otherwise, we will call native implementation.
                    processQueue.Sort(index, count, comparer)
                End If
            End SyncLock

        End Sub

        ''' <summary>Sorts the elements in the entire queue, using the specified comparison.</summary>
        ''' <param name="comparison">The comparison to use when comparing elements.</param>
        ''' <exception cref="ArgumentException">The implementation of comparison caused an error during the sort. For 
        ''' example, comparison might not return 0 when comparing an item with itself.</exception>
        ''' <exception cref="ArgumentNullException">comparison is null.</exception>
        Public Overridable Sub Sort(ByVal comparison As Comparison(Of T))

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If comparison Is Nothing Then Throw New ArgumentNullException("comparison", "comparison is null")

                    ' This sort implementation is a little harsh, but the normal process queue uses List(Of T) and the
                    ' keyed process queue is based on a sorted list anyway (i.e., no sorting needed); so, this alternate
                    ' sort implementation exists for any future derived process queue possibly based on a non-List(Of T)
                    ' queue and will at least ensure that the function will perform as expected.
                    Dim items As T() = ToArray()
                    Array.Sort(Of T)(items, comparison)
                    m_processQueue.Clear()
                    AddRange(items)
                Else
                    ' Otherwise we'll call native implementation
                    processQueue.Sort(comparison)
                End If
            End SyncLock

        End Sub

        ''' <summary>Copies the elements of the queue to a new array.</summary>
        ''' <returns>An array containing copies of the elements of the queue.</returns>
        Public Overridable Function ToArray() As T()

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    Dim items As T() = CreateArray(Of T)(m_processQueue.Count)

                    For x As Integer = 0 To m_processQueue.Count - 1
                        items(x) = m_processQueue(x)
                    Next

                    Return items
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.ToArray()
                End If
            End SyncLock

        End Function

        ''' <summary>Determines whether every element in the queue matches the conditions defined by the specified 
        ''' predicate.</summary>
        ''' <returns>True, if every element in the queue matches the conditions defined by the specified predicate; 
        ''' otherwise, false. If the list has no elements, the return value is true.</returns>
        ''' <param name="match">The Predicate delegate that defines the conditions to check against the elements.</param>
        ''' <exception cref="ArgumentNullException">match is null.</exception>
        Public Overridable Function TrueForAll(ByVal match As Predicate(Of T)) As Boolean

            SyncLock m_processQueue
                Dim processQueue As List(Of T) = TryCast(m_processQueue, List(Of T))

                If processQueue Is Nothing Then
                    ' We manually implement this feature, if process queue is not a List(Of T).
                    If match Is Nothing Then Throw New ArgumentNullException("match", "match is null")
                    Dim allTrue As Boolean = True

                    For Each item As T In m_processQueue
                        If Not match(item) Then
                            allTrue = False
                            Exit For
                        End If
                    Next

                    Return allTrue
                Else
                    ' Otherwise, we will call native implementation.
                    Return processQueue.TrueForAll(match)
                End If
            End SyncLock

        End Function

#End Region

#Region " Handy Queue Functions Implementation "

        ' Note: All queue function implementations should be synchronized, as necessary.

        ''' <summary>Inserts an item onto the top of the queue.</summary>
        ''' <param name="item">The item to push onto the queue.</param>
        Public Overridable Sub Push(ByVal item As T)

            SyncLock m_processQueue
                m_processQueue.Insert(0, item)
                DataAdded()
            End SyncLock

        End Sub

        ''' <summary>Removes the first item from the queue, and returns its value.</summary>
        ''' <exception cref="IndexOutOfRangeException">There are no items in the queue.</exception>
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

        ''' <summary>Removes the last item from the queue, and returns its value.</summary>
        ''' <exception cref="IndexOutOfRangeException">There are no items in the queue.</exception>
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

        ' Note: All IList(Of T) implementations should be synchronized, as necessary.

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

        ''' <summary>Copies the entire queue to a compatible one-dimensional array, starting at the beginning of the 
        ''' target array.</summary>
        ''' <param name="array">The one-dimensional array that is the destination of the elements copied from queue. The 
        ''' array must have zero-based indexing.</param>
        ''' <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        ''' <exception cref="ArgumentException">arrayIndex is equal to or greater than the length of array -or- the 
        ''' number of elements in the source queue is greater than the available space from arrayIndex to the end of the 
        ''' destination array.</exception>
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
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than 
        ''' queue length. </exception>
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

        ''' <summary>Searches for the specified object and returns the zero-based index of the first occurrence within 
        ''' the entire queue.</summary>
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
        ''' <returns>True, if item is found in the queue; otherwise, false.</returns>
        ''' <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
        Public Overridable Function Contains(ByVal item As T) As Boolean Implements IList(Of T).Contains

            SyncLock m_processQueue
                Return m_processQueue.Contains(item)
            End SyncLock

        End Function

        ''' <summary>Removes the first occurrence of a specific object from the queue.</summary>
        ''' <returns>True, if item is successfully removed; otherwise, false. This method also returns false if item was 
        ''' not found in the queue.</returns>
        ''' <param name="item">The object to remove from the queue. The value can be null for reference types.</param>
        Public Overridable Function Remove(ByVal item As T) As Boolean Implements IList(Of T).Remove

            SyncLock m_processQueue
                m_processQueue.Remove(item)
            End SyncLock

        End Function

        ''' <summary>Removes the element at the specified index of the queue.</summary>
        ''' <param name="index">The zero-based index of the element to remove.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than 
        ''' queue length.</exception>
        Public Overridable Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt

            SyncLock m_processQueue
                m_processQueue.RemoveAt(index)
            End SyncLock

        End Sub

        ''' <summary>Gets a value indicating whether the queue is read-only.</summary>
        ''' <returns>True, if the queue is read-only; otherwise, false. In the default implementation, this property 
        ''' always returns false.</returns>
        Public Overridable ReadOnly Property IsReadOnly() As Boolean Implements IList(Of T).IsReadOnly
            Get
                Return m_processQueue.IsReadOnly
            End Get
        End Property

#End Region

#Region " IEnumerable Implementation "

        ''' <summary>
        ''' Gets an enumerator of all items within the queue.
        ''' </summary>
        Private Function IEnumerableGetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return DirectCast(m_processQueue, IEnumerable).GetEnumerator()

        End Function

#End Region

#Region " ICollection Implementation "

        ''' <summary>Returns reference to internal IList that should be used to synchronize access to the queue.</summary>
        ''' <returns>Reference to internal IList that should be used to synchronize access to the queue.</returns>
        ''' <remarks>
        ''' <para>
        ''' Note that all the methods of this class are already individually synchronized; however, to safely enumerate 
        ''' through each queue element (i.e., to make sure list elements do not change during enumeration), derived 
        ''' classes and end users should perform their own synchronization by implementing a SyncLock using this SyncRoot 
        ''' property.
        ''' </para>
        ''' <para>
        ''' We return a typed object for synchronization as an optimization. Returning a generic object requires that 
        ''' SyncLock implementations validate that the referenced object is not a value type at run time.
        ''' </para>
        ''' </remarks>
        Public ReadOnly Property SyncRoot() As IList(Of T)
            Get
                Return m_processQueue
            End Get
        End Property

        ''' <summary>Gets an object that can be used to synchronize access to the queue.</summary>
        ''' <returns>An object that can be used to synchronize access to the queue.</returns>
        ''' <remarks>
        ''' Note that all the methods of this class are already individually synchronized; however, to safely enumerate 
        ''' through each queue element (i.e., to make sure list elements do not change during enumeration), derived 
        ''' classes and end users should perform their own synchronization by implementing a SyncLock using this SyncRoot 
        ''' property.
        ''' </remarks>
        Private ReadOnly Property ICollectionSyncRoot() As Object Implements ICollection.SyncRoot
            Get
                Return m_processQueue
            End Get
        End Property

        ''' <summary>Gets a value indicating whether access to the queue is synchronized (thread safe).</summary>
        ''' <returns>True, if access to the queue is synchronized (thread safe); otherwise, false. In the default 
        ''' implementation, this property always returns true.</returns>
        ''' <remarks>This queue is effectively "synchronized," since all functions SyncLock operations internally.</remarks>
        Public ReadOnly Property IsSynchronized() As Boolean Implements ICollection.IsSynchronized
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