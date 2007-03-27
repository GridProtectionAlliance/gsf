'*******************************************************************************************************
'  TVA.Collections.KeyedProcessQueue.vb - Multi-threaded Keyed Item Processing Queue
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
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.ComponentModel
Imports TVA.Common

Namespace Collections

    ''' <summary>
    ''' <para>This class will process a keyed collection of items on independent threads.</para>
    ''' <para>Consumer must implement a function to process items.</para>
    ''' </summary>
    ''' <typeparam name="TKey">Type of keys used to reference process items</typeparam>
    ''' <typeparam name="TValue">Type of values to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed sorted dictionary of objects to be processed.</para>
    ''' <para>Consumers are expected to create new instances of this class through the static construction functions (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' <para>Because this queue represents a dictionary style collection, all keys must be unique.</para>
    ''' <para>
    ''' Be aware that this class is based on a DictionaryList (i.e., a SortedList that implements IList) and
    ''' since items in this kind of list are automatically sorted, items will be processed in "sorted" order
    ''' regardless of the order in which they are added to the list.
    ''' </para>
    ''' <para>
    ''' Important note about using an "Integer" as the key for this class: because the queue base class must
    ''' implement IList, a normal dictionary cannot be used for the base class.  IDictionary implementations
    ''' do not normally implement the IList interface because of ambiguity that is caused when implementing
    ''' an integer key.  For example, if you implement this class with a key of type "Integer" you will not
    ''' be able to access items in the queue by index without "casting" the queue as IList, this is because
    ''' the Item property in both the IDictionary and IList would have the same parameters (see the
    ''' DictionaryList class for more details.)
    ''' </para>
    ''' </remarks>
    Public Class KeyedProcessQueue(Of TKey, TValue)

        Inherits ProcessQueue(Of KeyValuePair(Of TKey, TValue))

        Implements IDictionary(Of TKey, TValue)

#Region " Public Member Declarations "

        ''' <summary>
        ''' This is the function signature used for defining a method to process a key and value one at a time
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is required unless ProcessItemsFunction is implemented</para>
        ''' <para>This function is used when creating a queue to process one item at a time</para>
        ''' <para>Asynchronous queues will process individual items on multiple threads</para>
        ''' </remarks>
        ''' <param name="key">key to be processed</param>
        ''' <param name="value">value to be processed</param>
        Public Shadows Delegate Sub ProcessItemFunctionSignature(ByVal key As TKey, ByVal value As TValue)

        ''' <summary>
        ''' This is the function signature used for defining a method to process multiple keys and values at once
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is required unless ProcessItemFunction is implemented</para>
        ''' <para>This function is used when creating a queue to process multiple items at once</para>
        ''' <para>Asynchronous queues will process groups of items on multiple threads</para>
        ''' </remarks>
        ''' <param name="keys">keys to be processed</param>
        ''' <param name="values">values to be processed</param>
        Public Shadows Delegate Sub ProcessItemsFunctionSignature(ByVal keys As TKey(), ByVal values As TValue())

        ''' <summary>
        ''' This is the function signature used for determining if a key and value can be currently processed
        ''' </summary>
        ''' <remarks>
        ''' <para>Implementation of this function is optional; it will be assumed that an item can be processed if this function is not defined</para>
        ''' <para>Items must eventually get to a state where they can be processed or they will remain in the queue forever</para>
        ''' <para>
        ''' Note that when this function is implemented and ProcessingStyle = ManyAtOnce (i.e., ProcessItemsFunction is defined)
        ''' then each item presented for processing must evaluate as "CanProcessItem = True" before any items are processed
        ''' </para>
        ''' </remarks>
        ''' <param name="key">key to be checked for processing availablity</param>
        ''' <param name="value">value to be checked for processing availablity</param>
        ''' <returns>Function should return True if key and value can be processed</returns>
        Public Shadows Delegate Function CanProcessItemFunctionSignature(ByVal key As TKey, ByVal value As TValue) As Boolean

#End Region

#Region " Private Member Declarations "

        Private m_processItemFunction As ProcessItemFunctionSignature
        Private m_processItemsFunction As ProcessItemsFunctionSignature
        Private m_canProcessItemFunction As CanProcessItemFunctionSignature

#End Region

#Region " Construction Functions "

#Region " Single-Item Processing Constructors "

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemFunction, Nothing, processInterval, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemFunction, Nothing, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

#End Region

#Region " Multi-Item Processing Constructors "

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal maximumThreads As Integer) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemsFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new asynchronous bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemsFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemsFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemsFunction, Nothing, processInterval, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new synchronous bulk-item process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemsFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemsFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemsFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemsFunction, Nothing, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

        ''' <summary>
        ''' Create a new real-time bulk-item process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemsFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException)

        End Function

#End Region

#Region " Protected Constructors "

        ''' <summary>
        ''' This constructor creates a ProcessList based on the generic DictionaryList class
        ''' </summary>
        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean)

            MyBase.New(Nothing, Nothing, Nothing, New DictionaryList(Of TKey, TValue), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

            m_processItemFunction = processItemFunction         ' Defining this function creates a ProcessingStyle = OneAtATime keyed process queue
            m_canProcessItemFunction = canProcessItemFunction

            ' Assign translator functions for base class
            MyBase.ProcessItemFunction = AddressOf ProcessKeyedItem
            If m_canProcessItemFunction IsNot Nothing Then MyBase.CanProcessItemFunction = AddressOf CanProcessKeyedItem

        End Sub

        ''' <summary>
        ''' This constructor creates a bulk-item ProcessList based on the generic DictionaryList class
        ''' </summary>
        Protected Sub New(ByVal processItemsFunction As ProcessItemsFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean, ByVal requeueOnException As Boolean)

            MyBase.New(Nothing, Nothing, Nothing, New DictionaryList(Of TKey, TValue), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)

            m_processItemsFunction = processItemsFunction       ' Defining this function creates a ProcessingStyle = ManyAtOnce keyed process queue
            m_canProcessItemFunction = canProcessItemFunction

            ' Assign translator functions for base class
            MyBase.ProcessItemsFunction = AddressOf ProcessKeyedItems
            If m_canProcessItemFunction IsNot Nothing Then MyBase.CanProcessItemFunction = AddressOf CanProcessKeyedItem

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
        Public Overridable Shadows Property ProcessItemFunction() As ProcessItemFunctionSignature
            Get
                Return m_processItemFunction
            End Get
            Set(ByVal value As ProcessItemFunctionSignature)
                If value IsNot Nothing Then
                    m_processItemFunction = value
                    m_processItemsFunction = Nothing

                    ' Assign translator functions for base class
                    MyBase.ProcessItemFunction = AddressOf ProcessKeyedItem
                    MyBase.ProcessItemsFunction = Nothing
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
        Public Overridable Shadows Property ProcessItemsFunction() As ProcessItemsFunctionSignature
            Get
                Return m_processItemsFunction
            End Get
            Set(ByVal value As ProcessItemsFunctionSignature)
                If value IsNot Nothing Then
                    m_processItemsFunction = value
                    m_processItemFunction = Nothing

                    ' Assign translator functions for base class
                    MyBase.ProcessItemsFunction = AddressOf ProcessKeyedItems
                    MyBase.ProcessItemFunction = Nothing
                End If
            End Set
        End Property

        ''' <summary>
        ''' This property defines the user function used to determine if an item is ready to be processed
        ''' </summary>
        Public Overridable Shadows Property CanProcessItemFunction() As CanProcessItemFunctionSignature
            Get
                Return m_canProcessItemFunction
            End Get
            Set(ByVal value As CanProcessItemFunctionSignature)
                m_canProcessItemFunction = value

                ' Assign translator function for base class
                If m_canProcessItemFunction Is Nothing Then
                    MyBase.CanProcessItemFunction = Nothing
                Else
                    MyBase.CanProcessItemFunction = AddressOf CanProcessKeyedItem
                End If
            End Set
        End Property

        ''' <summary>
        ''' Returns class name
        ''' </summary>
        ''' <remarks>
        ''' <para>This name is used for class identification in strings (e.g., used in error message)</para>
        ''' <para>Derived classes should override this method with a proper class name</para>
        ''' </remarks>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return Me.GetType.Name
            End Get
        End Property

#End Region

#Region " Protected Methods Implementation "

        ''' <summary>
        ''' This property allows derived classes to access the internal sorted dictionary directly
        ''' </summary>
        Protected ReadOnly Property InternalDictionary() As DictionaryList(Of TKey, TValue)
            Get
                Return DirectCast(InternalList, DictionaryList(Of TKey, TValue))
            End Get
        End Property

#End Region

#Region " Private Methods Implementation "

        ' These functions act as intermediate "translators" between the delegate implementations of ProcessQueue and KeyedProcessQueue
        ' Users implementing a KeyedProcessQueue will be thinking in terms of "keys" and "values", not a KeyValuePair structure
        Private Sub ProcessKeyedItem(ByVal item As KeyValuePair(Of TKey, TValue))

            With item
                m_processItemFunction(.Key, .Value)
            End With

        End Sub

        Private Sub ProcessKeyedItems(ByVal items As KeyValuePair(Of TKey, TValue)())

            ' Copy array of KeyValuePairs into an array of keys and values
            Dim keys As TKey() = CreateArray(Of TKey)(items.Length)
            Dim values As TValue() = CreateArray(Of TValue)(items.Length)

            For x As Integer = 0 To items.Length - 1
                With items(x)
                    keys(x) = .Key
                    values(x) = .Value
                End With
            Next

            m_processItemsFunction(keys, values)

        End Sub

        Private Function CanProcessKeyedItem(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean

            With item
                Return m_canProcessItemFunction(.Key, .Value)
            End With

        End Function

#End Region

#Region " Generic IDictionary(Of TKey, TValue) Implementation "

        ''' <summary>Adds an element with the provided key and value to the queue.</summary>
        ''' <param name="value">The object to use as the value of the element to add.</param>
        ''' <param name="key">The object to use as the key of the element to add.</param>
        ''' <exception cref="NotSupportedException">The queue is read-only.</exception>
        ''' <exception cref="ArgumentException">An element with the same key already exists in the queue.</exception>
        ''' <exception cref="ArgumentNullException">key is null.</exception>
        Public Overloads Sub Add(ByVal key As TKey, ByVal value As TValue) Implements IDictionary(Of TKey, TValue).Add

            SyncLock SyncRoot
                InternalDictionary.Add(key, value)
                DataAdded()
            End SyncLock

        End Sub

        ''' <summary>Determines whether the queue contains an element with the specified key.</summary>
        ''' <returns>true if the queue contains an element with the key; otherwise, false.</returns>
        ''' <param name="key">The key to locate in the queue.</param>
        ''' <exception cref="ArgumentNullException">key is null.</exception>
        Public Function ContainsKey(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).ContainsKey

            SyncLock SyncRoot
                Return InternalDictionary.ContainsKey(key)
            End SyncLock

        End Function

        ''' <summary>Determines whether the queue contains an element with the specified value.</summary>
        ''' <returns>true if the queue contains an element with the value; otherwise, false.</returns>
        ''' <param name="value">The value to locate in the queue.</param>
        Public Function ContainsValue(ByVal value As TValue) As Boolean

            SyncLock SyncRoot
                Return InternalDictionary.ContainsValue(value)
            End SyncLock

        End Function

        Public Function IndexOfKey(ByVal key As TKey) As Integer

            SyncLock SyncRoot
                Return InternalDictionary.IndexOfKey(key)
            End SyncLock

        End Function

        Public Function IndexOfValue(ByVal value As TValue) As Integer

            SyncLock SyncRoot
                Return InternalDictionary.IndexOfValue(value)
            End SyncLock

        End Function

        ''' <summary>Gets or sets the value associated with the specified key.</summary>
        ''' <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a KeyNotFoundException, and a set operation creates a new element with the specified key.</returns>
        ''' <param name="key">The key of the value to get or set.</param>
        ''' <exception cref="ArgumentNullException">key is null.</exception>
        ''' <exception cref="KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
        Default Public Overloads Property Item(ByVal key As TKey) As TValue Implements IDictionary(Of TKey, TValue).Item
            Get
                SyncLock SyncRoot
                    Return InternalDictionary(key)
                End SyncLock
            End Get
            Set(ByVal value As TValue)
                SyncLock SyncRoot
                    InternalDictionary(key) = value
                    DataAdded()
                End SyncLock
            End Set
        End Property

        ''' <summary>Removes the element with the specified key from the queue.</summary>
        ''' <param name="key">The key of the element to remove.</param>
        ''' <exception cref="ArgumentNullException">key is null.</exception>
        Public Overloads Function Remove(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).Remove

            SyncLock SyncRoot
                InternalDictionary.Remove(key)
            End SyncLock

        End Function

        ''' <summary>Gets the value associated with the specified key.</summary>
        ''' <returns>true if the queue contains an element with the specified key; otherwise, false.</returns>
        ''' <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        ''' <param name="key">The key of the value to get.</param>
        ''' <exception cref="ArgumentNullException">key is null.</exception>
        Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean Implements IDictionary(Of TKey, TValue).TryGetValue

            SyncLock SyncRoot
                Return InternalDictionary.TryGetValue(key, value)
            End SyncLock

        End Function

        ''' <summary>Gets an ICollection containing the keys of the queue.</summary>
        ''' <returns>An ICollection containing the keys of the queue.</returns>
        Public ReadOnly Property Keys() As ICollection(Of TKey) Implements IDictionary(Of TKey, TValue).Keys
            Get
                Return InternalDictionary.Keys
            End Get
        End Property

        ''' <summary>Gets an ICollection containing the values of the queue.</summary>
        ''' <returns>An ICollection containing the values of the queue.</returns>
        Public ReadOnly Property Values() As ICollection(Of TValue) Implements IDictionary(Of TKey, TValue).Values
            Get
                Return InternalDictionary.Values
            End Get
        End Property

#End Region

#Region " Overriden List(T) Functions "

        ' Because consumers will be able to call these functions in their "dictionary" style queue, we'll make
        ' sure they return something that makes sense in case they get called - but we'll hide them from the
        ' editor to help avoid confusion
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function BinarySearch(ByVal item As KeyValuePair(Of TKey, TValue)) As Integer

            Return IndexOfKey(item.Key)

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function BinarySearch(ByVal item As KeyValuePair(Of TKey, TValue), ByVal comparer As IComparer(Of KeyValuePair(Of TKey, TValue))) As Integer

            Return IndexOfKey(item.Key)

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function BinarySearch(ByVal index As Integer, ByVal count As Integer, ByVal item As KeyValuePair(Of TKey, TValue), ByVal comparer As IComparer(Of KeyValuePair(Of TKey, TValue))) As Integer

            Return IndexOfKey(item.Key)

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function IndexOf(ByVal item As KeyValuePair(Of TKey, TValue)) As Integer

            Return IndexOfKey(item.Key)

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function IndexOf(ByVal item As KeyValuePair(Of TKey, TValue), ByVal index As Integer, ByVal count As Integer) As Integer

            Return IndexOfKey(item.Key)

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function LastIndexOf(ByVal item As KeyValuePair(Of TKey, TValue)) As Integer

            Return IndexOfKey(item.Key)

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function LastIndexOf(ByVal item As KeyValuePair(Of TKey, TValue), ByVal index As Integer, ByVal count As Integer) As Integer

            Return IndexOfKey(item.Key)

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub Sort()

            ' This list is already sorted...

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub Sort(ByVal comparer As IComparer(Of KeyValuePair(Of TKey, TValue)))

            ' This list is already sorted...

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub Sort(ByVal comparison As System.Comparison(Of KeyValuePair(Of TKey, TValue)))

            ' This list is already sorted...

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub Sort(ByVal index As Integer, ByVal count As Integer, ByVal comparer As IComparer(Of KeyValuePair(Of TKey, TValue)))

            ' This list is already sorted...

        End Sub

#End Region

    End Class

End Namespace