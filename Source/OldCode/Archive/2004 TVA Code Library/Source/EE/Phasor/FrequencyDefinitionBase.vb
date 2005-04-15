'***********************************************************************
'  FrequencyDefinitionBase.vb - Frequency and df/dt value definition base class
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of a frequency and df/dt value.
    Public MustInherit Class FrequencyDefinitionBase

        Inherits ChannelDefinitionBase
        Implements IFrequencyDefinition

        Protected m_dfdtScale As Integer
        Protected m_dfdtOffset As Double
        Protected m_nominalFrequency As LineFrequency

        ' Create frequency definition from other frequency definition
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in frequencyDefinitionType
        ' Dervied class must expose a Public Sub New(ByVal frequencyDefinition As IFrequencyDefinition)
        Protected Shared Shadows Function CreateFrom(ByVal frequencyDefinitionType As Type, ByVal frequencyDefinition As IFrequencyDefinition) As IFrequencyDefinition

            Return CType(Activator.CreateInstance(frequencyDefinitionType, New Object() {frequencyDefinition}), IFrequencyDefinition)

        End Function

        Protected Sub New()

            MyBase.New()

            m_scale = 1000
            m_dfdtScale = 100
            m_dfdtOffset = 0.0
            m_nominalFrequency = LineFrequency._60Hz

        End Sub

        Protected Sub New(ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double, ByVal dfdtScale As Double, ByVal dfdtOffset As Double, ByVal nominalLineFrequency As LineFrequency)

            MyBase.New(index, label, scale, offset)

            m_dfdtOffset = dfdtOffset
            m_nominalFrequency = nominalLineFrequency

            Me.DfDtScalingFactor = dfdtScale

        End Sub

        Protected Sub New(ByVal frequencyDefinition As IFrequencyDefinition)

            Me.New(frequencyDefinition.Index, frequencyDefinition.Label, frequencyDefinition.ScalingFactor, frequencyDefinition.Offset, _
                frequencyDefinition.DfDtScalingFactor, frequencyDefinition.DfDtOffset, frequencyDefinition.NominalFrequency)

        End Sub

        Public Overridable Property DfDtOffset() As Double Implements IFrequencyDefinition.DfDtOffset
            Get
                Return m_dfdtOffset
            End Get
            Set(ByVal Value As Double)
                m_dfdtOffset = Value
            End Set
        End Property

        Public Overridable Property DfDtScalingFactor() As Integer Implements IFrequencyDefinition.DfDtScalingFactor
            Get
                Return m_dfdtScale
            End Get
            Set(ByVal Value As Integer)
                If Value > MaximumDfDtScalingFactor Then Throw New OverflowException("DfDt scaling factor value cannot exceed " & MaximumDfDtScalingFactor)
                m_dfdtScale = Value
            End Set
        End Property

        Public Overridable ReadOnly Property MaximumDfDtScalingFactor() As Integer Implements IFrequencyDefinition.MaximumDfDtScalingFactor
            Get
                ' Typical scaling/conversion factors should fit within 3 bytes (i.e., 24 bits) of space
                Return 2 ^ 24
            End Get
        End Property

        Public Overridable Property NominalFrequency() As LineFrequency Implements IFrequencyDefinition.NominalFrequency
            Get
                Return m_nominalFrequency
            End Get
            Set(ByVal Value As LineFrequency)
                m_nominalFrequency = Value
            End Set
        End Property

        Public Overridable ReadOnly Property NominalFrequencyOffset() As Double Implements IFrequencyDefinition.NominalFrequencyOffset
            Get
                If m_nominalFrequency = LineFrequency._60Hz Then
                    Return 60.0
                Else
                    Return 50.0
                End If
            End Get
        End Property

    End Class

End Namespace