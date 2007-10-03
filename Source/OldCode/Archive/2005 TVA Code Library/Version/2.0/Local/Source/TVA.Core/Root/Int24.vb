Option Strict On

Imports System.Runtime.InteropServices
Imports System.Globalization
Imports Tva.Interop.Bit

<Serializable(), StructLayout(LayoutKind.Sequential), ComVisible(True)> _
Public Structure Int24

    Implements IComparable, IFormattable, IConvertible, IComparable(Of Int24), IComparable(Of Int32), IEquatable(Of Int24), IEquatable(Of Int32)

    Public Const BitMask As Int32 = (Bit24 Or Bit25 Or Bit26 Or Bit27 Or Bit28 Or Bit29 Or Bit30 Or Bit31)
    Public Const MaxValue As Int32 = 8388607
    Public Const MinValue As Int32 = -8388608

    ' We store the Int24 value in a 4-byte integer for convenience
    Friend m_value As Int32

    Public Sub New(ByVal value As Int24)

        m_value = CType(value, Int32)

    End Sub

    Public Sub New(ByVal value As Int32)

        ValidateBitSize(value)
        m_value = value

    End Sub

#Region " Operators "

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

    Public Shared Narrowing Operator CType(ByVal value As Int32) As Int24

        Return New Int24(value)

    End Operator

    Public Shared Widening Operator CType(ByVal value As Int24) As Int32

        Return value.ToInt32(Nothing)

    End Operator

    Public Shared Operator IsTrue(ByVal value As Int24) As Boolean

        Return (value <> 0)

    End Operator

    Public Shared Operator IsFalse(ByVal value As Int24) As Boolean

        Return (value = 0)

    End Operator

    Public Shared Operator Not(ByVal value As Int24) As Int24

        Return CType((Not CType(value, Int32)), Int24)

    End Operator

    Public Shared Operator And(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) And CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator And(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 And CType(value2, Int32))

    End Operator

    Public Shared Operator And(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) And value2)

    End Operator

    Public Shared Operator Or(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) Or CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator Or(ByVal value1 As Int32, ByVal value2 As Int24) As Int32

        Return (value1 Or CType(value2, Int32))

    End Operator

    Public Shared Operator Or(ByVal value1 As Int24, ByVal value2 As Int32) As Int32

        Return (CType(value1, Int32) Or value2)

    End Operator

    Public Shared Operator Xor(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) Xor CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator Mod(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) Mod CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator +(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) Mod CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator -(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) Mod CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator *(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) * CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator \(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(value1, Int32) \ CType(value2, Int32), Int24)

    End Operator

    Public Shared Operator /(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(CType(value1, Int32) / CType(value2, Int32), Int32), Int24)

    End Operator

    Public Shared Operator ^(ByVal value1 As Int24, ByVal value2 As Int24) As Int24

        Return CType(CType(CType(value1, Int32) ^ CType(value2, Int32), Int32), Int24)

    End Operator

    Public Shared Operator >>(ByVal value As Int24, ByVal shifts As Integer) As Int24

        Dim result As Int32 = CType(value, Int32)

        For x As Integer = 1 To shifts
            ' Perform a single right rotation
            result >>= 1

            ' Keep an eye on that 25th bit (24th if counting from 0) that will need to be rotated back to the front on the line
            If (result And Bit24) > 0 Then result = (result Or Bit0 And Not Bit24)
        Next

        Return CType(result, Int24)

    End Operator

    Public Shared Operator <<(ByVal value As Int24, ByVal shifts As Int32) As Int24

        Dim result As Int32 = CType(value, Int32)

        For x As Integer = 1 To shifts
            ' Perform a single left rotation
            result <<= 1

            ' Keep an eye on that 32nd bit (31st if counting from 0) that will need to be rotated back to the end on the line
            If (result And Bit31) > 0 Then result = (result Or Bit23 And Not Bit31)
        Next

        Return CType(result, Int24)

    End Operator

#End Region

    Public Function CompareTo(ByVal value As Object) As Integer Implements IComparable.CompareTo

        If value Is Nothing Then Return 1
        If Not TypeOf value Is Int32 AndAlso Not TypeOf value Is Int24 Then Throw New ArgumentException("Argument must be an Int32 or an Int24")

        Dim num As Int32 = CType(value, Int32)

        If m_value < num Then Return -1
        If m_value > num Then Return 1

        Return 0

    End Function

    Public Function CompareTo(ByVal value As Int24) As Integer Implements IComparable(Of Int24).CompareTo

        Return CompareTo(CType(value, Int32))

    End Function

    Public Function CompareTo(ByVal value As Int32) As Integer Implements IComparable(Of Int32).CompareTo

        If m_value < value Then Return -1
        If m_value > value Then Return 1

        Return 0

    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean

        If TypeOf obj Is Int32 Or TypeOf obj Is Int24 Then Return Equals(CType(obj, Int32))
        Return False

    End Function

    Public Overloads Function Equals(ByVal value As Int24) As Boolean Implements IEquatable(Of Int24).Equals

        Return Equals(CType(value, Int32))

    End Function

    Public Overloads Function Equals(ByVal value As Int32) As Boolean Implements IEquatable(Of Int32).Equals

        Return (m_value = value)

    End Function

    Public Overrides Function GetHashCode() As Integer

        Return m_value

    End Function

    Public Overrides Function ToString() As String

        Return m_value.ToString()

    End Function

    Public Overloads Function ToString(ByVal format As String) As String

        Return m_value.ToString(format)

    End Function

    Public Overloads Function ToString(ByVal provider As IFormatProvider) As String Implements IConvertible.ToString

        Return m_value.ToString(provider)

    End Function

    Public Overloads Function ToString(ByVal format As String, ByVal provider As IFormatProvider) As String Implements IFormattable.ToString

        Return m_value.ToString(format, provider)

    End Function

    Public Shared Function Parse(ByVal s As String) As Int24

        Return CType(Int32.Parse(s), Int24)

    End Function

    Public Shared Function Parse(ByVal s As String, ByVal style As NumberStyles) As Int24

        Return CType(Int32.Parse(s, style), Int24)

    End Function

    Public Shared Function Parse(ByVal s As String, ByVal provider As IFormatProvider) As Int24

        Return CType(Int32.Parse(s, provider), Int24)

    End Function

    Public Shared Function Parse(ByVal s As String, ByVal style As NumberStyles, ByVal provider As IFormatProvider) As Int24

        Return CType(Int32.Parse(s, style, provider), Int24)

    End Function

    Public Shared Function TryParse(ByVal s As String, ByRef result As Int24) As Boolean

        Dim parseResult As Int32
        Dim parseResponse As Boolean

        parseResponse = Int32.TryParse(s, parseResult)

        result = CType(parseResult, Int24)

    End Function

    Public Shared Function TryParse(ByVal s As String, ByVal style As NumberStyles, ByVal provider As IFormatProvider, ByRef result As Int24) As Boolean

        Dim parseResult As Int32
        Dim parseResponse As Boolean

        parseResponse = Int32.TryParse(s, style, provider, parseResult)

        result = CType(parseResult, Int24)

    End Function

    Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode

        ' There is no Int24 type code, and an Int24 will only fit inside an Int32 - so we return an Int32 type code
        Return TypeCode.Int32

    End Function

    Private Shared Sub ValidateBitSize(ByVal value As Int32)

        If value > Int24.MaxValue Or value < Int24.MinValue Then Throw New OverflowException(String.Format("Value of {0} will not fit in a 24-bit integer"))

    End Sub

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

    Public Function ToInt32() As Int32

        Return (m_value Or BitMask)

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

End Structure
