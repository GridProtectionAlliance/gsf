'***********************************************************************
'  PDCstream.FrequencyValue.vb - Frequency value
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

Namespace PDCstream

    Public Class FrequencyValue

        Private m_frequencyDefinition As FrequencyDefinition
        Private m_frequency As UInt16
        Private m_dfdt As UInt16

        Public Enum CompositeValue
            Frequency
            DfDt
        End Enum

        Public CompositeValues As New CompositeValues(2)

        Public Const BinaryLength As Integer = 4

        Public Shared ReadOnly Property Empty() As FrequencyValue
            Get
                Return New FrequencyValue(Nothing, Convert.ToUInt16(0), Convert.ToUInt16(0))
            End Get
        End Property

        Public Shared Function CreateFromScaledValues(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As Double, ByVal dfdt As Double) As FrequencyValue

            With frequencyDefinition
                Return CreateFromUnscaledValues(frequencyDefinition, Convert.ToUInt16((frequency - .Offset) * .Scale), Convert.ToUInt16((dfdt - .DfDtOffset) * .DfDtScale))
            End With

        End Function

        Public Shared Function CreateFromUnscaledValues(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As UInt16, ByVal dfdt As UInt16) As FrequencyValue

            Return New FrequencyValue(frequencyDefinition, frequency, dfdt)

        End Function

        Private Sub New(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As UInt16, ByVal dfdt As UInt16)

            m_frequencyDefinition = frequencyDefinition
            m_frequency = frequency
            m_dfdt = dfdt

        End Sub

        Public ReadOnly Property FrequencyDefinition() As FrequencyDefinition
            Get
                Return m_frequencyDefinition
            End Get
        End Property

        Public ReadOnly Property Frequency() As UInt16
            Get
                Return m_frequency
            End Get
        End Property

        Public ReadOnly Property DfDt() As UInt16
            Get
                Return m_dfdt
            End Get
        End Property

        ' In .NET, unsigned ints aren't typically usable directly in equations, so we provide these functions to provide usable values
        Public ReadOnly Property UnscaledFrequency() As Double
            Get
                Return Convert.ToDouble(m_frequency)
            End Get
        End Property

        Public ReadOnly Property UnscaledDfDt() As Double
            Get
                Return Convert.ToDouble(m_dfdt)
            End Get
        End Property

        Public ReadOnly Property ScaledFrequency() As Double
            Get
                With m_frequencyDefinition
                    Return UnscaledFrequency / .Scale + .Offset
                End With
            End Get
        End Property

        Public ReadOnly Property ScaledDfDt() As Double
            Get
                With m_frequencyDefinition
                    Return UnscaledDfDt / .DfDtScale + .DfDtOffset
                End With
            End Get
        End Property

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return (UnscaledFrequency = 0 And UnscaledDfDt = 0)
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                Array.Copy(BitConverter.GetBytes(m_frequency), 0, buffer, 0, 2)
                Array.Copy(BitConverter.GetBytes(m_dfdt), 0, buffer, 2, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace