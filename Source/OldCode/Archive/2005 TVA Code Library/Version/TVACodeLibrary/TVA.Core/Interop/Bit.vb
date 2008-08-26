'*******************************************************************************************************
'  TVA.Interop.Bit.vb - Bit Manipulation Functions
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
'  02/24/2004 - J. Ritchie Carroll
'       Original version of source code generated
'  01/14/2005 - J. Ritchie Carroll
'       Moved bit constants into Bit class - made sense to me :p
'       Deprecated LShiftWord and RShiftWord since VB now supports << and >> operators
'       Converted other functions to use standard .NET bit conversion operations, this will
'           be more reliable and more OS portable than having to deal with the "sign" bit
'           as the older code was doing...
'  12/29/2005 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Bit)
'  01/04/2006 - J. Ritchie Carroll
'       Added code comments - moved into Interop namespace
'  10/10/2007 - J. Ritchie Carroll
'       Added bit-rotation functions (BitRotL and BitRotR)
'
'*******************************************************************************************************

Imports TVA.Common

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

        ''' <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the left the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotL(ByVal value As Byte, ByVal rotations As Integer) As Byte

            Dim hiBitSet As Boolean

            For x As Integer = 1 To (rotations Mod 8)
                hiBitSet = ((value And Bit7) = Bit7)
                value <<= 1
                If hiBitSet Then value = (value Or Bit0)
            Next

            Return value

        End Function

        ''' <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the left the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotL(ByVal value As Int16, ByVal rotations As Integer) As Int16

            Dim hiBitSet As Boolean

            For x As Integer = 1 To (rotations Mod 16)
                hiBitSet = ((value And Bit15) = Bit15)
                value <<= 1
                If hiBitSet Then value = (value Or Bit0)
            Next

            Return value

        End Function

        ''' <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the left the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotL(ByVal value As Int24, ByVal rotations As Integer) As Int24

            Dim hiBitSet As Boolean
            Dim int24Bit0 As Int24 = Bit0
            Dim int24Bit23 As Int24 = Bit23

            For x As Integer = 1 To (rotations Mod 24)
                hiBitSet = ((value And int24Bit23) = int24Bit23)
                value <<= 1
                If hiBitSet Then value = (value Or int24Bit0)
            Next

            Return value

        End Function

        ''' <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the left the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotL(ByVal value As Int32, ByVal rotations As Integer) As Int32

            Dim hiBitSet As Boolean

            For x As Integer = 1 To (rotations Mod 32)
                hiBitSet = ((value And Bit31) = Bit31)
                value <<= 1
                If hiBitSet Then value = (value Or Bit0)
            Next

            Return value

        End Function

        ''' <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the right the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotR(ByVal value As Byte, ByVal rotations As Integer) As Byte

            Dim loBitSet As Boolean

            For x As Integer = 1 To (rotations Mod 8)
                loBitSet = ((value And Bit0) = Bit0)
                value >>= 1

                If loBitSet Then
                    value = (value Or Bit7)
                Else
                    value = (value And Not Bit7)
                End If
            Next

            Return value

        End Function

        ''' <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the right the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotR(ByVal value As Int16, ByVal rotations As Integer) As Int16

            Dim loBitSet As Boolean

            For x As Integer = 1 To (rotations Mod 16)
                loBitSet = ((value And Bit0) = Bit0)
                value >>= 1

                If loBitSet Then
                    value = (value Or Bit15)
                Else
                    value = (value And Not Bit15)
                End If
            Next

            Return value

        End Function

        ''' <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the right the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotR(ByVal value As Int24, ByVal rotations As Integer) As Int24

            Dim loBitSet As Boolean
            Dim int24Bit0 As Int24 = Bit0
            Dim int24Bit23 As Int24 = Bit23

            For x As Integer = 1 To (rotations Mod 24)
                loBitSet = ((value And int24Bit0) = int24Bit0)
                value >>= 1

                If loBitSet Then
                    value = (value Or int24Bit23)
                Else
                    value = (value And Not int24Bit23)
                End If
            Next

            Return value

        End Function

        ''' <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        ''' <param name="value">Value used for bit-rotation</param>
        ''' <param name="rotations">Number of rotations to perform</param>
        ''' <returns>Value that has its bits rotated to the right the specified number of times</returns>
        ''' <remarks>
        ''' Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        ''' .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        ''' architectures.
        ''' </remarks>
        Public Shared Function BitRotR(ByVal value As Int32, ByVal rotations As Integer) As Int32

            Dim loBitSet As Boolean

            For x As Integer = 1 To (rotations Mod 32)
                loBitSet = ((value And Bit0) = Bit0)
                value >>= 1

                If loBitSet Then
                    value = (value Or Bit31)
                Else
                    value = (value And Not Bit31)
                End If
            Next

            Return value

        End Function

        ''' <summary>Returns the high-byte from a word (Int16).</summary>
        ''' <param name="word">2-byte, 16-bit signed integer value.</param>
        ''' <returns>The high-order byte of the specified 16-bit signed integer value.</returns>
        ''' <remarks>
        ''' On little-endian architectures (e.g., Intel platforms), this will be the byte value whose in-memory representation
        ''' is the same as the right-most, most-significant-byte of the integer value.
        ''' </remarks>
        Public Shared Function HiByte(ByVal word As Int16) As Byte

            If BitConverter.IsLittleEndian Then
                Return BitConverter.GetBytes(word)(1)
            Else
                Return BitConverter.GetBytes(word)(0)
            End If

        End Function

        ''' <summary>Returns the high-word (Int16) from a double-word (Int32).</summary>
        ''' <param name="doubleWord">4-byte, 32-bit signed integer value.</param>
        ''' <returns>The high-order word of the specified 32-bit signed integer value.</returns>
        ''' <remarks>
        ''' On little-endian architectures (e.g., Intel platforms), this will be the word value
        ''' whose in-memory representation is the same as the right-most, most-significant-word
        ''' of the integer value.
        ''' </remarks>
        Public Shared Function HiWord(ByVal doubleWord As Int32) As Int16

            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2)
            Else
                Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0)
            End If

        End Function

        ''' <summary>Returns the low-byte from a word (Int16).</summary>
        ''' <param name="word">2-byte, 16-bit signed integer value.</param>
        ''' <returns>The low-order byte of the specified 16-bit signed integer value.</returns>
        ''' <remarks>
        ''' On little-endian architectures (e.g., Intel platforms), this will be the byte value
        ''' whose in-memory representation is the same as the left-most, least-significant-byte
        ''' of the integer value.
        ''' </remarks>
        Public Shared Function LoByte(ByVal word As Int16) As Byte

            If BitConverter.IsLittleEndian Then
                Return BitConverter.GetBytes(word)(0)
            Else
                Return BitConverter.GetBytes(word)(1)
            End If

        End Function

        ''' <summary>Returns the low-word (Int16) from a double-word (Int32).</summary>
        ''' <param name="doubleWord">4-byte, 32-bit signed integer value.</param>
        ''' <returns>The low-order word of the specified 32-bit signed integer value.</returns>
        ''' <remarks>
        ''' On little-endian architectures (e.g., Intel platforms), this will be the word value
        ''' whose in-memory representation is the same as the left-most, least-significant-word
        ''' of the integer value.
        ''' </remarks>
        Public Shared Function LoWord(ByVal doubleWord As Int32) As Int16

            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0)
            Else
                Return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2)
            End If

        End Function

        ''' <summary>Makes a word (Int16) from two bytes.</summary>
        ''' <returns>A 16-bit word made from the two specified bytes.</returns>
        Public Shared Function MakeWord(ByVal high As Byte, ByVal low As Byte) As Int16

            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt16(New Byte() {low, high}, 0)
            Else
                Return BitConverter.ToInt16(New Byte() {high, low}, 0)
            End If

        End Function

        ''' <summary>Makes a double-word (Int32) from two words (Int16).</summary>
        ''' <returns>A 32-bit double-word made from the two specified 16-bit words.</returns>
        Public Shared Function MakeDWord(ByVal high As Int16, ByVal low As Int16) As Int32

            Dim bytes As Byte() = CreateArray(Of Byte)(4)

            If BitConverter.IsLittleEndian Then
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 0, 2)
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 2, 2)
            Else
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 0, 2)
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 2, 2)
            End If

            Return BitConverter.ToInt32(bytes, 0)

        End Function

    End Class

End Namespace