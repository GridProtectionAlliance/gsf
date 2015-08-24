' James Ritchie Carroll - 2003
' 01/14/2005 JRC -
'       Moved bit constants into Bit class - made sense to me :p
'       Deprecated LShiftWord and RShiftWord since VB now supports << and >> operators
'       Converted other functions to use standard .NET bit conversion operations, this will
'           be more reliable and more OS portable than having to deal with the "sign" bit
'           as the older code was doing...

Option Explicit On 

Namespace [Shared]
    ''' <summary>
    ''' Bit Manipulation Functions
    ''' </summary>

    Public Class Bit

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Byte 0
        Public Const Bit0 As Byte = &H1             ' &H00000001    00000001 = 1
        Public Const Bit1 As Byte = &H2             ' &H00000002    00000010 = 2
        Public Const Bit2 As Byte = &H4             ' &H00000004    00000100 = 4
        Public Const Bit3 As Byte = &H8             ' &H00000008    00001000 = 8
        Public Const Bit4 As Byte = &H10            ' &H00000010    00010000 = 16
        Public Const Bit5 As Byte = &H20            ' &H00000020    00100000 = 32
        Public Const Bit6 As Byte = &H40            ' &H00000040    01000000 = 64
        Public Const Bit7 As Byte = &H80            ' &H00000080    10000000 = 128

        ' Byte 1
        Public Const Bit8 As Int16 = 256            ' &H00000100
        Public Const Bit9 As Int16 = 512            ' &H00000200
        Public Const Bit10 As Int16 = 1024          ' &H00000400
        Public Const Bit11 As Int16 = 2048          ' &H00000800
        Public Const Bit12 As Int16 = 4096          ' &H00001000
        Public Const Bit13 As Int16 = 8192          ' &H00002000
        Public Const Bit14 As Int16 = 16384         ' &H00004000
        Public Const Bit15 As Int16 = -32768        ' &H00008000

        ' Byte 2
        Public Const Bit16 As Int32 = 65536         ' &H00010000
        Public Const Bit17 As Int32 = 131072        ' &H00020000
        Public Const Bit18 As Int32 = 262144        ' &H00040000
        Public Const Bit19 As Int32 = 524288        ' &H00080000
        Public Const Bit20 As Int32 = 1048576       ' &H00100000
        Public Const Bit21 As Int32 = 2097152       ' &H00200000
        Public Const Bit22 As Int32 = 4194304       ' &H00400000
        Public Const Bit23 As Int32 = 8388608       ' &H00800000

        ' Byte 3
        Public Const Bit24 As Int32 = 16777216      ' &H01000000
        Public Const Bit25 As Int32 = 33554432      ' &H02000000
        Public Const Bit26 As Int32 = 67108864      ' &H04000000
        Public Const Bit27 As Int32 = 134217728     ' &H08000000
        Public Const Bit28 As Int32 = 268435456     ' &H10000000
        Public Const Bit29 As Int32 = 536870912     ' &H20000000
        Public Const Bit30 As Int32 = 1073741824    ' &H40000000
        Public Const Bit31 As Int32 = -2147483648   ' &H80000000

        ''' <summary>
        ''' <para>Returns the high byte (Int8) from a word (Int16).  On Intel platforms this should return the high-order byte of a 16-bit integer value, i.e., the byte value whose in-memory representation is the same as the left-most, most-significant-byte of the integer value.</para>
        ''' </summary>
       
        Public Shared Function HiByte(ByVal w As Int16) As Byte

            Return BitConverter.GetBytes(w)(0)
            'If w And &H8000 Then
            '    Return &H80 Or ((w And &H7FFF) \ 256)
            'Else
            '    Return w \ 256
            'End If

        End Function
        ''' <summary>
        ''' <para> Returns the high word (Int16) from a double word (Int32).  On Intel platforms this should return the high-order word of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the left-most, most-significant-word of the integer value.</para>
        ''' </summary>
        ''' <param name="dw">Double Word</param>

        ' Returns the high word (Int16) from a double word (Int32).  On Intel platforms this should return the high-order word
        ' of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the left-most,
        ' most-significant-word of the integer value.
        Public Shared Function HiWord(ByVal dw As Int32) As Int16

            Return BitConverter.ToInt16(BitConverter.GetBytes(dw), 0)
            'If dw And &H80000000 Then
            '    Return &H8000 Or ((dw And &H7FFF0000) \ 65536)
            'Else
            '    Return dw \ 65536
            'End If

        End Function
        ''' <summary>
        ''' <para>Returns the low byte (Int8) from a word (Int16).  On Intel platforms this should return the low-order byte of a 16-bit integer value, i.e., the byte value whose in-memory representation is the same as the right-most,least-significant-byte of the integer value.</para>
        ''' </summary>
        ''' <param name="w">Word</param>

        ' Returns the low byte (Int8) from a word (Int16).  On Intel platforms this should return the low-order byte
        ' of a 16-bit integer value, i.e., the byte value whose in-memory representation is the same as the right-most,
        ' least-significant-byte of the integer value.
        Public Shared Function LoByte(ByVal w As Int16) As Byte

            Return BitConverter.GetBytes(w)(1)
            'Return w And &HFF

        End Function
        ''' <summary>
        ''' <para>Returns the low word (Int16) from a double word (Int32).  On Intel platforms this should return the low-order word of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the right-most, least-significant-word of the integer value.</para>
        ''' </summary>
        ''' <param name="dw">Double Word</param>

        ' Returns the low word (Int16) from a double word (Int32).  On Intel platforms this should return the low-order word
        ' of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the right-most,
        ' least-significant-word of the integer value.
        Public Shared Function LoWord(ByVal dw As Int32) As Int16

            Return BitConverter.ToInt16(BitConverter.GetBytes(dw), 2)
            'If dw And &H8000 Then
            '    Return &H8000 Or (dw And &H7FFF)
            'Else
            '    Return dw And &HFFFF
            'End If

        End Function
        ''' <summary>
        ''' <para>Bits shifts word (Int16) value to the left "n" times.</para>
        ''' </summary>
        ''' <param name="w">Word</param>
        ''' <param name="n">Number of times to shift value to left</param>

        ' Bits shifts word (Int16) value to the left "n" times
        <Obsolete("This function has been deprecated, just use new << operator.  Note that this function may be removed from future versions.")> _
        Public Shared Function LShiftWord(ByVal w As Int16, ByVal n As Int16) As Int16

            Return w << n
            'Dim dw As Int32

            'dw = w * (2 ^ n)

            'If dw And &H8000 Then
            '    Return Convert.ToInt16(dw And &H7FFF) Or &H8000
            'Else
            '    Return dw And &HFFFF
            'End If

        End Function
        ''' <summary>
        ''' <para>Bits shifts word (Int16) value to the right "n" times.</para>
        ''' </summary>
        ''' <param name="w">Word</param>
        ''' <param name="n">Number of times to shift value to right</param>

        ' Bits shifts word (Int16) value to the right "n" times
        <Obsolete("This function has been deprecated, just use new >> operator.  Note that this function may be removed from future versions.")> _
        Public Shared Function RShiftWord(ByVal w As Int16, ByVal n As Int16) As Int16

            Return w >> n
            'Dim dw As Int32

            'If n = 0 Then
            '    Return w
            'Else
            '    dw = w And &HFFFF
            '    dw = dw \ (2 ^ n)
            '    Return dw And &HFFFF
            'End If

        End Function
        ''' <summary>
        ''' <para>Makes a word (Int16) from two bytes (Int8)</para>
        ''' </summary>
        ''' <param name="bHi">Byte</param>
        ''' <param name="bLo">Byte</param>

        ' Makes a word (Int16) from two bytes (Int8)
        Public Shared Function MakeWord(ByVal bHi As Byte, ByVal bLo As Byte) As Int16

            Return BitConverter.ToInt16(New Byte() {bHi, bLo}, 0)
            'If bHi And &H80 Then
            '    Return (((bHi And &H7F) * 256) + bLo) Or &H8000
            'Else
            '    Return (bHi * 256) + bLo
            'End If

        End Function
        ''' <summary>
        ''' <para> Makes a double word (Int32) from two words (Int16)</para>
        ''' </summary>
        ''' <param name="wHi">Word</param>
        ''' <param name="wLo">Word</param>
        ' Makes a double word (Int32) from two words (Int16)
        Public Shared Function MakeDWord(ByVal wHi As Int16, ByVal wLo As Int16) As Int32

            Dim bytes As Byte() = Array.CreateInstance(GetType(Byte), 4)

            Array.Copy(BitConverter.GetBytes(wHi), 0, bytes, 0, 2)
            Array.Copy(BitConverter.GetBytes(wLo), 0, bytes, 2, 2)

            Return BitConverter.ToInt32(bytes, 0)
            'If wHi And &H8000 Then
            '    Return (((wHi And &H7FFF) * 65536) Or (wLo And &HFFFF)) Or &H80000000
            'Else
            '    Return (wHi * 65536) + (wLo And &HFFFF)
            'End If

        End Function

    End Class

End Namespace