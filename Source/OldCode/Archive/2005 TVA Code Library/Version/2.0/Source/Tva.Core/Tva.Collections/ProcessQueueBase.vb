'*******************************************************************************************************
'  Tva.Collections.ProcessQueueBase.vb - Strongly Typed Processing Queue Base Class
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
'  01/07/2006 - James R Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    ''' <summary>
    ''' <para>This is the base class used for processing a collection of items</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Note to implementors: no derived queue should start processing until the Start method is called.</para>
    ''' </remarks>
    Public MustInherit Class ProcessQueueBase(Of T)

        Implements IList(Of T)

        ''' <summary>
        ''' This is the function signature used for defining a method to process items
        ''' </summary>
        ''' <param name="item">Item to be processed</param>
        Public Delegate Sub ProcessItemFunctionSignature(ByVal item As T)

        ''' <summary>
        ''' This event will be raised if there is an exception encountered while attempting to processing an item in the queue
        ''' </summary>
        Public Event ProcessException(ByVal ex As Exception)

        Private m_processItemFunction As ProcessItemFunctionSignature
        Private m_threadCount As Integer
        Private m_processQueue As List(Of T)

        ''' <summary>
        ''' Create a process queue using the specified settings
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature)

            m_processItemFunction = processItemFunction
            m_processQueue = New List(Of T)

        End Sub

        Public Property ProcessItemFunction() As ProcessItemFunctionSignature
            Get
                Return m_processItemFunction
            End Get
            Set(ByVal value As ProcessItemFunctionSignature)
                m_processItemFunction = value
            End Set
        End Property

        Protected ReadOnly Property InternalQueue() As List(Of T)
            Get
                Return m_processQueue
            End Get
        End Property

        Public MustOverride Sub Start()

        Public MustOverride Sub [Stop]()

        Public ReadOnly Property ThreadCount() As Integer
            Get
                Return m_threadCount
            End Get
        End Property

        Protected Sub IncrementThreadCount()

            Interlocked.Increment(m_threadCount)

        End Sub

        Protected Sub DecrementThreadCount()

            Interlocked.Decrement(m_threadCount)

        End Sub

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
                Dim lastIndex As Integer = m_processQueue.Count - 1
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
                    Return m_processQueue.Count
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

        Protected Sub RaiseProcessException(ByVal ex As Exception)

            RaiseEvent ProcessException(ex)

        End Sub

    End Class

End Namespace