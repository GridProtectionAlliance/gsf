'*******************************************************************************************************
'  Tva.Text.Common.vb - Common Text Functions
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
'  01/24/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.String)
'
'*******************************************************************************************************

Imports System.Text

Namespace Text

    ''' <summary>Defines common global functions related to string manipulation</summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Return the Unicode number for a character in proper Regular Expression format</summary>
        Public Shared Function EncodeRegexChar(ByVal item As Char) As String

            Return "\u" & Convert.ToInt32(item).ToString("x"c).PadLeft(4, "0"c)

        End Function

        ''' <summary>Performs a fast concatenation of given string array</summary>
        ''' <param name="values">String array to concatenate</param>
        ''' <returns>The concatenated string representation of the values of the elements in <paramref name="values" /> string array.</returns>
        ''' <remarks>
        ''' This is a replacement for the String.Concat function.  Tests show that the system implemenation of this function is slow:
        ''' http://www.developer.com/net/cplus/article.php/3304901
        ''' </remarks>
        Public Shared Function Concat(ByVal ParamArray values As String()) As String

            If values Is Nothing Then
                Return ""
            Else
                With New StringBuilder
                    For x As Integer = 0 To values.Length - 1
                        If Not String.IsNullOrEmpty(values(x)) Then .Append(values(x))
                    Next

                    Return .ToString
                End With
            End If

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

        ''' <summary>Removes duplicate character strings (adjoining replication) in a string</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="duplicatedValue">String whose duplicates are to be removed</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicated <paramref name="duplicatedValue" /> removed</returns>
        Public Shared Function RemoveDuplicates(ByVal value As String, ByVal duplicatedValue As String) As String

            If String.IsNullOrEmpty(value) Then Return ""
            If String.IsNullOrEmpty(duplicatedValue) Then Return value

            Dim duplicate As String = Concat(duplicatedValue, duplicatedValue)

            Do While value.IndexOf(duplicate) > -1
                value = value.Replace(duplicate, duplicatedValue)
            Loop

            Return value

        End Function

        ''' <summary>Removes the terminator (Chr(0)) from a null terminated string - useful for strings returned from API</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all characters to the left of the terminator</returns>
        Public Shared Function RemoveNull(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Return ""

            Dim nullPos As Integer = value.IndexOf(Chr(0))

            If nullPos > -1 Then
                Return value.Substring(0, nullPos)
            Else
                Return value
            End If

        End Function

        ''' <summary>Removes all carriage returns and line feeds (CrLf, Cr, and Lf's) from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all CR and LF characters removed.</returns>
        Public Shared Function RemoveCrLfs(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Return ""
            Return value.Replace(Environment.NewLine, "").Replace(Convert.ToChar(13), "").Replace(Convert.ToChar(10), "")

        End Function

        ''' <summary>Removes all whitespaces (as defined by IsWhiteSpace) from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all white spaces removed</returns>
        Public Shared Function RemoveWhiteSpace(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Return ""

            With New StringBuilder
                Dim character As Char

                For x As Integer = 0 To value.Length - 1
                    character = value.Chars(x)

                    If Not Char.IsWhiteSpace(character) Then
                        .Append(character)
                    End If
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Replaces all repeating white space with a single space</summary>
        ''' <param name="value">The string to process</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicate white space removed</returns>
        Public Shared Function RemoveDuplicateWhiteSpace(ByVal value As String) As String

            Return RemoveDuplicateWhiteSpace(value, " "c)

        End Function

        ''' <summary>Replaces all repeating white space with specified spacing character</summary>
        ''' <param name="value">The string to process</param>
        ''' <param name="spacingCharacter">Character value to use to insert as single white space value</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicate white space removed</returns>
        ''' <remarks>This function allows you to specify spacing character (e.g., you may want to use a non-breaking space: Convert.ToChar(160))</remarks>
        Public Shared Function RemoveDuplicateWhiteSpace(ByVal value As String, ByVal spacingCharacter As Char) As String

            If String.IsNullOrEmpty(value) Then Return ""

            With New StringBuilder
                Dim character As Char
                Dim lastCharWasSpace As Boolean

                For x As Integer = 0 To value.Length - 1
                    character = value.Chars(x)

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

        ''' <summary>Encodes string into a hexadecimal string</summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>
        ''' <para>This performs a simple form of string encoding useful for presentation of binary data, data obfuscation or safe XML data string transmission</para>
        ''' <para>Note that Base64 encoding will be a much more efficient way to safely send encoded binary data</para>
        ''' </remarks>
        Public Shared Function HexEncode(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Throw New ArgumentNullException("value", "input string ""value"" cannot be null")

            Return HexEncode(Encoding.Unicode.GetBytes(value))

        End Function

        ''' <summary>Encodes a binary data buffer into a hexadecimal string</summary>
        ''' <param name="data">Input data</param>
        ''' <remarks>
        ''' <para>This performs a simple form of data encoding useful for presentation of binary data, data obfuscation or safe XML data transmission</para>
        ''' <para>Note that Base64 encoding will be a much more efficient way to safely send encoded binary data</para>
        ''' </remarks>
        Public Shared Function HexEncode(ByVal data As Byte()) As String

            Return HexEncode(data, 0, data.Length)

        End Function

        ''' <summary>Encodes specified portion of a binary data buffer into a hexadecimal string</summary>
        ''' <param name="data">Input data</param>
        ''' <param name="offset">Offset position to begin encoding</param>
        ''' <param name="length">Total number of data bytes to encode</param>
        ''' <remarks>
        ''' <para>This performs a simple form of data encoding useful for presentation of binary data, data obfuscation or safe XML data transmission</para>
        ''' <para>Note that Base64 encoding will be a much more efficient way to safely send encoded binary data</para>
        ''' </remarks>
        Public Shared Function HexEncode(ByVal data As Byte(), ByVal offset As Integer, ByVal length As Integer) As String

            If data Is Nothing Then Throw New ArgumentNullException("data", "input buffer ""data"" cannot be null")

            With New StringBuilder
                For x As Integer = 0 To length - 1
                    .Append(data(offset + x).ToString("x"c).PadLeft(2, "0"c))
                Next

                Return .ToString()
            End With

        End Function

        ''' <summary>Decodes given hexadecimal string encoded with <see cref="HexEncode" /></summary>
        ''' <param name="value">Input string</param>
        Public Shared Function HexDecode(ByVal value As String) As String

            Dim data As Byte()

            HexDecode(value, data)

            Return Encoding.Unicode.GetString(data)

        End Function

        ''' <summary>Decodes given hexadecimal string encoded with <see cref="HexEncode" /></summary>
        ''' <param name="value">Input string</param>
        ''' <param name="data">Buffer used hold decoded data - buffer will be created and sized by this method</param>
        Public Shared Sub HexDecode(ByVal value As String, ByRef data As Byte())

            If String.IsNullOrEmpty(value) Then Throw New ArgumentNullException("value", "encoded hexadecimal input string ""value"" cannot be null")

            Dim index As Integer

            data = Array.CreateInstance(GetType(Byte), value.Length \ 2)

            For x As Integer = 0 To value.Length - 1 Step 2
                data(index) = Convert.ToByte(value.Substring(x, 2), 16)
                index += 1
            Next

        End Sub

        ''' <summary>Encodes a string into a base-64 string</summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>
        ''' <para>This performs a base-64 style of string encoding useful for data obfuscation or safe XML data string transmission</para>
        ''' <para>Note: this function encodes a "String", use the Convert.ToBase64String function to encode a binary data buffer</para>
        ''' </remarks>
        Public Shared Function Base64Encode(ByVal value As String) As String

            Return Convert.ToBase64String(Encoding.Unicode.GetBytes(value))

        End Function

        ''' <summary>Decodes given base-64 encoded string encoded with <see cref="Base64Encode" /></summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>Note: this function decodes value back into a "String", use the Convert.FromBase64String function to decode a base-64 encoded string back into a binary data buffer</remarks>
        Public Shared Function Base64Decode(ByVal value As String) As String

            Return Encoding.Unicode.GetString(Convert.FromBase64String(value))

        End Function

    End Class

End Namespace
