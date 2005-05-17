'***********************************************************************
'  FrequencyValueBase.vb - Frequency and DfDt value base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Namespace EE.Phasor

    ' This class represents the protocol independent definition of a frequency and dfdt value.
    Public MustInherit Class FrequencyValueBase

        Inherits ChannelValueBase
        Implements IFrequencyValue

        Private m_frequencyDefinition As IFrequencyDefinition
        Private m_frequency As Double
        Private m_dfdt As Double

        ' Create frequency value from other frequency value
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in frequencyValueType
        ' Dervied class must expose a Public Sub New(ByVal frequencyValue As IFrequencyValue)
        Protected Shared Shadows Function CreateFrom(ByVal frequencyValueType As Type, ByVal frequencyValue As IFrequencyValue) As IFrequencyValue

            Return CType(Activator.CreateInstance(frequencyValueType, New Object() {frequencyValue}), IFrequencyValue)

        End Function

        Protected Sub New(ByVal dataFormat As DataFormat, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Double, ByVal dfdt As Double)

            MyBase.New(dataFormat)

            m_frequencyDefinition = frequencyDefinition
            m_frequency = frequency
            m_dfdt = dfdt

        End Sub

        Protected Sub New(ByVal dataFormat As DataFormat, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)

            Me.New(dataFormat, frequencyDefinition, _
                unscaledFrequency / frequencyDefinition.ScalingFactor + frequencyDefinition.NominalFrequencyOffset, _
                unscaledDfDt / frequencyDefinition.DfDtScalingFactor)

        End Sub

        Protected Sub New(ByVal dataFormat As DataFormat, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(dataFormat)

            m_frequencyDefinition = frequencyDefinition

            If dataFormat = EE.Phasor.DataFormat.FixedInteger Then
                UnscaledFrequency = EndianOrder.ReverseToInt16(binaryImage, startIndex)
                UnscaledDfDt = EndianOrder.ReverseToInt16(binaryImage, startIndex + 2)
            Else
                m_frequency = EndianOrder.ReverseToSingle(binaryImage, startIndex) + frequencyDefinition.NominalFrequencyOffset
                m_dfdt = EndianOrder.ReverseToSingle(binaryImage, startIndex + 4)
            End If

        End Sub

        Protected Sub New(ByVal frequencyValue As IFrequencyValue)

            Me.New(frequencyValue.DataFormat, frequencyValue.Definition, frequencyValue.Frequency, frequencyValue.DfDt)

        End Sub

        Public Overridable ReadOnly Property Definition() As IFrequencyDefinition Implements IFrequencyValue.Definition
            Get
                Return m_frequencyDefinition
            End Get
        End Property

        Public Overridable Property Frequency() As Double Implements IFrequencyValue.Frequency
            Get
                Return m_frequency
            End Get
            Set(ByVal Value As Double)
                m_frequency = Value
            End Set
        End Property

        Public Overridable Property DfDt() As Double Implements IFrequencyValue.DfDt
            Get
                Return m_dfdt
            End Get
            Set(ByVal Value As Double)
                m_dfdt = Value
            End Set
        End Property

        Public Overridable Property UnscaledFrequency() As Int16 Implements IFrequencyValue.UnscaledFrequency
            Get
                With m_frequencyDefinition
                    Return Convert.ToInt16((m_frequency - .NominalFrequencyOffset) * .ScalingFactor)
                End With
            End Get
            Set(ByVal Value As Int16)
                With m_frequencyDefinition
                    m_frequency = Value / .ScalingFactor + .NominalFrequencyOffset
                End With
            End Set
        End Property

        Public Overridable Property UnscaledDfDt() As Int16 Implements IFrequencyValue.UnscaledDfDt
            Get
                Return Convert.ToInt16(m_dfdt * m_frequencyDefinition.DfDtScalingFactor)
            End Get
            Set(ByVal Value As Int16)
                m_dfdt = Value / m_frequencyDefinition.DfDtScalingFactor
            End Set
        End Property

        Public Overrides ReadOnly Property Values() As Double()
            Get
                Return New Double() {m_frequency, m_dfdt}
            End Get
        End Property

        Public Overrides ReadOnly Property IsEmpty() As Boolean
            Get
                Return (m_frequency = 0 And m_dfdt = 0)
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Int16
            Get
                If DataFormat = EE.Phasor.DataFormat.FixedInteger Then
                    Return 4
                Else
                    Return 8
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                If DataFormat = EE.Phasor.DataFormat.FixedInteger Then
                    EndianOrder.SwapCopyBytes(UnscaledFrequency, buffer, 0)
                    EndianOrder.SwapCopyBytes(UnscaledDfDt, buffer, 2)
                Else
                    EndianOrder.SwapCopyBytes(Convert.ToSingle(m_frequency - m_frequencyDefinition.NominalFrequencyOffset), buffer, 0)
                    EndianOrder.SwapCopyBytes(Convert.ToSingle(m_dfdt), buffer, 4)
                End If

                Return buffer
            End Get
        End Property

    End Class

End Namespace