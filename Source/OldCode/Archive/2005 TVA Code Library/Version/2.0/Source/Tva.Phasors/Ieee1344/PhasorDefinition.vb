'*******************************************************************************************************
'  PhasorDefinition.vb - Phasor definition
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Interop

Namespace Ieee1344

    Public Class PhasorDefinition

        Public Const BinaryLength As Int32 = 4
        Public Const MaximumLabelLength As Int32 = 16

        Private m_label As String
        Private m_type As PhasorType
        Private m_calFactor As Int32

        Public Sub New()

            m_label = ""

        End Sub

        Public Sub New(ByVal label As String, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

            ' Validate and store phasor label
            Me.Label = label

            ' Get phasor type from first byte
            m_type = binaryImage(startIndex)

            ' Last three bytes represent scaling factor
            EndianOrder.BigEndian.Copy(binaryImage, startIndex + 1, buffer, 0, BinaryLength - 1)

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

        Public Property CalFactor() As Single
            Get
                Return m_calFactor / 100000
            End Get
            Set(ByVal Value As Single)
                m_calFactor = Convert.ToInt32(Value * 100000)
                If m_calFactor > MaximumCalFactor Then Throw New OverflowException("CalFactor value cannot exceed " & MaximumCalFactor)
            End Set
        End Property

        Public ReadOnly Property MaximumCalFactor() As Int32
            Get
                ' Typical scaling/conversion factors should fit within 3 bytes (i.e., 24 bits) of space
                Return &H1FFFFFF
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
                    m_label = Trim(Replace(Value, Chr(20), " "))
                End If
            End Set
        End Property

        Public ReadOnly Property LabelImage() As Byte()
            Get
                Return Encoding.ASCII.GetBytes(m_label.PadRight(MaximumLabelLength))
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                ' Include phasor type
                buffer(0) = m_type

                ' Include calfactor
                EndianOrder.BigEndian.Copy(BitConverter.GetBytes(m_calFactor), 0, buffer, 1, 3)

                Return buffer
            End Get
        End Property

    End Class

End Namespace
