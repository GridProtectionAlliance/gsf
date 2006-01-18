'*******************************************************************************************************
'  Tva.Collections.ProcessListBase.vb - Strongly Typed Processing List Base Class
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
    ''' <para>This is the base class used for processing a list of items</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed list of objects to be processed.</para>
    ''' <para>Note to implementors: no derived queue should start processing until the Start method is called.</para>
    ''' </remarks>
    Public MustInherit Class ProcessListBase(Of T)

        Implements IList(Of T), ICollection

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
        Private m_processList As IList(Of T)
        Private m_threadCount As Integer

        ''' <summary>
        ''' Create a process queue using the specified settings
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processQueue As IList(Of T))

            m_processItemFunction = processItemFunction
            m_processList = processQueue

        End Sub

        Public Property ProcessItemFunction() As ProcessItemFunctionSignature
            Get
                Return m_processItemFunction
            End Get
            Set(ByVal value As ProcessItemFunctionSignature)
                m_processItemFunction = value
            End Set
        End Property

        Protected ReadOnly Property InternalList() As IList(Of T)
            Get
                Return m_processList
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

            SyncLock m_processList
                m_processList.Add(item)
            End SyncLock

        End Sub

        Public Sub Push(ByVal item As T)

            SyncLock m_processList
                m_processList.Insert(0, item)
            End SyncLock

        End Sub

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

        Public Function Pop() As T

            SyncLock m_processList
                Dim poppedItem As T = m_processList(0)
                m_processList.RemoveAt(0)
                Return poppedItem
            End SyncLock

        End Function

        Public Function Poop() As T

            SyncLock m_processList
                Dim lastIndex As Integer = m_processList.Count - 1
                Dim poopedItem As T = m_processList(lastIndex)
                m_processList.RemoveAt(lastIndex)
                Return poopedItem
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

        Protected Sub RaiseProcessException(ByVal ex As Exception)

            RaiseEvent ProcessException(ex)

        End Sub

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

        Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
            Get
                Return ICollection.IsSynchronized
            End Get
        End Property

        Private Sub ICollectionCopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo

            ICollection.CopyTo(array, index)

        End Sub

        Private Function ICollectionGetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator

            Return ICollection.GetEnumerator()

        End Function

        Private ReadOnly Property ICollectionCount() As Integer Implements System.Collections.ICollection.Count
            Get
                Return ICollection.Count
            End Get
        End Property

    End Class

End Namespace