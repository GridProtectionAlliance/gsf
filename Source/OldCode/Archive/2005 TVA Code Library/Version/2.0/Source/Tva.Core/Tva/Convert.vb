'*******************************************************************************************************
'  Tva.Common.vb - Conversion Functions
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
'
'*******************************************************************************************************

Imports System.Text
Imports System.Text.RegularExpressions

Public NotInheritable Class Convert

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>
    ''' Converts a string in ascii format to byte array.
    ''' </summary>
    ''' <param name="asciiData">Required. Ascii string to be converted to byte array.</param>
    ''' <returns>Byte representation of the ascii string.</returns>
    ''' <remarks>Ascii string format example: Hello World!</remarks>
    Public Shared Function AsciiToBytes(ByVal asciiData As String) As Byte()

        If asciiData IsNot Nothing Then
            Dim bytes(System.Convert.ToInt32(asciiData.Length() - 1)) As Byte

            For i As Integer = 0 To asciiData.Length() - 1
                bytes(i) = System.Convert.ToByte(System.Convert.ToChar(asciiData.Substring(i, 1)))
            Next

            Return bytes
        Else
            Return Nothing
        End If

    End Function

    ''' <summary>
    ''' Converts a byte array to a ascii string.
    ''' </summary>
    ''' <param name="bytes">Required. Byte array to be converted to ascii string.</param>
    ''' <returns>String in ascii format.</returns>
    ''' <remarks></remarks>
    Public Shared Function BytesToAscii(ByVal bytes As Byte()) As String

        With New StringBuilder()
            If bytes IsNot Nothing Then
                For i As Integer = 0 To bytes.Length() - 1
                    .Append(System.Convert.ToChar(System.Convert.ToInt32(bytes(i).ToString())))
                Next
            End If

            Return .ToString()
        End With

    End Function

    ''' <summary>
    ''' Converts a string in ascii format to its hex equivalent.
    ''' </summary>
    ''' <param name="asciiData">Required. Ascii string to be converted to hex string.</param>
    ''' <returns>String in hex format.</returns>
    ''' <remarks></remarks>
    Public Shared Function AsciiToHex(ByVal asciiData As String) As String

        Return BytesToHex(AsciiToBytes(asciiData))

    End Function

    Public Shared Function AsciiToDec(ByVal asciiData As String) As String

        Return BytesToDec(AsciiToBytes(asciiData))

    End Function

    ''' <summary>
    ''' Convert a string in hex format to byte array.
    ''' </summary>
    ''' <param name="hexData">Required. Hex string to be converted to byte array.</param>
    ''' <returns>Byte representation of the hex string.</returns>
    ''' <remarks>Hex string format example: 48656c6c6f20576f726c6421 (Ascii equivalent: Hello World!)</remarks>
    Public Shared Function HexToBytes(ByVal hexData As String) As Byte()

        ' Process the string only if it has data in hex format (Example: 48656C6C21).
        If hexData IsNot Nothing AndAlso Regex.Matches(hexData, "[^a-fA-F0-9]").Count() = 0 Then
            ' Trim the end of the string to discard any additional characters if present in the string that
            ' would prevent the string from being a hex string. 
            hexData = hexData.Substring(0, hexData.Length() - hexData.Length() Mod 2)

            Dim bytes(System.Convert.ToInt32(hexData.Length() / 2) - 1) As Byte
            Dim j As Integer = 0
            For i As Integer = 0 To hexData.Length() - 1 Step 2
                bytes(j) = System.Convert.ToByte(hexData.Substring(i, 2), 16)
                j += 1
            Next

            Return bytes
        Else
            Return Nothing
        End If

    End Function

    ''' <summary>
    ''' Converts a byte array to a hex string.
    ''' </summary>
    ''' <param name="bytes">Required. Byte array to be converted to hex string.</param>
    ''' <returns>String in hex format.</returns>
    ''' <remarks></remarks>
    Public Shared Function BytesToHex(ByVal bytes As Byte()) As String

        With New StringBuilder()
            If bytes IsNot Nothing Then
                For i As Integer = 0 To bytes.Length() - 1
                    .Append(bytes(i).ToString("X2"))
                Next
            End If

            Return .ToString()
        End With

    End Function

    ''' <summary>
    ''' Converts a string in hex format to its ascii equivalent.
    ''' </summary>
    ''' <param name="hexData">Required. Hex string to be converted to ascii string.</param>
    ''' <returns>String in ascii format.</returns>
    ''' <remarks></remarks>
    Public Shared Function HexToAscii(ByVal hexData As String) As String

        Return BytesToAscii(HexToBytes(hexData))

    End Function

    Public Shared Function HexToDec(ByVal hexData As String) As String

        Return BytesToDec(HexToBytes(hexData))

    End Function

    Public Shared Function DecToBytes(ByVal decData As String) As Byte()

        ' Process the string only if it has data in decimal format (Example: 072101108108033).
        If decData IsNot Nothing AndAlso Regex.Matches(decData, "[^a-fA-F0-9]").Count() = 0 Then
            ' Trim the end of the string to discard any additional characters, if present, in the 
            ' string that would prevent the string from being a decimal string. 
            ' Note: We require each character is represented by its 3 character decimal value.
            decData = decData.Substring(0, decData.Length() - decData.Length() Mod 3)

            Dim bytes(System.Convert.ToInt32(decData.Length() / 3) - 1) As Byte
            Dim j As Integer = 0
            For i As Integer = 0 To decData.Length() - 1 Step 3
                Dim decValue As Int16 = System.Convert.ToInt16(decData.Substring(i, 3))
                If decValue >= 0 AndAlso decValue <= 255 Then
                    bytes(j) = System.Convert.ToByte(decData.Substring(i, 3), 10)
                    j += 1
                Else
                    bytes = Nothing
                    Exit For
                End If
            Next
            Return bytes
        Else
            Return Nothing
        End If

    End Function

    Public Shared Function BytesToDec(ByVal bytes As Byte()) As String

        With New StringBuilder()
            If bytes IsNot Nothing Then
                For i As Integer = 0 To bytes.Length() - 1
                    .Append(bytes(i).ToString("D3"))
                Next
            End If

            Return .ToString()
        End With

    End Function

    Public Shared Function DecToAscii(ByVal decData As String) As String

        Return BytesToAscii(DecToBytes(decData))

    End Function

End Class
