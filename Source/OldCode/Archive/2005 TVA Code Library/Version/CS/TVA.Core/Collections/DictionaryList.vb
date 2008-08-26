'*******************************************************************************************************
'  TVA.Collections.DictionaryList.vb - Sorted dictionary style list that supports IList
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
'       Generated original version of source code
'
'*******************************************************************************************************

Namespace Collections

    ''' <summary>This is essentially a sorted dictionary style list that implements IList.</summary>
    ''' <remarks>
    ''' <para>
    ''' Important note about using an "Integer" as the key for this class: IDictionary implementations
    ''' do not normally implement the IList interface because of ambiguity that is caused when implementing
    ''' an integer key. For example, if you implement this class with a key of type "Integer" you will not
    ''' be able to access items in the queue by index without "casting" the class as IList. This is because
    ''' the Item property in both the IDictionary and IList would have the same parameters.
    ''' </para>
    ''' </remarks>
    Public Class DictionaryList(Of TKey, TValue)

        Implements IList(Of KeyValuePair(Of TKey, TValue)), IDictionary(Of TKey, TValue)

        Private m_list As SortedList(Of TKey, TValue)

        Public Sub New()

            m_list = New SortedList(Of TKey, TValue)

        End Sub

#Region " Generic IList(Of KeyValuePair(Of TKey, TValue)) Implementation "

        Public Sub Add(ByVal item As KeyValuePair(Of TKey, TValue)) Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Add

            m_list.Add(item.Key, item.Value)

        End Sub

        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Clear

            m_list.Clear()

        End Sub

        Public Function Contains(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Contains

            Return m_list.ContainsKey(item.Key)

        End Function

        Public Sub CopyTo(ByVal array() As KeyValuePair(Of TKey, TValue), ByVal arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of TKey, TValue)).CopyTo

            For x As Integer = 0 To m_list.Count - 1
                array(arrayIndex + x) = New KeyValuePair(Of TKey, TValue)(m_list.Keys(x), m_list.Values(x))
            Next

        End Sub

        Public ReadOnly Property Count() As Integer Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Count
            Get
                Return m_list.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Remove

            m_list.Remove(item.Key)

        End Function

        Public Function IndexOf(ByVal item As KeyValuePair(Of TKey, TValue)) As Integer Implements IList(Of KeyValuePair(Of TKey, TValue)).IndexOf

            Return m_list.IndexOfKey(item.Key)

        End Function

        Default Public Overloads Property Item(ByVal index As Integer) As KeyValuePair(Of TKey, TValue) Implements IList(Of KeyValuePair(Of TKey, TValue)).Item
            Get
                Return New KeyValuePair(Of TKey, TValue)(m_list.Keys(index), m_list.Values(index))
            End Get
            Set(ByVal value As KeyValuePair(Of TKey, TValue))
                m_list(value.Key) = value.Value
            End Set
        End Property

        Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of KeyValuePair(Of TKey, TValue)).RemoveAt

            m_list.RemoveAt(index)

        End Sub

        Public Sub Insert(ByVal index As Integer, ByVal item As KeyValuePair(Of TKey, TValue)) Implements IList(Of KeyValuePair(Of TKey, TValue)).Insert

            ' It does not matter where you try to insert the value, since it will be inserted into its sorted 
            ' location, so we just add the value.
            m_list.Add(item.Key, item.Value)

        End Sub

        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of TKey, TValue)) Implements IEnumerable(Of KeyValuePair(Of TKey, TValue)).GetEnumerator

            Return m_list.GetEnumerator()

        End Function

        Private Function IEnumerableGetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return DirectCast(m_list, IEnumerable).GetEnumerator()

        End Function

#End Region

#Region " Generic IDictionary(Of TKey, TValue) Implemenentation "

        Public Sub Add(ByVal key As TKey, ByVal value As TValue) Implements IDictionary(Of TKey, TValue).Add

            m_list.Add(key, value)

        End Sub

        Public Function ContainsKey(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).ContainsKey

            Return m_list.ContainsKey(key)

        End Function

        Public Function ContainsValue(ByVal value As TValue) As Boolean

            Return m_list.ContainsValue(value)

        End Function

        Public Function IndexOfKey(ByVal key As TKey) As Integer

            Return m_list.IndexOfKey(key)

        End Function

        Public Function IndexOfValue(ByVal value As TValue) As Integer

            Return m_list.IndexOfValue(value)

        End Function

        Default Public Overloads Property Item(ByVal key As TKey) As TValue Implements IDictionary(Of TKey, TValue).Item
            Get
                Return m_list(key)
            End Get
            Set(ByVal value As TValue)
                m_list(key) = value
            End Set
        End Property

        Public ReadOnly Property Keys() As ICollection(Of TKey) Implements IDictionary(Of TKey, TValue).Keys
            Get
                Return m_list.Keys
            End Get
        End Property

        Public Function Remove(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).Remove

            m_list.Remove(key)

        End Function

        Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean Implements IDictionary(Of TKey, TValue).TryGetValue

            Return m_list.TryGetValue(key, value)

        End Function

        Public ReadOnly Property Values() As ICollection(Of TValue) Implements IDictionary(Of TKey, TValue).Values
            Get
                Return m_list.Values
            End Get
        End Property

#End Region

    End Class

End Namespace
