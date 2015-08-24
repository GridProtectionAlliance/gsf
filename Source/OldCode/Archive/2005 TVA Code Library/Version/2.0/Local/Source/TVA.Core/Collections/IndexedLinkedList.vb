'*******************************************************************************************************
'  TVA.Collections.IndexedLinkedList.vb - Implements an indexed doubly linked list
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/27/2007 - J. Ritchie Carroll
'       Generated original version of source code.
'
'*******************************************************************************************************

Namespace Collections

    Public Class IndexedLinkedList(Of T)

        Implements IList(Of T), ICollection

        Private m_processQueue As LinkedList(Of T)
        Private m_currentNode As LinkedListNode(Of T)
        Private m_currentPosition As Integer

        Public Sub New()

            m_processQueue = New LinkedList(Of T)

        End Sub

        Public Sub New(ByVal collection As IEnumerable(Of T))

            m_processQueue = New LinkedList(Of T)(collection)

        End Sub

        Public Overridable Function BinarySearch(ByVal array As T(), ByVal index As Integer, ByVal length As Integer, ByVal value As T, ByVal comparer As IComparer(Of T)) As Integer

            If comparer Is Nothing Then comparer = Generic.Comparer(Of T).Default

            Dim num As Integer = index
            Dim num2 As Integer = ((index + length) - 1)

            Do While (num <= num2)
                Dim num4 As Integer
                Dim num3 As Integer = (num + ((num2 - num) >> 1))
                Try
                    num4 = comparer.Compare(array(num3), value)
                Catch exception As Exception
                    Throw New InvalidOperationException("InvalidOperation: IComparerFailed", exception)
                End Try
                If (num4 = 0) Then
                    Return num3
                End If

                If (num4 < 0) Then
                    num = (num3 + 1)
                Else
                    num2 = (num3 - 1)
                End If
            Loop

            Return Not num

        End Function

#Region " Generic IList(Of T) Implementation "

        ''' <summary>Adds an item to the list.</summary>
        ''' <param name="item">The item to add to the list.</param>
        Public Overridable Sub Add(ByVal item As T) Implements IList(Of T).Add

            m_processQueue.AddLast(item)

        End Sub

        ''' <summary>Inserts an element into the list at the specified index.</summary>
        ''' <param name="item">The object to insert. The value can be null for reference types.</param>
        ''' <param name="index">The zero-based index at which item should be inserted.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than list length.</exception>
        Public Overridable Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert

            'm_processQueue.Insert(index, item)

        End Sub

        ''' <summary>Copies the entire list to a compatible one-dimensional array, starting at the beginning of the 
        ''' target array.</summary>
        ''' <param name="array">The one-dimensional array that is the destination of the elements copied from list. The 
        ''' array must have zero-based indexing.</param>
        ''' <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        ''' <exception cref="ArgumentException">arrayIndex is equal to or greater than the length of array -or- the 
        ''' number of elements in the source list is greater than the available space from arrayIndex to the end of the 
        ''' destination array.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        ''' <exception cref="ArgumentNullException">array is null.</exception>
        Public Overridable Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements IList(Of T).CopyTo

            m_processQueue.CopyTo(array, arrayIndex)

        End Sub

        ''' <summary>Returns an enumerator that iterates through the list.</summary>
        ''' <returns>An enumerator for the list.</returns>
        Public Overridable Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator

            Return m_processQueue.GetEnumerator()

        End Function

        ''' <summary>Gets or sets the element at the specified index.</summary>
        ''' <returns>The element at the specified index.</returns>
        ''' <param name="index">The zero-based index of the element to get or set.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than 
        ''' list length. </exception>
        Default Public Overridable Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
            Get
                'Return m_processQueue(index)
            End Get
            Set(ByVal value As T)
                'm_processQueue(index) = value
            End Set
        End Property

        ''' <summary>Searches for the specified object and returns the zero-based index of the first occurrence within 
        ''' the entire list.</summary>
        ''' <returns>The zero-based index of the first occurrence of item within the entire list, if found; otherwise, –1.</returns>
        ''' <param name="item">The object to locate in the list. The value can be null for reference types.</param>
        Public Overridable Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf

            'Return m_processQueue.IndexOf(item)

        End Function

        ''' <summary>Gets the number of elements actually contained in the list.</summary>
        ''' <returns>The number of elements actually contained in the list.</returns>
        Public Overridable ReadOnly Property Count() As Integer Implements IList(Of T).Count
            Get
                Return m_processQueue.Count
            End Get
        End Property

        ''' <summary>Removes all elements from the list.</summary>
        Public Overridable Sub Clear() Implements IList(Of T).Clear

            m_processQueue.Clear()

        End Sub

        ''' <summary>Determines whether an element is in the list.</summary>
        ''' <returns>True, if item is found in the list; otherwise, false.</returns>
        ''' <param name="item">The object to locate in the list. The value can be null for reference types.</param>
        Public Overridable Function Contains(ByVal item As T) As Boolean Implements IList(Of T).Contains

            Return m_processQueue.Contains(item)

        End Function

        ''' <summary>Removes the first occurrence of a specific object from the list.</summary>
        ''' <returns>True, if item is successfully removed; otherwise, false. This method also returns false if item was 
        ''' not found in the list.</returns>
        ''' <param name="item">The object to remove from the list. The value can be null for reference types.</param>
        Public Overridable Function Remove(ByVal item As T) As Boolean Implements IList(Of T).Remove

            m_processQueue.Remove(item)

        End Function

        ''' <summary>Removes the element at the specified index of the list.</summary>
        ''' <param name="index">The zero-based index of the element to remove.</param>
        ''' <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than 
        ''' list length.</exception>
        Public Overridable Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt

            'm_processQueue.RemoveAt(index)

        End Sub

        ''' <summary>Gets a value indicating whether the list is read-only.</summary>
        ''' <returns>True, if the list is read-only; otherwise, false. In the default implementation, this property 
        ''' always returns false.</returns>
        Public Overridable ReadOnly Property IsReadOnly() As Boolean Implements IList(Of T).IsReadOnly
            Get
                Return False
            End Get
        End Property

#End Region

#Region " IEnumerable Implementation "

        ''' <summary>
        ''' Gets an enumerator of all items within the list.
        ''' </summary>
        Private Function IEnumerableGetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return DirectCast(m_processQueue, IEnumerable).GetEnumerator()

        End Function

#End Region

#Region " ICollection Implementation "

        ''' <summary>Returns reference to internal IList that should be used to synchronize access to the list.</summary>
        ''' <returns>Reference to internal IList that should be used to synchronize access to the list.</returns>
        ''' <remarks>
        ''' <para>
        ''' Note that all the methods of this class are already individually synchronized; however, to safely enumerate 
        ''' through each list element (i.e., to make sure list elements do not change during enumeration), derived 
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

        ''' <summary>Gets an object that can be used to synchronize access to the list.</summary>
        ''' <returns>An object that can be used to synchronize access to the list.</returns>
        ''' <remarks>
        ''' Note that all the methods of this class are already individually synchronized; however, to safely enumerate 
        ''' through each list element (i.e., to make sure list elements do not change during enumeration), derived 
        ''' classes and end users should perform their own synchronization by implementing a SyncLock using this SyncRoot 
        ''' property.
        ''' </remarks>
        Private ReadOnly Property ICollectionSyncRoot() As Object Implements ICollection.SyncRoot
            Get
                Return m_processQueue
            End Get
        End Property

        ''' <summary>Gets a value indicating whether access to the list is synchronized (thread safe).</summary>
        ''' <returns>True, if access to the list is synchronized (thread safe); otherwise, false. In the default 
        ''' implementation, this property always returns false.</returns>
        Public ReadOnly Property IsSynchronized() As Boolean Implements ICollection.IsSynchronized
            Get
                Return False
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