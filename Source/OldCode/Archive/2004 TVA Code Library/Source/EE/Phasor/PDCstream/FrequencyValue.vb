'***********************************************************************
'  FrequencyValue.vb - Frequency value
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

Imports TVA.Interop

Namespace EE.Phasor.PDCstream

    Public Class FrequencyValue

        Private m_frequencyDefinition As FrequencyDefinition
        Private m_frequency As Int16
        Private m_dfdt As Int16

        Public Const BinaryLength As Integer = 4

        Public Shared ReadOnly Property Empty(ByVal frequencyDefinition As FrequencyDefinition) As FrequencyValue
            Get
                Return New FrequencyValue(frequencyDefinition, 0, 0)
            End Get
        End Property

        Public Shared Function CreateFromScaledValues(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As Double, ByVal dfdt As Double) As FrequencyValue

            With frequencyDefinition
                Return CreateFromUnscaledValues(frequencyDefinition, (frequency - .Offset) * .Scale, (dfdt - .DfDtOffset) * .DfDtScale)
            End With

        End Function

        Public Shared Function CreateFromUnscaledValues(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As Int16, ByVal dfdt As Int16) As FrequencyValue

            Return New FrequencyValue(frequencyDefinition, frequency, dfdt)

        End Function

        Private Sub New(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As Int16, ByVal dfdt As Int16)

            m_frequencyDefinition = frequencyDefinition
            m_frequency = frequency
            m_dfdt = dfdt

        End Sub

        Public ReadOnly Property FrequencyDefinition() As FrequencyDefinition
            Get
                Return m_frequencyDefinition
            End Get
        End Property

        Public ReadOnly Property Frequency() As Int16
            Get
                Return m_frequency
            End Get
        End Property

        Public ReadOnly Property DfDt() As Int16
            Get
                Return m_dfdt
            End Get
        End Property

        Public Property ScaledFrequency() As Double
            Get
                With m_frequencyDefinition
                    Return m_frequency / .Scale + .Offset
                End With
            End Get
            Set(ByVal Value As Double)
                With m_frequencyDefinition
                    m_frequency = (Value - .Offset) * .Scale
                End With
            End Set
        End Property

        Public Property ScaledDfDt() As Double
            Get
                With m_frequencyDefinition
                    Return m_dfdt / .DfDtScale + .DfDtOffset
                End With
            End Get
            Set(ByVal Value As Double)
                With m_frequencyDefinition
                    m_dfdt = (Value - .DfDtOffset) * .DfDtScale
                End With
            End Set
        End Property

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return (m_frequency = 0 And m_dfdt = 0)
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                EndianOrder.SwapCopyBytes(m_frequency, buffer, 0)
                EndianOrder.SwapCopyBytes(m_dfdt, buffer, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace