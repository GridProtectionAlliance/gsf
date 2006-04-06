'*******************************************************************************************************
'  Tva.ByteEncoding.vb - Byte encoding functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/29/2005 - Pinal C Patel
'       Original version of source code generated
'  04/05/2006 - J. Ritchie Carroll
'       Transformed code into System.Text.Encoding styled hierarchy
'
'*******************************************************************************************************

Imports System.Text
Imports System.Text.RegularExpressions
Imports Tva.Interop.Bit

''' <summary>Handles conversion of a byte buffers to and from user presentable data formats</summary>
Public MustInherit Class ByteEncoding

#Region " Hexadecimal Encoding Class "

    Public Class HexadecimalEncoding

        Inherits ByteEncoding

        Friend Sub New()

            ' This class is meant for internal instatiation only

        End Sub

        ''' <summary>Decodes given string back into a byte buffer</summary>
        ''' <param name="hexData">Encoded hexadecimal data string to decode</param>
        ''' <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes</param>
        ''' <returns>Decoded bytes</returns>
        Public Overrides Function GetBytes(ByVal hexData As String, ByVal spacingCharacter As Char) As Byte()

            ' Remove spacing characters, if needed
            hexData = hexData.Trim
            If spacingCharacter <> NoSpacing Then hexData = hexData.Replace(spacingCharacter, "")

            ' Process the string only if it has data in hex format (Example: 48656C6C21).
            If Not String.IsNullOrEmpty(hexData) AndAlso Regex.Matches(hexData, "[^a-fA-F0-9]").Count = 0 Then
                ' Trim the end of the string to discard any additional characters if present in the string that
                ' would prevent the string from being a hex encoded string. 
                ' Note: We require each character is represented by its 2 character hex value.
                hexData = hexData.Substring(0, hexData.Length - hexData.Length Mod 2)

                Dim bytes As Byte() = Array.CreateInstance(GetType(Byte), hexData.Length \ 2)
                Dim index As Integer

                For x As Integer = 0 To hexData.Length - 1 Step 2
                    bytes(index) = Convert.ToByte(hexData.Substring(x, 2), 16)
                    index += 1
                Next

                Return bytes
            Else
                Throw New ArgumentException("Input string is not a valid hex encoded string - invalid characters encountered")
            End If

        End Function

        ''' <summary>Encodes given buffer into a user presentable representation</summary>
        ''' <param name="bytes">Bytes to encode</param>
        ''' <param name="offset">Offset into buffer to bgeing encoding</param>
        ''' <param name="length">Length of buffer to encode</param>
        ''' <param name="spacingCharacter">Spacing character to place between encoded bytes</param>
        ''' <returns>String of encoded bytes</returns>
        Public Overrides Function GetString(ByVal bytes() As Byte, ByVal offset As Integer, ByVal length As Integer, ByVal spacingCharacter As Char) As String

            Return BytesToString(bytes, offset, length, spacingCharacter, "X2")

        End Function

    End Class

#End Region

#Region " Decimal Encoding Class "

    Public Class DecimalEncoding

        Inherits ByteEncoding

        Friend Sub New()

            ' This class is meant for internal instatiation only

        End Sub

        ''' <summary>Decodes given string back into a byte buffer</summary>
        ''' <param name="intData">Encoded integer data string to decode</param>
        ''' <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes</param>
        ''' <returns>Decoded bytes</returns>
        Public Overrides Function GetBytes(ByVal intData As String, ByVal spacingCharacter As Char) As Byte()

            ' Remove spacing characters, if needed
            intData = intData.Trim
            If spacingCharacter <> NoSpacing Then intData = intData.Replace(spacingCharacter, "")

            ' Process the string only if it has data in decimal format (Example: 072101108108033).
            If Not String.IsNullOrEmpty(intData) AndAlso Regex.Matches(intData, "[^0-9]").Count = 0 Then
                ' Trim the end of the string to discard any additional characters, if present, in the 
                ' string that would prevent the string from being a integer encoded string. 
                ' Note: We require each character is represented by its 3 character decimal value.
                intData = intData.Substring(0, intData.Length - intData.Length Mod 3)

                Dim bytes As Byte() = Array.CreateInstance(GetType(Byte), intData.Length \ 3)
                Dim index As Integer

                For x As Integer = 0 To intData.Length - 1 Step 3
                    bytes(index) = Convert.ToByte(intData.Substring(x, 3), 10)
                    index += 1
                Next

                Return bytes
            Else
                Throw New ArgumentException("Input string is not a valid integer encoded string - invalid characters encountered")
            End If

        End Function

        ''' <summary>Encodes given buffer into a user presentable representation</summary>
        ''' <param name="bytes">Bytes to encode</param>
        ''' <param name="offset">Offset into buffer to bgeing encoding</param>
        ''' <param name="length">Length of buffer to encode</param>
        ''' <param name="spacingCharacter">Spacing character to place between encoded bytes</param>
        ''' <returns>String of encoded bytes</returns>
        Public Overrides Function GetString(ByVal bytes() As Byte, ByVal offset As Integer, ByVal length As Integer, ByVal spacingCharacter As Char) As String

            Return BytesToString(bytes, offset, length, spacingCharacter, "D3")

        End Function

    End Class

#End Region

#Region " Binary Encoding Class "

    Public Class BinaryEncoding

        Inherits ByteEncoding

        Friend Sub New()

            ' This class is meant for internal instatiation only

        End Sub

        ''' <summary>Decodes given string back into a byte buffer</summary>
        ''' <param name="binaryData">Encoded binary data string to decode</param>
        ''' <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes</param>
        ''' <returns>Decoded bytes</returns>
        Public Overrides Function GetBytes(ByVal binaryData As String, ByVal spacingCharacter As Char) As Byte()

            ' Remove spacing characters, if needed
            binaryData = binaryData.Trim
            If spacingCharacter <> NoSpacing Then binaryData = binaryData.Replace(spacingCharacter, "")

            ' Process the string only if it has data in decimal format (Example: 010101101010101).
            If Not String.IsNullOrEmpty(binaryData) AndAlso Regex.Matches(binaryData, "[^0-1]").Count = 0 Then
                ' Trim the end of the string to discard any additional characters, if present, in the 
                ' string that would prevent the string from being a binary encoded string. 
                ' Note: We require each character is represented by its 8 character binary value.
                binaryData = binaryData.Substring(0, binaryData.Length - binaryData.Length Mod 8)

                Dim bytes As Byte() = Array.CreateInstance(GetType(Byte), binaryData.Length \ 8)
                Dim index As Integer

                For x As Integer = 0 To binaryData.Length - 1 Step 8
                    bytes(index) = Nill

                    If binaryData(x + 0) = "1"c Then bytes(index) = (bytes(index) Or Bit0)
                    If binaryData(x + 1) = "1"c Then bytes(index) = (bytes(index) Or Bit1)
                    If binaryData(x + 2) = "1"c Then bytes(index) = (bytes(index) Or Bit2)
                    If binaryData(x + 3) = "1"c Then bytes(index) = (bytes(index) Or Bit3)
                    If binaryData(x + 4) = "1"c Then bytes(index) = (bytes(index) Or Bit4)
                    If binaryData(x + 5) = "1"c Then bytes(index) = (bytes(index) Or Bit5)
                    If binaryData(x + 6) = "1"c Then bytes(index) = (bytes(index) Or Bit6)
                    If binaryData(x + 7) = "1"c Then bytes(index) = (bytes(index) Or Bit7)

                    index += 1
                Next

                Return bytes
            Else
                Throw New ArgumentException("Input string is not a valid binary encoded string - invalid characters encountered")
            End If

        End Function

        ''' <summary>Encodes given buffer into a user presentable representation</summary>
        ''' <param name="bytes">Bytes to encode</param>
        ''' <param name="offset">Offset into buffer to bgeing encoding</param>
        ''' <param name="length">Length of buffer to encode</param>
        ''' <param name="spacingCharacter">Spacing character to place between encoded bytes</param>
        ''' <returns>String of encoded bytes</returns>
        Public Overrides Function GetString(ByVal bytes() As Byte, ByVal offset As Integer, ByVal length As Integer, ByVal spacingCharacter As Char) As String

            With New StringBuilder
                If bytes IsNot Nothing Then
                    For x As Integer = 0 To length - 1
                        If spacingCharacter <> NoSpacing AndAlso x > 0 Then .Append(spacingCharacter)
                        If (bytes(x) And Bit0) > 0 Then .Append("1"c) Else .Append("0"c)
                        If (bytes(x) And Bit1) > 0 Then .Append("1"c) Else .Append("0"c)
                        If (bytes(x) And Bit2) > 0 Then .Append("1"c) Else .Append("0"c)
                        If (bytes(x) And Bit3) > 0 Then .Append("1"c) Else .Append("0"c)
                        If (bytes(x) And Bit4) > 0 Then .Append("1"c) Else .Append("0"c)
                        If (bytes(x) And Bit5) > 0 Then .Append("1"c) Else .Append("0"c)
                        If (bytes(x) And Bit6) > 0 Then .Append("1"c) Else .Append("0"c)
                        If (bytes(x) And Bit7) > 0 Then .Append("1"c) Else .Append("0"c)
                    Next
                End If

                Return .ToString()
            End With

        End Function

    End Class

#End Region

#Region " Base64 Encoding Class "

    Public Class Base64Encoding

        Inherits ByteEncoding

        Friend Sub New()

            ' This class is meant for internal instatiation only

        End Sub

        ''' <summary>Decodes given string back into a byte buffer</summary>
        ''' <param name="binaryData">Encoded binary data string to decode</param>
        ''' <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes</param>
        ''' <returns>Decoded bytes</returns>
        Public Overrides Function GetBytes(ByVal binaryData As String, ByVal spacingCharacter As Char) As Byte()

            ' Remove spacing characters, if needed
            binaryData = binaryData.Trim
            If spacingCharacter <> NoSpacing Then binaryData = binaryData.Replace(spacingCharacter, "")

            Return Convert.FromBase64String(binaryData)

        End Function

        ''' <summary>Encodes given buffer into a user presentable representation</summary>
        ''' <param name="bytes">Bytes to encode</param>
        ''' <param name="offset">Offset into buffer to bgeing encoding</param>
        ''' <param name="length">Length of buffer to encode</param>
        ''' <param name="spacingCharacter">Spacing character to place between encoded bytes</param>
        ''' <returns>String of encoded bytes</returns>
        Public Overrides Function GetString(ByVal bytes() As Byte, ByVal offset As Integer, ByVal length As Integer, ByVal spacingCharacter As Char) As String

            If spacingCharacter = NoSpacing Then
                Return Convert.ToBase64String(bytes, offset, length)
            Else
                With New StringBuilder
                    If bytes IsNot Nothing Then
                        Dim base64String As String = Convert.ToBase64String(bytes, offset, length)

                        For x As Integer = 0 To base64String.Length - 1
                            If x > 0 Then .Append(spacingCharacter)
                            .Append(base64String(x))
                        Next
                    End If

                    Return .ToString()
                End With
            End If

        End Function

    End Class

#End Region

    Public Const NoSpacing As Char = Char.MinValue

    Private Shared m_hexadecimalEncoding As HexadecimalEncoding
    Private Shared m_decimalEncoding As DecimalEncoding
    Private Shared m_binaryEncoding As BinaryEncoding
    Private Shared m_base64Encoding As Base64Encoding

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>Handles encoding and decoding of a byte buffer into a hexadecimal based presentation format</summary>
    Public Shared ReadOnly Property Hexadecimal() As HexadecimalEncoding
        Get
            If m_hexadecimalEncoding Is Nothing Then m_hexadecimalEncoding = New HexadecimalEncoding
            Return m_hexadecimalEncoding
        End Get
    End Property

    ''' <summary>Handles encoding and decoding of a byte buffer into an integer based presentation format</summary>
    Public Shared ReadOnly Property [Decimal]() As DecimalEncoding
        Get
            If m_decimalEncoding Is Nothing Then m_decimalEncoding = New DecimalEncoding
            Return m_decimalEncoding
        End Get
    End Property

    ''' <summary>Handles encoding and decoding of a byte buffer into a binary (i.e., 0 and 1's) based presentation format</summary>
    Public Shared ReadOnly Property Binary() As BinaryEncoding
        Get
            If m_binaryEncoding Is Nothing Then m_binaryEncoding = New BinaryEncoding
            Return m_binaryEncoding
        End Get
    End Property

    ''' <summary>Handles encoding and decoding of a byte buffer into a base64 presentation format</summary>
    Public Shared ReadOnly Property Base64() As Base64Encoding
        Get
            If m_base64Encoding Is Nothing Then m_base64Encoding = New Base64Encoding
            Return m_base64Encoding
        End Get
    End Property

#Region " Inheritable Functionality "

    ''' <summary>Encodes given buffer into a user presentable representation</summary>
    ''' <param name="bytes">Bytes to encode</param>

    Public Overridable Function GetString(ByVal bytes As Byte()) As String

        Return GetString(bytes, NoSpacing)

    End Function

    ''' <summary>Encodes given buffer into a user presentable representation</summary>
    ''' <param name="bytes">Bytes to encode</param>
    ''' <param name="spacingCharacter">Spacing character to place between encoded bytes</param>
    ''' <returns>String of encoded bytes</returns>
    Public Overridable Function GetString(ByVal bytes As Byte(), ByVal spacingCharacter As Char) As String

        If bytes Is Nothing Then Throw New ArgumentNullException("bytes", "Input buffer cannot be null")
        Return GetString(bytes, 0, bytes.Length, spacingCharacter)

    End Function

    ''' <summary>Encodes given buffer into a user presentable representation</summary>
    ''' <param name="bytes">Bytes to encode</param>
    ''' <param name="offset">Offset into buffer to bgeing encoding</param>
    ''' <param name="length">Length of buffer to encode</param>
    ''' <returns>String of encoded bytes</returns>
    Public Overridable Function GetString(ByVal bytes As Byte(), ByVal offset As Integer, ByVal length As Integer) As String

        If bytes Is Nothing Then Throw New ArgumentNullException("bytes", "Input buffer cannot be null")
        Return GetString(bytes, offset, length, NoSpacing)

    End Function

    ''' <summary>Encodes given buffer into a user presentable representation</summary>
    ''' <param name="bytes">Bytes to encode</param>
    ''' <param name="offset">Offset into buffer to bgeing encoding</param>
    ''' <param name="length">Length of buffer to encode</param>
    ''' <param name="spacingCharacter">Spacing character to place between encoded bytes</param>
    ''' <returns>String of encoded bytes</returns>
    Public MustOverride Function GetString(ByVal bytes As Byte(), ByVal offset As Integer, ByVal length As Integer, ByVal spacingCharacter As Char) As String

    ''' <summary>Decodes given string back into a byte buffer</summary>
    ''' <param name="value">Encoded string to decode</param>
    ''' <returns>Decoded bytes</returns>
    Public Overridable Function GetBytes(ByVal value As String) As Byte()

        If String.IsNullOrEmpty(value) Then Throw New ArgumentNullException("value", "Input string cannot be null")
        Return GetBytes(value, NoSpacing)

    End Function

    ''' <summary>Decodes given string back into a byte buffer</summary>
    ''' <param name="value">Encoded string to decode</param>
    ''' <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes</param>
    ''' <returns>Decoded bytes</returns>
    Public MustOverride Function GetBytes(ByVal value As String, ByVal spacingCharacter As Char) As Byte()

    ''' <summary>Handles byte to string conversions for implementations that are available from Byte.ToString</summary>
    Protected Function BytesToString(ByVal bytes As Byte(), ByVal offset As Integer, ByVal length As Integer, ByVal spacingCharacter As Char, ByVal format As String) As String

        If bytes Is Nothing Then Throw New ArgumentNullException("bytes", "Input buffer cannot be null")

        With New StringBuilder
            If bytes IsNot Nothing Then
                For x As Integer = 0 To length - 1
                    If spacingCharacter <> NoSpacing AndAlso x > 0 Then .Append(spacingCharacter)
                    .Append(bytes(x + offset).ToString(format))
                Next
            End If

            Return .ToString()
        End With

    End Function

#End Region

End Class
