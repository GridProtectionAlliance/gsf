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

        Protected m_offset As Double
        Protected m_dfdtScale As Double
        Protected m_dfdtOffset As Double
        Protected m_nominalFrequency As LineFrequency

        Protected Sub New()

            MyBase.New()

            m_scale = 1000.0
            m_offset = 0.0
            m_dfdtScale = 100.0
            m_dfdtOffset = 0.0
            m_nominalFrequency = LineFrequency._60Hz

        End Sub

        Protected Sub New(ByVal index As Integer, ByVal label As String, ByVal scale As Double, ByVal offset As Double, ByVal dfdtScale As Double, ByVal dfdtOffset As Double, ByVal nominalLineFrequency As LineFrequency)

            MyBase.New(index, label, scale)

            m_offset = offset
            m_dfdtScale = dfdtScale
            m_dfdtOffset = dfdtOffset
            m_nominalFrequency = nominalLineFrequency

        End Sub

        Public Property Offset() As Double Implements IFrequencyDefinition.Offset
            Get
                Return m_offset
            End Get
            Set(ByVal Value As Double)
                m_offset = Value
            End Set
        End Property

        Public Property DfDtScalingFactor() As Double Implements IFrequencyDefinition.DfDtScalingFactor
            Get
                Return m_dfdtScale
            End Get
            Set(ByVal Value As Double)
                m_dfdtScale = Value
            End Set
        End Property

        Public Property DfDtOffset() As Double Implements IFrequencyDefinition.DfDtOffset
            Get
                Return m_dfdtOffset
            End Get
            Set(ByVal Value As Double)
                m_dfdtOffset = Value
            End Set
        End Property

        Public Property NominalFrequency() As LineFrequency Implements IFrequencyDefinition.NominalFrequency
            Get
                Return m_nominalFrequency
            End Get
            Set(ByVal Value As LineFrequency)
                m_nominalFrequency = Value
            End Set
        End Property

    End Class

End Namespace