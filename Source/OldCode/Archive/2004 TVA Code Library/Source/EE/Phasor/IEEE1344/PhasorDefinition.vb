'***********************************************************************
'  PhasorDefinition.vb - Phasor definition
'  Copyright © 2004 - TVA, all rights reserved
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

Imports System.Text
Imports TVA.Interop

Namespace EE.Phasor.IEEE1344

    Public Class PhasorDefinitions

        Inherits CollectionBase

        Friend Sub New()
        End Sub

        Public Sub Add(ByVal value As PhasorDefinition)

            List.Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As PhasorDefinition
            Get
                Return DirectCast(List.Item(index), PhasorDefinition)
            End Get
        End Property

    End Class

    Public Class PhasorDefinition

        Public Const BinaryLength As Integer = 4

        Private m_label As String
        Private m_type As PhasorType
        Private m_calFactor As Int32

        Public Sub New()

            m_label = ""

        End Sub

        Public Sub New(ByVal label As String, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

            m_label = label

            EndianOrder.SwapCopy(binaryImage, 0, buffer, 0, BinaryLength)

            ' Get phasor type
            m_type = buffer(0)

            ' Scoot the three calfactor bytes down so we can convert them to a 32-bit integer
            For x As Integer = 0 To 2
                buffer(x) = buffer(x + 1)
            Next

            buffer(3) = 0

            m_calFactor = BitConverter.ToInt32(buffer, 0)

        End Sub

        Public Property [Type]() As PhasorType
            Get
                Return m_type
            End Get
            Set(ByVal Value As PhasorType)
                m_type = Value
            End Set
        End Property

        Public Property CalFactor() As Double
            Get
                Return m_calFactor / 100000
            End Get
            Set(ByVal Value As Double)
                m_calFactor = Convert.ToInt32(Value * 100000)
                If m_calFactor > MaximumCalFactor Then Throw New OverflowException("CalFactor value cannot exceed " & MaximumCalFactor)
            End Set
        End Property

        Public ReadOnly Property MaximumCalFactor() As Int32
            Get
                ' CalFactor should fit within 3 bytes (i.e., 24 bits) of space
                Return 2 ^ 24
            End Get
        End Property

        Public Property Label() As String
            Get
                Return m_label
            End Get
            Set(ByVal Value As String)
                If Len(Value) > MaximumLabelLength Then
                    Throw New OverflowException("Label length cannot exceed " & MaximumLabelLength)
                Else
                    m_label = Value
                End If
            End Set
        End Property

        Public ReadOnly Property MaximumLabelLength()
            Get
                Return 16
            End Get
        End Property

        Public ReadOnly Property LabelImage() As Byte()
            Get
                Return Encoding.ASCII.GetBytes(m_label.PadRight(MaximumLabelLength))
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                Array.Copy(BitConverter.GetBytes(m_calFactor), 0, buffer, 0, 4)

                ' Scoot the three calfactor bytes up so we can include the phasor type byte
                For x As Integer = 3 To 1 Step -1
                    buffer(x) = buffer(x - 1)
                Next

                ' Include phasor type
                buffer(0) = m_type

                Return EndianOrder.ReverseBuffer(buffer)
            End Get
        End Property

    End Class

End Namespace
