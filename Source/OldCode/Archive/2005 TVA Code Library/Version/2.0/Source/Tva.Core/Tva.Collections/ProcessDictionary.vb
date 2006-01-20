'*******************************************************************************************************
'  Tva.Collections.KeyedProcessQueue.vb - Multi-threaded Keyed Item Processing Queue
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
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
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    ''' <summary>
    ''' <para>This class will process a keyed collection of items on independent threads</para>
    ''' </summary>
    ''' <typeparam name="TKey">Type of keys used to references process items</typeparam>
    ''' <typeparam name="TValue">Type of values to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed dictionary of objects to be processed.</para>
    ''' <para>Consumers are expected to create new instances of this class through the static construction functions (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class KeyedProcessQueue(Of TKey, TValue)

        Inherits ProcessQueue(Of KeyValuePair(Of TKey, TValue))

        Implements IDictionary(Of TKey, TValue), IDictionary

        ' **************************************
        '
        '        Construction Functions
        '
        ' **************************************

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal maximumThreads As Integer) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, Nothing, processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemFunction, Nothing, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemFunction, Nothing, processInterval, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemFunction, Nothing, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue with the default settings: ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return CreateRealTimeQueue(processItemFunction, Nothing, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed real-time process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateRealTimeQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As KeyedProcessQueue(Of TKey, TValue)

            Return New KeyedProcessQueue(Of TKey, TValue)(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' This constructor creates a ProcessList based on the generic SortedDictionary class
        ''' </summary>
        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal processInterval As Double, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            MyBase.New(processItemFunction, canProcessItemFunction, New SortedDictionary(Of TKey, TValue), processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Sub

        ' **************************************
        '
        '      Public Member Implementation
        '
        ' **************************************

        Public Overrides ReadOnly Property Name() As String
            Get
                Return Me.GetType.Name
            End Get
        End Property

        ' **************************************
        '
        '   Generic IDictionary Implementation
        '
        ' **************************************

        Protected ReadOnly Property InternalDictionary() As SortedDictionary(Of TKey, TValue)
            Get
                Return DirectCast(InternalList, SortedDictionary(Of TKey, TValue))
            End Get
        End Property

        Public Overloads Sub Add(ByVal key As TKey, ByVal value As TValue) Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Add

            SyncLock SyncRoot
                InternalDictionary.Add(key, value)
            End SyncLock

        End Sub

        Public Function ContainsKey(ByVal key As TKey) As Boolean Implements System.Collections.Generic.IDictionary(Of TKey, TValue).ContainsKey

            SyncLock SyncRoot
                Return InternalDictionary.ContainsKey(key)
            End SyncLock

        End Function

        Default Public Overloads Property Item(ByVal key As TKey) As TValue Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Item
            Get
                SyncLock SyncRoot
                    Return InternalDictionary(key)
                End SyncLock
            End Get
            Set(ByVal value As TValue)
                SyncLock SyncRoot
                    InternalDictionary(key) = value
                End SyncLock
            End Set
        End Property

        Public Overloads Function Remove(ByVal key As TKey) As Boolean Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Remove

            SyncLock SyncRoot
                InternalDictionary.Remove(key)
            End SyncLock

        End Function

        Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean Implements System.Collections.Generic.IDictionary(Of TKey, TValue).TryGetValue

            SyncLock SyncRoot
                Return InternalDictionary.TryGetValue(key, value)
            End SyncLock

        End Function

        Public ReadOnly Property Keys() As System.Collections.Generic.ICollection(Of TKey) Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Keys
            Get
                Return InternalDictionary.Keys
            End Get
        End Property

        Public ReadOnly Property Values() As System.Collections.Generic.ICollection(Of TValue) Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Values
            Get
                Return InternalDictionary.Values
            End Get
        End Property

        ' **************************************
        '
        '       IDictionary Implementation
        '
        ' **************************************

        Private ReadOnly Property IDictionary() As IDictionary
            Get
                Return DirectCast(InternalDictionary, IDictionary)
            End Get
        End Property

        Private Sub IDictionaryAdd(ByVal key As Object, ByVal value As Object) Implements System.Collections.IDictionary.Add

            SyncLock SyncRoot
                IDictionary.Add(key, value)
            End SyncLock

        End Sub

        Private Function IDictionaryContains(ByVal key As Object) As Boolean Implements System.Collections.IDictionary.Contains

            SyncLock SyncRoot
                Return IDictionary.Contains(key)
            End SyncLock

        End Function

        Private Property IDictionaryItem(ByVal key As Object) As Object Implements System.Collections.IDictionary.Item
            Get
                SyncLock SyncRoot
                    Return IDictionary(key)
                End SyncLock
            End Get
            Set(ByVal value As Object)
                SyncLock SyncRoot
                    IDictionary(key) = value
                End SyncLock
            End Set
        End Property

        Private ReadOnly Property IDictionaryKeys() As System.Collections.ICollection Implements System.Collections.IDictionary.Keys
            Get
                Return IDictionary.Keys
            End Get
        End Property

        Private ReadOnly Property IDictionaryValues() As System.Collections.ICollection Implements System.Collections.IDictionary.Values
            Get
                Return IDictionary.Values
            End Get
        End Property

        Private Sub IDictionaryRemove(ByVal key As Object) Implements System.Collections.IDictionary.Remove

            SyncLock SyncRoot
                IDictionary.Remove(key)
            End SyncLock

        End Sub

        Private ReadOnly Property IDictionaryIsFixedSize() As Boolean Implements System.Collections.IDictionary.IsFixedSize
            Get
                Return IDictionary.IsFixedSize
            End Get
        End Property

        Private ReadOnly Property IDictionaryIsReadOnly() As Boolean Implements System.Collections.IDictionary.IsReadOnly
            Get
                Return IDictionary.IsReadOnly
            End Get
        End Property

        Private Sub IDictionaryClear() Implements System.Collections.IDictionary.Clear

            SyncLock SyncRoot
                IDictionary.Clear()
            End SyncLock

        End Sub

        Private Function IDictionaryGetEnumerator() As System.Collections.IDictionaryEnumerator Implements System.Collections.IDictionary.GetEnumerator

            Return IDictionary.GetEnumerator

        End Function

    End Class

End Namespace