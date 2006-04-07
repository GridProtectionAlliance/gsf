'*******************************************************************************************************
'  Tva.Interop.EndianOrder.vb - Endian byte order interoperability class
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'  01/14/2005 - James R Carroll
'       Added GetByte overloads, and To<Type> functions - changes reviewed by John Shugart
'  01/05/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Interop.EndianOrder)
'
'*******************************************************************************************************

Namespace Interop

    ''' <summary>Endian Byte Order Enumeration</summary>
    Public Enum Endianness
        BigEndian
        LittleEndian
    End Enum
    
    ''' <summary>Endian byte order interoperability class</summary>
    ''' <remarks>
    ''' Intel systems use little-endian byte order, other systems, such as Unix, use big-endian byte ordering.
    ''' Little-endian ordering means bits are ordered such that the bit whose in-memory representation is right-most is the most-significant-bit in a byte.
    ''' Big-endian ordering means bits are ordered such that the bit whose in-memory representation is left-most is the most-significant-bit in a byte.
    ''' </remarks>
    Public Class EndianOrder

        ' Create shared big-endian class
        Public Shared BigEndian As New EndianOrder(Endianness.BigEndian)

        ' Create shared little-endian class
        Public Shared LittleEndian As New EndianOrder(Endianness.LittleEndian)

        Private Delegate Sub CopyFunction(ByVal sourceArray As Array, ByVal sourceIndex As Integer, ByVal destinationArray As Array, ByVal destinationIndex As Integer, ByVal length As Integer)
        Private Delegate Function CoerceByteOrderFunction(ByVal buffer As Byte()) As Byte()

        Private m_targetEndianness As Endianness
        Private m_copy As CopyFunction
        Private m_coerceByteOrder As CoerceByteOrderFunction

        Private Sub New(ByVal targetEndianness As Endianness)

            m_targetEndianness = targetEndianness

            ' We perform this logic only once for speed in conversions - we can do this because neither
            ' the target nor the OS endian order will change during the lifecycle of this class...
            If targetEndianness = Endianness.BigEndian Then
                If BitConverter.IsLittleEndian Then
                    ' If OS is little endian and we want big endian, we swap the bytes
                    m_copy = AddressOf SwapCopy
                    m_coerceByteOrder = AddressOf ReverseBuffer
                Else
                    ' If OS is big endian and we want big endian, we just copy the bytes
                    m_copy = AddressOf Buffer.BlockCopy
                    m_coerceByteOrder = AddressOf PassThroughBuffer
                End If
            Else
                If BitConverter.IsLittleEndian Then
                    ' If OS is little endian and we want little endian, we just copy the bytes
                    m_copy = AddressOf Buffer.BlockCopy
                    m_coerceByteOrder = AddressOf PassThroughBuffer
                Else
                    ' If OS is big endian and we want little endian, we swap the bytes
                    m_copy = AddressOf SwapCopy
                    m_coerceByteOrder = AddressOf ReverseBuffer
                End If
            End If

        End Sub

        ' This function behaves just like Array.Copy but takes a little-endian source array and copies it in big-endian order,
        ' or if the source array is big-endian it will copy it in little-endian order
        Private Sub SwapCopy(ByVal sourceArray As Array, ByVal sourceIndex As Integer, ByVal destinationArray As Array, ByVal destinationIndex As Integer, ByVal length As Integer)

            Dim offset As Integer = destinationIndex + length - 1

            For x As Integer = sourceIndex To sourceIndex + length - 1
                destinationArray.SetValue(sourceArray.GetValue(x), offset - (x - sourceIndex))
            Next

        End Sub

        Private Function ReverseBuffer(ByVal buffer As Byte()) As Byte()

            Array.Reverse(buffer)
            Return buffer

        End Function

        Private Function PassThroughBuffer(ByVal buffer As Byte()) As Byte()

            Return buffer

        End Function

        Public ReadOnly Property TargetEndianness() As Endianness
            Get
                Return m_targetEndianness
            End Get
        End Property

        ' For non-standard length byte manipulations, we expose copy function that will copy OS-ordered source buffer into proper target endian-order
        Public Sub Copy(ByVal sourceArray As Array, ByVal sourceIndex As Integer, ByVal destinationArray As Array, ByVal destinationIndex As Integer, ByVal length As Integer)

            m_copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)

        End Sub

        Public Function CoerceByteOrder(ByVal buffer As Byte()) As Byte()

            Return m_coerceByteOrder(buffer)

        End Function

        Public Function ToBoolean(ByVal value As Byte(), ByVal startIndex As Integer) As Boolean

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 1)

            m_copy(value, startIndex, buffer, 0, 1)

            Return BitConverter.ToBoolean(buffer, 0)

        End Function

        Public Function ToChar(ByVal value As Byte(), ByVal startIndex As Integer) As Char

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            m_copy(value, startIndex, buffer, 0, 2)

            Return BitConverter.ToChar(buffer, 0)

        End Function

        Public Function ToDouble(ByVal value As Byte(), ByVal startIndex As Integer) As Double

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            m_copy(value, startIndex, buffer, 0, 8)

            Return BitConverter.ToDouble(buffer, 0)

        End Function

        Public Function ToInt16(ByVal value As Byte(), ByVal startIndex As Integer) As Int16

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            m_copy(value, startIndex, buffer, 0, 2)

            Return BitConverter.ToInt16(buffer, 0)

        End Function

        Public Function ToInt32(ByVal value As Byte(), ByVal startIndex As Integer) As Int32

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            m_copy(value, startIndex, buffer, 0, 4)

            Return BitConverter.ToInt32(buffer, 0)

        End Function

        Public Function ToInt64(ByVal value As Byte(), ByVal startIndex As Integer) As Int64

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            m_copy(value, startIndex, buffer, 0, 8)

            Return BitConverter.ToInt64(buffer, 0)

        End Function

        Public Function ToSingle(ByVal value As Byte(), ByVal startIndex As Integer) As Single

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            m_copy(value, startIndex, buffer, 0, 4)

            Return BitConverter.ToSingle(buffer, 0)

        End Function

        <CLSCompliant(False)> _
        Public Function ToUInt16(ByVal value As Byte(), ByVal startIndex As Integer) As UInt16

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            m_copy(value, startIndex, buffer, 0, 2)

            Return BitConverter.ToUInt16(buffer, 0)

        End Function

        <CLSCompliant(False)> _
        Public Function ToUInt32(ByVal value As Byte(), ByVal startIndex As Integer) As UInt32

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            m_copy(value, startIndex, buffer, 0, 4)

            Return BitConverter.ToUInt32(buffer, 0)

        End Function

        <CLSCompliant(False)> _
        Public Function ToUInt64(ByVal value As Byte(), ByVal startIndex As Integer) As UInt64

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            m_copy(value, startIndex, buffer, 0, 8)

            Return BitConverter.ToUInt64(buffer, 0)

        End Function

        Public Function GetBytes(ByVal value As Boolean) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Char) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Double) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Int16) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Int32) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Int64) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Single) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        <CLSCompliant(False)> _
        Public Function GetBytes(ByVal value As UInt16) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        <CLSCompliant(False)> _
        Public Function GetBytes(ByVal value As UInt32) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        <CLSCompliant(False)> _
        Public Function GetBytes(ByVal value As UInt64) As Byte()

            Return m_coerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Sub CopyBytes(ByVal value As Boolean, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 1)

        End Sub

        Public Sub CopyBytes(ByVal value As Char, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Sub CopyBytes(ByVal value As Double, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

        Public Sub CopyBytes(ByVal value As Int16, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Sub CopyBytes(ByVal value As Int32, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        Public Sub CopyBytes(ByVal value As Int64, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

        Public Sub CopyBytes(ByVal value As Single, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        <CLSCompliant(False)> _
        Public Sub CopyBytes(ByVal value As UInt16, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        <CLSCompliant(False)> _
        Public Sub CopyBytes(ByVal value As UInt32, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        <CLSCompliant(False)> _
        Public Sub CopyBytes(ByVal value As UInt64, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            m_copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

    End Class

End Namespace