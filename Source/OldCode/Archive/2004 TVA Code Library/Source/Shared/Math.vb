'***********************************************************************
'  Math.vb - Common math functions / classes
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
'
'***********************************************************************

' Global Math Functions
Public Class Math

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    Public Const Bit0 As Byte = &H1     ' 00000001 = 1
    Public Const Bit1 As Byte = &H2     ' 00000010 = 2
    Public Const Bit2 As Byte = &H4     ' 00000100 = 4
    Public Const Bit3 As Byte = &H8     ' 00001000 = 8
    Public Const Bit4 As Byte = &H10    ' 00010000 = 16
    Public Const Bit5 As Byte = &H20    ' 00100000 = 32
    Public Const Bit6 As Byte = &H40    ' 01000000 = 64
    Public Const Bit7 As Byte = &H80    ' 10000000 = 128

    Public Shared Function XorCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As Int16

        Dim sum As Int16

        ' Word length XOR check-sum
        For x As Integer = 0 To length - 1 Step 2
            sum = sum Xor BitConverter.ToInt16(data, startIndex + x)
        Next

        Return sum

    End Function

    ' This function will take a double value and properly convert it back into a "signed" 16-bit integer
    Public Shared Function ParseInt16(ByVal source As Double) As Int16

        Try
            Return BitConverter.ToInt16(BitConverter.GetBytes(Convert.ToUInt16(source)), 0)
        Catch
            Return 0
        End Try

    End Function

    ' You can use this class to temporaily cache composite values until they've all been
    ' received so that a compound value can be created
    Public Class CompositeValues

        Private Structure CompositeValue

            Public Value As Double
            Public Received As Boolean

        End Structure

        Private m_compositeValues As CompositeValue()
        Private m_allReceived As Boolean

        Public Sub New(ByVal count As Integer)

            m_compositeValues = Array.CreateInstance(GetType(CompositeValue), count)

        End Sub

        Default Public Property Value(ByVal index As Integer) As Double
            Get
                Return m_compositeValues(index).Value
            End Get
            Set(ByVal Value As Double)
                With m_compositeValues(index)
                    .Value = Value
                    .Received = True
                End With
            End Set
        End Property

        Public ReadOnly Property Received(ByVal index As Integer) As Boolean
            Get
                Return m_compositeValues(index).Received
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return m_compositeValues.Length
            End Get
        End Property

        Public ReadOnly Property AllReceived() As Boolean
            Get
                If m_allReceived Then
                    Return True
                Else
                    Dim allValuesReceived As Boolean = True

                    For x As Integer = 0 To m_compositeValues.Length - 1
                        If Not m_compositeValues(x).Received Then
                            allValuesReceived = False
                            Exit For
                        End If
                    Next

                    If allValuesReceived Then m_allReceived = True
                    Return allValuesReceived
                End If
            End Get
        End Property

    End Class

End Class
