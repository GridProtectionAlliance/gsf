'*******************************************************************************************************
'  TVA.Text.Common.vb - Common Text Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - J. Ritchie Carroll
'       Original version of source code generated
'  01/24/2006 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.String)
'  06/01/2006 - J. Ritchie Carroll
'       Added ParseBoolean function to parse strings representing booleans that may be numeric
'  07/07/2006 - J. Ritchie Carroll
'       Added GetStringSegments function to break a string up into smaller chunks for parsing
'       and/or display purposes
'  08/02/2007 - J. Ritchie Carroll
'       Added a CenterText method for centering strings in console applications or fixed width fonts
'  08/03/2007 - Pinal C. Patel
'       Modified the CenterText method to handle multiple lines
'
'*******************************************************************************************************

Imports System.Text
Imports TVA.Common

Namespace Text

    ''' <summary>Defines common global functions related to string manipulation</summary>
    Public NotInheritable Class Common

        ''' <summary>Function signature used to test a character to see if it fits a certain criteria</summary>
        ''' <param name="c">Character to test</param>
        ''' <returns>Returns True if specified character passed test</returns>
        Public Delegate Function CharacterTestFunctionSignature(ByVal c As Char) As Boolean

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Performs a fast concatenation of given string array</summary>
        ''' <param name="values">String array to concatenate</param>
        ''' <returns>The concatenated string representation of the values of the elements in <paramref name="values" /> string array.</returns>
        ''' <remarks>
        ''' <para>This is a replacement for the String.Concat function.  Tests show that the system implemenation of this function is slow:
        ''' http://www.developer.com/net/cplus/article.php/3304901
        ''' </para>
        ''' </remarks>
        <Obsolete("Latest .NET versions of the String.Concat function have been optimized - this function will be removed from future builds of the code library.", False)> _
        Public Shared Function Concat(ByVal ParamArray values As String()) As String

            If values Is Nothing Then
                Return ""
            Else
                If values.Length = 2 Then
                    Return String.Concat(values(0), values(1))
                ElseIf values.Length = 3 Then
                    Return String.Concat(values(0), values(1), values(3))
                Else
                    Dim x, size As Integer

                    ' Precalculate needed size of string buffer
                    For x = 0 To values.Length - 1
                        If Not String.IsNullOrEmpty(values(x)) Then size += values(x).Length
                    Next

                    With New StringBuilder(size)
                        For x = 0 To values.Length - 1
                            If Not String.IsNullOrEmpty(values(x)) Then .Append(values(x))
                        Next

                        Return .ToString
                    End With
                End If
            End If

        End Function

        ''' <summary>Parses a string intended to represent a boolean value</summary>
        ''' <param name="value">String representing a boolean value</param>
        ''' <returns>Parsed boolean value</returns>
        ''' <remarks>
        ''' This function, unlike Boolean.Parse, correctly parses a boolean value even if the string value
        ''' specified is a number (e.g., 0 or -1).  Boolean.Parse expects a string to be represented as
        ''' "True" or "False" (i.e., Boolean.TrueString or Boolean.FalseString respectively)
        ''' </remarks>
        Public Shared Function ParseBoolean(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False
            value = value.Trim()

            If value.Length > 0 Then
                If IsNumeric(value) Then
                    ' String contains a number
                    Dim result As Integer

                    If Integer.TryParse(value, result) Then
                        Return (result <> 0)
                    Else
                        Return False
                    End If
                Else
                    ' String contains text
                    Dim result As Boolean

                    If Boolean.TryParse(value, result) Then
                        Return result
                    Else
                        Return False
                    End If
                End If
            Else
                Return False
            End If

        End Function

        ''' <summary>Turns source string into an array of string segements, each with a set maximum width, for parsing or display purposes</summary>
        ''' <param name="value">Input string to break up into segements</param>
        ''' <param name="segmentSize">Maximum size of returned segment</param>
        ''' <returns>Array of string segments as parsed from source string</returns>
        ''' <remarks>Function will return a single element array with an empty string if source string is null or empty</remarks>
        Public Shared Function GetStringSegments(ByVal value As String, ByVal segmentSize As Integer) As String()

            If String.IsNullOrEmpty(value) Then Return New String() {""}

            Dim totalSegments As Integer = Convert.ToInt32(System.Math.Ceiling(value.Length / segmentSize))
            Dim segments As String() = CreateArray(Of String)(totalSegments)

            For x As Integer = 0 To segments.Length - 1
                If x * segmentSize + segmentSize >= value.Length Then
                    segments(x) = value.Substring(x * segmentSize)
                Else
                    segments(x) = value.Substring(x * segmentSize, segmentSize)
                End If
            Next

            Return segments

        End Function

        ''' <summary>Parses key value pair parameters from a string which are delimeted by an equals sign and multiple pairs separated by a semi-colon</summary>
        ''' <param name="value">Key vair string to parse</param>
        ''' <returns>Dictionary of key/value pairs</returns>
        ''' <remarks>
        ''' This will parse a string formated like a typical connection string, e.g.:
        ''' <code>
        ''' IP=localhost; Port=1002; MaxEvents=50; UseTimeout=True
        ''' </code>
        ''' Note that "keys" will be stored in dictionary as "lower case".
        ''' </remarks>
        Public Shared Function ParseKeyValuePairs(ByVal value As String) As Dictionary(Of String, String)

            Return ParseKeyValuePairs(value, ";"c, "="c)

        End Function

        ''' <summary>Parses key value pair parameters from a string which are delimeted by an equals sign and multiple pairs separated by a semi-colon</summary>
        ''' <param name="value">Key vair string to parse</param>
        ''' <param name="parameterDelimeter">Character that delimits one key value pair from another (e.g., would be a ";" in a typical connection string)</param>
        ''' <param name="keyValueDelimeter">Character that delimits key from value (e.g., would be an "=" in a typical connection string)</param>
        ''' <returns>Dictionary of key/value pairs</returns>
        ''' <remarks>
        ''' This will parse a key value string that contains one or many pairs.
        ''' Note that "keys" will be stored in dictionary as "lower case".
        ''' </remarks>
        Public Shared Function ParseKeyValuePairs(ByVal value As String, ByVal parameterDelimeter As Char, ByVal keyValueDelimeter As Char) As Dictionary(Of String, String)

            Dim keyValuePairs As New Dictionary(Of String, String)
            Dim elements As String()

            ' Parse out connect string parameters
            For Each parameter As String In value.Split(parameterDelimeter)
                ' Parse out parameter's key/value elements
                elements = parameter.Split(keyValueDelimeter)
                If elements.Length = 2 Then
                    keyValuePairs.Add(elements(0).ToString().Trim().ToLower(), elements(1).ToString().Trim())
                End If
            Next

            Return keyValuePairs

        End Function

        ''' <summary>Ensures parameter is not an empty or null string - returns a single space if test value is empty</summary>
        ''' <param name="testValue">Value to test for null or empty</param>
        ''' <returns>A non-empty string</returns>
        Public Shared Function NotEmpty(ByVal testValue As String) As String

            Return NotEmpty(testValue, " ")

        End Function

        ''' <summary>Ensures parameter is not an empty or null string</summary>
        ''' <param name="testValue">Value to test for null or empty</param>
        ''' <param name="nonEmptyReturnValue">Value to return if <paramref name="testValue">testValue</paramref> is null or empty</param>
        ''' <returns>A non-empty string</returns>
        Public Shared Function NotEmpty(ByVal testValue As String, ByVal nonEmptyReturnValue As String) As String

            If String.IsNullOrEmpty(nonEmptyReturnValue) Then Throw New ArgumentException("nonEmptyReturnValue cannot be empty!")
            If String.IsNullOrEmpty(testValue) Then Return nonEmptyReturnValue Else Return testValue

        End Function

        ''' <summary>Replaces all characters passing delegate test with specified replacement character</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="replacementCharacter">Character used to replace characters passing delegate test</param>
        ''' <param name="characterTestFunction">Delegate used to determine whether or not character should be replaced</param>
        ''' <returns>Returns <paramref name="value" /> with all characters passing delegate test replaced</returns>
        ''' <remarks>This function allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160))</remarks>
        Public Shared Function ReplaceCharacters(ByVal value As String, ByVal replacementCharacter As Char, ByVal characterTestFunction As CharacterTestFunctionSignature) As String

            If String.IsNullOrEmpty(value) Then Return ""

            With New StringBuilder
                Dim character As Char

                For x As Integer = 0 To value.Length - 1
                    character = value(x)

                    If characterTestFunction(character) Then
                        .Append(replacementCharacter)
                    Else
                        .Append(character)
                    End If
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Removes all characters passing delegate test from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="characterTestFunction">Delegate used to determine whether or not character should be removed</param>
        ''' <returns>Returns <paramref name="value" /> with all characters passing delegate test removed</returns>
        Public Shared Function RemoveCharacters(ByVal value As String, ByVal characterTestFunction As CharacterTestFunctionSignature) As String

            If String.IsNullOrEmpty(value) Then Return ""

            With New StringBuilder
                Dim character As Char

                For x As Integer = 0 To value.Length - 1
                    character = value(x)

                    If Not characterTestFunction(character) Then
                        .Append(character)
                    End If
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Removes all white space (as defined by IsWhiteSpace) from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all white space removed</returns>
        Public Shared Function RemoveWhiteSpace(ByVal value As String) As String

            Return RemoveCharacters(value, AddressOf Char.IsWhiteSpace)

        End Function

        ''' <summary>Replaces all white space characters (as defined by IsWhiteSpace) with specified replacement character</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="replacementCharacter">Character used to "replace" white space characters</param>
        ''' <returns>Returns <paramref name="value" /> with all white space characters replaced</returns>
        ''' <remarks>This function allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160))</remarks>
        Public Shared Function ReplaceWhiteSpace(ByVal value As String, ByVal replacementCharacter As Char) As String

            Return ReplaceCharacters(value, replacementCharacter, AddressOf Char.IsWhiteSpace)

        End Function

        ''' <summary>Removes all control characters from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all control characters removed</returns>
        Public Shared Function RemoveControlCharacters(ByVal value As String) As String

            Return RemoveCharacters(value, AddressOf Char.IsControl)

        End Function

        ''' <summary>Replaces all control characters with a single space</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all control characters replaced as a single space</returns>
        Public Shared Function ReplaceControlCharacters(ByVal value As String) As String

            Return ReplaceControlCharacters(value, " "c)

        End Function

        ''' <summary>Replaces all control characters with specified replacement character</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="replacementCharacter">Character used to "replace" control characters</param>
        ''' <returns>Returns <paramref name="value" /> with all control characters replaced</returns>
        ''' <remarks>This function allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160))</remarks>
        Public Shared Function ReplaceControlCharacters(ByVal value As String, ByVal replacementCharacter As Char) As String

            Return ReplaceCharacters(value, replacementCharacter, AddressOf Char.IsControl)

        End Function

        ''' <summary>Removes all carriage returns and line feeds from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all CR and LF characters removed.</returns>
        Public Shared Function RemoveCrLfs(ByVal value As String) As String

            Return RemoveCharacters(value, AddressOf IsCrOrLf)

        End Function

        ''' <summary>Replaces all carriage return and line feed characters (as well as CR/LF sequences) with specified replacement character</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="replacementCharacter">Character used to "replace" CR and LF characters</param>
        ''' <returns>Returns <paramref name="value" /> with all CR and LF characters replaced</returns>
        ''' <remarks>This function allows you to specify a replacement character (e.g., you may want to use a non-breaking space: Convert.ToChar(160))</remarks>
        Public Shared Function ReplaceCrLfs(ByVal value As String, ByVal replacementCharacter As Char) As String

            Return ReplaceCharacters(value.Replace(Convert.ToChar(13) & Convert.ToChar(10), replacementCharacter), replacementCharacter, AddressOf IsCrOrLf)

        End Function

        Private Shared Function IsCrOrLf(ByVal c As Char) As Boolean

            Return (c = Convert.ToChar(13) OrElse c = Convert.ToChar(10))

        End Function

        ''' <summary>Removes duplicate character strings (adjoining replication) in a string</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="duplicatedValue">String whose duplicates are to be removed</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicated <paramref name="duplicatedValue" /> removed</returns>
        Public Shared Function RemoveDuplicates(ByVal value As String, ByVal duplicatedValue As String) As String

            If String.IsNullOrEmpty(value) Then Return ""
            If String.IsNullOrEmpty(duplicatedValue) Then Return value

            Dim duplicate As String = String.Concat(duplicatedValue, duplicatedValue)

            Do While value.IndexOf(duplicate) > -1
                value = value.Replace(duplicate, duplicatedValue)
            Loop

            Return value

        End Function

        ''' <summary>Removes the terminator (Convert.ToChar(0)) from a null terminated string - useful for strings returned from Windows API call</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all characters to the left of the terminator</returns>
        Public Shared Function RemoveNull(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Return ""

            Dim nullPos As Integer = value.IndexOf(Convert.ToChar(0))

            If nullPos > -1 Then
                Return value.Substring(0, nullPos)
            Else
                Return value
            End If

        End Function

        ''' <summary>Replaces all repeating white space with a single space</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicate white space removed</returns>
        Public Shared Function RemoveDuplicateWhiteSpace(ByVal value As String) As String

            Return RemoveDuplicateWhiteSpace(value, " "c)

        End Function

        ''' <summary>Replaces all repeating white space with specified spacing character</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="spacingCharacter">Character value to use to insert as single white space value</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicate white space removed</returns>
        ''' <remarks>This function allows you to specify spacing character (e.g., you may want to use a non-breaking space: Convert.ToChar(160))</remarks>
        Public Shared Function RemoveDuplicateWhiteSpace(ByVal value As String, ByVal spacingCharacter As Char) As String

            If String.IsNullOrEmpty(value) Then Return ""

            With New StringBuilder
                Dim character As Char
                Dim lastCharWasSpace As Boolean

                For x As Integer = 0 To value.Length - 1
                    character = value(x)

                    If Char.IsWhiteSpace(character) Then
                        lastCharWasSpace = True
                    Else
                        If lastCharWasSpace Then
                            .Append(spacingCharacter)
                        End If
                        .Append(character)
                        lastCharWasSpace = False
                    End If
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Counts the total number of the occurances of <paramref name="characterToCount" /> in the given string</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="characterToCount">Character to be counted</param>
        ''' <returns>Total number of the occurances of <paramref name="characterToCount" /> in the given string</returns>
        Public Shared Function CharCount(ByVal value As String, ByVal characterToCount As Char) As Integer

            If String.IsNullOrEmpty(value) Then Return 0

            Dim total As Integer

            For x As Integer = 0 To value.Length - 1
                If value(x) = characterToCount Then total += 1
            Next

            Return total

        End Function

        ''' <summary>Tests to see if a string is all digits</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are digits, otherwise false</returns>
        Public Shared Function IsAllDigits(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Not Char.IsDigit(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string is all numbers</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are numbers, otherwise false</returns>
        Public Shared Function IsAllNumbers(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Not Char.IsNumber(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string's letters are all upper case</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's letter characters are upper case, otherwise false</returns>
        Public Shared Function IsAllUpper(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Char.IsLetter(value(x)) AndAlso Not Char.IsUpper(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string's letters are all lower case</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's letter characters are lower case, otherwise false</returns>
        Public Shared Function IsAllLower(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Char.IsLetter(value(x)) AndAlso Not Char.IsLower(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string is all letters</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are letters, otherwise false</returns>
        ''' <remarks>Any non letter (e.g., punctuation marks) would cause this function to return False - see overload to ignore punctuation marks</remarks>
        Public Shared Function IsAllLetters(ByVal value As String) As Boolean

            Return IsAllLetters(value, False)

        End Function

        ''' <summary>Tests to see if a string is all letters</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="ignorePunctuation">Set to True to ignore punctuation</param>
        ''' <returns>True if all string's characters are letters, otherwise false</returns>
        Public Shared Function IsAllLetters(ByVal value As String, ByVal ignorePunctuation As Boolean) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If ignorePunctuation Then
                    If Not (Char.IsLetter(value(x)) OrElse Char.IsPunctuation(value(x))) Then Return False
                Else
                    If Not Char.IsLetter(value(x)) Then Return False
                End If
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string is all letters or digits</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are letters or digits, otherwise false</returns>
        ''' <remarks>Any non letter or digit (e.g., punctuation marks) would cause this function to return False - see overload to ignore punctuation marks</remarks>
        Public Shared Function IsAllLettersOrDigits(ByVal value As String) As Boolean

            Return IsAllLettersOrDigits(value, False)

        End Function

        ''' <summary>Tests to see if a string is all letters or digits</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="ignorePunctuation">Set to True to ignore punctuation</param>
        ''' <returns>True if all string's characters are letters or digits, otherwise false</returns>
        Public Shared Function IsAllLettersOrDigits(ByVal value As String, ByVal ignorePunctuation As Boolean) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If ignorePunctuation Then
                    If Not (Char.IsLetterOrDigit(value(x)) OrElse Char.IsPunctuation(value(x))) Then Return False
                Else
                    If Not Char.IsLetterOrDigit(value(x)) Then Return False
                End If
            Next

            Return True

        End Function

        ''' <summary>Encodes the specified Unicode character in proper Regular Expression format</summary>
        ''' <param name="item">Unicode character to encode in Regular Expression format</param>
        ''' <returns>Specified Unicode character in proper Regular Expression format</returns>
        Public Shared Function EncodeRegexChar(ByVal item As Char) As String

            Return "\u" & Convert.ToUInt16(item).ToString("x"c).PadLeft(4, "0"c)

        End Function

        ''' <summary>Decodes the specified Regular Expression character back into a standard Unicode character</summary>
        ''' <param name="value">Regular Expression character to decode back into a Unicode character</param>
        ''' <returns>Standard Unicode character representation of specified Regular Expression character</returns>
        Public Shared Function DecodeRegexChar(ByVal value As String) As Char

            Return Convert.ToChar(Convert.ToUInt16(value.Replace("\u", "0x"), 16))

        End Function

        ''' <summary>Encodes a string into a base-64 string</summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>
        ''' <para>This performs a base-64 style of string encoding useful for data obfuscation or safe XML data string transmission</para>
        ''' <para>Note: this function encodes a "String", use the Convert.ToBase64String function to encode a binary data buffer</para>
        ''' </remarks>
        Public Shared Function Base64Encode(ByVal value As String) As String

            Return Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(value))

        End Function

        ''' <summary>Decodes given base-64 encoded string encoded with <see cref="Base64Encode" /></summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>Note: this function decodes value back into a "String", use the Convert.FromBase64String function to decode a base-64 encoded string back into a binary data buffer</remarks>
        Public Shared Function Base64Decode(ByVal value As String) As String

            Return System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(value))

        End Function

        ''' <summary>
        ''' Truncates the provided string from left if it is longer that specified length.
        ''' </summary>
        Public Shared Function TruncateLeft(ByVal value As String, ByVal maxLength As Integer) As String

            If value.Length > maxLength Then Return value.Substring(value.Length - maxLength)
            Return value

        End Function

        ''' <summary>
        ''' Truncates the provided string from right if it is longer that specified length.
        ''' </summary>
        Public Shared Function TruncateRight(ByVal value As String, ByVal maxLength As Integer) As String

            If value.Length > maxLength Then Return value.Substring(0, maxLength)
            Return value

        End Function

        ''' <summary>
        ''' Centers text within specified maximum length, biased to the left.
        ''' Text will be padded to the left and right with spaces.
        ''' If value is greater than specified maximum length, value returned will be truncated from the right.
        ''' </summary>
        ''' <remarks>
        ''' This function will handle multiple lines of text separated by Environment.NewLine
        ''' </remarks>
        Public Shared Function CenterText(ByVal value As String, ByVal maxLength As Integer) As String

            Return CenterText(value, maxLength, " "c)

        End Function

        ''' <summary>
        ''' Centers text within specified maximum length, biased to the left.
        ''' Text will be padded to the left and right with specified padding character.
        ''' If value is greater than specified maximum length, value returned will be truncated from the right.
        ''' </summary>
        ''' <remarks>
        ''' This function will handle multiple lines of text separated by Environment.NewLine
        ''' </remarks>
        Public Shared Function CenterText(ByVal value As String, ByVal maxLength As Integer, ByVal paddingCharacter As Char) As String

            ' If the text to be centered contains multiple lines, we'll center all the lines individually.
            Static lineDelimiters As String() = New String() {Environment.NewLine}
            Dim result As New StringBuilder()
            Dim lines As String() = value.Split(lineDelimiters, StringSplitOptions.None)
            Dim line As String
            Dim lastLineIndex As Integer = lines.Length - 1

            For i As Integer = 0 To lastLineIndex
                ' Get current line
                line = lines(i)

                If line.Length >= maxLength Then
                    ' Truncate excess characters on the right
                    result.Append(line.Substring(0, maxLength))
                Else
                    Dim remainingSpace As Integer = maxLength - line.Length
                    Dim leftSpaces, rightSpaces As Integer

                    ' Split remaining space between the left and the right
                    leftSpaces = remainingSpace \ 2
                    rightSpaces = leftSpaces

                    ' Add any remaining odd space to the right (bias text to the left)
                    If remainingSpace Mod 2 > 0 Then rightSpaces += 1

                    result.Append(New String(paddingCharacter, leftSpaces))
                    result.Append(line)
                    result.Append(New String(paddingCharacter, rightSpaces))
                End If

                ' We create a new line only if the original text contains multiple lines.
                If i < lastLineIndex Then result.AppendLine()
            Next

            Return result.ToString()

        End Function

    End Class

End Namespace
