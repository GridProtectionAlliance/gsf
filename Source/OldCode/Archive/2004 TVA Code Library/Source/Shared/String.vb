' James Ritchie Carroll - 2003
' 01/10/2005 JRC - Added fast "Concat" function

Option Explicit On 

Imports System.Text

Namespace [Shared]

    ' Common String Functions
    Public Class [String]

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Does a "fast" concat - System.String.Concat is very slow (http://www.developer.com/net/cplus/article.php/3304901)
        Public Shared Function Concat(ByVal ParamArray args As String()) As String

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

        ' Ensures parameter is not an empty string
        Public Shared Function NotEmpty(ByVal TestData As Object, Optional ByVal NotReturnValue As String = " ") As String

            If IsDBNull(TestData) Or IsNothing(TestData) Then
                Return NotReturnValue
            ElseIf Len(CStr(TestData)) = 0 Then
                Return NotReturnValue
            Else
                Return CStr(TestData)
            End If

        End Function

        ' Ensures parameter is not zero
        Public Shared Function NotZero(ByVal TestData As Object, Optional ByVal NotReturnValue As Object = -1) As Object

            Return IIf(Val(TestData) = 0, NotReturnValue, TestData)

        End Function

        ' Ensures parameter is not Null
        Public Shared Function NotNull(ByVal TestData As Object, Optional ByVal NotReturnValue As Object = "") As Object

            Return IIf(IsDBNull(TestData), NotReturnValue, TestData)

        End Function

        ' Pads a value to the left given number of pad characters (optional pad character)
        Public Shared Function PadLeft(ByVal PadData As Object, ByVal PadLen As Long, Optional ByVal PadChar As Char = " "c) As String

            Return NotEmpty(PadData, "").PadLeft(PadLen, PadChar)

        End Function

        ' Pads a value to the right given number of pad characters (optional pad character)
        Public Shared Function PadRight(ByVal PadData As Object, ByVal PadLen As Long, Optional ByVal PadChar As Char = " "c) As String

            Return NotEmpty(PadData, "").PadRight(PadLen, PadChar)

        End Function

        ' Peforms capitization on a word or sentence
        Public Shared Function Proper(ByVal Str As String, Optional ByVal FirstWordOnly As Boolean = False) As String

            If FirstWordOnly Then
                Return UCase(Left(Str, 1)) & LCase(Mid(Str, 2))
            Else
                Return StrConv(Str, VbStrConv.ProperCase)
            End If

        End Function

        ' Returns a string with all of the duplicates of the specified string removed
        Public Shared Function RemoveDuplicates(ByVal Str As String, ByVal DupString As String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As String

            Do While InStr(1, Str, DupString & DupString, Compare) > 0
                Str = Replace(Str, DupString & DupString, DupString, Compare)
            Loop

            Return Str

        End Function

        ' Removes terminator (Chr(0) from a null terminated string) - useful for strings returned from API
        Public Shared Function RemoveNull(ByVal Str As String) As String

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

        ' Removes all carriage return/line feeds from a string
        Public Shared Function RemoveCrLf(ByVal Str As String) As String

            Return Replace(Replace(Str, vbCr, ""), vbLf, "")

        End Function

        ' Returns a string with all white space removed
        Public Shared Function RemoveWhiteSpace(ByVal Str As String) As String

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

        ' Returns the "nth" sub string from a delimited string
        Public Shared Function SubStr(ByVal Str As String, ByVal ItemNum As Long, ByVal Delimiter As String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As String

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

        ' Returns the string position of the specified "nth" delimiter in a string
        Public Shared Function GetDelimiterPos(ByVal Str As String, ByVal nthDelimiter As Long, ByVal Delimiter As String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As Long

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

        ' Returns count of the number of occurances of a sub string within a string
        Public Shared Function SubStrCount(ByVal Str As String, ByVal SubString As String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary) As Long

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

        ' Returns the number of occurances of the specified character in a string
        Public Shared Function CharCount(ByVal Str As String, ByVal CharToCount As Char) As Long

            Dim x As Long
            Dim lngCount As Long

            For x = 0 To Len(Str) - 1
                If Str.Chars(x) = CharToCount Then lngCount += 1
            Next

            Return lngCount

        End Function

        ' Extracts the specified characters from a string
        Public Shared Sub ExtractChars(ByVal Str As String, ByVal CharsToExtract As String, ByRef ExtractedChars As String, ByRef RemainingChars As String, Optional ByVal Compare As CompareMethod = CompareMethod.Binary)

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

        ' Returns ASC regardless of parameter type
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

        Public Shared Function HexEncode(ByVal Data As Byte()) As String

            Dim strHex As New StringBuilder
            Dim x As Integer

            For x = 0 To Data.Length - 1
                strHex.Append(Data(x).ToString("x"c).PadLeft(2, "0"c))
            Next

            Return strHex.ToString()

        End Function

        Public Shared Function HexEncodeStr(ByVal Str As String) As String

            Return HexEncode(Encoding.Unicode.GetBytes(Str))

        End Function

        Public Shared Function HexDecode(ByVal Str As String) As Byte()

            Dim Data As Byte() = Array.CreateInstance(GetType(Byte), CType(Len(Str) / 2, Integer))
            Dim x, y As Integer

            For x = 0 To Len(Str) - 1 Step 2
                Data(y) = Convert.ToByte(Str.Substring(x, 2), 16)
                y += 1
            Next

            Return Data

        End Function

        Public Shared Function HexDecodeStr(ByVal Str As String) As String

            Return Encoding.Unicode.GetString(HexDecode(Str))

        End Function

        Public Shared Function Base64Encode(ByVal Data As Byte()) As String

            Return Convert.ToBase64String(Data)

        End Function

        Public Shared Function Base64EncodeStr(ByVal Str As String) As String

            Return Base64Encode(Encoding.Unicode.GetBytes(Str))

        End Function

        Public Shared Function Base64Decode(ByVal Str As String) As Byte()

            Return Convert.FromBase64String(Str)

        End Function

        Public Shared Function Base64DecodeStr(ByVal Str As String) As String

            Return Encoding.Unicode.GetString(Base64Decode(Str))

        End Function


    End Class

End Namespace