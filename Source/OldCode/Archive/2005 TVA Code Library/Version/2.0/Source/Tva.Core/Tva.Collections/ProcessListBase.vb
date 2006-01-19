'*******************************************************************************************************
'  Tva.Collections.ProcessListBase.vb - Strongly Typed Item Processing List Base Class
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
Imports System.Text
Imports Tva.DateTime.Common

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
        ''' This event will be raised if there is an exception encountered while attempting to processing an item in the list
        ''' </summary>
        Public Event ProcessException(ByVal ex As Exception)

        Private m_processItemFunction As ProcessItemFunctionSignature
        Private m_processList As IList(Of T)
        Private m_threadCount As Integer
        Private m_processing As Boolean
        Private m_itemsProcessed As Long
        Private m_startTime As Long
        Private m_stopTime As Long

        Protected Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processList As IList(Of T))

            m_processItemFunction = processItemFunction
            m_processList = processList

        End Sub

        ''' <summary>
        ''' This property defines the user function used to process items in the list
        ''' </summary>
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

        ''' <summary>
        ''' Starts item processing
        ''' </summary>
        Public Overridable Sub Start()

            m_processing = True
            m_itemsProcessed = 0
            m_stopTime = 0
            m_startTime = Date.Now.Ticks

        End Sub

        ''' <summary>
        ''' Stops items processing
        ''' </summary>
        Public Overridable Sub [Stop]()

            m_processing = False
            m_stopTime = Date.Now.Ticks

        End Sub

        ''' <summary>
        ''' Determines if the list is currently processing
        ''' </summary>
        Public Property Processing() As Boolean
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
        Public ReadOnly Property ItemsProcessed() As Long
            Get
                Return m_itemsProcessed
            End Get
        End Property

        ''' <summary>
        ''' Returns the current number of active threads
        ''' </summary>
        Public ReadOnly Property ThreadCount() As Integer
            Get
                Return m_threadCount
            End Get
        End Property

        ''' <summary>
        ''' Returns the total amount of time, in seconds, that the process list has been active
        ''' </summary>
        Public ReadOnly Property RunTime() As Double
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

        Protected Sub IncrementThreadCount()

            Interlocked.Increment(m_threadCount)

        End Sub

        Protected Sub DecrementThreadCount()

            Interlocked.Decrement(m_threadCount)

        End Sub

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
                    Throw New IndexOutOfRangeException("The list is empty")
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
                    Throw New IndexOutOfRangeException("The list is empty")
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

        Public ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("  Current processing state: ")
                    If Processing Then .Append("Executing") Else .Append("Idle")
                    .Append(vbCrLf)
                    .Append("    Total process run time: ")
                    .Append(SecondsToText(RunTime))
                    .Append(vbCrLf)
                    .Append("   Queued items to process: ")
                    .Append(Count)
                    .Append(vbCrLf)
                    .Append("  Total processing threads: ")
                    .Append(ThreadCount)
                    .Append(vbCrLf)
                    .Append("     Total items processed: ")
                    .Append(ItemsProcessed)
                    .Append(vbCrLf)

                    Return .ToString()
                End With
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