'***********************************************************************
'  EndianOrder.vb - Endian Byte Order Interoperability Class
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'  01/14/2005 - James R Carroll
'       Added GetByte overloads, and To<Type> functions
'       Changes reviewed by John Shugart
'
'***********************************************************************

Namespace Interop

    Public Enum Endianness
        BigEndian
        LittleEndian
    End Enum
    
    Public Class EndianOrder

        ' Create shared big-endian class
        Public Shared BigEndian As New EndianOrder(Endianness.BigEndian)

        ' Create shared little-endian class
        Public Shared LittleEndian As New EndianOrder(Endianness.LittleEndian)

        Private m_targetEndianness As Endianness

        Private Sub New(ByVal targetEndianness As Endianness)

            m_targetEndianness = targetEndianness

        End Sub

        ' This function behaves just like Array.Copy but takes a little-endian source array and copies it in big-endian order,
        ' or if the source array is big-endian it will copy it in little-endian order
        Private Sub SwapCopy(ByVal sourceArray As Array, ByVal sourceIndex As Integer, ByVal destinationArray As Array, ByVal destinationIndex As Integer, ByVal length As Integer)

            For x As Integer = sourceIndex To sourceIndex + length - 1
                destinationArray.SetValue(sourceArray.GetValue(x), destinationIndex + length - 1 - (x - sourceIndex))
            Next

        End Sub

        ' This function behaves just like Array.Copy but makes sure data is copied in desired endian order - regardless of native endian-order of OS
        Public Sub Copy(ByVal sourceArray As Array, ByVal sourceIndex As Integer, ByVal destinationArray As Array, ByVal destinationIndex As Integer, ByVal length As Integer)

            If m_targetEndianness = Endianness.BigEndian Then
                If BitConverter.IsLittleEndian Then
                    ' If OS is little endian and we want big endian, we swap the bytes
                    SwapCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
                Else
                    ' If OS is big endian and we want big endian, we just copy the bytes
                    Buffer.BlockCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
                End If
            Else
                If BitConverter.IsLittleEndian Then
                    ' If OS is little endian and we want little endian, we just copy the bytes
                    Buffer.BlockCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
                Else
                    ' If OS is big endian and we want little endian, we swap the bytes
                    SwapCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
                End If
            End If

        End Sub

        ' This function adjusts buffer into desired endian order
        Public Function CoerceByteOrder(ByVal buffer As Byte()) As Byte()

            If m_targetEndianness = Endianness.BigEndian Then
                ' If OS is little endian and we want big endian, we reverse the bytes
                If BitConverter.IsLittleEndian Then Array.Reverse(buffer)
            Else
                ' If OS is big endian and we want little endian, we reverse the bytes
                If Not BitConverter.IsLittleEndian Then Array.Reverse(buffer)
            End If

            Return buffer

        End Function

        Public ReadOnly Property TargetEndianness() As Endianness
            Get
                Return m_targetEndianness
            End Get
        End Property

        Public Function ToBoolean(ByVal value As Byte(), ByVal startIndex As Integer) As Boolean

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 1)

            Copy(value, startIndex, buffer, 0, 1)

            Return BitConverter.ToBoolean(buffer, 0)

        End Function

        Public Function ToChar(ByVal value As Byte(), ByVal startIndex As Integer) As Char

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            Copy(value, startIndex, buffer, 0, 2)

            Return BitConverter.ToChar(buffer, 0)

        End Function

        Public Function ToDouble(ByVal value As Byte(), ByVal startIndex As Integer) As Double

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            Copy(value, startIndex, buffer, 0, 8)

            Return BitConverter.ToDouble(buffer, 0)

        End Function

        Public Function ToInt16(ByVal value As Byte(), ByVal startIndex As Integer) As Int16

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            Copy(value, startIndex, buffer, 0, 2)

            Return BitConverter.ToInt16(buffer, 0)

        End Function

        Public Function ToInt32(ByVal value As Byte(), ByVal startIndex As Integer) As Int32

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            Copy(value, startIndex, buffer, 0, 4)

            Return BitConverter.ToInt32(buffer, 0)

        End Function

        Public Function ToInt64(ByVal value As Byte(), ByVal startIndex As Integer) As Int64

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            Copy(value, startIndex, buffer, 0, 8)

            Return BitConverter.ToInt64(buffer, 0)

        End Function

        Public Function ToSingle(ByVal value As Byte(), ByVal startIndex As Integer) As Single

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            Copy(value, startIndex, buffer, 0, 4)

            Return BitConverter.ToSingle(buffer, 0)

        End Function

        Public Function ToUInt16(ByVal value As Byte(), ByVal startIndex As Integer) As UInt16

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            Copy(value, startIndex, buffer, 0, 2)

            Return BitConverter.ToUInt16(buffer, 0)

        End Function

        Public Function ToUInt32(ByVal value As Byte(), ByVal startIndex As Integer) As UInt32

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            Copy(value, startIndex, buffer, 0, 4)

            Return BitConverter.ToUInt32(buffer, 0)

        End Function

        Public Function ToUInt64(ByVal value As Byte(), ByVal startIndex As Integer) As UInt64

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            Copy(value, startIndex, buffer, 0, 8)

            Return BitConverter.ToUInt64(buffer, 0)

        End Function

        Public Function GetBytes(ByVal value As Boolean) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Char) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Double) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Int16) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Int32) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Int64) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As Single) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As UInt16) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As UInt32) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Function GetBytes(ByVal value As UInt64) As Byte()

            Return CoerceByteOrder(BitConverter.GetBytes(value))

        End Function

        Public Sub CopyBytes(ByVal value As Boolean, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 1)

        End Sub

        Public Sub CopyBytes(ByVal value As Char, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Sub CopyBytes(ByVal value As Double, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

        Public Sub CopyBytes(ByVal value As Int16, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Sub CopyBytes(ByVal value As Int32, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        Public Sub CopyBytes(ByVal value As Int64, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

        Public Sub CopyBytes(ByVal value As Single, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        Public Sub CopyBytes(ByVal value As UInt16, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Sub CopyBytes(ByVal value As UInt32, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        Public Sub CopyBytes(ByVal value As UInt64, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            Copy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

    End Class

End Namespace