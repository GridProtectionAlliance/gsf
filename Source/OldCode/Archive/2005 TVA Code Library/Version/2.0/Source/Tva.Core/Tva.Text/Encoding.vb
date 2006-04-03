'*******************************************************************************************************
'  Tva.Text.Encoding.vb - Common Text Encoding Functions
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
'  04/03/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.String)
'
'*******************************************************************************************************

Imports System.Text

Namespace Text

    ''' <summary>Defines common global functions related to string encoding</summary>
    Public NotInheritable Class Encoding

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Encodes the specified Unicode character in proper Regular Expression format</summary>
        ''' <param name="item">Unicode character to encode in Regular Expression format</param>
        ''' <returns>specified Unicode character in proper Regular Expression format</returns>
        Public Shared Function EncodeRegexChar(ByVal item As Char) As String

            Return "\u" & Convert.ToInt32(item).ToString("x"c).PadLeft(4, "0"c)

        End Function

        ''' <summary>Encodes the specified Unicode character in proper Regular Expression format</summary>
        ''' <param name="value">Unicode character to encode in Regular Expression format</param>
        ''' <returns>specified Unicode character in proper Regular Expression format</returns>
        Public Shared Function DecodeRegexChar(ByVal value As String) As Char

            Return Convert.ToChar(Convert.ToInt16(value.Replace("\u", "&H")))

        End Function

        ''' <summary>Encodes string into a hexadecimal string</summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>
        ''' <para>This performs a simple form of string encoding useful for presentation of binary data, data obfuscation or safe XML data string transmission</para>
        ''' <para>Note that Base64 encoding will be a much more efficient way to safely send encoded binary data</para>
        ''' </remarks>
        Public Shared Function HexEncode(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Throw New ArgumentNullException("value", "input string ""value"" cannot be null")

            Return HexEncode(System.Text.Encoding.Unicode.GetBytes(value))

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

            Return System.Text.Encoding.Unicode.GetString(data)

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

            Return Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(value))

        End Function

        ''' <summary>Decodes given base-64 encoded string encoded with <see cref="Base64Encode" /></summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>Note: this function decodes value back into a "String", use the Convert.FromBase64String function to decode a base-64 encoded string back into a binary data buffer</remarks>
        Public Shared Function Base64Decode(ByVal value As String) As String

            Return System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(value))

        End Function

    End Class

End Namespace