'***********************************************************************
'  EndianOrder.vb - Endian Byte Order Interoperability Classes
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
'       Added GetReverseByte overloads, and ReverseTo<Type> functions
'       Changes reviewed by John Shugart
'
'***********************************************************************

Namespace Interop

    Public Class EndianOrder

        Private Sub New()

            ' This is a shared function class not meant for instantiation

        End Sub

        ' This function behaves just like Array.Copy but takes a little-endian source array and copies it in big-endian order,
        ' or if the source array is big-endian it will copy it in little-endian order
        Public Shared Sub SwapCopy(ByVal sourceArray As Array, ByVal sourceIndex As Integer, ByVal destinationArray As Array, ByVal destinationIndex As Integer, ByVal length As Integer)

            For x As Integer = sourceIndex To sourceIndex + length - 1
                destinationArray.SetValue(sourceArray.GetValue(x), destinationIndex + length - 1 - (x - sourceIndex))
            Next

        End Sub

        ' This function reverses a buffer - reversed buffer is also return value
        Public Shared Function ReverseBuffer(ByVal buffer As Byte()) As Byte()

            Array.Reverse(buffer)
            Return buffer

        End Function

        Public Shared Function ReverseToBoolean(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As Boolean

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 1)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 1)

            Return BitConverter.ToBoolean(buffer, 0)

        End Function

        Public Shared Function ReverseToChar(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As Char

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 2)

            Return BitConverter.ToChar(buffer, 0)

        End Function

        Public Shared Function ReverseToDouble(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As Double

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 8)

            Return BitConverter.ToDouble(buffer, 0)

        End Function

        Public Shared Function ReverseToInt16(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As Int16

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 2)

            Return BitConverter.ToInt16(buffer, 0)

        End Function

        Public Shared Function ReverseToInt32(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As Int32

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 4)

            Return BitConverter.ToInt32(buffer, 0)

        End Function

        Public Shared Function ReverseToInt64(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As Int64

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 8)

            Return BitConverter.ToInt64(buffer, 0)

        End Function

        Public Shared Function ReverseToSingle(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As Single

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 4)

            Return BitConverter.ToSingle(buffer, 0)

        End Function

        Public Shared Function ReverseToUInt16(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As UInt16

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 2)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 2)

            Return BitConverter.ToUInt16(buffer, 0)

        End Function

        Public Shared Function ReverseToUInt32(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As UInt32

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 4)

            Return BitConverter.ToUInt32(buffer, 0)

        End Function

        Public Shared Function ReverseToUInt64(ByVal reverseBytes As Byte(), ByVal startIndex As Integer) As UInt64

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 8)

            SwapCopy(reverseBytes, startIndex, buffer, 0, 8)

            Return BitConverter.ToUInt64(buffer, 0)

        End Function

        Public Shared Function GetReverseBytes(ByVal value As Boolean) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As Char) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As Double) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As Int16) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As Int32) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As Int64) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As Single) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As UInt16) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As UInt32) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Function GetReverseBytes(ByVal value As UInt64) As Byte()

            Return ReverseBuffer(BitConverter.GetBytes(value))

        End Function

        Public Shared Sub SwapCopyBytes(ByVal value As Boolean, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 1)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As Char, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As Double, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As Int16, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As Int32, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As Int64, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As Single, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As UInt16, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As UInt32, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4)

        End Sub

        Public Shared Sub SwapCopyBytes(ByVal value As UInt64, ByVal destinationArray As Array, ByVal destinationIndex As Integer)

            SwapCopy(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8)

        End Sub

    End Class

End Namespace