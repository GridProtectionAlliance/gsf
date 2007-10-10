'*******************************************************************************************************
'  TVA.Int24.vb - Representation of a 3-byte, 24-bit signed integer
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
'  10/04/2007 - J. Ritchie Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Option Strict On

Imports System.Runtime.InteropServices
Imports System.Globalization
Imports TVA.Common
Imports TVA.Interop.Bit

''' <summary>Represents a 24-bit signed integer.</summary>
''' <remarks>
''' <para>
''' This class behaves like most other intrinsic signed integers but allows a 3-byte, 24-bit integer implementation
''' that is often found in many digital-signal processing arenas and different kinds of protocol parsing.  A signed
''' 24-bit integer is typically used to save storage space on disk where its value range of -8388608 to 8388607 is
''' sufficient, but the signed Int16 value range of -32768 to 32767 is too small.
''' </para>
''' <para>
''' This structure uses an Int32 internally for storage and most other common expected integer functionality, so using
''' a 24-bit integer will not save memory.  However, if the 24-bit signed integer range (-8388608 to 8388607) suits your
''' data needs you can save disk space by only storing the three bytes that this integer actually consumes.  You can do
''' this by calling the Int24.GetBytes function to return a three binary byte array that can be serialized to the desired
''' destination and then calling the Int24.GetValue function to restore the Int24 value from those three bytes.
''' </para>
''' <para>
''' All the standard operators for the Int24 have been fully defined for use with both Int24 and Int32 signed integers;
''' you should find that without the exception Int24 can be compared and numerically calculated with an Int24 or Int32.
''' Necessary casting should be minimal and typical use should be very simple - just as if you are using any other native
''' signed integer.
''' </para>
''' </remarks>
<Serializable()> _
Public Structure Int24

    Implements IComparable, IFormattable, IConvertible, IComparable(Of Int24), IComparable(Of Int32), IEquatable(Of Int24), IEquatable(Of Int32)

#Region " Public Constants "

    ''' <summary>High byte bit-mask used when a 24-bit integer is stored within a 32-bit integer. This field is constant.</summary>
    Public Const BitMask As Int32 = (Bit24 Or Bit25 Or Bit26 Or Bit27 Or Bit28 Or Bit29 Or Bit30 Or Bit31)

    ''' <summary>Represents the largest possible value of an Int24 as an Int32. This field is constant.</summary>
    Public Const MaxValue32 As Int32 = 8388607

    ''' <summary>Represents the smallest possible value of an Int24 as an Int32. This field is constant.</summary>
    Public Const MinValue32 As Int32 = -8388608

#End Region

#Region " Member Fields "

    ' We internally store the Int24 value in a 4-byte integer for convenience
    Private m_value As Int32

    Private Shared m_maxValue As Int24
    Private Shared m_minValue As Int24

#End Region

#Region " Constructors "

    Shared Sub New()

        m_maxValue = New Int24(MaxValue32)
        m_minValue = New Int24(MinValue32)

    End Sub

    ''' <summary>Creates 24-bit signed integer from an existing 24-bit signed integer.</summary>
    Public Sub New(ByVal value As Int24)

        m_value = ApplyBitMask(CType(value, Int32))

    End Sub

    ''' <summary>Creates 24-bit signed integer from a 32-bit signed integer.</summary>
    ''' <param name="value">32-bit signed integer to use as new 24-bit signed integer value.</param>
    ''' <exception cref="OverflowException">Source values outside 24-bit min/max range will cause an overflow exception.</exception>
    Public Sub New(ByVal value As Int32)

        ValidateNumericRange(value)
        m_value = ApplyBitMask(value)

    End Sub

    ''' <summary>Creates 24-bit signed integer from three bytes at a specified position in a byte array.</summary>
    ''' <param name="value">An array of bytes.</param>
    ''' <param name="startIndex">The starting position within value.</param>
    ''' <remarks>
    ''' <para>You can use this constructor in-lieu of a System.BitConverter.ToInt24 function.</para>
    ''' <para>Bytes endian order assumed to match that of currently executing process architecture (little-endian on Intel platforms).</para>
    ''' </remarks>
    Public Sub New(ByVal value As Byte(), ByVal startIndex As Integer)

        m_value = ApplyBitMask(CType(Int24.GetValue(value, startIndex), Int32))

    End Sub

#End Region

#Region " BitConverter Stand-in Operations "

    ''' <summary>Returns the Int24 value as an array of three bytes.</summary>
    ''' <returns>An array of bytes with length 3.</returns>
    ''' <remarks>
    ''' <para>You can use this function in-lieu of a System.BitConverter.GetBytes function.</para>
    ''' <para>Bytes will be returned in endian order of currently executing process architecture (little-endian on Intel platforms).</para>
    ''' </remarks>
    Public Function GetBytes() As Byte()

        ' Return serialized 3-byte representation of Int24
        Return Int24.GetBytes(Me)

    End Function

    ''' <summary>Returns the specified Int24 value as an array of three bytes.</summary>
    ''' <param name="value">Int24 value to </param>
    ''' <returns>An array of bytes with length 3.</returns>
    ''' <remarks>
    ''' <para>You can use this function in-lieu of a System.BitConverter.GetBytes function.</para>
    ''' <para>Bytes will be returned in endian order of currently executing process architecture (little-endian on Intel platforms).</para>
    ''' </remarks>
    Public Shared Function GetBytes(ByVal value As Int24) As Byte()

        ' We use a 32-bit integer to store 24-bit integer internally
        Dim int32Bytes As Byte() = BitConverter.GetBytes(CType(value, Int32))
        Dim int24Bytes As Byte() = CreateArray(Of Byte)(3)

        If BitConverter.IsLittleEndian Then
            ' Copy little-endian bytes starting at index 0
            Buffer.BlockCopy(int32Bytes, 0, int24Bytes, 0, 3)
        Else
            ' Copy big-endian bytes starting at index 1
            Buffer.BlockCopy(int32Bytes, 1, int24Bytes, 0, 3)
        End If

        ' Return serialized 3-byte representation of Int24
        Return int24Bytes

    End Function

    ''' <summary>Returns a 24-bit signed integer from three bytes at a specified position in a byte array.</summary>
    ''' <param name="value">An array of bytes.</param>
    ''' <param name="startIndex">The starting position within value.</param>
    ''' <returns>A 24-bit signed integer formed by three bytes beginning at startIndex.</returns>
    ''' <remarks>
    ''' <para>You can use this function in-lieu of a System.BitConverter.ToInt24 function.</para>
    ''' <para>Bytes endian order assumed to match that of currently executing process architecture (little-endian on Intel platforms).</para>
    ''' </remarks>
    Public Shared Function GetValue(ByVal value As Byte(), ByVal startIndex As Integer) As Int24

        ' We use a 32-bit integer to store 24-bit integer internally
        Dim bytes As Byte() = CreateArray(Of Byte)(4)

        If BitConverter.IsLittleEndian Then
            ' Copy little-endian bytes starting at index 0 leaving byte at index 3 blank
            Buffer.BlockCopy(value, 0, bytes, 0, 3)
        Else
            ' Copy big-endian bytes starting at index 1 leaving byte at index 0 blank
            Buffer.BlockCopy(value, 0, bytes, 1, 3)
        End If

        ' Deserialize value
        Return CType(ApplyBitMask(BitConverter.ToInt32(bytes, 0)), Int24)

    End Function

#End Region

#Region " Int24 Operators "

    ' Every effort has been made to make Int24 as cleanly interoperable with Int32 as possible...

#Region " Comparison Operators "

    Public Shared Operator =(ByVal value1 As Int24, ByVal value2 As Int24) As Boolean

        Return value1.Equals(value2)

    End Operator

    Public Shared Operator =(ByVal value1 As Int32, ByVal value2 As Int24) As Boolean

        Return value1.Equals(CType(value2, Int32))

    End Operator

    Public Shared Operator =(ByVal value1 As Int24, ByVal value2 As Int32) As Boolean

        Return CType(value1, Int32).Equals(value2)

    End Operator

    Public Shared Operator <>(ByVal value1 As Int24, ByVal value2 As Int24) As Boolean

        Return Not value1.Equals(value2)

    End Operator

    Public Shared Operator <>(ByVal value1 As Int32, ByVal value2 As Int24) As Boolean

        Return Not value1.Equals(CType(value2, Int32))

    End Operator

    Public Shared Operator <>(ByVal value1 As Int24, ByVal value2 As Int32) As Boolean

        Return Not CType(value1, Int32).Equals(value2)

    End Operator

    Public Shared Operator <(ByVal value1 As Int24, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(value2) < 0)

    End Operator

    Public Shared Operator <(ByVal value1 As Int32, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(CType(value2, Int32)) < 0)

    End Operator

    Public Shared Operator <(ByVal value1 As Int24, ByVal value2 As Int32) As Boolean

        Return (value1.CompareTo(value2) < 0)

    End Operator

    Public Shared Operator <=(ByVal value1 As Int24, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(value2) <= 0)

    End Operator

    Public Shared Operator <=(ByVal value1 As Int32, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(CType(value2, Int32)) <= 0)

    End Operator

    Public Shared Operator <=(ByVal value1 As Int24, ByVal value2 As Int32) As Boolean

        Return (value1.CompareTo(value2) <= 0)

    End Operator

    Public Shared Operator >(ByVal value1 As Int24, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(value2) > 0)

    End Operator

    Public Shared Operator >(ByVal value1 As Int32, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(CType(value2, Int32)) > 0)

    End Operator

    Public Shared Operator >(ByVal value1 As Int24, ByVal value2 As Int32) As Boolean

        Return (value1.CompareTo(value2) > 0)

    End Operator

    Public Shared Operator >=(ByVal value1 As Int24, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(value2) >= 0)

    End Operator

    Public Shared Operator >=(ByVal value1 As Int32, ByVal value2 As Int24) As Boolean

        Return (value1.CompareTo(CType(value2, Int32)) >= 0)

    End Operator

    Public Shared Operator >=(ByVal value1 As Int24, ByVal value2 As Int32) As Boolean

        Return (value1.CompareTo(value2) >= 0)

    End Operator

#End Region

#Region " Type Conversion Operators "

#Region " Narrowing Conversions "

    Public Shared Narrowing Operator CType(ByVal value As String) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Decimal) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Double) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Single) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Int64) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Int32) As Int24

        Return New Int24(value)

    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Int24) As Int16

        Return CType(CType(value, Int32), Int16)

    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Int24) As Byte

        Return CType(CType(value, Int32), Byte)

    End Operator

#End Region

#Region " Widening Conversions "

    Public Shared Widening Operator CType(ByVal value As Byte) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Widening Operator CType(ByVal value As Char) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int16) As Int24

        Return New Int24(Convert.ToInt32(value))

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int24) As Int32

        Return value.ToInt32(Nothing)

    End Operator

    <CLSCompliant(False)> _
    Public Shared Widening Operator CType(ByVal value As Int24) As UInt32

        Return value.ToUInt32(Nothing)

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int24) As Int64

        Return value.ToInt64(Nothing)

    End Operator

    <CLSCompliant(False)> _
    Public Shared Widening Operator CType(ByVal value As Int24) As UInt64

        Return value.ToUInt64(Nothing)

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int24) As Double

        Return value.ToDouble(Nothing)

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int24) As Single

        Return value.ToSingle(Nothing)

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int24) As Decimal

        Return value.ToDecimal(Nothing)

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int24) As String

        Return value.ToString()

    End Operator

#End Region

#End Region

#Region " Boolean and Bitwise Operators "

    Public Shared Operator IsTrue(ByVal value As Int24) As Boolean

        Return (value <> 0)

    End Operator

    Public Shared Operator IsFalse(ByVal value As Int24) As Boolean

        Return (value = 0)

    End Operator

    Public Shared Operator Not(ByVal value As Int24) As Int24

        Return CType(ApplyBitMask(Not CType(value, Int32)), Int24)

    End Operator

    Public Shared Operator And(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(ApplyBitMask(CType(value1, Int32) And CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator And(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 And CType(value2, Int32))

    End Operator

    Public Shared Operator And(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) And value2)

    End Operator

    Public Shared Operator Or(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(ApplyBitMask(CType(value1, Int32) Or CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator Or(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 Or CType(value2, Int32))

    End Operator

    Public Shared Operator Or(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) Or value2)

    End Operator

    Public Shared Operator Xor(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(ApplyBitMask(CType(value1, Int32) Xor CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator Xor(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 Xor CType(value2, Int32))

    End Operator

    Public Shared Operator Xor(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) Xor value2)

    End Operator

#End Region

#Region " Arithmetic Operators "

    Public Shared Operator Mod(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType((CType(value1, Int32) Mod CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator Mod(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 Mod CType(value2, Int32))

    End Operator

    Public Shared Operator Mod(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) Mod value2)

    End Operator

    Public Shared Operator +(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType((CType(value1, Int32) + CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator +(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 + CType(value2, Int32))

    End Operator

    Public Shared Operator +(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) + value2)

    End Operator

    Public Shared Operator -(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType((CType(value1, Int32) - CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator -(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 - CType(value2, Int32))

    End Operator

    Public Shared Operator -(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) - value2)

    End Operator

    Public Shared Operator *(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType((CType(value1, Int32) * CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator *(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 * CType(value2, Int32))

    End Operator

    Public Shared Operator *(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) * value2)

    End Operator

    Public Shared Operator \(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType((CType(value1, Int32) \ CType(value2, Int32)), Int24)

    End Operator

    Public Shared Operator \(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 \ CType(value2, Int32))

    End Operator

    Public Shared Operator \(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) \ value2)

    End Operator

    Public Shared Operator /(ByVal value1 As Int24, ByVal value2 As Int24) As Double

        Return (CType(value1, Double) / CType(value2, Double))

    End Operator

    Public Shared Operator /(ByVal value1 As Int32, ByVal value2 As Int24) As Double

        Return (CType(value1, Double) / CType(value2, Double))

    End Operator

    Public Shared Operator /(ByVal value1 As Int24, ByVal value2 As Int32) As Double

        Return (CType(value1, Double) / CType(value2, Int32))

    End Operator

    Public Shared Operator ^(ByVal value1 As Int24, ByVal value2 As Int24) As Double

        Return (CType(value1, Double) ^ CType(value2, Double))

    End Operator

    Public Shared Operator ^(ByVal value1 As Int32, ByVal value2 As Int24) As Double

        Return (CType(value1, Double) ^ CType(value2, Double))

    End Operator

    Public Shared Operator ^(ByVal value1 As Int24, ByVal value2 As Int32) As Double

        Return (CType(value1, Double) ^ CType(value2, Double))

    End Operator

    Public Shared Operator >>(ByVal value As Int24, ByVal shifts As Integer) As Int24

        Return CType(ApplyBitMask(CType(value, Int32) >> shifts), Int24)

    End Operator

    Public Shared Operator <<(ByVal value As Int24, ByVal shifts As Int32) As Int24

        Return CType(ApplyBitMask(CType(value, Int32) << shifts), Int24)

    End Operator

#End Region

#End Region

#Region " Int24 Specific Functions "

    ''' <summary>Represents the largest possible value of an Int24. This field is constant.</summary>
    Public Shared ReadOnly Property MaxValue() As Int24
        Get
            Return m_maxValue
        End Get
    End Property

    ''' <summary>Represents the smallest possible value of an Int24. This field is constant.</summary>
    Public Shared ReadOnly Property MinValue() As Int24
        Get
            Return m_minValue
        End Get
    End Property

    Private Shared Sub ValidateNumericRange(ByVal value As Int32)

        If value > (Int24.MaxValue32 + 1) Or value < Int24.MinValue32 Then Throw New OverflowException(String.Format("Value of {0} will not fit in a 24-bit signed integer", value))

    End Sub

    Private Shared Function ApplyBitMask(ByVal value As Int32) As Int32

        If (value And Bit23) > 0 Then
            ' If the sign-bit is set, this number will be negative - set all high-byte bits (keeps 32-bit number in 24-bit range)
            value = (value Or BitMask)
        Else
            ' If the sign-bit is not set, this number will be positive - clear all high-byte bits (keeps 32-bit number in 24-bit range)
            value = (value And Not BitMask)
        End If

        Return value

    End Function

#End Region

#Region " Standard Numeric Operations "

    ''' <summary>
    ''' Compares this instance to a specified object and returns an indication of their relative values.
    ''' </summary>
    ''' <param name="value">An object to compare, or null.</param>
    ''' <returns>
    ''' A signed number indicating the relative values of this instance and value. Returns less than zero
    ''' if this instance is less than value, zero if this instance is equal to value, or greater than zero
    ''' if this instance is greater than value.
    ''' </returns>
    ''' <exception cref="ArgumentException">value is not an Int32 or Int24.</exception>
    Public Function CompareTo(ByVal value As Object) As Integer Implements IComparable.CompareTo

        If value Is Nothing Then Return 1
        If Not TypeOf value Is Int32 AndAlso Not TypeOf value Is Int24 Then Throw New ArgumentException("Argument must be an Int32 or an Int24")

        Dim num As Int32 = CType(value, Int32)

        If m_value < num Then Return -1
        If m_value > num Then Return 1

        Return 0

    End Function

    ''' <summary>
    ''' Compares this instance to a specified 32-bit signed integer and returns an indication of their
    ''' relative values.
    ''' </summary>
    ''' <param name="value">An integer to compare.</param>
    ''' <returns>
    ''' A signed number indicating the relative values of this instance and value. Returns less than zero
    ''' if this instance is less than value, zero if this instance is equal to value, or greater than zero
    ''' if this instance is greater than value.
    ''' </returns>
    Public Function CompareTo(ByVal value As Int24) As Integer Implements IComparable(Of Int24).CompareTo

        Return CompareTo(CType(value, Int32))

    End Function

    ''' <summary>
    ''' Compares this instance to a specified 32-bit signed integer and returns an indication of their
    ''' relative values.
    ''' </summary>
    ''' <param name="value">An integer to compare.</param>
    ''' <returns>
    ''' A signed number indicating the relative values of this instance and value. Returns less than zero
    ''' if this instance is less than value, zero if this instance is equal to value, or greater than zero
    ''' if this instance is greater than value.
    ''' </returns>
    Public Function CompareTo(ByVal value As Int32) As Integer Implements IComparable(Of Int32).CompareTo

        If m_value < value Then Return -1
        If m_value > value Then Return 1

        Return 0

    End Function

    ''' <summary>
    ''' Returns a value indicating whether this instance is equal to a specified object.
    ''' </summary>
    ''' <param name="obj">An object to compare, or null.</param>
    ''' <returns>
    ''' True if obj is an instance of Int32 or Int24 and equals the value of this instance;
    ''' otherwise, False.
    ''' </returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean

        If TypeOf obj Is Int32 Or TypeOf obj Is Int24 Then Return Equals(CType(obj, Int32))
        Return False

    End Function

    ''' <summary>
    ''' Returns a value indicating whether this instance is equal to a specified Int24 value.
    ''' </summary>
    ''' <param name="obj">An Int24 value to compare to this instance.</param>
    ''' <returns>
    ''' True if obj has the same value as this instance; otherwise, False.
    ''' </returns>
    Public Overloads Function Equals(ByVal obj As Int24) As Boolean Implements IEquatable(Of Int24).Equals

        Return Equals(CType(obj, Int32))

    End Function

    ''' <summary>
    ''' Returns a value indicating whether this instance is equal to a specified Int32 value.
    ''' </summary>
    ''' <param name="obj">An Int32 value to compare to this instance.</param>
    ''' <returns>
    ''' True if obj has the same value as this instance; otherwise, False.
    ''' </returns>
    Public Overloads Function Equals(ByVal obj As Int32) As Boolean Implements IEquatable(Of Int32).Equals

        Return (m_value = obj)

    End Function

    ''' <summary>
    ''' Returns the hash code for this instance.
    ''' </summary>
    ''' <returns>
    ''' A 32-bit signed integer hash code.
    ''' </returns>
    Public Overrides Function GetHashCode() As Integer

        Return m_value

    End Function

    ''' <summary>
    ''' Converts the numeric value of this instance to its equivalent string representation.
    ''' </summary>
    ''' <returns>
    ''' The string representation of the value of this instance, consisting of a minus sign if
    ''' the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.
    ''' </returns>
    Public Overrides Function ToString() As String

        Return m_value.ToString()

    End Function

    ''' <summary>
    ''' Converts the numeric value of this instance to its equivalent string representation, using
    ''' the specified format.
    ''' </summary>
    ''' <param name="format">A format string.</param>
    ''' <returns>
    ''' The string representation of the value of this instance as specified by format.
    ''' </returns>
    Public Overloads Function ToString(ByVal format As String) As String

        Return m_value.ToString(format)

    End Function

    ''' <summary>
    ''' Converts the numeric value of this instance to its equivalent string representation using the
    ''' specified culture-specific format information.
    ''' </summary>
    ''' <param name="provider">
    ''' An System.IFormatProvider that supplies culture-specific formatting information.
    ''' </param>
    ''' <returns>
    ''' The string representation of the value of this instance as specified by provider.
    ''' </returns>
    Public Overloads Function ToString(ByVal provider As IFormatProvider) As String Implements IConvertible.ToString

        Return m_value.ToString(provider)

    End Function

    ''' <summary>
    ''' Converts the numeric value of this instance to its equivalent string representation using the
    ''' specified format and culture-specific format information.
    ''' </summary>
    ''' <param name="format">A format specification.</param>
    ''' <param name="provider">
    ''' An System.IFormatProvider that supplies culture-specific formatting information.
    ''' </param>
    ''' <returns>
    ''' The string representation of the value of this instance as specified by format and provider.
    ''' </returns>
    Public Overloads Function ToString(ByVal format As String, ByVal provider As IFormatProvider) As String Implements IFormattable.ToString

        Return m_value.ToString(format, provider)

    End Function

    ''' <summary>
    ''' Converts the string representation of a number to its 24-bit signed integer equivalent.
    ''' </summary>
    ''' <param name="s">A string containing a number to convert.</param>
    ''' <returns>
    ''' A 24-bit signed integer equivalent to the number contained in s.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">s is null.</exception>
    ''' <exception cref="OverflowAction">
    ''' s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
    ''' </exception>
    ''' <exception cref="FormatException">s is not in the correct format.</exception>
    Public Shared Function Parse(ByVal s As String) As Int24

        Return CType(Int32.Parse(s), Int24)

    End Function

    ''' <summary>
    ''' Converts the string representation of a number in a specified style to its 24-bit signed integer equivalent.
    ''' </summary>
    ''' <param name="s">A string containing a number to convert.</param>
    ''' <param name="style">
    ''' A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
    ''' A typical value to specify is System.Globalization.NumberStyles.Integer.
    ''' </param>
    ''' <returns>
    ''' A 24-bit signed integer equivalent to the number contained in s.
    ''' </returns>
    ''' <exception cref="ArgumentException">
    ''' style is not a System.Globalization.NumberStyles value. -or- style is not a combination of 
    ''' System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
    ''' </exception>
    ''' <exception cref="ArgumentNullException">s is null.</exception>
    ''' <exception cref="OverflowAction">
    ''' s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
    ''' </exception>
    ''' <exception cref="FormatException">s is not in a format compliant with style.</exception>
    Public Shared Function Parse(ByVal s As String, ByVal style As NumberStyles) As Int24

        Return CType(Int32.Parse(s, style), Int24)

    End Function

    ''' <summary>
    ''' Converts the string representation of a number in a specified culture-specific format to its 24-bit
    ''' signed integer equivalent.
    ''' </summary>
    ''' <param name="s">A string containing a number to convert.</param>
    ''' <param name="provider">
    ''' An System.IFormatProvider that supplies culture-specific formatting information about s.
    ''' </param>
    ''' <returns>
    ''' A 24-bit signed integer equivalent to the number contained in s.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">s is null.</exception>
    ''' <exception cref="OverflowAction">
    ''' s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
    ''' </exception>
    ''' <exception cref="FormatException">s is not in the correct format.</exception>
    Public Shared Function Parse(ByVal s As String, ByVal provider As IFormatProvider) As Int24

        Return CType(Int32.Parse(s, provider), Int24)

    End Function

    ''' <summary>
    ''' Converts the string representation of a number in a specified style and culture-specific format to its 24-bit
    ''' signed integer equivalent.
    ''' </summary>
    ''' <param name="s">A string containing a number to convert.</param>
    ''' <param name="style">
    ''' A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
    ''' A typical value to specify is System.Globalization.NumberStyles.Integer.
    ''' </param>
    ''' <param name="provider">
    ''' An System.IFormatProvider that supplies culture-specific formatting information about s.
    ''' </param>
    ''' <returns>
    ''' A 24-bit signed integer equivalent to the number contained in s.
    ''' </returns>
    ''' <exception cref="ArgumentException">
    ''' style is not a System.Globalization.NumberStyles value. -or- style is not a combination of 
    ''' System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
    ''' </exception>
    ''' <exception cref="ArgumentNullException">s is null.</exception>
    ''' <exception cref="OverflowAction">
    ''' s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
    ''' </exception>
    ''' <exception cref="FormatException">s is not in a format compliant with style.</exception>
    Public Shared Function Parse(ByVal s As String, ByVal style As NumberStyles, ByVal provider As IFormatProvider) As Int24

        Return CType(Int32.Parse(s, style, provider), Int24)

    End Function

    ''' <summary>
    ''' Converts the string representation of a number to its 24-bit signed integer equivalent. A return value
    ''' indicates whether the conversion succeeded or failed.
    ''' </summary>
    ''' <param name="s">A string containing a number to convert.</param>
    ''' <param name="result">
    ''' When this method returns, contains the 24-bit signed integer value equivalent to the number contained in s,
    ''' if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
    ''' is not of the correct format, or represents a number less than Int24.MinValue or greater than Int24.MaxValue.
    ''' This parameter is passed uninitialized.
    ''' </param>
    ''' <returns>true if s was converted successfully; otherwise, false.</returns>
    Public Shared Function TryParse(ByVal s As String, ByRef result As Int24) As Boolean

        Dim parseResult As Int32
        Dim parseResponse As Boolean

        parseResponse = Int32.TryParse(s, parseResult)

        Try
            result = CType(parseResult, Int24)
        Catch
            result = CType(0, Int24)
            parseResponse = False
        End Try

        Return parseResponse

    End Function

    ''' <summary>
    ''' Converts the string representation of a number in a specified style and culture-specific format to its
    ''' 24-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.
    ''' </summary>
    ''' <param name="s">A string containing a number to convert.</param>
    ''' <param name="style">
    ''' A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
    ''' A typical value to specify is System.Globalization.NumberStyles.Integer.
    ''' </param>
    ''' <param name="result">
    ''' When this method returns, contains the 24-bit signed integer value equivalent to the number contained in s,
    ''' if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
    ''' is not in a format compliant with style, or represents a number less than Int24.MinValue or greater than
    ''' Int24.MaxValue. This parameter is passed uninitialized.
    ''' </param>
    ''' <param name="provider">
    ''' An System.IFormatProvider objectthat supplies culture-specific formatting information about s.
    ''' </param>
    ''' <returns>true if s was converted successfully; otherwise, false.</returns>
    ''' <exception cref="ArgumentException">
    ''' style is not a System.Globalization.NumberStyles value. -or- style is not a combination of 
    ''' System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
    ''' </exception>
    Public Shared Function TryParse(ByVal s As String, ByVal style As NumberStyles, ByVal provider As IFormatProvider, ByRef result As Int24) As Boolean

        Dim parseResult As Int32
        Dim parseResponse As Boolean

        parseResponse = Int32.TryParse(s, style, provider, parseResult)

        Try
            result = CType(parseResult, Int24)
        Catch
            result = CType(0, Int24)
            parseResponse = False
        End Try

        Return parseResponse

    End Function

    ''' <summary>
    ''' Returns the System.TypeCode for value type System.Int32 (there is no defined type code for an Int24).
    ''' </summary>
    ''' <returns>The enumerated constant, System.TypeCode.Int32.</returns>
    ''' <remarks>
    ''' There is no defined Int24 type code and since an Int24 will easily fit inside an Int32, the
    ''' Int32 type code is returned.
    ''' </remarks>
    Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode

        ' There is no Int24 type code, and an Int24 will fit inside an Int32 - so we return an Int32 type code
        Return TypeCode.Int32

    End Function

#Region " Private IConvertible Implementation "

    ' These are are private on the native integer implementations, so we just make them private as well...

    Private Function ToBoolean(ByVal provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean

        Return Convert.ToBoolean(m_value, provider)

    End Function

    Private Function ToChar(ByVal provider As IFormatProvider) As Char Implements IConvertible.ToChar

        Return Convert.ToChar(m_value, provider)

    End Function

    Private Function ToSByte(ByVal provider As IFormatProvider) As SByte Implements IConvertible.ToSByte

        Return Convert.ToSByte(m_value, provider)

    End Function

    Private Function ToByte(ByVal provider As IFormatProvider) As Byte Implements IConvertible.ToByte

        Return Convert.ToByte(m_value, provider)

    End Function

    Private Function ToInt16(ByVal provider As IFormatProvider) As Short Implements IConvertible.ToInt16

        Return Convert.ToInt16(m_value, provider)

    End Function

    Private Function ToUInt16(ByVal provider As IFormatProvider) As UInt16 Implements IConvertible.ToUInt16

        Return Convert.ToUInt16(m_value, provider)

    End Function

    Private Function ToInt32(ByVal provider As IFormatProvider) As Integer Implements IConvertible.ToInt32

        Return m_value

    End Function

    Private Function ToUInt32(ByVal provider As IFormatProvider) As UInt32 Implements IConvertible.ToUInt32

        Return Convert.ToUInt32(m_value, provider)

    End Function

    Private Function ToInt64(ByVal provider As IFormatProvider) As Long Implements IConvertible.ToInt64

        Return Convert.ToInt64(m_value, provider)

    End Function

    Private Function ToUInt64(ByVal provider As IFormatProvider) As UInt64 Implements IConvertible.ToUInt64

        Return Convert.ToUInt64(m_value, provider)

    End Function

    Private Function ToSingle(ByVal provider As IFormatProvider) As Single Implements IConvertible.ToSingle

        Return Convert.ToSingle(m_value, provider)

    End Function

    Private Function ToDouble(ByVal provider As IFormatProvider) As Double Implements IConvertible.ToDouble

        Return Convert.ToDouble(m_value, provider)

    End Function

    Private Function ToDecimal(ByVal provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal

        Return Convert.ToDecimal(m_value, provider)

    End Function

    Private Function ToDateTime(ByVal provider As IFormatProvider) As System.DateTime Implements IConvertible.ToDateTime

        Return Convert.ToDateTime(m_value, provider)

    End Function

    Private Function ToType(ByVal type As Type, ByVal provider As IFormatProvider) As Object Implements IConvertible.ToType

        Return Convert.ChangeType(m_value, type, provider)

    End Function

#End Region

#End Region

End Structure
