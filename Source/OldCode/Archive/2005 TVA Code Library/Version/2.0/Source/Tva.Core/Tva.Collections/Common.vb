'*******************************************************************************************************
'  Tva.Collections.Common.vb - Common Collection Functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
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

Namespace Collections

    ' TODO: Fix-up this class to meet new naming convention guidelines and replace use of native VB functions with standard .NET methods

    ''' <summary>
    ''' Defines common global functions related to manipulation of collections
    ''' </summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' <para> Returns True if specified item is in parameter list</para>
        ''' </summary>
        ''' <param name="Item"> Required. Specific Item to find in the Parameter Array. </param>
        Public Shared Function InList(ByVal item As Object, ByVal ParamArray itemList() As Object) As Boolean

            Return ArrayContains(item, itemList)

        End Function

        ''' <summary>
        ''' <para> Performs case-insensitive text comparisons to find a specified item in an array</para>
        ''' </summary>
        ''' <param name="Item"> Required. Specific string . </param>
        ''' <returns>
        ''' <para>True if the String exists in the parameter array</para>
        '''</returns>
        Public Shared Function StrInList(ByVal Item As System.String, ByVal ParamArray ItemList() As System.String) As Boolean

            Return ArrayContains(Item, ItemList, CaseInsensitiveComparer.Default)

        End Function

        ''' <summary>
        ''' <para> Returns smallest item from list of parameters</para>
        ''' </summary>
        Public Shared Function Minimum(ByVal ParamArray ItemList() As Object) As Object

            Return ArrayMinimum(ItemList)

        End Function

        ''' <summary>
        ''' <para> Returns largest item from list of parameters</para>
        ''' </summary>
        Public Shared Function Maximum(ByVal ParamArray ItemList() As Object) As Object

            Return ArrayMaximum(ItemList)

        End Function

        ''' <summary>
        ''' <para> Returns True if specified item exists in given array</para>
        ''' </summary>
        ''' <param name="Item"> Required. Array of items . </param>
        Public Shared Function ArrayContains(ByVal Item As Object, ByVal ArrayItems As IEnumerable, Optional ByVal Comparer As IComparer = Nothing) As Boolean

            With ArrayItems.GetEnumerator()
                While .MoveNext()
                    If Compare(Item, .Current, Comparer) = 0 Then Return True
                End While
            End With

            Return False

        End Function

        ''' <summary>
        ''' <para> Returns smallest item from the specified array given a comparer.</para>
        ''' </summary>
        ''' <param name="ArrayItems"> Required. Array of items . </param>
        ''' <param name="Comparer"> Optional. Comparer . </param>
        Public Shared Function ArrayMinimum(ByVal ArrayItems As IEnumerable, Optional ByVal Comparer As IComparer = Nothing) As Object

            Dim objMin As Object

            With ArrayItems.GetEnumerator()
                If .MoveNext() Then
                    objMin = .Current
                    While .MoveNext()
                        If Compare(.Current, objMin, Comparer) < 0 Then objMin = .Current
                    End While
                End If
            End With

            Return objMin

        End Function

        ''' <summary>
        ''' <para> Returns largest item from the specified array given a comparer.</para>
        ''' </summary>
        ''' <param name="ArrayItems"> Required. Array of items . </param>
        ''' <param name="Comparer"> Optional. Comparer . </param>
        Public Shared Function ArrayMaximum(ByVal ArrayItems As IEnumerable, Optional ByVal Comparer As IComparer = Nothing) As Object

            Dim objMax As Object

            With ArrayItems.GetEnumerator()
                If .MoveNext() Then
                    objMax = .Current
                    While .MoveNext()
                        If Compare(.Current, objMax, Comparer) > 0 Then objMax = .Current
                    End While
                End If
            End With

            Return objMax

        End Function

        ''' <summary>
        ''' <para> Compares two elements of any type.</para>
        ''' </summary>
        ''' <param name="x"> Required. Element to compare. </param>
        ''' <param name="y"> Required. Element to compare . </param>
        ''' <param name="Comparer"> Optional. Comparer . </param>
        Public Shared Function Compare(ByVal x As Object, ByVal y As Object, Optional ByVal Comparer As IComparer = Nothing) As Integer

            If Comparer Is Nothing Then
                If IsNonStringReference(x) And IsNonStringReference(y) Then
                    ' If both items are non-string reference objects then test object equality by reference,
                    ' then if not equal by overriable Object.Equals function use default Comparer
                    If x Is y Then
                        Return 0
                    ElseIf x.GetType().Equals(y.GetType()) Then
                        If TypeOf x Is IComparable Then
                            Return DirectCast(x, IComparable).CompareTo(y)
                        ElseIf x.Equals(y) Then
                            Return 0
                        Else
                            Return System.Collections.Comparer.Default.Compare(x, y)
                        End If
                    Else
                        Return System.Collections.Comparer.Default.Compare(x, y)
                    End If
                Else
                    ' Otherwise compare using VB rules (ms-help://MS.VSCC/MS.MSDNVS/vblr7/html/vagrpComparison.htm)
                    Return IIf(x < y, -1, IIf(x > y, 1, 0))
                End If
            Else
                ' Use given comparer to compare objects
                Return Comparer.Compare(x, y)
            End If

        End Function

        ''' <summary>
        ''' <para> Compares two arrays.</para>
        ''' </summary>
        ''' <param name="ArrA"> Required. Array to compare. </param>
        ''' <param name="ArrB"> Required. Array to compare . </param>
        ''' <param name="Comparer"> Optional. Comparer . </param>
        Public Shared Function CompareArrays(ByVal ArrA As Array, ByVal ArrB As Array, Optional ByVal Comparer As IComparer = Nothing) As Integer

            If ArrA Is Nothing And ArrB Is Nothing Then
                Return 0
            ElseIf ArrA Is Nothing Then
                Return -1
            ElseIf ArrB Is Nothing Then
                Return 1
            Else
                If ArrA.Rank = 1 And ArrB.Rank = 1 Then
                    If ArrA.GetUpperBound(0) = ArrB.GetUpperBound(0) Then
                        Dim intComparison As Integer

                        For x As Integer = 0 To ArrA.Length - 1
                            intComparison = Compare(ArrA.GetValue(x), ArrB.GetValue(x), Comparer)
                            If intComparison <> 0 Then Exit For
                        Next

                        Return intComparison
                    Else
                        ' For arrays that don't have the same number of elements, array with most elements is assumed larger
                        Return Compare(ArrA.GetUpperBound(0), ArrB.GetUpperBound(0))
                    End If
                Else
                    Throw New ArgumentException("Cannot compare multidimensional arrays")
                End If
            End If

        End Function

        ''' <summary>
        ''' <para> Changes the type of all the elements in source list and copies the conversion result to destination list.</para>
        ''' </summary>
        ''' <param name="Source"> Required. Source List. </param>
        ''' <param name="Destination"> Required. Destination List . </param>
        ''' <param name="ToType"> Required. specified type to change the source elements. </param>
        Public Shared Sub ConvertList(ByVal Source As IList, ByVal Destination As IList, ByVal ToType As System.Type)

            If Source Is Nothing Then Throw New ArgumentNullException("Source list is null")
            If Destination Is Nothing Then Throw New ArgumentNullException("Destination list is null")
            If Destination.IsReadOnly Then Throw New ArgumentException("Cannot copy items to a read only list")

            If Source.Count = Destination.Count Then
                For x As Integer = 0 To Source.Count - 1
                    Destination(x) = Convert.ChangeType(Source(x), ToType)
                Next
            Else
                ' The source and destination don't contain the same number of elements, so we'll add items to destination
                ConvertEnumeration(Source, Destination, ToType)
            End If

        End Sub

        ''' <summary>
        ''' <para> Changes the type of all the elements in source enumeration and adds the conversion result to destination list.</para>
        ''' </summary>
        ''' <param name="Source"> Required. Source Enumeration. </param>
        ''' <param name="Destination"> Required. Destination Enumeration . </param>
        ''' <param name="ToType"> Required. specified type to change the source elements. </param>
        Public Shared Sub ConvertEnumeration(ByVal Source As IEnumerable, ByVal Destination As IList, ByVal ToType As System.Type)

            If Source Is Nothing Then Throw New ArgumentNullException("Source list is null")
            If Destination Is Nothing Then Throw New ArgumentNullException("Destination list is null")
            If Destination.IsReadOnly Then Throw New ArgumentException("Cannot add items to a read only list")
            If Destination.IsFixedSize Then Throw New ArgumentException("Cannot add items to a fixed size list")

            For Each Item As Object In Source
                Destination.Add(Convert.ChangeType(Item, ToType))
            Next

        End Sub

        ''' <summary>
        ''' <para>  Converts an array and all of its elements to the specified type.</para>
        ''' </summary>
        ''' <param name="Source"> Required. Source Array. </param>
        ''' <param name="ToType"> Required. specified type to change the elements of source array. </param>
        Public Shared Function ConvertArray(ByVal Source As Array, ByVal ToType As System.Type) As Array

            ' This function is just a semantic reference - Array class implements IList
            Return ListToArray(Source, ToType)

        End Function

        ''' <summary>
        ''' <para>Converts a list to an array.</para>
        ''' </summary>
        ''' <param name="Source"> Required.List. </param>
        ''' <param name="ElementType"> Required. Type . </param>
        Public Shared Function ListToArray(ByVal Source As IList, ByVal ElementType As System.Type) As Array

            If Source Is Nothing Then Throw New ArgumentNullException("Source list is null")

            Dim Destination As Array = Array.CreateInstance(ElementType, Source.Count)

            ConvertList(Source, Destination, ElementType)

            Return Destination

        End Function

        ''' <summary>
        ''' <para>Converts an array to a string.</para>
        ''' </summary>
        ''' <param name="Source"> Required.Array. </param>
        Public Shared Function ArrayToString(ByVal Source As Array) As String

            ' This function is just a semantic reference - Array class implements IEnumerable
            Return ListToString(Source)

        End Function

        ''' <summary>
        ''' <para>Converts an enumeration to a string.</para>
        ''' </summary>
        ''' <param name="Source"> Required.Enumeration. </param>
        Public Shared Function ListToString(ByVal Source As IEnumerable) As String

            If Source Is Nothing Then Throw New ArgumentNullException("Source list is null")

            Dim strArray As New StringBuilder

            For Each Item As Object In Source
                If strArray.Length > 0 Then strArray.Append(","c)
                strArray.Append(Item.ToString())
            Next

            Return strArray.ToString()

        End Function

        ''' <summary>
        ''' <para> Converts a string (created with ArrayToString) back into an array.</para>
        ''' </summary>
        Public Shared Function StringToArray(ByVal Source As System.String, ByVal ElementType As System.Type) As Array

            Dim lstItems As New ArrayList

            StringToList(Source, lstItems)

            Return ListToArray(lstItems, ElementType)

        End Function

        ''' <summary>
        ''' <para>Converts a string (created with ArrayToString or ListToString) into the given list.</para>
        ''' </summary>
        ''' <param name="Source"> Required.String. </param>
        ''' <param name="Destination"> Required.List. </param>
        Public Shared Sub StringToList(ByVal Source As System.String, ByVal Destination As IList)

            If Source Is Nothing Then Exit Sub
            If Destination Is Nothing Then Throw New ArgumentNullException("Destination list is null")
            If Destination.IsFixedSize Then Throw New ArgumentException("Cannot add items to a fixed size list")
            If Destination.IsReadOnly Then Throw New ArgumentException("Cannot add items to a read only list")

            For Each strItem As String In Source.Split(","c)
                strItem = Trim(strItem)
                If Len(strItem) > 0 Then Destination.Add(strItem)
            Next

        End Sub

        ''' <summary>
        ''' <para>Rearranges all the elements in the array into a random order.</para>
        ''' </summary>
        ''' <param name="Source"> Required.Array. </param>
        Public Shared Sub ScrambleArray(ByVal Source As Array)

            ' This function is just a semantic reference - Array class implements IList
            ScrambleList(Source)

        End Sub

        ''' <summary>
        ''' <para>Rearranges all the elements in the list into a random order.</para>
        ''' </summary>
        ''' <param name="Source"> Required.List. </param>
        Public Shared Sub ScrambleList(ByVal Source As IList)

            If Source Is Nothing Then Throw New ArgumentNullException("Source list is null")
            If Source.IsReadOnly Then Throw New ArgumentException("Cannot modify items in a read only list")

            Dim x, y As Integer
            Dim currItem As Object

            ' Mix up the data in random order
            For x = 0 To Source.Count - 1
                y = CInt(Int(Rnd() * Source.Count))

                If x <> y Then
                    ' Swap items
                    currItem = Source(x)
                    Source(x) = Source(y)
                    Source(y) = currItem
                End If
            Next

        End Sub

        ''' <summary>
        ''' <para>Determines if given item is an object (i.e., a reference type but not a string).</para>
        ''' </summary>
        Public Shared Function IsNonStringReference(ByVal Item As Object) As Boolean

            Return (IsReference(Item) And Not TypeOf Item Is String)

        End Function

    End Class

End Namespace
