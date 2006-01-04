'*******************************************************************************************************
'  Tva.Bit.vb - Bit Manipulation Functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  ??/??/200? - James R Carroll
'       Original version of source code generated
'  01/14/2005 - James R Carroll
'       Moved bit constants into Bit class - made sense to me :p
'       Deprecated LShiftWord and RShiftWord since VB now supports << and >> operators
'       Converted other functions to use standard .NET bit conversion operations, this will
'           be more reliable and more OS portable than having to deal with the "sign" bit
'           as the older code was doing...
'  12/29/2005 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Bit)
'
'*******************************************************************************************************

Public NotInheritable Class Bit

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ' Byte 0
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
    ''' <para>Performs proper bitwise conversion between signed and unsigned value</para>
    ''' </summary>
    ''' <remarks>
    ''' <para>This function is useful because Convert.ToUInt16 will throw an OverflowException for values less than zero.</para>
    ''' <para>For example, converts signed 16-bit integer -32768 (i.e., Int16.MinValue) to unsigned 16-bit integer 0 (i.e., UInt16.MinValue).</para>
    ''' </remarks>
    <CLSCompliant(False)> _
    Public Shared ReadOnly Property ToUInt16(ByVal signedInt As Int16) As UInt16
        Get
            Return BitConverter.ToUInt16(BitConverter.GetBytes(signedInt), 0)
        End Get
    End Property

    ''' <summary>
    ''' <para>Performs proper bitwise conversion between signed and unsigned value</para>
    ''' </summary>
    ''' <remarks>
    ''' <para>This function is useful because Convert.ToUInt32 will throw an OverflowException for values less than zero.</para>
    ''' <para>For example, converts signed 32-bit integer -2147483648 (i.e., Int32.MinValue) to unsigned 32-bit integer 0 (i.e., UInt32.MinValue).</para>
    ''' </remarks>
    <CLSCompliant(False)> _
    Public Shared ReadOnly Property ToUInt32(ByVal signedInt As Int32) As UInt32
        Get
            Return BitConverter.ToUInt32(BitConverter.GetBytes(signedInt), 0)
        End Get
    End Property

    ''' <summary>
    ''' <para>Performs proper bitwise conversion between signed and unsigned value</para>
    ''' </summary>
    ''' <remarks>
    ''' <para>This function is useful because Convert.ToUInt64 will throw an OverflowException for values less than zero.</para>
    ''' <para>For example, converts signed 64-bit integer -9223372036854775808 (i.e., Int64.MinValue) to unsigned 64-bit integer 0 (i.e., UInt64.MinValue).</para>
    ''' </remarks>
    <CLSCompliant(False)> _
    Public Shared ReadOnly Property ToUInt64(ByVal signedInt As Int64) As UInt64
        Get
            Return BitConverter.ToUInt64(BitConverter.GetBytes(signedInt), 0)
        End Get
    End Property

    ''' <summary>
    ''' <para>
    ''' Returns the high byte (Int8) from a word (Int16).  On Intel platforms this should return the high-order byte 
    ''' of a 16-bit integer value, i.e., the byte value whose in-memory representation is the same as the left-most, 
    ''' most-significant-byte of the integer value.
    ''' </para>
    ''' </summary>
    ''' <param name="word"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function HiByte(ByVal word As Int16) As Byte

        Return BitConverter.GetBytes(word)(0)

    End Function

    ''' <summary>
    ''' <para>
    ''' Returns the high word (Int16) from a double word (Int32).  On Intel platforms this should return the high-order word
    ''' of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the left-most,
    ''' most-significant-word of the integer value.
    ''' </para>
    ''' </summary>
    ''' <param name="doubleWord"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function HiWord(ByVal doubleWord As Int32) As Int16

        Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0)

    End Function

    ''' <summary>
    ''' <para>
    ''' Returns the low byte (Int8) from a word (Int16).  On Intel platforms this should return the low-order byte
    ''' of a 16-bit integer value, i.e., the byte value whose in-memory representation is the same as the right-most,
    ''' least-significant-byte of the integer value.
    ''' </para>
    ''' </summary>
    ''' <param name="word"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LoByte(ByVal word As Int16) As Byte

        Return BitConverter.GetBytes(word)(1)

    End Function

    ''' <summary>
    ''' <para>
    ''' Returns the low word (Int16) from a double word (Int32).  On Intel platforms this should return the low-order word
    ''' of a 32-bit integer value, i.e., the word value whose in-memory representation is the same as the right-most,
    ''' least-significant-word of the integer value.
    ''' </para>
    ''' </summary>
    ''' <param name="doubleWord"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LoWord(ByVal doubleWord As Int32) As Int16

        Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2)

    End Function

    ''' <summary>
    ''' <para>Bits shifts word (Int16) value to the left "n" times</para>
    ''' </summary>
    ''' <param name="word"></param>
    ''' <param name="shiftCount"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Obsolete("This function has been deprecated, just use new << operator.  Note that this function may be removed from future versions.")> _
    Public Shared Function LShiftWord(ByVal word As Int16, ByVal shiftCount As Int16) As Int16

        Return word << shiftCount

    End Function

    ''' <summary>
    ''' <para>Bits shifts word (Int16) value to the right "n" times</para>
    ''' </summary>
    ''' <param name="word"></param>
    ''' <param name="shiftCount"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Obsolete("This function has been deprecated, just use new >> operator.  Note that this function may be removed from future versions.")> _
    Public Shared Function RShiftWord(ByVal word As Int16, ByVal shiftCount As Int16) As Int16

        Return word >> shiftCount

    End Function

    ''' <summary>
    ''' <para>Makes a word (Int16) from two bytes (Int8)</para>
    ''' </summary>
    ''' <param name="high"></param>
    ''' <param name="low"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MakeWord(ByVal high As Byte, ByVal low As Byte) As Int16

        Return BitConverter.ToInt16(New Byte() {high, low}, 0)

    End Function

    ''' <summary>
    ''' <para>Makes a double word (Int32) from two words (Int16)</para>
    ''' </summary>
    ''' <param name="high"></param>
    ''' <param name="low"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MakeDWord(ByVal high As Int16, ByVal low As Int16) As Int32

        Dim bytes As Byte() = Array.CreateInstance(GetType(Byte), 4)

        Array.Copy(BitConverter.GetBytes(high), 0, bytes, 0, 2)
        Array.Copy(BitConverter.GetBytes(low), 0, bytes, 2, 2)

        Return BitConverter.ToInt32(bytes, 0)

    End Function

End Class
