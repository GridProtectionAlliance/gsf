' James Ritchie Carroll - 2003
' 01/10/2005 JRC - Added fast "Concat" function

Option Explicit On 

Imports System.Text

Namespace [Shared]

    ''' <summary>
    ''' Defines common global functions related to String manipulation
    ''' </summary>
    Public Class [String]

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        '''<summary>
        '''<para>Does a "fast" concat:
        '''<see cref="System.String.Concat" />
        '''is very slow 
        '''</para>
        '''<seealso cref="http://www.developer.com/net/cplus/article.php/3304901"/>
        '''</summary>
        '''<param name="args">String parameters to concatenate, passed by value</param>
        ''' <returns>
        ''' <para>The concatenated string representations of the values of the   elements in <paramref name="args" />.</para>
        '''</returns>
        Public Shared Function Concat(ByVal ParamArray args As System.String()) As System.String

            If args Is Nothing Then
                Return ""
            Else
                With New StringBuilder
                    For x As Integer = 0 To args.Length - 1
                        .Append(args(x))
                    Next

                    Return .ToString
                End With
            End If

        End Function
        '''<summary> 
        '''Ensures parameter is not an empty string 
        '''</summary>
        '''<param name="TestData"> a parameter value  </param>
        '''<param name="NotReturnValue"> String  returned by the function,optional </param>
        ''' <returns>
        ''' <para>A non-empty string </para>
        '''</returns>
        Public Shared Function NotEmpty(ByVal TestData As Object, Optional ByVal NotReturnValue As String = " ") As String

            If IsDBNull(TestData) Or IsNothing(TestData) Then
                Return NotReturnValue
            ElseIf Len(CStr(TestData)) = 0 Then
                Return NotReturnValue
            Else
                Return CStr(TestData)
            End If

        End Function
        '''<summary>
        ''' Ensures parameter passed to a function is not zero
        '''</summary>
        '''<param name="TestData"> a parameter value  </param>
        '''<param name="NotReturnValue"> An optional value for the string to be returned by the function </param>
        ''' <returns>
        ''' <para>A non-zero object </para>
        '''</returns>
        Public Shared Function NotZero(ByVal TestData As Object, Optional ByVal NotReturnValue As Object = -1) As Object
            Return IIf(Val(TestData) = 0, NotReturnValue, TestData)
        End Function
        '''<summary>
        ''' Ensures parameter is not Null
        '''</summary>
        '''<param name="TestData"> a parameter value  </param>
        '''<param name="NotReturnValue">Optional Value</param>

        Public Shared Function NotNull(ByVal TestData As Object, Optional ByVal NotReturnValue As Object = "") As Object
            Return IIf(IsDBNull(TestData), NotReturnValue, TestData)
        End Function

        '''<summary>
        '''<para> Pads a value to the left given number of pad characters (optional pad character).</para>
        '''</summary>
        '''<param name="PadData">Parameter value to pad data to its left </param>
        '''<param name="PadLen"> Padding length</param>
        '''<param name="PadChar"> An optional value for a Unicode padding character</param>
        '''   <returns>
        '''<para> A new string that is equivalent to this instance, but right-aligned and padded on the left with as many spaces as
        '''  specified in <paramref name="PadLen" />.</para>
        ''' </returns>

        Public Shared Function PadLeft(ByVal PadData As Object, ByVal PadLen As Long, Optional ByVal PadChar As Char = " "c) As String

            Return NotEmpty(PadData, "").PadLeft(PadLen, PadChar)

        End Function

        '''<summary>
        '''<para> Pads a value to the right given number of pad characters (optional pad character.</para>
        '''</summary>
        '''<param name="PadData"> a parameter value for padding data to its right </param>
        '''<param name="PadLen"> A parameter value for padding length</param>
        '''<param name="PadChar"> An optional value for a Unicode padding character</param>
        ''' <returns>
        '''<para> A new string that is equivalent to this instance, but left-aligned and padded on the right with as many spaces as
        '''  specified in <paramref name="PadLen" />.</para>
        ''' </returns>
        Public Shared Function PadRight(ByVal PadData As Object, ByVal PadLen As Long, Optional ByVal PadChar As Char = " "c) As String

            Return NotEmpty(PadData, "").PadRight(PadLen, PadChar)

        End Function
        '''<summary>
        ''' <para>
        ''' Peforms capitization on a word or sentence
        ''' </para>
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<param name="FirstWordOnly"> A boolean value for checking if the first letter of the input paramter is already in Uppercase, optional</param>
        ''' <returns>
        '''<para> A word or sentence with the first letter Capitalized<paramref name="Str" />.</para>
        ''' </returns>
        Public Shared Function Proper(ByVal Str As System.String, Optional ByVal FirstWordOnly As Boolean = False) As String
            If FirstWordOnly Then
                Return UCase(Left(Str, 1)) & LCase(Mid(Str, 2))
            Else
                Return StrConv(Str, VbStrConv.ProperCase)
            End If
        End Function
        '''<summary>
        ''' Returns a string with all of the duplicates of the specified string removed
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<param name="DupString"> Duplicate String</param>

        Public Shared Function RemoveDuplicates(ByVal Str As System.String, ByVal DupString As System.String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As String
            Do While InStr(1, Str, DupString & DupString, Compare) > 0
                Str = Replace(Str, DupString & DupString, DupString, Compare)
            Loop
            Return Str
        End Function
        '''<summary>
        ''' Removes terminator (Chr(0) from a null terminated string) - useful for strings returned from API
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        Public Shared Function RemoveNull(ByVal Str As System.String) As String
            Dim lngNullPos As Long
            If Len(Str) > 0 Then
                lngNullPos = InStr(Str, Chr(0))
                If lngNullPos > 0 Then
                    Return Left(Str, lngNullPos - 1)
                Else
                    Return Str
                End If
            End If
        End Function
        '''<summary>
        ''' Removes all carriage return/line feeds from a string
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<returns>
        '''<para> A String with no carriage return/line feeds<paramref name="Str" />.</para>
        ''' </returns>
        Public Shared Function RemoveCrLf(ByVal Str As System.String) As String

            Return Replace(Replace(Str, vbCr, ""), vbLf, "")

        End Function
        '''<summary>
        '''  Returns a string with all white space removed
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<returns>
        '''<para> A String with no white spaces<paramref name="Str" />.</para>
        ''' </returns>

        Public Shared Function RemoveWhiteSpace(ByVal Str As System.String) As String

            Dim x As Long
            Dim strOut As New StringBuilder
            Dim chrCurr As Char

            For x = 0 To Len(Str) - 1
                ' Get current character
                chrCurr = Str.Chars(x)

                If Not Char.IsWhiteSpace(chrCurr) Then
                    strOut.Append(chrCurr)
                End If
            Next

            Return strOut.ToString()

        End Function
        '''<summary>
        '''  Returns the "nth" sub string from a delimited string
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<param name="ItemNum"> Required.nth delimiter position</param>
        '''<param name="Delimiter"> Required.Delimiter string  </param>
        Public Shared Function SubStr(ByVal Str As System.String, ByVal ItemNum As Long, ByVal Delimiter As System.String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As String

            Dim lngStart As Long
            Dim lngStop As Long

            If InStr(1, Str, Delimiter, Compare) > 0 Then
                If ItemNum > 1 Then
                    lngStart = GetDelimiterPos(Str, ItemNum - 1, Delimiter, Compare) + 1
                Else
                    lngStart = 1
                End If

                lngStop = GetDelimiterPos(Str, ItemNum, Delimiter, Compare) - 1

                If lngStop < 1 And ItemNum > 1 Then
                    lngStop = Len(Str)
                End If

                If lngStop > 0 Then
                    Return Mid(Str, lngStart, lngStop - lngStart + 1)
                Else
                    Return ""
                End If
            Else
                If ItemNum = 1 Then
                    Return Str
                Else
                    Return ""
                End If
            End If

        End Function
        '''<summary>
        '''   <para>Returns the string position of the specified "nth" delimiter in a string.</para>
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<param name="nthDelimiter"> Required.nth delimiter position</param>
        '''<param name="Delimiter"> Required.Delimiter string  </param>  
        '''<param name="Compare"> Optional for binary comparison  </param>  

        Public Shared Function GetDelimiterPos(ByVal Str As System.String, ByVal nthDelimiter As Long, ByVal Delimiter As System.String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As Long

            Dim x As Long
            Dim lngPos As Long

            x = 1
            lngPos = InStr(1, Str, Delimiter, Compare)

            Do While lngPos <> 0 And x <> nthDelimiter
                lngPos = InStr(lngPos + 1, Str, Delimiter, Compare)
                x = x + 1
            Loop

            Return lngPos

        End Function
        '''<summary>
        '''   <para>Returns count of the number of occurences of a sub string within a string.</para>
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<param name="SubString"> Required.Substring sought</param>
        '''<param name="Compare"> Optional for binary comparison  </param>  
        Public Shared Function SubStrCount(ByVal Str As System.String, ByVal SubString As System.String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As Long
            Dim lngPos As Long
            Dim lngCount As Long
            lngPos = 1
            lngCount = 0
            lngPos = InStr(lngPos, Str, SubString, Compare)
            Do While lngPos > 0
                lngCount = lngCount + 1
                lngPos = InStr(lngPos + 1, Str, SubString, Compare)
            Loop
            Return lngCount
        End Function
        '''<summary>
        '''   <para>Returns the number of occurances of the specified character in a string.</para>
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<param name="CharToCount"> Required.Character sought.</param>

        Public Shared Function CharCount(ByVal Str As System.String, ByVal CharToCount As Char) As Long

            Dim x As Long
            Dim lngCount As Long

            For x = 0 To Len(Str) - 1
                If Str.Chars(x) = CharToCount Then lngCount += 1
            Next

            Return lngCount

        End Function
        '''<summary>
        '''   <para>Extracts the specified characters from a string.</para>
        '''</summary>
        '''<param name="Str"> Required. Any valid String or Char expression. </param>
        '''<param name="CharsToExtract"> Required.Character to be extracted.</param>
        Public Shared Sub ExtractChars(ByVal Str As System.String, ByVal CharsToExtract As System.String, ByRef ExtractedChars As System.String, ByRef RemainingChars As System.String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary)

            Dim x As Long
            Dim chrCurr As Char
            Dim strRemaining As New StringBuilder
            Dim strExtracted As New StringBuilder

            For x = 0 To Len(Str) - 1
                chrCurr = Str.Chars(x)

                If InStr(1, CharsToExtract, chrCurr, Compare) = 0 Then
                    strRemaining.Append(chrCurr)
                Else
                    strExtracted.Append(chrCurr)
                End If
            Next

            ExtractedChars = strExtracted.ToString()
            RemainingChars = strRemaining.ToString()

        End Sub
        '''<summary>
        '''   <para>Returns ASC(ANSI character code) regardless of parameter type.</para>
        '''</summary>
        '''<param name="KeyCode"> Required. Any valid object. </param>

        Public Shared Function GetCharCode(ByVal KeyCode As Object) As Integer

            If TypeOf KeyCode Is String Then
                ' User passed string value
                Return Asc(Left(KeyCode, 1))
            ElseIf TypeOf KeyCode Is Char Then
                ' User passed character value
                Return Asc(KeyCode)
            Else
                ' User passed value
                Return CInt(Val(KeyCode))
            End If

        End Function
        '''<summary>
        '''   <para></para>
        '''</summary>
        '''<param name="Data"> Required. Data of datatype Byte. </param>
        Public Shared Function HexEncode(ByVal Data As Byte()) As String

            Return HexEncode(Data, 0, Data.Length)

        End Function

        Public Shared Function HexEncode(ByVal Data As Byte(), ByVal offset As Integer, ByVal length As Integer) As String

            With New StringBuilder
                For x As Integer = 0 To length - 1
                    .Append(Data(offset + x).ToString("x"c).PadLeft(2, "0"c))
                Next

                Return .ToString()
            End With

        End Function
        '''<summary>
        '''   <para>Returns .</para>
        '''</summary>
        '''<param name="Str"> Required. String. </param>
        Public Shared Function HexEncodeStr(ByVal Str As System.String) As String

            Return HexEncode(Encoding.Unicode.GetBytes(Str))

        End Function
        '''<summary>
        '''   <para>.</para>
        '''</summary>
        '''<param name="Str"> Required. String. </param>
        Public Shared Function HexDecode(ByVal Str As System.String) As Byte()

            Dim Data As Byte() = Array.CreateInstance(GetType(Byte), CType(Len(Str) / 2, Integer))
            Dim x, y As Integer

            For x = 0 To Len(Str) - 1 Step 2
                Data(y) = Convert.ToByte(Str.Substring(x, 2), 16)
                y += 1
            Next

            Return Data

        End Function
        '''<summary>
        '''   <para></para>
        '''</summary>
        '''<param name="Str"> Required. String. </param>
        Public Shared Function HexDecodeStr(ByVal Str As System.String) As String

            Return Encoding.Unicode.GetString(HexDecode(Str))

        End Function
        '''<summary>
        '''   <para>Converts binary data to String</para>
        '''</summary>
        '''<param name="Data"> Required. Parameter of Byte datatype. </param>
        Public Shared Function Base64Encode(ByVal Data As Byte()) As String

            Return Convert.ToBase64String(Data)

        End Function
        '''<summary>
        '''   <para>Encodes a string to base64</para>
        '''</summary>
        '''<param name="Str"> Required. String parameter. </param>
        Public Shared Function Base64EncodeStr(ByVal Str As System.String) As String

            Return Base64Encode(Encoding.Unicode.GetBytes(Str))

        End Function
        '''<summary>
        '''   <para>Converts String to binary</para>
        '''</summary>
        '''<param name="Str"> Required. String parameter. </param>
        Public Shared Function Base64Decode(ByVal Str As System.String) As Byte()

            Return Convert.FromBase64String(Str)

        End Function
        '''<summary>
        '''   <para>Converts base64 string to an ASCII string</para>
        '''</summary>
        '''<param name="Str"> Required. String parameter. </param>
        Public Shared Function Base64DecodeStr(ByVal Str As System.String) As String

            Return Encoding.Unicode.GetString(Base64Decode(Str))

        End Function

    End Class

End Namespace