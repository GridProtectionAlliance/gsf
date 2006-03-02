'*******************************************************************************************************
'  Tva.Collections.Common.vb - Common Collection Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - James R Carroll
'       Original version of source code generated
'  01/23/2005 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Math.Common

Namespace Collections

    ''' <summary>Defines common global functions related to manipulation of collections</summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Returns smallest item from list of parameters</summary>
        Public Shared Function Minimum(ByVal ParamArray itemList() As Object) As Object

            Return Minimum(ItemList)

        End Function

        ''' <summary>Returns largest item from list of parameters</summary>
        Public Shared Function Maximum(ByVal ParamArray itemList() As Object) As Object

            Return Maximum(ItemList)

        End Function

        ''' <summary>Returns smallest item from the specified enumeration</summary>
        Public Shared Function Minimum(ByVal items As IEnumerable) As Object

            Dim objMin As Object = Nothing

            With items.GetEnumerator()
                If .MoveNext() Then
                    objMin = .Current
                    While .MoveNext()
                        If Compare(.Current, objMin) < 0 Then objMin = .Current
                    End While
                End If
            End With

            Return objMin

        End Function

        ''' <summary>Returns largest item from the specified enumeration</summary>
        Public Shared Function Maximum(ByVal items As IEnumerable) As Object

            Dim objMax As Object = Nothing

            With items.GetEnumerator()
                If .MoveNext() Then
                    objMax = .Current
                    While .MoveNext()
                        If Compare(.Current, objMax) > 0 Then objMax = .Current
                    End While
                End If
            End With

            Return objMax

        End Function

        ''' <summary> Compares two elements of any type.</summary>
        Public Shared Function Compare(ByVal x As Object, ByVal y As Object) As Integer

            If IsReference(x) And IsReference(y) Then
                ' If both items are string reference objects then test object equality by reference,
                ' then if not equal by overriable Object.Equals function use default Comparer
                If x Is y Then
                    Return 0
                ElseIf x.GetType().Equals(y.GetType()) Then
                    ' Comparing two items that ar ethe same type, see if type supports IComparable interface
                    If TypeOf x Is IComparable Then
                        Return DirectCast(x, IComparable).CompareTo(y)
                    ElseIf x.Equals(y) Then
                        Return 0
                    Else
                        Return Comparer.Default.Compare(x, y)
                    End If
                Else
                    Return Comparer.Default.Compare(x, y)
                End If
            Else
                ' Compare non-reference (i.e., value) types using VB rules
                ' ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.en/dv_vbalr/html/d6cb12a8-e52e-46a7-8aaf-f804d634a825.htm
                Return IIf(x < y, -1, IIf(x > y, 1, 0))
            End If

        End Function

        ''' <summary>Compares two arrays</summary>
        Public Shared Function CompareArrays(ByVal arrayA As Array, ByVal arrayB As Array) As Integer

            Return CompareArrays(arrayA, arrayB, Nothing)

        End Function

        ''' <summary>Compares two arrays</summary>
        Public Shared Function CompareArrays(ByVal arrayA As Array, ByVal arrayB As Array, ByVal comparer As IComparer) As Integer

            If arrayA Is Nothing And arrayB Is Nothing Then
                Return 0
            ElseIf arrayA Is Nothing Then
                Return -1
            ElseIf arrayB Is Nothing Then
                Return 1
            Else
                If arrayA.Rank = 1 And arrayB.Rank = 1 Then
                    If arrayA.GetUpperBound(0) = arrayB.GetUpperBound(0) Then
                        Dim comparison As Integer

                        For x As Integer = 0 To arrayA.Length - 1
                            If comparer Is Nothing Then
                                comparison = Compare(arrayA.GetValue(x), arrayB.GetValue(x))
                            Else
                                comparison = comparer.Compare(arrayA.GetValue(x), arrayB.GetValue(x))
                            End If

                            If comparison <> 0 Then Exit For
                        Next

                        Return comparison
                    Else
                        ' For arrays that don't have the same number of elements, array with most elements is assumed larger
                        Return Compare(arrayA.GetUpperBound(0), arrayB.GetUpperBound(0))
                    End If
                Else
                    Throw New ArgumentException("Cannot compare multidimensional arrays")
                End If
            End If

        End Function

        ''' <summary>Changes the type of all the elements in source enumeration and adds the conversion result to destination list</summary>
        ''' <remarks>Converted items in source enumeration are added to destination list - destination list is not cleared in advance</remarks>
        Public Shared Sub ConvertList(ByVal source As IEnumerable, ByVal destination As IList, ByVal toType As System.Type)

            If source Is Nothing Then Throw New ArgumentNullException("Source list is null")
            If destination Is Nothing Then Throw New ArgumentNullException("Destination list is null")
            If destination.IsReadOnly Then Throw New ArgumentException("Cannot add items to a read only list")
            If destination.IsFixedSize Then Throw New ArgumentException("Cannot add items to a fixed size list")

            For Each Item As Object In source
                destination.Add(Convert.ChangeType(Item, toType))
            Next

        End Sub

        ''' <summary>Converts a list (i.e., any collection implementing IList) to an array</summary>
        Public Shared Function ListToArray(ByVal sourceList As IList, ByVal toType As System.Type) As Array

            Dim destination As Array = Array.CreateInstance(toType, sourceList.Count)

            ConvertList(sourceList, destination, toType)

            Return destination

        End Function

        ''' <summary>Converts an array to a string using the default delimeter, "|", that can later be converted back to array using StringToArray</summary>
        ''' <remarks>
        ''' This function is just a semantic reference to the ListToString function (the Array class implements IEnumerable)
        ''' and is only provided for the sake of completeness
        ''' </remarks>
        Public Shared Function ArrayToString(ByVal source As Array) As String

            Return ListToString(source)

        End Function

        ''' <summary>Converts an array to a string that can later be converted back to array using StringToArray</summary>
        ''' <remarks>
        ''' This function is just a semantic reference to the ListToString function (the Array class implements IEnumerable)
        ''' and is only provided for the sake of completeness
        ''' </remarks>
        Public Shared Function ArrayToString(ByVal source As Array, ByVal delimeter As Char) As String

            Return ListToString(source, delimeter)

        End Function

        ''' <summary>Converts an enumeration to a string using the default delimeter, "|", that can later be converted back to array using StringToList</summary>
        Public Shared Function ListToString(ByVal source As IEnumerable) As String

            Return ListToString(source, "|"c)

        End Function

        ''' <summary>Converts an enumeration to a string that can later be converted back to array using StringToList</summary>
        Public Shared Function ListToString(ByVal source As IEnumerable, ByVal delimeter As Char) As String

            If source Is Nothing Then Throw New ArgumentNullException("Source list is null")

            With New StringBuilder
                For Each item As Object In source
                    If .Length > 0 Then .Append(delimeter)
                    .Append(item.ToString())
                Next

                Return .ToString()
            End With

        End Function

        ''' <summary>Converts a string, created with ArrayToString, using the default delimeter, "|", back into an array</summary>
        Public Shared Function StringToArray(ByVal source As String, ByVal toType As System.Type) As Array

            Return StringToArray(source, toType, "|"c)

        End Function

        ''' <summary>Converts a string, created with ArrayToString, back into an array</summary>
        Public Shared Function StringToArray(ByVal source As String, ByVal toType As System.Type, ByVal delimeter As Char) As Array

            Dim items As New ArrayList

            StringToList(source, items, delimeter)

            Return ListToArray(items, toType)

        End Function

        ''' <summary>Appends items parsed from delimited string, created with ArrayToString or ListToString, using the default delimeter, "|",  into the given list</summary>
        ''' <remarks>Converted items are added to destination list - destination list is not cleared in advance</remarks>
        Public Shared Sub StringToList(ByVal source As String, ByVal destination As IList)

            StringToList(source, destination, "|"c)

        End Sub

        ''' <summary>Appends items parsed from delimited string, created with ArrayToString or ListToString, into the given list</summary>
        ''' <remarks>Converted items are added to destination list - destination list is not cleared in advance</remarks>
        Public Shared Sub StringToList(ByVal source As String, ByVal destination As IList, ByVal delimeter As Char)

            If source Is Nothing Then Exit Sub
            If destination Is Nothing Then Throw New ArgumentNullException("Destination list is null")
            If destination.IsFixedSize Then Throw New ArgumentException("Cannot add items to a fixed size list")
            If destination.IsReadOnly Then Throw New ArgumentException("Cannot add items to a read only list")

            For Each item As String In source.Split(delimeter)
                If Not String.IsNullOrEmpty(item) Then
                    item = item.Trim
                    If item.Length > 0 Then destination.Add(item)
                End If
            Next

        End Sub

        ''' <summary>Rearranges all the elements in the array into a random order</summary>
        ''' <remarks>
        ''' <para>
        ''' This function is just a semantic reference to the ScrambleList function (the Array class implements IList)
        ''' and is only provided for the sake of completeness
        ''' </para>
        ''' <para>This function uses a cryptographically strong random number generator to perform the scramble</para>
        ''' </remarks>
        Public Shared Sub ScrambleArray(ByVal source As Array)

            ScrambleList(source)

        End Sub

        ''' <summary>Rearranges all the elements in the list (i.e., any collection implementing IList) into a random order</summary>
        ''' <remarks>This function uses a cryptographically strong random number generator to perform the scramble</remarks>
        Public Shared Sub ScrambleList(ByVal source As IList)

            If source Is Nothing Then Throw New ArgumentNullException("Source list is null")
            If source.IsReadOnly Then Throw New ArgumentException("Cannot modify items in a read only list")

            Dim x, y As Integer
            Dim currentItem As Object

            ' Mix up the data in random order
            For x = 0 To source.Count - 1
                ' Call random function in Math namespace
                y = RandomInt32Between(0, source.Count - 1)

                If x <> y Then
                    ' Swap items
                    currentItem = source(x)
                    source(x) = source(y)
                    source(y) = currentItem
                End If
            Next

        End Sub

        ''' <summary>Determines if given item is an object (i.e., a reference type) but not a string</summary>
        Public Shared Function IsNonStringReference(ByVal item As Object) As Boolean

            Return (IsReference(item) And Not TypeOf item Is String)

        End Function

    End Class

End Namespace
