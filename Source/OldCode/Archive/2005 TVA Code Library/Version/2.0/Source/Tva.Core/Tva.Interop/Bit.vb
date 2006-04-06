'*******************************************************************************************************
'  Tva.Interop.Bit.vb - Bit Manipulation Functions
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
'  02/24/2004 - James R Carroll
'       Original version of source code generated
'  01/14/2005 - James R Carroll
'       Moved bit constants into Bit class - made sense to me :p
'       Deprecated LShiftWord and RShiftWord since VB now supports << and >> operators
'       Converted other functions to use standard .NET bit conversion operations, this will
'           be more reliable and more OS portable than having to deal with the "sign" bit
'           as the older code was doing...
'  12/29/2005 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Bit)
'  01/04/2006 - James R Carroll
'       Added code comments - moved into Interop namespace
'
'*******************************************************************************************************

Namespace Interop

    Public NotInheritable Class Bit

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>No bits set (8-bit)</summary>
        Public Const Nill As Byte = 0

        ''' <summary>No bits set (16-bit)</summary>
        Public Const Nill16 As Int16 = 0

        ''' <summary>No bits set (32-bit)</summary>
        Public Const Nill32 As Int32 = 0

        ' Byte 0, Bits 0-7

        ''' <summary>Bit 0 (0x00000001)</summary>
        Public Const Bit0 As Byte = &H1             ' &H00000001    00000001 = 1

        ''' <summary>Bit 1 (0x00000002)</summary>
        Public Const Bit1 As Byte = &H2             ' &H00000002    00000010 = 2

        ''' <summary>Bit 2 (0x00000004)</summary>
        Public Const Bit2 As Byte = &H4             ' &H00000004    00000100 = 4

        ''' <summary>Bit 3 (0x00000008)</summary>
        Public Const Bit3 As Byte = &H8             ' &H00000008    00001000 = 8

        ''' <summary>Bit 4 (0x00000010)</summary>
        Public Const Bit4 As Byte = &H10            ' &H00000010    00010000 = 16

        ''' <summary>Bit 6 (0x00000020)</summary>
        Public Const Bit5 As Byte = &H20            ' &H00000020    00100000 = 32

        ''' <summary>Bit 6 (0x00000040)</summary>
        Public Const Bit6 As Byte = &H40            ' &H00000040    01000000 = 64

        ''' <summary>Bit 7 (0x00000080)</summary>
        Public Const Bit7 As Byte = &H80            ' &H00000080    10000000 = 128

        ' Byte 1, Bits 8-15

        ''' <summary>Bit 8 (0x00000100)</summary>
        Public Const Bit8 As Int16 = 256            ' &H00000100

        ''' <summary>Bit 9 (0x00000200)</summary>
        Public Const Bit9 As Int16 = 512            ' &H00000200

        ''' <summary>Bit 10 (0x00000400)</summary>
        Public Const Bit10 As Int16 = 1024          ' &H00000400

        ''' <summary>Bit 11 (0x00000800)</summary>
        Public Const Bit11 As Int16 = 2048          ' &H00000800

        ''' <summary>Bit 12 (0x00001000)</summary>
        Public Const Bit12 As Int16 = 4096          ' &H00001000

        ''' <summary>Bit 13 (0x00002000)</summary>
        Public Const Bit13 As Int16 = 8192          ' &H00002000

        ''' <summary>Bit 14 (0x00004000)</summary>
        Public Const Bit14 As Int16 = 16384         ' &H00004000

        ''' <summary>Bit 15 (0x00008000)</summary>
        Public Const Bit15 As Int16 = -32768        ' &H00008000

        ' Byte 2, Bits 16-23

        ''' <summary>Bit 16 (0x00010000)</summary>
        Public Const Bit16 As Int32 = 65536         ' &H00010000

        ''' <summary>Bit 17 (0x00020000)</summary>
        Public Const Bit17 As Int32 = 131072        ' &H00020000

        ''' <summary>Bit 18 (0x00040000)</summary>
        Public Const Bit18 As Int32 = 262144        ' &H00040000

        ''' <summary>Bit 19 (0x00080000)</summary>
        Public Const Bit19 As Int32 = 524288        ' &H00080000

        ''' <summary>Bit 20 (0x00100000)</summary>
        Public Const Bit20 As Int32 = 1048576       ' &H00100000

        ''' <summary>Bit 21 (0x00200000)</summary>
        Public Const Bit21 As Int32 = 2097152       ' &H00200000

        ''' <summary>Bit 22 (0x00400000)</summary>
        Public Const Bit22 As Int32 = 4194304       ' &H00400000

        ''' <summary>Bit 23 (0x00800000)</summary>
        Public Const Bit23 As Int32 = 8388608       ' &H00800000

        ' Byte 3, Bits 24-31

        ''' <summary>Bit 24 (0x01000000)</summary>
        Public Const Bit24 As Int32 = 16777216      ' &H01000000

        ''' <summary>Bit 25 (0x02000000)</summary>
        Public Const Bit25 As Int32 = 33554432      ' &H02000000

        ''' <summary>Bit 26 (0x04000000)</summary>
        Public Const Bit26 As Int32 = 67108864      ' &H04000000

        ''' <summary>Bit 27 (0x08000000)</summary>
        Public Const Bit27 As Int32 = 134217728     ' &H08000000

        ''' <summary>Bit 28 (0x10000000)</summary>
        Public Const Bit28 As Int32 = 268435456     ' &H10000000

        ''' <summary>Bit 29 (0x20000000)</summary>
        Public Const Bit29 As Int32 = 536870912     ' &H20000000

        ''' <summary>Bit 30 (0x40000000)</summary>
        Public Const Bit30 As Int32 = 1073741824    ' &H40000000

        ''' <summary>Bit 31 (0x80000000)</summary>
        Public Const Bit31 As Int32 = -2147483648   ' &H80000000

        ''' <summary>
        ''' Returns the high byte (Int8) from a word (Int16).  On Intel platforms this should return the high-order byte 
        ''' of a 16-bit integer value, i.e., the byte value whose in-memory representation is the same as the left-most, 
        ''' most-significant-byte of the integer value.
        ''' </summary>
        Public Shared Function HiByte(ByVal word As Int16) As Byte

            Return BitConverter.GetBytes(word)(0)

        End Function

        ''' <summary>
        ''' Returns the high word (Int16) from a double word (Int32).  On Intel platforms this should return the high-order word
        ''' of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the left-most,
        ''' most-significant-word of the integer value.
        ''' </summary>
        Public Shared Function HiWord(ByVal doubleWord As Int32) As Int16

            Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0)

        End Function

        ''' <summary>
        ''' Returns the low byte (Int8) from a word (Int16).  On Intel platforms this should return the low-order byte
        ''' of a 16-bit integer value, i.e., the byte value whose in-memory representation is the same as the right-most,
        ''' least-significant-byte of the integer value.
        ''' </summary>
        Public Shared Function LoByte(ByVal word As Int16) As Byte

            Return BitConverter.GetBytes(word)(1)

        End Function

        ''' <summary>
        ''' Returns the low word (Int16) from a double word (Int32).  On Intel platforms this should return the low-order word
        ''' of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the right-most,
        ''' least-significant-word of the integer value.
        ''' </summary>
        Public Shared Function LoWord(ByVal doubleWord As Int32) As Int16

            Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2)

        End Function

        ''' <summary>Bits shifts word (Int16) value to the left "n" times</summary>
        <Obsolete("This function has been deprecated, just use new << operator.  Note that this function may be removed from future versions.")> _
        Public Shared Function LShiftWord(ByVal word As Int16, ByVal shiftCount As Int16) As Int16

            Return word << shiftCount

        End Function

        ''' <summary>Bits shifts word (Int16) value to the right "n" times</summary>
        <Obsolete("This function has been deprecated, just use new >> operator.  Note that this function may be removed from future versions.")> _
        Public Shared Function RShiftWord(ByVal word As Int16, ByVal shiftCount As Int16) As Int16

            Return word >> shiftCount

        End Function

        ''' <summary>Makes a word (Int16) from two bytes (Int8).</summary>
        Public Shared Function MakeWord(ByVal high As Byte, ByVal low As Byte) As Int16

            Return BitConverter.ToInt16(New Byte() {high, low}, 0)

        End Function

        ''' <summary>Makes a double word (Int32) from two words (Int16).</summary>
        Public Shared Function MakeDWord(ByVal high As Int16, ByVal low As Int16) As Int32

            Dim bytes As Byte() = Array.CreateInstance(GetType(Byte), 4)

            Array.Copy(BitConverter.GetBytes(high), 0, bytes, 0, 2)
            Array.Copy(BitConverter.GetBytes(low), 0, bytes, 2, 2)

            Return BitConverter.ToInt32(bytes, 0)

        End Function

    End Class

End Namespace