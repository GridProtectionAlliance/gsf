' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Threading
Imports System.Text
Imports System.Xml
Imports VB = Microsoft.VisualBasic

Namespace [Shared]

    ''' <summary>
    ''' <para> Global Functions</para>
    ''' </summary>
    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' <para> Returns True if specified item is in parameter list</para>
        ''' </summary>
        ''' <param name="Item"> Required. Specific Item to find in the Parameter Array. </param>
        ''' <param name="ItemList()"> Required. Parameter list.</param> 
        Public Shared Function InList(ByVal Item As System.Object, ByVal ParamArray ItemList() As System.Object) As Boolean

            Return ArrayContains(Item, ItemList)

        End Function

        ''' <summary>
        ''' <para> Performs case-insensitive text comparisons to find a specified item in an array</para>
        ''' </summary>
        ''' <param name="Item"> Required. Specific string . </param>
        ''' <param name="ItemList()"> Required. Parameter string list.</param> 
        ''' <returns>
        ''' <para>True if the String exists in the parameter array</para>
        '''</returns>
        Public Shared Function StrInList(ByVal Item As System.String, ByVal ParamArray ItemList() As System.String) As Boolean

            Return ArrayContains(Item, ItemList, CaseInsensitiveComparer.Default)

        End Function

        ''' <summary>
        ''' <para> Returns smallest item from list of parameters</para>
        ''' </summary>
        ''' <param name="Item"> Required. parameter list . </param>
        Public Shared Function Minimum(ByVal ParamArray ItemList() As System.Object) As Object

            Return ArrayMinimum(ItemList)

        End Function

        ''' <summary>
        ''' <para> Returns largest item from list of parameters</para>
        ''' </summary>
        ''' <param name="Item"> Required. parameter list . </param>
        Public Shared Function Maximum(ByVal ParamArray ItemList() As System.Object) As Object

            Return ArrayMaximum(ItemList)

        End Function

        ''' <summary>
        ''' <para> Returns True if specified item exists in given array</para>
        ''' </summary>
        ''' <param name="Item"> Required. Array of items . </param>
        Public Shared Function ArrayContains(ByVal Item As System.Object, ByVal ArrayItems As IEnumerable, Optional ByVal Comparer As IComparer = Nothing) As Boolean

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
        Public Shared Function Compare(ByVal x As System.Object, ByVal y As System.Object, Optional ByVal Comparer As IComparer = Nothing) As Integer

            If Comparer Is Nothing Then
                If IsObject(x) And IsObject(y) Then
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
        ''' <param name="Source"> Required.Enumeration. </param>
        ''' <param name="Source"> Required.Enumeration. </param>
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
        ''' <param name="Source"> Required.Item. </param>
        Public Shared Function IsObject(ByVal Item As System.Object) As Boolean

            Return (IsReference(Item) And Not TypeOf Item Is String)

        End Function

        ''' <summary>
        ''' <para>Pauses execution for specified number of seconds.</para>
        ''' </summary>
        ''' <param name="DelaySeconds"> Required.Specific number of seconds. </param>
        Public Shared Sub Delay(ByVal DelaySeconds As System.Single)

            Dim sngStart As Single

            sngStart = VB.Timer

            Do While VB.Timer < sngStart + DelaySeconds
                ' Yield to other system threads...
                Thread.Sleep(0)
            Loop

        End Sub

        ''' <summary>
        ''' <para>Returns only assembly name and version from full assembly name.</para>
        ''' </summary>
        ''' <param name="AssemblyInstance"> Required.Assembly Instance. </param>
        Public Shared Function GetShortAssemblyName(ByVal AssemblyInstance As System.Reflection.Assembly) As String

            Dim strFullName As String = AssemblyInstance.FullName
            Dim intCulPos As Integer = InStr(strFullName, ", Culture=", CompareMethod.Text)

            If intCulPos > 0 Then
                Return Left(strFullName, intCulPos - 1)
            Else
                Return strFullName
            End If

        End Function

        ''' <summary>
        ''' <para> Performs a standard format, but returns blank string if no value is specified.</para>
        ''' </summary>
        ''' <param name="Expression"> Required.Expression. </param>
        ''' <param name="Style"> Optional.Format. </param>
        Public Shared Function FormatValue(ByVal Expression As Object, Optional ByVal Style As System.String = "MM/dd/yyyy") As String

            If IsDBNull(Expression) Then
                Return ""
            Else
                If Expression Is Nothing Then
                    Return ""
                ElseIf Len(CStr(Expression)) = 0 Then
                    Return ""
                Else
                    Return Format(Expression, Style)
                End If
            End If

        End Function

        ''' <summary>
        ''' <para> Convert a string to a byte array.</para>
        ''' </summary>
        ''' <param name="Str"> Required.String. </param>
        ''' <param name="OutputAsUnicode"> Optional.True. </param>
        Public Shared Function GetByteArrayFromString(ByVal Str As System.String, Optional ByVal OutputAsUnicode As Boolean = True) As Byte()

            If OutputAsUnicode Then
                Return Encoding.Unicode.GetBytes(Str)
            Else
                Return Encoding.ASCII.GetBytes(Str)
            End If

        End Function

        ''' <summary>
        ''' <para>  Convert a byte array to a string.</para>
        ''' </summary>
        ''' <param name="Bytes"> Required.String. </param>
        ''' <param name="InputIsUnicode"> Optional.True. </param>
        Public Shared Function GetStringFromByteArray(ByVal Bytes As Byte(), Optional ByVal InputIsUnicode As Boolean = True) As String

            If InputIsUnicode Then
                Return Encoding.Unicode.GetString(Bytes)
            Else
                Return Encoding.ASCII.GetString(Bytes)
            End If

        End Function

        ''' <summary>
        ''' <para>Returns a buffer of the specified length.</para>
        ''' </summary>
        ''' <param name="Buffer"> Required.Buffer. </param>
        ''' <param name="Length">Length of buffer </param>
        Public Shared Function TruncateBuffer(ByVal Buffer As Byte(), ByVal Length As Integer) As Byte()

            If Buffer.Length = Length Then
                Return Buffer
            Else
                Dim newBuffer As Byte() = Array.CreateInstance(GetType(Byte), Length)
                Array.Copy(Buffer, 0, newBuffer, 0, Length)
                Return newBuffer
            End If

        End Function

        ''' <summary>
        ''' <para>Return the Unicode number for a character in proper Regular Expression format.</para>
        ''' </summary>
        ''' <param name="Item"> Required.Character. </param>
        Public Shared Function GetRegexUnicodeChar(ByVal Item As Char) As String

            Return "\u" & AscW(Item).ToString("x"c).PadLeft(4, "0"c)

        End Function

        ''' <summary>
        ''' <para>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</para>
        ''' </summary>
        ''' <param name="parentNode"> Required.Node. </param>
        ''' <param name="xpath"> Required.Path. </param>
        ''' <param name="IsDirty"> Optional.Boolean value. </param>
        Public Shared Function GetXmlNode(ByVal parentNode As XmlNode, ByVal xpath As System.String, Optional ByRef IsDirty As Boolean = False) As XmlNode

            Dim node As XmlNode
            Dim element As String
            Dim elements As String()

            ' Remove any slash prefixes
            While Left(xpath, 1) = "/"
                xpath = Mid(xpath, 2)
            End While

            elements = xpath.Split("/"c)

            If elements.Length = 1 Then
                With parentNode.SelectNodes(xpath)
                    If .Count = 0 Then
                        node = parentNode.OwnerDocument.CreateElement(xpath)
                        parentNode.AppendChild(node)
                        IsDirty = True
                    Else
                        node = .Item(0)
                    End If
                End With
            Else
                For Each element In elements
                    node = GetXmlNode(parentNode, element)
                    parentNode = node
                Next
            End If

            Return node

        End Function

        ''' <summary>
        ''' <para>This overload just allows start given xml document by using its root element.</para>
        ''' </summary>
        ''' <param name="xmlDoc"> Required.xml document. </param>
        ''' <param name="xpath"> Required.Path. </param>
        ''' <param name="IsDirty"> Optional.Boolean value. </param>
        Public Shared Function GetXmlNode(ByVal xmlDoc As XmlDocument, ByVal xpath As System.String, Optional ByRef IsDirty As Boolean = False) As XmlNode

            Return GetXmlNode(xmlDoc.DocumentElement, xpath, IsDirty)

        End Function

        ''' <summary>
        ''' <para>Safely gets or sets an XML node's attribute, creating it if needed.</para>
        ''' </summary>
        ''' <param name="node"> Required.xml node. </param>
        ''' <param name="name"> Required.String. </param>
        Public Shared Property Attribute(ByVal node As XmlNode, ByVal name As System.String) As String
            Get
                Dim attr As XmlAttribute = node.Attributes(name)
                If attr Is Nothing Then
                    Return Nothing
                Else
                    Return attr.Value
                End If
            End Get
            Set(ByVal Value As String)
                Dim attr As XmlAttribute = node.Attributes(name)

                If attr Is Nothing Then
                    attr = node.OwnerDocument.CreateAttribute(name)
                    node.Attributes.Append(attr)
                End If

                If Not attr Is Nothing Then
                    attr.Value = Value
                    node.Attributes.SetNamedItem(attr)
                End If
            End Set
        End Property

    End Class

End Namespace